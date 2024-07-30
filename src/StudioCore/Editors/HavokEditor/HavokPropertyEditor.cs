using HKLib.hk2018;
using ImGuiNET;
using StudioCore.HavokEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor;

public class HavokPropertyEditor
{
    private HavokEditorScreen Screen;

    public HavokPropertyEditor(HavokEditorScreen screen)
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
                Screen.SelectedContainerInfo.IsModified = true;
                return newEntry;
            }
        }

        if (property.GetType() == typeof(Vector4))
        {
            var vector = (Vector4)property;
            ImGui.AlignTextToFramePadding();
            ImGui.InputFloat4($"##float4_{key}", ref vector);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                Screen.SelectedContainerInfo.IsModified = true;
                return vector;
            }
        }

        if(property.GetType() == typeof(hkbTransitionEffect.EventMode)) 
        {
            var propEnum = (hkbTransitionEffect.EventMode)property;
            var newValue = property;

            if (ImGui.BeginCombo($"##enumDropDown_{key}", property.ToString()))
            {
                foreach (var val in Enum.GetValues(propEnum.GetType()))
                {
                    if(ImGui.Selectable($"{val}"))
                    {
                        Screen.SelectedContainerInfo.IsModified = true;
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

            for(int i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                var newEntry = entry;
                ImGui.AlignTextToFramePadding();
                ImGui.InputText($"##textInput_{key}_{i}", ref newEntry, 255);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    Screen.SelectedContainerInfo.IsModified = true;
                    list[i] = newEntry;
                }
                if (i > 0)
                {
                    ImGui.SameLine();
                    if (ImGui.Button($"Remove##removeButton_{key}_{i}"))
                    {
                        Screen.SelectedContainerInfo.IsModified = true;
                        list.Remove(entry);
                        break;
                    }
                }
            }
            if(ImGui.Button($"Add##addButton_{key}"))
            {
                Screen.SelectedContainerInfo.IsModified = true;
                list.Add("");
            }

            newList = list;

            return newList;
        }

        return newProperty;
    }
}
