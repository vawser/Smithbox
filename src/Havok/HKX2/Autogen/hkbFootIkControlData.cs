using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbFootIkControlData : IHavokObject
    {
        public virtual uint Signature { get => 2132322726; }
        
        public hkbFootIkGains m_gains;
        public float m_enabled_0;
        public float m_enabled_1;
        public float m_enabled_2;
        public float m_enabled_3;
        public float m_enabled_4;
        public float m_enabled_5;
        public float m_enabled_6;
        public float m_enabled_7;
        public float m_enabled_8;
        public float m_enabled_9;
        public float m_enabled_10;
        public float m_enabled_11;
        public float m_enabled_12;
        public float m_enabled_13;
        public float m_enabled_14;
        public float m_enabled_15;
        public float m_enabled_16;
        public float m_enabled_17;
        public float m_enabled_18;
        public float m_enabled_19;
        public float m_enabled_20;
        public float m_enabled_21;
        public float m_enabled_22;
        public float m_enabled_23;
        public float m_enabled_24;
        public float m_enabled_25;
        public float m_enabled_26;
        public float m_enabled_27;
        public float m_enabled_28;
        public float m_enabled_29;
        public float m_enabled_30;
        public float m_enabled_31;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_gains = new hkbFootIkGains();
            m_gains.Read(des, br);
            m_enabled_0 = br.ReadSingle();
            m_enabled_1 = br.ReadSingle();
            m_enabled_2 = br.ReadSingle();
            m_enabled_3 = br.ReadSingle();
            m_enabled_4 = br.ReadSingle();
            m_enabled_5 = br.ReadSingle();
            m_enabled_6 = br.ReadSingle();
            m_enabled_7 = br.ReadSingle();
            m_enabled_8 = br.ReadSingle();
            m_enabled_9 = br.ReadSingle();
            m_enabled_10 = br.ReadSingle();
            m_enabled_11 = br.ReadSingle();
            m_enabled_12 = br.ReadSingle();
            m_enabled_13 = br.ReadSingle();
            m_enabled_14 = br.ReadSingle();
            m_enabled_15 = br.ReadSingle();
            m_enabled_16 = br.ReadSingle();
            m_enabled_17 = br.ReadSingle();
            m_enabled_18 = br.ReadSingle();
            m_enabled_19 = br.ReadSingle();
            m_enabled_20 = br.ReadSingle();
            m_enabled_21 = br.ReadSingle();
            m_enabled_22 = br.ReadSingle();
            m_enabled_23 = br.ReadSingle();
            m_enabled_24 = br.ReadSingle();
            m_enabled_25 = br.ReadSingle();
            m_enabled_26 = br.ReadSingle();
            m_enabled_27 = br.ReadSingle();
            m_enabled_28 = br.ReadSingle();
            m_enabled_29 = br.ReadSingle();
            m_enabled_30 = br.ReadSingle();
            m_enabled_31 = br.ReadSingle();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_gains.Write(s, bw);
            bw.WriteSingle(m_enabled_0);
            bw.WriteSingle(m_enabled_1);
            bw.WriteSingle(m_enabled_2);
            bw.WriteSingle(m_enabled_3);
            bw.WriteSingle(m_enabled_4);
            bw.WriteSingle(m_enabled_5);
            bw.WriteSingle(m_enabled_6);
            bw.WriteSingle(m_enabled_7);
            bw.WriteSingle(m_enabled_8);
            bw.WriteSingle(m_enabled_9);
            bw.WriteSingle(m_enabled_10);
            bw.WriteSingle(m_enabled_11);
            bw.WriteSingle(m_enabled_12);
            bw.WriteSingle(m_enabled_13);
            bw.WriteSingle(m_enabled_14);
            bw.WriteSingle(m_enabled_15);
            bw.WriteSingle(m_enabled_16);
            bw.WriteSingle(m_enabled_17);
            bw.WriteSingle(m_enabled_18);
            bw.WriteSingle(m_enabled_19);
            bw.WriteSingle(m_enabled_20);
            bw.WriteSingle(m_enabled_21);
            bw.WriteSingle(m_enabled_22);
            bw.WriteSingle(m_enabled_23);
            bw.WriteSingle(m_enabled_24);
            bw.WriteSingle(m_enabled_25);
            bw.WriteSingle(m_enabled_26);
            bw.WriteSingle(m_enabled_27);
            bw.WriteSingle(m_enabled_28);
            bw.WriteSingle(m_enabled_29);
            bw.WriteSingle(m_enabled_30);
            bw.WriteSingle(m_enabled_31);
            bw.WriteUInt64(0);
        }
    }
}
