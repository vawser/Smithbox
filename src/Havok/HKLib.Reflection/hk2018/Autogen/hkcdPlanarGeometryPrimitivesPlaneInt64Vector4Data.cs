// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkcdPlanarGeometryPrimitives;
using Plane = HKLib.hk2018.hkcdPlanarGeometryPrimitives.Plane;

namespace HKLib.Reflection.hk2018;

internal class hkcdPlanarGeometryPrimitivesPlaneInt64Vector4Data : HavokData<Plane.Int64Vector4> 
{
    private static readonly System.Reflection.FieldInfo _vecInfo = typeof(Plane.Int64Vector4).GetField("m_vec")!;
    public hkcdPlanarGeometryPrimitivesPlaneInt64Vector4Data(HavokType type, Plane.Int64Vector4 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vec":
            case "vec":
            {
                if (instance.m_vec is not TGet castValue) return false;
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
            case "m_vec":
            case "vec":
            {
                if (value is not long[] castValue || castValue.Length != 4) return false;
                try
                {
                    _vecInfo.SetValue(instance, value);
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
