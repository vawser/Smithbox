using Andre.Formats;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Formats;
using StudioCore.Locators;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// Dedicated class for the Image Preview, copies parts of the Texture Viewer but decouples it from the interactive aspects.
/// </summary>
public class TextureImagePreview : IResourceEventListener
{
    private Task _loadingTask;

    public static TextureResource CurrentTextureInView;
    public static string CurrentTextureName;
    public static string CurrentTextureContainerName;

    private static TextureFolderBank.TextureViewInfo _selectedTextureContainer;
    private static string _selectedTextureContainerKey = "";
    private static ResourceDescriptor _selectedAssetDescription;

    private static string _selectedTextureKey = "";
    private static TPF.Texture _selectedTexture;

    public SubTexture _cachedPreviewSubtexture;

    private ShoeboxLayoutContainer shoeboxContainer = null;

    public TextureImagePreview()
    {

    }

    public void OnProjectChanged()
    {
        CurrentTextureInView.Dispose();

        if (Smithbox.ProjectType is ProjectType.ER or ProjectType.AC6)
        {
            string sourcePath = $@"menu\hi\01_common.sblytbnd.dcx";
            if (File.Exists($@"{Smithbox.ProjectRoot}\{sourcePath}"))
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
    }

    public void InvalidatePreviewImage()
    {
        _cachedPreviewSubtexture = null;
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

            if (texRes != null)
            {
                CurrentTextureInView = texRes;
                CurrentTextureName = _selectedTextureKey;

                // Get the SubTexture that matches the current field value
                if (_cachedPreviewSubtexture == null)
                {
                    _cachedPreviewSubtexture = GetPreviewSubTexture(context, textureRef);
                }

                if (_cachedPreviewSubtexture != null)
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
                    Vector2 size = new Vector2(Xmax - Xmin, Ymax - Ymin) * scale;

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
        if (Smithbox.ProjectType == ProjectType.AC6)
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
        if (Smithbox.ProjectType == ProjectType.ER)
        {

        }

        return subTex;
    }

    public static void ResetTextureViewer()
    {
        // Texture Viewer itself will reload the texture folder bank
        //TextureFolderBank.LoadTextureFolders();

        _selectedTextureContainer = null;
        _selectedTextureContainerKey = "";
        _selectedAssetDescription = null;

        _selectedTextureKey = "";
        _selectedTexture = null;
    }

    // TODO: de-dupe the below functions so they aren't duplicated here and in TextureViewerScreen

    private void SelectTextureContainer(TextureViewInfo info)
    {
        // Ignore this if it is already loaded (e.g. same entry is double clicked)
        if (_selectedTextureContainer == info)
            return;

        _selectedTextureContainerKey = info.Name;
        _selectedTextureContainer = info;
        CurrentTextureContainerName = _selectedTextureContainerKey;

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

    

    public void OnResourceLoaded(IResourceHandle handle, int tag)
    {
    }

    public void OnResourceUnloaded(IResourceHandle handle, int tag)
    {

    }
}

