using L4RH.Model;
using L4RH.Model.Sceneries;
using L4RH.Readers;
using System.Buffers.Binary;
using System.Numerics;

using WaitTaskData = (System.Threading.Tasks.Task<byte[]> Task, System.Threading.CancellationTokenSource CancellationTokenSource);
using EventChunkData = (uint ChunkId, object Data);

namespace L4RH;

public class ChunkDeserializer
{
    public IList<byte[]> Data { get; private set; } = new List<byte[]>();
    public List<IChunkReader> ReadersSources { get; } = new();
    public SortedSet<long> NewFileMarkers { get; } = new();

    public event Action? SerializationEnded;
    public event EventHandler<List<EventChunkData>>? SerializationEndedChunks;

    private readonly IDictionary<uint, IChunkReader> _chunkReaders = new Dictionary<uint, IChunkReader>();
    private readonly List<WaitTaskData> _waitTasks = new();
    private readonly List<EventChunkData> _allChunks = new();

    public void AddData(byte[] data)
    {
        long lastMarker = NewFileMarkers.LastOrDefault();
        int lengthOfPreviousData = Data.Any() ? Data.Last().Length : 0;

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

                        byte[] decompressed = JDLZ.Decompress(span.ReadArray(compressedSize).ToArray());

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


                    EventChunkData result = new(id, obj);

                    if (SerializationEndedChunks is not null)
                        _allChunks.Add(result);
                }
            }
        };

        run += () =>
        {
            #region Post-Processing

            var visibles = _allChunks.FirstOrDefault(data => data.Data is IList<VisibleSection>).Data as IList<VisibleSection>;
            var sections = _allChunks.FirstOrDefault(data => data.Data is IList<TrackSection>).Data as IList<TrackSection>;

            var sceneries = _allChunks.Where(data => data.Data is Scenery)
                                    .Select(data => (Scenery)data.Data)
                                    .ToDictionary(scenery => scenery.Offset);

            if (sections?.Any() == true)
            {
                var sectionsToAdd = sceneries.Where(scenery => !sections.Any(section => section.Id == scenery.Value.VisibleSectionId && section.Visible is not null));

                foreach ((uint offset, Scenery scenery) in sectionsToAdd)
                {
                    var section = new TrackSection()
                    {
                        AssociatedChunkOffset = scenery.Offset,
                        Center = Vector2.Zero,
                        Id = scenery.VisibleSectionId,
                        Name = TrackSection.IdToName((ushort)scenery.VisibleSectionId),
                        Radius = 0,
                        Scenery = scenery,
                        Visible = visibles?.FirstOrDefault(v => v.Id == scenery.VisibleSectionId)
                    };

                    sections?.Add(section);
                    sceneries.Remove(offset);
                }

                foreach (var section in sections)
                {
                    if (section.Scenery is null && sceneries.TryGetValue(section.AssociatedChunkOffset, out Scenery? scenery))
                        section.Scenery = scenery;

                    if (section.Visible is null && visibles?.Any() == true)
                    {
                        var visible = visibles.FirstOrDefault(v => v.Id == section.Id);

                        if (visible is not null)
                            section.Visible = visible;
                    }
                }
            }

            #endregion

            SerializationEnded?.Invoke();
            SerializationEndedChunks?.Invoke(this, _allChunks);
            _allChunks.Clear();
        };

        return Task.Run(run);
    }
}
