using System;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class TAE3
    {
        /// <summary>
        /// Determines the behavior of an event and what data it contains.
        /// </summary>
        public enum EventType : ulong
        {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            JumpTable = 000,
            Unk001 = 001,
            Unk002 = 002,
            Unk005 = 005,
            Unk016 = 016,
            Unk017 = 017,
            Unk024 = 024,
            SwitchWeapon1 = 032,
            SwitchWeapon2 = 033,
            Unk034 = 034,
            Unk035 = 035,
            Unk064 = 064,
            Unk065 = 065,
            CreateSpEffect1 = 066,
            CreateSpEffect2 = 067,
            PlayFFX = 096,
            Unk110 = 110,
            HitEffect = 112,
            Unk113 = 113,
            Unk114 = 114,
            Unk115 = 115,
            Unk116 = 116,
            Unk117 = 117,
            Unk118 = 118,
            Unk119 = 119,
            Unk120 = 120,
            Unk121 = 121,
            PlaySound1 = 128,
            PlaySound2 = 129,
            PlaySound3 = 130,
            PlaySound4 = 131,
            PlaySound5 = 132,
            Unk136 = 136,
            Unk137 = 137,
            CreateDecal = 138,
            Unk144 = 144,
            Unk145 = 145,
            Unk150 = 150,
            Unk151 = 151,
            Unk161 = 161,
            Unk192 = 192,
            FadeOut = 193,
            Unk194 = 194,
            Unk224 = 224,
            DisableStaminaRegen = 225,
            Unk226 = 226,
            Unk227 = 227,
            RagdollReviveTime = 228,
            Unk229 = 229,
            SetEventMessageID = 231,
            Unk232 = 232,
            ChangeDrawMask = 233,
            RollDistanceReduction = 236,
            CreateAISound = 237,
            Unk300 = 300,
            Unk301 = 301,
            AddSpEffectDragonForm = 302,
            PlayAnimation = 303,
            BehaviorThing = 304,
            Unk306 = 306,
            CreateBehaviorPC = 307,
            Unk308 = 308,
            Unk310 = 310,
            Unk311 = 311,
            Unk312 = 312,
            Unk317 = 317,
            Unk320 = 320,
            Unk330 = 330,
            EffectDuringThrow = 331,
            Unk332 = 332,
            CreateSpEffect = 401,
            Unk500 = 500,
            Unk510 = 510,
            Unk520 = 520,
            KingOfTheStorm = 522,
            Unk600 = 600,
            Unk601 = 601,
            DebugAnimSpeed = 603,
            Unk605 = 605,
            Unk606 = 606,
            Unk700 = 700,
            EnableTurningDirection = 703,
            FacingAngleCorrection = 705,
            Unk707 = 707,
            HideWeapon = 710,
            HideModelMask = 711,
            DamageLevelModule = 712,
            ModelMask = 713,
            DamageLevelFunction = 714,
            Unk715 = 715,
            CultStart = 720,
            Unk730 = 730,
            Unk740 = 740,
            IFrameState = 760,
            BonePos = 770,
            BoneFixOn1 = 771,
            BoneFixOn2 = 772,
            TurnLowerBody = 781,
            Unk782 = 782,
            SpawnBulletByCultSacrifice1 = 785,
            Unk786 = 786,
            Unk790 = 790,
            Unk791 = 791,
            HitEffect2 = 792,
            CultSacrifice1 = 793,
            SacrificeEmpty = 794,
            Toughness = 795,
            BringCultMenu = 796,
            CeremonyParamID = 797,
            CultSingle = 798,
            CultEmpty2 = 799,
            Unk800 = 800,
            Unk900 = 900,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        /// <summary>
        /// An action or effect triggered at a certain time during an animation.
        /// </summary>
        public abstract class Event
        {
            /// <summary>
            /// The type of this event.
            /// </summary>
            public abstract EventType Type { get; }

            /// <summary>
            /// When the event begins.
            /// </summary>
            public float StartTime;

            /// <summary>
            /// When the event ends.
            /// </summary>
            public float EndTime;

            internal Event(float startTime, float endTime)
            {
                StartTime = startTime;
                EndTime = endTime;
            }

            internal void WriteHeader(BinaryWriterEx bw, int animIndex, int eventIndex, Dictionary<float, long> timeOffsets)
            {
                bw.WriteInt64(timeOffsets[StartTime]);
                bw.WriteInt64(timeOffsets[EndTime]);
                bw.ReserveInt64($"EventDataOffset{animIndex}:{eventIndex}");
            }

            internal void WriteData(BinaryWriterEx bw, int animIndex, int eventIndex)
            {
                bw.FillInt64($"EventDataOffset{animIndex}:{eventIndex}", bw.Position);
                bw.WriteUInt64((ulong)Type);
                bw.WriteInt64(bw.Position + 8);
                WriteSpecific(bw);
                bw.Pad(0x10);
            }

            internal abstract void WriteSpecific(BinaryWriterEx bw);

            /// <summary>
            /// Returns the start time, end time, and type of the event.
            /// </summary>
            public override string ToString()
            {
                return $"{(int)Math.Round(StartTime * 30):D3} - {(int)Math.Round(EndTime * 30):D3} {Type}";
            }

            internal static Event Read(BinaryReaderEx br)
            {
                long startTimeOffset = br.ReadInt64();
                long endTimeOffset = br.ReadInt64();
                long eventDataOffset = br.ReadInt64();
                float startTime = br.GetSingle(startTimeOffset);
                float endTime = br.GetSingle(endTimeOffset);

                Event result;
                br.StepIn(eventDataOffset);
                {
                    EventType type = br.ReadEnum64<EventType>();
                    br.AssertInt64(br.Position + 8);
                    switch (type)
                    {
                        case EventType.JumpTable: result = new JumpTable(startTime, endTime, br); break;
                        case EventType.Unk001: result = new Unk001(startTime, endTime, br); break;
                        case EventType.Unk002: result = new Unk002(startTime, endTime, br); break;
                        case EventType.Unk005: result = new Unk005(startTime, endTime, br); break;
                        case EventType.Unk016: result = new Unk016(startTime, endTime, br); break;
                        case EventType.Unk017: result = new Unk017(startTime, endTime, br); break;
                        case EventType.Unk024: result = new Unk024(startTime, endTime, br); break;
                        case EventType.SwitchWeapon1: result = new SwitchWeapon1(startTime, endTime, br); break;
                        case EventType.SwitchWeapon2: result = new SwitchWeapon2(startTime, endTime, br); break;
                        case EventType.Unk034: result = new Unk034(startTime, endTime, br); break;
                        case EventType.Unk035: result = new Unk035(startTime, endTime, br); break;
                        case EventType.Unk064: result = new Unk064(startTime, endTime, br); break;
                        case EventType.Unk065: result = new Unk065(startTime, endTime, br); break;
                        case EventType.CreateSpEffect1: result = new CreateSpEffect1(startTime, endTime, br); break;
                        case EventType.CreateSpEffect2: result = new CreateSpEffect2(startTime, endTime, br); break;
                        case EventType.PlayFFX: result = new PlayFFX(startTime, endTime, br); break;
                        case EventType.Unk110: result = new Unk110(startTime, endTime, br); break;
                        case EventType.HitEffect: result = new HitEffect(startTime, endTime, br); break;
                        case EventType.Unk113: result = new Unk113(startTime, endTime, br); break;
                        case EventType.Unk114: result = new Unk114(startTime, endTime, br); break;
                        case EventType.Unk115: result = new Unk115(startTime, endTime, br); break;
                        case EventType.Unk116: result = new Unk116(startTime, endTime, br); break;
                        case EventType.Unk117: result = new Unk117(startTime, endTime, br); break;
                        case EventType.Unk118: result = new Unk118(startTime, endTime, br); break;
                        case EventType.Unk119: result = new Unk119(startTime, endTime, br); break;
                        case EventType.Unk120: result = new Unk120(startTime, endTime, br); break;
                        case EventType.Unk121: result = new Unk121(startTime, endTime, br); break;
                        case EventType.PlaySound1: result = new PlaySound1(startTime, endTime, br); break;
                        case EventType.PlaySound2: result = new PlaySound2(startTime, endTime, br); break;
                        case EventType.PlaySound3: result = new PlaySound3(startTime, endTime, br); break;
                        case EventType.PlaySound4: result = new PlaySound4(startTime, endTime, br); break;
                        case EventType.PlaySound5: result = new PlaySound5(startTime, endTime, br); break;
                        case EventType.Unk137: result = new Unk137(startTime, endTime, br); break;
                        case EventType.CreateDecal: result = new CreateDecal(startTime, endTime, br); break;
                        case EventType.Unk144: result = new Unk144(startTime, endTime, br); break;
                        case EventType.Unk145: result = new Unk145(startTime, endTime, br); break;
                        case EventType.Unk150: result = new Unk150(startTime, endTime, br); break;
                        case EventType.Unk151: result = new Unk151(startTime, endTime, br); break;
                        case EventType.Unk161: result = new Unk161(startTime, endTime, br); break;
                        case EventType.FadeOut: result = new FadeOut(startTime, endTime, br); break;
                        case EventType.Unk194: result = new Unk194(startTime, endTime, br); break;
                        case EventType.Unk224: result = new Unk224(startTime, endTime, br); break;
                        case EventType.DisableStaminaRegen: result = new DisableStaminaRegen(startTime, endTime, br); break;
                        case EventType.Unk226: result = new Unk226(startTime, endTime, br); break;
                        case EventType.Unk227: result = new Unk227(startTime, endTime, br); break;
                        case EventType.RagdollReviveTime: result = new RagdollReviveTime(startTime, endTime, br); break;
                        case EventType.Unk229: result = new Unk229(startTime, endTime, br); break;
                        case EventType.SetEventMessageID: result = new SetEventMessageID(startTime, endTime, br); break;
                        case EventType.Unk232: result = new Unk232(startTime, endTime, br); break;
                        case EventType.ChangeDrawMask: result = new ChangeDrawMask(startTime, endTime, br); break;
                        case EventType.RollDistanceReduction: result = new RollDistanceReduction(startTime, endTime, br); break;
                        case EventType.CreateAISound: result = new CreateAISound(startTime, endTime, br); break;
                        case EventType.Unk300: result = new Unk300(startTime, endTime, br); break;
                        case EventType.Unk301: result = new Unk301(startTime, endTime, br); break;
                        case EventType.AddSpEffectDragonForm: result = new AddSpEffectDragonForm(startTime, endTime, br); break;
                        case EventType.PlayAnimation: result = new PlayAnimation(startTime, endTime, br); break;
                        case EventType.BehaviorThing: result = new BehaviorThing(startTime, endTime, br); break;
                        case EventType.CreateBehaviorPC: result = new CreateBehaviorPC(startTime, endTime, br); break;
                        case EventType.Unk308: result = new Unk308(startTime, endTime, br); break;
                        case EventType.Unk310: result = new Unk310(startTime, endTime, br); break;
                        case EventType.Unk311: result = new Unk311(startTime, endTime, br); break;
                        case EventType.Unk312: result = new Unk312(startTime, endTime, br); break;
                        case EventType.Unk320: result = new Unk320(startTime, endTime, br); break;
                        case EventType.Unk330: result = new Unk330(startTime, endTime, br); break;
                        case EventType.EffectDuringThrow: result = new EffectDuringThrow(startTime, endTime, br); break;
                        case EventType.Unk332: result = new Unk332(startTime, endTime, br); break;
                        case EventType.CreateSpEffect: result = new CreateSpEffect(startTime, endTime, br); break;
                        case EventType.Unk500: result = new Unk500(startTime, endTime, br); break;
                        case EventType.Unk510: result = new Unk510(startTime, endTime, br); break;
                        case EventType.Unk520: result = new Unk520(startTime, endTime, br); break;
                        case EventType.KingOfTheStorm: result = new KingOfTheStorm(startTime, endTime, br); break;
                        case EventType.Unk600: result = new Unk600(startTime, endTime, br); break;
                        case EventType.Unk601: result = new Unk601(startTime, endTime, br); break;
                        case EventType.DebugAnimSpeed: result = new DebugAnimSpeed(startTime, endTime, br); break;
                        case EventType.Unk605: result = new Unk605(startTime, endTime, br); break;
                        case EventType.Unk606: result = new Unk606(startTime, endTime, br); break;
                        case EventType.Unk700: result = new Unk700(startTime, endTime, br); break;
                        case EventType.EnableTurningDirection: result = new EnableTurningDirection(startTime, endTime, br); break;
                        case EventType.FacingAngleCorrection: result = new FacingAngleCorrection(startTime, endTime, br); break;
                        case EventType.Unk707: result = new Unk707(startTime, endTime, br); break;
                        case EventType.HideWeapon: result = new HideWeapon(startTime, endTime, br); break;
                        case EventType.HideModelMask: result = new HideModelMask(startTime, endTime, br); break;
                        case EventType.DamageLevelModule: result = new DamageLevelModule(startTime, endTime, br); break;
                        case EventType.ModelMask: result = new ModelMask(startTime, endTime, br); break;
                        case EventType.DamageLevelFunction: result = new DamageLevelFunction(startTime, endTime, br); break;
                        case EventType.Unk715: result = new Unk715(startTime, endTime, br); break;
                        case EventType.CultStart: result = new CultStart(startTime, endTime, br); break;
                        case EventType.Unk730: result = new Unk730(startTime, endTime, br); break;
                        case EventType.Unk740: result = new Unk740(startTime, endTime, br); break;
                        case EventType.IFrameState: result = new IFrameState(startTime, endTime, br); break;
                        case EventType.BonePos: result = new BonePos(startTime, endTime, br); break;
                        case EventType.BoneFixOn1: result = new BoneFixOn1(startTime, endTime, br); break;
                        case EventType.BoneFixOn2: result = new BoneFixOn2(startTime, endTime, br); break;
                        case EventType.TurnLowerBody: result = new TurnLowerBody(startTime, endTime, br); break;
                        case EventType.Unk782: result = new Unk782(startTime, endTime, br); break;
                        case EventType.SpawnBulletByCultSacrifice1: result = new SpawnBulletByCultSacrifice1(startTime, endTime, br); break;
                        case EventType.Unk786: result = new Unk786(startTime, endTime, br); break;
                        case EventType.Unk790: result = new Unk790(startTime, endTime, br); break;
                        case EventType.Unk791: result = new Unk791(startTime, endTime, br); break;
                        case EventType.HitEffect2: result = new HitEffect2(startTime, endTime, br); break;
                        case EventType.CultSacrifice1: result = new CultSacrifice1(startTime, endTime, br); break;
                        case EventType.SacrificeEmpty: result = new SacrificeEmpty(startTime, endTime, br); break;
                        case EventType.Toughness: result = new Toughness(startTime, endTime, br); break;
                        case EventType.BringCultMenu: result = new BringCultMenu(startTime, endTime, br); break;
                        case EventType.CeremonyParamID: result = new CeremonyParamID(startTime, endTime, br); break;
                        case EventType.CultSingle: result = new CultSingle(startTime, endTime, br); break;
                        case EventType.CultEmpty2: result = new CultEmpty2(startTime, endTime, br); break;
                        case EventType.Unk800: result = new Unk800(startTime, endTime, br); break;

                        default:
                            throw new NotImplementedException();
                    }

                    if (result.Type != type)
                    {
                        throw new InvalidProgramException("There is a typo in TAE3.Event.cs. Please bully me.");
                    }
                }
                br.StepOut();

                return result;
            }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            /// <summary>
            /// General-purpose event that calls different functions based on the first field.
            /// </summary>
            public class JumpTable : Event // 000
            {
                public override EventType Type => EventType.JumpTable;

                public int JumpTableID { get; set; }
                public int Unk04 { get; set; }
                // Used for jump table ID 3
                public int Unk08 { get; set; }
                public short Unk0C { get; set; }
                public short Unk0E { get; set; }

                internal JumpTable(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    JumpTableID = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt16();
                    Unk0E = br.ReadInt16();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(JumpTableID);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt16(Unk0C);
                    bw.WriteInt16(Unk0E);
                }

                public override string ToString()
                {
                    return $"{base.ToString()} : {JumpTableID}";
                }
            }

            public class Unk001 : Event // 001
            {
                public override EventType Type => EventType.Unk001;

                public int Unk00 { get; set; }
                public int Unk04 { get; set; }
                public int Condition { get; set; }
                public byte Unk0C { get; set; }
                public byte Unk0D { get; set; }
                public short StateInfo { get; set; }

                public Unk001(float startTime, float endTime, int unk00, int unk04, int condition, byte unk0C, byte unk0D, short stateInfo) : base(startTime, endTime)
                {
                    Unk00 = unk00;
                    Unk04 = unk04;
                    Condition = condition;
                    Unk0C = unk0C;
                    Unk0D = unk0D;
                    StateInfo = stateInfo;
                }

                internal Unk001(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Condition = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    StateInfo = br.ReadInt16();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Condition);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteInt16(StateInfo);
                }
            }

            public class Unk002 : Event // 002
            {
                public override EventType Type => EventType.Unk002;

                public int Unk00;
                public int Unk04;
                public int ChrAsmStyle;
                public byte Unk0C;
                public byte Unk0D;
                public ushort Unk0E;
                public ushort Unk10;

                internal Unk002(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    ChrAsmStyle = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadUInt16();
                    Unk10 = br.ReadUInt16();
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(ChrAsmStyle);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteUInt16(Unk0E);
                    bw.WriteUInt16(Unk10);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk005 : Event // 005
            {
                public override EventType Type => EventType.Unk005;

                public int Unk00;
                public int Unk04;

                internal Unk005(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class Unk016 : Event // 016
            {
                public override EventType Type => EventType.Unk016;

                internal Unk016(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime) { }

                internal override void WriteSpecific(BinaryWriterEx bw) { }
            }

            public class Unk017 : Event // 017
            {
                public override EventType Type => EventType.Unk017;

                internal Unk017(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk024 : Event // 024
            {
                public override EventType Type => EventType.Unk024;

                public int Unk00;
                public int Unk04;
                public int Unk08;
                public int Unk0C;

                internal Unk024(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class SwitchWeapon1 : Event // 032
            {
                public override EventType Type => EventType.SwitchWeapon1;

                public int SwitchState;

                internal SwitchWeapon1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SwitchState = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SwitchState);
                    bw.WriteInt32(0);
                }
            }

            public class SwitchWeapon2 : Event // 033
            {
                public override EventType Type => EventType.SwitchWeapon2;

                public int SwitchState;

                internal SwitchWeapon2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SwitchState = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SwitchState);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk034 : Event // 034
            {
                public override EventType Type => EventType.Unk034;

                public int State;

                internal Unk034(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    State = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk035 : Event // 035
            {
                public override EventType Type => EventType.Unk035;

                public int State;

                internal Unk035(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    State = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(State);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk064 : Event // 064
            {
                public override EventType Type => EventType.Unk064;

                public int Unk00;
                public ushort Unk04;
                public ushort Unk06;
                public byte Unk08;
                public byte Unk09;
                public byte Unk0A;
                public byte Unk0B;

                internal Unk064(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadUInt16();
                    Unk06 = br.ReadUInt16();
                    Unk08 = br.ReadByte();
                    Unk09 = br.ReadByte();
                    Unk0A = br.ReadByte();
                    Unk0B = br.ReadByte();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteUInt16(Unk04);
                    bw.WriteUInt16(Unk06);
                    bw.WriteByte(Unk08);
                    bw.WriteByte(Unk09);
                    bw.WriteByte(Unk0A);
                    bw.WriteByte(Unk0B);
                    bw.WriteInt32(0);
                }
            }

            public class Unk065 : Event // 065
            {
                public override EventType Type => EventType.Unk065;

                public int Unk00;
                public byte Unk04;
                public byte Unk05;
                public ushort Unk06;
                public int Unk08;

                internal Unk065(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadByte();
                    Unk05 = br.ReadByte();
                    Unk06 = br.ReadUInt16();
                    Unk08 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteByte(Unk04);
                    bw.WriteByte(Unk05);
                    bw.WriteUInt16(Unk06);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(0);
                }
            }

            // During attack
            public class CreateSpEffect1 : Event // 066
            {
                public override EventType Type => EventType.CreateSpEffect1;

                public int SpEffectID;

                public CreateSpEffect1(float startTime, float endTime, int speffectID) : base(startTime, endTime)
                {
                    SpEffectID = speffectID;
                }

                internal CreateSpEffect1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SpEffectID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SpEffectID);
                    bw.WriteInt32(0);
                }
            }

            // During roll
            public class CreateSpEffect2 : Event // 067
            {
                public override EventType Type => EventType.CreateSpEffect2;

                public int SpEffectID;

                internal CreateSpEffect2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SpEffectID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SpEffectID);
                    bw.WriteInt32(0);
                }
            }

            public class PlayFFX : Event // 096
            {
                public override EventType Type => EventType.PlayFFX;

                public int FFXID;
                public int Unk04;
                public int Unk08;
                public sbyte State0;
                public sbyte State1;
                public sbyte GhostFFXCondition;
                public byte Unk0F;

                internal PlayFFX(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    FFXID = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    State0 = br.ReadSByte();
                    State1 = br.ReadSByte();
                    GhostFFXCondition = br.ReadSByte();
                    Unk0F = br.ReadByte();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(FFXID);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteSByte(State0);
                    bw.WriteSByte(State1);
                    bw.WriteSByte(GhostFFXCondition);
                    bw.WriteByte(Unk0F);
                }
            }

            public class Unk110 : Event // 110
            {
                public override EventType Type => EventType.Unk110;

                public int ID;

                internal Unk110(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    ID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ID);
                    bw.WriteInt32(0);
                }
            }

            public class HitEffect : Event // 112
            {
                public override EventType Type => EventType.HitEffect;

                public int Size;
                public int Unk04;
                public int Unk08;

                internal HitEffect(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Size = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Size);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class Unk113 : Event // 113
            {
                public override EventType Type => EventType.Unk113;

                internal Unk113(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk114 : Event // 114
            {
                public override EventType Type => EventType.Unk114;

                public int Unk00;
                public ushort Unk04;
                public ushort Unk06;
                public int Unk08;
                public byte Unk0C;
                public sbyte Unk0D;
                public sbyte Unk0E;
                public byte Unk0F;
                public byte Unk10;
                public byte Unk11;
                public short Unk12;

                internal Unk114(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadUInt16();
                    Unk06 = br.ReadUInt16();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadSByte();
                    Unk0E = br.ReadSByte();
                    Unk0F = br.ReadByte();
                    Unk10 = br.ReadByte();
                    Unk11 = br.ReadByte();
                    Unk12 = br.ReadInt16();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteUInt16(Unk04);
                    bw.WriteUInt16(Unk06);
                    bw.WriteInt32(Unk08);
                    bw.WriteByte(Unk0C);
                    bw.WriteSByte(Unk0D);
                    bw.WriteSByte(Unk0E);
                    bw.WriteByte(Unk0F);
                    bw.WriteByte(Unk10);
                    bw.WriteByte(Unk11);
                    bw.WriteInt16(Unk12);
                    bw.WriteInt32(0);
                }
            }

            public class Unk115 : Event // 115
            {
                public override EventType Type => EventType.Unk115;

                public int Unk00;
                public ushort Unk04;
                public ushort Unk06;
                public int Unk08;
                public byte Unk0C;
                public byte Unk0D;
                public byte Unk0E;
                public byte Unk0F;
                public byte Unk10;
                public byte Unk11;
                public short Unk12;

                internal Unk115(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadUInt16();
                    Unk06 = br.ReadUInt16();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadByte();
                    Unk0F = br.ReadByte();
                    Unk10 = br.ReadByte();
                    Unk11 = br.ReadByte();
                    Unk12 = br.ReadInt16();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteUInt16(Unk04);
                    bw.WriteUInt16(Unk06);
                    bw.WriteInt32(Unk08);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteByte(Unk0E);
                    bw.WriteByte(Unk0F);
                    bw.WriteByte(Unk10);
                    bw.WriteByte(Unk11);
                    bw.WriteInt16(Unk12);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk116 : Event // 116
            {
                public override EventType Type => EventType.Unk116;

                public int Unk00;
                public int Unk04;
                public int Unk08;
                public int Unk0C;

                internal Unk116(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk117 : Event // 117
            {
                public override EventType Type => EventType.Unk117;

                public int Unk00;
                public int Unk04;
                public int Unk08;
                public byte Unk0C;
                public byte Unk0D;
                public byte Unk0E;
                public byte Unk0F;

                internal Unk117(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32(); // -1
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadByte();
                    Unk0F = br.ReadByte();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteByte(Unk0E);
                    bw.WriteByte(Unk0F);
                }
            }

            public class Unk118 : Event // 118
            {
                public override EventType Type => EventType.Unk118;

                public int Unk00;
                public ushort Unk04;
                public ushort Unk06;
                public ushort Unk08;
                public ushort Unk0A;

                internal Unk118(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadUInt16();
                    Unk06 = br.ReadUInt16();
                    Unk08 = br.ReadUInt16();
                    Unk0A = br.ReadUInt16();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteUInt16(Unk04);
                    bw.WriteUInt16(Unk06);
                    bw.WriteUInt16(Unk08);
                    bw.WriteUInt16(Unk0A);
                    bw.WriteInt32(0);
                }
            }

            public class Unk119 : Event // 119
            {
                public override EventType Type => EventType.Unk119;

                public int Unk00;
                public int Unk04;
                public int Unk08;
                public byte Unk0C;

                internal Unk119(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadByte(); // 0
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }
            }

            public class Unk120 : Event // 120
            {
                public override EventType Type => EventType.Unk120;

                public int ChrType;
                public int[] FFXIDs { get; private set; }
                public int Unk30;
                public int Unk34;
                public byte Unk38;

                internal Unk120(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    ChrType = br.ReadInt32();
                    FFXIDs = br.ReadInt32s(11);
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                    Unk38 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ChrType);
                    bw.WriteInt32s(FFXIDs);
                    bw.WriteInt32(Unk30);
                    bw.WriteInt32(Unk34);
                    bw.WriteByte(Unk38);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk121 : Event // 121
            {
                public override EventType Type => EventType.Unk121;

                public int Unk00;
                public ushort Unk04;
                public byte Unk06;
                public byte Unk07;

                internal Unk121(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadUInt16();
                    Unk06 = br.ReadByte();
                    Unk07 = br.ReadByte();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteUInt16(Unk04);
                    bw.WriteByte(Unk06);
                    bw.WriteByte(Unk07);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class PlaySound1 : Event // 128
            {
                public override EventType Type => EventType.PlaySound1;

                public int SoundType;
                public int SoundID;

                internal PlaySound1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    // After event version 0x10?
                    //br.AssertInt32(0);
                    //br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    //bw.WriteInt32(0);
                    //bw.WriteInt32(0);
                }
            }

            public class PlaySound2 : Event // 129
            {
                public override EventType Type => EventType.PlaySound2;

                public int SoundType;
                public int SoundID;
                public int Unk08;
                public int Unk0C;
                public int Unk10;

                internal PlaySound2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    // After event version 0x15?
                    Unk10 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(0);
                }
            }

            public class PlaySound3 : Event // 130
            {
                public override EventType Type => EventType.PlaySound3;

                public int SoundType;
                public int SoundID;
                public float Unk08;
                public float Unk0C;

                internal PlaySound3(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadSingle(); // int -1
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteSingle(Unk08);
                    bw.WriteSingle(Unk0C);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class PlaySound4 : Event // 131
            {
                public override EventType Type => EventType.PlaySound4;

                public int SoundType;
                public int SoundID;
                public int Unk08;

                internal PlaySound4(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class PlaySound5 : Event // 132
            {
                public override EventType Type => EventType.PlaySound5;

                public int SoundType;
                public int SoundID;

                internal PlaySound5(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk137 : Event // 137
            {
                public override EventType Type => EventType.Unk137;

                public int Unk00;

                internal Unk137(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class CreateDecal : Event // 138
            {
                public override EventType Type => EventType.CreateDecal;

                public int DecalParamID, Unk04;

                internal CreateDecal(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    DecalParamID = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(DecalParamID);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk144 : Event // 144
            {
                public override EventType Type => EventType.Unk144;

                public ushort Unk00;
                public ushort Unk02;
                public float Unk04;
                public float Unk08;

                internal Unk144(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadUInt16();
                    Unk02 = br.ReadUInt16();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteUInt16(Unk00);
                    bw.WriteUInt16(Unk02);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class Unk145 : Event // 145
            {
                public override EventType Type => EventType.Unk145;

                public short Unk00;
                public short Condition;

                internal Unk145(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt16();
                    Condition = br.ReadInt16();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Unk00);
                    bw.WriteInt16(Condition);
                    bw.WriteInt32(0);
                }
            }

            public class Unk150 : Event // 150
            {
                public override EventType Type => EventType.Unk150;

                public int Unk00;

                internal Unk150(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk151 : Event // 151
            {
                public override EventType Type => EventType.Unk151;

                public int DummyPointID;

                internal Unk151(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    DummyPointID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(DummyPointID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk161 : Event // 161
            {
                public override EventType Type => EventType.Unk161;

                internal Unk161(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class FadeOut : Event // 193
            {
                public override EventType Type => EventType.FadeOut;

                public float GhostVal1;
                public float GhostVal2;

                internal FadeOut(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    GhostVal1 = br.ReadSingle();
                    GhostVal2 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(GhostVal1);
                    bw.WriteSingle(GhostVal2);
                }
            }

            public class Unk194 : Event // 194
            {
                public override EventType Type => EventType.Unk194;

                public ushort Unk00;
                public ushort Unk02;
                public ushort Unk04;
                public ushort Unk06;
                public float Unk08;

                internal Unk194(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadUInt16();
                    Unk02 = br.ReadUInt16();
                    Unk04 = br.ReadUInt16();
                    Unk06 = br.ReadUInt16();
                    Unk08 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteUInt16(Unk00);
                    bw.WriteUInt16(Unk02);
                    bw.WriteUInt16(Unk04);
                    bw.WriteUInt16(Unk06);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class Unk224 : Event // 224
            {
                public override EventType Type => EventType.Unk224;

                public float Unk00;
                public int Unk04;

                internal Unk224(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            public class DisableStaminaRegen : Event // 225
            {
                public override EventType Type => EventType.DisableStaminaRegen;

                // "0x64 - Enables Regen Back" -Pav
                public byte State;

                internal DisableStaminaRegen(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    State = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(State);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk226 : Event // 226
            {
                public override EventType Type => EventType.Unk226;

                // "x/100 Coefficient" -Pav
                public byte State;

                internal Unk226(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    State = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(State);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk227 : Event // 227
            {
                public override EventType Type => EventType.Unk227;

                public int Mask;

                internal Unk227(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Mask = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Mask);
                    bw.WriteInt32(0);
                }
            }

            public class RagdollReviveTime : Event // 228
            {
                public override EventType Type => EventType.RagdollReviveTime;

                public float Unk00;
                public float ReviveTimer;

                internal RagdollReviveTime(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    ReviveTimer = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteSingle(ReviveTimer);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk229 : Event // 229
            {
                public override EventType Type => EventType.Unk229;

                public int Unk00;

                internal Unk229(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class SetEventMessageID : Event // 231
            {
                public override EventType Type => EventType.SetEventMessageID;

                public int EventMessageID;

                internal SetEventMessageID(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    EventMessageID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EventMessageID);
                    bw.WriteInt32(0);
                }
            }

            public class Unk232 : Event // 232
            {
                public override EventType Type => EventType.Unk232;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;

                internal Unk232(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteInt32(0);
                }
            }

            public class ChangeDrawMask : Event // 233
            {
                public override EventType Type => EventType.ChangeDrawMask;

                public byte[] DrawMask { get; private set; }

                internal ChangeDrawMask(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    DrawMask = br.ReadBytes(32);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBytes(DrawMask);
                }
            }

            public class RollDistanceReduction : Event // 236
            {
                public override EventType Type => EventType.RollDistanceReduction;

                public float Unk00;
                public float Unk04;
                public bool RollType;

                internal RollDistanceReduction(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadSingle();
                    RollType = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteBoolean(RollType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class CreateAISound : Event // 237
            {
                public override EventType Type => EventType.CreateAISound;

                public int AISoundID;

                internal CreateAISound(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    AISoundID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(AISoundID);
                    bw.WriteInt32(0);
                }
            }

            public class Unk300 : Event // 300
            {
                public override EventType Type => EventType.Unk300;

                public short JumpTableID1;
                public short JumpTableID2;
                public float Unk04;
                public float Unk08;
                public int Unk0C;

                internal Unk300(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    JumpTableID1 = br.ReadInt16();
                    JumpTableID2 = br.ReadInt16();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt16(JumpTableID1);
                    bw.WriteInt16(JumpTableID2);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            public class Unk301 : Event // 301
            {
                public override EventType Type => EventType.Unk301;

                public int Unk00;

                internal Unk301(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class AddSpEffectDragonForm : Event // 302
            {
                public override EventType Type => EventType.AddSpEffectDragonForm;

                public int SpEffectID;

                internal AddSpEffectDragonForm(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SpEffectID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SpEffectID);
                    bw.WriteInt32(0);
                }
            }

            public class PlayAnimation : Event // 303
            {
                public override EventType Type => EventType.PlayAnimation;

                public int AnimationID;

                internal PlayAnimation(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    AnimationID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(AnimationID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            // "Behavior Thing?" -Pav
            public class BehaviorThing : Event // 304
            {
                public override EventType Type => EventType.BehaviorThing;

                public ushort Unk00;
                public short Unk02;
                public int BehaviorListID;

                internal BehaviorThing(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadUInt16();
                    Unk02 = br.ReadInt16();
                    BehaviorListID = br.ReadInt32();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteUInt16(Unk00);
                    bw.WriteInt16(Unk02);
                    bw.WriteInt32(BehaviorListID);
                }
            }

            public class CreateBehaviorPC : Event // 307
            {
                public override EventType Type => EventType.CreateBehaviorPC;

                public short Unk00;
                public short Unk02;
                public int Condition;
                public int Unk08;

                internal CreateBehaviorPC(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                    Condition = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Unk00);
                    bw.WriteInt16(Unk02);
                    bw.WriteInt32(Condition);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(0);
                }
            }

            public class Unk308 : Event // 308
            {
                public override EventType Type => EventType.Unk308;

                public float Unk00;

                internal Unk308(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            // "Behavior?" -Pav
            public class Unk310 : Event // 310
            {
                public override EventType Type => EventType.Unk310;

                public byte Unk00;
                public byte Unk01;

                internal Unk310(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk311 : Event // 311
            {
                public override EventType Type => EventType.Unk311;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;

                internal Unk311(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk312 : Event // 312
            {
                public override EventType Type => EventType.Unk312;

                public byte[] BehaviorMask { get; private set; }

                internal Unk312(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    BehaviorMask = br.ReadBytes(32);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBytes(BehaviorMask);
                }
            }

            public class Unk320 : Event // 320
            {
                public override EventType Type => EventType.Unk320;

                public bool Unk00;
                public bool Unk01;
                public bool Unk02;
                public bool Unk03;
                public bool Unk04;
                public bool Unk05;
                public bool Unk06;

                internal Unk320(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadBoolean();
                    Unk01 = br.ReadBoolean();
                    Unk02 = br.ReadBoolean();
                    Unk03 = br.ReadBoolean();
                    Unk04 = br.ReadBoolean();
                    Unk05 = br.ReadBoolean();
                    Unk06 = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBoolean(Unk00);
                    bw.WriteBoolean(Unk01);
                    bw.WriteBoolean(Unk02);
                    bw.WriteBoolean(Unk03);
                    bw.WriteBoolean(Unk04);
                    bw.WriteBoolean(Unk05);
                    bw.WriteBoolean(Unk06);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk330 : Event // 330
            {
                public override EventType Type => EventType.Unk330;

                internal Unk330(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class EffectDuringThrow : Event // 331
            {
                public override EventType Type => EventType.EffectDuringThrow;

                public int SpEffectID1;
                public int SpEffectID2;

                internal EffectDuringThrow(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SpEffectID1 = br.ReadInt32();
                    SpEffectID2 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SpEffectID1);
                    bw.WriteInt32(SpEffectID2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk332 : Event // 332
            {
                public override EventType Type => EventType.Unk332;

                internal Unk332(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            // "When Landing" -Pav
            public class CreateSpEffect : Event // 401
            {
                public override EventType Type => EventType.CreateSpEffect;

                public int SpEffectID;

                public CreateSpEffect(float startTime, float endTime, int effectId) : base(startTime, endTime)
                {
                    SpEffectID = effectId;
                }

                internal CreateSpEffect(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SpEffectID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SpEffectID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk500 : Event // 500
            {
                public override EventType Type => EventType.Unk500;

                public byte Unk00;
                public byte Unk01;

                internal Unk500(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk510 : Event // 510
            {
                public override EventType Type => EventType.Unk510;

                internal Unk510(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk520 : Event // 520
            {
                public override EventType Type => EventType.Unk520;

                internal Unk520(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class KingOfTheStorm : Event // 522
            {
                public override EventType Type => EventType.KingOfTheStorm;

                public float Unk00;

                internal KingOfTheStorm(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle(); // 0
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk600 : Event // 600
            {
                public override EventType Type => EventType.Unk600;

                public int Mask;

                internal Unk600(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Mask = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Mask);
                    bw.WriteInt32(0);
                }
            }

            public class Unk601 : Event // 601
            {
                public override EventType Type => EventType.Unk601;

                public int StayAnimType;
                public float Unk04;
                public float Unk08;

                internal Unk601(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    StayAnimType = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(StayAnimType);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(0);
                }
            }

            // "TAE Debug Anim Speed" -Pav
            public class DebugAnimSpeed : Event // 603
            {
                public override EventType Type => EventType.DebugAnimSpeed;

                public uint AnimSpeed;

                internal DebugAnimSpeed(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    AnimSpeed = br.ReadUInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(AnimSpeed);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk605 : Event // 605
            {
                public override EventType Type => EventType.Unk605;

                public bool Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;
                public int Unk04;
                public float Unk08;
                public float Unk0C;

                internal Unk605(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadBoolean();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBoolean(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteInt32(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteSingle(Unk0C);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk606 : Event // 606
            {
                public override EventType Type => EventType.Unk606;

                public byte Unk00;
                public byte Unk04;
                public byte Unk06;

                internal Unk606(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadByte(); // 0
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    Unk04 = br.ReadByte();
                    br.AssertByte(0);
                    Unk06 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(Unk04);
                    bw.WriteByte(0);
                    bw.WriteByte(Unk06);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk700 : Event // 700
            {
                public override EventType Type => EventType.Unk700;

                public float Unk00;
                public float Unk04;
                public float Unk08;
                public float Unk0C;
                public int Unk10;
                // 6 - head turn
                public sbyte Unk14;
                public float Unk18;
                public float Unk1C;
                public float Unk20;
                public float Unk24;

                internal Unk700(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadSingle();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadSByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    Unk18 = br.ReadSingle();
                    Unk1C = br.ReadSingle();
                    Unk20 = br.ReadSingle();
                    Unk24 = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteSingle(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteSByte(Unk14);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteSingle(Unk18);
                    bw.WriteSingle(Unk1C);
                    bw.WriteSingle(Unk20);
                    bw.WriteSingle(Unk24);
                }
            }

            public class EnableTurningDirection : Event // 703
            {
                public override EventType Type => EventType.EnableTurningDirection;

                public byte State;

                internal EnableTurningDirection(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    State = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(State);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class FacingAngleCorrection : Event // 705
            {
                public override EventType Type => EventType.FacingAngleCorrection;

                public float CorrectionRate;

                internal FacingAngleCorrection(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    CorrectionRate = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(CorrectionRate);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk707 : Event // 707
            {
                public override EventType Type => EventType.Unk707;

                internal Unk707(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            // Used for Follower's Javelin WA
            // "Ladder State" -Pav
            public class HideWeapon : Event // 710
            {
                public override EventType Type => EventType.HideWeapon;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;

                internal HideWeapon(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class HideModelMask : Event // 711
            {
                public override EventType Type => EventType.HideModelMask;

                public byte[] Mask { get; private set; }

                internal HideModelMask(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Mask = br.ReadBytes(32);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Mask);
                }
            }

            public class DamageLevelModule : Event // 712
            {
                public override EventType Type => EventType.DamageLevelModule;

                public byte[] Mask { get; private set; }
                public byte Unk10;
                public byte Unk11;
                public byte Unk12;

                internal DamageLevelModule(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Mask = br.ReadBytes(16);
                    Unk10 = br.ReadByte();
                    Unk11 = br.ReadByte();
                    Unk12 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Mask);
                    bw.WriteByte(Unk10);
                    bw.WriteByte(Unk11);
                    bw.WriteByte(Unk12);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class ModelMask : Event // 713
            {
                public override EventType Type => EventType.ModelMask;

                public byte[] Mask { get; private set; }

                internal ModelMask(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Mask = br.ReadBytes(32);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Mask);
                }
            }

            public class DamageLevelFunction : Event // 714
            {
                public override EventType Type => EventType.DamageLevelFunction;

                public byte Unk00;

                internal DamageLevelFunction(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk715 : Event // 715
            {
                public override EventType Type => EventType.Unk715;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;
                public byte Unk04;
                public byte Unk05;
                public byte Unk06;
                public byte Unk07;

                internal Unk715(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadByte();
                    Unk05 = br.ReadByte();
                    Unk06 = br.ReadByte();
                    Unk07 = br.ReadByte();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteByte(Unk04);
                    bw.WriteByte(Unk05);
                    bw.WriteByte(Unk06);
                    bw.WriteByte(Unk07);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class CultStart : Event // 720
            {
                public override EventType Type => EventType.CultStart;

                public byte CultType;

                internal CultStart(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    CultType = br.ReadByte(); // 0
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(CultType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk730 : Event // 730
            {
                public override EventType Type => EventType.Unk730;

                public int Unk00;
                public int Unk04;

                internal Unk730(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk740 : Event // 740
            {
                public override EventType Type => EventType.Unk740;

                internal Unk740(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class IFrameState : Event // 760
            {
                public override EventType Type => EventType.IFrameState;

                public byte Unk00;
                public byte Unk01;
                public byte Unk02;
                public byte Unk03;
                public float Unk04;
                public float Unk08;
                public float Unk0C;
                public float Unk10;
                public float Unk14;

                internal IFrameState(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
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
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
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
                }
            }

            public class BonePos : Event // 770
            {
                public override EventType Type => EventType.BonePos;

                public int Unk00;
                public float Unk04;
                public byte Unk08;

                internal BonePos(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteByte(Unk08);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class BoneFixOn1 : Event // 771
            {
                public override EventType Type => EventType.BoneFixOn1;

                public byte BoneID;

                internal BoneFixOn1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    BoneID = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(BoneID);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class BoneFixOn2 : Event // 772
            {
                public override EventType Type => EventType.BoneFixOn2;

                public int Unk00;
                public float Unk04;
                public byte Unk08;

                internal BoneFixOn2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadSingle();
                    Unk08 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteSingle(Unk04);
                    bw.WriteByte(Unk08);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            public class TurnLowerBody : Event // 781
            {
                public override EventType Type => EventType.TurnLowerBody;

                public byte TurnState;

                internal TurnLowerBody(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    TurnState = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(TurnState);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk782 : Event // 782
            {
                public override EventType Type => EventType.Unk782;

                internal Unk782(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class SpawnBulletByCultSacrifice1 : Event // 785
            {
                public override EventType Type => EventType.SpawnBulletByCultSacrifice1;

                public float Unk00;
                public int DummyPointID;
                public int BulletID;
                public byte Unk0C;
                public byte Unk0D;

                internal SpawnBulletByCultSacrifice1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    DummyPointID = br.ReadInt32();
                    BulletID = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(DummyPointID);
                    bw.WriteInt32(BulletID);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }
            }

            public class Unk786 : Event // 786
            {
                public override EventType Type => EventType.Unk786;

                public float Unk00;

                internal Unk786(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(0);
                }
            }

            public class Unk790 : Event // 790
            {
                public override EventType Type => EventType.Unk790;

                internal Unk790(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk791 : Event // 791
            {
                public override EventType Type => EventType.Unk791;

                internal Unk791(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class HitEffect2 : Event // 792
            {
                public override EventType Type => EventType.HitEffect2;

                public short Unk00;
                public int Unk04;
                public int Unk08;
                public byte Unk0C;
                public byte Unk0D;
                public byte Unk0E;
                public byte Unk0F;

                internal HitEffect2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadInt16();
                    br.AssertInt16(0);
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadByte();
                    Unk0F = br.ReadByte();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Unk00);
                    bw.WriteInt16(0);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteByte(Unk0E);
                    bw.WriteByte(Unk0F);
                }
            }

            public class CultSacrifice1 : Event // 793
            {
                public override EventType Type => EventType.CultSacrifice1;

                public int SacrificeValue;

                internal CultSacrifice1(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    SacrificeValue = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SacrificeValue);
                    bw.WriteInt32(0);
                }
            }

            public class SacrificeEmpty : Event // 794
            {
                public override EventType Type => EventType.SacrificeEmpty;

                internal SacrificeEmpty(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Toughness : Event // 795
            {
                public override EventType Type => EventType.Toughness;

                public byte ToughnessParamID;
                public bool IsToughnessEffective;
                public float ToughnessRate;

                public Toughness(float startTime, float endTime, byte toughnessParamID, bool isToughnessEffective, float toughnessRate) : base(startTime, endTime)
                {
                    ToughnessParamID = toughnessParamID;
                    IsToughnessEffective = isToughnessEffective;
                    ToughnessRate = toughnessRate;
                }

                internal Toughness(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    ToughnessParamID = br.ReadByte();
                    IsToughnessEffective = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    ToughnessRate = br.ReadSingle();
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(ToughnessParamID);
                    bw.WriteBoolean(IsToughnessEffective);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteSingle(ToughnessRate);
                }
            }

            public class BringCultMenu : Event // 796
            {
                public override EventType Type => EventType.BringCultMenu;

                public byte MenuType;

                internal BringCultMenu(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    MenuType = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteByte(MenuType);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class CeremonyParamID : Event // 797
            {
                public override EventType Type => EventType.CeremonyParamID;

                public int ParamID;

                internal CeremonyParamID(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    ParamID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ParamID);
                    bw.WriteInt32(0);
                }
            }

            public class CultSingle : Event // 798
            {
                public override EventType Type => EventType.CultSingle;

                public float Unk00;

                internal CultSingle(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    Unk00 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class CultEmpty2 : Event // 799
            {
                public override EventType Type => EventType.CultEmpty2;

                internal CultEmpty2(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            public class Unk800 : Event // 800
            {
                public override EventType Type => EventType.Unk800;

                public float MetersPerTick;
                public float MetersOnTurn;
                public float Unk08;

                internal Unk800(float startTime, float endTime, BinaryReaderEx br) : base(startTime, endTime)
                {
                    MetersPerTick = br.ReadSingle();
                    MetersOnTurn = br.ReadSingle();
                    Unk08 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                internal override void WriteSpecific(BinaryWriterEx bw)
                {
                    bw.WriteSingle(MetersPerTick);
                    bw.WriteSingle(MetersOnTurn);
                    bw.WriteSingle(Unk08);
                    bw.WriteInt32(0);
                }
            }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
    }
}
