// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshModifiedFacesData : HavokData<HKLib.hk2019.hkaiNavMeshModifiedFaces> 
{
    public hkaiNavMeshModifiedFacesData(HavokType type, HKLib.hk2019.hkaiNavMeshModifiedFaces instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_modifiedFaceIndices":
            case "modifiedFaceIndices":
            {
                if (instance.m_modifiedFaceIndices is not TGet castValue) return false;
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
            case "m_modifiedFaceIndices":
            case "modifiedFaceIndices":
            {
                if (value is not List<HKLib.hk2019.hkHashSet<int>> castValue) return false;
                instance.m_modifiedFaceIndices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
