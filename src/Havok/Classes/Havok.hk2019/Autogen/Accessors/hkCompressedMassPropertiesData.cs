// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkCompressedMassPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkCompressedMassProperties>
{
    private static readonly System.Reflection.FieldInfo _majorAxisSpaceInfo = typeof(Havok.hk2019.hkCompressedMassProperties).GetField("m_majorAxisSpace")!;
    public hkCompressedMassPropertiesData(Havok.hk2019.hkCompressedMassProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_centerOfMass":
            case "centerOfMass":
            {
                value = instance.m_centerOfMass;
                return true;
            }
            case "m_inertia":
            case "inertia":
            {
                value = instance.m_inertia;
                return true;
            }
            case "m_majorAxisSpace":
            case "majorAxisSpace":
            {
                value = instance.m_majorAxisSpace;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_volume":
            case "volume":
            {
                value = instance.m_volume;
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
            case "m_centerOfMass":
            case "centerOfMass":
            {
                if (value is not Havok.hk2019.hkPackedVector3 castValue) return false;
                instance.m_centerOfMass = castValue;
                return true;
            }
            case "m_inertia":
            case "inertia":
            {
                if (value is not Havok.hk2019.hkPackedVector3 castValue) return false;
                instance.m_inertia = castValue;
                return true;
            }
            case "m_majorAxisSpace":
            case "majorAxisSpace":
            {
                if (value is not short[] castValue || castValue.Length != 4) return false;
                try
                {
                    _majorAxisSpaceInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_volume":
            case "volume":
            {
                if (value is not float castValue) return false;
                instance.m_volume = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
