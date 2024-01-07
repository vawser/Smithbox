using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// Bones available for vertices to be weighted to.
        /// </summary>
        public class Bone
        {
            /// <summary>
            /// Corresponds to the name of a bone in the parent skeleton. May also have a dummy name.
            /// </summary>
            public string Name { get; set; }

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
            /// Translation of this bone.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of this bone; euler radians.
            /// </summary>
            [RotationRadians]
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Scale of this bone.
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Minimum extent of the vertices weighted to this bone.
            /// </summary>
            public Vector3 BoundingBoxMin { get; set; }

            /// <summary>
            /// Maximum extent of the vertices weighted to this bone.
            /// </summary>
            public Vector3 BoundingBoxMax { get; set; }

            /// <summary>
            /// Unknown; only 0 or 1 before Sekiro.
            /// </summary>
            public int Unk3C { get; set; }

            /// <summary>
            /// Creates a new Bone with default values.
            /// </summary>
            public Bone()
            {
                Name = "";
                ParentIndex = -1;
                ChildIndex = -1;
                NextSiblingIndex = -1;
                PreviousSiblingIndex = -1;
                Scale = Vector3.One;
            }

            internal Bone(BinaryReaderEx br, FLVER2Header header)
            {
                Position = br.ReadVector3();
                int nameOffset = br.ReadInt32();
                Rotation = br.ReadVector3();
                ParentIndex = br.ReadInt16();
                ChildIndex = br.ReadInt16();
                Scale = br.ReadVector3();
                NextSiblingIndex = br.ReadInt16();
                PreviousSiblingIndex = br.ReadInt16();
                BoundingBoxMin = br.ReadVector3();
                Unk3C = br.ReadInt32();
                BoundingBoxMax = br.ReadVector3();
                br.AssertPattern(0x34, 0x00);

                if (header.Unicode)
                    Name = br.GetUTF16(nameOffset);
                else
                    Name = br.GetShiftJIS(nameOffset);
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteVector3(Position);
                bw.ReserveInt32($"BoneName{index}");
                bw.WriteVector3(Rotation);
                bw.WriteInt16(ParentIndex);
                bw.WriteInt16(ChildIndex);
                bw.WriteVector3(Scale);
                bw.WriteInt16(NextSiblingIndex);
                bw.WriteInt16(PreviousSiblingIndex);
                bw.WriteVector3(BoundingBoxMin);
                bw.WriteInt32(Unk3C);
                bw.WriteVector3(BoundingBoxMax);
                bw.WritePattern(0x34, 0x00);
            }

            internal void WriteStrings(BinaryWriterEx bw, FLVER2Header header, int index)
            {
                bw.FillInt32($"BoneName{index}", (int)bw.Position);
                if (header.Unicode)
                    bw.WriteUTF16(Name, true);
                else
                    bw.WriteShiftJIS(Name, true);
            }

            /// <summary>
            /// Returns the name of this bone.
            /// </summary>
            public override string ToString()
            {
                return Name;
            }
        }
    }
}
