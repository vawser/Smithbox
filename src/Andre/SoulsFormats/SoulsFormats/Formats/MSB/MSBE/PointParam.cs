using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;
using static SoulsFormats.MSBE.Region;

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
            DisableTumbleweed = 27,
            MufflingBox = 28,
            MufflingPortal = 29,
            SoundRegion = 30,
            MufflingPlane = 31,
            PatrolRoute = 32,
            MapPoint = 33,
            WeatherOverride = 35,
            AutoDrawGroupPoint = 36,
            GroupDefeatReward = 37,
            MapPointDiscoveryOverride = 38,
            MapPointParticipationOverride = 39,
            Hitset = 40,
            FastTravelRestriction = 41,
            WeatherCreateAssetPoint = 42,
            PlayArea = 43,
            EnvironmentMapOutput = 44,
            MountJump = 46,
            Dummy = 48,
            FallPreventionRemoval = 49,
            NavmeshCutting = 50,
            MapNameOverride = 51,
            MountJumpFall = 52,
            HorseRideOverride = 53,
            LockedMountJump = 54,
            LockedMountJumpFall = 55,
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
            /// Unknown.
            /// </summary>
            public List<Region.DisableTumbleweed> DisableTumbleweeds { get; set; }

            /// <summary>
            /// Areas where sound is muffled.
            /// </summary>
            public List<Region.MufflingBox> MufflingBoxes { get; set; }

            /// <summary>
            /// Entrances to muffling boxes.
            /// </summary>
            public List<Region.MufflingPortal> MufflingPortals { get; set; }

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
            public List<Region.MapPoint> MapPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WeatherOverride> WeatherOverrides { get; set; }


            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.AutoDrawGroupPoint> AutoDrawGroupPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.GroupDefeatReward> GroupDefeatRewards { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MapPointDiscoveryOverride> MapPointDiscoveryOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MapPointParticipationOverride> MapPointParticipationOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Hitset> Hitsets { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.FastTravelRestriction> FastTravelRestriction { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WeatherCreateAssetPoint> WeatherCreateAssetPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.PlayArea> PlayAreas { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvironmentMapOutput> EnvironmentMapOutputs { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MountJump> MountJumps { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Dummy> Dummies { get; set; }

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
            public List<Region.MapNameOverride> MapNameOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MountJumpFall> MountJumpFalls { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.HorseRideOverride> HorseRideOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.LockedMountJump> LockedMountJumps { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.LockedMountJumpFall> LockedMountJumpFalls { get; set; }

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
                DisableTumbleweeds = new List<Region.DisableTumbleweed>();
                MufflingBoxes = new List<Region.MufflingBox>();
                MufflingPortals = new List<Region.MufflingPortal>();
                SoundRegions = new List<Region.SoundRegion>();
                MufflingPlanes = new List<Region.MufflingPlane>();
                PatrolRoutes = new List<Region.PatrolRoute>();
                MapPoints = new List<Region.MapPoint>();
                WeatherOverrides = new List<Region.WeatherOverride>();
                AutoDrawGroupPoints = new List<Region.AutoDrawGroupPoint>();
                GroupDefeatRewards = new List<Region.GroupDefeatReward>();
                MapPointDiscoveryOverrides = new List<Region.MapPointDiscoveryOverride>();
                MapPointParticipationOverrides = new List<Region.MapPointParticipationOverride>();
                Hitsets = new List<Region.Hitset>();
                FastTravelRestriction = new List<Region.FastTravelRestriction>();
                WeatherCreateAssetPoints = new List<Region.WeatherCreateAssetPoint>();
                PlayAreas = new List<Region.PlayArea>();
                EnvironmentMapOutputs = new List<Region.EnvironmentMapOutput>();
                MountJumps = new List<Region.MountJump>();
                Dummies = new List<Region.Dummy>();
                FallPreventionRemovals = new List<Region.FallPreventionRemoval>();
                NavmeshCuttings = new List<Region.NavmeshCutting>();
                MapNameOverrides = new List<Region.MapNameOverride>();
                MountJumpFalls = new List<Region.MountJumpFall>();
                HorseRideOverrides = new List<Region.HorseRideOverride>();
                LockedMountJumps = new List<Region.LockedMountJump>();
                LockedMountJumpFalls = new List<Region.LockedMountJumpFall>();
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
                    case Region.MufflingBox r: MufflingBoxes.Add(r); break;
                    case Region.MufflingPortal r: MufflingPortals.Add(r); break;
                    case Region.SoundRegion r: SoundRegions.Add(r); break;
                    case Region.MufflingPlane r: MufflingPlanes.Add(r); break;
                    case Region.PatrolRoute r: PatrolRoutes.Add(r); break;
                    case Region.MapPoint r: MapPoints.Add(r); break;
                    case Region.WeatherOverride r: WeatherOverrides.Add(r); break;
                    case Region.AutoDrawGroupPoint r: AutoDrawGroupPoints.Add(r); break;
                    case Region.GroupDefeatReward r: GroupDefeatRewards.Add(r); break;
                    case Region.MapPointDiscoveryOverride r: MapPointDiscoveryOverrides.Add(r); break;
                    case Region.MapPointParticipationOverride r: MapPointParticipationOverrides.Add(r); break;
                    case Region.Hitset r: Hitsets.Add(r); break;
                    case Region.FastTravelRestriction r: FastTravelRestriction.Add(r); break;
                    case Region.WeatherCreateAssetPoint r: WeatherCreateAssetPoints.Add(r); break;
                    case Region.PlayArea r: PlayAreas.Add(r); break;
                    case Region.EnvironmentMapOutput r: EnvironmentMapOutputs.Add(r); break;
                    case Region.MountJump r: MountJumps.Add(r); break;
                    case Region.Dummy r: Dummies.Add(r); break;
                    case Region.FallPreventionRemoval r: FallPreventionRemovals.Add(r); break;
                    case Region.NavmeshCutting r: NavmeshCuttings.Add(r); break;
                    case Region.MapNameOverride r: MapNameOverrides.Add(r); break;
                    case Region.MountJumpFall r: MountJumpFalls.Add(r); break;
                    case Region.HorseRideOverride r: HorseRideOverrides.Add(r); break;
                    case Region.LockedMountJump r: LockedMountJumps.Add(r); break;
                    case Region.LockedMountJumpFall r: LockedMountJumpFalls.Add(r); break;
                    case Region.DisableTumbleweed r: DisableTumbleweeds.Add(r); break;
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
                    Connections, PatrolRoute22s, BuddySummonPoints, DisableTumbleweeds, MufflingBoxes,
                    MufflingPortals, SoundRegions, MufflingPlanes, PatrolRoutes,
                    MapPoints, WeatherOverrides, AutoDrawGroupPoints, GroupDefeatRewards,
                    MapPointDiscoveryOverrides, MapPointParticipationOverrides, Hitsets,
                    FastTravelRestriction, WeatherCreateAssetPoints, PlayAreas, EnvironmentMapOutputs,
                    MountJumps, Dummies, FallPreventionRemovals, NavmeshCuttings, MapNameOverrides,
                    MountJumpFalls, HorseRideOverrides, LockedMountJumps, LockedMountJumpFalls, Others);
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

                    case RegionType.DisableTumbleweed:
                        return DisableTumbleweeds.EchoAdd(new Region.DisableTumbleweed(br));

                    case RegionType.MufflingBox:
                        return MufflingBoxes.EchoAdd(new Region.MufflingBox(br));

                    case RegionType.MufflingPortal:
                        return MufflingPortals.EchoAdd(new Region.MufflingPortal(br));

                    case RegionType.SoundRegion:
                        return SoundRegions.EchoAdd(new Region.SoundRegion(br));

                    case RegionType.MufflingPlane:
                        return MufflingPlanes.EchoAdd(new Region.MufflingPlane(br));

                    case RegionType.PatrolRoute:
                        return PatrolRoutes.EchoAdd(new Region.PatrolRoute(br));

                    case RegionType.MapPoint:
                        return MapPoints.EchoAdd(new Region.MapPoint(br));

                    case RegionType.WeatherOverride:
                        return WeatherOverrides.EchoAdd(new Region.WeatherOverride(br));

                    case RegionType.AutoDrawGroupPoint:
                        return AutoDrawGroupPoints.EchoAdd(new Region.AutoDrawGroupPoint(br));

                    case RegionType.GroupDefeatReward:
                        return GroupDefeatRewards.EchoAdd(new Region.GroupDefeatReward(br));

                    case RegionType.MapPointDiscoveryOverride:
                        return MapPointDiscoveryOverrides.EchoAdd(new Region.MapPointDiscoveryOverride(br));

                    case RegionType.MapPointParticipationOverride:
                        return MapPointParticipationOverrides.EchoAdd(new Region.MapPointParticipationOverride(br));

                    case RegionType.Hitset:
                        return Hitsets.EchoAdd(new Region.Hitset(br));

                    case RegionType.FastTravelRestriction:
                        return FastTravelRestriction.EchoAdd(new Region.FastTravelRestriction(br));

                    case RegionType.WeatherCreateAssetPoint:
                        return WeatherCreateAssetPoints.EchoAdd(new Region.WeatherCreateAssetPoint(br));

                    case RegionType.PlayArea:
                        return PlayAreas.EchoAdd(new Region.PlayArea(br));

                    case RegionType.EnvironmentMapOutput:
                        return EnvironmentMapOutputs.EchoAdd(new Region.EnvironmentMapOutput(br));

                    case RegionType.MountJump:
                        return MountJumps.EchoAdd(new Region.MountJump(br));

                    case RegionType.Dummy:
                        return Dummies.EchoAdd(new Region.Dummy(br));

                    case RegionType.FallPreventionRemoval:
                        return FallPreventionRemovals.EchoAdd(new Region.FallPreventionRemoval(br));

                    case RegionType.NavmeshCutting:
                        return NavmeshCuttings.EchoAdd(new Region.NavmeshCutting(br));

                    case RegionType.MapNameOverride:
                        return MapNameOverrides.EchoAdd(new Region.MapNameOverride(br));

                    case RegionType.MountJumpFall:
                        return MountJumpFalls.EchoAdd(new Region.MountJumpFall(br));

                    case RegionType.HorseRideOverride:
                        return HorseRideOverrides.EchoAdd(new Region.HorseRideOverride(br));

                    case RegionType.LockedMountJump:
                        return LockedMountJumps.EchoAdd(new Region.LockedMountJump(br));

                    case RegionType.LockedMountJumpFall:
                        return LockedMountJumpFalls.EchoAdd(new Region.LockedMountJumpFall(br));

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
            /// Unknown.
            /// </summary>
            public int UnkS04 { get; set; }

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
                UnkS04 = br.ReadInt32();
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

                bw.FillInt64("Unk4Offset", bw.Position - start);
                bw.WriteInt32(MapID);
                bw.WriteInt32(UnkS04);
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
            /// A point where a player can invade your world.
            /// </summary>
            public class InvasionPoint : Region
            {
                private protected override RegionType Type => RegionType.InvasionPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Not sure what this does.
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
            /// Unknown.
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
                /// Unknown.
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
                /// Unknown.
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
                /// If true, the effect is off by default until enabled by event scripts.
                /// </summary>
                public int StartDisabled { get; set; }

                /// <summary>
                /// Creates an SFX with default values.
                /// </summary>
                public SFX() : base($"{nameof(Region)}: {nameof(SFX)}") { }

                internal SFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    StartDisabled = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(StartDisabled);
                }
            }

            /// <summary>
            /// Unknown.
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
                /// Unknown.
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
            /// A point where the player can spawn into the map.
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
                /// Unknown. Always 0 or 2.
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
            /// Unknown.
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
                /// Unknown.
                /// </summary>
                public byte ParallaxCorrection { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte Priority { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0A { get; set; }

                /// <summary>
                /// Strength of ambient fiffuse light .
                /// </summary>
                public float AmbientDiffuseIntensity { get; set; }

                /// <summary>
                /// Strength of ambient specular light.
                /// </summary>
                public float AmbientSpecularIntensity { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT2C { get; set; }

                /// <summary>
                /// Affects lighting with other fields when true. Possibly normalizes light when false.
                /// </summary>
                public bool IsModifyLight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT2F { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT32 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public bool UnkT33 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT36 { get; set; }

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
                    Priority = br.ReadByte();
                    UnkT0A = br.ReadInt16();
                    br.AssertPattern(0x18, 0x00);
                    AmbientDiffuseIntensity = br.ReadSingle();
                    AmbientSpecularIntensity = br.ReadSingle();
                    UnkT2C = br.ReadInt16();
                    IsModifyLight = br.ReadBoolean();
                    UnkT2F = br.ReadBoolean();
                    UnkT30 = br.ReadInt16();
                    UnkT32 = br.ReadBoolean();
                    UnkT33 = br.ReadBoolean();
                    UnkT34 = br.ReadInt16();
                    UnkT36 = br.ReadInt16();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(DisplayDistance);
                    bw.WriteSingle(FadeDistance);
                    bw.WriteByte(ParallaxCorrection);
                    bw.WriteByte(Priority);
                    bw.WriteInt16(UnkT0A);
                    bw.WritePattern(0x18, 0x00);
                    bw.WriteSingle(AmbientDiffuseIntensity);
                    bw.WriteSingle(AmbientSpecularIntensity);
                    bw.WriteInt16(UnkT2C);
                    bw.WriteBoolean(IsModifyLight);
                    bw.WriteBoolean(UnkT2F);
                    bw.WriteInt16(UnkT30);
                    bw.WriteBoolean(UnkT32);
                    bw.WriteBoolean(UnkT33);
                    bw.WriteInt16(UnkT34);
                    bw.WriteInt16(UnkT36);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
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
            /// Unknown
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
            /// Unknown
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
            /// An area where sound is muffled.
            /// </summary>
            public class MufflingBox : Region
            {
                private protected override RegionType Type => RegionType.MufflingBox;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT3C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT40 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT44 { get; set; }

                /// <summary>
                /// Creates a MufflingBox with default values.
                /// </summary>
                public MufflingBox() : base($"{nameof(Region)}: {nameof(MufflingBox)}") { }

                internal MufflingBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
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
                    bw.WriteInt32(UnkT00);
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
            /// An entrance to a muffling box.
            /// </summary>
            public class MufflingPortal : Region
            {
                private protected override RegionType Type => RegionType.MufflingPortal;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a MufflingPortal with default values.
                /// </summary>
                public MufflingPortal() : base($"{nameof(Region)}: {nameof(MufflingPortal)}") { }

                internal MufflingPortal(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
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
                    bw.WriteInt32(UnkT00);
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
            /// Unknown.
            /// </summary>
            public class SoundRegion : Region
            {
                private protected override RegionType Type => RegionType.SoundRegion;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT02 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "WwiseValueToStrParam_EnvPlaceType")]
                public int WwiseEnvPlaceTypeID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT0A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0C { get; set; }

                /// <summary>
                /// Creates a SoundRegion with default values.
                /// </summary>
                public SoundRegion() : base($"{nameof(Region)}: {nameof(SoundRegion)}") { }

                internal SoundRegion(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    UnkT02 = br.ReadByte();
                    UnkT03 = br.ReadByte();
                    WwiseEnvPlaceTypeID = br.ReadInt32();
                    UnkT08 = br.ReadInt16();
                    UnkT0A = br.ReadInt16();
                    UnkT0C = br.ReadByte();
                    br.AssertPattern(19, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteByte(UnkT02);
                    bw.WriteByte(UnkT03);
                    bw.WriteInt32(WwiseEnvPlaceTypeID);
                    bw.WriteInt16(UnkT08);
                    bw.WriteInt16(UnkT0A);
                    bw.WriteByte(UnkT0C);
                    bw.WritePattern(19, 0x00);
                }
            }

            /// <summary>
            /// Unknown.
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
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a PatrolRoute with default values.
                /// </summary>
                public PatrolRoute() : base($"{nameof(Region)}: {nameof(PatrolRoute)}") { }

                internal PatrolRoute(BinaryReaderEx br) : base(br) { }

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
            /// Unknown.
            /// </summary>
            public class MapPoint : Region
            {
                private protected override RegionType Type => RegionType.MapPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Determines which WorldMapPointParam to use.
                /// </summary>
                public int WorldMapPointParamID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Creates a MapPoint with default values.
                /// </summary>
                public MapPoint() : base($"{nameof(Region)}: {nameof(MapPoint)}") { }

                internal MapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WorldMapPointParamID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadInt32();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(WorldMapPointParamID);
                    bw.WriteInt32(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteInt32(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WeatherOverride : Region
            {
                private protected override RegionType Type => RegionType.WeatherOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Determines which WeatherLotParam ID to use.
                /// </summary>
                public int WeatherLotParamID { get; set; }

                public sbyte UnkT08 { get; set; }
                public sbyte UnkT09 { get; set; }
                public sbyte UnkT0A { get; set; }
                public sbyte UnkT0B { get; set; }

                /// <summary>
                /// Creates a WeatherOverride with default values.
                /// </summary>
                public WeatherOverride() : base($"{nameof(Region)}: {nameof(WeatherOverride)}") { }

                internal WeatherOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    WeatherLotParamID = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT08 = br.ReadSByte();
                    UnkT09 = br.ReadSByte();
                    UnkT0A = br.ReadSByte();
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
                    bw.WriteInt32(-1);
                    bw.WriteSByte(UnkT08);
                    bw.WriteSByte(UnkT09);
                    bw.WriteSByte(UnkT0A);
                    bw.WriteSByte(UnkT0B);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class AutoDrawGroupPoint : Region
            {
                private protected override RegionType Type => RegionType.AutoDrawGroupPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
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
            /// Unknown.
            /// </summary>
            public class GroupDefeatReward : Region
            {
                private protected override RegionType Type => RegionType.GroupDefeatReward;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT08 { get; set; }

                /// <summary>
                /// References to enemies to defeat to receive the reward.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Part))]
                public string[] PartNames { get; set; }
                private int[] PartIndices { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT34 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT38 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int EstusFlaskRecoveryID { get; set; }

                /// <summary>
                /// Creates a GroupDefeatReward with default values.
                /// </summary>
                public GroupDefeatReward() : base($"{nameof(Region)}: {nameof(GroupDefeatReward)}")
                {
                    PartNames = new string[8];
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var reward = (GroupDefeatReward)region;
                    reward.PartNames = (string[])PartNames.Clone();
                }

                internal GroupDefeatReward(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    PartIndices = br.ReadInt32s(8);

                    UnkT34 = br.ReadInt32();
                    UnkT38 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    EstusFlaskRecoveryID = br.ReadInt32();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(UnkT08);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32s(PartIndices);

                    bw.WriteInt32(UnkT34);
                    bw.WriteInt32(UnkT38);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(EstusFlaskRecoveryID);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                internal override void GetNames(Entries entries)
                {
                    base.GetNames(entries);
                    PartNames = MSB.FindNames(entries.Parts, PartIndices);
                }

                internal override void GetIndices(Entries entries)
                {
                    base.GetIndices(entries);
                    PartIndices = MSB.FindIndices(this, entries.Parts, PartNames);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MapPointDiscoveryOverride : Region
            {
                private protected override RegionType Type => RegionType.MapPointDiscoveryOverride;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an MapPointDiscoveryOverride with default values.
                /// </summary>
                public MapPointDiscoveryOverride() : base($"{nameof(Region)}: {nameof(MapPointDiscoveryOverride)}") { }

                internal MapPointDiscoveryOverride(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MapPointParticipationOverride : Region
            {
                private protected override RegionType Type => RegionType.MapPointParticipationOverride;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an MapPointParticipationOverride with default values.
                /// </summary>
                public MapPointParticipationOverride() : base($"{nameof(Region)}: {nameof(MapPointParticipationOverride)}") { }

                internal MapPointParticipationOverride(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Hitset : Region
            {
                private protected override RegionType Type => RegionType.Hitset;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
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
            /// Region that disables fast travel.
            /// </summary>
            public class FastTravelRestriction : Region
            {
                private protected override RegionType Type => RegionType.FastTravelRestriction;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Disables fast travel when flag is OFF.
                /// </summary>
                public uint EventFlagID { get; set; }

                /// <summary>
                /// Creates a FastTravelRestriction with default values.
                /// </summary>
                public FastTravelRestriction() : base($"{nameof(Region)}: {nameof(FastTravelRestriction)}") { }

                internal FastTravelRestriction(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EventFlagID = br.ReadUInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32(EventFlagID);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
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
            /// Unknown.
            /// </summary>
            public class PlayArea : Region
            {
                private protected override RegionType Type => RegionType.PlayArea;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int PlayRegionID { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a PlayArea with default values.
                /// </summary>
                public PlayArea() : base($"{nameof(Region)}: {nameof(PlayArea)}") { }

                internal PlayArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PlayRegionID = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlayRegionID);
                    bw.WriteInt32(UnkT04);
                }
            }

            /// <summary>
            /// Unknown.
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
            /// Unknown.
            /// </summary>
            public class MountJump : Region
            {
                private protected override RegionType Type => RegionType.MountJump;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Height the player will move upwards when activating a MountJump.
                /// </summary>
                public float JumpHeight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a MountJump with default values.
                /// </summary>
                public MountJump() : base($"{nameof(Region)}: {nameof(MountJump)}") { }

                internal MountJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpHeight = br.ReadSingle();
                    UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(JumpHeight);
                    bw.WriteInt32(UnkT04);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Dummy : Region
            {
                private protected override RegionType Type => RegionType.Dummy;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates a Dummy with default values.
                /// </summary>
                public Dummy() : base($"{nameof(Region)}: {nameof(Dummy)}") { }

                internal Dummy(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
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
            /// Unknown.
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
            /// Unknown.
            /// </summary>
            public class MapNameOverride : Region
            {
                private protected override RegionType Type => RegionType.MapNameOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// FMG id to use for popup titlecards. Negative values apply when entering, Positive values apply on map load.
                /// </summary>
                public int TextID { get; set; }

                /// <summary>
                /// Creates a MapNameOverride with default values.
                /// </summary>
                public MapNameOverride() : base($"{nameof(Region)}: {nameof(MapNameOverride)}") { }

                internal MapNameOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    TextID = br.ReadInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(TextID);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MountJumpFall : Region
            {
                private protected override RegionType Type => RegionType.MountJumpFall;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a MountJumpFall with default values.
                /// </summary>
                public MountJumpFall() : base($"{nameof(Region)}: {nameof(MountJumpFall)}") { }

                internal MountJumpFall(BinaryReaderEx br) : base(br) { }

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
            /// Affects where torrent can be summoned.
            /// </summary>
            public class HorseRideOverride : Region
            {
                private protected override RegionType Type => RegionType.HorseRideOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// 1 = Forbid riding torrent, 2 = Permit riding torrent
                /// </summary>
                public uint OverrideType { get; set; } = 1;

                /// <summary>
                /// Creates a HorseRideOverride with default values.
                /// </summary>
                public HorseRideOverride() : base($"{nameof(Region)}: {nameof(HorseRideOverride)}") 
                {
                    OverrideType = 1;
                }

                internal HorseRideOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    OverrideType = br.ReadUInt32();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteUInt32((uint)OverrideType);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class LockedMountJump : Region
            {
                private protected override RegionType Type => RegionType.LockedMountJump;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Height the player will move upwards when activating a MountJump.
                /// </summary>
                public float JumpHeight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Probably event flag to enable.
                /// </summary>
                public int EnableEventFlagID { get; set; }

                /// <summary>
                /// Creates a LockedMountJump with default values.
                /// </summary>
                public LockedMountJump() : base($"{nameof(Region)}: {nameof(LockedMountJump)}") { }

                internal LockedMountJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpHeight = br.ReadSingle();
                    UnkT04 = br.ReadInt32();
                    EnableEventFlagID = br.ReadInt32();
                    br.AssertInt32(-1);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(JumpHeight);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(EnableEventFlagID);
                    bw.WriteInt32(-1);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class LockedMountJumpFall : Region
            {
                private protected override RegionType Type => RegionType.LockedMountJumpFall;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Probably event flag to enable.
                /// </summary>
                public int EnableEventFlagID { get; set; }

                /// <summary>
                /// Creates a LockedMountJumpFall with default values.
                /// </summary>
                public LockedMountJumpFall() : base($"{nameof(Region)}: {nameof(LockedMountJumpFall)}") { }

                internal LockedMountJumpFall(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                    EnableEventFlagID = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(EnableEventFlagID);
                }

            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class DisableTumbleweed : Region
            {
                private protected override RegionType Type => RegionType.DisableTumbleweed;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates a DisableTumbleweed with default values.
                /// </summary>
                public DisableTumbleweed() : base($"{nameof(Region)}: {nameof(DisableTumbleweed)}") { }

                internal DisableTumbleweed(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(-1);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(-1);
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
