// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbGeneratorSyncInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbGeneratorSyncInfo>
{
    private static readonly System.Reflection.FieldInfo _syncPointsInfo = typeof(Havok.hk2014.hkbGeneratorSyncInfo).GetField("m_syncPoints")!;
    public hkbGeneratorSyncInfoData(Havok.hk2014.hkbGeneratorSyncInfo instance) : base(instance) { }

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
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
                return true;
            }
            case "m_localTime":
            case "localTime":
            {
                value = instance.m_localTime;
                return true;
            }
            case "m_playbackSpeed":
            case "playbackSpeed":
            {
                value = instance.m_playbackSpeed;
                return true;
            }
            case "m_numSyncPoints":
            case "numSyncPoints":
            {
                value = instance.m_numSyncPoints;
                return true;
            }
            case "m_isCyclic":
            case "isCyclic":
            {
                value = instance.m_isCyclic;
                return true;
            }
            case "m_isMirrored":
            case "isMirrored":
            {
                value = instance.m_isMirrored;
                return true;
            }
            case "m_isAdditive":
            case "isAdditive":
            {
                value = instance.m_isAdditive;
                return true;
            }
            case "m_activeInterval":
            case "activeInterval":
            {
                value = instance.m_activeInterval;
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
                if (value is not Havok.hk2014.hkbGeneratorSyncInfo.SyncPoint[] castValue || castValue.Length != 16) return false;
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
                if (value is not Havok.hk2014.hkbGeneratorSyncInfo.ActiveInterval castValue) return false;
                instance.m_activeInterval = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
