// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpCompoundShapeSimdTreeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpCompoundShapeSimdTree>
{
    public hknpCompoundShapeSimdTreeData(Havok.hk2018.hknpCompoundShapeSimdTree instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_isCompact":
            case "isCompact":
            {
                value = instance.m_isCompact;
                return true;
            }
            case "m_points":
            case "points":
            {
                value = instance.m_points;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Havok.hk2018.hkcdSimdTree.Node> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_isCompact":
            case "isCompact":
            {
                if (value is not bool castValue) return false;
                instance.m_isCompact = castValue;
                return true;
            }
            case "m_points":
            case "points":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_points = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
