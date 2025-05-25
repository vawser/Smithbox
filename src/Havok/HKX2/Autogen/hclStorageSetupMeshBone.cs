using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshBone : IHavokObject
    {
        public virtual uint Signature { get => 3822018135; }
        
        public string m_name;
        public Matrix4x4 m_boneFromSkin;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            br.ReadUInt64();
            m_boneFromSkin = des.ReadMatrix4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(0);
            s.WriteMatrix4(bw, m_boneFromSkin);
        }
    }
}
