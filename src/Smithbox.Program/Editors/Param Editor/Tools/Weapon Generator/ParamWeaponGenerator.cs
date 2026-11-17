using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Generates infusion sets for weapons. For each selected base row, one new row per enabled infusion, at the base
/// ID + the infusion's offset, copied from the base with the infusion's rules applied. Upgrade rows (base +1 … +99,
/// as in DS1) get the same treatment. Optionally adds the FMG entries (title from a template, other slots copied).
/// <para>
/// The infusions and their rules can be learned from the project's existing infused rows, and are saved with the
/// project.
/// </para>
/// </summary>
public class ParamWeaponGenerator
{
    private ParamEditorView View;
    private ProjectEntry Project;

    private string StorePath = "";

    public WeaponGeneratorStore Store = new();

    /// <summary>
    /// Goes up whenever the store changes, so the cached plan is rebuilt
    /// </summary>
    private int StoreVersion = 0;

    private string FieldFilter = "";
    private string ParamFilter = "";

    private bool ConfirmLearn = false;

    /// <summary>
    /// Rules whose details (source fields, group values) are shown
    /// </summary>
    private HashSet<WeaponGeneratorRule> ExpandedRules = new();

    private string LastResult = "";

    public ParamWeaponGenerator(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        StorePath = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Workflow", "Weapon Generator", "Weapon Generator.json");

        Load();
    }

    #region Plan

    /// <summary>
    /// One row to generate: the source (base or upgrade row), the infusion, and the new row's ID
    /// </summary>
    private class PlanItem
    {
        public Param.Row Source;
        public Param.Row Base;
        public WeaponGeneratorInfusion Infusion;
        public int TargetId;
        public bool Exists;
    }

    private class Plan
    {
        public List<PlanItem> Items = new();
        public int BaseCount;

        /// <summary>
        /// Selected rows whose ID isn't a multiple of the ID block, so they aren't base weapons
        /// </summary>
        public int IgnoredSelected;

        public int ToCreate => Items.Count(i => !i.Exists);
        public int Existing => Items.Count(i => i.Exists);
    }

    private Plan CachedPlan;
    private (Param Param, long Version, int Store, string Selection) PlanKey;

    private Param GetParam()
    {
        var bank = View.GetPrimaryBank();

        if (bank?.Params == null || string.IsNullOrEmpty(Store.ParamName))
            return null;

        return bank.Params.TryGetValue(Store.ParamName, out var param) ? param : null;
    }

    /// <summary>
    /// The selected rows of the target param, when it is the active param
    /// </summary>
    private List<Param.Row> GetSelectedRows(Param param)
    {
        if (param == null || View.Selection.GetActiveParam() != Store.ParamName)
            return new List<Param.Row>();

        var paramRows = new HashSet<Param.Row>(param.Rows);

        return View.Selection.GetSelectedRows()?
            .Where(r => r != null && paramRows.Contains(r))
            .Distinct()
            .OrderBy(r => r.ID)
            .ToList() ?? new List<Param.Row>();
    }

    private Plan GetPlan(Param param)
    {
        var selected = GetSelectedRows(param);
        var selectionKey = selected.Count == 0 ? "" : $"{selected.Count}:{selected[0].ID}:{selected[^1].ID}:{selected.Sum(r => (long)r.ID)}";
        var key = (param, param == null ? 0 : ParamDirtyTracker.GetVersion(param), StoreVersion, selectionKey);

        if (CachedPlan != null && PlanKey == key)
            return CachedPlan;

        CachedPlan = BuildPlan(param, selected);
        PlanKey = key;

        return CachedPlan;
    }

    private Plan BuildPlan(Param param, List<Param.Row> selected)
    {
        var plan = new Plan();

        if (param == null || selected.Count == 0)
            return plan;

        var block = Math.Max(1, Store.IdBlock);
        var byId = WeaponGeneratorCore.IndexById(param);
        var claimed = new HashSet<int>();

        foreach (var baseRow in selected)
        {
            if (baseRow.ID % block != 0)
            {
                plan.IgnoredSelected++;
                continue;
            }

            plan.BaseCount++;

            var sources = WeaponGeneratorCore.GetSourceRows(byId, baseRow, Store.IncludeUpgradeRows);

            foreach (var infusion in Store.Infusions.Where(i => i.Enabled))
            {
                foreach (var source in sources)
                {
                    var targetId = source.ID + infusion.IdOffset;

                    // Two infusions with the same offset, or overlapping bases: the first one wins
                    if (targetId < 0 || !claimed.Add(targetId))
                        continue;

                    plan.Items.Add(new PlanItem
                    {
                        Source = source,
                        Base = baseRow,
                        Infusion = infusion,
                        TargetId = targetId,
                        Exists = byId.ContainsKey(targetId)
                    });
                }
            }
        }

        return plan;
    }

    #endregion

    #region Generate

    private void Generate()
    {
        var param = GetParam();

        if (param == null)
            return;

        var plan = BuildPlan(param, GetSelectedRows(param));
        var overwrite = Store.ExistingMode == WeaponGeneratorExistingMode.Overwrite;

        var items = plan.Items.Where(i => overwrite || !i.Exists).ToList();

        if (items.Count == 0)
        {
            LastResult = LOC.Get("PARAM_WeaponGenerator_Result_Nothing");
            return;
        }

        var rows = new List<Param.Row>();
        var ruleErrors = new Dictionary<string, int>();

        foreach (var item in items)
        {
            // Built for this param and inserted as is (no second clone), so no row is left for the finalizer
            var row = new Param.Row(item.Source);
            row.ID = item.TargetId;
            row.Name = WeaponGeneratorCore.FormatName(item.Infusion.RowNameTemplate, item.Source.Name, item.Infusion.Name);

            foreach (var error in WeaponGeneratorCore.ApplyRules(row, item.Source, item.Infusion))
            {
                var key = $"{item.Infusion.Name}: {error}";
                ruleErrors[key] = ruleErrors.GetValueOrDefault(key) + 1;
            }

            rows.Add(row);
        }

        var action = new AddGeneratedRowsAction(View.Editor, param, rows, overwrite);

        // Refused while a save is running
        if (!View.Editor.ActionManager.ExecuteAction(action))
        {
            LastResult = LOC.Get("PARAM_WeaponGenerator_Result_Refused");
            return;
        }

        foreach (var (error, count) in ruleErrors)
        {
            Smithbox.LogWarning(this, LOC.Get("PARAM_WeaponGenerator_Rule_Error", error, count));
        }

        var textResult = "";

        if (Store.GenerateFmg)
        {
            var (added, changed, available) = GenerateFmgEntries(items);

            textResult = available
                ? LOC.Get("PARAM_WeaponGenerator_Result_Text", added, changed)
                : LOC.Get("PARAM_WeaponGenerator_Result_Text_Unavailable");
        }

        var overwritten = items.Count(i => i.Exists);

        LastResult = LOC.Get("PARAM_WeaponGenerator_Result_Rows", rows.Count - overwritten, overwritten, plan.BaseCount);

        if (textResult != "")
            LastResult += "\n" + textResult;

        if (ruleErrors.Count > 0)
            LastResult += "\n" + LOC.Get("PARAM_WeaponGenerator_Result_Rule_Errors", ruleErrors.Values.Sum());

        Smithbox.Log(this, LastResult.Replace("\n", " "));

        // Show the first new row of the first base
        var first = items.OrderBy(i => i.TargetId).First();
        EditorCommandQueue.AddCommand($@"param/select/{View.ViewIndex}/{Store.ParamName}/{first.TargetId}");
    }

    #endregion

    #region Text

    /// <summary>
    /// A text container in the primary language with the param's title FMGs (base first, then DLC)
    /// </summary>
    private class TitleSource
    {
        public TextContainerWrapper Container;
        public List<TextFmgWrapper> TitleFmgs = new();
    }

    private TextEditorView GetTextView()
    {
        return Project.Handler.TextEditor?.ViewHandler?.ActiveView;
    }

    private bool TextAvailable()
    {
        return GetTextView() != null && Project.Handler.TextData?.PrimaryBank?.Containers != null;
    }

    /// <summary>
    /// The containers holding the param's title FMGs, from the Row FMG Annotations
    /// </summary>
    private List<TitleSource> GetTitleSources()
    {
        var result = new List<TitleSource>();

        var annotation = Project.Handler.ParamData?.RowFmgAnnotations?.Entries?.FirstOrDefault(e => e.Param == Store.ParamName);
        var containers = Project.Handler.TextData?.PrimaryBank?.Containers;

        if (annotation == null || containers == null)
            return result;

        var names = new[] { annotation.FmgName_Base, annotation.FmgName_DLC1, annotation.FmgName_DLC2 }
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        foreach (var (_, container) in containers)
        {
            if (container.IsContainerUnused())
                continue;

            if (container.ContainerDisplayCategory != CFG.Current.TextEditor_Primary_Category)
                continue;

            if (container.FmgWrappers == null)
                continue;

            var source = new TitleSource { Container = container };

            foreach (var name in names)
            {
                var wrapper = container.FmgWrappers.FirstOrDefault(w =>
                    TextUtils.GetFmgInternalName(Project, container, w.ID, w.Name) == name);

                if (wrapper?.File != null)
                    source.TitleFmgs.Add(wrapper);
            }

            if (source.TitleFmgs.Count > 0)
                result.Add(source);
        }

        return result;
    }

    /// <summary>
    /// Title text by row ID, from the first title FMG (base, then DLC) that has a non-empty entry
    /// </summary>
    private Dictionary<int, string> GetTitleTexts()
    {
        var titles = new Dictionary<int, string>();

        foreach (var source in GetTitleSources())
        {
            foreach (var wrapper in source.TitleFmgs)
            {
                foreach (var entry in wrapper.File.Entries)
                {
                    if (!string.IsNullOrWhiteSpace(entry.Text))
                        titles.TryAdd(entry.ID, entry.Text);
                }
            }
        }

        return titles;
    }

    /// <summary>
    /// The other FMGs of the title FMG's text entry group (summary, description, …) in the container
    /// </summary>
    private List<TextFmgWrapper> GetOtherSlotFmgs(TextContainerWrapper container, TextFmgWrapper titleFmg)
    {
        var groups = Project.Handler.TextData?.EntryGroups?.Groups;
        var group = groups?.FirstOrDefault(g => g.Slots.Any(s => s.FmgID == titleFmg.ID));

        if (group == null)
            return new List<TextFmgWrapper>();

        return group.Slots
            .Where(s => s.FmgID != titleFmg.ID)
            .Select(s => container.FmgWrappers.FirstOrDefault(w => w.ID == s.FmgID))
            .Where(w => w?.File != null)
            .ToList();
    }

    /// <summary>
    /// Adds (or with Overwrite, changes) the FMG entries for the generated rows, as one undoable Text Editor action.
    /// A row gets text only when its source row has a title. The entry goes in the FMG that holds the source's title.
    /// </summary>
    private (int Added, int Changed, bool Available) GenerateFmgEntries(List<PlanItem> items)
    {
        var textView = GetTextView();

        if (textView == null)
            return (0, 0, false);

        var sources = GetTitleSources();

        if (sources.Count == 0)
            return (0, 0, false);

        var actions = new List<EditorAction>();
        var added = 0;
        var changed = 0;

        foreach (var source in sources)
        {
            // Entry lookups per FMG, kept up to date with the entries this run adds
            var lookups = new Dictionary<FMG, Dictionary<int, FMG.Entry>>();

            Dictionary<int, FMG.Entry> Lookup(FMG fmg)
            {
                if (!lookups.TryGetValue(fmg, out var lookup))
                {
                    lookup = new Dictionary<int, FMG.Entry>();

                    foreach (var e in fmg.Entries)
                        lookup.TryAdd(e.ID, e);

                    lookups[fmg] = lookup;
                }

                return lookup;
            }

            void Write(FMG fmg, int id, string text)
            {
                var lookup = Lookup(fmg);

                if (lookup.TryGetValue(id, out var existing))
                {
                    if (!Store.OverwriteFmg || existing.Text == text)
                        return;

                    actions.Add(new ChangeFmgEntryText(textView, source.Container, existing, text));
                    changed++;
                    return;
                }

                var newEntry = new FMG.Entry(fmg, id, text);
                actions.Add(new AddFmgEntry(textView, source.Container, newEntry, newEntry, id));
                lookup[id] = newEntry;
                added++;
            }

            foreach (var item in items)
            {
                // The title FMG that holds the source's title (base or DLC)
                TextFmgWrapper titleFmg = null;
                FMG.Entry sourceTitle = null;

                foreach (var wrapper in source.TitleFmgs)
                {
                    if (Lookup(wrapper.File).TryGetValue(item.Source.ID, out var entry) && !string.IsNullOrWhiteSpace(entry.Text))
                    {
                        titleFmg = wrapper;
                        sourceTitle = entry;
                        break;
                    }
                }

                if (titleFmg == null)
                    continue;

                var title = WeaponGeneratorCore.FormatName(item.Infusion.FmgTitleTemplate, sourceTitle.Text, item.Infusion.Name);
                Write(titleFmg.File, item.TargetId, title);

                if (!Store.CopyOtherTextSlots)
                    continue;

                foreach (var slotFmg in GetOtherSlotFmgs(source.Container, titleFmg))
                {
                    if (Lookup(slotFmg.File).TryGetValue(item.Source.ID, out var slotEntry) && !string.IsNullOrEmpty(slotEntry.Text))
                        Write(slotFmg.File, item.TargetId, slotEntry.Text);
                }
            }
        }

        if (actions.Count > 0)
            textView.ActionManager.ExecuteAction(new FmgGroupedAction(actions));

        return (added, changed, true);
    }

    #endregion

    #region Learn

    private void Learn()
    {
        var param = GetParam();

        if (param == null)
            return;

        Func<int, string> titleText = null;

        if (TextAvailable())
        {
            var titles = GetTitleTexts();

            if (titles.Count > 0)
                titleText = id => titles.GetValueOrDefault(id);
        }

        // The group fields are used for every rule; the stat fields only with the damage and scaling option
        var stats = new WeaponGeneratorStatOptions
        {
            LearnStats = Store.LearnStatRules,
            StatFields = Store.StatFields,
            GroupFields = Store.GroupFields
        };

        var result = WeaponGeneratorCore.Learn(param, Store.IdBlock, titleText, stats);

        if (result.Infusions.Count == 0)
        {
            LastResult = LOC.Get("PARAM_WeaponGenerator_Learn_None", Store.ParamName);
            return;
        }

        Store.IdBlock = result.IdBlock;
        Store.Infusions = result.Infusions;
        Changed();

        LastResult = LOC.Get("PARAM_WeaponGenerator_Learn_Result",
            result.Infusions.Count, result.BaseCount, result.IdBlock,
            result.Infusions.Sum(i => i.Rules.Count));

        if (result.IgnoredOffsets.Count > 0)
        {
            LastResult += "\n" + LOC.Get("PARAM_WeaponGenerator_Learn_Ignored",
                string.Join(", ", result.IgnoredOffsets.Select(o => $"+{o}")));
        }

        Smithbox.Log(this, LastResult.Replace("\n", " "));
    }

    #endregion

    #region Display

    public void Display()
    {
        if (!ToolSections.Header($"{LOC.Get("PARAM_WeaponGenerator_Header")}##weaponGeneratorHeader"))
            return;

        ImGui.BeginChild("WeaponGeneratorToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("PARAM_WeaponGenerator_Hint"));

        var param = GetParam();

        DisplayTarget(param);
        DisplayOptions();
        DisplayInfusions(param);
        DisplayGenerate(param);

        ImGui.EndChild();
    }

    private void DisplayTarget(Param param)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_WeaponGenerator_Header_Target"),
            LOC.Get("PARAM_WeaponGenerator_Header_Target_TT"));

        var bank = View.GetPrimaryBank();

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##weaponGeneratorParam", Store.ParamName ?? "", ImGuiComboFlags.HeightLarge))
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##weaponGeneratorParamFilter", LOC.Get("PARAM_WeaponGenerator_Filter_Hint"), ref ParamFilter, 128);

            if (bank?.Params != null)
            {
                foreach (var key in bank.Params.Keys.OrderBy(k => k))
                {
                    if (ParamFilter != "" && !key.Contains(ParamFilter, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (ImGui.Selectable(key, key == Store.ParamName))
                    {
                        Store.ParamName = key;
                        Changed();
                    }
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Param_TT"));

        if (param == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_WeaponGenerator_Param_Missing", Store.ParamName));
        }
    }

    private void DisplayOptions()
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_WeaponGenerator_Header_Options"),
            LOC.Get("PARAM_WeaponGenerator_Header_Options_TT"));

        // ID Block
        var block = Store.IdBlock;
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.5f);
        ImGui.InputInt($"{LOC.Get("PARAM_WeaponGenerator_Id_Block")}##weaponGeneratorBlock", ref block, 0, 0);
        Store.IdBlock = Math.Max(1, block);

        if (GUI.InputCommitted())
            Changed();
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Id_Block_TT"));

        var upgrade = Store.IncludeUpgradeRows;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_WeaponGenerator_Upgrade_Rows")}##weaponGeneratorUpgrade", ref upgrade))
        {
            Store.IncludeUpgradeRows = upgrade;
            Changed();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Upgrade_Rows_TT"));

        // Existing rows
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("PARAM_WeaponGenerator_Existing"));
        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##weaponGeneratorExisting", GetExistingModeLabel(Store.ExistingMode)))
        {
            foreach (var mode in Enum.GetValues<WeaponGeneratorExistingMode>())
            {
                if (ImGui.Selectable(GetExistingModeLabel(mode), mode == Store.ExistingMode))
                {
                    Store.ExistingMode = mode;
                    Changed();
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Existing_TT"));

        // Text
        var textAvailable = TextAvailable();

        ImGui.BeginDisabled(!textAvailable);

        var fmg = Store.GenerateFmg;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_WeaponGenerator_Generate_Fmg")}##weaponGeneratorFmg", ref fmg))
        {
            Store.GenerateFmg = fmg;
            Changed();
        }
        GUI.Tooltip(LOC.Get(textAvailable ? "PARAM_WeaponGenerator_Generate_Fmg_TT" : "PARAM_WeaponGenerator_Generate_Fmg_Unavailable_TT"));

        ImGui.BeginDisabled(!Store.GenerateFmg);
        ImGui.Indent();

        var copy = Store.CopyOtherTextSlots;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_WeaponGenerator_Copy_Slots")}##weaponGeneratorCopySlots", ref copy))
        {
            Store.CopyOtherTextSlots = copy;
            Changed();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Copy_Slots_TT"));

        var overwriteFmg = Store.OverwriteFmg;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_WeaponGenerator_Overwrite_Fmg")}##weaponGeneratorOverwriteFmg", ref overwriteFmg))
        {
            Store.OverwriteFmg = overwriteFmg;
            Changed();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Overwrite_Fmg_TT"));

        ImGui.Unindent();
        ImGui.EndDisabled();
        ImGui.EndDisabled();
    }

    private string GetExistingModeLabel(WeaponGeneratorExistingMode mode)
    {
        return mode == WeaponGeneratorExistingMode.Overwrite
            ? LOC.Get("PARAM_WeaponGenerator_Existing_Overwrite")
            : LOC.Get("PARAM_WeaponGenerator_Existing_Skip");
    }

    private void DisplayInfusions(Param param)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_WeaponGenerator_Header_Infusions"),
            LOC.Get("PARAM_WeaponGenerator_Header_Infusions_TT"));

        // Learn replaces the list, so it asks first when there is one
        if (ConfirmLearn)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("PARAM_WeaponGenerator_Learn_Confirm", Store.Infusions.Count));

            GUI.ConditionalMultiButtonInput("weaponGeneratorLearnConfirm",
                "weaponGeneratorLearnYes",
                LOC.Get("PARAM_WeaponGenerator_Learn_Replace"),
                LOC.Get("PARAM_WeaponGenerator_Learn_Replace_TT"),
                () => { ConfirmLearn = false; Learn(); }, true,

                "weaponGeneratorLearnNo",
                LOC.Get("PARAM_WeaponGenerator_Cancel"),
                "",
                () => { ConfirmLearn = false; }, true);
        }
        else
        {
            GUI.ConditionalMultiButtonInput("weaponGeneratorInfusionActions",
                "weaponGeneratorLearn",
                LOC.Get("PARAM_WeaponGenerator_Learn"),
                LOC.Get("PARAM_WeaponGenerator_Learn_TT"),
                () =>
                {
                    if (Store.Infusions.Count > 0)
                        ConfirmLearn = true;
                    else
                        Learn();
                }, param != null,

                "weaponGeneratorAddInfusion",
                LOC.Get("PARAM_WeaponGenerator_Add_Infusion"),
                LOC.Get("PARAM_WeaponGenerator_Add_Infusion_TT"),
                AddInfusion, true,

                "weaponGeneratorClearInfusions",
                LOC.Get("PARAM_WeaponGenerator_Clear_Infusions"),
                LOC.Get("PARAM_WeaponGenerator_Clear_Infusions_TT"),
                () => { Store.Infusions.Clear(); Changed(); }, Store.Infusions.Count > 0);
        }

        DisplayLearnOptions();

        if (Store.Infusions.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PARAM_WeaponGenerator_No_Infusions"));
            return;
        }

        int removeIndex = -1;
        int moveFrom = -1;
        int moveTo = -1;

        var fields = GetFields(param);

        for (int i = 0; i < Store.Infusions.Count; i++)
        {
            var infusion = Store.Infusions[i];

            ImGui.PushID($"weaponGeneratorInfusion_{i}");

            var enabled = infusion.Enabled;
            if (ImGui.Checkbox("##infusionEnabled", ref enabled))
            {
                infusion.Enabled = enabled;
                Changed();
            }
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Infusion_Enabled_TT"));

            ImGui.SameLine();

            var label = LOC.Get("PARAM_WeaponGenerator_Infusion_Label",
                string.IsNullOrEmpty(infusion.Name) ? "?" : infusion.Name, infusion.IdOffset, infusion.Rules.Count);

            // Room for the row buttons, at the right of the line
            var buttonWidth = ImGui.GetFrameHeight();
            var spacing = ImGui.GetStyle().ItemSpacing.X;
            var buttonsWidth = buttonWidth * 3 + spacing * 2;
            var lineStart = ImGui.GetCursorPosX();
            var lineWidth = ImGui.GetContentRegionAvail().X;

            if (!infusion.Enabled)
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

            var open = ImGui.TreeNodeEx($"{Utils.ImGuiEscape(label)}##infusionNode", ImGuiTreeNodeFlags.SpanAvailWidth | ImGuiTreeNodeFlags.AllowOverlap);

            if (!infusion.Enabled)
                ImGui.PopStyleColor(1);

            ImGui.SameLine(lineStart + Math.Max(0, lineWidth - buttonsWidth));

            ImGui.BeginDisabled(i == 0);
            if (ImGui.Button($"{Icons.ArrowUp}##infusionUp", new Vector2(buttonWidth, 0)))
            {
                moveFrom = i;
                moveTo = i - 1;
            }
            ImGui.EndDisabled();
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Move_Up_TT"));

            ImGui.SameLine();

            ImGui.BeginDisabled(i == Store.Infusions.Count - 1);
            if (ImGui.Button($"{Icons.ArrowDown}##infusionDown", new Vector2(buttonWidth, 0)))
            {
                moveFrom = i;
                moveTo = i + 1;
            }
            ImGui.EndDisabled();
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Move_Down_TT"));

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Trash}##infusionRemove", new Vector2(buttonWidth, 0)))
            {
                removeIndex = i;
            }
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Infusion_Remove_TT"));

            if (open)
            {
                DisplayInfusion(infusion, fields);
                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        if (moveFrom >= 0 && moveTo >= 0 && moveTo < Store.Infusions.Count)
        {
            (Store.Infusions[moveFrom], Store.Infusions[moveTo]) = (Store.Infusions[moveTo], Store.Infusions[moveFrom]);
            Changed();
        }

        if (removeIndex >= 0)
        {
            Store.Infusions.RemoveAt(removeIndex);
            Changed();
        }
    }

    private List<(string Name, string Display)> CachedFields;
    private PARAMDEF CachedFieldsDef;

    /// <summary>
    /// The param's fields (internal name, display name), without padding. Cached per paramdef.
    /// </summary>
    private List<(string Name, string Display)> GetFields(Param param)
    {
        if (CachedFields != null && CachedFieldsDef == param?.AppliedParamdef)
            return CachedFields;

        var result = new List<(string, string)>();

        CachedFields = result;
        CachedFieldsDef = param?.AppliedParamdef;

        if (param?.AppliedParamdef == null)
            return result;

        var paramData = View.GetParamData();
        var paramMeta = paramData.GetParamMeta(param.AppliedParamdef);

        foreach (var field in param.AppliedParamdef.Fields)
        {
            if (field.DisplayType == PARAMDEF.DefType.dummy8)
                continue;

            var altName = paramMeta == null ? null : paramData.GetParamFieldMeta(paramMeta, field)?.AltName;

            result.Add((field.InternalName, string.IsNullOrEmpty(altName) ? field.InternalName : $"{field.InternalName} ({altName})"));
        }

        return result;
    }

    private void DisplayInfusion(WeaponGeneratorInfusion infusion, List<(string Name, string Display)> fields)
    {
        var labelWidth = ImGui.GetContentRegionAvail().X * 0.35f;

        // Name
        var name = infusion.Name ?? "";
        LabeledField(LOC.Get("PARAM_WeaponGenerator_Infusion_Name"), labelWidth);
        ImGui.InputText("##infusionName", ref name, 128);
        infusion.Name = name;
        if (GUI.InputCommitted())
            Changed();
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Infusion_Name_TT"));

        // Offset
        var offset = infusion.IdOffset;
        LabeledField(LOC.Get("PARAM_WeaponGenerator_Infusion_Offset"), labelWidth);
        ImGui.InputInt("##infusionOffset", ref offset, 0, 0);
        infusion.IdOffset = offset;
        if (GUI.InputCommitted())
            Changed();
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Infusion_Offset_TT"));

        // Row name template
        var rowTemplate = infusion.RowNameTemplate ?? "";
        LabeledField(LOC.Get("PARAM_WeaponGenerator_Infusion_Row_Name"), labelWidth);
        ImGui.InputText("##infusionRowName", ref rowTemplate, 256);
        infusion.RowNameTemplate = rowTemplate;
        if (GUI.InputCommitted())
            Changed();
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Infusion_Template_TT"));

        // FMG title template
        var fmgTemplate = infusion.FmgTitleTemplate ?? "";
        LabeledField(LOC.Get("PARAM_WeaponGenerator_Infusion_Fmg_Title"), labelWidth);
        ImGui.InputText("##infusionFmgTitle", ref fmgTemplate, 256);
        infusion.FmgTitleTemplate = fmgTemplate;
        if (GUI.InputCommitted())
            Changed();
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Infusion_Template_TT"));

        // Rules
        GUI.Spacer();
        ImGui.TextDisabled(LOC.Get("PARAM_WeaponGenerator_Rules"));
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Rules_TT"));

        if (infusion.Rules.Count == 0)
            ImGui.TextDisabled(LOC.Get("PARAM_WeaponGenerator_No_Rules"));

        int removeIndex = -1;

        var buttonWidth = ImGui.GetFrameHeight();
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var opWidth = Enum.GetValues<WeaponGeneratorRuleOp>().Max(op => ImGui.CalcTextSize(GetOpLabel(op)).X)
            + ImGui.GetFrameHeight() + ImGui.GetStyle().FramePadding.X * 2;

        for (int r = 0; r < infusion.Rules.Count; r++)
        {
            var rule = infusion.Rules[r];

            ImGui.PushID($"rule_{r}");

            var ruleEnabled = rule.Enabled;
            if (ImGui.Checkbox("##ruleEnabled", ref ruleEnabled))
            {
                rule.Enabled = ruleEnabled;
                Changed();
            }
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Rule_Enabled_TT"));

            ImGui.SameLine();

            // The remaining width is split between the field, the operation and the value
            var avail = ImGui.GetContentRegionAvail().X - opWidth - buttonWidth * 2 - spacing * 4;
            var fieldWidth = Math.Max(60, avail * 0.6f);
            var valueWidth = Math.Max(40, avail - fieldWidth);

            // Field (an empty field is a new rule, not an unknown one)
            var known = fields.Count == 0 || string.IsNullOrEmpty(rule.Field) || fields.Any(f => f.Name == rule.Field);

            var pickedField = FieldCombo("##ruleField", rule.Field, fields, fieldWidth, false);

            if (pickedField != null)
            {
                rule.Field = pickedField;
                Changed();
            }

            var fieldDisplay = fields.FirstOrDefault(f => f.Name == rule.Field).Display;
            GUI.Tooltip(known
                ? (fieldDisplay ?? LOC.Get("PARAM_WeaponGenerator_Rule_Field_TT"))
                : LOC.Get("PARAM_WeaponGenerator_Rule_Field_Unknown_TT", rule.Field));

            ImGui.SameLine();

            // Operation
            ImGui.SetNextItemWidth(opWidth);

            if (ImGui.BeginCombo("##ruleOp", GetOpLabel(rule.Op)))
            {
                foreach (var op in Enum.GetValues<WeaponGeneratorRuleOp>())
                {
                    if (ImGui.Selectable(GetOpLabel(op), op == rule.Op))
                    {
                        rule.Op = op;
                        Changed();
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Rule_Op_TT"));

            ImGui.SameLine();

            // Value
            var value = rule.Value ?? "";
            ImGui.SetNextItemWidth(valueWidth);
            ImGui.InputText("##ruleValue", ref value, 256);
            rule.Value = value;
            if (GUI.InputCommitted())
                Changed();
            GUI.Tooltip(LOC.Get(string.IsNullOrWhiteSpace(rule.GroupField)
                ? "PARAM_WeaponGenerator_Rule_Value_TT"
                : "PARAM_WeaponGenerator_Rule_Value_Grouped_TT"));

            ImGui.SameLine();

            // Details: source fields, group field and group values
            var expanded = ExpandedRules.Contains(rule);
            var hasDetails = rule.Op == WeaponGeneratorRuleOp.ScaleFrom || !string.IsNullOrWhiteSpace(rule.GroupField);

            if (expanded || hasDetails)
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);

            if (ImGui.Button($"{Icons.Cog}##ruleDetails", new Vector2(buttonWidth, 0)))
            {
                if (!ExpandedRules.Add(rule))
                    ExpandedRules.Remove(rule);
            }

            if (expanded || hasDetails)
                ImGui.PopStyleColor(1);

            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Rule_Details_TT"));

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Trash}##ruleRemove", new Vector2(buttonWidth, 0)))
            {
                removeIndex = r;
            }
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Rule_Remove_TT"));

            DisplayRuleSummary(rule);

            if (ExpandedRules.Contains(rule))
                DisplayRuleDetails(rule, fields);

            ImGui.PopID();
        }

        if (removeIndex >= 0)
        {
            infusion.Rules.RemoveAt(removeIndex);
            Changed();
        }

        if (ImGui.Button($"{LOC.Get("PARAM_WeaponGenerator_Add_Rule")}##ruleAdd"))
        {
            infusion.Rules.Add(new WeaponGeneratorRule());
            Changed();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Add_Rule_TT"));

        GUI.Spacer();
    }

    private static void LabeledField(string label, float labelWidth)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(label);
        ImGui.SameLine(labelWidth);
        ImGui.SetNextItemWidth(-1);
    }

    private string GetOpLabel(WeaponGeneratorRuleOp op)
    {
        return op switch
        {
            WeaponGeneratorRuleOp.Add => LOC.Get("PARAM_WeaponGenerator_Op_Add"),
            WeaponGeneratorRuleOp.Multiply => LOC.Get("PARAM_WeaponGenerator_Op_Multiply"),
            WeaponGeneratorRuleOp.ScaleFrom => LOC.Get("PARAM_WeaponGenerator_Op_Scale_From"),
            _ => LOC.Get("PARAM_WeaponGenerator_Op_Set")
        };
    }

    /// <summary>
    /// The stat rule option of Learn, and the fields it looks at
    /// </summary>
    private void DisplayLearnOptions()
    {
        var learnStats = Store.LearnStatRules;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_WeaponGenerator_Learn_Stats")}##weaponGeneratorLearnStats", ref learnStats))
        {
            Store.LearnStatRules = learnStats;
            Changed();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Learn_Stats_TT"));

        ImGui.Indent();

        var labelWidth = ImGui.GetContentRegionAvail().X * 0.35f;

        if (Store.LearnStatRules)
        {
            var statFields = Store.StatFields ?? "";
            LabeledField(LOC.Get("PARAM_WeaponGenerator_Stat_Fields"), labelWidth);
            ImGui.InputText("##weaponGeneratorStatFields", ref statFields, 512);
            Store.StatFields = statFields;
            if (GUI.InputCommitted())
                Changed();
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Stat_Fields_TT"));
        }

        // Used by every learned rule, not only the stat rules
        var groupFields = Store.GroupFields ?? "";
        LabeledField(LOC.Get("PARAM_WeaponGenerator_Group_Fields"), labelWidth);
        ImGui.InputText("##weaponGeneratorGroupFields", ref groupFields, 512);
        Store.GroupFields = groupFields;
        if (GUI.InputCommitted())
            Changed();
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Group_Fields_TT"));

        ImGui.Unindent();
    }

    /// <summary>
    /// A field picker with a filter. Returns the picked field (or "" for None, when allowed), or null if nothing
    /// was picked this frame.
    /// </summary>
    private string FieldCombo(string id, string current, List<(string Name, string Display)> fields, float width, bool allowNone)
    {
        string picked = null;

        var known = fields.Count == 0 || string.IsNullOrEmpty(current) || fields.Any(f => f.Name == current);

        if (!known)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Warning_Text_Color);

        ImGui.SetNextItemWidth(width);
        var preview = string.IsNullOrEmpty(current)
            ? LOC.Get(allowNone ? "PARAM_WeaponGenerator_Field_None" : "PARAM_WeaponGenerator_Rule_Field_None")
            : current;
        var open = ImGui.BeginCombo(id, preview, ImGuiComboFlags.HeightLarge);

        if (!known)
            ImGui.PopStyleColor(1);

        if (open)
        {
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint($"{id}Filter", LOC.Get("PARAM_WeaponGenerator_Filter_Hint"), ref FieldFilter, 128);

            if (allowNone && ImGui.Selectable(LOC.Get("PARAM_WeaponGenerator_Field_None"), string.IsNullOrEmpty(current)))
                picked = "";

            foreach (var (fieldName, display) in fields)
            {
                if (FieldFilter != "" && !display.Contains(FieldFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable(Utils.ImGuiEscape(display), fieldName == current))
                    picked = fieldName;
            }

            ImGui.EndCombo();
        }

        return picked;
    }

    /// <summary>
    /// A one-line summary under rules that have more than a field, op and value
    /// </summary>
    private void DisplayRuleSummary(WeaponGeneratorRule rule)
    {
        var parts = new List<string>();

        if (rule.Op == WeaponGeneratorRuleOp.ScaleFrom)
            parts.Add(LOC.Get("PARAM_WeaponGenerator_Summary_From", rule.SourceFields ?? ""));

        if (!string.IsNullOrWhiteSpace(rule.GroupField))
            parts.Add(LOC.Get("PARAM_WeaponGenerator_Summary_Group", rule.GroupField, rule.Groups?.Count ?? 0));

        if (rule.Round)
            parts.Add(LOC.Get("PARAM_WeaponGenerator_Summary_Round"));

        if (rule.Accuracy.HasValue)
            parts.Add(LOC.Get("PARAM_WeaponGenerator_Summary_Fit", Math.Round(rule.Accuracy.Value * 100)));

        if (parts.Count == 0)
            return;

        ImGui.Indent(ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
        GUI.WrappedText(string.Join(" · ", parts));
        ImGui.PopStyleColor(1);
        ImGui.Unindent(ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X);

        if (rule.Accuracy.HasValue)
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Summary_Fit_TT"));
    }

    /// <summary>
    /// The rule's source fields (Scale From), group field and group values
    /// </summary>
    private void DisplayRuleDetails(WeaponGeneratorRule rule, List<(string Name, string Display)> fields)
    {
        var indent = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X;

        ImGui.Indent(indent);

        var labelWidth = ImGui.GetContentRegionAvail().X * 0.3f;

        // Rounding
        var round = rule.Round;
        if (ImGui.Checkbox($"{LOC.Get("PARAM_WeaponGenerator_Rule_Round")}##ruleRound", ref round))
        {
            rule.Round = round;
            Changed();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Rule_Round_TT"));

        // Source fields
        if (rule.Op == WeaponGeneratorRuleOp.ScaleFrom)
        {
            var sources = rule.SourceFields ?? "";
            LabeledField(LOC.Get("PARAM_WeaponGenerator_Rule_Sources"), labelWidth);
            ImGui.InputText("##ruleSources", ref sources, 512);
            rule.SourceFields = sources;
            if (GUI.InputCommitted())
                Changed();

            var unknown = WeaponGeneratorCore.SplitFields(rule.SourceFields)
                .Where(f => fields.Count > 0 && !fields.Any(x => x.Name == f))
                .ToList();

            GUI.Tooltip(unknown.Count == 0
                ? LOC.Get("PARAM_WeaponGenerator_Rule_Sources_TT")
                : LOC.Get("PARAM_WeaponGenerator_Rule_Field_Unknown_TT", string.Join(", ", unknown)));
        }

        // Group field
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("PARAM_WeaponGenerator_Rule_Group"));
        ImGui.SameLine(labelWidth);

        var pickedGroup = FieldCombo("##ruleGroupField", rule.GroupField, fields, -1, true);

        if (pickedGroup != null)
        {
            rule.GroupField = pickedGroup;
            Changed();
        }
        GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Rule_Group_TT"));

        // Group values
        if (!string.IsNullOrWhiteSpace(rule.GroupField))
        {
            rule.Groups ??= new();

            int removeIndex = -1;
            var buttonWidth = ImGui.GetFrameHeight();
            var spacing = ImGui.GetStyle().ItemSpacing.X;

            // Long tables (one value per weapon type) scroll
            var rowHeight = ImGui.GetFrameHeightWithSpacing();
            var scroll = rule.Groups.Count > 8;

            if (scroll)
                ImGui.BeginChild("##ruleGroups", new Vector2(0, rowHeight * 8), ImGuiChildFlags.Borders);

            for (int g = 0; g < rule.Groups.Count; g++)
            {
                var group = rule.Groups[g];

                ImGui.PushID($"group_{g}");

                var half = (ImGui.GetContentRegionAvail().X - buttonWidth - spacing * 2) / 2;

                var key = group.Key ?? "";
                ImGui.SetNextItemWidth(half);
                ImGui.InputTextWithHint("##groupKey", rule.GroupField, ref key, 128);
                group.Key = key;
                if (GUI.InputCommitted())
                    Changed();
                GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Group_Key_TT", rule.GroupField));

                ImGui.SameLine();

                var groupValue = group.Value ?? "";
                ImGui.SetNextItemWidth(half);
                ImGui.InputText("##groupValue", ref groupValue, 128);
                group.Value = groupValue;
                if (GUI.InputCommitted())
                    Changed();
                GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Group_Value_TT"));

                ImGui.SameLine();

                if (ImGui.Button($"{Icons.Trash}##groupRemove", new Vector2(buttonWidth, 0)))
                    removeIndex = g;
                GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Group_Remove_TT"));

                ImGui.PopID();
            }

            if (scroll)
                ImGui.EndChild();

            if (removeIndex >= 0)
            {
                rule.Groups.RemoveAt(removeIndex);
                Changed();
            }

            if (ImGui.Button($"{LOC.Get("PARAM_WeaponGenerator_Add_Group")}##groupAdd"))
            {
                rule.Groups.Add(new WeaponGeneratorRuleGroup { Value = rule.Value });
                Changed();
            }
            GUI.Tooltip(LOC.Get("PARAM_WeaponGenerator_Add_Group_TT"));
        }

        ImGui.Unindent(indent);
        GUI.Spacer();
    }

    private void DisplayGenerate(Param param)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_WeaponGenerator_Header_Generate"),
            LOC.Get("PARAM_WeaponGenerator_Header_Generate_TT"));

        var plan = GetPlan(param);

        if (param != null && View.Selection.GetActiveParam() != Store.ParamName)
        {
            GUI.WrappedText(LOC.Get("PARAM_WeaponGenerator_Select_Param", Store.ParamName));
        }
        else if (plan.BaseCount == 0 && plan.IgnoredSelected == 0)
        {
            GUI.WrappedText(LOC.Get("PARAM_WeaponGenerator_Select_Rows"));
        }
        else
        {
            var overwrite = Store.ExistingMode == WeaponGeneratorExistingMode.Overwrite;

            GUI.WrappedText(LOC.Get("PARAM_WeaponGenerator_Plan", plan.ToCreate, plan.BaseCount, Store.Infusions.Count(i => i.Enabled)));

            if (plan.Existing > 0)
            {
                GUI.WrappedTextColored(overwrite ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_AliasName_Text,
                    LOC.Get(overwrite ? "PARAM_WeaponGenerator_Plan_Overwrite" : "PARAM_WeaponGenerator_Plan_Skip", plan.Existing));
            }

            if (plan.IgnoredSelected > 0)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text,
                    LOC.Get("PARAM_WeaponGenerator_Plan_Not_Base", plan.IgnoredSelected, Store.IdBlock));
            }
        }

        var canGenerate = plan.ToCreate > 0 || (Store.ExistingMode == WeaponGeneratorExistingMode.Overwrite && plan.Existing > 0);

        GUI.ConditionalMultiButtonInput("weaponGeneratorGenerate",
            "weaponGeneratorGenerateButton",
            LOC.Get("PARAM_WeaponGenerator_Generate"),
            LOC.Get("PARAM_WeaponGenerator_Generate_TT"),
            Generate, canGenerate,

            "weaponGeneratorClearResult",
            LOC.Get("PARAM_WeaponGenerator_Clear_Result"),
            "",
            () => { LastResult = ""; }, LastResult != "");

        if (LastResult != "")
        {
            GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, LastResult);
        }
    }

    private void AddInfusion()
    {
        var nextOffset = Store.Infusions.Count == 0 ? 100 : Store.Infusions.Max(i => i.IdOffset) + 100;

        Store.Infusions.Add(new WeaponGeneratorInfusion
        {
            Name = LOC.Get("PARAM_WeaponGenerator_New_Infusion"),
            IdOffset = nextOffset
        });

        Changed();
    }

    #endregion

    #region Storage

    private void Changed()
    {
        StoreVersion++;
        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(StorePath))
                return;

            var json = File.ReadAllText(StorePath);
            var store = JsonSerializer.Deserialize(json, WeaponGeneratorSerializationContext.Default.WeaponGeneratorStore);

            if (store != null)
            {
                store.Infusions ??= new();

                foreach (var infusion in store.Infusions)
                {
                    infusion.Rules ??= new();

                    foreach (var rule in infusion.Rules)
                        rule.Groups ??= new();
                }

                // Settings saved before these options existed
                store.StatFields ??= new WeaponGeneratorStore().StatFields;
                store.GroupFields ??= new WeaponGeneratorStore().GroupFields;

                Store = store;
                StoreVersion++;
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamWeaponGenerator>($"[Param Editor] Failed to load the Weapon Generator settings from {StorePath}: {e.Message}");
        }
    }

    private void Save()
    {
        try
        {
            var dir = Path.GetDirectoryName(StorePath);

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var json = JsonSerializer.Serialize(Store, WeaponGeneratorSerializationContext.Default.WeaponGeneratorStore);
            File.WriteAllText(StorePath, json);
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamWeaponGenerator>($"[Param Editor] Failed to save the Weapon Generator settings to {StorePath}: {e.Message}");
        }
    }

    #endregion
}
