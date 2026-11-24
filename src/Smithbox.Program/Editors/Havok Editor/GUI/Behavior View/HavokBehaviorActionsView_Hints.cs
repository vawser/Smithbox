using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Free-ID hints for the dialogs that take an ID: which clip categories a selector already plays (a selector's clip
/// animations are unique in the game's files: all but 2 of 256,664 ER selectors), which state IDs a machine uses,
/// and how many selectors already have an animation ID
/// </summary>
public partial class HavokBehaviorActionsView
{
    /// <summary>
    /// The categories a selector's clips play, with the clip for each (aCCC_NNNNNN: CCC)
    /// </summary>
    private static SortedDictionary<long, object> GetSelectorCategories(object selector)
    {
        var result = new SortedDictionary<long, object>();

        if (HavokBehaviorSymbols.GetValue(selector, "m_generators") is not IList generators)
            return result;

        foreach (var generator in generators)
        {
            if (generator?.GetType().Name != "hkbClipGenerator")
                continue;

            var category = HavokBehaviorOperations.GetClipCategory(generator);

            if (category >= 0)
                result.TryAdd(category, generator);
        }

        return result;
    }

    /// <summary>
    /// Under a category input: the categories the selector already has, and a warning (with the next free one to
    /// use) when the chosen one is taken
    /// </summary>
    private void DisplayCategoryHint(object selector, ref int category)
    {
        var used = GetSelectorCategories(selector);

        if (used.Count > 0)
        {
            var text = string.Join(", ", used.Keys.Take(24)) + (used.Count > 24 ? " …" : "");
            ImGui.TextDisabled(LOC.Get("HAVOK_Hint_Categories_Used", used.Count, text));
            GUI.Tooltip(LOC.Get("HAVOK_Hint_Categories_Used_TT"));
        }

        if (!used.TryGetValue(category, out var clip))
            return;

        var free = (int)HavokBehaviorOperations.GetFreeCategory(selector);

        ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color,
            LOC.Get("HAVOK_Hint_Category_Taken", category, HavokBehaviorSymbols.GetValue(clip, "m_name") as string ?? ""));

        ImGui.SameLine();

        if (ImGui.SmallButton($"{LOC.Get("HAVOK_Hint_Use", free)}##useFreeCategory"))
            category = free;

        GUI.Tooltip(LOC.Get("HAVOK_Hint_Use_Category_TT"));
    }

    /// <summary>
    /// The state IDs a machine uses, beside the ID a new state gets
    /// </summary>
    private static void DisplayStateIdHint(object stateMachine)
    {
        var states = HavokBehaviorSymbols.GetStates(stateMachine);

        if (states.Count == 0)
            return;

        ImGui.TextDisabled(LOC.Get("HAVOK_Hint_State_IDs", states.Count, states.Max(s => s.StateId)));
        GUI.Tooltip(LOC.Get("HAVOK_Hint_State_IDs_TT"));
    }

    /// <summary>
    /// How many other selectors already have an animation ID (the new selector would play the same animations)
    /// </summary>
    private void DisplayAnimIdHint(long animId, object self)
    {
        var others = Owner.Symbols.Selectors.Count(s => !ReferenceEquals(s, self) &&
            HavokBehaviorSymbols.GetValue(s, "m_animId") is { } v && Convert.ToInt64(v) == animId);

        if (others > 0)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_NewAction_Anim_Id_Used", others));
    }
}
