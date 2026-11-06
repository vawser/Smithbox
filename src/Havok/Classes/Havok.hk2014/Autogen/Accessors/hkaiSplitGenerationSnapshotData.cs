// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSplitGenerationSnapshotData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSplitGenerationSnapshot>
{
    public hkaiSplitGenerationSnapshotData(Havok.hk2014.hkaiSplitGenerationSnapshot instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generationSnapshot":
            case "generationSnapshot":
            {
                value = instance.m_generationSnapshot;
                return true;
            }
            case "m_splitSettings":
            case "splitSettings":
            {
                value = instance.m_splitSettings;
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
            case "m_generationSnapshot":
            case "generationSnapshot":
            {
                if (value is not Havok.hk2014.hkaiNavMeshGenerationSnapshot castValue) return false;
                instance.m_generationSnapshot = castValue;
                return true;
            }
            case "m_splitSettings":
            case "splitSettings":
            {
                if (value is not Havok.hk2014.hkaiSplitGenerationUtils.Settings castValue) return false;
                instance.m_splitSettings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
