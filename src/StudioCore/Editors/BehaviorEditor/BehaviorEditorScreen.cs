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
using static StudioCore.Editors.BehaviorEditor.BehaviorBank;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Xml;

namespace StudioCore.BehaviorEditor;

public class BehaviorEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    private readonly PropertyEditor _propEditor;

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
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_BehaviorEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Project.Type != ProjectType.ER)
        {
            ImGui.Begin("Editor##InvalidBehaviorEditor");

            ImGui.Text($"This editor does not support {Project.Type}.");

            ImGui.End();
        }
        else
        {
            if (!BehaviorBank.IsLoaded)
            {
                BehaviorBank.LoadBehaviors();
            }

            if (BehaviorBank.IsLoaded)
            {
                BehaviorFileView();
                BehaviorHkxSelectView();
                BehaviorHkxTreeView();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
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
        ImGui.Begin("HKX##BehaviorHkxFileList");

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
        ImGui.Begin("Data##BehaviorHkxTree");

        if(_selectedHkxFileInfo != null)
        {

        }

        ImGui.End();
    }

    public void OnProjectChanged()
    {
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
