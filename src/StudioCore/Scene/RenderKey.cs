using System;
using System.Runtime.CompilerServices;

namespace StudioCore.Scene;

public struct RenderKey : IComparable<RenderKey>, IComparable
{
    public readonly ulong Value;

    public RenderKey(ulong value)
    {
        Value = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderKey Create(int materialID, float cameraDistance)
    {
        return Create((uint)materialID, cameraDistance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderKey Create(uint materialID, float cameraDistance)
    {
        var cameraDistanceInt = (uint)Math.Min(uint.MaxValue, cameraDistance * 1000f);

        return new RenderKey(
            ((ulong)materialID << 32) +
            cameraDistanceInt);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderKey Create(uint materialID, uint bufferID)
    {
        return new RenderKey(
            ((ulong)materialID << 32) +
            bufferID);
    }

    public int CompareTo(RenderKey other)
    {
        return Value.CompareTo(other.Value);
    }

    int IComparable.CompareTo(object obj)
    {
        return Value.CompareTo(obj);
    }
}
