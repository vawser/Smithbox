using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextNamingTemplateManager
{
    private TextEditorScreen Editor;

    public string RootPath = "";
    public string ProjectPath = "";

    public Dictionary<string, FmgEntryGeneratorBase> GeneratorDictionary = new();

    public bool RefreshGeneratorBaseList = true;

    public FmgEntryGeneratorBase SelectedGenerateBase;

    public TextNamingTemplateManager(TextEditorScreen editor)
    {
        Editor = editor;
        RootPath = $"{AppContext.BaseDirectory}\\Assets\\Workflow\\Naming Templates\\";
        ProjectPath = $"{editor.Project.ProjectPath}\\.smithbox\\Workflow\\Naming Templates\\";
    }

    public FmgEntryGeneratorBase GetGenerator(string name)
    {
        if(GeneratorDictionary.ContainsKey(name))
        {
            return GeneratorDictionary[name];
        }
        else
        {
            TaskLogs.AddLog("Invalid name for Naming template.", LogLevel.Warning);
            return null;
        }
    }

    public void SetupTemplates()
    {
        if (Editor.Project.ProjectType is not ProjectType.Undefined)
        {
            if (!Directory.Exists(ProjectPath))
            {
                Directory.CreateDirectory(ProjectPath);
            }

            foreach (var file in Directory.EnumerateFiles(RootPath))
            {
                var filename = Path.GetFileName(file);

                var rootPath = $"{RootPath}{filename}";
                var projectPath = $"{ProjectPath}{filename}";

                if (File.Exists(rootPath) && !File.Exists(projectPath))
                {
                    File.Copy(rootPath, projectPath);
                }
            }
        }
    }

    public void OnProjectChanged()
    {
        ProjectPath = $"{Editor.Project.ProjectPath}\\.smithbox\\Workflow\\Naming Templates\\";

        GeneratorDictionary = new();

        if (Editor.Project.ProjectType is not ProjectType.Undefined)
        {
            SetupTemplates();
            CFG.Current.TextEditor_CreationModal_IncrementalNaming_Template = "";

            if (Directory.Exists(ProjectPath))
            {
                foreach (var file in Directory.EnumerateFiles(ProjectPath, "*.json"))
                {
                    var jsonString = File.ReadAllText(file);
                    var newResource = JsonSerializer.Deserialize(jsonString, FmgEntryGeneratorBaseSerializationContext.Default.FmgEntryGeneratorBase);

                    GeneratorDictionary.Add(newResource.Name, newResource);
                }
            }
        }
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)
]
[JsonSerializable(typeof(FmgEntryGeneratorBase))]
[JsonSerializable(typeof(FmgEntryGeneratorRow))]
public partial class FmgEntryGeneratorBaseSerializationContext
    : JsonSerializerContext
{ }

public class FmgEntryGeneratorBase
{
    public int ProjectType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Count { get; set; }

    public List<FmgEntryGeneratorRow> DefinitionList { get; set; }
}

public class FmgEntryGeneratorRow
{
    public bool PossessiveAdjust { get; set; }
    public int Offset { get; set; }
    public string PrependText { get; set; }
    public string AppendText { get; set; }
}

