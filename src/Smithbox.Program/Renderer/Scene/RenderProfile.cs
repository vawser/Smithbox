using StudioCore.Application;
using System.Collections.Generic;

namespace StudioCore.Renderer;

/// <summary>
/// The families of games that share a shading model. They must match GameFamily.glsl's c_isLegacy and
/// c_isTransitional (Modern is the rest); Tools/RenderSnapshotDiff's families mode checks the two against each other.
/// </summary>
public enum RenderFamily
{
    /// <summary>DES and DS1 PTDE: Phong, HemEnv (Family_Legacy.glsl)</summary>
    Legacy,

    /// <summary>DS1R, DS2 and DS2S: GGX with the gloss in the specular texture (Family_Transitional.glsl)</summary>
    Transitional,

    /// <summary>DS3 onwards (Family_Modern.glsl)</summary>
    Modern
}

/// <summary>
/// How the renderer draws a game's models: its shading family, and what its model and material formats give.
/// The one place the renderer's model code asks "which game is this?", in place of lists of project types scattered
/// through FlverResource, so a new game is one entry here and every list stays in step with the shaders.
/// The GPARAM side (post effects, exposure, LUTs) is <see cref="GparamProfile"/>.
/// Plan: Documentation/Shader Family Split Plan.md, S4 (Appendix A5 lists what each flag replaced).
/// </summary>
/// <remarks>
/// This file uses nothing but <see cref="ProjectType"/>, so tools can compile it in; the selected project's profile
/// (<c>Current</c>) is in RenderProfile.Current.cs.
/// </remarks>
public sealed partial class RenderProfile
{
    public ProjectType Type { get; private init; }

    public RenderFamily Family { get; private init; }

    /// <summary>The models are FLVER0 (DES), read and drawn by FlverResource's DES path</summary>
    public bool Flver0 { get; private init; }

    /// <summary>
    /// FLVERs can be read by the GPU-optimized path that skips building a full FLVER2. Not for DS1 and DS1R, whose
    /// FLVERs it can't read, nor NR, whose newer FLVER version it doesn't know
    /// </summary>
    public bool OptimizedFlverLoad { get; private init; } = true;

    /// <summary>Materials are MATBINs rather than MTDs</summary>
    public bool Matbin { get; private init; }

    /// <summary>
    /// A MATBIN's sampler path wins over the FLVER's texture path: Duskbloods FLVERs only give paths for albedo
    /// slots (Documentation/Claude/DB_NT Textures.md)
    /// </summary>
    public bool MatbinTexturePaths { get; private init; }

    /// <summary>
    /// The specular texture holds the gloss, so MaterialBinding routes it to the shininess row, which the shader reads
    /// as roughness for the same games (GameFamily.glsl, c_roughnessFromShininess)
    /// </summary>
    public bool GlossInSpecular => Family == RenderFamily.Transitional;

    /// <summary>Metallic shading (specialization constant 12) for materials with a metallic map or no specular map</summary>
    public bool Metallic { get; private init; }

    /// <summary>DES material textures: the texture containers of its folders, and a map piece's g_Lightmap</summary>
    public bool DesMaterials { get; private init; }

    /// <summary>An MTD's g_LightingType says the model is lit by the model light set (DES)</summary>
    public bool MaterialLightingType { get; private init; }

    /// <summary>SpeedTree models have their own vertex layout (ER, NR)</summary>
    public bool SpeedTreeVertices { get; private init; }

    private static readonly Dictionary<ProjectType, RenderProfile> Profiles = new();

    /// <summary>The profile of a project type; an unknown one is drawn as a modern MTD game</summary>
    public static RenderProfile For(ProjectType type)
    {
        lock (Profiles)
        {
            if (!Profiles.TryGetValue(type, out var profile))
                Profiles[type] = profile = Create(type);

            return profile;
        }
    }

    /// <summary>The family of a project type</summary>
    public static RenderFamily FamilyOf(ProjectType type) => type switch
    {
        ProjectType.DES or ProjectType.DS1 => RenderFamily.Legacy,
        ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S => RenderFamily.Transitional,
        _ => RenderFamily.Modern
    };

    private static RenderProfile Create(ProjectType type)
    {
        var matbinGame = type is ProjectType.ER or ProjectType.AC6 or ProjectType.NR or ProjectType.DB_NT or ProjectType.DB;

        return new RenderProfile
        {
            Type = type,
            Family = FamilyOf(type),
            Flver0 = type is ProjectType.DES,
            OptimizedFlverLoad = type is not (ProjectType.DS1 or ProjectType.DS1R or ProjectType.NR),
            Matbin = matbinGame,
            MatbinTexturePaths = type is ProjectType.DB_NT or ProjectType.DB,
            Metallic = matbinGame,
            DesMaterials = type is ProjectType.DES,
            MaterialLightingType = type is ProjectType.DES,
            SpeedTreeVertices = type is ProjectType.ER or ProjectType.NR
        };
    }
}
