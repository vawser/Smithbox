using System.Collections.Generic;

namespace SoulsFormats.Formats.Morpheme.MorphemeBundle.Network.NodeAttrib
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class NodeAttribSourceEventTrack : NodeAttribBase
    {
        public int trackCount;
        public int padding;
        public List<int> trackSignatures;
        public List<int> trackSize;
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
