using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.Utilities;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamFileListView
{
    private GparamEditorScreen Editor;
    private GparamFilters Filters;
    private GparamSelection Selection;
    private GparamContextMenu ContextMenu;

    public GparamFileListView(GparamEditorScreen screen)
    {
        Editor = screen;
        Filters = screen.Filters;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Files##GparamFileList");
        Selection.SwitchWindowContext(GparamEditorContext.File);

        Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("GparamFileSection");
        Selection.SwitchWindowContext(GparamEditorContext.File);

        foreach (var entry in Editor.Project.GparamData.PrimaryBank.Entries)
        {
            var alias = AliasUtils.GetGparamAliasName(Editor.Project, entry.Key.Filename);

            if (Filters.IsFileFilterMatch(entry.Key.Filename, alias))
            {
                ImGui.BeginGroup();

                // File row
                if (ImGui.Selectable($@" {entry.Key.Filename}", entry.Key.Filename == Selection._selectedGparamKey))
                {
                    Selection.SetFileSelection(entry.Key);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Selection.SelectGparamFile)
                {
                    Selection.SelectGparamFile = false;

                    Selection.SetFileSelection(entry.Key);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Selection.SelectGparamFile = true;
                }

                if (CFG.Current.Interface_Display_Alias_for_Gparam)
                {
                    UIHelper.DisplayAlias(alias);
                }

                ImGui.EndGroup();
            }

            ContextMenu.FileContextMenu(entry.Key);
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
