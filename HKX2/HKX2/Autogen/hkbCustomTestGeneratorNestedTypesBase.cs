using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCustomTestGeneratorNestedTypesBase : hkbCustomTestGeneratorComplexTypes
    {
        public override uint Signature { get => 1447418576; }
        
        public hkbGenerator m_nestedTypeHkbGeneratorPtr;
        public hkbGenerator m_nestedTypeHkbGeneratorRefPtr;
        public hkbModifier m_nestedTypeHkbModifierPtr;
        public hkbModifier m_nestedTypeHkbModifierRefPtr;
        public hkbCustomIdSelector m_nestedTypeHkbCustomIdSelectorPtr;
        public hkbCustomIdSelector m_nestedTypeHkbCustomIdSelectorRefPtr;
        public List<bool> m_nestedTypeArrayBool;
        public List<bool> m_nestedTypeArrayHkBool;
        public List<string> m_nestedTypeArrayCString;
        public List<string> m_nestedTypeArrayHkStringPtr;
        public List<sbyte> m_nestedTypeArrayHkInt8;
        public List<short> m_nestedTypeArrayHkInt16;
        public List<int> m_nestedTypeArrayHkInt32;
        public List<byte> m_nestedTypeArrayHkUint8;
        public List<ushort> m_nestedTypeArrayHkUint16;
        public List<uint> m_nestedTypeArrayHkUint32;
        public List<float> m_nestedTypeArrayHkReal;
        public List<hkbGenerator> m_nestedTypeArrayHkbGeneratorPtr;
        public List<hkbGenerator> m_nestedTypeArrayHkbGeneratorRefPtr;
        public List<hkbModifier> m_nestedTypeArrayHkbModifierPtr;
        public List<hkbModifier> m_nestedTypeArrayHkbModifierRefPtr;
        public List<hkbCustomIdSelector> m_nestedTypeArrayHkbCustomIdSelectorPtr;
        public List<hkbCustomIdSelector> m_nestedTypeArrayHkbCustomIdSelectorRefPtr;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_nestedTypeHkbGeneratorPtr = des.ReadClassPointer<hkbGenerator>(br);
            m_nestedTypeHkbGeneratorRefPtr = des.ReadClassPointer<hkbGenerator>(br);
            m_nestedTypeHkbModifierPtr = des.ReadClassPointer<hkbModifier>(br);
            m_nestedTypeHkbModifierRefPtr = des.ReadClassPointer<hkbModifier>(br);
            m_nestedTypeHkbCustomIdSelectorPtr = des.ReadClassPointer<hkbCustomIdSelector>(br);
            m_nestedTypeHkbCustomIdSelectorRefPtr = des.ReadClassPointer<hkbCustomIdSelector>(br);
            m_nestedTypeArrayBool = des.ReadBooleanArray(br);
            m_nestedTypeArrayHkBool = des.ReadBooleanArray(br);
            m_nestedTypeArrayCString = des.ReadStringPointerArray(br);
            m_nestedTypeArrayHkStringPtr = des.ReadStringPointerArray(br);
            m_nestedTypeArrayHkInt8 = des.ReadSByteArray(br);
            m_nestedTypeArrayHkInt16 = des.ReadInt16Array(br);
            m_nestedTypeArrayHkInt32 = des.ReadInt32Array(br);
            m_nestedTypeArrayHkUint8 = des.ReadByteArray(br);
            m_nestedTypeArrayHkUint16 = des.ReadUInt16Array(br);
            m_nestedTypeArrayHkUint32 = des.ReadUInt32Array(br);
            m_nestedTypeArrayHkReal = des.ReadSingleArray(br);
            m_nestedTypeArrayHkbGeneratorPtr = des.ReadClassPointerArray<hkbGenerator>(br);
            m_nestedTypeArrayHkbGeneratorRefPtr = des.ReadClassPointerArray<hkbGenerator>(br);
            m_nestedTypeArrayHkbModifierPtr = des.ReadClassPointerArray<hkbModifier>(br);
            m_nestedTypeArrayHkbModifierRefPtr = des.ReadClassPointerArray<hkbModifier>(br);
            m_nestedTypeArrayHkbCustomIdSelectorPtr = des.ReadClassPointerArray<hkbCustomIdSelector>(br);
            m_nestedTypeArrayHkbCustomIdSelectorRefPtr = des.ReadClassPointerArray<hkbCustomIdSelector>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbGenerator>(bw, m_nestedTypeHkbGeneratorPtr);
            s.WriteClassPointer<hkbGenerator>(bw, m_nestedTypeHkbGeneratorRefPtr);
            s.WriteClassPointer<hkbModifier>(bw, m_nestedTypeHkbModifierPtr);
            s.WriteClassPointer<hkbModifier>(bw, m_nestedTypeHkbModifierRefPtr);
            s.WriteClassPointer<hkbCustomIdSelector>(bw, m_nestedTypeHkbCustomIdSelectorPtr);
            s.WriteClassPointer<hkbCustomIdSelector>(bw, m_nestedTypeHkbCustomIdSelectorRefPtr);
            s.WriteBooleanArray(bw, m_nestedTypeArrayBool);
            s.WriteBooleanArray(bw, m_nestedTypeArrayHkBool);
            s.WriteStringPointerArray(bw, m_nestedTypeArrayCString);
            s.WriteStringPointerArray(bw, m_nestedTypeArrayHkStringPtr);
            s.WriteSByteArray(bw, m_nestedTypeArrayHkInt8);
            s.WriteInt16Array(bw, m_nestedTypeArrayHkInt16);
            s.WriteInt32Array(bw, m_nestedTypeArrayHkInt32);
            s.WriteByteArray(bw, m_nestedTypeArrayHkUint8);
            s.WriteUInt16Array(bw, m_nestedTypeArrayHkUint16);
            s.WriteUInt32Array(bw, m_nestedTypeArrayHkUint32);
            s.WriteSingleArray(bw, m_nestedTypeArrayHkReal);
            s.WriteClassPointerArray<hkbGenerator>(bw, m_nestedTypeArrayHkbGeneratorPtr);
            s.WriteClassPointerArray<hkbGenerator>(bw, m_nestedTypeArrayHkbGeneratorRefPtr);
            s.WriteClassPointerArray<hkbModifier>(bw, m_nestedTypeArrayHkbModifierPtr);
            s.WriteClassPointerArray<hkbModifier>(bw, m_nestedTypeArrayHkbModifierRefPtr);
            s.WriteClassPointerArray<hkbCustomIdSelector>(bw, m_nestedTypeArrayHkbCustomIdSelectorPtr);
            s.WriteClassPointerArray<hkbCustomIdSelector>(bw, m_nestedTypeArrayHkbCustomIdSelectorRefPtr);
        }
    }
}
