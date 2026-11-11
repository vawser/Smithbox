using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// The texture of a sampler or MTD texture path (Material Editor plan, F4): a thumbnail when a viewport has it
/// loaded, where it is from the texture index, Open in Texture Viewer, and a picker over every texture name in the
/// project. One per Properties pane, as the picker is a modal it opens.
/// </summary>
public class MaterialTextureLink
{
    private readonly MaterialEditorView View;

    private const float ThumbnailSize = 192;

    // The picker: the entry and path field it sets
    private bool OpenPicker;
    private object PickerOwner;
    private PropertyInfo PickerProperty;
    private string PickerPath = "";
    private string PickerFilter = "";
    private List<string> PickerMatches;
    private string PickerMatchesFilter;

    private const string PickerPopupId = "###materialTexturePicker";

    public MaterialTextureLink(MaterialEditorView view)
    {
        View = view;
    }

    private ProjectEntry Project => View.Project;
    private MaterialTextureIndex Index => View.Editor.Textures;

    /// <summary>
    /// The texture name a path refers to: N:\…\tex\AEG007_001_a.tif is AEG007_001_a
    /// </summary>
    public static string TextureName(string path) => MaterialTextureIndex.Stem(path);

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
    /// A small icon after the field name: what is known of the texture, its thumbnail on hover, and a context menu
    /// with Pick Texture, Open in Texture Viewer and Scan Textures
    /// </summary>
    public void Display(object owner, PropertyInfo prop, string path, string id)
    {
        path ??= "";
        var name = TextureName(path);
        var empty = string.IsNullOrWhiteSpace(path);
        var handle = empty ? null : FindLoaded(name);
        var containers = empty ? new List<string>() : Index.ContainersOf(name);

        ImGui.SameLine();

        if (empty)
        {
            ImGui.TextDisabled($"{Icons.FileImageO}");
            GUI.Tooltip(LOC.Get("MAT_Texture_Empty_TT"));
        }
        else if (handle != null)
        {
            ImGui.Text($"{Icons.FileImageO}");
            if (ImGui.IsItemHovered())
                DisplayThumbnail(handle, name);
        }
        else if (containers.Count > 0)
        {
            ImGui.Text($"{Icons.FileImageO}");
            GUI.Tooltip(LOC.Get("MAT_Texture_Indexed_TT", name, string.Join("\n", containers.Take(5))));
        }
        else if (Index.HasScan)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, $"{Icons.FileImageO}");
            GUI.Tooltip(LOC.Get("MAT_Texture_Not_Found_TT", name));
        }
        else
        {
            ImGui.TextDisabled($"{Icons.FileImageO}");
            GUI.Tooltip(LOC.Get("MAT_Texture_Not_Loaded_TT", name));
        }

        if (!ImGui.BeginPopupContextItem($"materialTextureContext_{id}"))
            return;

        // Pick Texture
        if (ImGui.MenuItem($"{LOC.Get("MAT_Texture_Pick")}##pickTexture_{id}", "", false, Index.HasScan))
        {
            OpenPicker = true;
            PickerOwner = owner;
            PickerProperty = prop;
            PickerPath = path;
            PickerFilter = empty ? "" : name;
            PickerMatches = null;
        }
        if (!Index.HasScan)
            GUI.Tooltip(LOC.Get("MAT_Texture_Pick_Needs_Scan_TT"));

        // Open in Texture Viewer: from where it was loaded, or else where the index has it
        var container = handle?.Source?.ContainerPath ?? containers.FirstOrDefault();
        var entry = empty ? null : FindContainerEntry(Project, container);
        var canOpen = entry != null && Project.Handler.TextureViewer != null;

        if (ImGui.MenuItem($"{LOC.Get("MAT_Texture_Open_In_Viewer")}##openTexture_{id}", "", false, canOpen))
        {
            EditorCommandQueue.AddCommand($"texture/view/{entry.Filename}/{name}");
        }

        if (ImGui.MenuItem($"{LOC.Get("MAT_Texture_Copy_Name")}##copyTexture_{id}", "", false, !empty))
        {
            PlatformUtils.Instance.SetClipboardText(name);
        }

        ImGui.Separator();

        // Scan Textures
        var scanLabel = Index.IsScanning
            ? LOC.Get("MAT_Texture_Scanning", Index.Done, Index.Total)
            : Index.HasScan ? LOC.Get("MAT_Texture_Rescan") : LOC.Get("MAT_Texture_Scan");

        if (ImGui.MenuItem($"{scanLabel}##scanTextures_{id}", "", false, !Index.IsScanning))
        {
            Index.StartScan();
        }
        GUI.Tooltip(LOC.Get("MAT_Texture_Scan_TT"));

        ImGui.EndPopup();
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

    #region Picker
    /// <summary>
    /// The texture picker, opened from a path's context menu. Drawn every frame by the Properties pane.
    /// </summary>
    public void DrawPicker()
    {
        if (OpenPicker)
        {
            OpenPicker = false;
            ImGui.OpenPopup($"{LOC.Get("MAT_Texture_Picker_Title")}{PickerPopupId}");
        }

        ImGui.SetNextWindowSize(new Vector2(520, 480) * DPI.UIScale(), ImGuiCond.Appearing);

        var open = true;
        if (!ImGui.BeginPopupModal($"{LOC.Get("MAT_Texture_Picker_Title")}{PickerPopupId}", ref open))
            return;

        GUI.WrappedText(LOC.Get("MAT_Texture_Picker_Hint", Index.NameCount));

        // Filter
        ImGui.SetNextItemWidth(-1);
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.InputText("##materialTexturePickerFilter", ref PickerFilter, 255);
        GUI.Tooltip(LOC.Get("MAT_Texture_Picker_Filter_TT"));

        // The filtered names, kept until the filter changes
        if (PickerMatches == null || PickerMatchesFilter != PickerFilter)
        {
            PickerMatchesFilter = PickerFilter;
            PickerMatches = Index.AllNames
                .Where(n => string.IsNullOrWhiteSpace(PickerFilter) || n.Contains(PickerFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        GUI.WrappedText(LOC.Get("MAT_Texture_Picker_Count", PickerMatches.Count));

        string picked = null;

        var listHeight = ImGui.GetContentRegionAvail().Y - ImGui.GetFrameHeightWithSpacing() * 1.5f;
        if (ImGui.BeginChild("materialTexturePickerList", new Vector2(0, Math.Max(listHeight, 100)), ImGuiChildFlags.Borders))
        {
            var clipper = new ImGuiListClipper();
            clipper.Begin(PickerMatches.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var name = PickerMatches[i];

                    if (ImGui.Selectable($"{name}##pickerName{i}", false, ImGuiSelectableFlags.AllowDoubleClick) &&
                        ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        picked = name;
                    }

                    if (ImGui.IsItemHovered())
                        GUI.Tooltip(string.Join("\n", Index.ContainersOf(name).Take(5)));
                }
            }

            clipper.End();
        }
        ImGui.EndChild();

        GUI.WrappedText(LOC.Get("MAT_Texture_Picker_Double_Click"));

        if (ImGui.Button($"{LOC.Get("MAT_FileList_Cancel")}##materialTexturePickerCancel"))
            ImGui.CloseCurrentPopup();

        if (picked != null)
        {
            SetPath(picked);
            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    /// <summary>
    /// Points the path at a texture: the old path's folder and extension with the new name, as vanilla paths are
    /// full tool paths and the game finds the texture by name. An empty path becomes the name with .tif.
    /// </summary>
    private void SetPath(string name)
    {
        if (PickerOwner == null || PickerProperty == null)
            return;

        var old = PickerPath ?? "";
        var cut = Math.Max(old.LastIndexOf('\\'), old.LastIndexOf('/'));
        var folder = cut >= 0 ? old[..(cut + 1)] : "";
        var extension = Path.GetExtension(old);
        if (string.IsNullOrEmpty(extension))
            extension = ".tif";

        var path = $"{folder}{name}{extension}";

        View.ActionManager.ExecuteAction(new MatPropertyChange(View, View.Selection.SelectedFileWrapper, PickerProperty, PickerOwner, path));
    }
    #endregion

    /// <summary>
    /// The Texture Viewer's entry for a texture container (a .tpfbdt counts as its .tpfbhd)
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
