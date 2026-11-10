using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        GUI.TitleHeader(
            LOC.Get("TEXVIEW_InternalFileList_Header_Files"),
            LOC.Get("TEXVIEW_InternalFileList_Header_Files_TT"));

        EditorFilters.DisplayFramedListFilter("textureViewer_TpfList", ref TpfFileListFilter, ref ExactTpfFileListFilter);

        ImGui.BeginChild("TpfList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (View.Selection.SelectedBinder != null)
        {
            int index = 0;

            // Binder order; a snapshot, as a context menu command can add or remove TPFs during the loop
            var container = View.Selection.SelectedBinder;
            var entries = container.OrderedFiles()
                .Select(f => new KeyValuePair<BinderFile, TPF>(f, container.Files[f]))
                .ToList();

            foreach (var entry in entries)
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

                    var isModified = View.Selection.SelectedBinder.ModifiedFiles.Contains(file);
                    if (isModified)
                    {
                        displayName = $"{displayName} *";
                        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_TextureEditor_ModifiedFile_Text);
                    }

                    // Texture row (the id stays the file name, so the row keeps its state when the marker appears)
                    var clicked = ImGui.Selectable($@"{displayName}###{file.Name}{index}", isSelected);

                    if (isModified)
                    {
                        ImGui.PopStyleColor();
                        GUI.Tooltip(LOC.Get("TEXVIEW_FileList_Modified_TT"));
                    }

                    if (clicked)
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

            // Textures of the selected TPF, PC only
            var target = View.GetEditTarget();
            if (target != null && target.Tpf == tpf)
            {
                ImGui.Separator();

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Add_Texture")}##addTexture", "", false, !View.TextureImport.IsRunning))
                {
                    View.TextureImport.AddFromDialog();
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_Add_Texture_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Replace_Folder")}##replaceFolder", "", false, !View.TextureImport.IsRunning))
                {
                    View.TextureImport.ReplaceMatchingFromFolderDialog();
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_TexImport_Replace_Folder_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Paste_Textures")}##pasteTextures", InputManager.GetHint(KeybindID.Paste), false, View.Commands.CanPaste))
                {
                    View.Commands.PasteTextures();
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_Paste_Textures_TT"));
            }

            // TPFs of the container: real binders of PC TPFs only
            if (View.GetEditableContainer() == View.Selection.SelectedBinder && View.Selection.SelectedBinder != null && View.Selection.SelectedBinder.Files.ContainsKey(entry))
            {
                ImGui.Separator();

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Add_TPF")}##addTpf"))
                {
                    View.Selection.SelectTpfFile(entry, tpf);
                    View.Commands.AddEmptyTpf();
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_Add_TPF_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Duplicate_TPF")}##duplicateTpf", InputManager.GetHint(KeybindID.Duplicate)))
                {
                    View.Commands.DuplicateTpf(entry);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_Duplicate_TPF_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Rename_TPF")}##renameTpf"))
                {
                    View.Commands.RenameTpf(entry);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_Rename_TPF_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_InternalFileList_Context_Delete_TPF")}##deleteTpf", InputManager.GetHint(KeybindID.Delete)))
                {
                    View.Commands.DeleteTpf(entry);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_InternalFileList_Context_Delete_TPF_TT"));
            }

            ImGui.EndPopup();
        }
    }
}
