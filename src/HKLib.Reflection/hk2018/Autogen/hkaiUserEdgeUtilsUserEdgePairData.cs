// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiUserEdgeUtilsUserEdgePairData : HavokData<hkaiUserEdgeUtils.UserEdgePair> 
{
    public hkaiUserEdgeUtilsUserEdgePairData(HavokType type, hkaiUserEdgeUtils.UserEdgePair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_x":
            case "x":
            {
                if (instance.m_x is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (instance.m_y is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_z":
            case "z":
            {
                if (instance.m_z is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceUidA":
            case "instanceUidA":
            {
                if (instance.m_instanceUidA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceUidB":
            case "instanceUidB":
            {
                if (instance.m_instanceUidB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceA":
            case "faceA":
            {
                if (instance.m_faceA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_faceB":
            case "faceB":
            {
                if (instance.m_faceB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userDataA":
            case "userDataA":
            {
                if (instance.m_userDataA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userDataB":
            case "userDataB":
            {
                if (instance.m_userDataB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_costAtoB":
            case "costAtoB":
            {
                if (instance.m_costAtoB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_costBtoA":
            case "costBtoA":
            {
                if (instance.m_costBtoA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                if (instance.m_direction is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_direction is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_x":
            case "x":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_x = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_y = castValue;
                return true;
            }
            case "m_z":
            case "z":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_z = castValue;
                return true;
            }
            case "m_instanceUidA":
            case "instanceUidA":
            {
                if (value is not uint castValue) return false;
                instance.m_instanceUidA = castValue;
                return true;
            }
            case "m_instanceUidB":
            case "instanceUidB":
            {
                if (value is not uint castValue) return false;
                instance.m_instanceUidB = castValue;
                return true;
            }
            case "m_faceA":
            case "faceA":
            {
                if (value is not int castValue) return false;
                instance.m_faceA = castValue;
                return true;
            }
            case "m_faceB":
            case "faceB":
            {
                if (value is not int castValue) return false;
                instance.m_faceB = castValue;
                return true;
            }
            case "m_userDataA":
            case "userDataA":
            {
                if (value is not int castValue) return false;
                instance.m_userDataA = castValue;
                return true;
            }
            case "m_userDataB":
            case "userDataB":
            {
                if (value is not int castValue) return false;
                instance.m_userDataB = castValue;
                return true;
            }
            case "m_costAtoB":
            case "costAtoB":
            {
                if (value is not float castValue) return false;
                instance.m_costAtoB = castValue;
                return true;
            }
            case "m_costBtoA":
            case "costBtoA":
            {
                if (value is not float castValue) return false;
                instance.m_costBtoA = castValue;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                if (value is hkaiUserEdgeUtils.UserEdgeDirection castValue)
                {
                    instance.m_direction = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_direction = (hkaiUserEdgeUtils.UserEdgeDirection)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
