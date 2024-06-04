// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbJigglerModifierInternalStateData : HavokData<hkbJigglerModifierInternalState> 
{
    public hkbJigglerModifierInternalStateData(HavokType type, hkbJigglerModifierInternalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentVelocitiesWS":
            case "currentVelocitiesWS":
            {
                if (instance.m_currentVelocitiesWS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentPositions":
            case "currentPositions":
            {
                if (instance.m_currentPositions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeStep":
            case "timeStep":
            {
                if (instance.m_timeStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initNextModify":
            case "initNextModify":
            {
                if (instance.m_initNextModify is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_currentVelocitiesWS":
            case "currentVelocitiesWS":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_currentVelocitiesWS = castValue;
                return true;
            }
            case "m_currentPositions":
            case "currentPositions":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_currentPositions = castValue;
                return true;
            }
            case "m_timeStep":
            case "timeStep":
            {
                if (value is not float castValue) return false;
                instance.m_timeStep = castValue;
                return true;
            }
            case "m_initNextModify":
            case "initNextModify":
            {
                if (value is not bool castValue) return false;
                instance.m_initNextModify = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
