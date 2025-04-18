// Automatically Generated

namespace HKLib.hk2018;

public class hctAttributeDescription : IHavokObject
{
    public string? m_name;

    public string? m_enabledBy;

    public hctAttributeDescription20151.ForcedType m_forcedType;

    public hctAttributeDescription.Enum? m_enum;

    public hctAttributeDescription20151.Hint m_hint;

    public bool m_clearHints;

    public float m_floatScale;


    public class Enum : IHavokObject
    {
        public string? m_name;

        public List<hctAttributeDescription.Enum.Item> m_items = new();


        public class Item : IHavokObject
        {
            public int m_value;

            public string? m_name;

        }


    }


}

