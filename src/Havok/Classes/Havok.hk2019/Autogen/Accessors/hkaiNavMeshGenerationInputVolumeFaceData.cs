// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationInputVolumeFaceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.InputVolume.Face>
{
    public hkaiNavMeshGenerationInputVolumeFaceData(Havok.hk2019.hkaiNavMeshGeneration.InputVolume.Face instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgeIndex":
            case "edgeIndex":
            {
                value = instance.m_edgeIndex;
                return true;
            }
            case "m_plane":
            case "plane":
            {
                value = instance.m_plane;
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
                if (value is not Havok.hk2019.hkaiClipper.BiplanarInnerTypes.ImplicitPlane castValue) return false;
                instance.m_plane = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
