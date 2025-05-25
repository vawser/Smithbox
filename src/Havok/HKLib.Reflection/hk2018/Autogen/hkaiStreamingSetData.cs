// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiStreamingSetData : HavokData<hkaiStreamingSet> 
{
    public hkaiStreamingSetData(HavokType type, hkaiStreamingSet instance) : base(type, instance) {}

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
            case "m_aSectionUid":
            case "aSectionUid":
            {
                if (instance.m_aSectionUid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bSectionUid":
            case "bSectionUid":
            {
                if (instance.m_bSectionUid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_meshConnections":
            case "meshConnections":
            {
                if (instance.m_meshConnections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_graphConnections":
            case "graphConnections":
            {
                if (instance.m_graphConnections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_volumeConnections":
            case "volumeConnections":
            {
                if (instance.m_volumeConnections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aConnectionAabbs":
            case "aConnectionAabbs":
            {
                if (instance.m_aConnectionAabbs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bConnectionAabbs":
            case "bConnectionAabbs":
            {
                if (instance.m_bConnectionAabbs is not TGet castValue) return false;
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
            case "m_aSectionUid":
            case "aSectionUid":
            {
                if (value is not uint castValue) return false;
                instance.m_aSectionUid = castValue;
                return true;
            }
            case "m_bSectionUid":
            case "bSectionUid":
            {
                if (value is not uint castValue) return false;
                instance.m_bSectionUid = castValue;
                return true;
            }
            case "m_meshConnections":
            case "meshConnections":
            {
                if (value is not List<hkaiStreamingSet.NavMeshConnection> castValue) return false;
                instance.m_meshConnections = castValue;
                return true;
            }
            case "m_graphConnections":
            case "graphConnections":
            {
                if (value is not List<hkaiStreamingSet.GraphConnection> castValue) return false;
                instance.m_graphConnections = castValue;
                return true;
            }
            case "m_volumeConnections":
            case "volumeConnections":
            {
                if (value is not List<hkaiStreamingSet.VolumeConnection> castValue) return false;
                instance.m_volumeConnections = castValue;
                return true;
            }
            case "m_aConnectionAabbs":
            case "aConnectionAabbs":
            {
                if (value is not List<hkAabb> castValue) return false;
                instance.m_aConnectionAabbs = castValue;
                return true;
            }
            case "m_bConnectionAabbs":
            case "bConnectionAabbs":
            {
                if (value is not List<hkAabb> castValue) return false;
                instance.m_bConnectionAabbs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
