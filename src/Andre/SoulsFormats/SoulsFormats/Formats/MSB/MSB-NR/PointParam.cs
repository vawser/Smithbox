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
            MufflingBox = 28,
            MufflingPortal = 29,
            SoundOverride = 30,
            MufflingPlane = 31,
            PatrolPoint = 32,
            MapPoint = 33,
            SoundState = 34,
            MapInfoOverride = 35,
            AutoDrawGroupSample = 36,
            MassPlacement = 37,
            MapPointDiscoveryOverride = 38,
            MapPointParticipationOverride = 39,
            HitSetting = 40,
            FastTravelOverride = 41,
            WeatherAssetGeneration = 42,
            PlayArea = 43,
            MidRangeEnvMapOutput = 44,
            MapVisibilityOverride = 45,
            BigJump = 46,
            OpenCharacterActivateLimit = 47,
            SoundDummy = 48,
            FallPreventionOverride = 49,
            NavmeshCutting = 50,
            MapNameOverride = 51,
            BigJumpExit = 52,
            MountOverride = 53,
            SmallBaseAttach = 54,
            BirdRoute = 55,
            ClearInfo = 56,
            RespawnOverride = 57,
            UserEdgeRemovalInner = 58,
            UserEdgeRemovalOuter = 59,
            BigJumpSealable = 60,
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
            public List<Region.MufflingBox> MufflingBoxs { get; set; }
            public List<Region.MufflingPortal> MufflingPortals { get; set; }
            public List<Region.SoundOverride> SoundOverrides { get; set; }
            public List<Region.MufflingPlane> MufflingPlanes { get; set; }
            public List<Region.PatrolPoint> PatrolPoints { get; set; }
            public List<Region.MapPoint> MapPoints { get; set; }
            public List<Region.SoundState> SoundStates { get; set; }
            public List<Region.MapInfoOverride> MapInfoOverrides { get; set; }
            public List<Region.AutoDrawGroupSample> AutoDrawGroupSamples { get; set; }
            public List<Region.MassPlacement> MassPlacements { get; set; }
            public List<Region.MapPointDiscoveryOverride> MapPointDiscoveryOverrides { get; set; }
            public List<Region.MapPointParticipationOverride> MapPointParticipationOverrides { get; set; }
            public List<Region.HitSetting> HitSettings { get; set; }
            public List<Region.FastTravelOverride> FastTravelOverrides { get; set; }
            public List<Region.WeatherAssetGeneration> WeatherAssetGenerations { get; set; }
            public List<Region.PlayArea> PlayAreas { get; set; }
            public List<Region.MidRangeEnvMapOutput> MidRangeEnvMapOutputs { get; set; }
            public List<Region.MapVisibilityOverride> MapVisibilityOverrides { get; set; }
            public List<Region.BigJump> BigJumps { get; set; }
            public List<Region.OpenCharacterActivateLimit> OpenCharacterActivateLimits { get; set; }
            public List<Region.SoundDummy> SoundDummys { get; set; }
            public List<Region.FallPreventionOverride> FallPreventionOverrides { get; set; }
            public List<Region.NavmeshCutting> NavmeshCuttings { get; set; }
            public List<Region.MapNameOverride> MapNameOverrides { get; set; }
            public List<Region.BigJumpExit> BigJumpExits { get; set; }
            public List<Region.MountOverride> MountOverrides { get; set; }
            public List<Region.SmallBaseAttach> SmallBaseAttachs { get; set; }
            public List<Region.BirdRoute> BirdRoutes { get; set; }
            public List<Region.ClearInfo> ClearInfos { get; set; }
            public List<Region.RespawnOverride> RespawnOverrides { get; set; }
            public List<Region.UserEdgeRemovalInner> UserEdgeRemovalInners { get; set; }
            public List<Region.UserEdgeRemovalOuter> UserEdgeRemovalOuters { get; set; }
            public List<Region.BigJumpSealable> BigJumpSealables { get; set; }
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
                MufflingBoxs = new List<Region.MufflingBox>();
                MufflingPortals = new List<Region.MufflingPortal>();
                SoundOverrides = new List<Region.SoundOverride>();
                MufflingPlanes = new List<Region.MufflingPlane>();
                PatrolPoints = new List<Region.PatrolPoint>();
                MapPoints = new List<Region.MapPoint>();
                MapInfoOverrides = new List<Region.MapInfoOverride>();
                AutoDrawGroupSamples = new List<Region.AutoDrawGroupSample>();
                MassPlacements = new List<Region.MassPlacement>();
                MapPointDiscoveryOverrides = new List<Region.MapPointDiscoveryOverride>();
                MapPointParticipationOverrides = new List<Region.MapPointParticipationOverride>();
                HitSettings = new List<Region.HitSetting>();
                FastTravelOverrides = new List<Region.FastTravelOverride>();
                WeatherAssetGenerations = new List<Region.WeatherAssetGeneration>();
                PlayAreas = new List<Region.PlayArea>();
                MidRangeEnvMapOutputs = new List<Region.MidRangeEnvMapOutput>();
                BigJumps = new List<Region.BigJump>();
                SoundDummys = new List<Region.SoundDummy>();
                FallPreventionOverrides = new List<Region.FallPreventionOverride>();
                NavmeshCuttings = new List<Region.NavmeshCutting>();
                MapNameOverrides = new List<Region.MapNameOverride>();
                BigJumpExits = new List<Region.BigJumpExit>();
                MountOverrides = new List<Region.MountOverride>();
                SmallBaseAttachs = new List<Region.SmallBaseAttach>();
                BirdRoutes = new List<Region.BirdRoute>();
                ClearInfos = new List<Region.ClearInfo>();
                RespawnOverrides = new List<Region.RespawnOverride>();
                UserEdgeRemovalInners = new List<Region.UserEdgeRemovalInner>();
                UserEdgeRemovalOuters = new List<Region.UserEdgeRemovalOuter>();
                BigJumpSealables = new List<Region.BigJumpSealable>();

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
                    case Region.MufflingBox r: MufflingBoxs.Add(r); break;
                    case Region.MufflingPortal r: MufflingPortals.Add(r); break;
                    case Region.SoundOverride r: SoundOverrides.Add(r); break;
                    case Region.MufflingPlane r: MufflingPlanes.Add(r); break;
                    case Region.PatrolPoint r: PatrolPoints.Add(r); break;
                    case Region.MapPoint r: MapPoints.Add(r); break;
                    case Region.MapInfoOverride r: MapInfoOverrides.Add(r); break;
                    case Region.AutoDrawGroupSample r: AutoDrawGroupSamples.Add(r); break;
                    case Region.MassPlacement r: MassPlacements.Add(r); break;
                    case Region.MapPointDiscoveryOverride r: MapPointDiscoveryOverrides.Add(r); break;
                    case Region.MapPointParticipationOverride r: MapPointParticipationOverrides.Add(r); break;
                    case Region.HitSetting r: HitSettings.Add(r); break;
                    case Region.FastTravelOverride r: FastTravelOverrides.Add(r); break;
                    case Region.WeatherAssetGeneration r: WeatherAssetGenerations.Add(r); break;
                    case Region.PlayArea r: PlayAreas.Add(r); break;
                    case Region.MidRangeEnvMapOutput r: MidRangeEnvMapOutputs.Add(r); break;
                    case Region.BigJump r: BigJumps.Add(r); break;
                    case Region.SoundDummy r: SoundDummys.Add(r); break;
                    case Region.FallPreventionOverride r: FallPreventionOverrides.Add(r); break;
                    case Region.NavmeshCutting r: NavmeshCuttings.Add(r); break;
                    case Region.MapNameOverride r: MapNameOverrides.Add(r); break;
                    case Region.BigJumpExit r: BigJumpExits.Add(r); break;
                    case Region.MountOverride r: MountOverrides.Add(r); break;
                    case Region.SmallBaseAttach r: SmallBaseAttachs.Add(r); break;
                    case Region.BirdRoute r: BirdRoutes.Add(r); break;
                    case Region.RollingObjectOverride r: RollingObjectOverrides.Add(r); break;
                    case Region.ClearInfo r: ClearInfos.Add(r); break;
                    case Region.RespawnOverride r: RespawnOverrides.Add(r); break;
                    case Region.UserEdgeRemovalInner r: UserEdgeRemovalInners.Add(r); break;
                    case Region.UserEdgeRemovalOuter r: UserEdgeRemovalOuters.Add(r); break;
                    case Region.BigJumpSealable r: BigJumpSealables.Add(r); break;
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
                    MapConnections, SourceWaypoints, BuddySummonPoints, RollingObjectOverrides, MufflingBoxs,
                    MufflingPortals, SoundOverrides, MufflingPlanes, PatrolPoints,
                    MapPoints, MapInfoOverrides, AutoDrawGroupSamples, MassPlacements,
                    MapPointDiscoveryOverrides, MapPointParticipationOverrides, HitSettings,
                    FastTravelOverrides, WeatherAssetGenerations, PlayAreas, MidRangeEnvMapOutputs,
                    BigJumps, SoundDummys, FallPreventionOverrides, NavmeshCuttings, MapNameOverrides,
                    BigJumpExits, MountOverrides, SmallBaseAttachs, BirdRoutes,
                     ClearInfos, RespawnOverrides, UserEdgeRemovalInners, UserEdgeRemovalOuters,
                     BigJumpSealables,
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

                    case RegionType.MufflingBox:
                        return MufflingBoxs.EchoAdd(new Region.MufflingBox(br));

                    case RegionType.MufflingPortal:
                        return MufflingPortals.EchoAdd(new Region.MufflingPortal(br));

                    case RegionType.SoundOverride:
                        return SoundOverrides.EchoAdd(new Region.SoundOverride(br));

                    case RegionType.MufflingPlane:
                        return MufflingPlanes.EchoAdd(new Region.MufflingPlane(br));

                    case RegionType.PatrolPoint:
                        return PatrolPoints.EchoAdd(new Region.PatrolPoint(br));

                    case RegionType.MapPoint:
                        return MapPoints.EchoAdd(new Region.MapPoint(br));

                    case RegionType.MapInfoOverride:
                        return MapInfoOverrides.EchoAdd(new Region.MapInfoOverride(br));

                    case RegionType.AutoDrawGroupSample:
                        return AutoDrawGroupSamples.EchoAdd(new Region.AutoDrawGroupSample(br));

                    case RegionType.MassPlacement:
                        return MassPlacements.EchoAdd(new Region.MassPlacement(br));

                    case RegionType.MapPointDiscoveryOverride:
                        return MapPointDiscoveryOverrides.EchoAdd(new Region.MapPointDiscoveryOverride(br));

                    case RegionType.MapPointParticipationOverride:
                        return MapPointParticipationOverrides.EchoAdd(new Region.MapPointParticipationOverride(br));

                    case RegionType.HitSetting:
                        return HitSettings.EchoAdd(new Region.HitSetting(br));

                    case RegionType.FastTravelOverride:
                        return FastTravelOverrides.EchoAdd(new Region.FastTravelOverride(br));

                    case RegionType.WeatherAssetGeneration:
                        return WeatherAssetGenerations.EchoAdd(new Region.WeatherAssetGeneration(br));

                    case RegionType.PlayArea:
                        return PlayAreas.EchoAdd(new Region.PlayArea(br));

                    case RegionType.MidRangeEnvMapOutput:
                        return MidRangeEnvMapOutputs.EchoAdd(new Region.MidRangeEnvMapOutput(br));

                    case RegionType.BigJump:
                        return BigJumps.EchoAdd(new Region.BigJump(br));

                    case RegionType.SoundDummy:
                        return SoundDummys.EchoAdd(new Region.SoundDummy(br));

                    case RegionType.FallPreventionOverride:
                        return FallPreventionOverrides.EchoAdd(new Region.FallPreventionOverride(br));

                    case RegionType.NavmeshCutting:
                        return NavmeshCuttings.EchoAdd(new Region.NavmeshCutting(br));

                    case RegionType.MapNameOverride:
                        return MapNameOverrides.EchoAdd(new Region.MapNameOverride(br));

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

                    case RegionType.BigJumpSealable:
                        return BigJumpSealables.EchoAdd(new Region.BigJumpSealable(br));

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
                    if (HasTypeData ^ typeDataOffset != 0)
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

                if (HasTypeData)
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

                if (Type > RegionType.BuddySummonPoint && Type != RegionType.Other)
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

                if (Type <= RegionType.BuddySummonPoint || Type == RegionType.Other)
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
                    Unk18 = br.ReadInt32();
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
                    bw.WriteInt32(Unk18);
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
                public int Unk18 { get; set; }
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
                    Priority = br.ReadByte();
                    Unk0A = br.ReadInt16();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                    Unk20 = br.ReadInt32();
                    AmbientDiffuseIntensity = br.ReadSingle();
                    AmbientSpecularIntensity = br.ReadSingle();
                    Unk2C = br.ReadInt16();
                    Unk2E = br.ReadByte();
                    Unk2F = br.ReadByte();
                    Unk30 = br.ReadInt16();
                    Unk32 = br.ReadByte();
                    Unk33 = br.ReadByte();
                    Unk34 = br.ReadInt16();
                    Unk36 = br.ReadInt16();
                    Unk38 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(DisplayDistance);
                    bw.WriteSingle(FadeDistance);
                    bw.WriteByte(ApplyParallaxCorrection);
                    bw.WriteByte(Priority);
                    bw.WriteInt16(Unk0A);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                    bw.WriteInt32(Unk20);
                    bw.WriteSingle(AmbientDiffuseIntensity);
                    bw.WriteSingle(AmbientSpecularIntensity);
                    bw.WriteInt16(Unk2C);
                    bw.WriteByte(Unk2E);
                    bw.WriteByte(Unk2F);
                    bw.WriteInt16(Unk30);
                    bw.WriteByte(Unk32);
                    bw.WriteByte(Unk33);
                    bw.WriteInt16(Unk34);
                    bw.WriteInt16(Unk36);
                    bw.WriteInt32(Unk38);
                }

                // Layout
                public float DisplayDistance { get; set; } = 0;
                public float FadeDistance { get; set; } = 0;
                public byte ApplyParallaxCorrection { get; set; } = 0; // Boolean
                public byte Priority { get; set; } = 10; // Priority
                public short Unk0A { get; set; } = -1;
                public int Unk0C { get; set; } = 0;
                public int Unk10 { get; set; } = 0;
                public int Unk14 { get; set; } = 0;
                public int Unk18 { get; set; } = 0;
                public int Unk1C { get; set; } = 0;
                public int Unk20 { get; set; } = 0;
                public float AmbientDiffuseIntensity { get; set; } = 1f;
                public float AmbientSpecularIntensity { get; set; } = 1f;
                public short Unk2C { get; set; } = 0;
                public byte Unk2E { get; set; } = 1;
                public byte Unk2F { get; set; } = 1;
                public short Unk30 { get; set; } = -1;
                public byte Unk32 { get; set; } = 0;
                public byte Unk33 { get; set; } = 1; // Boolean
                public short Unk34 { get; set; } = 0;
                public short Unk36 { get; set; } = 1; // Bool
                public int Unk38 { get; set; } = 0;
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
                public sbyte[] ConnectionMapID { get; set; }
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

            public class RollingObjectOverride : Region
            {
                private protected override RegionType Type => RegionType.RollingObjectOverride;
                private protected override bool HasTypeData => false;

                public RollingObjectOverride() : base($"{nameof(Region)}: {nameof(RollingObjectOverride)}") { }

                internal RollingObjectOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class MufflingBox : Region
            {
                private protected override RegionType Type => RegionType.MufflingBox;
                private protected override bool HasTypeData => true;

                public MufflingBox() : base($"{nameof(Region)}: {nameof(MufflingBox)}") { }

                internal MufflingBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Offset18 = br.ReadInt64();

                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadSingle();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadSingle();
                    Unk38 = br.ReadInt32();
                    Unk3C = br.ReadSingle();
                    Unk40 = br.ReadSingle();
                    Unk44 = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt64(Offset18);

                    bw.WriteInt32(Unk20);
                    bw.WriteSingle(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteSingle(Unk34);
                    bw.WriteInt32(Unk38);
                    bw.WriteSingle(Unk3C);
                    bw.WriteSingle(Unk40);
                    bw.WriteSingle(Unk44);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;
                public int Unk08 { get; set; } = 0;
                public int Unk0C { get; set; } = 0;
                public int Unk10 { get; set; } = 0;
                public int Unk14 { get; set; } = 0;
                public int Unk20 { get; set; } = 0;
                public float Unk24 { get; set; } = 100f;
                public int Unk28 { get; set; } = 0;
                public int Unk2C { get; set; } = 0;
                public int Unk30 { get; set; } = 0;
                public float Unk34 { get; set; } = 100f;
                public int Unk38 { get; set; } = 0;
                public float Unk3C { get; set; } = -1f;
                public float Unk40 { get; set; } = -1f;
                public float Unk44 { get; set; } = -1f;

                // Offsets
                private long Offset18 { get; set; } = 32;
            }

            public class MufflingPortal : Region
            {
                private protected override RegionType Type => RegionType.MufflingPortal;
                private protected override bool HasTypeData => true;

                public MufflingPortal() : base($"{nameof(Region)}: {nameof(MufflingPortal)}") { }

                internal MufflingPortal(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Offset18 = br.ReadInt64();
                    Unk20 = br.ReadInt32();
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
                    Unk30 = br.ReadInt32();
                    Unk34 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt64(Offset18);
                    bw.WriteInt32(Unk20);
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
                    bw.WriteInt32(Unk30);
                    bw.WriteInt32(Unk34);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;
                public int Unk08 { get; set; } = 0;
                public int Unk0C { get; set; } = 0;
                public int Unk10 { get; set; } = 0;
                public int Unk14 { get; set; } = 0;
                public int Unk20 { get; set; } = 0;
                public int Unk24 { get; set; } = 0;
                public int Unk28 { get; set; } = 0;
                public int Unk2C { get; set; } = 0;
                public int Unk30 { get; set; } = 0;
                public int Unk34 { get; set; } = -1;

                // Offsets
                private long Offset18 { get; set; } = 32;
            }

            public class SoundOverride : Region
            {
                private protected override RegionType Type => RegionType.SoundOverride;
                private protected override bool HasTypeData => true;

                public SoundOverride() : base($"{nameof(Region)}: {nameof(SoundOverride)}") { }

                internal SoundOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadSByte();
                    Unk01 = br.ReadByte();
                    Unk02 = br.ReadByte();
                    Unk03 = br.ReadSByte();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt16();
                    Unk0A = br.ReadInt16();
                    Unk0C = br.ReadSByte();
                    Unk0D = br.ReadByte();
                    Unk0E = br.ReadInt16();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSByte(Unk00);
                    bw.WriteByte(Unk01);
                    bw.WriteByte(Unk02);
                    bw.WriteSByte(Unk03);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt16(Unk08);
                    bw.WriteInt16(Unk0A);
                    bw.WriteSByte(Unk0C);
                    bw.WriteByte(Unk0D);
                    bw.WriteInt16(Unk0E);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public sbyte Unk00 { get; set; } = -1;
                public byte Unk01 { get; set; } = 0;
                public byte Unk02 { get; set; } = 0;
                public sbyte Unk03 { get; set; } = -1;
                public int Unk04 { get; set; } = -1;
                public short Unk08 { get; set; } = -1;
                public short Unk0A { get; set; } = -1;
                public sbyte Unk0C { get; set; } = -1;
                public byte Unk0D { get; set; } = 0;
                public short Unk0E { get; set; } = 0;
                public int Unk10 { get; set; } = 0;
                public int Unk14 { get; set; } = 0;
                public int Unk18 { get; set; } = 0;
                public int Unk1C { get; set; } = 0;
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

            public class PatrolPoint : Region
            {
                private protected override RegionType Type => RegionType.PatrolPoint;
                private protected override bool HasTypeData => true;

                public PatrolPoint() : base($"{nameof(Region)}: {nameof(PatrolPoint)}") { }

                internal PatrolPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                }

                // Layout
                public int Unk00 { get; set; } = -1;
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

            public class MapPoint : Region
            {
                private protected override RegionType Type => RegionType.MapPoint;
                private protected override bool HasTypeData => true;

                public MapPoint() : base($"{nameof(Region)}: {nameof(MapPoint)}") { }

                internal MapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WorldMapPointParamID = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadSingle();
                    Unk0C = br.ReadSingle();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadSingle();
                    Unk18 = br.ReadSingle();
                    Unk1C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(WorldMapPointParamID);
                    bw.WriteInt32(Unk04);
                    bw.WriteSingle(Unk08);
                    bw.WriteSingle(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteSingle(Unk14);
                    bw.WriteSingle(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public int WorldMapPointParamID { get; set; } = -1;
                public int Unk04 { get; set; } = -1;
                public float Unk08 { get; set; } = -1f;
                public float Unk0C { get; set; } = -1f;
                public int Unk10 { get; set; } = -1;
                public float Unk14 { get; set; } = -1f;
                public float Unk18 { get; set; } = -1f;
                public int Unk1C { get; set; } = 0;
            }

            public class MapInfoOverride : Region
            {
                private protected override RegionType Type => RegionType.MapInfoOverride;
                private protected override bool HasTypeData => true;

                public MapInfoOverride() : base($"{nameof(Region)}: {nameof(MapInfoOverride)}") { }

                internal MapInfoOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadInt32();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk1C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                    bw.WriteInt32(Unk18);
                    bw.WriteInt32(Unk1C);
                }

                // Layout
                public int Unk00 { get; set; } = 4000;
                public int Unk04 { get; set; } = -1;
                public int Unk08 { get; set; } = 0;
                public int Unk0C { get; set; } = 0;
                public int Unk10 { get; set; } = 0;
                public int Unk14 { get; set; } = 0;
                public int Unk18 { get; set; } = 0;
                public int Unk1C { get; set; } = 0;

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

            public class MassPlacement : Region
            {
                private protected override RegionType Type => RegionType.MassPlacement;
                private protected override bool HasTypeData => true;

                public MassPlacement() : base($"{nameof(Region)}: {nameof(MassPlacement)}")
                {
                    //PartNames = new string[8];
                }

                internal MassPlacement(BinaryReaderEx br) : base(br) { }

                //private protected override void DeepCopyTo(Region region)
                //{
                //    var reward = (MassPlacement)region;
                //    reward.PartNames = (string[])PartNames.Clone();
                //}

                //internal override void GetNames(Entries entries)
                //{
                //    base.GetNames(entries);
                //    PartNames = MSB.FindNames(entries.Parts, PartIndices);
                //}

                //internal override void GetIndices(Entries entries)
                //{
                //    base.GetIndices(entries);
                //    PartIndices = MSB.FindIndices(this, entries.Parts, PartNames);
                //}

                private protected override void ReadTypeData(BinaryReaderEx br)
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
                    Unk24 = br.ReadInt32();
                    Unk28 = br.ReadInt32();
                    Unk2C = br.ReadInt32();
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
                    Unk58 = br.ReadInt32();
                    Unk5C = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
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
                    bw.WriteInt32(Unk24);
                    bw.WriteInt32(Unk28);
                    bw.WriteInt32(Unk2C);
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
                    bw.WriteInt32(Unk58);
                    bw.WriteInt32(Unk5C);
                }

                // Layout
                public int Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = -1;
                public int Unk08 { get; set; } = -1;
                public int Unk0C { get; set; } = -1;
                public int Unk10 { get; set; } = -1;
                public int Unk14 { get; set; } = -1;
                public int Unk18 { get; set; } = -1;
                public int Unk1C { get; set; } = -1;
                public int Unk20 { get; set; } = -1;
                public int Unk24 { get; set; } = -1;
                public int Unk28 { get; set; } = -1;
                public int Unk2C { get; set; } = -1;
                public int Unk30 { get; set; } = -1;
                public int Unk34 { get; set; } = -1;
                public int Unk38 { get; set; } = -1;
                public int Unk3C { get; set; } = -1;
                public int Unk40 { get; set; } = -1;
                public int Unk44 { get; set; } = -1;
                public int Unk48 { get; set; } = -1;
                public int Unk4C { get; set; } = -1;
                public int Unk50 { get; set; } = -1;
                public int Unk54 { get; set; } = -1;
                public int Unk58 { get; set; } = -1;
                public int Unk5C { get; set; } = 0;

            }

            public class MapPointDiscoveryOverride : Region
            {
                private protected override RegionType Type => RegionType.MapPointDiscoveryOverride;
                private protected override bool HasTypeData => false;

                public MapPointDiscoveryOverride() : base($"{nameof(Region)}: {nameof(MapPointDiscoveryOverride)}") { }

                internal MapPointDiscoveryOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
            }

            public class MapPointParticipationOverride : Region
            {
                private protected override RegionType Type => RegionType.MapPointParticipationOverride;
                private protected override bool HasTypeData => false;

                public MapPointParticipationOverride() : base($"{nameof(Region)}: {nameof(MapPointParticipationOverride)}") { }

                internal MapPointParticipationOverride(BinaryReaderEx br) : base(br) { }

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

            public class FastTravelOverride : Region
            {
                private protected override RegionType Type => RegionType.FastTravelOverride;
                private protected override bool HasTypeData => false;

                public FastTravelOverride() : base($"{nameof(Region)}: {nameof(FastTravelOverride)}") { }

                internal FastTravelOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
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

            public class PlayArea : Region
            {
                private protected override RegionType Type => RegionType.PlayArea;
                private protected override bool HasTypeData => false;

                public PlayArea() : base($"{nameof(Region)}: {nameof(PlayArea)}") { }

                internal PlayArea(BinaryReaderEx br) : base(br) { }

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

            public class BigJump : Region
            {
                private protected override RegionType Type => RegionType.BigJump;
                private protected override bool HasTypeData => true;

                public BigJump() : base($"{nameof(Region)}: {nameof(BigJump)}") { }

                internal BigJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpHeight = br.ReadSingle();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(JumpHeight);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                }

                // Layout
                public float JumpHeight { get; set; } = 10;
                public int Unk04 { get; set; } = 807100;
                public int Unk08 { get; set; } = 200;
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

            public class SoundDummy : Region
            {
                private protected override RegionType Type => RegionType.SoundDummy;
                private protected override bool HasTypeData => true;

                public SoundDummy() : base($"{nameof(Region)}: {nameof(SoundDummy)}") { }

                internal SoundDummy(BinaryReaderEx br) : base(br) { }

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

            public class MapNameOverride : Region
            {
                private protected override RegionType Type => RegionType.MapNameOverride;
                private protected override bool HasTypeData => false;

                public MapNameOverride() : base($"{nameof(Region)}: {nameof(MapNameOverride)}") { }

                internal MapNameOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }

                // Layout
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

            public class BigJumpSealable : Region
            {
                private protected override RegionType Type => RegionType.BigJumpSealable;
                private protected override bool HasTypeData => true;

                public BigJumpSealable() : base($"{nameof(Region)}: {nameof(BigJumpSealable)}") { }

                internal BigJumpSealable(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Unk00 = br.ReadSingle();
                    Unk04 = br.ReadInt32();
                    Unk08 = br.ReadInt32();
                    Unk0C = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(Unk00);
                    bw.WriteInt32(Unk04);
                    bw.WriteInt32(Unk08);
                    bw.WriteInt32(Unk0C);
                    bw.WriteInt32(Unk10);
                    bw.WriteInt32(Unk14);
                }

                // Layout
                public float Unk00 { get; set; } = 0;
                public int Unk04 { get; set; } = 0;
                public int Unk08 { get; set; } = 0;
                public int Unk0C { get; set; } = 0;
                public int Unk10 { get; set; } = 0;
                public int Unk14 { get; set; } = 0;
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
