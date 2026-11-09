using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    public partial class FXR
    {
        /// <summary>
        /// A new, empty FXR laid out like the game's own files: header version and minor, and the
        /// empty-list offset style most vanilla files of that game use (Phase 2 survey).
        /// The root node starts with no actions; the caller adds the root actions the game expects.
        /// </summary>
        public static FXR Create(FxrGame game, int id)
        {
            return new FXR
            {
                Id = id,
                Minor = (short)(game == FxrGame.NR ? 1 : 0),
                Version = game == FxrGame.DS3 ? FXRVersion.DarkSouls3 : FXRVersion.Sekiro,
                ZeroEmptyOffsets = DefaultEmptyOffsets(game),
                States = new List<FxrState> { new FxrState() },
                Root = new FxrNode { Type = 2000 },
            };
        }

        /// <summary>The most common vanilla empty-list style per game.</summary>
        public static FxrEmptyOffsets DefaultEmptyOffsets(FxrGame game) => game switch
        {
            // DS3/SDT/ER store the write position for empty action lists and property modifier lists
            FxrGame.DS3 or FxrGame.SDT or FxrGame.ER => FxrEmptyOffsets.All
                & ~(FxrEmptyOffsets.ActionFields | FxrEmptyOffsets.ActionProperties | FxrEmptyOffsets.ActionSection10s | FxrEmptyOffsets.PropertyModifiers),
            // AC6/NR store 0 for everything except empty modifier property lists
            _ => FxrEmptyOffsets.All & ~FxrEmptyOffsets.ModifierProperties,
        };

        /// <summary>
        /// The version 5 header lists as fxr-main builds them (getReferences): Proxy node effect ids,
        /// external values read by conditions and ExternalValue modifiers, and ExternalValue2 values.
        /// Vanilla SDT/ER/AC6 match this exactly; 10 NR files also list 10000 in the first list.
        /// </summary>
        public (List<int> Sfx, List<int> External1, List<int> External2) ComputeReferences()
        {
            var sfx = new SortedSet<int>();
            var ext1 = new SortedSet<int>();
            var ext2 = new SortedSet<int>();

            foreach (var node in Walk(Root))
            {
                if (node.Type == 2001)
                    foreach (var a in node.Actions)
                        if (a.Type == 132 && a.Fields1.Count > 0) sfx.Add((int)a.Fields1[0]);

                foreach (var a in node.Actions.Concat(node.Configs.SelectMany(c => c.Actions)))
                    foreach (var p in a.Properties1.Concat(a.Properties2))
                        foreach (var m in p.Modifiers)
                        {
                            if (m.Fields.Count == 0) continue;
                            int ev = (int)m.Fields[0];
                            if (m.ModifierType == 38) ext1.Add(ev);
                            else if (m.ModifierType == 39)
                            {
                                if (ev != 10000) ext1.Add(ev);
                                ext2.Add(ev);
                            }
                        }
            }
            foreach (var state in States)
                foreach (var c in state.Conditions)
                {
                    if (c.Left.Type == -3 && c.Left.HasValue) ext1.Add(c.Left.IntValue);
                    if (c.Right.Type == -3 && c.Right.HasValue) ext1.Add(c.Right.IntValue);
                }
            return (sfx.ToList(), ext1.ToList(), ext2.ToList());
        }

        /// <summary>Replaces the version 5 header lists with <see cref="ComputeReferences"/>. Call after edits.</summary>
        public void RebuildReferences()
        {
            var (sfx, ext1, ext2) = ComputeReferences();
            SfxReferences = sfx;
            ExternalValues1 = ext1;
            ExternalValues2 = ext2;
        }

        /// <summary>Every node, depth-first pre-order.</summary>
        public static IEnumerable<FxrNode> Walk(FxrNode root)
        {
            var stack = new Stack<FxrNode>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;
                for (int i = node.Children.Count - 1; i >= 0; i--) stack.Push(node.Children[i]);
            }
        }

        /// <summary>Every action (node actions, then config actions, per node in pre-order).</summary>
        public IEnumerable<FxrAction> AllActions()
        {
            foreach (var node in Walk(Root))
            {
                foreach (var a in node.Actions) yield return a;
                foreach (var c in node.Configs)
                    foreach (var a in c.Actions) yield return a;
            }
        }
    }

    public record FxrResource(string Kind, int Id, FxrAction Action, string Name, string TextureType);

    public static class FxrResources
    {
        /// <summary>
        /// Textures, models, animation binders and sounds the effect uses, from schema values tagged
        /// with a resource kind. Property values count when the property is constant.
        /// </summary>
        public static List<FxrResource> Collect(FXR fxr, FxrSchema schema, FxrGame game)
        {
            var list = new List<FxrResource>();
            foreach (var action in fxr.AllActions())
            {
                var view = new FxrActionView(schema, action, game);
                if (!view.IsTyped) continue;
                foreach (var name in view.Names)
                {
                    var def = view.Describe(name);
                    if (def?.Resource == null || !view.IsStored(name)) continue;
                    switch (view.KindOf(name))
                    {
                        case FxrValueKind.Field:
                            list.Add(new FxrResource(def.Resource, view.GetInt(name), action, name, def.TextureType));
                            break;
                        case FxrValueKind.Property:
                            var curve = FxrCurve.Decode(view.GetProperty(name));
                            if (curve.Constant != null)
                                list.Add(new FxrResource(def.Resource, (int)Math.Round(curve.Constant[0]), action, name, def.TextureType));
                            break;
                    }
                }
            }
            return list;
        }
    }
}
