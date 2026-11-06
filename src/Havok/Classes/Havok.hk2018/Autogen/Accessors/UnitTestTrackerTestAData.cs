// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class UnitTestTrackerTestAData : Havok.Formats.HavokFieldAccess<Havok.hk2018.UnitTest.TrackerTestA>
{
    public UnitTestTrackerTestAData(Havok.hk2018.UnitTest.TrackerTestA instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ptr":
            case "ptr":
            {
                value = instance.m_ptr;
                return true;
            }
            case "m_ptrA":
            case "ptrA":
            {
                value = instance.m_ptrA;
                return true;
            }
            case "m_l":
            case "l":
            {
                value = instance.m_l;
                return true;
            }
            case "m_reflected":
            case "reflected":
            {
                value = instance.m_reflected;
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
            case "m_ptr":
            case "ptr":
            {
                if (value is null)
                {
                    instance.m_ptr = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_ptr = castValue;
                    return true;
                }
                return false;
            }
            case "m_ptrA":
            case "ptrA":
            {
                if (value is null)
                {
                    instance.m_ptrA = default;
                    return true;
                }
                if (value is Havok.hk2018.UnitTest.TrackerTestA castValue)
                {
                    instance.m_ptrA = castValue;
                    return true;
                }
                return false;
            }
            case "m_l":
            case "l":
            {
                if (value is not long castValue) return false;
                instance.m_l = castValue;
                return true;
            }
            case "m_reflected":
            case "reflected":
            {
                if (value is not Havok.hk2018.UnitTest.Reflected castValue) return false;
                instance.m_reflected = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
