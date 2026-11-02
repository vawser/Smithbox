// Automatically Generated

namespace HKLib.hk2019;

public class hkxAnimationData : IHavokObject
{
    public string? m_name;

    public List<hkxAnimationData.Layer_2> m_layers = new();

    public hkxAnimationData.AnimationInfo m_info = new();


    public class AnimationInfo : IHavokObject
    {
        public double m_startTime;

        public double m_endTime;

        public double m_framesPerSecond;

        public int m_rotationOrder;

        public Quaternion m_preRotation = new();

        public Quaternion m_postRotation = new();

        public hkQsTransform m_postTransform = new();

        public hkQsTransform m_postTransformInverse = new();

        public hkQsTransform m_bindPose = new();

        public Vector4 m_rotationOffset = new();

        public Vector4 m_rotationPivot = new();

        public Vector4 m_scalingOffset = new();

        public Vector4 m_scalingPivot = new();

        public Vector4 m_geometricT = new();

        public Vector4 m_geometricR = new();

        public Vector4 m_geometricS = new();

    }

    public class Layer : IHavokObject
    {

        public enum ScaleAccumulationMode : int
        {
            Multiply = 0,
            Additive = 1
        }

        public enum RotationAccumulationMode : int
        {
            ByLayer = 0,
            ByChannel = 1
        }

        public enum BlendMode : int
        {
            Additive = 0,
            Override = 1,
            OverridePassthrough = 2
        }

    }

    public class Layer_2 : IHavokObject
    {
        public string? m_name;

        public List<hkxAnimationData.Curve> m_curves = new();

        public double m_weight;

        public bool m_isMute;

        public bool m_isSolo;

        public bool m_isLocked;

        public Vector4 m_color = new();

        public hkxAnimationData.Layer.BlendMode m_blendMode;

        public hkxAnimationData.Layer.RotationAccumulationMode m_rotationAccumulationMode;

        public hkxAnimationData.Layer.ScaleAccumulationMode m_scaleAccumulationMode;

    }

    public class Curve : IHavokObject
    {
        public hkxAnimationData.CurveType m_type;

        public List<hkxAnimationData.CurveKey> m_keys = new();

        public double m_timeStart;

        public double m_timeEnd;

        public hkxAnimationData.ExtrapolationType m_beforeFirst;

        public ulong m_beforeFirstCount;

        public hkxAnimationData.ExtrapolationType m_afterLast;

        public ulong m_afterLastCount;

        public hkxAnimationData.CurveOwnerType m_ownerType;

        public string? m_name;

        public string? m_channelName;

        public string? m_ownerName;

        public List<string?> m_enums = new();

    }

    public class CurveKey : IHavokObject
    {
        public double m_value;

        public double m_time;

        public hkxAnimationData.TangentMode m_tangentMode;

        public hkxAnimationData.InterpolationType m_interpolationType;

        public hkxAnimationData.WeightedMode m_weightedMode;

        public hkxAnimationData.ConstantMode m_constantMode;

        public hkxAnimationData.VelocityMode m_velocityMode;

        public bool m_tangentBreak;

        public hkxAnimationData.TangentVisibility m_tangentVisibility;

        public readonly double[] m_data = new double[6];

    }

    public class CurveKeyTangentInfo : IHavokObject
    {
        public double m_derivative;

        public double m_weight;

        public bool m_weighted;

        public double m_velocity;

        public bool m_hasVelocity;

        public double m_autoValue;

    }

    public enum ExtrapolationType : int
    {
        eConstant = 1,
        eRepetition = 2,
        eMirrorRepetition = 3,
        eKeepSlope = 4,
        eRelativeRepetition = 5
    }

    public enum DataIndex : int
    {
        RightSlope = 0,
        NextLeftSlope = 1,
        Weights = 2,
        RightWeight = 2,
        NextLeftWeight = 3,
        Velocity = 4,
        RightVelocity = 4,
        NextLeftVelocity = 5,
        TCBTension = 0,
        TCBContinuity = 1,
        TCBBias = 2
    }

    public enum TangentVisibility : int
    {
        None = 0,
        Left = 1048576,
        Right = 2097152,
        Both = 3145728
    }

    public enum VelocityMode : int
    {
        None = 0,
        Right = 268435456,
        NextLeft = 536870912,
        All = 805306368
    }

    public enum ConstantMode : int
    {
        Standard = 0,
        Next = 256
    }

    public enum WeightedMode : int
    {
        None = 0,
        Right = 16777216,
        NextLeft = 33554432,
        All = 50331648
    }

    public enum InterpolationType : int
    {
        Constant = 2,
        Linear = 4,
        Cubic = 8
    }

    [Flags]
    public enum TangentMode : int
    {
        Auto = 256,
        TCB = 512,
        User = 1024,
        GenericBreak = 2048,
        Break = 3072,
        AutoBreak = 2304,
        GenericClamp = 4096,
        GenericTimeIndependent = 8192,
        GenericClampProgressive = 24576,
        ClampProgressiveAuto = 24832,
        TimeIndependentAuto = 8448
    }

    public enum CurveOwnerType : int
    {
        Undefined = 0,
        Char = 1,
        UChar = 2,
        Short = 3,
        UShort = 4,
        UInt = 5,
        LongLong = 6,
        ULongLong = 7,
        HalfFloat = 8,
        Bool = 9,
        Int = 10,
        Float = 11,
        Double = 12,
        Double2 = 13,
        Double3 = 14,
        Double4 = 15,
        Double4x4 = 16,
        Enum = 17,
        EnumM = 18,
        String = 19,
        Time = 20,
        Reference = 21,
        Blob = 22,
        Distance = 23,
        DateTime = 24,
        TypeCount = 25
    }

    public enum CurveType : int
    {
        TRANSLATION_COMPONENT_X = 0,
        TRANSLATION_COMPONENT_Y = 1,
        TRANSLATION_COMPONENT_Z = 2,
        ROTATION_COMPONENT_X = 3,
        ROTATION_COMPONENT_Y = 4,
        ROTATION_COMPONENT_Z = 5,
        SCALING_COMPONENT_X = 6,
        SCALING_COMPONENT_Y = 7,
        SCALING_COMPONENT_Z = 8,
        COLOR_RED = 9,
        COLOR_GREEN = 10,
        COLOR_BLUE = 11,
        USER_TYPE = 12
    }

}
