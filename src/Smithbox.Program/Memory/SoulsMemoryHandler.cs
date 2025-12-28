using Microsoft.Extensions.Logging;
using ProcessMemoryUtilities.Managed;
using ProcessMemoryUtilities.Native;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Utilities;
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

    internal nint GetParamPtr(bool is64Bit, nint paramRepoPtr, GameOffsetBaseEntry entry, int pOffset)
    {
        if (is64Bit)
        {
            return GetParamPtr64Bit(paramRepoPtr, entry, pOffset);
        }

        return GetParamPtr32Bit(paramRepoPtr, entry, pOffset);
    }

    private nint GetParamPtr64Bit(nint paramRepoPtr, GameOffsetBaseEntry entry, int pOffset)
    {
        var paramPtr = paramRepoPtr;
        NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        paramPtr = nint.Add(paramPtr, pOffset);
        NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        foreach (var innerPathPart in entry.paramInnerPath)
        {
            paramPtr = nint.Add(paramPtr, innerPathPart);
            NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        }

        return paramPtr;
    }

    private nint GetParamPtr32Bit(nint paramRepoPtr, GameOffsetBaseEntry entry, int pOffset)
    {
        var ParamPtr = (int)paramRepoPtr;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr, ref ParamPtr);
        ParamPtr = ParamPtr + pOffset;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr, ref ParamPtr);
        foreach (var innerPathPart in entry.paramInnerPath)
        {
            ParamPtr = ParamPtr + innerPathPart;
            NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr, ref ParamPtr);
        }

        return ParamPtr;
    }

    internal int GetRowCount(ProjectType type, GameOffsetBaseEntry entry, nint paramPtr)
    {
        if (type is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
        {
            return GetRowCountInt(entry, paramPtr);
        }

        return GetRowCountShort(entry, paramPtr);
    }

    private int GetRowCountInt(GameOffsetBaseEntry entry, nint ParamPtr)
    {
        var buffer = 0;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr + entry.paramCountOffset, ref buffer);
        return buffer;
    }

    private int GetRowCountShort(GameOffsetBaseEntry entry, nint ParamPtr)
    {
        short buffer = 0;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr + entry.paramCountOffset, ref buffer);
        return buffer;
    }

    internal nint GetToRowPtr(GameOffsetBaseEntry entry, nint paramPtr)
    {
        paramPtr = nint.Add(paramPtr, entry.paramDataOffset);
        return paramPtr;
    }


    public void ExecuteCode(byte[] code)
    {
        var codeSize = code.Length;
        var codeAddress = NativeWrapper.VirtualAllocEx(memoryHandle, nint.Zero, codeSize,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ExecuteReadWrite);

        if (codeAddress != nint.Zero)
        {
            if (WriteProcessMemoryArray(codeAddress, code))
            {
                var threadHandle = NativeWrapper.CreateRemoteThread(memoryHandle, nint.Zero, 0, codeAddress,
                    nint.Zero, ThreadCreationFlags.Immediately, out var threadId);
                if (threadHandle != nint.Zero)
                {
                    Kernel32.WaitForSingleObject(threadHandle, 30000);
                }
            }

            NativeWrapper.VirtualFreeEx(memoryHandle, codeAddress, 0, FreeType.Release);
        }
    }

    public void RequestReloadChr(string chrName)
    {
        var chrNameBytes = Encoding.Unicode.GetBytes(chrName);
        var argSize = chrNameBytes.Length;
        var argAddress = NativeWrapper.VirtualAllocEx(memoryHandle, nint.Zero, argSize,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ReadWrite);

        if (argAddress == nint.Zero) return;

        WriteProcessMemoryArray(argAddress, chrNameBytes);

        byte[] code =
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

        var addrBytes = BitConverter.GetBytes((long)argAddress);
        Buffer.BlockCopy(addrBytes, 0, code, 2, addrBytes.Length);

        var memoryWriteBuffer = true;
        WriteProcessMemory(gameProcess.MainModule.BaseAddress + 0x4768F7F, ref memoryWriteBuffer);

        ExecuteCode(code);

        NativeWrapper.VirtualFreeEx(memoryHandle, argAddress, 0, FreeType.Release);
    }

    internal void PlayerItemGive_DS3(List<int> itemIds, int itemQuantity = 1, int itemDurability = -1)
    {
        if (Editor.Project.ProjectType is not ProjectType.DS3 || !itemIds.Any())
            return;

        var intListProcessing = new List<int> { 0, 0, 0, 0, itemIds.Count };
        foreach (var itemId in itemIds)
        {
            intListProcessing.Add(itemId);
            intListProcessing.Add(itemQuantity);
            intListProcessing.Add(itemDurability);
        }

        var itemGibArgumentsByteArray = new byte[Buffer.ByteLength(intListProcessing.ToArray())];
        Buffer.BlockCopy(intListProcessing.ToArray(), 0, itemGibArgumentsByteArray, 0, itemGibArgumentsByteArray.Length);

        var argAddress = NativeWrapper.VirtualAllocEx(memoryHandle, nint.Zero, itemGibArgumentsByteArray.Length,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ReadWrite);

        if (argAddress == nint.Zero) return;

        WriteProcessMemoryArray(argAddress, itemGibArgumentsByteArray);

        byte[] code =
        {
            0x48, 0x83, 0xEC, 0x48, // sub rsp, 48h
            0x48, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rcx, p_arg_struct
            0x48, 0x8B, 0x09, // mov rcx, [rcx]
            0x48, 0x8D, 0x51, 0x10, // lea rdx, [rcx+10h]
            0x48, 0xB8, 0x78, 0x55, 0x9A, 0x42, 0x01, 0x00, 0x00, 0x00, // mov rax, [GAME.EXE+29A5578]
            0x48, 0x8B, 0x00, // mov rax, [rax]
            0xFF, 0x15, 0x02, 0x00, 0x00, 0x00, 0xEB, 0x08, 0x90, 0x36, // call qword ptr [rip+8] | jmp over address
            0x85, 0x40, 0x01, 0x00, 0x00, 0x00, // address of ItemGibFunc
            0x48, 0x83, 0xC4, 0x48, // add rsp, 48h
            0xC3 // ret
        };

        var addrBytes = BitConverter.GetBytes((long)argAddress);
        Buffer.BlockCopy(addrBytes, 0, code, 6, addrBytes.Length);

        ExecuteCode(code);

        NativeWrapper.VirtualFreeEx(memoryHandle, argAddress, 0, FreeType.Release);
    }

    internal void PlayerItemGive_ER(GameOffsetBaseEntry entry, List<int> itemIds, int itemQuantity = 1, int gemId = -1)
    {
        if (Editor.Project.ProjectType is not ProjectType.ER || !itemIds.Any())
            return;

        const int maxItems = 10;
        var itemCount = Math.Min(itemIds.Count, maxItems);

        var itemTable = new byte[4 + maxItems * 16];
        BitConverter.GetBytes(itemCount).CopyTo(itemTable, 0);
        for (var i = 0; i < itemCount; i++)
        {
            var itemId = itemIds[i];
            BitConverter.GetBytes(itemId).CopyTo(itemTable, 4 + i * 16 + 0);
            BitConverter.GetBytes(itemQuantity).CopyTo(itemTable, 4 + i * 16 + 4);
            BitConverter.GetBytes(-1).CopyTo(itemTable, 4 + i * 16 + 8);
            BitConverter.GetBytes(gemId).CopyTo(itemTable, 4 + i * 16 + 12);
        }

        var itemTablePtr = NativeWrapper.VirtualAllocEx(memoryHandle, 0, itemTable.Length,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ReadWrite);
        if (itemTablePtr == nint.Zero) return;
        WriteProcessMemoryArray(itemTablePtr, itemTable);

        var statusStruct = new int[3] { -1, 0, 0 };
        var statusStructBytes = new byte[12];
        Buffer.BlockCopy(statusStruct, 0, statusStructBytes, 0, 12);
        var statusStructPtr = NativeWrapper.VirtualAllocEx(memoryHandle, 0, 12,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ReadWrite);
        if (statusStructPtr == nint.Zero)
        {
            NativeWrapper.VirtualFreeEx(memoryHandle, itemTablePtr, 0, FreeType.Release);
            return;
        }
        WriteProcessMemoryArray(statusStructPtr, statusStructBytes);

        var itemGiveFuncPtr = GetBaseAddress() + entry.ERItemGiveFuncOffset ?? throw new InvalidOperationException("ERItemGiveFuncOffset is not set in GameOffsetBaseEntry.");
        var mapItemManPtr = GetBaseAddress() + entry.ERMapItemManOffset ?? throw new InvalidOperationException("ERMapItemManOffset is not set in GameOffsetBaseEntry.");

        var argBuffer = new byte[32];
        BitConverter.GetBytes((long)mapItemManPtr).CopyTo(argBuffer, 0);
        BitConverter.GetBytes((long)itemTablePtr).CopyTo(argBuffer, 8);
        BitConverter.GetBytes((long)statusStructPtr).CopyTo(argBuffer, 16);
        BitConverter.GetBytes((long)itemGiveFuncPtr).CopyTo(argBuffer, 24);

        var argBufferPtr = NativeWrapper.VirtualAllocEx(memoryHandle, nint.Zero, argBuffer.Length,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ReadWrite);
        if (argBufferPtr == nint.Zero)
        {
            NativeWrapper.VirtualFreeEx(memoryHandle, itemTablePtr, 0, FreeType.Release);
            NativeWrapper.VirtualFreeEx(memoryHandle, statusStructPtr, 0, FreeType.Release);
            return;
        }
        WriteProcessMemoryArray(argBufferPtr, argBuffer);

        byte[] code =
        {
            0x48, 0x83, 0xEC, 0x28,                                     // sub rsp, 28h
            0x48, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // mov rax, p_arg_struct
            0x48, 0x8B, 0x48, 0x00,                                     // mov rcx, [rax+0]
            0x48, 0x8B, 0x09,                                           // mov rcx, [rcx]
            0x48, 0x8B, 0x50, 0x08,                                     // mov rdx, [rax+8]
            0x4C, 0x8B, 0x40, 0x10,                                     // mov r8, [rax+16]
            0x4D, 0x31, 0xC9,                                           // xor r9, r9
            0x48, 0x8B, 0x40, 0x18,                                     // mov rax, [rax+24]
            0xFF, 0xD0,                                                 // call rax
            0x48, 0x83, 0xC4, 0x28,                                     // add rsp, 28h
            0xC3                                                        // ret
        };

        var addrBytes = BitConverter.GetBytes((long)argBufferPtr);
        Buffer.BlockCopy(addrBytes, 0, code, 6, addrBytes.Length);

        ExecuteCode(code);

        NativeWrapper.VirtualFreeEx(memoryHandle, itemTablePtr, 0, FreeType.Release);
        NativeWrapper.VirtualFreeEx(memoryHandle, statusStructPtr, 0, FreeType.Release);
        NativeWrapper.VirtualFreeEx(memoryHandle, argBufferPtr, 0, FreeType.Release);
    }
}
