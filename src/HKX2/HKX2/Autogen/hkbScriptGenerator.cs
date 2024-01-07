using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbScriptGenerator : hkbGenerator
    {
        public override uint Signature { get => 805998961; }
        
        public hkbGenerator m_child;
        public string m_onActivateScript;
        public string m_onPreUpdateScript;
        public string m_onGenerateScript;
        public string m_onHandleEventScript;
        public string m_onDeactivateScript;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_child = des.ReadClassPointer<hkbGenerator>(br);
            m_onActivateScript = des.ReadStringPointer(br);
            m_onPreUpdateScript = des.ReadStringPointer(br);
            m_onGenerateScript = des.ReadStringPointer(br);
            m_onHandleEventScript = des.ReadStringPointer(br);
            m_onDeactivateScript = des.ReadStringPointer(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbGenerator>(bw, m_child);
            s.WriteStringPointer(bw, m_onActivateScript);
            s.WriteStringPointer(bw, m_onPreUpdateScript);
            s.WriteStringPointer(bw, m_onGenerateScript);
            s.WriteStringPointer(bw, m_onHandleEventScript);
            s.WriteStringPointer(bw, m_onDeactivateScript);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
