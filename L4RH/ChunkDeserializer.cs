using L4RH.Readers;
using System.Buffers.Binary;
using System.Reflection;

using WaitTaskData = (System.Threading.Tasks.Task<byte[]> Task, System.Threading.CancellationTokenSource CancellationTokenSource);
using EventChunkData = (uint ChunkId, object Data);

namespace L4RH;

public class ChunkDeserializer
{
    public IEnumerable<byte> Data { get; private set; } = Enumerable.Empty<byte>();
    public List<Assembly> ReadersSources { get; } = new();
    public SortedSet<long> NewFileMarkers { get; } = new();

    public event EventHandler<EventChunkData>? FoundChunk;
    public event Action? SerializationEnded;
    public event EventHandler<List<EventChunkData>>? SerializationEndedChunks;

    private readonly IDictionary<uint, IChunkReader> _chunkReaders = new Dictionary<uint, IChunkReader>();
    private readonly List<WaitTaskData> _waitTasks = new();
    private readonly List<EventChunkData> _allChunks = new();

    public void AddData(byte[] data)
    {
        NewFileMarkers.Add(Data.Count());
        Data = Data.Concat(data);
    }

    public void AddDataFromFile(string path)
    {
        var tokenSource = new CancellationTokenSource();
        _waitTasks.Add(new(File.ReadAllBytesAsync(path, tokenSource.Token), tokenSource));
    }

    public void ClearData()
    {
        Data = Enumerable.Empty<byte>();
        NewFileMarkers.Clear();

        foreach (var tuple in _waitTasks)
            tuple.Item2.Cancel();

        _waitTasks.Clear();
    }

    public Task Start()
    {
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

                #region Fill ChunkReaders

                foreach (Assembly asm in ReadersSources)
                {
                    try
                    {
                        IEnumerable<Type> types = asm.GetExportedTypes().Where(t => t.GetInterface(nameof(IChunkReader)) is not null);
                        foreach (Type type in types)
                        {
                            if (Activator.CreateInstance(type) is not IChunkReader chunkReader) return;
                            if (_chunkReaders.ContainsKey(chunkReader.ChunkId)) return;

                            _chunkReaders.Add(chunkReader.ChunkId, chunkReader);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException("Unable to get/create derived class from interface " + nameof(IChunkReader), e);
                    }
                }

                #endregion

                var span = new BinarySpan(Data.ToArray().AsSpan());

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

                    // JDLZ Compressed Chunk
                    if (id == 0x5A4C444A && length == 0x00001002)
                    {
                        var compressedSize = BinaryPrimitives.ReadInt32LittleEndian(span.Span.Slice(span.Pointer + 12, 4));

                        byte[] decompressed = JDLZ.Decompress(span.ReadArray(compressedSize).ToArray());

                        var decompressedReader = new ChunkDeserializer();
                        decompressedReader.AddData(decompressed);
                        decompressedReader.ReadersSources.AddRange(ReadersSources);

                        decompressedReader.FoundChunk += FoundChunk;
                        decompressedReader.SerializationEndedChunks += (sender, list) => _allChunks.AddRange(list);

                        decompressedReader.Start().Wait();
                        continue;
                    }

                    var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

                    if (!_chunkReaders.TryGetValue(id, out IChunkReader? chunkReader) || chunkReader is null)
                        continue;

                    int pointer = span.Pointer;

                    long marker = NewFileMarkers.Max(marker => marker < pointer ? marker : 0);

                    object obj = chunkReader.DeserializeObject(chunkBuffer, chunkStart - marker);


                    EventChunkData result = new(id, obj);

                    FoundChunk?.Invoke(this, result);
                    if (SerializationEndedChunks is not null)
                        _allChunks.Add(result);
                }
            }
        };

        run += () =>
        {
            _chunkReaders.Clear();
            SerializationEnded?.Invoke();
            SerializationEndedChunks?.Invoke(this, _allChunks);
            _allChunks.Clear();
        };

        return Task.Run(run);
    }
}
