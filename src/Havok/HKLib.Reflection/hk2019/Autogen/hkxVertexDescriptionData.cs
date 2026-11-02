// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxVertexDescriptionData : HavokData<HKLib.hk2019.hkxVertexDescription> 
{
    public hkxVertexDescriptionData(HavokType type, HKLib.hk2019.hkxVertexDescription instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_decls":
            case "decls":
            {
                if (instance.m_decls is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_decls":
            case "decls":
            {
                if (value is not List<HKLib.hk2019.hkxVertexDescription.ElementDecl> castValue) return false;
                instance.m_decls = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
