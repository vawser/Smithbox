using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats.Formats.Morpheme.NSA
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class DynamicSegment
    {
        public int sampleCount;
        public int translationBoneCount;
        public int rotationBoneCount;
        public List<List<TranslationData>> translationRawFrameLists = new List<List<TranslationData>>();
        public List<DequantizationInfo> translationDequantizationInfo = new List<DequantizationInfo>();
        public List<List<RotationData>> rotationRawFrameLists = new List<List<RotationData>>();
        public List<DequantizationInfo> rotationDequantizationInfo = new List<DequantizationInfo>();

        //Dequantized Lists
        public List<List<Vector3>> translationFrameLists = new List<List<Vector3>>();
        public List<List<Quaternion>> rotationFrameLists = new List<List<Quaternion>>();

        public DynamicSegment() { }

        public DynamicSegment(BinaryReaderEx br)
        {
            var dataStart = br.Position;
            sampleCount = br.ReadInt32();
            translationBoneCount = br.ReadInt32();
            rotationBoneCount = br.ReadInt32();

            var dataCurrent = br.Position;
            br.Position = dataCurrent + 4;

            var pTranslationFrames = br.ReadVarint();
            var pTranslationDeqInfo = br.ReadVarint();
            var pRotationFrames = br.ReadVarint();
            var pRotationDeqInfo = br.ReadVarint();

            var translDeqCount = GetDeqInfoCount(translationBoneCount);
            var rotDeqCount = GetDeqInfoCount(rotationBoneCount);

            br.Position = dataStart + pTranslationFrames;
            for (int i = 0; i < sampleCount; i++)
            {
                List<TranslationData> frames = new List<TranslationData>();
                for (int j = 0; j < translationBoneCount; j++)
                {
                    frames.Add(new TranslationData(br));
                }
                translationRawFrameLists.Add(frames);
            }

            br.Position = dataStart + pTranslationDeqInfo;
            for (int i = 0; i < translDeqCount; i++)
            {
                translationDequantizationInfo.Add(new DequantizationInfo(br));
            }

            br.Position = dataStart + pRotationFrames;
            for (int i = 0; i < sampleCount; i++)
            {
                List<RotationData> frames = new List<RotationData>();
                for (int j = 0; j < rotationBoneCount; j++)
                {
                    frames.Add(new RotationData(br));
                }
                rotationRawFrameLists.Add(frames);
            }

            br.Position = dataStart + pRotationDeqInfo;
            for (int i = 0; i < rotDeqCount; i++)
            {
                rotationDequantizationInfo.Add(new DequantizationInfo(br));
            }
        }

        public int GetDeqInfoCount(int bone_count)
        {
            int multiple = bone_count / 4;

            if (bone_count % 4 != 0)
                multiple = multiple + 1;

            return multiple * 4;
        }

        public void Dequantize(List<DequantizationFactor> translationAnimDequantizationFactors, List<DequantizationFactor> rotationAnimDequantizationFactors)
        {
            if (translationBoneCount > 0)
            {
                //Loop through sample sets
                //Sample sets are arrays of data for a particular frame for the count of bones defined.
                for (int frameId = 0; frameId < sampleCount; frameId++)
                {
                    List<Vector3> frameList = new List<Vector3>();
                    for (int bone = 0; bone < translationBoneCount; bone++)
                    {
                        var factorMin = new Vector3(translationAnimDequantizationFactors[translationDequantizationInfo[bone].factorIdx[0]].min.X,
                                                    translationAnimDequantizationFactors[translationDequantizationInfo[bone].factorIdx[1]].min.Y,
                                                    translationAnimDequantizationFactors[translationDequantizationInfo[bone].factorIdx[2]].min.Z);
                        var factorScaledExtent = new Vector3(translationAnimDequantizationFactors[translationDequantizationInfo[bone].factorIdx[0]].scaledExtent.X,
                                                             translationAnimDequantizationFactors[translationDequantizationInfo[bone].factorIdx[1]].scaledExtent.Y,
                                                             translationAnimDequantizationFactors[translationDequantizationInfo[bone].factorIdx[2]].scaledExtent.Z);

                        var dequantizedTranslation = translationRawFrameLists[frameId][bone].DequantizeTranslation(new DequantizationFactor(factorMin, factorScaledExtent));


                        frameList.Add(dequantizedTranslation);
                    }

                    translationFrameLists.Add(frameList);
                }
            }

            if (rotationBoneCount > 0)
            {
                for (int frameId = 0; frameId < sampleCount; frameId++)
                {
                    List<Quaternion> frameList = new List<Quaternion>();
                    for (int bone = 0; bone < rotationBoneCount; bone++)
                    {
                        var factorMin = new Vector3(rotationAnimDequantizationFactors[rotationDequantizationInfo[bone].factorIdx[0]].min.X,
                                                    rotationAnimDequantizationFactors[rotationDequantizationInfo[bone].factorIdx[1]].min.Y,
                                                    rotationAnimDequantizationFactors[rotationDequantizationInfo[bone].factorIdx[2]].min.Z);
                        var factorScaledExtent = new Vector3(rotationAnimDequantizationFactors[rotationDequantizationInfo[bone].factorIdx[0]].scaledExtent.X,
                                                             rotationAnimDequantizationFactors[rotationDequantizationInfo[bone].factorIdx[1]].scaledExtent.Y,
                                                             rotationAnimDequantizationFactors[rotationDequantizationInfo[bone].factorIdx[2]].scaledExtent.Z);

                        var dequantizedRotation = rotationRawFrameLists[frameId][bone].DequantizeRotation(new DequantizationFactor(factorMin, factorScaledExtent));


                        frameList.Add(dequantizedRotation);
                    }

                    rotationFrameLists.Add(frameList);
                }
            }
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
