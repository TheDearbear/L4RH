using System.Numerics;

namespace Speed.Engine.Camera.Frustum;

public struct FrustumPlane
{
    //public Vector3 Normal { get; set; } = Vector3.UnitY;
    //public float Distance { get; set; } = 0;

    //public FrustumPlane(Vector3 normal, Vector3 distance)
    //    => (Normal, Distance) = (Vector3.Normalize(normal), Vector3.Dot(Normal, distance));

    //public FrustumPlane(Vector3 p1, Vector3 p2, Vector3 p3)
    //{
    //    Normal = Vector3.Normalize(Vector3.Cross(p2 - p1, p3 - p1));
    //    Distance = -Vector3.Dot(p1, Normal);
    //}

    //public readonly float DistanceToPlane(Vector3 point)
    //    => Vector3.Dot(Normal, point) + Distance;

    public float A { get; set; }
    public float B { get; set; }
    public float C { get; set; }
    public float D { get; set; }
}
