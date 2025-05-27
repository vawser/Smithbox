using Hexa.NET.ImGui;
using StudioCore;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorFieldInput
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorFieldInput(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public unsafe void DisplayFieldInput(object data, int index, FieldInfo field)
    {
        var inputWidth = 400f;

        var imguiID = $"field{index}";
        var inputFlags = ImGuiInputTextFlags.None;

        object? curValue = field.GetValue(data);
        string fieldName = field.Name;

        var wasChanged = false;
        var commitChange = false;
        var newValue = curValue;

        ImGui.SetNextItemWidth(inputWidth);

        // Long
        //if (field.FieldType == typeof(ulong))
        //{
        //    var tempValue = (ulong)curValue;
        //    var stringValue = $@"{tempValue}";

        //    if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
        //    {
        //        var result = ulong.TryParse(stringValue, out tempValue);
        //        if (result)
        //        {
        //            newValue = tempValue;
        //            wasChanged = true;
        //        }
        //    }
        //}

        // Signed Long
        if (field.FieldType == typeof(long))
        {
            var tempValue = (long)curValue;
            var stringValue = $@"{tempValue}";

            if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
            {
                var result = long.TryParse(stringValue, out tempValue);
                if (result)
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        // Signed Integer
        if (field.FieldType == typeof(int))
        {
            var tempValue = (int)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Integer
        if (field.FieldType == typeof(uint))
        {
            var tempValue = (uint)curValue;
            var stringValue = $@"{tempValue}";

            if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
            {
                var result = uint.TryParse(stringValue, out tempValue);
                if (result)
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        // Signed Short
        if (field.FieldType == typeof(short))
        {
            int tempValue = (short)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Short
        if (field.FieldType == typeof(ushort))
        {
            var tempValue = (ushort)curValue;
            var stringValue = $@"{tempValue}";

            if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
            {
                var result = ushort.TryParse(stringValue, out tempValue);
                if (result)
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        // Signed Byte
        if (field.FieldType == typeof(sbyte))
        {
            int tempValue = (sbyte)curValue;

            if (ImGui.InputInt($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Unsigned Byte
        if (field.FieldType == typeof(byte))
        {
            var tempValue = (byte)curValue;
            var stringValue = $@"{tempValue}";

            if (ImGui.InputText($"##value_{imguiID}", ref stringValue, 128, inputFlags))
            {
                var result = byte.TryParse(stringValue, out tempValue);
                if (result)
                {
                    newValue = tempValue;
                    wasChanged = true;
                }
            }
        }

        // Boolean
        if (field.FieldType == typeof(bool))
        {
            var tempValue = (bool)curValue;

            if (ImGui.Checkbox($"##value_{imguiID}", ref tempValue))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Float
        if (field.FieldType == typeof(float))
        {
            var tempValue = (float)curValue;
            var format = CreateFloatFormat(tempValue);
            var formatPtr = InterfaceUtils.StringToUtf8(format);

            if (ImGui.InputFloat($"##value_{imguiID}", ref tempValue, 0.1f, 1.0f, formatPtr, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }

            InterfaceUtils.FreeUtf8(formatPtr);
        }

        // Double
        if (field.FieldType == typeof(double))
        {
            var tempValue = (double)curValue;
            double step = 0.1;
            double stepFast = 1.0;
            var format = CreateFloatFormat((float)tempValue);
            byte* formatPtr = InterfaceUtils.StringToUtf8(format);

            if (ImGui.InputScalar($"##value_{imguiID}", ImGuiDataType.Double, &tempValue, &step, &stepFast, formatPtr, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // String
        if (field.FieldType == typeof(string))
        {
            var tempValue = (string)curValue;

            if (tempValue == null)
            {
                tempValue = "";
            }

            if (ImGui.InputText($"##value_{imguiID}", ref tempValue, 128, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector2
        if (field.FieldType == typeof(Vector2))
        {
            var tempValue = (Vector2)curValue;

            if (ImGui.InputFloat2($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector3
        if (field.FieldType == typeof(Vector3))
        {
            var tempValue = (Vector3)curValue;

            if (ImGui.InputFloat3($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // Vector4
        if (field.FieldType == typeof(Vector4))
        {
            var tempValue = (Vector4)curValue;

            if (ImGui.InputFloat4($"##value_{imguiID}", ref tempValue, inputFlags))
            {
                newValue = tempValue;
                wasChanged = true;
            }
        }

        // TODO: change this to select the target object, rather than displaying its fields in a tree view
        if (!field.FieldType.IsPrimitive &&
            field.FieldType != typeof(string) &&
            !field.FieldType.IsEnum &&
            !field.FieldType.IsArray &&
            !typeof(System.Collections.IEnumerable).IsAssignableFrom(field.FieldType))
        {
            if (curValue != null)
            {
                if (ImGui.TreeNode($"{fieldName}##class_{imguiID}"))
                {
                    var nestedFields = field.FieldType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    int subIndex = 0;
                    foreach (var nestedField in nestedFields)
                    {
                        if (nestedField.IsPublic)
                        {
                            DisplayFieldInput(curValue, subIndex++, nestedField);
                        }
                    }

                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text($"{fieldName}: null");
            }
        }

        // TODO: handle list/arrays
        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(field.FieldType) && field.FieldType != typeof(string))
        {
            if (curValue is System.Collections.IList list)
            {
                if (ImGui.TreeNode($"{fieldName}##list_{imguiID}"))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        var item = list[i];

                        if (item != null)
                        {
                            var itemType = item.GetType();
                            var itemFields = itemType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                            if (ImGui.TreeNode($"[{i}]##item_{imguiID}_{i}"))
                            {
                                int subIndex = 0;
                                foreach (var itemField in itemFields)
                                {
                                    if (itemField.IsPublic)
                                    {
                                        DisplayFieldInput(item, subIndex++, itemField);
                                    }
                                }
                                ImGui.TreePop();
                            }
                        }
                        else
                        {
                            ImGui.Text($"[{i}] = null");
                        }
                    }

                    ImGui.TreePop();
                }
            }
            else
            {
                ImGui.Text($"{fieldName}: (not IList)");
            }
        }

        // TODO: handle enums
        if (field.FieldType.IsEnum)
        {
            var enumNames = Enum.GetNames(field.FieldType);
            var enumValues = Enum.GetValues(field.FieldType);
            int currentIndex = Array.IndexOf(enumValues, curValue);

            if (ImGui.Combo($"##value_{imguiID}", ref currentIndex, enumNames, enumNames.Length))
            {
                newValue = enumValues.GetValue(currentIndex);
                wasChanged = true;
            }
        }

        commitChange = ImGui.IsItemDeactivatedAfterEdit();

        // Apply action
        if (commitChange && wasChanged)
        {
            var changeAction = new BehaviorFieldChange(field, data, curValue, newValue);
            Editor.ActionManager.ExecuteAction(changeAction);
        }
    }

    /// <summary>
    /// Helper for the float formatting in the InputFloat input elements
    /// </summary>
    /// <param name="f"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public unsafe string CreateFloatFormat(float f, int min = 3, int max = 6)
    {
        var split = f.ToString("F6").TrimEnd('0').Split('.');
        return $"%.{Math.Clamp(split.Last().Length, min, max)}f";
    }
}
