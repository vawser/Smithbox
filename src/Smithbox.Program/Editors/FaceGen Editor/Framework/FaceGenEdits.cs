#nullable enable
using Andre.Formats;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// Writes to face rows through the Param Editor's action manager, so an edit made here is undone in either editor,
/// marks the row as modified, and is saved with the params.
/// </summary>
public static class FaceGenEdits
{
    private static readonly PropertyInfo CellValue = typeof(Param.Cell).GetProperty("Value")!;

    /// <summary>
    /// Sets fields of a row in one undo step. Values are converted to each field's type; unchanged fields are left
    /// out, and nothing is done if none change. False if nothing changed.
    /// </summary>
    public static bool SetFields(ParamEditorScreen editor, string paramName, Param.Row row,
        IReadOnlyList<(string Field, object Value)> values, string displayName)
    {
        var actions = new List<EditorAction>();

        foreach (var (field, value) in values)
        {
            if (row[field] is not { } cell)
                continue;

            var current = cell.Value;
            var converted = Convert.ChangeType(value, current.GetType());

            if (Equals(current, converted))
                continue;

            actions.Add(new PropertiesChangedAction(CellValue, cell, converted) { DisplayName = displayName });
        }

        if (actions.Count == 0)
            return false;

        EditorAction action = actions.Count == 1 ? actions[0] : new CompoundAction(actions) { DisplayName = displayName };

        editor.ActionManager.ExecuteAction(action);
        editor.Project.Handler.ParamData.PrimaryBank.RefreshParamRowDiffs(editor, row, paramName);
        return true;
    }

    public static bool SetField(ParamEditorScreen editor, string paramName, Param.Row row, string field, object value,
        string displayName) =>
        SetFields(editor, paramName, row, new[] { (field, value) }, displayName);
}
