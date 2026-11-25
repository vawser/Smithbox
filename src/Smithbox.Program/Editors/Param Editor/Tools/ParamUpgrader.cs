using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor.Upgrader;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Upgrades a project's params to the game's current regulation version, one version step at a time, using the
/// version diffs (Assets/PARAM/&lt;game&gt;/Upgrader Diffs, plus diffs made here). See
/// Claude/Documentation/Param Upgrader Diffs Plan.md.
/// </summary>
public class ParamUpgrader
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ParamUpgrader(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool SupportsParamUpgrading()
    {
        return RegulationHistory.GameFor(Project.Descriptor.ProjectType) != null;
    }

    public void ParamUpgradeWarning()
    {
        var paramData = Project.Handler.ParamData;

        if (SupportsParamUpgrading() && Project.Initialized && Project.IsSelected)
        {
            if (paramData.PrimaryBank.ParamVersion < paramData.VanillaBank.ParamVersion)
            {
                var primaryVersion = ParamUtils.ParseRegulationVersion(paramData.PrimaryBank.ParamVersion);
                var vanillaVersion = ParamUtils.ParseRegulationVersion(paramData.VanillaBank.ParamVersion);

                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Warning_Text_Color);
                ImGui.Text($"{LOC.Get("PARAM_Upgrader_Version_Mismatch_Hint", primaryVersion, vanillaVersion)}");
                ImGui.PopStyleColor(1);
            }
        }
    }

    #region State

    /// <summary>The result of a prepared upgrade, waiting for Apply Upgrade</summary>
    private sealed class PreparedUpgrade
    {
        public ulong FromVersion;
        public ulong ToVersion;
        public List<RegulationDiff> Chain;
        public Dictionary<string, Param> Params;
        public List<UpgradeConflict> Conflicts;
    }

    // Written by the background task, read by the UI
    private volatile bool Busy;
    private volatile string Status;
    private volatile bool StatusIsError;

    private PreparedUpgrade Prepared;
    private ulong MissingVersion;

    #endregion

    public void Display()
    {
        if (Project.Handler.ParamData == null)
            return;

        if (ToolSections.Header($"{LOC.Get("PARAM_Upgrader_Header")}##paramUpgraderHeader"))
        {
            ImGui.BeginChild("ParamUpgraderToolSection", ImGuiChildFlags.Borders);

            if (!SupportsParamUpgrading())
            {
                GUI.WrappedText(LOC.Get("PARAM_Upgrader_Not_Supported"));
            }
            else
            {
                UpgraderMenu();

                GUI.Spacer();
                GeneratorMenu();
            }

            ImGui.EndChild();
        }
    }

    #region Upgrade

    private void UpgraderMenu()
    {
        var primaryBank = Project.Handler.ParamData.PrimaryBank;
        var vanillaBank = Project.Handler.ParamData.VanillaBank;

        GUI.WrappedText(LOC.Get("PARAM_Upgrader_Hint"));

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_Upgrader_Header_Version"), LOC.Get("PARAM_Upgrader_Header_Version_TT"));

        ImGui.Text(LOC.Get("PARAM_Upgrader_Primary_Version", ParamUtils.ParseRegulationVersion(primaryBank.ParamVersion)));
        ImGui.Text(LOC.Get("PARAM_Upgrader_Source_Version", ParamUtils.ParseRegulationVersion(vanillaBank.ParamVersion)));

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_Upgrader_Header_Actions"), LOC.Get("PARAM_Upgrader_Header_Actions_TT"));

        if (primaryBank.ParamVersion >= vanillaBank.ParamVersion)
        {
            GUI.WrappedText(LOC.Get("PARAM_Upgrader_No_Upgrade_Needed"));
            Prepared = null;
        }
        else if (Busy)
        {
            ImGui.BeginDisabled();
            ImGui.Button($"{LOC.Get("PARAM_Upgrader_Action_Check_for_Conflicts")}##checkForConflictsAction");
            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{LOC.Get("PARAM_Upgrader_Action_Check_for_Conflicts")}##checkForConflictsAction"))
                StartPrepare();

            GUI.Tooltip(LOC.Get("PARAM_Upgrader_Action_Check_for_Conflicts_TT"));

            if (Prepared != null && Prepared.FromVersion == primaryBank.ParamVersion && Prepared.ToVersion == vanillaBank.ParamVersion)
            {
                ImGui.SameLine();
                if (ImGui.Button($"{LOC.Get("PARAM_Upgrader_Action_Apply_Upgrade")}##applyUpgradeAction"))
                    ApplyPrepared();

                GUI.Tooltip(LOC.Get("PARAM_Upgrader_Action_Apply_Upgrade_TT"));
            }
        }

        DisplayStatus();

        if (MissingVersion != 0 && !Busy)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("PARAM_Upgrader_Missing_Step", ParamUtils.ParseRegulationVersion(MissingVersion)));
        }

        if (Prepared != null && !Busy)
            DisplayPrepared();
    }

    private void DisplayStatus()
    {
        var status = Status;
        if (string.IsNullOrEmpty(status))
            return;

        if (StatusIsError)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, status);
        else
            GUI.WrappedText(status);
    }

    private void DisplayPrepared()
    {
        var steps = string.Join(" > ", new[] { Prepared.FromVersion }
            .Concat(Prepared.Chain.Select(d => d.ToVersion))
            .Select(v => ParamUtils.ParseRegulationVersion(v)));

        GUI.WrappedText(LOC.Get("PARAM_Upgrader_Steps", steps));

        if (Prepared.Conflicts.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PARAM_Upgrader_No_Conflicts"));
            return;
        }

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_Upgrader_Header_Conflicts"), LOC.Get("PARAM_Upgrader_Header_Conflicts_TT"));

        foreach (var group in Prepared.Conflicts.GroupBy(c => c.Param).OrderBy(g => g.Key))
        {
            if (!ImGui.TreeNodeEx($"{group.Key} ({group.Count()})##upgraderConflict_{group.Key}"))
                continue;

            foreach (var conflict in group)
            {
                var version = ParamUtils.ParseRegulationVersion(conflict.ToVersion);

                if (conflict.RowConflict)
                {
                    ImGui.Text(LOC.Get("PARAM_Upgrader_Conflict_Row", conflict.RowID, version));
                    continue;
                }

                foreach (var field in conflict.Fields)
                {
                    ImGui.Text(LOC.Get("PARAM_Upgrader_Conflict_Field", conflict.RowID, field.Field, version,
                        field.ModValue, field.OldValue, field.NewValue));
                }
            }

            ImGui.TreePop();
        }
    }

    /// <summary>
    /// Rebuilds the vanilla versions from the project's version to the game's and merges the project's params
    /// through each step, in the background. Nothing is changed until Apply Upgrade.
    /// </summary>
    private void StartPrepare()
    {
        var paramData = Project.Handler.ParamData;
        var history = RegulationHistory.For(Project);
        var from = paramData.PrimaryBank.ParamVersion;
        var to = paramData.VanillaBank.ParamVersion;

        // Shallow copies: the task must not see params added or removed while it runs
        var mod = new Dictionary<string, Param>(paramData.PrimaryBank.Params);
        var vanilla = new Dictionary<string, Param>(paramData.VanillaBank.Params);
        var context = history.CreateContext();

        Busy = true;
        Prepared = null;
        MissingVersion = 0;
        SetStatus(LOC.Get("PARAM_Upgrader_Status_Preparing"), false);

        Task.Run(() =>
        {
            try
            {
                var (chain, versions) = history.RebuildFrom(from, context, out var missing);

                if (chain == null)
                {
                    MissingVersion = missing;
                    SetStatus(LOC.Get("PARAM_Upgrader_Status_No_Chain"), true);
                    return;
                }

                if (chain.Count == 0 || chain[^1].ToVersion != to)
                {
                    SetStatus(LOC.Get("PARAM_Upgrader_Status_Game_Version_Mismatch",
                        ParamUtils.ParseRegulationVersion(to)), true);
                    return;
                }

                // The last step merges into the Vanilla bank's params (what the project shows as vanilla)
                var newest = versions[^1];
                versions[^1] = newest.ToDictionary(kv => kv.Key,
                    kv => vanilla.TryGetValue(kv.Key, out var v) ? v : kv.Value);

                var conflicts = new List<UpgradeConflict>();
                var result = RegulationUpgradeChain.Upgrade(mod, chain, versions, conflicts);

                // Params outside the regulation (system params and the like) aren't versioned by it: the
                // project's own copy is kept
                foreach (var (name, param) in vanilla)
                {
                    if (newest.ContainsKey(name))
                        continue;

                    result[name] = mod.TryGetValue(name, out var own) ? own : param;
                }

                Prepared = new PreparedUpgrade
                {
                    FromVersion = from,
                    ToVersion = to,
                    Chain = chain,
                    Params = result,
                    Conflicts = conflicts
                };

                SetStatus(LOC.Get("PARAM_Upgrader_Status_Prepared", chain.Count, conflicts.Count), false);
            }
            catch (Exception e)
            {
                SetStatus(LOC.Get("PARAM_Upgrader_Status_Failed", e.Message), true);
                Smithbox.LogError(this, LOC.Get("PARAM_Upgrader_Conflict_Check_Failed"), e);
            }
            finally
            {
                Busy = false;
            }
        });
    }

    private async void ApplyPrepared()
    {
        var prepared = Prepared;
        if (prepared == null)
            return;

        var paramData = Project.Handler.ParamData;
        var primaryBank = paramData.PrimaryBank;

        // Backup original
        if (CFG.Current.Project.Project_Enable_Backup_Saves)
        {
            var data = Project.VFS.ProjectFS.GetFile(RegulationHistory.RegulationFile)?.GetData().ToArray();
            if (data != null)
                await File.WriteAllBytesAsync(Path.Join(Project.Descriptor.ProjectPath, "regulation.bin.prev"), data);
        }

        primaryBank._params = prepared.Params;
        primaryBank._paramVersion = prepared.ToVersion;
        primaryBank._pendingUpgrade = true;

        CacheBank.ClearCaches();
        paramData.RefreshAllParamDiffCaches(false);

        // The upgrade rebuilds the params from the new game version, so all of them need to be written
        primaryBank.MarkAllParamsDirty();

        await primaryBank.Save();

        Prepared = null;
        SetStatus(LOC.Get("PARAM_Upgrader_Upgraded_Primary_Bank_Params"), false);
        Smithbox.Log(this, LOC.Get("PARAM_Upgrader_Upgraded_Primary_Bank_Params"));
    }

    private void SetStatus(string text, bool error)
    {
        StatusIsError = error;
        Status = text;
    }

    #endregion

    #region Generator

    private enum SourceKind
    {
        Game,
        Snapshot,
        File
    }

    private sealed class RegulationSource
    {
        public SourceKind Kind = SourceKind.Game;
        public RegulationSnapshotFile Snapshot;
        public string FilePath = "";
    }

    private readonly RegulationSource OlderSource = new() { Kind = SourceKind.Snapshot };
    private readonly RegulationSource NewerSource = new() { Kind = SourceKind.Game };

    private volatile bool GeneratorBusy;
    private volatile string GeneratorStatus;
    private volatile bool GeneratorStatusIsError;
    private List<string> GeneratorNotes = new();
    private List<RegulationSnapshotFile> Snapshots;

    private void GeneratorMenu()
    {
        var history = RegulationHistory.For(Project);

        if (!ImGui.CollapsingHeader($"{LOC.Get("PARAM_Upgrader_Generator_Header")}##upgraderGenerator"))
            return;

        GUI.WrappedText(LOC.Get("PARAM_Upgrader_Generator_Hint"));
        GUI.Spacer();

        Snapshots ??= history.ListSnapshots();

        if (OlderSource.Kind == SourceKind.Snapshot && OlderSource.Snapshot == null)
        {
            OlderSource.Snapshot = Snapshots.LastOrDefault();
            if (OlderSource.Snapshot == null)
                OlderSource.Kind = SourceKind.File;
        }

        SourceSelector("older", LOC.Get("PARAM_Upgrader_Generator_Older"), OlderSource);
        SourceSelector("newer", LOC.Get("PARAM_Upgrader_Generator_Newer"), NewerSource);

        GUI.Spacer();

        if (GeneratorBusy)
        {
            ImGui.BeginDisabled();
            ImGui.Button($"{LOC.Get("PARAM_Upgrader_Generator_Generate")}##upgraderGenerate");
            ImGui.EndDisabled();
        }
        else if (ImGui.Button($"{LOC.Get("PARAM_Upgrader_Generator_Generate")}##upgraderGenerate"))
        {
            StartGenerate(history);
        }

        GUI.Tooltip(LOC.Get("PARAM_Upgrader_Generator_Generate_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("PARAM_Upgrader_Generator_Open_Folder")}##upgraderOpenFolder"))
        {
            Directory.CreateDirectory(history.UserDiffDir);
            Process.Start("explorer.exe", $"\"{history.UserDiffDir}\"");
        }

        GUI.Tooltip(LOC.Get("PARAM_Upgrader_Generator_Open_Folder_TT"));

        var status = GeneratorStatus;
        if (!string.IsNullOrEmpty(status))
        {
            if (GeneratorStatusIsError)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, status);
            else
                GUI.WrappedText(status);
        }

        var notes = GeneratorNotes;
        if (!GeneratorBusy && notes.Count > 0 &&
            ImGui.TreeNodeEx($"{LOC.Get("PARAM_Upgrader_Generator_Notes", notes.Count)}##upgraderGeneratorNotes"))
        {
            foreach (var note in notes)
                GUI.WrappedText(note);

            ImGui.TreePop();
        }
    }

    private void SourceSelector(string id, string label, RegulationSource source)
    {
        var gameVersion = ParamUtils.ParseRegulationVersion(Project.Handler.ParamData.VanillaBank.ParamVersion);

        string Describe(SourceKind kind, RegulationSnapshotFile snapshot) => kind switch
        {
            SourceKind.Game => LOC.Get("PARAM_Upgrader_Generator_Source_Game", gameVersion),
            SourceKind.Snapshot => LOC.Get("PARAM_Upgrader_Generator_Source_Snapshot",
                ParamUtils.ParseRegulationVersion(snapshot?.Version ?? 0)),
            _ => LOC.Get("PARAM_Upgrader_Generator_Source_File")
        };

        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);
        ImGui.SameLine();

        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo($"##upgraderSource_{id}", Describe(source.Kind, source.Snapshot)))
        {
            if (ImGui.Selectable(Describe(SourceKind.Game, null), source.Kind == SourceKind.Game))
                source.Kind = SourceKind.Game;

            foreach (var snapshot in Snapshots)
            {
                if (ImGui.Selectable($"{Describe(SourceKind.Snapshot, snapshot)}##snapshot{snapshot.Version}",
                        source.Kind == SourceKind.Snapshot && source.Snapshot == snapshot))
                {
                    source.Kind = SourceKind.Snapshot;
                    source.Snapshot = snapshot;
                }
            }

            if (ImGui.Selectable(Describe(SourceKind.File, null), source.Kind == SourceKind.File))
                source.Kind = SourceKind.File;

            ImGui.EndCombo();
        }

        if (source.Kind != SourceKind.File)
            return;

        if (ImGui.Button($"{LOC.Get("DIALOG_Select")}##upgraderSelect_{id}"))
        {
            if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("PARAM_Upgrader_Generator_Select_Regulation"),
                    new[] { "bin" }, out var path) && !string.IsNullOrEmpty(path))
            {
                source.FilePath = path;
            }
        }

        ImGui.SameLine();
        ImGui.TextWrapped(string.IsNullOrEmpty(source.FilePath)
            ? LOC.Get("PARAM_Upgrader_Generator_No_File")
            : source.FilePath);
    }

    private byte[] ReadSource(RegulationHistory history, RegulationSource source)
    {
        return source.Kind switch
        {
            SourceKind.Game => history.ReadGameRegulation(),
            SourceKind.Snapshot when source.Snapshot != null => File.ReadAllBytes(source.Snapshot.Path),
            SourceKind.File when File.Exists(source.FilePath) => File.ReadAllBytes(source.FilePath),
            _ => throw new FileNotFoundException(LOC.Get("PARAM_Upgrader_Generator_Error_No_Source"))
        };
    }

    private void StartGenerate(RegulationHistory history)
    {
        byte[] olderData, newerData;

        try
        {
            olderData = ReadSource(history, OlderSource);
            newerData = ReadSource(history, NewerSource);
        }
        catch (Exception e)
        {
            SetGeneratorStatus(e.Message, true);
            return;
        }

        var context = history.CreateContext();

        GeneratorBusy = true;
        GeneratorNotes = new List<string>();
        SetGeneratorStatus(LOC.Get("PARAM_Upgrader_Generator_Status_Working"), false);

        Task.Run(() =>
        {
            try
            {
                var older = RegulationSnapshot.Load(olderData, context);
                var newer = RegulationSnapshot.Load(newerData, context);

                if (older.Version >= newer.Version)
                {
                    SetGeneratorStatus(LOC.Get("PARAM_Upgrader_Generator_Error_Order",
                        ParamUtils.ParseRegulationVersion(older.Version),
                        ParamUtils.ParseRegulationVersion(newer.Version)), true);
                    return;
                }

                var report = new RegulationDiffReport();
                var diff = RegulationDiffBuilder.Build(older, newer, context, "User", report);
                var path = history.SaveUserDiff(diff);

                var notes = new List<string>();
                notes.AddRange(older.Warnings.Select(w => $"{ParamUtils.ParseRegulationVersion(older.Version)}: {w}"));
                notes.AddRange(newer.Warnings.Select(w => $"{ParamUtils.ParseRegulationVersion(newer.Version)}: {w}"));
                notes.AddRange(report.Notes);
                GeneratorNotes = notes;

                SetGeneratorStatus(LOC.Get("PARAM_Upgrader_Generator_Status_Done",
                    ParamUtils.ParseRegulationVersion(older.Version),
                    ParamUtils.ParseRegulationVersion(newer.Version), path), false);

                Smithbox.Log(this, LOC.Get("PARAM_Upgrader_Generator_Status_Done",
                    ParamUtils.ParseRegulationVersion(older.Version),
                    ParamUtils.ParseRegulationVersion(newer.Version), path));

                // The missing step may be there now
                MissingVersion = 0;
            }
            catch (Exception e)
            {
                SetGeneratorStatus(LOC.Get("PARAM_Upgrader_Generator_Status_Failed", e.Message), true);
                Smithbox.LogError(this, LOC.Get("PARAM_Upgrader_Generator_Status_Failed", e.Message), e);
            }
            finally
            {
                GeneratorBusy = false;
            }
        });
    }

    private void SetGeneratorStatus(string text, bool error)
    {
        GeneratorStatusIsError = error;
        GeneratorStatus = text;
    }

    #endregion
}
