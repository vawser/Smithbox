using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Numerics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The texture of a sampler or MTD texture path (Material Editor plan, F4): a thumbnail when a viewport has the
/// texture loaded, and a way to open it in the Texture Viewer. Textures are found among those already loaded, by
/// name, as the Model Editor's material preview does; a texture no viewport uses can't be shown without an index of
/// every texture container, which isn't built (see F5).
/// </summary>
public static class MaterialTextureLink
{
    private const float ThumbnailSize = 192;

    /// <summary>
    /// The texture name a path refers to: N:\…\tex\AEG007_001_a.tif is AEG007_001_a
    /// </summary>
    public static string TextureName(string path) =>
        Path.GetFileNameWithoutExtension((path ?? "").Replace('\\', '/').Split('/')[^1]);

    /// <summary>
    /// The loaded texture of this name, in any pool
    /// </summary>
    private static ResourceHandle<TextureResource> FindLoaded(string name)
    {
        foreach (var pool in ResourceManager.Pools)
        {
            if (pool.TryFind<TextureResource>(name, out var handle) && handle != null && handle.IsLoaded &&
                handle.Get()?.GPUTexture != null)
            {
                return handle;
            }
        }

        return null;
    }

    /// <summary>
    /// A small icon after the field name, with the thumbnail on hover and a context menu
    /// </summary>
    public static void Display(ProjectEntry project, string path, string id)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var name = TextureName(path);
        var handle = FindLoaded(name);

        ImGui.SameLine();

        if (handle == null)
        {
            ImGui.TextDisabled($"{Icons.FileImageO}");
            GUI.Tooltip(LOC.Get("MAT_Texture_Not_Loaded_TT", name));
        }
        else
        {
            ImGui.Text($"{Icons.FileImageO}");

            if (ImGui.IsItemHovered())
            {
                DisplayThumbnail(handle, name);
            }
        }

        if (ImGui.BeginPopupContextItem($"materialTextureContext_{id}"))
        {
            var entry = handle == null ? null : FindContainerEntry(project, handle.Source?.ContainerPath);
            var canOpen = entry != null && project.Handler.TextureViewer != null;

            if (ImGui.MenuItem($"{LOC.Get("MAT_Texture_Open_In_Viewer")}##openTexture_{id}", "", false, canOpen))
            {
                EditorCommandQueue.AddCommand($"texture/view/{entry.Filename}/{name}");
            }

            if (ImGui.MenuItem($"{LOC.Get("MAT_Texture_Copy_Name")}##copyTexture_{id}"))
            {
                PlatformUtils.Instance.SetClipboardText(name);
            }

            ImGui.EndPopup();
        }
    }

    private static void DisplayThumbnail(ResourceHandle<TextureResource> handle, string name)
    {
        var texture = handle.Get()?.GPUTexture;
        if (texture == null)
            return;

        ImGui.BeginTooltip();

        GUI.WrappedText(LOC.Get("MAT_Texture_Loaded_TT", name, handle.Source?.ContainerPath ?? "?"));

        // Fit the longer side to the thumbnail size, keeping the aspect
        var width = Math.Max(1, (int)texture.Width);
        var height = Math.Max(1, (int)texture.Height);
        var scale = ThumbnailSize * DPI.UIScale() / Math.Max(width, height);

        ImGui.Image(new ImTextureID(texture.TexHandle), new Vector2(width * scale, height * scale));

        ImGui.EndTooltip();
    }

    /// <summary>
    /// The Texture Viewer's entry for the container a texture was loaded from (a .tpfbdt counts as its .tpfbhd)
    /// </summary>
    private static FileDictionaryEntry FindContainerEntry(ProjectEntry project, string containerPath)
    {
        if (string.IsNullOrEmpty(containerPath))
            return null;

        var key = Normalize(containerPath.Replace(".tpfbdt", ".tpfbhd", StringComparison.OrdinalIgnoreCase));

        return project.Locator.TextureFiles.Entries?.FirstOrDefault(e => Normalize(e.Path) == key)
            ?? project.Locator.TexturePackedFiles.Entries?.FirstOrDefault(e => Normalize(e.Path) == key);
    }

    private static string Normalize(string path) => (path ?? "").Replace('\\', '/').TrimStart('/').ToLowerInvariant();
}
