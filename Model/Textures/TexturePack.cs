using System.Diagnostics;

namespace L4RH.Model.Textures;

[DebuggerDisplay("{Name} ({PipelinePath}): {IndexEntries.Length + StreamEntries.Length} Textures")]
public class TexturePack
{
    public TexturePackVersion Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PipelinePath { get; set; } = string.Empty;
    public uint PipelinePathHash { get; set; }

    public TextureIndexEntry[] IndexEntries { get; set; } = Array.Empty<TextureIndexEntry>();
    public TextureStreamEntry[] StreamEntries { get; set; } = Array.Empty<TextureStreamEntry>();
    public TextureInfo[] InfoEntries { get; set; } = Array.Empty<TextureInfo>();
}
