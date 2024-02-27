using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Editors.MapEditor.MapGroup;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Interface.Windows;

public class MapGroupWindow
{
    private bool MenuOpenState;

    private string _searchInput = "";
    private string _searchInputCache = "";

    private string _newRefId = "";
    private string _newRefName = "";
    private string _newRefDescription = "";
    private string _newRefCategory = "";
    private string _newRefMembers = "";

    private string _selectedId;

    private string _refUpdateId = "";
    private string _refUpdateName = "";
    private string _refUpdateDesc = "";
    private string _refUpdateCategory = "";
    private string _refUpdateMembers = "";

    public MapGroupWindow() { }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        if (Project.Type == ProjectType.Undefined)
            return;

        if (MapGroupsBank.Bank.IsMapGroupBankLoaded)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Map Groups##MapGroupWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.InputText($"ID", ref _newRefId, 255);
            ImguiUtils.ShowHelpMarker("The numeric ID of the map group to add.");

            ImGui.InputText($"Name", ref _newRefName, 255);
            ImguiUtils.ShowHelpMarker("The name of the map group to add.");

            ImGui.InputText($"Description", ref _newRefDescription, 255);
            ImguiUtils.ShowHelpMarker("The description of this map group.");

            ImGui.InputText($"Category", ref _newRefCategory, 255);
            ImguiUtils.ShowHelpMarker("The category this map group should be placed under");

            ImGui.InputText($"Members", ref _newRefMembers, 255);
            ImguiUtils.ShowHelpMarker("The map ids to associate with this map group.\nEach map id should be separated by the ',' character.");

            if (ImGui.Button("Add Map Group"))
            {
                try
                {
                    var isNumeric = int.TryParse(_newRefId, out _);

                    // Make sure the ref ID is a number
                    if (isNumeric)
                    {
                        var isValid = true;

                        var entries = MapGroupsBank.Bank.Entries.list;

                        foreach (var entry in entries)
                        {
                            if (_newRefId == entry.id)
                                isValid = false;
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

                            MapGroupsBank.Bank.AddToLocalBank(_newRefId, _newRefName, _newRefDescription, _newRefCategory, mapGroupMembers);
                            ImGui.CloseCurrentPopup();
                            MapGroupsBank.Bank.CanReloadMapGroupBank = true;
                        }
                        else
                            PlatformUtils.Instance.MessageBox($"Map Group with {_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch
                {
                    TaskLogs.AddLog($"Map Group ID must be a numeric value!");
                }
            }
            ImguiUtils.ShowHelpMarker("Adds a new map group to the project-specific bank.");

            ImGui.Separator();

            ImGui.BeginChild("MapGroupListSearch");
            ImGui.InputText($"Search", ref _searchInput, 255);

            ImGui.SameLine();
            ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild("MapGroupList");

            DisplaySelectionList(MapGroupsBank.Bank.Entries.list);

            ImGui.EndChild();
            ImGui.EndChild();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);

        if (MapGroupsBank.Bank.CanReloadMapGroupBank)
        {
            MapGroupsBank.Bank.CanReloadMapGroupBank = false;
            MapGroupsBank.Bank.ReloadMapGroupBank();
        }
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

            foreach(var member in refMembers) 
            {
                refMembersString = refMembersString + $"{member.id},";
            }

            if (SearchFilters.IsSearchMatch(_searchInput, refID, refName, null))
            {
                if (ImGui.Selectable(displayedName))
                {
                    _selectedId = refID;
                    _refUpdateId = refID;
                    _refUpdateName = refName;
                    _refUpdateDesc = refDesc;
                    _refUpdateCategory = refCategory;
                    _refUpdateMembers = refMembersString;
                }

                if (_selectedId == refID)
                {
                    if (ImGui.BeginPopupContextItem($"{refID}##context"))
                    {
                        ImGui.InputText($"Name", ref _refUpdateName, 255);
                        ImGui.InputText($"Description", ref _refUpdateDesc, 255);
                        ImGui.InputText($"Category", ref _refUpdateCategory, 255);
                        ImGui.InputText($"Members", ref _refUpdateMembers, 255);

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

                            MapGroupsBank.Bank.AddToLocalBank(_refUpdateId, _refUpdateName, _refUpdateDesc, _refUpdateCategory, mapGroupMembers);
                            ImGui.CloseCurrentPopup();
                            MapGroupsBank.Bank.CanReloadMapGroupBank = true;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Restore Default"))
                        {
                            MapGroupsBank.Bank.RemoveFromLocalBank(_refUpdateId);
                            ImGui.CloseCurrentPopup();
                            MapGroupsBank.Bank.CanReloadMapGroupBank = true;
                        }

                        ImGui.EndPopup();
                    }
                }

                if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                {
                }
            }
        }
    }
}


