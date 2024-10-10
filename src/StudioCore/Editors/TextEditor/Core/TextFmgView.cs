using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg selection, viewing and editing.
/// </summary>
public class TextFmgView
{
    public TextEditorScreen Screen;
    public TextPropertyDecorator Decorator;
    public TextSelectionManager Selection;
    public TextFilters Filters;
    public TextContextMenu ContextMenu;

    public TextFmgView(TextEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// The main UI for the fmg view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Text Files##fmgList");

        Filters.DisplayFmgFilterSearch();

        if (Selection.SelectedContainer != null && Selection.SelectedContainer.FmgInfos != null)
        {
            // Categories
            foreach (var fmgInfo in Selection.SelectedContainer.FmgInfos)
            {
                var id = fmgInfo.ID;
                var fmgName = fmgInfo.Name;
                var displayName = TextUtils.GetFmgDisplayName(Selection.SelectedContainer, id);

                if (Filters.IsFmgFilterMatch(fmgName, displayName, id))
                {
                    var selectableName = $"{displayName}";

                    if (!CFG.Current.TextEditor_DisplayFmgPrettyName)
                    {
                        selectableName = $"{fmgName}";
                    }

                    if (CFG.Current.TextEditor_DisplayFmgID)
                    {
                        selectableName = $"[{id}] {selectableName}";
                    }

                    // Script row
                    if (ImGui.Selectable(selectableName, id == Selection.SelectedFmgKey))
                    {
                        Selection.SelectFmg(fmgInfo);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNextFmg)
                    {
                        Selection.SelectNextFmg = false;
                        Selection.SelectFmg(fmgInfo);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectNextFmg = true;
                    }

                    // Only apply to selection
                    if (Selection.SelectedFmgKey != -1)
                    {
                        if (Selection.SelectedFmgKey == id)
                        {
                            ContextMenu.FmgContextMenu(fmgInfo);
                        }
                    }
                }
            }
        }

        ImGui.End();
    }
}
