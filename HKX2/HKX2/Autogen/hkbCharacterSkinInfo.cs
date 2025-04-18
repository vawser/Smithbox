using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCharacterSkinInfo : hkReferencedObject
    {
        public override uint Signature { get => 2933460353; }
        
        public ulong m_characterId;
        public List<ulong> m_deformableSkins;
        public List<ulong> m_rigidSkins;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            m_deformableSkins = des.ReadUInt64Array(br);
            m_rigidSkins = des.ReadUInt64Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            s.WriteUInt64Array(bw, m_deformableSkins);
            s.WriteUInt64Array(bw, m_rigidSkins);
        }
    }
}
