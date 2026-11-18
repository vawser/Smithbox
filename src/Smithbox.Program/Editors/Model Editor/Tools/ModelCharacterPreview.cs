using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
///     The character being dressed, as described by the editor hosting the tool.
/// </summary>
/// <param name="ModelName">Name of the loaded model.</param>
/// <param name="Appearance">The parts that are currently displayed.</param>
/// <param name="Assign">Displays a new set of parts.</param>
public sealed record CharacterPreviewTarget(string ModelName, CharacterAppearance Appearance, Action<CharacterAppearance> Assign);

/// <summary>
///     Chooses what a player-like character is wearing while it is displayed. Nothing here is saved.
/// </summary>
public class ModelCharacterPreview
{
    private enum PreviewSource
    {
        CharaInit,
        Loadout
    }

    private const int SlotListRows = 8;

    private static readonly (CharacterSlot Slot, string Name)[] Slots =
    {
        (CharacterSlot.Head, "Head"),
        (CharacterSlot.Body, "Body"),
        (CharacterSlot.Arms, "Arms"),
        (CharacterSlot.Legs, "Legs"),
        (CharacterSlot.RightHand, "Right Hand"),
        (CharacterSlot.LeftHand, "Left Hand")
    };

    public ProjectEntry Project;

    // Null while no model is loaded
    private readonly Func<CharacterPreviewTarget> _getTarget;

    private readonly CharacterLoadout _loadout = new();
    private readonly Dictionary<CharacterSlot, IReadOnlyList<string>> _slotParts = new();

    // Whichever tab is open decides what the character wears
    private PreviewSource _source = PreviewSource.CharaInit;

    // The CharaInitParam row characters are dressed from. Anything negative leaves them with a bare body.
    private int _selectedCharaInitId = -1;

    // Set by DressFromCharaInit, so the tab bar shows the CharaInitParam tab on its next draw
    private bool _selectCharaInitTab;

    private Dictionary<string, string> _partAliases;

    private string _filter = "";
    private readonly Dictionary<CharacterSlot, string> _slotFilters = new();

    /// <summary>
    ///     Shown in place of the tool while there is no target. Null for the default (load c0000).
    /// </summary>
    public string NoTargetMessage { get; set; }

    /// <summary>
    ///     Set when the host chooses the body's sex itself (the FaceGen Editor, by the face's): the Individual Parts
    ///     tab shows this in place of its Male and Female choice.
    /// </summary>
    public string SexChoiceNote { get; set; }

    public ModelCharacterPreview(ProjectEntry project, Func<CharacterPreviewTarget> getTarget)
    {
        Project = project;
        _getTarget = getTarget;
    }

    /// <summary>
    ///     Whether nothing is chosen: the bare body of the CharaInitParam tab, or an Individual Parts tab with
    ///     every slot empty.
    /// </summary>
    public bool IsBare => _source is PreviewSource.Loadout
        ? _loadout.Parts.Count == 0
        : _selectedCharaInitId < 0;

    /// <summary>
    ///     Gets the parts for the current choices.
    /// </summary>
    public CharacterAppearance GetAppearance()
    {
        return _source is PreviewSource.Loadout
            ? CharacterComposition.Resolve(Project, _loadout)
            : CharacterComposition.Resolve(Project, _selectedCharaInitId);
    }

    /// <summary>
    ///     Gets the parts for the current choices on a body of the given sex, whatever the choices say.
    /// </summary>
    public CharacterAppearance GetAppearance(bool isFemale)
    {
        if (_source is not PreviewSource.Loadout)
        {
            return CharacterComposition.Resolve(Project, _selectedCharaInitId, isFemale);
        }

        CharacterLoadout loadout = new() { IsFemale = isFemale };

        foreach ((CharacterSlot slot, var part) in _loadout.Parts)
        {
            loadout.Parts[slot] = part;
        }

        return CharacterComposition.Resolve(Project, loadout);
    }

    public void OnToolWindow()
    {
        if (ToolSections.Header($"{LOC.Get("MODEL_CharacterPreview_Header")}##modelCharacterPreviewHeader"))
        {
            ImGui.BeginChild("ModelCharacterPreviewSection");
            Display();
            ImGui.EndChild();
        }
    }

    private void Display()
    {
        CharacterPreviewTarget target = _getTarget();

        if (target == null || !CharacterComposition.IsComposite(target.ModelName))
        {
            GUI.WrappedText(NoTargetMessage ?? $"Load {CharacterComposition.CompositeModel} to preview its equipment.");
            return;
        }

        if (target.Appearance is { Parts.Count: > 0 } appearance)
        {
            GUI.SimpleHeader("Parts", "The models that are currently displayed.");

            foreach (CharacterPart part in appearance.Parts)
            {
                GUI.WrappedText(GetPartLabel(part.Name));
            }

            GUI.Spacer();
        }
        else
        {
            GUI.WrappedText("This game has no parts to display.");
        }

        if (ImGui.BeginTabBar("characterPreviewTabs"))
        {
            // Opened from outside (DressFromCharaInit): the CharaInitParam tab is brought forward. ImGui switches
            // tabs a frame later, so until it does the other tab must not take the source back.
            var charaInitFlags = _selectCharaInitTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

            if (ImGui.BeginTabItem("CharaInitParam##characterPreviewCharaInit", charaInitFlags))
            {
                _selectCharaInitTab = false;
                SetSource(target, PreviewSource.CharaInit);
                DisplayCharaInitList(target);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Individual Parts##characterPreviewLoadout"))
            {
                if (!_selectCharaInitTab)
                {
                    SetSource(target, PreviewSource.Loadout);
                }

                DisplayLoadout(target);
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void SetSource(CharacterPreviewTarget target, PreviewSource source)
    {
        if (_source != source)
        {
            _source = source;
            Apply(target);
        }
    }

    private void DisplayCharaInitList(CharacterPreviewTarget target)
    {
        GUI.SimpleHeader("Equipment", "Click a row of CharaInitParam to dress the character in it.");

        Param charaInit = null;
        Project.Handler.ParamData?.PrimaryBank?.Params?.TryGetValue("CharaInitParam", out charaInit);

        if (charaInit == null)
        {
            GUI.WrappedText("CharaInitParam is not available.");
            return;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##characterPreviewFilter", "Filter...", ref _filter, 255);

        if (ImGui.BeginChild("characterPreviewList", default, ImGuiChildFlags.Borders))
        {
            if (ImGui.Selectable("Bare body##characterPreviewBare", _selectedCharaInitId < 0))
            {
                SelectCharaInit(target, -1);
            }

            foreach (Param.Row row in charaInit.Rows)
            {
                var label = $"[{row.ID}] {row.Name}";

                if (_filter.Length > 0 && !label.Contains(_filter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (ImGui.Selectable($"{label}##characterPreviewRow{row.ID}", row.ID == _selectedCharaInitId))
                {
                    SelectCharaInit(target, row.ID);
                }
            }
        }

        ImGui.EndChild();
    }

    private void SelectCharaInit(CharacterPreviewTarget target, int charaInitId)
    {
        _selectedCharaInitId = charaInitId;

        Apply(target);
    }

    /// <summary>
    ///     Dresses the character in a CharaInitParam row from outside the tool (the FaceGen Editor's References
    ///     window), and opens the tool's CharaInitParam tab so the choice stays when the tool is next shown.
    /// </summary>
    public void DressFromCharaInit(int charaInitId)
    {
        _source = PreviewSource.CharaInit;
        _selectedCharaInitId = charaInitId;
        _selectCharaInitTab = true;

        if (_getTarget() is { } target)
        {
            Apply(target);
        }
    }

    /// <summary>The CharaInitParam row the character is dressed from, or -1 for none (or another tab)</summary>
    public int DressedCharaInitId => _source is PreviewSource.CharaInit ? _selectedCharaInitId : -1;

    private void DisplayLoadout(CharacterPreviewTarget target)
    {
        GUI.SimpleHeader("Slots", "Choose the part model for each slot. Empty slots show the bare body.");

        var isFemale = _loadout.IsFemale;

        if (SexChoiceNote != null)
        {
            GUI.WrappedText(SexChoiceNote);
        }
        else
        {
            if (ImGui.RadioButton("Male##characterPreviewMale", !isFemale) && isFemale)
            {
                _loadout.IsFemale = false;
                Apply(target);
            }

            ImGui.SameLine();

            if (ImGui.RadioButton("Female##characterPreviewFemale", isFemale) && !isFemale)
            {
                _loadout.IsFemale = true;
                Apply(target);
            }
        }

        GUI.Spacer();

        if (ImGui.Button("Clear All##characterPreviewClear"))
        {
            _loadout.Parts.Clear();
            Apply(target);
        }

        GUI.Spacer();

        foreach ((CharacterSlot slot, var name) in Slots)
        {
            DisplaySlot(target, slot, name);
        }
    }

    private void DisplaySlot(CharacterPreviewTarget target, CharacterSlot slot, string name)
    {
        _loadout.Parts.TryGetValue(slot, out var current);

        // The header says what's chosen, so that it can still be seen with the section closed.
        // The ID stays the same, as a change of label would otherwise close the section.
        var header = current != null ? $"{name}: {GetPartLabel(current)}" : name;

        if (!ImGui.CollapsingHeader($"{header}###characterPreviewSection{slot}"))
        {
            return;
        }

        _slotFilters.TryGetValue(slot, out var filter);
        filter ??= "";

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputTextWithHint($"##characterPreviewFilter{slot}", "Filter...", ref filter, 255))
        {
            _slotFilters[slot] = filter;
        }

        if (!ImGui.BeginListBox($"##characterPreviewSlot{slot}", new Vector2(-1, ImGui.GetTextLineHeightWithSpacing() * SlotListRows)))
        {
            return;
        }

        if (ImGui.Selectable($"None##characterPreviewNone{slot}", current == null))
        {
            _loadout.Parts.Remove(slot);
            Apply(target);
        }

        if (!_slotParts.TryGetValue(slot, out IReadOnlyList<string> parts))
        {
            parts = CharacterComposition.GetSlotParts(Project, slot);
            _slotParts[slot] = parts;
        }

        foreach (var part in parts)
        {
            var label = GetPartLabel(part);

            if (filter.Length > 0 && !label.Contains(filter, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (ImGui.Selectable($"{label}##characterPreviewPart{slot}{part}", part == current))
            {
                _loadout.Parts[slot] = part;
                Apply(target);
            }
        }

        ImGui.EndListBox();

        GUI.Spacer();
    }

    private string GetPartLabel(string part)
    {
        if (_partAliases == null)
        {
            _partAliases = new(StringComparer.OrdinalIgnoreCase);

            if (Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Parts, out List<AliasEntry> aliases))
            {
                foreach (AliasEntry alias in aliases.Where(a => !string.IsNullOrEmpty(a.Name)))
                {
                    _partAliases.TryAdd(alias.ID, alias.Name);
                }
            }
        }

        return _partAliases.TryGetValue(part, out var name) ? $"{part} <{name}>" : part;
    }

    private void Apply(CharacterPreviewTarget target)
    {
        target.Assign(GetAppearance());
    }
}
