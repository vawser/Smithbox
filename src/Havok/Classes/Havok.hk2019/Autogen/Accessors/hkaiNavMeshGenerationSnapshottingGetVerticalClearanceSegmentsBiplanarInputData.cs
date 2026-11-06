// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBiplanarInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarInput>
{
    public hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBiplanarInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryFaceKey":
            case "geometryFaceKey":
            {
                value = instance.m_geometryFaceKey;
                return true;
            }
            case "m_basis":
            case "basis":
            {
                value = instance.m_basis;
                return true;
            }
            case "m_testHeights":
            case "testHeights":
            {
                value = instance.m_testHeights;
                return true;
            }
            case "m_stepHeight":
            case "stepHeight":
            {
                value = instance.m_stepHeight;
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
            case "m_geometryFaceKey":
            case "geometryFaceKey":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceKey = castValue;
                return true;
            }
            case "m_basis":
            case "basis":
            {
                if (value is not Havok.hk2019.hkaiClipper.BiplanarInnerTypes.Basis castValue) return false;
                instance.m_basis = castValue;
                return true;
            }
            case "m_testHeights":
            case "testHeights":
            {
                if (value is not List<List<byte>> castValue) return false;
                instance.m_testHeights = castValue;
                return true;
            }
            case "m_stepHeight":
            case "stepHeight":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_stepHeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
