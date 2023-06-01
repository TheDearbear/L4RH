using System.Collections;

namespace L4RH.Model.Solids;

public class SolidObjectList : IList<SolidObject>
{
    public int Marker { get; set; }
    public string PipelinePath { get; set; } = string.Empty;
    public string ParentSectionName { get; set; } = string.Empty;

    public int Count => Elements.Count;
    public bool IsReadOnly => false;
    public SolidObject this[int index] { get => Elements[index]; set => Elements[index] = value; }

    private readonly List<SolidObject> Elements = new();

    public int IndexOf(SolidObject item) => Elements.IndexOf(item);
    public void Insert(int index, SolidObject item) => Elements.Insert(index, item);
    public void RemoveAt(int index) => Elements.RemoveAt(index);
    public void Add(SolidObject item) => Elements.Add(item);
    public void Clear() => Elements.Clear();
    public bool Contains(SolidObject item) => Elements.Contains(item);
    public void CopyTo(SolidObject[] array, int arrayIndex) => Elements.CopyTo(array, arrayIndex);
    public bool Remove(SolidObject item) => Elements.Remove(item);

    public IEnumerator<SolidObject> GetEnumerator() => Elements.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();
}
