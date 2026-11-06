// Automatically Generated

namespace Havok.hk55;

public class hkpConstraintInstance : hkReferencedObject
{
    public hkpConstraintData? m_data;

    public hkpModifierConstraintAtom? m_constraintModifiers;

    public readonly hkpEntity?[] m_entities = new hkpEntity?[2];

    public hkpConstraintInstance.ConstraintPriority m_priority;

    public bool m_wantRuntime;

    public string? m_name;

    public ulong m_userData;


    public enum ConstraintPriority : int
    {
        PRIORITY_INVALID = 0,
        PRIORITY_PSI = 1,
        PRIORITY_TOI = 2,
        PRIORITY_TOI_HIGHER = 3,
        PRIORITY_TOI_FORCED = 4
    }

}
