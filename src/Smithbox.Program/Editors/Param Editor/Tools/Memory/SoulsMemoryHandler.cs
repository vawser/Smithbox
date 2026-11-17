using Microsoft.Extensions.Logging;
using ProcessMemoryUtilities.Managed;
using ProcessMemoryUtilities.Native;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Memory;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace StudioCore.Editors.ParamEditor;

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
            ProcessAccessFlags.VirtualMemoryOperation | ProcessAccessFlags.QueryInformation, gameProcess.Id);

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
        // Clean up the static offset cache for this process.
        // Once the process handle is closed, cached offsets are no longer valid.
        ProcessOffsetBank.Remove(gameProcess.Id);
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

        // Read only committed, readable pages that are not guard pages. A cross-process read of a PAGE_GUARD
        // page clears its guard flag, which breaks games that use guard pages for their own protection
        // (Sekiro's Arxan); unreadable ranges stay zero and simply never match.
        var mem = ReadModuleSafely(gameProcess.MainModule.BaseAddress, memSize, out _);

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
                    //Smithbox.Log(this, $"Found AOB in memory for {offsetName}. Offset: 0x{offset:X2}");
                    return true;
                }
            }
        }

        Smithbox.LogError(this, LOC.Get("PARAM_Tools_Memory_Missing_AOB", offsetName));
        return false;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MEMORY_BASIC_INFORMATION
    {
        public nint BaseAddress;
        public nint AllocationBase;
        public uint AllocationProtect;
        public ushort PartitionId;
        public nint RegionSize;
        public uint State;
        public uint Protect;
        public uint Type;
    }

    [DllImport("kernel32", SetLastError = true)]
    private static extern nint VirtualQueryEx(nint process, nint address, out MEMORY_BASIC_INFORMATION info, nint length);

    private const uint MEM_COMMIT = 0x1000;
    private const uint PAGE_NOACCESS = 0x01;
    private const uint PAGE_GUARD = 0x100;

    /// <summary>
    /// A copy of [baseAddress, baseAddress + size) with only committed, readable, non-guard regions read.
    /// </summary>
    private byte[] ReadModuleSafely(nint baseAddress, int size, out long skippedBytes)
    {
        var mem = new byte[size];
        skippedBytes = 0;

        var end = baseAddress + size;
        var address = baseAddress;
        var infoSize = Marshal.SizeOf<MEMORY_BASIC_INFORMATION>();

        while (address < end)
        {
            if (VirtualQueryEx(memoryHandle, address, out var info, infoSize) == 0)
            {
                skippedBytes += end - address;
                break;
            }

            var regionStart = Math.Max((long)address, (long)info.BaseAddress);
            var regionEnd = Math.Min((long)end, (long)info.BaseAddress + (long)info.RegionSize);
            var length = (int)(regionEnd - regionStart);

            var readable = info.State == MEM_COMMIT
                && (info.Protect & PAGE_GUARD) == 0
                && (info.Protect & PAGE_NOACCESS) == 0
                && info.Protect != 0;

            if (readable && length > 0)
            {
                var chunk = new byte[length];
                if (ReadProcessMemory(memoryHandle, (nint)regionStart, chunk, length, out _))
                    Buffer.BlockCopy(chunk, 0, mem, (int)(regionStart - baseAddress), length);
                else
                    skippedBytes += length;
            }
            else
            {
                skippedBytes += Math.Max(length, 0);
            }

            if (regionEnd <= (long)address)
                break;
            address = (nint)regionEnd;
        }

        return mem;
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

    /// <summary>
    /// The object the per-param offsets apply to: [paramRepoPtr], followed through paramOuterPath.
    /// </summary>
    internal nint GetOuterPtr(bool is64Bit, nint paramRepoPtr, GameOffsetBaseEntry entry)
    {
        if (is64Bit)
        {
            var ptr = paramRepoPtr;
            NativeWrapper.ReadProcessMemory(memoryHandle, ptr, ref ptr);
            foreach (var outerPathPart in entry.paramOuterPath)
            {
                ptr = nint.Add(ptr, outerPathPart);
                NativeWrapper.ReadProcessMemory(memoryHandle, ptr, ref ptr);
            }
            return ptr;
        }

        var ptr32 = (int)paramRepoPtr;
        NativeWrapper.ReadProcessMemory(memoryHandle, ptr32, ref ptr32);
        foreach (var outerPathPart in entry.paramOuterPath)
        {
            ptr32 = ptr32 + outerPathPart;
            NativeWrapper.ReadProcessMemory(memoryHandle, ptr32, ref ptr32);
        }
        return ptr32;
    }

    private nint GetParamPtr64Bit(nint paramRepoPtr, GameOffsetBaseEntry entry, int pOffset)
    {
        var paramPtr = GetOuterPtr(true, paramRepoPtr, entry);
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
        var ParamPtr = (int)GetOuterPtr(false, paramRepoPtr, entry);
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
        if (type is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.NR or ProjectType.AC6)
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
                    ProcessMemoryUtilities.Native.Kernel32.WaitForSingleObject(threadHandle, 30000);
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

    /// <summary>
    /// Module-relative offset of an Item Gib address: by AOB when one is set, else the hard offset.
    /// </summary>
    internal bool TryResolveItemGiveAddress(string name, ItemGiveAddress address, out int offset, out bool fromAob)
    {
        fromAob = false;
        if (address.Aob != null && TryFindOffsetFromAOB($"ItemGive_{name}_{address.Aob}", address.Aob, address.AobRelativeOffsets, out offset))
        {
            fromAob = true;
            return true;
        }

        offset = address.Offset ?? 0;
        return address.Offset != null;
    }

    /// <summary>
    /// The value of the manager singleton (0 while the game has not created it).
    /// </summary>
    internal nint ReadManager(int managerOffset)
    {
        nint manager = 0;
        ReadProcessMemory(GetBaseAddress() + managerOffset, ref manager);
        return manager;
    }

    /// <summary>
    /// [module + managerOffset], then each hop of path (add, read a pointer). 0 when any step is null.
    /// </summary>
    internal nint ReadPointerPath(int managerOffset, int[] path)
    {
        var ptr = ReadManager(managerOffset);
        foreach (var hop in path)
        {
            if (ptr == 0)
                return 0;

            nint next = 0;
            ReadProcessMemory(ptr + hop, ref next);
            ptr = next;
        }
        return ptr;
    }

    /// <summary>
    /// Calls the game's apply-SpEffect function: func(chr, spEffectId, flag).
    /// </summary>
    internal void PlayerSpEffectGive(int funcOffset, nint chr, int spEffectId, int flag, float? xmm2 = null)
    {
        var func = (long)(GetBaseAddress() + funcOffset);

        var code = new List<byte>();
        void Imm64(long v) => code.AddRange(BitConverter.GetBytes(v));

        code.AddRange(new byte[] { 0x48, 0x83, 0xEC, 0x28 });                   // sub rsp, 28h
        code.AddRange(new byte[] { 0x48, 0xB9 }); Imm64(chr);                   // mov rcx, chr
        code.Add(0xBA); code.AddRange(BitConverter.GetBytes(spEffectId));       // mov edx, spEffectId
        code.AddRange(new byte[] { 0x41, 0xB8 }); code.AddRange(BitConverter.GetBytes(flag)); // mov r8d, flag
        if (xmm2 is { } f)
        {
            code.Add(0xB8); code.AddRange(BitConverter.GetBytes(f));            // mov eax, float bits
            code.AddRange(new byte[] { 0x66, 0x0F, 0x6E, 0xD0 });               // movd xmm2, eax
        }
        code.AddRange(new byte[] { 0x48, 0xB8 }); Imm64(func);                  // mov rax, func
        code.AddRange(new byte[] { 0xFF, 0xD0 });                               // call rax
        code.AddRange(new byte[] { 0x48, 0x83, 0xC4, 0x28 });                   // add rsp, 28h
        code.Add(0xC3);                                                         // ret

        ExecuteCode(code.ToArray());
    }

    /// <summary>
    /// Calls the game's give-item function: func([manager], itemTable, status, r9, stack args...).
    /// Items beyond the function's per-call limit are given in further calls.
    /// </summary>
    /// <param name="prepares">Per param: module offsets of the lookup and prepare functions (Descriptor layout only).</param>
    internal void PlayerItemGive(GameOffsetBaseEntry entry, int funcOffset, int managerOffset, List<ItemGiveEntry> items,
        Dictionary<string, (int Lookup, int Prepare)> prepares = null)
    {
        if (entry.ItemGiveLayout is not { } layout || !items.Any())
            return;

        prepares ??= new();
        const int descriptorSize = 0x50;
        const int rowRefSize = 0x20;

        var funcPtr = GetBaseAddress() + funcOffset;
        var managerPtr = GetBaseAddress() + managerOffset;

        // Arguments marked "scratch" each get a 0x40-byte block whose first int is -1.
        const int scratchSize = 0x40;
        var scratchArgs = new[] { entry.ItemGiveR9 }.Concat(entry.ItemGiveStackArgs).Count(a => a.Scratch);

        foreach (var batch in items.Chunk(entry.ItemGiveMaxEntries))
        {
            var itemTable = BuildItemTable(layout, entry.ItemGiveMaxEntries, entry.ItemGiveSource, batch);
            var status = layout is ItemGiveLayout.Gem or ItemGiveLayout.Descriptor
                ? new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } // { -1, 0, 0 }
                : new byte[16];                                                             // zeros

            var scratch = new byte[Math.Max(1, scratchArgs) * scratchSize];
            for (var i = 0; i < scratchArgs; i++)
                BitConverter.GetBytes(-1).CopyTo(scratch, i * scratchSize);

            var itemTablePtr = AllocAndWrite(itemTable);
            var statusPtr = AllocAndWrite(status);
            var scratchPtr = AllocAndWrite(scratch);
            var rowRefPtr = AllocAndWrite(new byte[batch.Length * rowRefSize]);

            if (itemTablePtr != nint.Zero && statusPtr != nint.Zero && scratchPtr != nint.Zero && rowRefPtr != nint.Zero)
            {
                var nextScratch = 0;
                long Resolve(ItemGiveArg a) => a.Scratch ? (long)scratchPtr + scratchSize * nextScratch++ : a.Value;

                // Prepare steps, per descriptor: lookup(&rowRef, rowId); prepare(&rowRef, &descriptor);
                // then the designer's chosen values written into the prepared descriptor.
                var preSteps = new List<ItemGiveStep>();
                if (layout == ItemGiveLayout.Descriptor)
                {
                    for (var i = 0; i < batch.Length; i++)
                    {
                        if (batch[i].Param == null || !prepares.TryGetValue(batch[i].Param, out var p))
                            continue;

                        var rowRef = (long)rowRefPtr + i * rowRefSize;
                        var descriptor = (long)itemTablePtr + 8 + i * descriptorSize;
                        preSteps.Add(ItemGiveStep.Call((long)GetBaseAddress() + p.Lookup, rowRef, batch[i].Id & 0x0FFFFFFF));
                        preSteps.Add(ItemGiveStep.Call((long)GetBaseAddress() + p.Prepare, rowRef, descriptor));

                        foreach (var (offset, value) in batch[i].Overrides ?? Array.Empty<(int, int)>())
                        {
                            if (offset >= 0 && offset + 4 <= descriptorSize)
                                preSteps.Add(ItemGiveStep.Write(descriptor + offset, value));
                        }
                    }
                }

                var r9 = Resolve(entry.ItemGiveR9);
                var stackArgs = entry.ItemGiveStackArgs.Select(Resolve).ToArray();
                ExecuteCode(BuildCallStub(funcPtr, managerPtr, itemTablePtr, statusPtr, r9, stackArgs, preSteps));
            }

            if (itemTablePtr != nint.Zero) NativeWrapper.VirtualFreeEx(memoryHandle, itemTablePtr, 0, FreeType.Release);
            if (statusPtr != nint.Zero) NativeWrapper.VirtualFreeEx(memoryHandle, statusPtr, 0, FreeType.Release);
            if (scratchPtr != nint.Zero) NativeWrapper.VirtualFreeEx(memoryHandle, scratchPtr, 0, FreeType.Release);
            if (rowRefPtr != nint.Zero) NativeWrapper.VirtualFreeEx(memoryHandle, rowRefPtr, 0, FreeType.Release);
        }
    }

    private static byte[] BuildItemTable(ItemGiveLayout layout, int maxEntries, int source, ItemGiveEntry[] batch)
    {
        if (layout == ItemGiveLayout.Descriptor)
            return BuildDescriptorTable(maxEntries, batch);

        // Durability: { int count; { id, quantity, durability }[count] }
        // Basic:      { int count; { id, quantity, -1 }[maxEntries] }
        // Gem:        { int count; { id, quantity, -1, gemId }[maxEntries] }
        // Source:     { int count; { id, quantity, -1, source }[maxEntries] }
        var wide = layout is ItemGiveLayout.Gem or ItemGiveLayout.Source;
        var entrySize = wide ? 16 : 12;
        var tableEntries = layout == ItemGiveLayout.Durability ? batch.Length : maxEntries;
        var table = new byte[4 + tableEntries * entrySize];

        BitConverter.GetBytes(batch.Length).CopyTo(table, 0);
        for (var i = 0; i < batch.Length; i++)
        {
            var o = 4 + i * entrySize;
            BitConverter.GetBytes(batch[i].Id).CopyTo(table, o);
            BitConverter.GetBytes(batch[i].Quantity).CopyTo(table, o + 4);
            if (wide)
            {
                BitConverter.GetBytes(-1).CopyTo(table, o + 8);
                BitConverter.GetBytes(layout == ItemGiveLayout.Gem ? batch[i].GemId : source).CopyTo(table, o + 12);
            }
            else
            {
                BitConverter.GetBytes(layout == ItemGiveLayout.Basic ? -1 : batch[i].Durability).CopyTo(table, o + 8);
            }
        }
        return table;
    }

    /// <summary>
    /// NR: { byte count; pad[7]; ItemDescriptor[] } with each 0x50-byte descriptor at its constructor defaults
    /// (nightreign.exe 1406C0C10 + 14056DED0), then the ID and u16 quantity. The table is padded to 0x100
    /// so the fields after the descriptor array stay zero.
    /// </summary>
    private static byte[] BuildDescriptorTable(int maxEntries, ItemGiveEntry[] batch)
    {
        const int descriptorSize = 0x50;
        var table = new byte[Math.Max(0x100, 8 + maxEntries * descriptorSize)];
        table[0] = (byte)batch.Length;

        for (var i = 0; i < batch.Length; i++)
        {
            var d = 8 + i * descriptorSize;
            void I32(int o, int v) => BitConverter.GetBytes(v).CopyTo(table, d + o);
            void I64(int o, long v) => BitConverter.GetBytes(v).CopyTo(table, d + o);

            I64(0x00, -1);
            I64(0x08, -1);
            I32(0x10, -1);
            I64(0x14, -1);
            table[d + 0x1F] = 0xFF;   // +1C word 0, +1E flags 0
            I64(0x28, -1);
            I64(0x30, -1);
            I64(0x38, -1);
            I32(0x48, unchecked((int)0xFFFF0000));

            I32(0x00, batch[i].Id);
            BitConverter.GetBytes((ushort)Math.Clamp(batch[i].Quantity, 0, ushort.MaxValue)).CopyTo(table, d + 0x48);
        }
        return table;
    }

    private static byte[] BuildCallStub(nint funcPtr, nint managerPtr, nint itemTablePtr, nint statusPtr, long r9, long[] stackArgs,
        List<ItemGiveStep> preSteps = null)
    {
        // Shadow space + stack arguments, keeping rsp 16-byte aligned at the call
        // (the thread starts with rsp = 16n + 8).
        var frame = 0x20 + 8 * stackArgs.Length;
        if ((frame + 8) % 16 != 0)
            frame += 8;

        var code = new List<byte>();
        void Imm64(long v) => code.AddRange(BitConverter.GetBytes(v));

        code.AddRange(new byte[] { 0x48, 0x83, 0xEC, (byte)frame });           // sub rsp, frame

        // Prepare steps first: the stack arguments are written after them, so a callee using its
        // home space cannot overwrite them.
        foreach (var step in preSteps ?? new())
        {
            if (step.IsWrite)
            {
                code.AddRange(new byte[] { 0x48, 0xB8 }); Imm64(step.A);        // mov rax, address
                code.AddRange(new byte[] { 0xC7, 0x00 });                       // mov dword ptr [rax], imm32
                code.AddRange(BitConverter.GetBytes((int)step.B));
                continue;
            }

            code.AddRange(new byte[] { 0x48, 0xB9 }); Imm64(step.B);            // mov rcx, imm64
            code.AddRange(new byte[] { 0x48, 0xBA }); Imm64(step.C);            // mov rdx, imm64
            code.AddRange(new byte[] { 0x48, 0xB8 }); Imm64(step.A);            // mov rax, func
            code.AddRange(new byte[] { 0xFF, 0xD0 });                           // call rax
        }

        for (var i = 0; i < stackArgs.Length; i++)
        {
            code.AddRange(new byte[] { 0x48, 0xB8 }); Imm64(stackArgs[i]);      // mov rax, arg
            code.AddRange(new byte[] { 0x48, 0x89, 0x44, 0x24, (byte)(0x20 + 8 * i) }); // mov [rsp+20h+8i], rax
        }
        code.AddRange(new byte[] { 0x48, 0xB9 }); Imm64(managerPtr);            // mov rcx, &manager
        code.AddRange(new byte[] { 0x48, 0x8B, 0x09 });                         // mov rcx, [rcx]
        code.AddRange(new byte[] { 0x48, 0xBA }); Imm64(itemTablePtr);          // mov rdx, itemTable
        code.AddRange(new byte[] { 0x49, 0xB8 }); Imm64(statusPtr);             // mov r8, status
        if (r9 == 0)
            code.AddRange(new byte[] { 0x45, 0x31, 0xC9 });                     // xor r9d, r9d
        else
        {
            code.AddRange(new byte[] { 0x49, 0xB9 }); Imm64(r9);                // mov r9, imm64
        }
        code.AddRange(new byte[] { 0x48, 0xB8 }); Imm64(funcPtr);               // mov rax, func
        code.AddRange(new byte[] { 0xFF, 0xD0 });                               // call rax
        code.AddRange(new byte[] { 0x48, 0x83, 0xC4, (byte)frame });           // add rsp, frame
        code.Add(0xC3);                                                         // ret
        return code.ToArray();
    }

    private nint AllocAndWrite(byte[] data)
    {
        var ptr = NativeWrapper.VirtualAllocEx(memoryHandle, nint.Zero, data.Length,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ReadWrite);
        if (ptr != nint.Zero)
        {
            WriteProcessMemoryArray(ptr, data);
        }
        return ptr;
    }
}

/// <param name="Overrides">Values written into the prepared descriptor: (descriptor offset, int value).</param>
public record struct ItemGiveEntry(int Id, int Quantity, int Durability, int GemId, string Param = null,
    (int Offset, int Value)[] Overrides = null);

/// <summary>
/// A step of the injected code before the give: a call func(rcx, rdx), or an int write to an address.
/// </summary>
public readonly record struct ItemGiveStep(bool IsWrite, long A, long B, long C)
{
    public static ItemGiveStep Call(long func, long rcx, long rdx) => new(false, func, rcx, rdx);
    public static ItemGiveStep Write(long address, int value) => new(true, address, value, 0);
}
