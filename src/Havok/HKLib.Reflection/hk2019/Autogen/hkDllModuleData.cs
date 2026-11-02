// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkDllModuleData : HavokData<HKLib.hk2019.hkDllModule> 
{
    public hkDllModuleData(HavokType type, HKLib.hk2019.hkDllModule instance) : base(type, instance) {}

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
