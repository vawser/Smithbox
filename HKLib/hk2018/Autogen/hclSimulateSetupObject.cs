// Automatically Generated

namespace HKLib.hk2018;

public class hclSimulateSetupObject : hclOperatorSetupObject
{
    public string? m_name;

    public hclSimClothSetupObject? m_simClothSetupObject;

    public List<hclSimulateSetupObject.Config> m_simulateConfigs = new();


    public class Config : IHavokObject
    {
        public string? m_name;

        public uint m_numberOfSubsteps;

        public bool m_adaptConstraintStiffness;

        public uint m_numberOfSolveIterations;

        public bool m_useAllCollidables;

        public List<string?> m_specificCollidables = new();

        public bool m_explicitConstraintOrder;

        public List<hclConstraintSetSetupObject?> m_constraintSetExecutionOrder = new();

    }


}

