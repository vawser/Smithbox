using Hexa.NET.ImGui;
using HKLib.hk2018.hkSerialize.Note;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamImportPreviewModal
{
    private ParamDeltaPatcher Patcher;

    public string ModalName = "Import Preview";

    public bool DisplayModal = false;
    public bool InitialLayout = false;

    public string Filename = "";
    public ParamDeltaPatch PatchForImport = null;

    // Selection State
    private Dictionary<string, bool> ParamEnabled = new();
    private Dictionary<(string param, int rowId), bool> RowEnabled = new();
    private Dictionary<(string param, int rowId, string field), bool> FieldEnabled = new();

    public ParamImportPreviewModal(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public void Show(string filename, ParamDeltaPatch patch)
    {
        Filename = filename;
        PatchForImport = patch;

        ParamEnabled.Clear();
        RowEnabled.Clear();
        FieldEnabled.Clear();

        foreach (var param in PatchForImport.Params)
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
        PatchForImport = null;
        DisplayModal = false;
    }

    public void Draw()
    {
        if (!DisplayModal || PatchForImport == null)
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
        ImGui.Text($"Project Type: {PatchForImport.ProjectType}");
        ImGui.Text($"Param Version: {PatchForImport.ParamVersion}");
        ImGui.Text($"Params Changed: {PatchForImport.Params.Count}");

        ImGui.Spacing();

        if (ImGui.Button("Enable All"))
            SetAllToggles(true);

        ImGui.SameLine();

        if (ImGui.Button("Disable All"))
            SetAllToggles(false);

        ImGui.Spacing();
    }

    private void DrawPatchContents()
    {
        ImGui.BeginChild("##import_preview_scroll", new Vector2(600, 400));

        foreach (var param in PatchForImport.Params)
        {
            bool enabled = ParamEnabled[param.Name];

            if (ImGui.Checkbox($"##param_{param.Name}", ref enabled))
            {
                ParamEnabled[param.Name] = enabled;
                SetParamChildren(param.Name, enabled);
            }

            ImGui.SameLine();

            if (IsParamMixed(param.Name))
            {
                ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f), "(Mixed)");
                ImGui.SameLine();
            }

            if (ImGui.TreeNode($"{param.Name}##paramnode"))
            {
                foreach (var row in param.Rows)
                    DrawRow(param.Name, row);

                ImGui.TreePop();
            }
        }

        ImGui.EndChild();
    }

    private void DrawRow(string paramName, RowDelta row)
    {
        var rowKey = (paramName, row.ID);
        bool rowEnabled = RowEnabled[rowKey];

        if (ImGui.Checkbox($"##row_{paramName}_{row.ID}", ref rowEnabled))
        {
            RowEnabled[rowKey] = rowEnabled;

            SetRowChildren(paramName, row, rowEnabled);
            UpdateParamFromRows(paramName);
        }

        ImGui.SameLine();

        if (IsRowMixed(paramName, row))
        {
            ImGui.TextColored(new Vector4(1f, 0.8f, 0.3f, 1f), "(Mixed)");
            ImGui.SameLine();
        }

        string stateLabel = row.State switch
        {
            RowDeltaState.Added => "[Added]",
            RowDeltaState.Modified => "[Modified]",
            RowDeltaState.Deleted => "[Deleted]",
            _ => "[Unknown]"
        };

        if (ImGui.TreeNode($"{stateLabel} ID={row.ID} ({row.Name})##rownode"))
        {
            foreach (var field in row.Fields)
                DrawField(paramName, row.ID, field);

            ImGui.TreePop();
        }
    }

    private void DrawField(string paramName, int rowId, FieldDelta field)
    {
        var fieldKey = (paramName, rowId, field.Field);
        bool fieldEnabled = FieldEnabled[fieldKey];

        if (ImGui.Checkbox($"##field_{paramName}_{rowId}_{field.Field}", ref fieldEnabled))
        {
            FieldEnabled[fieldKey] = fieldEnabled;

            UpdateRowFromFields(paramName, rowId);
            UpdateParamFromRows(paramName);
        }

        ImGui.SameLine();
        ImGui.Text($"{field.Field}: {field.Value}");
    }

    private void DrawFooterButtons()
    {
        if (ImGui.Button("Import", new Vector2(120, 0)))
        {
            var filtered = BuildFilteredPatch();

            Patcher.Importer.ImportDelta(Filename, filtered);

            Smithbox.Log(this, "Imported selected param delta patch.");

            Hide();
        }

        ImGui.SameLine();

        if (ImGui.Button("Cancel", new Vector2(120, 0)))
        {
            Hide();
        }
    }

    private void SetAllToggles(bool enabled)
    {
        foreach (var key in ParamEnabled.Keys.ToList())
            ParamEnabled[key] = enabled;

        foreach (var key in RowEnabled.Keys.ToList())
            RowEnabled[key] = enabled;

        foreach (var key in FieldEnabled.Keys.ToList())
            FieldEnabled[key] = enabled;
    }

    private void SetParamChildren(string paramName, bool enabled)
    {
        var param = PatchForImport.Params.First(p => p.Name == paramName);

        foreach (var row in param.Rows)
        {
            RowEnabled[(paramName, row.ID)] = enabled;

            foreach (var field in row.Fields)
                FieldEnabled[(paramName, row.ID, field.Field)] = enabled;
        }
    }

    private void SetRowChildren(string paramName, RowDelta row, bool enabled)
    {
        foreach (var field in row.Fields)
            FieldEnabled[(paramName, row.ID, field.Field)] = enabled;
    }

    private void UpdateRowFromFields(string paramName, int rowId)
    {
        var param = PatchForImport.Params.First(p => p.Name == paramName);
        var row = param.Rows.First(r => r.ID == rowId);

        bool anyEnabled = row.Fields.Any(f =>
            FieldEnabled[(paramName, rowId, f.Field)]);

        RowEnabled[(paramName, rowId)] = anyEnabled;
    }

    private void UpdateParamFromRows(string paramName)
    {
        var param = PatchForImport.Params.First(p => p.Name == paramName);

        bool anyEnabled = param.Rows.Any(r =>
            RowEnabled[(paramName, r.ID)]);

        ParamEnabled[paramName] = anyEnabled;
    }

    private bool IsParamMixed(string paramName)
    {
        var param = PatchForImport.Params.First(p => p.Name == paramName);

        bool anyOn = param.Rows.Any(r => RowEnabled[(paramName, r.ID)]);
        bool anyOff = param.Rows.Any(r => !RowEnabled[(paramName, r.ID)]);

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
            ProjectType = PatchForImport.ProjectType,
            ParamVersion = PatchForImport.ParamVersion,
            Tag = PatchForImport.Tag
        };

        foreach (var param in PatchForImport.Params)
        {
            if (!ParamEnabled[param.Name])
                continue;

            var newParam = new ParamDelta { Name = param.Name };

            foreach (var row in param.Rows)
            {
                if (!RowEnabled[(param.Name, row.ID)])
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
                    if (!FieldEnabled[(param.Name, row.ID, field.Field)])
                        continue;

                    newRow.Fields.Add(new FieldDelta
                    {
                        Field = field.Field,
                        Value = field.Value
                    });
                }

                if (newRow.Fields.Count > 0)
                    newParam.Rows.Add(newRow);
            }

            if (newParam.Rows.Count > 0)
                result.Params.Add(newParam);
        }

        return result;
    }
}