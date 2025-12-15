using System;
using System.Numerics;

namespace StudioCore.Renderer;

public static class MathUtils
{
    public static Quaternion LookRotation(Vector3 forward, Vector3 up)
    {
        forward = Vector3.Normalize(forward);
        up = Vector3.Normalize(up);

        Vector3 right = Vector3.Normalize(Vector3.Cross(up, forward));
        up = Vector3.Cross(forward, right);

        var m = new Matrix4x4(
            right.X, right.Y, right.Z, 0,
            up.X, up.Y, up.Z, 0,
            forward.X, forward.Y, forward.Z, 0,
            0, 0, 0, 1
        );

        return Quaternion.CreateFromRotationMatrix(m);
    }

    public static Vector3 QuaternionToEuler(Quaternion q)
    {
        float sinPitch = 2f * (q.W * q.X - q.Y * q.Z);
        float pitch = MathF.Abs(sinPitch) >= 1
            ? MathF.CopySign(MathF.PI / 2, sinPitch)
            : MathF.Asin(sinPitch);

        float yaw = MathF.Atan2(
            2f * (q.W * q.Y + q.Z * q.X),
            1f - 2f * (q.X * q.X + q.Y * q.Y)
        );

        float roll = MathF.Atan2(
            2f * (q.W * q.Z + q.X * q.Y),
            1f - 2f * (q.Z * q.Z + q.X * q.X)
        );

        const float rad2deg = 180f / MathF.PI;
        return new Vector3(pitch, yaw, roll) * rad2deg;
    }
}
