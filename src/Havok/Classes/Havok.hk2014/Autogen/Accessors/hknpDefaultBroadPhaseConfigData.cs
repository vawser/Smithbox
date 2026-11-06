// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpDefaultBroadPhaseConfigData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpDefaultBroadPhaseConfig>
{
    private static readonly System.Reflection.FieldInfo _layersInfo = typeof(Havok.hk2014.hknpDefaultBroadPhaseConfig).GetField("m_layers")!;
    public hknpDefaultBroadPhaseConfigData(Havok.hk2014.hknpDefaultBroadPhaseConfig instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_layers":
            case "layers":
            {
                value = instance.m_layers;
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
            case "m_layers":
            case "layers":
            {
                if (value is not Havok.hk2014.hknpBroadPhaseConfig.Layer[] castValue || castValue.Length != 4) return false;
                try
                {
                    _layersInfo.SetValue(instance, value);
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
