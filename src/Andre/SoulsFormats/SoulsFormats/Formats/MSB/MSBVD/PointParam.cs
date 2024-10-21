using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBVD
    {
        /// <summary>
        /// The different types of regions.
        /// </summary>
        internal enum RegionType
        {
            /// <summary>
            /// Unknown; Seems to be a default type.
            /// </summary>
            Default = 0,

            /// <summary>
            /// A route start or goal point referenced by a route.
            /// </summary>
            RoutePoint = 1,

            /// <summary>
            /// Unknown; Seems to usually reference trigger or action bounds?
            /// </summary>
            Action = 50,

            /// <summary>
            /// A bounding area that, if left, causes the player to fail or die.
            /// </summary>
            OperationalArea = 100,

            /// <summary>
            /// A bounding area that will show a warning to the player if the player has left the normal area.
            /// </summary>
            WarningArea = 101,

            /// <summary>
            /// The normal bounding area the player plays in.
            /// </summary>
            AttentionArea = 102,

            /// <summary>
            /// A spawn point.
            /// </summary>
            Spawn = 200,

            /// <summary>
            /// A scrap collection point for collecting scrap parts the player can use.
            /// <para>Similar to treasure or items.</para>
            /// </summary>
            ScrapCollection = 250,

            /// <summary>
            /// An SFX region of some kind.
            /// </summary>
            SFX = 500,

            /// <summary>
            /// Unknown; Called AC sumbmersion return start and end points; Might reference drowning somehow.
            /// </summary>
            Submersion = 850,

            /// <summary>
            /// A sound trigger region of some kind.
            /// </summary>
            Sound = 1000,

            /// <summary>
            /// A sound reverb region of some kind.
            /// </summary>
            Reverb = 1100,

            /// <summary>
            /// A light point of some kind.
            /// </summary>
            Light = 1300,

            /// <summary>
            /// Unknown; Usually named spot.
            /// </summary>
            Spot = 1310,

            /// <summary>
            /// Unknown; Related to a landing of some kind.
            /// </summary>
            Landing = 2000,

            /// <summary>
            /// Unknown; Usually named debug navigation for AI.
            /// </summary>
            DebugNavigation = 3000,

            /// <summary>
            /// Unused points?
            /// </summary>
            Unused = -1
        }

        /// <summary>
        /// A collection of points and trigger volumes used by scripts and events.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion>
        {
            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.DefaultRegion> DefaultRegions { get; set; }

            /// <summary>
            /// Route start and end points referenced by routes.
            /// </summary>
            public List<Region.RoutePoint> RoutePoints { get; set; }

            /// <summary>
            /// Unknown; Seems to usually reference trigger or action bounds?
            /// </summary>
            public List<Region.Action> Actions { get; set; }

            /// <summary>
            /// Bounds the player must be in or they will fail.
            /// </summary>
            public List<Region.OperationalArea> OperationalAreas { get; set; }

            /// <summary>
            /// Bounds that show a warning if the player has left normal play areas.
            /// </summary>
            public List<Region.WarningArea> WarningAreas { get; set; }

            /// <summary>
            /// Bounds the player will be in for normal gameplay.
            /// </summary>
            public List<Region.AttentionArea> AttentionAreas { get; set; }

            /// <summary>
            /// Spawn points; Usually for the player.
            /// </summary>
            public List<Region.SpawnPoint> SpawnPoints { get; set; }

            /// <summary>
            /// Points the player can collect scrap parts at to use later.
            /// <para>Similar to treasure or items.</para>
            /// </summary>
            public List<Region.ScrapCollectionPoint> ScrapCollectionPoints { get; set; }

            /// <summary>
            /// SFX points of some kind.
            /// </summary>
            public List<Region.SFXRegion> SFXRegions { get; set; }

            /// <summary>
            /// Unknown; Called AC sumbmersion return start and end points; Might reference drowning somehow.
            /// <para>Has been referenced by routes instead of route points.</para>
            /// </summary>
            public List<Region.SubmersionRegion> SubmersionRegions { get; set; }

            /// <summary>
            /// Sound triggers of some kind.
            /// </summary>
            public List<Region.SoundRegion> Sounds { get; set; }

            /// <summary>
            /// Sound reverb areas of some kind.
            /// </summary>
            public List<Region.ReverbRegion> ReverbRegions { get; set; }

            /// <summary>
            /// Light points of some kind.
            /// </summary>
            public List<Region.LightRegion> LightRegions { get; set; }

            /// <summary>
            /// Unknown; Usually named spot.
            /// </summary>
            public List<Region.SpotRegion> SpotRegions { get; set; }

            /// <summary>
            /// Unknown; Related to landing somehow.
            /// </summary>
            public List<Region.LandingRegion> LandingRegions { get; set; }

            /// <summary>
            /// Unknown; Related to landing somehow.
            /// </summary>
            public List<Region.DebugNavigationRegion> DebugNavigationRegions { get; set; }

            /// <summary>
            /// Unused points?
            /// </summary>
            public List<Region.UnusedRegion> UnusedRegions { get; set; }

            /// <summary>
            /// Creates a new, empty PointParam with default values.
            /// </summary>
            public PointParam() : base(10001002, "POINT_PARAM_ST")
            {
                DefaultRegions = new List<Region.DefaultRegion>();
                RoutePoints = new List<Region.RoutePoint>();
                Actions = new List<Region.Action>();
                OperationalAreas = new List<Region.OperationalArea>();
                WarningAreas = new List<Region.WarningArea>();
                AttentionAreas = new List<Region.AttentionArea>();
                SpawnPoints = new List<Region.SpawnPoint>();
                ScrapCollectionPoints = new List<Region.ScrapCollectionPoint>();
                SFXRegions = new List<Region.SFXRegion>();
                SubmersionRegions = new List<Region.SubmersionRegion>();
                Sounds = new List<Region.SoundRegion>();
                ReverbRegions = new List<Region.ReverbRegion>();
                LightRegions = new List<Region.LightRegion>();
                SpotRegions = new List<Region.SpotRegion>();
                LandingRegions = new List<Region.LandingRegion>();
                DebugNavigationRegions = new List<Region.DebugNavigationRegion>();
                UnusedRegions = new List<Region.UnusedRegion>();
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region)
            {
                switch (region)
                {
                    case Region.DefaultRegion r: DefaultRegions.Add(r); break;
                    case Region.RoutePoint r: RoutePoints.Add(r); break;
                    case Region.Action r: Actions.Add(r); break;
                    case Region.OperationalArea r: OperationalAreas.Add(r); break;
                    case Region.WarningArea r: WarningAreas.Add(r); break;
                    case Region.AttentionArea r: AttentionAreas.Add(r); break;
                    case Region.SpawnPoint r: SpawnPoints.Add(r); break;
                    case Region.ScrapCollectionPoint r: ScrapCollectionPoints.Add(r); break;
                    case Region.SFXRegion r: SFXRegions.Add(r); break;
                    case Region.SubmersionRegion r: SubmersionRegions.Add(r); break;
                    case Region.SoundRegion r: Sounds.Add(r); break;
                    case Region.ReverbRegion r: ReverbRegions.Add(r); break;
                    case Region.LightRegion r: LightRegions.Add(r); break;
                    case Region.SpotRegion r: SpotRegions.Add(r); break;
                    case Region.LandingRegion r: LandingRegions.Add(r); break;
                    case Region.DebugNavigationRegion r: DebugNavigationRegions.Add(r); break;
                    case Region.UnusedRegion r: UnusedRegions.Add(r); break;
                    default: throw new ArgumentException($"Unrecognized type {region.GetType()}.", nameof(region));
                }
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion region) => Add((Region)region);

            /// <summary>
            /// Returns every region in the order they'll be written.
            /// </summary>
            public override List<Region> GetEntries() => SFUtil.ConcatAll<Region>(DefaultRegions, RoutePoints, Actions, OperationalAreas, WarningAreas, AttentionAreas,
                                                                                  SpawnPoints, ScrapCollectionPoints, SFXRegions, SubmersionRegions, Sounds,
                                                                                  ReverbRegions, LightRegions, SpotRegions, LandingRegions, DebugNavigationRegions, UnusedRegions);
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 8);
                switch (type)
                {
                    case RegionType.Default:
                        return DefaultRegions.EchoAdd(new Region.DefaultRegion(br));
                    case RegionType.RoutePoint:
                        return RoutePoints.EchoAdd(new Region.RoutePoint(br));
                    case RegionType.Action:
                        return Actions.EchoAdd(new Region.Action(br));
                    case RegionType.OperationalArea:
                        return OperationalAreas.EchoAdd(new Region.OperationalArea(br));
                    case RegionType.WarningArea:
                        return WarningAreas.EchoAdd(new Region.WarningArea(br));
                    case RegionType.AttentionArea:
                        return AttentionAreas.EchoAdd(new Region.AttentionArea(br));
                    case RegionType.Spawn:
                        return SpawnPoints.EchoAdd(new Region.SpawnPoint(br));
                    case RegionType.ScrapCollection:
                        return ScrapCollectionPoints.EchoAdd(new Region.ScrapCollectionPoint(br));
                    case RegionType.SFX:
                        return SFXRegions.EchoAdd(new Region.SFXRegion(br));
                    case RegionType.Submersion:
                        return SubmersionRegions.EchoAdd(new Region.SubmersionRegion(br));
                    case RegionType.Sound:
                        return Sounds.EchoAdd(new Region.SoundRegion(br));
                    case RegionType.Reverb:
                        return ReverbRegions.EchoAdd(new Region.ReverbRegion(br));
                    case RegionType.Light:
                        return LightRegions.EchoAdd(new Region.LightRegion(br));
                    case RegionType.Spot:
                        return SpotRegions.EchoAdd(new Region.SpotRegion(br));
                    case RegionType.Landing:
                        return LandingRegions.EchoAdd(new Region.LandingRegion(br));
                    case RegionType.DebugNavigation:
                        return DebugNavigationRegions.EchoAdd(new Region.DebugNavigationRegion(br));
                    case RegionType.Unused:
                        return UnusedRegions.EchoAdd(new Region.UnusedRegion(br));
                    default:
                        throw new NotImplementedException($"Unimplemented region type: {type}");
                }
            }
        }

        /// <summary>
        /// A point or volume that triggers some sort of interaction.
        /// </summary>
        public abstract class Region : ParamEntry, IMsbRegion
        {
            /// <summary>
            /// The type of the region.
            /// </summary>
            private protected abstract RegionType Type { get; }

            /// <summary>
            /// Whether or not a region has type data.
            /// </summary>
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// Describes the space encompassed by the region.
            /// </summary>
            public MSB.Shape Shape
            {
                get => _shape;
                set
                {
                    if (value is MSB.Shape.Composite)
                        throw new ArgumentException("Armored Core Verdict Day does not support composite shapes.");
                    _shape = value;
                }
            }
            private MSB.Shape _shape;

            /// <summary>
            /// Location of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Rotation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Identifies this point uniquely by id regardless of type.
            /// <para>Used by routes.</para>
            /// </summary>
            public int UniqueID { get; set; }

            /// <summary>
            /// Identifies the region in external files such as scripts.
            /// <para>Set to -1 when unused.</para>
            /// </summary>
            public int PointID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public UnkConfig1 Config1 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public UnkConfig2 Config2 { get; set; }

            /// <summary>
            /// Has a value for what layer the point is on.
            /// </summary>
            public LayerConfig Layer { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private UnkConfig4 Config4 { get; set; }

            private protected Region(string name)
            {
                Name = name;
                Shape = new MSB.Shape.Point();
                UniqueID = 0;
                PointID = -1;
                Config1 = new UnkConfig1();
                Config2 = new UnkConfig2();
                Layer = new LayerConfig();
                Config4 = new UnkConfig4();
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy()
            {
                var region = (Region)MemberwiseClone();
                region.Shape = Shape.DeepCopy();
                region.Config1 = Config1.DeepCopy();
                region.Config2 = Config2.DeepCopy();
                region.Layer = Layer.DeepCopy();
                region.Config4 = Config4.DeepCopy();
                DeepCopyTo(region);
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Region region) { }

            private protected Region(BinaryReaderEx br)
            {
                long start = br.Position;

                int nameOffset = br.ReadInt32();
                UniqueID = br.ReadInt32();
                br.AssertInt32((int)Type);
                br.ReadInt32(); // ID
                MSB.ShapeType shapeType = br.ReadEnum32<MSB.ShapeType>();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                int offsetUnkConfig1 = br.ReadInt32();
                int offsetUnkConfig2 = br.ReadInt32();
                PointID = br.ReadInt32();
                int shapeDataOffset = br.ReadInt32();
                int offsetUnkConfig3 = br.ReadInt32();
                int offsetUnkConfig4 = br.ReadInt32();
                int typeDataOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Shape = MSB.Shape.Create(shapeType);

                if (nameOffset == 0)
                    throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                if (offsetUnkConfig1 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig1)} must not be 0 in type {GetType()}.");
                if (offsetUnkConfig2 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig2)} must not be 0 in type {GetType()}.");
                if (Shape.HasShapeData ^ shapeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(shapeDataOffset)} 0x{shapeDataOffset:X} in type {GetType()}.");
                if (offsetUnkConfig3 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig3)} must not be 0 in type {GetType()}.");
                if (offsetUnkConfig4 == 0)
                    throw new InvalidDataException($"{nameof(offsetUnkConfig4)} must not be 0 in type {GetType()}.");
                if (HasTypeData ^ typeDataOffset != 0)
                    throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");
                
                br.Position = start + nameOffset;
                Name = br.ReadShiftJIS();

                br.Position = start + offsetUnkConfig1;
                Config1 = new UnkConfig1(br);

                br.Position = start + offsetUnkConfig2;
                Config2 = new UnkConfig2(br);

                if (Shape.HasShapeData)
                {
                    br.Position = start + shapeDataOffset;
                    Shape.ReadShapeData(br);
                }

                br.Position = start + offsetUnkConfig3;
                Layer = new LayerConfig(br);

                br.Position = start + offsetUnkConfig4;
                Config4 = new UnkConfig4(br);

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
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
                bw.WriteUInt32((uint)Shape.Type);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.ReserveInt32("OffsetUnkConfig1");
                bw.ReserveInt32("OffsetUnkConfig2");
                bw.WriteInt32(PointID);
                bw.ReserveInt32("ShapeDataOffset");
                bw.ReserveInt32("OffsetUnkConfig3");
                bw.ReserveInt32("OffsetUnkConfig4");
                bw.ReserveInt32("TypeDataOffset");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.FillInt32("NameOffset", (int)(bw.Position - start));
                bw.WriteShiftJIS(MSB.ReambiguateName(Name), true);
                bw.Pad(4);

                bw.FillInt32("OffsetUnkConfig1", (int)(bw.Position - start));
                Config1.Write(bw);

                bw.FillInt32("OffsetUnkConfig2", (int)(bw.Position - start));
                Config2.Write(bw);

                if (Shape.HasShapeData)
                {
                    bw.FillInt32("ShapeDataOffset", (int)(bw.Position - start));
                    Shape.WriteShapeData(bw);
                }
                else
                {
                    bw.FillInt32("ShapeDataOffset", 0);
                }

                bw.FillInt32("OffsetUnkConfig3", (int)(bw.Position - start));
                Layer.Write(bw);

                bw.FillInt32("OffsetUnkConfig4", (int)(bw.Position - start));
                Config4.Write(bw);

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

            #region Region Sub Structs

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkConfig1
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk02 { get; set; }

                public UnkConfig1()
                {
                    Unk00 = 0;
                    Unk02 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig1 DeepCopy()
                {
                    return (UnkConfig1)MemberwiseClone();
                }

                internal UnkConfig1(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Unk00);
                    bw.WriteInt16(Unk02);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class UnkConfig2
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short Unk02 { get; set; }

                public UnkConfig2()
                {
                    Unk00 = 0;
                    Unk02 = 0;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public UnkConfig2 DeepCopy()
                {
                    return (UnkConfig2)MemberwiseClone();
                }

                internal UnkConfig2(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt16();
                    Unk02 = br.ReadInt16();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt16(Unk00);
                    bw.WriteInt16(Unk02);
                }
            }

            /// <summary>
            /// Has a value for what layer the point is on.
            /// </summary>
            public class LayerConfig
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
                /// The layer this point appears on, set to -1 for all layers.
                /// </summary>
                public sbyte LayerID { get; set; }

                public LayerConfig()
                {
                    Unk00 = -1;
                    Unk01 = -1;
                    Unk02 = 0;
                    LayerID = -1;
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
                    Unk00 = br.ReadSByte();
                    Unk01 = br.ReadSByte();
                    Unk02 = br.ReadSByte();
                    LayerID = br.ReadSByte();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteSByte(Unk00);
                    bw.WriteSByte(Unk01);
                    bw.WriteSByte(Unk02);
                    bw.WriteSByte(LayerID);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            private class UnkConfig4
            {
                internal UnkConfig4(){}

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                internal UnkConfig4 DeepCopy()
                {
                    return (UnkConfig4)MemberwiseClone();
                }

                internal UnkConfig4(BinaryReaderEx br)
                {
                    br.AssertInt32(-1);
                    br.AssertInt32(12);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(-1);
                    bw.WriteInt32(12);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Typedata for area points.
            /// </summary>
            public class AreaConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public int Level { get; set; }

                /// <summary>
                /// Unknown; Was set to true on an "underground" level.
                /// </summary>
                public bool Unk04 { get; set; }

                /// <summary>
                /// Unknown; Was set to true on an "underground" level.
                /// </summary>
                public bool Unk05 { get; set; }

                public AreaConfig()
                {
                    Level = 0;
                    Unk04 = false;
                    Unk05 = false;
                }

                public AreaConfig(int level)
                {
                    Level = level;
                    Unk04 = false;
                    Unk05 = false;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public AreaConfig DeepCopy()
                {
                    return (AreaConfig)MemberwiseClone();
                }

                internal AreaConfig(BinaryReaderEx br)
                {
                    Level = br.ReadInt32();
                    Unk04 = br.ReadBoolean();
                    Unk05 = br.ReadBoolean();
                    br.AssertPattern(26, 0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Level);
                    bw.WriteBoolean(Unk04);
                    bw.WriteBoolean(Unk05);
                    bw.WritePattern(26, 0);
                }
            }

            /// <summary>
            /// Type data for spawn points.
            /// </summary>
            public class SpawnConfig
            {
                /// <summary>
                /// Unknown; Usually seems to be the index of the spawn point.
                /// </summary>
                public byte ID { get; set; }

                public SpawnConfig()
                {
                    ID = 0;
                }

                public SpawnConfig(byte id)
                {
                    ID = id;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public SpawnConfig DeepCopy()
                {
                    return (SpawnConfig)MemberwiseClone();
                }

                internal SpawnConfig(BinaryReaderEx br)
                {
                    ID = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteByte(ID);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }
            }

            /// <summary>
            /// Type data for scrap collection points.
            /// </summary>
            public class ScrapCollectionConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk00 { get; set; }

                public ScrapCollectionConfig()
                {
                    Unk00 = 6;
                }

                public ScrapCollectionConfig(byte unk00)
                {
                    Unk00 = unk00;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public ScrapCollectionConfig DeepCopy()
                {
                    return (ScrapCollectionConfig)MemberwiseClone();
                }

                internal ScrapCollectionConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }
            }

            /// <summary>
            /// Type data for SFX points.
            /// </summary>
            public class SFXConfig
            {
                /// <summary>
                /// The ID of the SFX to use.
                /// </summary>
                public int SFXID { get; set; }

                public SFXConfig()
                {
                    SFXID = -1;
                }

                public SFXConfig(int sfxID)
                {
                    SFXID = sfxID;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public SFXConfig DeepCopy()
                {
                    return (SFXConfig)MemberwiseClone();
                }

                internal SFXConfig(BinaryReaderEx br)
                {
                    SFXID = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SFXID);
                }
            }

            /// <summary>
            /// Type data for sound points.
            /// </summary>
            public class SoundConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Unk00 { get; set; }

                /// <summary>
                /// The ID of the sound to play.
                /// </summary>
                public int SoundID { get; set; }

                public SoundConfig()
                {
                    Unk00 = 5;
                    SoundID = -1;
                }

                public SoundConfig(int soundID)
                {
                    Unk00 = 5;
                    SoundID = soundID;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public SoundConfig DeepCopy()
                {
                    return (SoundConfig)MemberwiseClone();
                }

                internal SoundConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                    SoundID = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteByte(Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteInt32(SoundID);
                }
            }

            public class ReverbConfig
            {
                /// <summary>
                /// Unknown; Reverb type?
                /// </summary>
                public int Unk00 { get; set; }

                public ReverbConfig()
                {
                    Unk00 = 4;
                }

                public ReverbConfig(int unk00)
                {
                    Unk00 = unk00;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public ReverbConfig DeepCopy()
                {
                    return (ReverbConfig)MemberwiseClone();
                }

                internal ReverbConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }
            }

            /// <summary>
            /// Type data for light points.
            /// </summary>
            public class LightConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk28 { get; set; }

                public LightConfig()
                {
                    Unk00 = true;
                    Unk04 = 1f;
                    Unk08 = 1.5f;
                    Unk0C = 1.4f;
                    Unk10 = 1f;
                    Unk14 = 4f;
                    Unk18 = 1f;
                    Unk1C = 1f;
                    Unk20 = 1f;
                    Unk24 = 1f;
                    Unk28 = 1f;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public LightConfig DeepCopy()
                {
                    return (LightConfig)MemberwiseClone();
                }

                internal LightConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
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
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteBoolean(Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
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
                }
            }

            /// <summary>
            /// Type data for spot points.
            /// </summary>
            public class SpotConfig
            {
                /// <summary>
                /// Unknown.
                /// </summary>
                public bool Unk00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float Unk38 { get; set; }

                public SpotConfig()
                {
                    Unk00 = true;
                    Unk04 = 1f;
                    Unk08 = 5f;
                    Unk0C = 5f;
                    Unk10 = 5f;
                    Unk14 = 1f;
                    Unk18 = 1f;
                    Unk1C = 1f;
                    Unk20 = 1f;
                    Unk24 = 1f;
                    Unk28 = 1f;
                    Unk2C = 0.01f;
                    Unk30 = 30f;
                    Unk34 = 70f;
                    Unk38 = 90f;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public SpotConfig DeepCopy()
                {
                    return (SpotConfig)MemberwiseClone();
                }

                internal SpotConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
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
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteBoolean(Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
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
                }
            }

            /// <summary>
            /// Type data for landing points.
            /// </summary>
            public class LandingConfig
            {
                /// <summary>
                /// Unknown. Always -1.
                /// </summary>
                public sbyte Unk00 { get; set; }

                public LandingConfig()
                {
                    Unk00 = -1;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public LandingConfig DeepCopy()
                {
                    return (LandingConfig)MemberwiseClone();
                }

                internal LandingConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadSByte();
                    br.AssertByte(0);
                    br.AssertByte(0);
                    br.AssertByte(0);
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteSByte(Unk00);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }
            }

            #endregion

            #region RegionType Structs

            /// <summary>
            /// Unknown; Seems to be a default type.
            /// </summary>
            public class DefaultRegion : Region
            {
                private protected override RegionType Type => RegionType.Default;
                private protected override bool HasTypeData => false;

                public DefaultRegion() : base("default"){}

                private protected override void DeepCopyTo(Region region){}

                internal DefaultRegion(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A route start or goal point referenced by a route.
            /// </summary>
            public class RoutePoint : Region
            {
                private protected override RegionType Type => RegionType.RoutePoint;
                private protected override bool HasTypeData => false;

                public RoutePoint() : base("route") { }

                private protected override void DeepCopyTo(Region region) { }

                internal RoutePoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; Seems to usually reference trigger or action bounds?
            /// </summary>
            public class Action : Region
            {
                private protected override RegionType Type => RegionType.Action;
                private protected override bool HasTypeData => false;

                public Action() : base("action") { }

                private protected override void DeepCopyTo(Region region) { }

                internal Action(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A bounding area that, if left, causes the player to fail or die.
            /// </summary>
            public class OperationalArea : Region
            {
                private protected override RegionType Type => RegionType.OperationalArea;
                private protected override bool HasTypeData => true;

                public AreaConfig Area { get; set; }

                public OperationalArea() : base("operational area")
                {
                    Area = new AreaConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var operationalArea = (OperationalArea)region;
                    operationalArea.Area = Area.DeepCopy();
                }

                internal OperationalArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Area = new AreaConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Area.Write(bw);
            }

            /// <summary>
            /// A bounding area that will show a warning to the player if the player has left the normal area.
            /// </summary>
            public class WarningArea : Region
            {
                private protected override RegionType Type => RegionType.WarningArea;
                private protected override bool HasTypeData => true;

                public AreaConfig Area { get; set; }

                public WarningArea() : base("warning area")
                {
                    Area = new AreaConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var warningArea = (WarningArea)region;
                    warningArea.Area = Area.DeepCopy();
                }

                internal WarningArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Area = new AreaConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Area.Write(bw);
            }

            /// <summary>
            /// The normal bounding area the player plays in.
            /// </summary>
            public class AttentionArea : Region
            {
                private protected override RegionType Type => RegionType.AttentionArea;
                private protected override bool HasTypeData => true;

                public AreaConfig Area { get; set; }

                public AttentionArea() : base("attention area")
                {
                    Area = new AreaConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var attentionArea = (AttentionArea)region;
                    attentionArea.Area = Area.DeepCopy();
                }

                internal AttentionArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Area = new AreaConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Area.Write(bw);
            }

            /// <summary>
            /// A spawn point.
            /// </summary>
            public class SpawnPoint : Region
            {
                private protected override RegionType Type => RegionType.Spawn;
                private protected override bool HasTypeData => true;

                public SpawnConfig Spawn { get; set; }

                public SpawnPoint() : base("spawn point")
                {
                    Spawn = new SpawnConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var spawn = (SpawnPoint)region;
                    spawn.Spawn = Spawn.DeepCopy();
                }

                internal SpawnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Spawn = new SpawnConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Spawn.Write(bw);
            }

            /// <summary>
            /// A scrap collection point for collecting scrap parts the player can use.
            /// <para>Similar to treasure or items.</para>
            /// </summary>
            public class ScrapCollectionPoint : Region
            {
                private protected override RegionType Type => RegionType.ScrapCollection;
                private protected override bool HasTypeData => true;

                public ScrapCollectionConfig ScrapCollection { get; set; }

                public ScrapCollectionPoint() : base("scrap collection")
                {
                    ScrapCollection = new ScrapCollectionConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var scrapCollection = (ScrapCollectionPoint)region;
                    scrapCollection.ScrapCollection = ScrapCollection.DeepCopy();
                }

                internal ScrapCollectionPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => ScrapCollection = new ScrapCollectionConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => ScrapCollection.Write(bw);
            }

            /// <summary>
            /// An SFX region of some kind.
            /// </summary>
            public class SFXRegion : Region
            {
                private protected override RegionType Type => RegionType.SFX;
                private protected override bool HasTypeData => true;

                public SFXConfig SFX { get; set; }

                public SFXRegion() : base("sfx")
                {
                    SFX = new SFXConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var sfx = (SFXRegion)region;
                    sfx.SFX = SFX.DeepCopy();
                }

                internal SFXRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => SFX = new SFXConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => SFX.Write(bw);
            }

            /// <summary>
            /// Unknown; Called AC sumbmersion return start and end points; Might reference drowning somehow.
            /// </summary>
            public class SubmersionRegion : Region
            {
                private protected override RegionType Type => RegionType.Submersion;
                private protected override bool HasTypeData => false;

                public SubmersionRegion() : base("submersion") { }

                private protected override void DeepCopyTo(Region region) { }

                internal SubmersionRegion(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A sound trigger region of some kind.
            /// </summary>
            public class SoundRegion : Region
            {
                private protected override RegionType Type => RegionType.Sound;
                private protected override bool HasTypeData => true;

                public SoundConfig Sound { get; set; }

                public SoundRegion() : base("sound")
                {
                    Sound = new SoundConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var sound = (SoundRegion)region;
                    sound.Sound = Sound.DeepCopy();
                }

                internal SoundRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Sound = new SoundConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Sound.Write(bw);
            }

            /// <summary>
            /// A sound reverb region of some kind.
            /// </summary>
            public class ReverbRegion : Region
            {
                private protected override RegionType Type => RegionType.Reverb;
                private protected override bool HasTypeData => true;

                public ReverbConfig Reverb { get; set; }

                public ReverbRegion() : base("reverb")
                {
                    Reverb = new ReverbConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var reverb = (ReverbRegion)region;
                    reverb.Reverb = Reverb.DeepCopy();
                }

                internal ReverbRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Reverb = new ReverbConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Reverb.Write(bw);
            }

            /// <summary>
            /// A light point of some kind.
            /// </summary>
            public class LightRegion : Region
            {
                private protected override RegionType Type => RegionType.Light;
                private protected override bool HasTypeData => true;

                public LightConfig Light { get; set; }

                public LightRegion() : base("light")
                {
                    Light = new LightConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var light = (LightRegion)region;
                    light.Light = Light.DeepCopy();
                }

                internal LightRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Light = new LightConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Light.Write(bw);
            }

            /// <summary>
            /// Unknown; Usually named spot.
            /// </summary>
            public class SpotRegion : Region
            {
                private protected override RegionType Type => RegionType.Spot;
                private protected override bool HasTypeData => true;

                public SpotConfig Spot { get; set; }

                public SpotRegion() : base("spot")
                {
                    Spot = new SpotConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var spot = (SpotRegion)region;
                    spot.Spot = Spot.DeepCopy();
                }

                internal SpotRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Spot = new SpotConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Spot.Write(bw);
            }

            /// <summary>
            /// Unknown; Related to a landing of some kind.
            /// </summary>
            public class LandingRegion : Region
            {
                private protected override RegionType Type => RegionType.Landing;
                private protected override bool HasTypeData => true;

                public LandingConfig Landing { get; set; }

                public LandingRegion() : base("landing")
                {
                    Landing = new LandingConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var landing = (LandingRegion)region;
                    landing.Landing = Landing.DeepCopy();
                }

                internal LandingRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Landing = new LandingConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Landing.Write(bw);
            }

            /// <summary>
            /// Unknown; Usually named debug navigation for AI.
            /// </summary>
            public class DebugNavigationRegion : Region
            {
                private protected override RegionType Type => RegionType.DebugNavigation;
                private protected override bool HasTypeData => false;

                public DebugNavigationRegion() : base("debug navigation") { }

                private protected override void DeepCopyTo(Region region) { }

                internal DebugNavigationRegion(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unused points?
            /// </summary>
            public class UnusedRegion : Region
            {
                private protected override RegionType Type => RegionType.Unused;
                private protected override bool HasTypeData => false;

                public UnusedRegion() : base("") { }

                private protected override void DeepCopyTo(Region region) { }

                internal UnusedRegion(BinaryReaderEx br) : base(br) { }
            }

            #endregion
        }
    }
}
