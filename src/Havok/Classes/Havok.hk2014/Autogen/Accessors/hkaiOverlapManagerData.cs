// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiOverlapManagerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiOverlapManager>
{
    public hkaiOverlapManagerData(Havok.hk2014.hkaiOverlapManager instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_referenceFrameAndExtrusion":
            case "referenceFrameAndExtrusion":
            {
                value = instance.m_referenceFrameAndExtrusion;
                return true;
            }
            case "m_navMeshCollection":
            case "navMeshCollection":
            {
                value = instance.m_navMeshCollection;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
                return true;
            }
            case "m_stepCount":
            case "stepCount":
            {
                value = instance.m_stepCount;
                return true;
            }
            case "m_hasMovedTolerance":
            case "hasMovedTolerance":
            {
                value = instance.m_hasMovedTolerance;
                return true;
            }
            case "m_maxCutFacesPerStep":
            case "maxCutFacesPerStep":
            {
                value = instance.m_maxCutFacesPerStep;
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
            case "m_referenceFrameAndExtrusion":
            case "referenceFrameAndExtrusion":
            {
                if (value is not Havok.hk2014.hkaiReferenceFrameAndExtrusion castValue) return false;
                instance.m_referenceFrameAndExtrusion = castValue;
                return true;
            }
            case "m_navMeshCollection":
            case "navMeshCollection":
            {
                if (value is null)
                {
                    instance.m_navMeshCollection = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiStreamingCollection castValue)
                {
                    instance.m_navMeshCollection = castValue;
                    return true;
                }
                return false;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2014.hkaiOverlapManager.Section> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_stepCount":
            case "stepCount":
            {
                if (value is not int castValue) return false;
                instance.m_stepCount = castValue;
                return true;
            }
            case "m_hasMovedTolerance":
            case "hasMovedTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_hasMovedTolerance = castValue;
                return true;
            }
            case "m_maxCutFacesPerStep":
            case "maxCutFacesPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxCutFacesPerStep = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
