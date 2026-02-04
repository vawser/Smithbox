using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
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
        UIHelper.SimpleHeader("Fields", "");

        Parent.Filters.DisplayFieldFilterSearch();

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##addFieldToggle"))
        {
            CFG.Current.GparamEditor_Field_List_Display_Field_Add = !CFG.Current.GparamEditor_Field_List_Display_Field_Add;
        }
        UIHelper.Tooltip("Toggle the display of empty groups.");

        ImGui.BeginChild("GparamFieldsSection");
        Parent.Selection.SwitchWindowContext(GparamEditorContext.Field);

        if (Parent.Selection.IsGparamGroupSelected())
        {
            GPARAM.Param data = Parent.Selection.GetSelectedGparamGroup();

            for (int i = 0; i < data.Fields.Count; i++)
            {
                GPARAM.IField entry = data.Fields[i];

                var name = entry.Key;
                if (CFG.Current.GparamEditor_Field_List_Enable_Aliases)
                    name = FormatInformationUtils.GetReferenceName(Project.Handler.GparamData.GparamInformation, entry.Key, entry.Name);

                if (Parent.Filters.IsFieldFilterMatch(entry.Name, ""))
                {
                    // Field row
                    if (ImGui.Selectable($@" {name}##{entry.Key}{i}", i == Parent.Selection._selectedParamFieldKey))
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
                }

                Parent.ContextMenu.FieldContextMenu(i);
            }

            if (CFG.Current.GparamEditor_Field_List_Display_Field_Add)
            {
                ImGui.Separator();

                DisplayMissingFieldSection();
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Fields list: Add buttons
    /// </summary>
    public void DisplayMissingFieldSection()
    {
        GPARAM.Param data = Parent.Selection.GetSelectedGparamGroup();

        List<FormatMember> missingFields = new List<FormatMember>();

        // Get source Format Reference
        foreach (var entry in Project.Handler.GparamData.GparamInformation.list)
        {
            if (entry.id == Parent.Selection._selectedParamGroup.Key)
            {
                foreach (var member in entry.members)
                {
                    bool isPresent = false;

                    foreach (var pField in data.Fields)
                    {
                        if (member.id == pField.Key)
                        {
                            isPresent = true;
                        }
                    }

                    if (!isPresent)
                    {
                        missingFields.Add(member);
                    }
                }
            }
        }

        foreach (var missing in missingFields)
        {
            // Unknown should be skipped
            if (missing.id != "Unknown")
            {
                if (ImGui.Button($"Add##{missing.id}", DPI.StandardButtonSize))
                {
                    AddMissingField(Parent.Selection._selectedParamGroup, missing);
                }
                ImGui.SameLine();
                ImGui.Text($"{missing.name}");
            }
        }
    }

    /// <summary>
    /// Add missing field to target Param Group
    /// </summary>
    /// <param name="targetParam"></param>
    /// <param name="missingField"></param>
    public void AddMissingField(Param targetParam, FormatMember missingField)
    {
        var typeName = FormatInformationUtils.GetTypeForProperty(Project.Handler.GparamData.GparamInformation, missingField.id);

        if (typeName == "Byte")
        {
            GPARAM.ByteField newField = new GPARAM.ByteField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<byte>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<byte>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Short")
        {
            GPARAM.ShortField newField = new GPARAM.ShortField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<short>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<short>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "IntA" || typeName == "IntB")
        {
            GPARAM.IntField newField = new GPARAM.IntField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<int>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<int>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float")
        {
            GPARAM.FloatField newField = new GPARAM.FloatField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<float>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = 0;

            newField.Values = new List<FieldValue<float>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "BoolA" || typeName == "BoolB")
        {
            GPARAM.BoolField newField = new GPARAM.BoolField();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<bool>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = false;

            newField.Values = new List<FieldValue<bool>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float2")
        {
            GPARAM.Vector2Field newField = new GPARAM.Vector2Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector2>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector2(0f, 0f);

            newField.Values = new List<FieldValue<Vector2>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float3")
        {
            GPARAM.Vector3Field newField = new GPARAM.Vector3Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector3>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector3(0f, 0f, 0f);

            newField.Values = new List<FieldValue<Vector3>> { valueList };

            targetParam.Fields.Add(newField);
        }
        if (typeName == "Float4")
        {
            GPARAM.Vector4Field newField = new GPARAM.Vector4Field();
            newField.Key = missingField.id;
            newField.Name = missingField.name;

            // Empty values
            var valueList = new GPARAM.FieldValue<Vector4>();
            valueList.Id = 0;
            valueList.Unk04 = 0;
            valueList.Value = new Vector4(0f, 0f, 0f, 0f);

            newField.Values = new List<FieldValue<Vector4>> { valueList };

            targetParam.Fields.Add(newField);
        }

        // Unknown
    }
}

