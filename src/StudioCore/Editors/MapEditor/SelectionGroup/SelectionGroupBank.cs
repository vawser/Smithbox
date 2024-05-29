using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.MapGroup;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.SelectionGroup;

public static class SelectionGroupBank
{
    public static SelectionGroupContainer Bank { get; set; }

    public static bool IsLoaded { get; set; }
    public static bool IsBankLoading { get; set; }
    public static bool CanReloadBank { get; set; }

    public static List<SelectionGroupResource> Entries
    {
        get
        {
            if (CanReloadBank)
            {
                return new SelectionGroupList().Resources;
            }

            return Bank.Data.Resources;
        }
    }

    public static void ReloadBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Selection Groups - Load", TaskManager.RequeueType.None, false,
        () =>
        {
            Bank = new SelectionGroupContainer();
            IsBankLoading = true;

            if (Project.Type != ProjectType.Undefined)
            {
                try
                {
                    Bank = new SelectionGroupContainer();
                    IsLoaded = true;
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                }

                IsBankLoading = false;
            }
            else
                IsBankLoading = false;
        }));
    }
}
