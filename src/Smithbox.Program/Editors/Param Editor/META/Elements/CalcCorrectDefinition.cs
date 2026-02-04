using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class CalcCorrectDefinition
{
    public string[] adjPoint_maxGrowVal;
    public string fcsMaxdist;
    public string[] stageMaxGrowVal;
    public string[] stageMaxVal;

    internal CalcCorrectDefinition(ParamMeta parent, string ccd)
    {
        var parts = ccd.Split(',');
        if (parts.Length == 11)
        {
            // FCS param curve
            var cclength = 5;
            stageMaxVal = new string[cclength];
            stageMaxGrowVal = new string[cclength];
            Array.Copy(parts, 0, stageMaxVal, 0, cclength);
            Array.Copy(parts, cclength, stageMaxGrowVal, 0, cclength);
            adjPoint_maxGrowVal = null;
            fcsMaxdist = parts[10];
        }
        else
        {
            var cclength = (parts.Length + 1) / 3;
            stageMaxVal = new string[cclength];
            stageMaxGrowVal = new string[cclength];
            adjPoint_maxGrowVal = new string[cclength - 1];
            Array.Copy(parts, 0, stageMaxVal, 0, cclength);
            Array.Copy(parts, cclength, stageMaxGrowVal, 0, cclength);
            Array.Copy(parts, cclength * 2, adjPoint_maxGrowVal, 0, cclength - 1);
        }
    }

    internal string getStringForm()
    {
        var str = string.Join(',', stageMaxVal) + ',' + string.Join(',', stageMaxGrowVal) + ',';
        if (adjPoint_maxGrowVal != null)
        {
            str += string.Join(',', adjPoint_maxGrowVal);
        }

        if (fcsMaxdist != null)
        {
            str += string.Join(',', fcsMaxdist);
        }

        return str;
    }
}