// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbGeneratorSyncInfoActiveIntervalData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbGeneratorSyncInfo.ActiveInterval>
{
    private static readonly System.Reflection.FieldInfo _syncPointsInfo = typeof(Havok.hk2018.hkbGeneratorSyncInfo.ActiveInterval).GetField("m_syncPoints")!;
    public hkbGeneratorSyncInfoActiveIntervalData(Havok.hk2018.hkbGeneratorSyncInfo.ActiveInterval instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_syncPoints":
            case "syncPoints":
            {
                value = instance.m_syncPoints;
                return true;
            }
            case "m_fraction":
            case "fraction":
            {
                value = instance.m_fraction;
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
            case "m_syncPoints":
            case "syncPoints":
            {
                if (value is not Havok.hk2018.hkbGeneratorSyncInfo.SyncPoint[] castValue || castValue.Length != 2) return false;
                try
                {
                    _syncPointsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_fraction":
            case "fraction":
            {
                if (value is not float castValue) return false;
                instance.m_fraction = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
