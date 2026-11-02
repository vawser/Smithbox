// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiStreamingSetData : HavokData<HKLib.hk2019.hkaiStreamingSet> 
{
    public hkaiStreamingSetData(HavokType type, HKLib.hk2019.hkaiStreamingSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
                if (value is not List<HKLib.hk2019.hkaiStreamingSet.NavMeshConnection> castValue) return false;
                instance.m_meshConnections = castValue;
                return true;
            }
            case "m_graphConnections":
            case "graphConnections":
            {
                if (value is not List<HKLib.hk2019.hkaiStreamingSet.GraphConnection> castValue) return false;
                instance.m_graphConnections = castValue;
                return true;
            }
            case "m_volumeConnections":
            case "volumeConnections":
            {
                if (value is not List<HKLib.hk2019.hkaiStreamingSet.VolumeConnection> castValue) return false;
                instance.m_volumeConnections = castValue;
                return true;
            }
            case "m_aConnectionAabbs":
            case "aConnectionAabbs":
            {
                if (value is not List<HKLib.hk2019.hkAabb> castValue) return false;
                instance.m_aConnectionAabbs = castValue;
                return true;
            }
            case "m_bConnectionAabbs":
            case "bConnectionAabbs":
            {
                if (value is not List<HKLib.hk2019.hkAabb> castValue) return false;
                instance.m_bConnectionAabbs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
