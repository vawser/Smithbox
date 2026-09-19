using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

// Credit to Meowmaritus for ER reload elements
public static class Memory
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

    public static System.Diagnostics.Process AttachedProcess = null;

    public static IntPtr EldenRing_CrashFixPtr = IntPtr.Zero;
    public static IntPtr EldenRing_WorldChrManPtr = IntPtr.Zero;

    private static IntPtr ScanRelativeAob(AOBScanner aobScanner, string aob, int addrOffset, int endOfRelJumpInstr)
    {
        var aobLocation = aobScanner.Scan(AOBScanner.StringToAOB(aob));
        if (aobLocation == IntPtr.Zero)
            return IntPtr.Zero;
        uint relAddr = Kernel32.ReadUInt32(ProcessHandle, aobLocation + addrOffset);
        return (IntPtr)((ulong)(aobLocation + endOfRelJumpInstr) + relAddr);
    }

    public static void UpdateEldenRingAobs()
    {
        var aob = new AOBScanner(AttachedProcess);
        EldenRing_WorldChrManPtr = Memory.ScanRelativeAob(
            aob,
            HavokMemoryConsts.EldenRing_WorldChrManPtr_AOB,
            HavokMemoryConsts.EldenRing_WorldChrManPtr_JumpInstr_StartOffsetInAOB,
            HavokMemoryConsts.EldenRing_WorldChrManPtr_JumpInstr_EndOffsetInAOB);
        /*
		if( EldenRing_WorldChrManPtr == IntPtr.Zero ) {
			NotificationManager.PushNotification( "Live reload WARNING - Could not find Elden Ring WorldChrMan AOB" );
		}
		*/
        EldenRing_WorldChrManPtr = Kernel32.ReadIntPtr(ProcessHandle, EldenRing_WorldChrManPtr, true);

        var crashPatchOffsetAob = AOBScanner.StringToAOB(
            HavokMemoryConsts.EldenRing_CrashPatchOffset_AOB);

        IntPtr crashPatchOffset = aob.Scan(crashPatchOffsetAob);

        /*
		if( crashPatchOffset == IntPtr.Zero ) {
			NotificationManager.PushNotification( "Live reload WARNING - Could not find Elden Ring crash patch AOB" );
		}
		*/

        crashPatchOffset = crashPatchOffset + crashPatchOffsetAob.Length - (
            HavokMemoryConsts.EldenRing_CrashPatchOffset_DistFromEndOfAOB);

        EldenRing_CrashFixPtr = crashPatchOffset;
    }

    public static void AttachProc(ProjectEntry project, string procName)
    {
        if (AttachedProcess != null && AttachedProcess?.HasExited != true)
            return;

        if (!Kernel32.GetHandleInformation(ProcessHandle, out _))
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
                ProcessHandle = Kernel32.OpenProcess(0x2 | 0x8 | 0x10 | 0x20 | 0x400, false, Process.Id);
                AttachedProcess = Process;
                AttachedProcess.Exited += AttachedProcess_Exited;

                if (project.Descriptor.ProjectType is ProjectType.ER)
                {
                    UpdateEldenRingAobs();
                }
            }
            catch
            {
                CloseHandle();
            }
        }
        else
        {
            Smithbox.LogError(typeof(Memory), LOC.Get("HAVOK_ScriptReloader_Error_Failed_Game_Attach"));
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
                    Kernel32.CloseHandle(ProcessHandle);
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
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)1, UIntPtr.Zero);
        byte value = readBuffer[0];
        var boolRet = Convert.ToBoolean(value);
        return boolRet;
    }

    public static byte ReadInt8(IntPtr address)
    {
        var readBuffer = new byte[sizeof(byte)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)1, UIntPtr.Zero);
        var value = readBuffer[0];
        return value;
    }

    public static short ReadInt16(IntPtr address)
    {
        var readBuffer = new byte[sizeof(short)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)2, UIntPtr.Zero);
        var value = BitConverter.ToInt16(readBuffer, 0);
        return value;
    }

    public static int ReadInt32(IntPtr address)
    {
        var readBuffer = new byte[sizeof(int)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var value = BitConverter.ToInt32(readBuffer, 0);
        return value;
    }

    public static long ReadInt64(IntPtr address)
    {
        var readBuffer = new byte[sizeof(long)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var value = BitConverter.ToInt64(readBuffer, 0);
        return value;
    }

    public static float ReadFloat(IntPtr address)
    {
        var readBuffer = new byte[sizeof(float)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var value = BitConverter.ToSingle(readBuffer, 0);
        return value;
    }

    public static double ReadDouble(IntPtr address)
    {
        var readBuffer = new byte[sizeof(double)];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var value = BitConverter.ToDouble(readBuffer, 0);
        return value;
    }

    public static string ReadString(IntPtr address, int length, string encodingName)
    {
        var readBuffer = new byte[length];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);
        var encodingType = System.Text.Encoding.GetEncoding(encodingName);
        string value = encodingType.GetString(readBuffer, 0, readBuffer.Length);

        return value;
    }

    public static string ReadUnicodeString(IntPtr address, int length)
    {
        var readBuffer = new byte[length];
        var success = Kernel32.ReadProcessMemory(ProcessHandle, address, readBuffer, (UIntPtr)readBuffer.Length, UIntPtr.Zero);

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


        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes((short)WriteBit_), (UIntPtr)1, UIntPtr.Zero);
    }

    public static bool WriteBoolean(IntPtr address, bool value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)1, UIntPtr.Zero);
    }

    public static bool WriteInt8(IntPtr address, byte value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes((short)value), (UIntPtr)1, UIntPtr.Zero);
    }

    public static bool WriteInt16(IntPtr address, short value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)2, UIntPtr.Zero);
    }

    public static bool WriteInt32(IntPtr address, int value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
    }

    public static bool WriteInt64(IntPtr address, long value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
    }

    public static bool WriteFloat(IntPtr address, float value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)4, UIntPtr.Zero);
    }

    public static bool WriteDouble(IntPtr address, double value)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, BitConverter.GetBytes(value), (UIntPtr)8, UIntPtr.Zero);
    }

    public static bool WriteBytes(IntPtr address, Byte[] val)
    {
        return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
    }

    public static bool WriteUnicodeString(IntPtr address, string String)
    {
        byte[] val = Encoding.Unicode.GetBytes(String);
        return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
    }

    public static bool WriteASCIIString(IntPtr address, string String)
    {
        byte[] val = Encoding.ASCII.GetBytes(String);
        return Kernel32.WriteProcessMemory(ProcessHandle, address, val, new UIntPtr((uint)val.Length), UIntPtr.Zero);
    }

    public static void ExecuteFunction(byte[] array)
    {
        var buffer = 0x100;

        var address = Kernel32.VirtualAllocEx(ProcessHandle, IntPtr.Zero, buffer, 0x1000 | 0x2000, 0X40);

        if (address != IntPtr.Zero)
        {
            if (WriteBytes(address, array))
            {
                var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
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

        bytjmpAr = BitConverter.GetBytes((long)bufferAddress);
        Array.Copy(bytjmpAr, 0, array, bytjmp, bytjmpAr.Length);

        if (address != IntPtr.Zero)
        {
            if (WriteBytes(address, array))
            {
                var threadHandle = Kernel32.CreateRemoteThread(ProcessHandle, IntPtr.Zero, 0, address, IntPtr.Zero, 0, out var threadId);
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