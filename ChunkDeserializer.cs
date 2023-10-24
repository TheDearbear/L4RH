using L4RH.Compression;
using L4RH.Readers;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace L4RH;

public class ChunkDeserializer
{
    class WaitTaskData
    {
        public Task<byte[]> Task { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        
        public WaitTaskData(Task<byte[]> task, CancellationTokenSource cancellationTokenSource)
        {
            Task = task;
            CancellationTokenSource = cancellationTokenSource;
        }
    }

    public IList<byte[]> Data { get; private set; } = new List<byte[]>();
    public List<IChunkReader> ReadersSources { get; } = new();
    public SortedSet<long> NewFileMarkers { get; } = new();

    public event Action? SerializationEnded;
    public event EventHandler<IEnumerable<ChunkResult>>? SerializationEndedChunks;

    private readonly IDictionary<uint, IChunkReader> _chunkReaders = new Dictionary<uint, IChunkReader>();
    private readonly List<WaitTaskData> _waitTasks = new();
    private readonly List<ChunkResult> _allChunks = new();

    public void AddData(byte[] data)
    {
        long lastMarker = NewFileMarkers.Max;
        int lengthOfPreviousData = Data.LastOrDefault()?.Length ?? 0;

        NewFileMarkers.Add(lastMarker + lengthOfPreviousData);
        Data.Add(data);
    }

    public void AddDataFromFile(string path)
    {
        var tokenSource = new CancellationTokenSource();
        _waitTasks.Add(new(File.ReadAllBytesAsync(path, tokenSource.Token), tokenSource));
    }

    public void ClearData()
    {
        Data.Clear();
        NewFileMarkers.Clear();

        foreach (var tuple in _waitTasks)
            tuple.CancellationTokenSource.Cancel();

        _waitTasks.Clear();
    }

    public Task Start()
    {
        _chunkReaders.Clear();

        foreach (IChunkReader reader in ReadersSources)
            _chunkReaders.TryAdd(reader.ChunkId, reader);
     
        Action run = () =>
        {
            lock (Data)
            {
                #region Wait and process tasks

                Task.WaitAll(_waitTasks.Select(data => data.Task).ToArray());

                foreach (var data in _waitTasks)
                {
                    if (!data.Task.IsCompleted) continue;

                    AddData(data.Task.Result);
                }

                _waitTasks.Clear();

                #endregion

                #region Create BinarySpan from Data

                Span<byte> spanData = Span<byte>.Empty;

                if (Data.Any())
                {
                    if (Data.Count > 1)
                        spanData = Data.SelectMany(array => array).ToArray().AsSpan();
                    else
                        spanData = Data.First().AsSpan();
                }

                var span = new BinarySpan(spanData);

                #endregion

                while (span.Pointer < span.Length)
                {
                    var chunkStart = span.Pointer;

                    var id = span.ReadUInt32();
                    var length = span.ReadInt32();

                    if (id == 0)
                    {
                        span.Pointer += length;
                        continue;
                    }

                    span.Pointer = chunkStart;

                    #region Process JDLZ Chunk

                    if (id == 0x5A4C444A && length == 0x00001002)
                    {
                        var compressedSize = BinaryPrimitives.ReadInt32LittleEndian(span.Span.Slice(span.Pointer + 12, 4));

                        byte[] decompressed = JLZ.Decompress(span.ReadArray(compressedSize).ToArray());

                        var decompressedReader = new ChunkDeserializer();
                        decompressedReader.AddData(decompressed);
                        decompressedReader.ReadersSources.AddRange(ReadersSources);

                        decompressedReader.SerializationEndedChunks += (sender, list) => _allChunks.AddRange(list);

                        decompressedReader.Start().Wait();
                        continue;
                    }

                    #endregion

                    var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

                    if (!_chunkReaders.TryGetValue(id, out IChunkReader? chunkReader) || chunkReader is null)
                        continue;

                    int pointer = span.Pointer;

                    long marker = NewFileMarkers.Max(marker => marker < pointer ? marker : 0);

                    object obj = chunkReader.DeserializeObject(chunkBuffer, chunkStart - marker);

                    if (SerializationEndedChunks is not null)
                        _allChunks.Add(new ChunkResult(id, obj));
                }
            }
        };

        run += () =>
        {
            SerializationEnded?.Invoke();
            SerializationEndedChunks?.Invoke(this, _allChunks);
            _allChunks.Clear();
        };

        return Task.Run(run);
    }

    public struct ChunkResult
    {
        public uint ChunkId;
        public object? Result;

        public ChunkResult(uint chunkId, object? result)
        {
            ChunkId = chunkId;
            Result = result;
        }
    }
}
