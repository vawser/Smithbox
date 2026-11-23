using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.FxEditor.Model;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The effect tree's structure menus for DLSE effects (DES, DS2, DS2S; FFX Older Games Plan §21, F7a): states and
/// their exits, a state's actions and the entries of call lists, with actions added from the project's templates.
/// Every change is one <see cref="FxDlseStructureAction"/>, undone through a whole-effect snapshot.
/// </summary>
public partial class FxEffectTree
{
    private FFXDLSE.State PendingDlseDelete;
    private bool DlseDeleteToOpen;
    private const string DlseDeletePopup = "fxDlseStateDelete";

    // ---------------------------------------------------------------- menus

    /// <summary>The "States (n)" row's menu: add a state.</summary>
    private void DisplayDlseStatesMenu(FxEntry entry, FFXDLSE.State any, string idPrefix)
    {
        if (!ImGui.BeginPopupContextItem($"##fxDlseStates{idPrefix}"))
            return;
        if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_State_Add")}##fxDlseStateAddAll"))
            Deferred.Add(() => DlseEdit(entry, any, (fx, f) => FfxStructure.AddState(fx)));
        GUI.Tooltip(LOC.Get("FX_Dlse_State_Add_TT"));
        ImGui.EndPopup();
    }

    private void DisplayDlseStateMenu(FxEntry entry, FFXDLSE.State state, int index, int count, string idPrefix)
    {
        if (!ImGui.BeginPopupContextItem($"##fxDlseState_{idPrefix}{index}"))
            return;
        Editor.Selection.SelectItem(state);

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Preview")}##fxDlseStatePreview", "", Editor.Playback.PreviewState == index))
            Editor.Playback.SetPreviewState(index);
        GUI.Tooltip(LOC.Get("FX_EffectTree_State_Preview_TT"));
        ImGui.Separator();

        // a new action from the project's commonest instance of its id
        if (ImGui.BeginMenu($"{LOC.Get("FX_Dlse_Action_Add")}##fxDlseActionAdd"))
        {
            var templates = Data.DlseTemplates(entry.Ffx.Format);
            if (templates == null || templates.Uses.Count == 0)
                ImGui.TextDisabled(LOC.Get("FX_Dlse_Templates_None"));
            else
            {
                foreach (var (id, uses) in templates.Uses.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
                {
                    var name = Data.FfxSchema?.Actions.GetValueOrDefault(id)?.Name;
                    if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Action_Template", id, name ?? "", uses)}##fxDlseTpl{id}"))
                    {
                        int actionId = id;
                        Deferred.Add(() => DlseEdit(entry, state, (fx, f) =>
                        {
                            var a = Data.DlseTemplates(f.Format).New(actionId);
                            if (a != null)
                                state.Actions.Add(a);
                        }));
                    }
                }
            }
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("FX_Dlse_Action_Add_TT"));
        ImGui.Separator();

        if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_State_Add")}##fxDlseStateAdd"))
            Deferred.Add(() => DlseEdit(entry, state, (fx, f) => FfxStructure.AddState(fx)));
        GUI.Tooltip(LOC.Get("FX_Dlse_State_Add_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Duplicate")}##fxDlseStateDuplicate"))
            Deferred.Add(() => DlseEdit(entry, state, (fx, f) => FfxStructure.DuplicateState(fx, f.Format, fx.StateMap.States.IndexOf(state))));
        GUI.Tooltip(LOC.Get("FX_Dlse_State_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Delete")}##fxDlseStateDelete", "", false, count > 1))
        {
            PendingDlseDelete = state;
            DlseDeleteToOpen = true;
        }
        GUI.Tooltip(LOC.Get(count > 1 ? "FX_Dlse_State_Delete_TT" : "FX_EffectTree_State_Delete_Last_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Up")}##fxDlseStateUp", "", false, index > 0))
            Deferred.Add(() => DlseEdit(entry, state, (fx, f) => FfxStructure.SwapStates(fx, index, index - 1)));
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Down")}##fxDlseStateDown", "", false, index < count - 1))
            Deferred.Add(() => DlseEdit(entry, state, (fx, f) => FfxStructure.SwapStates(fx, index, index + 1)));
        GUI.Tooltip(LOC.Get("FX_Dlse_State_Move_TT"));

        // exits: drawn as text rows, so their edits are here
        if (state.Triggers.Count > 0)
        {
            ImGui.Separator();
            var model = Data.Adapt(entry, state) as IFxState;
            for (int x = 0; x < state.Triggers.Count; x++)
            {
                int xi = x;
                var text = model != null && x < model.Transitions.Count ? FxNames.Condition(model.Transitions[x]) : $"{x}";
                if (ImGui.BeginMenu($"{LOC.Get("FX_Dlse_Exit", x)}: {text}##fxDlseExit{x}"))
                {
                    if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Exit_Duplicate")}##fxDlseExitDup{x}"))
                        Deferred.Add(() => DlseEdit(entry, state, (fx, f) => FfxStructure.DuplicateExit(state, f.Format, xi)));
                    GUI.Tooltip(LOC.Get("FX_Dlse_Exit_Duplicate_TT"));
                    if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Exit_Delete")}##fxDlseExitDel{x}"))
                        Deferred.Add(() => DlseEdit(entry, state, (fx, f) => FfxStructure.DeleteExit(state, xi)));
                    GUI.Tooltip(LOC.Get("FX_Dlse_Exit_Delete_TT"));
                    ImGui.EndMenu();
                }
            }
        }

        ImGui.EndPopup();
    }

    /// <summary>A state's action, or an entry of a call list (a nested action or effect call).</summary>
    private void DisplayDlseActionMenu(FxEntry entry, object source, string id)
    {
        if (!ImGui.BeginPopupContextItem($"##fxDlseAction_{id}"))
            return;
        Editor.Selection.SelectItem(source);

        if (source is FFXDLSE.Action action)
        {
            var (_, list) = ActionList(entry.Ffx, action);
            int index = list?.IndexOf(action) ?? -1;
            if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Action_Duplicate")}##fxDlseActionDup"))
                Deferred.Add(() => DlseEdit(entry, action, (fx, f) => { var (_, l) = ActionList(f, action); FfxStructure.Duplicate(l, l.IndexOf(action), x => FFXDLSE.Clone(x, f.Format)); }));
            GUI.Tooltip(LOC.Get("FX_Dlse_Action_Duplicate_TT"));
            if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Action_Delete")}##fxDlseActionDel"))
                Deferred.Add(() => DlseEdit(entry, action, (fx, f) => { var (_, l) = ActionList(f, action); FfxStructure.Delete(l, l.IndexOf(action)); }));
            GUI.Tooltip(LOC.Get("FX_Dlse_Action_Delete_TT"));
            MoveItems(entry, action, index, list?.Count ?? 0, f => ActionList(f, action).List);
        }
        else
        {
            var param = (FFXDLSE.Param)source;
            var list = CallList(entry.Ffx, param);
            bool can = list != null;
            int index = list?.Params.IndexOf(param) ?? -1;
            if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Call_Duplicate")}##fxDlseCallDup", "", false, can))
                Deferred.Add(() => DlseEdit(entry, param, (fx, f) => { var l = CallList(f, param).Params; FfxStructure.Duplicate(l, l.IndexOf(param), x => FFXDLSE.Clone(x, f.Format)); }));
            GUI.Tooltip(LOC.Get(can ? "FX_Dlse_Call_Duplicate_TT" : "FX_Dlse_Call_Fixed_TT"));
            if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Call_Delete")}##fxDlseCallDel", "", false, can && list.Params.Count > 1))
                Deferred.Add(() => DlseEdit(entry, param, (fx, f) => { var l = CallList(f, param).Params; FfxStructure.Delete(l, l.IndexOf(param)); }));
            GUI.Tooltip(LOC.Get(can ? "FX_Dlse_Call_Delete_TT" : "FX_Dlse_Call_Fixed_TT"));
            if (can)
                MoveItems(entry, param, index, list.Params.Count, f => CallList(f, param).Params);
        }

        ImGui.EndPopup();
    }

    private void MoveItems<T>(FxEntry entry, T item, int index, int count, Func<FFXDLSE, List<T>> listOf) where T : class
    {
        if (index < 0)
            return;
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Up")}##fxDlseUp", "", false, index > 0))
            Deferred.Add(() => DlseEdit(entry, item, (fx, f) => { var l = listOf(f); int i = l.IndexOf(item); FfxStructure.Move(l, i, i - 1); }));
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Down")}##fxDlseDown", "", false, index < count - 1))
            Deferred.Add(() => DlseEdit(entry, item, (fx, f) => { var l = listOf(f); int i = l.IndexOf(item); FfxStructure.Move(l, i, i + 1); }));
        GUI.Tooltip(LOC.Get("FX_Dlse_Move_TT"));
    }

    /// <summary>Delete State, confirmed (as FXR3's): exits into it end the effect, later states are renumbered.</summary>
    private void DisplayDlsePopups(FxEntry entry)
    {
        if (DlseDeleteToOpen)
        {
            ImGui.OpenPopup($"{LOC.Get("FX_EffectTree_State_Delete")}###{DlseDeletePopup}");
            DlseDeleteToOpen = false;
        }
        if (!ImGui.BeginPopupModal($"{LOC.Get("FX_EffectTree_State_Delete")}###{DlseDeletePopup}", ImGuiWindowFlags.AlwaysAutoResize))
            return;
        var fx = PendingDlseDelete == null || entry?.Ffx == null ? null : OwnerOf(entry.Ffx, PendingDlseDelete);
        int index = fx?.StateMap.States.IndexOf(PendingDlseDelete) ?? -1;
        if (index < 0)
        {
            ImGui.CloseCurrentPopup();
        }
        else
        {
            int into = FfxStructure.ExitsInto(fx, index);
            int count = fx.StateMap.States.Count;
            GUI.WrappedText(LOC.Get("FX_EffectTree_State_Delete_Confirm", index));
            if (into > 0)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_Dlse_State_Delete_Into", into, index));
            if (index < count - 1)
                GUI.WrappedText(LOC.Get("FX_Dlse_State_Delete_Renumber", index + 1, count - 1));
            if (index == 0)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectTree_State_Delete_Start"));

            if (ImGui.Button($"{LOC.Get("FX_EffectTree_State_Delete")}##fxDlseStateDeleteConfirm"))
            {
                var state = PendingDlseDelete;
                Deferred.Add(() => DlseEdit(entry, state, (e, f) => FfxStructure.DeleteState(e, e.StateMap.States.IndexOf(state))));
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(LOC.Get("FX_EffectTree_Cancel")))
                ImGui.CloseCurrentPopup();
        }
        ImGui.EndPopup();
    }

    // ---------------------------------------------------------------- edits

    /// <summary>
    /// One structure edit of the effect (a DES wrapper's variant) that holds <paramref name="anchor"/>, as an undoable
    /// action. The edit runs on the entry's loaded file, where the anchor is.
    /// </summary>
    private void DlseEdit(FxEntry entry, object anchor, Action<FFXDLSE.FXEffect, FFXDLSE> edit)
    {
        if (entry?.Ffx == null)
            return;
        var fx = OwnerOf(entry.Ffx, anchor);
        if (fx == null)
            return;
        Execute(new FxDlseStructureAction(Editor, entry, f => edit(fx, f)));
    }


    /// <summary>The effect (the file's, or a DES wrapper variant's) that holds an object of it.</summary>
    private static FFXDLSE.FXEffect OwnerOf(FFXDLSE file, object item)
    {
        var effects = file.Effect.Variants?.Count > 0 ? file.Effect.Variants.Select(v => v.Effect).ToList() : [file.Effect];
        foreach (var fx in effects)
        {
            if (item is FFXDLSE.State s && fx.StateMap.States.Contains(s))
                return fx;
            if (item is FFXDLSE.Action a && fx.StateMap.States.Any(st => st.Actions.Contains(a)))
                return fx;
            if (item is FFXDLSE.Param p && FfxStructure.AllActions(fx).Any(x => x.ParamList?.Params.Contains(p) == true))
                return fx;
        }
        return null;
    }

    /// <summary>The state whose action list holds a state action.</summary>
    private static (FFXDLSE.State State, List<FFXDLSE.Action> List) ActionList(FFXDLSE file, FFXDLSE.Action action)
    {
        var fx = OwnerOf(file, action);
        var st = fx?.StateMap.States.FirstOrDefault(s => s.Actions.Contains(action));
        return (st, st?.Actions);
    }

    /// <summary>The call list holding a nested call, or null when its list has an action's fixed slots.</summary>
    private static FFXDLSE.ParamList CallList(FFXDLSE file, FFXDLSE.Param param)
    {
        var fx = OwnerOf(file, param);
        var list = fx == null ? null : FfxStructure.AllActions(fx).Select(a => a.ParamList).FirstOrDefault(l => l?.Params.Contains(param) == true);
        return FfxStructure.IsCallList(list) ? list : null;
    }
}
