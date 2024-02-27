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

namespace StudioCore.Interface.Windows;

public class MapNameWindow
{
    private bool MenuOpenState;

    private string _searchInput = "";
    private string _searchInputCache = "";

    private string _refUpdateId = "";
    private string _refUpdateName = "";
    private string _refUpdateTags = "";

    private string _newRefId = "";
    private string _newRefName = "";
    private string _newRefTags = "";

    private string _selectedName;

    public MapNameWindow()
    {
    }

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

        if (MapAliasBank.Bank.IsLoadingAliases)
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


        if (ImGui.Begin("Map Names##MapNameWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            if (ImGui.Button("Help"))
                ImGui.OpenPopup("##MapNameWindowHelp");

            ImGui.SameLine();
            ImGui.Checkbox("Show unused map names", ref CFG.Current.MapAliases_ShowUnusedNames);
            ImguiUtils.ShowHelpMarker("Enabling this option will allow unused or debug map names to appear in the scene tree view.");

            if (ImGui.BeginPopup("##MapNameWindowHelp"))
            {
                ImGui.Text("Double click to copy the map name to your clipboard.");
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Toggle Name Addition"))
                CFG.Current.MapAliases_ShowAliasAddition = !CFG.Current.MapAliases_ShowAliasAddition;

            if (CFG.Current.MapAliases_ShowAliasAddition)
            {
                ImGui.Separator();

                ImGui.InputText($"ID", ref _newRefId, 255);
                ImguiUtils.ShowHelpMarker("The numeric ID of the alias to add.");

                ImGui.InputText($"Name", ref _newRefName, 255);
                ImguiUtils.ShowHelpMarker("The name of the alias to add.");

                ImGui.InputText($"Tags", ref _newRefTags, 255);
                ImguiUtils.ShowHelpMarker("The tags of the alias to add.\nEach tag should be separated by the ',' character.");

                if (ImGui.Button("Add Name"))
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
                            MapAliasBank.Bank.mayReloadAliasBank = true;
                        }
                        else
                            PlatformUtils.Instance.MessageBox($"Map Name with {_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                ImguiUtils.ShowHelpMarker("Adds a new name to the project-specific alias bank.");

                ImGui.Separator();
            }

            ImGui.Columns(1);

            ImGui.BeginChild("MapNameListSearch");
            ImGui.InputText($"Search", ref _searchInput, 255);

            ImGui.SameLine();
            ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild("MapNameList");

            DisplaySelectionList(MapAliasBank.Bank.AliasNames.GetEntries("Maps"));

            ImGui.EndChild();
            ImGui.EndChild();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);

        if (MapAliasBank.Bank.mayReloadAliasBank)
        {
            MapAliasBank.Bank.mayReloadAliasBank = false;
            MapAliasBank.Bank.ReloadAliasBank();
        }
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
            if (!CFG.Current.MapAliases_ShowUnusedNames)
            {
                if (refTagList[0] == "unused")
                {
                    continue;
                }
            }

            // Append tags to to displayed name
            if (CFG.Current.MapAliases_ShowTagsInBrowser)
            {
                var tagString = string.Join(" ", refTagList);
                displayedName = $"{displayedName} {{ {tagString} }}";
            }

            if (SearchFilters.IsSearchMatch(_searchInput, refID, refName, refTagList))
            {
                if (ImGui.Selectable(displayedName))
                {
                    _selectedName = refID;
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
                        _refUpdateTags = "";
                }

                if (_selectedName == refID)
                    if (ImGui.BeginPopupContextItem($"{refID}##context"))
                    {
                        ImGui.InputText($"Name", ref _refUpdateName, 255);
                        ImGui.InputText($"Tags", ref _refUpdateTags, 255);

                        if (ImGui.Button("Update"))
                        {
                            MapAliasBank.Bank.AddToLocalAliasBank("", _refUpdateId, _refUpdateName, _refUpdateTags);
                            ImGui.CloseCurrentPopup();
                            MapAliasBank.Bank.mayReloadAliasBank = true;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Restore Default"))
                        {
                            MapAliasBank.Bank.RemoveFromLocalAliasBank("", _refUpdateId);
                            ImGui.CloseCurrentPopup();
                            MapAliasBank.Bank.mayReloadAliasBank = true;
                        }

                        ImGui.EndPopup();
                    }

                if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                {
                }
            }
        }
    }
}
