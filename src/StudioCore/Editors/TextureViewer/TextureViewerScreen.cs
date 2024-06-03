using Andre.Formats;
using DotNext;
using ImGuiNET;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SoulsFormats;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors;
using StudioCore.Editors.GparamEditor.Toolbar;
using StudioCore.Editors.GraphicsEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TextureViewer.Toolbar;
using StudioCore.Formats;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Resource;
using StudioCore.Settings;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using static Octokit.Caching.CachedResponse;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;

namespace StudioCore.TextureViewer;

public class TextureViewerScreen : EditorScreen, IResourceEventListener
{
    public bool FirstFrame { get; set; }

    public ActionManager EditorActionManager = new();

    private static string _fileSearchInput = "";
    private static string _fileSearchInputCache = "";

    private static TextureFolderBank.TextureViewInfo _selectedTextureContainer;
    private static string _selectedTextureContainerKey = "";
    private static ResourceDescriptor _selectedAssetDescription;

    private static string _textureSearchInput = "";
    private static string _textureSearchInputCache = "";

    private static string _selectedTextureKey = "";
    private static TPF.Texture _selectedTexture;

    public static TextureResource CurrentTextureInView;
    public static string CurrentTextureName;
    public static string CurrentTextureContainerName;

    private Task _loadingTask;

    public TextureToolbar _textureToolbar;
    public TextureToolbar_ActionList _textureToolbar_ActionList;
    public TextureToolbar_Configuration _textureToolbar_Configuration;

    private static Dictionary<string, string> ContainerNameCache = new Dictionary<string, string>();

    private SubTexture _cachedPreviewSubtexture;

    public TextureViewerScreen(Sdl2Window window, GraphicsDevice device)
    {
        _textureToolbar = new TextureToolbar();
        _textureToolbar_ActionList = new TextureToolbar_ActionList();
        _textureToolbar_Configuration = new TextureToolbar_Configuration();
    }

    public string EditorName => "Texture Viewer##TextureViewerEditor";
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";

    public void Init()
    {

    }

    private List<AliasReference> _chrNameCache = new List<AliasReference>();
    private List<AliasReference> _objNameCache = new List<AliasReference>();
    private List<AliasReference> _partNameCache = new List<AliasReference>();
    private List<AliasReference> _sfxNameCache = new List<AliasReference>();

    private ShoeboxLayoutContainer shoeboxContainer = null;

    public void OnProjectChanged()
    {
        _chrNameCache = ModelAliasBank.Bank.AliasNames.GetEntries("Characters");
        _objNameCache = ModelAliasBank.Bank.AliasNames.GetEntries("Objects");
        _partNameCache = ModelAliasBank.Bank.AliasNames.GetEntries("Parts");
        _sfxNameCache = ParticleAliasBank.Bank.AliasNames.GetEntries("Particles");

        if (Project.Type is ProjectType.ER or ProjectType.AC6)
        {
            string sourcePath = $@"menu\hi\01_common.sblytbnd.dcx";
            if(File.Exists($@"{Project.GameModDirectory}\{sourcePath}"))
            {
                sourcePath = $@"{Project.GameModDirectory}\{sourcePath}";
            }
            else
            {
                sourcePath = $@"{Project.GameRootDirectory}\{sourcePath}";
            }

            if (File.Exists(sourcePath))
            {
                shoeboxContainer = new ShoeboxLayoutContainer(sourcePath);
                shoeboxContainer.BuildTextureDictionary();
            }
            else
            {
                TaskLogs.AddLog($"Failed to load Shoebox Layout: {sourcePath}");
            }
        }
        else
        {
            // TODO: add support for 'custom' Shoebox layouts to the older games to map out their icons for usage with the Image Preview feature
        }

        ResetTextureViewer();

        ResetActionManager();
    }

    public static void ResetTextureViewer()
    {
        TextureFolderBank.LoadTextureFolders();

        _fileSearchInput = "";
        _fileSearchInputCache = "";

        _selectedTextureContainer = null;
        _selectedTextureContainerKey = "";
        _selectedAssetDescription = null;

        _textureSearchInput = "";
        _textureSearchInputCache = "";

        _selectedTextureKey = "";
        _selectedTexture = null;

        ContainerNameCache = new Dictionary<string, string>();
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

            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Link}");
            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_TextureViewer_ResourceList = !CFG.Current.Interface_TextureViewer_ResourceList;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextureViewer_ResourceList);

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

        if (!TextureFolderBank.IsLoaded)
        {
            TextureFolderBank.LoadTextureFolders();
        }

        // Commands
        if (initcmd != null && initcmd.Length > 1)
        {
            // View Image:
            // e.g. "texture/view/01_common/SB_GarageTop_04"
            if (initcmd[0] == "view" && initcmd.Length >= 3)
            {
                LoadTextureContainer(initcmd[1]);
                LoadTextureFile(initcmd[2]);
            }
        }

        TextureViewerShortcuts();

        if (TextureFolderBank.IsLoaded)
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

        if(CFG.Current.Interface_TextureViewer_Toolbar)
        {
            _textureToolbar_ActionList.OnGui();
            _textureToolbar_Configuration.OnGui();
        }

        if (CFG.Current.Interface_TextureViewer_ResourceList)
        {
            ResourceManager.OnGuiDrawResourceList("textureViewerResourceList");
        }

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

        if (Project.Type is ProjectType.AC6 or ProjectType.ER)
        {
            DisplayFileSection("Asset", TextureViewCategory.Asset);
        }
        else
        {
            DisplayFileSection("Object", TextureViewCategory.Object);
        }

        DisplayFileSection("Characters", TextureViewCategory.Character);

        // AC6 needs some adjustments to support its parts properly
        if (Project.Type != ProjectType.AC6)
        {
            DisplayFileSection("Parts", TextureViewCategory.Part);
        }

        DisplayFileSection("Particles", TextureViewCategory.Particle);

        DisplayFileSection("Menu", TextureViewCategory.Menu);

        // DS2S doesn't have an other folder
        if (Project.Type != ProjectType.DS2S && Project.Type != ProjectType.DS2)
        {
            DisplayFileSection("Other", TextureViewCategory.Other);
        }

        ImGui.End();
    }

    private void DisplayFileSection(string title, TextureViewCategory displayCategory)
    {
        if (ImGui.CollapsingHeader($"{title}"))
        {
            foreach (var (name, info) in TextureFolderBank.FolderBank)
            {
                if (info.Category == displayCategory)
                {
                    if (SearchFilters.IsEditorSearchMatch(_fileSearchInput, info.Name, "_"))
                    {
                        ImGui.BeginGroup();

                        var displayName = info.Name;

                        if (CFG.Current.TextureViewer_FileList_ShowAliasName)
                        {
                            if (ContainerNameCache.ContainsKey(info.Name))
                            {
                                displayName = ContainerNameCache[info.Name];
                            }
                            else
                            {
                                displayName = GetContainerDisplayName(info.Name, displayCategory);
                                ContainerNameCache.Add(info.Name, displayName);
                            }
                        }

                        if (ImGui.Selectable($@" {displayName}", info.Name == _selectedTextureContainerKey))
                        {
                            SelectTextureContainer(info);
                        }

                        ImGui.EndGroup();
                    }
                }
            }

        }
    }

    private string GetContainerDisplayName(string containerName, TextureViewCategory displayCategory)
    {
        List<string> _nameCache = null;
        string newName = containerName;

        // Characters
        if (displayCategory == TextureViewCategory.Character)
        {
            newName = AppendAliasToName(newName, _chrNameCache, displayCategory);
        }

        // Object
        if (displayCategory == TextureViewCategory.Asset || displayCategory == TextureViewCategory.Object)
        {
            newName = AppendAliasToName(newName, _objNameCache, displayCategory);
        }

        // Parts
        if (displayCategory == TextureViewCategory.Part)
        {
            newName = AppendAliasToName(newName, _partNameCache, displayCategory);
        }

        // Particles
        if (displayCategory == TextureViewCategory.Particle)
        {
            newName = AppendAliasToName(newName, _sfxNameCache, displayCategory);
        }

        // MapPieces
        // Not supported yet

        return newName;
    }

    private string AppendAliasToName(string name, List<AliasReference> nameCache, TextureViewCategory displayCategory)
    {
        var displayedName = $"{name}";

        foreach (var entry in nameCache)
        {
            // Convert the texture usage of aet to aeg to match with the stored aliases.
            if(displayCategory == TextureViewCategory.Asset)
            {
                name = name.Replace("aet", "aeg");
            }

            if (name.Contains(entry.id))
            {
                displayedName = displayedName + $" {{ {entry.name} }}";
                break;
            }
        }

        return displayedName;
    }

    private void SelectTextureContainer(TextureViewInfo info)
    {
        // Ignore this if it is already loaded (e.g. same entry is double clicked)
        if (_selectedTextureContainer == info)
            return;

        _selectedTextureContainerKey = info.Name;
        _selectedTextureContainer = info;
        CurrentTextureContainerName = _selectedTextureContainerKey;

        // TODO: fix issue with selection causing the job to unload at the same time it is loaded (only occurs every second press)

        ResourceManager.UnloadPersistentTextures();

        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob($@"Loading {info.Name} textures");

        ResourceDescriptor ad = null;

        if (info.Category == TextureViewCategory.Menu)
        {
            ad = ResourceTextureLocator.GetMenuTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Asset)
        {
            ad = ResourceTextureLocator.GetAssetTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Object)
        {
            ad = ResourceTextureLocator.GetObjTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Part)
        {
            var isLowDetail = false;
            var name = info.Path;
            if (name.Contains("_l."))
            {
                isLowDetail = true;
            }

            ad = ResourceTextureLocator.GetPartTextureContainer(_selectedTextureContainerKey, isLowDetail);
        }

        if (info.Category == TextureViewCategory.Other)
        {
            ad = ResourceTextureLocator.GetOtherTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Particle)
        {
            ad = ResourceTextureLocator.GetParticleTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Character)
        {
            var chrId = _selectedTextureContainerKey;

            var isLowDetail = false;
            if (chrId.Substring(chrId.Length - 2, 2) == "_l")
            {
                isLowDetail = true;
            }

            if (Project.Type is ProjectType.ER)
            {
                chrId = chrId.Substring(0, chrId.Length - 2); // remove the _h
            }

            ad = ResourceTextureLocator.GetChrTextures(chrId, isLowDetail);
        }

        if (ad != null)
        {
            _selectedAssetDescription = ad;

            // Load direct file
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

                _selectedTextureContainer.Textures = TPF.Read(info.Path).Textures;
            }

            // Load bnd archive
            if (ad.AssetArchiveVirtualPath != null)
            {
                if (!ResourceManager.IsResourceLoadedOrInFlight(ad.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
                {
                    if (ad.AssetArchiveVirtualPath != null)
                    {
                        job.AddLoadArchiveTask(ad.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceManager.ResourceType.Texture, null, true);
                    }

                    _loadingTask = job.Complete();
                }

                ResourceManager.AddResourceListener<TextureResource>(ad.AssetArchiveVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);


                _selectedTextureContainer.Textures = GetTexturesFromBinder(info.Path);
            }
        }
    }

    private List<TPF.Texture> GetTexturesFromBinder(string path)
    {
        List<TPF.Texture> textures = new List<TPF.Texture>();

        if (path == null || !File.Exists(path))
        {
            return textures;
        }

        var reader = ResourceManager.InstantiateBinderReaderForFile(path, Project.Type);
        if (reader != null)
        {
            foreach (var file in reader.Files)
            {
                Memory<byte> bytes = reader.ReadFile(file);

                if (file.Name.Contains(".tpf"))
                {
                    var tpf = TPF.Read(bytes);
                    foreach (var tex in tpf.Textures)
                    {
                        textures.Add(tex);
                    }
                }
            }
        }

        return textures;
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

        if (_selectedTextureContainer != null && _selectedTextureContainerKey != "")
        {
            TextureViewInfo data = _selectedTextureContainer;

            ImGui.Text($"Textures");
            ImGui.Separator();

            if (data.Textures != null)
            {
                foreach (var tex in data.Textures)
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
        }

        ImGui.End();
    }

    private Vector2 TextureViewWindowPosition = new Vector2(0, 0);
    private Vector2 TextureViewScrollPosition = new Vector2(0, 0);

    private void TextureViewer()
    {
        ImGui.Begin("Viewer##TextureViewer", ImGuiWindowFlags.AlwaysHorizontalScrollbar | ImGuiWindowFlags.AlwaysVerticalScrollbar);

        TextureViewWindowPosition = ImGui.GetWindowPos();
        TextureViewScrollPosition = new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());

        ResourceHandle<TextureResource> resHandle = GetImageTextureHandle(_selectedTextureKey, _selectedTexture, _selectedAssetDescription);

        if (resHandle != null)
        {
            TextureResource texRes = resHandle.Get();

            CurrentTextureInView = texRes;
            CurrentTextureName = _selectedTextureKey;

            if (texRes != null)
            {
                IntPtr handle = (nint)texRes.GPUTexture.TexHandle;
                Vector2 size = GetImageSize(texRes, true);

                ImGui.Image(handle, size);
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

        ImguiUtils.WrappedText($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
        ImguiUtils.WrappedText($"Press {KeyBindings.Current.TextureViewer_ZoomReset.HintText} to reset zoom level to 100%.");

        ImguiUtils.WrappedText($"");
        ImguiUtils.WrappedText($"Properties of {CurrentTextureName}:");

        if (_selectedTexture != null)
        {
            if (CurrentTextureInView != null)
            {
                Vector2 size = GetImageSize(CurrentTextureInView, false);
                Vector2 relativePos = GetRelativePosition(size, TextureViewWindowPosition, TextureViewScrollPosition);

                ImGui.Columns(2);

                ImGui.Text("Width:");
                ImGui.Text("Height:");
                ImGui.Text("Format:");

                ImGui.NextColumn();

                ImGui.Text($"{size.X}");
                ImGui.Text($"{size.Y}");

                if (CurrentTextureInView.GPUTexture != null)
                {
                    ImGui.Text($"{CurrentTextureInView.GPUTexture.Format}".ToUpper());
                }
                ImGui.Columns(1);

                ImGui.Text("");
                ImGui.Text($"Relative Position: {relativePos}");

                if(shoeboxContainer != null)
                {
                    if(shoeboxContainer.Textures.ContainsKey(CurrentTextureName))
                    {
                        var subTexs = shoeboxContainer.Textures[CurrentTextureName];
                        foreach(var entry in subTexs)
                        {
                            string IconName;
                            bool IsMatch;
                            (IconName, IsMatch) = MatchMousePosToIcon(entry, relativePos);

                            if (IsMatch)
                            {
                                ImGui.Text($"Icon: {IconName}");
                            }
                        }
                    }
                }
            }
        }

        ImGui.End();
    }

    private (string, bool) MatchMousePosToIcon(SubTexture entry, Vector2 relativePos)
    {
        var cursorPos = relativePos;

        var SubTexName = entry.Name.Replace(".png", "");

        var success = false;

        float Xmin = float.Parse(entry.X);
        float Xmax = (Xmin + float.Parse(entry.Width));
        float Ymin = float.Parse(entry.Y);
        float Ymax = (Ymin + float.Parse(entry.Height));

        if (cursorPos.X > Xmin && cursorPos.X < Xmax && cursorPos.Y > Ymin && cursorPos.Y < Ymax)
        {
            success = true;
        }

        return (SubTexName, success);
    }

    private Vector2 GetRelativePosition(Vector2 imageSize, Vector2 windowPos, Vector2 scrollPos)
    {
        Vector2 relativePos = new Vector2(0, 0);

        var fixedX = 3;
        var fixedY = 24;
        var cursorPos = ImGui.GetMousePos();

        // Account for window positiona and scroll
        relativePos.X = cursorPos.X - ((windowPos.X + fixedX) - scrollPos.X);
        relativePos.Y = cursorPos.Y - ((windowPos.Y + fixedY) - scrollPos.Y);

        // Account for zoom
        relativePos.X = relativePos.X / zoomFactor.X;
        relativePos.Y = relativePos.Y / zoomFactor.Y;

        return relativePos;
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

    public void LoadTextureContainer(string container)
    {
        if (_selectedTextureContainerKey != container)
        {
            foreach (var (name, info) in TextureFolderBank.FolderBank)
            {
                if (name == container)
                {
                    _selectedTextureContainerKey = name;
                    SelectTextureContainer(info);
                }
            }
        }
    }

    public void LoadTextureFile(string filename)
    {
        if (_selectedTextureContainerKey != filename)
        {
            if (_selectedTextureContainer != null && _selectedTextureContainerKey != "")
            {
                TextureViewInfo data = _selectedTextureContainer;

                if (data.Textures != null)
                {
                    foreach (var tex in data.Textures)
                    {
                        if (tex.Name == filename)
                        {
                            _selectedTextureKey = tex.Name;
                            _selectedTexture = tex;

                            // Clear this when a new texture is loaded
                            _cachedPreviewSubtexture = null;
                        }
                    }
                }
            }
        }
    }

    public ResourceHandle<TextureResource> GetImageTextureHandle(string key, TPF.Texture texture, ResourceDescriptor desc)
    {
        if (texture != null)
        {
            var path = desc.AssetVirtualPath;

            if (desc.AssetArchiveVirtualPath != null)
            {
                path = desc.AssetArchiveVirtualPath;
            }
            var virtName = $@"{path}/{key}".ToLower();

            var resources = ResourceManager.GetResourceDatabase();

            if (resources.ContainsKey(virtName))
            {
                return (ResourceHandle<TextureResource>)resources[virtName];
            }
        }

        return null;
    }

    public SubTexture GetMatchingSubTexture(string currentTextureName, string imageIndex, string namePrepend)
    {
        if (shoeboxContainer != null)
        {
            if (shoeboxContainer.Textures.ContainsKey(currentTextureName))
            {
                var subTexs = shoeboxContainer.Textures[currentTextureName];

                int matchId;
                var successMatch = int.TryParse(imageIndex, out matchId);

                foreach (var entry in subTexs)
                {
                    var SubTexName = entry.Name.Replace(".png", "");

                    Match contents = Regex.Match(SubTexName, $@"{namePrepend}([0-9]+)");
                    if (contents.Success)
                    {
                        var id = contents.Groups[1].Value;

                        int numId;
                        var successNum = int.TryParse(id, out numId);

                        if (successMatch && successNum && matchId == numId)
                        {
                            return entry;
                        }
                    }
                }
            }
        }

        return null;
    }

    public bool ShowImagePreview(Param.Row context, TexRef textureRef, bool displayImage = true)
    {
        // Display the texture
        LoadTextureContainer(textureRef.TextureContainer);
        LoadTextureFile(textureRef.TextureFile);
        ResourceHandle<TextureResource> resHandle = GetImageTextureHandle(_selectedTextureKey, _selectedTexture, _selectedAssetDescription);

        // Display Image
        if (resHandle != null)
        {
            TextureResource texRes = resHandle.Get();

            CurrentTextureInView = texRes;
            CurrentTextureName = _selectedTextureKey;

            // Get the SubTexture that matches the current field value
            if (_cachedPreviewSubtexture == null)
            {
                _cachedPreviewSubtexture = GetPreviewSubTexture(context, textureRef);
            }

            if (texRes != null && _cachedPreviewSubtexture != null)
            {
                IntPtr handle = (nint)texRes.GPUTexture.TexHandle;

                // Get scaled image size vector
                var scale = CFG.Current.Param_FieldContextMenu_ImagePreviewScale;

                // Get crop bounds
                float Xmin = float.Parse(_cachedPreviewSubtexture.X);
                float Xmax = Xmin + float.Parse(_cachedPreviewSubtexture.Width);
                float Ymin = float.Parse(_cachedPreviewSubtexture.Y);
                float Ymax = Ymin + float.Parse(_cachedPreviewSubtexture.Height);

                // Image size should be based on cropped image
                Vector2 size = new Vector2(Xmax-Xmin, Ymax-Ymin) * scale;

                // Get UV coordinates based on full image
                float left = (Xmin) / texRes.GPUTexture.Width; 
                float top = (Ymin) / texRes.GPUTexture.Height; 
                float right = (Xmax) / texRes.GPUTexture.Width; 
                float bottom = (Ymax) / texRes.GPUTexture.Height; 

                // Build UV coordinates
                var UV0 = new Vector2(left, top);
                var UV1 = new Vector2(right, bottom);

                if (CFG.Current.Param_FieldContextMenu_ImagePreview_ContextMenu)
                {
                    displayImage = true;
                }

                // Display image
                if (displayImage)
                {
                    ImGui.Image(handle, size, UV0, UV1);
                }

                return true;
            }
        }

        return false;
    }

    private SubTexture GetPreviewSubTexture(Param.Row context, TexRef textureRef)
    {
        // Guard clauses checking the validity of the TextureRef
        if (context[textureRef.TargetField] == null)
        {
            return null;
        }

        var imageIdx = $"{context[textureRef.TargetField].Value.Value}";

        SubTexture subTex = null;

        // Dynamic lookup based on meta information
        if (textureRef.LookupType == "Direct")
        {
            subTex = GetMatchingSubTexture(CurrentTextureName, imageIdx, textureRef.SubTexturePrefix);
        }

        // Hardcoded logic for AC6
        if (Project.Type == ProjectType.AC6)
        {
            if (textureRef.LookupType == "Booster")
            {
                subTex = GetMatchingSubTexture(CurrentTextureName, imageIdx, "BS_A_");
            }
            if (textureRef.LookupType == "Weapon")
            {
                // Check for WP_A_ match
                subTex = GetMatchingSubTexture(CurrentTextureName, imageIdx, "WP_A_");

                // If failed, check for WP_R_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(CurrentTextureName, imageIdx, "WP_R_");
                }

                // If failed, check for WP_L_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(CurrentTextureName, imageIdx, "WP_L_");
                }
            }
            if (textureRef.LookupType == "Armor")
            {
                var prefix = "";

                var headEquip = context["headEquip"].Value.Value.ToString();
                var bodyEquip = context["bodyEquip"].Value.Value.ToString();
                var armEquip = context["armEquip"].Value.Value.ToString();
                var legEquip = context["legEquip"].Value.Value.ToString();

                if (headEquip == "1")
                {
                    prefix = "HD_M_";
                }
                if (bodyEquip == "1")
                {
                    prefix = "BD_M_";
                }
                if (armEquip == "1")
                {
                    prefix = "AM_M_";
                }
                if (legEquip == "1")
                {
                    prefix = "LG_M_";
                }

                // Check for match
                subTex = GetMatchingSubTexture(CurrentTextureName, imageIdx, prefix);
            }
        }

        // Hardcoded logic for ER
        if (Project.Type == ProjectType.ER)
        {

        }

        return subTex;
    }
}
