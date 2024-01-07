using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum SplitAndGenerateOptions
    {
        SIMPLIFY_INDIVIDUALLY = 0,
        SIMPLIFY_INDIVIDUALLY_BORDER_PRESERVE = 1,
        SIMPLIFY_ALL_AT_ONCE = 2,
        SIMPLIFY_TWO_PASS = 3,
    }
    
    public enum SplitMethod
    {
        SPLIT_UNIFORM = 0,
        SPLIT_ADAPTIVE = 1,
    }
    
    public partial class hkaiSplitGenerationUtils : IHavokObject
    {
        public virtual uint Signature { get => 3498592564; }
        
        
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
