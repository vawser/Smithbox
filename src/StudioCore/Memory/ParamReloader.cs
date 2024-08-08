using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Memory;

internal class ParamReloader
{
    public static uint numberOfItemsToGive = 1;
    public static uint upgradeLevelItemToGive;

    private static readonly List<ProjectType> _supportedGames = new()
    {
        ProjectType.DS1,
        ProjectType.DS1R,
        ProjectType.SDT,
        ProjectType.DS3,
        ProjectType.ER,
        ProjectType.AC6
    };

    public static bool GameIsSupported(ProjectType gameType)
    {
        return _supportedGames.Contains(gameType);
    }

    public static bool CanReloadMemoryParams(ParamBank bank)
    {
        if (GameIsSupported(Smithbox.ProjectType) && bank.IsLoadingParams == false)
        {
            return true;
        }

        return false;
    }

    public static void ReloadMemoryParam(ParamBank bank, string paramName)
    {
        if (paramName != null)
        {
            ReloadMemoryParams(bank, new string[] { paramName });
        }
    }

    public static void ReloadMemoryParams(ParamBank bank, string[] paramNames)
    {
        TaskManager.Run(new TaskManager.LiveTask("Param - Hot Reload", TaskManager.RequeueType.WaitThenRequeue,
            true, () =>
            {
                GameOffsetsEntry offsets = GetGameOffsets();
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

    private static void ReloadMemoryParamsThreads(ParamBank bank, GameOffsetsEntry offsets, string[] paramNames,
        SoulsMemoryHandler handler)
    {
        nint soloParamRepositoryPtr;
        if (offsets.ParamBaseAobPattern != null)
        {
            if (!handler.TryFindOffsetFromAOB("ParamBase", offsets.ParamBaseAobPattern, offsets.ParamBaseAobRelativeOffsets, out var paramBase))
            {
                return;
            }

            soloParamRepositoryPtr = nint.Add(handler.GetBaseAddress(), paramBase);
        }
        else
        {
            soloParamRepositoryPtr = nint.Add(handler.GetBaseAddress(), offsets.ParamBaseOffset);
        }

        List<Task> tasks = new();
        foreach (var param in paramNames)
        {
            if (!offsets.paramOffsets.TryGetValue(param, out var pOffset) || param == null)
            {
                TaskLogs.AddLog($"Hot reload: Cannot find param offset for {param}", LogLevel.Warning, TaskLogs.LogPriority.Normal);
                continue;
            }

            if (offsets.type is ProjectType.DS1 or ProjectType.DS1R && param == "ThrowParam")
            {
                // DS1 ThrowParam requires an additional offset.
                tasks.Add(new Task(() =>
                    WriteMemoryPARAM(offsets, bank.Params[param], pOffset, handler, nint.Add(soloParamRepositoryPtr, 0x10))));
            }
            else
            {
                tasks.Add(new Task(() =>
                    WriteMemoryPARAM(offsets, bank.Params[param], pOffset, handler, soloParamRepositoryPtr)));
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

    public static void GiveItem(GameOffsetsEntry offsets, List<Param.Row> rowsToGib, string studioParamType,
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

    private static void WriteMemoryPARAM(GameOffsetsEntry offsets, Param param, int paramOffset,
        SoulsMemoryHandler memoryHandler, nint soloParamRepositoryPtr)
    {
        var BasePtr = memoryHandler.GetParamPtr(soloParamRepositoryPtr, offsets, paramOffset);
        WriteMemoryPARAM(offsets, param, BasePtr, memoryHandler);
    }

    private static void WriteMemoryPARAM(GameOffsetsEntry offsets, Param param, nint BasePtr,
        SoulsMemoryHandler memoryHandler)
    {
        var BaseDataPtr = memoryHandler.GetToRowPtr(offsets, BasePtr);
        var RowCount = memoryHandler.GetRowCount(offsets, BasePtr);

        if (RowCount <= 0)
        {
            TaskLogs.AddLog($"Hot reload: ParamType {param.ParamType} has invalid offset or no rows", LogLevel.Warning, TaskLogs.LogPriority.Low);
            return;
        }

        nint DataSectionPtr;

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

            DataSectionPtr = nint.Add(BasePtr, rowPtr);

            BaseDataPtr += offsets.rowHeaderSize;

            if (rowDictionary.TryGetValue(RowId, out Queue<Param.Row> queue) && queue.TryDequeue(out Param.Row row))
            {
                WriteMemoryRow(row, DataSectionPtr, memoryHandler);
            }
            else
            {
                TaskLogs.AddLog($"Hot reload: ParamType {param.ParamType}: row {RowId} index {i} is in memory but not in editor. Try saving params and restarting game.", LogLevel.Warning, TaskLogs.LogPriority.Normal);
                return;
            }
        }
    }

    private static void WriteMemoryRow(Param.Row row, nint RowDataSectionPtr, SoulsMemoryHandler memoryHandler)
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

    private static int WriteMemoryCell(Param.Cell cell, nint CellDataPtr, ref int bitFieldPos, ref BitArray bits,
        SoulsMemoryHandler memoryHandler)
    {
        PARAMDEF.DefType displayType = cell.Def.DisplayType;
        // If this can be simplified, that would be ideal. Currently we have to reconcile DefType, a numerical size in bits, and the Type used for the bitField array
        if (cell.Def.BitSize != -1)
        {
            int bitSizeTotal;
            switch (displayType)
            {
                case PARAMDEF.DefType.u8:
                case PARAMDEF.DefType.s8:
                    bitSizeTotal = 8; break;
                case PARAMDEF.DefType.u16:
                case PARAMDEF.DefType.s16:
                    bitSizeTotal = 16; break;
                case PARAMDEF.DefType.u32:
                case PARAMDEF.DefType.s32:
                case PARAMDEF.DefType.b32:
                    bitSizeTotal = 32; break;
                //Only handle non-array dummy8 bitfields. Not that we should expect array bitfields.
                case PARAMDEF.DefType.dummy8:
                    bitSizeTotal = 8; break;
                default:
                    throw new Exception("Unexpected BitField Type");
            }
            if (bitFieldPos == 0)
            {
                bits = new BitArray(bitSizeTotal);
            }

            return WriteBitArray(cell, CellDataPtr, ref bitFieldPos, ref bits, memoryHandler, false);
        }
        else if (bits != null && bitFieldPos != 0)
        {
            var offset = WriteBitArray(null, CellDataPtr, ref bitFieldPos, ref bits, memoryHandler, true);
            return offset +
                   WriteMemoryCell(cell, CellDataPtr + offset, ref bitFieldPos, ref bits,
                       memoryHandler); //should recomplete current cell
        }

        if (displayType == PARAMDEF.DefType.f64)
        {
            var valueRead = 0.0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToDouble(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(double);
        }

        if (displayType == PARAMDEF.DefType.f32 || displayType == PARAMDEF.DefType.angle32)
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

        if (displayType == PARAMDEF.DefType.s32 || displayType == PARAMDEF.DefType.b32)
        {
            var valueRead = 0;
            memoryHandler.ReadProcessMemory(CellDataPtr, ref valueRead);

            var value = Convert.ToInt32(cell.Value);
            if (valueRead != value)
            {
                memoryHandler.WriteProcessMemory(CellDataPtr, ref value);
            }

            return sizeof(int);
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

            return sizeof(short);
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

            return sizeof(uint);
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

            return sizeof(ushort);
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
            //We don't handle dummy8[] or strings in reloader
            return cell.Def.ArrayLength * (displayType == PARAMDEF.DefType.fixstrW ? 2 : 1);
        }

        throw new Exception("Unexpected Field Type");
    }

    private static int WriteBitArray(Param.Cell? cell, nint CellDataPtr, ref int bitFieldPos, ref BitArray bits,
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

    public static GameOffsetsEntry GetGameOffsets()
    {
        ProjectType game = Smithbox.ProjectType;
        if (!GameOffsetsEntry.GameOffsetBank.ContainsKey(game))
        {
            try
            {
                GameOffsetsEntry.GameOffsetBank.Add(game, new GameOffsetsEntry(game));
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("Unable to create GameOffsets for param hot reloader.", LogLevel.Error,
                    TaskLogs.LogPriority.High, e);
                return null;
            }
        }

        return GameOffsetsEntry.GameOffsetBank[game];
    }

    public static string[] GetReloadableParams()
    {
        GameOffsetsEntry offs = GetGameOffsets();
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