// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSphereShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSphereShape>
{
    public hclSphereShapeData(Havok.hk2014.hclSphereShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sphere":
            case "sphere":
            {
                value = instance.m_sphere;
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
            case "m_sphere":
            case "sphere":
            {
                if (value is not Havok.hk2014.hkSphere castValue) return false;
                instance.m_sphere = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
