using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Settings;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.CutsceneEditor.CutsceneBank;
using static StudioCore.Editors.EmevdEditor.EventScriptBank;

namespace StudioCore.EmevdEditor;

public class EventScriptEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private EventScriptInfo _selectedFileInfo;
    private EMEVD _selectedScript;
    private string _selectedScriptKey;

    public EventScriptEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Event Script Editor##EventScriptEditor";
    public string CommandEndpoint => "emevd";
    public string SaveType => "EMEVD";

    public void Init()
    {

    }
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

        if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

        if (!EventScriptBank.IsLoaded)
        {
            if (!CFG.Current.AutoLoadBank_EventScript)
            {
                if (ImGui.Button("Load Event Script Editor"))
                {
                    EventScriptBank.LoadEventScripts();
                }
            }
        }

        var dsid = ImGui.GetID("DockSpace_EventScriptEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (EventScriptBank.IsLoaded)
        {
            EventScriptFileView();
        }

        ImGui.PopStyleVar();
    }

    public void EventScriptFileView()
    {
        // File List
        ImGui.Begin("Files##EventScriptFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in EventScriptBank.ScriptBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedScriptKey))
            {
                _selectedScriptKey = info.Name;
                _selectedFileInfo = info;
                _selectedScript = binder;
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        if (CFG.Current.AutoLoadBank_EventScript)
            EventScriptBank.LoadEventScripts();

        ResetActionManager();
    }

    public void Save()
    {
        if (EventScriptBank.IsLoaded)
            EventScriptBank.SaveEventScript(_selectedFileInfo, _selectedScript);
    }

    public void SaveAll()
    {
        if (EventScriptBank.IsLoaded)
            EventScriptBank.SaveEventScripts();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
