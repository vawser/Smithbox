using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>What the analysis reads from: the kind and the project, folder or file</summary>
public record TimeActMergeRequest(TimeActMergeSourceType Type, ProjectEntry OtherProject, string Path,
    TimeActMergeOptions Options);

/// <summary>
/// Runs a Time Act Merge analysis off the UI thread (Claude/Documentation/Time Act.md §5.2). The targets,
/// and a lone .tae's selected source, are taken on the UI thread before anything awaits; the source is read and the
/// plans built on a worker; the result comes back through a queue the view drains every frame, so it arrives while the
/// panel is closed too. A result is dropped if another analysis was started or the analysis was cleared meanwhile.
/// Holds the source (its file system, which Add Source reads from) until cleared.
/// </summary>
public class TimeActMergeAnalyser
{
    private readonly TimeActEditorView View;
    private readonly ProjectEntry Project;

    private readonly ConcurrentQueue<Action> MainThreadQueue = new();
    private CancellationTokenSource Cancellation;
    private int RunId;

    public TimeActMergeAnalyser(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool IsAnalysing { get; private set; }

    /// <summary>The request the result was built from</summary>
    public TimeActMergeRequest Request { get; private set; }

    public TimeActMergeSource Source { get; private set; }

    public TimeActMergeAnalysisResult Result { get; private set; }

    /// <summary>Set when the analysis itself failed (not a problem the result lists)</summary>
    public string Error { get; private set; }

    /// <summary>Raised on the UI thread when a result (or an error) arrives</summary>
    public event Action Finished;

    /// <summary>Takes finished analyses; called every frame by the view</summary>
    public void ProcessPending()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            action();
    }

    /// <summary>Starts an analysis, dropping the one before. Call on the UI thread.</summary>
    public void Start(TimeActMergeRequest request)
    {
        Clear();

        var id = ++RunId;
        var cancel = new CancellationTokenSource();
        Cancellation = cancel;

        IsAnalysing = true;
        Request = request;

        // On the UI thread: the live time acts and the selection are only read here. A loose file is matched here too,
        // as a lone .tae looks up its time act in the selected source's list
        var targets = TimeActMergeSources.BuildTargets(Project);
        var loose = request.Type == TimeActMergeSourceType.LooseFile
            ? TimeActMergeSources.FromLooseFile(Project, request.Path, View.Selection.SelectedTimeActContainerWrapper)
            : null;

        _ = Task.Run(async () =>
        {
            TimeActMergeSource source = loose;
            TimeActMergeAnalysisResult result = null;
            string error = null;

            try
            {
                source ??= request.Type switch
                {
                    TimeActMergeSourceType.Project => await TimeActMergeSources.FromProject(Project, request.OtherProject, targets),
                    _ => TimeActMergeSources.FromModFolder(Project, request.Path, targets)
                };

                result = TimeActMergeAnalysis.Run(targets, source, request.Options, cancel.Token);
            }
            catch (Exception e)
            {
                error = e.Message;
            }

            MainThreadQueue.Enqueue(() =>
            {
                if (id != RunId)
                {
                    source?.Dispose();
                    return;
                }

                IsAnalysing = false;
                Source = source;
                Result = result;
                Error = error;

                if (error != null)
                    Smithbox.LogError(this, LOC.Get("TAE_Merge_Analysis_Failed", error));

                Finished?.Invoke();
            });
        }, cancel.Token);
    }

    /// <summary>Drops the analysis (and any still running) and releases the source</summary>
    public void Clear()
    {
        RunId++;
        Cancellation?.Cancel();
        Cancellation = null;

        IsAnalysing = false;
        Request = null;
        Result = null;
        Error = null;

        Source?.Dispose();
        Source = null;
    }
}
