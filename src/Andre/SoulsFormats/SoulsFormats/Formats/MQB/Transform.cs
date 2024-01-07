using System.Numerics;

namespace SoulsFormats
{
    public partial class MQB
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Transform
        {
            public float Frame { get; set; }

            public Vector3 Translation { get; set; }

            public Vector3 Unk10 { get; set; }

            public Vector3 Unk1C { get; set; }

            public Vector3 Rotation { get; set; }

            public Vector3 Unk34 { get; set; }

            public Vector3 Unk40 { get; set; }

            public Vector3 Scale { get; set; }

            public Vector3 Unk58 { get; set; }

            public Vector3 Unk64 { get; set; }

            public Transform()
            {
                Scale = Vector3.One;
            }

            internal Transform(BinaryReaderEx br)
            {
                Frame = br.ReadSingle();
                Translation = br.ReadVector3();
                Unk10 = br.ReadVector3();
                Unk1C = br.ReadVector3();
                Rotation = br.ReadVector3();
                Unk34 = br.ReadVector3();
                Unk40 = br.ReadVector3();
                Scale = br.ReadVector3();
                Unk58 = br.ReadVector3();
                Unk64 = br.ReadVector3();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteSingle(Frame);
                bw.WriteVector3(Translation);
                bw.WriteVector3(Unk10);
                bw.WriteVector3(Unk1C);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Unk34);
                bw.WriteVector3(Unk40);
                bw.WriteVector3(Scale);
                bw.WriteVector3(Unk58);
                bw.WriteVector3(Unk64);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
