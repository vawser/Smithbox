using Hexa.NET.ImGui;
using StudioCore.Renderer;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Live preview (Material Editor plan, F6): material files report their changes here, and once the user has
/// stopped editing for a moment, the models that use them are reloaded so the Model and Map Editor viewports show
/// the change. With live preview off, the changes wait for a save.
/// </summary>
public static class MaterialPreview
{
    // Short names of the materials changed since the last refresh
    private static readonly ConcurrentDictionary<string, byte> Pending = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Stopwatch SinceChange = Stopwatch.StartNew();

    // How long edits must stop before a refresh, so dragging a value doesn't reload models every frame
    private static readonly TimeSpan Settle = TimeSpan.FromMilliseconds(350);

    /// <summary>
    /// Notes that a material file changed (an edit, an undo, or a file added, removed or renamed)
    /// </summary>
    public static void Queue(string fileKey)
    {
        if (string.IsNullOrEmpty(fileKey))
            return;

        Pending[MaterialBank.ShortNameOf(fileKey)] = 0;
        SinceChange.Restart();
    }

    /// <summary>
    /// Called every frame by the Material Editor: refreshes once edits have settled, if live preview is on
    /// </summary>
    public static void Update(ProjectEntry project)
    {
        if (Pending.IsEmpty || !CFG.Current.MaterialEditor_Live_Preview)
            return;

        if (ImGui.IsAnyItemActive() || SinceChange.Elapsed < Settle)
            return;

        Flush(project);
    }

    /// <summary>
    /// Refreshes the models of every pending material now, live preview or not (after a save)
    /// </summary>
    public static void Flush(ProjectEntry project)
    {
        if (Pending.IsEmpty)
            return;

        var names = Pending.Keys.ToList();
        foreach (var name in names)
            Pending.TryRemove(name, out _);

        var bank = project.Handler.MaterialData?.PrimaryBank;
        bank?.InvalidateDefinitions(names);

        // A model with unsaved Model Editor changes would be read back from its file, losing them on screen
        var unsavedModels = project.Handler.ModelEditor?.GetUnsavedFiles() ?? new List<string>();

        bool Skip(string resourceName) => unsavedModels.Any(m =>
            resourceName.Contains(m, StringComparison.OrdinalIgnoreCase));

        var count = MaterialRefresh.ReloadModelsUsing(names, Skip);

        if (count > 0)
            Smithbox.Log(typeof(MaterialPreview), LOC.Get("MAT_Preview_Refreshed", count, names.Count));
    }
}
