using System;
using System.Collections.Generic;

namespace SpeedMapper;

/// <summary>
/// Class for managing user input
/// </summary>
/// <typeparam name="T">Type of user input key</typeparam>
internal class KeyBinder<T> where T : notnull
{
    /// <summary>
    /// Dictionary with all current keys, their name and state
    /// </summary>
    public IDictionary<T, KeyBind> KeyBinds { get; } = new Dictionary<T, KeyBind>();

    /// <summary>
    /// Tries to add key binding to dictionary
    /// </summary>
    /// <param name="key">Key binding value</param>
    /// <param name="name">Display name of key binding</param>
    /// <returns>Was operation successful</returns>
    public bool AddKeyBind(T key, string name)
    {
        return KeyBinds.TryAdd(key, new KeyBind { Binding = key, Name = name });
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

    /// <summary>
    /// Checks if key is currently pressed by user
    /// </summary>
    /// <param name="key">Key for check</param>
    /// <returns>Is key pressed</returns>
    public bool IsPressed(T key)
        => KeyBinds.TryGetValue(key, out var binding) && binding.Pressed;

    /// <summary>
    /// Updates states from <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <param name="states">Available states for update</param>
    public void Update(IEnumerable<ValueTuple<T, bool>> states)
    {
        foreach (var state in states)
            if (KeyBinds.TryGetValue(state.Item1, out KeyBind binding) && binding.Pressed != state.Item2)
            {
                binding.Pressed = state.Item2;
                KeyBinds[state.Item1] = binding;
            }
    }

    /// <summary>
    /// Little structure for holding data about key
    /// </summary>
    internal struct KeyBind
    {
        /// <summary>
        /// Key-specific value
        /// </summary>
        public T Binding;

        /// <summary>
        /// Display name of binding
        /// </summary>
        public string Name;

        /// <summary>
        /// Is key currently pressed
        /// </summary>
        public bool Pressed;
    }
}
