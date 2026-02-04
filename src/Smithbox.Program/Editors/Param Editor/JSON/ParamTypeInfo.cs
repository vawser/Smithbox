using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamTypeInfo
{
    /// <summary>
    /// Filename : Param Type string
    /// </summary>
    public Dictionary<string, string> Mapping { get; set; }

    /// <summary>
    /// This is for params that need skip the !defs.ContainsKey(curParam.ParamType) check (e.g. EquipParamWeapon_Npc)
    /// </summary>
    public List<string> Exceptions { get; set; }
}