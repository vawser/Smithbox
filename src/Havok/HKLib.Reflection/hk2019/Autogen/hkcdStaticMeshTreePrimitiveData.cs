// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdStaticMeshTreePrimitiveData : HavokData<HKLib.hk2019.hkcdStaticMeshTree.Primitive> 
{
    private static readonly System.Reflection.FieldInfo _indicesInfo = typeof(HKLib.hk2019.hkcdStaticMeshTree.Primitive).GetField("m_indices")!;
    public hkcdStaticMeshTreePrimitiveData(HavokType type, HKLib.hk2019.hkcdStaticMeshTree.Primitive instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_indices":
            case "indices":
            {
                if (instance.m_indices is not TGet castValue) return false;
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
            case "m_indices":
            case "indices":
            {
                if (value is not byte[] castValue || castValue.Length != 4) return false;
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
