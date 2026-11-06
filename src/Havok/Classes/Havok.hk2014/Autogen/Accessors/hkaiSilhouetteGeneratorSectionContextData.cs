// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiSilhouetteGeneratorSectionContextData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiSilhouetteGeneratorSectionContext>
{
    public hkaiSilhouetteGeneratorSectionContextData(Havok.hk2014.hkaiSilhouetteGeneratorSectionContext instance) : base(instance) { }

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
            case "m_generatedLastFrame":
            case "generatedLastFrame":
            {
                value = instance.m_generatedLastFrame;
                return true;
            }
            case "m_generatingThisFrame":
            case "generatingThisFrame":
            {
                value = instance.m_generatingThisFrame;
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
                if (value is not Havok.hk2014.hkQTransform castValue) return false;
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
                if (value is Havok.hk2014.hkaiSilhouetteGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            case "m_generatedLastFrame":
            case "generatedLastFrame":
            {
                if (value is not bool castValue) return false;
                instance.m_generatedLastFrame = castValue;
                return true;
            }
            case "m_generatingThisFrame":
            case "generatingThisFrame":
            {
                if (value is not bool castValue) return false;
                instance.m_generatingThisFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
