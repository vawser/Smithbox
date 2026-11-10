using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TextureViewerScreen : EditorScreen, IResourceEventListener
{
    public ProjectEntry Project;

    public TexViewHandler ViewHandler;

    public TexShortcuts Shortcuts;
    public TexCommandQueue CommandQueue;

    public TextureViewerScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new TexViewHandler(this, project);

        CommandQueue = new TexCommandQueue(this, Project);
        Shortcuts = new TexShortcuts(this, Project);
    }

    // Shown by the unsaved changes prompt
    public string EditorName => LOC.Get("TEXVIEW_Texture_Viewer_Stub");
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();
        var activeView = ViewHandler.ActiveView;

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            if(activeView != null)
            {
                activeView.ToolView.DisplayMenubar();
            }

            //OptionsMenu();

            if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
            {
                ImGui.Separator();

                ImGui.Text(LOC.Get("SYS_OpenGL_Texture_Viewer_Blocked"));
            }

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_TextureViewer");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_TextureViewer);

        ViewHandler.HandleViews(dsid);
    }

    public void FileMenu()
    {
        // File
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
            // Save
            if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Save")}##saveAction", $"{InputManager.GetHint(KeybindID.Save)}"))
            {
                Save();
            }

            ImGui.EndMenu();
        }
    }

    public void EditMenu()
    {
        var activeView = ViewHandler.ActiveView;

        // Edit
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Edit")}##editMenuHeader"))
        {
            if (activeView != null)
            {
                // Undo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo")}##undoAction", $"{InputManager.GetHint(KeybindID.Undo)} / {InputManager.GetHint(KeybindID.Undo_Repeat)}"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAction();
                    }
                }

                // Undo All
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Undo_All")}##undoAllAction"))
                {
                    if (activeView.ActionManager.CanUndo())
                    {
                        activeView.ActionManager.UndoAllAction();
                    }
                }

                // Redo
                if (ImGui.MenuItem($"{LOC.Get("EDITOR_Menubar_Action_Redo")}##redoAction", $"{InputManager.GetHint(KeybindID.Redo)} / {InputManager.GetHint(KeybindID.Redo_Repeat)}"))
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
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_Menubar_Toggle_View_Properties")}##propertiesViewToggle"))
            {
                CFG.Current.Interface_TextureViewer_Properties = !CFG.Current.Interface_TextureViewer_Properties;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Properties);

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_Menubar_Toggle_View_Tools")}##toolsViewToggle"))
            {
                CFG.Current.Interface_TextureViewer_ToolWindow = !CFG.Current.Interface_TextureViewer_ToolWindow;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_TextureViewer_ToolWindow);

            ImGui.Separator();

            ViewHandler.DisplayMenu();

            ImGui.EndMenu();
        }
    }

    public void OptionsMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Options")}##optionsMenuHeader"))
        {
            

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Writes every container with unsaved texture edits to the project (TextureBank.SaveAll), then the configuration.
    /// </summary>
    public void Save()
    {
        var bank = Project.Handler?.TextureData?.PrimaryBank;

        if (bank != null && bank.HasUnsavedChanges)
        {
            bank.SaveAll();
        }

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
    }

    public void SaveAll()
    {
        Save();
    }

    // Matches what Save writes: every container with an edited TPF
    public List<string> GetUnsavedFiles()
    {
        return Project.Handler?.TextureData?.PrimaryBank?.GetUnsavedFiles() ?? new();
    }

    public void SaveUnsaved()
    {
        Save();
    }

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
        // Nothing
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
        // Nothing
    }
}
