﻿using ImGuiNET;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Interface;

namespace StudioCore.Editors.ModelEditor.Core.Properties
{
    public class GxDataPropertyView
    {
        private ModelEditorScreen Screen;
        private GxDescriptorBank GxItemDescriptors;

        public GxDataPropertyView(ModelEditorScreen screen)
        {
            Screen = screen;
            GxItemDescriptors = screen.GxItemDescriptors;
        }

        public void Display(FLVER2.GXItem item)
        {
            // ER Mappings
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else if (Smithbox.ProjectType is ProjectType.AC6)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else if (Smithbox.ProjectType is ProjectType.SDT)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else if (Smithbox.ProjectType is ProjectType.DS3)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else if (Smithbox.ProjectType is ProjectType.DS2)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else
            {
                DataSection_Default(item, item.Data);
            }
        }

        // Unmapped Data
        public void DataSection_Default(FLVER2.GXItem entry, byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                DataGroup_Byte(entry, data[i], i);
            }
        }

        // Presentation Group - Byte
        public void DataGroup_Byte(FLVER2.GXItem entry, byte data, int index)
        {
            int curValue = data;

            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Byte");
            UIHelper.ShowHoverTooltip("");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.InputInt($"##data_{index}", ref curValue, 255);
            if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
            {
                if (entry.Data[index] != curValue)
                    Screen.EditorActionManager.ExecuteAction(
                        new UpdateProperty_FLVERGXList_GXItem_Data(entry, entry.Data[index], curValue, index));
            }

            ImGui.Columns(1);
        }
    }
}