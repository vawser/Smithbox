using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

public class EmevdPropertyEditor
{
    private EmevdEditorScreen Screen;

    private object _changingProperty;
    private EditorAction _lastUncommittedAction;

    public EmevdPropertyEditor(EmevdEditorScreen screen)
    {
        Screen = screen;
    }

    public (bool, bool) PropertyRow(ArgDataObject dataObject, object oldValue, out object newValue)
    {
        ImGui.SetNextItemWidth(-1);

        var typ = dataObject.ArgObject.GetType();

        newValue = null;
        var isChanged = false;
        if (typ == typeof(long))
        {
            var val = (long)oldValue;
            var strval = $@"{val}";

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputText($"##value{dataObject.ArgDoc.Name}", ref strval, 99))
            {
                var res = long.TryParse(strval, out val);
                if (res)
                {
                    newValue = val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(int))
        {
            var val = (int)oldValue;

            if (dataObject.ArgDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{dataObject.ArgDoc.Name}", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newValue = val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                if (ImGui.InputInt($"##value{dataObject.ArgDoc.Name}", ref val))
                {
                    newValue = val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(uint))
        {
            var val = (uint)oldValue;
            var strval = $@"{val}";

            if (dataObject.ArgDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{dataObject.ArgDoc.Name}", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newValue = val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                if (ImGui.InputText($"##value{dataObject.ArgDoc.Name}", ref strval, 16))
                {
                    var res = uint.TryParse(strval, out val);
                    if (res)
                    {
                        newValue = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(short))
        {
            int val = (short)oldValue;

            if (dataObject.ArgDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{dataObject.ArgDoc.Name}", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newValue = (short)val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                if (ImGui.InputInt($"##value{dataObject.ArgDoc.Name}", ref val))
                {
                    newValue = (short)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(ushort))
        {
            var val = (ushort)oldValue;
            var strval = $@"{val}";

            if (dataObject.ArgDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{dataObject.ArgDoc.Name}", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newValue = val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                if (ImGui.InputText($"##value{dataObject.ArgDoc.Name}", ref strval, 5))
                {
                    var res = ushort.TryParse(strval, out val);
                    if (res)
                    {
                        newValue = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(sbyte))
        {
            int val = (sbyte)oldValue;

            if (dataObject.ArgDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{dataObject.ArgDoc.Name}", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newValue = (sbyte)val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                if (ImGui.InputInt($"##value{dataObject.ArgDoc.Name}", ref val))
                {
                    newValue = (sbyte)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(byte))
        {
            var val = (byte)oldValue;
            var strval = $@"{val}";

            if (dataObject.ArgDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{dataObject.ArgDoc.Name}", ref bVar))
                {
                    if (bVar == true)
                        val = 1;
                    else
                        val = 0;

                    newValue = val;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                if (ImGui.InputText($"##value{dataObject.ArgDoc.Name}", ref strval, 3))
                {
                    var res = byte.TryParse(strval, out val);
                    if (res)
                    {
                        newValue = val;
                        isChanged = true;
                    }
                }
            }
        }
        else if (typ == typeof(bool))
        {
            var val = (bool)oldValue;

            ImGui.AlignTextToFramePadding();
            if (ImGui.Checkbox($"##value{dataObject.ArgDoc.Name}", ref val))
            {
                newValue = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(float))
        {
            var val = (float)oldValue;

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputFloat($"##value{dataObject.ArgDoc.Name}", ref val))
            {
                newValue = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(string))
        {
            var val = (string)oldValue;
            if (val == null)
            {
                val = "";
            }

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputText($"##value{dataObject.ArgDoc.Name}", ref val, 99))
            {
                newValue = val;
                isChanged = true;
            }
        }
        else
        {
            ImGui.Text("ImplementMe");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        return (isChanged, isDeactivatedAfterEdit);
    }

    public void UpdateProperty(ArgDataObject dataObject, object oldValue, object newValue, bool changed, bool committed)
    {
        if (changed)
        {
            ChangeProperty(dataObject, oldValue, newValue, ref committed);
        }

        if (committed)
        {
            if (_lastUncommittedAction != null && Screen.EditorActionManager.PeekUndoAction() == _lastUncommittedAction)
            {
                if (_lastUncommittedAction is InstructionPropertyChange a)
                {
                    Screen.EditorActionManager.UndoAction();
                    Screen.EditorActionManager.ExecuteAction(a);
                }

                _lastUncommittedAction = null;
            }
        }
    }

    private void ChangeProperty(ArgDataObject dataObject, object oldValue, object newValue, ref bool committed)
    {
        if (_lastUncommittedAction != null &&
            Screen.EditorActionManager.PeekUndoAction() == _lastUncommittedAction)
        {
            Screen.EditorActionManager.UndoAction();
        }
        else
        {
            _lastUncommittedAction = null;
        }

        // This needs to change ins.ArgData instead of just the data object

        var action = new InstructionPropertyChange(dataObject, oldValue, newValue);
        Screen.EditorActionManager.ExecuteAction(action);

        _lastUncommittedAction = action;
    }
}
