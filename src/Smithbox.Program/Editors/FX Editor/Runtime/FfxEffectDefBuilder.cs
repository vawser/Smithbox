using SoulsFormats;
using StudioCore.Editors.FxEditor.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The older formats (DES, DS2, DS2S: DLSE; DS1, DS1R, BB: FXR1) read into the runtime's definitions (FFX plan §20).
/// Works on the format-neutral model (IFxEffect) with the FFX schema: action values by their schema / ActionData
/// names, node-type slots by the schema's <c>nodeTypes</c> names (§20.8).
///
/// - The effect's states become one config per state on each node it lists (StateMap); <see cref="FxEffectDef.StateLogic"/>
///   evaluates the exits. A state holding only a5 ends the effect.
/// - An effect call is a node. Its template id gives its layout: an emitter slot makes a particle node (periodic
///   emitter: delay, duration, emission time / interval / count), an appearance without an emitter one particle for
///   the node's duration, an interval without either a node emitter, the LOD templates a levels-of-detail node; any
///   other a group. A call to an effect that isn't a template (no params) is a Proxy.
/// - Emitters (a28–a33, a45, a117), movements (a55, a84, a105) and billboard appearances (a27, a59, a70, a71) map to
///   the FXR3 runtime's shapes, spreads, speeds, gravity and billboards. Their less certain choices are marked
///   "provisional" below; what isn't mapped is counted in <see cref="FxEffectDef.Unmapped"/>.
/// </summary>
public static class FfxEffectDefBuilder
{
    // The LOD templates (§20.1): five children and four distance thresholds
    private static readonly HashSet<int> LodTemplates = [2116, 2117, 2118, 2119];

    // Child-list actions: their calls are the node's children (§20.1)
    private static readonly HashSet<int> ChildLists = [14, 17, 79, 87, 95];

    public static FxEffectDef Build(IFxEffect effect, FfxSchema schema, FfxGame game)
    {
        var b = new Builder(schema, game);
        return b.Effect(effect);
    }

    private sealed class Builder
    {
        private readonly FfxSchema schema;
        private readonly FfxGame game;
        private Dictionary<string, int> unmapped;
        private int nodes;

        public Builder(FfxSchema schema, FfxGame game) { this.schema = schema; this.game = game; }

        private void Note(string reason) => unmapped[reason] = unmapped.GetValueOrDefault(reason) + 1;

        public FxEffectDef Effect(IFxEffect effect)
        {
            unmapped = new();
            // FXR1 node 134 (several roots, DS1R / BB): each root has its own states, so each plays as an effect of its
            // own, started by a Proxy node of the outer one; the outer effect ends when all of them have (§20.14)
            var roots = effect.Root is { Kind: StudioCore.Editors.FxEditor.Model.FxNodeKind.Roots } multi
                ? multi.Children.Where(c => c is IFxStatesNode).ToList() : null;
            if (roots is not { Count: > 1 })
            {
                var single = Part(effect.Id, effect.States, effect.Root?.Source);
                single.Unmapped = unmapped;
                return single;
            }

            var outer = new FxEffectDef
            {
                Id = effect.Id,
                Unmapped = unmapped,
                Root = new FxNodeDef { Kind = FxNodeKind.Root, Source = effect.Root.Source },
            };
            foreach (var r in roots)
            {
                var part = Part(effect.Id, ((IFxStatesNode)r).States, r.Source);
                part.Unmapped = unmapped;
                outer.Root.Children.Add(new FxNodeDef { Kind = FxNodeKind.Proxy, ProxyDef = part, Source = r.Source });
            }
            // made on the first update; each ends when its effect has finished
            outer.StateLogic = inst => inst.Root.ChildrenMade
                && inst.Root.Children.All(c => c.Expired || c.Proxy is { Finished: true }) ? -1 : null;
            return outer;
        }

        /// <summary>One root's states and the nodes they list, as an effect.</summary>
        private FxEffectDef Part(int id, IReadOnlyList<IFxState> stateList, object source)
        {
            var d = new FxEffectDef { Id = id };
            int states = Math.Max(stateList.Count, 1);
            d.Root = new FxNodeDef { Kind = FxNodeKind.Root, Source = source };

            for (int s = 0; s < stateList.Count; s++)
                foreach (var action in stateList[s].Actions)
                    foreach (var child in ChildrenOf(action))
                    {
                        // a node listed by a state runs in that state only
                        var map = Enumerable.Repeat(-1, states).ToList();
                        map[s] = 0;
                        child.StateMap = map;
                        d.Root.Children.Add(child);
                    }

            d.StateLogic = StateLogic(stateList);
            return d;
        }

        // ---------------------------------------------------------------- state machine

        private Func<FxEffectInstance, int?> StateLogic(IReadOnlyList<IFxState> stateList)
        {
            if (stateList.Count == 0)
                return _ => null;
            var exits = stateList.Select(s => s.Transitions.ToList()).ToList();
            var ends = stateList.Select(s => s.Transitions.Count == 0 && s.Actions.Count > 0 && s.Actions.All(a => a.TypeId == 5)).ToList();

            return inst =>
            {
                int s = inst.State;
                if (s < 0 || s >= exits.Count)
                    return null;
                if (ends[s])
                    return -1;
                foreach (var t in exits[s])
                    if (Eval(t.Condition, inst) != 0)
                        return t.TargetState;
                return null;
            };
        }

        /// <summary>
        /// An exit condition at the effect level. Ticks are seconds (the model's display units): CurrentTick is the
        /// time in the state, TotalTick the effect's time. Arguments read 0 (a top-level effect has none). The preview
        /// effect always has a parent (as one started by a character does), so "!ParentExists" exits don't fire.
        /// </summary>
        private static double Eval(IFxExpression e, FxEffectInstance inst)
        {
            if (e == null)
                return 0;
            var o = e.Operands;
            switch (e.Kind)
            {
                case FxExpressionKind.Constant: return e.Value ?? 0;
                case FxExpressionKind.StateTime:
                case FxExpressionKind.CurrentTick: return inst.StateTime;
                case FxExpressionKind.TotalTick: return inst.Time;
                case FxExpressionKind.External: return inst.External(e.Index);
                case FxExpressionKind.ArgumentA:
                case FxExpressionKind.ArgumentB: return 0;
                case FxExpressionKind.ParentExists: return 1;
                // the exits run before the root's first update has made its children; the game runs a state's actions
                // (a14 making the children) before testing its exits, so a child exists until they're made (§20.14)
                case FxExpressionKind.ChildExists: return !inst.Root.ChildrenMade || inst.Root.Children.Any(c => !c.Dead) ? 1 : 0;
                case FxExpressionKind.EmittersStopped: return inst.Root.ChildrenMade && inst.Root.Children.All(c => c.Dead) ? 1 : 0;
                case FxExpressionKind.DistanceFromCamera: return Vector3.Distance(inst.Context.CameraPosition, inst.World.Translation);
                case FxExpressionKind.Not: return o.Count > 0 && Eval(o[0], inst) == 0 ? 1 : 0;
                case FxExpressionKind.And: return o.Count == 2 && Eval(o[0], inst) != 0 && Eval(o[1], inst) != 0 ? 1 : 0;
                case FxExpressionKind.Or: return o.Count == 2 && (Eval(o[0], inst) != 0 || Eval(o[1], inst) != 0) ? 1 : 0;
                case FxExpressionKind.Compare:
                {
                    if (o.Count != 2) return 0;
                    double l = Eval(o[0], inst), r = Eval(o[1], inst);
                    return e.Operator switch
                    {
                        "==" => l == r ? 1 : 0,
                        "!=" => l != r ? 1 : 0,
                        "<" => l < r ? 1 : 0,
                        "<=" => l <= r ? 1 : 0,
                        ">" => l > r ? 1 : 0,
                        ">=" => l >= r ? 1 : 0,
                        _ => 0,
                    };
                }
                default: return 0;
            }
        }

        // ---------------------------------------------------------------- nodes

        /// <summary>The nodes an action holds: its effect calls, and the effect calls of the child lists it calls.</summary>
        private IEnumerable<FxNodeDef> ChildrenOf(IFxAction action)
        {
            foreach (var call in action.Calls)
            {
                if (call is IFxProxy)
                {
                    if (Node(call) is { } n) yield return n;
                }
                else if (ChildLists.Contains(call.TypeId))
                {
                    foreach (var n in ChildrenOf(call)) yield return n;
                }
            }
        }

        private FxNodeDef Node(IFxAction call)
        {
            if (++nodes > 4000)
            {
                Note("node limit");
                return null;
            }
            int id = ((IFxProxy)call).TargetEffect;
            var values = call.Values;
            if (values.Count == 0)
            {
                // an empty call (id 0) is nothing; another id plays that effect
                if (id <= 0) return null;
                return new FxNodeDef { Kind = FxNodeKind.Proxy, ProxyEffectId = id, Source = call.Source };
            }

            var layout = schema.NodeLayout(id, game);
            if (layout == null) Note($"node type {id} without a layout");
            string NameOf(int k) => layout != null && k < layout.Params.Count ? layout.Params[k].Name : null;
            int Slot(string name)
            {
                for (int k = 0; k < values.Count; k++)
                    if (string.Equals(NameOf(k), name, StringComparison.OrdinalIgnoreCase)) return k;
                return -1;
            }
            IFxValue At(string name) { int k = Slot(name); return k >= 0 ? values[k] : null; }
            // an a0 in a slot is an empty slot
            IFxAction CallAt(string name) => At(name) is IFxCallValue { Call: { TypeId: not 0 } c } ? c : null;

            var node = new FxNodeDef { Source = call.Source };
            var config = new FxConfigDef();
            node.Configs.Add(config);

            // children: the "children" slot's list, and (in group / LOD nodes) effect calls in any slot
            for (int k = 0; k < values.Count; k++)
            {
                if (values[k] is not IFxCallValue cv || cv.Call == null) continue;
                var c = cv.Call;
                if (c is IFxProxy) { if (Node(c) is { } n) node.Children.Add(n); }
                else if (ChildLists.Contains(c.TypeId)) node.Children.AddRange(ChildrenOf(c));
            }

            // timing
            config.Duration = Seconds(At("duration")) ?? FxValue.Constant(-1);
            config.Delay = Scalar(At("delay"), 0);
            config.EmissionTime = Seconds(At("emissionTime"));
            config.Interval = Seconds(At("emissionInterval"));
            config.TotalEmissions = Value(At("emissionCount"), FxArgument.ActiveTime);
            // the emit action's count (a56's second param, §20.11); one when the node type has none
            config.PerEmission = Value(At("perEmission"), FxArgument.ActiveTime) ?? FxValue.Constant(1);

            // transform: an a35 call (offset x/y/z, rotation x/y/z in degrees, provisional from the value ranges)
            for (int k = 0; k < values.Count; k++)
                if (values[k] is IFxCallValue { Call: { TypeId: 35 } t } && t.Values.Count >= 6)
                {
                    var v = t.Values.Select(x => (float)(x.Get()?.FirstOrDefault() ?? 0)).ToArray();
                    config.Offset = new Vector3(v[0], v[1], v[2]);
                    config.Rotation = new Vector3(v[3], v[4], v[5]);
                }

            var emitter = CallAt("emitter");
            var appearance = CallAt("appearance");
            var movement = CallAt("movement");

            if (LodTemplates.Contains(id))
            {
                node.Kind = FxNodeKind.LevelsOfDetail;
                // (provisional) the four rising ints after the five children are the distance thresholds (§20.1)
                var ints = values.Skip(5).Take(4).Select(x => (int)(x.Get()?.FirstOrDefault() ?? 10000)).ToList();
                config.LodThresholds = ints.Concat(Enumerable.Repeat(10000, 5)).Take(5).ToArray();
            }
            else if (emitter != null)
            {
                node.Kind = FxNodeKind.Basic;
                config.Emitter = FxEmitterKind.Periodic;
                Emitter(config, emitter);
                if (movement != null) Movement(config, movement);
                if (appearance != null) Appearance(config, appearance);
                else Note("particle node without an appearance");
            }
            else if (appearance != null)
            {
                // a single-appearance node: one particle, living as long as the node
                node.Kind = FxNodeKind.Basic;
                config.Emitter = FxEmitterKind.OneTime;
                if (movement != null) Movement(config, movement);
                Appearance(config, appearance);
                config.ParticleDuration ??= config.Duration;
            }
            else if (config.Interval != null && node.Children.Count > 0)
            {
                node.Kind = FxNodeKind.NodeEmitter;
                config.Emitter = FxEmitterKind.Periodic;
            }
            else
            {
                node.Kind = FxNodeKind.Basic;
            }

            return node;
        }

        // ---------------------------------------------------------------- actions

        /// <summary>An action's values by name: its own, then its ActionData's (FXR1), case-insensitive.</summary>
        private Dictionary<string, IFxValue> Named(IFxAction a)
        {
            var d = new Dictionary<string, IFxValue>(StringComparer.OrdinalIgnoreCase);
            foreach (var v in a.Values)
                if (v.Name != null) d.TryAdd(v.Name, v);
            // a15000 compiles exactly like a71 (DS2's exe, §20.6), and its slots line up with a71's type for type:
            // it's read through a71's names too, for the slots its own names leave out (duration, colour)
            if (a.TypeId == 15000 && schema.Layout(71, game) is { } a71)
                for (int i = 0; i < a.Values.Count && i < a71.Params.Count; i++)
                    if (!System.Text.RegularExpressions.Regex.IsMatch(a71.Params[i].Name ?? "unk", @"^(unk|texture|sound|effect|action|param)\d\d$"))
                        d.TryAdd(a71.Params[i].Name, a.Values[i]);
            foreach (var c in a.Calls)
                if (c is IFxPacked { Kind: FxPackedKind.ActionData })
                    foreach (var v in c.Values)
                        if (v.Name != null) d.TryAdd(v.Name, v);
            return d;
        }

        private void Emitter(FxConfigDef c, IFxAction a)
        {
            var n = Named(a);
            IFxValue V(string name) => n.GetValueOrDefault(name);
            switch (a.TypeId)
            {
                case 28: case 33: case 45: c.Shape = FxShapeKind.Point; break;
                case 29: c.Shape = FxShapeKind.Rectangle; c.ShapeSizeX = Value(V("emitterSize") ?? V("emitterSizeX"), FxArgument.ActiveTime); c.ShapeSizeY = c.ShapeSizeX; break;
                case 30: c.Shape = FxShapeKind.Disk; c.ShapeRadius = Value(V("emitterRadius"), FxArgument.ActiveTime); break;
                case 31: c.Shape = FxShapeKind.Sphere; c.ShapeRadius = Value(V("emitterRadius"), FxArgument.ActiveTime); c.EmitInside = true; break;
                case 32:
                    c.Shape = FxShapeKind.Box;
                    c.ShapeSizeX = Value(V("emitterSizeX"), FxArgument.ActiveTime);
                    c.ShapeSizeY = Value(V("emitterSizeY"), FxArgument.ActiveTime);
                    c.ShapeSizeZ = Value(V("emitterSizeZ"), FxArgument.ActiveTime);
                    break;
                default: c.Shape = FxShapeKind.Point; Note($"emitter a{a.TypeId}"); break;
            }

            // (provisional) the emitter's degree is the cone's half-angle, in degrees (DS2's cone code turns it into
            // radians: ÷ 90 × π/2); its speed is the particles' initial speed
            var degree = Value(V("emitterDegree"), FxArgument.ActiveTime);
            if (degree != null) { c.Spread = FxSpreadKind.Circular; c.SpreadAngle = degree; }
            var speed = Value(V("emitterSpeed"), FxArgument.ActiveTime);
            if (speed != null) { c.ParticleMove = FxParticleMoveKind.Speed; c.ParticleMoveSpeed = speed; }
        }

        private void Movement(FxConfigDef c, IFxAction a)
        {
            var n = Named(a);
            // (provisional) gravity pulls down, in the runtime's units, over the particle's age
            var g = Value(n.GetValueOrDefault("gravity"), FxArgument.ParticleAge);
            if (g != null) c.ParticleGravity = g;
            if (c.ParticleMove == FxParticleMoveKind.None) c.ParticleMove = FxParticleMoveKind.Speed;
            if (a.TypeId is not (55 or 84 or 105)) Note($"movement a{a.TypeId}");
        }

        private void Appearance(FxConfigDef c, IFxAction a)
        {
            var app = new FxAppearanceDef { Name = a.Name ?? $"a{a.TypeId}", IsParticle = true };
            c.Appearances.Add(app);
            var n = Named(a);
            if (a.TypeId is 3 or 61 or 108)
            {
                Model(c, app, a, n);
                return;
            }
            if (a.TypeId is 40 or 43 or 107 && ScreenOrTrail(c, app, a, n))
                return;
            if (Light(c, app, n))
                return;
            if (a.TypeId is not (27 or 59 or 70 or 71 or 15000 or 15001))
            {
                var t = a.Values.FirstOrDefault(v => v.Resource == "texture") ?? n.Values.FirstOrDefault(v => v.Resource == "texture");
                if (t != null && a.TypeId is 43 or 107)
                {
                    // a distortion / radial blur whose slots aren't named (DS2, DS2S, DES, BB): its texture is a normal
                    // map or mask, so it's drawn as the screen effect it is, not as a billboard of the texture (§20.15).
                    // Provisional: size 1, FXR3's defaults (normal-map distortion, intensity 1)
                    bool distortion = a.TypeId == 43;
                    app.Billboard = new FxBillboardDef
                    {
                        Kind = distortion ? FxBillboardKind.Distortion : FxBillboardKind.RadialBlur,
                        Texture = (int)(t.Get()?.FirstOrDefault() ?? 0),
                        Orientation = 1,
                        Width = FxValue.Constant(1),
                        Height = FxValue.Constant(1),
                        InterpolateFrames = false,
                    };
                    Note($"appearance a{a.TypeId} ({(distortion ? "distortion" : "radial blur")}, size unknown)");
                }
                else if (t != null && a.TypeId is not (108 or 61 or 3))
                {
                    // another appearance with a texture: drawn as a plain billboard of it (size 1, white), in its blend
                    // mode when a slot names it (DS2's a15002, §20.15)
                    app.Billboard = new FxBillboardDef
                    {
                        Kind = FxBillboardKind.BillboardEx,
                        Orientation = 1,
                        Texture = (int)(t.Get()?.FirstOrDefault() ?? 0),
                        BlendMode = (int)(n.GetValueOrDefault("blendMode")?.Get()?.FirstOrDefault() ?? 2),
                    };
                    TypedColours(a, app.Billboard);
                    Note($"appearance a{a.TypeId} (texture only)");
                }
                else Note($"appearance a{a.TypeId}");
                return;
            }
            IFxValue V(params string[] names) => names.Select(x => n.GetValueOrDefault(x)).FirstOrDefault(x => x != null);
            var bb = new FxBillboardDef
            {
                Kind = a.TypeId == 27 ? FxBillboardKind.PointSprite : FxBillboardKind.BillboardEx,
                Orientation = 1,
            };

            // the texture: a named texture id, else the first texture-typed value
            var tex = V("textureID", "TextureID", "TextureID1") ?? a.Values.FirstOrDefault(v => v.Resource == "texture")
                ?? n.Values.FirstOrDefault(v => v.Resource == "texture");
            bb.Texture = (int)(tex?.Get()?.FirstOrDefault() ?? 0);
            // (provisional) the same blend mode numbers as FXR3
            bb.BlendMode = (int)(V("blendMode")?.Get()?.FirstOrDefault() ?? 2);
            bb.Columns = Math.Max(1, (int)(V("animFrameSliceCountPerRow")?.Get()?.FirstOrDefault() ?? 1));
            bb.TotalFrames = Math.Max(1, (int)(V("animFrameTotalCount")?.Get()?.FirstOrDefault() ?? 1));
            bb.FrameIndex = Value(V("animFrameInitial"), FxArgument.ParticleAge);
            bb.FrameIndexOffset = Value(V("animFrameTimeline"), FxArgument.ParticleAge);

            // (provisional) size: scale 2 is the billboard's size over the particle's age, scale 1 a factor on it
            // a missing axis takes the other one's size (a15001 has scale 2's Y named, not its X)
            var width = Value(V("scale2X"), FxArgument.ParticleAge) ?? Value(V("scale1X", "scale"), FxArgument.ParticleAge);
            var height = Value(V("scale2Y"), FxArgument.ParticleAge) ?? Value(V("scale1Y", "scale"), FxArgument.ParticleAge);
            bb.Width = width ?? height ?? FxValue.Constant(1);
            bb.Height = height ?? bb.Width;
            if (V("scale2X") != null && V("scale1X") != null)
            {
                c.ParticleScaleX = Value(V("scale1X"), FxArgument.ParticleAge);
                c.ParticleScaleY = Value(V("scale1Y"), FxArgument.ParticleAge);
            }
            bb.Size = bb.Width;

            // colours: a whole colour value, or four channel fields (DS1 ActionData Color1R..A)
            bb.Color1 = Colour(n, "color1");
            bb.Color2 = Colour(n, "color2") ?? Colour(n, "color");
            bb.Color3 = Colour(n, "color3");
            TypedColours(a, bb);
            bb.RotationZ = Angle(V("rotZ"));
            var rs = Angle(V("rotSpeedZ"));
            if (rs != null) bb.AngularSpeed = new FxValue[] { null, null, null, null, rs, null };

            app.Billboard = bb;

            // the particle's life: the appearance's duration (seconds)
            c.ParticleDuration = Seconds(V("duration"));
        }

        /// <summary>
        /// (provisional, §20.12 / §20.16) An appearance whose own slots name no colour (DS2's a15000, a15001, a15002,
        /// a18000, a20002, a59, a70): its colours by value type. Every DS2 appearance, in every layout length, has an
        /// RGBA sequence (the colour, usually fading its alpha), then an RGBA2 (its variation), then exactly one more
        /// RGBA sequence, often above 1 ([10, 3, 1, 2.5], [1.5, 1.25, 1, 1]) and its only tint when the first is white:
        /// the first is the colour, the second the unclamped multiplier, FXR3's color3. The constant RGBAs after it
        /// (a15001 / a70's three) aren't used.
        /// </summary>
        private void TypedColours(IFxAction a, FxBillboardDef bb)
        {
            if (a.Values.Any(v => v.Name != null && v.Name.StartsWith("color", StringComparison.OrdinalIgnoreCase)))
                return;
            var sequences = a.Values.Where(v => v is IFxTypedValue { TypeName: { } tn } && tn.StartsWith("RGBA") && tn.Contains("Sequence")).ToList();
            if (sequences.Count == 0)
                return;
            if (bb.Color1 == null && bb.Color2 == null)
                bb.Color2 = Value(sequences[0], FxArgument.ParticleAge);
            if (bb.Color3 == null && sequences.Count >= 2)
                bb.Color3 = Value(sequences[1], FxArgument.ParticleAge);
        }

        /// <summary>
        /// Tracer (a40), distortion (a43) and radial blur (a107), from DS1's ActionData field names (FXR1 games; the
        /// DLSE games don't name these slots, so they fall back to a plain billboard of the texture). False when the
        /// fields aren't there. Provisional: width = scale 1 (tracer), segment time = TrailMaxTime, segments =
        /// TrailLength, intensity = DistortionIntensity, the modes numbered as FXR3's.
        /// </summary>
        private bool ScreenOrTrail(FxConfigDef c, FxAppearanceDef app, IFxAction a, Dictionary<string, IFxValue> n)
        {
            IFxValue V(params string[] names) => names.Select(x => n.GetValueOrDefault(x)).FirstOrDefault(x => x != null);
            int Int(string name, int fallback) => (int)(V(name)?.Get()?.FirstOrDefault() ?? fallback);
            float Float(string name, float fallback) => (float)(V(name)?.Get()?.FirstOrDefault() ?? fallback);

            switch (a.TypeId)
            {
                case 40 when V("TextureID") != null:
                {
                    var t = new FxTracerDef
                    {
                        Texture = Int("TextureID", 0),
                        BlendMode = Int("BlendMode", 2),
                        Width = Value(V("Scale1X", "Scale2X"), FxArgument.ParticleAge) ?? FxValue.Constant(1),
                        SegmentDuration = Math.Max(Float("TrailMaxTime", 1), 1f / 30),
                        ConcurrentSegments = Math.Clamp(Int("TrailLength", 100), 2, 1000),
                        Color1 = Colour(n, "color"),
                    };
                    app.Tracer = t;
                    c.Tracer ??= t;
                    c.ParticleDuration ??= Seconds(V("Duration"));
                    return true;
                }
                case 43 when V("TextureID1") != null:
                    app.Billboard = new FxBillboardDef
                    {
                        Kind = FxBillboardKind.Distortion,
                        Texture = Int("TextureID1", 0),
                        Layer2 = Int("TextureID2", 0),
                        DistortionMode = Math.Clamp(Int("DistortionMode", 1), 0, 4),
                        DistortionShape = Math.Clamp(Int("ShapeMode", 0), 0, 2),
                        Orientation = 1,
                        Width = Value(V("ScaleX"), FxArgument.ParticleAge) ?? FxValue.Constant(1),
                        Height = Value(V("ScaleY", "ScaleX"), FxArgument.ParticleAge) ?? FxValue.Constant(1),
                        Intensity = Value(V("DistortionIntensity"), FxArgument.ParticleAge),
                        Color1 = Colour(n, "color"),
                        InterpolateFrames = false,
                    };
                    return true;
                case 107 when V("TextureID") != null:
                    app.Billboard = new FxBillboardDef
                    {
                        Kind = FxBillboardKind.RadialBlur,
                        Texture = Int("TextureID", 0),
                        Orientation = 1,
                        Width = Value(V("Scale1X"), FxArgument.ParticleAge) ?? FxValue.Constant(1),
                        Height = Value(V("Scale1Y", "Scale1X"), FxArgument.ParticleAge) ?? FxValue.Constant(1),
                        Color1 = Colour(n, "color"),
                        InterpolateFrames = false,
                    };
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Lights (a24 in DES / DS1 / DS1R / BB, DS2's a11001 point and a11000 spot light; §20.14), by their slot names
        /// (FXR3's PointLight / SpotLight order): any appearance with a diffuseColor is one, a spot light when it has a
        /// far. One light per particle, as FXR3's. The colour's alpha is the intensity (provisional: no multiplier
        /// slot is named; a24's alpha goes up to 7.8). Specular, falloff and flicker aren't applied.
        /// </summary>
        private static bool Light(FxConfigDef c, FxAppearanceDef app, Dictionary<string, IFxValue> n)
        {
            if (!n.TryGetValue("diffuseColor", out var diffuse))
                return false;
            var far = n.GetValueOrDefault("far");
            app.Light = new FxLightDef
            {
                Spot = far != null,
                Diffuse = Value(diffuse, FxArgument.ParticleAge),
                Radius = Value(n.GetValueOrDefault("radius"), FxArgument.ParticleAge),
                Far = Value(far, FxArgument.ParticleAge),
                RadiusX = Value(n.GetValueOrDefault("radiusX"), FxArgument.ParticleAge),
                RadiusY = Value(n.GetValueOrDefault("radiusY"), FxArgument.ParticleAge),
            };
            return true;
        }

        /// <summary>
        /// Model appearances (a3 / a61 model appearance, a108 Particle3D): the model id, its size (scale 2 over the
        /// particle's age × scale 1, as billboards; provisional), rotation and speed, colours, blend mode and the
        /// particle's duration. Tinted by the effect's colours (provisional).
        /// </summary>
        private void Model(FxConfigDef c, FxAppearanceDef app, IFxAction a, Dictionary<string, IFxValue> n)
        {
            IFxValue V(params string[] names) => names.Select(x => n.GetValueOrDefault(x)).FirstOrDefault(x => x != null);
            // the model id: a named field (DS1 ActionData ModelID), else the first int param of type 41 (DES 35), the
            // type the effects' ResourceSet lists as models (§16.1)
            var idValue = V("modelID", "ModelID") ?? a.Values.FirstOrDefault(v => v.Resource == "model") ?? n.Values.FirstOrDefault(v => v.Resource == "model")
                ?? a.Values.FirstOrDefault(v => v is IFxTypedValue t && (t.FileType == 41 || (game == FfxGame.DES && t.FileType == 35)));
            int model = (int)(idValue?.Get()?.FirstOrDefault() ?? 0);
            if (model <= 0)
            {
                Note($"appearance a{a.TypeId} (no model id)");
                return;
            }

            var m = new FxModelDef { Model = model, ColouredByEffect = true };
            FxValue Axis(string axis) => Value(V($"scale2{axis}"), FxArgument.ParticleAge) ?? Value(V($"scale1{axis}"), FxArgument.ParticleAge);
            m.SizeX = Axis("X") ?? FxValue.Constant(1);
            m.SizeY = Axis("Y") ?? m.SizeX;
            m.SizeZ = Axis("Z") ?? m.SizeX;
            if (V("scale2X") != null && V("scale1X") != null)
            {
                c.ParticleScaleX = Value(V("scale1X"), FxArgument.ParticleAge);
                c.ParticleScaleY = Value(V("scale1Y"), FxArgument.ParticleAge);
                c.ParticleScaleZ = Value(V("scale1Z"), FxArgument.ParticleAge);
            }
            m.RotationX = Angle(V("rotX"));
            m.RotationY = Angle(V("rotY"));
            m.RotationZ = Angle(V("rotZ"));
            var sx = Angle(V("rotSpeedX"));
            var sy = Angle(V("rotSpeedY"));
            var sz = Angle(V("rotSpeedZ"));
            m.AngularSpeed = new[] { sx, null, sy, null, sz, null }; // the renderer indexes it unchecked
            m.Color1 = Colour(n, "color1");
            m.Color2 = Colour(n, "color2") ?? Colour(n, "color");
            m.BlendModeValue = (int)(V("blendMode")?.Get()?.FirstOrDefault() ?? 2);
            app.Model = m;
            c.ParticleDuration = Seconds(V("duration"));
        }

        // ---------------------------------------------------------------- values

        private static FxValue Colour(Dictionary<string, IFxValue> n, string name)
        {
            if (n.TryGetValue(name, out var whole))
                return Value(whole, FxArgument.ParticleAge);
            var ch = new[] { "R", "G", "B", "A" }.Select(x => n.GetValueOrDefault(name + x)).ToArray();
            if (ch.Any(x => x == null))
                return null;
            var curves = ch.Select(ToCurve).ToArray();
            if (curves.Any(x => x == null))
                return null;
            // one 4-component curve: the channels sampled at every key time of any of them, linearly
            var times = curves.SelectMany(k => k.Function <= FxrPropertyFunction.Constant ? new[] { 0f } : k.Keyframes.Select(f => f.Position)).Distinct().OrderBy(t => t).ToList();
            if (curves.All(k => k.Function <= FxrPropertyFunction.Constant))
                return FxValue.Constant(curves.Select(k => k.Evaluate(0)[0]).ToArray());
            var curve = new FxrCurve { Function = FxrPropertyFunction.Linear, ValueType = FxrValueType.Vector4, Loop = curves.Any(k => k.Loop) };
            foreach (var t in times)
                curve.Keyframes.Add(new FxrKeyframe { Position = t, Value = curves.Select(k => k.Evaluate(t)[0]).ToArray() });
            return FxValue.FromCurve(curve, FxArgument.ParticleAge);
        }

        private static FxrCurve ToCurve(IFxValue v)
        {
            if (v is IFxWrappedValue w && w.Inner != null)
            {
                // ParamFloat / ParamRGBA (82 / 84): the inner value with a variation, not a factor. DS2 stores a scale
                // as 1 × 0 and a still emitter's speed as 0 × 0, next to speeds like 3.75 × 0.2: multiplied, every
                // such scale would be 0. The variation is left out (provisional: a random range around the value).
                return ToCurve(w.Inner);
            }
            if (v is IFxCurveValue cv && v.Kind == FxValueKind.Curve)
            {
                try { return cv.GetCurve(); } catch { return null; }
            }
            var d = v.Get();
            if (d == null || d.Length == 0)
                return null;
            var type = (FxrValueType)Math.Clamp(d.Length - 1, 0, 3);
            return new FxrCurve { Function = FxrPropertyFunction.Constant, ValueType = type, Constant = d.Take(4).Select(x => (float)x).ToArray() };
        }

        private static FxValue Value(IFxValue v, FxArgument arg)
        {
            if (v == null)
                return null;
            var curve = ToCurve(v);
            return curve == null ? null : FxValue.FromCurve(curve, arg);
        }

        /// <summary>
        /// A rotation or rotation speed in degrees, over the particle's age. DS2 stores radians (rotZ is the range
        /// [0, 6.28], speeds like 10.47); the other games are taken as degrees (provisional).
        /// </summary>
        private FxValue Angle(IFxValue v)
        {
            var c = v == null ? null : ToCurve(v);
            if (c == null)
                return null;
            if (game is FfxGame.DS2 or FfxGame.DS2S)
                for (int i = 0; i < c.Components; i++) c.ScaleComponent(i, 180 / Math.PI);
            return FxValue.FromCurve(c, FxArgument.ParticleAge);
        }

        /// <summary>A time in seconds; a negative one (-1 frame) means endless and reads -1.</summary>
        private static FxValue Seconds(IFxValue v)
        {
            var c = v == null ? null : ToCurve(v);
            if (c == null)
                return null;
            if (c.Function <= FxrPropertyFunction.Constant && c.Constant?.Length > 0 && c.Constant[0] < 0)
                return FxValue.Constant(-1);
            return FxValue.FromCurve(c, FxArgument.ActiveTime);
        }

        private static float Scalar(IFxValue v, float fallback) => v?.Get() is { Length: > 0 } d ? (float)d[0] : fallback;
    }
}
