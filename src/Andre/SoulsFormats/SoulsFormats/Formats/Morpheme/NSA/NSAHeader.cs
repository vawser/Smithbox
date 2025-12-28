namespace SoulsFormats.Formats.Morpheme.NSA
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class NSAHeader
    {
        public long pDynamicSegment;
        public int alignment;
        public int size;
        public float duration;
        public float fps;
        public int boneCount;
        public int animBoneCount;
        public long pStaticTranslationBoneIndices;
        public long pStaticRotationBoneIndices;
        public long ppDynamicTranslationBoneIndices;
        public long ppDynamicRotationBoneIndices;
        public DequantizationFactor translationStartPosFactors;
        public int translationDequantizationCount;
        public int rotationDequantizationCount;
        public long pTranslationDequantizationFactors;
        public long pRotationDequantizationFactors;
        public long pStaticSegment;
        public long dynamicSegmentSize;
        public long pVarA0;
        public long pDynamicSegmentSize;
        public long ppDynamicSegment;
        public long pRootMotionSegment;

        public NSAHeader() { }

        public NSAHeader(BinaryReaderEx br)
        {
            var dataStart = br.Position;
            br.Position += 8;
            pDynamicSegment = br.ReadVarint();
            alignment = br.ReadInt32();
            size = br.ReadInt32();

            br.Position = dataStart + 0x28;
            duration = br.ReadSingle();
            fps = br.ReadSingle();
            boneCount = br.ReadInt32();

            br.Position = dataStart + 0x3C;
            animBoneCount = br.ReadInt32();
            pStaticTranslationBoneIndices = br.ReadVarint();
            pStaticRotationBoneIndices = br.ReadVarint();
            ppDynamicTranslationBoneIndices = br.ReadVarint();
            ppDynamicRotationBoneIndices = br.ReadVarint();

            translationStartPosFactors = new DequantizationFactor(br);

            translationDequantizationCount = br.ReadInt32();
            rotationDequantizationCount = br.ReadInt32();

            pTranslationDequantizationFactors = br.ReadVarint();
            pRotationDequantizationFactors = br.ReadVarint();

            pStaticSegment = br.ReadVarint();
            dynamicSegmentSize = br.ReadVarint();
            pVarA0 = br.ReadVarint();
            pDynamicSegmentSize = br.ReadVarint();
            ppDynamicSegment = br.ReadVarint();
            pRootMotionSegment = br.ReadVarint();
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
