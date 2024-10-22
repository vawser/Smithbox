using System;
using System.Collections.Generic;
using System.IO;

namespace SoulsFormats
{
    public partial class MSBFA
    {
        /// <summary>
        /// The different types of events.
        /// </summary>
        internal enum EventType
        {
            /// <summary>
            /// Appears to specify the name of the mission event script.
            /// </summary>
            Script = 5,

            /// <summary>
            /// Events of this type have names that sound like effect names.
            /// </summary>
            Effect = 100,

            /// <summary>
            /// An event holding scene parameters such as render distance.<br/>
            /// Called "filter" a lot in ACFA.
            /// </summary>
            Scene = 101,

            /// <summary>
            /// An event referencing what background game music should be played.
            /// </summary>
            BGM = 400,

            /// <summary>
            /// Always named "Rev". Revision?
            /// </summary>
            Rev = 401,

            /// <summary>
            /// Name always starts with "SFX".
            /// </summary>
            SFX = 500
        }

        #region Param

        /// <summary>
        /// Dynamic or interactive systems.
        /// </summary>
        public class EventParam : Param<Event>, IMsbParam<IMsbEvent>
        {
            /// <summary>
            /// Event script names, should only be 1.
            /// </summary>
            public List<Event.Script> Scripts { get; set; }

            /// <summary>
            /// Effects.
            /// </summary>
            public List<Event.Effect> Effects { get; set; }

            /// <summary>
            /// Scene parameters that control things such as render distance and bloom.
            /// </summary>
            public List<Event.Scene> Scenes { get; set; }

            /// <summary>
            /// Background music events, should only be 1.
            /// </summary>
            public List<Event.BGM> BGMs { get; set; }

            /// <summary>
            /// Revision events? Should only be 1.
            /// </summary>
            public List<Event.Rev> Revs { get; set; }

            /// <summary>
            /// SFX events?
            /// </summary>
            public List<Event.SFX> SFXs { get; set; }

            /// <summary>
            /// Creates an empty EventParam with the default version.
            /// </summary>
            public EventParam() : base(10001002, "EVENT_PARAM_ST")
            {
                Scripts = new List<Event.Script>();
                Effects = new List<Event.Effect>();
                Scenes = new List<Event.Scene>();
                BGMs = new List<Event.BGM>();
                Revs = new List<Event.Rev>();
                SFXs = new List<Event.SFX>();
            }

            public Event Add(Event evnt)
            {
                switch (evnt)
                {
                    case Event.Script e: Scripts.Add(e); break;
                    case Event.Effect e: Effects.Add(e); break;
                    case Event.Scene e: Scenes.Add(e); break;
                    case Event.BGM e: BGMs.Add(e); break;
                    case Event.Rev e: Revs.Add(e); break;
                    case Event.SFX e: SFXs.Add(e); break;
                    default: throw new ArgumentException($"Unrecognized type {evnt.GetType()}.", nameof(evnt));
                }

                return evnt;
            }
            IMsbEvent IMsbParam<IMsbEvent>.Add(IMsbEvent item) => Add((Event)item);

            /// <summary>
            /// Returns every Event in the order they'll be written.
            /// </summary>
            public override List<Event> GetEntries() => SFUtil.ConcatAll<Event>(Scripts, Effects, Scenes, BGMs, Revs, SFXs);
            IReadOnlyList<IMsbEvent> IMsbParam<IMsbEvent>.GetEntries() => GetEntries();

            internal override Event ReadEntry(BinaryReaderEx br)
            {
                EventType type = br.GetEnum32<EventType>(br.Position + 8);
                switch (type)
                {
                    case EventType.Script: return Scripts.EchoAdd(new Event.Script(br));
                    case EventType.Effect: return Effects.EchoAdd(new Event.Effect(br));
                    case EventType.Scene: return Scenes.EchoAdd(new Event.Scene(br));
                    case EventType.BGM: return BGMs.EchoAdd(new Event.BGM(br));
                    case EventType.Rev: return Revs.EchoAdd(new Event.Rev(br));
                    case EventType.SFX: return SFXs.EchoAdd(new Event.SFX(br));
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

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }
                else if (typeDataOffset > 0)
                {
                    throw new InvalidDataException($"{nameof(typeDataOffset)} must be {0} for {GetType()}");
                }
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

                if (HasTypeData)
                {
                    bw.FillInt32("TypeDataOffset", (int)(bw.Position - start));
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt32("TypeDataOffset", 0);
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteTypeData)}.");

            #region EventType Structs

            public class Script : Event
            {
                private protected override EventType Type => EventType.Script;
                private protected override bool HasTypeData => false;

                public Script() : base("mXXX") { }

                private protected override void DeepCopyTo(Event evnt) { }

                internal Script(BinaryReaderEx br) : base(br) { }
            }

            public class Effect : Event
            {
                private protected override EventType Type => EventType.Effect;
                private protected override bool HasTypeData => true;

                public EffectConfig Config { get; set; }

                public Effect() : base("effect")
                {
                    Config = new EffectConfig();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var effect = (Effect)evnt;
                    effect.Config = Config.DeepCopy();
                }

                internal Effect(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Config = new EffectConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Config.Write(bw);

                #region Sub Structs

                public class EffectConfig
                {
                    public byte ID { get; set; }
                    public byte Unk01 { get; set; }
                    public short Unk02 { get; set; }
                    public short Unk04 { get; set; }
                    public short Unk06 { get; set; }
                    public sbyte Unk08 { get; set; }
                    public sbyte Unk09 { get; set; }
                    public sbyte Unk0A { get; set; }
                    public sbyte Unk0B { get; set; }
                    public sbyte Unk0C { get; set; }
                    public sbyte Unk0D { get; set; }
                    public sbyte Unk0E { get; set; }
                    public sbyte Unk0F { get; set; }
                    public short Unk10 { get; set; }
                    public sbyte Unk12 { get; set; }
                    public sbyte Unk13 { get; set; }
                    public sbyte Unk14 { get; set; }
                    public sbyte Unk15 { get; set; }
                    public sbyte Unk16 { get; set; }
                    public sbyte Unk17 { get; set; }
                    public int Unk18 { get; set; }
                    public int Unk1C { get; set; }
                    public int Unk20 { get; set; }
                    public int Unk24 { get; set; }
                    public int Unk28 { get; set; }
                    public int Unk2C { get; set; }
                    public int Unk30 { get; set; }
                    public int Unk34 { get; set; }
                    public short Unk38 { get; set; }
                    public short Unk3A { get; set; }
                    public short Unk3C { get; set; }
                    public short Unk3E { get; set; }
                    public short Unk40 { get; set; }
                    public short Unk42 { get; set; }
                    public short Unk44 { get; set; }
                    public short Unk46 { get; set; }
                    public short Unk48 { get; set; }
                    public short Unk4A { get; set; }
                    public short Unk4C { get; set; }

                    public EffectConfig()
                    {
                        ID = 0;
                        Unk01 = 1;
                        Unk02 = 15;
                        Unk04 = -160;
                        Unk06 = -61;
                        Unk08 = 105;
                        Unk09 = 70;
                        Unk0A = -1;
                        Unk0B = -61;
                        Unk0C = 115;
                        Unk0D = 100;
                        Unk0E = -1;
                        Unk0F = -10;
                        Unk10 = 40;
                        Unk12 = 98;
                        Unk13 = -128;
                        Unk14 = 105;
                        Unk15 = 40;
                        Unk16 = 0;
                        Unk17 = 0;
                        Unk18 = 0;
                        Unk1C = 0;
                        Unk20 = 0;
                        Unk24 = 27766;
                        Unk28 = -2146145162;
                        Unk2C = -2147483648;
                        Unk30 = 400;
                        Unk34 = 10000;
                        Unk38 = 15425;
                        Unk3A = 21760;
                        Unk3C = 0;
                        Unk3E = 0;
                        Unk40 = 0;
                        Unk42 = 0;
                        Unk44 = 0;
                        Unk46 = 0;
                        Unk48 = 0;
                        Unk4A = 0;
                        Unk4C = 0;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public EffectConfig DeepCopy()
                    {
                        var lightConfig = (EffectConfig)MemberwiseClone();
                        return lightConfig;
                    }

                    internal EffectConfig(BinaryReaderEx br)
                    {
                        ID = br.ReadByte();
                        Unk01 = br.ReadByte();
                        Unk02 = br.ReadInt16();
                        Unk04 = br.ReadInt16();
                        Unk06 = br.ReadInt16();
                        Unk08 = br.ReadSByte();
                        Unk09 = br.ReadSByte();
                        Unk0A = br.ReadSByte();
                        Unk0B = br.ReadSByte();
                        Unk0C = br.ReadSByte();
                        Unk0D = br.ReadSByte();
                        Unk0E = br.ReadSByte();
                        Unk0F = br.ReadSByte();
                        Unk10 = br.ReadInt16();
                        Unk12 = br.ReadSByte();
                        Unk13 = br.ReadSByte();
                        Unk14 = br.ReadSByte();
                        Unk15 = br.ReadSByte();
                        Unk16 = br.ReadSByte();
                        Unk17 = br.ReadSByte();
                        Unk18 = br.ReadInt32();
                        Unk1C = br.ReadInt32();
                        Unk20 = br.ReadInt32();
                        Unk24 = br.ReadInt32();
                        Unk28 = br.ReadInt32();
                        Unk2C = br.ReadInt32();
                        Unk30 = br.ReadInt32();
                        Unk34 = br.ReadInt32();
                        Unk38 = br.ReadInt16();
                        Unk3A = br.ReadInt16();
                        Unk3C = br.ReadInt16();
                        Unk3E = br.ReadInt16();
                        Unk40 = br.ReadInt16();
                        Unk42 = br.ReadInt16();
                        Unk44 = br.ReadInt16();
                        Unk46 = br.ReadInt16();
                        Unk48 = br.ReadInt16();
                        Unk4A = br.ReadInt16();
                        Unk4C = br.ReadInt16();
                        br.AssertPattern(434, 0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteByte(ID);
                        bw.WriteByte(Unk01);
                        bw.WriteInt16(Unk02);
                        bw.WriteInt16(Unk04);
                        bw.WriteInt16(Unk06);
                        bw.WriteSByte(Unk08);
                        bw.WriteSByte(Unk09);
                        bw.WriteSByte(Unk0A);
                        bw.WriteSByte(Unk0B);
                        bw.WriteSByte(Unk0C);
                        bw.WriteSByte(Unk0D);
                        bw.WriteSByte(Unk0E);
                        bw.WriteSByte(Unk0F);
                        bw.WriteInt16(Unk10);
                        bw.WriteSByte(Unk12);
                        bw.WriteSByte(Unk13);
                        bw.WriteSByte(Unk14);
                        bw.WriteSByte(Unk15);
                        bw.WriteSByte(Unk16);
                        bw.WriteSByte(Unk17);
                        bw.WriteInt32(Unk18);
                        bw.WriteInt32(Unk1C);
                        bw.WriteInt32(Unk20);
                        bw.WriteInt32(Unk24);
                        bw.WriteInt32(Unk28);
                        bw.WriteInt32(Unk2C);
                        bw.WriteInt32(Unk30);
                        bw.WriteInt32(Unk34);
                        bw.WriteInt16(Unk38);
                        bw.WriteInt16(Unk3A);
                        bw.WriteInt16(Unk3C);
                        bw.WriteInt16(Unk3E);
                        bw.WriteInt16(Unk40);
                        bw.WriteInt16(Unk42);
                        bw.WriteInt16(Unk44);
                        bw.WriteInt16(Unk46);
                        bw.WriteInt16(Unk48);
                        bw.WriteInt16(Unk4A);
                        bw.WriteInt16(Unk4C);
                        bw.WritePattern(434, 0);
                    }
                }

                #endregion
            }

            public class Scene : Event
            {
                private protected override EventType Type => EventType.Scene;
                private protected override bool HasTypeData => true;

                public SceneConfig Config { get; private set; }

                public Scene() : base("filter")
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
                    public UnkSceneConfig11 Config11 { get; set; }
                    public UnkSceneConfig12 Config12 { get; set; }
                    public UnkSceneConfig13 Config13 { get; set; }
                    public UnkSceneConfig14 Config14 { get; set; }

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
                        Config11 = new UnkSceneConfig11();
                        Config12 = new UnkSceneConfig12();
                        Config13 = new UnkSceneConfig13();
                        Config14 = new UnkSceneConfig14();
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
                        sceneConfig.Config11 = Config11.DeepCopy();
                        sceneConfig.Config12 = Config12.DeepCopy();
                        sceneConfig.Config13 = Config13.DeepCopy();
                        sceneConfig.Config14 = Config14.DeepCopy();
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
                        int offsetUnkSceneConfig11 = br.ReadInt32();
                        int offsetUnkSceneConfig12 = br.ReadInt32();
                        int offsetUnkSceneConfig13 = br.ReadInt32();
                        int offsetUnkSceneConfig14 = br.ReadInt32();
                        br.AssertPattern(456, 0);

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
                        else
                        {
                            Config10 = null;
                        }

                        if (offsetUnkSceneConfig11 > 0)
                        {
                            br.Position = start + offsetUnkSceneConfig11;
                            Config11 = new UnkSceneConfig11(br);
                        }
                        else
                        {
                            Config11 = null;
                        }

                        if (offsetUnkSceneConfig12 > 0)
                        {
                            br.Position = start + offsetUnkSceneConfig12;
                            Config12 = new UnkSceneConfig12(br);
                        }
                        else
                        {
                            Config12 = null;
                        }

                        if (offsetUnkSceneConfig13 > 0)
                        {
                            br.Position = start + offsetUnkSceneConfig13;
                            Config13 = new UnkSceneConfig13(br);
                        }
                        else
                        {
                            Config13 = null;
                        }

                        if (offsetUnkSceneConfig14 > 0)
                        {
                            br.Position = start + offsetUnkSceneConfig14;
                            Config14 = new UnkSceneConfig14(br);
                        }
                        else
                        {
                            Config14 = null;
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
                        bw.ReserveInt32("OffsetUnkSceneConfig11");
                        bw.ReserveInt32("OffsetUnkSceneConfig12");
                        bw.ReserveInt32("OffsetUnkSceneConfig13");
                        bw.ReserveInt32("OffsetUnkSceneConfig14");
                        bw.WritePattern(456, 0);

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

                        if (Config11 != null)
                        {
                            bw.FillInt32("OffsetUnkSceneConfig11", (int)(bw.Position - start));
                            Config11.Write(bw);
                        }
                        else
                        {
                            bw.FillInt32("OffsetUnkSceneConfig11", 0);
                        }

                        if (Config12 != null)
                        {
                            bw.FillInt32("OffsetUnkSceneConfig12", (int)(bw.Position - start));
                            Config12.Write(bw);
                        }
                        else
                        {
                            bw.FillInt32("OffsetUnkSceneConfig12", 0);
                        }

                        if (Config13 != null)
                        {
                            bw.FillInt32("OffsetUnkSceneConfig13", (int)(bw.Position - start));
                            Config13.Write(bw);
                        }
                        else
                        {
                            bw.FillInt32("OffsetUnkSceneConfig13", 0);
                        }

                        if (Config14 != null)
                        {
                            bw.FillInt32("OffsetUnkSceneConfig14", (int)(bw.Position - start));
                            Config14.Write(bw);
                        }
                        else
                        {
                            bw.FillInt32("OffsetUnkSceneConfig14", 0);
                        }
                    }

                    #region Sub Structs

                    public class UnkSceneConfig1
                    {
                        public byte Unk00 { get; set; }
                        public byte Unk01 { get; set; }
                        public byte Unk02 { get; set; }
                        public byte Unk03 { get; set; }
                        public float GlobalDrawDistance { get; set; }
                        public float GlobalWaterLevel { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public short Unk14 { get; set; }
                        public short Unk16 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk1C { get; set; }
                        public float Unk20 { get; set; }
                        public short Unk24 { get; set; }
                        public short Unk26 { get; set; }
                        public int Unk28 { get; set; }
                        public int Unk2C { get; set; }
                        public int Unk30 { get; set; }
                        public float Unk34 { get; set; }
                        public float Unk38 { get; set; }
                        public short Unk3C { get; set; }
                        public short Unk40 { get; set; }
                        public short Unk44 { get; set; }
                        public short Unk48 { get; set; }
                        public short Unk4C { get; set; }
                        public short Unk50 { get; set; }
                        public short Unk54 { get; set; }
                        public short Unk58 { get; set; }
                        public short Unk5C { get; set; }
                        public short Unk60 { get; set; }
                        public short Unk64 { get; set; }
                        public short Unk68 { get; set; }
                        public short Unk6C { get; set; }
                        public short Unk70 { get; set; }
                        public short Unk74 { get; set; }
                        public short Unk78 { get; set; }
                        public short Unk7C { get; set; }

                        public UnkSceneConfig1()
                        {
                            Unk00 = 0;
                            Unk01 = 0;
                            Unk02 = 0;
                            Unk03 = 0;
                            GlobalDrawDistance = 23000f;
                            GlobalWaterLevel = 0f;
                            Unk0C = 0;
                            Unk10 = 0;
                            Unk14 = 0;
                            Unk16 = 0;
                            Unk18 = 0;
                            Unk1C = 0;
                            Unk20 = 99999f;
                            Unk24 = 0;
                            Unk26 = 0;
                            Unk28 = 0;
                            Unk2C = 0;
                            Unk30 = 0;
                            Unk34 = -150f;
                            Unk38 = -100000f;
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
                        public UnkSceneConfig1 DeepCopy()
                        {
                            return (UnkSceneConfig1)MemberwiseClone();
                        }

                        internal UnkSceneConfig1(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadByte();
                            Unk01 = br.ReadByte();
                            Unk02 = br.ReadByte();
                            Unk03 = br.ReadByte();
                            GlobalDrawDistance = br.ReadSingle();
                            GlobalWaterLevel = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadInt16();
                            Unk16 = br.ReadInt16();
                            Unk18 = br.ReadSingle();
                            Unk1C = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadInt16();
                            Unk26 = br.ReadInt16();
                            Unk28 = br.ReadInt32();
                            Unk2C = br.ReadInt32();
                            Unk30 = br.ReadInt32();
                            Unk34 = br.ReadSingle();
                            Unk38 = br.ReadSingle();
                            Unk3C = br.ReadInt16();
                            Unk40 = br.ReadInt16();
                            Unk44 = br.ReadInt16();
                            Unk48 = br.ReadInt16();
                            Unk4C = br.ReadInt16();
                            Unk50 = br.ReadInt16();
                            Unk54 = br.ReadInt16();
                            Unk58 = br.ReadInt16();
                            Unk5C = br.ReadInt16();
                            Unk60 = br.ReadInt16();
                            Unk64 = br.ReadInt16();
                            Unk68 = br.ReadInt16();
                            Unk6C = br.ReadInt16();
                            Unk70 = br.ReadInt16();
                            Unk74 = br.ReadInt16();
                            Unk78 = br.ReadInt16();
                            Unk7C = br.ReadInt16();
                            br.AssertPattern(418, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteByte(Unk00);
                            bw.WriteByte(Unk01);
                            bw.WriteByte(Unk02);
                            bw.WriteByte(Unk03);
                            bw.WriteSingle(GlobalDrawDistance);
                            bw.WriteSingle(GlobalWaterLevel);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteInt16(Unk14);
                            bw.WriteInt16(Unk16);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk1C);
                            bw.WriteSingle(Unk20);
                            bw.WriteInt16(Unk24);
                            bw.WriteInt16(Unk26);
                            bw.WriteInt32(Unk28);
                            bw.WriteInt32(Unk2C);
                            bw.WriteInt32(Unk30);
                            bw.WriteSingle(Unk34);
                            bw.WriteSingle(Unk38);
                            bw.WriteInt16(Unk3C);
                            bw.WriteInt16(Unk40);
                            bw.WriteInt16(Unk44);
                            bw.WriteInt16(Unk48);
                            bw.WriteInt16(Unk4C);
                            bw.WriteInt16(Unk50);
                            bw.WriteInt16(Unk54);
                            bw.WriteInt16(Unk58);
                            bw.WriteInt16(Unk5C);
                            bw.WriteInt16(Unk60);
                            bw.WriteInt16(Unk64);
                            bw.WriteInt16(Unk68);
                            bw.WriteInt16(Unk6C);
                            bw.WriteInt16(Unk70);
                            bw.WriteInt16(Unk74);
                            bw.WriteInt16(Unk78);
                            bw.WriteInt16(Unk7C);
                            bw.WritePattern(418, 0);
                        }
                    }

                    public class UnkSceneConfig2
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
                        public int Unk24 { get; set; }

                        /// <summary>
                        /// Unknown; May be null.
                        /// </summary>
                        public UnkStruct1 Struct1 { get; set; }

                        public UnkSceneConfig2()
                        {
                            Unk00 = 1;
                            Unk04 = 4f;
                            Unk08 = 15f;
                            Unk0C = 1f;
                            Unk10 = 500f;
                            Unk14 = 1200f;
                            Unk18 = 1f;
                            Unk1C = 0.5f;
                            Unk20 = 0.7f;
                            Unk24 = 2;
                            Struct1 = new UnkStruct1();
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig2 DeepCopy()
                        {
                            var unkSceneConfig2 = (UnkSceneConfig2)MemberwiseClone();
                            unkSceneConfig2.Struct1 = Struct1.DeepCopy();
                            return unkSceneConfig2;
                        }

                        internal UnkSceneConfig2(BinaryReaderEx br)
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
                            Unk24 = br.ReadInt32();
                            int offset28 = br.ReadInt32();
                            br.AssertPattern(468, 0);

                            if (offset28 > 0)
                            {
                                br.Position = start + offset28;
                                Struct1 = new UnkStruct1(br);
                            }
                            else
                            {
                                Struct1 = null;
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
                            bw.WriteInt32(Unk24);
                            bw.ReserveInt32("Offset28");
                            bw.WritePattern(468, 0);

                            if (Struct1 != null)
                            {
                                bw.FillInt32("Offset28", (int)(bw.Position - start));
                                Struct1.Write(bw);
                            }
                            else
                            {
                                bw.FillInt32("Offset28", 0);
                            }
                        }

                        #region Sub Structs

                        public class UnkStruct1
                        {
                            public float Unk00 { get; set; }
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

                            public UnkStruct1()
                            {
                                Unk00 = 0f;
                                Unk04 = 1f;
                                Unk08 = 1f;
                                Unk0C = 0f;
                                Unk10 = 0f;
                                Unk14 = 0f;
                                Unk18 = 0f;
                                Unk1C = 1f;
                                Unk20 = 1f;
                                Unk24 = 1f;
                                Unk28 = 100f;
                                Unk2C = 200f;
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
                                Unk00 = br.ReadSingle();
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
                                br.AssertPattern(80, 0);
                            }

                            internal void Write(BinaryWriterEx bw)
                            {
                                bw.WriteSingle(Unk00);
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
                                bw.WritePattern(80, 0);
                            }
                        }

                        #endregion
                    }

                    public class UnkSceneConfig3
                    {
                        public int Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public int Unk0C { get; set; }
                        public float Red { get; set; }
                        public float Green { get; set; }
                        public float Blue { get; set; }
                        public float Brightness1 { get; set; }
                        public float Unk20 { get; set; }
                        public float Brightness2 { get; set; }
                        public float LightContrast { get; set; }
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
                        public float Unk64 { get; set; }

                        public UnkSceneConfig3()
                        {
                            Unk00 = 1;
                            Unk04 = 15f;
                            Unk08 = -160f;
                            Unk0C = 0;
                            Red = 255f;
                            Green = 225f;
                            Blue = 185f;
                            Brightness1 = 12.5f;
                            Unk20 = 0.5f;
                            Brightness2 = 0.7f;
                            LightContrast = 0.05f;
                            Unk2C = 200f;
                            Unk30 = 100f;
                            Unk34 = 255f;
                            Unk38 = 235f;
                            Unk3C = 215f;
                            Unk40 = 0.8f;
                            Unk44 = 0.6f;
                            Unk48 = 1;
                            Unk4C = 0.65f;
                            Unk50 = 0.57f;
                            Unk54 = 0.475f;
                            Unk58 = 0.65f;
                            Unk5C = 0.57f;
                            Unk60 = 0.475f;
                            Unk64 = 0f;
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
                            Unk0C = br.ReadInt32();
                            Red = br.ReadSingle();
                            Green = br.ReadSingle();
                            Blue = br.ReadSingle();
                            Brightness1 = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Brightness2 = br.ReadSingle();
                            LightContrast = br.ReadSingle();
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
                            Unk64 = br.ReadSingle();
                            br.AssertPattern(408, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteInt32(Unk0C);
                            bw.WriteSingle(Red);
                            bw.WriteSingle(Green);
                            bw.WriteSingle(Blue);
                            bw.WriteSingle(Brightness1);
                            bw.WriteSingle(Unk20);
                            bw.WriteSingle(Brightness2);
                            bw.WriteSingle(LightContrast);
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
                            bw.WriteSingle(Unk64);
                            bw.WritePattern(408, 0);
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

                        public UnkSceneConfig4()
                        {
                            Unk00 = 1;
                            Unk04 = 120f;
                            Unk08 = 2f;
                            Unk0C = 255f;
                            Unk10 = 180f;
                            Unk14 = 135f;
                            Unk18 = 1.3f;
                            Unk1C = 0.2f;
                            Unk20 = 0.2f;
                            Unk24 = 0f;
                            Unk28 = 0f;
                            Unk2C = 0f;
                            Unk30 = 0f;
                            Unk34 = 0f;
                            Unk38 = 0f;
                            Unk3C = 0f;
                            Unk40 = 0f;
                            Unk44 = 0f;
                            Unk48 = 0;
                            Unk4C = 0f;
                            Unk50 = 0f;
                            Unk54 = 0f;
                            Unk58 = 0f;
                            Unk5C = 0f;
                            Unk60 = 0f;
                        }

                        public UnkSceneConfig4 DeepCopy()
                        {
                            return (UnkSceneConfig4)MemberwiseClone();
                        }

                        internal UnkSceneConfig4(BinaryReaderEx br)
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
                            Unk40 = br.ReadSingle();
                            Unk44 = br.ReadSingle();
                            Unk48 = br.ReadInt32();
                            Unk4C = br.ReadSingle();
                            Unk50 = br.ReadSingle();
                            Unk54 = br.ReadSingle();
                            Unk58 = br.ReadSingle();
                            Unk5C = br.ReadSingle();
                            Unk60 = br.ReadSingle();
                            br.AssertPattern(412, 0);
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
                            bw.WriteSingle(Unk40);
                            bw.WriteSingle(Unk44);
                            bw.WriteInt32(Unk48);
                            bw.WriteSingle(Unk4C);
                            bw.WriteSingle(Unk50);
                            bw.WriteSingle(Unk54);
                            bw.WriteSingle(Unk58);
                            bw.WriteSingle(Unk5C);
                            bw.WriteSingle(Unk60);
                            bw.WritePattern(412, 0);
                        }
                    }

                    public class UnkSceneConfig5
                    {
                        public int Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public int Unk14 { get; set; }

                        public UnkSceneConfig5()
                        {
                            Unk00 = 1;
                            Unk04 = 135;
                            Unk08 = 134;
                            Unk0C = 138;
                            Unk10 = 0.3f;
                            Unk14 = 3;
                        }

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
                            Unk14 = br.ReadInt32();
                            br.AssertPattern(488, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteInt32(Unk14);
                            bw.WritePattern(488, 0);
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
                        public int Unk2C { get; set; }
                        public float Unk30 { get; set; }
                        public List<UnkStruct1> Struct1s { get; private set; }
                        public UnkStruct2 Struct2 { get; set; }
                        public UnkStruct3 Struct3 { get; set; }

                        public UnkSceneConfig6()
                        {
                            Unk00 = 1;
                            Unk04 = 10f;
                            Unk08 = -160f;
                            Unk0C = 255f;
                            Unk10 = 225f;
                            Unk14 = 185f;
                            Unk18 = 255f;
                            Unk1C = 255f;
                            Unk20 = 185f;
                            Unk24 = 125f;
                            Unk28 = 255f;
                            Unk2C = 0;
                            Unk30 = 0.2f;
                            Struct1s = new List<UnkStruct1>();
                            Struct2 = new UnkStruct2();
                            Struct3 = new UnkStruct3();
                        }

                        /// <summary>
                        /// Creates a deep copy of the struct.
                        /// </summary>
                        public UnkSceneConfig6 DeepCopy()
                        {
                            var unkSceneConfig4 = (UnkSceneConfig6)MemberwiseClone();

                            for (int i = 0; i < 22; i++)
                            {
                                unkSceneConfig4.Struct1s[i] = Struct1s[i].DeepCopy();
                            }

                            unkSceneConfig4.Struct2 = Struct2.DeepCopy();
                            unkSceneConfig4.Struct3 = Struct3.DeepCopy();
                            return unkSceneConfig4;
                        }

                        internal UnkSceneConfig6(BinaryReaderEx br)
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
                            Unk2C = br.ReadInt32();
                            Unk30 = br.ReadSingle();

                            br.AssertInt32(4); // Always 4
                            int[] offsetsUnkStruct1 = br.ReadInt32s(22);
                            int offsetUnkStruct2 = br.ReadInt32();
                            int offsetUnkStruct3 = br.ReadInt32();
                            br.AssertPattern(360, 0);

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
                            bw.WriteInt32(Unk2C);
                            bw.WriteSingle(Unk30);

                            bw.WriteInt32(4); // Always 4
                            for (int i = 0; i < 22; i++)
                            {
                                bw.ReserveInt32($"OffsetUnkStruct1_{i}");
                            }
                            bw.ReserveInt32("OffsetUnkStruct2");
                            bw.ReserveInt32("OffsetUnkStruct3");
                            bw.WritePattern(360, 0);

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
                                br.AssertPattern(108, 0);
                            }

                            internal void Write(BinaryWriterEx bw)
                            {
                                bw.WriteInt32(Unk00);
                                bw.WriteBytes(Unk04);
                                bw.WriteSingle(Unk08);
                                bw.WriteBytes(Unk0C);
                                bw.WriteBytes(Unk10);
                                bw.WritePattern(108, 0);
                            }
                        }

                        public class UnkStruct2
                        {
                            public int Unk00 { get; set; }
                            public float Unk04 { get; set; }
                            public float Unk08 { get; set; }
                            public int Unk0C { get; set; }
                            public float Unk10 { get; set; }
                            public float Unk14 { get; set; }
                            public float Unk18 { get; set; }
                            public float Unk1C { get; set; }
                            public float Unk20 { get; set; }
                            public int Unk24 { get; set; }
                            public int Unk28 { get; set; }

                            public UnkStruct2()
                            {
                                Unk00 = -1;
                                Unk04 = 1;
                                Unk08 = 0;
                                Unk0C = -1;
                                Unk10 = 0.3f;
                                Unk14 = 0.98f;
                                Unk18 = 0.98f;
                                Unk1C = 1;
                                Unk20 = 1;
                                Unk24 = -1;
                                Unk28 = -1;
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
                                Unk08 = br.ReadSingle();
                                Unk0C = br.ReadInt32();
                                Unk10 = br.ReadSingle();
                                Unk14 = br.ReadSingle();
                                Unk18 = br.ReadSingle();
                                Unk1C = br.ReadSingle();
                                Unk20 = br.ReadSingle();
                                Unk24 = br.ReadInt32();
                                Unk28 = br.ReadInt32();
                                br.AssertPattern(20, 0);
                            }

                            internal void Write(BinaryWriterEx bw)
                            {
                                bw.WriteInt32(Unk00);
                                bw.WriteSingle(Unk04);
                                bw.WriteSingle(Unk08);
                                bw.WriteInt32(Unk0C);
                                bw.WriteSingle(Unk10);
                                bw.WriteSingle(Unk14);
                                bw.WriteSingle(Unk18);
                                bw.WriteSingle(Unk1C);
                                bw.WriteSingle(Unk20);
                                bw.WriteInt32(Unk24);
                                bw.WriteInt32(Unk28);
                                bw.WritePattern(20, 0);
                            }
                        }

                        public class UnkStruct3
                        {
                            public int Unk00 { get; set; }
                            public float Unk04 { get; set; }
                            public float Unk08 { get; set; }
                            public float Unk0C { get; set; }
                            public int Unk10 { get; set; }

                            public UnkStruct3()
                            {
                                Unk00 = 0;
                                Unk04 = 70f;
                                Unk08 = 750f;
                                Unk0C = 750f;
                                Unk10 = -1;
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
                                Unk00 = br.ReadInt32();
                                Unk04 = br.ReadSingle();
                                Unk08 = br.ReadSingle();
                                Unk0C = br.ReadSingle();
                                Unk10 = br.ReadInt32();
                                br.AssertPattern(12, 0);
                            }

                            internal void Write(BinaryWriterEx bw)
                            {
                                bw.WriteInt32(Unk00);
                                bw.WriteSingle(Unk04);
                                bw.WriteSingle(Unk08);
                                bw.WriteSingle(Unk0C);
                                bw.WriteInt32(Unk10);
                                bw.WritePattern(12, 0);
                            }
                        }

                        #endregion
                    }

                    public class UnkSceneConfig7
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
                        public byte Unk20 { get; set; }
                        public byte Unk21 { get; set; }
                        public byte Unk22 { get; set; }
                        public byte Unk23 { get; set; }
                        public int Unk24 { get; set; }
                        public float Unk28 { get; set; }

                        public UnkSceneConfig7()
                        {
                            Unk00 = 511;
                            Unk02 = 0;
                            Unk04 = 0.7f;
                            Unk08 = 0.35f;
                            Unk0C = 0.35f;
                            Unk10 = 0f;
                            Unk14 = -0.01f;
                            Unk18 = -0.01f;
                            Unk1C = 4353;
                            Unk1E = 7715;
                            Unk20 = 3;
                            Unk21 = 0;
                            Unk22 = 0;
                            Unk23 = 0;
                            Unk24 = 3;
                            Unk28 = 1f;
                        }

                        public UnkSceneConfig7 DeepCopy()
                        {
                            return (UnkSceneConfig7)MemberwiseClone();
                        }

                        internal UnkSceneConfig7(BinaryReaderEx br)
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
                            Unk20 = br.ReadByte();
                            Unk21 = br.ReadByte();
                            Unk22 = br.ReadByte();
                            Unk23 = br.ReadByte();
                            Unk24 = br.ReadInt32();
                            Unk28 = br.ReadSingle();
                            br.AssertPattern(468, 0);
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
                            bw.WriteByte(Unk20);
                            bw.WriteByte(Unk21);
                            bw.WriteByte(Unk22);
                            bw.WriteByte(Unk23);
                            bw.WriteInt32(Unk24);
                            bw.WriteSingle(Unk28);
                            bw.WritePattern(468, 0);
                        }
                    }

                    public class UnkSceneConfig8
                    {
                        public float Unk00 { get; set; }
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
                        public float Unk40 { get; set; }
                        public float Unk44 { get; set; }

                        public UnkSceneConfig8()
                        {
                            Unk00 = 0f;
                            Unk04 = 90f;
                            Unk08 = 3500f;
                            Unk0C = 1000f;
                            Unk10 = 0.7f;
                            Unk14 = 2500f;
                            Unk18 = 1000f;
                            Unk1C = -0.1f;
                            Unk20 = 0f;
                            Unk24 = 0f;
                            Unk28 = 0f;
                            Unk2C = 0f;
                            Unk30 = 0.03f;
                            Unk34 = 0.2f;
                            Unk38 = 800f;
                            Unk3C = 450f;
                            Unk40 = 1500f;
                            Unk44 = -0.1f;
                        }

                        public UnkSceneConfig8 DeepCopy()
                        {
                            return (UnkSceneConfig8)MemberwiseClone();
                        }

                        internal UnkSceneConfig8(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadSingle();
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
                            Unk40 = br.ReadSingle();
                            Unk44 = br.ReadSingle();
                            br.AssertPattern(440, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteSingle(Unk00);
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
                            bw.WriteSingle(Unk40);
                            bw.WriteSingle(Unk44);
                            bw.WritePattern(440, 0);
                        }
                    }

                    public class UnkSceneConfig9
                    {
                        public int Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }

                        public UnkSceneConfig9()
                        {
                            Unk00 = 1;
                            Unk04 = 0.38f;
                            Unk08 = 1f;
                            Unk0C = 0.29f;
                            Unk10 = 0.43f;
                            Unk14 = 1f;
                            Unk18 = 0.01f;
                        }

                        public UnkSceneConfig9 DeepCopy()
                        {
                            return (UnkSceneConfig9)MemberwiseClone();
                        }

                        internal UnkSceneConfig9(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            br.AssertPattern(484, 0);
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
                            bw.WritePattern(484, 0);
                        }
                    }

                    public class UnkSceneConfig10
                    {
                        public int Unk00 { get; set; }
                        public int Unk04 { get; set; }
                        public int Unk08 { get; set; }
                        public int Unk0C { get; set; }
                        public int Unk10 { get; set; }
                        public int Unk14 { get; set; }

                        public UnkSceneConfig10()
                        {
                            Unk00 = 0;
                            Unk04 = 0;
                            Unk08 = 0;
                            Unk0C = 0;
                            Unk10 = 0;
                            Unk14 = 799;
                        }

                        public UnkSceneConfig10 DeepCopy()
                        {
                            return (UnkSceneConfig10)MemberwiseClone();
                        }

                        internal UnkSceneConfig10(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadInt32();
                            Unk08 = br.ReadInt32();
                            Unk0C = br.ReadInt32();
                            Unk10 = br.ReadInt32();
                            Unk14 = br.ReadInt32();
                            br.AssertPattern(104, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteInt32(Unk04);
                            bw.WriteInt32(Unk08);
                            bw.WriteInt32(Unk0C);
                            bw.WriteInt32(Unk10);
                            bw.WriteInt32(Unk14);
                            bw.WritePattern(104, 0);
                        }
                    }

                    public class UnkSceneConfig11
                    {
                        public float Unk00 { get; set; }
                        public float Unk04 { get; set; }
                        public float Unk08 { get; set; }
                        public float Unk0C { get; set; }
                        public float Unk10 { get; set; }
                        public float Unk14 { get; set; }
                        public float Unk18 { get; set; }
                        public float Unk1C { get; set; }
                        public float Unk20 { get; set; }
                        public int Unk24 { get; set; }
                        public float Unk28 { get; set; }
                        public float Unk2C { get; set; }
                        public float Unk30 { get; set; }
                        public float Unk34 { get; set; }

                        public UnkSceneConfig11()
                        {
                            Unk00 = 0f;
                            Unk04 = 0f;
                            Unk08 = 0f;
                            Unk0C = 0f;
                            Unk10 = 0f;
                            Unk14 = 0f;
                            Unk18 = 0f;
                            Unk1C = 0f;
                            Unk20 = 0f;
                            Unk24 = 0;
                            Unk28 = 0f;
                            Unk2C = 0f;
                            Unk30 = 0f;
                            Unk34 = 0f;
                        }

                        public UnkSceneConfig11 DeepCopy()
                        {
                            return (UnkSceneConfig11)MemberwiseClone();
                        }

                        internal UnkSceneConfig11(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadSingle();
                            Unk04 = br.ReadSingle();
                            Unk08 = br.ReadSingle();
                            Unk0C = br.ReadSingle();
                            Unk10 = br.ReadSingle();
                            Unk14 = br.ReadSingle();
                            Unk18 = br.ReadSingle();
                            Unk1C = br.ReadSingle();
                            Unk20 = br.ReadSingle();
                            Unk24 = br.ReadInt32();
                            Unk28 = br.ReadSingle();
                            Unk2C = br.ReadSingle();
                            Unk30 = br.ReadSingle();
                            Unk34 = br.ReadSingle();
                            br.AssertPattern(456, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteSingle(Unk00);
                            bw.WriteSingle(Unk04);
                            bw.WriteSingle(Unk08);
                            bw.WriteSingle(Unk0C);
                            bw.WriteSingle(Unk10);
                            bw.WriteSingle(Unk14);
                            bw.WriteSingle(Unk18);
                            bw.WriteSingle(Unk1C);
                            bw.WriteSingle(Unk20);
                            bw.WriteInt32(Unk24);
                            bw.WriteSingle(Unk28);
                            bw.WriteSingle(Unk2C);
                            bw.WriteSingle(Unk30);
                            bw.WriteSingle(Unk34);
                            bw.WritePattern(456, 0);
                        }
                    }

                    public class UnkSceneConfig12
                    {
                        public int Unk00 { get; set; }
                        public int Unk04 { get; set; }
                        public int Unk08 { get; set; }
                        public int Unk0C { get; set; }
                        public int Unk10 { get; set; }
                        public int Unk14 { get; set; }
                        public int Unk18 { get; set; }
                        public int Unk1C { get; set; }
                        public int Unk20 { get; set; }
                        public int Unk30 { get; set; }
                        public int Unk34 { get; set; }
                        public int Unk38 { get; set; }
                        public int Unk3C { get; set; }
                        public int Unk40 { get; set; }
                        public int Unk44 { get; set; }
                        public int Unk48 { get; set; }
                        public int Unk4C { get; set; }
                        public int Unk50 { get; set; }
                        public int Unk54 { get; set; }
                        public float Unk58 { get; set; }
                        public float Unk5C { get; set; }
                        public float Unk60 { get; set; }
                        public float Unk64 { get; set; }
                        public float Unk68 { get; set; }
                        public float Unk6C { get; set; }
                        public float Unk70 { get; set; }

                        public UnkSceneConfig12()
                        {
                            Unk00 = 0;
                            Unk04 = 0;
                            Unk08 = 0;
                            Unk0C = 0;
                            Unk10 = 0;
                            Unk14 = 0;
                            Unk18 = 0;
                            Unk1C = 0;
                            Unk20 = 0;
                            Unk30 = 0;
                            Unk34 = 0;
                            Unk38 = 0;
                            Unk3C = 0;
                            Unk40 = 0;
                            Unk44 = 0;
                            Unk48 = 0;
                            Unk4C = 0;
                            Unk50 = 0;
                            Unk54 = 1;
                            Unk58 = 0.6f;
                            Unk5C = 0.6f;
                            Unk60 = 0.6f;
                            Unk64 = 0.6f;
                            Unk68 = 0.6f;
                            Unk6C = 0.6f;
                            Unk70 = 0f;
                        }

                        public UnkSceneConfig12 DeepCopy()
                        {
                            return (UnkSceneConfig12)MemberwiseClone();
                        }

                        internal UnkSceneConfig12(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadInt32();
                            Unk04 = br.ReadInt32();
                            Unk08 = br.ReadInt32();
                            Unk0C = br.ReadInt32();
                            Unk10 = br.ReadInt32();
                            Unk14 = br.ReadInt32();
                            Unk18 = br.ReadInt32();
                            Unk1C = br.ReadInt32();
                            Unk20 = br.ReadInt32();
                            Unk30 = br.ReadInt32();
                            Unk34 = br.ReadInt32();
                            Unk38 = br.ReadInt32();
                            Unk3C = br.ReadInt32();
                            Unk40 = br.ReadInt32();
                            Unk44 = br.ReadInt32();
                            Unk48 = br.ReadInt32();
                            Unk4C = br.ReadInt32();
                            Unk50 = br.ReadInt32();
                            Unk54 = br.ReadInt32();
                            Unk58 = br.ReadSingle();
                            Unk5C = br.ReadSingle();
                            Unk60 = br.ReadSingle();
                            Unk64 = br.ReadSingle();
                            Unk68 = br.ReadSingle();
                            Unk6C = br.ReadSingle();
                            Unk70 = br.ReadSingle();
                            br.AssertPattern(408, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteInt32(Unk00);
                            bw.WriteInt32(Unk04);
                            bw.WriteInt32(Unk08);
                            bw.WriteInt32(Unk0C);
                            bw.WriteInt32(Unk10);
                            bw.WriteInt32(Unk14);
                            bw.WriteInt32(Unk18);
                            bw.WriteInt32(Unk1C);
                            bw.WriteInt32(Unk20);
                            bw.WriteInt32(Unk30);
                            bw.WriteInt32(Unk34);
                            bw.WriteInt32(Unk38);
                            bw.WriteInt32(Unk3C);
                            bw.WriteInt32(Unk40);
                            bw.WriteInt32(Unk44);
                            bw.WriteInt32(Unk48);
                            bw.WriteInt32(Unk4C);
                            bw.WriteInt32(Unk50);
                            bw.WriteInt32(Unk54);
                            bw.WriteSingle(Unk58);
                            bw.WriteSingle(Unk5C);
                            bw.WriteSingle(Unk60);
                            bw.WriteSingle(Unk64);
                            bw.WriteSingle(Unk68);
                            bw.WriteSingle(Unk6C);
                            bw.WriteSingle(Unk70);
                            bw.WritePattern(408, 0);
                        }
                    }

                    public class UnkSceneConfig13
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

                        public UnkSceneConfig13()
                        {
                            Unk00 = 511;
                            Unk02 = 15400;
                            Unk04 = 0.5f;
                            Unk08 = 0.35f;
                            Unk0C = 0.35f;
                            Unk10 = -0.03f;
                            Unk14 = -0.01f;
                            Unk18 = -0.006f;
                            Unk1C = 1287;
                        }

                        public UnkSceneConfig13 DeepCopy()
                        {
                            return (UnkSceneConfig13)MemberwiseClone();
                        }

                        internal UnkSceneConfig13(BinaryReaderEx br)
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
                            br.AssertPattern(482, 0);
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
                            bw.WritePattern(482, 0);
                        }
                    }

                    public class UnkSceneConfig14
                    {
                        public float Unk00 { get; set; }
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

                        public UnkSceneConfig14()
                        {
                            Unk00 = 0;
                            Unk04 = 0;
                            Unk08 = 0;
                            Unk0C = 0;
                            Unk10 = 0;
                            Unk14 = 0;
                            Unk18 = 0;
                            Unk1C = 0;
                            Unk20 = 0;
                            Unk24 = 0;
                            Unk28 = 0;
                            Unk2C = 0;
                            Unk30 = 1;
                        }

                        public UnkSceneConfig14 DeepCopy()
                        {
                            return (UnkSceneConfig14)MemberwiseClone();
                        }

                        internal UnkSceneConfig14(BinaryReaderEx br)
                        {
                            Unk00 = br.ReadSingle();
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
                            br.AssertPattern(12, 0);
                        }

                        internal void Write(BinaryWriterEx bw)
                        {
                            bw.WriteSingle(Unk00);
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
                            bw.WritePattern(12, 0);
                        }
                    }

                    #endregion
                }

                #endregion
            }

            public class BGM : Event
            {
                private protected override EventType Type => EventType.BGM;
                private protected override bool HasTypeData => true;

                public BGMConfig Config { get; private set; }

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
                    /// Unknown.
                    /// </summary>
                    public int Unk04 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk08 { get; set; }

                    public BGMConfig()
                    {
                        BGMID = -1;
                        Unk04 = 0;
                        Unk08 = 0;
                    }

                    public BGMConfig(int bgmID)
                    {
                        BGMID = bgmID;
                        Unk04 = 0;
                        Unk08 = 0;
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
                        Unk04 = br.ReadInt32();
                        Unk08 = br.ReadInt32();
                        br.AssertPattern(52, 0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(BGMID);
                        bw.WriteInt32(Unk04);
                        bw.WriteInt32(Unk08);
                        bw.WritePattern(52, 0);
                    }
                }

                #endregion
            }

            public class Rev : Event
            {
                private protected override EventType Type => EventType.Rev;
                private protected override bool HasTypeData => true;

                public RevConfig Config { get; set; }

                public Rev() : base("Rev")
                {
                    Config = new RevConfig();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var rev = (Rev)evnt;
                    rev.Config = Config.DeepCopy();
                }

                internal Rev(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Config = new RevConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Config.Write(bw);

                #region Sub Structs

                public class RevConfig
                {
                    public byte Unk00 { get; set; }
                    public byte Unk01 { get; set; }
                    public byte Unk02 { get; set; }
                    public byte Unk03 { get; set; }

                    public RevConfig()
                    {
                        Unk00 = 1;
                        Unk01 = 0;
                        Unk02 = 0;
                        Unk03 = 0;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public RevConfig DeepCopy()
                    {
                        var revConfig = (RevConfig)MemberwiseClone();
                        return revConfig;
                    }

                    internal RevConfig(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadByte();
                        Unk01 = br.ReadByte();
                        Unk02 = br.ReadByte();
                        Unk03 = br.ReadByte();
                        br.AssertPattern(60, 0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteByte(Unk00);
                        bw.WriteByte(Unk01);
                        bw.WriteByte(Unk02);
                        bw.WriteByte(Unk03);
                        bw.WritePattern(60, 0);
                    }
                }

                #endregion
            }

            public class SFX : Event
            {
                private protected override EventType Type => EventType.SFX;
                private protected override bool HasTypeData => true;

                public SFXConfig Config { get; set; }

                public SFX() : base("SFX")
                {
                    Config = new SFXConfig();
                }

                private protected override void DeepCopyTo(Event evnt)
                {
                    var sfx = (SFX)evnt;
                    sfx.Config = Config.DeepCopy();
                }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Config = new SFXConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Config.Write(bw);

                #region Sub Structs

                public class SFXConfig
                {
                    public int Unk00 { get; set; }
                    public int Unk04 { get; set; }
                    public int Unk08 { get; set; }
                    public int Unk0C { get; set; }

                    public SFXConfig()
                    {
                        Unk00 = 0;
                        Unk04 = 0;
                        Unk08 = 0;
                        Unk0C = 0;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public SFXConfig DeepCopy()
                    {
                        var sfxConfig = (SFXConfig)MemberwiseClone();
                        return sfxConfig;
                    }

                    internal SFXConfig(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadInt32();
                        Unk04 = br.ReadInt32();
                        Unk08 = br.ReadInt32();
                        Unk0C = br.ReadInt32();
                        br.AssertPattern(48, 0);
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteInt32(Unk00);
                        bw.WriteInt32(Unk04);
                        bw.WriteInt32(Unk08);
                        bw.WriteInt32(Unk0C);
                        bw.WritePattern(48, 0);
                    }
                }

                #endregion
            }

            #endregion
        }

        #endregion
    }
}
