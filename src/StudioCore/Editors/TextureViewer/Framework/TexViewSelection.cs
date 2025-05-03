using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Resource.Types;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;

namespace StudioCore.Editors.TextureViewer;

public class TexViewSelection
{
    private TextureViewerScreen Editor;

    private Task LoadingTask;

    // Texture Viewer
    public TextureFolderBank.TextureViewInfo _selectedTextureContainer;
    public string _selectedTextureContainerKey = "";
    public ResourceDescriptor _selectedAssetDescription;

    public string _selectedTextureKey = "";
    public TPF.Texture _selectedTexture;

    public TextureResource CurrentTextureInView;
    public string CurrentTextureName;
    public string CurrentTextureContainerName;

    public bool InvalidateCachedName = false;

    public bool SelectTexture = false;
    public bool SelectFile = false;

    // Texture Viewport
    public Vector2 TextureViewWindowPosition = new Vector2(0, 0);
    public Vector2 TextureViewScrollPosition = new Vector2(0, 0);

    // Image Preview
    public TextureResource CurrentPreviewTexture;
    public string CurrentPreviewTextureName;
    public string CurrentPreviewTextureContainerName;

    public TextureFolderBank.TextureViewInfo SelectedPreviewTextureContainer;
    public string SelectedPreviewTextureContainerKey = "";
    public ResourceDescriptor SelectedPreviewTextureDescriptor;

    public string SelectedPreviewTextureKey = "";
    public TPF.Texture SelectedPreviewTexture;

    public SubTexture CurrentPreviewSubTexture;

    public TexViewSelection(TextureViewerScreen screen)
    {
        Editor = screen;
    }

    // <summary>
    /// Reset selection state on project change
    /// </summary>
    public void OnProjectChanged()
    {
        // Texture Viewer
        if (CurrentTextureInView != null)
            CurrentTextureInView.Dispose();

        _selectedTextureContainer = null;
        _selectedTextureContainerKey = "";
        _selectedAssetDescription = null;

        _selectedTextureKey = "";
        _selectedTexture = null;

        // Image Preview
        if (CurrentPreviewTexture != null)
            CurrentPreviewTexture.Dispose();

        CurrentPreviewTextureContainerName = null;
        CurrentPreviewTextureName = null;
        CurrentPreviewSubTexture = null;

        SelectedPreviewTextureContainer = null;
        SelectedPreviewTextureContainerKey = "";
        SelectedPreviewTextureDescriptor = null;

        SelectedPreviewTextureKey = "";
        SelectedPreviewTexture = null;
    }

    /// <summary>
    /// Load passed texture container
    /// </summary>
    /// <param name="info"></param>
    public void SelectTextureContainer(TextureViewInfo info)
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
            ad = TextureLocator.GetMenuTextureContainer(Editor.Project, _selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Asset)
        {
            ad = TextureLocator.GetAssetTextureContainer(Editor.Project, _selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Object)
        {
            ad = TextureLocator.GetObjTextureContainer(Editor.Project, _selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Part)
        {
            var isLowDetail = false;
            var name = info.Path;
            if (name.Contains("_l."))
            {
                isLowDetail = true;
            }

            ad = TextureLocator.GetPartTextureContainer(Editor.Project, _selectedTextureContainerKey, isLowDetail);
        }

        if (info.Category == TextureViewCategory.Other)
        {
            ad = TextureLocator.GetOtherTextureContainer(Editor.Project, _selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Particle)
        {
            ad = TextureLocator.GetParticleTextureContainer(Editor.Project, _selectedTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Character)
        {
            var chrId = _selectedTextureContainerKey;

            var isLowDetail = false;
            if (chrId.Substring(chrId.Length - 2, 2) == "_l")
            {
                isLowDetail = true;
            }

            if (Editor.Project.ProjectType is ProjectType.ER)
            {
                chrId = chrId.Substring(0, chrId.Length - 2); // remove the _h
            }

            ad = TextureLocator.GetChrTextures(Editor.Project, chrId, isLowDetail);
        }

        if (ad != null)
        {
            _selectedAssetDescription = ad;

            // Load direct file
            if (ad.AssetVirtualPath != null)
            {
                if (!ResourceManager.IsResourceLoaded(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
                {
                    if (ad.AssetVirtualPath != null)
                    {
                        job.AddLoadFileTask(ad.AssetVirtualPath, AccessLevel.AccessGPUOptimizedOnly, true);
                    }

                    LoadingTask = job.Complete();
                }

                ResourceManager.AddResourceListener<TextureResource>(ad.AssetVirtualPath, Editor, AccessLevel.AccessGPUOptimizedOnly);

                _selectedTextureContainer.Textures = TPF.Read(info.Path).Textures;
            }

            // Load bnd archive
            if (ad.AssetArchiveVirtualPath != null)
            {
                if (!ResourceManager.IsResourceLoaded(ad.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly))
                {
                    if (ad.AssetArchiveVirtualPath != null)
                    {
                        job.AddLoadArchiveTask(ad.AssetArchiveVirtualPath, AccessLevel.AccessGPUOptimizedOnly, false, ResourceManager.ResourceType.Texture, null, true);
                    }

                    LoadingTask = job.Complete();
                }

                ResourceManager.AddResourceListener<TextureResource>(ad.AssetArchiveVirtualPath, Editor, AccessLevel.AccessGPUOptimizedOnly);


                _selectedTextureContainer.Textures = GetTexturesFromBinder(info.Path);
            }
        }
    }

    /// <summary>
    /// Get textures from passed binder path
    /// </summary>
    public List<TPF.Texture> GetTexturesFromBinder(string path)
    {
        List<TPF.Texture> textures = new List<TPF.Texture>();

        if (path == null || !File.Exists(path))
        {
            return textures;
        }

        var reader = ResourceManager.InstantiateBinderReaderForFile(path, Editor.Project.ProjectType);
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

    public TextureViewerContext CurrentWindowContext = TextureViewerContext.None;

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SwitchWindowContext(TextureViewerContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            CurrentWindowContext = newContext;
            //TaskLogs.AddLog($"Context: {newContext.GetDisplayName()}");
        }
    }
}
