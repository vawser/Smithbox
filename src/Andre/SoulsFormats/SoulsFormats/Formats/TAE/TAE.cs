using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

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
            SDT = 3
        }

        /// <summary>
        /// The format of this file. Different between most games.
        /// </summary>
        public TAEFormat Format { get; set; }

        /// <summary>
        /// Whether the format is big endian.
        /// Only valid for DS1 files.
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

        /// <summary>
        /// Gets the current bank being used in the currently applied template, if a template is applied.
        /// </summary>
        public Template.BankTemplate BankTemplate => AppliedTemplate?[EventBank];

        /// <summary>
        /// Applies a template to this TAE for easier editing.
        /// After applying template, use events' .Parameters property.
        /// </summary>
        public void ApplyTemplate(Template template)
        {
            if (template.Game != Format)
                throw new InvalidOperationException($"Template is for {template.Game} but this TAE is for {Format}.");

            if (template.ContainsKey(EventBank))
            {
                foreach (var anim in Animations)
                {
                    for (int i = 0; i < anim.Events.Count; i++)
                    {
                        anim.Events[i].ApplyTemplate(this, template, anim.ID, i, anim.Events[i].Type);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"This TAE uses event bank {EventBank} but no such bank exists in the template.");
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
            br.BigEndian = false;
            br.VarintLong = false;

            br.AssertASCII("TAE ");

            bool isBigEndian = br.AssertByte([0, 1]) == 1;
            br.BigEndian = isBigEndian;

            br.AssertByte(0);
            br.AssertByte(0);

            bool is64Bit = br.AssertByte([0, 0xFF]) == 0xFF;
            br.VarintLong = is64Bit;

            // 0x1000B: DeS, DS1(R)
            // 0x1000C: DS2, DS2 SOTFS, BB, DS3
            // 0x1000D: SDT
            int version = br.AssertInt32([0x1000B, 0x1000C, 0x1000D]);

            if (version == 0x1000B && !is64Bit)
            {
                Format = TAEFormat.DS1;
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
                    $"IsBigEndian={isBigEndian}, Is64Bit={is64Bit}, Version={version}");
            }

            br.ReadInt32(); // File size
            br.AssertVarint(0x40);
            br.AssertVarint(1);
            br.AssertVarint(0x50);

            if (is64Bit)
                br.AssertVarint(0x80);
            else
                br.AssertVarint(0x70);

            if (Format == TAEFormat.DS1)
            {
                br.AssertInt16(2);
                br.AssertInt16(1);
            }
            else
            {
                EventBank = br.ReadVarint();
            }

            br.AssertVarint(0);

            if (Format == TAEFormat.DS1)
            {
                br.AssertInt64(0);
                br.AssertInt64(0);
                br.AssertInt64(0);
            }

            Flags = br.ReadBytes(8);

            var unkFlagA = br.ReadBoolean();
            var unkFlagB = br.ReadBoolean();

            if (!unkFlagA && unkFlagB)
                Format = TAEFormat.SOTFS;
            else if ((unkFlagA && unkFlagB) || (!unkFlagA && !unkFlagB))
                throw new System.IO.InvalidDataException("Invalid unknown flags at 0x48.");

            for (int i = 0; i < 6; i++)
                br.AssertByte(0);

            ID = br.ReadInt32();

            int animCount = br.ReadInt32();
            long animsOffset = br.ReadVarint();
            br.ReadVarint(); // Anim groups offset

            br.AssertVarint(Format == TAEFormat.DS1 ? 0x90 : 0xA0);
            br.AssertVarint(animCount);
            br.ReadVarint(); // First anim offset
            if (Format == TAEFormat.DS1)
                br.AssertInt32(0);
            br.AssertVarint(1);
            br.AssertVarint(Format == TAEFormat.DS1 ? 0x80 : 0x90);
            if (Format == TAEFormat.DS1)
                br.AssertInt64(0);
            br.AssertInt32(ID);
            br.AssertInt32(ID);
            br.AssertVarint(0x50);
            br.AssertInt64(0);
            br.AssertVarint(Format == TAEFormat.DS1 ? 0x98 : 0xB0);
            
            long skeletonNameOffset = br.ReadVarint();
            long sibNameOffset = br.ReadVarint();

            if (Format != TAEFormat.SOTFS)
            {
                br.AssertVarint(0);
                br.AssertVarint(0);
            }

            if (Format != TAEFormat.SOTFS)
            {
                SkeletonName = br.GetUTF16(skeletonNameOffset);
                SibName = br.GetUTF16(sibNameOffset);
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

            bw.BigEndian = BigEndian;

            bw.WriteBoolean(BigEndian);
            bw.WriteByte(0);
            bw.WriteByte(0);

            if (Format == TAEFormat.DS1)
            {
                bw.VarintLong = false;
                bw.WriteByte(0);
            }
            else
            {
                bw.VarintLong = true;
                bw.WriteByte(0xFF);
            }

            if (Format == TAEFormat.DS1)
                bw.WriteInt32(0x1000B);
            else if (Format == TAEFormat.DS3 || Format == TAEFormat.SOTFS)
                bw.WriteInt32(0x1000C);
            else if (Format == TAEFormat.SDT)
                bw.WriteInt32(0x1000D);

            bw.ReserveInt32("FileSize");
            bw.WriteVarint(0x40);
            bw.WriteVarint(1);
            bw.WriteVarint(0x50);
            bw.WriteVarint(Format == TAEFormat.DS1 ? 0x70 : 0x80);

            if (Format == TAEFormat.DS1)
            {
                bw.WriteInt16(2);
                bw.WriteInt16(1);
            }
            else
            {
                bw.WriteVarint(EventBank);
            }
            
            bw.WriteVarint(0);

            //DeS also
            if (Format == TAEFormat.DS1)
            {
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
            }

            bw.WriteBytes(Flags);

            if (Format == TAEFormat.SOTFS)
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
            bw.WriteVarint(Format == TAEFormat.DS1 ? 0x90 : 0xA0);
            bw.WriteVarint(Animations.Count);
            bw.ReserveVarint("FirstAnimOffset");
            if (Format == TAEFormat.DS1)
                bw.WriteInt32(0);
            bw.WriteVarint(1);
            bw.WriteVarint(Format == TAEFormat.DS1 ? 0x80 : 0x90);
            if (Format == TAEFormat.DS1)
                bw.WriteInt64(0);
            bw.WriteInt32(ID);
            bw.WriteInt32(ID);
            bw.WriteVarint(0x50);
            bw.WriteInt64(0);
            bw.WriteVarint(Format == TAEFormat.DS1 ? 0x98 : 0xB0);
            bw.ReserveVarint("SkeletonName");
            bw.ReserveVarint("SibName");

            if (Format != TAEFormat.SOTFS)
            {
                bw.WriteVarint(0);
                bw.WriteVarint(0);
            }

            bw.FillVarint("SkeletonName", bw.Position);
            if (!string.IsNullOrEmpty(SkeletonName))
            {
                bw.WriteUTF16(SkeletonName, true);
                if (Format != TAEFormat.DS1)
                    bw.Pad(0x10);
            }

            bw.FillVarint("SibName", bw.Position);
            if (!string.IsNullOrEmpty(SibName))
            {
                bw.WriteUTF16(SibName, true);
                if (Format != TAEFormat.DS1)
                    bw.Pad(0x10);
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
                    Animations[i].WriteHeader(bw, i);
                }
            }

            bw.FillVarint("AnimGroupsOffset", bw.Position);
            bw.ReserveVarint("AnimGroupsCount");
            bw.ReserveVarint("AnimGroupsOffset");
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
                List<long> eventHeaderOffsets = anim.WriteEventHeaders(bw, i, timeOffsets);
                anim.WriteEventData(bw, i, Format);
                anim.WriteEventGroupHeaders(bw, i, Format);
                anim.WriteEventGroupData(bw, i, eventHeaderOffsets, Format);
            }

            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Controls an individual animation.
        /// </summary>
        public class Animation
        {
            /// <summary>
            /// ID number of this animation.
            /// </summary>
            public long ID { get; set; }

            /// <summary>
            /// Timed events in this animation.
            /// </summary>
            public List<Event> Events;

            /// <summary>
            /// Unknown groups of events.
            /// </summary>
            public List<EventGroup> EventGroups;

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool AnimFileReference { get; set; }

            ///// <summary>
            ///// ID of animation to reference when AnimFileReference == true.
            ///// </summary>
            //public int ReferenceID;

            ///// <summary>
            ///// Unknown functionality. Likely only relevant if AnimFileReference == true.
            ///// </summary>
            //public bool UnkReferenceFlag1;

            ///// <summary>
            ///// Name retrieved from debug menu. Only relevant if AnimFileReference == true.
            ///// </summary>
            //public bool ReferenceIsTAEOnly;

            ///// <summary>
            ///// Name retrieved from debug menu.  Only relevant if AnimFileReference == true.
            ///// </summary>
            //public bool ReferenceIsHKXOnly;

            ///// <summary>
            ///// Makes the animation loop by default. Only relevant for animations not controlled by
            ///// ESD or HKS such as ObjAct animations.
            ///// </summary>
            //public bool LoopByDefault;

            /// <summary>
            /// Unknown
            /// </summary>
            public int Unknown1 { get; set; }

            /// <summary>
            /// Unknown
            /// </summary>
            public int Unknown2 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public string AnimFileName { get; set; }

            /// <summary>
            /// Creates a new empty animation with the specified properties.
            /// </summary>
            public Animation(long id, bool isReference, int unk1, int unk2, string animFileName)
            {
                ID = id;
                AnimFileReference = isReference;
                Unknown1 = unk1;
                Unknown2 = unk2;
                AnimFileName = animFileName;
                Events = new List<Event>();
                EventGroups = new List<EventGroup>();
            }

            internal Animation(BinaryReaderEx br, TAEFormat format, 
                out bool lastEventNeedsParamGen, out long animFileOffset, 
                out long lastEventParamOffset)
            {
                lastEventNeedsParamGen = false;
                lastEventParamOffset = 0;
                ID = br.ReadVarint();
                long offset = br.ReadVarint();
                br.StepIn(offset);
                {
                    int eventCount;
                    long eventHeadersOffset;
                    int eventGroupCount;
                    long eventGroupsOffset;
                    long timesOffset;

                    if (format == TAEFormat.DS1)
                    {
                        eventCount = br.ReadInt32();
                        eventHeadersOffset = br.ReadVarint();
                        eventGroupCount = br.ReadInt32();
                        eventGroupsOffset = br.ReadVarint();
                        br.ReadInt32(); // Times count
                        timesOffset = br.ReadVarint(); // Times offset
                        animFileOffset = br.ReadVarint();

                        //For DeS assert 5 int32 == 0 here
                    }
                    else
                    {
                        eventHeadersOffset = br.ReadVarint();
                        eventGroupsOffset = br.ReadVarint();
                        timesOffset = br.ReadVarint(); // Times offset
                        animFileOffset = br.ReadVarint();
                        eventCount = br.ReadInt32();
                        eventGroupCount = br.ReadInt32();
                        br.ReadInt32(); // Times count
                        br.AssertInt32(0);
                    }

                    var eventHeaderOffsets = new List<long>(eventCount);
                    var eventParameterOffsets = new List<long>(eventCount);
                    Events = new List<Event>(eventCount);
                    br.StepIn(eventHeadersOffset);
                    {
                        for (int i = 0; i < eventCount; i++)
                        {
                            eventHeaderOffsets.Add(br.Position);
                            Events.Add(Event.Read(br, out long pOffset, format));
                            eventParameterOffsets.Add(pOffset);

                            if (i > 0)
                            {
                                //  Go to previous event's parameters
                                br.StepIn(eventParameterOffsets[i - 1]);
                                {
                                    // Read the space between the previous event's parameter start and the start of this event data.
                                    long gapBetweenEventParamOffsets = eventParameterOffsets[i] - eventParameterOffsets[i - 1];
                                    // Subtract to account for the current event's type and offset 
                                    Events[i - 1].ReadParameters(br, (int)(gapBetweenEventParamOffsets - (br.VarintLong ? 16 : 8)));
                                }
                                br.StepOut();
                            }
                        }
                    }
                    br.StepOut();

                    if (eventCount > 0)
                    {
                        if (eventGroupsOffset == 0)
                        {
                            lastEventNeedsParamGen = true;
                            lastEventParamOffset = eventParameterOffsets[eventCount - 1];
                        }
                        else
                        {
                            // Go to last event's parameters
                            br.StepIn(eventParameterOffsets[eventCount - 1]);
                            {
                                // Read the space between the last event's parameter start and the start of the event groups.
                                Events[eventCount - 1].ReadParameters(br, (int)(eventGroupsOffset - eventParameterOffsets[eventCount - 1]));
                            }
                            br.StepOut();
                        }
                    }

                    EventGroups = new List<EventGroup>(eventGroupCount);
                    br.StepIn(eventGroupsOffset);
                    {
                        for (int i = 0; i < eventGroupCount; i++)
                            EventGroups.Add(new EventGroup(br, eventHeaderOffsets, format));
                    }
                    br.StepOut();

                    br.StepIn(animFileOffset);
                    {
                        AnimFileReference = br.AssertVarint([0, 1]) == 1;
                        br.AssertVarint(br.Position + (br.VarintLong ? 8 : 4));
                        long animFileNameOffset = br.ReadVarint();

                        //if (AnimFileReference)
                        //{
                        //    ReferenceID = br.ReadInt32();

                        //    UnkReferenceFlag1 = br.ReadBoolean();
                        //    ReferenceIsTAEOnly = br.ReadBoolean();
                        //    ReferenceIsHKXOnly = br.ReadBoolean();
                        //    LoopByDefault = br.ReadBoolean();
                        //}
                        //else
                        //{
                        //    UnkReferenceFlag1 = br.ReadBoolean();
                        //    ReferenceIsTAEOnly = br.ReadBoolean();
                        //    ReferenceIsHKXOnly = br.ReadBoolean();
                        //    LoopByDefault = br.ReadBoolean();

                        //    ReferenceID = br.ReadInt32();
                        //}

                        Unknown1 = br.ReadInt32();
                        Unknown2 = br.ReadInt32();

                        if (format != TAEFormat.DS1)
                        {
                            br.AssertVarint(0);
                            br.AssertVarint(0);
                        }
                        else
                        {
                            br.AssertVarint(0);

                            if (AnimFileReference)
                                br.AssertVarint(0);
                        }

                        if (animFileNameOffset < br.Length && animFileNameOffset != timesOffset)
                        {
                            if (br.GetInt64(animFileNameOffset) != 1)
                            {
                                var floatCheck = br.GetSingle(animFileNameOffset);
                                if (!(floatCheck >= 0.016667f && floatCheck <= 100))
                                {
                                    AnimFileName = br.GetUTF16(animFileNameOffset);
                                }
                            }
                        }
                        
                        AnimFileName = AnimFileName ?? "";

                        // When Reference is false, there's always a filename.
                        // When true, there's usually not, but sometimes there is, and I cannot figure out why.
                        // Thus, this stupid hack to achieve byte-perfection.
                        //var animNameCheck = AnimFileName.ToLower();
                        //if (!(animNameCheck.EndsWith(".hkt") 
                        //    || (format == TAEFormat.SDT && animNameCheck.EndsWith("hkt")) 
                        //    || animNameCheck.EndsWith(".hkx") 
                        //    || animNameCheck.EndsWith(".sib") 
                        //    || animNameCheck.EndsWith(".hkxwin")))
                        //    AnimFileName = "";

                    }
                    br.StepOut();
                }
                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int i)
            {
                bw.WriteVarint(ID);
                bw.ReserveVarint($"AnimationOffset{i}");
            }

            internal void WriteBody(BinaryWriterEx bw, int i, TAEFormat format)
            {
                bw.FillVarint($"AnimationOffset{i}", bw.Position);

                if (format == TAEFormat.DS1)
                {
                    bw.WriteInt32(Events.Count);
                    bw.ReserveVarint($"EventHeadersOffset{i}");
                    bw.WriteInt32(EventGroups.Count);
                    bw.ReserveVarint($"EventGroupHeadersOffset{i}");
                    bw.ReserveInt32($"TimesCount{i}");
                    bw.ReserveVarint($"TimesOffset{i}");
                    bw.ReserveVarint($"AnimFileOffset{i}");
                    //For DeS write 5 int32 == 0
                }
                else
                {
                    bw.ReserveVarint($"EventHeadersOffset{i}");
                    bw.ReserveVarint($"EventGroupHeadersOffset{i}");
                    bw.ReserveVarint($"TimesOffset{i}");
                    bw.ReserveVarint($"AnimFileOffset{i}");
                    bw.WriteInt32(Events.Count);
                    bw.WriteInt32(EventGroups.Count);
                    bw.ReserveInt32($"TimesCount{i}");
                    bw.WriteInt32(0);
                }
            }

            internal void WriteAnimFile(BinaryWriterEx bw, int i, TAEFormat format)
            {
                bw.FillVarint($"AnimFileOffset{i}", bw.Position);
                bw.WriteVarint(AnimFileReference ? 1 : 0);
                bw.WriteVarint(bw.Position + (bw.VarintLong ? 8 : 4));
                bw.ReserveVarint("AnimFileNameOffset");

                //if (AnimFileReference)
                //{
                //    bw.WriteInt32(ReferenceID);

                //    bw.WriteBoolean(UnkReferenceFlag1);
                //    bw.WriteBoolean(ReferenceIsTAEOnly);
                //    bw.WriteBoolean(ReferenceIsHKXOnly);
                //    bw.WriteBoolean(LoopByDefault);
                //}
                //else
                //{
                //    bw.WriteBoolean(UnkReferenceFlag1);
                //    bw.WriteBoolean(ReferenceIsTAEOnly);
                //    bw.WriteBoolean(ReferenceIsHKXOnly);
                //    bw.WriteBoolean(LoopByDefault);

                //    bw.WriteInt32(ReferenceID);
                //}

                bw.WriteInt32(Unknown1);
                bw.WriteInt32(Unknown2);

                if (format != TAEFormat.DS1)
                {
                    bw.WriteVarint(0);
                    bw.WriteVarint(0);
                }
                else
                {
                    bw.WriteVarint(0);

                    if (AnimFileReference)
                        bw.WriteVarint(0);
                }

                bw.FillVarint("AnimFileNameOffset", bw.Position);
                if (AnimFileName != "")
                {
                    bw.WriteUTF16(AnimFileName, true);

                    if (format != TAEFormat.DS1)
                        bw.Pad(0x10);
                }
            }

            internal Dictionary<float, long> WriteTimes(BinaryWriterEx bw, int animIndex, TAEFormat format)
            {
                var times = new SortedSet<float>();

                foreach (Event evt in Events)
                {
                    times.Add(evt.StartTime);
                    times.Add(evt.EndTime);
                }

                bw.FillInt32($"TimesCount{animIndex}", times.Count);

                if (times.Count == 0)
                    bw.FillVarint($"TimesOffset{animIndex}", 0);
                else
                    bw.FillVarint($"TimesOffset{animIndex}", bw.Position);

                var timeOffsets = new Dictionary<float, long>();
                foreach (float time in times)
                {
                    timeOffsets[time] = bw.Position;
                    bw.WriteSingle(time);
                }

                if (format != TAEFormat.DS1)
                    bw.Pad(0x10);

                return timeOffsets;
            }

            internal List<long> WriteEventHeaders(BinaryWriterEx bw, int animIndex, Dictionary<float, long> timeOffsets)
            {
                var eventHeaderOffsets = new List<long>(Events.Count);
                if (Events.Count > 0)
                {
                    bw.FillVarint($"EventHeadersOffset{animIndex}", bw.Position);
                    for (int i = 0; i < Events.Count; i++)
                    {
                        eventHeaderOffsets.Add(bw.Position);
                        Events[i].WriteHeader(bw, animIndex, i, timeOffsets);
                    }
                }
                else
                {
                    bw.FillVarint($"EventHeadersOffset{animIndex}", 0);
                }
                return eventHeaderOffsets;
            }

            internal void WriteEventData(BinaryWriterEx bw, int i, TAEFormat format)
            {
                for (int j = 0; j < Events.Count; j++)
                    Events[j].WriteData(bw, i, j, format);
            }

            internal void WriteEventGroupHeaders(BinaryWriterEx bw, int i, TAEFormat format)
            {
                if (EventGroups.Count > 0)
                {
                    bw.FillVarint($"EventGroupHeadersOffset{i}", bw.Position);
                    for (int j = 0; j < EventGroups.Count; j++)
                        EventGroups[j].WriteHeader(bw, i, j, format);
                }
                else
                {
                    bw.FillVarint($"EventGroupHeadersOffset{i}", 0);
                }
            }

            internal void WriteEventGroupData(BinaryWriterEx bw, int i, List<long> eventHeaderOffsets, TAEFormat format)
            {
                for (int j = 0; j < EventGroups.Count; j++)
                    EventGroups[j].WriteData(bw, i, j, eventHeaderOffsets, format);
            }
        }

        /// <summary>
        /// A group of events in an animation with an associated EventType that does not necessarily match theirs.
        /// </summary>
        public class EventGroup
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public long EventType;

            /// <summary>
            /// Indices of events in this group in the parent animation's collection.
            /// </summary>
            public List<int> Indices;

            /// <summary>
            /// Creates a new empty EventGroup with the given type.
            /// </summary>
            public EventGroup(long eventType)
            {
                EventType = eventType;
                Indices = new List<int>();
            }

            internal EventGroup(BinaryReaderEx br, List<long> eventHeaderOffsets, TAEFormat format)
            {
                long entryCount = br.ReadVarint();
                long valuesOffset = br.ReadVarint();
                long typeOffset = br.ReadVarint();
                if (format != TAEFormat.DS1)
                    br.AssertVarint(0);

                br.StepIn(typeOffset);
                {
                    EventType = br.ReadVarint();
                    if (format == TAEFormat.SOTFS)
                    {
                        br.AssertVarint(br.Position + (br.VarintLong ? 8 : 4));
                        br.AssertVarint(0);
                        br.AssertVarint(0);
                    }
                    else if (format != TAEFormat.DS1)
                    {
                        br.AssertVarint(0);
                    }
                }
                br.StepOut();

                br.StepIn(valuesOffset);
                {
                    if (format == TAEFormat.SOTFS)
                        Indices = br.ReadVarints((int)entryCount).Select(offset 
                            => eventHeaderOffsets.FindIndex(headerOffset => headerOffset == offset)).ToList();
                    else
                        Indices = br.ReadInt32s((int)entryCount).Select(offset 
                            => eventHeaderOffsets.FindIndex(headerOffset => headerOffset == offset)).ToList();
                }
                br.StepOut();
            }

            internal void WriteHeader(BinaryWriterEx bw, int i, int j, TAEFormat format)
            {
                bw.WriteVarint(Indices.Count);
                bw.ReserveVarint($"EventGroupValuesOffset{i}:{j}");
                bw.ReserveVarint($"EventGroupTypeOffset{i}:{j}");
                if (format != TAEFormat.DS1)
                    bw.WriteVarint(0);
            }

            internal void WriteData(BinaryWriterEx bw, int i, int j, List<long> eventHeaderOffsets, TAEFormat format)
            {
                bw.FillVarint($"EventGroupTypeOffset{i}:{j}", bw.Position);
                bw.WriteVarint(EventType);

                if (format == TAEFormat.SOTFS)
                {
                    bw.WriteVarint(bw.Position + (bw.VarintLong ? 8 : 4));
                    bw.WriteVarint(0);
                    bw.WriteVarint(0);
                }
                else if (format != TAEFormat.DS1)
                {
                    bw.WriteVarint(0);
                }

                bw.FillVarint($"EventGroupValuesOffset{i}:{j}", bw.Position);
                for (int k = 0; k < Indices.Count; k++)
                {
                    if (format == TAEFormat.SOTFS)
                        bw.WriteVarint(eventHeaderOffsets[Indices[k]]);
                    else
                        bw.WriteInt32((int)eventHeaderOffsets[Indices[k]]);
                }

                if (format != TAEFormat.DS1)
                    bw.Pad(0x10);
            }
        }

        /// <summary>
        /// An action or effect triggered at a certain time during an animation.
        /// </summary>
        public class Event
        {
            /// <summary>
            /// A parameter in an event.
            /// </summary>
            public class ParameterContainer
            {
                private Dictionary<string, object> parameterValues;

                /// <summary>
                /// The template of the event for which these are the parameters.
                /// </summary>
                public Template.EventTemplate Template { get; private set; }

                /// <summary>
                /// Returns all parameters.
                /// </summary>
                public IReadOnlyDictionary<string, object> Values
                    => parameterValues;

                /// <summary>
                /// Value of the specified parameter.
                /// </summary>
                public object this[string paramName]
                {
                    get => parameterValues[paramName];
                    set => parameterValues[paramName] = value;
                }

                /// <summary>
                /// Gets the value of a parameter.
                /// </summary>
                public object GetParamValue(string paramName)
                {
                    return this[paramName];
                }

                /// <summary>
                /// Gets the value type of a parameter.
                /// </summary>
                public Template.ParamType GetParamValueType(string paramName)
                {
                    return Template[paramName].Type;
                }

                /// <summary>
                /// Gets the whole template of a parameter.
                /// </summary>
                public Template.ParameterTemplate GetParamTemplate(string paramName)
                {
                    return Template[paramName];
                }

                internal ParameterContainer(long animID, int eventIndex,
                    bool bigEndian, byte[] paramData, Template.EventTemplate template)
                {
                    parameterValues = new Dictionary<string, object>();
                    Template = template;
                    var br = new BinaryReaderEx(bigEndian, paramData);
                    int i = 0;
                    foreach (var paramKvp in Template)
                    {
                        var p = paramKvp.Value;
                        if (p.ValueToAssert != null)
                        {
                            try
                            {
                                p.AssertValue(br);
                            }
                            catch (System.IO.InvalidDataException ex)
                            {
                                var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                var txtEventType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                throw new Exception($"Animation {animID}\nEvent[{eventIndex}] (Type: {txtEventType})" +
                                                    $"\n  -> Assert failed on field {txtField} (Type: {p.Type})", ex);
                            }
                        }
                        else
                        {
                                
                            try
                            {
                                parameterValues.Add(p.Name, p.ReadValue(br));
                            }
                            catch (Exception ex)
                            {
                                var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                var txtEventType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                throw new Exception($"Animation {animID}\nEvent[{eventIndex}] (Type: {txtEventType})" +
                                                    $"\n  -> Failed to read value of field {txtField} (Type: {p.Type})", ex);
                            }
                        }
                        i++;
                    }
                }

                internal ParameterContainer(bool bigEndian, byte[] paramData, Template.EventTemplate template)
                {
                    parameterValues = new Dictionary<string, object>();
                    Template = template;
                    var br = new BinaryReaderEx(bigEndian, paramData);
                    int i = 0;
                    foreach (var paramKvp in Template)
                    {
                        var p = paramKvp.Value;
                        if (p.ValueToAssert != null)
                        {
                            try
                            {
                                p.AssertValue(br);
                            }
                            catch (System.IO.InvalidDataException ex)
                            {
                                var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                var txtEventType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                throw new Exception($"Event Type: {txtEventType}" +
                                                    $"\n  -> Assert failed on field {txtField}", ex);
                            }
                        }
                        else
                        {
                            try
                            {
                                parameterValues.Add(p.Name, p.ReadValue(br));
                            }
                            catch (Exception ex)
                            {
                                var txtField = p.Name != null ? $"'{p.Name}'" : $"{(i + 1)} of {Template.Count}";
                                var txtEventType = Template.Name != null ? $"'{Template.Name}'" : Template.ID.ToString();

                                throw new Exception($"Event Type: {txtEventType}" +
                                                    $"\n  -> Failed to read value of field {txtField} (Type: {p.Type})", ex);
                            }
                        }
                        i++;
                    }
                }

                internal byte[] AsBytes(bool bigEndian)
                {
                    using (var memStream = new System.IO.MemoryStream())
                    {
                        var bw = new BinaryWriterEx(bigEndian, memStream);

                        foreach (var paramKvp in Template)
                        {
                            var p = paramKvp.Value;
                            if (p.ValueToAssert != null)
                            {
                                p.WriteValue(bw, p.ValueToAssert);
                            }
                            else
                            {
                                p.WriteValue(bw, this[p.Name]);
                            }

                        }

                        return memStream.ToArray();
                    }
                }
            }

            /// <summary>
            /// The type of this event.
            /// </summary>
            public int Type { get; private set; }

            /// <summary>
            /// An unknown 32-bit integer following the event type.
            /// So far confirmed to be used in SOTFS and SDT
            /// </summary>
            public int Unk04 { get; private set; }

            /// <summary>
            /// When the event begins.
            /// </summary>
            public float StartTime;

            /// <summary>
            /// When the event ends.
            /// </summary>
            public float EndTime;

            internal byte[] ParameterBytes;

            /// <summary>
            /// Gets the bytes of this event's parameters. This will
            /// properly return ready-to-save bytes if a template
            /// is being used, otherwise it returns the bytes
            /// read directly from the file (which may
            /// include some padding).
            /// </summary>
            public byte[] GetParameterBytes(bool bigEndian)
            {
                if (Parameters != null)
                    CopyParametersToBytes(bigEndian);
                return ParameterBytes;
            }

            /// <summary>
            /// Sets this event's parameter bytes to those specified. Updates the
            /// .Parameters template values as well if a template is applied.
            /// </summary>
            public void SetParameterBytes(bool bigEndian, byte[] parameterBytes)
            {
                if (parameterBytes.Length != ParameterBytes.Length)
                    throw new ArgumentException("Not the same amount of bytes as was originally here.");

                ParameterBytes = parameterBytes;

                if (Parameters != null)
                {
                    var prevTemplate = Template;
                    Parameters = null;
                    ApplyTemplate(bigEndian, prevTemplate);
                }
            }

            /// <summary>
            /// Indexable parameter container of this event.
            /// Use .Parameters[name] for basic value get/set
            /// and use .GetValueType(name) to see how to convert
            /// it to/from System.Object.
            /// </summary>
            public ParameterContainer Parameters { get; private set; }

            /// <summary>
            /// The EventTemplate applied to this event, if any.
            /// </summary>
            public Template.EventTemplate Template
                => Parameters?.Template ?? null;

            /// <summary>
            /// Gets the name of this event's type if a template has been loaded.
            /// Otherwise returns null.
            /// </summary>
            public string TypeName
                => Parameters?.Template?.Name;

            /// <summary>
            /// Applies a template to allow editing of the parameters.
            /// </summary>
            internal void ApplyTemplate(TAE containingTae, Template template,
                long animID, int eventIndex, int eventType)
            {
                if (template[containingTae.EventBank].ContainsKey(Type))
                {
                    if (Parameters != null)
                    {
                        CopyParametersToBytes(containingTae.BigEndian);
                    }
                    Array.Resize(ref ParameterBytes, template[containingTae.EventBank][Type].GetAllParametersByteCount());
                    Parameters = new ParameterContainer(animID, eventIndex,
                        containingTae.BigEndian, ParameterBytes, template[containingTae.EventBank][Type]);
                }
            }

            /// <summary>
            /// Applies a template to allow editing of the parameters.
            /// </summary>
            public void ApplyTemplate(bool isBigEndian, Template.EventTemplate template)
            {
                if (template.ID != Type)
                {
                    throw new ArgumentException($"Template is for event type {template.ID} but this event is type {Type}");
                }
                if (Parameters != null)
                {
                    CopyParametersToBytes(isBigEndian);
                }
                Array.Resize(ref ParameterBytes, template.GetAllParametersByteCount());
                Parameters = new ParameterContainer(isBigEndian, ParameterBytes, template);
            }

            private void CopyParametersToBytes(bool isBigEndian)
            {
                if (Parameters != null)
                    ParameterBytes = Parameters.AsBytes(isBigEndian);
            }

            /// <summary>
            /// Applies a template to this TAE for editing and also wipes all
            /// values and replaces them with default values.
            /// </summary>
            public void ApplyTemplateWithDefaultValues(bool isBigEndian, Template.EventTemplate template)
            {
                Type = template.ID;
                ParameterBytes = template.GetDefaultBytes(isBigEndian);
                Parameters = new ParameterContainer(isBigEndian, ParameterBytes, template);
            }

            /// <summary>
            /// Creates a new event with the specified start time, end time, type, and unknown then
            /// applies default values from the provided template.
            /// </summary>
            public Event(float startTime, float endTime, int type, int unk04, bool isBigEndian, Template.EventTemplate template)
            {
                StartTime = startTime;
                EndTime = endTime;
                Type = type;
                Unk04 = unk04;
                ApplyTemplateWithDefaultValues(isBigEndian, template);
            }

            /// <summary>
            /// Creates a new event with the specified start time, end time, type, unknown, and parameters.
            /// </summary>
            public Event(float startTime, float endTime, int type, int unk04, byte[] parameters, bool isBigEndianParameters)
            {
                StartTime = startTime;
                EndTime = endTime;
                Type = type;
                Unk04 = unk04;
                ParameterBytes = parameters;
                if (Template != null)
                {
                    Parameters = new ParameterContainer(isBigEndianParameters, parameters, Template);
                }
            }

            internal Event(float startTime, float endTime)
            {
                StartTime = startTime;
                EndTime = endTime;
            }

            internal void WriteHeader(BinaryWriterEx bw, int animIndex, int eventIndex, Dictionary<float, long> timeOffsets)
            {
                bw.WriteVarint(timeOffsets[StartTime]);
                bw.WriteVarint(timeOffsets[EndTime]);
                bw.ReserveVarint($"EventDataOffset{animIndex}:{eventIndex}");
            }

            internal void WriteData(BinaryWriterEx bw, int animIndex, int eventIndex, TAEFormat format)
            {
                CopyParametersToBytes(bw.BigEndian);

                bw.FillVarint($"EventDataOffset{animIndex}:{eventIndex}", bw.Position);
                bw.WriteInt32(Type);

                if (format != TAEFormat.DS1)
                    bw.WriteInt32(Unk04);

                if (format == TAEFormat.SDT && Type == 943)
                    bw.WriteVarint(0);
                else
                    bw.WriteVarint(bw.Position + (bw.VarintLong ? 8 : 4));

                bw.WriteBytes(ParameterBytes);

                if (format != TAEFormat.DS1)
                    bw.Pad(0x10);
            }

            internal void ReadParameters(BinaryReaderEx br, int byteCount)
            {
                ParameterBytes = br.ReadBytes(byteCount);
            }

            /// <summary>
            /// Returns the start time, end time, and type of the event.
            /// </summary>
            public override string ToString()
            {
                return $"{(int)Math.Round(StartTime * 30):D3} - {(int)Math.Round(EndTime * 30):D3} {Type}";
            }

            internal static Event Read(BinaryReaderEx br, out long parametersOffset, TAEFormat format)
            {
                long startTimeOffset = br.ReadVarint();
                long endTimeOffset = br.ReadVarint();
                long eventDataOffset = br.ReadVarint();
                float startTime = br.GetSingle(startTimeOffset);
                float endTime = br.GetSingle(endTimeOffset);

                Event result = new Event(startTime, endTime);
                br.StepIn(eventDataOffset);
                {
                    result.Type = br.ReadInt32();

                    if (format != TAEFormat.DS1)
                        result.Unk04 = br.ReadInt32();

                    //if (format == TAEFormat.SDT)
                    //{
                    //    // offset will be 0 in sekiro if no parameters
                    //    br.AssertVarint(br.Position + (br.VarintLong ? 8 : 4), 0);
                    //    parametersOffset = br.Position;
                    //}
                    //else
                    //{
                    //    parametersOffset = br.AssertVarint(br.Position + (br.VarintLong ? 8 : 4));
                    //}
                    br.AssertVarint([br.Position + (br.VarintLong ? 8 : 4), 0]);
                    parametersOffset = br.Position;
                }
                br.StepOut();

                return result;
            }
        }
    }
}
