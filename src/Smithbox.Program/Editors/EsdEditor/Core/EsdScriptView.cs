using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using System.Linq;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Handles the script selection, viewing and editing.
/// </summary>
public class EsdScriptView
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public EsdScriptView(EsdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
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
        Editor.Selection.SwitchWindowContext(EsdEditorContext.Script);

        var fileEntry = Editor.Selection.SelectedFileEntry;
        var scriptKey = Editor.Selection.SelectedScriptIndex;

        Editor.Filters.DisplayScriptFilterSearch();

        ImGui.BeginChild("ScriptListSection");
        Editor.Selection.SwitchWindowContext(EsdEditorContext.Script);

        if (Editor.Selection.SelectedFileEntry != null)
        {
            var scripts = Project.EsdData.PrimaryBank.Scripts.FirstOrDefault(e => e.Key == Editor.Selection.SelectedFileEntry);

            if (scripts.Value != null)
            {
                for (int i = 0; i < scripts.Value.Files.Count; i++)
                {
                    var dictEntry = scripts.Value.Files.ElementAt(i);
                    var entry = dictEntry.Value;

                    var displayName = $"{entry.Name}";
                    var aliasName = displayName;

                    if (Editor.Filters.IsScriptFilterMatch(displayName, aliasName))
                    {
                        if (ImGui.Selectable($@" {displayName}", i == scriptKey))
                        {
                            Editor.Selection.ResetStateGroup();
                            Editor.Selection.ResetStateGroupNode();

                            Editor.Selection.SetScript(i, entry);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Editor.Selection.SelectNextScript)
                        {
                            Editor.Selection.SelectNextScript = false;
                            Editor.Selection.SetScript(i, entry);
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Editor.Selection.SelectNextScript = true;
                        }

                        // Only apply to selection
                        if (Editor.Selection.SelectedScriptIndex != -1)
                        {
                            if (Editor.Selection.SelectedScriptIndex == i)
                            {
                                Editor.ContextMenu.ScriptContextMenu(entry);
                            }
                        }

                        UIHelper.DisplayAlias(aliasName);
                    }
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
