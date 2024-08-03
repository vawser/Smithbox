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

            // TODO: ref links/enums
            // Add empty Text here if property has ref link or enum list
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
        if (type == Template.ParamType.b)
        {
            oldValue = property;
            bool propertyValue = (bool)property;
            bool inputPropertyValue = propertyValue;

            if (ImGui.Checkbox($"##boolInput{index}", ref inputPropertyValue))
            {
                newValue = inputPropertyValue;
                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.u8)
        {
            oldValue = property;
            byte propertyValue = (byte)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##byteInput{index}", ref inputPropertyValue, 1, 1))
            {
                if(inputPropertyValue >= byte.MaxValue)
                    inputPropertyValue = byte.MaxValue;

                if (inputPropertyValue <= byte.MinValue)
                    inputPropertyValue = byte.MinValue;

                newValue = (byte)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.x8)
        {
            oldValue = property;
            byte propertyValue = (byte)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##hexbyteInput{index}", ref inputPropertyValue, 1, 1, ImGuiInputTextFlags.CharsHexadecimal))
            {
                if (inputPropertyValue >= byte.MaxValue)
                    inputPropertyValue = byte.MaxValue;

                if (inputPropertyValue <= byte.MinValue)
                    inputPropertyValue = byte.MinValue;

                newValue = (byte)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.s8)
        {
            oldValue = property;
            sbyte propertyValue = (sbyte)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##sbyteInput{index}", ref inputPropertyValue, 1, 1))
            {
                if (inputPropertyValue >= sbyte.MaxValue)
                    inputPropertyValue = sbyte.MaxValue;

                if (inputPropertyValue <= sbyte.MinValue)
                    inputPropertyValue = sbyte.MinValue;

                newValue = (sbyte)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.u16)
        {
            oldValue = property;
            ushort propertyValue = (ushort)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##ushortInput{index}", ref inputPropertyValue, 1, 1))
            {
                if (inputPropertyValue >= ushort.MaxValue)
                    inputPropertyValue = ushort.MaxValue;

                if (inputPropertyValue <= ushort.MinValue)
                    inputPropertyValue = ushort.MinValue;

                newValue = (ushort)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.x16)
        {
            oldValue = property;
            ushort propertyValue = (ushort)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##hexshortInput{index}", ref inputPropertyValue, 1, 1, ImGuiInputTextFlags.CharsHexadecimal))
            {
                if (inputPropertyValue >= ushort.MaxValue)
                    inputPropertyValue = ushort.MaxValue;

                if (inputPropertyValue <= ushort.MinValue)
                    inputPropertyValue = ushort.MinValue;

                newValue = (ushort)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.s16)
        {
            oldValue = property;
            short propertyValue = (short)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##shortInput{index}", ref inputPropertyValue, 1, 1))
            {
                if (inputPropertyValue >= short.MaxValue)
                    inputPropertyValue = short.MaxValue;

                if (inputPropertyValue <= short.MinValue)
                    inputPropertyValue = short.MinValue;

                newValue = (short)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.u32)
        {
            oldValue = property;
            int propertyValue = (int)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##uintegerInput{index}", ref inputPropertyValue, 1, 1))
            {
                newValue = (uint)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.u64)
        {
            oldValue = property;
            int propertyValue = (int)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##uintegerInput{index}", ref inputPropertyValue, 1, 1))
            {
                newValue = (ulong)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.x32)
        {
            oldValue = property;
            int propertyValue = (int)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##hexintegerInput{index}", ref inputPropertyValue, 1, 1, ImGuiInputTextFlags.CharsHexadecimal))
            {
                newValue = (uint)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.x64)
        {
            oldValue = property;
            int propertyValue = (int)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##hexintegerInput{index}", ref inputPropertyValue, 1, 1, ImGuiInputTextFlags.CharsHexadecimal))
            {
                newValue = (ulong)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.s32)
        {
            oldValue = property;
            int propertyValue = (int)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##integerInput{index}", ref inputPropertyValue, 1, 1))
            {
                newValue = inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.s64)
        {
            oldValue = property;
            int propertyValue = (int)property;
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##integerInput{index}", ref inputPropertyValue, 1, 1))
            {
                newValue = (long)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.f32 || type == Template.ParamType.f32grad || type == Template.ParamType.f64)
        {
            oldValue = property;
            float propertyValue = (float)property;
            float inputPropertyValue = propertyValue;

            if (ImGui.InputFloat($"##floatInput{index}", ref inputPropertyValue))
            {
                newValue = (float)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.f64)
        {
            oldValue = property;
            float propertyValue = (float)property;
            float inputPropertyValue = propertyValue;

            if (ImGui.InputFloat($"##floatInput{index}", ref inputPropertyValue))
            {
                newValue = (double)inputPropertyValue;

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
