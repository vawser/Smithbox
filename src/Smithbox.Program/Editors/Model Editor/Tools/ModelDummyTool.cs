using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// A set of dummy reference IDs written as numbers and ranges: "220", "200-299", "100, 120, 300-310"
/// </summary>
public sealed class RefIdQuery
{
    private readonly List<(int Min, int Max)> Ranges;

    private RefIdQuery(List<(int, int)> ranges)
    {
        Ranges = ranges;
    }

    public bool Contains(int id) => Ranges.Any(r => id >= r.Min && id <= r.Max);

    /// <summary>Null when the text isn't a list of numbers and ranges</summary>
    public static RefIdQuery Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var ranges = new List<(int, int)>();

        foreach (var part in text.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            // A dash after the first character separates a range, so "-1" stays a number
            var dash = part.IndexOf('-', 1);

            if (dash > 0)
            {
                if (!int.TryParse(part[..dash], NumberStyles.Integer, CultureInfo.InvariantCulture, out var min) ||
                    !int.TryParse(part[(dash + 1)..], NumberStyles.Integer, CultureInfo.InvariantCulture, out var max))
                    return null;

                ranges.Add((Math.Min(min, max), Math.Max(min, max)));
            }
            else
            {
                if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                    return null;

                ranges.Add((id, id));
            }
        }

        return ranges.Count > 0 ? new RefIdQuery(ranges) : null;
    }
}

/// <summary>
/// Dummies: select them by reference ID, and offset or set the reference IDs of many at once. Dummy IDs are what TAE
/// events, params and SFX refer to. Also gives the dummy rows of Contents their Select All With Ref ID item, and the
/// Contents filter its "ref:" form. Plan: Documentation/Model Editor Second Review.md, R8.
/// </summary>
public class ModelDummyTool
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    /// <summary>Brings the tool forward the next time the Tools window is drawn</summary>
    public bool FocusOnNextDraw;

    // Select
    private string SelectQueryText = "";

    // Change: which dummies, and what to do with their IDs
    private enum Scope { Selected, Query }
    private enum Operation { Offset, Set }

    private Scope ChangeScope = Scope.Selected;
    private string ChangeQueryText = "";
    private Operation ChangeOperation = Operation.Offset;
    private int ChangeValue = 100;

    public ModelDummyTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private ModelContainer Container
    {
        get
        {
            var wrapper = View.Selection.SelectedModelWrapper;
            return wrapper?.Container?.View == View ? wrapper.Container : null;
        }
    }

    public void OnToolWindow()
    {
        if (FocusOnNextDraw)
        {
            FocusOnNextDraw = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get("MODEL_Dummy_Tool_Header")}##modelDummyToolHeader"))
            return;

        ImGui.BeginChild("ModelDummyToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("MODEL_Dummy_Tool_Hint"));

        var container = Container;

        if (container == null || container.Dummies.Count == 0)
        {
            GUI.WrappedText(LOC.Get(container == null ? "MODEL_Dummy_Tool_No_Model" : "MODEL_Dummy_Tool_No_Dummies"));
            return;
        }

        DisplaySelect(container);
        DisplayChange(container);
    }

    #region Select
    private void DisplaySelect(ModelContainer container)
    {
        // Select by Ref ID
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Dummy_Tool_Select_Header"), LOC.Get("MODEL_Dummy_Tool_Select_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##modelDummySelectQuery", ref SelectQueryText, 255);
        GUI.Tooltip(LOC.Get("MODEL_Dummy_Tool_Query_TT"));

        var query = RefIdQuery.Parse(SelectQueryText);
        var matches = query == null ? 0 : GetDummies(container).Count(d => query.Contains(d.Dummy.ReferenceID));

        GUI.ConditionalMultiButtonInput("modelDummySelectActions",
            "modelDummySelectQueryButton", LOC.Get("MODEL_Dummy_Tool_Select_Query", matches), LOC.Get("MODEL_Dummy_Tool_Select_Query_TT"),
            () => SelectWhere(container, d => query.Contains(d.ReferenceID)), matches > 0,

            "modelDummySelectSameButton", LOC.Get("MODEL_Dummy_Tool_Select_Same"), LOC.Get("MODEL_Dummy_Tool_Select_Same_TT"),
            () => SelectSameRefIds(container), GetSelectedDummies().Count > 0);
    }

    private static List<(Entity Entity, FLVER.Dummy Dummy)> GetDummies(ModelContainer container)
    {
        return container.Dummies
            .Where(e => e.WrappedObject is FLVER.Dummy)
            .Select(e => (e, (FLVER.Dummy)e.WrappedObject))
            .ToList();
    }

    private List<(Entity Entity, FLVER.Dummy Dummy)> GetSelectedDummies()
    {
        return View.ViewportSelection.GetFilteredSelection<Entity>()
            .Where(e => e.WrappedObject is FLVER.Dummy && Container?.Objects.Contains(e) == true)
            .Select(e => (e, (FLVER.Dummy)e.WrappedObject))
            .ToList();
    }

    private void SelectWhere(ModelContainer container, Func<FLVER.Dummy, bool> predicate)
    {
        var targets = GetDummies(container).Where(d => predicate(d.Dummy)).Select(d => d.Entity).ToList();

        View.ViewportSelection.ClearSelection();

        foreach (var entity in targets)
        {
            View.ViewportSelection.AddSelection(entity);
        }

        if (targets.Count > 0)
        {
            View.ViewportSelection.GotoTreeTarget = targets[0];
        }
    }

    /// <summary>
    /// Selects every dummy sharing a reference ID with the selected ones, e.g. all the points of a hitbox
    /// </summary>
    public void SelectSameRefIds(ModelContainer container)
    {
        var ids = new HashSet<short>(GetSelectedDummies().Select(d => d.Dummy.ReferenceID));

        if (ids.Count > 0)
        {
            SelectWhere(container, d => ids.Contains(d.ReferenceID));
        }
    }

    /// <summary>
    /// The dummy rows' context menu items: Select All With Ref ID, and Change Reference IDs (opens this tool on the
    /// selection)
    /// </summary>
    public void OnContext(Entity entity)
    {
        if (entity?.WrappedObject is not FLVER.Dummy dummy || Container == null)
            return;

        ImGui.Separator();

        var selectedIds = GetSelectedDummies().Select(d => d.Dummy.ReferenceID).Distinct().ToList();

        // The row's own ID when the selection doesn't include it
        if (!selectedIds.Contains(dummy.ReferenceID))
            selectedIds = new List<short> { dummy.ReferenceID };

        var idText = selectedIds.Count <= 3 ? string.Join(", ", selectedIds) : LOC.Get("MODEL_Dummy_Context_Ids", selectedIds.Count);

        // Select All With Ref ID
        if (ImGui.Selectable($"{LOC.Get("MODEL_Dummy_Context_Select_Same", idText)}##selectSameRefId"))
        {
            var ids = new HashSet<short>(selectedIds);
            SelectWhere(Container, d => ids.Contains(d.ReferenceID));
        }
        GUI.Tooltip(LOC.Get("MODEL_Dummy_Context_Select_Same_TT"));

        // Change Reference IDs
        if (ImGui.Selectable($"{LOC.Get("MODEL_Dummy_Context_Change")}##changeRefIds"))
        {
            // Right-clicking doesn't select the row, so the change applies to it when it wasn't selected
            if (!View.ViewportSelection.GetSelection().Contains(entity))
            {
                View.ViewportSelection.ClearSelection();
                View.ViewportSelection.AddSelection(entity);
            }

            ChangeScope = Scope.Selected;

            CFG.Current.Interface_ModelEditor_ToolWindow = true;
            CFG.Current.Interface_ModelEditor_Tool_Dummies = true;
            FocusOnNextDraw = true;
        }
        GUI.Tooltip(LOC.Get("MODEL_Dummy_Context_Change_TT"));
    }
    #endregion

    #region Change
    private void DisplayChange(ModelContainer container)
    {
        // Change Reference IDs
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Dummy_Tool_Change_Header"), LOC.Get("MODEL_Dummy_Tool_Change_Header_TT"));

        // Which dummies
        var selected = GetSelectedDummies();

        if (ImGui.RadioButton($"{LOC.Get("MODEL_Dummy_Tool_Scope_Selected", selected.Count)}##modelDummyScopeSelected", ChangeScope == Scope.Selected))
            ChangeScope = Scope.Selected;
        GUI.Tooltip(LOC.Get("MODEL_Dummy_Tool_Scope_Selected_TT"));

        if (ImGui.RadioButton($"{LOC.Get("MODEL_Dummy_Tool_Scope_Query")}##modelDummyScopeQuery", ChangeScope == Scope.Query))
            ChangeScope = Scope.Query;
        GUI.Tooltip(LOC.Get("MODEL_Dummy_Tool_Scope_Query_TT"));

        if (ChangeScope == Scope.Query)
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputText("##modelDummyChangeQuery", ref ChangeQueryText, 255);
            GUI.Tooltip(LOC.Get("MODEL_Dummy_Tool_Query_TT"));
        }

        // What to do
        if (ImGui.RadioButton($"{LOC.Get("MODEL_Dummy_Tool_Operation_Offset")}##modelDummyOpOffset", ChangeOperation == Operation.Offset))
            ChangeOperation = Operation.Offset;
        GUI.Tooltip(LOC.Get("MODEL_Dummy_Tool_Operation_Offset_TT"));

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("MODEL_Dummy_Tool_Operation_Set")}##modelDummyOpSet", ChangeOperation == Operation.Set))
            ChangeOperation = Operation.Set;
        GUI.Tooltip(LOC.Get("MODEL_Dummy_Tool_Operation_Set_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##modelDummyChangeValue", ref ChangeValue);
        GUI.Tooltip(LOC.Get(ChangeOperation == Operation.Offset ? "MODEL_Dummy_Tool_Value_Offset_TT" : "MODEL_Dummy_Tool_Value_Set_TT"));

        // Preview
        var targets = ChangeScope == Scope.Selected
            ? selected
            : RefIdQuery.Parse(ChangeQueryText) is { } query
                ? GetDummies(container).Where(d => query.Contains(d.Dummy.ReferenceID)).ToList()
                : new();

        var changes = targets
            .Select(t => (t.Entity, t.Dummy, New: ChangeOperation == Operation.Offset ? t.Dummy.ReferenceID + ChangeValue : ChangeValue))
            .Where(c => c.New != c.Dummy.ReferenceID)
            .ToList();

        var outOfRange = changes.Count(c => c.New < short.MinValue || c.New > short.MaxValue);

        // IDs the change gives that other dummies already have: often wanted (several points per effect), so a note
        var targetSet = new HashSet<Entity>(targets.Select(t => t.Entity));
        var otherIds = new HashSet<int>(GetDummies(container).Where(d => !targetSet.Contains(d.Entity)).Select(d => (int)d.Dummy.ReferenceID));
        var shared = changes.Select(c => c.New).Distinct().Count(otherIds.Contains);

        if (targets.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_Dummy_Tool_No_Targets"));
        }
        else
        {
            var examples = changes
                .GroupBy(c => (Old: (int)c.Dummy.ReferenceID, c.New))
                .Take(4)
                .Select(g => $"{g.Key.Old} → {g.Key.New}");

            GUI.WrappedText(LOC.Get("MODEL_Dummy_Tool_Preview", changes.Count, targets.Count,
                changes.Count > 0 ? string.Join(", ", examples) : "-"));

            if (outOfRange > 0)
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_Dummy_Tool_Out_Of_Range", outOfRange));

            if (shared > 0)
                GUI.WrappedText(LOC.Get("MODEL_Dummy_Tool_Shared", shared));
        }

        GUI.ConditionalMultiButtonInput("modelDummyChangeActions",
            "modelDummyChangeApply", LOC.Get("MODEL_Dummy_Tool_Apply", changes.Count), LOC.Get("MODEL_Dummy_Tool_Apply_TT"),
            () => Apply(changes.Select(c => (c.Entity, (short)c.New)).ToList()),
            changes.Count > 0 && outOfRange == 0);
    }

    private static readonly PropertyInfo ReferenceIDProperty = typeof(FLVER.Dummy).GetProperty(nameof(FLVER.Dummy.ReferenceID));

    /// <summary>Sets the new IDs as one undo step</summary>
    private void Apply(List<(Entity Entity, short NewId)> changes)
    {
        if (changes.Count == 0)
            return;

        var actions = changes
            .Select(c => (ViewportAction)new ModelPropertyChangeAction(View, ReferenceIDProperty, new HashSet<Entity> { c.Entity },
                c.NewId, ownerObject: c.Entity.WrappedObject))
            .ToList();

        var compound = new ViewportCompoundAction(actions)
        {
            Label = LOC.Get("MODEL_Dummy_History_Change", changes.Count)
        };

        View.ViewportActionManager.ExecuteAction(compound);
    }
    #endregion
}
