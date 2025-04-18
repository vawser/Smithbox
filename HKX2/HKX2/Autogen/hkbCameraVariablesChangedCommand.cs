using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCameraVariablesChangedCommand : hkReferencedObject
    {
        public override uint Signature { get => 855142481; }
        
        public List<string> m_cameraVariableFloatNames;
        public List<float> m_cameraFloatValues;
        public List<string> m_cameraVariableVectorNames;
        public List<Vector4> m_cameraVectorValues;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_cameraVariableFloatNames = des.ReadStringPointerArray(br);
            m_cameraFloatValues = des.ReadSingleArray(br);
            m_cameraVariableVectorNames = des.ReadStringPointerArray(br);
            m_cameraVectorValues = des.ReadVector4Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointerArray(bw, m_cameraVariableFloatNames);
            s.WriteSingleArray(bw, m_cameraFloatValues);
            s.WriteStringPointerArray(bw, m_cameraVariableVectorNames);
            s.WriteVector4Array(bw, m_cameraVectorValues);
        }
    }
}
