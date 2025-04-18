// Automatically Generated

namespace HKLib.hk2018;

public class TestSimpleExRecord : TestSimpleRecord
{
    public TestSimpleExRecord.Nested m_nested = new();

    public TestSimpleRecord m_record = new();


    public class Nested : IHavokObject
    {
        public TestSimpleRecord m_record = new();

    }


}

