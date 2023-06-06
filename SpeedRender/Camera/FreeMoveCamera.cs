using System.Numerics;
using Speed.Engine.Camera.Frustum;

namespace Speed.Engine.Camera;

public class FreeMoveCamera : IFrustumCamera
{
    public Vector3 Position { get; set; }

    public Vector3 Target { get; set; } = ICamera.DirectionForward;
    public Vector3 ReverseTarget => Position == Target ? Vector3.Zero : Vector3.Normalize(Position - Target);

    public Vector3 CameraUp => Vector3.Normalize(Vector3.Cross(CameraRight, Target));
    public Vector3 CameraRight => Vector3.Normalize(Vector3.Cross(Target, ICamera.DirectionUp));

    public Matrix4x4? ViewSnapshot { get; set; }

    public float FOV { get; set; } = 75;
    public float AspectRatio { get; set; } = 16f / 9;

    public float ZNear { get; set; } = 0.1f;
    public float ZFar { get; set; } = float.PositiveInfinity;

    public float Yaw { get; set; }
    public float Pitch { get; set; }

    public Matrix4x4 View => Matrix4x4.CreateLookAt(Position, Position + Target, CameraUp);

    #region Basic Movement

    public void Forward(float distance) => Position += distance * Target;
    public void Backward(float distance) => Forward(-distance);
    public void Right(float distance) => Position += Vector3.Normalize(Vector3.Cross(Target, CameraUp)) * distance;
    public void Left(float distance) => Right(-distance);

    #endregion

    #region Lerp Movement

    public void LerpForward(float distance, float amount) => Position += Vector3.Lerp(Vector3.Zero, distance * Target, amount);
    public void LerpBackward(float distance, float amount) => LerpForward(-distance, amount);
    public void LerpRight(float distance, float amount) => Position += Vector3.Lerp(Vector3.Zero, Vector3.Normalize(Vector3.Cross(Target, CameraUp)) * distance, amount);
    public void LerpLeft(float distance, float amount) => LerpRight(-distance, amount);

    #endregion
}
