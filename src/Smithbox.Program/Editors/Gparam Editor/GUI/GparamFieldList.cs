using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;
public class GparamFieldList
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    public GparamFieldList(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the event parameter view
    /// </summary>
    public void Display()
    {
        DisplayHeader();

        // Fields
        ImGui.BeginChild("GparamFieldsSection", ImGuiChildFlags.Borders);

        if (Parent.Selection.IsGparamGroupSelected())
        {
            var data = Parent.Selection.GetSelectedGparam();
            var group = Parent.Selection.GetSelectedGparamGroup();
            var field = Parent.Selection.GetSelectedGparamField();

            for (int i = 0; i < group.Fields.Count; i++)
            {
                var entry = group.Fields[i];

                var selected = i == Parent.Selection._selectedParamFieldKey;
                var fieldName = GetFieldName(entry);

                if (!Parent.Filters.IsFieldFilterMatch(entry.Name, ""))
                    continue;

                // Field row
                if (ImGui.Selectable($@"[{i}] {fieldName}##{entry.Key}{i}", selected))
                {
                    Parent.Selection.SetGparamField(i, entry);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Parent.Selection.SelectGparamField)
                {
                    Parent.Selection.SelectGparamField = false;
                    Parent.Selection.SetGparamField(i, entry);
                }

                if (ImGui.IsItemFocused())
                {
                    if (InputManager.HasArrowSelection())
                    {
                        Parent.Selection.SelectGparamField = true;
                    }
                }

                ContextMenu(data, group, entry, i);
            }

            Shortcuts(data, group, field);
        }

        ImGui.EndChild();
    }


    /// <summary>
    /// Context menu for Fields list
    /// </summary>
    public void ContextMenu(GPARAM data, GPARAM.Param param, GPARAM.IField field, int index)
    {
        if (index == Parent.Selection._selectedParamFieldKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_Field_Context"))
            {
                if (ImGui.BeginMenu("Add"))
                {
                    AddFieldMenu(data, param);

                    ImGui.EndMenu();
                }

                if (ImGui.Selectable("Delete"))
                {
                    var action = new GparamDeleteFieldAction(Project, data, param, field);

                    Parent.ActionManager.ExecuteAction(action);
                }
                UIHelper.Tooltip("Delete the selected field.");

                ImGui.Separator();

                if (ImGui.BeginMenu("Quick Edit"))
                {
                    if (ImGui.Selectable("Target"))
                    {
                        Parent.QuickEditHandler.UpdateFieldFilter(Parent.Selection._selectedParamField);
                    }
                    UIHelper.Tooltip("Add this file to the Field Filter in the Quick Edit window.");

                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }
        }
    }

    public void Shortcuts(GPARAM data, GPARAM.Param entry, GPARAM.IField field)
    {
        if (FocusManager.IsFocus(EditorFocusContext.GparamEditor_FieldList))
        {
            // Add (all mising fields)
            if (InputManager.IsPressed(KeybindID.Add))
            {

            }

            // Delete (selected fields)
            if (InputManager.IsPressed(KeybindID.Delete))
            {

            }
        }
    }

    public void DisplayHeader()
    {
        UIHelper.SimpleHeader("Fields", "");

        // Search
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("GparamFieldSearchSection", searchHeight, ImGuiChildFlags.Borders);

        Parent.Filters.DisplayFieldFilterSearch();

        ImGui.EndChild();

    }

    private string GetFieldName(IField entry)
    {
        var name = entry.Key;
        var groupId = Parent.Selection.GetSelectedGparamGroup().Key;
        var fieldId = entry.Key;
        var fieldName = GparamMetaUtils.GetFieldName(Project, groupId, fieldId);

        if (fieldName != null)
        {
            name = fieldName;
        }

        return name;
    }

    private Dictionary<string, List<FieldAddEntry>> AddOptions = null;

    public void AddFieldMenu(GPARAM data, GPARAM.Param param)
    {
        PopulateAddOptions(param);

        var listSize = new Vector2(350, 250) * DPI.UIScale();

        if (!AddOptions.ContainsKey(param.Key))
            return;

        var targetFields = AddOptions[param.Key];

        ImGui.BeginChild("##addFieldList", listSize);

        UIHelper.SimpleHeader("Fields to Add", "");

        for (int i = 0; i < targetFields.Count; i++)
        {
            var curOption = targetFields[i];

            var curAnnotation = curOption.Annotation;
            var curState = curOption.ToAdd;

            // Ignore existing groups, we only want to allow adding missing groups
            if (param.Fields.Any(e => e.Key == curAnnotation.ID))
                continue;

            ImGui.Checkbox($"{curAnnotation.Name}", ref curState);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                curOption.ToAdd = curState;
            }
        }

        ImGui.EndChild();

        if (ImGui.Selectable("Submit"))
        {
            AddNewFields(data, param);
        }
    }

    private void PopulateAddOptions(GPARAM.Param param)
    {
        // Only build the option list once
        if (AddOptions == null)
        {
            var potentialGroups = Project.Handler.GparamData.Annotations.Entries.FirstOrDefault(e => e.Key.Name == CFG.Current.GparamEditor_Annotation_Language);

            if (potentialGroups.Value == null)
                return;

            AddOptions = new();

            var groups = potentialGroups.Value.Params.ToList();
            foreach (var group in groups)
            {
                if(group.ID == param.Key)
                {
                    foreach(var field in group.Fields)
                    {
                        var addEntry = new FieldAddEntry();
                        addEntry.Annotation = field;
                        addEntry.ToAdd = false;

                        if (!AddOptions.ContainsKey(group.ID))
                        {
                            AddOptions.Add(group.ID, new List<FieldAddEntry>() { addEntry });
                        }
                        else
                        {
                            AddOptions[group.ID].Add(addEntry);
                        }
                    }
                }
            }
        }
    }
    public void AddNewFields(GPARAM data, GPARAM.Param param)
    {
        if (!AddOptions.ContainsKey(param.Key))
            return;

        var fields = AddOptions[param.Key];

        var newFields = fields.Where(e => e.ToAdd == true).ToList();
        var entries = new List<GparamAnnotationFieldEntry>();

        foreach (var entry in newFields)
        {
            if (entry.ToAdd)
            {
                var annotationEntry = entry.Annotation;
                entries.Add(annotationEntry);
            }
        }

        var action = new GparamAddFieldAction(Project, data, param, entries);
        Parent.ActionManager.ExecuteAction(action);

        // Reset the to add state so we don't add the already present entries on secondary usages
        foreach (var entry in AddOptions)
        {
            if (entry.Key == param.Key)
            {
                foreach(var field in entry.Value)
                {
                    field.ToAdd = false;
                }
            }
        }
    }
}

public class FieldAddEntry
{
    public GparamAnnotationFieldEntry Annotation { get; set; }

    public bool ToAdd { get; set; }
}

