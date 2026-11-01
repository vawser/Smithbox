using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActPropertiesWindow
{
    private const int MaxTextLength = 1024;

    public TimeActEditorView View;
    public ProjectEntry Project;

    private string PropertiesFilter = "";
    private bool ExactPropertiesFilter = false;

    // The text of the fields being typed into, as it only turns into a value once the edit is done
    private Dictionary<string, string> FieldText = new();
    private TAE.Event FieldTextEvent;

    public TimeActPropertiesWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_PropertiesWindow_List_Header"),
            LOC.Get("TAE_PropertiesWindow_List_Header_TT"));

        EditorFilters.DisplayFramedListFilter("timeActEditor_Properties_Filter",
            ref PropertiesFilter, ref ExactPropertiesFilter);

        ImGui.BeginChild($"PropertiesSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplayProperties();

        ImGui.EndChild();
    }

    public void DisplayProperties()
    {
        var ev = View.Selection.SelectedEvent;

        if (ev == null)
            return;

        if (FieldTextEvent != ev)
        {
            FieldTextEvent = ev;
            FieldText.Clear();
        }

        ImGui.BeginDisabled(!Project.Handler.TimeActData.CanSaveTimeActs);

        if (ImGui.BeginTable("##eventProperties", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
        {
            DisplayTimeField(ev, "TAE_PropertiesWindow_Start_Time", ev.StartTime, value => (value, ev.EndTime));
            DisplayTimeField(ev, "TAE_PropertiesWindow_End_Time", ev.EndTime, value => (ev.StartTime, value));

            if (ev.Parameters != null)
            {
                foreach (var (key, parameter) in ev.Parameters.Template)
                {
                    // Fixed values aren't parameters as such, and the writer fills them in
                    if (parameter.ValueToAssert != null)
                        continue;

                    if (!EditorFilters.IsMatch(PropertiesFilter, parameter.Name, ExactPropertiesFilter))
                        continue;

                    DisplayParameter(ev, key, parameter);
                }
            }

            ImGui.EndTable();
        }

        ImGui.EndDisabled();

        if (View.Playback.EventDisplay.GetStatus(ev) is { } status)
        {
            GUI.Spacer();
            GUI.WrappedText(status);
        }

        if (ev.Parameters == null)
        {
            GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_No_Template", ev.Type));

            var bytes = ev.GetParameterBytes(View.Selection.SelectedTimeAct.TimeAct.BigEndian);

            GUI.WrappedText(Convert.ToHexString(bytes));
        }
    }

    private void DisplayTimeField(TAE.Event ev, string nameKey, float value, Func<float, (float Start, float End)> apply)
    {
        var text = BeginField(LOC.Get(nameKey), nameKey, value.ToString());

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText($"##{nameKey}", ref text, MaxTextLength))
        {
            FieldText[nameKey] = text;
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            FieldText.Remove(nameKey);

            if (float.TryParse(text, out var newValue))
            {
                var (start, end) = apply(newValue);

                View.ViewportActionManager.ExecuteAction(new TimeActMoveEventAction(View.Selection.SelectedTimeAct, ev,
                    ev.StartTime, ev.EndTime, start, end));
            }
        }
    }

    private void DisplayParameter(TAE.Event ev, string key, TAE.Template.ParameterTemplate parameter)
    {
        var value = ev.Parameters[key];

        if (parameter.EnumEntries != null)
        {
            DisplayEnumParameter(ev, key, parameter, value);
            return;
        }

        if (parameter.Type is TAE.Template.ParamType.b)
        {
            BeginRow(parameter.Name);

            var ticked = (bool)value;

            if (ImGui.Checkbox($"##{key}", ref ticked))
            {
                SetParameter(ev, key, ticked);
            }

            return;
        }

        var text = BeginField(parameter.Name, key, parameter.ValueToString(value));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText($"##{key}", ref text, MaxTextLength))
        {
            FieldText[key] = text;
        }

        GUI.Tooltip(parameter.Type.ToString());

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            FieldText.Remove(key);

            try
            {
                SetParameter(ev, key, parameter.StringToValue(text));
            }
            catch (Exception e) when (e is FormatException or OverflowException or IndexOutOfRangeException)
            {
                Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_Invalid_Value", text, parameter.Type));
            }
        }
    }

    private void DisplayEnumParameter(TAE.Event ev, string key, TAE.Template.ParameterTemplate parameter, object value)
    {
        BeginRow(parameter.Name);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo($"##{key}", parameter.ValueToString(value)))
        {
            foreach (var (name, entryValue) in parameter.EnumEntries)
            {
                if (ImGui.Selectable(name, entryValue.Equals(value)))
                {
                    SetParameter(ev, key, entryValue);
                }
            }

            ImGui.EndCombo();
        }
    }

    private void SetParameter(TAE.Event ev, string key, object value)
    {
        if (value.Equals(ev.Parameters[key]))
            return;

        View.ViewportActionManager.ExecuteAction(new TimeActSetParameterAction(View.Selection.SelectedTimeAct, ev, key, value));
    }

    private static void BeginRow(string name)
    {
        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();
        ImGui.Text(name);

        ImGui.TableSetColumnIndex(1);
    }

    private string BeginField(string name, string key, string value)
    {
        BeginRow(name);

        return FieldText.TryGetValue(key, out var text) ? text : value;
    }
}
