// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbGeneratorSyncInfoActiveIntervalData : HavokData<hkbGeneratorSyncInfo.ActiveInterval> 
{
    private static readonly System.Reflection.FieldInfo _syncPointsInfo = typeof(hkbGeneratorSyncInfo.ActiveInterval).GetField("m_syncPoints")!;
    public hkbGeneratorSyncInfoActiveIntervalData(HavokType type, hkbGeneratorSyncInfo.ActiveInterval instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_syncPoints":
            case "syncPoints":
            {
                if (instance.m_syncPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fraction":
            case "fraction":
            {
                if (instance.m_fraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_syncPoints":
            case "syncPoints":
            {
                if (value is not hkbGeneratorSyncInfo.SyncPoint[] castValue || castValue.Length != 2) return false;
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
