// Automatically Generated

namespace HKLib.hk2018;

public class CustomManualSelectorGenerator : hkbGenerator, hkbVerifiable
{
    public List<hkbGenerator?> m_generators = new();

    public CustomManualSelectorGenerator.OffsetType m_offsetType;

    public int m_animId;

    public CustomManualSelectorGenerator.AnimeEndEventType m_animeEndEventType;

    public bool m_enableScript;

    public bool m_enableTae;

    public CustomManualSelectorGenerator.ChangeTypeOfSelectedIndexAfterActivate m_changeTypeOfSelectedIndexAfterActivate;

    public hkbTransitionEffect? m_generatorChangedTransitionEffect;

    public int m_checkAnimEndSlotNo;

    public CustomManualSelectorGenerator.ReplanningAI m_replanningAI;

    public CustomManualSelectorGenerator.RideSync m_rideSync;


    public enum ChangeTypeOfSelectedIndexAfterActivate : int
    {
        NONE = 0,
        SELF_TRANSITION = 1,
        UPDATE = 2
    }

    public enum RideSync : int
    {
        RideSync_Disable = 0,
        RideSync_Enable = 1
    }

    public enum ReplanningAI : int
    {
        Enable = 0,
        Disable = 1
    }

    public enum AnimeEndEventType : int
    {
        FireNextStateEvent = 0,
        FireStateEndEvent = 1,
        FireIdleEvent = 2,
        None = 3
    }

    public enum OffsetType : int
    {
        WeaponCategoryRight = 13,
        WeaponCategoryLeft = 14,
        IdleCategory = 11,
        OffsetNone = 0,
        AnimIdOffset = 15,
        WeaponCategoryHandStyle = 16,
        MagicCategory = 17,
        SwordArtsCategory = 18
    }

}

