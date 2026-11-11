using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The state view's edits: each builds one of Framework/EsdEdits' ops (gated by EsdSurvey editcheck) for the selected
/// block and runs it through the undo stack. Menu items for the block context menu and the Edit menu live here too.
/// </summary>
public class EsdEditCommands
{
    private readonly EsdEditorView View;

    public EsdEditCommands(EsdEditorView view)
    {
        View = view;
    }

    private EsdSelection Selection => View.Selection;
    private EsdContainer Container => Selection.SelectedFile?.Container;
    private Dictionary<long, ESD.State> Machine => Selection.Machine;

    /// <summary>The row the selection is on, from the state view's current rows</summary>
    public EsdRow SelectedRow => View.StateView.FindSelectedRow();

    // Edits wait for the end of the frame: one made from a popup mid-draw would change the lists the rest of the frame
    // is drawing
    private readonly List<EsdEditAction> Pending = new();

    private void Run(IEsdOp op, string what)
    {
        if (Container == null || op == null)
            return;
        Pending.Add(new EsdEditAction(Container, op, what));
    }

    /// <summary>Runs the edits queued this frame, once every window is drawn</summary>
    public void Flush()
    {
        if (Pending.Count == 0)
            return;
        var edits = Pending.ToList();
        Pending.Clear();
        foreach (var e in edits)
            View.ActionManager.ExecuteAction(e);
    }

    // --- values ---

    public void SetEvaluator(ESD.Condition c, byte[] bytes)
    {
        if (bytes == null || bytes.AsSpan().SequenceEqual(c.Evaluator))
            return;
        Run(new EsdSetOp<byte[]>(() => c.Evaluator, v => c.Evaluator = v, bytes), "test");
    }

    public void SetArgument(ESD.CommandCall cmd, int index, byte[] bytes)
    {
        if (bytes == null || index < 0 || index >= cmd.Arguments.Count || bytes.AsSpan().SequenceEqual(cmd.Arguments[index]))
            return;
        Run(new EsdSetOp<byte[]>(() => cmd.Arguments[index], v => cmd.Arguments[index] = v, bytes), "arg");
    }

    public void SetTarget(ESD.Condition c, long? target)
    {
        if (c.TargetState == target)
            return;
        Run(new EsdSetOp<long?>(() => c.TargetState, v => c.TargetState = v, target), "target");
    }

    // --- commands ---

    private static List<ESD.CommandCall> ListOf(EsdRow row) => row.List switch
    {
        EsdCommandList.Entry => row.State.EntryCommands,
        EsdCommandList.While => row.State.WhileCommands,
        EsdCommandList.Exit => row.State.ExitCommands,
        _ => row.Condition.PassCommands
    };

    /// <summary>Inserts a command into a list of a state or condition, and selects it</summary>
    public void InsertCommand(ESD.State state, long stateId, ESD.Condition cond, EsdCommandList list, int index, ESD.CommandCall cmd)
    {
        var target = list switch
        {
            EsdCommandList.Entry => state.EntryCommands,
            EsdCommandList.While => state.WhileCommands,
            EsdCommandList.Exit => state.ExitCommands,
            _ => cond?.PassCommands
        };
        if (target == null)
            return;

        Run(new EsdInsertOp<ESD.CommandCall>(target, index < 0 ? target.Count : index, cmd), "insert command");
        Selection.SelectItem(cmd, stateId);
    }

    /// <summary>A new command with the catalogue's default arg for each documented arg</summary>
    public static ESD.CommandCall NewCommand(EsdCallDef def)
    {
        return new ESD.CommandCall(def.Bank, def.Id, def.Args.Where(a => !a.Optional).Select(a => EsdEdits.DefaultArg(a.Type)).ToArray());
    }

    public void DeleteCommand(EsdRow row)
    {
        var list = ListOf(row);
        if (row.Index < 0 || row.Index >= list.Count || !ReferenceEquals(list[row.Index], row.Command))
            return;
        Run(new EsdRemoveOp<ESD.CommandCall>(list, row.Index), "delete command");
        Selection.SelectItem(row.State, row.StateId);
    }

    public void DuplicateCommand(EsdRow row)
    {
        var list = ListOf(row);
        var copy = EsdEdits.CloneCommand(row.Command);
        Run(new EsdInsertOp<ESD.CommandCall>(list, row.Index + 1, copy), "duplicate command");
        Selection.SelectItem(copy, row.StateId);
    }

    public void MoveCommand(EsdRow row, int dir)
    {
        var list = ListOf(row);
        var to = row.Index + dir;
        if (to < 0 || to >= list.Count)
            return;
        Run(new EsdMoveOp<ESD.CommandCall>(list, row.Index, to), "move command");
        Selection.ScrollToItem = true;
    }

    // --- transitions ---

    /// <summary>Adds a transition that is never taken, to the state itself: a safe start to set the test and target of</summary>
    public void AddTransition(List<ESD.Condition> list, long stateId, int index = -1)
    {
        var c = new ESD.Condition(stateId, EsdEdits.Never);
        Run(new EsdInsertOp<ESD.Condition>(list, index < 0 ? list.Count : index, c), "add transition");
        Selection.SelectItem(c, stateId);
    }

    public void DeleteCondition(EsdRow row)
    {
        if (row.ParentList == null || !ReferenceEquals(row.ParentList[row.Index], row.Condition))
            return;
        Run(new EsdRemoveOp<ESD.Condition>(row.ParentList, row.Index), "delete transition");
        Selection.SelectItem(row.State, row.StateId);
    }

    public void DuplicateCondition(EsdRow row)
    {
        var copy = EsdEdits.CloneCondition(row.Condition);
        Run(new EsdInsertOp<ESD.Condition>(row.ParentList, row.Index + 1, copy), "duplicate transition");
        Selection.SelectItem(copy, row.StateId);
    }

    public void MoveCondition(EsdRow row, int dir)
    {
        var to = row.Index + dir;
        if (row.ParentList == null || to < 0 || to >= row.ParentList.Count)
            return;
        Run(new EsdMoveOp<ESD.Condition>(row.ParentList, row.Index, to), "move transition");
        Selection.ScrollToItem = true;
    }

    /// <summary>Gives this place its own copy of a condition other places share, so editing it changes only here</summary>
    public void MakeUnique(EsdRow row)
    {
        if (row.SharedCount < 2 || row.ParentList == null)
            return;
        var copy = EsdEdits.CloneCondition(row.Condition);
        Run(new EsdReplaceOp<ESD.Condition>(row.ParentList, row.Index, copy), "make unique");
        Selection.SelectItem(copy, row.StateId);
    }

    // --- states ---

    public void AddState(long afterId)
    {
        if (Machine == null)
            return;
        var id = EsdEdits.NextStateId(Machine);
        var state = new ESD.State();
        Run(EsdStateOp.AddState(Machine, id, state, afterId), "add state");
        Selection.SelectItem(state, id);
    }

    public void DuplicateState(long id)
    {
        if (Machine == null || !Machine.TryGetValue(id, out var state))
            return;
        var newId = EsdEdits.NextStateId(Machine);
        var copy = EsdEdits.CloneState(state);
        Run(EsdStateOp.AddState(Machine, newId, copy, id), "duplicate state");
        Selection.SelectItem(copy, newId);
    }

    /// <summary>Why a state can't be deleted (null if it can): the start state, or one transitions still lead to</summary>
    public string DeleteStateRefusal(long id)
    {
        if (Machine == null || !Machine.ContainsKey(id))
            return "";
        if (id == 0)
            return LOC.Get("ESD_Edit_Delete_Start_Refused");
        var n = EsdChecks.TransitionsTo(Machine, id);
        return n > 0 ? LOC.Get("ESD_Edit_Delete_Targeted_Refused", id, n) : null;
    }

    public void DeleteState(long id)
    {
        var refusal = DeleteStateRefusal(id);
        if (refusal != null)
        {
            if (refusal.Length > 0)
                Smithbox.Log(this, refusal, Microsoft.Extensions.Logging.LogLevel.Warning);
            return;
        }
        Run(EsdStateOp.RemoveState(Machine, id), "delete state");
        Selection.ClearItem();
    }

    // --- keys and menus ---

    public void Delete()
    {
        var row = SelectedRow;
        switch (row?.Kind)
        {
            case EsdRowKind.State: DeleteState(row.StateId); break;
            case EsdRowKind.Command: DeleteCommand(row); break;
            case EsdRowKind.Condition: DeleteCondition(row); break;
        }
    }

    public void Duplicate()
    {
        var row = SelectedRow;
        switch (row?.Kind)
        {
            case EsdRowKind.State: DuplicateState(row.StateId); break;
            case EsdRowKind.Command: DuplicateCommand(row); break;
            case EsdRowKind.Condition: DuplicateCondition(row); break;
        }
    }

    public void Move(int dir)
    {
        var row = SelectedRow;
        switch (row?.Kind)
        {
            case EsdRowKind.Command: MoveCommand(row, dir); break;
            case EsdRowKind.Condition: MoveCondition(row, dir); break;
        }
    }

    /// <summary>The right-click menu of a block</summary>
    public void ContextMenu(EsdRow row)
    {
        switch (row.Kind)
        {
            case EsdRowKind.State:
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Add_State")))
                    AddState(row.StateId);
                GUI.Tooltip(LOC.Get("ESD_Edit_Add_State_TT"));
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Duplicate_State"), InputManager.GetHint(KeybindID.Duplicate)))
                    DuplicateState(row.StateId);
                var refusal = DeleteStateRefusal(row.StateId);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Delete_State"), InputManager.GetHint(KeybindID.Delete), false, refusal == null))
                    DeleteState(row.StateId);
                if (!string.IsNullOrEmpty(refusal))
                    GUI.Tooltip(refusal);
                ImGui.Separator();
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Add_Transition")))
                    AddTransition(row.State.Conditions, row.StateId);
                GUI.Tooltip(LOC.Get("ESD_Edit_Add_Transition_TT"));
                CommandMenu(LOC.Get("ESD_Edit_Add_Entry"), row.State, row.StateId, null, EsdCommandList.Entry, -1);
                CommandMenu(LOC.Get("ESD_Edit_Add_While"), row.State, row.StateId, null, EsdCommandList.While, -1);
                CommandMenu(LOC.Get("ESD_Edit_Add_Exit"), row.State, row.StateId, null, EsdCommandList.Exit, -1);
                break;

            case EsdRowKind.Command:
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Duplicate"), InputManager.GetHint(KeybindID.Duplicate)))
                    DuplicateCommand(row);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Delete"), InputManager.GetHint(KeybindID.Delete)))
                    DeleteCommand(row);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Move_Up"), InputManager.GetHint(KeybindID.Reorder_Up), false, row.Index > 0))
                    MoveCommand(row, -1);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Move_Down"), InputManager.GetHint(KeybindID.Reorder_Down), false, row.Index < ListOf(row).Count - 1))
                    MoveCommand(row, 1);
                ImGui.Separator();
                CommandMenu(LOC.Get("ESD_Edit_Insert_After"), row.State, row.StateId, row.Condition, row.List, row.Index + 1);
                break;

            case EsdRowKind.Condition:
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Duplicate"), InputManager.GetHint(KeybindID.Duplicate)))
                    DuplicateCondition(row);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Delete"), InputManager.GetHint(KeybindID.Delete)))
                    DeleteCondition(row);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Move_Up"), InputManager.GetHint(KeybindID.Reorder_Up), false, row.Index > 0))
                    MoveCondition(row, -1);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Move_Down"), InputManager.GetHint(KeybindID.Reorder_Down), false, row.ParentList != null && row.Index < row.ParentList.Count - 1))
                    MoveCondition(row, 1);
                if (row.SharedCount > 1)
                {
                    if (ImGui.MenuItem(LOC.Get("ESD_Edit_Make_Unique")))
                        MakeUnique(row);
                    GUI.Tooltip(LOC.Get("ESD_Edit_Make_Unique_TT", row.SharedCount));
                }
                ImGui.Separator();
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Set_Always")))
                    SetEvaluator(row.Condition, EsdEdits.Always);
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Set_Never")))
                    SetEvaluator(row.Condition, EsdEdits.Never);
                GUI.Tooltip(LOC.Get("ESD_Edit_Set_Never_TT"));
                if (row.Condition.TargetState != null && ImGui.MenuItem(LOC.Get("ESD_Edit_Clear_Target")))
                    SetTarget(row.Condition, null);
                ImGui.Separator();
                if (ImGui.MenuItem(LOC.Get("ESD_Edit_Add_Subcondition")))
                    AddTransition(row.Condition.Subconditions, row.StateId);
                GUI.Tooltip(LOC.Get("ESD_Edit_Add_Subcondition_TT"));
                CommandMenu(LOC.Get("ESD_Edit_Add_Pass"), row.State, row.StateId, row.Condition, EsdCommandList.Pass, -1);
                break;
        }
    }

    private string CommandFilter = "";

    /// <summary>A submenu of the commands to insert: the catalogue's for this kind of ESD, a call to a machine of the file, return</summary>
    private void CommandMenu(string label, ESD.State state, long stateId, ESD.Condition cond, EsdCommandList list, int index)
    {
        if (!ImGui.BeginMenu(label))
            return;

        ImGui.SetNextItemWidth(220f * DPI.UIScale());
        ImGui.InputTextWithHint("##esdCmdFilter", LOC.Get("ESD_Edit_Filter_Hint"), ref CommandFilter, 128);

        var file = Selection.SelectedFile;
        var cat = file != null ? View.Project.Handler.EsdData?.Catalog?.For(file.Kind) : null;

        if (ImGui.BeginMenu(LOC.Get("ESD_Edit_Call_Machine")))
        {
            foreach (var mid in file?.Esd?.StateGroups.Keys.OrderBy(k => k) ?? Enumerable.Empty<long>())
            {
                if (ImGui.MenuItem($"{EsdFormat.MachineLabel(file, mid)}##esdCallTo{mid}"))
                    InsertCommand(state, stateId, cond, list, index, new ESD.CommandCall(6, (int)mid));
            }
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("ESD_Edit_Call_Machine_TT"));

        if (ImGui.MenuItem(LOC.Get("ESD_Command_Return")))
            InsertCommand(state, stateId, cond, list, index, new ESD.CommandCall(7, -1, EsdEdits.DefaultArg("int")));
        GUI.Tooltip(LOC.Get("ESD_States_Return_TT"));

        ImGui.Separator();

        if (cat == null)
        {
            ImGui.TextDisabled(LOC.Get("ESD_States_No_Catalogue"));
        }
        else
        {
            ImGui.BeginChild("##esdCmdList", new System.Numerics.Vector2(320f * DPI.UIScale(), 300f * DPI.UIScale()));
            foreach (var def in cat.Commands.Values.Where(d => d.Name != null).OrderBy(d => d.Name))
            {
                if (!string.IsNullOrEmpty(CommandFilter) && !def.Name.Contains(CommandFilter, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (ImGui.Selectable($"{def.Name}##esdNewCmd{def.Bank}_{def.Id}"))
                {
                    InsertCommand(state, stateId, cond, list, index, NewCommand(def));
                    ImGui.CloseCurrentPopup();
                }
                if (ImGui.IsItemHovered())
                    GUI.Tooltip($"{def.Bank}:{def.Id}  ({string.Join(", ", def.Args.Select(a => $"{a.Name}: {a.Type}"))})" + (def.Comment != null ? $"\n{def.Comment}" : "")
                        + (def.Seen ? "" : $"\n{LOC.Get("ESD_Edit_Unseen_TT")}"));
            }
            ImGui.EndChild();
        }

        ImGui.EndMenu();
    }
}
