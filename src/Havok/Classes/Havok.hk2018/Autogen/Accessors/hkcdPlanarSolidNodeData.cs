// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdPlanarSolidNodeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdPlanarSolid.Node>
{
    public hkcdPlanarSolidNodeData(Havok.hk2018.hkcdPlanarSolid.Node instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_parent":
            case "parent":
            {
                value = instance.m_parent;
                return true;
            }
            case "m_left":
            case "left":
            {
                value = instance.m_left;
                return true;
            }
            case "m_right":
            case "right":
            {
                value = instance.m_right;
                return true;
            }
            case "m_nextFreeNodeId":
            case "nextFreeNodeId":
            {
                value = instance.m_nextFreeNodeId;
                return true;
            }
            case "m_planeId":
            case "planeId":
            {
                value = instance.m_planeId;
                return true;
            }
            case "m_aabbId":
            case "aabbId":
            {
                value = instance.m_aabbId;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_parent":
            case "parent":
            {
                if (value is not uint castValue) return false;
                instance.m_parent = castValue;
                return true;
            }
            case "m_left":
            case "left":
            {
                if (value is not uint castValue) return false;
                instance.m_left = castValue;
                return true;
            }
            case "m_right":
            case "right":
            {
                if (value is not uint castValue) return false;
                instance.m_right = castValue;
                return true;
            }
            case "m_nextFreeNodeId":
            case "nextFreeNodeId":
            {
                if (value is not uint castValue) return false;
                instance.m_nextFreeNodeId = castValue;
                return true;
            }
            case "m_planeId":
            case "planeId":
            {
                if (value is not uint castValue) return false;
                instance.m_planeId = castValue;
                return true;
            }
            case "m_aabbId":
            case "aabbId":
            {
                if (value is not uint castValue) return false;
                instance.m_aabbId = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (value is not Havok.hk2018.hkcdPlanarGeometryPolygonCollection.Material castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not uint castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is not ushort castValue) return false;
                instance.m_type = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not ushort castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
