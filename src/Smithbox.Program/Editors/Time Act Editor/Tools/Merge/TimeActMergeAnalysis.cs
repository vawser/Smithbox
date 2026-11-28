using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// One of this project's sources, as the analysis sees it. The editor builds these on the UI thread
/// (<c>TimeActMergeSources.BuildTargets</c>): an open source's time acts are copied there (unsaved edits included,
/// and the worker never reads a live object); an unopened one is read on the worker.
/// </summary>
public class TimeActMergeTarget
{
    /// <summary><see cref="TimeActMergeSourceKey"/> of the source's path</summary>
    public string Key { get; init; }

    /// <summary>The source's path as the locator has it</summary>
    public string Path { get; init; }

    public string DisplayName { get; init; }

    /// <summary>The editor's source (TimeActContainerWrapper), handed back with the result</summary>
    public object Tag { get; init; }

    /// <summary>
    /// Mine: copies the analysis may keep (the plan's Mine objects point into them, and each one's Tag is the
    /// editor's TimeActEntry). Null with a problem on failure.
    /// </summary>
    public Func<(List<TimeActMergeTimeAct> TimeActs, TimeActMergeProblem Problem)> ReadMine { get; init; }

    /// <summary>The game's copy; null (no problem) when the game has no such source</summary>
    public Func<(List<TimeActMergeTimeAct> TimeActs, TimeActMergeProblem Problem)> ReadVanilla { get; init; }

    /// <summary>
    /// Whether mine was read on the worker (an unopened source): Apply hands that list to the source, as Mass Edit
    /// does, since saving writes the source's list as the whole binder (P4)
    /// </summary>
    public bool MineReadForMerge { get; init; }

    /// <summary>
    /// The clip items against a source file of theirs (Clip level, unticked defaults applied by the analysis), or
    /// null when the editor doesn't compare clips for this source. Run on the worker.
    /// </summary>
    public Func<TimeActMergeSourceFile, (List<TimeActMergeItem> Items, TimeActMergeProblem Problem)> PlanClips { get; init; }
}

/// <summary>The plan for one matched source</summary>
public class TimeActMergeSourceResult
{
    public TimeActMergeTarget Target { get; init; }
    public TimeActMergeSourceFile SourceFile { get; init; }
    public TimeActMergeSourcePlan Plan { get; init; }

    /// <summary>Mine as analysed: Apply (P4) checks each item against these</summary>
    public List<TimeActMergeTimeAct> Mine { get; init; }

    /// <summary>Ticked in the review (P5): an unticked source is skipped at Apply</summary>
    public bool Included { get; set; } = true;
}

/// <summary>Everything one analysis found</summary>
public class TimeActMergeAnalysisResult
{
    public string SourceName { get; init; }

    /// <summary>Matched sources whose plan has items</summary>
    public List<TimeActMergeSourceResult> Sources { get; } = new();

    /// <summary>Matched sources with nothing to merge</summary>
    public int Identical { get; set; }

    /// <summary>Sources only the other side has (offered for Add Source, P4)</summary>
    public List<TimeActMergeSourceFile> Unmatched { get; } = new();

    /// <summary>Sources that couldn't be read or matched, and the plans' problems, prefixed with their source</summary>
    public List<TimeActMergeProblem> Problems { get; } = new();

    public bool Cancelled { get; set; }

    public int Count(TimeActMergeKind kind) => Sources.Sum(s => s.Plan.Count(kind));

    public int ItemCount => Sources.Sum(s => s.Plan.Items.Count);
}

/// <summary>
/// Matches a source's files to this project's sources and builds a plan for each (Claude/Documentation/Time Act
/// Merge Plan.md §3, §5). Read-only and editor-free: runs on a worker; the result goes back to the UI thread.
/// </summary>
public static class TimeActMergeAnalysis
{
    /// <summary>
    /// Plans for every source file this project has: matched by path, a loose file by file name (two candidates is
    /// an Ambiguous_Source problem), a lone .tae into its target. <paramref name="options"/> applies to every source,
    /// except that one the other side doesn't hold whole never reports removals.
    /// </summary>
    public static TimeActMergeAnalysisResult Run(IReadOnlyList<TimeActMergeTarget> targets, TimeActMergeSource source,
        TimeActMergeOptions options, CancellationToken cancel = default)
    {
        options ??= new TimeActMergeOptions();
        var result = new TimeActMergeAnalysisResult { SourceName = source.Name };

        if (!source.IsValid)
        {
            result.Problems.Add(source.Error);
            return result;
        }

        result.Problems.AddRange(source.Problems);

        var byKey = new Dictionary<string, TimeActMergeTarget>();
        foreach (var target in targets)
            byKey.TryAdd(target.Key, target);

        foreach (var file in source.Files)
        {
            if (cancel.IsCancellationRequested)
            {
                result.Cancelled = true;
                return result;
            }

            var target = Match(file, targets, byKey, out var ambiguous);

            if (ambiguous)
            {
                result.Problems.Add(new TimeActMergeProblem("Ambiguous_Source", file.Path));
                continue;
            }

            if (target == null)
            {
                result.Unmatched.Add(file);
                continue;
            }

            try
            {
                Analyse(result, target, file, options);
            }
            catch (Exception e)
            {
                result.Problems.Add(new TimeActMergeProblem("Merge_Failed", target.DisplayName, e.Message));
            }
        }

        return result;
    }

    private static void Analyse(TimeActMergeAnalysisResult result, TimeActMergeTarget target, TimeActMergeSourceFile file,
        TimeActMergeOptions options)
    {
        var (theirs, theirProblem) = file.Read();
        if (theirs == null || theirs.Count == 0)
        {
            result.Problems.Add(theirProblem ?? new TimeActMergeProblem("No_Time_Acts", file.Path));
            return;
        }

        var (mine, mineProblem) = target.ReadMine();
        if (mine == null)
        {
            result.Problems.Add(mineProblem ?? new TimeActMergeProblem("Read_Failed", target.DisplayName));
            return;
        }

        // None is normal (a source the project added); a broken one is reported and the source merged two-way
        var (vanilla, vanillaProblem) = target.ReadVanilla();
        if (vanillaProblem != null)
            result.Problems.Add(vanillaProblem);

        if (file.OnlyBinderID is { } only)
        {
            // A lone .tae: that time act alone, on every side
            theirs = theirs.Where(e => e.BinderID == only).ToList();
            vanilla = vanilla?.Where(e => e.BinderID == only).ToList();
            var mineOnly = mine.Where(e => e.BinderID == only).ToList();

            if (mineOnly.Count == 0)
            {
                result.Problems.Add(new TimeActMergeProblem("No_Matching_Time_Act", file.Path, target.DisplayName));
                return;
            }

            mine = mineOnly;
        }

        var sourceOptions = new TimeActMergeOptions
        {
            SourceIsWhole = options.SourceIsWhole && file.Whole,
            ConflictMode = options.ConflictMode,
            IncludeRemovals = options.IncludeRemovals,
            IncludeClips = options.IncludeClips
        };

        var plan = TimeActMerge.BuildPlan(target.DisplayName, vanilla, mine, theirs, sourceOptions);

        // The plan's problems name a time act or animation: the source goes in front
        foreach (var problem in plan.Problems)
            result.Problems.Add(problem with { Subject = $"{target.DisplayName} -> {problem.Subject}" });

        // Its clips, but not for a lone .tae, which holds none
        if (options.IncludeClips && file.OnlyBinderID == null && target.PlanClips != null)
        {
            var (clips, clipProblem) = target.PlanClips(file);

            if (clipProblem != null)
                result.Problems.Add(clipProblem);

            if (clips != null)
            {
                TimeActMerge.ApplyDefaults(clips, sourceOptions);
                plan.Items.AddRange(clips);
            }
        }

        if (plan.Items.Count == 0)
        {
            result.Identical++;
            return;
        }

        result.Sources.Add(new TimeActMergeSourceResult
        {
            Target = target,
            SourceFile = file,
            Plan = plan,
            Mine = mine
        });
    }

    private static TimeActMergeTarget Match(TimeActMergeSourceFile file, IReadOnlyList<TimeActMergeTarget> targets,
        Dictionary<string, TimeActMergeTarget> byKey, out bool ambiguous)
    {
        ambiguous = false;

        if (file.TargetKey != null)
            return byKey.GetValueOrDefault(file.TargetKey);

        if (!file.ByFileName)
            return byKey.GetValueOrDefault(file.Key);

        var candidates = targets.Where(t => TimeActMergeSourceKey.FileName(t.Key) == file.Key).Take(2).ToList();
        ambiguous = candidates.Count > 1;
        return candidates.Count == 1 ? candidates[0] : null;
    }
}
