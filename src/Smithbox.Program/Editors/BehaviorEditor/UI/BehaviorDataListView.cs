using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;

namespace BehaviorEditorNS;

/// <summary>
/// The list of havok objects (i.e. all clip generators if the Clip Generator bucket is selected)
/// </summary>
public class BehaviorDataListView
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorDataListView(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        DisplayHeader();
        DisplayCategories();
    }

    private void DisplayHeader()
    {
        var curInput = Editor.Filters.DataEntriesInput;
        ImGui.InputText($"Search##searchBar_DataEntries", ref curInput, 255);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            Editor.Filters.DataEntriesInput = curInput;
        }

        ImGui.SameLine();
        ImGui.Checkbox($"##searchIgnoreCase_DataEntries", ref Editor.Filters.DataEntriesInput_IgnoreCase);
        UIHelper.Tooltip("If enabled, the search will ignore case.");
    }

    public void DisplayCategories()
    {
        if (Editor.Selection.SelectedFieldObjects == null)
            return;

        ImGui.BeginChild("BehaviorDataListSection");

        for (int i = 0; i < Editor.Selection.SelectedFieldObjects.Count; i++)
        {
            var curObj = Editor.Selection.SelectedFieldObjects[i];

            var displayName = BehaviorUtils.GetObjectFieldValue(curObj, "m_name");

            if (Project.ProjectType is ProjectType.ER)
            {
                // Special cases
                if (curObj.GetType() == typeof(HKLib.hk2018.hkbClipGenerator))
                {
                    displayName = BehaviorUtils.GetObjectFieldValue(curObj, "m_animationName");
                }
            }

            var isSelected = Editor.Selection.IsHavokObjectSelected(i);

            if (Editor.Filters.IsBasicMatch(
                ref Editor.Filters.DataEntriesInput, Editor.Filters.DataEntriesInput_IgnoreCase,
                displayName, ""))
            {
                if (ImGui.Selectable($"[{i}] {displayName}", isSelected))
                {
                    Editor.Selection.SelectHavokObject(i, curObj);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.ForceSelectObject)
                {
                    Editor.Selection.ForceSelectObject = false;
                    Editor.Selection.SelectHavokObject(i, curObj);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.ForceSelectObject = true;
                }
                // Only apply to selection
                if (isSelected)
                {
                    DisplayContextMenu(curObj, i);
                }
            }
        }

        ImGui.EndChild();
    }

    private void DisplayContextMenu(object curObj, int index)
    {
        if (ImGui.BeginPopupContextItem($"##HavokObjectContextMenu{index}"))
        {

            ImGui.EndPopup();
        }
    }
}
