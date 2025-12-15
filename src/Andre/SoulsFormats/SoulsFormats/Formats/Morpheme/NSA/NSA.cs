using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.NSA
{
    /// <summary>
    /// Morpheme animation file
    /// </summary>
    public class NSA
    {
        /// <summary>
        /// NSA file header
        /// </summary>
        public NSAHeader header;
        /// <summary>
        /// Bone indices used for translation in the staticSegment
        /// </summary>
        public List<ushort> staticTranslationIndices;
        /// <summary>
        /// Bone indices used for rotation in the staticSegment
        /// </summary>
        public List<ushort> staticRotationIndices;
        /// <summary>
        /// Bone indices used for translation in the dynamicSegment
        /// </summary>
        public List<ushort> dynamicTranslationIndices;
        /// <summary>
        /// Bone indices used for rotation in the dynamicSegment
        /// </summary>
        public List<ushort> dynamicRotationIndicies;
        /// <summary>
        /// DequantizationFactors for translation in the dynamicSegment
        /// </summary>
        public List<DequantizationFactor> translationAnimDequantizationFactors = new List<DequantizationFactor>();
        /// <summary>
        /// DequantizationFactors for rotation in the dynamicSegment
        /// </summary>
        public List<DequantizationFactor> rotationAnimDequantizationFactors = new List<DequantizationFactor>();

        //The segments contain the actual animation data, which we process into something usable with the dequantization functions.
        /// <summary>
        /// An object containing root bone animation data. The root bone will never be included in the dynamic or static animation segments.
        /// </summary>
        public RootMotionSegment rootMotionSegment;
        /// <summary>
        /// An object containing single frame animation data for bones. Bones included here should not be in the dynamicSegment nor the rootMotionSegment.
        /// </summary>
        public StaticSegment staticSegment;
        /// <summary>
        /// An object contianing multi frame animation data for bones. Bones included here should not be in the staticSegment nor the RootMotionSegment.
        /// </summary>
        public DynamicSegment dynamicSegment;

        /// <summary>
        /// Default constructor
        /// </summary>
        public NSA() { }

        /// <summary>
        /// Constructor for reading an NSA file with BinaryReaderEx
        /// </summary>
        /// <param name="br"></param>
        public NSA(BinaryReaderEx br)
        {
            header = new NSAHeader(br);

            br.Position = header.pStaticTranslationBoneIndices;
            staticTranslationIndices = ReadBoneIndices(br);

            br.Position = header.pStaticRotationBoneIndices;
            staticRotationIndices = ReadBoneIndices(br);

            br.Position = header.ppDynamicTranslationBoneIndices;
            br.Position = br.ReadVarint();
            dynamicTranslationIndices = ReadBoneIndices(br);

            br.Position = header.ppDynamicRotationBoneIndices;
            br.Position = br.ReadVarint();
            dynamicRotationIndicies = ReadBoneIndices(br);

            br.Position = header.pTranslationDequantizationFactors;
            for (int i = 0; i < header.translationDequantizationCount; i++)
            {
                translationAnimDequantizationFactors.Add(new DequantizationFactor(br));
            }

            br.Position = header.pRotationDequantizationFactors;
            for (int i = 0; i < header.rotationDequantizationCount; i++)
            {
                rotationAnimDequantizationFactors.Add(new DequantizationFactor(br));
            }

            br.Position = header.pStaticSegment;
            staticSegment = header.pStaticSegment > 0 ? new StaticSegment(br) : new StaticSegment();

            br.Position = header.pDynamicSegment;
            dynamicSegment = header.pDynamicSegment > 0 ? new DynamicSegment(br) : new DynamicSegment();

            br.Position = header.pRootMotionSegment;
            rootMotionSegment = header.pRootMotionSegment > 0 ? new RootMotionSegment(br) : new RootMotionSegment();

            DequantizeAnimation();
        }

        /// <summary>
        /// Method for reading the bone indices for a particular segment's transform
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        public List<ushort> ReadBoneIndices(BinaryReaderEx br)
        {
            var count = br.ReadInt16();
            List<ushort> indices = new List<ushort>();
            for (int i = 0; i < count; i++)
            {
                indices.Add(br.ReadUInt16());
            }

            return indices;
        }

        /// <summary>
        /// Decompresses the animation data back to something we can use. Basic concept can be found here: https://github.com/ryanjsims/warpgate/blob/75c60875cd61aa275672b741e9ef472c4bb5b309/doc/formats/README.md?plain=1#L24
        /// Implementation is based on other code in the same project.
        /// </summary>
        public void DequantizeAnimation()
        {
            rootMotionSegment.Dequantize();                                                                         //Anim data for the root bone.
            staticSegment.Dequantize();                                                                             //Anim data for transforms that don't change during the animation. Does not include root bone.
            dynamicSegment.Dequantize(translationAnimDequantizationFactors, rotationAnimDequantizationFactors);     //Anim data for transforms that change during the animation. Does not include root bone.
        }

        /// <summary>
        /// A check for 64 bit values and the file's endianness. BinaryReader is adjusted as needed for this.
        /// </summary>
        public static void Set64BitAndEndianness(BinaryReaderEx br)
        {
            br.VarintLong = true;
            br.StepIn(0x8);

            //This should be reliable since there's no Big Endian platform that's 64 bit which DS2 is on.
            var endTest = br.ReadUInt32();
            br.BigEndian = true;
            br.Position -= 4;
            var endTest2 = br.ReadUInt32();
            if (endTest < endTest2)
            {
                br.BigEndian = false;
            }

            var test = br.ReadUInt32();
            if (test > 0)
            {
                br.VarintLong = false;
            }
            br.StepOut();
        }
    }
}
