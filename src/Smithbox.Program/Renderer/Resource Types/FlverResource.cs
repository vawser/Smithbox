#nullable enable
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

    public GPUBufferAllocator.GPUBufferHandle? StaticBoneBuffer { get; private set; }

    public bool IsSpeedtree = false;
    /// <summary>
    /// Where this model was loaded from. Gives access to the pool its textures are looked up in.
    /// </summary>
    public ResourceLoadContext? Context { get; private set; }

    // Set before anything is read, so that all of the load can follow it
    private bool ModelsOnly => Context?.Pool?.ModelsOnly == true;

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

        if (curProject.Descriptor.ProjectType is ProjectType.DES)
        {
            FlverDeS = FLVER0.Read(bytes);
            ret = LoadInternalDeS(al);
        }
        else
        {
            if (al == AccessLevel.AccessGPUOptimizedOnly &&
                curProject.Descriptor.ProjectType != ProjectType.NR &&
                curProject.Descriptor.ProjectType != ProjectType.DS1R &&
                curProject.Descriptor.ProjectType != ProjectType.DS1 && (ModelsOnly || !FocusManager.IsInModelEditor()))
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
                if (curProject.Descriptor.ProjectType is ProjectType.DES)
                {
                    FlverDeS = FLVER0.Read(fileData.Value);
                    ret = LoadInternalDeS(al);
                }
                else
                {
                    if (al == AccessLevel.AccessGPUOptimizedOnly &&
                        curProject.Descriptor.ProjectType != ProjectType.NR &&
                        curProject.Descriptor.ProjectType != ProjectType.DS1R &&
                        curProject.Descriptor.ProjectType != ProjectType.DS1 && (ModelsOnly || !FocusManager.IsInModelEditor()))
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

        if (mpath == "")
        {
            var mtdstring = Path.GetFileNameWithoutExtension(mtd.Replace('\\', Path.DirectorySeparatorChar));

            // MTD
            material = bank.GetMaterial(mtdstring);

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
            if (curProject.Descriptor.ProjectType is ProjectType.ER
                or ProjectType.AC6
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB)
            {
                matbin = bank.GetMatbin(mtdstring);

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
        }

        // Textures are found by name, in the same pool as this model
        string textureName = ResourceName.Normalize(path);

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
            dest.ApplySampler(textureType, resolvedType);
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

    /// <summary>
    /// Works out which layer (0 based) of a kind of texture a parameter name refers to.
    /// Layer 1 (index 0) has no number, the layers after it are numbered 2, 3 and 4.
    /// </summary>
    private static bool MatchLayer(string name, string exact1, string exact2, string containsPrefix,
        string containsBase, out int layer)
    {
        for (var n = Material.MaxLayers; n >= 2; n--)
        {
            if (name == exact1 + n || name == exact2 + n || name.Contains(containsPrefix + n))
            {
                layer = n - 1;
                return true;
            }
        }

        if (name == exact1 || name == exact2 || name.Contains(containsBase))
        {
            layer = 0;
            return true;
        }

        layer = 0;
        return false;
    }

    /// <summary>
    /// Whether the renderer knows how to weigh the layers of this material. Otherwise only its
    /// first layer is shown, since blending by the wrong weights looks worse than not blending.
    /// </summary>
    private static bool HasKnownBlend(FlverMaterial dest)
    {
        return dest.Definition is { BlendWeight: MaterialBlendWeight.Recipe } or { BlendWeight: MaterialBlendWeight.TexCoord1X, BlendLayers.Count: 2 };
    }

    /// <summary>
    /// Loads the textures of a recipe by what the material calls them, as the names
    /// in the flver don't always match, and a recipe can use textures the flver doesn't list.
    /// </summary>
    private void LoadBlendRecipe(FlverMaterial dest, string mtd, MaterialLayers layers)
    {
        if (dest.Definition?.SurfaceType != null)
        {
            LookupTexture(dest.Definition.Surface == MaterialSurface.Water
                ? FlverMaterial.TextureType.NormalTextureResource
                : FlverMaterial.TextureType.AlbedoTextureResource, dest, dest.Definition.SurfaceType, "", mtd);
        }

        if (dest.Definition?.Magma is { } magma)
        {
            LookupTexture(FlverMaterial.TextureType.MagmaTextureResource, dest, magma.TextureType, "", mtd);
        }

        MaterialResolvedDetail? detail = dest.Definition?.Detail;

        if (detail?.AlbedoType != null)
        {
            LookupTexture(FlverMaterial.TextureType.DetailAlbedoTextureResource, dest, detail.AlbedoType, "", mtd);
        }

        if (detail?.NormalType != null)
        {
            LookupTexture(FlverMaterial.TextureType.DetailNormalTextureResource, dest, detail.NormalType, "", mtd);
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
                LookupTexture(AlbedoLayers[i], dest, blendLayers[i].Albedo, "", mtd);
            }

            if (blendLayers[i].Normal != null)
            {
                LookupTexture(NormalLayers[i], dest, blendLayers[i].Normal, "", mtd);
                layers.BlendNormal = true;
            }
        }

        layers.LayerCount = blendLayers.Count;

        if (blend.MaskType != null)
        {
            LookupTexture(FlverMaterial.TextureType.BlendmaskTextureResource, dest, blend.MaskType, "", mtd);
        }

        if (blend.DetailType != null)
        {
            LookupTexture(FlverMaterial.TextureType.DetailMaskTextureResource, dest, blend.DetailType, "", mtd);
        }
    }

    /// <summary>
    /// Handles a texture of a material whose layers the definition has worked out. These can't be told
    /// apart by name: the layers of M[Flag][Mb2] have identical endings, and its normals are mixed in with
    /// an overlay normal that isn't a layer. Returns false for textures that aren't its concern.
    /// </summary>
    private bool TryProcessKnownBlendTexture(FlverMaterial dest, string? texType, string mtd, MaterialLayers layers)
    {
        if (texType == null || !HasKnownBlend(dest))
        {
            return false;
        }

        MaterialDefinition definition = dest.Definition!;

        if (definition.Blend != null)
        {
            MaterialSampler? known = definition.Samplers.Find(x => x.Type == texType);

            return known != null && (known.Role is "AlbedoMap" or "NormalMap" || known.Type == definition.Blend.MaskType
                || known.Type == definition.Blend.DetailType);
        }

        for (var i = 0; i < definition.BlendLayers.Count; i++)
        {
            MaterialBlendLayer layer = definition.BlendLayers[i];

            if (layer.Albedo == texType)
            {
                LookupTexture(AlbedoLayers[i], dest, texType, "", mtd);
                layers.LayerCount = Math.Max(layers.LayerCount, i + 1);
                return true;
            }

            if (layer.Normal == texType)
            {
                LookupTexture(NormalLayers[i], dest, texType, "", mtd);
                layers.BlendNormal = true;
                return true;
            }
        }

        // Anything else in the role of a layer would take that layer's slot
        var role = definition.Samplers.Find(x => x.Type == texType)?.Role;

        return role is "AlbedoMap" or "NormalMap";
    }

    private void ProcessMaterialTexture(FlverMaterial dest, string texType, string mpath, string mtd,
        MaterialLayers layers)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (dest.Definition?.Magma?.TextureType == texType)
        {
            return;
        }

        // Already loaded, and they would otherwise be taken for the main textures
        if (dest.Definition?.Detail is { } overlay && (texType == overlay.AlbedoType || texType == overlay.NormalType))
        {
            return;
        }

        if (TryProcessKnownBlendTexture(dest, texType, mtd, layers))
        {
            return;
        }

        if (texType != null && texType.Contains("Emissive", StringComparison.OrdinalIgnoreCase))
        {
            LookupTexture(FlverMaterial.TextureType.EmissiveTextureResource, dest, texType, mpath, mtd);
            return;
        }

        string paramNameCheck;

        if (texType == null)
        {
            paramNameCheck = "G_DIFFUSE";
        }
        else
        {
            paramNameCheck = texType.ToUpper();
        }

        int layer;

        if (MatchLayer(paramNameCheck, "G_DIFFUSETEXTURE", "G_DIFFUSE", "ALBEDO_", "ALBEDO", out layer))
        {
            LookupTexture(AlbedoLayers[layer], dest, texType, mpath, mtd);
            layers.LayerCount = Math.Max(layers.LayerCount, layer + 1);
        }
        else if (MatchLayer(paramNameCheck, "G_BUMPMAPTEXTURE", "G_BUMPMAP", "NORMAL_", "NORMAL", out layer))
        {
            LookupTexture(NormalLayers[layer], dest, texType, mpath, mtd);
            layers.LayerCount = Math.Max(layers.LayerCount, layer + 1);
            layers.BlendNormal |= layer > 0;
        }
        else if (MatchLayer(paramNameCheck, "G_SPECULARTEXTURE", "G_SPECULAR", "SPECULAR_", "SPECULAR", out layer))
        {
            layers.LayerCount = Math.Max(layers.LayerCount, layer + 1);

            // These games keep their gloss in the specular slot, which the shader reads as shininess
            if (curProject.Descriptor.ProjectType is ProjectType.DS1R or ProjectType.DS2S or ProjectType.DS2)
            {
                LookupTexture(ShininessLayers[layer], dest, texType, mpath, mtd);
                layers.BlendShininess |= layer > 0;
            }
            else
            {
                LookupTexture(SpecularLayers[layer], dest, texType, mpath, mtd);
                layers.BlendSpecular |= layer > 0;
            }
        }
        else if (MatchLayer(paramNameCheck, "G_SHININESSTEXTURE", "G_SHININESS", "SHININESS", "SHININESS", out layer))
        {
            LookupTexture(ShininessLayers[layer], dest, texType, mpath, mtd);
            layers.LayerCount = Math.Max(layers.LayerCount, layer + 1);
            layers.BlendShininess |= layer > 0;
        }
        else if (paramNameCheck.Contains("BLENDMASK"))
        {
            LookupTexture(FlverMaterial.TextureType.BlendmaskTextureResource, dest, texType, mpath, mtd);
            layers.HasBlendMask = true;
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

        dest.DepthWrite = dest.Blending == MaterialBlending.Opaque && !definition.DisableDepthWrite;
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

        if (dest.Definition != null)
        {
            ApplyDefinitionShading(dest, dest.Definition);
        }

        if (deformer != MaterialDeformer.None || knownBlend)
        {
            dest.ShaderName = layers.IsBlended ? @"FlverShader\FlverShader_deform_blend" : @"FlverShader\FlverShader_deform";
            dest.LayoutType = MeshLayoutType.LayoutDeform;
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

        specConstants.Add(new SpecializationConstant(6, dest.Blending != MaterialBlending.Opaque));
        specConstants.Add(new SpecializationConstant(8, hasDetail));
        specConstants.Add(new SpecializationConstant(9, hasMagma));
        specConstants.Add(new SpecializationConstant(10, (uint)(dest.Definition?.Surface ?? MaterialSurface.Standard)));
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

        // Upload the defaults so a material whose textures never arrive still shows something sensible
        dest.UpdateMaterial();
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

        return SceneRenderer.MaterialBufferAllocator.Allocate((uint)size, sizeof(Material));
    }

    /// <summary>
    /// Starts loading the textures of a vertex animation. Nothing in <see cref="ProcessMaterialTexture"/>
    /// recognizes them, so the shader table says which they are.
    /// </summary>
    private void LoadDeformTextures(FlverMaterial dest, string mtd)
    {
        if (dest.Definition?.Deformer is not (MaterialDeformer.Vat or MaterialDeformer.VatLoop))
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
            definition.Deformer is MaterialDeformer.Vat or MaterialDeformer.VatLoop ? 1 : 0);
        deform.Rate = GetDeformValue(definition, definition.Deformer switch
        {
            MaterialDeformer.Vat or MaterialDeformer.VatLoop => "FrameRate",
            MaterialDeformer.WindPivot => "Phase",
            _ => "Speed"
        }, 0);

        if (definition.DeformerParams.TryGetValue("Direction", out var direction) && direction.Length >= 3)
        {
            deform.Direction = new Vector4(direction[0], direction[1], direction[2], GetDeformValue(definition, "Phase", 0));
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
            MaterialDeformer.Sway => Math.Abs(deform.Amplitude) * deform.Direction.Length(),

            // Wind depends on the viewport's settings rather than the material
            _ => 1.0f
        };

        Vector3 pad = new(reach);
        dest.Bounds = new BoundingBox(dest.Bounds.Min - pad, dest.Bounds.Max + pad);
    }

    private unsafe void ProcessMaterial(IFlverMaterial mat, FlverMaterial dest)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        dest.MaterialName = Path.GetFileNameWithoutExtension(mat.MTD);
        dest.Definition = GetMaterialDefinition(mat.MTD);
        dest.MaterialBuffer = AllocateMaterialBuffer(dest);
        dest.MaterialData = new Material();

        var matName = mat.Name;
        if (!string.IsNullOrWhiteSpace(matName) && matName.Length >= 3 && matName[0] == '#' && char.IsDigit(matName[1]) && char.IsDigit(matName[2]))
        {
            dest.MaterialMask = int.Parse(matName.Substring(1, 2));
        }

        dest.SpecializationConstants = new List<SpecializationConstant>();

        //FLVER0 stores layouts directly in the material
        if (curProject.Descriptor.ProjectType is ProjectType.DES)
        {
            var desMat = (FLVER0.Material)mat;
            var foundBoneIndices = false;
            var foundBoneWeights = false;

            if (desMat.Layouts?.Count > 0)
            {
                foreach (FLVER.LayoutMember? layoutType in desMat.Layouts[0])
                {
                    switch (layoutType.Semantic)
                    {
                        case FLVER.LayoutSemantic.Normal:
                            if (layoutType.Type == FLVER.LayoutType.UByte4 ||
                                layoutType.Type == FLVER.LayoutType.Byte4E ||
                                layoutType.Type == FLVER.LayoutType.Byte4)
                            {
                                dest.SetNormalWBoneTransform();
                            }

                            break;
                        case FLVER.LayoutSemantic.BoneIndices:
                            foundBoneIndices = true;
                            break;
                        case FLVER.LayoutSemantic.BoneWeights:
                            foundBoneWeights = true;
                            break;
                    }
                }
            }

            //Transformation condition for DeS models
            if (foundBoneIndices && !foundBoneWeights)
            {
                dest.SetHasIndexNoWeightTransform();
            }
        }

        if (!CFG.Current.Viewport_Enable_Texturing || ModelsOnly)
        {
            dest.ShaderName = @"SimpleFlver";
            dest.LayoutType = MeshLayoutType.LayoutSky;
            dest.VertexLayout = MeshLayoutUtils.GetLayoutDescription(dest.LayoutType);
            dest.VertexSize = MeshLayoutUtils.GetLayoutVertexSize(dest.LayoutType);
            return;
        }

        var layers = new MaterialLayers();
        LoadBlendRecipe(dest, mat.MTD, layers);

        foreach (IFlverTexture? matparam in mat.Textures)
        {
            ProcessMaterialTexture(dest, matparam.ParamName, matparam.Path, mat.MTD, layers);
        }

        LoadDeformTextures(dest, mat.MTD);
        ConfigureMaterialShader(dest, layers);
    }

    private unsafe void ProcessMaterial(FlverMaterial dest, BinaryReaderEx br,
        ref FlverMaterialDef mat, Span<FlverTexture> textures, bool isUTF)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        var mtd = isUTF ? br.GetUTF16(mat.mtdOffset) : br.GetShiftJIS(mat.mtdOffset);
        var matName = isUTF ? br.GetUTF16(mat.nameOffset) : br.GetShiftJIS(mat.nameOffset);
        dest.MaterialName = Path.GetFileNameWithoutExtension(mtd);
        dest.Definition = GetMaterialDefinition(mtd);
        if (!string.IsNullOrWhiteSpace(matName) && matName.Length >= 3 && matName[0] == '#' && char.IsDigit(matName[1]) && char.IsDigit(matName[2]))
        {
            dest.MaterialMask = int.Parse(matName.Substring(1, 2));
        }
        dest.MaterialBuffer = AllocateMaterialBuffer(dest);
        dest.MaterialData = new Material();

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
        LoadBlendRecipe(dest, mtd, layers);

        for (var i = mat.textureIndex; i < mat.textureIndex + mat.textureCount; i++)
        {
            var ttype = isUTF ? br.GetUTF16(textures[i].typeOffset) : br.GetShiftJIS(textures[i].typeOffset);
            var tpath = isUTF ? br.GetUTF16(textures[i].pathOffset) : br.GetShiftJIS(textures[i].pathOffset);
            ProcessMaterialTexture(dest, ttype, tpath, mtd, layers);
        }

        LoadDeformTextures(dest, mtd);
        ConfigureMaterialShader(dest, layers);
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
        dest[0] = PackUV(uv.X);
        dest[1] = PackUV(uv.Y);
    }

    /// <summary>
    /// Vertex UVs are stored as 16 bit fixed point with 11 fractional bits. Values are rounded rather than truncated;
    /// anything outside the representable range wraps, which the repeat sampler makes invisible for whole tiles.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static short PackUV(float value)
    {
        if (ReportUVRanges)
        {
            UvRangeTracker.Track(value);
        }

        return (short)(int)MathF.Round(value * 2048.0f);
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
    private unsafe void FillUVPairShort(FlverLayoutUV2* v, ref FLVER.Vertex vert)
    {
        FillUVShort((*v).Uv1, ref vert, 0);
        if (vert.UVs.Count > 1)
        {
            FillUVShort((*v).Uv2, ref vert, 1);
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
            v = new Vector3(br.ReadVector2(), 0);

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
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, float uvFactor, bool firstBuffer)
    {
        br.StepIn(buffer.bufferOffset);
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
                else
                {
                    EatVertex(br, l.type);
                }
            }

            pickingVerts[i] = (*v).Position;
        }

        br.StepOut();
    }

    private unsafe void FillVerticesStandard(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer)
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
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, float uvFactor, bool firstBuffer, ref int uvSetsFilled)
    {
        Span<FlverLayoutUV2> verts = new(vertBuffer.ToPointer(), buffer.vertexCount);
        br.StepIn(buffer.bufferOffset);
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
                    else if (l.semantic == FLVER.LayoutSemantic.UV && uvsfilled < 2)
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
                    else if (l.semantic == FLVER.LayoutSemantic.VertexColor && l.index == 0)
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
    /// Which of TEXCOORD1-3 a layout member holds, or -1 if it isn't one of them.
    /// Their position in the layout says nothing, as other members can come before them.
    /// </summary>
    private static int GetTexCoordIndex(FLVER.LayoutSemantic semantic, FLVER.LayoutType type, int index)
    {
        if (semantic != FLVER.LayoutSemantic.UV || index < 1 || index > FlverLayoutDeform.TexCoord3Index + 1)
            return -1;

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
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, float uvFactor, bool firstBuffer, ref int uvSetsFilled)
    {
        Span<FlverLayoutDeform> verts = new(vertBuffer.ToPointer(), buffer.vertexCount);
        br.StepIn(buffer.bufferOffset);
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
                foreach (FlverBufferLayoutMember l in layouts)
                {
                    // ER meme
                    if (l.IsSpecialMember)
                    {
                        continue;
                    }

                    var texCoord = GetTexCoordIndex(l.semantic, l.type, l.index);

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
                    else if (l.semantic == FLVER.LayoutSemantic.UV && uvsfilled < 2)
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
                    else if (l.semantic == FLVER.LayoutSemantic.VertexColor && l.index == 0)
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
    /// Finds where TEXCOORD1-3 ended up in the flat UV list of a vertex: each entry is the index of a
    /// member's first UV, by TEXCOORD, or -1 for one the mesh doesn't have.
    /// </summary>
    private int[] GetTexCoordUVs(FLVER2.Mesh mesh, out bool halfTexCoord3)
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

                var texCoord = GetTexCoordIndex(member.Semantic, member.Type, member.Index);
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

    private unsafe void FillVerticesDeform(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer)
    {
        var texCoordUVs = GetTexCoordUVs(mesh, out var halfTexCoord3);

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
                FillUVPairShort(v, ref vert);
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
            }
        }
    }

    private unsafe void FillVerticesUV2(FLVER2.Mesh mesh, Span<Vector3> pickingVerts, nint vertBuffer)
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

    private unsafe void ProcessMesh(FLVER0.Mesh mesh, FlverSubmesh dest)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        ResourceFactory? factory = SceneRenderer.Factory;

        if (FlverDeS == null)
            return;

        if (GPUMaterials == null)
            return;

        dest.Material = GPUMaterials[mesh.MaterialIndex];

        if (dest.Material.GetHasIndexNoWeightTransform())
        {
            //Transform based on root
            for (var v = 0; v < mesh.Vertices.Count; v++)
            {
                FLVER.Vertex vert = mesh.Vertices[v];
                var boneTransformationIndex = mesh.BoneIndices[vert.BoneIndices[0]];

                if (BoneTransforms != null)
                {
                    if (boneTransformationIndex > -1 && BoneTransforms.Count > boneTransformationIndex)
                    {
                        Matrix4x4 boneTfm = BoneTransforms[boneTransformationIndex];

                        vert.Position = Vector3.Transform(vert.Position, boneTfm);
                        vert.Normal = Vector3.TransformNormal(vert.Normal, boneTfm);
                        mesh.Vertices[v] = vert;
                    }
                }
            }
        }

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
                BackfaceCulling = true,
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

        // Untextured
        if (dest.Material.LayoutType == MeshLayoutType.LayoutSky)
        {
            FillVerticesNormalOnly(mesh, pvhandle, meshVertices);
        }
        else if (dest.Material.LayoutType == MeshLayoutType.LayoutUV2)
        {
            FillVerticesUV2(mesh, pvhandle, meshVertices);
        }
        else if (dest.Material.LayoutType == MeshLayoutType.LayoutDeform)
        {
            FillVerticesDeform(mesh, pvhandle, meshVertices);
        }
        else
        {
            FillVerticesStandard(mesh, pvhandle, meshVertices);
        }

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
            if (!CFG.Current.Viewport_Enable_LOD_Facesets &&
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
            IEnumerable<FLVER.LayoutMember> elements =
                mesh.VertexBuffers.SelectMany(b => Flver.BufferLayouts[b.LayoutIndex]);

            dest.UseNormalWBoneTransform = elements.Any(e =>
                e.Semantic == FLVER.LayoutSemantic.Normal &&
                (e.Type == FLVER.LayoutType.UByte4 || e.Type == FLVER.LayoutType.Byte4E));

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

        var firstBuffer = true;
        var uvSetsFilled = 0;

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

            if (dest.Material.LayoutType == MeshLayoutType.LayoutSky)
            {
                FillVerticesNormalOnly(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, ref posfilled);
            }
            else if (dest.Material.LayoutType == MeshLayoutType.LayoutUV2)
            {
                FillVerticesUV2(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000E ? 2048 : 1024, firstBuffer, ref uvSetsFilled);
            }
            else if (dest.Material.LayoutType == MeshLayoutType.LayoutDeform)
            {
                FillVerticesDeform(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000E ? 2048 : 1024, firstBuffer, ref uvSetsFilled);
            }
            else
            {
                FillVerticesStandard(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000E ? 2048 : 1024, firstBuffer);
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
                    if (idx > vertexCount)
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
            Bones = FlverDeS.Nodes;
            BoneTransforms = new List<Matrix4x4>();
            for (var i = 0; i < Bones.Count; i++)
            {
                //BoneTransforms.Add(FlverDeS.ComputeBoneWorldMatrix(i));
                BoneTransforms.Add(Bones[i].ComputeLocalTransform());
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
            Bones = Flver.Nodes;

            for (var i = 0; i < Flver.Materials.Count(); i++)
            {
                GPUMaterials[i] = new FlverMaterial();
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

    // Read only flver loader designed to be very fast at reading with low memory usage
    private bool LoadInternalFast(BinaryReaderEx br)
    {
        // TO FIX: NR flvers don't seem to work with this properly (reads but no rendering)

        // Parse header
        br.BigEndian = false;
        br.AssertASCII("FLVER\0");
        br.BigEndian = br.AssertASCII(["L\0", "B\0"]) == "B\0";
        var version = br.AssertInt32(
                [0x20005, 0x20007, 0x20009, 0x2000B, 0x2000C, 0x2000D, 0x2000E, 0x2000F, 0x20010, 0x20013, 0x20014, 0x20016, 0x20017, 0x2001A, 0x2001B, 0x20021, 0x20022]);

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
            if (!CFG.Current.Viewport_Enable_LOD_Facesets &&
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
        //Bones = Flver.Bones;

        for (var i = 0; i < materialCount; i++)
        {
            GPUMaterials[i] = new FlverMaterial();
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
            var tbones = new Matrix4x4[FBones.Count];
            for (var i = 0; i < FBones.Count; i++)
            {
                tbones[i] = GetBoneWorldMatrix(FBones, FBones[i]);
            }

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
            TextureResourceCount
        }

        // Indices of the default textures, see SceneRenderer
        private const ushort BlackTexture = 2;
        private const ushort WhiteTexture = 3;

        public readonly bool[] TextureResourceFilled = new bool[(int)TextureType.TextureResourceCount];

        public readonly ResourceHandle<TextureResource>?[] TextureResources =
            new ResourceHandle<TextureResource>[(int)TextureType.TextureResourceCount];

        private bool _setHasIndexNoWeightTransform;

        private bool _setNormalWBoneTransform;

        private bool disposedValue;
        public MeshLayoutType LayoutType;
        public GPUBufferAllocator.GPUBufferHandle? MaterialBuffer;
        public Material MaterialData;
        public string? MaterialName;
        public MaterialDefinition? Definition;

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

        /// <summary>Tiling and scroll of the layers past the second, gathered as textures are found</summary>
        public MaterialLayerTiling LayerTiling = MaterialLayerTiling.Default;
        public MaterialLayerScroll LayerScroll;
        public Vector4 DetailTiling = Vector4.One;
        public Vector2 MagmaTiling = Vector2.One;

        /// <summary>What the material's parameters multiply the first layer's albedo by</summary>
        public Vector3 AlbedoTint = Vector3.One;

        /// <summary>The material has no albedo texture, so <see cref="AlbedoTint"/> is its whole color</summary>
        public bool AlbedoFromColor;

        public MaterialBlending Blending = MaterialBlending.Opaque;
        public bool DepthWrite = true;

        public int MaterialMask = -1;

        public string? ShaderName;
        public List<SpecializationConstant>? SpecializationConstants;
        public VertexLayoutDescription VertexLayout;
        public uint VertexSize;

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void OnResourceLoaded(IResourceHandle handle, int tag)
        {
            var texHandle = (ResourceHandle<TextureResource>)handle;
            texHandle.Acquire();
            TextureResources[tag]?.Release();
            TextureResources[tag] = texHandle;
            UpdateMaterial();
        }

        public void OnResourceUnloaded(IResourceHandle handle, int tag)
        {
            TextureResources[tag]?.Release();
            TextureResources[tag] = null;
            UpdateMaterial();
        }

        public bool GetHasIndexNoWeightTransform()
        {
            return _setHasIndexNoWeightTransform;
        }

        public void SetHasIndexNoWeightTransform()
        {
            if (!_setHasIndexNoWeightTransform)
            {
                _setHasIndexNoWeightTransform = true;
            }
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

        /// <summary>Fills in the overlay's values, leaving the textures to <see cref="UpdateMaterial"/></summary>
        public void ApplyDetail(MaterialResolvedDetail detail)
        {
            Detail = new MaterialDetail
            {
                Tiling = Vector4.One,
                ColorStrength = new Vector4(detail.Color[0], detail.Color[1], detail.Color[2], detail.NormalStrength)
            };
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
        }

        /// <summary>
        /// Applies what the sampler chosen for a texture slot says about it. Only the slots of the first
        /// two layers have tiling, as those are the only ones a material can blend, and only the
        /// first layer has color and scrolling.
        /// </summary>
        public void ApplySampler(TextureType textureType, string? samplerType)
        {
            MaterialSampler? sampler = Definition?.Samplers.Find(x => x.Type == samplerType);

            if (sampler == null || sampler.Tiling.Length < 2)
                return;

            var tiling = sampler.Tiling;
            var scroll = sampler.Scroll;

            switch (textureType)
            {
                case TextureType.AlbedoTextureResource:
                    Shading.Tiling.X = tiling[0];
                    Shading.Tiling.Y = tiling[1];
                    Effects.Scroll.X = scroll[0];
                    Effects.Scroll.Y = scroll[1];

                    if (sampler.Color != null)
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

                    if (sampler.Color != null)
                    {
                        Effects.EmissiveColor = new Vector4(sampler.Color[0], sampler.Color[1], sampler.Color[2], 0);
                    }
                    break;
                case TextureType.MagmaTextureResource:
                    MagmaTiling.X = tiling[0];
                    MagmaTiling.Y = tiling[1];
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
            SetMaterialTexture(TextureType.AlbedoTextureResource2, ref MaterialData.colorTex2, 0);
            SetMaterialTexture(TextureType.NormalTextureResource, ref MaterialData.normalTex, 1);
            SetMaterialTexture(TextureType.NormalTextureResource2, ref MaterialData.normalTex2, 1);
            SetMaterialTexture(TextureType.SpecularTextureResource, ref MaterialData.specTex, 2);
            SetMaterialTexture(TextureType.SpecularTextureResource2, ref MaterialData.specTex2, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource, ref MaterialData.shininessTex, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource2, ref MaterialData.shininessTex2, 2);
            SetMaterialTexture(TextureType.BlendmaskTextureResource, ref MaterialData.blendMaskTex, 0);

            SetMaterialTexture(TextureType.AlbedoTextureResource3, ref MaterialData.colorTex3, 0);
            SetMaterialTexture(TextureType.AlbedoTextureResource4, ref MaterialData.colorTex4, 0);
            SetMaterialTexture(TextureType.NormalTextureResource3, ref MaterialData.normalTex3, 1);
            SetMaterialTexture(TextureType.NormalTextureResource4, ref MaterialData.normalTex4, 1);
            SetMaterialTexture(TextureType.SpecularTextureResource3, ref MaterialData.specTex3, 2);
            SetMaterialTexture(TextureType.SpecularTextureResource4, ref MaterialData.specTex4, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource3, ref MaterialData.shininessTex3, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource4, ref MaterialData.shininessTex4, 2);

            MaterialWithDeform? deformed = null;

            if (Deform is MaterialDeform deform)
            {
                ushort position = 0;
                ushort normal = 0;
                SetMaterialTexture(TextureType.VatPositionTextureResource, ref position, 0);
                SetMaterialTexture(TextureType.VatNormalTextureResource, ref normal, 0);
                deform.VatPositionTex = position;
                deform.VatNormalTex = normal;

                deformed = new MaterialWithDeform { Material = MaterialData, Shading = Shading, Effects = Effects, Deform = deform };
            }

            MaterialWithBlend? blended = null;

            // An overlay comes after the blend entries, which are just unused without a recipe
            if ((Detail != null || Magma != null ? Blend ?? new MaterialWithBlend() : Blend) is MaterialWithBlend blend)
            {
                ushort detail = 0;
                SetMaterialTexture(TextureType.DetailMaskTextureResource, ref detail, BlackTexture);

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

            if ((Magma != null ? Detail ?? new MaterialDetail() : Detail) is MaterialDetail overlay)
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

            if (Magma is MaterialMagma magma)
            {
                ushort magmaTex = 0;
                SetMaterialTexture(TextureType.MagmaTextureResource, ref magmaTex, 0);

                magma.Tex = magmaTex;
                magma.Tiling.X = MagmaTiling.X;
                magma.Tiling.Y = MagmaTiling.Y;
                glowing = new MaterialWithMagma { Detailed = detailed!.Value, Magma = magma };
            }

            MaterialWithShading shaded = new() { Material = MaterialData, Shading = Shading, Effects = Effects };

            SceneRenderer.AddBackgroundUploadTask((d, cl) =>
            {
                Profiler.___tracy_c_zone_context ctx = Profiler.TracyCZoneN(1, @"Material upload");
                if (MaterialBuffer != null)
                {
                    if (glowing is MaterialWithMagma magmaData)
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
                if (disposing)
                {
                    if (MaterialBuffer != null)
                    {
                        MaterialBuffer.Dispose();
                    }
                }

                ReleaseTextures();
                disposedValue = true;
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
                StaticBoneBuffer.Dispose();
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