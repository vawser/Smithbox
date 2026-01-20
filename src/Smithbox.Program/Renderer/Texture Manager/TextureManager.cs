using Andre.Formats;
using DirectXTexNet;
using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using Microsoft.AspNetCore.Components.Forms;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Veldrid;
using static SoulsFormats.ACB.Asset;
using Texture = Veldrid.Texture;

namespace StudioCore.Renderer;
/// <summary>
/// For basic textures so we can avoid the messiness of the Resource Manager.
/// Used by Icon Preview and the Texture Viewer.
/// </summary>
public class TextureManager
{
    private readonly GraphicsDevice _gd;
    private readonly IImguiRenderer _imgui;

    private readonly VulkanImGuiRenderer imGuiRenderer;

    private Dictionary<string, CachedTexture> _cache = new();

    public TextureManager(GraphicsDevice gd, IImguiRenderer imgui)
    {
        _gd = gd;
        _imgui = imgui;

        if(!Smithbox.LowRequirementsMode)
        {
            imGuiRenderer = (VulkanImGuiRenderer)_imgui;
        }
    }

    public void PurgeCache()
    {
        foreach(var entry in _cache)
        {
            entry.Value.OldHandle.Dispose();
        }

        _cache.Clear();
    }

    public CachedTexture LoadIcon(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
    {
        CachedTexture texture = null;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (!IsValidIconConfiguration(iconConfig))
        {
            return null;
        }

        var key = $"icon_{fieldName}_{columnIndex}_{fieldName}_{fieldValue}";

        var iconEntry = curProject.Handler.ParamData.IconConfigurations.Configurations.Where(
            e => e.Name == iconConfig.TargetConfiguration).FirstOrDefault();

        if (iconEntry == null)
            return null;

        if (_cache.ContainsKey(key))
            return _cache[key];

        var targetFile = curProject.Locator.TextureFiles.Entries.FirstOrDefault(
            e => e.Filename == iconEntry.File);

        if (targetFile == null)
            return null;

        var targetBinder = curProject.Handler.TextureData.PrimaryBank.Entries.FirstOrDefault(
            e => e.Key.Filename == targetFile.Filename);

        if (targetBinder.Value == null)
        {
            Task<bool> loadTask = curProject.Handler.TextureData.PrimaryBank.LoadTextureBinder(targetFile);
            Task.WaitAll(loadTask);

            return null;
        }

        foreach (var entry in targetBinder.Value.Files)
        {
            var curTpf = entry.Value;

            for (int i = 0; i < curTpf.Textures.Count; i++)
            {
                var curTex = curTpf.Textures[i];

                foreach (var curInternalFilename in iconEntry.InternalFiles)
                {
                    if (curTex.Name == curInternalFilename)
                    {
                        var subTexture = SubTextureHelper.GetPreviewSubTexture(
                            curProject, curTex.Name, context,
                            iconConfig, iconEntry, fieldValue, fieldName);

                        if (subTexture == null)
                            continue;

                        var newCachedTexture = new CachedTexture(subTexture);
                        newCachedTexture.Load(curTpf, i);

                        _cache.Add(key, newCachedTexture);

                        break;
                    }
                }
            }
        }

        return texture;
    }

    public bool IsValidIconConfiguration(IconConfig iconConfig)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        // Check Icon Config, if not present then don't attempt to load or display anything
        if (iconConfig == null)
            return false;

        if (curProject.Handler.ParamData.IconConfigurations.Configurations == null)
            return false;

        return true;
    }

    public CachedTexture LoadImage(string key)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        CachedTexture texture = null;

        // TODO

        return texture;
    }

    public void DisplayIcon(CachedTexture cachedTexture)
    {
        if (cachedTexture.Texture == null)
            return;

        // Get scaled image size vector
        var scale = CFG.Current.Param_FieldContextMenu_ImagePreviewScale;

        // Get crop bounds
        float Xmin = float.Parse(cachedTexture.SubTexture.X);
        float Xmax = Xmin + float.Parse(cachedTexture.SubTexture.Width);
        float Ymin = float.Parse(cachedTexture.SubTexture.Y);
        float Ymax = Ymin + float.Parse(cachedTexture.SubTexture.Height);

        // Image size should be based on cropped image
        Vector2 size = new Vector2(Xmax - Xmin, Ymax - Ymin) * scale;

        // Get UV coordinates based on full image
        float left = (Xmin) / cachedTexture.Texture.Width;
        float top = (Ymin) / cachedTexture.Texture.Height;
        float right = (Xmax) / cachedTexture.Texture.Width;
        float bottom = (Ymax) / cachedTexture.Texture.Height;

        // Build UV coordinates
        var UV0 = new Vector2(left, top);
        var UV1 = new Vector2(right, bottom);

        var textureId = new ImTextureID(cachedTexture.Handle);

        ImGui.Image(textureId, size, UV0, UV1);
    }

    // TEMP
    // Temporary until we fully rework the TexturePool.TextureHandle stuff for direct images
    public void DisplayIcon_Old(CachedTexture cachedTexture)
    {
        if (cachedTexture.OldHandle == null)
            return;

        // Get scaled image size vector
        var scale = CFG.Current.Param_FieldContextMenu_ImagePreviewScale;

        // Get crop bounds
        float Xmin = float.Parse(cachedTexture.SubTexture.X);
        float Xmax = Xmin + float.Parse(cachedTexture.SubTexture.Width);
        float Ymin = float.Parse(cachedTexture.SubTexture.Y);
        float Ymax = Ymin + float.Parse(cachedTexture.SubTexture.Height);

        // Image size should be based on cropped image
        Vector2 size = new Vector2(Xmax - Xmin, Ymax - Ymin) * scale;

        // Get UV coordinates based on full image
        float left = (Xmin) / cachedTexture.OldHandle.Width;
        float top = (Ymin) / cachedTexture.OldHandle.Height;
        float right = (Xmax) / cachedTexture.OldHandle.Width;
        float bottom = (Ymax) / cachedTexture.OldHandle.Height;

        // Build UV coordinates
        var UV0 = new Vector2(left, top);
        var UV1 = new Vector2(right, bottom);

        var textureId = new ImTextureID(cachedTexture.OldHandle.TexHandle);

        ImGui.Image(textureId, size, UV0, UV1);
    }

    public void DisplayImage(CachedTexture cachedTexture)
    {
        if (cachedTexture.Texture == null)
            return;

        var scale = CFG.Current.Param_FieldContextMenu_ImagePreviewScale;

        Vector2 size = new Vector2(cachedTexture.Texture.Width, cachedTexture.Texture.Height) * scale;

        var textureId = new ImTextureID(cachedTexture.Handle);

        ImGui.Image(textureId, size);
    }
}

public class CachedTexture
{
    public Texture Texture { get; set; }
    public TextureView View { get; set; }
    public SubTexture SubTexture { get; set; }
    public nint Handle { get; set; }

    // TEMP
    public TexturePool.TextureHandle OldHandle { get; set; }

    public CachedTexture(SubTexture subTexture)
    {
        SubTexture = subTexture;
    }

    public CachedTexture(Texture texture, TextureView view, nint handle, SubTexture subTexture)
    {
        Texture = texture;
        View = view;
        Handle = handle;
        SubTexture = subTexture;
    }

    public void Load(TPF tpf, int index)
    {
        if (TexturePool.TextureHandle.IsTPFCube(tpf.Textures[index], tpf.Platform))
        {
            OldHandle = SceneRenderer.GlobalCubeTexturePool.AllocateTextureDescriptor();
        }
        else
        {
            OldHandle = SceneRenderer.GlobalTexturePool.AllocateTextureDescriptor();
        }

        if (OldHandle == null)
        {
            ResourceLog.AddLog("Unable to allocate texture descriptor");
            return;
        }

        if (tpf.Platform == TPF.TPFPlatform.PC || tpf.Platform == TPF.TPFPlatform.PS3)
        {
            SceneRenderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                if (OldHandle == null)
                {
                    return;
                }

                if (index < tpf.Textures.Count)
                {
                    if (tpf.Textures[index] != null)
                    {
                        try
                        {
                            OldHandle.FillWithTPF(
                                d, cl, tpf.Platform, tpf.Textures[index], tpf.Textures[index].Name);
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddError("Failed to fill TPF", ex);
                        }
                    }
                }

                Texture = null;
            });
        }
        else if (tpf.Platform == TPF.TPFPlatform.PS4)
        {
            SceneRenderer.AddLowPriorityBackgroundUploadTask((d, cl) =>
            {
                if (OldHandle == null)
                {
                    return;
                }

                try
                {
                    OldHandle.FillWithPS4TPF(
                        d, cl, tpf.Platform, tpf.Textures[index], tpf.Textures[index].Name);
                }
                catch (Exception ex)
                {
                    TaskLogs.AddError("Failed to fill TPF", ex);
                }

                Texture = null;
            });
        }
    }
}
