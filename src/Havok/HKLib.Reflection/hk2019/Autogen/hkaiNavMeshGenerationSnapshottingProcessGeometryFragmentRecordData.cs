// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentRecordData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentRecord> 
{
    public hkaiNavMeshGenerationSnapshottingProcessGeometryFragmentRecordData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentRecord instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_input":
            case "input":
            {
                if (instance.m_input is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_output":
            case "output":
            {
                if (instance.m_output is not TGet castValue) return false;
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
            case "m_input":
            case "input":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentInput castValue) return false;
                instance.m_input = castValue;
                return true;
            }
            case "m_output":
            case "output":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshGeneration.Snapshotting.ProcessGeometryFragmentOutput castValue) return false;
                instance.m_output = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
