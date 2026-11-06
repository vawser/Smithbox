// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclObjectSpaceDeformerLocalBlockPNData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclObjectSpaceDeformer.LocalBlockPN>
{
    private static readonly System.Reflection.FieldInfo _localPositionInfo = typeof(Havok.hk2018.hclObjectSpaceDeformer.LocalBlockPN).GetField("m_localPosition")!;
    private static readonly System.Reflection.FieldInfo _localNormalInfo = typeof(Havok.hk2018.hclObjectSpaceDeformer.LocalBlockPN).GetField("m_localNormal")!;
    public hclObjectSpaceDeformerLocalBlockPNData(Havok.hk2018.hclObjectSpaceDeformer.LocalBlockPN instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_localPosition":
            case "localPosition":
            {
                value = instance.m_localPosition;
                return true;
            }
            case "m_localNormal":
            case "localNormal":
            {
                value = instance.m_localNormal;
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
            case "m_localPosition":
            case "localPosition":
            {
                if (value is not Havok.hk2018.hkPackedVector3[] castValue || castValue.Length != 16) return false;
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
                if (value is not Havok.hk2018.hkPackedVector3[] castValue || castValue.Length != 16) return false;
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
