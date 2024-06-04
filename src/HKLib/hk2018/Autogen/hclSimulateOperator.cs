// Automatically Generated

namespace HKLib.hk2018;

public class hclSimulateOperator : hclOperator
{
    public uint m_simClothIndex;

    public List<hclSimulateOperator.Config> m_simulateOpConfigs = new();


    public class Config : IHavokObject
    {
        public string? m_name;

        public List<int> m_constraintExecution = new();

        public List<bool> m_instanceCollidablesUsed = new();

        public byte m_subSteps;

        public byte m_numberOfSolveIterations;

        public bool m_useAllInstanceCollidables;

        public bool m_adaptConstraintStiffness;

    }


}

