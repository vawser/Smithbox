using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TextureViewerScreen : EditorScreen, IResourceEventListener
{
    public ProjectEntry Project;

    public TexViewHandler ViewHandler;

    public TexShortcuts Shortcuts;
    public TexCommandQueue CommandQueue;

    public TexToolView ToolView;

    public TextureViewerScreen(ProjectEntry project)
    {
        Project = project;

        ViewHandler = new TexViewHandler(this, project);

        CommandQueue = new TexCommandQueue(this, Project);
        Shortcuts = new TexShortcuts(this, Project);

        ToolView = new TexToolView(this, Project);
    }

    public string EditorName => "";
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        var scale = DPI.UIScale();

        Shortcuts.Monitor();

        CommandQueue.Parse(commands);

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();

            ToolView.DisplayMenubar();

            //OptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_TextureViewer");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref UIHelper.DockGroup_TextureViewer);

        ViewHandler.HandleViews(dsid);

        if (ViewHandler.ActiveView != null)
        {
            ToolView.Display();
        }
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_File")}##fileMenuHeader"))
        {
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
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Properties);

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_Menubar_Toggle_View_Tools")}##toolsViewToggle"))
            {
                CFG.Current.Interface_TextureViewer_ToolWindow = !CFG.Current.Interface_TextureViewer_ToolWindow;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_ToolWindow);

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

    public void Save()
    {
        // Nothing

        // Save the configuration JSONs
        Smithbox.Instance.SaveConfiguration();
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
