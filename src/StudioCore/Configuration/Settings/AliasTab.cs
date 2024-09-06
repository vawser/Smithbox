using HKLib.hk2018.hk;
using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.Core.Project;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Configuration.Settings;

public class AliasTab
{
    private string _searchInput = "";
    private string _searchInputCache = "";

    public string _refUpdateId = "";
    public string _refUpdateName = "";
    public string _refUpdateTags = "";

    public string _newRefId = "";
    public string _newRefName = "";
    public string _newRefTags = "";

    public AliasReference _selectedEntry;

    private AliasBank Bank;
    private string EntryName;
    private bool TagBool;
    private bool IsNumericID;
    private bool IsMapID;

    public bool FocusSelection = false;

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

        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBgSecondary);
        ImGui.BeginChild($"AliasSelectionList_{EntryName}");

        if (Bank.Aliases != null)
        {
            DisplaySelectionList(Bank.Aliases.list);
        }

        ImGui.EndChild();
        ImGui.PopStyleColor(1);

        ImGui.NextColumn();

        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBgSecondary);
        ImGui.BeginChild($"EditAliasWindow_{EntryName}");

        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Alias Actions");
        ImGui.Separator();

        DisplayActionsWindow();

        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Edit Selected Alias");
        ImGui.Separator();

        DisplayEditWindow();

        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Add New Alias");
        ImGui.Separator();

        DisplayNameAddSection();

        ImGui.EndChild();
        ImGui.PopStyleColor(1);

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
            if (entry.id == "dummy" || entry.id == "default")
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

                if (FocusSelection && entry == _selectedEntry)
                {
                    FocusSelection = false;
                    ImGui.SetScrollHereY();
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

    private void DisplayActionsWindow()
    {
        var scale = Smithbox.GetUIScale();
        var width = ImGui.GetWindowWidth();
        var inputSize = new Vector2(width, 20 * scale);
        var buttonSize = new Vector2(width, 24 * scale);

        if (ImGui.Button("Copy Selected Alias Name", buttonSize))
        {
            PlatformUtils.Instance.SetClipboardText(_selectedEntry.name);
        }
        ImguiUtils.ShowHoverTooltip("Copy the currently selected alias name directly to your clipboard.");

        if (ImGui.Button("Copy Selected Alias ID", buttonSize))
        {
            PlatformUtils.Instance.SetClipboardText(_selectedEntry.id);
        }
        ImguiUtils.ShowHoverTooltip("Copy the currently selected alias id directly to your clipboard.");

        if (ImGui.Button("Copy Alias List", buttonSize))
        {
            var aliasList = "";
            foreach (var entry in Bank.GetEntries())
            {
                if (entry.Key == "dummy" || entry.Key == "default")
                    continue;

                var line = $"{entry.Key} {entry.Value.name}\n";

                if (SearchFilters.IsSearchMatch(_searchInput, entry.Value.id, entry.Value.name, entry.Value.tags))
                {
                    aliasList = aliasList + line;
                }
            }

            PlatformUtils.Instance.SetClipboardText(aliasList);
        }
        ImguiUtils.ShowHoverTooltip("Copy the aliases into a list: <ID> <Name>, saving to your clipboard.");
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
            ImGui.SetNextItemWidth(width);
            ImGui.InputText($"##EditName_{EntryName}", ref _refUpdateName, 255);

            ImGui.Text($"Tags");
            ImGui.SetNextItemWidth(width);
            ImGui.InputText($"##EditTags_{EntryName}", ref _refUpdateTags, 255);

            if (ImGui.Button("Update", buttonSize))
            {
                Bank.AddToLocalAliasBank(_refUpdateId, _refUpdateName, _refUpdateTags);

                Smithbox.AliasCacheHandler.ReloadAliasCaches = true;

                ImGui.CloseCurrentPopup();
            }

            if (ImGui.Button("Restore Default", buttonSize))
            {
                Bank.RemoveFromLocalAliasBank(_refUpdateId);

                Smithbox.AliasCacheHandler.ReloadAliasCaches = true;

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
        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##AddID_{EntryName}", ref _newRefId, 255);
        ImguiUtils.ShowHoverTooltip("The map ID of the map name to add.");

        ImGui.Text("Name");
        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##AddName_{EntryName}", ref _newRefName, 255);
        ImguiUtils.ShowHoverTooltip("The alias name to give to the added map name.");

        ImGui.Text("Tags");
        ImGui.SetNextItemWidth(width);
        ImGui.InputText($"##AddTags_{EntryName}", ref _newRefTags, 255);
        ImguiUtils.ShowHoverTooltip("The tags to associate with this map name.");

        if (ImGui.Button("Add New Alias", buttonSize))
        {
            bool isValid = true;

            if (IsNumericID)
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

                Smithbox.AliasCacheHandler.ReloadAliasCaches = true;

                ImGui.CloseCurrentPopup();
            }
            else
            {
                PlatformUtils.Instance.MessageBox($"{_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
