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

            // the node actions among the call's values, by their slot names (§20.23: FXR3's ids and field order):
            // a35 / a36 Static / RandomNodeTransform, a34 NodeSpin, a1 NodeAcceleration
            for (int k = 0; k < values.Count; k++)
                if (values[k] is IFxCallValue { Call: { } na })
                    NodeActions(config, na);

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

        /// <summary>
        /// A node action passed to a node type (§20.23), read by its slot names as FXR3's: the transform (offset,
        /// rotation in degrees, and for RandomNodeTransform their variances), the spin (angular speed and multiplier
        /// per axis) and the acceleration (speed and acceleration along Z, acceleration along Y). (provisional) A
        /// Float2 range reads its first value; the spin is taken as degrees per second.
        /// </summary>
        private void NodeActions(FxConfigDef c, IFxAction a)
        {
            // the action's own slots only: DS1's a1 also carries an ActionData record (Unk1 257, OffsetX / OffsetY, …)
            // that isn't the node acceleration's layout
            var n = new Dictionary<string, IFxValue>(StringComparer.OrdinalIgnoreCase);
            foreach (var v in a.Values)
                if (v.Name != null) n.TryAdd(v.Name, v);
            float F(string name) => (float)(n.GetValueOrDefault(name)?.Get()?.FirstOrDefault() ?? 0);
            Vector3 V3(string prefix) => new(F(prefix + "X"), F(prefix + "Y"), F(prefix + "Z"));
            if (n.ContainsKey("offsetX"))
            {
                c.Offset = V3("offset");
                c.Rotation = V3("rotation");
                if (n.ContainsKey("offsetVarianceX"))
                {
                    c.OffsetVariance = V3("offsetVariance");
                    c.RotationVariance = V3("rotationVariance");
                }
            }
            if (n.ContainsKey("angularSpeedX"))
                c.NodeSpin = new[] { "X", "Y", "Z" }.SelectMany(ax => new[]
                {
                    Value(n.GetValueOrDefault("angularSpeed" + ax), FxArgument.ActiveTime),
                    Value(n.GetValueOrDefault("angularSpeedMultiplier" + ax), FxArgument.ActiveTime),
                }).ToArray();
            if (n.ContainsKey("accelerationZ") || n.ContainsKey("speedZ"))
            {
                // NodeSpeed (a120 / a121, §20.24) has a speed multiplier and no acceleration Z; the others accelerate
                c.NodeMove = n.ContainsKey("accelerationZ") ? FxNodeMoveKind.Acceleration : FxNodeMoveKind.Speed;
                c.NodeSpeedMultiplierZ = Value(n.GetValueOrDefault("speedMultiplierZ"), FxArgument.ActiveTime);
                c.NodeSpeedZ = Value(n.GetValueOrDefault("speedZ"), FxArgument.ActiveTime);
                c.NodeAccelerationZ = Value(n.GetValueOrDefault("accelerationZ"), FxArgument.ActiveTime);
                c.NodeAccelerationMultiplierZ = Value(n.GetValueOrDefault("accelerationMultiplierZ"), FxArgument.ActiveTime);
                c.NodeAccelerationY = Value(n.GetValueOrDefault("accelerationY"), FxArgument.ActiveTime);
            }
        }

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
                case 28: c.Shape = FxShapeKind.Point; break;
                // the two-angle cones (§20.22): an elliptic cone spreads elliptically, a four-sided pyramid rectangularly,
                // over their X / Y angles (provisional: the angles read as a28's degree, a half-angle in degrees)
                case 33 or 45:
                    c.Shape = FxShapeKind.Point;
                    var dx = Value(V("emitterDegreeX"), FxArgument.ActiveTime);
                    var dy = Value(V("emitterDegreeY"), FxArgument.ActiveTime);
                    if (dx != null || dy != null)
                    {
                        c.Spread = a.TypeId == 33 ? FxSpreadKind.Elliptical : FxSpreadKind.Rectangular;
                        c.SpreadAngleX = dx ?? dy;
                        c.SpreadAngleY = dy ?? dx;
                    }
                    break;
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

            // the emitter's particle modifier (§20.22: FXR3 ParticleModifier's scale X / Y / Z, colour and uniform scale,
            // written by every DS2 emitter compile outside its block): multipliers on the particles' size and colour.
            // (provisional) a ParamFloat's variation isn't applied, as elsewhere
            if (V("particleScaleX") is { } psx)
            {
                c.ParticleScaleX = Value(psx, FxArgument.ActiveTime);
                c.ParticleScaleY = Value(V("particleScaleY"), FxArgument.ActiveTime);
                c.ParticleScaleZ = Value(V("particleScaleZ"), FxArgument.ActiveTime);
                c.UniformScale = (V("uniformScale")?.Get()?.FirstOrDefault() ?? 0) != 0;
            }
            if (V("particleColor") is { } pc)
                c.ParticleColor = Value(pc, FxArgument.ActiveTime);
        }

        private void Movement(FxConfigDef c, IFxAction a)
        {
            var n = Named(a);
            // (provisional) gravity pulls down, in the runtime's units, over the particle's age
            var g = Value(n.GetValueOrDefault("gravity"), FxArgument.ParticleAge);
            if (g != null) c.ParticleGravity = g;
            // FXR3's ParticleSpeed family (BB's a60 / a64 / a65, §20.24): the particles' speed and its multiplier
            // (provisional: a Float2 multiplier range reads its first value)
            var speed = Value(n.GetValueOrDefault("speed"), FxArgument.ParticleAge);
            if (speed != null)
            {
                c.ParticleMove = FxParticleMoveKind.Speed;
                c.ParticleMoveSpeed = speed;
                c.ParticleMoveSpeedMultiplier = Value(n.GetValueOrDefault("speedMultiplier"), FxArgument.ParticleAge);
            }
            if (c.ParticleMove == FxParticleMoveKind.None) c.ParticleMove = FxParticleMoveKind.Speed;
            // noted only when the action names neither (DS1's a55 often stores an empty FXField 28 as its gravity: no value, nothing lost)
            if (!n.ContainsKey("gravity") && !n.ContainsKey("speed")) Note($"movement a{a.TypeId}");
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
            if (a.TypeId is 40 or 43 or 107 or 18000 or 20005 && ScreenOrTrail(c, app, a, n))
                return;
            if (a.TypeId is 66 or 82 && QuadLine(c, app, n, quad: a.TypeId == 66))
                return;
            if (Light(c, app, n))
                return;
            if (GpuParticles(c, app, a, n))
                return;
            if (a.TypeId is not (20 or 27 or 59 or 70 or 71 or 15000 or 15001 or 15002 or 20002))
            {
                // §20.44: BB's a10003, FXR3's LightShaft (named §20.25), drawn as FXR3's are: layered flares at the
                // source, coloured color1 → color2 → color3 across the layers
                if (a.TypeId == 10003 && n.ContainsKey("layers"))
                {
                    IFxValue S(string x) => n.GetValueOrDefault(x);
                    int shaftTexture = (int)Scalar(S("texture"), 0);
                    app.LightShaft = new FxLightShaftDef
                    {
                        // BB's 9999x is "no texture" (§20.28)
                        Texture = shaftTexture >= 99990 ? 0 : shaftTexture,
                        BlendMode = (int)Scalar(S("blendMode"), 4),
                        Layers = Math.Clamp((int)Scalar(S("layers"), 30), 1, FxLightShaftDef.MaxLayers),
                        Weight = Math.Clamp(Scalar(S("layerWeight"), 1), 0, 4),
                        Spread = Math.Max(Scalar(S("layerSpread"), 1), 0),
                        Width = Value(S("width"), FxArgument.ParticleAge),
                        Height = Value(S("height"), FxArgument.ParticleAge),
                        Color1 = Value(S("color1"), FxArgument.ParticleAge),
                        Color2 = Value(S("color2"), FxArgument.ParticleAge),
                        Color3 = Value(S("color3"), FxArgument.ParticleAge),
                    };
                    return;
                }
                // §20.43: BB's forces (FXR3's WindForce / GravityForce / CancelForce) are named for the Inspector, not
                // simulated: no BB action responds to forces (FXR3's force movements 731–734 aren't in BB)
                if (a.TypeId is 10200 or 10300 or 10301)
                {
                    Note($"force a{a.TypeId} (named; nothing in BB responds to forces)");
                    return;
                }
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
                // BB's a10008 (FXR3 GPUSparkParticle) is a spark system: drawn as one billboard of its texture it was
                // an alpha-blended square (f000000960, §20.36), so it's left out until its slots are named
                else if (a.TypeId == 10008)
                    Note("appearance a10008 (GPU sparks, not drawn)");
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
                Kind = a.TypeId is 27 or 15002 ? FxBillboardKind.PointSprite : FxBillboardKind.BillboardEx,
                Orientation = 1,
            };

            // the texture: a named texture id, else the first texture-typed value
            var tex = V("textureID", "TextureID", "TextureID1") ?? a.Values.FirstOrDefault(v => v.Resource == "texture")
                ?? n.Values.FirstOrDefault(v => v.Resource == "texture");
            bb.Texture = (int)(tex?.Get()?.FirstOrDefault() ?? 0);

            // a multi-texture billboard (DS1's a70 ActionData, DS2's a70 / a15001 by the same field names, §20.18): its
            // second and third textures as layers, each scrolling and offset in UV. (provisional) UV units per second as
            // FXR3's, the layers combined as FXR3's opacity mode 0 (colours multiplied, opacity layer 1's alpha)
            int Id(string name) => (int)(V(name)?.Get()?.FirstOrDefault() ?? 0);
            if (Id("textureID2") > 0 || Id("textureID3") > 0)
            {
                bb.Kind = FxBillboardKind.MultiTexture;
                bb.Layer2 = Id("textureID2");
                bb.Layer3 = Id("textureID3");
                for (int i = 0; i < 3; i++)
                {
                    bb.LayerSpeedU[i] = Value(V($"tex{i + 1}UVScrollX"), FxArgument.ParticleAge);
                    bb.LayerSpeedV[i] = Value(V($"tex{i + 1}UVScrollY"), FxArgument.ParticleAge);
                    bb.LayerOffsetU[i] = Value(V($"tex{i + 1}UVOffsetX"), FxArgument.ParticleAge);
                    bb.LayerOffsetV[i] = Value(V($"tex{i + 1}UVOffsetY"), FxArgument.ParticleAge);
                }
            }
            // (provisional) the same blend mode numbers as FXR3
            bb.BlendMode = (int)(V("blendMode")?.Get()?.FirstOrDefault() ?? 2);
            bb.Columns = Math.Max(1, (int)(V("animFrameSliceCountPerRow")?.Get()?.FirstOrDefault() ?? 1));
            bb.TotalFrames = Math.Max(1, (int)(V("animFrameTotalCount")?.Get()?.FirstOrDefault() ?? 1));
            bb.FrameIndex = Value(V("animFrameInitial"), FxArgument.ParticleAge);
            bb.FrameIndexOffset = Value(V("animFrameTimeline"), FxArgument.ParticleAge);

            // (provisional) size: scale 2 is the billboard's size over the particle's age, scale 1 a factor on it; a
            // single scale pair (a59's scaleX / scaleY, DS1 ActionData 59 and DS2, §20.19) is the size itself
            // a missing axis takes the other one's size (a15001 has scale 2's Y named, not its X)
            var width = Value(V("scale2X"), FxArgument.ParticleAge) ?? Value(V("scale1X", "scaleX", "scale"), FxArgument.ParticleAge);
            var height = Value(V("scale2Y"), FxArgument.ParticleAge) ?? Value(V("scale1Y", "scaleY", "scale"), FxArgument.ParticleAge);
            bb.Width = width ?? height ?? FxValue.Constant(1);
            bb.Height = height ?? bb.Width;
            if (V("scale2X") != null && V("scale1X") != null)
            {
                c.ParticleScaleX ??= Value(V("scale1X"), FxArgument.ParticleAge);
                c.ParticleScaleY ??= Value(V("scale1Y"), FxArgument.ParticleAge);
            }
            bb.Size = bb.Width;
            // §20.41: a point sprite's size is its ScaleMult (DS1 ActionData 27: 0.02 / 0.5, beside a Scale that is a
            // random range around 1; DS2 a27 / a15002 and BB a27 by name: 0.008–0.05, BB up to 1.9), × the emitter's
            // particle scale. Read as "scale", every point sprite was about 1 unit across
            if (bb.Kind == FxBillboardKind.PointSprite && Value(V("scaleMult"), FxArgument.ParticleAge) is { } sm)
                bb.Size = sm;

            // colours: a whole colour value, or four channel fields (DS1 ActionData Color1R..A)
            bb.Color1 = Colour(n, "color1");
            bb.Color2 = Colour(n, "color2") ?? Colour(n, "color");
            bb.Color3 = Colour(n, "color3");
            TypedColours(a, bb);
            bb.RotationZ = Angle(a, V("rotZ"));
            var rs = Angle(a, V("rotSpeedZ"));
            if (rs != null) bb.AngularSpeed = new FxValue[] { null, null, null, null, rs, null };

            app.Billboard = bb;

            // the particle's life: the appearance's duration (seconds)
            // a20002 (Blink) names no duration; drawn by the "texture only" path before §20.42 it left the life as it was
            if (a.TypeId != 20002 || V("duration") != null)
                c.ParticleDuration = Seconds(V("duration"), range: true);
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
            var sequences = a.Values.Where(v => v is IFxTypedValue { TypeName: { } tn } && tn.StartsWith("RGBA") && tn.Contains("Sequence")).ToList();
            if (sequences.Count == 0)
                return;
            // the first: the colour, unless a slot names one; the second: the multiplier, unless it has a name of its
            // own (DES's a71 names its two colours color1 / color2; DS2's a59 names only its colour, §20.19)
            if (bb.Color1 == null && bb.Color2 == null)
                bb.Color2 = Value(sequences[0], FxArgument.ParticleAge);
            if (bb.Color3 == null && sequences.Count >= 2 && IsDefaultName(sequences[1].Name))
                bb.Color3 = Value(sequences[1], FxArgument.ParticleAge);
        }

        private static bool IsDefaultName(string name) =>
            name == null || System.Text.RegularExpressions.Regex.IsMatch(name, @"^(unk|texture|sound|effect|action|param)\d\d$");

        /// <summary>
        /// Tracer (a40), distortion (a43) and radial blur (a107), from DS1's ActionData field names (FXR1 games) or the
        /// slot names DS2's exe compile gave (a43 §20.33, BB's a107 §20.39); unnamed ones fall back to a screen effect
        /// of size 1. False when the
        /// fields aren't there. Provisional: width = scale 1 (tracer), segment time = TrailMaxTime, segments =
        /// TrailLength, intensity = DistortionIntensity, the modes numbered as FXR3's.
        /// </summary>
        private bool ScreenOrTrail(FxConfigDef c, FxAppearanceDef app, IFxAction a, Dictionary<string, IFxValue> n)
        {
            IFxValue V(params string[] names) => names.Select(x => n.GetValueOrDefault(x)).FirstOrDefault(x => x != null);
            int Int(string name, int fallback) => (int)(V(name)?.Get()?.FirstOrDefault() ?? fallback);
            float Float(string name, float fallback) => (float)(V(name)?.Get()?.FirstOrDefault() ?? fallback);
            // BB writes 99999 (or 9999x) for "no texture" (§20.28): none
            int Tex(string name) => Int(name, 0) is var t && t >= 99990 ? 0 : t;

            switch (a.TypeId)
            {
                // a18000 is DS2's own tracer, its first slots named as a40's (§20.41)
                case 40 or 18000 when V("TextureID") != null:
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
                        // the mask; DS2's a43 has none (TextureID1 0 in every instance, §20.20), nor do most of BB's:
                        // then none (the shader's full mask). The normal map used to stand in, but its colour,
                        // about (0.5, 0.5, 1), tinted the scene dark blue: BB f000000052's black disc (§20.32)
                        Texture = Tex("TextureID1"),
                        Layer2 = Tex("TextureID2"),
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
                // §20.39: DS1 / DS1R by their ActionData field names, BB by FXR3 RadialBlur's (DarkSoulsII.exe's compile
                // puts the slots on the same fields). DS1R's "Scale1Z" holds BB's blurRadius values (0 → 0.1 → 0
                // pulses), so it's read as the blur radius; FXR3's default 0.5 when there's neither
                // a20005 is DS2's own radial blur (SfxFxDrawEntityHostRadialBlur), named as BB's a107 (§20.42)
                case 107 or 20005 when V("TextureID", "mask") != null:
                    app.Billboard = new FxBillboardDef
                    {
                        Kind = FxBillboardKind.RadialBlur,
                        Texture = V("mask") != null ? Tex("mask") : Int("TextureID", 0),
                        Orientation = 1,
                        Width = Value(V("width", "Scale1X"), FxArgument.ParticleAge) ?? FxValue.Constant(1),
                        Height = Value(V("height", "Scale1Y", "width", "Scale1X"), FxArgument.ParticleAge) ?? FxValue.Constant(1),
                        Color1 = Colour(n, "color"),
                        BlurRadius = Value(V("blurRadius", "Scale1Z"), FxArgument.ParticleAge),
                        InterpolateFrames = false,
                    };
                    return true;
            }
            return false;
        }

        /// <summary>
        /// a66, FXR3's QuadLine (§20.40): a camera-facing strip from the particle back along its travel, width × length,
        /// coloured color1 × color2 × color3 and from startColor (leading) to endColor (trailing); no texture. DES /
        /// DS2 / BB by their slot names. DS1 / DS1R by ActionData 66's fields in struct order (DSAnimStudio's names
        /// are positional there): Unk6_3 width, Unk6_4 length, Unk6_5–8 color1, Unk6_9–12 color2, Unk6_13–16
        /// startColor, Unk6_17 / 18 / Color1R / G endColor, Color1B / A the width / length multipliers, Color2 color3,
        /// Unk5 the blend mode. (provisional) startColor / endColor follow FXR3's order; which end is which isn't
        /// confirmed against the games.
        /// </summary>
        private static bool QuadLine(FxConfigDef c, FxAppearanceDef app, Dictionary<string, IFxValue> n, bool quad = true)
        {
            bool ds1 = n.ContainsKey("Unk6_4");
            if (!ds1 && !n.ContainsKey("length"))
                return false;
            FxValue V(string name) => Value(n.GetValueOrDefault(name), FxArgument.ParticleAge);
            FxValue C(string r, string g, string b, string al) => Channels(n, r, g, b, al);
            // a82 is FXR3's Line (§20.44): no width, drawn 1.5 % of its length wide as FXR3's
            var line = new FxLineDef { Quad = quad };
            if (ds1)
            {
                line.Width = V("Unk6_3");
                line.Length = V("Unk6_4");
                line.Color1 = C("Unk6_5", "Unk6_6", "Unk6_7", "Unk6_8");
                line.Color2 = C("Unk6_9", "Unk6_10", "Unk6_11", "Unk6_12");
                line.StartColor = C("Unk6_13", "Unk6_14", "Unk6_15", "Unk6_16");
                line.EndColor = C("Unk6_17", "Unk6_18", "Color1R", "Color1G");
                line.WidthMultiplier = V("Color1B");
                line.LengthMultiplier = V("Color1A");
                line.Color3 = C("Color2R", "Color2G", "Color2B", "Color2A");
                line.BlendMode = (int)Scalar(n.GetValueOrDefault("Unk5"), 2);
            }
            else
            {
                line.Width = V("width");
                line.Length = V("length");
                line.WidthMultiplier = V("widthMultiplier");
                line.LengthMultiplier = V("lengthMultiplier");
                line.Color2 = V("color2");
                line.StartColor = V("startColor");
                line.EndColor = V("endColor");
                line.Color3 = V("color3");
                line.BlendMode = (int)Scalar(n.GetValueOrDefault("blendMode"), 2);
            }
            // DES has no width multiplier and stores the width as a sequence: unset values are 1 in FxLineDef
            app.Line = line;
            c.ParticleDuration ??= Seconds(n.GetValueOrDefault("duration") ?? n.GetValueOrDefault("Duration"), range: true);
            return true;
        }

        /// <summary>A colour from four channel values (each a constant or a sequence), sampled as <see cref="Colour"/> does.</summary>
        private static FxValue Channels(Dictionary<string, IFxValue> n, string r, string g, string b, string a)
        {
            var ch = new[] { r, g, b, a }.Select(x => n.GetValueOrDefault(x)).ToArray();
            if (ch.Any(x => x == null))
                return null;
            var tmp = new Dictionary<string, IFxValue> { ["cR"] = ch[0], ["cG"] = ch[1], ["cB"] = ch[2], ["cA"] = ch[3] };
            return Colour(tmp, "c");
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
        /// A GPU particle system (BB's a10000, FXR3's GPUStandardParticle, §20.25–26), from the slots named after FXR3's
        /// fields: textures, emitter shape / size / rotation / distribution, particle offset and speed with their
        /// ranges, colour and sprite sheet (§20.36), count / interval / life (frames at 30 Hz) and size / growth
        /// (§20.37), acceleration, rotation and angular speed / acceleration (§20.38). Run by the node, as FXR3's.
        /// Without the §20.37 names the runtime's defaults stay: 10 particles per emission, one emission a second, a
        /// one-second life, size 1.
        /// </summary>
        private bool GpuParticles(FxConfigDef c, FxAppearanceDef app, IFxAction a, Dictionary<string, IFxValue> n)
        {
            if (!n.ContainsKey("particleSpeedMinX") || !n.ContainsKey("emitterSizeX"))
                return false;
            float F(string name, float fallback = 0) => (float)(n.GetValueOrDefault(name)?.Get()?.FirstOrDefault() ?? fallback);
            Vector3 V3(string prefix) => new(F(prefix + "X"), F(prefix + "Y"), F(prefix + "Z"));
            Vector4 V4(string name) => n.GetValueOrDefault(name)?.Get() is { Length: >= 4 } v ? new((float)v[0], (float)v[1], (float)v[2], (float)v[3]) : Vector4.Zero;
            var d = new FxGpuParticleDef
            {
                Texture = (int)F("texture", 1),
                Shape = (int)F("emitterShape", 1),
                EmitterSize = V3("emitterSize"),
                EmitterRotation = V3("emitterRotation"),
                Distribution = F("emitterDistribution"),
                Offset = V3("particleOffset"),
                OffsetMin = V3("particleOffsetMin"),
                OffsetMax = V3("particleOffsetMax"),
                Speed = V3("particleSpeed"),
                SpeedMin = V3("particleSpeedMin"),
                SpeedMax = V3("particleSpeedMax"),
                BlendMode = (int)F("blendMode", 4),
                // the colour over the particle's age, its multipliers and the sprite sheet (§20.36: FXR3 fields1's
                // rgbMultiplier … maxFrameIndex run). Unnamed, the sheet was drawn whole and white at full alpha
                Color = Value(n.GetValueOrDefault("color"), FxArgument.ParticleAge),
                RgbMultiplier = F("rgbMultiplier", 1),
                AlphaMultiplier = F("alphaMultiplier", 1),
                ColorMin = V4("colorMin"),
                ColorMax = V4("colorMax"),
                Columns = Math.Max(1, (int)F("columns", 1)),
                TotalFrames = Math.Max(1, (int)F("totalFrames", 1)),
                RandomFrame = F("randomTextureFrame") != 0,
                MaxFrameIndex = (int)F("maxFrameIndex"),
            };

            // §20.37: count, interval and life are each a base int plus a min / max variance; interval and life are
            // frames at 30 Hz. The def takes the interval as an absolute range and the life as one value (its
            // variance isn't per particle in the runtime: the midpoint is used)
            bool timed = n.ContainsKey("particleDuration") && n.ContainsKey("emissionInterval");
            if (timed)
            {
                const float frame = FxGpuParticleDef.FrameInterval;
                d.PerEmission = (int)F("emissionParticleCount", 10);
                d.PerEmissionMin = (int)F("emissionParticleCountMin");
                d.PerEmissionMax = (int)F("emissionParticleCountMax");
                float interval = F("emissionInterval", 1);
                // 0 emits every frame (the runtime's own rule for a zero interval)
                d.IntervalMin = Math.Max(0, interval + F("emissionIntervalMin")) * frame;
                d.IntervalMax = Math.Max(0, interval + F("emissionIntervalMax")) * frame;
                d.LimitEmissions = F("limitEmissionCount") != 0;
                d.EmissionLimit = (int)F("emissionCountLimit");
                float life = F("particleDuration", 30) + (F("particleDurationMin") + F("particleDurationMax")) / 2;
                d.Duration = Math.Max(1, life) * frame;
            }
            else
            {
                // as FXR3's builder falls back when the interval isn't named (0 would emit every frame)
                d.IntervalMin = 1;
                d.IntervalMax = 1;
            }

            bool sized = n.ContainsKey("particleSizeX");
            if (sized)
            {
                d.UniformScale = F("particleUniformScale") != 0;
                d.SizeX = F("particleSizeX", 1);
                d.SizeY = F("particleSizeY", d.SizeX);
                d.SizeXMin = F("particleSizeXMin");
                d.SizeXMax = F("particleSizeXMax");
                d.SizeYMin = F("particleSizeYMin");
                d.SizeYMax = F("particleSizeYMax");
                d.GrowthRateX = Value(n.GetValueOrDefault("particleGrowthRateX"), FxArgument.ParticleAge);
                d.GrowthRateY = Value(n.GetValueOrDefault("particleGrowthRateY"), FxArgument.ParticleAge);
                d.GrowthStaticX = F("particleGrowthRateXStatic");
                d.GrowthStaticY = F("particleGrowthRateYStatic");
                d.GrowthXMin = F("particleGrowthRateXMin");
                d.GrowthXMax = F("particleGrowthRateXMax");
                d.GrowthYMin = F("particleGrowthRateYMin");
                d.GrowthYMax = F("particleGrowthRateYMax");
            }

            // §20.38: acceleration (properties over the particle's age, plus a random min / max per particle), rotation
            // and angular speed as min / max ranges (degrees, degrees per second), angular acceleration. The runtime
            // evaluates the properties once per system, so they take the active time (provisional: BB's argument
            // isn't known; only 17 vanilla instances aren't constant)
            bool moved = n.ContainsKey("particleRotationMinZ");
            if (moved)
            {
                d.AccelerationX = Value(n.GetValueOrDefault("particleAccelerationX"), FxArgument.ActiveTime);
                d.AccelerationY = Value(n.GetValueOrDefault("particleAccelerationY"), FxArgument.ActiveTime);
                d.AccelerationZ = Value(n.GetValueOrDefault("particleAccelerationZ"), FxArgument.ActiveTime);
                d.AccelerationMin = V3("particleAccelerationMin");
                d.AccelerationMax = V3("particleAccelerationMax");
                d.RotationRanges = true;
                d.RotationMinZ = F("particleRotationMinZ");
                d.RotationMaxZ = F("particleRotationMaxZ");
                d.AngularSpeedMinZ = F("particleAngularSpeedMinZ");
                d.AngularSpeedMaxZ = F("particleAngularSpeedMaxZ");
                d.AngularAccelerationZ = Value(n.GetValueOrDefault("particleAngularAccelerationZ"), FxArgument.ActiveTime);
                d.AngularAccelerationMin = V3("particleAngularAccelerationMin");
                d.AngularAccelerationMax = V3("particleAngularAccelerationMax");
            }

            app.GpuParticles = d;
            c.GpuSystems.Add(app);
            // the survey lists notes as unmapped: only what is still missing
            if (!(timed && sized && moved))
                Note($"appearance a{a.TypeId} (GPU particles, count / timing / size / rotation provisional)");
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
                c.ParticleScaleX ??= Value(V("scale1X"), FxArgument.ParticleAge);
                c.ParticleScaleY ??= Value(V("scale1Y"), FxArgument.ParticleAge);
                c.ParticleScaleZ ??= Value(V("scale1Z"), FxArgument.ParticleAge);
            }
            m.RotationX = Angle(a, V("rotX"));
            m.RotationY = Angle(a, V("rotY"));
            m.RotationZ = Angle(a, V("rotZ"));
            var sx = Angle(a, V("rotSpeedX"));
            var sy = Angle(a, V("rotSpeedY"));
            var sz = Angle(a, V("rotSpeedZ"));
            m.AngularSpeed = new[] { sx, null, sy, null, sz, null }; // the renderer indexes it unchecked
            m.Color1 = Colour(n, "color1");
            m.Color2 = Colour(n, "color2") ?? Colour(n, "color");
            m.BlendModeValue = (int)(V("blendMode")?.Get()?.FirstOrDefault() ?? 2);
            app.Model = m;
            c.ParticleDuration = Seconds(V("duration"), range: true);
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
                FxrCurve curve;
                try { curve = cv.GetCurve(); } catch { return null; }
                // §20.38: a cyclic sequence with one key (length 0) evaluates to NaN in FxrCurve, as FXR3 games do.
                // The older games use them as constants: BB's a10000 has 58 instances holding 0 at key 0 in its
                // acceleration / follow slots, and a NaN there removed every particle. Not looped, it holds its key
                if (curve is { Loop: true } && curve.SequenceDuration == 0)
                    curve.Loop = false;
                return curve;
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
        /// A rotation or rotation speed in degrees, over the particle's age. Degrees unless the action's schema slot
        /// says <c>unit: "radians"</c> (§20.17): DS2's a71 stores its rotations as radians ([0, 6.28]), while DS2's own
        /// a15000 / a15001 store degrees ([90, 90], [0, 360], [180, 180]), so the unit is per slot, not per game.
        /// </summary>
        private FxValue Angle(IFxAction a, IFxValue v)
        {
            var c = v == null ? null : ToCurve(v);
            if (c == null)
                return null;
            if (SlotOf(a, v)?.Unit == "radians")
                for (int i = 0; i < c.Components; i++) c.ScaleComponent(i, 180 / Math.PI);
            return FxValue.FromCurve(c, FxArgument.ParticleAge);
        }

        /// <summary>The schema slot an action's own value is in, or null (an ActionData value, no layout).</summary>
        private FfxParamDef SlotOf(IFxAction a, IFxValue v)
        {
            var layout = schema.Layout(a.TypeId, game);
            if (layout == null)
                return null;
            var values = a.Values;
            for (int k = 0; k < values.Count && k < layout.Params.Count; k++)
                if (ReferenceEquals(values[k], v))
                    return layout.Params[k];
            return null;
        }

        /// <summary>A time in seconds; a negative one (-1 frame) means endless and reads -1.</summary>
        private static FxValue Seconds(IFxValue v, bool range = false)
        {
            var c = v == null ? null : ToCurve(v);
            if (c == null)
                return null;
            if (c.Function <= FxrPropertyFunction.Constant && c.Constant?.Length > 0 && c.Constant[0] < 0)
                return FxValue.Constant(-1);
            // §20.41: an appearance's duration Tick2 is a [min, max] range (BB a27 [0.93, 2.07], a66 [0.75, 0.92], a71;
            // DS2's are [1, 1] / [2, 2]): its midpoint, as the runtime has one life per config (the first value alone
            // shortened BB's sprites). Node
            // durations / emission timing (range false) keep the first value: their Tick2s aren't known to be ranges
            if (range && v is IFxTypedValue { TypeName: "Tick2" } && c.Function <= FxrPropertyFunction.Constant && c.Constant?.Length == 2)
                return FxValue.Constant((c.Constant[0] + c.Constant[1]) / 2);
            return FxValue.FromCurve(c, FxArgument.ActiveTime);
        }

        private static float Scalar(IFxValue v, float fallback) => v?.Get() is { Length: > 0 } d ? (float)d[0] : fallback;
    }
}
