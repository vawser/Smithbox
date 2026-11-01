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

    private Dictionary<string, string> _partAliases;

    private string _filter = "";
    private readonly Dictionary<CharacterSlot, string> _slotFilters = new();

    public ModelCharacterPreview(ProjectEntry project, Func<CharacterPreviewTarget> getTarget)
    {
        Project = project;
        _getTarget = getTarget;
    }

    /// <summary>
    ///     Gets the parts for the current choices.
    /// </summary>
    public CharacterAppearance GetAppearance()
    {
        return _source is PreviewSource.Loadout
            ? CharacterComposition.Resolve(Project, _loadout)
            : CharacterComposition.Resolve(Project, _selectedCharaInitId);
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
            GUI.WrappedText($"Load {CharacterComposition.CompositeModel} to preview its equipment.");
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
            if (ImGui.BeginTabItem("CharaInitParam##characterPreviewCharaInit"))
            {
                SetSource(target, PreviewSource.CharaInit);
                DisplayCharaInitList(target);
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Individual Parts##characterPreviewLoadout"))
            {
                SetSource(target, PreviewSource.Loadout);
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

    private void DisplayLoadout(CharacterPreviewTarget target)
    {
        GUI.SimpleHeader("Slots", "Choose the part model for each slot. Empty slots show the bare body.");

        var isFemale = _loadout.IsFemale;

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
