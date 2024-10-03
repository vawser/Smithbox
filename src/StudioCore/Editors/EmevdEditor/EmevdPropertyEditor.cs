using ImGuiNET;
using Microsoft.Extensions.Logging;
using Octokit;
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
using static StudioCore.Editors.EmevdEditor.EMEDF;

namespace StudioCore.Editors.EmevdEditor;

public class EmevdPropertyEditor
{
    private EmevdEditorScreen Screen;
    private EmevdInstructionHandler InstructionHandler;

    private object _changingProperty;
    private EditorAction _lastUncommittedAction;



    public EmevdPropertyEditor(EmevdEditorScreen screen, EmevdInstructionHandler insHandler)
    {
        Screen = screen;
        InstructionHandler = insHandler;
    }

    /// <summary>
    /// Used for Instruction arguments
    /// </summary>
    public (bool, bool) InstructionArgumentPropertyRow(ArgDoc argDoc, object arg, out object newValue)
    {
        ImGui.SetNextItemWidth(-1);

        var typ = arg.GetType();

        newValue = null;
        var isChanged = false;

        if (typ == typeof(long))
        {
            var val = (long)arg;
            var strval = $@"{val}";

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputText($"##value{argDoc.Name}", ref strval, 99))
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
            var val = (int)arg;

            if (argDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{argDoc.Name}", ref bVar))
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
                if (ImGui.InputInt($"##value{argDoc.Name}", ref val))
                {
                    newValue = val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(uint))
        {
            var val = (uint)arg;
            var strval = $@"{val}";

            if (argDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{argDoc.Name}", ref bVar))
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
                if (ImGui.InputText($"##value{argDoc.Name}", ref strval, 16))
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
            int val = (short)arg;

            if (argDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{argDoc.Name}", ref bVar))
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
                if (ImGui.InputInt($"##value{argDoc.Name}", ref val))
                {
                    newValue = (short)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(ushort))
        {
            var val = (ushort)arg;
            var strval = $@"{val}";

            if (argDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{argDoc.Name}", ref bVar))
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
                if (ImGui.InputText($"##value{argDoc.Name}", ref strval, 5))
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
            int val = (sbyte)arg;

            if (argDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{argDoc.Name}", ref bVar))
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
                if (ImGui.InputInt($"##value{argDoc.Name}", ref val))
                {
                    newValue = (sbyte)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(byte))
        {
            var val = (byte)arg;
            var strval = $@"{val}";

            if (argDoc.EnumName == "BOOL")
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{argDoc.Name}", ref bVar))
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
                if (ImGui.InputText($"##value{argDoc.Name}", ref strval, 3))
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
            var val = (bool)arg;

            ImGui.AlignTextToFramePadding();
            if (ImGui.Checkbox($"##value{argDoc.Name}", ref val))
            {
                newValue = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(float))
        {
            var val = (float)arg;

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputFloat($"##value{argDoc.Name}", ref val))
            {
                newValue = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(string))
        {
            var val = (string)arg;
            if (val == null)
            {
                val = "";
            }

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputText($"##value{argDoc.Name}", ref val, 99))
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

    /// <summary>
    /// Used for Event parameters properties
    /// </summary>
    public (bool, bool) EventParameterPropertyRow(string name, object arg, out object newValue, bool isBool = false, bool isEnum = false)
    {
        ImGui.SetNextItemWidth(-1);

        var typ = arg.GetType();

        newValue = null;
        var isChanged = false;

        if (typ == typeof(long))
        {
            var val = (long)arg;
            var strval = $@"{val}";

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputText($"##value{name}", ref strval, 99))
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
            var val = (int)arg;

            if (isBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{name}", ref bVar))
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
                if (ImGui.InputInt($"##value{name}", ref val))
                {
                    newValue = val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(uint))
        {
            var val = (uint)arg;
            var strval = $@"{val}";

            if (isBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{name}", ref bVar))
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
                if (ImGui.InputText($"##value{name}", ref strval, 16))
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
            int val = (short)arg;

            if (isBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{name}", ref bVar))
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
                if (ImGui.InputInt($"##value{name}", ref val))
                {
                    newValue = (short)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(ushort))
        {
            var val = (ushort)arg;
            var strval = $@"{val}";

            if (isBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{name}", ref bVar))
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
                if (ImGui.InputText($"##value{name}", ref strval, 5))
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
            int val = (sbyte)arg;

            if (isBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{name}", ref bVar))
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
                if (ImGui.InputInt($"##value{name}", ref val))
                {
                    newValue = (sbyte)val;
                    isChanged = true;
                }
            }
        }
        else if (typ == typeof(byte))
        {
            var val = (byte)arg;
            var strval = $@"{val}";

            if (isBool)
            {
                bool bVar = false;

                if (val > 0)
                    bVar = true;

                ImGui.AlignTextToFramePadding();
                if (ImGui.Checkbox($"##value{name}", ref bVar))
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
                if (ImGui.InputText($"##value{name}", ref strval, 3))
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
            var val = (bool)arg;

            ImGui.AlignTextToFramePadding();
            if (ImGui.Checkbox($"##value{name}", ref val))
            {
                newValue = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(float))
        {
            var val = (float)arg;

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputFloat($"##value{name}", ref val))
            {
                newValue = val;
                isChanged = true;
            }
        }
        else if (typ == typeof(string))
        {
            var val = (string)arg;
            if (val == null)
            {
                val = "";
            }

            ImGui.AlignTextToFramePadding();
            if (ImGui.InputText($"##value{name}", ref val, 99))
            {
                newValue = val;
                isChanged = true;
            }
        }
        else if(typ.BaseType == typeof(Enum))
        {
            Array enumVals = typ.GetEnumValues();
            var enumNames = typ.GetEnumNames();
            var intVals = new int[enumVals.Length];

            if (typ.GetEnumUnderlyingType() == typeof(byte))
            {
                var val = (byte)arg;

                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (byte)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, val, out var retValue, intVals))
                {
                    newValue = retValue;
                    isChanged = true;
                }
            }
            else if (typ.GetEnumUnderlyingType() == typeof(int))
            {
                var val = (int)arg;

                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (int)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, val, out var retValue, intVals))
                {
                    newValue = retValue;
                    isChanged = true;
                }
            }
            else if (typ.GetEnumUnderlyingType() == typeof(uint))
            {
                var val = (uint)arg;

                for (var i = 0; i < enumVals.Length; i++)
                {
                    intVals[i] = (int)(uint)enumVals.GetValue(i);
                }

                if (Utils.EnumEditor(enumVals, enumNames, val, out var retValue, intVals))
                {
                    newValue = retValue;
                    isChanged = true;
                }
            }
            else
            {
                ImGui.Text("ImplementMe");
            }
        }
        else
        {
            ImGui.Text("ImplementMe");
        }

        var isDeactivatedAfterEdit = ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive();

        return (isChanged, isDeactivatedAfterEdit);
    }
}
