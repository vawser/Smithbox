using SoulsFormats;

namespace StudioCore.Editors.BehaviorEditor;

public enum BhvFieldType { U8, S16, S32, F32 }

/// <summary>What a field's value refers to, for display (and later links and checks)</summary>
public enum BhvRefKind
{
    None,
    /// <summary>Index into the file's states</summary>
    State,
    /// <summary>Index into the file's animation records</summary>
    Animation,
    /// <summary>TAE animation ID</summary>
    TaeAnimation,
    /// <summary>BehaviorChangeStateMatrixParam row ID (0 = none)</summary>
    ChangeStateMatrixParam,
    /// <summary>SpEffectParam row ID</summary>
    SpEffectParam,
}

/// <summary>
/// A known field inside one of the raw BHV blobs (state body, transition params, condition data, animation record,
/// preamble). Offsets are into the blob.
/// </summary>
public record BhvField(int Offset, BhvFieldType Type, string LabelKey, string Enum = null, BhvRefKind Ref = BhvRefKind.None)
{
    public int Size => Type switch { BhvFieldType.U8 => 1, BhvFieldType.S16 => 2, _ => 4 };

    /// <summary>The value, or null if the blob is too short to hold it</summary>
    public double? Read(byte[] blob)
    {
        if (blob == null || Offset + Size > blob.Length)
            return null;

        return Type switch
        {
            BhvFieldType.U8 => blob[Offset],
            BhvFieldType.S16 => BitConverter.ToInt16(blob, Offset),
            BhvFieldType.S32 => BitConverter.ToInt32(blob, Offset),
            _ => BitConverter.ToSingle(blob, Offset),
        };
    }
}

/// <summary>
/// Where the known fields sit in each blob, by version and file kind. Sources: Claude/Documentation/BHV Format.md
/// (enummatch, parammatch, refmatch, animmatch, exe accessors and runtime capture) and the P0 renumber survey in
/// Claude/Documentation/BHV Editor Analysis.md, which accepted the index slots below on every vanilla file.
/// </summary>
public static class BhvLayout
{
    // ---- Index slots (renumber survey: every vanilla value is -1 or in range) ----

    private static readonly int[] WeaponPreambleStates = [0x02, 0x06, 0x08, 0x0A, 0x16];
    private static readonly int[] WeaponPreambleStatesV6 = [0x02, 0x06, 0x08, 0x0A]; // 0x16 had an out-of-range value
    private static readonly int[] BasePreambleStates =
        [0x50, 0x52, 0x54, 0x56, 0x58, 0x5A, 0x5C, 0x5E, 0x60, 0x62, 0x80, 0xCC, 0xCE, 0xD0, 0xD4];

    /// <summary>s16 state indices in the preamble (-1 = none)</summary>
    public static int[] PreambleStateSlots(BHV b) => b.Kind switch
    {
        BHVKind.Weapon => b.Version == 6 ? WeaponPreambleStatesV6 : WeaponPreambleStates,
        BHVKind.Base => BasePreambleStates,
        _ => [],
    };

    /// <summary>s16 animation indices in a state body (-1 = none). Weapon 0x52/0x54 are rejected in v5.</summary>
    public static int[] BodyAnimationSlots(BHV b) =>
        b.Kind is BHVKind.Weapon && b.Version != 5
            ? [BHV.V12.BodyAnimationIndex, BHV.V12.WeaponBodyAnimationIndex2, BHV.V12.WeaponBodyAnimationIndex2 + 2]
            : [BHV.V12.BodyAnimationIndex];

    // ---- Field tables ----

    public static List<BhvField> StateBodyFields(BHV b)
    {
        var fields = new List<BhvField>();
        var slots = BodyAnimationSlots(b);

        fields.Add(new(slots[0], BhvFieldType.S16, "BHV_Field_State_Animation", Ref: BhvRefKind.Animation));
        if (slots.Length > 1)
        {
            fields.Add(new(slots[1], BhvFieldType.S16, "BHV_Field_State_Animation_2", Ref: BhvRefKind.Animation));
            fields.Add(new(slots[2], BhvFieldType.S16, "BHV_Field_State_Animation_3", Ref: BhvRefKind.Animation));
        }

        // Enum fields were matched on v12 only; older versions predate the enum tables
        if (b.Version == 12)
        {
            if (b.Kind is BHVKind.Weapon)
            {
                fields.Add(new(BHV.V12.WeaponBodyLocalActionType, BhvFieldType.U8, "BHV_Field_State_Local_Action_Type", BhvEnums.LocalActionType));
                for (int i = 0; i < 4; i++)
                    fields.Add(new(BHV.V12.WeaponBodyAtkActionTypes + i, BhvFieldType.U8, $"BHV_Field_State_Atk_Action_Type_{i + 1}", BhvEnums.AtkActionType));
            }
            else if (b.Kind is BHVKind.Base)
            {
                fields.Add(new(BHV.V12.BaseBodyActionAiType, BhvFieldType.U8, "BHV_Field_State_Action_Ai_Type", BhvEnums.ActionAiType));
                fields.Add(new(BHV.V12.BaseBodyActionSoundType, BhvFieldType.U8, "BHV_Field_State_Action_Sound_Type", BhvEnums.ActionSoundType));
                for (int i = 0; i < 4; i++)
                    fields.Add(new(BHV.V12.BaseBodyActionTypes + i, BhvFieldType.U8, $"BHV_Field_State_Action_Type_{i + 1}", BhvEnums.ActionType));
            }
        }

        return fields.OrderBy(f => f.Offset).ToList();
    }

    public static List<BhvField> TransitionParamFields(BHV b, byte[] parameters)
    {
        var fields = new List<BhvField>
        {
            new(0x00, BhvFieldType.U8, "BHV_Field_Params_Flag_00"),
            new(0x01, BhvFieldType.U8, "BHV_Field_Params_Type"),
        };

        // Types 3 and 4 (v10/v12)
        if (b.Version >= 10 && parameters.Length >= 0x20)
        {
            fields.Add(new(0x02, BhvFieldType.U8, "BHV_Field_Params_Has_Time"));
            fields.Add(new(BHV.V12.ParamsChangeStateMatrixId, BhvFieldType.S32, "BHV_Field_Params_Change_State_Matrix", Ref: BhvRefKind.ChangeStateMatrixParam));
            fields.Add(new(0x08, BhvFieldType.F32, "BHV_Field_Params_Time"));
            for (int i = 0; i < 4; i++)
                fields.Add(new(0x0C + i, BhvFieldType.U8, $"BHV_Field_Params_Flag_{0x0C + i:X2}"));
        }

        return fields;
    }

    public static List<BhvField> ConditionDataFields(BHV b, BHV.Condition c)
    {
        var fields = new List<BhvField>();

        // SpEffect check: holds in v5, v6 and v12
        if (c.Type == 43 && b.Version != 10)
            fields.Add(new(BHV.V12.Condition43SpEffectId, BhvFieldType.S32, "BHV_Field_Condition_SpEffect", Ref: BhvRefKind.SpEffectParam));

        if (b.Version == 12)
        {
            if (c.Type == 30)
                fields.Add(new(BHV.V12.Condition30ActionType, BhvFieldType.U8, "BHV_Field_Condition_Action_Type", BhvEnums.ActionType));
            else if (c.Type == 57)
                fields.Add(new(BHV.V12.Condition57AtkActionType, BhvFieldType.U8, "BHV_Field_Condition_Atk_Action_Type", BhvEnums.AtkActionType));
            else if (c.Type == 48)
                fields.Add(new(BHV.V12.Condition48LocalActionType, BhvFieldType.U8, "BHV_Field_Condition_Local_Action_Type", BhvEnums.LocalActionType));
        }

        return fields;
    }

    public static List<BhvField> AnimationFields(BHV b, bool isWeapon)
    {
        var fields = new List<BhvField>
        {
            new(BHV.V12.AnimationId, BhvFieldType.S32, "BHV_Field_Animation_Id", Ref: BhvRefKind.TaeAnimation),
        };

        if (b.Version >= 10)
        {
            for (int i = 0; i < 3; i++)
                fields.Add(new(BHV.V12.AnimationExtraIds + 4 * i, BhvFieldType.S32, $"BHV_Field_Animation_Extra_Id_{i + 1}", Ref: BhvRefKind.TaeAnimation));
            fields.Add(new(BHV.V12.AnimationBlendTime, BhvFieldType.F32, "BHV_Field_Animation_Blend_Time"));

            if (isWeapon)
            {
                for (int i = 0; i < 4; i++)
                {
                    fields.Add(new(BHV.V12.AnimationPartWeights + 4 * i, BhvFieldType.S16, $"BHV_Field_Animation_Part_{i + 1}"));
                    fields.Add(new(BHV.V12.AnimationPartWeights + 4 * i + 2, BhvFieldType.S16, $"BHV_Field_Animation_Weight_{i + 1}"));
                }
            }
        }

        return fields;
    }

    public static List<BhvField> PreambleFields(BHV b)
    {
        var fields = new List<BhvField>();

        if (b.Kind is BHVKind.Weapon)
            fields.Add(new(0x00, BhvFieldType.S16, "BHV_Field_Preamble_Weapon_Number"));

        foreach (var slot in PreambleStateSlots(b))
            fields.Add(new(slot, BhvFieldType.S16, "BHV_Field_Preamble_State", Ref: BhvRefKind.State));

        if (b.Kind is BHVKind.Base)
        {
            fields.Add(new(BHV.V12.BasePreambleFlag, BhvFieldType.U8, "BHV_Field_Preamble_Flag_A4"));
            fields.Add(new(BHV.V12.BasePreambleFlag + 1, BhvFieldType.U8, "BHV_Field_Preamble_Value_A5"));
        }

        return fields.OrderBy(f => f.Offset).ToList();
    }
}
