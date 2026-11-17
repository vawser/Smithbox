using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public static class HavokReload
{
    private const uint MEM_RELEASE = 0x8000;
    private const uint WAIT_OBJECT_0 = 0;

    public static bool RequestReloadChr(ProjectEntry project, string chrName)
    {
        if (project.Descriptor.ProjectType is ProjectType.ER)
        {
            return ReloadChr_ER(project, chrName);
        }
        else if (project.Descriptor.ProjectType is ProjectType.DS3)
        {
            return ReloadChr_DS3(project, chrName);
        }

        return false;
    }

    public static bool ReloadChr_ER(ProjectEntry project, string chrName)
    {
        byte[] chrNameBytes = Encoding.Unicode.GetBytes(chrName);

        try
        {
            Memory.AttachProc(project, "eldenring");
            if (Memory.ProcessHandle == IntPtr.Zero)
                Memory.AttachProc(project, "start_protected_game");

            if (Memory.ProcessHandle != IntPtr.Zero)
            {
                var fileInfo = Memory.AttachedProcess.MainModule.FileVersionInfo;
                var exeVersion = $"{fileInfo.FileMajorPart}.{fileInfo.FileMinorPart}.{fileInfo.FileBuildPart}.{fileInfo.FilePrivatePart}";

                var exactMatch = false;
                var offsets = project.Handler.HavokData?.CharacterReloaderOffsets?.GetGroup(exeVersion, out exactMatch);

                if (offsets == null)
                {
                    Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Error_No_Offsets"));
                    return false;
                }

                if (!exactMatch)
                {
                    Smithbox.LogWarning(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Warning_Version_Not_Listed", exeVersion, offsets.exeVersion));
                }

                var listOffset = HavokCharacterReloaderOffsets.ParseHex(offsets.reloadListOffset);
                var countOffset = HavokCharacterReloaderOffsets.ParseHex(offsets.reloadCountOffset);
                var timerOffset = HavokCharacterReloaderOffsets.ParseHex(offsets.reloadTimerOffset);
                var writeBytes = HavokCharacterReloaderOffsets.ParseBytes(offsets.crashPatchBytes);

                if (listOffset == null || countOffset == null || timerOffset == null || writeBytes == null)
                {
                    Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Error_Invalid_Offsets", offsets.exeVersion));
                    return false;
                }

                Memory.UpdateEldenRingAobs(offsets);

                if (Memory.EldenRing_WorldChrManPtr == IntPtr.Zero)
                {
                    Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Error_WorldChrMan_Not_Found"));
                    return false;
                }

                if (Memory.EldenRing_CrashFixPtr == IntPtr.Zero)
                {
                    Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Error_Crash_Patch_Not_Found"));
                    return false;
                }

                var chrReload = Kernel32.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, 256, 0x1000 | 0x2000, 0x40);
                var chrReload_DataSetup = Kernel32.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, 256, 0x1000 | 0x2000, 0x40);

                if (chrReload != IntPtr.Zero && chrReload_DataSetup != IntPtr.Zero)
                {
                    // Set once the remote thread has finished, or if it was never started
                    var threadFinished = true;

                    // Set once the request node is in the game's reload list
                    var nodeQueued = false;

                    try
                    {
                        // Request node (a list node): next +0x0, prev +0x8 (both set by the code below),
                        // name string +0x58 (pointer; capacity at +0x70 must be 8..0x1F, so the pointer is used).
                        // The game compares the name as a null-terminated wide string; the allocation is zeroed.
                        Memory.WriteInt64(chrReload_DataSetup + 0x58, (chrReload_DataSetup + 0x100).ToInt64()); // Pointer to string
                        Memory.WriteInt8(chrReload_DataSetup + 0x70, 0x1F); // String capacity
                        Memory.WriteBytes(chrReload_DataSetup + 0x100, chrNameBytes);

                        // Once the reload is done the game frees the node through its own allocator; the node is
                        // ours, so this patch makes it free null instead (mov rdx,rbx -> xor rdx,rdx)
                        Memory.WriteBytes(Memory.EldenRing_CrashFixPtr, writeBytes);

                        // Appends the node to WorldChrMan's reload list (std::list push_back), so requests for
                        // several characters queue up, then bumps the request count and sets the reload timer.
                        // The zeroed operands are filled in below.
                        byte[] buffer = new byte[] {
                            0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx,node
                            0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rcx,WorldChrMan
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
                        Array.Copy(BitConverter.GetBytes(Memory.EldenRing_WorldChrManPtr.ToInt64()), 0, buffer, 12, 8);
                        Array.Copy(BitConverter.GetBytes(listOffset.Value), 0, buffer, 23, 4);
                        Array.Copy(BitConverter.GetBytes(countOffset.Value), 0, buffer, 48, 4);
                        Array.Copy(BitConverter.GetBytes(timerOffset.Value), 0, buffer, 54, 4);

                        Memory.WriteBytes(chrReload, buffer);

                        var threadHandle = Kernel32.CreateRemoteThread(Memory.ProcessHandle, IntPtr.Zero, 0, chrReload, IntPtr.Zero, 0, out var threadId);
                        if (threadHandle != IntPtr.Zero)
                        {
                            nodeQueued = true;
                            threadFinished = Kernel32.WaitForSingleObject(threadHandle, 30000) == WAIT_OBJECT_0;
                            Kernel32.CloseHandle(threadHandle);
                        }
                        else
                        {
                            // The code never ran, so nothing in the game refers to the buffers
                            threadFinished = true;
                        }

                        return nodeQueued;
                    }
                    finally
                    {
                        // The code buffer can go once the thread is done with it (a timed-out thread may still
                        // be running it). MEM_RELEASE needs a size of 0.
                        if (threadFinished)
                        {
                            Kernel32.VirtualFreeEx(Memory.ProcessHandle, chrReload, 0, MEM_RELEASE);
                        }

                        // The node is only read by the game when the reload timer runs out, well after the thread
                        // returns, and nothing tells us when it is done with it, so a queued node is left allocated
                        // (one page per reload). Freeing it here crashes the game.
                        if (!nodeQueued)
                        {
                            Kernel32.VirtualFreeEx(Memory.ProcessHandle, chrReload_DataSetup, 0, MEM_RELEASE);
                        }
                    }
                }
            }
            else
            {
                Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Error_Failed_Game_Process"));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Error_Failed_Hot_Reload"), e);
        }

        return false;
    }

    public static bool ReloadChr_DS3(ProjectEntry project, string chrName)
    {
        byte[] chrNameBytes = Encoding.Unicode.GetBytes(chrName);

        try
        {
            Memory.AttachProc(project, "DarkSoulsIII");

            if (Memory.ProcessHandle != IntPtr.Zero)
            {
                var fileInfo = Memory.AttachedProcess.MainModule.FileVersionInfo;
                int gameVersion = fileInfo.FileMajorPart * 1_00_00_00
                                  + fileInfo.FileMinorPart * 1_00_00
                                  + fileInfo.FileBuildPart * 1_00
                                  + fileInfo.FilePrivatePart;

                if (gameVersion == 1150000)
                {
                    Memory.WriteBoolean(Memory.BaseAddress + 0x4768F7F, true);

                    byte[] buffer =
                    {
                        0x48, 0xBA, 0, 0, 0, 0, 0, 0, 0, 0, //mov rdx,Alloc
                        0x48, 0xA1, 0x78, 0x8E, 0x76, 0x44, 0x01, 0x00, 0x00, 0x00, //mov rax,[144768E78]
                        0x48, 0x8B, 0xC8, //mov rcx,rax
                        0x49, 0xBE, 0x10, 0x1E, 0x8D, 0x40, 0x01, 0x00, 0x00, 0x00, //mov r14,00000001408D1E10
                        0x48, 0x83, 0xEC, 0x28, //sub rsp,28
                        0x41, 0xFF, 0xD6, //call r14
                        0x48, 0x83, 0xC4, 0x28, //add rsp,28
                        0xC3 //ret
                    };

                    Memory.ExecuteBufferFunction(buffer, chrNameBytes);
                }
                else
                {
                    Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Log_Reload_for_Old_DS3"));
                }
            }
            else
            {
                Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Error_Failed_Game_Process"));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_CharacterReloader_Error_Failed_Hot_Reload"), e);
        }

        return false;
    }
}