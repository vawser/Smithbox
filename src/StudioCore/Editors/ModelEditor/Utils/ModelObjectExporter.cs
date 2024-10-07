using Org.BouncyCastle.Utilities;
using Pfim;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Editors.TextureViewer.Utils;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Utils;

/// <summary>
/// Credit to Pear for the OBJ exporter original code.
/// </summary>
public static class ModelObjectExporter
{
    public static string ExportPath = Smithbox.ProjectRoot;

    public static void ExportModel(ModelEditorScreen screen)
    {
        var flver = screen.ResManager.GetCurrentFLVER();
        var name = screen.ResManager.GetCurrentInternalFile().Name;
        var path = $"{ExportPath}\\{name}.obj";
        var mapId = Smithbox.EditorHandler.ModelEditor.Selection._selectedAssociatedMapID;
        var modelType = Smithbox.EditorHandler.ModelEditor.ResManager.LoadedFlverContainer.Type;

        var success = ObjExport(flver, path, name, modelType, mapId);

        if (success)
        {
            TaskLogs.AddLog($"Model exported successfully: {path}");
        }
        else
        {
            TaskLogs.AddLog($"Model was not exported.");
        }
    }

    public static bool ObjExport(IFlver iflver, string outPath, string modelName, FlverContainerType modelType, 
        string mapId = "")
    {
        // Assume FLVER2 for now
        FLVER2 flver = (FLVER2) iflver;

        OBJ obj = new();

        Matrix4x4[] boneMatrices = new Matrix4x4[flver.Nodes.Count];

        // Bones
        for (int i = 0; i < flver.Nodes.Count; i++)
        {
            FLVER.Node bone = flver.Nodes[i];
            Matrix4x4 global = Matrix4x4.Identity;

            if (bone.ParentIndex != -1)
            {
                global = boneMatrices[bone.ParentIndex];
            }
            boneMatrices[i] = bone.ComputeLocalTransform() * global;
        }

        int meshCount = 0;
        int currentFaceIndex = 0;

        string outputObjFolderPath = Path.GetDirectoryName(ExportPath) ?? "";
        string outputTexFolderPath = $"{outputObjFolderPath}\\textures";

        // Meshes
        foreach (FLVER2.Mesh flverMesh in flver.Meshes)
        {
            OBJ.Mesh mesh = new()
            {
                Indices = flverMesh.FaceSets
                .Find(x => x.Flags == FLVER2.FaceSet.FSFlags.None)?
                .Triangulate(false) ?? new List<int>()
            };

            // Skip if empty
            if (flverMesh.Vertices.Count == 0 ||
                flverMesh.FaceSets.Count == 0 ||
                mesh.Indices.Count == 0 ||
                mesh.Indices.All(x => x == mesh.Indices[0]))
            {
                continue;
            }

            mesh.Name = meshCount.ToString();
            meshCount++;
            mesh.MaterialName = mesh.Name;

            FLVER2.Material material = flver.Materials[flverMesh.MaterialIndex];
            FLVER2.Texture? diffuse = material.Textures.Find(i => (Path.GetFileName(i.Path)).Contains("_a"));

            if (diffuse != null)
            {
                string diffuseTexName = $"{Path.GetFileNameWithoutExtension(diffuse.Path)}.png";

                // Games that use MATBIN
                if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
                {
                    diffuse = material.Textures.Find(i => i.Type.Contains("AlbedoMap"));

                    MATBIN.Sampler diffuseTexSampler = GetMatchingSampler(material, "allmaterial");

                    // Look in the dlc ones for ER if no match is found
                    if(Smithbox.ProjectType is ProjectType.ER)
                    {
                        if (diffuseTexSampler == null)
                        {
                            diffuseTexSampler = GetMatchingSampler(material, "allmaterial_dlc01");
                        }

                        if (diffuseTexSampler == null)
                        {
                            diffuseTexSampler = GetMatchingSampler(material, "allmaterial_dlc02");
                        }
                    }

                    // Skip if no match if found
                    if (diffuseTexSampler == null)
                        continue;

                    var samplerPath = Path.GetFileNameWithoutExtension(diffuseTexSampler.Path);

                    // TODO: Support BND and BXF containers, not just direct TPF
                    var tpfPath = samplerPath.Replace("_a", "").ToLower();
                    string tpfFileName = $"{tpfPath}.tpf.dcx";

                    // Get search dir
                    var filePathTPF = GetTPFFilePath(tpfFileName, modelType, mapId);

                    // Skip if invalid path
                    if (string.IsNullOrEmpty(filePathTPF) || !TPF.IsRead(filePathTPF, out TPF tpf)) 
                        continue;

                    // Get diffuse texture from TPF
                    TPF.Texture? diffuseTexture = tpf.Textures.Find(i => i.Name.Contains("_a"));

                    // Skip if no diffuse texture exists in TPF
                    if (diffuseTexture == null) 
                        continue;

                    diffuseTexName = $"{diffuseTexture.Name}.png";

                    ExportImageFromTexture(diffuseTexture, outputTexFolderPath);
                }
                // Games that use MATERIAL
                else
                {

                }
                
                obj.AddNewMaterial(mesh.MaterialName, diffuseTexName);
            }

            for (int q = 0; q < mesh.Indices.Count; q++)
            {
                mesh.Indices[q] += currentFaceIndex + 1;
            }

            currentFaceIndex += flverMesh.Vertices.Count;

            foreach (FLVER.Vertex vert in flverMesh.Vertices)
            {
                mesh.Vertices.Add(vert.Position);
            }

            obj.Meshes.Add(mesh);
        }

        if (!Directory.Exists(outputObjFolderPath))
        {
            Directory.CreateDirectory(outputObjFolderPath);
        }

        obj.Write(outPath, Matrix4x4.Identity);

        return true;
    }

    public static MATBIN.Sampler GetMatchingSampler(FLVER2.Material material, string file)
    {
        MATBIN.Sampler diffuseTexSampler = null;

        string matbinBndFilePath = $"{Smithbox.GameRoot}\\material\\{file}.matbinbnd.dcx";

        BND4 matbinBnd = BND4.Read(matbinBndFilePath);
        BinderFile? matbinFile = matbinBnd.Files.FirstOrDefault(i => i.Name.Contains(material.Name));

        if (matbinFile?.Bytes == null || !MATBIN.IsRead(matbinFile.Bytes.ToArray(), out MATBIN matbin))
        {
            return diffuseTexSampler;
        }

        return matbin.Samplers.Find(i => i.Path.Contains("_a"));
    }

    public static string GetTPFFilePath(string tpfFileName, FlverContainerType modelType, string mapId)
    {
        var path = "";

        if(modelType is FlverContainerType.Character)
        {
            // TODO
        }

        if (modelType is FlverContainerType.Object)
        {
            path = $"{Smithbox.GameRoot}/asset/aet/";
        }

        if (modelType is FlverContainerType.Parts)
        {
            // TODO
        }

        if (modelType is FlverContainerType.Character)
        {
            // TODO
        }

        string[] tpfFilePaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

        return tpfFilePaths.ToList().Find(i => i.Contains(tpfFileName)) ?? "";
    }

    private static void ExportImageFromTexture(TPF.Texture texture, string outputDir)
    {
        string exportPath = $"{outputDir}\\{texture.Name}.png";

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        TexUtils.ExportPNGImage(exportPath, texture.Bytes.ToArray());
    }
}

public class OBJ
{
    internal OBJ()
    {
        MTL = "";
        Name = "";
        Meshes = new List<Mesh>();
    }

    public string Name { get; set; }
    public List<Mesh> Meshes { get; set; }
    public string MTL { get; set; }

    public void AddNewMaterial(string name, string diffuseTexName)
    {
        string newMaterialEntry = $"newmtl {name}\r\n"
            + "Ka 1.000000 1.000000 1.000000\r\n"
            + "Kd 0.800000 0.800000 0.800000\r\n"
            + "Ks 0.500000 0.500000 0.500000\r\n"
            + "Ns 200.000000\r\n"
            + "Ni 1.000000\r\n"
            + "d 1.000000\r\n"
            + "illum 2\r\n"
            + $"map_Kd textures\\\\{diffuseTexName}\r\n\r\n";
        MTL += newMaterialEntry;
    }

    public void Write(string path, Matrix4x4 transform)
    {
        string mtlFileName = $"{Path.GetFileNameWithoutExtension(path)}.mtl";

        StringBuilder objSb = new();

        objSb.AppendLine($"mtllib {mtlFileName}");

        foreach (Mesh mesh in Meshes)
        {
            var verts = mesh.Vertices.Select(
                vert => Vector3.Transform(vert, transform) * new Vector3(-1, 1, 1));

            foreach (Vector3 v in verts)
            {
                objSb.AppendLine($"v  {v.X} {v.Y} {v.Z}");
            }
            objSb.AppendLine($"g {mesh.Name}");
            objSb.AppendLine($"usemtl {mesh.MaterialName}");

            for (int i = 0; i < mesh.Indices.Count - 2; i += 3)
            {
                objSb.AppendLine($"f {mesh.Indices[i]} {mesh.Indices[i + 1]} {mesh.Indices[i + 2]}");
            }
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? "");
        File.WriteAllText(path, objSb.ToString());

        StringBuilder mtlSb = new();
        mtlSb.AppendLine(MTL);
        File.WriteAllText($"{Path.GetDirectoryName(path)}\\{mtlFileName}", MTL);
    }

    public class Mesh
    {
        internal Mesh()
        {
            Name = "";
            MaterialName = "";
            Indices = new List<int>();
            Vertices = new List<Vector3>();
        }

        public string Name { get; set; }
        public string MaterialName { get; set; }
        public List<int> Indices { get; set; }
        public List<Vector3> Vertices { get; set; }
    }
}