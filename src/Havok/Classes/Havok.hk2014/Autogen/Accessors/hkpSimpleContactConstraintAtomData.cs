// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSimpleContactConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSimpleContactConstraintAtom>
{
    public hkpSimpleContactConstraintAtomData(Havok.hk2014.hkpSimpleContactConstraintAtom instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_sizeOfAllAtoms":
            case "sizeOfAllAtoms":
            {
                value = instance.m_sizeOfAllAtoms;
                return true;
            }
            case "m_numContactPoints":
            case "numContactPoints":
            {
                value = instance.m_numContactPoints;
                return true;
            }
            case "m_numReservedContactPoints":
            case "numReservedContactPoints":
            {
                value = instance.m_numReservedContactPoints;
                return true;
            }
            case "m_numUserDatasForBodyA":
            case "numUserDatasForBodyA":
            {
                value = instance.m_numUserDatasForBodyA;
                return true;
            }
            case "m_numUserDatasForBodyB":
            case "numUserDatasForBodyB":
            {
                value = instance.m_numUserDatasForBodyB;
                return true;
            }
            case "m_contactPointPropertiesStriding":
            case "contactPointPropertiesStriding":
            {
                value = instance.m_contactPointPropertiesStriding;
                return true;
            }
            case "m_maxNumContactPoints":
            case "maxNumContactPoints":
            {
                value = instance.m_maxNumContactPoints;
                return true;
            }
            case "m_info":
            case "info":
            {
                value = instance.m_info;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2014.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_sizeOfAllAtoms":
            case "sizeOfAllAtoms":
            {
                if (value is not ushort castValue) return false;
                instance.m_sizeOfAllAtoms = castValue;
                return true;
            }
            case "m_numContactPoints":
            case "numContactPoints":
            {
                if (value is not ushort castValue) return false;
                instance.m_numContactPoints = castValue;
                return true;
            }
            case "m_numReservedContactPoints":
            case "numReservedContactPoints":
            {
                if (value is not ushort castValue) return false;
                instance.m_numReservedContactPoints = castValue;
                return true;
            }
            case "m_numUserDatasForBodyA":
            case "numUserDatasForBodyA":
            {
                if (value is not byte castValue) return false;
                instance.m_numUserDatasForBodyA = castValue;
                return true;
            }
            case "m_numUserDatasForBodyB":
            case "numUserDatasForBodyB":
            {
                if (value is not byte castValue) return false;
                instance.m_numUserDatasForBodyB = castValue;
                return true;
            }
            case "m_contactPointPropertiesStriding":
            case "contactPointPropertiesStriding":
            {
                if (value is not byte castValue) return false;
                instance.m_contactPointPropertiesStriding = castValue;
                return true;
            }
            case "m_maxNumContactPoints":
            case "maxNumContactPoints":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxNumContactPoints = castValue;
                return true;
            }
            case "m_info":
            case "info":
            {
                if (value is not Havok.hk2014.hkpSimpleContactConstraintDataInfo castValue) return false;
                instance.m_info = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
