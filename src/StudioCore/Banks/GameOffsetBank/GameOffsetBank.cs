using StudioCore.Banks.AliasBank;
using StudioCore.Editor;
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
            TaskLogs.AddLog($"Banks: setup game offsets for Param Reloader.");
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Banks: failed to setup game offsets for Param Reloader: {e.Message}");
        }
    }
}
