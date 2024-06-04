// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkSkinnedMeshShapeBoneSetData : HavokData<hkSkinnedMeshShape.BoneSet> 
{
    public hkSkinnedMeshShapeBoneSetData(HavokType type, hkSkinnedMeshShape.BoneSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneBufferOffset":
            case "boneBufferOffset":
            {
                if (instance.m_boneBufferOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                if (instance.m_numBones is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_boneBufferOffset":
            case "boneBufferOffset":
            {
                if (value is not ushort castValue) return false;
                instance.m_boneBufferOffset = castValue;
                return true;
            }
            case "m_numBones":
            case "numBones":
            {
                if (value is not ushort castValue) return false;
                instance.m_numBones = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
