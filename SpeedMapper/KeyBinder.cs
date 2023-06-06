using System;
using System.Collections.Generic;

namespace SpeedMapper;

public class KeyBinder<T> where T : notnull
{
    public IDictionary<T, KeyBind<T>> KeyBinds { get; }

    public KeyBinder()
    {
        KeyBinds = new Dictionary<T, KeyBind<T>>();
    }

    /// <summary>
    /// Tries to add key binding to dictionary
    /// </summary>
    /// <param name="key">Key binding value</param>
    /// <param name="name">Display name of key binding</param>
    /// <returns>Was operation successful</returns>
    public bool AddKeyBind(T key, string name)
    {
        return KeyBinds.TryAdd(key, new KeyBind<T> { Binding = key, Name = name });
    }

    /// <summary>
    /// Tries to remove key binding from dictionary
    /// </summary>
    /// <param name="key">Key binding</param>
    /// <returns>Was operation successful</returns>
    public bool RemoveKeyBind(T key)
    {
        return KeyBinds.Remove(key);
    }

    public bool IsPressed(T key)
        => KeyBinds.TryGetValue(key, out var binding) && binding.Pressed;

    public void Update(IEnumerable<ValueTuple<T, bool>> states)
    {
        foreach (var state in states)
            if (KeyBinds.TryGetValue(state.Item1, out KeyBind<T> binding))
            {
                binding.Pressed = state.Item2;
                KeyBinds[state.Item1] = binding;
            }
    }
}
