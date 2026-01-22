using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class FMGRef
{
    public string conditionField;
    public int conditionValue;
    public int offset;
    public string fmg;

    internal FMGRef(ParamMeta parent, string refString)
    {
        var conditionSplit = refString.Split('(', 2, StringSplitOptions.TrimEntries);
        var offsetSplit = conditionSplit[0].Split('+', 2);
        fmg = offsetSplit[0];
        if (offsetSplit.Length > 1)
        {
            offset = int.Parse(offsetSplit[1]);
        }

        if (conditionSplit.Length > 1 && conditionSplit[1].EndsWith(')'))
        {
            var condition = conditionSplit[1].Substring(0, conditionSplit[1].Length - 1)
                .Split('=', 2, StringSplitOptions.TrimEntries);
            conditionField = condition[0];
            conditionValue = int.Parse(condition[1]);
        }
    }

    internal string getStringForm()
    {
        return conditionField != null ? fmg + '(' + conditionField + '=' + conditionValue + ')' : fmg;
    }
}