// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaiUserEdgeUtilsUserEdgeSetupData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaiUserEdgeUtils.UserEdgeSetup>
{
    public hkaiUserEdgeUtilsUserEdgeSetupData(Havok.hk2016.hkaiUserEdgeUtils.UserEdgeSetup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_obbA":
            case "obbA":
            {
                value = instance.m_obbA;
                return true;
            }
            case "m_obbB":
            case "obbB":
            {
                value = instance.m_obbB;
                return true;
            }
            case "m_userDataA":
            case "userDataA":
            {
                value = instance.m_userDataA;
                return true;
            }
            case "m_userDataB":
            case "userDataB":
            {
                value = instance.m_userDataB;
                return true;
            }
            case "m_costAtoB":
            case "costAtoB":
            {
                value = instance.m_costAtoB;
                return true;
            }
            case "m_costBtoA":
            case "costBtoA":
            {
                value = instance.m_costBtoA;
                return true;
            }
            case "m_worldUpA":
            case "worldUpA":
            {
                value = instance.m_worldUpA;
                return true;
            }
            case "m_worldUpB":
            case "worldUpB":
            {
                value = instance.m_worldUpB;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                value = instance.m_direction;
                return true;
            }
            case "m_space":
            case "space":
            {
                value = instance.m_space;
                return true;
            }
            case "m_forceAlign":
            case "forceAlign":
            {
                value = instance.m_forceAlign;
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
            case "m_obbA":
            case "obbA":
            {
                if (value is not Havok.hk2016.hkaiUserEdgeUtils.Obb castValue) return false;
                instance.m_obbA = castValue;
                return true;
            }
            case "m_obbB":
            case "obbB":
            {
                if (value is not Havok.hk2016.hkaiUserEdgeUtils.Obb castValue) return false;
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
                if (value is Havok.hk2016.hkaiUserEdgeUtils.UserEdgeDirection castValue)
                {
                    instance.m_direction = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_direction = (Havok.hk2016.hkaiUserEdgeUtils.UserEdgeDirection)byteValue;
                    return true;
                }
                return false;
            }
            case "m_space":
            case "space":
            {
                if (value is Havok.hk2016.hkaiUserEdgeUtils.UserEdgeSetupSpace castValue)
                {
                    instance.m_space = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_space = (Havok.hk2016.hkaiUserEdgeUtils.UserEdgeSetupSpace)byteValue;
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
