using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public enum FxrConversionKind
    {
        /// <summary>Displayed = raw * Factor (one component, or all when Component is -1).</summary>
        Scale,
        /// <summary>Displayed = !raw (bool fields).</summary>
        Invert,
    }

    /// <summary>
    /// One of fxr-main's display conversions (ActionDataConversion in fxr.ts), e.g. frames to seconds or a
    /// mirrored X axis. Applied only by FxrActionView's converted getters/setters; raw words never change
    /// unless a value is set.
    /// </summary>
    public sealed class FxrConversion
    {
        public string Action { get; init; }
        public string Name { get; init; }
        public FxrConversionKind Kind { get; init; }
        public double Factor { get; init; } = 1;
        /// <summary>Vector component to convert, or -1 for every component.</summary>
        public int Component { get; init; } = -1;
        /// <summary>Games it applies to; null for all.</summary>
        public FxrGame[] Games { get; init; }
        /// <summary>Games it does not apply to.</summary>
        public FxrGame[] ExceptGames { get; init; }

        public bool AppliesTo(FxrGame game) =>
            (Games == null || Array.IndexOf(Games, game) >= 0) && (ExceptGames == null || Array.IndexOf(ExceptGames, game) < 0);
    }

    /// <summary>
    /// The invertible subset of fxr-main's 23 ActionDataConversion entries (fxr-main 32.1.3).
    /// Left out on purpose:
    /// - GPUStandard(Correct)Particle uniform scale: folds Y sizes into X and zeroes Y on write (loses data).
    /// - PointLight/SpotLight DS3 multipliers: values DS3 files do not store, folded into colour alpha on write.
    /// - RichModel ER UV: regroups U and V into one vector without changing values; the schema names U and V.
    /// - StateConfigMap / StaticNodeTransform / RandomNodeTransform minify, and fallbacks: write-time rewrites.
    /// </summary>
    public static class FxrConversions
    {
        private const double RadToDeg = 180 / Math.PI;
        private static readonly FxrGame[] DS3 = { FxrGame.DS3 };

        public static IReadOnlyList<FxrConversion> All { get; } = Build();

        private static readonly Dictionary<(string, string), List<FxrConversion>> byName = Index();

        /// <summary>The conversions for one value of an action in a game (usually zero or one).</summary>
        public static IEnumerable<FxrConversion> For(string action, string name, FxrGame game)
        {
            if (action == null || !byName.TryGetValue((action, name), out var list)) yield break;
            foreach (var c in list)
                if (c.AppliesTo(game)) yield return c;
        }

        private static Dictionary<(string, string), List<FxrConversion>> Index()
        {
            var d = new Dictionary<(string, string), List<FxrConversion>>();
            foreach (var c in All)
            {
                if (!d.TryGetValue((c.Action, c.Name), out var list)) d[(c.Action, c.Name)] = list = new();
                list.Add(c);
            }
            return d;
        }

        private static List<FxrConversion> Build()
        {
            var list = new List<FxrConversion>();
            void Scale(string action, string name, double factor, int component = -1, FxrGame[] games = null, FxrGame[] except = null) =>
                list.Add(new FxrConversion { Action = action, Name = name, Kind = FxrConversionKind.Scale, Factor = factor, Component = component, Games = games, ExceptGames = except });
            void Invert(string action, string name) =>
                list.Add(new FxrConversion { Action = action, Name = name, Kind = FxrConversionKind.Invert });

            // Mirrored X axis
            Scale("StaticNodeTransform", "offset", -1, 0);
            Scale("StaticNodeTransform", "rotation", -1, 0);
            Scale("RandomNodeTransform", "offset", -1, 0);

            // turnInterval is stored in 1/50 s steps; partial follow stores followRotation inverted
            foreach (var a in new[] { "ParticleSpeedRandomTurns", "NodeAccelerationRandomTurns", "ParticleAccelerationRandomTurns", "NodeSpeedRandomTurns" })
                Scale(a, "turnInterval", 1.0 / 50);
            foreach (var a in new[] { "ParticleSpeedPartialFollow", "ParticleAccelerationPartialFollow", "NodeAccelerationPartialFollow", "NodeSpeedPartialFollow" })
            {
                Scale(a, "turnInterval", 1.0 / 50);
                Invert(a, "followRotation");
            }

            // Radians to degrees
            Scale("Distortion", "stirSpeed", RadToDeg);
            Scale("GPUStandardParticle", "particleAngularAccelerationZ", RadToDeg);
            Scale("GPUStandardCorrectParticle", "particleAngularAccelerationZ", RadToDeg);

            // Frames (30 fps) to seconds
            Scale("PointLight", "fadeOutTime", 1.0 / 30);
            Scale("SpotLight", "fadeOutTime", 1.0 / 30);
            Scale("WindForce", "fadeOutTime", 1.0 / 30);
            Scale("GravityForce", "fadeOutTime", 1.0 / 30);
            Scale("TurbulenceForce", "fadeOutTime", 1.0 / 30);
            Scale("Unk10500", "initialSimulationTime", 1.0 / 30, except: new[] { FxrGame.AC6 });
            foreach (var a in new[] { "GPUStandardParticle", "GPUStandardCorrectParticle" })
                foreach (var n in new[] { "emissionIntervalMin", "emissionIntervalMax", "particleDuration", "particleRandomTurnIntervalMax" })
                    Scale(a, n, 1.0 / 30);
            foreach (var a in new[] { "GPUSparkParticle", "GPUSparkCorrectParticle" })
                foreach (var n in new[] { "emissionInterval", "emissionIntervalMin", "emissionIntervalMax", "particleDuration" })
                    Scale(a, n, 1.0 / 30);

            // SpotLight light strength: x0.1 multipliers (not DS3), DS3 colours in 0-255
            Scale("SpotLight", "diffuseMultiplier", 0.1, except: DS3);
            Scale("SpotLight", "specularMultiplier", 0.1, except: DS3);
            Scale("SpotLight", "diffuseColor", 1.0 / 255, games: DS3);
            Scale("SpotLight", "specularColor", 1.0 / 255, games: DS3);

            return list;
        }

        /// <summary>Raw to displayed, in place.</summary>
        public static void ToDisplay(IEnumerable<FxrConversion> conversions, double[] values)
        {
            foreach (var c in conversions) Apply(c, values, false);
        }

        /// <summary>Displayed to raw, in place.</summary>
        public static void FromDisplay(IEnumerable<FxrConversion> conversions, double[] values)
        {
            foreach (var c in conversions) Apply(c, values, true);
        }

        private static void Apply(FxrConversion c, double[] values, bool inverse)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (c.Component >= 0 && c.Component != i) continue;
                values[i] = c.Kind switch
                {
                    FxrConversionKind.Scale => inverse ? values[i] / c.Factor : values[i] * c.Factor,
                    FxrConversionKind.Invert => values[i] != 0 ? 0 : 1,
                    _ => values[i],
                };
            }
        }

        /// <summary>The scale factor applied to a property's values (curves), or 1.</summary>
        public static double CurveFactor(IEnumerable<FxrConversion> conversions, int component)
        {
            double f = 1;
            foreach (var c in conversions)
                if (c.Kind == FxrConversionKind.Scale && (c.Component < 0 || c.Component == component)) f *= c.Factor;
            return f;
        }
    }
}
