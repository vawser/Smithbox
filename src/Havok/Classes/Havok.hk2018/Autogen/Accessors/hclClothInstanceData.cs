// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclClothInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclClothInstance>
{
    public hclClothInstanceData(Havok.hk2018.hclClothInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_stateTransitionIndex":
            case "stateTransitionIndex":
            {
                value = instance.m_stateTransitionIndex;
                return true;
            }
            case "m_stateTransitionStateIndex":
            case "stateTransitionStateIndex":
            {
                value = instance.m_stateTransitionStateIndex;
                return true;
            }
            case "m_stateTiming":
            case "stateTiming":
            {
                value = instance.m_stateTiming;
                return true;
            }
            case "m_update":
            case "update":
            {
                value = instance.m_update;
                return true;
            }
            case "m_multithreaded":
            case "multithreaded":
            {
                value = instance.m_multithreaded;
                return true;
            }
            case "m_currentState":
            case "currentState":
            {
                value = instance.m_currentState;
                return true;
            }
            case "m_viewersEnabled":
            case "viewersEnabled":
            {
                value = instance.m_viewersEnabled;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_stateTransitionIndex":
            case "stateTransitionIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_stateTransitionIndex = castValue;
                return true;
            }
            case "m_stateTransitionStateIndex":
            case "stateTransitionStateIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_stateTransitionStateIndex = castValue;
                return true;
            }
            case "m_stateTiming":
            case "stateTiming":
            {
                if (value is not float castValue) return false;
                instance.m_stateTiming = castValue;
                return true;
            }
            case "m_update":
            case "update":
            {
                if (value is not bool castValue) return false;
                instance.m_update = castValue;
                return true;
            }
            case "m_multithreaded":
            case "multithreaded":
            {
                if (value is not bool castValue) return false;
                instance.m_multithreaded = castValue;
                return true;
            }
            case "m_currentState":
            case "currentState":
            {
                if (value is not uint castValue) return false;
                instance.m_currentState = castValue;
                return true;
            }
            case "m_viewersEnabled":
            case "viewersEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_viewersEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
