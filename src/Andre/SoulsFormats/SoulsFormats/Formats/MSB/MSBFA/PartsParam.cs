using System.Numerics;
using System.Collections.Generic;
using System;
using System.IO;

namespace SoulsFormats
{
    public partial class MSBFA
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
            /// Unused instances of objects or map pieces that were left in.
            /// </summary>
            Unused = -1
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
            public List<Part.Unused> Unused { get; set; }

            /// <summary>
            /// Creates an empty PartsParam with the default version.
            /// </summary>
            public PartsParam() : base(10001002, "PARTS_PARAM_ST")
            {
                MapPieces = new List<Part.MapPiece>();
                Objects = new List<Part.Object>();
                Enemies = new List<Part.Enemy>();
                Unused = new List<Part.Unused>();
            }

            /// <summary>
            /// Adds a <see cref="Part"/> to the appropriate list for its type and returns it.
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
            /// Returns every <see cref="Part"/> in the order they'll be written.
            /// </summary>
            public override List<Part> GetEntries() => SFUtil.ConcatAll<Part>(MapPieces, Objects, Enemies, Unused);
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
            /// Unknown; Affects rendering.
            /// </summary>
            public RenderConfig Render { get; set; }

            /// <summary>
            /// Holds event group IDs referenced in scripts for an MSB.
            /// </summary>
            public EventGroupConfig EventGroup { get; set; }

            /// <summary>
            /// Links this part to a parent part for animation.
            /// </summary>
            public AnimLinkConfig AnimLink { get; set; }

            /// <summary>
            /// Propagates destruction from a parent part to this part.
            /// </summary>
            public BreakLinkConfig BreakLink { get; set; }

            /// <summary>
            /// References a row ID in the charaeventdata param.<br/>
            /// May be null.
            /// </summary>
            public CharaEventDataConfig CharaEventData { get; set; }

            /// <summary>
            /// Controls what layers this part appears on.
            /// </summary>
            public LayerConfig Layer { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public UnkConfig10 Config10 { get; set; }

            /// <summary>
            /// Unknown.<br/>
            /// May be null.
            /// </summary>
            public UnkConfig11 Config11 { get; set; }

            /// <summary>
            /// Unknown.<br/>
            /// May be null.
            /// </summary>
            public UnkConfig12 Config12 { get; set; }

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
                Render = new RenderConfig();
                EventGroup = new EventGroupConfig();
                AnimLink = new AnimLinkConfig();
                BreakLink = new BreakLinkConfig();
                CharaEventData = new CharaEventDataConfig();
                Layer = new LayerConfig();
                Config10 = new UnkConfig10();
            }

            /// <summary>
            /// Creates a deep copy of the part.
            /// </summary>
            public Part DeepCopy()
            {
                var part = (Part)MemberwiseClone();
                part.Render = Render.DeepCopy();
                part.EventGroup = EventGroup.DeepCopy();
                part.AnimLink = AnimLink.DeepCopy();
                part.BreakLink = BreakLink.DeepCopy();

                if (CharaEventData != null)
                    part.CharaEventData = CharaEventData.DeepCopy();

                part.Layer = Layer.DeepCopy();
                part.Config10 = Config10.DeepCopy();

                if (Config11 != null)
                    part.Config11 = Config11.DeepCopy();

                if (Config12 != null)
                    part.Config12 = Config12.DeepCopy();

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
                int offsetRenderConfig = br.ReadInt32();
                int offsetEventGroupConfig = br.ReadInt32();
                int offsetAnimLinkConfig = br.ReadInt32();
                int offsetBreakLinkConfig = br.ReadInt32();
                int offsetCharaEventDataConfig = br.ReadInt32();
                int offsetObjectConfig = br.ReadInt32();
                int offsetLayerConfig = br.ReadInt32();
                int offsetUnkConfig10 = br.ReadInt32();
                int offsetUnkConfig11 = br.ReadInt32();
                int offsetUnkConfig12 = br.ReadInt32();
                br.AssertInt32(0);
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
                if (offsetRenderConfig == 0)
                    throw new InvalidDataException($"{nameof(offsetRenderConfig)} must not be 0 in type {GetType()}.");
                if (offsetEventGroupConfig == 0)
                    throw new InvalidDataException($"{nameof(offsetEventGroupConfig)} must not be 0 in type {GetType()}.");
                if (offsetAnimLinkConfig == 0)
                    throw new InvalidDataException($"{nameof(offsetAnimLinkConfig)} must not be 0 in type {GetType()}.");
                if (offsetBreakLinkConfig == 0)
                    throw new InvalidDataException($"{nameof(offsetBreakLinkConfig)} must not be 0 in type {GetType()}.");
                if (HasObjectConfig ^ offsetObjectConfig != 0)
                    throw new InvalidDataException($"Unexpected {nameof(offsetObjectConfig)} 0x{offsetObjectConfig:X} in type {GetType()}.");
                if (offsetLayerConfig == 0)
                    throw new InvalidDataException($"{nameof(offsetLayerConfig)} must not be 0 in type {GetType()}.");
                if (offsetUnkConfig10 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig10)} must not be 0 in type {GetType()}.");

                br.Position = start + nameOffset;
                Name = br.ReadShiftJIS();

                br.Position = start + modelNameOffset;
                ModelName = br.ReadShiftJIS();

                br.Position = start + resourceOffset;
                ResourcePath = br.ReadShiftJIS();

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

                br.Position = start + offsetRenderConfig;
                Render = new RenderConfig(br);

                br.Position = start + offsetEventGroupConfig;
                EventGroup = new EventGroupConfig(br);

                br.Position = start + offsetAnimLinkConfig;
                AnimLink = new AnimLinkConfig(br);

                br.Position = start + offsetBreakLinkConfig;
                BreakLink = new BreakLinkConfig(br);

                if (offsetCharaEventDataConfig > 0)
                {
                    br.Position = start + offsetCharaEventDataConfig;

                    bool isShort;
                    if (HasObjectConfig)
                    {
                        isShort = (offsetObjectConfig - offsetCharaEventDataConfig) == 4;
                    }
                    else
                    {
                        isShort = (offsetLayerConfig - offsetCharaEventDataConfig) == 4;
                    }

                    CharaEventData = new CharaEventDataConfig(br, isShort);
                }
                else
                {
                    CharaEventData = null;
                }

                if (HasObjectConfig)
                {
                    br.Position = start + offsetObjectConfig;
                    ReadObjectConfig(br);
                }

                br.Position = start + offsetLayerConfig;
                Layer = new LayerConfig(br);

                br.Position = start + offsetUnkConfig10;
                Config10 = new UnkConfig10(br);

                if (offsetUnkConfig11 > 0)
                {
                    br.Position = start + offsetUnkConfig11;
                    Config11 = new UnkConfig11(br);
                }
                else
                {
                    Config11 = null;
                }

                if (offsetUnkConfig12 > 0)
                {
                    br.Position = start + offsetUnkConfig12;
                    Config12 = new UnkConfig12(br);
                }
                else
                {
                    Config12 = null;
                }
            }

            private protected virtual void ReadBehaviorConfig(BinaryReaderEx br) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadBehaviorConfig)}.");
            private protected virtual void ReadEnemyConfig(BinaryReaderEx br) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadEnemyConfig)}.");
            private protected virtual void ReadObjectConfig(BinaryReaderEx br) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadObjectConfig)}.");

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
                bw.ReserveInt32("OffsetRender");
                bw.ReserveInt32("OffsetEventGroupConfig");
                bw.ReserveInt32("OffsetAnimLink");
                bw.ReserveInt32("OffsetBreakLink");
                bw.ReserveInt32("OffsetCharaEventData");
                bw.ReserveInt32("OffsetObjectConfig");
                bw.ReserveInt32("OffsetLayerConfig");
                bw.ReserveInt32("OffsetUnkConfig10");
                bw.ReserveInt32("OffsetUnkConfig11");
                bw.ReserveInt32("OffsetUnkConfig12");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(Name), true);
                // Not padded between strings

                bw.FillInt32("ModelNameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(ModelName, true);
                // Not padded between strings

                bw.FillInt32("ResourcePathOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(ResourcePath, true);
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

                bw.FillInt32("OffsetRender", (int)(bw.Position - start));
                Render.Write(bw);

                bw.FillInt32("OffsetEventGroupConfig", (int)(bw.Position - start));
                EventGroup.Write(bw);

                bw.FillInt32("OffsetAnimLink", (int)(bw.Position - start));
                AnimLink.Write(bw);

                bw.FillInt32("OffsetBreakLink", (int)(bw.Position - start));
                BreakLink.Write(bw);

                // TODO: Investigate this just in case we can avoid a bunch of null objects.
                if (CharaEventData != null)
                {
                    bw.FillInt32("OffsetCharaEventData", (int)(bw.Position - start));
                    CharaEventData.Write(bw);
                }
                else
                {
                    bw.FillInt32("OffsetCharaEventData", 0);
                }

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
                Layer.Write(bw);

                bw.FillInt32("OffsetUnkConfig10", (int)(bw.Position - start));
                Config10.Write(bw);

                // TODO: Investigate this just in case we can avoid a bunch of null objects.
                if (Config11 != null)
                {
                    bw.FillInt32("OffsetUnkConfig11", (int)(bw.Position - start));
                    Config11.Write(bw);
                }
                else
                {
                    bw.FillInt32("OffsetUnkConfig11", 0);
                }

                // TODO: Investigate this just in case we can avoid a bunch of null objects.
                if (Config12 != null)
                {
                    bw.FillInt32("OffsetUnkConfig12", (int)(bw.Position - start));
                    Config12.Write(bw);
                }
                else
                {
                    bw.FillInt32("OffsetUnkConfig12", 0);
                }
            }

            private protected virtual void WriteBehaviorConfig(BinaryWriterEx bw) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteBehaviorConfig)}.");
            private protected virtual void WriteEnemyConfig(BinaryWriterEx bw) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteEnemyConfig)}.");
            private protected virtual void WriteObjectConfig(BinaryWriterEx bw) => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(WriteObjectConfig)}.");

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
                    BehaviorScript = br.ReadShiftJIS();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long pos = bw.Position;
                    bw.ReserveInt32("PartBehaviorOffsetBehaviorScript");

                    bw.FillInt32("PartBehaviorOffsetBehaviorScript", (int)(bw.Position - pos));
                    bw.WriteShiftJIS(BehaviorScript, true);
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

                public EnemyConfig() { }

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
                    AIScript = br.ReadShiftJIS();

                    br.Position = start + offsetUnk;
                    Unk = br.ReadShiftJIS();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long start = bw.Position;
                    bw.ReserveInt32("PartEnemyOffsetAIScript");
                    bw.ReserveInt32("PartEnemyOffsetUnk");

                    bw.FillInt32("PartEnemyOffsetAIScript", (int)(bw.Position - start));
                    bw.WriteShiftJIS(AIScript, true);
                    // Not padded between strings

                    bw.FillInt32("PartEnemyOffsetUnk", (int)(bw.Position - start));
                    bw.WriteShiftJIS(Unk, true);
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// Unknown; Affects rendering.
            /// </summary>
            public class RenderConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk05 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk06 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk07 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk09 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk0A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk0B { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk12 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte Unk13 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk16 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk3C { get; set; }

                public RenderConfig()
                {
                    Unk00 = 0;
                    Unk01 = 0;
                    Unk02 = 0;
                    Unk03 = 0;
                    Unk04 = 1;
                    Unk05 = 1;
                    Unk06 = 0;
                    Unk07 = 1;
                    Unk08 = -128;
                    Unk09 = 0;
                    Unk0A = 0;
                    Unk0B = -1;
                    Unk0C = -256;
                    Unk10 = 1280;
                    Unk12 = 0;
                    Unk13 = 0;
                    Unk14 = 0;
                    Unk16 = 0;
                    Unk18 = 0;
                    Unk1C = 0;
                    Unk20 = 0;
                    Unk24 = 0;
                    Unk28 = 0;
                    Unk2C = 0;
                    Unk30 = 0;
                    Unk34 = 0;
                    Unk38 = 0;
                    Unk3C = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public RenderConfig DeepCopy()
                {
                    return (RenderConfig)MemberwiseClone();
                }

                internal RenderConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadByte();
                    Unk04 = br.ReadByte();
                    Unk05 = br.ReadByte();
                    Unk06 = br.ReadByte();
                    Unk07 = br.ReadByte();
                    Unk08 = br.ReadSByte();
                    Unk09 = br.ReadSByte();
                    Unk0A = br.ReadSByte();
                    Unk0B = br.ReadSByte();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt16();
                    Unk12 = br.ReadSByte();
                    Unk13 = br.ReadSByte();
                    Unk14 = br.ReadInt16();
                    Unk16 = br.ReadInt16();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteByte(Unk03);
                    bw.WriteByte(Unk04);
                    bw.WriteByte(Unk05);
                    bw.WriteByte(Unk06);
                    bw.WriteByte(Unk07);
                    bw.WriteSByte(Unk08);
                    bw.WriteSByte(Unk09);
                    bw.WriteSByte(Unk0A);
                    bw.WriteSByte(Unk0B);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt16(Unk10);
                    bw.WriteSByte(Unk12);
                    bw.WriteSByte(Unk13);
                    bw.WriteInt16(Unk14);
                    bw.WriteInt16(Unk16);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteInt32(Unk34);
                    bw.WriteInt32(Unk38);
                    bw.WriteInt32(Unk3C);
                }
            }

            /// <summary>
            /// Holds event group IDs referenced in scripts for an MSB. Required.
            /// </summary>
            public class EventGroupConfig
            {
                /// <summary>
                /// Event group IDs referenced in scripts.
                /// </summary>
                public short[] EventGroupIDs { get; private set; }

                public EventGroupConfig()
                {
                    EventGroupIDs = new short[4] { -1, -1, -1, -1 };
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
                    EventGroupIDs = br.ReadInt16s(4);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16s(EventGroupIDs);
                }
            }

            /// <summary>
            /// Links this part to a parent part for animation. Required.
            /// </summary>
            public class AnimLinkConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// References a parent part to link to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string LinkPartName { get; set; }

                /// <summary>
                /// References a parent part model to link to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Model))]
                public string LinkModelName { get; set; }

                public AnimLinkConfig()
                {
                    Unk08 = 0;
                    Unk0C = 0;
                    Unk10 = 0;
                    Unk14 = 0;
                    Unk18 = 0;
                    Unk1C = 0;
                    LinkPartName = "-1";
                    LinkModelName = string.Empty;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public AnimLinkConfig DeepCopy()
                {
                    return (AnimLinkConfig)MemberwiseClone();
                }

                internal AnimLinkConfig(BinaryReaderEx br)
                {
                    long pos = br.Position;
                    int nameOffset = br.ReadInt32();
                    int modelNameOffset = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();

                    br.Position = pos + nameOffset;
                    LinkPartName = br.ReadShiftJIS();

                    br.Position = pos + modelNameOffset;
                    LinkModelName = br.ReadShiftJIS();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long pos = bw.Position;
                    bw.ReserveInt32("OffsetPartNameUnkConfig5");
                    bw.ReserveInt32("OffsetModelNameUnkConfig5");
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);

                    bw.FillInt32("OffsetPartNameUnkConfig5", (int)(bw.Position - pos));
                    bw.WriteShiftJIS(LinkPartName, true);
                    // No padding between strings

                    bw.FillInt32("OffsetModelNameUnkConfig5", (int)(bw.Position - pos));
                    bw.WriteShiftJIS(LinkModelName, true);
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// Propagates destruction from a parent part to this part. Required.
            /// </summary>
            public class BreakLinkConfig
            {
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

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                /// <summary>
                /// The name of the parent to link to.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string LinkPartName { get; set; }

                public BreakLinkConfig()
                {
                    Unk04 = 0;
                    Unk08 = 0;
                    Unk0C = 0;
                    Unk10 = 0;
                    Unk14 = 0;
                    Unk18 = 0;
                    Unk1C = 0;
                    LinkPartName = "-1";
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public BreakLinkConfig DeepCopy()
                {
                    return (BreakLinkConfig)MemberwiseClone();
                }

                internal BreakLinkConfig(BinaryReaderEx br)
                {
                    long pos = br.Position;
                    int offset = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();

                    br.Position = pos + offset;
                    LinkPartName = br.ReadShiftJIS();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    long pos = bw.Position;
                    bw.ReserveInt32("OffsetUnkConfig6");
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);

                    bw.FillInt32("OffsetUnkConfig6", (int)(bw.Position - pos));
                    bw.WriteShiftJIS(LinkPartName, true);
                    bw.Pad(4);
                }
            }

            /// <summary>
            /// References a row ID in the charaeventdata param. Required.
            /// </summary>
            public class CharaEventDataConfig
            {
                /// <summary>
                /// The row ID of the character event data.
                /// </summary>
                [MSBParamReference(ParamName = "CharaEventData")]
                public int CharaEventDataID { get; set; }
                
                /// <summary>
                /// Unknown.
                /// </summary>
                // m040_hasem_test.msb's last two enemy part entries are missing this field.
                public int Unk04 { get; set; }

                /// <summary>
                /// Whether or not <see cref="CharaEventDataID"/> was the only field detected when reading, leaving no fields after.
                /// </summary>
                public bool IsShort { get; set; }

                public CharaEventDataConfig()
                {
                    CharaEventDataID = 0;
                    Unk04 = 0;
                    IsShort = false;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public CharaEventDataConfig DeepCopy()
                {
                    return (CharaEventDataConfig)MemberwiseClone();
                }

                internal CharaEventDataConfig(BinaryReaderEx br, bool isShort)
                {
                    CharaEventDataID = br.ReadInt32();

                    IsShort = isShort;
                    if (!isShort)
                    {
                        Unk04 = br.ReadInt32();
                    }
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CharaEventDataID);

                    if (!IsShort)
                    {
                        bw.WriteInt32(Unk04);
                    }
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
                    br.AssertPattern(16, 0);

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
                    bw.WritePattern(16, 0);

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
                    public int Unk08 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk0C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk10 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk14 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk18 { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk1C { get; set; }

                    /// <summary>
                    /// Unknown.
                    /// </summary>
                    public int Unk20 { get; set; }

                    public ObjectConfigBlock()
                    {
                        Unk00 = -1;
                        Unk01 = 0;
                        Unk02 = 0;
                        Unk03 = 0;
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
                        br.AssertInt32(32); // Offset? Seen as 8 in ACV where the struct is 24 bytes smaller.
                        Unk08 = br.ReadInt32();
                        Unk0C = br.ReadInt32();
                        Unk10 = br.ReadInt32();
                        Unk14 = br.ReadInt32();
                        Unk18 = br.ReadInt32();
                        Unk1C = br.ReadInt32();
                        Unk20 = br.ReadInt32();
                    }

                    internal void Write(BinaryWriterEx bw)
                    {
                        bw.WriteSByte(Unk00);
                        bw.WriteSByte(Unk01);
                        bw.WriteSByte(Unk02);
                        bw.WriteSByte(Unk03);
                        bw.WriteInt32(32); // Offset? Seen as 8 in ACV where the struct is 24 bytes smaller.
                        bw.WriteInt32(Unk08);
                        bw.WriteInt32(Unk0C);
                        bw.WriteInt32(Unk10);
                        bw.WriteInt32(Unk14);
                        bw.WriteInt32(Unk18);
                        bw.WriteInt32(Unk1C);
                        bw.WriteInt32(Unk20);
                    }
                }
            }

            /// <summary>
            /// Controls what layers this part appears on. Required.
            /// </summary>
            public class LayerConfig
            {
                /// <summary>
                /// Determines what layer this part will appear on.<br/>
                /// -1 means it appears on all layers.
                /// </summary>
                public short LayerID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk02 { get; set; }

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

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int Unk1C { get; set; }

                public LayerConfig()
                {
                    LayerID = -1;
                    Unk02 = 0;
                    Unk04 = 0;
                    Unk08 = 0;
                    Unk0C = 0;
                    Unk10 = 0;
                    Unk14 = 0;
                    Unk1C = 0;
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
                    LayerID = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(LayerID);
                    bw.WriteInt16(Unk02);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
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

                public UnkConfig11()
                {
                    Unk00 = 0;
                    Unk04 = 0;
                    Unk08 = 0;
                    Unk0C = 0;
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
            public class UnkConfig12
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

                public UnkConfig12()
                {
                    Unk00 = 0;
                    Unk04 = 0;
                    Unk08 = 0;
                    Unk0C = 0;
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

                public BehaviorConfig Behavior { get; set; }

                public MapPiece() : base("mXXXX")
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

                /// <summary>
                /// Behavior type data referencing scripts of some kind.
                /// </summary>
                public BehaviorConfig Behavior { get; set; }

                /// <summary>
                /// Unknown. Some kind of type data for objects.
                /// </summary>
                public ObjectConfig ObjectData { get; set; }

                public Object() : base("oXXXX_XXXX")
                {
                    Behavior = new BehaviorConfig();
                    ObjectData = new ObjectConfig();
                }

                public Object(string name) : base(name)
                {
                    Behavior = new BehaviorConfig();
                    ObjectData = new ObjectConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var obj = (Object)part;
                    obj.Behavior = Behavior.DeepCopy();
                    obj.ObjectData = ObjectData.DeepCopy();
                }

                internal Object(BinaryReaderEx br) : base(br) { }

                private protected override void ReadBehaviorConfig(BinaryReaderEx br) => Behavior = new BehaviorConfig(br);
                private protected override void ReadObjectConfig(BinaryReaderEx br) => ObjectData = new ObjectConfig(br);

                private protected override void WriteBehaviorConfig(BinaryWriterEx bw) => Behavior.Write(bw);
                private protected override void WriteObjectConfig(BinaryWriterEx bw) => ObjectData.Write(bw);
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

                /// <summary>
                /// Behavior type data referencing scripts of some kind.
                /// </summary>
                public BehaviorConfig Behavior { get; set; }

                /// <summary>
                /// Enemy type data referencing AI scripts.
                /// </summary>
                public EnemyConfig EnemyData { get; set; }

                public Enemy() : base("eXXXX_XX")
                {
                    Behavior = new BehaviorConfig();
                    EnemyData = new EnemyConfig();
                }

                public Enemy(string name) : base(name)
                {
                    Behavior = new BehaviorConfig();
                    EnemyData = new EnemyConfig();
                }

                private protected override void DeepCopyTo(Part part)
                {
                    var enemy = (Enemy)part;
                    enemy.Behavior = Behavior.DeepCopy();
                    enemy.EnemyData = EnemyData.DeepCopy();
                }

                internal Enemy(BinaryReaderEx br) : base(br) { }

                private protected override void ReadBehaviorConfig(BinaryReaderEx br) => Behavior = new BehaviorConfig(br);
                private protected override void ReadEnemyConfig(BinaryReaderEx br) => EnemyData = new EnemyConfig(br);

                private protected override void WriteBehaviorConfig(BinaryWriterEx bw) => Behavior.Write(bw);
                private protected override void WriteEnemyConfig(BinaryWriterEx bw) => EnemyData.Write(bw);
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

                public Unused() : base("oXXXX_XXXX") { }
                public Unused(string name) : base(name) { }

                internal Unused(BinaryReaderEx br) : base(br) { }
            }

            #endregion
        }

        #endregion
    }
}
