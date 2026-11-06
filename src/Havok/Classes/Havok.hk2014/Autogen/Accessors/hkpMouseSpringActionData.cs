// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpMouseSpringActionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpMouseSpringAction>
{
    public hkpMouseSpringActionData(Havok.hk2014.hkpMouseSpringAction instance) : base(instance) { }

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
            case "m_positionInRbLocal":
            case "positionInRbLocal":
            {
                value = instance.m_positionInRbLocal;
                return true;
            }
            case "m_mousePositionInWorld":
            case "mousePositionInWorld":
            {
                value = instance.m_mousePositionInWorld;
                return true;
            }
            case "m_springDamping":
            case "springDamping":
            {
                value = instance.m_springDamping;
                return true;
            }
            case "m_springElasticity":
            case "springElasticity":
            {
                value = instance.m_springElasticity;
                return true;
            }
            case "m_maxRelativeForce":
            case "maxRelativeForce":
            {
                value = instance.m_maxRelativeForce;
                return true;
            }
            case "m_objectDamping":
            case "objectDamping":
            {
                value = instance.m_objectDamping;
                return true;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                value = instance.m_shapeKey;
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
            case "m_positionInRbLocal":
            case "positionInRbLocal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_positionInRbLocal = castValue;
                return true;
            }
            case "m_mousePositionInWorld":
            case "mousePositionInWorld":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_mousePositionInWorld = castValue;
                return true;
            }
            case "m_springDamping":
            case "springDamping":
            {
                if (value is not float castValue) return false;
                instance.m_springDamping = castValue;
                return true;
            }
            case "m_springElasticity":
            case "springElasticity":
            {
                if (value is not float castValue) return false;
                instance.m_springElasticity = castValue;
                return true;
            }
            case "m_maxRelativeForce":
            case "maxRelativeForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxRelativeForce = castValue;
                return true;
            }
            case "m_objectDamping":
            case "objectDamping":
            {
                if (value is not float castValue) return false;
                instance.m_objectDamping = castValue;
                return true;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                if (value is not uint castValue) return false;
                instance.m_shapeKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
