using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Changes (Behavior Editor Tools window): what the open behavior file adds, removes or changes against the game's
/// copy, from the behavior view's change index (the same comparison the lists are coloured by), with value fields
/// revertable one by one or per object. See Claude/Documentation/Havok Behavior Changes Plan.md and Tool UI Styling.md.
/// </summary>
public class HavokBehaviorChangesTool
{
    private readonly HavokEditorView View;
    private readonly ProjectEntry Project;

    private HavokBehaviorView Behavior => View.PropertyView.BehaviorView;

    private enum KindFilter { All, Events, Variables, Objects }

    private KindFilter Filter = KindFilter.All;
    private string TextFilter = "";

    private const int MaxShown = 1000;

    public HavokBehaviorChangesTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("HAVOK_Changes_Header")}##havokBehaviorChangesToolHeader"))
            return;

        ImGui.BeginChild("HavokBehaviorChangesToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("HAVOK_Changes_Hint"));

        var symbols = Behavior.Symbols;

        if (!Behavior.IsShown || symbols == null || View.Selection.SelectedFileWrapper == null)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("HAVOK_Scope_Needs_File"));
            return;
        }

        var changes = Behavior.Changes;

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("HAVOK_Tool_Results_Header"), LOC.Get("HAVOK_Changes_Results_Header_TT"));

        if (changes.IsNewFile)
        {
            ImGui.TextColored(VanillaMarkers.Colour(VanillaStatus.Added) ?? UI.Current.ImGui_Default_Text_Color, LOC.Get("HAVOK_Changes_Not_In_Game"));
            return;
        }

        if (changes.IsUnchanged)
        {
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("HAVOK_Changes_None_Found"));
            return;
        }

        // Reading the game's copy: its progress
        if (changes.IsLoading)
        {
            changes.DrawProgress("havokChangesProgress");
            return;
        }

        if (changes.Diff == null)
        {
            ImGui.TextDisabled(LOC.Get(changes.IsPending ? "HAVOK_Changes_Reading_Game" : "HAVOK_Changes_Unavailable"));
            return;
        }

        if (changes.IsPending)
            ImGui.TextDisabled(LOC.Get("HAVOK_Changes_Updating"));

        DisplayResults(symbols, changes.Diff);
    }

    #region Results
    private void DisplayResults(HavokBehaviorSymbols symbols, HavokBehaviorDiff diff)
    {
        var changes = diff.Changes;

        if (changes.Count == 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("HAVOK_Changes_None_Found"));
            return;
        }

        ImGui.Text(LOC.Get("HAVOK_Changes_Summary",
            changes.Count(c => c.Status is HavokChangeStatus.Added),
            changes.Count(c => c.Status is HavokChangeStatus.Removed),
            changes.Count(c => c.Status is HavokChangeStatus.Modified)));

        // Filters
        ImGui.SetNextItemWidth(140f * DPI.UIScale());

        if (ImGui.BeginCombo("##havokChangesFilter", LOC.Get($"HAVOK_Changes_Filter_{Filter}")))
        {
            foreach (var filter in Enum.GetValues<KindFilter>())
            {
                if (ImGui.Selectable(LOC.Get($"HAVOK_Changes_Filter_{filter}"), Filter == filter))
                    Filter = filter;
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##havokChangesText", LOC.Get("HAVOK_Changes_Filter_Hint"), ref TextFilter, 255);

        var shown = changes.Where(c => Filter switch
            {
                KindFilter.Events => c.Kind is HavokChangeKind.Event,
                KindFilter.Variables => c.Kind is HavokChangeKind.Variable,
                KindFilter.Objects => c.Kind is HavokChangeKind.Object,
                _ => true
            })
            .Where(c => TextFilter.Length == 0 || c.Name.Contains(TextFilter, StringComparison.OrdinalIgnoreCase) ||
                c.TypeName.Contains(TextFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (shown.Count > MaxShown)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Scan_Capped", MaxShown));

        HavokChange revertObject = null;
        HavokFieldChange revertField = null;

        ImGui.BeginChild("havokChangesList");

        for (var i = 0; i < Math.Min(shown.Count, MaxShown); i++)
        {
            var change = shown[i];

            ImGui.PushID(i);

            // The shared vanilla colours (Added blue, Modified green); a removed entry is dimmed, as it isn't in the file
            var color = change.Status switch
            {
                HavokChangeStatus.Added => VanillaMarkers.Colour(VanillaStatus.Added),
                HavokChangeStatus.Modified => VanillaMarkers.Colour(VanillaStatus.Modified),
                _ => null
            } ?? ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled];

            var label = $"{LOC.Get($"HAVOK_Changes_Status_{change.Status}")}  {change.TypeName}  {change.Name}";

            if (change.Fields.Count > 0)
                label += $"  ({LOC.Get("HAVOK_Changes_Field_Count", change.Fields.Count)})";

            var flags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.SpanAvailWidth;

            if (change.Fields.Count == 0)
                flags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

            ImGui.PushStyleColor(ImGuiCol.Text, color);
            var open = ImGui.TreeNodeEx($"{label}##change", flags);
            ImGui.PopStyleColor();

            if (ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen())
                Jump(symbols, change);

            GUI.Tooltip(LOC.Get(change.Status is HavokChangeStatus.Removed ? "HAVOK_Changes_Removed_TT" : "HAVOK_Changes_Row_TT"));

            if (change.HasRevertable)
            {
                ImGui.SameLine();

                if (ImGui.SmallButton(LOC.Get("HAVOK_Changes_Revert_Values")))
                    revertObject = change;

                GUI.Tooltip(LOC.Get("HAVOK_Changes_Revert_Values_TT"));
            }

            if (open && change.Fields.Count > 0)
            {
                for (var f = 0; f < change.Fields.Count; f++)
                {
                    var field = change.Fields[f];

                    ImGui.PushID(f);

                    ImGui.TextDisabled(field.Path);
                    ImGui.SameLine();
                    ImGui.Text($"{field.OldText}  →  {field.NewText}");

                    if (field.Revertable)
                    {
                        ImGui.SameLine();

                        var canRevert = HavokBehaviorDiff.GetRevertValue(symbols, field) != null || field.VanillaValue == null && field.Role is HavokIndexRole.None;

                        ImGui.BeginDisabled(!canRevert);
                        if (ImGui.SmallButton(LOC.Get("HAVOK_Changes_Revert")))
                            revertField = field;
                        ImGui.EndDisabled();

                        GUI.Tooltip(LOC.Get(canRevert ? "HAVOK_Changes_Revert_TT" : "HAVOK_Changes_Revert_Missing_TT"));
                    }
                    else
                    {
                        ImGui.SameLine();
                        ImGui.TextDisabled(LOC.Get("HAVOK_Changes_Structural"));
                        GUI.Tooltip(LOC.Get("HAVOK_Changes_Structural_TT"));
                    }

                    ImGui.PopID();
                }

                ImGui.TreePop();
            }

            ImGui.PopID();
        }

        ImGui.EndChild();

        // After the list is drawn, since the edits change it
        if (revertField != null)
            Revert(symbols, new[] { revertField });
        else if (revertObject != null)
            Revert(symbols, revertObject.Fields.Where(f => f.Revertable));
    }

    private void Jump(HavokBehaviorSymbols symbols, HavokChange change)
    {
        switch (change.Kind)
        {
            case HavokChangeKind.Event:
            {
                var index = IndexOfName(symbols.EventNames, change.Name);
                if (index >= 0)
                    Behavior.EventView.RequestSelect(index);
                break;
            }
            case HavokChangeKind.Variable:
            {
                var index = IndexOfName(symbols.VariableNames, change.Name);
                if (index >= 0)
                    Behavior.VariableView.RequestSelect(index);
                break;
            }
            default:
                if (change.Live != null && symbols.Contains(change.Live))
                    Behavior.OutlineView.Navigate(change.Live);
                break;
        }
    }

    private static int IndexOfName(System.Collections.IList names, string name)
    {
        if (names == null)
            return -1;

        for (var i = 0; i < names.Count; i++)
        {
            if ((names[i] as string ?? "") == name)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// Puts fields back to the game's values, as one undo step, then compares again
    /// </summary>
    private void Revert(HavokBehaviorSymbols symbols, IEnumerable<HavokFieldChange> fields)
    {
        var file = View.Selection.SelectedFileWrapper;
        var actions = new List<EditorAction>();

        foreach (var field in fields)
        {
            // Null is a real value for a string the game leaves unset; otherwise it means the field can't go back now
            var value = HavokBehaviorDiff.GetRevertValue(symbols, field);

            if (value == null && !(field.Role is HavokIndexRole.None && field.VanillaValue == null))
                continue;

            actions.Add(new HavokChangeField(file, field.Field, field.Owner, value, field.Index));
        }

        if (actions.Count == 0)
            return;

        View.ActionManager.ExecuteAction(actions.Count == 1 ? actions[0] : new HavokCompoundAction(file, actions));
    }
    #endregion
}
