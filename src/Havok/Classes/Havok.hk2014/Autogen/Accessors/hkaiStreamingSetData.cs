// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiStreamingSetData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiStreamingSet>
{
    public hkaiStreamingSetData(Havok.hk2014.hkaiStreamingSet instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_thisUid":
            case "thisUid":
            {
                value = instance.m_thisUid;
                return true;
            }
            case "m_oppositeUid":
            case "oppositeUid":
            {
                value = instance.m_oppositeUid;
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
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_thisUid":
            case "thisUid":
            {
                if (value is not uint castValue) return false;
                instance.m_thisUid = castValue;
                return true;
            }
            case "m_oppositeUid":
            case "oppositeUid":
            {
                if (value is not uint castValue) return false;
                instance.m_oppositeUid = castValue;
                return true;
            }
            case "m_meshConnections":
            case "meshConnections":
            {
                if (value is not List<Havok.hk2014.hkaiStreamingSet.NavMeshConnection> castValue) return false;
                instance.m_meshConnections = castValue;
                return true;
            }
            case "m_graphConnections":
            case "graphConnections":
            {
                if (value is not List<Havok.hk2014.hkaiStreamingSet.GraphConnection> castValue) return false;
                instance.m_graphConnections = castValue;
                return true;
            }
            case "m_volumeConnections":
            case "volumeConnections":
            {
                if (value is not List<Havok.hk2014.hkaiStreamingSet.VolumeConnection> castValue) return false;
                instance.m_volumeConnections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
