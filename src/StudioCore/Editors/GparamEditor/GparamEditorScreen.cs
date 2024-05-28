using DotNext.Collections.Generic;
using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks;
using StudioCore.Banks.FormatBank;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.GparamEditor.Toolbar;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsEditor;

public class GparamEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public static ActionManager EditorActionManager = new();

    private GparamParamBank.GparamInfo _selectedGparamInfo;
    private GPARAM _selectedGparam;
    private string _selectedGparamKey;

    private string _fileSearchInput = "";
    private string _fileSearchInputCache = "";

    private GPARAM.Param _selectedParamGroup;
    private int _selectedParamGroupKey;

    private string _paramGroupSearchInput = "";
    private string _paramGroupSearchInputCache = "";

    private GPARAM.IField _selectedParamField;
    private int _selectedParamFieldKey;

    private string _paramFieldSearchInput = "";
    private string _paramFieldSearchInputCache = "";

    private GPARAM.IFieldValue _selectedFieldValue = null;
    private int _selectedFieldValueKey;

    private string _fieldIdSearchInput = "";
    private string _fieldIdSearchInputCache = "";

    private string _copyFileNewName = "";

    private int _duplicateValueRowId = 0;

    private bool[] displayTruth;

    public GparamToolbar _gparamToolbar;
    public GparamToolbar_ActionList _gparamToolbar_ActionList;
    public GparamToolbar_Configuration _gparamToolbar_Configuration;

    public GparamEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _gparamToolbar = new GparamToolbar(EditorActionManager);
        _gparamToolbar_ActionList = new GparamToolbar_ActionList();
        _gparamToolbar_Configuration = new GparamToolbar_Configuration();
    }

    public string EditorName => "Gparam Editor##GparamEditor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";

    public void Init()
    {
        
    }

    public void DrawEditorMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo", KeyBindings.Current.Core_Undo.HintText, false, EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false, EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", KeyBindings.Current.Core_Redo.HintText, false, EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("View"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Files"))
            {
                CFG.Current.Interface_GparamEditor_Files = !CFG.Current.Interface_GparamEditor_Files;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Files);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Groups"))
            {
                CFG.Current.Interface_GparamEditor_Groups = !CFG.Current.Interface_GparamEditor_Groups;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Groups);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Fields"))
            {
                CFG.Current.Interface_GparamEditor_Fields = !CFG.Current.Interface_GparamEditor_Fields;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Fields);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Values"))
            {
                CFG.Current.Interface_GparamEditor_Values = !CFG.Current.Interface_GparamEditor_Values;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Values);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Toolbar"))
            {
                CFG.Current.Interface_GparamEditor_Toolbar = !CFG.Current.Interface_GparamEditor_Toolbar;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_GparamEditor_Toolbar);

            ImGui.EndMenu();
        }
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_GparamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Project.Type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S or ProjectType.DS2)
        {
            ImGui.Begin("Editor##InvalidGparamEditor");

            ImGui.Text($"This editor does not support {Project.Type}.");

            ImGui.End();
        }
        else
        {
            if (!GparamParamBank.IsLoaded)
            {
                GparamParamBank.LoadGraphicsParams();
            }

            // Commands
            if (initcmd != null && initcmd.Length > 1)
            {
                // View Image:
                // e.g. "gparam/view/m00_00_0000/LightSet ParamEditor/Directional Light DiffColor0/100"
                if (initcmd[0] == "view" && initcmd.Length >= 2)
                {
                    // Gparam
                    foreach (var (name, info) in GparamParamBank.ParamBank)
                    {
                        TaskLogs.AddLog($"{name}");
                        if (initcmd[1] == name)
                        {
                            _selectedGparamKey = info.Name;
                            _selectedGparamInfo = info;
                            _selectedGparam = info.Gparam;
                        }
                    }

                    // Param Group
                    if(initcmd.Length >= 3)
                    {
                        if (_selectedGparam != null && _selectedGparamKey != "")
                        {
                            GPARAM data = _selectedGparam;

                            for (int i = 0; i < data.Params.Count; i++)
                            {
                                GPARAM.Param entry = data.Params[i];

                                if (initcmd[2] == entry.Key)
                                {
                                    _selectedParamGroup = entry;
                                    _selectedParamGroupKey = i;
                                }
                            }
                        }

                        // Fields
                        if(initcmd.Length >= 4)
                        {
                            if (_selectedParamGroup != null && _selectedParamGroupKey != -1)
                            {
                                GPARAM.Param data = _selectedParamGroup;

                                for (int i = 0; i < data.Fields.Count; i++)
                                {
                                    GPARAM.IField entry = data.Fields[i];

                                    if (initcmd[3] == entry.Key)
                                    {
                                        _selectedParamField = entry;
                                        GparamQuickEdit.SelectedParamField = entry;
                                        _selectedParamFieldKey = i;
                                    }
                                }
                            }

                            // Field Row
                            if(initcmd.Length >= 5)
                            {
                                if (_selectedParamField != null && _selectedParamFieldKey != -1)
                                {
                                    GPARAM.IField field = _selectedParamField;

                                    for (int i = 0; i < field.Values.Count; i++)
                                    {
                                        GPARAM.IFieldValue entry = field.Values[i];

                                        if (initcmd[4] == entry.Id.ToString())
                                        {
                                            _selectedFieldValue = entry;
                                            _selectedFieldValueKey = i;
                                            _duplicateValueRowId = i;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            GparamShortcuts();

            if (GparamParamBank.IsLoaded)
            {
                if (CFG.Current.Interface_GparamEditor_Files)
                {
                    GparamListView();
                }
                if (CFG.Current.Interface_GparamEditor_Groups)
                {
                    GparamGroupList();
                }
                if (CFG.Current.Interface_GparamEditor_Fields)
                {
                    GparamFieldList();
                }
                if (CFG.Current.Interface_GparamEditor_Values)
                {
                    GparamValueProperties();
                }
            }

            if (CFG.Current.Interface_GparamEditor_Toolbar)
            {
                _gparamToolbar_ActionList.OnGui();
                _gparamToolbar_Configuration.OnGui();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void GparamShortcuts()
    {
        // Keyboard shortcuts
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
        {
            EditorActionManager.RedoAction();
        }
    }

    /// <summary>
    /// Files list: selectable files
    /// </summary>
    private void GparamListView()
    {
        ImGui.Begin("Files##GparamFileList");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fileSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_fileSearchInput != _fileSearchInputCache)
        {
            _fileSearchInputCache = _fileSearchInput;
        }

        foreach (var (name, info) in GparamParamBank.ParamBank)
        {
            if (SearchFilters.IsEditorSearchMatch(_fileSearchInput, info.Name, "_"))
            {
                ImGui.BeginGroup();
                if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedGparamKey))
                {
                    ResetGroupSelection();
                    ResetFieldSelection();
                    ResetValueSelection();

                    _selectedGparamKey = info.Name;
                    _selectedGparamInfo = info;
                    _selectedGparam = info.Gparam;
                }

                if (CFG.Current.Interface_Display_Alias_for_Gparam)
                {
                    if (GparamAliasBank.Bank.AliasNames != null)
                    {
                        var prettyName = "";

                        var entries = GparamAliasBank.Bank.AliasNames.GetEntries("Gparams");
                        foreach (var entry in entries)
                        {
                            if (name == entry.id)
                            {
                                prettyName = entry.name;
                                break;
                            }
                        }

                        if (prettyName != "")
                        {
                            ImGui.SameLine();
                            ImGui.PushTextWrapPos();

                            ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"<{prettyName}>");

                            ImGui.PopTextWrapPos();
                        }
                    }
                }

                ImGui.EndGroup();
            }

            GparamFileContextMenu(name, info);
        }

        ImGui.End();
    }

    /// <summary>
    /// Groups list: selectable groups
    /// </summary>
    public void GparamGroupList()
    {
        ImGui.Begin("Groups##GparamGroups");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _paramGroupSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_paramGroupSearchInput != _paramGroupSearchInputCache)
        {
            _paramGroupSearchInputCache = _paramGroupSearchInput;
        }

        if (_selectedGparam != null && _selectedGparamKey != "")
        {
            GPARAM data = _selectedGparam;

            ImGui.Text($"Group");
            ImGui.Separator();

            // Available groups
            for (int i = 0; i < data.Params.Count; i++)
            {
                GPARAM.Param entry = data.Params[i];

                var name = GparamFormatBank.Bank.GetReferenceName(entry.Key, entry.Name);

                var display = false;

                if (!CFG.Current.Gparam_DisplayEmptyGroups)
                {
                    foreach(var fieldEntry in entry.Fields)
                    {
                        if(fieldEntry.Values.Count > 0)
                        {
                            display = true;
                        }
                    }
                }
                else
                {
                    display = true;
                }

                if (SearchFilters.IsEditorSearchMatch(_paramGroupSearchInput, entry.Name, " "))
                {
                    if (display)
                    {
                        if (ImGui.Selectable($@" {name}##{entry.Key}", i == _selectedParamGroupKey))
                        {
                            ResetFieldSelection();
                            ResetValueSelection();

                            _selectedParamGroup = entry;
                            _selectedParamGroupKey = i;
                        }
                    }
                }

                ShowGparamGroupContext(i);
            }

            if (CFG.Current.Gparam_DisplayAddGroups)
            {
                ImGui.Separator();

                GparamGroupAddSection();
            }
        }

        ImGui.End();
    }

    /// <summary>
    /// Groups List: add buttons
    /// </summary>
    public void GparamGroupAddSection()
    {
        GPARAM data = _selectedGparam;

        List<FormatReference> missingGroups = new List<FormatReference>();

        // Get source Format Reference
        foreach (var entry in GparamFormatBank.Bank.Entries.list)
        {
            bool isPresent = false;

            foreach (var param in data.Params)
            {
                if (entry.id == param.Key)
                {
                    isPresent = true;
                }
            }

            if (!isPresent)
            {
                missingGroups.Add(entry);
            }
        }

        foreach (var missing in missingGroups)
        {
            if (ImGui.Button($"Add##{missing.id}"))
            {
                AddMissingGroup(missing);
                _selectedGparamInfo.WasModified = true;
            }
            ImGui.SameLine();
            ImGui.Text($"{missing.name}");
        }
    }

    /// <summary>
    /// Fields List: selectable fields
    /// </summary>
    public void GparamFieldList()
    {
        ImGui.Begin("Fields##GparamFields");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _paramFieldSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_paramFieldSearchInput != _paramFieldSearchInputCache)
        {
            _paramFieldSearchInputCache = _paramFieldSearchInput;
        }

        if (_selectedParamGroup != null && _selectedParamGroupKey != -1)
        {
            GPARAM.Param data = _selectedParamGroup;

            ImGui.Text($"Field");
            ImGui.Separator();

            for (int i = 0; i < data.Fields.Count; i++)
            {
                GPARAM.IField entry = data.Fields[i];

                var name = GparamFormatBank.Bank.GetReferenceName(entry.Key, entry.Name);

                if (SearchFilters.IsEditorSearchMatch(_paramFieldSearchInput, entry.Name, " "))
                {
                    if (ImGui.Selectable($@" {name}##{entry.Key}{i}", i == _selectedParamFieldKey))
                    {
                        ResetValueSelection();

                        _selectedParamField = entry;
                        GparamQuickEdit.SelectedParamField = entry;
                        _selectedParamFieldKey = i;
                    }
                }

                ShowGparamFieldContext(i);
            }

            if (CFG.Current.Gparam_DisplayAddFields)
            {
                ImGui.Separator();

                GparamFieldAddSection();
            }
        }

        ImGui.End();
    }

    /// <summary>
    /// Fields list: Add buttons
    /// </summary>
    public void GparamFieldAddSection()
    {
        GPARAM.Param data = _selectedParamGroup;

        List<FormatMember> missingFields = new List<FormatMember>();

        // Get source Format Reference
        foreach(var entry in GparamFormatBank.Bank.Entries.list)
        {
            if (entry.id == _selectedParamGroup.Key)
            {
                foreach (var member in entry.members)
                {
                    bool isPresent = false;

                    foreach (var pField in data.Fields)
                    {
                        if (member.id == pField.Key)
                        {
                            isPresent = true;
                        }
                    }

                    if (!isPresent)
                    {
                        missingFields.Add(member);
                    }
                }
            }
        }

        foreach (var missing in missingFields)
        {
            // Unknown should be skipped
            if (missing.id != "Unknown")
            {
                if (ImGui.Button($"Add##{missing.id}"))
                {
                    AddMissingField(_selectedParamGroup, missing);
                    _selectedGparamInfo.WasModified = true;
                }
                ImGui.SameLine();
                ImGui.Text($"{missing.name}");
            }
        }
    }

    /// <summary>
    /// Values table
    /// </summary>
    private void GparamValueProperties()
    {
        ImGui.Begin("Values##GparamValues");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fieldIdSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        GparamQuickEdit.OnGui();

        ImGui.Separator();

        if (_fieldIdSearchInput != _fieldIdSearchInputCache)
        {
            _fieldIdSearchInputCache = _fieldIdSearchInput;
        }

        if (_selectedParamField != null && _selectedParamFieldKey != -1)
        {
            GPARAM.IField field = _selectedParamField;

            ResetDisplayTruth(field);

            ImGui.Columns(4);

            // ID
            ImGui.BeginChild("IdList##GparamPropertyIds");
            ImGui.Text($"ID");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                GPARAM.IFieldValue entry = field.Values[i];

                displayTruth[i] = SearchFilters.IsIdSearchMatch(_fieldIdSearchInput, entry.Id.ToString());

                if (displayTruth[i])
                {
                    GparamProperty_ID(i, field, entry);
                }
            }

            // Display "Add" button if field has no value rows.
            if(field.Values.Count <= 0)
            {
                if (ImGui.Button("Add"))
                {
                    GparamEditor.AddValueField(field);
                    ResetDisplayTruth(field);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Time of Day
            ImGui.BeginChild("IdList##GparamTimeOfDay");
            ImGui.Text($"Time of Day");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_TimeOfDay(i, field, entry);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("ValueList##GparamPropertyValues");
            ImGui.Text($"Value");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_Value(i, field, entry);
                }
            }

            ImGui.EndChild();

            // Information
            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("InfoList##GparamPropertyInfo");
            ImGui.Text($"Information");
            ImGui.Separator();

            // Only show once
            GparamProperty_Info(field);

            ImGui.EndChild();
        }

        ImGui.End();
    }

    /// <summary>
    /// Reset the Values display truth list
    /// </summary>
    /// <param name="field"></param>
    public void ResetDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count];

        for (int i = 0; i < field.Values.Count; i++)
        {
            displayTruth[i] = true;
        }
    }

    /// <summary>
    /// Extend the Values display truth list in preparation for value row addition.
    /// </summary>
    /// <param name="field"></param>
    public void ExtendDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count + 1];

        for (int i = 0; i < field.Values.Count + 1; i++)
        {
            displayTruth[i] = true;
        }
    }

    /// <summary>
    /// Values table: ID column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_ID(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();

        string name = value.Id.ToString();

        if (ImGui.Selectable($"{name}##{index}", index == _selectedFieldValueKey))
        {
            _selectedFieldValue = value;
            _selectedFieldValueKey = index;
            _duplicateValueRowId = value.Id;
        }

        DisplayPropertyIdContext(index);
    }

    /// <summary>
    /// Values table: Time of Day column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_TimeOfDay(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        GparamEditor.TimeOfDayField(index, field, value, _selectedGparamInfo);
    }

    /// <summary>
    /// Values table: Value column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_Value(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        GparamEditor.ValueField(index, field, value,
        _selectedGparamInfo);
    }

    /// <summary>
    /// Values table: Information column
    /// </summary>
    /// <param name="field"></param>
    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        string desc = GparamFormatBank.Bank.GetReferenceDescription(_selectedParamField.Key);

        // Skip if empty
        if (desc != "")
        {
            ImGui.Text($"{desc}");
        }

        // Show enum list if they exist
        var propertyEnum = GparamFormatBank.Bank.GetEnumForProperty(field.Key);
        if (propertyEnum != null)
        {
            foreach (var entry in propertyEnum.members)
            {
                ImGui.Text($"{entry.id} - {entry.name}");
            }
        }
    }

    /// <summary>
    /// Context menu for File list
    /// </summary>
    /// <param name="name"></param>
    /// <param name="info"></param>
    public void GparamFileContextMenu(string name, GparamParamBank.GparamInfo info)
    {
        if (info.Name == _selectedGparamKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
            {
                // Only show if the file exists in the project directory
                if (info.Path.Contains(Project.GameModDirectory))
                {
                    if (ImGui.Selectable("Remove"))
                    {
                        RemoveGparamFile(info);

                        ImGui.CloseCurrentPopup();
                    }
                    ImguiUtils.ShowHoverTooltip("Delete the selected file from your project.");
                }

                if (ImGui.Selectable("Duplicate"))
                {
                    DuplicateGparamFile();

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate this file, incrementing the numeric four digit ID at the end of the file name if possible.");

                if (ImGui.Selectable("Copy"))
                {
                    CopyGparamFile(info);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowHoverTooltip("Copy the selected file and rename it to the name specified below");

                ImGui.Separator();

                // Copy
                if (_copyFileNewName == "")
                    _copyFileNewName = name;

                ImGui.InputText("##copyInputName", ref _copyFileNewName, 255);

                ImGui.EndPopup();
            }
        }
    }


    /// <summary>
    /// Context menu for Groups list
    /// </summary>
    /// <param name="index"></param>
    public void ShowGparamGroupContext(int index)
    {
        if (index == _selectedParamGroupKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Group_Context"))
            {
                if (ImGui.Selectable("Remove"))
                {
                    _selectedGparam.Params.Remove(_selectedParamGroup);
                    _selectedGparamInfo.WasModified = true;

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowHoverTooltip("Delete the selected group.");

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Context menu for Fields list
    /// </summary>
    /// <param name="index"></param>
    public void ShowGparamFieldContext(int index)
    {
        if (index == _selectedParamFieldKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Field_Context"))
            {
                if (ImGui.Selectable("Remove"))
                {
                    _selectedParamGroup.Fields.Remove(_selectedParamField);
                    _selectedGparamInfo.WasModified = true;

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowHoverTooltip("Delete the selected row.");

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Context menu for Values table, ID column
    /// </summary>
    /// <param name="index"></param>
    public void DisplayPropertyIdContext(int index)
    {
        if (index == _selectedFieldValueKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_PropId_Context"))
            {
                if (ImGui.Selectable("Remove"))
                {
                    GparamEditor.RemovePropertyValueRow(_selectedParamField, _selectedFieldValue);
                    _selectedGparamInfo.WasModified = true;

                    // Update the group index lists to account for the removed ID.
                    GparamEditor.UpdateGroupIndexes(_selectedGparam);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowHoverTooltip("Delete the value row.");

                if (ImGui.Selectable("Duplicate"))
                {
                    ExtendDisplayTruth(_selectedParamField);
                    GparamEditor.AddPropertyValueRow(_selectedParamField, _selectedFieldValue, _duplicateValueRowId);
                    _selectedGparamInfo.WasModified = true;

                    // Update the group index lists to account for the new ID.
                    GparamEditor.UpdateGroupIndexes(_selectedGparam);

                    ImGui.CloseCurrentPopup();
                }
                ImguiUtils.ShowHoverTooltip("Duplicate the selected value row, assigning the specified ID below as the new id.");

                ImGui.InputInt("##valueIdInput", ref _duplicateValueRowId);

                if (_duplicateValueRowId < 0)
                {
                    _duplicateValueRowId = 0;
                }

                ImGui.EndPopup();
            }
        }
    }


    public void OnProjectChanged()
    {
        GparamParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParam(_selectedGparamInfo);
    }

    public void SaveAll()
    {
        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParams();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }


    private void ResetAllSelection()
    {
        ResetFileSelection();
        ResetGroupSelection();
        ResetFieldSelection();
        ResetValueSelection();
    }

    private void ResetFileSelection()
    {
        _selectedGparam = null;
        _selectedGparamKey = "";
    }

    private void ResetGroupSelection()
    {
        _selectedParamGroup = null;
        _selectedParamGroupKey = -1;
    }

    private void ResetFieldSelection()
    {
        _selectedParamField = null;
        _selectedParamFieldKey = -1;
    }

    private void ResetValueSelection()
    {
        _selectedFieldValue = null;
        _selectedFieldValueKey = -1;
    }

    /// <summary>
    /// Remove target GPARAM file from project
    /// </summary>
    /// <param name="info"></param>
    public void RemoveGparamFile(GparamParamBank.GparamInfo info)
    {
        string filePath = info.Path;
        string baseFileName = info.Name;

        filePath = filePath.Replace($"{Project.GameRootDirectory}", $"{Project.GameModDirectory}");

        if (File.Exists(filePath))
        {
            TaskLogs.AddLog($"{baseFileName} was removed from your project.");
            File.Delete(filePath);
        }
        else
        {
            TaskLogs.AddLog($"{baseFileName} does not exist within your project.");
        }

        GparamParamBank.LoadGraphicsParams();
    }

    /// <summary>
    /// Copy and rename target GPARAM file
    /// </summary>
    /// <param name="info"></param>
    public void CopyGparamFile(GparamParamBank.GparamInfo info)
    {
        string filePath = info.Path;
        string baseFileName = info.Name;
        string tryFileName = _copyFileNewName;

        string newFilePath = filePath.Replace(baseFileName, tryFileName);

        // If the original is in the root dir, change the path to mod
        newFilePath = newFilePath.Replace($"{Project.GameRootDirectory}", $"{Project.GameModDirectory}");

        if (!File.Exists(newFilePath))
        {
            File.Copy(filePath, newFilePath);
        }
        else
        {
            TaskLogs.AddLog($"{newFilePath} already exists!");
        }

        GparamParamBank.LoadGraphicsParams();
    }

    /// <summary>
    /// Duplicate target GPARAM file, increment sub ID if valid
    /// </summary>
    public void DuplicateGparamFile()
    {
        bool isValidFile = false;
        string filePath = _selectedGparamInfo.Path;
        string baseFileName = _selectedGparamInfo.Name;
        string tryFileName = _selectedGparamInfo.Name;

        do
        {
            string currentfileName = CreateDuplicateFileName(tryFileName);
            string newFilePath = filePath.Replace(baseFileName, currentfileName);

            // If the original is in the root dir, change the path to mod
            newFilePath = newFilePath.Replace($"{Project.GameRootDirectory}", $"{Project.GameModDirectory}");

            if (!File.Exists(newFilePath))
            {
                File.Copy(filePath, newFilePath);
                isValidFile = true;
            }
            else
            {
                TaskLogs.AddLog($"{newFilePath} already exists!");
                tryFileName = currentfileName;
            }
        }
        while (!isValidFile);

        GparamParamBank.LoadGraphicsParams();
    }

    /// <summary>
    /// Search for valid duplicate name for a GPARAM file
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string CreateDuplicateFileName(string fileName)
    {
        Match mapMatch = Regex.Match(fileName, @"[0-9]{4}");

        if(mapMatch.Success)
        {
            var res = mapMatch.Groups[0].Value;

            int slot = 0;
            string slotStr = "";

            try
            {
                int number;
                int.TryParse(res, out number);

                slot = number + 1;
            }
            catch { }

            if(slot >= 100 && slot < 999)
            {
                slotStr = "0";
            }
            if (slot >= 10 && slot < 99)
            {
                slotStr = "00";
            }
            if (slot >= 0 && slot < 9)
            {
                slotStr = "000";
            }

            var finalSlotStr = $"{slotStr}{slot}";
            var final = fileName.Replace(res, finalSlotStr);

            return final;
        }
        else
        {
            Match dupeMatch = Regex.Match(fileName, @"__[0-9]{1}");

            if (dupeMatch.Success)
            {
                var res = dupeMatch.Groups[0].Value;

                Match numMatch = Regex.Match(res, @"[0-9]{1}");

                var num = numMatch.Groups[0].Value;
                try
                {
                    int number;
                    int.TryParse(res, out number);

                    number = number + 1;

                    return $"{fileName}__{number}";
                }
                catch 
                {
                    return $"{fileName}__1";
                }
            }
            else
            {
                return $"{fileName}__1";
            }
        }
    }

    /// <summary>
    /// Add missing param group to target GPARAM
    /// </summary>
    /// <param name="missingGroup"></param>
    public void AddMissingGroup(FormatReference missingGroup)
    {
        var newGroup = new GPARAM.Param();
        newGroup.Key = missingGroup.id;
        newGroup.Name = missingGroup.name;
        newGroup.Fields = new List<GPARAM.IField>();

        _selectedGparam.Params.Add(newGroup);
    }

    /// <summary>
    /// Add missing field to target Param Group
    /// </summary>
    /// <param name="targetParam"></param>
    /// <param name="missingField"></param>
    public void AddMissingField(Param targetParam, FormatMember missingField)
    {
        var typeName = GparamFormatBank.Bank.GetTypeForProperty(missingField.id);

        if (typeName == "Byte")
        {
            GPARAM.ByteField newField = new GPARAM.ByteField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<byte>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<byte>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Short")
        {
            GPARAM.ShortField newField = new GPARAM.ShortField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<short>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<short>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "IntA" || typeName == "IntB")
        {
            GPARAM.IntField newField = new GPARAM.IntField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<int>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<int>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float")
        {
            GPARAM.FloatField newField = new GPARAM.FloatField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<float>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<float>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "BoolA" || typeName == "BoolB")
        {
            GPARAM.BoolField newField = new GPARAM.BoolField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<bool>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = false;

            newField.Values = new List<FieldValue<bool>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float2")
        {
            GPARAM.Vector2Field newField = new GPARAM.Vector2Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector2>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector2(0f, 0f);

            newField.Values = new List<FieldValue<Vector2>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float3")
        {
            GPARAM.Vector3Field newField = new GPARAM.Vector3Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector3>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector3(0f, 0f, 0f);

            newField.Values = new List<FieldValue<Vector3>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float4")
        {
            GPARAM.Vector4Field newField = new GPARAM.Vector4Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector4>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector4(0f, 0f, 0f, 0f);

            newField.Values = new List<FieldValue<Vector4>> { valueList };

            targetParam.Fields.Add(newField);
        }

        // Unknown
    }
}
