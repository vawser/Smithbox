using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.TalkEditor;
using StudioCore.Interface;
using StudioCore.UserProject;
using System.Collections.Generic;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.ESD;
using static StudioCore.Editors.TalkEditor.EsdBank;

namespace StudioCore.TalkEditor;

public class EsdEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ActionManager EditorActionManager = new();

    private EsdScriptInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private ESD _selectedEsdScript;
    private int _selectedEsdScriptKey;
    private long _selectedStateGroupKey;
    private long _selectedStateGroupSubKey;
    private Dictionary<long, State> _selectedStateGroups;
    private State _selectedStateNode;

    public EsdEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
    }

    public string EditorName => "ESD Editor##TalkScriptEditor";
    public string Discription => "ESD 编辑器 ESD Editor";
    public string CommandEndpoint => "esd";
    public string SaveType => "ESD";

    public void Init()
    {
        ShowSaveOption = false;
    }

    public void DrawEditorMenu()
    {
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

        var dsid = ImGui.GetID("DockSpace_TalkScriptEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (!EsdBank.IsLoaded)
        {
            EsdBank.LoadEsdScripts();
        }

        if (EsdBank.IsLoaded)
        {
            EsdFileView();
            EsdStateGroupSelectView();
            EsdStateNodeSelectView();
            EsdStateNodeView();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void EsdFileView()
    {
        // File List
        ImGui.Begin("Files##TalkFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in EsdBank.TalkBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedEsdScriptKey = -1; // Clear talk key if file is changed

                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;
            }
        }

        ImGui.End();

        ImGui.Begin("Scripts##EsdScriptList");

        if (_selectedFileInfo != null)
        {
            ImGui.Text($"Scripts");
            ImGui.Separator();

            for (int i = 0; i < _selectedFileInfo.EsdFiles.Count; i++)
            {
                ESD entry = _selectedFileInfo.EsdFiles[i];

                if (ImGui.Selectable($@" {entry.Name}", i == _selectedEsdScriptKey))
                {
                    _selectedEsdScriptKey = i;
                    _selectedEsdScript = entry;
                }
            }
        }

        ImGui.End();
    }

    public void EsdStateGroupSelectView()
    {
        ImGui.Begin("State Group Selection##EsdStateGroupSelectView");

        if (_selectedEsdScript != null)
        {
            foreach (var entry in _selectedEsdScript.StateGroups)
            {
                var stateId = entry.Key;
                var stateGroups = entry.Value;

                if (ImGui.Selectable($@" {stateId}", _selectedStateGroupKey == stateId))
                {
                    _selectedStateGroupKey = stateId;
                    _selectedStateGroups = stateGroups;
                }
            }
        }

        ImGui.End();
    }

    public void EsdStateNodeSelectView()
    {
        // File List
        ImGui.Begin("State Node Selection##EsdStateNodeSelectView");

        if (_selectedStateGroups != null)
        {
            foreach(var (key, entry) in _selectedStateGroups)
            {
                if (ImGui.Selectable($@" {key}", key == _selectedStateGroupSubKey))
                {
                    _selectedStateGroupSubKey = key;
                    _selectedStateNode = entry;
                }
            }
        }

        ImGui.End();
    }

    // TODO: add ESD node stuff
    public void EsdStateNodeView()
    {
        // File List
        ImGui.Begin("State Node##EsdStateNodeView");

        if (_selectedStateNode != null)
        {
            ImGui.Columns(2);

            ImGui.NextColumn();

            ImGui.Columns(1);
        }

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        EsdBank.LoadEsdScripts();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (EsdBank.IsLoaded)
            EsdBank.SaveEsdScript(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (EsdBank.IsLoaded)
            EsdBank.SaveEsdScripts();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
