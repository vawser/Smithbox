using Assimp;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Utils;

// Credit to original author of implementation in FLVER Editor
public static class ModelColladaExporter
{
    public static string ExportPath = "";

    public static void ExportModel(ModelEditorScreen editor)
    {
        ExportPath = editor.Project.ProjectPath;

        var path = "";
        var success = false;

        path = $"{ExportPath}\\{editor.ResManager.GetCurrentInternalFile().Name}.dae";
        success = AssimpExport(editor.ResManager.GetCurrentFLVER(), path, "collada");

        if (success)
        {
            TaskLogs.AddLog($"Model Exporter: exported DAE file: {path}");
        }
        else
        {
            TaskLogs.AddLog($"Model Exporter: model was not exported.", LogLevel.Error);
        }
    }

    public static bool AssimpExport(IFlver flver, string outPath, string exportType)
    {
        // Create scene
        Assimp.Scene scene = new()
        {
            RootNode = new Node()
        };

        // Add materials
        foreach (IFlverMaterial material in flver.Materials)
            scene.Materials.Add(new Material { Name = material.Name });

        // Add meshes
        for (int i = 0; i < flver.Meshes.Count; ++i)
        {
            var mesh = flver.Meshes[i];
            Mesh newMesh = new("Mesh_M" + i, PrimitiveType.Triangle);

            // Add vertices
            foreach (FLVER.Vertex vertex in mesh.Vertices)
            {
                newMesh.Vertices.Add(new Vector3D(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
                newMesh.Normals.Add(new Vector3D(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z));

                if (newMesh.Tangents.Count > 0)
                    newMesh.Tangents.Add(new Vector3D(vertex.Tangents[0].X, vertex.Tangents[0].Y, vertex.Tangents[0].Z));

                // Add UVs
                for (int j = 0; j < 4; ++j)
                {
                    if (vertex.UVs.Count > j)
                    {
                        newMesh.TextureCoordinateChannels[j].Add(new Vector3D(vertex.UVs[j].X, vertex.UVs[j].Y, 0f));
                    }
                    else
                    {
                        newMesh.TextureCoordinateChannels[j].Add(new Vector3D(1, 1, 1));
                    }
                }

                // Set UV component count
                for (int j = 0; j < newMesh.TextureCoordinateChannelCount; j++)
                {
                    newMesh.UVComponentCount[j] = 2;
                }

                // Add vertex colors
                for (int j = 0; j < vertex.Colors.Count; j++)
                {
                    var color = vertex.Colors[j];
                    newMesh.VertexColorChannels[j].Add(new Color4D(color.R, color.G, color.B, color.A));
                }
            }

            // Add facesets
            if (mesh is FLVER2.Mesh flver2mesh)
            {
                foreach (FLVER2.FaceSet faceSet in flver2mesh.FaceSets)
                {
                    for (int j = 0; j < faceSet.Indices.Count; j += 3)
                    {
                        if (j > faceSet.Indices.Count - 2) continue;
                        newMesh.Faces.Add(new Face(new int[] { faceSet.Indices[j], faceSet.Indices[j + 1], faceSet.Indices[j + 2] }));
                    }
                }
            }

            // Finish up adding mesh
            newMesh.MaterialIndex = mesh.MaterialIndex;
            scene.Meshes.Add(newMesh);
            Node node = new() { Name = "M_" + i + "_" + flver.Materials[mesh.MaterialIndex].Name };
            node.MeshIndices.Add(i);
            scene.RootNode.Children.Add(node);
        }

        AssimpContext exporter = new();
        return exporter.ExportFile(scene, outPath, exportType);
    }
}
