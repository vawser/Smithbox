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
using static SoulsFormats.MSB_AC6.Part;

namespace StudioCore.Interface.Tabs;

public class GparamNameTab
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

    private bool ShowNameAddSection = false;

    public GparamNameTab() { }

    public void Display()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        if (GparamAliasBank.Bank.IsLoadingAliases)
            return;

        if (ShowNameAddSection)
        {
            if (ImGui.Button("Show Alias List"))
            {
                ShowNameAddSection = false;
            }
        }
        else
        {
            if (ImGui.Button("Add New Alias"))
            {
                ShowNameAddSection = true;
            }
        }

        ImGui.SameLine();
        ImGui.Checkbox("Toggle Display in Editor", ref CFG.Current.Interface_Display_Alias_for_Gparam);
        ImguiUtils.ShowHoverTooltip("Toggle the display of the File aliases in the Gparam Editor.");

        ImGui.Separator();

        if (ShowNameAddSection)
        {
            DisplayNameAddSection();
        }
        else
        {
            DisplayNameGroupList();
        }


        if (GparamAliasBank.Bank.CanReloadBank)
        {
            GparamAliasBank.Bank.CanReloadBank = false;
            GparamAliasBank.Bank.ReloadAliasBank();
        }
    }

    public void DisplayNameAddSection()
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
            bool isValid = true;

            var entries = GparamAliasBank.Bank.AliasNames.GetEntries("Gparams");

            foreach (var entry in entries)
            {
                if (_newRefId == entry.id)
                    isValid = false;
            }

            if (isValid)
            {
                GparamAliasBank.Bank.AddToLocalAliasBank("", _newRefId, _newRefName, _newRefTags);
                ImGui.CloseCurrentPopup();
                GparamAliasBank.Bank.CanReloadBank = true;
            }
            else
            {
                PlatformUtils.Instance.MessageBox($"{_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public void DisplayNameGroupList()
    {
        ImGui.InputText($"Search", ref _searchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Columns(2);

        ImGui.BeginChild("ParticleList");

        DisplaySelectionList(GparamAliasBank.Bank.AliasNames.GetEntries("Gparams"));

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

        var entries = GparamAliasBank.Bank.AliasNames.GetEntries("Gparams");

        foreach (var entry in entries)
        {
            var displayedName = $"{entry.id} - {entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            // Append tags to to displayed name
            if (CFG.Current.GparamNameAtlas_ShowTags)
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
                        {
                            tagStr = $"{tagStr},{tEntry}";
                        }
                        _refUpdateTags = tagStr;
                    }
                    else
                        _refUpdateTags = "";
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
                GparamAliasBank.Bank.AddToLocalAliasBank("", _refUpdateId, _refUpdateName, _refUpdateTags);
                ImGui.CloseCurrentPopup();
                GparamAliasBank.Bank.CanReloadBank = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Restore Default"))
            {
                GparamAliasBank.Bank.RemoveFromLocalAliasBank("", _refUpdateId);
                ImGui.CloseCurrentPopup();
                GparamAliasBank.Bank.CanReloadBank = true;
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
