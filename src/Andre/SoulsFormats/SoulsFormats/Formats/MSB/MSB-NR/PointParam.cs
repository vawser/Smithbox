using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSB_NR
    {
        public enum RegionType : uint
        {
            // Unused = 0
            EntryPoint = 1,
            EnvMapPoint = 2,
            RespawnPoint = 3,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            // Unused = 7
            ReturnPoint = 8,
            Message = 9,
            // Unused = 10
            // Unused = 11
            // Unused = 12
            // Unused = 13
            // Unused = 14
            // Unused = 15
            // Unused = 16
            EnvMapEffectBox = 17,
            WindPlacement = 18,
            // Unused = 19
            // Unused = 20
            MapConnection = 21,
            SourceWaypoint = 22,
            StaticWaypoint = 23,
            MapGridLayerConnection = 24,
            EnemySpawnPoint = 25,
            BuddySummonPoint = 26,
            RollingObjectOverride = 27,
            OcclusionBox = 28,
            OcclusionPortal = 29,
            SoundRegion = 30,
            MufflingPlane = 31,
            PatrolPoint = 32,
            WorldMapPoint = 33,
            SoundState = 34,
            WeatherLotRegion = 35,
            AutoDrawGroupSample = 36,
            GroupPlacementPoint = 37,
            WorldMapDiscoveryPoint = 38,
            WorldMapReentryPoint = 39,
            HitSetting = 40,
            FastTravelOverride = 41,
            WeatherAssetGeneration = 42,
            PlayRegion = 43,
            MidRangeEnvMapOutput = 44,
            MapVisibilityOverride = 45,
            RideJump = 46,
            OpenCharacterActivateLimit = 47,
            SoundDummy = 48,
            FallPreventionOverride = 49,
            NavmeshCutting = 50,
            MapPlaceNameOverride = 51,
            BigJumpExit = 52,
            MountOverride = 53,
            SmallBaseAttach = 54,
            BirdRoute = 55,
            ClearInfo = 56,
            RespawnOverride = 57,
            UserEdgeRemovalInner = 58,
            UserEdgeRemovalOuter = 59,
            DlcRideJump = 60,
            Other = 0xFFFFFFFF,
        }

        /// <summary>
        /// Points and volumes used to trigger various effects.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion>
        {
            public List<Region.EntryPoint> EntryPoints { get; set; }
            public List<Region.EnvMapPoint> EnvMapPoints { get; set; }
            public List<Region.RespawnPoint> RespawnPoints { get; set; }
            public List<Region.Sound> Sounds { get; set; }
            public List<Region.SFX> SFX { get; set; }
            public List<Region.WindSFX> WindSFX { get; set; }
            public List<Region.ReturnPoint> ReturnPoints { get; set; }
            public List<Region.Message> Messages { get; set; }
            public List<Region.EnvMapEffectBox> EnvMapEffectBoxs { get; set; }
            public List<Region.WindPlacement> WindPlacements { get; set; }
            public List<Region.MapConnection> MapConnections { get; set; }
            public List<Region.SourceWaypoint> SourceWaypoints { get; set; }
            public List<Region.StaticWaypoint> StaticWaypoints { get; set; }
            public List<Region.MapGridLayerConnection> MapGridLayerConnections { get; set; }
            public List<Region.EnemySpawnPoint> EnemySpawnPoints { get; set; }
            public List<Region.BuddySummonPoint> BuddySummonPoints { get; set; }
            public List<Region.RollingObjectOverride> RollingObjectOverrides { get; set; }
            public List<Region.OcclusionBox> OcclusionBoxes { get; set; }
            public List<Region.OcclusionPortal> OcclusionPortals { get; set; }
            public List<Region.SoundRegion> SoundRegions { get; set; }
            public List<Region.MufflingPlane> MufflingPlanes { get; set; }
            public List<Region.PatrolPoint> PatrolPoints { get; set; }
            public List<Region.WorldMapPoint> WorldMapPoints { get; set; }
            public List<Region.SoundState> SoundStates { get; set; }
            public List<Region.WeatherLotRegion> WeatherLotRegions { get; set; }
            public List<Region.AutoDrawGroupSample> AutoDrawGroupSamples { get; set; }
            public List<Region.GroupPlacementPoint> GroupPlacementPoints { get; set; }
            public List<Region.WorldMapDiscoveryPoint> WorldMapDiscoveryPoints { get; set; }
            public List<Region.WorldMapReentryPoint> WorldMapReentryPoints { get; set; }
            public List<Region.HitSetting> HitSettings { get; set; }
            public List<Region.FastTravelOverride> FastTravelOverrides { get; set; }
            public List<Region.WeatherAssetGeneration> WeatherAssetGenerations { get; set; }
            public List<Region.PlayRegion> PlayRegions { get; set; }
            public List<Region.MidRangeEnvMapOutput> MidRangeEnvMapOutputs { get; set; }
            public List<Region.MapVisibilityOverride> MapVisibilityOverrides { get; set; }
            public List<Region.RideJump> RideJumps { get; set; }
            public List<Region.OpenCharacterActivateLimit> OpenCharacterActivateLimits { get; set; }
            public List<Region.SoundDummy> SoundDummys { get; set; }
            public List<Region.FallPreventionOverride> FallPreventionOverrides { get; set; }
            public List<Region.NavmeshCutting> NavmeshCuttings { get; set; }
            public List<Region.MapPlaceNameOverride> MapPlaceNameOverrides { get; set; }
            public List<Region.BigJumpExit> BigJumpExits { get; set; }
            public List<Region.MountOverride> MountOverrides { get; set; }
            public List<Region.SmallBaseAttach> SmallBaseAttachs { get; set; }
            public List<Region.BirdRoute> BirdRoutes { get; set; }
            public List<Region.ClearInfo> ClearInfos { get; set; }
            public List<Region.RespawnOverride> RespawnOverrides { get; set; }
            public List<Region.UserEdgeRemovalInner> UserEdgeRemovalInners { get; set; }
            public List<Region.UserEdgeRemovalOuter> UserEdgeRemovalOuters { get; set; }
            public List<Region.DlcRideJump> DlcRideJumps { get; set; }
            public List<Region.Other> Others { get; set; }

            public PointParam() : base(78, "POINT_PARAM_ST")
            {
                EntryPoints = new List<Region.EntryPoint>();
                EnvMapPoints = new List<Region.EnvMapPoint>();
                RespawnPoints = new List<Region.RespawnPoint>();
                Sounds = new List<Region.Sound>();
                SFX = new List<Region.SFX>();
                WindSFX = new List<Region.WindSFX>();
                ReturnPoints = new List<Region.ReturnPoint>();
                Messages = new List<Region.Message>();
                EnvMapEffectBoxs = new List<Region.EnvMapEffectBox>();
                WindPlacements = new List<Region.WindPlacement>();
                MapConnections = new List<Region.MapConnection>();
                SourceWaypoints = new List<Region.SourceWaypoint>();
                BuddySummonPoints = new List<Region.BuddySummonPoint>();
                RollingObjectOverrides = new List<Region.RollingObjectOverride>();
                OcclusionBoxes = new List<Region.OcclusionBox>();
                OcclusionPortals = new List<Region.OcclusionPortal>();
                SoundRegions = new List<Region.SoundRegion>();
                MufflingPlanes = new List<Region.MufflingPlane>();
                PatrolPoints = new List<Region.PatrolPoint>();
                WorldMapPoints = new List<Region.WorldMapPoint>();
                WeatherLotRegions = new List<Region.WeatherLotRegion>();
                AutoDrawGroupSamples = new List<Region.AutoDrawGroupSample>();
                GroupPlacementPoints = new List<Region.GroupPlacementPoint>();
                WorldMapDiscoveryPoints = new List<Region.WorldMapDiscoveryPoint>();
                WorldMapReentryPoints = new List<Region.WorldMapReentryPoint>();
                HitSettings = new List<Region.HitSetting>();
                FastTravelOverrides = new List<Region.FastTravelOverride>();
                WeatherAssetGenerations = new List<Region.WeatherAssetGeneration>();
                PlayRegions = new List<Region.PlayRegion>();
                MidRangeEnvMapOutputs = new List<Region.MidRangeEnvMapOutput>();
                RideJumps = new List<Region.RideJump>();
                SoundDummys = new List<Region.SoundDummy>();
                FallPreventionOverrides = new List<Region.FallPreventionOverride>();
                NavmeshCuttings = new List<Region.NavmeshCutting>();
                MapPlaceNameOverrides = new List<Region.MapPlaceNameOverride>();
                BigJumpExits = new List<Region.BigJumpExit>();
                MountOverrides = new List<Region.MountOverride>();
                SmallBaseAttachs = new List<Region.SmallBaseAttach>();
                BirdRoutes = new List<Region.BirdRoute>();
                ClearInfos = new List<Region.ClearInfo>();
                RespawnOverrides = new List<Region.RespawnOverride>();
                UserEdgeRemovalInners = new List<Region.UserEdgeRemovalInner>();
                UserEdgeRemovalOuters = new List<Region.UserEdgeRemovalOuter>();
                DlcRideJumps = new List<Region.DlcRideJump>();

                Others = new List<Region.Other>();
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region)
            {
                switch (region)
                {
                    case Region.EntryPoint r: EntryPoints.Add(r); break;
                    case Region.EnvMapPoint r: EnvMapPoints.Add(r); break;
                    case Region.RespawnPoint r: RespawnPoints.Add(r); break;
                    case Region.Sound r: Sounds.Add(r); break;
                    case Region.SFX r: SFX.Add(r); break;
                    case Region.WindSFX r: WindSFX.Add(r); break;
                    case Region.ReturnPoint r: ReturnPoints.Add(r); break;
                    case Region.Message r: Messages.Add(r); break;
                    case Region.EnvMapEffectBox r: EnvMapEffectBoxs.Add(r); break;
                    case Region.WindPlacement r: WindPlacements.Add(r); break;
                    case Region.MapConnection r: MapConnections.Add(r); break;
                    case Region.SourceWaypoint r: SourceWaypoints.Add(r); break;
                    case Region.BuddySummonPoint r: BuddySummonPoints.Add(r); break;
                    case Region.OcclusionBox r: OcclusionBoxes.Add(r); break;
                    case Region.OcclusionPortal r: OcclusionPortals.Add(r); break;
                    case Region.SoundRegion r: SoundRegions.Add(r); break;
                    case Region.MufflingPlane r: MufflingPlanes.Add(r); break;
                    case Region.PatrolPoint r: PatrolPoints.Add(r); break;
                    case Region.WorldMapPoint r: WorldMapPoints.Add(r); break;
                    case Region.WeatherLotRegion r: WeatherLotRegions.Add(r); break;
                    case Region.AutoDrawGroupSample r: AutoDrawGroupSamples.Add(r); break;
                    case Region.GroupPlacementPoint r: GroupPlacementPoints.Add(r); break;
                    case Region.WorldMapDiscoveryPoint r: WorldMapDiscoveryPoints.Add(r); break;
                    case Region.WorldMapReentryPoint r: WorldMapReentryPoints.Add(r); break;
                    case Region.HitSetting r: HitSettings.Add(r); break;
                    case Region.FastTravelOverride r: FastTravelOverrides.Add(r); break;
                    case Region.WeatherAssetGeneration r: WeatherAssetGenerations.Add(r); break;
                    case Region.PlayRegion r: PlayRegions.Add(r); break;
                    case Region.MidRangeEnvMapOutput r: MidRangeEnvMapOutputs.Add(r); break;
                    case Region.RideJump r: RideJumps.Add(r); break;
                    case Region.SoundDummy r: SoundDummys.Add(r); break;
                    case Region.FallPreventionOverride r: FallPreventionOverrides.Add(r); break;
                    case Region.NavmeshCutting r: NavmeshCuttings.Add(r); break;
                    case Region.MapPlaceNameOverride r: MapPlaceNameOverrides.Add(r); break;
                    case Region.BigJumpExit r: BigJumpExits.Add(r); break;
                    case Region.MountOverride r: MountOverrides.Add(r); break;
                    case Region.SmallBaseAttach r: SmallBaseAttachs.Add(r); break;
                    case Region.BirdRoute r: BirdRoutes.Add(r); break;
                    case Region.RollingObjectOverride r: RollingObjectOverrides.Add(r); break;
                    case Region.ClearInfo r: ClearInfos.Add(r); break;
                    case Region.RespawnOverride r: RespawnOverrides.Add(r); break;
                    case Region.UserEdgeRemovalInner r: UserEdgeRemovalInners.Add(r); break;
                    case Region.UserEdgeRemovalOuter r: UserEdgeRemovalOuters.Add(r); break;
                    case Region.DlcRideJump r: DlcRideJumps.Add(r); break;
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
                    EntryPoints, EnvMapPoints, RespawnPoints, Sounds, SFX, WindSFX,
                    ReturnPoints, Messages, EnvMapEffectBoxs, WindPlacements,
                    MapConnections, SourceWaypoints, BuddySummonPoints, RollingObjectOverrides, OcclusionBoxes,
                    OcclusionPortals, SoundRegions, MufflingPlanes, PatrolPoints,
                    WorldMapPoints, WeatherLotRegions, AutoDrawGroupSamples, GroupPlacementPoints,
                    WorldMapDiscoveryPoints, WorldMapReentryPoints, HitSettings,
                    FastTravelOverrides, WeatherAssetGenerations, PlayRegions, MidRangeEnvMapOutputs,
                    RideJumps, SoundDummys, FallPreventionOverrides, NavmeshCuttings, MapPlaceNameOverrides,
                    BigJumpExits, MountOverrides, SmallBaseAttachs, BirdRoutes,
                     ClearInfos, RespawnOverrides, UserEdgeRemovalInners, UserEdgeRemovalOuters,
                     DlcRideJumps,
                    Others);
            }
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br, int version)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 8);
                switch (type)
                {
                    case RegionType.EntryPoint:
                        return EntryPoints.EchoAdd(new Region.EntryPoint(br));

                    case RegionType.EnvMapPoint:
                        return EnvMapPoints.EchoAdd(new Region.EnvMapPoint(br));

                    case RegionType.RespawnPoint:
                        return RespawnPoints.EchoAdd(new Region.RespawnPoint(br));

                    case RegionType.Sound:
                        return Sounds.EchoAdd(new Region.Sound(br));

                    case RegionType.SFX:
                        return SFX.EchoAdd(new Region.SFX(br));

                    case RegionType.WindSFX:
                        return WindSFX.EchoAdd(new Region.WindSFX(br));

                    case RegionType.ReturnPoint:
                        return ReturnPoints.EchoAdd(new Region.ReturnPoint(br));

                    case RegionType.Message:
                        return Messages.EchoAdd(new Region.Message(br));

                    case RegionType.EnvMapEffectBox:
                        return EnvMapEffectBoxs.EchoAdd(new Region.EnvMapEffectBox(br));

                    case RegionType.WindPlacement:
                        return WindPlacements.EchoAdd(new Region.WindPlacement(br));

                    case RegionType.MapConnection:
                        return MapConnections.EchoAdd(new Region.MapConnection(br));
                        
                    case RegionType.SourceWaypoint:
                        return SourceWaypoints.EchoAdd(new Region.SourceWaypoint(br));

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

                    case RegionType.PatrolPoint:
                        return PatrolPoints.EchoAdd(new Region.PatrolPoint(br));

                    case RegionType.WorldMapPoint:
                        return WorldMapPoints.EchoAdd(new Region.WorldMapPoint(br));

                    case RegionType.WeatherLotRegion:
                        return WeatherLotRegions.EchoAdd(new Region.WeatherLotRegion(br));

                    case RegionType.AutoDrawGroupSample:
                        return AutoDrawGroupSamples.EchoAdd(new Region.AutoDrawGroupSample(br));

                    case RegionType.GroupPlacementPoint:
                        return GroupPlacementPoints.EchoAdd(new Region.GroupPlacementPoint(br));

                    case RegionType.WorldMapDiscoveryPoint:
                        return WorldMapDiscoveryPoints.EchoAdd(new Region.WorldMapDiscoveryPoint(br));

                    case RegionType.WorldMapReentryPoint:
                        return WorldMapReentryPoints.EchoAdd(new Region.WorldMapReentryPoint(br));

                    case RegionType.HitSetting:
                        return HitSettings.EchoAdd(new Region.HitSetting(br));

                    case RegionType.FastTravelOverride:
                        return FastTravelOverrides.EchoAdd(new Region.FastTravelOverride(br));

                    case RegionType.WeatherAssetGeneration:
                        return WeatherAssetGenerations.EchoAdd(new Region.WeatherAssetGeneration(br));

                    case RegionType.PlayRegion:
                        return PlayRegions.EchoAdd(new Region.PlayRegion(br));

                    case RegionType.MidRangeEnvMapOutput:
                        return MidRangeEnvMapOutputs.EchoAdd(new Region.MidRangeEnvMapOutput(br));

                    case RegionType.RideJump:
                        return RideJumps.EchoAdd(new Region.RideJump(br));

                    case RegionType.SoundDummy:
                        return SoundDummys.EchoAdd(new Region.SoundDummy(br));

                    case RegionType.FallPreventionOverride:
                        return FallPreventionOverrides.EchoAdd(new Region.FallPreventionOverride(br));

                    case RegionType.NavmeshCutting:
                        return NavmeshCuttings.EchoAdd(new Region.NavmeshCutting(br));

                    case RegionType.MapPlaceNameOverride:
                        return MapPlaceNameOverrides.EchoAdd(new Region.MapPlaceNameOverride(br));

                    case RegionType.BigJumpExit:
                        return BigJumpExits.EchoAdd(new Region.BigJumpExit(br));

                    case RegionType.MountOverride:
                        return MountOverrides.EchoAdd(new Region.MountOverride(br));

                    case RegionType.SmallBaseAttach:
                        return SmallBaseAttachs.EchoAdd(new Region.SmallBaseAttach(br));

                    case RegionType.BirdRoute:
                        return BirdRoutes.EchoAdd(new Region.BirdRoute(br));

                    case RegionType.ClearInfo:
                        return ClearInfos.EchoAdd(new Region.ClearInfo(br));

                    case RegionType.RespawnOverride:
                        return RespawnOverrides.EchoAdd(new Region.RespawnOverride(br));

                    case RegionType.UserEdgeRemovalInner:
                        return UserEdgeRemovalInners.EchoAdd(new Region.UserEdgeRemovalInner(br));

                    case RegionType.UserEdgeRemovalOuter:
                        return UserEdgeRemovalOuters.EchoAdd(new Region.UserEdgeRemovalOuter(br));

                    case RegionType.DlcRideJump:
                        return DlcRideJumps.EchoAdd(new Region.DlcRideJump(br));

                    case RegionType.Other:
                        return Others.EchoAdd(new Region.Other(br));

                    default:
                        throw new NotImplementedException($"Unimplemented region type: {type}");
                }
            }
        }

        public abstract class Region : Entry, IMsbRegion
        {
            private protected abstract RegionType Type { get; }
            private protected abstract bool HasTypeData { get; }

            /// <summary>
            /// True for types that gained type data from the ER findings but have no retail NR instance; a file written
            /// before that (type data offset 0) is still read, keeping the defaults.
            /// </summary>
            private protected virtual bool TypeDataOptional => false;

            private protected Region(string name)
            {
                Name = name;
                Shape = new MSB.Shape.Point();
                MapStudioLayer = 0xFFFFFFFF;
                UnkA_Region = new List<short>();
                UnkB_Region = new List<short>();
                Common_EntityID = 0;
            }

            public Region DeepCopy()
            {
                var region = (Region)MemberwiseClone();
                region.Shape = Shape.DeepCopy();
                region.UnkA_Region = new List<short>(UnkA_Region);
                region.UnkB_Region = new List<short>(UnkB_Region);
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
                Unk40_Region = br.ReadInt32();
                MapStudioLayer = br.ReadUInt32();
                long shapeDataOffset = br.ReadInt64();
                long baseDataOffset3 = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();
                long struct98Offset = br.ReadInt64();

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
                    if (HasTypeData ^ typeDataOffset != 0 && !(TypeDataOptional && typeDataOffset == 0))
                        throw new InvalidDataException($"Unexpected {nameof(typeDataOffset)} 0x{typeDataOffset:X} in type {GetType()}.");
                    if (struct98Offset == 0)
                        throw new InvalidDataException($"{nameof(struct98Offset)} must not be 0 in type {GetType()}.");
                }


                br.Position = start + nameOffset;
                Name = br.ReadUTF16();

                br.Position = start + baseDataOffset1;
                short countA = br.ReadInt16();
                UnkA_Region = new List<short>(br.ReadInt16s(countA));

                br.Position = start + baseDataOffset2;
                short countB = br.ReadInt16();
                UnkB_Region = new List<short>(br.ReadInt16s(countB));

                if (Shape.HasShapeData)
                {
                    br.Position = start + shapeDataOffset;
                    Shape.ReadShapeData(br);
                }

                br.Position = start + baseDataOffset3;

                // Common
                Common_PartIndex = br.ReadInt32();
                Common_EntityID = br.ReadUInt32();
                Unk08_Common = br.ReadSByte();
                Unk09_Common = br.ReadByte();
                Unk0A_Common = br.ReadInt16();
                Unk0C_Common = br.ReadInt32();
                Common_VariationID = br.ReadInt32();
                Unk14_Common = br.ReadInt32();
                Unk18_Common = br.ReadInt32();
                Unk1C_Common = br.ReadInt32();

                if (HasTypeData && typeDataOffset != 0)
                {
                    br.Position = start + typeDataOffset;
                    ReadTypeData(br);
                }

                // Unk4
                br.Position = start + struct98Offset;
                Stuct98_MapID = br.ReadMapIDBytes(4);
                Unk04_Struct98 = br.ReadInt32();
                br.AssertInt32(0);
                Unk0C_Struct98 = br.ReadInt32();
                Unk10_Struct98 = br.ReadInt32();
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
                bw.WriteInt32(Unk40_Region);
                bw.WriteUInt32(MapStudioLayer);
                bw.ReserveInt64("ShapeDataOffset");
                bw.ReserveInt64("EntityDataOffset");
                bw.ReserveInt64("TypeDataOffset");
                bw.ReserveInt64("Struct98Offset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.Pad(4);

                bw.FillInt64("BaseDataOffset1", bw.Position - start);
                bw.WriteInt16((short)UnkA_Region.Count);
                bw.WriteInt16s(UnkA_Region);
                bw.Pad(4);

                bw.FillInt64("BaseDataOffset2", bw.Position - start);
                bw.WriteInt16((short)UnkB_Region.Count);
                bw.WriteInt16s(UnkB_Region);
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


                // Common
                bw.FillInt64("EntityDataOffset", bw.Position - start);
                bw.WriteInt32(Common_PartIndex);
                bw.WriteUInt32(Common_EntityID);
                bw.WriteSByte(Unk08_Common);
                bw.WriteByte(Unk09_Common);
                bw.WriteInt16(Unk0A_Common);
                bw.WriteInt32(Unk0C_Common);
                bw.WriteInt32(Common_VariationID);
                bw.WriteInt32(Unk14_Common);
                bw.WriteInt32(Unk18_Common);
                bw.WriteInt32(Unk1C_Common);

                // Type 27 (RollingObjectOverride) is laid out like the lower types in ER: type data right after the
                // entity data, padding after it.
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

                // Struct 98
                bw.FillInt64("Struct98Offset", bw.Position - start);
                bw.WriteMapIDBytes(Stuct98_MapID);
                bw.WriteInt32(Unk04_Struct98);
                bw.WriteInt32(Unk08_Struct98);
                bw.WriteInt32(Unk0C_Struct98);
                bw.WriteInt32(Unk10_Struct98);
                bw.WriteInt32(Unk14_Struct98);
                bw.WriteInt32(Unk18_Struct98);
                bw.WriteInt32(Unk1C_Struct98);
                bw.Pad(8);
            }

            // Region
            public MSB.Shape Shape { get; set; }
            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }
            public int RegionID { get; set; }
            public int Unk40_Region { get; set; }
            public uint MapStudioLayer { get; set; }
            public List<short> UnkA_Region { get; set; } = new List<short>();
            public List<short> UnkB_Region { get; set; } = new List<short>();

            // Common
            private int Common_PartIndex { get; set; } = -1;
            public uint Common_EntityID { get; set; } = 0;
            public sbyte Unk08_Common { get; set; } = -1;
            public byte Unk09_Common { get; set; } = 0;
            public short Unk0A_Common { get; set; } = 0;
            public int Unk0C_Common { get; set; } = 0;
            public int Common_VariationID { get; set; } = -1;
            public int Unk14_Common { get; set; } = 0;
            public int Unk18_Common { get; set; } = 0;
            public int Unk1C_Common { get; set; } = 0;

            // Struct 98

            public sbyte[] Stuct98_MapID { get; set; } = new sbyte[4];
            public int Unk04_Struct98 { get; set; } = 0;
            public int Unk08_Struct98 { get; set; } = 0;
            public int Unk0C_Struct98 { get; set; } = -1;
            public int Unk10_Struct98 { get; set; } = -1;
            public int Unk14_Struct98 { get; set; } = 0;
            public int Unk18_Struct98 { get; set; } = 0;
            public int Unk1C_Struct98 { get; set; } = 0;


            // Names
            [MSBReference(ReferenceType = typeof(Part))]
            public string Common_PartName { get; set; }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal virtual void GetNames(Entries entries)
            {
                Common_PartName = MSB.FindName(entries.Parts, Common_PartIndex);
                if (Shape is MSB.Shape.Composite composite)
                    composite.GetNames(entries.Regions);
            }

            internal virtual void GetIndices(Entries entries)
            {
                Common_PartIndex = MSB.FindIndex(this, entries.Parts, Common_PartName);
                if (Shape is MSB.Shape.Composite composite)
                    composite.GetIndices(entries.Regions);
            }

            public override string ToString()
            {
                return $"{Type} {Shape.Type} {Name}";
            }

            public class EntryPoint : Region
            {
                private protected override RegionType Type => RegionType.EntryPoint;
                private protected override bool HasTypeData => true;

                public EntryPoint() : base($"{nameof(Region)}: {nameof(EntryPoint)}") { }

                internal EntryPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Priority = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Priority);
                }

                // Layout
                public int Priority { get; set; }
            }

            public class EnvMapPoint : Region
            {
                private protected override RegionType Type => RegionType.EnvMapPoint;
                private protected override bool HasTypeData => true;

                public EnvMapPoint() : base($"{nameof(Region)}: {nameof(EnvMapPoint)}")
                {
                }

                internal EnvMapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void DeepCopyTo(Region region)
                {
                    var point = (EnvMapPoint)region;
                    point.UnkMapID = (byte[])UnkMapID.Clone();
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadByte();
                    Unk0F = br.ReadByte();
                    Unk10 = br.ReadSingle();
                    Unk14 = br.ReadSingle();
                    UnkMapID = br.ReadBytes(4);
                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadByte();
                    Unk2D = br.ReadByte();
                    Unk2E = br.ReadInt16();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteByte(Unk0E);
                    bw.WriteByte(Unk0F);
                    bw.WriteSingle(Unk10);
                    bw.WriteSingle(Unk14);
                    bw.WriteBytes(UnkMapID);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteByte(Unk2C);
                    bw.WriteByte(Unk2D);
                    bw.WriteInt16(Unk2E);
                }

                // Layout
                public float Unk00 { get; set; } = 1;
                public int Unk04 { get; set; } = 4;
                public int Unk08 { get; set; } = -1;
                public byte Unk0C { get; set; } = 0;
                public byte Unk0D { get; set; } // Boolean
                public byte Unk0E { get; set; } // Boolean
                public byte Unk0F { get; set; } // Boolean
                public float Unk10 { get; set; } = 1.0f;
                public float Unk14 { get; set; } = 1.0f;

                /// <summary>
                /// Map the point belongs to (ER: bytes 0-2 are compared with a map ID's AA/BB/CC; byte 0 = 0: none; byte 3
                /// is not read and is 2 almost everywhere). Retail NR: [0,0,0,2], or open-field tiles such as [60,8,8,2].
                /// Formerly Unk18 (an int).
                /// </summary>
                public byte[] UnkMapID { get; set; } = new byte[] { 0, 0, 0, 2 };
                public int Unk1C { get; set; } = 0;
                public int Unk20 { get; set; }
                public int Unk24 { get; set; }
                public int Unk28 { get; set; }
                public byte Unk2C { get; set; } = 0;
                public byte Unk2D { get; set; }
                public short Unk2E { get; set; } = 0;

            }

            public class RespawnPoint : Region
            {
                private protected override RegionType Type => RegionType.RespawnPoint;
                private protected override bool HasTypeData => true;

                public RespawnPoint() : base($"{nameof(Region)}: {nameof(RespawnPoint)}") { }

                internal RespawnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;
            }

            public class Sound : Region
            {
                private protected override RegionType Type => RegionType.Sound;
                private protected override bool HasTypeData => true;

                public Sound() : base($"{nameof(Region)}: {nameof(Sound)}")
                {
                    ChildRegionNames = new string[16];
                }

                internal Sound(BinaryReaderEx br) : base(br) { }

                private protected override void DeepCopyTo(Region region)
                {
                    var sound = (Sound)region;
                    sound.ChildRegionNames = (string[])ChildRegionNames.Clone();
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

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    SoundType = br.ReadInt32();
                    SoundID = br.ReadInt32();
                    ChildRegionIndices = br.ReadInt32s(16);
                    Unk48 = br.ReadByte();
                    Unk49 = br.ReadByte();
                    Unk4A = br.ReadInt16();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32s(ChildRegionIndices);
                    bw.WriteByte(Unk48);
                    bw.WriteByte(Unk49);
                    bw.WriteInt16(Unk4A);
                }

                // Layout
                public int SoundType { get; set; }
                public int SoundID { get; set; }
                private int[] ChildRegionIndices { get; set; }
                public byte Unk48 { get; set; } = 0;
                public byte Unk49 { get; set; } // Boolean
                public short Unk4A { get; set; } = 0;

                // Names

                [MSBReference(ReferenceType = typeof(Region))]
                public string[] ChildRegionNames { get; set; }

            }

            public class SFX : Region
            {
                private protected override RegionType Type => RegionType.SFX;
                private protected override bool HasTypeData => true;

                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    StartDisabled = br.ReadByte();
                    Unk05 = br.ReadByte();
                    Unk06 = br.ReadInt16();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteByte(StartDisabled);
                    bw.WriteByte(Unk05);
                    bw.WriteInt16(Unk06);
                }

                // Layout
                public int EffectID { get; set; }
                public byte StartDisabled { get; set; } = 0; // Boolean
                public byte Unk05 { get; set; } = 0; // Boolean
                public short Unk06 { get; set; } = 0; 
            }

            public class WindSFX : Region
            {
                private protected override RegionType Type => RegionType.WindSFX;
                private protected override bool HasTypeData => true;

                public WindSFX() : base($"{nameof(Region)}: {nameof(WindSFX)}") { }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

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

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    WindAreaIndex = br.ReadInt32();
                    Unk08 = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(WindAreaIndex);
                    bw.WriteSingle(Unk08);
                }

                // Layout
                public int EffectID { get; set; } = 808006;
                private int WindAreaIndex { get; set; } = -1;
                public float Unk08 { get; set; } = -1.0f;

                // Names
                [MSBReference(ReferenceType = typeof(Region))]
                public string WindAreaName { get; set; }

            }

            public class ReturnPoint : Region
            {
                private protected override RegionType Type => RegionType.ReturnPoint;
                private protected override bool HasTypeData => true;

                public ReturnPoint() : base($"{nameof(Region)}: {nameof(ReturnPoint)}") { }

                internal ReturnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }

                // Layout
                public int Unk00 { get; set; } = -1;
                public int Unk04 { get; set; } = 0;
                public int Unk08 { get; set; } = 0;
                public int Unk0C { get; set; } = 0;
            }

            public class Message : Region
            {
                private protected override RegionType Type => RegionType.Message;
                private protected override bool HasTypeData => true;

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

                // Layout
                public short MessageID { get; set; }
                public short UnkT02 { get; set; }
                public bool Hidden { get; set; }
                public int ItemLotParamID { get; set; }
                public int MessageSfxID { get; set; }
                public uint EnableEventFlagID { get; set; }
                public int CharacterModelName { get; set; }
                public int NPCParamID { get; set; }
                public int AnimationID { get; set; }

                [MSBParamReference(ParamName = "CharaInitParam")]
                public int CharaInitParamID { get; set; }

            }

            /// <summary>
            /// An environment map effect box (vanilla: Env_BoxNNN): applies the env map of the Env_PointNNN point with its
            /// env map number, and links to irradiance volume GIIV NNN. Same layout as ER's EnvironmentMapEffectBox; NR's
            /// env-map setup (140C72520) reads types 2 and 17 together like ER's.
            /// </summary>
            public class EnvMapEffectBox : Region
            {
                private protected override RegionType Type => RegionType.EnvMapEffectBox;
                private protected override bool HasTypeData => true;

                public EnvMapEffectBox() : base($"{nameof(Region)}: {nameof(EnvMapEffectBox)}") { }

                internal EnvMapEffectBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    DisplayDistance = br.ReadSingle();
                    FadeDistance = br.ReadSingle();
                    ApplyParallaxCorrection = br.ReadByte();
                    LegacyPriority = br.ReadByte();
                    EnvMapNumber = br.ReadInt16();
                    BoundsExpandNegative = br.ReadVector3();
                    BoundsExpandPositive = br.ReadVector3();
                    AmbientDiffuseIntensity = br.ReadSingle();
                    AmbientSpecularIntensity = br.ReadSingle();
                    UnkT2C = br.ReadByte();
                    UnkT2D = br.ReadByte();
                    IsModifyLight = br.ReadByte();
                    UnkT2F = br.ReadByte();
                    AdditionalEnvMapNumber = br.ReadInt16();
                    UnkT32 = br.ReadByte();
                    UnkT33 = br.ReadByte();
                    Priority = br.ReadInt16();
                    UnkT36 = br.ReadByte();
                    UnkT37 = br.ReadByte();
                    UnkT38 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(DisplayDistance);
                    bw.WriteSingle(FadeDistance);
                    bw.WriteByte(ApplyParallaxCorrection);
                    bw.WriteByte(LegacyPriority);
                    bw.WriteInt16(EnvMapNumber);
                    bw.WriteVector3(BoundsExpandNegative);
                    bw.WriteVector3(BoundsExpandPositive);
                    bw.WriteSingle(AmbientDiffuseIntensity);
                    bw.WriteSingle(AmbientSpecularIntensity);
                    bw.WriteByte(UnkT2C);
                    bw.WriteByte(UnkT2D);
                    bw.WriteByte(IsModifyLight);
                    bw.WriteByte(UnkT2F);
                    bw.WriteInt16(AdditionalEnvMapNumber);
                    bw.WriteByte(UnkT32);
                    bw.WriteByte(UnkT33);
                    bw.WriteInt16(Priority);
                    bw.WriteByte(UnkT36);
                    bw.WriteByte(UnkT37);
                    bw.WriteInt32(UnkT38);
                }

                /// <summary>
                /// Distance from camera required before enabling envmap. 0 = always enabled.
                /// </summary>
                public float DisplayDistance { get; set; } = 0;

                /// <summary>
                /// Distance it takes for an envmap to fully transition into view.
                /// </summary>
                public float FadeDistance { get; set; } = 0;

                /// <summary>
                /// Applies parallax correction to the env map (non-zero = on).
                /// </summary>
                public byte ApplyParallaxCorrection { get; set; } = 0;

                /// <summary>
                /// The box priority for MSB versions before 0x38 (ER). Later versions use Priority; 10 in all retail NR boxes.
                /// </summary>
                public byte LegacyPriority { get; set; } = 10;

                /// <summary>
                /// The env map number this box uses (Env_Point and GIIV volume). -1: the three digits at the end of the
                /// box name (Env_Box015 = 15).
                /// </summary>
                public short EnvMapNumber { get; set; } = -1;

                /// <summary>
                /// Extends the influence bounds of the box beyond its shape on the negative X/Y/Z sides (min = -size/2 - this). 0 in retail.
                /// </summary>
                public Vector3 BoundsExpandNegative { get; set; }

                /// <summary>
                /// Extends the influence bounds of the box beyond its shape on the positive X/Y/Z sides (max = size/2 + this). 0 in retail.
                /// </summary>
                public Vector3 BoundsExpandPositive { get; set; }

                /// <summary>
                /// Strength of ambient diffuse light.
                /// </summary>
                public float AmbientDiffuseIntensity { get; set; } = 1f;

                /// <summary>
                /// Strength of ambient specular light.
                /// </summary>
                public float AmbientSpecularIntensity { get; set; } = 1f;

                /// <summary>
                /// Selects two irradiance volume flags (ER): 0 = both on, 1 = only the second, 2 = only the first. 0 in retail NR.
                /// </summary>
                public byte UnkT2C { get; set; } = 0;

                /// <summary>
                /// Unknown; copied with the UnkT2C flags but never read in ER. 0 in retail.
                /// </summary>
                public byte UnkT2D { get; set; } = 0;

                /// <summary>
                /// Affects lighting with other fields when set (boolean). 1 in all retail NR boxes.
                /// </summary>
                public byte IsModifyLight { get; set; } = 1;

                /// <summary>
                /// Unknown (boolean); no effect in ER.
                /// </summary>
                public byte UnkT2F { get; set; } = 1;

                /// <summary>
                /// A second env map / irradiance volume number for this box; when 0 or more, the volume setup runs a
                /// second time with it. -1: none.
                /// </summary>
                public short AdditionalEnvMapNumber { get; set; } = -1;

                /// <summary>
                /// Unknown (boolean); no effect in ER.
                /// </summary>
                public byte UnkT32 { get; set; } = 0;

                /// <summary>
                /// Unknown (boolean); no effect in ER.
                /// </summary>
                public byte UnkT33 { get; set; } = 1;

                /// <summary>
                /// The box priority (MSB version 0x38 and later). Retail NR: 10, 30, 20, 31, 32, 35 and others.
                /// </summary>
                public short Priority { get; set; } = 10;

                /// <summary>
                /// Unknown (boolean); no effect in ER.
                /// </summary>
                public byte UnkT36 { get; set; } = 1;

                /// <summary>
                /// Unknown; 0 in retail.
                /// </summary>
                public byte UnkT37 { get; set; } = 0;

                /// <summary>
                /// Unknown; 0 in retail.
                /// </summary>
                public int UnkT38 { get; set; } = 0;
            }

            public class WindPlacement : Region
            {
                private protected override RegionType Type => RegionType.WindPlacement;
                private protected override bool HasTypeData => false;

                public WindPlacement() : base($"{nameof(Region)}: {nameof(WindPlacement)}") { }

                internal WindPlacement(BinaryReaderEx br) : base(br) { }
                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class MapConnection : Region
            {
                private protected override RegionType Type => RegionType.MapConnection;
                private protected override bool HasTypeData => true;

                public MapConnection() : base($"{nameof(Region)}: {nameof(MapConnection)}") { }

                internal MapConnection(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ConnectionMapID = br.ReadSBytes(4);
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSBytes(ConnectionMapID);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                }

                // Layout
                public sbyte[] ConnectionMapID { get; set; } = new sbyte[4];
                public int Unk04 { get; set; } = 0;
                public int Unk08 { get; set; } = 0;
                public int Unk0C { get; set; } = 0;
            }

            public class SourceWaypoint : Region
            {
                private protected override RegionType Type => RegionType.SourceWaypoint;
                private protected override bool HasTypeData => false;

                public SourceWaypoint() : base($"{nameof(Region)}: {nameof(SourceWaypoint)}") { }

                internal SourceWaypoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class StaticWaypoint : Region
            {
                private protected override RegionType Type => RegionType.StaticWaypoint;
                private protected override bool HasTypeData => false;

                public StaticWaypoint() : base($"{nameof(Region)}: {nameof(StaticWaypoint)}") { }

                internal StaticWaypoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class MapGridLayerConnection : Region
            {
                private protected override RegionType Type => RegionType.MapGridLayerConnection;
                private protected override bool HasTypeData => false;

                public MapGridLayerConnection() : base($"{nameof(Region)}: {nameof(MapGridLayerConnection)}") { }

                internal MapGridLayerConnection(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class EnemySpawnPoint : Region
            {
                private protected override RegionType Type => RegionType.EnemySpawnPoint;
                private protected override bool HasTypeData => false;

                public EnemySpawnPoint() : base($"{nameof(Region)}: {nameof(EnemySpawnPoint)}") { }

                internal EnemySpawnPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class BuddySummonPoint : Region
            {
                private protected override RegionType Type => RegionType.BuddySummonPoint;
                private protected override bool HasTypeData => false;

                public BuddySummonPoint() : base($"{nameof(Region)}: {nameof(BuddySummonPoint)}") { }

                internal BuddySummonPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            /// <summary>
            /// The exe's CSRollingObjOverrideRegionMan region: inside it, rolling objects spawn from RollingObjLotParamID
            /// instead of the map tile's own lot. No retail NR map uses it; nightreign.exe reads +0 as an int (-1 when
            /// there is no type data, 1406DCB10) like ER, so the ER layout is used. Files without type data for this
            /// type (written by older versions) are still read.
            /// </summary>
            public class RollingObjectOverride : Region
            {
                private protected override RegionType Type => RegionType.RollingObjectOverride;
                private protected override bool HasTypeData => true;
                private protected override bool TypeDataOptional => true;

                public RollingObjectOverride() : base($"{nameof(Region)}: {nameof(RollingObjectOverride)}") { }

                internal RollingObjectOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    RollingObjLotParamID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(RollingObjLotParamID);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                }

                /// <summary>
                /// The RollingObjLotParam row used for spawns inside the region. -1 means nothing spawns.
                /// </summary>
                [MSBParamReference(ParamName = "RollingObjLotParam")]
                public int RollingObjLotParamID { get; set; } = -1;

                /// <summary>
                /// Unknown; 0 in ER and never read.
                /// </summary>
                public int UnkT04 { get; set; } = 0;

                /// <summary>
                /// Unknown; 0 in ER and never read.
                /// </summary>
                public int UnkT08 { get; set; } = 0;
            }

            /// <summary>
            /// The exe's CSMsbPointOcclusionBox (formerly MufflingBox): a room of the sound occlusion graph. Its
            /// OcclusionPortals are the openings, and routes (MufflingPortalLink / MufflingBoxLink) join the portals.
            /// </summary>
            public class OcclusionBox : Region
            {
                private protected override RegionType Type => RegionType.OcclusionBox;
                private protected override bool HasTypeData => true;

                public OcclusionBox() : base($"{nameof(Region)}: {nameof(OcclusionBox)}") { }

                internal OcclusionBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    BoxNumber = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    Offset18 = br.ReadInt64();

                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadSingle();
                    UnkT28 = br.ReadInt32();
                    UnkT2C = br.ReadInt32();
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadSingle();
                    UnkT38 = br.ReadInt32();
                    UnkT3C = br.ReadSingle();
                    UnkT40 = br.ReadSingle();
                    UnkT44 = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(BoxNumber);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt64(Offset18);

                    bw.WriteInt32(UnkT20);
                    bw.WriteSingle(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(UnkT2C);
                    bw.WriteInt32(UnkT30);
                    bw.WriteSingle(UnkT34);
                    bw.WriteInt32(UnkT38);
                    bw.WriteSingle(UnkT3C);
                    bw.WriteSingle(UnkT40);
                    bw.WriteSingle(UnkT44);
                }

                /// <summary>
                /// The room number. Several boxes can share a number and form one room; each OcclusionPortal names the
                /// room it belongs to with the same number.
                /// </summary>
                public int BoxNumber { get; set; } = 0;

                public int UnkT04 { get; set; } = 0;
                public int UnkT08 { get; set; } = 0;
                public int UnkT0C { get; set; } = 0;
                public int UnkT10 { get; set; } = 0;
                public int UnkT14 { get; set; } = 0;
                public int UnkT20 { get; set; } = 0;

                /// <summary>
                /// Unknown. 100 in every retail box; not seen read in ER.
                /// </summary>
                public float UnkT24 { get; set; } = 100f;
                public int UnkT28 { get; set; } = 0;
                public int UnkT2C { get; set; } = 0;
                public int UnkT30 { get; set; } = 0;

                /// <summary>
                /// Unknown. 100 in every retail box; not seen read in ER.
                /// </summary>
                public float UnkT34 { get; set; } = 100f;
                public int UnkT38 { get; set; } = 0;

                /// <summary>
                /// Unknown. -1 in every retail box; not seen read in ER.
                /// </summary>
                public float UnkT3C { get; set; } = -1f;
                public float UnkT40 { get; set; } = -1f;
                public float UnkT44 { get; set; } = -1f;

                // Offsets
                private long Offset18 { get; set; } = 32;
            }

            /// <summary>
            /// The exe's CSMsbPointOcclusionPortal (formerly MufflingPortal): an opening of an OcclusionBox. Routes
            /// reference portals by their RegionID.
            /// </summary>
            public class OcclusionPortal : Region
            {
                private protected override RegionType Type => RegionType.OcclusionPortal;
                private protected override bool HasTypeData => true;

                public OcclusionPortal() : base($"{nameof(Region)}: {nameof(OcclusionPortal)}") { }

                internal OcclusionPortal(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    BoxNumber = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    Offset18 = br.ReadInt64();
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    UnkT2C = br.ReadInt32();
                    UnkT30 = br.ReadInt32();
                    UnkT34 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(BoxNumber);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt64(Offset18);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(UnkT2C);
                    bw.WriteInt32(UnkT30);
                    bw.WriteInt32(UnkT34);
                }

                /// <summary>
                /// The BoxNumber of the OcclusionBox this portal belongs to (283 of 283 retail NR portals match a box in their map).
                /// </summary>
                public int BoxNumber { get; set; } = 0;

                public int UnkT04 { get; set; } = 0;
                public int UnkT08 { get; set; } = 0;
                public int UnkT0C { get; set; } = 0;
                public int UnkT10 { get; set; } = 0;
                public int UnkT14 { get; set; } = 0;
                public int UnkT20 { get; set; } = 0;
                public int UnkT24 { get; set; } = 0;
                public int UnkT28 { get; set; } = 0;
                public int UnkT2C { get; set; } = 0;
                public int UnkT30 { get; set; } = 0;
                public int UnkT34 { get; set; } = -1;

                // Offsets
                private long Offset18 { get; set; } = 32;
            }

            /// <summary>
            /// The exe's CSMsbPointSoundRegion (formerly SoundOverride): overrides map sound settings while the listener
            /// is inside it. Same layout as ER's SoundRegion; in ER a field only counts when it is not -1 and the shape
            /// is a Box.
            /// </summary>
            public class SoundRegion : Region
            {
                private protected override RegionType Type => RegionType.SoundRegion;
                private protected override bool HasTypeData => true;

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
                    UnkT0D = br.ReadByte();
                    UnkT0E = br.ReadInt16();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
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
                    bw.WriteByte(UnkT0D);
                    bw.WriteInt16(UnkT0E);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);
                }

                /// <summary>
                /// Reverb type used inside the region, replacing MapDefaultInfoParam ReverbDefaultType. -1: no override.
                /// </summary>
                public sbyte ReverbType { get; set; } = -1;

                /// <summary>
                /// Water body type for the region's water sound: 0 none, 1 sea, 2 lake, 3 river. Regions with the same
                /// WaterType and WaterID form one water sound.
                /// </summary>
                public byte WaterType { get; set; } = 0;

                /// <summary>
                /// ID of the water sound group within its WaterType. Only used when WaterType is above 0.
                /// </summary>
                public byte WaterID { get; set; } = 0;

                /// <summary>
                /// Outdoor/indoor override, like Collision OutdoorIndoorType: 0 outdoor, 1 half indoor, 2 indoor. -1: no override.
                /// </summary>
                public sbyte OutdoorIndoorType { get; set; } = -1;

                /// <summary>
                /// Wwise environment place type used inside the region. -1: no override.
                /// </summary>
                [MSBParamReference(ParamName = "WwiseValueToStrParam_EnvPlaceType")]
                public int WwiseEnvPlaceTypeID { get; set; } = -1;

                /// <summary>
                /// Replaces MapDefaultInfoParam EnvPlaceInfo inside the region. -1: no override.
                /// </summary>
                [MSBParamReference(ParamName = "WwiseValueToStrParam_EnvPlaceType")]
                public short EnvPlaceInfo { get; set; } = -1;

                /// <summary>
                /// Common place type used inside the region (-1 = none).
                /// </summary>
                public short CommonPlaceType { get; set; } = -1;

                /// <summary>
                /// Replaces MapDefaultInfoParam isDisableAutoCliffWind inside the region (ER: only read in m61 maps).
                /// -1: no override. -1 in all retail NR regions.
                /// </summary>
                public sbyte DisableAutoCliffWind { get; set; } = -1;

                /// <summary>
                /// Unknown; 0 in retail.
                /// </summary>
                public byte UnkT0D { get; set; } = 0;
                public short UnkT0E { get; set; } = 0;
                public int UnkT10 { get; set; } = 0;
                public int UnkT14 { get; set; } = 0;
                public int UnkT18 { get; set; } = 0;
                public int UnkT1C { get; set; } = 0;
            }

            public class MufflingPlane : Region
            {
                private protected override RegionType Type => RegionType.MufflingPlane;
                private protected override bool HasTypeData => false;

                public MufflingPlane() : base($"{nameof(Region)}: {nameof(MufflingPlane)}") { }

                internal MufflingPlane(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            /// <summary>
            /// A point along an NPC patrol path.
            /// </summary>
            public class PatrolPoint : Region
            {
                private protected override RegionType Type => RegionType.PatrolPoint;
                private protected override bool HasTypeData => true;

                public PatrolPoint() : base($"{nameof(Region)}: {nameof(PatrolPoint)}") { }

                internal PatrolPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ActionID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ActionID);
                }

                /// <summary>
                /// Action ID that AI scripts read for this point (ER: Lua GetMovePointActionId). -1: none.
                /// Retail NR: mostly 900002-900006.
                /// </summary>
                public int ActionID { get; set; } = -1;
            }

            public class SoundState : Region
            {
                private protected override RegionType Type => RegionType.SoundState;
                private protected override bool HasTypeData => false;

                public SoundState() : base($"{nameof(Region)}: {nameof(SoundState)}") { }

                internal SoundState(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            /// <summary>
            /// The exe's CSMsbPointWorldMapPoint (formerly MapPoint): a point shown on the world map, with a discovery
            /// area and a re-entry area. nightreign.exe loads types 38, 39 and 33 together (140BAC6E0), like ER.
            /// </summary>
            public class WorldMapPoint : Region
            {
                private protected override RegionType Type => RegionType.WorldMapPoint;
                private protected override bool HasTypeData => true;

                public WorldMapPoint() : base($"{nameof(Region)}: {nameof(WorldMapPoint)}") { }

                internal WorldMapPoint(BinaryReaderEx br) : base(br) { }

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

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WorldMapPointParamID = br.ReadInt32();
                    DiscoveryRegionIndex = br.ReadInt32();
                    DiscoveryExpandRangeXZ = br.ReadSingle();
                    DiscoveryExpandRangeY = br.ReadSingle();
                    ReentryRegionIndex = br.ReadInt32();
                    ReentryExpandRangeXZ = br.ReadSingle();
                    ReentryExpandRangeY = br.ReadSingle();
                    UnkT1C = br.ReadInt32();
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
                    bw.WriteInt32(UnkT1C);
                }

                /// <summary>
                /// Determines which WorldMapPointParam to use.
                /// </summary>
                public int WorldMapPointParamID { get; set; } = -1;

                /// <summary>
                /// A WorldMapDiscoveryPoint region that replaces this point's discovery area. Empty: the discovery area
                /// is this region's shape, expanded by DiscoveryExpandRangeXZ / DiscoveryExpandRangeY.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string DiscoveryRegionName { get; set; }
                private int DiscoveryRegionIndex { get; set; } = -1;

                /// <summary>
                /// Horizontal expansion of the shape for the discovery area. Negative: GameSystemCommonParam default.
                /// </summary>
                public float DiscoveryExpandRangeXZ { get; set; } = -1f;

                /// <summary>
                /// Vertical expansion of the shape for the discovery area. Negative: GameSystemCommonParam default.
                /// </summary>
                public float DiscoveryExpandRangeY { get; set; } = -1f;

                /// <summary>
                /// A WorldMapReentryPoint region that replaces this point's re-entry area. Empty: the re-entry area is
                /// this region's shape, expanded by ReentryExpandRangeXZ / ReentryExpandRangeY.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string ReentryRegionName { get; set; }
                private int ReentryRegionIndex { get; set; } = -1;

                /// <summary>
                /// Horizontal expansion of the shape for the re-entry area. Negative: GameSystemCommonParam default.
                /// </summary>
                public float ReentryExpandRangeXZ { get; set; } = -1f;

                /// <summary>
                /// Vertical expansion of the shape for the re-entry area. Negative: GameSystemCommonParam default.
                /// </summary>
                public float ReentryExpandRangeY { get; set; } = -1f;

                /// <summary>
                /// Unknown; 0 in retail.
                /// </summary>
                public int UnkT1C { get; set; } = 0;
            }

            /// <summary>
            /// The exe's CSWeatherLotRegionMan region (formerly MapInfoOverride). While the player is inside, the region's
            /// WeatherLotParamID overrides the map's (nightreign.exe 140654C40). NR reads only +0 and +4; ER's
            /// MimicryEstablishmentTexParam overrides at +8..+A are not read by NR.
            /// </summary>
            public class WeatherLotRegion : Region
            {
                private protected override RegionType Type => RegionType.WeatherLotRegion;
                private protected override bool HasTypeData => true;

                public WeatherLotRegion() : base($"{nameof(Region)}: {nameof(WeatherLotRegion)}") { }

                internal WeatherLotRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WeatherLotParamID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(WeatherLotParamID);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);
                }

                /// <summary>
                /// Determines which WeatherLotParam ID to use. -1: no override. Retail NR: 4000 (one region).
                /// </summary>
                [MSBParamReference(ParamName = "WeatherLotParam")]
                public int WeatherLotParamID { get; set; } = -1;

                /// <summary>
                /// Unknown, -1 = none. Read by 140654BD0 (-1 when there is no type data); -1 in retail.
                /// </summary>
                public int UnkT04 { get; set; } = -1;

                /// <summary>
                /// Unknown; 0 in retail and not read by NR (ER: three sbyte mimicry overrides and a byte).
                /// </summary>
                public int UnkT08 { get; set; } = 0;

                public int UnkT0C { get; set; } = 0;
                public int UnkT10 { get; set; } = 0;
                public int UnkT14 { get; set; } = 0;
                public int UnkT18 { get; set; } = 0;
                public int UnkT1C { get; set; } = 0;
            }

            public class AutoDrawGroupSample : Region
            {
                private protected override RegionType Type => RegionType.AutoDrawGroupSample;
                private protected override bool HasTypeData => false;

                public AutoDrawGroupSample() : base($"{nameof(Region)}: {nameof(AutoDrawGroupSample)}") { }

                internal AutoDrawGroupSample(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            /// <summary>
            /// A group placement point (the game's CSMsbPointGroupPlacementPoint; formerly MassPlacement): a group of
            /// enemies, placed from a RandomAppearParam row and tied to platoons. The first 0x54 bytes follow ER's
            /// GroupPlacementPoint; NR has one more int before the tail (UnkT54), so ER's EstusFlaskRecoveryID (+54h)
            /// is probably UnkT58 here. Not traced in nightreign.exe.
            /// </summary>
            public class GroupPlacementPoint : Region
            {
                private protected override RegionType Type => RegionType.GroupPlacementPoint;
                private protected override bool HasTypeData => true;

                public GroupPlacementPoint() : base($"{nameof(Region)}: {nameof(GroupPlacementPoint)}")
                {
                    PlatoonEventNames = new string[4];
                    PartNames = new string[8];
                    LinkedAssetNames = new string[8];
                }

                internal GroupPlacementPoint(BinaryReaderEx br) : base(br) { }

                private protected override void DeepCopyTo(Region region)
                {
                    var group = (GroupPlacementPoint)region;
                    group.PlatoonEventNames = (string[])PlatoonEventNames.Clone();
                    group.PartNames = (string[])PartNames.Clone();
                    group.LinkedAssetNames = (string[])LinkedAssetNames.Clone();
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

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    RandomAppearParamID = br.ReadInt32();
                    PlatoonEventIndices = br.ReadInt32s(4);
                    PartIndices = br.ReadInt32s(8);
                    LinkedAssetIndices = br.ReadInt32s(8);
                    UnkT54 = br.ReadInt32();
                    UnkT58 = br.ReadInt32();
                    UnkT5C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(RandomAppearParamID);
                    bw.WriteInt32s(PlatoonEventIndices);
                    bw.WriteInt32s(PartIndices);
                    bw.WriteInt32s(LinkedAssetIndices);
                    bw.WriteInt32(UnkT54);
                    bw.WriteInt32(UnkT58);
                    bw.WriteInt32(UnkT5C);
                }

                /// <summary>
                /// RandomAppearParam row the group's enemies are placed from; 0 for none (0 in all retail NR points).
                /// </summary>
                [MSBParamReference(ParamName = "RandomAppearParam")]
                public int RandomAppearParamID { get; set; } = 0;

                /// <summary>
                /// PlatoonInfo events of the group (up to 4).
                /// </summary>
                [MSBReference(ReferenceType = typeof(Event))]
                public string[] PlatoonEventNames { get; set; }
                private int[] PlatoonEventIndices { get; set; } = new int[] { -1, -1, -1, -1 };

                /// <summary>
                /// Enemies of the group.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] PartNames { get; set; }
                private int[] PartIndices { get; set; } = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };

                /// <summary>
                /// Assets tied to the group (up to 8); in ER they only activate while the group does.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] LinkedAssetNames { get; set; }
                private int[] LinkedAssetIndices { get; set; } = new int[] { -1, -1, -1, -1, -1, -1, -1, -1 };

                /// <summary>
                /// Unknown, NR only (not in ER's layout). -1 in all retail NR points.
                /// </summary>
                public int UnkT54 { get; set; } = -1;

                /// <summary>
                /// Unknown. Retail NR: 0, 1, -1. Probably ER's EstusFlaskRecoveryID (the recovery row applied when the
                /// group is defeated), moved back by one int.
                /// </summary>
                public int UnkT58 { get; set; } = 0;

                /// <summary>
                /// Unknown; 0 in retail.
                /// </summary>
                public int UnkT5C { get; set; } = 0;
            }

            public class WorldMapDiscoveryPoint : Region
            {
                private protected override RegionType Type => RegionType.WorldMapDiscoveryPoint;
                private protected override bool HasTypeData => false;

                public WorldMapDiscoveryPoint() : base($"{nameof(Region)}: {nameof(WorldMapDiscoveryPoint)}") { }

                internal WorldMapDiscoveryPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class WorldMapReentryPoint : Region
            {
                private protected override RegionType Type => RegionType.WorldMapReentryPoint;
                private protected override bool HasTypeData => false;

                public WorldMapReentryPoint() : base($"{nameof(Region)}: {nameof(WorldMapReentryPoint)}") { }

                internal WorldMapReentryPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class HitSetting : Region
            {
                private protected override RegionType Type => RegionType.HitSetting;
                private protected override bool HasTypeData => true;

                public HitSetting() : base($"{nameof(Region)}: {nameof(HitSetting)}") { }

                internal HitSetting(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
            }

            /// <summary>
            /// The exe's CSFastTravelOverrideRegionIns: while the player is inside it, its flag replaces the
            /// EnableFastTravelEventFlagID of the collision and of MapDefaultInfoParam. No retail NR map uses it;
            /// nightreign.exe reads +0 as an int (140B94070) like ER, so the ER layout is used. Files without type data
            /// for this type (written by older versions) are still read.
            /// </summary>
            public class FastTravelOverride : Region
            {
                private protected override RegionType Type => RegionType.FastTravelOverride;
                private protected override bool HasTypeData => true;
                private protected override bool TypeDataOptional => true;

                public FastTravelOverride() : base($"{nameof(Region)}: {nameof(FastTravelOverride)}") { }

                internal FastTravelOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EnableFastTravelEventFlagID = br.ReadUInt32();
                    UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(EnableFastTravelEventFlagID);
                    bw.WriteInt32(UnkT04);
                }

                /// <summary>
                /// Fast travel is allowed only while this event flag is ON. 0: no override.
                /// </summary>
                public uint EnableFastTravelEventFlagID { get; set; } = 0;

                /// <summary>
                /// Unknown; 0 in ER and never read.
                /// </summary>
                public int UnkT04 { get; set; } = 0;
            }

            public class WeatherAssetGeneration : Region
            {
                private protected override RegionType Type => RegionType.WeatherAssetGeneration;
                private protected override bool HasTypeData => true;

                public WeatherAssetGeneration() : base($"{nameof(Region)}: {nameof(WeatherAssetGeneration)}") { }

                internal WeatherAssetGeneration(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
            }

            public class PlayRegion : Region
            {
                private protected override RegionType Type => RegionType.PlayRegion;
                private protected override bool HasTypeData => false;

                public PlayRegion() : base($"{nameof(Region)}: {nameof(PlayRegion)}") { }

                internal PlayRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class MidRangeEnvMapOutput : Region
            {
                private protected override RegionType Type => RegionType.MidRangeEnvMapOutput;
                private protected override bool HasTypeData => false;

                public MidRangeEnvMapOutput() : base($"{nameof(Region)}: {nameof(MidRangeEnvMapOutput)}") { }

                internal MidRangeEnvMapOutput(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class MapVisibilityOverride : Region
            {
                private protected override RegionType Type => RegionType.MapVisibilityOverride;
                private protected override bool HasTypeData => false;

                public MapVisibilityOverride() : base($"{nameof(Region)}: {nameof(MapVisibilityOverride)}") { }

                internal MapVisibilityOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            /// <summary>
            /// The exe's ride jump region in CSRideJumpRegionManImpl (formerly BigJump): a spiritspring that launches
            /// the player upwards, marked by an SFX.
            /// </summary>
            public class RideJump : Region
            {
                private protected override RegionType Type => RegionType.RideJump;
                private protected override bool HasTypeData => true;

                public RideJump() : base($"{nameof(Region)}: {nameof(RideJump)}") { }

                internal RideJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpHeight = br.ReadSingle();
                    SfxID = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(JumpHeight);
                    bw.WriteInt32(SfxID);
                    bw.WriteInt32(UnkT08);
                }

                /// <summary>
                /// Height the player will move upwards when activating this spiritspring.
                /// </summary>
                public float JumpHeight { get; set; } = 10;

                /// <summary>
                /// SFX shown at the spiritspring. Negative uses the GameSystemCommonParam default. Retail NR: 807100, 807101.
                /// </summary>
                public int SfxID { get; set; } = 807100;

                /// <summary>
                /// Unknown, NR only. Read as an int for MSB version 0x4B and later (140BA6400; -1 otherwise). Retail: 200, once 201.
                /// </summary>
                public int UnkT08 { get; set; } = 200;
            }

            public class OpenCharacterActivateLimit : Region
            {
                private protected override RegionType Type => RegionType.OpenCharacterActivateLimit;
                private protected override bool HasTypeData => false;

                public OpenCharacterActivateLimit() : base($"{nameof(Region)}: {nameof(OpenCharacterActivateLimit)}") { }

                internal OpenCharacterActivateLimit(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            /// <summary>
            /// A sound dummy point. The sound system registers its world position and feeds the listener's distance to
            /// it into the Wwise RTPC DistanceFromDummyPoint_0N, where N is DummyPointIndex (nightreign.exe 140BFB7F0
            /// reads +0 as a byte clamped to 5, like ER).
            /// </summary>
            public class SoundDummy : Region
            {
                private protected override RegionType Type => RegionType.SoundDummy;
                private protected override bool HasTypeData => true;

                public SoundDummy() : base($"{nameof(Region)}: {nameof(SoundDummy)}") { }

                internal SoundDummy(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    DummyPointIndex = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    UnkT02 = br.ReadInt16();
                    UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(DummyPointIndex);
                    bw.WriteByte(UnkT01);
                    bw.WriteInt16(UnkT02);
                    bw.WriteInt32(UnkT04);
                }

                /// <summary>
                /// Which RTPC the point drives: 0-4 = DistanceFromDummyPoint_00-04. Retail NR: 0, 1, 4.
                /// </summary>
                public byte DummyPointIndex { get; set; } = 0;

                /// <summary>
                /// Unknown; 0 in retail and not read.
                /// </summary>
                public byte UnkT01 { get; set; } = 0;

                /// <summary>
                /// Unknown; 0 in retail and not read.
                /// </summary>
                public short UnkT02 { get; set; } = 0;

                /// <summary>
                /// Unknown; 0 in retail and not read.
                /// </summary>
                public int UnkT04 { get; set; } = 0;
            }

            public class FallPreventionOverride : Region
            {
                private protected override RegionType Type => RegionType.FallPreventionOverride;
                private protected override bool HasTypeData => true;

                public FallPreventionOverride() : base($"{nameof(Region)}: {nameof(FallPreventionOverride)}") { }

                internal FallPreventionOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;

            }

            public class NavmeshCutting : Region
            {
                private protected override RegionType Type => RegionType.NavmeshCutting;
                private protected override bool HasTypeData => false;

                public NavmeshCutting() : base($"{nameof(Region)}: {nameof(NavmeshCutting)}") { }

                internal NavmeshCutting(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            /// <summary>
            /// The exe's CSMapPlaceNameOverrideRegionMan region (formerly MapNameOverride): while the player is inside
            /// it, its LocationTextID replaces the one from the collision the player stands on. No retail NR map uses
            /// it; nightreign.exe reads +0 as an int (140B99AE0) like ER, so the ER layout is used. Files without type
            /// data for this type (written by older versions) are still read.
            /// </summary>
            public class MapPlaceNameOverride : Region
            {
                private protected override RegionType Type => RegionType.MapPlaceNameOverride;
                private protected override bool HasTypeData => true;
                private protected override bool TypeDataOptional => true;

                public MapPlaceNameOverride() : base($"{nameof(Region)}: {nameof(MapPlaceNameOverride)}") { }

                internal MapPlaceNameOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    LocationTextID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(LocationTextID);
                    bw.WriteInt32(UnkT04);
                }

                /// <summary>
                /// Place-name text (PlaceName FMG) used instead of the collision's LocationTextID. -1: none.
                /// </summary>
                public int LocationTextID { get; set; } = -1;

                /// <summary>
                /// Unknown; 0 in ER and never read.
                /// </summary>
                public int UnkT04 { get; set; } = 0;
            }

            public class BigJumpExit : Region
            {
                private protected override RegionType Type => RegionType.BigJumpExit;
                private protected override bool HasTypeData => true;

                public BigJumpExit() : base($"{nameof(Region)}: {nameof(BigJumpExit)}") { }

                internal BigJumpExit(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class MountOverride : Region
            {
                private protected override RegionType Type => RegionType.MountOverride;
                private protected override bool HasTypeData => false;

                public MountOverride() : base($"{nameof(Region)}: {nameof(MountOverride)}") 
                {
                }

                internal MountOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class SmallBaseAttach : Region
            {
                private protected override RegionType Type => RegionType.SmallBaseAttach;
                private protected override bool HasTypeData => true;

                public SmallBaseAttach() : base($"{nameof(Region)}: {nameof(SmallBaseAttach)}") { }

                internal SmallBaseAttach(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;
            }

            public class BirdRoute : Region
            {
                private protected override RegionType Type => RegionType.BirdRoute;
                private protected override bool HasTypeData => true;

                public BirdRoute() : base($"{nameof(Region)}: {nameof(BirdRoute)}") { }

                internal BirdRoute(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;
            }

            public class ClearInfo : Region
            {
                private protected override RegionType Type => RegionType.ClearInfo;
                private protected override bool HasTypeData => false;

                public ClearInfo() : base($"{nameof(Region)}: {nameof(ClearInfo)}") { }

                internal ClearInfo(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class RespawnOverride : Region
            {
                private protected override RegionType Type => RegionType.RespawnOverride;
                private protected override bool HasTypeData => true;

                public RespawnOverride() : base($"{nameof(Region)}: {nameof(RespawnOverride)}") { }

                internal RespawnOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    TargetEntityID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(TargetEntityID);
                }

                // Layout
                public int TargetEntityID { get; set; }
            }

            public class UserEdgeRemovalInner : Region
            {
                private protected override RegionType Type => RegionType.UserEdgeRemovalInner;
                private protected override bool HasTypeData => true;

                public UserEdgeRemovalInner() : base($"{nameof(Region)}: {nameof(UserEdgeRemovalInner)}") { }

                internal UserEdgeRemovalInner(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;
            }

            public class UserEdgeRemovalOuter : Region
            {
                private protected override RegionType Type => RegionType.UserEdgeRemovalOuter;
                private protected override bool HasTypeData => true;

                public UserEdgeRemovalOuter() : base($"{nameof(Region)}: {nameof(UserEdgeRemovalOuter)}") { }

                internal UserEdgeRemovalOuter(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;
            }

            /// <summary>
            /// The exe's CSDlcRideJumpRegionIns (formerly BigJumpSealable): a sealable spiritspring. It only launches
            /// the player while its event flag is ON, and shows one SFX while active and another while sealed. Loaded
            /// by the same ride jump loader as RideJump (nightreign.exe 140BA7080). Fields +0..+C match ER's DlcRideJump.
            /// </summary>
            public class DlcRideJump : Region
            {
                private protected override RegionType Type => RegionType.DlcRideJump;
                private protected override bool HasTypeData => true;

                public DlcRideJump() : base($"{nameof(Region)}: {nameof(DlcRideJump)}") { }

                internal DlcRideJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpHeight = br.ReadSingle();
                    SfxID = br.ReadInt32();
                    EnableEventFlagID = br.ReadInt32();
                    DisabledSfxID = br.ReadInt32();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(JumpHeight);
                    bw.WriteInt32(SfxID);
                    bw.WriteInt32(EnableEventFlagID);
                    bw.WriteInt32(DisabledSfxID);
                    bw.WriteInt32(UnkT10);
                    bw.WriteInt32(UnkT14);
                }

                /// <summary>
                /// Height the player will move upwards when activating this spiritspring (read as a float at +0).
                /// </summary>
                public float JumpHeight { get; set; } = 0;

                /// <summary>
                /// SFX shown while the spiritspring is active. Negative uses the GameSystemCommonParam default.
                /// </summary>
                public int SfxID { get; set; } = -1;

                /// <summary>
                /// The spiritspring is active only while this event flag is ON. 0 or -1: always active.
                /// </summary>
                public int EnableEventFlagID { get; set; } = 0;

                /// <summary>
                /// SFX shown while the spiritspring is sealed (flag OFF). Negative uses the GameSystemCommonParam default.
                /// </summary>
                public int DisabledSfxID { get; set; } = -1;

                /// <summary>
                /// Unknown, NR only. Read as an int by 140B939F0 (-1 when there is no type data).
                /// </summary>
                public int UnkT10 { get; set; } = 0;

                /// <summary>
                /// Unknown, NR only. Read for MSB version 0x4F and later; when 0 or more it is used for a param lookup
                /// and an object created at the region (140B93C40).
                /// </summary>
                public int UnkT14 { get; set; } = 0;
            }

            public class Other : Region
            {
                private protected override RegionType Type => RegionType.Other;
                private protected override bool HasTypeData => false;

                public Other() : base($"{nameof(Region)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }
            }
        }
    }
}
