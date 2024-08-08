using Andre.Formats;
using ImGuiNET;
using Silk.NET.Core;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TextureViewer.Actions;
using StudioCore.Editors.TextureViewer.Tools;
using StudioCore.Formats;
using StudioCore.Interface;
using StudioCore.Locators;
using StudioCore.Resource;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Veldrid;
using Veldrid.Sdl2;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;

namespace StudioCore.TextureViewer;

public class TextureViewerScreen : EditorScreen, IResourceEventListener
{
    public bool FirstFrame { get; set; }

    public bool ShowSaveOption { get; set; }

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

    public ToolWindow ToolWindow;
    public ToolSubMenu ToolSubMenu;
    public ActionSubMenu ActionSubMenu;

    private SubTexture _cachedPreviewSubtexture;

    public TextureImagePreview ImagePreview;

    public TextureViewerScreen(Sdl2Window window, GraphicsDevice device)
    {
        ToolWindow = new ToolWindow(this);
        ToolSubMenu = new ToolSubMenu(this);
        ActionSubMenu = new ActionSubMenu(this);
        ImagePreview = new TextureImagePreview();
    }

    public string EditorName => "Texture Viewer##TextureViewerEditor";
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";

    public void Init()
    {
        ShowSaveOption = false;
    }

    private ShoeboxLayoutContainer shoeboxContainer = null;

    public void OnProjectChanged()
    {
        ImagePreview.OnProjectChanged();

        if (Smithbox.ProjectType != ProjectType.Undefined)
        {
            ToolWindow.OnProjectChanged();
            ToolSubMenu.OnProjectChanged();
            ActionSubMenu.OnProjectChanged();
        }

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            string sourcePath = $@"menu\hi\01_common.sblytbnd.dcx";
            if(File.Exists($@"{Smithbox.ProjectRoot}\{sourcePath}"))
            {
                sourcePath = $@"{Smithbox.ProjectRoot}\{sourcePath}";
            }
            else
            {
                sourcePath = $@"{Smithbox.GameRoot}\{sourcePath}";
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
    }

    public void DrawEditorMenu()
    {
        ImGui.Separator();

        ActionSubMenu.DisplayMenu();

        ImGui.Separator();

        ToolSubMenu.DisplayMenu();

        ImGui.Separator();

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
            if (ImGui.MenuItem("Tool Window"))
            {
                CFG.Current.Interface_TextureViewer_ToolConfiguration = !CFG.Current.Interface_TextureViewer_ToolConfiguration;
            }
            ImguiUtils.ShowActiveStatus(CFG.Current.Interface_TextureViewer_ToolConfiguration);

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
        
        if (Smithbox.ProjectHandler.CurrentProject == null)
        {
            ImGui.Begin("Viewer##InvalidTextureViewer");

            ImGui.Text("No project loaded. File -> New Project");

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

        ActionSubMenu.Shortcuts();
        ToolSubMenu.Shortcuts();

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

        if(CFG.Current.Interface_TextureViewer_ToolConfiguration)
        {
            ToolWindow.OnGui();
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

        if (Smithbox.ProjectType is ProjectType.AC6 or ProjectType.ER)
        {
            DisplayFileSection("Asset", TextureViewCategory.Asset);
        }
        else
        {
            DisplayFileSection("Object", TextureViewCategory.Object);
        }

        DisplayFileSection("Characters", TextureViewCategory.Character);

        // AC6 needs some adjustments to support its parts properly
        if (Smithbox.ProjectType != ProjectType.AC6)
        {
            DisplayFileSection("Parts", TextureViewCategory.Part);
        }

        DisplayFileSection("Particles", TextureViewCategory.Particle);

        DisplayFileSection("Menu", TextureViewCategory.Menu);

        // DS2S doesn't have an other folder
        if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
        {
            DisplayFileSection("Other", TextureViewCategory.Other);
        }

        ImGui.End();
    }

    public bool InvalidateCachedName = false;

    private void DisplayFileSection(string title, TextureViewCategory displayCategory)
    {
        if (ImGui.CollapsingHeader($"{title}"))
        {
            foreach (var (name, info) in TextureFolderBank.FolderBank)
            {
                // Skip if info is null
                if (info == null)
                    continue;

                if(InvalidateCachedName)
                {
                    info.CachedName = null;
                }

                if (info.Category == displayCategory)
                {
                    var rawName = info.Name.ToLower();
                    var aliasName = "";

                    switch(displayCategory)
                    {
                        case TextureViewCategory.Character:
                            aliasName = AliasUtils.GetCharacterAlias(rawName);
                            break;
                        case TextureViewCategory.Asset:
                            aliasName = AliasUtils.GetAssetAlias(rawName);
                            break;
                        case TextureViewCategory.Part:
                            aliasName = AliasUtils.GetPartAlias(rawName);
                            break;
                    }

                    //TaskLogs.AddLog(aliasName);

                    if (SearchFilters.IsTextureSearchMatch(_fileSearchInput, info.Name, "_", aliasName))
                    {
                        if(!CFG.Current.TextureViewer_FileList_ShowLowDetail_Entries)
                        {
                            if(info.Name.Contains("_l"))
                            {
                                continue;
                            }
                        }

                        ImGui.BeginGroup();

                        var displayName = info.Name;

                        if (ImGui.Selectable($@" {displayName}", info.Name == _selectedTextureContainerKey))
                        {
                            SelectTextureContainer(info);
                        }
                        var alias = AliasUtils.GetTextureContainerAliasName(info);
                        AliasUtils.DisplayAlias(alias);

                        ImGui.EndGroup();
                    }
                }
            }
        }
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
            ad = TextureLocator.GetMenuTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Asset)
        {
            ad = TextureLocator.GetAssetTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Object)
        {
            ad = TextureLocator.GetObjTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Part)
        {
            var isLowDetail = false;
            var name = info.Path;
            if (name.Contains("_l."))
            {
                isLowDetail = true;
            }

            ad = TextureLocator.GetPartTextureContainer(_selectedTextureContainerKey, isLowDetail);
        }

        if (info.Category == TextureViewCategory.Other)
        {
            ad = TextureLocator.GetOtherTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Particle)
        {
            ad = TextureLocator.GetParticleTextureContainer(_selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Character)
        {
            var chrId = _selectedTextureContainerKey;

            var isLowDetail = false;
            if (chrId.Substring(chrId.Length - 2, 2) == "_l")
            {
                isLowDetail = true;
            }

            if (Smithbox.ProjectType is ProjectType.ER)
            {
                chrId = chrId.Substring(0, chrId.Length - 2); // remove the _h
            }

            ad = TextureLocator.GetChrTextures(chrId, isLowDetail);
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

        var reader = ResourceManager.InstantiateBinderReaderForFile(path, Smithbox.ProjectType);
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

        // Account for window position and scroll
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
}
