// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class TestComplexExRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2018.TestComplexExRecord>
{
    public TestComplexExRecordData(Havok.hk2018.TestComplexExRecord instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cstring":
            case "cstring":
            {
                value = instance.m_cstring;
                return true;
            }
            case "m_string":
            case "string":
            {
                value = instance.m_string;
                return true;
            }
            case "m_unsupported_ptr":
            case "unsupported_ptr":
            {
                value = instance.m_unsupported_ptr;
                return true;
            }
            case "m_unsupported_array":
            case "unsupported_array":
            {
                value = instance.m_unsupported_array;
                return true;
            }
            case "m_record":
            case "record":
            {
                value = instance.m_record;
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
            case "m_cstring":
            case "cstring":
            {
                if (value is null)
                {
                    instance.m_cstring = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_cstring = castValue;
                    return true;
                }
                return false;
            }
            case "m_string":
            case "string":
            {
                if (value is null)
                {
                    instance.m_string = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_string = castValue;
                    return true;
                }
                return false;
            }
            case "m_unsupported_ptr":
            case "unsupported_ptr":
            {
                if (value is null)
                {
                    instance.m_unsupported_ptr = default;
                    return true;
                }
                if (value is Havok.hk2018.hkReferencedObject castValue)
                {
                    instance.m_unsupported_ptr = castValue;
                    return true;
                }
                return false;
            }
            case "m_unsupported_array":
            case "unsupported_array":
            {
                if (value is not List<bool> castValue) return false;
                instance.m_unsupported_array = castValue;
                return true;
            }
            case "m_record":
            case "record":
            {
                if (value is not Havok.hk2018.TestComplexRecord castValue) return false;
                instance.m_record = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
