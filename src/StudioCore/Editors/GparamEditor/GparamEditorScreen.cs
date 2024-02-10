using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.UserProject;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsEditor;

public class GparamEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    private ActionManager EditorActionManager = new();

    private GparamParamBank.GparamInfo _selectedGparamInfo;
    private GPARAM _selectedGparam;
    private string _selectedGparamKey;

    private GPARAM.Param _selectedParamGroup;
    private int _selectedParamGroupKey;

    private GPARAM.IField _selectedParamField;
    private int _selectedParamFieldKey;

    private GPARAM.IFieldValue _selectedFieldValue = null;
    private int _selectedFieldValueKey;

    public GparamEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        ResetAllSelection();

        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Gparam Editor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Gparam";

    public void DrawEditorMenu()
    {
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Keyboard shortcuts
        if (EditorActionManager.CanUndo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Undo))
        {
            ParamUndo();
        }

        if (EditorActionManager.CanRedo() && InputTracker.GetKeyDown(KeyBindings.Current.Core_Redo))
        {
            ParamRedo();
        }

        // Docking setup
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        if (Project.Type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S)
        {
            ImGui.Text($"This editor does not support {Project.Type}.");
            ImGui.PopStyleVar();
            return;
        }
        else if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

        if (!GparamParamBank.IsLoaded)
        {
            if (!CFG.Current.AutoLoadBank_Gparam)
            {
                if (ImGui.Button("Load Gparam Editor"))
                {
                    GparamParamBank.LoadGraphicsParams();
                }
            }
        }

        var dsid = ImGui.GetID("DockSpace_GparamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (GparamParamBank.IsLoaded)
        {
            GraphicsParamView();
        }

        ImGui.PopStyleVar();
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

    private void GraphicsParamView()
    {
        // GPARAM List
        ImGui.Begin("Files##GparamFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, param) in GparamParamBank.ParamBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedGparamKey))
            {
                ResetGroupSelection();
                ResetFieldSelection();
                ResetValueSelection();

                _selectedGparamKey = info.Name;
                _selectedGparamInfo = info;
                _selectedGparam = param;
            }

            // Context menu action for duplicating exist to new name
            if (info.Name == _selectedGparamKey)
            {
                if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
                {
                    if (ImGui.Button("Duplicate"))
                    {
                        ImGui.CloseCurrentPopup();
                    }
                    // If it is a mod-local added file, then it may be deleted
                    if (ImGui.Button("Delete") && info.Added)
                    {
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }
            }
        }

        ImGui.End();

        // GPARAM Groups
        ImGui.Begin("Groups##GparamGroups");

        if (_selectedGparam != null && _selectedGparamKey != "")
        {
            GPARAM data = _selectedGparam;

            ImGui.Text($"Group");
            ImGui.Separator();

            for (int i = 0; i < data.Params.Count; i++)
            {
                GPARAM.Param entry = data.Params[i];

                if (ImGui.Selectable($@" {entry.Name}##{entry.Key}", i == _selectedParamGroupKey))
                {
                    ResetFieldSelection();
                    ResetValueSelection();

                    _selectedParamGroup = entry;
                    _selectedParamGroupKey = i;
                }
            }
        }

        ImGui.End();

        // GPARAM Fields
        ImGui.Begin("Fields##GparamFields");

        if (_selectedParamGroup != null && _selectedParamGroupKey != -1)
        {
            GPARAM.Param data = _selectedParamGroup;

            ImGui.Text($"Field");
            ImGui.Separator();

            for (int i = 0; i < data.Fields.Count; i++)
            {
                GPARAM.IField entry = data.Fields[i];

                if (ImGui.Selectable($@" {entry.Name}##{entry.Key}", i == _selectedParamFieldKey))
                {
                    ResetValueSelection();

                    _selectedParamField = entry;
                    _selectedParamFieldKey = i;
                }
            }
        }

        ImGui.End();

        // GPARAM Values
        ImGui.Begin("Values##GparamValues");

        if (_selectedParamField != null && _selectedParamFieldKey != -1)
        {
            GraphicsParamPropertyView();
        }

        ImGui.End();
    }

    private void GraphicsParamPropertyView()
    {
        GPARAM.IField field = _selectedParamField;

        if (Project.Type == ProjectType.SDT)
        {
            ImGui.Columns(2); // 3 if the floats are shown
        }
        else
        {
            ImGui.Columns(2);
        }

        // ID
        ImGui.BeginChild("IdList##GparamPropertyIds");
        ImGui.Text($"ID");
        ImGui.Separator();

        for (int i = 0; i < field.Values.Count; i++)
        {
            GPARAM.IFieldValue entry = field.Values[i];
            GraphicsParamIdView(i, entry);
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        // Never used?
        // Unk04 (Sekiro)
        /*
        if (Project.Type == ProjectType.SDT)
        {
            ImGui.BeginChild("UnkList");
            ImGui.Text($"Floats");
            ImGui.Separator();
            foreach (var val in field.Values)
            {
                ImGui.Text($"{val.Unk04}");
            }
            ImGui.EndChild();

            ImGui.NextColumn();
        }
        */

        // Value
        ImGui.BeginChild("ValueList##GparamPropertyValues");
        ImGui.Text($"Value");
        ImGui.Separator();

        for (int i = 0; i < field.Values.Count; i++)
        {
            GPARAM.IFieldValue entry = field.Values[i];
            GraphicsParamValueView(i, entry);
        }

        ImGui.EndChild();
    }

    public void GraphicsParamIdView(int index, IFieldValue val)
    {
        ImGui.AlignTextToFramePadding();

        if (ImGui.Selectable($"{val.Id}##{index}", index == _selectedFieldValueKey))
        {
            _selectedFieldValue = val;
            _selectedFieldValueKey = index;
        }
    }

    public void GraphicsParamValueView(int index, IFieldValue val)
    {
        string value = val.Value.ToString();
        Type type = val.GetType();

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"{value}"); // Temp until we implement the property edit part
        //ImGui.InputText($"##{val.Id}{index}", ref value, 256);
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        if (CFG.Current.AutoLoadBank_Gparam)
            GparamParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParam(_selectedGparamInfo, _selectedGparam);
    }

    public void SaveAll()
    {
        if (GparamParamBank.IsLoaded)
            GparamParamBank.SaveGraphicsParams();
    }

    private void ParamUndo()
    {
        EditorActionManager.UndoAction();
    }

    private void ParamRedo()
    {
        EditorActionManager.RedoAction();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
