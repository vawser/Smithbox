// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiUserEdgeUtilsUserEdgePairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiUserEdgeUtils.UserEdgePair>
{
    public hkaiUserEdgeUtilsUserEdgePairData(Havok.hk2018.hkaiUserEdgeUtils.UserEdgePair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_x":
            case "x":
            {
                value = instance.m_x;
                return true;
            }
            case "m_y":
            case "y":
            {
                value = instance.m_y;
                return true;
            }
            case "m_z":
            case "z":
            {
                value = instance.m_z;
                return true;
            }
            case "m_instanceUidA":
            case "instanceUidA":
            {
                value = instance.m_instanceUidA;
                return true;
            }
            case "m_instanceUidB":
            case "instanceUidB":
            {
                value = instance.m_instanceUidB;
                return true;
            }
            case "m_faceA":
            case "faceA":
            {
                value = instance.m_faceA;
                return true;
            }
            case "m_faceB":
            case "faceB":
            {
                value = instance.m_faceB;
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
            case "m_direction":
            case "direction":
            {
                value = instance.m_direction;
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
                if (value is Havok.hk2018.hkaiUserEdgeUtils.UserEdgeDirection castValue)
                {
                    instance.m_direction = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_direction = (Havok.hk2018.hkaiUserEdgeUtils.UserEdgeDirection)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
