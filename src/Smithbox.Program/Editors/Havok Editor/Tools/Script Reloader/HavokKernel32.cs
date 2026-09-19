using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Editors.HavokEditor;


// Credit to Meowmaritus for ER reload elements
public static class Kernel32
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    #region Constants from winnt.h
    public const uint PAGE_NOACCESS = 0x01;
    public const uint PAGE_READONLY = 0x02;
    public const uint PAGE_READWRITE = 0x04;
    public const uint PAGE_WRITECOPY = 0x08;
    public const uint PAGE_EXECUTE = 0x10;
    public const uint PAGE_EXECUTE_READ = 0x20;
    public const uint PAGE_EXECUTE_READWRITE = 0x40;
    public const uint PAGE_EXECUTE_WRITECOPY = 0x80;
    public const uint PAGE_GUARD = 0x100;
    public const uint PAGE_NOCACHE = 0x200;
    public const uint PAGE_WRITECOMBINE = 0x400;
    public const uint PAGE_ENCLAVE_THREAD_CONTROL = 0x80000000;
    public const uint PAGE_REVERT_TO_FILE_MAP = 0x80000000;
    public const uint PAGE_TARGETS_NO_UPDATE = 0x40000000;
    public const uint PAGE_TARGETS_INVALID = 0x40000000;
    public const uint PAGE_ENCLAVE_UNVALIDATED = 0x20000000;
    public const uint PAGE_ENCLAVE_DECOMMIT = 0x10000000;
    public const uint MEM_COMMIT = 0x00001000;
    public const uint MEM_RESERVE = 0x00002000;
    public const uint MEM_REPLACE_PLACEHOLDER = 0x00004000;
    public const uint MEM_RESERVE_PLACEHOLDER = 0x00040000;
    public const uint MEM_RESET = 0x00080000;
    public const uint MEM_TOP_DOWN = 0x00100000;
    public const uint MEM_WRITE_WATCH = 0x00200000;
    public const uint MEM_PHYSICAL = 0x00400000;
    public const uint MEM_ROTATE = 0x00800000;
    public const uint MEM_DIFFERENT_IMAGE_BASE_OK = 0x00800000;
    public const uint MEM_RESET_UNDO = 0x01000000;
    public const uint MEM_LARGE_PAGES = 0x20000000;
    public const uint MEM_4MB_PAGES = 0x80000000;
    public const uint MEM_64K_PAGES = MEM_LARGE_PAGES | MEM_PHYSICAL;
    public const uint MEM_UNMAP_WITH_TRANSIENT_BOOST = 0x00000001;
    public const uint MEM_COALESCE_PLACEHOLDERS = 0x00000001;
    public const uint MEM_PRESERVE_PLACEHOLDER = 0x00000002;
    public const uint MEM_DECOMMIT = 0x00004000;
    public const uint MEM_RELEASE = 0x00008000;
    public const uint MEM_FREE = 0x00010000;
    #endregion

    [StructLayout(LayoutKind.Sequential)]
    public struct MEMORY_BASIC_INFORMATION
    {
        public IntPtr BaseAddress;
        public IntPtr AllocationBase;
        public uint AllocationProtect;
        public IntPtr RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [DllImport("kernel32.dll")]
    public static extern bool GetHandleInformation(IntPtr hObject, out uint lpdwFlags);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]
    public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out uint lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll")]
    public static extern bool IsWow64Process(IntPtr hProcess, out bool Wow64Process);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, UIntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll")]
    public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);

    [DllImport("kernel32.dll")]
    public static extern uint VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, IntPtr dwLength);

    [DllImport("kernel32.dll")]
    public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, UIntPtr lpNumberOfBytesWritten);


    public static byte[] ReadBytes(IntPtr handle, IntPtr address, uint length)
    {
        byte[] bytes = new byte[length];
        ReadProcessMemory(handle, address, bytes, (UIntPtr)length, UIntPtr.Zero);
        return bytes;
    }

    public static bool WriteBytes(IntPtr handle, IntPtr address, byte[] bytes)
    {
        return WriteProcessMemory(handle, address, bytes, (UIntPtr)bytes.Length, UIntPtr.Zero);
    }

    public static IntPtr ReadIntPtr(IntPtr handle, IntPtr address, bool is64bit)
    {
        if (is64bit)
            return (IntPtr)ReadInt64(handle, address);
        else
            return (IntPtr)ReadInt32(handle, address);
    }

    public static bool ReadFlag32(IntPtr handle, IntPtr address, uint mask)
    {
        uint flags = ReadUInt32(handle, address);
        return (flags & mask) != 0;
    }

    public static bool WriteFlag32(IntPtr handle, IntPtr address, uint mask, bool state)
    {
        uint flags = ReadUInt32(handle, address);
        if (state)
            flags |= mask;
        else
            flags &= ~mask;
        return WriteUInt32(handle, address, flags);
    }


    public static sbyte ReadSByte(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 1);
        return (sbyte)bytes[0];
    }

    public static bool WriteSByte(IntPtr handle, IntPtr address, sbyte value)
    {
        // Note: do not BitConverter.GetBytes this, stupid
        return WriteBytes(handle, address, new byte[] { (byte)value });
    }


    public static byte ReadByte(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 1);
        return bytes[0];
    }

    public static bool WriteByte(IntPtr handle, IntPtr address, byte value)
    {
        // Note: do not BitConverter.GetBytes this, stupid
        return WriteBytes(handle, address, new byte[] { value });
    }


    public static bool ReadBoolean(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 1);
        return BitConverter.ToBoolean(bytes, 0);
    }

    public static bool WriteBoolean(IntPtr handle, IntPtr address, bool value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static short ReadInt16(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 2);
        return BitConverter.ToInt16(bytes, 0);
    }

    public static bool WriteInt16(IntPtr handle, IntPtr address, short value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static ushort ReadUInt16(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 2);
        return BitConverter.ToUInt16(bytes, 0);
    }

    public static bool WriteUInt16(IntPtr handle, IntPtr address, ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static int ReadInt32(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 4);
        return BitConverter.ToInt32(bytes, 0);
    }

    public static bool WriteInt32(IntPtr handle, IntPtr address, int value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static uint ReadUInt32(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 4);
        return BitConverter.ToUInt32(bytes, 0);
    }

    public static bool WriteUInt32(IntPtr handle, IntPtr address, uint value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static long ReadInt64(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 8);
        return BitConverter.ToInt64(bytes, 0);
    }

    public static bool WriteInt64(IntPtr handle, IntPtr address, long value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static ulong ReadUInt64(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 8);
        return BitConverter.ToUInt64(bytes, 0);
    }

    public static bool WriteUInt64(IntPtr handle, IntPtr address, ulong value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static float ReadSingle(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 4);
        return BitConverter.ToSingle(bytes, 0);
    }

    public static bool WriteSingle(IntPtr handle, IntPtr address, float value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static double ReadDouble(IntPtr handle, IntPtr address)
    {
        byte[] bytes = ReadBytes(handle, address, 8);
        return BitConverter.ToDouble(bytes, 0);
    }

    public static bool WriteDouble(IntPtr handle, IntPtr address, double value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        return WriteBytes(handle, address, bytes);
    }


    public static string ReadString(IntPtr handle, IntPtr address, Encoding encoding, uint byteCount, bool trim = true)
    {
        byte[] bytes = ReadBytes(handle, address, byteCount);
        string value = encoding.GetString(bytes);
        if (trim)
        {
            int term = value.IndexOf('\0');
            if (term != -1)
                value = value.Substring(0, term);
        }
        return value;
    }

    public static bool WriteString(IntPtr handle, IntPtr address, Encoding encoding, uint byteCount, string value)
    {
        char[] chars = value.ToCharArray();
        int charCount = chars.Length;
        while (encoding.GetByteCount(chars, 0, charCount) > byteCount)
            charCount--;

        byte[] bytes = new byte[byteCount];
        encoding.GetBytes(chars, 0, charCount, bytes, 0);
        return WriteBytes(handle, address, bytes);
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}