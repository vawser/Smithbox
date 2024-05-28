using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSBE.Region.HorseRideOverride;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        public enum RegionType : int
        {
            None = 0,
            EntryPoint = 1,
            EnvMapPoint = 2,
            Unknown_3 = 3, // NOT IMPLEMENTED
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            Unknown_7 = 7, // NOT IMPLEMENTED
            ReturnPoint = 8, // NOT IMPLEMENTED
            Message = 9, // NOT IMPLEMENTED
            Unknown_10 = 10, // NOT IMPLEMENTED
            Unknown_11 = 11, // NOT IMPLEMENTED
            Unknown_12 = 12, // NOT IMPLEMENTED
            FallReturnPoint = 13, // NOT IMPLEMENTED
            Unknown_14 = 14, // NOT IMPLEMENTED
            Unknown_15 = 15, // NOT IMPLEMENTED
            Unknown_16 = 16, // NOT IMPLEMENTED
            EnvMapEffectBox = 17,
            WindPlacement = 18,
            Unknown_19 = 19, // NOT IMPLEMENTED
            Unknown_20 = 20, // NOT IMPLEMENTED
            Connection = 21, // NOT IMPLEMENTED
            SourceWaypoint = 22, // NOT IMPLEMENTED
            StaticWaypoint = 23, // NOT IMPLEMENTED
            MapGridLayerConnection = 24, // NOT IMPLEMENTED
            EnemySpawnPoint = 25, // NOT IMPLEMENTED
            BuddySummonPoint = 26, // NOT IMPLEMENTED
            RollingAssetGeneration = 27, // NOT IMPLEMENTED
            MufflingBox = 28,
            MufflingPortal = 29,
            SoundOverride = 30,
            MufflingPlane = 31, // NOT IMPLEMENTED
            Patrol = 32,
            FeMapDisplay = 33,
            ElectroMagneticStorm = 34, // NOT IMPLEMENTED
            OperationalArea = 35,
            AiInformationSharing = 36,
            AiTarget = 37,
            WaveSimulation = 38, // NOT IMPLEMENTED
            WwiseEnvironmentSound = 39,
            Cover = 40, // NOT IMPLEMENTED
            MissionPlacement = 41, // NOT IMPLEMENTED
            NaviVolumeResolution = 42, // NOT IMPLEMENTED
            MiniArea = 43, // NOT IMPLEMENTED
            ConnectionBorder = 44, // NOT IMPLEMENTED
            NaviGeneration = 45,
            TopdownView = 46,
            CharacterFollowing = 47,
            NaviCvCancel = 48, // NOT IMPLEMENTED
            NavmeshCostControl = 49,
            ArenaControl = 50,
            ArenaAppearance = 51,
            GarageCamera = 52,
            JumpEdgeRestriction = 53,
            CutscenePlayback = 54,
            FallPreventionWallRemoval = 55,
            BigJump = 56,
            Other = -1,
        }

        /// <summary>
        /// Points and volumes used to trigger various effects.
        /// </summary>
        public class PointParam : Param<Region>, IMsbParam<IMsbRegion>
        {
            private int ParamVersion;

            /// <summary>
            /// Unknown
            /// </summary>
            public List<Region.EntryPoint> EntryPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvMapPoint> EnvMapPoints { get; set; }

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
            /// Unknown.
            /// </summary>
            public List<Region.EnvMapEffectBox> EnvMapEffectBoxes { get; set; }

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
            public List<Region.SoundOverride> SoundOverrides { get; set; }

            /// <summary>
            /// Points that describe an NPC patrol path.
            /// </summary>
            public List<Region.Patrol> Patrols { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.FeMapDisplay> FeMapDisplays { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.OperationalArea> OperationalAreas { get; set; }


            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.AiTarget> AiTargets { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WwiseEnvironmentSound> WwiseEnvironmentSounds { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.CharacterFollowing> CharacterFollowings { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.NavmeshCostControl> NavmeshCostControls { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.ArenaControl> ArenaControls { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.ArenaAppearance> ArenaAppearances { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.GarageCamera> GarageCameras { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.CutscenePlayback> CutscenePlaybacks { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.FallPreventionWallRemoval> FallPreventionWallRemovals { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.BigJump> BigJumps { get; set; }

            /// <summary>
            /// Most likely a dumping ground for unused regions.
            /// </summary>
            public List<Region.Other> Others { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.None> Nones { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_3> Unknown_3s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_7> Unknown_7s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.ReturnPoint> ReturnPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Message> Messages { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_10> Unknown_10s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_11> Unknown_11s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_12> Unknown_12s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.FallReturnPoint> FallReturnPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_14> Unknown_14s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_15> Unknown_15s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_16> Unknown_16s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WindPlacement> WindPlacements { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_19> Unknown_19s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Unknown_20> Unknown_20s { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Connection> Connections { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.SourceWaypoint> SourceWaypoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.StaticWaypoint> StaticWaypoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MapGridLayerConnection> MapGridLayerConnections { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnemySpawnPoint> EnemySpawnPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.BuddySummonPoint> BuddySummonPoints { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.RollingAssetGeneration> RollingAssetGenerations { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MufflingPlane> MufflingPlanes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.ElectroMagneticStorm> ElectroMagneticStorms { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.AiInformationSharing> AiInformationSharings { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WaveSimulation> WaveSimulations { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Cover> Covers { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MissionPlacement> MissionPlacements { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.NaviVolumeResolution> NaviVolumeResolutions { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MiniArea> MiniAreas { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.ConnectionBorder> ConnectionBorders { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.NaviGeneration> NaviGenerations { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.TopdownView> TopdownViews { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.NaviCvCancel> NaviCvCancels { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.JumpEdgeRestriction> JumpEdgeRestrictions { get; set; }

            /// <summary>
            /// Creates an empty PointParam with the default version.
            /// </summary>
            public PointParam() : base(52, "POINT_PARAM_ST")
            {
                ParamVersion = base.Version;

                EntryPoints = new List<Region.EntryPoint>();
                EnvMapPoints = new List<Region.EnvMapPoint>();
                Sounds = new List<Region.Sound>();
                SFX = new List<Region.SFX>();
                WindSFX = new List<Region.WindSFX>();
                EnvMapEffectBoxes = new List<Region.EnvMapEffectBox>();
                MufflingBoxes = new List<Region.MufflingBox>();
                MufflingPortals = new List<Region.MufflingPortal>();
                SoundOverrides = new List<Region.SoundOverride>();
                Patrols = new List<Region.Patrol>();
                FeMapDisplays = new List<Region.FeMapDisplay>();
                OperationalAreas = new List<Region.OperationalArea>();
                AiTargets = new List<Region.AiTarget>();
                WwiseEnvironmentSounds = new List<Region.WwiseEnvironmentSound>();
                CharacterFollowings = new List<Region.CharacterFollowing>();
                NavmeshCostControls = new List<Region.NavmeshCostControl>();
                ArenaControls = new List<Region.ArenaControl>();
                ArenaAppearances = new List<Region.ArenaAppearance>();
                GarageCameras = new List<Region.GarageCamera>();
                CutscenePlaybacks = new List<Region.CutscenePlayback>();
                FallPreventionWallRemovals = new List<Region.FallPreventionWallRemoval>();
                BigJumps = new List<Region.BigJump>();

                Unknown_3s = new List<Region.Unknown_3>();
                Unknown_7s = new List<Region.Unknown_7>();
                ReturnPoints = new List<Region.ReturnPoint>();
                Messages = new List<Region.Message>();
                Unknown_10s = new List<Region.Unknown_10>();
                Unknown_11s = new List<Region.Unknown_11>();
                Unknown_12s = new List<Region.Unknown_12>();
                FallReturnPoints = new List<Region.FallReturnPoint>();
                Unknown_14s = new List<Region.Unknown_14>();
                Unknown_15s = new List<Region.Unknown_15>();
                Unknown_16s = new List<Region.Unknown_16>();
                WindPlacements = new List<Region.WindPlacement>();
                Unknown_19s = new List<Region.Unknown_19>();
                Unknown_20s = new List<Region.Unknown_20>();
                Connections = new List<Region.Connection>();
                SourceWaypoints = new List<Region.SourceWaypoint>();
                StaticWaypoints = new List<Region.StaticWaypoint>();
                MapGridLayerConnections = new List<Region.MapGridLayerConnection>();
                EnemySpawnPoints = new List<Region.EnemySpawnPoint>();
                BuddySummonPoints = new List<Region.BuddySummonPoint>();
                RollingAssetGenerations = new List<Region.RollingAssetGeneration>();
                MufflingPlanes = new List<Region.MufflingPlane>();
                ElectroMagneticStorms = new List<Region.ElectroMagneticStorm>();
                AiInformationSharings = new List<Region.AiInformationSharing>();
                WaveSimulations = new List<Region.WaveSimulation>();
                Covers = new List<Region.Cover>();
                MissionPlacements = new List<Region.MissionPlacement>();
                NaviVolumeResolutions = new List<Region.NaviVolumeResolution>();
                MiniAreas = new List<Region.MiniArea>();
                ConnectionBorders = new List<Region.ConnectionBorder>();
                NaviGenerations = new List<Region.NaviGeneration>();
                TopdownViews = new List<Region.TopdownView>();
                NaviCvCancels = new List<Region.NaviCvCancel>();
                JumpEdgeRestrictions = new List<Region.JumpEdgeRestriction>();

                Others = new List<Region.Other>();
                Nones = new List<Region.None>();
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region)
            {
                switch (region)
                {
                    case Region.EntryPoint r:
                        EntryPoints.Add(r);
                        break;
                    case Region.EnvMapPoint r:
                        EnvMapPoints.Add(r);
                        break;
                    case Region.Sound r:
                        Sounds.Add(r);
                        break;
                    case Region.SFX r:
                        SFX.Add(r);
                        break;
                    case Region.WindSFX r:
                        WindSFX.Add(r);
                        break;
                    case Region.EnvMapEffectBox r:
                        EnvMapEffectBoxes.Add(r);
                        break;
                    case Region.MufflingBox r:
                        MufflingBoxes.Add(r);
                        break;
                    case Region.MufflingPortal r:
                        MufflingPortals.Add(r);
                        break;
                    case Region.SoundOverride r:
                        SoundOverrides.Add(r);
                        break;
                    case Region.Patrol r:
                        Patrols.Add(r);
                        break;
                    case Region.FeMapDisplay r:
                        FeMapDisplays.Add(r);
                        break;
                    case Region.OperationalArea r:
                        OperationalAreas.Add(r);
                        break;
                    case Region.AiTarget r:
                        AiTargets.Add(r);
                        break;
                    case Region.WwiseEnvironmentSound r:
                        WwiseEnvironmentSounds.Add(r);
                        break;
                    case Region.CharacterFollowing r:
                        CharacterFollowings.Add(r);
                        break;
                    case Region.NavmeshCostControl r:
                        NavmeshCostControls.Add(r);
                        break;
                    case Region.ArenaControl r:
                        ArenaControls.Add(r);
                        break;
                    case Region.ArenaAppearance r:
                        ArenaAppearances.Add(r);
                        break;
                    case Region.GarageCamera r:
                        GarageCameras.Add(r);
                        break;
                    case Region.CutscenePlayback r:
                        CutscenePlaybacks.Add(r);
                        break;
                    case Region.FallPreventionWallRemoval r:
                        FallPreventionWallRemovals.Add(r);
                        break;
                    case Region.BigJump r:
                        BigJumps.Add(r);
                        break;
                    case Region.Unknown_3 r:
                        Unknown_3s.Add(r);
                        break;
                    case Region.Unknown_7 r:
                        Unknown_7s.Add(r);
                        break;
                    case Region.ReturnPoint r:
                        ReturnPoints.Add(r);
                        break;
                    case Region.Message r:
                        Messages.Add(r);
                        break;
                    case Region.Unknown_10 r:
                        Unknown_10s.Add(r);
                        break;
                    case Region.Unknown_11 r:
                        Unknown_11s.Add(r);
                        break;
                    case Region.Unknown_12 r:
                        Unknown_12s.Add(r);
                        break;
                    case Region.FallReturnPoint r:
                        FallReturnPoints.Add(r);
                        break;
                    case Region.Unknown_14 r:
                        Unknown_14s.Add(r);
                        break;
                    case Region.Unknown_15 r:
                        Unknown_15s.Add(r);
                        break;
                    case Region.Unknown_16 r:
                        Unknown_16s.Add(r);
                        break;
                    case Region.WindPlacement r:
                        WindPlacements.Add(r);
                        break;
                    case Region.Unknown_19 r:
                        Unknown_19s.Add(r);
                        break;
                    case Region.Unknown_20 r:
                        Unknown_20s.Add(r);
                        break;
                    case Region.Connection r:
                        Connections.Add(r);
                        break;
                    case Region.SourceWaypoint r:
                        SourceWaypoints.Add(r);
                        break;
                    case Region.StaticWaypoint r:
                        StaticWaypoints.Add(r);
                        break;
                    case Region.MapGridLayerConnection r:
                        MapGridLayerConnections.Add(r);
                        break;
                    case Region.EnemySpawnPoint r:
                        EnemySpawnPoints.Add(r);
                        break;
                    case Region.BuddySummonPoint r:
                        BuddySummonPoints.Add(r);
                        break;
                    case Region.RollingAssetGeneration r:
                        RollingAssetGenerations.Add(r);
                        break;
                    case Region.MufflingPlane r:
                        MufflingPlanes.Add(r);
                        break;
                    case Region.ElectroMagneticStorm r:
                        ElectroMagneticStorms.Add(r);
                        break;
                    case Region.AiInformationSharing r:
                        AiInformationSharings.Add(r);
                        break;
                    case Region.WaveSimulation r:
                        WaveSimulations.Add(r);
                        break;
                    case Region.Cover r:
                        Covers.Add(r);
                        break;
                    case Region.MissionPlacement r:
                        MissionPlacements.Add(r);
                        break;
                    case Region.NaviVolumeResolution r:
                        NaviVolumeResolutions.Add(r);
                        break;
                    case Region.MiniArea r:
                        MiniAreas.Add(r);
                        break;
                    case Region.ConnectionBorder r:
                        ConnectionBorders.Add(r);
                        break;
                    case Region.NaviGeneration r:
                        NaviGenerations.Add(r);
                        break;
                    case Region.TopdownView r:
                        TopdownViews.Add(r);
                        break;
                    case Region.NaviCvCancel r:
                        NaviCvCancels.Add(r);
                        break;
                    case Region.JumpEdgeRestriction r:
                        JumpEdgeRestrictions.Add(r);
                        break;
                    case Region.Other r:
                        Others.Add(r);
                        break;
                    case Region.None r:
                        Nones.Add(r);
                        break;

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
                    EntryPoints, EnvMapPoints, Sounds, SFX, WindSFX,
                    EnvMapEffectBoxes, MufflingBoxes,
                    MufflingPortals, SoundOverrides, Patrols,
                    FeMapDisplays, OperationalAreas, AiTargets, WwiseEnvironmentSounds,
                    CharacterFollowings, NavmeshCostControls, ArenaControls,
                    ArenaAppearances, GarageCameras, CutscenePlaybacks, FallPreventionWallRemovals, BigJumps,

                    Unknown_3s, Unknown_7s, ReturnPoints, Messages, Unknown_10s, Unknown_11s, Unknown_12s, FallReturnPoints,
                    Unknown_14s, Unknown_15s, Unknown_16s, WindPlacements, Unknown_19s, Unknown_20s, Connections,
                    SourceWaypoints, StaticWaypoints, MapGridLayerConnections, EnemySpawnPoints, BuddySummonPoints, RollingAssetGenerations,
                    MufflingPlanes, ElectroMagneticStorms, AiInformationSharings, WaveSimulations, Covers, MissionPlacements,
                    NaviVolumeResolutions, MiniAreas, ConnectionBorders, NaviGenerations, TopdownViews, NaviCvCancels, JumpEdgeRestrictions,

                    Others, Nones
                );
            }
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br, long offsetLength)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 8);
                switch (type)
                {
                    case RegionType.EntryPoint:
                        return EntryPoints.EchoAdd(new Region.EntryPoint(br));

                    case RegionType.EnvMapPoint:
                        return EnvMapPoints.EchoAdd(new Region.EnvMapPoint(br, ParamVersion));

                    case RegionType.Sound:
                        return Sounds.EchoAdd(new Region.Sound(br));

                    case RegionType.SFX:
                        return SFX.EchoAdd(new Region.SFX(br));

                    case RegionType.WindSFX:
                        return WindSFX.EchoAdd(new Region.WindSFX(br));

                    case RegionType.EnvMapEffectBox:
                        return EnvMapEffectBoxes.EchoAdd(new Region.EnvMapEffectBox(br));

                    case RegionType.MufflingBox:
                        return MufflingBoxes.EchoAdd(new Region.MufflingBox(br));

                    case RegionType.MufflingPortal:
                        return MufflingPortals.EchoAdd(new Region.MufflingPortal(br));

                    case RegionType.SoundOverride:
                        return SoundOverrides.EchoAdd(new Region.SoundOverride(br));

                    case RegionType.Patrol:
                        return Patrols.EchoAdd(new Region.Patrol(br));

                    case RegionType.FeMapDisplay:
                        return FeMapDisplays.EchoAdd(new Region.FeMapDisplay(br));

                    case RegionType.OperationalArea:
                        return OperationalAreas.EchoAdd(new Region.OperationalArea(br));

                    case RegionType.AiTarget:
                        return AiTargets.EchoAdd(new Region.AiTarget(br));

                    case RegionType.WwiseEnvironmentSound:
                        return WwiseEnvironmentSounds.EchoAdd(new Region.WwiseEnvironmentSound(br));

                    case RegionType.CharacterFollowing:
                        return CharacterFollowings.EchoAdd(new Region.CharacterFollowing(br));

                    case RegionType.NavmeshCostControl:
                        return NavmeshCostControls.EchoAdd(new Region.NavmeshCostControl(br));

                    case RegionType.ArenaControl:
                        return ArenaControls.EchoAdd(new Region.ArenaControl(br));

                    case RegionType.ArenaAppearance:
                        return ArenaAppearances.EchoAdd(new Region.ArenaAppearance(br));

                    case RegionType.GarageCamera:
                        return GarageCameras.EchoAdd(new Region.GarageCamera(br));

                    case RegionType.CutscenePlayback:
                        return CutscenePlaybacks.EchoAdd(new Region.CutscenePlayback(br));

                    case RegionType.FallPreventionWallRemoval:
                        return FallPreventionWallRemovals.EchoAdd(new Region.FallPreventionWallRemoval(br));

                    case RegionType.BigJump:
                        return BigJumps.EchoAdd(new Region.BigJump(br));

                    case RegionType.Unknown_3:
                        return Unknown_3s.EchoAdd(new Region.Unknown_3(br, offsetLength));

                    case RegionType.Unknown_7:
                        return Unknown_7s.EchoAdd(new Region.Unknown_7(br, offsetLength));

                    case RegionType.ReturnPoint:
                        return ReturnPoints.EchoAdd(new Region.ReturnPoint(br, offsetLength));

                    case RegionType.Message:
                        return Messages.EchoAdd(new Region.Message(br, offsetLength));

                    case RegionType.Unknown_10:
                        return Unknown_10s.EchoAdd(new Region.Unknown_10(br, offsetLength));

                    case RegionType.Unknown_11:
                        return Unknown_11s.EchoAdd(new Region.Unknown_11(br, offsetLength));

                    case RegionType.Unknown_12:
                        return Unknown_12s.EchoAdd(new Region.Unknown_12(br, offsetLength));

                    case RegionType.FallReturnPoint:
                        return FallReturnPoints.EchoAdd(new Region.FallReturnPoint(br, offsetLength));

                    case RegionType.Unknown_14:
                        return Unknown_14s.EchoAdd(new Region.Unknown_14(br, offsetLength));

                    case RegionType.Unknown_15:
                        return Unknown_15s.EchoAdd(new Region.Unknown_15(br, offsetLength));

                    case RegionType.Unknown_16:
                        return Unknown_16s.EchoAdd(new Region.Unknown_16(br, offsetLength));

                    case RegionType.WindPlacement:
                        return WindPlacements.EchoAdd(new Region.WindPlacement(br, offsetLength));

                    case RegionType.Unknown_19:
                        return Unknown_19s.EchoAdd(new Region.Unknown_19(br, offsetLength));

                    case RegionType.Unknown_20:
                        return Unknown_20s.EchoAdd(new Region.Unknown_20(br, offsetLength));

                    case RegionType.Connection:
                        return Connections.EchoAdd(new Region.Connection(br, offsetLength));

                    case RegionType.SourceWaypoint:
                        return SourceWaypoints.EchoAdd(new Region.SourceWaypoint(br, offsetLength));

                    case RegionType.StaticWaypoint:
                        return StaticWaypoints.EchoAdd(new Region.StaticWaypoint(br, offsetLength));

                    case RegionType.MapGridLayerConnection:
                        return MapGridLayerConnections.EchoAdd(new Region.MapGridLayerConnection(br, offsetLength));

                    case RegionType.EnemySpawnPoint:
                        return EnemySpawnPoints.EchoAdd(new Region.EnemySpawnPoint(br, offsetLength));

                    case RegionType.BuddySummonPoint:
                        return BuddySummonPoints.EchoAdd(new Region.BuddySummonPoint(br, offsetLength));

                    case RegionType.RollingAssetGeneration:
                        return RollingAssetGenerations.EchoAdd(new Region.RollingAssetGeneration(br, offsetLength));

                    case RegionType.MufflingPlane:
                        return MufflingPlanes.EchoAdd(new Region.MufflingPlane(br, offsetLength));

                    case RegionType.ElectroMagneticStorm:
                        return ElectroMagneticStorms.EchoAdd(new Region.ElectroMagneticStorm(br, offsetLength));

                    case RegionType.AiInformationSharing:
                        return AiInformationSharings.EchoAdd(new Region.AiInformationSharing(br, offsetLength));

                    case RegionType.WaveSimulation:
                        return WaveSimulations.EchoAdd(new Region.WaveSimulation(br, offsetLength));

                    case RegionType.Cover:
                        return Covers.EchoAdd(new Region.Cover(br, offsetLength));

                    case RegionType.MissionPlacement:
                        return MissionPlacements.EchoAdd(new Region.MissionPlacement(br, offsetLength));

                    case RegionType.NaviVolumeResolution:
                        return NaviVolumeResolutions.EchoAdd(new Region.NaviVolumeResolution(br, offsetLength));

                    case RegionType.MiniArea:
                        return MiniAreas.EchoAdd(new Region.MiniArea(br, offsetLength));

                    case RegionType.ConnectionBorder:
                        return ConnectionBorders.EchoAdd(new Region.ConnectionBorder(br, offsetLength));

                    case RegionType.NaviGeneration:
                        return NaviGenerations.EchoAdd(new Region.NaviGeneration(br, offsetLength));

                    case RegionType.TopdownView:
                        return TopdownViews.EchoAdd(new Region.TopdownView(br, offsetLength));

                    case RegionType.NaviCvCancel:
                        return NaviCvCancels.EchoAdd(new Region.NaviCvCancel(br, offsetLength));

                    case RegionType.JumpEdgeRestriction:
                        return JumpEdgeRestrictions.EchoAdd(new Region.JumpEdgeRestriction(br, offsetLength));

                    case RegionType.Other:
                        return Others.EchoAdd(new Region.Other(br, offsetLength));

                    case RegionType.None:
                        return Nones.EchoAdd(new Region.None(br, offsetLength));

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

            // Index among points of the same type
            public int TypeIndex { get; set; }

            /// <summary>
            /// The shape of the region.
            /// </summary>
            public MSB.Shape Shape { get; set; }

            /// <summary>
            /// The location of the region.
            /// </summary>
            [PositionProperty]
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotiation of the region, in degrees.
            /// </summary>
            [RotationProperty]
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk2C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long parentListOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long childListOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk78 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int Unk7C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> ParentListIndices { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<short> ChildListIndices { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long NameOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long FormOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long CommonOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long TypeOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long Struct98Offset { get; set; }


            /// <summary>
            /// Unknown.
            /// </summary>
            /// /// <summary>
            /// If specified, the region is only active when the part is loaded.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Part))]
            public string ActivationPartName { get; set; }
            public int ActivationPartIndex { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public uint EntityID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public sbyte UnkC08 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkC0C { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkC10 { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public int UnkC28 { get; set; }

            private protected Region(string name)
            {
                Name = name;
                Shape = new MSB.Shape.Point();
                Unk2C = -1;
                ParentListIndices = new List<short>();
                ChildListIndices = new List<short>();
                Unk78 = -1;
                Unk7C = -1;
            }

            /// <summary>
            /// Creates a deep copy of the region.
            /// </summary>
            public Region DeepCopy()
            {
                var region = (Region)MemberwiseClone();
                region.Shape = Shape.DeepCopy();
                DeepCopyTo(region);
                return region;
            }
            IMsbRegion IMsbRegion.DeepCopy() => DeepCopy();

            private protected virtual void DeepCopyTo(Region region) { }

            private protected Region(BinaryReaderEx br)
            {
                long start = br.Position;
                NameOffset = br.ReadInt64();
                br.AssertInt32((int)Type);
                TypeIndex = br.ReadInt32();
                MSB.ShapeType shapeType = br.ReadEnum32<MSB.ShapeType>();
                Shape = MSB.Shape.Create(shapeType);
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                Unk2C = br.ReadInt32();

                parentListOffset = br.ReadInt64();
                childListOffset = br.ReadInt64();

                Unk78 = br.ReadInt32();
                Unk7C = br.ReadInt32();

                FormOffset = br.ReadInt64();
                CommonOffset = br.ReadInt64();
                TypeOffset = br.ReadInt64();
                Struct98Offset = br.ReadInt64();

                // Name
                Name = br.GetUTF16(start + NameOffset);

                // Point Indices 30
                br.Position = start + parentListOffset;
                short countA = br.ReadInt16();
                ParentListIndices = new List<short>(br.ReadInt16s(countA));

                // Point Indices 38
                br.Position = start + childListOffset;
                short countB = br.ReadInt16();
                ChildListIndices = new List<short>(br.ReadInt16s(countB));

                // Shape
                if (Shape.HasShapeData && FormOffset != 0L)
                {
                    br.Position = start + FormOffset;
                    Shape.ReadShapeData(br);
                }

                // Common
                br.Position = start + CommonOffset;

                ActivationPartIndex = br.ReadInt32();
                EntityID = br.ReadUInt32();
                UnkC08 = br.ReadSByte();
                br.AssertByte(new byte[1]);
                br.AssertInt16((short)-1);
                UnkC0C = br.ReadInt32();
                UnkC10 = br.ReadInt32();
                br.AssertInt32(new int[1]);
                br.AssertInt32(new int[1]);
                br.AssertInt32(new int[1]);
                br.AssertInt32(new int[1]);
                br.AssertInt32(new int[1]);
                UnkC28 = br.ReadInt32();
                br.AssertInt32(new int[1]);

                // Type
                if (HasTypeData && TypeOffset != 0L)
                {
                    br.Position = start + TypeOffset;
                    ReadTypeData(br);
                }

                if(Type == RegionType.Other)
                {
                    long otherSize = Struct98Offset - TypeOffset;
                    otherStuff = br.ReadBytes((int)otherSize);
                }

                // Struct98 Offset
                br.Position = start + Struct98Offset;
                br.AssertInt32(-1);
                br.AssertInt32(new int[1]);
                br.AssertInt32(-1);
            }

            private byte[] otherStuff;

            private protected virtual void ReadTypeData(BinaryReaderEx br)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal override void Write(BinaryWriterEx bw, int id)
            {
                long start = bw.Position;

                bw.ReserveInt64("NameOffset");
                bw.WriteInt32((int) Type);
                bw.WriteInt32(TypeIndex);
                bw.WriteUInt32((uint)Shape.Type);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk2C);

                bw.ReserveInt64("IndexListOffset30");
                bw.ReserveInt64("IndexListOffset38");

                bw.WriteInt32(Unk78);
                bw.WriteInt32(Unk7C);

                bw.ReserveInt64("FormOffset");
                bw.ReserveInt64("CommonOffset");
                bw.ReserveInt64("TypeOffset");
                bw.ReserveInt64("Struct98Offset");

                bw.FillInt64("NameOffset", bw.Position - start);
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
                bw.Pad(4);

                bw.FillInt64("IndexListOffset30", bw.Position - start);
                bw.WriteInt16((short)ParentListIndices.Count);
                bw.WriteInt16s(ParentListIndices);
                bw.Pad(4);

                bw.FillInt64("IndexListOffset38", bw.Position - start);
                bw.WriteInt16((short)ChildListIndices.Count);
                bw.WriteInt16s(ChildListIndices);
                bw.Pad(8);

                if (Shape.HasShapeData && FormOffset != 0L)
                {
                    bw.FillInt64("FormOffset", bw.Position - start);
                    Shape.WriteShapeData(bw);
                }
                else
                {
                    bw.FillInt64("FormOffset", 0L);
                }

                bw.FillInt64("CommonOffset", bw.Position - start);
                bw.WriteInt32(ActivationPartIndex);
                bw.WriteUInt32(EntityID);
                bw.WriteSByte(UnkC08);
                bw.WriteByte((byte)0);
                bw.WriteInt16((short)-1);
                bw.WriteInt32(UnkC0C);
                bw.WriteInt32(UnkC10);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(UnkC28);
                bw.WriteInt32(0);

                if (Type >= RegionType.MufflingBox || Type == RegionType.Other)
                {
                    bw.Pad(8);
                }

                if (HasTypeData && TypeOffset != 0L)
                {
                    bw.FillInt64("TypeOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeOffset", 0L);
                }

                if (Type == RegionType.Other)
                {
                    bw.WriteBytes(otherStuff);
                }

                if (Type <= RegionType.MufflingBox && Type != RegionType.Other)
                {
                    bw.Pad(8);
                }

                bw.FillInt64("Struct98Offset", bw.Position - start);
                bw.WriteInt32(-1);
                bw.WriteInt32(0);
                bw.WriteInt32(-1);
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
            public class EntryPoint : Region
            {
                private protected override RegionType Type => RegionType.EntryPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Not sure what this does.
                /// </summary>
                public int Priority { get; set; }

                /// <summary>
                /// Creates an InvasionPoint with default values.
                /// </summary>
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
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class EnvMapPoint : Region
            {
                private int version;
                private protected override RegionType Type => RegionType.EnvMapPoint;
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
                public byte UnkT0C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0D { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0F { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT20 { get; set; }

                /// <summary>
                /// Creates an EnvMapPoint with default values.
                /// </summary>
                public EnvMapPoint() : base($"{nameof(Region)}: {nameof(EnvMapPoint)}")
                {
                    UnkT00 = 1000f;
                    UnkT04 = 4;
                    UnkT0C = (byte)1;
                    UnkT0D = (byte)1;
                    UnkT0F = (byte)1;
                    UnkT10 = 1f;
                }

                internal EnvMapPoint(BinaryReaderEx br, int _version) : base(br) 
                {
                    version = _version;
                }

                private protected override void DeepCopyTo(Region region)
                {
                    var point = (EnvMapPoint)region;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT0C = br.ReadByte();
                    UnkT0D = br.ReadByte();
                    br.AssertByte((byte)1);
                    UnkT0F = br.ReadByte();
                    UnkT10 = br.ReadSingle();
                    br.AssertSingle(1f);
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();

                    if (version < 52)
                        return;

                    UnkT20 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteByte(UnkT0C);
                    bw.WriteByte(UnkT0D);
                    bw.WriteByte((byte)1);
                    bw.WriteByte(UnkT0F);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(1f);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);

                    if (version < 52)
                        return;

                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(0);
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
                public string[] ChildRegionNames { get; private set; }
                public int[] ChildRegionIndices;

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
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32s(ChildRegionIndices);
                    bw.WriteInt32(0);
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
                [MSBAliasEnum(AliasEnumType = "PARTICLES")]
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
                [MSBAliasEnum(AliasEnumType = "PARTICLES")]
                public int EffectID { get; set; }

                /// <summary>
                /// Reference to a WindArea region.
                /// </summary>
                [MSBReference(ReferenceType = typeof(Region))]
                public string WindAreaName { get; set; }
                public int WindAreaIndex;

                /// <summary>
                /// Creates a WindSFX with default values.
                /// </summary>
                public WindSFX() : base($"{nameof(Region)}: {nameof(WindSFX)}") { }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    WindAreaIndex = br.ReadInt32();
                    br.AssertSingle(-1f);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(WindAreaIndex);
                    bw.WriteSingle(-1f);
                    bw.WriteInt32(0);
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
            /// Unknown.
            /// </summary>
            public class EnvMapEffectBox : Region
            {
                private protected override RegionType Type => RegionType.EnvMapEffectBox;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Distance from camera required before enabling envmap. 0 = always enabled.
                /// </summary>
                public float EnableDist { get; set; }

                /// <summary>
                /// Distance it takes for an envmap to fully transition into view.
                /// </summary>
                public float TransitionDist { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT09 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0A { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT0B { get; set; }

                /// <summary>
                /// Strength of specular light in region.
                /// </summary>
                public float SpecularLightMult { get; set; }

                /// <summary>
                /// Strength of direct light emitting from EnvironmentMapPoint.
                /// </summary>
                public float PointLightMult { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT2C { get; set; }

                /// <summary>
                /// Affects lighting with other fields when true. Possibly normalizes light when false.
                /// </summary>
                public byte IsModifyLight { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT2F { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT30 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public short UnkT32 { get; set; }

                /// <summary>
                /// Creates an EnvMapEffectBox with default values.
                /// </summary>
                public EnvMapEffectBox() : base($"{nameof(Region)}: {nameof(EnvMapEffectBox)}") 
                {
                    SpecularLightMult = 1f;
                    PointLightMult = 1f;
                    IsModifyLight = (byte)1;
                    UnkT30 = (short)-1;
                    UnkT32 = (short)-1;
                }

                internal EnvMapEffectBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EnableDist = br.ReadSingle();
                    TransitionDist = br.ReadSingle();
                    br.AssertByte(new byte[1]);
                    UnkT09 = br.ReadByte();
                    UnkT0A = br.ReadByte();
                    UnkT0B = br.ReadByte();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    SpecularLightMult = br.ReadSingle();
                    PointLightMult = br.ReadSingle();
                    UnkT2C = br.ReadInt16();
                    IsModifyLight = br.ReadByte();
                    UnkT2F = br.ReadByte();
                    UnkT30 = br.ReadInt16();
                    UnkT32 = br.ReadInt16();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(EnableDist);
                    bw.WriteSingle(TransitionDist);
                    bw.WriteByte((byte)0);
                    bw.WriteByte(UnkT09);
                    bw.WriteByte(UnkT0A);
                    bw.WriteByte(UnkT0B);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteSingle(SpecularLightMult);
                    bw.WriteSingle(PointLightMult);
                    bw.WriteInt16(UnkT2C);
                    bw.WriteByte(IsModifyLight);
                    bw.WriteByte(UnkT2F);
                    bw.WriteInt16(UnkT30);
                    bw.WriteInt16(UnkT32);
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
                public int UnkT20 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT24 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT28 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT2C { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT34 { get; set; }

                /// <summary>
                /// Creates a MufflingBox with default values.
                /// </summary>
                public MufflingBox() : base($"{nameof(Region)}: {nameof(MufflingBox)}") { }

                internal MufflingBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt64(32L);
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadSingle();
                    UnkT28 = br.ReadSingle();
                    UnkT2C = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    UnkT34 = br.ReadSingle();
                    br.AssertInt32(new int[1]);
                    br.AssertSingle(-1f);
                    br.AssertSingle(-1f);
                    br.AssertSingle(-1f);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt64(32L);
                    bw.WriteInt32(UnkT20);
                    bw.WriteSingle(UnkT24);
                    bw.WriteSingle(UnkT28);
                    bw.WriteInt32(UnkT2C);
                    bw.WriteInt32(0);
                    bw.WriteSingle(UnkT34);
                    bw.WriteInt32(0);
                    bw.WriteSingle(-1f);
                    bw.WriteSingle(-1f);
                    bw.WriteSingle(-1f);
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
                /// Creates a MufflingPortal with default values.
                /// </summary>
                public MufflingPortal() : base($"{nameof(Region)}: {nameof(MufflingPortal)}") { }

                internal MufflingPortal(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt64(32L);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(-1);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt64(32L);
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
            public class SoundOverride : Region
            {
                private protected override RegionType Type => RegionType.SoundOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte UnkT03 { get; set; }

                /// <summary>
                /// Creates a SoundOverride with default values.
                /// </summary>
                public SoundOverride() : base($"{nameof(Region)}: {nameof(SoundOverride)}") { }

                internal SoundOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSByte();
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    UnkT03 = br.ReadSByte();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSByte(UnkT00);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                    bw.WriteSByte(UnkT03);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// A point along an NPC patrol path.
            /// </summary>
            public class Patrol : Region
            {
                private protected override RegionType Type => RegionType.Patrol;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public sbyte UnkT04 { get; set; }

                /// <summary>
                /// Creates a Patrol with default values.
                /// </summary>
                public Patrol() : base($"{nameof(Region)}: {nameof(Patrol)}") { }

                internal Patrol(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadSByte();
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteSByte(UnkT04);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class FeMapDisplay : Region
            {
                private protected override RegionType Type => RegionType.FeMapDisplay;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Unknown
                /// </summary>
                public byte UnkT02 { get; set; }

                /// <summary>
                /// Unknown
                /// </summary>
                public byte UnkT03 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a MapPoint with default values.
                /// </summary>
                public FeMapDisplay() : base($"{nameof(Region)}: {nameof(FeMapDisplay)}") { }

                internal FeMapDisplay(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    UnkT02 = br.ReadByte();
                    UnkT03 = br.ReadByte();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteByte(UnkT02);
                    bw.WriteByte(UnkT03);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class OperationalArea : Region
            {
                private protected override RegionType Type => RegionType.OperationalArea;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

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
                public float UnkT10 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT14 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT18 { get; set; }

                /// <summary>
                /// Creates a OperationalArea with default values.
                /// </summary>
                public OperationalArea() : base($"{nameof(Region)}: {nameof(OperationalArea)}") { }

                internal OperationalArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadSingle();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteInt32(UnkT18);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class AiTarget : Region
            {
                private protected override RegionType Type => RegionType.AiTarget;
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
                public int UnkT04 { get; set; }

                /// <summary>
                /// Creates a AiTarget with default values.
                /// </summary>
                public AiTarget() : base($"{nameof(Region)}: {nameof(AiTarget)}") { }

                internal AiTarget(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    br.AssertInt16(new short[1]);
                    UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteInt16((short)0);
                    bw.WriteInt32(UnkT04);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WwiseEnvironmentSound : Region
            {
                private protected override RegionType Type => RegionType.WwiseEnvironmentSound;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown. May be: WwiseValuetoStrParam_RuntimeReflectTextType or WwiseValuetoStrParam_Material
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Creates an WwiseEnvironmentSound with default values.
                /// </summary>
                public WwiseEnvironmentSound() : base($"{nameof(Region)}: {nameof(WwiseEnvironmentSound)}") { }

                internal WwiseEnvironmentSound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    br.AssertInt16(new short[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WriteInt16((short)0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class CharacterFollowing : Region
            {
                private protected override RegionType Type => RegionType.CharacterFollowing;
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
                public float UnkT08 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT0C { get; set; }

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
                public float UnkT18 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT1C { get; set; }

                /// <summary>
                /// Creates an CharacterFollowing with default values.
                /// </summary>
                public CharacterFollowing() : base($"{nameof(Region)}: {nameof(CharacterFollowing)}") { }

                internal CharacterFollowing(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    UnkT04 = br.ReadInt32();
                    UnkT08 = br.ReadSingle();
                    UnkT0C = br.ReadSingle();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadSingle();
                    UnkT1C = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteSingle(UnkT08);
                    bw.WriteSingle(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteSingle(UnkT18);
                    bw.WriteSingle(UnkT1C);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class NavmeshCostControl : Region
            {
                private protected override RegionType Type => RegionType.NavmeshCostControl;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown. Probably Navigation Weighting?
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates an NavmeshCostControl with default values.
                /// </summary>
                public NavmeshCostControl() : base($"{nameof(Region)}: {nameof(NavmeshCostControl)}") { }

                internal NavmeshCostControl(BinaryReaderEx br) : base(br) { }

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
            public class ArenaControl : Region
            {
                private protected override RegionType Type => RegionType.ArenaControl;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an ArenaControl with default values.
                /// </summary>
                public ArenaControl() : base($"{nameof(Region)}: {nameof(ArenaControl)}") { }

                internal ArenaControl(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class ArenaAppearance : Region
            {
                private protected override RegionType Type => RegionType.ArenaAppearance;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an ArenaAppearance with default values.
                /// </summary>
                public ArenaAppearance() : base($"{nameof(Region)}: {nameof(ArenaAppearance)}") { }

                internal ArenaAppearance(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class GarageCamera : Region
            {
                private protected override RegionType Type => RegionType.GarageCamera;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT00 { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT04 { get; set; }

                /// <summary>
                /// Creates an GarageCamera with default values.
                /// </summary>
                public GarageCamera() : base($"{nameof(Region)}: {nameof(GarageCamera)}") 
                {
                    UnkT00 = -1f;
                }

                internal GarageCamera(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadSingle();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteSingle(UnkT04);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class CutscenePlayback : Region
            {
                private protected override RegionType Type => RegionType.CutscenePlayback;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public int UnkT00 { get; set; }

                /// <summary>
                /// Creates an CutscenePlayback with default values.
                /// </summary>
                public CutscenePlayback() : base($"{nameof(Region)}: {nameof(CutscenePlayback)}") { }

                internal CutscenePlayback(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
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
            public class FallPreventionWallRemoval : Region
            {
                private protected override RegionType Type => RegionType.FallPreventionWallRemoval;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an FallPreventionWallRemoval with default values.
                /// </summary>
                public FallPreventionWallRemoval() : base($"{nameof(Region)}: {nameof(FallPreventionWallRemoval)}") { }

                internal FallPreventionWallRemoval(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
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
            public class BigJump : Region
            {
                private protected override RegionType Type => RegionType.BigJump;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                [MSBParamReference(ParamName = "JumpSpecifyAltParam")]
                public int JumpSpecifyAltParamID { get; set; }

                /// <summary>
                /// Creates an BigJump with default values.
                /// </summary>
                public BigJump() : base($"{nameof(Region)}: {nameof(BigJump)}") { }

                internal BigJump(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    JumpSpecifyAltParamID = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(JumpSpecifyAltParamID);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_3 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_3;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_3() : base($"{nameof(Region)}: {nameof(Unknown_3)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_3(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_7 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_7;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_7() : base($"{nameof(Region)}: {nameof(Unknown_7)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_7(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class ReturnPoint : Region
            {
                private protected override RegionType Type => RegionType.ReturnPoint;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public ReturnPoint() : base($"{nameof(Region)}: {nameof(ReturnPoint)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal ReturnPoint(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Message : Region
            {
                private protected override RegionType Type => RegionType.Message;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Message() : base($"{nameof(Region)}: {nameof(Message)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Message(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_10 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_10;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_10() : base($"{nameof(Region)}: {nameof(Unknown_10)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_10(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_11 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_11;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_11() : base($"{nameof(Region)}: {nameof(Unknown_11)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_11(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_12 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_12;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_12() : base($"{nameof(Region)}: {nameof(Unknown_12)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_12(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class FallReturnPoint : Region
            {
                private protected override RegionType Type => RegionType.FallReturnPoint;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public FallReturnPoint() : base($"{nameof(Region)}: {nameof(FallReturnPoint)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal FallReturnPoint(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_14 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_14;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_14() : base($"{nameof(Region)}: {nameof(Unknown_14)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_14(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_15 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_15;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_15() : base($"{nameof(Region)}: {nameof(Unknown_15)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_15(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_16 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_16;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_16() : base($"{nameof(Region)}: {nameof(Unknown_16)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_16(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WindPlacement : Region
            {
                private protected override RegionType Type => RegionType.WindPlacement;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public WindPlacement() : base($"{nameof(Region)}: {nameof(WindPlacement)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal WindPlacement(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_19 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_19;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_19() : base($"{nameof(Region)}: {nameof(Unknown_19)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_19(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Unknown_20 : Region
            {
                private protected override RegionType Type => RegionType.Unknown_20;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Unknown_20() : base($"{nameof(Region)}: {nameof(Unknown_20)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Unknown_20(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Connection : Region
            {
                private protected override RegionType Type => RegionType.Connection;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Connection() : base($"{nameof(Region)}: {nameof(Connection)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Connection(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class SourceWaypoint : Region
            {
                private protected override RegionType Type => RegionType.SourceWaypoint;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public SourceWaypoint() : base($"{nameof(Region)}: {nameof(SourceWaypoint)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal SourceWaypoint(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class StaticWaypoint : Region
            {
                private protected override RegionType Type => RegionType.StaticWaypoint;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public StaticWaypoint() : base($"{nameof(Region)}: {nameof(StaticWaypoint)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal StaticWaypoint(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MapGridLayerConnection : Region
            {
                private protected override RegionType Type => RegionType.MapGridLayerConnection;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public MapGridLayerConnection() : base($"{nameof(Region)}: {nameof(MapGridLayerConnection)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal MapGridLayerConnection(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class EnemySpawnPoint : Region
            {
                private protected override RegionType Type => RegionType.EnemySpawnPoint;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                public byte[] Bytes { get; set; }

                public EnemySpawnPoint() : base($"{nameof(Region)}: {nameof(EnemySpawnPoint)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal EnemySpawnPoint(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class BuddySummonPoint : Region
            {
                private protected override RegionType Type => RegionType.BuddySummonPoint;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public BuddySummonPoint() : base($"{nameof(Region)}: {nameof(BuddySummonPoint)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal BuddySummonPoint(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class RollingAssetGeneration : Region
            {
                private protected override RegionType Type => RegionType.RollingAssetGeneration;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public RollingAssetGeneration() : base($"{nameof(Region)}: {nameof(RollingAssetGeneration)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal RollingAssetGeneration(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MufflingPlane : Region
            {
                private protected override RegionType Type => RegionType.MufflingPlane;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public MufflingPlane() : base($"{nameof(Region)}: {nameof(MufflingPlane)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal MufflingPlane(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class ElectroMagneticStorm : Region
            {
                private protected override RegionType Type => RegionType.ElectroMagneticStorm;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public ElectroMagneticStorm() : base($"{nameof(Region)}: {nameof(ElectroMagneticStorm)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal ElectroMagneticStorm(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class AiInformationSharing : Region
            {
                private protected override RegionType Type => RegionType.AiInformationSharing;
                private protected override bool HasTypeData => true;

                private byte[] Bytes { get; set; }

                private long Length { get; set; }
                public AiInformationSharing() : base($"{nameof(Region)}: {nameof(AiInformationSharing)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal AiInformationSharing(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class WaveSimulation : Region
            {
                private protected override RegionType Type => RegionType.WaveSimulation;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public WaveSimulation() : base($"{nameof(Region)}: {nameof(WaveSimulation)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal WaveSimulation(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Cover : Region
            {
                private protected override RegionType Type => RegionType.Cover;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public Cover() : base($"{nameof(Region)}: {nameof(Cover)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Cover(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MissionPlacement : Region
            {
                private protected override RegionType Type => RegionType.MissionPlacement;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public MissionPlacement() : base($"{nameof(Region)}: {nameof(MissionPlacement)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal MissionPlacement(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class NaviVolumeResolution : Region
            {
                private protected override RegionType Type => RegionType.NaviVolumeResolution;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public NaviVolumeResolution() : base($"{nameof(Region)}: {nameof(NaviVolumeResolution)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal NaviVolumeResolution(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class MiniArea : Region
            {
                private protected override RegionType Type => RegionType.MiniArea;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public MiniArea() : base($"{nameof(Region)}: {nameof(MiniArea)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal MiniArea(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class ConnectionBorder : Region
            {
                private protected override RegionType Type => RegionType.ConnectionBorder;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public ConnectionBorder() : base($"{nameof(Region)}: {nameof(ConnectionBorder)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal ConnectionBorder(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class NaviGeneration : Region
            {
                private protected override RegionType Type => RegionType.NaviGeneration;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public NaviGeneration() : base($"{nameof(Region)}: {nameof(NaviGeneration)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal NaviGeneration(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class TopdownView : Region
            {
                private protected override RegionType Type => RegionType.TopdownView;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public TopdownView() : base($"{nameof(Region)}: {nameof(TopdownView)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal TopdownView(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class NaviCvCancel : Region
            {
                private protected override RegionType Type => RegionType.NaviCvCancel;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public NaviCvCancel() : base($"{nameof(Region)}: {nameof(NaviCvCancel)}")
                {
                    Bytes = Array.Empty<byte>();
                }
                internal NaviCvCancel(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }
                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class JumpEdgeRestriction : Region
            {
                private protected override RegionType Type => RegionType.JumpEdgeRestriction;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                public JumpEdgeRestriction() : base($"{nameof(Region)}: {nameof(JumpEdgeRestriction)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal JumpEdgeRestriction(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Other.
            /// </summary>
            public class Other : Region
            {
                private protected override RegionType Type => RegionType.Other;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Region)}: {nameof(Other)}") 
                {
                    Bytes = Array.Empty<byte>();
                }

                internal Other(BinaryReaderEx br, long _length) : base(br) 
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }

            /// <summary>
            /// Unknown
            /// </summary>
            public class None : Region
            {
                private protected override RegionType Type => RegionType.None;
                private protected override bool HasTypeData => true;

                private long Length { get; set; }
                private byte[] Bytes { get; set; }

                /// <summary>
                /// Creates an None with default values.
                /// </summary>
                public None() : base($"{nameof(Region)}: {nameof(None)}")
                {
                    Bytes = Array.Empty<byte>();
                }

                internal None(BinaryReaderEx br, long _length) : base(br)
                {
                    Length = _length;
                }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    Bytes = br.ReadBytes((int)Length);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteBytes(Bytes);
                }
            }
        }
    }
}
