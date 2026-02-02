using Andre.Formats;
using DotNext.IO.Log;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Renderer;

/// <summary>
/// Handles icon textures, which crop the whole texture to the specific icon space.
/// </summary>
public class IconManager
{
    public Dictionary<string, CachedTexture> Cache = new();

    public IconManager() { }

    public void PurgeCache()
    {
        foreach (var entry in Cache)
        {
            entry.Value.Handle?.Dispose();
        }

        Cache.Clear();
    }

    public CachedTexture HandleIcon(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
    {
        CachedTexture texture = null;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        if(curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            return texture = LoadDirectIcon(context, iconConfig, fieldValue, fieldName, columnIndex);
        }
        else
        {
            return texture = LoadContainerIcon(context, iconConfig, fieldValue, fieldName, columnIndex);
        }
    }

    public CachedTexture LoadDirectIcon(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
    {
        CachedTexture texture = null;

        var curProject = Smithbox.Orchestrator.SelectedProject;

        // Instead of icon config, we search the layout folder directly
        var layouts = Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(curProject.Descriptor.ProjectType), "Icon Layouts");

        var key = $"icon_{context.ID}_{columnIndex}_{fieldName}_{fieldValue}";

        if (Cache.ContainsKey(key))
            return Cache[key];

        var targetLayoutPath = "";
        
        foreach(var file in Directory.EnumerateFiles(layouts))
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            Match match = Regex.Match(filename, @"\d+");
            if (match.Success)
            {
                var idString = match.Value;
                int id;

                if(int.TryParse(idString, out id))
                {
                    if(id.ToString() == fieldValue.ToString())
                    {
                        targetLayoutPath = file;
                        break;
                    }
                }
            }
        }

        if (targetLayoutPath == "")
            return texture;

        ShoeboxLayout layout = new ShoeboxLayout(targetLayoutPath);

        var atlas = layout.TextureAtlases.FirstOrDefault();
        if (atlas == null)
            return texture;

        // For DS2 we have a type attribute in the layout file that
        // corresponds to the IconConfig ref string
        var isMatch = atlas.Type == iconConfig.TargetConfiguration;

        if (!isMatch)
            return texture;

        // ImagePath is set to the relative path needed for the VFS
        var relativePath = atlas.ImagePath;

        // Create this as it is needed in SubTextureHelper.GetPreviewSubTexture
        var iconConfigEntry = new IconConfigurationEntry();
        iconConfigEntry.SubTexturePrefix = "ICON_";

        try
        {
            var tpfData = curProject.VFS.FS.ReadFileOrThrow(relativePath);

            TPF curTPF = TPF.Read(tpfData);

            // DS2 TPFs only contain 1 texture, so we can skip proper matching
            var curTex = curTPF.Textures.FirstOrDefault();

            var subTexture = atlas.SubTextures.FirstOrDefault();

            if (subTexture != null)
            {
                var newCachedTexture = new CachedTexture(subTexture);
                newCachedTexture.Load(curTPF, 0);

                if (!Cache.ContainsKey(key))
                {
                    Cache.Add(key, newCachedTexture);
                }
            }
        }
        catch(Exception e)
        {
            TaskLogs.AddError($"Failed to read TPF: {relativePath}", e);
        }

        return texture;
    }

    public CachedTexture LoadContainerIcon(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
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

                        if (!Cache.ContainsKey(key))
                        {
                            Cache.Add(key, newCachedTexture);
                        }

                        break;
                    }
                }
            }
        }

        return texture;
    }

    public Vector2 DisplayIcon(CachedTexture cachedTexture)
    {
        if (cachedTexture.Handle == null)
            return new Vector2();

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
        float left = (Xmin) / cachedTexture.Handle.Width;
        float top = (Ymin) / cachedTexture.Handle.Height;
        float right = (Xmax) / cachedTexture.Handle.Width;
        float bottom = (Ymax) / cachedTexture.Handle.Height;

        // Build UV coordinates
        var UV0 = new Vector2(left, top);
        var UV1 = new Vector2(right, bottom);

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
        {
            var vulkanHandle = (VulkanTextureHandle)cachedTexture.Handle;
            var textureId = new ImTextureID(vulkanHandle.Handle.TexHandle);
            ImGui.Image(textureId, size, UV0, UV1);
        }

        if (Smithbox.Instance.CurrentBackend is RenderingBackend.OpenGL)
        {
            var openglHandle = (OpenGLTextureHandle)cachedTexture.Handle;
            var textureId = new ImTextureID(openglHandle.Handle);
            ImGui.Image(textureId, size, UV0, UV1);
        }

        return size;
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
