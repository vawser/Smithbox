using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActCommandQueue
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    public TimeActCommandQueue(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] commands)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (commands == null)
            return;

        if (commands.Length <= 0)
            return;

        // Select an animation: "tae/select/{character}/{full animation ID}", e.g. tae/select/c0000/3000
        // (sent by the Havok Editor's behavior views)
        if (commands[0] == "select" && commands.Length >= 3)
        {
            SelectAnimation(activeView, commands[1], commands[2]);
        }

        // Retime an animation's events as its clip was: "tae/retime/{character}/{full animation ID}/{scale}/{offset}/
        // {duration}/{frame duration}", times in seconds, invariant culture (sent by the Animation Editor's Clip tool)
        if (commands[0] == "retime" && commands.Length >= 7)
        {
            RetimeAnimation(activeView, commands);
        }

        // Add an event: "tae/addevent/{character}/{full animation ID}/{type}/{first field}/{start}/{end}", times in
        // seconds, invariant culture (sent by the Animation Editor's gravity warning, for Disable Gravity)
        if (commands[0] == "addevent" && commands.Length >= 7)
        {
            AddEvent(activeView, commands);
        }

        // Make an animation's entry from another's: "tae/copyanim/{character}/{template full ID}/{new full ID}" (sent
        // by the Animation Editor for a clip saved under a new ID)
        if (commands[0] == "copyanim" && commands.Length >= 4)
        {
            CopyAnimation(activeView, commands);
        }
    }

    /// <summary>
    /// Adds an entry for a new animation ID, with the events of the template's entry (of the animation it imports them
    /// from, for an <c>ImportOtherAnim</c> one), to the time act of the new ID's category, in ID order, as one
    /// undoable step. The header is a plain Standard one that imports nothing, so the entry plays its own HKX (the new
    /// clip) rather than the template's; loop and delay load are kept. Refused when the ID has an entry already or its
    /// category has no time act.
    /// </summary>
    private void CopyAnimation(TimeActEditorView view, string[] commands)
    {
        if (!long.TryParse(commands[2], out var templateId) || !long.TryParse(commands[3], out var newId))
            return;

        var source = Project.Handler.TimeActData?.PrimaryBank?.GetModelSources()
            .FirstOrDefault(e => e.Name.Equals(commands[1], StringComparison.OrdinalIgnoreCase));

        if (source == null)
        {
            SelectAnimation(view, commands[1], commands[2]);
            return;
        }

        var size = source.AnimationCategorySize;
        var newName = source.FormatAnimationName(newId);

        if (source.TryFindAnimation(newId, out _, out _))
        {
            Smithbox.LogError(this, LOC.Get("TAE_Command_Copy_Exists", newName));
            view = SelectAnimation(view, commands[1], commands[3]);
            return;
        }

        if (!source.TryFindAnimation(templateId, out var templateEntry, out var template))
        {
            Smithbox.LogError(this, LOC.Get("TAE_Command_Select_Animation_Missing", source.FormatAnimationName(templateId), commands[1]));
            return;
        }

        // The events: the template's own, or those of the animation it imports them from (a few steps at most)
        var (eventsEntry, eventsFrom) = (templateEntry, template);
        for (var step = 0; step < 8 && eventsFrom.MiniHeader is SoulsFormats.TAE.Animation.AnimMiniHeader.ImportOtherAnim import; step++)
        {
            var from = eventsEntry.GetFullAnimationID(import.ImportFromAnimID, size);
            if (!source.TryFindAnimation(from, out var nextEntry, out var next))
                break;

            (eventsEntry, eventsFrom) = (nextEntry, next);
        }

        // The time act of the new ID's category
        var category = (int)(newId / size);
        var entry = source.TimeActs.FirstOrDefault(e => e.BindIndex == category);
        entry?.Load();
        if (entry?.TimeAct == null)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Command_Copy_No_Time_Act", newName, category));
            return;
        }

        // Stored as the template is: full IDs, or IDs within the category
        var id = template.ID >= size ? newId : newId - (long)category * size;

        var oldName = source.GetAnimationName(templateEntry, template);
        var fileName = template.AnimFileName ?? "";
        if (fileName.Contains(oldName, StringComparison.OrdinalIgnoreCase))
            fileName = fileName.Replace(oldName, newName, StringComparison.OrdinalIgnoreCase);

        var copy = TimeActEntry.CloneAnimation(eventsFrom, id, fileName, eventsEntry.TimeAct.BigEndian, false);

        var header = new SoulsFormats.TAE.Animation.AnimMiniHeader.Standard();
        if (template.MiniHeader is SoulsFormats.TAE.Animation.AnimMiniHeader.Standard standard)
        {
            header.IsLoopByDefault = standard.IsLoopByDefault;
            header.AllowDelayLoad = standard.AllowDelayLoad;
            header.Unk03 = standard.Unk03;
        }
        copy.MiniHeader = header;

        var animations = entry.TimeAct.Animations;
        var index = animations.FindIndex(e => e.ID > id);
        view.ViewportActionManager.ExecuteAction(new TimeActAddAnimationAction(view, entry, copy, index < 0 ? animations.Count : index));

        view = SelectAnimation(view, commands[1], commands[3]);

        Smithbox.Log(this, LOC.Get("TAE_Command_Copy_Done", newName, source.FormatAnimationName(templateId), copy.Events.Count, entry.Name));
    }

    /// <summary>
    /// Adds an event of a type to an animation as one undoable step: the template's defaults, with its first field set
    /// (an enum such as ChrActionFlag's FlagType), as the graph's Add Event makes one. Not for an entry that imports
    /// another animation's events, or a type the game's templates don't have.
    /// </summary>
    private void AddEvent(TimeActEditorView view, string[] commands)
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        if (!int.TryParse(commands[3], culture, out var type) || !int.TryParse(commands[4], culture, out var firstField) ||
            !float.TryParse(commands[5], culture, out var start) || !float.TryParse(commands[6], culture, out var end) ||
            end < start)
            return;

        view = SelectAnimation(view, commands[1], commands[2]);

        var data = Project.Handler.TimeActData;
        var source = data?.PrimaryBank?.GetModelSources()
            .FirstOrDefault(e => e.Name.Equals(commands[1], StringComparison.OrdinalIgnoreCase));

        if (source == null || !long.TryParse(commands[2], out var id) || !source.TryFindAnimation(id, out var entry, out var animation))
            return;

        var name = source.FormatAnimationName(id);
        if (animation.MiniHeader is SoulsFormats.TAE.Animation.AnimMiniHeader.ImportOtherAnim)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Command_Add_Event_Imported", name));
            return;
        }

        if (data.GetTemplate(source) is not { } template || !template.TryGetValue(type, out var eventTemplate))
        {
            Smithbox.LogError(this, LOC.Get("TAE_Command_Add_Event_No_Template", type));
            return;
        }

        var unk04 = animation.Events.FirstOrDefault(e => e.Type == type)?.Unk04 ?? 0;
        var ev = new SoulsFormats.TAE.Event(start, end, type, unk04, entry.TimeAct.BigEndian, eventTemplate);

        // The first field takes the type the template gives it
        if (eventTemplate.Keys.FirstOrDefault() is { } field && ev.Parameters[field] is { } current)
        {
            try
            {
                ev.Parameters[field] = Convert.ChangeType(firstField, current.GetType(), culture);
            }
            catch (Exception e) when (e is InvalidCastException or OverflowException or FormatException)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Command_Add_Event_Field", firstField, field), e);
                return;
            }
        }

        view.ViewportActionManager.ExecuteAction(new TimeActAddEventAction(view.Selection, entry, animation, ev,
            groupMap: data.GetGroupMap()));

        Smithbox.Log(this, LOC.Get("TAE_Command_Add_Event_Done", TimeActAction.NameEvent(ev), name,
            start.ToString("0.###", culture), end.ToString("0.###", culture)));
    }

    /// <summary>
    /// Maps every event of an animation's time act entry by <c>t' = t × scale + offset</c> (an open end stays open),
    /// rounded to whole frames, as one undoable step. Events then wholly before 0 or from the clip's end on would never
    /// play and are removed in the same step; one partly before 0 starts at 0. An entry that imports another
    /// animation's events is left alone, as they belong to that animation.
    /// </summary>
    private void RetimeAnimation(TimeActEditorView view, string[] commands)
    {
        var culture = System.Globalization.CultureInfo.InvariantCulture;
        if (!float.TryParse(commands[3], culture, out var scale) || !float.TryParse(commands[4], culture, out var offset) ||
            !float.TryParse(commands[5], culture, out var duration) || !float.TryParse(commands[6], culture, out var frame) ||
            scale <= 0 || frame <= 0)
            return;

        // Shows the entry, so the change is in view
        view = SelectAnimation(view, commands[1], commands[2]);

        var source = Project.Handler.TimeActData?.PrimaryBank?.GetModelSources()
            .FirstOrDefault(e => e.Name.Equals(commands[1], StringComparison.OrdinalIgnoreCase));

        if (source == null || !long.TryParse(commands[2], out var id) || !source.TryFindAnimation(id, out var entry, out var animation))
            return;

        if (animation.MiniHeader is SoulsFormats.TAE.Animation.AnimMiniHeader.ImportOtherAnim)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Command_Retime_Imported", source.FormatAnimationName(id)));
            return;
        }

        float Map(float t) => MathF.Round((t * scale + offset) / frame) * frame;

        var actions = new List<StudioCore.Editors.Common.ViewportAction>();
        var removed = new List<SoulsFormats.TAE.Event>();
        foreach (var ev in animation.Events)
        {
            var start = Map(ev.StartTime);
            var end = ev.IsInfiniteEnd ? ev.EndTime : Map(ev.EndTime);

            if (!ev.IsInfiniteEnd && end <= 0 || start > duration + frame * 0.5f)
            {
                removed.Add(ev);
                continue;
            }

            start = Math.Max(start, 0);
            if (!ev.IsInfiniteEnd)
                end = Math.Max(end, start + frame);

            if (start != ev.StartTime || end != ev.EndTime)
                actions.Add(new TimeActMoveEventAction(entry, ev, ev.StartTime, ev.EndTime, start, end));
        }

        if (removed.Count > 0)
            actions.Add(new TimeActDeleteEventsAction(view.Selection, entry, animation, removed));

        if (actions.Count == 0)
            return;

        view.ViewportActionManager.ExecuteAction(new TimeActCompoundAction(actions,
            LOC.Get("TAE_History_Retime_Events", source.FormatAnimationName(id))));

        Smithbox.Log(this, LOC.Get("TAE_Command_Retime_Done", source.FormatAnimationName(id),
            actions.Count - (removed.Count > 0 ? 1 : 0), removed.Count));
    }

    /// <summary>
    /// Selects a character's source, then the time act and animation with a full animation ID,
    /// as the search tool's jump does. Gives the view that shows it (the source may be open in another one).
    /// </summary>
    private TimeActEditorView SelectAnimation(TimeActEditorView view, string character, string animationID)
    {
        var bank = Project.Handler.TimeActData?.PrimaryBank;

        var source = bank?.GetModelSources()
            .FirstOrDefault(e => e.Name.Equals(character, StringComparison.OrdinalIgnoreCase));

        if (source == null)
        {
            // The sources are found in the background, so a character may not be listed yet
            Smithbox.LogError(this, LOC.Get(bank is { IsScanning: true }
                ? "TAE_Command_Select_Source_Scanning" : "TAE_Command_Select_Source_Missing", character));
            return view;
        }

        if (view.Selection.SelectedTimeActContainerWrapper != source)
        {
            // The source may be open in another view, which then takes the rest of the selection
            view = view.SourceWindow.SelectSource(source);
            view.SourceWindow.ScrollTo(source, source.ListType);
        }

        if (!long.TryParse(animationID, out var id))
            return view;

        if (!source.TryFindAnimation(id, out var entry, out var animation))
        {
            Smithbox.LogError(this, LOC.Get("TAE_Command_Select_Animation_Missing", source.FormatAnimationName(id), character));
            return view;
        }

        if (view.Selection.SelectedTimeAct != entry)
        {
            view.Selection.SelectedTimeAct = entry;
            view.SelectionWindow.ScrollTo(entry);
        }

        if (view.Selection.SelectedAnimation != animation)
        {
            view.AnimationSelectionWindow.SelectAnimation(animation);
        }
        view.AnimationSelectionWindow.ScrollTo(animation);

        return view;
        view.AnimationSelectionWindow.ScrollTo(animation);
    }
}
