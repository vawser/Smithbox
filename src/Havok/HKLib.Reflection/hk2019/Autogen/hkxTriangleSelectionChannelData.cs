// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxTriangleSelectionChannelData : HavokData<HKLib.hk2019.hkxTriangleSelectionChannel> 
{
    public hkxTriangleSelectionChannelData(HavokType type, HKLib.hk2019.hkxTriangleSelectionChannel instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_selectedTriangles":
            case "selectedTriangles":
            {
                if (instance.m_selectedTriangles is not TGet castValue) return false;
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
            case "m_selectedTriangles":
            case "selectedTriangles":
            {
                if (value is not List<int> castValue) return false;
                instance.m_selectedTriangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
