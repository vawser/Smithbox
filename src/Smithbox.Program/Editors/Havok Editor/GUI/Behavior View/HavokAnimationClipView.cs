using Havok.Shared;
using Hexa.NET.DirectXTex;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

public class HavokAnimationClipView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    public List<IClipGenerator> SelectedAnimationClips = new();
    private List<IClipGenerator> AnimationClips = new();

    private bool RebuildAliasCache = false;
    private Dictionary<string, string> _aliasCache = new();

    public HavokAnimationClipView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        SelectedAnimationClips.Clear();
        _aliasCache.Clear();
        RebuildAliasCache = true;
    }

    public void Setup(object sourceObject)
    {
        // The marker interface is implemented by the class in every SDK's class set
        AnimationClips = HavokTreeSearch.FindAll<IClipGenerator>(
                sourceObject, View.PropertyCache.GetCachedHavokFields)
                .ToList();

        // Drop selections that are no longer in the graph (after a structural edit)
        SelectedAnimationClips.RemoveAll(e => !AnimationClips.Contains(e));

        RequestAliasRebuild();
    }

    /// <summary>
    /// Rebuild the alias column on the next draw (after the clip list or a community name changed)
    /// </summary>
    public void RequestAliasRebuild()
    {
        _aliasCache.Clear();
        RebuildAliasCache = true;
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    /// <summary>
    /// Draws this view in the Contents window (its category is selected)
    /// </summary>
    public void DisplayContents()
    {
        DisplayTabHeader();
        DisplayTabContents();
    }

    public void DisplayTabContents()
    {
        var tblFlags  = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV;

        UpdateFilteredClips();

        if (ImGui.BeginTable($"havokBehaviorElementListSection", 2, tblFlags))
        {
            // Only the visible rows are drawn
            var clipper = new ImGuiListClipper();
            clipper.Begin(FilteredClips.Count);

            while (clipper.Step())
            {
                for (int row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
                {
                    var i = FilteredClips[row];
                    var entry = AnimationClips[i];
                    var entryName = GetClipName(entry);

                    var selected = SelectedAnimationClips.Contains(entry);

                    var alias = _aliasCache.GetValueOrDefault(entryName) ?? "";

                    // ID
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    if (ImGui.Selectable($"{entryName}##clipGenerator_{entryName}{i}", selected))
                    {
                        if (InputManager.HasCtrlDown())
                        {
                            SelectedAnimationClips.Add(entry);
                        }
                        else
                        {
                            SelectedAnimationClips.Clear();
                            SelectedAnimationClips.Add(entry);
                        }
                    }

                    DisplayRowMenu(entry, $"clipMenu_{i}");

                    // Alias
                    ImGui.TableSetColumnIndex(1);

                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
                    if (ImGui.Selectable($"{alias}##clipGenerator_Alias_{entryName}{i}", selected))
                    {
                        if (InputManager.HasCtrlDown())
                        {
                            SelectedAnimationClips.Add(entry);
                        }
                        else
                        {
                            SelectedAnimationClips.Clear();
                            SelectedAnimationClips.Add(entry);
                        }
                    }
                    ImGui.PopStyleColor(1);

                    DisplayRowMenu(entry, $"clipAliasMenu_{i}");
                }
            }

            clipper.End();

            ImGui.EndTable();
        }
    }

    /// <summary>
    /// The right-click menu of a clip row: duplicate, delete, copy, paste, and its variable bindings
    /// </summary>
    private void DisplayRowMenu(IClipGenerator entry, string id)
    {
        if (ImGui.BeginPopupContextItem(id))
        {
            Owner.ActionsView.DisplayGeneratorItems(entry, HavokBehaviorActionsView.Source.List, SelectedAnimationClips.Cast<object>().ToList());
            ImGui.Separator();
            Owner.ActionsView.DisplayBindingItems(entry, HavokBehaviorActionsView.Source.List);
            ImGui.EndPopup();
        }
    }

    // Indices into AnimationClips of the rows that pass the filter, rebuilt only when the filter,
    // the clip list or the aliases change (behavior files can hold thousands of clips)
    private List<int> FilteredClips = new();
    private string FilteredFilter;
    private bool FilteredExact;
    private bool RefilterClips = true;

    private string GetClipName(IClipGenerator entry)
    {
        return HavokTypeUtils.GetName(entry);
    }

    private void UpdateFilteredClips()
    {
        if (RebuildAliasCache)
        {
            _aliasCache.Clear();

            foreach (var entry in AnimationClips)
            {
                var id = GetClipName(entry);

                if (!_aliasCache.ContainsKey(id))
                    _aliasCache[id] = Project.Handler.HavokData.GetHavokObjectName(View.Selection.FilePath, id);
            }

            RebuildAliasCache = false;
            RefilterClips = true;
        }

        if (!RefilterClips && FilteredFilter == Owner.PropFilter && FilteredExact == Owner.ExactPropFilter)
            return;

        RefilterClips = false;
        FilteredFilter = Owner.PropFilter;
        FilteredExact = Owner.ExactPropFilter;

        FilteredClips.Clear();

        for (int i = 0; i < AnimationClips.Count; i++)
        {
            var entryName = GetClipName(AnimationClips[i]);
            var alias = _aliasCache.GetValueOrDefault(entryName) ?? "";

            if (EditorFilters.IsMatch(Owner.PropFilter, entryName, Owner.ExactPropFilter, alias))
                FilteredClips.Add(i);
        }
    }

    public void DisplayTabHeader()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

        ImGui.EndChild();
    }

    public bool CanDisplayProperties()
    {
        var firstSelection = SelectedAnimationClips.FirstOrDefault();

        if (!IsCurrentTab)
            return false;

        if (firstSelection == null)
            return false;

        return true;
    }

    public void DisplayProperties()
    {
        // Only edit the first selection (multi-select is only for the entry manipulation actions)
        var firstSelection = SelectedAnimationClips.FirstOrDefault();

        if (firstSelection == null)
            return;

        var havokMeta = HavokMetaHelper.GetMeta(Project, firstSelection.GetType());

        ImGui.BeginChild("havokBehaviorPropEditSection", Owner.PropertySectionSize);

        var columnCount = 2;
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        View.PropertyView.HavokPropEditGeneric(firstSelection, havokMeta);

        ImGui.Columns(1);

        ImGui.EndChild();
    }
}
