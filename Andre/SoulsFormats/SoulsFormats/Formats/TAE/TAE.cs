using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using static SoulsFormats.TAE3;

namespace SoulsFormats
{
    /// <summary>
    /// Controls when different events happen during animations; this specific version is used in DS3. Extension: .tae
    /// </summary>
    public partial class TAE : SoulsFile<TAE>
    {
        /// <summary>
        /// Which format this file is.
        /// </summary>
        public enum TAEFormat
        {
            /// <summary>
            /// Dark Souls 1.
            /// </summary>
            DS1 = 0,
            /// <summary>
            /// Dark Souls II: Scholar of the First Sin. 
            /// Does not support 32-bit original Dark Souls II release.
            /// </summary>
            SOTFS = 1,
            /// <summary>
            /// Dark Souls III. Same value as Bloodborne.
            /// </summary>
            DS3 = 2,
            /// <summary>
            /// Bloodborne. Same value as Dark Souls III.
            /// </summary>
            BB = 2,
            /// <summary>
            /// Sekiro: Shadows Die Twice
            /// </summary>
            SDT = 3,
            /// <summary>
            /// Demon's Souls
            /// </summary>
            DES = 4,
            /// <summary>
            /// Demon's Souls Remastered
            /// </summary>
            DESR = 5,
        }

        /// <summary>
        /// The format of this file. Different between most games.
        /// </summary>
        public TAEFormat Format { get; set; }

        public bool IsOldDemonsSoulsFormat_0x1000A { get; set; } = false;
        public bool IsOldDemonsSoulsFormat_0x10000 { get; set; } = false;

        public bool HasErrorAndNeedsResave = false;

        public long? AnimCount2Value { get; set; } = null;

        /// <summary>
        /// Whether the format is big endian.
        /// Only valid for DES/DS1 files.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// ID number of this TAE.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Unknown flags.
        /// </summary>
        public byte[] Flags { get; private set; }

        /// <summary>
        /// Unknown .hkt file.
        /// </summary>
        public string SkeletonName { get; set; }

        /// <summary>
        /// Unknown .sib file.
        /// </summary>
        public string SibName { get; set; }

        /// <summary>
        /// Animations controlled by this TAE.
        /// </summary>
        public List<Animation> Animations;

        /// <summary>
        /// What set of events this TAE uses. Can be different within the same game.
        /// Often found in OBJ TAEs.
        /// Not stored in DS1 TAE files.
        /// </summary>
        public long EventBank { get; set; }

        /// <summary>
        /// The template currently applied. Set by ApplyTemplate method.
        /// </summary>
        public Template AppliedTemplate { get; private set; }

        public TAE Clone()
        {
            return (TAE)MemberwiseClone();
        }

        /// <summary>
        /// Applies a template to this TAE for easier editing.
        /// After applying template, use events' .Parameters property.
        /// </summary>
        public void ApplyTemplate(Template template)
        {
            if (template.Game != Format)
                throw new InvalidOperationException($"Template is for {template.Game} but this TAE is for {Format}.");

            foreach (var anim in Animations)
            {
                for (int i = 0; i < anim.Events.Count; i++)
                {
                    anim.Events[i].ApplyTemplate(this, template, anim.ID, i, anim.Events[i].Type);
                }
            }

            AppliedTemplate = template;
        }

        /// <summary>
        /// For use with porting to other games etc. Make sure fields are named the same between games or this will throw a KeyNotFoundException (obviously).
        /// </summary>
        public void ChangeTemplateAfterLoading(Template template, int bank = -1)
        {
            if (template.Game != Format)
                throw new InvalidOperationException($"Template is for {template.Game} but this TAE is for {Format}.");

            if (bank >= 0)
                EventBank = bank;

            foreach (var anim in Animations)
            {
                for (int i = 0; i < anim.Events.Count; i++)
                {
                    anim.Events[i].ChangeTemplateAfterLoading(this, template, anim.ID, i, anim.Events[i].Type);
                }
            }

            AppliedTemplate = template;
        }

        protected override bool Is(BinaryReaderEx br)
        {
            string magic = br.GetASCII(0, 4);
            return magic == "TAE ";
        }

        protected override void Read(BinaryReaderEx br)
        {
            HasErrorAndNeedsResave = false;

            br.BigEndian = false;
            br.VarintLong = false;

            br.AssertASCII("TAE ");

            BigEndian = br.AssertByte([0, 1]) == 1;
            br.BigEndian = BigEndian;

            br.AssertByte(0);
            br.AssertByte(0);

            bool is64Bit = br.AssertByte([0, 0xFF]) == 0xFF;
            br.VarintLong = is64Bit;
            bool isDESR = false;
            // Demon's Souls Remastered check.
            if (br.GetByte(0x10) == 0x30)
            {
                br.BigEndian = BigEndian = false;
                br.VarintLong = is64Bit = true;
                isDESR = true;
            }


            // 0x1000A: DeS Cut Characters
            // 0x1000B: DeS, DS1(R)
            // 0x1000C: DS2, DS2 SOTFS, BB, DS3
            // 0x1000D: SDT, ER
            int version = br.AssertInt32([0x10000, 0x1000A, 0x1000B, 0x1000C, 0x1000D]);


            if (version == 0x1000B && !is64Bit)
            {
                Format = TAEFormat.DS1;
            }
            else if (version == 0x1000A && !is64Bit)
            {
                Format = TAEFormat.DS1;
                IsOldDemonsSoulsFormat_0x1000A = true;
            }
            else if (version == 0x10000 && !is64Bit)
            {
                Format = TAEFormat.DS1;
                IsOldDemonsSoulsFormat_0x10000 = true;
            }
            else if (version == 0x1000B && isDESR)
            {
                Format = TAEFormat.DESR;
            }
            else if (version == 0x1000A && isDESR)
            {
                Format = TAEFormat.DESR;
                IsOldDemonsSoulsFormat_0x1000A = true;
            }
            else if (version == 0x10000 && isDESR)
            {
                Format = TAEFormat.DESR;
                IsOldDemonsSoulsFormat_0x10000 = true;
            }
            else if (version == 0x1000C && !is64Bit)
            {
                throw new NotImplementedException("Dark Souls II 32-Bit original release not supported. Only Scholar of the First Sin.");
            }
            else if (version == 0x1000C && is64Bit)
            {
                Format = TAEFormat.DS3;
            }
            else if (version == 0x1000D)
            {
                Format = TAEFormat.SDT;
            }
            else
            {
                throw new System.IO.InvalidDataException("Invalid combination of TAE header values: " +
                    $"IsBigEndian={BigEndian}, Is64Bit={is64Bit}, Version={version}");
            }

            if (isDESR)
            {
                Format = TAEFormat.DESR;
            }

            br.ReadInt32(); // File size
            br.AssertVarint(Format is TAEFormat.DESR ? 0x30 : 0x40);
            br.AssertVarint(1);
            br.AssertVarint(Format is TAEFormat.DESR ? 0x40 : 0x50);

            if (is64Bit && Format is not TAEFormat.DESR)
                br.AssertVarint(0x80);
            else
                br.AssertVarint(0x70);

            if (Format is not TAEFormat.DESR)
            {
                if (Format == TAEFormat.DS1)
                {
                    if (IsOldDemonsSoulsFormat_0x1000A || IsOldDemonsSoulsFormat_0x10000)
                    {
                        br.AssertInt16(0);
                        br.AssertInt16(0);
                        Format = TAEFormat.DES;
                    }
                    else
                    {
                        var subFormat = br.AssertInt16([0, 1, 2]);
                        if (subFormat == 0)
                            Format = TAEFormat.DES;
                        var test = br.AssertInt16([0, 1]);
                        if (test == 0)
                        {
                            HasErrorAndNeedsResave = true;
                        }
                    }
                }
                else
                {
                    EventBank = br.ReadVarint();
                }

                br.AssertVarint(0);

                if (Format == TAEFormat.DS1 || Format == TAEFormat.DES)
                {
                    br.AssertInt64(0);
                    br.AssertInt64(0);
                    br.AssertInt64(0);
                }
            }



            Flags = br.ReadBytes(8);

            var unkFlagA = br.ReadBoolean();
            var unkFlagB = br.ReadBoolean();

            if (version == 0x1000C)
            {
                if (!unkFlagA && unkFlagB)
                    Format = TAEFormat.SOTFS;
                else if ((unkFlagA && unkFlagB) || (!unkFlagA && !unkFlagB))
                    throw new System.IO.InvalidDataException("Invalid unknown flags at 0x48.");
            }

            for (int i = 0; i < 6; i++)
                br.AssertByte(0);

            ID = br.ReadInt32();

            int animCount = br.ReadInt32();
            long animsOffset = br.ReadVarint();
            br.ReadVarint(); // Anim groups offset

            br.AssertVarint((Format is TAEFormat.DES or TAEFormat.DS1 or TAEFormat.DESR) ? 0x90 : 0xA0);
            var animCount2 = br.ReadVarint();
            if (animCount2 == animCount)
            {
                AnimCount2Value = null;
            }
            else
            {
                AnimCount2Value = animCount2;
            }
            br.ReadVarint(); // First anim offset
            if (Format is TAEFormat.DS1 or TAEFormat.DES)
                br.AssertInt32(0);
            br.AssertVarint(1);
            br.AssertVarint(Format is TAEFormat.DES or TAEFormat.DS1 or TAEFormat.DESR ? 0x80 : 0x90);
            if (Format is TAEFormat.DS1 or TAEFormat.DES)
                br.AssertInt64(0);
            br.AssertInt32(ID);
            br.AssertInt32(ID);
            br.AssertVarint(Format is TAEFormat.DESR ? 0x40 : 0x50);
            br.AssertInt64(0);



            if (Format is TAEFormat.DES or TAEFormat.DESR)
                br.AssertVarint(0xA0);
            else if (Format is TAEFormat.DS1)
                br.AssertVarint(0x98);
            else
                br.AssertVarint(0xB0);

            long skeletonNameOffset = 0;
            long sibNameOffset = 0;

            if (Format is TAEFormat.DESR)
            {
                br.AssertVarint(0xB0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                //br.AssertInt32(0);
                //br.AssertInt32(0);
            }
            else
            {
                if (BigEndian)
                {
                    if (Format is not TAEFormat.SOTFS)
                    {
                        br.AssertVarint(0);
                        br.AssertVarint(0);
                    }

                    skeletonNameOffset = br.ReadVarint();
                    sibNameOffset = br.ReadVarint();
                }
                else
                {
                    skeletonNameOffset = br.ReadVarint();
                    sibNameOffset = br.ReadVarint();

                    if (Format is not TAEFormat.SOTFS)
                    {
                        br.AssertVarint(0);
                        br.AssertVarint(0);
                    }
                }



                if (Format is not TAEFormat.SOTFS)
                {
                    SkeletonName = skeletonNameOffset == 0 ? null : br.GetUTF16(skeletonNameOffset);
                    SibName = sibNameOffset == 0 ? null : br.GetUTF16(sibNameOffset);
                }
            }





            br.StepIn(animsOffset);
            {
                Animations = new List<Animation>(animCount);
                bool previousAnimNeedsParamGen = false;
                long previousAnimParamStart = 0;
                for (int i = 0; i < animCount; i++)
                {
                    Animations.Add(new Animation(br, Format,
                        out bool lastEventNeedsParamGen,
                        out long animFileOffset, out long lastEventParamOffset));

                    if (previousAnimNeedsParamGen)
                    {
                        br.StepIn(previousAnimParamStart);
                        Animations[i - 1].Events[Animations[i - 1].Events.Count - 1].ReadParameters(br, (int)(animFileOffset - previousAnimParamStart));
                        br.StepOut();
                    }

                    previousAnimNeedsParamGen = lastEventNeedsParamGen;
                    previousAnimParamStart = lastEventParamOffset;
                }

                // Read from very last anim's very last event's parameters offset to end of file lul
                if (previousAnimNeedsParamGen)
                {
                    br.StepIn(previousAnimParamStart);
                    Animations[Animations.Count - 1].Events[Animations[Animations.Count - 1].Events.Count - 1].ReadParameters(br, (int)(br.Length - previousAnimParamStart));
                    br.StepOut();
                }
            }
            br.StepOut();

            // Don't bother reading anim groups.
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.WriteASCII("TAE ");

            bw.BigEndian = (Format is TAEFormat.DESR) ? false : BigEndian;

            bw.WriteBoolean((Format is TAEFormat.DESR) ? true : BigEndian);
            bw.WriteByte(0);
            bw.WriteByte(0);

            if (Format is TAEFormat.DESR)
            {
                bw.VarintLong = true;
                bw.WriteByte(0);
            }
            else if (Format is TAEFormat.DES or TAEFormat.DS1)
            {
                bw.VarintLong = false;
                bw.WriteByte(0);
            }
            else
            {
                bw.VarintLong = true;
                bw.WriteByte(0xFF);
            }

            if (Format is TAEFormat.DES or TAEFormat.DS1 or TAEFormat.DESR)
            {
                if (IsOldDemonsSoulsFormat_0x1000A)
                    bw.WriteInt32(0x1000A);
                else if (IsOldDemonsSoulsFormat_0x10000)
                    bw.WriteInt32(0x10000);
                else
                    bw.WriteInt32(0x1000B);
            }
            else if (Format is TAEFormat.DS3 or TAEFormat.SOTFS)
                bw.WriteInt32(0x1000C);
            else if (Format is TAEFormat.SDT)
                bw.WriteInt32(0x1000D);

            bw.ReserveInt32("FileSize");
            bw.WriteVarint(Format is TAEFormat.DESR ? 0x30 : 0x40);
            bw.WriteVarint(1);
            bw.WriteVarint(Format is TAEFormat.DESR ? 0x40 : 0x50);
            bw.WriteVarint(bw.VarintLong && (Format is not TAEFormat.DESR) ? 0x80 : 0x70);

            if (Format is not TAEFormat.DESR)
            {

                if ((Format is TAEFormat.DES or TAEFormat.DS1))
                {
                    if (IsOldDemonsSoulsFormat_0x1000A || IsOldDemonsSoulsFormat_0x10000)
                    {
                        bw.WriteInt16(0);
                        bw.WriteInt16(0);
                    }
                    else
                    {
                        bw.WriteInt16((short)(Format == TAEFormat.DES ? 0 : 2));
                        bw.WriteInt16(1);
                    }
                }
                else
                {
                    bw.WriteVarint(EventBank);
                }

                bw.WriteVarint(0);

                if (Format is TAEFormat.DES or TAEFormat.DS1)
                {
                    bw.WriteInt64(0);
                    bw.WriteInt64(0);
                    bw.WriteInt64(0);
                }
            }

            bw.WriteBytes(Flags);

            if (Format is TAEFormat.SOTFS)
            {
                bw.WriteByte(0);
                bw.WriteByte(1);
            }
            else
            {
                bw.WriteByte(1);
                bw.WriteByte(0);
            }

            for (int i = 0; i < 6; i++)
                bw.WriteByte(0);

            bw.WriteInt32(ID);
            bw.WriteInt32(Animations.Count);
            bw.ReserveVarint("AnimsOffset");
            bw.ReserveVarint("AnimGroupsOffset");
            bw.WriteVarint((Format is TAEFormat.DES or TAEFormat.DS1 or TAEFormat.DESR) ? 0x90 : 0xA0);
            bw.WriteVarint(AnimCount2Value ?? Animations.Count);
            bw.ReserveVarint("FirstAnimOffset");
            if ((Format is TAEFormat.DES or TAEFormat.DS1))
                bw.WriteInt32(0);
            bw.WriteVarint(1);
            bw.WriteVarint((Format is TAEFormat.DES or TAEFormat.DS1 or TAEFormat.DESR) ? 0x80 : 0x90);
            if ((Format is TAEFormat.DES or TAEFormat.DS1))
                bw.WriteInt64(0);
            bw.WriteInt32(ID);
            bw.WriteInt32(ID);
            bw.WriteVarint((Format is TAEFormat.DESR) ? 0x40 : 0x50);
            bw.WriteInt64(0);

            if (Format is TAEFormat.DES or TAEFormat.DESR)
                bw.WriteVarint(0xA0);
            else if (Format is TAEFormat.DS1)
                bw.WriteVarint(0x98);
            else
                bw.WriteVarint(0xB0);

            if (Format == TAEFormat.DESR)
            {
                bw.WriteVarint(0xB0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
            else
            {
                if (BigEndian)
                {
                    if (Format != TAEFormat.SOTFS)
                    {
                        bw.WriteVarint(0);
                        bw.WriteVarint(0);
                    }

                    bw.ReserveVarint("SkeletonName");
                    bw.ReserveVarint("SibName");
                }
                else
                {
                    bw.ReserveVarint("SkeletonName");
                    bw.ReserveVarint("SibName");

                    if (Format != TAEFormat.SOTFS)
                    {
                        bw.WriteVarint(0);
                        bw.WriteVarint(0);
                    }
                }



                if (SkeletonName != null)
                {
                    bw.FillVarint("SkeletonName", bw.Position);
                    if (!string.IsNullOrEmpty(SkeletonName))
                    {
                        bw.WriteUTF16(SkeletonName, true);
                        if (bw.VarintLong || Format == TAEFormat.DES)
                            bw.Pad(0x10);
                    }
                }
                else
                {
                    bw.FillVarint("SkeletonName", 0);
                }

                if (SibName != null)
                {
                    bw.FillVarint("SibName", bw.Position);
                    if (!string.IsNullOrEmpty(SibName))
                    {
                        bw.WriteUTF16(SibName, true);
                        if (bw.VarintLong || Format == TAEFormat.DES)
                            bw.Pad(0x10);
                    }
                }
                else
                {
                    bw.FillVarint("SibName", 0);
                }
            }




            Animations.Sort((a1, a2) => a1.ID.CompareTo(a2.ID));

            var animOffsets = new List<long>(Animations.Count);
            if (Animations.Count == 0)
            {
                bw.FillVarint("AnimsOffset", 0);
            }
            else
            {
                bw.FillVarint("AnimsOffset", bw.Position);
                for (int i = 0; i < Animations.Count; i++)
                {
                    animOffsets.Add(bw.Position);
                    Animations[i].WriteHeader(bw, i, Format);
                }
            }

            bw.FillVarint("AnimGroupsOffset", bw.Position);
            bw.ReserveVarint("AnimGroupsCount");
            bw.ReserveVarint("AnimGroupsOffset");
            if (Format == TAEFormat.DES)
                bw.Pad(0x10);
            int groupCount = 0;
            long groupStart = bw.Position;
            for (int i = 0; i < Animations.Count; i++)
            {
                int firstIndex = i;
                bw.WriteInt32((int)Animations[i].ID);
                while (i < Animations.Count - 1 && Animations[i + 1].ID == Animations[i].ID + 1)
                    i++;
                bw.WriteInt32((int)Animations[i].ID);
                bw.WriteVarint(animOffsets[firstIndex]);
                if (Format == TAEFormat.DES)
                    bw.Pad(0x10);
                groupCount++;
            }
            bw.FillVarint("AnimGroupsCount", groupCount);

            if (groupCount == 0)
                bw.FillVarint("AnimGroupsOffset", 0);
            else
                bw.FillVarint("AnimGroupsOffset", groupStart);

            if (Animations.Count == 0)
            {
                bw.FillVarint("FirstAnimOffset", 0);
            }
            else
            {
                bw.FillVarint("FirstAnimOffset", bw.Position);
                for (int i = 0; i < Animations.Count; i++)
                    Animations[i].WriteBody(bw, i, Format);
            }

            for (int i = 0; i < Animations.Count; i++)
            {
                Animation anim = Animations[i];
                anim.WriteAnimFile(bw, i, Format);
                Dictionary<float, long> timeOffsets = anim.WriteTimes(bw, i, Format);
                List<long> eventHeaderOffsets = anim.WriteEventHeaders(bw, i, timeOffsets, Format);
                anim.WriteEventData(bw, i, Format);
                anim.WriteEventGroupHeaders(bw, i, Format);
                anim.WriteEventGroupData(bw, i, eventHeaderOffsets, Format);
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

    }
}
