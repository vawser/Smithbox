﻿using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor.Tools.SelectionGroups;

public class SelectionGroupView
{
    private MapEditorScreen Screen;
    private IViewport _viewport;
    private ViewportSelection _selection;

    private MsbEntity selectedEntity;

    public SelectionGroupView(MapEditorScreen screen)
    {
        Screen = screen;

        _selection = screen.Selection;
        _viewport = screen.MapViewportView.Viewport;
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

    public void OnProjectChanged()
    {
        Smithbox.BankHandler.EntitySelectionGroups.CreateSelectionGroups();
    }

    public void SelectionGroupShortcuts()
    {
        // Selection Groups
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_CreateSelectionGroup))
        {
            CreateSelectionGroup("External");
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup_0))
        {
            ShortcutSelectGroup(0);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup_1))
        {
            ShortcutSelectGroup(1);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup_2))
        {
            ShortcutSelectGroup(2);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup_3))
        {
            ShortcutSelectGroup(3);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup4))
        {
            ShortcutSelectGroup(4);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup5))
        {
            ShortcutSelectGroup(5);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup6))
        {
            ShortcutSelectGroup(6);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup7))
        {
            ShortcutSelectGroup(7);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup8))
        {
            ShortcutSelectGroup(8);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup9))
        {
            ShortcutSelectGroup(9);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_SelectionGroup10))
        {
            ShortcutSelectGroup(10);
        }
    }

    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.BankHandler.EntitySelectionGroups.Groups == null || Smithbox.BankHandler.EntitySelectionGroups.Groups.Resources == null)
            return;

        // This exposes the pop-up to the map editor
        if (ImGui.BeginPopup("##selectionGroupModalExternal"))
        {
            DisplayCreationModal();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Display the window.
    /// </summary>
    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.BankHandler.EntitySelectionGroups.Groups == null || Smithbox.BankHandler.EntitySelectionGroups.Groups.Resources == null)
            return;

        var width = ImGui.GetWindowWidth();
        var buttonWidth = width / 100 * 95;

        if (ImGui.Button("Create Selection Group from Current Selection", new Vector2(buttonWidth, 32)))
        {
            CreateSelectionGroup("Internal");
        }
        UIHelper.ShowHoverTooltip($"Shortcut: {KeyBindings.Current.MAP_CreateSelectionGroup.HintText}\nBring up the selection group creation menu to assign your current selection to a selection group.");

        if (ImGui.BeginPopup("##selectionGroupModalInternal"))
        {
            DisplayCreationModal();

            ImGui.EndPopup();
        }

        ImGui.Columns(2);

        ImGui.BeginChild("##selectionGroupList");

        ImGui.InputText($"Search", ref _searchInput, 255);
        UIHelper.ShowHoverTooltip("Separate terms are split via the + character.");

        foreach (var entry in Smithbox.BankHandler.EntitySelectionGroups.Groups.Resources)
        {
            var displayName = $"{entry.Name}";

            if (CFG.Current.MapEditor_SelectionGroup_ShowKeybind)
            {
                if (entry.SelectionGroupKeybind != -1)
                {
                    var keyBind = GetSelectionGroupKeyBind(entry.SelectionGroupKeybind);
                    if (keyBind != null)
                    {
                        displayName = $"{displayName} [{keyBind.HintText}]";
                    }
                }
            }

            if (CFG.Current.MapEditor_SelectionGroup_ShowTags)
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
                    editPromptTags = AliasUtils.GetTagListString(entry.Tags);
                    editPromptKeybind = entry.SelectionGroupKeybind;

                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        SelectSelectionGroup();
                    }
                }
            }
        }
        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild("##selectionGroupActions");

        width = ImGui.GetWindowWidth();
        buttonWidth = width / 100 * 95;

        if (selectedResourceName != "")
        {
            if (ImGui.Button("Select Contents", new Vector2(buttonWidth, 32)))
            {
                SelectSelectionGroup();
            }
            UIHelper.ShowHoverTooltip("Select the map objects listed by your currently selected group.");

            if (ImGui.Button("Edit Group", new Vector2(buttonWidth, 32)))
            {
                ImGui.OpenPopup($"##selectionGroupModalEdit");
            }
            UIHelper.ShowHoverTooltip("Edit the name, tags and keybind for the selected group.");

            if (ImGui.BeginPopup("##selectionGroupModalEdit"))
            {
                DisplayEditModal();

                ImGui.EndPopup();
            }

            if (ImGui.Button("Delete Group", new Vector2(buttonWidth, 32)))
            {
                DeleteSelectionGroup();
            }
            UIHelper.ShowHoverTooltip("Delete this selected group.");

            if (selectedResourceTags.Count > 0)
            {
                UIHelper.WrappedText("");
                UIHelper.WrappedText("Tags:");
                var tagString = string.Join(" ", selectedResourceTags);
                UIHelper.WrappedTextColored(UI.Current.ImGui_Default_Text_Color, tagString);
            }

            UIHelper.WrappedText("");
            UIHelper.WrappedText("Contents:");
            foreach (var entry in selectedResourceContents)
            {
                UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, entry);
            }
        }
        ImGui.EndChild();
    }

    private int currentKeyBindOption = -1;
    private List<int> keyBindOptions = new List<int>() { -1, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

    private void DisplayCreationModal()
    {
        var width = ImGui.GetWindowWidth();
        var buttonWidth = width / 100 * 95;

        ImGui.InputText("Group Name##selectionGroup_GroupName", ref createPromptGroupName, 255);
        UIHelper.ShowHoverTooltip("The name of the selection group.");
        ImGui.InputText("Tags##selectionGroup_Tags", ref createPromptTags, 255);
        UIHelper.ShowHoverTooltip("Separate each tag with the , character as a delimiter.");

        var keyBind = GetSelectionGroupKeyBind(currentKeyBindOption);
        var previewString = "None";
        if (keyBind != null)
        {
            previewString = keyBind.HintText;
        }

        if (ImGui.BeginCombo("Keybind##keybindCombo", previewString))
        {
            foreach (var entry in keyBindOptions)
            {
                keyBind = GetSelectionGroupKeyBind(entry);
                var nameString = "None";
                if (keyBind != null)
                {
                    nameString = keyBind.HintText;
                }

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
        UIHelper.ShowHoverTooltip("The keybind to quickly select the contents of this selection group.");

        if (ImGui.Button("Create Group", new Vector2(buttonWidth, 32)))
        {
            AmendSelectionGroupBank();
            ImGui.CloseCurrentPopup();
        }
    }

    private void DisplayEditModal()
    {
        var width = ImGui.GetWindowWidth();
        var buttonWidth = width / 100 * 95;

        ImGui.InputText("Group Name##selectionGroup_GroupName", ref editPromptGroupName, 255);
        UIHelper.ShowHoverTooltip("The name of the selection group.");
        ImGui.InputText("Tags##selectionGroup_Tags", ref editPromptTags, 255);
        UIHelper.ShowHoverTooltip("Separate each tag with the , character as a delimiter.");

        var keyBind = GetSelectionGroupKeyBind(editPromptKeybind);
        var previewString = "None";
        if (keyBind != null)
        {
            previewString = keyBind.HintText;
        }

        if (ImGui.BeginCombo("Keybind##keybindCombo", previewString))
        {
            foreach (var entry in keyBindOptions)
            {
                keyBind = GetSelectionGroupKeyBind(entry);
                var nameString = "None";
                if (keyBind != null)
                {
                    nameString = keyBind.HintText;
                }

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
        UIHelper.ShowHoverTooltip("The keybind to quickly select the contents of this selection group.");

        if (ImGui.Button("Edit Group", new Vector2(buttonWidth, 32)))
        {
            createPromptGroupName = editPromptGroupName;
            createPromptTags = editPromptTags;
            currentKeyBindOption = editPromptKeybind;

            AmendSelectionGroupBank(true);
            ImGui.CloseCurrentPopup();
        }
    }

    public void CreateSelectionGroup(string type)
    {
        if (CFG.Current.MapEditor_SelectionGroup_AutoCreation)
        {
            if (_selection.GetSelection().Count != 0)
            {
                var ent = (Entity)_selection.GetSelection().First();
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

        if (CFG.Current.MapEditor_SelectionGroup_ConfirmDelete)
        {
            result = PlatformUtils.Instance.MessageBox($"You are about to delete this selection group. Are you sure?", $"Smithbox", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
        }

        if (result == DialogResult.Yes)
        {
            Smithbox.BankHandler.EntitySelectionGroups.DeleteSelectionGroup(selectedResourceName);

            selectedResourceName = "";
            selectedResourceTags = new List<string>();
            selectedResourceContents = new List<string>();

            Smithbox.BankHandler.EntitySelectionGroups.LoadBank();
        }
    }

    private void SelectSelectionGroup()
    {
        _selection.ClearSelection();

        List<Entity> entities = new List<Entity>();

        // TODO: add something to prevent confusion if multiple maps are loaded with the same names within
        foreach (var entry in Screen.Universe.GetLoadedMapContainerList())
        {
            foreach (var mapObj in entry.Objects)
            {
                if (selectedResourceContents.Contains(mapObj.Name))
                {
                    //TaskLogs.AddLog(mapObj.Name);
                    entities.Add(mapObj);
                }
            }
        }

        foreach (var entry in entities)
        {
            _selection.AddSelection(entry);
        }

        if (CFG.Current.MapEditor_SelectionGroup_FrameSelection)
        {
            Screen.ActionHandler.ApplyFrameInViewport();
            Screen.ActionHandler.ApplyGoToInObjectList();
        }
    }

    private void AmendSelectionGroupBank(bool isEdit = false)
    {
        List<string> tagList = AliasUtils.GetTagList(createPromptTags);
        List<string> selectionList = new List<string>();

        if (isEdit)
        {
            selectionList = selectedResourceContents;
        }
        else
        {
            foreach (var entry in _selection.GetSelection())
            {
                var ent = (Entity)entry;
                selectionList.Add(ent.Name);
            }
        }

        if (Smithbox.BankHandler.EntitySelectionGroups.AddSelectionGroup(createPromptGroupName, tagList, selectionList, currentKeyBindOption, isEdit, editPromptOldGroupName))
        {
            Smithbox.BankHandler.EntitySelectionGroups.LoadBank();
        }
    }

    public void ShortcutSelectGroup(int index)
    {
        foreach (var entry in Smithbox.BankHandler.EntitySelectionGroups.Groups.Resources)
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

    private KeyBind GetSelectionGroupKeyBind(int index)
    {
        if (index == -1)
        {
            return null;
        }

        switch (index)
        {
            case 0: return KeyBindings.Current.MAP_SelectionGroup_0;
            case 1: return KeyBindings.Current.MAP_SelectionGroup_1;
            case 2: return KeyBindings.Current.MAP_SelectionGroup_2;
            case 3: return KeyBindings.Current.MAP_SelectionGroup_3;
            case 4: return KeyBindings.Current.MAP_SelectionGroup4;
            case 5: return KeyBindings.Current.MAP_SelectionGroup5;
            case 6: return KeyBindings.Current.MAP_SelectionGroup6;
            case 7: return KeyBindings.Current.MAP_SelectionGroup7;
            case 8: return KeyBindings.Current.MAP_SelectionGroup8;
            case 9: return KeyBindings.Current.MAP_SelectionGroup9;
            case 10: return KeyBindings.Current.MAP_SelectionGroup10;
            default: return null;
        }
    }
}
