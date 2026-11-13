using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FFXDLSE
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public class FXEffect : FXSerializable
        {
            internal override string ClassName => "FXSerializableEffect";

            internal override int Version => 5;

            internal override int[] DesVersions => [3, 4];

            [XmlAttribute]
            public int ID { get; set; }

            public ParamList ParamList1 { get; set; }

            public ParamList ParamList2 { get; set; }

            public StateMap StateMap { get; set; }

            public ResourceSet ResourceSet { get; set; }

            /// <summary>One entry of a DeS wrapper effect (see <see cref="Variants"/>).</summary>
            public class Variant
            {
                /// <summary>1 in vanilla.</summary>
                public short Unk00 { get; set; } = 1;

                /// <summary>Differs per entry but repeats across related effects (a hash?).</summary>
                public uint Hash { get; set; }

                /// <summary>A complete effect with the wrapper's id.</summary>
                public FXEffect Effect { get; set; }
            }

            /// <summary>
            /// DeS only (22 vanilla files, all with 3 entries): the effect is a list of complete effects with the same id.
            /// Body: i32 1, i32 id, i32 0, u16 (DLVector class index), i32 count, then per entry u16, u32, FXEffect.
            /// When set, the other members of this object are unused.
            /// </summary>
            public List<Variant> Variants { get; set; }

            public FXEffect()
            {
                ParamList1 = new ParamList();
                ParamList2 = new ParamList();
                StateMap = new StateMap();
                ResourceSet = new ResourceSet();
            }

            internal FXEffect(BinaryReaderEx br, ClassTable classNames) : base(br, classNames) { }

            protected internal override void Deserialize(BinaryReaderEx br, ClassTable classNames)
            {
                if (classNames.Format == StreamFormat.DES && br.GetInt32(br.Position) == 1)
                {
                    br.AssertInt32(1);
                    ID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt16((short)(classNames.IndexOf("DLVector") + 1));
                    int count = br.ReadInt32();
                    Variants = new List<Variant>(count);
                    for (int i = 0; i < count; i++)
                    {
                        var v = new Variant { Unk00 = br.ReadInt16(), Hash = br.ReadUInt32() };
                        v.Effect = new FXEffect(br, classNames);
                        Variants.Add(v);
                    }
                    return;
                }

                br.AssertInt32(0);
                ID = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(2); // Param list count?
                br.AssertInt16(0);
                br.AssertInt16(2); // Judging by the order of class names, this must be an always-empty DLVector
                br.AssertInt32(0);

                ParamList1 = new ParamList(br, classNames);
                ParamList2 = new ParamList(br, classNames);

                StateMap = new StateMap(br, classNames);
                ResourceSet = new ResourceSet(br, classNames);
                if (ClassVersion >= 4) br.AssertByte(0); // DeS v3 effects end without it
            }

            internal override void AddClassNames(ClassTable classNames)
            {
                base.AddClassNames(classNames);
                DLVector.AddClassNames(classNames);
                if (Variants != null)
                {
                    foreach (var v in Variants)
                        v.Effect.AddClassNames(classNames);
                    return;
                }

                ParamList1.AddClassNames(classNames);
                ParamList2.AddClassNames(classNames);

                StateMap.AddClassNames(classNames);
                ResourceSet.AddClassNames(classNames);
            }

            protected internal override void Serialize(BinaryWriterEx bw, ClassTable classNames)
            {
                if (Variants != null)
                {
                    bw.WriteInt32(1);
                    bw.WriteInt32(ID);
                    bw.WriteInt32(0);
                    bw.WriteInt16((short)(classNames.IndexOf("DLVector") + 1));
                    bw.WriteInt32(Variants.Count);
                    foreach (var v in Variants)
                    {
                        bw.WriteInt16(v.Unk00);
                        bw.WriteUInt32(v.Hash);
                        v.Effect.Write(bw, classNames);
                    }
                    return;
                }

                bw.WriteInt32(0);
                bw.WriteInt32(ID);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(2);
                bw.WriteInt16(0);
                bw.WriteInt16(2);
                bw.WriteInt32(0);

                ParamList1.Write(bw, classNames);
                ParamList2.Write(bw, classNames);

                StateMap.Write(bw, classNames);
                ResourceSet.Write(bw, classNames);
                if (ClassVersion >= 4) bw.WriteByte(0);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
