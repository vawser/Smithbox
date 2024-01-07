using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbProjectStringData : hkReferencedObject
    {
        public override uint Signature { get => 3389571770; }
        
        public List<string> m_animationFilenames;
        public List<string> m_behaviorFilenames;
        public List<string> m_characterFilenames;
        public List<string> m_eventNames;
        public string m_animationPath;
        public string m_behaviorPath;
        public string m_characterPath;
        public string m_scriptsPath;
        public string m_fullPathToSource;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_animationFilenames = des.ReadStringPointerArray(br);
            m_behaviorFilenames = des.ReadStringPointerArray(br);
            m_characterFilenames = des.ReadStringPointerArray(br);
            m_eventNames = des.ReadStringPointerArray(br);
            m_animationPath = des.ReadStringPointer(br);
            m_behaviorPath = des.ReadStringPointer(br);
            m_characterPath = des.ReadStringPointer(br);
            m_scriptsPath = des.ReadStringPointer(br);
            m_fullPathToSource = des.ReadStringPointer(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointerArray(bw, m_animationFilenames);
            s.WriteStringPointerArray(bw, m_behaviorFilenames);
            s.WriteStringPointerArray(bw, m_characterFilenames);
            s.WriteStringPointerArray(bw, m_eventNames);
            s.WriteStringPointer(bw, m_animationPath);
            s.WriteStringPointer(bw, m_behaviorPath);
            s.WriteStringPointer(bw, m_characterPath);
            s.WriteStringPointer(bw, m_scriptsPath);
            s.WriteStringPointer(bw, m_fullPathToSource);
            bw.WriteUInt64(0);
        }
    }
}
