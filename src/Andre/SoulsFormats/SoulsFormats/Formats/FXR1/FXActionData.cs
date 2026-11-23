using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace SoulsFormats
{
    public partial class FXR1
    {
        public class ResourceEntry
        {
            [XmlAttribute]
            public int Unk;

            public FXField Data;
        }

        [XmlInclude(typeof(ClusterAppearancePointSpriteActionData27))]
        [XmlInclude(typeof(ClusterEmitterConeActionData28))]
        [XmlInclude(typeof(ClusterEmitterSquareActionData29))]
        [XmlInclude(typeof(ClusterEmitterCircleActionData30))]
        [XmlInclude(typeof(ClusterEmitterSphereActionData31))]
        [XmlInclude(typeof(ClusterEmitterBoxActionData32))]
        [XmlInclude(typeof(ParticleAppearanceTracerActionData40))]
        [XmlInclude(typeof(ParticleAppearanceDistortionActionData43))]
        [XmlInclude(typeof(ClusterMovementAccelerationActionData55))]
        [XmlInclude(typeof(ParticleAppearanceBillboardActionData59))]
        [XmlInclude(typeof(FXActionData61))]
        [XmlInclude(typeof(FXActionData66))]
        [XmlInclude(typeof(ClusterAppearanceMultiTexBillboardActionData70))]
        [XmlInclude(typeof(ClusterAppearanceBillboardActionData71))]
        [XmlInclude(typeof(ClusterMovementWobblingActionData84))]
        [XmlInclude(typeof(ClusterMovementPartialFollowActionData105))]
        [XmlInclude(typeof(ParticleAppearanceRadialBlurActionData107))]
        [XmlInclude(typeof(Particle3DActionData108))]
        [XmlInclude(typeof(FXActionData117))]
        [XmlInclude(typeof(ActionDataRef))]
        public abstract class FXActionData : XIDable
        {
            public override bool ShouldSerializeXID() => FXR1.FlattenFXActionDatas;

            [XmlIgnore]
            public abstract int Type { get; }

            [XmlIgnore] // Set automatically during parent FXContainer's Write()
            public FXContainer ParentContainer;

            public List<ResourceEntry> Resources;

            [XmlIgnore]
            internal int DEBUG_SizeOnRead = -1;

            public virtual bool ShouldSerializeResources() => true;

            internal abstract void InnerRead(BinaryReaderEx br, FxrEnvironment env);
            internal abstract void InnerWrite(BinaryWriterEx bw, FxrEnvironment env);

            internal override void ToXIDs(FXR1 fxr)
            {
                ParentContainer = fxr.ReferenceFXContainer(ParentContainer);
                InnerToXIDs(fxr);
            }

            internal override void FromXIDs(FXR1 fxr)
            {
                ParentContainer = fxr.DereferenceFXContainer(ParentContainer);
                InnerFromXIDs(fxr);
            }

            internal virtual void InnerToXIDs(FXR1 fxr)
            {

            }

            internal virtual void InnerFromXIDs(FXR1 fxr)
            {

            }

            /// <summary>
            /// Per ActionData type: the field slots (by offset from the ActionData's start) in the order their data is
            /// written. Learned from every vanilla DS1 / DS1R ActionData by Claude/Tools/FfxSurvey --fxr1fieldorder
            /// (each type's order holds in 100 % of them); types not listed write their data in field order.
            /// </summary>
            internal static readonly Dictionary<int, int[]> FieldDataOrderNarrow = new Dictionary<int, int[]>
            {
                [28] = new[] { 60, 28, 36, 44 },
                [30] = new[] { 76, 28, 36, 44, 52 },
                [31] = new[] { 72, 28, 36, 44, 52 },
                [43] = new[] { 64, 72, 80, 88, 112, 136, 144, 152, 160, 96, 104, 120, 128 },
                [61] = new[] { 224, 232, 240, 56, 64, 72, 132, 140, 148, 156, 164, 172, 180, 188 },
                [70] = new[] { 72, 80, 88, 96, 104, 112, 120, 128, 136, 144, 248, 256, 264, 272, 280, 288, 296, 304, 160, 168, 176, 184, 192, 200, 208, 216, 224, 232, 240 },
                [71] = new[] { 288, 308, 316, 324, 332, 340, 348, 68, 76, 84, 92, 100, 108, 116, 124, 132, 140, 172, 180, 188, 196, 204, 212, 220, 228, 156, 164 },
                [108] = new[] { 312, 316, 320, 324, 332, 340, 348, 356, 68, 76, 84, 92, 100, 108, 116, 124, 132, 140, 148, 156, 172, 180, 188, 196, 204, 212, 220, 228, 236, 244, 252, 260, 268, 276 },
            };

            /// <summary>The same for DS1R's 64-bit layout.</summary>
            internal static readonly Dictionary<int, int[]> FieldDataOrderWide = new Dictionary<int, int[]>
            {
                [27] = new[] { 88, 104, 120, 136, 152, 168, 184, 200, 216, 232, 264, 280, 296, 328, 312 },
                [28] = new[] { 108, 48, 64, 80 },
                [32] = new[] { 156, 48, 64, 80, 96, 112, 128 },
                [40] = new[] { 80, 96, 128, 168, 184, 200, 216, 248, 272, 288, 312, 328, 344, 376, 360 },
                [43] = new[] { 88, 104, 120, 136, 184, 232, 248, 264, 280, 152, 168, 200, 216 },
                [59] = new[] { 72, 88, 104, 120, 176, 224, 240, 256, 272, 312, 328, 344, 376, 360 },
                [70] = new[] { 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 448, 464, 480, 496, 512, 528, 544, 560, 256, 272, 288, 304, 320, 336, 352, 368, 384, 400, 416, 432, 640, 656, 672, 704, 688 },
                [71] = new[] { 96, 112, 128, 144, 160, 176, 192, 208, 224, 240, 296, 312, 328, 344, 360, 376, 392, 408, 264, 280, 488, 504, 520, 552, 536 },
            };

            private FxrEnvironment currentWriteEnvironment = null;
            private Dictionary<FXField, List<long>> fieldWriteLocations = new Dictionary<FXField, List<long>>();

            internal void WriteField(FXField p)
            {
                WriteFXR1Varint(currentWriteEnvironment.bw, p.Type);

                if (p.IsEmptyPointer())
                {
                    currentWriteEnvironment.bw.WriteUInt32(0);
                }
                else
                {
                    if (!fieldWriteLocations.ContainsKey(p))
                        fieldWriteLocations.Add(p, new List<long>());

                    if (!fieldWriteLocations[p].Contains(currentWriteEnvironment.bw.Position))
                        fieldWriteLocations[p].Add(currentWriteEnvironment.bw.Position);

                    currentWriteEnvironment.RegisterPointerOffset(currentWriteEnvironment.bw.Position);

                    currentWriteEnvironment.bw.WriteUInt32(0xEEEEEEEE);
                }

                // garbage on end of offset to packed data
                WriteFXR1Garbage(currentWriteEnvironment.bw);
            }

            internal void Write(BinaryWriterEx bw, FxrEnvironment env)
            {
                env.RegisterOffset(bw.Position, this);

                long startPos = bw.Position;

                bw.WriteInt32(Type);
                bw.ReserveInt32("ActionData.Size");
                WriteFXR1Varint(bw, Resources.Count);

                env.RegisterPointerOffset(bw.Position);
                ReserveFXR1Varint(bw, "ActionData.Resources.Numbers");

                env.RegisterPointerOffset(bw.Position);
                ReserveFXR1Varint(bw, "ActionData.Resources.Datas");

                env.RegisterPointer(ParentContainer, useExistingPointerOnly: true, assertNotNull: true);

                fieldWriteLocations.Clear();
                currentWriteEnvironment = env;
                InnerWrite(bw, env);

                if (bw.VarintLong)
                    bw.Pad(8);

                FillFXR1Varint(bw, "ActionData.Resources.Numbers", (int)bw.Position);
                for (int i = 0; i < Resources.Count; i++)
                {
                    bw.WriteInt32(Resources[i].Unk);
                }

                FillFXR1Varint(bw, "ActionData.Resources.Datas", (int)bw.Position);
                for (int i = 0; i < Resources.Count; i++)
                {
                    WriteField(Resources[i].Data);
                }

                // The field data comes in the order the game's tool wrote it, not always field order (FFX plan §17)
                var table = (bw.VarintLong ? FieldDataOrderWide : FieldDataOrderNarrow).GetValueOrDefault(Type);
                var ordered = fieldWriteLocations.Select((kvp, i) => (kvp, i, rel: (int)(kvp.Value.Min() - startPos)))
                    .OrderBy(x => table == null ? x.i : Array.IndexOf(table, x.rel) is int r && r >= 0 ? r : table.Length + x.i)
                    .Select(x => x.kvp).ToList();
                foreach (var kvp in ordered)
                {
                    long offsetOfThisField = bw.Position;

                    foreach (var location in kvp.Value)
                    {
                        bw.StepIn(location);
                        WriteFXR1Varint(bw, (int)offsetOfThisField);
                        bw.StepOut();
                    }

                    kvp.Key.InnerWrite(bw, env); 
                }

                int writtenSize = (int)(bw.Position - startPos);

                if (DEBUG_SizeOnRead != -1 && writtenSize != DEBUG_SizeOnRead)
                {
                    //throw new Exception("sdfsgfdsgfds");
                    //Console.WriteLine($"Warning: ActionDataType[{this.GetType().Name}] Read data size {DEBUG_SizeOnRead} but wrote {writtenSize} bytes.");
                }

                bw.FillInt32("ActionData.Size", writtenSize);

                bw.Pad(16); //Might be 16?

                fieldWriteLocations.Clear();
                currentWriteEnvironment = null;
            }

            internal static FXActionData Read(BinaryReaderEx br, FxrEnvironment env)
            {
                long startOffset = br.Position;

                int subType = br.ReadInt32();
                int size = br.ReadInt32();
                int preDataCount = ReadFXR1Varint(br);
                int offsetToResourceNumbers = ReadFXR1Varint(br);
                int offsetToResourceNodes = ReadFXR1Varint(br);

                int offsetToParentEffect = ReadFXR1Varint(br);
                var parentEffect = env.GetFXContainer(br, offsetToParentEffect);

                FXActionData data;

                switch (subType)
                {
                    case 27: data = new ClusterAppearancePointSpriteActionData27(); break;
                    case 28: data = new ClusterEmitterConeActionData28(); break;
                    case 29: data = new ClusterEmitterSquareActionData29(); break;
                    case 30: data = new ClusterEmitterCircleActionData30(); break;
                    case 31: data = new ClusterEmitterSphereActionData31(); break;
                    case 32: data = new ClusterEmitterBoxActionData32(); break;
                    case 40: data = new ParticleAppearanceTracerActionData40(); break;
                    case 43: data = new ParticleAppearanceDistortionActionData43(); break;
                    case 55: data = new ClusterMovementAccelerationActionData55(); break;
                    case 59: data = new ParticleAppearanceBillboardActionData59(); break;
                    case 61: data = new FXActionData61(); break;
                    case 66: data = new FXActionData66(); break;
                    case 70: data = new ClusterAppearanceMultiTexBillboardActionData70(); break;
                    case 71: data = new ClusterAppearanceBillboardActionData71(); break;
                    case 84: data = new ClusterMovementWobblingActionData84(); break;
                    case 105: data = new ClusterMovementPartialFollowActionData105(); break;
                    case 107: data = new ParticleAppearanceRadialBlurActionData107(); break;
                    case 108: data = new Particle3DActionData108(); break;
                    case 117: data = new FXActionData117(); break;
                    default: throw new NotImplementedException();
                }

                env.RegisterOffset(startOffset, data);

                //Testing
                data.DEBUG_SizeOnRead = size;

                data.InnerRead(br, env);

                data.ParentContainer = parentEffect;

                data.Resources = new List<ResourceEntry>(preDataCount);

                br.StepIn(offsetToResourceNumbers);
                for (int i = 0; i < preDataCount; i++)
                {
                    data.Resources.Add(new ResourceEntry()
                    {
                        Unk = br.ReadInt32()
                    });
                }
                br.StepOut();

                br.StepIn(offsetToResourceNodes);
                for (int i = 0; i < preDataCount; i++)
                {
                    data.Resources[i].Data = FXField.Read(br, env);
                }
                br.StepOut();

                br.Position = startOffset + size;

                return data;
            }

            public class ClusterAppearancePointSpriteActionData27 : FXActionData
            {
                public override int Type => 27;

                public float Duration;
                public float DurationMult;
                public float DurationVariance;
                public float RenderDepth;
                public int TextureID;
                public int BlendMode;
                public FXField Scale;
                public FXField ScaleMult;
                public FXField Color1R;
                public FXField Color1G;
                public FXField Color1B;
                public FXField Color1A;
                public FXField Color2R;
                public FXField Color2G;
                public FXField Color2B;
                public FXField Color2A;
                public int Unk8;
                public int Unk9;
                public int Unk10;
                public float Unk11;
                public DS1RExtraNodes DS1RData;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Duration = br.ReadSingle();
                    DurationMult = br.ReadSingle();
                    DurationVariance = br.ReadSingle();

                    br.AssertInt32(0);

                    RenderDepth = br.ReadSingle();

                    if (br.VarintLong)
                        br.AssertInt32(0);

                    TextureID = br.ReadInt32();
                    BlendMode = br.ReadInt32();

                    AssertFXR1Varint(br, 0);

                    Scale = FXField.Read(br, env);
                    ScaleMult = FXField.Read(br, env);
                    Color1R = FXField.Read(br, env);
                    Color1G = FXField.Read(br, env);
                    Color1B = FXField.Read(br, env);
                    Color1A = FXField.Read(br, env);
                    Color2R = FXField.Read(br, env);
                    Color2G = FXField.Read(br, env);
                    Color2B = FXField.Read(br, env);
                    Color2A = FXField.Read(br, env);

                    Unk8 = br.ReadInt32();
                    Unk9 = br.ReadInt32();
                    Unk10 = br.ReadInt32();
                    Unk11 = br.ReadSingle();

                    if (br.VarintLong)
                        DS1RData = DS1RExtraNodes.Read(br, env);
                    
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Duration);
                    bw.WriteSingle(DurationMult);
                    bw.WriteSingle(DurationVariance);

                    bw.WriteInt32(0);

                    bw.WriteSingle(RenderDepth);

                    if (bw.VarintLong)
                        bw.WriteInt32(0);

                    bw.WriteInt32(TextureID);
                    bw.WriteInt32(BlendMode);

                    WriteFXR1Varint(bw, 0);

                    WriteField(Scale);
                    WriteField(ScaleMult);
                    WriteField(Color1R);
                    WriteField(Color1G);
                    WriteField(Color1B);
                    WriteField(Color1A);
                    WriteField(Color2R);
                    WriteField(Color2G);
                    WriteField(Color2B);
                    WriteField(Color2A);
                    bw.WriteInt32(Unk8);
                    bw.WriteInt32(Unk9);
                    bw.WriteInt32(Unk10);
                    bw.WriteSingle(Unk11);

                    if (bw.VarintLong)
                        DS1RData.Write(bw, this);
                }
            }



            public class ClusterEmitterConeActionData28 : FXActionData
            {
                public override int Type => 28;

                public FXField EmitterDegree;
                public FXField EmitterSpread;
                public FXField EmitterSpeed;
                public int EmitterDirectionMode;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    EmitterDegree = FXField.Read(br, env);
                    EmitterSpread = FXField.Read(br, env);
                    EmitterSpeed = FXField.Read(br, env);
                    EmitterDirectionMode = ReadFXR1Varint(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(EmitterDegree);
                    WriteField(EmitterSpread);
                    WriteField(EmitterSpeed);
                    WriteFXR1Varint(bw, EmitterDirectionMode);
                }
            }


            public class ClusterEmitterSquareActionData29 : FXActionData
            {
                public override int Type => 29;

                public FXField EmitterSize;
                public FXField EmitterDegree;
                public FXField EmitterSpread;
                public FXField EmitterDistributionMode;
                public FXField EmitterSpeed;
                public int EmitterDirectionMode;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    EmitterSize = FXField.Read(br, env);
                    EmitterDegree = FXField.Read(br, env);
                    EmitterSpread = FXField.Read(br, env);
                    EmitterDistributionMode = FXField.Read(br, env);
                    EmitterSpeed = FXField.Read(br, env);
                    EmitterDirectionMode = br.ReadInt32();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(EmitterSize);
                    WriteField(EmitterDegree);
                    WriteField(EmitterSpread);
                    WriteField(EmitterDistributionMode);
                    WriteField(EmitterSpeed);
                    bw.WriteInt32(EmitterDirectionMode);
                }
            }

            public class ClusterEmitterCircleActionData30 : FXActionData
            {
                public override int Type => 30;

                public FXField EmitterRadius;
                public FXField EmitterDegree;
                public FXField EmitterSpread;
                public FXField EmitterSpeed;
                public float EmitterDistribution;
                public int EmitterDistributionMode;
                public int EmitterDirectionMode;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    EmitterRadius = FXField.Read(br, env);
                    EmitterDegree = FXField.Read(br, env);
                    EmitterSpread = FXField.Read(br, env);
                    EmitterSpeed = FXField.Read(br, env);
                    EmitterDistribution = br.ReadSingle();
                    EmitterDistributionMode = br.ReadInt32();
                    EmitterDirectionMode = ReadFXR1Varint(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(EmitterRadius);
                    WriteField(EmitterDegree);
                    WriteField(EmitterSpread);
                    WriteField(EmitterSpeed);
                    bw.WriteSingle(EmitterDistribution);
                    bw.WriteInt32(EmitterDistributionMode);
                    WriteFXR1Varint(bw, EmitterDirectionMode);
                }
            }


            public class ClusterEmitterSphereActionData31 : FXActionData
            {
                public override int Type => 31;

                public FXField EmitterRadius;
                public FXField EmitterDegree;
                public FXField EmitterSpread;
                public FXField EmitterSpeed;
                public int EmitterDistributionMode;
                public int EmitterMovementMode;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    EmitterRadius = FXField.Read(br, env);
                    EmitterDegree = FXField.Read(br, env);
                    EmitterSpread = FXField.Read(br, env);
                    EmitterSpeed = FXField.Read(br, env);
                    EmitterDistributionMode = br.ReadInt32();
                    EmitterMovementMode = br.ReadInt32();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(EmitterRadius);
                    WriteField(EmitterDegree);
                    WriteField(EmitterSpread);
                    WriteField(EmitterSpeed);
                    bw.WriteInt32(EmitterDistributionMode);
                    bw.WriteInt32(EmitterMovementMode);
                }
            }

            public class ClusterEmitterBoxActionData32 : FXActionData
            {
                public override int Type => 32;

                public FXField EmitterSizeX;
                public FXField EmitterSizeY;
                public FXField EmitterSizeZ;
                public FXField EmitterDegree;
                public FXField EmitterSpread;
                public FXField EmitterSpeed;
                public int EmitterDistributionMode;
                public int EmitterMovementMode;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    EmitterSizeX = FXField.Read(br, env);
                    EmitterSizeY = FXField.Read(br, env);
                    EmitterSizeZ = FXField.Read(br, env);
                    EmitterDegree = FXField.Read(br, env);
                    EmitterSpread = FXField.Read(br, env);
                    EmitterSpeed = FXField.Read(br, env);
                    EmitterDistributionMode = br.ReadInt32();
                    EmitterMovementMode = br.ReadInt32();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(EmitterSizeX);
                    WriteField(EmitterSizeY);
                    WriteField(EmitterSizeZ);
                    WriteField(EmitterDegree);
                    WriteField(EmitterSpread);
                    WriteField(EmitterSpeed);
                    bw.WriteInt32(EmitterDistributionMode);
                    bw.WriteInt32(EmitterMovementMode);
                }
            }

            public class ParticleAppearanceTracerActionData40 : FXActionData
            {
                public override int Type => 40;

                public float DS1R_Unk0;
                public float Duration;
                public int TextureID;
                public int OrientationMode;
                public int BlendMode;
                public int TrailThickness;
                public FXField Scale1X;
                public FXField Scale1Y;
                public FXField Scale2X;
                public FXField Scale2Y;
                public float Unk7;
                public float TrailMaxTime;
                public int TrailLength;
                public int Unk10;
                public FXField ColorR;
                public FXField ColorG;
                public FXField ColorB;
                public FXField ColorA;
                public int Unk12;
                public int Unk13;
                public FXField Unk14;
                public int Unk15;
                public float Unk16;
                public FXField TrailFadeIn;
                public FXField TrailFadeOut;
                public int TrailFadeMode;

                public DS1RExtraNodes DS1RData;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    DS1R_Unk0 = br.ReadSingle();
                    Duration = br.ReadSingle();
                    TextureID = br.ReadInt32();

                    br.AssertInt32(0);

                    OrientationMode = br.ReadInt32();
                    BlendMode = br.ReadInt32();
                    TrailThickness = ReadFXR1Varint(br);
                    Scale1X = FXField.Read(br, env);
                    Scale1Y = FXField.Read(br, env);
                    Scale2X = FXField.Read(br, env);
                    Scale2Y = FXField.Read(br, env);
                    Unk7 = br.ReadSingle();
                    TrailMaxTime = br.ReadSingle();
                    TrailLength = br.ReadInt32();
                    Unk10 = br.ReadInt32();

                    AssertFXR1Varint(br, 0);

                    ColorR = FXField.Read(br, env);
                    ColorG = FXField.Read(br, env);
                    ColorB = FXField.Read(br, env);
                    ColorA = FXField.Read(br, env);
                    Unk12 = br.ReadInt32();
                    Unk13 = br.ReadInt32();

                    AssertFXR1Varint(br, 0);

                    Unk14 = FXField.Read(br, env);
                    Unk15 = br.ReadInt32();
                    Unk16 = br.ReadSingle();
                    TrailFadeIn = FXField.Read(br, env);
                    TrailFadeOut = FXField.Read(br, env);
                    TrailFadeMode = ReadFXR1Varint(br);

                    if (br.VarintLong)
                        DS1RData = DS1RExtraNodes.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(DS1R_Unk0);
                    bw.WriteSingle(Duration);
                    bw.WriteInt32(TextureID);

                    bw.WriteInt32(0);

                    bw.WriteInt32(OrientationMode);
                    bw.WriteInt32(BlendMode);
                    WriteFXR1Varint(bw, TrailThickness);
                    WriteField(Scale1X);
                    WriteField(Scale1Y);
                    WriteField(Scale2X);
                    WriteField(Scale2Y);
                    bw.WriteSingle(Unk7);
                    bw.WriteSingle(TrailMaxTime);
                    bw.WriteInt32(TrailLength);
                    bw.WriteInt32(Unk10);

                    WriteFXR1Varint(bw, 0);

                    WriteField(ColorR);
                    WriteField(ColorG);
                    WriteField(ColorB);
                    WriteField(ColorA);
                    bw.WriteInt32(Unk12);
                    bw.WriteInt32(Unk13);

                    WriteFXR1Varint(bw, 0);

                    WriteField(Unk14);
                    bw.WriteInt32(Unk15);
                    bw.WriteSingle(Unk16);
                    WriteField(TrailFadeIn);
                    WriteField(TrailFadeOut);
                    WriteFXR1Varint(bw, TrailFadeMode);

                    if (bw.VarintLong)
                        DS1RData.Write(bw, this);
                }
            }

            public class ParticleAppearanceDistortionActionData43 : FXActionData
            {
                public override int Type => 43;

                public float Unk1;
                public int TextureID1;
                public int TextureID2;
                public int DistortionMode;
                public int ShapeMode;
                public int OrientationMode;
                public int BlendMode;
                public FXField ScaleX;
                public FXField ScaleY;
                public FXField ScaleZ;
                public FXField StretchingDistance;
                public FXField DistortionIntensity;
                public FXField WobblingSpeed;
                public FXField WobblingRadius;
                public FXField WaveSpreed;
                public FXField FlowSpeed;
                public FXField ColorR;
                public FXField ColorG;
                public FXField ColorB;
                public FXField ColorA;
                public int Unk8;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Unk1 = br.ReadSingle();
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    TextureID1 = br.ReadInt32();
                    TextureID2 = br.ReadInt32();
                    DistortionMode = br.ReadInt32();
                    ShapeMode = br.ReadInt32();
                    OrientationMode = br.ReadInt32();
                    BlendMode = ReadFXR1Varint(br);
                    ScaleX = FXField.Read(br, env);
                    ScaleY = FXField.Read(br, env);
                    ScaleZ = FXField.Read(br, env);
                    StretchingDistance = FXField.Read(br, env);
                    DistortionIntensity = FXField.Read(br, env);
                    WobblingSpeed = FXField.Read(br, env);
                    WobblingRadius = FXField.Read(br, env);
                    WaveSpreed = FXField.Read(br, env);
                    FlowSpeed = FXField.Read(br, env);
                    ColorR = FXField.Read(br, env);
                    ColorG = FXField.Read(br, env);
                    ColorB = FXField.Read(br, env);
                    ColorA = FXField.Read(br, env);
                    Unk8 = ReadFXR1Varint(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Unk1);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TextureID1);
                    bw.WriteInt32(TextureID2);
                    bw.WriteInt32(DistortionMode);
                    bw.WriteInt32(ShapeMode);
                    bw.WriteInt32(OrientationMode);
                    WriteFXR1Varint(bw, BlendMode);
                    WriteField(ScaleX);
                    WriteField(ScaleY);
                    WriteField(ScaleZ);
                    WriteField(StretchingDistance);
                    WriteField(DistortionIntensity);
                    WriteField(WobblingSpeed);
                    WriteField(WobblingRadius);
                    WriteField(WaveSpreed);
                    WriteField(FlowSpeed);
                    WriteField(ColorR);
                    WriteField(ColorG);
                    WriteField(ColorB);
                    WriteField(ColorA);
                    WriteFXR1Varint(bw, Unk8);
                }
            }

            public class ClusterMovementAccelerationActionData55 : FXActionData
            {
                public override int Type => 55;

                public FXField Gravity;
                public FXField MovementFalloff;
                public FXField MovementFalloffMult;
                public float PhaseShift;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Gravity = FXField.Read(br, env);
                    MovementFalloff = FXField.Read(br, env);
                    MovementFalloffMult = FXField.Read(br, env);

                    br.AssertInt32(0);

                    PhaseShift = br.ReadSingle();
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(Gravity);
                    WriteField(MovementFalloff);
                    WriteField(MovementFalloffMult);

                    bw.WriteInt32(0);

                    bw.WriteSingle(PhaseShift);
                }
            }

            public class ParticleAppearanceBillboardActionData59 : FXActionData
            {
                public override int Type => 59;

                public float Unk1;
                public int TextureID;
                public int OrientationMode;
                public int BlendMode;
                public FXField ScaleX;
                public FXField ScaleY;
                public FXField RotX;
                public FXField RotY;
                public FXField RotZ;
                public int AnimFrameSliceCountPerRow;
                public int AnimFrameTotalCount;
                public FXField AnimFrameInitial;
                public FXField AnimFrameTimeline;
                public FXField UVShiftX;
                public FXField UVShiftY;
                public FXField ColorR;
                public FXField ColorG;
                public FXField ColorB;
                public FXField ColorA;
                public int Unk8;
                public int Unk9;
                public int Unk10;
                public float Unk11;
                public int DS1R_Unk12;

                public DS1RExtraNodes DS1RData;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Unk1 = br.ReadSingle();

                    br.AssertInt32(0);

                    TextureID = br.ReadInt32();

                    br.AssertInt32(0);

                    OrientationMode = br.ReadInt32();
                    BlendMode = br.ReadInt32();
                    ScaleX = FXField.Read(br, env);
                    ScaleY = FXField.Read(br, env);
                    RotX = FXField.Read(br, env);
                    RotY = FXField.Read(br, env);
                    RotZ = FXField.Read(br, env);
                    AnimFrameSliceCountPerRow = br.ReadInt32();
                    AnimFrameTotalCount = br.ReadInt32();
                    AnimFrameInitial = FXField.Read(br, env);
                    AnimFrameTimeline = FXField.Read(br, env);
                    UVShiftX = FXField.Read(br, env);
                    UVShiftY = FXField.Read(br, env);
                    ColorR = FXField.Read(br, env);
                    ColorG = FXField.Read(br, env);
                    ColorB = FXField.Read(br, env);
                    ColorA = FXField.Read(br, env);
                    Unk8 = br.ReadInt32();
                    Unk9 = br.ReadInt32();

                    br.AssertInt32(0);

                    Unk10 = br.ReadInt32();
                    Unk11 = br.ReadSingle();

                    DS1R_Unk12 = br.ReadInt32();

                    if (br.VarintLong)
                        DS1RData = DS1RExtraNodes.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Unk1);

                    bw.WriteInt32(0);

                    bw.WriteInt32(TextureID);

                    bw.WriteInt32(0);

                    bw.WriteInt32(OrientationMode);
                    bw.WriteInt32(BlendMode);
                    WriteField(ScaleX);
                    WriteField(ScaleY);
                    WriteField(RotX);
                    WriteField(RotY);
                    WriteField(RotZ);
                    bw.WriteInt32(AnimFrameSliceCountPerRow);
                    bw.WriteInt32(AnimFrameTotalCount);
                    WriteField(AnimFrameInitial);
                    WriteField(AnimFrameTimeline);
                    WriteField(UVShiftX);
                    WriteField(UVShiftY);
                    WriteField(ColorR);
                    WriteField(ColorG);
                    WriteField(ColorB);
                    WriteField(ColorA);
                    bw.WriteInt32(Unk8);
                    bw.WriteInt32(Unk9);

                    bw.WriteInt32(0);

                    bw.WriteInt32(Unk10);
                    bw.WriteSingle(Unk11);

                    bw.WriteInt32(DS1R_Unk12);

                    if (bw.VarintLong)
                        DS1RData.Write(bw, this);
                }
            }

            public class FXActionData61 : FXActionData
            {
                public override int Type => 61;

                public int ModelID;
                public int Orientation;
                public int BlendMode;
                public int Unk3_1;
                public int Unk3_2;
                public FXField Scale1X;
                public FXField Scale1Y;
                public FXField Scale1Z;
                public int Unk5;
                public float Unk6;
                public FXField Unk7;
                public int AnimFrameSliceCountPerRow;
                public int AnimFrameTotalCount;
                public FXField AnimFrameInitial;
                public FXField AnimFrameTimeline;
                public FXField UVShiftX;
                public FXField UVShiftY;
                public FXField UVScrollX;
                public FXField UVScrollY;
                public FXField ColorR;
                public FXField ColorG;
                public FXField ColorB;
                public FXField ColorA;
                public int Unk11;
                public int Unk12;

                public DS1RExtraNodes DS1RData;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    ModelID = br.ReadInt32();
                    Orientation = br.ReadInt32();
                    BlendMode = br.ReadInt32();
                    Unk3_1 = br.ReadInt32();
                    Unk3_2 = ReadFXR1Varint(br);
                    Scale1X = FXField.Read(br, env);
                    Scale1Y = FXField.Read(br, env);
                    Scale1Z = FXField.Read(br, env);

                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    Unk5 = br.ReadInt32();
                    Unk6 = br.ReadSingle();

                    AssertFXR1Varint(br, 0);

                    Unk7 = FXField.Read(br, env);
                    AnimFrameSliceCountPerRow = br.ReadInt32();
                    AnimFrameTotalCount = br.ReadInt32();
                    AnimFrameInitial = FXField.Read(br, env);
                    AnimFrameTimeline = FXField.Read(br, env);
                    UVShiftX = FXField.Read(br, env);
                    UVShiftY = FXField.Read(br, env);
                    UVScrollX = FXField.Read(br, env);
                    UVScrollY = FXField.Read(br, env);
                    ColorR = FXField.Read(br, env);
                    ColorG = FXField.Read(br, env);
                    ColorB = FXField.Read(br, env);
                    ColorA = FXField.Read(br, env);
                    Unk11 = br.ReadInt32();
                    Unk12 = br.ReadInt32();

                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    if (br.VarintLong)
                        DS1RData = DS1RExtraNodes.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);

                    bw.WriteInt32(ModelID);
                    bw.WriteInt32(Orientation);
                    bw.WriteInt32(BlendMode);
                    bw.WriteInt32(Unk3_1);
                    WriteFXR1Varint(bw, Unk3_2);
                    WriteField(Scale1X);
                    WriteField(Scale1Y);
                    WriteField(Scale1Z);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);

                    bw.WriteInt32(Unk5);
                    bw.WriteSingle(Unk6);

                    WriteFXR1Varint(bw, 0);

                    WriteField(Unk7);
                    bw.WriteInt32(AnimFrameSliceCountPerRow);
                    bw.WriteInt32(AnimFrameTotalCount);
                    WriteField(AnimFrameInitial);
                    WriteField(AnimFrameTimeline);
                    WriteField(UVShiftX);
                    WriteField(UVShiftY);
                    WriteField(UVScrollX);
                    WriteField(UVScrollY);
                    WriteField(ColorR);
                    WriteField(ColorG);
                    WriteField(ColorB);
                    WriteField(ColorA);
                    bw.WriteInt32(Unk11);
                    bw.WriteInt32(Unk12);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);

                    if (bw.VarintLong)
                        DS1RData.Write(bw, this);
                }
            }


            public class FXActionData66 : FXActionData
            {
                public override int Type => 66;

                public float Duration;
                public float Unk1;
                public float DS1R_Unk2;
                public int AttachedBool;
                public float Unk4;
                public int Unk5;

                public int DS1R_Unk5B;

                public FXField Unk6_1;
                public FXField Unk6_2;
                public FXField Unk6_3;
                public FXField Unk6_4;
                public FXField Unk6_5;
                public FXField Unk6_6;
                public FXField Unk6_7;
                public FXField Unk6_8;
                public FXField Unk6_9;
                public FXField Unk6_10;
                public FXField Unk6_11;
                public FXField Unk6_12;
                public FXField Unk6_13;
                public FXField Unk6_14;
                public FXField Unk6_15;
                public FXField Unk6_16;
                public FXField Unk6_17;
                public FXField Unk6_18;
                public FXField Color1R;
                public FXField Color1G;
                public FXField Color1B;
                public FXField Color1A;
                public FXField Color2R;
                public FXField Color2G;
                public FXField Color2B;
                public FXField Color2A;

                public DS1RExtraNodes DS1RData;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Duration = br.ReadSingle();
                    Unk1 = br.ReadSingle();
                    DS1R_Unk2 = br.ReadSingle();
                    AttachedBool = br.ReadInt32();
                    Unk4 = br.ReadSingle();
                    Unk5 = br.ReadInt32();

                    if (br.VarintLong)
                        DS1R_Unk5B = ReadFXR1Varint(br);

                    Unk6_1 = FXField.Read(br, env);
                    Unk6_2 = FXField.Read(br, env);
                    Unk6_3 = FXField.Read(br, env);
                    Unk6_4 = FXField.Read(br, env);
                    Unk6_5 = FXField.Read(br, env);
                    Unk6_6 = FXField.Read(br, env);
                    Unk6_7 = FXField.Read(br, env);
                    Unk6_8 = FXField.Read(br, env);
                    Unk6_9 = FXField.Read(br, env);
                    Unk6_10 = FXField.Read(br, env);
                    Unk6_11 = FXField.Read(br, env);
                    Unk6_12 = FXField.Read(br, env);
                    Unk6_13 = FXField.Read(br, env);
                    Unk6_14 = FXField.Read(br, env);
                    Unk6_15 = FXField.Read(br, env);
                    Unk6_16 = FXField.Read(br, env);
                    Unk6_17 = FXField.Read(br, env);
                    Unk6_18 = FXField.Read(br, env);
                    Color1R = FXField.Read(br, env);
                    Color1G = FXField.Read(br, env);
                    Color1B = FXField.Read(br, env);
                    Color1A = FXField.Read(br, env);
                    Color2R = FXField.Read(br, env);
                    Color2G = FXField.Read(br, env);
                    Color2B = FXField.Read(br, env);
                    Color2A = FXField.Read(br, env);

                    br.AssertInt32(0);

                    if (br.VarintLong)
                    {
                        br.AssertInt32(0);
                        DS1RData = DS1RExtraNodes.Read(br, env);
                    }
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Duration);
                    bw.WriteSingle(Unk1);
                    bw.WriteSingle(DS1R_Unk2);
                    bw.WriteInt32(AttachedBool);
                    bw.WriteSingle(Unk4);
                    bw.WriteInt32(Unk5);

                    if (bw.VarintLong)
                        WriteFXR1Varint(bw, DS1R_Unk5B);

                    WriteField(Unk6_1);
                    WriteField(Unk6_2);
                    WriteField(Unk6_3);
                    WriteField(Unk6_4);
                    WriteField(Unk6_5);
                    WriteField(Unk6_6);
                    WriteField(Unk6_7);
                    WriteField(Unk6_8);
                    WriteField(Unk6_9);
                    WriteField(Unk6_10);
                    WriteField(Unk6_11);
                    WriteField(Unk6_12);
                    WriteField(Unk6_13);
                    WriteField(Unk6_14);
                    WriteField(Unk6_15);
                    WriteField(Unk6_16);
                    WriteField(Unk6_17);
                    WriteField(Unk6_18);
                    WriteField(Color1R);
                    WriteField(Color1G);
                    WriteField(Color1B);
                    WriteField(Color1A);
                    WriteField(Color2R);
                    WriteField(Color2G);
                    WriteField(Color2B);
                    WriteField(Color2A);

                    bw.WriteInt32(0);

                    if (bw.VarintLong)
                    {
                        bw.WriteInt32(0);
                        DS1RData.Write(bw, this);
                    }
                }
            }

            public class ClusterAppearanceMultiTexBillboardActionData70 : FXActionData
            {
                public override int Type => 70;

                public float Duration;
                public float DurationMult;
                public float LifetimeVariance;
                public int AttachedBool;
                public float RenderDepth;
                public int TextureID1;
                public int TextureID2;
                public int TextureID3;
                public int OrientationMode;
                public int BlendMode;
                public int Unk8;
                public FXField Scale1X;
                public FXField Scale1Y;
                public FXField Scale2X;
                public FXField Scale2Y;
                public FXField RotX;
                public FXField RotY;
                public FXField RotZ;
                public FXField RotSpeedX;
                public FXField RotSpeedY;
                public FXField RotSpeedZ;
                public FXField Tex1UVOffsetX;
                public FXField Tex1UVOffsetY;
                public FXField Tex1UVScrollX;
                public FXField Tex1UVScrollY;
                public FXField Tex2UVOffsetX;
                public FXField Tex2UVOffsetY;
                public FXField Tex2UVScrollX;
                public FXField Tex2UVScrollY;
                public FXField Tex3UVOffsetX;
                public FXField Tex3UVOffsetY;
                public FXField Tex3UVScrollX;
                public FXField Tex3UVScrollY;
                public FXField Color1R;
                public FXField Color1G;
                public FXField Color1B;
                public FXField Color1A;
                public FXField Color2R;
                public FXField Color2G;
                public FXField Color2B;
                public FXField Color2A;
                public int Unk10;
                public int Unk11;
                public int Unk12;
                public int Unk13;
                public float Unk14;
                public int AlphaMode;
                public float Saturation;
                public int Unk17;

                public DS1RExtraNodes DS1RData;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Duration = br.ReadSingle();
                    DurationMult = br.ReadSingle();
                    LifetimeVariance = br.ReadSingle();
                    AttachedBool = br.ReadInt32();
                    RenderDepth = br.ReadSingle();

                    if (br.VarintLong)
                        br.AssertInt32(0);

                    TextureID1 = br.ReadInt32();
                    TextureID2 = br.ReadInt32();
                    TextureID3 = br.ReadInt32();
                    OrientationMode = br.ReadInt32();
                    BlendMode = br.ReadInt32();
                    Unk8 = br.ReadInt32();
                    Scale1X = FXField.Read(br, env);
                    Scale1Y = FXField.Read(br, env);
                    Scale2X = FXField.Read(br, env);
                    Scale2Y = FXField.Read(br, env);
                    RotX = FXField.Read(br, env);
                    RotY = FXField.Read(br, env);
                    RotZ = FXField.Read(br, env);
                    RotSpeedX = FXField.Read(br, env);
                    RotSpeedY = FXField.Read(br, env);
                    RotSpeedZ = FXField.Read(br, env);
                    Tex1UVOffsetX = FXField.Read(br, env);
                    Tex1UVOffsetY = FXField.Read(br, env);
                    Tex1UVScrollX = FXField.Read(br, env);
                    Tex1UVScrollY = FXField.Read(br, env);
                    Tex2UVOffsetX = FXField.Read(br, env);
                    Tex2UVOffsetY = FXField.Read(br, env);
                    Tex2UVScrollX = FXField.Read(br, env);
                    Tex2UVScrollY = FXField.Read(br, env);
                    Tex3UVOffsetX = FXField.Read(br, env);
                    Tex3UVOffsetY = FXField.Read(br, env);
                    Tex3UVScrollX = FXField.Read(br, env);
                    Tex3UVScrollY = FXField.Read(br, env);
                    Color1R = FXField.Read(br, env);
                    Color1G = FXField.Read(br, env);
                    Color1B = FXField.Read(br, env);
                    Color1A = FXField.Read(br, env);
                    Color2R = FXField.Read(br, env);
                    Color2G = FXField.Read(br, env);
                    Color2B = FXField.Read(br, env);
                    Color2A = FXField.Read(br, env);
                    Unk10 = br.ReadInt32();

                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    if (br.VarintLong)
                    {
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                        br.AssertInt32(0);
                    }

                    Unk11 = br.ReadInt32();
                    Unk12 = br.ReadInt32();
                    Unk13 = br.ReadInt32();
                    Unk14 = br.ReadSingle();
                    AlphaMode = br.ReadInt32();
                    Saturation = br.ReadSingle();
                    Unk17 = br.ReadInt32();

                    if (br.VarintLong)
                        DS1RData = DS1RExtraNodes.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Duration);
                    bw.WriteSingle(DurationMult);
                    bw.WriteSingle(LifetimeVariance);
                    bw.WriteInt32(AttachedBool);
                    bw.WriteSingle(RenderDepth);

                    if (bw.VarintLong)
                        bw.WriteInt32(0);

                    bw.WriteInt32(TextureID1);
                    bw.WriteInt32(TextureID2);
                    bw.WriteInt32(TextureID3);
                    bw.WriteInt32(OrientationMode);
                    bw.WriteInt32(BlendMode);
                    bw.WriteInt32(Unk8);
                    WriteField(Scale1X);
                    WriteField(Scale1Y);
                    WriteField(Scale2X);
                    WriteField(Scale2Y);
                    WriteField(RotX);
                    WriteField(RotY);
                    WriteField(RotZ);
                    WriteField(RotSpeedX);
                    WriteField(RotSpeedY);
                    WriteField(RotSpeedZ);
                    WriteField(Tex1UVOffsetX);
                    WriteField(Tex1UVOffsetY);
                    WriteField(Tex1UVScrollX);
                    WriteField(Tex1UVScrollY);
                    WriteField(Tex2UVOffsetX);
                    WriteField(Tex2UVOffsetY);
                    WriteField(Tex2UVScrollX);
                    WriteField(Tex2UVScrollY);
                    WriteField(Tex3UVOffsetX);
                    WriteField(Tex3UVOffsetY);
                    WriteField(Tex3UVScrollX);
                    WriteField(Tex3UVScrollY);
                    WriteField(Color1R);
                    WriteField(Color1G);
                    WriteField(Color1B);
                    WriteField(Color1A);
                    WriteField(Color2R);
                    WriteField(Color2G);
                    WriteField(Color2B);
                    WriteField(Color2A);
                    bw.WriteInt32(Unk10);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);

                    if (bw.VarintLong)
                    {
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                        bw.WriteInt32(0);
                    }

                    bw.WriteInt32(Unk11);
                    bw.WriteInt32(Unk12);
                    bw.WriteInt32(Unk13);
                    bw.WriteSingle(Unk14);
                    bw.WriteInt32(AlphaMode);
                    bw.WriteSingle(Saturation);
                    bw.WriteInt32(Unk17);

                    if (bw.VarintLong)
                        DS1RData.Write(bw, this);
                }
            }

            public class ClusterAppearanceBillboardActionData71 : FXActionData
            {
                public override int Type => 71;

                public float Duration;
                public float DurationMult;
                public float LifetimeVariance;
                public int AttachedBool;
                public float RenderDepth;
                public int TextureID;
                public int Unk6; //this might be a mistake
                public int Unk7;
                public int OrientationMode;
                public int BlendMode;
                public int Unk10;
                public FXField Scale1X;
                public FXField Scale1Y;
                public FXField Scale2X;
                public FXField Scale2Y;
                public FXField RotX;
                public FXField RotY;
                public FXField RotZ;
                public FXField RotSpeedX;
                public FXField RotSpeedY;
                public FXField RotSpeedZ;
                public int AnimFrameSliceCountPerRow;
                public int AnimFrameTotalCount;
                public FXField AnimFrameInitial;
                public FXField AnimFrameTimeline;
                public FXField Color1R;
                public FXField Color1G;
                public FXField Color1B;
                public FXField Color1A;
                public FXField Color2R;
                public FXField Color2G;
                public FXField Color2B;
                public FXField Color2A;

                public int DS1R_UnkA1;
                public int DS1R_UnkA2;
                public int DS1R_UnkA3;
                public int DS1R_UnkA4;

                public int Unk15;
                public int Unk16;
                public int Unk17;
                public int Unk18;
                public float Unk19;
                public int AlphaMode;
                public float Saturation;
                public int Unk22;

                public DS1RExtraNodes DS1RData;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Duration = br.ReadSingle();
                    DurationMult = br.ReadSingle();
                    LifetimeVariance = br.ReadSingle();
                    AttachedBool = br.ReadInt32();
                    RenderDepth = br.ReadSingle();

                    if (br.VarintLong)
                        br.AssertInt32(0);

                    TextureID = br.ReadInt32();
                    Unk7 = br.ReadInt32();
                    OrientationMode = br.ReadInt32();
                    BlendMode = br.ReadInt32();
                    Unk10 = br.ReadInt32();

                    if (br.VarintLong)
                        br.AssertInt32(0);

                    Scale1X = FXField.Read(br, env);
                    Scale1Y = FXField.Read(br, env);
                    Scale2X = FXField.Read(br, env);
                    Scale2Y = FXField.Read(br, env);
                    RotX = FXField.Read(br, env);
                    RotY = FXField.Read(br, env);
                    RotZ = FXField.Read(br, env);
                    RotSpeedX = FXField.Read(br, env);
                    RotSpeedY = FXField.Read(br, env);
                    RotSpeedZ = FXField.Read(br, env);

                    AnimFrameSliceCountPerRow = br.ReadInt32();
                    AnimFrameTotalCount = br.ReadInt32();

                    AnimFrameInitial = FXField.Read(br, env);
                    AnimFrameTimeline = FXField.Read(br, env);
                    Color1R = FXField.Read(br, env);
                    Color1G = FXField.Read(br, env);
                    Color1B = FXField.Read(br, env);
                    Color1A = FXField.Read(br, env);
                    Color2R = FXField.Read(br, env);
                    Color2G = FXField.Read(br, env);
                    Color2B = FXField.Read(br, env);
                    Color2A = FXField.Read(br, env);

                    if (br.VarintLong)
                    {
                        DS1R_UnkA1 = br.ReadInt32();
                        DS1R_UnkA2 = br.ReadInt32();
                        DS1R_UnkA3 = br.ReadInt32();
                        DS1R_UnkA4 = br.ReadInt32();
                    }

                    Unk15 = br.ReadInt32();

                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);

                    Unk16 = br.ReadInt32();
                    Unk17 = br.ReadInt32();
                    Unk18 = br.ReadInt32();
                    Unk19 = br.ReadSingle();
                    AlphaMode = br.ReadInt32();
                    Saturation = br.ReadSingle();
                    Unk22 = br.ReadInt32();

                    if (br.VarintLong)
                        DS1RData = DS1RExtraNodes.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Duration);
                    bw.WriteSingle(DurationMult);
                    bw.WriteSingle(LifetimeVariance);
                    bw.WriteInt32(AttachedBool);
                    bw.WriteSingle(RenderDepth);

                    if (bw.VarintLong)
                        bw.WriteInt32(0);

                    bw.WriteInt32(TextureID);
                    bw.WriteInt32(Unk7);
                    bw.WriteInt32(OrientationMode);
                    bw.WriteInt32(BlendMode);
                    bw.WriteInt32(Unk10);

                    if (bw.VarintLong)
                        bw.WriteInt32(0);

                    WriteField(Scale1X);
                    WriteField(Scale1Y);
                    WriteField(Scale2X);
                    WriteField(Scale2Y);
                    WriteField(RotX);
                    WriteField(RotY);
                    WriteField(RotZ);
                    WriteField(RotSpeedX);
                    WriteField(RotSpeedY);
                    WriteField(RotSpeedZ);

                    bw.WriteInt32(AnimFrameSliceCountPerRow);
                    bw.WriteInt32(AnimFrameTotalCount);

                    WriteField(AnimFrameInitial);
                    WriteField(AnimFrameTimeline);
                    WriteField(Color1R);
                    WriteField(Color1G);
                    WriteField(Color1B);
                    WriteField(Color1A);
                    WriteField(Color2R);
                    WriteField(Color2G);
                    WriteField(Color2B);
                    WriteField(Color2A);

                    if (bw.VarintLong)
                    {
                        bw.WriteInt32(DS1R_UnkA1);
                        bw.WriteInt32(DS1R_UnkA2);
                        bw.WriteInt32(DS1R_UnkA3);
                        bw.WriteInt32(DS1R_UnkA4);
                    }

                    bw.WriteInt32(Unk15);

                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);

                    bw.WriteInt32(Unk16);
                    bw.WriteInt32(Unk17);
                    bw.WriteInt32(Unk18);
                    bw.WriteSingle(Unk19);
                    bw.WriteInt32(AlphaMode);
                    bw.WriteSingle(Saturation);
                    bw.WriteInt32(Unk22);

                    if (bw.VarintLong)
                        DS1RData.Write(bw, this);
                }
            }

            public class ClusterMovementWobblingActionData84 : FXActionData
            {
                public override int Type => 84;

                public FXField Gravity;
                public FXField MovementFalloff;
                public FXField MovementFalloffMult;
                public float PhaseShift;
                public FXField TurnAngle;
                public int TurnInterval;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Gravity = FXField.Read(br, env);
                    MovementFalloff = FXField.Read(br, env);
                    MovementFalloffMult = FXField.Read(br, env);
                    br.AssertInt32(0);
                    PhaseShift = br.ReadSingle();
                    TurnAngle = FXField.Read(br, env);
                    TurnInterval = ReadFXR1Varint(br);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(Gravity);
                    WriteField(MovementFalloff);
                    WriteField(MovementFalloffMult);
                    bw.WriteInt32(0);
                    bw.WriteSingle(PhaseShift);
                    WriteField(TurnAngle);
                    WriteFXR1Varint(bw, TurnInterval);
                }
            }

            public class ClusterMovementPartialFollowActionData105 : FXActionData
            {
                public override int Type => 105;

                public FXField Gravity;
                public FXField MovementFalloff;
                public FXField MovementFalloffMult;
                public float PhaseShift;
                public FXField TurnAngle;
                public int TurnInterval;
                public FXField FollowRate;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Gravity = FXField.Read(br, env);
                    MovementFalloff = FXField.Read(br, env);
                    MovementFalloffMult = FXField.Read(br, env);
                    br.AssertInt32(0);
                    PhaseShift = br.ReadSingle();
                    TurnAngle = FXField.Read(br, env);
                    TurnInterval = ReadFXR1Varint(br);
                    FollowRate = FXField.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(Gravity);
                    WriteField(MovementFalloff);
                    WriteField(MovementFalloffMult);
                    bw.WriteInt32(0);
                    bw.WriteSingle(PhaseShift);
                    WriteField(TurnAngle);
                    WriteFXR1Varint(bw, TurnInterval);
                    WriteField(FollowRate);
                }
            }

            public class ParticleAppearanceRadialBlurActionData107 : FXActionData
            {
                public override int Type => 107;

                public float RenderDepth;
                public int TextureID;
                public int DistortionMode;
                public FXField Scale1X;
                public FXField Scale1Y;
                public FXField Scale1Z;
                public FXField ColorR;
                public FXField ColorG;
                public FXField ColorB;
                public FXField ColorA;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    RenderDepth = br.ReadSingle();
                    br.AssertInt32(0);
                    TextureID = br.ReadInt32();
                    DistortionMode = br.ReadInt32();
                    Scale1X = FXField.Read(br, env);
                    Scale1Y = FXField.Read(br, env);
                    Scale1Z = FXField.Read(br, env);
                    ColorR = FXField.Read(br, env);
                    ColorG = FXField.Read(br, env);
                    ColorB = FXField.Read(br, env);
                    ColorA = FXField.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(RenderDepth);
                    bw.WriteInt32(0);
                    bw.WriteInt32(TextureID);
                    bw.WriteInt32(DistortionMode);
                    WriteField(Scale1X);
                    WriteField(Scale1Y);
                    WriteField(Scale1Z);
                    WriteField(ColorR);
                    WriteField(ColorG);
                    WriteField(ColorB);
                    WriteField(ColorA);
                }
            }

            public class Particle3DActionData108 : FXActionData
            {
                public override int Type => 108;

                public float Duration;
                public float DurationMult;
                public float LifetimeVariance;
                public int AttachedBool;
                public float Unk5;
                public int ModelID;
                public int OrientationMode;
                public int BlendMode;
                public int Unk8;
                public int DS1R_Unk8B;
                public FXField Scale1X;
                public FXField Scale1Y;
                public FXField Scale1Z;
                public FXField Scale2X;
                public FXField Scale2Y;
                public FXField Scale2Z;
                public FXField RotX;
                public FXField RotY;
                public FXField RotZ;
                public FXField RotSpeedX;
                public FXField RotSpeedY;
                public FXField RotSpeedZ;
                public int AnimFrameSliceCountPerRow;
                public int AnimFrameTotalCount;
                public FXField AnimFrameInitial;
                public FXField AnimFrameTimeline;
                public FXField UVOffsetX;
                public FXField UVOffsetY;
                public FXField UVScrollX;
                public FXField UVScrollY;
                public FXField Color1R;
                public FXField Color1G;
                public FXField Color1B;
                public FXField Color1A;
                public FXField Color2R;
                public FXField Color2G;
                public FXField Color2B;
                public FXField Color2A;
                public int Unk12;
                public int Unk13;
                public int Unk14;
                public float Unk15;
                public int Unk16;

                public DS1RExtraNodes DS1RData;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Duration = br.ReadSingle();
                    DurationMult = br.ReadSingle();
                    LifetimeVariance = br.ReadSingle();
                    AttachedBool = br.ReadInt32();
                    Unk5 = br.ReadSingle();

                    if (br.VarintLong)
                        br.AssertInt32(0);

                    ModelID = br.ReadInt32();
                    OrientationMode = br.ReadInt32();
                    BlendMode = br.ReadInt32();
                    Unk8 = br.ReadInt32();
                    DS1R_Unk8B = ReadFXR1Varint(br);
                    Scale1X = FXField.Read(br, env);
                    Scale1Y = FXField.Read(br, env);
                    Scale1Z = FXField.Read(br, env);
                    Scale2X = FXField.Read(br, env);
                    Scale2Y = FXField.Read(br, env);
                    Scale2Z = FXField.Read(br, env);
                    RotX = FXField.Read(br, env);
                    RotY = FXField.Read(br, env);
                    RotZ = FXField.Read(br, env);
                    RotSpeedX = FXField.Read(br, env);
                    RotSpeedY = FXField.Read(br, env);
                    RotSpeedZ = FXField.Read(br, env);
                    AnimFrameSliceCountPerRow = br.ReadInt32();
                    AnimFrameTotalCount = br.ReadInt32();
                    AnimFrameInitial = FXField.Read(br, env);
                    AnimFrameTimeline = FXField.Read(br, env);
                    UVOffsetX = FXField.Read(br, env);
                    UVOffsetY = FXField.Read(br, env);
                    UVScrollX = FXField.Read(br, env);
                    UVScrollY = FXField.Read(br, env);
                    Color1R = FXField.Read(br, env);
                    Color1G = FXField.Read(br, env);
                    Color1B = FXField.Read(br, env);
                    Color1A = FXField.Read(br, env);
                    Color2R = FXField.Read(br, env);
                    Color2G = FXField.Read(br, env);
                    Color2B = FXField.Read(br, env);
                    Color2A = FXField.Read(br, env);
                    Unk12 = br.ReadInt32();
                    Unk13 = br.ReadInt32();
                    Unk14 = br.ReadInt32();
                    Unk15 = br.ReadSingle();
                    Unk16 = ReadFXR1Varint(br);

                    if (br.VarintLong)
                        DS1RData = DS1RExtraNodes.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    bw.WriteSingle(Duration);
                    bw.WriteSingle(DurationMult);
                    bw.WriteSingle(LifetimeVariance);
                    bw.WriteInt32(AttachedBool);
                    bw.WriteSingle(Unk5);

                    if (bw.VarintLong)
                        bw.WriteInt32(0);

                    bw.WriteInt32(ModelID);
                    bw.WriteInt32(OrientationMode);
                    bw.WriteInt32(BlendMode);
                    bw.WriteInt32(Unk8);
                    WriteFXR1Varint(bw, DS1R_Unk8B);
                    WriteField(Scale1X);
                    WriteField(Scale1Y);
                    WriteField(Scale1Z);
                    WriteField(Scale2X);
                    WriteField(Scale2Y);
                    WriteField(Scale2Z);
                    WriteField(RotX);
                    WriteField(RotY);
                    WriteField(RotZ);
                    WriteField(RotSpeedX);
                    WriteField(RotSpeedY);
                    WriteField(RotSpeedZ);
                    bw.WriteInt32(AnimFrameSliceCountPerRow);
                    bw.WriteInt32(AnimFrameTotalCount);
                    WriteField(AnimFrameInitial);
                    WriteField(AnimFrameTimeline);
                    WriteField(UVOffsetX);
                    WriteField(UVOffsetY);
                    WriteField(UVScrollX);
                    WriteField(UVScrollY);
                    WriteField(Color1R);
                    WriteField(Color1G);
                    WriteField(Color1B);
                    WriteField(Color1A);
                    WriteField(Color2R);
                    WriteField(Color2G);
                    WriteField(Color2B);
                    WriteField(Color2A);
                    bw.WriteInt32(Unk12);
                    bw.WriteInt32(Unk13);
                    bw.WriteInt32(Unk14);
                    bw.WriteSingle(Unk15);
                    WriteFXR1Varint(bw, Unk16);

                    if (bw.VarintLong)
                        DS1RData.Write(bw, this);
                }
            }

            public class FXActionData117 : FXActionData
            {
                public override int Type => 117;

                public FXField Unk1_1;
                public FXField Unk1_2;
                public FXField Unk1_3;
                public FXField Unk1_4;
                public FXField Unk1_5;
                public FXField Unk1_6;
                public int Unk2;
                public int Unk3;
                public FXField Unk4;

                internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
                {
                    Unk1_1 = FXField.Read(br, env);
                    Unk1_2 = FXField.Read(br, env);
                    Unk1_3 = FXField.Read(br, env);
                    Unk1_4 = FXField.Read(br, env);
                    Unk1_5 = FXField.Read(br, env);
                    Unk1_6 = FXField.Read(br, env);
                    Unk2 = br.ReadInt32();
                    Unk3 = br.ReadInt32();
                    Unk4 = FXField.Read(br, env);
                }

                internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
                {
                    WriteField(Unk1_1);
                    WriteField(Unk1_2);
                    WriteField(Unk1_3);
                    WriteField(Unk1_4);
                    WriteField(Unk1_5);
                    WriteField(Unk1_6);
                    bw.WriteInt32(Unk2);
                    bw.WriteInt32(Unk3);
                    WriteField(Unk4);
                }
            }
        }


        public class ActionDataRef : FXActionData
        {
            public override int Type => -1;

            [XmlAttribute]
            public string ReferenceXID;

            public override bool ShouldSerializeResources() => false;

            public ActionDataRef(FXActionData refVal)
            {
                ReferenceXID = refVal?.XID;
            }

            public ActionDataRef()
            {

            }

            internal override void InnerRead(BinaryReaderEx br, FxrEnvironment env)
            {
                throw new InvalidOperationException("Cannot actually serialize a reference class.");
            }

            internal override void InnerWrite(BinaryWriterEx bw, FxrEnvironment env)
            {
                throw new InvalidOperationException("Cannot actually deserialize a reference class.");
            }
        }

    }
}
