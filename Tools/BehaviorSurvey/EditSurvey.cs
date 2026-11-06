using System.Collections;
using System.Reflection;

// editsurvey: the facts the Havok Editor's duplicate / delete / bind edits rely on
//   - what holds CMSGs and state machines (list entry or single field), and how often they're shared
//   - what a state machine's subtree shares with the rest of the file (what Duplicate would keep shared)
//   - binding set layout: owners, sharing, empty sets, indexOfBindingToEnable, hasOutputBinding, bit index
//   - which members are bound on clip generators
//   - every other int field holding a variable index (what Delete Variable has to re-index)
class EditSurvey
{
    public readonly SortedDictionary<string, long> Counts = new();
    public readonly SortedDictionary<string, List<string>> Examples = new();

    void Count(string key, long n = 1) => Counts[key] = Counts.GetValueOrDefault(key) + n;

    void Example(string key, string value, int max = 10)
    {
        if (!Examples.TryGetValue(key, out var list))
            Examples[key] = list = new List<string>();
        if (list.Count < max && !list.Contains(value))
            list.Add(value);
    }

    public void Merge(EditSurvey other)
    {
        foreach (var (k, v) in other.Counts) Count(k, v);
        foreach (var (k, l) in other.Examples) foreach (var e in l) Example(k, e, 20);
    }

    static string Name(object o) => Survey.Get(o, "m_name") as string ?? "";

    static bool IsTransitionEffect(Type t)
    {
        for (var x = t; x != null; x = x.BaseType)
            if (x.Name == "hkbTransitionEffect") return true;
        return false;
    }

    public void Run(object root)
    {
        var all = Survey.CollectAll(root);

        // object -> (parent, field, isList)
        var parents = new Dictionary<object, List<(object P, string F, bool L)>>(ReferenceEqualityComparer.Instance);

        foreach (var o in all)
        {
            foreach (var f in Survey.Fields(o.GetType()))
            {
                if (f.FieldType == typeof(string) || f.FieldType.IsValueType) continue;
                var v = f.GetValue(o);
                if (v is IList l)
                {
                    foreach (var e in l)
                        if (e != null && e.GetType().IsClass && e is not string)
                            Add(parents, e, (o, f.Name, true));
                }
                else if (v != null && v is not string && v is not Array)
                {
                    Add(parents, v, (o, f.Name, false));
                }
            }
        }

        string Describe((object P, string F, bool L) p) => $"{p.P.GetType().Name}.{p.F}{(p.L ? "[]" : "")}";

        // Holders of CMSGs and state machines
        foreach (var o in all)
        {
            var tn = o.GetType().Name;

            if (tn is not ("CustomManualSelectorGenerator" or "hkbStateMachine" or "hkbClipGenerator"))
                continue;

            var ps = parents.GetValueOrDefault(o) ?? new();
            var tag = tn == "hkbStateMachine" ? "sm" : tn == "hkbClipGenerator" ? "clip" : "cmsg";

            Count($"{tag} total");
            Count($"{tag} parent count {Math.Min(ps.Count, 5)}{(ps.Count >= 5 ? "+" : "")}");

            foreach (var p in ps)
            {
                Count($"{tag} held by {Describe(p)}");

                // One step further up, for single-field holders (StateInfo.m_generator: where is the state held?)
                if (!p.L)
                    foreach (var pp in parents.GetValueOrDefault(p.P) ?? new())
                        Count($"{tag} held by {Describe(p)} <- {Describe(pp)}");
            }

            if (tag == "cmsg")
            {
                // How many of the CMSG's clips are shared outside it
                if (Survey.Get(o, "m_generators") is IList gens)
                {
                    foreach (var g in gens)
                    {
                        if (g == null) { Count("cmsg generator null"); continue; }
                        Count($"cmsg generator type {g.GetType().Name}");
                        var gp = parents.GetValueOrDefault(g) ?? new();
                        if (gp.Count > 1) Count("cmsg generator shared with others");
                    }

                    // Clip order and uniqueness within a CMSG (does Duplicate need a new category?)
                    var animNames = gens.Cast<object>().Where(g => g?.GetType().Name == "hkbClipGenerator")
                        .Select(g => (Survey.Get(g, "m_animationName") as string ?? "").ToLowerInvariant()).ToList();
                    Count($"cmsg clip animation names unique {animNames.Distinct().Count() == animNames.Count}");
                    Count($"cmsg clips in name order {animNames.SequenceEqual(animNames.OrderBy(n => n, StringComparer.Ordinal))}");
                    var clipNames = gens.Cast<object>().Where(g => g?.GetType().Name == "hkbClipGenerator")
                        .Select(g => Survey.Get(g, "m_name") as string ?? "").ToList();
                    Count($"cmsg clip names unique {clipNames.Distinct().Count() == clipNames.Count}");
                }
            }
            else if (tag == "sm")
            {
                SurveyStateMachineSubtree(o, parents);
            }
        }

        // Binding sets
        foreach (var o in all)
        {
            var tn = o.GetType().Name;

            if (tn == "hkbVariableBindingSet")
            {
                var ps = parents.GetValueOrDefault(o) ?? new();
                Count($"bindingset parent count {Math.Min(ps.Count, 3)}");
                foreach (var p in ps) Count($"bindingset held by {Describe(p)}");

                var bindings = Survey.Get(o, "m_bindings") as IList;
                Count($"bindingset bindings {(bindings == null ? "null" : bindings.Count == 0 ? "0" : bindings.Count < 4 ? bindings.Count.ToString() : "4+")}");

                var enable = Survey.GetInt(o, "m_indexOfBindingToEnable");
                Count($"bindingset indexOfBindingToEnable {(enable < 0 ? enable.ToString() : "index")}");

                if (enable >= 0 && bindings != null && enable < bindings.Count)
                    Example("bindingset enable path", Survey.Get(bindings[enable], "m_memberPath") as string ?? "");

                var hasOutput = Survey.Get(o, "m_hasOutputBinding");
                Count($"bindingset hasOutputBinding {hasOutput}");

                if (hasOutput is true && bindings != null)
                {
                    // Is hasOutputBinding = any binding has the output flag?
                    var anyOutput = bindings.Cast<object>().Any(b => (Convert.ToInt32(Survey.Get(b, "m_flags") ?? 0) & 2) != 0);
                    Count($"bindingset hasOutputBinding true, a binding has flag 2: {anyOutput}");
                }

                foreach (var f in Survey.Fields(o.GetType()))
                    Count($"bindingset field {f.Name}:{f.FieldType.Name}");

                if (bindings == null) continue;

                foreach (var b in bindings)
                {
                    if (b == null) { Count("binding null"); continue; }

                    foreach (var f in Survey.Fields(b.GetType()))
                    {
                        Count($"binding field {f.Name}:{f.FieldType.Name}");

                        if (f.FieldType.IsValueType && f.Name is not "m_variableIndex" and not "m_offset")
                            Count($"binding value {f.Name}={f.GetValue(b)}");
                    }

                    // Bindings on clip generators: what's bound
                    foreach (var p in ps)
                    {
                        var ptn = p.P.GetType().Name;
                        if (ptn is "hkbClipGenerator" or "CustomManualSelectorGenerator")
                            Count($"bound member on {ptn}: {Survey.Get(b, "m_memberPath")} type {Survey.Get(b, "m_bindingType")} bit {Survey.Get(b, "m_bitIndex")}");
                    }
                }
            }
        }

        // Fields that hold a variable index, other than bindings
        foreach (var o in all)
        {
            var tn = o.GetType().Name;

            foreach (var f in Survey.Fields(o.GetType()))
            {
                if (!f.FieldType.IsPrimitive) continue;
                if (!f.Name.Contains("ariable", StringComparison.Ordinal)) continue;
                if (tn.Contains("Binding") && f.Name == "m_variableIndex") continue;

                var v = Convert.ToInt64(f.GetValue(o));
                Count($"var field {tn}.{f.Name} {(v == -1 ? "-1" : "set")}");
                if (v != -1) Example($"var field {tn}.{f.Name}", v.ToString());
            }

            if (tn.Contains("Expression", StringComparison.Ordinal))
            {
                Count($"expression type {tn}");
                foreach (var f in Survey.Fields(o.GetType()))
                    Count($"expression field {tn}.{f.Name}:{f.FieldType.Name}");
                if (Survey.Get(o, "m_expression") is string expr && expr != "")
                    Example($"expression text {tn}", expr, 6);
            }

            if (tn is "hkbVariableInfo")
                Count($"variable type {Survey.Get(o, "m_type")}");
        }
    }

    // What a duplicate of the state machine would share: objects in its subtree with a parent outside it
    void SurveyStateMachineSubtree(object sm, Dictionary<object, List<(object P, string F, bool L)>> parents)
    {
        var subtree = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var stack = new Stack<object>();
        stack.Push(sm);

        while (stack.Count > 0)
        {
            var o = stack.Pop();
            if (o == null || !subtree.Add(o)) continue;
            if (!ReferenceEquals(o, sm) && IsTransitionEffect(o.GetType())) continue;

            foreach (var f in Survey.Fields(o.GetType()))
            {
                if (f.FieldType == typeof(string) || f.FieldType.IsValueType) continue;
                var v = f.GetValue(o);
                if (v is IList l) { foreach (var e in l) if (e != null && e.GetType().IsClass && e is not string) stack.Push(e); }
                else if (v != null && v is not string && v is not Array) stack.Push(v);
            }
        }

        Count($"sm subtree size {(subtree.Count < 50 ? "<50" : subtree.Count < 1000 ? "<1000" : subtree.Count < 10000 ? "<10000" : "10000+")}");

        var sharedTypes = new HashSet<string>();
        foreach (var o in subtree)
        {
            if (ReferenceEquals(o, sm) || IsTransitionEffect(o.GetType())) continue;
            var ps = parents.GetValueOrDefault(o) ?? new();
            if (ps.Any(p => !subtree.Contains(p.P)))
            {
                Count($"sm subtree shared-out object {o.GetType().Name}");
                sharedTypes.Add(o.GetType().Name);
            }
        }

        Count($"sm with shared-out objects {sharedTypes.Count > 0}");

        // Does the machine have a name unique in the file? (Duplicate needs a name)
        Example("sm names", Name(sm), 15);
    }

    static void Add(Dictionary<object, List<(object, string, bool)>> map, object key, (object, string, bool) value)
    {
        if (!map.TryGetValue(key, out var list)) map[key] = list = new();
        list.Add(value);
    }

    public void Print()
    {
        foreach (var (k, v) in Counts)
            Console.WriteLine($"  {k}: {v}");
        foreach (var (k, l) in Examples)
            Console.WriteLine($"  e.g. {k}: {string.Join(" | ", l)}");
    }
}
