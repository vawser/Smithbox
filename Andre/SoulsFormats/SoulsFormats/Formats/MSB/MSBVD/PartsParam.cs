using System.Numerics;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq.Expressions;
using System.Diagnostics;

namespace SoulsFormats
{
    public partial class MSBVD
    {
        /// <summary>
        /// The different types of instances of things on the map.
        /// </summary>
        internal enum PartType : int
        {
            /// <summary>
            /// A piece of the map.
            /// </summary>
            MapPiece = 0,

            /// <summary>
            /// A dynamic or interactive object.
            /// </summary>
            Object = 1,

            /// <summary>
            /// All non-player characters.
            /// </summary>
            Enemy = 2,

            /// <summary>
            /// Object models referenced in MSB files other than the map MSB.
            /// </summary>
            ExternalObject = 4,

            /// <summary>
            /// Unused instances of objects or map pieces that were left in.
            /// </summary>
            Unused = -1,
        }

        #region Param

        /// <summary>
        /// Instances of actual things in the map.
        /// </summary>
        public class PartsParam : Param<Part>, IMsbParam<IMsbPart>
        {
            public List<Part.MapPiece> MapPieces { get; set; }
            public List<Part.Object> Objects { get; set; }
            public List<Part.Enemy> Enemies { get; set; }
            public List<Part.ExternalObject> ExternalObjects { get; set; }
            public List<Part.Unused> Unused { get; set; }

            /// <summary>
            /// Creates an empty PartsParam with the default version.
            /// </summary>
            public PartsParam() : base(10001002, "PARTS_PARAM_ST")
            {
                MapPieces = new List<Part.MapPiece>();
                Objects = new List<Part.Object>();
                Enemies = new List<Part.Enemy>();
                ExternalObjects = new List<Part.ExternalObject>();
                Unused = new List<Part.Unused>();
            }

            /// <summary>
            /// Adds a part to the appropriate list for its type; returns the part.
            /// </summary>
            public Part Add(Part part)
            {
                switch (part)
                {
                    case Part.MapPiece p:
                        MapPieces.Add(p);
                        break;
                    case Part.Object p:
                        Objects.Add(p);
                        break;
                    case Part.Enemy p:
                        Enemies.Add(p);
                        break;
                    case Part.ExternalObject p:
                        ExternalObjects.Add(p);
                        break;
                    case Part.Unused p:
                        Unused.Add(p);
                        break;
                    default:
                        throw new ArgumentException($"Unrecognized type {part.GetType()}.", nameof(part));
                }
                return part;
            }
            IMsbPart IMsbParam<IMsbPart>.Add(IMsbPart item) => Add((Part)item);

            /// <summary>
            /// Returns every Part in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries() => SFUtil.ConcatAll<Part>(MapPieces, Objects, Enemies, ExternalObjects, Unused);
            IReadOnlyList<IMsbPart> IMsbParam<IMsbPart>.GetEntries() => GetEntries();

            internal override Part ReadEntry(BinaryReaderEx br)
            {
                PartType type = br.GetEnum32<PartType>(br.Position + 4);
                switch (type)
                {
                    case PartType.MapPiece:
                        return MapPieces.EchoAdd(new Part.MapPiece(br));
                    case PartType.Object:
                        return Objects.EchoAdd(new Part.Object(br));
                    case PartType.Enemy:
                        return Enemies.EchoAdd(new Part.Enemy(br));
                    case PartType.ExternalObject:
                        return ExternalObjects.EchoAdd(new Part.ExternalObject(br));
                    case PartType.Unused:
                        return Unused.EchoAdd(new Part.Unused(br));
                    default:
                        throw new NotImplementedException($"Unimplemented part type: {type}");
                }
            }
        }

        #endregion

        #region Entry

        public abstract class Part : ParamEntry, IMsbPart
        {
            #region Properties

            /// <summary>
            /// The type of the part.
            /// </summary>
            private protected abstract PartType Type { get; }

            /// <summary>
            /// Whether or not BehaviorConfig is present.
            /// </summary>
            private protected abstract bool HasBehaviorConfig { get; }

            /// <summary>
            /// Whether or not EnemyConfig is present.
            /// </summary>
            private protected abstract bool HasEnemyConfig { get; }

            /// <summary>
            /// Whether or not ObjectConfig is present.
            /// </summary>
            private protected abstract bool HasObjectConfig { get; }

            /// <summary>
            /// Whether or not UnkConfig11 is present.
            /// </summary>
            private protected abstract bool HasUnkConfig11 { get; }

            /// <summary>
            /// Whether or not UnkConfig12 is present.
            /// </summary>
            private protected abstract bool HasUnkConfig12 { get; }

            /// <summary>
            /// The name of the model used by this part without folder or extension.
            /// </summary>
            public string ModelName { get; set; }

            /// <summary>
            /// The path to a resource that presumably used to contain this part during development. Usually a path to an SIB file.
            /// </summary>
            public string ResourcePath { get; set; }

            /// <summary>
            /// The position of the part.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotation of the part in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// The scale of the part.
            /// <para>May not be obeyed.</para>
            /// </summary>
            public Vector3 Scale { get; set; }

            /// <summary>
            /// Determines the hostility of an entity to other entities.
            /// </summary>
            public int EntityGroupID { get; set; }

            /// <summary>
            /// Identifies the part in external files.
            /// </summary>
            public int EntityID { get; set; }

            /// <summary>
            /// Unknown; Always -1.
            /// </summary>
            public short Unk40 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk42 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public bool Unk43 { get; set; }

            /// <summary>
            /// Unknown; Affects shadows and fog somehow.
            /// </summary>
            public UnkConfig3 Config3 { get; set; }

            /// <summary>
            /// Holds event group IDs referenced in scripts for an MSB.
            /// </summary>
            public EventGroupConfig EventGroup { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public UnkConfig5 Config5 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public UnkConfig6 Config6 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public UnkConfig7 Config7 { get; set; }

            /// <summary>
            /// Controls what layers this part appears on.
            /// </summary>
            public LayerConfig Layer { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public UnkConfig10 Config10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public UnkConfig13 Config13 { get; set; }

            #endregion

            #region Methods

            private protected Part(string name)
            {
                Name = name;
                Scale = Vector3.One;
                EntityGroupID = -1;
                EntityID = 0;
                Unk40 = -1;
                Unk42 = false;
                Unk43 = false;
                Config3 = new UnkConfig3();
                EventGroup = new EventGroupConfig();
                Config5 = new UnkConfig5();
                Config6 = new UnkConfig6();
                Config7 = new UnkConfig7();
                Layer = new LayerConfig();
                Config10 = new UnkConfig10();
                Config13 = new UnkConfig13();
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                part.Config3 = Config3.DeepCopy();
                part.EventGroup = EventGroup.DeepCopy();
                part.Config5 = Config5.DeepCopy();
                part.Config6 = Config6.DeepCopy();
                part.Config7 = Config7.DeepCopy();
                part.Layer = Layer.DeepCopy();
                part.Config10 = Config10.DeepCopy();
                part.Config13 = Config13.DeepCopy();
                DeepCopyTo(part);
                return part;
            }
            IMsbPart IMsbPart.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Part part) { }

            internal Part(BinaryReaderEx br)
            {
                long start = br.Position;

                int nameOffset = br.ReadInt32();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                int modelNameOffset = br.ReadInt32();
                int resourceOffset = br.ReadInt32();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Scale = br.ReadVector3();
                EntityGroupID = br.ReadInt32();
                EntityID = br.ReadInt32();
                Unk40 = br.ReadInt16();
                Unk42 = br.ReadBoolean();
                Unk43 = br.ReadBoolean();

                int offsetBehaviorConfig = br.ReadInt32();
                int offsetEnemyConfig = br.ReadInt32();
                int offsetUnkConfig3 = br.ReadInt32();
                int offsetEventGroupConfig = br.ReadInt32();
                int offsetUnkConfig5 = br.ReadInt32();
                int offsetUnkConfig6 = br.ReadInt32();
                int offsetUnkConfig7 = br.ReadInt32();
                int offsetObjectConfig = br.ReadInt32();
                int offsetLayerConfig = br.ReadInt32();
                int offsetUnkConfig10 = br.ReadInt32();
                int offsetUnkConfig11 = br.ReadInt32();
                int offsetUnkConfig12 = br.ReadInt32();
                int offsetUnkConfig13 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (modelNameOffset == 0)
                    throw new InvalidDataException($"{nameof(modelNameOffset)} must not be 0 in type {GetType()}.");
                if (resourceOffset == 0)
                    throw new InvalidDataException($"{nameof(resourceOffset)} must not be 0 in type {GetType()}.");
                if (HasBehaviorConfig ^ offsetBehaviorConfig != 0)
                    throw new InvalidDataException($"Unexpected {nameof(offsetBehaviorConfig)} 0x{offsetBehaviorConfig:X} in type {GetType()}.");
                if (HasEnemyConfig ^ offsetEnemyConfig != 0)
                    throw new InvalidDataException($"Unexpected {nameof(offsetEnemyConfig)} 0x{offsetEnemyConfig:X} in type {GetType()}.");
                if (offsetUnkConfig3 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig3)} must not be 0 in type {GetType()}.");
                if (offsetEventGroupConfig == 0)
                    throw new InvalidDataException($"{nameof(offsetEventGroupConfig)} must not be 0 in type {GetType()}.");
                if (offsetUnkConfig5 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig5)} must not be 0 in type {GetType()}.");
                if (offsetUnkConfig6 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig6)} must not be 0 in type {GetType()}.");
                if (offsetUnkConfig7 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig7)} must not be 0 in type {GetType()}.");
                if (HasObjectConfig ^ offsetObjectConfig != 0)
                    throw new InvalidDataException($"Unexpected {nameof(offsetObjectConfig)} 0x{offsetObjectConfig:X} in type {GetType()}.");
                if (offsetLayerConfig == 0)
                    throw new InvalidDataException($"{nameof(offsetLayerConfig)} must not be 0 in type {GetType()}.");
                if (offsetUnkConfig10 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig10)} must not be 0 in type {GetType()}.");
                if (HasUnkConfig11 ^ offsetUnkConfig11 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(offsetUnkConfig11)} 0x{offsetUnkConfig11:X} in type {GetType()}.");
                if (HasUnkConfig12 ^ offsetUnkConfig12 != 0)
                    throw new InvalidDataException($"Unexpected {nameof(offsetUnkConfig12)} 0x{offsetUnkConfig12:X} in type {GetType()}.");
                if (offsetUnkConfig13 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig13)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadShiftJIS();

                br.Position = start + modelNameOffset;
                ModelName = br.ReadASCII();

                br.Position = start + resourceOffset;
                ResourcePath = br.ReadASCII();

                if (HasBehaviorConfig)
                {
                    br.Position = start + offsetBehaviorConfig;
                    ReadBehaviorConfig(br);
                }

                if (HasEnemyConfig)
                {
                    br.Position = start + offsetEnemyConfig;
                    ReadEnemyConfig(br);
                }

                br.Position = start + offsetUnkConfig3;
                ReadUnkConfig3(br);

                br.Position = start + offsetEventGroupConfig;
                ReadEventGroupConfig(br);

                br.Position = start + offsetUnkConfig5;
                ReadUnkConfig5(br);

                br.Position = start + offsetUnkConfig6;
                ReadUnkConfig6(br);

                br.Position = start + offsetUnkConfig7;
                ReadUnkConfig7(br);

                if (HasObjectConfig)
                {
                    br.Position = start + offsetObjectConfig;
                    ReadObjectConfig(br);
                }

                br.Position = start + offsetLayerConfig;
                ReadLayerConfig(br);

                br.Position = start + offsetUnkConfig10;
                ReadUnkConfig10(br);

                if (HasUnkConfig11)
                {
                    br.Position = start + offsetUnkConfig11;
                    ReadUnkConfig11(br);
                }

                if (HasUnkConfig12)
                {
                    br.Position = start + offsetUnkConfig12;
                    ReadUnkConfig12(br);
                }

                br.Position = start + offsetUnkConfig13;
                ReadUnkConfig13(br);
            }

            private protected virtual void ReadBehaviorConfig(BinaryReaderEx br) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadBehaviorConfig)}.");
            private protected virtual void ReadEnemyConfig(BinaryReaderEx br) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadEnemyConfig)}.");
            private void ReadUnkConfig3(BinaryReaderEx br) => Config3 = new UnkConfig3(br);
            private void ReadEventGroupConfig(BinaryReaderEx br) => EventGroup = new EventGroupConfig(br);
            private void ReadUnkConfig5(BinaryReaderEx br) => Config5 = new UnkConfig5(br);
            private void ReadUnkConfig6(BinaryReaderEx br) => Config6 = new UnkConfig6(br);
            private void ReadUnkConfig7(BinaryReaderEx br) => Config7 = new UnkConfig7(br);
            private protected virtual void ReadObjectConfig(BinaryReaderEx br) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadObjectConfig)}.");
            private void ReadLayerConfig(BinaryReaderEx br) => Layer = new LayerConfig(br);
            private void ReadUnkConfig10(BinaryReaderEx br) => Config10 = new UnkConfig10(br);
            private protected virtual void ReadUnkConfig11(BinaryReaderEx br) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkConfig11)}.");
            private protected virtual void ReadUnkConfig12(BinaryReaderEx br) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadUnkConfig12)}.");
            private void ReadUnkConfig13(BinaryReaderEx br) => Config13 = new UnkConfig13(br);

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt32("NameOffset");
                bw.WriteInt32((int)Type);
                bw.WriteInt32(id);
                bw.ReserveInt32("ModelNameOffset");
                bw.ReserveInt32("ResourcePathOffset");
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteVector3(Scale);
                bw.WriteInt32(EntityGroupID);
                bw.WriteInt32(EntityID);
                bw.WriteInt16(Unk40);
                bw.WriteBoolean(Unk42);
                bw.WriteBoolean(Unk43);
                bw.ReserveInt32("OffsetBehaviorConfig");
                bw.ReserveInt32("OffsetEnemyConfig");
                bw.ReserveInt32("OffsetUnkConfig3");
                bw.ReserveInt32("OffsetEventGroupConfig");
                bw.ReserveInt32("OffsetUnkConfig5");
                bw.ReserveInt32("OffsetUnkConfig6");
                bw.ReserveInt32("OffsetUnkConfig7");
                bw.ReserveInt32("OffsetObjectConfig");
                bw.ReserveInt32("OffsetLayerConfig");
                bw.ReserveInt32("OffsetUnkConfig10");
                bw.ReserveInt32("OffsetUnkConfig11");
                bw.ReserveInt32("OffsetUnkConfig12");
                bw.ReserveInt32("OffsetUnkConfig13");
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(Name), true);
                // Not padded between strings

                bw.FillInt32("ModelNameOffset", (int)(bw.Position - start));
                bw.WriteASCII(ModelName, true);
                // Not padded between strings

                bw.FillInt32("ResourcePathOffset", (int)(bw.Position - start));
                bw.WriteASCII(ResourcePath, true);
                bw.Pad(4);

                if (HasBehaviorConfig)
                {
                    bw.FillInt32("OffsetBehaviorConfig", (int)(bw.Position - start));
                    WriteBehaviorConfig(bw);
                }
                else
                {
                    bw.FillInt32("OffsetBehaviorConfig", 0);
                }

                if (HasEnemyConfig)
                {
                    bw.FillInt32("OffsetEnemyConfig", (int)(bw.Position - start));
                    WriteEnemyConfig(bw);
                }
                else
                {
                    bw.FillInt32("OffsetEnemyConfig", 0);
                }

                bw.FillInt32("OffsetUnkConfig3", (int)(bw.Position - start));
                WriteUnkConfig3(bw);

                bw.FillInt32("OffsetEventGroupConfig", (int)(bw.Position - start));
                WriteEventGroupConfig(bw);

                bw.FillInt32("OffsetUnkConfig5", (int)(bw.Position - start));
                WriteUnkConfig5(bw);

                bw.FillInt32("OffsetUnkConfig6", (int)(bw.Position - start));
                WriteUnkConfig6(bw);

                bw.FillInt32("OffsetUnkConfig7", (int)(bw.Position - start));
                WriteUnkConfig7(bw);

                if (HasObjectConfig)
                {
                    bw.FillInt32("OffsetObjectConfig", (int)(bw.Position - start));
                    WriteObjectConfig(bw);
                }
                else
                {
                    bw.FillInt32("OffsetObjectConfig", 0);
                }

                bw.FillInt32("OffsetLayerConfig", (int)(bw.Position - start));
                WriteLayerConfig(bw);

                bw.FillInt32("OffsetUnkConfig10", (int)(bw.Position - start));
                WriteUnkConfig10(bw);

                if (HasUnkConfig11)
                {
                    bw.FillInt32("OffsetUnkConfig11", (int)(bw.Position - start));
                    WriteUnkConfig11(bw);
                }
                else
                {
                    bw.FillInt32("OffsetUnkConfig11", 0);
                }

                if (HasUnkConfig12)
                {
                    bw.FillInt32("OffsetUnkConfig12", (int)(bw.Position - start));
                    WriteUnkConfig12(bw);
                }
                else
                {
                    bw.FillInt32("OffsetUnkConfig12", 0);
                }

                bw.FillInt32("OffsetUnkConfig13", (int)(bw.Position - start));
                WriteUnkConfig13(bw);
            }

            private protected virtual void WriteBehaviorConfig(BinaryWriterEx bw) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteBehaviorConfig)}.");
            private protected virtual void WriteEnemyConfig(BinaryWriterEx bw) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteEnemyConfig)}.");
            private void WriteUnkConfig3(BinaryWriterEx bw) => Config3.Write(bw);
            private void WriteEventGroupConfig(BinaryWriterEx bw) => EventGroup.Write(bw);
            private void WriteUnkConfig5(BinaryWriterEx bw) => Config5.Write(bw);
            private void WriteUnkConfig6(BinaryWriterEx bw) => Config6.Write(bw);
            private void WriteUnkConfig7(BinaryWriterEx bw) => Config7.Write(bw);
            private protected virtual void WriteObjectConfig(BinaryWriterEx bw) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteObjectConfig)}.");
            private void WriteLayerConfig(BinaryWriterEx bw) => Layer.Write(bw);
            private void WriteUnkConfig10(BinaryWriterEx bw) => Config10.Write(bw);
            private protected virtual void WriteUnkConfig11(BinaryWriterEx bw) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkConfig11)}.");
            private protected virtual void WriteUnkConfig12(BinaryWriterEx bw) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteUnkConfig12)}.");
            private void WriteUnkConfig13(BinaryWriterEx bw) => Config13.Write(bw);

            /// <summary>
            /// Returns the type and name of the part as a string.
            /// </summary>
            public override string ToString()
            {
                return $"{Type} {Name}";
            }

            #endregion

            #region Part Sub Structs

            /// <summary>
            /// Behavior type data referencing scripts of some kind. Optional.
            /// </summary>
            public class BehaviorConfig
            {
                /// <summary>
                /// The name of a Behavior script of some kind without the folder or extension.
                /// <para>Always "0" on map pieces.</para>
                /// </summary>
                public string BehaviorScript { get; set; }

                public BehaviorConfig()
                {
                    BehaviorScript = "0";
                }

                public BehaviorConfig(string behaviorScript)
                {
                    BehaviorScript = behaviorScript;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public BehaviorConfig DeepCopy()
                {
                    return (BehaviorConfig)MemberwiseClone();
                }

                internal BehaviorConfig(BinaryReaderEx br)
                {
                    long pos = br.Position;
                    int offset = br.ReadInt32();

                    br.Position = pos + offset;
                    BehaviorScript = br.ReadASCII();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long pos = bw.Position;
                    bw.ReserveInt32("PartBehaviorOffsetBehaviorScript");

                    bw.FillInt32("PartBehaviorOffsetBehaviorScript", (int)(bw.Position - pos));
                    bw.WriteASCII(BehaviorScript, true);
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// Enemy type data referencing AI scripts. Optional.
            /// </summary>
            public class EnemyConfig
            {
                /// <summary>
                /// The name of an AI enemy script without the folder or extension.
                /// </summary>
                public string AIScript { get; set; } = "-1";

                /// <summary>
                /// Unknown. Always "-1".
                /// </summary>
                public string Unk { get; set; } = "-1";

                public EnemyConfig()
                {
                }

                public EnemyConfig(string aiscript)
                {
                    AIScript = aiscript;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public EnemyConfig DeepCopy()
                {
                    return (EnemyConfig)MemberwiseClone();
                }

                internal EnemyConfig(BinaryReaderEx br)
                {
                    long start = br.Position;
                    int offsetAIScript = br.ReadInt32();
                    int offsetUnk = br.ReadInt32();

                    br.Position = start + offsetAIScript;
                    AIScript = br.ReadASCII();

                    br.Position = start + offsetUnk;
                    Unk = br.ReadASCII();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long start = bw.Position;
                    bw.ReserveInt32("PartEnemyOffsetAIScript");
                    bw.ReserveInt32("PartEnemyOffsetUnk");

                    bw.FillInt32("PartEnemyOffsetAIScript", (int)(bw.Position - start));
                    bw.WriteASCII(AIScript, true);
                    // Not padded between strings

                    bw.FillInt32("PartEnemyOffsetUnk", (int)(bw.Position - start));
                    bw.WriteASCII(Unk, true);
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// Unknown; Affects shadows and fog somehow. Required.
            /// </summary>
            public class UnkConfig3
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk05 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk07 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte[] Unk08 { get; private set; }

                public UnkConfig3()
                {
                    Unk00 = -1;
                    Unk01 = 0;
                    Unk02 = 0;
                    Unk03 = 0;
                    Unk04 = false;
                    Unk05 = false;
                    Unk06 = false;
                    Unk07 = true;
                    Unk08 = new byte[16] { 128, 0, 255, 255, 255, 255, 255, 0, 5, 0, 255, 255, 0, 0, 0, 0 };
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig3 DeepCopy()
                {
                    return (UnkConfig3)MemberwiseClone();
                }

                internal UnkConfig3(BinaryReaderEx br)
                {
                    Unk00 = br.ReadSByte();
                    Unk01 = br.ReadSByte();
                    Unk02 = br.ReadSByte();
                    Unk03 = br.ReadSByte();
                    Unk04 = br.ReadBoolean();
                    Unk05 = br.ReadBoolean();
                    Unk06 = br.ReadBoolean();
                    Unk07 = br.ReadBoolean();
                    Unk08 = br.ReadBytes(16);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteSByte(Unk00);
                    bw.WriteSByte(Unk01);
                    bw.WriteSByte(Unk02);
                    bw.WriteSByte(Unk03);
                    bw.WriteBoolean(Unk04);
                    bw.WriteBoolean(Unk05);
                    bw.WriteBoolean(Unk06);
                    bw.WriteBoolean(Unk07);
                    bw.WriteBytes(Unk08);
                }
            }

            /// <summary>
            /// Holds event group IDs referenced in scripts for an MSB. Required.
            /// </summary>
            public class EventGroupConfig
            {
                /// <summary>
                /// IDs of some kind referenced in scripts.
                /// </summary>
                public int[] EventGroupIDs { get; private set; }

                public EventGroupConfig()
                {
                    EventGroupIDs = new int[4] { -1, -1, -1, -1 };
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public EventGroupConfig DeepCopy()
                {
                    return (EventGroupConfig)MemberwiseClone();
                }

                internal EventGroupConfig(BinaryReaderEx br)
                {
                    EventGroupIDs = br.ReadInt32s(4);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32s(EventGroupIDs);
                }
            }

            /// <summary>
            /// Unknown. Required.
            /// </summary>
            public class UnkConfig5
            {
                /// <summary>
                /// Unknown. Usually 0, but seen as 2560 in m3524_mother_test03.msb e3211_0000.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown. Always "-1".
                /// </summary>
                public string Unk { get; set; } = "-1";

                public UnkConfig5()
                {
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig5 DeepCopy()
                {
                    return (UnkConfig5)MemberwiseClone();
                }

                internal UnkConfig5(BinaryReaderEx br)
                {
                    long pos = br.Position;
                    int offset = br.ReadInt32();
                    br.ReadInt32(); // Size
                    Unk08 = br.ReadInt32();

                    br.Position = pos + offset;
                    br.ReadASCII();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long pos = bw.Position;
                    bw.ReserveInt32("PartUnk54Offset");
                    bw.ReserveInt32("PartUnk54Size");
                    bw.WriteInt32(Unk08);

                    bw.FillInt32("PartUnk54Offset", (int)(bw.Position - pos));
                    bw.WriteASCII(Unk, true);
                    bw.FillInt32("PartUnk54Size", (int)(bw.Position - pos));
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// Unknown. Required.
            /// </summary>
            public class UnkConfig6
            {
                /// <summary>
                /// Unknown. Always "-1".
                /// </summary>
                public string Unk { get; set; } = "-1";

                public UnkConfig6()
                {
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig6 DeepCopy()
                {
                    return (UnkConfig6)MemberwiseClone();
                }

                internal UnkConfig6(BinaryReaderEx br)
                {
                    long pos = br.Position;
                    int offset = br.ReadInt32();

                    br.Position = pos + offset;
                    br.ReadASCII();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long pos = bw.Position;
                    bw.ReserveInt32("PartUnk58Offset");

                    bw.FillInt32("PartUnk58Offset", (int)(bw.Position - pos));
                    bw.WriteASCII(Unk, true);
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// Unknown. Required.
            /// </summary>
            public class UnkConfig7
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                public UnkConfig7()
                {
                    Unk00 = 0;
                    Unk04 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig7 DeepCopy()
                {
                    return (UnkConfig7)MemberwiseClone();
                }

                internal UnkConfig7(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }
            }

            /// <summary>
            /// Unknown. Some kind of type data for objects. Optional.
            /// </summary>
            public class ObjectConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public ObjectConfigBlock Block1 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public ObjectConfigBlock Block2 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public ObjectConfigBlock Block3 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public ObjectConfigBlock Block4 { get; set; }

                public ObjectConfig()
                {
                    Block1 = new ObjectConfigBlock();
                    Block2 = new ObjectConfigBlock();
                    Block3 = new ObjectConfigBlock();
                    Block4 = new ObjectConfigBlock();
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public ObjectConfig DeepCopy()
                {
                    var partObject = (ObjectConfig)MemberwiseClone();
                    partObject.Block1 = Block1.DeepCopy();
                    partObject.Block2 = Block2.DeepCopy();
                    partObject.Block3 = Block3.DeepCopy();
                    partObject.Block4 = Block4.DeepCopy();
                    return partObject;
                }

                internal ObjectConfig(BinaryReaderEx br)
                {
                    long start = br.Position;

                    int offsetBlock1 = br.ReadInt32();
                    int offsetBlock2 = br.ReadInt32();
                    int offsetBlock3 = br.ReadInt32();
                    int offsetBlock4 = br.ReadInt32();

                    br.Position = start + offsetBlock1;
                    Block1 = new ObjectConfigBlock(br);

                    br.Position = start + offsetBlock2;
                    Block2 = new ObjectConfigBlock(br);

                    br.Position = start + offsetBlock3;
                    Block3 = new ObjectConfigBlock(br);

                    br.Position = start + offsetBlock4;
                    Block4 = new ObjectConfigBlock(br);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long start = bw.Position;
                    bw.ReserveInt32("PartObjectOffsetBlock1");
                    bw.ReserveInt32("PartObjectOffsetBlock2");
                    bw.ReserveInt32("PartObjectOffsetBlock3");
                    bw.ReserveInt32("PartObjectOffsetBlock4");

                    bw.FillInt32("PartObjectOffsetBlock1", (int)(bw.Position - start));
                    Block1.Write(bw);

                    bw.FillInt32("PartObjectOffsetBlock2", (int)(bw.Position - start));
                    Block2.Write(bw);

                    bw.FillInt32("PartObjectOffsetBlock3", (int)(bw.Position - start));
                    Block3.Write(bw);

                    bw.FillInt32("PartObjectOffsetBlock4", (int)(bw.Position - start));
                    Block4.Write(bw);
                }

                /// <summary>
                /// Unknown. Some kind of block of data for objects.
                /// </summary>
                public class ObjectConfigBlock
                {
                    public sbyte Unk00 { get; set; }
                    public sbyte Unk01 { get; set; }
                    public sbyte Unk02 { get; set; }
                    public sbyte Unk03 { get; set; }
                    public int Unk04 { get; set; }
                    public int Unk08 { get; set; }

                    public ObjectConfigBlock()
                    {
                        Unk00 = -1;
                        Unk01 = 0;
                        Unk02 = 0;
                        Unk03 = 0;
                        Unk04 = 8;
                        Unk08 = 0;
                    }

                    /// <summary>
                    /// Creates a deep copy of the struct.
                    /// </summary>
                    public ObjectConfigBlock DeepCopy()
                    {
                        return (ObjectConfigBlock)MemberwiseClone();
                    }

                    internal ObjectConfigBlock(BinaryReaderEx br)
                    {
                        Unk00 = br.ReadSByte();
                        Unk01 = br.ReadSByte();
                        Unk02 = br.ReadSByte();
                        Unk03 = br.ReadSByte();
                        Unk04 = br.ReadInt32();
                        Unk08 = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteSByte(Unk00);
                        bw.WriteSByte(Unk01);
                        bw.WriteSByte(Unk02);
                        bw.WriteSByte(Unk03);
                        bw.WriteInt32(Unk04);
                        bw.WriteInt32(Unk08);
                    }
                }
            }

            /// <summary>
            /// Controls what layers this part appears on. Required.
            /// </summary>
            public class LayerConfig
            {
                /// <summary>
                /// Determines what layers this part will appear on.
                /// </summary>
                public short LayerMask { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk02 { get; set; }

                public LayerConfig()
                {
                    LayerMask = -1;
                    Unk02 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public LayerConfig DeepCopy()
                {
                    return (LayerConfig)MemberwiseClone();
                }

                internal LayerConfig(BinaryReaderEx br)
                {
                    LayerMask = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(LayerMask);
                    bw.WriteInt16(Unk02);
                }
            }

            /// <summary>
            /// Unknown. Required.
            /// </summary>
            public class UnkConfig10
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                public UnkConfig10()
                {
                    Unk00 = 0;
                    Unk04 = 0;
                    Unk08 = 0;
                    Unk0C = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig10 DeepCopy()
                {
                    return (UnkConfig10)MemberwiseClone();
                }

                internal UnkConfig10(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }
            }

            /// <summary>
            /// Unknown. Optional.
            /// </summary>
            public class UnkConfig11
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                public UnkConfig11()
                {
                    Unk00 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig11 DeepCopy()
                {
                    return (UnkConfig11)MemberwiseClone();
                }

                internal UnkConfig11(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }
            }

            /// <summary>
            /// Unknown. Optional.
            /// </summary>
            public class UnkConfig12
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                public UnkConfig12()
                {
                    Unk00 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig12 DeepCopy()
                {
                    return (UnkConfig12)MemberwiseClone();
                }

                internal UnkConfig12(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }
            }

            /// <summary>
            /// Unknown. Required.
            /// </summary>
            public class UnkConfig13
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk00 { get; set; }

                public UnkConfig13()
                {
                    Unk00 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig13 DeepCopy()
                {
                    return (UnkConfig13)MemberwiseClone();
                }

                internal UnkConfig13(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }
            }

            #endregion

            #region PartType Structs

            /// <summary>
            /// A piece of the map.
            /// </summary>
            public class MapPiece : Part
            {
                private protected override PartType Type => PartType.MapPiece;
                private protected override bool HasBehaviorConfig => true;
                private protected override bool HasEnemyConfig => false;
                private protected override bool HasObjectConfig => false;
                private protected override bool HasUnkConfig11 => false;
                private protected override bool HasUnkConfig12 => false;

                public BehaviorConfig Behavior { get; set; }

                public MapPiece() : base("mXXXX_XXXX")
                {
                    Behavior = new BehaviorConfig();
                }

                public MapPiece(string name) : base(name)
                {
                    Behavior = new BehaviorConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var piece = (MapPiece)part;
                    piece.Behavior = Behavior.DeepCopy();
                }

                internal MapPiece(BinaryReaderEx br) : base(br) { }

                private protected override void ReadBehaviorConfig(BinaryReaderEx br) => Behavior = new BehaviorConfig(br);
                private protected override void WriteBehaviorConfig(BinaryWriterEx bw) => Behavior.Write(bw);
            }

            /// <summary>
            /// A dynamic or interactive object.
            /// </summary>
            public class Object : Part
            {
                private protected override PartType Type => PartType.Object;
                private protected override bool HasBehaviorConfig => true;
                private protected override bool HasEnemyConfig => false;
                private protected override bool HasObjectConfig => true;
                private protected override bool HasUnkConfig11 => true;
                private protected override bool HasUnkConfig12 => true;

                /// <summary>
                /// Behavior type data referencing scripts of some kind.
                /// </summary>
                public BehaviorConfig Behavior { get; set; }

                /// <summary>
                /// Unknown. Some kind of type data for objects.
                /// </summary>
                public ObjectConfig ObjectData { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkConfig11 Config11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkConfig12 Config12 { get; set; }

                public Object() : base("oXXXX_XXXX")
                {
                    Behavior = new BehaviorConfig();
                    ObjectData = new ObjectConfig();
                    Config11 = new UnkConfig11();
                    Config12 = new UnkConfig12();
                }

                public Object(string name) : base(name)
                {
                    Behavior = new BehaviorConfig();
                    ObjectData = new ObjectConfig();
                    Config11 = new UnkConfig11();
                    Config12 = new UnkConfig12();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var obj = (Object)part;
                    obj.Behavior = Behavior.DeepCopy();
                    obj.ObjectData = ObjectData.DeepCopy();
                    obj.Config11 = Config11.DeepCopy();
                    obj.Config12 = Config12.DeepCopy();
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadBehaviorConfig(BinaryReaderEx br) => Behavior = new BehaviorConfig(br);
                private protected override void ReadObjectConfig(BinaryReaderEx br) => ObjectData = new ObjectConfig(br);
                private protected override void ReadUnkConfig11(BinaryReaderEx br) => Config11 = new UnkConfig11(br);
                private protected override void ReadUnkConfig12(BinaryReaderEx br) => Config12 = new UnkConfig12(br);

                private protected override void WriteBehaviorConfig(BinaryWriterEx bw) => Behavior.Write(bw);
                private protected override void WriteObjectConfig(BinaryWriterEx bw) => ObjectData.Write(bw);
                private protected override void WriteUnkConfig11(BinaryWriterEx bw) => Config11.Write(bw);
                private protected override void WriteUnkConfig12(BinaryWriterEx bw) => Config12.Write(bw);
            }

            /// <summary>
            /// All non-player characters.
            /// </summary>
            public class Enemy : Part
            {
                private protected override PartType Type => PartType.Enemy;
                private protected override bool HasBehaviorConfig => true;
                private protected override bool HasEnemyConfig => true;
                private protected override bool HasObjectConfig => false;
                private protected override bool HasUnkConfig11 => true;
                private protected override bool HasUnkConfig12 => true;

                /// <summary>
                /// Behavior type data referencing scripts of some kind.
                /// </summary>
                public BehaviorConfig Behavior { get; set; }

                /// <summary>
                /// Enemy type data referencing AI scripts.
                /// </summary>
                public EnemyConfig EnemyData { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkConfig11 Config11 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public UnkConfig12 Config12 { get; set; }

                public Enemy() : base("eXXXX_XXXX")
                {
                    Behavior = new BehaviorConfig();
                    EnemyData = new EnemyConfig();
                    Config11 = new UnkConfig11();
                    Config12 = new UnkConfig12();
                }

                public Enemy(string name) : base(name)
                {
                    Behavior = new BehaviorConfig();
                    EnemyData = new EnemyConfig();
                    Config11 = new UnkConfig11();
                    Config12 = new UnkConfig12();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (Enemy)part;
                    enemy.Behavior = Behavior.DeepCopy();
                    enemy.EnemyData = EnemyData.DeepCopy();
                    enemy.Config11 = Config11.DeepCopy();
                    enemy.Config12 = Config12.DeepCopy();
                }

                internal Enemy(BinaryReaderEx br) : base(br) { }

                private protected override void ReadBehaviorConfig(BinaryReaderEx br) => Behavior = new BehaviorConfig(br);
                private protected override void ReadEnemyConfig(BinaryReaderEx br) => EnemyData = new EnemyConfig(br);
                private protected override void ReadUnkConfig11(BinaryReaderEx br) => Config11 = new UnkConfig11(br);
                private protected override void ReadUnkConfig12(BinaryReaderEx br) => Config12 = new UnkConfig12(br);

                private protected override void WriteBehaviorConfig(BinaryWriterEx bw) => Behavior.Write(bw);
                private protected override void WriteEnemyConfig(BinaryWriterEx bw) => EnemyData.Write(bw);
                private protected override void WriteUnkConfig11(BinaryWriterEx bw) => Config11.Write(bw);
                private protected override void WriteUnkConfig12(BinaryWriterEx bw) => Config12.Write(bw);
            }

            /// <summary>
            /// Object models referenced in MSB files other than the map MSB.
            /// </summary>
            public class ExternalObject : Part
            {
                private protected override PartType Type => PartType.ExternalObject;
                private protected override bool HasBehaviorConfig => true;
                private protected override bool HasEnemyConfig => false;
                private protected override bool HasObjectConfig => false;
                private protected override bool HasUnkConfig11 => false;
                private protected override bool HasUnkConfig12 => false;

                public BehaviorConfig Behavior { get; set; }

                public ExternalObject() : base("oXXXX_XXXX")
                {
                    Behavior = new BehaviorConfig();
                }

                public ExternalObject(string name) : base(name)
                {
                    Behavior = new BehaviorConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var externalObj = (ExternalObject)part;
                    externalObj.Behavior = Behavior.DeepCopy();
                }

                internal ExternalObject(BinaryReaderEx br) : base(br) { }

                private protected override void ReadBehaviorConfig(BinaryReaderEx br) => Behavior = new BehaviorConfig(br);

                private protected override void WriteBehaviorConfig(BinaryWriterEx bw) => Behavior.Write(bw);
            }

            /// <summary>
            /// Unused instances of objects or map pieces that were left in.
            /// </summary>
            public class Unused : Part
            {
                private protected override PartType Type => PartType.Unused;
                private protected override bool HasBehaviorConfig => false;
                private protected override bool HasEnemyConfig => false;
                private protected override bool HasObjectConfig => false;
                private protected override bool HasUnkConfig11 => false;
                private protected override bool HasUnkConfig12 => false;

                public Unused() : base("oXXXX_XXXX") { }
                public Unused(string name) : base(name) { }

                internal Unused(BinaryReaderEx br) : base(br) { }
            }

            #endregion
        }

        #endregion
    }
}
