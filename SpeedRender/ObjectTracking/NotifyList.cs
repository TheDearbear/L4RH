using System.Collections;
using System.Collections.Specialized;

namespace Speed.Engine.ObjectTracking;

/// <summary>
/// Basic <see cref="IList{T}"/> realization with notification mechanism via <see cref="INotifyCollectionChanged"/>
/// </summary>
/// <typeparam name="T">Type of storing objects</typeparam>
public class NotifyList<T> : IList<T>, INotifyCollectionChanged
{
    /// <summary>
    /// Basic list for proxy
    /// </summary>
    private readonly List<T> _list;

    /// <summary>
    /// Returns object by its index in list
    /// </summary>
    /// <param name="index">Index of object in list</param>
    /// <returns>Requested object by index</returns>
    public T this[int index]
    {
        get => _list[index];
        set
        {
            if (_list[index]?.Equals(value) == true) return;

            T oldValue = _list[index];
            _list[index] = value;

            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, value, oldValue, index));
        }
    }

    /// <summary>
    /// Count of objects in list
    /// </summary>
    public int Count => _list.Count;

    /// <summary>
    /// Read-only state value
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Maximum number of objects can be stored in list without resizing
    /// </summary>
    public int Capacity
    {
        get => _list.Capacity;
        set => _list.Capacity = value;
    }

    /// <summary>
    /// Event for listening list changes
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// The most primitive constructor
    /// </summary>
    public NotifyList()
        => _list = new List<T>();
    
    /// <summary>
    /// Constructor with definition for starting capacity
    /// </summary>
    /// <param name="capacity">Start capacity of list</param>
    public NotifyList(int capacity)
        => _list = new List<T>(capacity);

    /// <summary>
    /// Constructor for copying <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <param name="collection">Enumerable for copy</param>
    public NotifyList(IEnumerable<T> collection)
        => _list = new List<T>(collection);

    /// <summary>
    /// Constructor for using class as wrapper for existing <see cref="List{T}"/>
    /// </summary>
    /// <param name="list">Wrapper destination</param>
    public NotifyList(List<T> list)
        => _list = list;

    public void Add(T item)
    {
        _list.Add(item);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item));
    }

    public void Clear()
    {
        _list.Clear();
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
    }

    public bool Contains(T item)
        => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex)
        => _list.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator()
        => _list.GetEnumerator();

    public int IndexOf(T item)
        => _list.IndexOf(item);

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Move, _list.Skip(index).ToList(), index));
    }

    public bool Remove(T item)
    {
        bool removed = _list.Remove(item);

        if (removed)
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item));

        return removed;
    }

    public void RemoveAt(int index)
    {
        if (Count <= index) return;

        T item = _list[index];
        _list.RemoveAt(index);

        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, index));
    }

    public void AddRange(IEnumerable<T> collection)
        => _list.AddRange(collection);

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}
