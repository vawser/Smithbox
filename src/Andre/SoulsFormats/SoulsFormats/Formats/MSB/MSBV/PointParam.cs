using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBV
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
            /// Unknown; A save point?
            /// </summary>
            Save = 260,

            /// <summary>
            /// Unknown; AI collision avoidance?
            /// </summary>
            CollisionAvoidance = 270,

            /// <summary>
            /// Unknown; An area for communication?
            /// </summary>
            CommunicationArea = 280,

            /// <summary>
            /// Unknown; Water surface change?
            /// </summary>
            WaterSurface = 400,

            /// <summary>
            /// An SFX region of some kind.
            /// </summary>
            SFX = 500,

            /// <summary>
            /// Unknown; An entity generator of some kind?
            /// </summary>
            Gen = 650,

            /// <summary>
            /// A sound trigger region of some kind.
            /// </summary>
            Sound = 1000,

            /// <summary>
            /// A sound reverb region of some kind.
            /// </summary>
            Reverb = 1100,

            /// <summary>
            /// Unknown; Related to a landing of some kind.
            /// </summary>
            Landing = 2000,

            /// <summary>
            /// Unknown; Usually named debug navigation for AI.
            /// </summary>
            DebugNavigation = 3000,

            /// <summary>
            /// Unknown; Load measurement?
            /// </summary>
            LoadMeasurement = 3100,

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
            /// Unknown; Save points?.
            /// </summary>
            public List<Region.SaveRegion> SaveRegions { get; set; }

            /// <summary>
            /// Unknown; AI collision avoidance areas?.
            /// </summary>
            public List<Region.CollisionAvoidanceRegion> CollisionAvoidanceRegions { get; set; }

            /// <summary>
            /// Unknown; An area for communication?.
            /// </summary>
            public List<Region.CommunicationArea> CommunicationAreas { get; set; }

            /// <summary>
            /// Unknown; Water surface level changes?.
            /// </summary>
            public List<Region.WaterSurfaceRegion> WaterSurfaces { get; set; }

            /// <summary>
            /// SFX points of some kind.
            /// </summary>
            public List<Region.SFXRegion> SFXRegions { get; set; }

            /// <summary>
            /// Unknown; Entity generators of some kind?.
            /// </summary>
            public List<Region.GenRegion> Generators { get; set; }

            /// <summary>
            /// Sound triggers of some kind.
            /// </summary>
            public List<Region.SoundRegion> Sounds { get; set; }

            /// <summary>
            /// Sound reverb areas of some kind.
            /// </summary>
            public List<Region.ReverbRegion> ReverbRegions { get; set; }

            /// <summary>
            /// Unknown; Related to landing somehow.
            /// </summary>
            public List<Region.LandingRegion> LandingRegions { get; set; }

            /// <summary>
            /// Unknown; Related to landing somehow.
            /// </summary>
            public List<Region.DebugNavigationRegion> DebugNavigationRegions { get; set; }

            /// <summary>
            /// Unknown; Load measurement regions?
            /// </summary>
            public List<Region.LoadMeasurementRegion> LoadMeasurementRegions { get; set; }

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
                SaveRegions = new List<Region.SaveRegion>();
                CollisionAvoidanceRegions = new List<Region.CollisionAvoidanceRegion>();
                CommunicationAreas = new List<Region.CommunicationArea>();
                WaterSurfaces = new List<Region.WaterSurfaceRegion>();
                SFXRegions = new List<Region.SFXRegion>();
                Generators = new List<Region.GenRegion>();
                Sounds = new List<Region.SoundRegion>();
                ReverbRegions = new List<Region.ReverbRegion>();
                LandingRegions = new List<Region.LandingRegion>();
                DebugNavigationRegions = new List<Region.DebugNavigationRegion>();
                LoadMeasurementRegions = new List<Region.LoadMeasurementRegion>();
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
                    case Region.SaveRegion r: SaveRegions.Add(r); break;
                    case Region.CollisionAvoidanceRegion r: CollisionAvoidanceRegions.Add(r); break;
                    case Region.CommunicationArea r: CommunicationAreas.Add(r); break;
                    case Region.WaterSurfaceRegion r: WaterSurfaces.Add(r); break;
                    case Region.SFXRegion r: SFXRegions.Add(r); break;
                    case Region.GenRegion r: Generators.Add(r); break;
                    case Region.SoundRegion r: Sounds.Add(r); break;
                    case Region.ReverbRegion r: ReverbRegions.Add(r); break;
                    case Region.LandingRegion r: LandingRegions.Add(r); break;
                    case Region.DebugNavigationRegion r: DebugNavigationRegions.Add(r); break;
                    case Region.LoadMeasurementRegion r: LoadMeasurementRegions.Add(r); break;
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
                                                                                  SpawnPoints, ScrapCollectionPoints, SaveRegions, CollisionAvoidanceRegions, CommunicationAreas,
                                                                                  SFXRegions, Generators, Sounds, ReverbRegions, LandingRegions, DebugNavigationRegions,
                                                                                  LoadMeasurementRegions, UnusedRegions);
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
                    case RegionType.Save:
                        return SaveRegions.EchoAdd(new Region.SaveRegion(br));
                    case RegionType.CollisionAvoidance:
                        return CollisionAvoidanceRegions.EchoAdd(new Region.CollisionAvoidanceRegion(br));
                    case RegionType.CommunicationArea:
                        return CommunicationAreas.EchoAdd(new Region.CommunicationArea(br));
                    case RegionType.WaterSurface:
                        return WaterSurfaces.EchoAdd(new Region.WaterSurfaceRegion(br));
                    case RegionType.SFX:
                        return SFXRegions.EchoAdd(new Region.SFXRegion(br));
                    case RegionType.Gen:
                        return Generators.EchoAdd(new Region.GenRegion(br));
                    case RegionType.Sound:
                        return Sounds.EchoAdd(new Region.SoundRegion(br));
                    case RegionType.Reverb:
                        return ReverbRegions.EchoAdd(new Region.ReverbRegion(br));
                    case RegionType.Landing:
                        return LandingRegions.EchoAdd(new Region.LandingRegion(br));
                    case RegionType.DebugNavigation:
                        return DebugNavigationRegions.EchoAdd(new Region.DebugNavigationRegion(br));
                    case RegionType.LoadMeasurement:
                        return LoadMeasurementRegions.EchoAdd(new Region.LoadMeasurementRegion(br));
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
            /// Describes the space encompassed by the region.
            /// </summary>
            public MSB.Shape Shape
            {
                get => _shape;
                set
                {
                    if (value is MSB.Shape.Composite)
                        throw new ArgumentException("Armored Core V does not support composite shapes.");
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
                int offsetArea = br.ReadInt32();
                int offsetSpawn = br.ReadInt32();
                br.AssertInt32(0); // Probably unknown and unused type offset
                int offsetWaterSurface = br.ReadInt32();
                int offsetSFX = br.ReadInt32();
                br.AssertInt32(0); // Probably unknown and unused type offset
                br.AssertInt32(0); // Probably unknown and unused type offset
                br.AssertInt32(0); // Probably unknown and unused type offset
                int offsetUnkConfig3 = br.ReadInt32();
                int offsetGen = br.ReadInt32();
                int offsetReverb = br.ReadInt32();
                int offsetUnkConfig4 = br.ReadInt32();
                br.AssertInt32(0); // Probably unknown and unused type offset
                br.AssertInt32(0); // Probably unknown and unused type offset
                int offsetScrapCollection = br.ReadInt32();
                int offsetSound = br.ReadInt32();
                int offsetLanding = br.ReadInt32();

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

                if (offsetArea > 0)
                {
                    if (Type != RegionType.OperationalArea && Type != RegionType.WarningArea && Type != RegionType.AttentionArea)
                        throw new InvalidDataException($"{nameof(offsetArea)} must be 0 for type {GetType()}");

                    br.Position = start + offsetArea;
                    ReadTypeData(br);
                }

                if (offsetSpawn > 0)
                {
                    if (Type != RegionType.Spawn)
                        throw new InvalidDataException($"{nameof(offsetSpawn)} must be 0 for type {GetType()}");

                    br.Position = start + offsetSpawn;
                    ReadTypeData(br);
                }

                if (offsetWaterSurface > 0)
                {
                    if (Type != RegionType.WaterSurface)
                        throw new InvalidDataException($"{nameof(offsetWaterSurface)} must be 0 for type {GetType()}");

                    br.Position = start + offsetWaterSurface;
                    ReadTypeData(br);
                }

                if (offsetSFX > 0)
                {
                    if (Type != RegionType.SFX)
                        throw new InvalidDataException($"{nameof(offsetSFX)} must be 0 for type {GetType()}");

                    br.Position = start + offsetSFX;
                    ReadTypeData(br);
                }

                br.Position = start + offsetUnkConfig3;
                Layer = new LayerConfig(br);

                if (offsetGen > 0)
                {
                    if (Type != RegionType.Gen)
                        throw new InvalidDataException($"{nameof(offsetGen)} must be 0 for type {GetType()}");

                    br.Position = start + offsetGen;
                    ReadTypeData(br);
                }

                if (offsetReverb > 0)
                {
                    if (Type != RegionType.Reverb)
                        throw new InvalidDataException($"{nameof(offsetReverb)} must be 0 for type {GetType()}");

                    br.Position = start + offsetReverb;
                    ReadTypeData(br);
                }

                br.Position = start + offsetUnkConfig4;
                Config4 = new UnkConfig4(br);

                if (offsetScrapCollection > 0)
                {
                    if (Type != RegionType.ScrapCollection)
                        throw new InvalidDataException($"{nameof(offsetScrapCollection)} must be 0 for type {GetType()}");

                    br.Position = start + offsetScrapCollection;
                    ReadTypeData(br);
                }

                if (offsetSound > 0)
                {
                    if (Type != RegionType.Sound)
                        throw new InvalidDataException($"{nameof(offsetSound)} must be 0 for type {GetType()}");

                    br.Position = start + offsetSound;
                    ReadTypeData(br);
                }

                if (offsetLanding > 0)
                {
                    if (Type != RegionType.Landing)
                        throw new InvalidDataException($"{nameof(offsetLanding)} must be 0 for type {GetType()}");

                    br.Position = start + offsetLanding;
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
                bw.ReserveInt32("OffsetArea");
                bw.ReserveInt32("OffsetSpawn");
                bw.WriteInt32(0); // Probably unknown and unused type offset
                bw.ReserveInt32("OffsetWaterSurface");
                bw.ReserveInt32("OffsetSFX");
                bw.WriteInt32(0); // Probably unknown and unused type offset
                bw.WriteInt32(0); // Probably unknown and unused type offset
                bw.WriteInt32(0); // Probably unknown and unused type offset
                bw.ReserveInt32("OffsetUnkConfig3");
                bw.ReserveInt32("OffsetGen");
                bw.ReserveInt32("OffsetReverb");
                bw.ReserveInt32("OffsetUnkConfig4");
                bw.WriteInt32(0); // Probably unknown and unused type offset
                bw.WriteInt32(0); // Probably unknown and unused type offset
                bw.ReserveInt32("OffsetScrapCollection");
                bw.ReserveInt32("OffsetSound");
                bw.ReserveInt32("OffsetLanding");

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

                FillTypeDataOffset(bw, start, "OffsetArea", Type == RegionType.OperationalArea || Type == RegionType.WarningArea || Type == RegionType.AttentionArea);
                FillTypeDataOffset(bw, start, "OffsetSpawn", Type == RegionType.Spawn);
                // Probably unknown type
                FillTypeDataOffset(bw, start, "OffsetWaterSurface", Type == RegionType.WaterSurface);
                FillTypeDataOffset(bw, start, "OffsetSFX", Type == RegionType.SFX);
                // Probably unknown type
                // Probably unknown type
                // Probably unknown type
                bw.FillInt32("OffsetUnkConfig3", (int)(bw.Position - start));
                Layer.Write(bw);

                FillTypeDataOffset(bw, start, "OffsetGen", Type == RegionType.Gen);
                FillTypeDataOffset(bw, start, "OffsetReverb", Type == RegionType.Reverb);

                bw.FillInt32("OffsetUnkConfig4", (int)(bw.Position - start));
                Config4.Write(bw);

                // Probably unknown type
                // Probably unknown type
                FillTypeDataOffset(bw, start, "OffsetScrapCollection", Type == RegionType.ScrapCollection);
                FillTypeDataOffset(bw, start, "OffsetSound", Type == RegionType.Sound);
                FillTypeDataOffset(bw, start, "OffsetLanding", Type == RegionType.Landing);
            }

            private void FillTypeDataOffset(BinaryWriterEx bw, long start, string name, bool hasType)
            {
                if (hasType)
                {
                    bw.FillInt32(name, (int)(bw.Position - start));
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt32(name, 0);
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

            /// <summary>
            /// Unknown; Water surface change?
            /// </summary>
            public class WaterSurfaceConfig
            {
                /// <summary>
                /// Unknown; Water surface level?
                /// </summary>
                public float Unk00 { get; set; }

                public WaterSurfaceConfig()
                {
                    Unk00 = -73;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public WaterSurfaceConfig DeepCopy()
                {
                    return (WaterSurfaceConfig)MemberwiseClone();
                }

                internal WaterSurfaceConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadSingle();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                }
            }

            /// <summary>
            /// Unknown; Entity generator of some kind?
            /// </summary>
            public class GenConfig
            {
                /// <summary>
                /// Unknown; A generator ID of some kind?
                /// </summary>
                public int Unk00 { get; set; }

                public GenConfig()
                {
                    Unk00 = -73;
                }

                /// <summary>
                /// Creates a deep copy of the struct.
                /// </summary>
                public GenConfig DeepCopy()
                {
                    return (GenConfig)MemberwiseClone();
                }

                internal GenConfig(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                internal void Write(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
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
            /// Unknown; A save point?
            /// </summary>
            public class SaveRegion : Region
            {
                private protected override RegionType Type => RegionType.Save;


                public SaveRegion() : base("save") { }

                private protected override void DeepCopyTo(Region region) { }

                internal SaveRegion(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; AI collision avoidance?
            /// </summary>
            public class CollisionAvoidanceRegion : Region
            {
                private protected override RegionType Type => RegionType.CollisionAvoidance;


                public CollisionAvoidanceRegion() : base("collision avoidance") { }

                private protected override void DeepCopyTo(Region region) { }

                internal CollisionAvoidanceRegion(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; An area for communication?
            /// </summary>
            public class CommunicationArea : Region
            {
                private protected override RegionType Type => RegionType.CommunicationArea;


                public CommunicationArea() : base("communication area") { }

                private protected override void DeepCopyTo(Region region) { }

                internal CommunicationArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; Water surface level?
            /// </summary>
            public class WaterSurfaceRegion : Region
            {
                private protected override RegionType Type => RegionType.WaterSurface;

                public WaterSurfaceConfig WaterSurface { get; set; }

                public WaterSurfaceRegion() : base("water surface")
                {
                    WaterSurface = new WaterSurfaceConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var waterSurface = (WaterSurfaceRegion)region;
                    waterSurface.WaterSurface = WaterSurface.DeepCopy();
                }

                internal WaterSurfaceRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => WaterSurface = new WaterSurfaceConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => WaterSurface.Write(bw);
            }

            /// <summary>
            /// An SFX region of some kind.
            /// </summary>
            public class SFXRegion : Region
            {
                private protected override RegionType Type => RegionType.SFX;

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
            /// Unknown; An entity generator of some kind?
            /// </summary>
            public class GenRegion : Region
            {
                private protected override RegionType Type => RegionType.Gen;

                public GenConfig Gen { get; set; }

                public GenRegion() : base("gen")
                {
                    Gen = new GenConfig();
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var gen = (GenRegion)region;
                    gen.Gen = Gen.DeepCopy();
                }

                internal GenRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br) => Gen = new GenConfig(br);
                private protected override void WriteTypeData(BinaryWriterEx bw) => Gen.Write(bw);
            }

            /// <summary>
            /// A sound trigger region of some kind.
            /// </summary>
            public class SoundRegion : Region
            {
                private protected override RegionType Type => RegionType.Sound;

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
            /// Unknown; Related to a landing of some kind.
            /// </summary>
            public class LandingRegion : Region
            {
                private protected override RegionType Type => RegionType.Landing;

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

                public DebugNavigationRegion() : base("debug navigation") { }

                private protected override void DeepCopyTo(Region region) { }

                internal DebugNavigationRegion(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown; Load measurement?
            /// </summary>
            public class LoadMeasurementRegion : Region
            {
                private protected override RegionType Type => RegionType.LoadMeasurement;


                public LoadMeasurementRegion() : base("load measurement") { }

                private protected override void DeepCopyTo(Region region) { }

                internal LoadMeasurementRegion(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unused points?
            /// </summary>
            public class UnusedRegion : Region
            {
                private protected override RegionType Type => RegionType.Unused;

                public UnusedRegion() : base("") { }

                private protected override void DeepCopyTo(Region region) { }

                internal UnusedRegion(BinaryReaderEx br) : base(br) { }
            }

            #endregion
        }
    }
}
