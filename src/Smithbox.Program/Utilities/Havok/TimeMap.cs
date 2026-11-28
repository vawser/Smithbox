#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.Utilities;

/// <summary>
///     A change of timing as a piecewise-linear map from old times to new ones (seconds): straight lines between the
///     points, and beyond the first and last points the end segments carried on. Both old and new times rise from
///     point to point, so the map can be inverted and composed. Used for a clip's time act events after a time
///     operation (whole-clip trim / speed, or a range retime, hold or removal) in the Animation Editor, and sent to
///     the Time Act Editor as <see cref="ToCommand" />.
/// </summary>
public sealed class TimeMap
{
    public IReadOnlyList<(float Old, float New)> Points { get; }

    private TimeMap(List<(float Old, float New)> points)
    {
        Points = points;
    }

    /// <summary>A map through these points (at least two, old and new both rising); null when they don't</summary>
    public static TimeMap? From(IEnumerable<(float Old, float New)> points)
    {
        var list = points.OrderBy(p => p.Old).ToList();

        // Points closer than a microsecond are one point
        var merged = new List<(float Old, float New)>();
        foreach (var p in list)
        {
            if (merged.Count > 0 && MathF.Abs(p.Old - merged[^1].Old) < 1e-6f)
                continue;

            merged.Add(p);
        }

        if (merged.Count < 2)
            return null;

        for (var i = 1; i < merged.Count; i++)
        {
            if (merged[i].New <= merged[i - 1].New)
                return null;
        }

        return new TimeMap(merged);
    }

    /// <summary>t → t × scale + offset (a whole-clip trim or stretch)</summary>
    public static TimeMap Affine(float scale, float offset) => new([(0, offset), (1, scale + offset)]);

    public static TimeMap Identity { get; } = Affine(1, 0);

    public float Map(float t)
    {
        var i = SegmentFor(Points.Select(p => p.Old).ToList(), t);
        var (a, b) = (Points[i], Points[i + 1]);
        return a.New + (t - a.Old) * (b.New - a.New) / (b.Old - a.Old);
    }

    public float Unmap(float t)
    {
        var i = SegmentFor(Points.Select(p => p.New).ToList(), t);
        var (a, b) = (Points[i], Points[i + 1]);
        return a.Old + (t - a.New) * (b.Old - a.Old) / (b.New - a.New);
    }

    // The segment a time falls on (the first or last one beyond the ends)
    private static int SegmentFor(List<float> times, float t)
    {
        for (var i = 1; i < times.Count - 1; i++)
        {
            if (t < times[i])
                return i - 1;
        }

        return times.Count - 2;
    }

    /// <summary>This map, then <paramref name="next" />: t → next(this(t))</summary>
    public TimeMap Then(TimeMap next)
    {
        var points = Points.Select(p => (p.Old, next.Map(p.New)))
            .Concat(next.Points.Select(p => (Unmap(p.Old), p.New)));
        return From(points) ?? this;
    }

    /// <summary>Whether every point maps to itself, within a microsecond</summary>
    public bool IsIdentity => Points.All(p => MathF.Abs(p.Old - p.New) < 1e-6f);

    /// <summary>Whether this is one straight line (scale and offset)</summary>
    public bool IsAffine(out float scale, out float offset)
    {
        var (a, b) = (Points[0], Points[^1]);
        scale = (b.New - a.New) / (b.Old - a.Old);
        offset = a.New - a.Old * scale;
        var (s, o) = (scale, offset);
        return Points.All(p => MathF.Abs(p.Old * s + o - p.New) < 1e-5f);
    }

    /// <summary>"old:new;old:new;…" in seconds, invariant culture, round-trip precision</summary>
    public string ToCommand()
    {
        var culture = CultureInfo.InvariantCulture;
        return string.Join(";", Points.Select(p => $"{p.Old.ToString("R", culture)}:{p.New.ToString("R", culture)}"));
    }

    public static TimeMap? Parse(string text)
    {
        var culture = CultureInfo.InvariantCulture;
        var points = new List<(float, float)>();
        foreach (var part in text.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split(':');
            if (pair.Length != 2 || !float.TryParse(pair[0], NumberStyles.Float, culture, out var o) ||
                !float.TryParse(pair[1], NumberStyles.Float, culture, out var n))
                return null;

            points.Add((o, n));
        }

        return From(points);
    }

    /// <summary>The points in frames, "old → new" each, for showing</summary>
    public string Describe(float frameDuration)
    {
        if (frameDuration <= 0)
            return "";

        return string.Join(", ", Points.Select(p => $"{p.Old / frameDuration:0.#} → {p.New / frameDuration:0.#}"));
    }
}
