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

    public string EditorName => "Texture Viewer##TextureViewerEditor";
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

            OptionsMenu();

            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_TextureViewer");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        ViewHandler.HandleViews();

        if (ViewHandler.ActiveView != null)
        {
            ToolView.Display();
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
            if (ImGui.MenuItem("Properties"))
            {
                CFG.Current.Interface_TextureViewer_Properties = !CFG.Current.Interface_TextureViewer_Properties;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Properties);

            if (ImGui.MenuItem("Tool Window"))
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
        if (ImGui.BeginMenu("Options"))
        {
            if (ImGui.BeginMenu("Display"))
            {
                ImGui.SliderFloat("Containers##containersDisplayPercentage", ref CFG.Current.TextureViewer_Display_ContainerList_Percentage, 0.01f, 0.99f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    var remainder = 1f - CFG.Current.TextureViewer_Display_ContainerList_Percentage;

                    StudioMath.Redistribute(
                        ref CFG.Current.TextureViewer_Display_InternalFileList_Percentage,
                        ref CFG.Current.TextureViewer_Display_FileList_Percentage,
                        remainder);
                }
                UIHelper.Tooltip("The percentage of the window the Containers section occupies.");

                ImGui.SliderFloat("Files##internalFilesDisplayPercentage", ref CFG.Current.TextureViewer_Display_InternalFileList_Percentage, 0.01f, 0.99f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    var remainder = 1f - CFG.Current.TextureViewer_Display_InternalFileList_Percentage;

                    StudioMath.Redistribute(
                        ref CFG.Current.TextureViewer_Display_ContainerList_Percentage,
                        ref CFG.Current.TextureViewer_Display_FileList_Percentage,
                        remainder);
                }
                UIHelper.Tooltip("The percentage of the window the Files section occupies.");

                ImGui.SliderFloat("Textures##filesDisplayPercentage", ref CFG.Current.TextureViewer_Display_FileList_Percentage, 0.01f, 0.99f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    var remainder = 1f - CFG.Current.TextureViewer_Display_FileList_Percentage;

                    StudioMath.Redistribute(
                        ref CFG.Current.TextureViewer_Display_ContainerList_Percentage,
                        ref CFG.Current.TextureViewer_Display_InternalFileList_Percentage,
                        remainder);
                }
                UIHelper.Tooltip("The percentage of the window the Textures section occupies.");

                ImGui.EndMenu();
            }

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
