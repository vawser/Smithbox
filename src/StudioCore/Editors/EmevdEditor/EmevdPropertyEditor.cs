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

    public (bool, bool) PropertyRow(ArgDoc argDoc, object arg, out object newValue)
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
}
