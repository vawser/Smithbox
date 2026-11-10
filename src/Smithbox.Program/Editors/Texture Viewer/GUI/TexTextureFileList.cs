using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        GUI.TitleHeader(
            LOC.Get("TEXVIEW_TextureList_Header_Textures"),
            LOC.Get("TEXVIEW_TextureList_Header_Textures_TT"));

        EditorFilters.DisplayFramedListFilter("textureViewer_TextureList", ref TextureFileListFilter, ref ExactTextureFileListFilter);

        ImGui.BeginChild("TextureList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        if (View.Selection.SelectedTpf != null)
        {
            int index = 0;

            // A snapshot, as a context menu command can add or remove textures during the loop
            foreach (var entry in View.Selection.SelectedTpf.Textures.ToList())
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
                _ = View.ToolView.TextureExport.ExportTextureAsync(View.Selection.SelectedTpf, tex);
            }
            GUI.Tooltip(
                LOC.Get("TEXVIEW_TextureList_Context_Action_Export_TT", CFG.Current.TextureViewerToolbar_ExportTextureLocation));

            ImGui.Separator();

            if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_TextureList_Context_Action_Copy_Name")}##copyNameAction"))
            {
                ImGui.SetClipboardText(tex.Name);
            }
            GUI.Tooltip(LOC.Get("TEXVIEW_TextureList_Context_Action_Copy_Name_TT"));

            // Editing: PC TPFs only (console writers are T8)
            var target = View.GetEditTarget();
            if (target != null)
            {
                ImGui.Separator();

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_TextureList_Context_Action_Replace")}##replaceAction", "", false, !View.TextureImport.IsRunning))
                {
                    View.TextureImport.ReplaceFromDialog(tex);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_TextureList_Context_Action_Replace_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_TextureList_Context_Action_Copy")}##copyAction", InputManager.GetHint(KeybindID.Copy)))
                {
                    View.Commands.CopyTexture(target.Tpf, tex);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_TextureList_Context_Action_Copy_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_TextureList_Context_Action_Paste")}##pasteAction", InputManager.GetHint(KeybindID.Paste), false, View.Commands.CanPaste))
                {
                    View.Selection.SelectTextureEntry(tex.Name, tex);
                    View.Commands.PasteTextures();
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_TextureList_Context_Action_Paste_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_TextureList_Context_Action_Duplicate")}##duplicateAction", InputManager.GetHint(KeybindID.Duplicate)))
                {
                    Duplicate(target, tex);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_TextureList_Context_Action_Duplicate_TT"));

                if (ImGui.MenuItem($"{LOC.Get("TEXVIEW_TextureList_Context_Action_Delete")}##deleteAction", InputManager.GetHint(KeybindID.Delete)))
                {
                    Delete(target, tex);
                }
                GUI.Tooltip(LOC.Get("TEXVIEW_TextureList_Context_Action_Delete_TT"));
            }
            else if (View.Selection.SelectedTpf != null && !TextureFormats.IsEditable(View.Selection.SelectedTpf))
            {
                ImGui.Separator();
                ImGui.TextDisabled(LOC.Get("TEXVIEW_TexImport_Read_Only_Platform", View.Selection.SelectedTpf.Platform));
            }

            ImGui.EndPopup();
        }
    }

    public void Delete(TextureTarget target, TPF.Texture tex)
    {
        if (target == null || tex == null || !target.Tpf.Textures.Contains(tex))
            return;

        View.ActionManager.ExecuteAction(new TextureInsertRemoveAction(View, target, tex, target.Tpf.Textures.IndexOf(tex), false));
    }

    /// <summary>A copy of the texture after it, named {name}_copy (then _copy_1, ...).</summary>
    public void Duplicate(TextureTarget target, TPF.Texture tex)
    {
        if (target == null || tex == null || !target.Tpf.Textures.Contains(tex))
            return;

        var used = new HashSet<string>(target.Tpf.Textures.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);
        var name = $"{tex.Name}_copy";
        for (int n = 1; used.Contains(name); n++)
            name = $"{tex.Name}_copy_{n}";

        var state = TextureState.Capture(tex);
        state.Name = name;

        var copy = state.CreateTexture(target.Tpf.Platform);
        View.ActionManager.ExecuteAction(new TextureInsertRemoveAction(View, target, copy, target.Tpf.Textures.IndexOf(tex) + 1, true));
    }
    private int TargetIndex = -1;
    public bool LoadTexture = false;

    /// <summary>
    /// Selects a texture of the selected TPF and (re)loads it into the viewport, e.g. after an edit or an undo.
    /// Null selects nothing.
    /// </summary>
    public void SelectAndLoad(TPF.Texture tex)
    {
        var tpf = View.Selection.SelectedTpf;

        if (tpf == null || tex == null || !tpf.Textures.Contains(tex))
        {
            View.Selection.SelectTextureEntry("", null);
            View.Selection.ViewerTextureResource?.Dispose();
            View.Selection.ViewerTextureResource = null;
            TargetIndex = -1;
            return;
        }

        View.Selection.SelectTextureEntry(tex.Name, tex);
        TargetIndex = tpf.Textures.IndexOf(tex);
        LoadTexture = true;
    }

    public void Update()
    {
        if (LoadTexture)
        {
            if (TargetIndex != -1 && View.Selection.SelectedTpf != null && TargetIndex < View.Selection.SelectedTpf.Textures.Count)
            {
                // The previous texture's GPU descriptor is freed, as edits reload the same texture many times
                View.Selection.ViewerTextureResource?.Dispose();
                View.Selection.ViewerTextureResource = new TextureResource(View.Selection.SelectedTpf, TargetIndex);
                View.Selection.ViewerTextureResource._LoadTexture(AccessLevel.AccessFull);
            }

            LoadTexture = false;
        }
    }
}
