// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpControlledReachModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpControlledReachModifier>
{
    private static readonly System.Reflection.FieldInfo _handIndexInfo = typeof(Havok.hk2014.hkbpControlledReachModifier).GetField("m_handIndex")!;
    private static readonly System.Reflection.FieldInfo _isHandEnabledInfo = typeof(Havok.hk2014.hkbpControlledReachModifier).GetField("m_isHandEnabled")!;
    public hkbpControlledReachModifierData(Havok.hk2014.hkbpControlledReachModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_fadeInStart":
            case "fadeInStart":
            {
                value = instance.m_fadeInStart;
                return true;
            }
            case "m_fadeInEnd":
            case "fadeInEnd":
            {
                value = instance.m_fadeInEnd;
                return true;
            }
            case "m_fadeOutStart":
            case "fadeOutStart":
            {
                value = instance.m_fadeOutStart;
                return true;
            }
            case "m_fadeOutEnd":
            case "fadeOutEnd":
            {
                value = instance.m_fadeOutEnd;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                value = instance.m_fadeOutDuration;
                return true;
            }
            case "m_sensorAngle":
            case "sensorAngle":
            {
                value = instance.m_sensorAngle;
                return true;
            }
            case "m_handIndex":
            case "handIndex":
            {
                value = instance.m_handIndex;
                return true;
            }
            case "m_isHandEnabled":
            case "isHandEnabled":
            {
                value = instance.m_isHandEnabled;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_fadeInStart":
            case "fadeInStart":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInStart = castValue;
                return true;
            }
            case "m_fadeInEnd":
            case "fadeInEnd":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInEnd = castValue;
                return true;
            }
            case "m_fadeOutStart":
            case "fadeOutStart":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutStart = castValue;
                return true;
            }
            case "m_fadeOutEnd":
            case "fadeOutEnd":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutEnd = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutDuration = castValue;
                return true;
            }
            case "m_sensorAngle":
            case "sensorAngle":
            {
                if (value is not float castValue) return false;
                instance.m_sensorAngle = castValue;
                return true;
            }
            case "m_handIndex":
            case "handIndex":
            {
                if (value is not short[] castValue || castValue.Length != 2) return false;
                try
                {
                    _handIndexInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_isHandEnabled":
            case "isHandEnabled":
            {
                if (value is not bool[] castValue || castValue.Length != 2) return false;
                try
                {
                    _isHandEnabledInfo.SetValue(instance, value);
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
