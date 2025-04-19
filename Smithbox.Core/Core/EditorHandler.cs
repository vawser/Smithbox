using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Graphics;
using StudioCore.GraphicsEditor;
using StudioCore.TextEditor;
using StudioCore.TextureViewer;
using System.Collections.Generic;
using System.Linq;

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
    public GparamEditorScreen GparamEditor;
    public TextureViewerScreen TextureViewer;

    public EditorHandler(IGraphicsContext _context)
    {
        EditorList = new();

        // Editors
        MapEditor = new MapEditorScreen(_context.Window, _context.Device);
        ModelEditor = new ModelEditorScreen(_context.Window, _context.Device);
        ParamEditor = new ParamEditorScreen(_context.Window, _context.Device);
        TextEditor = new TextEditorScreen(_context.Window, _context.Device);
        GparamEditor = new GparamEditorScreen(_context.Window, _context.Device);
        TextureViewer = new TextureViewerScreen(_context.Window, _context.Device);

        // Editors to Display
        if (CFG.Current.EnableEditor_MSB)
        {
            EditorList.Add(MapEditor);
        }

        if (CFG.Current.EnableEditor_FLVER)
        {
            EditorList.Add(ModelEditor);
        }

        if (CFG.Current.EnableEditor_PARAM)
        {
            EditorList.Add(ParamEditor);
        }

        if (CFG.Current.EnableEditor_FMG)
        {
            EditorList.Add(TextEditor);
        }

        if (CFG.Current.EnableViewer_TEXTURE)
        {
            EditorList.Add(TextureViewer);
        }

        if (CFG.Current.EnableEditor_GPARAM)
        {
            EditorList.Add(GparamEditor);
        }

        if (EditorList.Count > 0)
        {
            FocusedEditor = EditorList.First();
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

    public void FileDropdown()
    {
        if (ImGui.BeginMenu("File"))
        {
            // Save
            if (ImGui.MenuItem($"Save Selected {FocusedEditor.SaveType}", KeyBindings.Current.CORE_Save.HintText))
            {
                Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                SaveFocusedEditor();
            }

            // Save All
            if (ImGui.MenuItem($"Save All Modified {FocusedEditor.SaveType}", KeyBindings.Current.CORE_SaveAll.HintText))
            {
                Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                SaveAllFocusedEditor();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }
}
