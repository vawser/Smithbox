using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Settings;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.CutsceneEditor.CutsceneBank;

namespace StudioCore.CutsceneEditor;

public class CutsceneEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private CutsceneFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private MQB _selectedCutscene;
    private int _selectedCutsceneKey;

    public CutsceneEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Cutscene Editor";
    public string CommandEndpoint => "cutscene";
    public string SaveType => "Cutscene";

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

        var dsid = ImGui.GetID("DockSpace_CutsceneEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (CutsceneBank.IsLoaded)
        {
            CutsceneFileView();
        }

        ImGui.PopStyleVar();
    }

    public void CutsceneFileView()
    {
        // File List
        ImGui.Begin("Files##CutsceneFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in CutsceneBank.FileBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedCutsceneKey = -1; // Clear cutscene key if file is changed

                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;
            }
        }

        ImGui.End();

        // File List
        ImGui.Begin("Cutscenes##CutsceneList");

        if (_selectedFileInfo.CutsceneFiles != null)
        {
            ImGui.Text($"Cutscenes");
            ImGui.Separator();

            for (int i = 0; i < _selectedFileInfo.CutsceneFiles.Count; i++)
            {
                MQB entry = _selectedFileInfo.CutsceneFiles[i];

                if (ImGui.Selectable($@" {entry.Name}", i == _selectedCutsceneKey))
                {
                    _selectedCutsceneKey = i;
                    _selectedCutscene = entry;
                }
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        CutsceneBank.LoadCutscenes();

        ResetActionManager();
    }

    public void Save()
    {
        CutsceneBank.SaveCutscene(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        CutsceneBank.SaveCutscenes();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
