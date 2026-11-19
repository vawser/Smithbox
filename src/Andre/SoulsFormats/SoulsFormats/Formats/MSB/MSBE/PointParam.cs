using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class MSBE
    {
        internal enum RegionType : uint
        {
            InvasionPoint = 1,
            EnvironmentMapPoint = 2,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            SpawnPoint = 8,
            Message = 9,
            EnvironmentMapEffectBox = 17,
            WindArea = 18,
            Connection = 21,
            PatrolRoute22 = 22,
            BuddySummonPoint = 26,
            RollingObjectOverride = 27,
            OcclusionBox = 28,
            OcclusionPortal = 29,
            SoundRegion = 30,
            MufflingPlane = 31,
            PatrolRoute = 32,
            WorldMapPoint = 33,
            WeatherLotRegion = 35,
            AutoDrawGroupPoint = 36,
            GroupPlacementPoint = 37,
            WorldMapDiscoveryPoint = 38,
            WorldMapReentryPoint = 39,
            Hitset = 40,
            FastTravelOverride = 41,
            WeatherCreateAssetPoint = 42,
            PlayRegion = 43,
            EnvironmentMapOutput = 44,
            RideJump = 46,
            SoundDummy = 48,
            FallPreventionRemoval = 49,
            NavmeshCutting = 50,
            MapPlaceNameOverride = 51,
            RideJumpoff = 52,
            DisableHorseOverride = 53,
            DlcRideJump = 54,
            DlcRideJumpoff = 55,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Points and volumes used to trigger various effects.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion>
        {
            /// <summary>
            /// Previously points where players will appear when invading; not sure if they do anything in Sekiro.
            /// </summary>
            public List<Region.InvasionPoint> InvasionPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvironmentMapPoint> EnvironmentMapPoints { get; set; }

            /// <summary>
            /// Areas where a sound will play.
            /// </summary>
            public List<Region.Sound> Sounds { get; set; }

            /// <summary>
            /// Points for particle effects to play at.
            /// </summary>
            public List<Region.SFX> SFX { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WindSFX> WindSFX { get; set; }

            /// <summary>
            /// Points where the player can spawn into a map.
            /// </summary>
            public List<Region.SpawnPoint> SpawnPoints { get; set; }

            /// <summary>
            /// Points that have developer messages.
            /// </summary>
            public List<Region.Message> Messages { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvironmentMapEffectBox> EnvironmentMapEffectBoxes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WindArea> WindAreas { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Connection> Connections { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.PatrolRoute22> PatrolRoute22s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.BuddySummonPoint> BuddySummonPoints { get; set; }

            /// <summary>
            /// Regions that change which RollingObjLotParam row rolling objects spawn from.
            /// </summary>
            public List<Region.RollingObjectOverride> RollingObjectOverrides { get; set; }

            /// <summary>
            /// Areas where sound is muffled.
            /// </summary>
            public List<Region.OcclusionBox> OcclusionBoxes { get; set; }

            /// <summary>
            /// Entrances to muffling boxes.
            /// </summary>
            public List<Region.OcclusionPortal> OcclusionPortals { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.SoundRegion> SoundRegions { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MufflingPlane> MufflingPlanes { get; set; }

            /// <summary>
            /// Points that describe an NPC patrol path.
            /// </summary>
            public List<Region.PatrolRoute> PatrolRoutes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WorldMapPoint> WorldMapPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WeatherLotRegion> WeatherLotRegions { get; set; }


            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.AutoDrawGroupPoint> AutoDrawGroupPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.GroupPlacementPoint> GroupPlacementPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WorldMapDiscoveryPoint> WorldMapDiscoveryPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WorldMapReentryPoint> WorldMapReentryPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Hitset> Hitsets { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.FastTravelOverride> FastTravelOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WeatherCreateAssetPoint> WeatherCreateAssetPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.PlayRegion> PlayRegions { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvironmentMapOutput> EnvironmentMapOutputs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.RideJump> RideJumps { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.SoundDummy> SoundDummies { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.FallPreventionRemoval> FallPreventionRemovals { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.NavmeshCutting> NavmeshCuttings { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MapPlaceNameOverride> MapPlaceNameOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.RideJumpoff> RideJumpoffs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.DisableHorseOverride> DisableHorseOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.DlcRideJump> DlcRideJumps { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.DlcRideJumpoff> DlcRideJumpoffs { get; set; }

            /// <summary>
            /// Most likely a dumping ground for unused regions.
            /// </summary>
            public List<Region.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty PointParam with the default version.
            /// </summary>
            public PointParam() : base(73, "POINT_PARAM_ST")
            {
                InvasionPoints = new List<Region.InvasionPoint>();
                EnvironmentMapPoints = new List<Region.EnvironmentMapPoint>();
                Sounds = new List<Region.Sound>();
                SFX = new List<Region.SFX>();
                WindSFX = new List<Region.WindSFX>();
                SpawnPoints = new List<Region.SpawnPoint>();
                Messages = new List<Region.Message>();
                EnvironmentMapEffectBoxes = new List<Region.EnvironmentMapEffectBox>();
                WindAreas = new List<Region.WindArea>();
                Connections = new List<Region.Connection>();
                PatrolRoute22s = new List<Region.PatrolRoute22>();
                BuddySummonPoints = new List<Region.BuddySummonPoint>();
                RollingObjectOverrides = new List<Region.RollingObjectOverride>();
                OcclusionBoxes = new List<Region.OcclusionBox>();
                OcclusionPortals = new List<Region.OcclusionPortal>();
                SoundRegions = new List<Region.SoundRegion>();
                MufflingPlanes = new List<Region.MufflingPlane>();
                PatrolRoutes = new List<Region.PatrolRoute>();
                WorldMapPoints = new List<Region.WorldMapPoint>();
                WeatherLotRegions = new List<Region.WeatherLotRegion>();
                AutoDrawGroupPoints = new List<Region.AutoDrawGroupPoint>();
                GroupPlacementPoints = new List<Region.GroupPlacementPoint>();
                WorldMapDiscoveryPoints = new List<Region.WorldMapDiscoveryPoint>();
                WorldMapReentryPoints = new List<Region.WorldMapReentryPoint>();
                Hitsets = new List<Region.Hitset>();
                FastTravelOverrides = new List<Region.FastTravelOverride>();
                WeatherCreateAssetPoints = new List<Region.WeatherCreateAssetPoint>();
                PlayRegions = new List<Region.PlayRegion>();
                EnvironmentMapOutputs = new List<Region.EnvironmentMapOutput>();
                RideJumps = new List<Region.RideJump>();
                SoundDummies = new List<Region.SoundDummy>();
                FallPreventionRemovals = new List<Region.FallPreventionRemoval>();
                NavmeshCuttings = new List<Region.NavmeshCutting>();
                MapPlaceNameOverrides = new List<Region.MapPlaceNameOverride>();
                RideJumpoffs = new List<Region.RideJumpoff>();
                DisableHorseOverrides = new List<Region.DisableHorseOverride>();
                DlcRideJumps = new List<Region.DlcRideJump>();
                DlcRideJumpoffs = new List<Region.DlcRideJumpoff>();
                Others = new List<Region.Other>();
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region)
            {
                switch (region)
                {
                    case Region.InvasionPoint r: InvasionPoints.Add(r); break;
                    case Region.EnvironmentMapPoint r: EnvironmentMapPoints.Add(r); break;
                    case Region.Sound r: Sounds.Add(r); break;
                    case Region.SFX r: SFX.Add(r); break;
                    case Region.WindSFX r: WindSFX.Add(r); break;
                    case Region.SpawnPoint r: SpawnPoints.Add(r); break;
                    case Region.Message r: Messages.Add(r); break;
                    case Region.EnvironmentMapEffectBox r: EnvironmentMapEffectBoxes.Add(r); break;
                    case Region.WindArea r: WindAreas.Add(r); break;
                    case Region.Connection r: Connections.Add(r); break;
                    case Region.PatrolRoute22 r: PatrolRoute22s.Add(r); break;
                    case Region.BuddySummonPoint r: BuddySummonPoints.Add(r); break;
                    case Region.OcclusionBox r: OcclusionBoxes.Add(r); break;
                    case Region.OcclusionPortal r: OcclusionPortals.Add(r); break;
                    case Region.SoundRegion r: SoundRegions.Add(r); break;
                    case Region.MufflingPlane r: MufflingPlanes.Add(r); break;
                    case Region.PatrolRoute r: PatrolRoutes.Add(r); break;
                    case Region.WorldMapPoint r: WorldMapPoints.Add(r); break;
                    case Region.WeatherLotRegion r: WeatherLotRegions.Add(r); break;
                    case Region.AutoDrawGroupPoint r: AutoDrawGroupPoints.Add(r); break;
                    case Region.GroupPlacementPoint r: GroupPlacementPoints.Add(r); break;
                    case Region.WorldMapDiscoveryPoint r: WorldMapDiscoveryPoints.Add(r); break;
                    case Region.WorldMapReentryPoint r: WorldMapReentryPoints.Add(r); break;
                    case Region.Hitset r: Hitsets.Add(r); break;
                    case Region.FastTravelOverride r: FastTravelOverrides.Add(r); break;
                    case Region.WeatherCreateAssetPoint r: WeatherCreateAssetPoints.Add(r); break;
                    case Region.PlayRegion r: PlayRegions.Add(r); break;
                    case Region.EnvironmentMapOutput r: EnvironmentMapOutputs.Add(r); break;
                    case Region.RideJump r: RideJumps.Add(r); break;
                    case Region.SoundDummy r: SoundDummies.Add(r); break;
                    case Region.FallPreventionRemoval r: FallPreventionRemovals.Add(r); break;
                    case Region.NavmeshCutting r: NavmeshCuttings.Add(r); break;
                    case Region.MapPlaceNameOverride r: MapPlaceNameOverrides.Add(r); break;
                    case Region.RideJumpoff r: RideJumpoffs.Add(r); break;
                    case Region.DisableHorseOverride r: DisableHorseOverrides.Add(r); break;
                    case Region.DlcRideJump r: DlcRideJumps.Add(r); break;
                    case Region.DlcRideJumpoff r: DlcRideJumpoffs.Add(r); break;
                    case Region.RollingObjectOverride r: RollingObjectOverrides.Add(r); break;
                    case Region.Other r: Others.Add(r); break;

                    default:
                        throw new ArgumentException($"Unrecognized type {region.GetType()}.", nameof(region));
                }
                return region;
            }
            IMsbRegion IMsbParam<IMsbRegion>.Add(IMsbRegion item) => Add((Region)item);

            /// <summary>
            /// Returns every region in the order they'll be written.
            /// </summary>
            public override List<Region> GetEntries()
            {
                return SFUtil.ConcatAll<Region>(
                    InvasionPoints, EnvironmentMapPoints, Sounds, SFX, WindSFX,
                    SpawnPoints, Messages, EnvironmentMapEffectBoxes, WindAreas,
                    Connections, PatrolRoute22s, BuddySummonPoints, RollingObjectOverrides, OcclusionBoxes,
                    OcclusionPortals, SoundRegions, MufflingPlanes, PatrolRoutes,
                    WorldMapPoints, WeatherLotRegions, AutoDrawGroupPoints, GroupPlacementPoints,
                    WorldMapDiscoveryPoints, WorldMapReentryPoints, Hitsets,
                    FastTravelOverrides, WeatherCreateAssetPoints, PlayRegions, EnvironmentMapOutputs,
                    RideJumps, SoundDummies, FallPreventionRemovals, NavmeshCuttings, MapPlaceNameOverrides,
                    RideJumpoffs, DisableHorseOverrides, DlcRideJumps, DlcRideJumpoffs, Others);
            }
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 8);
                switch (type)
                {
                    case RegionType.InvasionPoint:
                        return InvasionPoints.EchoAdd(new Region.InvasionPoint(br));

                    case RegionType.EnvironmentMapPoint:
                        return EnvironmentMapPoints.EchoAdd(new Region.EnvironmentMapPoint(br));

                    case RegionType.Sound:
                        return Sounds.EchoAdd(new Region.Sound(br));

                    case RegionType.SFX:
                        return SFX.EchoAdd(new Region.SFX(br));

                    case RegionType.WindSFX:
                        return WindSFX.EchoAdd(new Region.WindSFX(br));

                    case RegionType.SpawnPoint:
                        return SpawnPoints.EchoAdd(new Region.SpawnPoint(br));

                    case RegionType.Message:
                        return Messages.EchoAdd(new Region.Message(br));

                    case RegionType.EnvironmentMapEffectBox:
                        return EnvironmentMapEffectBoxes.EchoAdd(new Region.EnvironmentMapEffectBox(br));

                    case RegionType.WindArea:
                        return WindAreas.EchoAdd(new Region.WindArea(br));

                    case RegionType.Connection:
                        return Connections.EchoAdd(new Region.Connection(br));
                        
                    case RegionType.PatrolRoute22:
                        return PatrolRoute22s.EchoAdd(new Region.PatrolRoute22(br));

                    case RegionType.BuddySummonPoint:
                        return BuddySummonPoints.EchoAdd(new Region.BuddySummonPoint(br));

                    case RegionType.RollingObjectOverride:
                        return RollingObjectOverrides.EchoAdd(new Region.RollingObjectOverride(br));

                    case RegionType.OcclusionBox:
                        return OcclusionBoxes.EchoAdd(new Region.OcclusionBox(br));

                    case RegionType.OcclusionPortal:
                        return OcclusionPortals.EchoAdd(new Region.OcclusionPortal(br));

                    case RegionType.SoundRegion:
                        return SoundRegions.EchoAdd(new Region.SoundRegion(br));

                    case RegionType.MufflingPlane:
                        return MufflingPlanes.EchoAdd(new Region.MufflingPlane(br));

                    case RegionType.PatrolRoute:
                        return PatrolRoutes.EchoAdd(new Region.PatrolRoute(br));

                    case RegionType.WorldMapPoint:
                        return WorldMapPoints.EchoAdd(new Region.WorldMapPoint(br));

                    case RegionType.WeatherLotRegion:
                        return WeatherLotRegions.EchoAdd(new Region.WeatherLotRegion(br));

                    case RegionType.AutoDrawGroupPoint:
                        return AutoDrawGroupPoints.EchoAdd(new Region.AutoDrawGroupPoint(br));

                    case RegionType.GroupPlacementPoint:
                        return GroupPlacementPoints.EchoAdd(new Region.GroupPlacementPoint(br));

                    case RegionType.WorldMapDiscoveryPoint:
                        return WorldMapDiscoveryPoints.EchoAdd(new Region.WorldMapDiscoveryPoint(br));

                    case RegionType.WorldMapReentryPoint:
                        return WorldMapReentryPoints.EchoAdd(new Region.WorldMapReentryPoint(br));

                    case RegionType.Hitset:
                        return Hitsets.EchoAdd(new Region.Hitset(br));

                    case RegionType.FastTravelOverride:
                        return FastTravelOverrides.EchoAdd(new Region.FastTravelOverride(br));

                    case RegionType.WeatherCreateAssetPoint:
                        return WeatherCreateAssetPoints.EchoAdd(new Region.WeatherCreateAssetPoint(br));

                    case RegionType.PlayRegion:
                        return PlayRegions.EchoAdd(new Region.PlayRegion(br));

                    case RegionType.EnvironmentMapOutput:
                        return EnvironmentMapOutputs.EchoAdd(new Region.EnvironmentMapOutput(br));

                    case RegionType.RideJump:
                        return RideJumps.EchoAdd(new Region.RideJump(br));

                    case RegionType.SoundDummy:
                        return SoundDummies.EchoAdd(new Region.SoundDummy(br));

                    case RegionType.FallPreventionRemoval:
                        return FallPreventionRemovals.EchoAdd(new Region.FallPreventionRemoval(br));

                    case RegionType.NavmeshCutting:
                        return NavmeshCuttings.EchoAdd(new Region.NavmeshCutting(br));

                    case RegionType.MapPlaceNameOverride:
                        return MapPlaceNameOverrides.EchoAdd(new Region.MapPlaceNameOverride(br));

                    case RegionType.RideJumpoff:
                        return RideJumpoffs.EchoAdd(new Region.RideJumpoff(br));

                    case RegionType.DisableHorseOverride:
                        return DisableHorseOverrides.EchoAdd(new Region.DisableHorseOverride(br));

                    case RegionType.DlcRideJump:
                        return DlcRideJumps.EchoAdd(new Region.DlcRideJump(br));

                    case RegionType.DlcRideJumpoff:
                        return DlcRideJumpoffs.EchoAdd(new Region.DlcRideJumpoff(br));

                    case RegionType.Other:
                        return Others.EchoAdd(new Region.Other(br));

                    default:
                        throw new NotImplementedException($"Unimplemented region type: {type}");
                }
            }
        }

        /// <summary>
        /// A point or volume that triggers some sort of interaction.
        /// </summary>
        public abstract class Region : Entry, IMsbRegion
        {
            private protected abstract RegionType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// The shape of the region.
            /// </summary>
            public MSB.Shape Shape { get; set; }

            /// <summary>
            /// The location of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotiation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Presumed ID for regions. Unique per map / incremented per region.
            /// </summary>
            public int RegionID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk40 { get; set; }

            /// <summary>
            /// Controls whether the region is active in different ceremonies.
            /// </summary>
            public uint MapStudioLayer { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> UnkA { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> UnkB { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public byte UnkE08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int MapID { get; set; }

            /// <summary>
            /// Unknown. The game reads it as a byte.
            /// </summary>
            public byte UnkS04 { get; set; }

            /// <summary>
            /// Unknown; not read by the game.
            /// </summary>
            public byte UnkS05 { get; set; }

            /// <summary>
            /// Unknown; not read by the game.
            /// </summary>
            public byte UnkS06 { get; set; }

            /// <summary>
            /// Unknown; not read by the game.
            /// </summary>
            public byte UnkS07 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkS0C { get; set; }

            /// <summary>
            /// If specified, the region is only active when the part is loaded.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Part))]
            public string ActivationPartName { get; set; }
            private int ActivationPartIndex { get; set; }

            /// <summary>
            /// Identifies the region in event scripts.
            /// </summary>
            public uint EntityID { get; set; }

            private protected Region(string name)
            {
                Name = name;
                Shape = new MSB.Shape.Point();
                MapStudioLayer = 0xFFFFFFFF;
                UnkA = new List<short>();
                UnkB = new List<short>();
                EntityID = 0;
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy()
            {
                var region = (Region)MemberwiseClone();
                region.Shape = Shape.DeepCopy();
                region.UnkA = new List<short>(UnkA);
                region.UnkB = new List<short>(UnkB);
                DeepCopyTo(region);
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Region region) { }

            private protected Region(BinaryReaderEx br)
            {
                long start = br.Position;
                long nameOffset = br.ReadInt64();
                br.AssertUInt32((uint)Type);
                br.ReadInt32(); // ID
                MSB.ShapeType shapeType = br.ReadEnum32<MSB.ShapeType>();
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                RegionID = br.ReadInt32();
                long baseDataOffset1 = br.ReadInt64();
                long baseDataOffset2 = br.ReadInt64();
                Unk40 = br.ReadInt32();
                MapStudioLayer = br.ReadUInt32();
                long shapeDataOffset = br.ReadInt64();
                long baseDataOffset3 = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long unkOffset4 = br.ReadInt64();

                Shape = MSB.Shape.Create(shapeType);

                if (!BinaryReaderEx.IgnoreAsserts)
                {
                    if (nameOffset == 0)
                        throw new InvalidDataException($"{nameof(nameOffset)} must not be 0 in type {GetType()}.");
                    if (baseDataOffset1 == 0)
                        throw new InvalidDataException($"{nameof(baseDataOffset1)} must not be 0 in type {GetType()}.");
                    if (baseDataOffset2 == 0)
                        throw new InvalidDataException($"{nameof(baseDataOffset2)} must not be 0 in type {GetType()}.");
                    if (Shape.HasShapeData ^ shapeDataOffset != 0)
                        throw new InvalidDataException($"Unexpected {nameof(shapeDataOffset)} 0x{shapeDataOffset:X} in type {GetType()}.");
                    if (baseDataOffset3 == 0)
                        throw new InvalidDataException($"{nameof(baseDataOffset3)} must not be 0 in type {GetType()}.");
                    if (HasTypeData ^ typeDataOffset != 0)
                        throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");
                    if (unkOffset4 == 0)
                        throw new InvalidDataException($"{nameof(unkOffset4)} must not be 0 in type {GetType()}.");
                }

                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + baseDataOffset1;
                short countA = br.ReadInt16();
                UnkA = new List<short>(br.ReadInt16s(countA));

                br.Position = start + baseDataOffset2;
                short countB = br.ReadInt16();
                UnkB = new List<short>(br.ReadInt16s(countB));

                if (Shape.HasShapeData)
                {
                    br.Position = start + shapeDataOffset;
                    Shape.ReadShapeData(br);
                }

                br.Position = start + baseDataOffset3;
                ActivationPartIndex = br.ReadInt32();
                EntityID = br.ReadUInt32();
                UnkE08 = br.ReadByte();
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertInt32(0);

                if (HasTypeData)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }

                // Unk4
                br.Position = start + unkOffset4;
                MapID = br.ReadInt32();
                UnkS04 = br.ReadByte();
                UnkS05 = br.ReadByte();
                UnkS06 = br.ReadByte();
                UnkS07 = br.ReadByte();
                br.AssertInt32(0);
                UnkS0C = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
            }

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteUInt32((uint)Shape.Type);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(RegionID);
                bw.ReserveInt64("BaseDataOffset1");
                bw.ReserveInt64("BaseDataOffset2");
                bw.WriteInt32(Unk40);
                bw.WriteUInt32(MapStudioLayer);
                bw.ReserveInt64("ShapeDataOffset");
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("Unk4Offset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.Pad(4);

                bw.FillInt64("BaseDataOffset1", bw.Position - start);
                bw.WriteInt16((short)UnkA.Count);
                bw.WriteInt16s(UnkA);
                bw.Pad(4);

                bw.FillInt64("BaseDataOffset2", bw.Position - start);
                bw.WriteInt16((short)UnkB.Count);
                bw.WriteInt16s(UnkB);
                bw.Pad(8);

                if (Shape.HasShapeData)
                {
                    bw.FillInt64("ShapeDataOffset", bw.Position - start);
                    Shape.WriteShapeData(bw);
                }
                else
                {
                    bw.FillInt64("ShapeDataOffset", 0);
                }

                bw.FillInt64("EntityDataOffset", bw.Position - start);
                bw.WriteInt32(ActivationPartIndex);
                bw.WriteUInt32(EntityID);
                bw.WriteByte(UnkE08);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteInt32(0);

                // Types up to RollingObjectOverride (27) pad after the type data, later types before it
                if (Type > RegionType.RollingObjectOverride && Type != RegionType.Other)
                {
                    bw.Pad(8);
                }

                if (HasTypeData)
                {
                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }

                if (Type <= RegionType.RollingObjectOverride || Type == RegionType.Other)
                {
                    bw.Pad(8);
                }

                bw.FillInt64("Unk4Offset", bw.Position - start);
                bw.WriteInt32(MapID);
                bw.WriteByte(UnkS04);
                bw.WriteByte(UnkS05);
                bw.WriteByte(UnkS06);
                bw.WriteByte(UnkS07);
                bw.WriteInt32(0);
                bw.WriteInt32(UnkS0C);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.Pad(8);
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal virtual void GetNames(Entries entries)
            {
                ActivationPartName = MSB.FindName(entries.Parts, ActivationPartIndex);
                if (Shape is MSB.Shape.Composite composite)
                    composite.GetNames(entries.Regions);
            }

            internal virtual void GetIndices(Entries entries)
            {
                ActivationPartIndex = MSB.FindIndex(this, entries.Parts, ActivationPartName);
                if (Shape is MSB.Shape.Composite composite)
                    composite.GetIndices(entries.Regions);
            }

            /// <summary>
            /// Returns the type, shape type, and name of the region as a string.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                return $"{Type} {Shape.Type} {Name}";
            }

            /// <summary>
            /// A point where an invader can appear. Vanilla names are 侵入ポイント_ID_PRIORITY.
            /// </summary>
            public class InvasionPoint : Region
            {
                private protected override RegionType Type => RegionType.InvasionPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Ranks candidate invasion points together with their distance to the target player; vanilla points carry it in their name suffix (2182 of 2184).
                /// </summary>
                public int Priority { get; set; }

                /// <summary>
                /// Creates an InvasionPoint with default values.
                /// </summary>
                public InvasionPoint() : base($"{nameof(Region)}: {nameof(InvasionPoint)}") { }

                internal InvasionPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Priority = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Priority);
                }
            }

            /// <summary>
            /// An environment map capture point (vanilla: Env_PointNNN). EnvironmentMapEffectBoxes use the point whose name ends in their env map number.
            /// </summary>
            public class EnvironmentMapPoint : Region
            {
                private protected override RegionType Type => RegionType.EnvironmentMapPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT0D { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT0E { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT0F { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Map the point belongs to: bytes 0-2 are compared with a map ID's AA/BB/CC (byte 0 = 0: none). Byte 3 is not read.
                /// </summary>
                public byte[] UnkMapID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT2D { get; set; }

                /// <summary>
                /// Creates an EnvironmentMapPoint with default values.
                /// </summary>
                public EnvironmentMapPoint() : base($"{nameof(Region)}: {nameof(EnvironmentMapPoint)}")
                {
                    UnkMapID = new byte[4];
                }

                internal EnvironmentMapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void DeepCopyTo(Region region)
                {
                    var point = (EnvironmentMapPoint)region;
                    point.UnkMapID = (byte[])UnkMapID.Clone();
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertByte(0);
                    UnkT0D = br.ReadBoolean();
                    UnkT0E = br.ReadBoolean();
                    UnkT0F = br.ReadBoolean();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkMapID = br.ReadBytes(4);
                    br.AssertInt32(0);
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    UnkT2C = br.ReadByte();
                    UnkT2D = br.ReadByte();
                    br.AssertInt16(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteByte(0);
                    bw.WriteBoolean(UnkT0D);
                    bw.WriteBoolean(UnkT0E);
                    bw.WriteBoolean(UnkT0F);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteBytes(UnkMapID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteByte(UnkT2C);
                    bw.WriteByte(UnkT2D);
                    bw.WriteInt16(0);
                }
            }

            /// <summary>
            /// An area where a sound plays.
            /// </summary>
            public class Sound : Region
            {
                private protected override RegionType Type => RegionType.Sound;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The category of the sound.
                /// </summary>
                public int SoundType { get; set; }

                /// <summary>
                /// The ID of the sound.
                /// </summary>
                public int SoundID { get; set; }

                /// <summary>
                /// References to other regions used to build a composite shape.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string[] ChildRegionNames { get; set; }
                private int[] ChildRegionIndices { get; set; }

                /// <summary>
                /// Unknown. Not seen read by the game.
                /// </summary>
                public bool UnkT49 { get; set; }

                /// <summary>
                /// Creates a Sound with default values.
                /// </summary>
                public Sound() : base($"{nameof(Region)}: {nameof(Sound)}")
                {
                    ChildRegionNames = new string[16];
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var sound = (Sound)region;
                    sound.ChildRegionNames = (string[])ChildRegionNames.Clone();
                }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    ChildRegionIndices = br.ReadInt32s(16);
                    br.AssertByte(0);
                    UnkT49 = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertByte(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32s(ChildRegionIndices);
                    bw.WriteByte(0);
                    bw.WriteBoolean(UnkT49);
                    bw.WriteByte(0);
                    bw.WriteByte(0);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    ChildRegionNames = MSB.FindNames(entries.Regions, ChildRegionIndices);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    ChildRegionIndices = MSB.FindIndices(this, entries.Regions, ChildRegionNames);
                }
            }

            /// <summary>
            /// A point where a particle effect can play.
            /// </summary>
            public class SFX : Region
            {
                private protected override RegionType Type => RegionType.SFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The ID of the particle effect FFX.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// If non-zero, the effect is off by default until enabled by event scripts. The game reads it as a
                /// byte.
                /// </summary>
                public byte StartDisabled { get; set; }

                /// <summary>
                /// Unknown flag, read as a byte and passed to the SFX creation (non-zero = true). 0 in retail.
                /// </summary>
                public byte UnkT05 { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    StartDisabled = br.ReadByte();
                    UnkT05 = br.ReadByte();
                    br.AssertInt16(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteByte(StartDisabled);
                    bw.WriteByte(UnkT05);
                    bw.WriteInt16(0);
                }
            }

            /// <summary>
            /// A wind effect: an SFX placed with a WindArea region.
            /// </summary>
            public class WindSFX : Region
            {
                private protected override RegionType Type => RegionType.WindSFX;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of the effect FFX.
                /// </summary>
                public int EffectID { get; set; }

                /// <summary>
                /// Reference to a WindArea region.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string WindAreaName { get; set; }
                private int WindAreaIndex { get; set; }

                /// <summary>
                /// When 0 or more, sets a value on the created wind effect; -1 (every retail region) leaves the effect's own value.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Creates a WindSFX with default values.
                /// </summary>
                public WindSFX() : base($"{nameof(Region)}: {nameof(WindSFX)}") { }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    WindAreaIndex = br.ReadInt32();
                    UnkT08 = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(WindAreaIndex);
                    bw.WriteSingle(UnkT08);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    WindAreaName = MSB.FindName(entries.Regions, WindAreaIndex);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    WindAreaIndex = MSB.FindIndex(this, entries.Regions, WindAreaName);
                }
            }

            /// <summary>
            /// A warp destination or retry point (vanilla: ワープ先, リトライポイント), used by event scripts through its entity ID. NR calls the type ReturnPoint.
            /// </summary>
            public class SpawnPoint : Region
            {
                private protected override RegionType Type => RegionType.SpawnPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a SpawnPoint with default values.
                /// </summary>
                public SpawnPoint() : base($"{nameof(Region)}: {nameof(SpawnPoint)}") { }

                internal SpawnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A developer message.
            /// </summary>
            public class Message : Region
            {
                private protected override RegionType Type => RegionType.Message;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// ID of the message's text in the FMGs.
                /// </summary>
                public short MessageID { get; set; }

                /// <summary>
                /// Unknown. Always 0 or 2 (2 on seven hint messages). Copied into the blood message instance right after
                /// MessageID (CS::BloodMessageIns +30h, MessageID at +2Ch), and the two are read together as one value.
                /// </summary>
                public short UnkT02 { get; set; }

                /// <summary>
                /// Whether the message requires Seek Guidance to appear.
                /// </summary>
                public bool Hidden { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int ItemLotParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int MessageSfxID { get; set; }

                /// <summary>
                /// Event Flag required to be ON for the message to appear.
                /// </summary>
                public uint EnableEventFlagID { get; set; }

                /// <summary>
                /// ID of character to render along with the message.
                /// </summary>
                public int CharacterModelName { get; set; }

                /// <summary>
                /// NpcParam ID to use when rendering a character with the message.
                /// </summary>
                public int NPCParamID { get; set; }

                /// <summary>
                /// Animation ID to use when rendering a character with the message.
                /// </summary>
                public int AnimationID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "CharaInitParam")]
                public int CharaInitParamID { get; set; }

                /// <summary>
                /// Creates a Message with default values.
                /// </summary>
                public Message() : base($"{nameof(Region)}: {nameof(Message)}")
                {
                    MessageID = -1;
                }

                internal Message(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    MessageID = br.ReadInt16();
                    UnkT02 = br.ReadInt16();
                    Hidden = br.AssertInt32([0, 1]) == 1;
                    ItemLotParamID = br.ReadInt32();
                    MessageSfxID = br.ReadInt32();
                    EnableEventFlagID = br.ReadUInt32();
                    CharacterModelName = br.ReadInt32();
                    NPCParamID = br.ReadInt32();
                    AnimationID = br.ReadInt32();
                    CharaInitParamID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt16(MessageID);
                    bw.WriteInt16(UnkT02);
                    bw.WriteInt32(Hidden ? 1 : 0);
                    bw.WriteInt32(ItemLotParamID);
                    bw.WriteInt32(MessageSfxID);
                    bw.WriteUInt32(EnableEventFlagID);
                    bw.WriteInt32(CharacterModelName);
                    bw.WriteInt32(NPCParamID);
                    bw.WriteInt32(AnimationID);
                    bw.WriteInt32(CharaInitParamID);
                }
            }

            /// <summary>
            /// An environment map effect box (vanilla: Env_BoxNNN): applies the env map of the Env_PointNNN point with its env map number, and links to irradiance volume GIIV NNN.
            /// </summary>
            public class EnvironmentMapEffectBox : Region
            {
                private protected override RegionType Type => RegionType.EnvironmentMapEffectBox;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Distance from camera required before enabling envmap. 0 = always enabled.
                /// </summary>
                public float DisplayDistance { get; set; }

                /// <summary>
                /// Distance it takes for an envmap to fully transition into view.
                /// </summary>
                public float FadeDistance { get; set; }

                /// <summary>
                /// Applies parallax correction to the env map (non-zero = on).
                /// </summary>
                public byte ParallaxCorrection { get; set; }

                /// <summary>
                /// The box priority for MSB versions before 0x38. Later versions (every retail map with boxes) use
                /// Priority instead; 10 in retail.
                /// </summary>
                public byte LegacyPriority { get; set; } = 10;

                /// <summary>
                /// The env map number this box uses (Env_Point and GIIV volume). -1: the three digits at the end of the
                /// box name (Env_Box015 = 15).
                /// </summary>
                public short EnvMapNumber { get; set; } = -1;

                /// <summary>
                /// Extends the influence bounds of the box beyond its shape on the negative X/Y/Z sides (local, metres).
                /// The game uses min = -size/2 - this. 0 in retail.
                /// </summary>
                public Vector3 BoundsExpandNegative { get; set; }

                /// <summary>
                /// Extends the influence bounds of the box beyond its shape on the positive X/Y/Z sides (local, metres).
                /// The game uses max = size/2 + this. 0 in retail.
                /// </summary>
                public Vector3 BoundsExpandPositive { get; set; }

                /// <summary>
                /// Strength of ambient diffuse light.
                /// </summary>
                public float AmbientDiffuseIntensity { get; set; } = 1;

                /// <summary>
                /// Strength of ambient specular light.
                /// </summary>
                public float AmbientSpecularIntensity { get; set; } = 1;

                /// <summary>
                /// Selects two irradiance volume flags, stored in the volume record next to the ambient intensities: 0 = both
                /// on, 1 = only the second, 2 = only the first, other = default (both on). 0 in retail except two boxes (1).
                /// </summary>
                public byte UnkT2C { get; set; }

                /// <summary>
                /// Unknown. Copied with the UnkT2C flags but never read, so it has no effect in ER. 0 in retail.
                /// </summary>
                public byte UnkT2D { get; set; }

                /// <summary>
                /// Affects lighting with other fields when true. Possibly normalizes light when false.
                /// </summary>
                public bool IsModifyLight { get; set; } = true;

                /// <summary>
                /// Unknown. Passed to the irradiance volume setup (140B37F90), which never reads it: no effect in ER.
                /// </summary>
                public bool UnkT2F { get; set; } = true;

                /// <summary>
                /// A second env map / irradiance volume number for this box; when 0 or more, the volume setup runs a
                /// second time with it. -1: none.
                /// </summary>
                public short AdditionalEnvMapNumber { get; set; } = -1;

                /// <summary>
                /// Unknown. Passed to the irradiance volume setup (140B37F90), which never reads it: no effect in ER.
                /// </summary>
                public bool UnkT32 { get; set; }

                /// <summary>
                /// Unknown. Passed to the irradiance volume setup (140B37F90), which never reads it: no effect in ER.
                /// </summary>
                public bool UnkT33 { get; set; } = true;

                /// <summary>
                /// The box priority (MSB version 0x38 and later). Retail: 10, 20, 30, 21, 31, 22 and others.
                /// </summary>
                public short Priority { get; set; } = 10;

                /// <summary>
                /// Unknown. Passed (MSB version 0x3F and later; older versions pass 1) to the irradiance volume setup
                /// (140B37F90), which never reads it: no effect in ER.
                /// </summary>
                public bool UnkT36 { get; set; } = true;

                /// <summary>
                /// Creates an EnvironmentMapEffectBox with default values.
                /// </summary>
                public EnvironmentMapEffectBox() : base($"{nameof(Region)}: {nameof(EnvironmentMapEffectBox)}") { }

                internal EnvironmentMapEffectBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    DisplayDistance = br.ReadSingle();
                    FadeDistance = br.ReadSingle();
                    ParallaxCorrection = br.ReadByte();
                    LegacyPriority = br.ReadByte();
                    EnvMapNumber = br.ReadInt16();
                    BoundsExpandNegative = br.ReadVector3();
                    BoundsExpandPositive = br.ReadVector3();
                    AmbientDiffuseIntensity = br.ReadSingle();
                    AmbientSpecularIntensity = br.ReadSingle();
                    UnkT2C = br.ReadByte();
                    UnkT2D = br.ReadByte();
                    IsModifyLight = br.ReadBoolean();
                    UnkT2F = br.ReadBoolean();
                    AdditionalEnvMapNumber = br.ReadInt16();
                    UnkT32 = br.ReadBoolean();
                    UnkT33 = br.ReadBoolean();
                    Priority = br.ReadInt16();
                    UnkT36 = br.ReadBoolean();
                    br.AssertByte(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(DisplayDistance);
                    bw.WriteSingle(FadeDistance);
                    bw.WriteByte(ParallaxCorrection);
                    bw.WriteByte(LegacyPriority);
                    bw.WriteInt16(EnvMapNumber);
                    bw.WriteVector3(BoundsExpandNegative);
                    bw.WriteVector3(BoundsExpandPositive);
                    bw.WriteSingle(AmbientDiffuseIntensity);
                    bw.WriteSingle(AmbientSpecularIntensity);
                    bw.WriteByte(UnkT2C);
                    bw.WriteByte(UnkT2D);
                    bw.WriteBoolean(IsModifyLight);
                    bw.WriteBoolean(UnkT2F);
                    bw.WriteInt16(AdditionalEnvMapNumber);
                    bw.WriteBoolean(UnkT32);
                    bw.WriteBoolean(UnkT33);
                    bw.WriteInt16(Priority);
                    bw.WriteBoolean(UnkT36);
                    bw.WriteByte(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An area for wind effects (WindSFX reference it); no type data. NR calls the type WindPlacement.
            /// </summary>
            public class WindArea : Region
            {
                private protected override RegionType Type => RegionType.WindArea;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a WindArea with default values.
                /// </summary>
                public WindArea() : base($"{nameof(Region)}: {nameof(WindArea)}") { }

                internal WindArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Used to align different maps.
            /// </summary>
            public class Connection : Region
            {
                private protected override RegionType Type => RegionType.Connection;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Map ID this connection targets.
                /// </summary>
                public sbyte[] TargetMapID { get; set; }

                /// <summary>
                /// Creates a Connection with default values.
                /// </summary>
                public Connection() : base($"{nameof(Region)}: {nameof(Connection)}") 
                {
                    TargetMapID = new sbyte[4];
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var connect = (Connection)region;
                    connect.TargetMapID = (sbyte[])TargetMapID.Clone();
                }

                internal Connection(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    TargetMapID = br.ReadSBytes(4);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSBytes(TargetMapID);
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0);
                    bw.WriteUInt32(0);
                }
            }

            /// <summary>
            /// An automatically generated walk route point (vanilla: walk_route_MAP-CHR_N). NR calls the type SourceWaypoint.
            /// </summary>
            public class PatrolRoute22 : Region
            {
                private protected override RegionType Type => RegionType.PatrolRoute22;
                private protected override bool HasTypeData => true;


                /// <summary>
                /// Creates a PatrolRoute22 with default values.
                /// </summary>
                public PatrolRoute22() : base($"{nameof(Region)}: {nameof(PatrolRoute22)}") { }

                internal PatrolRoute22(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A spirit ash summon point (vanilla: バディ召喚ポイント), carrying an entity ID. No lookup of the type by number was found in the exe; its type data is four zero ints.
            /// </summary>
            public class BuddySummonPoint : Region
            {
                private protected override RegionType Type => RegionType.BuddySummonPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a BuddySummonPoint with default values.
                /// </summary>
                public BuddySummonPoint() : base($"{nameof(Region)}: {nameof(BuddySummonPoint)}") { }

                internal BuddySummonPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// The exe's CSMsbPointOcclusionBox (formerly MufflingBox): a room of the sound occlusion graph. Its
            /// OcclusionPortals are the openings, and routes (MufflingPortalLink / MufflingBoxLink) join the portals.
            /// </summary>
            public class OcclusionBox : Region
            {
                private protected override RegionType Type => RegionType.OcclusionBox;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The room number. Several boxes can share a number and form one room; each OcclusionPortal names the
                /// room it belongs to with the same number.
                /// </summary>
                public int BoxNumber { get; set; }

                /// <summary>
                /// Unknown. 100 in every retail box; not seen read.
                /// </summary>
                public float UnkT24 { get; set; }

                /// <summary>
                /// Unknown. 100 in every retail box; not seen read.
                /// </summary>
                public float UnkT34 { get; set; }

                /// <summary>
                /// Unknown. -1 in every retail box; not seen read.
                /// </summary>
                public float UnkT3C { get; set; }

                /// <summary>
                /// Unknown. -1 in every retail box; not seen read.
                /// </summary>
                public float UnkT40 { get; set; }

                /// <summary>
                /// Unknown. -1 in every retail box; not seen read.
                /// </summary>
                public float UnkT44 { get; set; }

                /// <summary>
                /// Creates a OcclusionBox with default values.
                /// </summary>
                public OcclusionBox() : base($"{nameof(Region)}: {nameof(OcclusionBox)}") { }

                internal OcclusionBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    BoxNumber = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(32);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT24 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    UnkT34 = br.ReadSingle();
                    br.AssertInt32(0);
                    UnkT3C = br.ReadSingle();
                    UnkT40 = br.ReadSingle();
                    UnkT44 = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(BoxNumber);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(32);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT24);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT34);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT3C);
                    bw.WriteSingle(UnkT40);
                    bw.WriteSingle(UnkT44);
                }
            }

            /// <summary>
            /// The exe's CSMsbPointOcclusionPortal (formerly MufflingPortal): an opening of an OcclusionBox. Routes
            /// reference portals by their RegionID.
            /// </summary>
            public class OcclusionPortal : Region
            {
                private protected override RegionType Type => RegionType.OcclusionPortal;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The BoxNumber of the OcclusionBox this portal belongs to (414 of 414 retail portals match a box in
                /// their map).
                /// </summary>
                public int BoxNumber { get; set; }

                /// <summary>
                /// Creates a OcclusionPortal with default values.
                /// </summary>
                public OcclusionPortal() : base($"{nameof(Region)}: {nameof(OcclusionPortal)}") { }

                internal OcclusionPortal(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    BoxNumber = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(32);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(-1);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(BoxNumber);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(32);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(-1);
                }
            }

            /// <summary>
            /// The exe's CSMsbPointSoundRegion: overrides map sound settings while the listener is inside it. The
            /// sound-region manager picks the active region separately for each field; a field only counts when it
            /// is not -1 and the region's shape is a Box.
            /// </summary>
            public class SoundRegion : Region
            {
                private protected override RegionType Type => RegionType.SoundRegion;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Reverb type used inside the region, replacing MapDefaultInfoParam ReverbDefaultType and the automatic
                /// reverb selection. -1: no override.
                /// </summary>
                public sbyte ReverbType { get; set; } = -1;

                /// <summary>
                /// Water body type for the region's water sound: 0 none, 1 sea (海), 2 lake (湖), 3 river (川). Regions
                /// with the same WaterType and WaterID form one water sound, named "type_ID:n" by the game.
                /// </summary>
                public byte WaterType { get; set; }

                /// <summary>
                /// ID of the water sound group within its WaterType. Only used when WaterType is above 0.
                /// </summary>
                public byte WaterID { get; set; }

                /// <summary>
                /// Outdoor/indoor override, like Collision OutdoorIndoorType: 0 outdoor, 1 half indoor, 2 indoor
                /// (Wwise OutdoorIndoor states). -1 = no override.
                /// </summary>
                public sbyte OutdoorIndoorType { get; set; } = -1;

                /// <summary>
                /// Wwise environment place type used inside the region. -1 = no override.
                /// </summary>
                [MSBParamReference(ParamName = "WwiseValueToStrParam_EnvPlaceType")]
                public int WwiseEnvPlaceTypeID { get; set; } = -1;

                /// <summary>
                /// Replaces MapDefaultInfoParam EnvPlaceInfo inside the region; the game looks it up in
                /// WwiseValueToStrParam_EnvPlaceType and sets the Wwise state from its string. -1 = no override.
                /// </summary>
                [MSBParamReference(ParamName = "WwiseValueToStrParam_EnvPlaceType")]
                public short EnvPlaceInfo { get; set; } = -1;

                /// <summary>
                /// Common place type used inside the region (-1 = none); the sound update switches to it after a
                /// 2-second delay.
                /// </summary>
                public short CommonPlaceType { get; set; } = -1;

                /// <summary>
                /// Replaces MapDefaultInfoParam isDisableAutoCliffWind inside the region: 1 disables the automatic
                /// cliff wind sound, 0 enables it, -1 = no override. Only read in DLC (m61) maps.
                /// </summary>
                public sbyte DisableAutoCliffWind { get; set; } = -1;

                /// <summary>
                /// Creates a SoundRegion with default values.
                /// </summary>
                public SoundRegion() : base($"{nameof(Region)}: {nameof(SoundRegion)}") { }

                internal SoundRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ReverbType = br.ReadSByte();
                    WaterType = br.ReadByte();
                    WaterID = br.ReadByte();
                    OutdoorIndoorType = br.ReadSByte();
                    WwiseEnvPlaceTypeID = br.ReadInt32();
                    EnvPlaceInfo = br.ReadInt16();
                    CommonPlaceType = br.ReadInt16();
                    DisableAutoCliffWind = br.ReadSByte();
                    br.AssertPattern(19, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSByte(ReverbType);
                    bw.WriteByte(WaterType);
                    bw.WriteByte(WaterID);
                    bw.WriteSByte(OutdoorIndoorType);
                    bw.WriteInt32(WwiseEnvPlaceTypeID);
                    bw.WriteInt16(EnvPlaceInfo);
                    bw.WriteInt16(CommonPlaceType);
                    bw.WriteSByte(DisableAutoCliffWind);
                    bw.WritePattern(19, 0x00);
                }
            }

            /// <summary>
            /// Sound muffling plane. No retail ER map uses it and the game never references the type.
            /// </summary>
            public class MufflingPlane : Region
            {
                private protected override RegionType Type => RegionType.MufflingPlane;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a MufflingPlane with default values.
                /// </summary>
                public MufflingPlane() : base($"{nameof(Region)}: {nameof(MufflingPlane)}") { }

                internal MufflingPlane(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A point along an NPC patrol path.
            /// </summary>
            public class PatrolRoute : Region
            {
                private protected override RegionType Type => RegionType.PatrolRoute;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Action ID that AI scripts read for this point through the Lua function GetMovePointActionId.
                /// -1: none. Retail mostly 900001-900006.
                /// </summary>
                public int ActionID { get; set; } = -1;

                /// <summary>
                /// Creates a PatrolRoute with default values.
                /// </summary>
                public PatrolRoute() : base($"{nameof(Region)}: {nameof(PatrolRoute)}") { }

                internal PatrolRoute(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ActionID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ActionID);
                }
            }

            /// <summary>
            /// The exe's CSMsbPointWorldMapPoint (formerly MapPoint): a point shown on the world map, with a discovery
            /// area and a re-entry area.
            /// </summary>
            public class WorldMapPoint : Region
            {
                private protected override RegionType Type => RegionType.WorldMapPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Determines which WorldMapPointParam to use.
                /// </summary>
                public int WorldMapPointParamID { get; set; }

                /// <summary>
                /// A WorldMapDiscoveryPoint region that replaces this point's discovery area. Empty: the discovery
                /// area is this region's shape, expanded by DiscoveryExpandRangeXZ / DiscoveryExpandRangeY.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string DiscoveryRegionName { get; set; }
                private int DiscoveryRegionIndex { get; set; } = -1;

                /// <summary>
                /// Horizontal expansion of the shape for the discovery area. Negative: GameSystemCommonParam
                /// worldMapPointDiscoveryExpandRange.
                /// </summary>
                public float DiscoveryExpandRangeXZ { get; set; } = -1;

                /// <summary>
                /// Vertical expansion of the shape for the discovery area. Negative: GameSystemCommonParam
                /// worldMapPointDiscoveryExpandRange.
                /// </summary>
                public float DiscoveryExpandRangeY { get; set; } = -1;

                /// <summary>
                /// A WorldMapReentryPoint region that replaces this point's re-entry area. Empty: the re-entry
                /// area is this region's shape, expanded by ReentryExpandRangeXZ / ReentryExpandRangeY.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string ReentryRegionName { get; set; }
                private int ReentryRegionIndex { get; set; } = -1;

                /// <summary>
                /// Horizontal expansion of the shape for the re-entry area. Negative: GameSystemCommonParam
                /// worldMapPointReentryExpandRange.
                /// </summary>
                public float ReentryExpandRangeXZ { get; set; } = -1;

                /// <summary>
                /// Vertical expansion of the shape for the re-entry area. Negative: GameSystemCommonParam
                /// worldMapPointReentryExpandRange.
                /// </summary>
                public float ReentryExpandRangeY { get; set; } = -1;

                /// <summary>
                /// Creates a WorldMapPoint with default values.
                /// </summary>
                public WorldMapPoint() : base($"{nameof(Region)}: {nameof(WorldMapPoint)}") { }

                internal WorldMapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WorldMapPointParamID = br.ReadInt32();
                    DiscoveryRegionIndex = br.ReadInt32();
                    DiscoveryExpandRangeXZ = br.ReadSingle();
                    DiscoveryExpandRangeY = br.ReadSingle();
                    ReentryRegionIndex = br.ReadInt32();
                    ReentryExpandRangeXZ = br.ReadSingle();
                    ReentryExpandRangeY = br.ReadSingle();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(WorldMapPointParamID);
                    bw.WriteInt32(DiscoveryRegionIndex);
                    bw.WriteSingle(DiscoveryExpandRangeXZ);
                    bw.WriteSingle(DiscoveryExpandRangeY);
                    bw.WriteInt32(ReentryRegionIndex);
                    bw.WriteSingle(ReentryExpandRangeXZ);
                    bw.WriteSingle(ReentryExpandRangeY);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    DiscoveryRegionName = MSB.FindName(entries.Regions, DiscoveryRegionIndex);
                    ReentryRegionName = MSB.FindName(entries.Regions, ReentryRegionIndex);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    DiscoveryRegionIndex = MSB.FindIndex(this, entries.Regions, DiscoveryRegionName);
                    ReentryRegionIndex = MSB.FindIndex(this, entries.Regions, ReentryRegionName);
                }
            }

            /// <summary>
            /// The exe's CSWeatherLotRegionMan region (formerly WeatherOverride). While the player is inside, each
            /// field that is not -1 overrides the map's value; with overlapping regions, the first match per field wins.
            /// </summary>
            public class WeatherLotRegion : Region
            {
                private protected override RegionType Type => RegionType.WeatherLotRegion;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Determines which WeatherLotParam ID to use. -1: no override.
                /// </summary>
                public int WeatherLotParamID { get; set; } = -1;

                /// <summary>
                /// Unknown, -1 = none (always -1 in retail; the loader sets it to -1 for MSB versions before 0x42).
                /// The manager merges it like the other fields and stores it, but nothing in the exe reads the stored
                /// value, so it has no effect in ER.
                /// </summary>
                public int UnkT04 { get; set; } = -1;

                /// <summary>
                /// Overrides MimicryEstablishmentTexParam isApplyMimicryLotLimitSpEffect0: -1 none, 0 off, 1 on. The
                /// loader sets the three overrides to -1 for MSB versions before 0x4A.
                /// </summary>
                public sbyte ApplyMimicryLotLimitSpEffect0 { get; set; } = -1;

                /// <summary>
                /// Overrides MimicryEstablishmentTexParam isApplyMimicryLotLimitSpEffect1: -1 none, 0 off, 1 on.
                /// </summary>
                public sbyte ApplyMimicryLotLimitSpEffect1 { get; set; } = -1;

                /// <summary>
                /// Overrides MimicryEstablishmentTexParam isApplyMimicryLotLimitSpEffect2: -1 none, 0 off, 1 on.
                /// </summary>
                public sbyte ApplyMimicryLotLimitSpEffect2 { get; set; } = -1;

                /// <summary>
                /// Unknown; 0 in retail and never read.
                /// </summary>
                public sbyte UnkT0B { get; set; }

                /// <summary>
                /// Creates a WeatherLotRegion with default values.
                /// </summary>
                public WeatherLotRegion() : base($"{nameof(Region)}: {nameof(WeatherLotRegion)}") { }

                internal WeatherLotRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WeatherLotParamID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    ApplyMimicryLotLimitSpEffect0 = br.ReadSByte();
                    ApplyMimicryLotLimitSpEffect1 = br.ReadSByte();
                    ApplyMimicryLotLimitSpEffect2 = br.ReadSByte();
                    UnkT0B = br.ReadSByte();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(WeatherLotParamID);
                    bw.WriteInt32(UnkT04);
                    bw.WriteSByte(ApplyMimicryLotLimitSpEffect0);
                    bw.WriteSByte(ApplyMimicryLotLimitSpEffect1);
                    bw.WriteSByte(ApplyMimicryLotLimitSpEffect2);
                    bw.WriteSByte(UnkT0B);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A sample point for the offline automatic draw-group generation (NR names the type
            /// AutoDrawGroupSample). Retail points are all named AutoDrawGroupPoint_NNN, in legacy dungeons only.
            /// Never read by the game.
            /// </summary>
            public class AutoDrawGroupPoint : Region
            {
                private protected override RegionType Type => RegionType.AutoDrawGroupPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown. -1, or a value always below the map's collision count (362 of 362 retail points), so
                /// probably an index into the map's collisions. Never read by the game.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a AutoDrawGroupPoint with default values.
                /// </summary>
                public AutoDrawGroupPoint() : base($"{nameof(Region)}: {nameof(AutoDrawGroupPoint)}") { }

                internal AutoDrawGroupPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A group placement point (the game's CSMsbPointGroupPlacementPoint): a group of enemies, placed from a
            /// RandomAppearParam row and tied to platoons, that gives a reward when all of them are defeated.
            /// </summary>
            public class GroupPlacementPoint : Region
            {
                private protected override RegionType Type => RegionType.GroupPlacementPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// RandomAppearParam row the group's enemies are placed from; 0 for none. 7 of the 12 distinct retail
                /// values are rows; the other 5 are on test points.
                /// </summary>
                public int RandomAppearParamID { get; set; }

                /// <summary>
                /// PlatoonInfo events of the group (up to 4); the game checks a defeated character's platoon against them.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event))]
                public string[] PlatoonEventNames { get; set; }
                private int[] PlatoonEventIndices { get; set; }

                /// <summary>
                /// References to enemies to defeat to receive the reward.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] PartNames { get; set; }
                private int[] PartIndices { get; set; }

                /// <summary>
                /// Assets tied to the group (up to 8). The group registers itself on each asset, and the asset only
                /// activates while the group does. Retail: three regions, all with a RandomAppearParamID.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] LinkedAssetNames { get; set; }
                private int[] LinkedAssetIndices { get; set; }

                /// <summary>
                /// HP/MPEstusFlaskRecoveryParam row applied when the whole group is defeated.
                /// </summary>
                public int EstusFlaskRecoveryID { get; set; }

                /// <summary>
                /// Creates a GroupPlacementPoint with default values.
                /// </summary>
                public GroupPlacementPoint() : base($"{nameof(Region)}: {nameof(GroupPlacementPoint)}")
                {
                    PlatoonEventNames = new string[4];
                    PartNames = new string[8];
                    LinkedAssetNames = new string[8];
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var reward = (GroupPlacementPoint)region;
                    reward.PlatoonEventNames = (string[])PlatoonEventNames.Clone();
                    reward.PartNames = (string[])PartNames.Clone();
                    reward.LinkedAssetNames = (string[])LinkedAssetNames.Clone();
                }

                internal GroupPlacementPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    RandomAppearParamID = br.ReadInt32();
                    PlatoonEventIndices = br.ReadInt32s(4);
                    PartIndices = br.ReadInt32s(8);
                    LinkedAssetIndices = br.ReadInt32s(8);
                    EstusFlaskRecoveryID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(RandomAppearParamID);
                    bw.WriteInt32s(PlatoonEventIndices);
                    bw.WriteInt32s(PartIndices);
                    bw.WriteInt32s(LinkedAssetIndices);
                    bw.WriteInt32(EstusFlaskRecoveryID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    PlatoonEventNames = MSB.FindNames(entries.Events, PlatoonEventIndices);
                    PartNames = MSB.FindNames(entries.Parts, PartIndices);
                    LinkedAssetNames = MSB.FindNames(entries.Parts, LinkedAssetIndices);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    PlatoonEventIndices = MSB.FindIndices(this, entries.Events, PlatoonEventNames);
                    PartIndices = MSB.FindIndices(this, entries.Parts, PartNames);
                    LinkedAssetIndices = MSB.FindIndices(this, entries.Parts, LinkedAssetNames);
                }
            }
            /// <summary>
            /// The exe's CSWorldMapDiscoveryPointIns (formerly MapPointDiscoveryOverride): the area in which the
            /// player discovers a map point. A WorldMapPoint uses it when its DiscoveryRegionName names this region;
            /// otherwise the game expands the WorldMapPoint's own shape.
            /// </summary>
            public class WorldMapDiscoveryPoint : Region
            {
                private protected override RegionType Type => RegionType.WorldMapDiscoveryPoint;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an WorldMapDiscoveryPoint with default values.
                /// </summary>
                public WorldMapDiscoveryPoint() : base($"{nameof(Region)}: {nameof(WorldMapDiscoveryPoint)}") { }

                internal WorldMapDiscoveryPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// The exe's CSWorldMapReentryPointIns (formerly MapPointParticipationOverride; vanilla: 出場領域). The
            /// re-entry area of a map point. A WorldMapPoint uses it when its ReentryRegionName names this region;
            /// otherwise the game expands the WorldMapPoint's own shape.
            /// </summary>
            public class WorldMapReentryPoint : Region
            {
                private protected override RegionType Type => RegionType.WorldMapReentryPoint;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an WorldMapReentryPoint with default values.
                /// </summary>
                public WorldMapReentryPoint() : base($"{nameof(Region)}: {nameof(WorldMapReentryPoint)}") { }

                internal WorldMapReentryPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A build-time marker named after a collision model (hitset_hAABBCCNN), usually one tile-sized box per
            /// open-world tile. Never read by the game.
            /// </summary>
            public class Hitset : Region
            {
                private protected override RegionType Type => RegionType.Hitset;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown set ID (-1 to ~150), shared by the sub-boxes of one collision. Never read by the game.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a Hitset with default values.
                /// </summary>
                public Hitset() : base($"{nameof(Region)}: {nameof(Hitset)}") { }

                internal Hitset(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                }
            }

            /// <summary>
            /// The exe's CSFastTravelOverrideRegionIns (formerly FastTravelRestriction): while the player is inside it,
            /// its flag replaces the EnableFastTravelEventFlagID of the collision and of MapDefaultInfoParam.
            /// </summary>
            public class FastTravelOverride : Region
            {
                private protected override RegionType Type => RegionType.FastTravelOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Fast travel is allowed only while this event flag is ON. 0: no override.
                /// </summary>
                public uint EnableFastTravelEventFlagID { get; set; }

                /// <summary>
                /// Creates a FastTravelOverride with default values.
                /// </summary>
                public FastTravelOverride() : base($"{nameof(Region)}: {nameof(FastTravelOverride)}") { }

                internal FastTravelOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EnableFastTravelEventFlagID = br.ReadUInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(EnableFastTravelEventFlagID);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A spawn point for weather assets. WeatherAssetCreateParam CreatePoint0-3 pick it by the 3-digit suffix
            /// of its name, which must be exactly "WeatherCreateAssetPoint_NNN"; the asset is created at its position
            /// and rotation. The type data is never read.
            /// </summary>
            public class WeatherCreateAssetPoint : Region
            {
                private protected override RegionType Type => RegionType.WeatherCreateAssetPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a WeatherCreateAssetPoint with default values.
                /// </summary>
                public WeatherCreateAssetPoint() : base($"{nameof(Region)}: {nameof(WeatherCreateAssetPoint)}") { }

                internal WeatherCreateAssetPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// The exe's CSMsbPointPlayRegion (formerly PlayArea): inside it, its PlayRegionID competes with the
            /// PlayRegionID of the collision the player stands on. The one with the higher priority wins (collisions
            /// have a fixed priority of 10); a tie goes to the larger ID.
            /// </summary>
            public class PlayRegion : Region
            {
                private protected override RegionType Type => RegionType.PlayRegion;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The PlayRegionParam row used inside this region. -1: the region is ignored.
                /// </summary>
                [MSBParamReference(ParamName = "PlayRegionParam")]
                public int PlayRegionID { get; set; } = -1;

                /// <summary>
                /// Priority against the collision's play region, whose priority is always 10. Negative: the region is
                /// ignored. The game reads it as a signed short.
                /// </summary>
                public short Priority { get; set; }

                /// <summary>
                /// Creates a PlayRegion with default values.
                /// </summary>
                public PlayRegion() : base($"{nameof(Region)}: {nameof(PlayRegion)}") { }

                internal PlayRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PlayRegionID = br.ReadInt32();
                    Priority = br.ReadInt16();
                    br.AssertInt16(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt16(Priority);
                    bw.WriteInt16(0);
                }
            }

            /// <summary>
            /// Mid-range environment map output marker (vanilla: 中景環境マップ出力目印ポイント). A build-time marker in
            /// the _10 LOD maps; never read by the game.
            /// </summary>
            public class EnvironmentMapOutput : Region
            {
                private protected override RegionType Type => RegionType.EnvironmentMapOutput;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a EnvironmentMapOutput with default values.
                /// </summary>
                public EnvironmentMapOutput() : base($"{nameof(Region)}: {nameof(EnvironmentMapOutput)}") { }

                internal EnvironmentMapOutput(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// The exe's ride jump region in CSRideJumpRegionManImpl (formerly MountJump): a spiritspring that
            /// launches Torrent upwards, marked by an SFX.
            /// </summary>
            public class RideJump : Region
            {
                private protected override RegionType Type => RegionType.RideJump;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Height the player will move upwards when activating this spiritspring.
                /// </summary>
                public float JumpHeight { get; set; }

                /// <summary>
                /// SFX shown at the spiritspring. Negative uses GameSystemCommonParam rideJumpRegionDefaultSfxId (806730 in retail).
                /// </summary>
                public int SfxID { get; set; } = -1;

                /// <summary>
                /// Creates a RideJump with default values.
                /// </summary>
                public RideJump() : base($"{nameof(Region)}: {nameof(RideJump)}") { }

                internal RideJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpHeight = br.ReadSingle();
                    SfxID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(JumpHeight);
                    bw.WriteInt32(SfxID);
                }
            }

            /// <summary>
            /// A sound dummy point (formerly Dummy; NR calls it SoundDummy). The sound system registers its world
            /// position and feeds the listener's distance to it into the Wwise RTPC DistanceFromDummyPoint_0N, where N is
            /// DummyPointIndex.
            /// </summary>
            public class SoundDummy : Region
            {
                private protected override RegionType Type => RegionType.SoundDummy;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Which RTPC the point drives: 0-4 = DistanceFromDummyPoint_00-04. The game reads a byte and clamps it to 5.
                /// </summary>
                public byte DummyPointIndex { get; set; }

                /// <summary>
                /// Creates a SoundDummy with default values.
                /// </summary>
                public SoundDummy() : base($"{nameof(Region)}: {nameof(SoundDummy)}") { }

                internal SoundDummy(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    DummyPointIndex = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(DummyPointIndex);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Build-time marker: the generated fall-prevention walls are left out inside it (vanilla: 落下防止壁削除). Never read by the game.
            /// </summary>
            public class FallPreventionRemoval : Region
            {
                private protected override RegionType Type => RegionType.FallPreventionRemoval;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a FallPreventionRemoval with default values.
                /// </summary>
                public FallPreventionRemoval() : base($"{nameof(Region)}: {nameof(FallPreventionRemoval)}") { }

                internal FallPreventionRemoval(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Build-time marker for the offline navmesh build (vanilla: navmesh adjacent-cutting merge areas). Never read by the game.
            /// </summary>
            public class NavmeshCutting : Region
            {
                private protected override RegionType Type => RegionType.NavmeshCutting;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a NavmeshCutting with default values.
                /// </summary>
                public NavmeshCutting() : base($"{nameof(Region)}: {nameof(NavmeshCutting)}") { }

                internal NavmeshCutting(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// The exe's CSMapPlaceNameOverrideRegionMan region (formerly MapNameOverride): while the player is inside
            /// it, its LocationTextID replaces the one from the collision the player stands on.
            /// </summary>
            public class MapPlaceNameOverride : Region
            {
                private protected override RegionType Type => RegionType.MapPlaceNameOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Place-name text (PlaceName FMG) used instead of the collision's LocationTextID, with the same sign
                /// convention: negative values show the title card when entering, positive ones on map load.
                /// </summary>
                public int LocationTextID { get; set; }

                /// <summary>
                /// Creates a MapPlaceNameOverride with default values.
                /// </summary>
                public MapPlaceNameOverride() : base($"{nameof(Region)}: {nameof(MapPlaceNameOverride)}") { }

                internal MapPlaceNameOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    LocationTextID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(LocationTextID);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// The exe's ride jump-off region in CSRideJumpRegionManImpl (formerly MountJumpFall): the descent area of
            /// a spiritspring. Riding into it gives the rider and Torrent the jump-off SpEffects
            /// (GameSystemCommonParam rideJumpoff_SpEffectIdPc / rideJumpoff_SpEffectId), and an SFX can mark it.
            /// </summary>
            public class RideJumpoff : Region
            {
                private protected override RegionType Type => RegionType.RideJumpoff;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// SFX shown at the region. Negative uses GameSystemCommonParam rideJumpoff_SfxId (-1 in retail: none).
                /// </summary>
                public int SfxID { get; set; } = -1;

                /// <summary>
                /// Height of the SFX above the region's position. 0 or less uses GameSystemCommonParam
                /// rideJumpoff_SfxHeightOffset.
                /// </summary>
                public float SfxHeightOffset { get; set; } = 0;

                /// <summary>
                /// Creates a RideJumpoff with default values.
                /// </summary>
                public RideJumpoff() : base($"{nameof(Region)}: {nameof(RideJumpoff)}") { }

                internal RideJumpoff(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SfxID = br.ReadInt32();
                    SfxHeightOffset = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SfxID);
                    bw.WriteSingle(SfxHeightOffset);
                }
            }

            /// <summary>
            /// The exe's CSDisableHorseOverrideRegionMan region (formerly HorseRideOverride): overrides whether Torrent
            /// can be summoned and ridden inside it, ahead of the collision's own setting.
            /// </summary>
            public class DisableHorseOverride : Region
            {
                private protected override RegionType Type => RegionType.DisableHorseOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// 1 = riding disabled, 2 = riding allowed (cancels a disable from the map). Other values: no override.
                /// The game reads it as a byte.
                /// </summary>
                public byte OverrideType { get; set; } = 1;

                /// <summary>
                /// Creates a DisableHorseOverride with default values.
                /// </summary>
                public DisableHorseOverride() : base($"{nameof(Region)}: {nameof(DisableHorseOverride)}") { }

                internal DisableHorseOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    OverrideType = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(OverrideType);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// The exe's CSDlcRideJumpRegionIns (formerly LockedMountJump): a sealable DLC spiritspring. It only
            /// launches Torrent while its event flag is ON; it shows one SFX while active and another while sealed.
            /// </summary>
            public class DlcRideJump : Region
            {
                private protected override RegionType Type => RegionType.DlcRideJump;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Height the player will move upwards when activating this spiritspring.
                /// </summary>
                public float JumpHeight { get; set; }

                /// <summary>
                /// SFX shown while the spiritspring is active. Negative uses GameSystemCommonParam
                /// dlcRideJumpRegionDefaultSfxId (841070 in retail).
                /// </summary>
                public int SfxID { get; set; } = -1;

                /// <summary>
                /// The spiritspring is active only while this event flag is ON. 0 or -1: always active.
                /// </summary>
                public int EnableEventFlagID { get; set; }

                /// <summary>
                /// SFX shown while the spiritspring is sealed (flag OFF). Negative uses GameSystemCommonParam
                /// dlcRideJumpRegionDefaultDisableSfxId (841071 in retail).
                /// </summary>
                public int DisabledSfxID { get; set; } = -1;

                /// <summary>
                /// Creates a DlcRideJump with default values.
                /// </summary>
                public DlcRideJump() : base($"{nameof(Region)}: {nameof(DlcRideJump)}") { }

                internal DlcRideJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpHeight = br.ReadSingle();
                    SfxID = br.ReadInt32();
                    EnableEventFlagID = br.ReadInt32();
                    DisabledSfxID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(JumpHeight);
                    bw.WriteInt32(SfxID);
                    bw.WriteInt32(EnableEventFlagID);
                    bw.WriteInt32(DisabledSfxID);
                }
            }

            /// <summary>
            /// The exe's CSDlcRideJumpoffRegionIns (formerly LockedMountJumpFall): the landing/descent area of a
            /// sealable DLC spiritspring. While it is active, riding into it gives the rider and Torrent the jump-off
            /// SpEffects (GameSystemCommonParam rideJumpoff_SpEffectIdPc / rideJumpoff_SpEffectId), and an SFX can
            /// mark it.
            /// </summary>
            public class DlcRideJumpoff : Region
            {
                private protected override RegionType Type => RegionType.DlcRideJumpoff;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// SFX shown at the region. Negative uses GameSystemCommonParam dlcRideJumpoff_SfxId (-1 in retail: none).
                /// </summary>
                public int SfxID { get; set; } = -1;

                /// <summary>
                /// Height of the SFX above the region's position. 0 or less uses GameSystemCommonParam
                /// dlcRideJumpoff_SfxHeightOffset.
                /// </summary>
                public float SfxHeightOffset { get; set; } = 0;

                /// <summary>
                /// The region is active only while this event flag is ON. 0 or -1: always active.
                /// </summary>
                public int EnableEventFlagID { get; set; }

                /// <summary>
                /// Creates a DlcRideJumpoff with default values.
                /// </summary>
                public DlcRideJumpoff() : base($"{nameof(Region)}: {nameof(DlcRideJumpoff)}") { }

                internal DlcRideJumpoff(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SfxID = br.ReadInt32();
                    SfxHeightOffset = br.ReadSingle();
                    EnableEventFlagID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SfxID);
                    bw.WriteSingle(SfxHeightOffset);
                    bw.WriteInt32(EnableEventFlagID);
                }

            }

            /// <summary>
            /// Inside this region, rolling objects (tumbleweeds and similar) spawn from RollingObjLotParamID
            /// instead of the map tile's own lot. Formerly named DisableTumbleweed.
            /// </summary>
            public class RollingObjectOverride : Region
            {
                private protected override RegionType Type => RegionType.RollingObjectOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The RollingObjLotParam row used for spawns inside the region. -1 means nothing spawns.
                /// </summary>
                [MSBParamReference(ParamName = "RollingObjLotParam")]
                public int RollingObjLotParamID { get; set; }

                /// <summary>
                /// Creates a RollingObjectOverride with default values.
                /// </summary>
                public RollingObjectOverride() : base($"{nameof(Region)}: {nameof(RollingObjectOverride)}")
                {
                    RollingObjLotParamID = -1;
                }

                internal RollingObjectOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    RollingObjLotParamID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(RollingObjLotParamID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Most likely an unused region.
            /// </summary>
            public class Other : Region
            {
                private protected override RegionType Type => RegionType.Other;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Region)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
