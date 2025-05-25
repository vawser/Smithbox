using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Resource;
using StudioCore.Resource.Types;
using StudioCore.TextureViewer;
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

    /// <summary>
    /// Display the Image Preview texture in the Param Editor properties view
    /// </summary>
    public bool DisplayImagePreview(Param.Row context, TexRef textureRef, bool displayImage = true)
    {
        if(textureRef == null) 
            return false;

        var targetFile = Project.TextureData.TextureFiles.Entries.FirstOrDefault(e => e.Filename == textureRef.TextureContainer);

        if(targetFile == null)
            return false;

        Task<bool> loadTask = Project.TextureData.PrimaryBank.LoadTextureBinder(targetFile);

        Task.WaitAll(loadTask);

        var targetBinder = Project.TextureData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == targetFile.Filename);

        if (targetBinder.Key != null)
        {
            Editor.Selection.SelectTextureFile(targetBinder.Key, targetBinder.Value);
        }

        // TPF
        foreach(var entry in targetBinder.Value.Files)
        {
            var binderFile = entry.Key;
            var tpfEntry = entry.Value;

            if (binderFile.Name == textureRef.TextureContainer)
            {
                Editor.Selection.SelectTpfFile(entry.Key, entry.Value);
                break;
            }
        }

        if (Editor.Selection.SelectedTpf == null)
            return false;

        // Texture
        int index = 0;

        foreach (var entry in Editor.Selection.SelectedTpf.Textures)
        {
            if(entry.Name == textureRef.TextureFile)
            {
                TargetIndex = index;
                Editor.Selection.SelectTextureEntry(entry.Name, entry);
                LoadTexture = true;
                break;
            }

            index++;
        }

        if (Editor.Selection.PreviewTextureResource == null)
            return false;

        if (Editor.Selection.PreviewTextureResource.GPUTexture == null)
            return false;

        Editor.Selection.PreviewSubTexture = GetPreviewSubTexture(context, textureRef);

        if (Editor.Selection.PreviewSubTexture == null)
            return false;

        DisplayImage(displayImage);

        return true;
    }

    private int TargetIndex = -1;
    private bool LoadTexture = false;

    public void Update()
    {
        if (!Smithbox.LowRequirementsMode)
        {
            if (LoadTexture)
            {
                Editor.Selection.PreviewTextureResource = new TextureResource(Editor.Selection.SelectedTpf, TargetIndex);
                Editor.Selection.PreviewTextureResource._LoadTexture(AccessLevel.AccessFull);

                LoadTexture = false;
            }
        }
    }

    public void DisplayImage(bool displayImage)
    {
        // Get scaled image size vector
        var scale = CFG.Current.Param_FieldContextMenu_ImagePreviewScale;

        // Get crop bounds
        float Xmin = float.Parse(Editor.Selection.PreviewSubTexture.X);
        float Xmax = Xmin + float.Parse(Editor.Selection.PreviewSubTexture.Width);
        float Ymin = float.Parse(Editor.Selection.PreviewSubTexture.Y);
        float Ymax = Ymin + float.Parse(Editor.Selection.PreviewSubTexture.Height);

        // Image size should be based on cropped image
        Vector2 size = new Vector2(Xmax - Xmin, Ymax - Ymin) * scale;

        // Get UV coordinates based on full image
        float left = (Xmin) / Editor.Selection.PreviewTextureResource.GPUTexture.Width;
        float top = (Ymin) / Editor.Selection.PreviewTextureResource.GPUTexture.Height;
        float right = (Xmax) / Editor.Selection.PreviewTextureResource.GPUTexture.Width;
        float bottom = (Ymax) / Editor.Selection.PreviewTextureResource.GPUTexture.Height;

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
            var textureId = new ImTextureID(Editor.Selection.PreviewTextureResource.GPUTexture.TexHandle);
            ImGui.Image(textureId, size, UV0, UV1);
        }
    }

    /// <summary>
    /// Get the image preview sub texture
    /// </summary>
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
            subTex = GetMatchingSubTexture(Editor.Selection.SelectedTextureKey, imageIdx, textureRef.SubTexturePrefix);
        }

        // Hardcoded logic for AC6
        if (Editor.Project.ProjectType == ProjectType.AC6)
        {
            if (textureRef.LookupType == "Booster")
            {
                subTex = GetMatchingSubTexture(Editor.Selection.SelectedTextureKey, imageIdx, "BS_A_");
            }
            if (textureRef.LookupType == "Weapon")
            {
                // Check for WP_A_ match
                subTex = GetMatchingSubTexture(Editor.Selection.SelectedTextureKey, imageIdx, "WP_A_");

                // If failed, check for WP_R_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(Editor.Selection.SelectedTextureKey, imageIdx, "WP_R_");
                }

                // If failed, check for WP_L_ match
                if (subTex == null)
                {
                    subTex = GetMatchingSubTexture(Editor.Selection.SelectedTextureKey, imageIdx, "WP_L_");
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
                subTex = GetMatchingSubTexture(Editor.Selection.SelectedTextureKey, imageIdx, prefix);
            }
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

