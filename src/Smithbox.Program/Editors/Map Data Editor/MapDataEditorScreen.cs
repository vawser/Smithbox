using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataEditorScreen : EditorScreen
{
    private ProjectEntry Project;

    public MapDataViewHandler ViewHandler;

    public MapDataCommandQueue CommandQueue;

    public MapDataToolView ToolWindow;

    public MapDataEditorScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new MapDataViewHandler(this, project);

        CommandQueue = new(this, project);

        ToolWindow = new(this, project);
    }

    public string EditorName => "Map Data Editor";
    public string CommandEndpoint => "mapdata";
    public string SaveType => "Map Data";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        CommandQueue.Parse(commands);
        Shortcuts();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolWindow.ToolMenu();
            OptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_MapDataEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None,ref UIHelper.DockGroup_MapDataEditor);

        ViewHandler.HandleViews(dsid);

        if (ViewHandler.ActiveView != null)
        {
            ToolWindow.Draw();
        }
    }

    public void Shortcuts()
    {
        var activeView = ViewHandler.ActiveView;

        // Save
        if (InputManager.IsPressed(KeybindID.Save))
        {
            Save();
        }

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_MapDataEditor_ToolWindow = !CFG.Current.Interface_MapDataEditor_ToolWindow;
        }

        if (activeView != null)
        {
            // Undo
            if (activeView.ActionManager.CanUndo())
            {
                if (InputManager.IsPressed(KeybindID.Undo))
                {
                    activeView.ActionManager.UndoAction();
                }

                if (InputManager.IsPressedOrRepeated(KeybindID.Undo_Repeat))
                {
                    activeView.ActionManager.UndoAction();
                }
            }

            // Redo
            if (activeView.ActionManager.CanRedo())
            {
                if (InputManager.IsPressed(KeybindID.Redo))
                {
                    activeView.ActionManager.RedoAction();
                }

                if (InputManager.IsPressedOrRepeated(KeybindID.Redo_Repeat))
                {
                    activeView.ActionManager.RedoAction();
                }
            }

            // MSB Editor
            if (activeView.Selection.SubEditorMode is SubEditorType.MSB)
            {
                activeView.MsbEditor.Shortcuts();
            }

            // ENFL Editor
        }
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            ImGui.EndMenu();
        }
    }
    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Edit"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"Undo", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"Undo All"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"Redo", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanRedo())
                    {
                        activeView.ActionManager.RedoAction();
                    }
                }
            }

            ImGui.EndMenu();
        }
    }
    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Tools"))
            {
                CFG.Current.Interface_MapDataEditor_ToolWindow = !CFG.Current.Interface_MapDataEditor_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapDataEditor_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public void OptionsMenu()
    {
        if (ImGui.BeginMenu("Options"))
        {
            if (ImGui.BeginMenu("Saving"))
            {
                ImGui.Checkbox("Save Selected Only", ref CFG.Current.MapDataEditor_SaveSelectedOnly);
                UIHelper.Tooltip("If enabled, only the selected map, entry list, etc will be saved (by default all loaded resources will be saved)");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void Save(bool autoSave = false)
    {
        var mapDataHandler = Project.Handler.MapDataHandler;
        var activeView = ViewHandler.ActiveView;

        if (activeView == null)
            return;

        // MSB
        if(activeView.Selection.SubEditorMode is SubEditorType.MSB)
        {
            foreach(var entry in mapDataHandler.PrimaryBank_MSB.Maps)
            {
                if (entry.Value == null)
                    continue;

                if (CFG.Current.MapDataEditor_SaveSelectedOnly)
                {
                    if (activeView.Selection.SelectedMapDescriptor != entry.Key)
                        continue;
                }

                // TODO: add dirty check so we only save maps that have been edited
                var saveTask = mapDataHandler.PrimaryBank_MSB.SaveMap(activeView, entry.Key);
                if(saveTask)
                {
                    Smithbox.Log<MsbEditor>($"Saved map: {entry.Key.Filename}");
                }
            }
        }

        // ENFL
        if (activeView.Selection.SubEditorMode is SubEditorType.ENFL)
        {
            foreach (var entry in mapDataHandler.PrimaryBank_ENFL.EntryFileLists)
            {
                if (entry.Value == null)
                    continue;

                if(CFG.Current.MapDataEditor_SaveSelectedOnly)
                {
                    if (activeView.Selection.SelectedListDescriptor != entry.Key)
                        continue;
                }

                // TODO: add dirty check so we only save maps that have been edited
                var saveTask = mapDataHandler.PrimaryBank_ENFL.SaveEntryFileList(activeView, entry.Key);
                if (saveTask)
                {
                    Smithbox.Log<MsbEditor>($"Saved entry file list: {entry.Key.Filename}");
                }
            }
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }
}
