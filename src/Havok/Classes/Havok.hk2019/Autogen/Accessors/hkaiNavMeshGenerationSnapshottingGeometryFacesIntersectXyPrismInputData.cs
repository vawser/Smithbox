// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGeometryFacesIntersectXyPrismInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput>
{
    public hkaiNavMeshGenerationSnapshottingGeometryFacesIntersectXyPrismInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_prism":
            case "prism":
            {
                value = instance.m_prism;
                return true;
            }
            case "m_prismAabb":
            case "prismAabb":
            {
                value = instance.m_prismAabb;
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
            case "m_prism":
            case "prism":
            {
                if (value is not Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GeometryFacesIntersectXyPrismInput.XyPrism castValue) return false;
                instance.m_prism = castValue;
                return true;
            }
            case "m_prismAabb":
            case "prismAabb":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_prismAabb = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
