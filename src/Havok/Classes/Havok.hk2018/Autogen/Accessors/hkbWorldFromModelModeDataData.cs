// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbWorldFromModelModeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbWorldFromModelModeData>
{
    public hkbWorldFromModelModeDataData(Havok.hk2018.hkbWorldFromModelModeData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_poseMatchingBone0":
            case "poseMatchingBone0":
            {
                value = instance.m_poseMatchingBone0;
                return true;
            }
            case "m_poseMatchingBone1":
            case "poseMatchingBone1":
            {
                value = instance.m_poseMatchingBone1;
                return true;
            }
            case "m_poseMatchingBone2":
            case "poseMatchingBone2":
            {
                value = instance.m_poseMatchingBone2;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                value = instance.m_mode;
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
            case "m_poseMatchingBone0":
            case "poseMatchingBone0":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone0 = castValue;
                return true;
            }
            case "m_poseMatchingBone1":
            case "poseMatchingBone1":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone1 = castValue;
                return true;
            }
            case "m_poseMatchingBone2":
            case "poseMatchingBone2":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone2 = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (value is Havok.hk2018.hkbWorldFromModelModeData.WorldFromModelMode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_mode = (Havok.hk2018.hkbWorldFromModelModeData.WorldFromModelMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
