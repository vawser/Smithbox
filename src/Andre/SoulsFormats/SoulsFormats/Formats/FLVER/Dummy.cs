using System.Drawing;
using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// "Dummy polygons" used for hit detection, particle effect locations, and much more.
        /// </summary>
        public class Dummy
        {
            /// <summary>
            /// Location of the dummy point.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Vector indicating the dummy point's forward direction.
            /// </summary>
            public Vector3 Forward { get; set; }

            /// <summary>
            /// Vector indicating the dummy point's upward direction.
            /// </summary>
            public Vector3 Upward { get; set; }

            /// <summary>
            /// Indicates the type of dummy point this is (hitbox, sfx, etc).
            /// </summary>
            public short ReferenceID { get; set; }

            /// <summary>
            /// Index of a bone that the dummy point is initially transformed to before binding to the attach bone.
            /// </summary>
            public short ParentBoneIndex { get; set; }

            /// <summary>
            /// Index of the bone that the dummy point follows physically.
            /// </summary>
            public short AttachBoneIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public Color Color { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Flag1 { get; set; }

            /// <summary>
            /// If false, the upward vector is not read.
            /// </summary>
            public bool UseUpwardVector { get; set; }

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk30 { get; set; }

            /// <summary>
            /// Unknown; only used in Sekiro.
            /// </summary>
            public int Unk34 { get; set; }

            /// <summary>
            /// Creates a new dummy point with default values.
            /// </summary>
            public Dummy()
            {
                ParentBoneIndex = -1;
                AttachBoneIndex = -1;
            }

            /// <summary>
            /// Returns a string representation of the dummy.
            /// </summary>
            public override string ToString()
            {
                return $"{ReferenceID}";
            }

            internal Dummy(BinaryReaderEx br, int version)
            {
                Position = br.ReadVector3();
                // Not certain about the ordering of RGB here
                if (version == 0x20010)
                    Color = br.ReadBGRA();
                else
                    Color = br.ReadARGB();
                Forward = br.ReadVector3();
                ReferenceID = br.ReadInt16();
                ParentBoneIndex = br.ReadInt16();
                Upward = br.ReadVector3();
                AttachBoneIndex = br.ReadInt16();
                Flag1 = br.ReadBoolean();
                UseUpwardVector = br.ReadBoolean();
                Unk30 = br.ReadInt32();
                Unk34 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            internal void Write(BinaryWriterEx bw, int version)
            {
                bw.WriteVector3(Position);
                if (version == 0x20010)
                    bw.WriteBGRA(Color);
                else
                    bw.WriteARGB(Color);
                bw.WriteVector3(Forward);
                bw.WriteInt16(ReferenceID);
                bw.WriteInt16(ParentBoneIndex);
                bw.WriteVector3(Upward);
                bw.WriteInt16(AttachBoneIndex);
                bw.WriteBoolean(Flag1);
                bw.WriteBoolean(UseUpwardVector);
                bw.WriteInt32(Unk30);
                bw.WriteInt32(Unk34);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
        }
    }
}
