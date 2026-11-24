using StudioCore.Editors.Common;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Plans a State template (Claude/Documentation/Behavior.md (Havok behavior edits)): new states in a machine, each a copy
/// of a template state playing one new animation, with copies of the template's wildcards on renamed events.
///
/// The template state is one of the machine whose name matches the template's TemplateMatch, whose number is its
/// selector's animation ID (vanilla ER: Attack3000 plays 3000, but Attack3110 plays 3000 too), with a wildcard into it.
/// </summary>
public static class HavokStateTemplatePlanner
{
    public class Candidate
    {
        public object State;
        public string Name;

        /// <summary>The ID in its names (its selector's animation ID)</summary>
        public string IdText;

        public List<string> Events = new();
        public List<long> Categories = new();
    }

    public enum RowStatus
    {
        Add,
        /// <summary>The machine already has a state of the name</summary>
        Exists
    }

    public class Row
    {
        public long Id;
        public string StateName;
        public string AnimationName;
        public List<string> Events = new();
        public List<string> ExistingEvents = new();
        public RowStatus Status;

        /// <summary>Selectors of the machine that already play the ID</summary>
        public int IdUsers;

        public bool Include;
    }

    public static object FindMachine(HavokBehaviorSymbols symbols, string name) =>
        symbols?.Objects.FirstOrDefault(o => o.GetType().Name == "hkbStateMachine" &&
            string.Equals(HavokBehaviorSymbols.GetValue(o, "m_name") as string, name, StringComparison.Ordinal));

    /// <summary>The states that can be the template, in the machine's order</summary>
    public static List<Candidate> FindCandidates(HavokBehaviorSymbols symbols, HavokBehaviorTemplate template, object machine)
    {
        var result = new List<Candidate>();

        if (machine == null || HavokBehaviorSymbols.GetValue(machine, "m_states") is not System.Collections.IList states)
            return result;

        var regex = new Regex(template.TemplateMatch);

        foreach (var state in states)
        {
            if (state == null)
                continue;

            var name = HavokBehaviorSymbols.GetValue(state, "m_name") as string ?? "";
            var match = regex.Match(name);

            if (!match.Success || match.Groups.Count < 2)
                continue;

            var selector = HavokBehaviorOperations.FindStateSelector(symbols, state, out _, out _);

            if (selector == null)
                continue;

            var idText = match.Groups[1].Value;
            var animId = Convert.ToInt64(HavokBehaviorSymbols.GetValue(selector, "m_animId") ?? -1);

            if (!long.TryParse(idText, out var id) || id != animId)
                continue;

            var stateId = Convert.ToInt32(HavokBehaviorSymbols.GetValue(state, "m_stateId") ?? -1);
            var wildcards = HavokBehaviorOperations.FindIncomingWildcards(machine, stateId);

            if (wildcards.Count == 0)
                continue;

            var candidate = new Candidate { State = state, Name = name, IdText = idText };

            foreach (var w in wildcards)
            {
                var index = Convert.ToInt32(HavokBehaviorSymbols.GetValue(w, "m_eventId") ?? -1);
                candidate.Events.Add(HavokBehaviorSymbols.GetName(symbols.EventNames, index) ?? $"#{index}");
            }

            if (HavokBehaviorSymbols.GetValue(selector, "m_generators") is System.Collections.IList generators)
            {
                foreach (var g in generators)
                {
                    if (g?.GetType().Name == "hkbClipGenerator" && HavokBehaviorOperations.GetClipCategory(g) is var c and >= 0 && !candidate.Categories.Contains(c))
                        candidate.Categories.Add(c);
                }
            }

            if (candidate.Categories.Count > 0)
                result.Add(candidate);
        }

        return result;
    }

    /// <summary>The default template: the first candidate with a clip of the category, else the first</summary>
    public static int DefaultCandidate(List<Candidate> candidates, long category)
    {
        var index = candidates.FindIndex(c => c.Categories.Contains(category));
        return index >= 0 ? index : 0;
    }

    public static List<Row> Plan(HavokBehaviorSymbols symbols, object machine, Candidate candidate, long firstId, int count, int step,
        long category, int categorySize)
    {
        var rows = new List<Row>();

        if (machine == null || candidate == null)
            return rows;

        var stateNames = new HashSet<string>(HavokBehaviorSymbols.GetStates(machine).Select(s => s.Name), StringComparer.Ordinal);
        var eventNames = new HashSet<string>((symbols.EventNames?.Cast<object>() ?? Enumerable.Empty<object>()).Select(n => n as string ?? ""), StringComparer.Ordinal);
        var machineIds = symbols.Selectors
            .Where(s => symbols.FindOwningStateMachine(s) == machine)
            .Select(s => Convert.ToInt64(HavokBehaviorSymbols.GetValue(s, "m_animId") ?? -1))
            .GroupBy(i => i)
            .ToDictionary(g => g.Key, g => g.Count());

        for (var i = 0; i < count; i++)
        {
            var id = firstId + (long)i * step;
            var name = HavokBehaviorOperations.ReplaceTemplateId(candidate.Name, candidate.IdText, id) ?? $"{candidate.Name}_{id}";

            var row = new Row
            {
                Id = id,
                StateName = name,
                AnimationName = HavokTemplatePlanner.AnimationName(category, id, categorySize),
                Status = stateNames.Contains(name) ? RowStatus.Exists : RowStatus.Add,
                IdUsers = machineIds.GetValueOrDefault(id),
            };

            foreach (var e in candidate.Events)
            {
                var eventName = HavokBehaviorOperations.ReplaceTemplateId(e, candidate.IdText, id) ?? e;
                row.Events.Add(eventName);

                if (eventNames.Contains(eventName))
                    row.ExistingEvents.Add(eventName);
            }

            row.Include = row.Status is RowStatus.Add;
            rows.Add(row);
        }

        return rows;
    }

    public static EditorAction Apply(HavokBehaviorContext context, Candidate candidate, IEnumerable<Row> rows, long category,
        out int added, out List<string> errors)
    {
        var builders = new List<Func<(EditorAction, string)>>();
        var included = rows.Where(r => r.Include && r.Status is RowStatus.Add).ToList();

        foreach (var row in included)
            builders.AddRange(HavokBehaviorOperations.NewTemplateStateSteps(context, candidate.State, candidate.IdText, row.Id, category, row.StateName));

        var result = HavokBehaviorOperations.Sequence(context, builders, out errors);

        added = included.Count;

        return result;
    }
}
