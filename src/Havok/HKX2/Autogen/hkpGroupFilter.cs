using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpGroupFilter : hkpCollisionFilter
    {
        public override uint Signature { get => 2289863781; }
        
        public int m_nextFreeSystemGroup;
        public uint m_collisionLookupTable_0;
        public uint m_collisionLookupTable_1;
        public uint m_collisionLookupTable_2;
        public uint m_collisionLookupTable_3;
        public uint m_collisionLookupTable_4;
        public uint m_collisionLookupTable_5;
        public uint m_collisionLookupTable_6;
        public uint m_collisionLookupTable_7;
        public uint m_collisionLookupTable_8;
        public uint m_collisionLookupTable_9;
        public uint m_collisionLookupTable_10;
        public uint m_collisionLookupTable_11;
        public uint m_collisionLookupTable_12;
        public uint m_collisionLookupTable_13;
        public uint m_collisionLookupTable_14;
        public uint m_collisionLookupTable_15;
        public uint m_collisionLookupTable_16;
        public uint m_collisionLookupTable_17;
        public uint m_collisionLookupTable_18;
        public uint m_collisionLookupTable_19;
        public uint m_collisionLookupTable_20;
        public uint m_collisionLookupTable_21;
        public uint m_collisionLookupTable_22;
        public uint m_collisionLookupTable_23;
        public uint m_collisionLookupTable_24;
        public uint m_collisionLookupTable_25;
        public uint m_collisionLookupTable_26;
        public uint m_collisionLookupTable_27;
        public uint m_collisionLookupTable_28;
        public uint m_collisionLookupTable_29;
        public uint m_collisionLookupTable_30;
        public uint m_collisionLookupTable_31;
        public Vector4 m_pad256_0;
        public Vector4 m_pad256_1;
        public Vector4 m_pad256_2;
        public Vector4 m_pad256_3;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_nextFreeSystemGroup = br.ReadInt32();
            m_collisionLookupTable_0 = br.ReadUInt32();
            m_collisionLookupTable_1 = br.ReadUInt32();
            m_collisionLookupTable_2 = br.ReadUInt32();
            m_collisionLookupTable_3 = br.ReadUInt32();
            m_collisionLookupTable_4 = br.ReadUInt32();
            m_collisionLookupTable_5 = br.ReadUInt32();
            m_collisionLookupTable_6 = br.ReadUInt32();
            m_collisionLookupTable_7 = br.ReadUInt32();
            m_collisionLookupTable_8 = br.ReadUInt32();
            m_collisionLookupTable_9 = br.ReadUInt32();
            m_collisionLookupTable_10 = br.ReadUInt32();
            m_collisionLookupTable_11 = br.ReadUInt32();
            m_collisionLookupTable_12 = br.ReadUInt32();
            m_collisionLookupTable_13 = br.ReadUInt32();
            m_collisionLookupTable_14 = br.ReadUInt32();
            m_collisionLookupTable_15 = br.ReadUInt32();
            m_collisionLookupTable_16 = br.ReadUInt32();
            m_collisionLookupTable_17 = br.ReadUInt32();
            m_collisionLookupTable_18 = br.ReadUInt32();
            m_collisionLookupTable_19 = br.ReadUInt32();
            m_collisionLookupTable_20 = br.ReadUInt32();
            m_collisionLookupTable_21 = br.ReadUInt32();
            m_collisionLookupTable_22 = br.ReadUInt32();
            m_collisionLookupTable_23 = br.ReadUInt32();
            m_collisionLookupTable_24 = br.ReadUInt32();
            m_collisionLookupTable_25 = br.ReadUInt32();
            m_collisionLookupTable_26 = br.ReadUInt32();
            m_collisionLookupTable_27 = br.ReadUInt32();
            m_collisionLookupTable_28 = br.ReadUInt32();
            m_collisionLookupTable_29 = br.ReadUInt32();
            m_collisionLookupTable_30 = br.ReadUInt32();
            m_collisionLookupTable_31 = br.ReadUInt32();
            br.ReadUInt32();
            m_pad256_0 = des.ReadVector4(br);
            m_pad256_1 = des.ReadVector4(br);
            m_pad256_2 = des.ReadVector4(br);
            m_pad256_3 = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_nextFreeSystemGroup);
            bw.WriteUInt32(m_collisionLookupTable_0);
            bw.WriteUInt32(m_collisionLookupTable_1);
            bw.WriteUInt32(m_collisionLookupTable_2);
            bw.WriteUInt32(m_collisionLookupTable_3);
            bw.WriteUInt32(m_collisionLookupTable_4);
            bw.WriteUInt32(m_collisionLookupTable_5);
            bw.WriteUInt32(m_collisionLookupTable_6);
            bw.WriteUInt32(m_collisionLookupTable_7);
            bw.WriteUInt32(m_collisionLookupTable_8);
            bw.WriteUInt32(m_collisionLookupTable_9);
            bw.WriteUInt32(m_collisionLookupTable_10);
            bw.WriteUInt32(m_collisionLookupTable_11);
            bw.WriteUInt32(m_collisionLookupTable_12);
            bw.WriteUInt32(m_collisionLookupTable_13);
            bw.WriteUInt32(m_collisionLookupTable_14);
            bw.WriteUInt32(m_collisionLookupTable_15);
            bw.WriteUInt32(m_collisionLookupTable_16);
            bw.WriteUInt32(m_collisionLookupTable_17);
            bw.WriteUInt32(m_collisionLookupTable_18);
            bw.WriteUInt32(m_collisionLookupTable_19);
            bw.WriteUInt32(m_collisionLookupTable_20);
            bw.WriteUInt32(m_collisionLookupTable_21);
            bw.WriteUInt32(m_collisionLookupTable_22);
            bw.WriteUInt32(m_collisionLookupTable_23);
            bw.WriteUInt32(m_collisionLookupTable_24);
            bw.WriteUInt32(m_collisionLookupTable_25);
            bw.WriteUInt32(m_collisionLookupTable_26);
            bw.WriteUInt32(m_collisionLookupTable_27);
            bw.WriteUInt32(m_collisionLookupTable_28);
            bw.WriteUInt32(m_collisionLookupTable_29);
            bw.WriteUInt32(m_collisionLookupTable_30);
            bw.WriteUInt32(m_collisionLookupTable_31);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_pad256_0);
            s.WriteVector4(bw, m_pad256_1);
            s.WriteVector4(bw, m_pad256_2);
            s.WriteVector4(bw, m_pad256_3);
        }
    }
}
