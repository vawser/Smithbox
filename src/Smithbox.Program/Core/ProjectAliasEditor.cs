using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Core;

public static class ProjectAliasEditor
{
    private static Smithbox BaseEditor;
    private static ProjectEntry TargetProject;

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

        Display = true;
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

            if (ImGui.Begin("Project Aliases##projectAliasWindow", ref Display, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse))
            {
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

                var buttonSize = new Vector2(440, 24);

                // Commit
                if (ImGui.Button("Commit##updateProjectAliases", buttonSize))
                {
                    Display = false;
                    Save();
                }
                UIHelper.Tooltip("Commits the project aliases changes to the stored project aliases.");

                ImGui.SameLine();

                // Cancel
                if (ImGui.Button("Close##closeProjectAliases", buttonSize))
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
        }
    }

    /// <summary>
    /// Editor for the currently selected alias category
    /// </summary>
    private static void DisplayAliasEntryList()
    {
        var sectionWidth = 360f;
        var listHeight = 238f;

        if (CurrentAliasEditor == AliasType.None)
            return;

        var source = GetAliasList();

        ImGui.SetNextItemWidth(sectionWidth);
        ImGui.InputText("##aliasEntryFilter", ref AliasEntryFilter, 255);
        UIHelper.Tooltip("Filter the alias entry list by this term.");

        ImGui.BeginChild("aliasEntryList", new Vector2(sectionWidth, listHeight));

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
            var buttonSize = new Vector2(sectionWidth - 5, 24);

            if (ImGui.Button("Add##addAliasEntry", buttonSize))
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
        var inputWidth = 250f;

        if (CurrentAliasEditor == AliasType.None)
            return;

        if (CurrentAliasEntry == null)
            return;

        var curID = CurrentAliasEntry.ID;
        var curName = CurrentAliasEntry.Name;

        ImGui.SetNextItemWidth(inputWidth);
        ImGui.InputText("Alias ID##curAliasID", ref curID, 255);
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            var action = new ChangeAliasField(
                CurrentAliasEntry, CurrentAliasEntry.ID, curID, ChangeAliasField.AliasField.ID);
            
            TargetProject.ActionManager.ExecuteAction(action);
        }
        UIHelper.Tooltip("The ID of the currently selected alias entry.");
        
        ImGui.SetNextItemWidth(inputWidth);
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

            if (ImGui.Button($"{Icons.Minus}##tagRemove{i}"))
            {
                var action = new ChangeAliasTagList(CurrentAliasEntry.Tags, tag, tag, ChangeAliasTagList.TagListChange.Remove, i);

                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("Remove this tag.");

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Plus}##tagAdd{i}"))
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

        switch (CurrentAliasEditor)
        {
            case AliasType.None:
                break;

            case AliasType.Assets:
                source = TargetProject.Aliases.Assets;
                break;

            case AliasType.Characters:
                source = TargetProject.Aliases.Characters;
                break;

            case AliasType.Cutscenes:
                source = TargetProject.Aliases.Cutscenes;
                break;

            case AliasType.EventFlags:
                source = TargetProject.Aliases.EventFlags;
                break;

            case AliasType.Gparams:
                source = TargetProject.Aliases.Gparams;
                break;

            case AliasType.MapPieces:
                source = TargetProject.Aliases.MapPieces;
                break;

            case AliasType.MapNames:
                source = TargetProject.Aliases.MapNames;
                break;

            case AliasType.Movies:
                source = TargetProject.Aliases.Movies;
                break;

            case AliasType.Particles:
                source = TargetProject.Aliases.Particles;
                break;

            case AliasType.Parts:
                source = TargetProject.Aliases.Parts;
                break;

            case AliasType.Sounds:
                source = TargetProject.Aliases.Sounds;
                break;

            case AliasType.TalkScripts:
                source = TargetProject.Aliases.TalkScripts;
                break;

            case AliasType.TimeActs:
                source = TargetProject.Aliases.TimeActs;
                break;
        }

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
        var projectFolder = $@"{TargetProject.ProjectPath}\.smithbox\Assets\Aliases\{ProjectUtils.GetGameDirectory(TargetProject.ProjectType)}";
        var projectFile = Path.Combine(projectFolder, "Aliases.json");

        var json = JsonSerializer.Serialize(TargetProject.Aliases, SmithboxSerializerContext.Default.AliasStore);

        if(!Directory.Exists(projectFolder))
        {
            Directory.CreateDirectory(projectFolder);
        }

        File.WriteAllText(projectFile, json);
    }

    /// <summary>
    /// Keep in sync with the AliasStore class
    /// </summary>
    public enum AliasType
    {
        [Display(Name = "None")]
        None,
        [Display(Name = "Assets")]
        Assets,
        [Display(Name = "Characters")]
        Characters,
        [Display(Name = "Cutscenes")]
        Cutscenes,
        [Display(Name = "Event Flags")]
        EventFlags,
        [Display(Name = "Gparams")]
        Gparams,
        [Display(Name = "Map Pieces")]
        MapPieces,
        [Display(Name = "Map Names")]
        MapNames,
        [Display(Name = "Movies")]
        Movies,
        [Display(Name = "Particles")]
        Particles,
        [Display(Name = "Parts")]
        Parts,
        [Display(Name = "Sounds")]
        Sounds,
        [Display(Name = "Talk Scripts")]
        TalkScripts,
        [Display(Name = "Time Acts")]
        TimeActs
    }
}
