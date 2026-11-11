using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class DisplayGroupTool
{
    private MapEditorView View;
    public ProjectEntry Project;

    private const int BitsPerWord = 32;
    private const int MaxListedTargets = 8;
    private const int MaxVisibilityRows = 5;
    private const int MaxListedGroups = 12;

    private int _dispGroupCount = 8;

    /// <summary>
    /// The value a left-button drag paints into the cells it passes over; null while no drag is in progress.
    /// </summary>
    private bool? _paintValue;

    private DisplayGroupPresetStore _presets;
    private DisplayGroupPresetStore Presets => _presets ??= new DisplayGroupPresetStore(Project);
    private string _presetName = "";

    // Group usage over the loaded maps, refreshed by RefreshStats
    private int[] _drawCounts = Array.Empty<int>();
    private int[] _displayCounts = Array.Empty<int>();
    private int _objectCount;
    private int _hiddenCount;
    private int _alwaysDrawnCount;
    private Dictionary<uint[], int> _drawArrayUsers = new(ReferenceEqualityComparer.Instance);
    private uint[] _statsDisplaySnapshot;
    private DateTime _statsTime = DateTime.MinValue;

    private static readonly TimeSpan StatsInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Which version of the groups the viewport tests (ER/NR/DB_NT: the loader's alternate-group states).
    /// </summary>
    public DisplayGroupLoaderMode LoaderMode { get; private set; } = DisplayGroupLoaderMode.Stored;

    /// <summary>
    /// Which arrays the list's edit columns work on.
    /// </summary>
    private DisplayGroupEditSet _editSet = DisplayGroupEditSet.Main;

    /// <summary>
    /// A set of group arrays that Get and the edit actions work on. Map objects that inherit their groups from a
    /// collision share that collision's arrays, so they collapse into one target owned by the collision.
    /// </summary>
    private class GroupTarget
    {
        public Entity Owner;

        /// <summary>The map object that owns EditFirst/EditSecond, marked modified when they change.</summary>
        public Entity EditOwner;

        public string OwnerName;
        public bool Inherited;

        // Stored arrays
        public uint[] Draw;
        public uint[] Display;

        // As the viewport tests them (the loader preview, or the stored arrays)
        public uint[] RenderDraw;
        public uint[] RenderDisplay;

        // The arrays the list's edit columns work on (the second is null for single-array sets)
        public uint[] EditFirst;
        public uint[] EditSecond;
    }

    public DisplayGroupTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void SetupDrawgroupCount()
    {
        switch (View.Project.Descriptor.ProjectType)
        {
            // DES, DS1 and DS2 store 4 words (128 groups); the later games store 8 (256 groups).
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
                _dispGroupCount = 4;
                break;
            default:
                _dispGroupCount = 8;
                break;
        }
    }

    #region Data

    private bool HasLoaderModes => View.Project.Descriptor.ProjectType is
        ProjectType.ER or ProjectType.NR or ProjectType.DB_NT or ProjectType.DB;

    private bool HasBackreadGroups => View.Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB;

    private bool HasNavmeshGroups => View.Project.Descriptor.ProjectType is
        ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S;

    /// <summary>
    /// The groups the viewport tests for a map object, for Entity.UpdateDispDrawGroups. Nulls mean "use the stored
    /// arrays" (the Stored mode, or a game without alternate groups).
    /// </summary>
    public (uint[] draw, uint[] display) ResolveRenderGroups(Entity ent)
    {
        if (LoaderMode == DisplayGroupLoaderMode.Stored)
            return (null, null);

        var resolved = DisplayGroupFields.ApplyLoader(ent.RenderGroupOwner ?? ent, ent.DrawGroups, ent.DisplayGroups, LoaderMode);
        return resolved ?? (null, null);
    }

    private void SetLoaderMode(DisplayGroupLoaderMode mode)
    {
        if (mode == LoaderMode)
            return;

        LoaderMode = mode;
        RefreshAllRenderGroups();
    }

    /// <summary>
    /// Re-resolves every loaded map object's groups, after the loader mode or an array the preview reads changed.
    /// </summary>
    private void RefreshAllRenderGroups()
    {
        foreach (var ent in GetLoadedGroupEntities())
        {
            ent.RefreshRenderGroups();
        }

        _statsTime = DateTime.MinValue;
    }

    /// <summary>
    /// Returns the active viewport's display group, set up for this game's group count, or null without a scene.
    /// </summary>
    private DrawGroup GetDisplayGroup()
    {
        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;
        if (scene == null)
            return null;

        DrawGroup dg = scene.DisplayGroup;

        if (dg.AlwaysVisible || dg.RenderGroups == null || dg.RenderGroups.Length != _dispGroupCount)
        {
            dg.RenderGroups = new uint[_dispGroupCount];
            for (var i = 0; i < _dispGroupCount; i++)
            {
                dg.RenderGroups[i] = 0xFFFFFFFF;
            }

            dg.AlwaysVisible = false;
        }

        return dg;
    }

    private List<Entity> GetSelection()
    {
        return View.ViewportSelection.GetFilteredSelection<Entity>(e => e.HasRenderGroups && e.DrawGroups != null).ToList();
    }

    /// <summary>
    /// The selection's group arrays in selection order. For the main set, one target per distinct array, so map
    /// objects inheriting from a collision collapse into it. The other sets (alternate, masks, backread, navmesh)
    /// are each map object's own fields, so every selected map object is its own target there.
    /// </summary>
    private List<GroupTarget> GetTargets(List<Entity> selection)
    {
        var targets = new List<GroupTarget>();
        var seen = new HashSet<uint[]>(ReferenceEqualityComparer.Instance);
        bool mainSet = _editSet == DisplayGroupEditSet.Main;

        foreach (var ent in selection)
        {
            if (mainSet && !seen.Add(ent.DrawGroups))
                continue;

            bool inherited = mainSet && !string.IsNullOrEmpty(ent.RenderGroupRefName);
            Entity owner = ent.RenderGroupOwner ?? ent;

            var target = new GroupTarget
            {
                Owner = owner,
                EditOwner = mainSet ? owner : ent,
                OwnerName = inherited ? ent.RenderGroupRefName : ent.Name,
                Inherited = inherited,
                Draw = ent.DrawGroups,
                Display = ent.DisplayGroups,
                RenderDraw = ent.RenderDrawGroups ?? ent.DrawGroups,
                RenderDisplay = ent.RenderDisplayGroups ?? ent.DisplayGroups
            };

            if (mainSet)
            {
                target.EditFirst = target.Draw;
                target.EditSecond = target.Display;
            }
            else
            {
                var (first, second) = DisplayGroupFields.ArrayNames(_editSet);
                target.EditFirst = DisplayGroupFields.GetArray(ent.WrappedObject, first);
                target.EditSecond = second != null ? DisplayGroupFields.GetArray(ent.WrappedObject, second) : null;
            }

            targets.Add(target);
        }

        return targets;
    }

    /// <summary>
    /// The map objects with render groups in the maps this view has loaded.
    /// </summary>
    private IEnumerable<Entity> GetLoadedGroupEntities()
    {
        foreach (MapWrapper map in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            if (map.MapContainer?.OwnerView != View)
                continue;

            foreach (Entity obj in map.MapContainer.Objects)
            {
                if (obj.HasRenderGroups && obj.DrawGroups != null)
                    yield return obj;
            }
        }
    }

    private static bool HasBit(uint[] groups, int flat)
    {
        int word = flat / BitsPerWord;
        return groups != null && word < groups.Length && (groups[word] >> (flat % BitsPerWord) & 0x1) != 0;
    }

    private static bool IsAllZero(uint[] groups)
    {
        for (var i = 0; i < groups.Length; i++)
        {
            if (groups[i] != 0)
                return false;
        }

        return true;
    }

    private static IEnumerable<int> SetBits(uint[] groups)
    {
        if (groups == null)
            yield break;

        for (int g = 0; g < groups.Length; g++)
        {
            uint word = groups[g];
            while (word != 0)
            {
                int bit = BitOperations.TrailingZeroCount(word);
                yield return g * BitsPerWord + bit;
                word &= word - 1;
            }
        }
    }

    /// <summary>
    /// Counts, for each group, the map objects drawn with it and the collisions that show it, plus how many map
    /// objects the current view hides. Runs at most once a second, or when the view changes.
    /// </summary>
    private void RefreshStats(DrawGroup dg)
    {
        var now = DateTime.UtcNow;
        bool viewChanged = _statsDisplaySnapshot == null || !_statsDisplaySnapshot.SequenceEqual(dg.RenderGroups);

        if (!viewChanged && now - _statsTime < StatsInterval)
            return;

        _statsTime = now;
        _statsDisplaySnapshot = (uint[])dg.RenderGroups.Clone();

        int bitCount = _dispGroupCount * BitsPerWord;
        var drawCounts = new int[bitCount];
        var displayCounts = new int[bitCount];
        var drawArrayUsers = new Dictionary<uint[], int>(ReferenceEqualityComparer.Instance);
        int objectCount = 0;
        int hiddenCount = 0;
        int alwaysDrawnCount = 0;

        foreach (Entity ent in GetLoadedGroupEntities())
        {
            objectCount++;

            // Sharing is counted on the stored arrays; visibility on the groups the viewport tests
            drawArrayUsers[ent.DrawGroups] = drawArrayUsers.GetValueOrDefault(ent.DrawGroups) + 1;

            uint[] draw = ent.RenderDrawGroups ?? ent.DrawGroups;
            uint[] display = ent.RenderDisplayGroups ?? ent.DisplayGroups;

            if (IsAllZero(draw))
            {
                alwaysDrawnCount++;
            }
            else
            {
                if (!IsShown(draw, dg))
                    hiddenCount++;

                foreach (int flat in SetBits(draw))
                {
                    if (flat < bitCount)
                        drawCounts[flat]++;
                }
            }

            // Only a collision's display groups are known to drive what is shown (while standing on it)
            if (EntityHelper.IsPartCollision(ent) && display != null)
            {
                foreach (int flat in SetBits(display))
                {
                    if (flat < bitCount)
                        displayCounts[flat]++;
                }
            }
        }

        _drawCounts = drawCounts;
        _displayCounts = displayCounts;
        _drawArrayUsers = drawArrayUsers;
        _objectCount = objectCount;
        _hiddenCount = hiddenCount;
        _alwaysDrawnCount = alwaysDrawnCount;
    }

    private static bool IsShown(uint[] draw, DrawGroup dg)
    {
        for (int g = 0; g < draw.Length && g < dg.RenderGroups.Length; g++)
        {
            if ((draw[g] & dg.RenderGroups[g]) != 0)
                return true;
        }

        return false;
    }

    private static string FormatGroups(IEnumerable<int> groups)
    {
        var list = groups.ToList();
        var shown = string.Join(", ", list.Take(MaxListedGroups));
        return list.Count > MaxListedGroups ? $"{shown}, …" : shown;
    }

    #endregion

    #region Shortcuts

    /// <summary>
    /// Shortcut: runs whenever the Map Editor handles shortcuts, whether or not the tool is shown.
    /// </summary>
    public void OnShortcut()
    {
        if (GetDisplayGroup() == null)
            return;

        if (InputManager.IsPressed(KeybindID.MapEditor_Show_All_Display_Groups))
            ShowAll();

        if (InputManager.IsPressed(KeybindID.MapEditor_Hide_All_Display_Groups))
            HideAll();

        if (InputManager.IsPressed(KeybindID.MapEditor_View_Display_Group))
            ViewFromCollision();

        if (InputManager.IsPressed(KeybindID.MapEditor_View_Draw_Group))
            IsolateDrawGroups();

        if (InputManager.IsPressed(KeybindID.MapEditor_Apply_Display_Group))
            CopyViewToDisplayGroups();

        if (InputManager.IsPressed(KeybindID.MapEditor_Apply_Draw_Group))
            CopyViewToDrawGroups();

        if (InputManager.IsPressed(KeybindID.MapEditor_Select_Display_Group_Highlights))
            ApplyLastPreset();
    }

    #endregion

    #region Tool Window

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        DrawGroup dg = GetDisplayGroup();

        if (dg == null)
        {
            GUI.WrappedText(LOC.Get("MAP_DisplayGroups_No_Viewport"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_DisplayGroups_Description"));

        // Shared by every tab, so it runs before the tab bar
        RefreshStats(dg);

        var selection = GetSelection();
        var targets = GetTargets(selection);

        GUI.Spacer();

        if (ImGui.BeginTabBar("displayGroupsTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MAP_DisplayGroups_Tab_View")}##displayGroupsTabView"))
            {
                DisplayViewTab(dg);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_DisplayGroups_Tab_Selection")}##displayGroupsTabSelection"))
            {
                DisplaySelectionTab(dg, selection, targets);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_DisplayGroups_Tab_Edit")}##displayGroupsTabEdit"))
            {
                DisplayEditTab(dg, targets);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    /// <summary>
    /// View: what the viewport shows, for the whole map.
    /// </summary>
    private void DisplayViewTab(DrawGroup dg)
    {
        GUI.WrappedText(LOC.Get("MAP_DisplayGroups_Tab_View_Hint"));

        // Preview (ER/NR/DB_NT)
        if (HasLoaderModes)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Preview_Header"), LOC.Get("MAP_DisplayGroups_Loader_TT"));
            DisplayLoaderCombo();
        }

        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Viewport_Header"), LOC.Get("MAP_DisplayGroups_Viewport_Header_TT"));
        DisplayViewportActions();

        // Presets
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Presets_Header"), LOC.Get("MAP_DisplayGroups_Presets_Header_TT"));
        DisplayPresets();

        // Groups
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Groups_Header"), LOC.Get("MAP_DisplayGroups_Groups_Header_TT"));
        DisplayGroupList(dg, null);
    }

    /// <summary>
    /// Selection: whether the selected map objects are drawn, and viewing the map from them.
    /// </summary>
    private void DisplaySelectionTab(DrawGroup dg, List<Entity> selection, List<GroupTarget> targets)
    {
        if (!selection.Any())
        {
            GUI.WrappedText(LOC.Get("MAP_DisplayGroups_Selection_None"));
            return;
        }

        // Visibility
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Visibility_Header"), LOC.Get("MAP_DisplayGroups_Visibility_Header_TT"));
        DisplayVisibility(dg, selection);

        // View From Selection
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Selection_Actions_Header"), LOC.Get("MAP_DisplayGroups_Selection_Actions_Header_TT"));
        DisplaySelectionActions(targets.Any());
    }

    /// <summary>
    /// Edit: the selected map objects' saved groups.
    /// </summary>
    private void DisplayEditTab(DrawGroup dg, List<GroupTarget> targets)
    {
        if (!targets.Any())
        {
            GUI.WrappedText(LOC.Get("MAP_DisplayGroups_Edit_No_Targets"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_DisplayGroups_Edit_Hint"));

        // Targets
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Targets_Header"), LOC.Get("MAP_DisplayGroups_Targets_Header_TT"));
        DisplayTargets(targets);

        // Edit Set
        var sets = AvailableEditSets();
        if (sets.Count > 1)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_EditSet_Header"), LOC.Get("MAP_DisplayGroups_EditSet_TT"));
            DisplayEditSetCombo(sets);
        }

        // Copy
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Copy_Header"), LOC.Get("MAP_DisplayGroups_Copy_Header_TT"));
        DisplayCopyActions();

        // Suggest From Nearby
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Nearby_Header"), LOC.Get("MAP_DisplayGroups_Nearby_Header_TT"));
        DisplayNearbyActions();

        // Groups
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_DisplayGroups_Groups_Header"), LOC.Get("MAP_DisplayGroups_Edit_Groups_Header_TT"));
        DisplayGroupList(dg, targets);
    }

    private void DisplayTargets(List<GroupTarget> targets)
    {
        if (!targets.Any())
        {
            GUI.WrappedText(LOC.Get("MAP_DisplayGroups_Targets_None"));
            return;
        }

        foreach (var target in targets.Take(MaxListedTargets))
        {
            if (target.Inherited)
            {
                int users = _drawArrayUsers.GetValueOrDefault(target.Draw);
                GUI.WrappedText(string.Format(LOC.Get("MAP_DisplayGroups_Target_Inherited"), target.OwnerName, users));
            }
            else
            {
                GUI.WrappedText(target.OwnerName);
            }
        }

        if (targets.Count > MaxListedTargets)
        {
            GUI.WrappedText(string.Format(LOC.Get("MAP_DisplayGroups_Targets_More"), targets.Count - MaxListedTargets));
        }
    }

    /// <summary>
    /// Says whether each selected map object is drawn by the current view, and if not, which groups would show it.
    /// </summary>
    private void DisplayVisibility(DrawGroup dg, List<Entity> selection)
    {
        foreach (var ent in selection.Take(MaxVisibilityRows))
        {
            uint[] draw = ent.RenderDrawGroups ?? ent.DrawGroups;

            if (IsAllZero(draw))
            {
                GUI.WrappedText(string.Format(LOC.Get("MAP_DisplayGroups_Visibility_Always"), ent.Name));
                continue;
            }

            var groups = SetBits(draw).ToList();
            var shownBy = groups.Where(f => HasBit(dg.RenderGroups, f)).ToList();

            if (shownBy.Any())
            {
                GUI.WrappedText(string.Format(LOC.Get("MAP_DisplayGroups_Visibility_Shown"), ent.Name, FormatGroups(shownBy)));
            }
            else
            {
                GUI.WrappedText(string.Format(LOC.Get("MAP_DisplayGroups_Visibility_Hidden"), ent.Name, FormatGroups(groups)));
            }
        }

        if (selection.Count > MaxVisibilityRows)
        {
            GUI.WrappedText(string.Format(LOC.Get("MAP_DisplayGroups_Targets_More"), selection.Count - MaxVisibilityRows));
        }
    }

    private static readonly DisplayGroupLoaderMode[] LoaderModes =
    {
        DisplayGroupLoaderMode.Stored, DisplayGroupLoaderMode.Merged,
        DisplayGroupLoaderMode.Replaced, DisplayGroupLoaderMode.NoAlternate
    };

    private static string LoaderModeName(DisplayGroupLoaderMode mode) => LOC.Get($"MAP_DisplayGroups_Loader_{mode}");

    private void DisplayLoaderCombo()
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##displayGroupsLoaderMode", LoaderModeName(LoaderMode)))
        {
            foreach (var mode in LoaderModes)
            {
                if (ImGui.Selectable($"{LoaderModeName(mode)}##loaderMode_{mode}", mode == LoaderMode))
                    SetLoaderMode(mode);
                GUI.Tooltip(LOC.Get($"MAP_DisplayGroups_Loader_{mode}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Loader_TT"));
    }

    private void DisplayViewportActions()
    {
        GUI.ConditionalMultiButtonInput("dispGroupViewportActions",
            "showAll", LOC.Get("MAP_DisplayGroups_Show_All"),
            $"{LOC.Get("MAP_DisplayGroups_Show_All_TT")}\n{InputManager.GetHint(KeybindID.MapEditor_Show_All_Display_Groups)}", ShowAll, true,

            "hideAll", LOC.Get("MAP_DisplayGroups_Hide_All"),
            $"{LOC.Get("MAP_DisplayGroups_Hide_All_TT")}\n{InputManager.GetHint(KeybindID.MapEditor_Hide_All_Display_Groups)}", HideAll, true);
    }

    /// <summary>
    /// Saved views: click one to show its groups; save the current view under a name.
    /// </summary>
    private void DisplayPresets()
    {
        var presets = Presets.Data.Presets;

        if (!presets.Any())
        {
            GUI.WrappedText(LOC.Get("MAP_DisplayGroups_Presets_None"));
        }
        else
        {
            var rowHeight = ImGui.GetTextLineHeightWithSpacing();
            var height = Math.Min(presets.Count, 8) * rowHeight + ImGui.GetStyle().WindowPadding.Y * 2;

            ImGui.BeginChild("displayGroupsPresetList", new Vector2(-1, height), ImGuiChildFlags.Borders);

            DisplayGroupPreset toDelete = null;

            for (int p = 0; p < presets.Count; p++)
            {
                var preset = presets[p];
                bool last = string.Equals(preset.Name, Presets.Data.LastUsed, StringComparison.OrdinalIgnoreCase);
                var label = last ? string.Format(LOC.Get("MAP_DisplayGroups_Presets_Last_Used"), preset.Name) : preset.Name;

                if (ImGui.Selectable($"{label}##displayGroupsPreset{p}", last))
                {
                    ApplyPreset(preset);
                }
                GUI.Tooltip(string.Format(LOC.Get("MAP_DisplayGroups_Presets_Entry_TT"), SetBits(preset.Groups).Count()));

                if (ImGui.BeginPopupContextItem($"displayGroupsPresetContext{p}"))
                {
                    if (ImGui.Selectable($"{LOC.Get("MAP_DisplayGroups_Presets_Overwrite")}##displayGroupsPresetOverwrite{p}"))
                    {
                        SaveViewAsPreset(preset.Name);
                    }
                    GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Presets_Overwrite_TT"));

                    if (ImGui.Selectable($"{LOC.Get("MAP_DisplayGroups_Presets_Delete")}##displayGroupsPresetDelete{p}"))
                    {
                        toDelete = preset;
                    }
                    GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Presets_Delete_TT"));

                    ImGui.EndPopup();
                }
            }

            // Removed after the loop, so the list isn't changed while it's drawn
            if (toDelete != null)
                Presets.Remove(toDelete);

            ImGui.EndChild();
        }

        // Save Current View
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##displayGroupsPresetName", LOC.Get("MAP_DisplayGroups_Presets_Name_Hint"), ref _presetName, 128);
        GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Presets_Name_TT"));

        bool hasName = !string.IsNullOrWhiteSpace(_presetName);
        bool replaces = hasName && Presets.Find(_presetName.Trim()) != null;

        GUI.ConditionalMultiButtonInput("displayGroupsPresetActions",
            "savePreset", LOC.Get(replaces ? "MAP_DisplayGroups_Presets_Replace" : "MAP_DisplayGroups_Presets_Save"),
            LOC.Get("MAP_DisplayGroups_Presets_Save_TT"), () =>
            {
                SaveViewAsPreset(_presetName);
                _presetName = "";
            }, hasName,

            "applyLastPreset", LOC.Get("MAP_DisplayGroups_Presets_Apply_Last"),
            $"{LOC.Get("MAP_DisplayGroups_Presets_Apply_Last_TT")}\n{InputManager.GetHint(KeybindID.MapEditor_Select_Display_Group_Highlights)}",
            ApplyLastPreset, Presets.Find(Presets.Data.LastUsed) != null);
    }

    private void DisplaySelectionActions(bool hasTargets)
    {
        GUI.ConditionalMultiButtonInput("dispGroupSelectionActions",
            "viewFromCollision", LOC.Get("MAP_DisplayGroups_View_From_Collision"),
            $"{LOC.Get("MAP_DisplayGroups_View_From_Collision_TT")}\n{InputManager.GetHint(KeybindID.MapEditor_View_Display_Group)}", ViewFromCollision, hasTargets,

            "isolateDrawGroups", LOC.Get("MAP_DisplayGroups_Isolate"),
            $"{LOC.Get("MAP_DisplayGroups_Isolate_TT")}\n{InputManager.GetHint(KeybindID.MapEditor_View_Draw_Group)}", IsolateDrawGroups, hasTargets,

            "showSelectionGroups", LOC.Get("MAP_DisplayGroups_Show_Selection"),
            LOC.Get("MAP_DisplayGroups_Show_Selection_TT"), ShowSelectionGroups, hasTargets);
    }

    /// <summary>
    /// Which arrays the list's edit columns change.
    /// </summary>
    private void DisplayEditSetCombo(List<DisplayGroupEditSet> sets)
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##displayGroupsEditSet", EditSetName(_editSet)))
        {
            foreach (var set in sets)
            {
                if (ImGui.Selectable($"{EditSetName(set)}##editSet_{set}", set == _editSet))
                    _editSet = set;
                GUI.Tooltip(LOC.Get($"MAP_DisplayGroups_EditSet_{set}_TT"));
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_DisplayGroups_EditSet_TT"));
    }

    // Suggest From Nearby options and the last run's summary
    private int _nearbyCount = 8;
    private float _nearbyAgreement = 0.5f;
    private bool _nearbyDisplayForCollisions = true;
    private List<string> _nearbyResult = new();

    private void DisplayNearbyActions()
    {
        ImGui.SetNextItemWidth(-1);
        ImGui.SliderInt($"##displayGroupsNearbyCount", ref _nearbyCount, 1, 32,
            string.Format(LOC.Get("MAP_DisplayGroups_Nearby_Count"), "%d"));
        GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Nearby_Count_TT"));

        var agreementPercent = (int)MathF.Round(_nearbyAgreement * 100);
        ImGui.SetNextItemWidth(-1);
        if (ImGui.SliderInt($"##displayGroupsNearbyAgreement", ref agreementPercent, 10, 100,
            string.Format(LOC.Get("MAP_DisplayGroups_Nearby_Agreement"), "%d%%")))
        {
            _nearbyAgreement = agreementPercent / 100.0f;
        }
        GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Nearby_Agreement_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_DisplayGroups_Nearby_Display")}##displayGroupsNearbyDisplay", ref _nearbyDisplayForCollisions);
        GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Nearby_Display_TT"));

        GUI.MultiButtonInput("displayGroupsNearbyActions",
            "assignFromNearby", LOC.Get("MAP_DisplayGroups_Nearby_Assign"),
            LOC.Get("MAP_DisplayGroups_Nearby_Assign_TT"), AssignFromNearby);

        foreach (var line in _nearbyResult)
        {
            GUI.WrappedText(line);
        }
    }

    /// <summary>
    /// Gives every target the groups its nearest map objects agree on (DisplayGroupNeighbours), as one undo step.
    /// Draw groups come from map objects with their own draw groups; a collision's display groups come from the
    /// nearest collisions. Neighbours are taken from the target's own map, and never from the selection.
    /// </summary>
    public void AssignFromNearby()
    {
        var selection = GetSelection();

        // One target per stored draw array, as in the main edit set, whatever the edit set combo says
        var targets = new List<GroupTarget>();
        var seen = new HashSet<uint[]>(ReferenceEqualityComparer.Instance);
        foreach (var target in GetTargets(selection))
        {
            if (seen.Add(target.Draw))
                targets.Add(target);
        }

        if (!targets.Any())
        {
            Smithbox.LogError<DisplayGroupTool>(LOC.Get("MAP_DisplayGroups_Error_No_Selection"));
            return;
        }

        // The selection and the arrays' owners never vote for themselves
        var excluded = new HashSet<Entity>(selection);
        foreach (var target in targets)
            excluded.Add(target.Owner);

        // Candidates per map, built once
        var drawCandidates = new Dictionary<ObjectContainer, List<DisplayGroupNeighbours.Candidate>>();
        var displayCandidates = new Dictionary<ObjectContainer, List<DisplayGroupNeighbours.Candidate>>();

        foreach (var ent in GetLoadedGroupEntities())
        {
            if (excluded.Contains(ent) || ent.Container == null)
                continue;

            // Map objects inheriting through Collision Name repeat their collision's groups; count the collision once
            if (!string.IsNullOrEmpty(ent.RenderGroupRefName))
                continue;

            var bounds = DisplayGroupNeighbours.BoundsOf(ent);

            if (!IsAllZero(ent.DrawGroups) && !ent.DrawGroups.All(w => w == 0xFFFFFFFF))
            {
                if (!drawCandidates.TryGetValue(ent.Container, out var list))
                    drawCandidates[ent.Container] = list = new();
                list.Add(new DisplayGroupNeighbours.Candidate { Entity = ent, Bounds = bounds, Groups = ent.DrawGroups });
            }

            if (EntityHelper.IsPartCollision(ent) && ent.DisplayGroups != null && !IsAllZero(ent.DisplayGroups))
            {
                if (!displayCandidates.TryGetValue(ent.Container, out var list))
                    displayCandidates[ent.Container] = list = new();
                list.Add(new DisplayGroupNeighbours.Candidate { Entity = ent, Bounds = bounds, Groups = ent.DisplayGroups });
            }
        }

        var changes = new List<DisplayGroupEditAction.Change>();
        var result = new List<string>();
        int skipped = 0;

        foreach (var target in targets)
        {
            var owner = target.Owner;
            var bounds = DisplayGroupNeighbours.BoundsOf(owner);
            var container = owner.Container;

            // Draw groups
            var draw = container != null && drawCandidates.TryGetValue(container, out var drawList)
                ? DisplayGroupNeighbours.Suggest(bounds, drawList, _nearbyCount, _nearbyAgreement, target.Draw.Length)
                : null;

            if (draw == null)
            {
                skipped++;
                continue;
            }

            AddChange(changes, target.Draw, draw.Groups, owner);

            string line = string.Format(LOC.Get("MAP_DisplayGroups_Nearby_Result_Draw"), target.OwnerName,
                FormatGroups(SetBits(draw.Groups)), draw.NeighbourCount, $"{draw.FarthestDistance:0.#}");

            // Display groups, for collisions only
            if (_nearbyDisplayForCollisions && EntityHelper.IsPartCollision(owner) && target.Display != null
                && displayCandidates.TryGetValue(container, out var displayList))
            {
                var display = DisplayGroupNeighbours.Suggest(bounds, displayList, _nearbyCount, _nearbyAgreement, target.Display.Length);
                if (display != null)
                {
                    AddChange(changes, target.Display, display.Groups, owner);
                    line += " " + string.Format(LOC.Get("MAP_DisplayGroups_Nearby_Result_Display"), FormatGroups(SetBits(display.Groups)));
                }
            }

            result.Add(line);
        }

        if (skipped > 0)
            result.Add(string.Format(LOC.Get("MAP_DisplayGroups_Nearby_Result_Skipped"), skipped));

        // Long selections: keep the summary short
        if (result.Count > MaxListedTargets + 1)
        {
            int hidden = result.Count - MaxListedTargets;
            result = result.Take(MaxListedTargets).Append(string.Format(LOC.Get("MAP_DisplayGroups_Targets_More"), hidden)).ToList();
        }

        _nearbyResult = result;

        if (changes.Any())
            View.ViewportActionManager.ExecuteAction(new DisplayGroupEditAction(changes, AfterGroupEdit));
        else if (skipped == 0)
            _nearbyResult.Add(LOC.Get("MAP_DisplayGroups_Nearby_Result_Unchanged"));
    }

    private static void AddChange(List<DisplayGroupEditAction.Change> changes, uint[] target, uint[] values, Entity owner)
    {
        var next = new uint[target.Length];
        for (int i = 0; i < next.Length && i < values.Length; i++)
            next[i] = values[i];

        if (target.SequenceEqual(next))
            return;

        changes.Add(new DisplayGroupEditAction.Change
        {
            Target = target,
            OldValues = (uint[])target.Clone(),
            NewValues = next,
            Owner = owner
        });
    }

    private void DisplayCopyActions()
    {
        GUI.MultiButtonInput("dispGroupEditActions",
            "copyToDraw", LOC.Get("MAP_DisplayGroups_Copy_To_Draw"),
            $"{LOC.Get("MAP_DisplayGroups_Copy_To_Draw_TT")}\n{InputManager.GetHint(KeybindID.MapEditor_Apply_Draw_Group)}", CopyViewToDrawGroups,

            "copyToDisplay", LOC.Get("MAP_DisplayGroups_Copy_To_Display"),
            $"{LOC.Get("MAP_DisplayGroups_Copy_To_Display_TT")}\n{InputManager.GetHint(KeybindID.MapEditor_Apply_Display_Group)}", CopyViewToDisplayGroups);
    }

    private List<DisplayGroupEditSet> AvailableEditSets()
    {
        var sets = new List<DisplayGroupEditSet> { DisplayGroupEditSet.Main };

        if (HasLoaderModes)
        {
            sets.Add(DisplayGroupEditSet.Alternate);
            sets.Add(DisplayGroupEditSet.AlternateMask);
        }

        if (HasBackreadGroups)
            sets.Add(DisplayGroupEditSet.Backread);

        if (HasNavmeshGroups)
            sets.Add(DisplayGroupEditSet.Navmesh);

        // A project switch can leave a set behind that this game doesn't have
        if (!sets.Contains(_editSet))
            _editSet = DisplayGroupEditSet.Main;

        return sets;
    }

    private static string EditSetName(DisplayGroupEditSet set) => LOC.Get($"MAP_DisplayGroups_EditSet_{set}");

    /// <summary>
    /// The list's column titles for the current edit set; the second is null for single-array sets.
    /// </summary>
    private (string first, string second) EditColumnNames() => _editSet switch
    {
        DisplayGroupEditSet.Alternate => (LOC.Get("MAP_DisplayGroups_Column_Alt_Draw"), LOC.Get("MAP_DisplayGroups_Column_Alt_Display")),
        DisplayGroupEditSet.AlternateMask => (LOC.Get("MAP_DisplayGroups_Column_Draw_Mask"), LOC.Get("MAP_DisplayGroups_Column_Display_Mask")),
        DisplayGroupEditSet.Backread => (LOC.Get("MAP_DisplayGroups_Column_Backread"), null),
        DisplayGroupEditSet.Navmesh => (LOC.Get("MAP_DisplayGroups_Column_Navmesh"), null),
        _ => (LOC.Get("MAP_DisplayGroups_Column_Target_Draw"), LOC.Get("MAP_DisplayGroups_Column_Target_Display"))
    };

    #endregion

    #region Viewport Actions

    public void ShowAll()
    {
        DrawGroup dg = GetDisplayGroup();
        if (dg == null)
            return;

        for (var i = 0; i < _dispGroupCount; i++)
        {
            dg.RenderGroups[i] = 0xFFFFFFFF;
        }
    }

    public void HideAll()
    {
        DrawGroup dg = GetDisplayGroup();
        if (dg == null)
            return;

        for (var i = 0; i < _dispGroupCount; i++)
        {
            dg.RenderGroups[i] = 0;
        }
    }

    /// <summary>
    /// Shows what the player sees standing on the first target: its collision's display groups.
    /// </summary>
    public void ViewFromCollision()
    {
        DrawGroup dg = GetDisplayGroup();
        var target = GetTargets(GetSelection()).FirstOrDefault();

        if (dg == null || target == null || target.RenderDisplay == null)
        {
            Smithbox.LogError<DisplayGroupTool>(LOC.Get("MAP_DisplayGroups_Error_No_Selection"));
            return;
        }

        if (!EntityHelper.IsPartCollision(target.Owner))
        {
            Smithbox.LogWarning<DisplayGroupTool>(string.Format(LOC.Get("MAP_DisplayGroups_Warning_Not_Collision"), target.OwnerName));
        }

        for (var i = 0; i < _dispGroupCount; i++)
        {
            dg.RenderGroups[i] = i < target.RenderDisplay.Length ? target.RenderDisplay[i] : 0;
        }
    }

    /// <summary>
    /// Shows only the groups the selection draws with.
    /// </summary>
    public void IsolateDrawGroups()
    {
        SetViewFromSelection(replace: true);
    }

    /// <summary>
    /// Adds the groups the selection draws with to the view, so hidden selected objects appear.
    /// </summary>
    public void ShowSelectionGroups()
    {
        SetViewFromSelection(replace: false);
    }

    private void SetViewFromSelection(bool replace)
    {
        DrawGroup dg = GetDisplayGroup();
        var targets = GetTargets(GetSelection());

        if (dg == null || !targets.Any())
        {
            Smithbox.LogError<DisplayGroupTool>(LOC.Get("MAP_DisplayGroups_Error_No_Selection"));
            return;
        }

        for (var i = 0; i < _dispGroupCount; i++)
        {
            uint union = 0;
            foreach (var target in targets)
            {
                if (i < target.RenderDraw.Length)
                    union |= target.RenderDraw[i];
            }

            dg.RenderGroups[i] = replace ? union : dg.RenderGroups[i] | union;
        }
    }

    /// <summary>
    /// Shows exactly a preset's groups. A preset saved in a game with more words than this one is cut short, and
    /// a shorter one leaves the extra words hidden.
    /// </summary>
    private void ApplyPreset(DisplayGroupPreset preset)
    {
        DrawGroup dg = GetDisplayGroup();
        if (dg == null || preset == null)
            return;

        for (int g = 0; g < dg.RenderGroups.Length; g++)
        {
            dg.RenderGroups[g] = g < preset.Groups.Length ? preset.Groups[g] : 0;
        }

        Presets.MarkUsed(preset);
    }

    /// <summary>
    /// Applies the preset used most recently (the Apply Last Preset keybind).
    /// </summary>
    public void ApplyLastPreset()
    {
        var preset = Presets.Find(Presets.Data.LastUsed);
        if (preset == null)
        {
            Smithbox.LogError<DisplayGroupTool>(LOC.Get("MAP_DisplayGroups_Error_No_Last_Preset"));
            return;
        }

        ApplyPreset(preset);
    }

    private void SaveViewAsPreset(string name)
    {
        DrawGroup dg = GetDisplayGroup();
        if (dg == null || string.IsNullOrWhiteSpace(name))
            return;

        Presets.Store(name.Trim(), dg.RenderGroups);
    }

    /// <summary>
    /// Selects the loaded map objects drawn with a group, or the collisions that show it.
    /// </summary>
    private void SelectGroupMembers(int flat, bool display)
    {
        var members = GetLoadedGroupEntities()
            .Where(e => display
                ? EntityHelper.IsPartCollision(e) && HasBit(e.RenderDisplayGroups ?? e.DisplayGroups, flat)
                : HasBit(e.RenderDrawGroups ?? e.DrawGroups, flat))
            .Cast<ISelectable>()
            .ToList();

        View.ViewportSelection.ClearSelection();
        View.ViewportSelection.AddSelection(members);
    }

    #endregion

    #region Edit Actions

    public void CopyViewToDrawGroups()
    {
        CopyViewToTargets(t => t.Draw);
    }

    public void CopyViewToDisplayGroups()
    {
        CopyViewToTargets(t => t.Display);
    }

    private void CopyViewToTargets(Func<GroupTarget, uint[]> arrayOf)
    {
        DrawGroup dg = GetDisplayGroup();
        if (dg == null)
            return;

        EditTargets(arrayOf, _ => (uint[])dg.RenderGroups.Clone(), t => t.Owner);
    }

    /// <summary>
    /// Adds a group to, or removes it from, one array of every target.
    /// </summary>
    private void SetTargetsBit(Func<GroupTarget, uint[]> arrayOf, int flat, bool value)
    {
        int word = flat / BitsPerWord;
        uint mask = 1u << (flat % BitsPerWord);

        EditTargets(arrayOf, current =>
        {
            var next = (uint[])current.Clone();
            if (word < next.Length)
                next[word] = value ? next[word] | mask : next[word] & ~mask;
            return next;
        }, t => t.EditOwner);
    }

    /// <summary>
    /// Writes new values into one array of every target as a single undo step.
    /// </summary>
    private void EditTargets(Func<GroupTarget, uint[]> arrayOf, Func<uint[], uint[]> newValues, Func<GroupTarget, Entity> ownerOf)
    {
        var targets = GetTargets(GetSelection());

        var changes = new List<DisplayGroupEditAction.Change>();
        var seen = new HashSet<uint[]>(ReferenceEqualityComparer.Instance);

        foreach (var target in targets)
        {
            var array = arrayOf(target);
            if (array == null || !seen.Add(array))
                continue;

            var next = newValues(array);
            if (array.SequenceEqual(next.Take(array.Length)))
                continue;

            changes.Add(new DisplayGroupEditAction.Change
            {
                Target = array,
                OldValues = (uint[])array.Clone(),
                NewValues = next,
                Owner = ownerOf(target)
            });
        }

        if (!targets.Any())
        {
            Smithbox.LogError<DisplayGroupTool>(LOC.Get("MAP_DisplayGroups_Error_No_Selection"));
            return;
        }

        if (!changes.Any())
            return;

        View.ViewportActionManager.ExecuteAction(new DisplayGroupEditAction(changes, AfterGroupEdit));
    }

    /// <summary>
    /// After an edit, undo or redo: a loader preview holds copies derived from the stored arrays, so rebuild them;
    /// otherwise the viewport already reads the edited arrays and only the counts are stale.
    /// </summary>
    private void AfterGroupEdit()
    {
        if (LoaderMode != DisplayGroupLoaderMode.Stored)
            RefreshAllRenderGroups();
        else
            _statsTime = DateTime.MinValue;
    }

    #endregion

    #region Group List

    /// <summary>
    /// The group list. With targets (the Edit tab), it adds their edit columns and lists the groups they hold.
    /// </summary>
    private void DisplayGroupList(DrawGroup dg, List<GroupTarget> targets)
    {
        targets ??= new List<GroupTarget>();
        int bitCount = dg.RenderGroups.Length * BitsPerWord;

        // Show Unused
        var showUnused = CFG.Current.MapEditor_DisplayGroups_Show_Unused;
        if (ImGui.Checkbox($"{LOC.Get("MAP_DisplayGroups_Show_Unused")}##displayGroupsShowUnused", ref showUnused))
        {
            CFG.Current.MapEditor_DisplayGroups_Show_Unused = showUnused;
        }
        GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Show_Unused_TT"));

        // Groups the targets' edit arrays hold are listed too, so e.g. alternate or backread groups can be removed
        var targetBits = new HashSet<int>();
        foreach (var target in targets)
        {
            targetBits.UnionWith(SetBits(target.EditFirst));
            targetBits.UnionWith(SetBits(target.EditSecond));
        }

        var rows = new List<int>();
        int usedCount = 0;
        for (int flat = 0; flat < bitCount; flat++)
        {
            bool used = flat < _drawCounts.Length && (_drawCounts[flat] > 0 || _displayCounts[flat] > 0);
            if (used)
                usedCount++;

            if (used || showUnused || targetBits.Contains(flat))
                rows.Add(flat);
        }

        // Status
        GUI.WrappedText(string.Format(LOC.Get("MAP_DisplayGroups_Status"),
            usedCount, bitCount, _hiddenCount, _objectCount, _alwaysDrawnCount));

        if (rows.Count == 0)
        {
            GUI.WrappedText(_objectCount == 0
                ? LOC.Get("MAP_DisplayGroups_Empty_No_Maps")
                : LOC.Get("MAP_DisplayGroups_Empty_No_Groups"));
            return;
        }

        // A drag ends when the button is released, wherever the cursor is
        if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
            _paintValue = null;

        var (firstColumn, secondColumn) = EditColumnNames();
        bool hasFirst = targets.Any(t => t.EditFirst != null);
        bool hasSecond = secondColumn != null && targets.Any(t => t.EditSecond != null);
        int columnCount = 4 + (hasFirst ? 1 : 0) + (hasSecond ? 1 : 0);

        var rowHeight = ImGui.GetFrameHeightWithSpacing();
        var tableHeight = (Math.Min(rows.Count, 16) + 1) * rowHeight + ImGui.GetStyle().CellPadding.Y * 2;

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        if (ImGui.BeginTable("displayGroupsTable", columnCount, flags, new Vector2(-1, tableHeight)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("MAP_DisplayGroups_Column_Shown"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_DisplayGroups_Column_Group"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn(LOC.Get("MAP_DisplayGroups_Column_Drawn"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn(LOC.Get("MAP_DisplayGroups_Column_Shown_By"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
            if (hasFirst)
                ImGui.TableSetupColumn(firstColumn, ImGuiTableColumnFlags.WidthFixed);
            if (hasSecond)
                ImGui.TableSetupColumn(secondColumn, ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(rows.Count);

            while (clipper.Step())
            {
                for (var r = clipper.DisplayStart; r < clipper.DisplayEnd; r++)
                {
                    int flat = rows[r];
                    int g = flat / BitsPerWord;
                    int i = flat % BitsPerWord;

                    ImGui.TableNextRow();

                    // Shown
                    ImGui.TableSetColumnIndex(0);
                    DisplayShownCell(dg, g, i);

                    // Group
                    ImGui.TableSetColumnIndex(1);
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{flat}");
                    GUI.Tooltip(string.Format(LOC.Get("MAP_DisplayGroups_Group_TT"), flat, g, i));

                    // Drawn
                    ImGui.TableSetColumnIndex(2);
                    DisplayCountCell(_drawCounts[flat], $"drawn_{flat}", "MAP_DisplayGroups_Drawn_TT", () => SelectGroupMembers(flat, false));

                    // Shown By
                    ImGui.TableSetColumnIndex(3);
                    DisplayCountCell(_displayCounts[flat], $"shownBy_{flat}", "MAP_DisplayGroups_Shown_By_TT", () => SelectGroupMembers(flat, true));

                    // Targets' first edit array (draw groups, or the chosen set's)
                    if (hasFirst)
                    {
                        ImGui.TableSetColumnIndex(4);
                        DisplayTargetBitCell(targets, t => t.EditFirst, flat, $"targetFirst_{flat}",
                            UI.Current.DisplayGroupEditor_DrawActive_Checkbox,
                            (have, count) => EditCellTooltip(true, firstColumn, have, count));
                    }

                    // Targets' second edit array (display groups, or the chosen set's)
                    if (hasSecond)
                    {
                        ImGui.TableSetColumnIndex(5);
                        DisplayTargetBitCell(targets, t => t.EditSecond, flat, $"targetSecond_{flat}",
                            UI.Current.DisplayGroupEditor_DisplayActive_Checkbox,
                            (have, count) => EditCellTooltip(false, secondColumn, have, count));
                    }
                }
            }

            ImGui.EndTable();
        }
    }

    private string EditCellTooltip(bool first, string column, int have, int count)
    {
        if (_editSet == DisplayGroupEditSet.Main)
        {
            return string.Format(LOC.Get(first ? "MAP_DisplayGroups_Target_Draw_TT" : "MAP_DisplayGroups_Target_Display_TT"), have, count);
        }

        return string.Format(LOC.Get("MAP_DisplayGroups_Target_Edit_TT"), have, count, column);
    }

    private void DisplayShownCell(DrawGroup dg, int g, int i)
    {
        var check = (dg.RenderGroups[g] >> i & 0x1) > 0;

        // The checkbox only draws the cell: its own click result is ignored, and the paint
        // logic below sets the bit, so a click and a drag can't toggle the same cell twice.
        var shown = check;
        ImGui.Checkbox($"##cell_{g}_{i}", ref shown);

        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem))
        {
            // Left press starts a drag that paints the opposite of this cell's value
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                _paintValue = !check;

            if (_paintValue.HasValue && _paintValue.Value != check)
            {
                if (_paintValue.Value)
                    dg.RenderGroups[g] |= 1u << i;
                else
                    dg.RenderGroups[g] &= ~(1u << i);
            }
        }

        if (_paintValue == null)
            GUI.Tooltip(LOC.Get("MAP_DisplayGroups_Shown_TT"));
    }

    /// <summary>
    /// A tick for whether the targets have a group: ticked when all do, a filled square when only some do.
    /// Clicking adds the group to every target, or removes it when all of them already have it.
    /// </summary>
    private void DisplayTargetBitCell(List<GroupTarget> targets, Func<GroupTarget, uint[]> arrayOf, int flat,
        string id, Vector4 colour, Func<int, int, string> tooltip)
    {
        var arrays = targets.Select(arrayOf).Where(a => a != null).ToList();
        if (!arrays.Any())
            return;

        int have = arrays.Count(a => HasBit(a, flat));
        bool all = have == arrays.Count;
        bool mixed = have > 0 && !all;

        ImGui.PushStyleColor(ImGuiCol.CheckMark, colour);

        var value = all;
        if (ImGui.Checkbox($"##{id}", ref value))
        {
            SetTargetsBit(arrayOf, flat, !all);
        }

        if (mixed)
        {
            // Mixed state, drawn as ImGui does for its own tri-state checkboxes
            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var size = max.Y - min.Y;
            var pad = MathF.Max(1.0f, MathF.Floor(size / 3.6f));
            ImGui.GetWindowDrawList().AddRectFilled(
                new Vector2(min.X + pad, min.Y + pad),
                new Vector2(min.X + size - pad, min.Y + size - pad),
                ImGui.ColorConvertFloat4ToU32(colour));
        }

        ImGui.PopStyleColor();

        GUI.Tooltip(tooltip(have, arrays.Count));
    }

    private static void DisplayCountCell(int count, string id, string tooltipKey, Action onClick)
    {
        if (count == 0)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled("0");
            return;
        }

        if (ImGui.Selectable($"{count}##{id}", false, ImGuiSelectableFlags.None))
        {
            onClick();
        }
        GUI.Tooltip(LOC.Get(tooltipKey));
    }

    #endregion
}
