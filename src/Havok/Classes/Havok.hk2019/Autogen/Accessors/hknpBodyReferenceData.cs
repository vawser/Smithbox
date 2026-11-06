// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpBodyReferenceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpBodyReference>
{
    public hknpBodyReferenceData(Havok.hk2019.hknpBodyReference instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                value = instance.m_id;
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
            case "m_id":
            case "id":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
