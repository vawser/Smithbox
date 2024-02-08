using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
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

public class EventFlagWindow
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

    public EventFlagWindow()
    {
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
        CFG.Current.EventFlagBrowser_Open = !CFG.Current.EventFlagBrowser_Open;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        if (Project.Type == ProjectType.Undefined)
            return;

        if (FlagAliasBank.Bank.IsLoadingAliases)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 0.98f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Event Flags##EventFlagBrowser", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            if (ImGui.Button("Help"))
                ImGui.OpenPopup("##EventFlagBrowserHelp");

            ImGui.SameLine();
            ImGui.Checkbox("Show tags", ref CFG.Current.EventFlagBrowser_ShowTagsInBrowser);
            ImguiUtils.ShowHelpMarker("Show the tags for each entry within the browser list as part of their displayed name.");

            if (ImGui.BeginPopup("##EventFlagBrowserHelp"))
            {
                ImGui.Text("Double click to copy the event flag to your clipboard.");
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            if (ImGui.Button("Toggle Alias Addition"))
                CFG.Current.EventFlagBrowser_ShowAliasAddition = !CFG.Current.EventFlagBrowser_ShowAliasAddition;

            ImGui.SameLine();
            ImguiUtils.ShowHelpMarker("When enabled the Browser List will display the tags next to the name.");
            ImGui.Checkbox("Show Tags", ref CFG.Current.EventFlagBrowser_ShowTagsInBrowser);

            if (CFG.Current.EventFlagBrowser_ShowAliasAddition)
            {
                ImGui.Separator();

                ImGui.InputText($"ID", ref _newRefId, 255);
                ImguiUtils.ShowHelpMarker("The numeric ID of the alias to add.");

                ImGui.InputText($"Name", ref _newRefName, 255);
                ImguiUtils.ShowHelpMarker("The name of the alias to add.");

                ImGui.InputText($"Tags", ref _newRefTags, 255);
                ImguiUtils.ShowHelpMarker("The tags of the alias to add.\nEach tag should be separated by the ',' character.");

                if (ImGui.Button("Add New Alias"))
                    // Make sure the ref ID is a number
                    if (Regex.IsMatch(_newRefId, @"^\d+$"))
                    {
                        var isValid = true;

                        var entries = FlagAliasBank.Bank.AliasNames.GetEntries("Flags");

                        foreach (var entry in entries)
                            if (_newRefId == entry.id)
                                isValid = false;

                        if (isValid)
                        {
                            FlagAliasBank.Bank.AddToLocalAliasBank("", _newRefId, _newRefName, _newRefTags);
                            ImGui.CloseCurrentPopup();
                            FlagAliasBank.Bank.mayReloadAliasBank = true;
                        }
                        else
                            PlatformUtils.Instance.MessageBox($"Event Flag Alias with {_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                ImguiUtils.ShowHelpMarker("Adds a new alias to the project-specific alias bank.");

                ImGui.Separator();
            }

            ImGui.Columns(1);

            ImGui.BeginChild("FlagListSearch");
            ImGui.InputText($"Search", ref _searchInput, 255);

            ImGui.SameLine();
            ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild("EventFlagList");

            DisplaySelectionList(FlagAliasBank.Bank.AliasNames.GetEntries("Flags"));

            ImGui.EndChild();
            ImGui.EndChild();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);

        if (FlagAliasBank.Bank.mayReloadAliasBank)
        {
            FlagAliasBank.Bank.mayReloadAliasBank = false;
            FlagAliasBank.Bank.ReloadAliasBank();
        }
    }

    /// <summary>
    /// Display the event flag selection list
    /// </summary>
    private void DisplaySelectionList(List<AliasReference> referenceList)
    {
        var referenceDict = new Dictionary<string, AliasReference>();

        foreach (AliasReference v in referenceList)
            if (!referenceDict.ContainsKey(v.id))
                referenceDict.Add(v.id, v);

        if (_searchInput != _searchInputCache)
            _searchInputCache = _searchInput;

        var entries = FlagAliasBank.Bank.AliasNames.GetEntries("Flags");

        foreach (var entry in entries)
        {
            var displayedName = $"{entry.id} - {entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            // Append tags to to displayed name
            if (CFG.Current.EventFlagBrowser_ShowTagsInBrowser)
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
                            FlagAliasBank.Bank.AddToLocalAliasBank("", _refUpdateId, _refUpdateName, _refUpdateTags);
                            ImGui.CloseCurrentPopup();
                            FlagAliasBank.Bank.mayReloadAliasBank = true;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Restore Default"))
                        {
                            FlagAliasBank.Bank.RemoveFromLocalAliasBank("", _refUpdateId);
                            ImGui.CloseCurrentPopup();
                            FlagAliasBank.Bank.mayReloadAliasBank = true;
                        }

                        ImGui.EndPopup();
                    }

                if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                {
                    var num = long.Parse(refID.Replace("f", ""));

                    PlatformUtils.Instance.SetClipboardText($"{num}");
                }
            }
        }
    }
}
