using HKLib.hk2018;
using ImGuiNET;
using StudioCore.HavokEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Framework;

public class HavokPropertyEdit
{
    private HavokEditorScreen Screen;

    public HavokPropertyEdit(HavokEditorScreen screen)
    {
        Screen = screen;
    }

    // Handle all property types here
    public object EditProperty(string key, object property)
    {
        object newProperty = property;

        if (property.GetType() == typeof(string))
        {
            var newEntry = (string)property;
            ImGui.AlignTextToFramePadding();
            ImGui.InputText($"##textInput_{key}", ref newEntry, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                Screen.Selection.GetContainer().IsModified = true;
                return newEntry;
            }
        }

        if (property.GetType() == typeof(Vector4))
        {
            var vector = (Vector4)property;
            ImGui.AlignTextToFramePadding();
            ImGui.InputFloat4($"##float4_{key}", ref vector);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                Screen.Selection.MarkAsModified();
                return vector;
            }
        }

        // TODO: Use JSON enum stuff 
        if (property.GetType() == typeof(hkbTransitionEffect.EventMode))
        {
            var propEnum = (hkbTransitionEffect.EventMode)property;
            var newValue = property;

            if (ImGui.BeginCombo($"##enumDropDown_{key}", property.ToString()))
            {
                foreach (var val in Enum.GetValues(propEnum.GetType()))
                {
                    if (ImGui.Selectable($"{val}"))
                    {
                        Screen.Selection.MarkAsModified();
                        newValue = (hkbTransitionEffect.EventMode)val;
                        break;
                    }
                }

                ImGui.EndCombo();
            }

            return newValue;
        }

        if (property.GetType() == typeof(List<string>))
        {
            var list = (List<string>)property;
            var newList = property;

            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                var newEntry = entry;
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##textInput_{key}_{i}", ref newEntry, 255);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    Screen.Selection.MarkAsModified();
                    list[i] = newEntry;
                }
                if (i > 0)
                {
                    ImGui.SameLine();
                    if (ImGui.Button($"Remove##removeButton_{key}_{i}"))
                    {
                        Screen.Selection.MarkAsModified();
                        list.Remove(entry);
                        break;
                    }
                }
            }
            if (ImGui.Button($"Add##addButton_{key}"))
            {
                Screen.Selection.MarkAsModified();
                list.Add("");
            }

            newList = list;

            return newList;
        }

        return newProperty;
    }
}
