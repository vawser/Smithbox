// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiSplitGenerationSnapshotData : HavokData<HKLib.hk2019.hkaiSplitGenerationSnapshot> 
{
    public hkaiSplitGenerationSnapshotData(HavokType type, HKLib.hk2019.hkaiSplitGenerationSnapshot instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generationSnapshot":
            case "generationSnapshot":
            {
                if (instance.m_generationSnapshot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_splitSettings":
            case "splitSettings":
            {
                if (instance.m_splitSettings is not TGet castValue) return false;
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
            case "m_generationSnapshot":
            case "generationSnapshot":
            {
                if (value is not HKLib.hk2019.hkaiNavMeshGenerationSnapshot castValue) return false;
                instance.m_generationSnapshot = castValue;
                return true;
            }
            case "m_splitSettings":
            case "splitSettings":
            {
                if (value is not HKLib.hk2019.hkaiSplitGenerationUtils.Settings castValue) return false;
                instance.m_splitSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
