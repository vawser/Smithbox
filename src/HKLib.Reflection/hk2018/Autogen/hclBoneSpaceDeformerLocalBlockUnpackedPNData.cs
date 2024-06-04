// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBoneSpaceDeformerLocalBlockUnpackedPNData : HavokData<hclBoneSpaceDeformer.LocalBlockUnpackedPN> 
{
    private static readonly System.Reflection.FieldInfo _localPositionInfo = typeof(hclBoneSpaceDeformer.LocalBlockUnpackedPN).GetField("m_localPosition")!;
    private static readonly System.Reflection.FieldInfo _localNormalInfo = typeof(hclBoneSpaceDeformer.LocalBlockUnpackedPN).GetField("m_localNormal")!;
    public hclBoneSpaceDeformerLocalBlockUnpackedPNData(HavokType type, hclBoneSpaceDeformer.LocalBlockUnpackedPN instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localPosition":
            case "localPosition":
            {
                if (instance.m_localPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localNormal":
            case "localNormal":
            {
                if (instance.m_localNormal is not TGet castValue) return false;
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
            case "m_localPosition":
            case "localPosition":
            {
                if (value is not Vector4[] castValue || castValue.Length != 16) return false;
                try
                {
                    _localPositionInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_localNormal":
            case "localNormal":
            {
                if (value is not Vector4[] castValue || castValue.Length != 16) return false;
                try
                {
                    _localNormalInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
