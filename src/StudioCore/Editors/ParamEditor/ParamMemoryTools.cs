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
                TaskLogs.AddLog("Param Reloader: No param has been selected.");
            }
        }
        else
        {
            TaskLogs.AddLog("Param Reloader: Cannot reload.");
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
            TaskLogs.AddLog("Param Reloader: Cannot reload.");
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

            if (activeParam == "EquipParamGoods")
            {
                ParamReloader.GiveItem(offsets, rowsToGib, param, SpawnedItemAmount);
            }
            if (activeParam == "EquipParamWeapon")
            {
                ParamReloader.GiveItem(offsets, rowsToGib, param, SpawnWeaponLevel);
            }
        }
        else
        {
            TaskLogs.AddLog("Item Gib: no param selected.");
        }
    }
}
