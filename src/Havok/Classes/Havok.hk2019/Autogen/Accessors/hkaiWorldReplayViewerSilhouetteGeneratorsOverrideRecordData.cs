// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiWorldReplayViewerSilhouetteGeneratorsOverrideRecordData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiWorldReplayViewer.SilhouetteGeneratorsOverrideRecord>
{
    public hkaiWorldReplayViewerSilhouetteGeneratorsOverrideRecordData(Havok.hk2019.hkaiWorldReplayViewer.SilhouetteGeneratorsOverrideRecord instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generatorTransforms":
            case "generatorTransforms":
            {
                value = instance.m_generatorTransforms;
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
            case "m_generatorTransforms":
            case "generatorTransforms":
            {
                if (value is not List<Havok.hk2019.hkQTransform> castValue) return false;
                instance.m_generatorTransforms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
