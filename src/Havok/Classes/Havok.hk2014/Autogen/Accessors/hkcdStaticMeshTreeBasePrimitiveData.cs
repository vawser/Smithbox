// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdStaticMeshTreeBasePrimitiveData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdStaticMeshTreeBasePrimitive>
{
    private static readonly System.Reflection.FieldInfo _indicesInfo = typeof(Havok.hk2014.hkcdStaticMeshTreeBasePrimitive).GetField("m_indices")!;
    public hkcdStaticMeshTreeBasePrimitiveData(Havok.hk2014.hkcdStaticMeshTreeBasePrimitive instance) : base(instance) { }

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
