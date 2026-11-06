// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStretchLinkConstraintSetMxData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStretchLinkConstraintSetMx>
{
    public hclStretchLinkConstraintSetMxData(Havok.hk2014.hclStretchLinkConstraintSetMx instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_batches":
            case "batches":
            {
                value = instance.m_batches;
                return true;
            }
            case "m_singles":
            case "singles":
            {
                value = instance.m_singles;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_batches":
            case "batches":
            {
                if (value is not List<Havok.hk2014.hclStretchLinkConstraintSetMx.Batch> castValue) return false;
                instance.m_batches = castValue;
                return true;
            }
            case "m_singles":
            case "singles":
            {
                if (value is not List<Havok.hk2014.hclStretchLinkConstraintSetMx.Single> castValue) return false;
                instance.m_singles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
