using Andre.Formats;
using Hexa.NET.DirectXTex;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
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

    public List<int> CurrentTableGroupIDs = new List<int>();
    public int CurrentTableGroupID = -1;
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
        DisplayTitle();

        DisplayHeader(isActiveView);

        ImGui.BeginChild("rowGroupSection");
        FocusManager.SetFocus(EditorFocusContext.ParamEditor_TableList);

        DisplayTableListHeader(doFocus, scrollTo);
        DisplayTableList(doFocus, scrollTo);

        Shortcuts();

        ImGui.EndChild();
    }
    public void DisplayTitle()
    {
        var tableListTitle = "Table List";

        UIHelper.SimpleHeader($"{tableListTitle}", "");
    }

    private void DisplayHeader(bool isActiveView)
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("ParamTableListHeaderSection", searchHeight, ImGuiChildFlags.Borders);

        ImGui.AlignTextToFramePadding();
        ImGui.InputText($"##rowGroupSearch", ref CurrentTableGroupSearch, 256);

        // Toggle Row Display Type
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##rowDisplayType"))
        {
            if (CFG.Current.ParamEditor_Table_List_Row_Name_Display_Type is ParamTableRowDisplayType.ID)
            {
                CFG.Current.ParamEditor_Table_List_Row_Name_Display_Type = ParamTableRowDisplayType.None;
            }
            else if (CFG.Current.ParamEditor_Table_List_Row_Name_Display_Type is ParamTableRowDisplayType.None)
            {
                CFG.Current.ParamEditor_Table_List_Row_Name_Display_Type = ParamTableRowDisplayType.ID;
            }
        }

        var rowDisplayType = "";
        if (CFG.Current.ParamEditor_Table_List_Row_Name_Display_Type is ParamTableRowDisplayType.ID)
        {
            rowDisplayType = "ID";
        }
        if (CFG.Current.ParamEditor_Table_List_Row_Name_Display_Type is ParamTableRowDisplayType.None)
        {
            rowDisplayType = "None";
        }

        UIHelper.Tooltip($"Toggle the row display type when in Table mode.\nCurrent Mode: {rowDisplayType}");

        if (CFG.Current.Developer_Enable_Tools)
        {
            ParamDebugTools.DisplayQuickTableNameExport(Editor, Project);
        }

        ImGui.EndChild();
    }

    private void DisplayTableListHeader(bool doFocus, float scrollTo)
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders;

        var columnCount = 2;

        if (ImGui.BeginTable($"tableListHeader", columnCount, tblFlags))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_TableList);

            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthStretch, 0.2f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);

            // ID
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("ID");

            // Name
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Name");

            ImGui.EndTable();
        }
    }

    private void DisplayTableList(bool doFocus, float scrollTo)
    {
        ImGui.BeginChild("TableListSection", ImGuiChildFlags.None);

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;

        if (CFG.Current.ParamEditor_Enable_Table_Borders)
        {
            tblFlags = tblFlags | ImGuiTableFlags.Borders;
        }

        var columnCount = 2;

        if (ImGui.BeginTable($"TableListTbl", columnCount, tblFlags))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_TableList);

            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthStretch, 0.2f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);

            for (int i = 0; i < CurrentTableGroupIDs.Count; i++)
            {
                var curTableID = CurrentTableGroupIDs[i];
                var filterText = GetTableName(curTableID);

                if (!IsValidMatch(curTableID, filterText))
                    continue;

                // ID
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                DisplayTableSelectable_ID(curTableID);

                // Name
                ImGui.TableSetColumnIndex(1);
                DisplayTableSelectable_Name(curTableID);
            }

            ImGui.EndTable();
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

        ImGui.EndChild();
    }

    public void DisplayTableSelectable_ID(int curTableID)
    {
        var displayName = $"{curTableID}";

        if (ImGui.Selectable($"{displayName}##tableEntry_ID_{curTableID}", CurrentTableGroupID == curTableID))
        {
            UpdateTableGroupSelection(curTableID);
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
            UpdateTableGroupSelection(curTableID);

            _arrowKeyPressed = false;
        }

        DisplayContextMenu("id", curTableID);
    }

    public void DisplayTableSelectable_Name(int curTableID)
    {
        var displayName = GetTableName(curTableID);

        if (ImGui.Selectable($"{displayName}##tableEntry_Name_{curTableID}", CurrentTableGroupID == curTableID))
        {
            UpdateTableGroupSelection(curTableID);
        }

        DisplayContextMenu("name", curTableID);
    }

    public bool IsValidMatch(int curTableID, string filterText)
    {
        var idStr = $"{curTableID}".ToLower();
        var textStr = filterText.ToLower();
        var matchStr = CurrentTableGroupSearch.ToLower();

        if (matchStr != "")
        {
            var display = false;

            if (idStr.Contains(matchStr))
            {
                display = true;
            }
            else if (textStr.Contains(matchStr))
            {
                display = true;
            }

            if (display)
            {
                return true;
            }
        }
        else if(matchStr == "")
        {
            return true;
        }

        return false;
    }

    public string GetTableName(int tableKey)
    {
        var activeParam = ParentView.Selection.GetActiveParam();

        if (Project.Handler.ParamData.TableGroupNames != null)
        {
            if (Project.Handler.ParamData.TableGroupNames.Groups.Any(e => e.Param == activeParam))
            {
                var curGroup = Project.Handler.ParamData.TableGroupNames.Groups.FirstOrDefault(e => e.Param == activeParam);

                if (curGroup != null)
                {
                    if (curGroup.Entries.Any(e => e.ID == tableKey))
                    {
                        var curEntry = curGroup.Entries.FirstOrDefault(e => e.ID == tableKey);

                        return curEntry.Name;
                    }
                }
            }
        }

        return $"";
    }

    public void DisplayContextMenu(string imguiKey, int groupKey)
    {
        if (ImGui.BeginPopupContextItem($"contextmenu_{groupKey}_{imguiKey}"))
        {
            if (imguiKey == "id")
            {
                DisplayEditInput_ID(imguiKey, groupKey);
            }
            else if (imguiKey == "name")
            {
                DisplayEditInput_Name(imguiKey, groupKey);
            }

            if (ImGui.BeginMenu("Duplicate"))
            {
                DisplayDuplicateMenu();

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

    public void DisplayEditInput_ID(string imguiKey, int groupKey)
    {
        var activeParam = ParentView.Selection.GetActiveParam();

        if (Project.Handler.ParamData.TableGroupNames != null)
        {
            var curTableGroup = Project.Handler.ParamData.TableGroupNames.Groups.FirstOrDefault(e => e.Param == activeParam);

            var curID = groupKey;
            var curName = "";
            if (curTableGroup != null)
            {
                if (curTableGroup.Entries.Any(e => e.ID == groupKey))
                {
                    curName = curTableGroup.Entries.FirstOrDefault(e => e.ID == groupKey).Name;
                }
            }

            // ID Input
            ImGui.InputInt("##editTableGroupID", ref curID, 255);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                UpdateTableGroupIDs(activeParam, curTableGroup, groupKey, curID, curName);
            }
        }
    }

    public void DisplayEditInput_Name(string imguiKey, int groupKey)
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
            ImGui.InputText("##editTableGroupName", ref curName, 255);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                UpdateTableGroupNames(activeParam, curTableGroup, groupKey, curName);
            }
        }
    }

    public void DisplayDuplicateMenu()
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
    }

    public void Shortcuts()
    {
        if (FocusManager.Focus is EditorFocusContext.ParamEditor_TableList)
        {
            if (!ImGui.IsAnyItemActive() && CurrentTableGroupID != -1)
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

        var targetRows = param.Rows.Where(e => e.ID == CurrentTableGroupID).ToList();

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

            CurrentTableGroupIDs.Clear();

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

        var targetRows = param.Rows.Where(e => e.ID == CurrentTableGroupID).ToList();

        ParentView.Selection.ClearRowSelection();

        foreach (var entry in targetRows)
        {
            ParentView.Selection.AddRowToSelection(entry);
        }

        CurrentTableGroupIDs.Clear();

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

    public void UpdateTableGroupIDs(string activeParam, TableGroupParamEntry curTableGroup, int groupKey, int newID, string curName)
    {
        if (curTableGroup == null)
        {
            var newTableGroup = new TableGroupParamEntry();
            newTableGroup.Param = activeParam;
            newTableGroup.Entries = new();

            var newTableEntry = new TableGroupEntry();
            newTableEntry.ID = newID;
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
                    curEntry.ID = newID;
                }
            }
            else
            {
                var newTableEntry = new TableGroupEntry();
                newTableEntry.ID = newID;
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
        CurrentTableGroupIDs.Clear();

        if (IsInTableGroupMode(activeParam))
        {
            var curParam = Project.Handler.ParamData.PrimaryBank.Params[activeParam];

            foreach (var entry in curParam.Rows)
            {
                if (!CurrentTableGroupIDs.Contains(entry.ID))
                {
                    CurrentTableGroupIDs.Add(entry.ID);
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

        CurrentTableGroupID = group;
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
    public bool AllowTableGroupToggle(string activeParam)
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
            var lang = CFG.Current.ParamEditor_Import_Language;
            targetDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Param Table Names", lang);
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
