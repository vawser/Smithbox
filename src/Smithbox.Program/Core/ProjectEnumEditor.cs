using Google.Protobuf.WellKnownTypes;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SoulsFormats.MQB;
using static StudioCore.Core.ChangeEnumList;
using static StudioCore.Core.ProjectAliasEditor;

namespace StudioCore.Core;

public class ProjectEnumEditor
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

            if (ImGui.Begin("Project Enums##projectEnumsWindow", ref Display, ImGuiWindowFlags.NoResize  | ImGuiWindowFlags.NoCollapse))
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

                    DisplayEnumEntries();

                    ImGui.TableSetColumnIndex(1);

                    DisplayOptionEntries();

                    ImGui.TableSetColumnIndex(2);

                    DisplayEditor();

                    ImGui.EndTable();
                }
                ImGui.EndChild();

                // Commit
                if (ImGui.Button("Commit##updateProjectEnums", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Display = false;
                    Save();
                }
                UIHelper.Tooltip("Commits the project enum changes to the stored project enums.");

                ImGui.SameLine();

                // Cancel
                if (ImGui.Button("Close##closeProjectEnums", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Display = false;
                }
                UIHelper.Tooltip("Closes the project enum editor.");

                ImGui.End();
            }

            ImGui.PopStyleColor(1);
        }
    }

    private static ProjectEnumEntry CurrentEnum;
    private static ProjectEnumOption CurrentOption;

    private static string OptionEntryFilter = "";

    private static void DisplayEnumEntries()
    {
        foreach(var entry in TargetProject.ProjectEnums.List)
        {
            if (ImGui.Selectable($"{entry.Name}##enumEntry_{entry.Name}", entry == CurrentEnum))
            {
                CurrentEnum = entry;
                CurrentOption = null;
            }
        }
    }

    private static void DisplayOptionEntries()
    {
        DPI.ApplyInputWidth(260f);
        ImGui.InputText("##optionEntryFilter", ref OptionEntryFilter, 255);
        UIHelper.Tooltip("Filter the option entry list by this term.");

        ImGui.BeginChild("optionEntryList", DPI.ListSize(260f, 238f));

        if (CurrentEnum != null)
        {
            for(int i = 0; i < CurrentEnum.Options.Count; i++)
            {
                var option = CurrentEnum.Options[i];

                var name = option.Name.ToLower();
                var filter = OptionEntryFilter.ToLower();

                if (name.Contains(filter))
                {
                    if (ImGui.Selectable($"[{option.ID}] {option.Name}##optionEntry_{option.Name}", option == CurrentOption))
                    {
                        CurrentOption = option;
                    }

                    if (ImGui.BeginPopupContextItem($"OptionEntryContextMenu_{option.Name}"))
                    {
                        if (ImGui.Selectable($"Duplicate##duplicateEntry_{option.Name}"))
                        {
                            var duplicateEntry = new ProjectEnumOption();
                            duplicateEntry.ID = option.ID;
                            duplicateEntry.Name = $"{option.Name}_1";
                            duplicateEntry.Description = option.Description;

                            var action = new ChangeEnumList(CurrentEnum, CurrentOption, duplicateEntry, 
                                EnumListChange.Add, i + 1);
                            TargetProject.ActionManager.ExecuteAction(action);
                        }

                        if (ImGui.Selectable($"Remove##removeEntry_{option.Name}"))
                        {
                            var action = new ChangeEnumList(CurrentEnum, CurrentOption, null,
                                EnumListChange.Remove, i);

                            TargetProject.ActionManager.ExecuteAction(action);
                        }

                        ImGui.EndPopup();
                    }
                }
            }

            if(CurrentEnum.Options.Count == 0)
            {
                if (ImGui.Button("Add##addOptionEntry", DPI.StandardButtonSize))
                {
                    var blankEntry = new ProjectEnumOption();
                    blankEntry.ID = "BLANK_ID";
                    blankEntry.Name = "BLANK_NAME";
                    blankEntry.Description = "";

                    if(CurrentEnum.Options == null)
                    {
                        CurrentEnum.Options = new();
                    }

                    CurrentEnum.Options.Add(blankEntry);
                }
                UIHelper.Tooltip("Add an otpion entry for this enum list.");
            }
        }

        ImGui.EndChild();
    }

    private static void DisplayEditor()
    {
        // Enum
        if (CurrentEnum != null)
        {
            var curDisplayName = CurrentEnum.DisplayName;
            var curDescription = CurrentEnum.Description;

            // Display Name
            DPI.ApplyInputWidth(250f);
            ImGui.InputText("Enum Display Name##enumDisplayName", ref curDisplayName, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeEnumField(
                    CurrentEnum, CurrentEnum.DisplayName, curDisplayName, ChangeEnumField.EnumField.DisplayName);

                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("The display name of the currently selected enum entry.");

            // Description
            DPI.ApplyInputWidth(250f);
            ImGui.InputText("Enum Description##curDescription", ref curDescription, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeEnumField(
                    CurrentEnum, CurrentEnum.Description, curDescription, ChangeEnumField.EnumField.Description);

                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("The description of the currently selected enum entry.");
        }

        // Option
        if (CurrentOption != null)
        {
            ImGui.Separator();

            var curID = CurrentOption.ID;
            var curName = CurrentOption.Name;
            var curDescription = CurrentOption.Description;

            // ID
            DPI.ApplyInputWidth(250f);
            ImGui.InputText("Option ID##enumOptionID", ref curID, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeEnumOptionField(
                    CurrentOption, CurrentOption.ID, curID, ChangeEnumOptionField.EnumOptionField.ID);

                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("The ID of the currently selected enum option entry.");

            // Name
            DPI.ApplyInputWidth(250f);
            ImGui.InputText("Option Name##enumOptionName", ref curName, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeEnumOptionField(
                    CurrentOption, CurrentOption.Name, curName, ChangeEnumOptionField.EnumOptionField.Name);

                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("The name of the currently selected enum option entry.");

            // Description
            DPI.ApplyInputWidth(250f);
            ImGui.InputText("Option Description##enumOptionDescription", ref curDescription, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var action = new ChangeEnumOptionField(
                    CurrentOption, CurrentOption.Description, curDescription, ChangeEnumOptionField.EnumOptionField.Description);

                TargetProject.ActionManager.ExecuteAction(action);
            }
            UIHelper.Tooltip("The description of the currently selected enum option entry.");
        }
    }

    /// <summary>
    /// Undo/Redo for enum entry changes
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
    /// Save the current session's project enums to file.
    /// </summary>
    public static void Save()
    {
        var projectFolder = Path.Join(TargetProject.ProjectPath, ".smithbox", "Project");
        var projectFile = Path.Combine(projectFolder, "Shared Param Enums.json");

        var json = JsonSerializer.Serialize(TargetProject.ProjectEnums, SmithboxSerializerContext.Default.ProjectEnumResource);

        if (!Directory.Exists(projectFolder))
        {
            Directory.CreateDirectory(projectFolder);
        }

        File.WriteAllText(projectFile, json);
    }
}
