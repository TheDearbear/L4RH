using L4RH.Readers;
using System.Reflection;

using EventChunkData = (uint ChunkId, object Data);

namespace L4RH;

public class ChunkDeserializer
{
    public IEnumerable<byte> Data { get; private set; }
    public List<Assembly> ReadersSources { get; }
    public SortedSet<long> NewFileMarkers { get; }

    public event EventHandler<EventChunkData>? FoundChunk;
    public event Action? SerializationEnded;
    public event EventHandler<List<EventChunkData>>? SerializationEndedChunks;

    private readonly IDictionary<uint, IChunkReader> ChunkReaders;
    private readonly List<Task<byte[]>> WaitableTasks;
    private readonly List<EventChunkData> AllChunks;

    public ChunkDeserializer()
    {
        Data = Enumerable.Empty<byte>();
        ReadersSources = new List<Assembly>();
        NewFileMarkers = new SortedSet<long>();
        ChunkReaders = new Dictionary<uint, IChunkReader>();
        WaitableTasks = new();
        AllChunks = new();
    }

    public void AddData(byte[] data)
    {
        NewFileMarkers.Add(Data.Count());
        Data = Data.Concat(data);
    }

    public void AddDataFromFile(string path)
    {
        WaitableTasks.Add(File.ReadAllBytesAsync(path));
    }

    public void ClearData()
    {
        Data = Enumerable.Empty<byte>();
        NewFileMarkers.Clear();
    }

    public Thread Start()
    {
        ThreadStart run = () =>
        {
            lock (Data)
            {
                #region Wait and process tasks

                Task.WaitAll(WaitableTasks.ToArray());

                foreach (Task<byte[]> task in WaitableTasks)
                {
                    if (!task.IsCompleted) continue;

                    AddData(task.Result);
                }

                WaitableTasks.Clear();

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
                            if (ChunkReaders.ContainsKey(chunkReader.ChunkId)) return;

                            ChunkReaders.Add(chunkReader.ChunkId, chunkReader);
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

                    var chunkBuffer = new BinarySpan(span.ReadArray(length + 8));

                    if (!ChunkReaders.TryGetValue(id, out IChunkReader? chunkReader) || chunkReader is null)
                        continue;

                    int pointer = span.Pointer;

                    long marker = NewFileMarkers.Max(marker => marker < pointer ? marker : 0);

                    object obj = chunkReader.DeserializeObject(chunkBuffer, chunkStart - marker);


                    EventChunkData result = new(id, obj);

                    FoundChunk?.Invoke(this, result);
                    if (SerializationEndedChunks is not null)
                        AllChunks.Add(result);
                }
            }
        };

        run += () =>
        {
            ChunkReaders.Clear();
            SerializationEnded?.Invoke();
            SerializationEndedChunks?.Invoke(this, AllChunks);
            AllChunks.Clear();
        };

        Thread thread = new(run) { IsBackground = true };
        thread.Start();

        return thread;
    }
}
