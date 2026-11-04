using SoulsFormats;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// A query in progress. The matches are passed over through the queue and picked up as the tool is drawn.
/// </summary>
public class TimeActQueryRun
{
    public readonly CancellationTokenSource Cancellation = new();
    public readonly ConcurrentQueue<TimeActQueryMatch> Found = new();

    public volatile int Progress;
    public volatile int Total;
    public volatile string Status = "";
    public volatile bool Done;
    public volatile bool Truncated;

    /// <summary>
    /// The most matches that are kept, or 0 for no limit
    /// </summary>
    public int MaxResults;

    public int Count;

    public void Cancel() => Cancellation.Cancel();
}

/// <summary>
/// Gathers the sources to query on the main thread, and runs the query over them in the background.
/// A global query reads the time acts of every source, so it can take a while.
/// </summary>
public class TimeActQueryRunner
{
    public TimeActEditorView View;
    public ProjectEntry Project;
    public TimeActQueryMatcher Matcher;

    public TimeActQueryRunner(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        Matcher = new TimeActQueryMatcher(project);
    }

    #region Sources
    /// <summary>
    /// The given time acts of one source, which must be open
    /// </summary>
    public TimeActQuerySource GetLocalSource(TimeActContainerWrapper source, List<TimeActEntry> entries)
    {
        return new TimeActQuerySource(source, GetListType(source), entries, Project.Handler.TimeActData.GetTemplate(source));
    }

    /// <summary>
    /// Every source of the project, characters then assets, each in name order
    /// </summary>
    public List<TimeActQuerySource> GetGlobalSources()
    {
        var data = Project.Handler.TimeActData;
        var bank = data.PrimaryBank;

        var sources = new List<TimeActQuerySource>();

        foreach (var (listType, list) in new[] { (ModelListType.Character, bank.GetCharacters()), (ModelListType.Asset, bank.GetAssets()) })
        {
            foreach (var source in list.OrderBy(e => e.Name))
            {
                // An open source, or one with unsaved changes, is queried as it is in the editor.
                // The rest are read again for the query, without being opened.
                var entries = source.TimeActs.Count > 0 ? source.TimeActs.ToList() : null;

                sources.Add(new TimeActQuerySource(source, listType, entries, data.GetTemplate(source)));
            }
        }

        return sources;
    }

    public ModelListType GetListType(TimeActContainerWrapper source)
    {
        return Project.Handler.TimeActData.PrimaryBank.GetAssets().Contains(source)
            ? ModelListType.Asset
            : ModelListType.Character;
    }
    #endregion

    #region Run
    /// <summary>
    /// Starts the query on a background task. The progress counts the time acts when countEntries is set,
    /// for a query of one source, or else the sources.
    /// </summary>
    public TimeActQueryRun Start(ITimeActQueryFilter query, List<TimeActQuerySource> sources, bool countEntries, int maxResults)
    {
        var run = new TimeActQueryRun
        {
            MaxResults = maxResults,
            Total = countEntries ? sources.Sum(e => e.Entries?.Count ?? 0) : sources.Count
        };

        var token = run.Cancellation.Token;

        Task.Run(() =>
        {
            try
            {
                Run(run, query, sources, countEntries, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_SearchTool_Failed"), e);
            }
            finally
            {
                run.Done = true;
            }
        }, token);

        return run;
    }

    private void Run(TimeActQueryRun run, ITimeActQueryFilter query, List<TimeActQuerySource> sources, bool countEntries,
        CancellationToken token)
    {
        foreach (var item in sources)
        {
            token.ThrowIfCancellationRequested();

            run.Status = item.Source.Name;

            List<TimeActEntry> entries;

            try
            {
                entries = item.Entries ?? item.Source.ReadTimeActs();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Source", item.Source.SourcePath), e);

                run.Progress++;
                continue;
            }

            foreach (var entry in entries)
            {
                token.ThrowIfCancellationRequested();

                if (countEntries)
                {
                    run.Status = $"{item.Source.Name} {entry.Name}";
                }

                try
                {
                    if (!QueryTimeAct(run, query, item, entry, token))
                        return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // The open time act can be edited while it is queried, which can upset the lists being read
                    Smithbox.LogError(this, LOC.Get("TAE_SearchTool_TimeAct_Failed", item.Source.Name, entry.Name), e);
                }

                if (countEntries)
                {
                    run.Progress++;
                }
            }

            if (!countEntries)
            {
                run.Progress++;
            }
        }
    }

    /// <summary>
    /// Returns false once there are as many matches as are kept
    /// </summary>
    private bool QueryTimeAct(TimeActQueryRun run, ITimeActQueryFilter query, TimeActQuerySource item, TimeActEntry entry,
        CancellationToken token)
    {
        entry.Load();

        if (entry.TimeAct == null)
            return true;

        var source = item.Source;

        foreach (var animation in entry.TimeAct.Animations.ToArray())
        {
            token.ThrowIfCancellationRequested();

            var animationName = source.GetAnimationName(entry, animation);

            if (!query.MatchAnimation(Matcher, source, animation, animationName, out var animationMatch))
                continue;

            if (!query.MatchesEvents)
            {
                var label = animationMatch != null ? $"{animationName}  ({animationMatch})" : animationName;

                if (!AddMatch(run, CreateMatch(item, entry, animation, null, -1, label)))
                    return false;

                continue;
            }

            if (query.MatchAnimationProperties(Matcher, animation, out var propertyMatch))
            {
                if (!AddMatch(run, CreateMatch(item, entry, animation, null, -1, $"{animationName}  {propertyMatch}")))
                    return false;
            }

            var events = animation.Events.ToArray();

            for (int i = 0; i < events.Length; i++)
            {
                var ev = events[i];
                var eventName = TimeActQueryMatcher.GetEventName(ev, item.Template);

                if (!query.MatchEventType(Matcher, ev, eventName))
                    continue;

                // Events whose template hasn't been applied yet are read through a copy, so the time act is left as it is
                var readable = entry.GetReadableEvent(ev, item.Template);

                if (!query.MatchEvent(Matcher, ev, readable, out var fieldMatch))
                    continue;

                var label = fieldMatch != null
                    ? $"{animationName}  {eventName}  {fieldMatch}"
                    : $"{animationName}  {eventName}";

                if (!AddMatch(run, CreateMatch(item, entry, animation, ev, i, label)))
                    return false;
            }
        }

        return true;
    }

    private static TimeActQueryMatch CreateMatch(TimeActQuerySource item, TimeActEntry entry, TAE.Animation animation,
        TAE.Event ev, int eventIndex, string label)
    {
        return new TimeActQueryMatch(item.Source, item.ListType,
            entry, entry.Name, entry.BindIndex,
            animation, animation.ID,
            ev, eventIndex, ev?.Type ?? -1,
            label);
    }

    private static bool AddMatch(TimeActQueryRun run, TimeActQueryMatch match)
    {
        if (run.MaxResults > 0 && run.Count >= run.MaxResults)
        {
            run.Truncated = true;
            return false;
        }

        run.Count++;
        run.Found.Enqueue(match);

        return true;
    }
    #endregion

    #region Navigation
    /// <summary>
    /// Opens the source of a match if it isn't open, then selects its time act, animation and event,
    /// and moves the playback to the start of the event
    /// </summary>
    public void Jump(TimeActQueryMatch match)
    {
        var selection = View.Selection;
        var source = match.Source;

        if (selection.SelectedTimeActContainerWrapper != source)
        {
            View.SourceWindow.SelectSource(source);
            View.SourceWindow.ScrollTo(source, match.ListType);
        }

        if (!TryResolve(match, out var entry, out var animation, out var ev))
        {
            // The match may have been removed since, or moved by an edit
            Smithbox.LogError(this, LOC.Get("TAE_SearchTool_Stale_Result"));
            return;
        }

        if (selection.SelectedTimeAct != entry)
        {
            selection.SelectedTimeAct = entry;
            View.SelectionWindow.ScrollTo(entry);
        }

        if (selection.SelectedAnimation != animation)
        {
            View.AnimationSelectionWindow.SelectAnimation(animation);
        }

        View.AnimationSelectionWindow.ScrollTo(animation);

        if (ev != null)
        {
            selection.SelectedEvent = ev;

            View.Playback.Pause();
            View.Playback.Seek(ev.StartTime);
        }
    }

    /// <summary>
    /// Finds what a match points to among the time acts the source has now. A source that wasn't open was queried
    /// through copies, so what was found is looked up again by its IDs and position. False if it is no longer there.
    /// </summary>
    public static bool TryResolve(TimeActQueryMatch match, out TimeActEntry entry, out TAE.Animation animation, out TAE.Event ev)
    {
        var source = match.Source;

        entry = source.TimeActs.Contains(match.Entry)
            ? match.Entry
            : source.TimeActs.FirstOrDefault(e => e.BindIndex == match.BindIndex && e.Name == match.EntryName);

        entry?.Load();

        animation = entry?.TimeAct == null ? null
            : entry.TimeAct.Animations.Contains(match.Animation) ? match.Animation
            : entry.TimeAct.Animations.FirstOrDefault(e => e.ID == match.AnimationID);

        ev = null;

        if (animation != null && match.EventIndex >= 0)
        {
            if (animation.Events.Contains(match.Event))
            {
                ev = match.Event;
            }
            else if (match.EventIndex < animation.Events.Count && animation.Events[match.EventIndex].Type == match.EventType)
            {
                ev = animation.Events[match.EventIndex];
            }
        }

        return animation != null && (match.EventIndex < 0 || ev != null);
    }
    #endregion
}
