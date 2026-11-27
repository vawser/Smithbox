using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// One of this project's GPARAMs, as the analysis sees it. The editor builds these on the UI thread
/// (<c>GparamMergeSources.BuildTargets</c>): an open file's mine is a clone taken there, an unopened one is read on
/// the worker.
/// </summary>
public class GparamMergeTarget
{
    public GparamMergeFileKey Key { get; init; }

    /// <summary>The editor's file entry (FileDictionaryEntry), handed back with the result</summary>
    public object Tag { get; init; }

    /// <summary>Mine: a copy the analysis may keep (the plan's Mine objects point into it). Null with a problem on failure.</summary>
    public Func<(GPARAM Gparam, GparamMergeProblem Problem)> ReadMine { get; init; }

    /// <summary>The game's copy; null GPARAM (no problem) when the game has no such file</summary>
    public Func<(GPARAM Gparam, GparamMergeProblem Problem)> ReadVanilla { get; init; }
}

/// <summary>The plan for one matched file</summary>
public class GparamMergeFileResult
{
    public GparamMergeTarget Target { get; init; }
    public GparamMergeSourceFile SourceFile { get; init; }
    public GparamMergeFilePlan Plan { get; init; }

    /// <summary>Mine as analysed: P3 applies the plan to the live file, checking each item against this</summary>
    public GPARAM Mine { get; init; }

    /// <summary>Ticked in the review (P4): an unticked file is skipped at Apply</summary>
    public bool Included { get; set; } = true;
}

/// <summary>Everything one analysis found</summary>
public class GparamMergeAnalysisResult
{
    public string SourceName { get; init; }

    /// <summary>Matched files whose plan has items</summary>
    public List<GparamMergeFileResult> Files { get; } = new();

    /// <summary>Matched files with nothing to merge</summary>
    public int Identical { get; set; }

    /// <summary>Files only the source has (D2: offered for Add File)</summary>
    public List<GparamMergeSourceFile> Unmatched { get; } = new();

    /// <summary>Files that couldn't be read, matched ambiguously, and the source's own problems</summary>
    public List<GparamMergeProblem> Problems { get; } = new();

    /// <summary>The plans' warnings (version differences)</summary>
    public List<string> Warnings { get; } = new();

    public bool Cancelled { get; set; }

    public int Count(GparamMergeKind kind) => Files.Sum(f => f.Plan.Count(kind));

    public int ItemCount => Files.Sum(f => f.Plan.Items.Count);
}

/// <summary>
/// Matches a source's files to this project's and builds a plan for each (Claude/Documentation/Gparam.md §7.3
/// §4). Read-only and editor-free: runs on a worker; the result goes back to the UI thread.
/// </summary>
public static class GparamMergeAnalysis
{
    /// <summary>
    /// Plans for every source file this project has. A source file is matched by file name and extension (a
    /// transfer export without an extension by file name alone; two candidates is an Ambiguous_File problem).
    /// <paramref name="options"/> applies to every file, except that a file the source doesn't hold whole never
    /// reports removals.
    /// </summary>
    public static GparamMergeAnalysisResult Run(IReadOnlyList<GparamMergeTarget> targets, GparamMergeSource source,
        GparamMergeOptions options, CancellationToken cancel = default)
    {
        options ??= new GparamMergeOptions();
        var result = new GparamMergeAnalysisResult { SourceName = source.Name };

        if (!source.IsValid)
        {
            result.Problems.Add(source.Error);
            return result;
        }

        result.Problems.AddRange(source.Problems);

        var byKey = new Dictionary<GparamMergeFileKey, GparamMergeTarget>();
        foreach (var target in targets)
            byKey.TryAdd(target.Key, target);

        foreach (var file in source.Files)
        {
            if (cancel.IsCancellationRequested)
            {
                result.Cancelled = true;
                return result;
            }

            var target = Match(file.Key, targets, byKey, out var ambiguous);

            if (ambiguous)
            {
                result.Problems.Add(new GparamMergeProblem("Ambiguous_File", file.Key.ToString()));
                continue;
            }

            if (target == null)
            {
                result.Unmatched.Add(file);
                continue;
            }

            var (theirs, theirProblem) = file.Read();
            if (theirs == null)
            {
                result.Problems.Add(theirProblem ?? new GparamMergeProblem("Read_Failed", file.Path));
                continue;
            }

            var (mine, mineProblem) = target.ReadMine();
            if (mine == null)
            {
                result.Problems.Add(mineProblem ?? new GparamMergeProblem("Read_Failed", target.Key.ToString()));
                continue;
            }

            // A missing vanilla copy is normal (a file the project added); a broken one is reported and the file is
            // merged two-way, as if the game had none
            var (vanilla, vanillaProblem) = target.ReadVanilla();
            if (vanillaProblem != null)
                result.Problems.Add(vanillaProblem);

            var fileOptions = new GparamMergeOptions
            {
                SourceIsWhole = options.SourceIsWhole && file.Whole,
                ConflictMode = options.ConflictMode,
                IncludeRemovals = options.IncludeRemovals
            };

            GparamMergeFilePlan plan;
            try
            {
                plan = GparamMerge.BuildPlan(target.Key.ToString(), vanilla, mine, theirs, fileOptions);
            }
            catch (Exception e)
            {
                result.Problems.Add(new GparamMergeProblem("Merge_Failed", target.Key.ToString(), e.Message));
                continue;
            }

            result.Warnings.AddRange(plan.Warnings);

            if (plan.Items.Count == 0)
            {
                result.Identical++;
                continue;
            }

            result.Files.Add(new GparamMergeFileResult
            {
                Target = target,
                SourceFile = file,
                Plan = plan,
                Mine = mine
            });
        }

        return result;
    }

    private static GparamMergeTarget Match(GparamMergeFileKey key, IReadOnlyList<GparamMergeTarget> targets,
        Dictionary<GparamMergeFileKey, GparamMergeTarget> byKey, out bool ambiguous)
    {
        ambiguous = false;

        if (key.Extension != null)
            return byKey.GetValueOrDefault(key);

        var candidates = targets.Where(t => key.Matches(t.Key)).Take(2).ToList();
        ambiguous = candidates.Count > 1;
        return candidates.Count == 1 ? candidates[0] : null;
    }
}
