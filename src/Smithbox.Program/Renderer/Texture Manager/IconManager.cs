using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Renderer;

/// <summary>
/// Handles icon textures, which crop the whole texture to the specific icon space.
/// </summary>
public class IconManager
{
    private readonly GraphicsDevice _gd;
    private readonly IImguiRenderer _imgui;

    private readonly VulkanImGuiRenderer imGuiRenderer;

    public Dictionary<string, CachedTexture> Cache = new();

    public IconManager(GraphicsDevice gd, IImguiRenderer imgui)
    {
        _gd = gd;
        _imgui = imgui;

        if (!Smithbox.LowRequirementsMode)
        {
            imGuiRenderer = (VulkanImGuiRenderer)_imgui;
        }
    }

    public void PurgeCache()
    {
        foreach (var entry in Cache)
        {
            entry.Value.OldHandle.Dispose();
        }

        Cache.Clear();
    }

    public CachedTexture LoadIcon(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
    {
        CachedTexture texture = null;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if (!IsValidIconConfiguration(iconConfig))
        {
            return null;
        }

        var key = $"icon_{context.ID}_{columnIndex}_{fieldName}_{fieldValue}";

        var iconEntry = curProject.Handler.ParamData.IconConfigurations.Configurations.Where(
            e => e.Name == iconConfig.TargetConfiguration).FirstOrDefault();

        if (iconEntry == null)
            return null;

        if (Cache.ContainsKey(key))
            return Cache[key];

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

                        Cache.Add(key, newCachedTexture);

                        break;
                    }
                }
            }
        }

        return texture;
    }

    public void DisplayIcon(CachedTexture cachedTexture)
    {
        if (cachedTexture.OldHandle == null)
            return;

        // Get scaled image size vector
        var scale = CFG.Current.ParamEditor_Field_List_Icon_Preview_Scale;

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

    public bool IsValidIconConfiguration(IconConfig iconConfig)
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        // Check Icon Config, if not present then don't attempt to load or display anything
        if (iconConfig == null)
            return false;

        if (curProject.Handler.ParamData == null)
            return false;

        if (curProject.Handler.ParamData.IconConfigurations.Configurations == null)
            return false;

        return true;
    }
}
