// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentInput>
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentInput instance) : base(instance) { }

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
            case "m_fragmentIndex":
            case "fragmentIndex":
            {
                value = instance.m_fragmentIndex;
                return true;
            }
            case "m_geometryFaceData":
            case "geometryFaceData":
            {
                value = instance.m_geometryFaceData;
                return true;
            }
            case "m_verticalClearanceTestData":
            case "verticalClearanceTestData":
            {
                value = instance.m_verticalClearanceTestData;
                return true;
            }
            case "m_geometryVolumeDatas":
            case "geometryVolumeDatas":
            {
                value = instance.m_geometryVolumeDatas;
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
            case "m_fragmentIndex":
            case "fragmentIndex":
            {
                if (value is not int castValue) return false;
                instance.m_fragmentIndex = castValue;
                return true;
            }
            case "m_geometryFaceData":
            case "geometryFaceData":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceData = castValue;
                return true;
            }
            case "m_verticalClearanceTestData":
            case "verticalClearanceTestData":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_verticalClearanceTestData = castValue;
                return true;
            }
            case "m_geometryVolumeDatas":
            case "geometryVolumeDatas":
            {
                if (value is not List<Havok.hk2019.hkHandle<uint>> castValue) return false;
                instance.m_geometryVolumeDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
