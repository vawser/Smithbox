// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class UnitTestTypeVmRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2019.UnitTest.TypeVm.Record>
{
    private static readonly System.Reflection.FieldInfo _floatsInfo = typeof(Havok.hk2019.UnitTest.TypeVm.Record).GetField("m_floats")!;
    public UnitTestTypeVmRecordData(Havok.hk2019.UnitTest.TypeVm.Record instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_int":
            case "int":
            {
                value = instance.m_int;
                return true;
            }
            case "m_floats":
            case "floats":
            {
                value = instance.m_floats;
                return true;
            }
            case "m_embedded":
            case "embedded":
            {
                value = instance.m_embedded;
                return true;
            }
            case "m_array":
            case "array":
            {
                value = instance.m_array;
                return true;
            }
            case "m_string":
            case "string":
            {
                value = instance.m_string;
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
            case "m_int":
            case "int":
            {
                if (value is not int castValue) return false;
                instance.m_int = castValue;
                return true;
            }
            case "m_floats":
            case "floats":
            {
                if (value is not float[] castValue || castValue.Length != 2) return false;
                try
                {
                    _floatsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_embedded":
            case "embedded":
            {
                if (value is not Havok.hk2019.UnitTest.TypeVm.Bar castValue) return false;
                instance.m_embedded = castValue;
                return true;
            }
            case "m_array":
            case "array":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_array = castValue;
                return true;
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
            default:
            return false;
        }
    }

}
