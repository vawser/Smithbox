// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshot_2Data : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshGeneration.Snapshot>
{
    public hkaiNavMeshGenerationSnapshot_2Data(Havok.hk2018.hkaiNavMeshGeneration.Snapshot instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_geometrySource":
            case "geometrySource":
            {
                value = instance.m_geometrySource;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                value = instance.m_settings;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_geometrySource":
            case "geometrySource":
            {
                if (value is null)
                {
                    instance.m_geometrySource = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiNavMeshGeneration.GeometrySource castValue)
                {
                    instance.m_geometrySource = castValue;
                    return true;
                }
                return false;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not Havok.hk2018.hkaiNavMeshGeneration.Settings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
