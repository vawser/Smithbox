using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BundleType
    {
        ANIMATION_BUNDLE = 0,
    }
    
    public partial class hkbAssetBundle : hkReferencedObject
    {
        public override uint Signature { get => 282321521; }
        
        public List<hkReferencedObject> m_assets;
        public string m_name;
        public BundleType m_type;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_assets = des.ReadClassPointerArray<hkReferencedObject>(br);
            m_name = des.ReadStringPointer(br);
            m_type = (BundleType)br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkReferencedObject>(bw, m_assets);
            s.WriteStringPointer(bw, m_name);
            bw.WriteSByte((sbyte)m_type);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
