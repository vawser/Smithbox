// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkRootLevelContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkRootLevelContainer>
{
    public hkRootLevelContainerData(Havok.hk2015.hkRootLevelContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_namedVariants":
            case "namedVariants":
            {
                value = instance.m_namedVariants;
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
            case "m_namedVariants":
            case "namedVariants":
            {
                if (value is not List<Havok.hk2015.hkRootLevelContainer.NamedVariant> castValue) return false;
                instance.m_namedVariants = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
