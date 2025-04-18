// Automatically Generated

namespace HKLib.hk2018;

public class hclStateTransition : hkReferencedObject
{
    public string? m_name;

    public List<uint> m_stateIds = new();

    public List<hclStateTransition.StateTransitionData> m_stateTransitionData = new();

    public List<List<hkHandle<uint>>> m_simClothTransitionConstraints = new();


    public enum TransitionType : int
    {
        TRANSITION_INACTIVE = 0,
        ACQUIRE_VELOCITY_FROM_ANIMATION = 1,
        TRANSITION_TO_ANIMATION = 2,
        TRANSITION_FROM_ANIMATION = 4,
        BLEND_TO_ANIMATION = 8,
        BLEND_FROM_ANIMATION = 16,
        ANIMATION_TRANSITION_TYPE = 6,
        ANIMATION_BLEND_TYPE = 24,
        ANIMATION_TYPE = 30,
        TO_ANIMATION_TYPE = 10,
        FROM_ANIMATION_TYPE = 20,
        TRANSFER_VELOCITY_TO_SLOD = 32,
        ACQUIRE_VELOCITY_FROM_SLOD = 64,
        TRANSITION_TO_SLOD = 128,
        TRANSITION_FROM_SLOD = 256,
        BLEND_TO_SLOD = 512,
        BLEND_FROM_SLOD = 1024,
        SLOD_TRANSITION_TYPE = 384,
        SLOD_BLEND_TYPE = 1536,
        SLOD_TYPE = 1920,
        TO_SLOD_TYPE = 640,
        FROM_SLOD_TYPE = 1280,
        BLEND_NO_SIM_TRANSITION = 2048
    }

    public class StateTransitionData : IHavokObject
    {
        public List<hclStateTransition.SimClothTransitionData> m_simClothTransitionData = new();

        public List<hclStateTransition.BlendOpTransitionData> m_blendOpTransitionData = new();

        public bool m_simulatedState;

        public bool m_emptyState;

    }


    public class BlendOpTransitionData : IHavokObject
    {
        public List<int> m_bufferASimCloths = new();

        public List<int> m_bufferBSimCloths = new();

        public hclStateTransition.TransitionType m_transitionType;

        public hclBlendSomeVerticesOperator.BlendWeightType m_blendWeightType;

        public uint m_blendOperatorId;

    }


    public class SimClothTransitionData : IHavokObject
    {
        public bool m_isSimulated;

        public List<hkHandle<uint>> m_transitionConstraints = new();

        public uint m_transitionType;

    }


}

