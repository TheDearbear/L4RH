using System.Numerics;

namespace Speed.Engine.Camera;

public interface ICamera
{
    public Vector3 Position { get; set; }

    public Vector3 CameraUp { get; }
    public Vector3 CameraRight { get; }

    public float FOV { get; set; }
    public float AspectRatio { get; set; }

    public float ZNear { get; set; }
    public float ZFar { get; set; }

    public Matrix4x4 Perspective => Matrix4x4.CreatePerspectiveFieldOfView((float)(FOV * Math.PI / 180), AspectRatio, ZNear, ZFar);
    public Matrix4x4 View { get; }

    public static Vector3 DirectionForward => DirectionBackward * -1;
    public static Vector3 DirectionBackward => Vector3.UnitZ;
    public static Vector3 DirectionLeft => Vector3.Normalize(Vector3.Cross(DirectionBackward, DirectionUp));
    public static Vector3 DirectionRight => Vector3.Normalize(Vector3.Cross(DirectionForward, DirectionUp));
    public static Vector3 DirectionUp => Vector3.UnitY;
    public static Vector3 DirectionDown => DirectionUp * -1;
}
