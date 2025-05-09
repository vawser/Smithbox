using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TextureViewer.Tools;
using StudioCore.Editors.TextureViewer.Utils;
using StudioCore.Interface;
using StudioCore.Resource;
using StudioCore.Utilities;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.TextureViewer;

public class TextureViewerScreen : EditorScreen, IResourceEventListener
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public ActionManager EditorActionManager = new();

    public TexViewSelection Selection;
    public TexShortcuts EditorShortcuts;
    public TexViewerZoom ViewerZoom;
    public TexFilters Filters;
    public TexCommandQueue CommandQueue;

    public TexToolView ToolWindow;
    public TexToolMenubar ToolMenubar;

    public TexTools Tools;

    public TexImagePreview ImagePreview;

    public TexFileContainerView FileContainerView;
    public TexTextureListView TextureListView;
    public TexTextureViewport TextureViewport;
    public TexTexturePropertyView TexturePropertyView;

    public TextureViewerScreen(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;

        Selection = new TexViewSelection(this);
        Tools = new TexTools(this);
        Filters = new TexFilters(this);
        CommandQueue = new TexCommandQueue(this);

        ImagePreview = new TexImagePreview(this);

        ViewerZoom = new TexViewerZoom(this);
        EditorShortcuts = new TexShortcuts(this);

        ToolWindow = new TexToolView(this);
        ToolMenubar = new TexToolMenubar(this);

        FileContainerView = new TexFileContainerView(this);
        TextureListView = new TexTextureListView(this);
        TextureViewport = new TexTextureViewport(this);
        TexturePropertyView = new TexTexturePropertyView(this);
    }

    public string EditorName => "Texture Viewer##TextureViewerEditor";
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    /// <summary>
    /// The editor loop
    /// </summary>
    public void OnGUI(string[] initcmd)
    {
        var scale = DPI.GetUIScale();

        // Docking setup
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(4, 4) * scale);
        Vector2 wins = ImGui.GetWindowSize();
        Vector2 winp = ImGui.GetWindowPos();
        winp.Y += 20.0f * scale;
        wins.Y -= 20.0f * scale;
        ImGui.SetNextWindowPos(winp);
        ImGui.SetNextWindowSize(wins);

        var dsid = ImGui.GetID("DockSpace_TextureViewer");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (Smithbox.LowRequirementsMode)
        {
            ImGui.Begin("Viewer##InvalidTextureViewerLowReqs");

            ImGui.Text("Not usable in Low Requirements mode.");

            ImGui.End();

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(1);

            return;
        }

        CommandQueue.Parse(initcmd);
        EditorShortcuts.Monitor();

        if (ImGui.BeginMenuBar())
        {
            FileMenu();
            EditMenu();
            ViewMenu();
            ToolMenu();

            ImGui.EndMenuBar();
        }

        if (CFG.Current.Interface_TextureViewer_Files)
        {
            FileContainerView.Display();
        }
        if (CFG.Current.Interface_TextureViewer_Textures)
        {
            TextureListView.Display();
        }
        if (CFG.Current.Interface_TextureViewer_Viewer)
        {
            TextureViewport.Display();
        }
        if (CFG.Current.Interface_TextureViewer_Properties)
        {
            TexturePropertyView.Display();
        }

        if (CFG.Current.Interface_TextureViewer_ToolWindow)
        {
            ToolWindow.Display();
        }

        if (CFG.Current.Interface_TextureViewer_ResourceList)
        {
            ResourceListWindow.DisplayWindow("textureViewerResourceList");
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void FileMenu()
    {
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem($"Save", $"{KeyBindings.Current.CORE_Save.HintText}"))
            {
                Save();
            }

            if (ImGui.MenuItem($"Save All", $"{KeyBindings.Current.CORE_SaveAll.HintText}"))
            {
                SaveAll();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void EditMenu()
    {
        if (ImGui.BeginMenu("Edit"))
        {
            // Undo
            if (ImGui.MenuItem($"Undo", $"{KeyBindings.Current.CORE_UndoAction.HintText} / {KeyBindings.Current.CORE_UndoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAction();
                }
            }

            // Undo All
            if (ImGui.MenuItem($"Undo All"))
            {
                if (EditorActionManager.CanUndo())
                {
                    EditorActionManager.UndoAllAction();
                }
            }

            // Redo
            if (ImGui.MenuItem($"Redo", $"{KeyBindings.Current.CORE_RedoAction.HintText} / {KeyBindings.Current.CORE_RedoContinuousAction.HintText}"))
            {
                if (EditorActionManager.CanRedo())
                {
                    EditorActionManager.RedoAction();
                }
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Files"))
            {
                CFG.Current.Interface_TextureViewer_Files = !CFG.Current.Interface_TextureViewer_Files;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Files);

            if (ImGui.MenuItem("Textures"))
            {
                CFG.Current.Interface_TextureViewer_Textures = !CFG.Current.Interface_TextureViewer_Textures;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Textures);

            if (ImGui.MenuItem("Viewer"))
            {
                CFG.Current.Interface_TextureViewer_Viewer = !CFG.Current.Interface_TextureViewer_Viewer;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Viewer);

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

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_TextureViewer_ResourceList = !CFG.Current.Interface_TextureViewer_ResourceList;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_ResourceList);

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void ToolMenu()
    {
        ToolMenubar.Display();
    }

    public void Save()
    {
        // Nothing

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
    }

    public void SaveAll()
    {
        // Nothing

        // Save the configuration JSONs
        BaseEditor.SaveConfiguration();
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
