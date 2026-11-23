using System;
using System.Runtime.CompilerServices;

namespace StudioCore.Renderer;

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

    /// <summary>
    /// Creates a key that sorts after everything but overlays, with distant objects first, as
    /// translucent objects must be drawn over whatever is behind them.
    /// </summary>
    public const ulong BackToFrontStart = (ulong)uint.MaxValue << 32;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static RenderKey CreateBackToFront(float cameraDistance)
    {
        // At least 2, so the two top keys stay free: ScreenEffect, then ulong.MaxValue (overdrawn debug primitives)
        var cameraDistanceInt = (uint)Math.Clamp(cameraDistance * 1000f, 2f, 4.0e9f);

        return new RenderKey(BackToFrontStart | (uint.MaxValue - cameraDistanceInt));
    }

    /// <summary>
    /// The key of draws that read the scene colour behind them (the FX Editor's Distortion and RadialBlur): after
    /// every other draw, translucent ones included, so the copy they read, taken just before the first of them, has
    /// what is in front of the opaque scene too (FFX plan §20.34). Only overdrawn debug primitives (ulong.MaxValue)
    /// come after.
    /// </summary>
    public const ulong ScreenEffect = ulong.MaxValue - 1;

    public int CompareTo(RenderKey other)
    {
        return Value.CompareTo(other.Value);
    }

    int IComparable.CompareTo(object obj)
    {
        return Value.CompareTo(obj);
    }
}
