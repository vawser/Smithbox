using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// An FXR read into what the runtime needs (FX Editor Plan §10.2): the state conditions and the node tree, each
/// node's configs reduced to their timing, transform, movement, emitter, shape, spread, particle and termination
/// values. Built once per FXR (and again after an edit), so the simulation never reads the schema.
/// </summary>
public sealed class FxEffectDef
{
    public int Id;
    public FxrGame Game;
    public List<FxrState> States = new();
    public FxNodeDef Root;

    /// <summary>Actions the runtime reads but couldn't (no schema layout, or the layout didn't fit).</summary>
    public int UntypedActions;

    public static FxEffectDef Build(FxrSchema schema, FxrGame game, FXR fxr)
    {
        var def = new FxEffectDef { Id = fxr.Id, Game = game, States = fxr.States };
        def.Root = FxNodeDef.Build(def, schema, game, fxr, fxr.Root);
        return def;
    }

    /// <summary>
    /// The config a node uses in a state, as fxr-main's <c>getActiveConfig</c>: the StateConfigMap's entry for the
    /// state, else its entry for state 0. A node with no map uses its first config.
    /// </summary>
    public static int ConfigIndexFor(List<int> stateMap, int configCount, int state)
    {
        if (configCount == 0)
            return -1;
        if (stateMap == null || stateMap.Count == 0)
            return 0;

        int index = state >= 0 && state < stateMap.Count ? stateMap[state] : stateMap[0];
        return index >= 0 && index < configCount ? index : -1;
    }

    /// <summary>The node's StateConfigMap (action 199), null if it has none.</summary>
    public static List<int> StateConfigMap(FxrNode node)
    {
        foreach (var a in node.Actions)
            if (a.Type == 199)
                return a.Section10s.Count > 0 ? a.Section10s[0] : new List<int>();
        return null;
    }
}

public enum FxNodeKind
{
    Root,
    Proxy,
    LevelsOfDetail,
    Basic,
    NodeEmitter,
    Unknown,
}

public sealed class FxNodeDef
{
    public FxrNode Source;
    public FxNodeKind Kind;
    public List<FxNodeDef> Children = new();
    public List<FxConfigDef> Configs = new();
    public List<int> StateMap;

    /// <summary>Proxy: the effect it starts.</summary>
    public int ProxyEffectId = -1;

    public FxConfigDef ConfigFor(int state)
    {
        int i = FxEffectDef.ConfigIndexFor(StateMap, Configs.Count, state);
        return i >= 0 ? Configs[i] : null;
    }

    public static FxNodeDef Build(FxEffectDef effect, FxrSchema schema, FxrGame game, FXR fxr, FxrNode node)
    {
        var def = new FxNodeDef
        {
            Source = node,
            Kind = node.Type switch
            {
                2000 => FxNodeKind.Root,
                2001 => FxNodeKind.Proxy,
                2002 => FxNodeKind.LevelsOfDetail,
                2200 => FxNodeKind.Basic,
                2202 => FxNodeKind.NodeEmitter,
                _ => FxNodeKind.Unknown,
            },
            StateMap = FxEffectDef.StateConfigMap(node),
        };

        if (def.Kind == FxNodeKind.Proxy && node.Actions.FirstOrDefault(a => a.Type == 132) is { Fields1.Count: > 0 } proxy)
            def.ProxyEffectId = (int)proxy.Fields1[0];

        foreach (var config in node.Configs)
            def.Configs.Add(FxConfigDef.Build(effect, schema, game, fxr, node, config));

        foreach (var child in node.Children)
            def.Children.Add(Build(effect, schema, game, fxr, child));

        return def;
    }
}

public enum FxEmitterKind { None, OneTime, Periodic, EqualDistance }
public enum FxShapeKind { Point, Disk, Rectangle, Sphere, Box, Cylinder }
public enum FxSpreadKind { None, Circular, Elliptical, Rectangular }
public enum FxNodeMoveKind { None, Speed, Acceleration }
public enum FxParticleMoveKind { None, Speed, Acceleration }
public enum FxTerminationKind { Simulate, Fade, Instant }

/// <summary>An appearance action of a config (BillboardEx, PointLight, ...), with its views for the renderers.</summary>
public sealed class FxAppearanceDef
{
    public FxrAction Action;
    public FxrActionView View;
    public string Name;
    public bool IsParticle;
}

public sealed class FxConfigDef
{
    public FxrConfig Source;

    /// <summary>Every action of the node and the config, for the renderers and the highlight.</summary>
    public List<FxrAction> Actions = new();

    // NodeAttributes (128) / LevelsOfDetailThresholds (133)
    public FxValue Duration = FxValue.Constant(-1);
    public float Delay;
    public int Attachment = 1;
    public int[] LodThresholds;

    // NodeTransform (35/36)
    public System.Numerics.Vector3 Offset, Rotation, OffsetVariance, RotationVariance;

    // NodeMovement
    public FxNodeMoveKind NodeMove;
    public FxValue NodeSpeedZ, NodeSpeedMultiplierZ, NodeAccelerationZ, NodeAccelerationMultiplierZ, NodeAccelerationY;
    public FxValue NodeTranslation;
    public FxValue[] NodeSpin; // x, mx, y, my, z, mz

    // Emitter
    public FxEmitterKind Emitter;
    public FxValue Interval, PerEmission, TotalEmissions, MaxConcurrent, Threshold;

    // NodeSelector (201): weights for one random child per emission; null = all children
    public List<int> SelectWeights;

    // EmitterShape
    public FxShapeKind Shape;
    public FxValue ShapeRadius, ShapeSizeX, ShapeSizeY, ShapeSizeZ, ShapeHeight, ShapeDistribution;
    public int Direction;
    public bool EmitInside, CylinderYAxis;

    // DirectionSpread
    public FxSpreadKind Spread;
    public FxValue SpreadAngle, SpreadAngleX, SpreadAngleY, SpreadDistribution;

    // ParticleAttributes (129)
    public FxValue ParticleDuration = FxValue.Constant(-1);
    public int ParticleAttachment = 1;

    // ParticleModifier (131)
    public FxValue ParticleSpeed, ParticleScaleX, ParticleScaleY, ParticleScaleZ, ParticleColor;
    public bool UniformScale;

    // ParticleMovement
    public FxParticleMoveKind ParticleMove;
    public FxValue ParticleGravity, ParticleMoveSpeed, ParticleMoveSpeedMultiplier, ParticleAcceleration, ParticleAccelerationMultiplier;

    // Termination
    public FxTerminationKind Termination;
    public FxValue FadeDuration;

    public List<FxAppearanceDef> Appearances = new();

    /// <summary>
    /// A Basic node's emitter makes particles when the config has an appearance. Lights and other non-particle
    /// appearances get them too: their position and lifetime come from the emitted instance.
    /// </summary>
    public bool EmitsParticles => Appearances.Count > 0;

    public static FxConfigDef Build(FxEffectDef effect, FxrSchema schema, FxrGame game, FXR fxr, FxrNode node, FxrConfig config)
    {
        var def = new FxConfigDef { Source = config };
        def.Actions.AddRange(node.Actions);
        def.Actions.AddRange(config.Actions);

        foreach (var action in def.Actions)
        {
            var view = new FxrActionView(schema, action, game, fxr);
            var d = view.Definition;
            if (d == null)
                continue;
            if (!view.IsTyped)
            {
                effect.UntypedActions++;
                continue;
            }

            switch (d.Slot)
            {
                case "NodeAttributes":
                    def.Duration = FxValue.From(view, "duration") ?? def.Duration;
                    def.Delay = Float(view, "delay");
                    def.Attachment = Int(view, "attachment", 1);
                    break;
                case "LevelsOfDetailThresholds":
                    def.Duration = FxValue.From(view, "duration") ?? def.Duration;
                    def.LodThresholds = Enumerable.Range(0, 5).Select(i => Int(view, $"threshold{i}", 10000)).ToArray();
                    break;
                case "NodeTransform":
                    def.Offset = Vec3(view, "offset");
                    def.Rotation = Vec3(view, "rotation");
                    def.OffsetVariance = Vec3(view, "offsetVariance");
                    def.RotationVariance = Vec3(view, "rotationVariance");
                    break;
                case "NodeMovement":
                    ReadNodeMovement(def, view);
                    break;
                case "Emitter":
                    def.Emitter = d.Name switch
                    {
                        "OneTimeEmitter" => FxEmitterKind.OneTime,
                        "PeriodicEmitter" => FxEmitterKind.Periodic,
                        "EqualDistanceEmitter" => FxEmitterKind.EqualDistance,
                        _ => FxEmitterKind.None,
                    };
                    def.Interval = FxValue.From(view, "interval");
                    def.PerEmission = FxValue.From(view, "perEmission");
                    def.TotalEmissions = FxValue.From(view, "totalEmissions");
                    def.MaxConcurrent = FxValue.From(view, "maxConcurrent");
                    def.Threshold = FxValue.From(view, "threshold");
                    break;
                case "NodeSelector":
                    def.SelectWeights = d.Name == "SelectRandomNode" ? view.GetSection10("weights") : null;
                    break;
                case "EmitterShape":
                    def.Shape = d.Name switch
                    {
                        "DiskEmitterShape" => FxShapeKind.Disk,
                        "RectangleEmitterShape" => FxShapeKind.Rectangle,
                        "SphereEmitterShape" => FxShapeKind.Sphere,
                        "BoxEmitterShape" => FxShapeKind.Box,
                        "CylinderEmitterShape" => FxShapeKind.Cylinder,
                        _ => FxShapeKind.Point,
                    };
                    def.ShapeRadius = Prop(view, "radius");
                    def.ShapeSizeX = Prop(view, "sizeX");
                    def.ShapeSizeY = Prop(view, "sizeY");
                    def.ShapeSizeZ = Prop(view, "sizeZ");
                    def.ShapeHeight = Prop(view, "height");
                    def.ShapeDistribution = Prop(view, "distribution");
                    def.Direction = Int(view, "direction", 0);
                    def.EmitInside = Int(view, "emitInside", 0) != 0;
                    def.CylinderYAxis = Int(view, "yAxis", 0) != 0;
                    break;
                case "DirectionSpread":
                    def.Spread = d.Name switch
                    {
                        "CircularSpread" => FxSpreadKind.Circular,
                        "EllipticalSpread" => FxSpreadKind.Elliptical,
                        "RectangularSpread" => FxSpreadKind.Rectangular,
                        _ => FxSpreadKind.None,
                    };
                    def.SpreadAngle = Prop(view, "angle");
                    def.SpreadAngleX = Prop(view, "angleX");
                    def.SpreadAngleY = Prop(view, "angleY");
                    def.SpreadDistribution = Prop(view, "distribution");
                    break;
                case "ParticleAttributes":
                    def.ParticleDuration = FxValue.From(view, "duration") ?? def.ParticleDuration;
                    def.ParticleAttachment = Int(view, "attachment", 1);
                    break;
                case "ParticleModifier":
                    def.ParticleSpeed = Prop(view, "speed");
                    def.ParticleScaleX = Prop(view, "scaleX");
                    def.ParticleScaleY = Prop(view, "scaleY");
                    def.ParticleScaleZ = Prop(view, "scaleZ");
                    def.ParticleColor = Prop(view, "color");
                    def.UniformScale = Int(view, "uniformScale", 0) != 0;
                    break;
                case "ParticleMovement":
                    ReadParticleMovement(def, view);
                    break;
                case "Termination":
                    def.Termination = d.Name switch
                    {
                        "FadeTermination" => FxTerminationKind.Fade,
                        "InstantTermination" => FxTerminationKind.Instant,
                        _ => FxTerminationKind.Simulate,
                    };
                    def.FadeDuration = Prop(view, "duration");
                    break;
                case "Appearance":
                    def.Appearances.Add(new FxAppearanceDef { Action = action, View = view, Name = d.Name, IsParticle = d.IsParticle });
                    break;
            }
        }

        return def;
    }

    /// <summary>
    /// The Speed and Acceleration families share property names: speedZ (+ multiplier), accelerationZ
    /// (+ multiplier) and accelerationY. Their random-turn, partial-follow and spin variants move like the base
    /// action here; the turns and follows are left out.
    /// </summary>
    private static void ReadNodeMovement(FxConfigDef def, FxrActionView view)
    {
        var name = view.Definition.Name;
        if (name == "NodeTranslation")
        {
            def.NodeTranslation = Prop(view, "translation");
            return;
        }
        if (name == "NodeSpin")
        {
            def.NodeSpin = new[]
            {
                Prop(view, "angularSpeedX"), Prop(view, "angularSpeedMultiplierX"),
                Prop(view, "angularSpeedY"), Prop(view, "angularSpeedMultiplierY"),
                Prop(view, "angularSpeedZ"), Prop(view, "angularSpeedMultiplierZ"),
            };
            return;
        }

        def.NodeMove = name.StartsWith("NodeAcceleration", StringComparison.Ordinal) ? FxNodeMoveKind.Acceleration
            : name.StartsWith("NodeSpeed", StringComparison.Ordinal) ? FxNodeMoveKind.Speed
            : FxNodeMoveKind.None;
        def.NodeSpeedZ = Prop(view, "speedZ");
        def.NodeSpeedMultiplierZ = Prop(view, "speedMultiplierZ");
        def.NodeAccelerationZ = Prop(view, "accelerationZ");
        def.NodeAccelerationMultiplierZ = Prop(view, "accelerationMultiplierZ");
        def.NodeAccelerationY = Prop(view, "accelerationY");
    }

    private static void ReadParticleMovement(FxConfigDef def, FxrActionView view)
    {
        var name = view.Definition.Name;
        def.ParticleMove = name.StartsWith("ParticleAcceleration", StringComparison.Ordinal) ? FxParticleMoveKind.Acceleration
            : name.StartsWith("ParticleSpeed", StringComparison.Ordinal) ? FxParticleMoveKind.Speed
            : FxParticleMoveKind.None;
        def.ParticleGravity = Prop(view, "gravity");
        def.ParticleMoveSpeed = Prop(view, "speed");
        def.ParticleMoveSpeedMultiplier = Prop(view, "speedMultiplier");
        def.ParticleAcceleration = Prop(view, "acceleration");
        def.ParticleAccelerationMultiplier = Prop(view, "accelerationMultiplier");
    }

    // ---------------------------------------------------------------- reading

    private static FxValue Prop(FxrActionView view, string name) =>
        view.Names.Contains(name) ? FxValue.From(view, name) : null;

    private static float Float(FxrActionView view, string name)
    {
        try { return view.Names.Contains(name) ? (float)view.GetValue(name)[0] : 0; }
        catch { return 0; }
    }

    private static int Int(FxrActionView view, string name, int fallback)
    {
        try { return view.Names.Contains(name) ? (int)view.GetValue(name)[0] : fallback; }
        catch { return fallback; }
    }

    private static System.Numerics.Vector3 Vec3(FxrActionView view, string name)
    {
        try
        {
            if (!view.Names.Contains(name))
                return default;
            var v = view.GetValue(name);
            return v.Length >= 3 ? new System.Numerics.Vector3((float)v[0], (float)v[1], (float)v[2]) : default;
        }
        catch
        {
            return default;
        }
    }
}
