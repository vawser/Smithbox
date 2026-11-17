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

    // Name of the attached process, so a reload for another game attaches again
    private static string AttachedProcessName = null;

    /// <summary>
    /// Resolve a rel32 inside an AOB match: relSpec is "start/end" of the rel32 within the AOB ("0x3/0x7"),
    /// and the target is the end + rel32 (rel32 is signed). Zero if the AOB isn't found or relSpec can't be read.
    /// </summary>
    public static IntPtr ResolveRelative(CharacterReloadAobScanner scanner, string aob, string relSpec)
    {
        if (string.IsNullOrWhiteSpace(aob))
            return IntPtr.Zero;

        var relParts = (relSpec ?? "").Split('/');
        var relStart = relParts.Length == 2 ? CharacterReloaderOffsets.ParseHex(relParts[0]) : null;
        var relEnd = relParts.Length == 2 ? CharacterReloaderOffsets.ParseHex(relParts[1]) : null;

        if (relStart is not int start || relEnd is not int end)
            return IntPtr.Zero;

        var aobLocation = scanner.Scan(CharacterReloadAobScanner.StringToAOB(aob));
        if (aobLocation == IntPtr.Zero)
            return IntPtr.Zero;

        int relAddr = (int)CharacterReloadKernel32.ReadUInt32(ProcessHandle, aobLocation + start);
        return aobLocation + end + relAddr;
    }

    /// <summary>
    /// The address an AOB match ends at, less a distance. Zero if the AOB isn't found or the distance can't be read.
    /// </summary>
    public static IntPtr ResolveFromEnd(CharacterReloadAobScanner scanner, string aob, string distFromEnd)
    {
        var dist = CharacterReloaderOffsets.ParseHex(distFromEnd);

        if (string.IsNullOrWhiteSpace(aob) || dist is not int d)
            return IntPtr.Zero;

        var pattern = CharacterReloadAobScanner.StringToAOB(aob);
        var aobLocation = scanner.Scan(pattern);

        return aobLocation == IntPtr.Zero ? IntPtr.Zero : aobLocation + pattern.Length - d;
    }

    /// <summary>
    /// Attach to the first process with this name (no extension). Keeps the current attachment when it is to
    /// the same, still running, process. False if there is no such process or it can't be opened.
    /// </summary>
    public static bool AttachProc(string procName)
    {
        if (AttachedProcess != null)
        {
            var exited = true;
            try { exited = AttachedProcess.HasExited; } catch { }

            if (!exited && string.Equals(AttachedProcessName, procName, StringComparison.OrdinalIgnoreCase))
                return true;

            CloseHandle();
        }

        var processes = System.Diagnostics.Process.GetProcessesByName(procName);
        if (processes.Length == 0)
            return false;

        var process = processes[0];

        try
        {
            BaseAddress = process.MainModule.BaseAddress;
            ProcessHandle = CharacterReloadKernel32.OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, false, process.Id);

            if (ProcessHandle == IntPtr.Zero)
            {
                process.Dispose();
                return false;
            }

            AttachedProcess = process;
            AttachedProcessName = procName;
            return true;
        }
        catch
        {
            process.Dispose();
            CloseHandle();
            return false;
        }
    }

    public static void CloseHandle(bool handleInvalid = false)
    {
        AttachedProcessName = null;

        if (AttachedProcess != null)
        {
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