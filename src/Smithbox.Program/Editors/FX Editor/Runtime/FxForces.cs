using SoulsFormats;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

public enum FxForceKind { Wind, Gravity, Turbulence, Cancel }

/// <summary>ForceVolumeShape: 0 Boundless, 1 Sphere, 2 Box, 3 Cylinder, 4 SquarePrism.</summary>
public enum FxForceShape { Boundless = 0, Sphere = 1, Box = 2, Cylinder = 3, SquarePrism = 4 }

/// <summary>
/// A force appearance (FX Editor Plan §20): WindForce (10300), GravityForce (10301), TurbulenceForce (10303) or
/// CancelForce. Like lights, it's placed by the instances its config emits (every vanilla force config has a
/// OneTimeEmitter), each a volume with the node's rotation at the instance's position.
/// </summary>
public sealed class FxForceDef
{
    public FxrAction Action;
    public FxForceKind Kind;
    public FxForceShape Shape;
    public float SphereRadius;
    public Vector3 BoxSize;
    public float CylinderHeight, CylinderRadius, PrismHeight, PrismApothem;

    public FxValue Force;
    public float ForceMultiplier = 1, RandomMin = 1, RandomMax = 1;

    // Turbulence
    public FxValue NoiseOffsetX, NoiseOffsetY, NoiseOffsetZ;
    public float NoiseScale = 1;
    public bool SoftRadiusEnabled;
    public float SoftRadius = 1;

    public static FxForceDef Build(FxAppearanceDef appearance)
    {
        var view = appearance.View;
        if (view?.IsTyped != true)
            return null;
        FxForceKind? kind = appearance.Action.Type switch
        {
            10300 => FxForceKind.Wind,
            10301 => FxForceKind.Gravity,
            10303 => FxForceKind.Turbulence,
            _ => appearance.Name == "CancelForce" ? FxForceKind.Cancel : null,
        };
        if (kind == null)
            return null;

        var box = FxRead.Vec3(view, "boxSize");
        return new FxForceDef
        {
            Action = appearance.Action,
            Kind = kind.Value,
            Shape = (FxForceShape)Math.Clamp(FxRead.Int(view, "shape", 1), 0, 4),
            SphereRadius = FxRead.Float(view, "sphereRadius", 10),
            BoxSize = box,
            CylinderHeight = FxRead.Float(view, "cylinderHeight", 10),
            CylinderRadius = FxRead.Float(view, "cylinderRadius", 10),
            PrismHeight = FxRead.Float(view, "squarePrismHeight", 10),
            PrismApothem = FxRead.Float(view, "squarePrismApothem", 10),
            Force = FxRead.Prop(view, "force"),
            ForceMultiplier = FxRead.Float(view, "forceMultiplier", 1),
            RandomMin = FxRead.Float(view, "forceRandomMultiplierMin", 1),
            RandomMax = FxRead.Float(view, "forceRandomMultiplierMax", 1),
            NoiseOffsetX = FxRead.Prop(view, "noiseOffsetX"),
            NoiseOffsetY = FxRead.Prop(view, "noiseOffsetY"),
            NoiseOffsetZ = FxRead.Prop(view, "noiseOffsetZ"),
            NoiseScale = FxRead.Float(view, "noiseScale", 1),
            SoftRadiusEnabled = FxRead.Int(view, "enableSoftRadius", 0) != 0,
            SoftRadius = FxRead.Float(view, "softRadius", 1),
        };
    }
}

/// <summary>One force volume where it is this step: its definition, its world transform, its strength.</summary>
public struct FxForceVolume
{
    public FxForceDef Def;
    public Matrix4x4 World;
    public Matrix4x4 Inverse;
    public float Strength;
    public Vector3 NoiseOffset;
}

/// <summary>
/// The forces acting in a running effect (FX Editor Plan §20): the volumes its own force appearances place, plus the
/// preview wind standing in for the game's weather (boundless, along a set direction). Gathered once per update of the
/// top-level effect, so a particle feels the volumes as they were at the end of the previous update.
/// </summary>
public sealed class FxForceField
{
    public readonly List<FxForceVolume> Volumes = new();

    /// <summary>The preview wind (acceleration, world space), as weather wind would push responders.</summary>
    public Vector3 Wind;

    /// <summary>Whether the effect's own force volumes act (the survey turns them off to compare).</summary>
    public bool UseVolumes = true;

    public bool IsEmpty => Wind == Vector3.Zero && Volumes.Count == 0;

    /// <summary>The force at a world position: the sum of the volumes holding it, plus the wind; none inside a CancelForce.</summary>
    public Vector3 At(Vector3 world)
    {
        if (IsEmpty)
            return Vector3.Zero;

        var sum = Wind;
        foreach (var v in Volumes)
        {
            var local = Vector3.Transform(world, v.Inverse);
            if (!Inside(v.Def, local))
                continue;

            switch (v.Def.Kind)
            {
                case FxForceKind.Cancel:
                    return Vector3.Zero;
                case FxForceKind.Wind:
                    // Along the volume node's +Z (the axis the long sides of vanilla's wind boxes run along)
                    sum += SafeNormalize(Vector3.TransformNormal(Vector3.UnitZ, v.World)) * v.Strength;
                    break;
                case FxForceKind.Gravity:
                {
                    // Towards the volume's origin; a negative strength pushes away
                    var to = v.World.Translation - world;
                    float d = to.Length();
                    if (d > 1e-4f)
                        sum += to / d * v.Strength;
                    break;
                }
                case FxForceKind.Turbulence:
                {
                    float soft = 1;
                    if (v.Def.SoftRadiusEnabled && v.Def.SoftRadius > 0)
                        soft = Math.Clamp(1 - local.Length() / v.Def.SoftRadius, 0, 1);
                    var q = local / Math.Max(v.Def.NoiseScale, 1e-3f) + v.NoiseOffset;
                    var dir = new Vector3(Noise(q), Noise(q + new Vector3(31.4f, 7.1f, 12.9f)), Noise(q + new Vector3(-5.3f, 23.8f, -17.2f)));
                    sum += Vector3.TransformNormal(dir, v.World) * v.Strength * soft;
                    break;
                }
            }
        }
        return sum;
    }

    /// <summary>Whether a point in the volume's own space is inside it. Box, sphere and cylinder are centred; the prism stands on its base.</summary>
    public static bool Inside(FxForceDef d, Vector3 p) => d.Shape switch
    {
        FxForceShape.Boundless => true,
        FxForceShape.Sphere => p.LengthSquared() <= d.SphereRadius * d.SphereRadius,
        FxForceShape.Box => MathF.Abs(p.X) <= d.BoxSize.X / 2 && MathF.Abs(p.Y) <= d.BoxSize.Y / 2 && MathF.Abs(p.Z) <= d.BoxSize.Z / 2,
        FxForceShape.Cylinder => MathF.Abs(p.Y) <= d.CylinderHeight / 2 && p.X * p.X + p.Z * p.Z <= d.CylinderRadius * d.CylinderRadius,
        FxForceShape.SquarePrism => p.Y >= 0 && p.Y <= d.PrismHeight && MathF.Abs(p.X) <= d.PrismApothem && MathF.Abs(p.Z) <= d.PrismApothem,
        _ => false,
    };

    private static Vector3 SafeNormalize(Vector3 v) => v.LengthSquared() > 1e-12f ? Vector3.Normalize(v) : Vector3.UnitZ;

    // ---------------------------------------------------------------- noise

    /// <summary>Smooth value noise in [-1, 1]: a stand-in for the game's 3D noise, deterministic so replays match.</summary>
    public static float Noise(Vector3 p)
    {
        int x0 = (int)MathF.Floor(p.X), y0 = (int)MathF.Floor(p.Y), z0 = (int)MathF.Floor(p.Z);
        float fx = Smooth(p.X - x0), fy = Smooth(p.Y - y0), fz = Smooth(p.Z - z0);
        float L(float a, float b, float t) => a + (b - a) * t;
        float C(int dx, int dy, int dz) => Hash(x0 + dx, y0 + dy, z0 + dz);
        return L(L(L(C(0, 0, 0), C(1, 0, 0), fx), L(C(0, 1, 0), C(1, 1, 0), fx), fy),
                 L(L(C(0, 0, 1), C(1, 0, 1), fx), L(C(0, 1, 1), C(1, 1, 1), fx), fy), fz);
    }

    /// <summary>A value that wanders smoothly between <paramref name="min"/> and <paramref name="max"/> over time (twice a second).</summary>
    public static float Wander(float min, float max, float time, uint seed)
    {
        if (min == max)
            return min;
        float t = time * 2;
        int i = (int)MathF.Floor(t);
        float a = (Hash(i, (int)seed, 17) + 1) / 2, b = (Hash(i + 1, (int)seed, 17) + 1) / 2;
        float f = Smooth(t - i);
        return min + (max - min) * (a + (b - a) * f);
    }

    private static float Smooth(float t) => t * t * (3 - 2 * t);

    private static float Hash(int x, int y, int z)
    {
        uint h = (uint)x * 0x8DA6B343u ^ (uint)y * 0xD8163841u ^ (uint)z * 0xCB1AB31Fu;
        h ^= h >> 13;
        h *= 0x5BD1E995u;
        h ^= h >> 15;
        return (h & 0xFFFFFF) / (float)0x7FFFFF - 1;
    }
}
