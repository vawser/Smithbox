using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclMoveFixedParticlesSetupObject : hclOperatorSetupObject
    {
        public override uint Signature { get => 386333361; }
        
        public string m_name;
        public hclSimClothSetupObject m_simClothSetupObject;
        public hclBufferSetupObject m_displayBufferSetup;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_simClothSetupObject = des.ReadClassPointer<hclSimClothSetupObject>(br);
            m_displayBufferSetup = des.ReadClassPointer<hclBufferSetupObject>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclSimClothSetupObject>(bw, m_simClothSetupObject);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_displayBufferSetup);
        }
    }
}
