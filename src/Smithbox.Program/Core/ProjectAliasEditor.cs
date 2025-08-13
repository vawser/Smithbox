using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509;
using StudioCore.Configuration;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using static SoulsFormats.MQB;

namespace StudioCore.Core;

public static class ProjectAliasEditor
{
    private static Smithbox BaseEditor;
    private static ProjectEntry TargetProject;
    private static AliasStore? BaseAliases = null;

    private static bool Display = false;

    public static void Show(Smithbox baseEditor, ProjectEntry curProject)
    {
        BaseEditor = baseEditor;
        TargetProject = curProject;

        if (BaseEditor == null)
            return;

        if (TargetProject == null)
            return;

        if (TargetProject.Aliases == null)
            return;

        if (BaseAliases == null)
        {
            SetupBaseAliases();
        }

        Display = true;
    }

    public static void SetupBaseAliases()
    {
        BaseAliases = new();

        var dir = Path.Join(AppContext.BaseDirectory, "Assets", "Aliases",
            ProjectUtils.GetGameDirectory(TargetProject.ProjectType));

        List<string> sourceFiles = Directory.GetFiles(dir, "*.json").ToList();

        foreach (string sourceFile in sourceFiles)
        {
            try
            {
                if (!Enum.TryParse(Path.GetFileNameWithoutExtension(sourceFile), out AliasType type)) continue;
                string text = File.ReadAllText(sourceFile);
                try
                {
                    // var options = new JsonSerializerOptions();
                    var entries = JsonSerializer.Deserialize(text, SmithboxSerializerContext.Default.ListAliasEntry);
                    if (!BaseAliases.ContainsKey(type))
                    {
                        BaseAliases.TryAdd(type, entries);
                        continue;
                    }
                    BaseAliases[type] = entries.UnionBy(BaseAliases[type], e => e.ID).ToList();
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the aliases: {sourceFile}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the aliases: {sourceFile}", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }
    }

    public static void Draw()
    {
        // Ignore if setup hasn't been finished yet
        if (BaseEditor == null)
            return;

        if (TargetProject == null)
            return;

        if (TargetProject.Aliases == null)
            return;

        var tableWidth = 890f;
        var tableHeight = 280f;

        var viewport = ImGui.GetMainViewport();
        Vector2 center = viewport.Pos + viewport.Size / 2;

        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        ImGui.SetNextWindowSize(new Vector2(900, 354), ImGuiCond.Always);

        if (Display)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, UI.Current.ImGui_ChildBg);

            if (ImGui.Begin("Project Aliases##projectAliasWindow", ref Display, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse))
            {
                var windowWidth = ImGui.GetWindowWidth();

                Shortcuts();

                var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

                ImGui.BeginChild("tableSection", new Vector2(tableWidth, tableHeight));
                if (ImGui.BeginTable($"projectAliasTbl", 3, tblFlags))
                {
                    ImGui.TableSetupColumn("List", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("EntryList", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Editor", ImGuiTableColumnFlags.WidthFixed);

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    DisplayAliasSelection();

                    ImGui.TableSetColumnIndex(1);

                    DisplayAliasEntryList();

                    ImGui.TableSetColumnIndex(2);

                    DisplayAliasEditor();

                    ImGui.EndTable();
                }
                ImGui.EndChild();

                // Commit
                if (ImGui.Button("Commit##updateProjectAliases", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Display = false;
                    Save();
                }
                UIHelper.Tooltip("Commits the project aliases changes to the stored project aliases.");

                ImGui.SameLine();

                // Cancel
                if (ImGui.Button("Close##closeProjectAliases", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Display = false;
                }
                UIHelper.Tooltip("Closes the project alias editor.");

                ImGui.End();
            }

            ImGui.PopStyleColor(1);
        }
    }

    private static AliasType CurrentAliasEditor = AliasType.None;
    private static string AliasEntryFilter = "";
    private static AliasEntry CurrentAliasEntry;

    /// <summary>
    /// Select which aliases to edit
    /// </summary>
    private static void DisplayAliasSelection()
    {
        foreach(var entry in Enum.GetValues<AliasType>())
        {
            if (entry == AliasType.None)
                continue;

            if(ImGui.Selectable($"{entry.GetDisplayName()}##aliasType_{entry}", entry == CurrentAliasEditor))
            {
                CurrentAliasEditor = entry;
                CurrentAliasEntry = null;
            }

            if(entry == CurrentAliasEditor)
            {
                if (ImGui.BeginPopupContextItem($"AliasTypeContextMenu"))
                {
                    if (ImGui.Selectable($"Copy Entries as Text"))
                    {
                        var entries = new List<string>();

                        var source = GetAliasList();

                        foreach(var aliasEntry in source)
                        {
                            var line = $"{aliasEntry.ID}:{aliasEntry.Name}";
                            entries.Add(line);
                        }

                        var output = string.Join("\n", entries.ToArray());
                        PlatformUtils.Instance.SetClipboardText(output);
                    }

                    if(CFG.Current.EnableWikiTools)
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
        }
    }

    /// <summary>
    /// Editor for the currently selected alias category
    /// </summary>
    private static void DisplayAliasEntryList()
    {
        if (CurrentAliasEditor == AliasType.None)
            return;

        var source = GetAliasList();

        DPI.ApplyInputWidth(360f);
        ImGui.InputText("##aliasEntryFilter", ref AliasEntryFilter, 255);
        UIHelper.Tooltip("Filter the alias entry list by this term.");

        ImGui.BeginChild("aliasEntryList", DPI.ListSize(360f, 238f));

        for(int i = 0; i < source.Count; i++)
        {
            var entryImGuiID = $"entry{i}";
            var entry = source[i];
            var display = true;

            if (AliasEntryFilter != "")
            {
                display = false;
                if (entry.ID.Contains(AliasEntryFilter))
                {
                    display = true;
                }
                if (entry.Name.ToLower().Contains(AliasEntryFilter.ToLower()))
                {
                    display = true;
                }

                var tagMatch = entry.Tags.Any(e => e.ToLower().Contains(AliasEntryFilter.ToLower()));
                if (tagMatch)
                {
                    display = true;
                }
            }

            if(display)
            {
                var displayName = InterfaceUtils.TruncateWithEllipsis($"{entry.ID}:{entry.Name}", 42);

                if (ImGui.Selectable($"{displayName}##aliasEntry_{entryImGuiID}", entry == CurrentAliasEntry))
                {
                    CurrentAliasEntry = entry;
                }

                if (ImGui.BeginPopupContextItem($"AliasEntryContextMenu{entryImGuiID}"))
                {
                    if (ImGui.Selectable($"Duplicate##duplicateEntry_{entryImGuiID}"))
                    {
                        var duplicateEntry = new AliasEntry();
                        duplicateEntry.ID = entry.ID;
                        duplicateEntry.Name = $"{entry.Name}_1";
                        duplicateEntry.Tags = entry.Tags;

                        var action = new ChangeAliasList(source, entry, duplicateEntry, ChangeAliasList.AliasListChange.Add, i+1);
                        TargetProject.ActionManager.ExecuteAction(action);
                    }

                    if (ImGui.Selectable($"Remove##removeEntry_{entryImGuiID}"))
                    {
                        var action = new ChangeAliasList(source, entry, null, ChangeAliasList.AliasListChange.Remove, i);
                        TargetProject.ActionManager.ExecuteAction(action);
                    }

                    ImGui.EndPopup();
                }
            }
        }

        if(source.Count == 0)
        {
            if (ImGui.Button("Add##addAliasEntry", DPI.StandardButtonSize))
            {
                var blankEntry = new AliasEntry();
                blankEntry.ID = "BLANK_ID";
                blankEntry.Name = "BLANK_NAME";
                blankEntry.Tags = new List<string>();
                source.Add(blankEntry);
            }
            UIHelper.Tooltip("Add an alias entry for this alias list.");
        }
        ImGui.EndChild();
    }

    /// <summary>
    /// Editor for the currently selected alias category
    /// </summary>
    private static void DisplayAliasEditor()
    {
        if (CurrentAliasEditor == AliasType.None)
            return;

        if (CurrentAliasEntry == null)
            return;

        var curID = CurrentAliasEntry.ID;
        var curName = CurrentAliasEntry.Name;

        DPI.ApplyInputWidth(250f);
        ImGui.InputText("Alias ID##curAliasID", ref curID, 255);
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            var action = new ChangeAliasField(
                CurrentAliasEntry, CurrentAliasEntry.ID, curID, ChangeAliasField.AliasField.ID);
            
            TargetProject.ActionManager.ExecuteAction(action);
        }
        UIHelper.Tooltip("The ID of the currently selected alias entry.");

        DPI.ApplyInputWidth(250f);
        ImGui.InputText("Alias Name##curAliasName", ref curName, 255);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            var action = new ChangeAliasField(
                CurrentAliasEntry, CurrentAliasEntry.Name, curName, ChangeAliasField.AliasField.Name);

            TargetProject.ActionManager.ExecuteAction(action);
        }
        UIHelper.Tooltip("The name of the currently selected alias entry.");

        for(int i = 0; i < CurrentAliasEntry.Tags.Count; i++)
        {
            var tag = CurrentAliasEntry.Tags[i];
            var curTag = tag;

            ImGui.InputText($"Tag##curTagEntry{i}", ref curTag, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeAliasField(
                    CurrentAliasEntry, tag, curTag, ChangeAliasField.AliasField.Tags, i);

                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("A tag for the currently selected alias entry.");

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Minus}##tagRemove{i}", DPI.IconButtonSize))
            {
                var action = new ChangeAliasTagList(CurrentAliasEntry.Tags, tag, tag, ChangeAliasTagList.TagListChange.Remove, i);

                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("Remove this tag.");

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Plus}##tagAdd{i}", DPI.IconButtonSize))
            {
                var action = new ChangeAliasTagList(CurrentAliasEntry.Tags, tag, tag, ChangeAliasTagList.TagListChange.Add, i + 1);
                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("Duplicate this tag.");
        }
    }

    /// <summary>
    /// Get the alias list based on the current alias editor selected
    /// </summary>
    /// <returns></returns>
    private static List<AliasEntry> GetAliasList()
    {
        var source = new List<AliasEntry>();

        if (TargetProject.Aliases.ContainsKey(CurrentAliasEditor))
            source = TargetProject.Aliases[CurrentAliasEditor];

        return source;
    }

    /// <summary>
    /// Undo/Redo for alias entry changes
    /// </summary>
    private static void Shortcuts()
    {
        // Undo
        if (TargetProject.ActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            TargetProject.ActionManager.UndoAction();
        }

        // Redo
        if (TargetProject.ActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            TargetProject.ActionManager.RedoAction();
        }
    }

    /// <summary>
    /// Save the current session's project aliases to file.
    /// </summary>
    public static void Save()
    {
        var projectFolder = Path.Join(TargetProject.ProjectPath, ".smithbox", "Assets", "Aliases");

        if (!Directory.Exists(projectFolder))
            Directory.CreateDirectory(projectFolder);

        foreach ((AliasType aliasType, List<AliasEntry> aliases) in TargetProject.Aliases)
        {
            string path = Path.Combine(projectFolder, $"{aliasType}.json");

            List<AliasEntry> baseAliases =
                (BaseAliases != null && BaseAliases.TryGetValue(aliasType, out List<AliasEntry> bAliases)) ? bAliases : new();

            List<AliasEntry> diffAliases = aliases.Where(a =>
            {
                var baseA = baseAliases.FirstOrDefault(b => b.ID == a.ID);

                if (baseA == null) return true;
                return baseA.Name != a.Name || !baseA.Tags.SequenceEqual(a.Tags);
            }).ToList();

            if (!diffAliases.Any()) continue;

            var json = JsonSerializer.Serialize(diffAliases, SmithboxSerializerContext.Default.ListAliasEntry);

            File.WriteAllText(path, json);
        }
    }
}
