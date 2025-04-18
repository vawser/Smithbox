using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ParameterType
    {
        UNKNOWN = 0,
        LINEAR_SPEED = 1,
        LINEAR_DIRECTION = 2,
        TURN_SPEED = 3,
    }
    
    public partial class hkaParameterizedAnimationReferenceFrame : hkaDefaultAnimatedReferenceFrame
    {
        public override uint Signature { get => 1059887376; }
        
        public List<float> m_parameterValues;
        public List<int> m_parameterTypes;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_parameterValues = des.ReadSingleArray(br);
            m_parameterTypes = des.ReadInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteSingleArray(bw, m_parameterValues);
            s.WriteInt32Array(bw, m_parameterTypes);
        }
    }
}
