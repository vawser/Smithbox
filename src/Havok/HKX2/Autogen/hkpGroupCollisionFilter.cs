using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpGroupCollisionFilter : hkpCollisionFilter
    {
        public override uint Signature { get => 667383739; }
        
        public bool m_noGroupCollisionEnabled;
        public uint m_collisionGroups_0;
        public uint m_collisionGroups_1;
        public uint m_collisionGroups_2;
        public uint m_collisionGroups_3;
        public uint m_collisionGroups_4;
        public uint m_collisionGroups_5;
        public uint m_collisionGroups_6;
        public uint m_collisionGroups_7;
        public uint m_collisionGroups_8;
        public uint m_collisionGroups_9;
        public uint m_collisionGroups_10;
        public uint m_collisionGroups_11;
        public uint m_collisionGroups_12;
        public uint m_collisionGroups_13;
        public uint m_collisionGroups_14;
        public uint m_collisionGroups_15;
        public uint m_collisionGroups_16;
        public uint m_collisionGroups_17;
        public uint m_collisionGroups_18;
        public uint m_collisionGroups_19;
        public uint m_collisionGroups_20;
        public uint m_collisionGroups_21;
        public uint m_collisionGroups_22;
        public uint m_collisionGroups_23;
        public uint m_collisionGroups_24;
        public uint m_collisionGroups_25;
        public uint m_collisionGroups_26;
        public uint m_collisionGroups_27;
        public uint m_collisionGroups_28;
        public uint m_collisionGroups_29;
        public uint m_collisionGroups_30;
        public uint m_collisionGroups_31;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_noGroupCollisionEnabled = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_collisionGroups_0 = br.ReadUInt32();
            m_collisionGroups_1 = br.ReadUInt32();
            m_collisionGroups_2 = br.ReadUInt32();
            m_collisionGroups_3 = br.ReadUInt32();
            m_collisionGroups_4 = br.ReadUInt32();
            m_collisionGroups_5 = br.ReadUInt32();
            m_collisionGroups_6 = br.ReadUInt32();
            m_collisionGroups_7 = br.ReadUInt32();
            m_collisionGroups_8 = br.ReadUInt32();
            m_collisionGroups_9 = br.ReadUInt32();
            m_collisionGroups_10 = br.ReadUInt32();
            m_collisionGroups_11 = br.ReadUInt32();
            m_collisionGroups_12 = br.ReadUInt32();
            m_collisionGroups_13 = br.ReadUInt32();
            m_collisionGroups_14 = br.ReadUInt32();
            m_collisionGroups_15 = br.ReadUInt32();
            m_collisionGroups_16 = br.ReadUInt32();
            m_collisionGroups_17 = br.ReadUInt32();
            m_collisionGroups_18 = br.ReadUInt32();
            m_collisionGroups_19 = br.ReadUInt32();
            m_collisionGroups_20 = br.ReadUInt32();
            m_collisionGroups_21 = br.ReadUInt32();
            m_collisionGroups_22 = br.ReadUInt32();
            m_collisionGroups_23 = br.ReadUInt32();
            m_collisionGroups_24 = br.ReadUInt32();
            m_collisionGroups_25 = br.ReadUInt32();
            m_collisionGroups_26 = br.ReadUInt32();
            m_collisionGroups_27 = br.ReadUInt32();
            m_collisionGroups_28 = br.ReadUInt32();
            m_collisionGroups_29 = br.ReadUInt32();
            m_collisionGroups_30 = br.ReadUInt32();
            m_collisionGroups_31 = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_noGroupCollisionEnabled);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteUInt32(m_collisionGroups_0);
            bw.WriteUInt32(m_collisionGroups_1);
            bw.WriteUInt32(m_collisionGroups_2);
            bw.WriteUInt32(m_collisionGroups_3);
            bw.WriteUInt32(m_collisionGroups_4);
            bw.WriteUInt32(m_collisionGroups_5);
            bw.WriteUInt32(m_collisionGroups_6);
            bw.WriteUInt32(m_collisionGroups_7);
            bw.WriteUInt32(m_collisionGroups_8);
            bw.WriteUInt32(m_collisionGroups_9);
            bw.WriteUInt32(m_collisionGroups_10);
            bw.WriteUInt32(m_collisionGroups_11);
            bw.WriteUInt32(m_collisionGroups_12);
            bw.WriteUInt32(m_collisionGroups_13);
            bw.WriteUInt32(m_collisionGroups_14);
            bw.WriteUInt32(m_collisionGroups_15);
            bw.WriteUInt32(m_collisionGroups_16);
            bw.WriteUInt32(m_collisionGroups_17);
            bw.WriteUInt32(m_collisionGroups_18);
            bw.WriteUInt32(m_collisionGroups_19);
            bw.WriteUInt32(m_collisionGroups_20);
            bw.WriteUInt32(m_collisionGroups_21);
            bw.WriteUInt32(m_collisionGroups_22);
            bw.WriteUInt32(m_collisionGroups_23);
            bw.WriteUInt32(m_collisionGroups_24);
            bw.WriteUInt32(m_collisionGroups_25);
            bw.WriteUInt32(m_collisionGroups_26);
            bw.WriteUInt32(m_collisionGroups_27);
            bw.WriteUInt32(m_collisionGroups_28);
            bw.WriteUInt32(m_collisionGroups_29);
            bw.WriteUInt32(m_collisionGroups_30);
            bw.WriteUInt32(m_collisionGroups_31);
            bw.WriteUInt32(0);
        }
    }
}
