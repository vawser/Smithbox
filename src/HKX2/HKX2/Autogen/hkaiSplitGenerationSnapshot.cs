using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSplitGenerationSnapshot : IHavokObject
    {
        public virtual uint Signature { get => 2576061005; }
        
        public hkaiNavMeshGenerationSnapshot m_generationSnapshot;
        public hkaiSplitGenerationUtilsSettings m_splitSettings;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_generationSnapshot = new hkaiNavMeshGenerationSnapshot();
            m_generationSnapshot.Read(des, br);
            m_splitSettings = new hkaiSplitGenerationUtilsSettings();
            m_splitSettings.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_generationSnapshot.Write(s, bw);
            m_splitSettings.Write(s, bw);
        }
    }
}
