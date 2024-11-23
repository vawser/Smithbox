using StudioCore.Core;
using StudioCore.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class ParamMemoryTools
{
    public static bool IsParamReloaderSupported()
    {
        return ParamReloader.GameIsSupported(Smithbox.ProjectType);
    }

    public static void ReloadCurrentParam()
    {
        var canHotReload = ParamReloader.CanReloadMemoryParams(ParamBank.PrimaryBank);

        if (canHotReload)
        {
            if(Smithbox.EditorHandler.ParamEditor._activeView._selection.GetActiveParam() != null)
            {
                ParamReloader.ReloadMemoryParam(ParamBank.PrimaryBank, Smithbox.EditorHandler.ParamEditor._activeView._selection.GetActiveParam());
            }
            else
            {
                TaskLogs.AddLog("No param has been selected yet for the Param Reloder.");
            }
        }
        else
        {
            TaskLogs.AddLog("Param Reloader cannot reload for this project.");
        }
    }

    public static void ReloadAllParams()
    {
        var canHotReload = ParamReloader.CanReloadMemoryParams(ParamBank.PrimaryBank);

        if (canHotReload)
        {
            ParamReloader.ReloadMemoryParams(ParamBank.PrimaryBank, ParamBank.PrimaryBank.Params.Keys.ToArray());
        }
        else
        {
            TaskLogs.AddLog("Param Reloader cannot reload for this project.");
        }
    }

    public static int SpawnedItemAmount = 1;
    public static int SpawnWeaponLevel = 0;

    public static void GiveItem()
    {
        var activeParam = Smithbox.EditorHandler.ParamEditor._activeView._selection.GetActiveParam();
        if (activeParam != null)
        {
            GameOffsetsEntry offsets = ParamReloader.GetGameOffsets();

            var rowsToGib = Smithbox.EditorHandler.ParamEditor._activeView._selection.GetSelectedRows();
            var param = Smithbox.EditorHandler.ParamEditor._activeView._selection.GetActiveParam();

            if (activeParam is "EquipParamGoods" or "EquipParamProtector" or "EquipParamAccessory")
            {
                ParamReloader.GiveItem(offsets, rowsToGib, param, SpawnedItemAmount);
            }
            if (activeParam == "EquipParamWeapon")
            {
                ParamReloader.GiveItem(offsets, rowsToGib, param, SpawnedItemAmount, SpawnWeaponLevel);
            }
        }
        else
        {
            TaskLogs.AddLog("No param selected yet for Item Gib.");
        }
    }
}
