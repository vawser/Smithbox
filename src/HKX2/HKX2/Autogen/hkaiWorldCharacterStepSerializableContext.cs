using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiWorldCharacterStepSerializableContext : hkReferencedObject
    {
        public override uint Signature { get => 3240525311; }
        
        public CharacterCallbackType m_callbackType;
        public float m_timestep;
        public List<hkaiCharacter> m_characters;
        public List<hkaiLocalSteeringInput> m_localSteeringInputs;
        public List<hkaiObstacleGenerator> m_obstacleGenerators;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_callbackType = (CharacterCallbackType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_timestep = br.ReadSingle();
            m_characters = des.ReadClassPointerArray<hkaiCharacter>(br);
            m_localSteeringInputs = des.ReadClassArray<hkaiLocalSteeringInput>(br);
            m_obstacleGenerators = des.ReadClassPointerArray<hkaiObstacleGenerator>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteByte((byte)m_callbackType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_timestep);
            s.WriteClassPointerArray<hkaiCharacter>(bw, m_characters);
            s.WriteClassArray<hkaiLocalSteeringInput>(bw, m_localSteeringInputs);
            s.WriteClassPointerArray<hkaiObstacleGenerator>(bw, m_obstacleGenerators);
        }
    }
}
