using L4RH.Model.Sceneries;
using Speed.Engine.DebugUtils;
using Speed.Engine.SceneryObjects;
using System.Diagnostics;
using System.Numerics;
using Veldrid;

namespace Speed.Engine.Camera.Frustum;

public interface IFrustumCamera : ICamera
{
    public Vector3 Target { get; set; }
    public Vector3 ReverseTarget => Position == Target ? Vector3.Zero : Vector3.Normalize(Position - Target);

    public Matrix4x4? ViewSnapshot { get; set; }

    //public FrustumBox Frustum => new()
    //{
    //    NearPlane = new(Target, Position + ZNear * Target),
    //    FarPlane = new(Target * -1, Position + ZFar * Target),
    //    TopPlane = new(Vector3.Cross(CameraRight, ZFar * Target - CameraUp * (ZFar * (float)Math.Tan(FOV * .5))), Position),
    //    BottomPlane = new(Vector3.Cross(ZFar * Target + CameraUp * (ZFar * (float)Math.Tan(FOV * .5)), CameraRight), Position),
    //    LeftPlane = new(Vector3.Cross(CameraUp, ZFar * Target + CameraRight * (ZFar * (float)Math.Tan(FOV * .5) * AspectRatio)), Position),
    //    RightPlane = new(Vector3.Cross(ZFar * Target - CameraRight * (ZFar * (float)Math.Tan(FOV * .5) * AspectRatio), CameraUp), Position)
    //};

    public void DebugDrawFrustum(CommandList list)
    {
        var view = ViewSnapshot ?? View;

        var mat = Perspective * Matrix4x4.Transpose(view);
        FrustumBox frustum = new()
        {
            LeftPlane = new(mat.M41 + mat.M11, mat.M42 + mat.M12, mat.M43 + mat.M13, mat.M44 + mat.M14),
            RightPlane = new(mat.M41 - mat.M11, mat.M42 - mat.M12, mat.M43 - mat.M13, mat.M44 - mat.M14),
            TopPlane = new(mat.M41 - mat.M21, mat.M42 - mat.M22, mat.M43 - mat.M23, mat.M44 - mat.M34),
            BottomPlane = new(mat.M41 + mat.M21, mat.M42 + mat.M22, mat.M43 + mat.M23, mat.M44 + mat.M34),
            NearPlane = new(mat.M41 + mat.M31, mat.M42 + mat.M32, mat.M43 + mat.M33, mat.M44 + mat.M34),
            FarPlane = new(mat.M41 - mat.M31, mat.M42 - mat.M32, mat.M43 - mat.M33, mat.M44 - mat.M34)
        };

        void DrawPlanes(params Vector4[] planes)
        {
            foreach (var plane in planes)
                DebugCube.Draw(new Vector3(plane.X, plane.Y, plane.Z) * 100, list);
        }

        DrawPlanes(frustum.LeftPlane, frustum.RightPlane, frustum.TopPlane, frustum.BottomPlane, frustum.NearPlane, frustum.FarPlane);
    }

    public bool IsOnFrustum(SceneryInstance instance)
    {
        if (instance.Info is not SceneryObjectInfo info) return false;

        FrustumBox f = GetFrustum(instance);

        Vector3 meshDataPos = instance.BoundBoxMin + (instance.BoundBoxMax - instance.BoundBoxMin) / 2;

        Vector3 pos = meshDataPos + info.Solid.Matrix.Translation + instance.Position;

        bool IsFrustum(Vector4 v)
            => (Vector3.Dot(pos, new(v.X, v.Y, v.Z)) + v.W + info.Radius) >= 0;

        return IsFrustum(f.LeftPlane) && IsFrustum(f.RightPlane) &&
            IsFrustum(f.FarPlane) && IsFrustum(f.NearPlane) &&
            IsFrustum(f.TopPlane) && IsFrustum(f.BottomPlane);
    }

        //Vector3 meshDataPos = instance.BoundBoxMin + (instance.BoundBoxMax - instance.BoundBoxMin) / 2;

        //Vector3 pos = meshDataPos + info.Solid.Matrix.Translation + instance.Position;

        //return false; //instance.Mesh.Name.Contains("CONE") || instance.Mesh.Name.Contains("SKYDOM") ||
               //Vector3.Distance(pos, Position) < rad &&
            //f.LeftPlane.DistanceToPlane(pos) < rad &&
            //f.RightPlane.DistanceToPlane(pos) < rad &&
            //f.FarPlane.DistanceToPlane(pos) < rad &&
            //f.NearPlane.DistanceToPlane(pos) < rad &&
            //f.TopPlane.DistanceToPlane(pos) < rad &&
            //f.BottomPlane.DistanceToPlane(pos) < rad;
    //}

    private FrustumBox GetFrustum(SceneryInstance instance)
    {
        var view = ViewSnapshot ?? View;

        var mat = Perspective * (view * (instance.InstanceMatrix * ((SceneryObjectInfo)instance.Info).Solid.Matrix));
        return new()
        {
            LeftPlane   = new(mat.M41 + mat.M11, mat.M42 + mat.M12, mat.M43 + mat.M13, mat.M44 + mat.M14),
            RightPlane  = new(mat.M41 - mat.M11, mat.M42 - mat.M12, mat.M43 - mat.M13, mat.M44 - mat.M14),
            TopPlane    = new(mat.M41 - mat.M21, mat.M42 - mat.M22, mat.M43 - mat.M23, mat.M44 - mat.M34),
            BottomPlane = new(mat.M41 + mat.M21, mat.M42 + mat.M22, mat.M43 + mat.M23, mat.M44 + mat.M34),
            NearPlane   = new(mat.M41 + mat.M31, mat.M42 + mat.M32, mat.M43 + mat.M33, mat.M44 + mat.M34),
            FarPlane    = new(mat.M41 - mat.M31, mat.M42 - mat.M32, mat.M43 - mat.M33, mat.M44 - mat.M34)
        };
    }

    private static void NormalizeBox(FrustumBox box)
    {
        box.LeftPlane = NormalizeAsFrustum(box.LeftPlane);
        box.RightPlane = NormalizeAsFrustum(box.RightPlane);
        box.TopPlane = NormalizeAsFrustum(box.TopPlane);
        box.BottomPlane = NormalizeAsFrustum(box.BottomPlane);
        box.NearPlane = NormalizeAsFrustum(box.NearPlane);
        box.FarPlane = NormalizeAsFrustum(box.FarPlane);
    }

    private static Vector4 NormalizeAsFrustum(Vector4 v)
    {
        var mag = new Vector3(v.X, v.Y, v.Z).Length();

        return v / mag;
    }

    //public FrustumBox FrustumBox
    //{
    //    get
    //    {
    //        var box = new FrustumBox();

    //        float nearHeight = (float)(2 * Math.Tan(FOV / 2) * ZNear);
    //        float nearWidth = nearHeight * AspectRatio;

    //        float farHeight = (float)(2 * Math.Tan(FOV / 2) * ZFar);
    //        float farWidth = farHeight * AspectRatio;

    //        var target = new Vector3(Target.Y, Target.X, Target.Z);//Vector3.Cross(Target, CameraRight);

    //        Vector3 nearCenter = Position + target * ZNear;
    //        Vector3 farCenter = Position + target * ZFar;

    //        // box.Center = Position + (Target * (ZFar / 2));

    //        var nBL = nearCenter - CameraUp * (nearHeight / 2) - CameraRight * (nearWidth / 2);
    //        var nBR = nearCenter - CameraUp * (nearHeight / 2) + CameraRight * (nearWidth / 2);
    //        var nTL = nearCenter + CameraUp * (nearHeight / 2) - CameraRight * (nearWidth / 2);
    //        var nTR = nearCenter + CameraUp * (nearHeight / 2) + CameraRight * (nearWidth / 2);

    //        var fBL = farCenter - CameraUp * (farHeight / 2) - CameraRight * (farWidth / 2);
    //        var fBR = farCenter - CameraUp * (farHeight / 2) + CameraRight * (farWidth / 2);
    //        var fTL = farCenter + CameraUp * (farHeight / 2) - CameraRight * (farWidth / 2);
    //        var fTR = farCenter + CameraUp * (farHeight / 2) + CameraRight * (farWidth / 2);

    //        box.NearPlane = new FrustumPlane(nBL, nBR, nTL);
    //        box.FarPlane = new FrustumPlane(fTL, fTR, fBL);
    //        box.LeftPlane = new FrustumPlane(nBL, nTL, fBL);
    //        box.RightPlane = new FrustumPlane(fTR, nTR, fBR);
    //        box.TopPlane = new FrustumPlane(fTL, nTL, fTR);
    //        box.BottomPlane = new FrustumPlane(nBL, fBL, nBR);

    //        return box;
    //    }
    //}
}
