using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Modifier edits. Modifiers are held in two places only (BehaviorSurvey modsurvey, ER / NR / DS3): a modifier
/// generator's m_modifier (one), and a modifier list's m_modifiers (in run order). Every vanilla modifier is named, and
/// 9 per game are shared. Adding, duplicating and deleting work on lists; a generator's own modifier can't be deleted.
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region Modifiers
    /// <summary>
    /// The modifier lists (hkbModifierList.m_modifiers) holding a modifier, and whether a modifier generator holds it
    /// </summary>
    public static (List<object> Lists, object Generator) FindModifierHolders(HavokBehaviorSymbols symbols, object modifier)
    {
        var lists = new List<object>();
        object generator = null;

        foreach (var parent in symbols.GetParents(modifier))
        {
            if (GetList(parent, "m_modifiers") is IList list && list.Cast<object>().Any(m => ReferenceEquals(m, modifier)))
                lists.Add(parent);

            if (ReferenceEquals(HavokBehaviorSymbols.GetValue(parent, "m_modifier"), modifier))
                generator = parent;
        }

        return (lists, generator);
    }

    /// <summary>
    /// A copy of a modifier, right after it in its list (its children copied too; the copy is named uniquely)
    /// </summary>
    public static EditorAction DuplicateModifier(HavokBehaviorContext context, object modifier, out object newModifier, out string error)
    {
        newModifier = null;
        error = null;

        var (lists, _) = FindModifierHolders(context.Symbols, modifier);

        if (lists.Count == 0)
        {
            error = LOC.Get("HAVOK_Modifiers_Error_Not_In_List");
            return null;
        }

        var list = GetList(lists[0], "m_modifiers");
        var index = IndexOfReference(list, modifier);
        var copy = HavokBehaviorCloner.Clone(modifier, context.Symbols, keepShared: true);

        HavokBehaviorSymbols.GetField(copy.GetType(), "m_name")?.SetValue(copy,
            UniqueObjectName(context.Symbols, copy.GetType(), HavokBehaviorSymbols.GetValue(modifier, "m_name") as string ?? ""));

        newModifier = copy;

        var at = index + 1;

        return Wrap(context, new List<EditorAction> { Step(context, () => list.Insert(at, copy), () => list.RemoveAt(at)) });
    }

    /// <summary>
    /// Removes a modifier from every list holding it. Refused when a modifier generator holds it directly (the
    /// generator would be left without a modifier).
    /// </summary>
    public static EditorAction DeleteModifier(HavokBehaviorContext context, object modifier, out string error)
    {
        error = null;

        var (lists, generator) = FindModifierHolders(context.Symbols, modifier);

        if (generator != null)
        {
            error = LOC.Get("HAVOK_Modifiers_Error_Generator_Holds", HavokBehaviorSymbols.GetOutlineLabel(generator));
            return null;
        }

        if (lists.Count == 0)
        {
            error = LOC.Get("HAVOK_Modifiers_Error_Not_In_List");
            return null;
        }

        var steps = new List<EditorAction>();

        foreach (var holder in lists)
        {
            var list = GetList(holder, "m_modifiers");
            var index = IndexOfReference(list, modifier);

            if (index < 0)
                continue;

            var at = index;
            steps.Add(Step(context, () => list.RemoveAt(at), () => list.Insert(at, modifier)));
        }

        return Wrap(context, steps);
    }

    /// <summary>
    /// Adds a modifier of the template's type at the end of a modifier list: a copy of the template (an existing
    /// modifier of that type, so its settings are ones the game uses), named uniquely
    /// </summary>
    public static EditorAction AddModifier(HavokBehaviorContext context, object modifierList, object template, out object newModifier, out string error)
    {
        newModifier = null;
        error = null;

        var list = GetList(modifierList, "m_modifiers");

        if (list == null || template == null)
        {
            error = LOC.Get("HAVOK_Modifiers_Error_Not_In_List");
            return null;
        }

        var copy = HavokBehaviorCloner.Clone(template, context.Symbols, keepShared: true);

        HavokBehaviorSymbols.GetField(copy.GetType(), "m_name")?.SetValue(copy,
            UniqueObjectName(context.Symbols, copy.GetType(), HavokBehaviorSymbols.GetValue(template, "m_name") as string ?? copy.GetType().Name));

        newModifier = copy;

        return Wrap(context, new List<EditorAction> { AddToList(context, list, copy) });
    }

    /// <summary>
    /// One modifier of each type in the file (the first found), to add copies of
    /// </summary>
    public static List<object> GetModifierTemplates(HavokBehaviorSymbols symbols)
    {
        return symbols.Modifiers
            .GroupBy(m => m.GetType())
            .Select(g => g.First())
            .OrderBy(m => m.GetType().Name, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>Whether the modifier runs (m_enable), or null if it has no such field</summary>
    public static bool? IsModifierEnabled(object modifier)
        => HavokBehaviorSymbols.GetValue(modifier, "m_enable") is bool enabled ? enabled : null;
    #endregion
}
