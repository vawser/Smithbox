using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    internal static class BinderHashTable
    {
        public static void Assert(BinaryReaderEx br)
        {
            br.ReadInt64(); // Hashes offset
            br.ReadInt32(); // Bucket count
            br.AssertByte(0x10); // Hash table header size?
            br.AssertByte(8); // Bucket size?
            br.AssertByte(8); // Hash size?
            br.AssertByte(0);
            // Don't actually care about the hashes, I just like asserting
        }

        public static void Write(BinaryWriterEx bw, List<BinderFileHeader> files)
        {
            uint groupCount = 0;
            for (uint p = (uint)files.Count / 7; p <= 100000; p++)
            {
                if (SFUtil.IsPrime(p))
                {
                    groupCount = p;
                    break;
                }
            }

            if (groupCount == 0)
                throw new InvalidOperationException("Could not determine hash group count.");

            var hashLists = new List<PathHash>[groupCount];
            for (int i = 0; i < groupCount; i++)
                hashLists[i] = new List<PathHash>();

            for (int i = 0; i < files.Count; i++)
            {
                var pathHash = new PathHash(i, files[i].Name);
                uint group = pathHash.Hash % groupCount;
                hashLists[group].Add(pathHash);
            }

            for (int i = 0; i < groupCount; i++)
                hashLists[i].Sort((ph1, ph2) => ph1.Hash.CompareTo(ph2.Hash));

            var hashGroups = new List<HashGroup>();
            var pathHashes = new List<PathHash>();

            int count = 0;
            foreach (List<PathHash> hashList in hashLists)
            {
                int index = count;
                foreach (PathHash pathHash in hashList)
                {
                    pathHashes.Add(pathHash);
                    count++;
                }

                hashGroups.Add(new HashGroup(index, count - index));
            }

            bw.ReserveInt64("HashesOffset");
            bw.WriteUInt32(groupCount);

            bw.WriteByte(0x10);
            bw.WriteByte(8);
            bw.WriteByte(8);
            bw.WriteByte(0);

            foreach (HashGroup hashGroup in hashGroups)
                hashGroup.Write(bw);

            bw.FillInt64("HashesOffset", bw.Position);
            foreach (PathHash pathHash in pathHashes)
                pathHash.Write(bw);
        }

        private class PathHash
        {
            public int Index;
            public uint Hash;

            public PathHash(BinaryReaderEx br)
            {
                Hash = br.ReadUInt32();
                Index = br.ReadInt32();
            }

            public PathHash(int index, string path)
            {
                Index = index;
                Hash = SFUtil.FromPathHash(path);
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(Hash);
                bw.WriteInt32(Index);
            }
        }

        private class HashGroup
        {
            public int Index, Length;

            public HashGroup(BinaryReaderEx br)
            {
                Length = br.ReadInt32();
                Index = br.ReadInt32();
            }

            public HashGroup(int index, int length)
            {
                Index = index;
                Length = length;
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(Length);
                bw.WriteInt32(Index);
            }
        }
    }
}
