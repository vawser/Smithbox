using StudioCore.Editors.Common;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// One field of a copied generator that names an event, variable or character property, by its name in the source
/// file (null if it named nothing), and which copy of that name it is (DS3 and SDT repeat event names: 0 is the first)
/// </summary>
public record HavokSnapshotRef(object Owner, FieldInfo Field, HavokIndexRole Role, string Name, int Occurrence = 0);

/// <summary>
/// A selector, state machine or clip as copied, for pasting into another behavior file: a copy sharing nothing with
/// the source, the names its event / variable / character property fields refer to, and the variables it uses.
/// Taken at Copy, so later edits to the source don't change what's pasted.
/// </summary>
public class HavokGeneratorSnapshot
{
    public object Root { get; init; }
    public string Label { get; init; }

    /// <summary>The Havok class set (Havok.hk2018 ...): a paste needs the same one</summary>
    public string ClassSet { get; init; }

    public List<HavokSnapshotRef> Refs { get; } = new();

    public Dictionary<string, HavokBehaviorOperations.HavokVariableSnapshot> Variables { get; } = new(StringComparer.Ordinal);
}

/// <summary>
/// What a paste into another file would add: the events and variables the target lacks (offered, ticked), the
/// character properties it lacks (their bindings are dropped), and the animation names it would add
/// </summary>
public record HavokCrossPastePlan(List<string> MissingEvents, List<string> MissingVariables, List<string> MissingCharacterProperties,
    int AnimationsToAdd, int EffectsReused, string Error);

/// <summary>
/// Cross-file paste: see Claude/Documentation/Havok Behavior Cross-File Paste Plan.md
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region Cross-file paste
    private static readonly Regex CrossClipAnimationRegex = new(@"^a(\d+)_(\d+)$", RegexOptions.IgnoreCase);

    private static string ClassSet(object obj) => obj?.GetType().Namespace?.Split('.').Take(2).Aggregate((a, b) => $"{a}.{b}") ?? "";

    /// <summary>
    /// Takes the snapshot of a selector, state machine or clip in the shown file
    /// </summary>
    public static HavokGeneratorSnapshot SnapshotGenerator(ProjectEntry project, HavokBehaviorSymbols symbols, object obj)
    {
        var root = HavokBehaviorCloner.CloneAll(obj, out var copies);

        var snapshot = new HavokGeneratorSnapshot
        {
            Root = root,
            Label = HavokBehaviorSymbols.GetOutlineLabel(obj),
            ClassSet = ClassSet(obj)
        };

        foreach (var (original, copy) in copies)
        {
            if (original is IList || original.GetType().IsArray)
                continue;

            var type = original.GetType();
            var meta = HavokMetaHelper.GetMeta(project, type);

            if (meta == null)
                continue;

            foreach (var fieldMeta in meta.Fields)
            {
                var role = HavokMetaHelper.GetIndexRole(meta, fieldMeta.Field);

                if (role is not (HavokIndexRole.Event or HavokIndexRole.Variable))
                    continue;

                var field = HavokBehaviorSymbols.GetField(type, fieldMeta.Field);

                if (field == null)
                    continue;

                if (role is HavokIndexRole.Variable && HavokBehaviorSymbols.IsCharacterPropertyBinding(original))
                    role = HavokIndexRole.CharacterProperty;

                var index = ToInt(field.GetValue(original));
                var names = symbols.GetNames(role);
                var name = HavokBehaviorSymbols.GetName(names, index);

                snapshot.Refs.Add(new(copy, field, role, name, name == null ? 0 : HavokBehaviorSymbols.OccurrenceOf(names, index)));

                if (role is HavokIndexRole.Variable && name != null && !snapshot.Variables.ContainsKey(name))
                {
                    var variable = SnapshotVariable(symbols, index);

                    if (variable != null)
                        snapshot.Variables[name] = variable;
                }
            }
        }

        return snapshot;
    }

    private static int IndexOfName(IList names, string name)
    {
        if (names == null || name == null)
            return -1;

        for (var i = 0; i < names.Count; i++)
        {
            if ((names[i] as string ?? "") == name)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Every object reachable from a root (a copy not yet in any file)
    /// </summary>
    private static List<object> Reachable(object root)
    {
        var result = new List<object>();
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<object>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var obj = stack.Pop();

            if (obj == null || obj is string || obj.GetType().IsValueType || !seen.Add(obj))
                continue;

            if (obj is IList list)
            {
                foreach (var entry in list)
                    stack.Push(entry);

                continue;
            }

            result.Add(obj);

            foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                    continue;

                stack.Push(field.GetValue(obj));
            }
        }

        return result;
    }

    private static bool IsTransitionEffectType(Type type)
    {
        for (var t = type; t != null; t = t.BaseType)
        {
            if (t.Name == "hkbTransitionEffect")
                return true;
        }

        return false;
    }

    /// <summary>
    /// What pasting the snapshots into the target file would add, or why it can't
    /// </summary>
    public static HavokCrossPastePlan PlanCrossPaste(HavokBehaviorSymbols target, IReadOnlyList<HavokGeneratorSnapshot> snapshots)
    {
        var targetSet = ClassSet(target.StringData);

        if (snapshots.Any(s => s.ClassSet != targetSet))
            return new(new(), new(), new(), 0, 0, LOC.Get("HAVOK_CrossPaste_Error_Version", snapshots[0].ClassSet, targetSet));

        List<string> Missing(HavokIndexRole role) => snapshots.SelectMany(s => s.Refs)
            .Where(r => r.Role == role && r.Name != null && IndexOfName(target.GetNames(role), r.Name) < 0)
            .Select(r => r.Name).Distinct().OrderBy(n => n, StringComparer.Ordinal).ToList();

        // Animations the target's clips don't play yet (each would add an animation name, where the file has them)
        var targetAnimations = target.Clips.Select(c => HavokBehaviorSymbols.GetValue(c, "m_animationName") as string)
            .Where(n => n != null).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var animations = snapshots.SelectMany(s => Reachable(s.Root))
            .Where(o => o.GetType().Name == "hkbClipGenerator")
            .Select(c => HavokBehaviorSymbols.GetValue(c, "m_animationName") as string)
            .Where(n => n != null && !targetAnimations.Contains(n))
            .Distinct(StringComparer.OrdinalIgnoreCase).Count();

        var effects = snapshots.SelectMany(s => Reachable(s.Root))
            .Where(o => IsTransitionEffectType(o.GetType()))
            .Count(e => FindTargetEffect(target, e) != null);

        return new(Missing(HavokIndexRole.Event), Missing(HavokIndexRole.Variable), Missing(HavokIndexRole.CharacterProperty),
            animations, effects, null);
    }

    private static object FindTargetEffect(HavokBehaviorSymbols target, object effect)
    {
        var name = HavokBehaviorSymbols.GetValue(effect, "m_name") as string;

        if (string.IsNullOrEmpty(name))
            return null;

        return target.Objects.FirstOrDefault(o => o.GetType() == effect.GetType() &&
            (HavokBehaviorSymbols.GetValue(o, "m_name") as string) == name);
    }

    /// <summary>
    /// Pastes the snapshots into the shown file as one undo step: the ticked missing events and variables first,
    /// then each object, remapped by name, placed where the target is (a clip into the target selector; a selector or
    /// state machine where the target selector or state machine is held)
    /// </summary>
    public static EditorAction CrossPaste(HavokBehaviorContext context, IReadOnlyList<HavokGeneratorSnapshot> snapshots, object target,
        IReadOnlySet<string> addEvents, IReadOnlySet<string> addVariables, List<object> created, out List<string> errors)
    {
        var builders = new List<Func<(EditorAction, string)>>();

        foreach (var name in addEvents)
            builders.Add(() => (AddEvent(context, name, out _, out var e), e));

        foreach (var name in addVariables)
        {
            var variable = snapshots.Select(s => s.Variables.GetValueOrDefault(name)).FirstOrDefault(v => v != null);

            if (variable != null)
                builders.Add(() => (PasteVariable(context, variable, out _, out var e), e));
        }

        foreach (var snapshot in snapshots)
            builders.Add(() => BuildCrossPaste(context, snapshot, target, created));

        return Sequence(context, builders, out errors);
    }

    private static (EditorAction, string) BuildCrossPaste(HavokBehaviorContext context, HavokGeneratorSnapshot snapshot, object target, List<object> created)
    {
        var symbols = context.Symbols;
        var copy = HavokBehaviorCloner.CloneAll(snapshot.Root, out var copies);
        var label = snapshot.Label;

        // References: the target's index for each name; a missing variable or character property drops its binding
        var dropBindings = new HashSet<object>(ReferenceEqualityComparer.Instance);

        foreach (var reference in snapshot.Refs)
        {
            if (copies.GetValueOrDefault(reference.Owner) is not { } owner)
                continue;

            var index = HavokBehaviorSymbols.IndexOfName(symbols.GetNames(reference.Role), reference.Name, reference.Occurrence);

            if (index < 0 && reference.Role is HavokIndexRole.Variable or HavokIndexRole.CharacterProperty &&
                HavokBehaviorSymbols.GetField(owner.GetType(), "m_memberPath") != null)
            {
                dropBindings.Add(owner);
                continue;
            }

            reference.Field.SetValue(owner, Convert.ChangeType(index, reference.Field.FieldType));
        }

        var objects = Reachable(copy);

        if (dropBindings.Count > 0)
            DropBindings(objects, dropBindings);

        // Transition effects: the target's of the same type and name, where it has one
        foreach (var obj in objects)
        {
            foreach (var field in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                    continue;

                var value = field.GetValue(obj);

                if (value != null && IsTransitionEffectType(value.GetType()) && FindTargetEffect(symbols, value) is { } existing)
                    field.SetValue(obj, existing);
            }
        }

        var steps = new List<EditorAction>();
        var addedNames = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // A pasted clip: into the target selector, for its category (or the next free one), at the selector's ID
        if (copy.GetType().Name == "hkbClipGenerator")
        {
            var selector = target?.GetType().Name switch
            {
                "CustomManualSelectorGenerator" => target,
                "hkbClipGenerator" => GetClipSelectors(symbols, target).FirstOrDefault(),
                _ => null
            };

            if (selector == null)
                return (null, LOC.Get("HAVOK_BehaviorActions_Error_Paste_Clip_Target", label));

            var animId = Convert.ToInt64(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? 0);
            var category = GetClipCategory(copy);
            var generators = GetList(selector, "m_generators");

            if (category < 0 || generators?.Cast<object>().Any(g => g != null && GetClipCategory(g) == category) == true)
                category = GetFreeCategory(selector);

            if (!SetClipCategory(copy, category, out var setError))
                return (null, $"{label}: {setError}");

            if (!RetargetClip(context, copy, animId, addedNames, steps, out var retargetError))
                return (null, $"{label}: {retargetError}");

            steps.Add(AddToList(context, generators, copy));
            created.Add(copy);

            return (Wrap(context, steps), null);
        }

        // The clips below: the target's internal ID for each animation (or a new animation name)
        foreach (var clip in objects.Where(o => o.GetType().Name == "hkbClipGenerator"))
        {
            var match = CrossClipAnimationRegex.Match(HavokBehaviorSymbols.GetValue(clip, "m_animationName") as string ?? "");

            if (!match.Success)
                continue;

            if (!RetargetClip(context, clip, long.Parse(match.Groups[2].Value), addedNames, steps, out var clipError))
                return (null, $"{label}: {clipError}");
        }

        // A selector or state machine: where the target (of either kind) is held
        if (target == null || target.GetType().Name is not ("CustomManualSelectorGenerator" or "hkbStateMachine"))
            return (null, LOC.Get("HAVOK_BehaviorActions_Error_Paste_Generator_Target", label));

        var placement = FindPlacements(symbols, target).FirstOrDefault(p => p.Kind is not PlacementKind.Unsupported);

        if (placement == null)
            return (null, LOC.Get("HAVOK_BehaviorActions_Cannot_Duplicate", HavokBehaviorSymbols.GetOutlineLabel(target)));

        var place = PlaceGenerator(context, copy, placement, out var placeError);

        if (place == null)
            return (null, $"{label}: {placeError}");

        steps.AddRange(place);
        created.Add(copy);

        return (Wrap(context, steps), null);
    }

    /// <summary>
    /// A clip's category set before it's retargeted: its animation name (and its name, where it holds the animation)
    /// </summary>
    private static bool SetClipCategory(object clip, long category, out string error)
    {
        error = null;

        var animation = HavokBehaviorSymbols.GetValue(clip, "m_animationName") as string ?? "";
        var match = CrossClipAnimationRegex.Match(animation);

        if (!match.Success)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Clip_Animation_Name", animation);
            return false;
        }

        var renamed = $"a{category.ToString().PadLeft(match.Groups[1].Value.Length, '0')}_{match.Groups[2].Value}";
        var name = HavokBehaviorSymbols.GetValue(clip, "m_name") as string ?? "";

        HavokBehaviorSymbols.GetField(clip.GetType(), "m_animationName").SetValue(clip, renamed);
        HavokBehaviorSymbols.GetField(clip.GetType(), "m_name")?.SetValue(clip,
            name.Contains(animation, StringComparison.OrdinalIgnoreCase)
                ? Regex.Replace(name, Regex.Escape(animation), renamed, RegexOptions.IgnoreCase)
                : renamed);

        return true;
    }

    /// <summary>
    /// Removes bindings from the binding sets of a copy; a set left empty is removed from its owner (the game's
    /// files have no empty sets), and m_indexOfBindingToEnable follows the removal
    /// </summary>
    private static void DropBindings(List<object> objects, HashSet<object> drop)
    {
        foreach (var set in objects.Where(o => o.GetType().Name == "hkbVariableBindingSet").ToList())
        {
            if (HavokBehaviorSymbols.GetBindings(set) is not IList bindings)
                continue;

            var enableField = HavokBehaviorSymbols.GetField(set.GetType(), "m_indexOfBindingToEnable");
            var enable = enableField == null ? -1 : ToInt(enableField.GetValue(set));
            var enableBinding = enable >= 0 && enable < bindings.Count ? bindings[enable] : null;

            for (var i = bindings.Count - 1; i >= 0; i--)
            {
                if (bindings[i] != null && drop.Contains(bindings[i]))
                    bindings.RemoveAt(i);
            }

            if (enableField != null)
            {
                var newEnable = enableBinding == null ? -1 : bindings.IndexOf(enableBinding);
                enableField.SetValue(set, Convert.ChangeType(newEnable, enableField.FieldType));
            }

            if (bindings.Count > 0)
                continue;

            foreach (var owner in objects)
            {
                var field = HavokBehaviorSymbols.GetField(owner.GetType(), "m_variableBindingSet");

                if (field != null && ReferenceEquals(field.GetValue(owner), set))
                    field.SetValue(owner, null);
            }
        }
    }
    #endregion
}
