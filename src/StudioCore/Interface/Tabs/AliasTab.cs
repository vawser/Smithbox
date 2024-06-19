using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editors.AssetBrowser;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Interface.Tabs;

public class AliasTab
{
    private string _searchInput = "";
    private string _searchInputCache = "";

    private string _refUpdateId = "";
    private string _refUpdateName = "";
    private string _refUpdateTags = "";

    private string _newRefId = "";
    private string _newRefName = "";
    private string _newRefTags = "";

    private AliasReference _selectedEntry;

    private AliasBank Bank;
    private string EntryName;
    private bool TagBool;
    private bool IsNumericID;
    private bool IsMapID;

    public AliasTab(AliasBank bank, string name, ref bool tagBool, bool isNumericId = false, bool isMapId = false) 
    {
        Bank = bank;
        EntryName = name;
        TagBool = tagBool;
        IsNumericID = isNumericId;
        IsMapID = isMapId;
    }

    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        DisplayNameGroupList();
    }

    public void DisplayNameGroupList()
    {
        ImGui.InputText($"Search", ref _searchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");
        ImGui.SameLine();
        ImGui.Checkbox("Show tags in alias list", ref TagBool);

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Columns(2);

        ImGui.BeginChild($"AliasSelectionList_{EntryName}");

        if(Bank.Aliases != null)
        {
            DisplaySelectionList(Bank.Aliases.list);
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild($"EditAliasWindow_{EntryName}");

        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Edit Selected Alias");
        ImGui.Separator();

        DisplayEditWindow();

        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Add New Alias");
        ImGui.Separator();

        DisplayNameAddSection();

        ImGui.EndChild();

        ImGui.Columns(1);
    }

    /// <summary>
    /// Display the event flag selection list
    /// </summary>
    private void DisplaySelectionList(List<AliasReference> referenceList)
    {
        if (_searchInput != _searchInputCache)
        {
            _searchInputCache = _searchInput;
        }

        foreach (var entry in Bank.Aliases.list)
        {
            // Skip these entries
            if(entry.id == "dummy" || entry.id == "default")
            {
                continue;
            }

            var displayedName = $"{entry.id}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            if (SearchFilters.IsSearchMatch(_searchInput, refID, refName, refTagList))
            {
                if (ImGui.Selectable(displayedName, entry == _selectedEntry))
                {
                    _selectedEntry = entry;

                    _refUpdateId = refID;
                    _refUpdateName = refName;

                    if (refTagList.Count > 0)
                    {
                        var tagStr = refTagList[0];
                        foreach (var tEntry in refTagList.Skip(1))
                        {
                            tagStr = $"{tagStr},{tEntry}";
                        }
                        _refUpdateTags = tagStr;
                    }
                    else
                        _refUpdateTags = "";
                }

                // Display name alias
                if (entry.name != "")
                {
                    AliasUtils.DisplayAlias(entry.name);
                }

                // Display tags
                if (TagBool)
                {
                    var tagString = string.Join(" ", refTagList);
                    AliasUtils.DisplayTagAlias(tagString);
                }
            }
        }
    }

    private void DisplayEditWindow()
    {
        var scale = Smithbox.GetUIScale();
        var width = ImGui.GetWindowWidth();
        var inputSize = new Vector2(width, 20 * scale);
        var buttonSize = new Vector2(width, 24 * scale);

        if (_selectedEntry != null)
        {
            var refID = $"{_selectedEntry.id}";

            ImGui.Text($"Alias for {refID}");

            ImGui.Text($"Name");
            ImGui.InputTextMultiline($"##EditName_{EntryName}", ref _refUpdateName, 255, inputSize);

            ImGui.Text($"Tags");
            ImGui.InputTextMultiline($"##EditTags_{EntryName}", ref _refUpdateTags, 255, inputSize);

            if (ImGui.Button("Update", buttonSize))
            {
                Bank.AddToLocalAliasBank(_refUpdateId, _refUpdateName, _refUpdateTags);

                Smithbox.NameCacheHandler.ReloadNameCaches = true;

                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Button("Restore Default", buttonSize))
            {
                Bank.RemoveFromLocalAliasBank(_refUpdateId);

                Smithbox.NameCacheHandler.ReloadNameCaches = true;

                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Button("Copy ID to Clipboard", buttonSize))
            {
                PlatformUtils.Instance.SetClipboardText($"{refID}");
            }
        }
        else
        {
            ImGui.Text("Select an entry to edit its properties.");
        }
    }
    public void DisplayNameAddSection()
    {
        var scale = Smithbox.GetUIScale();
        var width = ImGui.GetWindowWidth();
        var inputSize = new Vector2(width, 20 * scale);
        var buttonSize = new Vector2(width, 24 * scale);

        ImGui.Text("ID");
        ImGui.InputTextMultiline($"##AddID_{EntryName}", ref _newRefId, 255, inputSize);
        ImguiUtils.ShowHoverTooltip("The map ID of the map name to add.");

        ImGui.Text("Name");
        ImGui.InputTextMultiline($"##AddName_{EntryName}", ref _newRefName, 255, inputSize);
        ImguiUtils.ShowHoverTooltip("The alias name to give to the added map name.");

        ImGui.Text("Tags");
        ImGui.InputTextMultiline($"##AddTags_{EntryName}", ref _newRefTags, 255, inputSize);
        ImguiUtils.ShowHoverTooltip("The tags to associate with this map name.");

        if (ImGui.Button("Add New Alias", buttonSize))
        {
            bool isValid = true;

            if(IsNumericID)
            {
                if (!Regex.IsMatch(_newRefId, @"^\d+$"))
                {
                    isValid = false;
                }
            }

            if (IsMapID)
            {
                if (!Regex.IsMatch(_newRefId, @"m\d{2}_\d{2}_\d{2}_\d{2}"))
                {
                    isValid = false;
                }
            }

            foreach (var entry in Bank.Aliases.list)
            {
                if (_newRefId == entry.id)
                    isValid = false;
            }

            if (isValid)
            {
                Bank.AddToLocalAliasBank(_newRefId, _newRefName, _newRefTags);

                Smithbox.NameCacheHandler.ReloadNameCaches = true;

                ImGui.CloseCurrentPopup();
            }
            else
            {
                PlatformUtils.Instance.MessageBox($"{_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
