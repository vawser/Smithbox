// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class TestComplexExRecordData : HavokData<HKLib.hk2019.TestComplexExRecord> 
{
    public TestComplexExRecordData(HavokType type, HKLib.hk2019.TestComplexExRecord instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cstring":
            case "cstring":
            {
                if (instance.m_cstring is null)
                {
                    return true;
                }
                if (instance.m_cstring is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_string":
            case "string":
            {
                if (instance.m_string is null)
                {
                    return true;
                }
                if (instance.m_string is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_unsupported_ptr":
            case "unsupported_ptr":
            {
                if (instance.m_unsupported_ptr is null)
                {
                    return true;
                }
                if (instance.m_unsupported_ptr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_unsupported_array":
            case "unsupported_array":
            {
                if (instance.m_unsupported_array is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vector4":
            case "vector4":
            {
                if (instance.m_vector4 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_quaternion":
            case "quaternion":
            {
                if (instance.m_quaternion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_record":
            case "record":
            {
                if (instance.m_record is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkReferencedObject castValue)
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
                if (value is not HKLib.hk2019.TestComplexRecord castValue) return false;
                instance.m_record = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
