// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkPlaneData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkPlane>
{
    public hkPlaneData(Havok.hk2019.hkPlane instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_equation":
            case "equation":
            {
                value = instance.m_equation;
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
            case "m_equation":
            case "equation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_equation = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
