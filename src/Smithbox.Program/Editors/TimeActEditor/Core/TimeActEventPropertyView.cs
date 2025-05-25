using Hexa.NET.ImGui;
using StudioCore.Core;
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
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActEventPropertyView(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.Begin("Properties##TimeActEventProperties");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.EventProperty);

        if (!Editor.Selection.HasSelectedTimeActEvent())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActEventPropertyFilter", ref Editor.Filters._timeActEventPropertyFilterString, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("EventPropertyList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.EventProperty);

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

        for (int i = 0; i < Editor.Selection.CurrentTimeActEvent.Parameters.ParameterValues.Count; i++)
        {
            var property = Editor.Selection.CurrentTimeActEvent.Parameters.ParameterValues.ElementAt(i).Key;

            if (Editor.Filters.TimeActEventPropertyFilter(property))
            {
                var isSelected = false;
                if (i == Editor.Selection.CurrentTimeActEventPropertyIndex)
                {
                    isSelected = true;
                }

                ImGui.AlignTextToFramePadding();
                if (ImGui.Selectable($@"{property}##taeEventProperty{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Editor.Selection.TimeActEventPropertyChange(property, i);
                }

                Editor.ContextMenu.TimeActEventPropertiesMenu(isSelected, i.ToString());

                Editor.Decorator.HandleNameColumn(property);
            }
        }

        ImGui.NextColumn();

        // Value Column
        Editor.PropertyEditor.ValueSection(Editor.Selection);

        // Type Column
        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("f32");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("f32");

            for (int i = 0; i < Editor.Selection.CurrentTimeActEvent.Parameters.ParameterValues.Count; i++)
            {
                var property = Editor.Selection.CurrentTimeActEvent.Parameters.ParameterValues.ElementAt(i).Key;
                var propertyType = Editor.Selection.CurrentTimeActEvent.Parameters.GetParamValueType(property);

                if (Editor.Filters.TimeActEventPropertyFilter(property))
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text($"{propertyType}");

                    Editor.Decorator.HandleTypeColumn(property);

                }
            }
        }

        ImGui.Columns(1);

        ImGui.EndChild();

        ImGui.End();
    }
}
