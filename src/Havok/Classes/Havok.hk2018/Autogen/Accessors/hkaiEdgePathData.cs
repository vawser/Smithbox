// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiEdgePathData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiEdgePath>
{
    public hkaiEdgePathData(Havok.hk2018.hkaiEdgePath instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                value = instance.m_edges;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                value = instance.m_edgeData;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                value = instance.m_edgeDataStriding;
                return true;
            }
            case "m_leftTurnRadius":
            case "leftTurnRadius":
            {
                value = instance.m_leftTurnRadius;
                return true;
            }
            case "m_rightTurnRadius":
            case "rightTurnRadius":
            {
                value = instance.m_rightTurnRadius;
                return true;
            }
            case "m_characterRadius":
            case "characterRadius":
            {
                value = instance.m_characterRadius;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2018.hkaiEdgePath.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_edgeData = castValue;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                if (value is not int castValue) return false;
                instance.m_edgeDataStriding = castValue;
                return true;
            }
            case "m_leftTurnRadius":
            case "leftTurnRadius":
            {
                if (value is not float castValue) return false;
                instance.m_leftTurnRadius = castValue;
                return true;
            }
            case "m_rightTurnRadius":
            case "rightTurnRadius":
            {
                if (value is not float castValue) return false;
                instance.m_rightTurnRadius = castValue;
                return true;
            }
            case "m_characterRadius":
            case "characterRadius":
            {
                if (value is not float castValue) return false;
                instance.m_characterRadius = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
