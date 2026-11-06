// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclSimulationSetupMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclSimulationSetupMesh>
{
    public hclSimulationSetupMeshData(Havok.hk2016.hclSimulationSetupMesh instance) : base(instance) { }

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
            case "m_originalMesh":
            case "originalMesh":
            {
                value = instance.m_originalMesh;
                return true;
            }
            case "m_mergeOptions":
            case "mergeOptions":
            {
                value = instance.m_mergeOptions;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_originalMesh":
            case "originalMesh":
            {
                if (value is null)
                {
                    instance.m_originalMesh = default;
                    return true;
                }
                if (value is Havok.hk2016.hclSetupMesh castValue)
                {
                    instance.m_originalMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_mergeOptions":
            case "mergeOptions":
            {
                if (value is not Havok.hk2016.hclSimulationSetupMeshMapOptions castValue) return false;
                instance.m_mergeOptions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
