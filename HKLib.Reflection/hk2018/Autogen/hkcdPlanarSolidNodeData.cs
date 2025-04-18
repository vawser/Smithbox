// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarSolidNodeData : HavokData<hkcdPlanarSolid.Node> 
{
    public hkcdPlanarSolidNodeData(HavokType type, hkcdPlanarSolid.Node instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_parent":
            case "parent":
            {
                if (instance.m_parent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_left":
            case "left":
            {
                if (instance.m_left is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_right":
            case "right":
            {
                if (instance.m_right is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextFreeNodeId":
            case "nextFreeNodeId":
            {
                if (instance.m_nextFreeNodeId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_planeId":
            case "planeId":
            {
                if (instance.m_planeId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabbId":
            case "aabbId":
            {
                if (instance.m_aabbId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (instance.m_material is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
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
                if (value is not hkcdPlanarGeometryPolygonCollection.Material castValue) return false;
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
