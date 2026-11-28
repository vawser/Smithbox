using System;
using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class MSB_AC6
    {
        public enum RegionType : int
        {
            EntryPoint = 1,
            EnvMapPoint = 2,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            EnvMapEffectBox = 17,
            WindPlacement = 18,
            MufflingBox = 28,
            MufflingPortal = 29,
            SoundOverride = 30,
            Patrol = 32,
            FeMapDisplay = 33,
            OperationalArea = 35,
            AiInformationSharing = 36,
            AiTarget = 37,
            WwiseEnvironmentSound = 39,
            NaviGeneration = 45,
            TopdownView = 46,
            CharacterFollowing = 47,
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
            public int ParamVersion;

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
            /// Unknown.
            /// </summary>
            public List<Region.WindPlacement> WindPlacements { get; set; }

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
            public List<Region.AiInformationSharing> AiInformationSharings { get; set; }

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
            public List<Region.NaviGeneration> NaviGenerations { get; set; }

            /// <summary>
            /// Top-down map image capture areas (offline tool data; not read by the game).
            /// </summary>
            public List<Region.TopdownView> TopdownViews { get; set; }

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
            public List<Region.JumpEdgeRestriction> JumpEdgeRestrictions { get; set; }

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
            /// Creates an empty PointParam with the default version.
            /// </summary>
            public PointParam() : base(52, "POINT_PARAM_ST")
            {
                ParamVersion = base.Version;

                EntryPoints = new List<Region.EntryPoint>(); // 1
                EnvMapPoints = new List<Region.EnvMapPoint>(); // 2
                Sounds = new List<Region.Sound>(); // 4
                SFX = new List<Region.SFX>(); // 5
                WindSFX = new List<Region.WindSFX>(); // 6
                EnvMapEffectBoxes = new List<Region.EnvMapEffectBox>(); // 17
                WindPlacements = new List<Region.WindPlacement>(); // 18
                MufflingBoxes = new List<Region.MufflingBox>(); // 28
                MufflingPortals = new List<Region.MufflingPortal>(); // 29
                SoundOverrides = new List<Region.SoundOverride>(); // 30
                Patrols = new List<Region.Patrol>(); // 32
                FeMapDisplays = new List<Region.FeMapDisplay>(); // 33
                OperationalAreas = new List<Region.OperationalArea>(); // 35
                AiInformationSharings = new List<Region.AiInformationSharing>(); // 36
                AiTargets = new List<Region.AiTarget>(); // 37
                WwiseEnvironmentSounds = new List<Region.WwiseEnvironmentSound>(); // 39
                NaviGenerations = new List<Region.NaviGeneration>();  // 45
                TopdownViews = new List<Region.TopdownView>(); // 46
                CharacterFollowings = new List<Region.CharacterFollowing>(); // 47
                NavmeshCostControls = new List<Region.NavmeshCostControl>(); // 49
                ArenaControls = new List<Region.ArenaControl>(); // 50
                ArenaAppearances = new List<Region.ArenaAppearance>(); // 51
                GarageCameras = new List<Region.GarageCamera>(); // 52
                JumpEdgeRestrictions = new List<Region.JumpEdgeRestriction>(); // 53
                CutscenePlaybacks = new List<Region.CutscenePlayback>(); // 54
                FallPreventionWallRemovals = new List<Region.FallPreventionWallRemoval>(); // 55
                BigJumps = new List<Region.BigJump>(); // 56
                Others = new List<Region.Other>(); // -1
            }

            /// <summary>
            /// Adds a region to the appropriate list for its type; returns the region.
            /// </summary>
            public Region Add(Region region)
            {
                switch (region)
                {
                    // 1
                    case Region.EntryPoint r:
                        EntryPoints.Add(r);
                        break;
                    // 2
                    case Region.EnvMapPoint r:
                        EnvMapPoints.Add(r);
                        break;
                    // 4
                    case Region.Sound r:
                        Sounds.Add(r);
                        break;
                    // 5
                    case Region.SFX r:
                        SFX.Add(r);
                        break;
                    // 6
                    case Region.WindSFX r:
                        WindSFX.Add(r);
                        break;
                    // 17
                    case Region.EnvMapEffectBox r:
                        EnvMapEffectBoxes.Add(r);
                        break;
                    // 18
                    case Region.WindPlacement r:
                        WindPlacements.Add(r);
                        break;
                    // 28
                    case Region.MufflingBox r:
                        MufflingBoxes.Add(r);
                        break;
                    // 29
                    case Region.MufflingPortal r:
                        MufflingPortals.Add(r);
                        break;
                    // 30
                    case Region.SoundOverride r:
                        SoundOverrides.Add(r);
                        break;
                    // 32
                    case Region.Patrol r:
                        Patrols.Add(r);
                        break;
                    // 33
                    case Region.FeMapDisplay r:
                        FeMapDisplays.Add(r);
                        break;
                    // 35
                    case Region.OperationalArea r:
                        OperationalAreas.Add(r);
                        break;
                    // 36
                    case Region.AiInformationSharing r:
                        AiInformationSharings.Add(r);
                        break;
                    // 37
                    case Region.AiTarget r:
                        AiTargets.Add(r);
                        break;
                    // 39
                    case Region.WwiseEnvironmentSound r:
                        WwiseEnvironmentSounds.Add(r);
                        break;
                    // 45
                    case Region.NaviGeneration r:
                        NaviGenerations.Add(r);
                        break;
                    // 46
                    case Region.TopdownView r:
                        TopdownViews.Add(r);
                        break;
                    // 47
                    case Region.CharacterFollowing r:
                        CharacterFollowings.Add(r);
                        break;
                    // 49
                    case Region.NavmeshCostControl r:
                        NavmeshCostControls.Add(r);
                        break;
                    // 50
                    case Region.ArenaControl r:
                        ArenaControls.Add(r);
                        break;
                    // 51
                    case Region.ArenaAppearance r:
                        ArenaAppearances.Add(r);
                        break;
                    // 52
                    case Region.GarageCamera r:
                        GarageCameras.Add(r);
                        break;
                    // 53
                    case Region.JumpEdgeRestriction r:
                        JumpEdgeRestrictions.Add(r);
                        break;
                    // 54
                    case Region.CutscenePlayback r:
                        CutscenePlaybacks.Add(r);
                        break;
                    // 55
                    case Region.FallPreventionWallRemoval r:
                        FallPreventionWallRemovals.Add(r);
                        break;
                    // 56
                    case Region.BigJump r:
                        BigJumps.Add(r);
                        break;
                    // -1
                    case Region.Other r:
                        Others.Add(r);
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
                    EntryPoints, 
                    EnvMapPoints, 
                    Sounds, 
                    SFX, 
                    WindSFX,
                    EnvMapEffectBoxes,
                    WindPlacements,
                    MufflingBoxes,
                    MufflingPortals, 
                    SoundOverrides, 
                    Patrols,
                    FeMapDisplays, 
                    OperationalAreas,
                    AiInformationSharings,
                    AiTargets, 
                    WwiseEnvironmentSounds,
                    NaviGenerations, 
                    TopdownViews, 
                    CharacterFollowings, 
                    NavmeshCostControls, 
                    ArenaControls,
                    ArenaAppearances, 
                    GarageCameras, 
                    JumpEdgeRestrictions, 
                    CutscenePlaybacks, 
                    FallPreventionWallRemovals, 
                    BigJumps,
                    Others
                );
            }
            IReadOnlyList<IMsbRegion> IMsbParam<IMsbRegion>.GetEntries() => GetEntries();

            internal override Region ReadEntry(BinaryReaderEx br, long offsetLength)
            {
                RegionType type = br.GetEnum32<RegionType>(br.Position + 8);
                switch (type)
                {
                    // 1
                    case RegionType.EntryPoint:
                        return EntryPoints.EchoAdd(new Region.EntryPoint(br));

                    // 2
                    case RegionType.EnvMapPoint:
                        return EnvMapPoints.EchoAdd(new Region.EnvMapPoint(br));

                    // 4
                    case RegionType.Sound:
                        return Sounds.EchoAdd(new Region.Sound(br));

                    // 5
                    case RegionType.SFX:
                        return SFX.EchoAdd(new Region.SFX(br));

                    // 6
                    case RegionType.WindSFX:
                        return WindSFX.EchoAdd(new Region.WindSFX(br));

                    // 17
                    case RegionType.EnvMapEffectBox:
                        return EnvMapEffectBoxes.EchoAdd(new Region.EnvMapEffectBox(br));

                    // 18
                    case RegionType.WindPlacement:
                        return WindPlacements.EchoAdd(new Region.WindPlacement(br));

                    // 28
                    case RegionType.MufflingBox:
                        return MufflingBoxes.EchoAdd(new Region.MufflingBox(br));

                    // 29
                    case RegionType.MufflingPortal:
                        return MufflingPortals.EchoAdd(new Region.MufflingPortal(br));

                    // 30
                    case RegionType.SoundOverride:
                        return SoundOverrides.EchoAdd(new Region.SoundOverride(br));

                    // 32
                    case RegionType.Patrol:
                        return Patrols.EchoAdd(new Region.Patrol(br));

                    // 33
                    case RegionType.FeMapDisplay:
                        return FeMapDisplays.EchoAdd(new Region.FeMapDisplay(br));

                    // 35
                    case RegionType.OperationalArea:
                        return OperationalAreas.EchoAdd(new Region.OperationalArea(br));

                    // 36
                    case RegionType.AiInformationSharing:
                        return AiInformationSharings.EchoAdd(new Region.AiInformationSharing(br));

                    // 37
                    case RegionType.AiTarget:
                        return AiTargets.EchoAdd(new Region.AiTarget(br));

                    // 39
                    case RegionType.WwiseEnvironmentSound:
                        return WwiseEnvironmentSounds.EchoAdd(new Region.WwiseEnvironmentSound(br));

                    // 45
                    case RegionType.NaviGeneration:
                        return NaviGenerations.EchoAdd(new Region.NaviGeneration(br));

                    // 46
                    case RegionType.TopdownView:
                        return TopdownViews.EchoAdd(new Region.TopdownView(br));

                    // 47
                    case RegionType.CharacterFollowing:
                        return CharacterFollowings.EchoAdd(new Region.CharacterFollowing(br));

                    // 49
                    case RegionType.NavmeshCostControl:
                        return NavmeshCostControls.EchoAdd(new Region.NavmeshCostControl(br));

                    // 50
                    case RegionType.ArenaControl:
                        return ArenaControls.EchoAdd(new Region.ArenaControl(br));

                    // 51
                    case RegionType.ArenaAppearance:
                        return ArenaAppearances.EchoAdd(new Region.ArenaAppearance(br));

                    // 52
                    case RegionType.GarageCamera:
                        return GarageCameras.EchoAdd(new Region.GarageCamera(br));

                    // 53
                    case RegionType.JumpEdgeRestriction:
                        return JumpEdgeRestrictions.EchoAdd(new Region.JumpEdgeRestriction(br));

                    // 54
                    case RegionType.CutscenePlayback:
                        return CutscenePlaybacks.EchoAdd(new Region.CutscenePlayback(br));

                    // 55
                    case RegionType.FallPreventionWallRemoval:
                        return FallPreventionWallRemovals.EchoAdd(new Region.FallPreventionWallRemoval(br));

                    // 56
                    case RegionType.BigJump:
                        return BigJumps.EchoAdd(new Region.BigJump(br));

                    // -1
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

            // Index among points of the same type
            public int TypeIndex { get; set; }

            /// <summary>
            /// The shape of the region.
            /// </summary>
            public MSB.Shape Shape { get; set; }

            /// <summary>
            /// The location of the region.
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// The rotation of the region, in degrees.
            /// </summary>
            public Vector3 Rotation { get; set; }

            /// <summary>
            /// The region's ID: the game keeps the map's regions in a table keyed by it (140278D20) and takes the largest
            /// (141761480). As MSBE.Region.RegionID.
            /// </summary>
            public int RegionID { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long parentListOffset { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            private long childListOffset { get; set; }

            // Header +40/+44: no region getter reads them (MSBE reads +44 as MapStudioLayer; AC6 does not).
            private int Unk78 { get; set; }
            private int Unk7C { get; set; }

            /// <summary>
            /// Regions listed by the header +30 list (no read found; the +38 list is read). Empty in every retail MSB except
            /// the GarageCamera lanes, where it holds the reverse of ChildRegionNames (each point names the point before it).
            /// </summary>
            [MSBReference(ReferenceType = typeof(Region))]
            public string[] ParentRegionNames { get; set; }

            private short[] ParentListIndices { get; set; }

            /// <summary>
            /// Regions listed by the header +38 list, which the game reads; empty in every retail MSB except composite shapes.
            /// </summary>
            [MSBReference(ReferenceType = typeof(Region))]
            public string[] ChildRegionNames { get; set; }

            private short[] ChildListIndices { get; set; }

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
            /// Allows multiple regions to be identified by the same entity ID; read by index (0-7) for points param version
            /// 0x29 and later (14155E500). Only the first is set in retail (AiTarget 7000, EntryPoint 73xx).
            /// </summary>
            public int[] EntityGroupIDs { get; set; }

            private protected Region(string name)
            {
                Name = name;
                Shape = new MSB.Shape.Point();
                RegionID = -1;

                ParentRegionNames = new string[0];
                ChildRegionNames = new string[0];

                ParentListIndices = new short[0];
                ChildListIndices = new short[0];
                EntityGroupIDs = new int[8];

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
                region.EntityGroupIDs = (int[])EntityGroupIDs.Clone();
                region.ParentRegionNames = (string[])ParentRegionNames.Clone();
                region.ChildRegionNames = (string[])ChildRegionNames.Clone();
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
                Position = br.ReadVector3();
                Rotation = br.ReadVector3();
                RegionID = br.ReadInt32();

                parentListOffset = br.ReadInt64();
                childListOffset = br.ReadInt64();

                Unk78 = br.ReadInt32();
                Unk7C = br.ReadInt32();

                FormOffset = br.ReadInt64();
                CommonOffset = br.ReadInt64();
                TypeOffset = br.ReadInt64();
                Struct98Offset = br.ReadInt64();

                Shape = MSB.Shape.Create(shapeType);

                // Name
                Name = br.GetUTF16(start + NameOffset);

                // Point Indices 30
                br.Position = start + parentListOffset;
                short countA = br.ReadInt16();
                ParentListIndices = br.ReadInt16s(countA);

                // Point Indices 38
                br.Position = start + childListOffset;
                short countB = br.ReadInt16();
                ChildListIndices = br.ReadInt16s(countB);

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
                EntityGroupIDs = br.ReadInt32s(8);
                br.AssertInt32(new int[1]);

                // Type
                if (HasTypeData && TypeOffset != 0L)
                {
                    br.Position = start + TypeOffset;
                    ReadTypeData(br);
                }

                if(Type == RegionType.Other)
                {
                    OtherTypeData = null;

                    if (TypeOffset > 0)
                    {
                        long otherSize = Struct98Offset - TypeOffset;

                        if (otherSize > 0)
                            OtherTypeData = br.ReadBytes((int)otherSize);
                    }
                }

                // Struct98 Offset
                br.Position = start + Struct98Offset;
                br.AssertInt32(-1);
                br.AssertInt32(new int[1]);
                br.AssertInt32(-1);
            }

            private byte[] OtherTypeData;

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
                bw.WriteInt32(RegionID);

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
                bw.WriteInt16((short)ParentListIndices.Length);
                bw.WriteInt16s(ParentListIndices);
                bw.Pad(4);

                bw.FillInt64("IndexListOffset38", bw.Position - start);
                bw.WriteInt16((short)ChildListIndices.Length);
                bw.WriteInt16s(ChildListIndices);
                bw.Pad(8);

                if (Shape.HasShapeData)
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
                bw.WriteInt32s(EntityGroupIDs);
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

                if(Type == RegionType.Other)
                {
                    if(OtherTypeData != null)
                    {
                        bw.WriteBytes(OtherTypeData);
                    }
                }

                if (HasTypeDataPadding())
                {
                    if (MSB_AC6.CurrentVersion >= 52 && Type == RegionType.GarageCamera)
                    {
                        bw.Pad(4);
                    }
                    else
                    {
                        bw.Pad(8);
                    }
                }

                bw.FillInt64("Struct98Offset", bw.Position - start);
                bw.WriteInt32(-1);
                bw.WriteInt32(0);
                bw.WriteInt32(-1);

                if (HasStruct98Padding())
                {
                    bw.WriteInt32(0);
                }
            }

            private bool HasTypeDataPadding()
            {
                switch (Type)
                {
                    case RegionType.EntryPoint:
                    case RegionType.EnvMapPoint:
                    case RegionType.Sound:
                    case RegionType.SFX:
                    case RegionType.WindSFX:
                    case RegionType.EnvMapEffectBox:
                    case RegionType.WindPlacement:
                    case RegionType.MufflingBox:
                    case RegionType.MufflingPortal:
                    case RegionType.SoundOverride:
                    case RegionType.Patrol:
                    case RegionType.FeMapDisplay:
                        return true;

                    case RegionType.OperationalArea:
                        return false;

                    case RegionType.AiInformationSharing:
                    case RegionType.AiTarget:
                        return true;

                    case RegionType.WwiseEnvironmentSound:
                        return false;

                    case RegionType.NaviGeneration:
                    case RegionType.TopdownView:
                    case RegionType.CharacterFollowing:
                        return true;

                    case RegionType.NavmeshCostControl:
                    case RegionType.ArenaControl:
                    case RegionType.ArenaAppearance:
                        return false;

                    case RegionType.GarageCamera:
                    case RegionType.JumpEdgeRestriction:
                    case RegionType.CutscenePlayback:
                    case RegionType.FallPreventionWallRemoval:
                    case RegionType.BigJump:
                    case RegionType.Other:
                        return true;

                    default:
                        return false;
                }
            }

            // Which types have 4 bytes of padding at the end
            private bool HasStruct98Padding()
            {
                switch(Type)
                {
                    case RegionType.EntryPoint:
                    case RegionType.EnvMapPoint:
                    case RegionType.Sound:
                    case RegionType.SFX:
                    case RegionType.WindSFX:
                    case RegionType.EnvMapEffectBox:
                    case RegionType.WindPlacement:
                    case RegionType.MufflingBox:
                    case RegionType.MufflingPortal:
                    case RegionType.SoundOverride:
                    case RegionType.Patrol:
                    case RegionType.FeMapDisplay:
                        return true;

                    case RegionType.OperationalArea:
                        return false;

                    case RegionType.AiInformationSharing:
                    case RegionType.AiTarget:
                        return true;

                    case RegionType.WwiseEnvironmentSound:
                        return false;

                    case RegionType.NaviGeneration:
                    case RegionType.TopdownView:
                    case RegionType.CharacterFollowing:
                        return true;

                    case RegionType.NavmeshCostControl:
                    case RegionType.ArenaControl:
                    case RegionType.ArenaAppearance:
                    case RegionType.GarageCamera:
                        return false;

                    case RegionType.JumpEdgeRestriction:
                    case RegionType.CutscenePlayback:
                    case RegionType.FallPreventionWallRemoval:
                    case RegionType.BigJump:
                    case RegionType.Other:
                        return true;

                    default: 
                        return false;
                }
            }

            private protected virtual void WriteTypeData(BinaryWriterEx bw)
                => throw new NotImplementedException($"Type {GetType()} missing valid {nameof(ReadTypeData)}.");

            internal virtual void GetNames(Entries entries)
            {
                ParentRegionNames = MSB.FindNames(entries.Regions, ParentListIndices);
                ChildRegionNames = MSB.FindNames(entries.Regions, ChildListIndices);

                ActivationPartName = MSB.FindName(entries.Parts, ActivationPartIndex);

                if (Shape is MSB.Shape.Composite composite)
                    composite.GetNames(entries.Regions);
            }

            internal virtual void GetIndices(Entries entries)
            {
                ParentListIndices = MSB.FindShortIndices(this, entries.Regions, ParentRegionNames);
                ChildListIndices = MSB.FindShortIndices(this, entries.Regions, ChildRegionNames);

                ActivationPartIndex = MSB.FindIndex(this, entries.Parts, ActivationPartName);

                if (Shape is MSB.Shape.Composite composite)
                    composite.GetIndices(entries.Regions);
            }

            public RegionType GetRegionType()
            {
                return Type;
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

                // +00: not seen read by armoredcore6.exe; 0 on every retail entry point.
                private int Priority { get; set; }

                /// <summary>
                /// Creates an EntryPoint with default values.
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
            /// The capture point of an env map / irradiance volume. EnvMapEffectBox regions find their point by number:
            /// the box's FixedEnvMapNumber when 0 or more, else its EnvMapNumber, against the point's EnvMapNumber (older MSBs,
            /// version below 50, used the last three digits of the name, Env_PointNNN). When the box has UseIrradianceVolume
            /// set, the point's fields fill the irradiance volume capture settings (the GX debug menu's
            /// "IrradianceVolume Capture Param"); otherwise the defaults are used.
            /// </summary>
            public class EnvMapPoint : Region
            {
                private protected override RegionType Type => RegionType.EnvMapPoint;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Size of one irradiance volume voxel (GX: Voxel Size Basis). Clamped to 0.001 - 1000.
                /// </summary>
                public float VoxelSizeBasis { get; set; } = 22f;

                /// <summary>
                /// Number of light bounces of the irradiance capture (GX: Bounce Num). Default 2 without a point.
                /// </summary>
                public int BounceNum { get; set; } = 4;

                /// <summary>
                /// Light ID in the map's light bank used as both the directional and the sky light of the capture
                /// (GX: Dir Light Id, Sky Light Id). Negative values become 0. Was asserted -1; -1 on every retail point.
                /// </summary>
                public int LightID { get; set; } = -1;

                /// <summary>
                /// Not read by the game.
                /// </summary>
                private byte EnvMapPoint_UnkT0C { get; set; } = 1;

                /// <summary>
                /// Not read by the game.
                /// </summary>
                private byte EnvMapPoint_UnkT0D { get; set; } = 1;

                /// <summary>
                /// Sets a capture setting to 3 when on, 1 when off (3 is also the value without a point). Meaning not traced.
                /// </summary>
                public byte EnvMapPoint_UnkT0F { get; set; } = 1;

                /// <summary>
                /// Scale of the directional light in the capture (GX: Dir Light Scale). Default 1.
                /// </summary>
                public float DirLightScale { get; set; } = 1f;

                /// <summary>
                /// Scale of the sky light in the capture (GX: Sky Light Scale). Default 1. Was asserted 1.
                /// </summary>
                public float SkyLightScale { get; set; } = 1f;

                /// <summary>
                /// Not read by the game.
                /// </summary>
                private int EnvMapPoint_UnkT18 { get; set; } = 8389120;

                /// <summary>
                /// Not read by the game (bytes 0x1C-0x1E; byte 0x1F is <see cref="EnvMapPoint_UnkT1F"/>).
                /// </summary>
                private byte EnvMapPoint_UnkT1C { get; set; } = 0;

                /// <summary>
                /// Not read by the game.
                /// </summary>
                private byte EnvMapPoint_UnkT1D { get; set; } = 3;

                /// <summary>
                /// Not read by the game.
                /// </summary>
                private byte EnvMapPoint_UnkT1E { get; set; } = 6;

                /// <summary>
                /// A flag passed to the irradiance volume capture settings. Meaning not traced.
                /// </summary>
                public byte EnvMapPoint_UnkT1F { get; set; } = 1;

                /// <summary>
                /// The number EnvMapEffectBox regions use to find this point (the game reads it as a short).
                /// </summary>
                public int EnvMapNumber { get; set; }

                /// <summary>
                /// Creates an EnvMapPoint with default values.
                /// </summary>
                public EnvMapPoint() : base($"{nameof(Region)}: {nameof(EnvMapPoint)}") { }

                internal EnvMapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    VoxelSizeBasis = br.ReadSingle();
                    BounceNum = br.ReadInt32();
                    LightID = br.ReadInt32();
                    EnvMapPoint_UnkT0C = br.ReadByte();
                    EnvMapPoint_UnkT0D = br.ReadByte();
                    br.AssertByte((byte)1);
                    EnvMapPoint_UnkT0F = br.ReadByte();
                    DirLightScale = br.ReadSingle();
                    SkyLightScale = br.ReadSingle();
                    EnvMapPoint_UnkT18 = br.ReadInt32();
                    EnvMapPoint_UnkT1C = br.ReadByte();
                    EnvMapPoint_UnkT1D = br.ReadByte();
                    EnvMapPoint_UnkT1E = br.ReadByte();
                    EnvMapPoint_UnkT1F = br.ReadByte();

                    if(MSB_AC6.CurrentVersion >= 52)
                    {
                        EnvMapNumber = br.ReadInt32();
                        br.AssertInt32(new int[1]);
                    }
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(VoxelSizeBasis);
                    bw.WriteInt32(BounceNum);
                    bw.WriteInt32(LightID);
                    bw.WriteByte(EnvMapPoint_UnkT0C);
                    bw.WriteByte(EnvMapPoint_UnkT0D);
                    bw.WriteByte((byte)1);
                    bw.WriteByte(EnvMapPoint_UnkT0F);
                    bw.WriteSingle(DirLightScale);
                    bw.WriteSingle(SkyLightScale);
                    bw.WriteInt32(EnvMapPoint_UnkT18);
                    bw.WriteByte(EnvMapPoint_UnkT1C);
                    bw.WriteByte(EnvMapPoint_UnkT1D);
                    bw.WriteByte(EnvMapPoint_UnkT1E);
                    bw.WriteByte(EnvMapPoint_UnkT1F);

                    if (MSB_AC6.CurrentVersion >= 52)
                    {
                        bw.WriteInt32(EnvMapNumber);
                        bw.WriteInt32(0);
                    }
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
                public new string[] ChildRegionNames { get; set; }
                private int[] ChildRegionIndices { get; set; }

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
            /// An environment map region: a Box that lights what is inside it with the map's baked env maps (the
            /// GILM####_##_rem specular and _iem diffuse cubemaps) and, with UseIrradianceVolume, its GIIV####_## irradiance
            /// volume. Set up per box and per bake by 1412D04D0 (texture reload 1412D2210), which fills an EnvMapRegion param
            /// (debug menu labels at 14240BFA5..14240C803) and registers it (1412CE0E0 → 142403F70). Non-Box shapes are skipped.
            /// </summary>
            public class EnvMapEffectBox : Region
            {
                private protected override RegionType Type => RegionType.EnvMapEffectBox;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// EnvMapRegion "Draw Distance": the box is drawn while the camera is within this distance. 0 or less: a
                /// distance worked out from the box's size (142403C40).
                /// </summary>
                public float EnableDist { get; set; }

                /// <summary>
                /// EnvMapRegion "Fade Distance": the blend from the box faces inwards. Values under 0.1 are raised to 0.1.
                /// </summary>
                public float TransitionDist { get; set; }

                /// <summary>
                /// EnvMapRegion "Parallax Corrected Area: Enable" (non-zero = on); Elden Ring's ApplyParallaxCorrection.
                /// Was asserted 0.
                /// </summary>
                public byte ParallaxCorrection { get; set; }

                /// <summary>
                /// EnvMapRegion "Priority" (was UnkT09).
                /// </summary>
                public byte Priority { get; set; }

                /// <summary>
                /// When 0 or more, replaces EnvMapNumber for the textures and volume, and registers the region as
                /// "FixedRegionId" with this number (was the UnkT0A / UnkT0B bytes, read as one value). -1: none.
                /// </summary>
                public short FixedEnvMapNumber { get; set; } = -1;

                /// <summary>
                /// Moves the box's -X / -Y / -Z faces outwards by these amounts. Was asserted 0.
                /// </summary>
                public Vector3 BoundsExpandNegative { get; set; }

                /// <summary>
                /// Moves the box's +X / +Y / +Z faces outwards by these amounts. Was asserted 0.
                /// </summary>
                public Vector3 BoundsExpandPositive { get; set; }

                /// <summary>
                /// EnvMapRegion "Specular Color Scale".
                /// </summary>
                public float SpecularLightMult { get; set; } = 1f;

                /// <summary>
                /// EnvMapRegion "Diffuse Color Scale" (was PointLightMult).
                /// </summary>
                public float DiffuseLightMult { get; set; } = 1f;

                /// <summary>
                /// Sets "Enable Specular" / "Enable Diffuse": 0 both, 1 diffuse only, 2 specular only; other values keep
                /// both. Also decides whether the _rem / _iem cubemaps are loaded (was UnkT2C).
                /// </summary>
                public byte EnvMapComponents { get; set; }

                /// <summary>
                /// EnvMapRegion "EmphasisHighlight" (non-zero = on); 0 in every retail box (was UnkT2D).
                /// </summary>
                public byte EmphasisHighlight { get; set; }

                /// <summary>
                /// Loads the box's GIIV irradiance volume and gives the region the matching EnvMapPoint's volume settings
                /// (was IsModifyLight).
                /// </summary>
                public byte UseIrradianceVolume { get; set; } = 1;

                /// <summary>
                /// EnvMapRegion "Exclude Search" (non-zero = on) (was UnkT2F).
                /// </summary>
                public byte ExcludeSearch { get; set; }

                /// <summary>
                /// When 0 or more, a second EnvMapRegion is registered with this number and Exclude Search on (MSB version
                /// 0x27 or later; was UnkT30). -1: none.
                /// </summary>
                public short AdditionalEnvMapNumber { get; set; } = -1;

                /// <summary>
                /// The env map number: the GILM / GIIV texture number and the EnvMapPoint the box uses (MSB version 0x32 or
                /// later; older versions take it from an "env_box" name; was UnkT32).
                /// </summary>
                public short EnvMapNumber { get; set; }

                /// <summary>
                /// Creates an EnvMapEffectBox with default values.
                /// </summary>
                public EnvMapEffectBox() : base($"{nameof(Region)}: {nameof(EnvMapEffectBox)}")
                {
                    TransitionDist = 5f;
                    Priority = 20;
                }

                internal EnvMapEffectBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EnableDist = br.ReadSingle();
                    TransitionDist = br.ReadSingle();
                    ParallaxCorrection = br.ReadByte();
                    Priority = br.ReadByte();
                    FixedEnvMapNumber = br.ReadInt16();
                    BoundsExpandNegative = br.ReadVector3();
                    BoundsExpandPositive = br.ReadVector3();
                    SpecularLightMult = br.ReadSingle();
                    DiffuseLightMult = br.ReadSingle();
                    EnvMapComponents = br.ReadByte();
                    EmphasisHighlight = br.ReadByte();
                    UseIrradianceVolume = br.ReadByte();
                    ExcludeSearch = br.ReadByte();
                    AdditionalEnvMapNumber = br.ReadInt16();
                    EnvMapNumber = br.ReadInt16();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(EnableDist);
                    bw.WriteSingle(TransitionDist);
                    bw.WriteByte(ParallaxCorrection);
                    bw.WriteByte(Priority);
                    bw.WriteInt16(FixedEnvMapNumber);
                    bw.WriteVector3(BoundsExpandNegative);
                    bw.WriteVector3(BoundsExpandPositive);
                    bw.WriteSingle(SpecularLightMult);
                    bw.WriteSingle(DiffuseLightMult);
                    bw.WriteByte(EnvMapComponents);
                    bw.WriteByte(EmphasisHighlight);
                    bw.WriteByte(UseIrradianceVolume);
                    bw.WriteByte(ExcludeSearch);
                    bw.WriteInt16(AdditionalEnvMapNumber);
                    bw.WriteInt16(EnvMapNumber);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// The area of a wind effect: a WindSFX names it by WindAreaIndex (a global region index, looked up without a
            /// type check, 14120E5B0 → 141557A30), and the region's shape (Sphere / Cylinder / Box; others skipped) and
            /// world transform become the effect's area (14268DEF0 / 14268DE20 / 14268DE80). No other read of type 18.
            /// </summary>
            public class WindPlacement : Region
            {
                private protected override RegionType Type => RegionType.WindPlacement;
                private protected override bool HasTypeData => true;

                public WindPlacement() : base($"{nameof(Region)}: {nameof(WindPlacement)}") { }

                internal WindPlacement(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// A Wwise spatial audio room (exe CSSoundSpatialRoom): a Box whose walls are sent to Wwise as reflective
            /// geometry ("Geometry_%04d"), with reverb / occlusion / reflection settings. The type data at +18 points to
            /// the room block at +20; getters 14157A270..14157A6D0 read it (version ≥ 0x21, +38..+44 version ≥ 0x26).
            /// </summary>
            public class MufflingBox : Region
            {
                private protected override RegionType Type => RegionType.MufflingBox;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The room's number (getter 14157A200): names its Wwise geometry "Geometry_%04d" and is part of the room's
                /// Wwise ID (200,000,000 + number, high dword from the owner).
                /// </summary>
                public int BoxNumber { get; set; }

                /// <summary>
                /// WwiseValueToStrParam_ReverbAuxBusType row (param index 0xF0, getter 14157A390; no caller found).
                /// </summary>
                public int ReverbAuxBusType { get; set; } = 2;

                /// <summary>
                /// Reverb level in percent (getter 14157A350 divides by 100; shown as "ReverbLv" in the debug display).
                /// </summary>
                public float ReverbLevel { get; set; } = 100f;

                /// <summary>
                /// Occlusion in percent (getter 14157A4F0 / 100): the transmission loss of the room's Wwise wall surfaces
                /// (debug "Occlusion").
                /// </summary>
                public float Occlusion { get; set; } = 20f;

                /// <summary>
                /// WwiseValueToStrParam_ReflectTextureType row (param index 0xF2, getter 14157A310): the acoustic texture
                /// of the room's Wwise wall surfaces.
                /// </summary>
                public int ReflectTextureType { get; set; } = 1;

                /// <summary>
                /// WwiseValueToStrParam_ReflectAuxBusType row (param index 0xF1, getter 14157A270). 0 in retail.
                /// </summary>
                public int ReflectAuxBusType { get; set; }

                /// <summary>
                /// Reflection level in percent (getter 14157A2D0 / 100; debug "ReflectLv").
                /// </summary>
                public float ReflectLevel { get; set; } = 100f;

                /// <summary>
                /// When set (getter 14157A6D0, byte), the room's walls are not built at load (141249700 returns early);
                /// they are built (141249CA0) while the listener is inside the box and removed when it leaves (141248F70).
                /// 0 in retail.
                /// </summary>
                public byte BuildWallsOnEntry { get; set; }

                /// <summary>
                /// Replace the Box's Width / Depth / Height when above 0 (getter 14157A400 returns half of each); −1 keeps
                /// the shape's size.
                /// </summary>
                public float OverrideWidth { get; set; } = -1f;

                /// <summary>
                /// See <see cref="OverrideWidth"/>.
                /// </summary>
                public float OverrideDepth { get; set; } = -1f;

                /// <summary>
                /// See <see cref="OverrideWidth"/>.
                /// </summary>
                public float OverrideHeight { get; set; } = -1f;

                /// <summary>
                /// Creates a MufflingBox with default values.
                /// </summary>
                public MufflingBox() : base($"{nameof(Region)}: {nameof(MufflingBox)}") { }

                internal MufflingBox(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    BoxNumber = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt64(32L);
                    ReverbAuxBusType = br.ReadInt32();
                    ReverbLevel = br.ReadSingle();
                    Occlusion = br.ReadSingle();
                    ReflectTextureType = br.ReadInt32();
                    ReflectAuxBusType = br.ReadInt32();
                    ReflectLevel = br.ReadSingle();
                    BuildWallsOnEntry = br.ReadByte();
                    br.AssertByte(0);
                    br.AssertInt16(0);
                    OverrideWidth = br.ReadSingle();
                    OverrideDepth = br.ReadSingle();
                    OverrideHeight = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(BoxNumber);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt64(32L);
                    bw.WriteInt32(ReverbAuxBusType);
                    bw.WriteSingle(ReverbLevel);
                    bw.WriteSingle(Occlusion);
                    bw.WriteInt32(ReflectTextureType);
                    bw.WriteInt32(ReflectAuxBusType);
                    bw.WriteSingle(ReflectLevel);
                    bw.WriteByte(BuildWallsOnEntry);
                    bw.WriteByte(0);
                    bw.WriteInt16(0);
                    bw.WriteSingle(OverrideWidth);
                    bw.WriteSingle(OverrideDepth);
                    bw.WriteSingle(OverrideHeight);
                }
            }

            /// <summary>
            /// The exe's CSMsbPointOcclusionPortal: a Wwise spatial audio portal (an opening between two rooms). At map
            /// load the sound manager (141241542 → 141247350) calls AK::SpatialAudio::SetPortal with the region's
            /// transform, an extent from its shape (half-sizes; a Point gives 3 on every axis, 14155E3E0), enabled, and
            /// the front / back rooms below, then SetPortalObstructionAndOcclusion. Same layout as MSBE OcclusionPortal.
            /// Retail: one portal (m50_50_00_00).
            /// </summary>
            public class MufflingPortal : Region
            {
                private protected override RegionType Type => RegionType.MufflingPortal;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Getter 14157AD70: only formats the portal's debug name "Portal_box%04d". MSBE: the owning box's number.
                /// </summary>
                public int BoxNumber { get; set; }

                /// <summary>
                /// Getter 14157AF10 (version ≥ 0x21): the portal's Wwise ID = (map key &lt;&lt; 32) + 400,000,000 + value.
                /// </summary>
                public int PortalID { get; set; }

                /// <summary>
                /// Getter 14157AE80: the portal's front room, Wwise room ID (map key &lt;&lt; 32) + 100,000,000 + value.
                /// </summary>
                public int FrontRoomNumber { get; set; }

                /// <summary>
                /// +28: no getter.
                /// </summary>
                private int UnkT28 { get; set; }

                /// <summary>
                /// +2C: getter 14157AEE0 exists but has no caller.
                /// </summary>
                private int UnkT2C { get; set; }

                /// <summary>
                /// Getter 14157AEB0: the back room, as <see cref="FrontRoomNumber"/>; 0 or less uses the Outdoors room
                /// (141242E40, "Room_Outdoors").
                /// </summary>
                public int BackRoomNumber { get; set; }

                /// <summary>
                /// Getter 14157AE10 (version ≥ 0x23): percent; when 0 or more, value / 100 is passed as both the
                /// obstruction and the occlusion of SetPortalObstructionAndOcclusion. −1 (retail) skips the call.
                /// </summary>
                public int ObstructionOcclusion { get; set; } = -1;

                /// <summary>
                /// Creates a MufflingPortal with default values.
                /// </summary>
                public MufflingPortal() : base($"{nameof(Region)}: {nameof(MufflingPortal)}") { }

                internal MufflingPortal(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    BoxNumber = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                    br.AssertInt64(32L);
                    PortalID = br.ReadInt32();
                    FrontRoomNumber = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    UnkT2C = br.ReadInt32();
                    BackRoomNumber = br.ReadInt32();
                    ObstructionOcclusion = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(BoxNumber);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt64(32L);
                    bw.WriteInt32(PortalID);
                    bw.WriteInt32(FrontRoomNumber);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(UnkT2C);
                    bw.WriteInt32(BackRoomNumber);
                    bw.WriteInt32(ObstructionOcclusion);
                }
            }

            /// <summary>
            /// A box that overrides the reverb and the outdoor / indoor sound state while the player is inside.
            /// Same layout as MSBE.Region.SoundRegion. Only Box shapes are used.
            /// </summary>
            public class SoundOverride : Region
            {
                private protected override RegionType Type => RegionType.SoundOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Reverb type while inside, sent to the Wwise ReverbType and RoomType RTPCs (range 0-8; 16 or more is
                /// ignored) and turns off auto reverb. -1 leaves it unchanged.
                /// </summary>
                public sbyte ReverbType { get; set; }

                /// <summary>
                /// Wwise OutdoorIndoor / OutdoorIndoorForEnv state while inside: 0 Outdoor, 1 IndoorHalf, 2 IndoorAll.
                /// -1 (or any other value) leaves it unchanged.
                /// </summary>
                public sbyte OutdoorIndoorType { get; set; }

                /// <summary>
                /// Creates a SoundOverride with default values.
                /// </summary>
                public SoundOverride() : base($"{nameof(Region)}: {nameof(SoundOverride)}") { }

                internal SoundOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ReverbType = br.ReadSByte();
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    OutdoorIndoorType = br.ReadSByte();
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
                    bw.WriteSByte(ReverbType);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                    bw.WriteSByte(OutdoorIndoorType);
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
            /// A walk point of a PatrolRoute event. The route setup (1406430E0) copies each point's +0 and +4 into the
            /// route's walk point record (140628C80: +20 action ID, +24 turn type, labelled SetActionId / SetTurnType in the
            /// exe's debug text). A Sphere shape's radius is read for the point's inside test (140643EB0, Lua IsInsideMovePoint).
            /// </summary>
            public class Patrol : Region
            {
                private protected override RegionType Type => RegionType.Patrol;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// The action at this move point; AI scripts get it with the Lua function GetMovePointActionId (1406378F0,
                /// -1 when the index is out of range). No retail AI script calls it. As MSBE.Region.PatrolRoute.ActionID.
                /// -1 or 0 in retail.
                /// </summary>
                public int ActionID { get; set; }

                /// <summary>
                /// How the AI turns at this point (debug label 旋回タイプ). 0: smooth patrol (なめらか巡回), the route
                /// is blended through the point. 1: quick turn (クイックターン). Read as a byte by the move goals through
                /// 140637960; any other value (retail -1) gets neither the smoothing nor the quick turn.
                /// Retail: 0 ×1372, -1 ×492, 1 ×172.
                /// </summary>
                public sbyte TurnType { get; set; }

                /// <summary>
                /// Creates a Patrol with default values.
                /// </summary>
                public Patrol() : base($"{nameof(Region)}: {nameof(Patrol)}") { }

                internal Patrol(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    ActionID = br.ReadInt32();
                    TurnType = br.ReadSByte();
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                    br.AssertByte(new byte[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(ActionID);
                    bw.WriteSByte(TurnType);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                    bw.WriteByte((byte)0);
                }
            }

            /// <summary>
            /// A marker shown on the HUD / mission map at the region's position (getters 14156CC50..14156CDC0).
            /// Turned on and off by EMEVD 2003[1000] Change Target Marker Enable State through the region's EntityID or
            /// one of its EntityGroupIDs (handler 140B00A60, only when UnkT00 is 1).
            /// </summary>
            public class FeMapDisplay : Region
            {
                private protected override RegionType Type => RegionType.FeMapDisplay;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Marker mode. 0: never listed. 1: target marker (event-controlled; UnkT02 and UnkT03 apply).
                /// Any other value is listed with kind = this value and icon type 1. 1 on all retail regions but one (2).
                /// </summary>
                public byte FeMapDisplay_UnkT00 { get; set; }

                // +01: getter 14156CCC0 (returns it when UnkT00 is 2 or 4) has no caller; 1 on all retail regions but one.
                private byte FeMapDisplay_UnkT01 { get; set; }

                /// <summary>
                /// Marker icon type, used when UnkT00 is 1 (otherwise 1). Values 2 to 7 are left out of one marker list
                /// (140D12560). Retail: 101, 102, 104, 1.
                /// </summary>
                public byte FeMapDisplay_UnkT02 { get; set; }

                /// <summary>
                /// When UnkT00 is 1 and this is set, the region's name is passed as the marker's label (14156CDC0).
                /// </summary>
                public byte FeMapDisplay_UnkT03 { get; set; }

                // +04: getter 14156CD70 (returns it when UnkT00 is 1) has no caller; 0 everywhere but one region (99961000).
                private int FeMapDisplay_UnkT04 { get; set; }

                /// <summary>
                /// Creates a MapPoint with default values.
                /// </summary>
                public FeMapDisplay() : base($"{nameof(Region)}: {nameof(FeMapDisplay)}") { }

                internal FeMapDisplay(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    FeMapDisplay_UnkT00 = br.ReadByte();
                    FeMapDisplay_UnkT01 = br.ReadByte();
                    FeMapDisplay_UnkT02 = br.ReadByte();
                    FeMapDisplay_UnkT03 = br.ReadByte();
                    FeMapDisplay_UnkT04 = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(FeMapDisplay_UnkT00);
                    bw.WriteByte(FeMapDisplay_UnkT01);
                    bw.WriteByte(FeMapDisplay_UnkT02);
                    bw.WriteByte(FeMapDisplay_UnkT03);
                    bw.WriteInt32(FeMapDisplay_UnkT04);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// The mission's operational area (combat area): a box the player is tested against. EMEVD 3[29] IF In
            /// Combat Area (140A8E5C0) reads 0 inside the box, 1 inside the box grown by both margin sets, 2 elsewhere.
            /// Only Box shapes count (141579AA0), and an area disabled by 2003[1008] never counts as inside.
            /// </summary>
            public class OperationalArea : Region
            {
                private protected override RegionType Type => RegionType.OperationalArea;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Margin set A, horizontal: added to every side of the box (141579A10, 141579950). Applied only
                /// when the area's runtime mode (wrapper +68h, 141573B30) is 1 or 2, and always in the warning box.
                /// </summary>
                public float OperationalArea_UnkT00 { get; set; }

                /// <summary>
                /// Margin set A, upward: added to the top of the box.
                /// </summary>
                public float OperationalArea_UnkT04 { get; set; }

                /// <summary>
                /// Margin set A, downward: added below the bottom of the box.
                /// </summary>
                public float OperationalArea_UnkT08 { get; set; }

                /// <summary>
                /// Margin set B, horizontal: with set A, the warning band on every side (141579950). Also read on
                /// its own by the HUD-side area system (getter 141579420, 140D1E880).
                /// </summary>
                public float OperationalArea_UnkT0C { get; set; } = 150;

                /// <summary>
                /// Margin set B, upward.
                /// </summary>
                public float OperationalArea_UnkT10 { get; set; } = 150;

                /// <summary>
                /// Margin set B, downward.
                /// </summary>
                public float OperationalArea_UnkT14 { get; set; } = 150;

                // +18: no read in the OperationalArea getter unit (141579200..141579C80) or its consumers; 0 on all
                // retail regions but one (m01_01_30_00, 1).
                private int OperationalArea_UnkT18 { get; set; }

                /// <summary>
                /// Creates a OperationalArea with default values.
                /// </summary>
                public OperationalArea() : base($"{nameof(Region)}: {nameof(OperationalArea)}") { }

                internal OperationalArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    OperationalArea_UnkT00 = br.ReadSingle();
                    OperationalArea_UnkT04 = br.ReadSingle();
                    OperationalArea_UnkT08 = br.ReadSingle();
                    OperationalArea_UnkT0C = br.ReadSingle();
                    OperationalArea_UnkT10 = br.ReadSingle();
                    OperationalArea_UnkT14 = br.ReadSingle();
                    OperationalArea_UnkT18 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(OperationalArea_UnkT00);
                    bw.WriteSingle(OperationalArea_UnkT04);
                    bw.WriteSingle(OperationalArea_UnkT08);
                    bw.WriteSingle(OperationalArea_UnkT0C);
                    bw.WriteSingle(OperationalArea_UnkT10);
                    bw.WriteSingle(OperationalArea_UnkT14);
                    bw.WriteInt32(OperationalArea_UnkT18);
                }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class AiInformationSharing : Region
            {
                private protected override RegionType Type => RegionType.AiInformationSharing;
                private protected override bool HasTypeData => true;

                public AiInformationSharing() : base($"{nameof(Region)}: {nameof(AiInformationSharing)}") { }

                internal AiInformationSharing(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// An AI destination point (CSMsbPointAiDest): found by the AI Lua bindings SearchAiDestinationPoint / Ex / Shoot
            /// and claimed with ReserveAiDestinationPoint.
            /// </summary>
            public class AiTarget : Region
            {
                private protected override RegionType Type => RegionType.AiTarget;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Point type (AIDestPoint_Type: 0 Snipe, 1 Cover, 2 Shield, 3 Defence, 4 Search, 5 Shoot); a search matches this or UnkT01.
                /// </summary>
                public byte AiTarget_UnkT00 { get; set; }

                /// <summary>
                /// Secondary point type, matched the same way as UnkT00.
                /// </summary>
                public byte AiTarget_UnkT01 { get; set; }

                /// <summary>
                /// For Shoot points: entity ID of the target character, matched by SearchAiDestinationPointShoot.
                /// </summary>
                public int AiTarget_UnkT04 { get; set; }

                /// <summary>
                /// Creates a AiTarget with default values.
                /// </summary>
                public AiTarget() : base($"{nameof(Region)}: {nameof(AiTarget)}") { }

                internal AiTarget(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    AiTarget_UnkT00 = br.ReadByte();
                    AiTarget_UnkT01 = br.ReadByte();
                    br.AssertInt16(new short[1]);
                    AiTarget_UnkT04 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(AiTarget_UnkT00);
                    bw.WriteByte(AiTarget_UnkT01);
                    bw.WriteInt16((short)0);
                    bw.WriteInt32(AiTarget_UnkT04);
                }
            }

            /// <summary>
            /// Wwise ambient sound area (namelist wwise環境音). While the listener is in one, its environment ID drives the
            /// Wwise game parameters Env_Region_1st / Env_Region_2nd (type 0) or Env_Wind_1st / Env_Wind_2nd (type 1):
            /// the sound object's callback 14163A800 (registered on the point repository) collects up to two IDs per type,
            /// and 141643210 writes them through 141658A70 / 141658AA0 to RTPC slots 13-16 (range 0-99, 0 when absent).
            /// </summary>
            public class WwiseEnvironmentSound : Region
            {
                private protected override RegionType Type => RegionType.WwiseEnvironmentSound;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Which pair of Wwise game parameters the region feeds: 0 Env_Region (regional ambience), 1 Env_Wind (wind).
                /// Other values are ignored. The exe's MSB debug table calls it point_wwise_env_sound_env_type (環境タイプ).
                /// </summary>
                public byte EnvType { get; set; }

                /// <summary>
                /// The value written to the Env_Region / Env_Wind game parameter (clamped to 0-99). The exe's MSB debug table
                /// calls it point_wwise_env_sound_env_id (環境ID).
                /// </summary>
                public byte EnvID { get; set; }

                /// <summary>
                /// Creates an WwiseEnvironmentSound with default values.
                /// </summary>
                public WwiseEnvironmentSound() : base($"{nameof(Region)}: {nameof(WwiseEnvironmentSound)}") { }

                internal WwiseEnvironmentSound(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EnvType = br.ReadByte();
                    EnvID = br.ReadByte();
                    br.AssertInt16(new short[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(EnvType);
                    bw.WriteByte(EnvID);
                    bw.WriteInt16((short)0);
                }
            }

            /// <summary>
            /// Generation bounds of the map's flying navigation (nav volume): the volume's grid is the box's world AABB
            /// (exact in 14 of 18 retail maps). Not read by armoredcore6.exe (no typed lookup or collector for type 45).
            /// No type data.
            /// </summary>
            public class NaviGeneration : Region
            {
                private protected override RegionType Type => RegionType.NaviGeneration;
                private protected override bool HasTypeData => true;

                public NaviGeneration() : base($"{nameof(Region)}: {nameof(NaviGeneration)}") { }

                internal NaviGeneration(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// The area FromSoftware's tools capture as a top-down image of the map (namelist 上面図撮影領域). Not read by the game.
            /// </summary>
            public class TopdownView : Region
            {
                private protected override RegionType Type => RegionType.TopdownView;
                private protected override bool HasTypeData => true;

                public TopdownView() : base($"{nameof(Region)}: {nameof(TopdownView)}") { }

                internal TopdownView(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// A region attached to a character's dummy poly (exe 141763310 / 1417631B0); its world transform follows the character.
            /// </summary>
            public class CharacterFollowing : Region
            {
                private protected override RegionType Type => RegionType.CharacterFollowing;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Entity ID of the followed character (resolved like EMEVD IDs, 140AD4950).
                /// </summary>
                public int CharacterFollowing_UnkT00 { get; set; }

                /// <summary>
                /// Dummy poly ID on the character; if missing, the character's own transform is used.
                /// </summary>
                public int CharacterFollowing_UnkT04 { get; set; }

                /// <summary>
                /// Offset X from the dummy poly, in its frame.
                /// </summary>
                public float CharacterFollowing_UnkT08 { get; set; }

                /// <summary>
                /// Offset Y from the dummy poly, in its frame.
                /// </summary>
                public float CharacterFollowing_UnkT0C { get; set; }

                /// <summary>
                /// Offset Z from the dummy poly, in its frame.
                /// </summary>
                public float CharacterFollowing_UnkT10 { get; set; }

                /// <summary>
                /// Rotation offset X from the dummy poly, in degrees.
                /// </summary>
                public float CharacterFollowing_UnkT14 { get; set; }

                /// <summary>
                /// Rotation offset Y from the dummy poly, in degrees.
                /// </summary>
                public float CharacterFollowing_UnkT18 { get; set; }

                /// <summary>
                /// Rotation offset Z from the dummy poly, in degrees.
                /// </summary>
                public float CharacterFollowing_UnkT1C { get; set; }

                /// <summary>
                /// Creates an CharacterFollowing with default values.
                /// </summary>
                public CharacterFollowing() : base($"{nameof(Region)}: {nameof(CharacterFollowing)}") { }

                internal CharacterFollowing(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    CharacterFollowing_UnkT00 = br.ReadInt32();
                    CharacterFollowing_UnkT04 = br.ReadInt32();
                    CharacterFollowing_UnkT08 = br.ReadSingle();
                    CharacterFollowing_UnkT0C = br.ReadSingle();
                    CharacterFollowing_UnkT10 = br.ReadSingle();
                    CharacterFollowing_UnkT14 = br.ReadSingle();
                    CharacterFollowing_UnkT18 = br.ReadSingle();
                    CharacterFollowing_UnkT1C = br.ReadSingle();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(CharacterFollowing_UnkT00);
                    bw.WriteInt32(CharacterFollowing_UnkT04);
                    bw.WriteSingle(CharacterFollowing_UnkT08);
                    bw.WriteSingle(CharacterFollowing_UnkT0C);
                    bw.WriteSingle(CharacterFollowing_UnkT10);
                    bw.WriteSingle(CharacterFollowing_UnkT14);
                    bw.WriteSingle(CharacterFollowing_UnkT18);
                    bw.WriteSingle(CharacterFollowing_UnkT1C);
                }
            }

            /// <summary>
            /// A volume that marks the navmesh faces inside it as Obstacle, Impassable or High Cost for enemy pathing.
            /// At map load (exe 14143C3E0) each type-49 region with a Sphere, Cylinder or Box shape becomes a Havok
            /// hkaiPointCloudSilhouetteGenerator (a navmesh cutter) keyed by RegionID; Change Area Enable State
            /// (EMEVD 2003[1008]) flips its enabled flag (14155FAD0 → 1414180E0).
            /// </summary>
            public class NavmeshCostControl : Region
            {
                private protected override RegionType Type => RegionType.NavmeshCostControl;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Read as a byte: 1 Obstacle (cutter material 0x8), 2 Impassable (0xFFFFFFFF, cut out), 3 High Cost
                /// (0x200000); any other value builds no cutter. The same mapping as AssetGeometryParam navimeshFlag.
                /// </summary>
                public byte NavmeshCostControl_UnkT00 { get; set; }

                // +01..+03: never read (the loader reads only the byte at +00); 0 on every retail region.
                private byte NavmeshCostControl_UnkT01 { get; set; }
                private byte NavmeshCostControl_UnkT02 { get; set; }
                private byte NavmeshCostControl_UnkT03 { get; set; }

                /// <summary>
                /// Creates an NavmeshCostControl with default values.
                /// </summary>
                public NavmeshCostControl() : base($"{nameof(Region)}: {nameof(NavmeshCostControl)}")
                {
                    // Impassable, as on 218 of 227 retail regions (0 would build no cutter)
                    NavmeshCostControl_UnkT00 = 2;
                }

                internal NavmeshCostControl(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    NavmeshCostControl_UnkT00 = br.ReadByte();
                    NavmeshCostControl_UnkT01 = br.ReadByte();
                    NavmeshCostControl_UnkT02 = br.ReadByte();
                    NavmeshCostControl_UnkT03 = br.ReadByte();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(NavmeshCostControl_UnkT00);
                    bw.WriteByte(NavmeshCostControl_UnkT01);
                    bw.WriteByte(NavmeshCostControl_UnkT02);
                    bw.WriteByte(NavmeshCostControl_UnkT03);
                }
            }

            /// <summary>
            /// An unused arena area (namelist アリーナ制圧領域, "arena control area"). armoredcore6.exe has no typed read of
            /// type 50 (no ptgetters getter; typerange 32 finds no point lookup or collector call with it). One retail
            /// region (m01_40_00_00, Cylinder, Entity ID 0). The type data is one int, asserted 0.
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
            /// An arena respawn point (namelist アリーナ出現位置, "arena appearance position"). The one reader is the
            /// respawn picker 141112BC0: it collects every type-51 point (141571540 with 33h) whose map matches the
            /// respawning character's and whose EntityID % 10000 is not 401..499, and sorts them into three buckets by
            /// distance to the other characters (NetworkParam row 0 +6C ArenaEneNoRespawnDist, +70
            /// ArenaFriPrioritizeDist, +74 ArenaFriNoRespawnDist). Caller 141111469 warps the character to the chosen
            /// point's position and Y rotation + pi. The type data is one int, asserted 0.
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
            /// A point on a garage camera lane (type 52). Points are chained by ChildRegionNames (the parent list holds the
            /// reverse links); one point per lane has EntityID 101600 + the GarageCameraAnimParam laneCamPointIdx. Retail: 16 in
            /// m00_10_00_00. The exe reader was not found (MSB Research.md §6b).
            /// </summary>
            public class GarageCamera : Region
            {
                private protected override RegionType Type => RegionType.GarageCamera;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Not traced; one value per lane (-0.2 on lane 1, -1 elsewhere).
                /// </summary>
                public float GarageCamera_UnkT00 { get; set; }

                /// <summary>
                /// Not traced; one value per lane (2 on the high lanes 3 and 4, 3 elsewhere).
                /// </summary>
                public float GarageCamera_UnkT04 { get; set; }

                /// <summary>
                /// Creates an GarageCamera with default values.
                /// </summary>
                public GarageCamera() : base($"{nameof(Region)}: {nameof(GarageCamera)}") { }

                internal GarageCamera(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    GarageCamera_UnkT00 = br.ReadSingle();
                    GarageCamera_UnkT04 = br.ReadSingle();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(GarageCamera_UnkT00);
                    bw.WriteSingle(GarageCamera_UnkT04);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An area AI characters can't jump into or out of (ジャンプエッジ通行制限領域). The AI jump-edge check
            /// (140532860) refuses a jump whose two ends are split by an enabled region of this type (wrapper +60h == 0),
            /// in the maps of both ends. No type data (TypeOffset 0 in retail).
            /// </summary>
            public class JumpEdgeRestriction : Region
            {
                private protected override RegionType Type => RegionType.JumpEdgeRestriction;
                private protected override bool HasTypeData => true;

                public JumpEdgeRestriction() : base($"{nameof(Region)}: {nameof(JumpEdgeRestriction)}") { }

                internal JumpEdgeRestriction(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }

            /// <summary>
            /// The origin of an arena cutscene (カットシーン再生ポイント, cutscene playback point): when the game plays an
            /// arena or versus intro / outro cutscene it collects every enabled region of this type, takes the one whose
            /// PlaybackPointID matches the requested ID, and plays the cutscene relative to its position and rotation.
            /// </summary>
            public class CutscenePlayback : Region
            {
                private protected override RegionType Type => RegionType.CutscenePlayback;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Which cutscene request uses this point: 100 versus team A intro, 101 versus team B intro,
                /// 200 single-player arena start and end. No other value is requested by the game.
                /// </summary>
                public int PlaybackPointID { get; set; }

                /// <summary>
                /// Creates an CutscenePlayback with default values.
                /// </summary>
                public CutscenePlayback() : base($"{nameof(Region)}: {nameof(CutscenePlayback)}") { }

                internal CutscenePlayback(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    PlaybackPointID = br.ReadInt32();
                    br.AssertInt32(new int[1]);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(PlaybackPointID);
                    bw.WriteInt32(0);
                }
            }

            /// <summary>
            /// An area where the map's fall prevention walls (the f collision files) are left out (落下防止壁削除).
            /// Not read by the game (no typed getter, no 37h type test in either .text section); most likely an input to
            /// the offline wall generator, like ER's FallPreventionRemoval. Type data: two ints, asserted 0.
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
            /// A jump pad area. Condition switch 1407D6AC0 case 22 (category 39) asks whether the character is inside
            /// any region of this type; jump state 138 (高度指定ジャンプ（マップ指定）) takes the first one in repo order
            /// holding the character (1407AA210 / visitor 14079F3B0) and passes JumpSpecifyAltParamID to the jump
            /// setup 14083C460. The enable flag (wrapper +60h) is not read.
            /// </summary>
            public class BigJump : Region
            {
                private protected override RegionType Type => RegionType.BigJump;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// JumpSpecifyAltParam row for the jump; a row field below 0 keeps the normal jump value, a missing
                /// row keeps all of them.
                /// </summary>
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
            /// Other.
            /// </summary>
            public class Other : Region
            {
                private protected override RegionType Type => RegionType.Other;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Creates an Other with default values.
                /// </summary>
                public Other() : base($"{nameof(Region)}: {nameof(Other)}") { }

                internal Other(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                }
            }
        }
    }
}
