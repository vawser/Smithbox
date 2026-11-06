// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkTupleConstCharHkReflectVarHkReflectVarData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkTupleConstCharHkReflectVarHkReflectVar>
{
    public hkTupleConstCharHkReflectVarHkReflectVarData(Havok.hk2018.hkTupleConstCharHkReflectVarHkReflectVar instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_0":
            case "0":
            {
                value = instance.m_0;
                return true;
            }
            case "m_1":
            case "1":
            {
                value = instance.m_1;
                return true;
            }
            case "m_2":
            case "2":
            {
                value = instance.m_2;
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
            case "m_0":
            case "0":
            {
                if (value is null)
                {
                    instance.m_0 = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_0 = castValue;
                    return true;
                }
                return false;
            }
            case "m_1":
            case "1":
            {
                if (value is null)
                {
                    instance.m_1 = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_1 = castValue;
                    return true;
                }
                return false;
            }
            case "m_2":
            case "2":
            {
                if (value is null)
                {
                    instance.m_2 = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_2 = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
