using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.ParticleEditor;
using StudioCore.Editors.TalkEditor;
using System.Numerics;
using System.Reflection;
using Veldrid;
using Veldrid.Sdl2;
using HKLib.Serialization.hk2018.Binary;
using HKLib.Serialization.hk2018.Xml;
using StudioCore.Core;
using HKLib.hk2018;
using StudioCore.Editors.HavokEditor;
using static StudioCore.Editors.HavokEditor.HavokBehaviorBank;
using System.Linq;
using System;

namespace StudioCore.HavokEditor;

public class HavokEditorScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    public ActionManager EditorActionManager = new();

    private BehaviorFileInfo _selectedFileInfo;
    private BND4Reader _selectedBinder;
    private string _selectedBinderKey;

    public hkRootLevelContainer CurrentBehaviorFile;
    private HavokBehaviorGraph BehaviorGraph;


    public HavokEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        BehaviorGraph = new HavokBehaviorGraph(this);
    }

    public string EditorName => "Havok Editor##HavokEditor";
    public string CommandEndpoint => "Havok";
    public string SaveType => "Havok";

    public void Init()
    {
        ShowSaveOption = true;
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

        if (Smithbox.ProjectType != ProjectType.ER)
        {
            ImGui.Begin("Editor##InvalidHavokEditor");

            ImGui.Text($"This editor does not support {Smithbox.ProjectType}.");

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
                HavokFileCategoryView();
                HavokBehaviorFileView();
                BehaviorGraph.DisplayGraph();
            }
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public enum HavokFileCategories
    {
        None,

        // behbnd
        Configuration,
        Character,
        Behavior,
        Behavior_Common, // c9997

        // anibnd - Uses compendium
        Animation, 
        Skeleton
    }

    private HavokFileCategories _selectedFileCategoryKey = HavokFileCategories.None;

    public void HavokFileCategoryView()
    {
        ImGui.Begin("File Category##HavokFileCategoryList");

        foreach (var entry in Enum.GetNames(typeof(HavokFileCategories)))
        {
            if(entry != "None")
            {
                if (ImGui.Selectable($@"{entry.Replace("_", " ")}", entry == _selectedBinderKey))
                {
                    _selectedBinderKey = entry;
                }
            }
        }

        ImGui.End();
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

                CurrentBehaviorFile = HavokBehaviorBank.LoadSelectedHavokBehaviorFile(info, binder);
            }
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
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (HavokBehaviorBank.IsLoaded)
            HavokBehaviorBank.SaveBehavior(_selectedFileInfo, _selectedBinder);
    }

    public void SaveAll()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (HavokBehaviorBank.IsLoaded)
            HavokBehaviorBank.SaveBehaviors();
    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
