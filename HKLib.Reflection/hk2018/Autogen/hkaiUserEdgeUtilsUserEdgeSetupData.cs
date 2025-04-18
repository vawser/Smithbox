// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiUserEdgeUtilsUserEdgeSetupData : HavokData<hkaiUserEdgeUtils.UserEdgeSetup> 
{
    public hkaiUserEdgeUtilsUserEdgeSetupData(HavokType type, hkaiUserEdgeUtils.UserEdgeSetup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_obbA":
            case "obbA":
            {
                if (instance.m_obbA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_obbB":
            case "obbB":
            {
                if (instance.m_obbB is not TGet castValue) return false;
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
            case "m_worldUpA":
            case "worldUpA":
            {
                if (instance.m_worldUpA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldUpB":
            case "worldUpB":
            {
                if (instance.m_worldUpB is not TGet castValue) return false;
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
            case "m_space":
            case "space":
            {
                if (instance.m_space is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_space is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_forceAlign":
            case "forceAlign":
            {
                if (instance.m_forceAlign is not TGet castValue) return false;
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
            case "m_obbA":
            case "obbA":
            {
                if (value is not hkaiUserEdgeUtils.Obb castValue) return false;
                instance.m_obbA = castValue;
                return true;
            }
            case "m_obbB":
            case "obbB":
            {
                if (value is not hkaiUserEdgeUtils.Obb castValue) return false;
                instance.m_obbB = castValue;
                return true;
            }
            case "m_userDataA":
            case "userDataA":
            {
                if (value is not uint castValue) return false;
                instance.m_userDataA = castValue;
                return true;
            }
            case "m_userDataB":
            case "userDataB":
            {
                if (value is not uint castValue) return false;
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
            case "m_worldUpA":
            case "worldUpA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_worldUpA = castValue;
                return true;
            }
            case "m_worldUpB":
            case "worldUpB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_worldUpB = castValue;
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
            case "m_space":
            case "space":
            {
                if (value is hkaiUserEdgeUtils.UserEdgeSetupSpace castValue)
                {
                    instance.m_space = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_space = (hkaiUserEdgeUtils.UserEdgeSetupSpace)byteValue;
                    return true;
                }
                return false;
            }
            case "m_forceAlign":
            case "forceAlign":
            {
                if (value is not bool castValue) return false;
                instance.m_forceAlign = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
