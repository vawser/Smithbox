using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1 : SoulsFile<FXR1>
    {
        public bool BigEndian { get; set; } = false;
        public bool Wide { get; set; } = false;
        public int Unk1 { get; set; }
        public int Unk2 { get; set; }

        /// <summary>
        /// 1: FXR v1 (DS1, DS1R). 2: FXR v2 (Bloodborne): 32-byte containers holding a small record instead of
        /// ActionData and modifier, a new root layout and container nodes 141–148 (FX.md, FFX §19). Always wide.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>The version of the file being read or written on this thread, for the object readers and writers.</summary>
        [ThreadStatic] internal static int currentVersion;

        internal static bool IsV2 => currentVersion == 2;

        /// <summary>FXR v2 reading: where the packed tail starts (the header's 141 list offset).</summary>
        [ThreadStatic] internal static int v2TailStart;

        public FXNode RootFXNode { get; set; }

        // Ported from DSAnimStudio's SoulsAssetPipeline (Claude/Reference/DSAnimStudio, FX.md, FFX §8.3, F4).
        // The XML read/write entry points were dropped (they pulled XmlSerializer into SoulsFormats); the XID
        // flattening below has no callers left and goes once the F4 writer is settled.


        [XmlIgnore]
        internal static bool FlattenStates = true;
        [XmlIgnore]
        internal static bool FlattenFXNodePointers = false;
        [XmlIgnore]
        internal static bool FlattenTransitions = true;
        [XmlIgnore]
        internal static bool FlattenFXActions = false;
        [XmlIgnore]
        internal static bool FlattenFXNodes = false;
        [XmlIgnore]
        internal static bool FlattenFXContainers = false;
        [XmlIgnore]
        internal static bool FlattenFXActionDatas = false;
        [XmlIgnore]
        internal static bool FlattenModifiers = false;

        public List<FXState> AllStates { get; set; } = new List<FXState>();
        public List<FXNodePointer> AllFXNodePointers { get; set; } = new List<FXNodePointer>();
        public List<FXTransition> AllTransitions { get; set; } = new List<FXTransition>();
        public List<FXAction> AllFXActions { get; set; } = new List<FXAction>();
        public List<FXNode> AllFXNodes { get; set; } = new List<FXNode>();
        public List<FXContainer> AllFXContainers { get; set; } = new List<FXContainer>();
        public List<FXActionData> AllFXActionDatas { get; set; } = new List<FXActionData>();
        public List<FXModifier> AllModifiers { get; set; } = new List<FXModifier>();



        public bool ShouldSerializeAllStates() => FlattenStates;
        public bool ShouldSerializeAllFXNodePointers() => FlattenFXNodePointers;
        public bool ShouldSerializeAllTransitions() => FlattenTransitions;
        public bool ShouldSerializeAllFXActions() => FlattenFXActions;
        public bool ShouldSerializeAllFXNodes() => FlattenFXNodes;
        public bool ShouldSerializeAllFXContainers() => FlattenFXContainers;
        public bool ShouldSerializeAllFXActionDatas() => FlattenFXActionDatas;
        public bool ShouldSerializeAllModifiers() => FlattenModifiers;


        internal FXState GetState(string xid) => AllStates.FirstOrDefault(x => x.XID == xid);
        internal FXState DereferenceState(FXState v)
        {
            if (!FlattenStates)
                return v;

            if (v is FXStateRef asRef)
                return GetState(asRef.ReferenceXID);
            else
                return v;
        }
        internal FXState ReferenceState(FXState v)
        {
            if (!FlattenStates)
                return v;

            if (v is FXStateRef asRef)
                return v;
            else
                return new FXStateRef(v);
        }


        internal FXNodePointer GetFXNodePointer(string xid) => AllFXNodePointers.FirstOrDefault(x => x.XID == xid);
        internal FXNodePointer DereferenceFXNodePointer(FXNodePointer v)
        {
            if (!FlattenFXNodePointers)
                return v;

            if (v is FXNodePointerRef asRef)
                return GetFXNodePointer(asRef.ReferenceXID);
            else
                return v;
        }
        internal FXNodePointer ReferenceFXNodePointer(FXNodePointer v)
        {
            if (!FlattenFXNodePointers)
                return v;

            if (v is FXNodePointerRef asRef)
                return v;
            else
                return new FXNodePointerRef(v);
        }


        internal FXTransition GetTransition(string xid) => AllTransitions.FirstOrDefault(x => x.XID == xid);
        internal FXTransition DereferenceTransition(FXTransition v)
        {
            if (!FlattenTransitions)
                return v;

            if (v is FXTransitionRef asRef)
                return GetTransition(asRef.ReferenceXID);
            else
                return v;
        }

        internal FXTransition ReferenceTransition(FXTransition v)
        {
            if (!FlattenTransitions)
                return v;

            if (v is FXTransitionRef asRef)
                return v;
            else
                return new FXTransitionRef(v);
        }


        internal FXAction GetFXAction(string xid) => AllFXActions.FirstOrDefault(x => x.XID == xid);
        internal FXAction DereferenceFXAction(FXAction v)
        {
            if (!FlattenFXActions)
                return v;

            if (v is FXActionRef asRef)
                return GetFXAction(asRef.ReferenceXID);
            else
                return v;
        }
        internal FXAction ReferenceFXAction(FXAction v)
        {
            if (!FlattenFXActions)
                return v;

            if (v is FXActionRef asRef)
                return v;
            else
                return new FXActionRef(v);
        }


        internal FXNode GetFXNode(string xid) => AllFXNodes.FirstOrDefault(x => x.XID == xid);
        internal FXNode DereferenceFXNode(FXNode v)
        {
            if (!FlattenFXNodes)
                return v;

            if (v is FXNode.FXNodeRef asRef)
                return GetFXNode(asRef.ReferenceXID);
            else
                return v;
        }
        internal FXNode ReferenceFXNode(FXNode v)
        {
            if (!FlattenFXNodes)
                return v;

            if (v is FXNode.FXNodeRef asRef)
                return v;
            else
                return new FXNode.FXNodeRef(v);
        }


        internal FXContainer GetFXContainer(string xid) => AllFXContainers.FirstOrDefault(x => x.XID == xid);
        internal FXContainer DereferenceFXContainer(FXContainer v)
        {
            if (!FlattenFXContainers)
                return v;

            if (v is FXContainerRef asRef)
                return GetFXContainer(asRef.ReferenceXID);
            else
                return v;
        }
        internal FXContainer ReferenceFXContainer(FXContainer v)
        {
            if (!FlattenFXContainers)
                return v;

            if (v is FXContainerRef asRef)
                return v;
            else
                return new FXContainerRef(v);
        }


        internal FXActionData GetActionData(string xid) => AllFXActionDatas.FirstOrDefault(x => x.XID == xid);
        internal FXActionData DereferenceFXActionData(FXActionData v)
        {
            if (!FlattenFXActionDatas)
                return v;

            if (v is ActionDataRef asRef)
                return GetActionData(asRef.ReferenceXID);
            else
                return v;
        }
        internal FXActionData ReferenceFXActionData(FXActionData v)
        {
            if (!FlattenFXActionDatas)
                return v;

            if (v is ActionDataRef asRef)
                return v;
            else
                return new ActionDataRef(v);
        }


        internal FXModifier GetModifier(string xid) => AllModifiers.FirstOrDefault(x => x.XID == xid);
        internal FXModifier DereferenceModifier(FXModifier v)
        {
            if (!FlattenModifiers)
                return v;

            if (v is FXModifierRef asRef)
                return GetModifier(asRef.ReferenceXID);
            else
                return v;
        }
        internal FXModifier ReferenceModifier(FXModifier v)
        {
            if (!FlattenModifiers)
                return v;

            if (v is FXModifierRef asRef)
                return v;
            else
                return new FXModifierRef(v);
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.AssertASCII("FXR\0");
            int endianCheck = br.AssertInt32([0x10000, 0x100, 0x20000]);
            br.BigEndian = BigEndian = (endianCheck == 0x100);
            Version = endianCheck == 0x20000 ? 2 : 1;
            currentVersion = Version;

            uint longCheck = br.GetUInt32(0xC);
            br.VarintLong = Wide = (longCheck == 0 || longCheck == 0xCDCDCDCD);
            Garbage.Clear();
            readingGarbage = Garbage;

            long mainDataOffset = ReadFXR1Varint(br);
            int metadataTableOffset = br.ReadInt32();

            int pointerTableCount = br.ReadInt32();
            int functionTableCount = br.ReadInt32();
            Unk1 = br.ReadInt32();
            Unk2 = br.ReadInt32();
            v2TailStart = Version == 2 ? Unk2 : int.MaxValue;

            var env = new FxrEnvironment();

            AllFXActionDatas.Clear();
            AllModifiers.Clear();
            AllFXContainers.Clear();
            AllFXActions.Clear();
            AllTransitions.Clear();
            AllStates.Clear();
            AllFXNodes.Clear();
            AllFXNodePointers.Clear();

            env.fxr = this;

            br.StepIn(metadataTableOffset);
            env.ReadPointerTable(br, pointerTableCount);
            br.StepOut();

            //br.Pad(16);

            RootFXNode = env.GetFXNode(br, mainDataOffset);
            readingGarbage = null;
            currentVersion = 1;

            void Register<T>(string type, long offset, List<T> list, T thing)
                where T : XIDable
            {
                thing.XID = $"{type}_0x{offset:X}";

                if (!list.Contains(thing))
                    list.Add(thing);
            }

            foreach (var kvp in env.ObjectsByOffset)
            {
                //switch (kvp.Value)
                //{
                //    case FXState state: Register("FXState", kvp.Key, AllStates, state); break;
                //    case FXTransition transition: Register("FXTransition", kvp.Key, AllTransitions, transition); break;
                //}

                if (kvp.Value is FXActionData asActionData)
                {
                    Register("ActionData", kvp.Key, AllFXActionDatas, asActionData);
                }
                else if (kvp.Value is FXRecord)
                {
                    // FXR v2 records have no list of their own
                }
                else if (kvp.Value is FXModifier asModifier)
                {
                    Register("Modifier", kvp.Key, AllModifiers, asModifier);
                }
                else if (kvp.Value is FXContainer asFXContainer)
                {
                    Register("FXContainer", kvp.Key, AllFXContainers, asFXContainer);
                }
                else if (kvp.Value is FXAction asFXAction)
                {
                    Register("FXAction", kvp.Key, AllFXActions, asFXAction);
                }
                else if (kvp.Value is FXTransition asTransition)
                {
                    Register("FXTransition", kvp.Key, AllTransitions, asTransition);
                }
                else if (kvp.Value is FXState asState)
                {
                    Register("FXState", kvp.Key, AllStates, asState);
                }
                else if (kvp.Value is FXNode asFXNode)
                {
                    Register("FXNode", kvp.Key, AllFXNodes, asFXNode);
                }
                else if (kvp.Value is FXNodePointer asFXNodePointer)
                {
                    Register("FXNodePointer", kvp.Key, AllFXNodePointers, asFXNodePointer);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void Flatten()
        {
            foreach (var x in AllFXActionDatas)
                x.ToXIDs(this);
            foreach (var x in AllModifiers)
                x.ToXIDs(this);
            foreach (var x in AllFXContainers)
                x.ToXIDs(this);
            foreach (var x in AllFXActions)
                x.ToXIDs(this);
            foreach (var x in AllTransitions)
                x.ToXIDs(this);
            foreach (var x in AllStates)
                x.ToXIDs(this);
            foreach (var x in AllFXNodes)
                x.ToXIDs(this);
            foreach (var x in AllFXNodePointers)
                x.ToXIDs(this);

            RootFXNode.ToXIDs(this);
            //RootFXNode = ReferenceFXNode(RootFXNode);
        }

        public void Unflatten()
        {
            foreach (var x in AllFXActionDatas)
                x.FromXIDs(this);
            foreach (var x in AllModifiers)
                x.FromXIDs(this);
            foreach (var x in AllFXContainers)
                x.FromXIDs(this);
            foreach (var x in AllFXActions)
                x.FromXIDs(this);
            foreach (var x in AllTransitions)
                x.FromXIDs(this);
            foreach (var x in AllStates)
                x.FromXIDs(this);
            foreach (var x in AllFXNodes)
                x.FromXIDs(this);
            foreach (var x in AllFXNodePointers)
                x.FromXIDs(this);

            RootFXNode.FromXIDs(this);
            //RootFXNode = DereferenceFXNode(RootFXNode);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.WriteASCII("FXR\0");
            bw.BigEndian = BigEndian;
            bw.WriteUInt32(Version == 2 ? 0x20000u : 0x10000u);
            currentVersion = Version;
            bw.VarintLong = Wide;
            var env = new FxrEnvironment();
            env.bw = bw;
            env.fxr = this;
            writingEnvironment = env;

            env.RegisterPointer(RootFXNode);

            bw.ReserveInt32("OffsetToTable");

            bw.ReserveInt32("TablePointerCount");
            bw.ReserveInt32("TableFXNodeCount");

            bw.WriteInt32(Unk1);
            // FXR v2 (Bloodborne): the offset of the first root's node 141 node list (FFX plan §19), filled in below
            if (Version == 2)
                bw.ReserveInt32("V2RootList");
            else
                bw.WriteInt32(Unk2);

            bw.Pad(16);

            // Write RootFXNode and everything else.
            env.FinishRecursiveWrite();

            if (Version == 2)
            {
                var first = RootFXNode as FXNode.EffectCreateNode133 ?? (RootFXNode as FXNode.EffectCreateNode134)?.Nodes.OfType<FXNode.EffectCreateNode133>().FirstOrDefault();
                var list = (first?.V2Node as FXNode.ContainerNode)?.Container?.WrittenPointers;
                bw.FillInt32("V2RootList", list != null && env.OffsetsByObject.TryGetValue(list, out long at) ? (int)at : 0);
            }

            bw.Pad(16);
            bw.FillInt32("OffsetToTable", (int)bw.Position);
            env.WritePointerTable("TablePointerCount");
            env.WriteFXNodeTable("TableFXNodeCount");
            env.ApplyGarbage();
            writingEnvironment = null;
            currentVersion = 1;
        }

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.GetASCII(0, 4) != "FXR\0")
                return false;

            var version = br.GetInt32(4);
            return version == 0x10000 || version == 0x100 || version == 0x20000;
        }

        static int ReadFXR1Varint(BinaryReaderEx br)
        {
            int result = br.ReadInt32();
            AssertFXR1Garbage(br);
            return result;
        }

        /// <summary>
        /// DS1R: the high half of each 64-bit varint, by file offset. Vanilla leaves 0xCDCDCDCD (uninitialised padding)
        /// after some fields and 0 after others; it's kept as read, so a rewrite puts each back where it was.
        /// </summary>
        internal Dictionary<long, uint> Garbage { get; } = new Dictionary<long, uint>();

        [ThreadStatic] private static Dictionary<long, uint> readingGarbage;
        [ThreadStatic] private static FxrEnvironment writingEnvironment;

        static void AssertFXR1Garbage(BinaryReaderEx br)
        {
            if (br.VarintLong)
            {
                long at = br.Position;
                // v1 (DS1R) leaves 0 or 0xCDCDCDCD; v2 (Bloodborne) is 0 but for a few words with real-looking values
                uint v = IsV2 ? br.ReadUInt32() : br.AssertUInt32([0, 0xCDCDCDCD]);
                if (v != 0 && readingGarbage != null)
                    readingGarbage[at] = v;
            }
        }

        static int GetFXR1Varint(BinaryReaderEx br, long offset)
        {
            int result = -1;
            br.StepIn(offset);
            result = br.ReadInt32();
            AssertFXR1Garbage(br);
            br.StepOut();
            return result;
        }

        static int AssertFXR1Varint(BinaryReaderEx br, params int[] v)
        {
            int result = br.AssertInt32(v);
            AssertFXR1Garbage(br);
            return result;
        }

        static float ReadFXR1Single(BinaryReaderEx br)
        {
            float result = br.ReadSingle();
            AssertFXR1Garbage(br);
            return result;
        }

        static void WriteFXR1Garbage(BinaryWriterEx bw)
        {
            // 0 for now; the value read at this slot's final offset is put back once the layout is known
            if (bw.VarintLong)
            {
                writingEnvironment?.RecordGarbage(bw.Position);
                bw.WriteUInt32(0);
            }
        }

        static void WriteFXR1Varint(BinaryWriterEx bw, int v)
        {
            bw.WriteInt32(v);
            WriteFXR1Garbage(bw);
        }

        static void ReserveFXR1Varint(BinaryWriterEx bw, string name)
        {
            bw.ReserveInt32(name);
            WriteFXR1Garbage(bw);
        }

        static void FillFXR1Varint(BinaryWriterEx bw, string name, int v)
        {
            bw.FillInt32(name, v);
            //bw.WriteFXR1Garbage();
        }

        static void WriteFXR1Single(BinaryWriterEx bw, float v)
        {
            bw.WriteSingle(v);
            WriteFXR1Garbage(bw);
        }
    }
}
