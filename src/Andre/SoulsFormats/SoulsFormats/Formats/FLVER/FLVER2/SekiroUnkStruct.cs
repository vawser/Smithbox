using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// Unknown; only present in Sekiro.
        /// </summary>
        public class SekiroUnkStruct
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Member1> Members1 { get; set; }
            
            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Member2> Members2 { get; set; }

            /// <summary>
            /// Creates an empty SekiroUnkStruct.
            /// </summary>
            public SekiroUnkStruct()
            {
                Members1 = new List<Member1>();
                Members2 = new List<Member2>();
            }

            internal SekiroUnkStruct(BinaryReaderEx br)
            {
                short count1 = br.ReadInt16();
                short count2 = br.ReadInt16();
                uint offset1 = br.ReadUInt32();
                uint offset2 = br.ReadUInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                br.StepIn(offset1);
                {
                    Members1 = new List<Member1>(count1);
                    for (int i = 0; i < count1; i++)
                        Members1.Add(new Member1(br));
                }
                br.StepOut();

                br.StepIn(offset2);
                {
                    Members2 = new List<Member2>(count2);
                    for (int i = 0; i < count2; i++)
                        Members2.Add(new Member2(br));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16((short)Members1.Count);
                bw.WriteInt16((short)Members2.Count);
                bw.ReserveUInt32("SekiroUnkOffset1");
                bw.ReserveUInt32("SekiroUnkOffset2");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillUInt32("SekiroUnkOffset1", (uint)bw.Position);
                foreach (Member1 member1 in Members1)
                    member1.Write(bw);

                bw.FillUInt32("SekiroUnkOffset2", (uint)bw.Position);
                foreach (Member2 member2 in Members2)
                    member2.Write(bw);
            }

            /// <summary>
            /// Unknown.
            /// References a bone index and perfectly matches its contained indices.
            /// </summary>
            public class Member1
            {
                /// <summary>
                /// Index of the parent in this FLVER's bone collection, or -1 for none.
                /// </summary>
                public short ParentIndex { get; set; }

                /// <summary>
                /// Index of the first child in this FLVER's bone collection, or -1 for none.
                /// </summary>
                public short ChildIndex { get; set; }

                /// <summary>
                /// Index of the next child of this bone's parent, or -1 for none.
                /// </summary>
                public short NextSiblingIndex { get; set; }

                /// <summary>
                /// Index of the previous child of this bone's parent, or -1 for none.
                /// </summary>
                public short PreviousSiblingIndex { get; set; }

                /// <summary>
                /// Unknown; seems to just count up from 0.
                /// </summary>
                public int BoneIndex { get; set; }

                /// <summary>
                /// Creates a Member with default values.
                /// </summary>
                public Member1()
                {
                    ParentIndex = -1;
                    ChildIndex = -1;
                    NextSiblingIndex = -1;
                    PreviousSiblingIndex = -1;
                }

                internal Member1(BinaryReaderEx br)
                {
                    ParentIndex = br.ReadInt16();
                    ChildIndex = br.ReadInt16();
                    NextSiblingIndex = br.ReadInt16();
                    PreviousSiblingIndex = br.ReadInt16();
                    BoneIndex = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(ParentIndex);
                    bw.WriteInt16(ChildIndex);
                    bw.WriteInt16(NextSiblingIndex);
                    bw.WriteInt16(PreviousSiblingIndex);
                    bw.WriteInt32(BoneIndex);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Member2
            {
                /// <summary>
                /// Index of the parent in this FLVER's bone collection, or -1 for none.
                /// </summary>
                public short Unk00 { get; set; }

                /// <summary>
                /// Index of the first child in this FLVER's bone collection, or -1 for none.
                /// </summary>
                public short Unk02 { get; set; }

                /// <summary>
                /// Index of the next child of this bone's parent, or -1 for none.
                /// </summary>
                public short Unk04 { get; set; }

                /// <summary>
                /// Index of the previous child of this bone's parent, or -1 for none.
                /// </summary>
                public short Unk06 { get; set; }

                /// <summary>
                /// Unknown; seems to just count up from 0.
                /// </summary>
                public int Index { get; set; }

                /// <summary>
                /// Creates a Member with default values.
                /// </summary>
                public Member2()
                {
                    Unk00 = -1;
                    Unk02 = -1;
                    Unk04 = -1;
                    Unk06 = -1;
                }

                internal Member2(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                    Unk04 = br.ReadInt16();
                    Unk06 = br.ReadInt16();
                    Index = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Unk00);
                    bw.WriteInt16(Unk02);
                    bw.WriteInt16(Unk04);
                    bw.WriteInt16(Unk06);
                    bw.WriteInt32(Index);
                    bw.WriteInt32(0);
                }
            }
        }
    }
}
