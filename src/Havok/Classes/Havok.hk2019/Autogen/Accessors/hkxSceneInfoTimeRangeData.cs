// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxSceneInfoTimeRangeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxSceneInfo.TimeRange>
{
    public hkxSceneInfoTimeRangeData(Havok.hk2019.hkxSceneInfo.TimeRange instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_start":
            case "start":
            {
                value = instance.m_start;
                return true;
            }
            case "m_end":
            case "end":
            {
                value = instance.m_end;
                return true;
            }
            case "m_fps":
            case "fps":
            {
                value = instance.m_fps;
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
            case "m_start":
            case "start":
            {
                if (value is not float castValue) return false;
                instance.m_start = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (value is not float castValue) return false;
                instance.m_end = castValue;
                return true;
            }
            case "m_fps":
            case "fps":
            {
                if (value is not float castValue) return false;
                instance.m_fps = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
