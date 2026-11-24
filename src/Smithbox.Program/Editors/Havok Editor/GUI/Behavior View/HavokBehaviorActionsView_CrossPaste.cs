using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Paste into another behavior file: an Apply-only dialog listing what the target lacks (events and variables,
/// ticked to be created; character properties, whose bindings are dropped; animation names to add). See
/// Claude/Documentation/Behavior.md (Havok behavior edits).
/// </summary>
public partial class HavokBehaviorActionsView
{
    private object CrossPasteTarget;
    private HavokCrossPastePlan CrossPastePlan;
    private readonly HashSet<string> CrossPasteEvents = new(StringComparer.Ordinal);
    private readonly HashSet<string> CrossPasteVariables = new(StringComparer.Ordinal);

    /// <summary>
    /// Opens the dialog for pasting the clipboard's snapshots into the shown file at a target
    /// </summary>
    private void RequestCrossPaste(object target, Source source)
    {
        CrossPasteTarget = target;
        RequestSource = source;
        CrossPastePlan = HavokBehaviorOperations.PlanCrossPaste(Owner.Symbols, Clipboard.Snapshots);

        // Every missing event and variable ticked to be created
        CrossPasteEvents.Clear();
        CrossPasteVariables.Clear();
        CrossPasteEvents.UnionWith(CrossPastePlan.MissingEvents);
        CrossPasteVariables.UnionWith(CrossPastePlan.MissingVariables);

        Request(Dialog.CrossPaste);
    }

    private void DisplayCrossPaste(out bool canApply)
    {
        var plan = CrossPastePlan;
        canApply = plan != null && plan.Error == null;

        if (plan == null)
            return;

        ImGui.Text(LOC.Get("HAVOK_CrossPaste_Source", Clipboard.SourceLabel, string.Join(", ", Clipboard.Snapshots.Select(s => s.Label))));
        ImGui.Text(LOC.Get("HAVOK_CrossPaste_Target", CrossPasteTarget == null ? "-" : HavokBehaviorSymbols.GetOutlineLabel(CrossPasteTarget)));

        if (plan.Error != null)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, plan.Error);
            return;
        }

        // Events
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_CrossPaste_Events_Header", plan.MissingEvents.Count), LOC.Get("HAVOK_CrossPaste_Events_Header_TT"));
        DisplayTickList("crossPasteEvents", plan.MissingEvents, CrossPasteEvents, "HAVOK_CrossPaste_None_Missing");

        // Variables
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_CrossPaste_Variables_Header", plan.MissingVariables.Count), LOC.Get("HAVOK_CrossPaste_Variables_Header_TT"));
        DisplayTickList("crossPasteVariables", plan.MissingVariables, CrossPasteVariables, "HAVOK_CrossPaste_None_Missing");

        // What else happens
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_CrossPaste_Notes_Header"), LOC.Get("HAVOK_CrossPaste_Notes_Header_TT"));

        if (plan.MissingCharacterProperties.Count > 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_CrossPaste_Character_Properties", plan.MissingCharacterProperties.Count));
            GUI.Tooltip(string.Join("\n", plan.MissingCharacterProperties));
        }

        ImGui.BulletText(LOC.Get("HAVOK_CrossPaste_Animations", plan.AnimationsToAdd));
        ImGui.BulletText(LOC.Get("HAVOK_CrossPaste_Effects", plan.EffectsReused));
        ImGui.TextDisabled(LOC.Get("HAVOK_CrossPaste_Unticked_Note"));
    }

    /// <summary>
    /// The missing names, each with a tick; All / None for a long list
    /// </summary>
    private static void DisplayTickList(string id, List<string> names, HashSet<string> ticked, string noneKey)
    {
        if (names.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get(noneKey));
            return;
        }

        if (ImGui.SmallButton($"{LOC.Get("HAVOK_CrossPaste_All")}##{id}_all"))
            ticked.UnionWith(names);
        ImGui.SameLine();
        if (ImGui.SmallButton($"{LOC.Get("HAVOK_CrossPaste_None")}##{id}_none"))
            ticked.Clear();

        var height = Math.Min(names.Count, 8) * ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().WindowPadding.Y * 2;

        ImGui.BeginChild($"{id}_list", new System.Numerics.Vector2(0, height), ImGuiChildFlags.Borders);

        foreach (var name in names)
        {
            var on = ticked.Contains(name);

            if (ImGui.Checkbox($"{name}##{id}_{name}", ref on))
            {
                if (on)
                    ticked.Add(name);
                else
                    ticked.Remove(name);
            }
        }

        ImGui.EndChild();
    }

    private EditorAction ApplyCrossPaste(HavokBehaviorContext context, out object show, out string error)
    {
        show = null;
        error = null;

        var created = new List<object>();
        var action = HavokBehaviorOperations.CrossPaste(context, Clipboard.Snapshots, CrossPasteTarget,
            CrossPasteEvents, CrossPasteVariables, created, out var errors);

        // Parts that couldn't be pasted are logged; the rest is pasted
        foreach (var e in errors)
            Smithbox.LogError(this, e);

        if (action == null)
        {
            error = errors.FirstOrDefault() ?? LOC.Get("HAVOK_CrossPaste_Nothing");
            return null;
        }

        show = created.FirstOrDefault();

        Smithbox.Log(this, LOC.Get("HAVOK_CrossPaste_Done", created.Count, CrossPasteEvents.Count, CrossPasteVariables.Count));

        return action;
    }
}
