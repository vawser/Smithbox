﻿#nullable enable
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using StudioCore.Scene;
using StudioCore.Scene.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using Veldrid.Utilities;
using static SoulsFormats.FLVER;

namespace StudioCore.Resource.Types;

public class FlverResource : IResource, IDisposable
{
    public class VertexInfo
    {
        public int MeshIndex;
        public int VertexIndex;
    }

    //private static ArrayPool<FlverLayout> VerticesPool = ArrayPool<FlverLayout>.Create();

    public const bool CaptureMaterialLayouts = false;
    //private static readonly Stack<FlverCache> FlverCaches = new();
    private static readonly object CacheLock = new();

    /// <summary>
    ///     Cache of material layouts that can be dumped
    /// </summary>
    public static Dictionary<string, FLVER2.BufferLayout> MaterialLayouts = new();

    public static object _matLayoutLock = new();
    public FLVER2? Flver;

    /// <summary>
    ///     Low level access to the flver struct. Use only in modification mode.
    /// </summary>
    public FLVER0? FlverDeS;

    public FlverMaterial[]? GPUMaterials;

    public FlverSubmesh[]? GPUMeshes;

    //public static int CacheCount { get; private set; }

    /*
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
    */

    public BoundingBox Bounds { get; set; }

    public List<FLVER.Node>? Bones { get; private set; }
    private List<FlverBone>? FBones { get; set; }
    private List<Matrix4x4>? BoneTransforms { get; set; }

    public bool IsSpeedtree = false;

    public GPUBufferAllocator.GPUBufferHandle? StaticBoneBuffer { get; private set; }

    /// <summary>
    /// Bytes
    /// </summary>
    public bool _Load(Memory<byte> bytes, AccessLevel al, string virtPath)
    {
        if (ResourceManager.BaseEditor.ProjectManager.SelectedProject != null)
        {
            var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

            // HACK: circumvent resource manager, grab updated FLVER data directly for representing the model
            if (virtPath.Contains("direct/flver"))
            {
                if (curProject.ModelEditor != null)
                {
                    var data = curProject.ModelEditor.ResManager.GetCurrentFLVER().Clone();
                    bytes = data.Write();
                }
            }

            bool ret;
            if (curProject.ProjectType is ProjectType.DES or ProjectType.ACFA)
            {
                FlverDeS = FLVER0.Read(bytes);
                ret = LoadInternalDeS(al);
            }
            else
            {
                if (al == AccessLevel.AccessGPUOptimizedOnly &&
                    curProject.ProjectType != ProjectType.DS1R &&
                    curProject.ProjectType != ProjectType.DS1)
                {
                    BinaryReaderEx br = new(false, bytes);
                    DCX.Type ctype;


                    Flver = FLVER2.Read(bytes);
                    ret = LoadInternal(al);

                    //br = SFUtil.GetDecompressedBR(br, out ctype);
                    //ret = LoadInternalFast(br);
                }
                else
                {
                    Flver = FLVER2.Read(bytes);
                    ret = LoadInternal(al);
                }
            }

            return ret;
        }

        return false;
    }

    /// <summary>
    /// Bytes
    /// </summary>
    public bool _Load(Memory<byte> bytes)
    {
        bool ret;

        Flver = FLVER2.Read(bytes);

        ret = LoadInternal(AccessLevel.AccessFull);

        return ret;
    }

    /// <summary>
    /// Path
    /// </summary>
    public bool _Load(string path, AccessLevel al, string virtPath)
    {
        if (ResourceManager.BaseEditor.ProjectManager.SelectedProject != null)
        {
            var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

            byte[] fileBytes = Array.Empty<byte>();

            // HACK: circumvent resource manager, grab updated FLVER data directly for representing the model
            if (virtPath.Contains("direct/flver"))
            {
                if (curProject.ModelEditor != null)
                {
                    var curFlver = curProject.ModelEditor.ResManager.GetCurrentFLVER();

                    if (curFlver != null)
                    {
                        var data = curFlver.Clone();
                        fileBytes = data.Write();
                    }
                }
            }
            else
            {
                fileBytes = File.ReadAllBytes(path);
            }

            bool ret = false;

            if (fileBytes.Length > 1)
            {
                if (curProject.ProjectType is ProjectType.DES or ProjectType.ACFA)
                {
                    FlverDeS = FLVER0.Read(fileBytes);
                    ret = LoadInternalDeS(al);
                }
                else
                {
                    if (al == AccessLevel.AccessGPUOptimizedOnly &&
                        curProject.ProjectType != ProjectType.DS1R &&
                        curProject.ProjectType != ProjectType.DS1)
                    {
                        BinaryReaderEx br = new(false, fileBytes);
                        DCX.Type ctype;
                        br = SFUtil.GetDecompressedBR(br, out ctype);
                        ret = LoadInternalFast(br);
                    }
                    else
                    {
                        Flver = FLVER2.Read(fileBytes);
                        ret = LoadInternal(al);
                    }
                }
            }

            return ret;
        }

        return false;
    }

    private void LookupTexture(FlverMaterial.TextureType textureType, FlverMaterial dest, string? type, string mpath,
        string mtd)
    {
        if (ResourceManager.BaseEditor.ProjectManager.SelectedProject == null)
            return;

        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        if (curProject.MaterialData == null || curProject.MaterialData.PrimaryBank == null)
            return;

        var bank = curProject.MaterialData.PrimaryBank;

        var path = mpath;
        if (mpath == "")
        {
            var mtdstring = Path.GetFileNameWithoutExtension(mtd.Replace('\\', Path.DirectorySeparatorChar));

            var material = bank.GetMaterial(mtdstring);

            if (material != null)
            {
                MTD.Texture? tex = material.Textures.Find(x => x.Type == type);
                if (tex == null || !tex.Extended || tex.Path == "")
                {
                    //TaskLogs.AddLog($"Failed to find MTD string: {mtdstring} - {type}");
                    return;
                }

                path = tex.Path;
                //TaskLogs.AddLog($"MTD: {path}");
            }

            if (curProject.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                var matbin = bank.GetMatbin(mtdstring);

                if (matbin != null)
                {
                    MATBIN.Sampler? tex = null;

                    //MATBIN.Sampler? tex = Smithbox.BankHandler.MaterialBank.Matbins[mtdstring].Matbin.Samplers.Find(x => x.Type == type);

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
                        //TaskLogs.AddLog($"Failed to find MATBIN string: {mtdstring} - {type}");
                        return;
                    }

                    path = tex.Path;
                    //TaskLogs.AddLog($"MATBIN: {path}");
                }
            }
        }

        if (!dest.TextureResourceFilled[(int)textureType])
        {
            string virtualPath = VirtualPathLocator.TexturePathToVirtual(path.ToLower());

            // Correct path if it is malformed (as game itself ignores this)
            //virtualPath = Smithbox.BankHandler.CorrectedTextureInfo.CorrectTexturePath(virtualPath);

            ResourceManager.AddResourceListener<TextureResource>(virtualPath, dest, AccessLevel.AccessGPUOptimizedOnly, (int)textureType);
            dest.TextureResourceFilled[(int)textureType] = true;
        }
    }

    private void ProcessMaterialTexture(FlverMaterial dest, string texType, string mpath, string mtd,
        out bool blend, out bool hasNormal2, out bool hasSpec2, out bool hasShininess2, out bool blendMask)
    {
        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        blend = false;
        blendMask = false;
        hasNormal2 = false;
        hasSpec2 = false;
        hasShininess2 = false;

        string paramNameCheck;

        if (texType == null)
        {
            paramNameCheck = "G_DIFFUSE";
        }
        else
        {
            paramNameCheck = texType.ToUpper();
        }

        if (paramNameCheck == "G_DIFFUSETEXTURE2" || paramNameCheck == "G_DIFFUSE2" ||
            paramNameCheck.Contains("ALBEDO_2"))
        {
            LookupTexture(FlverMaterial.TextureType.AlbedoTextureResource2, dest, texType, mpath, mtd);
            blend = true;
        }
        else if (paramNameCheck == "G_DIFFUSETEXTURE" || paramNameCheck == "G_DIFFUSE" ||
                 paramNameCheck.Contains("ALBEDO"))
        {
            LookupTexture(FlverMaterial.TextureType.AlbedoTextureResource, dest, texType, mpath, mtd);
        }
        else if (paramNameCheck == "G_BUMPMAPTEXTURE2" || paramNameCheck == "G_BUMPMAP2" ||
                 paramNameCheck.Contains("NORMAL_2"))
        {
            LookupTexture(FlverMaterial.TextureType.NormalTextureResource2, dest, texType, mpath, mtd);
            blend = true;
            hasNormal2 = true;
        }
        else if (paramNameCheck == "G_BUMPMAPTEXTURE" || paramNameCheck == "G_BUMPMAP" ||
                 paramNameCheck.Contains("NORMAL"))
        {
            LookupTexture(FlverMaterial.TextureType.NormalTextureResource, dest, texType, mpath, mtd);
        }
        else if (paramNameCheck == "G_SPECULARTEXTURE2" || paramNameCheck == "G_SPECULAR2" ||
                 paramNameCheck.Contains("SPECULAR_2"))
        {
            if (curProject.ProjectType is ProjectType.DS1R or ProjectType.DS2S or ProjectType.DS2)
            {
                LookupTexture(FlverMaterial.TextureType.ShininessTextureResource2, dest, texType, mpath, mtd);
                blend = true;
                hasShininess2 = true;
            }
            else
            {
                LookupTexture(FlverMaterial.TextureType.SpecularTextureResource2, dest, texType, mpath, mtd);
                blend = true;
                hasSpec2 = true;
            }
        }
        else if (paramNameCheck == "G_SPECULARTEXTURE" || paramNameCheck == "G_SPECULAR" ||
                 paramNameCheck.Contains("SPECULAR"))
        {
            if (curProject.ProjectType is ProjectType.DS1R or ProjectType.DS2S or ProjectType.DS2)
            {
                LookupTexture(FlverMaterial.TextureType.ShininessTextureResource, dest, texType, mpath, mtd);
            }
            else
            {
                LookupTexture(FlverMaterial.TextureType.SpecularTextureResource, dest, texType, mpath, mtd);
            }
        }
        else if (paramNameCheck == "G_SHININESSTEXTURE2" || paramNameCheck == "G_SHININESS2" ||
                 paramNameCheck.Contains("SHININESS2"))
        {
            LookupTexture(FlverMaterial.TextureType.ShininessTextureResource2, dest, texType, mpath, mtd);
            blend = true;
            hasShininess2 = true;
        }
        else if (paramNameCheck == "G_SHININESSTEXTURE" || paramNameCheck == "G_SHININESS" ||
                 paramNameCheck.Contains("SHININESS"))
        {
            LookupTexture(FlverMaterial.TextureType.ShininessTextureResource, dest, texType, mpath, mtd);
        }
        else if (paramNameCheck.Contains("BLENDMASK"))
        {
            LookupTexture(FlverMaterial.TextureType.BlendmaskTextureResource, dest, texType, mpath, mtd);
            blendMask = true;
        }
    }

    private unsafe void ProcessMaterial(IFlverMaterial mat, FlverMaterial dest)
    {
        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        dest.MaterialName = Path.GetFileNameWithoutExtension(mat.MTD);
        dest.MaterialBuffer = Renderer.MaterialBufferAllocator.Allocate((uint)sizeof(Material), sizeof(Material));
        dest.MaterialData = new Material();

        var matName = mat.Name;
        if (!string.IsNullOrWhiteSpace(matName) && matName.Length >= 3 && matName[0] == '#' && char.IsDigit(matName[1]) && char.IsDigit(matName[2]))
        {
            dest.MaterialMask = int.Parse(matName.Substring(1, 2));
        }

        dest.SpecializationConstants = new List<SpecializationConstant>();

        //FLVER0 stores layouts directly in the material
        if (curProject.ProjectType is ProjectType.DES or ProjectType.ACFA)
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
                            if (layoutType.Type == FLVER.LayoutType.Byte4B ||
                                layoutType.Type == FLVER.LayoutType.Byte4E ||
                                layoutType.Type == FLVER.LayoutType.Short2toFloat2)
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

        if (!CFG.Current.Viewport_Enable_Texturing)
        {
            dest.ShaderName = @"SimpleFlver";
            dest.LayoutType = MeshLayoutType.LayoutSky;
            dest.VertexLayout = MeshLayoutUtils.GetLayoutDescription(dest.LayoutType);
            dest.VertexSize = MeshLayoutUtils.GetLayoutVertexSize(dest.LayoutType);
            return;
        }

        var blend = false;
        var blendMask = false;
        var hasNormal2 = false;
        var hasSpec2 = false;
        var hasShininess2 = false;

        foreach (IFlverTexture? matparam in mat.Textures)
        {
            ProcessMaterialTexture(dest, matparam.Type, matparam.Path, mat.MTD, out blend, out hasNormal2, out hasSpec2, out hasShininess2, out blendMask);
        }

        if (blendMask)
        {
            dest.ShaderName = @"FlverShader\FlverShader_blendmask";
            dest.LayoutType = MeshLayoutType.LayoutUV2;
        }
        else if (blend)
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
        specConstants.Add(new SpecializationConstant(0, (uint)curProject.ProjectType));
        if (blend || blendMask)
        {
            specConstants.Add(new SpecializationConstant(1, hasNormal2));
            specConstants.Add(new SpecializationConstant(2, hasSpec2));
            specConstants.Add(new SpecializationConstant(3, hasShininess2));
        }

        dest.SpecializationConstants.AddRange(specConstants);
        dest.VertexLayout = MeshLayoutUtils.GetLayoutDescription(dest.LayoutType);
        dest.VertexSize = MeshLayoutUtils.GetLayoutVertexSize(dest.LayoutType);

        dest.UpdateMaterial();
    }

    private unsafe void ProcessMaterial(FlverMaterial dest, BinaryReaderEx br,
        ref FlverMaterialDef mat, Span<FlverTexture> textures, bool isUTF)
    {
        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        var mtd = isUTF ? br.GetUTF16(mat.mtdOffset) : br.GetShiftJIS(mat.mtdOffset);
        var matName = isUTF ? br.GetUTF16(mat.nameOffset) : br.GetShiftJIS(mat.nameOffset);
        dest.MaterialName = Path.GetFileNameWithoutExtension(mtd);
        if (!string.IsNullOrWhiteSpace(matName) && matName.Length >= 3 && matName[0] == '#' && char.IsDigit(matName[1]) && char.IsDigit(matName[2]))
        {
            dest.MaterialMask = int.Parse(matName.Substring(1, 2));
        }
        dest.MaterialBuffer = Renderer.MaterialBufferAllocator.Allocate((uint)sizeof(Material), sizeof(Material));
        dest.MaterialData = new Material();

        if (!CFG.Current.Viewport_Enable_Texturing)
        {
            dest.ShaderName = @"SimpleFlver";
            dest.LayoutType = MeshLayoutType.LayoutSky;
            dest.VertexLayout = MeshLayoutUtils.GetLayoutDescription(dest.LayoutType);
            dest.VertexSize = MeshLayoutUtils.GetLayoutVertexSize(dest.LayoutType);
            dest.SpecializationConstants = new List<SpecializationConstant>();
            return;
        }

        var blend = false;
        var blendMask = false;
        var hasNormal2 = false;
        var hasSpec2 = false;
        var hasShininess2 = false;

        for (var i = mat.textureIndex; i < mat.textureIndex + mat.textureCount; i++)
        {
            var ttype = isUTF ? br.GetUTF16(textures[i].typeOffset) : br.GetShiftJIS(textures[i].typeOffset);
            var tpath = isUTF ? br.GetUTF16(textures[i].pathOffset) : br.GetShiftJIS(textures[i].pathOffset);
            ProcessMaterialTexture(dest, ttype, tpath, mtd, out blend, out hasNormal2, out hasSpec2, out hasShininess2, out blendMask);
        }

        if (blendMask)
        {
            dest.ShaderName = @"FlverShader\FlverShader_blendmask";
            dest.LayoutType = MeshLayoutType.LayoutUV2;
        }
        else if (blend)
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
        specConstants.Add(new SpecializationConstant(0, (uint)curProject.ProjectType));
        if (blend || blendMask)
        {
            specConstants.Add(new SpecializationConstant(1, hasNormal2));
            specConstants.Add(new SpecializationConstant(2, hasSpec2));
            specConstants.Add(new SpecializationConstant(3, hasShininess2));
        }

        dest.SpecializationConstants = specConstants;
        dest.VertexLayout = MeshLayoutUtils.GetLayoutDescription(dest.LayoutType);
        dest.VertexSize = MeshLayoutUtils.GetLayoutVertexSize(dest.LayoutType);

        dest.UpdateMaterial();
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
            throw new NotImplementedException($"Read not implemented for {type} vertex.");
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
                throw new InvalidDataException($"Float4 Normal W was not a whole number: {w}");
            }
        }
        else if (type == FLVER.LayoutType.Byte4A)
        {
            *n = FLVER.Vertex.ReadByteNormXYZ(br);
            nw = br.ReadByte();
        }
        else if (type == FLVER.LayoutType.Byte4B)
        {
            *n = FLVER.Vertex.ReadByteNormXYZ(br);
            nw = br.ReadByte();
        }
        else if (type == FLVER.LayoutType.Short2toFloat2)
        {
            nw = br.ReadByte();
            *n = FLVER.Vertex.ReadSByteNormZYX(br);
        }
        else if (type == FLVER.LayoutType.Byte4C)
        {
            *n = FLVER.Vertex.ReadByteNormXYZ(br);
            nw = br.ReadByte();
        }
        else if (type == FLVER.LayoutType.Short4toFloat4A)
        {
            *n = FLVER.Vertex.ReadShortNormXYZ(br);
            nw = br.ReadInt16();
        }
        else if (type == FLVER.LayoutType.Short4toFloat4B)
        {
            //Normal = ReadUShortNormXYZ(br);
            *n = FLVER.Vertex.ReadFloat16NormXYZ(br);
            nw = br.ReadInt16();
        }
        else if (type == FLVER.LayoutType.Byte4E)
        {
            *n = FLVER.Vertex.ReadByteNormXYZ(br);
            nw = br.ReadByte();
        }
        else if (type == FLVER.LayoutType.ShortBoneIndices)
        {
            *n = FLVER.Vertex.ReadShortNormXYZ(br);
            nw = br.ReadInt16();
        }
        else
        {
            throw new NotImplementedException($"Read not implemented for {type} normal.");
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
        Vector3 uv = v.UVs[index];
        dest[0] = (short)(uv.X * 2048.0f);
        dest[1] = (short)(uv.Y * 2048.0f);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void FillUVShort(short* dest, BinaryReaderEx br, FLVER.LayoutType type, float uvFactor,
        bool allowv2, out bool hasv2)
    {
        Vector3 v;
        Vector3 v2;
        hasv2 = false;
        if (type == FLVER.LayoutType.Float2)
        {
            v = new Vector3(br.ReadVector2(), 0);
        }
        else if (type == FLVER.LayoutType.Float3)
        {
            v = br.ReadVector3();
        }
        else if (type == FLVER.LayoutType.Float4)
        {
            v = new Vector3(br.ReadVector2(), 0);
            v2 = new Vector3(br.ReadVector2(), 0);
            hasv2 = allowv2;
        }
        else if (type == FLVER.LayoutType.Byte4A)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
        }
        else if (type == FLVER.LayoutType.Byte4B)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
        }
        else if (type == FLVER.LayoutType.Short2toFloat2)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
        }
        else if (type == FLVER.LayoutType.Byte4C)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
        }
        else if (type == FLVER.LayoutType.UV)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
        }
        else if (type == FLVER.LayoutType.UVPair)
        {
            v = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
            v2 = new Vector3(br.ReadInt16(), br.ReadInt16(), 0) / uvFactor;
            hasv2 = allowv2;
        }
        else if (type == FLVER.LayoutType.Short4toFloat4B)
        {
            //AddUV(new Vector3(br.ReadInt16(), br.ReadInt16(), br.ReadInt16()) / uvFactor);
            v = FLVER.Vertex.ReadFloat16NormXYZ(br);
            br.AssertInt16(0);
        }
        else
        {
            throw new NotImplementedException($"Read not implemented for {type} UV.");
        }

        dest[0] = (short)(v.X * 2048.0f);
        dest[1] = (short)(v.Y * 2048.0f);
        if (hasv2)
        {
            dest[3] = (short)(v.X * 2048.0f);
            dest[4] = (short)(v.Y * 2048.0f);
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
        else if (type == FLVER.LayoutType.Byte4A)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else if (type == FLVER.LayoutType.Byte4B)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else if (type == FLVER.LayoutType.Byte4C)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else if (type == FLVER.LayoutType.Short4toFloat4A)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else if (type == FLVER.LayoutType.Byte4E)
        {
            tan = FLVER.Vertex.ReadByteNormXYZW(br);
        }
        else
        {
            throw new NotImplementedException($"Read not implemented for {type} tangent.");
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
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EatVertex(BinaryReaderEx br, FLVER.LayoutType type)
    {
        switch (type)
        {
            case FLVER.LayoutType.Byte4A:
            case FLVER.LayoutType.Byte4B:
            case FLVER.LayoutType.Short2toFloat2:
            case FLVER.LayoutType.Byte4C:
            case FLVER.LayoutType.UV:
            case FLVER.LayoutType.Byte4E:
            case FLVER.LayoutType.Short2ToFloat2B:
                br.ReadUInt32();
                break;

            case FLVER.LayoutType.Float2:
            case FLVER.LayoutType.UVPair:
            case FLVER.LayoutType.ShortBoneIndices:
            case FLVER.LayoutType.Short4toFloat4A:
            case FLVER.LayoutType.Short4toFloat4B:
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
                throw new NotImplementedException($"No size defined for buffer layout type: {type}");
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
                    if (l.unk00 == -2147483647)
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
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, float uvFactor)
    {
        br.StepIn(buffer.bufferOffset);
        var pverts = (FlverLayout*)vertBuffer;

        for (var i = 0; i < buffer.vertexCount; i++)
        {
            FlverLayout* v = &pverts[i];
            Vector3 n = Vector3.UnitX;
            FillUVShortZero((*v).Uv1);
            FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
            var posfilled = false;
            foreach (FlverBufferLayoutMember l in layouts)
            {
                // ER meme
                if (l.unk00 == -2147483647)
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
                else if (l.semantic == FLVER.LayoutSemantic.UV && l.index == 0)
                {
                    bool hasv2;
                    FillUVShort((*v).Uv1, br, l.type, uvFactor, false, out hasv2);
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

            if (!posfilled)
            {
                (*v).Position = new Vector3(0, 0, 0);
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
        Span<FlverBufferLayoutMember> layouts, Span<Vector3> pickingVerts, nint vertBuffer, int[] meshBoneIndices, bool useNormalWTransform, float uvFactor)
    {
        Span<FlverLayoutUV2> verts = new(vertBuffer.ToPointer(), buffer.vertexCount);
        br.StepIn(buffer.bufferOffset);
        fixed (FlverLayoutUV2* pverts = verts)
        {
            for (var i = 0; i < buffer.vertexCount; i++)
            {
                FlverLayoutUV2* v = &pverts[i];
                Vector3 n = Vector3.UnitX;
                FillBinormalBitangentSNorm8Zero((*v).Binormal, (*v).Bitangent);
                var uvsfilled = 0;
                foreach (FlverBufferLayoutMember l in layouts)
                {
                    // ER meme
                    if (l.unk00 == -2147483647)
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
                        FillUVShort(uvsfilled > 0 ? (*v).Uv2 : (*v).Uv1, br, l.type, uvFactor, false, out hasv2);
                        uvsfilled += hasv2 ? 2 : 1;
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
        }

        br.StepOut();
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
                FillUVShort((*v).Uv1, ref vert, 0);
                FillUVShort((*v).Uv2, ref vert, 1);
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
                FillUVShort((*v).Uv1, ref vert, 0);
                FillUVShort((*v).Uv2, ref vert, 1);
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
        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        ResourceFactory? factory = Renderer.Factory;

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

        // Move NormalW local indices to global bone indices for shader
        if (curProject.ProjectType is ProjectType.ACFA && mesh.Dynamic != 1)
        {
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vert = mesh.Vertices[i];
                if (vert.NormalW < mesh.BoneIndices.Length && vert.NormalW >= 0)
                {
                    mesh.Vertices[i].NormalW = mesh.BoneIndices[vert.NormalW];
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

        var indices = mesh.Triangulate(FlverDeS.Header.Version).ToArray();
        var indicesTotal = indices.Length;

        dest.GeomBuffer = Renderer.GeometryBufferAllocator.Allocate(vbuffersize,
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

        if (mesh.VertexIndices.Count != 0)
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

        var curProject = ResourceManager.BaseEditor.ProjectManager.SelectedProject;

        // Move NormalW local indices to global bone indices for shader
        if (curProject.ProjectType is ProjectType.ACV or ProjectType.ACVD && mesh.Dynamic != 1)
        {
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var vert = mesh.Vertices[i];
                if (vert.NormalW < mesh.BoneIndices.Count && vert.NormalW >= 0)
                {
                    mesh.Vertices[i].NormalW = mesh.BoneIndices[vert.NormalW];
                }
            }
        }

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
        dest.GeomBuffer = Renderer.GeometryBufferAllocator.Allocate(vbuffersize,
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
                (e.Type == FLVER.LayoutType.Byte4B || e.Type == FLVER.LayoutType.Byte4E));

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
        dest.GeomBuffer = Renderer.GeometryBufferAllocator.Allocate(vbuffersize,
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
                    (layoutmembers[i].type == FLVER.LayoutType.Byte4A ||
                     layoutmembers[i].type == FLVER.LayoutType.Byte4B ||
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
                FillVerticesUV2(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000F ? 2048 : 1024);
            }
            else
            {
                FillVerticesStandard(br, ref vb, layoutmembers, pvhandle, meshVertices, meshBoneIndices, dest.UseNormalWBoneTransform, version >= 0x2000F ? 2048 : 1024);
            }
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
            ushort[] memberIndexes = FLVER2.EdgeMemberInfo.DecompressIndexes(br, edgeMember.spuConfigInfo.numIndexes);
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
            ushort[] memberIndexes = FLVER2.EdgeMemberInfo.DecompressIndexes(br, edgeMember.spuConfigInfo.numIndexes);

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
        if (!CFG.Current.Viewport_Enable_Rendering)
        {
            return true;
        }

        if (FlverDeS == null)
            return false;

        if (GPUMaterials == null)
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
                ProcessMesh(flverMesh, GPUMeshes[i]);
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

        if (GPUMaterials.Any(e => e.GetNormalWBoneTransform()))
        {
            if (Bones != null)
            {
                StaticBoneBuffer = Renderer.BoneBufferAllocator.Allocate(64 * (uint)Bones.Count, 64);
                var tbones = new Matrix4x4[Bones.Count];
                for (var i = 0; i < Bones.Count; i++)
                {
                    tbones[i] = Utils.GetBoneWorldMatrix(Bones[i], Bones);
                }

                Renderer.AddBackgroundUploadTask((d, cl) =>
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
                ProcessMesh(Flver.Meshes[i], GPUMeshes[i]);
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
                StaticBoneBuffer = Renderer.BoneBufferAllocator.Allocate(64 * (uint)Bones.Count, 64);
                var tbones = new Matrix4x4[Bones.Count];
                for (var i = 0; i < Bones.Count; i++)
                {
                    tbones[i] = Utils.GetBoneWorldMatrix(Bones[i], Bones);
                }

                Renderer.AddBackgroundUploadTask((d, cl) =>
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
        // Parse header
        br.BigEndian = false;
        br.AssertASCII("FLVER\0");
        br.BigEndian = br.AssertASCII(["L\0", "B\0"]) == "B\0";
        var version = br.AssertInt32([0x20005,
            0x20007,
            0x20009,
            0x2000C,
            0x2000D,
            0x2000E,
            0x2000F,
            0x20010,
            0x20013,
            0x20014,
            0x20016,
            0x2001A,
            0x2001B,
            0x20021]);

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
        br.AssertInt32(0);
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
            ProcessMesh(ref meshes[i], br, version, vertexbuffers, bufferLayouts, facesets, GPUMeshes[i]);
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
            StaticBoneBuffer = Renderer.BoneBufferAllocator.Allocate(64 * (uint)FBones.Count, 64);
            var tbones = new Matrix4x4[FBones.Count];
            for (var i = 0; i < FBones.Count; i++)
            {
                tbones[i] = GetBoneWorldMatrix(FBones, FBones[i]);
            }

            Renderer.AddBackgroundUploadTask((d, cl) =>
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
            TextureResourceCount
        }

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
            SetMaterialTexture(TextureType.AlbedoTextureResource, ref MaterialData.colorTex, 0);
            SetMaterialTexture(TextureType.AlbedoTextureResource2, ref MaterialData.colorTex2, 0);
            SetMaterialTexture(TextureType.NormalTextureResource, ref MaterialData.normalTex, 1);
            SetMaterialTexture(TextureType.NormalTextureResource2, ref MaterialData.normalTex2, 1);
            SetMaterialTexture(TextureType.SpecularTextureResource, ref MaterialData.specTex, 2);
            SetMaterialTexture(TextureType.SpecularTextureResource2, ref MaterialData.specTex2, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource, ref MaterialData.shininessTex, 2);
            SetMaterialTexture(TextureType.ShininessTextureResource2, ref MaterialData.shininessTex2, 2);
            SetMaterialTexture(TextureType.BlendmaskTextureResource, ref MaterialData.blendMaskTex, 0);

            Renderer.AddBackgroundUploadTask((d, cl) =>
            {
                Tracy.___tracy_c_zone_context ctx = Tracy.TracyCZoneN(1, @"Material upload");
                if (MaterialBuffer != null)
                {
                    MaterialBuffer.FillBuffer(d, cl, ref MaterialData);
                }
                Tracy.TracyCZoneEnd(ctx);
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
            vertexBufferCount = br.AssertInt32([0, 1, 2, 3]);
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
            br.ReadByte(); // unk
            br.ReadByte(); // unk
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
