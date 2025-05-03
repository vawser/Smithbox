using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editors.EsdEditor.Enums;
using StudioCore.Interface;
using StudioCore.TalkEditor;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the script selection, viewing and editing.
/// </summary>
public class EsdScriptView
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;
    private EsdFilters Filters;
    private EsdContextMenu ContextMenu;

    public EsdScriptView(EsdEditorScreen screen)
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
    /// The main UI for the view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Scripts##EsdScriptList");
        Selection.SwitchWindowContext(EsdEditorContext.Script);

        var info = Selection._selectedFileInfo;
        var scriptKey = Selection._selectedEsdScriptKey;

        Filters.DisplayScriptFilterSearch();

        ImGui.BeginChild("ScriptListSection");
        Selection.SwitchWindowContext(EsdEditorContext.Script);

        if (info != null)
        {
            for (int i = 0; i < info.EsdFiles.Count; i++)
            {
                ESD entry = info.EsdFiles[i];

                var displayName = $"{entry.Name}";
                var aliasName = displayName;

                if (Filters.IsScriptFilterMatch(displayName, aliasName))
                {
                    if (ImGui.Selectable($@" {displayName}", i == scriptKey))
                    {
                        Selection.ResetStateGroup();
                        Selection.ResetStateGroupNode();

                        Selection.SetScript(i, entry);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNextScript)
                    {
                        Selection.SelectNextScript = false;
                        Selection.SetScript(i, entry);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectNextScript = true;
                    }

                    // Only apply to selection
                    if (Selection._selectedEsdScriptKey != -1)
                    {
                        if (Selection._selectedEsdScriptKey == i)
                        {
                            ContextMenu.ScriptContextMenu(entry);
                        }
                    }

                    UIHelper.DisplayAlias(aliasName);
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
