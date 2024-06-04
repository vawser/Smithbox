// Automatically Generated

namespace HKLib.hk2018;

public class hknpShapeTagCodec : hkReferencedObject
{
    public hknpShapeTagCodec.Hints m_hints;

    public hknpShapeTagCodec.Type m_type;


    [Flags]
    public enum Hints : int
    {
        HINT_CALL_OVERRIDE_INITIAL_FILTER_DATA = 1
    }

    public enum Type : int
    {
        TYPE_NULL = 0,
        TYPE_MATERIAL_PALETTE = 1,
        TYPE_UFM = 2,
        TYPE_USER = 3
    }

}

