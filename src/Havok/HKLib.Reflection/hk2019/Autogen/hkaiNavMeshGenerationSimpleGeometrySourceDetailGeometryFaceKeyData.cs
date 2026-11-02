// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSimpleGeometrySourceDetailGeometryFaceKeyData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryFaceKey> 
{
    public hkaiNavMeshGenerationSimpleGeometrySourceDetailGeometryFaceKeyData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.SimpleGeometrySourceDetail.GeometryFaceKey instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_index":
            case "index":
            {
                if (instance.m_index is not TGet castValue) return false;
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
            case "m_index":
            case "index":
            {
                if (value is not int castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
