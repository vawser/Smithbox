// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkSkinnedMeshShapeBoneSectionData : HavokData<hkSkinnedMeshShape.BoneSection> 
{
    public hkSkinnedMeshShapeBoneSectionData(HavokType type, hkSkinnedMeshShape.BoneSection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_meshBuffer":
            case "meshBuffer":
            {
                if (instance.m_meshBuffer is null)
                {
                    return true;
                }
                if (instance.m_meshBuffer is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_startBoneSetId":
            case "startBoneSetId":
            {
                if (instance.m_startBoneSetId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numBoneSets":
            case "numBoneSets":
            {
                if (instance.m_numBoneSets is not TGet castValue) return false;
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
            case "m_meshBuffer":
            case "meshBuffer":
            {
                if (value is null)
                {
                    instance.m_meshBuffer = default;
                    return true;
                }
                if (value is hkMeshShape castValue)
                {
                    instance.m_meshBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_startBoneSetId":
            case "startBoneSetId":
            {
                if (value is not ushort castValue) return false;
                instance.m_startBoneSetId = castValue;
                return true;
            }
            case "m_numBoneSets":
            case "numBoneSets":
            {
                if (value is not short castValue) return false;
                instance.m_numBoneSets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
