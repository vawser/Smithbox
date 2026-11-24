using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The behavior graph's events: each one's name, info, and what uses it (the transitions it triggers, and the fields
/// that send or listen for it). In the game's files an event drives at most two or three transitions, mostly
/// wildcards, so a row names where it leads (Tools/BehaviorSurvey eventsurvey).
/// See Claude/Documentation/Havok Behavior Events Category Plan.md.
/// </summary>
public class HavokEventView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    /// <summary>The selected event's index (the last one clicked), or -1</summary>
    public int SelectedIndex = -1;

    /// <summary>Every selected event (Ctrl+click adds), for Delete</summary>
    public readonly SortedSet<int> SelectedIndices = new();

    private bool ScrollToSelection = false;

    private enum EventFilter { All, Used, Unused, Duplicates }
    private EventFilter Filter = EventFilter.All;

    // The uses of every event, rebuilt after any edit (a plain field edit doesn't rebuild the symbols)
    private List<HavokEventUse>[] Uses;
    private HavokBehaviorSymbols UsesSymbols;
    private bool UsesDirty = true;
    private bool Subscribed = false;

    // Names used by more than one event (the game's DS3 files have some)
    private HashSet<string> DuplicateNames = new(StringComparer.Ordinal);

    private readonly List<int> Visible = new();

    public HavokEventView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        SelectedIndex = -1;
        SelectedIndices.Clear();
        UsesDirty = true;
    }

    public void Setup(object sourceObject)
    {
        var count = Owner.Symbols?.EventNames?.Count ?? 0;

        if (SelectedIndex >= count)
            SelectedIndex = -1;

        SelectedIndices.RemoveWhere(i => i >= count);
        UsesDirty = true;
    }

    /// <summary>
    /// Select an event and show this category (from an event field's menu, or after an edit)
    /// </summary>
    public void RequestSelect(int index)
    {
        SelectedIndex = index;
        SelectedIndices.Clear();
        SelectedIndices.Add(index);
        Owner.ContentsMode = HavokBehaviorContentsMode.Events;
        Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;
        ScrollToSelection = true;
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    #region Uses
    /// <summary>
    /// The uses of every event, by index (empty when the file has no events)
    /// </summary>
    public List<HavokEventUse>[] GetUses()
    {
        var symbols = Owner.Symbols;

        if (symbols == null)
            return Array.Empty<List<HavokEventUse>>();

        if (!Subscribed && View.ActionManager != null)
        {
            View.ActionManager.ActionPerformed += () => UsesDirty = true;
            Subscribed = true;
        }

        if (UsesDirty || Uses == null || !ReferenceEquals(UsesSymbols, symbols))
        {
            Uses = symbols.BuildEventUses(Project);
            UsesSymbols = symbols;
            UsesDirty = false;

            DuplicateNames = new HashSet<string>(StringComparer.Ordinal);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var name in symbols.EventNames?.Cast<object>() ?? Enumerable.Empty<object>())
            {
                var text = name as string ?? "";
                if (!seen.Add(text))
                    DuplicateNames.Add(text);
            }
        }

        return Uses;
    }

    private List<HavokEventUse> GetUses(int index)
    {
        var uses = GetUses();
        return index >= 0 && index < uses.Length ? uses[index] : new List<HavokEventUse>();
    }

    /// <summary>
    /// The dim text after an event's name: where its transition leads, how many uses it has, or nothing if unused
    /// </summary>
    private static string GetSuffix(List<HavokEventUse> uses)
    {
        if (uses.Count == 0)
            return "";

        var transitions = uses.Where(u => u.IsTransition).ToList();

        if (transitions.Count == 1 && uses.Count == 1)
        {
            var use = transitions[0];
            var machine = HavokBehaviorSymbols.GetValue(use.StateMachine, "m_name") as string ?? "?";
            var to = HavokBehaviorSymbols.GetStateName(use.StateMachine, use.ToStateId) ?? $"#{use.ToStateId}";

            return $"→ {machine} › {to}";
        }

        if (transitions.Count == 0)
            return LOC.Get("HAVOK_EventView_Suffix_Notify", uses.Count);

        return LOC.Get("HAVOK_EventView_Suffix_Uses", uses.Count);
    }
    #endregion

    #region Contents
    /// <summary>
    /// Draws this view in the Contents window (its category is selected)
    /// </summary>
    public void DisplayContents()
    {
        DisplayTabHeader();
        DisplayTabContents();
    }

    public void DisplayTabHeader()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

        ImGui.EndChild();

        var names = Owner.Symbols?.EventNames;

        GetUses();

        // Filter
        ImGui.SetNextItemWidth(160f * DPI.UIScale());

        if (ImGui.BeginCombo("##eventFilter", LOC.Get($"HAVOK_EventView_Filter_{Filter}")))
        {
            foreach (var filter in Enum.GetValues<EventFilter>())
            {
                // Only offered when the file has duplicate names
                if (filter is EventFilter.Duplicates && DuplicateNames.Count == 0)
                    continue;

                if (ImGui.Selectable(LOC.Get($"HAVOK_EventView_Filter_{filter}"), Filter == filter))
                    Filter = filter;

                GUI.Tooltip(LOC.Get($"HAVOK_EventView_Filter_{filter}_TT"));
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("HAVOK_EventView_Filter_TT"));

        if (Filter is EventFilter.Duplicates && DuplicateNames.Count == 0)
            Filter = EventFilter.All;

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("HAVOK_BehaviorActions_Add_Event")}##addEvent"))
            Owner.ActionsView.RequestAddEvent();

        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Add_Event_TT"));

        var hasSelection = HavokBehaviorSymbols.GetName(names, SelectedIndex) != null;

        ImGui.BeginDisabled(!hasSelection);

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("HAVOK_BehaviorActions_Delete_Event")}##deleteEvent"))
            Owner.ActionsView.RequestDeleteEvents(GetSelection());
        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Delete_Event_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("HAVOK_EventView_Copy_Name")}##copyEventName"))
            CopyNames(GetSelection());
        GUI.Tooltip(LOC.Get("HAVOK_EventView_Copy_Name_TT"));

        ImGui.EndDisabled();
    }

    /// <summary>
    /// The selected events, or the last clicked one alone
    /// </summary>
    public List<int> GetSelection()
    {
        var count = Owner.Symbols?.EventNames?.Count ?? 0;
        var selection = SelectedIndices.Where(i => i >= 0 && i < count).ToList();

        if (selection.Count == 0 && SelectedIndex >= 0 && SelectedIndex < count)
            selection.Add(SelectedIndex);

        return selection;
    }

    /// <summary>
    /// Copies the events' names to the clipboard, one per line
    /// </summary>
    public void CopyNames(IEnumerable<int> indices)
    {
        var names = Owner.Symbols?.EventNames;
        var text = string.Join("\n", indices.Select(i => HavokBehaviorSymbols.GetName(names, i)).Where(n => n != null));

        if (text.Length > 0)
            ImGui.SetClipboardText(text);
    }

    public void DisplayTabContents()
    {
        ImGui.BeginChild("havokBehaviorElementListSection");

        var names = Owner.Symbols?.EventNames;

        if (names == null || names.Count == 0)
        {
            DisplayNoEvents();
            ImGui.EndChild();
            return;
        }

        var uses = GetUses();

        Visible.Clear();

        for (var i = 0; i < names.Count; i++)
        {
            var name = names[i] as string ?? "";
            var eventUses = i < uses.Length ? uses[i] : null;
            var count = eventUses?.Count ?? 0;

            var keep = Filter switch
            {
                EventFilter.Used => count > 0,
                EventFilter.Unused => count == 0,
                EventFilter.Duplicates => DuplicateNames.Contains(name),
                _ => true
            };

            if (!keep)
                continue;

            // The name, the index, or where it leads (a state's name finds the events into it)
            var leadsTo = eventUses == null ? "" : string.Join(" ", eventUses.Where(u => u.IsTransition)
                .Select(u => HavokBehaviorSymbols.GetStateName(u.StateMachine, u.ToStateId) ?? ""));

            if (!EditorFilters.IsMatch(Owner.PropFilter, name, Owner.ExactPropFilter, i.ToString(), rawSecondaryText: leadsTo))
                continue;

            Visible.Add(i);
        }

        var scrollTo = ScrollToSelection ? Visible.IndexOf(SelectedIndex) : -1;
        ScrollToSelection = false;

        if (scrollTo >= 0)
            ImGui.SetScrollY(scrollTo * ImGui.GetTextLineHeightWithSpacing());

        var clipper = new ImGuiListClipper();
        clipper.Begin(Visible.Count);

        while (clipper.Step())
        {
            for (var row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var i = Visible[row];
                var name = names[i] as string ?? "";
                var selected = SelectedIndices.Contains(i) || (SelectedIndices.Count == 0 && SelectedIndex == i);

                // The row is an empty selectable with the name and suffix drawn over it
                if (ImGui.Selectable($"##event_{i}", selected, ImGuiSelectableFlags.AllowOverlap))
                    Select(i);

                if (ImGui.BeginPopupContextItem($"eventMenu_{i}"))
                {
                    if (!SelectedIndices.Contains(i))
                        Select(i);

                    DisplayEventItems();
                    ImGui.EndPopup();
                }

                ImGui.SameLine(0, 0);

                // Vanilla state: Modified / Added colour
                var status = Owner.Changes.EventStatus(i);
                var coloured = VanillaMarkers.PushColour(status);
                ImGui.Text($"{i}: {name}");
                VanillaMarkers.PopColour(coloured);
                VanillaMarkers.Tooltip(status);

                var suffix = GetSuffix(i < uses.Length ? uses[i] : new());

                if (suffix.Length > 0)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled(suffix);
                }

                if (DuplicateNames.Contains(name))
                {
                    ImGui.SameLine();
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_EventView_Duplicate_Tag"));
                    GUI.Tooltip(LOC.Get("HAVOK_EventView_Duplicate_TT"));
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private void Select(int index)
    {
        if (InputManager.HasCtrlDown())
        {
            if (!SelectedIndices.Remove(index))
                SelectedIndices.Add(index);
        }
        else
        {
            SelectedIndices.Clear();
            SelectedIndices.Add(index);
        }

        SelectedIndex = index;
        Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;
    }

    /// <summary>
    /// The right-click menu of an event row
    /// </summary>
    private void DisplayEventItems()
    {
        var selection = GetSelection();

        if (ImGui.MenuItem(LOC.Get("HAVOK_EventView_Copy_Name")))
            CopyNames(selection);

        if (ImGui.MenuItem(selection.Count > 1
                ? LOC.Get("HAVOK_BehaviorActions_Delete_Events", selection.Count)
                : LOC.Get("HAVOK_BehaviorActions_Delete_Event"),
                InputManager.GetHint(KeybindID.Delete)))
        {
            Owner.ActionsView.RequestDeleteEvents(selection);
        }

        if (ImGui.MenuItem(LOC.Get("HAVOK_BehaviorActions_Add_Event")))
            Owner.ActionsView.RequestAddEvent();
    }

    /// <summary>
    /// A file with no events: in ER and NR a character's own behavior has none, its events are in the shared
    /// c9997.hkx (or c0000.hkx for the player), so that file is offered
    /// </summary>
    private void DisplayNoEvents()
    {
        GUI.WrappedText(LOC.Get("HAVOK_EventView_No_Events"));

        var binder = View.Selection.BinderFileEntry;
        var files = binder == null ? null : View.HavokBank.GetBank(HavokCategoryMode.Behavior)?.GetValueOrDefault(binder)?.Keys;

        var shared = files?.FirstOrDefault(f =>
            Path.GetFileName(f.Replace('\\', '/')).StartsWith("c9997", StringComparison.OrdinalIgnoreCase)
            && f.Contains("Behaviors", StringComparison.OrdinalIgnoreCase));

        if (shared == null || binder == null)
            return;

        var fileName = Path.GetFileName(shared.Replace('\\', '/'));

        GUI.Spacer();

        if (ImGui.Button($"{LOC.Get("HAVOK_EventView_Open_Shared", fileName)}##openSharedEvents"))
            EditorCommandQueue.AddCommand($"behavior/select/{binder.Filename}/{fileName}");

        GUI.Tooltip(LOC.Get("HAVOK_EventView_Open_Shared_TT"));
    }
    #endregion

    #region Properties
    public bool CanDisplayProperties()
    {
        if (!IsCurrentTab || Owner.Symbols == null)
            return false;

        return HavokBehaviorSymbols.GetName(Owner.Symbols.EventNames, SelectedIndex) != null;
    }

    public void DisplayProperties()
    {
        var symbols = Owner.Symbols;

        if (symbols == null)
            return;

        var index = SelectedIndex;
        var name = HavokBehaviorSymbols.GetName(symbols.EventNames, index);

        if (name == null)
            return;

        ImGui.BeginChild("havokBehaviorPropEditSection", Owner.PropertySectionSize);

        var columnCount = CFG.Current.HavokEditor_Properties_Display_Type_Column ? 3 : 2;

        ImGui.Columns(columnCount);

        DisplayNameRow(symbols, index, name);
        DisplayIndexRow(index);

        var infos = HavokBehaviorSymbols.GetValue(symbols.GraphData, "m_eventInfos") as IList;
        var info = infos != null && index < infos.Count ? infos[index] : null;

        if (info != null)
        {
            var open = ImGui.TreeNodeEx($"{LOC.Get("HAVOK_EventView_Info")}##eventInfoNode", ImGuiTreeNodeFlags.DefaultOpen);
            GUI.Tooltip(LOC.Get("HAVOK_EventView_Info_TT"));

            ImGui.NextColumn();
            ImGui.NextColumn();

            if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            {
                ImGui.TextDisabled(info.GetType().FullName);
                ImGui.NextColumn();
            }

            if (open)
            {
                View.PropertyView.HavokPropEditGeneric(info, HavokMetaHelper.GetMeta(Project, info.GetType()));
                ImGui.TreePop();
            }
        }

        ImGui.Columns(1);

        DisplayUses(index);

        ImGui.EndChild();
    }

    /// <summary>
    /// The event's name, editable (an edit to the string data's name list), applied when the field is left
    /// </summary>
    private void DisplayNameRow(HavokBehaviorSymbols symbols, int index, string name)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("HAVOK_EventView_Name"));
        GUI.Tooltip(LOC.Get("HAVOK_EventView_Name_TT"));

        ImGui.NextColumn();

        ImGui.SetNextItemWidth(-1);
        var newName = name;

        ImGui.InputText($"##eventName_{index}", ref newName, 255);

        if (GUI.InputCommitted() && newName != name)
        {
            var field = HavokBehaviorSymbols.GetField(symbols.StringData.GetType(), "m_eventNames");

            if (field != null)
            {
                var action = new HavokChangeField(View.Selection.SelectedFileWrapper, field, symbols.StringData, newName, index);
                View.ActionManager.ExecuteAction(action);
            }
        }

        GUI.Tooltip(LOC.Get("HAVOK_EventView_Rename_Warning_TT"));

        // Another event with the same name (allowed: the game's DS3 files have them)
        if (DuplicateNames.Contains(name))
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_EventView_Duplicate_TT"));

        ImGui.NextColumn();

        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            ImGui.NextColumn();
    }

    private static void DisplayIndexRow(int index)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("HAVOK_EventView_Index"));
        GUI.Tooltip(LOC.Get("HAVOK_EventView_Index_TT"));

        ImGui.NextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextDisabled(index.ToString());
        ImGui.NextColumn();

        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
            ImGui.NextColumn();
    }

    /// <summary>
    /// Every field that names the event. Clicking a row shows it in the Outline (a transition shows the state it
    /// leads to); a transition can be deleted from its menu, another field set to None.
    /// </summary>
    private void DisplayUses(int index)
    {
        ImGui.Separator();

        var uses = GetUses(index);

        ImGui.Text(LOC.Get("HAVOK_EventView_Uses", uses.Count));
        GUI.Tooltip(LOC.Get("HAVOK_EventView_Uses_TT"));

        if (uses.Count == 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_EventView_Unused"));
            return;
        }

        HavokEventUse deleteTransition = null;
        HavokEventUse clearField = null;

        if (ImGui.BeginTable("eventUsesTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("HAVOK_EventView_Uses_Kind"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("HAVOK_EventView_Uses_Where"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("HAVOK_EventView_Uses_Field"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            for (var i = 0; i < uses.Count; i++)
            {
                var use = uses[i];

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.Text(LOC.Get($"HAVOK_EventView_Kind_{use.Kind}"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Selectable($"{HavokBehaviorSymbols.DescribeEventUse(use)}##eventUse{i}", false, ImGuiSelectableFlags.SpanAllColumns))
                    ShowUse(use);

                GUI.Tooltip(LOC.Get(use.IsTransition ? "HAVOK_EventView_Uses_Row_Transition_TT" : "HAVOK_EventView_Uses_Row_TT"));

                if (ImGui.BeginPopupContextItem($"eventUseMenu{i}"))
                {
                    if (ImGui.MenuItem(LOC.Get("HAVOK_EventView_Show_In_Outline")))
                        ShowUse(use);

                    if (use.IsTransition)
                    {
                        if (ImGui.MenuItem(LOC.Get("HAVOK_BehaviorActions_Delete_Transition")))
                            deleteTransition = use;

                        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Delete_Transition_TT"));
                    }
                    else
                    {
                        if (ImGui.MenuItem(LOC.Get("HAVOK_EventView_Set_None")))
                            clearField = use;

                        GUI.Tooltip(LOC.Get("HAVOK_EventView_Set_None_TT"));
                    }

                    ImGui.EndPopup();
                }

                ImGui.TableSetColumnIndex(2);
                ImGui.TextDisabled(use.Field.Name);
            }

            ImGui.EndTable();
        }

        // After the table is drawn, since the edits change it
        if (deleteTransition != null)
            Owner.ActionsView.DeleteTransition(deleteTransition.Owner);

        if (clearField != null)
        {
            var none = Convert.ChangeType(-1, clearField.Field.FieldType);
            View.ActionManager.ExecuteAction(new HavokChangeField(View.Selection.SelectedFileWrapper, clearField.Field, clearField.Owner, none));
        }
    }

    /// <summary>
    /// Shows a use in the Outline: a transition's target state (the Graph follows to its machine), else the object
    /// </summary>
    private void ShowUse(HavokEventUse use)
    {
        object target = use.Owner;

        if (use.IsTransition)
            target = HavokBehaviorSymbols.FindState(use.StateMachine, use.ToStateId) ?? use.FromState ?? use.Owner;

        Owner.OutlineView.Navigate(target);
    }
    #endregion
}
