using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum SpuCollisionCallbackEventFilter
    {
        SPU_SEND_NONE = 0,
        SPU_SEND_CONTACT_POINT_ADDED = 1,
        SPU_SEND_CONTACT_POINT_PROCESS = 2,
        SPU_SEND_CONTACT_POINT_REMOVED = 4,
        SPU_SEND_CONTACT_POINT_ADDED_OR_PROCESS = 3,
    }
    
    public partial class hkpEntity : hkpWorldObject
    {
        public override uint Signature { get => 2282002532; }
        
        public hkpMaterial m_material;
        public float m_damageMultiplier;
        public ushort m_storageIndex;
        public ushort m_contactPointCallbackDelay;
        public sbyte m_autoRemoveLevel;
        public byte m_numShapeKeysInContactPointProperties;
        public byte m_responseModifierFlags;
        public uint m_uid;
        public hkpEntitySpuCollisionCallback m_spuCollisionCallback;
        public hkpMaxSizeMotion m_motion;
        public hkLocalFrame m_localFrame;
        public uint m_npData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_material = new hkpMaterial();
            m_material.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
            m_damageMultiplier = br.ReadSingle();
            br.ReadUInt64();
            br.ReadUInt64();
            m_storageIndex = br.ReadUInt16();
            m_contactPointCallbackDelay = br.ReadUInt16();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_autoRemoveLevel = br.ReadSByte();
            m_numShapeKeysInContactPointProperties = br.ReadByte();
            m_responseModifierFlags = br.ReadByte();
            br.ReadByte();
            m_uid = br.ReadUInt32();
            m_spuCollisionCallback = new hkpEntitySpuCollisionCallback();
            m_spuCollisionCallback.Read(des, br);
            br.ReadUInt64();
            m_motion = new hkpMaxSizeMotion();
            m_motion.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_localFrame = des.ReadClassPointer<hkLocalFrame>(br);
            br.ReadUInt64();
            m_npData = br.ReadUInt32();
            br.ReadUInt64();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_material.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteSingle(m_damageMultiplier);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(m_storageIndex);
            bw.WriteUInt16(m_contactPointCallbackDelay);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteSByte(m_autoRemoveLevel);
            bw.WriteByte(m_numShapeKeysInContactPointProperties);
            bw.WriteByte(m_responseModifierFlags);
            bw.WriteByte(0);
            bw.WriteUInt32(m_uid);
            m_spuCollisionCallback.Write(s, bw);
            bw.WriteUInt64(0);
            m_motion.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkLocalFrame>(bw, m_localFrame);
            bw.WriteUInt64(0);
            bw.WriteUInt32(m_npData);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
        }
    }
}
