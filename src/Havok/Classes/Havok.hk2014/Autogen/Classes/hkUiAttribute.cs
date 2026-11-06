// Automatically Generated

namespace Havok.hk2014;

public class hkUiAttribute : IHavokObject
{
    public bool m_visible;

    public bool m_editable;

    public hkUiAttribute.HideCriteria m_hideCriteria;

    public string? m_label;

    public string? m_group;

    public string? m_hideBaseClassMembers;

    public bool m_endGroup;

    public bool m_endGroup2;

    public bool m_advanced;


    public enum HideCriteria : int
    {
        NONE = 0,
        MODELER_IS_MAX = 1,
        MODELER_IS_MAYA = 2,
        UI_SCHEME_IS_DESTRUCTION = 4,
        UI_SCHEME_IS_DESTRUCTION_2012 = 8
    }

}
