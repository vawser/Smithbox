using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSBVD
    {
        /// <summary>
        /// The different types of events.
        /// </summary>
        internal enum EventType
        {
            /// <summary>
            /// An event holding light parameters.
            /// </summary>
            Light = 100,

            /// <summary>
            /// An event holding scene parameters such as render distance.
            /// </summary>
            Scene = 101,

            /// <summary>
            /// An event holding wind parameters.
            /// </summary>
            Wind = 102,

            /// <summary>
            /// An event holding rain parameters.
            /// </summary>
            Rain = 103,

            /// <summary>
            /// An event referencing what background game music should be played.
            /// </summary>
            BGM = 400
        }

        #region Param

        /// <summary>
        /// Dynamic or interactive systems.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            /// <summary>
            /// Lights.
            /// </summary>
            public List<Event.Light> Lights { get; set; }

            /// <summary>
            /// Scene parameters that control things such as render distance and bloom.
            /// </summary>
            public List<Event.Scene> Scenes { get; set; }

            /// <summary>
            /// Wind parameters for how certain effects interact in wind.
            /// </summary>
            public List<Event.Wind> Winds { get; set; }

            /// <summary>
            /// Rain parameters.
            /// </summary>
            public List<Event.Rain> Rains { get; set; }

            /// <summary>
            /// Background music events.
            /// </summary>
            public List<Event.BGM> BGMs { get; set; }

            /// <summary>
            /// Creates an empty EventParam with the default version.
            /// </summary>
            public EventParam() : base(10001002, "EVENT_PARAM_ST")
            {
                Lights = new List<Event.Light>();
                Scenes = new List<Event.Scene>();
                Winds = new List<Event.Wind>();
                Rains = new List<Event.Rain>();
                BGMs = new List<Event.BGM>();
            }

            public Event Add(Event evnt)
            {
                switch (evnt)
                {
                    case Event.Light e: Lights.Add(e); break;
                    case Event.Scene e: Scenes.Add(e); break;
                    case Event.Wind e: Winds.Add(e); break;
                    case Event.Rain e: Rains.Add(e); break;
                    case Event.BGM e: BGMs.Add(e); break;
                    default: throw new ArgumentException($"Unrecognized type {evnt.GetType()}.", nameof(evnt));
                }

                return evnt;
            }
            IMsbEvent IMsbParam<IMsbEvent>.Add(IMsbEvent item) => Add((Event)item);

            /// <summary>
            /// Returns every Event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries() => SFUtil.ConcatAll<Event>(Lights, Scenes, Winds, Rains, BGMs);
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 8);
                switch (type)
                {
                    case EventType.Light: return Lights.EchoAdd(new Event.Light(br));
                    case EventType.Scene: return Scenes.EchoAdd(new Event.Scene(br));
                    case EventType.Wind: return Winds.EchoAdd(new Event.Wind(br));
                    case EventType.Rain: return Rains.EchoAdd(new Event.Rain(br));
                    case EventType.BGM: return BGMs.EchoAdd(new Event.BGM(br));
                    default: throw new NotImplementedException($"Unimplemented event type: {type}");
                }
            }
        }

        #endregion

        #region Entry

        public abstract class Event : ParamEntry, IMsbEvent
        {
            /// <summary>
            /// The type of the event.
            /// </summary>
            private protected abstract EventType Type { get; }

            /// <summary>
            /// Whether or not an event has type data.
            /// </summary>
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UniqueID { get; set; }

            private protected Event(string name)
            {
                Name = name;
                UniqueID = -1;
            }

            /// <summary>
            /// Creates a deep copy of the event.
            /// </summary>
            public Event DeepCopy()
            {
                var evnt = (Event)MemberwiseClone();
                DeepCopyTo(evnt);
                return evnt;
            }
            IMsbEvent IMsbEvent.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Event evnt) { }

            private protected Event(BinaryReaderEx br)
            {
                long start = br.Position;

                int nameOffset = br.ReadInt32();
                UniqueID = br.ReadInt32();
                br.AssertInt32((int)Type);
                br.ReadInt32(); // ID
                int typeDataOffset = br.ReadInt32();

                br.Position = start + nameOffset;
                Name = br.ReadShiftJIS();

                br.Position = start + typeDataOffset;
                ReadTypeData(br);
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt32("NameOffset");
                bw.WriteInt32(UniqueID);
                bw.WriteInt32((int)Type);
                bw.WriteInt32(id);
                bw.ReserveInt32("TypeDataOffset");

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(Name), true);
                bw.Pad(4);

                bw.FillInt32("TypeDataOffset", (int)(bw.Position - start));
                WriteTypeData(bw);
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTypeData)}.");

            #region EventType Structs

            public class Light : Event
            {
                private protected override EventType Type => EventType.Light;
                private protected override bool HasTypeData => true;

                public LightConfig Config { get; set; }

                /// <summary>
                /// Creates a Light with default values.
                /// </summary>
                public Light() : base("IDXX Light")
                {
                    Config = new LightConfig();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var light = (Light)evnt;
                    light.Config = Config.DeepCopy();
                }

                internal Light(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Config = new LightConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Config.Write(bw);

                #region Sub Structs

                public class LightConfig
                {
                    public short Unk00 { get; set; }
                    public short Unk02 { get; set; }
                    public UnkBloomConfig BloomConfig1 { get; set; }
                    public UnkBloomConfig BloomConfig2 { get; set; }
                    public short Unk4C { get; set; }
                    public short Unk4E { get; set; }
                    public float Unk50 { get; set; }
                    public float Unk54 { get; set; }
                    public float Unk58 { get; set; }
                    public float Unk5C { get; set; }
                    public float Unk60 { get; set; }
                    public float Unk64 { get; set; }
                    public float Unk68 { get; set; }
                    public float Unk6C { get; set; }
                    public float Unk70 { get; set; }
                    public float Unk74 { get; set; }
                    public float Unk78 { get; set; }
                    public float Unk7C { get; set; }
                    public float Unk80 { get; set; }
                    public float Unk84 { get; set; }
                    public float Unk88 { get; set; }
                    public float Unk8C { get; set; }
                    public float Unk90 { get; set; }
                    public float Unk94 { get; set; }
                    public float Unk98 { get; set; }
                    public float Unk9C { get; set; }
                    public float Unk100 { get; set; }
                    public float Unk104 { get; set; }
                    public float Unk108 { get; set; }
                    public float Unk10C { get; set; }
                    public float Unk110 { get; set; }
                    public float Unk114 { get; set; }
                    public float Unk118 { get; set; }
                    public float Unk11C { get; set; }
                    public float Unk120 { get; set; }
                    public float Unk124 { get; set; }
                    public byte Unk128 { get; set; }
                    public byte Unk129 { get; set; }
                    public byte Unk12A { get; set; }
                    public byte Unk12B { get; set; }
                    public float Unk12C { get; set; }
                    public float Unk130 { get; set; }
                    public float Unk134 { get; set; }
                    public float Unk138 { get; set; }
                    public float Unk13C { get; set; }
                    public float Unk140 { get; set; }
                    public float Unk144 { get; set; }
                    public float Unk148 { get; set; }
                    public float Unk14C { get; set; }
                    public float Unk150 { get; set; }
                    public float Unk154 { get; set; }
                    public float Unk158 { get; set; }
                    public float Unk15C { get; set; }

                    public LightConfig()
                    {
                        Unk00 = 0;
                        Unk02 = 0;
                        BloomConfig1 = new UnkBloomConfig();
                        BloomConfig2 = new UnkBloomConfig();
                        Unk4C = 0;
                        Unk4E = 40;
                        Unk50 = 1f;
                        Unk54 = 1f;
                        Unk58 = 1f;
                        Unk5C = 1f;
                        Unk60 = 1f;
                        Unk64 = 1f;
                        Unk68 = 1f;
                        Unk6C = 1f;
                        Unk70 = 1f;
                        Unk74 = 1f;
                        Unk78 = 1f;
                        Unk7C = 1f;
                        Unk80 = 1f;
                        Unk84 = 1f;
                        Unk88 = 1f;
                        Unk8C = 1f;
                        Unk90 = 1f;
                        Unk94 = 1f;
                        Unk98 = 1f;
                        Unk9C = 0;
                        Unk100 = 0.9f;
                        Unk104 = 0.9f;
                        Unk108 = 0.8f;
                        Unk10C = 1f;
                        Unk110 = 1400f;
                        Unk114 = 20f;
                        Unk118 = 50f;
                        Unk11C = 10f;
                        Unk120 = 0.1f;
                        Unk124 = 0.5f;
                        Unk128 = 0;
                        Unk129 = 0;
                        Unk12A = 0;
                        Unk12B = 0;
                        Unk12C = 0;
                        Unk130 = 0;
                        Unk134 = 0;
                        Unk138 = 0;
                        Unk13C = 0;
                        Unk140 = 0;
                        Unk144 = 0;
                        Unk148 = 0;
                        Unk14C = 0;
                        Unk150 = 0;
                        Unk154 = 0;
                        Unk158 = 0;
                        Unk15C = 0;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public LightConfig DeepCopy()
                    {
                        var lightConfig = (LightConfig)MemberwiseClone();
                        lightConfig.BloomConfig1 = BloomConfig1.DeepCopy();
                        lightConfig.BloomConfig2 = BloomConfig2.DeepCopy();
                        return lightConfig;
                    }

                    internal LightConfig(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt16();
                        Unk02 = br.ReadInt16();
                        BloomConfig1 = new UnkBloomConfig(br);
                        BloomConfig2 = new UnkBloomConfig(br);
                        Unk4C = br.ReadInt16();
                        Unk4E = br.ReadInt16();
                        Unk50 = br.ReadSingle();
                        Unk54 = br.ReadSingle();
                        Unk58 = br.ReadSingle();
                        Unk5C = br.ReadSingle();
                        Unk60 = br.ReadSingle();
                        Unk64 = br.ReadSingle();
                        Unk68 = br.ReadSingle();
                        Unk6C = br.ReadSingle();
                        Unk70 = br.ReadSingle();
                        Unk74 = br.ReadSingle();
                        Unk78 = br.ReadSingle();
                        Unk7C = br.ReadSingle();
                        Unk80 = br.ReadSingle();
                        Unk84 = br.ReadSingle();
                        Unk88 = br.ReadSingle();
                        Unk8C = br.ReadSingle();
                        Unk90 = br.ReadSingle();
                        Unk94 = br.ReadSingle();
                        Unk98 = br.ReadSingle();
                        Unk9C = br.ReadSingle();
                        Unk100 = br.ReadSingle();
                        Unk104 = br.ReadSingle();
                        Unk108 = br.ReadSingle();
                        Unk10C = br.ReadSingle();
                        Unk110 = br.ReadSingle();
                        Unk114 = br.ReadSingle();
                        Unk118 = br.ReadSingle();
                        Unk11C = br.ReadSingle();
                        Unk120 = br.ReadSingle();
                        Unk124 = br.ReadSingle();
                        Unk128 = br.ReadByte();
                        Unk129 = br.ReadByte();
                        Unk12A = br.ReadByte();
                        Unk12B = br.ReadByte();
                        Unk12C = br.ReadSingle();
                        Unk130 = br.ReadSingle();
                        Unk134 = br.ReadSingle();
                        Unk138 = br.ReadSingle();
                        Unk13C = br.ReadSingle();
                        Unk140 = br.ReadSingle();
                        Unk144 = br.ReadSingle();
                        Unk148 = br.ReadSingle();
                        Unk14C = br.ReadSingle();
                        Unk150 = br.ReadSingle();
                        Unk154 = br.ReadSingle();
                        Unk158 = br.ReadSingle();
                        Unk15C = br.ReadSingle();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(Unk00);
                        bw.WriteInt16(Unk02);
                        BloomConfig1.Write(bw);
                        BloomConfig2.Write(bw);
                        bw.WriteInt16(Unk4C);
                        bw.WriteInt16(Unk4E);
                        bw.WriteSingle(Unk50);
                        bw.WriteSingle(Unk54);
                        bw.WriteSingle(Unk58);
                        bw.WriteSingle(Unk5C);
                        bw.WriteSingle(Unk60);
                        bw.WriteSingle(Unk64);
                        bw.WriteSingle(Unk68);
                        bw.WriteSingle(Unk6C);
                        bw.WriteSingle(Unk70);
                        bw.WriteSingle(Unk74);
                        bw.WriteSingle(Unk78);
                        bw.WriteSingle(Unk7C);
                        bw.WriteSingle(Unk80);
                        bw.WriteSingle(Unk84);
                        bw.WriteSingle(Unk88);
                        bw.WriteSingle(Unk8C);
                        bw.WriteSingle(Unk90);
                        bw.WriteSingle(Unk94);
                        bw.WriteSingle(Unk98);
                        bw.WriteSingle(Unk9C);
                        bw.WriteSingle(Unk100);
                        bw.WriteSingle(Unk104);
                        bw.WriteSingle(Unk108);
                        bw.WriteSingle(Unk10C);
                        bw.WriteSingle(Unk110);
                        bw.WriteSingle(Unk114);
                        bw.WriteSingle(Unk118);
                        bw.WriteSingle(Unk11C);
                        bw.WriteSingle(Unk120);
                        bw.WriteSingle(Unk124);
                        bw.WriteByte(Unk128);
                        bw.WriteByte(Unk129);
                        bw.WriteByte(Unk12A);
                        bw.WriteByte(Unk12B);
                        bw.WriteSingle(Unk12C);
                        bw.WriteSingle(Unk130);
                        bw.WriteSingle(Unk134);
                        bw.WriteSingle(Unk138);
                        bw.WriteSingle(Unk13C);
                        bw.WriteSingle(Unk140);
                        bw.WriteSingle(Unk144);
                        bw.WriteSingle(Unk148);
                        bw.WriteSingle(Unk14C);
                        bw.WriteSingle(Unk150);
                        bw.WriteSingle(Unk154);
                        bw.WriteSingle(Unk158);
                        bw.WriteSingle(Unk15C);
                    }

                    #region Sub Struct

                    public class UnkBloomConfig
                    {
                        public short Unk00 { get; set; }
                        public short Unk02 { get; set; }
                        public Bloom Bloom1 { get; set; }
                        public Bloom Bloom2 { get; set; }

                        public UnkBloomConfig()
                        {
                            Unk00 = 0;
                            Unk02 = 0;
                            Bloom1 = new Bloom();
                            Bloom2 = new Bloom();
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkBloomConfig DeepCopy()
                        {
                            var unkBloomConfig = (UnkBloomConfig)MemberwiseClone();
                            unkBloomConfig.Bloom1 = Bloom1.DeepCopy();
                            unkBloomConfig.Bloom2 = Bloom2.DeepCopy();
                            return unkBloomConfig;
                        }

                        internal UnkBloomConfig(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt16();
                            Unk02 = br.ReadInt16();
                            Bloom1 = new Bloom(br);
                            Bloom2 = new Bloom(br);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt16(Unk00);
                            bw.WriteInt16(Unk02);
                            Bloom1.Write(bw);
                            Bloom2.Write(bw);
                        }

                        #region Sub Struct

                        public class Bloom
                        {
                            public float Red { get; set; }
                            public float Blue { get; set; }
                            public float Green { get; set; }
                            public float Unk { get; set; }

                            public Bloom()
                            {
                                Red = 1f;
                                Blue = 1f;
                                Green = 1f;
                                Unk = 1f;
                            }

                            public Bloom(float red, float blue, float green)
                            {
                                Red = red;
                                Blue = blue;
                                Green = green;
                                Unk = 1f;
                            }

                            /// <summary>
                            /// Creates a deep copy of the struct.
                            /// </summary>
                            public Bloom DeepCopy()
                            {
                                return (Bloom)MemberwiseClone();
                            }

                            internal Bloom(BinaryReaderEx br)
                            {
                                Red = br.ReadSingle();
                                Blue = br.ReadSingle();
                                Green = br.ReadSingle();
                                Unk = br.ReadSingle();
                            }

                            internal void Write(BinaryWriterEx bw)
                            {
                                bw.WriteSingle(Red);
                                bw.WriteSingle(Blue);
                                bw.WriteSingle(Green);
                                bw.WriteSingle(Unk);
                            }
                        }

                        #endregion
                    }

                    #endregion
                }

                #endregion
            }

            public class Scene : Event
            {
                private protected override EventType Type => EventType.Scene;
                private protected override bool HasTypeData => true;

                public SceneConfig Config { get; private set; }

                /// <summary>
                /// Creates scene parameters with default values.
                /// </summary>
                public Scene() : base("scene parameters")
                {
                    Config = new SceneConfig();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var scene = (Scene)evnt;
                    scene.Config = Config.DeepCopy();
                }

                internal Scene(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Config = new SceneConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Config.Write(bw);

                #region Sub Structs

                public class SceneConfig
                {
                    public UnkSceneConfig1 Config1 { get; set; }
                    public UnkSceneConfig2 Config2 { get; set; }
                    public UnkSceneConfig3 Config3 { get; set; }
                    public UnkSceneConfig4 Config4 { get; set; }
                    public UnkSceneConfig5 Config5 { get; set; }
                    public UnkSceneConfig6 Config6 { get; set; }
                    public UnkSceneConfig7 Config7 { get; set; }
                    public UnkSceneConfig8 Config8 { get; set; }
                    public UnkSceneConfig9 Config9 { get; set; }
                    public UnkSceneConfig10 Config10 { get; set; }

                    public SceneConfig()
                    {
                        Config1 = new UnkSceneConfig1();
                        Config2 = new UnkSceneConfig2();
                        Config3 = new UnkSceneConfig3();
                        Config4 = new UnkSceneConfig4();
                        Config5 = new UnkSceneConfig5();
                        Config6 = new UnkSceneConfig6();
                        Config7 = new UnkSceneConfig7();
                        Config8 = new UnkSceneConfig8();
                        Config9 = new UnkSceneConfig9();
                        Config10 = new UnkSceneConfig10();
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public SceneConfig DeepCopy()
                    {
                        var sceneConfig = (SceneConfig)MemberwiseClone();
                        sceneConfig.Config1 = Config1.DeepCopy();
                        sceneConfig.Config2 = Config2.DeepCopy();
                        sceneConfig.Config3 = Config3.DeepCopy();
                        sceneConfig.Config4 = Config4.DeepCopy();
                        sceneConfig.Config5 = Config5.DeepCopy();
                        sceneConfig.Config6 = Config6.DeepCopy();
                        sceneConfig.Config7 = Config7.DeepCopy();
                        sceneConfig.Config8 = Config8.DeepCopy();
                        sceneConfig.Config9 = Config9.DeepCopy();
                        sceneConfig.Config10 = Config10.DeepCopy();
                        return sceneConfig;
                    }

                    internal SceneConfig(BinaryReaderEx br)
                    {
                        long start = br.Position;
                        int offsetUnkSceneConfig1 = br.ReadInt32();
                        int offsetUnkSceneConfig2 = br.ReadInt32();
                        int offsetUnkSceneConfig3 = br.ReadInt32();
                        int offsetUnkSceneConfig4 = br.ReadInt32();
                        int offsetUnkSceneConfig5 = br.ReadInt32();
                        int offsetUnkSceneConfig6 = br.ReadInt32();
                        int offsetUnkSceneConfig7 = br.ReadInt32();
                        int offsetUnkSceneConfig8 = br.ReadInt32();
                        int offsetUnkSceneConfig9 = br.ReadInt32();
                        int offsetUnkSceneConfig10 = br.ReadInt32();
                        br.AssertPattern(24, 0);

                        br.Position = start + offsetUnkSceneConfig1;
                        Config1 = new UnkSceneConfig1(br);

                        br.Position = start + offsetUnkSceneConfig2;
                        Config2 = new UnkSceneConfig2(br);

                        br.Position = start + offsetUnkSceneConfig3;
                        Config3 = new UnkSceneConfig3(br);

                        br.Position = start + offsetUnkSceneConfig4;
                        Config4 = new UnkSceneConfig4(br);

                        br.Position = start + offsetUnkSceneConfig5;
                        Config5 = new UnkSceneConfig5(br);

                        br.Position = start + offsetUnkSceneConfig6;
                        Config6 = new UnkSceneConfig6(br);

                        br.Position = start + offsetUnkSceneConfig7;
                        Config7 = new UnkSceneConfig7(br);

                        br.Position = start + offsetUnkSceneConfig8;
                        Config8 = new UnkSceneConfig8(br);

                        br.Position = start + offsetUnkSceneConfig9;
                        Config9 = new UnkSceneConfig9(br);

                        if (offsetUnkSceneConfig10 > 0)
                        {
                            br.Position = start + offsetUnkSceneConfig10;
                            Config10 = new UnkSceneConfig10(br);
                        }

                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        long start = bw.Position;
                        bw.ReserveInt32("OffsetUnkSceneConfig1");
                        bw.ReserveInt32("OffsetUnkSceneConfig2");
                        bw.ReserveInt32("OffsetUnkSceneConfig3");
                        bw.ReserveInt32("OffsetUnkSceneConfig4");
                        bw.ReserveInt32("OffsetUnkSceneConfig5");
                        bw.ReserveInt32("OffsetUnkSceneConfig6");
                        bw.ReserveInt32("OffsetUnkSceneConfig7");
                        bw.ReserveInt32("OffsetUnkSceneConfig8");
                        bw.ReserveInt32("OffsetUnkSceneConfig9");
                        bw.ReserveInt32("OffsetUnkSceneConfig10");
                        bw.WritePattern(24, 0);

                        bw.FillInt32("OffsetUnkSceneConfig1", (int)(bw.Position - start));
                        Config1.Write(bw);

                        bw.FillInt32("OffsetUnkSceneConfig2", (int)(bw.Position - start));
                        Config2.Write(bw);

                        bw.FillInt32("OffsetUnkSceneConfig3", (int)(bw.Position - start));
                        Config3.Write(bw);

                        bw.FillInt32("OffsetUnkSceneConfig4", (int)(bw.Position - start));
                        Config4.Write(bw);

                        bw.FillInt32("OffsetUnkSceneConfig5", (int)(bw.Position - start));
                        Config5.Write(bw);

                        bw.FillInt32("OffsetUnkSceneConfig6", (int)(bw.Position - start));
                        Config6.Write(bw);

                        bw.FillInt32("OffsetUnkSceneConfig7", (int)(bw.Position - start));
                        Config7.Write(bw);

                        bw.FillInt32("OffsetUnkSceneConfig8", (int)(bw.Position - start));
                        Config8.Write(bw);

                        bw.FillInt32("OffsetUnkSceneConfig9", (int)(bw.Position - start));
                        Config9.Write(bw);

                        if (Config10 != null)
                        {
                            bw.FillInt32("OffsetUnkSceneConfig10", (int)(bw.Position - start));
                            Config10.Write(bw);
                        }
                        else
                        {
                            bw.FillInt32("OffsetUnkSceneConfig10", 0);
                        }
                    }

                    #region Sub Structs

                    public class UnkSceneConfig1
                    {
                        public short Unk00 { get; set; }
                        public short Unk02 { get; set; }
                        public float GlobalDrawDistance { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public short Unk14 { get; set; }
                        public short Unk16 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk1C { get; set; }
                        public float Unk20 { get; set; }
                        public byte Unk24 { get; set; }
                        public byte Unk25 { get; set; }
                        public byte Unk26 { get; set; }
                        public byte Unk27 { get; set; }
                        public float Unk28 { get; set; }
                        public float Unk2C { get; set; }
                        public short Unk30 { get; set; }
                        public short Unk32 { get; set; }
                        public float Unk34 { get; set; }
                        public float Unk38 { get; set; }
                        public short Unk3C { get; set; }
                        public short Unk3E { get; set; }
                        public short Unk40 { get; set; }
                        public short Unk42 { get; set; }
                        public short Unk44 { get; set; }
                        public short Unk46 { get; set; }
                        public short Unk48 { get; set; }
                        public short Unk4A { get; set; }
                        public short Unk4C { get; set; }
                        public short Unk4E { get; set; }
                        public short Unk50 { get; set; }
                        public short Unk52 { get; set; }
                        public short Unk54 { get; set; }
                        public short Unk56 { get; set; }
                        public float Unk58 { get; set; }
                        public float Unk5C { get; set; }

                        public UnkSceneConfig1()
                        {
                            Unk00 = 9522;
                            Unk02 = 12544;
                            GlobalDrawDistance = 15000f;
                            Unk08 = -10f;
                            Unk0C = 0;
                            Unk10 = 0;
                            Unk14 = 0;
                            Unk16 = 0;
                            Unk18 = 100f;
                            Unk1C = 100f;
                            Unk20 = 99999f;
                            Unk24 = 1;
                            Unk25 = 169;
                            Unk26 = 181;
                            Unk27 = 188;
                            Unk28 = -200f;
                            Unk2C = 0;
                            Unk30 = 1;
                            Unk32 = 10;
                            Unk34 = 0;
                            Unk38 = 0;
                            Unk3C = 50;
                            Unk3E = 1000;
                            Unk40 = -1;
                            Unk42 = 50;
                            Unk44 = 0;
                            Unk46 = 0;
                            Unk48 = 0;
                            Unk4A = 0;
                            Unk4C = 50;
                            Unk4E = 0;
                            Unk50 = 0;
                            Unk52 = 0;
                            Unk54 = 0;
                            Unk56 = 0;
                            Unk58 = 6f;
                            Unk5C = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig1 DeepCopy()
                        {
                            return (UnkSceneConfig1)MemberwiseClone();
                        }

                        internal UnkSceneConfig1(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt16();
                            Unk02 = br.ReadInt16();
                            GlobalDrawDistance = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadInt16();
                            Unk16 = br.ReadInt16();
                            Unk18 = br.ReadSingle();
                            Unk1C = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadByte();
                            Unk25 = br.ReadByte();
                            Unk26 = br.ReadByte();
                            Unk27 = br.ReadByte();
                            Unk28 = br.ReadSingle();
                            Unk2C = br.ReadSingle();
                            Unk30 = br.ReadInt16();
                            Unk32 = br.ReadInt16();
                            Unk34 = br.ReadSingle();
                            Unk38 = br.ReadSingle();
                            Unk3C = br.ReadInt16();
                            Unk3E = br.ReadInt16();
                            Unk40 = br.ReadInt16();
                            Unk42 = br.ReadInt16();
                            Unk44 = br.ReadInt16();
                            Unk46 = br.ReadInt16();
                            Unk48 = br.ReadInt16();
                            Unk4A = br.ReadInt16();
                            Unk4C = br.ReadInt16();
                            Unk4E = br.ReadInt16();
                            Unk50 = br.ReadInt16();
                            Unk52 = br.ReadInt16();
                            Unk54 = br.ReadInt16();
                            Unk56 = br.ReadInt16();
                            Unk58 = br.ReadSingle();
                            Unk5C = br.ReadSingle();
                            br.AssertPattern(416, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt16(Unk00);
                            bw.WriteInt16(Unk02);
                            bw.WriteSingle(GlobalDrawDistance);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteInt16(Unk14);
                            bw.WriteInt16(Unk16);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk1C);
                            bw.WriteSingle(Unk20);
                            bw.WriteByte(Unk24);
                            bw.WriteByte(Unk25);
                            bw.WriteByte(Unk26);
                            bw.WriteByte(Unk27);
                            bw.WriteSingle(Unk28);
                            bw.WriteSingle(Unk2C);
                            bw.WriteInt16(Unk30);
                            bw.WriteInt16(Unk32);
                            bw.WriteSingle(Unk34);
                            bw.WriteSingle(Unk38);
                            bw.WriteInt16(Unk3C);
                            bw.WriteInt16(Unk3E);
                            bw.WriteInt16(Unk40);
                            bw.WriteInt16(Unk42);
                            bw.WriteInt16(Unk44);
                            bw.WriteInt16(Unk46);
                            bw.WriteInt16(Unk48);
                            bw.WriteInt16(Unk4A);
                            bw.WriteInt16(Unk4C);
                            bw.WriteInt16(Unk4E);
                            bw.WriteInt16(Unk50);
                            bw.WriteInt16(Unk52);
                            bw.WriteInt16(Unk54);
                            bw.WriteInt16(Unk56);
                            bw.WriteSingle(Unk58);
                            bw.WriteSingle(Unk5C);
                            bw.WritePattern(416, 0);
                        }
                    }

                    public class UnkSceneConfig2
                    {
                        public byte Unk00 { get; set; }
                        public byte Unk01 { get; set; }
                        public byte Unk02 { get; set; }
                        public byte Unk03 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk1C { get; set; }
                        public float Unk20 { get; set; }
                        public float Unk24 { get; set; }
                        public float Unk28 { get; set; }
                        public short Unk2C { get; set; }
                        public short Unk2E { get; set; }

                        public UnkSceneConfig2()
                        {
                            Unk00 = 0;
                            Unk01 = 1;
                            Unk02 = 0;
                            Unk03 = 1;
                            Unk04 = 0;
                            Unk08 = 15f;
                            Unk0C = 0;
                            Unk10 = 1000f;
                            Unk14 = 100f;
                            Unk18 = 1f;
                            Unk1C = 60f;
                            Unk20 = 5.6f;
                            Unk24 = 1f;
                            Unk28 = 0.024f;
                            Unk2C = 1;
                            Unk2E = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig2 DeepCopy()
                        {
                            return (UnkSceneConfig2)MemberwiseClone();
                        }

                        internal UnkSceneConfig2(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadByte();
                            Unk01 = br.ReadByte();
                            Unk02 = br.ReadByte();
                            Unk03 = br.ReadByte();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            Unk1C = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadSingle();
                            Unk28 = br.ReadSingle();
                            Unk2C = br.ReadInt16();
                            Unk2E = br.ReadInt16();
                            br.AssertPattern(80, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteByte(Unk00);
                            bw.WriteByte(Unk01);
                            bw.WriteByte(Unk02);
                            bw.WriteByte(Unk03);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk14);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk1C);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Unk24);
                            bw.WriteSingle(Unk28);
                            bw.WriteInt16(Unk2C);
                            bw.WriteInt16(Unk2E);
                            bw.WritePattern(80, 0);
                        }
                    }

                    public class UnkSceneConfig3
                    {
                        public int Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk14 { get; set; }
                        public short Unk18 { get; set; }
                        public short Unk1A { get; set; }
                        public float Unk1C { get; set; }
                        public float Unk20 { get; set; }
                        public float Unk24 { get; set; }
                        public float Unk28 { get; set; }
                        public float Unk2C { get; set; }
                        public float Unk30 { get; set; }
                        public float Unk34 { get; set; }
                        public float Unk38 { get; set; }
                        public float Unk3C { get; set; }
                        public float Unk40 { get; set; }
                        public float Unk44 { get; set; }
                        public int Unk48 { get; set; }
                        public float Unk4C { get; set; }
                        public float Unk50 { get; set; }
                        public float Unk54 { get; set; }
                        public float Unk58 { get; set; }
                        public float Unk5C { get; set; }
                        public float Unk60 { get; set; }

                        public UnkSceneConfig3()
                        {
                            Unk00 = 1;
                            Unk04 = 0.5f;
                            Unk08 = 1.1f;
                            Unk0C = 0.6f;
                            Unk10 = 10f;
                            Unk14 = 1f;
                            Unk18 = 3;
                            Unk1A = 256;
                            Unk1C = 0.9f;
                            Unk20 = 0.8f;
                            Unk24 = 1f;
                            Unk28 = 1f;
                            Unk2C = 1f;
                            Unk30 = 1f;
                            Unk34 = 1f;
                            Unk38 = 1f;
                            Unk3C = 0;
                            Unk40 = 0;
                            Unk44 = 0;
                            Unk48 = 0;
                            Unk4C = 0;
                            Unk50 = 0;
                            Unk54 = 0;
                            Unk58 = 0;
                            Unk5C = 0;
                            Unk60 = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig3 DeepCopy()
                        {
                            return (UnkSceneConfig3)MemberwiseClone();
                        }

                        internal UnkSceneConfig3(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadInt16();
                            Unk1A = br.ReadInt16();
                            Unk1C = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadSingle();
                            Unk28 = br.ReadSingle();
                            Unk2C = br.ReadSingle();
                            Unk30 = br.ReadSingle();
                            Unk34 = br.ReadSingle();
                            Unk38 = br.ReadSingle();
                            Unk3C = br.ReadSingle();
                            Unk40 = br.ReadSingle();
                            Unk44 = br.ReadSingle();
                            Unk48 = br.ReadInt32();
                            Unk4C = br.ReadSingle();
                            Unk50 = br.ReadSingle();
                            Unk54 = br.ReadSingle();
                            Unk58 = br.ReadSingle();
                            Unk5C = br.ReadSingle();
                            Unk60 = br.ReadSingle();
                            br.AssertPattern(28, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk14);
                            bw.WriteInt16(Unk18);
                            bw.WriteInt16(Unk1A);
                            bw.WriteSingle(Unk1C);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Unk24);
                            bw.WriteSingle(Unk28);
                            bw.WriteSingle(Unk2C);
                            bw.WriteSingle(Unk30);
                            bw.WriteSingle(Unk34);
                            bw.WriteSingle(Unk38);
                            bw.WriteSingle(Unk3C);
                            bw.WriteSingle(Unk40);
                            bw.WriteSingle(Unk44);
                            bw.WriteInt32(Unk48);
                            bw.WriteSingle(Unk4C);
                            bw.WriteSingle(Unk50);
                            bw.WriteSingle(Unk54);
                            bw.WriteSingle(Unk58);
                            bw.WriteSingle(Unk5C);
                            bw.WriteSingle(Unk60);
                            bw.WritePattern(28, 0);
                        }
                    }

                    public class UnkSceneConfig4
                    {
                        public int Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk1C { get; set; }
                        public float Unk20 { get; set; }
                        public float Unk24 { get; set; }
                        public float Unk28 { get; set; }
                        public float Unk2C { get; set; }
                        public float Unk30 { get; set; }
                        public List<UnkStruct1> Struct1s { get; private set; }
                        public UnkStruct2 Struct2 { get; set; }
                        public UnkStruct3 Struct3 { get; set; }

                        public UnkSceneConfig4()
                        {
                            Unk00 = 1;
                            Unk04 = 0;
                            Unk08 = 0;
                            Unk0C = 255f;
                            Unk10 = 255f;
                            Unk14 = 255f;
                            Unk18 = 255f;
                            Unk1C = 255f;
                            Unk20 = 255f;
                            Unk24 = 255f;
                            Unk28 = 255f;
                            Unk2C = 0;
                            Unk30 = 0;
                            Struct1s = new List<UnkStruct1>();
                            Struct2 = new UnkStruct2();
                            Struct3 = new UnkStruct3();
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig4 DeepCopy()
                        {
                            var unkSceneConfig4 = (UnkSceneConfig4)MemberwiseClone();

                            for (int i = 0; i < 22; i++)
                            {
                                unkSceneConfig4.Struct1s[i] = Struct1s[i].DeepCopy();
                            }

                            unkSceneConfig4.Struct2 = Struct2.DeepCopy();
                            unkSceneConfig4.Struct3 = Struct3.DeepCopy();
                            return unkSceneConfig4;
                        }

                        internal UnkSceneConfig4(BinaryReaderEx br)
                        {
                            long start = br.Position;

                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            Unk1C = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadSingle();
                            Unk28 = br.ReadSingle();
                            Unk2C = br.ReadSingle();
                            Unk30 = br.ReadSingle();

                            int[] offsetsUnkStruct1 = br.ReadInt32s(22);
                            int offsetUnkStruct2 = br.ReadInt32();
                            int offsetUnkStruct3 = br.ReadInt32();
                            br.AssertPattern(108, 0);

                            Struct1s = new List<UnkStruct1>();
                            if (offsetsUnkStruct1[0] > 0)
                            {
                                for (int i = 0; i < 22; i++)
                                {
                                    if (offsetsUnkStruct1[i] > 0)
                                    {
                                        br.Position = start + offsetsUnkStruct1[i];
                                        Struct1s.Add(new UnkStruct1(br));
                                    }
                                }
                            }

                            if (offsetUnkStruct2 > 0)
                            {
                                br.Position = start + offsetUnkStruct2;
                                Struct2 = new UnkStruct2(br);
                            }

                            if (offsetUnkStruct3 > 0)
                            {
                                br.Position = start + offsetUnkStruct3;
                                Struct3 = new UnkStruct3(br);
                            }
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            long start = bw.Position;

                            bw.WriteInt32(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk14);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk1C);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Unk24);
                            bw.WriteSingle(Unk28);
                            bw.WriteSingle(Unk2C);
                            bw.WriteSingle(Unk30);
                            for (int i = 0; i < 22; i++)
                            {
                                bw.ReserveInt32($"OffsetUnkStruct1_{i}");
                            }
                            bw.ReserveInt32("OffsetUnkStruct2");
                            bw.ReserveInt32("OffsetUnkStruct3");
                            bw.WritePattern(108, 0);

                            int struct1Count = Math.Min(22, Struct1s.Count);
                            for (int i = 0; i < 22; i++)
                            {
                                if (i < struct1Count)
                                {
                                    bw.FillInt32($"OffsetUnkStruct1_{i}", (int)(bw.Position - start));
                                    Struct1s[i].Write(bw);
                                }
                                else
                                {
                                    bw.FillInt32($"OffsetUnkStruct1_{i}", 0);
                                }
                            }

                            if (Struct2 != null)
                            {
                                bw.FillInt32("OffsetUnkStruct2", (int)(bw.Position - start));
                                Struct2.Write(bw);
                            }
                            else
                            {
                                bw.FillInt32($"OffsetUnkStruct2", 0);
                            }

                            if (Struct3 != null)
                            {
                                bw.FillInt32("OffsetUnkStruct3", (int)(bw.Position - start));
                                Struct3.Write(bw);
                            }
                            else
                            {
                                bw.FillInt32($"OffsetUnkStruct3", 0);
                            }
                        }

                        #region Sub Structs

                        public class UnkStruct1
                        {
                            public int Unk00 { get; set; }
                            public byte[] Unk04 { get; private set; }
                            public float Unk08 { get; set; }
                            public byte[] Unk0C { get; private set; }
                            public byte[] Unk10 { get; private set; }

                            public UnkStruct1()
                            {
                                Unk00 = -1;
                                Unk04 = new byte[4] { 128, 0, 0, 0 };
                                Unk08 = 1f;
                                Unk0C = new byte[4] { 63, 128, 0, 0 };
                                Unk10 = new byte[4] { 0, 0, 0, 0 };
                            }

                            /// <summary>
                            /// Creates a deep copy of the struct.
                            /// </summary>
                            public UnkStruct1 DeepCopy()
                            {
                                return (UnkStruct1)MemberwiseClone();
                            }

                            internal UnkStruct1(BinaryReaderEx br)
                            {
                                Unk00 = br.ReadInt32();
                                Unk04 = br.ReadBytes(4);
                                Unk08 = br.ReadSingle();
                                Unk0C = br.ReadBytes(4);
                                Unk10 = br.ReadBytes(4);
                                br.AssertPattern(12, 0);
                            }

                            internal void Write(BinaryWriterEx bw)
                            {
                                bw.WriteInt32(Unk00);
                                bw.WriteBytes(Unk04);
                                bw.WriteSingle(Unk08);
                                bw.WriteBytes(Unk0C);
                                bw.WriteBytes(Unk10);
                                bw.WritePattern(12, 0);
                            }
                        }

                        public class UnkStruct2
                        {
                            public int Unk00 { get; set; }
                            public float Unk04 { get; set; }
                            public int Unk08 { get; set; }
                            public short Unk0C { get; set; }
                            public short Unk0E { get; set; }
                            public float Unk10 { get; set; }
                            public float Unk14 { get; set; }
                            public float Unk18 { get; set; }
                            public float Unk1C { get; set; }
                            public float Unk20 { get; set; }
                            public short Unk24 { get; set; }
                            public short Unk26 { get; set; }
                            public short Unk28 { get; set; }
                            public short Unk2A { get; set; }

                            public UnkStruct2()
                            {
                                Unk00 = -1;
                                Unk04 = 1f;
                                Unk08 = 0;
                                Unk0C = -1;
                                Unk0E = -1;
                                Unk10 = 0.3f;
                                Unk14 = 0.98f;
                                Unk18 = 0.98f;
                                Unk1C = 1f;
                                Unk20 = 1f;
                                Unk24 = -1;
                                Unk26 = -1;
                                Unk28 = -1;
                                Unk2A = -1;
                            }

                            /// <summary>
                            /// Creates a deep copy of the struct.
                            /// </summary>
                            public UnkStruct2 DeepCopy()
                            {
                                return (UnkStruct2)MemberwiseClone();
                            }

                            internal UnkStruct2(BinaryReaderEx br)
                            {
                                Unk00 = br.ReadInt32();
                                Unk04 = br.ReadSingle();
                                Unk08 = br.ReadInt32();
                                Unk0C = br.ReadInt16();
                                Unk0E = br.ReadInt16();
                                Unk10 = br.ReadSingle();
                                Unk14 = br.ReadSingle();
                                Unk18 = br.ReadSingle();
                                Unk1C = br.ReadSingle();
                                Unk20 = br.ReadSingle();
                                Unk24 = br.ReadInt16();
                                Unk26 = br.ReadInt16();
                                Unk28 = br.ReadInt16();
                                Unk2A = br.ReadInt16();
                                br.AssertPattern(20, 0);
                            }

                            internal void Write(BinaryWriterEx bw)
                            {
                                bw.WriteInt32(Unk00);
                                bw.WriteSingle(Unk04);
                                bw.WriteInt32(Unk08);
                                bw.WriteInt16(Unk0C);
                                bw.WriteInt16(Unk0E);
                                bw.WriteSingle(Unk10);
                                bw.WriteSingle(Unk14);
                                bw.WriteSingle(Unk18);
                                bw.WriteSingle(Unk1C);
                                bw.WriteSingle(Unk20);
                                bw.WriteInt16(Unk24);
                                bw.WriteInt16(Unk26);
                                bw.WriteInt16(Unk28);
                                bw.WriteInt16(Unk2A);
                                bw.WritePattern(20, 0);
                            }
                        }

                        public class UnkStruct3
                        {
                            public float Unk00 { get; set; }
                            public float Unk04 { get; set; }
                            public float Unk08 { get; set; }
                            public float Unk0C { get; set; }
                            public sbyte Unk10 { get; set; }
                            public sbyte Unk11 { get; set; }
                            public sbyte Unk12 { get; set; }
                            public sbyte Unk13 { get; set; }

                            public UnkStruct3()
                            {
                                Unk00 = -80f;
                                Unk04 = 98f;
                                Unk08 = 752f;
                                Unk0C = 672f;
                                Unk10 = -1;
                                Unk11 = -1;
                                Unk12 = -101;
                                Unk13 = -1;
                            }

                            /// <summary>
                            /// Creates a deep copy of the struct.
                            /// </summary>
                            public UnkStruct3 DeepCopy()
                            {
                                return (UnkStruct3)MemberwiseClone();
                            }

                            internal UnkStruct3(BinaryReaderEx br)
                            {
                                Unk00 = br.ReadSingle();
                                Unk04 = br.ReadSingle();
                                Unk08 = br.ReadSingle();
                                Unk0C = br.ReadSingle();
                                Unk10 = br.ReadSByte();
                                Unk11 = br.ReadSByte();
                                Unk12 = br.ReadSByte();
                                Unk13 = br.ReadSByte();
                                br.AssertPattern(12, 0);
                            }

                            internal void Write(BinaryWriterEx bw)
                            {
                                bw.WriteSingle(Unk00);
                                bw.WriteSingle(Unk04);
                                bw.WriteSingle(Unk08);
                                bw.WriteSingle(Unk0C);
                                bw.WriteSByte(Unk10);
                                bw.WriteSByte(Unk11);
                                bw.WriteSByte(Unk12);
                                bw.WriteSByte(Unk13);
                                bw.WritePattern(12, 0);
                            }
                        }

                        #endregion
                    }

                    public class UnkSceneConfig5
                    {
                        public int Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk1C { get; set; }
                        public float Unk20 { get; set; }
                        public float Unk24 { get; set; }
                        public byte Unk28 { get; set; }
                        public byte Unk29 { get; set; }
                        public byte Unk2A { get; set; }
                        public byte Unk2B { get; set; }
                        public float Unk2C { get; set; }
                        public float Unk30 { get; set; }
                        public float Unk34 { get; set; }
                        public float Unk38 { get; set; }
                        public float Unk3C { get; set; }
                        public float Unk40 { get; set; }
                        public float Unk44 { get; set; }
                        public float Unk48 { get; set; }
                        public float Unk4C { get; set; }
                        public float Unk50 { get; set; }
                        public float Unk54 { get; set; }
                        public float Unk58 { get; set; }
                        public float Unk5C { get; set; }
                        public float Unk60 { get; set; }
                        public float Unk64 { get; set; }
                        public float Unk68 { get; set; }
                        public float Unk6C { get; set; }
                        public float Unk70 { get; set; }
                        public float Unk74 { get; set; }
                        public float Unk78 { get; set; }
                        public float Unk7C { get; set; }

                        public UnkSceneConfig5()
                        {
                            Unk00 = 0;
                            Unk04 = 60f;
                            Unk08 = 300f;
                            Unk0C = 0;
                            Unk10 = 0.84f;
                            Unk14 = 100f;
                            Unk18 = 300f;
                            Unk1C = -0.1f;
                            Unk20 = 0.0009f;
                            Unk24 = 0;
                            Unk28 = 1;
                            Unk29 = 0;
                            Unk2A = 0;
                            Unk2B = 0;
                            Unk2C = 0;
                            Unk30 = 0;
                            Unk34 = 0;
                            Unk38 = 150f;
                            Unk3C = 200f;
                            Unk40 = 300f;
                            Unk44 = -0.1f;
                            Unk48 = 0;
                            Unk4C = 0;
                            Unk50 = 0;
                            Unk54 = 0;
                            Unk58 = 0;
                            Unk5C = 0;
                            Unk60 = 0;
                            Unk64 = 0;
                            Unk68 = 0;
                            Unk6C = 0;
                            Unk70 = 0;
                            Unk74 = 0;
                            Unk78 = 0;
                            Unk7C = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig5 DeepCopy()
                        {
                            return (UnkSceneConfig5)MemberwiseClone();
                        }

                        internal UnkSceneConfig5(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            Unk1C = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadSingle();
                            Unk28 = br.ReadByte();
                            Unk29 = br.ReadByte();
                            Unk2A = br.ReadByte();
                            Unk2B = br.ReadByte();
                            Unk2C = br.ReadSingle();
                            Unk30 = br.ReadSingle();
                            Unk34 = br.ReadSingle();
                            Unk38 = br.ReadSingle();
                            Unk3C = br.ReadSingle();
                            Unk40 = br.ReadSingle();
                            Unk44 = br.ReadSingle();
                            Unk48 = br.ReadSingle();
                            Unk4C = br.ReadSingle();
                            Unk50 = br.ReadSingle();
                            Unk54 = br.ReadSingle();
                            Unk58 = br.ReadSingle();
                            Unk5C = br.ReadSingle();
                            Unk60 = br.ReadSingle();
                            Unk64 = br.ReadSingle();
                            Unk68 = br.ReadSingle();
                            Unk6C = br.ReadSingle();
                            Unk70 = br.ReadSingle();
                            Unk74 = br.ReadSingle();
                            Unk78 = br.ReadSingle();
                            Unk7C = br.ReadSingle();
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk14);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk1C);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Unk24);
                            bw.WriteByte(Unk28);
                            bw.WriteByte(Unk29);
                            bw.WriteByte(Unk2A);
                            bw.WriteByte(Unk2B);
                            bw.WriteSingle(Unk2C);
                            bw.WriteSingle(Unk30);
                            bw.WriteSingle(Unk34);
                            bw.WriteSingle(Unk38);
                            bw.WriteSingle(Unk3C);
                            bw.WriteSingle(Unk40);
                            bw.WriteSingle(Unk44);
                            bw.WriteSingle(Unk48);
                            bw.WriteSingle(Unk4C);
                            bw.WriteSingle(Unk50);
                            bw.WriteSingle(Unk54);
                            bw.WriteSingle(Unk58);
                            bw.WriteSingle(Unk5C);
                            bw.WriteSingle(Unk60);
                            bw.WriteSingle(Unk64);
                            bw.WriteSingle(Unk68);
                            bw.WriteSingle(Unk6C);
                            bw.WriteSingle(Unk70);
                            bw.WriteSingle(Unk74);
                            bw.WriteSingle(Unk78);
                            bw.WriteSingle(Unk7C);
                        }
                    }

                    public class UnkSceneConfig6
                    {
                        public int Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk1C { get; set; }
                        public float Unk20 { get; set; }
                        public float Unk24 { get; set; }
                        public float Unk28 { get; set; }
                        public float Unk2C { get; set; }
                        public float Unk30 { get; set; }
                        public float Unk34 { get; set; }
                        public float Unk38 { get; set; }
                        public float Unk3C { get; set; }

                        public UnkSceneConfig6()
                        {
                            Unk00 = 0;
                            Unk04 = 3.8f;
                            Unk08 = 4000f;
                            Unk0C = 0.09f;
                            Unk10 = 0.06f;
                            Unk14 = 0;
                            Unk18 = 1f;
                            Unk1C = 4f;
                            Unk20 = 1.4f;
                            Unk24 = 1.1f;
                            Unk28 = 0;
                            Unk2C = 2f;
                            Unk30 = 1.2f;
                            Unk34 = 0;
                            Unk38 = 0.1f;
                            Unk3C = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig6 DeepCopy()
                        {
                            return (UnkSceneConfig6)MemberwiseClone();
                        }

                        internal UnkSceneConfig6(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            Unk1C = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadSingle();
                            Unk28 = br.ReadSingle();
                            Unk2C = br.ReadSingle();
                            Unk30 = br.ReadSingle();
                            Unk34 = br.ReadSingle();
                            Unk38 = br.ReadSingle();
                            Unk3C = br.ReadSingle();
                            br.AssertPattern(64, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk14);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk1C);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Unk24);
                            bw.WriteSingle(Unk28);
                            bw.WriteSingle(Unk2C);
                            bw.WriteSingle(Unk30);
                            bw.WriteSingle(Unk34);
                            bw.WriteSingle(Unk38);
                            bw.WriteSingle(Unk3C);
                            bw.WritePattern(64, 0);
                        }
                    }

                    public class UnkSceneConfig7
                    {
                        public byte Unk00 { get; set; }

                        public UnkSceneConfig7()
                        {
                            Unk00 = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig7 DeepCopy()
                        {
                            return (UnkSceneConfig7)MemberwiseClone();
                        }

                        internal UnkSceneConfig7(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadByte();
                            br.AssertPattern(63, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteByte(Unk00);
                            bw.WritePattern(63, 0);
                        }
                    }

                    public class UnkSceneConfig8
                    {
                        public short Unk00 { get; set; }
                        public short Unk02 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }
                        public short Unk1C { get; set; }
                        public short Unk1E { get; set; }
                        public float Unk20 { get; set; }
                        public float Unk24 { get; set; }
                        public byte Unk28 { get; set; }
                        public byte Unk29 { get; set; }
                        public byte Unk2A { get; set; }
                        public byte Unk2B { get; set; }
                        public float Unk2C { get; set; }
                        public float Unk30 { get; set; }
                        public float Unk34 { get; set; }
                        public float Unk38 { get; set; }
                        public float Unk3C { get; set; }
                        public float Unk40 { get; set; }
                        public float Unk44 { get; set; }

                        public UnkSceneConfig8()
                        {
                            Unk00 = 511;
                            Unk02 = 23090;
                            Unk04 = 0.5f;
                            Unk08 = 0.35f;
                            Unk0C = 0.4f;
                            Unk10 = -0.1f;
                            Unk14 = -0.04f;
                            Unk18 = 0.03f;
                            Unk1C = 1799;
                            Unk1E = 256;
                            Unk20 = 1f;
                            Unk24 = 1f;
                            Unk28 = 63;
                            Unk29 = 128;
                            Unk2A = 0;
                            Unk2B = 0;
                            Unk2C = 0.1f;
                            Unk30 = 0.1f;
                            Unk34 = 0;
                            Unk38 = 0;
                            Unk3C = 0;
                            Unk40 = 0;
                            Unk44 = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig8 DeepCopy()
                        {
                            return (UnkSceneConfig8)MemberwiseClone();
                        }

                        internal UnkSceneConfig8(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt16();
                            Unk02 = br.ReadInt16();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            Unk1C = br.ReadInt16();
                            Unk1E = br.ReadInt16();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadSingle();
                            Unk28 = br.ReadByte();
                            Unk29 = br.ReadByte();
                            Unk2A = br.ReadByte();
                            Unk2B = br.ReadByte();
                            Unk2C = br.ReadSingle();
                            Unk30 = br.ReadSingle();
                            Unk34 = br.ReadSingle();
                            Unk38 = br.ReadSingle();
                            Unk3C = br.ReadSingle();
                            Unk40 = br.ReadSingle();
                            Unk44 = br.ReadSingle();
                            br.AssertPattern(56, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt16(Unk00);
                            bw.WriteInt16(Unk02);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk14);
                            bw.WriteSingle(Unk18);
                            bw.WriteInt16(Unk1C);
                            bw.WriteInt16(Unk1E);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Unk24);
                            bw.WriteByte(Unk28);
                            bw.WriteByte(Unk29);
                            bw.WriteByte(Unk2A);
                            bw.WriteByte(Unk2B);
                            bw.WriteSingle(Unk2C);
                            bw.WriteSingle(Unk30);
                            bw.WriteSingle(Unk34);
                            bw.WriteSingle(Unk38);
                            bw.WriteSingle(Unk3C);
                            bw.WriteSingle(Unk40);
                            bw.WriteSingle(Unk44);
                            bw.WritePattern(56, 0);
                        }
                    }

                    public class UnkSceneConfig9
                    {
                        public float Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk12 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk20 { get; set; }
                        public float Unk22 { get; set; }
                        public float Unk24 { get; set; }
                        public float Unk28 { get; set; }
                        public float Unk30 { get; set; }
                        public float Unk32 { get; set; }
                        public float Unk34 { get; set; }
                        public float Unk38 { get; set; }

                        public UnkSceneConfig9()
                        {
                            Unk00 = 0;
                            Unk04 = 0;
                            Unk08 = 0;
                            Unk0C = 0;
                            Unk10 = 0;
                            Unk12 = 0;
                            Unk14 = 0;
                            Unk18 = 0;
                            Unk20 = 0;
                            Unk22 = 0;
                            Unk24 = 0;
                            Unk28 = 0;
                            Unk30 = 0;
                            Unk32 = 0;
                            Unk34 = 0;
                            Unk38 = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig9 DeepCopy()
                        {
                            return (UnkSceneConfig9)MemberwiseClone();
                        }

                        internal UnkSceneConfig9(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadSingle();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk12 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk22 = br.ReadSingle();
                            Unk24 = br.ReadSingle();
                            Unk28 = br.ReadSingle();
                            Unk30 = br.ReadSingle();
                            Unk32 = br.ReadSingle();
                            Unk34 = br.ReadSingle();
                            Unk38 = br.ReadSingle();
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteSingle(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk12);
                            bw.WriteSingle(Unk14);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Unk22);
                            bw.WriteSingle(Unk24);
                            bw.WriteSingle(Unk28);
                            bw.WriteSingle(Unk30);
                            bw.WriteSingle(Unk32);
                            bw.WriteSingle(Unk34);
                            bw.WriteSingle(Unk38);
                        }
                    }

                    public class UnkSceneConfig10
                    {
                        public int Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk12 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk20 { get; set; }
                        public float Unk22 { get; set; }
                        public float Unk24 { get; set; }
                        public float Unk28 { get; set; }
                        public float Unk30 { get; set; }
                        public float Unk32 { get; set; }
                        public float Unk34 { get; set; }
                        public float Unk38 { get; set; }

                        public UnkSceneConfig10()
                        {
                            Unk00 = 1;
                            Unk04 = 0;
                            Unk08 = 0;
                            Unk0C = 0;
                            Unk10 = 0;
                            Unk12 = 2000f;
                            Unk14 = 0.5f;
                            Unk18 = 10f;
                            Unk20 = 10f;
                            Unk22 = 1f;
                            Unk24 = 0;
                            Unk28 = 0;
                            Unk30 = 0;
                            Unk32 = 0;
                            Unk34 = 0;
                            Unk38 = 0;
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig10 DeepCopy()
                        {
                            return (UnkSceneConfig10)MemberwiseClone();
                        }

                        internal UnkSceneConfig10(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk12 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk22 = br.ReadSingle();
                            Unk24 = br.ReadSingle();
                            Unk28 = br.ReadSingle();
                            Unk30 = br.ReadSingle();
                            Unk32 = br.ReadSingle();
                            Unk34 = br.ReadSingle();
                            Unk38 = br.ReadSingle();
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk12);
                            bw.WriteSingle(Unk14);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Unk22);
                            bw.WriteSingle(Unk24);
                            bw.WriteSingle(Unk28);
                            bw.WriteSingle(Unk30);
                            bw.WriteSingle(Unk32);
                            bw.WriteSingle(Unk34);
                            bw.WriteSingle(Unk38);
                        }
                    }

                    #endregion
                }

                #endregion
            }

            public class Wind : Event
            {
                private protected override EventType Type => EventType.Wind;
                private protected override bool HasTypeData => true;

                public WindConfig Config { get; private set; }

                /// <summary>
                /// Creates wind parameters with default values.
                /// </summary>
                public Wind() : base("wind parameters")
                {
                    Config = new WindConfig();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var wind = (Wind)evnt;
                    wind.Config = Config.DeepCopy();
                }

                internal Wind(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Config = new WindConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Config.Write(bw);

                #region Sub Structs

                public class WindConfig
                {
                    public byte Unk00 { get; set; }
                    public byte Unk01 { get; set; }
                    public short Unk02 { get; set; }
                    public short Unk04 { get; set; }
                    public short Unk06 { get; set; }
                    public float WindPower { get; set; }

                    public WindConfig()
                    {
                        Unk00 = 1;
                        Unk01 = 0;
                        Unk02 = 3;
                        Unk04 = 10;
                        Unk06 = 0;
                        WindPower = 3f;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public WindConfig DeepCopy()
                    {
                        return (WindConfig)MemberwiseClone();
                    }

                    internal WindConfig(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadByte();
                        Unk01 = br.ReadByte();
                        Unk02 = br.ReadInt16();
                        Unk04 = br.ReadInt16();
                        Unk06 = br.ReadInt16();
                        WindPower = br.ReadSingle();
                        br.AssertPattern(20, 0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteByte(Unk00);
                        bw.WriteByte(Unk01);
                        bw.WriteInt16(Unk02);
                        bw.WriteInt16(Unk04);
                        bw.WriteInt16(Unk06);
                        bw.WriteSingle(WindPower);
                        bw.WritePattern(20, 0);
                    }
                }

                #endregion
            }

            public class Rain : Event
            {
                private protected override EventType Type => EventType.Rain;
                private protected override bool HasTypeData => true;

                public RainConfig Config { get; private set; }

                /// <summary>
                /// Creates rain parameters with default values.
                /// </summary>
                public Rain() : base("rain parameters")
                {
                    Config = new RainConfig();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var rain = (Rain)evnt;
                    rain.Config = Config.DeepCopy();
                }

                internal Rain(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Config = new RainConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Config.Write(bw);

                #region Sub Structs

                public class RainConfig
                {
                    public short Unk00 { get; set; }
                    public short Unk02 { get; set; }
                    public short Unk04 { get; set; }
                    public short Unk06 { get; set; }
                    public short Unk08 { get; set; }
                    public short Unk0A { get; set; }
                    public float Unk0C { get; set; }
                    public float Unk10 { get; set; }
                    public short Unk14 { get; set; }
                    public short Unk16 { get; set; }
                    public float Unk18 { get; set; }
                    public float Unk1C { get; set; }
                    public float Unk20 { get; set; }
                    public int Unk24 { get; set; }

                    public RainConfig()
                    {
                        Unk00 = 1;
                        Unk02 = 0;
                        Unk04 = 83;
                        Unk06 = 20;
                        Unk08 = -1;
                        Unk0A = 0;
                        Unk0C = 30f;
                        Unk10 = 0.003f;
                        Unk14 = 2056;
                        Unk16 = 0;
                        Unk18 = 80f;
                        Unk1C = 0.8f;
                        Unk20 = 0.8f;
                        Unk24 = -1;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public RainConfig DeepCopy()
                    {
                        return (RainConfig)MemberwiseClone();
                    }

                    internal RainConfig(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt16();
                        Unk02 = br.ReadInt16();
                        Unk04 = br.ReadInt16();
                        Unk06 = br.ReadInt16();
                        Unk08 = br.ReadInt16();
                        Unk0A = br.ReadInt16();
                        Unk0C = br.ReadSingle();
                        Unk10 = br.ReadSingle();
                        Unk14 = br.ReadInt16();
                        Unk16 = br.ReadInt16();
                        Unk18 = br.ReadSingle();
                        Unk1C = br.ReadSingle();
                        Unk20 = br.ReadSingle();
                        Unk24 = br.ReadInt32();
                        br.AssertPattern(24, 0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt16(Unk00);
                        bw.WriteInt16(Unk02);
                        bw.WriteInt16(Unk04);
                        bw.WriteInt16(Unk06);
                        bw.WriteInt16(Unk08);
                        bw.WriteInt16(Unk0A);
                        bw.WriteSingle(Unk0C);
                        bw.WriteSingle(Unk10);
                        bw.WriteInt16(Unk14);
                        bw.WriteInt16(Unk16);
                        bw.WriteSingle(Unk18);
                        bw.WriteSingle(Unk1C);
                        bw.WriteSingle(Unk20);
                        bw.WriteInt32(Unk24);
                        bw.WritePattern(24, 0);
                    }
                }

                #endregion
            }

            public class BGM : Event
            {
                private protected override EventType Type => EventType.BGM;
                private protected override bool HasTypeData => true;

                public BGMConfig Config { get; private set; }

                /// <summary>
                /// Creates BGM parameters with default values.
                /// </summary>
                public BGM() : base("BGM")
                {
                    Config = new BGMConfig();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var bgm = (BGM)evnt;
                    bgm.Config = Config.DeepCopy();
                }

                internal BGM(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Config = new BGMConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Config.Write(bw);

                #region Sub Structs

                public class BGMConfig
                {
                    /// <summary>
                    /// The ID of the BGM to be played.
                    /// </summary>
                    public int BGMID { get; set; }

                    /// <summary>
                    /// Unknown. Some kind of ID.
                    /// </summary>
                    public int UnkID { get; set; }

                    public BGMConfig()
                    {
                        BGMID = -1;
                        UnkID = -1;
                    }

                    public BGMConfig(int bgmID)
                    {
                        BGMID = bgmID;
                        UnkID = -1;
                    }

                    public BGMConfig(int bgmID, int unkID)
                    {
                        BGMID = bgmID;
                        UnkID = unkID;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public BGMConfig DeepCopy()
                    {
                        return (BGMConfig)MemberwiseClone();
                    }

                    internal BGMConfig(BinaryReaderEx br)
                    {
                        BGMID = br.ReadInt32();
                        UnkID = br.ReadInt32();
                        br.AssertPattern(56, 0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(BGMID);
                        bw.WriteInt32(UnkID);
                        bw.WritePattern(56, 0);
                    }
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}
