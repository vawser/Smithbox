using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Tools;

public static class FmgEntryGenerator
{
    public static string RootPath = $"{AppContext.BaseDirectory}\\Assets\\Workflow\\FMG Entry Generators\\";
    public static string ProjectPath = $"{Smithbox.ProjectRoot}\\.smithbox\\Workflow\\FMG Entry Generators\\";

    public static List<FmgEntryGeneratorBase> GenerateBases = new List<FmgEntryGeneratorBase>();

    public static bool RefreshGeneratorBaseList = true;

    public static FmgEntryGeneratorBase SelectedGenerateBase;

    public static void UpdateGeneratorBaseList()
    {
        if (RefreshGeneratorBaseList)
        {
            RefreshGeneratorBaseList = false;

            GenerateBases = new();

            if (Directory.Exists(ProjectPath))
            {
                foreach (var file in Directory.EnumerateFiles(ProjectPath, "*.json"))
                {
                    var jsonString = File.ReadAllText(file);
                    var newResource = JsonSerializer.Deserialize(jsonString, FmgEntryGeneratorBaseSerializationContext.Default.FmgEntryGeneratorBase);

                    GenerateBases.Add(newResource);
                }
            }
        }
    }

    public static void DisplayConfiguration()
    {
        var sectionWidth = ImGui.GetWindowWidth();
        var sectionHeight = ImGui.GetWindowHeight();
        var defaultButtonSize = new Vector2(sectionWidth, 32);

        ImguiUtils.WrappedText("Select a FMG entry generator configuration to apply.");
        ImguiUtils.WrappedText("");

        UpdateGeneratorBaseList();

        // TODO: create generator base

        ImGui.Columns(2);

        ImGui.BeginChild("##generatorBaseSelection");

        foreach (var entry in GenerateBases)
        {
            if ((ProjectType)entry.ProjectType == Smithbox.ProjectType)
            {
                if (ImGui.Selectable($"{entry.Name}##generatorBase_{entry.Name}", entry == SelectedGenerateBase))
                {
                    SelectedGenerateBase = entry;
                }
                ImguiUtils.ShowHoverTooltip($"{entry.Description}");
            }
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild("#generatorBaseActions");

        var width = ImGui.GetWindowWidth();
        var buttonWidth = width;

        if (SelectedGenerateBase != null)
        {
            ImguiUtils.WrappedText($"{SelectedGenerateBase.Description}");
            ImguiUtils.WrappedText("");

            if (ImGui.Button("Generate", new Vector2(buttonWidth, 32)))
            {
                GenerateEntries(SelectedGenerateBase);
            }
            ImguiUtils.ShowHoverTooltip("Generate new FMG entries based on this Entry Generator script.");
        }

        ImGui.EndChild();

        ImGui.Columns(1);
    }

    public static void SetupTemplates()
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

    public static void GenerateEntries(FmgEntryGeneratorBase source)
    {
        var entryIds = Smithbox.EditorHandler.TextEditor.SelectionHandler.EntryIds;
        var entries = Smithbox.EditorHandler.TextEditor._EntryLabelCacheFiltered;
        var fmgInfo = Smithbox.EditorHandler.TextEditor._activeFmgInfo;

        FMGEntryGroup baseEntryGroup = null;
        for (var i = 0; i < entries.Count; i++)
        {
            FMG.Entry r = entries[i];
            if (entryIds[0] == r.ID)
            {
                baseEntryGroup = Smithbox.BankHandler.FMGBank.GenerateEntryGroup(r.ID, fmgInfo);
            }
        }

        if(baseEntryGroup != null)
        {
            var action = new GenerateFMGEntryAction(fmgInfo, baseEntryGroup, source);
            Smithbox.EditorHandler.TextEditor.EditorActionManager.ExecuteAction(action);
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
[JsonSerializable(typeof(FmgEntryGeneratorReplaceRow))]
public partial class FmgEntryGeneratorBaseSerializationContext
    : JsonSerializerContext
{ }

public class FmgEntryGeneratorBase
{
    public int ProjectType { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int Count { get; set; }

    public List<FmgEntryGeneratorRow> FMG_Title { get; set; }
    public List<FmgEntryGeneratorRow> FMG_TextBody { get; set; }
    public List<FmgEntryGeneratorRow> FMG_Summary { get; set; }
    public List<FmgEntryGeneratorRow> FMG_Description { get; set; }
    public List<FmgEntryGeneratorRow> FMG_ExtraText { get; set; }
}

public class FmgEntryGeneratorRow
{
    public bool PossessiveAdjust { get; set; }
    public int Offset { get; set; }
    public string PrependText { get; set; }
    public string AppendText { get; set; }
    public List<FmgEntryGeneratorReplaceRow> ReplaceList { get; set; }
}

public class FmgEntryGeneratorReplaceRow
{
    public string SearchText { get; set; }
    public string ReplaceText { get; set; }
}