using StudioCore.Banks.AliasBank;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Framework;

public class HavokDisplayUtils
{
    public static void DisplaySelectableAlias(string name, Dictionary<string, AliasReference> referenceDict)
    {
        var lowerName = name.ToLower();

        if (referenceDict.ContainsKey(lowerName))
        {
            var aliasName = referenceDict[lowerName].name;

            UIHelper.DisplayAlias(aliasName);
        }
    }
}
