using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.UserProject;
using System;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsEditor;

public class GraphicsEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    private ActionManager EditorActionManager = new();

    private GraphicsParamBank.GraphicsParamInfo _selectedGraphicsParamInfo;
    private GPARAM _selectedGraphicsParam;
    private string _selectedGraphicsParamKey;

    private GPARAM.Param _selectedParamGroup;
    private string _selectedParamGroupKey;

    private GPARAM.IField _selectedParamField;
    private string _selectedParamFieldKey;

    private GPARAM.IFieldValue _selectedFieldValue = null;
    private string _selectedFieldValueKey = "";

    public GraphicsEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        ResetAllSelection();

        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Graphics Editor";
    public string CommandEndpoint => "gparam";
    public string SaveType => "Param";

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

        var dsid = ImGui.GetID("DockSpace_GraphicsParamEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (!GraphicsParamBank.IsLoaded)
        {
            ResetAllSelection();

            if (GraphicsParamBank.IsLoading)
            {
                ImGui.Text("Loading...");
            }
        }

        GraphicsParamView();
        
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
        _selectedGraphicsParam = null;
        _selectedGraphicsParamKey = "";
    }

    private void ResetGroupSelection()
    {
        _selectedParamGroup = null;
        _selectedParamGroupKey = "";
    }

    private void ResetFieldSelection()
    {
        _selectedParamField = null;
        _selectedParamFieldKey = "";
    }

    private void ResetValueSelection()
    {
        _selectedFieldValue = null;
        _selectedFieldValueKey = "";
    }

    private void GraphicsParamView()
    {
        // GPARAM List
        ImGui.Begin("Files");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, param) in GraphicsParamBank.ParamBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedGraphicsParamKey))
            {
                ResetGroupSelection();
                ResetFieldSelection();
                ResetValueSelection();

                _selectedGraphicsParamKey = info.Name;
                _selectedGraphicsParamInfo = info;
                _selectedGraphicsParam = param;
            }

            // Context menu action for duplicating exist to new name
            if (info.Name == _selectedGraphicsParamKey)
            {
                if (ImGui.BeginPopupContextItem($"Duplicate##gparamFile_Duplicate"))
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
        ImGui.Begin("Groups");

        if (_selectedGraphicsParam != null && _selectedGraphicsParamKey != "")
        {
            GPARAM data = _selectedGraphicsParam;

            ImGui.Text($"Group");
            ImGui.Separator();

            foreach (GPARAM.Param entry in data.Params)
            {
                if (ImGui.Selectable($@" {entry.Name}##{entry.Key}", entry.Key == _selectedParamGroupKey))
                {
                    ResetFieldSelection();
                    ResetValueSelection();

                    _selectedParamGroup = entry;
                    _selectedParamGroupKey = entry.Key;
                }
            }
        }

        ImGui.End();

        // GPARAM Fields
        ImGui.Begin("Fields");

        if (_selectedParamGroup != null && _selectedParamGroupKey != "")
        {
            GPARAM.Param data = _selectedParamGroup;

            ImGui.Text($"Field");
            ImGui.Separator();

            foreach (var entry in data.Fields)
            {
                if (ImGui.Selectable($@" {entry.Name}##{entry.Key}", entry.Key == _selectedParamFieldKey))
                {
                    ResetValueSelection();

                    _selectedParamField = entry;
                    _selectedParamFieldKey = entry.Key;
                }
            }
        }

        ImGui.End();

        // GPARAM Values
        ImGui.Begin("Values");

        if (_selectedParamField != null && _selectedParamFieldKey != "")
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
        ImGui.BeginChild("IdList");
        ImGui.Text($"ID");
        ImGui.Separator();
        int idx = 0;
        foreach (var val in field.Values)
        {
            GraphicsParamIdView(idx, val);
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
        ImGui.BeginChild("ValueList");
        ImGui.Text($"Value");
        ImGui.Separator();

        idx = 0;
        foreach (var val in field.Values)
        {
            GraphicsParamValueView(idx, val);
        }
        ImGui.EndChild();
    }

    public void GraphicsParamIdView(int index, IFieldValue val)
    {
        ImGui.AlignTextToFramePadding();

        if (ImGui.Selectable($"{val.Id}##{index}", $"{val.Id}{index}" == _selectedFieldValueKey))
        {
            _selectedFieldValue = val;
            _selectedFieldValueKey = $"{val.Id}{index}";
        }
    }

    public void GraphicsParamValueView(int index, IFieldValue val)
    {
        string value = val.Value.ToString();
        Type type = val.GetType();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText($"##{val.Id}{index}", ref value, 256);
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;
        GraphicsParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        GraphicsParamBank.SaveGraphicsParam(_selectedGraphicsParamInfo, _selectedGraphicsParam);
    }

    public void SaveAll()
    {
        GraphicsParamBank.SaveGraphicsParams();
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
