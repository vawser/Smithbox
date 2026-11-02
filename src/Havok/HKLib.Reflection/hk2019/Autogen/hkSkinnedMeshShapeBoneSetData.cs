// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkSkinnedMeshShapeBoneSetData : HavokData<HKLib.hk2019.hkSkinnedMeshShape.BoneSet> 
{
    public hkSkinnedMeshShapeBoneSetData(HavokType type, HKLib.hk2019.hkSkinnedMeshShape.BoneSet instance) : base(type, instance) {}

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
