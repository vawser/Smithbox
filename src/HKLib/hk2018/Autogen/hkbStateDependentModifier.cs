// Automatically Generated

namespace HKLib.hk2018;

public class hkbStateDependentModifier : hkbModifier, hkbVerifiable
{
    public bool m_applyModifierDuringTransition;

    public List<int> m_stateIds = new();

    public hkbModifier? m_modifier;

    public bool m_isActive;

    public hkbStateMachine? m_stateMachine;

}

