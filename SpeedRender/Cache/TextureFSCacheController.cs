using Speed.Engine.Texture;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Speed.Engine.Cache;

// TODO: Add Critical sections
/// <summary>
/// File system texture cache controller
/// </summary>
public class TextureFSCacheController
{
    /// <summary>
    /// Folder for cache
    /// </summary>
    public string CacheFolder { get; set; }

    public const string CacheIndexFile = "CacheIndex.json";
    public const string OldCacheIndexFile = "CacheIndex.old.json";

    private Dictionary<Guid, string> _lookupTable = new();
    private readonly static JsonSerializerOptions _serializeOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    public TextureFSCacheController(string cacheLocation = "RenderCache")
    {
        string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        CacheFolder = Path.Combine(path, cacheLocation);

        LoadCacheIndex();
    }

    /// <summary>
    /// Loads cache or initializes new if it was missing
    /// </summary>
    /// <returns>Was cache existed before</returns>
    public bool LoadCacheIndex()
    {
        if (!Directory.Exists(CacheFolder))
            Directory.CreateDirectory(CacheFolder);

        string cacheIndexPath = Path.Combine(CacheFolder, CacheIndexFile);
        bool existed = File.Exists(cacheIndexPath);

        if (!existed)
        {
            File.WriteAllText(cacheIndexPath, "{}");
        }
        else
        {
            var jsonText = File.ReadAllText(cacheIndexPath);

            try
            {
                var value = JsonSerializer.Deserialize<Dictionary<Guid, string>>(jsonText);

                if (value is not null)
                    _lookupTable = value;
            }
            catch (JsonException ex)
            {
                string oldCacheIndexPath = Path.Combine(CacheFolder, OldCacheIndexFile);
                File.Move(cacheIndexPath, oldCacheIndexPath);
                File.WriteAllText(cacheIndexPath, "{}");

#if DEBUG
                System.Diagnostics.Debug.WriteLine("Error while deserializing cache file. New index file create and old was renamed:" + Environment.NewLine + "\t- " + ex.Message);
#endif
            }
        }

        return existed;
    }

    public void SaveCacheIndex()
    {
        if (!Directory.Exists(CacheFolder))
            Directory.CreateDirectory(CacheFolder);

        string cacheIndexPath = Path.Combine(CacheFolder, CacheIndexFile);

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(_lookupTable, _serializeOptions);

        File.WriteAllBytes(cacheIndexPath, jsonBytes);
    }

    public void SaveToCache(IObjectTexture texture)
    {
        Guid nameGuid = NameToGuid(texture.Name);

        if (_lookupTable.TryAdd(nameGuid, texture.Name))
            SaveCacheIndex();

        var cachedFilePath = Path.Combine(CacheFolder, $"{nameGuid}.bin");

        File.WriteAllBytes(cachedFilePath, texture.Data);
    }

    public byte[]? LoadFromCache(string name)
    {
        Guid nameGuid = NameToGuid(name);

        if (!_lookupTable.ContainsKey(nameGuid))
            return null;

        var cachedFilePath = Path.Combine(CacheFolder, $"{nameGuid}.bin");

        return File.ReadAllBytes(cachedFilePath);
    }

    // TODO: Better Guid creation (add metadata for hashing to minimize guid collision when switching between regions)
    private static Guid NameToGuid(string name)
        => new(MD5.HashData(Encoding.UTF8.GetBytes(name)));
}
