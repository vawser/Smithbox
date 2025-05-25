using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleDefaultEngine : hknpVehicleEngine
    {
        public override uint Signature { get => 2784585923; }
        
        public float m_minRPM;
        public float m_optRPM;
        public float m_maxRPM;
        public float m_maxTorque;
        public float m_torqueFactorAtMinRPM;
        public float m_torqueFactorAtMaxRPM;
        public float m_resistanceFactorAtMinRPM;
        public float m_resistanceFactorAtOptRPM;
        public float m_resistanceFactorAtMaxRPM;
        public float m_clutchSlipRPM;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_minRPM = br.ReadSingle();
            m_optRPM = br.ReadSingle();
            m_maxRPM = br.ReadSingle();
            m_maxTorque = br.ReadSingle();
            m_torqueFactorAtMinRPM = br.ReadSingle();
            m_torqueFactorAtMaxRPM = br.ReadSingle();
            m_resistanceFactorAtMinRPM = br.ReadSingle();
            m_resistanceFactorAtOptRPM = br.ReadSingle();
            m_resistanceFactorAtMaxRPM = br.ReadSingle();
            m_clutchSlipRPM = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_minRPM);
            bw.WriteSingle(m_optRPM);
            bw.WriteSingle(m_maxRPM);
            bw.WriteSingle(m_maxTorque);
            bw.WriteSingle(m_torqueFactorAtMinRPM);
            bw.WriteSingle(m_torqueFactorAtMaxRPM);
            bw.WriteSingle(m_resistanceFactorAtMinRPM);
            bw.WriteSingle(m_resistanceFactorAtOptRPM);
            bw.WriteSingle(m_resistanceFactorAtMaxRPM);
            bw.WriteSingle(m_clutchSlipRPM);
        }
    }
}
