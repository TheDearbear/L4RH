using System.Numerics;

namespace Speed.Engine.Collision;

public class BoundingAABB : IBoundingBox
{
    public Vector3 Point { get; set; }
    public Vector3 Size { get; set; }

    public bool IsBounding(Vector3 point)
    {
        Vector3 max = Point + Size;

        return point.X >= Point.X && point.Y >= Point.Y && point.Z >= Point.Z &&
            point.X <= max.X && point.Y <= max.Y && point.Z <= max.Z;
    }
}
