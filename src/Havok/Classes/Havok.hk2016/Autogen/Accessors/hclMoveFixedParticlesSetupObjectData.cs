// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclMoveFixedParticlesSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclMoveFixedParticlesSetupObject>
{
    public hclMoveFixedParticlesSetupObjectData(Havok.hk2016.hclMoveFixedParticlesSetupObject instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_simClothSetupObject":
            case "simClothSetupObject":
            {
                value = instance.m_simClothSetupObject;
                return true;
            }
            case "m_displayBufferSetup":
            case "displayBufferSetup":
            {
                value = instance.m_displayBufferSetup;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_simClothSetupObject":
            case "simClothSetupObject":
            {
                if (value is null)
                {
                    instance.m_simClothSetupObject = default;
                    return true;
                }
                if (value is Havok.hk2016.hclSimClothSetupObject castValue)
                {
                    instance.m_simClothSetupObject = castValue;
                    return true;
                }
                return false;
            }
            case "m_displayBufferSetup":
            case "displayBufferSetup":
            {
                if (value is null)
                {
                    instance.m_displayBufferSetup = default;
                    return true;
                }
                if (value is Havok.hk2016.hclBufferSetupObject castValue)
                {
                    instance.m_displayBufferSetup = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
