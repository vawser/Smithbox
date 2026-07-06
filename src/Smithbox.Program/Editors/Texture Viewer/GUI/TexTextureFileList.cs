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
    public TexEditorView View;
    public ProjectEntry Project;

    private string TextureFileListFilter = "";
    private bool ExactTextureFileListFilter = false;

    public TexTextureFileList(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader(
            LOC.Get("TEXVIEW_TextureList_Header_Textures"),
            LOC.Get("TEXVIEW_TextureList_Header_Textures_TT"));

        EditorFilters.DisplayFramedListFilter("textureViewer_TextureList", ref TextureFileListFilter, ref ExactTextureFileListFilter);

        ImGui.BeginChild("TextureList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (View.Selection.SelectedTpf != null)
        {
            int index = 0;

            foreach (var entry in View.Selection.SelectedTpf.Textures)
            {
                var isMatch = EditorFilters.IsMatch(
                    TextureFileListFilter, entry.Name, ExactTextureFileListFilter, "", true);

                if (isMatch)
                {
                    var displayName = entry.Name;

                    var isSelected = false;
                    if (View.Selection.SelectedTextureKey == entry.Name)
                    {
                        isSelected = true;
                    }

                    // Texture row
                    if (ImGui.Selectable($@"{displayName}", isSelected))
                    {
                        View.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                        View.Editor.ViewHandler.ActiveView = View;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && View.Selection.SelectTexture)
                    {
                        View.Selection.SelectTexture = false;
                        View.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            View.Selection.SelectTexture = true;
                        }
                    }

                    if(index == 0 && View.Selection.AutoSelectTexture)
                    {
                        View.Selection.AutoSelectTexture = false;
                        View.Selection.SelectTexture = false;
                        View.Selection.SelectTextureEntry(entry.Name, entry);
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
            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_TextureList_Context_Action_Export")}##exportAction"))
            {
                _ = View.ToolView.TextureExport.ExportTextureAsync(tex);
            }
            UIHelper.Tooltip(
                LOC.Get("TEXVIEW_TextureList_Context_Action_Export_TT", CFG.Current.TextureViewerToolbar_ExportTextureLocation));

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_TextureList_Context_Action_Copy_Name")}##copyNameAction"))
            {
                ImGui.SetClipboardText(tex.Name);
            }
            UIHelper.Tooltip(LOC.Get("TEXVIEW_TextureList_Context_Action_Copy_Name_TT"));

            ImGui.EndPopup();
        }
    }
    private int TargetIndex = -1;
    public bool LoadTexture = false;

    public void Update()
    {
        if (LoadTexture)
        {
            if (TargetIndex != -1 && View.Selection.SelectedTpf != null)
            {
                View.Selection.ViewerTextureResource = new TextureResource(View.Selection.SelectedTpf, TargetIndex);
                View.Selection.ViewerTextureResource._LoadTexture(AccessLevel.AccessFull);
            }

            LoadTexture = false;
        }
    }
}
