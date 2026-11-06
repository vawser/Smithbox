// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdPlanarSolidNodeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdPlanarSolid.Node>
{
    public hkcdPlanarSolidNodeData(Havok.hk2014.hkcdPlanarSolid.Node instance) : base(instance) { }

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
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_typeAndFlags":
            case "typeAndFlags":
            {
                value = instance.m_typeAndFlags;
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
            case "m_data":
            case "data":
            {
                if (value is not uint castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            case "m_typeAndFlags":
            case "typeAndFlags":
            {
                if (value is not uint castValue) return false;
                instance.m_typeAndFlags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
