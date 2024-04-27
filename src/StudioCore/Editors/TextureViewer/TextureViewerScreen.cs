using ImGuiNET;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SoulsFormats;
using StudioCore.AssetLocator;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.TextureViewer;
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

    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    private string _fileSearchInput = "";
    private string _fileSearchInputCache = "";

    private TextureViewBank.TextureViewInfo _selectedTextureView;
    private string _selectedTextureViewKey;

    private string _textureSearchInput = "";
    private string _textureSearchInputCache = "";

    private string _selectedTextureKey;
    private TPF.Texture _selectedTexture;

    private Task _loadingTask;

    public TextureViewerScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Texture Viewer##TextureViewerEditor";
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";

    public void Init()
    {

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

        ImGui.PopStyleVar();
        ImGui.PopStyleColor(1);
    }

    public void TextureViewerShortcuts()
    {

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

        foreach (var (name, info) in TextureViewBank.TextureBank)
        {
            if (SearchFilters.IsEditorSearchMatch(_fileSearchInput, info.Name, "_"))
            {
                ImGui.BeginGroup();
                if (ImGui.Selectable($@" {info.Name}", info.Name == _selectedTextureViewKey))
                {
                    ResourceManager.UnloadMenuTextures();

                    _selectedTextureViewKey = info.Name;
                    _selectedTextureView = info;

                    LoadSelectedTextureContainer();
                }

                ImGui.EndGroup();
            }
        }

        ImGui.End();
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
                if (ImGui.Selectable($@" {tex.Name}", tex.Name == _selectedTextureKey))
                {
                    _selectedTextureKey = tex.Name;
                    _selectedTexture = tex;
                }
            }
        }

        ImGui.End();
    }

    private void TextureViewer()
    {
        ImGui.Begin("Viewer##TextureViewer");

        if (_selectedTexture != null)
        {
            var virtName = $"menu/{_selectedTextureViewKey}/tex/{_selectedTextureKey}".ToLower();
            var resources = ResourceManager.GetResourceDatabase();

            if (resources.ContainsKey(virtName))
            {
                ResourceHandle<TextureResource> resHandle = (ResourceHandle<TextureResource>)resources[virtName];
                TextureResource texRes = resHandle.Get();

                if (texRes != null)
                {
                    IntPtr handle = (nint)texRes.GPUTexture.TexHandle;
                    Vector2 size = GetImageSize(texRes);

                    ImGui.Image(handle, size);
                }
            }
        }

        ImGui.End();
    }

    private Vector2 GetImageSize(TextureResource texRes)
    {
        var Width = texRes.GPUTexture.Width;
        var Height = texRes.GPUTexture.Height;

        Vector2 size = new Vector2(0, 0);

        if (Height != 0 && Width != 0)
        {
            size = new Vector2(Width, Height);
        }

        return size;
    }

    private void TextureProperties()
    {

    }

    private void LoadSelectedTextureContainer()
    {
        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob(@"Loading menu textures");

        AssetDescription ad = TextureAssetLocator.GetMenuTextures(_selectedTextureViewKey);

        if(ad.AssetVirtualPath != null)
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

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        _selectedTextureViewKey = null;
        _selectedTextureView = null;

        _selectedTextureKey = null;
        _selectedTexture = null;

        ResetActionManager();
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
}
