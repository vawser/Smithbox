// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdPlanarSolidData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdPlanarSolid>
{
    public hkcdPlanarSolidData(Havok.hk2019.hkcdPlanarSolid instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_planes":
            case "planes":
            {
                value = instance.m_planes;
                return true;
            }
            case "m_rootNodeId":
            case "rootNodeId":
            {
                value = instance.m_rootNodeId;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is null)
                {
                    instance.m_nodes = default;
                    return true;
                }
                if (value is Havok.hk2019.hkcdPlanarSolid.NodeStorage castValue)
                {
                    instance.m_nodes = castValue;
                    return true;
                }
                return false;
            }
            case "m_planes":
            case "planes":
            {
                if (value is null)
                {
                    instance.m_planes = default;
                    return true;
                }
                if (value is Havok.hk2019.hkcdPlanarEntity.PlanesCollection castValue)
                {
                    instance.m_planes = castValue;
                    return true;
                }
                return false;
            }
            case "m_rootNodeId":
            case "rootNodeId":
            {
                if (value is not uint castValue) return false;
                instance.m_rootNodeId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
