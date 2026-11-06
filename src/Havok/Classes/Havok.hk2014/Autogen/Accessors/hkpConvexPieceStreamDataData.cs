// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpConvexPieceStreamDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpConvexPieceStreamData>
{
    public hkpConvexPieceStreamDataData(Havok.hk2014.hkpConvexPieceStreamData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_convexPieceStream":
            case "convexPieceStream":
            {
                value = instance.m_convexPieceStream;
                return true;
            }
            case "m_convexPieceOffsets":
            case "convexPieceOffsets":
            {
                value = instance.m_convexPieceOffsets;
                return true;
            }
            case "m_convexPieceSingleTriangles":
            case "convexPieceSingleTriangles":
            {
                value = instance.m_convexPieceSingleTriangles;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_convexPieceStream":
            case "convexPieceStream":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_convexPieceStream = castValue;
                return true;
            }
            case "m_convexPieceOffsets":
            case "convexPieceOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_convexPieceOffsets = castValue;
                return true;
            }
            case "m_convexPieceSingleTriangles":
            case "convexPieceSingleTriangles":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_convexPieceSingleTriangles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
