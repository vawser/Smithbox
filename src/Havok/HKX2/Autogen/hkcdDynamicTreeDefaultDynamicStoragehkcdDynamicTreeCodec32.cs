using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdDynamicTreeDefaultDynamicStoragehkcdDynamicTreeCodec32 : hkcdDynamicTreeDynamicStorage0hkcdDynamicTreeAnisotropicMetrichkcdDynamicTreeCodec32
    {
        public override uint Signature { get => 5325906; }
        
        
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
