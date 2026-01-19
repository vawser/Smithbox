using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Application;

public class ProjectAliasMenu
{
    public bool IsDisplayed = false;
    public ProjectOrchestrator Orchestrator;

    private AliasStore BaseAliases;

    private static ProjectAliasType CurrentAliasEditor = ProjectAliasType.None;
    private static string AliasEntryFilter = "";
    private static AliasEntry CurrentAliasEntry;

    private bool InitialLayout = false;

    public ProjectAliasMenu(ProjectOrchestrator orchestrator)
    {
        Orchestrator = orchestrator;
    }

    public void Draw()
    {
        if (!IsDisplayed)
            return;

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (!ImGui.Begin("Project Aliases", ref IsDisplayed, UIHelper.GetEditorPopupWindowFlags()))
        {
            ImGui.End();
            return;
        }

        ImGui.BeginMenuBar();

        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("Save Aliases"))
            {
                IsDisplayed = false;
                Save();
            }
            UIHelper.Tooltip("Save alias changes to the project");

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Data"))
        {
            ImGui.InputText("##exportDelimiter", ref CFG.Current.Alias_Export_Delimiter, 255);
            UIHelper.Tooltip("Set the delimiter to use when exporting the alias lists via 'Copy Entries as Text'");

            ImGui.Checkbox("Ignore Empty on Export", ref CFG.Current.Alias_Editor_Export_Ignore_Empty);
            UIHelper.Tooltip("If enabled, empty entries will be ignored by the 'Copy Entries as Text' action.");

            ImGui.EndMenu();
        }

        ImGui.EndMenuBar();

        ImGui.Separator();

        DrawMainLayout();

        ImGui.End();
    }

    #region Layout

    private void DrawMainLayout()
    {
        ImGui.Columns(3, "aliasLayout", true);

        DrawAliasTypeSidebar();
        ImGui.NextColumn();

        DrawAliasEntryList();
        ImGui.NextColumn();

        DrawAliasEditor();

        ImGui.Columns(1);
    }

    #endregion

    #region Alias Type Sidebar

    private void DrawAliasTypeSidebar()
    {
        ImGui.BeginChild("AliasTypeSidebar", new Vector2(0, 0));

        ImGui.Text("Alias Types");
        ImGui.Separator();

        foreach (var entry in Enum.GetValues<ProjectAliasType>())
        {
            if (entry == ProjectAliasType.None)
                continue;

            bool selected = entry == CurrentAliasEditor;

            if (selected)
                ImGui.PushStyleColor(ImGuiCol.Header, ImGui.GetStyle().Colors[(int)ImGuiCol.HeaderActive]);

            if (ImGui.Selectable($"{Icons.List} {entry.GetDisplayName()}", selected))
            {
                CurrentAliasEditor = entry;
                CurrentAliasEntry = null;
            }

            if (CurrentAliasEditor == entry)
            {
                if (ImGui.BeginPopupContextItem($"AliasTypeContextMenu"))
                {
                    if (ImGui.Selectable($"Copy Entries as Text"))
                    {
                        var entries = new List<string>();

                        var source = GetAliasList();

                        foreach (var aliasEntry in source)
                        {
                            if (CFG.Current.Alias_Editor_Export_Ignore_Empty)
                            {
                                if (aliasEntry.Name == "")
                                    continue;
                            }

                            var line = $"{aliasEntry.ID}{CFG.Current.Alias_Export_Delimiter}{aliasEntry.Name}";
                            entries.Add(line);
                        }

                        var output = string.Join("\n", entries.ToArray());
                        PlatformUtils.Instance.SetClipboardText(output);
                    }

                    if (CFG.Current.Developer_Enable_Tools)
                    {
                        if (ImGui.Selectable($"Copy Entries as Table"))
                        {
                            var entries = new List<string>();

                            var source = GetAliasList();

                            foreach (var aliasEntry in source)
                            {
                                var line = $"| {aliasEntry.ID} | {aliasEntry.Name} |";
                                entries.Add(line);
                            }

                            var output = string.Join("\n", entries.ToArray());
                            PlatformUtils.Instance.SetClipboardText(output);
                        }
                    }

                    ImGui.EndPopup();
                }
            }

            if (selected)
                ImGui.PopStyleColor();
        }

        ImGui.EndChild();
    }

    #endregion

    #region Alias Entry List

    private void DrawAliasEntryList()
    {
        ImGui.BeginChild("AliasEntryPanel", new Vector2(0, 0));

        if (CurrentAliasEditor == ProjectAliasType.None)
        {
            ImGui.TextDisabled("Select an alias type.");
            ImGui.EndChild();
            return;
        }

        var source = GetAliasList();

        ImGui.Text($"Entries ({source.Count})");
        ImGui.Separator();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint(
            "##aliasFilter",
            "Filter by ID, name, or tag...",
            ref AliasEntryFilter,
            128
        );

        ImGui.Separator();

        ImGui.BeginChild("AliasEntryListInner");

        if (source.Count == 0)
        {
            ImGui.TextDisabled("No aliases defined.");
            ImGui.Spacing();

            if (ImGui.Button($"{Icons.Plus} Add Alias"))
            {
                source.Add(new AliasEntry
                {
                    ID = "NEW_ID",
                    Name = "New Alias",
                    Tags = new List<string>()
                });
            }

            ImGui.EndChild();
            ImGui.EndChild();
            return;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(source.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entry = source[i];
                if (!PassesFilter(entry))
                    continue;

                bool selected = entry == CurrentAliasEntry;
                if (ImGui.Selectable($"{entry.ID}  —  {entry.Name}", selected))
                {
                    CurrentAliasEntry = entry;
                }

                if (ImGui.BeginPopupContextItem($"entry_ctx_{i}"))
                {
                    if (ImGui.Selectable("Duplicate"))
                    {
                        Orchestrator.ActionManager.ExecuteAction(
                            new ChangeAliasList(
                                source,
                                entry,
                                new AliasEntry
                                {
                                    ID = entry.ID,
                                    Name = $"{entry.Name}_1",
                                    Tags = new List<string>(entry.Tags)
                                },
                                ProjectAliasListOperation.Add,
                                i + 1));
                    }

                    if (ImGui.Selectable("Remove"))
                    {
                        Orchestrator.ActionManager.ExecuteAction(
                            new ChangeAliasList(
                                source,
                                entry,
                                null,
                                ProjectAliasListOperation.Remove,
                                i));
                    }

                    ImGui.EndPopup();
                }
            }
        }

        ImGui.EndChild();
        ImGui.EndChild();
    }

    private bool PassesFilter(AliasEntry entry)
    {
        if (string.IsNullOrWhiteSpace(AliasEntryFilter))
            return true;

        var f = AliasEntryFilter.ToLowerInvariant();

        return entry.ID.ToLowerInvariant().Contains(f)
            || entry.Name.ToLowerInvariant().Contains(f)
            || entry.Tags.Any(t => t.ToLowerInvariant().Contains(f));
    }

    #endregion

    #region Alias Editor

    private void DrawAliasEditor()
    {
        ImGui.BeginChild("AliasEditorPanel", new Vector2(0, 0));

        if (CurrentAliasEntry == null)
        {
            ImGui.TextDisabled("Select an alias entry to edit.");
            ImGui.EndChild();
            return;
        }

        ImGui.Text("Alias Details");
        ImGui.Separator();

        ImGui.Columns(2, "aliasEditorCols", false);

        DrawTextField("ID", CurrentAliasEntry.ID, ProjectAliasFieldType.ID);
        DrawTextField("Name", CurrentAliasEntry.Name, ProjectAliasFieldType.Name);

        ImGui.Columns(1);
        ImGui.Separator();

        DrawTagsEditor();

        ImGui.EndChild();
    }

    private void DrawTextField(string label, string value, ProjectAliasFieldType field)
    {
        string original = value;

        ImGui.Text(label);
        ImGui.NextColumn();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText($"##{label}", ref value, 256);

        if (ImGui.IsItemDeactivatedAfterEdit() && original != value)
        {
            Orchestrator.ActionManager.ExecuteAction(
                new ChangeAliasField(CurrentAliasEntry, original, value, field));
        }

        ImGui.NextColumn();
    }

    private void DrawTagsEditor()
    {
        ImGui.Text("Tags");
        ImGui.Separator();

        for (int i = 0; i < CurrentAliasEntry.Tags.Count; i++)
        {
            ImGui.PushID(i);

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.25f, 0.25f, 0.25f, 1));
            ImGui.Button(CurrentAliasEntry.Tags[i]);
            ImGui.PopStyleColor();

            if (ImGui.BeginPopupContextItem())
            {
                var curTagContents = CurrentAliasEntry.Tags[i];

                ImGui.InputText("##tagContents", ref curTagContents, 255);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    Orchestrator.ActionManager.ExecuteAction(
                        new ChangeAliasTag(
                            CurrentAliasEntry.Tags,
                            i,
                            curTagContents));
                }

                if (ImGui.Selectable("Remove"))
                {
                    Orchestrator.ActionManager.ExecuteAction(
                        new ChangeAliasTagList(
                            CurrentAliasEntry.Tags,
                            CurrentAliasEntry.Tags[i],
                            CurrentAliasEntry.Tags[i],
                            ProjectAliasTagListOperation.Remove,
                            i));
                }
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            ImGui.PopID();
        }

        ImGui.Spacing();
        if (ImGui.Button($"{Icons.Plus} Add Tag"))
        {
            Orchestrator.ActionManager.ExecuteAction(
                new ChangeAliasTagList(
                    CurrentAliasEntry.Tags,
                    "",
                    "",
                    ProjectAliasTagListOperation.Add,
                    CurrentAliasEntry.Tags.Count));
        }
    }

    #endregion

    private List<AliasEntry> GetAliasList()
    {
        return Orchestrator.SelectedProject.Handler.ProjectData.Aliases
            .TryGetValue(CurrentAliasEditor, out var list)
            ? list
            : new List<AliasEntry>();
    }

    public void Setup(ProjectEntry project)
    {
        if (BaseAliases != null)
            return;

        BaseAliases = new();

        var dir = Path.Join(AppContext.BaseDirectory, "Assets", "Aliases",
            ProjectUtils.GetGameDirectory(project.Descriptor.ProjectType));

        List<string> sourceFiles = Directory.GetFiles(dir, "*.json").ToList();

        foreach (string sourceFile in sourceFiles)
        {
            try
            {
                if (!Enum.TryParse(Path.GetFileNameWithoutExtension(sourceFile), out ProjectAliasType type)) continue;
                string text = File.ReadAllText(sourceFile);
                try
                {
                    var entries = JsonSerializer.Deserialize(text, ProjectJsonSerializerContext.Default.ListAliasEntry);

                    if (!BaseAliases.ContainsKey(type))
                    {
                        BaseAliases.TryAdd(type, entries);
                        continue;
                    }
                    BaseAliases[type] = entries.UnionBy(BaseAliases[type], e => e.ID).ToList();
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"Failed to deserialize the aliases: {sourceFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed to read the aliases: {sourceFile}", LogLevel.Error, LogPriority.High, e);
            }
        }
    }


    public void Save()
    {
        var projectFolder = Path.Join(Orchestrator.SelectedProject.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        foreach ((ProjectAliasType aliasType, List<AliasEntry> aliases) in Orchestrator.SelectedProject.Handler.ProjectData.Aliases)
        {
            string path = Path.Combine(projectFolder, $"{aliasType}.json");

            List<AliasEntry> baseAliases =
                (BaseAliases != null && BaseAliases.TryGetValue(aliasType, out List<AliasEntry> bAliases)) ? bAliases : new();

            List<AliasEntry> diffAliases = aliases.Where(a =>
            {
                var baseA = baseAliases.FirstOrDefault(b => b.ID == a.ID);

                if (baseA == null)
                    return true;

                return baseA.Name != a.Name || !baseA.Tags.SequenceEqual(a.Tags);
            }).ToList();

            if (!diffAliases.Any()) continue;

            var json = JsonSerializer.Serialize(diffAliases, ProjectJsonSerializerContext.Default.ListAliasEntry);

            File.WriteAllText(path, json);
        }
    }

    public void SaveIndividual(ProjectAliasType targetType)
    {
        var projectFolder = Path.Join(Orchestrator.SelectedProject.Descriptor.ProjectPath, ".smithbox", "Assets", "Aliases");

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        foreach ((ProjectAliasType aliasType, List<AliasEntry> aliases) in Orchestrator.SelectedProject.Handler.ProjectData.Aliases)
        {
            if (targetType != aliasType)
                continue;

            string path = Path.Combine(projectFolder, $"{aliasType}.json");

            List<AliasEntry> baseAliases =
                (BaseAliases != null && BaseAliases.TryGetValue(aliasType, out List<AliasEntry> bAliases)) ? bAliases : new();

            List<AliasEntry> diffAliases = aliases.Where(a =>
            {
                var baseA = baseAliases.FirstOrDefault(b => b.ID == a.ID);

                if (baseA == null)
                    return true;

                return baseA.Name != a.Name || !baseA.Tags.SequenceEqual(a.Tags);
            }).ToList();

            if (!diffAliases.Any()) continue;

            var json = JsonSerializer.Serialize(diffAliases, ProjectJsonSerializerContext.Default.ListAliasEntry);

            File.WriteAllText(path, json);
        }
    }
}
