using DotNext;
using Google.Protobuf.WellKnownTypes;
using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using SixLabors.ImageSharp.Metadata.Profiles.Iptc;
using StudioCore.Core;
using StudioCore.Formats.PureFLVER.FLVER2;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor
{
    public class GXDataEditor
    {
        public GXDataEditor() { }

        public byte[] DisplayProperties_GXItem_HandleData(FLVER2.GXItem item)
        {
            byte[] newData = new byte[item.Data.Length];

            // ER Mappings
            if (Smithbox.ProjectType is ProjectType.ER)
            {
                switch (item.ID)
                {
                    default:
                        newData = DataSection_Default(item.Data);
                        break;
                }
            }
            else if (Smithbox.ProjectType is ProjectType.AC6)
            {
                switch (item.ID)
                {
                    default:
                        newData = DataSection_Default(item.Data);
                        break;
                }
            }
            else if (Smithbox.ProjectType is ProjectType.SDT)
            {
                switch (item.ID)
                {
                    default:
                        newData = DataSection_Default(item.Data);
                        break;
                }
            }
            else if (Smithbox.ProjectType is ProjectType.DS3)
            {
                switch (item.ID)
                {
                    default:
                        newData = DataSection_Default(item.Data);
                        break;
                }
            }
            else if (Smithbox.ProjectType is ProjectType.DS2)
            {
                switch (item.ID)
                {
                    default:
                        newData = DataSection_Default(item.Data);
                        break;
                }
            }
            else
            {
                newData = DataSection_Default(item.Data);
            }

            return newData;
        }

        // Unmapped Data
        public byte[] DataSection_Default(byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = DataGroup_Byte(data[i], i);
            }

            return data;
        }

        // Presentation Group - Byte
        public byte DataGroup_Byte(byte data, int index)
        {
            int curValue = data;

            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Byte");
            ImguiUtils.ShowHoverTooltip("");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.InputInt($"##data_{index}", ref curValue, 255);

            ImGui.Columns(1);

            if (curValue > byte.MaxValue)
                curValue = byte.MaxValue;

            return (byte)curValue;
        }

        // Presentation Group - Short
        public short DataGroup_Byte(short data, int index)
        {
            int curValue = data;

            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Short");
            ImguiUtils.ShowHoverTooltip("");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.InputInt($"##data_{index}", ref curValue, 255);

            ImGui.Columns(1);

            if (curValue > short.MaxValue)
                curValue = short.MaxValue;

            return (short)curValue;
        }

        // Presentation Group - Int
        public int DataGroup_Int(int data, int index)
        {
            int curValue = data;

            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Int");
            ImguiUtils.ShowHoverTooltip("");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.InputInt($"##data_{index}", ref curValue, 255);

            ImGui.Columns(1);

            return curValue;
        }

        // Presentation Group - Float
        public float DataGroup_Float(float data, int index)
        {
            float curValue = data;

            ImGui.Columns(2);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Float");
            ImguiUtils.ShowHoverTooltip("");

            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.InputFloat($"##data_{index}", ref curValue, 255);

            ImGui.Columns(1);

            return curValue;
        }
    }
}
