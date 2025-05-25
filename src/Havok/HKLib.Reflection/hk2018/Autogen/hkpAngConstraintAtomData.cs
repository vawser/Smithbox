// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpAngConstraintAtomData : HavokData<hkpAngConstraintAtom> 
{
    private static readonly System.Reflection.FieldInfo _constrainedAxesInfo = typeof(hkpAngConstraintAtom).GetField("m_constrainedAxes")!;
    public hkpAngConstraintAtomData(HavokType type, hkpAngConstraintAtom instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_constrainedAxes":
            case "constrainedAxes":
            {
                if (instance.m_constrainedAxes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numConstrainedAxes":
            case "numConstrainedAxes":
            {
                if (instance.m_numConstrainedAxes is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_constrainedAxes":
            case "constrainedAxes":
            {
                if (value is not byte[] castValue || castValue.Length != 3) return false;
                try
                {
                    _constrainedAxesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numConstrainedAxes":
            case "numConstrainedAxes":
            {
                if (value is not byte castValue) return false;
                instance.m_numConstrainedAxes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
