using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Formats.Tar;
using System.IO;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexTextureFileList
{
    public TexEditorView Parent;
    public ProjectEntry Project;

    private string TextureFileListFilter = "";
    private bool ExactTextureFileListFilter = false;

    public TexTextureFileList(TexEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Textures", "");

        EditorFilters.DisplayFramedListFilter("textureViewer_TextureList", ref TextureFileListFilter, ref ExactTextureFileListFilter);

        ImGui.BeginChild("TextureList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (Parent.Selection.SelectedTpf != null)
        {
            int index = 0;

            foreach (var entry in Parent.Selection.SelectedTpf.Textures)
            {
                var isMatch = EditorFilters.IsMatch(
                    TextureFileListFilter, entry.Name, ExactTextureFileListFilter, "", true);

                if (isMatch)
                {
                    var displayName = entry.Name;

                    var isSelected = false;
                    if (Parent.Selection.SelectedTextureKey == entry.Name)
                    {
                        isSelected = true;
                    }

                    // Texture row
                    if (ImGui.Selectable($@"{displayName}", isSelected))
                    {
                        Parent.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                        Parent.Editor.ViewHandler.ActiveView = Parent;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Parent.Selection.SelectTexture)
                    {
                        Parent.Selection.SelectTexture = false;
                        Parent.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            Parent.Selection.SelectTexture = true;
                        }
                    }

                    if(index == 0 && Parent.Selection.AutoSelectTexture)
                    {
                        Parent.Selection.AutoSelectTexture = false;
                        Parent.Selection.SelectTexture = false;
                        Parent.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                    }
                }

                ContextMenu(entry, index);

                index++;
            }
        }

        ImGui.EndChild();
    }

    private void ContextMenu(TPF.Texture tex, int index)
    {
        if (ImGui.BeginPopupContextItem($"context_{tex.Name}{index}"))
        {
            if (ImGui.MenuItem("Export"))
            {
                _ = Parent.Editor.ToolView.TextureExport.ExportTextureAsync(tex);
            }
            UIHelper.Tooltip($"Export this file to your current export directory : {CFG.Current.TextureViewerToolbar_ExportTextureLocation}");

            ImGui.Separator();

            if (ImGui.MenuItem("Copy Name"))
            {
                ImGui.SetClipboardText(tex.Name);
            }
            UIHelper.Tooltip("Copy the texture name to the clipboard.");

            ImGui.EndPopup();
        }
    }
    private int TargetIndex = -1;
    public bool LoadTexture = false;

    public void Update()
    {
        if (LoadTexture)
        {
            if (TargetIndex != -1 && Parent.Selection.SelectedTpf != null)
            {
                Parent.Selection.ViewerTextureResource = new TextureResource(Parent.Selection.SelectedTpf, TargetIndex);
                Parent.Selection.ViewerTextureResource._LoadTexture(AccessLevel.AccessFull);
            }

            LoadTexture = false;
        }
    }
}
