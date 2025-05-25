using HKLib.hk2018;
using HKLib.hk2018.hkcdStaticMeshTree;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ModelEditor.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static StudioCore.Resource.Types.HavokCollisionResource;

namespace StudioCore.Resource;

public enum HavokCollisionType
{
    Low,
    High
}

public class HavokCollisionManager
{
    public EditorScreen Editor;
    public ProjectEntry Project;

    public Dictionary<string, hkRootLevelContainer> HavokContainers = new Dictionary<string, hkRootLevelContainer>();

    public HavokCollisionType VisibleCollisionType = HavokCollisionType.Low;

    public HavokCollisionManager(EditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnLoadMap(string mapId)
    {
        if (!CFG.Current.MapEditor_LoadCollisions_ER)
            return;

        if (Project.ProjectType == ProjectType.ER)
        {
            LoadMapCollision(mapId, "h");
            LoadMapCollision(mapId, "l");
        }
    }

    public void OnUnloadMap(string mapId)
    {
        if (!CFG.Current.MapEditor_LoadCollisions_ER)
            return;

        if (Project.ProjectType == ProjectType.ER)
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
        // Mark as invalid by default
        bool isValid = false;
        byte[] CompendiumBytes = null;

        var bdtPath = $"{Project.DataPath}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{type}{mapId.Substring(1)}.hkxbdt";
        var bhdPath = $"{Project.DataPath}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{type}{mapId.Substring(1)}.hkxbhd";

        // If game root version exists, mark as valid
        if (File.Exists(bdtPath) && File.Exists(bhdPath))
        {
            isValid = true;
        }

        // If project version exists, point path to that instead, and mark as valid
        var projectBdtPath = $"{Project.ProjectPath}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{type}{mapId.Substring(1)}.hkxbdt";
        var projectBhdPath = $"{Project.ProjectPath}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{type}{mapId.Substring(1)}.hkxbhd";

        if (File.Exists(projectBdtPath) && File.Exists(projectBhdPath))
        {
            bdtPath = projectBdtPath;
            bhdPath = projectBhdPath;
            isValid = true; // Load project if they are custom hkxbhd/hkxbdt
        }

        // If not marked as valid, return early to avoid bad read
        if (!isValid)
        {
            return;
        }

        BXF4Reader reader = new BXF4Reader(bhdPath, bdtPath);

        // Get compendium
        foreach (var file in reader.Files)
        {
            BinderFileHeader f = file;

            if (file.Name.Contains(".compendium.dcx"))
            {
                Memory<byte> bytes = reader.ReadFile(f);
                CompendiumBytes = DCX.Decompress(bytes).ToArray();
            }
        }

        if (CompendiumBytes != null)
        {
            // Read collisions
            foreach (var file in reader.Files)
            {
                BinderFileHeader f = file;

                var parts = f.Name.Split('\\');
                if (parts.Length == 2)
                {
                    var name = parts[1];

                    if (file.Name.Contains(".hkx.dcx"))
                    {
                        Memory<byte> bytes = reader.ReadFile(f);
                        var FileBytes = DCX.Decompress(bytes).ToArray();

                        try
                        {
                            HavokBinarySerializer serializer = new HavokBinarySerializer();
                            using (MemoryStream memoryStream = new MemoryStream(CompendiumBytes))
                            {
                                serializer.LoadCompendium(memoryStream);
                            }
                            using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                            {
                                var fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                                if (!HavokContainers.ContainsKey(name))
                                {
                                    HavokContainers.Add(name, fileHkx);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddLog($"Failed to serialize havok file: {name} - {ex}");
                        }
                    }
                }
            }
        }
    }

    public void OnLoadModel(string modelName, FlverContainerType modelType)
    {
        if (!CFG.Current.MapEditor_LoadCollisions_ER)
            return;

        if (Project.ProjectType == ProjectType.ER)
        {
            LoadModelCollision(modelName, "h", modelType);
            LoadModelCollision(modelName, "l", modelType);
        }
    }

    public void OnUnloadModel(string modelName)
    {
        if (!CFG.Current.MapEditor_LoadCollisions_ER)
            return;

        if (Project.ProjectType == ProjectType.ER)
        {
            //UnloadModelCollision(mapId, "h");
            //UnloadModelCollision(mapId, "l");
        }
    }

    private void LoadModelCollision(string modelName, string colType, FlverContainerType modelType)
    {
        var checkedName = $"{modelName}_{colType}".ToLower();

        if (HavokContainers.ContainsKey(checkedName))
        {
            return;
        }

        // Mark as invalid by default
        bool isValid = false;

        var bndPath = $"{Project.DataPath}\\asset\\aeg\\{modelName.Substring(0, 6)}\\{modelName}_{colType}.geomhkxbnd.dcx";

        // If game root version exists, mark as valid
        if (File.Exists(bndPath))
        {
            isValid = true;
        }

        // If project version exists, point path to that instead, and mark as valid
        var projectBndPath = $"{Project.ProjectPath}\\asset\\aeg\\{modelName.Substring(0, 6)}\\{modelName}_{colType}.geomhkxbnd.dcx";

        if (File.Exists(projectBndPath))
        {
            bndPath = projectBndPath;
            isValid = true; // Load project if they are custom hkxbhd/hkxbdt
        }

        // If not marked as valid, return early to avoid bad read
        if (!isValid)
        {
            return;
        }

        BND4Reader reader = new BND4Reader(bndPath);

        // Read collisions
        foreach (var file in reader.Files)
        {
            BinderFileHeader f = file;

            var name = Path.GetFileName(f.Name).ToLower();

            if (name.Contains(".hkx"))
            {
                var storedName = Path.GetFileNameWithoutExtension(f.Name).ToLower();

                Memory<byte> bytes = reader.ReadFile(f);
                var FileBytes = bytes.ToArray();

                try
                {
                    HavokBinarySerializer serializer = new HavokBinarySerializer();
                    using (MemoryStream memoryStream = new MemoryStream(FileBytes))
                    {
                        var fileHkx = (hkRootLevelContainer)serializer.Read(memoryStream);

                        if (!HavokContainers.ContainsKey(storedName))
                        {
                            HavokContainers.Add(storedName, fileHkx);
                        }
                    }
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"Failed to serialize havok file: {name} - {ex}");
                }
            }
        }
    }

    #region Utils
    public static (CollisionSubmesh, List<Vector3>, List<int>) ProcessColData(
    hknpCompressedMeshShapeData coldata,
    hknpBodyCinfo bodyinfo,
    CollisionSubmesh mesh)
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
