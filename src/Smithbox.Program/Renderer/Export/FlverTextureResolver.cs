using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Renderer.Export;

/// <summary>
/// Resolves the albedo (diffuse), normal and specular/roughness-like texture virtual paths for a FLVER2
/// material, independent of the GPU resource pipeline. Mirrors the texture-type matching rules used by the
/// live renderer (<c>FlverResource.LookupTexture</c>/<c>ProcessMaterialTexture</c>), plus a best-effort
/// alpha-mode guess from the material's shader/MTD name (blend masks, detail maps, ... are still ignored -
/// there's no glTF slot for them).
/// </summary>
public static class FlverTextureResolver
{
    public class ResolvedTextures
    {
        public string AlbedoVirtualPath;
        public string NormalVirtualPath;

        /// <summary>
        /// Whichever of the material's specular/shininess/reflectance-style maps was found. From's engine
        /// doesn't use glTF's metallic-roughness model, so this is wired into the roughness channel of the
        /// exported material as a best-effort approximation, not a byte-exact conversion.
        /// </summary>
        public string SpecularVirtualPath;

        /// <summary>
        /// The material's metallic mask (ER-era "MetallicMap" samplers), if it has one.
        /// </summary>
        public string MetallicVirtualPath;

        /// <summary>
        /// Always "MASK" - see <see cref="DetermineAlphaMode"/> for why.
        /// </summary>
        public string AlphaMode = "MASK";
    }

    public static ResolvedTextures Resolve(ProjectEntry project, FLVER2.Material material, string flverVirtPath)
    {
        var result = new ResolvedTextures
        {
            AlphaMode = DetermineAlphaMode()
        };

        // Several samplers can map onto one slot (detail blends, body + hair albedo, ...). For chr models in MATBIN
        // games pick the way the live viewport does: the character's own texture named after the material first,
        // then any of its own textures, then shared (AAT) ones. Everything else keeps the first one found.
        var orderedSelection = flverVirtPath != null && flverVirtPath.StartsWith("chr/") &&
                               project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR;

        var albedoCandidates = new List<string>();
        var normalCandidates = new List<string>();
        var metallicCandidates = new List<string>();

        foreach (var tex in material.Textures)
        {
            var paramName = (tex.ParamName ?? "G_DIFFUSE").ToUpperInvariant();

            var isAlbedo2 = paramName is "G_DIFFUSETEXTURE2" or "G_DIFFUSE2" || paramName.Contains("ALBEDO_2");
            var isAlbedo = !isAlbedo2 && (paramName is "G_DIFFUSETEXTURE" or "G_DIFFUSE" || paramName.Contains("ALBEDO"));

            var isNormal2 = paramName is "G_BUMPMAPTEXTURE2" or "G_BUMPMAP2" || paramName.Contains("NORMAL_2");
            var isNormal = !isNormal2 && (paramName is "G_BUMPMAPTEXTURE" or "G_BUMPMAP" || paramName.Contains("NORMAL"));

            var isSpecular2 = paramName is "G_SPECULARTEXTURE2" or "G_SPECULAR2" || paramName.Contains("SPECULAR_2");
            var isSpecular = !isSpecular2 && (paramName is "G_SPECULARTEXTURE" or "G_SPECULAR" || paramName.Contains("SPECULAR"));

            var isShininess2 = paramName is "G_SHININESSTEXTURE2" or "G_SHININESS2" || paramName.Contains("SHININESS2");
            var isShininess = !isShininess2 && (paramName is "G_SHININESSTEXTURE" or "G_SHININESS" || paramName.Contains("SHININESS"));

            var isRoughnessLike = paramName.Contains("ROUGHNESS") || paramName.Contains("REFLECTANCE");
            var isMetallic = paramName.Contains("METALLIC");

            if (!isAlbedo && !isNormal && !isSpecular && !isShininess && !isRoughnessLike && !isMetallic)
                continue;

            var resolvedPath = ResolveTexturePath(project, tex.ParamName, tex.Path, material.MTD);
            if (string.IsNullOrEmpty(resolvedPath))
                continue;

            var virtualPath = TextureLocator.GetFlverTextureVirtualPath(flverVirtPath, resolvedPath.ToLowerInvariant(), material.Name);

            if (isAlbedo)
                albedoCandidates.Add(virtualPath);
            else if (isNormal)
                normalCandidates.Add(virtualPath);
            else if (isMetallic)
                metallicCandidates.Add(virtualPath);
            else if ((isSpecular || isShininess || isRoughnessLike) && result.SpecularVirtualPath == null)
                result.SpecularVirtualPath = virtualPath;
        }

        result.AlbedoVirtualPath = PickTexture(albedoCandidates, material.Name, orderedSelection);
        result.NormalVirtualPath = PickTexture(normalCandidates, material.Name, orderedSelection);
        result.MetallicVirtualPath = PickTexture(metallicCandidates, material.Name, orderedSelection);

        return result;
    }

    private static string PickTexture(List<string> candidates, string materialName, bool orderedSelection)
    {
        if (candidates.Count == 0)
            return null;

        if (!orderedSelection)
            return candidates[0];

        foreach (var c in candidates)
        {
            if (!c.StartsWith("aat/") && TextureLocator.TextureNameMatchesMaterial(c, materialName))
                return c;
        }

        foreach (var c in candidates)
        {
            if (!c.StartsWith("aat/"))
                return c;
        }

        return candidates[0];
    }

    /// <summary>
    /// Smithbox's own viewport (FlverShader.frag) doesn't actually branch on material type/g_BlendMode for
    /// transparency at all - every FLVER material, unconditionally, does a hard alpha-test discard against
    /// the diffuse texture's own alpha channel at a fixed 0.5 threshold:
    /// <code>if (diffuseColor.w &lt; 0.5) discard;</code>
    /// That's the entire "transparency system" the live viewport uses, so this reproduces it exactly with
    /// glTF's MASK alphaMode (alphaCutoff 0.5) rather than trying to infer OPAQUE/BLEND from the material's
    /// shader name or g_BlendMode param, which don't actually drive this in Smithbox's renderer.
    /// </summary>
    private static string DetermineAlphaMode()
    {
        return "MASK";
    }

    /// <summary>
    /// Given a single FLVER texture entry, returns the actual sampler path to use - either the path stored
    /// directly on the FLVER texture entry, or (for games using MTD/MATBIN indirection, where the FLVER
    /// entry's path is blank) the path looked up from the material's MTD/MATBIN definition.
    /// </summary>
    private static string ResolveTexturePath(ProjectEntry project, string type, string mpath, string mtd)
    {
        if (!string.IsNullOrEmpty(mpath))
            return mpath;

        var bank = project.Handler.MaterialData?.PrimaryBank;
        if (bank == null)
            return null;

        var mtdString = Path.GetFileNameWithoutExtension(mtd.Replace('\\', Path.DirectorySeparatorChar));

        // MTD-based games
        var mtdMaterial = bank.GetMaterial(mtdString);
        if (mtdMaterial != null)
        {
            var tex = mtdMaterial.Textures.Find(x => x.Type == type);
            if (tex != null && tex.Extended && tex.Path != "")
                return tex.Path;
        }

        // MATBIN-based games (ER, AC6, NR)
        if (project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            var matbin = bank.GetMatbin(mtdString);
            if (matbin != null)
            {
                MATBIN.Sampler match = null;

                foreach (var sampler in matbin.Samplers)
                {
                    if (sampler.Type == type)
                    {
                        match = sampler;
                        break;
                    }
                }

                if (match == null)
                {
                    foreach (var sampler in matbin.Samplers)
                    {
                        if (type == null)
                            continue;

                        if (sampler.Type.Contains("__") && type.Contains("__"))
                        {
                            var samplerType = sampler.Type.Split("__")[1];
                            var sourceType = type.Split("__")[1];

                            if (samplerType == sourceType)
                            {
                                match = sampler;
                                break;
                            }
                        }

                        if ((type == "g_DiffuseTexture" || type.Contains("AlbedoMap")) && sampler.Type.Contains("AlbedoMap"))
                        {
                            match = sampler;
                            break;
                        }

                        if ((type == "g_BumpmapTexture" || type.Contains("NormalMap")) && sampler.Type.Contains("NormalMap"))
                        {
                            match = sampler;
                            break;
                        }
                    }
                }

                if (match != null && match.Path != "")
                    return match.Path;
            }
        }

        return null;
    }
}
