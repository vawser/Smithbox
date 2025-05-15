using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StudioCore.Editors.ParamEditor.META;


public class ParamDisplayName
{
    public string Param;
    public string Name;

    public ParamDisplayName(ParamMeta parent, XmlNode node)
    {
        Param = "";
        Name = "";

        if (node.Attributes["Param"] != null)
        {
            Param = node.Attributes["Param"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamColorEdit Name property for {Param}");
        }

        if (node.Attributes["Name"] != null)
        {
            Name = node.Attributes["Name"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamColorEdit Name property for {Name}");
        }
    }
}

public class ParamColorEdit
{
    public string Name;
    public string Fields;
    public string PlacedField;

    public ParamColorEdit(ParamMeta parent, XmlNode colorEditNode)
    {
        Name = "";
        Fields = "";
        PlacedField = "";

        if (colorEditNode.Attributes["Name"] != null)
        {
            Name = colorEditNode.Attributes["Name"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamColorEdit Name property for {colorEditNode.Name}");
        }
        if (colorEditNode.Attributes["Fields"] != null)
        {
            Fields = colorEditNode.Attributes["Fields"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamColorEdit Fields property for {colorEditNode.Name}");
        }
        if (colorEditNode.Attributes["PlacedField"] != null)
        {
            PlacedField = colorEditNode.Attributes["PlacedField"].InnerText;
        }
        else
        {
            TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamColorEdit PlacedField property for {colorEditNode.Name}");
        }
    }
}

public class ParamEnum
{
    public string Name;

    public Dictionary<string, string> Values = new(); // using string as an intermediate type. first string is value, second is name.

    public ParamEnum(ParamMeta parent, XmlNode enumNode)
    {
        Name = "";

        if (enumNode.Attributes["Name"] != null)
        {
            Name = enumNode.Attributes["Name"].InnerText;
        }
        else
        {
            //TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamEnum Name property for {enumNode.Name}", LogLevel.Error);
        }

        foreach (XmlNode option in enumNode.SelectNodes("Option"))
        {
            if (option.Attributes["Value"] != null)
            {
                Values[option.Attributes["Value"].InnerText] = option.Attributes["Name"].InnerText;
            }
            else
            {
                //TaskLogs.AddLog($"PARAM META: {parent.Name} - Unable to populate ParamEnum Option Attribute Value property for {enumNode.Name}", LogLevel.Error);
            }
        }
    }
}

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

public class TexRef
{
    /// <summary>
    /// The lookup process to use.
    /// </summary>
    public string LookupType = "";

    /// <summary>
    /// The name of the texture container.
    /// </summary>
    public string TextureContainer = "";

    /// <summary>
    /// The name of the texture file within the texture container.
    /// </summary>
    public string TextureFile = "";

    /// <summary>
    /// The param row field that the image index is taken from.
    /// </summary>
    public string TargetField = "";

    /// <summary>
    /// The initial part of the subtexture filename to match with.
    /// </summary>
    public string SubTexturePrefix = "";

    internal TexRef(ParamMeta parent, string refString)
    {
        var refSplit = refString.Split('/');

        LookupType = refSplit[0];

        if (refSplit.Length > 1)
        {
            TextureContainer = refSplit[1];
        }
        if (refSplit.Length > 2)
        {
            TextureFile = refSplit[2];
        }
        if (refSplit.Length > 3)
        {
            TargetField = refSplit[3];
        }

        if (LookupType == "Direct")
        {
            if (refSplit.Length > 4)
            {
                SubTexturePrefix = refSplit[4];
            }
        }
    }

    internal string getStringForm()
    {
        return TextureFile;
    }
}

public class ExtRef
{
    public string name;
    public List<string> paths;

    internal ExtRef(ParamMeta parent, string refString)
    {
        var parts = refString.Split(",");
        name = parts[0];
        paths = parts.Skip(1).ToList();
    }

    internal string getStringForm()
    {
        return name + ',' + string.Join(',', paths);
    }
}

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

public class SoulCostDefinition
{
    public string adjustment_value;
    public string boundry_inclination_soul;
    public string boundry_value;
    public int cost_row;
    public string init_inclination_soul;
    public int max_level_for_game;

    internal SoulCostDefinition(ParamMeta parent, string ccd)
    {
        var parts = ccd.Split(',');
        init_inclination_soul = parts[0];
        adjustment_value = parts[1];
        boundry_inclination_soul = parts[2];
        boundry_value = parts[3];
        cost_row = int.Parse(parts[4]);
        max_level_for_game = int.Parse(parts[5]);
    }

    internal string getStringForm()
    {
        return
            $@"{init_inclination_soul},{adjustment_value},{boundry_inclination_soul},{boundry_value},{cost_row},{max_level_for_game}";
    }
}
