using System.Diagnostics;
using System.Numerics;
using L4RH.Model.Sceneries;
using L4RH.Model.Solids;

namespace L4RH.Model;

/// <summary>
/// Represents section that can be streamed by game
/// </summary>
[DebuggerDisplay("Section {Name} ({Id})")]
public class TrackSection
{
    /// <summary>
    /// Instance id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Priority of section
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Center position. Can be used in pair with <seealso cref="Radius"/> for creating bounding sphere
    /// </summary>
    public Vector2 Center { get; set; }

    /// <summary>
    /// Radius of section. Can be used in pair with <seealso cref="Center"/> for creating bounding sphere
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Position of associated chunk inside STREAML*R*.BUN file
    /// </summary>
    public uint AssociatedChunkOffset { get; set; }

    public bool Usable { get; set; }

    /// <summary>
    /// Associated SolidList
    /// </summary>
    public SolidObjectList? Solids { get; set; }

    /// <summary>
    /// Associated Scenery
    /// </summary>
    public Scenery? Scenery { get; set; }

    /// <summary>
    /// Associated Visible data
    /// </summary>
    public VisibleSection? Visible { get; set; }

    public static ushort NameToId(string name)
    {
        if (name.Length < 2 || name.Length > 3)
            throw new ArgumentException("Invalid Section Name!", nameof(name));

        char letter = name[0];

        if (letter < 'A' || letter > 'Z')
            throw new ArgumentException("Invalid Section Name Format!", nameof(name));

        letter -= '@'; // letter -= 0x40 (0x40 == '@' && 0x41 == 'A')

        if (!byte.TryParse(name[1..], out byte value))
            throw new ArgumentException("Invalid Section Name Format!", nameof(name));

        return (ushort)(100 * letter + value);
    }

    public static string IdToName(ushort id)
    {
        if (id > 2699 || id < 100)
            throw new ArgumentException("Invalid Section Id!", nameof(id));

        char letter = (char)(id / 100 + 0x40);
        int number = id % 100;

        return $"{letter}{number}";
    }
}
