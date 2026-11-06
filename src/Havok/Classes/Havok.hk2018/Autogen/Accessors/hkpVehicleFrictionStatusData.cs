// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpVehicleFrictionStatusData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpVehicleFrictionStatus>
{
    private static readonly System.Reflection.FieldInfo _axisInfo = typeof(Havok.hk2018.hkpVehicleFrictionStatus).GetField("m_axis")!;
    public hkpVehicleFrictionStatusData(Havok.hk2018.hkpVehicleFrictionStatus instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_axis":
            case "axis":
            {
                value = instance.m_axis;
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
            case "m_axis":
            case "axis":
            {
                if (value is not Havok.hk2018.hkpVehicleFrictionStatus.AxisStatus[] castValue || castValue.Length != 2) return false;
                try
                {
                    _axisInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
