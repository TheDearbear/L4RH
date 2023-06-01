using System.Collections;
using System.Diagnostics;

namespace L4RH.Model.Textures;

[DebuggerDisplay("{Name} ({PipelinePath}): {Count} Textures")]
public class TexturePack : IList<Texture>
{
    public Texture this[int index] { get => Elements[index]; set => Elements[index] = value; }
    public int Count => Elements.Count;
    public bool IsReadOnly => false;

    public TexturePackVersion PackVersion { get; set; }
    public int Version { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PipelinePath { get; set; } = string.Empty;

    private readonly List<Texture> Elements = new();

    public void Add(Texture item) => Elements.Add(item);
    public void Clear() => Elements.Clear();
    public bool Contains(Texture item) => Elements.Contains(item);
    public void CopyTo(Texture[] array, int arrayIndex) => Elements.CopyTo(array, arrayIndex);
    public int IndexOf(Texture item) => Elements.IndexOf(item);
    public void Insert(int index, Texture item) => Elements.Insert(index, item);
    public bool Remove(Texture item) => Elements.Remove(item);
    public void RemoveAt(int index) => Elements.RemoveAt(index);

    public IEnumerator<Texture> GetEnumerator() => Elements.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();
}
