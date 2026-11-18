#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The active face, as groups of sliders (the FaceGen controls and amounts, 0 to 255), colours (each R/G/B triplet as
/// one colour) and fields (part IDs and flags, edited as the Param Editor edits them).
/// <para>
/// A slider or colour only writes its row when it is let go, as one undo step; while it is dragged the window shows
/// the value being dragged. Shape and texture fields are named by the game's FaceGen controls (si.ctl) when the
/// param's annotations have no better name.
/// </para>
/// </summary>
public class FacePropertiesView
{
    private static readonly PropertyInfo CellValue = typeof(Param.Cell).GetProperty("Value")!;

    private readonly FaceGenEditorView View;

    private string FieldFilter = "";
    private bool ExactFieldFilter = false;
    private string NameBuffer = "";
    private Param.Row? NameBufferRow;
    private bool NameActive;

    // The groups of the shown param, made again when the param changes (a reload makes a new one)
    private List<FaceFieldGroup> Groups = new();
    private Param? GroupsFor;

    // The value being dragged, shown instead of the row's until the widget is let go
    private string? EditingKey;
    private int EditingValue;
    private Vector3 EditingColor;

    // The Param Editor's field decorators and inputs, for the fields that aren't sliders or colours
    private ParamEditorView? FieldView;
    private ParamFieldDecorators? Decorators;
    private ParamFieldInput? FieldInput;

    public FacePropertiesView(FaceGenEditorView view)
    {
        View = view;
    }

    private ProjectEntry Project => View.Project;

    public void Display()
    {
        var param = View.ActiveParam;
        var row = View.ActiveRow;
        var paramName = View.ParamName;
        var editor = View.Editor.ParamEditor;

        if (param == null || row == null || paramName == null)
        {
            GUI.WrappedText(LOC.Get("FACEGEN_Properties_No_Face"));
            return;
        }

        if (editor == null || !UpdateFieldView())
        {
            GUI.WrappedText(LOC.Get("FACEGEN_Properties_No_Param_Editor"));
            return;
        }

        if (GroupsFor != param)
        {
            Groups = FaceGenLayout.Build(Project, param);
            GroupsFor = param;
        }

        DisplayHeader(editor, paramName, param, row);

        EditorFilters.DisplayFramedListFilter("faceGenFieldFilter", ref FieldFilter, ref ExactFieldFilter);

        if (ImGui.BeginChild("faceGenProperties"))
        {
            var storesModes = FaceGenParams.StoresModes(param);
            var model = FaceGenModelData.Get(Project);
            var filter = FieldFilter.Trim();

            for (var g = 0; g < Groups.Count; g++)
            {
                var group = Groups[g];
                var fields = group.Fields
                    .Where(f => filter.Length == 0 || FieldMatches(param, f, storesModes, model, filter))
                    .ToList();

                if (fields.Count == 0)
                    continue;

                if (!ImGui.CollapsingHeader($"{group.Title}##faceGenGroup{g}", ImGuiTreeNodeFlags.DefaultOpen))
                    continue;

                DisplayGroup(editor, paramName, param, row, fields, storesModes, model, g);
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Points the decorators and inputs at the Param Editor's active view, making new ones when it changes.
    /// False if there is no Param Editor view to edit through.
    /// </summary>
    private bool UpdateFieldView()
    {
        var paramEditor = View.Editor.ParamEditor;
        var activeView = paramEditor?.ViewHandler?.ActiveView;

        if (paramEditor == null || activeView == null)
            return false;

        if (FieldView != activeView)
        {
            FieldView = activeView;
            Decorators = new ParamFieldDecorators(paramEditor, Project, activeView);
            FieldInput = new ParamFieldInput(paramEditor, Project, activeView);
        }

        return true;
    }

    // ---- header: the face's ID and name, and what kind of face param it is

    private void DisplayHeader(ParamEditorScreen editor, string paramName, Param param, Param.Row row)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        ImGui.Selectable($"{paramName}: {FaceGenParams.RowLabel(row.ID, row.Name)}##faceGenRowLabel");
        ImGui.PopStyleColor();
        GUI.Tooltip(LOC.Get("FACEGEN_Properties_Row_TT"));

        if (ImGui.BeginPopupContextItem("faceGenRowLabelContext"))
        {
            if (ImGui.Selectable($"{LOC.Get("FACEGEN_Open_In_Param_Editor")}##openInParamEditor"))
                EditorCommandQueue.AddCommand($"param/select/-1/{paramName}/{row.ID}");

            ImGui.EndPopup();
        }

        // The name, applied when the field is left. Kept in step with the row (undo, a rename in the list) while the
        // field isn't being typed in.
        if (NameBufferRow != row || !NameActive)
        {
            NameBuffer = row.Name ?? "";
            NameBufferRow = row;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##faceGenRowName", LOC.Get("FACEGEN_Properties_Name_Hint"), ref NameBuffer, 1024);
        NameActive = ImGui.IsItemActive();

        if (GUI.InputCommitted() && NameBuffer != (row.Name ?? ""))
            ParamRowOperations.SetRowName(editor, new[] { row }, NameBuffer);

        ImGui.TextDisabled(FaceGenParams.StoresModes(param)
            ? LOC.Get("FACEGEN_Properties_Stores_Modes")
            : LOC.Get("FACEGEN_Properties_Stores_Controls"));
        GUI.Tooltip(LOC.Get("FACEGEN_Properties_Stores_TT"));
    }

    // ---- names

    /// <summary>
    /// The name a field is shown with: its annotation's name, unless that is only a placeholder; else the FaceGen
    /// control's label for a shape or texture field; else the internal name
    /// </summary>
    private string DisplayName(Param param, string internalName, bool storesModes, FaceGenModelData model)
    {
        if (CFG.Current.FaceGenEditor_Show_Internal_Names)
            return internalName;

        var annotation = Project.Handler.ParamData.GetFieldAnnotation(
            Project.Handler.ParamData.GetParamAnnotations(param.ParamType), internalName);

        var annotated = annotation?.Name;

        // Placeholders such as "Face - Geometry Data [19]" say no more than the internal name
        if (!string.IsNullOrWhiteSpace(annotated) && !annotated.Contains("Data [", StringComparison.Ordinal))
            return annotated;

        return FaceGenLabel(internalName, storesModes, model) ?? internalName;
    }

    /// <summary>What a shape or texture field is in the FaceGen model: a control's label, or a mode's number</summary>
    private static string? FaceGenLabel(string internalName, bool storesModes, FaceGenModelData model)
    {
        if (FaceGenParams.ParseFaceGenField(internalName) is not { } faceGen)
            return null;

        if (storesModes)
        {
            return faceGen.IsGeometry
                ? LOC.Get("FACEGEN_Field_Shape_Mode", faceGen.Index)
                : LOC.Get("FACEGEN_Field_Texture_Mode", faceGen.Index);
        }

        return faceGen.IsGeometry ? model.GeometryLabel(faceGen.Index) : model.TextureLabel(faceGen.Index);
    }

    private bool FieldMatches(Param param, FaceField field, bool storesModes, FaceGenModelData model, string filter)
    {
        return EditorFilters.IsMatch(filter, field.Name, ExactFieldFilter,
            DisplayName(param, field.Name, storesModes, model),
            rawSecondaryText: FaceGenLabel(field.Name, storesModes, model) ?? "");
    }

    private void NameTooltip(Param param, FaceField field, bool storesModes, FaceGenModelData model)
    {
        if (!ImGui.IsItemHovered())
            return;

        var lines = new List<string> { field.Kind == FaceFieldKind.Color ? $"{field.Name}, {field.Green}, {field.Blue}" : field.Name };

        if (FaceGenLabel(field.Name, storesModes, model) is { } label)
            lines.Add(LOC.Get("FACEGEN_Field_FaceGen_Label", label));

        var annotation = Project.Handler.ParamData.GetFieldAnnotation(
            Project.Handler.ParamData.GetParamAnnotations(param.ParamType), field.Name);

        if (!string.IsNullOrWhiteSpace(annotation?.Description))
            lines.Add(annotation.Description);

        GUI.Tooltip(string.Join("\n", lines));
    }

    // ---- a group: a table of name and widget

    private void DisplayGroup(ParamEditorScreen editor, string paramName, Param param, Param.Row row,
        List<FaceField> fields, bool storesModes, FaceGenModelData model, int groupIndex)
    {
        var tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable;

        if (!ImGui.BeginTable($"faceGenFields{groupIndex}", 2, tableFlags))
            return;

        ImGui.TableSetupColumn("##name", ImGuiTableColumnFlags.WidthStretch, 0.45f);
        ImGui.TableSetupColumn("##value", ImGuiTableColumnFlags.WidthStretch, 0.55f);

        foreach (var field in fields)
        {
            if (row[field.Name] is not { } cell)
                continue;

            ImGui.PushID(field.Name);
            ImGui.TableNextRow();

            if (field.Kind == FaceFieldKind.Field && FaceComposition.IsPartField(field.Name))
            {
                PartRow(editor, paramName, param, row, field, storesModes, model);
            }
            else if (field.Kind == FaceFieldKind.Field)
            {
                FieldRow(paramName, param, row, cell, field, storesModes, model);
            }
            else
            {
                ImGui.TableSetColumnIndex(0);
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(DisplayName(param, field.Name, storesModes, model));
                NameTooltip(param, field, storesModes, model);

                ImGui.TableSetColumnIndex(1);

                if (field.Kind == FaceFieldKind.Slider)
                    SliderRow(editor, paramName, row, field, storesModes);
                else
                    ColorRow(editor, paramName, row, field);
            }

            ImGui.PopID();
        }

        ImGui.EndTable();
    }

    private static int GetInt(Param.Row row, string field) =>
        row[field]?.Value is IConvertible value ? value.ToInt32(null) : 0;

    // ---- slider: one byte

    private void SliderRow(ParamEditorScreen editor, string paramName, Param.Row row, FaceField field, bool storesModes)
    {
        var key = $"{row.ID}/{field.Name}";
        var value = EditingKey == key ? EditingValue : GetInt(row, field.Name);

        ImGui.SetNextItemWidth(-1);
        ImGui.SliderInt("##slider", ref value, 0, 255);

        DrawNeutralMark();

        if (ImGui.IsItemActive())
        {
            EditingKey = key;
            EditingValue = value;

            // The viewport shows the value being dragged before it is written
            View.PreviewOverrides.Clear();
            View.PreviewOverrides[field.Name] = value;
        }

        if (GUI.InputCommitted())
        {
            FaceGenEdits.SetField(editor, paramName, row, field.Name, value, LOC.Get("FACEGEN_Action_Edit_Field", field.Name, row.ID));
            EditingKey = null;
            View.PreviewOverrides.Clear();
        }
        else if (ImGui.IsItemDeactivated() && EditingKey == key)
        {
            EditingKey = null;
            View.PreviewOverrides.Clear();
        }

        // What the byte stands for in the game: a mode coefficient, or the slider's own value
        if (ImGui.IsItemHovered())
        {
            if (storesModes && FaceGenParams.ParseFaceGenField(field.Name) != null)
                GUI.Tooltip(LOC.Get("FACEGEN_Field_Coefficient", FaceGenParams.ByteToCoefficient(value).ToString("0.00")));
            else if (!storesModes && FaceGenModel.SliderValue(Project, field.Name, value) is { } sliderValue)
                GUI.Tooltip(LOC.Get("FACEGEN_Field_Slider_Value", sliderValue.ToString("0.00")));
        }

        ValueMenu(editor, paramName, row, new[] { field.Name }, FaceGenParams.Neutral);
    }

    /// <summary>A tick on the slider where the neutral value (128) is</summary>
    private static void DrawNeutralMark()
    {
        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var grab = ImGui.GetStyle().GrabMinSize;
        var x = min.X + grab * 0.5f + (max.X - min.X - grab) * (FaceGenParams.Neutral / 255f);

        ImGui.GetWindowDrawList().AddLine(new Vector2(x, max.Y - 3), new Vector2(x, max.Y),
            ImGui.GetColorU32(ImGuiCol.TextDisabled), 1.5f);
    }

    // ---- colour: three bytes

    private void ColorRow(ParamEditorScreen editor, string paramName, Param.Row row, FaceField field)
    {
        var key = $"{row.ID}/{field.Name}";
        var color = EditingKey == key
            ? EditingColor
            : new Vector3(GetInt(row, field.Name), GetInt(row, field.Green!), GetInt(row, field.Blue!)) / 255f;

        ImGui.SetNextItemWidth(-1);
        ImGui.ColorEdit3("##color", ref color, ImGuiColorEditFlags.Uint8 | ImGuiColorEditFlags.DisplayRgb);

        var bytes = Vector3.Clamp(color * 255f, Vector3.Zero, new Vector3(255f));

        if (ImGui.IsItemActive())
        {
            EditingKey = key;
            EditingColor = color;

            // The viewport shows the colour being picked before it is written
            View.PreviewOverrides.Clear();
            View.PreviewOverrides[field.Name] = (int)MathF.Round(bytes.X);
            View.PreviewOverrides[field.Green!] = (int)MathF.Round(bytes.Y);
            View.PreviewOverrides[field.Blue!] = (int)MathF.Round(bytes.Z);
        }

        if (GUI.InputCommitted())
        {

            FaceGenEdits.SetFields(editor, paramName, row, new (string, object)[]
            {
                (field.Name, (int)MathF.Round(bytes.X)),
                (field.Green!, (int)MathF.Round(bytes.Y)),
                (field.Blue!, (int)MathF.Round(bytes.Z)),
            }, LOC.Get("FACEGEN_Action_Edit_Color", field.Name, row.ID));

            EditingKey = null;
            View.PreviewOverrides.Clear();
        }
        else if (ImGui.IsItemDeactivated() && EditingKey == key)
        {
            EditingKey = null;
            View.PreviewOverrides.Clear();
        }

        ValueMenu(editor, paramName, row, new[] { field.Name, field.Green!, field.Blue! }, null);
    }

    /// <summary>A slider's or colour's right-click menu: back to neutral, and the row in the Param Editor</summary>
    private void ValueMenu(ParamEditorScreen editor, string paramName, Param.Row row, string[] fields, int? neutral)
    {
        if (!ImGui.BeginPopupContextItem("faceGenValueMenu"))
            return;

        if (neutral is { } value &&
            ImGui.Selectable($"{LOC.Get("FACEGEN_Value_Reset_Neutral", value)}##faceGenResetNeutral"))
        {
            FaceGenEdits.SetFields(editor, paramName, row, fields.Select(f => (f, (object)value)).ToList(),
                LOC.Get("FACEGEN_Action_Edit_Field", fields[0], row.ID));
        }

        if (ImGui.Selectable($"{LOC.Get("FACEGEN_Open_In_Param_Editor")}##openInParamEditor"))
            EditorCommandQueue.AddCommand($"param/select/-1/{paramName}/{row.ID}");

        ImGui.EndPopup();
    }

    // ---- part: a picker of the models the field can name

    private void PartRow(ParamEditorScreen editor, string paramName, Param param, Param.Row row, FaceField field,
        bool storesModes, FaceGenModelData model)
    {
        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(DisplayName(param, field.Name, storesModes, model));
        NameTooltip(param, field, storesModes, model);

        ImGui.TableSetColumnIndex(1);

        var isFemale = View.ViewportWindow.IsFemale();
        var value = GetInt(row, field.Name);
        var part = FaceComposition.Resolve(Project, field.Name, value, isFemale);
        var preview = part.Body != null || (part.Model != null && !part.IsFallback)
            ? $"{value}: {part.Label}"
            : LOC.Get("FACEGEN_Part_None", value);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##part", preview))
        {
            foreach (var choice in FaceComposition.GetChoices(Project, field.Name, isFemale))
            {
                var selected = choice.Value == value || (choice.Model == part.Label && !part.IsFallback);

                if (ImGui.Selectable($"{choice.Value}: {choice.Model}##part{choice.Value}", selected))
                {
                    FaceGenEdits.SetField(editor, paramName, row, field.Name, choice.Value,
                        LOC.Get("FACEGEN_Action_Edit_Field", field.Name, row.ID));
                }

                if (selected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(part.IsFallback
            ? LOC.Get("FACEGEN_Part_Fallback", value, part.Model)
            : LOC.Get("FACEGEN_Part_Picker_TT"));

        ValueMenu(editor, paramName, row, new[] { field.Name }, null);
    }

    // ---- field: as the Param Editor draws it

    /// <summary>One field, as the Param Editor draws it: name, labels, input, hints, clicks and context menu</summary>
    private void FieldRow(string paramName, Param param, Param.Row row, Param.Cell cell, FaceField faceField,
        bool storesModes, FaceGenModelData model)
    {
        var decorators = Decorators!;
        var input = FieldInput!;
        var paramData = Project.Handler.ParamData;
        var field = cell.Def;
        var meta = paramData.GetParamMeta(param.AppliedParamdef);
        var fieldMeta = paramData.GetParamFieldMeta(meta, field);
        var annotation = paramData.GetFieldAnnotation(paramData.GetParamAnnotations(param.ParamType), field.InternalName);
        var context = new FieldMetaContext(FieldView!, meta, fieldMeta, annotation, paramName, field.InternalName);
        var oldval = cell.Value;
        object newval = null!;

        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(DisplayName(param, field.InternalName, storesModes, model));
        NameTooltip(param, faceField, storesModes, model);

        decorators.HandleCache(context, row, oldval);
        decorators.HandleLabels(context, row, oldval);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup("faceGenFieldValueMenu");

        ImGui.TableSetColumnIndex(1);

        var isReference = context.HasAnyReferenceElements();

        if (isReference)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_IsRef_Text);

        input.DisplayFieldInput(context, oldval.GetType(), oldval, ref newval, field.BitSize);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup("faceGenFieldValueMenu");

        decorators.HandleHints(context, row, paramName, field.InternalName, oldval);

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            ImGui.OpenPopup("faceGenFieldValueMenu");

        decorators.HandleClick(context, row, oldval);

        if (isReference)
            ImGui.PopStyleColor();

        // The Param Editor's reference, enum and alias choices
        if (ImGui.BeginPopup("faceGenFieldValueMenu"))
        {
            if (context.HasAnyDisplayedElements() && decorators.HandleContextMenu(context, row, oldval, ref newval))
                input.SetLastPropertyManual(newval);

            if (ImGui.Selectable($"{LOC.Get("FACEGEN_Open_In_Param_Editor")}##openInParamEditor"))
                EditorCommandQueue.AddCommand($"param/select/-1/{paramName}/{row.ID}");

            ImGui.EndPopup();
        }

        if (input.UpdateProperty(cell, CellValue, oldval))
            paramData.PrimaryBank.RefreshParamRowDiffs(View.Editor.ParamEditor!, row, paramName);
    }
}
