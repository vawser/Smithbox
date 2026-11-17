using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.Common;

/// <summary>
/// Requests a character reload in the running game. What to do comes from the project's Character Reloader offsets
/// (Assets/Character Reloader/{game}/*.json), not from the game type: see <see cref="CharacterReloaderOffsetEntry"/>.
/// </summary>
public static class CharacterReload
{
    private const uint MEM_RELEASE = 0x8000;
    private const uint WAIT_OBJECT_0 = 0;

    /// <summary>
    /// Addresses resolved from an offsets group's AOBs, for one game process
    /// </summary>
    private class ResolvedAddresses
    {
        public int ProcessId;
        public CharacterReloaderOffsetEntry Offsets;

        public IntPtr WorldChrManGlobal;
        public IntPtr RequestFunction;
        public IntPtr PlayerReloadFlag;
        public IntPtr CrashPatch;
    }

    // The AOB scan reads the game's whole executable image, so it is only done once per process
    private static ResolvedAddresses Resolved;

    /// <summary>
    /// True if the project's game has Character Reloader offsets
    /// </summary>
    public static bool IsSupported(ProjectEntry project)
    {
        return project.Handler.CommonData?.CharacterReloaderOffsets?.Groups.Count > 0;
    }

    public static bool RequestReloadChr(ProjectEntry project, string chrName)
    {
        var store = project.Handler.CommonData?.CharacterReloaderOffsets;

        if (store == null || store.Groups.Count == 0)
        {
            Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_No_Offsets"));
            return false;
        }

        try
        {
            var exeName = store.Groups.Select(e => e.exeName).FirstOrDefault(e => !string.IsNullOrWhiteSpace(e));
            var procName = Path.GetFileNameWithoutExtension(exeName ?? "");

            if (string.IsNullOrEmpty(procName) || !CharacterReloadMemory.AttachProc(procName))
            {
                Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_Failed_Game_Process"));
                return false;
            }

            var resolved = Resolve(store);
            if (resolved == null)
                return false;

            var offsets = resolved.Offsets;

            // WorldChrMan only exists once the game has loaded into the world
            if (CharacterReloadKernel32.ReadIntPtr(CharacterReloadMemory.ProcessHandle, resolved.WorldChrManGlobal, true) == IntPtr.Zero)
            {
                Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_WorldChrMan_Not_Loaded"));
                return false;
            }

            if (string.Equals(offsets.method, "Call", StringComparison.OrdinalIgnoreCase))
            {
                return ReloadByCall(resolved, chrName);
            }

            if (string.Equals(offsets.method, "Queue", StringComparison.OrdinalIgnoreCase))
            {
                return ReloadByQueue(resolved, chrName);
            }

            Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_Invalid_Offsets", offsets.exeVersion));
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_Failed_Hot_Reload"), e);
        }

        return false;
    }

    /// <summary>
    /// Pick the offsets group for the attached game's version and resolve its AOBs (cached per process).
    /// Null, with the error logged, if something needed isn't found.
    /// </summary>
    private static ResolvedAddresses Resolve(CharacterReloaderOffsets store)
    {
        var process = CharacterReloadMemory.AttachedProcess;

        if (Resolved != null && Resolved.ProcessId == process.Id)
            return Resolved;

        Resolved = null;

        var fileInfo = process.MainModule.FileVersionInfo;
        var exeVersion = $"{fileInfo.FileMajorPart}.{fileInfo.FileMinorPart}.{fileInfo.FileBuildPart}.{fileInfo.FilePrivatePart}";

        var offsets = store.GetGroup(exeVersion, out var exactMatch);

        if (!exactMatch)
        {
            Smithbox.LogWarning(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Warning_Version_Not_Listed", exeVersion, offsets.exeVersion));
        }

        var scanner = new CharacterReloadAobScanner(process);
        var isCall = string.Equals(offsets.method, "Call", StringComparison.OrdinalIgnoreCase);

        var result = new ResolvedAddresses
        {
            ProcessId = process.Id,
            Offsets = offsets,
            WorldChrManGlobal = CharacterReloadMemory.ResolveRelative(scanner, offsets.worldChrManAob, offsets.worldChrManAobRelativeOffset)
        };

        if (result.WorldChrManGlobal == IntPtr.Zero)
        {
            Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_WorldChrMan_Not_Found"));
            return null;
        }

        if (isCall)
        {
            var functionAob = string.IsNullOrWhiteSpace(offsets.requestFunctionAob) ? offsets.worldChrManAob : offsets.requestFunctionAob;
            result.RequestFunction = CharacterReloadMemory.ResolveRelative(scanner, functionAob, offsets.requestFunctionAobRelativeOffset);

            if (result.RequestFunction == IntPtr.Zero)
            {
                Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_Request_Function_Not_Found"));
                return null;
            }

            // Optional: without it, player characters may not reload, but others still do
            if (!string.IsNullOrWhiteSpace(offsets.playerReloadFlagAob))
            {
                result.PlayerReloadFlag = CharacterReloadMemory.ResolveRelative(scanner, offsets.playerReloadFlagAob, offsets.playerReloadFlagAobRelativeOffset);

                if (result.PlayerReloadFlag == IntPtr.Zero)
                {
                    Smithbox.LogWarning(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Warning_Player_Flag_Not_Found"));
                }
            }
        }
        else
        {
            result.CrashPatch = CharacterReloadMemory.ResolveFromEnd(scanner, offsets.crashPatchAob, offsets.crashPatchDistFromEndOfAob);

            if (result.CrashPatch == IntPtr.Zero)
            {
                Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_Crash_Patch_Not_Found"));
                return null;
            }
        }

        // Diagnostics: the addresses, to compare against the analysis when a game update breaks the reload
        Smithbox.Log(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Log_Resolved",
            offsets.exeVersion,
            ToHex(result.WorldChrManGlobal),
            ToHex(result.RequestFunction),
            ToHex(result.PlayerReloadFlag),
            ToHex(result.CrashPatch)));

        Resolved = result;
        return result;
    }

    private static string ToHex(IntPtr address) => address == IntPtr.Zero ? "-" : $"{address.ToInt64():X}";

    /// <summary>
    /// Call the game's RequestReloadChr(WorldChrMan, name). The game copies the name into its own request,
    /// so both buffers are freed once the call returns.
    /// </summary>
    private static bool ReloadByCall(ResolvedAddresses resolved, string chrName)
    {
        var handle = CharacterReloadMemory.ProcessHandle;

        // Code at +0x0, the null-terminated name at +0x100 (the allocation is zeroed)
        var buffer = CharacterReloadKernel32.VirtualAllocEx(handle, IntPtr.Zero, 0x1000, 0x1000 | 0x2000, 0x40);
        if (buffer == IntPtr.Zero)
            return false;

        var threadFinished = true;

        try
        {
            var namePtr = buffer + 0x100;
            CharacterReloadMemory.WriteBytes(namePtr, Encoding.Unicode.GetBytes(chrName));

            byte[] code = new byte[] {
                0x53, // push rbx
                0x56, // push rsi
                0x48, 0x83, 0xEC, 0x28, // sub rsp,28h (16-byte aligned stack for the call)
                0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx,PlayerReloadFlag (0 if none)
                0x48, 0x85, 0xDB, // test rbx,rbx
                0x74, 0x06, // je +6
                0x40, 0x8A, 0x33, // mov sil,[rbx] (keep the flag)
                0xC6, 0x03, 0x01, // mov byte ptr [rbx],1
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rax,WorldChrManGlobal
                0x48, 0x8B, 0x08, // mov rcx,[rax]
                0x48, 0xBA, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rdx,name
                0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rax,RequestReloadChr
                0xFF, 0xD0, // call rax
                0x48, 0x85, 0xDB, // test rbx,rbx
                0x74, 0x03, // je +3
                0x40, 0x88, 0x33, // mov [rbx],sil (restore the flag)
                0x48, 0x83, 0xC4, 0x28, // add rsp,28h
                0x5E, // pop rsi
                0x5B, // pop rbx
                0xC3, // ret
            };

            Array.Copy(BitConverter.GetBytes(resolved.PlayerReloadFlag.ToInt64()), 0, code, 8, 8);
            Array.Copy(BitConverter.GetBytes(resolved.WorldChrManGlobal.ToInt64()), 0, code, 29, 8);
            Array.Copy(BitConverter.GetBytes(namePtr.ToInt64()), 0, code, 42, 8);
            Array.Copy(BitConverter.GetBytes(resolved.RequestFunction.ToInt64()), 0, code, 52, 8);

            CharacterReloadMemory.WriteBytes(buffer, code);

            var threadHandle = CharacterReloadKernel32.CreateRemoteThread(handle, IntPtr.Zero, 0, buffer, IntPtr.Zero, 0, out _);
            if (threadHandle == IntPtr.Zero)
                return false;

            threadFinished = CharacterReloadKernel32.WaitForSingleObject(threadHandle, 30000) == WAIT_OBJECT_0;
            CharacterReloadKernel32.CloseHandle(threadHandle);

            return threadFinished;
        }
        finally
        {
            // A timed-out thread may still be running the code, so its buffer is left allocated.
            // MEM_RELEASE needs a size of 0.
            if (threadFinished)
            {
                CharacterReloadKernel32.VirtualFreeEx(handle, buffer, 0, MEM_RELEASE);
            }
        }
    }

    /// <summary>
    /// Append a request node to WorldChrMan's reload list, for games where RequestReloadChr was stripped.
    /// </summary>
    private static bool ReloadByQueue(ResolvedAddresses resolved, string chrName)
    {
        var offsets = resolved.Offsets;
        var handle = CharacterReloadMemory.ProcessHandle;

        var listOffset = CharacterReloaderOffsets.ParseHex(offsets.reloadListOffset);
        var countOffset = CharacterReloaderOffsets.ParseHex(offsets.reloadCountOffset);
        var timerOffset = CharacterReloaderOffsets.ParseHex(offsets.reloadTimerOffset);
        var writeBytes = CharacterReloaderOffsets.ParseBytes(offsets.crashPatchBytes);

        if (listOffset == null || countOffset == null || timerOffset == null || writeBytes == null)
        {
            Smithbox.LogError(typeof(CharacterReload), LOC.Get("COMMON_CharacterReloader_Error_Invalid_Offsets", offsets.exeVersion));
            return false;
        }

        var chrReload = CharacterReloadKernel32.VirtualAllocEx(handle, IntPtr.Zero, 256, 0x1000 | 0x2000, 0x40);
        var chrReload_DataSetup = CharacterReloadKernel32.VirtualAllocEx(handle, IntPtr.Zero, 256, 0x1000 | 0x2000, 0x40);

        if (chrReload == IntPtr.Zero || chrReload_DataSetup == IntPtr.Zero)
        {
            if (chrReload != IntPtr.Zero) CharacterReloadKernel32.VirtualFreeEx(handle, chrReload, 0, MEM_RELEASE);
            if (chrReload_DataSetup != IntPtr.Zero) CharacterReloadKernel32.VirtualFreeEx(handle, chrReload_DataSetup, 0, MEM_RELEASE);
            return false;
        }

        // Set once the remote thread has finished, or if it was never started
        var threadFinished = true;

        // Set once the request node is in the game's reload list
        var nodeQueued = false;

        try
        {
            // Request node (a list node): next +0x0, prev +0x8 (both set by the code below),
            // name string +0x58 (pointer; capacity at +0x70 must be 8..0x1F, so the pointer is used).
            // The game compares the name as a null-terminated wide string; the allocation is zeroed.
            CharacterReloadMemory.WriteInt64(chrReload_DataSetup + 0x58, (chrReload_DataSetup + 0x100).ToInt64()); // Pointer to string
            CharacterReloadMemory.WriteInt8(chrReload_DataSetup + 0x70, 0x1F); // String capacity
            CharacterReloadMemory.WriteBytes(chrReload_DataSetup + 0x100, Encoding.Unicode.GetBytes(chrName));

            // Once the reload is done the game frees the node through its own allocator; the node is
            // ours, so this patch makes it free null instead (mov rdx,rbx -> xor rdx,rdx)
            CharacterReloadMemory.WriteBytes(resolved.CrashPatch, writeBytes);

            // Appends the node to WorldChrMan's reload list (std::list push_back), so requests for
            // several characters queue up, then bumps the request count and sets the reload timer.
            // The zeroed operands are filled in below.
            byte[] buffer = new byte[] {
                0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx,node
                0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rcx,WorldChrManGlobal
                0x48, 0x8B, 0x09, // mov rcx,[rcx] (WorldChrMan)
                0x48, 0x8B, 0x91, 0x00, 0x00, 0x00, 0x00, // mov rdx,[rcx+Offset1] (list sentinel)
                0x48, 0x8B, 0x42, 0x08, // mov rax,[rdx+08] (last node)
                0x48, 0x89, 0x13, // mov [rbx],rdx (node.next = sentinel)
                0x48, 0x89, 0x43, 0x08, // mov [rbx+08],rax (node.prev = last)
                0x48, 0x89, 0x18, // mov [rax],rbx (last.next = node)
                0x48, 0x89, 0x5A, 0x08, // mov [rdx+08],rbx (sentinel.prev = node)
                0x48, 0xFF, 0x81, 0x00, 0x00, 0x00, 0x00, // inc qword ptr [rcx+Offset2] (count)
                0xC7, 0x81, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20, 0x41, // mov [rcx+Offset3],41200000 { 10.00 } (timer)
                0xC3, // ret
            };

            Array.Copy(BitConverter.GetBytes(chrReload_DataSetup.ToInt64()), 0, buffer, 2, 8);
            Array.Copy(BitConverter.GetBytes(resolved.WorldChrManGlobal.ToInt64()), 0, buffer, 12, 8);
            Array.Copy(BitConverter.GetBytes(listOffset.Value), 0, buffer, 26, 4);
            Array.Copy(BitConverter.GetBytes(countOffset.Value), 0, buffer, 51, 4);
            Array.Copy(BitConverter.GetBytes(timerOffset.Value), 0, buffer, 57, 4);

            CharacterReloadMemory.WriteBytes(chrReload, buffer);

            var threadHandle = CharacterReloadKernel32.CreateRemoteThread(handle, IntPtr.Zero, 0, chrReload, IntPtr.Zero, 0, out _);
            if (threadHandle != IntPtr.Zero)
            {
                nodeQueued = true;
                threadFinished = CharacterReloadKernel32.WaitForSingleObject(threadHandle, 30000) == WAIT_OBJECT_0;
                CharacterReloadKernel32.CloseHandle(threadHandle);
            }

            return nodeQueued;
        }
        finally
        {
            // The code buffer can go once the thread is done with it (a timed-out thread may still
            // be running it). MEM_RELEASE needs a size of 0.
            if (threadFinished)
            {
                CharacterReloadKernel32.VirtualFreeEx(handle, chrReload, 0, MEM_RELEASE);
            }

            // The node is only read by the game when the reload timer runs out, well after the thread
            // returns, and nothing tells us when it is done with it, so a queued node is left allocated
            // (one page per reload). Freeing it here crashes the game.
            if (!nodeQueued)
            {
                CharacterReloadKernel32.VirtualFreeEx(handle, chrReload_DataSetup, 0, MEM_RELEASE);
            }
        }
    }
}
