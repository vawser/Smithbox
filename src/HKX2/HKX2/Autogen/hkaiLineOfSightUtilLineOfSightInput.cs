using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiLineOfSightUtilLineOfSightInput : hkaiLineOfSightUtilInputBase
    {
        public override uint Signature { get => 1732765811; }
        
        public Vector4 m_goalPoint;
        public uint m_goalFaceKey;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_goalPoint = des.ReadVector4(br);
            m_goalFaceKey = br.ReadUInt32();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_goalPoint);
            bw.WriteUInt32(m_goalFaceKey);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
