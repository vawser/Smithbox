using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum UserEdgeDirection
    {
        USER_EDGE_BLOCKED = 0,
        USER_EDGE_A_TO_B = 1,
        USER_EDGE_B_TO_A = 2,
        USER_EDGE_BIDIRECTIONAL = 3,
    }
    
    public enum UserEdgeSetupSpace
    {
        USER_EDGE_SPACE_WORLD = 0,
        USER_EDGE_SPACE_LOCAL = 1,
    }
    
    public enum ClearanceResetMode
    {
        RESET_CLEARANCE_CACHE = 0,
        DONT_RESET_CLEARANCE_CACHE = 1,
    }
    
    public partial class hkaiUserEdgeUtils : IHavokObject
    {
        public virtual uint Signature { get => 454187807; }
        
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(0);
        }
    }
}
