using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.BanksMain;
using StudioCore.Editors;
using StudioCore.Editors.AssetBrowser;
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

    private AssetCategoryType AssetType;

    public AliasTab(AliasBank bank, string name, ref bool tagBool, bool isNumericId = false, bool isMapId = false, AssetCategoryType assetType = AssetCategoryType.None) 
    {
        Bank = bank;
        EntryName = name;
        TagBool = tagBool;
        IsNumericID = isNumericId;
        IsMapID = isMapId;
        AssetType = assetType;
    }

    public void Display()
    {
        if (Project.Type == ProjectType.Undefined)
            return;

        if (Bank.IsLoadingAliases)
            return;

        DisplayNameGroupList();

        if (Bank.CanReloadBank)
        {
            Bank.CanReloadBank = false;
            Bank.ReloadAliasBank();

            // Invalidate these so the name updates there
            EditorContainer.MsbEditor.MapAssetBrowser.InvalidateNameCaches();
            EditorContainer.ModelEditor.ModelAssetBrowser.InvalidateNameCaches();
        }
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

        DisplaySelectionList(Bank.AliasNames.GetEntries(EntryName));

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

        var entries = Bank.AliasNames.GetEntries(EntryName);

        foreach (var entry in entries)
        {
            var displayedName = $"{entry.id} - {entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            // Append tags to to displayed name
            if (TagBool)
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
        var width = ImGui.GetWindowWidth();
        var inputSize = new Vector2(width, 20);
        var buttonSize = new Vector2(width, 24);

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
                if (AssetType == AssetCategoryType.None)
                {
                    Bank.AddToLocalAliasBank("", _refUpdateId, _refUpdateName, _refUpdateTags);
                }
                else
                {
                    switch (AssetType)
                    {
                        case AssetCategoryType.Character:
                            Bank.AddToLocalAliasBank("Chr", _refUpdateId, _refUpdateName, _refUpdateTags);
                            break;
                        case AssetCategoryType.Asset:
                            Bank.AddToLocalAliasBank("Obj", _refUpdateId, _refUpdateName, _refUpdateTags);
                            break;
                        case AssetCategoryType.Part:
                            Bank.AddToLocalAliasBank("Part", _refUpdateId, _refUpdateName, _refUpdateTags);
                            break;
                        case AssetCategoryType.MapPiece:
                            Bank.AddToLocalAliasBank("MapPiece", _refUpdateId, _refUpdateName, _refUpdateTags);
                            break;
                    }
                }

                ImGui.CloseCurrentPopup();
                Bank.CanReloadBank = true;
            }

            if (ImGui.Button("Restore Default", buttonSize))
            {
                if (AssetType == AssetCategoryType.None)
                {
                    Bank.RemoveFromLocalAliasBank("", _refUpdateId);
                }
                else
                {
                    switch (AssetType)
                    {
                        case AssetCategoryType.Character:
                            Bank.RemoveFromLocalAliasBank("Chr", _refUpdateId);
                            break;
                        case AssetCategoryType.Asset:
                            Bank.RemoveFromLocalAliasBank("Obj", _refUpdateId);
                            break;
                        case AssetCategoryType.Part:
                            Bank.RemoveFromLocalAliasBank("Part", _refUpdateId);
                            break;
                        case AssetCategoryType.MapPiece:
                            Bank.RemoveFromLocalAliasBank("MapPiece", _refUpdateId);
                            break;
                    }
                }

                ImGui.CloseCurrentPopup();
                Bank.CanReloadBank = true;
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
        var width = ImGui.GetWindowWidth();
        var inputSize = new Vector2(width, 20);
        var buttonSize = new Vector2(width, 24);

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

            var entries = Bank.AliasNames.GetEntries(EntryName);

            foreach (var entry in entries)
            {
                if (_newRefId == entry.id)
                    isValid = false;
            }

            if (isValid)
            {
                if (AssetType == AssetCategoryType.None)
                {
                    Bank.AddToLocalAliasBank("", _newRefId, _newRefName, _newRefTags);
                }
                else
                { 
                    switch(AssetType)
                    {
                        case AssetCategoryType.Character:
                            Bank.AddToLocalAliasBank("Chr", _newRefId, _newRefName, _newRefTags);
                            break;
                        case AssetCategoryType.Asset:
                            Bank.AddToLocalAliasBank("Obj", _newRefId, _newRefName, _newRefTags);
                            break;
                        case AssetCategoryType.Part:
                            Bank.AddToLocalAliasBank("Part", _newRefId, _newRefName, _newRefTags);
                            break;
                        case AssetCategoryType.MapPiece:
                            Bank.AddToLocalAliasBank("MapPiece", _newRefId, _newRefName, _newRefTags);
                            break;
                    }
                }


                ImGui.CloseCurrentPopup();
                Bank.CanReloadBank = true;
            }
            else
            {
                PlatformUtils.Instance.MessageBox($"{_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
