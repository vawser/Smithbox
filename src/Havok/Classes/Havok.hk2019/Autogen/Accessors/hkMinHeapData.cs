// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMinHeapData<T> : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMinHeap<T>>
{
    public hkMinHeapData(Havok.hk2019.hkMinHeap<T> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_contents":
            case "contents":
            {
                value = instance.m_contents;
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
            case "m_contents":
            case "contents":
            {
                if (value is not List<T> castValue) return false;
                instance.m_contents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
