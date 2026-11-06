using System.Numerics;

namespace StudioCore.Renderer;

// The renderer's transform, as OldHavokTagfile.cs (a copy of the pre-6.1 Renderer/Animation/HavokTagfile.cs) returns it.
public struct HavokTransformData
{
    public Vector3 Translation;
    public Quaternion Rotation;
    public Vector3 Scale;
}
