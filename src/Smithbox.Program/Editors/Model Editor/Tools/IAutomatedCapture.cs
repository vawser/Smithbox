using System.Collections.Generic;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// A Model Editor tool that <see cref="RenderSnapshotAutomation" /> can run unattended: the render snapshots
/// (<see cref="ModelRenderSnapshotTool" />) or the material oracle (<see cref="MaterialOracleTool" />)
/// </summary>
public interface IAutomatedCapture
{
    /// <summary>Starts a run. Returns false, with why, if it can't start.</summary>
    bool Start(out string reason);

    bool IsRunning { get; }

    void Cancel();

    /// <summary>How the last run ended</summary>
    string LastStatus { get; }

    /// <summary>How many of the last run's entries were captured, of how many were tried</summary>
    (int Ok, int Total) LastRunCounts { get; }

    /// <summary>The last run's entries that weren't captured, with why</summary>
    IEnumerable<string> LastRunFailures { get; }
}
