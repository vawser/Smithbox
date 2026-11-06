// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclUpdateSomeVertexFramesOperatorTriangleData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclUpdateSomeVertexFramesOperator.Triangle>
{
    private static readonly System.Reflection.FieldInfo _indicesInfo = typeof(Havok.hk2018.hclUpdateSomeVertexFramesOperator.Triangle).GetField("m_indices")!;
    public hclUpdateSomeVertexFramesOperatorTriangleData(Havok.hk2018.hclUpdateSomeVertexFramesOperator.Triangle instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_indices":
            case "indices":
            {
                value = instance.m_indices;
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
            case "m_indices":
            case "indices":
            {
                if (value is not ushort[] castValue || castValue.Length != 3) return false;
                try
                {
                    _indicesInfo.SetValue(instance, value);
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
