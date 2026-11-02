// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpVehicleEngineData : HavokData<HKLib.hk2019.hknpVehicleEngine> 
{
    public hknpVehicleEngineData(HavokType type, HKLib.hk2019.hknpVehicleEngine instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            default:
            return false;
        }
    }

}
