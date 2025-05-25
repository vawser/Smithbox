// Automatically Generated

namespace HKLib.hk2018;

public class hkbBodyIkControlsModifier : hkbModifier, hkbVerifiable
{
    public string? m_profileName;

    public List<hkbBodyIkControlsModifier.ControlData> m_controlDatas = new();

    public hkbBodyIkControlsModifier.PosePredictionMode m_posePredictionMode;


    public enum PosePredictionMode : int
    {
        NO_PREDICTION = 0,
        SINGLE_FRAME_PREDICTION = 1,
        CONTINUOUS_PREDICTION = 2
    }

    public class PrecomputeData : IHavokObject
    {
    }


    public class ControlData : hkbEventDrivenBlendingObject
    {
        public string? m_controlPointName;

        public hkbBodyIkControlBits.Enum m_effectors;

        public float m_animationInfluence;

        public Vector4 m_targetPosition = new();

        public Quaternion m_targetRotation = new();

        public hkbHandle? m_targetHandle;

        public float m_targetTransitionDuration;

        public bool m_blendAnimationDuringTargetTransition;

    }


}

