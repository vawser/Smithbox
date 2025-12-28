using System.Collections.Generic;
using System.Numerics;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// Skeleton Rig
    /// </summary>
    public class Rig : MorphemeBundle_Base
    {
        public Quaternion blendFrameOrientation;

        public Vector4 blendFrameTranslation;

        public int trajectoryBoneId;

        public int characterRootBoneId;

        /// <summary>
        /// List of bone parent ids
        /// </summary>
        public List<int> parentIdList;

        /// <summary>
        /// List of bone ids
        /// </summary>
        public List<int> idList;

        /// <summary>
        /// List of offsets to bone names
        /// </summary>
        public List<int> nameOffsetsList;

        /// <summary>
        /// List of bone names
        /// </summary>
        public List<string> boneNames;

        public MorphemeSizeAlignFormatting transformFormat;

        public List<ElementDescriptor> elementDescriptors;

        public int flagBitsUsed;
        public List<uint> flagBitUintSet;

        public List<Vector3> boneTranslations;
        public List<Quaternion> boneRotations;

        public Rig() { }

        public Rig(BinaryReaderEx br)
        {
            Read(br);
        }

        public void Read(BinaryReaderEx br)
        {
            base.Read(br);
            var start = br.Position;
            blendFrameOrientation = br.ReadQuaternion();
            blendFrameTranslation = br.ReadVector4();
            var hierarchyPtr = br.ReadVarint();
            trajectoryBoneId = br.ReadInt32();
            characterRootBoneId = br.ReadInt32();
            var boneIdNamesTablePtr = br.ReadVarint();
            var bindPosePtr = br.ReadVarint();

            //Read parents
            br.Position = start + hierarchyPtr;
            var hierarchyStart = br.Position;
            int boneCount = br.ReadInt32();
            br.Pad(8);
            var parentArrayPtr = br.ReadVarint();
            br.Position = hierarchyStart + parentArrayPtr;
            parentIdList = new List<int>();
            for (int i = 0; i < boneCount; i++)
            {
                parentIdList.Add(br.ReadInt32());
            }


            //Read bone names
            br.Position = start + boneIdNamesTablePtr;
            var boneNamesStart = br.Position;

            int numEntries = br.ReadInt32();
            int dataLength = br.ReadInt32();

            var IdsPtr = br.ReadVarint();
            var offsetsPtr = br.ReadVarint();
            var stringsPtr = br.ReadVarint();

            //Read Ids
            br.Position = boneNamesStart + IdsPtr;
            idList = new List<int>();
            for (int i = 0; i < numEntries; i++)
            {
                idList.Add(br.ReadInt32());
            }

            //Read bone name offsets
            br.Position = boneNamesStart + offsetsPtr;
            nameOffsetsList = new List<int>();
            for (int i = 0; i < numEntries; i++)
            {
                nameOffsetsList.Add(br.ReadInt32());
            }

            //Read bone names
            var boneNamesNamesStart = br.Position = boneNamesStart + stringsPtr;
            boneNames = new List<string>();
            foreach (var offset in nameOffsetsList)
            {
                br.Position = boneNamesNamesStart + offset;
                boneNames.Add(br.ReadASCII());
            }

            //Read bind pose data 
            //Honestly this whole section is its own structure, but we don't use much of it in Dark Souls 2.
            var bindPoseStart = br.Position = start + bindPosePtr;
            br.Position += isX64 ? 8 : 4; //Skip padding
            var unkSht0 = br.ReadInt16();
            var attributeType = br.ReadEnum16<AttribType>(); //Should always be 0xD, transform buffer
            if (isX64)
            {
                br.Position += 4;
            }
            var transformBufferPtr = br.ReadVarint();
            br.Position = bindPoseStart + transformBufferPtr;
            br.Pad(0x10);

            var transformDataStart = br.Position;
            transformFormat = new MorphemeSizeAlignFormatting()
            {
                dataSize = br.ReadVarint(),
                dataAlignment = br.ReadInt32(),
                unkValue = isX64 ? br.ReadInt32() : 0
            };
            var transformSeCount = br.ReadInt32();
            var unkBt0 = br.ReadByte();
            //Padding
            br.ReadByte(); br.ReadByte(); br.ReadByte();
            var elementCount = br.ReadInt32();
            if (isX64)
            {
                br.Position += 4;
            }
            var elementDescriptorsPtr = br.ReadVarint();
            var dataPointersPtr = br.ReadVarint();
            var usedFlagsPtr = br.ReadVarint();

            br.Position = transformDataStart + elementDescriptorsPtr;
            elementDescriptors = new List<ElementDescriptor>();
            for (int i = 0; i < elementCount; i++)
            {
                elementDescriptors.Add(new ElementDescriptor()
                {
                    elementType = br.ReadInt32(),
                    elementSize = br.ReadInt32(),
                    elementAlignment = br.ReadInt32(),
                });
            }

            //Read Transforms
            br.Position = transformDataStart + dataPointersPtr;
            List<long> elementPointers = new List<long>();
            for (int i = 0; i < elementCount; i++)
            {
                elementPointers.Add(br.ReadVarint());
            }

            boneTranslations = new List<Vector3>();
            boneRotations = new List<Quaternion>();
            for (int i = 0; i < elementCount; i++)
            {
                br.Position = transformDataStart + elementPointers[i];
                for (int j = 0; j < transformSeCount; j++)
                {
                    switch (elementDescriptors[i].elementType)
                    {
                        case 0x2:
                            boneTranslations.Add(br.ReadVector3());
                            break;
                        case 0x6:
                            boneRotations.Add(br.ReadQuaternion());
                            break;
                        default:
                            throw new System.NotImplementedException();
                    }
                }
            }

            //Read UsedFlags
            br.Position = transformDataStart + usedFlagsPtr;
            flagBitsUsed = br.ReadInt32();
            var uintsUsed = br.ReadInt32();
            flagBitUintSet = new List<uint>();
            for (int i = 0; i < uintsUsed; i++)
            {
                flagBitUintSet.Add(br.ReadUInt32());
            }

            //Skip to end in case of oddities
            br.Position = start + format.dataSize;
        }

        public override long CalculateBundleSize()
        {
            throw new System.NotImplementedException();
        }

        public struct ElementDescriptor
        {
            public int elementType;
            public int elementSize;
            public int elementAlignment;
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
