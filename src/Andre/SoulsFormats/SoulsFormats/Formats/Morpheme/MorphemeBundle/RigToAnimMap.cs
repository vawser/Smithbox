using System;
using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// Mapping from rig to the animation rig
    /// </summary>
    public class RigToAnimMap : MorphemeBundle_Base
    {
        /// <summary>
        /// Mapping Type
        /// </summary>
        public RigToAnimMapType mappingType;

        /// <summary>
        /// Size and Alignment Formatting
        /// </summary>
        public MorphemeSizeAlignFormatting skelAnimMapFormat;
        
        /// <summary>
        /// Number of bits used between all bit uints 
        /// </summary>
        public int bitsUsed;

        /// <summary>
        /// BitArray data
        /// </summary>
        public List<uint> bitUintSet;

        /// <summary>
        /// Rig to Anim Mappings
        /// </summary>
        public RigToAnimEntryMap rigToAnimMap = null;

        /// <summary>
        /// Anim to Rig Mappings
        /// </summary>
        public AnimToRigTableMap animToRigMap = null;

        /// <summary>
        /// Morpheme Bundle Size Calculator
        /// </summary>
        public override long CalculateBundleSize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Basic RigToAnimMap Constructor
        /// </summary>
        public RigToAnimMap()
        {

        }

        /// <summary>
        ///  RigToAnimMap Read Constructor
        /// </summary>
        public RigToAnimMap(BinaryReaderEx br)
        {
            Read(br);
        }

        /// <summary>
        /// Method for reading a MorphemeBundle
        /// </summary>
        public override void Read(BinaryReaderEx br)
        {
            base.Read(br);
            var structStart = br.Position;
            mappingType = (RigToAnimMapType)br.ReadVarint();
            var usedFlagsPointer = br.ReadVarint();
            skelAnimMapFormat = new MorphemeSizeAlignFormatting()
            {
                dataSize = br.ReadVarint(),
                dataAlignment = br.ReadInt32(),
                unkValue = isX64 ? br.ReadInt32() : 0
            };
            var animToRigPointer = br.ReadVarint();

            //Read BitArray
            br.Position = structStart + usedFlagsPointer;
            bitsUsed = br.ReadInt32();
            var uintsUsed = br.ReadInt32();
            bitUintSet = new List<uint>();
            for (int i = 0; i < uintsUsed; i++)
            {
                bitUintSet.Add(br.ReadUInt32());
            }

            br.Position = structStart + animToRigPointer;
            switch(mappingType)
            {
                case RigToAnimMapType.MapPairs:
                    rigToAnimMap = new RigToAnimEntryMap(br);
                    break;
                case RigToAnimMapType.AnimToRig:
                    animToRigMap = new AnimToRigTableMap(br);
                    break;
                case RigToAnimMapType.CompToRig:
                default:
                    break;
            }

            //Skip to end in case of oddities
            br.Position = structStart + format.dataSize;
        }

        /// <summary>
        /// Method for writing a MorphemeBundle
        /// </summary>
        public override void Write(BinaryWriterEx bw)
        {
            base.Write(bw);
            throw new NotImplementedException();
        }
    }
}
