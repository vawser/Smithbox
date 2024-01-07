using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER0
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Dummy
        {
            public Vector3 Position;

            public Vector3 Forward;

            public Vector3 Upward;

            public short ReferenceID;

            public short DummyBoneIndex;

            public short AttachBoneIndex;

            public int Unk0C;

            public bool Flag1, Flag2;

            internal Dummy(BinaryReaderEx br)
            {
                Position = br.ReadVector3();
                Unk0C = br.ReadInt32();
                Forward = br.ReadVector3();
                ReferenceID = br.ReadInt16();
                DummyBoneIndex = br.ReadInt16();
                Upward = br.ReadVector3();
                AttachBoneIndex = br.ReadInt16();
                Flag1 = br.ReadBoolean();
                Flag2 = br.ReadBoolean();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
