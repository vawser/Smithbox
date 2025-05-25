using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRadialSelectorGenerator : hkbGenerator
    {
        public override uint Signature { get => 131468265; }
        
        public List<hkbRadialSelectorGeneratorGeneratorPair> m_generatorPairs;
        public float m_angle;
        public float m_radius;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_generatorPairs = des.ReadClassArray<hkbRadialSelectorGeneratorGeneratorPair>(br);
            m_angle = br.ReadSingle();
            m_radius = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbRadialSelectorGeneratorGeneratorPair>(bw, m_generatorPairs);
            bw.WriteSingle(m_angle);
            bw.WriteSingle(m_radius);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
