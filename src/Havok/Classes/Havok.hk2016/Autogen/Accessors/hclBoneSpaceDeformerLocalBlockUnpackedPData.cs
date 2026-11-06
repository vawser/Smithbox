// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclBoneSpaceDeformerLocalBlockUnpackedPData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclBoneSpaceDeformer.LocalBlockUnpackedP>
{
    private static readonly System.Reflection.FieldInfo _localPositionInfo = typeof(Havok.hk2016.hclBoneSpaceDeformer.LocalBlockUnpackedP).GetField("m_localPosition")!;
    public hclBoneSpaceDeformerLocalBlockUnpackedPData(Havok.hk2016.hclBoneSpaceDeformer.LocalBlockUnpackedP instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
