// Automatically Generated

namespace Havok.hk2019.UnitTest;

public class SetterTest : IHavokObject
{
    public int m_intSet;

    public int m_int;

    public int m_stringSet;

    public string? m_string;

    public int m_pointerSet;

    public UnitTest.SetterTest.Foo? m_pointer;

    public int m_nestedSet;

    public UnitTest.SetterTest.Nested m_nested = new();

    public int m_arraySet;

    public List<int> m_array = new();

    public int m_arrayRecSet;

    public List<UnitTest.SetterTest.Nested> m_arrayRec = new();

    public int m_arrayArraySet;

    public List<List<int>> m_arrayArray = new();


    public class Nested : IHavokObject
    {
        public int m_int;

        public float m_float;

        public UnitTest.SetterTest.TwiceNested m_plain = new();

        public int m_twiceNestedSet;

        public UnitTest.SetterTest.TwiceNested m_twiceNested = new();

        public List<int> m_array = new();

        public List<object> m_variant = new();

    }

    public class TwiceNested : IHavokObject
    {
        public int m_int;

        public float m_float;

    }

    public class Foo : hkReferencedObject
    {
        public int m_int;

    }

}
