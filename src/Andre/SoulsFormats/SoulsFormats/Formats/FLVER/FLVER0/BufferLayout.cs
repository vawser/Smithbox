using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoulsFormats
{
    public partial class FLVER0
    {
        /// <summary>
        /// Determines which properties of a vertex are read and written, and in what order and format.
        /// </summary>
        public class BufferLayout : List<FLVER.LayoutMember>
        {
            /// <summary>
            /// The total size of all ValueTypes in this layout.
            /// </summary>
            public int Size => this.Sum(member => member.Size);

            /// <summary>
            /// Creates a new empty BufferLayout.
            /// </summary>
            public BufferLayout() : base() { }

            /// <summary>
            /// Clone an existing BufferLayout.
            /// </summary>
            public BufferLayout(BufferLayout bufferLayout)
            {
                Capacity = bufferLayout.Capacity;
                foreach (var member in bufferLayout)
                {
                    Add(new FLVER.LayoutMember(member));
                }
            }

            /// <summary>
            /// Read a BufferLayout from a stream.
            /// </summary>
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
                    var member = new FLVER.LayoutMember(br, structOffset, false);
                    structOffset += member.Size;
                    Add(member);
                }

                if (Size != structSize)
                    throw new InvalidDataException("Mismatched buffer layout size.");
            }
        }
    }
}
