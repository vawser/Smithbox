using ImGuiNET;
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
    public ActionManager EditorActionManager = new();

    public TexViewSelection Selection;
    public TexShortcuts EditorShortcuts;
    public TexViewerZoom ViewerZoom;
    public TexFilters Filters;
    public TexCommandQueue CommandQueue;

    public TexToolView ToolWindow;
    public TexToolMenubar ToolMenubar;
    public TexActionMenubar ActionMenubar;

    public TexTools Tools;

    public TexImagePreview ImagePreview;
    public ShoeboxLayoutContainer ShoeboxLayouts;

    public TexFileContainerView FileContainerView;
    public TexTextureListView TextureListView;
    public TexTextureViewport TextureViewport;
    public TexTexturePropertyView TexturePropertyView;

    public TextureViewerScreen(Sdl2Window window, GraphicsDevice device)
    {
        Selection = new TexViewSelection(this);
        Tools = new TexTools(this);
        Filters = new TexFilters(this);
        CommandQueue = new TexCommandQueue(this);

        ShoeboxLayouts = new ShoeboxLayoutContainer(this);
        ImagePreview = new TexImagePreview(this);

        ViewerZoom = new TexViewerZoom(this);
        EditorShortcuts = new TexShortcuts(this);

        ToolWindow = new TexToolView(this);
        ToolMenubar = new TexToolMenubar(this);
        ActionMenubar = new TexActionMenubar(this);

        FileContainerView = new TexFileContainerView(this);
        TextureListView = new TexTextureListView(this);
        TextureViewport = new TexTextureViewport(this);
        TexturePropertyView = new TexTexturePropertyView(this);
    }

    public string EditorName => "Texture Viewer##TextureViewerEditor";
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";

    /// <summary>
    /// The editor menubar
    /// </summary>
    public void DrawEditorMenu()
    {
        // ImGui.Separator();

        // ActionMenubar.DisplayMenu();

        ImGui.Separator();

        ToolMenubar.Display();

        ImGui.Separator();

        if (ImGui.BeginMenu("Windows"))
        {
            if (ImGui.Button("Files", UI.MenuButtonSize))
            {
                UI.Current.Interface_TextureViewer_Files = !UI.Current.Interface_TextureViewer_Files;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextureViewer_Files);

            if (ImGui.Button("Textures", UI.MenuButtonSize))
            {
                UI.Current.Interface_TextureViewer_Textures = !UI.Current.Interface_TextureViewer_Textures;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextureViewer_Textures);

            if (ImGui.Button("Viewer", UI.MenuButtonSize))
            {
                UI.Current.Interface_TextureViewer_Viewer = !UI.Current.Interface_TextureViewer_Viewer;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextureViewer_Viewer);

            if (ImGui.Button("Properties", UI.MenuButtonSize))
            {
                UI.Current.Interface_TextureViewer_Properties = !UI.Current.Interface_TextureViewer_Properties;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextureViewer_Properties);

            if (ImGui.Button("Tool Window", UI.MenuButtonSize))
            {
                UI.Current.Interface_TextureViewer_ToolConfiguration = !UI.Current.Interface_TextureViewer_ToolConfiguration;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextureViewer_ToolConfiguration);

            if (ImGui.Button("Resource List", UI.MenuButtonSize))
            {
                UI.Current.Interface_TextureViewer_ResourceList = !UI.Current.Interface_TextureViewer_ResourceList;
            }
            UIHelper.ShowActiveStatus(UI.Current.Interface_TextureViewer_ResourceList);

            ImGui.EndMenu();
        }
    }

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
        
        if(!TexUtils.IsSupportedProjectType() || Smithbox.ProjectHandler.CurrentProject == null)
        {
            ImGui.Begin("Viewer##InvalidTextureViewer");

            ImGui.Text("Texture Viewer does not support this project type.");

            ImGui.End();

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(1);

            return;
        }
        else if(Smithbox.LowRequirementsMode)
        {
            ImGui.Begin("Viewer##InvalidTextureViewerLowReqs");

            ImGui.Text("Not usable in Low Requirements mode.");

            ImGui.End();

            ImGui.PopStyleVar();
            ImGui.PopStyleColor(1);

            return;
        }

        if (!TextureFolderBank.IsLoaded)
        {
            TextureFolderBank.LoadTextureFolders();
        }

        CommandQueue.Parse(initcmd);
        EditorShortcuts.Monitor();

        if (TextureFolderBank.IsLoaded)
        {
            if (UI.Current.Interface_TextureViewer_Files)
            {
                FileContainerView.Display();
            }
            if (UI.Current.Interface_TextureViewer_Textures)
            {
                TextureListView.Display();
            }
            if (UI.Current.Interface_TextureViewer_Viewer)
            {
                TextureViewport.Display();
            }
            if (UI.Current.Interface_TextureViewer_Properties)
            {
                TexturePropertyView.Display();
            }
        }

        if(UI.Current.Interface_TextureViewer_ToolConfiguration)
        {
            ToolWindow.Display();
        }

        if (UI.Current.Interface_TextureViewer_ResourceList)
        {
            ResourceListWindow.DisplayWindow("textureViewerResourceList");
        }

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    // <summary>
    /// Reset editor state on project change
    /// </summary>
    public void OnProjectChanged()
    {
        EditorActionManager.Clear();

        FileContainerView.OnProjectChanged();
        TextureListView.OnProjectChanged();
        TextureViewport.OnProjectChanged();
        TexturePropertyView.OnProjectChanged();

        Selection.OnProjectChanged();
        Filters.OnProjectChanged();
        ImagePreview.OnProjectChanged();
        ShoeboxLayouts.OnProjectChanged();
        TextureFolderBank.OnProjectChanged();
    }

    public void Save()
    {
        // Nothing
    }

    public void SaveAll()
    {
        // Nothing
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
