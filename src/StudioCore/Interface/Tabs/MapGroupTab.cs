using ImGuiNET;
using StudioCore.Banks.MapGroupBank;
using StudioCore.Core;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Interface.Tabs;

public class MapGroupTab
{
    private string _searchInput = "";
    private string _searchInputCache = "";

    private string _newRefId = "";
    private string _newRefName = "";
    private string _newRefDescription = "";
    private string _newRefCategory = "";
    private string _newRefMembers = "";

    private MapGroupReference _selectedEntry;

    private string _refUpdateId = "";
    private string _refUpdateName = "";
    private string _refUpdateDesc = "";
    private string _refUpdateCategory = "";
    private string _refUpdateMembers = "";

    private bool ShowMapGroupAddSection = false;

    public MapGroupTab() { }

    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if(Smithbox.BankHandler.MapGroups.GetEntries() == null)
        {
            return;
        }

        if (ShowMapGroupAddSection)
        {
            if (ImGui.Button("Show Map Group List"))
            {
                ShowMapGroupAddSection = false;
            }
        }
        else
        {
            if (ImGui.Button("Add New Map Group"))
            {
                ShowMapGroupAddSection = true;
            }
        }

        ImGui.SameLine();
        ImGui.Checkbox("Toggle Display in Editor", ref CFG.Current.Interface_DisplayMapGroups);
        ImguiUtils.ShowHoverTooltip("Toggle the display of the Map Group list boxes in the Map Editor.");


        ImGui.Separator();

        if (ShowMapGroupAddSection)
        {
            DisplayMapGroupAddSection();
        }
        else
        {
            DisplayMapGroupList();
        }
    }

    public void DisplayMapGroupAddSection()
    {
        var scale = Smithbox.GetUIScale();

        ImGui.Text("ID");
        ImGui.InputText($"##ID", ref _newRefId, 255);
        ImguiUtils.ShowHoverTooltip("The numeric ID of the map group to add.");

        ImGui.Text("Name");
        ImGui.InputText($"##Name", ref _newRefName, 255);
        ImguiUtils.ShowHoverTooltip("The name of the map group to add.");

        ImGui.Text("Description");
        ImGui.InputTextMultiline($"##Description", ref _newRefDescription, 255, new Vector2(255, 200 * scale));
        ImguiUtils.ShowHoverTooltip("The description of this map group.");

        ImGui.Text("Category");
        ImGui.InputText($"##Category", ref _newRefCategory, 255);
        ImguiUtils.ShowHoverTooltip("The category this map group should be placed under");

        ImGui.Text("Members");
        ImGui.InputText($"##Members", ref _newRefMembers, 255);
        ImguiUtils.ShowHoverTooltip("The map ids to associate with this map group.\nEach map id should be separated by the ',' character.");

        if (ImGui.Button("Add Map Group"))
        {
            try
            {
                var isNumeric = int.TryParse(_newRefId, out _);

                // Make sure the ref ID is a number
                if (isNumeric)
                {
                    var isValid = true;

                    var entries = Smithbox.BankHandler.MapGroups.GetEntries();

                    foreach (var entry in entries)
                    {
                        if (_newRefId == entry.id)
                        {
                            isValid = false;
                        }
                    }

                    if (isValid)
                    {
                        var mapGroupMembers = new List<MapGroupMember>();

                        // Convert string to MapGroupMember
                        if (_newRefMembers != "")
                        {
                            var members = _newRefMembers.Split(",");
                            if (members.Length > 0)
                            {
                                foreach (var member in members)
                                {
                                    var newMember = new MapGroupMember();
                                    newMember.id = member;
                                    mapGroupMembers.Add(newMember);
                                }
                            }
                            else
                            {
                                var newMember = new MapGroupMember();
                                newMember.id = _newRefMembers;
                                mapGroupMembers.Add(newMember);
                            }
                        }

                        Smithbox.BankHandler.MapGroups.AddToLocalBank(_newRefId, _newRefName, _newRefDescription, _newRefCategory, mapGroupMembers);
                        ImGui.CloseCurrentPopup();

                    }
                    else
                    {
                        PlatformUtils.Instance.MessageBox($"Map Group with {_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    PlatformUtils.Instance.MessageBox($"Map Group ID must be numeric.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch { }
        }
    }

    public void DisplayMapGroupList()
    {
        ImGui.InputText($"Search", ref _searchInput, 255);
        ImguiUtils.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Columns(2);

        ImGui.BeginChild("MapGroupList");

        if(Smithbox.BankHandler.MapGroups.GetEntries() != null )
        {
            DisplaySelectionList(Smithbox.BankHandler.MapGroups.GetEntries());
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild("EditWindow");

        DisplayEditWindow();

        ImGui.EndChild();

        ImGui.Columns(1);
    }

    private void DisplaySelectionList(List<MapGroupReference> referenceList)
    {
        if (_searchInput != _searchInputCache)
        {
            _searchInputCache = _searchInput;
        }

        foreach (var entry in referenceList)
        {
            var displayedName = $"{entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refDesc = $"{entry.description}";
            var refCategory = $"{entry.category}";
            var refMembers = entry.members;

            var refMembersString = "";

            for (int i = 0; i < refMembers.Count; i++)
            {
                if (i == refMembers.Count - 1)
                {
                    refMembersString = refMembersString + $"{refMembers[i].id}";
                }
                else
                {
                    refMembersString = refMembersString + $"{refMembers[i].id},";
                }
            }


            if (SearchFilters.IsSearchMatch(_searchInput, refID, refName, null))
            {
                if (ImGui.Selectable($"{displayedName}##{refID}", entry == _selectedEntry))
                {
                    _selectedEntry = entry;

                    _refUpdateId = refID;
                    _refUpdateName = refName;
                    _refUpdateDesc = refDesc;
                    _refUpdateCategory = refCategory;
                    _refUpdateMembers = refMembersString;
                }
            }
        }
    }

    private void DisplayEditWindow()
    {
        var scale = Smithbox.GetUIScale();

        if (_selectedEntry != null)
        {
            ImGui.Text($"Alias for {_selectedEntry.id}");

            ImGui.Text("Name");
            ImGui.InputText($"##Name", ref _refUpdateName, 255);

            ImGui.Text("Description");
            ImGui.InputTextMultiline($"##Description", ref _refUpdateDesc, 255, new Vector2(255, 200 * scale));

            ImGui.Text("Category");
            ImGui.InputText($"##Category", ref _refUpdateCategory, 255);

            ImGui.Text("Members");
            ImGui.InputText($"##Members", ref _refUpdateMembers, 255);

            if (ImGui.Button("Update"))
            {
                var mapGroupMembers = new List<MapGroupMember>();

                // Convert string to MapGroupMember
                if (_refUpdateMembers != "")
                {
                    var members = _refUpdateMembers.Split(",");
                    if (members.Length > 0)
                    {
                        foreach (var member in members)
                        {
                            var newMember = new MapGroupMember();
                            newMember.id = member;
                            mapGroupMembers.Add(newMember);
                        }
                    }
                    else
                    {
                        var newMember = new MapGroupMember();
                        newMember.id = _newRefMembers;
                        mapGroupMembers.Add(newMember);
                    }
                }

                Smithbox.BankHandler.MapGroups.AddToLocalBank(_refUpdateId, _refUpdateName, _refUpdateDesc, _refUpdateCategory, mapGroupMembers);
            }
            ImguiUtils.ShowHoverTooltip("Edit this map group.");

            ImGui.SameLine();

            // Is from base, delete local instance to restore base read
            if (!Smithbox.BankHandler.MapGroups.IsLocalMapGroup(_refUpdateId) && ImGui.Button("Restore Default"))
            {
                Smithbox.BankHandler.MapGroups.RemoveFromLocalBank(_refUpdateId);
            }
            ImguiUtils.ShowHoverTooltip("Restore this map group to its default values.");

            // Is local only, this will delete it
            if (Smithbox.BankHandler.MapGroups.IsLocalMapGroup(_refUpdateId) && ImGui.Button("Delete"))
            {
                Smithbox.BankHandler.MapGroups.RemoveFromLocalBank(_refUpdateId);
            }
            ImguiUtils.ShowHoverTooltip("Delete this map group.");
        }
        else
        {
            ImGui.Text("Select an entry to edit its properties.");
        }
    }
}


