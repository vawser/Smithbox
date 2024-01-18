using ImGuiNET;
using StudioCore.Aliases;
using StudioCore.Help;
using StudioCore.JSON;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace StudioCore.Browsers;

public class FxrBrowser
{
    private string _id;
    private AssetLocator _locator;

    private bool MenuOpenState;

    private string _searchInput = "";
    private string _searchInputCache = "";

    private string _refUpdateId = "";
    private string _refUpdateName = "";
    private string _refUpdateTags = "";

    private string _selectedName;

    private bool reloadFxrAlias = false;

    public FxrBrowser(string id, AssetLocator locator)
    {
        _id = id;
        _locator = locator;
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
        CFG.Current.FxrBrowser_Open = !CFG.Current.FxrBrowser_Open;
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

        if (ImGui.Begin("Particle ID##FxrBrowser", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            if (ImGui.Button("Help"))
                ImGui.OpenPopup("##FxrBrowserHelp");

            if (ImGui.BeginPopup("##FxrBrowserHelp"))
            {
                ImGui.Text("Double click to copy the particle ID to your clipboard.");
                ImGui.EndPopup();
            }

            ImGui.SameLine();
            ImGui.Checkbox("Show Tags", ref CFG.Current.FxrBrowser_ShowTagsInBrowser);

            ImGui.Columns(1);

            ImGui.BeginChild("ParticleListSearch");
            ImGui.InputText($"Search", ref _searchInput, 255);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild("ParticleFlagList");

            DisplaySelectionList(FxrAliasBank.AliasNames.GetEntries());

            ImGui.EndChild();
            ImGui.EndChild();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);

        if (reloadFxrAlias)
        {
            reloadFxrAlias = false;
            FxrAliasBank.ReloadAliasBank();
        }
    }

    /// <summary>
    /// Display the fxr selection list
    /// </summary>
    private void DisplaySelectionList(List<FxrAliasReference> referenceList)
    {
        Dictionary<string, FxrAliasReference> referenceDict = new Dictionary<string, FxrAliasReference>();

        foreach (FxrAliasReference v in referenceList)
        {
            if (!referenceDict.ContainsKey(v.id))
                referenceDict.Add(v.id, v);
        }

        if (_searchInput != _searchInputCache)
        {
            _searchInputCache = _searchInput;
        }

        var entries = FxrAliasBank._loadedAliasBank.GetEntries();

        foreach (var entry in entries)
        {
            var displayedName = $"{entry.id} - {entry.name}";

            var refID = $"{entry.id}";
            var refName = $"{entry.name}";
            var refTagList = entry.tags;

            // Append tags to to displayed name
            if (CFG.Current.FxrBrowser_ShowTagsInBrowser)
            {
                var tagString = string.Join(" ", refTagList);
                displayedName = $"{displayedName} {{ {tagString} }}";
            }

            if (Utils.IsReferenceSearchFilterMatch(_searchInput, refID, refName, refTagList))
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
                            FxrAliasBank.AddToLocalAliasBank(_refUpdateId, _refUpdateName, _refUpdateTags);
                            ImGui.CloseCurrentPopup();
                            reloadFxrAlias = true;
                        }
                        if (ImGui.Button("Restore Default"))
                        {
                            FxrAliasBank.RemoveFromLocalAliasBank(_refUpdateId);
                            ImGui.CloseCurrentPopup();
                            reloadFxrAlias = true;
                        }

                        ImGui.EndPopup();
                    }
                }

                if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                {
                    PlatformUtils.Instance.SetClipboardText(refID);
                }
            }
        }
    }
}
