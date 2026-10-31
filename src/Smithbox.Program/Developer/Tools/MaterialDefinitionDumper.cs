using SoulsFormats;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace StudioCore.Application;

/// <summary>
/// Writes out how the current project's materials resolve, for working on the renderer or the shader tables.
/// </summary>
public class MaterialDefinitionDumper
{
    private static readonly string[] ShaderFolders = ["/shader", "/shader_d3d12"];

    public string _outputPath = "";
    public string _filter = "";

    public void Display()
    {
        GUI.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Output_Folder"),
            LOC.Get("DEV_Tool_Header_Output_Folder_TT"));

        GUI.SinglelineTextInput("MaterialDumpOutput", ref _outputPath);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Material_Filter"),
            LOC.Get("DEV_Tool_Header_Material_Filter_TT"));

        GUI.SinglelineTextInput("MaterialDumpFilter", ref _filter);

        GUI.Spacer();
        GUI.MultiButtonInput("materialDumpActions",
            "selectOutput",
            LOC.Get("DEV_Tool_Action_Select_Directory"),
            LOC.Get("DEV_Tool_Action_Select_Directory_TT"),
            SelectOutput,

            "dumpMaterials",
            LOC.Get("DEV_Tool_Action_Dump_Material_Definitions"),
            LOC.Get("DEV_Tool_Action_Dump_Material_Definitions_TT"),
            Dump,

            "generateTextureGroups",
            LOC.Get("DEV_Tool_Action_Generate_Texture_Groups"),
            LOC.Get("DEV_Tool_Action_Generate_Texture_Groups_TT"),
            GenerateTextureGroups);
    }

    /// <summary>
    /// Writes the table of which UV group each texture of each shader is in. The result
    /// belongs in Assets/Material Definitions, and only changes if a game's shaders do.
    /// </summary>
    public void GenerateTextureGroups()
    {
        var project = Smithbox.Orchestrator.SelectedProject;

        if (project == null)
        {
            Smithbox.Log(this, LOC.Get("DEV_Tool_Material_Dump_No_Project"));
            return;
        }

        if (string.IsNullOrWhiteSpace(_outputPath) || !Directory.Exists(_outputPath))
        {
            Smithbox.Log(this, LOC.Get("DEV_Tool_Material_Dump_No_Output"));
            return;
        }

        var game = project.Descriptor.ProjectType;
        var path = Path.Join(_outputPath, $"{game}_TextureGroups.json");

        try
        {
            Dictionary<string, Dictionary<int, string>> groups = new();

            // AC6 only has them in the D3D12 folder. Ray tracing variants have the same shaders
            var bundlePaths = ShaderFolders
                .SelectMany(x => project.VFS.FS.GetFileNamesMatching(x, @"\.shaderbdlebnd\.dcx$"))
                .Where(x => !x.Contains("[rt]"))
                .Select(x => x.Replace('\\', '/'));

            foreach (var bundlePath in bundlePaths)
            {
                foreach (BinderFile bundle in BND4.Read(project.VFS.FS.ReadFile(bundlePath).Value).Files)
                {
                    if (!bundle.Name.EndsWith(".shaderbdle"))
                        continue;

                    foreach (BinderFile file in BND4.Read(bundle.Bytes).Files)
                    {
                        if (!file.Name.EndsWith(".metaparam"))
                            continue;

                        var shader = Path.GetFileNameWithoutExtension(file.Name.Split('\\', '/')[^1]);
                        var textureGroups = MaterialMetaparam.ReadTextureGroups(file.Bytes.ToArray());

                        if (textureGroups.Count > 0)
                        {
                            groups[shader] = textureGroups;
                        }
                    }
                }
            }

            File.WriteAllText(path, JsonSerializer.Serialize(groups,
                MaterialEditorJsonSerializerContext.Default.DictionaryStringDictionaryInt32String));

            Smithbox.Log(this, LOC.Get("DEV_Tool_Material_Dump_Groups_Done", groups.Count, path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("DEV_Tool_Material_Dump_Failed", path), e);
        }
    }

    public void SelectOutput()
    {
        if (PlatformUtils.Instance.OpenFolderDialog(LOC.Get("DEV_Tool_Dialog_Select_Folder"), out var path))
        {
            _outputPath = path;
        }
    }

    public void Dump()
    {
        var bank = Smithbox.Orchestrator.SelectedProject?.Handler?.MaterialData?.PrimaryBank;

        if (bank == null)
        {
            Smithbox.Log(this, LOC.Get("DEV_Tool_Material_Dump_No_Project"));
            return;
        }

        if (string.IsNullOrWhiteSpace(_outputPath) || !Directory.Exists(_outputPath))
        {
            Smithbox.Log(this, LOC.Get("DEV_Tool_Material_Dump_No_Output"));
            return;
        }

        // Matches on the material or shader name, so that a whole shader can be dumped
        List<MaterialDefinition> definitions = bank.ResolveAllDefinitions()
            .Where(x => _filter.Length == 0
                || x.Name.Contains(_filter, StringComparison.OrdinalIgnoreCase)
                || x.ShaderName.Contains(_filter, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var game = bank.Project.Descriptor.ProjectType;
        var materialPath = Path.Join(_outputPath, $"{game}_MaterialDefinitions.json");
        var shaderPath = Path.Join(_outputPath, $"{game}_MaterialShaders.tsv");

        try
        {
            File.WriteAllText(materialPath,
                JsonSerializer.Serialize(definitions, MaterialEditorJsonSerializerContext.Default.ListMaterialDefinition));

            File.WriteAllText(shaderPath, BuildShaderSummary(definitions));

            Smithbox.Log(this, LOC.Get("DEV_Tool_Material_Dump_Done", definitions.Count, _outputPath));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("DEV_Tool_Material_Dump_Failed", _outputPath), e);
        }
    }

    /// <summary>
    /// One row for each shader, most used first, which shows what the shader table is still missing.
    /// </summary>
    private static string BuildShaderSummary(List<MaterialDefinition> definitions)
    {
        StringBuilder summary = new("Materials\tShader\tKnown\tDeformer\tBlendLayers\tBlendWeight\n");

        foreach (var shader in definitions.GroupBy(x => x.ShaderName).OrderByDescending(x => x.Count()))
        {
            MaterialDefinition first = shader.First();
            summary.Append($"{shader.Count()}\t{shader.Key}\t{first.IsKnownShader}\t{first.Deformer}\t");
            summary.Append($"{first.BlendLayerCount}\t{first.BlendWeight}\n");
        }

        return summary.ToString();
    }
}
