using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Editors.TalkEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.CutsceneEditor.CutsceneBank;
using static StudioCore.Editors.EmevdEditor.EventScriptBank;
using static StudioCore.Editors.TalkEditor.TalkScriptBank;

namespace StudioCore.TalkEditor;

public class TalkScriptEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private TalkScriptInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private ESD _selectedTalkScript;
    private int _selectedTalkScriptKey;

    public TalkScriptEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Talk Script Editor##TalkScriptEditor";
    public string CommandEndpoint => "esd";
    public string SaveType => "ESD";

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

        if (!TalkScriptBank.IsLoaded)
        {
            if (!CFG.Current.AutoLoadBank_TalkScript)
            {
                if (ImGui.Button("Load Talk Script Editor"))
                {
                    TalkScriptBank.LoadTalkScripts();
                }
            }
        }

        var dsid = ImGui.GetID("DockSpace_TalkScriptEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (TalkScriptBank.IsLoaded)
        {
            TalkScriptFileView();
        }

        ImGui.PopStyleVar();
    }

    public void TalkScriptFileView()
    {
        // File List
        ImGui.Begin("Files##TalkFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in TalkScriptBank.TalkBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedTalkScriptKey = -1; // Clear talk key if file is changed

                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;
            }
        }

        ImGui.End();

        // File List
        ImGui.Begin("Scripts##EsdScriptList");

        if (_selectedFileInfo.EsdFiles != null)
        {
            ImGui.Text($"Scripts");
            ImGui.Separator();

            for (int i = 0; i < _selectedFileInfo.EsdFiles.Count; i++)
            {
                ESD entry = _selectedFileInfo.EsdFiles[i];

                if (ImGui.Selectable($@" {entry.Name}", i == _selectedTalkScriptKey))
                {
                    _selectedTalkScriptKey = i;
                    _selectedTalkScript = entry;
                }
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        if (CFG.Current.AutoLoadBank_TalkScript)
            TalkScriptBank.LoadTalkScripts();

        ResetActionManager();
    }

    public void Save()
    {
        if (TalkScriptBank.IsLoaded)
            TalkScriptBank.SaveTalkScript(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (TalkScriptBank.IsLoaded)
            TalkScriptBank.SaveTalkScripts();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
