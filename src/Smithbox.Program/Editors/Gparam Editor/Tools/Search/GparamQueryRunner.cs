using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// A query in progress. The matches are passed over through the queue and picked up as the tool is drawn.
/// </summary>
public class GparamQueryRun
{
    public readonly CancellationTokenSource Cancellation = new();
    public readonly ConcurrentQueue<GparamQueryMatch> Found = new();

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
/// Gathers the files to query on the main thread, and runs the query over them in the background.
/// A global query reads every file of the project, so it can take a while.
/// </summary>
public class GparamQueryRunner
{
    public GparamEditorView View;
    public ProjectEntry Project;

    public GparamQueryRunner(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    #region Sources
    /// <summary>
    /// The selected file, which must be open, narrowed down to a group or field if given
    /// </summary>
    public GparamQuerySource GetLocalSource(FileDictionaryEntry fileEntry, GPARAM gparam, string groupKey, string fieldKey)
    {
        return new GparamQuerySource(fileEntry, GetAlias(fileEntry), gparam, groupKey, fieldKey);
    }

    /// <summary>
    /// Every file of the project, as the file list shows them
    /// </summary>
    public List<GparamQuerySource> GetGlobalSources()
    {
        var sources = new List<GparamQuerySource>();

        foreach (var (fileEntry, wrapper) in Project.Handler.GparamData.PrimaryBank.Entries)
        {
            if (!IsListed(fileEntry))
                continue;

            // An open file, or one with unsaved changes, is queried as it is in the editor.
            // The rest are read again for the query, without being opened.
            sources.Add(new GparamQuerySource(fileEntry, GetAlias(fileEntry), wrapper.Gparam));
        }

        return sources;
    }

    /// <summary>
    /// For BB, the file list shows either the loose files or the containers, which hold the same GPARAMs,
    /// so only the ones on show are searched
    /// </summary>
    private bool IsListed(FileDictionaryEntry fileEntry)
    {
        if (Project.Descriptor.ProjectType is not ProjectType.BB)
            return true;

        return CFG.Current.GparamEditor_File_List_Display_BB_BND_Files
            ? fileEntry.Extension != "gparam"
            : fileEntry.Extension != "gparambnd";
    }

    private string GetAlias(FileDictionaryEntry fileEntry)
    {
        return AliasHelper.GetGparamAliasName(Project, fileEntry.Filename);
    }
    #endregion

    #region Run
    /// <summary>
    /// Starts the query on a background task. The progress counts the groups when countGroups is set,
    /// for a query of one file, or else the files.
    /// </summary>
    public GparamQueryRun Start(GparamQuery query, List<GparamQuerySource> sources, bool countGroups, int maxResults)
    {
        var run = new GparamQueryRun
        {
            MaxResults = maxResults,
            Total = countGroups ? sources.Sum(e => e.Gparam?.Params.Count ?? 0) : sources.Count
        };

        var token = run.Cancellation.Token;
        var matcher = new GparamQueryMatcher(Project);

        Task.Run(() =>
        {
            try
            {
                Run(run, matcher, query, sources, countGroups, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("GPARAM_SearchTool_Failed"), e);
            }
            finally
            {
                run.Done = true;
            }
        }, token);

        return run;
    }

    private void Run(GparamQueryRun run, GparamQueryMatcher matcher, GparamQuery query, List<GparamQuerySource> sources,
        bool countGroups, CancellationToken token)
    {
        var bank = Project.Handler.GparamData.PrimaryBank;

        foreach (var item in sources)
        {
            token.ThrowIfCancellationRequested();

            run.Status = item.FileEntry.Filename;

            if (!matcher.MatchFile(query, item.FileEntry, item.Alias))
            {
                run.Progress++;
                continue;
            }

            // A file matched on its own, with nothing asked of its groups, so it needn't be read
            if (!query.MatchesGroups)
            {
                if (!AddMatch(run, CreateMatch(item, null, -1, null, -1, null, -1, null, Label(item, null))))
                    return;

                run.Progress++;
                continue;
            }

            var gparam = item.Gparam;

            if (gparam == null)
            {
                try
                {
                    bank.TryReadGraphicsParam(item.FileEntry, out gparam);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("GPARAM_SearchTool_File_Failed", item.FileEntry.Filename), e);
                }
            }

            if (gparam == null)
            {
                run.Progress++;
                continue;
            }

            var groups = gparam.Params.ToArray();

            for (int g = 0; g < groups.Length; g++)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    if (!QueryGroup(run, matcher, query, item, groups[g], g, token))
                        return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // The open file can be edited while it is queried, which can upset the lists being read
                    Smithbox.LogError(this, LOC.Get("GPARAM_SearchTool_File_Failed", item.FileEntry.Filename), e);
                }

                if (countGroups)
                {
                    run.Progress++;
                }
            }

            if (!countGroups)
            {
                run.Progress++;
            }
        }
    }

    /// <summary>
    /// Returns false once there are as many matches as are kept
    /// </summary>
    private bool QueryGroup(GparamQueryRun run, GparamQueryMatcher matcher, GparamQuery query, GparamQuerySource item,
        GPARAM.Param group, int groupIndex, CancellationToken token)
    {
        if (group == null)
            return true;

        if (item.GroupKey != null && group.Key != item.GroupKey)
            return true;

        if (!matcher.MatchGroup(query, group))
            return true;

        var groupName = matcher.GetGroupName(group.Key);

        if (!query.MatchesFields)
        {
            return AddMatch(run, CreateMatch(item, group, groupIndex, null, -1, null, -1, null, Label(item, groupName)));
        }

        var fields = group.Fields.ToArray();

        for (int f = 0; f < fields.Length; f++)
        {
            token.ThrowIfCancellationRequested();

            var field = fields[f];

            if (field == null)
                continue;

            if (item.FieldKey != null && field.Key != item.FieldKey)
                continue;

            if (!matcher.MatchField(query, group.Key, field))
                continue;

            var fieldName = matcher.GetFieldName(group.Key, field.Key);

            if (!query.HasValue)
            {
                if (!AddMatch(run, CreateMatch(item, group, groupIndex, field, f, null, -1, null,
                    Label(item, $"{groupName}  >  {fieldName}"))))
                    return false;

                continue;
            }

            var values = field.Values.ToArray();

            for (int v = 0; v < values.Length; v++)
            {
                var value = values[v];

                if (value == null)
                    continue;

                if (!matcher.MatchValue(query, group.Key, field, value, out var shown))
                    continue;

                var label = query.ValueTarget is GparamQueryValueTarget.Value
                    ? $"{groupName}  >  {fieldName}  [{value.ID}] = {shown}"
                    : $"{groupName}  >  {fieldName}  [{value.ID}]  {GetValueTargetName(query.ValueTarget)} = {shown}";

                if (!AddMatch(run, CreateMatch(item, group, groupIndex, field, f, value, v, shown, Label(item, label))))
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// The label leaves out the file, which the tool adds for a global search, as a local search has the one file.
    /// A match on the file itself is labelled by its name.
    /// </summary>
    private static string Label(GparamQuerySource item, string text)
    {
        return text ?? item.FileEntry.Filename;
    }

    public static string GetValueTargetName(GparamQueryValueTarget target) => target switch
    {
        GparamQueryValueTarget.Value => LOC.Get("GPARAM_SearchTool_Value_Target_Value"),
        GparamQueryValueTarget.ID => LOC.Get("GPARAM_SearchTool_Value_Target_ID"),
        GparamQueryValueTarget.TimeOfDay => LOC.Get("GPARAM_SearchTool_Value_Target_Time_Of_Day"),
        _ => $"{target}"
    };

    private static GparamQueryMatch CreateMatch(GparamQuerySource item,
        GPARAM.Param group, int groupIndex,
        GPARAM.IField field, int fieldIndex,
        GPARAM.IFieldValue value, int valueIndex,
        string valueText, string label)
    {
        return new GparamQueryMatch(item.FileEntry,
            group, group?.Key, groupIndex,
            field, field?.Key, fieldIndex,
            value, value?.ID ?? -1, valueIndex,
            valueText, label);
    }

    private static bool AddMatch(GparamQueryRun run, GparamQueryMatch match)
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
    /// Opens the file of a match if it isn't open, then selects its group, field and value, scrolling the lists to them
    /// </summary>
    public void Jump(GparamQueryMatch match)
    {
        var selection = View.Selection;

        if (!selection.IsSelectedFile(match.FileEntry) || !selection.IsFileSelected())
        {
            selection.SetFileSelection(match.FileEntry);
        }

        selection.FocusFile = true;

        if (match.GroupKey == null)
            return;

        if (!TryResolve(match, out var groupIndex, out var group, out var fieldIndex, out var field,
            out var valueIndex, out var value))
        {
            // The match may have been removed since, or moved by an edit
            Smithbox.LogError(this, LOC.Get("GPARAM_SearchTool_Stale_Result"));
            return;
        }

        if (selection._selectedParamGroupKey != group.Key || selection._selectedParamGroupIndex != groupIndex)
        {
            selection.SetGparamGroup(groupIndex, group);
            View.FieldListView.InvalidateAddOptions();
        }

        selection.FocusGroup = true;

        if (field == null)
            return;

        if (selection._selectedParamFieldKey != field.Key || selection._selectedParamFieldIndex != fieldIndex)
        {
            selection.SetGparamField(fieldIndex, field);
        }

        selection.FocusField = true;

        if (value == null)
            return;

        selection.SelectSingleFieldValue(valueIndex, value);
        selection.FocusValue = true;
    }

    /// <summary>
    /// Finds what a match points to in the GPARAM the editor has now. A file that wasn't open was queried through
    /// a copy, so what was found is looked up again by its keys, ID and position. False if it is no longer there.
    /// </summary>
    public bool TryResolve(GparamQueryMatch match,
        out int groupIndex, out GPARAM.Param group,
        out int fieldIndex, out GPARAM.IField field,
        out int valueIndex, out GPARAM.IFieldValue value)
    {
        groupIndex = -1;
        group = null;
        fieldIndex = -1;
        field = null;
        valueIndex = -1;
        value = null;

        var gparam = View.Selection.GetSelectedGparam();

        if (gparam == null)
            return false;

        groupIndex = FindIndex(gparam.Params, match.Group, match.GroupIndex, e => e?.Key == match.GroupKey);

        if (groupIndex < 0)
            return false;

        group = gparam.Params[groupIndex];

        if (match.FieldKey == null)
            return true;

        fieldIndex = FindIndex(group.Fields, match.Field, match.FieldIndex, e => e?.Key == match.FieldKey);

        if (fieldIndex < 0)
            return false;

        field = group.Fields[fieldIndex];

        if (match.ValueIndex < 0)
            return true;

        // IDs are unique within a field, so the ID finds the value even if it has moved
        valueIndex = FindIndex(field.Values, match.Value, match.ValueIndex, e => e?.ID == match.ValueID);

        if (valueIndex < 0)
            return false;

        value = field.Values[valueIndex];

        return true;
    }

    /// <summary>
    /// The position of the object itself if it is still in the list, else of the first entry that fits, preferring
    /// the one at the position it was found at
    /// </summary>
    private static int FindIndex<T>(IReadOnlyList<T> list, T original, int position, Func<T, bool> fits) where T : class
    {
        if (original != null)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], original))
                    return i;
            }
        }

        if (position >= 0 && position < list.Count && fits(list[position]))
            return position;

        for (int i = 0; i < list.Count; i++)
        {
            if (fits(list[i]))
                return i;
        }

        return -1;
    }
    #endregion
}
