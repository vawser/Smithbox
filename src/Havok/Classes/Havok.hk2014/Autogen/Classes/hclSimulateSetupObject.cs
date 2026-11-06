// Automatically Generated

namespace Havok.hk2014;

public class hclSimulateSetupObject : hclOperatorSetupObject
{
    public string? m_name;

    public hclSimClothSetupObject? m_simClothSetupObject;

    public uint m_numberOfSubsteps;

    public uint m_numberOfSolveIterations;

    public bool m_adaptConstraintStiffness;

    public bool m_explicitConstraintOrder;

    public List<hclConstraintSetSetupObject?> m_constraintSetExecutionOrder = new();

}
