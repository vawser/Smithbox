using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Linq;
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

    private GPARAM.IField _selectedParam;
    private string _selectedParamKey;

    private string _newGparamFile_Name;

    public GraphicsEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _newGparamFile_Name = "";

        _selectedGraphicsParamKey = "";
        _selectedParamGroupKey = "";
        _selectedParamKey = "";

        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Graphics Editor";
    public string CommandEndpoint => "graphics";
    public string SaveType => "Graphics";

    public void DrawEditorMenu()
    {
    }

    public void OnGUI(string[] initcmd)
    {
        var scale = Smithbox.GetUIScale();

        // Docking setup
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_GraphicsEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Project.Type is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2S)
        {
            ImGui.Text($"This editor does not support {Project.Type}.");
        }
        else if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

        if (!GraphicsParamBank.IsLoaded)
        {
            // Deselect any currently selected group
            _selectedParamGroup = null;
            _selectedParamGroupKey = "";

            // Deselect any currently selected param
            _selectedParam = null;
            _selectedParamKey = "";

            if (GraphicsParamBank.IsLoading)
            {
                ImGui.Text("Loading...");
            }
        }

        // GPARAM List
        ImGui.Begin("Files");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, param) in GraphicsParamBank.ParamBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedGraphicsParamKey))
            {
                // Deselect any currently selected group
                _selectedParamGroup = null;
                _selectedParamGroupKey = "";

                // Deselect any currently selected param
                _selectedParam = null;
                _selectedParamKey = "";

                _selectedGraphicsParamKey = info.Name;
                _selectedGraphicsParamInfo = info;
                _selectedGraphicsParam = param;
            }

            // Context menu action for duplicating exist to new name
            if (info.Name == _selectedGraphicsParamKey)
            {
                if (ImGui.BeginPopupContextItem($"Duplicate##gparamFile_Duplicate"))
                {
                    if(ImGui.Button("Duplicate"))
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

        // GPARAM Params
        ImGui.Begin("Params");

        if (_selectedGraphicsParam != null && _selectedGraphicsParamKey != "")
        {
            GPARAM data = _selectedGraphicsParam;

            ImGui.Text($"Param");
            ImGui.Separator();

            foreach (GPARAM.Param entry in data.Params)
            {
                if (ImGui.Selectable($@" {entry.Name}##{entry.Key}", entry.Key == _selectedParamGroupKey))
                {
                    // Deselect any currently selected param
                    _selectedParam = null;
                    _selectedParamKey = "";

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
                if (ImGui.Selectable($@" {entry.Name}##{entry.Key}", entry.Key == _selectedParamKey))
                {
                    _selectedParam = entry;
                    _selectedParamKey = entry.Key;
                }
            }

            // Display add action
            if(data.Fields.Count == 0)
            {

            }
        }

        ImGui.End();

        // GPARAM Values
        ImGui.Begin("Values");

        if (_selectedParam != null && _selectedParamKey != "")
        {
            GraphicsParamPropertyView();
        }

        ImGui.End();

        ImGui.PopStyleVar();
    }

    private void GraphicsParamPropertyView()
    {
        GPARAM.IField field = _selectedParam;

        if (Project.Type == ProjectType.SDT)
        {
            ImGui.Columns(3);
        }
        else
        {
            ImGui.Columns(2);
        }

        // ID
        ImGui.BeginChild("IdList");
        ImGui.Text($"ID");
        ImGui.Separator();
        foreach (var val in field.Values)
        {
            ImGui.Text($"{val.Id}");
        }

        // Add entry action here
        if (ImGui.Button($"{ForkAwesome.Plus}"))
        {
            // Popup menu to add new entry in in value list
        }

        ImGui.EndChild();

        // Unk04 (Sekiro)
        if (Project.Type == ProjectType.SDT)
        {
            ImGui.Text($"Floats");
            ImGui.Separator();
            ImGui.NextColumn();
            ImGui.BeginChild("UnkList");
            foreach (var val in field.Values)
            {
                ImGui.Text($"{val.Unk04}");
            }
            ImGui.EndChild();
        }

        // Value
        ImGui.NextColumn();
        ImGui.BeginChild("ValueList");
        ImGui.Text($"Value");
        ImGui.Separator();
        foreach (var val in field.Values)
        {
            ImGui.Text($"{val.Value}");
        }
        ImGui.EndChild();
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;
        GraphicsParamBank.LoadGraphicsParams();

        ResetActionManager();
    }

    public void Save()
    {
        //GraphicsParamBank.SaveGraphicsParam(_selectedGraphicsParamInfo, _selectedGraphicsParam);
    }

    public void SaveAll()
    {
        //GraphicsParamBank.SaveGraphicsParams();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
