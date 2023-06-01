using System.Numerics;

namespace Speed.Engine.Camera.Frustum;

public class FrustumBox
{
    public Vector4 LeftPlane { get; set; }
    public Vector4 RightPlane { get; set; }
    public Vector4 TopPlane { get; set; }
    public Vector4 BottomPlane { get; set; }
    public Vector4 NearPlane { get; set; }
    public Vector4 FarPlane { get; set; }
}
