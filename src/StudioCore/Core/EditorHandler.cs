﻿using ImGuiNET;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Configuration;
using StudioCore.CutsceneEditor;
using StudioCore.Editor;
using StudioCore.Editors;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.EmevdEditor;
using StudioCore.Graphics;
using StudioCore.GraphicsEditor;
using StudioCore.HavokEditor;
using StudioCore.Interface;
using StudioCore.MaterialEditor;
using StudioCore.ParticleEditor;
using StudioCore.Settings;
using StudioCore.TalkEditor;
using StudioCore.TextEditor;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core;

/// <summary>
/// Handler class that holds all of the editors and related editor state for access elsewhere.
/// </summary>
public class EditorHandler
{
    public List<EditorScreen> EditorList;
    public EditorScreen FocusedEditor;

    public MapEditorScreen MapEditor;
    public ModelEditorScreen ModelEditor;
    public TextEditorScreen TextEditor;
    public ParamEditorScreen ParamEditor;
    public TimeActEditorScreen TimeActEditor;
    public CutsceneEditorScreen CutsceneEditor;
    public GparamEditorScreen GparamEditor;
    public MaterialEditorScreen MaterialEditor;
    public ParticleEditorScreen ParticleEditor;
    public EmevdEditorScreen EmevdEditor;
    public EsdEditorScreen EsdEditor;
    public TextureViewerScreen TextureViewer;
    public HavokEditorScreen HavokEditor;

    public EditorHandler(IGraphicsContext _context)
    {
        MapEditor = new MapEditorScreen(_context.Window, _context.Device);
        ModelEditor = new ModelEditorScreen(_context.Window, _context.Device);
        TextEditor  = new TextEditorScreen(_context.Window, _context.Device);
        ParamEditor = new ParamEditorScreen(_context.Window, _context.Device);
        TimeActEditor = new TimeActEditorScreen(_context.Window, _context.Device);
        CutsceneEditor = new CutsceneEditorScreen(_context.Window, _context.Device);
        GparamEditor = new GparamEditorScreen(_context.Window, _context.Device);
        MaterialEditor = new MaterialEditorScreen(_context.Window, _context.Device);
        ParticleEditor = new ParticleEditorScreen(_context.Window, _context.Device);
        EmevdEditor = new EmevdEditorScreen(_context.Window, _context.Device);
        EsdEditor = new EsdEditorScreen(_context.Window, _context.Device);
        TextureViewer = new TextureViewerScreen(_context.Window, _context.Device);
        HavokEditor = new HavokEditorScreen(_context.Window, _context.Device);

        EditorList = [
            MapEditor,
            ModelEditor,
            ParamEditor,
            TextEditor,
            GparamEditor,
            TextureViewer,
        ];

        if(FeatureFlags.EnableEditor_Cutscene) 
            EditorList.Add(CutsceneEditor);

        if (FeatureFlags.EnableEditor_TimeAct)
            EditorList.Add(TimeActEditor);

        if (FeatureFlags.EnableEditor_HavokBehavior)
            EditorList.Add(HavokEditor);

        if (FeatureFlags.EnableEditor_Material)
            EditorList.Add(MaterialEditor);

        if (FeatureFlags.EnableEditor_Particle)
            EditorList.Add(ParticleEditor);

        if (FeatureFlags.EnableEditor_Evemd)
            EditorList.Add(EmevdEditor);

        if (FeatureFlags.EnableEditor_Esd)
            EditorList.Add(EsdEditor);

        FocusedEditor = MapEditor;

        foreach (EditorScreen editor in EditorList)
        {
            editor.Init();
        }
    }

    public void UpdateEditors()
    {
        foreach (EditorScreen editor in EditorList)
        {
            editor.OnProjectChanged();
        }
    }

    public void SaveAllFocusedEditor()
    {
        if (FocusedEditor.ShowSaveOption)
        {
            FocusedEditor.SaveAll();
        }
    }

    public void SaveFocusedEditor()
    {
        if (FocusedEditor.ShowSaveOption)
        {
            FocusedEditor.Save();
        }
    }

    public void HandleEditorShortcuts()
    {
        if (FocusedEditor.ShowSaveOption)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_SaveCurrentEditor))
            {
                Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                SaveFocusedEditor();
            }

            if (InputTracker.GetKeyDown(KeyBindings.Current.Core_SaveAllCurrentEditor))
            {
                Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                SaveAllFocusedEditor();
            }
        }
    }

    public void HandleEditorSharedBar()
    {
        // Dropdown: File
        if (ImGui.BeginMenu("文件 File"))
        {
            // New Project
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.File}");
            if (ImGui.MenuItem("新建 New Project", "", false, !TaskManager.AnyActiveTasks()))
            {
                Smithbox.ProjectHandler.ClearProject();
                Smithbox.ProjectHandler.IsInitialLoad = true;
            }

            // Open Project
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Folder}");
            if (ImGui.MenuItem("打开 Open Project", "", false, !TaskManager.AnyActiveTasks()))
            {
                Smithbox.ProjectHandler.OpenProjectDialog();
            }

            // Recent Projects
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.FolderOpen}");
            if (ImGui.BeginMenu("历史 Recent Projects",
                    !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
            {
                Smithbox.ProjectHandler.DisplayRecentProjects();

                ImGui.EndMenu();
            }

            // Open in Explorer
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Archive}");
            if (ImGui.BeginMenu("打开文件夹 Open in Explorer",
                    !TaskManager.AnyActiveTasks() && CFG.Current.RecentProjects.Count > 0))
            {
                if (ImGui.MenuItem("项目路径 Project Folder", "", false, !TaskManager.AnyActiveTasks()))
                {
                    var projectPath = Smithbox.ProjectRoot;
                    Process.Start("explorer.exe", projectPath);
                }

                if (ImGui.MenuItem("游戏路径 Game Folder", "", false, !TaskManager.AnyActiveTasks()))
                {
                    var gamePath = Smithbox.GameRoot;
                    Process.Start("explorer.exe", gamePath);
                }

                if (ImGui.MenuItem("配置文件夹 Config Folder", "", false, !TaskManager.AnyActiveTasks()))
                {
                    var configPath = CFG.GetConfigFolderPath();
                    Process.Start("explorer.exe", configPath);
                }

                ImGui.EndMenu();
            }

            // Save
            if (FocusedEditor.ShowSaveOption)
            {
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.FloppyO}");
                if (ImGui.MenuItem($"保存选中 Save Selected {FocusedEditor.SaveType}",
                        KeyBindings.Current.Core_SaveCurrentEditor.HintText))
                {
                    Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                    SaveFocusedEditor();
                }

                // Save All
                ImguiUtils.ShowMenuIcon($"{ForkAwesome.FloppyO}");
                if (ImGui.MenuItem($"保存全部 Save All Modified {FocusedEditor.SaveType}", KeyBindings.Current.Core_SaveAllCurrentEditor.HintText))
                {
                    Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                    SaveAllFocusedEditor();
                }
            }

            ImGui.EndMenu();
        }
    }
}
