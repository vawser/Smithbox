using StudioCore.Banks.AliasBank;
using StudioCore.Interface;
using System.Collections.Generic;

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
