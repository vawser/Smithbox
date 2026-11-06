// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiSilhouetteGeneratorSectionContextData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiSilhouetteGeneratorSectionContext>
{
    public hkaiSilhouetteGeneratorSectionContextData(Havok.hk2019.hkaiSilhouetteGeneratorSectionContext instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_lastRelativeTransform":
            case "lastRelativeTransform":
            {
                value = instance.m_lastRelativeTransform;
                return true;
            }
            case "m_generator":
            case "generator":
            {
                value = instance.m_generator;
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
            case "m_lastRelativeTransform":
            case "lastRelativeTransform":
            {
                if (value is not Havok.hk2019.hkQTransform castValue) return false;
                instance.m_lastRelativeTransform = castValue;
                return true;
            }
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiSilhouetteGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
