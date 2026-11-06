// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpParticleBodyContactData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpParticleBodyContact>
{
    public hknpParticleBodyContactData(Havok.hk2018.hknpParticleBodyContact instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_contactNormal":
            case "contactNormal":
            {
                value = instance.m_contactNormal;
                return true;
            }
            case "m_impulse":
            case "impulse":
            {
                value = instance.m_impulse;
                return true;
            }
            case "m_body":
            case "body":
            {
                value = instance.m_body;
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
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_contactNormal":
            case "contactNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_contactNormal = castValue;
                return true;
            }
            case "m_impulse":
            case "impulse":
            {
                if (value is not float castValue) return false;
                instance.m_impulse = castValue;
                return true;
            }
            case "m_body":
            case "body":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_body = castValue;
                return true;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                if (value is not Havok.hk2018.hkHandle<uint> castValue) return false;
                instance.m_shapeKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
