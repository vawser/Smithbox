// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class TestComplexExRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2019.TestComplexExRecord>
{
    public TestComplexExRecordData(Havok.hk2019.TestComplexExRecord instance) : base(instance) { }

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
            case "m_vector4":
            case "vector4":
            {
                value = instance.m_vector4;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_quaternion":
            case "quaternion":
            {
                value = instance.m_quaternion;
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
                if (value is Havok.hk2019.hkReferencedObject castValue)
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
            case "m_vector4":
            case "vector4":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vector4 = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_quaternion":
            case "quaternion":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_quaternion = castValue;
                return true;
            }
            case "m_record":
            case "record":
            {
                if (value is not Havok.hk2019.TestComplexRecord castValue) return false;
                instance.m_record = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
