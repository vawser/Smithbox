using SoulsFormats;

namespace StudioCore.Editors.FxEditor;

/// <summary>One problem found by the Project Check: the effect, where in it (a node path, or empty), and what.</summary>
public record FxProblem(FxEntry Entry, string Where, string Text);

/// <summary>
/// The Project Check (FX Effect List Actions Plan, phase D): the Effect Tree's structure checks, plus resource and
/// binder checks, over every effect the project has changed (modified from vanilla, added, unsaved, or with binder
/// changes waiting). Unloaded effects are read from their packed copies.
/// </summary>
public class FxProjectCheck
{
    private readonly FxEditorScreen Editor;

    public FxProjectCheck(FxEditorScreen editor)
    {
        Editor = editor;
    }

    public List<FxProblem> Results { get; private set; } = new();

    /// <summary>The effects with problems in the last run, for the list's Problems filter.</summary>
    public HashSet<int> WithProblems { get; private set; } = new();

    public int Checked { get; private set; }
    public DateTime? RanAt { get; private set; }

    /// <summary>The effects the project has changed in some way.</summary>
    public static bool IsChanged(FxEntry e) =>
        e.Vanilla != FxVanillaState.Same || e.IsNew || e.IsDirty && e.IsLoaded || e.MembershipChanged;

    public void Run()
    {
        var bank = Editor.Data.Bank;
        var results = new List<FxProblem>();
        var effects = bank.Entries.Values.Where(IsChanged).ToList();

        foreach (var entry in effects)
        {
            var fxr = entry.IsLoaded ? entry.Fxr : bank.ReadCopy(entry);
            if (fxr == null)
            {
                results.Add(new FxProblem(entry, "", LOC.Get("FX_ProjectCheck_Unreadable")));
                continue;
            }

            foreach (var node in FXR.Walk(fxr.Root))
                foreach (var p in FxNodeOps.StructureProblems(fxr, entry.Id, bank.Entries.ContainsKey, node)
                    .Concat(FxNodeOps.SlotProblems(Editor.Data.Schema, Editor.Data.Game, node)))
                    results.Add(new FxProblem(entry, FxNodeOps.PathOf(fxr.Root, node), p));

            // Textures and models no sfx binder holds (a resource can come from another binder the game has loaded,
            // e.g. commoneffects, so only one no binder has is reported)
            foreach (var name in FxUsage.ResourceNames(fxr, Editor.Data.Schema, Editor.Data.Game))
                if (!HasResource(bank, name))
                    results.Add(new FxProblem(entry, "", LOC.Get("FX_ProjectCheck_Resource_Missing", name)));

            if (entry.Binders.Count == 0)
                results.Add(new FxProblem(entry, "", LOC.Get("FX_ProjectCheck_No_Binder")));
        }

        Results = results;
        WithProblems = results.Select(r => r.Entry.Id).ToHashSet();
        Checked = effects.Count;
        RanAt = DateTime.Now;
    }

    /// <summary>Whether any binder holds the texture (s88610 or s88610_x) or model (s88610).</summary>
    private static bool HasResource(FxBank bank, string name)
    {
        if (bank.Textures.ContainsKey(name) || bank.Models.ContainsKey(name))
            return true;
        return bank.Textures.Keys.Any(k => k.StartsWith(name + "_", StringComparison.OrdinalIgnoreCase));
    }
}
