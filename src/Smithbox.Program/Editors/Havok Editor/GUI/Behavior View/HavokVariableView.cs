using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Reflection;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The behavior graph's variables, from its string data and graph data:
/// name, info, bounds, initial value, and the bindings that use each one
/// </summary>
public class HavokVariableView
{
    private HavokEditorView View;
    private ProjectEntry Project;
    private HavokBehaviorView Owner;

    public bool IsCurrentTab = false;

    /// <summary>
    /// The selected variable's index, or -1
    /// </summary>
    public int SelectedIndex = -1;

    /// <summary>
    /// Set by <see cref="RequestSelect"/>, so the tab is brought to the front on the next draw
    /// </summary>
    private bool ScrollToSelection = false;

    public HavokVariableView(HavokEditorView view, HavokBehaviorView ownerView, ProjectEntry project)
    {
        View = view;
        Project = project;
        Owner = ownerView;
    }

    public void ResetSelection()
    {
        SelectedIndex = -1;
    }

    public void Setup(object sourceObject)
    {
        // Drop a selection that is no longer in the list (after a structural edit)
        var names = Owner.Symbols?.VariableNames;

        if (names == null || SelectedIndex >= names.Count)
        {
            SelectedIndex = -1;
        }
    }

    /// <summary>
    /// Select a variable and show this tab (used by the binding rows in the property view)
    /// </summary>
    public void RequestSelect(int index)
    {
        SelectedIndex = index;
        Owner.ContentsMode = HavokBehaviorContentsMode.Variables;
        Owner.SelectionSource = HavokBehaviorSelectionSource.Contents;
        ScrollToSelection = true;
    }

    public void SetTabState(bool state)
    {
        IsCurrentTab = state;
    }

    /// <summary>
    /// Draws this view in the Contents window (its category is selected)
    /// </summary>
    public void DisplayContents()
    {
        DisplayTabHeader();
        DisplayTabContents();
    }

    public void DisplayTabContents()
    {
        ImGui.BeginChild("havokBehaviorElementListSection");

        var names = Owner.Symbols?.VariableNames;

        if (names != null)
        {
            for (int i = 0; i < names.Count; i++)
            {
                var name = names[i] as string ?? "";

                var isMatch = EditorFilters.IsMatch(Owner.PropFilter, name, Owner.ExactPropFilter);

                if (!isMatch)
                    continue;

                var selected = SelectedIndex == i;

                if (ImGui.Selectable($"{i}: {name}##variable_{i}", selected))
                {
                    SelectedIndex = i;
                }

                if (selected && ScrollToSelection)
                {
                    ImGui.SetScrollHereY();
                    ScrollToSelection = false;
                }
            }
        }

        ImGui.EndChild();
    }

    public void DisplayTabHeader()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBehaviorTabListSearch", ref Owner.PropFilter, ref Owner.ExactPropFilter);

        ImGui.EndChild();

        if (ImGui.Button($"{LOC.Get("HAVOK_BehaviorActions_Add_Variable")}##addVariable"))
            Owner.ActionsView.RequestAddVariable();

        GUI.Tooltip(LOC.Get("HAVOK_BehaviorActions_Add_Variable_TT"));
    }

    public bool CanDisplayProperties()
    {
        if (!IsCurrentTab)
            return false;

        if (Owner.Symbols == null)
            return false;

        return HavokBehaviorSymbols.GetName(Owner.Symbols.VariableNames, SelectedIndex) != null;
    }

    public void DisplayProperties()
    {
        var symbols = Owner.Symbols;

        if (symbols == null)
            return;

        var index = SelectedIndex;
        var name = HavokBehaviorSymbols.GetName(symbols.VariableNames, index);

        if (name == null)
            return;

        ImGui.BeginChild("havokBehaviorPropEditSection");

        var columnCount = 2;
        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            columnCount = 3;
        }

        ImGui.Columns(columnCount);

        DisplayNameRow(symbols, index, name);

        DisplayObjectNode(LOC.Get("HAVOK_VariableView_Info"), symbols.GetVariableInfo(index));
        DisplayObjectNode(LOC.Get("HAVOK_VariableView_Bounds"), symbols.GetVariableBounds(index));
        DisplayObjectNode(LOC.Get("HAVOK_VariableView_Initial_Value"), symbols.GetVariableInitialValue(index),
            LOC.Get("HAVOK_VariableView_Initial_Value_TT"));

        ImGui.Columns(1);

        DisplayBindings(symbols, index);

        ImGui.EndChild();
    }

    /// <summary>
    /// The variable's name, editable (an edit to the string data's name list)
    /// </summary>
    private void DisplayNameRow(HavokBehaviorSymbols symbols, int index, string name)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get("HAVOK_VariableView_Name"));
        GUI.Tooltip(LOC.Get("HAVOK_VariableView_Name_TT", index));

        ImGui.NextColumn();

        ImGui.SetNextItemWidth(-1);
        var newName = name;

        ImGui.InputText("##variableName", ref newName, 255);

        if (ImGui.IsItemDeactivatedAfterEdit() && newName != name)
        {
            var field = HavokBehaviorSymbols.GetField(symbols.StringData.GetType(), "m_variableNames");

            if (field != null)
            {
                var action = new HavokChangeField(View.Selection.SelectedFileWrapper, field, symbols.StringData, newName, index);
                View.ActionManager.ExecuteAction(action);
            }
        }

        ImGui.NextColumn();

        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            ImGui.NextColumn();
        }
    }

    /// <summary>
    /// An object (info, bounds, initial value) as an open tree node, edited like any other object
    /// </summary>
    private void DisplayObjectNode(string label, object obj, string tooltip = "")
    {
        if (obj == null)
            return;

        var open = ImGui.TreeNodeEx($"{label}##variableNode_{label}", ImGuiTreeNodeFlags.DefaultOpen);
        GUI.Tooltip(tooltip);

        ImGui.NextColumn();
        ImGui.NextColumn();

        if (CFG.Current.HavokEditor_Properties_Display_Type_Column)
        {
            ImGui.TextDisabled(obj.GetType().FullName);
            ImGui.NextColumn();
        }

        if (open)
        {
            View.PropertyView.HavokPropEditGeneric(obj, HavokMetaHelper.GetMeta(Project, obj.GetType()));
            ImGui.TreePop();
        }
    }

    /// <summary>
    /// Every binding that reads this variable, and the object it drives
    /// </summary>
    private void DisplayBindings(HavokBehaviorSymbols symbols, int index)
    {
        ImGui.Separator();

        var bindings = symbols.FindBindingsTo(index, characterProperty: false);

        ImGui.Text(LOC.Get("HAVOK_VariableView_Bindings", bindings.Count));
        GUI.Tooltip(LOC.Get("HAVOK_VariableView_Bindings_TT"));

        if (bindings.Count == 0)
            return;

        if (ImGui.BeginTable("variableBindingsTable", 2, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable))
        {
            ImGui.TableSetupColumn(LOC.Get("HAVOK_VariableView_Bindings_Object"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("HAVOK_VariableView_Bindings_Member"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();

            foreach (var (bindable, binding) in bindings)
            {
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.Text(HavokBehaviorSymbols.GetLabel(bindable));

                ImGui.TableSetColumnIndex(1);
                ImGui.Text(HavokBehaviorSymbols.GetMemberPath(binding));
            }

            ImGui.EndTable();
        }
    }
}
