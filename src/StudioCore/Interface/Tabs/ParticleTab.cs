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

public class ParticleTab
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

    private bool ShowEventFlagAddSection = false;

    public ParticleTab() { }

    public void Display()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        if (ParticleAliasBank.Bank.IsLoadingAliases)
            return;


        if (ShowEventFlagAddSection)
        {
            if (ImGui.Button("Show Alias List"))
            {
                ShowEventFlagAddSection = false;
            }
        }
        else
        {
            if (ImGui.Button("Add New Alias"))
            {
                ShowEventFlagAddSection = true;
            }
        }

        ImGui.Separator();

        if (ShowEventFlagAddSection)
        {
            DisplayEventFlagAddSection();
        }
        else
        {
            DisplayEventFlagGroupList();
        }

        if (ParticleAliasBank.Bank.CanReloadBank)
        {
            ParticleAliasBank.Bank.CanReloadBank = false;
            ParticleAliasBank.Bank.ReloadAliasBank();
        }
    }

    public void DisplayEventFlagAddSection()
    {
        ImGui.Text("ID");
        ImGui.InputText($"##ID", ref _newRefId, 255);
        ImguiUtils.ShowHoverTooltip("The numeric ID of the event flag to add.");

        ImGui.Text("Name");
        ImGui.InputText($"##Name", ref _newRefName, 255);
        ImguiUtils.ShowHoverTooltip("The alias name to give to the added event flag.");

        ImGui.Text("Tags");
        ImGui.InputText($"##Tags", ref _newRefTags, 255);
        ImguiUtils.ShowHoverTooltip("The tags to associate with this event flag.");

        if (ImGui.Button("Add New Alias"))
        {
            // Make sure the ref ID is a number
            if (Regex.IsMatch(_newRefId, @"^\d+$"))
            {
                var isValid = true;

                var entries = ParticleAliasBank.Bank.AliasNames.GetEntries("Particles");

                foreach (var entry in entries)
                {
                    if (_newRefId == entry.id)
                        isValid = false;
                }

                if (isValid)
                {
                    ParticleAliasBank.Bank.AddToLocalAliasBank("", _newRefId, _newRefName, _newRefTags);
                    ImGui.CloseCurrentPopup();
                    ParticleAliasBank.Bank.CanReloadBank = true;
                }
                else
                {
                    PlatformUtils.Instance.MessageBox($"{_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                PlatformUtils.Instance.MessageBox($"{_newRefId} ID must be numeric.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public void DisplayEventFlagGroupList()
    {
        ImGui.InputText($"Search", ref _searchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");
        ImGui.SameLine();
        ImGui.Checkbox("Show Tags", ref CFG.Current.ParticleAtlas_ShowTags);
        ImguiUtils.ShowHoverTooltip("When enabled the list will display the tags next to the name.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Columns(2);

        ImGui.BeginChild("ParticleList");

        DisplaySelectionList(ParticleAliasBank.Bank.AliasNames.GetEntries("Particles"));

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
        var referenceDict = new Dictionary<string, AliasReference>();

        foreach (AliasReference v in referenceList)
        {
            if (!referenceDict.ContainsKey(v.id))
                referenceDict.Add(v.id, v);
        }

        if (_searchInput != _searchInputCache)
            _searchInputCache = _searchInput;

        var entries = ParticleAliasBank.Bank.AliasNames.GetEntries("Particles");

        foreach (var entry in entries)
        {
            var displayedName = $"{entry.id} - {entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            // Append tags to to displayed name
            if (CFG.Current.ParticleAtlas_ShowTags)
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
        if(_selectedEntry != null)
        {
            var refID = $"{_selectedEntry.id}";

            ImGui.Text($"Alias for {refID}");

            ImGui.Text($"Name");
            ImGui.InputText($"##Name", ref _refUpdateName, 255);

            ImGui.Text($"Tags");
            ImGui.InputText($"##Tags", ref _refUpdateTags, 255);

            if (ImGui.Button("Update"))
            {
                ParticleAliasBank.Bank.AddToLocalAliasBank("", _refUpdateId, _refUpdateName, _refUpdateTags);
                ImGui.CloseCurrentPopup();
                ParticleAliasBank.Bank.CanReloadBank = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Restore Default"))
            {
                ParticleAliasBank.Bank.RemoveFromLocalAliasBank("", _refUpdateId);
                ImGui.CloseCurrentPopup();
                ParticleAliasBank.Bank.CanReloadBank = true;
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy ID to Clipboard"))
            {
                var num = long.Parse(refID.Replace("f", ""));

                PlatformUtils.Instance.SetClipboardText($"{num}");
            }
        }
        else
        {
            ImGui.Text("Select an entry to edit its properties.");
        }
    }
}
