using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// Dedicated class for the Image Preview, copies parts of the Texture Viewer but decouples it from the interactive aspects.
/// </summary>
public class TexImagePreview : IResourceEventListener
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    private Task LoadingTask;

    public TexImagePreview(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    private Dictionary<string, TextureResource> LoadedResources = new();

    public void ClearIcons()
    {
        foreach (var entry in LoadedResources)
        {
            entry.Value.Dispose();
        }

        LoadedResources.Clear();
    }

    /// <summary>
    /// Display the Image Preview texture in the Param Editor properties view
    /// </summary>
    public bool DisplayImagePreview(Param.Row context, IconConfig iconConfig, object fieldValue, string fieldName, int columnIndex)
    {
        var resourceKey = $"{fieldName}_{columnIndex}";

        if (Project.ParamData.IconConfigurations == null)
            return false;

        // Check Icon Config, if not present then don't attempt to load or display anything
        if (iconConfig == null)
            return false;

        if (Project.ParamData.IconConfigurations.Configurations == null)
            return false;

        var iconEntry = Project.ParamData.IconConfigurations.Configurations.Where(e => e.Name == iconConfig.TargetConfiguration).FirstOrDefault();

        if (iconEntry == null)
            return false;

        // If icon texture resource doesn't exist yet, load it
        if (!LoadedResources.ContainsKey(resourceKey))
        {
            var targetFile = Project.TextureData.TextureFiles.Entries.FirstOrDefault(e => e.Filename == iconEntry.File);

            if(targetFile == null)
                return false;

            Task<bool> loadTask = Project.TextureData.PrimaryBank.LoadTextureBinder(targetFile);

            Task.WaitAll(loadTask);

            var targetBinder = Project.TextureData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == targetFile.Filename);

            int index = 0;

            SubTexture curPreviewTexture = null;

            // TPF
            foreach (var entry in targetBinder.Value.Files)
            {
                var curTpf = entry.Value;

                index = 0;

                foreach (var curTex in curTpf.Textures)
                {
                    foreach (var curInternalFilename in iconEntry.InternalFiles)
                    {
                        if (curTex.Name == curInternalFilename)
                        {
                            curPreviewTexture = GetPreviewSubTexture(curTex.Name, context, 
                                iconConfig, iconEntry, fieldValue, fieldName);

                            if(curPreviewTexture != null)
                            {
                                if (!LoadedResources.ContainsKey(resourceKey))
                                {
                                    var newResource = new TextureResource(curTpf, index);
                                    newResource.SubTexture = curPreviewTexture;
                                    newResource._LoadTexture(AccessLevel.AccessFull);

                                    LoadedResources.Add(resourceKey, newResource);
                                }

                                break;
                            }
                        }
                    }

                    index++;
                }
            }
        }

        // If icon texture resource exists, grab it and display the image
        if (LoadedResources.ContainsKey(resourceKey))
        {
            var curResource = LoadedResources[resourceKey];

            if (curResource == null)
                return false;

            if (curResource.GPUTexture == null)
                return false;

            if (CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn)
            {
                DisplayImage(curResource);
            }

            return true;
        }

        return false;
    }

    public void DisplayImage(TextureResource curResource)
    {
        // Get scaled image size vector
        var scale = CFG.Current.Param_FieldContextMenu_ImagePreviewScale;

        // Get crop bounds
        float Xmin = float.Parse(curResource.SubTexture.X);
        float Xmax = Xmin + float.Parse(curResource.SubTexture.Width);
        float Ymin = float.Parse(curResource.SubTexture.Y);
        float Ymax = Ymin + float.Parse(curResource.SubTexture.Height);

        // Image size should be based on cropped image
        Vector2 size = new Vector2(Xmax - Xmin, Ymax - Ymin) * scale;

        // Get UV coordinates based on full image
        float left = (Xmin) / curResource.GPUTexture.Width;
        float top = (Ymin) / curResource.GPUTexture.Height;
        float right = (Xmax) / curResource.GPUTexture.Width;
        float bottom = (Ymax) / curResource.GPUTexture.Height;

        // Build UV coordinates
        var UV0 = new Vector2(left, top);
        var UV1 = new Vector2(right, bottom);

        if(curResource.GPUTexture != null)
        {
            var textureId = new ImTextureID(curResource.GPUTexture.TexHandle);
            ImGui.Image(textureId, size, UV0, UV1);
        }
    }

    /// <summary>
    /// Get the image preview sub texture
    /// </summary>
    private SubTexture GetPreviewSubTexture(string textureKey, Param.Row context, IconConfig fieldIcon, IconConfigurationEntry iconEntry, object fieldValue, string fieldName)
    {
        // Guard clauses checking the validity of the TextureRef
        if (context[fieldName] == null)
        {
            return null;
        }

        var imageIdx = $"{fieldValue}";

        SubTexture subTex = null;

        // Special-case handling for some icons in AC6 that are awkward to fit into the Icon Configuration system
        if(iconEntry.SubTexturePrefix == "AC6-Weapon" || iconEntry.SubTexturePrefix == "AC6-Armor")
        {
            if(iconEntry.SubTexturePrefix == "AC6-Weapon")
            {
                subTex = GetMatchingSubTexture(textureKey, imageIdx, "WP_A_");

                // If failed, check for WP_R_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(textureKey, imageIdx, "WP_R_");
                }

                // If failed, check for WP_L_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(textureKey, imageIdx, "WP_L_");
                }
            }

            if (iconEntry.SubTexturePrefix == "AC6-Armor")
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

                subTex = GetMatchingSubTexture(textureKey, imageIdx, prefix);
            }
        }
        else
        {
            subTex = GetMatchingSubTexture(textureKey, imageIdx, iconEntry.SubTexturePrefix);
        }

        return subTex;
    }

    public SubTexture GetMatchingSubTexture(string currentTextureName, string imageIndex, string namePrepend)
    {
        if (Editor.Project.TextureData.ShoeboxFiles == null)
            return null;

        if (Editor.Project.TextureData.ShoeboxFiles.Entries == null)
            return null;

        var shoeboxEntry = Editor.Project.TextureData.PrimaryBank.ShoeboxEntries.FirstOrDefault();

        if(shoeboxEntry.Value == null)
            return null;

        if (shoeboxEntry.Value.Textures.ContainsKey(currentTextureName))
        {
            var subTexs = shoeboxEntry.Value.Textures[currentTextureName];

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

