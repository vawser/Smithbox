using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats.Formats.Morpheme.NSA
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class StaticSegment
    {
        public int translationBoneCount;
        public int rotationBoneCount;
        public DequantizationFactor translationBoneDequantizationFactor;
        public DequantizationFactor rotationBoneDequantizationFactor;
        public List<TranslationData> translationSamples = new List<TranslationData>();
        public List<RotationData> rotationSamples = new List<RotationData>();

        //Dequantized Lists
        public List<Vector3> translationFrames = new List<Vector3>();
        public List<Quaternion> rotationFrames = new List<Quaternion>();

        /// <summary>
        /// Default constructor
        /// </summary>
        public StaticSegment() { }

        /// <summary>
        /// Constructor for reading a staticSegment with a BinaryReaderEx
        /// </summary>
        /// <param name="br"></param>
        public StaticSegment(BinaryReaderEx br)
        {
            var dataStart = br.Position;
            translationBoneCount = br.ReadInt32();
            rotationBoneCount = br.ReadInt32();
            translationBoneDequantizationFactor = new DequantizationFactor(br);
            rotationBoneDequantizationFactor = new DequantizationFactor(br);

            var compressedTranslationOffset = br.ReadVarint();
            var compressedRotationOffset = br.ReadVarint();

            br.StepIn(dataStart + compressedTranslationOffset);
            for (int i = 0; i < translationBoneCount; i++)
            {
                translationSamples.Add(new TranslationData(br));
            }
            br.StepOut();
            br.StepIn(dataStart + compressedRotationOffset);
            for (int i = 0; i < rotationBoneCount; i++)
            {
                rotationSamples.Add(new RotationData(br));
            }
            br.StepOut();
        }

        /// <summary>
        /// Method for dequantizing this staticSegment
        /// </summary>
        public void Dequantize()
        {
            for (int i = 0; i < translationBoneCount; i++)
            {
                if (i < translationSamples.Count)
                {
                    translationFrames.Add(translationSamples[i].DequantizeTranslation(translationBoneDequantizationFactor));
                }
            }

            for (int i = 0; i < rotationBoneCount; i++)
            {

                if (i < rotationSamples.Count)
                {
                    rotationFrames.Add(rotationSamples[i].DequantizeRotation(rotationBoneDequantizationFactor));
                }
            }
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
