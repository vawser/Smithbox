// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBoneSpaceDeformerLocalBlockUnpackedPNTData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBoneSpaceDeformer.LocalBlockUnpackedPNT>
{
    private static readonly System.Reflection.FieldInfo _localPositionInfo = typeof(Havok.hk2014.hclBoneSpaceDeformer.LocalBlockUnpackedPNT).GetField("m_localPosition")!;
    private static readonly System.Reflection.FieldInfo _localNormalInfo = typeof(Havok.hk2014.hclBoneSpaceDeformer.LocalBlockUnpackedPNT).GetField("m_localNormal")!;
    private static readonly System.Reflection.FieldInfo _localTangentInfo = typeof(Havok.hk2014.hclBoneSpaceDeformer.LocalBlockUnpackedPNT).GetField("m_localTangent")!;
    public hclBoneSpaceDeformerLocalBlockUnpackedPNTData(Havok.hk2014.hclBoneSpaceDeformer.LocalBlockUnpackedPNT instance) : base(instance) { }

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
            case "m_localTangent":
            case "localTangent":
            {
                value = instance.m_localTangent;
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
            case "m_localTangent":
            case "localTangent":
            {
                if (value is not Vector4[] castValue || castValue.Length != 16) return false;
                try
                {
                    _localTangentInfo.SetValue(instance, value);
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
