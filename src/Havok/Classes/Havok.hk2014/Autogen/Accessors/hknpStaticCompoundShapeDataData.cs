// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpStaticCompoundShapeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpStaticCompoundShapeData>
{
    public hknpStaticCompoundShapeDataData(Havok.hk2014.hknpStaticCompoundShapeData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabbTree":
            case "aabbTree":
            {
                value = instance.m_aabbTree;
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
            case "m_aabbTree":
            case "aabbTree":
            {
                if (value is not Havok.hk2014.hknpStaticCompoundShapeTree castValue) return false;
                instance.m_aabbTree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
