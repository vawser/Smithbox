// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpStepInputData : HavokData<hknpStepInput> 
{
    public hknpStepInputData(HavokType type, hknpStepInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_deltaTime":
            case "deltaTime":
            {
                if (instance.m_deltaTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_followingStepMaxDeltaTime":
            case "followingStepMaxDeltaTime":
            {
                if (instance.m_followingStepMaxDeltaTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numThreads":
            case "numThreads":
            {
                if (instance.m_numThreads is not TGet castValue) return false;
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
            case "m_deltaTime":
            case "deltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_deltaTime = castValue;
                return true;
            }
            case "m_followingStepMaxDeltaTime":
            case "followingStepMaxDeltaTime":
            {
                if (value is not float castValue) return false;
                instance.m_followingStepMaxDeltaTime = castValue;
                return true;
            }
            case "m_numThreads":
            case "numThreads":
            {
                if (value is not int castValue) return false;
                instance.m_numThreads = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
