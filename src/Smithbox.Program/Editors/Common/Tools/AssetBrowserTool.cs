using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.Common;

/// <summary>
/// Browses the assets/objects of the project, as 3D previews or as a list.
/// What double-clicking an entry does is up to the editor that owns this.
/// </summary>
public class AssetBrowserTool
{
    private ProjectEntry Project;

    private ModelPreviewBrowser Browser;
    private Action<ModelPreviewEntry> OnActivate;
    private string Hint;

    private string Filter = "";
    private bool ExactFilter = false;

    private List<ModelPreviewEntry> Entries;
    private List<ModelPreviewEntry> FilteredEntries = new();

    private string AppliedFilter = null;
    private bool AppliedExactFilter = false;

    public AssetBrowserTool(ProjectEntry project, string hint, Action<ModelPreviewEntry> onActivate)
    {
        Project = project;
        Hint = hint;
        OnActivate = onActivate;

        Browser = new ModelPreviewBrowser(project);
    }

    public void Display()
    {
        ImGui.BeginChild("AssetBrowserToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(Hint);
        GUI.Spacer();

        if (ImGui.RadioButton($"{LOC.Get("EDITOR_ModelPreview_List_Mode")}##assetBrowserListMode", !CFG.Current.Interface_Asset_Browser_Use_Previews))
        {
            CFG.Current.Interface_Asset_Browser_Use_Previews = false;
        }
        GUI.Tooltip(LOC.Get("EDITOR_ModelPreview_List_Mode_TT"));

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("EDITOR_ModelPreview_Preview_Mode")}##assetBrowserPreviewMode", CFG.Current.Interface_Asset_Browser_Use_Previews))
        {
            CFG.Current.Interface_Asset_Browser_Use_Previews = true;
        }
        GUI.Tooltip(LOC.Get("EDITOR_ModelPreview_Preview_Mode_TT"));

        GUI.Spacer();

        if (CFG.Current.Interface_Asset_Browser_Use_Previews)
        {
            Browser.DisplayOptions();
        }

        EditorFilters.DisplayFramedListFilter("assetBrowser", ref Filter, ref ExactFilter);

        UpdateFilteredEntries();

        if (CFG.Current.Interface_Asset_Browser_Use_Previews)
        {
            Browser.Display("assetBrowser", FilteredEntries, OnActivate);
        }
        else
        {
            DisplayList();
        }

        ImGui.EndChild();
    }

    private void DisplayList()
    {
        ImGui.BeginChild("##assetBrowserList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(FilteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entry = FilteredEntries[i];

                if (ImGui.Selectable($"{entry.ID}##assetBrowserEntry{i}", entry.ID == Browser.SelectedID, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Browser.SelectedID = entry.ID;

                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        OnActivate(entry);
                    }
                }

                GUI.DisplayAlias(entry.Alias);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private void UpdateFilteredEntries()
    {
        if (Entries == null)
        {
            Entries = GetEntries();
        }
        else if (Filter == AppliedFilter && ExactFilter == AppliedExactFilter)
        {
            return;
        }

        AppliedFilter = Filter;
        AppliedExactFilter = ExactFilter;

        FilteredEntries = Entries
            .Where(e => EditorFilters.IsMatch(Filter, e.ID, ExactFilter, e.Alias))
            .ToList();
    }

    private List<ModelPreviewEntry> GetEntries()
    {
        Dictionary<string, string> aliases = new(StringComparer.OrdinalIgnoreCase);

        if (Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Assets, out List<AliasEntry> assetAliases))
        {
            foreach (var alias in assetAliases)
            {
                aliases.TryAdd(alias.ID, alias.Name);
            }
        }

        return Project.Locator.AssetFiles.Entries
            .Select(e => new ModelPreviewEntry(e.Filename, aliases.GetValueOrDefault(e.Filename, ""), ModelPreviewKind.Asset))
            .ToList();
    }
}
