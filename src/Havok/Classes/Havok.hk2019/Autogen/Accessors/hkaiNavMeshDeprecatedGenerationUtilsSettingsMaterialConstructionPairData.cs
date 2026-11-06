// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshDeprecatedGenerationUtilsSettingsMaterialConstructionPairData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.MaterialConstructionPair>
{
    public hkaiNavMeshDeprecatedGenerationUtilsSettingsMaterialConstructionPairData(Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.MaterialConstructionPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_materialIndex":
            case "materialIndex":
            {
                value = instance.m_materialIndex;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_materialIndex":
            case "materialIndex":
            {
                if (value is not int castValue) return false;
                instance.m_materialIndex = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.ConstructionFlagsBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.ConstructionFlagsBits)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
