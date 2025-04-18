// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpParticleBodyContactData : HavokData<hknpParticleBodyContact> 
{
    public hknpParticleBodyContactData(HavokType type, hknpParticleBodyContact instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_contactNormal":
            case "contactNormal":
            {
                if (instance.m_contactNormal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_impulse":
            case "impulse":
            {
                if (instance.m_impulse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_body":
            case "body":
            {
                if (instance.m_body is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                if (instance.m_shapeKey is not TGet castValue) return false;
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
                if (value is not hknpBodyId castValue) return false;
                instance.m_body = castValue;
                return true;
            }
            case "m_shapeKey":
            case "shapeKey":
            {
                if (value is not hkHandle<uint> castValue) return false;
                instance.m_shapeKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
