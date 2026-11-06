// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpDashpotActionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpDashpotAction>
{
    private static readonly System.Reflection.FieldInfo _pointInfo = typeof(Havok.hk2014.hkpDashpotAction).GetField("m_point")!;
    public hkpDashpotActionData(Havok.hk2014.hkpDashpotAction instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_entityA":
            case "entityA":
            {
                value = instance.m_entityA;
                return true;
            }
            case "m_entityB":
            case "entityB":
            {
                value = instance.m_entityB;
                return true;
            }
            case "m_point":
            case "point":
            {
                value = instance.m_point;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                value = instance.m_strength;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                value = instance.m_damping;
                return true;
            }
            case "m_impulse":
            case "impulse":
            {
                value = instance.m_impulse;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_entityA":
            case "entityA":
            {
                if (value is null)
                {
                    instance.m_entityA = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_entityA = castValue;
                    return true;
                }
                return false;
            }
            case "m_entityB":
            case "entityB":
            {
                if (value is null)
                {
                    instance.m_entityB = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_entityB = castValue;
                    return true;
                }
                return false;
            }
            case "m_point":
            case "point":
            {
                if (value is not Vector4[] castValue || castValue.Length != 2) return false;
                try
                {
                    _pointInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_strength":
            case "strength":
            {
                if (value is not float castValue) return false;
                instance.m_strength = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (value is not float castValue) return false;
                instance.m_damping = castValue;
                return true;
            }
            case "m_impulse":
            case "impulse":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_impulse = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
