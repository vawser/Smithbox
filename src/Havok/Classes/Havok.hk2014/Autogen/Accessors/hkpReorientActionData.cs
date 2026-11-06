// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpReorientActionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpReorientAction>
{
    public hkpReorientActionData(Havok.hk2014.hkpReorientAction instance) : base(instance) { }

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
            case "m_entity":
            case "entity":
            {
                value = instance.m_entity;
                return true;
            }
            case "m_rotationAxis":
            case "rotationAxis":
            {
                value = instance.m_rotationAxis;
                return true;
            }
            case "m_upAxis":
            case "upAxis":
            {
                value = instance.m_upAxis;
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
            case "m_entity":
            case "entity":
            {
                if (value is null)
                {
                    instance.m_entity = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_entity = castValue;
                    return true;
                }
                return false;
            }
            case "m_rotationAxis":
            case "rotationAxis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_rotationAxis = castValue;
                return true;
            }
            case "m_upAxis":
            case "upAxis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_upAxis = castValue;
                return true;
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
            default:
            return false;
        }
    }

}
