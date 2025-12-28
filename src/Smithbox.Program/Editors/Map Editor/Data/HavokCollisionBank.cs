using HKLib.hk2018;
using HKLib.hk2018.hkcdStaticMeshTree;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Xml;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class HavokCollisionBank
{
    public EditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, hkRootLevelContainer> HavokContainers = new Dictionary<string, hkRootLevelContainer>();

    public HavokCollisionType VisibleCollisionType = HavokCollisionType.Low;

    public HavokCollisionBank(EditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnLoadMap(string mapId)
    {
        if (!CFG.Current.MapEditor_ModelLoad_Collisions)
            return;

        if (Project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            LoadMapCollision(mapId, "h");
            LoadMapCollision(mapId, "l");
        }
    }

    public void OnUnloadMap(string mapId)
    {
        if (!CFG.Current.MapEditor_ModelLoad_Collisions)
            return;

        if (Project.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            // HACK: clear all viewport collisions on load
            foreach (KeyValuePair<string, IResourceHandle> item in ResourceManager.GetResourceDatabase())
            {
                if (item.Key.Contains("collision"))
                {
                    item.Value.Release(true);
                }
            }
        }
    }

    private void LoadMapCollision(string mapId, string type)
    {
        byte[] CompendiumBytes = null;

        var bdtPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbdt");
        var bhdPath = Path.Join("map", mapId.Substring(0, 3), mapId, $"{type}{mapId.Substring(1)}.hkxbhd");

        try
        {
            var bdtData = Project.FS.ReadFile(bdtPath);
            var bhdData = Project.FS.ReadFile(bhdPath);

            if (bdtData == null || bhdData == null)
                return;

            var packedBinder = BXF4.Read((Memory<byte>)bhdData, (Memory<byte>)bdtData);

            HavokBinarySerializer serializer = new HavokBinarySerializer();
            HavokXmlSerializer xmlSerializer = null;

            // Get compendium
            foreach (var file in packedBinder.Files)
            {
                if (file.Name.Contains(".compendium.dcx"))
                {
                    CompendiumBytes = DCX.Decompress(file.Bytes).ToArray();
                }
            }

            foreach (var file in packedBinder.Files)
            {
                var parts = file.Name.Split('\\');

                if (parts.Length != 2)
                    continue;

                var name = parts[1];

                if (!file.Name.Contains(".hkx.dcx"))
                    continue;

                var FileBytes = DCX.Decompress(file.Bytes).ToArray();

                try
                {
                    if (CompendiumBytes != null)
                    {
                        using MemoryStream memoryStream = new MemoryStream(CompendiumBytes);
                        serializer.LoadCompendium(memoryStream);
                    }

                    using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                    {
                        hkRootLevelContainer fileHkx;
                        try
                        {
                            fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);
                        }
                        catch (InvalidDataException)
                        {
                            if (xmlSerializer == null)
                                xmlSerializer = new HavokXmlSerializer();
                            memoryStream.Position = 0;
                            fileHkx = (hkRootLevelContainer)xmlSerializer.Read(memoryStream);
                        }

                        if (!HavokContainers.ContainsKey(name))
                        {
                            HavokContainers.Add(name, fileHkx);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"[{Project}:Map Editor] Failed to serialize havok file: {name}", LogLevel.Error, LogPriority.High, ex);
                }
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project}:Map Editor] Failed to load map collision: {bdtPath}", LogLevel.Error, LogPriority.High, e);
        }
    }

    #region Utils
    public static (HavokCollisionResource.CollisionSubmesh, List<Vector3>, List<int>) ProcessColData(
    hknpCompressedMeshShapeData coldata,
    hknpBodyCinfo bodyinfo,
    HavokCollisionResource.CollisionSubmesh mesh)
    {
        List<Vector3> vector3List = new List<Vector3>();
        List<int> intList = new List<int>();

        for (int index1 = 0; index1 < coldata.m_meshTree.m_sections.Count; ++index1)
        {
            Section section = coldata.m_meshTree.m_sections[index1];
            for (int index2 = 0; index2 < (int)section.m_numPrimitives; ++index2)
            {
                Primitive primitive = coldata.m_meshTree.m_primitives[index2 + (int)section.m_firstPrimitiveIndex];
                if (primitive.m_indices[0] != (byte)222 || primitive.m_indices[1] != (byte)173 || primitive.m_indices[2] != (byte)222 || primitive.m_indices[3] != (byte)173)
                {
                    byte numPackedVertices = section.m_numPackedVertices;
                    uint sharedVertexIndex = section.m_firstSharedVertexIndex;
                    Vector3 offset = new Vector3(section.m_codecParms[0], section.m_codecParms[1], section.m_codecParms[2]);
                    Vector3 scale = new Vector3(section.m_codecParms[3], section.m_codecParms[4], section.m_codecParms[5]);
                    if ((int)primitive.m_indices[0] < (int)numPackedVertices)
                    {
                        ushort index3 = (ushort)((uint)primitive.m_indices[0] + section.m_firstPackedVertexIndex);
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(int)index3], scale, offset);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    else
                    {
                        ushort index4 = coldata.m_meshTree.m_sharedVerticesIndex[(int)primitive.m_indices[0] + (int)sharedVertexIndex - (int)numPackedVertices];
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[(int)index4], coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    if ((int)primitive.m_indices[1] < (int)numPackedVertices)
                    {
                        ushort index5 = (ushort)((uint)primitive.m_indices[1] + section.m_firstPackedVertexIndex);
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(int)index5], scale, offset);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    else
                    {
                        ushort index6 = coldata.m_meshTree.m_sharedVerticesIndex[(int)primitive.m_indices[1] + (int)sharedVertexIndex - (int)numPackedVertices];
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[(int)index6], coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    if ((int)primitive.m_indices[2] < (int)numPackedVertices)
                    {
                        ushort index7 = (ushort)((uint)primitive.m_indices[2] + section.m_firstPackedVertexIndex);
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(int)index7], scale, offset);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    else
                    {
                        ushort index8 = coldata.m_meshTree.m_sharedVerticesIndex[(int)primitive.m_indices[2] + (int)sharedVertexIndex - (int)numPackedVertices];
                        intList.Add(vector3List.Count);
                        Vector3 vert = DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[(int)index8], coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                        vector3List.Add(TransformVert(vert, bodyinfo));
                    }
                    if ((int)primitive.m_indices[2] != (int)primitive.m_indices[3])
                    {
                        intList.Add(vector3List.Count);
                        vector3List.Add(vector3List[vector3List.Count - 3]);
                        intList.Add(vector3List.Count);
                        vector3List.Add(vector3List[vector3List.Count - 2]);
                        if ((int)primitive.m_indices[3] < (int)numPackedVertices)
                        {
                            ushort index9 = (ushort)((uint)primitive.m_indices[3] + section.m_firstPackedVertexIndex);
                            intList.Add(vector3List.Count);
                            Vector3 vert = DecompressPackedVertex(coldata.m_meshTree.m_packedVertices[(int)index9], scale, offset);
                            vector3List.Add(TransformVert(vert, bodyinfo));
                        }
                        else
                        {
                            ushort index10 = coldata.m_meshTree.m_sharedVerticesIndex[(int)primitive.m_indices[3] + (int)sharedVertexIndex - (int)numPackedVertices];
                            intList.Add(vector3List.Count);
                            Vector3 vert = DecompressSharedVertex(coldata.m_meshTree.m_sharedVertices[(int)index10], coldata.m_meshTree.m_domain.m_min, coldata.m_meshTree.m_domain.m_max);
                            vector3List.Add(TransformVert(vert, bodyinfo));
                        }
                    }
                }
            }
        }

        return (mesh, vector3List, intList);
    }

    private static Vector3 TransformVert(Vector3 vert, hknpBodyCinfo body)
    {
        Vector3 vector3_1 = new Vector3(vert.X, vert.Y, vert.Z);
        if (body == null)
            return vector3_1;
        Vector3 vector3_2 = new Vector3(body.m_position.X, body.m_position.Y, body.m_position.Z);
        return Vector3.Transform(vector3_1, body.m_orientation) + vector3_2;
    }

    private static Vector3 DecompressSharedVertex(ulong vertex, Vector4 bbMin, Vector4 bbMax)
    {
        float num1 = (float)(((double)bbMax.X - (double)bbMin.X) / 2097151.0);
        float num2 = (float)(((double)bbMax.Y - (double)bbMin.Y) / 2097151.0);
        float num3 = (float)(((double)bbMax.Z - (double)bbMin.Z) / 4194303.0);
        double x = (double)(vertex & 2097151UL) * (double)num1 + (double)bbMin.X;
        float num4 = (float)(vertex >> 21 & 2097151UL) * num2 + bbMin.Y;
        float num5 = (float)(vertex >> 42 & 4194303UL) * num3 + bbMin.Z;
        double y = (double)num4;
        double z = (double)num5;
        return new Vector3((float)x, (float)y, (float)z);
    }

    private static Vector3 DecompressPackedVertex(uint vertex, Vector3 scale, Vector3 offset)
    {
        double x = (double)(vertex & 2047U) * (double)scale.X + (double)offset.X;
        float num1 = (float)(vertex >> 11 & 2047U) * scale.Y + offset.Y;
        float num2 = (float)(vertex >> 22 & 1023U) * scale.Z + offset.Z;
        double y = (double)num1;
        double z = (double)num2;
        return new Vector3((float)x, (float)y, (float)z);
    }
    #endregion
}
