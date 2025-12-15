using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats.Formats.Morpheme.NSA
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class RootMotionSegment
    {
        public float fps;
        public long sampleCount;
        public DequantizationFactor dequantizationFactor;
        public Quaternion rotation;
        public List<TranslationData> translationSamples = new List<TranslationData>();
        public List<RotationData> rotationSamples = new List<RotationData>();

        //Dequantized Lists
        public List<Vector3> translationFrames = new List<Vector3>();
        public List<Quaternion> rotationFrames = new List<Quaternion>();

        public RootMotionSegment() { }

        public RootMotionSegment(BinaryReaderEx br)
        {
            var dataStart = br.Position;
            br.Position += 0x20;
            fps = br.ReadSingle();
            sampleCount = br.ReadInt32();
            dequantizationFactor = new DequantizationFactor(br);
            //var dequantizationFactor2 = new DequantizationFactor(br);
            rotation = new Quaternion(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

            br.Position += 0x8;

            var pTranslationSample = br.ReadVarint();
            var pRotationSample = br.ReadVarint();

            if (pTranslationSample != 0)
            {
                br.Position = dataStart + pTranslationSample;

                for (int i = 0; i < sampleCount; i++)
                {
                    translationSamples.Add(new TranslationData(br));
                }
            }

            if (pRotationSample != 0)
            {
                br.Position = dataStart + pRotationSample;

                for (int i = 0; i < sampleCount; i++)
                {
                    rotationSamples.Add(new RotationData(br));
                }
            }
        }

        public void Dequantize()
        {
            for (int i = 0; i < sampleCount; i++)
            {
                if (i < translationSamples.Count)
                {
                    translationFrames.Add(translationSamples[i].DequantizeTranslation(dequantizationFactor));
                }

                if (i < rotationSamples.Count)
                {
                    rotationFrames.Add(rotationSamples[i].DequantizeRotation(dequantizationFactor));
                }
            }
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
