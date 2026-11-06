// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSimpleWindActionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSimpleWindAction>
{
    public hclSimpleWindActionData(Havok.hk2014.hclSimpleWindAction instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_windDirection":
            case "windDirection":
            {
                value = instance.m_windDirection;
                return true;
            }
            case "m_windMinSpeed":
            case "windMinSpeed":
            {
                value = instance.m_windMinSpeed;
                return true;
            }
            case "m_windMaxSpeed":
            case "windMaxSpeed":
            {
                value = instance.m_windMaxSpeed;
                return true;
            }
            case "m_windFrequency":
            case "windFrequency":
            {
                value = instance.m_windFrequency;
                return true;
            }
            case "m_maximumDrag":
            case "maximumDrag":
            {
                value = instance.m_maximumDrag;
                return true;
            }
            case "m_airVelocity":
            case "airVelocity":
            {
                value = instance.m_airVelocity;
                return true;
            }
            case "m_currentTime":
            case "currentTime":
            {
                value = instance.m_currentTime;
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
            case "m_windDirection":
            case "windDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_windDirection = castValue;
                return true;
            }
            case "m_windMinSpeed":
            case "windMinSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_windMinSpeed = castValue;
                return true;
            }
            case "m_windMaxSpeed":
            case "windMaxSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_windMaxSpeed = castValue;
                return true;
            }
            case "m_windFrequency":
            case "windFrequency":
            {
                if (value is not float castValue) return false;
                instance.m_windFrequency = castValue;
                return true;
            }
            case "m_maximumDrag":
            case "maximumDrag":
            {
                if (value is not float castValue) return false;
                instance.m_maximumDrag = castValue;
                return true;
            }
            case "m_airVelocity":
            case "airVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_airVelocity = castValue;
                return true;
            }
            case "m_currentTime":
            case "currentTime":
            {
                if (value is not float castValue) return false;
                instance.m_currentTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
