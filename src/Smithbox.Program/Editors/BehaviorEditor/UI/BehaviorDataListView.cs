using Hexa.NET.ImGui;
using StudioCore;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using System.Net.Sockets;

namespace BehaviorEditorNS;

/// <summary>
/// The list of havok objects (i.e. all clip generators if the Clip Generator bucket is selected)
/// </summary>
public class BehaviorDataListView
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public bool DetectShortcuts = false;

    private BehaviorRowSelectMode CurrentSelectionMode;

    public BehaviorDataListView(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        DetectShortcuts = InterfaceUtils.UpdateShortcutDetection();

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
        CurrentSelectionMode = BehaviorRowSelectMode.ClearAndSelect;

        Shortcuts();

        if (Editor.Selection.SelectedCategoryObjects == null)
            return;

        ImGui.BeginChild("BehaviorDataListSection");

        // TODO: might want to use ListClipper here

        for (int i = 0; i < Editor.Selection.SelectedCategoryObjects.Count; i++)
        {
            var curObj = Editor.Selection.SelectedCategoryObjects[i];

            var displayName = GetDataEntryName(curObj);

            if (Editor.Filters.IsBasicMatch(
                ref Editor.Filters.DataEntriesInput, Editor.Filters.DataEntriesInput_IgnoreCase,
                displayName, ""))
            {
                var isSelected = Editor.Selection.IsObjectSelected(i);

                if (ImGui.Selectable($"[{i}] {displayName}", isSelected))
                {
                    Editor.Selection.SelectHavokObjectRow(i, curObj, CurrentSelectionMode);
                }

                if (CurrentSelectionMode is BehaviorRowSelectMode.SelectAll)
                {
                    Editor.Selection.SelectHavokObjectRow(i, curObj, CurrentSelectionMode);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.ForceSelectObject)
                {
                    Editor.Selection.ForceSelectObject = false;
                    Editor.Selection.SelectHavokObjectRow(i, curObj, CurrentSelectionMode);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.ForceSelectObject = true;
                }

                if (Editor.Selection.FocusObjectSelection && isSelected)
                {
                    Editor.Selection.FocusObjectSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }

        ImGui.EndChild();
    }

    public string GetDataEntryName(object curObj)
    {
        var displayName = BehaviorUtils.GetObjectFieldValue(curObj, "m_name");

        if (Project.ProjectType is ProjectType.ER)
        {
            // Special cases
            if (curObj.GetType() == typeof(HKLib.hk2018.hkbClipGenerator))
            {
                displayName = BehaviorUtils.GetObjectFieldValue(curObj, "m_animationName");
            }
        }

        return displayName;
    }

    public void Shortcuts()
    {
        if (DetectShortcuts)
        {
            // Append
            if (ImGui.IsKeyDown(ImGuiKey.LeftCtrl))
            {
                CurrentSelectionMode = BehaviorRowSelectMode.SelectAppend;
            }

            // Range Append
            if (ImGui.IsKeyDown(ImGuiKey.LeftShift))
            {
                CurrentSelectionMode = BehaviorRowSelectMode.SelectRangeAppend;
            }

            // Select All
            if(InputTracker.GetKeyDown(KeyBindings.Current.BEHAVIOR_SelectAll))
            {
                CurrentSelectionMode = BehaviorRowSelectMode.SelectAll;
            }

            // Focus Selection
            if (InputTracker.GetKeyDown(KeyBindings.Current.BEHAVIOR_FocusSelection))
            {
                Editor.Selection.FocusObjectSelection = true;
            }
        }
    }
}
