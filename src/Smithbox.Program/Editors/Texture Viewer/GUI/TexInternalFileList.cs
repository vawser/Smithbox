using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexInternalFileList
{
    public TexEditorView View;
    public ProjectEntry Project;

    private string TpfFileListFilter = "";
    private bool ExactTpfFileListFilter = false;

    public TexInternalFileList(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display(float width, float height)
    {
        GUI.SimpleHeader(
            LOC.Get("TEXVIEW_InternalFileList_Header_Files"),
            LOC.Get("TEXVIEW_InternalFileList_Header_Files_TT"));

        EditorFilters.DisplayFramedListFilter("textureViewer_TpfList", ref TpfFileListFilter, ref ExactTpfFileListFilter);

        ImGui.BeginChild("TpfList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (View.Selection.SelectedBinder != null)
        {
            int index = 0;

            foreach (var entry in View.Selection.SelectedBinder.Files)
            {
                var file = entry.Key;
                var tpfEntry = entry.Value;

                var isMatch = EditorFilters.IsMatch(
                    TpfFileListFilter, file.Name, ExactTpfFileListFilter, "", true);

                if (isMatch)
                {
                    var displayName = file.Name;

                    var isSelected = false;
                    if(View.Selection.SelectedTpfKey == file.Name)
                    {
                        isSelected = true;
                    }

                    // Texture row
                    if (ImGui.Selectable($@"{displayName}", isSelected))
                    {
                        View.Selection.SelectTpfFile(entry.Key, entry.Value);
                        View.Editor.ViewHandler.ActiveView = View;
                        View.Selection.AutoSelectTexture = true;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && View.Selection.SelectTpf)
                    {
                        View.Selection.SelectTpf = false;
                        View.Selection.SelectTpfFile(entry.Key, entry.Value);
                        View.Selection.AutoSelectTexture = true;
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            View.Selection.SelectTpf = true;
                        }
                    }

                    if (index == 0 && View.Selection.AutoSelectTpf)
                    {
                        View.Selection.AutoSelectTpf = false;
                        View.Selection.SelectTpfFile(entry.Key, entry.Value);
                        View.Editor.ViewHandler.ActiveView = View;
                        View.Selection.AutoSelectTexture = true;
                    }
                }

                ContextMenu(file, tpfEntry, index);

                index++;
            }
        }

        ImGui.EndChild();
    }

    private void ContextMenu(BinderFile entry, TPF tpf, int index)
    {
        var filename = Path.GetFileName(entry.Name);

        if (ImGui.BeginPopupContextItem($"context_{entry.Name}{index}"))
        {
            if(ImGui.BeginMenu($"{LOC.Get("TEXVIEW_InternalFileList_Context_Header_Export")}##exportMenuHeader"))
            {
                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_TPF")}##tpfAction"))
                {
                    _ = View.ToolView.TextureExport.ExportTPFAsync(tpf, filename);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_TPF_TT", CFG.Current.TextureViewerToolbar_ExportTextureLocation));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_All_Textures")}##allTexturesAction"))
                {
                    _ = View.ToolView.TextureExport.ExportTexturesFromTPFAsync(tpf);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_All_Textures_TT", CFG.Current.TextureViewerToolbar_ExportTextureLocation));

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Copy_Path")}##copyPath"))
            {
                ImGui.SetClipboardText(entry.Name);
            }
            GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_Copy_Path_TT"));

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Copy_Filename")}##copyFilename"))
            {
                ImGui.SetClipboardText(filename);
            }
            GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_Copy_Filename_TT"));
            ImGui.EndPopup();
        }
    }
}
