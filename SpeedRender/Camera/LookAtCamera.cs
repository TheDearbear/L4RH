using System.Numerics;

namespace Speed.Engine.Camera;

public class LookAtCamera : ICamera
{
    public Vector3 Position { get; set; } = new(0, 0, 1);

    public Vector3 Target { get; set; } = Vector3.Zero;
    public Vector3 ReverseTarget => Position == Target ? Vector3.Zero : Vector3.Normalize(Position - Target);

    public Vector3 CameraUp => Vector3.Cross(ReverseTarget, CameraRight);
    public Vector3 CameraRight => Vector3.Normalize(Vector3.Cross(ICamera.DirectionUp, ReverseTarget));

    public float FOV { get; set; } = 45f;
    public float AspectRatio { get; set; } = 16f / 9;

    public float ZNear { get; set; } = 0.01f;
    public float ZFar { get; set; } = 500;

    public Matrix4x4 View => Matrix4x4.CreateLookAt(Position, Target, CameraUp);
}
