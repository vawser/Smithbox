using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats
{
    public partial class FLVER2
    {
        /// <summary>
        /// An edge member structure of some kind with information on edge compressed index and vertex buffers.
        /// </summary>
        public class EdgeMemberInfo
        {
            /// <summary>
            /// The total length of the edge index buffer.
            /// </summary>
            internal int EdgeIndexesLength { get; private set; }

            /// <summary>
            /// The offset to the edge index buffer.
            /// </summary>
            internal int EdgeIndexesOffset { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            internal byte Unk10 { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            internal byte Unk11 { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            internal byte Unk12 { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            internal byte Unk13 { get; private set; }

            /// <summary>
            /// The index all decompressed indexes are based from.<br/>
            /// Add this to all decompressed indexes.
            /// </summary>
            internal ushort BaseIndex { get; private set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            internal short Unk16 { get; private set; }

            /// <summary>
            /// The length of the vertices used by this member in the edge vertex buffer plus padding.
            /// </summary>
            internal int EdgeVertexBufferLength { get; private set; }

            /// <summary>
            /// The offset of the vertices used by this member in the edge vertex buffer.
            /// </summary>
            internal int EdgeVertexBufferOffset { get; private set; }

            /// <summary>
            /// SPU configuration information for edge geometry.
            /// </summary>
            internal EdgeGeomSpuConfigInfo SpuConfigInfo { get; private set; }

            /// <summary>
            /// Read a member from a stream.
            /// </summary>
            /// <param name="br">A <see cref="BinaryReaderEx"/>.</param>
            internal EdgeMemberInfo(BinaryReaderEx br)
            {
                EdgeIndexesLength = br.ReadInt32();
                EdgeIndexesOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk10 = br.ReadByte();
                Unk11 = br.ReadByte();
                Unk12 = br.ReadByte();
                Unk13 = br.ReadByte();
                BaseIndex = br.ReadUInt16();
                Unk16 = br.ReadInt16();
                EdgeVertexBufferLength = br.ReadInt32();
                EdgeVertexBufferOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                SpuConfigInfo = new EdgeGeomSpuConfigInfo(br);
            }

            #region Indexes

            internal List<int> GetFaceIndexes(BinaryReaderEx br, long membersStartOffset)
            {
                var faceIndexes = new List<int>();
                br.Position = membersStartOffset + EdgeIndexesOffset;
                ushort[] indexes = DecompressIndexes(br, SpuConfigInfo.NumIndexes);
                for (int j = 0; j < SpuConfigInfo.NumIndexes; j++)
                    faceIndexes.Add(indexes[j] + BaseIndex);
                return faceIndexes;
            }

            public static ushort[] DecompressIndexes(BinaryReaderEx br, uint numIndexes)
            {
                ushort numIndexesInNBitStream = br.ReadUInt16(); // The number of delta compressed indexes, each taking bitsPerIndex number of bits.
                ushort baseDelta = br.ReadUInt16(); // The base delta, which was the minimum index after delta compressing which must be subtracted from each delta compressed index first.
                ushort num1Bytes = br.ReadUInt16(); // The number of bytes taken by the 1 bit stream for sequential indexes.
                byte bitsPerIndex = br.ReadByte(); // How many bits each delta index takes up.
                br.AssertByte(0); // Padding, should always be 0.

                // There are 8 bits per byte, so multiply the number of 1 bits bytes by 8.
                uint num1Bits = (uint)(num1Bytes * 8);

                // There are 3 indexes per triangle so divide the number of indexes by 3
                uint numTriangles = numIndexes / 3;

                // There are 2 bits per triangle, so double the number of triangles in the calculation
                // This is the number of triangle configurations, which are two bits each.
                // It must be padded to 8 bits, which I think adding 7 is supposed to do? Seen that in a python script.
                uint num2Bits = (numTriangles + numTriangles) + 7;

                // There are 8 bits per byte, so divide the number of 2 bits by 8.
                uint num2Bytes = num2Bits / 8;

                // Multiply the number of n bit indexes by the number of bits per index to get the number of n bits
                uint numNBits = (uint)(numIndexesInNBitStream * bitsPerIndex);

                // There are 8 bits per byte, so divide the number of n bits by 8.
                // Ensure the bit count is padded to 8 bits since they must be padded to bytes.
                uint numNBytes = (numNBits + (8 - (numNBits % 8))) / 8;

                // Read raw bytes
                var bit1Bytes = br.ReadBytes(num1Bytes);
                var bit2Bytes = br.ReadBytes((int)num2Bytes);
                var nbitBytes = br.ReadBytes((int)numNBytes);

                // Read actual data from bytes (Needs better implementation later)
                var bit1Table = bit1Bytes.ToBinary().FromBinary(1);
                var bit2Table = bit2Bytes.ToBinary().FromBinary(2);

                // If bits per index is 0, store 0s as the delta indexes?
                ushort[] deltaIndexes;
                if (bitsPerIndex == 0)
                {
                    deltaIndexes = new ushort[numIndexesInNBitStream];
                }
                else
                {
                    deltaIndexes = nbitBytes.ToBinary().FromBinaryToUInt16(bitsPerIndex);
                }

                // Decompress delta indexes in place.
                DecompressDeltaIndexes(deltaIndexes, baseDelta);

                // Add back the sequential indexes using the 1 bit stream.
                ushort[] newIndexes = DecompressSequentialIndexes(deltaIndexes, bit1Table);

                // Sort the triangle configurations given by the 2 bit stream.
                // The following is each configuration by bits,
                // Each number is a previous index, and N means the next New index.
                // 00 = 1 0 N
                // 01 = 0 2 N
                // 10 = 2 1 N
                // 11 = N N N
                int currentIndex = 0;
                var result = new List<ushort>((int)numIndexes);
                ushort[] triangleIndexes = new ushort[3];
                for (int i = 0; i < numTriangles; i++)
                {
                    var configValue = bit2Table[i];
                    switch (configValue)
                    {
                        case 0:
                            triangleIndexes[1] = triangleIndexes[2];
                            triangleIndexes[2] = newIndexes[currentIndex];
                            currentIndex++;
                            break;
                        case 1:
                            triangleIndexes[0] = triangleIndexes[2];
                            triangleIndexes[2] = newIndexes[currentIndex];
                            currentIndex++;
                            break;
                        case 2:
                            ushort tempIndex = triangleIndexes[0];
                            triangleIndexes[0] = triangleIndexes[1];
                            triangleIndexes[1] = tempIndex;
                            triangleIndexes[2] = newIndexes[currentIndex];
                            currentIndex++;
                            break;
                        case 3:
                            triangleIndexes[0] = newIndexes[currentIndex];
                            currentIndex++;
                            triangleIndexes[1] = newIndexes[currentIndex];
                            currentIndex++;
                            triangleIndexes[2] = newIndexes[currentIndex];
                            currentIndex++;
                            break;
                        default:
                            throw new Exception($"Unknown config value: {configValue}");
                    }
                    result.Add(triangleIndexes[0]);
                    result.Add(triangleIndexes[1]);
                    result.Add(triangleIndexes[2]);
                }

                // Finished
                return result.ToArray();
            }

            private static void DecompressDeltaIndexes(ushort[] indexes, ushort baseDelta)
            {
                for (int i = 0; i < indexes.Length; i++)
                {
                    indexes[i] -= baseDelta;
                }

                for (int i = 8; i < indexes.Length; i++)
                {
                    indexes[i] = (ushort)(indexes[i] + indexes[i - 8]);
                }
            }

            private static ushort[] DecompressSequentialIndexes(ushort[] indexes, byte[] bitStream)
            {
                ushort[] newIndexes = new ushort[bitStream.Length];

                ushort increment = 0;
                int indexesIndex = 0;
                for (int i = 0; i < bitStream.Length; i++)
                {
                    if (bitStream[i] == 1)
                    {
                        newIndexes[i] = indexes[indexesIndex];
                        indexesIndex++;
                    }
                    else
                    {
                        newIndexes[i] = increment;
                        increment++;
                    }
                }
                return newIndexes;
            }

            #endregion

            #region Vertexes

            internal List<FLVER.Vertex> GetVertexes(BinaryReaderEx br, long vertexBuffersStartOffset)
            {
                br.Position = vertexBuffersStartOffset + EdgeVertexBufferOffset;

                // I don't know what that extra buffer is when type is not 0, but it's important.
                // Positions are wrong usually when extracting types other than 0,
                // Armored Core Verdict Day also crashes immediately upon loading a model if the extra buffer is modified improperly.
                var edgeVertexBufferInfo = new EdgeVertexBufferInfo(br);

                var vertexes = new List<FLVER.Vertex>(SpuConfigInfo.NumVertexes);
                for (int i = 0; i < SpuConfigInfo.NumVertexes; i++)
                {
                    var vertex = new FLVER.Vertex();
                    vertex.Position = ReadFIXEDc3(br, edgeVertexBufferInfo.Multiplier, edgeVertexBufferInfo.Offset);
                    vertexes.Add(vertex);
                }

                return vertexes;
            }

            internal Vector3 ReadFIXEDc3(BinaryReaderEx br, Vector4 multiplier, Vector4 offset)
                => new Vector3((br.ReadUInt16() * multiplier.X) + offset.X, (br.ReadUInt16() * multiplier.Y) + offset.Y, (br.ReadUInt16() * multiplier.Z) + offset.Z);

            #endregion
        }
    }
}
