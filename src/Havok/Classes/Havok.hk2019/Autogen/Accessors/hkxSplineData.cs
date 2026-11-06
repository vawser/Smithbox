// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxSplineData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxSpline>
{
    public hkxSplineData(Havok.hk2019.hkxSpline instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_controlPoints":
            case "controlPoints":
            {
                value = instance.m_controlPoints;
                return true;
            }
            case "m_isClosed":
            case "isClosed":
            {
                value = instance.m_isClosed;
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
            case "m_controlPoints":
            case "controlPoints":
            {
                if (value is not List<Havok.hk2019.hkxSpline.ControlPoint> castValue) return false;
                instance.m_controlPoints = castValue;
                return true;
            }
            case "m_isClosed":
            case "isClosed":
            {
                if (value is not bool castValue) return false;
                instance.m_isClosed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
