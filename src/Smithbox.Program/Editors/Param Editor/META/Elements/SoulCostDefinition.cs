using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

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
