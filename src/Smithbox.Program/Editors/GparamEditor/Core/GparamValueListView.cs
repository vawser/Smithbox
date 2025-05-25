using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Interface;
using StudioCore.Utilities;
using static SoulsFormats.GPARAM;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamValueListView
{
    private GparamEditorScreen Screen;
    private GparamFilters Filters;
    private GparamSelection Selection;
    private GparamContextMenu ContextMenu;

    private bool[] displayTruth;

    public GparamValueListView(GparamEditorScreen screen)
    {
        Screen = screen;
        Filters = screen.Filters;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the event parameter view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Values##GparamValues");
        Selection.SwitchWindowContext(GparamEditorContext.FieldValue);

        Filters.DisplayFieldValueFilterSearch();

        if (Selection.IsGparamFieldSelected())
        {
            GPARAM.IField field = Selection.GetSelectedGparamField();

            ResetDisplayTruth(field);

            ImGui.Columns(4);

            // ID
            ImGui.BeginChild("IdList##GparamPropertyIds");
            Selection.SwitchWindowContext(GparamEditorContext.FieldValue);
            ImGui.Text($"ID");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                GPARAM.IFieldValue entry = field.Values[i];

                displayTruth[i] = Filters.IsFieldValueFilterMatch(entry.Id.ToString(), "");

                if (displayTruth[i])
                {
                    GparamProperty_ID(i, field, entry);
                }
            }

            // Display "Add" button if field has no value rows.
            if (field.Values.Count <= 0)
            {
                if (ImGui.Button("Add"))
                {
                    Screen.PropertyEditor.AddValueField(field);
                    ResetDisplayTruth(field);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Time of Day
            ImGui.BeginChild("IdList##GparamTimeOfDay");
            Selection.SwitchWindowContext(GparamEditorContext.FieldValue);
            ImGui.Text($"Time of Day");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_TimeOfDay(i, field, entry);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("ValueList##GparamPropertyValues");
            Selection.SwitchWindowContext(GparamEditorContext.FieldValue);
            ImGui.Text($"Value");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_Value(i, field, entry);
                }
            }

            ImGui.EndChild();

            // Information
            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("InfoList##GparamPropertyInfo");
            Selection.SwitchWindowContext(GparamEditorContext.FieldValue);
            ImGui.Text($"Information");
            ImGui.Separator();

            // Only show once
            GparamProperty_Info(field);

            ImGui.EndChild();
        }

        ImGui.End();
    }

    /// <summary>
    /// Reset the Values display truth list
    /// </summary>
    /// <param name="field"></param>
    public void ResetDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count];

        for (int i = 0; i < field.Values.Count; i++)
        {
            displayTruth[i] = true;
        }
    }

    /// <summary>
    /// Extend the Values display truth list in preparation for value row addition.
    /// </summary>
    /// <param name="field"></param>
    public void ExtendDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count + 1];

        for (int i = 0; i < field.Values.Count + 1; i++)
        {
            displayTruth[i] = true;
        }
    }
    /// <summary>
    /// REduce the Values display truth list in preparation for value row removal.
    /// </summary>
    /// <param name="field"></param>
    public void ReduceDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count + -1];

        for (int i = 0; i < field.Values.Count + -1; i++)
        {
            displayTruth[i] = true;
        }
    }

    /// <summary>
    /// Values table: ID column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_ID(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();

        string name = value.Id.ToString();

        if (ImGui.Selectable($"{name}##{index}", index == Selection._selectedFieldValueKey))
        {
            Selection.SetGparamFieldValue(index, value);
        }

        ContextMenu.FieldValueContextMenu(index);
    }

    /// <summary>
    /// Values table: Time of Day column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_TimeOfDay(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        Screen.PropertyEditor.TimeOfDayField(index, field, value);
    }

    /// <summary>
    /// Values table: Value column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_Value(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        Screen.PropertyEditor.ValueField(index, field, value);
    }

    /// <summary>
    /// Values table: Information column
    /// </summary>
    /// <param name="field"></param>
    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        string desc = FormatInformationUtils.GetReferenceDescription(Screen.Project.GparamInformation, Selection._selectedParamGroup.Key, Selection._selectedParamField.Key);

        UIHelper.WrappedText($"Type: {GparamUtils.GetReadableObjectTypeName(field)}");
        UIHelper.WrappedText($"");

        // Skip if empty
        if (desc != "")
        {
            UIHelper.WrappedText($"{desc}");
        }

        // Show enum list if they exist
        var propertyEnum = FormatInformationUtils.GetEnumForProperty(Screen.Project.GparamInformation, Screen.Project.GparamEnums, field.Key);

        if (propertyEnum != null)
        {
            foreach (var entry in propertyEnum.members)
            {
                UIHelper.WrappedText($"{entry.id} - {entry.name}");
            }
        }
    }
}


