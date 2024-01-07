using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBonePlanesSetupObject : hclConstraintSetSetupObject
    {
        public override uint Signature { get => 1076021204; }
        
        public string m_name;
        public hclSimulationSetupMesh m_simulationMesh;
        public hclTransformSetSetupObject m_transformSetSetup;
        public List<hclBonePlanesSetupObjectPerParticlePlane> m_perParticlePlanes;
        public List<hclBonePlanesSetupObjectGlobalPlane> m_globalPlanes;
        public List<hclBonePlanesSetupObjectPerParticleAngle> m_perParticleAngle;
        public bool m_angleSpecifiedInDegrees;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_simulationMesh = des.ReadClassPointer<hclSimulationSetupMesh>(br);
            m_transformSetSetup = des.ReadClassPointer<hclTransformSetSetupObject>(br);
            m_perParticlePlanes = des.ReadClassArray<hclBonePlanesSetupObjectPerParticlePlane>(br);
            m_globalPlanes = des.ReadClassArray<hclBonePlanesSetupObjectGlobalPlane>(br);
            m_perParticleAngle = des.ReadClassArray<hclBonePlanesSetupObjectPerParticleAngle>(br);
            m_angleSpecifiedInDegrees = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclSimulationSetupMesh>(bw, m_simulationMesh);
            s.WriteClassPointer<hclTransformSetSetupObject>(bw, m_transformSetSetup);
            s.WriteClassArray<hclBonePlanesSetupObjectPerParticlePlane>(bw, m_perParticlePlanes);
            s.WriteClassArray<hclBonePlanesSetupObjectGlobalPlane>(bw, m_globalPlanes);
            s.WriteClassArray<hclBonePlanesSetupObjectPerParticleAngle>(bw, m_perParticleAngle);
            bw.WriteBoolean(m_angleSpecifiedInDegrees);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
