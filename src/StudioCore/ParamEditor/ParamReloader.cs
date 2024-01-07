using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using ProcessMemoryUtilities.Managed;
using ProcessMemoryUtilities.Native;
using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.ParamEditor;

internal class ParamReloader
{
    public static uint numberOfItemsToGive = 1;
    public static uint upgradeLevelItemToGive;

    private static readonly List<GameType> _supportedGames = new()
    {
        GameType.DarkSoulsPTDE,
        GameType.DarkSoulsRemastered,
        GameType.Sekiro,
        GameType.DarkSoulsIII,
        GameType.EldenRing,
        GameType.ArmoredCoreVI
    };

    public static bool GameIsSupported(GameType gameType)
    {
        return _supportedGames.Contains(gameType);
    }

    public static bool CanReloadMemoryParams(ParamBank bank, ProjectSettings projectSettings)
    {
        if (projectSettings != null && GameIsSupported(projectSettings.GameType) && bank.IsLoadingParams == false)
        {
            return true;
        }

        return false;
    }

    public static void ReloadMemoryParam(ParamBank bank, AssetLocator loc, string paramName)
    {
        if (paramName != null)
        {
            ReloadMemoryParams(bank, loc, new string[] { paramName });
        }
    }

    public static void ReloadMemoryParams(ParamBank bank, AssetLocator loc, string[] paramNames)
    {
        TaskManager.Run(new TaskManager.LiveTask("Param - Hot Reload", TaskManager.RequeueType.WaitThenRequeue,
            true, () =>
            {
                GameOffsets offsets = GetGameOffsets(loc);
                if (offsets == null)
                {
                    return;
                }

                Process[] processArray = Process.GetProcessesByName(offsets.exeName);
                if (!processArray.Any())
                {
                    processArray = Process.GetProcessesByName(offsets.exeName.Replace(".exe", ""));
                }

                if (processArray.Any())
                {
                    SoulsMemoryHandler memoryHandler = new(processArray.First());

                    ReloadMemoryParamsThreads(bank, offsets, paramNames, memoryHandler);
                    memoryHandler.Terminate();
                }
                else
                {
                    throw new Exception("Unable to find running game");
                }
            }));
    }

    private static void ReloadMemoryParamsThreads(ParamBank bank, GameOffsets offsets, string[] paramNames,
        SoulsMemoryHandler handler)
    {
        List<Task> tasks = new();
        foreach (var param in paramNames)
        {
            if ((offsets.type is GameType.DarkSoulsPTDE or GameType.DarkSoulsRemastered) &&
                param == "ThrowParam" && offsets.paramOffsets.ContainsKey(param))
            {
                // DS1 ThrowParam requires an additional offset.
                tasks.Add(new Task(() =>
                    WriteMemoryPARAM(offsets, bank.Params[param], offsets.paramOffsets[param], handler, 0x41C0)));
            }
            else if (param != null && offsets.paramOffsets.ContainsKey(param))
            {
                tasks.Add(new Task(() =>
                    WriteMemoryPARAM(offsets, bank.Params[param], offsets.paramOffsets[param], handler)));
            }
        }

        foreach (Task task in tasks)
        {
            task.Start();
        }

        foreach (Task task in tasks)
        {
            task.Wait();
        }
    }

    public static void GiveItemMenu(AssetLocator loc, List<Param.Row> rowsToGib, string param)
    {
        GameOffsets offsets = GetGameOffsets(loc);

        if (!offsets.itemGibOffsets.ContainsKey(param))
        {
            return;
        }

        if (ImGui.MenuItem("Spawn Selected Items In Game"))
        {
            GiveItem(offsets, rowsToGib, param, param == "EquipParamGoods" ? (int)numberOfItemsToGive : 1,
                param == "EquipParamWeapon" ? (int)upgradeLevelItemToGive : 0);
        }

        if (param == "EquipParamGoods")
        {
            var itemsNum = numberOfItemsToGive.ToString();
            ImGui.Indent();
            ImGui.Text("Number of Spawned Items");
            ImGui.SameLine();
            if (ImGui.InputText("##Number of Spawned Items", ref itemsNum, 2))
            {
                if (uint.TryParse(itemsNum, out var result) && result != 0)
                {
                    numberOfItemsToGive = result;
                }
            }
        }
        else if (param == "EquipParamWeapon")
        {
            ImGui.Text("Spawned Weapon Level");
            ImGui.SameLine();
            var weaponLevel = upgradeLevelItemToGive.ToString();
            if (ImGui.InputText("##Spawned Weapon Level", ref weaponLevel, 2))
            {
                if (uint.TryParse(weaponLevel, out var result) && result < 11)
                {
                    upgradeLevelItemToGive = result;
                }
            }
        }

        ImGui.Unindent();
    }

    private static void GiveItem(GameOffsets offsets, List<Param.Row> rowsToGib, string studioParamType,
        int itemQuantityReceived, int upgradeLevelItemToGive = 0)
    {
        if (rowsToGib.Any())
        {
            Process[] processArray = Process.GetProcessesByName("DarkSoulsIII");
            if (processArray.Any())
            {
                SoulsMemoryHandler memoryHandler = new(processArray.First());

                memoryHandler.PlayerItemGive(offsets, rowsToGib, studioParamType, itemQuantityReceived, -1,
                    upgradeLevelItemToGive);

                memoryHandler.Terminate();
            }
        }
    }

    private static void WriteMemoryPARAM(GameOffsets offsets, Param param, int paramOffset,
        SoulsMemoryHandler memoryHandler, int paramBaseExtraOffset = 0)
    {
        if (offsets.ParamBaseAobPattern != null)
        {
            if (memoryHandler.TryFindOffsetFromAOB("ParamBase", offsets.ParamBaseAobPattern, offsets.ParamBaseAobRelativeOffsets, out int paramBase))
            {
                var soloParamRepositoryPtr = IntPtr.Add(memoryHandler.GetBaseAddress(), paramBase + paramBaseExtraOffset);
                var BasePtr = memoryHandler.GetParamPtr(soloParamRepositoryPtr, offsets, paramOffset);
                WriteMemoryPARAM(offsets, param, BasePtr, memoryHandler);
            }
        }
        else
        {
            var soloParamRepositoryPtr = IntPtr.Add(memoryHandler.GetBaseAddress(), offsets.ParamBaseOffset + paramBaseExtraOffset);
            var BasePtr = memoryHandler.GetParamPtr(soloParamRepositoryPtr, offsets, paramOffset);
            WriteMemoryPARAM(offsets, param, BasePtr, memoryHandler);
        }
    }

    private static void WriteMemoryPARAM(GameOffsets offsets, Param param, IntPtr BasePtr,
        SoulsMemoryHandler memoryHandler)
    {
        var BaseDataPtr = memoryHandler.GetToRowPtr(offsets, BasePtr);
        var RowCount = memoryHandler.GetRowCount(offsets, BasePtr);

        IntPtr DataSectionPtr;

        var RowId = 0;
        var rowPtr = 0;

        // Track how many times this ID has been defined for the purposes of handing dupe ID row names.
        Dictionary<int, Queue<Param.Row>> rowDictionary = GetRowQueueDictionary(param);

        for (var i = 0; i < RowCount; i++)
        {
            memoryHandler.ReadProcessMemory(BaseDataPtr, ref RowId);
            memoryHandler.ReadProcessMemory(BaseDataPtr + offsets.rowPointerOffset, ref rowPtr);
            if (RowId < 0 || rowPtr < 0)
            {
                BaseDataPtr += offsets.rowHeaderSize;
                continue;
            }

            DataSectionPtr = IntPtr.Add(BasePtr, rowPtr);

            BaseDataPtr += offsets.rowHeaderSize;

            if (rowDictionary.TryGetValue(RowId, out Queue<Param.Row> queue)
                && queue.TryDequeue(out Param.Row row))
            {
                WriteMemoryRow(row, DataSectionPtr, memoryHandler);
            }
            else
            {
                throw new InvalidOperationException("Param row in memory cannot be found in editor. Try saving params and restarting game.");
            }
        }
    }

    private static void WriteMemoryRow(Param.Row row, IntPtr RowDataSectionPtr, SoulsMemoryHandler memoryHandler)
    {
        var offset = 0;
        var bitFieldPos = 0;
        BitArray bits = null;

        foreach (Param.Column cell in row.Columns)
        {
            offset += WriteMemoryCell(row[cell], RowDataSectionPtr + offset, ref bitFieldPos, ref bits,
                memoryHandler);
        }
    }

    private static int WriteMemoryCell(Param.Cell cell, IntPtr CellDataPtr, ref int bitFieldPos, ref BitArray bits,
        SoulsMemoryHandler memoryHandler)
    {
        PARAMDEF.DefType displayType = cell.Def.DisplayType;
        // If this can be simplified, that would be ideal. Currently we have to reconcile DefType, a numerical size in bits, and the Type used for the bitField array
        if (cell.Def.BitSize != -1)
        {
            if (displayType == PARAMDEF.DefType.u8 || displayType == PARAMDEF.DefType.dummy8)
            {
                if (bitFieldPos == 0)
                {
                    bits = new BitArray(8);
                }

                return WriteBitArray(cell, CellDataPtr, ref bitFieldPos, ref bits, memoryHandler, false);
            }

            if (displayType == PARAMDEF.DefType.u16)
            {
                if (bitFieldPos == 0)
                {
                    bits = new BitArray(16);
                }

                return WriteBitArray(cell, CellDataPtr, ref bitFieldPos, ref bits, memoryHandler, false);
            }

            if (displayType == PARAMDEF.DefType.u32)
            {
                if (bitFieldPos == 0)
                {
                    bits = new BitArray(32);
                }

                return WriteBitArray(cell, CellDataPtr, ref bitFieldPos, ref bits, memoryHandler, false);
            }
        }
        else if (bits != null && bitFieldPos != 0)
        {
            var offset = WriteBitArray(null, CellDataPtr, ref bitFieldPos, ref bits, memoryHandler, true);
            return offset +
                   WriteMemoryCell(cell, CellDataPtr + offset, ref bitFieldPos, ref bits,
                       memoryHandler); //should recomplete current cell
        }

        if (displayType == PARAMDEF.DefType.f32)
        {
            var valueRead = 0f;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToSingle(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(float);
        }

        if (displayType == PARAMDEF.DefType.s32)
        {
            var valueRead = 0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToInt32(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(Int32);
        }

        if (displayType == PARAMDEF.DefType.s16)
        {
            short valueRead = 0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToInt16(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(Int16);
        }

        if (displayType == PARAMDEF.DefType.s8)
        {
            sbyte valueRead = 0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToSByte(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(sbyte);
        }

        if (displayType == PARAMDEF.DefType.u32)
        {
            uint valueRead = 0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToUInt32(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(UInt32);
        }

        if (displayType == PARAMDEF.DefType.u16)
        {
            ushort valueRead = 0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToUInt16(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(UInt16);
        }

        if (displayType == PARAMDEF.DefType.u8)
        {
            byte valueRead = 0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToByte(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(byte);
        }

        if (displayType == PARAMDEF.DefType.dummy8 || displayType == PARAMDEF.DefType.fixstr ||
            displayType == PARAMDEF.DefType.fixstrW)
        {
            return cell.Def.ArrayLength * (displayType == PARAMDEF.DefType.fixstrW ? 2 : 1);
        }

        throw new Exception("Unexpected Field Type");
    }

    private static int WriteBitArray(Param.Cell? cell, IntPtr CellDataPtr, ref int bitFieldPos, ref BitArray bits,
        SoulsMemoryHandler memoryHandler, bool flushBits)
    {
        if (!flushBits)
        {
            if (cell == null)
            {
                throw new ArgumentException();
            }

            BitArray cellValueBitArray = null;
            if (bits.Count == 8)
            {
                cellValueBitArray = new BitArray(BitConverter.GetBytes((byte)cell.Value.Value << bitFieldPos));
            }
            else if (bits.Count == 16)
            {
                cellValueBitArray = new BitArray(BitConverter.GetBytes((ushort)cell.Value.Value << bitFieldPos));
            }
            else if (bits.Count == 32)
            {
                cellValueBitArray = new BitArray(BitConverter.GetBytes((uint)cell.Value.Value << bitFieldPos));
            }
            else
            {
                throw new Exception("Unknown bitfield length");
            }

            for (var i = 0; i < cell.Value.Def.BitSize; i++)
            {
                bits.Set(bitFieldPos, cellValueBitArray[bitFieldPos]);
                bitFieldPos++;
            }
        }

        if (bitFieldPos == bits.Count || flushBits)
        {
            byte valueRead = 0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);
            var bitField = new byte[bits.Count / 8];
            bits.CopyTo(bitField, 0);
            if (bits.Count == 8)
            {
                var bitbuffer = bitField[0];
                if (valueRead != bitbuffer)
                {
                    memoryHandler.WriteProcessMemory(CellDataPtr, ref bitbuffer);
                }
            }
            else if (bits.Count == 16)
            {
                var bitbuffer = BitConverter.ToUInt16(bitField, 0);
                if (valueRead != bitbuffer)
                {
                    memoryHandler.WriteProcessMemory(CellDataPtr, ref bitbuffer);
                }
            }
            else if (bits.Count == 32)
            {
                var bitbuffer = BitConverter.ToUInt32(bitField, 0);
                if (valueRead != bitbuffer)
                {
                    memoryHandler.WriteProcessMemory(CellDataPtr, ref bitbuffer);
                }
            }
            else
            {
                throw new Exception("Unknown bitfield length");
            }

            var advance = bits.Count / 8;
            bitFieldPos = 0;
            bits = null;
            return advance;
        }

        return 0;
    }

    private static GameOffsets GetGameOffsets(AssetLocator loc)
    {
        GameType game = loc.Type;
        if (!GameOffsets.GameOffsetBank.ContainsKey(game))
        {
            try
            {
                GameOffsets.GameOffsetBank.Add(game, new GameOffsets(game, loc));
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("Unable to create GameOffsets for param hot reloader.", LogLevel.Error,
                    TaskLogs.LogPriority.High, e);
                return null;
            }
        }

        return GameOffsets.GameOffsetBank[game];
    }

    public static string[] GetReloadableParams(AssetLocator loc)
    {
        GameOffsets offs = GetGameOffsets(loc);
        if (offs == null)
        {
            return new string[0];
        }

        return offs.paramOffsets.Keys.ToArray();
    }

    /// <summary>
    /// Returns dictionary of Row ID keys corresponding with Queue of rows, for the purpose of handling duplicate row IDs.
    /// </summary>
    private static Dictionary<int, Queue<Param.Row>> GetRowQueueDictionary(Param param)
    {
        Dictionary<int, Queue<Param.Row>> rows = new();

        foreach (var row in param.Rows)
        {
            rows.TryAdd(row.ID, new());
            rows[row.ID].Enqueue(row);
        }

        return rows;
    }
}

internal class GameOffsets
{
    internal static Dictionary<GameType, GameOffsets> GameOffsetBank = new();

    internal Dictionary<string, string> coreOffsets;
    internal string exeName;
    internal bool Is64Bit;
    internal Dictionary<string, int> itemGibOffsets;

    // Hard offset for param base. Unused if ParamBase AOB is set.
    internal int ParamBaseOffset = 0;

    // AOB for param base offset. If null, ParamBaseOffset will be used instead.
    internal string? ParamBaseAobPattern;
    internal List<(int, int)> ParamBaseAobRelativeOffsets = new();

    internal int paramCountOffset;
    internal int paramDataOffset;
    internal int[] paramInnerPath;
    internal Dictionary<string, int> paramOffsets;
    internal int rowHeaderSize;
    internal int rowPointerOffset;
    internal GameType type;

    internal GameOffsets(GameType type, AssetLocator loc)
    {
        var dir = loc.GetGameOffsetsAssetsDir();
        Dictionary<string, string> basicData = GetOffsetFile(dir + "/CoreOffsets.txt");
        exeName = basicData["exeName"];

        if (basicData.TryGetValue("paramBase", out string paramBaseStr))
        {
            ParamBaseOffset = Utils.ParseHexFromString(paramBaseStr);
        }
        basicData.TryGetValue("paramBaseAob", out ParamBaseAobPattern);

        if (basicData.TryGetValue("paramBaseAobRelativeOffset", out string paramBaseAobRelativeOffsetStr))
        {
            foreach (var relativeOffset in paramBaseAobRelativeOffsetStr.Split(','))
            {
                var split = relativeOffset.Split('/');
                ParamBaseAobRelativeOffsets.Add(new (Utils.ParseHexFromString(split[0]), Utils.ParseHexFromString(split[1])));
            }
        }

        var innerpath = basicData["paramInnerPath"].Split("/");
        paramInnerPath = new int[innerpath.Length];
        for (var i = 0; i < innerpath.Length; i++)
        {
            paramInnerPath[i] = Utils.ParseHexFromString(innerpath[i]);
        }

        paramCountOffset = Utils.ParseHexFromString(basicData["paramCountOffset"]);
        paramDataOffset = Utils.ParseHexFromString(basicData["paramDataOffset"]);
        rowPointerOffset = Utils.ParseHexFromString(basicData["rowPointerOffset"]);
        rowHeaderSize = Utils.ParseHexFromString(basicData["rowHeaderSize"]);
        paramOffsets = GetOffsetsIntFile(dir + "/ParamOffsets.txt");
        itemGibOffsets = GetOffsetsIntFile(dir + "/ItemGibOffsets.txt");
        Is64Bit = type != GameType.DarkSoulsPTDE;
        this.type = type;

        coreOffsets = basicData;
    }

    internal GameOffsets()
    { }

    private static Dictionary<string, int> GetOffsetsIntFile(string dir)
    {
        Dictionary<string, string> paramData = GetOffsetFile(dir);
        Dictionary<string, int> offsets = new();
        foreach (KeyValuePair<string, string> entry in paramData)
        {
            offsets.Add(entry.Key, Utils.ParseHexFromString(entry.Value));
        }

        return offsets;
    }

    private static Dictionary<string, string> GetOffsetFile(string dir)
    {
        var data = File.ReadAllLines(dir);
        Dictionary<string, string> values = new();
        foreach (var line in data)
        {
            var split = line.Split(":");
            values.Add(split[0], split[1]);
        }

        return values;
    }
}

public class SoulsMemoryHandler
{
    internal record RelativeOffset(int StartOffset, int EndOffset);

    // Outer dict: key = process ID. Inner dict: key = arbitrary id, value = memory offset.
    internal static Dictionary<long, Dictionary<string, int>> ProcessOffsetBank = new();

    private readonly Process gameProcess;
    private readonly Dictionary<string, int> _processOffsets;
    public IntPtr memoryHandle;

    public SoulsMemoryHandler(Process gameProcess)
    {
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

    public IntPtr GetBaseAddress()
    {
        return gameProcess.MainModule.BaseAddress;
    }

    public void Terminate()
    {
        NativeWrapper.CloseHandle(memoryHandle);
        memoryHandle = 0;
    }

    [DllImport("kernel32", EntryPoint = "ReadProcessMemory")]
    private static extern bool ReadProcessMemory(IntPtr Handle, IntPtr Address,
        [Out] byte[] Arr, int Size, out int BytesRead);

    public bool ReadProcessMemory(IntPtr baseAddress, ref byte[] arr, int size)
    {
        return ReadProcessMemory(memoryHandle, baseAddress, arr, size, out _);
    }

    public bool ReadProcessMemory<T>(IntPtr baseAddress, ref T buffer) where T : unmanaged
    {
        return NativeWrapper.ReadProcessMemory(memoryHandle, baseAddress, ref buffer);
    }

    public bool WriteProcessMemory<T>(IntPtr baseAddress, ref T buffer) where T : unmanaged
    {
        return NativeWrapper.WriteProcessMemory(memoryHandle, baseAddress, ref buffer);
    }

    public bool WriteProcessMemoryArray<T>(IntPtr baseAddress, T[] buffer) where T : unmanaged
    {
        return NativeWrapper.WriteProcessMemoryArray(memoryHandle, baseAddress, buffer);
    }

    private int GetRelativeOffset(byte[] mem, int offset, int startOffset, int endOffset)
    {
        var start = offset + startOffset;
        var end = start + 4;
        var target = mem[start..end];
        int address = BitConverter.ToInt32(target);
        return offset + address + endOffset;
    }

    /// <summary>
    /// Finds and caches offset that matches provided AOB pattern.
    /// </summary>
    /// <returns>True if offset was found; otherwise false.</returns>
    public bool TryFindOffsetFromAOB(string offsetName, string aobPattern, List<(int, int)> relativeOffsets, out int outOffset)
    {
        if (_processOffsets.TryGetValue(offsetName, out outOffset))
        {
            return true;
        }
        
        GenerateAobPattern(aobPattern, out byte[] pattern, out bool[] wildcard);

        int memSize = gameProcess.MainModule.ModuleMemorySize;
        int memFindLength = memSize - pattern.Length;
        byte[] mem = new byte[memSize];

        ReadProcessMemory(gameProcess.MainModule.BaseAddress, ref mem, memSize);

        for (var offset = 0; offset < memFindLength; offset++)
        {
            if (mem[offset] == pattern[0])
            {
                bool matched = true;
                for (int iPattern = 1; iPattern < pattern.Length; iPattern++)
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
                    TaskLogs.AddLog($"Found AOB in memory for {offsetName}. Offset: 0x{offset:X2}", LogLevel.Debug);
                    return true;
                }
            }
        }

        TaskLogs.AddLog($"Unable to find AOB in memory for {offsetName}", LogLevel.Warning);
        return false;
    }

    private void GenerateAobPattern(string str, out byte[] pattern, out bool[] wildcard)
    {
        string[] split = str.Split(",");
        pattern = new byte[split.Length];
        wildcard = new bool[split.Length];

        for (var i = 0; i < split.Length; i++)
        {
            string byteStr = split[i].Replace("0x", "");

            if (byteStr == "??")
                wildcard[i] = true;
            else
                pattern[i] = byte.Parse(byteStr, NumberStyles.HexNumber);
        }
    }

    internal IntPtr GetParamPtr(IntPtr paramRepoPtr, GameOffsets offsets, int pOffset)
    {
        if (offsets.Is64Bit)
        {
            return GetParamPtr64Bit(paramRepoPtr, offsets, pOffset);
        }

        return GetParamPtr32Bit(paramRepoPtr, offsets, pOffset);
    }

    private IntPtr GetParamPtr64Bit(IntPtr paramRepoPtr, GameOffsets offsets, int pOffset)
    {
        var paramPtr = paramRepoPtr;
        NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        paramPtr = IntPtr.Add(paramPtr, pOffset);
        NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        foreach (var innerPathPart in offsets.paramInnerPath)
        {
            paramPtr = IntPtr.Add(paramPtr, innerPathPart);
            NativeWrapper.ReadProcessMemory(memoryHandle, paramPtr, ref paramPtr);
        }

        return paramPtr;
    }

    private IntPtr GetParamPtr32Bit(IntPtr paramRepoPtr, GameOffsets offsets, int pOffset)
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

    internal int GetRowCount(GameOffsets gOffsets, IntPtr paramPtr)
    {
        if (gOffsets.type is GameType.DarkSoulsIII or GameType.Sekiro or GameType.EldenRing or GameType.ArmoredCoreVI)
        {
            return GetRowCountInt(gOffsets, paramPtr);
        }

        return GetRowCountShort(gOffsets, paramPtr);
    }

    private int GetRowCountInt(GameOffsets gOffsets, IntPtr ParamPtr)
    {
        var buffer = 0;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr + gOffsets.paramCountOffset, ref buffer);
        return buffer;
    }

    private int GetRowCountShort(GameOffsets gOffsets, IntPtr ParamPtr)
    {
        Int16 buffer = 0;
        NativeWrapper.ReadProcessMemory(memoryHandle, ParamPtr + gOffsets.paramCountOffset, ref buffer);
        return buffer;
    }

    internal IntPtr GetToRowPtr(GameOffsets gOffsets, IntPtr paramPtr)
    {
        paramPtr = IntPtr.Add(paramPtr, gOffsets.paramDataOffset);
        return paramPtr;
    }


    public void ExecuteFunction(byte[] array)
    {
        IntPtr buffer = 0x100;

        var address = NativeWrapper.VirtualAllocEx(memoryHandle, IntPtr.Zero, buffer,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ExecuteReadWrite);

        if (address != IntPtr.Zero)
        {
            if (WriteProcessMemoryArray(address, array))
            {
                var threadHandle = NativeWrapper.CreateRemoteThread(memoryHandle, IntPtr.Zero, 0, address,
                    IntPtr.Zero, ThreadCreationFlags.Immediately, out var threadId);
                if (threadHandle != IntPtr.Zero)
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

        var address = NativeWrapper.VirtualAllocEx(memoryHandle, IntPtr.Zero, Size1,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ExecuteReadWrite);
        var bufferAddress = NativeWrapper.VirtualAllocEx(memoryHandle, IntPtr.Zero, Size2,
            AllocationType.Commit | AllocationType.Reserve, MemoryProtectionFlags.ExecuteReadWrite);

        var bytjmp = 0x2;
        var bytjmpAr = new byte[7];

        WriteProcessMemoryArray(bufferAddress, argument);

        bytjmpAr = BitConverter.GetBytes(bufferAddress);
        Array.Copy(bytjmpAr, 0, array, bytjmp, bytjmpAr.Length);

        if (address != IntPtr.Zero)
        {
            if (WriteProcessMemoryArray(address, array))
            {
                var threadHandle = NativeWrapper.CreateRemoteThread(memoryHandle, IntPtr.Zero, 0, address,
                    IntPtr.Zero, ThreadCreationFlags.Immediately, out var threadId);
                if (threadHandle != IntPtr.Zero)
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

    internal void PlayerItemGive(GameOffsets offsets, List<Param.Row> rows, string paramDefParamType,
        int itemQuantityReceived = 1, int itemDurabilityReceived = -1, int upgradeLevelItemToGive = 0)
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
}
