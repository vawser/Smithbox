// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBiplanarInputData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarInput> 
{
    public hkaiNavMeshGenerationSnapshottingGetVerticalClearanceSegmentsBiplanarInputData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.GetVerticalClearanceSegmentsBiplanarInput instance) : base(type, instance) {}

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
            case "m_basis":
            case "basis":
            {
                if (instance.m_basis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_testHeights":
            case "testHeights":
            {
                if (instance.m_testHeights is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stepHeight":
            case "stepHeight":
            {
                if (instance.m_stepHeight is not TGet castValue) return false;
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
            case "m_basis":
            case "basis":
            {
                if (value is not HKLib.hk2019.hkaiClipper.BiplanarInnerTypes.Basis castValue) return false;
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
