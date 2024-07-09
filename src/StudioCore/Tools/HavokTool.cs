using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using HKX2;
using Silk.NET.OpenGL;
using SoulsFormats;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid.Utilities;
using static StudioCore.Resource.HavokCollisionResource;

namespace StudioCore.Tools;

public static class HavokTool
{
    public static HKLib.hk2018.hkRootLevelContainer ER_HKX;

    public static void TestLoad()
    {
        var mapId = "m10_01_00_00";
        var type = "h";
        var testContainer = $"h{mapId.Substring(1)}";
        var testbhd = $"{testContainer}.hkxbhd";
        var testbdt = $"{testContainer}.hkxbdt";
        var testCompendium = $"{testContainer}.compendium.dcx";
        var testFile = $"{type}{mapId.Substring(1)}_001000.hkx.dcx";

        var bdtPath = $"{Smithbox.GameRoot}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{testbdt}";
        var bhdPath = $"{Smithbox.GameRoot}\\map\\{mapId.Substring(0, 3)}\\{mapId}\\{testbhd}";

        Memory<byte> testFileBytes = null;
        Memory<byte> compendiumBytes = null;

        BXF4Reader reader = new BXF4Reader(bhdPath, bdtPath);
        foreach (var file in reader.Files)
        {
            BinderFileHeader f = file;

            if (file.Name.Contains(testCompendium))
            {
                Memory<byte> bytes = reader.ReadFile(f);
                compendiumBytes = DCX.Decompress(bytes);

                TaskLogs.AddLog($"file: {file.Name}");
                TaskLogs.AddLog($"compendiumBytes: {compendiumBytes.Length}");
            }

            if (file.Name.Contains(testFile))
            {
                Memory<byte> bytes = reader.ReadFile(f);
                testFileBytes = DCX.Decompress(bytes);

                TaskLogs.AddLog($"file: {file.Name}");
                TaskLogs.AddLog($"testFileBytes: {testFileBytes.Length}");
            }
        }

        HavokBinarySerializer serializer = new HavokBinarySerializer();
        using (MemoryStream memoryStream = new MemoryStream(compendiumBytes.ToArray()))
        {
            serializer.LoadCompendium(memoryStream);
        }

        using (MemoryStream memoryStream = new MemoryStream(testFileBytes.ToArray()))
        {
            ER_HKX = (HKLib.hk2018.hkRootLevelContainer)serializer.Read(memoryStream);
        }
    }

    public static bool TestResource()
    {
        BoundingBox Bounds = new BoundingBox();
        var submeshes = new List<CollisionSubmesh>();
        var first = true;

        if (ER_HKX.m_namedVariants.Count == 0)
        {
            // Yes this happens for some cols wtf From???
            return false;
        }

        var physicsscene = (HKLib.hk2018.hknpPhysicsSceneData)ER_HKX.m_namedVariants[0].m_variant;

        foreach (HKLib.hk2018.hknpBodyCinfo bodyInfo in physicsscene.m_systemDatas[0].m_bodyCinfos)
        {
            if (bodyInfo.m_shape is not HKLib.hk2018.fsnpCustomParamCompressedMeshShape ncol)
            {
                continue;
            }

            try
            {
                var mesh = new CollisionSubmesh();
                var indices = new List<int>();
                var vertices = new List<Vector3>();

                if (bodyInfo.m_shape is HKLib.hk2018.fsnpCustomParamCompressedMeshShape shape2)
                {
                    (mesh, vertices, indices) = HKXProcessor.ProcessColData(
                        (HKLib.hk2018.hknpCompressedMeshShapeData)shape2.m_data, bodyInfo, mesh);
                    //RenderHKX2018(mesh, vertices, indices);
                    TaskLogs.AddLog($"vertices: {vertices.Count}");
                }
                else if (bodyInfo.m_shape is HKLib.hk2018.hknpCompressedMeshShape shape1)
                {
                    (mesh, vertices, indices) = HKXProcessor.ProcessColData(
                        (HKLib.hk2018.hknpCompressedMeshShapeData)shape1.m_data, bodyInfo, mesh);
                    //RenderHKX2018(mesh, vertices, indices);
                    TaskLogs.AddLog($"indices: {indices.Count}");
                }

                if (first)
                {
                    Bounds = mesh.Bounds;
                    first = false;
                }
                else
                {
                    Bounds = BoundingBox.Combine(Bounds, mesh.Bounds);
                }

                submeshes.Add(mesh);
            }
            catch (Exception e)
            {
                // Debug failing cases later
            }
        }

        return true;
    }

    private static void PrintInfo(List<Vector3> vertices, List<int> indices)
    {
        var vertLine = "";
        foreach (var entry in vertices)
        {
            vertLine = $"{vertLine} {entry}";
        }
        TaskLogs.AddLog(vertLine);

        var indiceline = "";
        foreach (var entry in indices)
        {
            indiceline = $"{indiceline} {entry}";
        }
        TaskLogs.AddLog(indiceline);
    }
}
