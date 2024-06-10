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
using static StudioCore.Editors.BehaviorEditor.HavokBehaviorBank;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Xml;

namespace StudioCore.BehaviorEditor;

public class HavokEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ActionManager EditorActionManager = new();

    private BehaviorFileInfo _selectedFileInfo;
    private IBinder _selectedBinder;
    private string _selectedBinderKey;

    private string _selectedHkxKey;
    private HkxFileInfo _selectedHkxFileInfo;

    public HavokEditorScreen(Sdl2Window window, GraphicsDevice device)
    {

    }

    public string EditorName => "Havok Editor##HavokEditor";
    public string CommandEndpoint => "havok";
    public string SaveType => "Havok";

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

        var dsid = ImGui.GetID("DockSpace_BehaviorEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Project.Type != ProjectType.ER)
        {
            ImGui.Begin("Editor##InvalidHavokEditor");

            ImGui.Text($"This editor does not support {Project.Type}.");

            ImGui.End();
        }
        else
        {
            if (!HavokBehaviorBank.IsLoaded)
            {
                HavokBehaviorBank.LoadBehaviors();
            }

            if (HavokBehaviorBank.IsLoaded)
            {
                HavokBehaviorFileView();
                HavokBehaviorSelectView();
                HavokBehaviorTreeView();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void HavokBehaviorFileView()
    {
        // File List
        ImGui.Begin("Files##BehaviorFileList");

        foreach (var (info, binder) in HavokBehaviorBank.FileBank)
        {
            if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedBinderKey))
            {
                _selectedBinderKey = info.Name;
                _selectedFileInfo = info;
                _selectedBinder = binder;

                HavokBehaviorBank.LoadSelectedHavokBehaviorFiles(info, binder);
            }
        }

        ImGui.End();
    }

    public void HavokBehaviorSelectView()
    {
        // HKX
        ImGui.Begin("Havok Behavior##HavokBehaviorFileList");

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

    public void HavokBehaviorTreeView()
    {
        // Class
        ImGui.Begin("Data##HavokBehaviorTree");

        if(_selectedHkxFileInfo != null)
        {

        }

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        if (CFG.Current.AutoLoadBank_Behavior)
            HavokBehaviorBank.LoadBehaviors();

        ResetActionManager();
    }

    public void Save()
    {
        if (HavokBehaviorBank.IsLoaded)
            HavokBehaviorBank.SaveBehavior(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (HavokBehaviorBank.IsLoaded)
            HavokBehaviorBank.SaveBehaviors();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
