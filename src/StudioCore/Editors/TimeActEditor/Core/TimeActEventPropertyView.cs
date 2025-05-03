using Hexa.NET.ImGui;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Core;

public class TimeActEventPropertyView
{
    private TimeActEditorScreen Editor;
    private ActionManager EditorActionManager;
    private TimeActSelectionManager Selection;
    private TimeActDecorator Decorator;
    private TimeActPropertyEditor PropertyEditor;

    public TimeActEventPropertyView(TimeActEditorScreen screen)
    {
        Editor = screen;
        EditorActionManager = screen.EditorActionManager;
        Selection = screen.Selection;
        Decorator = screen.Decorator;
        PropertyEditor = screen.PropertyEditor;
    }

    public void Display()
    {
        ImGui.Begin("Properties##TimeActEventProperties");
        Selection.SwitchWindowContext(TimeActEditorContext.EventProperty);

        if (!Selection.HasSelectedTimeActEvent())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActEventPropertyFilter", ref TimeActFilters._timeActEventPropertyFilterString, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("EventPropertyList");
        Selection.SwitchWindowContext(TimeActEditorContext.EventProperty);

        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.Columns(3);
        }
        else
        {
            ImGui.Columns(2);
        }

        // Property Column
        ImGui.AlignTextToFramePadding();
        ImGui.Selectable($@"Start Time##taeEventProperty_StartTime", false);

        ImGui.AlignTextToFramePadding();
        ImGui.Selectable($@"End Time##taeEventProperty_StartTime", false);

        for (int i = 0; i < Selection.CurrentTimeActEvent.Parameters.ParameterValues.Count; i++)
        {
            var property = Selection.CurrentTimeActEvent.Parameters.ParameterValues.ElementAt(i).Key;

            if (TimeActFilters.TimeActEventPropertyFilter(Selection.ContainerInfo, property))
            {
                var isSelected = false;
                if (i == Selection.CurrentTimeActEventPropertyIndex)
                {
                    isSelected = true;
                }

                ImGui.AlignTextToFramePadding();
                if (ImGui.Selectable($@"{property}##taeEventProperty{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Selection.TimeActEventPropertyChange(property, i);
                }

                Selection.ContextMenu.TimeActEventPropertiesMenu(isSelected, i.ToString());

                Decorator.HandleNameColumn(property);
            }
        }

        ImGui.NextColumn();

        // Value Column
        PropertyEditor.ValueSection(Selection);

        // Type Column
        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("f32");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("f32");

            for (int i = 0; i < Selection.CurrentTimeActEvent.Parameters.ParameterValues.Count; i++)
            {
                var property = Selection.CurrentTimeActEvent.Parameters.ParameterValues.ElementAt(i).Key;
                var propertyType = Selection.CurrentTimeActEvent.Parameters.GetParamValueType(property);

                if (TimeActFilters.TimeActEventPropertyFilter(Selection.ContainerInfo, property))
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{propertyType}");

                    Decorator.HandleTypeColumn(property);

                }
            }
        }

        ImGui.Columns(1);

        ImGui.EndChild();

        ImGui.End();
    }
}
