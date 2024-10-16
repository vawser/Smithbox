using ImGuiNET;
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
            TimeActEditor
        ];

        if(FeatureFlags.EnableEditor_Cutscene) 
            EditorList.Add(CutsceneEditor);

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
        FocusedEditor.SaveAll();
    }

    public void SaveFocusedEditor()
    {
        FocusedEditor.Save();
    }

    public void HandleEditorShortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
        {
            Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
            SaveFocusedEditor();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveAll))
        {
            Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
            SaveAllFocusedEditor();
        }
    }

    public void HandleEditorSharedBar()
    {
        ImGui.Separator();

        if (ImGui.BeginMenu("Data"))
        {
            // Save
            if (ImGui.Button($"Save Selected {FocusedEditor.SaveType}", UI.MenuButtonSize))
            {
                Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                SaveFocusedEditor();
            }
            UIHelper.ShowHoverTooltip(KeyBindings.Current.CORE_Save.HintText);

            // Save All
            if (ImGui.Button($"Save All Modified {FocusedEditor.SaveType}", UI.MenuButtonSize))
            {
                Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                SaveAllFocusedEditor();
            }
            UIHelper.ShowHoverTooltip(KeyBindings.Current.CORE_SaveAll.HintText);

            ImGui.EndMenu();
        }
    }
}
