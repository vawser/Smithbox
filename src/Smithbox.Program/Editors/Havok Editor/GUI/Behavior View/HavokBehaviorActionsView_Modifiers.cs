using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The modifiers' right-click menu (Modifiers category and the Outline's modifier trees): enable / disable,
/// duplicate, delete, add a modifier to a list, and show it in the Outline. Each edit is one undo step, applied at once.
/// </summary>
public partial class HavokBehaviorActionsView
{
    public void DisplayModifierItems(object modifier)
    {
        var symbols = Owner.Symbols;

        if (symbols == null || modifier == null)
            return;

        // Enable / Disable
        if (HavokBehaviorOperations.IsModifierEnabled(modifier) is bool enabled)
        {
            if (ImGui.MenuItem(LOC.Get(enabled ? "HAVOK_Modifiers_Disable" : "HAVOK_Modifiers_Enable")))
            {
                var field = HavokBehaviorSymbols.GetField(modifier.GetType(), "m_enable");
                View.ActionManager.ExecuteAction(new HavokChangeField(View.Selection.SelectedFileWrapper, field, modifier, !enabled));
            }

            GUI.Tooltip(LOC.Get("HAVOK_Modifiers_Enable_TT"));
        }

        var (lists, generator) = HavokBehaviorOperations.FindModifierHolders(symbols, modifier);

        // Duplicate: in its list
        if (ImGui.MenuItem(LOC.Get("HAVOK_Modifiers_Duplicate"), InputManager.GetHint(KeybindID.Duplicate), false, lists.Count > 0))
            DuplicateModifier(modifier);

        TooltipWhenDisabled(lists.Count > 0 ? LOC.Get("HAVOK_Modifiers_Duplicate_TT") : LOC.Get("HAVOK_Modifiers_Error_Not_In_List"));

        // Delete: from its lists, not from a generator
        var canDelete = generator == null && lists.Count > 0;

        if (ImGui.MenuItem(LOC.Get("HAVOK_Modifiers_Delete"), InputManager.GetHint(KeybindID.Delete), false, canDelete))
            DeleteModifier(modifier);

        TooltipWhenDisabled(canDelete
            ? LOC.Get("HAVOK_Modifiers_Delete_TT")
            : generator != null
                ? LOC.Get("HAVOK_Modifiers_Error_Generator_Holds", HavokBehaviorSymbols.GetOutlineLabel(generator))
                : LOC.Get("HAVOK_Modifiers_Error_Not_In_List"));

        // Add to a list: a copy of an existing modifier of the chosen type
        if (modifier.GetType().Name == "hkbModifierList" && ImGui.BeginMenu(LOC.Get("HAVOK_Modifiers_Add")))
        {
            foreach (var template in HavokBehaviorOperations.GetModifierTemplates(symbols))
            {
                if (ImGui.MenuItem(template.GetType().Name))
                    AddModifier(modifier, template);

                GUI.Tooltip(LOC.Get("HAVOK_Modifiers_Add_TT", HavokBehaviorSymbols.GetValue(template, "m_name") as string ?? ""));
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("HAVOK_Modifiers_Show_In_Outline")))
            Owner.OutlineView.ShowModifier(modifier);

        if (ImGui.MenuItem(LOC.Get("HAVOK_BehaviorActions_Copy_Name")))
            ImGui.SetClipboardText(HavokBehaviorSymbols.GetValue(modifier, "m_name") as string ?? "");
    }

    private static void TooltipWhenDisabled(string text)
    {
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(text);
    }

    public void DuplicateModifier(object modifier)
    {
        var action = HavokBehaviorOperations.DuplicateModifier(Context, modifier, out var copy, out var error);

        if (action == null)
        {
            Smithbox.LogError(this, error);
            return;
        }

        View.ActionManager.ExecuteAction(action);
        Owner.ModifierView.Selected = copy;
    }

    public void DeleteModifier(object modifier)
    {
        var action = HavokBehaviorOperations.DeleteModifier(Context, modifier, out var error);

        if (action == null)
        {
            Smithbox.LogError(this, error);
            return;
        }

        View.ActionManager.ExecuteAction(action);
        Owner.ModifierView.Selected = null;
    }

    private void AddModifier(object modifierList, object template)
    {
        var action = HavokBehaviorOperations.AddModifier(Context, modifierList, template, out var copy, out var error);

        if (action == null)
        {
            Smithbox.LogError(this, error);
            return;
        }

        View.ActionManager.ExecuteAction(action);
        Owner.ModifierView.Selected = copy;
    }
}
