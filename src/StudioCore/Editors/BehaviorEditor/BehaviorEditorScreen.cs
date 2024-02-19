using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.BehaviorEditor;
using StudioCore.Editors.ParticleEditor;
using StudioCore.Editors.TalkEditor;
using StudioCore.UserProject;
using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.Sdl2;
using static SoulsFormats.HKX;
using static StudioCore.Editors.BehaviorEditor.BehaviorBank;

namespace StudioCore.BehaviorEditor;

public class BehaviorEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private BehaviorFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private string _selectedHkxKey;
    private HkxFileInfo _selectedHkxFileInfo;

    public BehaviorEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Behavior Editor##BehaviorEditor";
    public string CommandEndpoint => "behavior";
    public string SaveType => "Behavior";

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

        if (Project.Type != ProjectType.DS3)
        {
            ImGui.Text($"This editor does not support {Project.Type}.");
            ImGui.PopStyleVar();
            return;
        }
        else if (_projectSettings == null)
        {
            ImGui.Text("No project loaded. File -> New Project");
        }

        if (!BehaviorBank.IsLoaded)
        {
            if (!CFG.Current.AutoLoadBank_Behavior)
            {
                if (ImGui.Button("Load Behavior Editor"))
                {
                    BehaviorBank.LoadBehaviors();
                }
            }
        }

        var dsid = ImGui.GetID("DockSpace_BehaviorEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (BehaviorBank.IsLoaded)
        {
            BehaviorFileView();
            BehaviorHkxSelectView();
            BehaviorHkxTreeView();
        }

        ImGui.PopStyleVar();
    }

    public void BehaviorFileView()
    {
        // File List
        ImGui.Begin("Files##BehaviorFileList");

        foreach (var (info, binder) in BehaviorBank.FileBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;

                BehaviorBank.LoadSelectedHkxFiles(info, binder);
            }
        }

        ImGui.End();
    }

    public void BehaviorHkxSelectView()
    {
        // HKX
        ImGui.Begin("HKX##HkxFileList");

        if (_selectedFileInfo != null)
        {
            for (int i = 0; i < _selectedFileInfo.HkxFiles.Count; i++)
            {
                HkxFileInfo entry = _selectedFileInfo.HkxFiles[i];

                // i is added into the name to account for duplicate names across multiple directories
                if (ImGui.Selectable($@" {entry.Name}##{entry.Name}{i}", entry.Name == _selectedHkxKey))
                {
                    _selectedHkxKey = $"{entry.Name}{i}";
                    _selectedHkxFileInfo = entry;
                }
            }
        }

        ImGui.End();
    }

    public void BehaviorHkxTreeView()
    {
        // Class
        ImGui.Begin("Tree##HkxTree");

        if(_selectedHkxFileInfo != null)
        {
            HKXSection section = _selectedHkxFileInfo.Entry.DataSection;

            // Class Section
            ImGui.Text($"{section.SectionID}");
            ImGui.Text($"{section.SectionTag}");


        }

        ImGui.End();
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        if (CFG.Current.AutoLoadBank_Behavior)
            BehaviorBank.LoadBehaviors();

        ResetActionManager();
    }

    public void Save()
    {
        if (BehaviorBank.IsLoaded)
            BehaviorBank.SaveBehavior(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (BehaviorBank.IsLoaded)
            BehaviorBank.SaveBehaviors();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
