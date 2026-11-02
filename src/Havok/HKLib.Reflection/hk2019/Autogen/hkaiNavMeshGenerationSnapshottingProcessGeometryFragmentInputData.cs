// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentInput> 
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometryFaceKey":
            case "geometryFaceKey":
            {
                if (instance.m_geometryFaceKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fragmentIndex":
            case "fragmentIndex":
            {
                if (instance.m_fragmentIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometryFaceData":
            case "geometryFaceData":
            {
                if (instance.m_geometryFaceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_verticalClearanceTestData":
            case "verticalClearanceTestData":
            {
                if (instance.m_verticalClearanceTestData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geometryVolumeDatas":
            case "geometryVolumeDatas":
            {
                if (instance.m_geometryVolumeDatas is not TGet castValue) return false;
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
            case "m_geometryFaceKey":
            case "geometryFaceKey":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
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
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_geometryFaceData = castValue;
                return true;
            }
            case "m_verticalClearanceTestData":
            case "verticalClearanceTestData":
            {
                if (value is not HKLib.hk2019.hkHandle<uint> castValue) return false;
                instance.m_verticalClearanceTestData = castValue;
                return true;
            }
            case "m_geometryVolumeDatas":
            case "geometryVolumeDatas":
            {
                if (value is not List<HKLib.hk2019.hkHandle<uint>> castValue) return false;
                instance.m_geometryVolumeDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
