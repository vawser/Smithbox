using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    /// <summary>
    /// Defines static decals in DS3 maps. Extension: .pmdcl
    /// </summary>
    public class PMDCL : SoulsFile<PMDCL>
    {
        /// <summary>
        /// Decals in this map.
        /// </summary>
        public List<Decal> Decals;

        /// <summary>
        /// Creates a new PMDCL with no decals.
        /// </summary>
        public PMDCL()
        {
            Decals = new List<Decal>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            long decalCount = br.ReadInt64();
            // Header size/offsets offset
            br.AssertInt64(0x20);
            br.AssertInt64(0);
            br.AssertInt64(0);

            Decals = new List<Decal>((int)decalCount);
            for (int i = 0; i < decalCount; i++)
            {
                long offset = br.ReadInt64();
                br.StepIn(offset);
                {
                    Decals.Add(new Decal(br));
                }
                br.StepOut();
            }
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = false;

            bw.WriteInt64(Decals.Count);
            bw.WriteInt64(0x20);
            bw.WriteInt64(0);
            bw.WriteInt64(0);

            for (int i = 0; i < Decals.Count; i++)
                bw.ReserveInt64($"Decal{i}");

            bw.Pad(0x20);
            for (int i = 0; i < Decals.Count; i++)
            {
                bw.FillInt64($"Decal{i}", bw.Position);
                Decals[i].Write(bw);
            }
        }

        /// <summary>
        /// Effects such as blood spatter that are applied on nearby surfaces.
        /// </summary>
        public class Decal
        {
            /// <summary>
            /// Unknown. Might not even be floats.
            /// </summary>
            public Vector3 XAngles, YAngles, ZAngles;

            /// <summary>
            /// Coordinates of the decal.
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Unknown, 1 or 0 in existing files.
            /// </summary>
            public float Unk3C;

            /// <summary>
            /// ID of a row in DecalParam.
            /// </summary>
            public int DecalParamID;

            /// <summary>
            /// Controls the size of the decal in ways that are not entirely clear to me.
            /// </summary>
            public short Size1, Size2;

            /// <summary>
            /// Creates a new Decal with the given decal param and position, and other values default.
            /// </summary>
            public Decal(int decalParamID, Vector3 position)
            {
                XAngles = Vector3.Zero;
                YAngles = Vector3.Zero;
                ZAngles = Vector3.Zero;
                Position = position;
                Unk3C = 1;
                DecalParamID = decalParamID;
                Size1 = 10;
                Size2 = 10;
            }

            /// <summary>
            /// Creates a new Decal with values copied from another.
            /// </summary>
            public Decal(Decal clone)
            {
                XAngles = clone.XAngles;
                YAngles = clone.YAngles;
                ZAngles = clone.ZAngles;
                Position = clone.Position;
                Unk3C = clone.Unk3C;
                DecalParamID = clone.DecalParamID;
                Size1 = clone.Size1;
                Size2 = clone.Size2;
            }

            internal Decal(BinaryReaderEx br)
            {
                XAngles = br.ReadVector3();
                br.AssertInt32(0);
                YAngles = br.ReadVector3();
                br.AssertInt32(0);
                ZAngles = br.ReadVector3();
                br.AssertInt32(0);
                Position = br.ReadVector3();
                Unk3C = br.ReadSingle();
                DecalParamID = br.ReadInt32();
                Size1 = br.ReadInt16();
                Size2 = br.ReadInt16();
                br.AssertInt64(0);
                br.AssertInt64(0);
                br.AssertInt64(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteVector3(XAngles);
                bw.WriteInt32(0);
                bw.WriteVector3(YAngles);
                bw.WriteInt32(0);
                bw.WriteVector3(ZAngles);
                bw.WriteInt32(0);
                bw.WriteVector3(Position);
                bw.WriteSingle(Unk3C);
                bw.WriteInt32(DecalParamID);
                bw.WriteInt16(Size1);
                bw.WriteInt16(Size2);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
            }
        }
    }
}
