using Hexa.NET.ImGui;
using System.Collections.Concurrent;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// A scan over event files on a background task, for the Called By and Search tools (Tool UI Styling.md, Long work):
/// the file list and inputs are snapshotted on the main thread, hits come back through a queue emptied while the tool
/// draws, progress is shown with a Cancel button, and starting again cancels the run in progress. Files not read yet
/// are read by the task (EmevdFileWrapper.EnsureLoaded is locked for this).
/// </summary>
public class EmevdScan
{
    private readonly EmevdEditorView View;
    private readonly string Id;

    private CancellationTokenSource Cts;
    private readonly ConcurrentQueue<EmevdHit> Queue = new();

    private volatile int Done;
    private volatile int Total;
    private volatile bool Running;

    public List<EmevdHit> Results { get; } = new();

    /// <summary>What the results are for, e.g. "calls to event 90005200"</summary>
    public string Subject { get; private set; }

    public bool HasRun { get; private set; }

    public bool IsRunning => Running;

    public EmevdScan(EmevdEditorView view, string id)
    {
        View = view;
        Id = id;
    }

    public void Start(string subject, List<EmevdFileWrapper> files, Func<EmevdFileWrapper, IEnumerable<EmevdHit>> work)
    {
        Cancel();
        Results.Clear();
        Queue.Clear();

        Subject = subject;
        HasRun = true;
        Done = 0;
        Total = files.Count;
        Running = true;

        var cts = new CancellationTokenSource();
        Cts = cts;

        Task.Run(() =>
        {
            try
            {
                foreach (var file in files)
                {
                    if (cts.IsCancellationRequested)
                        break;

                    if (file.EnsureLoaded())
                    {
                        foreach (var hit in work(file))
                            Queue.Enqueue(hit);
                    }

                    Done++;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("EMEVD_Scan_Failed", subject), e);
            }
            finally
            {
                if (Cts == cts)
                    Running = false;
            }
        });
    }

    public void Cancel()
    {
        Cts?.Cancel();
        Cts = null;
        Running = false;
    }

    public void Clear()
    {
        Cancel();
        Results.Clear();
        Queue.Clear();
        HasRun = false;
    }

    /// <summary>Takes the hits found since the last frame</summary>
    public void Pump()
    {
        while (Queue.TryDequeue(out var hit))
            Results.Add(hit);
    }

    /// <summary>The Progress group while a run is going (and its count once done)</summary>
    public void DrawProgress()
    {
        Pump();

        if (!Running)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_Scan_Progress_Header"), LOC.Get("EMEVD_Scan_Progress_Header_TT"));

        var total = Math.Max(1, Total);
        ImGui.ProgressBar((float)Done / total, new Vector2(-1, 0), $"{Done}/{Total}");
        GUI.WrappedText(LOC.Get("EMEVD_Scan_Status", Results.Count));

        GUI.MultiButtonInput($"{Id}ProgressActions",
            $"{Id}Cancel", LOC.Get("EMEVD_Scan_Cancel"), LOC.Get("EMEVD_Scan_Cancel_TT"), Cancel);
    }

    /// <summary>The results list: click a hit to go to its line</summary>
    public void DrawResults(string headerKey, ref bool show)
    {
        if (!HasRun)
            return;

        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get(headerKey), LOC.Get($"{headerKey}_TT"), ref show);

        GUI.WrappedText(Running
            ? LOC.Get("EMEVD_Scan_Results_Running", Results.Count, Subject)
            : LOC.Get("EMEVD_Scan_Results_Done", Results.Count, Subject));

        if (!show || Results.Count == 0)
            return;

        ImGui.BeginChild($"{Id}Results", Vector2.Zero, ImGuiChildFlags.Borders);

        var bank = View.Project.Handler.EmevdData?.PrimaryBank;
        var clipper = new ImGuiListClipper();
        clipper.Begin(Results.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var hit = Results[i];
                var where = hit.Line >= 0
                    ? LOC.Get("EMEVD_Scan_Hit_Line", hit.File, hit.EventId, hit.Line)
                    : LOC.Get("EMEVD_Scan_Hit_Event", hit.File, hit.EventId);

                if (ImGui.Selectable($"{where}##{Id}Hit{i}"))
                {
                    var file = bank?.FindFile(hit.File);
                    var ev = file?.FindEvent(hit.EventId);
                    if (ev != null)
                        View.Selection.JumpTo(file, ev, hit.Line);
                }
                GUI.Tooltip(LOC.Get("EMEVD_Scan_Hit_TT"));

                ImGui.SameLine();
                ImGui.TextDisabled(hit.What);
            }
        }

        clipper.End();
        ImGui.EndChild();
    }
}
