// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSkinnedMeshShapeBoneSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkSkinnedMeshShape.BoneSection>
{
    public hkSkinnedMeshShapeBoneSectionData(Havok.hk2019.hkSkinnedMeshShape.BoneSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_meshBuffer":
            case "meshBuffer":
            {
                value = instance.m_meshBuffer;
                return true;
            }
            case "m_startBoneSetId":
            case "startBoneSetId":
            {
                value = instance.m_startBoneSetId;
                return true;
            }
            case "m_numBoneSets":
            case "numBoneSets":
            {
                value = instance.m_numBoneSets;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
                if (value is Havok.hk2019.hkMeshShape castValue)
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
