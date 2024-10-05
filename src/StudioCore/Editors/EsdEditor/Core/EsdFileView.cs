using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Editors.TalkEditor;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using StudioCore.Utilities;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class EsdFileView
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;
    private EsdFilters Filters;
    private EsdContextMenu ContextMenu;

    public EsdFileView(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the file view
    /// </summary>
    public void Display()
    {
        // File List
        ImGui.Begin("Files##TalkFileList");

        ImGui.Text($"File");
        ImGui.Separator();

        foreach (var (info, binder) in EsdBank.TalkBank)
        {
            var displayName = $"{info.Name}";
            var aliasName = AliasUtils.GetMapNameAlias(info.Name);

            // File row
            if (ImGui.Selectable($@" {displayName}", displayName == Selection._selectedBinderKey))
            {
                Selection.ResetScript();
                Selection.ResetStateGroup();
                Selection.ResetStateGroupNode();

                Selection.SetFile(info, binder);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Selection.SelectNextFile)
            {
                Selection.SelectNextFile = false;
                Selection.SetFile(info, binder);
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
            {
                Selection.SelectNextFile = true;
            }

            UIHelper.DisplayAlias(aliasName);
        }

        ImGui.End();
    }
}
