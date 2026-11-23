using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.FxEditor.Model;
using StudioCore.Keybinds;
using static SoulsFormats.FXR1.FXNode;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The effect tree's structure menus for FXR1 / FXR v2 effects (DS1, DS1R, BB; FFX Older Games Plan §21.8, F7b), as
/// FxEffectTree.Dlse.cs is for DLSE: a root's states and exits, a state's actions, call-list entries, templates,
/// copy / paste and the tree keys. An FXR1 exit's target is its state object, so states move without renumbering, and
/// a deleted state's incoming exits end the effect. Every change is one <see cref="FxFxr1StructureAction"/>.
/// </summary>
public partial class FxEffectTree
{
    private FXR1.FXState PendingFxr1Delete;
    private bool Fxr1DeleteToOpen;
    private const string Fxr1DeletePopup = "fxFxr1StateDelete";

    // copied state action or call-list entry: a copy in its own graph, pasted only into effects of the same game
    private object Fxr1Clip;
    private SoulsFormats.FfxGame Fxr1ClipGame;
    private string Fxr1ClipSource;

    // ---------------------------------------------------------------- menus

    private void DisplayFxr1StatesMenu(FxEntry entry, FXR1.FXState any, string idPrefix)
    {
        if (!ImGui.BeginPopupContextItem($"##fxFxr1States{idPrefix}"))
            return;
        if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_State_Add")}##fxFxr1StateAddAll"))
            Deferred.Add(() => Fxr1Edit(entry, f => { if (Fxr1Structure.RootOf(f, any) is { } r) Fxr1Structure.AddState(r); }));
        GUI.Tooltip(LOC.Get("FX_Dlse_State_Add_TT"));
        ImGui.EndPopup();
    }

    private void DisplayFxr1StateMenu(FxEntry entry, FXR1.FXState state, int index, int count, string idPrefix)
    {
        if (!ImGui.BeginPopupContextItem($"##fxFxr1State_{idPrefix}{index}"))
            return;
        Editor.Selection.SelectItem(state);

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Preview")}##fxFxr1StatePreview", "", Editor.Playback.PreviewState == index))
            Editor.Playback.SetPreviewState(index);
        GUI.Tooltip(LOC.Get("FX_EffectTree_State_Preview_TT"));
        ImGui.Separator();

        if (ImGui.BeginMenu($"{LOC.Get("FX_Dlse_Action_Add")}##fxFxr1ActionAdd"))
        {
            var templates = Data.Fxr1Templates();
            if (templates == null || templates.Uses.Count == 0)
                ImGui.TextDisabled(LOC.Get("FX_Dlse_Templates_None"));
            else
            {
                foreach (var (type, uses) in templates.Uses.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
                {
                    var name = Data.FfxSchema?.Actions.GetValueOrDefault(type)?.Name;
                    if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Action_Template", type, name ?? "", uses)}##fxFxr1Tpl{type}"))
                    {
                        int t = type;
                        Deferred.Add(() => Fxr1Edit(entry, f => { if (Data.Fxr1Templates().New(t) is { } a) state.Actions.Add(a); }));
                    }
                }
            }
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("FX_Dlse_Action_Add_TT"));

        bool pasteAction = Fxr1Clip is FXR1.FXAction && Fxr1ClipGame == Data.FfxGame;
        if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Paste_Into_State")}##fxFxr1PasteState", InputManager.GetHint(KeybindID.Paste), false, pasteAction))
            Deferred.Add(() => Fxr1Edit(entry, f => state.Actions.Add(Fxr1Paste<FXR1.FXAction>())));
        GUI.Tooltip(Fxr1PasteTooltip(Fxr1Clip is FXR1.FXAction));

        if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Exit_Add")}##fxFxr1ExitAdd"))
            Deferred.Add(() => Fxr1Edit(entry, f => { if (Data.Fxr1Templates()?.NewExit() is { } t) state.Transitions.Add(t); }));
        GUI.Tooltip(LOC.Get("FX_Dlse_Exit_Add_TT"));
        ImGui.Separator();

        if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_State_Add")}##fxFxr1StateAdd"))
            Deferred.Add(() => Fxr1Edit(entry, f => { if (Fxr1Structure.RootOf(f, state) is { } r) Fxr1Structure.AddState(r); }));
        GUI.Tooltip(LOC.Get("FX_Dlse_State_Add_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Duplicate")}##fxFxr1StateDuplicate"))
            Deferred.Add(() => Fxr1Edit(entry, f => { if (Fxr1Structure.RootOf(f, state) is { } r) Fxr1Structure.DuplicateState(f, r, r.States.IndexOf(state)); }));
        GUI.Tooltip(LOC.Get("FX_Dlse_State_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_State_Delete")}##fxFxr1StateDelete", "", false, count > 1))
        {
            PendingFxr1Delete = state;
            Fxr1DeleteToOpen = true;
        }
        GUI.Tooltip(LOC.Get(count > 1 ? "FX_Dlse_State_Delete_TT" : "FX_EffectTree_State_Delete_Last_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Up")}##fxFxr1StateUp", "", false, index > 0))
            Deferred.Add(() => Fxr1Edit(entry, f => { if (Fxr1Structure.RootOf(f, state) is { } r) Fxr1Structure.SwapStates(r, index, index - 1); }));
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Down")}##fxFxr1StateDown", "", false, index < count - 1))
            Deferred.Add(() => Fxr1Edit(entry, f => { if (Fxr1Structure.RootOf(f, state) is { } r) Fxr1Structure.SwapStates(r, index, index + 1); }));
        GUI.Tooltip(LOC.Get("FX_Dlse_State_Move_TT"));

        if (state.Transitions.Count > 0)
        {
            ImGui.Separator();
            var model = Data.Adapt(entry, state) as IFxState;
            for (int x = 0; x < state.Transitions.Count; x++)
            {
                var transition = state.Transitions[x];
                var text = model != null && x < model.Transitions.Count ? FxNames.Condition(model.Transitions[x]) : $"{x}";
                if (ImGui.BeginMenu($"{LOC.Get("FX_Dlse_Exit", x)}: {text}##fxFxr1Exit{x}"))
                {
                    if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Exit_Duplicate")}##fxFxr1ExitDup{x}"))
                        Deferred.Add(() => Fxr1Edit(entry, f => state.Transitions.Insert(state.Transitions.IndexOf(transition) + 1, Fxr1Structure.Clone(f, transition))));
                    GUI.Tooltip(LOC.Get("FX_Dlse_Exit_Duplicate_TT"));
                    if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Exit_Delete")}##fxFxr1ExitDel{x}"))
                        Deferred.Add(() => Fxr1Edit(entry, f => state.Transitions.Remove(transition)));
                    GUI.Tooltip(LOC.Get("FX_Dlse_Exit_Delete_TT"));
                    ImGui.EndMenu();
                }
            }
        }

        ImGui.EndPopup();
    }

    /// <summary>A state's action, or an entry of a call list (an action or effect call node).</summary>
    private void DisplayFxr1ActionMenu(FxEntry entry, object source, string id)
    {
        if (!ImGui.BeginPopupContextItem($"##fxFxr1Action_{id}"))
            return;
        Editor.Selection.SelectItem(source);

        if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Copy")}##fxFxr1Copy", InputManager.GetHint(KeybindID.Copy)))
            Fxr1Copy(entry, source);
        GUI.Tooltip(LOC.Get("FX_Dlse_Copy_TT"));
        bool canPaste = Fxr1CanPasteAfter(entry, source);
        if (ImGui.MenuItem($"{LOC.Get("FX_Dlse_Paste_After")}##fxFxr1Paste", InputManager.GetHint(KeybindID.Paste), false, canPaste))
            Deferred.Add(() => Fxr1PasteAfter(entry, source));
        GUI.Tooltip(Fxr1PasteTooltip(source is FXR1.FXAction ? Fxr1Clip is FXR1.FXAction : Fxr1Clip is FXR1.FXNode));
        ImGui.Separator();

        var (list, canEdit, minCount) = Fxr1ListOf(entry.Fxr1, source);
        if (ImGui.MenuItem($"{LOC.Get(source is FXR1.FXAction ? "FX_Dlse_Action_Duplicate" : "FX_Dlse_Call_Duplicate")}##fxFxr1Dup", "", false, canEdit))
            Deferred.Add(() => Fxr1Edit(entry, f => { var (l, _, _) = Fxr1ListOf(f, source); l.Insert(l.IndexOf(source) + 1, Fxr1Structure.Clone(f, source)); }));
        GUI.Tooltip(LOC.Get(!canEdit ? "FX_Dlse_Call_Fixed_TT" : source is FXR1.FXAction ? "FX_Dlse_Action_Duplicate_TT" : "FX_Dlse_Call_Duplicate_TT"));
        if (ImGui.MenuItem($"{LOC.Get(source is FXR1.FXAction ? "FX_Dlse_Action_Delete" : "FX_Dlse_Call_Delete")}##fxFxr1Del", "", false, canEdit && list.Count > minCount))
            Deferred.Add(() => Fxr1Edit(entry, f => Fxr1ListOf(f, source).List.Remove(source)));
        GUI.Tooltip(LOC.Get(!canEdit ? "FX_Dlse_Call_Fixed_TT" : source is FXR1.FXAction ? "FX_Dlse_Action_Delete_TT" : "FX_Dlse_Call_Delete_TT"));
        if (canEdit)
        {
            int index = list.IndexOf(source);
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Up")}##fxFxr1Up", "", false, index > 0))
                Deferred.Add(() => Fxr1Edit(entry, f => { var l = Fxr1ListOf(f, source).List; int i = l.IndexOf(source); MoveIn(l, i, i - 1); }));
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectTree_Move_Down")}##fxFxr1Down", "", false, index >= 0 && index < list.Count - 1))
                Deferred.Add(() => Fxr1Edit(entry, f => { var l = Fxr1ListOf(f, source).List; int i = l.IndexOf(source); MoveIn(l, i, i + 1); }));
            GUI.Tooltip(LOC.Get("FX_Dlse_Move_TT"));
        }

        ImGui.EndPopup();
    }

    private void DisplayFxr1Popups(FxEntry entry)
    {
        if (Fxr1DeleteToOpen)
        {
            ImGui.OpenPopup($"{LOC.Get("FX_EffectTree_State_Delete")}###{Fxr1DeletePopup}");
            Fxr1DeleteToOpen = false;
        }
        if (!ImGui.BeginPopupModal($"{LOC.Get("FX_EffectTree_State_Delete")}###{Fxr1DeletePopup}", ImGuiWindowFlags.AlwaysAutoResize))
            return;
        var root = PendingFxr1Delete == null || entry?.Fxr1 == null ? null : Fxr1Structure.RootOf(entry.Fxr1, PendingFxr1Delete);
        int index = root?.States.IndexOf(PendingFxr1Delete) ?? -1;
        if (index < 0)
        {
            ImGui.CloseCurrentPopup();
        }
        else
        {
            int into = Fxr1Structure.ExitsInto(root, PendingFxr1Delete);
            GUI.WrappedText(LOC.Get("FX_EffectTree_State_Delete_Confirm", index));
            if (into > 0)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_Dlse_State_Delete_Into", into, index));
            if (index == 0)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("FX_EffectTree_State_Delete_Start"));
            if (ImGui.Button($"{LOC.Get("FX_EffectTree_State_Delete")}##fxFxr1StateDeleteConfirm"))
            {
                var state = PendingFxr1Delete;
                Deferred.Add(() => Fxr1Edit(entry, f => { if (Fxr1Structure.RootOf(f, state) is { } r) Fxr1Structure.DeleteState(r, r.States.IndexOf(state)); }));
                ImGui.CloseCurrentPopup();
            }
            ImGui.SameLine();
            if (ImGui.Button(LOC.Get("FX_EffectTree_Cancel")))
                ImGui.CloseCurrentPopup();
        }
        ImGui.EndPopup();
    }

    // ---------------------------------------------------------------- clipboard and keys

    private void Fxr1Copy(FxEntry entry, object source)
    {
        // the clipboard keeps the source effect written out and the item's place in its graph: each paste reads a
        // fresh copy from them, sharing nothing with any effect (as the templates do)
        var walk = Fxr1Structure.Walk(entry.Fxr1.RootFXNode);
        int at = walk.FindIndex(o => ReferenceEquals(o, source));
        if (at < 0)
            return;
        Fxr1ClipBytes = entry.Fxr1.Write();
        Fxr1ClipIndex = at;
        Fxr1Clip = source;
        Fxr1ClipGame = Data.FfxGame;
        Fxr1ClipSource = entry.Stem;
    }

    private byte[] Fxr1ClipBytes;
    private int Fxr1ClipIndex;

    /// <summary>A fresh copy of the clipboard item, read from the copied effect's bytes.</summary>
    private T Fxr1Paste<T>() where T : class =>
        Fxr1ClipBytes == null ? null : Fxr1Structure.Walk(FXR1.Read(Fxr1ClipBytes).RootFXNode)[Fxr1ClipIndex] as T;

    private bool Fxr1CanPasteAfter(FxEntry entry, object target)
    {
        if (Fxr1Clip == null || Fxr1ClipGame != Data.FfxGame || entry?.Fxr1 == null)
            return false;
        var (_, canEdit, _) = Fxr1ListOf(entry.Fxr1, target);
        return canEdit && (target is FXR1.FXAction ? Fxr1Clip is FXR1.FXAction : Fxr1Clip is EffectCallNode or ActionCallNode);
    }

    private void Fxr1PasteAfter(FxEntry entry, object target)
    {
        if (!Fxr1CanPasteAfter(entry, target))
            return;
        Fxr1Edit(entry, f =>
        {
            var l = Fxr1ListOf(f, target).List;
            l.Insert(l.IndexOf(target) + 1, Fxr1Paste<object>());
        });
    }

    private string Fxr1PasteTooltip(bool kindCopied)
    {
        if (!kindCopied)
            return LOC.Get("FX_Dlse_Paste_Empty");
        if (Fxr1ClipGame != Data.FfxGame)
            return LOC.Get("FX_Dlse_Paste_Game", Fxr1ClipGame);
        return LOC.Get("FX_Dlse_Paste_TT", Fxr1ClipSource);
    }

    private void Fxr1Shortcuts(FxEntry entry)
    {
        var item = Editor.Selection.Item;
        if (item is FXR1.FXState state)
        {
            var root = Fxr1Structure.RootOf(entry.Fxr1, state);
            if (root == null)
                return;
            int i = root.States.IndexOf(state), n = root.States.Count;
            if (InputManager.IsPressed(KeybindID.Duplicate))
                Deferred.Add(() => Fxr1Edit(entry, f => { if (Fxr1Structure.RootOf(f, state) is { } r) Fxr1Structure.DuplicateState(f, r, r.States.IndexOf(state)); }));
            else if (InputManager.IsPressed(KeybindID.Delete) && n > 1)
            {
                PendingFxr1Delete = state;
                Fxr1DeleteToOpen = true;
            }
            else if (InputManager.IsPressed(KeybindID.Reorder_Up) && i > 0)
                Deferred.Add(() => Fxr1Edit(entry, f => Fxr1Structure.SwapStates(root, i, i - 1)));
            else if (InputManager.IsPressed(KeybindID.Reorder_Down) && i < n - 1)
                Deferred.Add(() => Fxr1Edit(entry, f => Fxr1Structure.SwapStates(root, i, i + 1)));
            else if (InputManager.IsPressed(KeybindID.Paste) && Fxr1Clip is FXR1.FXAction && Fxr1ClipGame == Data.FfxGame)
                Deferred.Add(() => Fxr1Edit(entry, f => state.Actions.Add(Fxr1Paste<FXR1.FXAction>())));
            return;
        }

        if (item is not (FXR1.FXAction or EffectCallNode or ActionCallNode))
            return;
        if (InputManager.IsPressed(KeybindID.Copy)) { Fxr1Copy(entry, item); return; }
        if (InputManager.IsPressed(KeybindID.Paste)) { if (Fxr1CanPasteAfter(entry, item)) Deferred.Add(() => Fxr1PasteAfter(entry, item)); return; }

        var (list, canEdit, minCount) = Fxr1ListOf(entry.Fxr1, item);
        if (!canEdit)
            return;
        int at = list.IndexOf(item);
        if (InputManager.IsPressed(KeybindID.Duplicate))
            Deferred.Add(() => Fxr1Edit(entry, f => { var l = Fxr1ListOf(f, item).List; l.Insert(l.IndexOf(item) + 1, Fxr1Structure.Clone(f, item)); }));
        else if (InputManager.IsPressed(KeybindID.Delete) && list.Count > minCount)
            Deferred.Add(() => Fxr1Edit(entry, f => Fxr1ListOf(f, item).List.Remove(item)));
        else if (InputManager.IsPressed(KeybindID.Reorder_Up) && at > 0)
            Deferred.Add(() => Fxr1Edit(entry, f => { var l = Fxr1ListOf(f, item).List; int i = l.IndexOf(item); MoveIn(l, i, i - 1); }));
        else if (InputManager.IsPressed(KeybindID.Reorder_Down) && at >= 0 && at < list.Count - 1)
            Deferred.Add(() => Fxr1Edit(entry, f => { var l = Fxr1ListOf(f, item).List; int i = l.IndexOf(item); MoveIn(l, i, i + 1); }));
    }

    // ---------------------------------------------------------------- edits

    /// <summary>Moves an item of an untyped list one place (a state's actions or a call list's nodes).</summary>
    private static void MoveIn(System.Collections.IList list, int from, int to)
    {
        if (from < 0 || to < 0 || from >= list.Count || to >= list.Count || from == to)
            return;
        var item = list[from];
        list.RemoveAt(from);
        list.Insert(to, item);
    }

    private void Fxr1Edit(FxEntry entry, Action<FXR1> edit)
    {
        if (entry?.Fxr1 == null)
            return;
        Execute(new FxFxr1StructureAction(Editor, entry, edit));
    }

    /// <summary>
    /// The list holding an item, as untyped: a state action's state actions (any count), or a call node's call list
    /// (at least one entry). <c>canEdit</c> is false for a call in a container with fixed fields.
    /// </summary>
    private static (System.Collections.IList List, bool CanEdit, int MinCount) Fxr1ListOf(FXR1 file, object item)
    {
        if (item is FXR1.FXAction a)
        {
            var (_, s) = Fxr1Structure.OwnerOf(file, a);
            return (s?.Actions, s != null, 0);
        }
        if (item is FXR1.FXNode n && Fxr1Structure.CallListOf(file, n) is { } c)
            return (c.FXNodes, true, 1);
        return (null, false, 0);
    }
}
