using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Tools.Randomiser;

public static class RandomiserUtils
{
    public static List<string> GetRowList(string input)
    {
        var pattern = @"(\[.*?\])";
        var matches = Regex.Match(input, pattern);

        var resultList = new List<string>();

        foreach (var group in matches.Groups)
        {
            var res = group.ToString();
            res = res.Replace("[", "");
            res = res.Replace("]", "");

            if (res.Contains(","))
            {
                var values = res.Split(",");

                if (Regex.IsMatch(values[0], @"^\d+$") && Regex.IsMatch(values[1], @"^\d+$"))
                {
                    var startVal = int.Parse(values[0]);
                    var endVal = int.Parse(values[1]);
                    foreach (var val in Enumerable.Range(startVal, (endVal - startVal)))
                    {
                        resultList.Add($"{val}");
                    }
                }
                else
                {
                    TaskLogs.AddLog($"Invalid value in input string: {input}");
                }
            }
            else
            {
                resultList.Add(res);
            }
        }

        return resultList;
    }

    public static List<string> GetIdList(string input)
    {
        if (input.Contains(","))
        {
            return input.Split(",").ToList();
        }
        else
        {
            return new List<string> { input };
        }
    }
}
