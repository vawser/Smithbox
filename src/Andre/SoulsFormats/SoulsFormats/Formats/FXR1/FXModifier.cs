using System;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {
        [XmlInclude(typeof(FXModifier0_TrimParamList))]
        [XmlInclude(typeof(FXModifier1_MoveEffect))]
        [XmlInclude(typeof(FXModifier2_SetLifetime))]
        [XmlInclude(typeof(FXModifier3))]
        [XmlInclude(typeof(FXModifier4))]
        [XmlInclude(typeof(FXModifier5))]
        [XmlInclude(typeof(FXModifier6_SetLifetimeB))]
        [XmlInclude(typeof(FXModifier7))]
        [XmlInclude(typeof(FXModifierRef))]
        public abstract class FXModifier : XIDable
        {
            public override bool ShouldSerializeXID() => FXR1.FlattenModifiers;

            [XmlIgnore]
            public abstract int ModifierType { get; }

            public virtual bool ShouldSerializeCommandType() => true;

            internal abstract void InnerRead(BinaryReaderEx br, FxrEnvironment env);
            internal abstract void InnerWrite(BinaryWriterEx bw, FxrEnvironment env);

            internal override void ToXIDs(FXR1 fxr)
            {
                InnerToXIDs(fxr);
            }

            internal override void FromXIDs(FXR1 fxr)
            {
                InnerFromXIDs(fxr);
            }

            internal virtual void InnerToXIDs(FXR1 fxr)
            {

            }

            internal virtual void InnerFromXIDs(FXR1 fxr)
            {

            }

            internal static FXModifier GetProperType(BinaryReaderEx br, FxrEnvironment env)
            {
                int commandType = br.GetInt32(br.Position);
                FXModifier data = null;
                switch (commandType)
                {
                    case 0: data = new FXModifier0_TrimParamList(); break;
                    case 1: data = new FXModifier1_MoveEffect(); break;
                    case 2: data = new FXModifier2_SetLifetime(); break;
                    case 3: data = new FXModifier3(); break;
                    case 4: data = new FXModifier4(); break;
                    case 5: data = new FXModifier5(); break;
                    case 6: data = new FXModifier6_SetLifetimeB(); break;
                    case 7: data = new FXModifier7(); break;
                }
                return data;
            }

            internal void Read(BinaryReaderEx br, FxrEnvironment env)
            {
                InnerRead(br, env);
            }

            internal void Write(BinaryWriterEx bw, FxrEnvironment env)
            {
                env.RegisterOffset(bw.Position, this);
                InnerWrite(bw, env);
            }
        }


        public class FXModifierRef : FXModifier
        {
            public override int ModifierType => -1;

            [XmlAttribute]
            public string ReferenceXID;

            public override bool ShouldSerializeCommandType() => false;

            public FXModifierRef(FXModifier refVal)
            {
                ReferenceXID = refVal?.XID;
            }

            public FXModifierRef()
            {

            }

            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                throw new InvalidOperationException("Cannot actually serialize a reference class.");
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                throw new InvalidOperationException("Cannot actually deserialize a reference class.");
            }
        }


        public class FXModifier0_TrimParamList : FXModifier
        {
            public override int ModifierType => 0;

            public int Unk;
            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                br.AssertInt32(0);

                Unk = br.ReadInt32();
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                bw.WriteInt32(0);

                bw.WriteInt32(Unk);
            }
        }

        public class FXModifier1_MoveEffect : FXModifier
        {
            public override int ModifierType => 1;

            public int Unk1;
            public int Unk2;
            public int Unk3;
            public float OffsetX;
            public float OffsetY;
            public float Unk6;
            public int Unk7;
            public float Unk8;
            public float Unk9;
            public float Unk10;
            public float Unk11;

            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                br.AssertInt32(1);

                Unk1 = br.ReadInt32();
                Unk2 = br.ReadInt32();
                Unk3 = br.ReadInt32();
                OffsetX = br.ReadSingle();
                OffsetY = br.ReadSingle();
                Unk6 = br.ReadSingle();
                Unk7 = br.ReadInt32();
                Unk8 = br.ReadSingle();
                Unk9 = br.ReadSingle();
                Unk10 = br.ReadSingle();
                Unk11 = br.ReadSingle();
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                bw.WriteInt32(1);

                bw.WriteInt32(Unk1);
                bw.WriteInt32(Unk2);
                bw.WriteInt32(Unk3);
                bw.WriteSingle(OffsetX);
                bw.WriteSingle(OffsetY);
                bw.WriteSingle(Unk6);
                bw.WriteInt32(Unk7);
                bw.WriteSingle(Unk8);
                bw.WriteSingle(Unk9);
                bw.WriteSingle(Unk10);
                bw.WriteSingle(Unk11);
            }
        }



        public class FXModifier2_SetLifetime : FXModifier
        {
            public override int ModifierType => 2;

            public float Lifetime;
            public float Unk2;
            public int Unk3;
            public float Unk4;
            public byte Unk5;
            public byte Unk6;

            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                br.AssertInt32(2);

                Lifetime = br.ReadSingle();
                Unk2 = br.ReadSingle();
                Unk3 = br.ReadInt32();
                Unk4 = br.ReadSingle();
                Unk5 = br.ReadByte();
                Unk6 = br.ReadByte();

                br.AssertInt16(0);
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                bw.WriteInt32(2);

                bw.WriteSingle(Lifetime);
                bw.WriteSingle(Unk2);
                bw.WriteInt32(Unk3);
                bw.WriteSingle(Unk4);
                bw.WriteByte(Unk5);
                bw.WriteByte(Unk6);

                bw.WriteInt16(0);
            }
        }

        public class FXModifier3 : FXModifier
        {
            public override int ModifierType => 3;

            public float Unk1;
            public int Unk2;
            public float Unk3;
            public int Unk4;

            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                br.AssertInt32(3);

                Unk1 = br.ReadSingle();
                Unk2 = br.ReadInt32();
                Unk3 = br.ReadSingle();
                Unk4 = br.ReadInt32();
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                bw.WriteInt32(3);

                bw.WriteSingle(Unk1);
                bw.WriteInt32(Unk2);
                bw.WriteSingle(Unk3);
                bw.WriteInt32(Unk4);
            }
        }

        public class FXModifier4 : FXModifier
        {
            public override int ModifierType => 4;

            public int Unk;
            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                br.AssertInt32(4);

                Unk = br.ReadInt32();
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                bw.WriteInt32(4);

                bw.WriteInt32(Unk);
            }
        }

        public class FXModifier5 : FXModifier
        {
            public override int ModifierType => 5;

            public int Unk;
            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                br.AssertInt32(5);

                Unk = br.ReadInt32();
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                bw.WriteInt32(5);

                bw.WriteInt32(Unk);
            }
        }

        public class FXModifier6_SetLifetimeB : FXModifier
        {
            public override int ModifierType => 6;

            public float Unk1;
            public int Unk2;
            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                br.AssertInt32(6);

                Unk1 = br.ReadSingle();
                Unk2 = br.ReadInt32();
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                bw.WriteInt32(6);

                bw.WriteSingle(Unk1);
                bw.WriteInt32(Unk2);
            }
        }

        public class FXModifier7 : FXModifier
        {
            public override int ModifierType => 7;

            public int Unk;
            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                br.AssertInt32(7);

                Unk = br.ReadInt32();
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                bw.WriteInt32(7);

                bw.WriteInt32(Unk);
            }
        }


    }
}
