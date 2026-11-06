// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkFpMathFpIntVec2Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkFpMath.FpIntVec2>
{
    public hkFpMathFpIntVec2Data(Havok.hk2019.hkFpMath.FpIntVec2 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_x":
            case "x":
            {
                value = instance.m_x;
                return true;
            }
            case "m_y":
            case "y":
            {
                value = instance.m_y;
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
            case "m_x":
            case "x":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_x = castValue;
                return true;
            }
            case "m_y":
            case "y":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_y = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
