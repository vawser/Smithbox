using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// Where a payload is taken from or applied to: the editor's selection, captured at one moment.
/// </summary>
public class GparamTransferTarget
{
    public FileDictionaryEntry FileEntry { get; init; }
    public GPARAM Gparam { get; init; }
    public GPARAM.Param Group { get; init; }
    public GPARAM.IField Field { get; init; }

    /// <summary>
    /// The selected values, in list order.
    /// </summary>
    public List<GPARAM.IFieldValue> Values { get; init; } = new();

    public static GparamTransferTarget FromSelection(GparamSelection selection)
    {
        if (selection == null)
            return new GparamTransferTarget();

        var field = selection.GetSelectedField();

        return new GparamTransferTarget
        {
            FileEntry = selection.SelectedFileEntry,
            Gparam = selection.GetSelectedGparam(),
            Group = selection.GetSelectedGroup(),
            Field = field,
            // SelectedFieldValues is keyed by index, so this is in list order. Keep only values still in the field.
            Values = field == null
                ? new List<GPARAM.IFieldValue>()
                : selection.GetSelectedValues().Where(e => field.IndexOfValue(e) >= 0).ToList()
        };
    }

    /// <summary>
    /// The same target, with the values replaced, e.g. for a context menu acting on one clicked value.
    /// </summary>
    public GparamTransferTarget WithValues(IEnumerable<GPARAM.IFieldValue> values)
    {
        return new GparamTransferTarget
        {
            FileEntry = FileEntry,
            Gparam = Gparam,
            Group = Group,
            Field = Field,
            Values = values.ToList()
        };
    }
}

/// <summary>
/// The result of checking a payload against a target, before anything changes.
/// </summary>
public class GparamTransferCheck
{
    /// <summary>
    /// Why it can't be applied, or null if it can.
    /// </summary>
    public string Error { get; init; }

    /// <summary>
    /// Something to point out that doesn't stop it, e.g. the payload is from another game.
    /// </summary>
    public string Warning { get; init; }

    /// <summary>
    /// What applying it will do, in one line.
    /// </summary>
    public string Description { get; init; }

    public bool CanApply => Error == null;
}

/// <summary>
/// Builds, checks and applies transfer payloads. Holds no UI and no state of its own: the context menus, the menu bar,
/// keybinds and the Tools window all go through it, so they behave the same.
/// </summary>
public class GparamTransferService
{
    private GparamEditorView View;
    private ProjectEntry Project;

    public GparamTransferService(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private string Game => Project.Descriptor.ProjectType.ToString();

    #region Build

    /// <summary>
    /// Take a payload of the kind from the target. Returns null with the reason in <paramref name="error"/> if the
    /// target has nothing of that kind.
    /// </summary>
    public GparamTransferPayload BuildPayload(GparamTransferKind kind, GparamTransferTarget target, out string error)
    {
        error = null;

        if (target?.Gparam == null || target.FileEntry == null)
        {
            error = LOC.Get("GPARAM_Transfer_Need_File");
            return null;
        }

        var source = new GparamTransferSource
        {
            File = target.FileEntry.Filename,
            Group = target.Group?.Key,
            Field = target.Field?.Key
        };

        switch (kind)
        {
            case GparamTransferKind.Gparam:
                source.Group = null;
                source.Field = null;
                return GparamTransferPayload.FromGparam(target.Gparam, Game, source);

            case GparamTransferKind.Group:
                if (target.Group == null)
                {
                    error = LOC.Get("GPARAM_Transfer_Need_Group");
                    return null;
                }

                source.Field = null;
                return GparamTransferPayload.FromGroups(new[] { target.Group }, Game, source);

            case GparamTransferKind.Field:
                if (target.Group == null || target.Field == null)
                {
                    error = LOC.Get("GPARAM_Transfer_Need_Field");
                    return null;
                }

                return GparamTransferPayload.FromFields(new[] { target.Field }, Game, source);

            case GparamTransferKind.Values:
                if (target.Field == null || target.Values.Count == 0)
                {
                    error = LOC.Get("GPARAM_Transfer_Need_Values");
                    return null;
                }

                return GparamTransferPayload.FromValues(target.Field, target.Values, Game, source);
        }

        error = LOC.Get("GPARAM_Transfer_Parse_Unknown");
        return null;
    }

    #endregion

    #region Check

    /// <summary>
    /// Whether the payload can go into the target with the mode for its kind, and what it will do there.
    /// Changes nothing.
    /// </summary>
    public GparamTransferCheck Check(GparamTransferPayload payload, GparamTransferTarget target, GparamPasteModes modes)
    {
        if (payload == null || payload.Count == 0)
            return new GparamTransferCheck { Error = LOC.Get("GPARAM_Transfer_Parse_No_Items") };

        string warning = null;

        if (!string.IsNullOrEmpty(payload.Game) && payload.Game != Game)
        {
            warning = LOC.Get("GPARAM_Transfer_Warning_Other_Game", payload.Game, Game);
        }

        if (target?.Gparam == null || target.FileEntry == null)
            return Fail(LOC.Get("GPARAM_Transfer_Need_File"), warning);

        string error;
        string description;

        switch (payload.Kind)
        {
            case GparamTransferKind.Gparam:
                description = CheckGparam(payload, target, modes.File);
                return new GparamTransferCheck { Warning = warning, Description = description };

            case GparamTransferKind.Group:
            {
                var keys = payload.GetGroups().Select(e => e.Key).ToList();

                (error, description) = CheckKeyed(payload.Kind, keys, target.Gparam.Params.Select(e => e?.Key).ToList(),
                    target.Group?.Key, modes.Group, target.FileEntry.Filename, LOC.Get("GPARAM_Transfer_Need_Group"));
                break;
            }

            case GparamTransferKind.Field:
            {
                if (target.Group == null)
                    return Fail(LOC.Get("GPARAM_Transfer_Paste_Field_Need_Group"), warning);

                var keys = payload.GetFields().Select(e => e.Key).ToList();

                (error, description) = CheckKeyed(payload.Kind, keys, target.Group.Fields.Select(e => e?.Key).ToList(),
                    target.Field?.Key, modes.Field, target.Group.Key, LOC.Get("GPARAM_Transfer_Need_Field"));
                break;
            }

            case GparamTransferKind.Values:
                (error, description) = CheckValues(payload, target, modes.Values);
                break;

            default:
                return Fail(LOC.Get("GPARAM_Transfer_Parse_Unknown"), warning);
        }

        return error != null
            ? Fail(error, warning)
            : new GparamTransferCheck { Warning = warning, Description = description };
    }

    private static GparamTransferCheck Fail(string error, string warning)
    {
        return new GparamTransferCheck { Error = error, Warning = warning };
    }

    private static string CheckGparam(GparamTransferPayload payload, GparamTransferTarget target, GparamFilePasteMode mode)
    {
        var fileName = target.FileEntry.Filename;

        if (mode is GparamFilePasteMode.ReplaceAll)
            return LOC.Get("GPARAM_Transfer_Describe_Gparam", fileName);

        // Merge: the payload's groups, matched by key
        var keys = (payload.GetGparam()?.Params ?? new List<GPARAM.Param>())
            .Where(e => e != null)
            .Select(e => e.Key)
            .ToList();

        var replaced = keys.Count(k => target.Gparam.Params.Any(e => e != null && e.Key == k));

        return LOC.Get("GPARAM_Transfer_Describe_Keyed",
            GparamTransferPayload.KindName(GparamTransferKind.Group), replaced, keys.Count - replaced, fileName);
    }

    /// <summary>
    /// Groups into a GPARAM, or fields into a group: both are lists matched by key.
    /// </summary>
    /// <param name="keys">The payload's keys.</param>
    /// <param name="existingKeys">The keys already in the target list.</param>
    /// <param name="selectedKey">The selected entry's key, for Replace Selected.</param>
    /// <param name="targetName">The list's owner, for messages.</param>
    /// <param name="needSelection">The error when Replace Selected has nothing selected.</param>
    private static (string error, string description) CheckKeyed(GparamTransferKind kind, List<string> keys,
        List<string> existingKeys, string selectedKey, GparamKeyedPasteMode mode, string targetName, string needSelection)
    {
        var kindName = GparamTransferPayload.KindName(kind);

        if (keys.Distinct().Count() != keys.Count)
            return (LOC.Get("GPARAM_Transfer_Paste_Duplicate_Keys", kindName), null);

        var existing = keys.Count(existingKeys.Contains);

        switch (mode)
        {
            case GparamKeyedPasteMode.MatchKey:
                return (null, LOC.Get("GPARAM_Transfer_Describe_Keyed", kindName, existing, keys.Count - existing, targetName));

            case GparamKeyedPasteMode.AddMissing:
                if (existing == keys.Count)
                    return (LOC.Get("GPARAM_Transfer_Paste_Nothing_Missing", kindName, targetName), null);

                return (null, LOC.Get("GPARAM_Transfer_Describe_Add_Missing", kindName, keys.Count - existing, existing, targetName));

            case GparamKeyedPasteMode.ReplaceSelected:
                if (selectedKey == null)
                    return (needSelection, null);

                if (keys.Count != 1)
                    return (LOC.Get("GPARAM_Transfer_Paste_Replace_Selected_One", kindName, keys.Count), null);

                // The pasted entry takes the selected one's place: its key mustn't already be another entry's
                if (keys[0] != selectedKey && existingKeys.Contains(keys[0]))
                    return (LOC.Get("GPARAM_Transfer_Paste_Replace_Selected_Key_Used", targetName, kindName, keys[0]), null);

                return (null, LOC.Get("GPARAM_Transfer_Describe_Replace_Selected", selectedKey, keys[0], targetName));
        }

        return (LOC.Get("GPARAM_Transfer_Parse_Unknown"), null);
    }

    private static (string error, string description) CheckValues(GparamTransferPayload payload, GparamTransferTarget target, GparamValuePasteMode mode)
    {
        if (target.Field == null)
            return (LOC.Get("GPARAM_Transfer_Paste_Values_Need_Field"), null);

        var field = target.Field;
        var values = payload.GetValues();

        // A value of another type (e.g. a float into an int field) can't go in the field's list
        if (!values.All(field.AcceptsValue))
        {
            return (LOC.Get("GPARAM_Transfer_Paste_Values_Wrong_Type",
                payload.FieldType, field.Key, GparamUtils.GetReadableObjectTypeName(field)), null);
        }

        switch (mode)
        {
            case GparamValuePasteMode.ReplaceSelection:
                if (target.Values.Count != values.Count)
                    return (LOC.Get("GPARAM_Transfer_Paste_Values_Count_Mismatch", values.Count, target.Values.Count), null);

                return (null, LOC.Get("GPARAM_Transfer_Describe_Values_Replace", values.Count, field.Key));

            case GparamValuePasteMode.ReplaceById:
            {
                var ids = values.Select(e => e.ID).ToList();

                if (ids.Distinct().Count() != ids.Count)
                    return (LOC.Get("GPARAM_Transfer_Paste_Values_Duplicate_Ids"), null);

                var replaced = ids.Count(id => field.Values.Any(e => e.ID == id));

                return (null, LOC.Get("GPARAM_Transfer_Describe_Values_By_Id", replaced, ids.Count - replaced, field.Key));
            }

            default:
            {
                var anchor = target.Values.LastOrDefault();

                return (null, anchor == null
                    ? LOC.Get("GPARAM_Transfer_Describe_Values_Append", values.Count, field.Key)
                    : LOC.Get("GPARAM_Transfer_Describe_Values_Insert", values.Count, field.Key, anchor.ID));
            }
        }
    }

    #endregion

    #region Apply

    /// <summary>
    /// Check the payload against the target and, if it fits, apply it as one undoable step. Logs the outcome.
    /// Returns whether anything changed.
    /// </summary>
    public bool Apply(GparamTransferPayload payload, GparamTransferTarget target, GparamPasteModes modes)
    {
        var check = Check(payload, target, modes);

        if (check.Warning != null)
        {
            Smithbox.LogWarning<GparamTransferService>(check.Warning);
        }

        if (!check.CanApply)
        {
            Smithbox.LogError<GparamTransferService>(check.Error);
            return false;
        }

        var action = CreateAction(payload, target, modes);

        if (action == null)
            return false;

        View.ActionManager.ExecuteAction(action);

        Smithbox.Log<GparamTransferService>(check.Description);

        return true;
    }

    /// <summary>
    /// The action that applies the payload. Expects a target that passed <see cref="Check"/>.
    /// Each call makes fresh objects from the payload.
    /// </summary>
    private EditorAction CreateAction(GparamTransferPayload payload, GparamTransferTarget target, GparamPasteModes modes)
    {
        var actions = new List<EditorAction>();

        switch (payload.Kind)
        {
            case GparamTransferKind.Gparam:
            {
                var newData = payload.GetGparam();

                if (modes.File is GparamFilePasteMode.ReplaceAll)
                {
                    // String width follows the game being edited, not the one the payload came from
                    newData.WideStrings = Project.Descriptor.ProjectType is not (ProjectType.DS2 or ProjectType.DS2S);

                    actions.Add(new ImportGparamAction(Project, View, target.FileEntry, target.Gparam, newData));
                }
                else
                {
                    // Merge: each group replaces the one with its key, or is added. The file's own header data stays.
                    foreach (var group in newData.Params ?? new List<GPARAM.Param>())
                    {
                        if (group == null)
                            continue;

                        actions.Add(new ImportParamAction(Project, View, target.FileEntry, target.Gparam, target.Group, group));
                    }
                }
                break;
            }

            case GparamTransferKind.Group:
                foreach (var group in payload.GetGroups())
                {
                    switch (modes.Group)
                    {
                        case GparamKeyedPasteMode.AddMissing:
                            if (target.Gparam.Params.Any(e => e != null && e.Key == group.Key))
                                continue;
                            break;

                        case GparamKeyedPasteMode.ReplaceSelected:
                            actions.Add(new ImportParamAction(Project, View, target.FileEntry, target.Gparam,
                                target.Group, group, replaceTarget: target.Group));
                            continue;
                    }

                    // Replaces the group with its key, or is added
                    actions.Add(new ImportParamAction(Project, View, target.FileEntry, target.Gparam, target.Group, group));
                }
                break;

            case GparamTransferKind.Field:
                foreach (var field in payload.GetFields())
                {
                    switch (modes.Field)
                    {
                        case GparamKeyedPasteMode.AddMissing:
                            if (target.Group.Fields.Any(e => e != null && e.Key == field.Key))
                                continue;
                            break;

                        case GparamKeyedPasteMode.ReplaceSelected:
                            actions.Add(new ImportFieldAction(Project, View, target.FileEntry, target.Gparam,
                                target.Group, target.Field, field, replaceTarget: target.Field));
                            continue;
                    }

                    // Replaces the field with its key in the target group, or is added
                    actions.Add(new ImportFieldAction(Project, View, target.FileEntry, target.Gparam, target.Group, target.Field, field));
                }
                break;

            case GparamTransferKind.Values:
                AddValueActions(actions, payload.GetValues(), target, modes.Values);
                break;
        }

        if (actions.Count == 0)
            return null;

        return actions.Count == 1 ? actions[0] : new CompoundAction(actions);
    }

    private void AddValueActions(List<EditorAction> actions, List<GPARAM.IFieldValue> values, GparamTransferTarget target, GparamValuePasteMode mode)
    {
        var field = target.Field;

        switch (mode)
        {
            case GparamValuePasteMode.ReplaceSelection:
                for (int i = 0; i < values.Count; i++)
                {
                    actions.Add(new InsertFieldValueAction(Project, View, target.FileEntry, target.Gparam,
                        target.Group, field, target.Values[i], values[i], true));
                }
                break;

            case GparamValuePasteMode.ReplaceById:
                // In ID order, so that a value inserted before a later one keeps the order, and a value is always
                // placed before the one it was positioned against is replaced
                foreach (var value in values.OrderBy(e => e.ID))
                {
                    var existing = field.Values.FirstOrDefault(e => e.ID == value.ID);

                    if (existing != null)
                    {
                        actions.Add(new InsertFieldValueAction(Project, View, target.FileEntry, target.Gparam,
                            target.Group, field, existing, value, true));
                    }
                    else
                    {
                        // Before the first value with a higher ID, or at the end
                        var next = field.Values.FirstOrDefault(e => e.ID > value.ID);

                        actions.Add(new InsertFieldValueAction(Project, View, target.FileEntry, target.Gparam,
                            target.Group, field, next, value, false, insertBefore: next != null));
                    }
                }
                break;

            default:
            {
                // Each goes after the one before it, so the payload's order is kept. With no anchor, the first
                // goes at the end of the list.
                GPARAM.IFieldValue anchor = target.Values.LastOrDefault();

                foreach (var value in values)
                {
                    actions.Add(new InsertFieldValueAction(Project, View, target.FileEntry, target.Gparam,
                        target.Group, field, anchor, value, false));

                    anchor = value;
                }
                break;
            }
        }
    }

    #endregion

    #region Files

    /// <summary>
    /// Read a payload from a JSON file. Returns null, with the error logged, if it can't be read.
    /// </summary>
    public GparamTransferPayload ReadFile(string path)
    {
        string json;

        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Smithbox.LogError<GparamTransferService>(LOC.Get("GPARAM_Transfer_File_Read_Failed", path), e);
            return null;
        }

        var payload = GparamTransferPayload.Parse(json, out var error);

        if (payload == null)
        {
            Smithbox.LogError<GparamTransferService>(LOC.Get("GPARAM_Transfer_File_Not_Payload", path, error));
        }

        return payload;
    }

    /// <summary>
    /// Write a payload to a JSON file. Returns whether it was written; a failure is logged.
    /// </summary>
    public bool WriteFile(GparamTransferPayload payload, string path)
    {
        try
        {
            File.WriteAllText(path, payload.ToJson());
        }
        catch (Exception e)
        {
            // e.g. a name with characters a file name can't hold, or a folder that has gone
            Smithbox.LogError<GparamTransferService>(LOC.Get("GPARAM_Transfer_File_Write_Failed", path), e);
            return false;
        }

        Smithbox.Log<GparamTransferService>(LOC.Get("GPARAM_Transfer_File_Written",
            GparamTransferPayload.KindName(payload.Kind), path));

        return true;
    }

    #endregion
}
