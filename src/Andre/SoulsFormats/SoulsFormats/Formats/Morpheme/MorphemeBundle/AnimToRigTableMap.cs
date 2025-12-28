using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
    /// <summary>
    /// AnimToRigTableMap for one way mapping of anims to rig channels.
    /// </summary>
    public class AnimToRigTableMap
    {
        /// <summary>
        /// Number of animation channels
        /// </summary>
        public uint numAnimChannels;

        /// <summary>
        /// Number of used entries
        /// </summary>
        public ushort numUsedEntries;

        /// <summary>
        /// Number of Anim Channels for Level of Detail
        /// </summary>
        public ushort numAnimChannelsForLOD;

        /// <summary>
        /// Anim To Rig Entries List
        /// </summary>
        public List<ushort> animToRigEntries;

        /// <summary>
        /// Basic AnimToRigTableMap Constructor
        /// </summary>
        public AnimToRigTableMap()
        {

        }

        /// <summary>
        ///  AnimToRigTableMap Reader
        /// </summary>
        public AnimToRigTableMap(BinaryReaderEx br)
        {
            var start = br.Position;
            numAnimChannels = br.ReadUInt32();
            numUsedEntries = br.ReadUInt16();
            numAnimChannelsForLOD = br.ReadUInt16();
            var animToRigEntriesPtr = br.ReadVarint();

            br.Position = start + animToRigEntriesPtr;
            animToRigEntries = new List<ushort>();
            for(int i = 0; i < numAnimChannels; i++)
            {
                animToRigEntries.Add(br.ReadUInt16());
            }
        }
    }
}
