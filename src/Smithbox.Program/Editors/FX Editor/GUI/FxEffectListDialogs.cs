using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Utilities;
using System.IO;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.FxEditor;

/// <summary>The Effect List's dialogs and the operations behind them (FX Effect List Actions Plan, phases A–D).</summary>
public partial class FxEffectList
{
    private const string DuplicatePopup = "fxListDuplicatePopup";
    private const string BindersPopup = "fxListBindersPopup";
    private const string DeletePopup = "fxListDeletePopup";
    private const string AliasPopup = "fxListAliasPopup";
    private const string ImportPopup = "fxListImportPopup";
    private const string ComparePopup = "fxListComparePopup";
    private const string CompareWithPopup = "fxListCompareWithPopup";
    private const string ChangeIdPopup = "fxListChangeIdPopup";
    private const string ProjectCheckPopup = "fxListProjectCheckPopup";

    private bool DuplicateToOpen, BindersToOpen, DeleteToOpen, AliasToOpen, ImportToOpen, CompareToOpen,
        CompareWithToOpen, ChangeIdToOpen, ProjectCheckToOpen;

    private void DisplayDialogs()
    {
        DisplayDuplicatePopup();
        DisplayBindersPopup();
        DisplayDeletePopup();
        DisplayAliasPopup();
        DisplayImportPopup();
        DisplayComparePopup();
        DisplayCompareWithPopup();
        DisplayChangeIdPopup();
        DisplayProjectCheckPopup();
    }

    /// <summary>Opens a modal the frame it's asked for; false while it isn't showing.</summary>
    private static bool BeginModal(string title, ref bool toOpen, bool resize = false)
    {
        if (toOpen)
        {
            ImGui.OpenPopup(title);
            toOpen = false;
        }
        if (resize)
            ImGui.SetNextWindowSize(new Vector2(900, 600) * DPI.UIScale(), ImGuiCond.FirstUseEver);
        return ImGui.BeginPopupModal(title, resize ? ImGuiWindowFlags.None : ImGuiWindowFlags.AlwaysAutoResize);
    }

    private static bool CancelButton()
    {
        ImGui.SameLine();
        if (!ImGui.Button(LOC.Get("FX_EffectTree_Cancel")))
            return false;
        ImGui.CloseCurrentPopup();
        return true;
    }

    // ---------------------------------------------------------------- binders

    private List<FileDictionaryEntry> AllBinders =>
        Editor.Project.Locator.SfxFiles?.Entries?.OrderBy(e => e.Filename, StringComparer.OrdinalIgnoreCase).ToList() ?? new();

    // A sample FXR name per binder (the folder its effects are stored under), rebuilt when the bank changes
    private Dictionary<string, string> SampleNames;
    private int SampleVersion = -1;

    /// <summary>The internal name the effect <paramref name="stem"/> would have in the binder, from its other effects' names.</summary>
    private string NameIn(FileDictionaryEntry binder, string stem)
    {
        if (SampleNames == null || SampleVersion != Bank.Version)
        {
            SampleNames = new(StringComparer.OrdinalIgnoreCase);
            foreach (var e in Bank.Entries.Values)
                foreach (var b in e.Binders)
                    SampleNames.TryAdd(b.Binder.Path, b.InternalName);
            SampleVersion = Bank.Version;
        }
        var sample = SampleNames.GetValueOrDefault(binder.Path) ?? SampleNames.Values.FirstOrDefault() ?? $"sfx\\effect\\{stem}.fxr";
        return FxBank.SiblingName(sample, stem, ".fxr");
    }

    private string BinderSearch = "";

    /// <summary>A checklist of every sfx binder. Ticks are by binder path; returns whether anything changed.</summary>
    private void BinderChecklist(string id, Dictionary<string, int> ticks, int total)
    {
        ImGui.SetNextItemWidth(300 * DPI.UIScale());
        ImGui.InputTextWithHint($"##{id}Search", LOC.Get("FX_EffectList_Binder_Search"), ref BinderSearch, 128);

        ImGui.BeginChild($"##{id}List", new Vector2(500, 300) * DPI.UIScale(), ImGuiChildFlags.Borders);
        foreach (var binder in AllBinders)
        {
            if (BinderSearch != "" && !binder.Filename.Contains(BinderSearch, StringComparison.OrdinalIgnoreCase))
                continue;
            int count = ticks.GetValueOrDefault(binder.Path);
            bool on = count > 0;
            // Some but not all of several effects: marked with how many, and a click puts it in all of them
            bool mixed = count > 0 && count < total;
            var label = mixed ? $"{binder.Filename}  ({LOC.Get("FX_EffectList_Binders_Some", count, total)})" : binder.Filename;
            if (ImGui.Checkbox($"{label}##{id}_{binder.Path}", ref on))
                ticks[binder.Path] = mixed ? total : on ? total : 0;
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(binder.Path);
        }
        ImGui.EndChild();
    }

    private List<FxEntry> BinderTargets = new();
    private Dictionary<string, int> BinderTicks = new();

    private void RequestBinders(List<FxEntry> targets)
    {
        BinderTargets = targets;
        BinderTicks = targets.SelectMany(t => t.Binders.Select(b => b.Binder.Path))
            .GroupBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);
        BinderSearch = "";
        BindersToOpen = true;
    }

    private void DisplayBindersPopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Binders")}###{BindersPopup}", ref BindersToOpen))
            return;

        GUI.WrappedText(BinderTargets.Count == 1
            ? LOC.Get("FX_EffectList_Binders_Explain", BinderTargets[0].Stem)
            : LOC.Get("FX_EffectList_Binders_Explain_Many", BinderTargets.Count));
        BinderChecklist("fxBinders", BinderTicks, BinderTargets.Count);

        bool none = BinderTicks.Values.All(c => c == 0);
        if (none)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectList_Binders_None"));

        ImGui.BeginDisabled(none);
        if (ImGui.Button($"{LOC.Get("FX_EffectList_Apply")}##fxBindersApply"))
        {
            var targets = BinderTargets;
            var ticks = new Dictionary<string, int>(BinderTicks, StringComparer.OrdinalIgnoreCase);
            Deferred.Add(() => ApplyBinders(targets, ticks));
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();
        CancelButton();
        ImGui.EndPopup();
    }

    /// <summary>
    /// Sets the targets' binders from the ticks: a ticked binder (count = all) holds every target, an unticked one
    /// none, a mixed one is left as each target has it. A target can't be left in no binder.
    /// </summary>
    private void ApplyBinders(List<FxEntry> targets, Dictionary<string, int> ticks)
    {
        var binders = AllBinders.ToDictionary(b => b.Path, StringComparer.OrdinalIgnoreCase);
        var actions = new List<EditorAction>();
        foreach (var t in targets)
        {
            if (!Bank.Load(t))
                continue;
            var list = new List<FxBinderRef>();
            foreach (var (path, count) in ticks)
            {
                bool want = count == targets.Count || count > 0 && t.InBinder(path);
                if (!want || !binders.TryGetValue(path, out var binder))
                    continue;
                // Its existing name there, else the binder's naming
                var existing = t.Binders.FirstOrDefault(b => b.Binder.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                    ?? (t.OnDisk.TryGetValue(path, out var was) ? was : null);
                list.Add(existing ?? new FxBinderRef(binder, NameIn(binder, t.Stem)));
            }
            if (list.Count == 0)
            {
                Smithbox.LogWarning(this, LOC.Get("FX_EffectList_Binders_Kept", t.Stem));
                continue;
            }
            var before = t.Binders.Select(b => b.Binder.Path).ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (before.SetEquals(list.Select(b => b.Binder.Path)))
                continue;
            actions.Add(new FxMembershipAction(Bank, t, list));
        }
        if (actions.Count > 0)
            Execute(new CompoundAction(actions));
    }

    // ---------------------------------------------------------------- duplicate

    private FxEntry DuplicateSource;
    private int DuplicateId;
    private Dictionary<string, int> DuplicateTicks = new();

    private void RequestDuplicate(FxEntry entry)
    {
        DuplicateSource = entry;
        DuplicateId = Bank.FreeId(entry.Id + 1);
        DuplicateTicks = entry.Binders.ToDictionary(b => b.Binder.Path, _ => 1, StringComparer.OrdinalIgnoreCase);
        BinderSearch = "";
        DuplicateToOpen = true;
    }

    /// <summary>The id input shared by the dialogs that make an effect; returns whether the id is free.</summary>
    private bool IdInput(string id, ref int value)
    {
        ImGui.Text(LOC.Get("FX_EffectList_New_Id"));
        ImGui.SetNextItemWidth(200 * DPI.UIScale());
        ImGui.InputInt($"##{id}", ref value);
        value = Math.Max(0, value);
        ImGui.SameLine();
        ImGui.TextDisabled($"f{value:D9}");
        bool taken = Bank.IsIdTaken(value);
        if (taken)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectTree_Extract_Taken"));
        return !taken;
    }

    private void DisplayDuplicatePopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Duplicate")}###{DuplicatePopup}", ref DuplicateToOpen))
            return;
        if (DuplicateSource == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("FX_EffectList_Duplicate_Explain", DuplicateSource.Stem));
        bool free = IdInput("fxDuplicateId", ref DuplicateId);
        ImGui.Text(LOC.Get("FX_EffectList_Duplicate_Binders"));
        BinderChecklist("fxDuplicateBinders", DuplicateTicks, 1);
        bool none = DuplicateTicks.Values.All(c => c == 0);
        if (none)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectList_Binders_None"));

        ImGui.BeginDisabled(!free || none);
        if (ImGui.Button($"{LOC.Get("FX_EffectList_Duplicate_Confirm")}##fxDuplicateConfirm"))
        {
            var source = DuplicateSource;
            int id = DuplicateId;
            var paths = DuplicateTicks.Where(t => t.Value > 0).Select(t => t.Key).ToList();
            Deferred.Add(() => DuplicateEffect(source, id, paths));
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();
        CancelButton();
        ImGui.EndPopup();
    }

    /// <summary>The effect as it is now (unsaved edits included), as a separate copy.</summary>
    private FXR CopyOf(FxEntry entry)
    {
        var fxr = entry.IsLoaded ? entry.Fxr : Bank.ReadCopy(entry);
        if (fxr == null)
            return null;
        if (fxr.Version == FXR.FXRVersion.Sekiro)
            fxr.RebuildReferences();
        // A write and read gives a copy with nothing shared with the original
        return FXR.Read(fxr.Write());
    }

    private FxEntry MakeEntry(int id, FXR fxr, IEnumerable<string> binderPaths, byte[] resourceList)
    {
        var binders = AllBinders.ToDictionary(b => b.Path, StringComparer.OrdinalIgnoreCase);
        var stem = $"f{id:D9}";
        fxr.Id = id;
        return new FxEntry
        {
            Id = id,
            Fxr = fxr,
            Binders = binderPaths
                .Where(binders.ContainsKey)
                .Select(p => new FxBinderRef(binders[p], NameIn(binders[p], stem)))
                .OrderBy(b => b.Binder.Path, StringComparer.OrdinalIgnoreCase)
                .ToList(),
            ResourceList = resourceList,
        };
    }

    /// <summary>A copy of the effect under a new id, in the given binders (written on save).</summary>
    public void DuplicateEffect(FxEntry source, int id, List<string> binderPaths)
    {
        if (Bank.IsIdTaken(id))
            return;
        var fxr = CopyOf(source);
        if (fxr == null)
            return;
        var created = MakeEntry(id, fxr, binderPaths, source.ResourceList);
        if (created.Binders.Count == 0)
            return;
        Execute(new FxEntryPresenceAction(Bank, new() { created }, true, OnEffectsChanged));
        Select(created);
        Editor.Selection.FocusEntry = true;
        Smithbox.Log(this, LOC.Get("FX_EffectList_Duplicate_Done", source.Stem, created.Stem, created.Binders.Count));
    }

    // ---------------------------------------------------------------- delete

    private List<FxEntry> DeleteTargets = new();

    private void RequestDelete(List<FxEntry> targets)
    {
        DeleteTargets = targets;
        // What still uses them is shown, so the scan is started if it hasn't run
        Usage.StartScan();
        DeleteToOpen = true;
    }

    private void DisplayDeletePopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Delete")}###{DeletePopup}", ref DeleteToOpen))
            return;

        GUI.WrappedText(LOC.Get("FX_EffectList_Delete_Explain", DeleteTargets.Count));
        ImGui.BeginChild("##fxDeleteList", new Vector2(600, 250) * DPI.UIScale(), ImGuiChildFlags.Borders);
        foreach (var t in DeleteTargets)
        {
            ImGui.Text($"{t.Stem}{AliasSuffix(t)}");
            ImGui.Indent();
            ImGui.TextDisabled(LOC.Get("FX_EffectList_Delete_Binders", t.Binders.Count));
            if (Usage.HasScan)
            {
                var hosts = Usage.StartedBy(t.Id).Where(h => !DeleteTargets.Contains(h)).ToList();
                if (hosts.Count > 0)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectList_Delete_Started_By",
                        hosts.Count, string.Join(", ", hosts.Take(5).Select(h => h.Stem)) + (hosts.Count > 5 ? ", ..." : "")));
            }
            else
            {
                ImGui.TextDisabled(LOC.Get("FX_EffectList_Scanning", Usage.ScanDone, Usage.ScanTotal));
            }
            if (Usage.ParamsAvailable)
            {
                var uses = Usage.ParamsUsing(t.Id);
                if (uses.Count > 0)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectList_Delete_Params",
                        uses.Count, string.Join(", ", uses.Take(3).Select(u => $"{u.Param} {u.RowId}")) + (uses.Count > 3 ? ", ..." : "")));
            }
            ImGui.Unindent();
        }
        ImGui.EndChild();

        if (ImGui.Button($"{LOC.Get("FX_EffectList_Delete_Confirm")}##fxDeleteConfirm"))
        {
            var targets = DeleteTargets.ToList();
            Deferred.Add(() => DeleteEffects(targets));
            ImGui.CloseCurrentPopup();
        }
        CancelButton();
        ImGui.EndPopup();
    }

    /// <summary>Deletes the effects; their files leave every binder on save. Undo brings them back.</summary>
    public void DeleteEffects(List<FxEntry> targets)
    {
        // Loaded, so that undoing after the save can write them back
        var list = targets.Where(t => Bank.Load(t)).ToList();
        if (list.Count == 0)
            return;
        Execute(new FxEntryPresenceAction(Bank, list, false, OnEffectsChanged));
        Smithbox.Log(this, LOC.Get("FX_EffectList_Delete_Done", list.Count));
    }

    // ---------------------------------------------------------------- revert

    /// <summary>Whether the effect differs from the game's copy, or has unsaved edits to drop.</summary>
    private static bool CanRevert(FxEntry e) =>
        e.Vanilla == FxVanillaState.Modified && e.VanillaPacked != null || e.Vanilla == FxVanillaState.Same && e.IsLoaded && e.IsDirty;

    /// <summary>Replaces each effect's content with the game's (undoable; saved like any edit).</summary>
    public void Revert(List<FxEntry> targets)
    {
        var actions = new List<EditorAction>();
        foreach (var t in targets)
        {
            if (!Bank.Load(t))
                continue;
            var vanilla = Bank.ReadVanilla(t);
            if (vanilla != null)
                actions.Add(new FxReplaceEffectAction(Bank, t, vanilla, OnEffectsChanged));
        }
        if (actions.Count == 0)
            return;
        Execute(new CompoundAction(actions));
        Smithbox.Log(this, LOC.Get("FX_EffectList_Revert_Done", actions.Count));
    }

    // ---------------------------------------------------------------- alias

    private FxEntry AliasEntry;
    private string AliasText = "";

    private void RequestAlias(FxEntry entry)
    {
        AliasEntry = entry;
        AliasText = entry.Alias ?? "";
        AliasToOpen = true;
    }

    private void DisplayAliasPopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Edit_Alias")}###{AliasPopup}", ref AliasToOpen))
            return;

        GUI.WrappedText(LOC.Get("FX_EffectList_Alias_Explain", AliasEntry?.Stem ?? ""));
        ImGui.SetNextItemWidth(400 * DPI.UIScale());
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        bool enter = ImGui.InputText("##fxAliasText", ref AliasText, 255, ImGuiInputTextFlags.EnterReturnsTrue);

        if (ImGui.Button($"{LOC.Get("FX_EffectList_Alias_Save")}##fxAliasSave") || enter)
        {
            SetAlias(AliasEntry, AliasText.Trim());
            ImGui.CloseCurrentPopup();
        }
        CancelButton();
        ImGui.EndPopup();
    }

    /// <summary>
    /// Sets the effect's Particles alias and writes the project's alias file at once, as the Model Editor does (the
    /// other editors' effect fields show it after their alias lists are next read).
    /// </summary>
    public void SetAlias(FxEntry entry, string name)
    {
        if (entry == null)
            return;
        var aliases = Editor.Project.Handler.ProjectData?.Aliases;
        if (aliases == null)
            return;
        if (!aliases.TryGetValue(ProjectAliasType.Particles, out var list))
            aliases[ProjectAliasType.Particles] = list = new();

        var id = entry.Id.ToString();
        var existing = list.FirstOrDefault(a => a.ID == id);
        if (string.IsNullOrEmpty(name))
        {
            if (existing != null)
                list.Remove(existing);
        }
        else if (existing != null)
            existing.Name = name;
        else
            list.Add(new AliasEntry { ID = id, Name = name, Tags = new() });

        Smithbox.Orchestrator.ProjectEditor.AliasMenu.SaveIndividualAlias(ProjectAliasType.Particles);
        entry.Alias = name;
        Bank.Touch();
    }

    // ---------------------------------------------------------------- export / import

    /// <summary>Writes the effects (unsaved edits included) as .fxr files: one picked path, or a folder for several.</summary>
    public void Export(List<FxEntry> targets)
    {
        if (targets.Count == 1)
        {
            if (!PlatformUtils.Instance.SaveFileDialog(LOC.Get("FX_EffectList_Export"), new[] { "fxr" }, out var path) || string.IsNullOrEmpty(path))
                return;
            if (!path.EndsWith(".fxr", StringComparison.OrdinalIgnoreCase))
                path += ".fxr";
            WriteEffect(targets[0], path);
            return;
        }

        if (!PlatformUtils.Instance.OpenFolderDialog(LOC.Get("FX_EffectList_Export_Many"), out var folder) || string.IsNullOrEmpty(folder))
            return;
        int written = targets.Count(t => WriteEffect(t, Path.Join(folder, $"{t.Stem}.fxr")));
        Smithbox.Log(this, LOC.Get("FX_EffectList_Export_Done_Many", written, folder));
    }

    private bool WriteEffect(FxEntry entry, string path)
    {
        try
        {
            var fxr = CopyOf(entry);
            if (fxr == null)
                return false;
            File.WriteAllBytes(path, fxr.Write());
            Smithbox.Log(this, LOC.Get("FX_EffectList_Export_Done", entry.Stem, path));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_EffectList_Export_Failed", entry.Stem, path), e);
            return false;
        }
    }

    private FXR ImportFxr;
    private string ImportPath;
    private int ImportId;
    private bool ImportReplace;
    private Dictionary<string, int> ImportTicks = new();
    private string ImportVersionWarning;

    private void RequestImport()
    {
        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("FX_EffectList_Import"), new[] { "fxr" }, out var path) || string.IsNullOrEmpty(path))
            return;
        try
        {
            ImportFxr = FXR.Read(File.ReadAllBytes(path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("FX_EffectList_Import_Failed", path), e);
            return;
        }
        ImportPath = path;

        // The id from the file name (f000620400.fxr), else the header's
        var match = Regex.Match(Path.GetFileNameWithoutExtension(path), @"^f(\d+)$", RegexOptions.IgnoreCase);
        ImportId = match.Success && int.TryParse(match.Groups[1].Value, out var fromName) ? fromName : ImportFxr.Id;
        ImportReplace = Bank.Entries.ContainsKey(ImportId);
        if (!ImportReplace && Bank.IsIdTaken(ImportId))
            ImportId = Bank.FreeId(ImportId);

        // New effects go where the selected effect is, as a starting point
        var near = Editor.Selection.Entry ?? Bank.Entries.Values.FirstOrDefault();
        ImportTicks = near?.Binders.ToDictionary(b => b.Binder.Path, _ => 1, StringComparer.OrdinalIgnoreCase) ?? new();

        // An effect from another game won't play here
        var sample = Bank.Entries.Values.FirstOrDefault(e => e.IsLoaded)?.Fxr ?? Bank.ReadCopy(Bank.Entries.Values.First());
        ImportVersionWarning = sample != null && sample.Version != ImportFxr.Version
            ? LOC.Get("FX_EffectList_Import_Version", ImportFxr.Version, sample.Version)
            : null;
        BinderSearch = "";
        ImportToOpen = true;
    }

    private void DisplayImportPopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Import")}###{ImportPopup}", ref ImportToOpen))
            return;

        GUI.WrappedText(LOC.Get("FX_EffectList_Import_Explain", Path.GetFileName(ImportPath ?? "")));
        if (ImportVersionWarning != null)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, ImportVersionWarning);

        bool exists = Bank.Entries.ContainsKey(ImportId);
        if (exists)
            ImGui.Checkbox($"{LOC.Get("FX_EffectList_Import_Replace")}##fxImportReplace", ref ImportReplace);
        else
            ImportReplace = false;

        bool ok;
        if (ImportReplace)
        {
            ImGui.Text(LOC.Get("FX_EffectList_New_Id"));
            ImGui.SetNextItemWidth(200 * DPI.UIScale());
            ImGui.InputInt("##fxImportId", ref ImportId);
            ImGui.SameLine();
            ImGui.TextDisabled($"f{ImportId:D9}");
            GUI.WrappedText(LOC.Get("FX_EffectList_Import_Replace_Explain", $"f{ImportId:D9}"));
            ok = Bank.Entries.ContainsKey(ImportId);
        }
        else
        {
            ok = IdInput("fxImportId", ref ImportId);
            ImGui.Text(LOC.Get("FX_EffectList_Duplicate_Binders"));
            BinderChecklist("fxImportBinders", ImportTicks, 1);
            if (ImportTicks.Values.All(c => c == 0))
            {
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectList_Binders_None"));
                ok = false;
            }
        }

        ImGui.BeginDisabled(!ok);
        if (ImGui.Button($"{LOC.Get("FX_EffectList_Import_Confirm")}##fxImportConfirm"))
        {
            var fxr = ImportFxr;
            int id = ImportId;
            bool replace = ImportReplace;
            var paths = ImportTicks.Where(t => t.Value > 0).Select(t => t.Key).ToList();
            Deferred.Add(() => Import(fxr, id, replace, paths));
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();
        CancelButton();
        ImGui.EndPopup();
    }

    /// <summary>An effect read from a file: replacing an existing effect's content, or as a new effect.</summary>
    public void Import(FXR fxr, int id, bool replace, List<string> binderPaths)
    {
        if (replace)
        {
            if (!Bank.Entries.TryGetValue(id, out var target) || !Bank.Load(target))
                return;
            fxr.Id = id;
            Execute(new FxReplaceEffectAction(Bank, target, fxr, OnEffectsChanged));
            Select(target);
        }
        else
        {
            if (Bank.IsIdTaken(id))
                return;
            var created = MakeEntry(id, fxr, binderPaths, null);
            if (created.Binders.Count == 0)
                return;
            Execute(new FxEntryPresenceAction(Bank, new() { created }, true, OnEffectsChanged));
            Select(created);
        }
        Editor.Selection.FocusEntry = true;
        Smithbox.Log(this, LOC.Get("FX_EffectList_Import_Done", Path.GetFileName(ImportPath ?? ""), $"f{id:D9}"));
    }

    // ---------------------------------------------------------------- compare

    private string CompareTitleA = "", CompareTitleB = "";
    private List<FxDifference> Differences = new();
    private string CompareFilter = "";

    private void ShowCompare(string a, FXR fa, string b, FXR fb)
    {
        if (fa == null || fb == null)
            return;
        CompareTitleA = a;
        CompareTitleB = b;
        Differences = FxCompare.Diff(fa, fb);
        CompareFilter = "";
        CompareToOpen = true;
    }

    private void RequestCompareVanilla(FxEntry entry) =>
        ShowCompare(entry.Stem, entry.IsLoaded ? entry.Fxr : Bank.ReadCopy(entry),
            LOC.Get("FX_EffectList_Compare_Vanilla_Label"), Bank.ReadVanilla(entry));

    private void RequestCompare(FxEntry a, FxEntry b) =>
        ShowCompare(a.Stem, a.IsLoaded ? a.Fxr : Bank.ReadCopy(a), b.Stem, b.IsLoaded ? b.Fxr : Bank.ReadCopy(b));

    private FxEntry CompareWithSource;
    private int CompareWithId;

    private void RequestCompareWith(FxEntry entry)
    {
        CompareWithSource = entry;
        CompareWithId = entry.Id;
        CompareWithToOpen = true;
    }

    private void DisplayCompareWithPopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Compare_With")}###{CompareWithPopup}", ref CompareWithToOpen))
            return;

        GUI.WrappedText(LOC.Get("FX_EffectList_Compare_With_Explain", CompareWithSource?.Stem ?? ""));
        ImGui.SetNextItemWidth(200 * DPI.UIScale());
        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();
        ImGui.InputInt("##fxCompareWithId", ref CompareWithId);
        bool exists = Bank.Entries.TryGetValue(CompareWithId, out var other);
        ImGui.SameLine();
        ImGui.TextDisabled(exists ? $"{other.Stem}{AliasSuffix(other)}" : LOC.Get("FX_EffectList_Missing_Effect", $"f{CompareWithId:D9}"));

        ImGui.BeginDisabled(!exists || CompareWithSource == null);
        if (ImGui.Button($"{LOC.Get("FX_EffectList_Compare")}##fxCompareWithConfirm"))
        {
            ImGui.CloseCurrentPopup();
            RequestCompare(CompareWithSource, other);
        }
        ImGui.EndDisabled();
        CancelButton();
        ImGui.EndPopup();
    }

    private void DisplayComparePopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Compare_Title", CompareTitleA, CompareTitleB)}###{ComparePopup}", ref CompareToOpen, resize: true))
            return;

        ImGui.Text(Differences.Count == 0
            ? LOC.Get("FX_EffectList_Compare_Same")
            : LOC.Get("FX_EffectList_Compare_Count", Differences.Count));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(250 * DPI.UIScale());
        ImGui.InputTextWithHint("##fxCompareFilter", LOC.Get("FX_EffectList_Compare_Filter"), ref CompareFilter, 128);
        ImGui.SameLine();
        if (ImGui.Button(LOC.Get("FX_EffectList_Close")))
            ImGui.CloseCurrentPopup();
        GUI.WrappedText(LOC.Get("FX_EffectList_Compare_Note"));

        var rows = CompareFilter == "" ? Differences
            : Differences.Where(d => d.Key.Contains(CompareFilter, StringComparison.OrdinalIgnoreCase)).ToList();

        if (ImGui.BeginTable("##fxCompareTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("FX_EffectList_Compare_Where"), ImGuiTableColumnFlags.WidthStretch, 2);
            ImGui.TableSetupColumn(CompareTitleA, ImGuiTableColumnFlags.WidthStretch, 1);
            ImGui.TableSetupColumn(CompareTitleB, ImGuiTableColumnFlags.WidthStretch, 1);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(rows.Count);
            while (clipper.Step())
            {
                for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var d = rows[i];
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextUnformatted(d.Key);
                    ImGui.TableSetColumnIndex(1);
                    if (d.A == null)
                        ImGui.TextDisabled(LOC.Get("FX_EffectList_Compare_Absent"));
                    else
                        ImGui.TextUnformatted(d.A);
                    ImGui.TableSetColumnIndex(2);
                    if (d.B == null)
                        ImGui.TextDisabled(LOC.Get("FX_EffectList_Compare_Absent"));
                    else
                        ImGui.TextUnformatted(d.B);
                }
            }
            clipper.End();
            ImGui.EndTable();
        }
        ImGui.EndPopup();
    }

    // ---------------------------------------------------------------- change id

    private FxEntry ChangeIdSource;
    private int ChangeIdValue;
    private bool ChangeIdProxies = true;

    private void RequestChangeId(FxEntry entry)
    {
        ChangeIdSource = entry;
        ChangeIdValue = Bank.FreeId(entry.Id + 1);
        Usage.StartScan();
        ChangeIdToOpen = true;
    }

    private void DisplayChangeIdPopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Change_Id")}###{ChangeIdPopup}", ref ChangeIdToOpen))
            return;
        if (ChangeIdSource == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("FX_EffectList_Change_Id_Explain", ChangeIdSource.Stem, ChangeIdSource.Binders.Count));
        bool free = IdInput("fxChangeId", ref ChangeIdValue);

        bool scanned = Usage.HasScan;
        var hosts = scanned ? Usage.StartedBy(ChangeIdSource.Id) : new();
        ImGui.Checkbox($"{LOC.Get("FX_EffectList_Change_Id_Proxies", hosts.Count)}##fxChangeIdProxies", ref ChangeIdProxies);
        if (!scanned)
            ImGui.TextDisabled(LOC.Get("FX_EffectList_Scanning", Usage.ScanDone, Usage.ScanTotal));

        if (Usage.ParamsAvailable)
        {
            var uses = Usage.ParamsUsing(ChangeIdSource.Id);
            if (uses.Count > 0)
            {
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectList_Change_Id_Params", uses.Count));
                ImGui.BeginChild("##fxChangeIdParams", new Vector2(500, 120) * DPI.UIScale(), ImGuiChildFlags.Borders);
                foreach (var use in uses)
                    if (ImGui.Selectable($"{use.Param} {use.RowId} {use.RowName} : {use.Field}##fxChangeIdParam_{use.Param}_{use.RowId}_{use.Field}"))
                        FxUsage.JumpToParam(use);
                ImGui.EndChild();
            }
        }
        if (!string.IsNullOrEmpty(ChangeIdSource.Alias))
            ImGui.TextDisabled(LOC.Get("FX_EffectList_Change_Id_Alias"));

        ImGui.BeginDisabled(!free || ChangeIdProxies && !scanned);
        if (ImGui.Button($"{LOC.Get("FX_EffectList_Change_Id_Confirm")}##fxChangeIdConfirm"))
        {
            var source = ChangeIdSource;
            int id = ChangeIdValue;
            var update = ChangeIdProxies ? hosts : new();
            Deferred.Add(() => ChangeId(source, id, update));
            ImGui.CloseCurrentPopup();
        }
        ImGui.EndDisabled();
        CancelButton();
        ImGui.EndPopup();
    }

    /// <summary>
    /// Moves the effect to a new id: a new effect with its content in the same binders, the old one deleted, and the
    /// Proxy nodes of <paramref name="hosts"/> pointed at the new id. One undo step. Params are left to the user.
    /// </summary>
    public void ChangeId(FxEntry source, int id, List<FxEntry> hosts)
    {
        if (Bank.IsIdTaken(id) || !Bank.Load(source))
            return;
        var fxr = CopyOf(source);
        if (fxr == null)
            return;
        var created = MakeEntry(id, fxr, source.Binders.Select(b => b.Binder.Path), source.ResourceList);

        var actions = new List<EditorAction>
        {
            new FxEntryPresenceAction(Bank, new() { source }, false),
            new FxEntryPresenceAction(Bank, new() { created }, true, OnEffectsChanged),
        };

        int updated = 0;
        foreach (var host in hosts)
        {
            if (host == source || !Bank.Load(host))
                continue;
            foreach (var node in FXR.Walk(host.Fxr.Root))
            {
                if (node.Type != FxNodeOps.ProxyType || FxProxyOps.Target(node) != source.Id)
                    continue;
                var proxy = node.Actions.First(a => a.Type == FxNodeOps.ProxyAction);
                actions.Add(new FxEditFieldAction(Bank, host, proxy, a => a.Fields1[0] = unchecked((uint)id)));
                updated++;
            }
        }

        Execute(new CompoundAction(actions));
        Select(created);
        Editor.Selection.FocusEntry = true;
        Smithbox.Log(this, LOC.Get("FX_EffectList_Change_Id_Done", source.Stem, created.Stem, updated));
    }

    // ---------------------------------------------------------------- project check

    private void DisplayProjectCheckPopup()
    {
        if (!BeginModal($"{LOC.Get("FX_EffectList_Project_Check")}###{ProjectCheckPopup}", ref ProjectCheckToOpen, resize: true))
            return;

        var results = ProjectCheck.Results;
        ImGui.Text(LOC.Get("FX_EffectList_Project_Check_Summary", ProjectCheck.Checked, results.Count, ProjectCheck.WithProblems.Count));
        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("FX_EffectList_Project_Check_Run")}##fxProjectCheckRun"))
            ProjectCheck.Run();
        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("FX_EffectList_Filter_Problems")}##fxProjectCheckFilter"))
        {
            Mode = FxListFilter.Problems;
            ImGui.CloseCurrentPopup();
        }
        GUI.Tooltip(LOC.Get("FX_EffectList_Filter_Problems_TT"));
        ImGui.SameLine();
        if (ImGui.Button(LOC.Get("FX_EffectList_Close")))
            ImGui.CloseCurrentPopup();
        GUI.WrappedText(LOC.Get("FX_EffectList_Project_Check_Explain"));

        if (results.Count == 0)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("FX_EffectList_Project_Check_Clean"));

        if (results.Count > 0 && ImGui.BeginTable("##fxProjectCheckTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("FX_EffectList_Project_Check_Effect"), ImGuiTableColumnFlags.WidthFixed, 220 * DPI.UIScale());
            ImGui.TableSetupColumn(LOC.Get("FX_EffectList_Compare_Where"), ImGuiTableColumnFlags.WidthFixed, 120 * DPI.UIScale());
            ImGui.TableSetupColumn(LOC.Get("FX_EffectList_Project_Check_Problem"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            for (int i = 0; i < results.Count; i++)
            {
                var p = results[i];
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                // Clicking opens the effect and selects the node
                if (ImGui.Selectable($"{p.Entry.Stem}{AliasSuffix(p.Entry)}##fxProblem_{i}", false, ImGuiSelectableFlags.SpanAllColumns))
                {
                    var entry = p.Entry;
                    var where = p.Where;
                    Deferred.Add(() => OpenProblem(entry, where));
                    ImGui.CloseCurrentPopup();
                }
                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(string.IsNullOrEmpty(p.Where) ? "-" : p.Where);
                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(p.Text);
            }
            ImGui.EndTable();
        }
        ImGui.EndPopup();
    }

    private void OpenProblem(FxEntry entry, string where)
    {
        if (!Editor.JumpTo(entry.Id))
            return;
        if (!string.IsNullOrEmpty(where) && entry.Fxr != null && FxNodeOps.FromPath(entry.Fxr.Root, where) is { } node)
            Editor.Selection.SelectItem(node);
    }
}
