using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface;

public static class ImPlotHelper
{
    public static bool TryGetSafeAxisLimits(double min, double max, out double safeMin, out double safeMax)
    {
        safeMin = safeMax = 0;

        if (double.IsNaN(min) || double.IsNaN(max) ||
            double.IsInfinity(min) || double.IsInfinity(max))
            return false;

        if (min == max)
        {
            safeMin = min - 1.0;
            safeMax = max + 1.0;
            return true;
        }

        if (min > max)
            (min, max) = (max, min);

        // Clamp insane ranges
        const double Limit = 1e12;
        min = Math.Clamp(min, -Limit, Limit);
        max = Math.Clamp(max, -Limit, Limit);

        safeMin = min;
        safeMax = max;
        return true;
    }
    public static bool SanitizeSeries(double[] data, double clamp = 1e12)
    {
        for (int i = 0; i < data.Length; i++)
        {
            if (double.IsNaN(data[i]) || double.IsInfinity(data[i]))
                return false;

            data[i] = Math.Clamp(data[i], -clamp, clamp);
        }
        return true;
    }
}
