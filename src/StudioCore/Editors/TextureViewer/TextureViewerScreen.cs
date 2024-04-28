using ImGuiNET;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SoulsFormats;
using StudioCore.AssetLocator;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor.Toolbar;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TextureViewer.Toolbar;
using StudioCore.Interface;
using StudioCore.Resource;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.TextureViewer.TextureViewBank;

namespace StudioCore.TextureViewer;

public class TextureViewerScreen : EditorScreen, IResourceEventListener
{
    public bool FirstFrame { get; set; }

    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private static string _fileSearchInput = "";
    private static string _fileSearchInputCache = "";

    private static TextureViewBank.TextureViewInfo _selectedTextureView;
    private static string _selectedTextureViewKey = "";

    private static string _textureSearchInput = "";
    private static string _textureSearchInputCache = "";

    private static string _selectedTextureKey = "";
    private static TPF.Texture _selectedTexture;

    public static TextureResource CurrentTextureInView;

    private Task _loadingTask;

    public TextureViewerToolbarView _toolbarView;

    public TextureViewerScreen(Sdl2Window window, GraphicsDevice device)
    {
        _toolbarView = new TextureViewerToolbarView(this);
    }

    public string EditorName => "Texture Viewer##TextureViewerEditor";
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";

    public void Init()
    {

    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        ResetTextureViewer();

        ResetActionManager();
    }

    public static void ResetTextureViewer()
    {
        TextureViewBank.LoadTextureFolders();

        _fileSearchInput = "";
        _fileSearchInputCache = "";

        _selectedTextureView = null;
        _selectedTextureViewKey = "";

        _textureSearchInput = "";
        _textureSearchInputCache = "";

        _selectedTextureKey = "";
        _selectedTexture = null;
    }

    public void DrawEditorMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Files"))
            {
                CFG.Current.Interface_TextureViewer_Files = !CFG.Current.Interface_TextureViewer_Files;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Files);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Textures"))
            {
                CFG.Current.Interface_TextureViewer_Textures = !CFG.Current.Interface_TextureViewer_Textures;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Textures);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Viewer"))
            {
                CFG.Current.Interface_TextureViewer_Viewer = !CFG.Current.Interface_TextureViewer_Viewer;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Viewer);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Properties"))
            {
                CFG.Current.Interface_TextureViewer_Properties = !CFG.Current.Interface_TextureViewer_Properties;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Properties);

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Toolbar"))
            {
                CFG.Current.Interface_TextureViewer_Toolbar = !CFG.Current.Interface_TextureViewer_Toolbar;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Toolbar);

            ImGui.EndMenu();
        }
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

        var dsid = ImGui.GetID("DockSpace_TextureViewer");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None);

        if (!TextureViewBank.IsLoaded)
        {
            TextureViewBank.LoadTextureFolders();
        }

        TextureViewerShortcuts();

        if (TextureViewBank.IsLoaded)
        {
            if (CFG.Current.Interface_TextureViewer_Files)
            {
                TextureContainerList();
            }
            if (CFG.Current.Interface_TextureViewer_Textures)
            {
                TextureList();
            }
            if (CFG.Current.Interface_TextureViewer_Viewer)
            {
                TextureViewer();
            }
            if (CFG.Current.Interface_TextureViewer_Properties)
            {
                TextureProperties();
            }
        }

        _toolbarView.OnGui();

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void TextureViewerShortcuts()
    {
        if(InputTracker.GetKey(Key.LControl))
        {
            HandleZoom();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.TextureViewer_ZoomReset))
        {
            ZoomReset();
        }
    }

    /// <summary>
    /// Files list: selectable files
    /// </summary>
    private void TextureContainerList()
    {
        ImGui.Begin("Files##TextureContainerList");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _fileSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_fileSearchInput != _fileSearchInputCache)
        {
            _fileSearchInputCache = _fileSearchInput;
        }

        // Section: MENU
        DisplayFileSection("Menu", CFG.Current.TextureViewer_IncludeTextures_Menu, TextureViewCategory.Menu);

        // Section: Chr

        ImGui.End();
    }

    private void DisplayFileSection(string title, bool displaySection, TextureViewCategory displayCategory)
    {
        if (displaySection)
        {
            ImGui.Separator();
            ImGui.Text($"{title}");
            ImGui.Separator();

            foreach (var (name, info) in TextureViewBank.TextureBank)
            {
                if (info.Category == displayCategory)
                {
                    if (SearchFilters.IsEditorSearchMatch(_fileSearchInput, info.Name, "_"))
                    {
                        ImGui.BeginGroup();
                        if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedTextureViewKey))
                        {
                            SelectTextureContainer(info);
                        }

                        ImGui.EndGroup();
                    }
                }
            }
        }
    }

    private void SelectTextureContainer(TextureViewInfo info)
    {
        _selectedTextureViewKey = info.Name;
        _selectedTextureView = info;

        ResourceManager.UnloadPersistentTextures();

        LoadSelectedTextureContainer(info.Category);
    }

    private void TextureList()
    {
        ImGui.Begin("Textures##TextureViewList");

        ImGui.Separator();

        ImGui.InputText($"Search", ref _textureSearchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Separator();

        if (_textureSearchInput != _textureSearchInputCache)
        {
            _textureSearchInputCache = _textureSearchInput;
        }

        if (_selectedTextureView != null && _selectedTextureViewKey != "")
        {
            TextureViewInfo data = _selectedTextureView;

            ImGui.Text($"Textures");
            ImGui.Separator();

            foreach(var tex in data.Textures)
            {
                if (SearchFilters.IsEditorSearchMatch(_textureSearchInput, tex.Name, "_"))
                {
                    if (ImGui.Selectable($@" {tex.Name}", tex.Name == _selectedTextureKey))
                    {
                        _selectedTextureKey = tex.Name;
                        _selectedTexture = tex;
                    }
                }
            }
        }

        ImGui.End();
    }

    private void TextureViewer()
    {
        ImGui.Begin("Viewer##TextureViewer", ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        if (_selectedTexture != null)
        {
            var virtName = $"menu/{_selectedTextureViewKey}/tex/{_selectedTextureKey}".ToLower();
            var resources = ResourceManager.GetResourceDatabase();

            if (resources.ContainsKey(virtName))
            {
                ResourceHandle<TextureResource> resHandle = (ResourceHandle<TextureResource>)resources[virtName];
                TextureResource texRes = resHandle.Get();

                CurrentTextureInView = texRes;

                if (texRes != null)
                {
                    IntPtr handle = (nint)texRes.GPUTexture.TexHandle;
                    Vector2 size = GetImageSize(texRes, true);

                    ImGui.Image(handle, size);
                }
            }
        }

        ImGui.End();
    }

    private Vector2 GetImageSize(TextureResource texRes, bool includeZoomFactor)
    {
        Vector2 size = new Vector2(0, 0);

        if (texRes.GPUTexture != null)
        {
            var Width = texRes.GPUTexture.Width;
            var Height = texRes.GPUTexture.Height;

            if (Height != 0 && Width != 0)
            {
                if (includeZoomFactor)
                {
                    size = new Vector2((Width * zoomFactor.X), (Height * zoomFactor.Y));
                }
                else
                {
                    size = new Vector2(Width, Height);
                }
            }
        }

        return size;
    }

    private void TextureProperties()
    {
        ImGui.Begin("Properties##PropertiesView");

        if (_selectedTexture != null)
        {
            if (CurrentTextureInView != null)
            {
                Vector2 size = GetImageSize(CurrentTextureInView, false);

                ImGui.Columns(2);

                ImGui.Text("Width:");
                ImGui.Text("Height:");

                ImGui.NextColumn();

                ImGui.Text($"{size.X}");
                ImGui.Text($"{size.Y}");

                ImGui.Columns(1);
            }
        }

        ImGui.End();
    }

    private void LoadSelectedTextureContainer(TextureViewCategory category)
    {
        if (category == TextureViewCategory.Menu)
        {
            ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading menu textures");

            AssetDescription ad = TextureAssetLocator.GetMenuTextures(_selectedTextureViewKey);

            LoadTextureContainer(ad, job);
        }
    }

    private void LoadTextureContainer(AssetDescription ad, ResourceManager.ResourceJobBuilder job)
    {
        if (ad.AssetVirtualPath != null)
        {
            if (!ResourceManager.IsResourceLoadedOrInFlight(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
            {
                if (ad.AssetVirtualPath != null)
                {
                    job.AddLoadFileTask(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly, true);
                }

                _loadingTask = job.Complete();
            }

            ResourceManager.AddResourceListener<TextureResource>(ad.AssetVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);
        }
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

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {
    }

    private Vector2 zoomFactor = new Vector2(1.0f, 1.0f);

    private float zoomFactorStep = 0.1f;

    private void HandleZoom()
    {
        var delta = InputTracker.GetMouseWheelDelta();

        if(delta > 0)
        {
            ZoomIn();
        }
        if (delta < 0)
        {
            ZoomOut();
        }
    }

    private void ZoomIn()
    {
        zoomFactor.X = zoomFactor.X + zoomFactorStep;
        zoomFactor.Y = zoomFactor.Y + zoomFactorStep;

        if (zoomFactor.X > 10.0f)
        {
            zoomFactor.X = 10.0f;
        }
        if (zoomFactor.Y > 10.0f)
        {
            zoomFactor.Y = 10.0f;
        }
    }
    private void ZoomOut()
    {
        zoomFactor.X = zoomFactor.X - zoomFactorStep;
        zoomFactor.Y = zoomFactor.Y - zoomFactorStep;

        if (zoomFactor.X < 0.1f)
        {
            zoomFactor.X = 0.1f;
        }
        if (zoomFactor.Y < 0.1f)
        {
            zoomFactor.Y = 0.1f;
        }
    }
    private void ZoomReset()
    {
        zoomFactor = new Vector2(1.0f, 1.0f);
    }
}
