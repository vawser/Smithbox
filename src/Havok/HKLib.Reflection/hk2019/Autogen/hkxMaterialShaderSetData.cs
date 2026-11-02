// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxMaterialShaderSetData : HavokData<HKLib.hk2019.hkxMaterialShaderSet> 
{
    public hkxMaterialShaderSetData(HavokType type, HKLib.hk2019.hkxMaterialShaderSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shaders":
            case "shaders":
            {
                if (instance.m_shaders is not TGet castValue) return false;
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
            case "m_shaders":
            case "shaders":
            {
                if (value is not List<HKLib.hk2019.hkxMaterialShader?> castValue) return false;
                instance.m_shaders = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
