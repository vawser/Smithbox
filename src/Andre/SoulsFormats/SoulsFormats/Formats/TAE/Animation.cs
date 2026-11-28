using System;
using System.Collections.Generic;
using System.Linq;

namespace SoulsFormats
{
    public partial class TAE
    {

        /// <summary>
        /// Controls an individual animation.
        /// </summary>
        public class Animation
        {
            public enum MiniHeaderType : uint
            {
                /// <summary>
                /// Standard AnimMiniHeader with three flags, 2 of which can reference specific parts of another animation.
                /// </summary>
                Standard = 0,

                /// <summary>
                /// AnimMiniHeader that signifies that the animation fully imports the motion data and all events from another animation.
                /// </summary>
                ImportOtherAnim = 1
            }

            public abstract class AnimMiniHeader
            {
                /// <summary>
                /// Type of AnimMiniHeader that this is.
                /// </summary>
                public abstract MiniHeaderType Type { get; }
                internal abstract void ReadInner(BinaryReaderEx br, TAEFormat format);
                internal abstract void WriteInner(BinaryWriterEx bw, TAEFormat format);

                /// <summary>
                /// Gets a clone of this not tied by reference.
                /// </summary>
                public abstract AnimMiniHeader GetClone();

                /// <summary>
                /// Standard MiniHeader with three flags, 2 of which can reference specific parts of another animation.
                /// </summary>
                public sealed class Standard : AnimMiniHeader
                {
                    /// <summary>
                    /// Type of AnimMiniHeader that this is.
                    /// </summary>
                    public override MiniHeaderType Type => MiniHeaderType.Standard;

                    /// <summary>
                    /// Gets a clone of this not tied by reference.
                    /// </summary>
                    public override AnimMiniHeader GetClone()
                    {
                        var newClone = new Standard();

                        newClone.IsLoopByDefault = IsLoopByDefault;
                        newClone.AllowDelayLoad = AllowDelayLoad;
                        newClone.ImportsHKX = ImportsHKX;
                        newClone.DesLeftoverBytes = DesLeftoverBytes;
                        newClone.DesLoopByte = DesLoopByte;
                        newClone.DesImportsByte = DesImportsByte;

                        newClone.ImportHKXSourceAnimID = ImportHKXSourceAnimID;
                        newClone.Unk03 = Unk03;

                        return newClone;
                    }

                    /// <summary>
                    /// Makes the animation loop by default. Only relevant for animations not controlled by
                    /// ESD or HKS such as ObjAct animations.
                    /// </summary>
                    public bool IsLoopByDefault { get; set; } = false;

                    /// <summary>
                    /// Whether to import the HKX (actual motion data) of the animation with the ID of <see cref="ImportHKXSourceAnimID"/>.
                    /// </summary>
                    public bool ImportsHKX { get; set; } = false;

                    /// <summary>
                    /// Whether to allow this animation to be loaded from delayload anibnds such as the c0000_cXXXX.anibnd player throw anibnds.
                    /// </summary>
                    public bool AllowDelayLoad { get; set; } = false;

                    /// <summary>
                    /// Anim ID to import HKX from. Only functional if
                    /// <see cref="ImportsHKX"/> is enabled.
                    /// </summary>
                    public int ImportHKXSourceAnimID { get; set; } = 0;

                    /// <summary>
                    /// The byte after the delay load flag (not DES / DESR), 0 except in AC6 movtae anim 14 (1). Unknown.
                    /// </summary>
                    public byte Unk03 { get; set; } = 0;

                    /// <summary>
                    /// DES only: the bytes where later games keep the delay load flag and a pad byte.
                    /// The exporter left stale memory there (values near the struct's own offset, 0xEEEF), so the
                    /// flag is ignored for DES; they are kept only to write the file back as it was read.
                    /// </summary>
                    private ushort DesLeftoverBytes;

                    /// <summary>
                    /// DES only: the loop and import bytes as read. Some hold stale values (0x2E, 0x8C) rather than 0 or 1;
                    /// a byte is written back as read while its flag is unchanged.
                    /// </summary>
                    private byte DesLoopByte, DesImportsByte;

                    internal override void ReadInner(BinaryReaderEx br, TAEFormat format)
                    {
                        if (format == TAEFormat.DESR)
                        {
                            ImportHKXSourceAnimID = br.ReadInt32();

                            IsLoopByDefault = br.ReadByte() != 0;
                            ImportsHKX = br.ReadByte() != 0;
                            AllowDelayLoad = br.ReadByte() != 0;
                            AllowDelayLoad = false;
                            br.ReadByte();

                            
                        }
                        else
                        {
                            DesLoopByte = br.ReadByte();
                            DesImportsByte = br.ReadByte();
                            IsLoopByDefault = DesLoopByte != 0;
                            ImportsHKX = DesImportsByte != 0;

                            if (format == TAEFormat.DES)
                            {
                                AllowDelayLoad = false;
                                DesLeftoverBytes = br.ReadUInt16();
                            }
                            else
                            {
                                AllowDelayLoad = br.ReadByte() != 0;
                                Unk03 = br.ReadByte();
                            }

                            ImportHKXSourceAnimID = br.ReadInt32();
                        }
                        
                    }

                    internal override void WriteInner(BinaryWriterEx bw, TAEFormat format)
                    {
                        if (format is TAEFormat.DESR)
                        {
                            bw.WriteInt32(ImportHKXSourceAnimID);
                            bw.WriteBoolean(IsLoopByDefault);
                            bw.WriteBoolean(ImportsHKX);
                            bw.WriteBoolean(false);
                            bw.WriteByte(0);
                        }
                        else
                        {
                            if (format == TAEFormat.DES)
                            {
                                bw.WriteByte(IsLoopByDefault == (DesLoopByte != 0) ? DesLoopByte : (byte)(IsLoopByDefault ? 1 : 0));
                                bw.WriteByte(ImportsHKX == (DesImportsByte != 0) ? DesImportsByte : (byte)(ImportsHKX ? 1 : 0));
                                bw.WriteUInt16(DesLeftoverBytes);
                            }
                            else
                            {
                                bw.WriteBoolean(IsLoopByDefault);
                                bw.WriteBoolean(ImportsHKX);
                                bw.WriteBoolean(AllowDelayLoad);
                                bw.WriteByte(Unk03);
                            }

                            bw.WriteInt32(ImportHKXSourceAnimID);
                        }
                        
                    }
                }

                /// <summary>
                /// AnimMiniHeader that signifies that the animation fully imports the motion data and all events from another animation.
                /// </summary>
                public sealed class ImportOtherAnim : AnimMiniHeader
                {
                    /// <summary>
                    /// Type of AnimMiniHeader that this is.
                    /// </summary>
                    public override MiniHeaderType Type => MiniHeaderType.ImportOtherAnim;

                    /// <summary>
                    /// Gets a clone of this not tied by reference.
                    /// </summary>
                    public override AnimMiniHeader GetClone()
                    {
                        var newClone = new ImportOtherAnim();

                        newClone.ImportFromAnimID = ImportFromAnimID;
                        newClone.Unknown = Unknown;

                        return newClone;
                    }

                    /// <summary>
                    /// ID of animation from which to import motion dat and all events.
                    /// </summary>
                    public int ImportFromAnimID { get; set; } = 0;

                    /// <summary>
                    /// Unknown usage.
                    /// </summary>
                    public int Unknown { get; set; } = -1;

                    internal override void ReadInner(BinaryReaderEx br, TAEFormat format)
                    {
                        if (format is TAEFormat.DESR)
                        {
                            Unknown = br.ReadInt32();
                            ImportFromAnimID = br.ReadInt32();
                        }
                        else
                        {
                            ImportFromAnimID = br.ReadInt32();
                            Unknown = br.ReadInt32();
                        }
                        

                        if (format == TAEFormat.DES)
                            br.Pad(0x10);
                    }

                    internal override void WriteInner(BinaryWriterEx bw, TAEFormat format)
                    {
                        if (format == TAEFormat.DESR)
                        {
                            bw.WriteInt32(Unknown);
                            bw.WriteInt32(ImportFromAnimID);
                        }
                        else
                        {
                            bw.WriteInt32(ImportFromAnimID);
                            bw.WriteInt32(Unknown);
                        }
                        

                        if (format == TAEFormat.DES)
                            bw.Pad(0x10);
                    }
                }
            }

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
            /// The mini-header of this animation entry.
            /// </summary>
            public AnimMiniHeader MiniHeader { get; set; } = null;

            /// <summary>
            /// Unknown.
            /// </summary>
            public string AnimFileName { get; set; }

            /// <summary>
            /// The anim file struct was empty: type 0 and a null name offset offset, 16 bytes with no mini header or name
            /// after it (AC6 c0100/c0200/c0300 a330/a331, the first animation). Written back the same way while the
            /// mini header and name stay at their defaults.
            /// </summary>
            private bool EmptyAnimFile;

            /// <summary>
            /// The anim file struct ended after the mini header, without the two zero varints, and the name offset
            /// pointed at its end, where the name is if there is one (AC6 /other/movtae.movtae, every animation; some
            /// name a movie, "1010.mp4"). Written the same way.
            /// </summary>
            private bool NoAnimFileTail;

            /// <summary>
            /// The offset of the body this animation was read from
            /// </summary>
            internal long ReadBodyOffset = -1;

            /// <summary>
            /// The times table as read, written again along with the times the events use now
            /// </summary>
            internal List<float> ReadTimes;

            /// <summary>
            /// The earlier animation whose body this one shared when read (AC6 asset TAEs: several animation IDs
            /// pointing at one body), or null. Written shared again while the two are still identical and both listed.
            /// </summary>
            internal Animation SharedBodySource;

            /// <summary>
            /// Whether everything the body and its data hold (all but the ID) is the same as in the other animation
            /// </summary>
            internal bool HasSameBodyAs(Animation other, bool bigEndian)
            {
                if (AnimFileName != other.AnimFileName || EmptyAnimFile != other.EmptyAnimFile
                    || NoAnimFileTail != other.NoAnimFileTail
                    || Events.Count != other.Events.Count || EventGroups.Count != other.EventGroups.Count)
                    return false;

                switch (MiniHeader, other.MiniHeader)
                {
                    case (AnimMiniHeader.Standard a, AnimMiniHeader.Standard b):
                        if (a.IsLoopByDefault != b.IsLoopByDefault || a.ImportsHKX != b.ImportsHKX
                            || a.AllowDelayLoad != b.AllowDelayLoad || a.ImportHKXSourceAnimID != b.ImportHKXSourceAnimID
                            || a.Unk03 != b.Unk03)
                            return false;
                        break;
                    case (AnimMiniHeader.ImportOtherAnim a, AnimMiniHeader.ImportOtherAnim b):
                        if (a.ImportFromAnimID != b.ImportFromAnimID || a.Unknown != b.Unknown)
                            return false;
                        break;
                    default:
                        return false;
                }

                for (int i = 0; i < Events.Count; i++)
                {
                    Event a = Events[i], b = other.Events[i];
                    if (a.StartTime != b.StartTime || a.MemeEndTime != b.MemeEndTime || a.Unk04 != b.Unk04
                        || !a.IsIdenticalTo(b, bigEndian)
                        || EventGroups.IndexOf(a.Group) != other.EventGroups.IndexOf(b.Group))
                        return false;
                }

                for (int i = 0; i < EventGroups.Count; i++)
                {
                    EventGroup a = EventGroups[i], b = other.EventGroups[i];
                    if (a.GroupType != b.GroupType || !a.GroupData.Equals(b.GroupData)
                        || !(a.RawGroupData ?? []).SequenceEqual(b.RawGroupData ?? []))
                        return false;
                }

                return true;
            }

            /// <summary>
            /// Creates a new empty animation with the specified properties.
            /// </summary>
            public Animation(long id, AnimMiniHeader miniHeader, string animFileName)
            {
                ID = id;
                MiniHeader = miniHeader;
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
                ReadBodyOffset = offset;

                if (format == TAEFormat.DES) // Not in DESR
                {
                    br.Pad(0x10);
                }

                br.StepIn(offset);
                {
                    int eventCount;
                    long eventHeadersOffset;
                    int eventGroupCount;
                    long eventGroupsOffset;
                    long timesOffset;
                    int timesCount;

                    if (format == TAEFormat.DS1 || format == TAEFormat.DES)
                    {
                        eventCount = br.ReadInt32();
                        eventHeadersOffset = br.ReadVarint();
                        eventGroupCount = br.ReadInt32();
                        eventGroupsOffset = br.ReadVarint();
                        timesCount = br.ReadInt32();
                        timesOffset = br.ReadVarint(); // Times offset
                        animFileOffset = br.ReadVarint();

                        //For DeS assert 5 int32 == 0 here
                        if (format == TAEFormat.DES)
                        {
                            for (int i = 0; i < 5; i++)
                                br.AssertInt32(0);
                        }
                    }
                    else if (format == TAEFormat.DESR)
                    {
                        eventCount = br.ReadInt32();
                        eventGroupCount = br.ReadInt32();
                        timesCount = br.ReadInt32();
                        br.AssertInt32(0);
                        eventHeadersOffset = br.ReadVarint();
                        eventGroupsOffset = br.ReadVarint();
                        timesOffset = br.ReadVarint(); // Times offset
                        animFileOffset = br.ReadVarint();
                    }
                    else
                    {
                        eventHeadersOffset = br.ReadVarint();
                        eventGroupsOffset = br.ReadVarint();
                        timesOffset = br.ReadVarint(); // Times offset
                        animFileOffset = br.ReadVarint();
                        eventCount = br.ReadInt32();
                        eventGroupCount = br.ReadInt32();
                        timesCount = br.ReadInt32();
                        br.AssertInt32(0);
                    }

                    // Kept, as the table can hold times no event uses any more (AC6 AEG120_106: 54 times, 44 used)
                    ReadTimes = new List<float>(Math.Max(0, timesCount));
                    if (timesOffset > 0)
                    {
                        for (int t = 0; t < timesCount; t++)
                            ReadTimes.Add(br.GetSingle(timesOffset + t * 4));
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
                                    // Less this event's data header: type and parameters offset, plus the unknown in
                                    // the 64-bit formats; DES pads its header to 0x10 (type, offset, 0, 0)
                                    int eventDataHeaderSize = br.VarintLong || format == TAEFormat.DES ? 16 : 8;
                                    Events[i - 1].ReadParameters(br, (int)(gapBetweenEventParamOffsets - eventDataHeaderSize));
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

                    foreach (var grp in EventGroups)
                    {
                        grp.ReadOrder = new List<Event>(grp.indices.Count);

                        foreach (var idx in grp.indices)
                        {
                            var ev = Events[idx];
                            if (ev.Group == null)
                                ev.Group = grp;
                            else
                                throw new Exception("TAE Event in multiple groups...");

                            grp.ReadOrder.Add(ev);
                        }
                    }

                    br.StepIn(animFileOffset);
                    {
                        var miniHeaderType = br.ReadEnum32<MiniHeaderType>();

                        if (br.VarintLong)
                            br.AssertInt32(0);

                        var fileNameOffsetOffset = br.GetNextPaddedOffsetAfterCurrentField(br.VarintSize, format == TAEFormat.DES ? 0x10 : 0);

                        // An empty struct: no name offset offset, and nothing after it
                        if (br.VarintLong && format != TAEFormat.DESR && miniHeaderType == MiniHeaderType.Standard
                            && br.GetInt64(br.Position) == 0)
                        {
                            EmptyAnimFile = true;
                            MiniHeader = new AnimMiniHeader.Standard();
                            AnimFileName = "";
                        }
                        else
                        {

                        // Offset being read as 32bit int to deal with bad data in the upper 32bits of the offsets in DESR
                        br.AssertInt32((int)fileNameOffsetOffset);
                        if (br.VarintLong)
                            br.ReadInt32();

                        br.Position = fileNameOffsetOffset;
                        // Offset being read as 32bit int to deal with bad data in the upper 32bits of the offsets in DESR
                        int animFileNameOffset = br.ReadInt32();
                        if (br.VarintLong)
                            br.ReadInt32();

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

                        if (miniHeaderType == MiniHeaderType.Standard)
                        {
                            MiniHeader = new AnimMiniHeader.Standard();
                        }
                        else if (miniHeaderType == MiniHeaderType.ImportOtherAnim)
                        {
                            MiniHeader = new AnimMiniHeader.ImportOtherAnim();
                        }
                        else
                        {
                            throw new NotImplementedException($"{nameof(AnimMiniHeader)} type not implemented yet.");
                        }

                        MiniHeader.ReadInner(br, format);

                        if (!(format == TAEFormat.DES || format == TAEFormat.DS1 || format == TAEFormat.DESR))
                        {
                            // The name offset pointing here means the struct ends without its two zeros
                            NoAnimFileTail = animFileNameOffset == br.Position;

                            if (!NoAnimFileTail)
                            {
                                br.AssertVarint(0);
                                br.AssertVarint(0);
                            }
                        }
                        else
                        {
                            // Check for end of file for certain DESR files where this struct is the very last thing
                            // and it does not have the padding in such case
                            if (br.Position < br.Length)
                            {
                                if (format == TAEFormat.DESR)
                                {
                                    //br.AssertInt32(0);

                                    //if (MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
                                    //    br.AssertInt32(0);
                                }
                                else
                                {
                                    br.AssertVarint(0);

                                    if (MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
                                        br.AssertVarint(0);
                                }
                            }
                            
                        }

                        // A nameless animation's name offset points at whatever follows its struct: the end of the
                        // file, the times table, or the next animation's struct. Read there, that struct's type
                        // (0 or 1) came back as a name of one control character, which was then written out as text
                        // (4 stray bytes per DS1 ImportOtherAnim animation; one left last ran a re-read off the end).
                        if (animFileNameOffset + 2 <= br.Length && animFileNameOffset != timesOffset
                            && !IsAnimFileStructAt(br, animFileNameOffset, format))
                        {
                            if (animFileNameOffset + 8 > br.Length || br.GetInt64(animFileNameOffset) != 1)
                            {
                                var floatCheck = animFileNameOffset + 4 <= br.Length ? br.GetSingle(animFileNameOffset) : 0;
                                if (!(floatCheck >= 0.016667f && floatCheck <= 100))
                                {
                                    AnimFileName = br.GetUTF16(animFileNameOffset);
                                }
                            }
                        }

                        AnimFileName = AnimFileName ?? "";

                        // Files saved by the versions that read such a name hold it as text: it is no name either
                        if (AnimFileName.Length > 0 && AnimFileName.All(c => c < 0x20))
                            AnimFileName = "";

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
                    }
                    br.StepOut();
                }
                br.StepOut();
            }

            /// <summary>
            /// Whether an animation file struct starts at the offset: a mini header type (Standard or ImportOtherAnim)
            /// followed by its name offset offset, which points just past itself (padded to 0x10 in DES)
            /// </summary>
            private static bool IsAnimFileStructAt(BinaryReaderEx br, long offset, TAEFormat format)
            {
                var size = br.VarintSize;

                if (offset + 2 * size > br.Length)
                    return false;

                var type = br.GetInt32(offset);

                if (type != (int)MiniHeaderType.Standard && type != (int)MiniHeaderType.ImportOtherAnim)
                    return false;

                var nameOffsetOffset = size == 8 ? br.GetInt64(offset + 8) : br.GetInt32(offset + 4);
                var afterField = offset + 2 * size;

                return format == TAEFormat.DES
                    ? nameOffsetOffset >= afterField && nameOffsetOffset < afterField + 0x10
                    : nameOffsetOffset == afterField;
            }

            internal void WriteHeader(BinaryWriterEx bw, int i, TAEFormat format)
            {
                bw.WriteVarint(ID);
                bw.ReserveVarint($"AnimationOffset{i}");
                if (format == TAEFormat.DES)
                    bw.Pad(0x10);
            }

            internal void WriteBody(BinaryWriterEx bw, int i, TAEFormat format)
            {
                bw.FillVarint($"AnimationOffset{i}", bw.Position);

                // redoing the event groups causes unnecessary diffing
                /*EventGroups.Clear();
                foreach (var ev in Events)
                {
                    if (ev.Group != null && !EventGroups.Contains(ev.Group))
                        EventGroups.Add(ev.Group);
                }*/

                if (format == TAEFormat.DS1 || format == TAEFormat.DES)
                {
                    bw.WriteInt32(Events.Count);
                    bw.ReserveVarint($"EventHeadersOffset{i}");
                    bw.WriteInt32(EventGroups.Count);
                    bw.ReserveVarint($"EventGroupHeadersOffset{i}");
                    bw.ReserveInt32($"TimesCount{i}");
                    bw.ReserveVarint($"TimesOffset{i}");
                    bw.ReserveVarint($"AnimFileOffset{i}");
                    //For DeS write 5 int32 == 0
                    if (format == TAEFormat.DES)
                        for (int j = 0; j < 5; j++)
                            bw.WriteInt32(0);
                }
                else if (format is TAEFormat.DESR)
                {
                    bw.WriteInt32(Events.Count);
                    bw.WriteInt32(EventGroups.Count);
                    bw.ReserveInt32($"TimesCount{i}");
                    bw.WriteInt32(0);
                    bw.ReserveVarint($"EventHeadersOffset{i}");
                    bw.ReserveVarint($"EventGroupHeadersOffset{i}");
                    bw.ReserveVarint($"TimesOffset{i}");
                    bw.ReserveVarint($"AnimFileOffset{i}");
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
                bw.WriteVarint((int)MiniHeader.Type);

                // Kept empty while nothing has been set that the full struct would hold
                if (EmptyAnimFile && bw.VarintLong && format != TAEFormat.DESR && string.IsNullOrEmpty(AnimFileName) && MiniHeader is AnimMiniHeader.Standard
                    {
                        IsLoopByDefault: false, ImportsHKX: false, AllowDelayLoad: false, ImportHKXSourceAnimID: 0, Unk03: 0
                    })
                {
                    bw.WriteVarint(0);
                    return;
                }


                bw.ReserveVarint("AnimFileNameOffsetOffset");
                if (format is TAEFormat.DES) // Not in DESR
                    bw.Pad(0x10);
                bw.FillVarint("AnimFileNameOffsetOffset", bw.Position);

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

                MiniHeader.WriteInner(bw, format);

                if (!(format == TAEFormat.DES || format == TAEFormat.DS1 || format == TAEFormat.DESR))
                {
                    // Without them the name, if there is one, follows the mini header (movtae "1010.mp4")
                    if (!NoAnimFileTail)
                    {
                        bw.WriteVarint(0);
                        bw.WriteVarint(0);
                    }
                }
                else
                {
                    if (format is TAEFormat.DESR)
                    {
                        bw.WriteInt32(0);

                        if (MiniHeader.Type is MiniHeaderType.ImportOtherAnim)
                            bw.WriteInt32(0);
                    }
                    else
                    {
                        bw.WriteVarint(0);

                        if (MiniHeader.Type is MiniHeaderType.ImportOtherAnim)
                            bw.WriteVarint(0);
                    }
                }

                // DES aligns the name to 0x10; only ImportOtherAnim structs don't already end aligned
                if (format == TAEFormat.DES)
                    bw.Pad(0x10);

                bw.FillVarint("AnimFileNameOffset", bw.Position);
                if (!string.IsNullOrWhiteSpace(AnimFileName))
                {
                    bw.WriteUTF16(AnimFileName, true);

                    if (format != TAEFormat.DS1)
                        bw.Pad(0x10);
                }
            }

            internal Dictionary<float, long> WriteTimes(BinaryWriterEx bw, int animIndex, TAEFormat format)
            {
                var times = new SortedSet<float>();

                if (ReadTimes != null)
                {
                    foreach (var time in ReadTimes)
                        times.Add(time);
                }

                foreach (Event evt in Events)
                {
                    times.Add(evt.StartTime);
                    times.Add(evt.MemeEndTime);
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

            internal List<long> WriteEventHeaders(BinaryWriterEx bw, int animIndex, Dictionary<float, long> timeOffsets, TAEFormat format)
            {
                var eventHeaderOffsets = new List<long>(Events.Count);
                if (Events.Count > 0)
                {
                    bw.FillVarint($"EventHeadersOffset{animIndex}", bw.Position);
                    for (int i = 0; i < Events.Count; i++)
                    {
                        eventHeaderOffsets.Add(bw.Position);
                        Events[i].WriteHeader(bw, animIndex, i, timeOffsets, format);
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
                    {
                        var members = Events.Where(ev => ev.Group == EventGroups[j]).ToList();

                        // The order as read (not always ascending in AC6), while the group still has the same events
                        var read = EventGroups[j].ReadOrder;
                        var order = read != null && read.Count == members.Count &&
                            read.ToHashSet(ReferenceEqualityComparer.Instance).SetEquals(members)
                            ? read : members;

                        EventGroups[j].indices = order.Select(ev => Events.IndexOf(ev)).ToList();
                        EventGroups[j].WriteHeader(bw, i, j, format);
                    }
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

    }
}
