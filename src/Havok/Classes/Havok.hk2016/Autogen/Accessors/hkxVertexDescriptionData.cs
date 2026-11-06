// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkxVertexDescriptionData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkxVertexDescription>
{
    public hkxVertexDescriptionData(Havok.hk2016.hkxVertexDescription instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_decls":
            case "decls":
            {
                value = instance.m_decls;
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
            case "m_decls":
            case "decls":
            {
                if (value is not List<Havok.hk2016.hkxVertexDescription.ElementDecl> castValue) return false;
                instance.m_decls = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
