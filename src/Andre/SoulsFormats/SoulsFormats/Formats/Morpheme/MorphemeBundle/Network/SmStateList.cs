using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle.Network
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// State Machine State List
    /// </summary>
    public class SmStateList
    {
        public long numStateMachinesNodes;
        public List<short> stateMachineNodeIDs = new List<short>();

        public SmStateList() { }

        public SmStateList(BinaryReaderEx br)
        {
            Read(br);
        }

        public void Read(BinaryReaderEx br)
        {
            var dataStart = br.Position;
            numStateMachinesNodes = br.ReadVarint();

            var stateMachineNodeIdsOffset = br.ReadVarint();
            br.Position = dataStart + stateMachineNodeIdsOffset;
            for (int i = 0; i < numStateMachinesNodes; i++)
            {
                stateMachineNodeIDs.Add(br.ReadInt16());
            }
        }

        public void Write(BinaryWriterEx bw)
        {
            bw.WriteVarint(stateMachineNodeIDs.Count);
            bw.WriteVarint(bw.VarintLong ? 0x10 : 0x8);
            for (int i = 0; i < stateMachineNodeIDs.Count; i++)
            {
                bw.WriteInt16(stateMachineNodeIDs[i]);
            }
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
