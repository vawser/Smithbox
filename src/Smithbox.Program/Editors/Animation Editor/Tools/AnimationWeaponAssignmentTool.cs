#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Weapon Assignment tool (Viewport category; W7, c0000 only): a weapon model for each hand, held where its
/// EquipParamWeapon row's WepAbsorpPosParam row puts it in that hand, so arm edits can be judged against the weapon.
/// The viewport makes the character again on a change. The time act's weapon moves (sheathing, two-handing) aren't
/// followed here; the Time Act Editor shows those.
/// </summary>
public class AnimationWeaponAssignmentTool
{
    private readonly AnimationEditorView View;

    private readonly Dictionary<string, string?> Names = new();
    private Dictionary<string, string>? Aliases;

    /// <summary>A hand's list: its filter, the models it matches (made again when the filter changes), and whether
    /// the list still has to scroll to the held model</summary>
    private sealed class HandList
    {
        public string Filter = "";
        public List<string>? Shown;
        public string? ShownFor;
        public bool ScrollToChosen = true;
    }

    private readonly HandList RightList = new();
    private readonly HandList LeftList = new();

    // Rows the list shows at once
    private const int ListRows = 8;

    public AnimationWeaponAssignmentTool(AnimationEditorView view)
    {
        View = view;
    }

    /// <summary>As a tool of the Tools window</summary>
    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("ANIM_Tool_Weapon_Assignment")}##animationWeaponAssignmentToolHeader"))
            return;

        ImGui.BeginChild("AnimationWeaponAssignmentToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("ANIM_Weapons_Menu_TT"));

        if (!CharacterComposition.IsComposite(View.Bank.Character?.Name))
        {
            GUI.Spacer();
            ImGui.TextDisabled(LOC.Get("ANIM_Weapons_Composite_Only"));
            return;
        }

        Picker("ANIM_Weapons_Right_Hand", ref CFG.Current.Interface.Interface_AnimationEditor_Right_Weapon, "Right", RightList);
        Picker("ANIM_Weapons_Left_Hand", ref CFG.Current.Interface.Interface_AnimationEditor_Left_Weapon, "Left", LeftList);
    }

    private string? NameOf(string model)
    {
        // Looked up once per model: each look-up goes through the weapon param
        if (!Names.TryGetValue(model, out var name))
        {
            name = CharacterComposition.GetWeaponName(View.Project, model);
            Names[model] = name;
        }
        return name;
    }

    private string? AliasOf(string model)
    {
        // The project's part aliases, read into a look-up once
        if (Aliases == null)
        {
            Aliases = new(StringComparer.OrdinalIgnoreCase);
            if (View.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Parts, out var aliases))
            {
                foreach (var alias in aliases)
                {
                    if (!string.IsNullOrEmpty(alias.Name))
                        Aliases.TryAdd(alias.ID, alias.Name);
                }
            }
        }
        return Aliases.TryGetValue(model, out var name) ? name : null;
    }

    private string LabelOf(string model)
    {
        if (model.Length == 0)
            return LOC.Get("ANIM_Weapons_None");

        var label = model;

        var alias = AliasOf(model);
        if (alias != null)
            label = $"{label} <{alias}>";

        // The weapon param's name, unless the alias already says the same
        var name = NameOf(model);
        if (name != null && !string.Equals(name, alias, StringComparison.OrdinalIgnoreCase))
            label = $"{label}  {name}";

        return label;
    }

    /// <summary>
    /// A hand: its header, what it holds, a filter (model, alias or weapon name) and an eight-row list of the
    /// matching models, None first. Clicking a row holds that model.
    /// </summary>
    private void Picker(string labelKey, ref string chosen, string id, HandList list)
    {
        AnimationToolLayout.Group(LOC.Get(labelKey), LOC.Get("ANIM_Weapons_Hand_TT"));
        ImGui.TextDisabled(LOC.Get("ANIM_Weapons_Holding", LabelOf(chosen)));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint($"##animationWeaponFilter{id}", LOC.Get("ANIM_Filter_Hint"), ref list.Filter, 64);
        GUI.Tooltip(LOC.Get("ANIM_Weapons_Filter_TT"));

        if (list.Shown == null || list.ShownFor != list.Filter)
        {
            var filter = list.Filter;
            list.Shown = new List<string> { "" };
            foreach (var model in View.ViewportWindow.WeaponModels())
            {
                if (filter.Length > 0 && !model.Contains(filter, StringComparison.OrdinalIgnoreCase) &&
                    NameOf(model)?.Contains(filter, StringComparison.OrdinalIgnoreCase) != true &&
                    AliasOf(model)?.Contains(filter, StringComparison.OrdinalIgnoreCase) != true)
                    continue;

                list.Shown.Add(model);
            }

            list.ShownFor = filter;
        }

        var shown = list.Shown;
        var height = ImGui.GetTextLineHeightWithSpacing() * ListRows + ImGui.GetStyle().FramePadding.Y * 2;
        if (!ImGui.BeginListBox($"##animationWeaponList{id}", new System.Numerics.Vector2(-1, height)))
            return;

        var chosenIndex = shown.IndexOf(chosen);
        var clipper = new ImGuiListClipper();
        clipper.Begin(shown.Count);
        if (list.ScrollToChosen && chosenIndex >= 0)
            clipper.IncludeItemByIndex(chosenIndex);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var model = shown[i];
                var selected = chosen == model;
                if (ImGui.Selectable($"{LabelOf(model)}##animationWeapon{id}_{i}", selected))
                    chosen = model;

                if (selected && list.ScrollToChosen)
                {
                    ImGui.SetScrollHereY();
                    list.ScrollToChosen = false;
                }
            }
        }

        clipper.End();

        // A held model the filter hides isn't scrolled to later
        if (chosenIndex < 0)
            list.ScrollToChosen = false;

        ImGui.EndListBox();
    }
}
