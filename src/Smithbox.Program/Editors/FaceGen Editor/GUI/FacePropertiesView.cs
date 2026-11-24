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
/// The active face, as groups of sliders (the FaceGen controls and amounts), colours (each R/G/B triplet as one
/// colour) and fields (part IDs and flags, edited as the Param Editor edits them).
/// <para>
/// A slider shows the game's value where the byte has one (the creator's slider value, a mode coefficient, or
/// FaceRangeParam's value), with the stored byte beside it. A slider or colour only writes its row when it is let go,
/// as one undo step; while it is dragged the window shows the value being dragged.
/// </para>
/// <para>
/// Fields that differ from the vanilla row are drawn in the Param Editor's modified colour, and can be put back from
/// their right-click menu. A group's header copies the group's values, and pastes them onto this face or onto every
/// face selected in the list.
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

    // The vanilla row of the active face, found once per frame (null for a face vanilla doesn't have)
    private Param.Row? VanillaRow;

    // The Param Editor's field decorators and inputs, for the fields that aren't sliders or colours
    private ParamEditorView? FieldView;
    private ParamFieldDecorators? Decorators;
    private ParamFieldInput? FieldInput;

    /// <summary>A copied group: its title, the param it came from, and every field's value by name</summary>
    private sealed record GroupCopy(string Title, string ParamName, List<(string Field, object Value)> Values);

    // Shared by every face, so a group can be copied from one face and pasted onto another
    private static GroupCopy? GroupClipboard;

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

        VanillaRow = FaceGenParams.GetVanillaRow(Project, paramName, row);

        DisplayHeader(editor, paramName, param, row);

        ImGui.BeginChild("faceGenPropertiesHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("faceGenFieldFilter", ref FieldFilter, ref ExactFieldFilter);

        // Toggle: internal names
        GUI.DisplayToggleButton("faceGenInternalNamesToggle", Icons.Book,
            ref CFG.Current.FaceGenEditor.FaceGenEditor_Show_Internal_Names,
            "FACEGEN_Properties_Names_Toggle_Display",
            "FACEGEN_Properties_Names_Toggle_Internal",
            "FACEGEN_Properties_Names_Toggle_TT");

        ImGui.EndChild();

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

                var open = ImGui.CollapsingHeader($"{group.Title}##faceGenGroup{g}", ImGuiTreeNodeFlags.DefaultOpen);

                // On the whole group, whatever the filter shows
                GroupMenu(editor, paramName, param, row, group, g);

                if (!open)
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

        // A face vanilla doesn't have can't be compared or reverted
        if (VanillaRow == null)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("FACEGEN_Properties_No_Vanilla"));
            GUI.Tooltip(LOC.Get("FACEGEN_Properties_No_Vanilla_TT"));
        }
    }

    // ---- names

    /// <summary>
    /// The name a field is shown with: the character creator's caption (ER), else its annotation's name unless that is
    /// only a placeholder, else the FaceGen control's label for a shape or texture field, else the internal name
    /// </summary>
    private string DisplayName(Param param, string internalName, bool storesModes, FaceGenModelData model)
    {
        if (CFG.Current.FaceGenEditor.FaceGenEditor_Show_Internal_Names)
            return internalName;

        if (!storesModes && FaceGenMenu.Caption(Project, internalName) is { } caption)
            return caption;

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

    private void NameTooltip(Param param, Param.Row row, FaceField field, bool storesModes, FaceGenModelData model)
    {
        if (!ImGui.IsItemHovered())
            return;

        var lines = new List<string> { field.Kind == FaceFieldKind.Color ? $"{field.Name}, {field.Green}, {field.Blue}" : field.Name };

        if (!storesModes && FaceGenMenu.Caption(Project, field.Name) is { } caption)
            lines.Add(LOC.Get("FACEGEN_Field_Creator_Caption", caption));

        if (FaceGenLabel(field.Name, storesModes, model) is { } label)
            lines.Add(LOC.Get("FACEGEN_Field_FaceGen_Label", label));

        var annotation = Project.Handler.ParamData.GetFieldAnnotation(
            Project.Handler.ParamData.GetParamAnnotations(param.ParamType), field.Name);

        if (!string.IsNullOrWhiteSpace(annotation?.Description))
            lines.Add(annotation.Description);

        if (IsModified(row, field))
            lines.Add(LOC.Get("FACEGEN_Field_Vanilla_Value", VanillaText(field)));

        GUI.Tooltip(string.Join("\n", lines));
    }

    /// <summary>The field's name in the first column, in the modified colour when it differs from vanilla</summary>
    private void NameCell(Param param, Param.Row row, FaceField field, bool storesModes, FaceGenModelData model)
    {
        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();

        var modified = IsModified(row, field);

        // Fields that differ from vanilla take the shared modified colour
        var coloured = modified && VanillaMarkers.PushColour(VanillaStatus.Modified);

        ImGui.TextUnformatted(DisplayName(param, field.Name, storesModes, model));

        VanillaMarkers.PopColour(coloured);

        NameTooltip(param, row, field, storesModes, model);
    }

    // ---- vanilla

    /// <summary>A field's fields: the one, or a colour's three</summary>
    private static string[] FieldNames(FaceField field) =>
        field.Kind == FaceFieldKind.Color ? new[] { field.Name, field.Green!, field.Blue! } : new[] { field.Name };

    private bool IsModified(Param.Row row, FaceField field) =>
        FieldNames(field).Any(f => FaceGenParams.DiffersFromVanilla(row, VanillaRow, f));

    private string VanillaText(FaceField field) =>
        string.Join(", ", FieldNames(field).Select(f => VanillaRow?[f]?.Value?.ToString() ?? "?"));

    /// <summary>The vanilla values of fields, for writing back; empty with no vanilla row</summary>
    private List<(string Field, object Value)> VanillaValues(IEnumerable<string> fields)
    {
        var values = new List<(string, object)>();

        if (VanillaRow == null)
            return values;

        foreach (var field in fields)
        {
            if (VanillaRow[field]?.Value is { } value)
                values.Add((field, value));
        }

        return values;
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
                FieldRow(editor, paramName, param, row, cell, field, storesModes, model);
            }
            else
            {
                NameCell(param, row, field, storesModes, model);

                ImGui.TableSetColumnIndex(1);

                if (field.Kind == FaceFieldKind.Slider)
                    SliderRow(editor, paramName, param, row, field);
                else
                    ColorRow(editor, paramName, row, field);
            }

            ImGui.PopID();
        }

        ImGui.EndTable();
    }

    private static int GetInt(Param.Row row, string field) =>
        row[field]?.Value is IConvertible value ? value.ToInt32(null) : 0;

    // ---- a group's header menu: copy, paste and revert the whole group

    private void GroupMenu(ParamEditorScreen editor, string paramName, Param param, Param.Row row, FaceFieldGroup group, int groupIndex)
    {
        if (!ImGui.BeginPopupContextItem($"faceGenGroupContext{groupIndex}"))
            return;

        var fields = group.Fields.SelectMany(FieldNames).ToList();

        if (ImGui.Selectable($"{LOC.Get("FACEGEN_Group_Copy")}##faceGenGroupCopy"))
        {
            var values = fields
                .Select(f => (Field: f, Value: row[f]?.Value))
                .Where(v => v.Value != null)
                .Select(v => (v.Field, v.Value!))
                .ToList();

            GroupClipboard = new GroupCopy(group.Title, paramName, values);
        }
        GUI.Tooltip(LOC.Get("FACEGEN_Group_Copy_TT"));

        var clipboard = GroupClipboard;
        var pasteFlags = clipboard != null ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled;
        var pasteTitle = clipboard?.Title ?? "";

        if (ImGui.Selectable($"{LOC.Get("FACEGEN_Group_Paste", pasteTitle)}##faceGenGroupPaste", false, pasteFlags))
            PasteGroup(editor, paramName, new[] { row }, clipboard!);
        GUI.Tooltip(clipboard != null && clipboard.ParamName != paramName
            ? LOC.Get("FACEGEN_Group_Paste_Other_Param_TT", clipboard.ParamName)
            : LOC.Get("FACEGEN_Group_Paste_TT"));

        // Onto every face selected in the list, when there are several
        var selected = View.ListView.GetSelectedRows();

        if (selected.Count > 1 &&
            ImGui.Selectable($"{LOC.Get("FACEGEN_Group_Paste_Selected", pasteTitle, selected.Count)}##faceGenGroupPasteSelected", false, pasteFlags))
        {
            PasteGroup(editor, paramName, selected, clipboard!);
        }

        ImGui.Separator();

        var modified = VanillaRow != null && fields.Any(f => FaceGenParams.DiffersFromVanilla(row, VanillaRow, f));

        if (ImGui.Selectable($"{LOC.Get("FACEGEN_Group_Revert")}##faceGenGroupRevert", false,
                modified ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            FaceGenEdits.SetFields(editor, paramName, row, VanillaValues(fields),
                LOC.Get("FACEGEN_Action_Revert_Group", group.Title, row.ID));
        }
        GUI.Tooltip(LOC.Get("FACEGEN_Group_Revert_TT"));

        ImGui.EndPopup();
    }

    /// <summary>
    /// Writes a copied group onto rows, as one undo step. Only the fields the rows' param has are written, so a group
    /// copied from the other face param pastes what the two share.
    /// </summary>
    private static void PasteGroup(ParamEditorScreen editor, string paramName, IReadOnlyList<Param.Row> rows, GroupCopy copy)
    {
        var name = rows.Count == 1
            ? LOC.Get("FACEGEN_Action_Paste_Group", copy.Title, rows[0].ID)
            : LOC.Get("FACEGEN_Action_Paste_Group_Rows", copy.Title, rows.Count);

        FaceGenEdits.SetFieldsOnRows(editor, paramName, rows, copy.Values, name);
    }

    // ---- slider: one byte, shown as its game value where it has one

    private void SliderRow(ParamEditorScreen editor, string paramName, Param param, Param.Row row, FaceField field)
    {
        var key = $"{row.ID}/{field.Name}";
        var value = EditingKey == key ? EditingValue : GetInt(row, field.Name);
        var range = FaceGenModel.GameRange(Project, param, field.Name);

        if (range is { } r)
        {
            // The game's value, with the stored byte beside it. An inverted range (10 to −10) is drawn as given, so
            // the slider moves the way the byte does, as in the game's menu.
            var byteWidth = ImGui.CalcTextSize("255").X + ImGui.GetStyle().ItemSpacing.X;
            var gameValue = FaceGenModel.ToGameValue(r, value);

            ImGui.SetNextItemWidth(-byteWidth);
            ImGui.SliderFloat("##slider", ref gameValue, r.Min, r.Max, GameValueFormat(r), ImGuiSliderFlags.AlwaysClamp);

            // Snapped to what a byte can hold
            value = FaceGenModel.ToByte(r, gameValue);
        }
        else
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.SliderInt("##slider", ref value, 0, 255);
        }

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

        if (ImGui.IsItemHovered())
        {
            GUI.Tooltip(range is { } tr
                ? LOC.Get("FACEGEN_Field_Game_Value", FaceGenModel.ToGameValue(tr, value).ToString("0.00"), value,
                    tr.Min.ToString("0.##"), tr.Max.ToString("0.##"))
                : LOC.Get("FACEGEN_Field_Byte_Value", value));
        }

        ValueMenu(editor, paramName, row, field, FaceGenParams.Neutral);

        if (range != null)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(value.ToString());
        }
    }

    /// <summary>Enough decimals to tell neighbouring bytes apart: about 1/255 of the range</summary>
    private static string GameValueFormat((float Min, float Max) range)
    {
        var step = MathF.Abs(range.Max - range.Min) / 255f;

        return step >= 0.1f ? "%.1f" : step >= 0.01f ? "%.2f" : "%.3f";
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

        ValueMenu(editor, paramName, row, field, null);
    }

    /// <summary>
    /// A slider's, colour's or part's right-click menu: back to neutral, back to vanilla, and the row in the Param
    /// Editor
    /// </summary>
    private void ValueMenu(ParamEditorScreen editor, string paramName, Param.Row row, FaceField field, int? neutral)
    {
        if (!ImGui.BeginPopupContextItem("faceGenValueMenu"))
            return;

        var fields = FieldNames(field);

        if (neutral is { } value &&
            ImGui.Selectable($"{LOC.Get("FACEGEN_Value_Reset_Neutral", value)}##faceGenResetNeutral"))
        {
            FaceGenEdits.SetFields(editor, paramName, row, fields.Select(f => (f, (object)value)).ToList(),
                LOC.Get("FACEGEN_Action_Edit_Field", fields[0], row.ID));
        }

        RevertItem(editor, paramName, row, field);

        if (ImGui.Selectable($"{LOC.Get("FACEGEN_Open_In_Param_Editor")}##openInParamEditor"))
            EditorCommandQueue.AddCommand($"param/select/-1/{paramName}/{row.ID}");

        ImGui.EndPopup();
    }

    /// <summary>"Revert to vanilla (value)", enabled when the field differs from vanilla</summary>
    private void RevertItem(ParamEditorScreen editor, string paramName, Param.Row row, FaceField field)
    {
        var modified = IsModified(row, field);
        var label = VanillaRow != null
            ? LOC.Get("FACEGEN_Value_Revert_Vanilla", VanillaText(field))
            : LOC.Get("FACEGEN_Value_Revert_Vanilla_None");

        if (ImGui.Selectable($"{label}##faceGenRevertVanilla", false,
                modified ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            FaceGenEdits.SetFields(editor, paramName, row, VanillaValues(FieldNames(field)),
                LOC.Get("FACEGEN_Action_Revert_Field", field.Name, row.ID));
        }
    }

    // ---- part: a picker of the models the field can name

    private void PartRow(ParamEditorScreen editor, string paramName, Param param, Param.Row row, FaceField field,
        bool storesModes, FaceGenModelData model)
    {
        NameCell(param, row, field, storesModes, model);

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

        ValueMenu(editor, paramName, row, field, null);
    }

    // ---- field: as the Param Editor draws it

    /// <summary>One field, as the Param Editor draws it: name, labels, input, hints, clicks and context menu</summary>
    private void FieldRow(ParamEditorScreen editor, string paramName, Param param, Param.Row row, Param.Cell cell,
        FaceField faceField, bool storesModes, FaceGenModelData model)
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

        NameCell(param, row, faceField, storesModes, model);

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

            RevertItem(editor, paramName, row, faceField);

            if (ImGui.Selectable($"{LOC.Get("FACEGEN_Open_In_Param_Editor")}##openInParamEditor"))
                EditorCommandQueue.AddCommand($"param/select/-1/{paramName}/{row.ID}");

            ImGui.EndPopup();
        }

        if (input.UpdateProperty(cell, CellValue, oldval))
            paramData.PrimaryBank.RefreshParamRowDiffs(View.Editor.ParamEditor!, row, paramName);
    }
}
