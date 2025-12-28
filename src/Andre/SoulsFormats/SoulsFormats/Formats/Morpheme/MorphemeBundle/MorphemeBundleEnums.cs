namespace SoulsFormats.Formats.Morpheme.MorphemeBundle
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    /// MorphemeBundle type
    /// </summary>
    public enum eBundleType : uint
    {
        Bundle_Invalid = uint.MaxValue,
        Bundle_Skeleton = 1,
        Bundle_SkeletonToAnimMap = 2,
        Bundle_DiscreteEventTrack = 3,
        Bundle_DurationEventTrack = 4,
        Bundle_CurveEventTrack = 5,
        Bundle_CharacterControllerDef = 7,
        Bundle_Network = 10,
        Bundle_FileHeader = 12,
        Bundle_FileNameLookupTable = 13
    };

    /// <summary>
    /// The variant of a RigToAnimMap
    /// </summary>
    public enum RigToAnimMapType : uint
    {
        MapPairs = 0,
        AnimToRig = 1,
        CompToRig = 2,
    }

    /// <summary>
    /// EventTrack Types
    /// </summary>
    public enum EventType : uint
    {
        kEventType_Discrete = 0,
        kEventType_Curve = 1,
        kEventType_Duration = 2,
        kEventTypeNumTypes = 3,
    };

    /// <summary>
    /// The type of a particular NMB node.
    /// </summary>
    public enum NodeType : uint
    {
        NodeType_NetworkInstance = 9,
        NodeType_StateMachine = 10,
        NodeType_ControlParameterFloat = 20,
        NodeType_ControlParameterVector3 = 21,
        NodeType_ControlParameterBool = 23,
        NodeType_ControlParameterInt = 24,
        NodeType_NodeAnimSyncEvents = 104,
        Nodetype_ShareChildren_105 = 105,
        NodeType_Blend2SyncEvents = 107,
        NodeType_Blend2Additive = 108,
        NodeType_Share1Child1InputCP_109,
        NodeType_ShareCreateFloatOutputAttribute_110 = 110,
        NodeType_ShareCreateFloatOutputAttribute_112 = 112,
        NodeType_Blend2Additive_2 = 114,
        NodeType_TwoBoneIK = 120,
        NodeType_LockFoot = 121,
        NodeType_ShareChildren1CompulsoryManyOptionalInputCPs_120 = 122,
        NodeType_Share1Child1InputCP = 125,
        NodeType_Freeze = 126,
        NodeType_ShareChildrenOptionalInputCPs = 129,
        NodeType_Switch = 131,
        NodeType_ShareChildren = 134,
        NodeType_ShareChildren_2 = 135,
        NodeType_ShareUpdateConnections1Child2OptionalInputCP = 136,
        NodeType_PredictiveUnevenTerrain = 138,
        NodeType_OperatorSmoothDamp = 142,
        NodeType_ShareCreateVector3OutputAttribute = 144,
        NodeType_OperatorRandomFloat = 146,
        NodeType_ShareChildren1CompulsoryManyOptionalInputCPs_150 = 150,
        NodeType_ShareChild1InputCP_151 = 151,
        NodeType_ShareChildren_153 = 153,
        NodeType_SubtractiveBlend = 170,
        NodeType_TransitSyncEvents = 400,
        NodeType_Transit = 402,
        NodeType_Share1Child1OptionalInputCP = 500,
        Unk550 = 550,
    };

    /// <summary>
    /// The type of node attributes
    /// </summary>
    public enum AttribType : ushort
    {
        ATTRIB_TYPE_BOOL = 0,
        ATTRIB_TYPE_UINT = 1,
        ATTRIB_TYPE_INT = 2,
        ATTRIB_TYPE_FLOAT = 3,
        ATTRIB_TYPE_VECTOR3 = 4,
        ATTRIB_TYPE_VECTOR4 = 5,
        ATTRIB_TYPE_BOOL_ARRAY = 6,
        ATTRIB_TYPE_UINT_ARRAY = 7,
        ATTRIB_TYPE_INT_ARRAY = 8,
        ATTRIB_TYPE_FLOAT_ARRAY = 9,
        ATTRIB_TYPE_UPDATE_PLAYBACK_POS = 10,
        ATTRIB_TYPE_PLAYBACK_POS = 11,
        ATTRIB_TYPE_UPDATE_SYNC_EVENT_PLAYBACK_POS = 12,
        ATTRIB_TYPE_TRANSFORM_BUFFER = 13,
        ATTRIB_TYPE_TRAJECTORY_DELTA_TRANSFORM = 14,
        ATTRIB_TYPE_TRANSFORM = 15,
        ATTRIB_TYPE_VELOCITY = 16,
        ATTRIB_TYPE_SYNC_EVENT_TRACK = 17,
        ATTRIB_TYPE_SAMPLED_EVENTS_BUFFER = 18,
        ATTRIB_TYPE_DURATION_EVENT_TRACK_SET = 19,
        ATTRIB_TYPE_RIG = 20,
        ATTRIB_TYPE_SOURCE_ANIM = 21,
        ATTRIB_TYPE_SOURCE_EVENT_TRACKS = 23,
        ATTRIB_TYPE_HEAD_LOOK_SETUP = 24,
        ATTRIB_TYPE_HEAD_LOOK_CHAIN = 25,
        ATTRIB_TYPE_GUN_AIM_SETUP = 26,
        ATTRIB_TYPE_GUN_AIM_IK_CHAIN = 27,
        ATTRIB_TYPE_TWO_BONE_IK_SETUP = 28,
        ATTRIB_TYPE_TWO_BONE_IK_CHAIN = 29,
        ATTRIB_TYPE_LOCK_FOOT_SETUP = 30,
        ATTRIB_TYPE_LOCK_FOOT_CHAIN = 31,
        ATTRIB_TYPE_LOCK_FOOT_STATE = 32,
        ATTRIB_TYPE_HIPS_IK_DEF = 33,
        ATTRIB_TYPE_HIPS_IK_ANIM_SET_DEF = 34,
        ATTRIB_TYPE_CLOSEST_ANIM_DEF = 35,
        ATTRIB_TYPE_CLOSEST_ANIM_DEF_ANIM_SET = 36,
        ATTRIB_TYPE_CLOSEST_ANIM_STATE = 37,
        ATTRIB_TYPE_STATE_MACHINE_DEF = 38,
        ATTRIB_TYPE_STATE_MACHINE = 39,
        ATTRIB_TYPE_CHARACTER_PROPERTIES = 42,
        ATTRIB_TYPE_CHARACTER_CONTROLLER_DEF = 43,
        ATTRIB_TYPE_PHYSICS_SETUP = 45,
        ATTRIB_TYPE_PHYSICS_SETUP_ANIM_SET = 46,
        ATTRIB_TYPE_PHYSICS_STATE = 47,
        ATTRIB_TYPE_PHYSICS_INITIALISATION = 48,
        ATTRIB_TYPE_PHYSICS_GROUPER_CONFIG = 49,
        ATTRIB_TYPE_FLOAT_OPERATION = 50,
        ATTRIB_TYPE_2_FLOAT_OPERATION = 51,
        ATTRIB_TYPE_SMOOTH_FLOAT_OPERATION = 52,
        ATTRIB_TYPE_RATE_OF_CHANGE_OPERATION = 53,
        ATTRIB_TYPE_RANDOM_FLOAT_OPERATION = 54,
        ATTRIB_TYPE_RANDOM_FLOAT_DEF = 55,
        ATTRIB_TYPE_NOISE_GEN_DEF = 56,
        ATTRIB_TYPE_SWITCH_DEF = 57,
        ATTRIB_TYPE_RAY_CAST_DEF = 58,
        ATTRIB_TYPE_TRANSIT_DEF = 59,
        ATTRIB_TYPE_TRANSIT_SYNC_EVENTS_DEF = 60,
        ATTRIB_TYPE_TRANSIT_SYNC_EVENTS = 61,
        ATTRIB_TYPE_DEAD_BLEND_DEF = 62,
        ATTRIB_TYPE_DEAD_BLEND_STATE = 63,
        ATTRIB_TYPE_BLEND_NXM_DEF = 65,
        ATTRIB_TYPE_ANIM_MIRRORED_MAPPING = 66,
        ATTRIB_TYPE_PLAYBACK_POS_INIT = 67,
        ATTRIB_TYPE_EMITTED_MESSAGE_MAP = 68,
        ATTRIB_TYPE_BASIC_UNEVEN_TERRAIN_SETUP = 69,
        ATTRIB_TYPE_BASIC_UNEVEN_TERRAIN_IK_SETUP = 70,
        ATTRIB_TYPE_BASIC_UNEVEN_TERRAIN_FOOT_LIFTING_TARGET = 71,
        ATTRIB_TYPE_BASIC_UNEVEN_TERRAIN_IK_STATE = 72,
        ATTRIB_TYPE_BASIC_UNEVEN_TERRAIN_CHAIN = 73,
        ATTRIB_TYPE_PREDICTIVE_UNEVEN_TERRAIN_IK_PREDICTION_STATE = 74,
        ATTRIB_TYPE_PREDICTIVE_UNEVEN_TERRAIN_FOOT_LIFTING_STATE = 75,
        ATTRIB_TYPE_PREDICTIVE_UNEVEN_TERRAIN_PREDICTION_DEF = 76,
        ATTRIB_TYPE_SCATTER_BLEND_ANALYSIS_DEF = 77,
        ATTRIB_TYPE_SCATTER_BLEND_1D_DEF = 78,
        ATTRIB_TYPE_SCATTER_BLEND_2D_DEF = 79,
        ATTRIB_TYPE_EMIT_MESSAGE_ON_CP_VALUE = 81,
        ATTRIB_TYPE_PHYSICS_INFO_DEF = 82,
        ATTRIB_TYPE_JOINT_LIMITS = 84,
        ATTRIB_TYPE_BLEND_FLAGS = 85,
        ATTRIB_TYPE_BLEND_WEIGHTS = 86,
        ATTRIB_TYPE_FEATHER_BLEND2_CHANNEL_ALPHAS = 87,
        ATTRIB_TYPE_RETARGET_STATE = 88,
        ATTRIB_TYPE_RIG_RETARGET_MAPPING = 89,
        ATTRIB_TYPE_SCALECHARACTER_STATE = 90,
        ATTRIB_TYPE_RETARGET_STORAGE_STATS = 91,
        ATTRIB_TYPE_C_C_OVERRIDE_CONDITIONS_DEF = 92,
        ATTRIB_TYPE_C_C_OVERRIDE_PROPERTIES_DEF = 93,
        ATTRIB_TYPE_C_C_OVERRIDE_CONDITIONS = 94,
    };
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
