// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxVertexDescriptionData : HavokData<hkxVertexDescription> 
{
    public hkxVertexDescriptionData(HavokType type, hkxVertexDescription instance) : base(type, instance) {}

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
                if (value is not List<hkxVertexDescription.ElementDecl> castValue) return false;
                instance.m_decls = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
