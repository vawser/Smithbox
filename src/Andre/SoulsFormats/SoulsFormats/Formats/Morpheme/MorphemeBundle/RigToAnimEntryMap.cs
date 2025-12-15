using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// RigToAnimMap 
    /// </summary>
    public class RigToAnimEntryMap
    {
        /// <summary>
        /// Number of mappings entries
        /// </summary>
        public int numberOfEntries;
        /// <summary>
        /// A table of (rig, anim) channel mappings.
        /// </summary>
        public List<MapPair> mappings;

        /// <summary>
        /// Basic RigToAnimEntryMap Constructor
        /// </summary>
        public RigToAnimEntryMap()
        {

        }

        /// <summary>
        /// Reading RigToAnimEntryMap Constructor
        /// </summary>
        public RigToAnimEntryMap(BinaryReaderEx br)
        {
            Read(br);
        }

        /// <summary>
        /// RigToAnimEntryMap Reader
        /// </summary>
        public void Read(BinaryReaderEx br)
        {
            var start = br.Position;
            numberOfEntries = br.ReadInt32();
            var mappingsPtr = br.ReadVarint();
            br.Position = start + mappingsPtr;

            mappings = new List<MapPair>();
            for (int i = 0; i < numberOfEntries; i++)
            {
                mappings.Add(new MapPair()
                {
                    rigChannelIndex = br.ReadUInt16(),
                    animChannelIndex = br.ReadUInt16(),
                });
            }
        }
    }

    /// <summary>
    /// Rig to Anim Mapping
    /// </summary>
    public struct MapPair
    {
        /// <summary>
        /// Channel index for the rig
        /// </summary>
        public ushort rigChannelIndex;
        /// <summary>
        /// Channel index for the anim rig
        /// </summary>
        public ushort animChannelIndex;
    }
}
