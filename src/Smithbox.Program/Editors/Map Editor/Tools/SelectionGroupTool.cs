using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static StudioCore.Keybinds.InputManager;

namespace StudioCore.Editors.MapEditor;

public class SelectionGroupTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public SelectionGroupTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
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

    private string _searchInput = "";

    private int currentKeyBindOption = -1;
    private List<int> keyBindOptions = new List<int>() { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    public bool OpenPopup = false;

    /// <summary>
    /// Update Loop
    /// </summary>
    public void OnGui()
    {
        if (Editor.Project.Descriptor.ProjectType == ProjectType.Undefined)
            return;

        if (Editor.Project.Handler.MapData.MapObjectSelections.Resources == null)
            return;

        if (OpenPopup)
        {
            CreateSelectionGroup("External");
            OpenPopup = false;
        }

        // This exposes the pop-up to the map editor
        if (ImGui.BeginPopup("##selectionGroupModalExternal"))
        {
            DisplayCreationModal();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Shortcuts
    /// </summary>
    public void OnShortcut()
    {
        if (CFG.Current.MapEditor_Selection_Group_Enable_Shortcuts)
        {
            // Selection Groups
            if (InputManager.IsPressed(KeybindID.MapEditor_Create_Selection_Group))
            {
                CreateSelectionGroup("External");
            }

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
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Create Selection Group"))
        {
            OpenPopup = true;
        }
        UIHelper.Tooltip($"Create a selection group from the current selection.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Create_Selection_Group)}");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        if (Editor.Project.Descriptor.ProjectType == ProjectType.Undefined)
            return;

        if (Editor.Project.Handler.MapData.MapObjectSelections.Resources == null)
            return;

        if (ImGui.CollapsingHeader("Selection Groups"))
        {
            var windowSize = DPI.GetWindowSize(Smithbox.Instance._context);
            var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
            var topSectionHeight = windowSize.Y * 0.1f;
            var bottomSectionHeight = windowSize.Y * 0.3f;
            var topSectionSize = new Vector2(sectionWidth * DPI.UIScale(), topSectionHeight * DPI.UIScale());
            var bottomSectionSize = new Vector2(sectionWidth * DPI.UIScale(), bottomSectionHeight * DPI.UIScale());

            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginPopup("##selectionGroupModalInternal"))
            {
                DisplayCreationModal();

                ImGui.EndPopup();
            }

            UIHelper.SimpleHeader("Selection Groups", "Selection Groups", "", UI.Current.ImGui_Default_Text_Color);

            ImGui.BeginChild("##selectionGroupList", topSectionSize, ImGuiChildFlags.Borders);

            ImGui.InputText($"##selectionGroupFilter", ref _searchInput, 255);
            UIHelper.Tooltip("Filter the selection group list. Separate terms are split via the + character.");

            ImGui.Separator();

            foreach (var entry in Editor.Project.Handler.MapData.MapObjectSelections.Resources)
            {
                var displayName = $"{entry.Name}";

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
                        var tagString = string.Join(" ", entry.Tags);
                        displayName = $"{displayName} {{ {tagString} }}";
                    }
                }

                if (SearchFilters.IsSelectionSearchMatch(_searchInput, entry.Name, entry.Tags))
                {
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
            }
            ImGui.EndChild();

            if (ImGui.Button("Create New Selection Group", DPI.WholeWidthButton(windowWidth, 24)))
            {
                CreateSelectionGroup("Internal");
            }
            UIHelper.Tooltip($"Shortcut: {InputManager.GetHint(KeybindID.MapEditor_Create_Selection_Group)}\nBring up the selection group creation menu to assign your current selection to a selection group.");


            UIHelper.SimpleHeader("Current Selection Group", "Current Selection Group", "", UI.Current.ImGui_Default_Text_Color);

            ImGui.BeginChild("##selectionGroupActions", bottomSectionSize, ImGuiChildFlags.Borders);

            if (selectedResourceName != "")
            {
                if (ImGui.Button("Select", DPI.ThirdWidthButton(bottomSectionSize.X, 24)))
                {
                    SelectSelectionGroup();
                }
                UIHelper.Tooltip("Select the map objects listed by your currently selected group.");

                ImGui.SameLine();

                if (ImGui.Button("Edit", DPI.ThirdWidthButton(bottomSectionSize.X, 24)))
                {
                    ImGui.OpenPopup($"##selectionGroupModalEdit");
                }
                UIHelper.Tooltip("Edit the name, tags and keybind for the selected group.");

                ImGui.SameLine();

                if (ImGui.Button("Delete", DPI.ThirdWidthButton(bottomSectionSize.X, 24)))
                {
                    DeleteSelectionGroup();
                }
                UIHelper.Tooltip("Delete this selected group.");

                if (ImGui.BeginPopup("##selectionGroupModalEdit"))
                {
                    DisplayEditModal();

                    ImGui.EndPopup();
                }

                if (selectedResourceTags.Count > 0)
                {
                    var tagString = string.Join(" ", selectedResourceTags);
                    if (tagString != "")
                    {
                        UIHelper.WrappedText("");
                        UIHelper.WrappedText("Tags:");
                        UIHelper.WrappedTextColored(UI.Current.ImGui_Default_Text_Color, tagString);
                        UIHelper.WrappedText("");
                    }
                }

                UIHelper.WrappedText("Contents:");
                foreach (var entry in selectedResourceContents)
                {
                    UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, entry);
                }
            }

            ImGui.EndChild();
        }
    }

    private void DisplayCreationModal()
    {
        var windowWidth = ImGui.GetWindowWidth();

        ImGui.InputText("Group Name##selectionGroup_GroupName", ref createPromptGroupName, 255);
        UIHelper.Tooltip("The name of the selection group.");
        ImGui.InputText("Tags##selectionGroup_Tags", ref createPromptTags, 255);
        UIHelper.Tooltip("Separate each tag with the , character as a delimiter.");

        var previewString = GetSelectionGroupKeyBind(currentKeyBindOption);

        if (ImGui.BeginCombo("Keybind##keybindCombo", previewString))
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
        UIHelper.Tooltip("The keybind to quickly select the contents of this selection group.");

        if (ImGui.Button("Create Group", DPI.WholeWidthButton(windowWidth, 24)))
        {
            AmendSelectionGroupBank();
            ImGui.CloseCurrentPopup();
        }
    }

    private void DisplayEditModal()
    {
        var windowWidth = ImGui.GetWindowWidth();

        ImGui.InputText("Group Name##selectionGroup_GroupName", ref editPromptGroupName, 255);
        UIHelper.Tooltip("The name of the selection group.");
        ImGui.InputText("Tags##selectionGroup_Tags", ref editPromptTags, 255);
        UIHelper.Tooltip("Separate each tag with the , character as a delimiter.");

        var previewString = GetSelectionGroupKeyBind(editPromptKeybind);
        
        if (ImGui.BeginCombo("Keybind##keybindCombo", previewString))
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
        UIHelper.Tooltip("The keybind to quickly select the contents of this selection group.");

        if (ImGui.Button("Edit Group", DPI.WholeWidthButton(windowWidth, 24)))
        {
            createPromptGroupName = editPromptGroupName;
            createPromptTags = editPromptTags;
            currentKeyBindOption = editPromptKeybind;

            AmendSelectionGroupBank(true);
            ImGui.CloseCurrentPopup();
        }
    }

    /// <summary>
    /// Effect
    /// </summary>

    public bool DeleteSelectionGroup(string currentResourceName)
    {
        var resource = Editor.Project.Handler.MapData.MapObjectSelections.Resources.Where(x => x.Name == currentResourceName).FirstOrDefault();

        Editor.Project.Handler.MapData.MapObjectSelections.Resources.Remove(resource);
        Editor.Project.Handler.MapData.SaveMapObjectSelections();

        return true;
    }

    public bool AddSelectionGroup(string name, List<string> tags, List<string> selection, int keybindIndex, bool isEdit = false, string oldName = "")
    {
        if (name == "")
        {
            PlatformUtils.Instance.MessageBox("Group name is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (!isEdit && Editor.Project.Handler.MapData.MapObjectSelections.Resources.Any(x => x.Name == name))
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
        else if (keybindIndex != -1 && Editor.Project.Handler.MapData.MapObjectSelections.Resources.Any(x => x.SelectionGroupKeybind == keybindIndex))
        {
            var group = Editor.Project.Handler.MapData.MapObjectSelections.Resources.Where(x => x.SelectionGroupKeybind == keybindIndex).First();
            if (isEdit)
            {
                group = Editor.Project.Handler.MapData.MapObjectSelections.Resources.Where(x => x.SelectionGroupKeybind == keybindIndex && x.Name != name).First();
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

            Editor.Project.Handler.MapData.MapObjectSelections.Resources.Add(res);
            Editor.Project.Handler.MapData.SaveMapObjectSelections();
        }

        return false;
    }
    public void CreateSelectionGroup(string type)
    {
        if (CFG.Current.MapEditor_Selection_Group_Enable_Quick_Creation)
        {
            if (Editor.ViewportSelection.GetSelection().Count != 0)
            {
                var ent = (Entity)Editor.ViewportSelection.GetSelection().First();
                createPromptGroupName = ent.Name;
                createPromptTags = "";
                AmendSelectionGroupBank();
            }
        }
        else
        {
            ImGui.OpenPopup($"##selectionGroupModal{type}");
        }
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

            Editor.Project.Handler.MapData.SaveMapObjectSelections();
        }
    }

    private void SelectSelectionGroup()
    {
        Editor.ViewportSelection.ClearSelection();

        List<Entity> entities = new List<Entity>();

        // TODO: add something to prevent confusion if multiple maps are loaded with the same names within
        foreach (var entry in Editor.Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer != null)
            {
                foreach (var mapObj in entry.Value.MapContainer.Objects)
                {
                    if (selectedResourceContents.Contains(mapObj.Name))
                    {
                        //TaskLogs.AddLog(mapObj.Name);
                        entities.Add(mapObj);
                    }
                }
            }
        }

        foreach (var entry in entities)
        {
            Editor.ViewportSelection.AddSelection(entry);
        }

        if (CFG.Current.MapEditor_Selection_Group_Frame_Selection_On_Use)
        {
            Editor.FrameAction.ApplyViewportFrame();
            Editor.GotoAction.GotoMapObjectEntry();
        }
    }

    private void AmendSelectionGroupBank(bool isEdit = false)
    {
        List<string> tagList = AliasHelper.GetTagList(createPromptTags);
        List<string> selectionList = new List<string>();

        if (isEdit)
        {
            selectionList = selectedResourceContents;
        }
        else
        {
            foreach (var entry in Editor.ViewportSelection.GetSelection())
            {
                var ent = (Entity)entry;
                selectionList.Add(ent.Name);
            }
        }

        if (AddSelectionGroup(createPromptGroupName, tagList, selectionList, currentKeyBindOption, isEdit, editPromptOldGroupName))
        {
            Editor.Project.Handler.MapData.SaveMapObjectSelections();
        }
    }

    public void ShortcutSelectGroup(int index)
    {
        if (Editor.Project.Handler.MapData.MapObjectSelections.Resources == null)
            return;

        foreach (var entry in Editor.Project.Handler.MapData.MapObjectSelections.Resources)
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
