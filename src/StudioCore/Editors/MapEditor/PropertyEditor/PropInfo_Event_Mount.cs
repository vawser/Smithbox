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

public static class PropInfo_Event_Mount
{
    public static void Display(Entity firstEnt)
    {
        if (firstEnt.IsEventMount())
        {
            var riderAlias = "None";
            var mountAlias = "None";

            string riderId = (string)PropFinderUtil.FindPropertyValue("RiderPartName", firstEnt.WrappedObject);

            var riderEnts = firstEnt.Container.Objects.Where(o => o.Name == riderId);

            if(riderEnts.Count() > 0)
            {
                Entity rider = firstEnt.Container.Objects.Where(o => o.Name == riderId).First();

                if (rider != null)
                {
                    var riderModel = rider.GetPropertyValue<string>("ModelName");

                    riderAlias = GetAliasFromCache(riderModel, ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                }
            }

            string mountId = (string)PropFinderUtil.FindPropertyValue("MountPartName", firstEnt.WrappedObject);
            var mountEnts = firstEnt.Container.Objects.Where(o => o.Name == mountId);

            if (mountEnts.Count() > 0)
            {
                Entity mount = firstEnt.Container.Objects.Where(o => o.Name == mountId).First();

                if (mount != null)
                {
                    var mountModel = mount.GetPropertyValue<string>("ModelName");

                    mountAlias = GetAliasFromCache(mountModel, ModelAliasBank.Bank.AliasNames.GetEntries("Characters"));
                }
            }

            ImGui.Text($"Rider:");
            AliasUtils.DisplayAlias(riderAlias);
            ImGui.Text($"Mount:");
            AliasUtils.DisplayAlias(mountAlias);
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
