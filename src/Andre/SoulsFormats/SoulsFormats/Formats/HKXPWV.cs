using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats
{
    /// <summary>
    /// Some ragdoll-related container found in Dark Souls 1, Dark Souls III, and Bloodborne
    /// </summary>
    public class HKXPWV : SoulsFile<HKXPWV>
    {
        private int Version;
        
        /// <summary>
        /// The game this HKXPWV is for.
        /// </summary>
        public enum GameType
        {
            /// <summary>
            /// Dark Souls
            /// </summary>
            DS1,
            /// <summary>
            /// Bloodborne
            /// </summary>
            BB,
            /// <summary>
            /// Dark Souls III
            /// </summary>
            DS3,
            /// <summary>
            /// Demon's Souls: big endian, version 0, one table of 4-byte entries split into six groups
            /// </summary>
            DES
        }

        /// <summary>
        /// Demon's Souls only: the header value at 0x0A (1 in every vanilla file).
        /// </summary>
        private ushort DesUnk0A { get; set; }

        /// <summary>
        /// Demon's Souls only: how many of <see cref="BoneMapEntries"/> belong to each of the six groups, in order.
        /// The groups always add up to the entry count.
        /// </summary>
        private ushort[] DesGroupCounts { get; set; } = new ushort[6];
        /// <summary>
        /// Whether the file is big endian.
        /// </summary>
        private bool BigEndian { get; set; }

        /// <summary>
        /// Which game this file is for.
        /// </summary>
        private GameType Game { get; set; }

        /// <summary>
        /// Unknown in header. Can be 0, 1, 2, or 5. Is 0 typically.
        /// </summary>
        private uint UnkC { get; set; }

        /// <summary>
        /// List of items which affect bone map entries in the HKX.
        /// </summary>
        public List<BoneMapEntry> BoneMapEntries { get; set; }

        /// <summary>
        /// List of items which affect bones in the animation skeleton.
        /// </summary>
        public List<AnimBoneEntry> AnimBoneEntries { get; set; }

        /// <summary>
        /// List of items which affect ragdoll bones.
        /// </summary>
        public List<RagdollBoneEntry> RagdollBoneEntries { get; set; }

        /// <summary>
        /// Creates a new empty HKXPWV.
        /// </summary>
        public HKXPWV()
        {
            BoneMapEntries = new List<BoneMapEntry>();
            AnimBoneEntries = new List<AnimBoneEntry>();
            RagdollBoneEntries = new List<RagdollBoneEntry>();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException();
        }

        protected override void Read(BinaryReaderEx br)
        {
            // Value should be 1 in either endianness
            Version = br.ReadInt32();

            // Demon's Souls: version 0, and the header size (0x20) at 0x06 in big endian
            if (Version == 0 && br.Length >= 0x20 && br.GetUInt16(6) == 0x2000)
            {
                ReadDes(br);
                return;
            }

            if (Version == 0x1000000)
            {
                BigEndian = true;
                br.BigEndian = true;
            }

            br.AssertInt16(0);

            ushort boneMapEntryCount = br.ReadUInt16();
            ushort animBoneEntryCount = br.ReadUInt16();
            ushort ragdollBoneEntryCount = br.ReadUInt16();

            if (br.Length == (0x20 + (boneMapEntryCount * 8) + (animBoneEntryCount * 8) + (ragdollBoneEntryCount * 16)))
            {
                Game = GameType.BB;
            }
            else if (br.Length == (0x20 + (boneMapEntryCount * 4) + (animBoneEntryCount * 8) + (ragdollBoneEntryCount * 16)))
            {
                Game = GameType.DS3;
            }
            else if (br.Length == (0x20 + (boneMapEntryCount * 4) + (animBoneEntryCount * 8) + (ragdollBoneEntryCount * 8)))
            {
                Game = GameType.DS1;
            }
            else
            {
                throw new System.IO.InvalidDataException("Invalid struct sizes found in HKXPWV file.");
            }

            UnkC = br.AssertUInt32([0, 1, 2, 5]);

            for (int i = 0; i < 4; i++)
                br.AssertInt32(0);

            for (int i = 0; i < boneMapEntryCount; i++)
                BoneMapEntries.Add(new BoneMapEntry(br, Game));

            for (int i = 0; i < animBoneEntryCount; i++)
                AnimBoneEntries.Add(new AnimBoneEntry(br));

            for (int i = 0; i < ragdollBoneEntryCount; i++)
                RagdollBoneEntries.Add(new RagdollBoneEntry(br, Game));
        }

        /// <summary>
        /// Demon's Souls layout (all 39 vanilla files): 0x00 int 0, 0x04 short 0, 0x06 short 0x20 (header size),
        /// 0x08 entry count, 0x0A unknown (1), 0x0C-0x16 six group counts adding up to the entry count, 0x18 8 zero
        /// bytes, then the entries as two shorts each.
        /// </summary>
        private void ReadDes(BinaryReaderEx br)
        {
            Game = GameType.DES;
            BigEndian = true;
            br.BigEndian = true;
            br.Position = 4;

            br.AssertInt16(0);
            br.AssertInt16(0x20);
            ushort entryCount = br.ReadUInt16();
            DesUnk0A = br.ReadUInt16();

            int sum = 0;
            for (int i = 0; i < 6; i++)
            {
                DesGroupCounts[i] = br.ReadUInt16();
                sum += DesGroupCounts[i];
            }

            br.AssertInt32(0);
            br.AssertInt32(0);

            if (sum != entryCount || br.Length != 0x20 + entryCount * 4)
                throw new System.IO.InvalidDataException("Invalid struct sizes found in Demon's Souls HKXPWV file.");

            for (int i = 0; i < entryCount; i++)
                BoneMapEntries.Add(new BoneMapEntry(br, Game));
        }

        private void WriteDes(BinaryWriterEx bw)
        {
            int sum = DesGroupCounts.Sum(c => c);
            if (sum != BoneMapEntries.Count)
                throw new InvalidOperationException(
                    $"The Demon's Souls HKXPWV groups hold {sum} entries, but there are {BoneMapEntries.Count}.");

            bw.BigEndian = true;
            bw.WriteInt32(0);
            bw.WriteInt16(0);
            bw.WriteInt16(0x20);
            bw.WriteUInt16((ushort)BoneMapEntries.Count);
            bw.WriteUInt16(DesUnk0A);
            foreach (var count in DesGroupCounts)
                bw.WriteUInt16(count);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            foreach (var boneMapEntry in BoneMapEntries)
                boneMapEntry.Write(bw, Game);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            if (Game == GameType.DES)
            {
                WriteDes(bw);
                return;
            }

            bw.BigEndian = BigEndian;

            bw.WriteInt32(Version);

            bw.WriteInt16(0);

            bw.WriteUInt16((ushort)BoneMapEntries.Count);
            bw.WriteUInt16((ushort)AnimBoneEntries.Count);
            bw.WriteUInt16((ushort)RagdollBoneEntries.Count);

            bw.WriteUInt32(UnkC);

            for (int i = 0; i < 4; i++)
                bw.WriteInt32(0);

            foreach (var boneMapEntry in BoneMapEntries)
                boneMapEntry.Write(bw, Game);

            foreach (var animBoneEntry in AnimBoneEntries)
                animBoneEntry.Write(bw);

            foreach (var ragdollBoneEntry in RagdollBoneEntries)
                ragdollBoneEntry.Write(bw, Game);
        }

        /// <summary>
        /// Includes parameters for a bone map entry in the HKX.
        /// </summary>
        public class BoneMapEntry
        {
            /// <summary>
            /// Unknown
            /// </summary>
            public int UnknownValueA { get; set; } = -1;

            /// <summary>
            /// Unknown
            /// </summary>
            public int UnknownValueB { get; set; } = 0;

            /// <summary>
            /// Creates a default BoneMapEntry with default values.
            /// </summary>
            public BoneMapEntry()
            {

            }

            internal BoneMapEntry(BinaryReaderEx br, GameType game)
            {
                if (game == GameType.BB)
                {
                    UnknownValueA = br.ReadInt32();
                    UnknownValueB = br.ReadInt32();
                }
                else
                {
                    UnknownValueA = br.ReadInt16();
                    UnknownValueB = br.ReadInt16();
                }
            }

            internal void Write(BinaryWriterEx bw, GameType game)
            {
                if (game == GameType.BB)
                {
                    bw.WriteInt32(UnknownValueA);
                    bw.WriteInt32(UnknownValueB);
                }
                else
                {
                    bw.WriteInt16((short)UnknownValueA);
                    bw.WriteInt16((short)UnknownValueB);
                }
            }
        }

        /// <summary>
        /// Includes parameters for an animation bone entry in the HKX.
        /// </summary>
        public class AnimBoneEntry
        {
            /// <summary>
            /// ID of the SkeletonParam entry referenced by this animation bone.
            /// </summary>
            public short SkeletonParamID { get; set; }

            /// <summary>
            /// Unknown. 
            /// Possible values on PC: { 0, 1, 2, 3 }.
            /// Possible values on PS3: { 0, 0x10, 0x20, 0x30 }
            /// </summary>
            public byte Unk2 { get; set; }

            /// <summary>
            /// Creates a default AnimBoneEntry with the most commonly found
            /// field values: SkeletonParamRowID = -1, Unk2 = 0.
            /// </summary>
            public AnimBoneEntry()
            {
                SkeletonParamID = -1;
                Unk2 = 0;
            }

            internal AnimBoneEntry(BinaryReaderEx br)
            {
                SkeletonParamID = br.ReadInt16();
                Unk2 = br.ReadByte();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt16(SkeletonParamID);
                bw.WriteByte(Unk2);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
            }
        }

        /// <summary>
        /// Includes parameters for a ragdoll bone entry in the HKX.
        /// </summary>
        public class RagdollBoneEntry
        {
            /// <summary>
            /// ID of the RagdollParam entry referenced by this ragdoll bone.
            /// -1 to not reference an entry.
            /// </summary>
            public int RagdollParamID { get; set; } = -1;

            /// <summary>
            /// The animation ID to play when this ragdoll bone is damaged.
            /// Plays animation a99_XXXX in the TAE.
            /// </summary>
            public short DamageAnimID { get; set; } = -1;

            /// <summary>
            /// Defines which NPC part group (if any) this ragdoll bone's collider is assigned.
            /// Used for NPC part events in EMEVD or enemy weak points.
            /// </summary>
            public byte NPCPartGroupIndex { get; set; } = 0;

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte Unknown { get; set; } = 0;

            /// <summary>
            /// Unknown value unique to Bloodborne HKXPWV files.
            /// </summary>
            public sbyte UnknownBB { get; set; } = -1;

            public int Unk0C { get; set; } = 0;
            public int Unk10 { get; set; } = 0;

            public byte UnkDB { get; set; } = 0;

            /// <summary>
            /// Creates a default RagdollBoneEntry
            /// </summary>
            public RagdollBoneEntry()
            {

            }

            internal RagdollBoneEntry(BinaryReaderEx br, GameType game)
            {
                if (game == GameType.BB)
                {
                    RagdollParamID = br.ReadInt32();
                    DamageAnimID = br.ReadInt16();
                    NPCPartGroupIndex = br.ReadByte();
                    Unknown = br.ReadByte();
                    UnknownBB = br.ReadSByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }
                else
                {
                    RagdollParamID = br.ReadInt16();
                    DamageAnimID = br.ReadInt16();
                    NPCPartGroupIndex = br.ReadByte();
                    Unknown = br.ReadByte();
                    br.AssertByte(0);
                    UnkDB = br.ReadByte(); // br.AssertByte(0);

                    if (game == GameType.DS3)
                    {
                        Unk0C = br.ReadInt32();
                        Unk10 = br.ReadInt32();
                    }
                }
                
            }

            internal void Write(BinaryWriterEx bw, GameType game)
            {
                if (game == GameType.BB)
                {
                    bw.WriteInt32(RagdollParamID);
                    bw.WriteInt16(DamageAnimID);
                    bw.WriteByte(NPCPartGroupIndex);
                    bw.WriteByte(Unknown);
                    bw.WriteSByte(UnknownBB);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
                else
                {
                    bw.WriteInt16((short)RagdollParamID);
                    bw.WriteInt16(DamageAnimID);
                    bw.WriteByte(NPCPartGroupIndex);
                    bw.WriteByte(Unknown);
                    bw.WriteByte(0);
                    bw.WriteByte(UnkDB);

                    if (game == GameType.DS3)
                    {
                        bw.WriteInt32(Unk0C);
                        bw.WriteInt32(Unk10);
                    }
                }
            }
        }

    }
}
