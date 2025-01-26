using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace SoulsFormats
{
    public partial class MSBS
    {
        internal enum RegionType : uint
        {
            InvasionPoint = 1,
            EnvironmentMapPoint = 2,
            //Region3 = 3,
            Sound = 4,
            SFX = 5,
            WindSFX = 6,
            //Region7 = 7,
            SpawnPoint = 8,
            //Message = 9,
            //PseudoMultiplayer = 10,
            PatrolRoute = 11,
            //MovementPoint = 12,
            WarpPoint = 13,
            ActivationArea = 14,
            Event = 15,
            Logic = 0, // There are no regions of type 16 and type 0 is written in this order, so I suspect this is correct
            EnvironmentMapEffectBox = 17,
            WindArea = 18,
            //Region19 = 19,
            MufflingBox = 20,
            MufflingPortal = 21,
            //DrawGroupArea = 22,
            SoundSpaceOverride = 23,
            MufflingPlane = 24,
            PartsGroupArea = 25,
            AutoDrawGroupPoint = 26,
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
            /// Points that describe an NPC patrol path.
            /// </summary>
            public List<Region.PatrolRoute> PatrolRoutes { get; set; }

            /// <summary>
            /// Regions for warping the player.
            /// </summary>
            public List<Region.WarpPoint> WarpPoints { get; set; }

            /// <summary>
            /// Regions that trigger enemies when entered.
            /// </summary>
            public List<Region.ActivationArea> ActivationAreas { get; set; }

            /// <summary>
            /// Generic regions for use with event scripts.
            /// </summary>
            public List<Region.Event> Events { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.Logic> Logic { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.EnvironmentMapEffectBox> EnvironmentMapEffectBoxes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.WindArea> WindAreas { get; set; }

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
            public List<Region.SoundSpaceOverride> SoundSpaceOverrides { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.MufflingPlane> MufflingPlanes { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.PartsGroupArea> PartsGroupAreas { get; set; }

            /// <summary>
            /// Unknown.
            /// </summary>
            public List<Region.AutoDrawGroupPoint> AutoDrawGroupPoints { get; set; }

            /// <summary>
            /// Most likely a dumping ground for unused regions.
            /// </summary>
            public List<Region.Other> Others { get; set; }

            /// <summary>
            /// Creates an empty PointParam with the default version.
            /// </summary>
            public PointParam() : base(35, "POINT_PARAM_ST")
            {
                InvasionPoints = new List<Region.InvasionPoint>();
                EnvironmentMapPoints = new List<Region.EnvironmentMapPoint>();
                Sounds = new List<Region.Sound>();
                SFX = new List<Region.SFX>();
                WindSFX = new List<Region.WindSFX>();
                SpawnPoints = new List<Region.SpawnPoint>();
                PatrolRoutes = new List<Region.PatrolRoute>();
                WarpPoints = new List<Region.WarpPoint>();
                ActivationAreas = new List<Region.ActivationArea>();
                Events = new List<Region.Event>();
                Logic = new List<Region.Logic>();
                EnvironmentMapEffectBoxes = new List<Region.EnvironmentMapEffectBox>();
                WindAreas = new List<Region.WindArea>();
                MufflingBoxes = new List<Region.MufflingBox>();
                MufflingPortals = new List<Region.MufflingPortal>();
                SoundSpaceOverrides = new List<Region.SoundSpaceOverride>();
                MufflingPlanes = new List<Region.MufflingPlane>();
                PartsGroupAreas = new List<Region.PartsGroupArea>();
                AutoDrawGroupPoints = new List<Region.AutoDrawGroupPoint>();
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
                    case Region.PatrolRoute r: PatrolRoutes.Add(r); break;
                    case Region.WarpPoint r: WarpPoints.Add(r); break;
                    case Region.ActivationArea r: ActivationAreas.Add(r); break;
                    case Region.Event r: Events.Add(r); break;
                    case Region.Logic r: Logic.Add(r); break;
                    case Region.EnvironmentMapEffectBox r: EnvironmentMapEffectBoxes.Add(r); break;
                    case Region.WindArea r: WindAreas.Add(r); break;
                    case Region.MufflingBox r: MufflingBoxes.Add(r); break;
                    case Region.MufflingPortal r: MufflingPortals.Add(r); break;
                    case Region.SoundSpaceOverride r: SoundSpaceOverrides.Add(r); break;
                    case Region.MufflingPlane r: MufflingPlanes.Add(r); break;
                    case Region.PartsGroupArea r: PartsGroupAreas.Add(r); break;
                    case Region.AutoDrawGroupPoint r: AutoDrawGroupPoints.Add(r); break;
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
                    SpawnPoints, PatrolRoutes, WarpPoints, ActivationAreas, Events,
                    Logic, EnvironmentMapEffectBoxes, WindAreas, MufflingBoxes, MufflingPortals,
                    SoundSpaceOverrides, MufflingPlanes, PartsGroupAreas, AutoDrawGroupPoints, Others);
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

                    case RegionType.PatrolRoute:
                        return PatrolRoutes.EchoAdd(new Region.PatrolRoute(br));

                    case RegionType.WarpPoint:
                        return WarpPoints.EchoAdd(new Region.WarpPoint(br));

                    case RegionType.ActivationArea:
                        return ActivationAreas.EchoAdd(new Region.ActivationArea(br));

                    case RegionType.Event:
                        return Events.EchoAdd(new Region.Event(br));

                    case RegionType.Logic:
                        return Logic.EchoAdd(new Region.Logic(br));

                    case RegionType.EnvironmentMapEffectBox:
                        return EnvironmentMapEffectBoxes.EchoAdd(new Region.EnvironmentMapEffectBox(br));

                    case RegionType.WindArea:
                        return WindAreas.EchoAdd(new Region.WindArea(br));

                    case RegionType.MufflingBox:
                        return MufflingBoxes.EchoAdd(new Region.MufflingBox(br));

                    case RegionType.MufflingPortal:
                        return MufflingPortals.EchoAdd(new Region.MufflingPortal(br));

                    case RegionType.SoundSpaceOverride:
                        return SoundSpaceOverrides.EchoAdd(new Region.SoundSpaceOverride(br));

                    case RegionType.MufflingPlane:
                        return MufflingPlanes.EchoAdd(new Region.MufflingPlane(br));

                    case RegionType.PartsGroupArea:
                        return PartsGroupAreas.EchoAdd(new Region.PartsGroupArea(br));

                    case RegionType.AutoDrawGroupPoint:
                        return AutoDrawGroupPoints.EchoAdd(new Region.AutoDrawGroupPoint(br));

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
            /// Unknown.
            /// </summary>
            public int Unk2C { get; set; }

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
            /// If specified, the region is only active when the part is loaded.
            /// </summary>
            public string ActivationPartName { get; set; }
            public int ActivationPartIndex { get; set; }

            /// <summary>
            /// Identifies the region in event scripts.
            /// </summary>
            public int EntityID { get; set; }

            private protected Region(string name)
            {
                Name = name;
                Shape = new MSB.Shape.Point();
                MapStudioLayer = 0xFFFFFFFF;
                UnkA = new List<short>();
                UnkB = new List<short>();
                EntityID = -1;
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
                Unk2C = br.ReadInt32();
                long baseDataOffset1 = br.ReadInt64();
                long baseDataOffset2 = br.ReadInt64();
                br.AssertInt32(-1);
                MapStudioLayer = br.ReadUInt32();
                long shapeDataOffset = br.ReadInt64();
                long baseDataOffset3 = br.ReadInt64();
                long typeDataOffset = br.ReadInt64();

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
                EntityID = br.ReadInt32();

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
                bw.ReserveInt64("NameOffset");
                bw.WriteUInt32((uint)Type);
                bw.WriteInt32(id);
                bw.WriteUInt32((uint)Shape.Type);
                bw.WriteVector3(Position);
                bw.WriteVector3(Rotation);
                bw.WriteInt32(Unk2C);
                bw.ReserveInt64("BaseDataOffset1");
                bw.ReserveInt64("BaseDataOffset2");
                bw.WriteInt32(-1);
                bw.WriteUInt32(MapStudioLayer);
                bw.ReserveInt64("ShapeDataOffset");
                bw.ReserveInt64("BaseDataOffset3");
                bw.ReserveInt64("TypeDataOffset");

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

                bw.FillInt64("BaseDataOffset3", bw.Position - start);
                bw.WriteInt32(ActivationPartIndex);
                bw.WriteInt32(EntityID);

                if (HasTypeData)
                {
                    if (Type == RegionType.SoundSpaceOverride || Type == RegionType.PartsGroupArea || Type == RegionType.AutoDrawGroupPoint)
                        bw.Pad(8);

                    bw.FillInt64("TypeDataOffset", bw.Position - start);
                    WriteTypeData(bw);
                }
                else
                {
                    bw.FillInt64("TypeDataOffset", 0);
                }
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
                public int UnkT0C { get; set; }

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
                /// Unknown.
                /// </summary>
                public int UnkT1C { get; set; }

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
                /// Creates an EnvironmentMapPoint with default values.
                /// </summary>
                public EnvironmentMapPoint() : base($"{nameof(Region)}: {nameof(EnvironmentMapPoint)}") { }

                internal EnvironmentMapPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadSingle();
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(-1);
                    UnkT0C = br.ReadInt32();
                    UnkT10 = br.ReadSingle();
                    UnkT14 = br.ReadSingle();
                    UnkT18 = br.ReadInt32();
                    UnkT1C = br.ReadInt32();
                    UnkT20 = br.ReadInt32();
                    UnkT24 = br.ReadInt32();
                    UnkT28 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertPattern(0x10, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteSingle(UnkT00);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(UnkT0C);
                    bw.WriteSingle(UnkT10);
                    bw.WriteSingle(UnkT14);
                    bw.WriteInt32(UnkT18);
                    bw.WriteInt32(UnkT1C);
                    bw.WriteInt32(UnkT20);
                    bw.WriteInt32(UnkT24);
                    bw.WriteInt32(UnkT28);
                    bw.WriteInt32(-1);
                    bw.WritePattern(0x10, 0x00);
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
                /// Unknown.
                /// </summary>
                public int UnkT48 { get; set; }

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
                    UnkT48 = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(SoundType);
                    bw.WriteInt32(SoundID);
                    bw.WriteInt32s(ChildRegionIndices);
                    bw.WriteInt32(UnkT48);
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
                /// Unknown.
                /// </summary>
                public int UnkT04 { get; set; }

                /// <summary>
                /// Whether the effect is off until activated.
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
                    UnkT04 = br.ReadInt32();
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    br.AssertInt32(-1);
                    StartDisabled = br.ReadInt32();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WriteInt32(UnkT04);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
                    bw.WriteInt32(-1);
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
                public int WindAreaIndex { get; set; }

                /// <summary>
                /// Unknown.
                /// </summary>
                public float UnkT18 { get; set; }

                /// <summary>
                /// Creates a WindSFX with default values.
                /// </summary>
                public WindSFX() : base($"{nameof(Region)}: {nameof(WindSFX)}") { }

                internal WindSFX(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    EffectID = br.ReadInt32();
                    br.AssertPattern(0x10, 0xFF);
                    WindAreaIndex = br.ReadInt32();
                    UnkT18 = br.ReadSingle();
                    br.AssertInt32(0);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt32(EffectID);
                    bw.WritePattern(0x10, 0xFF);
                    bw.WriteInt32(WindAreaIndex);
                    bw.WriteSingle(UnkT18);
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
            /// A point along an NPC patrol path.
            /// </summary>
            public class PatrolRoute : Region
            {
                private protected override RegionType Type => RegionType.PatrolRoute;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a PatrolRoute with default values.
                /// </summary>
                public PatrolRoute() : base($"{nameof(Region)}: {nameof(PatrolRoute)}") { }

                internal PatrolRoute(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A point the player can be warped to.
            /// </summary>
            public class WarpPoint : Region
            {
                private protected override RegionType Type => RegionType.WarpPoint;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a WarpPoint with default values.
                /// </summary>
                public WarpPoint() : base($"{nameof(Region)}: {nameof(WarpPoint)}") { }

                internal WarpPoint(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// An area that triggers enemies when entered.
            /// </summary>
            public class ActivationArea : Region
            {
                private protected override RegionType Type => RegionType.ActivationArea;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an ActivationArea with default values.
                /// </summary>
                public ActivationArea() : base($"{nameof(Region)}: {nameof(ActivationArea)}") { }

                internal ActivationArea(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// A generic area used by event scripts.
            /// </summary>
            public class Event : Region
            {
                private protected override RegionType Type => RegionType.Event;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates an Event with default values.
                /// </summary>
                public Event() : base($"{nameof(Region)}: {nameof(Event)}") { }

                internal Event(BinaryReaderEx br) : base(br) { }
            }

            /// <summary>
            /// Unknown.
            /// </summary>
            public class Logic : Region
            {
                private protected override RegionType Type => RegionType.Logic;
                private protected override bool HasTypeData => false;

                /// <summary>
                /// Creates a Logic with default values.
                /// </summary>
                public Logic() : base($"{nameof(Region)}: {nameof(Logic)}") { }

                internal Logic(BinaryReaderEx br) : base(br) { }
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
                /// Unknown.
                /// </summary>
                public int UnkT24 { get; set; }

                /// <summary>
                /// Strength of ambient diffuse light.
                /// </summary>
                public float AmbientDiffuseIntensity { get; set; }

                /// <summary>
                /// Strength of ambient specular light.
                /// </summary>
                public float AmbientSpecularIntensity { get; set; }

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
                    UnkT24 = br.ReadInt32();
                    AmbientDiffuseIntensity = br.ReadSingle();
                    AmbientSpecularIntensity = br.ReadSingle();
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
                    bw.WriteInt32(UnkT24);
                    bw.WriteSingle(AmbientDiffuseIntensity);
                    bw.WriteSingle(AmbientSpecularIntensity);
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
                /// Creates a MufflingBox with default values.
                /// </summary>
                public MufflingBox() : base($"{nameof(Region)}: {nameof(MufflingBox)}") { }

                internal MufflingBox(BinaryReaderEx br) : base(br) { }

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
            public class SoundSpaceOverride : Region
            {
                private protected override RegionType Type => RegionType.SoundSpaceOverride;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown, probably a soundspace type.
                /// </summary>
                public byte UnkT00 { get; set; }

                /// <summary>
                /// Unknown, probably a soundspace type.
                /// </summary>
                public byte UnkT01 { get; set; }

                /// <summary>
                /// Creates a SoundSpaceOverride with default values.
                /// </summary>
                public SoundSpaceOverride() : base($"{nameof(Region)}: {nameof(SoundSpaceOverride)}") { }

                internal SoundSpaceOverride(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadByte();
                    UnkT01 = br.ReadByte();
                    br.AssertPattern(0x1E, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteByte(UnkT00);
                    bw.WriteByte(UnkT01);
                    bw.WritePattern(0x1E, 0x00);
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
            /// Unknown.
            /// </summary>
            public class PartsGroupArea : Region
            {
                private protected override RegionType Type => RegionType.PartsGroupArea;
                private protected override bool HasTypeData => true;

                /// <summary>
                /// Unknown.
                /// </summary>
                public long UnkT00 { get; set; }

                /// <summary>
                /// Creates a PartsGroupArea with default values.
                /// </summary>
                public PartsGroupArea() : base($"{nameof(Region)}: {nameof(PartsGroupArea)}") { }

                internal PartsGroupArea(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt64();
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt64(UnkT00);
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
                public long UnkT00 { get; set; }

                /// <summary>
                /// Creates an AutoDrawGroupPoint with default values.
                /// </summary>
                public AutoDrawGroupPoint() : base($"{nameof(Region)}: {nameof(AutoDrawGroupPoint)}") { }

                internal AutoDrawGroupPoint(BinaryReaderEx br) : base(br) { }

                private protected override void ReadTypeData(BinaryReaderEx br)
                {
                    UnkT00 = br.ReadInt64();
                    br.AssertPattern(0x18, 0x00);
                }

                private protected override void WriteTypeData(BinaryWriterEx bw)
                {
                    bw.WriteInt64(UnkT00);
                    bw.WritePattern(0x18, 0x00);
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
