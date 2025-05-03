using Andre.Formats;
using Assimp;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Resource.Locators;
using StudioCore.Resource.Types;
using StudioCore.TextureViewer;
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
public class TexImagePreview : IResourceEventListener
{
    private TextureViewerScreen Editor;
    private TexViewSelection Selection;

    private Task LoadingTask;

    public TexImagePreview(TextureViewerScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
    }

    // <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// Display the Image Preview texture in the Param Editor properties view
    /// </summary>
    public bool DisplayImagePreview(Param.Row context, TexRef textureRef, bool displayImage = true)
    {
        if(textureRef == null) 
            return false;

        if (Selection == null)
            return false;

        // Display the texture
        LoadTextureContainer(textureRef.TextureContainer);
        LoadTextureFile(textureRef.TextureFile);

        ResourceHandle<TextureResource> resHandle = GetImageTextureHandle(Selection.SelectedPreviewTextureKey, Selection.SelectedPreviewTexture, Selection.SelectedPreviewTextureDescriptor);

        if (resHandle == null)
            return false;

        TextureResource texRes = resHandle.Get();

        if (texRes == null)
            return false;

        if (texRes.GPUTexture == null)
            return false;

        Selection.CurrentPreviewTexture = texRes;
        Selection.CurrentPreviewTextureName = Selection.SelectedPreviewTextureKey;

        Selection.CurrentPreviewSubTexture = GetPreviewSubTexture(context, textureRef);

        if (Selection.CurrentPreviewSubTexture == null)
            return false;

        // Get scaled image size vector
        var scale = CFG.Current.Param_FieldContextMenu_ImagePreviewScale;

        // Get crop bounds
        float Xmin = float.Parse(Selection.CurrentPreviewSubTexture.X);
        float Xmax = Xmin + float.Parse(Selection.CurrentPreviewSubTexture.Width);
        float Ymin = float.Parse(Selection.CurrentPreviewSubTexture.Y);
        float Ymax = Ymin + float.Parse(Selection.CurrentPreviewSubTexture.Height);

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
            var textureId = new ImTextureID(texRes.GPUTexture.TexHandle);
            ImGui.Image(textureId, size, UV0, UV1);
        }

        return true;
    }

    /// <summary>
    /// Get the image preview sub texture
    /// </summary>
    private SubTexture GetPreviewSubTexture(Param.Row context, TexRef textureRef)
    {
        if (Selection == null)
            return null;

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
            subTex = GetMatchingSubTexture(Selection.CurrentPreviewTextureName, imageIdx, textureRef.SubTexturePrefix);
        }

        // Hardcoded logic for AC6
        if (Editor.Project.ProjectType == ProjectType.AC6)
        {
            if (textureRef.LookupType == "Booster")
            {
                subTex = GetMatchingSubTexture(Selection.CurrentPreviewTextureName, imageIdx, "BS_A_");
            }
            if (textureRef.LookupType == "Weapon")
            {
                // Check for WP_A_ match
                subTex = GetMatchingSubTexture(Selection.CurrentPreviewTextureName, imageIdx, "WP_A_");

                // If failed, check for WP_R_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(Selection.CurrentPreviewTextureName, imageIdx, "WP_R_");
                }

                // If failed, check for WP_L_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(Selection.CurrentPreviewTextureName, imageIdx, "WP_L_");
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
                subTex = GetMatchingSubTexture(Selection.CurrentPreviewTextureName, imageIdx, prefix);
            }
        }

        return subTex;
    }

    /// <summary>
    /// Select the texture container for the Image Preview
    /// </summary>
    private void SelectTextureContainer(TextureViewInfo info)
    {
        if (Selection == null)
            return;

        // Ignore this if it is already loaded (e.g. same entry is double clicked)
        if (Selection.SelectedPreviewTextureContainer == info)
            return;

        Selection.SelectedPreviewTextureContainerKey = info.Name;
        Selection.SelectedPreviewTextureContainer = info;
        Selection.CurrentPreviewTextureContainerName = Selection.SelectedPreviewTextureContainerKey;

        ResourceManager.UnloadPersistentTextures();

        ResourceManager.ResourceJobBuilder job = ResourceManager.CreateNewJob($@"Loading {info.Name} textures");

        ResourceDescriptor ad = null;

        if (info.Category == TextureViewCategory.Menu)
        {
            ad = TextureLocator.GetMenuTextureContainer(Selection.SelectedPreviewTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Asset)
        {
            ad = TextureLocator.GetAssetTextureContainer(Selection.SelectedPreviewTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Object)
        {
            ad = TextureLocator.GetObjTextureContainer(Selection.SelectedPreviewTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Part)
        {
            var isLowDetail = false;
            var name = info.Path;
            if (name.Contains("_l."))
            {
                isLowDetail = true;
            }

            ad = TextureLocator.GetPartTextureContainer(Selection.SelectedPreviewTextureContainerKey, isLowDetail);
        }

        if (info.Category == TextureViewCategory.Other)
        {
            ad = TextureLocator.GetOtherTextureContainer(Selection.SelectedPreviewTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Particle)
        {
            ad = TextureLocator.GetParticleTextureContainer(Selection.SelectedPreviewTextureContainerKey);
        }

        if (info.Category == TextureViewCategory.Character)
        {
            var chrId = Selection.SelectedPreviewTextureContainerKey;

            var isLowDetail = false;
            if (chrId.Substring(chrId.Length - 2, 2) == "_l")
            {
                isLowDetail = true;
            }

            if (Editor.Project.ProjectType is ProjectType.ER)
            {
                chrId = chrId.Substring(0, chrId.Length - 2); // remove the _h
            }

            ad = TextureLocator.GetChrTextures(chrId, isLowDetail);
        }

        if (ad != null)
        {
            Selection.SelectedPreviewTextureDescriptor = ad;

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

                ResourceManager.AddResourceListener<TextureResource>(ad.AssetVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);

                Selection.SelectedPreviewTextureContainer.Textures = TPF.Read(info.Path).Textures;
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

                ResourceManager.AddResourceListener<TextureResource>(ad.AssetArchiveVirtualPath, this, AccessLevel.AccessGPUOptimizedOnly);


                Selection.SelectedPreviewTextureContainer.Textures = GetTexturesFromBinder(info.Path);
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

    public void LoadTextureContainer(string container)
    {
        if (Selection == null)
            return;

        if (Editor.Project.TextureData.PrimaryBank.Entries == null)
            return;

        if (Selection.SelectedPreviewTextureContainerKey != container)
        {
            foreach (var (name, info) in Editor.Project.TextureData.PrimaryBank.Entries)
            {
                if (name == container)
                {
                    Selection.SelectedPreviewTextureContainerKey = name;
                    SelectTextureContainer(info);
                }
            }
        }
    }

    public void LoadTextureFile(string filename)
    {
        if (Selection == null)
            return;

        if (Selection.SelectedPreviewTextureContainerKey != filename)
        {
            if (Selection.SelectedPreviewTextureContainer != null && Selection.SelectedPreviewTextureContainerKey != "")
            {
                TextureViewInfo data = Selection.SelectedPreviewTextureContainer;

                if (data.Textures != null)
                {
                    foreach (var tex in data.Textures)
                    {
                        if (tex.Name == filename)
                        {
                            Selection.SelectedPreviewTextureKey = tex.Name;
                            Selection.SelectedPreviewTexture = tex;
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
        if (Editor.Project.TextureData.Shoebox == null)
            return null;

        if (Editor.Project.TextureData.Shoebox.Textures.ContainsKey(currentTextureName))
        {
            var subTexs = Editor.Project.TextureData.Shoebox.Textures[currentTextureName];

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

        return null;
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

