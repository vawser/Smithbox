using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbAssetBundleStringData : IHavokObject
    {
        public virtual uint Signature { get => 1175661485; }
        
        public string m_bundleName;
        public List<string> m_assetNames;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_bundleName = des.ReadStringPointer(br);
            m_assetNames = des.ReadStringPointerArray(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_bundleName);
            s.WriteStringPointerArray(bw, m_assetNames);
        }
    }
}
