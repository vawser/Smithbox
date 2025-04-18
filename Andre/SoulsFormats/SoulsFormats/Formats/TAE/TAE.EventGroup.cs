using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.TAE;

namespace SoulsFormats
{
    public partial class TAE : SoulsFile<TAE>
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

            public EventGroup GetClone()
            {
                var g = new EventGroup(GroupType);
                g.GroupData = GroupData;
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

            public long heldVarInt;

            internal EventGroup(BinaryReaderEx br, List<long> eventHeaderOffsets, TAEFormat format)
            {
                long entryCount = br.ReadVarint();
                long valuesOffset = br.ReadVarint();
                long typeOffset = br.ReadVarint();
                if (format is not TAEFormat.DS1 && format is not TAEFormat.DESR)
                    br.AssertVarint(0);

                br.StepIn(typeOffset);
                {
                    GroupType = br.ReadVarint();
                    if (format is TAEFormat.SOTFS)
                    {
                        br.AssertVarint(br.Position + (br.VarintLong ? 8 : 4));
                        br.AssertVarint(0);
                        br.AssertVarint(0);
                    }
                    else if (format is TAEFormat.DS3 or TAEFormat.SDT)
                    {
                        heldVarInt = br.ReadVarint();
                    }
                    else
                    {
                        GroupData.DataType = (EventGroupDataType)GroupType;
                        long dataOffset = br.ReadVarint();
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
                if (format is not TAEFormat.DS1 && format is not TAEFormat.DESR)
                    bw.WriteVarint(0);
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
                    bw.WriteVarint(heldVarInt);
                }
                else
                {
                    bw.ReserveVarint("EventGroupDataOffset");
                    long dataStartPos = bw.Position;

                    if (GroupData.DataType == EventGroupDataType.ApplyToSpecificCutsceneEntity)
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
