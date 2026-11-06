// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSilhouetteRecorderSilhouettesSteppedEventData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSilhouetteRecorderSilhouettesSteppedEvent>
{
    public hkaiSilhouetteRecorderSilhouettesSteppedEventData(Havok.hk2014.hkaiSilhouetteRecorderSilhouettesSteppedEvent instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_eventType":
            case "eventType":
            {
                value = instance.m_eventType;
                return true;
            }
            case "m_stepThreading":
            case "stepThreading":
            {
                value = instance.m_stepThreading;
                return true;
            }
            case "m_generators":
            case "generators":
            {
                value = instance.m_generators;
                return true;
            }
            case "m_instanceTransforms":
            case "instanceTransforms":
            {
                value = instance.m_instanceTransforms;
                return true;
            }
            case "m_overlapManagerSections":
            case "overlapManagerSections":
            {
                value = instance.m_overlapManagerSections;
                return true;
            }
            case "m_updatedSections":
            case "updatedSections":
            {
                value = instance.m_updatedSections;
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
            case "m_eventType":
            case "eventType":
            {
                if (value is Havok.hk2014.hkaiSilhouetteRecorderReplayEvent.ReplayEventType castValue)
                {
                    instance.m_eventType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_eventType = (Havok.hk2014.hkaiSilhouetteRecorderReplayEvent.ReplayEventType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_stepThreading":
            case "stepThreading":
            {
                if (value is Havok.hk2014.hkaiWorld.StepThreading castValue)
                {
                    instance.m_stepThreading = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_stepThreading = (Havok.hk2014.hkaiWorld.StepThreading)uintValue;
                    return true;
                }
                return false;
            }
            case "m_generators":
            case "generators":
            {
                if (value is not List<Havok.hk2014.hkaiSilhouetteGenerator?> castValue) return false;
                instance.m_generators = castValue;
                return true;
            }
            case "m_instanceTransforms":
            case "instanceTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_instanceTransforms = castValue;
                return true;
            }
            case "m_overlapManagerSections":
            case "overlapManagerSections":
            {
                if (value is not List<Havok.hk2014.hkaiOverlapManager.Section> castValue) return false;
                instance.m_overlapManagerSections = castValue;
                return true;
            }
            case "m_updatedSections":
            case "updatedSections":
            {
                if (value is not Havok.hk2014.hkBitField castValue) return false;
                instance.m_updatedSections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
