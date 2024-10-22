using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// A grouping of edge members.
        /// </summary>
        public class EdgeMemberInfoGroup : List<EdgeMemberInfo>
        {
            /// <summary>
            /// Creates an empty <see cref="EdgeMemberInfoGroup"/>.
            /// </summary>
            internal EdgeMemberInfoGroup(){}

            /// <summary>
            /// Read an <see cref="EdgeMemberInfoGroup"/> from a stream.
            /// </summary>
            /// <param name="br">A <see cref="BinaryReaderEx"/>.</param>
            /// <param name="faceIndexes">An array to place decompressed face indexes into.</param>
            /// <exception cref="InvalidDataException">The member index buffer length was invalid.</exception>
            internal EdgeMemberInfoGroup(BinaryReaderEx br, List<int> faceIndexes)
            {
                long start = br.Position;
                short memberCount = br.ReadInt16();
                br.ReadInt16();
                br.ReadInt16();
                br.ReadByte();
                bool unk07 = br.ReadBoolean();
                br.AssertInt32(0); // Members offset?
                int memberIndexBufferLength = br.ReadInt32(); // The length of all edge index buffer data plus padding.

                if (!unk07 && memberIndexBufferLength != 0)
                {
                    throw new InvalidDataException($"{nameof(memberIndexBufferLength)} must be 0 when {nameof(unk07)} is false.");
                }

                for (int i = 0; i < memberCount; i++)
                {
                    Add(new EdgeMemberInfo(br));
                }

                for (int i = 0; i < memberCount; i++)
                {
                    var indexes = this[i].GetFaceIndexes(br, start);
                    for (int j = 0; j < indexes.Count; j++)
                    {
                        faceIndexes.Add(indexes[i]);
                    }
                }
            }

            /// <summary>
            /// Get all of the vertexes of the members in the group.
            /// </summary>
            /// <param name="br">A <see cref="BinaryReaderEx"/>.</param>
            /// <param name="vertexBuffersStartOffset">The starting offset of the groups.</param>
            /// <param name="vertexes">A list of vertexes to add the read vertexes to.</param>
            internal void GetVertexes(BinaryReaderEx br, long vertexBuffersStartOffset, List<FLVER.Vertex> vertexes)
            {
                foreach (var member in this)
                {
                    vertexes.AddRange(member.GetVertexes(br, vertexBuffersStartOffset));
                }
            }
        }
    }
}
