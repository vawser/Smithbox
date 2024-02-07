using ImGuiNET;
using StudioCore.Data.Aliases;
using StudioCore.Help;
using StudioCore.Interface;
using StudioCore.JSON;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Browsers;

public class ParticleBrowser
{
    private string _id;

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

    public AliasBank _aliasBank;

    public ParticleBrowser(string id, AliasBank aliasBank)
    {
        _id = id;
        _aliasBank = aliasBank;
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
        CFG.Current.ParticleBrowser_Open = !CFG.Current.ParticleBrowser_Open;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();

        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(600.0f, 600.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 0.98f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Particle ID Browser##FxrBrowser", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImguiUtils.ShowHelpButton("Help", "Double click to copy the particle ID to your clipboard.", "particle");

            ImGui.SameLine();
            if (ImGui.Button("Toggle Alias Addition"))
            {
                CFG.Current.ParticleBrowser_ShowAliasAddition = !CFG.Current.ParticleBrowser_ShowAliasAddition;
            }

            ImGui.SameLine();

            ImGui.Checkbox("Show Tags", ref CFG.Current.ParticleBrowser_ShowTagsInBrowser);
            ImguiUtils.ShowHelpMarker("When enabled the Browser List will display the tags next to the name.");

            if (CFG.Current.ParticleBrowser_ShowAliasAddition)
            {
                ImGui.Separator();

                ImGui.InputText($"ID", ref _newRefId, 255);
                ImguiUtils.ShowHelpMarker("The numeric ID of the alias to add.");

                ImGui.InputText($"Name", ref _newRefName, 255);
                ImguiUtils.ShowHelpMarker("The name of the alias to add.");

                ImGui.InputText($"Tags", ref _newRefTags, 255);
                ImguiUtils.ShowHelpMarker("The tags of the alias to add.\nEach tag should be separated by the ',' character.");

                if (ImGui.Button("Add New Alias"))
                {
                    // Make sure the ref ID is a number
                    if (Regex.IsMatch(_newRefId, @"^\d+$"))
                    {
                        bool isValid = true;

                        var entries = _aliasBank.AliasNames.GetEntries("Particles");

                        foreach (var entry in entries)
                        {
                            if (_newRefId == entry.id)
                                isValid = false;
                        }

                        if (isValid)
                        {
                            _aliasBank.AddToLocalAliasBank("", _newRefId, _newRefName, _newRefTags);
                            ImGui.CloseCurrentPopup();
                            _aliasBank.mayReloadAliasBank = true;
                        }
                        else
                        {
                            PlatformUtils.Instance.MessageBox($"FXR Alias with {_newRefId} ID already exists.", $"Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                ImguiUtils.ShowHelpMarker("Adds a new alias to the project-specific alias bank.");

                ImGui.Separator();
            }

            ImGui.Columns(1);

            ImGui.BeginChild("ParticleListSearch");
            ImGui.InputText($"Search", ref _searchInput, 255);

            ImGui.SameLine();
            ImguiUtils.ShowHelpMarker("Separate terms are split via the + character.");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild("ParticleFlagList");

            DisplaySelectionList(_aliasBank.AliasNames.GetEntries("Particles"));

            ImGui.EndChild();
            ImGui.EndChild();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);

        if (_aliasBank.mayReloadAliasBank)
        {
            _aliasBank.mayReloadAliasBank = false;
            _aliasBank.ReloadAliasBank();
        }
    }

    /// <summary>
    /// Display the fxr selection list
    /// </summary>
    private void DisplaySelectionList(List<AliasReference> referenceList)
    {
        Dictionary<string, AliasReference> referenceDict = new Dictionary<string, AliasReference>();

        foreach (AliasReference v in referenceList)
        {
            if (!referenceDict.ContainsKey(v.id))
                referenceDict.Add(v.id, v);
        }

        if (_searchInput != _searchInputCache)
        {
            _searchInputCache = _searchInput;
        }

        var entries = _aliasBank.AliasNames.GetEntries("Particles");

        foreach (var entry in entries)
        {
            var displayedName = $"{entry.id} - {entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            // Append tags to to displayed name
            if (CFG.Current.ParticleBrowser_ShowTagsInBrowser)
            {
                var tagString = string.Join(" ", refTagList);
                displayedName = $"{displayedName} {{ {tagString} }}";
            }

            if (SearchFilters.IsSearchMatch(_searchInput, refID, refName, refTagList, false, true))
            {
                if (ImGui.Selectable(displayedName))
                {
                    _selectedName = refID;
                    _refUpdateId = refID;
                    _refUpdateName = refName;

                    if (refTagList.Count > 0)
                    {
                        string tagStr = refTagList[0];
                        foreach (string tEntry in refTagList.Skip(1))
                        {
                            tagStr = $"{tagStr},{tEntry}";
                        }
                        _refUpdateTags = tagStr;
                    }
                    else
                    {
                        _refUpdateTags = "";
                    }
                }

                if (_selectedName == refID)
                {
                    if (ImGui.BeginPopupContextItem($"{refID}##context"))
                    {
                        ImGui.InputText($"Name", ref _refUpdateName, 255);
                        ImGui.InputText($"Tags", ref _refUpdateTags, 255);

                        if (ImGui.Button("Update"))
                        {
                            _aliasBank.AddToLocalAliasBank("", _refUpdateId, _refUpdateName, _refUpdateTags);
                            ImGui.CloseCurrentPopup();
                            _aliasBank.mayReloadAliasBank = true;
                        }
                        ImGui.SameLine();
                        if (ImGui.Button("Restore Default"))
                        {
                            _aliasBank.RemoveFromLocalAliasBank("", _refUpdateId);
                            ImGui.CloseCurrentPopup();
                            _aliasBank.mayReloadAliasBank = true;
                        }

                        ImGui.EndPopup();
                    }
                }

                if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                {
                    string numId = refID.Replace("f", "");
                    int num = int.Parse(numId);

                    PlatformUtils.Instance.SetClipboardText(num.ToString());
                }
            }
        }
    }
}
