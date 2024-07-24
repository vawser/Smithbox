using StudioCore.Banks.AliasBank;
using StudioCore.Editor;
using StudioCore.Localization;
using StudioCore.Memory;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Banks.GameOffsetBank;

public class GameOffsetBank
{
    public GameOffsetResource Offsets { get; set; }

    private string OffsetDirectory = "";

    private string OffsetFileName = "";

    public GameOffsetBank()
    {
        OffsetDirectory = "GameOffsets";
        OffsetFileName = "Data";
    }

    public void LoadBank()
    {
        try
        {
            Offsets = BankUtils.LoadGameOffsetJSON(OffsetDirectory, OffsetFileName);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog(
                $"{LOC.Get("GAME_OFFSET_BANK__FAILED_TO_LOAD")}" +
                $"{e.Message}");
        }

        TaskLogs.AddLog($"{LOC.Get("GAME_OFFSET_BANK__SUCCESSFUL_LOAD")}");
    }
}
