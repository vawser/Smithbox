// Automatically Generated

#nullable enable

namespace Havok.hk2012.Accessors;

internal sealed class hclObjectSpaceDeformerLocalBlockPData : Havok.Formats.HavokFieldAccess<Havok.hk2012.hclObjectSpaceDeformer.LocalBlockP>
{
    private static readonly System.Reflection.FieldInfo _localPositionInfo = typeof(Havok.hk2012.hclObjectSpaceDeformer.LocalBlockP).GetField("m_localPosition")!;
    public hclObjectSpaceDeformerLocalBlockPData(Havok.hk2012.hclObjectSpaceDeformer.LocalBlockP instance) : base(instance) { }

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
                if (value is not short[] castValue || castValue.Length != 64) return false;
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
