// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclObjectSpaceDeformerLocalBlockPData : HavokData<hclObjectSpaceDeformer.LocalBlockP> 
{
    private static readonly System.Reflection.FieldInfo _localPositionInfo = typeof(hclObjectSpaceDeformer.LocalBlockP).GetField("m_localPosition")!;
    public hclObjectSpaceDeformerLocalBlockPData(HavokType type, hclObjectSpaceDeformer.LocalBlockP instance) : base(type, instance) {}

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
                if (value is not hkPackedVector3[] castValue || castValue.Length != 16) return false;
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
            default:
            return false;
        }
    }

}
