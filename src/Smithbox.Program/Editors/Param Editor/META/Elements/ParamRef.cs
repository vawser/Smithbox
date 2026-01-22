using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamRef
{
    public string ConditionField;
    public int ConditionValue;
    public int Offset;
    public string ParamName;

    internal ParamRef(ParamMeta parent, string refString)
    {
        if (refString == "")
        {
            TaskLogs.AddLog($"PARAM META: {parent.Name} - ParamRef string is empty.");
            return;
        }

        var conditionSplit = refString.Split('(', 2, StringSplitOptions.TrimEntries);
        var offsetSplit = conditionSplit[0].Split('+', 2);
        ParamName = offsetSplit[0];
        if (offsetSplit.Length > 1)
        {
            Offset = int.Parse(offsetSplit[1]);
        }

        if (conditionSplit.Length > 1 && conditionSplit[1].EndsWith(')'))
        {
            var condition = conditionSplit[1].Substring(0, conditionSplit[1].Length - 1)
                .Split('=', 2, StringSplitOptions.TrimEntries);
            ConditionField = condition[0];
            ConditionValue = int.Parse(condition[1]);
        }
    }

    internal string getStringForm()
    {
        return ConditionField != null ? ParamName + '(' + ConditionField + '=' + ConditionValue + ')' : ParamName;
    }
}