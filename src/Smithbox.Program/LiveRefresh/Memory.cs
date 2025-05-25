using System;
using System.Diagnostics;
using System.Text;

namespace StudioCore.LiveRefresh;

public class Memory
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

    //Memory Stuff
    public static bool Is64Bit => IntPtr.Size == 8;

    public static IntPtr ProcessHandle { get; set; }

    public static IntPtr BaseAddress { get; set; }
    //

    /// <summary>
    ///     Checks if an address is valid.
    /// </summary>
    /// <param name="address">The address (the pointer points to).</param>
    /// <returns>True if (pointer points to a) valid address.</returns>
    public static IntPtr AttachProc(string procName)
    {
        IntPtr ZeroRt = new(0);
        Process[] processes = Process.GetProcessesByName(procName);
        if (processes.Length > 0)
        {
            Process Process = processes[0];
            BaseAddress = Process.MainModule.BaseAddress;
            ProcessHandle = Kernel32.OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, false, Process.Id);
            return ProcessHandle;
        }

        Console.WriteLine("Cant find process. Is it running?", "Process");
        return ZeroRt;
    }

    public static void CloseHandle()
    {
        Kernel32.CloseHandle(ProcessHandle);
    }

    // read address

    public static bool ReadBoolean(IntPtr address)
    {
        var readBuffer = new byte[sizeof(byte)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, 1, UIntPtr.Zero);
        var value = readBuffer[0];
        var boolRet = Convert.ToBoolean(value);
        return boolRet;
    }

    public static byte ReadInt8(IntPtr address)
    {
        var readBuffer = new byte[sizeof(byte)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, 1, UIntPtr.Zero);
        var value = readBuffer[0];
        return value;
    }

    public static short ReadInt16(IntPtr address)
    {
        var readBuffer = new byte[sizeof(short)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, 2, UIntPtr.Zero);
        var value = BitConverter.ToInt16(readBuffer, 0);
        return value;
    }

    public static int ReadInt32(IntPtr address)
    {
        var readBuffer = new byte[sizeof(int)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length,
            UIntPtr.Zero);
        var value = BitConverter.ToInt32(readBuffer, 0);
        return value;
    }

    public static long ReadInt64(IntPtr address)
    {
        var readBuffer = new byte[sizeof(long)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length,
            UIntPtr.Zero);
        var value = BitConverter.ToInt64(readBuffer, 0);
        return value;
    }

    public static float ReadFloat(IntPtr address)
    {
        var readBuffer = new byte[sizeof(float)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length,
            UIntPtr.Zero);
        var value = BitConverter.ToSingle(readBuffer, 0);
        return value;
    }

    public static double ReadDouble(IntPtr address)
    {
        var readBuffer = new byte[sizeof(double)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length,
            UIntPtr.Zero);
        var value = BitConverter.ToDouble(readBuffer, 0);
        return value;
    }

    public static string ReadString(IntPtr address, int length, string encodingName)
    {
        var readBuffer = new byte[length];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length,
            UIntPtr.Zero);
        var encodingType = Encoding.GetEncoding(encodingName);
        var value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

        return value;
    }

    public static string ReadUnicodeString(IntPtr address, int length)
    {
        var readBuffer = new byte[length];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length,
            UIntPtr.Zero);

        for (var i = 0; i < readBuffer.Length; i++)
        {
            if (readBuffer[i] == 0 && readBuffer[i + 1] == 0)
            {
                Array.Resize(ref readBuffer, i + 1);
                break;
            }
        }

        var encodingType = Encoding.GetEncoding("UNICODE");
        var value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

        return value;
    }

    //write to address
    public static bool WriteFlags8(IntPtr address, bool value, Startbit startbit)
    {
        // Unused but needs to be fixed if ever used
        throw new NotImplementedException();
        /*var WriteBit = Convert.ToByte(value) * Convert.ToByte(Math.Pow((double)2, (double)startbit));
        var WriteBit_ = (byte)WriteBit;


        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(WriteBit_), (UIntPtr)1, UIntPtr.Zero);*/
    }

    public static bool WriteBoolean(IntPtr address, bool value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), 1, UIntPtr.Zero);
    }

    public static bool WriteInt8(IntPtr address, byte value)
    {
        // Unused but needs to be fixed if ever used
        throw new NotImplementedException();
        //return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)1, UIntPtr.Zero);
    }

    public static bool WriteInt16(IntPtr address, short value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), 2, UIntPtr.Zero);
    }

    public static bool WriteInt32(IntPtr address, int value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), 4, UIntPtr.Zero);
    }

    public static bool WriteInt64(IntPtr address, long value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), 8, UIntPtr.Zero);
    }

    public static bool WriteFloat(IntPtr address, float value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), 4, UIntPtr.Zero);
    }

    public static bool WriteDouble(IntPtr address, double value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), 8, UIntPtr.Zero);
    }

    public static bool WriteBytes(IntPtr address, Byte[] val)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length),
            UIntPtr.Zero);
    }

    public static bool WriteUnicodeString(IntPtr address, string String)
    {
        var val = Encoding.Unicode.GetBytes(String);
        return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length),
            UIntPtr.Zero);
    }

    public static bool WriteASCIIString(IntPtr address, string String)
    {
        var val = Encoding.ASCII.GetBytes(String);
        return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length),
            UIntPtr.Zero);
    }

    public static void ExecuteFunction(byte[] array)
    {
        var buffer = 0x100;

        var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

        if (address != IntPtr.Zero)
        {
            if (WriteBytes(address, array))
            {
                var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address,
                    IntPtr.Zero, 0, out var threadId);
                if (threadHandle != IntPtr.Zero)
                {
                    Kernel32.WaitForSingleObject(threadHandle, 30000);
                }
            }

            Kernel32.VirtualFreeEx(ProcessHandle, address, buffer, 2);
        }
    }

    public static void ExecuteBufferFunction(byte[] array, byte[] argument)
    {
        var Size1 = 0x100;
        var Size2 = 0x100;

        var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, Size1, 0x1000 | 0x2000, 0X40);
        var bufferAddress = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, Size2, 0x1000 | 0x2000, 0X40);

        var bytjmp = 0x2;
        var bytjmpAr = new byte[7];

        WriteBytes(bufferAddress, argument);

        bytjmpAr = BitConverter.GetBytes(bufferAddress);
        Array.Copy(bytjmpAr, 0, array, bytjmp, bytjmpAr.Length);

        if (address != IntPtr.Zero)
        {
            if (WriteBytes(address, array))
            {
                var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address,
                    IntPtr.Zero, 0, out var threadId);
                if (threadHandle != IntPtr.Zero)
                {
                    Kernel32.WaitForSingleObject(threadHandle, 30000);
                }
            }

            Kernel32.VirtualFreeEx(ProcessHandle, address, Size1, 2);
            Kernel32.VirtualFreeEx(ProcessHandle, bufferAddress, Size2, 2);
        }
    }
}
