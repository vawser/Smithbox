using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Help;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Interface.Tabs;

public class MapNameTab
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

    private bool ShowMapNameAddSection = false;

    public MapNameTab() { }

    public void Display()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        if (MapAliasBank.Bank.IsLoadingAliases)
            return;

        if (ShowMapNameAddSection)
        {
            if (ImGui.Button("Show Alias List"))
            {
                ShowMapNameAddSection = false;
            }
        }
        else
        {
            if (ImGui.Button("Add New Alias"))
            {
                ShowMapNameAddSection = true;
            }
        }

        ImGui.Separator();

        if (ShowMapNameAddSection)
        {
            DisplayMapNameAddSection();
        }
        else
        {
            DisplayMapNameGroupList();
        }


        if (MapAliasBank.Bank.CanReloadBank)
        {
            MapAliasBank.Bank.CanReloadBank = false;
            MapAliasBank.Bank.ReloadAliasBank();
        }
    }

    public void DisplayMapNameAddSection()
    {
        ImGui.Text("ID");
        ImGui.InputText($"##ID", ref _newRefId, 255);
        ImguiUtils.ShowHoverTooltip("The map ID of the map name to add.");

        ImGui.Text("Name");
        ImGui.InputText($"##Name", ref _newRefName, 255);
        ImguiUtils.ShowHoverTooltip("The alias name to give to the added map name.");

        ImGui.Text("Tags");
        ImGui.InputText($"##Tags", ref _newRefTags, 255);
        ImguiUtils.ShowHoverTooltip("The tags to associate with this map name.");

        if (ImGui.Button("Add New Alias"))
        {
            // Make sure the ref ID is a number
            if (Regex.IsMatch(_newRefId, @"m\d{2}_\d{2}_\d{2}_\d{2}"))
            {
                var isValid = true;

                var entries = MapAliasBank.Bank.AliasNames.GetEntries("Maps");

                foreach (var entry in entries)
                {
                    if (_newRefId == entry.id)
                        isValid = false;
                }

                if (isValid)
                {
                    MapAliasBank.Bank.AddToLocalAliasBank("", _newRefId, _newRefName, _newRefTags);
                    ImGui.CloseCurrentPopup();
                    MapAliasBank.Bank.CanReloadBank = true;
                }
                else
                {
                    PlatformUtils.Instance.MessageBox($"{_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                PlatformUtils.Instance.MessageBox($"ID must match map name format - mXX_XX_XX_XX.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public void DisplayMapNameGroupList()
    {
        ImGui.InputText($"Search", ref _searchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");
        ImGui.SameLine();
        ImGui.Checkbox("Show unused map names", ref CFG.Current.MapNameAtlas_ShowUnused);
        ImguiUtils.ShowHoverTooltip("Enabling this option will allow unused or debug map names to appear in the scene tree view.");


        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Columns(2);

        ImGui.BeginChild("MapNameList");

        DisplaySelectionList(MapAliasBank.Bank.AliasNames.GetEntries("Maps"));

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild("EditWindow");

        DisplayEditWindow();

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

        var entries = MapAliasBank.Bank.AliasNames.GetEntries("Maps");

        foreach (var entry in entries)
        {
            var displayedName = $"{entry.id} - {entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            // Skip the unused names if this is disabled
            if (!CFG.Current.MapNameAtlas_ShowUnused)
            {
                if (refTagList[0] == "unused")
                {
                    continue;
                }
            }

            // Append tags to to displayed name
            if (CFG.Current.MapNameAtlas_ShowTags)
            {
                var tagString = string.Join(" ", refTagList);
                displayedName = $"{displayedName} {{ {tagString} }}";
            }

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
                            tagStr = $"{tagStr},{tEntry}";
                        _refUpdateTags = tagStr;
                    }
                    else
                    {
                        _refUpdateTags = "";
                    }
                }
            }
        }
    }

    private void DisplayEditWindow()
    {
        if (_selectedEntry != null)
        {
            var refID = $"{_selectedEntry.id}";

            ImGui.Text($"Alias for {refID}");

            ImGui.Text($"Name");
            ImGui.InputText($"##Name", ref _refUpdateName, 255);

            ImGui.Text($"Tags");
            ImGui.InputText($"##Tags", ref _refUpdateTags, 255);

            if (ImGui.Button("Update"))
            {
                MapAliasBank.Bank.AddToLocalAliasBank("", _refUpdateId, _refUpdateName, _refUpdateTags);
                ImGui.CloseCurrentPopup();
                MapAliasBank.Bank.CanReloadBank = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Restore Default"))
            {
                MapAliasBank.Bank.RemoveFromLocalAliasBank("", _refUpdateId);
                ImGui.CloseCurrentPopup();
                MapAliasBank.Bank.CanReloadBank = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy ID to Clipboard"))
            {
                PlatformUtils.Instance.SetClipboardText($"{refID}");
            }
        }
        else
        {
            ImGui.Text("Select an entry to edit its properties.");
        }
    }
}
