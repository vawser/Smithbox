using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor.Upgrader;

/// <summary>
/// Upgrades across several version steps: finds the diffs between two versions, rebuilds each vanilla version
/// from the newest one (the game's regulation), and merges the mod one step at a time.
/// </summary>
public static class RegulationUpgradeChain
{
    /// <summary>
    /// The diffs from <paramref name="from"/> to <paramref name="to"/>, oldest first, or null when a step is missing
    /// (<paramref name="missingAt"/> is then the version no diff leads to). Earlier entries in
    /// <paramref name="diffs"/> win when two diffs cover the same step (bundled before user generated).
    /// </summary>
    public static List<RegulationDiff> FindChain(IEnumerable<RegulationDiff> diffs, ulong from, ulong to, out ulong missingAt)
    {
        missingAt = 0;
        var byTo = new Dictionary<ulong, List<RegulationDiff>>();

        foreach (var diff in diffs)
        {
            if (!byTo.TryGetValue(diff.ToVersion, out var list))
                byTo[diff.ToVersion] = list = new List<RegulationDiff>();

            if (!list.Any(d => d.FromVersion == diff.FromVersion))
                list.Add(diff);
        }

        var chain = new List<RegulationDiff>();
        var current = to;

        while (current > from)
        {
            // The step that ends here and starts closest above the target
            var step = byTo.TryGetValue(current, out var candidates)
                ? candidates.Where(d => d.FromVersion >= from).OrderByDescending(d => d.FromVersion).FirstOrDefault()
                : null;

            if (step == null)
            {
                missingAt = current;
                return null;
            }

            chain.Add(step);
            current = step.FromVersion;
        }

        if (current != from)
        {
            missingAt = current;
            return null;
        }

        chain.Reverse();
        return chain;
    }

    /// <summary>
    /// Rebuilds every vanilla version of the chain from the newest one. Index 0 is the oldest version, the last
    /// index is <paramref name="newest"/> itself.
    /// </summary>
    public static List<Dictionary<string, Param>> RebuildVersions(
        Dictionary<string, Param> newest, List<RegulationDiff> chain, RegulationLoadContext context)
    {
        var versions = new List<Dictionary<string, Param>> { newest };
        var current = newest;

        for (var i = chain.Count - 1; i >= 0; i--)
        {
            current = RegulationDiffApplier.RebuildOlder(current, chain[i], context);
            versions.Insert(0, current);
        }

        return versions;
    }

    /// <summary>
    /// Merges the mod through every step. <paramref name="versions"/> comes from <see cref="RebuildVersions"/>;
    /// its last entry should be the params the project uses as its vanilla bank.
    /// </summary>
    public static Dictionary<string, Param> Upgrade(
        Dictionary<string, Param> mod, List<RegulationDiff> chain, List<Dictionary<string, Param>> versions,
        List<UpgradeConflict> conflicts)
    {
        if (versions.Count != chain.Count + 1)
            throw new ArgumentException("There must be one more version than steps.");

        var current = mod;

        for (var i = 0; i < chain.Count; i++)
            current = RegulationStepMerge.Merge(current, versions[i], versions[i + 1], chain[i], conflicts);

        return current;
    }
}
