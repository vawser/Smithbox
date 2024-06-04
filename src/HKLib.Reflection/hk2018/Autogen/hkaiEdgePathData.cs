// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiEdgePathData : HavokData<hkaiEdgePath> 
{
    public hkaiEdgePathData(HavokType type, hkaiEdgePath instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (instance.m_edges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (instance.m_edgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                if (instance.m_edgeDataStriding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leftTurnRadius":
            case "leftTurnRadius":
            {
                if (instance.m_leftTurnRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rightTurnRadius":
            case "rightTurnRadius":
            {
                if (instance.m_rightTurnRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterRadius":
            case "characterRadius":
            {
                if (instance.m_characterRadius is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<hkaiEdgePath.Edge> castValue) return false;
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
