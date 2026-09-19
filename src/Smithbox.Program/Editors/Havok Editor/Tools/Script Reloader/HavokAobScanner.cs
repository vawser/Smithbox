using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

// Credit to Meowmaritus for ER reload elements
public class AOBScanner
{
    private const uint PAGE_EXECUTE_ANY = Kernel32.PAGE_EXECUTE | Kernel32.PAGE_EXECUTE_READ | Kernel32.PAGE_EXECUTE_READWRITE | Kernel32.PAGE_EXECUTE_WRITECOPY;

    private List<Kernel32.MEMORY_BASIC_INFORMATION> MemRegions;

    private Dictionary<IntPtr, byte[]> ReadMemory;

    public AOBScanner(Process process)
    {
        MemRegions = new List<Kernel32.MEMORY_BASIC_INFORMATION>();
        IntPtr memRegionAddr = process.MainModule.BaseAddress;
        IntPtr mainModuleEnd = process.MainModule.BaseAddress + process.MainModule.ModuleMemorySize;
        uint queryResult;

        do
        {
            var memInfo = new Kernel32.MEMORY_BASIC_INFORMATION();
            queryResult = Kernel32.VirtualQueryEx(process.Handle, memRegionAddr, out memInfo, (IntPtr)Marshal.SizeOf(memInfo));
            if (queryResult != 0)
            {
                if ((memInfo.State & Kernel32.MEM_COMMIT) != 0 && (memInfo.Protect & Kernel32.PAGE_GUARD) == 0 && (memInfo.Protect & PAGE_EXECUTE_ANY) != 0)
                    MemRegions.Add(memInfo);
                memRegionAddr = memInfo.BaseAddress + (int)memInfo.RegionSize;
            }
        } while (queryResult != 0 && (ulong)memRegionAddr < (ulong)mainModuleEnd);

        ReadMemory = new Dictionary<IntPtr, byte[]>();
        foreach (Kernel32.MEMORY_BASIC_INFORMATION memRegion in MemRegions)
            ReadMemory[memRegion.BaseAddress] = Kernel32.ReadBytes(process.Handle, memRegion.BaseAddress, (uint)memRegion.RegionSize);
    }

    public IntPtr Scan(byte?[] aob)
    {
        int[] pattern = Unbox(aob);
        foreach (IntPtr baseAddress in ReadMemory.Keys)
        {
            byte[] bytes = ReadMemory[baseAddress];
            if (TryScan(bytes, pattern, out int index))
                return baseAddress + index;
        }

        return IntPtr.Zero;
    }

    // Using nullable byte for comparisons is very slow
    private static int[] Unbox(byte?[] aob)
    {
        var pattern = new int[aob.Length];
        for (int i = 0; i < aob.Length; i++)
        {
            if (aob[i].HasValue)
                pattern[i] = aob[i].Value;
            else
                pattern[i] = -1;
        }
        return pattern;
    }

    private static bool TryScan(byte[] text, int[] pattern, out int index)
    {
        for (int i = 0; i < text.Length - pattern.Length; i++)
        {
            for (int j = 0; j < pattern.Length; j++)
            {
                if (pattern[j] != -1 && pattern[j] != text[i + j])
                {
                    break;
                }
                else if (j == pattern.Length - 1)
                {
                    index = i;
                    return true;
                }
            }
        }

        index = -1;
        return false;
    }

    public static byte?[] StringToAOB(string text)
    {
        string[] items = text.Split(' ');
        byte?[] aob = new byte?[items.Length];
        for (int i = 0; i < aob.Length; i++)
        {
            string item = items[i];
            if (item == "?" || item == "??")
                aob[i] = null;
            else
                aob[i] = byte.Parse(item, System.Globalization.NumberStyles.HexNumber);
        }
        return aob;
    }
}