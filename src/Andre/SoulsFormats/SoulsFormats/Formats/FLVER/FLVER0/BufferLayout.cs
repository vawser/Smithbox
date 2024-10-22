using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class FLVER0
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class BufferLayout : List<FLVER.LayoutMember>
        {
            /// <summary>
            /// The total size of all ValueTypes in this layout.
            /// </summary>
            public int Size => this.Sum(member => member.Size);

            internal BufferLayout(BinaryReaderEx br) : base()
            {
                short memberCount = br.ReadInt16();
                short structSize = br.ReadInt16();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                int structOffset = 0;
                Capacity = memberCount;
                for (int i = 0; i < memberCount; i++)
                {
                    var member = new FLVER.LayoutMember(br, structOffset);
                    structOffset += member.Size;
                    Add(member);
                }

                if (Size != structSize)
                    throw new InvalidDataException("Mismatched buffer layout size.");
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
