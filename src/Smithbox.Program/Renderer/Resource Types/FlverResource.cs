#nullable enable
using Microsoft.Extensions.Logging;
using SoulsFormats;
using SoulsFormats.Utilities;
using StudioCore.Developer;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Tracy;
using Veldrid;
using Veldrid.Utilities;
using static SoulsFormats.FLVER;

namespace StudioCore.Renderer;

public class FlverResource : IResource, IDisposable
{
    public const bool CaptureMaterialLayouts = false;
    private static readonly Stack<FlverCache> FlverCaches = new();
    private static readonly object CacheLock = new();

    public static Dictionary<string, FLVER2.BufferLayout> MaterialLayouts = new();
    public static object _matLayoutLock = new();

    public FLVER0? FlverDeS;
    public FLVER2? Flver;

    public FlverMaterial[]? GPUMaterials;
    public FlverSubmesh[]? GPUMeshes;

    public static int CacheCount { get; private set; }

    public static long CacheFootprint
    {
        get
        {
            long total = 0;
            lock (CacheLock)
            {
                foreach (FlverCache c in FlverCaches)
                {
                    total += c.MemoryUsage;
                }
            }

            return total;
        }
    }

    public BoundingBox Bounds { get; set; }

    public List<FLVER.Node>? Bones { get; private set; }
    private List<FlverBone>? FBones { get; set; }
    private List<Matrix4x4>? BoneTransforms { get; set; }

    /// <summary>
    /// The world matrix of each node of a FLVER2, which <see cref="StaticBoneBuffer"/> holds for the static meshes whose
    /// normal W picks their node. Set before the meshes are processed, as their bounds need it too.
    /// </summary>
    private Matrix4x4[] StaticBoneTransforms { get; set; } = [];

    public GPUBufferAllocator.GPUBufferHandle? StaticBoneBuffer { get; private set; }

    /// <summary>
    /// Triangles of the full detail facesets, so not counting faceset LODs or motion blur.
    /// </summary>
    public int DetailTriangleCount { get; private set; }

    /// <summary>
    /// Surface area of the full detail facesets, in model space.
    /// </summary>
    public float DetailSurfaceArea { get; private set; }

    /// <summary>
    /// A material or texture is named as a low detail one (i.e. m30_00_low_hall_03_a).
    /// </summary>
    public bool HasLowDetailNaming { get; private set; }

    public bool IsSpeedtree = false;
    /// <summary>
    /// Where this model was loaded from. Gives access to the pool its textures are looked up in.
    /// </summary>
    public ResourceLoadContext? Context { get; private set; }

    // Set before anything is read, so that all of the load can follow it
    private bool ModelsOnly => Context?.Pool?.ModelsOnly == true;
    private static bool RequiresFullFlver => FocusManager.IsInModelEditor() || FocusManager.IsInTimeActEditor();

    public bool _Load(Memory<byte> bytes, AccessLevel al, ResourceLoadContext ctx)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        Context = ctx;

        if (curProject == null)
            return false;

        if (CFG.Current.Developer_Enable_Tools)
        {
            ResourceViewer.ProcessedMeshes.Add(ctx.QualifiedName);
            ResourceViewer.MeshConsumptionSize += bytes.Length;
        }

        bool ret;
        var profile = RenderProfile.Of(curProject);

        if (profile.Flver0)
        {
            FlverDeS = FLVER0.Read(bytes);
            ret = LoadInternalDeS(al);
        }
        else
        {
            if (al == AccessLevel.AccessGPUOptimizedOnly &&
                profile.OptimizedFlverLoad && (ModelsOnly || !RequiresFullFlver))
            {
                BinaryReaderEx br = new(false, bytes);
                DCX.Type ctype;
                br = SFUtil.GetDecompressedBR(br, out ctype);

                // Vital for fast map loading
                ret = LoadInternalFast(br);
            }
            else
            {
                FlverCache? cache = al == AccessLevel.AccessGPUOptimizedOnly ? GetCache() : null;

                Flver = FLVER2.Read(bytes, cache);
                ret = LoadInternal(al);

                if (cache != null)
                    ReleaseCache(cache);
            }
        }

        return ret;
    }

    public bool _Load(string relativePath, AccessLevel al, ResourceLoadContext ctx)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        Context = ctx;

        if (curProject == null)
            return false;

        // Small hack so the chrbnd's that are passed here are skipped.
        if (!(relativePath.Contains(".flv")))
            return false;

        try
        {
            var fileData = curProject.VFS.FS.ReadFile(relativePath);

            bool ret = false;

            if (fileData != null && fileData.Value.Length > 1)
            {
                var profile = RenderProfile.Of(curProject);

                if (profile.Flver0)
                {
                    FlverDeS = FLVER0.Read(fileData.Value);
                    ret = LoadInternalDeS(al);
                }
                else
                {
                    if (al == AccessLevel.AccessGPUOptimizedOnly &&
                        profile.OptimizedFlverLoad && (ModelsOnly || !RequiresFullFlver))
                    {
                        BinaryReaderEx br = new(false, fileData.Value);
                        DCX.Type ctype;
                        br = SFUtil.GetDecompressedBR(br, out ctype);

                        // Vital for fast map loading
                        ret = LoadInternalFast(br);
                    }
                    else
                    {
                        FlverCache? cache = al == AccessLevel.AccessGPUOptimizedOnly ? GetCache() : null;

                        Flver = FLVER2.Read(fileData.Value, cache);
                        ret = LoadInternal(al);

                        if (cache != null)
                            ReleaseCache(cache);
                    }
                }
            }

            return ret;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this,
                LOC.Get("REND_FlverResource_Load_Failed", relativePath), e);
        }

        return false;
    }

    private FlverCache GetCache()
    {
        lock (CacheLock)
        {
            if (FlverCaches.Count > 0)
            {
                return FlverCaches.Pop();
            }

            CacheCount++;
        }

        return new FlverCache();
    }

    private void ReleaseCache(FlverCache cache)
    {
        if (cache != null)
        {
            cache.ResetUsage();
            lock (CacheLock)
            {
                FlverCaches.Push(cache);
            }
        }
    }

    private void LookupTexture(FlverMaterial.TextureType textureType, FlverMaterial dest, string? type, string mpath,
        string mtd)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (curProject == null)
            return;

        if (curProject.Handler.MaterialData == null || curProject.Handler.MaterialData.PrimaryBank == null)
            return;

        var bank = curProject.Handler.MaterialData.PrimaryBank;

        var path = mpath;
        var resolvedType = type;

        MTD? material = null;
        MATBIN? matbin = null;

        // Duskbloods FLVERs only give paths for albedo slots, and most that differ from the MATBIN name the
        // normal map (810 of 882 in DB_NT), so the MATBIN's path wins where it has one.
        // See Documentation/Claude/DB_NT Textures.md
        var profile = RenderProfile.Of(curProject);

        if (mpath != "" && type != null && profile.MatbinTexturePaths)
        {
            var mtdName = Path.GetFileNameWithoutExtension(mtd.Replace('\\', Path.DirectorySeparatorChar));
            var sampler = bank.GetMatbin(mtdName)?.Samplers.Find(x => x.Type == type);

            if (sampler != null && sampler.Path != "")
            {
                path = sampler.Path;
            }
        }

        if (mpath == "")
        {
            var mtdstring = Path.GetFileNameWithoutExtension(mtd.Replace('\\', Path.DirectorySeparatorChar));

            if (profile.Matbin)
            {
                matbin = bank.GetMatbin(mtdstring);
            }

            // MTD. These games can still have an old MTD by the name of a MATBIN, for a different
            // shader, so it would be missing the textures the FLVER asks for
            material = matbin == null ? bank.GetMaterial(mtdstring) : null;

            if (material != null)
            {
                MTD.Texture? tex = material.Textures.Find(x => x.Type == type);
                if (tex == null || !tex.Extended || tex.Path == "")
                {
                    //Smithbox.Log(this, $"{Context?.QualifiedName}: Failed to find MTD string: {mtdstring} - {type}");
                    return;
                }

                path = tex.Path;
                //Smithbox.Log(this, $"{Context?.QualifiedName}: MTD: {path}");
            }

            // MATBIN
            if (matbin != null)
            {
                MATBIN.Sampler? tex = null;

                bool match = false;

                foreach (var t in matbin.Samplers)
                {
                    if (t.Type == type)
                    {
                        tex = t;
                        match = true;
                        break;
                    }
                }

                // If normal match fails, try heuristic match to account for disparities between sampler and source
                if (!match)
                {
                    foreach (var t in matbin.Samplers)
                    {
                        if (type == null)
                            continue;

                        // Hack to allow some differing paths between sampler and source to work correctly in ER
                        // Namely for these aspects of the paths:
                        // Mb<x> where <x> is a number, but differs between sampler and source
                        // <x>Mask where <x> is a number, but differs between sampler and source
                        if (t.Type.Contains("__") && type.Contains("__"))
                        {
                            var samplerType = t.Type.Split("__")[1];
                            var sourceType = type.Split("__")[1];

                            if (samplerType == sourceType)
                            {
                                tex = t;
                                break;
                            }
                        }

                        // Loose match for Albedo
                        if (type == "g_DiffuseTexture" || type.Contains("AlbedoMap"))
                        {
                            if (t.Type.Contains("AlbedoMap"))
                            {
                                tex = t;
                                break;
                            }
                        }

                        // Loose match for Normal
                        if (type == "g_BumpmapTexture" || type.Contains("NormalMap"))
                        {
                            if (t.Type.Contains("NormalMap"))
                            {
                                tex = t;
                                break;
                            }
                        }
                    }
                }

                if (tex == null || tex.Path == "")
                {
                    //Smithbox.Log(this, $"{Context?.QualifiedName}: Failed to find MATBIN string: {mtdstring} - {type}");
                    return;
                }

                path = tex.Path;
                resolvedType = tex.Type;
                //Smithbox.Log(this, $"{Context?.QualifiedName}: MATBIN: {path}");
            }
        }

        // Textures are found by name, in the same pool as this model
        string textureName = ResourceName.Normalize(path);

        // TEMPORARY BB texture diagnostics (BbTextureDiag)
        if (dest.DiagTag != null)
        {
            BbTextureDiag.TracedTextures.TryAdd(textureName, 0);
            BbTextureDiag.Write($"LOOKUP {dest.DiagTag}: slot={textureType} type={type} -> '{textureName}' alreadyFilled={dest.TextureResourceFilled[(int)textureType]} pool={(Context?.Pool ?? ResourceManager.DefaultPool).Name}");
        }

        if (!dest.TextureResourceFilled[(int)textureType])
        {
            var selectedProject = Smithbox.Orchestrator.SelectedProject;
            var handler = selectedProject.Handler;

            //Smithbox.Log(this, $"LISTENER for {virtualPath}");

            // Used to allow for association of models and textures
            if (handler.FocusedEditor is MapEditorScreen)
            {
                var mapEditor = (MapEditorScreen)handler.FocusedEditor;

                var activeView = mapEditor.ViewHandler.ActiveView;
                if (activeView != null)
                {
                    activeView.ModelInsightTool.UpdateEntry(Context?.Name ?? "", Context?.ContainerPath ?? "", textureName, Flver, material, matbin, mtd);
                }
            }

            (Context?.Pool ?? ResourceManager.DefaultPool).AddListener<TextureResource>(textureName, dest, AccessLevel.AccessGPUOptimizedOnly, (int)textureType);
            dest.TextureResourceFilled[(int)textureType] = true;
            dest.ApplySampler(textureType, resolvedType, type);
        }
        else
        {
            // Smithbox.Log(this, $"{Context?.QualifiedName}: NO LISTENER for {textureName}", Microsoft.Extensions.Logging.LogLevel.Error);
        }
    }

    /// <summary>
    /// What the textures of one material say about how it has to be shaded.
    /// Filled in by looking at every texture of the material, so it describes the material as a whole.
    /// </summary>
    private class MaterialLayers
    {
        /// <summary>Number of texture layers: the highest layer any albedo/normal/specular/shininess texture is on</summary>
        public int LayerCount = 1;

        public bool HasBlendMask;

        // Whether the normals / specular / shininess have more than one layer and so need blending
        public bool BlendNormal;
        public bool BlendSpecular;
        public bool BlendShininess;

        public bool IsBlended => LayerCount > 1 || HasBlendMask;
    }

    private static readonly FlverMaterial.TextureType[] AlbedoLayers =
    {
        FlverMaterial.TextureType.AlbedoTextureResource, FlverMaterial.TextureType.AlbedoTextureResource2,
        FlverMaterial.TextureType.AlbedoTextureResource3, FlverMaterial.TextureType.AlbedoTextureResource4
    };

    private static readonly FlverMaterial.TextureType[] NormalLayers =
    {
        FlverMaterial.TextureType.NormalTextureResource, FlverMaterial.TextureType.NormalTextureResource2,
        FlverMaterial.TextureType.NormalTextureResource3, FlverMaterial.TextureType.NormalTextureResource4
    };

    private static readonly FlverMaterial.TextureType[] SpecularLayers =
    {
        FlverMaterial.TextureType.SpecularTextureResource, FlverMaterial.TextureType.SpecularTextureResource2,
        FlverMaterial.TextureType.SpecularTextureResource3, FlverMaterial.TextureType.SpecularTextureResource4
    };

    private static readonly FlverMaterial.TextureType[] ShininessLayers =
    {
        FlverMaterial.TextureType.ShininessTextureResource, FlverMaterial.TextureType.ShininessTextureResource2,
        FlverMaterial.TextureType.ShininessTextureResource3, FlverMaterial.TextureType.ShininessTextureResource4
    };

    private static readonly FlverMaterial.TextureType[] LayerDetailMasks =
    {
        FlverMaterial.TextureType.LayerDetailMaskTextureResource2, FlverMaterial.TextureType.LayerDetailMaskTextureResource3,
        FlverMaterial.TextureType.LayerDetailMaskTextureResource4
    };

    private static readonly FlverMaterial.TextureType[] DetailStackAlbedos =
    {
        FlverMaterial.TextureType.DetailStackAlbedoTextureResource1, FlverMaterial.TextureType.DetailStackAlbedoTextureResource2,
        FlverMaterial.TextureType.DetailStackAlbedoTextureResource3, FlverMaterial.TextureType.DetailStackAlbedoTextureResource4,
        FlverMaterial.TextureType.DetailStackBackAlbedoTextureResource
    };

    private static readonly FlverMaterial.TextureType[] DetailStackNormals =
    {
        FlverMaterial.TextureType.DetailStackNormalTextureResource1, FlverMaterial.TextureType.DetailStackNormalTextureResource2,
        FlverMaterial.TextureType.DetailStackNormalTextureResource3, FlverMaterial.TextureType.DetailStackNormalTextureResource4,
        FlverMaterial.TextureType.DetailStackBackNormalTextureResource
    };

    /// <summary>See <see cref="MaterialBinding.HasKnownBlend"/></summary>
    private static bool HasKnownBlend(FlverMaterial dest) => MaterialBinding.HasKnownBlend(dest.Definition);

    /// <summary>
    /// Loads the textures of a recipe by what the material calls them, as the names
    /// in the flver don't always match, and a recipe can use textures the flver doesn't list.
    /// A path in the flver wins over the material's: terrain shares a material, and sets its own mask per piece.
    /// </summary>
    private void LoadBlendRecipe(FlverMaterial dest, string mtd, MaterialLayers layers, Dictionary<string, string> flverPaths)
    {
        dest.MetallicType = MaterialBinding.ChooseMetallic(dest.Definition, flverPaths);

        // The base textures of a shader whose first texture of a role is an overlay (ER's ChrCustomize skin: beard
        // stubble and burn scars before the skin). Bound first, so the textures loop below keeps them.
        if (dest.Definition?.Slots is { } slots)
        {
            if (slots.AlbedoType != null)
            {
                LookupTexture(FlverMaterial.TextureType.AlbedoTextureResource, dest, slots.AlbedoType, "", mtd);
            }

            if (slots.NormalType != null)
            {
                LookupTexture(FlverMaterial.TextureType.NormalTextureResource, dest, slots.NormalType, "", mtd);
            }

            if (CustomizeMode(dest.Definition) != 0 && slots.MaskType != null)
            {
                LookupTexture(FlverMaterial.TextureType.CustomizeMaskTextureResource, dest, slots.MaskType, "", mtd);
            }

            if (CustomizeMode(dest.Definition) == MaterialCustomize.ModeSkin && slots.StubbleType != null)
            {
                LookupTexture(FlverMaterial.TextureType.CustomizeStubbleTextureResource, dest, slots.StubbleType, "", mtd);
            }

            // The same slot holds a FaceGen skin's body texture (an MTD sampler: its path is the FLVER's)
            if (CustomizeMode(dest.Definition) == MaterialCustomize.ModeFaceGen && slots.BodyType != null)
            {
                LookupTexture(FlverMaterial.TextureType.CustomizeStubbleTextureResource, dest, slots.BodyType,
                    flverPaths.GetValueOrDefault(slots.BodyType, ""), mtd);
            }

            // The same slot holds an eye patch's cloth or leather albedo
            if (CustomizeMode(dest.Definition) == MaterialCustomize.ModeEyePatch && slots.DetailType != null)
            {
                LookupTexture(FlverMaterial.TextureType.CustomizeStubbleTextureResource, dest, slots.DetailType, "", mtd);
            }

            // The same slot holds an eye's iris height, for its parallax
            if (CustomizeMode(dest.Definition) == MaterialCustomize.ModeEye && slots.HeightType != null)
            {
                LookupTexture(FlverMaterial.TextureType.CustomizeStubbleTextureResource, dest, slots.HeightType, "", mtd);
            }

            // The decal's mask, and the decal's tiling (its texture is set at run time, SetCustomizeDecal)
            if (CustomizeMode(dest.Definition) == MaterialCustomize.ModeSkin && slots.Decal is { } decal)
            {
                if (decal.MaskType != null)
                    LookupTexture(FlverMaterial.TextureType.CustomizeDecalMaskTextureResource, dest, decal.MaskType, "", mtd);

                dest.ApplySampler(FlverMaterial.TextureType.CustomizeDecalTextureResource, decal.TextureType);
            }

            // The burn scar's textures (the body's: the shared AAT500_PCBurn maps)
            if (CustomizeMode(dest.Definition) == MaterialCustomize.ModeSkin && slots.Burn is { } burn)
            {
                LookupTexture(FlverMaterial.TextureType.CustomizeBurnTextureResource, dest, burn.AlbedoType, "", mtd);
                LookupTexture(FlverMaterial.TextureType.CustomizeBurnNormalTextureResource, dest, burn.NormalType, "", mtd);
            }

            if (CustomizeMode(dest.Definition) == MaterialCustomize.ModeSkin && slots.Makeup is { } makeup)
            {
                if (makeup.MaskTypes[0] != null)
                    LookupTexture(FlverMaterial.TextureType.CustomizeMakeupMaskTextureResource0, dest, makeup.MaskTypes[0], "", mtd);

                if (makeup.MaskTypes[1] != null)
                    LookupTexture(FlverMaterial.TextureType.CustomizeMakeupMaskTextureResource1, dest, makeup.MaskTypes[1], "", mtd);
            }
        }

        // An MTD's samplers have no paths, so its textures are the FLVER's (Bloodborne's water)
        string FlverPathIfUnnamed(string type) =>
            dest.Definition?.Samplers.Find(x => x.Type == type) is { Path: "" } ? flverPaths.GetValueOrDefault(type, "") : "";

        if (dest.Definition?.SurfaceType != null)
        {
            LookupTexture(dest.Definition.Surface == MaterialSurface.Water
                ? FlverMaterial.TextureType.NormalTextureResource
                : FlverMaterial.TextureType.AlbedoTextureResource, dest, dest.Definition.SurfaceType,
                dest.Definition.Surface == MaterialSurface.Water ? FlverPathIfUnnamed(dest.Definition.SurfaceType) : "", mtd);
        }

        if (dest.Definition?.Fur is { } fur)
        {
            // The material's own path as before (found from ""), or the FLVER's where it has none: an MTD has no paths
            // of its own (DS3's C[Fur])
            string FurPath(string type) =>
                string.IsNullOrEmpty(dest.Definition.Samplers.Find(x => x.Type == type)?.Path) ? flverPaths.GetValueOrDefault(type, "") : "";

            if (fur.BaseType != null)
            {
                LookupTexture(FlverMaterial.TextureType.AlbedoTextureResource, dest, fur.BaseType, FurPath(fur.BaseType), mtd);
            }

            LookupTexture(FlverMaterial.TextureType.AlbedoTextureResource2, dest, fur.StrandType, FurPath(fur.StrandType), mtd);

            if (fur.BaseNormalType != null)
            {
                LookupTexture(FlverMaterial.TextureType.NormalTextureResource, dest, fur.BaseNormalType, FurPath(fur.BaseNormalType), mtd);
            }

            if (fur.StrandNormalType != null)
            {
                LookupTexture(FlverMaterial.TextureType.NormalTextureResource2, dest, fur.StrandNormalType, FurPath(fur.StrandNormalType), mtd);
            }

            // The masked layer and its mask (FlverShader.frag, IsFur), both sampled with the first layer's tiling
            if (fur.MaskType != null && fur.MaskedType != null)
            {
                LookupTexture(FlverMaterial.TextureType.AlbedoTextureResource3, dest, fur.MaskedType, flverPaths.GetValueOrDefault(fur.MaskedType, ""), mtd);
                LookupTexture(FlverMaterial.TextureType.BlendmaskTextureResource, dest, fur.MaskType, flverPaths.GetValueOrDefault(fur.MaskType, ""), mtd);
            }

            // Only the blended shaders have a second UV set
            layers.LayerCount = 2;
        }

        if (dest.Definition?.Water?.DetailType is { } waterDetail)
        {
            LookupTexture(FlverMaterial.TextureType.NormalTextureResource2, dest, waterDetail, FlverPathIfUnnamed(waterDetail), mtd);
        }

        // The snow's finer normal, read with the snow's own tiling (FlverShader.frag, SnowCover)
        if (dest.Definition?.Snow?.DetailType is { } snowDetail)
        {
            LookupTexture(FlverMaterial.TextureType.NormalTextureResource2, dest, snowDetail, flverPaths.GetValueOrDefault(snowDetail, ""), mtd);
        }

        // And the height its cover is found from, or that DS1R's water waves by (FlverShader.frag, LayeredHeight)
        if ((dest.Definition?.Snow?.HeightType ?? dest.Definition?.ReflectWater?.HeightType) is { } layeredHeight)
        {
            LookupTexture(FlverMaterial.TextureType.NormalTextureResource3, dest, layeredHeight, flverPaths.GetValueOrDefault(layeredHeight, ""), mtd);
        }

        if (dest.Definition?.Magma is { } magma)
        {
            LookupTexture(FlverMaterial.TextureType.MagmaTextureResource, dest, magma.TextureType, "", mtd);
        }

        MaterialResolvedDetail? detail = dest.Definition?.Detail;

        if (detail?.AlbedoType != null)
        {
            LookupTexture(FlverMaterial.TextureType.DetailAlbedoTextureResource, dest, detail.AlbedoType, flverPaths.GetValueOrDefault(detail.AlbedoType, ""), mtd);
        }

        if (detail?.NormalType != null)
        {
            LookupTexture(FlverMaterial.TextureType.DetailNormalTextureResource, dest, detail.NormalType, flverPaths.GetValueOrDefault(detail.NormalType, ""), mtd);
        }

        // A detail stack's layers, each tiled by its albedo's sampler (its normal shares the UV group), and the weight map
        // and the back face's in the fourth entry
        if (detail is { HasStackEntries: true })
        {
            if (detail.StackWeightType is { } weightType)
            {
                LookupTexture(FlverMaterial.TextureType.DetailStackWeightTextureResource, dest, weightType, flverPaths.GetValueOrDefault(weightType, ""), mtd);
            }

            var entries = detail.Stack.Select(x => (x.AlbedoType, x.NormalType)).ToList();
            while (entries.Count < MaterialResolvedDetail.MaxStackLayers) entries.Add((null, null));
            entries.Add((detail.BackAlbedoType, detail.BackNormalType));

            for (var i = 0; i < entries.Count; i++)
            {
                var (albedoType, normalType) = entries[i];

                if (albedoType != null)
                {
                    LookupTexture(DetailStackAlbedos[i], dest, albedoType, flverPaths.GetValueOrDefault(albedoType, ""), mtd);
                }

                if (normalType != null)
                {
                    LookupTexture(DetailStackNormals[i], dest, normalType, flverPaths.GetValueOrDefault(normalType, ""), mtd);
                }

                var tiled = albedoType ?? normalType;
                dest.DetailStack[i].Tiling = dest.Definition!.Samplers.Find(x => x.Type == tiled)?.Tiling is { Length: >= 2 } t
                    ? new Vector2(t[0], t[1]) : Vector2.One;
            }
        }

        MaterialResolvedBlend? blend = dest.Definition?.Blend;

        if (blend == null)
        {
            return;
        }

        List<MaterialBlendLayer> blendLayers = dest.Definition!.BlendLayers;

        for (var i = 0; i < blendLayers.Count; i++)
        {
            if (blendLayers[i].Albedo != null)
            {
                LookupTexture(AlbedoLayers[i], dest, blendLayers[i].Albedo, flverPaths.GetValueOrDefault(blendLayers[i].Albedo!, ""), mtd);
                dest.LayerAlbedoTypes[i] ??= blendLayers[i].Albedo;
            }
            else if (blendLayers[i].ColorOnly && i < Material.MaxLayers)
            {
                // White, for the layer's tint to color (UpdateMaterial)
                dest.ColorOnlyLayers[i] = true;
            }

            if (blendLayers[i].Normal != null)
            {
                LookupTexture(NormalLayers[i], dest, blendLayers[i].Normal, flverPaths.GetValueOrDefault(blendLayers[i].Normal!, ""), mtd);
                layers.BlendNormal = true;
            }
        }

        layers.LayerCount = blendLayers.Count;

        if (blend.MaskType != null)
        {
            LookupTexture(FlverMaterial.TextureType.BlendmaskTextureResource, dest, blend.MaskType, flverPaths.GetValueOrDefault(blend.MaskType, ""), mtd);
        }

        if (blend.DetailType != null)
        {
            LookupTexture(FlverMaterial.TextureType.DetailMaskTextureResource, dest, blend.DetailType, flverPaths.GetValueOrDefault(blend.DetailType, ""), mtd);
        }

        // The extended form's detail mask of each layer, and its tiling (MaterialLayerDetailExtra.zw)
        for (var i = 0; i < MaterialResolvedBlend.MaxWeighted; i++)
        {
            var type = blend.LayerDetailTypes[i];
            if (type == null)
            {
                continue;
            }

            LookupTexture(LayerDetailMasks[i], dest, type, flverPaths.GetValueOrDefault(type, ""), mtd);
            dest.LayerDetailTiling[i] = dest.Definition.Samplers.Find(x => x.Type == type)?.Tiling is { Length: >= 2 } t
                ? new Vector2(t[0], t[1]) : Vector2.One;
        }
    }

    /// <summary>
    /// Loads one of the material's own textures where <see cref="MaterialBinding.Route"/> says it goes, and notes
    /// what the shader needs to know about it. Textures loaded from the definition beforehand need nothing more.
    /// </summary>
    private void ProcessMaterialTexture(FlverMaterial dest, string texType, string mpath, string mtd,
        MaterialLayers layers)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;
        // The Transitional family, whose shininess row the shader reads as roughness (RenderProfile.GlossInSpecular)
        var glossInSpecular = RenderProfile.Of(curProject).GlossInSpecular;

        MaterialTextureRoute route = MaterialBinding.Route(dest.Definition, texType, dest.MetallicType, glossInSpecular);
        var layer = route.Layer;

        // TEMPORARY BB texture diagnostics (BbTextureDiag)
        if (dest.DiagTag != null)
        {
            BbTextureDiag.Write($"ROUTE {dest.DiagTag}: {texType} -> use={route.Use} layer={layer} elsewhere={route.LoadedElsewhere} path='{mpath}'");
        }

        if (route.LoadedElsewhere)
        {
            if (route.Use == MaterialTextureUse.Lightmap)
            {
                dest.UsesLightmap = true;
            }

            return;
        }

        switch (route.Use)
        {
            case MaterialTextureUse.Albedo:
                LookupTexture(AlbedoLayers[layer], dest, texType, mpath, mtd);
                layers.LayerCount = Math.Max(layers.LayerCount, layer + 1);
                // The first is the one loaded (LookupTexture keeps a filled slot)
                dest.LayerAlbedoTypes[layer] ??= texType;
                break;

            case MaterialTextureUse.Normal:
                LookupTexture(NormalLayers[layer], dest, texType, mpath, mtd);

                // The layers of a known blend are counted by their albedos
                if (route.FromKnownLayers)
                {
                    layers.BlendNormal = true;
                }
                else
                {
                    layers.LayerCount = Math.Max(layers.LayerCount, layer + 1);
                    layers.BlendNormal |= layer > 0;
                }
                break;

            case MaterialTextureUse.Specular:
                LookupTexture(SpecularLayers[layer], dest, texType, mpath, mtd);
                layers.LayerCount = Math.Max(layers.LayerCount, layer + 1);
                layers.BlendSpecular |= layer > 0;
                break;

            case MaterialTextureUse.Shininess:
                LookupTexture(ShininessLayers[layer], dest, texType, mpath, mtd);
                layers.LayerCount = Math.Max(layers.LayerCount, layer + 1);
                layers.BlendShininess |= layer > 0;

                // DSR's specular maps (DsrSpecularLayout) are read by the shader from the specular row, where the
                // black default would otherwise be
                if (DsrSpecularLayout(dest.Definition, glossInSpecular) != 0 &&
                    texType.Contains("Specular", StringComparison.OrdinalIgnoreCase))
                {
                    LookupTexture(SpecularLayers[layer], dest, texType, mpath, mtd);
                    layers.BlendSpecular |= layer > 0;
                }
                break;

            case MaterialTextureUse.Metallic:
                LookupTexture(SpecularLayers[layer], dest, texType, mpath, mtd);
                dest.UsesMetallic = true;
                dest.MetallicOffsetBits[layer] = BitConverter.HalfToUInt16Bits((Half)(dest.Definition?.MetallicOffsets.GetValueOrDefault(texType) ?? 0));
                layers.BlendSpecular |= layer > 0;
                break;

            case MaterialTextureUse.Emissive:
                LookupTexture(FlverMaterial.TextureType.EmissiveTextureResource, dest, texType, mpath, mtd);
                break;

            case MaterialTextureUse.BlendMask:
                LookupTexture(FlverMaterial.TextureType.BlendmaskTextureResource, dest, texType, mpath, mtd);
                layers.HasBlendMask = true;
                break;

            case MaterialTextureUse.Opacity:
                // Their tiling lands in MaterialLayerTiling.MaskTiling (xy, zw), see FlverMaterial.OpacityFadeUvIndex
                LookupTexture(route.Layer == 0 ? FlverMaterial.TextureType.BlendmaskTextureResource
                    : FlverMaterial.TextureType.DetailMaskTextureResource, dest, texType, mpath, mtd);
                dest.HasOpacity = true;
                break;
        }
    }

    /// <summary>
    /// Fills in the values a material sets. The alpha reference is 0 to 255 in the file, and materials
    /// that don't set one only have an alpha test if they are in the "edge" blend mode of the older games.
    /// </summary>
    private static void ApplyDefinitionShading(FlverMaterial dest, MaterialDefinition definition)
    {
        var alphaRef = definition.AlphaRef > 0
            ? definition.AlphaRef / 255f
            : definition.BlendMode == MaterialDefinition.BlendModeEdge ? MaterialShading.DefaultAlphaRef : 0;

        // A material with no albedo texture but a color is that color. Without the color it stays the default gray
        MaterialSampler? colorOnly = definition.Samplers.Find(x => x.Role == "AlbedoMap" && x.Color != null);
        var hasAlbedo = dest.TextureResourceFilled[(int)FlverMaterial.TextureType.AlbedoTextureResource];

        if (!hasAlbedo && colorOnly != null)
        {
            dest.AlbedoFromColor = true;
            dest.AlbedoTint = Vector3.Min(new Vector3(colorOnly.Color![0], colorOnly.Color[1], colorOnly.Color[2]), Vector3.One);
        }
        else if (!hasAlbedo && definition.Detail?.BaseColor is { } baseColor)
        {
            dest.AlbedoFromColor = true;
            dest.AlbedoTint = Vector3.Min(new Vector3(baseColor[0], baseColor[1], baseColor[2]), Vector3.One);
        }
        else if (CustomizeMode(definition) is var mode and not 0 && definition.Slots is { Color: { } customizeColor } customize)
        {
            // A ChrCustomize colouring: the shader makes the albedo from the colour (hair) or re-tints the texture to it
            // (skin), so the colour goes in the customize entry, not the tint
            dest.AlbedoFromColor = !hasAlbedo;
            dest.Customize = mode switch
            {
                MaterialCustomize.ModeEye => new MaterialCustomize
                {
                    Color = new Vector4(customizeColor[0], customizeColor[1], customizeColor[2], customize.Cataract),
                    Params = new Vector4(customize.Iris[0], customize.Iris[1], customize.Iris[2], customize.Pupil),
                    MaskTiling = dest.CustomizeMaskTiling
                },
                MaterialCustomize.ModeBodyHair => new MaterialCustomize
                {
                    Color = new Vector4(customizeColor[0], customizeColor[1], customizeColor[2], 0),
                    Params = new Vector4(customize.Density, 0, 0, mode),
                    MaskTiling = dest.CustomizeMaskTiling
                },
                MaterialCustomize.ModeSkin => new MaterialCustomize
                {
                    Color = new Vector4(customizeColor[0], customizeColor[1], customizeColor[2], 0),
                    Params = new Vector4(customize.StubbleAmount, 1, 1, mode),
                    MaskTiling = dest.CustomizeMaskTiling
                },
                // yz: the body texture's tiling, filled in by the upload
                // x: 1 if the colour is only added where the albedo's alpha is (an eye)
                MaterialCustomize.ModeFaceGen => new MaterialCustomize
                {
                    Color = new Vector4(customizeColor[0], customizeColor[1], customizeColor[2], 0),
                    Params = new Vector4(customize.AlphaMasked ? 1 : 0, 1, 1, mode),
                    MaskTiling = Vector2.One
                },
                // yz: the detail's tiling, filled in by the upload
                MaterialCustomize.ModeEyePatch => new MaterialCustomize
                {
                    Color = new Vector4(customizeColor[0], customizeColor[1], customizeColor[2], 0),
                    Params = new Vector4(0, 1, 1, mode),
                    MaskTiling = dest.CustomizeMaskTiling
                },
                _ => new MaterialCustomize
                {
                    Color = new Vector4(customizeColor[0], customizeColor[1], customizeColor[2], customize.RootBlack),
                    Params = new Vector4(customize.WhiteDensity, customize.RootChannel, customize.RootMasked ? 1 : 0, mode),
                    MaskTiling = dest.CustomizeMaskTiling
                }
            };
            dest.CustomizeExtra = mode == MaterialCustomize.ModeEye
                ? new MaterialCustomizeExtra
                {
                    A = new Vector4(customize.CataractColor[0], customize.CataractColor[1], customize.CataractColor[2], customize.IrisScale),
                    // y the iris parallax depth; zw its height map's tiling, filled in by the upload
                    B = new Vector4(-1, customize.HeightDepth, 1, 1)
                }
                : mode == MaterialCustomize.ModeSkin && customize.Decal is { } decal
                    ? new MaterialCustomizeExtra
                    {
                        // The decal: colour and scale, offset, angle and mirror; C (the tilings) is filled in by the upload
                        A = new Vector4(decal.Color[0], decal.Color[1], decal.Color[2], decal.Scale),
                        B = new Vector4(decal.Offset[0], decal.Offset[1], decal.Angle, decal.Mirror ? -1 : 1),
                        C = Vector4.One
                    }
                    : mode == MaterialCustomize.ModeSkin && customize.Burn is { } burn
                        // The burn scar's amount (B.y); C (the tilings) is filled in by the upload
                        ? MaterialCustomizeExtra.Default with { B = new Vector4(-1, burn.Amount, 0, 0), C = Vector4.One }
                        : MaterialCustomizeExtra.Default with { B = new Vector4(customize.Gloss, 0, 0, 0) };
            dest.CustomizeMakeup = customize.Makeup?.Data ?? MaterialMakeup.Default;

            // An eye patch's colour (color_0) is the texture input of its albedo too, so the sampler tinted the whole
            // patch by it. The shader applies it, only where the mask says
            if (mode == MaterialCustomize.ModeEyePatch)
            {
                dest.AlbedoTint = Vector3.One;
            }
        }
        else if (!hasAlbedo && definition.Slots?.Color is { } slotColor)
        {
            // ChrCustomize hair-type shaders: the albedo is a material color (MaterialSlotRecipe.Color)
            dest.AlbedoFromColor = true;
            dest.AlbedoTint = Vector3.Min(new Vector3(slotColor[0], slotColor[1], slotColor[2]), Vector3.One);
        }

        // Their cut-out is the normal map's alpha, tested against the recipe's value where the material sets none
        if (definition.Slots is { AlphaFromNormal: true, AlphaRef: { } slotAlphaRef } && alphaRef == 0)
        {
            alphaRef = slotAlphaRef;
        }

        // A recipe tints each of its layers instead
        var tint = definition.Blend != null ? Vector3.One : dest.AlbedoTint;
        var color = new Vector3(definition.DiffuseColor[0], definition.DiffuseColor[1], definition.DiffuseColor[2]) * tint;
        dest.Shading.DiffuseAlpha = new Vector4(color, alphaRef);

        dest.Blending = definition.BlendMode switch
        {
            MaterialDefinition.BlendModeAlpha => MaterialBlending.Alpha,
            MaterialDefinition.BlendModeAdd => MaterialBlending.Add,
            MaterialDefinition.BlendModeSubtract => MaterialBlending.Subtract,
            _ => MaterialBlending.Opaque
        };

        // A recipe that knows the shader's blending better than its g_BlendMode says (ER's [EyeAO]: multiply)
        if (definition.Slots?.Blending is { } recipeBlending)
        {
            dest.Blending = recipeBlending;
        }

        // DS1R's water lets the scene through, which its g_BlendMode (3, water) doesn't say
        if (definition.ReflectWater != null)
        {
            dest.Blending = MaterialBlending.Alpha;
        }

        dest.DepthWrite = dest.Blending == MaterialBlending.Opaque && !definition.DisableDepthWrite;
        dest.MeshDecal = definition.MeshDecal;
    }

    /// <summary>
    /// Picks the shader, vertex layout and shader constants for a material from what its textures need
    /// </summary>
    private static void ConfigureMaterialShader(FlverMaterial dest, MaterialLayers layers)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        var deformer = dest.Definition?.Deformer ?? MaterialDeformer.None;
        var knownBlend = HasKnownBlend(dest);
        var hasRecipe = dest.Definition?.Blend != null;
        var hasDetail = dest.Definition?.Detail != null;
        var hasMagma = dest.Definition?.Magma != null;
        var hasWater = dest.Definition?.Water != null;
        MaterialResolvedFur? fur = dest.Definition?.Fur;

        if (fur != null)
        {
            dest.AlbedoTint = new Vector3(fur.Color[0], fur.Color[1], fur.Color[2]);
            dest.Effects.EmissiveColor.W = fur.DitherWidth;
        }

        if (dest.Definition != null)
        {
            ApplyDefinitionShading(dest, dest.Definition);
        }

        // The shader multiplies the strands by 4.55, as a base averages out to 1 / 4.55 once it is under them.
        // Without one the strands should come out as they are, so a white texture has to be brought down to that
        if (fur is { BaseType: null })
        {
            dest.AlbedoFromColor = true;
            dest.Shading.DiffuseAlpha *= new Vector4(new Vector3(1 / 4.55f), 1);
        }

        // Also from the material's own samplers, in case the model doesn't list the lightmaps it has no paths for
        dest.UsesLightmap |= dest.Definition?.Samplers.Exists(s =>
            s.Type != null && s.Type.Contains("DOLTexture", StringComparison.OrdinalIgnoreCase)) == true;

        if (deformer != MaterialDeformer.None || knownBlend)
        {
            dest.ShaderName = layers.IsBlended ? @"FlverShader\FlverShader_deform_blend" : @"FlverShader\FlverShader_deform";
            dest.LayoutType = MeshLayoutType.LayoutDeform;
        }
        else if (dest.DesLightmapThirdUv && !layers.HasBlendMask)
        {
            // DES [ML]: two layers on UV 1 and 2, the lightmap on UV 3 (LookupDesLightmap)
            dest.ShaderName = @"FlverShader\FlverShader_blend_uv3";
            dest.LayoutType = MeshLayoutType.LayoutUV3;
        }
        else if (dest.HasOpacity && OpacityUvSets(dest.Definition) is { } opacitySets)
        {
            // An opacity blend (FlverMaterial.OpacityFadeUvIndex): its second opacity on a third UV slot, its fade
            // in the vertex alpha
            dest.ShaderName = @"FlverShader\FlverShader_blend_uv3";
            dest.LayoutType = MeshLayoutType.LayoutUV3;
            (dest.OpacityUvIndex, dest.OpacityFadeUvIndex) = opacitySets;
        }
        else if (layers.HasBlendMask && dest.Definition?.MaskBlend is { } maskBlend)
        {
            // A GXFlver mask blend: the game's weight (FlverShader.frag, MATERIAL_MASK_BLEND), its mask on a UV set of
            // its own (the fourth of M[ARSN]_l_blend's), which goes in the third slot
            dest.ShaderName = @"FlverShader\FlverShader_blendmask_uv3";
            dest.LayoutType = MeshLayoutType.LayoutUV3;
            dest.MaskBlendUvIndex = maskBlend.MaskUvIndex;
        }
        else if (layers.HasBlendMask)
        {
            dest.ShaderName = @"FlverShader\FlverShader_blendmask";
            dest.LayoutType = MeshLayoutType.LayoutUV2;
        }
        else if (layers.IsBlended)
        {
            dest.ShaderName = @"FlverShader\FlverShader_blend";
            dest.LayoutType = MeshLayoutType.LayoutUV2;
        }
        else if ((CustomizeMode(dest.Definition) == MaterialCustomize.ModeSkin &&
                  dest.Definition?.Slots is { Decal: not null } or { Burn: not null }) ||
                 CustomizeMode(dest.Definition) == MaterialCustomize.ModeFaceGen)
        {
            // (A FaceGen skin's body texture is on the second UV set too)
            // A skin's decal or burn scar is on the second UV set: the blend variant for it, with one layer (as a lightmap)
            dest.ShaderName = @"FlverShader\FlverShader_blend";
            dest.LayoutType = MeshLayoutType.LayoutUV2;
        }
        else if (dest.UsesLightmap && CFG.Current.Viewport_Map_Lightmaps)
        {
            // The blend variant for its second UV set, the lightmap's, with one layer, so nothing is blended
            dest.ShaderName = @"FlverShader\FlverShader_blend";
            dest.LayoutType = MeshLayoutType.LayoutUV2;
        }
        else
        {
            dest.ShaderName = @"FlverShader\FlverShader";
            dest.LayoutType = MeshLayoutType.LayoutStandard;
        }

        List<SpecializationConstant> specConstants = new();
        specConstants.Add(new SpecializationConstant(0, (uint)curProject.Descriptor.ProjectType));
        if (layers.IsBlended)
        {
            specConstants.Add(new SpecializationConstant(1, layers.BlendNormal));
            specConstants.Add(new SpecializationConstant(2, layers.BlendSpecular));
            specConstants.Add(new SpecializationConstant(3, layers.BlendShininess));
            specConstants.Add(new SpecializationConstant(4, (uint)Math.Clamp(layers.LayerCount, 2, Material.MaxLayers)));
            specConstants.Add(new SpecializationConstant(5, knownBlend && !hasRecipe));
            specConstants.Add(new SpecializationConstant(7, hasRecipe));
        }
        else if (dest.ShaderName == @"FlverShader\FlverShader_blend")
        {
            // A lightmapped material drawn with the blend variant: one layer, which then takes all the weight
            specConstants.Add(new SpecializationConstant(4, 1u));
        }

        specConstants.Add(new SpecializationConstant(6, dest.Blending != MaterialBlending.Opaque));
        // How it blends, so a transparent material fogs itself as its blending needs (an additive one only dims)
        specConstants.Add(new SpecializationConstant(11, (uint)dest.Blending));
        specConstants.Add(new SpecializationConstant(8, hasDetail));
        specConstants.Add(new SpecializationConstant(9, hasMagma));
        specConstants.Add(new SpecializationConstant(10, (uint)(dest.Definition?.Surface ?? MaterialSurface.Standard)));
        // Metallic shading for the games that use it, unless the material has an old specular color map instead.
        // Without a metallic map the default black one makes it a non-metal, which reflects 4% as in these games
        var metallicGame = RenderProfile.Of(curProject).Metallic;
        dest.MetallicShading = metallicGame &&
            (dest.UsesMetallic || !dest.TextureResourceFilled[(int)FlverMaterial.TextureType.SpecularTextureResource]);
        specConstants.Add(new SpecializationConstant(12, dest.MetallicShading));

        // The cut-out from the normal map's alpha (MaterialSlotRecipe.AlphaFromNormal), only where a normal is bound
        specConstants.Add(new SpecializationConstant(13, dest.Definition?.Slots?.AlphaFromNormal == true &&
            dest.TextureResourceFilled[(int)FlverMaterial.TextureType.NormalTextureResource]));
        // A ChrCustomize colouring (MaterialCustomize), read from the entry after the effects
        specConstants.Add(new SpecializationConstant(14, dest.Customize != null ? CustomizeMode(dest.Definition) : 0u));
        // A "Col" shader (MaterialDefinition.VertexColor): 1 the diffuse times the vertex color, 2 also the opacity times
        // its alpha, for an alpha blended material that doesn't blend layers (whose alpha is then a layer weight)
        var vertexColor = dest.Definition?.VertexColor == true
            ? dest.Blending == MaterialBlending.Alpha && !layers.IsBlended ? 2u : 1u
            : 0u;
        specConstants.Add(new SpecializationConstant(15, vertexColor));
        // How a DSR material's specular map is read (DsrSpecularLayout), and its specular color's scale
        var dsrSpecular = DsrSpecularLayout(dest.Definition, RenderProfile.Of(curProject).GlossInSpecular);
        specConstants.Add(new SpecializationConstant(16, (uint)dsrSpecular));
        specConstants.Add(new SpecializationConstant(17, dsrSpecular != 0 ? dest.Definition!.SpecularMapScale : 1f));
        // The UVs are stored at 1024 rather than 2048 (FlverMaterial.UvPackScale)
        specConstants.Add(new SpecializationConstant(51, dest.UvPackScale == 1024f));
        specConstants.Add(new SpecializationConstant(60, (uint)deformer));

        dest.SpecializationConstants ??= new List<SpecializationConstant>();
        dest.SpecializationConstants.AddRange(specConstants);
        dest.VertexLayout = MeshLayoutUtils.GetLayoutDescription(dest.LayoutType);
        dest.VertexSize = MeshLayoutUtils.GetLayoutVertexSize(dest.LayoutType);

        // The first layer reads the first UV set. The layers blended onto it read the second,
        // which for meshes with only one UV set is a copy of the first.
        dest.MaterialData.uvSet = 0;
        dest.MaterialData.uvSet2 = 1;
        dest.MaterialData.uvSet3 = 1;
        dest.MaterialData.uvSet4 = 1;

        // These shaders read every layer through the first UV set
        if (knownBlend)
        {
            dest.MaterialData.uvSet2 = 0;
            dest.MaterialData.uvSet3 = 0;
            dest.MaterialData.uvSet4 = 0;
        }

        if (fur != null)
        {
            // The strands (layer 2) on the third UV set, which sits in the second slot, or on the first. The base
            // (layer 1) on the first, or on the second where the recipe says (DS3's Fur_FDP)
            dest.MaterialData.uvSet = (ushort)(fur.BaseSecondUv ? 1 : 0);
            dest.MaterialData.uvSet2 = (ushort)(fur.ThirdUv ? 1 : 0);
        }
        else if (dest.Definition != null)
        {
            // Where the game's shader is known to sample a layer's albedo with one UV set (MaterialShaderTable.UvSets),
            // the layer uses that set. M[AMSN_V][Mb4_Ch2] has all of its layers on the second set, and AC6's [MS]
            // blends their second layer on the first. A layer's other textures follow its albedo, as in the game.
            // Only the blend shaders have a second set; the others read the first for both.
            for (var i = 0; i < Material.MaxLayers; i++)
            {
                var type = dest.LayerAlbedoTypes[i];
                var set = type == null ? -1 : dest.Definition.Samplers.Find(x => x.Type == type)?.UvSet ?? -1;

                if (set < 0)
                {
                    continue;
                }

                switch (i)
                {
                    case 0: dest.MaterialData.uvSet = (ushort)set; break;
                    case 1: dest.MaterialData.uvSet2 = (ushort)set; break;
                    case 2: dest.MaterialData.uvSet3 = (ushort)set; break;
                    default: dest.MaterialData.uvSet4 = (ushort)set; break;
                }
            }
        }

        if (deformer != MaterialDeformer.None)
        {
            dest.Deform = BuildMaterialDeform(dest.Definition!);
        }

        if (hasRecipe)
        {
            dest.ApplyBlend(dest.Definition!.Blend!);
        }

        if (hasDetail)
        {
            dest.ApplyDetail(dest.Definition!.Detail!);
        }

        if (hasMagma)
        {
            dest.ApplyMagma(dest.Definition!.Magma!);
        }

        if (hasWater)
        {
            dest.ApplyWater(dest.Definition!.Water!);
        }

        if (dest.Definition?.Snow is { } snow)
        {
            dest.ApplySnow(snow);
        }

        if (dest.Definition?.ReflectWater is { } reflectWater)
        {
            dest.ApplyReflectWater(reflectWater);
        }

        // Upload the defaults so a material whose textures never arrive still shows something sensible
        dest.UpdateMaterial();
    }

    /// <summary>
    /// The ChrCustomize colouring a material is drawn with (<see cref="MaterialCustomize.ModeHair"/>,
    /// <see cref="MaterialCustomize.ModeSkin"/>), or 0. Its entry sits where a deform entry would, and before the blend,
    /// overlay, glow and water entries, so a material with any of those (none of the ChrCustomize shaders) has none.
    /// </summary>
    /// <summary>
    /// A texture path that only holds a slot: LiveTokyo's dummy128 (not shipped in any game's data) or a SysTex dummy
    /// (SYSTEX_DummyNormal and the like, flat values). Survey: Claude/Tools/Ds3MapPieceProbe placeholders. An empty path
    /// isn't one, as a MATBIN can fill it
    /// </summary>
    public static bool IsPlaceholderTexture(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path.Replace('\\', '/'));
        return name.Equals("dummy128", StringComparison.OrdinalIgnoreCase) ||
               name.StartsWith("SYSTEX_Dummy", StringComparison.OrdinalIgnoreCase);
    }

    public static uint CustomizeMode(MaterialDefinition? definition)
    {
        if (definition?.Slots is not { Customize: not 0 and var mode } ||
            definition.Deformer != MaterialDeformer.None ||
            definition.Blend != null || definition.Detail != null || definition.Magma != null ||
            definition.Water != null || definition.Fur != null)
        {
            return 0;
        }

        return mode;
    }

    /// <summary>
    /// The resolved form of the MTD or MATBIN a FLVER material points at, if the project has it
    /// </summary>
    private static MaterialDefinition? GetMaterialDefinition(string mtd)
    {
        var bank = Smithbox.Orchestrator.SelectedProject?.Handler?.MaterialData?.PrimaryBank;

        return bank?.GetDefinition(Path.GetFileNameWithoutExtension(mtd.Replace('\\', Path.DirectorySeparatorChar)));
    }

    /// <summary>
    /// A material takes three entries of the material buffer, four with a deformer, or eight with a blend recipe.
    /// See <see cref="MaterialShading"/>, <see cref="MaterialEffects"/>, <see cref="MaterialDeform"/> and <see cref="MaterialWithBlend"/>
    /// </summary>
    private static unsafe GPUBufferAllocator.GPUBufferHandle AllocateMaterialBuffer(FlverMaterial dest)
    {
        var size = dest.Definition?.Deformer is null or MaterialDeformer.None
            ? sizeof(MaterialWithShading)
            : sizeof(MaterialWithDeform);

        // The customize entry takes the deform entry's place (CustomizeMode rules out the materials that have one)
        if (CustomizeMode(dest.Definition) != 0)
        {
            size = sizeof(MaterialWithCustomize);
        }

        if (dest.Definition?.Blend != null)
        {
            size = sizeof(MaterialWithBlend);
        }

        if (dest.Definition?.Detail != null)
        {
            size = sizeof(MaterialWithDetail);
        }

        if (dest.Definition?.Magma != null)
        {
            size = sizeof(MaterialWithMagma);
        }

        // Snow and DS1R's water have water's entries (MaterialSnow, MaterialReflectWater)
        if (dest.Definition?.Water != null || dest.Definition?.Snow != null || dest.Definition?.ReflectWater != null)
        {
            size = sizeof(MaterialWithWater);
        }

        if (dest.Definition?.Blend is { Extended: true } or { Adjusted: true })
        {
            size = sizeof(MaterialWithLayerDetail);
        }

        if (dest.Definition?.Detail?.HasStackEntries == true)
        {
            size = sizeof(MaterialWithDetailStack);
        }

        // A blend mask the FLVER tiles, on a material without a recipe: the blend entries for its tiling (Material.MaskTilingFlag).
        // Fur and the ChrCustomize colourings use the mask slot or the deform entry for other things
        // A GXFlver mask blend keeps its intensity in them as well (FlverMaterial.MaskBlendUvIndex)
        if (dest.Definition?.Blend == null && dest.Definition?.Fur == null && CustomizeMode(dest.Definition) == 0 &&
            (dest.Definition?.MaskBlend != null || dest.HasOpacity ||
             dest.FlverTiling.Keys.Any(x => x.Contains("BlendMask", StringComparison.OrdinalIgnoreCase))))
        {
            dest.MaskTilingEntries = true;
            size = Math.Max(size, sizeof(MaterialWithBlend));
        }

        return SceneRenderer.MaterialBufferAllocator.Allocate((uint)size, sizeof(Material));
    }

    /// <summary>
    /// Starts loading the textures of a vertex animation. Nothing in <see cref="ProcessMaterialTexture"/>
    /// recognizes them, so the shader table says which they are.
    /// </summary>
    private void LoadDeformTextures(FlverMaterial dest, string mtd)
    {
        if (dest.Definition?.Deformer is not (MaterialDeformer.Vat or MaterialDeformer.VatLoop
            or MaterialDeformer.Birds or MaterialDeformer.DirectionalWave))
            return;

        if (dest.Definition.DeformerTextures.TryGetValue("VatPosition", out var position))
        {
            LookupTexture(FlverMaterial.TextureType.VatPositionTextureResource, dest, null, position, mtd);
        }

        if (dest.Definition.DeformerTextures.TryGetValue("VatNormal", out var normal))
        {
            LookupTexture(FlverMaterial.TextureType.VatNormalTextureResource, dest, null, normal, mtd);
        }
    }

    private static float GetDeformValue(MaterialDefinition definition, string role, float fallback)
    {
        return definition.DeformerParams.TryGetValue(role, out var value) && value.Length > 0 ? value[0] : fallback;
    }

    private static MaterialDeform BuildMaterialDeform(MaterialDefinition definition)
    {
        MaterialDeform deform = new();

        // Some vertex animations play as they are, with no parameter to scale them
        deform.Amplitude = GetDeformValue(definition, "Amplitude",
            definition.Deformer is MaterialDeformer.Vat or MaterialDeformer.VatLoop or MaterialDeformer.VatFrame ? 1 : 0);
        deform.Rate = GetDeformValue(definition, definition.Deformer switch
        {
            MaterialDeformer.Vat or MaterialDeformer.VatLoop or MaterialDeformer.Birds => "FrameRate",
            MaterialDeformer.WindPivot => "Phase",
            _ => "Speed"
        }, 0);

        if (definition.DeformerParams.TryGetValue("Direction", out var direction) && direction.Length >= 3)
        {
            deform.Direction = new Vector4(direction[0], direction[1], direction[2], GetDeformValue(definition, "Phase", 0));
        }

        if (definition.Deformer == MaterialDeformer.Birds)
        {
            deform.Extra0 = GetDeformValue(definition, "Speed", 0);
            deform.Extra1 = GetDeformValue(definition, "Offset", 0);
            deform.Direction.W = GetDeformValue(definition, "Roll", 0);
            deform.Extra2 = GetDeformValue(definition, "UseSpin", 0) != 0 ? GetDeformValue(definition, "Spin", 0) : 0;
            deform.Extra3 = GetDeformValue(definition, "UseBob", 0) != 0 ? GetDeformValue(definition, "Bob", 0) : 0;
        }

        if (definition.Deformer == MaterialDeformer.DirectionalWave)
        {
            definition.DeformerParams.TryGetValue("Wave", out var wave);
            definition.DeformerParams.TryGetValue("WaveScale", out var waveScale);

            if (wave?.Length >= 3 && waveScale?.Length >= 3)
            {
                deform.Extra2 = wave[0] * (waveScale[0] + 1);
                deform.Direction.W = wave[1] * wave[2];
                deform.Extra3 = wave[2] * (waveScale[2] + 1);
            }

            deform.Extra0 = GetDeformValue(definition, "NoisePhase0", 0) + GetDeformValue(definition, "NoisePhase1", 0);
            deform.Extra1 = GetDeformValue(definition, "NoiseScale", 1);
        }

        return deform;
    }

    /// <summary>
    /// Grows the bounds of a submesh to cover its deformed vertices, so that it isn't culled while on screen.
    /// </summary>
    private static void PadDeformedBounds(FlverSubmesh dest)
    {
        MaterialDefinition? definition = dest.Material?.Definition;

        if (definition == null || definition.Deformer == MaterialDeformer.None)
            return;

        MaterialDeform deform = BuildMaterialDeform(definition);

        // How far a vertex animation moves isn't known without its texture, but the
        // offsets seen so far stay under a world unit, before the amplitude scales them
        var reach = definition.Deformer switch
        {
            MaterialDeformer.Vat or MaterialDeformer.VatLoop => Math.Abs(deform.Amplitude) * 2.0f,

            // A cutscene's cloth and flags move further: up to 4.2 units in ER
            MaterialDeformer.VatFrame => 5.0f,
            MaterialDeformer.Sway => Math.Abs(deform.Amplitude) * deform.Direction.Length(),

            // The whole flight path is around the model, on top of the animation
            MaterialDeformer.Birds => Math.Abs(deform.Amplitude) * new Vector3(deform.Direction.X, deform.Direction.Y, deform.Direction.Z).Length() + 2.0f,

            // Wind depends on the viewport's settings rather than the material
            _ => 1.0f
        };

        Vector3 pad = new(reach);
        dest.Bounds = new BoundingBox(dest.Bounds.Min - pad, dest.Bounds.Max + pad);
    }

    /// <summary>
    /// Elden Ring's body models hold their body hair as extra meshes, such as #03#_hairy and #03#_hairy_plane.
    /// These share their masks with the skin (#03#), so masks can't tell the two apart.
    /// </summary>
    private static bool IsBodyHairMaterial(string? materialName)
    {
        return materialName != null && materialName.Contains("_hairy", StringComparison.OrdinalIgnoreCase);
    }

    private static readonly char[] NameTokenSeparators = ['_', ' ', '-', '#', '.'];

    /// <summary>
    /// Matches whole name tokens only, so m30_00_low_hall_03_a matches, but m30_00_lower_hall does not.
    /// </summary>
    private static bool IsLowDetailName(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var name = Path.GetFileNameWithoutExtension(path.Replace('\\', '/'));

        foreach (var token in name.Split(NameTokenSeparators, StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Equals("low", StringComparison.OrdinalIgnoreCase) ||
                token.Equals("lowpoly", StringComparison.OrdinalIgnoreCase))
                return true;

            // lod, lod1, lod2 and so on
            if (token.StartsWith("lod", StringComparison.OrdinalIgnoreCase) &&
                token.Skip(3).All(char.IsDigit))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Makes every index point at a vertex of the mesh. The GPU reads whatever an index points at,
    /// and an index past the last vertex can read past the end of the vertex buffer and lose the device.
    /// Strips keep their restart values, which the pipeline restarts on. Lists have no restart,
    /// so a triangle with a bad index is collapsed onto its first vertex instead.
    /// </summary>
    private void SanitizeIndices(FlverSubmesh dest, Span<ushort> fs16, Span<int> fs32)
    {
        var vertexCount = dest.VertexCount;
        var fixedCount = 0;

        foreach (var faceset in dest.MeshFacesets)
        {
            if (faceset.IsTriangleStrip)
            {
                for (var i = 0; i < faceset.IndexCount; i++)
                {
                    var offset = faceset.IndexOffset + i;

                    if (faceset.Is32Bit)
                    {
                        if (fs32[offset] != -1 && (uint)fs32[offset] >= (uint)vertexCount)
                        {
                            fs32[offset] = -1;
                            fixedCount++;
                        }
                    }
                    else if (fs16[offset] != 0xFFFF && fs16[offset] >= vertexCount)
                    {
                        fs16[offset] = 0xFFFF;
                        fixedCount++;
                    }
                }

                continue;
            }

            for (var t = 0; t + 2 < faceset.IndexCount; t += 3)
            {
                var offset = faceset.IndexOffset + t;
                var bad = false;

                for (var k = 0; k < 3; k++)
                {
                    var idx = faceset.Is32Bit ? (uint)fs32[offset + k] : fs16[offset + k];
                    bad |= idx >= (uint)vertexCount;
                }

                if (!bad)
                    continue;

                for (var k = 0; k < 3; k++)
                {
                    if (faceset.Is32Bit)
                        fs32[offset + k] = 0;
                    else
                        fs16[offset + k] = 0;
                }

                fixedCount++;
            }
        }

        if (fixedCount > 0)
        {
            Smithbox.Log(this, $"{Context?.QualifiedName}: {fixedCount} triangles or strip indices pointed past the {vertexCount} vertices of their mesh and were removed.", LogLevel.Warning);
        }
    }

    /// <summary>
    /// Adds the full detail facesets of a mesh to the detail stats, which the low detail filter reads.
    /// Called before the index buffer is unmapped, as the indices can't be read back after.
    /// </summary>
    private void AddDetailStats(FlverSubmesh dest, Span<Vector3> positions, Span<ushort> fs16, Span<int> fs32)
    {
        foreach (var faceset in dest.MeshFacesets)
        {
            if (faceset.LOD != 0 || faceset.IsMotionBlur)
                continue;

            // The previous two indices of the strip, or of the current triangle of a list
            int a = -1, b = -1;
            var stripLength = 0;

            for (var i = 0; i < faceset.IndexCount; i++)
            {
                var offset = faceset.IndexOffset + i;
                var c = faceset.Is32Bit ? fs32[offset] : (fs16[offset] == 0xFFFF ? -1 : fs16[offset]);

                if (c < 0 || c >= positions.Length)
                {
                    stripLength = 0;
                    continue;
                }

                stripLength++;

                if (faceset.IsTriangleStrip)
                {
                    if (stripLength >= 3)
                        AddTriangle(positions[a], positions[b], positions[c]);

                    a = b;
                    b = c;
                }
                else
                {
                    if (stripLength == 1)
                        a = c;
                    else if (stripLength == 2)
                        b = c;
                    else
                    {
                        AddTriangle(positions[a], positions[b], positions[c]);
                        stripLength = 0;
                    }
                }
            }
        }
    }

    private void ResetDetailStats()
    {
        DetailTriangleCount = 0;
        DetailSurfaceArea = 0.0f;
        HasLowDetailNaming = false;
    }

    private void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        var area = Vector3.Cross(b - a, c - a).Length() * 0.5f;

        // Strips join with degenerate triangles, which aren't real faces
        if (area <= 0.0f || float.IsNaN(area))
            return;

        DetailTriangleCount++;
        DetailSurfaceArea += area;
    }

    /// <summary>
    /// Keeps a texture's FLVER tiling (<see cref="FlverMaterial.FlverTiling"/>) when it says something: 1,1 is
    /// the default, and 0 in either axis would collapse the texture to one texel.
    /// </summary>
    private static void RecordFlverTiling(FlverMaterial dest, string paramName, Vector2 scale)
    {
        if (scale == Vector2.One || scale.X == 0 || scale.Y == 0 || !float.IsFinite(scale.X) || !float.IsFinite(scale.Y))
            return;

        dest.FlverTiling[paramName] = scale;
    }

    /// <summary>
    /// An opacity blend's FLVER UV sets (FlverMaterial.OpacityFadeUvIndex): OpacityTexture2's, the rank of its MTD UV
    /// number, and the fade's, the one after every number the MTD's textures use. Null without UV numbers
    /// </summary>
    private static (int Second, int Fade)? OpacityUvSets(MaterialDefinition? definition)
    {
        if (definition == null)
        {
            return null;
        }

        var numbers = definition.Samplers.Where(x => x.UvNumber > 0).Select(x => x.UvNumber).Distinct().Order().ToList();
        if (numbers.Count == 0)
        {
            return null;
        }

        MaterialSampler? second = definition.Samplers.Find(x => x.Type?.Contains("OpacityTexture2", StringComparison.OrdinalIgnoreCase) == true);
        var secondSet = second is { UvNumber: > 0 } ? numbers.IndexOf(second.UvNumber) : Math.Min(1, numbers.Count - 1);

        return (secondSet, numbers.Count);
    }

    /// <summary>
    /// The first float of the material's GX01 item (BlendMaskIntensity), 1 where it has none. Every BB mask blend
    /// material has one (0 to 10, 1 the most common), with its other floats 0
    /// </summary>
    private float ReadBlendMaskIntensity(IFlverMaterial mat)
    {
        if (mat is not FLVER2.Material material || Flver == null || material.GXIndex < 0 || material.GXIndex >= Flver.GXLists.Count)
        {
            return 1f;
        }

        FLVER2.GXItem? item = Flver.GXLists[material.GXIndex].Find(x => x.ID == "GX01");
        if (item?.Data is not { Length: >= 4 } data)
        {
            return 1f;
        }

        var value = BitConverter.ToSingle(data, 0);
        return float.IsFinite(value) ? value : 1f;
    }

    private unsafe void ProcessMaterial(IFlverMaterial mat, FlverMaterial dest)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        dest.MaterialName = Path.GetFileNameWithoutExtension(mat.MTD);
        dest.FlverMaterialName = mat.Name;
        dest.Definition = GetMaterialDefinition(mat.MTD);

        // Before the buffer, whose size can depend on it (AllocateMaterialBuffer)
        foreach (IFlverTexture? texture in mat.Textures)
        {
            if (texture is FLVER2.Texture { ParamName: not null } flverTexture)
            {
                RecordFlverTiling(dest, flverTexture.ParamName, flverTexture.TilingScale);
            }
        }

        // An OpacityTexture (MaterialTextureUse.Opacity): known before the buffer, as its tiling is in the blend entries
        dest.HasOpacity = dest.Definition is { Blend: null } &&
            mat.Textures.Any(x => x.ParamName?.Contains("OpacityTexture1", StringComparison.OrdinalIgnoreCase) == true);

        // The GXFlver mask blends' intensity, from the FLVER material's GX01 (BlendMaskIntensity, GX_Item_Descriptors)
        if (dest.Definition?.MaskBlend != null)
        {
            dest.MaskBlendIntensity = ReadBlendMaskIntensity(mat);
        }

        dest.MaterialBuffer = AllocateMaterialBuffer(dest);
        dest.MaterialData = new Material();

        var matName = mat.Name;
        if (!string.IsNullOrWhiteSpace(matName) && matName.Length >= 3 && matName[0] == '#' && char.IsDigit(matName[1]) && char.IsDigit(matName[2]))
        {
            dest.MaterialMask = int.Parse(matName.Substring(1, 2));
        }
        dest.IsBodyHair = IsBodyHairMaterial(matName);

        if (IsLowDetailName(matName) || IsLowDetailName(mat.MTD) || mat.Textures.Any(x => IsLowDetailName(x.Path)))
        {
            HasLowDetailNaming = true;
        }

        dest.SpecializationConstants = new List<SpecializationConstant>();

        // FLVER0 meshes are placed per mesh in ProcessMesh(FLVER0.Mesh), not per material: DES normals are
        // LayoutType.Color, whose W isn't a node index, and one material can serve both bind-pose and rigid meshes

        if (!CFG.Current.Viewport_Enable_Texturing || ModelsOnly)
        {
            dest.ShaderName = @"SimpleFlver";
            dest.LayoutType = MeshLayoutType.LayoutSky;
            dest.VertexLayout = MeshLayoutUtils.GetLayoutDescription(dest.LayoutType);
            dest.VertexSize = MeshLayoutUtils.GetLayoutVertexSize(dest.LayoutType);
            return;
        }

        var layers = new MaterialLayers();
        var flverPaths = new Dictionary<string, string>();

        foreach (IFlverTexture? matparam in mat.Textures)
        {
            if (matparam.ParamName != null)
            {
                flverPaths[matparam.ParamName] = matparam.Path;
            }
        }

        LoadBlendRecipe(dest, mat.MTD, layers, flverPaths);

        foreach (IFlverTexture? matparam in mat.Textures)
        {
            ProcessMaterialTexture(dest, matparam.ParamName, matparam.Path, mat.MTD, layers);
        }

        if (RenderProfile.Of(curProject).DesMaterials)
        {
            RequestDesTextureContainers(curProject, mat);
            LookupDesLightmap(dest, mat, layers);
        }

        LoadDeformTextures(dest, mat.MTD);
        ConfigureMaterialShader(dest, layers);
        MarkModelLighting(dest);
    }

    /// <summary>
    /// DES models often use textures that live in another model's container, which nothing else would load. The path
    /// of each texture usually still names that container, so it is loaded into this model's pool, textures only.
    /// Textures whose path names the wrong container (c6050's head textures are only in the m06/m08 area TPFs) are
    /// left to <see cref="TextureNameIndex"/>, the last rule of ResourceFallbacks, which runs for every game. The pool
    /// skips containers that are loaded or loading already, so this is cheap when the map has them.
    /// </summary>
    private void RequestDesTextureContainers(ProjectEntry project, IFlverMaterial mat)
    {
        var pool = Context?.Pool ?? ResourceManager.DefaultPool;
        var ownContainer = NormalizeContainerPath(Context?.ContainerPath);

        foreach (IFlverTexture? texture in mat.Textures)
        {
            if (string.IsNullOrWhiteSpace(texture.Path))
                continue;

            foreach (ResourceDescriptor container in TextureLocator.GetDesReferencedTextureContainers(project, texture.Path))
            {
                var path = container.GetContainerPath(project);

                if (path != null && NormalizeContainerPath(path) != ownContainer)
                    RequestDesTextureContainer(pool, path);
            }
        }
    }

    private void RequestDesTextureContainer(ResourcePool pool, string path)
    {
        lock (_requestedTextureContainers)
        {
            if (!_requestedTextureContainers.Add(path))
                return;
        }

        pool.LoadContainer(path, new LoadOptions
        {
            Types = ResourceType.Texture,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            HideFromLoadWindow = true,
            JobName = $"DES textures: {path}"
        });
    }

    /// <summary>A container path compared without its slashes' direction, leading slash, case or .dcx</summary>
    private static string? NormalizeContainerPath(string? path)
    {
        if (path == null)
            return null;

        var p = path.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
        return p.EndsWith(".dcx") ? p[..^4] : p;
    }

    /// <summary>
    /// A DES map piece's g_Lightmap: its static light, a texture of the area TPFs read through the last UV set (the MTDs
    /// give it UV 3, after the diffuse layers'). A single layer material is drawn with the blend variant, whose second
    /// UV set is that one when the mesh has two. A multi texture material ([ML], DS_Phn_*MulLit, 855 meshes) has three
    /// UV sets: layer 1, layer 2 and the lightmap, so it is drawn with the blend_uv3 variant, which carries the third.
    /// </summary>
    private void LookupDesLightmap(FlverMaterial dest, IFlverMaterial mat, MaterialLayers layers)
    {
        if (!CFG.Current.Viewport_Map_Lightmaps)
            return;

        foreach (IFlverTexture? texture in mat.Textures)
        {
            if (texture.ParamName == "g_Lightmap" && !string.IsNullOrWhiteSpace(texture.Path))
            {
                LookupTexture(FlverMaterial.TextureType.LightmapTextureResource, dest, texture.ParamName, texture.Path, mat.MTD);
                dest.UsesLightmap = true;
                dest.DesLightmapThirdUv = layers.IsBlended;
                return;
            }
        }
    }

    // The containers this model has already asked for, so each is requested once per model
    private readonly HashSet<string> _requestedTextureContainers = new(StringComparer.OrdinalIgnoreCase);

    private unsafe void ProcessMaterial(FlverMaterial dest, BinaryReaderEx br,
        ref FlverMaterialDef mat, Span<FlverTexture> textures, bool isUTF)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        var mtd = isUTF ? br.GetUTF16(mat.mtdOffset) : br.GetShiftJIS(mat.mtdOffset);
        var matName = isUTF ? br.GetUTF16(mat.nameOffset) : br.GetShiftJIS(mat.nameOffset);
        dest.MaterialName = Path.GetFileNameWithoutExtension(mtd);
        dest.FlverMaterialName = matName;
        dest.Definition = GetMaterialDefinition(mtd);

        // TEMPORARY BB texture diagnostics (BbTextureDiag)
        if (BbTextureDiag.IsTracedModel(Context?.Name))
        {
            dest.DiagTag = $"{Context?.Name} ({Context?.Pool?.Name}) mat '{matName}' mtd '{dest.MaterialName}'";
            BbTextureDiag.Write($"MATERIAL {dest.DiagTag}: definition={(dest.Definition != null ? "found" : "NULL")} texturing={CFG.Current.Viewport_Enable_Texturing} modelsOnly={ModelsOnly}");
        }
        if (!string.IsNullOrWhiteSpace(matName) && matName.Length >= 3 && matName[0] == '#' && char.IsDigit(matName[1]) && char.IsDigit(matName[2]))
        {
            dest.MaterialMask = int.Parse(matName.Substring(1, 2));
        }
        dest.IsBodyHair = IsBodyHairMaterial(matName);

        // Before the buffer, whose size can depend on it (AllocateMaterialBuffer)
        for (var i = mat.textureIndex; i < mat.textureIndex + mat.textureCount; i++)
        {
            var ttype = isUTF ? br.GetUTF16(textures[i].typeOffset) : br.GetShiftJIS(textures[i].typeOffset);
            RecordFlverTiling(dest, ttype, textures[i].scale);
        }

        dest.MaterialBuffer = AllocateMaterialBuffer(dest);
        dest.MaterialData = new Material();

        // Read before the untextured return, as the naming is needed either way
        if (IsLowDetailName(matName) || IsLowDetailName(mtd))
        {
            HasLowDetailNaming = true;
        }

        for (var i = mat.textureIndex; i < mat.textureIndex + mat.textureCount && !HasLowDetailNaming; i++)
        {
            var tpath = isUTF ? br.GetUTF16(textures[i].pathOffset) : br.GetShiftJIS(textures[i].pathOffset);
            HasLowDetailNaming = IsLowDetailName(tpath);
        }

        // Nothing but placeholders: DS3's M[A] join meshes of m38_00_00_00_005430, whose only texture is dummy128, drew
        // as flat untextured walls (Claude/Documentation/DS3 Carthus Rendering.md). A character-creator material is
        // left alone, as its textures come from the character (ER's FaceGen skin lists only placeholders)
        // A slot whose definition names a texture of its own isn't a placeholder: an ER MATBIN does that for c3950's
        // glowing claws, whose FLVER lists only dummies
        dest.NeverDrawn = mat.textureCount > 0 && CustomizeMode(dest.Definition) == 0;
        for (var i = mat.textureIndex; i < mat.textureIndex + mat.textureCount && dest.NeverDrawn; i++)
        {
            var ttype = isUTF ? br.GetUTF16(textures[i].typeOffset) : br.GetShiftJIS(textures[i].typeOffset);
            var definitionPath = dest.Definition?.Samplers.Find(x => x.Type == ttype)?.Path;
            dest.NeverDrawn = string.IsNullOrEmpty(definitionPath) &&
                IsPlaceholderTexture(isUTF ? br.GetUTF16(textures[i].pathOffset) : br.GetShiftJIS(textures[i].pathOffset));
        }

        if (!CFG.Current.Viewport_Enable_Texturing || ModelsOnly)
        {
            dest.ShaderName = @"SimpleFlver";
            dest.LayoutType = MeshLayoutType.LayoutSky;
            dest.VertexLayout = MeshLayoutUtils.GetLayoutDescription(dest.LayoutType);
            dest.VertexSize = MeshLayoutUtils.GetLayoutVertexSize(dest.LayoutType);
            dest.SpecializationConstants = new List<SpecializationConstant>();
            return;
        }

        var layers = new MaterialLayers();
        var flverPaths = new Dictionary<string, string>();

        for (var i = mat.textureIndex; i < mat.textureIndex + mat.textureCount; i++)
        {
            var ttype = isUTF ? br.GetUTF16(textures[i].typeOffset) : br.GetShiftJIS(textures[i].typeOffset);
            flverPaths[ttype] = isUTF ? br.GetUTF16(textures[i].pathOffset) : br.GetShiftJIS(textures[i].pathOffset);
        }

        LoadBlendRecipe(dest, mtd, layers, flverPaths);

        for (var i = mat.textureIndex; i < mat.textureIndex + mat.textureCount; i++)
        {
            var ttype = isUTF ? br.GetUTF16(textures[i].typeOffset) : br.GetShiftJIS(textures[i].typeOffset);
            var tpath = isUTF ? br.GetUTF16(textures[i].pathOffset) : br.GetShiftJIS(textures[i].pathOffset);
            ProcessMaterialTexture(dest, ttype, tpath, mtd, layers);
        }

        LoadDeformTextures(dest, mtd);
        ConfigureMaterialShader(dest, layers);
        MarkModelLighting(dest);

        // TEMPORARY BB texture diagnostics (BbTextureDiag)
        if (dest.DiagTag != null)
        {
            BbTextureDiag.Write($"SHADER {dest.DiagTag}: shader={dest.ShaderName} layers={layers.LayerCount} albedoFilled={dest.TextureResourceFilled[(int)FlverMaterial.TextureType.AlbedoTextureResource]} albedoFromColor={dest.AlbedoFromColor}");
        }
    }

    /// <summary>
    /// Characters are lit by the model set of the map's GPARAM LightSet instead of the scene's, as the game lights
    /// them. Flagged in the material, see <see cref="Material.ModelLightingFlag" />.
    /// <para>
    /// Demon's Souls picks its lighting by the MTD's g_LightingType instead (see the DES DrawParam lighting plan):
    /// 1 is the HemDir3 shaders, three directional lights and a hemisphere, which the model set holds; 3 is the
    /// HemEnv shaders, the hemisphere and env cubemaps, see <see cref="Material.EnvLightingFlag" />. Other values,
    /// and MTDs without it, are lit as before.
    /// </para>
    /// </summary>
    private void MarkModelLighting(FlverMaterial dest)
    {
        if (RenderProfile.Current.MaterialLightingType &&
            dest.Definition?.Params is { } mtdParams &&
            mtdParams.TryGetValue("g_LightingType", out var lightingType) && lightingType.Length > 0)
        {
            if ((int)lightingType[0] == 1)
            {
                dest.MaterialData.uvSet |= Material.ModelLightingFlag;
                return;
            }

            if ((int)lightingType[0] == 3)
            {
                var slot = mtdParams.TryGetValue("g_EnvSpcSlotNo", out var slotValue) && slotValue.Length > 0
                    ? Math.Clamp((int)slotValue[0], 0, 3) : 0;
                var shadowPow = mtdParams.TryGetValue("g_ShadowPowMul", out var shadowValue) && shadowValue.Length > 0
                    ? Math.Clamp(shadowValue[0], 0.0f, 1.0f) : 0.8f;

                dest.MaterialData.uvSet |= (ushort)(Material.EnvLightingFlag
                    | (slot << Material.EnvSpecularSlotShift)
                    | ((int)MathF.Round(shadowPow * 15.0f) << Material.EnvShadowShift));
                return;
            }
        }

        if (IsCharacterModel(Context))
        {
            dest.MaterialData.uvSet |= Material.ModelLightingFlag;
        }
    }

    /// <summary>
    /// Whether a FLVER belongs to a character: a c#### model, or a part from the parts folder. Characters built
    /// from parts, such as c0000 (players, and the ACs of Armored Core VI), are drawn from those parts instead of
    /// their own FLVER, see CharacterComposition. The parts folder only holds bodies and equipment.
    /// </summary>
    private static bool IsCharacterModel(ResourceLoadContext? ctx)
    {
        if (ctx == null)
            return false;

        var name = ctx.Name;

        if (name.Length >= 5 && char.ToLowerInvariant(name[0]) == 'c' &&
            char.IsDigit(name[1]) && char.IsDigit(name[2]) && char.IsDigit(name[3]) && char.IsDigit(name[4]))
            return true;

        // parts/xx_x_0000.partsbnd.dcx, or loose files under parts/
        foreach (var path in new[] { ctx.ContainerPath, ctx.EntryPath })
        {
            var normalized = path.Replace('\\', '/').TrimStart('/');

            if (normalized.Contains(".partsbnd", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("parts/", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillVertex(ref Vector3 dest, ref FLVER.Vertex v)
    {
        dest = v.Position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillVertex(Vector3* dest, BinaryReaderEx br, FLVER.LayoutType type)
    {
        if (type == FLVER.LayoutType.Float3)
        {
            *dest = br.ReadVector3();
        }
        else if (type == FLVER.LayoutType.Float4)
        {
            *dest = br.ReadVector3();
            br.AssertSingle(0);
        }
        else
        {
            throw new NotImplementedException(LOC.Get("REND_FlverResource_Vertex_Read_Not_Implemented", type));
        }

        // Sanity check position to find bugs
        //if (dest.X > 10000.0f || dest.Y > 10000.0f || dest.Z > 10000.0f)
        //{
        //    Debugger.Break();
        //}
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillNormalSNorm8(sbyte* dest, ref FLVER.Vertex v)
    {
        Vector3 n = Vector3.Normalize(new Vector3(v.Normal.X, v.Normal.Y, v.Normal.Z));
        dest[0] = (sbyte)(n.X * 127.0f);
        dest[1] = (sbyte)(n.Y * 127.0f);
        dest[2] = (sbyte)(n.Z * 127.0f);
        dest[3] = (sbyte)v.NormalW;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillNormalSNorm8(sbyte* dest, BinaryReaderEx br, FLVER.LayoutType type, Vector3* n, int[] meshBoneIndices, bool useNormalWTransform)
    {
        var nw = 0;
        if (type == FLVER.LayoutType.Float3)
        {
            *n = br.ReadVector3();
        }
        else if (type == FLVER.LayoutType.Float4)
        {
            *n = br.ReadVector3();
            var w = br.ReadSingle();
            nw = (int)w;
            if (w != nw)
            {
                throw new InvalidDataException(LOC.Get("REND_FlverResource_Float4_Invalid", w));
            }
        }
        else if (type == FLVER.LayoutType.Color)
        {
            *n = FLVER.Vertex.ReadByteNormXYZ(br);
            nw = br.ReadByte();
        }
        else if (type == FLVER.LayoutType.UByte4)
        {
            *n = FLVER.Vertex.ReadByteNormXYZ(br);
            nw = br.ReadByte();
        }
        else if (type == FLVER.LayoutType.Byte4)
        {
            nw = br.ReadByte();
            *n = FLVER.Vertex.ReadSByteNormZYX(br);
        }
        else if (type == FLVER.LayoutType.UByte4Norm)
        {
            *n = FLVER.Vertex.ReadByteNormXYZ(br);
            nw = br.ReadByte();
        }
        else if (type == FLVER.LayoutType.Short4Norm)
        {
            *n = FLVER.Vertex.ReadShortNormXYZ(br);
            nw = br.ReadInt16();
        }
        else if (type == FLVER.LayoutType.Half4)
        {
            *n = FLVER.Vertex.ReadUShortNormXYZ(br);
            nw = br.ReadInt16();
        }
        else if (type == FLVER.LayoutType.Byte4E)
        {
            *n = FLVER.Vertex.ReadByteNormXYZ(br);
            nw = br.ReadByte();
        }
        else if (type == FLVER.LayoutType.UShort4)
        {
            *n = FLVER.Vertex.ReadShortNormXYZAC6(br);
            nw = br.ReadInt16();
        }
        else
        {
            throw new NotImplementedException(
                LOC.Get("REND_FlverResource_Normal_Read_Not_Implemented", type));
        }

        dest[0] = (sbyte)(n->X * 127.0f);
        dest[1] = (sbyte)(n->Y * 127.0f);
        dest[2] = (sbyte)(n->Z * 127.0f);
        dest[3] = (sbyte)nw;

        // HACK: Move normalW index from local to global bone index
        if (useNormalWTransform && nw >= 0 && nw < meshBoneIndices.Length)
        {
            dest[3] = (sbyte)meshBoneIndices[nw];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillUVShort(short* dest, ref FLVER.Vertex v, byte index)
    {
        // A vertex doesn't have to have every UV set
        if (index >= v.UVs.Count)
        {
            dest[0] = 0;
            dest[1] = 0;
            return;
        }

        Vector3 uv = v.UVs[index];
        if (_uvSetScales != null && index < _uvSetScales.Length)
        {
            uv *= _uvSetScales[index];
        }

        dest[0] = PackUV(uv.X);
        dest[1] = PackUV(uv.Y);
    }

    /// <summary>
    /// What each UV set of the FLVER2 mesh being filled is multiplied by (<see cref="FloatUvScales" />), or null for
    /// none. Only the FLVER.Vertex fills read it; the raw buffer fills scale in <c>FillUVShort(..., BinaryReaderEx, ...)</c>
    /// </summary>
    private float[]? _uvSetScales;

    /// <summary>
    /// The scale of each UV set FLVER.Vertex reads from the mesh's buffer layouts, in the order it reads them: 1 / the
    /// UV scale for a Float2 member, 1 for the rest, or null when no member needs one. FLVER.Vertex reads float UVs
    /// unscaled, but a Float2 UV holds whole numbers at the file's UV scale, as the short types do: every Float2 UV of
    /// DS3's MultiBlend map pieces (M[ARSN]_2m_l and its kin, ~3,700 meshes, all whole numbers) is, and read unscaled
    /// they wrapped when packed and smeared the textures across the mesh (m30_00_00_00_000740). Float3 and Float4 UVs
    /// (AC6 billboards, NR SpeedTree, DS3 FlowDisplacement) are fractional and stay as they are
    /// (Claude/Tools/Ds3MapPieceProbe floatuv)
    /// </summary>
    private float[]? FloatUvScales(FLVER2.Mesh mesh)
    {
        if (Flver is not { } flver)
        {
            return null;
        }

        List<float> scales = new();
        var any = false;
        foreach (FLVER2.VertexBuffer buffer in mesh.VertexBuffers)
        {
            if (buffer.LayoutIndex < 0 || buffer.LayoutIndex >= flver.BufferLayouts.Count)
            {
                continue;
            }

            foreach (FLVER.LayoutMember member in flver.BufferLayouts[buffer.LayoutIndex])
            {
                if (member.Semantic != FLVER.LayoutSemantic.UV)
                {
                    continue;
                }

                var sets = member.Type is FLVER.LayoutType.Float4 or FLVER.LayoutType.Short4 or FLVER.LayoutType.Half4
                    or FLVER.LayoutType.UByte4Norm ? 2 : 1;
                var scale = member.Type == FLVER.LayoutType.Float2 ? 1f / _uvPackScale : 1f;
                any |= scale != 1f;
                for (var s = 0; s < sets; s++)
                {
                    scales.Add(scale);
                }
            }
        }

        return any ? scales.ToArray() : null;
    }

    /// <summary>
    /// How the shader reads the specular map of a Dark Souls Remastered material, as DS1R's FRPG_ shaders do
    /// (Claude/Tools/Ds1rShaderDisasm, FRPG_Phn_DifSpcBmp's gFC_MaterialWorkflow): 0 not one (the family's own
    /// reading), 1 for g_MaterialWorkflow 1 (RGB the specular color, A the roughness), 2 for the rest (R roughness,
    /// G metallic, B reflectance, A occlusion). FRPG_ shaders are only remastered where the specular map is read as
    /// gloss (RenderProfile.GlossInSpecular), which DS1 PTDE's aren't
    /// </summary>
    private static int DsrSpecularLayout(MaterialDefinition? definition, bool glossInSpecular)
    {
        if (!glossInSpecular || definition is not { FrpgShader: true })
        {
            return 0;
        }

        return definition.MaterialWorkflow == 1 ? 1 : 2;
    }

    /// <summary>
    /// What this FLVER's UVs are multiplied by to store them as 16 bit fixed point: see <see cref="UvPackScaleOf" />
    /// </summary>
    private float _uvPackScale = 2048f;

    /// <summary>
    /// The UV scale of a FLVER2 of this version, as its own short UVs have it: 1024 before 0x2000E (DES and DS1, whose
    /// map UVs reach ±32), 2048 after. Storing them at the file's scale keeps all of their range and loses no
    /// precision; at 2048 a DS1 UV past ±16 wrapped to the other side, and a triangle across that point was smeared
    /// over 32 tiles (DS1R m0100B2A10). The shaders take it as specialization constant 51 (FlverMaterial.UvPackScale)
    /// </summary>
    private static float UvPackScaleOf(int version)
    {
        return version >= 0x2000E ? 2048f : 1024f;
    }

    /// <summary>
    /// Vertex UVs are stored as 16 bit fixed point, at this FLVER's scale (<see cref="_uvPackScale" />). Values are
    /// rounded rather than truncated; anything outside the representable range wraps.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private short PackUV(float value)
    {
        return PackUV(value, _uvPackScale);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static short PackUV(float value, float scale)
    {
        if (ReportUVRanges)
        {
            UvRangeTracker.Track(value);
        }

        return (short)(int)MathF.Round(value * scale);
    }

    /// <summary>
    /// Debug switch. When true, the UV range of every sub-mesh is logged as it is loaded, and sub-meshes whose
    /// UVs don't fit the packed 16 bit range are flagged.
    /// </summary>
    public static bool ReportUVRanges = false;

    /// <summary>
    /// Largest magnitude a UV can have once packed as 16 bit fixed point with 11 fractional bits.
    /// </summary>
    private const float PackedUVLimit = 32768f / 2048f;

    /// <summary>
    /// Collects the UV values passed through PackUV while one sub-mesh is being filled. Thread static because
    /// models can be loaded on several threads at once.
    /// </summary>
    private static class UvRangeTracker
    {
        [ThreadStatic] private static float _min;
        [ThreadStatic] private static float _max;
        [ThreadStatic] private static int _count;
        [ThreadStatic] private static int _overflowCount;
        [ThreadStatic] private static bool _hasNonFinite;

        public static void Begin()
        {
            _min = float.MaxValue;
            _max = float.MinValue;
            _count = 0;
            _overflowCount = 0;
            _hasNonFinite = false;
        }

        public static void Track(float value)
        {
            if (!float.IsFinite(value))
            {
                _hasNonFinite = true;
                return;
            }

            _count++;
            if (value < _min) _min = value;
            if (value > _max) _max = value;
            if (value >= PackedUVLimit || value < -PackedUVLimit) _overflowCount++;
        }

        public static void End(object owner, string resourceName, int meshIndex, string materialName, int vertexCount)
        {
            if (_count == 0 && !_hasNonFinite)
                return;

            var range = _count > 0 ? $"min {_min:0.####}, max {_max:0.####}" : "no finite values";
            var flags = "";
            if (_overflowCount > 0)
                flags += $"  ** OVERFLOW: {_overflowCount} of {_count} UV values outside +/-{PackedUVLimit} and will wrap **";
            if (_hasNonFinite)
                flags += "  ** contains NaN/Infinity **";

            Smithbox.Log(owner, $"UV range: {resourceName} mesh {meshIndex} ({materialName}, {vertexCount} verts): {range}{flags}");
        }
    }

    private void BeginUVReport()
    {
        if (ReportUVRanges)
            UvRangeTracker.Begin();
    }

    private void EndUVReport(int meshIndex, FlverSubmesh mesh)
    {
        if (ReportUVRanges)
        {
            UvRangeTracker.End(this, Context?.QualifiedName ?? "", meshIndex,
                mesh.Material?.MaterialName ?? "", mesh.VertexCount);
        }
    }

    /// <summary>
    /// Fills both UV sets. A mesh with a single UV set uses it for both, which is what blended materials
    /// authored for one UV set expect.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillUVPairShort(FlverLayoutUV2* v, ref FLVER.Vertex vert, int maxUvSets = 2)
    {
        FillUVShort((*v).Uv1, ref vert, 0);
        if (vert.UVs.Count > 1)
        {
            FillUVShort((*v).Uv2, ref vert, (byte)(Math.Min(vert.UVs.Count, maxUvSets) - 1));
        }
        else
        {
            (*v).Uv2[0] = (*v).Uv1[0];
            (*v).Uv2[1] = (*v).Uv1[1];
        }
    }

    /// <summary>
    /// Reads a UV member. Some layout types hold two UV sets in one member (a UV pair): the first goes to dest
    /// and, if dest2 is given, the second to dest2. hasv2 reports whether the second one was stored.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillUVShort(short* dest, short* dest2, BinaryReaderEx br, FLVER.LayoutType type, float uvFactor,
        bool allowv2, out bool hasv2)
    {
        Vector3 v;
        Vector3 v2;
        hasv2 = false;
        if (type == FLVER.LayoutType.Float2)
        {
            // Float2 UVs hold whole numbers at the file's UV scale, as the short types do (see FloatUvScales)
            v = new Vector3(br.ReadVector2(), 0) / uvFactor;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
        }
        else if (type == FLVER.LayoutType.Float3)
        {
            v = br.ReadVector3();

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
        }
        else if (type == FLVER.LayoutType.Float4)
        {
            v = new Vector3(br.ReadVector2(), 0);
            v2 = new Vector3(br.ReadVector2(), 0);
            hasv2 = allowv2 && dest2 != null;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
            if (hasv2)
            {
                dest2[0] = PackUV(v2.X);
                dest2[1] = PackUV(v2.Y);
            }
        }
        else if (type == FLVER.LayoutType.Color)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
        }
        else if (type == FLVER.LayoutType.UByte4)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
        }
        else if (type == FLVER.LayoutType.Byte4)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
        }
        else if (type == FLVER.LayoutType.UByte4Norm)
        {
            // Two UVs packed into four bytes, each component a byte normalised by 255 (no uvFactor)
            v = new Vector3(br.ReadByte(), br.ReadByte(), 0) / 255f;
            v2 = new Vector3(br.ReadByte(), br.ReadByte(), 0) / 255f;
            hasv2 = allowv2 && dest2 != null;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
            if (hasv2)
            {
                dest2[0] = PackUV(v2.X);
                dest2[1] = PackUV(v2.Y);
            }
        }
        else if (type == FLVER.LayoutType.Short2)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
        }
        else if (type == FLVER.LayoutType.Half2)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
        }
        else if (type == FLVER.LayoutType.Short4)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
            v2 = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
            hasv2 = allowv2 && dest2 != null;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
            if (hasv2)
            {
                dest2[0] = PackUV(v2.X);
                dest2[1] = PackUV(v2.Y);
            }
        }
        else if (type == FLVER.LayoutType.Half4)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
            v2 = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
            hasv2 = allowv2 && dest2 != null;

            dest[0] = PackUV(v.X);
            dest[1] = PackUV(v.Y);
            if (hasv2)
            {
                dest2[0] = PackUV(v2.X);
                dest2[1] = PackUV(v2.Y);
            }
        }
        else
        {
            throw new NotImplementedException(
                LOC.Get("REND_FlverResource_UV_Read_Not_Implemented", type));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillUVShortZero(short* dest)
    {
        dest[0] = 0;
        dest[1] = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillUVFloat(ref Vector2 dest, ref FLVER.Vertex v, byte index)
    {
        Vector3 uv = v.UVs[index];
        dest.X = uv.X;
        dest.Y = uv.Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillBinormalBitangentSNorm8(sbyte* destBinorm, sbyte* destBitan, ref FLVER.Vertex v,
        byte index)
    {
        Vector4 tan = v.Tangents[index];
        Vector3 t = Vector3.Normalize(new Vector3(tan.X, tan.Y, tan.Z));
        destBitan[0] = (sbyte)(t.X * 127.0f);
        destBitan[1] = (sbyte)(t.Y * 127.0f);
        destBitan[2] = (sbyte)(t.Z * 127.0f);
        destBitan[3] = (sbyte)(tan.W * 127.0f);

        Vector3 bn = Vector3.Cross(Vector3.Normalize(v.Normal), Vector3.Normalize(new Vector3(t.X, t.Y, t.Z))) *
                     tan.W;
        destBinorm[0] = (sbyte)(bn.X * 127.0f);
        destBinorm[1] = (sbyte)(bn.Y * 127.0f);
        destBinorm[2] = (sbyte)(bn.Z * 127.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillBinormalBitangentSNorm8(sbyte* destBinorm, sbyte* destBitan, Vector3* n,
        BinaryReaderEx br, FLVER.LayoutType type)
    {
        Vector4 tan;
        if (type == FLVER.LayoutType.Float4)
        {
            tan = br.ReadVector4();
        }
        else if (type == FLVER.LayoutType.Color)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else if (type == FLVER.LayoutType.UByte4)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else if (type == FLVER.LayoutType.UByte4Norm)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else if (type == FLVER.LayoutType.Byte4Norm)
        {
            tan = FLVER.Vertex.ReadSByteNormWZYX(br);
        }
        else if (type == FLVER.LayoutType.Short4Norm)
        {
            tan = FLVER.Vertex.ReadShortNormXYZW(br);
        }
        else if (type == FLVER.LayoutType.Byte4E)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else
        {
            throw new NotImplementedException(
                LOC.Get("REND_FlverResource_Tangent_Read_Not_Implemented", type));
        }

        Vector3 t = Vector3.Normalize(new Vector3(tan.X, tan.Y, tan.Z));
        destBitan[0] = (sbyte)(t.X * 127.0f);
        destBitan[1] = (sbyte)(t.Y * 127.0f);
        destBitan[2] = (sbyte)(t.Z * 127.0f);
        destBitan[3] = (sbyte)(tan.W * 127.0f);

        Vector3 bn = Vector3.Cross(Vector3.Normalize(*n), Vector3.Normalize(new Vector3(t.X, t.Y, t.Z))) * tan.W;
        destBinorm[0] = (sbyte)(bn.X * 127.0f);
        destBinorm[1] = (sbyte)(bn.Y * 127.0f);
        destBinorm[2] = (sbyte)(bn.Z * 127.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillBinormalBitangentSNorm8Zero(sbyte* destBinorm, sbyte* destBitan)
    {
        destBitan[0] = 0;
        destBitan[1] = 0;
        destBitan[2] = 0;
        destBitan[3] = 127;

        destBinorm[0] = 0;
        destBinorm[1] = 0;
        destBinorm[2] = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillColorUNorm(byte* dest, ref FLVER.Vertex v)
    {
        if (v.Colors == null || v.Colors.Count == 0)
        {
            return;
        }

        var c = v.Colors[0];
        dest[0] = ColorChannel(c.R);
        dest[1] = ColorChannel(c.G);
        dest[2] = ColorChannel(c.B);
        dest[3] = ColorChannel(c.A);
    }

    // Vertex color channels are stored by the format library as either bytes or floats
    private static byte ColorChannel(byte value)
    {
        return value;
    }

    private static byte ColorChannel(float value)
    {
        return (byte)Math.Clamp((int)MathF.Round(value * 255.0f), 0, 255);
    }

    /// <summary>
    /// The vertex color member a buffer's vertices are given: its lowest index, so VertexColor0 where there is one.
    /// DS3's map pieces have only VertexColor1, which their GXFlver vertex shaders read as COLOR1 (and pass on to the
    /// mask blend's weight), so taking index 0 alone left them with no color at all
    /// </summary>
    private static int VertexColorIndex(Span<FlverBufferLayoutMember> layouts)
    {
        var index = int.MaxValue;
        foreach (FlverBufferLayoutMember l in layouts)
        {
            if (!l.IsSpecialMember && l.semantic == FLVER.LayoutSemantic.VertexColor && l.index < index)
            {
                index = l.index;
            }
        }

        return index == int.MaxValue ? 0 : index;
    }

    /// <summary>
    /// Reads a vertex color member as red, green, blue, alpha.
    /// </summary>
    private unsafe void FillVertexColor(byte* dest, BinaryReaderEx br, FLVER.LayoutType type)
    {
        switch (type)
        {
            case FLVER.LayoutType.Float4:
                dest[0] = ColorChannel(br.ReadSingle());
                dest[1] = ColorChannel(br.ReadSingle());
                dest[2] = ColorChannel(br.ReadSingle());
                dest[3] = ColorChannel(br.ReadSingle());
                break;
            case FLVER.LayoutType.Color:
            case FLVER.LayoutType.UByte4:
            case FLVER.LayoutType.UByte4Norm:
            case FLVER.LayoutType.Byte4:
            case FLVER.LayoutType.Byte4Norm:
            case FLVER.LayoutType.Byte4E:
                dest[0] = br.ReadByte();
                dest[1] = br.ReadByte();
                dest[2] = br.ReadByte();
                dest[3] = br.ReadByte();
                break;
            default:
                EatVertex(br, type);
                break;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EatVertex(BinaryReaderEx br, FLVER.LayoutType type)
    {
        switch (type)
        {
            case FLVER.LayoutType.EdgeCompressed:
                br.ReadByte();
                break;

            case FLVER.LayoutType.Float1:
            case FLVER.LayoutType.Color:
            case FLVER.LayoutType.UByte4:
            case FLVER.LayoutType.Byte4:
            case FLVER.LayoutType.UByte4Norm:
            case FLVER.LayoutType.Byte4Norm:
            case FLVER.LayoutType.Short2:
            case FLVER.LayoutType.UShort2:
            case FLVER.LayoutType.Byte4E:
            case FLVER.LayoutType.Half2:
                br.ReadUInt32();
                break;

            case FLVER.LayoutType.Float2:
            case FLVER.LayoutType.Short4:
            case FLVER.LayoutType.UShort4:
            case FLVER.LayoutType.Short4Norm:
            case FLVER.LayoutType.Half4:
                br.ReadUInt64();
                break;

            case FLVER.LayoutType.Float3:
                br.ReadUInt32();
                br.ReadUInt64();
                break;

            case FLVER.LayoutType.Float4:
                br.ReadUInt64();
                br.ReadUInt64();
                break;

            default:
                throw new NotImplementedException(
                    LOC.Get("REND_FlverResource_BufferLayout_Read_Not_Implemented", type));
        }
    }

    /// <summary>
    /// SpeedTree models lay their vertices out differently, which is only known for these games.
    /// See <see cref="FillVerticesSpeedTree(BinaryReaderEx, ref FlverVertexBuffer, Span{FlverBufferLayoutMember}, Span{Vector3}, nint, FlverMaterial)"/>
    /// </summary>
    private bool HasSpeedTreeVertices => IsSpeedtree &&
        RenderProfile.Current.SpeedTreeVertices;

    /// <summary>
    /// Stores a SpeedTree vertex in the layout of its material. The tangent of a SpeedTree vertex follows U, where
    /// the one of a standard FLVER follows V with its handedness flipped, so it is converted to what the shader expects.
    /// </summary>
    private static unsafe void StoreSpeedTreeVertex(byte* dest, FlverMaterial material, Vector3 position, Vector2 uv,
        Vector3 normal, Vector3 tangent, float handedness)
    {
        new Span<byte>(dest, (int)material.VertexSize).Clear();

        Vector3 n = Vector3.Normalize(normal);

        if (material.LayoutType == MeshLayoutType.LayoutSky)
        {
            FlverLayoutSky* sky = (FlverLayoutSky*)dest;
            (*sky).Position = position;
            (*sky).Normal[0] = (sbyte)(n.X * 127.0f);
            (*sky).Normal[1] = (sbyte)(n.Y * 127.0f);
            (*sky).Normal[2] = (sbyte)(n.Z * 127.0f);
            return;
        }

        // The other layouts all start the same
        FlverLayout* v = (FlverLayout*)dest;
        (*v).Position = position;
        (*v).Uv1[0] = PackUV(uv.X, material.UvPackScale);
        (*v).Uv1[1] = PackUV(uv.Y, material.UvPackScale);
        (*v).Normal[0] = (sbyte)(n.X * 127.0f);
        (*v).Normal[1] = (sbyte)(n.Y * 127.0f);
        (*v).Normal[2] = (sbyte)(n.Z * 127.0f);

        Vector3 t = Vector3.Normalize(tangent);
        Vector3 standardTangent = Vector3.Cross(n, t) * handedness;
        (*v).Bitangent[0] = (sbyte)(standardTangent.X * 127.0f);
        (*v).Bitangent[1] = (sbyte)(standardTangent.Y * 127.0f);
        (*v).Bitangent[2] = (sbyte)(standardTangent.Z * 127.0f);
        (*v).Bitangent[3] = (sbyte)(-handedness * 127.0f);
        (*v).Binormal[0] = (sbyte)(t.X * 127.0f);
        (*v).Binormal[1] = (sbyte)(t.Y * 127.0f);
        (*v).Binormal[2] = (sbyte)(t.Z * 127.0f);

        if (material.LayoutType is MeshLayoutType.LayoutUV2 or MeshLayoutType.LayoutDeform)
        {
            FlverLayoutUV2* uv2 = (FlverLayoutUV2*)dest;
            (*uv2).Uv2[0] = (*uv2).Uv1[0];
            (*uv2).Uv2[1] = (*uv2).Uv1[1];
        }
    }

    /// <summary>
    /// The layout of a SpeedTree model doesn't describe its vertices. What it calls the position and the first two UVs
    /// are the instance data of the game, which isn't in the file, and the rest is labelled as UVs:
    /// half4 position.xyz, half4 uv.xy, wind data, ubyte4 normal.xyz + handedness, ubyte4 tangent.xyz.
    /// The billboards start with a float4 instead. They need an atlas that isn't in the file, so they are left empty.
    /// </summary>
    private unsafe void FillVerticesSpeedTree(BinaryReaderEx br, ref FlverVertexBuffer buffer,
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, FlverMaterial material)
    {
        var hasGeometry = false;
        foreach (FlverBufferLayoutMember l in layouts)
        {
            if (!l.IsSpecialMember)
            {
                hasGeometry = l.type == FLVER.LayoutType.Half4;
                break;
            }
        }

        if (!hasGeometry)
        {
            new Span<byte>(vertBuffer.ToPointer(), buffer.vertexCount * (int)material.VertexSize).Clear();
            pickingVerts.Clear();
            return;
        }

        br.StepIn(buffer.bufferOffset);
        for (var i = 0; i < buffer.vertexCount; i++)
        {
            long start = buffer.bufferOffset + (long)i * buffer.vertexSize;

            br.Position = start;
            Vector3 position = new(ReadHalf(br), ReadHalf(br), ReadHalf(br));
            br.Position = start + 8;
            Vector2 uv = new(ReadHalf(br), ReadHalf(br));

            br.Position = start + buffer.vertexSize - 8;
            Vector4 normal = FLVER.Vertex.ReadByteNormXYZW(br);
            Vector4 tangent = FLVER.Vertex.ReadByteNormXYZW(br);

            StoreSpeedTreeVertex((byte*)vertBuffer + (i * material.VertexSize), material, position, uv,
                new Vector3(normal.X, normal.Y, normal.Z), new Vector3(tangent.X, tangent.Y, tangent.Z), MathF.Sign(normal.W));

            pickingVerts[i] = position;
        }

        br.StepOut();
    }

    private unsafe void FillVerticesSpeedTree(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer, FlverMaterial material)
    {
        FLVER.LayoutMember? first = mesh.VertexBuffers
            .SelectMany(b => Flver!.BufferLayouts[b.LayoutIndex])
            .FirstOrDefault(e => e.SpecialModifier == 0);

        if (first?.Type != FLVER.LayoutType.Half4)
        {
            new Span<byte>(vertBuffer.ToPointer(), mesh.Vertices.Count * (int)material.VertexSize).Clear();
            pickingVerts.Clear();
            return;
        }

        // FLVER.Vertex reads half floats as if they were 16 bit fixed point, so this gets the bits back
        float uvFactor = Flver!.Header.Version >= 0x2000E ? 2048 : 1024;
        float Half(float value) => (float)BitConverter.UInt16BitsToHalf((ushort)(short)Math.Round(value * uvFactor));

        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            List<Vector3> uvs = mesh.Vertices[i].UVs;

            Vector3 position = new(Half(uvs[0].X), Half(uvs[0].Y), Half(uvs[1].X));
            Vector2 uv = new(Half(uvs[2].X), Half(uvs[2].Y));

            // Bytes are kept as 0 to 1, and read as two UVs each
            Vector3 normal = new(uvs[^4].X * 2 - 1, uvs[^4].Y * 2 - 1, uvs[^3].X * 2 - 1);
            Vector3 tangent = new(uvs[^2].X * 2 - 1, uvs[^2].Y * 2 - 1, uvs[^1].X * 2 - 1);
            float handedness = MathF.Sign(uvs[^3].Y * 2 - 1);

            StoreSpeedTreeVertex((byte*)vertBuffer + (i * material.VertexSize), material, position, uv, normal, tangent, handedness);

            pickingVerts[i] = position;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ReadHalf(BinaryReaderEx br)
    {
        return (float)BitConverter.UInt16BitsToHalf(br.ReadUInt16());
    }

    private unsafe void FillVerticesNormalOnly(BinaryReaderEx br, ref FlverVertexBuffer buffer,
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, ref bool posfilled)
    {
        Span<FlverLayoutSky> verts = new(vertBuffer.ToPointer(), buffer.vertexCount);
        br.StepIn(buffer.bufferOffset);
        for (var i = 0; i < buffer.vertexCount; i++)
        {
            Vector3 n = Vector3.Zero;
            fixed (FlverLayoutSky* v = &verts[i])
            {
                foreach (FlverBufferLayoutMember l in layouts)
                {
                    // ER meme
                    if (l.IsSpecialMember)
                    {
                        continue;
                    }

                    if (l.semantic == FLVER.LayoutSemantic.Position)
                    {
                        // Edge compression is not supported here
                        if (l.type == LayoutType.EdgeCompressed)
                        {
                            continue;
                        }

                        FillVertex(&(*v).Position, br, l.type);
                        posfilled = true;
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.Normal)
                    {
                        FillNormalSNorm8((*v).Normal, br, l.type, &n, meshBoneIndices, useNormalWTransform);
                    }
                    else
                    {
                        EatVertex(br, l.type);
                    }
                }

                if (!posfilled)
                {
                    (*v).Position = new Vector3(0, 0, 0);
                }

                pickingVerts[i] = (*v).Position;
            }
        }

        br.StepOut();
    }

    private unsafe void FillVerticesNormalOnly(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer)
    {
        Span<FlverLayoutSky> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            FLVER.Vertex vert = mesh.Vertices[i];

            verts[i] = new FlverLayoutSky();
            pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
            fixed (FlverLayoutSky* v = &verts[i])
            {
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);
            }
        }
    }

    private unsafe void FillVerticesNormalOnly(FLVER0.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer)
    {
        Span<FlverLayoutSky> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        for (var i = 0; i < mesh.Vertices.Count; i++)
        {
            FLVER.Vertex vert = mesh.Vertices[i];

            verts[i] = new FlverLayoutSky();
            pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
            fixed (FlverLayoutSky* v = &verts[i])
            {
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);
            }
        }
    }

    private unsafe void FillVerticesStandard(BinaryReaderEx br, ref FlverVertexBuffer buffer,
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, float uvFactor, bool firstBuffer,
        bool vertexColor)
    {
        br.StepIn(buffer.bufferOffset);
        var colorIndex = VertexColorIndex(layouts);
        var pverts = (FlverLayout*)vertBuffer;

        for (var i = 0; i < buffer.vertexCount; i++)
        {
            FlverLayout* v = &pverts[i];
            Vector3 n = Vector3.UnitX;

            // A mesh can split its vertices across buffers, so only the first buffer may clear them
            if (firstBuffer)
            {
                *v = new FlverLayout();
                FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);

                // White where the mesh has no color, so that it changes nothing
                if (vertexColor)
                {
                    FillColorWhite((*v).Color);
                }
            }

            foreach (FlverBufferLayoutMember l in layouts)
            {
                // ER meme
                if (l.IsSpecialMember)
                {
                    continue;
                }

                if (l.semantic == FLVER.LayoutSemantic.Position)
                {
                    // Edge compression is not supported here
                    if (l.type == LayoutType.EdgeCompressed)
                    {
                        continue;
                    }

                    FillVertex(&(*v).Position, br, l.type);
                }
                else if (l.semantic == FLVER.LayoutSemantic.Normal)
                {
                    FillNormalSNorm8((*v).Normal, br, l.type, &n, meshBoneIndices, useNormalWTransform);
                }
                else if (l.semantic == FLVER.LayoutSemantic.UV && l.index == 0)
                {
                    bool hasv2;
                    FillUVShort((*v).Uv1, null, br, l.type, uvFactor, false, out hasv2);
                }
                else if (l.semantic == FLVER.LayoutSemantic.Tangent && l.index == 0)
                {
                    FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, &n, br, l.type);
                }
                else if (vertexColor && l.semantic == FLVER.LayoutSemantic.VertexColor && l.index == colorIndex)
                {
                    FillVertexColor((*v).Color, br, l.type);
                }
                else
                {
                    EatVertex(br, l.type);
                }
            }

            pickingVerts[i] = (*v).Position;
        }

        br.StepOut();
    }

    private static unsafe void FillColorWhite(byte* dest)
    {
        dest[0] = 255;
        dest[1] = 255;
        dest[2] = 255;
        dest[3] = 255;
    }

    /// <param name="vertexColor">
    /// Whether the material reads the vertex color (MaterialDefinition.VertexColor). The others leave it 0, as before:
    /// ER's ChrCustomize hair reads it in this layout too, and hasn't been checked with real colors
    /// </param>
    private unsafe void FillVerticesStandard(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer, bool vertexColor)
    {
        Span<FlverLayout> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        fixed (FlverLayout* pverts = verts)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                FlverLayout* v = &pverts[i];
                FLVER.Vertex vert = mesh.Vertices[i];

                verts[i] = new FlverLayout();
                pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);
                if (vert.UVs.Count > 0)
                {
                    FillUVShort((*v).Uv1, ref vert, 0);
                }
                else
                {
                    FillUVShortZero((*v).Uv1);
                }

                if (vert.Tangents.Count > 0)
                {
                    FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, ref vert, 0);
                }
                else
                {
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }

                if (vertexColor)
                {
                    FillColorWhite((*v).Color);
                    FillColorUNorm((*v).Color, ref vert);
                }
            }
        }
    }

    private unsafe void FillVerticesStandard(FLVER0.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer)
    {
        Span<FlverLayout> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        fixed (FlverLayout* pverts = verts)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                FlverLayout* v = &pverts[i];
                FLVER.Vertex vert = mesh.Vertices[i];

                verts[i] = new FlverLayout();
                pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);
                if (vert.UVs.Count > 0)
                {
                    FillUVShort((*v).Uv1, ref vert, 0);
                }
                else
                {
                    FillUVShortZero((*v).Uv1);
                }

                FillDesColor((*v).Color, ref vert);

                if (vert.Tangents.Count > 0)
                {
                    FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, ref vert, 0);
                }
                else
                {
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }
            }
        }
    }

    private unsafe void FillVerticesUV2(BinaryReaderEx br, ref FlverVertexBuffer buffer,
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, float uvFactor, bool firstBuffer, ref int uvSetsFilled, int maxUvSets)
    {
        Span<FlverLayoutUV2> verts = new(vertBuffer.ToPointer(), buffer.vertexCount);
        br.StepIn(buffer.bufferOffset);
        var colorIndex = VertexColorIndex(layouts);
        var uvsfilled = uvSetsFilled;
        fixed (FlverLayoutUV2* pverts = verts)
        {
            for (var i = 0; i < buffer.vertexCount; i++)
            {
                FlverLayoutUV2* v = &pverts[i];
                Vector3 n = Vector3.UnitX;

                // The buffer isn't cleared, and not every member is guaranteed to be in the mesh's layout.
                // A mesh can split its vertices across buffers, so only the first buffer may clear them.
                if (firstBuffer)
                {
                    *v = new FlverLayoutUV2();
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }

                uvsfilled = uvSetsFilled;
                foreach (FlverBufferLayoutMember l in layouts)
                {
                    // ER meme
                    if (l.IsSpecialMember)
                    {
                        continue;
                    }

                    if (l.semantic == FLVER.LayoutSemantic.Position)
                    {
                        // Edge compression is not supported here
                        if (l.type == LayoutType.EdgeCompressed)
                        {
                            continue;
                        }

                        FillVertex(&(*v).Position, br, l.type);
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.Normal)
                    {
                        FillNormalSNorm8((*v).Normal, br, l.type, &n, meshBoneIndices, useNormalWTransform);
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.UV && uvsfilled < maxUvSets)
                    {
                        bool hasv2;
                        if (uvsfilled == 0)
                        {
                            // A UV pair member holds both sets, so it can fill both
                            FillUVShort((*v).Uv1, (*v).Uv2, br, l.type, uvFactor, true, out hasv2);
                        }
                        else
                        {
                            FillUVShort((*v).Uv2, null, br, l.type, uvFactor, false, out hasv2);
                        }

                        uvsfilled += hasv2 ? 2 : 1;
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.VertexColor && l.index == colorIndex)
                    {
                        FillVertexColor((*v).Color, br, l.type);
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.Tangent && l.index == 0)
                    {
                        FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, &n, br, l.type);
                    }
                    else
                    {
                        EatVertex(br, l.type);
                    }
                }

                if (uvsfilled == 1)
                {
                    (*v).Uv2[0] = (*v).Uv1[0];
                    (*v).Uv2[1] = (*v).Uv1[1];
                }

                pickingVerts[i] = (*v).Position;
            }
        }

        uvSetsFilled = uvsfilled;
        br.StepOut();
    }

    /// <summary>
    /// <see cref="FillVerticesUV2"/> plus a third UV slot holding the FLVER UV set <paramref name="thirdUv"/> (a GXFlver
    /// mask blend's mask, FlverMaterial.MaskBlendUvIndex). The first two slots are filled as FillVerticesUV2 fills
    /// them; the third counts a UV pair member as its two sets, as FLVER.Vertex lists them. A mesh with fewer sets
    /// repeats its last one there. <paramref name="uvSetsSeen"/> carries the count across a mesh's buffers.
    /// </summary>
    private unsafe void FillVerticesUV3(BinaryReaderEx br, ref FlverVertexBuffer buffer,
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform,
        float uvFactor, bool firstBuffer, ref int uvSetsFilled, ref int uvSetsSeen, int maxUvSets, int thirdUv, int fadeUv = -1)
    {
        Span<FlverLayoutUV3> verts = new(vertBuffer.ToPointer(), buffer.vertexCount);
        br.StepIn(buffer.bufferOffset);
        var colorIndex = VertexColorIndex(layouts);
        var uvsfilled = uvSetsFilled;
        var seen = uvSetsSeen;
        short* first = stackalloc short[2];
        short* second = stackalloc short[2];

        fixed (FlverLayoutUV3* pverts = verts)
        {
            for (var i = 0; i < buffer.vertexCount; i++)
            {
                FlverLayoutUV3* v = &pverts[i];
                Vector3 n = Vector3.UnitX;

                // See FillVerticesUV2
                if (firstBuffer)
                {
                    *v = new FlverLayoutUV3();
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }

                uvsfilled = uvSetsFilled;
                seen = uvSetsSeen;
                var thirdFound = false;

                foreach (FlverBufferLayoutMember l in layouts)
                {
                    if (l.IsSpecialMember)
                    {
                        continue;
                    }

                    if (l.semantic == FLVER.LayoutSemantic.Position)
                    {
                        if (l.type == LayoutType.EdgeCompressed)
                        {
                            continue;
                        }

                        FillVertex(&(*v).Position, br, l.type);
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.Normal)
                    {
                        FillNormalSNorm8((*v).Normal, br, l.type, &n, meshBoneIndices, useNormalWTransform);
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.UV)
                    {
                        FillUVShort(first, second, br, l.type, uvFactor, true, out var hasv2);
                        var count = hasv2 ? 2 : 1;

                        if (uvsfilled < maxUvSets)
                        {
                            if (uvsfilled == 0)
                            {
                                (*v).Uv1[0] = first[0];
                                (*v).Uv1[1] = first[1];
                                if (hasv2)
                                {
                                    (*v).Uv2[0] = second[0];
                                    (*v).Uv2[1] = second[1];
                                }

                                uvsfilled += count;
                            }
                            else
                            {
                                (*v).Uv2[0] = first[0];
                                (*v).Uv2[1] = first[1];
                                uvsfilled += 1;
                            }
                        }

                        // The third slot: the set asked for, else the last one there is
                        if (!thirdFound)
                        {
                            short* pick = hasv2 && thirdUv > seen ? second : first;
                            (*v).Uv3[0] = pick[0];
                            (*v).Uv3[1] = pick[1];
                            thirdFound = thirdUv >= seen && thirdUv < seen + count;
                        }

                        // An opacity fade's set: its u as the vertex alpha (FlverMaterial.OpacityFadeUvIndex)
                        if (fadeUv >= seen && fadeUv < seen + count)
                        {
                            short* fade = fadeUv > seen ? second : first;
                            (*v).Color[0] = (*v).Color[1] = (*v).Color[2] = 255;
                            (*v).Color[3] = (byte)Math.Clamp((int)MathF.Round(fade[0] / _uvPackScale * 255f), 0, 255);
                        }

                        seen += count;
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.VertexColor && l.index == colorIndex)
                    {
                        FillVertexColor((*v).Color, br, l.type);
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.Tangent && l.index == 0)
                    {
                        FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, &n, br, l.type);
                    }
                    else
                    {
                        EatVertex(br, l.type);
                    }
                }

                if (uvsfilled == 1)
                {
                    (*v).Uv2[0] = (*v).Uv1[0];
                    (*v).Uv2[1] = (*v).Uv1[1];
                }

                pickingVerts[i] = (*v).Position;
            }
        }

        uvSetsFilled = uvsfilled;
        uvSetsSeen = seen;
        br.StepOut();
    }

    /// <summary>
    /// Which of TEXCOORD1-3 a layout member holds, or -1 if it isn't one of them.
    /// Their position in the layout says nothing, as other members can come before them.
    /// </summary>
    /// <param name="blendAttributes">
    ///     The TEXCOORDs the material's blend weights read (FlverMaterial.BlendVertexAttribute): the first in the low
    ///     8 bits, a second above them (0 for none). The first is put where TEXCOORD1 would be and the second where
    ///     TEXCOORD2 would be, which the shader's vertex blend values come from, leaving out what they replace.
    ///     AC6's [MS] blends read TEXCOORD1 and TEXCOORD5
    /// </param>
    private static int GetTexCoordIndex(FLVER.LayoutSemantic semantic, FLVER.LayoutType type, int index, int blendAttributes = 1)
    {
        var first = blendAttributes & 0xFF;
        var second = blendAttributes >> 8;

        if (semantic == FLVER.LayoutSemantic.UV && type == FLVER.LayoutType.UByte4Norm && (first != 1 || second != 0))
        {
            if (second != 0 && index == second)
                return 1;
            if (index == first)
                return 0;
            if ((index == 1 && first != 1) || (index == 2 && second != 0))
                return -1;
        }

        if (semantic != FLVER.LayoutSemantic.UV || index < 1 || index > FlverLayoutDeform.TexCoord3Index + 1)
            return -1;

        // Lower detail meshes have the pivot of TEXCOORD3 one attribute earlier, and no TEXCOORD3
        if (type == FLVER.LayoutType.Half4 && index - 1 == FlverLayoutDeform.TexCoord3Index - 1)
            return FlverLayoutDeform.TexCoord3Index;

        var isTexCoord = type == FLVER.LayoutType.UByte4Norm
            || (type == FLVER.LayoutType.Half4 && index - 1 == FlverLayoutDeform.TexCoord3Index);

        return isTexCoord ? index - 1 : -1;
    }

    /// <summary>
    /// Stores a byte attribute, which TEXCOORD3 keeps as half floats whatever the mesh had
    /// </summary>
    private static unsafe void SetByteTexCoord(FlverLayoutDeform* dest, int texCoord, int component, byte value)
    {
        if (texCoord == FlverLayoutDeform.TexCoord3Index)
        {
            (*dest).TexCoord3[component] = BitConverter.HalfToUInt16Bits((Half)(value / 255f));
        }
        else
        {
            (*dest).TexCoords[(texCoord * FlverLayoutDeform.TexCoordSize) + component] = value;
        }
    }

    private unsafe void FillVerticesDeform(BinaryReaderEx br, ref FlverVertexBuffer buffer,
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, float uvFactor, bool firstBuffer, ref int uvSetsFilled, int maxUvSets, int blendAttribute = 1, int vatColumnMember = -1)
    {
        Span<FlverLayoutDeform> verts = new(vertBuffer.ToPointer(), buffer.vertexCount);
        br.StepIn(buffer.bufferOffset);
        var colorIndex = VertexColorIndex(layouts);
        var uvsfilled = uvSetsFilled;
        fixed (FlverLayoutDeform* pverts = verts)
        {
            for (var i = 0; i < buffer.vertexCount; i++)
            {
                FlverLayoutDeform* d = &pverts[i];
                FlverLayoutUV2* v = (FlverLayoutUV2*)d;
                Vector3 n = Vector3.UnitX;

                // See FillVerticesUV2
                if (firstBuffer)
                {
                    *d = new FlverLayoutDeform();
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }

                uvsfilled = uvSetsFilled;
                var hasColumn = false;
                byte columnLow = 0, columnHigh = 0;
                foreach (FlverBufferLayoutMember l in layouts)
                {
                    // ER meme
                    if (l.IsSpecialMember)
                    {
                        continue;
                    }

                    var texCoord = GetTexCoordIndex(l.semantic, l.type, l.index, blendAttribute);

                    if (vatColumnMember > 0 && l.index == vatColumnMember && IsVatFrameColumnCandidate(l.semantic, l.type, l.index))
                    {
                        // Kept as it would be otherwise, and its first two bytes are the vertex's column
                        for (var c = 0; c < FlverLayoutDeform.TexCoordSize; c++)
                        {
                            var value = br.ReadByte();
                            if (texCoord >= 0)
                            {
                                SetByteTexCoord(d, texCoord, c, value);
                            }

                            if (c == 0)
                            {
                                columnLow = value;
                            }
                            else if (c == 1)
                            {
                                columnHigh = value;
                            }
                        }

                        hasColumn = true;
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.Position)
                    {
                        // Edge compression is not supported here
                        if (l.type == LayoutType.EdgeCompressed)
                        {
                            continue;
                        }

                        FillVertex(&(*v).Position, br, l.type);
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.Normal)
                    {
                        FillNormalSNorm8((*v).Normal, br, l.type, &n, meshBoneIndices, useNormalWTransform);
                    }
                    else if (texCoord >= 0)
                    {
                        for (var c = 0; c < FlverLayoutDeform.TexCoordSize; c++)
                        {
                            if (l.type == FLVER.LayoutType.Half4)
                            {
                                (*d).TexCoord3[c] = br.ReadUInt16();
                            }
                            else
                            {
                                SetByteTexCoord(d, texCoord, c, br.ReadByte());
                            }
                        }
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.UV && uvsfilled < maxUvSets)
                    {
                        bool hasv2;
                        if (uvsfilled == 0)
                        {
                            // A UV pair member holds both sets, so it can fill both
                            FillUVShort((*v).Uv1, (*v).Uv2, br, l.type, uvFactor, true, out hasv2);
                        }
                        else
                        {
                            FillUVShort((*v).Uv2, null, br, l.type, uvFactor, false, out hasv2);
                        }

                        uvsfilled += hasv2 ? 2 : 1;
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.VertexColor && l.index == colorIndex)
                    {
                        FillVertexColor((*v).Color, br, l.type);
                    }
                    else if (l.semantic == FLVER.LayoutSemantic.Tangent && l.index == 0)
                    {
                        FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, &n, br, l.type);
                    }
                    else
                    {
                        EatVertex(br, l.type);
                    }
                }

                if (uvsfilled == 1)
                {
                    (*v).Uv2[0] = (*v).Uv1[0];
                    (*v).Uv2[1] = (*v).Uv1[1];
                }

                if (hasColumn)
                {
                    SetVatFrameColumn(d, columnLow, columnHigh);
                }

                pickingVerts[i] = (*v).Position;
            }
        }

        uvSetsFilled = uvsfilled;
        br.StepOut();
    }

    /// <summary>
    /// Whether a vertex attribute can hold a cutscene vertex animation's column: a TEXCOORD1-3 byte attribute. The
    /// highest one a mesh has holds it (Claude/Documentation/Cutscene Vertex Animation.md).
    /// </summary>
    private static bool IsVatFrameColumnCandidate(FLVER.LayoutSemantic semantic, FLVER.LayoutType type, int index)
    {
        return semantic == FLVER.LayoutSemantic.UV && type == FLVER.LayoutType.UByte4Norm && index is >= 1 and <= 3;
    }

    /// <summary>
    /// Puts a cutscene vertex animation's column in TEXCOORD3.xy, which those materials don't otherwise read. The two
    /// bytes go in as whole numbers, as columns go past what a half float holds exactly (7977 in ER).
    /// </summary>
    private static unsafe void SetVatFrameColumn(FlverLayoutDeform* dest, byte low, byte high)
    {
        (*dest).TexCoord3[0] = BitConverter.HalfToUInt16Bits((Half)low);
        (*dest).TexCoord3[1] = BitConverter.HalfToUInt16Bits((Half)high);
    }

    /// <summary>
    /// Finds where TEXCOORD1-3 ended up in the flat UV list of a vertex: each entry is the index of a
    /// member's first UV, by TEXCOORD, or -1 for one the mesh doesn't have.
    /// </summary>
    private int[] GetTexCoordUVs(FLVER2.Mesh mesh, out bool halfTexCoord3, int blendAttribute = 1)
    {
        var uvs = new int[FlverLayoutDeform.TexCoord3Index + 1];
        Array.Fill(uvs, -1);

        halfTexCoord3 = false;

        var uvIndex = 0;
        foreach (FLVER2.VertexBuffer buffer in mesh.VertexBuffers)
        {
            foreach (FLVER.LayoutMember member in Flver!.BufferLayouts[buffer.LayoutIndex])
            {
                if (member.Semantic != FLVER.LayoutSemantic.UV)
                {
                    continue;
                }

                var texCoord = GetTexCoordIndex(member.Semantic, member.Type, member.Index, blendAttribute);
                if (texCoord >= 0)
                {
                    uvs[texCoord] = uvIndex;
                    halfTexCoord3 |= member.Type == FLVER.LayoutType.Half4;
                }

                // These are read as two UVs each
                uvIndex += member.Type is FLVER.LayoutType.Float4 or FLVER.LayoutType.UByte4Norm
                    or FLVER.LayoutType.Short4 or FLVER.LayoutType.Half4 ? 2 : 1;
            }
        }

        return uvs;
    }

    /// <summary>
    /// Where a cutscene vertex animation's column is in the flat UV list of a vertex: the first UV of the mesh's highest
    /// TEXCOORD1-3 byte attribute, or -1 if it has none.
    /// </summary>
    private int GetVatFrameColumnUV(FLVER2.Mesh mesh)
    {
        int uvIndex = 0, column = -1, columnIndex = -1;
        foreach (FLVER2.VertexBuffer buffer in mesh.VertexBuffers)
        {
            foreach (FLVER.LayoutMember member in Flver!.BufferLayouts[buffer.LayoutIndex])
            {
                if (member.Semantic != FLVER.LayoutSemantic.UV)
                {
                    continue;
                }

                if (IsVatFrameColumnCandidate(member.Semantic, member.Type, member.Index) && member.Index > columnIndex)
                {
                    columnIndex = member.Index;
                    column = uvIndex;
                }

                // As GetTexCoordUVs counts them
                uvIndex += member.Type is FLVER.LayoutType.Float4 or FLVER.LayoutType.UByte4Norm
                    or FLVER.LayoutType.Short4 or FLVER.LayoutType.Half4 ? 2 : 1;
            }
        }

        return column;
    }

    private unsafe void FillVerticesDeform(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer, int maxUvSets, int blendAttribute = 1, bool vatFrame = false)
    {
        var texCoordUVs = GetTexCoordUVs(mesh, out var halfTexCoord3, blendAttribute);
        var vatColumnUV = vatFrame ? GetVatFrameColumnUV(mesh) : -1;

        // FLVER.Vertex reads half floats as if they were 16 bit fixed point, so this gets the bits back
        float uvFactor = Flver!.Header.Version >= 0x2000E ? 2048 : 1024;

        Span<FlverLayoutDeform> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        fixed (FlverLayoutDeform* pverts = verts)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                FLVER.Vertex vert = mesh.Vertices[i];

                verts[i] = new FlverLayoutDeform();
                pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
                FlverLayoutDeform* d = &pverts[i];
                FlverLayoutUV2* v = (FlverLayoutUV2*)d;
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);
                FillUVPairShort(v, ref vert, maxUvSets);
                FillColorUNorm((*v).Color, ref vert);
                if (vert.Tangents.Count > 0)
                {
                    FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, ref vert, 0);
                }
                else
                {
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }

                for (var t = 0; t < texCoordUVs.Length; t++)
                {
                    if (texCoordUVs[t] < 0)
                    {
                        continue;
                    }

                    Vector3 first = vert.UVs[texCoordUVs[t]];
                    Vector3 second = vert.UVs[texCoordUVs[t] + 1];
                    if (halfTexCoord3 && t == FlverLayoutDeform.TexCoord3Index)
                    {
                        (*d).TexCoord3[0] = (ushort)(short)Math.Round(first.X * uvFactor);
                        (*d).TexCoord3[1] = (ushort)(short)Math.Round(first.Y * uvFactor);
                        (*d).TexCoord3[2] = (ushort)(short)Math.Round(second.X * uvFactor);
                        (*d).TexCoord3[3] = (ushort)(short)Math.Round(second.Y * uvFactor);
                        continue;
                    }

                    SetByteTexCoord(d, t, 0, (byte)Math.Round(first.X * 255f));
                    SetByteTexCoord(d, t, 1, (byte)Math.Round(first.Y * 255f));
                    SetByteTexCoord(d, t, 2, (byte)Math.Round(second.X * 255f));
                    SetByteTexCoord(d, t, 3, (byte)Math.Round(second.Y * 255f));
                }

                if (vatColumnUV >= 0)
                {
                    Vector3 column = vert.UVs[vatColumnUV];
                    SetVatFrameColumn(d, (byte)Math.Round(column.X * 255f), (byte)Math.Round(column.Y * 255f));
                }
            }
        }
    }

    /// <summary>
    /// <see cref="FillVerticesUV2(FLVER2.Mesh, Span{Vector3}, nint, int)"/> plus the FLVER UV set <paramref name="thirdUv"/>
    /// in the third slot (a GXFlver mask blend's mask), or the last set where the mesh has fewer
    /// </summary>
    private unsafe void FillVerticesUV3(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer, int maxUvSets, int thirdUv, int fadeUv = -1)
    {
        Span<FlverLayoutUV3> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        fixed (FlverLayoutUV3* pverts = verts)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                FLVER.Vertex vert = mesh.Vertices[i];

                verts[i] = new FlverLayoutUV3();
                pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
                FlverLayoutUV3* v = &pverts[i];
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);
                // FlverLayoutUV3 starts as FlverLayoutUV2 does
                FillUVPairShort((FlverLayoutUV2*)v, ref vert, maxUvSets);
                if (vert.UVs.Count > 0)
                {
                    FillUVShort((*v).Uv3, ref vert, (byte)Math.Min(thirdUv, vert.UVs.Count - 1));
                }

                FillColorUNorm((*v).Color, ref vert);

                // An opacity fade's set: its u as the vertex alpha (FlverMaterial.OpacityFadeUvIndex)
                if (fadeUv >= 0 && fadeUv < vert.UVs.Count)
                {
                    (*v).Color[0] = (*v).Color[1] = (*v).Color[2] = 255;
                    (*v).Color[3] = (byte)Math.Clamp((int)MathF.Round(vert.UVs[fadeUv].X * 255f), 0, 255);
                }

                if (vert.Tangents.Count > 0)
                {
                    FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, ref vert, 0);
                }
                else
                {
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }
            }
        }
    }

    private unsafe void FillVerticesUV2(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer, int maxUvSets)
    {
        Span<FlverLayoutUV2> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        fixed (FlverLayoutUV2* pverts = verts)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                FLVER.Vertex vert = mesh.Vertices[i];

                verts[i] = new FlverLayoutUV2();
                pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
                FlverLayoutUV2* v = &pverts[i];
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);
                FillUVPairShort(v, ref vert, maxUvSets);
                FillColorUNorm((*v).Color, ref vert);
                if (vert.Tangents.Count > 0)
                {
                    FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, ref vert, 0);
                }
                else
                {
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }
            }
        }
    }

    /// <summary>
    /// A FLVER0 vertex color, which the DES "Col" shaders (all of them) multiply the diffuse by, see FlverShader.frag.
    /// White when the vertex has none, so the multiply leaves it as it is. Every surveyed DES layout has one.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillDesColor(byte* dest, ref FLVER.Vertex v)
    {
        if (v.Colors == null || v.Colors.Count == 0)
        {
            dest[0] = dest[1] = dest[2] = dest[3] = 255;
            return;
        }

        FillColorUNorm(dest, ref v);
    }

    private unsafe void FillVerticesUV2(FLVER0.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer)
    {
        Span<FlverLayoutUV2> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        fixed (FlverLayoutUV2* pverts = verts)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                FLVER.Vertex vert = mesh.Vertices[i];

                verts[i] = new FlverLayoutUV2();
                pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
                FlverLayoutUV2* v = &pverts[i];
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);
                FillUVPairShort(v, ref vert);
                FillDesColor((*v).Color, ref vert);
                if (vert.Tangents.Count > 0)
                {
                    FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, ref vert, 0);
                }
                else
                {
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }
            }
        }
    }

    /// <summary>
    /// DES multi texture meshes with a lightmap ([ML]): UV 1 (layer 1), UV 2 (layer 2) and the last UV set (the
    /// lightmap's, UV 3 in the MTD). A mesh with fewer sets repeats its last one, as <see cref="FillUVPairShort"/> does.
    /// </summary>
    private unsafe void FillVerticesUV3(FLVER0.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer)
    {
        Span<FlverLayoutUV3> verts = new(vertBuffer.ToPointer(), mesh.Vertices.Count);
        fixed (FlverLayoutUV3* pverts = verts)
        {
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                FLVER.Vertex vert = mesh.Vertices[i];

                verts[i] = new FlverLayoutUV3();
                pickingVerts[i] = new Vector3(vert.Position.X, vert.Position.Y, vert.Position.Z);
                FlverLayoutUV3* v = &pverts[i];
                FillVertex(ref (*v).Position, ref vert);
                FillNormalSNorm8((*v).Normal, ref vert);

                var last = (byte)Math.Max(vert.UVs.Count - 1, 0);
                FillUVShort((*v).Uv1, ref vert, 0);
                FillUVShort((*v).Uv2, ref vert, Math.Min((byte)1, last));
                FillUVShort((*v).Uv3, ref vert, last);

                FillDesColor((*v).Color, ref vert);
                if (vert.Tangents.Count > 0)
                {
                    FillBinormalBitangentSNorm8((*v).Binormal, (*v).Bitangent, ref vert, 0);
                }
                else
                {
                    FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                }
            }
        }
    }

    /// <summary>
    /// Moves the vertices of a rigid FLVER0 mesh (Dynamic 0) to where their nodes put them. Each vertex names its node
    /// through the mesh's bone list with BoneIndices[0], and is stored relative to it, so it takes the node's world
    /// transform. A bind-pose mesh (Dynamic 1) is already in place, whatever its layout has.
    /// ModelRig skins rigid meshes to the same node at full weight, so an animation moves them from this pose.
    /// </summary>
    private void PlaceRigidVertices(FLVER0.Mesh mesh)
    {
        if (mesh.Dynamic != 0 || BoneTransforms == null || BoneTransforms.Count == 0 || FlverDeS == null)
            return;

        var layouts = FlverDeS.Materials[mesh.MaterialIndex].Layouts;
        var hasBoneIndices = mesh.LayoutIndex < layouts.Count &&
            layouts[mesh.LayoutIndex].Any(m => m.Semantic == FLVER.LayoutSemantic.BoneIndices);
        var usedBones = mesh.UsedBoneCount;

        for (var v = 0; v < mesh.Vertices.Count; v++)
        {
            FLVER.Vertex vert = mesh.Vertices[v];
            var node = GetRigidVertexNode(mesh, vert, hasBoneIndices, usedBones);

            if (node < 0 || node >= BoneTransforms.Count)
                continue;

            Matrix4x4 world = BoneTransforms[node];

            vert.Position = Vector3.Transform(vert.Position, world);
            vert.Normal = Vector3.TransformNormal(vert.Normal, world);

            // The binormal is rebuilt from the normal and tangent, so only the tangent has to turn; W keeps the handedness
            for (var t = 0; t < vert.Tangents.Count; t++)
            {
                Vector4 tan = vert.Tangents[t];
                var dir = Vector3.TransformNormal(new Vector3(tan.X, tan.Y, tan.Z), world);
                vert.Tangents[t] = new Vector4(dir, tan.W);
            }
        }
    }

    /// <summary>
    /// The node a vertex of a rigid FLVER0 mesh is attached to: its first bone index through the mesh's bone list,
    /// or the mesh's own node when the layout has no bone indices. -1 if the index is out of range, which leaves it in place.
    /// Every DES layout surveyed has bone indices, so the mesh node is only a fallback.
    /// </summary>
    internal static int GetRigidVertexNode(FLVER0.Mesh mesh, FLVER.Vertex vert, bool hasBoneIndices, int usedBones)
    {
        if (!hasBoneIndices)
            return mesh.NodeIndex;

        var local = vert.BoneIndices[0];

        return local >= 0 && local < usedBones ? mesh.BoneIndices[local] : -1;
    }

    private unsafe void ProcessMesh(FLVER0.Mesh mesh, FlverSubmesh dest)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        ResourceFactory? factory = SceneRenderer.Factory;

        if (FlverDeS == null)
            return;

        if (GPUMaterials == null)
            return;

        dest.Material = GPUMaterials[mesh.MaterialIndex];

        PlaceRigidVertices(mesh);

        var vSize = dest.Material.VertexSize;
        dest.PickingVertices = Marshal.AllocHGlobal(mesh.Vertices.Count * sizeof(Vector3));
        Span<Vector3> pvhandle = new(dest.PickingVertices.ToPointer(), mesh.Vertices.Count);
        var vbuffersize = (uint)mesh.Vertices.Count * vSize;

        dest.VertexCount = mesh.Vertices.Count;

        dest.MeshFacesets = new List<FlverSubmesh.FlverSubmeshFaceSet>();

        var is32bit = false; //FlverDeS.Version > 0x20005 && mesh.Vertices.Count > 65535;
        Span<ushort> fs16 = null;
        Span<int> fs32 = null;

        var indices = mesh.Triangulate(FlverDeS.Header.Version, false, false).ToArray();
        var indicesTotal = indices.Length;

        dest.GeomBuffer = SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize,
            (uint)indicesTotal * (is32bit ? 4u : 2u), (int)vSize, 4);
        var meshVertices = dest.GeomBuffer.MapVBuffer();
        var meshIndices = dest.GeomBuffer.MapIBuffer();

        if (dest.Material.LayoutType == MeshLayoutType.LayoutSky)
        {
            FillVerticesNormalOnly(mesh, pvhandle, meshVertices);
        }
        else if (dest.Material.LayoutType == MeshLayoutType.LayoutUV2)
        {
            FillVerticesUV2(mesh, pvhandle, meshVertices);
        }
        else if (dest.Material.LayoutType == MeshLayoutType.LayoutUV3)
        {
            FillVerticesUV3(mesh, pvhandle, meshVertices);
        }
        else
        {
            FillVerticesStandard(mesh, pvhandle, meshVertices);
        }

        if (mesh.Indices.Count != 0)
        {
            if (is32bit)
            {
                fs32 = new Span<int>(meshIndices.ToPointer(), indicesTotal);
            }
            else
            {
                fs16 = new Span<ushort>(meshIndices.ToPointer(), indicesTotal);
            }

            FlverSubmesh.FlverSubmeshFaceSet newFaceSet = new()
            {
                // FLVER0 keeps the flag on the mesh: cloth, hair cards, foliage and banners are drawn from both sides
                BackfaceCulling = mesh.CullBackfaces,
                IsTriangleStrip = false,
                //IndexBuffer = factory.CreateBuffer(new BufferDescription(buffersize, BufferUsage.IndexBuffer)),
                IndexOffset = 0,
                IndexCount = indices.Length,
                Is32Bit = is32bit,
                PickingIndicesCount = indices.Length
                //PickingIndices = Marshal.AllocHGlobal(indices.Length * 4),
            };

            if (is32bit)
            {
                for (var i = 0; i < indices.Length; i++)
                {
                    if (indices[i] == 0xFFFF && indices[i] > mesh.Vertices.Count)
                    {
                        fs32[newFaceSet.IndexOffset + i] = -1;
                    }
                    else
                    {
                        fs32[newFaceSet.IndexOffset + i] = indices[i];
                    }
                }
            }
            else
            {
                for (var i = 0; i < indices.Length; i++)
                {
                    if (indices[i] == 0xFFFF && indices[i] > mesh.Vertices.Count)
                    {
                        fs16[newFaceSet.IndexOffset + i] = 0xFFFF;
                    }
                    else
                    {
                        fs16[newFaceSet.IndexOffset + i] = (ushort)indices[i];
                    }
                }
            }

            dest.MeshFacesets.Add(newFaceSet);
        }

        SanitizeIndices(dest, fs16, fs32);
        AddDetailStats(dest, pvhandle, fs16, fs32);
        RetainVertices(dest, meshVertices, vbuffersize, vSize);

        dest.GeomBuffer.UnmapVBuffer();
        dest.GeomBuffer.UnmapIBuffer();

        dest.Bounds = BoundingBox.CreateFromPoints((Vector3*)dest.PickingVertices.ToPointer(), dest.VertexCount, 12,
            Quaternion.Identity, Vector3.Zero, Vector3.One);

        //if (CaptureMaterialLayouts)
        //{
        //    lock (_matLayoutLock)
        //    {
        //        if (!MaterialLayouts.ContainsKey(dest.Material.MaterialName))
        //        {
        //            MaterialLayouts.Add(dest.Material.MaterialName, Flver.BufferLayouts[mesh.LayoutIndex]);
        //        }
        //    }
        //}
    }

    private unsafe void ProcessMesh(FLVER2.Mesh mesh, FlverSubmesh dest)
    {
        if (mesh.Vertices == null)
            return;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (Flver == null)
            return;

        if (GPUMaterials == null)
            return;

        dest.Material = GPUMaterials[mesh.MaterialIndex];

        var vSize = dest.Material.VertexSize;
        dest.PickingVertices = Marshal.AllocHGlobal(mesh.Vertices.Count * sizeof(Vector3));
        Span<Vector3> pvhandle = new(dest.PickingVertices.ToPointer(), mesh.Vertices.Count);

        dest.VertexCount = mesh.Vertices.Count;

        dest.MeshFacesets = new List<FlverSubmesh.FlverSubmeshFaceSet>();
        List<FLVER2.FaceSet>? facesets = mesh.FaceSets;

        var is32bit = Flver.Header.Version > 0x20005 && mesh.Vertices.Count > 65535;
        var indicesTotal = 0;
        Span<ushort> fs16 = null;
        Span<int> fs32 = null;
        foreach (FLVER2.FaceSet? faceset in facesets)
        {
            indicesTotal += faceset.Indices.Count;
        }

        var vbuffersize = (uint)mesh.Vertices.Count * vSize;
        dest.GeomBuffer = SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize,
            (uint)indicesTotal * (is32bit ? 4u : 2u), (int)vSize, 4);
        var meshVertices = dest.GeomBuffer.MapVBuffer();
        var meshIndices = dest.GeomBuffer.MapIBuffer();

        _uvSetScales = FloatUvScales(mesh);

        if (HasSpeedTreeVertices)
        {
            FillVerticesSpeedTree(mesh, pvhandle, meshVertices, dest.Material);
        }
        // Untextured
        else if (dest.Material.LayoutType == MeshLayoutType.LayoutSky)
        {
            FillVerticesNormalOnly(mesh, pvhandle, meshVertices);
        }
        else if (dest.Material.LayoutType == MeshLayoutType.LayoutUV2)
        {
            FillVerticesUV2(mesh, pvhandle, meshVertices, dest.Material.MaxUvSets);
        }
        else if (dest.Material.LayoutType == MeshLayoutType.LayoutUV3)
        {
            FillVerticesUV3(mesh, pvhandle, meshVertices, dest.Material.MaxUvSets, dest.Material.ThirdUvIndex, dest.Material.OpacityFadeUvIndex);
        }
        else if (dest.Material.LayoutType == MeshLayoutType.LayoutDeform)
        {
            FillVerticesDeform(mesh, pvhandle, meshVertices, dest.Material.MaxUvSets, dest.Material.BlendVertexAttribute,
                dest.Material.Definition?.Deformer == MaterialDeformer.VatFrame);        }
        else
        {
            FillVerticesStandard(mesh, pvhandle, meshVertices, dest.Material.Definition?.ReadsVertexColor == true);
        }

        _uvSetScales = null;

        if (is32bit)
        {
            fs32 = new Span<int>(meshIndices.ToPointer(), indicesTotal);
        }
        else
        {
            fs16 = new Span<ushort>(meshIndices.ToPointer(), indicesTotal);
        }

        var idxoffset = 0;
        foreach (FLVER2.FaceSet? faceset in facesets)
        {
            if (faceset.Indices.Count == 0)
            {
                continue;
            }
            // The LODs of a SpeedTree model are whole meshes of their own, which would be drawn over each other
            if ((!CFG.Current.Viewport_Enable_LOD_Facesets || HasSpeedTreeVertices) &&
                faceset.Flags != FLVER2.FaceSet.FSFlags.None &&
                faceset.Flags != FLVER2.FaceSet.FSFlags.EdgeCompressed)
            {
                continue;
            }

            //At this point they use 32-bit faceset vertex indices
            FlverSubmesh.FlverSubmeshFaceSet newFaceSet = new()
            {
                BackfaceCulling = faceset.CullBackfaces,
                IsTriangleStrip = faceset.TriangleStrip,
                IndexOffset = idxoffset,
                IndexCount = faceset.Indices.Count,
                Is32Bit = is32bit
            };


            if ((faceset.Flags & FLVER2.FaceSet.FSFlags.LodLevel1) > 0)
            {
                newFaceSet.LOD = 1;
                newFaceSet.IsMotionBlur = false;
            }
            else if ((faceset.Flags & FLVER2.FaceSet.FSFlags.LodLevel2) > 0)
            {
                newFaceSet.LOD = 2;
                newFaceSet.IsMotionBlur = false;
            }
            else if ((faceset.Flags & FLVER2.FaceSet.FSFlags.LodLevelEx) > 0)
            {
                newFaceSet.LOD = 3;
                newFaceSet.IsMotionBlur = false;
            }

            if ((faceset.Flags & FLVER2.FaceSet.FSFlags.MotionBlur) > 0)
            {
                newFaceSet.IsMotionBlur = true;
            }

            if (is32bit)
            {
                for (var i = 0; i < faceset.Indices.Count; i++)
                {
                    if (faceset.Indices[i] == 0xFFFF && faceset.Indices[i] > mesh.Vertices.Count)
                    {
                        fs32[newFaceSet.IndexOffset + i] = -1;
                    }
                    else
                    {
                        fs32[newFaceSet.IndexOffset + i] = faceset.Indices[i];
                    }
                }
            }
            else
            {
                for (var i = 0; i < faceset.Indices.Count; i++)
                {
                    if (faceset.Indices[i] == 0xFFFF && faceset.Indices[i] > mesh.Vertices.Count)
                    {
                        fs16[newFaceSet.IndexOffset + i] = 0xFFFF;
                    }
                    else
                    {
                        fs16[newFaceSet.IndexOffset + i] = (ushort)faceset.Indices[i];
                    }
                }
            }

            dest.MeshFacesets.Add(newFaceSet);
            idxoffset += faceset.Indices.Count;
        }

        if (mesh.Dynamic == 0)
        {
            dest.UseNormalWBoneTransform = mesh.VertexBuffers
                .SelectMany(b => Flver.BufferLayouts[b.LayoutIndex])
                .Any(e => e.Semantic == FLVER.LayoutSemantic.Normal &&
                    (e.Type == FLVER.LayoutType.UByte4 || e.Type == FLVER.LayoutType.Byte4E));

            if (dest.UseNormalWBoneTransform)
            {
                MovePickingVerticesToNormalWNodes(pvhandle, meshVertices, dest.Material);
            }
        }

        SanitizeIndices(dest, fs16, fs32);
        AddDetailStats(dest, pvhandle, fs16, fs32);
        RetainVertices(dest, meshVertices, vbuffersize, vSize);

        dest.GeomBuffer.UnmapVBuffer();
        dest.GeomBuffer.UnmapIBuffer();

        dest.Bounds = BoundingBox.CreateFromPoints((Vector3*)dest.PickingVertices.ToPointer(), dest.VertexCount, 12,
            Quaternion.Identity, Vector3.Zero, Vector3.One);

        //if (CaptureMaterialLayouts)
        //{
        //    lock (_matLayoutLock)
        //    {
        //        if (!MaterialLayouts.ContainsKey(dest.Material.MaterialName))
        //        {
        //            MaterialLayouts.Add(dest.Material.MaterialName,
        //                Flver.BufferLayouts[mesh.VertexBuffers[0].LayoutIndex]);
        //        }
        //    }
        //}

        if (mesh.Dynamic == 0)
        {
            if (dest.UseNormalWBoneTransform)
            {
                dest.Material.SetNormalWBoneTransform();
            }
            else if (Bones != null && mesh.NodeIndex != -1 && mesh.NodeIndex < Bones.Count)
            {
                dest.LocalTransform = Utils.GetBoneWorldMatrix(Bones[mesh.NodeIndex], Bones);
            }
        }

        Marshal.FreeHGlobal(dest.PickingVertices);
    }

    private static Matrix4x4 GetBoneWorldMatrix(List<FlverBone> bones, FlverBone bone)
    {
        Matrix4x4 matrix = bone.ComputeLocalTransform();
        while (bone.parentIndex != -1)
        {
            bone = bones[bone.parentIndex];
            matrix *= bone.ComputeLocalTransform();
        }

        return matrix;
    }

    /// <summary>
    /// The vertices of a static mesh whose normal W picks its node are in that node's space, and the shader moves each
    /// by its node's world matrix (GetBoneMatrix in FlverShader.vert). Moves the picking vertices the same way, so the
    /// bounds fit what is drawn: the plants of AEG099_720 are stored at about 80 times their size under nodes that
    /// scale them down, which framed and culled the model by bounds far bigger than it.
    /// Reads the node index back from the vertices as filled, as the shader does (uint(normal.w) &amp; 0xFF).
    /// </summary>
    private unsafe void MovePickingVerticesToNormalWNodes(Span<Vector3> pickingVerts, nint meshVertices, FlverMaterial material)
    {
        Matrix4x4[] nodes = StaticBoneTransforms;
        if (nodes.All(m => m.IsIdentity))
        {
            return;
        }

        var stride = (int)material.VertexSize;
        // The normal follows the position, and the first UV in every layout but the sky's (see MeshLayouts)
        var normalW = material.LayoutType == MeshLayoutType.LayoutSky
            ? sizeof(Vector3) + 3
            : sizeof(Vector3) + (2 * sizeof(short)) + 3;

        var vertices = (byte*)meshVertices;
        for (var i = 0; i < pickingVerts.Length; i++)
        {
            int node = vertices[(i * stride) + normalW];
            if (node < nodes.Length)
            {
                pickingVerts[i] = Vector3.Transform(pickingVerts[i], nodes[node]);
            }
        }
    }

    /// <summary>
    /// Which models keep a copy of their vertices as they were sent to the GPU, by lookup name (e.g. "fg_a_0100"),
    /// so their positions can be rewritten later (the FaceGen Editor's morphs, through
    /// <see cref="FlverSubmesh.RetainedVertices"/>). Null: none do.
    /// </summary>
    public static Func<string, bool>? RetainVerticesFor;

    private unsafe void RetainVertices(FlverSubmesh dest, nint meshVertices, uint size, uint stride)
    {
        if (RetainVerticesFor == null || Context == null || !RetainVerticesFor(Context.Name) || size == 0)
            return;

        var copy = new byte[size];
        Marshal.Copy(meshVertices, copy, 0, (int)size);

        dest.RetainedVertices = copy;
        dest.VertexStride = stride;
    }

    private unsafe void ProcessMesh(ref FlverMesh mesh, BinaryReaderEx br, int version,
        Span<FlverVertexBuffer> buffers, Span<FlverBufferLayout> layouts,
        Span<FlverFaceset> facesets, FlverSubmesh dest)
    {
        if (GPUMaterials == null)
            return;

        dest.Material = GPUMaterials[mesh.materialIndex];

        Span<int> facesetIndices = stackalloc int[mesh.facesetCount];
        br.StepIn(mesh.facesetIndicesOffset);
        for (var i = 0; i < mesh.facesetCount; i++)
        {
            facesetIndices[i] = br.ReadInt32();
        }

        br.StepOut();

        Span<int> vertexBufferIndices = stackalloc int[mesh.vertexBufferCount];
        br.StepIn(mesh.vertexBufferIndicesOffset);
        for (var i = 0; i < mesh.vertexBufferCount; i++)
        {
            vertexBufferIndices[i] = br.ReadInt32();
        }

        br.StepOut();
        var vertexCount = mesh.vertexBufferCount > 0 ? buffers[vertexBufferIndices[0]].vertexCount : 0;

        var vSize = dest.Material.VertexSize;
        dest.PickingVertices = Marshal.AllocHGlobal(vertexCount * sizeof(Vector3));
        Span<Vector3> pvhandle = new(dest.PickingVertices.ToPointer(), vertexCount);

        // TODO EDGE
        var is32bit = version > 0x20005 && vertexCount > 65535;
        bool isEdgeCompressed = false;
        var indicesTotal = 0;
        foreach (var fsidx in facesetIndices)
        {
            is32bit = (is32bit || facesets[fsidx].indexSize > 16) && !isEdgeCompressed;
            bool edgeCompressed = (facesets[fsidx].flags & FLVER2.FaceSet.FSFlags.EdgeCompressed) > 0;
            isEdgeCompressed = isEdgeCompressed || edgeCompressed;

            // TODO EDGE
            // Get the true index count by going to each edge member and grabbing its EdgeGeomSpuConfigInfo numIndexes.
            if (edgeCompressed)
            {
                FlverFaceset faceset = facesets[fsidx];
                br.StepIn(faceset.indicesOffset);
                indicesTotal += GetEdgeMembersIndexCount(br);
                br.StepOut();
            }
            else
            {
                indicesTotal += facesets[fsidx].indexCount;
            }
        }

        var vbuffersize = (uint)vertexCount * vSize;
        dest.GeomBuffer = SceneRenderer.GeometryBufferAllocator.Allocate(vbuffersize,
            (uint)indicesTotal * (is32bit ? 4u : 2u), (int)vSize, 4);
        var meshVertices = dest.GeomBuffer.MapVBuffer();
        var meshIndices = dest.GeomBuffer.MapIBuffer();

        var meshBoneIndices = mesh.GetMeshBoneIndices(br);
        bool posfilled = false;

        int maxLayoutMemberCount = 0;
        foreach (var vbi in vertexBufferIndices)
        {
            int layoutIndex = buffers[vbi].layoutIndex;
            maxLayoutMemberCount = Math.Max(maxLayoutMemberCount, layouts[layoutIndex].memberCount);
        }

        Span<FlverBufferLayoutMember> layoutmembersBuffer = stackalloc FlverBufferLayoutMember[maxLayoutMemberCount];

        // A cutscene vertex animation's column can be in any of the buffers, so it is found across all of them first
        var vatColumnMember = -1;
        if (dest.Material!.Definition?.Deformer == MaterialDeformer.VatFrame)
        {
            foreach (var vbi in vertexBufferIndices)
            {
                FlverBufferLayout columnLayout = layouts[buffers[vbi].layoutIndex];
                br.StepIn(columnLayout.membersOffset);
                for (var i = 0; i < columnLayout.memberCount; i++)
                {
                    var member = new FlverBufferLayoutMember(br);
                    if (IsVatFrameColumnCandidate(member.semantic, member.type, member.index))
                    {
                        vatColumnMember = Math.Max(vatColumnMember, member.index);
                    }
                }

                br.StepOut();
            }
        }

        var firstBuffer = true;
        var uvSetsFilled = 0;
        var uvSetsSeen = 0;

        foreach (var vbi in vertexBufferIndices)
        {
            FlverVertexBuffer vb = buffers[vbi];
            FlverBufferLayout layout = layouts[vb.layoutIndex];
            var layoutmembers = layoutmembersBuffer.Slice(0, layout.memberCount);

            br.StepIn(layout.membersOffset);
            for (var i = 0; i < layout.memberCount; i++)
            {
                layoutmembers[i] = new FlverBufferLayoutMember(br);
                if (layoutmembers[i].semantic == FLVER.LayoutSemantic.Normal &&
                    (layoutmembers[i].type == FLVER.LayoutType.Color ||
                     layoutmembers[i].type == FLVER.LayoutType.UByte4 ||
                     layoutmembers[i].type == FLVER.LayoutType.Byte4E))
                {
                    dest.UseNormalWBoneTransform = true;
                }
            }

            br.StepOut();

            // Skip edge compressed buffers
            if (vb.edgeCompressed
            && layoutmembers.Length == 1
            && layoutmembers[0].type == LayoutType.EdgeCompressed
            && layoutmembers[0].semantic == LayoutSemantic.Position
            && posfilled)
            {
                continue;
            }

            if (HasSpeedTreeVertices)
            {
                FillVerticesSpeedTree(br, ref vb, layoutmembers, pvhandle, meshVertices, dest.Material);
            }
            else if (dest.Material.LayoutType == MeshLayoutType.LayoutSky)
            {
                FillVerticesNormalOnly(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, ref posfilled);
            }
            else if (dest.Material.LayoutType == MeshLayoutType.LayoutUV2)
            {
                FillVerticesUV2(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000E ? 2048 : 1024, firstBuffer, ref uvSetsFilled, dest.Material.MaxUvSets);
            }
            else if (dest.Material.LayoutType == MeshLayoutType.LayoutUV3)
            {
                // A GXFlver mask blend, its mask's UV set in the third slot
                FillVerticesUV3(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000E ? 2048 : 1024, firstBuffer,
                    ref uvSetsFilled, ref uvSetsSeen, dest.Material.MaxUvSets, dest.Material.ThirdUvIndex, dest.Material.OpacityFadeUvIndex);
            }
            else if (dest.Material.LayoutType == MeshLayoutType.LayoutDeform)
            {
                FillVerticesDeform(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000E ? 2048 : 1024, firstBuffer, ref uvSetsFilled, dest.Material.MaxUvSets, dest.Material.BlendVertexAttribute, vatColumnMember);
            }
            else
            {
                FillVerticesStandard(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000E ? 2048 : 1024, firstBuffer,
                    dest.Material.Definition?.ReadsVertexColor == true);
            }

            firstBuffer = false;
        }
        dest.VertexCount = vertexCount;
        dest.MeshFacesets = new List<FlverSubmesh.FlverSubmeshFaceSet>();

        Span<ushort> fs16 = null;
        Span<int> fs32 = null;
        if (is32bit)
        {
            fs32 = new Span<int>(meshIndices.ToPointer(), indicesTotal);
        }
        else
        {
            fs16 = new Span<ushort>(meshIndices.ToPointer(), indicesTotal);
        }

        var idxoffset = 0;
        foreach (var fsidx in facesetIndices)
        {
            FlverFaceset faceset = facesets[fsidx];
            if (faceset.indexCount == 0)
            {
                continue;
            }

            //At this point they use 32-bit faceset vertex indices
            FlverSubmesh.FlverSubmeshFaceSet newFaceSet = new()
            {
                BackfaceCulling = faceset.cullBackfaces,
                IsTriangleStrip = faceset.triangleStrip,
                IndexOffset = idxoffset,
                IndexCount = faceset.indexCount,
                Is32Bit = is32bit,
                PickingIndicesCount = 0
            };


            if ((faceset.flags & FLVER2.FaceSet.FSFlags.LodLevel1) > 0)
            {
                newFaceSet.LOD = 1;
                newFaceSet.IsMotionBlur = false;
            }
            else if ((faceset.flags & FLVER2.FaceSet.FSFlags.LodLevel2) > 0)
            {
                newFaceSet.LOD = 2;
                newFaceSet.IsMotionBlur = false;
            }
            else if ((faceset.flags & FLVER2.FaceSet.FSFlags.LodLevelEx) > 0)
            {
                newFaceSet.LOD = 3;
                newFaceSet.IsMotionBlur = false;
            }

            if ((faceset.flags & FLVER2.FaceSet.FSFlags.MotionBlur) > 0)
            {
                newFaceSet.IsMotionBlur = true;
            }

            // TODO EDGE
            // Prevent repeating reading edge members again
            if ((faceset.flags & FLVER2.FaceSet.FSFlags.EdgeCompressed) > 0)
            {
                br.StepIn(faceset.indicesOffset);
                if (is32bit)
                {
                    newFaceSet.IndexCount = GetDecompressedEdgeIndexes32(br, fs32, ref idxoffset);
                }
                else
                {
                    newFaceSet.IndexCount = GetDecompressedEdgeIndexes16(br, fs16, ref idxoffset);
                }
                br.StepOut();

                dest.MeshFacesets.Add(newFaceSet);
                continue;
            }

            br.StepIn(faceset.indicesOffset);
            for (var i = 0; i < faceset.indexCount; i++)
            {
                if (faceset.indexSize == 16)
                {
                    var idx = br.ReadUInt16();
                    if (is32bit)
                    {
                        fs32[newFaceSet.IndexOffset + i] = idx == 0xFFFF ? -1 : idx;
                    }
                    else
                    {
                        fs16[newFaceSet.IndexOffset + i] = idx;
                    }
                }
                else
                {
                    var idx = br.ReadInt32();
                    if (idx >= vertexCount)
                    {
                        fs32[newFaceSet.IndexOffset + i] = -1;
                    }
                    else
                    {
                        fs32[newFaceSet.IndexOffset + i] = idx;
                    }
                }
            }

            br.StepOut();

            dest.MeshFacesets.Add(newFaceSet);
            idxoffset += newFaceSet.IndexCount;
        }

        if (mesh.dynamic == 0 && dest.UseNormalWBoneTransform)
        {
            MovePickingVerticesToNormalWNodes(pvhandle, meshVertices, dest.Material);
        }

        SanitizeIndices(dest, fs16, fs32);
        AddDetailStats(dest, pvhandle, fs16, fs32);
        RetainVertices(dest, meshVertices, vbuffersize, vSize);

        dest.GeomBuffer.UnmapIBuffer();
        dest.GeomBuffer.UnmapVBuffer();

        dest.Bounds = BoundingBox.CreateFromPoints((Vector3*)dest.PickingVertices.ToPointer(), dest.VertexCount, 12,
            Quaternion.Identity, Vector3.Zero, Vector3.One);

        /*if (CaptureMaterialLayouts)
        {
            lock (_matLayoutLock)
            {
                if (!MaterialLayouts.ContainsKey(dest.Material.MaterialName))
                {
                    MaterialLayouts.Add(dest.Material.MaterialName, Flver.BufferLayouts[mesh.VertexBuffers[0].LayoutIndex]);
                }
            }
        }*/

        if (mesh.dynamic == 0)
        {
            if (dest.UseNormalWBoneTransform)
            {
                dest.Material.SetNormalWBoneTransform();
            }
            else if (FBones != null && mesh.defaultBoneIndex != -1 && mesh.defaultBoneIndex < FBones.Count)
            {
                dest.LocalTransform = GetBoneWorldMatrix(FBones, FBones[mesh.defaultBoneIndex]);
            }
        }

        Marshal.FreeHGlobal(dest.PickingVertices);
    }

    private int GetEdgeMembersIndexCount(BinaryReaderEx br)
    {
        EdgeMemberGroup edgeGroup = new EdgeMemberGroup(br);
        Span<EdgeMember> edgeMembers = stackalloc EdgeMember[edgeGroup.memberCount];

        int indexTotal = 0;
        for (int i = 0; i < edgeGroup.memberCount; i++)
        {
            edgeMembers[i] = new EdgeMember(br);
            indexTotal += edgeMembers[i].spuConfigInfo.numIndexes;
        }

        return indexTotal;
    }

    private int GetDecompressedEdgeIndexes16(BinaryReaderEx br, Span<ushort> indexes, ref int indexesOffset)
    {
        long start = br.Position;
        EdgeMemberGroup edgeGroup = new EdgeMemberGroup(br);
        Span<EdgeMember> edgeMembers = stackalloc EdgeMember[edgeGroup.memberCount];
        for (int i = 0; i < edgeGroup.memberCount; i++)
        {
            edgeMembers[i] = new EdgeMember(br);
        }

        int indexCount = 0;
        for (int i = 0; i < edgeGroup.memberCount; i++)
        {
            var edgeMember = edgeMembers[i];
            br.Position = start + edgeMember.edgeIndexesOffset;

            ushort[] memberIndexes = EdgeGeom.DecompressIndexes(br, edgeMember.spuConfigInfo.numIndexes);

            for (int j = 0; j < edgeMember.spuConfigInfo.numIndexes; j++)
            {
                indexes[indexesOffset + j] = (ushort)(memberIndexes[j] + edgeMember.baseIndex);
            }
            indexesOffset += memberIndexes.Length;
            indexCount += edgeMember.spuConfigInfo.numIndexes;
        }

        return indexCount;
    }

    private int GetDecompressedEdgeIndexes32(BinaryReaderEx br, Span<int> indexes, ref int indexesOffset)
    {
        long start = br.Position;
        EdgeMemberGroup edgeGroup = new EdgeMemberGroup(br);
        Span<EdgeMember> edgeMembers = stackalloc EdgeMember[edgeGroup.memberCount];
        for (int i = 0; i < edgeGroup.memberCount; i++)
        {
            edgeMembers[i] = new EdgeMember(br);
        }

        int indexCount = 0;
        for (int i = 0; i < edgeGroup.memberCount; i++)
        {
            var edgeMember = edgeMembers[i];
            br.Position = start + edgeMember.edgeIndexesOffset;
            ushort[] memberIndexes = EdgeGeom.DecompressIndexes(br, edgeMember.spuConfigInfo.numIndexes);

            for (int j = 0; j < edgeMember.spuConfigInfo.numIndexes; j++)
            {
                indexes[indexesOffset + j] = memberIndexes[j] + edgeMember.baseIndex;
            }

            indexesOffset += memberIndexes.Length;
            indexCount += edgeMember.spuConfigInfo.numIndexes;
        }

        return indexCount;
    }

    private bool LoadInternalDeS(AccessLevel al)
    {
        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            return false;

        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return true;
        }

        if (FlverDeS == null)
            return false;

        if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
        {
            GPUMeshes = new FlverSubmesh[FlverDeS.Meshes.Count()];
            GPUMaterials = new FlverMaterial[FlverDeS.Materials.Count()];
            Bounds = new BoundingBox();
            ResetDetailStats();
            Bones = FlverDeS.Nodes;
            // World transforms: a rigid vertex under a nested node needs every parent's offset as well
            BoneTransforms = new List<Matrix4x4>();
            for (var i = 0; i < Bones.Count; i++)
            {
                BoneTransforms.Add(Utils.GetBoneWorldMatrix(Bones[i], Bones));
            }

            for (var i = 0; i < FlverDeS.Materials.Count(); i++)
            {
                GPUMaterials[i] = new FlverMaterial();
                ProcessMaterial(FlverDeS.Materials[i], GPUMaterials[i]);
            }

            for (var i = 0; i < FlverDeS.Meshes.Count(); i++)
            {
                GPUMeshes[i] = new FlverSubmesh();

                FLVER0.Mesh? flverMesh = FlverDeS.Meshes[i];
                BeginUVReport();
                ProcessMesh(flverMesh, GPUMeshes[i]);
                EndUVReport(i, GPUMeshes[i]);
                if (i == 0)
                {
                    Bounds = GPUMeshes[i].Bounds;
                }
                else
                {
                    Bounds = BoundingBox.Combine(Bounds, GPUMeshes[i].Bounds);
                }
            }

            BoneTransforms.Clear();
        }

        if (GPUMaterials != null && GPUMaterials.Any(e => e.GetNormalWBoneTransform()))
        {
            if (Bones != null)
            {
                StaticBoneBuffer = SceneRenderer.BoneBufferAllocator.Allocate(64 * (uint)Bones.Count, 64);
                var tbones = new Matrix4x4[Bones.Count];
                for (var i = 0; i < Bones.Count; i++)
                {
                    tbones[i] = Utils.GetBoneWorldMatrix(Bones[i], Bones);
                }

                SceneRenderer.AddBackgroundUploadTask((d, cl) =>
                {
                    StaticBoneBuffer.FillBuffer(cl, tbones);
                });
            }
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            Flver = null;
        }

        return true;
    }

    private bool LoadInternal(AccessLevel al)
    {
        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            return false;

        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return true;
        }

        if (Flver == null)
            return false;

        if (al == AccessLevel.AccessFull || al == AccessLevel.AccessGPUOptimizedOnly)
        {
            IsSpeedtree = Flver.IsSpeedtree();
            GPUMeshes = new FlverSubmesh[Flver.Meshes.Count()];
            GPUMaterials = new FlverMaterial[Flver.Materials.Count()];
            Bounds = new BoundingBox();
            ResetDetailStats();
            Bones = Flver.Nodes;
            StaticBoneTransforms = Bones.Select(b => Utils.GetBoneWorldMatrix(b, Bones)).ToArray();
            _uvPackScale = UvPackScaleOf(Flver.Header.Version);

            for (var i = 0; i < Flver.Materials.Count(); i++)
            {
                GPUMaterials[i] = new FlverMaterial { UvPackScale = _uvPackScale };
                ProcessMaterial(Flver.Materials[i], GPUMaterials[i]);
            }

            for (var i = 0; i < Flver.Meshes.Count(); i++)
            {
                GPUMeshes[i] = new FlverSubmesh();
                BeginUVReport();
                ProcessMesh(Flver.Meshes[i], GPUMeshes[i]);
                PadDeformedBounds(GPUMeshes[i]);
                EndUVReport(i, GPUMeshes[i]);
                if (i == 0)
                {
                    Bounds = GPUMeshes[i].Bounds;
                }
                else
                {
                    Bounds = BoundingBox.Combine(Bounds, GPUMeshes[i].Bounds);
                }
            }

            if (GPUMeshes.Any(e => e.UseNormalWBoneTransform))
            {
                StaticBoneBuffer = SceneRenderer.BoneBufferAllocator.Allocate(64 * (uint)Bones.Count, 64);
                var tbones = StaticBoneTransforms;

                SceneRenderer.AddBackgroundUploadTask((d, cl) =>
                {
                    StaticBoneBuffer.FillBuffer(cl, tbones);
                });
            }
        }

        if (al == AccessLevel.AccessGPUOptimizedOnly)
        {
            Flver = null;
        }

        return true;
    }

    // Read only flver loader designed to be very fast at reading with low memory usage
    private bool LoadInternalFast(BinaryReaderEx br)
    {
        // Builds GPU buffers as it reads, so there is nothing to load into without a renderer
        if (!SceneRenderer.IsInitialized)
            return false;

        // TO FIX: NR flvers don't seem to work with this properly (reads but no rendering)

        // Parse header
        br.BigEndian = false;
        br.AssertASCII("FLVER\0");
        br.BigEndian = br.AssertASCII(["L\0", "B\0"]) == "B\0";
        var version = br.AssertInt32(
                [0x20005, 0x20007, 0x20009, 0x2000B, 0x2000C, 0x2000D, 0x2000E, 0x2000F, 0x20010, 0x20013, 0x20014, 0x20016, 0x20017, 0x2001A, 0x2001B, 0x20021, 0x20022]);
        _uvPackScale = UvPackScaleOf(version);

        var dataOffset = br.ReadUInt32();
        br.ReadInt32(); // Data length

        var dummyCount = br.ReadInt32();
        var materialCount = br.ReadInt32();
        var boneCount = br.ReadInt32();
        var meshCount = br.ReadInt32();
        var vertexBufferCount = br.ReadInt32();

        // Eat bounding boxes because we compute them ourself
        br.ReadVector3(); // min
        br.ReadVector3(); // max

        br.ReadInt32(); // Face count not including motion blur meshes or degenerate faces
        br.ReadInt32(); // Total face count
        int vertexIndicesSize = br.AssertByte([0, 8, 16, 32]);
        var unicode = br.ReadBoolean();
        br.ReadBoolean(); // unknown
        br.ReadBoolean(); // unknown
        br.ReadInt32(); // unknown
        var faceSetCount = br.ReadInt32();
        var bufferLayoutCount = br.ReadInt32();
        var textureCount = br.ReadInt32();
        br.ReadByte(); // unknown
        br.ReadByte(); // unknown

        br.AssertByte(0);
        br.AssertByte(0);

        br.ReadInt32();
        br.AssertInt32(0);

        br.AssertInt16([0, 1, 2, 3, 4, 5]);  // unknown
        var specialModifier = br.AssertInt16([0, -32768]);
        IsSpeedtree = specialModifier == -32768;
        br.AssertInt32(0);
        br.AssertInt32(0);
        br.AssertInt32([0x0, 0x10]);
        br.AssertInt32(0);
        br.AssertInt32(0);

        // Don't care about dummies for now so skip them
        br.Position += dummyCount * 64; // 64 bytes per dummy

        // Materials
        Span<FlverMaterialDef> materials = stackalloc FlverMaterialDef[materialCount];
        for (var i = 0; i < materialCount; i++)
        {
            materials[i] = new FlverMaterialDef(br);
        }

        // bones
        FBones = new List<FlverBone>();
        for (var i = 0; i < boneCount; i++)
        {
            FBones.Add(new FlverBone(br));
        }

        StaticBoneTransforms = FBones.Select(b => GetBoneWorldMatrix(FBones, b)).ToArray();

        // Meshes
        Span<FlverMesh> meshes = stackalloc FlverMesh[meshCount];
        for (var i = 0; i < meshCount; i++)
        {
            meshes[i] = new FlverMesh(br);
        }

        // Facesets
        Span<FlverFaceset> facesets = stackalloc FlverFaceset[faceSetCount];
        for (var i = 0; i < faceSetCount; i++)
        {
            var faceset = new FlverFaceset(br, version, vertexIndicesSize, dataOffset);
            // The LODs of a SpeedTree model are whole meshes of their own, which would be drawn over each other
            if ((!CFG.Current.Viewport_Enable_LOD_Facesets || HasSpeedTreeVertices) &&
                faceset.flags != FLVER2.FaceSet.FSFlags.None &&
                faceset.flags != FLVER2.FaceSet.FSFlags.EdgeCompressed)
            {
                continue;
            }
            facesets[i] = faceset;
        }

        // Vertex buffers
        Span<FlverVertexBuffer> vertexbuffers = stackalloc FlverVertexBuffer[vertexBufferCount];
        for (var i = 0; i < vertexBufferCount; i++)
        {
            vertexbuffers[i] = new FlverVertexBuffer(br, dataOffset);
        }

        // Buffer layouts
        Span<FlverBufferLayout> bufferLayouts = stackalloc FlverBufferLayout[bufferLayoutCount];
        for (var i = 0; i < bufferLayoutCount; i++)
        {
            bufferLayouts[i] = new FlverBufferLayout(br);
        }

        // Textures
        Span<FlverTexture> textures = stackalloc FlverTexture[textureCount];
        for (var i = 0; i < textureCount; i++)
        {
            textures[i] = new FlverTexture(br);
        }

        // Process the materials and meshes
        GPUMeshes = new FlverSubmesh[meshCount];
        GPUMaterials = new FlverMaterial[materialCount];
        Bounds = new BoundingBox();
        ResetDetailStats();
        //Bones = Flver.Bones;

        for (var i = 0; i < materialCount; i++)
        {
            GPUMaterials[i] = new FlverMaterial { UvPackScale = _uvPackScale };
            ProcessMaterial(GPUMaterials[i], br, ref materials[i], textures, unicode);
        }

        for (var i = 0; i < meshCount; i++)
        {
            GPUMeshes[i] = new FlverSubmesh();
            BeginUVReport();
            ProcessMesh(ref meshes[i], br, version, vertexbuffers, bufferLayouts, facesets, GPUMeshes[i]);
            PadDeformedBounds(GPUMeshes[i]);
            EndUVReport(i, GPUMeshes[i]);
            if (i == 0)
            {
                Bounds = GPUMeshes[i].Bounds;
            }
            else
            {
                Bounds = BoundingBox.Combine(Bounds, GPUMeshes[i].Bounds);
            }
        }

        if (GPUMeshes.Any(e => e.UseNormalWBoneTransform))
        {
            StaticBoneBuffer = SceneRenderer.BoneBufferAllocator.Allocate(64 * (uint)FBones.Count, 64);
            var tbones = StaticBoneTransforms;

            SceneRenderer.AddBackgroundUploadTask((d, cl) =>
            {
                StaticBoneBuffer.FillBuffer(cl, tbones);
            });
        }

        return true;
    }

    public class FlverMaterial : IResourceEventListener, IDisposable
    {
        public enum TextureType
        {
            AlbedoTextureResource = 0,
            AlbedoTextureResource2,
            NormalTextureResource,
            NormalTextureResource2,
            SpecularTextureResource,
            SpecularTextureResource2,
            ShininessTextureResource,
            ShininessTextureResource2,
            BlendmaskTextureResource,
            AlbedoTextureResource3,
            AlbedoTextureResource4,
            NormalTextureResource3,
            NormalTextureResource4,
            SpecularTextureResource3,
            SpecularTextureResource4,
            ShininessTextureResource3,
            ShininessTextureResource4,
            EmissiveTextureResource,
            DetailMaskTextureResource,
            DetailAlbedoTextureResource,
            DetailNormalTextureResource,
            MagmaTextureResource,
            VatPositionTextureResource,
            VatNormalTextureResource,
            LightmapTextureResource,
            // The detail masks of blend layers 1 to 3 in the extended form
            LayerDetailMaskTextureResource2,
            LayerDetailMaskTextureResource3,
            LayerDetailMaskTextureResource4,
            // The details stacked over the first (C[DetailBlend]), layers 1 to 3, and the map their weights read
            DetailStackAlbedoTextureResource1,
            DetailStackAlbedoTextureResource2,
            DetailStackAlbedoTextureResource3,
            DetailStackAlbedoTextureResource4,
            DetailStackNormalTextureResource1,
            DetailStackNormalTextureResource2,
            DetailStackNormalTextureResource3,
            DetailStackNormalTextureResource4,
            DetailStackWeightTextureResource,
            // What back faces of a stacked detail are drawn with
            DetailStackBackAlbedoTextureResource,
            DetailStackBackNormalTextureResource,
            // The mask of a ChrCustomize colouring (MaterialCustomize): root and white hair, or where skin is tinted
            CustomizeMaskTextureResource,
            // The skin's beard stubble (MaterialCustomize.SecondTex)
            CustomizeStubbleTextureResource,
            // The skin's two makeup masks (MaterialMakeup)
            CustomizeMakeupMaskTextureResource0,
            CustomizeMakeupMaskTextureResource1,
            // The skin's decal (set at run time, SetCustomizeDecal) and its mask
            CustomizeDecalTextureResource,
            CustomizeDecalMaskTextureResource,
            // The body's burn scar: its albedo, and the normal map whose alpha is its shape
            CustomizeBurnTextureResource,
            CustomizeBurnNormalTextureResource,
            TextureResourceCount
        }

        // Indices of the default textures, see SceneRenderer
        private const ushort BlackTexture = 2;
        private const ushort WhiteTexture = 3;

        public readonly bool[] TextureResourceFilled = new bool[(int)TextureType.TextureResourceCount];

        public readonly ResourceHandle<TextureResource>?[] TextureResources =
            new ResourceHandle<TextureResource>[(int)TextureType.TextureResourceCount];

        private bool _setNormalWBoneTransform;

        private bool disposedValue;
        public MeshLayoutType LayoutType;
        public GPUBufferAllocator.GPUBufferHandle? MaterialBuffer;
        public Material MaterialData;
        public string? MaterialName;

        /// <summary>The FLVER material's own name (e.g. #11#Hair01), which MQB events name materials by</summary>
        public string? FlverMaterialName;

        public MaterialDefinition? Definition;

        // A cutscene's vertex animation (MaterialDeformer.VatFrame), see SetVertexAnimation
        private ResourceHandle<TextureResource>? _vatFramePosition;
        private ResourceHandle<TextureResource>? _vatFrameNormal;
        private uint _vatFramePositionTex;
        private uint _vatFrameNormalTex;
        private float _vatFramePlay;

        /// <summary>
        /// Plays a cutscene's vertex animation on a <see cref="MaterialDeformer.VatFrame"/> material: the textures of its
        /// offsets and normals, and how far through it is (0 to 1). A null position stops it, leaving the bind pose.
        /// Uploads only when something changed, so it can be called every frame. The caller keeps the textures loaded.
        /// </summary>
        public void SetVertexAnimation(ResourceHandle<TextureResource>? position, ResourceHandle<TextureResource>? normal, float play)
        {
            if (Definition?.Deformer != MaterialDeformer.VatFrame)
            {
                return;
            }

            // A texture that is still loading reads as none, and is picked up once it is on the GPU
            var positionTex = TextureIndexOf(position);
            var normalTex = positionTex == 0 ? 0 : TextureIndexOf(normal);
            play = positionTex == 0 ? 0 : play;

            if (position == _vatFramePosition && normal == _vatFrameNormal && positionTex == _vatFramePositionTex
                && normalTex == _vatFrameNormalTex && play == _vatFramePlay)
            {
                return;
            }

            _vatFramePosition = position;
            _vatFrameNormal = normal;
            _vatFramePositionTex = positionTex;
            _vatFrameNormalTex = normalTex;
            _vatFramePlay = play;
            UpdateMaterial();
        }

        private static uint TextureIndexOf(ResourceHandle<TextureResource>? handle)
        {
            return handle is { IsLoaded: true } && handle.Get() is { GPUTexture: { } texture } ? texture.TexHandle : 0u;
        }

        /// <summary>Uploaded straight after the material</summary>
        public MaterialShading Shading = MaterialShading.Default;

        /// <summary>Uploaded straight after the shading</summary>
        public MaterialEffects Effects = MaterialEffects.Default;

        /// <summary>Set for materials with a deformer, and uploaded straight after the effects</summary>
        public MaterialDeform? Deform;

        /// <summary>Set for materials with a blend recipe, and uploaded straight after the deform entry</summary>
        public MaterialWithBlend? Blend;

        /// <summary>Set for materials with an overlay, and uploaded straight after the blend entries</summary>
        public MaterialDetail? Detail;

        /// <summary>Set for materials with a glow, and uploaded straight after the overlay</summary>
        public MaterialMagma? Magma;

        /// <summary>Set for water, and uploaded straight after the glow</summary>
        public MaterialWater? Water;

        /// <summary>
        ///     Set for ChrCustomize colourings (<see cref="CustomizeMode"/>), and uploaded straight after the effects, in
        ///     the deform entry's place. The FaceGen Editor writes a face's colours into it.
        /// </summary>
        public MaterialCustomize? Customize;

        /// <summary>Uploaded straight after <see cref="Customize"/> (the eye's further values)</summary>
        public MaterialCustomizeExtra CustomizeExtra = MaterialCustomizeExtra.Default;

        /// <summary>The tiling of the customize mask, from its sampler</summary>
        public Vector2 CustomizeMaskTiling = Vector2.One;

        /// <summary>The tiling of the skin's stubble texture, from its sampler</summary>
        public Vector2 CustomizeStubbleTiling = Vector2.One;

        /// <summary>Uploaded after <see cref="CustomizeExtra"/>: the skin's makeup layers. The FaceGen Editor writes a face's into it</summary>
        public MaterialMakeup CustomizeMakeup = MaterialMakeup.Default;

        /// <summary>The tilings of the two makeup masks, from their samplers</summary>
        public Vector4 CustomizeMakeupTiling = Vector4.One;

        /// <summary>xy the decal mask's tiling, zw the decal's, from their samplers</summary>
        public Vector4 CustomizeDecalTiling = Vector4.One;

        /// <summary>xy the burn albedo's tiling, zw the burn normal's, from their samplers</summary>
        public Vector4 CustomizeBurnTiling = Vector4.One;

        /// <summary>
        /// Sets a skin's decal texture (a fg_a_60NN part's, which no material names), or none. The FaceGen Editor
        /// sets it from the face's decal_partsId once the part's textures have loaded.
        /// </summary>
        public void SetCustomizeDecal(ResourceHandle<TextureResource>? handle)
        {
            var slot = (int)TextureType.CustomizeDecalTextureResource;

            if (ReferenceEquals(TextureResources[slot], handle))
                return;

            handle?.Acquire();
            TextureResources[slot]?.Release();
            TextureResources[slot] = handle;
            UpdateMaterial();
        }

        /// <summary>The texture bound in a slot, or null</summary>
        public ResourceHandle<TextureResource>? GetTexture(TextureType slot) => TextureResources[(int)slot];

        /// <summary>
        ///     Puts a texture in a slot at run time, as <see cref="SetCustomizeDecal"/> does: acquires the new handle and
        ///     releases the old one. The slot's own texture, if it loads later, takes the slot back, so a caller keeping
        ///     a texture there sets it again (the FaceGen Editor's female face textures, every frame)
        /// </summary>
        public void SetTexture(TextureType slot, ResourceHandle<TextureResource>? handle)
        {
            if (ReferenceEquals(TextureResources[(int)slot], handle))
                return;

            handle?.Acquire();
            TextureResources[(int)slot]?.Release();
            TextureResources[(int)slot] = handle;
            UpdateMaterial();
        }

        /// <summary>
        ///     A FaceGen skin's face texture (<see cref="MaterialCustomize.ModeFaceGen"/>): its global texture index, set
        ///     at run time by the FaceGen Editor, 0 for none (the material's own albedo then stands in)
        /// </summary>
        public uint CustomizeFaceTexture;

        /// <summary>The decal texture set by <see cref="SetCustomizeDecal"/>, or null</summary>
        public ResourceHandle<TextureResource>? CustomizeDecal => TextureResources[(int)TextureType.CustomizeDecalTextureResource];

        /// <summary>Tiling and scroll of the layers past the second, gathered as textures are found</summary>
        public MaterialLayerTiling LayerTiling = MaterialLayerTiling.Default;
        public MaterialLayerScroll LayerScroll;
        public Vector4 DetailTiling = Vector4.One;
        public Vector2 MagmaTiling = Vector2.One;

        /// <summary>What the material's parameters multiply the first layer's albedo by</summary>
        public Vector3 AlbedoTint = Vector3.One;

        /// <summary>The material has no albedo texture, so <see cref="AlbedoTint"/> is its whole color</summary>
        public bool AlbedoFromColor;

        /// <summary>
        ///     The material samples DS3's lightmaps (g_DOLTexture), which come from the map rather than the model, so it
        ///     is drawn with the second UV set for them (see DolLightmapBank)
        /// </summary>
        public bool UsesLightmap;

        /// <summary>
        ///     A DES multi texture material ([ML], DS_Phn_*MulLit) with a g_Lightmap: its layers read UV 1 and 2 and the
        ///     lightmap UV 3, so it is drawn with the blend_uv3 variant and <see cref="MeshLayoutType.LayoutUV3"/>
        /// </summary>
        public bool DesLightmapThirdUv;

        /// <summary>The extended blend form's per-layer values, uploaded after the water entries (see MaterialWithLayerDetail)</summary>
        public MaterialLayerDetail? LayerDetail;
        public MaterialLayerDetailExtra LayerDetailExtra = MaterialLayerDetailExtra.Default;
        public Vector2[] LayerDetailTiling = [Vector2.One, Vector2.One, Vector2.One];

        /// <summary>
        ///     The layers of a detail stack, uploaded after the extended blend entries (see MaterialWithDetailStack).
        ///     Their values come from ApplyDetail and their tiling from LoadBlendRecipe, the textures from UpdateMaterial
        /// </summary>
        public MaterialDetailStackEntry[] DetailStack =
            [MaterialDetailStackEntry.Default, MaterialDetailStackEntry.Default, MaterialDetailStackEntry.Default, MaterialDetailStackEntry.Default,
                MaterialDetailStackEntry.Default];

        /// <summary>The material has the stack entries (stack layers or a back face), see MaterialDetail.HasStackEntries</summary>
        public bool HasDetailStack;

        /// <summary>The blend layers that are the color of their tint, drawn with a white texture</summary>
        public bool[] ColorOnlyLayers = new bool[Material.MaxLayers];

        /// <summary>The TEXCOORD the blend recipe's vertex weights read, see GetTexCoordIndex</summary>
        public int BlendVertexAttribute => Definition?.Blend != null ? Definition.BlendVertexAttribute | (Definition.BlendVertexAttribute2 << 8) : 1;

        /// <summary>The sampler type of the metallic map drawn in the specular slot, see ChooseMetallic</summary>
        public string? MetallicType;

        /// <summary>The specular slot holds a metallic map rather than a specular color</summary>
        public bool UsesMetallic;

        /// <summary>Drawn with the metallic path (specialization constant 12), which reads row 3 as MetallicOffsetBits</summary>
        public bool MetallicShading;

        /// <summary>
        ///     What each layer adds to its metallic before it is saturated, as half floats. The metallic path puts
        ///     these in row 3 of the material in place of the shininess textures, which these games don't have
        /// </summary>
        public ushort[] MetallicOffsetBits = new ushort[Material.MaxLayers];

        /// <summary>The sampler type of each layer's albedo, which says the layer's UV set, see ConfigureMaterialShader</summary>
        public string?[] LayerAlbedoTypes = new string?[Material.MaxLayers];

        /// <summary>
        /// The FLVER's own tiling of each texture, by its param name, where it is set (not 1,1 or 0). DS2 tiles its
        /// textures here rather than in the MTD (m10_04's ground: grass 30x30, sand 22x22, mask 6x6), so it wins
        /// over the definition's tiling. The other games' FLVERs leave it at 1,1.
        /// </summary>
        public Dictionary<string, Vector2> FlverTiling = new();

        /// <summary>The material has the blend entries for its blend mask's tiling alone, see <see cref="Material.MaskTilingFlag"/></summary>
        public bool MaskTilingEntries;

        /// <summary>
        /// A GXFlver MulMask or DirtMask blend (<see cref="MaterialResolvedMaskBlend"/>), weighed as the game does: the
        /// FLVER UV set of its mask, which goes in the third UV slot (FlverLayoutUV3), or -1 for none
        /// </summary>
        public int MaskBlendUvIndex = -1;

        /// <summary>The material has an OpacityTexture (MaterialTextureUse.Opacity)</summary>
        public bool HasOpacity;

        /// <summary>
        /// DS3's Fujir_*_OpacityBlend (M[ARSN]_al_m_Opacity, M[A]_al_m_NoLight_Opacity): alpha = OpacityTexture1.r *
        /// OpacityTexture2.r * TEXCOORD3.x, from the shaders' disassembly, the albedo on UV set 0, the opacities on
        /// sets 1 and 2 (their MTD UV numbers 2 and 4), the fade on set 3. This is the fade's FLVER UV set: the one after
        /// those the MTD's textures use (a UByte4Norm, 0-0.9), put in the vertex alpha; -1 for none. OpacityTexture1 is
        /// in the blend mask slot read on the second UV slot, OpacityTexture2 in the detail mask slot read on the third
        /// (<see cref="OpacityUvIndex"/>). Their scrolling (GXMD) is left out (Material.OpacityFlag, FlverShader.frag)
        /// </summary>
        public int OpacityFadeUvIndex = -1;

        /// <summary>OpacityTexture2's FLVER UV set, in the third UV slot (<see cref="OpacityFadeUvIndex"/>)</summary>
        public int OpacityUvIndex = -1;

        /// <summary>The FLVER UV set in the third UV slot of FlverLayoutUV3: an opacity's or a mask blend's mask's</summary>
        public int ThirdUvIndex => OpacityUvIndex >= 0 ? OpacityUvIndex : Math.Max(MaskBlendUvIndex, 0);

        /// <summary>The FLVER material's GX01 BlendMaskIntensity, which FC_BlendMaskParam is made from</summary>
        public float MaskBlendIntensity = 1f;

        /// <summary>
        /// FC_BlendMaskParam's x and z from GX01's BlendMaskIntensity I, so that k = mask.r * x + z: centred,
        /// k = I(2 mask - 1). The shader scales the alpha up by 1 + k for k above 0 and pulls it toward k below, so
        /// alpha 0 and 1 stay the pure layers and the centred mask moves the transition either way (BB keeps 3616 of its
        /// blend meshes at alpha 0.5). Inferred from the shader and the data, not read from the game's code
        /// (Claude/Documentation/BB Mask Blend.md)
        /// </summary>
        public static (float X, float Z) MaskBlendParam(float intensity) => (2f * intensity, -intensity);

        public MaterialBlending Blending = MaterialBlending.Opaque;
        public bool DepthWrite = true;

        /// <summary>
        /// Lies on another surface at the same depth, so it is pulled toward the camera to be drawn over it
        /// </summary>
        public bool MeshDecal;

        /// <summary>
        /// Every texture the material lists is a placeholder (<see cref="IsPlaceholderTexture"/>), so
        /// it has nothing of its own to draw: its meshes get no renderable (FlverSubmeshProvider.IndexCount)
        /// </summary>
        public bool NeverDrawn;

        /// <summary>
        /// How many of a mesh's UV sets to read. The last that fits goes in the second UV slot of a vertex,
        /// which is how fur gets its third set without the vertices of everything else growing to hold it.
        /// </summary>
        public int MaxUvSets => Definition?.Fur?.ThirdUv == true ? 3 : 2;

        public int MaterialMask = -1;

        public bool IsBodyHair;

        public string? ShaderName;
        public List<SpecializationConstant>? SpecializationConstants;
        public VertexLayoutDescription VertexLayout;
        public uint VertexSize;

        /// <summary>
        /// What the UVs of this material's FLVER were stored at (FlverResource.UvPackScaleOf): 2048, or 1024 for DES
        /// and DS1 FLVER2s. The vertex shaders divide by it (specialization constant 51)
        /// </summary>
        public float UvPackScale = 2048f;

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>TEMPORARY: set on the materials BbTextureDiag traces</summary>
        public string? DiagTag;

        public void OnResourceLoaded(IResourceHandle handle, int tag)
        {
            var texHandle = (ResourceHandle<TextureResource>)handle;

            // TEMPORARY BB texture diagnostics (BbTextureDiag)
            if (DiagTag != null)
            {
                var gpu = texHandle.Get()?.GPUTexture;
                BbTextureDiag.Write($"LOADED {DiagTag}: slot={(TextureType)tag} '{handle.Name}' gpuTexture={(gpu == null ? "NULL" : $"index {gpu.TexHandle} resident={gpu.Resident}")}");
            }

            texHandle.Acquire();
            TextureResources[tag]?.Release();
            TextureResources[tag] = texHandle;
            UpdateMaterial();
        }

        public void OnResourceUnloaded(IResourceHandle handle, int tag)
        {
            // TEMPORARY BB texture diagnostics (BbTextureDiag)
            if (DiagTag != null)
            {
                BbTextureDiag.Write($"UNLOADED {DiagTag}: slot={(TextureType)tag} '{handle.Name}'");
            }

            TextureResources[tag]?.Release();
            TextureResources[tag] = null;
            UpdateMaterial();
        }

        public bool GetNormalWBoneTransform()
        {
            return _setNormalWBoneTransform;
        }

        public void SetNormalWBoneTransform()
        {
            if (!_setNormalWBoneTransform)
            {
                if (SpecializationConstants != null)
                {
                    SpecializationConstants.Add(new SpecializationConstant(50, true));
                }
                _setNormalWBoneTransform = true;
            }
        }

        /// <summary>Fills in the glow's values, leaving the textures to <see cref="UpdateMaterial"/></summary>
        public void ApplyMagma(MaterialResolvedMagma magma)
        {
            Magma = new MaterialMagma
            {
                Flow0 = new Vector4(magma.Scroll0[0], magma.Scroll0[1], magma.Scale0[0], magma.Scale0[1]),
                Flow1 = new Vector4(magma.Scroll1[0], magma.Scroll1[1], magma.Scale1[0], magma.Scale1[1]),
                Tiling = new Vector4(1, 1, magma.Strength, magma.Power),
                Distort = new Vector4(magma.Distort[0], magma.Distort[1], 0, 0),
                Color = new Vector4(magma.Color[0], magma.Color[1], magma.Color[2], magma.Limit),
                RedSample = (uint)magma.RedSample
            };
        }

        public void ApplyWater(MaterialResolvedWater water)
        {
            Water = new MaterialWater
            {
                DetailTiling = new Vector4(water.DetailTiling[0], water.DetailTiling[1], water.DetailScroll[0], water.DetailScroll[1]),
                DetailBend = new Vector4(water.DetailBend[0], water.DetailBend[1], water.DetailStrength, water.Gloss),
                Tint = new Vector4(water.Tint[0], water.Tint[1], water.Tint[2], water.HasVolume ? 1 : 0),
                Scatter = new Vector4(water.Scatter[0], water.Scatter[1], water.Scatter[2], water.EdgeFade),
                Absorption = new Vector4(water.Absorption[0], water.Absorption[1], water.Absorption[2], 0)
            };
        }

        /// <summary>
        /// Fills in the snow's values, which go where water's would (a material is never both), so everything about
        /// uploading water's entries holds for them too
        /// </summary>
        public void ApplySnow(MaterialResolvedSnow snow)
        {
            MaterialSnow values = new()
            {
                Color = new Vector4(MathF.Pow(snow.Color[0], 2.2f), MathF.Pow(snow.Color[1], 2.2f), MathF.Pow(snow.Color[2], 2.2f), snow.Roughness),
                Surface = new Vector4(snow.Metal, snow.F0, snow.CoverStart, snow.CoverScale),
                Detail = new Vector4(snow.DetailTiling, snow.DetailTiling, snow.DetailStrength, 0),
                Height = MaterialLayeredHeight.Of(snow.HeightTiling, snow.HeightBlend, snow.HeightScroll)
            };

            Water = Unsafe.As<MaterialSnow, MaterialWater>(ref values);
        }

        /// <summary>Fills in DS1R's water's values, which go where water's would, as the snow's do</summary>
        public void ApplyReflectWater(MaterialResolvedReflectWater water)
        {
            static float Linear(float x) => MathF.Pow(MathF.Max(x, 0), 2.2f);

            MaterialReflectWater values = new()
            {
                Color = new Vector4(Linear(water.Color[0]), Linear(water.Color[1]), Linear(water.Color[2]), water.Color[3]),
                Fresnel = new Vector4(water.FresnelPower, water.FresnelBias, water.FresnelScale, water.SpecularPower),
                FresnelColor = new Vector4(Linear(water.FresnelColor[0]), Linear(water.FresnelColor[1]), Linear(water.FresnelColor[2]), water.WaveHeight),
                Height = MaterialLayeredHeight.Of(water.HeightTiling, water.HeightBlend, water.HeightScroll)
            };
            values.Height.Extra0 = water.FadeBegin;

            Water = Unsafe.As<MaterialReflectWater, MaterialWater>(ref values);
        }

        /// <summary>Fills in the overlay's values, leaving the textures to <see cref="UpdateMaterial"/></summary>
        public void ApplyDetail(MaterialResolvedDetail detail)
        {
            Detail = new MaterialDetail
            {
                Tiling = Vector4.One,
                ColorStrength = new Vector4(detail.Color[0], detail.Color[1], detail.Color[2], detail.NormalStrength),
                // See MaterialDetail: how the albedo is laid over, and the strength of an overlay
                Pad0 = (detail.Overlay ? MaterialDetail.AlbedoOverlay : 0)
                    | ((uint)detail.Stack.Count << MaterialDetail.StackCountShift)
                    | (detail.HasStackEntries ? MaterialDetail.HasStackEntries : 0),
                Pad1 = BitConverter.SingleToUInt32Bits(detail.OverlayStrength)
            };

            HasDetailStack = detail.HasStackEntries;
            for (var i = 0; i < detail.Stack.Count; i++)
            {
                MaterialResolvedDetailLayer layer = detail.Stack[i];
                DetailStack[i].Color = new Vector4(layer.Color[0], layer.Color[1], layer.Color[2], 1);
                DetailStack[i].Weight = new Vector4(layer.Scale, layer.Bias, layer.Power, layer.Channel
                    + (layer.Invert ? MaterialDetailStackEntry.Inverted : 0) + (layer.Replace ? MaterialDetailStackEntry.Replacing : 0)
                    + (layer.OwnAlpha ? MaterialDetailStackEntry.OwnAlpha : 0));
            }

            DetailStack[MaterialResolvedDetail.MaxStackLayers].Color = new Vector4(detail.BackColor[0], detail.BackColor[1], detail.BackColor[2], 1);
        }

        /// <summary>Fills in the recipe's values, leaving the textures to <see cref="UpdateMaterial"/></summary>
        public void ApplyBlend(MaterialResolvedBlend blend)
        {
            var t = blend.Tints;

            Blend = new MaterialWithBlend
            {
                Blend = new MaterialBlend
                {
                    Sharpness = new Vector4(blend.Sharpness[0], blend.Sharpness[1], blend.Sharpness[2], blend.DetailBias),
                    Detail = new Vector4(blend.Detail[0], blend.Detail[1], blend.Detail[2], blend.Scale),
                    Source1 = blend.Sources[0],
                    Source2 = blend.Sources[1],
                    Source3 = blend.Sources[2]
                },
                LayerTint = new MaterialLayerTint
                {
                    Tint2 = new Vector4(t[1][0], t[1][1], t[1][2], t[0][0]),
                    Tint3 = new Vector4(t[2][0], t[2][1], t[2][2], t[0][1]),
                    Tint4 = new Vector4(t[3][0], t[3][1], t[3][2], t[0][2])
                }
            };

            if (blend.Extended)
            {
                Vector4 Channels(int i) => new(blend.LayerChannels[i][0], blend.LayerChannels[i][1], blend.LayerChannels[i][2], blend.LayerChannels[i][3]);
                Vector4 Extra(int i) => new(blend.LayerBias[i], blend.LayerSoftScale[i], LayerDetailTiling[i].X, LayerDetailTiling[i].Y);

                LayerDetail = new MaterialLayerDetail { Channels2 = Channels(0), Channels3 = Channels(1), Channels4 = Channels(2) };
                LayerDetailExtra = new MaterialLayerDetailExtra { Layer2 = Extra(0), Layer3 = Extra(1), Layer4 = Extra(2) };
            }
            else if (blend.Adjusted)
            {
                // The [MbN_ColorAdjust] form in the same two entries (FlverShader.frag, SOURCE_ADJUSTED). Of each layer's
                // channels, x is its color adjust; the bottom's is the first's y, and the second's yzw the snow's facing
                // direction. Of each layer's extra, x is its hard mix, y its weight scale, z its own detail bias, w its
                // facing bias
                var a = blend.ColorAdjust;
                LayerDetail = new MaterialLayerDetail
                {
                    Channels2 = new Vector4(a[1], a[0], 0, 0),
                    Channels3 = new Vector4(a[2], blend.FacingDirection[0], blend.FacingDirection[1], blend.FacingDirection[2]),
                    Channels4 = new Vector4(a[3], 0, 0, 0)
                };
                LayerDetailExtra = new MaterialLayerDetailExtra
                {
                    Layer2 = new Vector4(blend.LayerHardMix[0], blend.LayerWeightScale[0], blend.LayerBiasOverride[0], blend.LayerFacingBias[0]),
                    Layer3 = new Vector4(blend.LayerHardMix[1], blend.LayerWeightScale[1], blend.LayerBiasOverride[1], blend.LayerFacingBias[1]),
                    Layer4 = new Vector4(blend.LayerHardMix[2], blend.LayerWeightScale[2], blend.LayerBiasOverride[2], blend.LayerFacingBias[2])
                };
                Blend = Blend.Value with { Blend = Blend.Value.Blend with { Source1 = Blend.Value.Blend.Source1 | MaterialResolvedBlend.SourceAdjusted } };
            }
        }

        /// <summary>
        /// Applies what the sampler chosen for a texture slot says about it. Only the slots of the first
        /// two layers have tiling, as those are the only ones a material can blend, and only the
        /// first layer has color and scrolling.
        /// </summary>
        public void ApplySampler(TextureType textureType, string? samplerType, string? flverType = null)
        {
            MaterialSampler? sampler = Definition?.Samplers.Find(x => x.Type == samplerType);
            Vector2 flverTiling = default;
            var hasFlverTiling = flverType != null && FlverTiling.TryGetValue(flverType, out flverTiling);

            if (!hasFlverTiling && (sampler == null || sampler.Tiling.Length < 2))
                return;

            float[] tiling = hasFlverTiling ? [flverTiling.X, flverTiling.Y] : sampler!.Tiling;
            var scroll = sampler?.Scroll ?? [0, 0];

            switch (textureType)
            {
                case TextureType.AlbedoTextureResource:
                    Shading.Tiling.X = tiling[0];
                    Shading.Tiling.Y = tiling[1];
                    Effects.Scroll.X = scroll[0];
                    Effects.Scroll.Y = scroll[1];

                    if (sampler?.Color != null)
                    {
                        // The game saturates this, unlike the color of light given off
                        AlbedoTint = Vector3.Min(new Vector3(sampler.Color[0], sampler.Color[1], sampler.Color[2]), Vector3.One);
                    }
                    break;
                case TextureType.NormalTextureResource:
                    Shading.Tiling.Z = tiling[0];
                    Shading.Tiling.W = tiling[1];
                    Effects.Scroll.Z = scroll[0];
                    Effects.Scroll.W = scroll[1];
                    break;
                case TextureType.EmissiveTextureResource:
                    Effects.EmissiveTiling = new Vector4(tiling[0], tiling[1], scroll[0], scroll[1]);

                    if (sampler?.Color != null)
                    {
                        Effects.EmissiveColor = new Vector4(sampler.Color[0], sampler.Color[1], sampler.Color[2], 0);
                    }

                    // A mask whose red is the glow (MaterialEmissiveMask): w -1 has the shader take red for all three.
                    // Fur keeps its dither width in w, which is never below 0
                    if (samplerType == Definition?.EmissiveMaskType)
                    {
                        Effects.EmissiveColor.W = -1;
                    }
                    break;
                case TextureType.MagmaTextureResource:
                    MagmaTiling.X = tiling[0];
                    MagmaTiling.Y = tiling[1];
                    break;
                case TextureType.CustomizeMaskTextureResource:
                    CustomizeMaskTiling = new Vector2(tiling[0], tiling[1]);
                    break;
                case TextureType.CustomizeStubbleTextureResource:
                    CustomizeStubbleTiling = new Vector2(tiling[0], tiling[1]);
                    break;
                case TextureType.CustomizeMakeupMaskTextureResource0:
                    CustomizeMakeupTiling.X = tiling[0];
                    CustomizeMakeupTiling.Y = tiling[1];
                    break;
                case TextureType.CustomizeMakeupMaskTextureResource1:
                    CustomizeMakeupTiling.Z = tiling[0];
                    CustomizeMakeupTiling.W = tiling[1];
                    break;
                case TextureType.CustomizeDecalMaskTextureResource:
                    CustomizeDecalTiling.X = tiling[0];
                    CustomizeDecalTiling.Y = tiling[1];
                    break;
                case TextureType.CustomizeDecalTextureResource:
                    CustomizeDecalTiling.Z = tiling[0];
                    CustomizeDecalTiling.W = tiling[1];
                    break;
                case TextureType.CustomizeBurnTextureResource:
                    CustomizeBurnTiling.X = tiling[0];
                    CustomizeBurnTiling.Y = tiling[1];
                    break;
                case TextureType.CustomizeBurnNormalTextureResource:
                    CustomizeBurnTiling.Z = tiling[0];
                    CustomizeBurnTiling.W = tiling[1];
                    break;
                case TextureType.DetailAlbedoTextureResource:
                    DetailTiling.X = tiling[0];
                    DetailTiling.Y = tiling[1];
                    break;
                case TextureType.DetailNormalTextureResource:
                    DetailTiling.Z = tiling[0];
                    DetailTiling.W = tiling[1];
                    break;
                case TextureType.BlendmaskTextureResource:
                    LayerTiling.MaskTiling.X = tiling[0];
                    LayerTiling.MaskTiling.Y = tiling[1];
                    break;
                case TextureType.DetailMaskTextureResource:
                    LayerTiling.MaskTiling.Z = tiling[0];
                    LayerTiling.MaskTiling.W = tiling[1];
                    break;
                case TextureType.AlbedoTextureResource3:
                    LayerTiling.Tiling3.X = tiling[0];
                    LayerTiling.Tiling3.Y = tiling[1];
                    LayerScroll.Scroll3.X = scroll[0];
                    LayerScroll.Scroll3.Y = scroll[1];
                    break;
                case TextureType.NormalTextureResource3:
                    LayerTiling.Tiling3.Z = tiling[0];
                    LayerTiling.Tiling3.W = tiling[1];
                    LayerScroll.Scroll3.Z = scroll[0];
                    LayerScroll.Scroll3.W = scroll[1];
                    break;
                case TextureType.AlbedoTextureResource4:
                    LayerTiling.Tiling4.X = tiling[0];
                    LayerTiling.Tiling4.Y = tiling[1];
                    LayerScroll.Scroll4.X = scroll[0];
                    LayerScroll.Scroll4.Y = scroll[1];
                    break;
                case TextureType.NormalTextureResource4:
                    LayerTiling.Tiling4.Z = tiling[0];
                    LayerTiling.Tiling4.W = tiling[1];
                    LayerScroll.Scroll4.Z = scroll[0];
                    LayerScroll.Scroll4.W = scroll[1];
                    break;
                case TextureType.AlbedoTextureResource2:
                    Shading.Tiling2.X = tiling[0];
                    Shading.Tiling2.Y = tiling[1];
                    LayerScroll.Scroll2.X = scroll[0];
                    LayerScroll.Scroll2.Y = scroll[1];
                    break;
                case TextureType.NormalTextureResource2:
                    Shading.Tiling2.Z = tiling[0];
                    Shading.Tiling2.W = tiling[1];
                    LayerScroll.Scroll2.Z = scroll[0];
                    LayerScroll.Scroll2.W = scroll[1];
                    break;
            }
        }

        private void SetMaterialTexture(TextureType textureType, ref ushort matTex, ushort defaultTex)
        {
            ResourceHandle<TextureResource>? handle = TextureResources[(int)textureType];
            if (handle != null && handle.IsLoaded)
            {
                TextureResource? res = handle.Get();
                if (res != null && res.GPUTexture != null)
                {
                    matTex = (ushort)handle.Get().GPUTexture.TexHandle;
                }
                else
                {
                    matTex = defaultTex;
                }

                // Allow instant toggle if texture has been loaded in the past
                if (!CFG.Current.Viewport_Enable_Texturing)
                {
                    matTex = defaultTex;
                }
            }
            else
            {
                matTex = defaultTex;
            }
        }

        public void ReleaseTextures()
        {
            for (var i = 0; i < (int)TextureType.TextureResourceCount; i++)
            {
                TextureResources[i]?.Release();
                TextureResources[i] = null;
            }
        }

        public void UpdateMaterial()
        {
            SetMaterialTexture(TextureType.AlbedoTextureResource, ref MaterialData.colorTex, AlbedoFromColor ? WhiteTexture : (ushort)0);
            SetMaterialTexture(TextureType.EmissiveTextureResource, ref MaterialData.emissiveTex, BlackTexture);
            SetMaterialTexture(TextureType.AlbedoTextureResource2, ref MaterialData.colorTex2, ColorOnlyLayers[1] ? WhiteTexture : (ushort)0);
            SetMaterialTexture(TextureType.NormalTextureResource, ref MaterialData.normalTex, 1);
            SetMaterialTexture(TextureType.NormalTextureResource2, ref MaterialData.normalTex2, 1);
            SetMaterialTexture(TextureType.SpecularTextureResource, ref MaterialData.specTex, 2);
            SetMaterialTexture(TextureType.SpecularTextureResource2, ref MaterialData.specTex2, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource, ref MaterialData.shininessTex, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource2, ref MaterialData.shininessTex2, 2);
            SetMaterialTexture(TextureType.BlendmaskTextureResource, ref MaterialData.blendMaskTex, OpacityFadeUvIndex >= 0 ? WhiteTexture : (ushort)0);
            // 0 until the lightmap is on the GPU, which the shader reads as no lightmap
            SetMaterialTexture(TextureType.LightmapTextureResource, ref MaterialData.lightmapTex, 0);

            SetMaterialTexture(TextureType.AlbedoTextureResource3, ref MaterialData.colorTex3, ColorOnlyLayers[2] ? WhiteTexture : (ushort)0);
            SetMaterialTexture(TextureType.AlbedoTextureResource4, ref MaterialData.colorTex4, ColorOnlyLayers[3] ? WhiteTexture : (ushort)0);
            SetMaterialTexture(TextureType.NormalTextureResource3, ref MaterialData.normalTex3, 1);
            SetMaterialTexture(TextureType.NormalTextureResource4, ref MaterialData.normalTex4, 1);
            SetMaterialTexture(TextureType.SpecularTextureResource3, ref MaterialData.specTex3, 2);
            SetMaterialTexture(TextureType.SpecularTextureResource4, ref MaterialData.specTex4, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource3, ref MaterialData.shininessTex3, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource4, ref MaterialData.shininessTex4, 2);

            // The metallic path has no shininess textures: row 3 is the metallic offset of each layer
            if (MetallicShading)
            {
                MaterialData.shininessTex = MetallicOffsetBits[0];
                MaterialData.shininessTex2 = MetallicOffsetBits[1];
                MaterialData.shininessTex3 = MetallicOffsetBits[2];
                MaterialData.shininessTex4 = MetallicOffsetBits[3];
            }

            MaterialWithDeform? deformed = null;

            if (Deform is MaterialDeform deform)
            {
                ushort position = 0;
                ushort normal = 0;
                SetMaterialTexture(TextureType.VatPositionTextureResource, ref position, 0);
                SetMaterialTexture(TextureType.VatNormalTextureResource, ref normal, 0);
                deform.VatPositionTex = position;
                deform.VatNormalTex = normal;

                // The cutscene's textures and frame, rather than the material's own (placeholders in the MATBIN)
                if (Definition?.Deformer == MaterialDeformer.VatFrame)
                {
                    deform.VatPositionTex = _vatFramePositionTex;
                    deform.VatNormalTex = _vatFrameNormalTex;
                    deform.Rate = _vatFramePlay;                }

                deformed = new MaterialWithDeform { Material = MaterialData, Shading = Shading, Effects = Effects, Deform = deform };
            }

            MaterialWithBlend? blended = null;

            // The extended blend form's entries come after water, and a detail stack's after those, so everything before
            // them has to be there
            var detailStack = HasDetailStack;
            var layerDetail = LayerDetail != null || detailStack;

            if (MaskTilingEntries)
            {
                MaterialData.uvSet2 |= Material.MaskTilingFlag;
            }

            // An opacity blend (OpacityFadeUvIndex): its tilings are in the blend entries too
            if (OpacityFadeUvIndex >= 0 && MaskTilingEntries)
            {
                MaterialData.uvSet3 |= Material.OpacityFlag;
            }

            // A GXFlver mask blend: FC_BlendMaskParam in the blend entries (FlverShader.frag, MATERIAL_MASK_BLEND)
            if (MaskBlendUvIndex >= 0 && MaskTilingEntries)
            {
                MaterialData.uvSet2 |= Material.MaskBlendFlag;
                var (x, z) = MaskBlendParam(MaskBlendIntensity);
                MaterialWithBlend withParam = Blend ?? new MaterialWithBlend();
                withParam.Blend.Sharpness = new Vector4(x, 0, 0, 0);
                withParam.Blend.Detail = new Vector4(z, 0, 0, 0);
                Blend = withParam;
            }

            // An overlay comes after the blend entries, which are just unused without a recipe, as they are for a mask's tiling
            if ((Detail != null || Magma != null || Water != null || layerDetail || MaskTilingEntries ? Blend ?? new MaterialWithBlend() : Blend) is MaterialWithBlend blend)
            {
                ushort detail = 0;
                SetMaterialTexture(TextureType.DetailMaskTextureResource, ref detail, OpacityFadeUvIndex >= 0 ? WhiteTexture : BlackTexture);

                // An extended layer's own detail mask, in the top 16 bits of its source (black until it loads)
                if (Definition?.Blend?.Extended == true)
                {
                    ushort mask2 = 0, mask3 = 0, mask4 = 0;
                    SetMaterialTexture(TextureType.LayerDetailMaskTextureResource2, ref mask2, BlackTexture);
                    SetMaterialTexture(TextureType.LayerDetailMaskTextureResource3, ref mask3, BlackTexture);
                    SetMaterialTexture(TextureType.LayerDetailMaskTextureResource4, ref mask4, BlackTexture);
                    blend.Blend.Source1 = (blend.Blend.Source1 & 0xFFFF) | ((uint)mask2 << 16);
                    blend.Blend.Source2 = (blend.Blend.Source2 & 0xFFFF) | ((uint)mask3 << 16);
                    blend.Blend.Source3 = (blend.Blend.Source3 & 0xFFFF) | ((uint)mask4 << 16);
                }

                blend.Material = MaterialData;
                blend.Shading = Shading;
                blend.Effects = Effects;
                blend.Deform = deformed?.Deform ?? default;
                blend.LayerTiling = LayerTiling;
                blend.LayerScroll = LayerScroll;
                blend.Blend.DetailTex = detail;
                blended = blend;
            }

            MaterialWithDetail? detailed = null;

            if ((Magma != null || Water != null || layerDetail ? Detail ?? new MaterialDetail() : Detail) is MaterialDetail overlay)
            {
                ushort overlayAlbedo = 0;
                ushort overlayNormal = 0;
                SetMaterialTexture(TextureType.DetailAlbedoTextureResource, ref overlayAlbedo, 0);
                SetMaterialTexture(TextureType.DetailNormalTextureResource, ref overlayNormal, 0);

                overlay.Tiling = DetailTiling;
                overlay.AlbedoTex = overlayAlbedo;
                overlay.NormalTex = overlayNormal;
                detailed = new MaterialWithDetail { Blended = blended!.Value, Detail = overlay };
            }

            MaterialWithMagma? glowing = null;

            if ((Water != null || layerDetail ? Magma ?? new MaterialMagma() : Magma) is MaterialMagma magma)
            {
                ushort magmaTex = 0;
                SetMaterialTexture(TextureType.MagmaTextureResource, ref magmaTex, 0);

                magma.Tex = magmaTex;
                magma.Tiling.X = MagmaTiling.X;
                magma.Tiling.Y = MagmaTiling.Y;
                glowing = new MaterialWithMagma { Detailed = detailed!.Value, Magma = magma };
            }

            MaterialWithWater? watery = Water is MaterialWater water
                ? new MaterialWithWater { Glowing = glowing!.Value, Water = water }
                : layerDetail ? new MaterialWithWater { Glowing = glowing!.Value } : null;

            MaterialWithLayerDetail? layered = layerDetail
                ? new MaterialWithLayerDetail { Watery = watery!.Value, Detail = LayerDetail ?? default, Extra = LayerDetailExtra }
                : null;

            MaterialWithDetailStack? stacked = null;

            if (detailStack)
            {
                ushort weightTex = 0;
                SetMaterialTexture(TextureType.DetailStackWeightTextureResource, ref weightTex, 0);

                var entries = new MaterialDetailStackEntry[DetailStack.Length];
                for (var i = 0; i < entries.Length; i++)
                {
                    ushort albedoTex = 0, normalTex = 0;
                    SetMaterialTexture(DetailStackAlbedos[i], ref albedoTex, 0);
                    SetMaterialTexture(DetailStackNormals[i], ref normalTex, 0);
                    entries[i] = DetailStack[i];
                    entries[i].Textures = albedoTex | ((uint)normalTex << 16);
                    entries[i].WeightTex = weightTex;
                }

                stacked = new MaterialWithDetailStack { Layered = layered!.Value, Layer1 = entries[0], Layer2 = entries[1], Layer3 = entries[2], Layer4 = entries[3], Back = entries[4] };
            }

            MaterialWithShading shaded = new() { Material = MaterialData, Shading = Shading, Effects = Effects };

            // A ChrCustomize colouring, in the deform entry's place (CustomizeMode rules out materials with a deformer)
            MaterialWithCustomize? customized = null;

            if (Customize is MaterialCustomize customize)
            {
                ushort mask = 0;
                SetMaterialTexture(TextureType.CustomizeMaskTextureResource, ref mask, 0);
                customize.MaskTex = mask;
                customize.MaskTiling = CustomizeMaskTiling;

                // A FaceGen skin's face texture, built at run time, in the mask's place (it has none)
                if (CustomizeMode(Definition) == MaterialCustomize.ModeFaceGen)
                {
                    customize.MaskTex = CustomizeFaceTexture;
                    customize.MaskTiling = Vector2.One;
                }

                // The skin's stubble texture, an eye patch's cloth or leather, or a FaceGen skin's body texture, and
                // its tiling (Params.yz)
                if (CustomizeMode(Definition) is MaterialCustomize.ModeSkin or MaterialCustomize.ModeEyePatch or MaterialCustomize.ModeFaceGen)
                {
                    ushort stubble = 0;
                    SetMaterialTexture(TextureType.CustomizeStubbleTextureResource, ref stubble, 0);
                    customize.SecondTex = stubble;
                    customize.Params.Y = CustomizeStubbleTiling.X;
                    customize.Params.Z = CustomizeStubbleTiling.Y;
                }

                // The makeup masks and their tilings
                var makeup = CustomizeMakeup;
                ushort makeupMask0 = 0, makeupMask1 = 0;
                SetMaterialTexture(TextureType.CustomizeMakeupMaskTextureResource0, ref makeupMask0, 0);
                SetMaterialTexture(TextureType.CustomizeMakeupMaskTextureResource1, ref makeupMask1, 0);
                makeup.MaskTex0 = makeupMask0;
                makeup.MaskTex1 = makeupMask1;
                makeup.MaskTiling = CustomizeMakeupTiling;

                // The decal (set at run time) and its mask
                ushort decal = 0, decalMask = 0;
                SetMaterialTexture(TextureType.CustomizeDecalTextureResource, ref decal, 0);
                SetMaterialTexture(TextureType.CustomizeDecalMaskTextureResource, ref decalMask, 0);
                makeup.DecalTex = decal;
                makeup.DecalMaskTex = decalMask;

                // The burn scar's albedo and normal
                ushort burnAlbedo = 0, burnNormal = 0;
                SetMaterialTexture(TextureType.CustomizeBurnTextureResource, ref burnAlbedo, 0);
                SetMaterialTexture(TextureType.CustomizeBurnNormalTextureResource, ref burnNormal, 0);
                makeup.BurnTex = burnAlbedo;
                makeup.BurnNormalTex = burnNormal;

                // An eye's iris height (the second texture) and its tiling (Extra.B.zw)
                var extra = CustomizeExtra;
                if (CustomizeMode(Definition) == MaterialCustomize.ModeSkin && Definition?.Slots?.Decal != null)
                {
                    extra.C = CustomizeDecalTiling;
                }
                else if (CustomizeMode(Definition) == MaterialCustomize.ModeSkin && Definition?.Slots?.Burn != null)
                {
                    extra.C = CustomizeBurnTiling;
                }

                if (CustomizeMode(Definition) == MaterialCustomize.ModeEye)
                {
                    ushort height = 0;
                    SetMaterialTexture(TextureType.CustomizeStubbleTextureResource, ref height, 0);
                    customize.SecondTex = height;
                    extra.B.Z = CustomizeStubbleTiling.X;
                    extra.B.W = CustomizeStubbleTiling.Y;
                }

                customized = new MaterialWithCustomize
                {
                    Material = MaterialData, Shading = Shading, Effects = Effects, Customize = customize, Extra = extra,
                    Makeup = makeup
                };
            }

            SceneRenderer.AddBackgroundUploadTask((d, cl) =>
            {
                Profiler.___tracy_c_zone_context ctx = Profiler.TracyCZoneN(1, @"Material upload");
                if (MaterialBuffer != null && !disposedValue)
                {
                    if (stacked is MaterialWithDetailStack stackedData)
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref stackedData);
                    }
                    else                     if (layered is MaterialWithLayerDetail layeredData)
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref layeredData);
                    }
                    else if (watery is MaterialWithWater waterData)
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref waterData);
                    }
                    else if (glowing is MaterialWithMagma magmaData)
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref magmaData);
                    }
                    else if (detailed is MaterialWithDetail detailData)
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref detailData);
                    }
                    else if (blended is MaterialWithBlend blendData)
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref blendData);
                    }
                    else if (customized is MaterialWithCustomize customizedData)
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref customizedData);
                    }
                    else if (deformed is MaterialWithDeform data)
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref data);
                    }
                    else
                    {
                        MaterialBuffer.FillBuffer(d, cl, ref shaded);
                    }
                }
                Profiler.TracyCZoneEnd(ctx);
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                // First, as releasing the textures would otherwise upload to the buffer being given up
                disposedValue = true;

                if (disposing)
                {
                    if (MaterialBuffer != null)
                    {
                        SceneRenderer.DisposeAfterFramesInFlight(MaterialBuffer);
                    }
                }

                ReleaseTextures();
            }
        }

        ~FlverMaterial()
        {
            Dispose(false);
        }
    }

    public class FlverSubmesh
    {
        public Matrix4x4 LocalTransform = Matrix4x4.Identity;

        // This is native because using managed arrays causes a weird memory leak
        public nint PickingVertices = nint.Zero;

        public List<FlverSubmeshFaceSet> MeshFacesets { get; set; } = new();

        public VertexIndexBufferAllocator.VertexIndexBufferHandle? GeomBuffer { get; set; }

        public int VertexCount { get; set; }

        public BoundingBox Bounds { get; set; }

        // Use the w field in the normal as an index to a bone that has a transform
        public bool UseNormalWBoneTransform { get; set; }

        public int DefaultBoneIndex { get; set; } = -1;

        public FlverMaterial? Material { get; set; }

        /// <summary>
        /// The vertices as they were sent to the GPU, for models <see cref="RetainVerticesFor"/> names; else null.
        /// Every vertex layout starts with its position (a Vector3), <see cref="VertexStride"/> bytes apart.
        /// </summary>
        public byte[]? RetainedVertices { get; set; }

        public uint VertexStride { get; set; }

        public struct FlverSubmeshFaceSet
        {
            public int IndexCount;
            public int IndexOffset;

            public int PickingIndicesCount;

            //public IntPtr PickingIndices;
            public bool BackfaceCulling;
            public bool IsTriangleStrip;
            public byte LOD;
            public bool IsMotionBlur;
            public bool Is32Bit;
        }
    }

    private struct FlverMaterialDef
    {
        public uint nameOffset;
        public readonly uint mtdOffset;
        public readonly int textureCount;
        public readonly int textureIndex;
        public int flags;
        public int gxOffset;

        public FlverMaterialDef(BinaryReaderEx br)
        {
            nameOffset = br.ReadUInt32();
            mtdOffset = br.ReadUInt32();
            textureCount = br.ReadInt32();
            textureIndex = br.ReadInt32();
            flags = br.ReadInt32();
            gxOffset = br.ReadInt32();
            br.ReadInt32(); // unknown
            br.AssertInt32(0);
        }
    }

    private struct FlverBone
    {
        public readonly Vector3 position;
        public uint nameOffset;
        public readonly Vector3 rotation;
        public readonly short parentIndex;
        public short childIndex;
        public readonly Vector3 scale;
        public short nextSiblingIndex;
        public short previousSiblingIndex;
        public Vector3 boundingBoxMin;
        public Vector3 boundingBoxMax;

        public Matrix4x4 ComputeLocalTransform()
        {
            return Matrix4x4.CreateScale(scale)
                   * Matrix4x4.CreateRotationX(rotation.X)
                   * Matrix4x4.CreateRotationZ(rotation.Z)
                   * Matrix4x4.CreateRotationY(rotation.Y)
                   * Matrix4x4.CreateTranslation(position);
        }

        public FlverBone(BinaryReaderEx br)
        {
            position = br.ReadVector3();
            nameOffset = br.ReadUInt32();
            rotation = br.ReadVector3();
            parentIndex = br.ReadInt16();
            childIndex = br.ReadInt16();
            scale = br.ReadVector3();
            nextSiblingIndex = br.ReadInt16();
            previousSiblingIndex = br.ReadInt16();
            boundingBoxMin = br.ReadVector3();
            br.ReadInt32(); // unknown
            boundingBoxMax = br.ReadVector3();
            br.Position += 0x34;
        }
    }

    private struct FlverMesh
    {
        public readonly int dynamic;
        public readonly int materialIndex;
        public readonly int defaultBoneIndex;
        public int boneCount;
        public readonly int boneOffset;
        public readonly int facesetCount;
        public readonly uint facesetIndicesOffset;
        public readonly int vertexBufferCount;
        public readonly uint vertexBufferIndicesOffset;

        public FlverMesh(BinaryReaderEx br)
        {
            dynamic = br.AssertByte([0, 1]);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertByte(0);
            materialIndex = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            defaultBoneIndex = br.ReadInt32();
            boneCount = br.ReadInt32();
            br.ReadInt32(); // bb offset
            boneOffset = br.ReadInt32();
            facesetCount = br.ReadInt32();
            facesetIndicesOffset = br.ReadUInt32();
            vertexBufferCount = br.ReadInt32();
            vertexBufferIndicesOffset = br.ReadUInt32();
        }

        public readonly int[] GetMeshBoneIndices(BinaryReaderEx br)
            => br.GetInt32s(boneOffset, boneCount);
    }

    private struct FlverFaceset
    {
        public readonly FLVER2.FaceSet.FSFlags flags;
        public readonly bool triangleStrip;
        public readonly bool cullBackfaces;
        public readonly int indexCount;
        public readonly uint indicesOffset;
        public readonly int indexSize;

        public FlverFaceset(BinaryReaderEx br, int version, int headerIndexSize, uint dataOffset)
        {
            flags = (FLVER2.FaceSet.FSFlags)br.ReadUInt32();
            triangleStrip = br.ReadBoolean();
            cullBackfaces = br.ReadBoolean();
            br.ReadInt16();
            indexCount = br.ReadInt32();
            indicesOffset = br.ReadUInt32() + dataOffset;
            indexSize = 0;
            if (version > 0x20005)
            {
                br.ReadInt32(); // Indices length
                br.AssertInt32(0);
                indexSize = br.AssertInt32([0, 16, 32]);
                br.AssertInt32(0);
            }

            if (indexSize == 0)
            {
                indexSize = headerIndexSize;
            }
        }
    }

    private struct FlverVertexBuffer
    {
        public bool edgeCompressed;
        public int bufferIndex;
        public readonly int layoutIndex;
        public int vertexSize;
        public readonly int vertexCount;
        public readonly uint bufferOffset;

        public FlverVertexBuffer(BinaryReaderEx br, uint dataOffset)
        {
            bufferIndex = br.ReadInt32();

            // TODO EDGE
            int final = bufferIndex & ~0x60000000;
            if (final != bufferIndex)
            {
                bufferIndex = final;
                edgeCompressed = true;
            }

            layoutIndex = br.ReadInt32();
            vertexSize = br.ReadInt32();
            vertexCount = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.ReadInt32(); // Buffer length
            bufferOffset = br.ReadUInt32() + dataOffset;
        }
    }

    private struct FlverBufferLayoutMember
    {
        public readonly int unk00;
        public readonly FLVER.LayoutType type;
        public readonly FLVER.LayoutSemantic semantic;
        public readonly int index;

        // Matches the SpecialModifier check in FLVER.Vertex.Read: these members have no data in the vertex
        public bool IsSpecialMember => (short)(unk00 >> 16) == -32768;

        public FlverBufferLayoutMember(BinaryReaderEx br)
        {
            unk00 = br.ReadInt32(); // unk
            br.ReadInt32(); // struct offset
            type = br.ReadEnum32<FLVER.LayoutType>();
            semantic = br.ReadEnum32<FLVER.LayoutSemantic>();
            index = br.ReadInt32();
        }
    }

    private struct FlverBufferLayout
    {
        public readonly int memberCount;
        public readonly uint membersOffset;

        public FlverBufferLayout(BinaryReaderEx br)
        {
            memberCount = br.ReadInt32();
            br.AssertInt32(0);
            br.AssertInt32(0);
            membersOffset = br.ReadUInt32();
        }
    }

    private struct FlverTexture
    {
        public readonly uint pathOffset;
        public readonly uint typeOffset;
        public Vector2 scale;

        public FlverTexture(BinaryReaderEx br)
        {
            pathOffset = br.ReadUInt32();
            typeOffset = br.ReadUInt32();
            scale = br.ReadVector2();

            // unks
            br.ReadByte();
            br.ReadBoolean();
            br.AssertByte(0);
            br.AssertByte(0);
            br.ReadSingle();
            br.ReadSingle();
            br.ReadSingle();
        }
    }

    private struct EdgeMemberGroup
    {
        public readonly short memberCount;
        public readonly short unk02;
        public readonly short unk04;
        public readonly byte unk06;
        public readonly bool unk07;
        public readonly int edgeIndexBuffersLength;

        public EdgeMemberGroup(BinaryReaderEx br)
        {
            memberCount = br.ReadInt16();
            unk02 = br.ReadInt16();
            unk04 = br.ReadInt16();
            unk06 = br.ReadByte();
            unk07 = br.ReadBoolean();
            br.AssertInt32(0);
            edgeIndexBuffersLength = br.ReadInt32();
        }
    }

    private struct EdgeMember
    {
        public readonly int edgeIndexesLength;
        public readonly int edgeIndexesOffset;
        public readonly byte unk10;
        public readonly byte unk11;
        public readonly byte unk12;
        public readonly byte unk13;
        public readonly ushort baseIndex;
        public readonly short unk16;
        public readonly int edgeVertexBufferLength;
        public readonly int edgeVertexBufferOffset;
        public readonly EdgeGeomSpuConfigInfo spuConfigInfo;

        public EdgeMember(BinaryReaderEx br)
        {
            edgeIndexesLength = br.ReadInt32();
            edgeIndexesOffset = br.ReadInt32();
            br.AssertInt64(0);

            var buf = br.ReadBytes(4);
            unk10 = buf[0];
            unk11 = buf[1];
            unk12 = buf[2];
            unk13 = buf[3];

            baseIndex = br.ReadUInt16();
            unk16 = br.ReadInt16();
            edgeVertexBufferLength = br.ReadInt32();
            edgeVertexBufferOffset = br.ReadInt32();
            br.AssertInt64(0);
            br.AssertInt64(0);
            spuConfigInfo = new EdgeGeomSpuConfigInfo(br);
        }
    }

    private struct EdgeGeomSpuConfigInfo
    {
        public readonly byte flagsAndUniformTableCount;
        public readonly byte commandBufferHoleSize;
        public readonly byte inputVertexFormatId;
        public readonly byte secondaryInputVertexFormatId;
        public readonly byte outputVertexFormatId;
        public readonly byte vertexDeltaFormatId;
        public readonly byte indexesAndSkinningFlavor;
        public readonly byte skinningMatrixFormat;
        public readonly ushort numVertexes;
        public readonly ushort numIndexes;
        public readonly int indexesOffset;

        public EdgeGeomSpuConfigInfo(BinaryReaderEx br)
        {
            var buf = br.ReadBytes(8);
            flagsAndUniformTableCount = buf[0];
            commandBufferHoleSize = buf[1];
            inputVertexFormatId = buf[2];
            secondaryInputVertexFormatId = buf[3];
            outputVertexFormatId = buf[4];
            vertexDeltaFormatId = buf[5];
            indexesAndSkinningFlavor = buf[6];
            skinningMatrixFormat = buf[7];

            numVertexes = br.ReadUInt16();
            numIndexes = br.ReadUInt16();
            indexesOffset = br.ReadInt32();
        }
    }

    #region IDisposable Support

    private bool disposedValue; // To detect redundant calls

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
            }

            if (GPUMaterials != null)
            {
                foreach (FlverMaterial m in GPUMaterials)
                {
                    if (m != null)
                        m.Dispose();
                }
            }

            if (GPUMeshes != null)
            {
                foreach (FlverSubmesh m in GPUMeshes)
                {
                    if (m != null)
                    {
                        if (m.GeomBuffer != null)
                            m.GeomBuffer.Dispose();

                        //Marshal.FreeHGlobal(m.PickingVertices);
                    }
                }
            }

            if (StaticBoneBuffer != null)
            {
                SceneRenderer.DisposeAfterFramesInFlight(StaticBoneBuffer);
            }

            disposedValue = true;
        }
    }

    ~FlverResource()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(false);
    }

    // This code added to correctly implement the disposable pattern.
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        Dispose(true);
        // TODO: uncomment the following line if the finalizer is overridden above.
        GC.SuppressFinalize(this);
    }

    #endregion
}