using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Event-level edits (P5): new, duplicate, delete and change ID, all through the undo stack. New and Change ID ask for
/// the ID in a modal; Change ID can renumber the Initialize Event calls in the same file too.
/// </summary>
public class EmevdEventCommands
{
    private readonly EmevdEditorView View;

    private bool OpenNew;
    private bool OpenChangeId;
    private string IdText = "";
    private EMEVD.Event.RestBehaviorType NewRest = EMEVD.Event.RestBehaviorType.Default;
    private bool UpdateCalls = true;

    public EmevdEventCommands(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdFileWrapper File => View.Selection.SelectedFile is { IsLoaded: true } f ? f : null;
    private EMEVD.Event Event => View.Selection.SelectedEvent;
    private EmevdCatalog Catalog => View.Project.Handler.EmevdData?.Catalog;

    public bool HasFile => File != null;
    public bool HasEvent => File != null && Event != null;

    /// <summary>The event menu items, shared by the Events list's context menu and the Edit menu</summary>
    public void MenuItems()
    {
        if (ImGui.MenuItem($"{LOC.Get("EMEVD_Event_New")}##emevdEventNew", "", false, HasFile))
            New();
        GUI.Tooltip(LOC.Get("EMEVD_Event_New_TT"));

        if (ImGui.MenuItem($"{LOC.Get("EMEVD_Event_Duplicate")}##emevdEventDuplicate", "", false, HasEvent))
            Duplicate();
        GUI.Tooltip(LOC.Get("EMEVD_Event_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("EMEVD_Event_Change_Id")}##emevdEventChangeId", "", false, HasEvent))
            ChangeId();
        GUI.Tooltip(LOC.Get("EMEVD_Event_Change_Id_TT"));

        ImGui.Separator();

        if (ImGui.MenuItem($"{LOC.Get("EMEVD_Event_Delete")}##emevdEventDelete", "", false, HasEvent))
            Delete();
        GUI.Tooltip(LOC.Get("EMEVD_Event_Delete_TT"));
    }

    public void New()
    {
        if (!HasFile)
            return;

        IdText = EmevdEventOps.NextFreeId(File.Emevd.Events, Event != null ? Event.ID + 1 : 0).ToString();
        NewRest = EMEVD.Event.RestBehaviorType.Default;
        OpenNew = true;
    }

    /// <summary>A copy of the selected event under the next free ID after it, placed right after it</summary>
    public void Duplicate()
    {
        if (!HasEvent)
            return;

        var file = File;
        var source = Event;
        var copy = EmevdEventOps.Copy(source, EmevdEventOps.NextFreeId(file.Emevd.Events, source.ID + 1));

        View.Edit(EmevdEventListChange.Insert(file, copy, file.Emevd.Events.IndexOf(source) + 1));
        View.Selection.JumpTo(file, copy);
        Smithbox.Log(this, LOC.Get("EMEVD_Event_Duplicated", source.ID, copy.ID));
    }

    /// <summary>Removes the selected event (undoable). Calls to it in the file are left, and reported.</summary>
    public void Delete()
    {
        if (!HasEvent)
            return;

        var file = File;
        var ev = Event;
        var index = file.Emevd.Events.IndexOf(ev);
        var calls = file.Emevd.Events.Sum(e => EmevdEventOps.CallLines(EmevdScript.Build(Catalog, e), ev.ID).Count);

        View.Edit(EmevdEventListChange.Remove(file, ev));

        var events = file.Emevd.Events;
        View.Selection.SelectEvent(events.Count > 0 ? events[Math.Min(index, events.Count - 1)] : null);
        View.Selection.ScrollToEvent = true;

        if (calls > 0)
            Smithbox.Log(this, LOC.Get("EMEVD_Event_Deleted_Called", ev.ID, calls), Microsoft.Extensions.Logging.LogLevel.Warning);
    }

    public void ChangeId()
    {
        if (!HasEvent)
            return;

        IdText = Event.ID.ToString();
        UpdateCalls = true;
        OpenChangeId = true;
    }

    // ---------------------------------------------------------------- modals

    /// <summary>Drawn every frame, so the modals can open outside the menus that ask for them</summary>
    public void DisplayPopups()
    {
        if (OpenNew)
        {
            ImGui.OpenPopup($"{LOC.Get("EMEVD_Event_New")}###emevdEventNewModal");
            OpenNew = false;
        }

        if (OpenChangeId)
        {
            ImGui.OpenPopup($"{LOC.Get("EMEVD_Event_Change_Id")}###emevdEventChangeIdModal");
            OpenChangeId = false;
        }

        NewModal();
        ChangeIdModal();
    }

    private void NewModal()
    {
        ImGui.SetNextWindowSize(new Vector2(420, 0) * DPI.UIScale(), ImGuiCond.Always);

        var open = true;
        if (!ImGui.BeginPopupModal($"{LOC.Get("EMEVD_Event_New")}###emevdEventNewModal", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var file = File;
        if (file == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("EMEVD_Event_New_Hint", file.Name));

        // ID
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_Event_Id_Header"), LOC.Get("EMEVD_Event_Id_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##emevdNewEventId", ref IdText, 20, ImGuiInputTextFlags.CharsDecimal);
        GUI.Tooltip(LOC.Get("EMEVD_Event_Id_Header_TT"));

        var error = IdError(file, null, out var id);
        if (error != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, error);

        // Rest behaviour
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_Properties_Event_Rest"), LOC.Get("EMEVD_Edit_Rest_TT"));
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##emevdNewEventRest", EmevdFormat.RestBehavior(NewRest)))
        {
            foreach (var rest in Enum.GetValues<EMEVD.Event.RestBehaviorType>())
            {
                if (ImGui.Selectable(EmevdFormat.RestBehavior(rest), rest == NewRest))
                    NewRest = rest;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("EMEVD_Edit_Rest_TT"));

        // Actions
        GUI.Spacer();
        GUI.ConditionalMultiButtonInput("emevdNewEventActions",
            "emevdNewEventCreate", LOC.Get("EMEVD_Event_Create"), LOC.Get("EMEVD_Event_Create_TT"),
            () =>
            {
                var ev = new EMEVD.Event(id, NewRest);
                var after = Event != null ? file.Emevd.Events.IndexOf(Event) + 1 : file.Emevd.Events.Count;
                View.Edit(EmevdEventListChange.Insert(file, ev, after));
                View.Selection.JumpTo(file, ev);
                ImGui.CloseCurrentPopup();
            }, error == null,

            "emevdNewEventCancel", LOC.Get("EMEVD_Event_Cancel"), LOC.Get("EMEVD_Event_Cancel_TT"),
            ImGui.CloseCurrentPopup, true);

        ImGui.EndPopup();
    }

    private void ChangeIdModal()
    {
        ImGui.SetNextWindowSize(new Vector2(420, 0) * DPI.UIScale(), ImGuiCond.Always);

        var open = true;
        if (!ImGui.BeginPopupModal($"{LOC.Get("EMEVD_Event_Change_Id")}###emevdEventChangeIdModal", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var file = File;
        var ev = Event;
        if (file == null || ev == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("EMEVD_Event_Change_Id_Hint", ev.ID, file.Name));

        // ID
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_Event_Id_Header"), LOC.Get("EMEVD_Event_Id_Header_TT"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##emevdChangeEventId", ref IdText, 20, ImGuiInputTextFlags.CharsDecimal);
        GUI.Tooltip(LOC.Get("EMEVD_Event_Id_Header_TT"));

        var error = IdError(file, ev, out var id);
        if (error != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, error);

        // Calls
        var calls = file.Emevd.Events
            .Select(e => (e, lines: EmevdEventOps.CallLines(EmevdScript.Build(Catalog, e), ev.ID)))
            .Where(x => x.lines.Count > 0)
            .ToList();
        var callCount = calls.Sum(c => c.lines.Count);

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_Event_Calls_Header"), LOC.Get("EMEVD_Event_Calls_Header_TT"));
        ImGui.Checkbox($"{LOC.Get("EMEVD_Event_Update_Calls", callCount)}##emevdChangeIdUpdateCalls", ref UpdateCalls);
        GUI.Tooltip(LOC.Get("EMEVD_Event_Update_Calls_TT"));

        if (file.IsCommon)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Event_Change_Id_Common"));

        // Actions
        GUI.Spacer();
        GUI.ConditionalMultiButtonInput("emevdChangeIdActions",
            "emevdChangeIdApply", LOC.Get("EMEVD_Event_Apply"), LOC.Get("EMEVD_Event_Apply_TT"),
            () =>
            {
                var actions = new List<EditorAction>
                {
                    new EmevdValueChange<long>(file, () => ev.ID, v => ev.ID = v, id)
                };

                if (UpdateCalls)
                {
                    foreach (var (_, lines) in calls)
                    {
                        foreach (var line in lines)
                        {
                            var arg = line.Decoded.Args.FirstOrDefault(a => a.Offset == line.FlowArgOffset);
                            if (arg.Def != null && EmevdDecoded.EncodeInt(arg.Type, id, out var bytes))
                                actions.Add(new EmevdArgChange(file, line.Instr, arg.Offset, bytes));
                        }
                    }
                }

                View.ActionManager.ExecuteAction(new CompoundAction(actions));
                ImGui.CloseCurrentPopup();
            }, error == null && id != ev.ID,

            "emevdChangeIdCancel", LOC.Get("EMEVD_Event_Cancel"), LOC.Get("EMEVD_Event_Cancel_TT"),
            ImGui.CloseCurrentPopup, true);

        ImGui.EndPopup();
    }

    /// <summary>Why the typed ID can't be used, or null: not a number, out of range, or another event's</summary>
    private string IdError(EmevdFileWrapper file, EMEVD.Event self, out long id)
    {
        if (!long.TryParse(IdText.Trim(), out id) || id < 0 || id > uint.MaxValue)
            return LOC.Get("EMEVD_Event_Id_Invalid");

        var taken = file.FindEvent(id);
        if (taken != null && taken != self)
            return LOC.Get("EMEVD_Event_Id_Taken", id);

        return null;
    }
}
