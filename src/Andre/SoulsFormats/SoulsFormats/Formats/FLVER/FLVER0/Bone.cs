using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER0
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class Bone
        {
            public string Name;

            public Vector3 Position;

            public Vector3 Rotation;

            public Vector3 Scale;

            public Vector3 BoundingBoxMin;

            public Vector3 BoundingBoxMax;

            public short ParentIndex;

            public short ChildIndex;

            public short NextSiblingIndex;

            public short PreviousSiblingIndex;

            internal Bone(BinaryReaderEx br, bool useUnicode)
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
                br.AssertInt32(0);
                BoundingBoxMax = br.ReadVector3();

                for (int i = 0; i < 13; i++)
                    br.AssertInt32(0);

                Name = useUnicode ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
