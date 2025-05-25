using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Utils;
using System;
using System.Linq;
using System.Numerics;
using static SoulsFormats.TAE;
using static SoulsFormats.TAE.Animation;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Handles property edits for direct fields
/// </summary>
public class TimeActPropertyEditor
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActPropertyEditor(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void AnimationHeaderSection(TimeActSelection handler)
    {
        var anim = handler.CurrentTimeActAnimation;
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width * 0.975f, 32);
        var tempHeader = handler.CurrentTemporaryAnimHeader;

        if (anim.MiniHeader.Type == MiniHeaderType.Standard)
        {
            if (ImGui.Button("Switch to Import", buttonSize))
            {
                var tempHeaderOld = tempHeader.Clone();
                var newHeader = new AnimMiniHeader();
                newHeader.Type = MiniHeaderType.ImportOtherAnim;
                newHeader.ImportFromAnimID = tempHeader.ImportFromAnimID;
                newHeader.Unknown = tempHeader.Unknown;

                var action = new TaeAnimEndTimeChange(Editor, anim, anim.MiniHeader, newHeader, tempHeaderOld);
                Editor.EditorActionManager.ExecuteAction(action);
            }
        }
        if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
        {
            if (ImGui.Button("Switch to Standard", buttonSize))
            {
                var tempHeaderOld = tempHeader.Clone();
                var newHeader = new AnimMiniHeader();
                newHeader.Type = MiniHeaderType.Standard;
                newHeader.IsLoopByDefault = tempHeader.IsLoopByDefault;
                newHeader.ImportsHKX = tempHeader.ImportsHKX;
                newHeader.AllowDelayLoad = tempHeader.AllowDelayLoad;
                newHeader.ImportHKXSourceAnimID = tempHeader.ImportHKXSourceAnimID;

                var action = new TaeAnimEndTimeChange(Editor, anim, anim.MiniHeader, newHeader, tempHeaderOld);
                Editor.EditorActionManager.ExecuteAction(action);
            }
        }
    }

    public void AnimationValueSection(TimeActSelection handler)
    {
        var anim = handler.CurrentTimeActAnimation;
        var tempHeader = handler.CurrentTemporaryAnimHeader;

        // Animation
        object newValue;
        bool changed = false;

        (changed, newValue) = HandleProperty("ID", anim.ID, TAE.Template.ParamType.s64);
        if (changed)
        {
            var action = new TaeAnimIdChange(anim, anim.ID, newValue);
            Editor.EditorActionManager.ExecuteAction(action);

            // Re-select row at new index
            TimeActUtils.SelectAdjustedAnimation(Editor, anim);
        }

        changed = false;

        (changed, newValue) = HandleProperty("Name", anim.AnimFileName, TAE.Template.ParamType.str);
        if (changed)
        {
            var action = new TaeAnimFileNameChange(anim, anim.AnimFileName, newValue);
            Editor.EditorActionManager.ExecuteAction(action);
        }

        changed = false;

        if (handler.CurrentTemporaryAnimHeader != null)
        {
            // Header
            if (anim.MiniHeader.Type == MiniHeaderType.Standard)
            {
                // Standard
                (changed, newValue) = HandleProperty("IsLoopByDefault", tempHeader.IsLoopByDefault, TAE.Template.ParamType.b);
                if (changed)
                {
                    var tempHeaderOld = tempHeader.Clone();
                    tempHeader.IsLoopByDefault = (bool)newValue;

                    var newHeader = new AnimMiniHeader();
                    newHeader.Type = MiniHeaderType.Standard;

                    newHeader.IsLoopByDefault = (bool)newValue;
                    newHeader.ImportsHKX = tempHeader.ImportsHKX;
                    newHeader.AllowDelayLoad = tempHeader.AllowDelayLoad;
                    newHeader.ImportHKXSourceAnimID = tempHeader.ImportHKXSourceAnimID;

                    var action = new TaeAnimEndTimeChange(Editor, anim, anim.MiniHeader, newHeader, tempHeaderOld);
                    Editor.EditorActionManager.ExecuteAction(action);
                }

                changed = false;

                (changed, newValue) = HandleProperty("AllowDelayLoad", tempHeader.AllowDelayLoad, TAE.Template.ParamType.b);
                if (changed)
                {
                    var tempHeaderOld = tempHeader.Clone();
                    tempHeader.AllowDelayLoad = (bool)newValue;

                    var newHeader = new AnimMiniHeader();
                    newHeader.Type = MiniHeaderType.Standard;

                    newHeader.IsLoopByDefault = tempHeader.IsLoopByDefault;
                    newHeader.ImportsHKX = tempHeader.ImportsHKX;
                    newHeader.AllowDelayLoad = (bool)newValue;
                    newHeader.ImportHKXSourceAnimID = tempHeader.ImportHKXSourceAnimID;

                    var action = new TaeAnimEndTimeChange(Editor, anim, anim.MiniHeader, newHeader, tempHeaderOld);
                    Editor.EditorActionManager.ExecuteAction(action);

                }

                changed = false;

                (changed, newValue) = HandleProperty("ImportsHKX", tempHeader.ImportsHKX, TAE.Template.ParamType.b);
                if (changed)
                {
                    var tempHeaderOld = tempHeader.Clone();
                    tempHeader.ImportsHKX = (bool)newValue;

                    var newHeader = new AnimMiniHeader();
                    newHeader.Type = MiniHeaderType.Standard;

                    newHeader.IsLoopByDefault = tempHeader.IsLoopByDefault;
                    newHeader.ImportsHKX = (bool)newValue;
                    newHeader.AllowDelayLoad = tempHeader.AllowDelayLoad;
                    newHeader.ImportHKXSourceAnimID = tempHeader.ImportHKXSourceAnimID;

                    var action = new TaeAnimEndTimeChange(Editor, anim, anim.MiniHeader, newHeader, tempHeaderOld);
                    Editor.EditorActionManager.ExecuteAction(action);
                }

                changed = false;

                (changed, newValue) = HandleProperty("ImportHKXSourceAnimID", tempHeader.ImportHKXSourceAnimID, TAE.Template.ParamType.s32);
                if (changed)
                {
                    var tempHeaderOld = tempHeader.Clone();
                    tempHeader.ImportHKXSourceAnimID = (int)newValue;

                    var newHeader = new AnimMiniHeader();
                    newHeader.Type = MiniHeaderType.Standard;

                    newHeader.IsLoopByDefault = tempHeader.IsLoopByDefault;
                    newHeader.ImportsHKX = tempHeader.ImportsHKX;
                    newHeader.AllowDelayLoad = tempHeader.AllowDelayLoad;
                    newHeader.ImportHKXSourceAnimID = (int)newValue;

                    var action = new TaeAnimEndTimeChange(Editor, anim, anim.MiniHeader, newHeader, tempHeaderOld);
                    Editor.EditorActionManager.ExecuteAction(action);
                }
            }

            // Import
            if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
            {
                (changed, newValue) = HandleProperty("ImportFromAnimID", tempHeader.ImportFromAnimID, TAE.Template.ParamType.s32);
                if (changed)
                {
                    var tempHeaderOld = tempHeader.Clone();
                    tempHeader.ImportFromAnimID = (int)newValue;

                    var newHeader = new AnimMiniHeader();
                    newHeader.Type = MiniHeaderType.ImportOtherAnim;

                    newHeader.ImportFromAnimID = (int)newValue;
                    newHeader.Unknown = tempHeader.Unknown;

                    var action = new TaeAnimEndTimeChange(Editor, anim, anim.MiniHeader, newHeader, tempHeaderOld);
                    Editor.EditorActionManager.ExecuteAction(action);
                }
            }
        }
    }

    public void ValueSection(TimeActSelection handler)
    {
        var parameters = handler.CurrentTimeActEvent.Parameters;
        var paramValues = handler.CurrentTimeActEvent.Parameters.ParameterValues;

        object newValue;
        bool changed = false;

        (changed, newValue) = HandleProperty("startTime", handler.CurrentTimeActEvent.StartTime, TAE.Template.ParamType.f32);
        if(changed)
        {
            var action = new TaeEventStartTimeChange(handler.CurrentTimeActEvent, handler.CurrentTimeActEvent.StartTime, newValue);
            Editor.EditorActionManager.ExecuteAction(action);
        }

        changed = false;

        (changed, newValue) = HandleProperty("endTime", handler.CurrentTimeActEvent.EndTime, TAE.Template.ParamType.f32);

        if (changed)
        {
            var action = new TaeEventEndTimeChange(handler.CurrentTimeActEvent, handler.CurrentTimeActEvent.EndTime, newValue);
            Editor.EditorActionManager.ExecuteAction(action);
        }

        for (int i = 0; i < paramValues.Count; i++)
        {
            var propertyName = paramValues.ElementAt(i).Key;
            var propertyValue = paramValues[propertyName];
            var propertyType = parameters.GetParamValueType(propertyName);

            changed = false;

            (changed, newValue) = HandleProperty(i.ToString(), propertyValue, propertyType);

            if(changed)
            {
                var action = new TaeEventParametersChange(paramValues, propertyName, propertyValue, newValue, propertyValue.GetType());
                Editor.EditorActionManager.ExecuteAction(action);
            }

            Editor.Decorator.HandleValueColumn(paramValues, i);
        }
    }

    private object _editedValueCache;
    private bool _changedCache;
    private bool _committedCache;

    public (bool, object) HandleProperty(string index, object property, Template.ParamType type)
    {
        _changedCache = false;
        _committedCache = false;

        object newValue = null;
        object oldValue = null;

        ImGui.SetNextItemWidth(-1);

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
            ulong longValue = (ulong)property;
            int propertyValue = Convert.ToInt32(longValue);
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##ulongInput{index}", ref inputPropertyValue, 1, 1))
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
            long longValue = (long)property;
            int propertyValue = Convert.ToInt32(longValue);
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##hexlongInput{index}", ref inputPropertyValue, 1, 1, ImGuiInputTextFlags.CharsHexadecimal))
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
            long longValue = (long)property;
            int propertyValue = Convert.ToInt32(longValue);
            int inputPropertyValue = propertyValue;

            if (ImGui.InputInt($"##longInput{index}", ref inputPropertyValue, 1, 1))
            {
                newValue = (long)inputPropertyValue;

                _editedValueCache = newValue;
                _changedCache = true;
            }
            _committedCache = ImGui.IsItemDeactivatedAfterEdit();
        }
        else if (type == Template.ParamType.f32 || type == Template.ParamType.f64)
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
        else if (type == Template.ParamType.f32grad)
        {
            oldValue = property;
            Vector2 propertyValue = (Vector2)property;
            Vector2 inputPropertyValue = propertyValue;

            if (ImGui.InputFloat2($"##float2Input{index}", ref inputPropertyValue))
            {
                newValue = (Vector2)inputPropertyValue;

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
        else if (type == Template.ParamType.str)
        {
            oldValue = property;
            string propertyValue = (string)property;
            string inputPropertyValue = propertyValue;

            if (ImGui.InputText($"##textInput{index}", ref inputPropertyValue, 255))
            {
                newValue = (string)inputPropertyValue;

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
                    return (false, property);
                }
                else
                {
                    return (true, newValue);
                }
            }
        }

        return (false, property);
    }
}
