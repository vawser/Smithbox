using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

/// <summary>A conflict the Full Mod merge has to resolve (one per conflicting merge item)</summary>
public sealed class GparamFullModConflict
{
    /// <summary>"[prefix -> ]group -> field -> ID" (slot keys shown as "key [2]")</summary>
    public string InternalPath { get; init; }

    public string ExistingValue { get; init; }
    public string IncomingValue { get; init; }

    /// <summary>Both_Changed, Both_Added, Deleted_Changed or Type_Mismatch</summary>
    public string Reason { get; init; }
}

/// <summary>
/// Gparam Merge for the Param Editor's Full Mod merge (Claude/Documentation/Gparam Merge Plan.md D4, P5): merges a
/// later mod's GPARAM into the earlier result, three-way against the game's copy (two-way when there is none).
/// Their changes and additions are taken, each conflict is decided by the caller (strategy or saved resolution), and
/// what they removed is never removed (a later mod leaving something out isn't asking for it to go). No editor
/// dependencies.
/// </summary>
public static class GparamFullModMerge
{
    /// <summary>A loose GPARAM by its name (.gparam or .fltparam, DCX or not). A .gparambnd is a binder: its entries are.</summary>
    public static bool IsGparamFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return GparamMergeFileKey.FromPath(path).Extension is "gparam" or "fltparam";
    }

    /// <summary>
    /// Merge <paramref name="theirsBytes"/> into <paramref name="mineBytes"/>. <paramref name="takeTheirs"/> is asked
    /// for each conflict (true: theirs). <paramref name="merged"/> is written with mine's compression; unchanged bytes
    /// when nothing was taken. False (with <paramref name="error"/>) when a side doesn't read as a GPARAM, so the
    /// caller can fall back to a whole-file conflict.
    /// </summary>
    public static bool TryMerge(byte[] mineBytes, byte[] theirsBytes, GPARAM vanilla, string name, string internalPrefix,
        Func<GparamFullModConflict, bool> takeTheirs, out byte[] merged, out int applied, out string error)
    {
        merged = null;
        applied = 0;
        error = null;

        GPARAM mine, theirs;
        try
        {
            // Read from the stored bytes, so writing keeps the file's own DCX type
            mine = GPARAM.Read(mineBytes);
            theirs = GPARAM.Read(theirsBytes);
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }

        try
        {
            var plan = GparamMerge.BuildPlan(name, vanilla, mine.Clone(), theirs,
                new GparamMergeOptions { SourceIsWhole = true, ConflictMode = GparamMergeConflictMode.KeepMine });

            var chosen = new List<GparamMergeItem>();

            foreach (var item in plan.Items)
            {
                switch (item.Kind)
                {
                    case GparamMergeKind.Added:
                    case GparamMergeKind.Changed:
                        chosen.Add(item);
                        break;

                    case GparamMergeKind.Conflict:
                        var conflict = new GparamFullModConflict
                        {
                            InternalPath = string.IsNullOrEmpty(internalPrefix) ? item.Path : $"{internalPrefix} -> {item.Path}",
                            ExistingValue = DescribeSide(item.Mine, item.Level),
                            IncomingValue = DescribeSide(item.Theirs, item.Level),
                            Reason = item.Reason
                        };

                        if (takeTheirs(conflict))
                            chosen.Add(item);
                        break;

                    // RemovedByThem: never applied
                }
            }

            if (chosen.Count == 0)
            {
                merged = mineBytes;
                return true;
            }

            var result = GparamMerge.Apply(mine, chosen);
            applied = result.Applied;
            merged = mine.Write();
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    /// <summary>One side of an item as plain text for a conflict table: "-" when absent</summary>
    public static string DescribeSide(object side, GparamMergeLevel level)
    {
        switch (side)
        {
            case null:
                return "-";

            case GPARAM g:
                return string.Format(CultureInfo.InvariantCulture, "Unk0D {0}, Unk40 {1}, Unk50 {2}", g.Unk0D, g.Unk40, g.Unk50);

            case GPARAM.Param p when level == GparamMergeLevel.GroupProperties:
                return p.Name ?? "";

            case GPARAM.Param p:
                return string.Format(CultureInfo.InvariantCulture, "{0} fields", p.Fields.Count);

            case GPARAM.IField f when level == GparamMergeLevel.FieldProperties:
                return string.Format(CultureInfo.InvariantCulture, "{0} (Unk {1})", f.Name ?? "", GparamMerge.FieldUnkOf(f));

            case GPARAM.IField f:
                return string.Format(CultureInfo.InvariantCulture, "{0} x{1}", f.GetType().Name, f.Values.Count);

            case IReadOnlyList<GPARAM.IFieldValue> keyframes:
                if (keyframes.Count == 1 && keyframes[0].TimeOfDay == 0)
                    return FormatValue(keyframes[0].Value);

                return string.Join("; ", keyframes.Select(k =>
                    string.Format(CultureInfo.InvariantCulture, "{0}: {1}", k.TimeOfDay, FormatValue(k.Value))));
        }

        return side.ToString();
    }

    /// <summary>A value as invariant text: floats to 3 decimals, vectors in brackets, colours as #RRGGBBAA</summary>
    public static string FormatValue(object value) => value switch
    {
        float f => f.ToString("0.###", CultureInfo.InvariantCulture),
        double d => d.ToString("0.###", CultureInfo.InvariantCulture),
        Vector2 v => string.Format(CultureInfo.InvariantCulture, "({0:0.###}, {1:0.###})", v.X, v.Y),
        Vector3 v => string.Format(CultureInfo.InvariantCulture, "({0:0.###}, {1:0.###}, {2:0.###})", v.X, v.Y, v.Z),
        Vector4 v => string.Format(CultureInfo.InvariantCulture, "({0:0.###}, {1:0.###}, {2:0.###}, {3:0.###})", v.X, v.Y, v.Z, v.W),
        System.Drawing.Color c => $"#{c.R:X2}{c.G:X2}{c.B:X2}{c.A:X2}",
        null => "",
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? ""
    };
}
