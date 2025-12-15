using System.Numerics;
using System.Runtime.InteropServices;

namespace SoulsFormats.Utilities
{
    /// <summary>
    /// Helper methods for edge geometry.
    /// </summary>
    public static class EdgeGeom
    {
        #region Indices

        /// <summary>
        /// Decompress edge SPU face indices into an array of <see cref="ushort"/>.
        /// </summary>
        /// <param name="br">The stream reader.</param>
        /// <param name="numIndexes">The number of indices.</param>
        /// <returns>The decompressed face indices.</returns>
        public static ushort[] DecompressIndexes(BinaryReaderEx br, int numIndexes)
        {
            // First expand the n-bit stream into delta indexes for ease of use,
            // This requires knowing the number of bits per delta index and how many delta or unique indexes there are.
            // This gets the unique or non-sequential indexes that are delta compressed for smaller sizes.

            // Second decompress the delta indexes into unique or non-sequential indexes.
            // This requires knowing the base delta, or minimum value that was used to normalize them for better compression.
            // This decompresses the unique or non-sequential indexes, which are indexes that are not incrementing by 1 in order.

            // Third add the sequential indexes back to get the new face indexes.
            // This requires knowing the number of bytes in the 1-bit stream.
            // The 1-bit stream specifies for each new index if it was simply incremental by 1, or if it needs the next unique index.
            // This gets the new face indexes that aren't repeating for triangles.

            // Fourth use the 2-bit triangle config stream to see the rotation of each triangle and use the new indexes accordingly.
            // This gets the final resulting decompressed triangle indexes.

            ushort numIndexesInNBitStream = br.ReadUInt16(); // The number of delta compressed indexes, each taking bitsPerIndex number of bits.
            ushort baseDelta = br.ReadUInt16(); // The base delta, which was the minimum index after delta compressing which must be subtracted from each delta compressed index first.
            ushort num1Bytes = br.ReadUInt16(); // The number of bytes taken by the 1 bit stream for sequential indexes.
            byte bitsPerIndex = br.ReadByte(); // How many bits each delta index takes up.
            br.AssertByte(0); // Padding, should always be 0.

            // There are 3 indexes per triangle so divide the number of indexes by 3
            int numTriangles = numIndexes / 3;

            // There are 2 bits per triangle, so double the number of triangles in the calculation
            // This is the number of triangle configurations, which are two bits each.
            // It must be padded to 8 bits, which I think adding 7 is supposed to do? Seen that in a python script.
            int num2Bits = (numTriangles + numTriangles) + 7;

            // There are 8 bits per byte, so divide the number of 2 bits by 8.
            int num2Bytes = num2Bits / 8;

            // Multiply the number of n bit indexes by the number of bits per index to get the number of n bits
            int numNBits = numIndexesInNBitStream * bitsPerIndex;

            // There are 8 bits per byte, so divide the number of n bits by 8.
            // Ensure the bit count is padded to 8 bits since they must be padded to bytes.
            int numNBytes = (numNBits + (8 - (numNBits % 8))) / 8;

            // Read raw bytes
            var bit1Bytes = br.ReadBytes(num1Bytes);
            var bit2Bytes = br.ReadBytes(num2Bytes);
            var nbitBytes = br.ReadBytes(numNBytes);

            ushort[] deltaIndexes;
            if (bitsPerIndex > 0)
            {
                // Expand delta indexes
                deltaIndexes = ExpandNBits(nbitBytes, bitsPerIndex, numIndexesInNBitStream);
            }
            else
            {
                // If bits per index is 0, store 0s as the delta indexes?
                deltaIndexes = new ushort[numIndexesInNBitStream];
            }

            // Decompress delta indexes in place into unique indexes.
            DecompressDeltaIndexes(deltaIndexes, baseDelta);

            // Add the sequential indexes back using the 1 bit stream.
            ushort[] newIndexes = DecompressSequentialIndexes(deltaIndexes, bit1Bytes);

            // Sort the triangle configurations given by the 2 bit stream.
            // The following is each configuration by bits,
            // Each number is a previous index, and N means the next New index.
            // 00 = 1 0 N
            // 01 = 0 2 N
            // 10 = 2 1 N
            // 11 = N N N
            int shift = 6; // For accessing the 2-bit table without expanding it first
            int currentIndex = 0; // For indexing decompressed indices
            int resultIndex = 0; // For indexing result array
            ushort[] result = new ushort[numTriangles * 3]; // The resulting face indexes
            ushort[] triangleBuffer = new ushort[3]; // A buffer for holding each triangle while decompressing
            for (int i = 0; i < numTriangles; i++)
            {
                // The configuration of the triangle which describes how it is rotated
                // Get the current 2-bit byte (i >> 2)
                // Shift it down to only the bits we want (>> shift)
                // Mask any upper bits we don't want and ensure config is only in a 2-bit range (& 3).
                byte config = (byte)(((bit2Bytes[i >> 2]) >> shift) & 3);

                // Decrement our shift, if the shift has hit -2 that means we need to reset it.
                shift -= 2;
                if (shift == -2)
                    shift = 6;

                // Get our triangle indexes
                switch (config)
                {
                    case 0:
                        // 1 0 N
                        triangleBuffer[1] = triangleBuffer[2];
                        triangleBuffer[2] = newIndexes[currentIndex];
                        currentIndex++;
                        break;
                    case 1:
                        // 0 2 N
                        triangleBuffer[0] = triangleBuffer[2];
                        triangleBuffer[2] = newIndexes[currentIndex];
                        currentIndex++;
                        break;
                    case 2:
                        // 2 1 N
                        ushort tempIndex = triangleBuffer[0];
                        triangleBuffer[0] = triangleBuffer[1];
                        triangleBuffer[1] = tempIndex;
                        triangleBuffer[2] = newIndexes[currentIndex];
                        currentIndex++;
                        break;
                    case 3:
                        // N N N
                        triangleBuffer[0] = newIndexes[currentIndex];
                        currentIndex++;
                        triangleBuffer[1] = newIndexes[currentIndex];
                        currentIndex++;
                        triangleBuffer[2] = newIndexes[currentIndex];
                        currentIndex++;
                        break;
                }

                // Save the triangle indexes
                result[resultIndex++] = triangleBuffer[0];
                result[resultIndex++] = triangleBuffer[1];
                result[resultIndex++] = triangleBuffer[2];
            }

            // Finished
            return result;
        }

        /// <summary>
        /// Expand the n-bit compressed delta stream into an array of <see cref="ushort"/>.
        /// </summary>
        /// <param name="input">The bytes to expand.</param>
        /// <param name="n">The number of bits per byte.</param>
        /// <param name="expandCount">The number of deltas to expand.</param>
        /// <returns>An array of compressed delta indices.</returns>
        private static ushort[] ExpandNBits(byte[] input, int n, int expandCount)
        {
            ushort[] output = new ushort[expandCount];

            int outIndex = 0;
            int bitPos = 0;
            for (int i = 0; i < expandCount; i++)
            {
                // Get the current byte position by dividing the bit position by 8 (right shift by 3)
                int bytePos = bitPos >> 3;

                // Get the next 3 bytes to pack into a buffer
                uint b0 = input[bytePos];
                uint b1 = 0;
                uint b2 = 0;

                // Get the second byte if possible
                if ((bytePos + 1) < input.Length)
                    b1 = input[bytePos + 1];

                // Get the third byte if possible
                if ((bytePos + 2) < input.Length)
                    b2 = input[bytePos + 2];

                // Pack the bytes into a buffer for shifting
                uint buffer = (b0 << 24) | (b1 << 16) | (b2 << 8);

                // Remove the undesired leading bits of the first byte.
                buffer <<= bitPos & 7;

                // Shift the entire buffer down to only bits we want.
                buffer >>= 32 - n;

                // Save the output.
                output[outIndex++] = (ushort)buffer;

                // Increment the bit position.
                bitPos += n;
            }

            return output;
        }

        /// <summary>
        /// Decompress delta indexes into non-sequential indexes. 
        /// </summary>
        /// <param name="deltaIndexes">The delta indexes to decompress.</param>
        /// <param name="baseDelta">The base delta to shift the delta indexes by.</param>
        private static void DecompressDeltaIndexes(ushort[] deltaIndexes, ushort baseDelta)
        {
            // Remove the base delta from the indexes.
            for (int i = 0; i < deltaIndexes.Length; i++)
            {
                deltaIndexes[i] -= baseDelta;
            }

            // Add the 8th previous index to each index.
            // The first 8 indexes are not compressed.
            for (int i = 8; i < deltaIndexes.Length; i++)
            {
                deltaIndexes[i] = (ushort)(deltaIndexes[i] + deltaIndexes[i - 8]);
            }
        }

        /// <summary>
        /// Decompress the sequential bitstream into the new face indexes.
        /// </summary>
        /// <param name="uniqueIndexes">The non-sequential or unique indexes.</param>
        /// <param name="bit1Bytes">The 1-bit packed sequential stream.</param>
        /// <returns>The new face indexes.</returns>
        private static ushort[] DecompressSequentialIndexes(ushort[] uniqueIndexes, byte[] bit1Bytes)
        {
            // Get the number of 1-bit values by multiplying by 8 (left shift by 3)
            // Also the same number of new indexes
            int num1Bits = bit1Bytes.Length << 3;
            ushort[] newIndexes = new ushort[num1Bits];

            int uniqueIndex = 0;
            ushort incrementalIndex = 0;

            int bit1Index = 0; // The byte index into the 1-bit stream.
            byte mask = 1 << 7; // A mask for checking the current 1-bit value we are on.
            for (int i = 0; i < num1Bits; i++)
            {
                // If the current 1-bit value is 1, that means the next index is unique and not incremental.
                // Otherwise the next index is incrementing by 1 from the previous incremental index.
                if ((bit1Bytes[bit1Index] & mask) != 0)
                {
                    // Unique
                    newIndexes[i] = uniqueIndexes[uniqueIndex++];
                }
                else
                {
                    // Sequential
                    newIndexes[i] = incrementalIndex++;
                }

                // Reset our bit position to 0 and increment the 1-bit byte index when needed.
                // Move bit mask
                mask >>= 1;
                if (mask == 0)
                {
                    // Reset bit mask
                    mask = 1 << 7;
                    bit1Index++;
                }
            }
            return newIndexes;
        }

        #endregion

        #region Vertices

        [StructLayout(LayoutKind.Explicit)]
        internal struct Fixed3
        {
            [FieldOffset(0)]
            public ushort X;

            [FieldOffset(2)]
            public ushort Y;

            [FieldOffset(4)]
            public ushort Z;

            internal Fixed3(BinaryReaderEx br)
            {
                X = br.ReadUInt16();
                Y = br.ReadUInt16();
                Z = br.ReadUInt16();
            }

            public Vector3 Decompress(Vector4 multiplier, Vector4 offset)
                => new Vector3((X * multiplier.X) + offset.X, (Y * multiplier.Y) + offset.Y, (Z * multiplier.Z) + offset.Z);
        }

        #endregion
    }
}
