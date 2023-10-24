using System;
using System.Diagnostics;
using System.Numerics;

namespace L4RH.Model;

[DebuggerDisplay("Visible Section {Id} ({CanSeeIds.Length} Neighbors)")]
public class VisibleSection
{
    public ushort Id { get; set; }
    public ushort[] CanSeeIds { get; set; } = Array.Empty<ushort>();
    public Vector2[] Polygon { get; set; } = Array.Empty<Vector2>();
    public Vector2 MinPoint { get; set; }
    public Vector2 MaxPoint { get; set; }
    public Vector2 Center { get; set; }

    public bool IsInPolygon(float x, float y)
    {
        bool inside = false;
        for (int i = 0, j = Polygon.Length - 1; i < Polygon.Length; j = i++)
        {
            if ((Polygon[i].Y > y) != (Polygon[j].Y > y) && x < (Polygon[j].X - Polygon[i].X) * (y - Polygon[i].Y) / (Polygon[j].Y - Polygon[i].Y) + Polygon[i].X)
            {
                inside = !inside;
            }
        }

        return inside;
    }
}
