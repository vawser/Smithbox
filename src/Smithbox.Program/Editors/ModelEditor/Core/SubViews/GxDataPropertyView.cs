using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Interface;

namespace StudioCore.Editors.ModelEditor.Core.Properties
{
    public class GxDataPropertyView
    {
        private ModelEditorScreen Editor;
        //private GxDescriptorBank GxItemDescriptors;

        public GxDataPropertyView(ModelEditorScreen screen)
        {
            Editor = screen;
            //GxItemDescriptors = screen.GxItemDescriptors;
        }

        public void Display(FLVER2.GXItem item)
        {
            // ER Mappings
            if (Editor.Project.ProjectType is ProjectType.ER)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else if (Editor.Project.ProjectType is ProjectType.AC6)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else if (Editor.Project.ProjectType is ProjectType.SDT)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else if (Editor.Project.ProjectType is ProjectType.DS3)
            {
                switch (item.ID)
                {
                    default:
                        DataSection_Default(item, item.Data);
                        break;
                }
            }
            else if (Editor.Project.ProjectType is ProjectType.DS2)
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

        public void DataSection_Default(FLVER2.GXItem itemEntry, byte[] data)
        {
            /*
            GXValue[] mappedData = GX.ToGxValues(data);

            var ID = itemEntry.ID;
            var Unk04 = itemEntry.Unk04;

            var descriptor = Screen.GxItemDescriptors.GetEntry(ID);

            foreach(var item in descriptor.Items)
            {
                DataGroup_ItemValueDescriptor(item);
            }
            */

            for (int i = 0; i < data.Length; i++)
            {
                DataGroup_Byte(itemEntry, data[i], i);
            }
        }

        /*
        public void DataGroup_ItemValueDescriptor(GX00ItemValueDescriptor entry)
        {
            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Name");
            UIHelper.ShowHoverTooltip("");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Type");
            UIHelper.ShowHoverTooltip("");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Min");
            UIHelper.ShowHoverTooltip("");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Max");
            UIHelper.ShowHoverTooltip("");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Enum");
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
        */

        // Presentation Group - Byte
        public void DataGroup_Byte(FLVER2.GXItem entry, byte data, int index)
        {
            int curValue = data;

            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Byte");
            UIHelper.Tooltip("");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.InputInt($"##data_{index}", ref curValue, 255);
            if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
            {
                if (entry.Data[index] != curValue)
                    Editor.EditorActionManager.ExecuteAction(
                        new UpdateProperty_FLVERGXList_GXItem_Data(entry, entry.Data[index], curValue, index));
            }

            ImGui.Columns(1);
        }
    }
}
