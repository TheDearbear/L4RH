using System.Numerics;

namespace Speed.Engine.Collision;

public interface IBoundingBox
{
    Vector3 Point { get; set; }

    bool IsBounding(Vector3 point);
}
