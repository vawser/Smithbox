using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaAnimationContainer : hkReferencedObject
    {
        public override uint Signature { get => 646291276; }
        
        public List<hkaSkeleton> m_skeletons;
        public List<hkaAnimation> m_animations;
        public List<hkaAnimationBinding> m_bindings;
        public List<hkaBoneAttachment> m_attachments;
        public List<hkaMeshBinding> m_skins;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_skeletons = des.ReadClassPointerArray<hkaSkeleton>(br);
            m_animations = des.ReadClassPointerArray<hkaAnimation>(br);
            m_bindings = des.ReadClassPointerArray<hkaAnimationBinding>(br);
            m_attachments = des.ReadClassPointerArray<hkaBoneAttachment>(br);
            m_skins = des.ReadClassPointerArray<hkaMeshBinding>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkaSkeleton>(bw, m_skeletons);
            s.WriteClassPointerArray<hkaAnimation>(bw, m_animations);
            s.WriteClassPointerArray<hkaAnimationBinding>(bw, m_bindings);
            s.WriteClassPointerArray<hkaBoneAttachment>(bw, m_attachments);
            s.WriteClassPointerArray<hkaMeshBinding>(bw, m_skins);
        }
    }
}
