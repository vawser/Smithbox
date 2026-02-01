using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ParamExportPreviewModal
{
    private ParamDeltaPatcher Patcher;

    public string ModalName = "Export Preview";

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public ParamDeltaPatch PatchForExport = null;

    private Dictionary<string, bool> ParamEnabled = new();
    private Dictionary<(string param, int rowId), bool> RowEnabled = new();
    private Dictionary<(string param, int rowId, string field), bool> FieldEnabled = new();

    public ParamExportPreviewModal(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public void Show(ParamDeltaPatch exportPatch)
    {
        PatchForExport = exportPatch;

        ParamEnabled.Clear();
        RowEnabled.Clear();
        FieldEnabled.Clear();

        // Default everything to enabled
        foreach (var param in PatchForExport.Params)
        {
            ParamEnabled[param.Name] = true;

            foreach (var row in param.Rows)
            {
                RowEnabled[(param.Name, row.ID)] = true;

                foreach (var field in row.Fields)
                {
                    FieldEnabled[(param.Name, row.ID, field.Field)] = true;
                }
            }
        }

        DisplayModal = true;
        InitialLayout = false;
    }

    public void Hide()
    {
        PatchForExport = null;
        DisplayModal = false;
    }

    public void Draw()
    {
        if (!DisplayModal)
            return;

        var popupName = $"{ModalName}##{ModalName.GetHashCode()}";

        ImGui.OpenPopup(popupName);

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(popupName,
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            DrawHeader();

            ImGui.Separator();

            DrawPatchContents();

            ImGui.Separator();

            DrawFooterButtons();

            ImGui.EndPopup();
        }
    }

    private void DrawHeader()
    {
        ImGui.Text($"Project Type: {PatchForExport.ProjectType}");
        ImGui.Text($"Param Version: {PatchForExport.ParamVersion}");
        ImGui.Text($"Params Changed: {PatchForExport.Params.Count}");

        ImGui.Spacing();

        if (ImGui.Button("Enable All"))
        {
            SetAllToggles(true);
        }

        ImGui.SameLine();

        if (ImGui.Button("Disable All"))
        {
            SetAllToggles(false);
        }

        ImGui.Spacing();
    }

    private void SetAllToggles(bool enabled)
    {
        // Params
        foreach (var key in ParamEnabled.Keys.ToList())
        {
            ParamEnabled[key] = enabled;
        }

        // Rows
        foreach (var key in RowEnabled.Keys.ToList())
        {
            RowEnabled[key] = enabled;
        }

        // Fields
        foreach (var key in FieldEnabled.Keys.ToList())
        {
            FieldEnabled[key] = enabled;
        }
    }

    private void DrawPatchContents()
    {
        ImGui.BeginChild("##preview_scroll", new System.Numerics.Vector2(600, 400));

        foreach (var param in PatchForExport.Params)
        {
            bool enabled = ParamEnabled[param.Name];

            // Parent checkbox
            if (ImGui.Checkbox($"##param_toggle_{param.Name}", ref enabled))
            {
                ParamEnabled[param.Name] = enabled;

                // Apply to all rows + fields
                SetParamChildren(param.Name, enabled);
            }
            UIHelper.Tooltip("Determines if this element is included in the delta export.");

            ImGui.SameLine();

            // Mixed indicator
            if (IsParamMixed(param.Name))
            {
                ImGui.TextColored(new System.Numerics.Vector4(1f, 0.8f, 0.3f, 1f), "(Mixed)");
                ImGui.SameLine();
            }

            if (ImGui.TreeNode($"{param.Name}##param"))
            {
                foreach (var row in param.Rows)
                    DrawRow(param.Name, row);

                ImGui.TreePop();
            }
        }

        ImGui.EndChild();
    }

    private void SetParamChildren(string paramName, bool enabled)
    {
        var param = PatchForExport.Params.First(p => p.Name == paramName);

        foreach (var row in param.Rows)
        {
            RowEnabled[(paramName, row.ID)] = enabled;

            foreach (var field in row.Fields)
            {
                FieldEnabled[(paramName, row.ID, field.Field)] = enabled;
            }
        }
    }

    private void DrawRow(string paramName, RowDelta row)
    {
        var rowKey = (paramName, row.ID);
        bool rowEnabled = RowEnabled[rowKey];

        if (ImGui.Checkbox($"##row_toggle_{paramName}_{row.ID}", ref rowEnabled))
        {
            RowEnabled[rowKey] = rowEnabled;

            // Apply to all fields
            SetRowChildren(paramName, row, rowEnabled);

            // Update parent param checkbox
            UpdateParamFromRows(paramName);
        }
        UIHelper.Tooltip("Determines if this element is included in the delta export.");

        ImGui.SameLine();

        if (IsRowMixed(paramName, row))
        {
            ImGui.TextColored(new System.Numerics.Vector4(1f, 0.8f, 0.3f, 1f), "(Mixed)");
            ImGui.SameLine();
        }

        string stateLabel = row.State switch
        {
            RowDeltaState.Added => "[Added]",
            RowDeltaState.Modified => "[Modified]",
            RowDeltaState.Deleted => "[Deleted]",
            _ => "[Unknown]"
        };

        if (ImGui.TreeNode($"{stateLabel} ID={row.ID} ({row.Name})##row"))
        {
            foreach (var field in row.Fields)
                DrawField(paramName, row.ID, field);

            ImGui.TreePop();
        }
    }
    private void SetRowChildren(string paramName, RowDelta row, bool enabled)
    {
        foreach (var field in row.Fields)
        {
            FieldEnabled[(paramName, row.ID, field.Field)] = enabled;
        }
    }

    private void DrawField(string paramName, int rowId, FieldDelta field)
    {
        var fieldKey = (paramName, rowId, field.Field);
        bool fieldEnabled = FieldEnabled[fieldKey];

        if (ImGui.Checkbox($"##field_toggle_{paramName}_{rowId}_{field.Field}", ref fieldEnabled))
        {
            FieldEnabled[fieldKey] = fieldEnabled;

            // Update row state based on fields
            UpdateRowFromFields(paramName, rowId);

            // Update param state based on rows
            UpdateParamFromRows(paramName);
        }
        UIHelper.Tooltip("Determines if this element is included in the delta export.");

        ImGui.SameLine();
        ImGui.Text($"{field.Field}: {field.Value}");
    }

    private void DrawFooterButtons()
    {
        if (ImGui.Button("Export", DPI.StandardButtonSize))
        {
            var filteredPatch = BuildFilteredPatch();

            Patcher.WriteDeltaPatch(filteredPatch, Patcher.Selection.ExportName);

            TaskLogs.AddLog($"Saved param delta: {Patcher.Selection.ExportName}.json");

            Patcher.Selection.QueueImportListRefresh = true;

            Hide();
        }

        ImGui.SameLine();

        if (ImGui.Button("Cancel", DPI.StandardButtonSize))
        {
            Hide();
        }
    }

    private void UpdateRowFromFields(string paramName, int rowId)
    {
        var param = PatchForExport.Params.First(p => p.Name == paramName);
        var row = param.Rows.First(r => r.ID == rowId);

        bool anyEnabled = row.Fields.Any(f =>
            FieldEnabled[(paramName, rowId, f.Field)]);

        RowEnabled[(paramName, rowId)] = anyEnabled;
    }

    private void UpdateParamFromRows(string paramName)
    {
        var param = PatchForExport.Params.First(p => p.Name == paramName);

        bool anyEnabled = param.Rows.Any(r =>
            RowEnabled[(paramName, r.ID)]);

        ParamEnabled[paramName] = anyEnabled;
    }

    private bool IsParamMixed(string paramName)
    {
        var rows = PatchForExport.Params
            .First(p => p.Name == paramName)
            .Rows;

        bool anyOn = rows.Any(r => RowEnabled[(paramName, r.ID)]);
        bool anyOff = rows.Any(r => !RowEnabled[(paramName, r.ID)]);

        return anyOn && anyOff;
    }

    private bool IsRowMixed(string paramName, RowDelta row)
    {
        bool anyOn = row.Fields.Any(f =>
            FieldEnabled[(paramName, row.ID, f.Field)]);

        bool anyOff = row.Fields.Any(f =>
            !FieldEnabled[(paramName, row.ID, f.Field)]);

        return anyOn && anyOff;
    }

    private ParamDeltaPatch BuildFilteredPatch()
    {
        var result = new ParamDeltaPatch
        {
            ProjectType = PatchForExport.ProjectType,
            ParamVersion = PatchForExport.ParamVersion,
            Tag = PatchForExport.Tag,
        };

        foreach (var param in PatchForExport.Params)
        {
            if (!ParamEnabled[param.Name])
                continue;

            var newParam = new ParamDelta
            {
                Name = param.Name
            };

            foreach (var row in param.Rows)
            {
                var rowKey = (param.Name, row.ID);

                if (!RowEnabled[rowKey])
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
                    var fieldKey = (param.Name, row.ID, field.Field);

                    if (!FieldEnabled[fieldKey])
                        continue;

                    newRow.Fields.Add(new FieldDelta
                    {
                        Field = field.Field,
                        Value = field.Value
                    });
                }

                if (newRow.Fields.Count > 0)
                {
                    newParam.Rows.Add(newRow);
                }
            }

            if (newParam.Rows.Count > 0)
            {
                result.Params.Add(newParam);
            }
        }

        return result;
    }
}
