using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Interface;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamFileView
{
    public GparamEditor Editor;
    public Project Project;

    public GparamFileView(Project curPoject, GparamEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    public void Draw()
    {
        ImGui.Begin("Files##GparamFileList");
        Editor.EditorFocus.SetFocusContext(GparamEditorContext.File);

        Editor.Filters.DisplayFileFilterSearch();

        ImGui.BeginChild("GparamFileSection");

        for (int i = 0; i < Project.GparamData.PrimaryBank.GraphicsParams.Count; i++)
        {
            var curEntry = Project.GparamData.PrimaryBank.GraphicsParams.ElementAt(i);

            var alias = AliasUtils.GetGparamAliasName(curEntry.Key);
            var isSelected = Editor.Selection.IsGparamSelected(curEntry.Key);

            var displayName = curEntry.Key;

            if (Editor.Filters.IsFileFilterMatch(displayName, alias))
            {
                ImGui.BeginGroup();

                // File row
                if (ImGui.Selectable($@" {displayName}", isSelected))
                {
                    Project.GparamData.PrimaryBank.LoadGraphicsParam(curEntry.Key);
                    Project.GparamEditor.Selection.SelectGparam(curEntry.Key, curEntry.Value);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.AutoSelectGparam)
                {
                    Editor.Selection.AutoSelectGparam = false;
                    Project.GparamData.PrimaryBank.LoadGraphicsParam(curEntry.Key);
                    Project.GparamEditor.Selection.SelectGparam(curEntry.Key, curEntry.Value);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.AutoSelectGparam = false;
                }

                if (CFG.Current.Interface_Display_Alias_for_Gparam)
                {
                    UIHelper.DisplayAlias(alias);
                }

                ImGui.EndGroup();
            }

            // Context Menu
            if (curEntry.Key == Editor.Selection._selectedGparamKey)
            {
                if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
                {
                    if (ImGui.Selectable("Target in Quick Edit"))
                    {
                        Editor.QuickEdit.UpdateFileFilter(curEntry.Key);

                        ImGui.CloseCurrentPopup();
                    }
                    UIHelper.Tooltip("Add this file to the File Filter in the Quick Edit window.");

                    ImGui.EndPopup();
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
