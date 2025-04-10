using Microsoft.Extensions.Logging;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static StudioCore.Editors.GparamEditor.Data.GparamParamBank;

namespace StudioCore.Editors.GparamEditor.Utils;

public static class GparamMemoryTools
{
    public static bool IsGparamReloaderSupported()
    {
        if(Smithbox.ProjectType is ProjectType.ER)
        {
            return true;
        }

        return false;
    }

    public static void ReloadCurrentGparam(GparamInfo info)
    {
        if (info == null)
            return;

        if (info.Gparam != null)
        {
            ReloadGparamInMemory(info);
        }
    }
    public static void ReloadAllGparam(List<GparamInfo> gparamList)
    {
        foreach (var entry in gparamList)
        {
            ReloadCurrentGparam(entry);
        }
    }

    public static void ReloadGparamInMemory(GparamInfo info)
    {
        var offsets_Name = "eldenring.exe";

        TaskManager.LiveTask task = new(
            "gparamReloader_ReloadGparams",
            "Gparam Reloader",
            "gparams have been reloaded in-game.",
            "gparams reload has failed.",
            TaskManager.RequeueType.WaitThenRequeue,
            true, () =>
            {
                Process[] processArray = Process.GetProcessesByName(offsets_Name);
                if (!processArray.Any())
                {
                    processArray = Process.GetProcessesByName(offsets_Name.Replace(".exe", ""));
                }

                if (processArray.Any())
                {
                    SoulsMemoryHandler memoryHandler = new(processArray.First());

                    HandleGparamReload(info, memoryHandler);
                    memoryHandler.Terminate();
                }
                else
                {
                    throw new Exception("Unable to find running game");
                }
            }
        );

        TaskManager.Run(task);
    }

    public static void HandleGparamReload(GparamInfo info, SoulsMemoryHandler handler)
    {
        var name = info.Name;

        // There are the bytes the GPARAM had when loaded initially
        var originalGparamBytes = info.Bytes;

        // These are the bytes of the GPARAM after any edits
        var currentGparamBytes = info.Gparam.Write();

        if(currentGparamBytes.Length != originalGparamBytes.Length)
        {
            TaskLogs.AddLog($"GPARAM structure has been modified. " +
                $"Original Length: {originalGparamBytes.Length}" +
                $"Write Length: {currentGparamBytes.Length}");
            return;
        }

        // Overwrite original bytes when reload is done so we can find the GPARAM again
        info.Bytes = currentGparamBytes;

        // Try and find match for the GPARAM
        if (!TryFindGparamOffsetFromAOB(handler, $"GparamPtr_{name}", originalGparamBytes, out var gparamBase))
        {
            TaskLogs.AddLog("Failed to find GPARAM offset", LogLevel.Error);
            return;
        }

        // Get GPARAM pointer
        nint gparamPtr = nint.Add(handler.GetBaseAddress(), gparamBase);

        // Write current bytes to memory
        handler.WriteProcessMemoryArray(gparamPtr, currentGparamBytes);
    }

    public static bool TryFindGparamOffsetFromAOB(SoulsMemoryHandler handler, string offsetName, byte[] aob, out int outOffset)
    {
        var memSize = handler.gameProcess.MainModule.ModuleMemorySize;
        var memFindLength = memSize - aob.Length;
        var mem = new byte[memSize];

        handler.ReadProcessMemory(handler.gameProcess.MainModule.BaseAddress, ref mem, memSize);

        for (var offset = 0; offset < memFindLength; offset++)
        {
            if (mem[offset] == aob[0])
            {
                var matched = true;
                for (var iPattern = 1; iPattern < aob.Length; iPattern++)
                {
                    if (mem[offset + iPattern] == aob[iPattern])
                    {
                        continue;
                    }

                    matched = false;
                    break;
                }

                if (matched)
                {
                    outOffset = offset;
                    //TaskLogs.AddLog($"Found AOB in memory for {offsetName}. Offset: 0x{offset:X2}");
                    return true;
                }
            }
        }

        TaskLogs.AddLog($"Unable to find AOB in memory for {offsetName}", LogLevel.Error);
        outOffset = -1;
        return false;
    }
}
