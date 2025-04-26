using System;

namespace StudioCore.Banks.GameOffsetBank;

public class GameOffsetBank
{
    public GameOffsetResource Offsets { get; set; }

    private string OffsetFileName = "";

    public GameOffsetBank()
    {
        OffsetFileName = "Offsets";
    }

    public void LoadBank()
    {
        try
        {
            Offsets = BankUtils.LoadGameOffsetJSON(OffsetFileName);
            TaskLogs.AddLog($"Banks: setup game offsets for Param Reloader.");
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Banks: failed to setup game offsets for Param Reloader: {e.Message}");
        }
    }
}
