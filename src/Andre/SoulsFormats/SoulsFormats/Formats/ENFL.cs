using System.Collections.Generic;

namespace SoulsFormats
{
    /// <summary>
    /// A list of files to preload when the player enters a map, used from BB onwards. Extension: .entryfilelist
    /// Files named e{ID} belong to the SpawnPoint region with that entity ID, and e{map}999 to the map itself.
    /// The entries form a hash table: each string is kept in the bucket of its hash, and Struct1s give the extent of each bucket.
    /// </summary>
    public class ENFL : SoulsFile<ENFL>
    {
        /// <summary>
        /// The buckets of the hash table, which are rebuilt from Struct2s when the file is written.
        /// </summary>
        public List<Struct1> Struct1s;

        /// <summary>
        /// The hash of each string, in the same order as Strings.
        /// </summary>
        public List<Struct2> Struct2s;

        /// <summary>
        /// The paths of the files to load, written as device:/path.
        /// </summary>
        public List<string> Strings;

        /// <summary>
        /// Checks whether the data appears to be a file of this format.
        /// </summary>
        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "ENFL";
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("ENFL");
            // Probably 4 bytes
            br.AssertInt32(0x10415);
            int compressedSize = br.ReadInt32();
            int uncompressedSize = br.ReadInt32();
            byte[] data = SFUtil.ReadZlib(br, compressedSize);

            br = new BinaryReaderEx(false, data);
            br.AssertInt32(0);
            int unkCount1 = br.ReadInt32();
            int unkCount2 = br.ReadInt32();
            br.AssertInt32(0);

            Struct1s = new List<Struct1>(unkCount1);
            for (int i = 0; i < unkCount1; i++)
                Struct1s.Add(new Struct1(br));
            br.Pad(0x10);

            Struct2s = new List<Struct2>(unkCount2);
            for (int i = 0; i < unkCount2; i++)
                Struct2s.Add(new Struct2(br));
            br.Pad(0x10);

            br.AssertInt16(0);
            Strings = new List<string>(unkCount2);
            for (int i = 0; i < unkCount2; i++)
                Strings.Add(br.ReadUTF16());
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            RebuildHashTable();

            BinaryWriterEx bwData = new BinaryWriterEx(false);

            bwData.WriteInt32(0);
            bwData.WriteInt32(Struct1s.Count);
            bwData.WriteInt32(Struct2s.Count);
            bwData.WriteInt32(0);

            foreach (Struct1 struct1 in Struct1s)
                struct1.Write(bwData);
            bwData.Pad(0x10);

            foreach (Struct2 struct2 in Struct2s)
                struct2.Write(bwData);
            bwData.Pad(0x10);

            bwData.WriteInt16(0);
            foreach (string str in Strings)
                bwData.WriteUTF16(str, true);
            bwData.Pad(0x10);

            byte[] data = bwData.FinishBytes();

            bw.WriteASCII("ENFL");
            bw.WriteInt32(0x10415);
            bw.ReserveInt32("CompressedSize");
            bw.WriteInt32(data.Length);

            // The zlib checksum is big-endian
            bw.BigEndian = true;
            int compressedSize = SFUtil.WriteZlib(bw, 0xDA, data);
            bw.BigEndian = false;

            bw.FillInt32("CompressedSize", compressedSize);
            bw.Pad(0x10);
        }

        /// <summary>
        /// Sorts the entries into the buckets of their hashes and rebuilds Struct1s to match.
        /// Entries keep their order within a bucket, so an unchanged file is written as it was read.
        /// </summary>
        public void RebuildHashTable()
        {
            // The table keeps its size while the entries fit, as the game's files do
            int bucketCount = Struct1s != null && Strings.Count < Struct1s.Count
                ? Struct1s.Count
                : GetBucketCount(Strings.Count);

            var order = new List<int>(Strings.Count);
            for (int i = 0; i < Strings.Count; i++)
                order.Add(i);

            // List.Sort isn't stable, so ties are broken by the original index
            order.Sort((a, b) =>
            {
                int compare = GetBucket(Struct2s[a], bucketCount).CompareTo(GetBucket(Struct2s[b], bucketCount));
                return compare != 0 ? compare : a.CompareTo(b);
            });

            var struct2s = new List<Struct2>(order.Count);
            var strings = new List<string>(order.Count);
            foreach (int i in order)
            {
                struct2s.Add(Struct2s[i]);
                strings.Add(Strings[i]);
            }
            Struct2s = struct2s;
            Strings = strings;

            Struct1s = new List<Struct1>(bucketCount);
            int index = 0;
            for (int bucket = 0; bucket < bucketCount; bucket++)
            {
                short start = (short)index;
                while (index < Struct2s.Count && GetBucket(Struct2s[index], bucketCount) == bucket)
                    index++;

                Struct1s.Add(new Struct1 { Step = (short)(index - start), Index = start });
            }
        }

        private static uint GetBucket(Struct2 struct2, int bucketCount)
        {
            return (uint)struct2.Unk1 % (uint)bucketCount;
        }

        /// <summary>
        /// The size of the hash table for the number of entries: the smallest size above it, where the sizes are
        /// the largest primes within 256, 512, 1024, 2048 and each multiple of 1024 after it (251, 509, 1021, 2039, 3067, 4093...).
        /// This matches every file of DS3 and SDT.
        /// </summary>
        public static int GetBucketCount(int entryCount)
        {
            int capacity = 256;

            while (true)
            {
                int size = LargestPrimeWithin(capacity);

                if (size > entryCount)
                    return size;

                capacity = capacity < 2048 ? capacity * 2 : capacity + 1024;
            }
        }

        private static int LargestPrimeWithin(int capacity)
        {
            for (int n = capacity; n > 2; n--)
            {
                if (IsPrime(n))
                    return n;
            }

            return 2;
        }

        private static bool IsPrime(int n)
        {
            if (n % 2 == 0)
                return n == 2;

            for (int i = 3; i * i <= n; i += 2)
            {
                if (n % i == 0)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Some kind of weird iteration through the strings.
        /// </summary>
        public class Struct1
        {
            /// <summary>
            /// Increase of index to next Struct1. For instance, if Step is 0, the next Struct1 will have the same index. If Step is 2, the next Struct will have this Index + 2.
            /// </summary>
            public short Step;

            /// <summary>
            /// Almost certainly an index into the strings.
            /// </summary>
            public short Index;

            /// <summary>
            /// Creates an empty bucket.
            /// </summary>
            public Struct1() { }

            internal Struct1(BinaryReaderEx br)
            {
                Step = br.ReadInt16();
                Index = br.ReadInt16();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(Step);
                bw.WriteInt16(Index);
            }

            /// <summary>
            /// Returns the step and index.
            /// </summary>
            public override string ToString()
            {
                return $"0x{Step:X4} 0x{Index:X4}";
            }
        }

        /// <summary>
        /// Some data corresponding to each string.
        /// </summary>
        public class Struct2
        {
            /// <summary>
            /// The low 32 bits are a hash of the string, the same in every file, which picks its bucket.
            /// The high bits differ between files for the same string and look like an offset; they aren't understood.
            /// </summary>
            public long Unk1;

            /// <summary>
            /// Creates an entry with the given value.
            /// </summary>
            public Struct2(long unk1)
            {
                Unk1 = unk1;
            }

            internal Struct2(BinaryReaderEx br)
            {
                Unk1 = br.ReadInt64();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt64(Unk1);
            }

            /// <summary>
            /// Returns the entire struct as a number.
            /// </summary>
            public override string ToString()
            {
                return $"0x{Unk1:X16}";
            }
        }
    }
}
