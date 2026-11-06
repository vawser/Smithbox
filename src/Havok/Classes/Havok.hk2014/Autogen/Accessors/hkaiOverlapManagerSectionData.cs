// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiOverlapManagerSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiOverlapManager.Section>
{
    public hkaiOverlapManagerSectionData(Havok.hk2014.hkaiOverlapManager.Section instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numOriginalFaces":
            case "numOriginalFaces":
            {
                value = instance.m_numOriginalFaces;
                return true;
            }
            case "m_generatorData":
            case "generatorData":
            {
                value = instance.m_generatorData;
                return true;
            }
            case "m_facePriorities":
            case "facePriorities":
            {
                value = instance.m_facePriorities;
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
            case "m_numOriginalFaces":
            case "numOriginalFaces":
            {
                if (value is not int castValue) return false;
                instance.m_numOriginalFaces = castValue;
                return true;
            }
            case "m_generatorData":
            case "generatorData":
            {
                if (value is not List<Havok.hk2014.hkaiOverlapManagerSectionGeneratorData?> castValue) return false;
                instance.m_generatorData = castValue;
                return true;
            }
            case "m_facePriorities":
            case "facePriorities":
            {
                if (value is not Havok.hk2014.hkSetIntFloatPair castValue) return false;
                instance.m_facePriorities = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
