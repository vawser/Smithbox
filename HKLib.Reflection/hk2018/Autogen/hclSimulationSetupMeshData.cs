// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimulationSetupMeshData : HavokData<hclSimulationSetupMesh> 
{
    public hclSimulationSetupMeshData(HavokType type, hclSimulationSetupMesh instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_originalMesh":
            case "originalMesh":
            {
                if (instance.m_originalMesh is null)
                {
                    return true;
                }
                if (instance.m_originalMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_mergeOptions":
            case "mergeOptions":
            {
                if (instance.m_mergeOptions is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is hclSetupMesh castValue)
                {
                    instance.m_originalMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_mergeOptions":
            case "mergeOptions":
            {
                if (value is not hclSimulationSetupMeshMapOptions castValue) return false;
                instance.m_mergeOptions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
