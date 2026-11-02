// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshCutConfigurationDynamicUserEdgeData : HavokData<HKLib.hk2019.hkaiNavMeshCutConfiguration.DynamicUserEdge> 
{
    public hkaiNavMeshCutConfigurationDynamicUserEdgeData(HavokType type, HKLib.hk2019.hkaiNavMeshCutConfiguration.DynamicUserEdge instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_oppFace":
            case "oppFace":
            {
                if (instance.m_oppFace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_oppDynUserEdgeIdx":
            case "oppDynUserEdgeIdx":
            {
                if (instance.m_oppDynUserEdgeIdx is not TGet castValue) return false;
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
