// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbGeneratorSyncInfoData : HavokData<hkbGeneratorSyncInfo> 
{
    private static readonly System.Reflection.FieldInfo _syncPointsInfo = typeof(hkbGeneratorSyncInfo).GetField("m_syncPoints")!;
    public hkbGeneratorSyncInfoData(HavokType type, hkbGeneratorSyncInfo instance) : base(type, instance) {}

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
            case "m_duration":
            case "duration":
            {
                if (instance.m_duration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                if (instance.m_localTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_playbackSpeed":
            case "playbackSpeed":
            {
                if (instance.m_playbackSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numSyncPoints":
            case "numSyncPoints":
            {
                if (instance.m_numSyncPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isCyclic":
            case "isCyclic":
            {
                if (instance.m_isCyclic is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isMirrored":
            case "isMirrored":
            {
                if (instance.m_isMirrored is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isAdditive":
            case "isAdditive":
            {
                if (instance.m_isAdditive is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_activeInterval":
            case "activeInterval":
            {
                if (instance.m_activeInterval is not TGet castValue) return false;
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
                if (value is not hkbGeneratorSyncInfo.SyncPoint[] castValue || castValue.Length != 16) return false;
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
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                if (value is not float castValue) return false;
                instance.m_localTime = castValue;
                return true;
            }
            case "m_playbackSpeed":
            case "playbackSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_playbackSpeed = castValue;
                return true;
            }
            case "m_numSyncPoints":
            case "numSyncPoints":
            {
                if (value is not sbyte castValue) return false;
                instance.m_numSyncPoints = castValue;
                return true;
            }
            case "m_isCyclic":
            case "isCyclic":
            {
                if (value is not bool castValue) return false;
                instance.m_isCyclic = castValue;
                return true;
            }
            case "m_isMirrored":
            case "isMirrored":
            {
                if (value is not bool castValue) return false;
                instance.m_isMirrored = castValue;
                return true;
            }
            case "m_isAdditive":
            case "isAdditive":
            {
                if (value is not bool castValue) return false;
                instance.m_isAdditive = castValue;
                return true;
            }
            case "m_activeInterval":
            case "activeInterval":
            {
                if (value is not hkbGeneratorSyncInfo.ActiveInterval castValue) return false;
                instance.m_activeInterval = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
