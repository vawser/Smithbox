using Google.Protobuf.WellKnownTypes;
using HKLib.hk2018.hkAsyncThreadPool;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks.FormatBank;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.GparamEditor.Actions;
using StudioCore.Editors.GparamEditor.Tools;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.GPARAM;
using static StudioCore.Editors.GparamEditor.GparamEditorActions;
using static StudioCore.Editors.GraphicsEditor.GparamParamBank;

namespace StudioCore.GraphicsEditor;

public class GparamEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ActionManager EditorActionManager = new();
    public GparamEditor PropertyEditor;

    public GparamParamBank.GparamInfo _selectedGparamInfo;
    public GPARAM _selectedGparam;
    public string _selectedGparamKey;

    private string _fileSearchInput = "";
    private string _fileSearchInputCache = "";

    public GPARAM.Param _selectedParamGroup;
    public int _selectedParamGroupKey;

    private string _paramGroupSearchInput = "";
    private string _paramGroupSearchInputCache = "";

    public GPARAM.IField _selectedParamField;
    public int _selectedParamFieldKey;

    private string _paramFieldSearchInput = "";
    private string _paramFieldSearchInputCache = "";

    public GPARAM.IFieldValue _selectedFieldValue = null;
    public int _selectedFieldValueKey;

    private string _fieldIdSearchInput = "";
    private string _fieldIdSearchInputCache = "";

    private string _copyFileNewName = "";

    public int _duplicateValueRowId = 0;

    private bool[] displayTruth;

    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;
    public ActionSubMenu ActionSubMenu;

    public GparamQuickEdit QuickEditHandler;

    public GparamEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        PropertyEditor = new GparamEditor(this);
        ToolWindow = new ToolWindow(this);
        ToolSubMenu = new ToolSubMenu(this);
        ActionSubMenu = new ActionSubMenu(this);
        QuickEditHandler = new GparamQuickEdit(this);
    }

    public string EditorName => "Gparam Editor##GparamEditor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";

    public void Init()
    {
        ShowSaveOption = true;
    }

    public void DrawEditorMenu()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Edit"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}", false, EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Undo}");
            if (ImGui.MenuItem("Undo All", "", false, EditorActionManager.CanUndo()))
            {
                EditorActionManager.UndoAllAction();
            }

            UIHelper.ShowMenuIcon($"{ForkAwesome.Repeat}");
            if (ImGui.MenuItem("Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}", false, EditorActionManager.CanRedo()))
            {
                EditorActionManager.RedoAction();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        ActionSubMenu.DisplayMenu();

        ImGui.Separator();

        ToolSubMenu.DisplayMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu("View"))
        {
            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Files"))
            {
                UI.Current.Interface_GparamEditor_Files = !UI.Current.Interface_GparamEditor_Files;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Files);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Groups"))
            {
                UI.Current.Interface_GparamEditor_Groups = !UI.Current.Interface_GparamEditor_Groups;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Groups);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Fields"))
            {
                UI.Current.Interface_GparamEditor_Fields = !UI.Current.Interface_GparamEditor_Fields;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Fields);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Values"))
            {
                UI.Current.Interface_GparamEditor_Values = !UI.Current.Interface_GparamEditor_Values;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_Values);

            UIHelper.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Tool Window"))
            {
                UI.Current.Interface_GparamEditor_ToolConfiguration = !UI.Current.Interface_GparamEditor_ToolConfiguration;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_GparamEditor_ToolConfiguration);

            ImGui.EndMenu();
        }
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_GparamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2S or ProjectType.DS2)
        {
            ImGui.Begin("Editor##InvalidGparamEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else if(Smithbox.ProjectHandler.CurrentProject == null)
        {
            ImGui.Begin("Editor##InvalidGparamEditor");

            ImGui.Text("No project loaded. File -> New Project");

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
                                        QuickEditHandler.targetParamField = entry;
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

            ActionSubMenu.Shortcuts();
            ToolSubMenu.Shortcuts();

            GparamShortcuts();
            QuickEditHandler.Shortcuts();

            if (GparamParamBank.IsLoaded)
            {
                if (UI.Current.Interface_GparamEditor_Files)
                {
                    GparamListView();
                }
                if (UI.Current.Interface_GparamEditor_Groups)
                {
                    GparamGroupList();
                }
                if (UI.Current.Interface_GparamEditor_Fields)
                {
                    GparamFieldList();
                }
                if (UI.Current.Interface_GparamEditor_Values)
                {
                    GparamValueProperties();
                }
            }

            if (UI.Current.Interface_GparamEditor_ToolConfiguration)
            {
                ToolWindow.OnGui(QuickEditHandler);
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void GparamShortcuts()
    {
        // Keyboard shortcuts
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_UndoAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanUndo() && InputTracker.GetKey(KeyBindings.Current.CORE_UndoContinuousAction))
        {
            EditorActionManager.UndoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.CORE_RedoAction))
        {
            EditorActionManager.RedoAction();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKey(KeyBindings.Current.CORE_RedoContinuousAction))
        {
            EditorActionManager.RedoAction();
        }
    }

    public void DeleteValueRow()
    {
        if (_selectedGparam != null && _selectedParamField != null && _selectedFieldValue != null)
        {
            var action = new GparamRemoveValueRow(this, _selectedGparam, _selectedParamField, _selectedFieldValue);
            EditorActionManager.ExecuteAction(action);
        }
    }
    public void DuplicateValueRow()
    {
        if (_selectedParamField != null && _selectedFieldValue != null && _selectedGparam != null)
        {
            var action = new GparamDuplicateValueRow(this, _selectedGparam, _selectedParamField, _selectedFieldValue, _duplicateValueRowId);
            EditorActionManager.ExecuteAction(action);
        }
    }

    private bool SelectGparamFile = false;

    /// <summary>
    /// Files list: selectable files
    /// </summary>
    private void GparamListView()
    {
        // Selection
        void ApplyGparamFileSelection(GparamInfo info)
        {
            ResetGroupSelection();
            ResetFieldSelection();
            ResetValueSelection();

            _selectedGparamKey = info.Name;
            _selectedGparamInfo = info;
            _selectedGparam = info.Gparam;
        }

        ImGui.Begin("Files##GparamFileList");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fileSearchInput, 255);
        UIHelper.ShowHoverTooltip("Separate terms are split via the + character.");

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

                // File row
                if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedGparamKey))
                {
                    ApplyGparamFileSelection(info);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && SelectGparamFile)
                {
                    SelectGparamFile = false;
                    ApplyGparamFileSelection(info);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    SelectGparamFile = true;
                }

                if (CFG.Current.Interface_Display_Alias_for_Gparam)
                {
                    var aliasName = AliasUtils.GetGparamAliasName(info.Name);
                    UIHelper.DisplayAlias(aliasName);
                }

                ImGui.EndGroup();
            }

            GparamFileContextMenu(name, info);
        }

        ImGui.End();
    }

    private bool SelectGparamGroup = false;

    /// <summary>
    /// Groups list: selectable groups
    /// </summary>
    public void GparamGroupList()
    {
        // Selection
        void ApplyGparamGroupSelection(int index, GPARAM.Param entry)
        {
            ResetFieldSelection();
            ResetValueSelection();

            _selectedParamGroup = entry;
            _selectedParamGroupKey = index;
        }

        ImGui.Begin("Groups##GparamGroups");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _paramGroupSearchInput, 255);
        UIHelper.ShowHoverTooltip("Separate terms are split via the + character.");

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

                var name = entry.Key;
                if(CFG.Current.Gparam_DisplayParamGroupAlias)
                    name = Smithbox.BankHandler.GPARAM_Info.GetReferenceName(entry.Key, entry.Name);

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
                        // Group row
                        if (ImGui.Selectable($@" {name}##{entry.Key}", i == _selectedParamGroupKey))
                        {
                            ApplyGparamGroupSelection(i, entry);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && SelectGparamGroup)
                        {
                            SelectGparamGroup = false;
                            ApplyGparamGroupSelection(i, entry);
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            SelectGparamGroup = true;
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
        foreach (var entry in Smithbox.BankHandler.GPARAM_Info.Information.list)
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

    private bool SelectGparamField = false;

    /// <summary>
    /// Fields List: selectable fields
    /// </summary>
    public void GparamFieldList()
    {
        // Selection
        void ApplyGparamFieldSelection(int index, GPARAM.IField entry)
        {
            ResetValueSelection();

            _selectedParamField = entry;
            QuickEditHandler.targetParamField = entry;
            _selectedParamFieldKey = index;
        }

        ImGui.Begin("Fields##GparamFields");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _paramFieldSearchInput, 255);
        UIHelper.ShowHoverTooltip("Separate terms are split via the + character.");

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

                var name = entry.Key;
                if (CFG.Current.Gparam_DisplayParamFieldAlias)
                    name = Smithbox.BankHandler.GPARAM_Info.GetReferenceName(entry.Key, entry.Name);

                if (SearchFilters.IsEditorSearchMatch(_paramFieldSearchInput, entry.Name, " "))
                {
                    // Field row
                    if (ImGui.Selectable($@" {name}##{entry.Key}{i}", i == _selectedParamFieldKey))
                    {
                        ApplyGparamFieldSelection(i, entry);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && SelectGparamField)
                    {
                        SelectGparamField = false;
                        ApplyGparamFieldSelection(i, entry);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        SelectGparamField = true;
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
        foreach(var entry in Smithbox.BankHandler.GPARAM_Info.Information.list)
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
        UIHelper.ShowHoverTooltip("Separate terms are split via the + character.");

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
                    PropertyEditor.AddValueField(field);
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
    /// REduce the Values display truth list in preparation for value row removal.
    /// </summary>
    /// <param name="field"></param>
    public void ReduceDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count + -1];

        for (int i = 0; i < field.Values.Count + -1; i++)
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
        PropertyEditor.TimeOfDayField(index, field, value, _selectedGparamInfo);
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
        PropertyEditor.ValueField(index, field, value,
        _selectedGparamInfo);
    }

    /// <summary>
    /// Values table: Information column
    /// </summary>
    /// <param name="field"></param>
    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        string desc = Smithbox.BankHandler.GPARAM_Info.GetReferenceDescription(_selectedParamGroup.Key, _selectedParamField.Key);

        UIHelper.WrappedText($"Type: {GetHumanTypeName(field)}");
        UIHelper.WrappedText($"");

        // Skip if empty
        if (desc != "")
        {
            UIHelper.WrappedText($"{desc}");
        }

        // Show enum list if they exist
        var propertyEnum = Smithbox.BankHandler.GPARAM_Info.GetEnumForProperty(field.Key);
        if (propertyEnum != null)
        {
            foreach (var entry in propertyEnum.members)
            {
                UIHelper.WrappedText($"{entry.id} - {entry.name}");
            }
        }
    }

    private string GetHumanTypeName(IField field)
    {
        string typeName = "Unknown";

        if(field is GPARAM.IntField)
        {
            typeName = "Signed Integer";
        }
        if (field is GPARAM.UintField)
        {
            typeName = "Unsigned Integer";
        }
        if (field is GPARAM.ShortField)
        {
            typeName = "Signed Short";
        }
        if (field is GPARAM.SbyteField)
        {
            typeName = "Signed Byte";
        }
        if (field is GPARAM.ByteField)
        {
            typeName = "Byte";
        }
        if (field is GPARAM.FloatField)
        {
            typeName = "Float";
        }
        if (field is GPARAM.Vector2Field)
        {
            typeName = "Vector2";
        }
        if (field is GPARAM.Vector3Field)
        {
            typeName = "Vector3";
        }
        if (field is GPARAM.Vector4Field)
        {
            typeName = "Vector4";
        }
        if (field is GPARAM.BoolField)
        {
            typeName = "Boolean";
        }
        if (field is GPARAM.ColorField)
        {
            typeName = "Color";
        }

        return typeName;
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
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    QuickEditHandler.UpdateFileFilter(name);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Add this file to the File Filter in the Quick Edit window.");

                // Only show if the file exists in the project directory
                if (info.Path.Contains(Smithbox.ProjectRoot))
                {
                    if (ImGui.Selectable("Remove"))
                    {
                        RemoveGparamFile(info);

                        ImGui.CloseCurrentPopup();
                    }
                    UIHelper.ShowHoverTooltip("Delete the selected file from your project.");
                }

                if (ImGui.Selectable("Duplicate"))
                {
                    DuplicateGparamFile();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Duplicate this file, incrementing the numeric four digit ID at the end of the file name if possible.");

                if (ImGui.Selectable("Copy"))
                {
                    CopyGparamFile(info);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Copy the selected file and rename it to the name specified below");

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
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    QuickEditHandler.UpdateGroupFilter(_selectedParamGroup.Key);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Add this group to the Group Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    _selectedGparam.Params.Remove(_selectedParamGroup);
                    _selectedGparamInfo.WasModified = true;

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Delete the selected group.");

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
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    QuickEditHandler.UpdateFieldFilter(_selectedParamField.Key);

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Add this field to the Field Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    _selectedParamGroup.Fields.Remove(_selectedParamField);
                    _selectedGparamInfo.WasModified = true;

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Delete the selected row.");

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
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    var fieldIndex = -1;
                    for (int i = 0; i < _selectedParamField.Values.Count; i++)
                    {
                        if (_selectedParamField.Values[i] == _selectedFieldValue)
                        {
                            fieldIndex = i;
                            break;
                        }
                    }

                    if(fieldIndex != -1)
                    {
                        QuickEditHandler.UpdateValueRowFilter(fieldIndex);
                    }

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Add this field to the Field Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    DeleteValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Delete the value row.");

                if (ImGui.Selectable("Duplicate"))
                {
                    DuplicateValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.ShowHoverTooltip("Duplicate the selected value row, assigning the specified ID below as the new id.");

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
        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ToolWindow.OnProjectChanged();
            ToolSubMenu.OnProjectChanged();
            ActionSubMenu.OnProjectChanged();
        }

        GparamParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParam(_selectedGparamInfo);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParams();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
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

        filePath = filePath.Replace($"{Smithbox.GameRoot}", $"{Smithbox.ProjectRoot}");

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
        newFilePath = newFilePath.Replace($"{Smithbox.GameRoot}", $"{Smithbox.ProjectRoot}");

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
            newFilePath = newFilePath.Replace($"{Smithbox.GameRoot}", $"{Smithbox.ProjectRoot}");

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
        var typeName = Smithbox.BankHandler.GPARAM_Info.GetTypeForProperty(missingField.id);

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
