using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum VertexChannelType
    {
        HCL_VERTEX_CHANNEL_INVALID = 0,
        HCL_VERTEX_CHANNEL_FLOAT = 1,
        HCL_VERTEX_CHANNEL_DISTANCE = 2,
        HCL_VERTEX_CHANNEL_ANGLE = 3,
        HCL_VERTEX_CHANNEL_SELECTION = 4,
    }
    
    public enum TriangleChannelType
    {
        HCL_TRIANGLE_CHANNEL_INVALID = 0,
        HCL_TRIANGLE_CHANNEL_SELECTION = 1,
    }
    
    public enum EdgeChannelType
    {
        HCL_EDGE_CHANNEL_INVALID = 0,
        HCL_EDGE_CHANNEL_SELECTION = 1,
    }
    
    public partial class hclSetupMesh : hkReferencedObject
    {
        public override uint Signature { get => 1262840578; }
        
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
        }
    }
}
