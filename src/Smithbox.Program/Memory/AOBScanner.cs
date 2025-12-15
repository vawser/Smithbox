using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StudioCore.Memory;

public class AOBScanner
{
    private readonly Process process;
    private readonly IntPtr processHandle;

    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int dwSize,
        out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll")]
    public static extern int VirtualQueryEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        out AOBScanner.MEMORY_BASIC_INFORMATION lpBuffer,
        int dwLength);

    [DllImport("kernel32.dll")]
    public static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int nSize,
        out int lpNumberOfBytesWritten);

    public AOBScanner(Process targetProcess)
    {
        this.process = targetProcess;
        this.processHandle = this.process.Handle;
    }

    public IntPtr FindPattern(byte[] pattern, string mask, bool scanFromZero = true)
    {
        int length = pattern.Length;
        if (mask.Length != length)
            throw new ArgumentException("Pattern and mask lengths must be equal");
        IntPtr lpAddress = scanFromZero ? IntPtr.Zero : this.process.MainModule.BaseAddress;
        AOBScanner.MEMORY_BASIC_INFORMATION lpBuffer1 = new AOBScanner.MEMORY_BASIC_INFORMATION();
        while (AOBScanner.VirtualQueryEx(this.processHandle, lpAddress, out lpBuffer1, Marshal.SizeOf<AOBScanner.MEMORY_BASIC_INFORMATION>(lpBuffer1)) != 0 && lpAddress.ToInt64() < (long)(IntPtr.MaxValue - (IntPtr)(int)lpBuffer1.RegionSize))
        {
            if (((int)lpBuffer1.Protect & 4) != 0 && ((int)lpBuffer1.State & 4096) != 0 && lpBuffer1.Protect != 1U)
            {
                IntPtr num = IntPtr.Add(lpBuffer1.BaseAddress, (int)lpBuffer1.RegionSize);
                for (IntPtr index1 = lpBuffer1.BaseAddress; index1.ToInt64() < num.ToInt64(); index1 = IntPtr.Add(index1, 65536))
                {
                    try
                    {
                        int dwSize = (int)Math.Min(65536L, num.ToInt64() - index1.ToInt64());
                        byte[] lpBuffer2 = new byte[dwSize];
                        int lpNumberOfBytesRead;
                        if (AOBScanner.ReadProcessMemory(this.processHandle, index1, lpBuffer2, dwSize, out lpNumberOfBytesRead))
                        {
                            if (lpNumberOfBytesRead >= length)
                            {
                                for (int offset = 0; offset <= lpNumberOfBytesRead - length; ++offset)
                                {
                                    bool flag = true;
                                    for (int index2 = 0; index2 < length; ++index2)
                                    {
                                        if (mask[index2] != '?' && (int)pattern[index2] != (int)lpBuffer2[offset + index2])
                                        {
                                            flag = false;
                                            break;
                                        }
                                    }
                                    if (flag)
                                        return IntPtr.Add(index1, offset);
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            IntPtr num1 = IntPtr.Add(lpBuffer1.BaseAddress, (int)lpBuffer1.RegionSize);
            if (num1.ToInt64() > lpAddress.ToInt64())
            {
                lpAddress = num1;
                Console.WriteLine((long)lpAddress);
            }
            else
                break;
        }
        return IntPtr.Zero;
    }

    public byte[] ReadBytes(IntPtr address, int length)
    {
        try
        {
            byte[] lpBuffer = new byte[length];
            int lpNumberOfBytesRead;
            if (AOBScanner.ReadProcessMemory(this.processHandle, address, lpBuffer, length, out lpNumberOfBytesRead))
            {
                if (lpNumberOfBytesRead == length)
                    return lpBuffer;
            }
        }
        catch
        {
        }
        return (byte[])null;
    }

    public T Read<T>(IntPtr address) where T : struct
    {
        int length = Marshal.SizeOf(typeof(T));
        byte[] numArray = this.ReadBytes(address, length);
        if (numArray == null)
            return default(T);
        GCHandle gcHandle = GCHandle.Alloc((object)numArray, GCHandleType.Pinned);
        try
        {
            return Marshal.PtrToStructure<T>(gcHandle.AddrOfPinnedObject());
        }
        finally
        {
            gcHandle.Free();
        }
    }

    public sbyte ReadSByte(IntPtr address) => this.Read<sbyte>(address);

    public int ReadInt32(IntPtr address) => this.Read<int>(address);

    public uint ReadUInt32(IntPtr address) => this.Read<uint>(address);

    public long ReadInt64(IntPtr address) => this.Read<long>(address);

    public ulong ReadUInt64(IntPtr address) => this.Read<ulong>(address);

    public float ReadFloat(IntPtr address) => this.Read<float>(address);

    public double ReadDouble(IntPtr address) => this.Read<double>(address);

    public short ReadInt16(IntPtr address) => this.Read<short>(address);

    public ushort ReadUInt16(IntPtr address) => this.Read<ushort>(address);

    public byte ReadByte(IntPtr address)
    {
        byte[] numArray = this.ReadBytes(address, 1);
        return numArray == null ? (byte)0 : numArray[0];
    }

    public bool ReadBool(IntPtr address) => this.ReadByte(address) > (byte)0;

    public bool WriteBytes(IntPtr address, byte[] data)
    {
        try
        {
            int lpNumberOfBytesWritten;
            return AOBScanner.WriteProcessMemory(this.processHandle, address, data, data.Length, out lpNumberOfBytesWritten) && lpNumberOfBytesWritten == data.Length;
        }
        catch
        {
            return false;
        }
    }

    public bool WriteSBytes(IntPtr address, sbyte[] data)
    {
        try
        {
            byte[] numArray = new byte[data.Length];
            Buffer.BlockCopy((Array)data, 0, (Array)numArray, 0, data.Length);
            int lpNumberOfBytesWritten;
            return AOBScanner.WriteProcessMemory(this.processHandle, address, numArray, numArray.Length, out lpNumberOfBytesWritten) && lpNumberOfBytesWritten == numArray.Length;
        }
        catch
        {
            return false;
        }
    }

    public bool Write<T>(IntPtr address, T value) where T : struct
    {
        byte[] data = new byte[Marshal.SizeOf(typeof(T))];
        GCHandle gcHandle = GCHandle.Alloc((object)data, GCHandleType.Pinned);
        try
        {
            Marshal.StructureToPtr<T>(value, gcHandle.AddrOfPinnedObject(), false);
            return this.WriteBytes(address, data);
        }
        finally
        {
            gcHandle.Free();
        }
    }

    public bool WriteInt32(IntPtr address, int value) => this.Write<int>(address, value);

    public bool WriteUInt32(IntPtr address, uint value) => this.Write<uint>(address, value);

    public bool WriteInt64(IntPtr address, long value) => this.Write<long>(address, value);

    public bool WriteUInt64(IntPtr address, ulong value) => this.Write<ulong>(address, value);

    public bool WriteSingle(IntPtr address, float value) => this.Write<float>(address, value);

    public bool WriteFloat(IntPtr address, float value) => this.Write<float>(address, value);

    public bool WriteDouble(IntPtr address, double value) => this.Write<double>(address, value);

    public bool WriteInt16(IntPtr address, short value) => this.Write<short>(address, value);

    public bool WriteUInt16(IntPtr address, ushort value) => this.Write<ushort>(address, value);

    public bool WriteByte(IntPtr address, byte value)
    {
        return this.WriteBytes(address, new byte[1] { value });
    }

    public bool WriteSByte(IntPtr address, sbyte value)
    {
        return this.WriteSBytes(address, new sbyte[1] { value });
    }

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
}
