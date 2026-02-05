using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using Veldrid;

namespace StudioCore.Editors.ParamEditor;


public class ParamTableWindow
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamEditorView ParentView;

    public bool _arrowKeyPressed;

    public string CurrentTableGroupSearch = "";

    public List<int> CurrentTableGroups = new List<int>();
    public int CurrentTableGroup = -1;
    public List<int> CurrentTableGroupIndices = new();

    public int TotalChance = 0;
    private Dictionary<Param.Row, string> RollChances = new();

    public ParamTableWindow(ParamEditorScreen editor, ProjectEntry project, ParamEditorView parentView)
    {
        Editor = editor;
        Project = project;
        ParentView = parentView;
    }

    public void Display(bool doFocus, bool isActiveView, float scrollTo, string paramKey)
    {
        DisplayHeader(isActiveView);

        ImGui.BeginChild("rowGroupSection");
        FocusManager.SetFocus(EditorFocusContext.ParamEditor_TableList);

        DisplayRowGroups(doFocus, scrollTo);

        Shortcuts();

        ImGui.EndChild();
    }

    private void DisplayHeader(bool isActiveView)
    {
        ImGui.Text("Table Groups");

        ImGui.Separator();
        ImGui.AlignTextToFramePadding();
        ImGui.InputText($"##rowGroupSearch", ref CurrentTableGroupSearch, 256);

        // Toggle Row Display Type
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##rowDisplayType"))
        {
            if (CFG.Current.ParamEditor_Table_List_Row_Display_Type is ParamTableRowDisplayType.ID)
            {
                CFG.Current.ParamEditor_Table_List_Row_Display_Type = ParamTableRowDisplayType.None;
            }
            else if (CFG.Current.ParamEditor_Table_List_Row_Display_Type is ParamTableRowDisplayType.None)
            {
                CFG.Current.ParamEditor_Table_List_Row_Display_Type = ParamTableRowDisplayType.ID;
            }
        }

        var rowDisplayType = "";
        if (CFG.Current.ParamEditor_Table_List_Row_Display_Type is ParamTableRowDisplayType.ID)
        {
            rowDisplayType = "ID";
        }
        if (CFG.Current.ParamEditor_Table_List_Row_Display_Type is ParamTableRowDisplayType.None)
        {
            rowDisplayType = "None";
        }

        UIHelper.Tooltip($"Toggle the row display type when in Table mode.\nCurrent Mode: {rowDisplayType}");

        if (CFG.Current.Developer_Enable_Tools)
        {
            ParamDebugTools.DisplayQuickTableNameExport(Editor, Project);
        }

        ImGui.Separator();
    }

    private void DisplayRowGroups(bool doFocus, float scrollTo)
    {
        foreach (var group in CurrentTableGroups)
        {
            var groupStr = $"{group}";

            if (CurrentTableGroupSearch != "" && !groupStr.Contains(CurrentTableGroupSearch))
                continue;

            var displayName = $"{group}";

            var activeParam = ParentView.Selection.GetActiveParam();

            if (Project.Handler.ParamData.TableGroupNames != null)
            {
                if (Project.Handler.ParamData.TableGroupNames.Groups.Any(e => e.Param == activeParam))
                {
                    var curGroup = Project.Handler.ParamData.TableGroupNames.Groups.FirstOrDefault(e => e.Param == activeParam);

                    if (curGroup != null)
                    {
                        if (curGroup.Entries.Any(e => e.ID == group))
                        {
                            var curEntry = curGroup.Entries.FirstOrDefault(e => e.ID == group);

                            displayName = $"{group} {curEntry.Name}";
                        }
                    }
                }
            }

            if (ImGui.Selectable($"{displayName}##rowGroup{group}", CurrentTableGroup == group))
            {
                UpdateTableGroupSelection(group);
            }

            // Up/Down arrow key input
            if (InputManager.HasArrowSelection())
            {
                if (!ImGui.IsAnyItemActive())
                {
                    _arrowKeyPressed = true;
                }
            }

            if (_arrowKeyPressed && ImGui.IsItemFocused())
            {
                UpdateTableGroupSelection(group);

                _arrowKeyPressed = false;
            }

            DisplayContextMenu(group);
        }

        if (ApplyTableGroupDuplicate)
        {
            DuplicateTableGroup();
            ApplyTableGroupDuplicate = false;
        }

        if (ApplyTableGroupDelete)
        {
            DeleteTableGroup();
            ApplyTableGroupDelete = false;
        }
    }

    public void DisplayContextMenu(int groupKey)
    {
        if (ImGui.BeginPopupContextItem($"{groupKey}"))
        {
            var activeParam = ParentView.Selection.GetActiveParam();

            if (Project.Handler.ParamData.TableGroupNames != null)
            {
                var curTableGroup = Project.Handler.ParamData.TableGroupNames.Groups.FirstOrDefault(e => e.Param == activeParam);

                var curName = "";
                if (curTableGroup != null)
                {
                    if (curTableGroup.Entries.Any(e => e.ID == groupKey))
                    {
                        curName = curTableGroup.Entries.FirstOrDefault(e => e.ID == groupKey).Name;
                    }
                }

                // Name Input
                ImGui.InputText("##tableGroupName", ref curName, 255);

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    UpdateTableGroupNames(activeParam, curTableGroup, groupKey, curName);
                }
            }

            if (ImGui.BeginMenu("Duplicate"))
            {
                ImGui.InputInt("Offset##duplicateOffset", ref CFG.Current.Param_Toolbar_Duplicate_Offset);

                UIHelper.Tooltip("The ID offset to apply when duplicating.\nSet to 0 for row indexed params to duplicate as expected.");

                ImGui.InputInt("Amount##duplicateAmount", ref CFG.Current.Param_Toolbar_Duplicate_Amount);

                UIHelper.Tooltip("The number of times the current selection will be duplicated.");

                ImGui.Checkbox("Allow Unrestricted Duplicate##allowUnrestrictedDuplicate", ref CFG.Current.Param_TableGroupView_AllowDuplicateInject);

                UIHelper.Tooltip("If enabled, duplicate will allow for ID collisions. A collided duplicate will add the source rows into the collided group with the new ID.");

                if (ImGui.Selectable("Apply"))
                {
                    ApplyTableGroupDuplicate = true;
                }
                UIHelper.Tooltip($"Duplicate this table group. This will duplicate the rows that comprise this group.");

                ImGui.EndMenu();
            }

            if (ImGui.Selectable("Delete"))
            {
                ApplyTableGroupDelete = true;
            }
            UIHelper.Tooltip($"Delete this table group. This will remove the rows that comprise this group.");

            ImGui.EndPopup();
        }
    }

    public void Shortcuts()
    {
        if (FocusManager.Focus is EditorFocusContext.ParamEditor_TableList)
        {
            if (!ImGui.IsAnyItemActive() && CurrentTableGroup != -1)
            {
                if (InputManager.IsPressed(KeybindID.Duplicate))
                {
                    ApplyTableGroupDuplicate = true;
                }

                if (InputManager.IsPressed(KeybindID.Delete))
                {
                    ApplyTableGroupDelete = true;
                }
            }
        }
    }

    private bool ApplyTableGroupDuplicate = false;
    private bool ApplyTableGroupDelete = false;

    public void DuplicateTableGroup()
    {
        var curParamKey = ParentView.Selection.GetActiveParam();
        var idOffset = CFG.Current.Param_Toolbar_Duplicate_Offset;
        var allowUnrestricted = CFG.Current.Param_TableGroupView_AllowDuplicateInject;

        if (curParamKey == null)
            return;

        Param param = Editor.Project.Handler.ParamData.PrimaryBank.Params[curParamKey];

        var newId = -1;

        var targetRows = param.Rows.Where(e => e.ID == CurrentTableGroup).ToList();

        newId = targetRows.First().ID + idOffset;

        if (!allowUnrestricted && param.Rows.Any(e => e.ID == newId))
        {
            Smithbox.LogError(this, "Duplicate aborted. This duplicate would have injected rows into an existing table group.");
        }
        else
        {
            ParentView.Selection.ClearRowSelection();

            foreach (var entry in targetRows)
            {
                ParentView.Selection.AddRowToSelection(entry);
            }

            CurrentTableGroups.Clear();

            ParamRowDuplicate.ApplyDuplicate(ParentView, true);

            UpdateTableSelection(curParamKey);
        }
    }

    public void DeleteTableGroup()
    {
        var curParamKey = ParentView.Selection.GetActiveParam();

        if (curParamKey == null)
            return;

        Param param = Editor.Project.Handler.ParamData.PrimaryBank.Params[curParamKey];

        var targetRows = param.Rows.Where(e => e.ID == CurrentTableGroup).ToList();

        ParentView.Selection.ClearRowSelection();

        foreach (var entry in targetRows)
        {
            ParentView.Selection.AddRowToSelection(entry);
        }

        CurrentTableGroups.Clear();

        ParamRowDelete.ApplyDelete(ParentView);

        UpdateTableSelection(curParamKey);
    }

    public void UpdateTableGroupNames(string activeParam, TableGroupParamEntry curTableGroup, int groupKey, string curName)
    {
        if (curTableGroup == null)
        {
            var newTableGroup = new TableGroupParamEntry();
            newTableGroup.Param = activeParam;
            newTableGroup.Entries = new();

            var newTableEntry = new TableGroupEntry();
            newTableEntry.ID = groupKey;
            newTableEntry.Name = curName;

            newTableGroup.Entries.Add(newTableEntry);

            Project.Handler.ParamData.TableGroupNames.Groups.Add(newTableGroup);
        }
        else
        {
            if (curTableGroup.Entries.Any(e => e.ID == groupKey))
            {
                var curEntry = curTableGroup.Entries.FirstOrDefault(e => e.ID == groupKey);
                if (curEntry != null)
                {
                    curEntry.Name = curName;
                }
            }
            else
            {
                var newTableEntry = new TableGroupEntry();
                newTableEntry.ID = groupKey;
                newTableEntry.Name = curName;

                curTableGroup.Entries.Add(newTableEntry);
            }
        }
    }

    public void UpdateTableSelection()
    {
        var curParamKey = ParentView.Selection.GetActiveParam();

        UpdateTableSelection(curParamKey);
    }

    /// <summary>
    /// Build the row group list when a valid param is selected
    /// </summary>
    /// <param name="activeParam"></param>
    public void UpdateTableSelection(string activeParam)
    {
        CurrentTableGroups.Clear();

        if (IsInTableGroupMode(activeParam))
        {
            var curParam = Project.Handler.ParamData.PrimaryBank.Params[activeParam];

            foreach (var entry in curParam.Rows)
            {
                if (!CurrentTableGroups.Contains(entry.ID))
                {
                    CurrentTableGroups.Add(entry.ID);
                }
            }
        }
    }

    public void UpdateTableGroupSelection(int group)
    {
        RollChances = new();

        TotalChance = 0;

        var curParam = ParentView.Selection.GetActiveParam();
        var fieldName = GetChanceFieldName();

        // Get overall roll chance total
        foreach (var row in Project.Handler.ParamData.PrimaryBank.Params[curParam].Rows)
        {
            if (row.ID != group)
                continue;

            if (row.Cells.Any(e => e.Def.InternalName == fieldName))
            {
                var chanceWeight = row.Cells.FirstOrDefault(e => e.Def.InternalName == fieldName);
                if (chanceWeight.Value != null)
                {
                    TotalChance = TotalChance + int.Parse($"{chanceWeight.Value}");
                }
            }
        }

        // Construct list of the individual roll chances
        foreach (var row in Project.Handler.ParamData.PrimaryBank.Params[curParam].Rows)
        {
            if (row.ID != group)
                continue;

            if (row.Cells.Any(e => e.Def.InternalName == fieldName))
            {
                var chanceWeight = row.Cells.FirstOrDefault(e => e.Def.InternalName == fieldName);
                if (chanceWeight.Value != null)
                {
                    if (TotalChance != 0)
                    {
                        var curChance = int.Parse($"{chanceWeight.Value}");

                        float curRollChance = 0;

                        if (curChance != 0)
                        {
                            curRollChance = ((float)curChance / (float)TotalChance) * 100;
                        }

                        if (!RollChances.ContainsKey(row))
                        {
                            RollChances.Add(row, curRollChance.ToString("F3"));
                        }
                    }
                }
            }

        }

        CurrentTableGroup = group;
    }

    public void DisplayTableEntryChance(Param.Row r)
    {
        var displayChance = "";

        if (RollChances.ContainsKey(r))
        {
            displayChance = RollChances[r];
        }

        if (displayChance != "")
        {
            ImGui.SameLine();

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
            ImGui.TextUnformatted($@" {displayChance}%");
            ImGui.PopStyleColor(1);
        }
    }

    public string GetChanceFieldName()
    {
        var curParam = ParentView.Selection.GetActiveParam();

        switch (curParam)
        {
            case "AttachEffectTableParam":
            case "ItemTableParam":
            case "MagicTableParam":
            case "SwordArtsTableParam":
                return "chanceWeight";
            default: break;
        }

        return "";
    }

    public bool IsInTableGroupMode(string activeParam)
    {
        if (!CFG.Current.ParamEditor_Display_Table_List)
            return false;

        if (Project.Handler.ParamData.TableParamList.Params.Count == 0)
            return false;

        if (Project.Handler.ParamData.TableParamList.Params.Contains(activeParam))
        {
            return true;
        }

        return false;
    }

    public void WriteTableGroupNames(string writeDir = "")
    {
        // If the project type doesn't support table groups, don't attempt to write anything
        if (Project.Handler.ParamData.TableGroupNames == null)
            return;

        if (Project.Handler.ParamData.TableParamList.Params.Count == 0)
            return;

        var targetDir = writeDir;

        if (writeDir == "")
        {
            targetDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Community Table Names");
        }

        if (!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        foreach (var group in Project.Handler.ParamData.TableGroupNames.Groups)
        {
            var filePath = Path.Combine(targetDir, $"{group.Param}.json");

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(group, typeof(TableGroupParamEntry), options);

            File.WriteAllText(filePath, json);
        }
    }
}
