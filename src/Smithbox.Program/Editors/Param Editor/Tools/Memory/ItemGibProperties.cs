using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;
public class ItemGibProperties
{
    public int Quantity = 1;
    public int ReinforceLvl = 0;
    public int GemId = -1;
    // DS3 only
    public int Durability = -1;

    public ItemGibProperties() { }

    public ItemGibProperties(ItemGibProperties other)
    {
        Quantity = other.Quantity;
        ReinforceLvl = other.ReinforceLvl;
        GemId = other.GemId;
        Durability = other.Durability;
    }
}