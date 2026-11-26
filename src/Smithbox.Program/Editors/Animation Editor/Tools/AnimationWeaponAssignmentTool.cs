#nullable enable
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
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

    private string Filter = "";
    private readonly Dictionary<string, string?> Names = new();

    // The first matches only: a list of thousands of rows is no use
    private const int MaxShown = 200;

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

        GUI.Spacer();
        Picker("ANIM_Weapons_Right_Hand", ref CFG.Current.Interface.Interface_AnimationEditor_Right_Weapon, "Right");
        Picker("ANIM_Weapons_Left_Hand", ref CFG.Current.Interface.Interface_AnimationEditor_Left_Weapon, "Left");
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

    private string LabelOf(string model)
    {
        if (model.Length == 0)
            return LOC.Get("ANIM_Weapons_None");

        var name = NameOf(model);
        return name != null ? $"{model}  {name}" : model;
    }

    private void Picker(string labelKey, ref string chosen, string id)
    {
        ImGui.Text(LOC.Get(labelKey));
        ImGui.SetNextItemWidth(-1);
        if (!ImGui.BeginCombo($"##animationWeapon{id}", LabelOf(chosen), ImGuiComboFlags.HeightLarge))
            return;

        // The filter is typed into straight away, as the list opens
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint($"##animationWeaponFilter{id}", LOC.Get("ANIM_Filter_Hint"), ref Filter, 64);

        if (ImGui.Selectable($"{LOC.Get("ANIM_Weapons_None")}##animationWeaponNone{id}", chosen.Length == 0))
            chosen = "";

        var shown = 0;
        foreach (var model in View.ViewportWindow.WeaponModels())
        {
            var name = NameOf(model);
            if (Filter.Length > 0 && !model.Contains(Filter, StringComparison.OrdinalIgnoreCase) &&
                name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) != true)
                continue;

            if (++shown > MaxShown)
            {
                ImGui.TextDisabled(LOC.Get("ANIM_Weapons_More"));
                break;
            }

            var selected = chosen == model;
            if (ImGui.Selectable($"{LabelOf(model)}##animationWeapon{id}_{model}", selected))
                chosen = model;

            if (selected && ImGui.IsWindowAppearing())
                ImGui.SetItemDefaultFocus();
        }

        ImGui.EndCombo();
    }
}
