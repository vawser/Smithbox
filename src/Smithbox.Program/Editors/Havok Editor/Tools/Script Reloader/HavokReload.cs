using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

// Credit to Meowmaritus for ER reload elements
public static class HavokReload
{
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
                int gameVersion = fileInfo.FileMajorPart * 1_00_00_00
                                  + fileInfo.FileMinorPart * 1_00_00
                                  + fileInfo.FileBuildPart * 1_00
                                  + fileInfo.FilePrivatePart;

                var chrReload = Kernel32.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, 256, 0x1000 | 0x2000, 0x40);
                var chrReload_DataSetup = Kernel32.VirtualAllocEx(Memory.ProcessHandle, IntPtr.Zero, 256, 0x1000 | 0x2000, 0x40);

                if (chrReload != IntPtr.Zero && chrReload_DataSetup != IntPtr.Zero)
                {
                    try
                    {
                        Memory.UpdateEldenRingAobs();

                        var worldChrManStrucOffset = HavokMemoryConsts.GetHexFromString(HavokMemoryConsts.EldenRing_WorldChrManStructOffset);
                        var crashOffsetWriteBytes = HavokMemoryConsts.GetByteArrayFromString(HavokMemoryConsts.EldenRing_CrashPatchOffset_WriteBytes);

                        var dataPointer = Memory.ReadInt64((IntPtr)Memory.ReadInt64(Memory.EldenRing_WorldChrManPtr + (worldChrManStrucOffset ?? 0x1E668)) + 0x0);

                        Memory.WriteInt64(chrReload_DataSetup + 0x8, dataPointer); // Pointer to data
                        Memory.WriteInt64(chrReload_DataSetup + 0x58, (chrReload_DataSetup + 0x100).ToInt64()); // Pointer to string
                        Memory.WriteInt8(chrReload_DataSetup + 0x70, 0x1F); // String length
                        Memory.WriteBytes(chrReload_DataSetup + 0x100, chrNameBytes);

                        // Crash fix offset, last updated for 1.05
                        var writeBytes = crashOffsetWriteBytes ?? new byte[] { 0x48, 0x31, 0xD2 };
                        Memory.WriteBytes(Memory.EldenRing_CrashFixPtr, writeBytes);

                        byte[] buffer = null;

                        if (gameVersion >= 01_07_00_00)
                        {
                            buffer = new byte[] {
                                0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx,0000000000000000 (ChrReload_DataSetup)
								0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rcx,0000000000000000 (WorldChrMan)
								0x48, 0x8B, 0x91, 0x68, 0xE6, 0x01, 0x00, // mov rdx,[rcx+0001E668]
								0x48, 0x89, 0x1A, // mov [rdx],rbx
								0x48, 0x89, 0x13, // mov [rbx],rdx
								0x48, 0x8B, 0x91, 0x68, 0xE6, 0x01, 0x00, // mov rdx,[rcx+0001E668]
								0x48, 0x89, 0x5A, 0x08, // mov [rdx+08],rbx
								0x48, 0x89, 0x53, 0x08, // mov [rbx+08],rdx
								0xC7, 0x81, 0x70, 0xE6, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, // mov [rcx+0001E670],00000001 { 1 }
								0xC7, 0x81, 0x78, 0xE6, 0x01, 0x00, 0x00, 0x00, 0x20, 0x41, // mov [rcx+0001E678],41200000 { 10.00 }
								0xC3, // ret 
							};
                        }
                        else
                        {
                            buffer = new byte[] {
                                0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rbx,0000000000000000 (ChrReload_DataSetup)
								0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rcx,0000000000000000 (WorldChrMan)
								0x48, 0x8B, 0x91, 0xC0, 0x85, 0x01, 0x00, // mov rdx,[rcx+000185C0]
								0x48, 0x89, 0x1A, // mov [rdx],rbx
								0x48, 0x89, 0x13, // mov [rbx],rdx
								0x48, 0x8B, 0x91, 0xC0, 0x85, 0x01, 0x00, // mov rdx,[rcx+000185C0]
								0x48, 0x89, 0x5A, 0x08, // mov [rdx+08],rbx
								0x48, 0x89, 0x53, 0x08, // mov [rbx+08],rdx
								0xC7, 0x81, 0xC8, 0x85, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, // mov [rcx+000185C8],00000001 { 1 }
								0xC7, 0x81, 0xD0, 0x85, 0x01, 0x00, 0x00, 0x00, 0x20, 0x41, // mov [rcx+000185D0],41200000 { 10.00 }
								0xC3, // ret 
							};
                        }

                        Array.Copy(BitConverter.GetBytes(chrReload_DataSetup.ToInt64()), 0, buffer, 0x2, 0x8);
                        Array.Copy(BitConverter.GetBytes(Memory.EldenRing_WorldChrManPtr.ToInt64()), 0, buffer, 0xC, 0x8);


                        Memory.WriteBytes(chrReload, buffer);

                        var threadHandle = Kernel32.CreateRemoteThread(Memory.ProcessHandle, IntPtr.Zero, 0, chrReload, IntPtr.Zero, 0, out var threadId);
                        if (threadHandle != IntPtr.Zero)
                        {
                            Kernel32.WaitForSingleObject(threadHandle, 30000);
                        }

                        return true;
                    }
                    finally
                    {
                        Kernel32.VirtualFreeEx(Memory.ProcessHandle, chrReload, 256, 2);
                        Kernel32.VirtualFreeEx(Memory.ProcessHandle, chrReload_DataSetup, 256, 2);
                    }
                }
            }
            else
            {
                Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_ScriptReloader_Error_Failed_Game_Process"));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_ScriptReloader_Error_Failed_Hot_Reload"), e);
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
                    Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_ScriptReloader_Log_Reload_for_Old_DS3"));
                }
            }
            else
            {
                Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_ScriptReloader_Error_Failed_Game_Process"));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(HavokReload), LOC.Get("HAVOK_ScriptReloader_Error_Failed_Hot_Reload"), e);
        }

        return false;
    }
}