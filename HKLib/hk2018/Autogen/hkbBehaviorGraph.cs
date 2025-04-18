// Automatically Generated

namespace HKLib.hk2018;

public class hkbBehaviorGraph : hkbGenerator, hkbVerifiable
{
    public hkbBehaviorGraph.VariableMode m_variableMode;

    public hkbGenerator? m_rootGenerator;

    public hkbBehaviorGraphData? m_data;


    public enum NodeIdRanges : int
    {
        FIRST_TRANSITION_EFFECT_ID = 64512,
        FIRST_DYNAMIC_NODE_ID = 64511,
        LAST_DYNAMIC_NODE_ID = 32255,
        LAST_STANDARD_NODE_ID = 32254
    }

    public enum VariableMode : int
    {
        VARIABLE_MODE_DISCARD_WHEN_INACTIVE = 0,
        VARIABLE_MODE_MAINTAIN_VALUES_WHEN_INACTIVE = 1
    }

}

