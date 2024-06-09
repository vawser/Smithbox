using ImGuiNET;
using StudioCore.BanksMain;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoulsFormats;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StudioCore.Banks.AliasBank;

namespace StudioCore.Editors.MapEditor.PropertyEditor;

public static class PropInfo_Event_ObjAct
{
    public static void Display(Entity firstEnt)
    {
        if (firstEnt.IsEventObjAct())
        {
            var assetAlias = "None";

            string assetId = (string)PropFinderUtil.FindPropertyValue("ObjActPartName", firstEnt.WrappedObject);

            var assetEnts = firstEnt.Container.Objects.Where(o => o.Name == assetId);

            if (assetEnts.Count() > 0)
            {
                Entity asset = firstEnt.Container.Objects.Where(o => o.Name == assetId).First();

                if (asset != null)
                {
                    var assetModel = asset.GetPropertyValue<string>("ModelName");

                    assetAlias = GetAliasFromCache(assetModel.ToLower(), ModelAliasBank.Bank.AliasNames.GetEntries("Objects"));
                }
            }
            ImGui.Text($"Asset:");
            AliasUtils.DisplayAlias(assetAlias);
            ImGui.Text("");
        }
    }

    public static string GetAliasFromCache(string name, List<AliasReference> referenceList)
    {
        foreach (var alias in referenceList)
        {
            if (name == alias.id)
            {
                return alias.name;
            }
        }

        return "";
    }
}
