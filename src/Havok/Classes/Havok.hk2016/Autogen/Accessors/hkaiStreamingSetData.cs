// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaiStreamingSetData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaiStreamingSet>
{
    public hkaiStreamingSetData(Havok.hk2016.hkaiStreamingSet instance) : base(instance) { }

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
            case "m_aSectionUid":
            case "aSectionUid":
            {
                value = instance.m_aSectionUid;
                return true;
            }
            case "m_bSectionUid":
            case "bSectionUid":
            {
                value = instance.m_bSectionUid;
                return true;
            }
            case "m_meshConnections":
            case "meshConnections":
            {
                value = instance.m_meshConnections;
                return true;
            }
            case "m_graphConnections":
            case "graphConnections":
            {
                value = instance.m_graphConnections;
                return true;
            }
            case "m_volumeConnections":
            case "volumeConnections":
            {
                value = instance.m_volumeConnections;
                return true;
            }
            case "m_aConnectionAabbs":
            case "aConnectionAabbs":
            {
                value = instance.m_aConnectionAabbs;
                return true;
            }
            case "m_bConnectionAabbs":
            case "bConnectionAabbs":
            {
                value = instance.m_bConnectionAabbs;
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
                if (value is not List<Havok.hk2016.hkaiStreamingSet.NavMeshConnection> castValue) return false;
                instance.m_meshConnections = castValue;
                return true;
            }
            case "m_graphConnections":
            case "graphConnections":
            {
                if (value is not List<Havok.hk2016.hkaiStreamingSet.GraphConnection> castValue) return false;
                instance.m_graphConnections = castValue;
                return true;
            }
            case "m_volumeConnections":
            case "volumeConnections":
            {
                if (value is not List<Havok.hk2016.hkaiStreamingSet.VolumeConnection> castValue) return false;
                instance.m_volumeConnections = castValue;
                return true;
            }
            case "m_aConnectionAabbs":
            case "aConnectionAabbs":
            {
                if (value is not List<Havok.hk2016.hkAabb> castValue) return false;
                instance.m_aConnectionAabbs = castValue;
                return true;
            }
            case "m_bConnectionAabbs":
            case "bConnectionAabbs":
            {
                if (value is not List<Havok.hk2016.hkAabb> castValue) return false;
                instance.m_bConnectionAabbs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
