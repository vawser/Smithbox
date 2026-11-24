using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Merge (Behavior Editor Tools window): brings another version of the open behavior file (theirs: a loose .hkx or a
/// behbnd entry) into the project's, three ways against a base (the game's copy, or a picked file). Phase 1 of
/// Claude/Documentation/Havok Behavior Merge Analysis.md: the events and variables theirs adds, variable definitions and
/// the value fields of objects all three have; structure is listed as notes.
/// <para>
/// Analyze reads ours from the saved file, off the UI thread; Apply (the only way to apply) turns the plan into one undo
/// step on the live file. Any edit after Analyze makes the plan out of date.
/// </para>
/// </summary>
public class HavokBehaviorMergeTool
{
    private readonly HavokEditorView View;
    private readonly ProjectEntry Project;

    private HavokBehaviorView Behavior => View.PropertyView.BehaviorView;

    private static readonly string[] FileFilters = { "dcx", "hkx", "behbnd" };

    /// <summary>A picked file outside the project: its path, its behavior files (for a binder) and the one chosen</summary>
    private class PickedFile
    {
        public string Path = "";
        public List<string> Entries = new();
        public string Entry = "";
        public bool Listing;
    }

    private readonly PickedFile Source = new();
    private readonly PickedFile Base = new();
    private bool UseBaseFile;

    private HavokMergeStrategy Strategy = HavokMergeStrategy.Ask;

    private enum ShowFilter { Conflicts, Applied, All }

    private ShowFilter Show = ShowFilter.Conflicts;
    private string TextFilter = "";

    // The analysis: the plan, the file it's for, and whether an edit since has made it out of date
    private HavokBehaviorMerge Plan;
    private HavokFileWrapper PlanFile;
    private volatile bool Running;
    private int Run;
    private string AnalyzeError;
    private bool Stale;
    private bool Subscribed;

    private List<string> ApplyErrors = new();
    private List<string> ApplyNotes = new();
    private string ApplyResult;

    private const int MaxShown = 500;

    public HavokBehaviorMergeTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("HAVOK_Merge_Header")}##havokBehaviorMergeToolHeader"))
            return;

        ImGui.BeginChild("HavokBehaviorMergeToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("HAVOK_Merge_Hint"));

        var file = View.Selection.SelectedFileWrapper;

        if (!Behavior.IsShown || Behavior.Symbols == null || file == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("HAVOK_Scope_Needs_File"));
            return;
        }

        if (!Subscribed && View.ActionManager != null)
        {
            View.ActionManager.ActionPerformed += () => Stale = true;
            Subscribed = true;
        }

        // A plan for another file is dropped
        if (Plan != null && !ReferenceEquals(PlanFile, file))
            ClearPlan();

        var gameHasFile = View.HavokBank.EntryStatus(View.Selection.BinderFileEntry, false, file.Key) is not VanillaStatus.Added;

        // Theirs
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Merge_Source_Header"), LOC.Get("HAVOK_Merge_Source_Header_TT"));
        DisplayPicker("havokMergeSource", Source, file);

        // Base
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Merge_Base_Header"), LOC.Get("HAVOK_Merge_Base_Header_TT"));

        if (!gameHasFile)
        {
            UseBaseFile = true;
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Merge_Base_Not_In_Game"));
        }
        else
        {
            if (ImGui.RadioButton($"{LOC.Get("HAVOK_Merge_Base_Game")}##havokMergeBaseGame", !UseBaseFile))
                UseBaseFile = false;

            GUI.Tooltip(LOC.Get("HAVOK_Merge_Base_Game_TT"));
            ImGui.SameLine();

            if (ImGui.RadioButton($"{LOC.Get("HAVOK_Merge_Base_File")}##havokMergeBaseFile", UseBaseFile))
                UseBaseFile = true;

            GUI.Tooltip(LOC.Get("HAVOK_Merge_Base_File_TT"));
        }

        if (UseBaseFile)
            DisplayPicker("havokMergeBase", Base, file);

        // Analyze
        GUI.Spacer();

        var ready = Source.Entry.Length > 0 && (!UseBaseFile || Base.Entry.Length > 0) && !Running;

        if (file.IsDirty)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Merge_Save_First"));

        ImGui.BeginDisabled(!ready || file.IsDirty);

        if (ImGui.Button($"{LOC.Get("HAVOK_Merge_Analyze")}##havokMergeAnalyze"))
            StartAnalyze(file);

        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("HAVOK_Merge_Analyze_TT"));

        if (Running)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("HAVOK_Merge_Analyzing"));
        }

        if (AnalyzeError != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, AnalyzeError);

        if (ApplyResult != null)
        {
            GUI.Spacer();
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, ApplyResult);

            foreach (var note in ApplyNotes.Take(50))
                GUI.WrappedText(note);

            foreach (var error in ApplyErrors.Take(50))
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, error);
        }

        if (Plan != null)
            DisplayPlan(file);
    }

    #region Pickers
    private void DisplayPicker(string id, PickedFile picked, HavokFileWrapper file)
    {
        if (ImGui.Button($"{LOC.Get("HAVOK_Merge_Pick")}##{id}Pick"))
        {
            var folder = picked.Path.Length > 0 ? Path.GetDirectoryName(picked.Path) : CFG.Current.BehaviorEditor.BehaviorEditor_Merge_Last_Folder;

            if (PlatformUtils.Instance.OpenFileDialog(LOC.Get("HAVOK_Merge_Pick"), FileFilters, folder, out var path) && !string.IsNullOrEmpty(path))
            {
                CFG.Current.BehaviorEditor.BehaviorEditor_Merge_Last_Folder = Path.GetDirectoryName(path) ?? "";
                StartListing(picked, path, file);
            }
        }

        GUI.Tooltip(LOC.Get("HAVOK_Merge_Pick_TT"));
        ImGui.SameLine();

        if (picked.Path.Length == 0)
            ImGui.TextDisabled(LOC.Get("HAVOK_Merge_No_File"));
        else
            ImGui.TextUnformatted(Path.GetFileName(picked.Path));

        if (picked.Path.Length > 0)
            GUI.Tooltip(picked.Path);

        if (picked.Listing)
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_Merge_Reading_File"));
            return;
        }

        if (picked.Path.Length == 0)
            return;

        if (picked.Entries.Count == 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Merge_No_Behaviors"));
            return;
        }

        // A binder's behavior file
        if (picked.Entries.Count > 1)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo($"##{id}Entry", Path.GetFileName(picked.Entry.Replace('\\', '/'))))
            {
                foreach (var entry in picked.Entries)
                {
                    if (ImGui.Selectable(entry, entry == picked.Entry))
                    {
                        picked.Entry = entry;
                        ClearPlan();
                    }
                }

                ImGui.EndCombo();
            }

            GUI.Tooltip(LOC.Get("HAVOK_Merge_Entry_TT"));
        }
    }

    /// <summary>
    /// Lists a picked file's behavior files in the background, choosing the one with the open file's name
    /// </summary>
    private void StartListing(PickedFile picked, string path, HavokFileWrapper file)
    {
        picked.Path = path;
        picked.Entries = new();
        picked.Entry = "";
        picked.Listing = true;
        ClearPlan();

        var openName = Path.GetFileName(file.Key.Replace('\\', '/'));

        Task.Run(() =>
        {
            var entries = HavokBank.ListExternalBehaviorFiles(path);
            var match = entries.FirstOrDefault(e => string.Equals(Path.GetFileName(e.Replace('\\', '/')), openName, StringComparison.OrdinalIgnoreCase));

            if (picked.Path != path)
                return;

            picked.Entries = entries;
            picked.Entry = match ?? entries.FirstOrDefault() ?? "";
            picked.Listing = false;
        });
    }
    #endregion

    #region Analyze
    private void ClearPlan()
    {
        Plan = null;
        Running = false;
        PlanFile = null;
        AnalyzeError = null;
        Stale = false;
        Run++;
    }

    /// <summary>
    /// Reads base, ours (saved) and theirs, and builds the plan, in the background
    /// </summary>
    private void StartAnalyze(HavokFileWrapper file)
    {
        ClearPlan();
        ApplyResult = null;
        ApplyErrors = new();
        Running = true;

        var run = Run;
        var bank = View.HavokBank;
        var binder = View.Selection.BinderFileEntry;
        var key = file.Key;
        var source = (Source.Path, Source.Entry);
        var baseFile = UseBaseFile ? (Base.Path, Base.Entry) : (null, null);

        Task.Run(() =>
        {
            HavokBehaviorMerge plan = null;
            string error = null;

            try
            {
                var oursDocument = bank.ReadStoredDocument(binder, false, key);
                var theirsDocument = bank.ReadExternalDocument(source.Path, source.Entry, binder);
                var baseDocument = baseFile.Path != null
                    ? bank.ReadExternalDocument(baseFile.Path, baseFile.Entry, binder)
                    : bank.ReadVanillaDocument(binder, false, key);

                error = oursDocument == null ? LOC.Get("HAVOK_Merge_Error_Read_Ours")
                    : theirsDocument == null ? LOC.Get("HAVOK_Merge_Error_Read_Theirs")
                    : baseDocument == null ? LOC.Get("HAVOK_Merge_Error_Read_Base")
                    : null;

                if (error == null)
                {
                    var oursSide = Symbols(oursDocument);
                    var theirsSide = Symbols(theirsDocument);
                    var baseSide = Symbols(baseDocument);

                    if (oursSide.Symbols.StringData == null || theirsSide.Symbols.StringData == null || baseSide.Symbols.StringData == null)
                        error = LOC.Get("HAVOK_Merge_Error_Not_Behavior");
                    else if (oursSide.Symbols.StringData.GetType() != theirsSide.Symbols.StringData.GetType() ||
                             oursSide.Symbols.StringData.GetType() != baseSide.Symbols.StringData.GetType())
                        error = LOC.Get("HAVOK_Merge_Error_Version");
                    else
                        plan = HavokBehaviorMerge.Build(Project, baseSide.Symbols, baseSide.Fields, oursSide.Symbols, oursSide.Fields,
                            theirsSide.Symbols, theirsSide.Fields);
                }
            }
            catch (Exception e)
            {
                error = LOC.Get("HAVOK_Merge_Error_Failed", e.Message);
                Smithbox.LogError(this, error, e);
            }

            if (run != Run)
                return;

            Plan = plan;
            PlanFile = plan != null ? file : null;
            AnalyzeError = error;
            Stale = false;
            Running = false;
        });
    }

    private static (HavokBehaviorSymbols Symbols, Func<Type, System.Reflection.FieldInfo[]> Fields) Symbols(Havok.HavokDocument document)
    {
        // A field cache of its own per document: the editor's isn't thread-safe
        var cache = new HavokPropertyCache { ProfileSource = () => document.Profile };
        return (HavokBehaviorSymbols.Build(document.Root, cache.GetCachedHavokFields), cache.GetCachedHavokFields);
    }
    #endregion

    #region Plan
    private void DisplayPlan(HavokFileWrapper file)
    {
        var plan = Plan;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Tool_Results_Header"), LOC.Get("HAVOK_Merge_Results_Header_TT"));

        if (Stale)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Merge_Stale"));

        var conflictItems = plan.Items.Count(i => i.HasConflicts);

        ImGui.TextUnformatted(LOC.Get("HAVOK_Merge_Summary", plan.AddEvents.Count, plan.AddVariables.Count, plan.AutoFields,
            plan.ConflictFields, conflictItems, plan.Notes.Count));

        if (plan.Inserts.Count > 0)
            ImGui.TextUnformatted(LOC.Get("HAVOK_Merge_Summary_Inserts", plan.Inserts.Count(i => i.Include), plan.Inserts.Count));

        // Strategy
        ImGui.SetNextItemWidth(180f * DPI.UIScale());

        if (ImGui.BeginCombo("##havokMergeStrategy", LOC.Get($"HAVOK_Merge_Strategy_{Strategy}")))
        {
            foreach (var strategy in Enum.GetValues<HavokMergeStrategy>())
            {
                if (ImGui.Selectable(LOC.Get($"HAVOK_Merge_Strategy_{strategy}"), Strategy == strategy))
                    Strategy = strategy;
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("HAVOK_Merge_Strategy_TT"));

        // Apply
        var unresolved = plan.IsUnresolved(Strategy);
        var empty = plan.AddEvents.Count == 0 && plan.AddVariables.Count == 0 && plan.Items.Count == 0 &&
            !plan.Inserts.Any(i => i.Include) && !plan.Removals.Any(r => r.Include) &&
            !plan.Reorders.Any(r => r.Include) && !plan.RestoreVariables.Any(r => r.Include);

        ImGui.SameLine();
        ImGui.BeginDisabled(unresolved || Stale || empty);

        if (ImGui.Button($"{LOC.Get("HAVOK_Merge_Apply")}##havokMergeApply"))
            Apply(file);

        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get(unresolved ? "HAVOK_Merge_Apply_Unresolved_TT" : Stale ? "HAVOK_Merge_Apply_Stale_TT" : "HAVOK_Merge_Apply_TT"));

        if (empty)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("HAVOK_Merge_Nothing"));

        // Added events and variables
        if (plan.AddEvents.Count > 0 && ImGui.TreeNodeEx($"{LOC.Get("HAVOK_Merge_Add_Events", plan.AddEvents.Count)}##havokMergeEvents"))
        {
            foreach (var name in plan.AddEvents)
                ImGui.BulletText(name);

            ImGui.TreePop();
        }

        if (plan.AddVariables.Count > 0 && ImGui.TreeNodeEx($"{LOC.Get("HAVOK_Merge_Add_Variables", plan.AddVariables.Count)}##havokMergeVariables"))
        {
            foreach (var variable in plan.AddVariables)
                ImGui.BulletText($"{variable.Name}  ({variable.TypeName})");

            ImGui.TreePop();
        }

        // Entries their file adds to lists, by the object holding the list; each can be left out
        if (plan.Inserts.Count > 0 && ImGui.TreeNodeEx($"{LOC.Get("HAVOK_Merge_Inserts", plan.Inserts.Count)}##havokMergeInserts"))
        {
            GUI.WrappedText(LOC.Get("HAVOK_Merge_Inserts_Hint"));

            if (ImGui.SmallButton($"{LOC.Get("HAVOK_Merge_Inserts_All")}##havokMergeInsertsAll"))
                plan.Inserts.ForEach(i => i.Include = true);

            ImGui.SameLine();

            if (ImGui.SmallButton($"{LOC.Get("HAVOK_Merge_Inserts_None")}##havokMergeInsertsNone"))
                plan.Inserts.ForEach(i => i.Include = false);

            var groups = plan.Inserts.GroupBy(i => (i.OwnerType, i.OwnerName, i.OwnerOrdinal)).ToList();

            for (var g = 0; g < Math.Min(groups.Count, MaxShown); g++)
            {
                var group = groups[g];

                ImGui.PushID($"insertGroup{g}");

                if (ImGui.TreeNodeEx($"{group.Key.OwnerType}  {group.Key.OwnerName}  ({group.Count()})##group"))
                {
                    var n = 0;

                    foreach (var insert in group)
                    {
                        ImGui.PushID(n++);

                        var include = insert.Include;

                        if (ImGui.Checkbox($"{insert.Text}##insert", ref include))
                            insert.Include = include;

                        GUI.Tooltip(LOC.Get(insert.IsState ? "HAVOK_Merge_Insert_State_TT" : insert.Append ? "HAVOK_Merge_Insert_Append_TT" : "HAVOK_Merge_Insert_TT"));

                        if (insert.OursRemoved)
                        {
                            ImGui.SameLine();
                            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Merge_Insert_Ours_Removed"));
                            GUI.Tooltip(LOC.Get("HAVOK_Merge_Insert_Ours_Removed_TT"));
                        }

                        ImGui.PopID();
                    }

                    ImGui.TreePop();
                }

                ImGui.PopID();
            }

            ImGui.TreePop();
        }

        DisplayRemovals(plan);
        DisplayReorders(plan);
        DisplayRestoredVariables(plan);

        // Notes
        if (plan.Notes.Count > 0 && ImGui.TreeNodeEx($"{LOC.Get("HAVOK_Merge_Notes", plan.Notes.Count)}##havokMergeNotes"))
        {
            GUI.WrappedText(LOC.Get("HAVOK_Merge_Notes_Hint"));

            foreach (var group in plan.Notes.GroupBy(n => n.Kind))
            {
                if (!ImGui.TreeNodeEx($"{LOC.Get($"HAVOK_Merge_Note_Kind_{group.Key}")} ({group.Count()})##havokMergeNote{group.Key}"))
                    continue;

                foreach (var note in group.Take(MaxShown))
                    GUI.WrappedText(note.Text);

                ImGui.TreePop();
            }

            ImGui.TreePop();
        }

        if (plan.Items.Count == 0)
            return;

        // Items
        GUI.Spacer();
        ImGui.SetNextItemWidth(140f * DPI.UIScale());

        if (ImGui.BeginCombo("##havokMergeShow", LOC.Get($"HAVOK_Merge_Show_{Show}")))
        {
            foreach (var show in Enum.GetValues<ShowFilter>())
            {
                if (ImGui.Selectable(LOC.Get($"HAVOK_Merge_Show_{show}"), Show == show))
                    Show = show;
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##havokMergeText", LOC.Get("HAVOK_Changes_Filter_Hint"), ref TextFilter, 255);

        var shown = plan.Items.Where(i => Show switch
            {
                ShowFilter.Conflicts => i.HasConflicts,
                ShowFilter.Applied => i.Fields.Any(f => !f.IsConflict),
                _ => true
            })
            .Where(i => TextFilter.Length == 0 || i.Name.Contains(TextFilter, StringComparison.OrdinalIgnoreCase) ||
                i.TypeName.Contains(TextFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (shown.Count > MaxShown)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Scan_Capped", MaxShown));

        if (shown.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("HAVOK_Merge_None_Shown"));
            return;
        }

        ImGui.BeginChild("havokMergeItems");

        for (var i = 0; i < Math.Min(shown.Count, MaxShown); i++)
        {
            ImGui.PushID(i);
            DisplayItem(shown[i]);
            ImGui.PopID();
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// What their file removes that the project still has: list entries (ticked unless the project changed them), and
    /// events and variables (unticked), each with a tick box
    /// </summary>
    private void DisplayRemovals(HavokBehaviorMerge plan)
    {
        if (plan.Removals.Count == 0 ||
            !ImGui.TreeNodeEx($"{LOC.Get("HAVOK_Merge_Removals", plan.Removals.Count(r => r.Include), plan.Removals.Count)}##havokMergeRemovals"))
            return;

        GUI.WrappedText(LOC.Get("HAVOK_Merge_Removals_Hint"));

        if (ImGui.SmallButton($"{LOC.Get("HAVOK_Merge_Inserts_All")}##havokMergeRemovalsAll"))
            plan.Removals.ForEach(r => r.Include = true);

        ImGui.SameLine();

        if (ImGui.SmallButton($"{LOC.Get("HAVOK_Merge_Inserts_None")}##havokMergeRemovalsNone"))
            plan.Removals.ForEach(r => r.Include = false);

        var groups = plan.Removals.GroupBy(r => r.Kind is HavokMergeRemovalKind.Entry
                ? $"{r.OwnerType}  {r.OwnerName}"
                : LOC.Get($"HAVOK_Merge_Removals_{r.Kind}"))
            .ToList();

        for (var g = 0; g < Math.Min(groups.Count, MaxShown); g++)
        {
            var group = groups[g];

            ImGui.PushID($"removalGroup{g}");

            if (ImGui.TreeNodeEx($"{group.Key}  ({group.Count()})##group"))
            {
                var n = 0;

                foreach (var removal in group)
                {
                    ImGui.PushID(n++);

                    var include = removal.Include;

                    if (ImGui.Checkbox($"{removal.Text}##removal", ref include))
                        removal.Include = include;

                    GUI.Tooltip(LOC.Get(removal.Kind switch
                    {
                        HavokMergeRemovalKind.Event => "HAVOK_Merge_Removal_Event_TT",
                        HavokMergeRemovalKind.Variable => "HAVOK_Merge_Removal_Variable_TT",
                        _ => "HAVOK_Merge_Removal_Entry_TT"
                    }));

                    if (removal.OursChanged)
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Merge_Removal_Ours_Changed"));
                        GUI.Tooltip(LOC.Get("HAVOK_Merge_Removal_Ours_Changed_TT"));
                    }

                    ImGui.PopID();
                }

                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        ImGui.TreePop();
    }

    /// <summary>
    /// Lists their file puts in another order: ticked unless the project reordered them too
    /// </summary>
    private void DisplayReorders(HavokBehaviorMerge plan)
    {
        if (plan.Reorders.Count == 0 ||
            !ImGui.TreeNodeEx($"{LOC.Get("HAVOK_Merge_Reorders", plan.Reorders.Count(r => r.Include), plan.Reorders.Count)}##havokMergeReorders"))
            return;

        GUI.WrappedText(LOC.Get("HAVOK_Merge_Reorders_Hint"));

        for (var i = 0; i < Math.Min(plan.Reorders.Count, MaxShown); i++)
        {
            var reorder = plan.Reorders[i];

            ImGui.PushID($"reorder{i}");

            var include = reorder.Include;

            if (ImGui.Checkbox($"{reorder.OwnerType}  {reorder.OwnerName}: {reorder.Text}##reorder", ref include))
                reorder.Include = include;

            GUI.Tooltip(LOC.Get("HAVOK_Merge_Reorder_TT", string.Join("\n", reorder.Order.Take(30))));

            if (reorder.OursChanged)
            {
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Merge_Reorder_Ours_Changed"));
                GUI.Tooltip(LOC.Get("HAVOK_Merge_Reorder_Ours_Changed_TT"));
            }

            ImGui.PopID();
        }

        ImGui.TreePop();
    }

    /// <summary>
    /// Variables the project removed that their file changed: unticked, each can be brought back
    /// </summary>
    private void DisplayRestoredVariables(HavokBehaviorMerge plan)
    {
        if (plan.RestoreVariables.Count == 0 ||
            !ImGui.TreeNodeEx($"{LOC.Get("HAVOK_Merge_Restore_Variables", plan.RestoreVariables.Count(r => r.Include), plan.RestoreVariables.Count)}##havokMergeRestoreVariables"))
            return;

        GUI.WrappedText(LOC.Get("HAVOK_Merge_Restore_Variables_Hint"));

        for (var i = 0; i < plan.RestoreVariables.Count; i++)
        {
            var restore = plan.RestoreVariables[i];

            ImGui.PushID($"restoreVariable{i}");

            var include = restore.Include;

            if (ImGui.Checkbox($"{restore.Snapshot.Name}  ({restore.Snapshot.TypeName})##restore", ref include))
                restore.Include = include;

            ImGui.PopID();
        }

        ImGui.TreePop();
    }

    private void DisplayItem(HavokMergeItem item)
    {
        var conflicts = item.Fields.Count(f => f.IsConflict);
        var applied = item.Fields.Count - conflicts;
        var unresolved = item.Fields.Any(f => f.IsConflict && item.Effective(f, Strategy) is HavokMergeChoice.Inherit);

        var label = $"{item.TypeName}  {item.Name}  ({LOC.Get("HAVOK_Merge_Item_Counts", conflicts, applied)})";
        var color = conflicts == 0 ? VanillaMarkers.Colour(VanillaStatus.Modified)
            : unresolved ? UI.Current.ImGui_Warning_Text_Color
            : (System.Numerics.Vector4?)null;

        if (color != null)
            ImGui.PushStyleColor(ImGuiCol.Text, color.Value);

        var open = ImGui.TreeNodeEx($"{label}##item", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth);

        if (color != null)
            ImGui.PopStyleColor();

        if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
            Jump(item);

        GUI.Tooltip(LOC.Get(conflicts == 0 ? "HAVOK_Merge_Item_Applied_TT" : "HAVOK_Merge_Item_Conflict_TT"));

        // The item's side, for its conflicting fields
        if (conflicts > 0)
        {
            ImGui.SameLine();
            ChoiceButtons("item", item.Choice, c => item.Choice = c, includeInherit: false);
        }

        if (!open)
            return;

        if (ImGui.BeginTable("##fields", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
        {
            ImGui.TableSetupColumn(LOC.Get("HAVOK_Merge_Column_Field"));
            ImGui.TableSetupColumn(LOC.Get("HAVOK_Merge_Column_Base"));
            ImGui.TableSetupColumn(LOC.Get("HAVOK_Merge_Column_Ours"));
            ImGui.TableSetupColumn(LOC.Get("HAVOK_Merge_Column_Theirs"));
            ImGui.TableSetupColumn(LOC.Get("HAVOK_Merge_Column_Take"));
            ImGui.TableHeadersRow();

            for (var f = 0; f < item.Fields.Count; f++)
            {
                var field = item.Fields[f];

                ImGui.PushID(f);
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.TextUnformatted(field.DisplayPath);
                ImGui.TableNextColumn();
                ImGui.TextDisabled(field.BaseText);
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(field.OursText);
                ImGui.TableNextColumn();
                ImGui.TextUnformatted(field.TheirsText);
                ImGui.TableNextColumn();

                if (field.IsConflict)
                {
                    ChoiceButtons("field", field.Choice, c => field.Choice = c, includeInherit: true);

                    var effective = item.Effective(field, Strategy);

                    if (effective is HavokMergeChoice.Inherit)
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Merge_Unresolved"));
                    }
                }
                else
                {
                    ImGui.TextColored(VanillaMarkers.Colour(VanillaStatus.Modified) ?? UI.Current.ImGui_Default_Text_Color, LOC.Get("HAVOK_Merge_Theirs_Only"));
                    GUI.Tooltip(LOC.Get("HAVOK_Merge_Theirs_Only_TT"));
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }

        ImGui.TreePop();
    }

    /// <summary>
    /// Ours / Theirs radio buttons (and "Item" for a field, which takes the item's side)
    /// </summary>
    private static void ChoiceButtons(string id, HavokMergeChoice current, Action<HavokMergeChoice> set, bool includeInherit)
    {
        if (includeInherit)
        {
            if (ImGui.RadioButton($"{LOC.Get("HAVOK_Merge_Choice_Item")}##{id}Inherit", current is HavokMergeChoice.Inherit))
                set(HavokMergeChoice.Inherit);

            GUI.Tooltip(LOC.Get("HAVOK_Merge_Choice_Item_TT"));
            ImGui.SameLine();
        }

        if (ImGui.RadioButton($"{LOC.Get("HAVOK_Merge_Choice_Ours")}##{id}Ours", current is HavokMergeChoice.Ours))
            set(HavokMergeChoice.Ours);

        GUI.Tooltip(LOC.Get("HAVOK_Merge_Choice_Ours_TT"));
        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("HAVOK_Merge_Choice_Theirs")}##{id}Theirs", current is HavokMergeChoice.Theirs))
            set(HavokMergeChoice.Theirs);

        GUI.Tooltip(LOC.Get("HAVOK_Merge_Choice_Theirs_TT"));
    }

    private void Jump(HavokMergeItem item)
    {
        var symbols = Behavior.Symbols;

        if (item.Kind is HavokMergeKind.Variable)
        {
            var index = HavokBehaviorSymbols.IndexOfName(symbols.VariableNames, item.Name, 0);

            if (index >= 0)
                Behavior.VariableView.RequestSelect(index);

            return;
        }

        var n = 0;

        foreach (var obj in symbols.Objects)
        {
            if (obj?.GetType().Name != item.TypeName || HavokBehaviorSymbols.GetValue(obj, "m_name") as string != item.Name)
                continue;

            if (n++ == item.Ordinal)
            {
                Behavior.OutlineView.Navigate(obj);
                return;
            }
        }
    }
    #endregion

    #region Apply
    private void Apply(HavokFileWrapper file)
    {
        var context = new HavokBehaviorContext(file, Behavior.Symbols, Project)
        {
            Character = () => HavokBehaviorOperations.FindCharacterAnimations(View.HavokBank, View.Selection.BinderFileEntry,
                View.PropertyCache.GetCachedHavokFields)
        };

        var action = HavokBehaviorOperations.ApplyMerge(context, Plan, Strategy, out var errors, out var notes);

        ApplyErrors = errors;
        ApplyNotes = notes;

        if (action != null)
        {
            View.ActionManager.ExecuteAction(action);
            ApplyResult = LOC.Get(errors.Count > 0 ? "HAVOK_Merge_Applied_Some" : "HAVOK_Merge_Applied", errors.Count);
        }
        else
        {
            ApplyResult = LOC.Get("HAVOK_Merge_Applied_None");
        }

        ClearPlan();
    }
    #endregion
}
