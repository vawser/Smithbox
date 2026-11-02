// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestCopyableReferencedObjectData : HavokData<HKLib.hk2019.UnitTest.CopyableReferencedObject> 
{
    public UnitTestCopyableReferencedObjectData(HavokType type, HKLib.hk2019.UnitTest.CopyableReferencedObject instance) : base(type, instance) {}

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
