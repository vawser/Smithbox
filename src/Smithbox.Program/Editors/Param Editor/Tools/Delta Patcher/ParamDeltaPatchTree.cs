using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The include/exclude tree of a delta patch, shared by the export and import previews.
/// Rows are keyed by (ID, Index), so rows sharing an ID have their own toggles. Row table groups have one
/// toggle each and no field toggles, since a group is applied as a whole.
/// </summary>
public class ParamDeltaPatchTree
{
    public ParamDeltaPatch Patch { get; private set; }

    private readonly Dictionary<string, bool> ParamEnabled = new();
    private readonly Dictionary<(string param, int id, int index), bool> RowEnabled = new();
    private readonly Dictionary<(string param, int id, int index, string field), bool> FieldEnabled = new();
    private readonly Dictionary<(string param, int id), bool> GroupEnabled = new();

    public void Reset(ParamDeltaPatch patch)
    {
        Patch = patch;

        ParamEnabled.Clear();
        RowEnabled.Clear();
        FieldEnabled.Clear();
        GroupEnabled.Clear();

        if (patch == null)
            return;

        foreach (var param in patch.Params)
        {
            ParamEnabled[param.Name] = true;

            foreach (var row in param.Rows)
            {
                RowEnabled[RowKey(param.Name, row)] = true;

                foreach (var field in row.Fields)
                {
                    FieldEnabled[FieldKey(param.Name, row, field)] = true;
                }
            }

            foreach (var group in param.Groups)
            {
                GroupEnabled[(param.Name, group.ID)] = true;
            }
        }
    }

    private static (string, int, int) RowKey(string paramName, RowDelta row) => (paramName, row.ID, row.Index);

    private static (string, int, int, string) FieldKey(string paramName, RowDelta row, FieldDelta field)
        => (paramName, row.ID, row.Index, field.Field);

    public void SetAll(bool enabled)
    {
        foreach (var key in ParamEnabled.Keys.ToList())
            ParamEnabled[key] = enabled;

        foreach (var key in RowEnabled.Keys.ToList())
            RowEnabled[key] = enabled;

        foreach (var key in FieldEnabled.Keys.ToList())
            FieldEnabled[key] = enabled;

        foreach (var key in GroupEnabled.Keys.ToList())
            GroupEnabled[key] = enabled;
    }

    #region Draw
    public void Draw(string toggleTooltip)
    {
        foreach (var param in Patch.Params)
        {
            bool enabled = ParamEnabled[param.Name];

            if (ImGui.Checkbox($"##param_toggle_{param.Name}", ref enabled))
            {
                ParamEnabled[param.Name] = enabled;
                SetParamChildren(param, enabled);
            }
            if (toggleTooltip != null)
                GUI.Tooltip(toggleTooltip);

            ImGui.SameLine();

            if (IsParamMixed(param))
            {
                ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f), LOC.Get("PARAM_DeltaPatcher_Preview_Row_State_Mixed"));
                ImGui.SameLine();
            }

            if (ImGui.TreeNode($"{param.Name}##param"))
            {
                foreach (var group in param.Groups)
                {
                    DrawGroup(param, group);
                }

                foreach (var row in param.Rows)
                {
                    DrawRow(param, row, toggleTooltip);
                }

                ImGui.TreePop();
            }
        }
    }

    private static string GetStateLabel(RowDeltaState state)
    {
        return state switch
        {
            RowDeltaState.Added => LOC.Get("PARAM_DeltaPatcher_Preview_Row_State_Added"),
            RowDeltaState.Modified => LOC.Get("PARAM_DeltaPatcher_Preview_Row_State_Modified"),
            RowDeltaState.Deleted => LOC.Get("PARAM_DeltaPatcher_Preview_Row_State_Deleted"),
            _ => LOC.Get("PARAM_DeltaPatcher_Preview_Row_State_Unknown")
        };
    }

    private void DrawGroup(ParamDelta param, RowGroupDelta group)
    {
        var key = (param.Name, group.ID);
        bool enabled = GroupEnabled[key];

        if (ImGui.Checkbox($"##group_toggle_{param.Name}_{group.ID}", ref enabled))
        {
            GroupEnabled[key] = enabled;
            UpdateParamFromChildren(param);
        }
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Preview_Group_Toggle_TT"));

        ImGui.SameLine();

        var displayName = LOC.Get("PARAM_DeltaPatcher_Preview_Group_Display_Name",
            GetStateLabel(group.State), group.ID, group.Rows.Count);

        if (ImGui.TreeNode($"{displayName}##groupnode_{group.ID}"))
        {
            for (var i = 0; i < group.Rows.Count; i++)
            {
                var row = group.Rows[i];

                if (ImGui.TreeNode($"{LOC.Get("PARAM_DeltaPatcher_Preview_Group_Row_Display_Name", i, row.Name)}##grouprow_{group.ID}_{i}"))
                {
                    foreach (var field in row.Fields)
                    {
                        ImGui.Text($"{field.Field}: {field.Value}");
                    }

                    ImGui.TreePop();
                }
            }

            ImGui.TreePop();
        }
    }

    private void DrawRow(ParamDelta param, RowDelta row, string toggleTooltip)
    {
        var rowKey = RowKey(param.Name, row);
        bool rowEnabled = RowEnabled[rowKey];

        if (ImGui.Checkbox($"##row_toggle_{param.Name}_{row.ID}_{row.Index}", ref rowEnabled))
        {
            RowEnabled[rowKey] = rowEnabled;

            foreach (var field in row.Fields)
            {
                FieldEnabled[FieldKey(param.Name, row, field)] = rowEnabled;
            }

            UpdateParamFromChildren(param);
        }
        if (toggleTooltip != null)
            GUI.Tooltip(toggleTooltip);

        ImGui.SameLine();

        if (IsRowMixed(param.Name, row))
        {
            ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f), LOC.Get("PARAM_DeltaPatcher_Preview_Row_State_Mixed"));
            ImGui.SameLine();
        }

        var displayName = LOC.Get("PARAM_DeltaPatcher_Preview_Row_Display_Name", GetStateLabel(row.State), row.ID, row.Name);

        if (ImGui.TreeNode($"{displayName}##rownode_{row.ID}_{row.Index}"))
        {
            foreach (var field in row.Fields)
            {
                DrawField(param, row, field, toggleTooltip);
            }

            ImGui.TreePop();
        }
    }

    private void DrawField(ParamDelta param, RowDelta row, FieldDelta field, string toggleTooltip)
    {
        var fieldKey = FieldKey(param.Name, row, field);
        bool fieldEnabled = FieldEnabled[fieldKey];

        if (ImGui.Checkbox($"##field_toggle_{param.Name}_{row.ID}_{row.Index}_{field.Field}", ref fieldEnabled))
        {
            FieldEnabled[fieldKey] = fieldEnabled;

            RowEnabled[RowKey(param.Name, row)] = row.Fields.Any(f => FieldEnabled[FieldKey(param.Name, row, f)]);

            UpdateParamFromChildren(param);
        }
        if (toggleTooltip != null)
            GUI.Tooltip(toggleTooltip);

        ImGui.SameLine();
        ImGui.Text($"{field.Field}: {field.Value}");
    }
    #endregion

    #region State
    private void SetParamChildren(ParamDelta param, bool enabled)
    {
        foreach (var row in param.Rows)
        {
            RowEnabled[RowKey(param.Name, row)] = enabled;

            foreach (var field in row.Fields)
            {
                FieldEnabled[FieldKey(param.Name, row, field)] = enabled;
            }
        }

        foreach (var group in param.Groups)
        {
            GroupEnabled[(param.Name, group.ID)] = enabled;
        }
    }

    private void UpdateParamFromChildren(ParamDelta param)
    {
        ParamEnabled[param.Name] =
            param.Rows.Any(r => RowEnabled[RowKey(param.Name, r)]) ||
            param.Groups.Any(g => GroupEnabled[(param.Name, g.ID)]);
    }

    private bool IsParamMixed(ParamDelta param)
    {
        var states = param.Rows.Select(r => RowEnabled[RowKey(param.Name, r)])
            .Concat(param.Groups.Select(g => GroupEnabled[(param.Name, g.ID)]))
            .ToList();

        return states.Any(e => e) && states.Any(e => !e);
    }

    private bool IsRowMixed(string paramName, RowDelta row)
    {
        bool anyOn = row.Fields.Any(f => FieldEnabled[FieldKey(paramName, row, f)]);
        bool anyOff = row.Fields.Any(f => !FieldEnabled[FieldKey(paramName, row, f)]);

        return anyOn && anyOff;
    }
    #endregion

    /// <summary>
    /// The patch with only the enabled params, groups, rows and fields. A deleted row has no fields, so it is
    /// kept by its own toggle.
    /// </summary>
    public ParamDeltaPatch BuildFilteredPatch()
    {
        var result = new ParamDeltaPatch
        {
            ProjectType = Patch.ProjectType,
            ParamVersion = Patch.ParamVersion,
            Tag = Patch.Tag,
        };

        foreach (var param in Patch.Params)
        {
            if (!ParamEnabled[param.Name])
                continue;

            var newParam = new ParamDelta { Name = param.Name };

            foreach (var group in param.Groups)
            {
                if (GroupEnabled[(param.Name, group.ID)])
                {
                    newParam.Groups.Add(ParamRowGroups.CloneGroup(group));
                }
            }

            foreach (var row in param.Rows)
            {
                if (!RowEnabled[RowKey(param.Name, row)])
                    continue;

                var newRow = new RowDelta
                {
                    ID = row.ID,
                    Index = row.Index,
                    Name = row.Name,
                    State = row.State
                };

                foreach (var field in row.Fields)
                {
                    if (!FieldEnabled[FieldKey(param.Name, row, field)])
                        continue;

                    newRow.Fields.Add(new FieldDelta
                    {
                        Field = field.Field,
                        Value = field.Value
                    });
                }

                if (newRow.Fields.Count > 0 || newRow.State == RowDeltaState.Deleted)
                {
                    newParam.Rows.Add(newRow);
                }
            }

            if (newParam.Rows.Count > 0 || newParam.Groups.Count > 0)
            {
                result.Params.Add(newParam);
            }
        }

        return result;
    }
}
