using Andre.Formats;
using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.ParamEditor;

public class ParamStats
{
    public static void ShowValueDistribution(List<Param.Row> rows, PARAMDEF.Field field)
    {
        List<(object, int)> distribution = GetValueDistribution(rows, field);
        //sort
        //imgui print
    }

    public static object GetAverageValue(List<Param.Row> rows, PARAMDEF.Field field)
    {
        return null;
    }

    public static List<(object, int)> GetValueDistribution(List<Param.Row> rows, PARAMDEF.Field field)
    {
        return null;
    }
}
