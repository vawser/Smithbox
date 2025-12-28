using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

public static class VectorExtensions
{
    public static bool TryParseVector3(string s, out Vector3 result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(s))
            return false;

        var parts = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        if (float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
        {
            result = new Vector3(x, y, z);
            return true;
        }

        return false;
    }
}