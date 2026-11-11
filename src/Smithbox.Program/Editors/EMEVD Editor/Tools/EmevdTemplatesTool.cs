using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Templates (P6): ready-made event patterns with fill-in fields. Built-ins are FromSoftware's own copy-paste
/// patterns, generated from vanilla events (the args that differ between the copies are the fields); the project's
/// templates are saved from a selection or a whole event. A template goes in after the selected block (like a paste),
/// or as a new event in the selected file.
/// </summary>
public class EmevdTemplatesTool
{
    private readonly EmevdEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    private EmevdTemplate Selected;
    private List<string> FieldTexts = new();

    private string NewEventIdText = "";
    private string SaveName = "";

    public EmevdTemplatesTool(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdTemplateLibrary Library => View.Templates;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("EMEVD_TemplatesTool_Header")}##emevdTemplatesToolHeader"))
            return;

        ImGui.BeginChild("EmevdTemplatesToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("EMEVD_TemplatesTool_Hint"));

        if (View.Project.Handler.EmevdData?.Catalog == null)
        {
            GUI.WrappedText(LOC.Get("EMEVD_PaletteTool_No_Layouts"));
            return;
        }

        var all = Library.All;

        // Templates
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_TemplatesTool_List_Header"), LOC.Get("EMEVD_TemplatesTool_List_Header_TT"));

        EditorFilters.DisplayFramedListFilter("emevdTemplatesFilter", ref Filter, ref ExactFilter);

        ImGui.BeginChild("emevdTemplatesList", new Vector2(0, 220f * DPI.UIScale()), ImGuiChildFlags.Borders | ImGuiChildFlags.ResizeY);

        if (all.Count == 0)
            GUI.WrappedText(LOC.Get("EMEVD_TemplatesTool_None"));

        var groups = all
            .Where(t => Filter.Length == 0 || EditorFilters.IsMatch(Filter, t.Name, ExactFilter, t.Category ?? "", rawSecondaryText: t.Description ?? ""))
            .GroupBy(t => t.BuiltIn ? t.Category ?? "Other" : LOC.Get("EMEVD_TemplatesTool_Project_Category"));

        var index = 0;
        foreach (var group in groups)
        {
            if (Filter.Length > 0)
                ImGui.SetNextItemOpen(true, ImGuiCond.Always);

            if (!ImGui.TreeNodeEx($"{group.Key} ({group.Count()})##emevdTemplateGroup{group.Key}", ImGuiTreeNodeFlags.SpanAvailWidth))
                continue;

            foreach (var t in group)
            {
                if (ImGui.Selectable($"{t.Name}##emevdTemplate{index++}", ReferenceEquals(t, Selected)))
                    Select(t);
                GUI.Tooltip(t.Description ?? t.Name);
            }

            ImGui.TreePop();
        }

        ImGui.EndChild();

        if (Selected != null && !all.Contains(Selected))
            Select(null);

        if (Selected != null)
        {
            DisplaySelected();
            DisplayFields();
            DisplayInsert();
        }

        DisplaySave();
    }

    private void Select(EmevdTemplate t)
    {
        Selected = t;
        FieldTexts = t?.Fields.Select(f => Format(f, f.Default)).ToList() ?? new();
        NewEventIdText = "";
    }

    private void DisplaySelected()
    {
        var t = Selected;

        // Template
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_TemplatesTool_Selected_Header"), LOC.Get("EMEVD_TemplatesTool_Selected_Header_TT"));

        GUI.WrappedText(t.Name);
        if (!string.IsNullOrEmpty(t.Description))
            GUI.WrappedText(t.Description);
        GUI.WrappedText(LOC.Get("EMEVD_TemplatesTool_Size", t.Lines.Count, t.Fields.Count, t.Parameters.Count));

        if (!string.IsNullOrEmpty(t.Source))
        {
            GUI.WrappedText(LOC.Get(t.BuiltIn ? "EMEVD_TemplatesTool_Source_BuiltIn" : "EMEVD_TemplatesTool_Source_Project", t.Source));

            if (SourceFile(t, out var file, out var id))
            {
                GUI.MultiButtonInput("emevdTemplateSourceActions",
                    "emevdTemplateGoSource", LOC.Get("EMEVD_TemplatesTool_Go_Source"), LOC.Get("EMEVD_TemplatesTool_Go_Source_TT"),
                    () =>
                    {
                        // Read on demand, as the file list does
                        if (file.EnsureLoaded() && file.FindEvent(id) is { } ev)
                            View.Selection.JumpTo(file, ev);
                    });
            }
        }

        if (!t.BuiltIn)
        {
            GUI.MultiButtonInput("emevdTemplateArchiveActions",
                "emevdTemplateArchive", LOC.Get("EMEVD_TemplatesTool_Archive"), LOC.Get("EMEVD_TemplatesTool_Archive_TT"),
                () => { if (Library.Archive(t)) Select(null); });
        }
    }

    private void DisplayFields()
    {
        var t = Selected;
        if (t == null)
            return;

        // Fields
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_TemplatesTool_Fields_Header"), LOC.Get("EMEVD_TemplatesTool_Fields_Header_TT"));

        if (t.Fields.Count == 0)
            GUI.WrappedText(LOC.Get("EMEVD_TemplatesTool_No_Fields"));

        for (var f = 0; f < t.Fields.Count; f++)
        {
            var field = t.Fields[f];
            var text = FieldTexts[f];

            ImGui.TextDisabled(field.Name);
            if (field.Role != EmevdArgRole.None)
            {
                ImGui.SameLine();
                ImGui.TextDisabled($"({LOC.Get($"EMEVD_MassEditTool_Role_{field.Role}")}{(string.IsNullOrEmpty(field.RoleType) ? "" : $": {field.RoleType}")})");
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"##emevdTemplateField{f}", ref text, 32);
            FieldTexts[f] = text;
            GUI.Tooltip(LOC.Get("EMEVD_TemplatesTool_Field_TT", Format(field, field.Default), field.Uses.Count,
                string.Join(", ", field.Uses.Select(u => u.Line).Distinct())));

            if (!TryValue(field, text, out _))
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_SearchTool_Value_Invalid", text));
        }

        if (t.Fields.Count > 0)
        {
            GUI.MultiButtonInput("emevdTemplateFieldActions",
                "emevdTemplateDefaults", LOC.Get("EMEVD_TemplatesTool_Defaults"), LOC.Get("EMEVD_TemplatesTool_Defaults_TT"),
                () => Select(t));
        }
    }

    private void DisplayInsert()
    {
        var t = Selected;
        var selection = View.Selection;

        // Insert
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_TemplatesTool_Insert_Header"), LOC.Get("EMEVD_TemplatesTool_Insert_Header_TT"));

        var values = Values(out var badField);
        string problem = null;
        if (badField != null)
            problem = LOC.Get("EMEVD_TemplatesTool_Bad_Field", badField);
        else if (EmevdTemplates.Build(t, values, out var buildProblem) == null)
            problem = buildProblem;

        if (problem != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, problem);

        // New event ID: the next free one after the selected event
        var file = selection.SelectedFile;
        if (file?.Emevd != null)
        {
            if (NewEventIdText.Length == 0)
                NewEventIdText = EmevdEventOps.NextFreeId(file.Emevd.Events, selection.SelectedEvent != null ? selection.SelectedEvent.ID + 1 : 0).ToString(CultureInfo.InvariantCulture);

            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##emevdTemplateNewId", LOC.Get("EMEVD_TemplatesTool_New_Id_Hint"), ref NewEventIdText, 16);
            GUI.Tooltip(LOC.Get("EMEVD_TemplatesTool_New_Id_TT"));
        }

        var idOk = long.TryParse(NewEventIdText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var newId) && newId >= 0 && newId <= uint.MaxValue;
        var idFree = idOk && file?.Emevd != null && !file.HasEvent(newId);

        if (file?.Emevd != null && !idOk)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Event_Id_Invalid"));
        else if (file?.Emevd != null && !idFree)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Event_Id_Taken", newId));

        GUI.ConditionalMultiButtonInput("emevdTemplateInsertActions",
            "emevdTemplateInsertLines", LOC.Get("EMEVD_TemplatesTool_Insert_Lines"), LOC.Get("EMEVD_TemplatesTool_Insert_Lines_TT"),
            () => InsertLines(t, values), problem == null && selection.SelectedEvent != null,

            "emevdTemplateInsertEvent", LOC.Get("EMEVD_TemplatesTool_Insert_Event"), LOC.Get("EMEVD_TemplatesTool_Insert_Event_TT"),
            () => InsertEvent(t, values, file, newId), problem == null && idFree);

        if (selection.SelectedEvent == null)
            GUI.WrappedText(LOC.Get("EMEVD_TemplatesTool_Need_Event"));
    }

    private void DisplaySave()
    {
        var selection = View.Selection;
        var script = View.BlockView.CurrentScript;

        // Save as Template
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EMEVD_TemplatesTool_Save_Header"), LOC.Get("EMEVD_TemplatesTool_Save_Header_TT"));

        if (script == null)
        {
            GUI.WrappedText(LOC.Get("EMEVD_TemplatesTool_Save_Need_Event"));
            return;
        }

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##emevdTemplateSaveName", LOC.Get("EMEVD_TemplatesTool_Save_Name_Hint"), ref SaveName, 128);
        GUI.Tooltip(LOC.Get("EMEVD_TemplatesTool_Save_Name_TT"));

        var nameOk = EmevdTemplateLibrary.Clean(SaveName) != null;
        var exists = nameOk && Library.Exists(SaveName);
        if (exists)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_TemplatesTool_Save_Exists", SaveName));

        var lines = selection.Lines.Where(l => l < script.Lines.Count).OrderBy(l => l).ToList();

        GUI.ConditionalMultiButtonInput("emevdTemplateSaveActions",
            "emevdTemplateSaveSelection", LOC.Get("EMEVD_TemplatesTool_Save_Selection", lines.Count), LOC.Get("EMEVD_TemplatesTool_Save_Selection_TT"),
            () => Save(script, lines), nameOk && !exists && lines.Count > 0,

            "emevdTemplateSaveEvent", LOC.Get("EMEVD_TemplatesTool_Save_Event"), LOC.Get("EMEVD_TemplatesTool_Save_Event_TT"),
            () => Save(script, Enumerable.Range(0, script.Lines.Count).ToList()), nameOk && !exists && script.Lines.Count > 0);
    }

    // ---------------------------------------------------------------- actions

    private void InsertLines(EmevdTemplate t, List<double> values)
    {
        var lines = EmevdTemplates.Build(t, values, out _);
        if (lines != null)
            View.Structure.InsertLines(lines, LOC.Get("EMEVD_TemplatesTool_Insert_What", t.Name));
    }

    private void InsertEvent(EmevdTemplate t, List<double> values, EmevdFileWrapper file, long id)
    {
        var ev = EmevdTemplates.MakeEvent(t, id, values, out _);
        if (ev == null || file?.Emevd == null)
            return;

        var selected = View.Selection.SelectedEvent;
        var at = selected != null ? file.Emevd.Events.IndexOf(selected) + 1 : file.Emevd.Events.Count;

        View.Edit(EmevdEventListChange.Insert(file, ev, at));
        View.Selection.SelectEvent(ev);
        View.Selection.ScrollToEvent = true;
        NewEventIdText = "";
    }

    private void Save(EmevdScript script, List<int> lines)
    {
        var catalog = View.Project.Handler.EmevdData?.Catalog;
        var file = View.Selection.SelectedFile;

        var t = EmevdTemplates.FromLines(catalog, script, lines, SaveName.Trim());
        t.Source = $"{file?.Name} {script.Event.ID}";
        t.Description = LOC.Get("EMEVD_TemplatesTool_Saved_Description", file?.Name, script.Event.ID, EmevdTemplates.Outline(script));

        if (Library.Save(t))
        {
            SaveName = "";
            Select(Library.All.FirstOrDefault(x => !x.BuiltIn && x.Name == t.Name));
        }
    }

    // ---------------------------------------------------------------- values

    /// <summary>The field values typed in; the first field that doesn't parse is named</summary>
    private List<double> Values(out string badField)
    {
        badField = null;
        var values = new List<double>();

        for (var f = 0; f < Selected.Fields.Count; f++)
        {
            if (!TryValue(Selected.Fields[f], FieldTexts[f], out var v))
            {
                badField ??= Selected.Fields[f].Name;
                v = Selected.Fields[f].Default;
            }
            values.Add(v);
        }

        return values;
    }

    /// <summary>A float field takes a decimal number; an integer one a whole number (negative allowed) or 0x hex</summary>
    private static bool TryValue(EmevdTemplateField field, string text, out double value)
    {
        value = 0;
        text = text.Trim();

        if (field.IsFloat)
            return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);

        if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && long.TryParse(text.AsSpan(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
        {
            value = hex;
            return true;
        }

        if (long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
        {
            value = v;
            return true;
        }

        return false;
    }

    private static string Format(EmevdTemplateField field, double value)
    {
        return field.IsFloat ? ((float)value).ToString(CultureInfo.InvariantCulture) : ((long)value).ToString(CultureInfo.InvariantCulture);
    }

    /// <summary>The file and event ID a template's Source names ("file id"), if that file is in the project</summary>
    private bool SourceFile(EmevdTemplate t, out EmevdFileWrapper file, out long id)
    {
        file = null;
        id = -1;

        var parts = t.Source?.Split(' ');
        if (parts == null || parts.Length != 2 || !long.TryParse(parts[1], out id))
            return false;

        file = View.Project.Handler.EmevdData?.PrimaryBank?.FindFile(parts[0]);
        return file != null;
    }
}
