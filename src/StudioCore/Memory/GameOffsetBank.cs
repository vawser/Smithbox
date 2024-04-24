using StudioCore.Banks.FormatBank;
using StudioCore.BanksMain;
using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Memory;

public class GameOffsetBank
{
    public GameOffsetContainer _Bank { get; set; }

    public bool IsBankLoading { get; set; }
    public bool CanReloadBank { get; set; }

    private bool IsGameSpecific;

    public GameOffsetBank()
    {
        CanReloadBank = false;
    }

    public GameOffsetResource Entries
    {
        get
        {
            if (IsBankLoading)
            {
                return new GameOffsetResource();
            }

            return _Bank.Data;
        }
    }

    public void ReloadBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Game Offsets - Load Game Offsets", TaskManager.RequeueType.None, false,
        () =>
        {
            _Bank = new GameOffsetContainer();
            IsBankLoading = true;

            if (Project.Type != ProjectType.Undefined)
            {
                try
                {
                    _Bank = new GameOffsetContainer();
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
