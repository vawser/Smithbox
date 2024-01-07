using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiDefaultAstarCostModifier : hkaiAstarCostModifier
    {
        public override uint Signature { get => 1154498593; }
        
        public float m_maxCostPenalty;
        public short m_costMultiplierLookupTable_0;
        public short m_costMultiplierLookupTable_1;
        public short m_costMultiplierLookupTable_2;
        public short m_costMultiplierLookupTable_3;
        public short m_costMultiplierLookupTable_4;
        public short m_costMultiplierLookupTable_5;
        public short m_costMultiplierLookupTable_6;
        public short m_costMultiplierLookupTable_7;
        public short m_costMultiplierLookupTable_8;
        public short m_costMultiplierLookupTable_9;
        public short m_costMultiplierLookupTable_10;
        public short m_costMultiplierLookupTable_11;
        public short m_costMultiplierLookupTable_12;
        public short m_costMultiplierLookupTable_13;
        public short m_costMultiplierLookupTable_14;
        public short m_costMultiplierLookupTable_15;
        public short m_costMultiplierLookupTable_16;
        public short m_costMultiplierLookupTable_17;
        public short m_costMultiplierLookupTable_18;
        public short m_costMultiplierLookupTable_19;
        public short m_costMultiplierLookupTable_20;
        public short m_costMultiplierLookupTable_21;
        public short m_costMultiplierLookupTable_22;
        public short m_costMultiplierLookupTable_23;
        public short m_costMultiplierLookupTable_24;
        public short m_costMultiplierLookupTable_25;
        public short m_costMultiplierLookupTable_26;
        public short m_costMultiplierLookupTable_27;
        public short m_costMultiplierLookupTable_28;
        public short m_costMultiplierLookupTable_29;
        public short m_costMultiplierLookupTable_30;
        public short m_costMultiplierLookupTable_31;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_maxCostPenalty = br.ReadSingle();
            m_costMultiplierLookupTable_0 = br.ReadInt16();
            m_costMultiplierLookupTable_1 = br.ReadInt16();
            m_costMultiplierLookupTable_2 = br.ReadInt16();
            m_costMultiplierLookupTable_3 = br.ReadInt16();
            m_costMultiplierLookupTable_4 = br.ReadInt16();
            m_costMultiplierLookupTable_5 = br.ReadInt16();
            m_costMultiplierLookupTable_6 = br.ReadInt16();
            m_costMultiplierLookupTable_7 = br.ReadInt16();
            m_costMultiplierLookupTable_8 = br.ReadInt16();
            m_costMultiplierLookupTable_9 = br.ReadInt16();
            m_costMultiplierLookupTable_10 = br.ReadInt16();
            m_costMultiplierLookupTable_11 = br.ReadInt16();
            m_costMultiplierLookupTable_12 = br.ReadInt16();
            m_costMultiplierLookupTable_13 = br.ReadInt16();
            m_costMultiplierLookupTable_14 = br.ReadInt16();
            m_costMultiplierLookupTable_15 = br.ReadInt16();
            m_costMultiplierLookupTable_16 = br.ReadInt16();
            m_costMultiplierLookupTable_17 = br.ReadInt16();
            m_costMultiplierLookupTable_18 = br.ReadInt16();
            m_costMultiplierLookupTable_19 = br.ReadInt16();
            m_costMultiplierLookupTable_20 = br.ReadInt16();
            m_costMultiplierLookupTable_21 = br.ReadInt16();
            m_costMultiplierLookupTable_22 = br.ReadInt16();
            m_costMultiplierLookupTable_23 = br.ReadInt16();
            m_costMultiplierLookupTable_24 = br.ReadInt16();
            m_costMultiplierLookupTable_25 = br.ReadInt16();
            m_costMultiplierLookupTable_26 = br.ReadInt16();
            m_costMultiplierLookupTable_27 = br.ReadInt16();
            m_costMultiplierLookupTable_28 = br.ReadInt16();
            m_costMultiplierLookupTable_29 = br.ReadInt16();
            m_costMultiplierLookupTable_30 = br.ReadInt16();
            m_costMultiplierLookupTable_31 = br.ReadInt16();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_maxCostPenalty);
            bw.WriteInt16(m_costMultiplierLookupTable_0);
            bw.WriteInt16(m_costMultiplierLookupTable_1);
            bw.WriteInt16(m_costMultiplierLookupTable_2);
            bw.WriteInt16(m_costMultiplierLookupTable_3);
            bw.WriteInt16(m_costMultiplierLookupTable_4);
            bw.WriteInt16(m_costMultiplierLookupTable_5);
            bw.WriteInt16(m_costMultiplierLookupTable_6);
            bw.WriteInt16(m_costMultiplierLookupTable_7);
            bw.WriteInt16(m_costMultiplierLookupTable_8);
            bw.WriteInt16(m_costMultiplierLookupTable_9);
            bw.WriteInt16(m_costMultiplierLookupTable_10);
            bw.WriteInt16(m_costMultiplierLookupTable_11);
            bw.WriteInt16(m_costMultiplierLookupTable_12);
            bw.WriteInt16(m_costMultiplierLookupTable_13);
            bw.WriteInt16(m_costMultiplierLookupTable_14);
            bw.WriteInt16(m_costMultiplierLookupTable_15);
            bw.WriteInt16(m_costMultiplierLookupTable_16);
            bw.WriteInt16(m_costMultiplierLookupTable_17);
            bw.WriteInt16(m_costMultiplierLookupTable_18);
            bw.WriteInt16(m_costMultiplierLookupTable_19);
            bw.WriteInt16(m_costMultiplierLookupTable_20);
            bw.WriteInt16(m_costMultiplierLookupTable_21);
            bw.WriteInt16(m_costMultiplierLookupTable_22);
            bw.WriteInt16(m_costMultiplierLookupTable_23);
            bw.WriteInt16(m_costMultiplierLookupTable_24);
            bw.WriteInt16(m_costMultiplierLookupTable_25);
            bw.WriteInt16(m_costMultiplierLookupTable_26);
            bw.WriteInt16(m_costMultiplierLookupTable_27);
            bw.WriteInt16(m_costMultiplierLookupTable_28);
            bw.WriteInt16(m_costMultiplierLookupTable_29);
            bw.WriteInt16(m_costMultiplierLookupTable_30);
            bw.WriteInt16(m_costMultiplierLookupTable_31);
            bw.WriteUInt32(0);
        }
    }
}
