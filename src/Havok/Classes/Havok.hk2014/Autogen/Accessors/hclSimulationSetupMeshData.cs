// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSimulationSetupMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSimulationSetupMesh>
{
    public hclSimulationSetupMeshData(Havok.hk2014.hclSimulationSetupMesh instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_originalMesh":
            case "originalMesh":
            {
                if (value is null)
                {
                    instance.m_originalMesh = default;
                    return true;
                }
                if (value is Havok.hk2014.hclSetupMesh castValue)
                {
                    instance.m_originalMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_mergeOptions":
            case "mergeOptions":
            {
                if (value is not Havok.hk2014.hclSimulationSetupMeshMapOptions castValue) return false;
                instance.m_mergeOptions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
