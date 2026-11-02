// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationInputVolumeFaceData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.InputVolume.Face> 
{
    public hkaiNavMeshGenerationInputVolumeFaceData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.InputVolume.Face instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (instance.m_edgeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_plane":
            case "plane":
            {
                if (instance.m_plane is not TGet castValue) return false;
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
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_edgeIndex = castValue;
                return true;
            }
            case "m_plane":
            case "plane":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.ImplicitPlane castValue) return false;
                instance.m_plane = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
