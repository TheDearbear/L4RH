using System.Numerics;

namespace Speed.Engine.Collision;

public class BoundingSphere : IBoundingBox
{
    public Vector3 Point { get; set; }
    public double Radius { get; set; }

    public bool IsBounding(Vector3 point)
        => Vector3.Distance(Point, point) <= Radius;
}
