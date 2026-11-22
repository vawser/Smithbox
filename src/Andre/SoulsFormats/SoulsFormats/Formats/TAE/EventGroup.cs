using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class TAE
    {

        /// <summary>
        /// A group of events in an animation with an associated EventType that does not necessarily match theirs.
        /// </summary>
        public class EventGroup
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public long GroupType { get; set; }

            public EventGroup()
            {

            }

            public EventGroupDataStruct GroupData;

            /// <summary>
            /// The 64-bit (SDT layout) group's data block as read, from its data offset up to its values; null when the
            /// offset was 0. AC6 type 192 groups have 16 bytes here. Not decoded.
            /// </summary>
            public byte[] RawGroupData { get; set; }

            public EventGroup GetClone()
            {
                var g = new EventGroup(GroupType);
                g.GroupData = GroupData;
                g.RawGroupData = RawGroupData?.ToArray();
                return g;
            }

            public enum EventGroupDataType : long
            {
                GroupData0 = 0,
                GroupData16 = 16,
                ApplyToSpecificCutsceneEntity = 128,
                GroupData192 = 192,
            }

            public struct EventGroupDataStruct
            {
                public EventGroupDataType DataType;
                public enum EntityTypes : ushort
                {
                    Character = 0,
                    Object = 1,
                    MapPiece = 2,
                    DummyNode = 4,
                }
                public EntityTypes CutsceneEntityType;
                public short CutsceneEntityIDPart1;
                public short CutsceneEntityIDPart2;
                public sbyte Area;
                public sbyte Block;
            }

            internal List<int> indices;

            /// <summary>
            /// Creates a new empty EventGroup with the given type.
            /// </summary>
            public EventGroup(long eventType)
            {
                GroupType = eventType;
                indices = new List<int>();
            }

            /// <summary>
            /// Whether the group has its data struct. Read groups keep what the file had: DES cutscene entity groups
            /// have none (data offset 0). New groups write it.
            /// </summary>
            private bool HasDataStruct = true;

            internal EventGroup(BinaryReaderEx br, List<long> eventHeaderOffsets, TAEFormat format)
            {
                long entryCount = br.ReadVarint();
                long valuesOffset = br.ReadVarint();
                long typeOffset = br.ReadVarint();
                if (format != TAEFormat.DS1 && format != TAEFormat.DESR)
                    br.AssertVarint(0);

                // DES group headers are 0x20 long: the four fields, then 16 zero bytes
                if (format == TAEFormat.DES)
                    br.AssertPattern(0x10, 0);

                br.StepIn(typeOffset);
                {
                    GroupType = br.ReadVarint();
                    if (format is TAEFormat.SOTFS)
                    {
                        br.AssertVarint(br.Position + (br.VarintLong ? 8 : 4));
                        br.AssertVarint(0);
                        br.AssertVarint(0);
                    }
                    else if (format == TAEFormat.DS3 || format == TAEFormat.SDT)
                    {
                        // A data offset, 0 for most groups. AC6 type 192 groups point it right after itself, at a
                        // data block that runs up to the group values (16 bytes, 01 01 01 00 …), kept as read.
                        long dataOffset = br.GetVarint(br.Position);

                        if (format == TAEFormat.SDT && dataOffset != 0 && dataOffset == br.Position + br.VarintSize
                            && valuesOffset > dataOffset)
                        {
                            br.ReadVarint();
                            RawGroupData = br.GetBytes(dataOffset, (int)(valuesOffset - dataOffset));
                        }
                        else
                        {
                            br.AssertVarint(0);
                        }
                    }
                    else
                    {
                        GroupData.DataType = (EventGroupDataType)GroupType;
                        long dataOffset = br.ReadVarint();
                        HasDataStruct = dataOffset != 0;
                        if (dataOffset != 0)
                        {
                            br.StepIn(dataOffset);
                            {
                                if (GroupData.DataType is EventGroupDataType.ApplyToSpecificCutsceneEntity)
                                {
                                    GroupData.CutsceneEntityType = br.ReadEnum16<EventGroupDataStruct.EntityTypes>();
                                    GroupData.CutsceneEntityIDPart1 = br.ReadInt16();
                                    GroupData.CutsceneEntityIDPart2 = br.ReadInt16();
                                    GroupData.Block = br.ReadSByte();
                                    GroupData.Area = br.ReadSByte();
                                    br.AssertInt32(0);
                                    br.AssertInt32(0);
                                }
                            }
                            br.StepOut();
                        }
                    }
                }
                br.StepOut();

                br.StepIn(valuesOffset);
                {
                    if (format == TAEFormat.SOTFS)
                        indices = br.ReadVarints((int)entryCount).Select(offset
                            => eventHeaderOffsets.FindIndex(headerOffset => headerOffset == offset)).ToList();
                    else
                        indices = br.ReadInt32s((int)entryCount).Select(offset
                            => eventHeaderOffsets.FindIndex(headerOffset => headerOffset == offset)).ToList();
                }
                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int i, int j, TAEFormat format)
            {
                bw.WriteVarint(indices.Count);
                bw.ReserveVarint($"EventGroupValuesOffset{i}:{j}");
                bw.ReserveVarint($"EventGroupTypeOffset{i}:{j}");
                if (format != TAEFormat.DS1 && format != TAEFormat.DESR)
                    bw.WriteVarint(0);

                if (format == TAEFormat.DES)
                    bw.WritePattern(0x10, 0);
            }

            internal void WriteData(BinaryWriterEx bw, int i, int j, List<long> eventHeaderOffsets, TAEFormat format)
            {
                bw.FillVarint($"EventGroupTypeOffset{i}:{j}", bw.Position);
                bw.WriteVarint(GroupType);

                if (format == TAEFormat.SOTFS)
                {
                    bw.WriteVarint(bw.Position + (bw.VarintLong ? 8 : 4));
                    bw.WriteVarint(0);
                    bw.WriteVarint(0);
                }
                else if (format == TAEFormat.DS3 || format == TAEFormat.SDT)
                {
                    if (format == TAEFormat.SDT && RawGroupData != null)
                    {
                        bw.WriteVarint(bw.Position + bw.VarintSize);
                        bw.WriteBytes(RawGroupData);
                    }
                    else
                    {
                        bw.WriteVarint(0);
                    }
                }
                else
                {
                    bw.ReserveVarint("EventGroupDataOffset");
                    long dataStartPos = bw.Position;
                    
                    if (GroupData.DataType == EventGroupDataType.ApplyToSpecificCutsceneEntity && HasDataStruct)
                    {
                        bw.WriteUInt16((ushort)(GroupData.CutsceneEntityType));
                        bw.WriteInt16(GroupData.CutsceneEntityIDPart1);
                        bw.WriteInt16(GroupData.CutsceneEntityIDPart2);
                        bw.WriteSByte(GroupData.Block);
                        bw.WriteSByte(GroupData.Area);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                    }

                    if (dataStartPos != bw.Position)
                    {
                        bw.FillVarint("EventGroupDataOffset", dataStartPos);
                    }
                    else
                    {
                        bw.FillVarint("EventGroupDataOffset", 0);
                    }


                    if ((int)GroupData.DataType != GroupType)
                    {
                        throw new InvalidDataException("TAE event group data is not for the correct type.");
                    }
                }

                // DES pads the type struct to 0x10 before the values
                if (format == TAEFormat.DES)
                    bw.Pad(0x10);

                bw.FillVarint($"EventGroupValuesOffset{i}:{j}", bw.Position);
                for (int k = 0; k < indices.Count; k++)
                {
                    if (format == TAEFormat.SOTFS)
                        bw.WriteVarint(eventHeaderOffsets[indices[k]]);
                    else
                        bw.WriteInt32((int)eventHeaderOffsets[indices[k]]);
                }

                if (format != TAEFormat.DS1)
                    bw.Pad(0x10);
            }
        }

    }
}
