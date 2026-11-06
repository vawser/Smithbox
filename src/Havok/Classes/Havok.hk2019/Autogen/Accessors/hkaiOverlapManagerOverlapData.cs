// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiOverlapManagerOverlapData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiOverlapManager.Overlap>
{
    public hkaiOverlapManagerOverlapData(Havok.hk2019.hkaiOverlapManager.Overlap instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generator":
            case "generator":
            {
                value = instance.m_generator;
                return true;
            }
            case "m_section":
            case "section":
            {
                value = instance.m_section;
                return true;
            }
            case "m_generating":
            case "generating":
            {
                value = instance.m_generating;
                return true;
            }
            case "m_queryAabb":
            case "queryAabb":
            {
                value = instance.m_queryAabb;
                return true;
            }
            case "m_context":
            case "context":
            {
                value = instance.m_context;
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
            case "m_generator":
            case "generator":
            {
                if (value is not int castValue) return false;
                instance.m_generator = castValue;
                return true;
            }
            case "m_section":
            case "section":
            {
                if (value is not int castValue) return false;
                instance.m_section = castValue;
                return true;
            }
            case "m_generating":
            case "generating":
            {
                if (value is not bool castValue) return false;
                instance.m_generating = castValue;
                return true;
            }
            case "m_queryAabb":
            case "queryAabb":
            {
                if (value is not Havok.hk2019.hkAabb castValue) return false;
                instance.m_queryAabb = castValue;
                return true;
            }
            case "m_context":
            case "context":
            {
                if (value is not Havok.hk2019.hkaiSilhouetteGeneratorSectionContext castValue) return false;
                instance.m_context = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
