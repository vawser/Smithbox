using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class SelectionGroupTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    public string ListFilter = "";
    public bool ExactListFilter = false;

    public SelectionGroupTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private string selectedResourceName = "";
    private List<string> selectedResourceTags = new List<string>();
    private List<string> selectedResourceContents = new List<string>();
    private int selectedResourceKeybind = -1;

    private string createPromptGroupName = "";
    private string createPromptTags = "";

    private string editPromptGroupName = "";
    private string editPromptTags = "";
    private int editPromptKeybind = -1;

    private string editPromptOldGroupName = "";

    private int currentKeyBindOption = -1;
    private List<int> keyBindOptions = new List<int>() { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    public bool DisplayNewGroupForm = true;

    public SelectionGroupMode Mode = SelectionGroupMode.View;

    /// <summary>
    /// Shortcuts
    /// </summary>
    public void OnShortcut()
    {
        if (CFG.Current.MapEditor_Selection_Group_Enable_Shortcuts)
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_0))
            {
                ShortcutSelectGroup(0);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_1))
            {
                ShortcutSelectGroup(1);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_2))
            {
                ShortcutSelectGroup(2);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_3))
            {
                ShortcutSelectGroup(3);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_4))
            {
                ShortcutSelectGroup(4);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_5))
            {
                ShortcutSelectGroup(5);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_6))
            {
                ShortcutSelectGroup(6);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_7))
            {
                ShortcutSelectGroup(7);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_8))
            {
                ShortcutSelectGroup(8);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_9))
            {
                ShortcutSelectGroup(9);
            }
            if (InputManager.IsPressed(KeybindID.MapEditor_Select_Group_10))
            {
                ShortcutSelectGroup(10);
            }
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        if (View.Project.Descriptor.ProjectType == ProjectType.Undefined)
            return;

        if (View.Project.Handler.MapData.MapObjectSelections == null)
            return;

        if (View.Project.Handler.MapData.MapObjectSelections.Resources == null)
            return;

        GUI.WrappedText("Use this to define groups of map objects under a name. You can then quickly re-select the group via this tool.");

        GUI.Spacer();
        GUI.SimpleHeader("Selection Groups", "");

        EditorFilters.DisplayFramedListFilter("selectionGroupFilter", ref ListFilter, ref ExactListFilter);

        ImGui.BeginChild("##selectionGroupList", new Vector2(0, 250f), ImGuiChildFlags.Borders);

        foreach (var entry in View.Project.Handler.MapData.MapObjectSelections.Resources)
        {
            var displayName = $"{entry.Name}";
            var tagString = "";

            if (CFG.Current.MapEditor_Selection_Group_Show_Keybind)
            {
                if (entry.SelectionGroupKeybind != -1)
                {
                    var hint = GetSelectionGroupKeyBind(entry.SelectionGroupKeybind);
                    if (hint != "None")
                    {
                        displayName = $"{displayName} [{hint}]";
                    }
                }
            }

            if (CFG.Current.MapEditor_Selection_Group_Show_Tags)
            {
                if (entry.Tags.Count > 0)
                {
                    tagString = string.Join(" ", entry.Tags);
                    displayName = $"{displayName} {{ {tagString} }}";
                }
            }

            var isMatch = EditorFilters.IsMatch(ListFilter, entry.Name, ExactListFilter, tagString);

            if (!isMatch)
                continue;

            if (ImGui.Selectable(displayName, selectedResourceName == entry.Name))
            {
                selectedResourceName = entry.Name;
                selectedResourceTags = entry.Tags;
                selectedResourceContents = entry.Selection;
                selectedResourceKeybind = entry.SelectionGroupKeybind;

                editPromptOldGroupName = entry.Name;
                editPromptGroupName = entry.Name;
                editPromptTags = AliasHelper.GetTagListString(entry.Tags);
                editPromptKeybind = entry.SelectionGroupKeybind;

                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    SelectSelectionGroup();
                }
            }
        }
        ImGui.EndChild();

        GUI.Spacer();
        GUI.SimpleHeader("Mode", "Determines if we are creating a new group, or editing an existing one.");

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##modeType", Mode.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SelectionGroupMode)))
            {
                var curMode = (SelectionGroupMode)entry;

                if (ImGui.Selectable($"{curMode.GetDisplayName()}", curMode == Mode))
                {
                    Mode = curMode;
                }
            }

            ImGui.EndCombo();
        }

        if (Mode is SelectionGroupMode.View)
        {
            GUI.Spacer();
            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("viewActions",
                "selectGroup", "Select", "", SelectSelectionGroup);

            GUI.Spacer();
            GUI.SimpleHeader("Group Contents", "");

            if (selectedResourceTags.Count > 0)
            {
                var tagString = string.Join(" ", selectedResourceTags);
                if (tagString != "")
                {
                    GUI.WrappedText("");
                    GUI.WrappedText("Tags:");
                    GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color, tagString);
                    GUI.WrappedText("");
                }
            }

            foreach (var entry in selectedResourceContents)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, entry);
            }
        }

        if (Mode is SelectionGroupMode.Create)
        {
            GUI.Spacer();
            GUI.SimpleHeader("New Group Name", "");

            GUI.HintTextInput("Group Name", ref createPromptGroupName, "Enter the name of the group...");

            GUI.Spacer();
            GUI.SimpleHeader("New Group Tags", "");

            GUI.HintTextInput("Tags", ref createPromptTags, "Enter the tags associated with the group, split with the , character...");

            GUI.Spacer();
            GUI.SimpleHeader("New Group Keybind", "");

            var previewString = GetSelectionGroupKeyBind(currentKeyBindOption);

            if (ImGui.BeginCombo("##keybindCombo", previewString))
            {
                foreach (var entry in keyBindOptions)
                {
                    var nameString = GetSelectionGroupKeyBind(entry);

                    bool isSelected = currentKeyBindOption == entry;

                    if (ImGui.Selectable($"{nameString}##{entry}", isSelected))
                    {
                        currentKeyBindOption = entry;
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip("The keybind to quickly select the contents of this selection group.");

            GUI.Spacer();
            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("createActions",
                "createGroup", "Create Group", "", CreateSelectionGroup);
        }

        if(Mode is SelectionGroupMode.Edit)
        {
            GUI.Spacer();
            GUI.SimpleHeader("Edit Group Name", "");

            GUI.HintTextInput("Group Name", ref editPromptGroupName, "Enter the name of the group...");

            GUI.Spacer();
            GUI.SimpleHeader("Edit Group Tags", "");

            GUI.HintTextInput("Tags", ref editPromptTags, "Enter the tags associated with the group, split with the , character...");

            GUI.Spacer();
            GUI.SimpleHeader("Edit Group Keybind", "");

            var previewString = GetSelectionGroupKeyBind(editPromptKeybind);

            if (ImGui.BeginCombo("##keybindCombo", previewString))
            {
                foreach (var entry in keyBindOptions)
                {
                    var nameString = GetSelectionGroupKeyBind(entry);

                    bool isSelected = editPromptKeybind == entry;

                    if (ImGui.Selectable($"{nameString}##{entry}", isSelected))
                    {
                        editPromptKeybind = entry;
                    }
                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip("The keybind to quickly select the contents of this selection group.");

            GUI.Spacer();
            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("editActions",
                "editGroup", "Finalize Edit", "", EditSelectionGroup,
                "deleteGroup", "Delete", "", DeleteSelectionGroup);

            GUI.Spacer();
            GUI.SimpleHeader("Group Contents", "");

            if (selectedResourceTags.Count > 0)
            {
                var tagString = string.Join(" ", selectedResourceTags);
                if (tagString != "")
                {
                    GUI.WrappedText("");
                    GUI.WrappedText("Tags:");
                    GUI.WrappedTextColored(UI.Current.ImGui_Default_Text_Color, tagString);
                    GUI.WrappedText("");
                }
            }

            foreach (var entry in selectedResourceContents)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, entry);
            }
        }
    }

    public bool DeleteSelectionGroup(string currentResourceName)
    {
        var resource = View.Project.Handler.MapData.MapObjectSelections.Resources.Where(x => x.Name == currentResourceName).FirstOrDefault();

        View.Project.Handler.MapData.MapObjectSelections.Resources.Remove(resource);
        View.Project.Handler.MapData.SaveMapObjectSelections();

        return true;
    }

    public bool AddSelectionGroup(string name, List<string> tags, List<string> selection, int keybindIndex, bool isEdit = false, string oldName = "")
    {
        if (name == "")
        {
            PlatformUtils.Instance.MessageBox("Group name is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (!isEdit && View.Project.Handler.MapData.MapObjectSelections.Resources.Any(x => x.Name == name))
        {
            PlatformUtils.Instance.MessageBox("Group name already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (!isEdit && selection == null)
        {
            PlatformUtils.Instance.MessageBox("Selection is invalid.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (!isEdit && selection.Count == 0)
        {
            PlatformUtils.Instance.MessageBox("Selection is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (keybindIndex != -1 && View.Project.Handler.MapData.MapObjectSelections.Resources.Any(x => x.SelectionGroupKeybind == keybindIndex))
        {
            var group = View.Project.Handler.MapData.MapObjectSelections.Resources.Where(x => x.SelectionGroupKeybind == keybindIndex).First();
            if (isEdit)
            {
                group = View.Project.Handler.MapData.MapObjectSelections.Resources.Where(x => x.SelectionGroupKeybind == keybindIndex && x.Name != name).First();
            }
            PlatformUtils.Instance.MessageBox($"Keybind already assigned to another selection group: {group.Name}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else
        {
            // Delete old entry, since we will create it a-new with the edits immediately
            if (isEdit)
            {
                DeleteSelectionGroup(oldName);
            }

            var res = new EntitySelectionGroupResource();
            res.Name = name;
            res.Tags = tags;
            res.Selection = selection;
            res.SelectionGroupKeybind = keybindIndex;

            View.Project.Handler.MapData.MapObjectSelections.Resources.Add(res);
            View.Project.Handler.MapData.SaveMapObjectSelections();
        }

        return false;
    }

    private void DeleteSelectionGroup()
    {
        var result = DialogResult.Yes;

        if (CFG.Current.MapEditor_Selection_Group_Confirm_Delete)
        {
            result = PlatformUtils.Instance.MessageBox($"You are about to delete this selection group. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        }

        if (result == DialogResult.Yes)
        {
            DeleteSelectionGroup(selectedResourceName);

            selectedResourceName = "";
            selectedResourceTags = new List<string>();
            selectedResourceContents = new List<string>();

            View.Project.Handler.MapData.SaveMapObjectSelections();
        }
    }

    private void SelectSelectionGroup()
    {
        if(selectedResourceContents == null || selectedResourceContents.Count == 0)
        {
            Smithbox.LogError<SelectionGroupTool>("No group has been selected.");
            return;
        }

        View.ViewportSelection.ClearSelection();

        List<Entity> entities = new List<Entity>();

        // TODO: add something to prevent confusion if multiple maps are loaded with the same names within
        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                foreach (var mapObj in entry.Value.MapContainer.Objects)
                {
                    if (selectedResourceContents.Contains(mapObj.Name))
                    {
                        //Smithbox.Log(this, mapObj.Name);
                        entities.Add(mapObj);
                    }
                }
            }
        }

        foreach (var entry in entities)
        {
            View.ViewportSelection.AddSelection(entry);
        }

        if (CFG.Current.MapEditor_Selection_Group_Frame_Selection_On_Use)
        {
            View.FrameAction.ApplyViewportFrame();
            View.GotoAction.GotoMapObjectEntry();
        }
    }

    private void CreateSelectionGroup()
    {
        List<string> tagList = AliasHelper.GetTagList(createPromptTags);
        List<string> selectionList = new List<string>();

        foreach (var entry in View.ViewportSelection.GetSelection())
        {
            var ent = (Entity)entry;
            selectionList.Add(ent.Name);
        }

        if (AddSelectionGroup(createPromptGroupName, tagList, selectionList, currentKeyBindOption, false, editPromptOldGroupName))
        {
            View.Project.Handler.MapData.SaveMapObjectSelections();
        }
    }

    private void EditSelectionGroup()
    {
        createPromptGroupName = editPromptGroupName;
        createPromptTags = editPromptTags;
        currentKeyBindOption = editPromptKeybind;

        List<string> tagList = AliasHelper.GetTagList(createPromptTags);
        List<string> selectionList = new List<string>();

        selectionList = selectedResourceContents;

        if (AddSelectionGroup(createPromptGroupName, tagList, selectionList, currentKeyBindOption, true, editPromptOldGroupName))
        {
            View.Project.Handler.MapData.SaveMapObjectSelections();
        }
    }

    public void ShortcutSelectGroup(int index)
    {
        if (View.Project.Handler.MapData.MapObjectSelections.Resources == null)
            return;

        foreach (var entry in View.Project.Handler.MapData.MapObjectSelections.Resources)
        {
            if (entry.SelectionGroupKeybind == index)
            {
                selectedResourceName = entry.Name;
                selectedResourceTags = entry.Tags;
                selectedResourceContents = entry.Selection;

                SelectSelectionGroup();
            }
        }
    }

    private string GetSelectionGroupKeyBind(int index)
    {
        if (index == -1)
        {
            return "None";
        }

        switch (index)
        {
            case 0: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_0);
            case 1: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_1);
            case 2: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_2);
            case 3: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_3);
            case 4: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_4);
            case 5: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_5);
            case 6: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_6);
            case 7: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_7);
            case 8: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_8);
            case 9: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_9);
            case 10: return InputManager.GetHint(KeybindID.MapEditor_Select_Group_10);
            default: return "None";
        }
    }
}

public enum SelectionGroupMode
{
    [Display(Name = "View")]
    View,
    [Display(Name = "Create")]
    Create,
    [Display(Name = "Edit")]
    Edit
}