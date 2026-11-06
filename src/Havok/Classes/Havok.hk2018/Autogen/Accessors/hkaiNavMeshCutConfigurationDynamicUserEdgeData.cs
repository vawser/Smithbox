// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshCutConfigurationDynamicUserEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshCutConfiguration.DynamicUserEdge>
{
    public hkaiNavMeshCutConfigurationDynamicUserEdgeData(Havok.hk2018.hkaiNavMeshCutConfiguration.DynamicUserEdge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_oppFace":
            case "oppFace":
            {
                value = instance.m_oppFace;
                return true;
            }
            case "m_oppDynUserEdgeIdx":
            case "oppDynUserEdgeIdx":
            {
                value = instance.m_oppDynUserEdgeIdx;
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
            case "m_oppFace":
            case "oppFace":
            {
                if (value is not int castValue) return false;
                instance.m_oppFace = castValue;
                return true;
            }
            case "m_oppDynUserEdgeIdx":
            case "oppDynUserEdgeIdx":
            {
                if (value is not int castValue) return false;
                instance.m_oppDynUserEdgeIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
