using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbAiControlPathToCommand : hkReferencedObject
    {
        public override uint Signature { get => 2977775141; }
        
        public ulong m_characterId;
        public Vector4 m_goalPoint;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_characterId = br.ReadUInt64();
            br.ReadUInt64();
            m_goalPoint = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_characterId);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_goalPoint);
        }
    }
}
