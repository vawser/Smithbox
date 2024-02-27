using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.MapGroup;

public class MapGroupBank
{
    public MapGroupContainer _MapGroupBank { get; set; }

    public bool IsMapGroupBankLoaded { get; set; }
    public bool CanReloadMapGroupBank { get; set; }

    private string FormatInfoName = "";

    private bool IsGameSpecific;

    public MapGroupBank()
    {
        CanReloadMapGroupBank = false;
    }

    public MapGroupResource Entries
    {
        get
        {
            if (IsMapGroupBankLoaded)
            {
                return new MapGroupResource();
            }

            return _MapGroupBank.Data;
        }
    }

    public void ReloadMapGroupBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Map Groups - Load", TaskManager.RequeueType.None, false,
        () =>
        {
            _MapGroupBank = new MapGroupContainer();
            IsMapGroupBankLoaded = true;

            if (Project.Type != ProjectType.Undefined)
            {
                try
                {
                    _MapGroupBank = new MapGroupContainer();
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                }

                IsMapGroupBankLoaded = false;
            }
            else
                IsMapGroupBankLoaded = false;
        }));
    }
}
