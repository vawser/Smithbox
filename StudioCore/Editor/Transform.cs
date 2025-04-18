using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore;

/// <summary>
/// Struct that holds Position, Rotation, Scale and Override Matrix information.
/// </summary>
public struct Transform
{
    public static readonly Transform Default = new(Vector3.Zero, Vector3.Zero, Vector3.One);

    public Transform(Vector3 pos, Vector3 rot)
    {
        Position = pos;
        Rotation = EulerToQuaternion(rot);
        Scale = Vector3.One;
        OverrideMatrixWorld = Matrix4x4.Identity;
    }

    public Transform(Vector3 pos, Vector3 rot, Vector3 scale)
    {
        Position = pos;
        Rotation = EulerToQuaternion(rot);
        Scale = scale;
        OverrideMatrixWorld = Matrix4x4.Identity;
    }

    public Transform(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        Position = pos;
        Rotation = rot;
        Scale = scale;
        OverrideMatrixWorld = Matrix4x4.Identity;
    }

    public Transform(Matrix4x4 overrideMatrixWorld)
    {
        Position = Vector3.Zero;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
        OverrideMatrixWorld = overrideMatrixWorld;
    }

    public Transform(float x, float y, float z, float rx, float ry, float rz)
        : this(new Vector3(x, y, z), new Vector3(rx, ry, rz))
    {
    }

    public Transform(float x, float y, float z, float rx, float ry, float rz, float sx, float sy, float sz)
        : this(new Vector3(x, y, z), new Vector3(rx, ry, rz), new Vector3(sx, sy, sz))
    {
    }

    public static Quaternion EulerToQuaternion(Vector3 euler)
    {
        var qy = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), euler.Y);
        var qz = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), euler.Z);
        var qx = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), euler.X);
        return qy * qz * qx;
    }

    public static Quaternion EulerToQuaternionXZY(Vector3 euler)
    {
        /*var qy = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), euler.Y);
        var qz = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), euler.Z);
        var qx = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), euler.X);
        var qd = qx * qz * qy;*/
        var cx = MathF.Cos(euler.X / 2);
        var cy = MathF.Cos(euler.Y / 2);
        var cz = MathF.Cos(euler.Z / 2);
        var sx = MathF.Sin(euler.X / 2);
        var sy = MathF.Sin(euler.Y / 2);
        var sz = MathF.Sin(euler.Z / 2);
        var q = new Quaternion();
        q.X = (sx * cy * cz) + (cx * sy * sz);
        q.Y = (cx * sy * cz) + (sx * cy * sz);
        q.Z = (cx * cy * sz) + (sx * sy * cz);
        q.W = (cx * cy * cz) + (sx * sy * sz);
        //return q;
        Matrix4x4 m1 = Matrix4x4.CreateRotationX(euler.X) *
                       Matrix4x4.CreateRotationZ(euler.Z) * Matrix4x4.CreateRotationY(euler.Y);
        var qs = Quaternion.CreateFromRotationMatrix(m1);
        //return qs;

        cx = MathF.Cos(euler.X);
        cy = MathF.Cos(euler.Y);
        cz = MathF.Cos(euler.Z);
        sx = MathF.Sin(euler.X);
        sy = MathF.Sin(euler.Y);
        sz = MathF.Sin(euler.Z);
        Matrix4x4 m = Matrix4x4.Identity;
        m.M11 = cz * cy;
        m.M12 = sz; //-sz;
        m.M13 = -cz * sy; //cz * sy;
        m.M21 = (sx * sy) - (sz * cx * cy); //sx * sy + cx * sz * cy;
        m.M22 = cx * cz;
        m.M23 = (cx * sz * sy) + (cy * sx); //cx * sz * sy - cy * sx;
        m.M31 = (cy * sx * sz) + (cx * sy); //cy * sx * sz - cx * sy;
        m.M32 = -cz * sx; //cz * sx;
        m.M33 = (cx * cy) - (sx * sz * sy); //cx * cy + sx * sz * sy;
        var qm = Quaternion.CreateFromRotationMatrix(m);
        return qm;
    }

    public static Vector3 QuaternionToEuler(Quaternion q, EulerUtils.RotSeq r = EulerUtils.RotSeq.yzx)
    {
        return EulerUtils.quaternion2Euler(q, r);
    }

    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Vector3 EulerRotation
    {
        get => QuaternionToEuler(Rotation);
        set => Rotation = EulerToQuaternion(value);
    }

    public Vector3 EulerRotationXZY
    {
        get =>
            //return QuaternionToEuler(Rotation, EulerUtils.RotSeq.xzy);
            EulerUtils.MatrixToEulerXZY(RotationMatrix);
        set => Rotation = EulerToQuaternionXZY(value);
    }

    public Matrix4x4 ScaleMatrix => Matrix4x4.CreateScale(Scale);

    public Matrix4x4 TranslationMatrix => Matrix4x4.CreateTranslation(Position.X, Position.Y, Position.Z);

    //public Matrix4x4 RotationMatrix => Matrix4x4.CreateRotationY(EulerRotation.Y)
    //    * Matrix4x4.CreateRotationZ(EulerRotation.Z)
    //    * Matrix4x4.CreateRotationX(EulerRotation.X);
    public Matrix4x4 RotationMatrix => Matrix4x4.CreateFromQuaternion(Rotation);

    public Matrix4x4 RotationMatrixNeg => Matrix4x4.CreateRotationY(-EulerRotation.Y)
                                          * Matrix4x4.CreateRotationZ(-EulerRotation.Z)
                                          * Matrix4x4.CreateRotationX(-EulerRotation.X);

    public Matrix4x4 RotationMatrixXYZ => Matrix4x4.CreateRotationX(EulerRotation.X)
                                          * Matrix4x4.CreateRotationY(EulerRotation.Y)
                                          * Matrix4x4.CreateRotationZ(EulerRotation.Z);


    public Matrix4x4 WorldMatrix => OverrideMatrixWorld != Matrix4x4.Identity
        ? OverrideMatrixWorld
        : ScaleMatrix * RotationMatrix * TranslationMatrix;

    public Vector3 Look => Vector3.Transform(Vector3.UnitZ, WorldMatrix);
    public Vector3 Up => Vector3.Transform(Vector3.UnitY, Rotation);

    public Matrix4x4 CameraViewMatrix => Matrix4x4.CreateTranslation(-Position.X, -Position.Y, -Position.Z)
                                         * Matrix4x4.CreateRotationY(EulerRotation.Y)
                                         * Matrix4x4.CreateRotationZ(EulerRotation.Z)
                                         * Matrix4x4.CreateRotationX(EulerRotation.X);

    public Matrix4x4 CameraViewMatrixLH
    {
        get
        {
            Vector3 look = Vector3.Normalize(Look - Position);
            Vector3 right = Vector3.Normalize(Vector3.Cross(Up, look));
            Vector3 up = Vector3.Normalize(Vector3.Cross(look, right));
            var a = -Vector3.Dot(right, Position);
            var b = -Vector3.Dot(up, Position);
            var c = -Vector3.Dot(look, Position);
            return Matrix4x4.Transpose(new Matrix4x4(right.X, right.Y, right.Z, a,
                up.X, up.Y, up.Z, b,
                look.X, look.Y, look.Z, c,
                0.0f, 0.0f, 0.0f, 1.0f));
        }
    }

    private static readonly Random rand = new();

    public static Transform RandomUnit(bool randomRot = false)
    {
        float randFloat()
        {
            return (float)((rand.NextDouble() * 2) - 1);
        }

        return new Transform(randFloat(), randFloat(), randFloat(),
            randomRot ? randFloat() * Utils.PiOver2 : 0,
            randomRot ? randFloat() * Utils.PiOver2 : 0,
            randomRot ? randFloat() * Utils.PiOver2 : 0);
    }

    public Matrix4x4 OverrideMatrixWorld;

    public override string ToString()
    {
        return $"Pos: {Position.ToString()} Rot (deg): {EulerRotation.ToString()}";
        //return $"Pos: {Position.ToString()} Rot (deg): {EulerRotation.Rad2Deg().ToString()}";
    }

    public static Transform operator *(Transform a, float b)
    {
        return new Transform(a.Position * b, a.EulerRotation);
    }

    public static Transform operator /(Transform a, float b)
    {
        return new Transform(a.Position / b, a.EulerRotation);
    }

    public static Transform operator +(Transform a, Vector3 b)
    {
        return new Transform(a.Position + b, a.EulerRotation);
    }

    public static Transform operator -(Transform a, Vector3 b)
    {
        return new Transform(a.Position - b, a.EulerRotation);
    }

    public static Transform operator +(Transform a, Transform b)
    {
        return new Transform(a.Position + b.Position, a.EulerRotation + b.EulerRotation);
    }

    public static Transform operator -(Transform a, Transform b)
    {
        return new Transform(a.Position - b.Position, a.EulerRotation - b.EulerRotation);
    }

    public static implicit operator Transform(Vector3 v)
    {
        return new Transform(v, Vector3.Zero);
    }
}
