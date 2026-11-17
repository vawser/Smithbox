using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.Common;

public static class CharacterReloadMemory
{
    public enum Startbit : byte
    {
        Bit0 = 0,
        Bit1 = 1,
        Bit2 = 2,
        Bit3 = 3,
        Bit4 = 4,
        Bit5 = 5,
        Bit6 = 6,
        Bit7 = 7
    }

    // Memory
    public static bool Is64Bit => IntPtr.Size == 8;

    public static IntPtr ProcessHandle { get; set; }

    public static IntPtr BaseAddress { get; set; }

    public static System.Diagnostics.Process AttachedProcess = null;

    public static IntPtr EldenRing_CrashFixPtr = IntPtr.Zero;
    public static IntPtr EldenRing_WorldChrManPtr = IntPtr.Zero;

    private static IntPtr ScanRelativeAob(CharacterReloadAobScanner aobScanner, string aob, int addrOffset, int endOfRelJumpInstr)
    {
        var aobLocation = aobScanner.Scan(CharacterReloadAobScanner.StringToAOB(aob));
        if (aobLocation == IntPtr.Zero)
            return IntPtr.Zero;
        // rel32 is signed: the target can be before the instruction
        int relAddr = (int)CharacterReloadKernel32.ReadUInt32(ProcessHandle, aobLocation + addrOffset);
        return aobLocation + endOfRelJumpInstr + relAddr;
    }

    /// <summary>
    /// Find WorldChrMan and the crash patch site with the AOBs of an offsets group.
    /// A pointer is left as zero when its AOB isn't found (or the group's values can't be parsed),
    /// so the reload can stop instead of writing to a bad address.
    /// </summary>
    public static void UpdateEldenRingAobs(CharacterReloaderOffsetEntry offsets)
    {
        EldenRing_WorldChrManPtr = IntPtr.Zero;
        EldenRing_CrashFixPtr = IntPtr.Zero;

        var aob = new CharacterReloadAobScanner(AttachedProcess);

        // "start/end" of the rel32 in the AOB
        var relParts = (offsets.worldChrManAobRelativeOffset ?? "").Split('/');
        var relStart = relParts.Length == 2 ? CharacterReloaderOffsets.ParseHex(relParts[0]) : null;
        var relEnd = relParts.Length == 2 ? CharacterReloaderOffsets.ParseHex(relParts[1]) : null;

        if (!string.IsNullOrWhiteSpace(offsets.worldChrManAob) && relStart is int start && relEnd is int end)
        {
            var global = ScanRelativeAob(aob, offsets.worldChrManAob, start, end);

            if (global != IntPtr.Zero)
            {
                EldenRing_WorldChrManPtr = CharacterReloadKernel32.ReadIntPtr(ProcessHandle, global, true);
            }
        }

        var distFromEnd = CharacterReloaderOffsets.ParseHex(offsets.crashPatchDistFromEndOfAob);

        if (!string.IsNullOrWhiteSpace(offsets.crashPatchAob) && distFromEnd is int dist)
        {
            var crashPatchAob = CharacterReloadAobScanner.StringToAOB(offsets.crashPatchAob);
            var crashPatchAobStart = aob.Scan(crashPatchAob);

            if (crashPatchAobStart != IntPtr.Zero)
            {
                EldenRing_CrashFixPtr = crashPatchAobStart + crashPatchAob.Length - dist;
            }
        }
    }

    public static void AttachProc(ProjectEntry project, string procName)
    {
        if (AttachedProcess != null && AttachedProcess?.HasExited != true)
            return;

        if (!CharacterReloadKernel32.GetHandleInformation(ProcessHandle, out _))
        {
            CloseHandle(handleInvalid: true);
        }

        //CloseHandle();
        var processes = System.Diagnostics.Process.GetProcessesByName(procName);
        if (processes.Length > 0)
        {
            var Process = processes[0];
            BaseAddress = Process.MainModule.BaseAddress;
            try
            {
                ProcessHandle = CharacterReloadKernel32.OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, false, Process.Id);
                AttachedProcess = Process;
                AttachedProcess.Exited += AttachedProcess_Exited;
            }
            catch
            {
                CloseHandle();
            }
        }
        else
        {
            Smithbox.LogError(typeof(CharacterReloadMemory), LOC.Get("COMMON_CharacterReloader_Error_Failed_Game_Attach"));
        }
    }

    private static void AttachedProcess_Exited(object sender, EventArgs e)
    {
        CloseHandle();
    }

    public static void CloseHandle(bool handleInvalid = false)
    {
        if (AttachedProcess != null)
        {
            AttachedProcess.Exited -= AttachedProcess_Exited;
            AttachedProcess.Dispose();
            AttachedProcess = null;
        }

        if (ProcessHandle != IntPtr.Zero)
        {
            if (!handleInvalid)
            {
                try
                {
                    CharacterReloadKernel32.CloseHandle(ProcessHandle);
                }
                catch
                {
                }
            }

            ProcessHandle = IntPtr.Zero;
        }

        EldenRing_CrashFixPtr = IntPtr.Zero;
        EldenRing_WorldChrManPtr = IntPtr.Zero;
    }

    // read address

    public static bool ReadBoolean(IntPtr address)
    {
        var readBuffer = new byte[sizeof(byte)];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)1, UIntPtr.Zero);
        byte value = readBuffer[0];
        var boolRet = Convert.ToBoolean(value);
        return boolRet;
    }

    public static byte ReadInt8(IntPtr address)
    {
        var readBuffer = new byte[sizeof(byte)];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)1, UIntPtr.Zero);
        var value = readBuffer[0];
        return value;
    }

    public static short ReadInt16(IntPtr address)
    {
        var readBuffer = new byte[sizeof(short)];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)2, UIntPtr.Zero);
        var value = BitConverter.ToInt16(readBuffer, 0);
        return value;
    }

    public static int ReadInt32(IntPtr address)
    {
        var readBuffer = new byte[sizeof(int)];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var value = BitConverter.ToInt32(readBuffer, 0);
        return value;
    }

    public static long ReadInt64(IntPtr address)
    {
        var readBuffer = new byte[sizeof(long)];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var value = BitConverter.ToInt64(readBuffer, 0);
        return value;
    }

    public static float ReadFloat(IntPtr address)
    {
        var readBuffer = new byte[sizeof(float)];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var value = BitConverter.ToSingle(readBuffer, 0);
        return value;
    }

    public static double ReadDouble(IntPtr address)
    {
        var readBuffer = new byte[sizeof(double)];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var value = BitConverter.ToDouble(readBuffer, 0);
        return value;
    }

    public static string ReadString(IntPtr address, int length, string encodingName)
    {
        var readBuffer = new byte[length];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var encodingType = System.Text.Encoding.GetEncoding(encodingName);
        string value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

        return value;
    }

    public static string ReadUnicodeString(IntPtr address, int length)
    {
        var readBuffer = new byte[length];
        var success = CharacterReloadKernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);

        for (int i = 0; i < readBuffer.Length; i++)
        {
            if (readBuffer[i] == 0 && readBuffer[i + 1] == 0)
            {
                Array.Resize(ref readBuffer, i + 1);
                break;
            }
        }

        var encodingType = System.Text.Encoding.GetEncoding("UNICODE");
        string value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

        return value;
    }

    //write to address
    public static bool WriteFlags8(IntPtr address, bool value, Startbit startbit)
    {
        var WriteBit = Convert.ToByte(value) * Convert.ToByte(Math.Pow((double)2, (double)startbit));
        var WriteBit_ = (byte)WriteBit;


        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes((short)WriteBit_), (UIntPtr)1, UIntPtr.Zero);
    }

    public static bool WriteBoolean(IntPtr address, bool value)
    {
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)1, UIntPtr.Zero);
    }

    public static bool WriteInt8(IntPtr address, byte value)
    {
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes((short)value), (UIntPtr)1, UIntPtr.Zero);
    }

    public static bool WriteInt16(IntPtr address, short value)
    {
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)2, UIntPtr.Zero);
    }

    public static bool WriteInt32(IntPtr address, int value)
    {
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
    }

    public static bool WriteInt64(IntPtr address, long value)
    {
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
    }

    public static bool WriteFloat(IntPtr address, float value)
    {
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
    }

    public static bool WriteDouble(IntPtr address, double value)
    {
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
    }

    public static bool WriteBytes(IntPtr address, Byte[] val)
    {
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
    }

    public static bool WriteUnicodeString(IntPtr address, string String)
    {
        byte[] val = Encoding.Unicode.GetBytes(String);
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
    }

    public static bool WriteASCIIString(IntPtr address, string String)
    {
        byte[] val = Encoding.ASCII.GetBytes(String);
        return CharacterReloadKernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
    }

    public static void ExecuteFunction(byte[] array)
    {
        var buffer = 0x100;

        var address = CharacterReloadKernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

        if (address != IntPtr.Zero)
        {
            if (WriteBytes(address, array))
            {
                var threadHandle = CharacterReloadKernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                if (threadHandle != IntPtr.Zero)
                {
                    CharacterReloadKernel32.WaitForSingleObject(threadHandle, 30000);
                }
            }

            CharacterReloadKernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
        }
    }

    public static void ExecuteBufferFunction(byte[] array, byte[] argument)
    {
        var Size1 = 0x100;
        var Size2 = 0x100;

        var address = CharacterReloadKernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, Size1, 0x1000 | 0x2000, 0X40);
        var bufferAddress = CharacterReloadKernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, Size2, 0x1000 | 0x2000, 0X40);

        var bytjmp = 0x2;
        var bytjmpAr = new byte[7];

        WriteBytes(bufferAddress, argument);

        bytjmpAr = BitConverter.GetBytes((long)bufferAddress);
        Array.Copy(bytjmpAr, 0, array, bytjmp, bytjmpAr.Length);

        if (address != IntPtr.Zero)
        {
            if (WriteBytes(address, array))
            {
                var threadHandle = CharacterReloadKernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
                if (threadHandle != IntPtr.Zero)
                {
                    CharacterReloadKernel32.WaitForSingleObject(threadHandle, 30000);
                }
            }

            CharacterReloadKernel32.VirtualFreeEx(ProcessHandle, address, Size1, 2);
            CharacterReloadKernel32.VirtualFreeEx(ProcessHandle, bufferAddress, Size2, 2);
        }
    }
}