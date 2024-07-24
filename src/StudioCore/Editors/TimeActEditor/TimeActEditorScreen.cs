using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.GraphicsEditor;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.TimeActEditor.AnimationBank;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    private readonly PropertyEditor _propEditor;

    public ActionManager EditorActionManager = new();

    private AnimationFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private TAE _selectedTimeAct;
    private int _selectedTimeActKey;

    public TimeActEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "TimeAct Editor##TimeActEditor";
    public string Discription => "帧行为(TAE) TimeAct Editor";
    public string CommandEndpoint => "timeact";
    public string SaveType => "TAE";

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

        var dsid = ImGui.GetID("DockSpace_TimeActEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.BB or ProjectType.DS2S or ProjectType.DS2)
        {
            ImGui.Begin("Editor##InvalidTaeEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }
        else
        {

            if (!AnimationBank.IsLoaded)
            {
                AnimationBank.LoadTimeActs();
            }

            if (AnimationBank.IsLoaded)
            {
                TimeActFileView();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void TimeActFileView()
    {
        // File List
        ImGui.Begin("Files##TimeActFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in AnimationBank.FileBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedTimeActKey = -1; // Clear tae key if file is changed

                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;
            }
        }

        ImGui.End();

        // File List
        ImGui.Begin("TimeActs##TimeActList");

        if (_selectedFileInfo != null)
        {
            ImGui.Text($"TimeActs");
            ImGui.Separator();

            for (int i = 0; i < _selectedFileInfo.TimeActFiles.Count; i++)
            {
                TAE entry = _selectedFileInfo.TimeActFiles[i];

                if (ImGui.Selectable($@" {entry.ID}", i == _selectedTimeActKey))
                {
                    _selectedTimeActKey = i;
                    _selectedTimeAct = entry;
                }
            }
        }

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        if (CFG.Current.AutoLoadBank_TimeAct)
            AnimationBank.LoadTimeActs();

        ResetActionManager();
    }

    public void Save()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (AnimationBank.IsLoaded)
            AnimationBank.SaveTimeAct(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (AnimationBank.IsLoaded)
            AnimationBank.SaveTimeActs();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
