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
            public List<Member> Members1 { get; set; }
            
            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Member> Members2 { get; set; }

            /// <summary>
            /// Creates an empty SekiroUnkStruct.
            /// </summary>
            public SekiroUnkStruct()
            {
                Members1 = new List<Member>();
                Members2 = new List<Member>();
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
                    Members1 = new List<Member>(count1);
                    for (int i = 0; i < count1; i++)
                        Members1.Add(new Member(br));
                }
                br.StepOut();

                br.StepIn(offset2);
                {
                    Members2 = new List<Member>(count2);
                    for (int i = 0; i < count2; i++)
                        Members2.Add(new Member(br));
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
                foreach (Member member in Members1)
                    member.Write(bw);

                bw.FillUInt32("SekiroUnkOffset2", (uint)bw.Position);
                foreach (Member member in Members2)
                    member.Write(bw);
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Member
            {
                /// <summary>
                /// Unknown; maybe bone indices? Length 4.
                /// </summary>
                public short[] Unk00 { get; private set; }

                /// <summary>
                /// Unknown; seems to just count up from 0.
                /// </summary>
                public int Index { get; set; }

                /// <summary>
                /// Creates a Member with default values.
                /// </summary>
                public Member()
                {
                    Unk00 = new short[4];
                }

                internal Member(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt16s(4);
                    Index = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16s(Unk00);
                    bw.WriteInt32(Index);
                    bw.WriteInt32(0);
                }
            }
        }
    }
}
