using System.Numerics;

namespace Speed.Engine.Collision;

public class BoundingPoint : IBoundingBox
{
    public Vector3 Point { get; set; }

    public bool IsBounding(Vector3 point)
        => Point == point;
}
