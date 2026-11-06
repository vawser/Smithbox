// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMotionPropertiesLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMotionPropertiesLibrary>
{
    public hknpMotionPropertiesLibraryData(Havok.hk2019.hknpMotionPropertiesLibrary instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_motionProperties":
            case "motionProperties":
            {
                value = instance.m_motionProperties;
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
            case "m_motionProperties":
            case "motionProperties":
            {
                if (value is not Havok.hk2019.hknpThreadSafeObjectPoolHknpMotionProperties castValue) return false;
                instance.m_motionProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
