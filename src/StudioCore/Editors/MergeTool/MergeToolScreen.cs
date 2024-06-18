using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.CutsceneEditor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.MergeTool;
using StudioCore.Editors.ParticleEditor;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Locators.MergeUtils;

namespace StudioCore.MergeTool;

public class MergeToolScreen : EditorScreen
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

    private readonly MergePropertyEditor _propEditor;

    public ActionManager EditorActionManager = new();

    public MergeToolScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new MergePropertyEditor(EditorActionManager);
    }

    public string EditorName => "Merge Tool##MergeTool";
    public string CommandEndpoint => "mergetool";
    public string SaveType => "MergeTool";

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

        var dsid = ImGui.GetID("DockSpace_MergeTool");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if(Smithbox.ProjectType == ProjectType.ER)
        {
            MergeToolView();
        }
        else
        {
            ImGui.Begin("Tool##InvalidMergeTool");

            ImGui.Text($"This tool does not support {Smithbox.ProjectType}.");

            ImGui.End();
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void MergeToolView()
    {
        MergeConfigurationView();

        ParamMergeView();
        FmgMergeView();
        GparamMergeView();
        MsbMergeView();
    }

    string RootProjectPath = "";
    string TargetProjectPath = "";

    public void MergeConfigurationView()
    {
        RootProjectPath = Smithbox.ProjectRoot;

        ImGui.Begin("Configuration##MergeConfiguration");

        ImguiUtils.WrappedText("Current Project:");
        AliasUtils.DisplayAlias(RootProjectPath);

        ImguiUtils.WrappedText("External Project:");
        AliasUtils.DisplayAlias(TargetProjectPath);

        if(ImGui.Button("Select External Project"))
        {

        }
        ImGui.SameLine();

        if (ImGui.Button("Select Game Root as External Project"))
        {
            TargetProjectPath = Smithbox.GameRoot;
        }

        ImGui.End();
    }

    public void ParamMergeView()
    {
        ImGui.Begin("Params##MergeParamView");

        ImGui.End();
    }

    public void FmgMergeView()
    {
        ImGui.Begin("Text##MergeFmgView");

        ImGui.End();
    }

    public void GparamMergeView()
    {
        ImGui.Begin("Gparams##MergeGparamView");

        ImGui.End();
    }

    List<MapInfo> rootMaps = new List<MapInfo>();
    List<MapInfo> targetMaps = new List<MapInfo>();

    MapInfo _selectedMap = null;



    public void MsbMergeView()
    {
        ImGui.Begin("Maps##MergeMsbView");

        if (ImGui.Button("Start Merge"))
        {
            rootMaps = MergeUtils.GetMaps(RootProjectPath);
            targetMaps = MergeUtils.GetMaps(TargetProjectPath);
        }

        ImGui.Columns(3);

        // Root
        if(rootMaps.Count > 0)
        {
            foreach (var entry in rootMaps)
            {
                if(ImGui.Selectable(entry.mapId, entry == _selectedMap))
                {
                    _selectedMap = entry;
                    _selectedMap.diffMap = null; // Re-build on selection change
                }
            }
        }

        ImGui.NextColumn();

        if(_selectedMap != null)
        {
            var currentMap = _selectedMap.map;
            var targetMap = targetMaps.Where(e => e.mapId == _selectedMap.mapId).FirstOrDefault();

            if(_selectedMap.diffMap == null)
            {
                _selectedMap.diffMap = MergeMapUtils.BuildDiffMap(currentMap, targetMap.map);
            }
            else
            {
                // Display diff stuff 
            }
        }

        ImGui.NextColumn();

        ImGui.Columns(1);

        ImGui.End();
    }

    public void OnProjectChanged()
    {
        ResetActionManager();
    }

    public void Save()
    {

    }

    public void SaveAll()
    {

    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
