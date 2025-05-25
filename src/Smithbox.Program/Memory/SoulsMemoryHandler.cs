using Andre.Formats;
using Microsoft.Extensions.Logging;
using ProcessMemoryUtilities.Managed;
using ProcessMemoryUtilities.Native;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Memory;

public class SoulsMemoryHandler
{
    private ParamEditorScreen Editor;

    internal record RelativeOffset(int StartOffset, int EndOffset);

    // Outer dict: key = process ID. Inner dict: key = arbitrary id, value = memory offset.
    internal static Dictionary<long, Dictionary<string, int>> ProcessOffsetBank = new();

    public readonly Process gameProcess;
    public readonly Dictionary<string, int> _processOffsets;
    public nint memoryHandle;

    public SoulsMemoryHandler(ParamEditorScreen editor, Process gameProcess)
    {
        Editor = editor;

        this.gameProcess = gameProcess;
        memoryHandle = NativeWrapper.OpenProcess(
            ProcessAccessFlags.CreateThread | ProcessAccessFlags.ReadWrite | ProcessAccessFlags.Execute |
            ProcessAccessFlags.VirtualMemoryOperation, gameProcess.Id);

        if (!ProcessOffsetBank.TryGetValue(gameProcess.Id, out _processOffsets))
        {
            _processOffsets = new();
            ProcessOffsetBank.Add(gameProcess.Id, _processOffsets);
        }
    }

    public nint GetBaseAddress()
    {
        return gameProcess.MainModule.BaseAddress;
    }

    public void Terminate()
    {
        NativeWrapper.CloseHandle(memoryHandle);
        memoryHandle = 0;
    }

    [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
    private static extern bool ReadProcessMemory(nint Handle, nint Address,
        [Out] byte[] Arr, int Size, out int BytesRead);

    public bool ReadProcessMemory(nint baseAddress, ref byte[] arr, int size)
    {
        return ReadProcessMemory(memoryHandle, baseAddress, arr, size, out _);
    }

    public bool ReadProcessMemory<T>(nint baseAddress, ref T buffer) where T : unmanaged
    {
        return NativeWrapper.ReadProcessMemory(memoryHandle, baseAddress, ref buffer);
    }

    public bool WriteProcessMemory<T>(nint baseAddress, ref T buffer) where T : unmanaged
    {
        return NativeWrapper.WriteProcessMemory(memoryHandle, baseAddress, ref buffer);
    }

    public bool WriteProcessMemoryArray<T>(nint baseAddress, T[] buffer) where T : unmanaged
    {
        return NativeWrapper.WriteProcessMemoryArray(memoryHandle, baseAddress, buffer);
    }

    private int GetRelativeOffset(byte[] mem, int offset, int startOffset, int endOffset)
    {
        var start = offset + startOffset;
        var end = start + 4;
        var target = mem[start..end];
        var address = BitConverter.ToInt32(target);
        return offset + address + endOffset;
    }

    /// <summary>
    /// Finds and caches offset that matches provided AOB pattern.
    /// </summary>
    /// <returns>True if offset was found; otherwise false.</returns>
    public bool TryFindOffsetFromAOB(string offsetName, string aobPattern, List<(int, int)> relativeOffsets, out int outOffset)
    {
        if (_processOffsets.TryGetValue(offsetName, out outOffset))
            return true;

        GenerateAobPattern(aobPattern, out var pattern, out var wildcard);

        var memSize = gameProcess.MainModule.ModuleMemorySize;
        var memFindLength = memSize - pattern.Length;
        var mem = new byte[memSize];

        ReadProcessMemory(gameProcess.MainModule.BaseAddress, ref mem, memSize);

        for (var offset = 0; offset < memFindLength; offset++)
        {
            if (mem[offset] == pattern[0])
            {
                var matched = true;
                for (var iPattern = 1; iPattern < pattern.Length; iPattern++)
                {
                    if (wildcard[iPattern] || mem[offset + iPattern] == pattern[iPattern])
                    {
                        continue;
                    }

                    matched = false;
                    break;
                }

                if (matched)
                {
                    // Match has been found. Set out variable and add to process offsets.
                    foreach (var relativeOffset in relativeOffsets)
                    {
                        offset = GetRelativeOffset(mem, offset, relativeOffset.Item1, relativeOffset.Item2);
                    }

                    outOffset = offset;
                    _processOffsets.Add(offsetName, offset);
                    //TaskLogs.AddLog($"Found AOB in memory for {offsetName}. Offset: 0x{offset:X2}");
                    return true;
                }
            }
        }

        TaskLogs.AddLog($"Unable to find AOB in memory for {offsetName}.", LogLevel.Warning);
        return false;
    }

    private void GenerateAobPattern(string str, out byte[] pattern, out bool[] wildcard)
    {
        var split = str.Split(",");
        pattern = new byte[split.Length];
        wildcard = new bool[split.Length];

        for (var i = 0; i < split.Length; i++)
        {
            var byteStr = split[i].Replace("0x", "");

            if (byteStr == "??")
            {
                wildcard[i] = true;
            }
            else
            {
                pattern[i] = byte.Parse(byteStr, NumberStyles.HexNumber);
            }
        }
    }

    internal nint GetParamPtr(nint paramRepoPtr, GameOffsetsEntry offsets, int pOffset)
    {
        if (offsets.Is64Bit)
        {
            return GetParamPtr64Bit(paramRepoPtr, offsets, pOffset);
        }

        return GetParamPtr32Bit(paramRepoPtr, offsets, pOffset);
    }

    private nint GetParamPtr64Bit(nint paramRepoPtr, GameOffsetsEntry offsets, int pOffset)
    {
        var paramPtr = paramRepoPtr;
        NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        paramPtr = nint.Add(paramPtr, pOffset);
        NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        foreach (var innerPathPart in offsets.paramInnerPath)
        {
            paramPtr = nint.Add(paramPtr, innerPathPart);
            NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        }

        return paramPtr;
    }

    private nint GetParamPtr32Bit(nint paramRepoPtr, GameOffsetsEntry offsets, int pOffset)
    {
        var ParamPtr = (int)paramRepoPtr;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr, ref ParamPtr);
        ParamPtr = ParamPtr + pOffset;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr, ref ParamPtr);
        foreach (var innerPathPart in offsets.paramInnerPath)
        {
            ParamPtr = ParamPtr + innerPathPart;
            NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr, ref ParamPtr);
        }

        return ParamPtr;
    }

    internal int GetRowCount(GameOffsetsEntry gOffsets, nint paramPtr)
    {
        if (gOffsets.type is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            return GetRowCountInt(gOffsets, paramPtr);
        }

        return GetRowCountShort(gOffsets, paramPtr);
    }

    private int GetRowCountInt(GameOffsetsEntry gOffsets, nint ParamPtr)
    {
        var buffer = 0;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr + gOffsets.paramCountOffset, ref buffer);
        return buffer;
    }

    private int GetRowCountShort(GameOffsetsEntry gOffsets, nint ParamPtr)
    {
        short buffer = 0;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr + gOffsets.paramCountOffset, ref buffer);
        return buffer;
    }

    internal nint GetToRowPtr(GameOffsetsEntry gOffsets, nint paramPtr)
    {
        paramPtr = nint.Add(paramPtr, gOffsets.paramDataOffset);
        return paramPtr;
    }


    public void ExecuteFunction(byte[] array)
    {
        nint buffer = 0x100;

        var address = NativeWrapper.VirtualAllocEx(memoryHandle, nint.Zero, buffer,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ExecuteReadWrite);

        if (address != nint.Zero)
        {
            if (WriteProcessMemoryArray(address, array))
            {
                var threadHandle = NativeWrapper.CreateRemoteThread(memoryHandle, nint.Zero, 0, address,
                    nint.Zero, ThreadCreationFlags.Immediately, out var threadId);
                if (threadHandle != nint.Zero)
                {
                    Kernel32.WaitForSingleObject(threadHandle, 30000);
                }
            }

            NativeWrapper.VirtualFreeEx(memoryHandle, address, buffer, FreeType.PreservePlaceholder);
        }
    }

    public void ExecuteBufferFunction(byte[] array, byte[] argument)
    {
        var Size1 = 0x100;
        var Size2 = 0x100;

        var address = NativeWrapper.VirtualAllocEx(memoryHandle, nint.Zero, Size1,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ExecuteReadWrite);
        var bufferAddress = NativeWrapper.VirtualAllocEx(memoryHandle, nint.Zero, Size2,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ExecuteReadWrite);

        var bytjmp = 0x2;
        var bytjmpAr = new byte[7];

        WriteProcessMemoryArray(bufferAddress, argument);

        bytjmpAr = BitConverter.GetBytes(bufferAddress);
        Array.Copy(bytjmpAr, 0, array, bytjmp, bytjmpAr.Length);

        if (address != nint.Zero)
        {
            if (WriteProcessMemoryArray(address, array))
            {
                var threadHandle = NativeWrapper.CreateRemoteThread(memoryHandle, nint.Zero, 0, address,
                    nint.Zero, ThreadCreationFlags.Immediately, out var threadId);

                if (threadHandle != nint.Zero)
                {
                    Kernel32.WaitForSingleObject(threadHandle, 30000);
                }
            }

            NativeWrapper.VirtualFreeEx(memoryHandle, address, Size1, FreeType.PreservePlaceholder);
            NativeWrapper.VirtualFreeEx(memoryHandle, address, Size2, FreeType.PreservePlaceholder);
        }
    }

    public void RequestReloadChr(string chrName)
    {
        var chrNameBytes = Encoding.Unicode.GetBytes(chrName);

        var memoryWriteBuffer = true;
        WriteProcessMemory(gameProcess.MainModule.BaseAddress + 0x4768F7F, ref memoryWriteBuffer);

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

        ExecuteBufferFunction(buffer, chrNameBytes);
    }

    internal void PlayerItemGive(GameOffsetsEntry offsets, List<Param.Row> rows, string paramDefParamType,
        int itemQuantityReceived = 1, int itemDurabilityReceived = -1, int upgradeLevelItemToGive = 0)
    {
        // ItemGib - DS3
        if (Editor.Project.ProjectType is ProjectType.DS3)
        {
            //Thanks Church Guard for providing the foundation of this.
            //Only supports ds3 as of now
            if (offsets.itemGibOffsets.ContainsKey(paramDefParamType) && rows.Any())
            {
                var paramOffset = offsets.itemGibOffsets[paramDefParamType];

                List<int> intListProcessing = new();

                //Padding? Supposedly?
                intListProcessing.Add(0);
                intListProcessing.Add(0);
                intListProcessing.Add(0);
                intListProcessing.Add(0);
                //Items to give amount
                intListProcessing.Add(rows.Count());

                foreach (Param.Row row in rows)
                {
                    intListProcessing.Add(row.ID + paramOffset + upgradeLevelItemToGive);
                    intListProcessing.Add(itemQuantityReceived);
                    intListProcessing.Add(itemDurabilityReceived);
                }

                //ItemGib ASM in byte format
                byte[] itemGibByteFunctionDS3 =
                {
                0x48, 0x83, 0xEC, 0x48, 0x4C, 0x8D, 0x01, 0x48, 0x8D, 0x51, 0x10, 0x48, 0xA1, 0x00, 0x23, 0x75,
                0x44, 0x01, 0x00, 0x00, 0x00, 0x48, 0x8B, 0xC8, 0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08,
                0x70, 0xBA, 0x7B, 0x40, 0x01, 0x00, 0x00, 0x00, 0x48, 0x83, 0xC4, 0x48, 0xC3
            };

                //ItemGib Arguments Int Array
                var itemGibArgumentsIntArray = new int[intListProcessing.Count()];
                intListProcessing.CopyTo(itemGibArgumentsIntArray);

                //Copy itemGibArgumentsIntArray's Bytes into a byte array
                var itemGibArgumentsByteArray = new byte[Buffer.ByteLength(itemGibArgumentsIntArray)];
                Buffer.BlockCopy(itemGibArgumentsIntArray, 0, itemGibArgumentsByteArray, 0,
                    itemGibArgumentsByteArray.Length);

                //Allocate Memory for ItemGib and Arguments
                var itemGibByteFunctionPtr = NativeWrapper.VirtualAllocEx(memoryHandle, 0,
                    Buffer.ByteLength(itemGibByteFunctionDS3), AllocationType.Commit | AllocationType.Reserve,
                    MemoryProtectionFlags.ExecuteReadWrite);
                var itemGibArgumentsPtr = NativeWrapper.VirtualAllocEx(memoryHandle, 0,
                    Buffer.ByteLength(itemGibArgumentsIntArray), AllocationType.Commit | AllocationType.Reserve,
                    MemoryProtectionFlags.ExecuteReadWrite);

                //Write ItemGib Function and Arguments into the previously allocated memory
                NativeWrapper.WriteProcessMemoryArray(memoryHandle, itemGibByteFunctionPtr, itemGibByteFunctionDS3);
                NativeWrapper.WriteProcessMemoryArray(memoryHandle, itemGibArgumentsPtr, itemGibArgumentsByteArray);

                //Create a new thread at the copied ItemGib function in memory

                NativeWrapper.WaitForSingleObject(
                    NativeWrapper.CreateRemoteThread(memoryHandle, itemGibByteFunctionPtr, itemGibArgumentsPtr), 30000);


                //Frees memory used by the ItemGib function and it's arguments
                NativeWrapper.VirtualFreeEx(memoryHandle, itemGibByteFunctionPtr,
                    Buffer.ByteLength(itemGibByteFunctionDS3), FreeType.PreservePlaceholder);
                NativeWrapper.VirtualFreeEx(memoryHandle, itemGibArgumentsPtr,
                    Buffer.ByteLength(itemGibArgumentsIntArray), FreeType.PreservePlaceholder);
            }
        }

        // ItemGib - ER
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
            //var MapItemMan = "48 8B 0D ?? ?? ?? ?? C7 44 24 50 FF FF FF FF";
            //var addr = "41 0F B6 F9 41 8B E8"; // Find offset, then minus 0x31
        }
    }
}
