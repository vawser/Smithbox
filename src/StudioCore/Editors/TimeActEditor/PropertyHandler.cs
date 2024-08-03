using Assimp;
using Google.Protobuf.WellKnownTypes;
using ImGuiNET;
using SoapstoneLib.Proto.Internal;
using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.TAE;
using static StudioCore.Editors.GparamEditor.GparamEditorActions;

namespace StudioCore.Editors.TimeActEditor;

public class PropertyHandler
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;

    public PropertyHandler(ActionManager editorActionManager, TimeActEditorScreen screen)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;
    }

    public void ValueSection(TimeActSelectionHandler handler)
    {
        var parameters = handler.CurrentTimeActEvent.Parameters;
        var paramValues = handler.CurrentTimeActEvent.Parameters.ParameterValues;

        // TODO: add action here
        handler.CurrentTimeActEvent.StartTime = (float)HandleProperty("startTime", handler.CurrentTimeActEvent.StartTime, TAE.Template.ParamType.f32);
        // TODO: add action here
        handler.CurrentTimeActEvent.EndTime = (float)HandleProperty("endTime", handler.CurrentTimeActEvent.EndTime, TAE.Template.ParamType.f32);
        
        for (int i = 0; i < paramValues.Count; i++)
        {
            var property = paramValues.ElementAt(i).Key;
            var propertyValue = paramValues[property];
            var propertyType = parameters.GetParamValueType(property);

            // TODO: add action here
            paramValues[property] = HandleProperty(i.ToString(), propertyValue, propertyType);
        }
    }

    private object _editedValueCache;
    private bool _changedCache;
    private bool _committedCache;

    public object HandleProperty(string index, object property, Template.ParamType type)
    {
        _changedCache = false;
        _committedCache = false;

        object newValue = null;
        object oldValue = null;

        ImGui.SetNextItemWidth(-1);

        // TODO: the rest of the types
        if(type == Template.ParamType.f32)
        {
            oldValue = property;
            float propertyValue = (float)property;
            float inputPropertyValue = propertyValue;

            if (ImGui.InputFloat($"##floatInput{index}", ref inputPropertyValue))
            {
                newValue = inputPropertyValue;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else
        {
            ImGui.Text($"{property}");
        }

        if (_editedValueCache != null && _editedValueCache != oldValue)
        {
            _changedCache = true;
        }

        if (_changedCache)
        {
            if (_committedCache)
            {
                if (newValue == null)
                {
                    return property;
                }
                else
                {
                    return newValue;
                }
            }
        }

        return property;
    }
}
