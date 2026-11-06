// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpStepInputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpStepInput>
{
    public hknpStepInputData(Havok.hk2018.hknpStepInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_deltaTime":
            case "deltaTime":
            {
                value = instance.m_deltaTime;
                return true;
            }
            case "m_followingStepMaxDeltaTime":
            case "followingStepMaxDeltaTime":
            {
                value = instance.m_followingStepMaxDeltaTime;
                return true;
            }
            case "m_numThreads":
            case "numThreads":
            {
                value = instance.m_numThreads;
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
