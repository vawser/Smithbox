using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Memory;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamReloader
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamReloader(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        UpdateSelectedOffset();
    }

    // Set the game offset to the latest from the JSON so user's always use the latest offset,
    // which is what is expected.
    private void UpdateSelectedOffset()
    {
        if (CFG.Current.UseLatestGameOffset)
        {
            if (Project.Handler.ParamData.ParamMemoryOffsets != null && Project.Handler.ParamData.ParamMemoryOffsets.list != null)
            {
                var entries = Project.Handler.ParamData.ParamMemoryOffsets.list.Select(entry => entry.exeVersion).ToArray();

                CFG.Current.SelectedGameOffsetData = entries.Count() - 1;
            }
        }
    }

    private readonly List<ProjectType> _reloaderSupportedGames = new()
    {
        ProjectType.DS1,
        ProjectType.DS1R,
        ProjectType.SDT,
        ProjectType.DS3,
        ProjectType.ER,
        ProjectType.NR,
        ProjectType.AC6
    };

    public bool ParamReloadSupported(ProjectType gameType)
    {
        return _reloaderSupportedGames.Contains(gameType);
    }

    public bool CanReloadMemoryParams(ParamBank bank)
    {
        if (ParamReloadSupported(Project.Descriptor.ProjectType))
        {
            return true;
        }
        return false;
    }

    public void ReloadMemoryParam(ParamBank bank, string paramName)
    {
        if (paramName != null)
        {
            ReloadMemoryParams(bank, new string[] { paramName });
        }
    }

    public void DisplayParamReloader()
    {
        var windowWidth = ImGui.GetWindowWidth();

        // Param Reloader
        if (ParamReloadSupported(Editor.Project.Descriptor.ProjectType))
        {
            if (ImGui.CollapsingHeader("Param Reloader"))
            {
                UIHelper.WrappedText("WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");
                UIHelper.WrappedText("");

                if (ImGui.Button("Reload Current Param", DPI.WholeWidthButton(windowWidth, 24)))
                {
                    ReloadCurrentParam(Editor);
                }
                UIHelper.Tooltip($"{InputManager.GetHint(KeybindID.ParamEditor_Reload_Selected_Param)}");

                if (ImGui.Button("Reload All Params", DPI.WholeWidthButton(windowWidth, 24)))
                {
                    ReloadAllParams(Editor);
                }
                UIHelper.Tooltip($"{InputManager.GetHint(KeybindID.ParamEditor_Reload_All_Params)}");
            }
        }
    }

    public void DisplayParamReloaderMenu()
    {
        if (ParamReloadSupported(Project.Descriptor.ProjectType))
        {
            if (ImGui.BeginMenu("Param Reloader"))
            {
                if (ImGui.MenuItem("Current Param", InputManager.GetHint(KeybindID.ParamEditor_Reload_Selected_Param)))
                {
                    ReloadCurrentParam(Editor);
                }
                UIHelper.Tooltip($"WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");

                if (ImGui.MenuItem("All Params", InputManager.GetHint(KeybindID.ParamEditor_Reload_All_Params)))
                {
                    ReloadAllParams(Editor);
                }
                UIHelper.Tooltip($"WARNING: Param Reloader only works for existing row entries.\nGame must be restarted for new rows and modified row IDs.");

                ImGui.EndMenu();
            }
        }
    }


    public void ReloadMemoryParams(ParamBank bank, string[] paramNames)
    {
        TaskManager.LiveTask task = new(
            "paramEditor_reloadParamData",
            "Param Editor",
            "param reloader has updated the in-game params.",
            "param reloader has failed.",
            TaskManager.RequeueType.None,
            false,
            () =>
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
                    SoulsMemoryHandler memoryHandler = new(Editor, processArray.First());

                    ReloadMemoryParamsThreads(bank, offsets, paramNames, memoryHandler);
                    memoryHandler.Terminate();
                }
                else
                {
                    TaskLogs.AddLog("Unable to find running game", LogLevel.Error);
                    //throw new Exception("Unable to find running game");
                }
            }
        );

        TaskManager.Run(task);
    }

    private void ReloadMemoryParamsThreads(ParamBank bank, GameOffsetsEntry offsets, string[] paramNames,
        SoulsMemoryHandler handler)
    {

        List<Task> tasks = new();
        foreach (string param in paramNames)
        {
            // Skip these for now: cause it to CTD due to type issue
            if (param == "ThrustersLocomotionParam_PC" || param == "ThrustersParam_NPC")
            {
                TaskLogs.AddLog($"Cannot reload {param} in Param Reloader.", LogLevel.Warning, LogPriority.Normal);
                continue;
            }

            bool paramFound = false;
            foreach (GameOffsetBaseEntry paramRepo in offsets.Bases)
            {
                if (!paramRepo.paramOffsets.TryGetValue(param, out int paramOffset) || param == null)
                {
                    continue;
                }

                paramFound = true;

                try
                {
                    nint paramRepositoryPtr = handler.GetBaseAddress();
                    if (paramRepo.ParamBaseAobPattern != null)
                    {
                        if (!handler.TryFindOffsetFromAOB("ParamBase", paramRepo.ParamBaseAobPattern, paramRepo.ParamBaseAobRelativeOffsets, out int paramBase))
                        {
                            TaskLogs.AddLog($"Param Reloader cannot reload {param} because the given AOB was not found.", LogLevel.Warning, LogPriority.Normal);
                            continue;
                        }

                        paramRepositoryPtr += paramBase;
                    }
                    else
                    {
                        paramRepositoryPtr += paramRepo.ParamBaseOffset;
                    }

                    if (offsets.type is ProjectType.DS1 or ProjectType.DS1R && param == "ThrowParam")
                    {
                        // DS1 ThrowParam requires an additional offset.
                        paramRepositoryPtr += 0x10;
                    }

                    tasks.Add(new Task(() =>
                        WriteMemoryPARAM(offsets.type, offsets.Is64Bit, paramRepo, bank.Params[param], paramOffset, handler, paramRepositoryPtr)));
                }
                catch (Exception ex)
                {
                    TaskLogs.AddLog($"Failed to find base address.", LogLevel.Error, LogPriority.High, ex);
                }
            }

            if (!paramFound)
            {
                TaskLogs.AddLog($"Cannot find param offset for {param} in Param Reloader.", LogLevel.Warning, LogPriority.Normal);
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

    private void WriteMemoryPARAM(ProjectType type, bool is64Bit, GameOffsetBaseEntry entry, Param param, int paramOffset,
        SoulsMemoryHandler memoryHandler, nint soloParamRepositoryPtr)
    {
        var BasePtr = memoryHandler.GetParamPtr(is64Bit, soloParamRepositoryPtr, entry, paramOffset);
        WriteMemoryPARAM(type, entry, param, BasePtr, memoryHandler);
    }

    private void WriteMemoryPARAM(ProjectType type, GameOffsetBaseEntry entry, Param param, nint BasePtr,
        SoulsMemoryHandler memoryHandler)
    {
        var BaseDataPtr = memoryHandler.GetToRowPtr(entry, BasePtr);
        var RowCount = memoryHandler.GetRowCount(type, entry, BasePtr);

        if (RowCount <= 0)
        {
            TaskLogs.AddLog($"ParamType {param.ParamType} has invalid offset or no rows for Param Reloader.", LogLevel.Warning, LogPriority.Low);
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
            memoryHandler.ReadProcessMemory(BaseDataPtr + entry.rowPointerOffset, ref rowPtr);
            if (RowId < 0 || rowPtr < 0)
            {
                BaseDataPtr += entry.rowHeaderSize;
                continue;
            }

            DataSectionPtr = nint.Add(BasePtr, rowPtr);

            BaseDataPtr += entry.rowHeaderSize;

            if (rowDictionary.TryGetValue(RowId, out Queue<Param.Row> queue) && queue.TryDequeue(out Param.Row row))
            {
                WriteMemoryRow(row, DataSectionPtr, memoryHandler);
            }
            else
            {
                TaskLogs.AddLog($"ParamType {param.ParamType}: row {RowId} index {i} is in memory but not in editor during Param Reloader opeation.\nTry saving params and restarting game.", LogLevel.Warning, LogPriority.Normal);
                return;
            }
        }
    }

    private void WriteMemoryRow(Param.Row row, nint RowDataSectionPtr, SoulsMemoryHandler memoryHandler)
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

    private int WriteMemoryCell(Param.Cell cell, nint CellDataPtr, ref int bitFieldPos, ref BitArray bits,
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

    private int WriteBitArray(Param.Cell? cell, nint CellDataPtr, ref int bitFieldPos, ref BitArray bits,
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

    public GameOffsetsEntry GetGameOffsets()
    {
        ProjectType game = Project.Descriptor.ProjectType;
        if (!GameOffsetsEntry.GameOffsetBank.ContainsKey(game))
        {
            try
            {
                GameOffsetsEntry.GameOffsetBank.Add(game, new GameOffsetsEntry(Project));
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("Unable to create GameOffsets for Param Reloader.", LogLevel.Error,
                    LogPriority.High, e);
                return null;
            }
        }

        return GameOffsetsEntry.GameOffsetBank[game];
    }

    public List<string> GetReloadableParams()
    {
        GameOffsetsEntry offs = GetGameOffsets();
        if (offs == null)
        {
            return new List<string>();
        }

        List<string> reloadableParams = new();

        foreach (var entry in offs.Bases)
        {
            var curParamList = entry.paramOffsets.Keys.ToList();
            foreach (var t in curParamList)
            {
                reloadableParams.Add(t);
            }
        }

        return reloadableParams;
    }

    /// <summary>
    /// Returns dictionary of Row ID keys corresponding with Queue of rows, for the purpose of handling duplicate row IDs.
    /// </summary>
    private Dictionary<int, Queue<Param.Row>> GetRowQueueDictionary(Param param)
    {
        Dictionary<int, Queue<Param.Row>> rows = new();

        foreach (var row in param.Rows)
        {
            rows.TryAdd(row.ID, new());
            rows[row.ID].Enqueue(row);
        }

        return rows;
    }

    public void ReloadCurrentParam(ParamEditorScreen editor)
    {
        var canHotReload = CanReloadMemoryParams(editor.Project.Handler.ParamData.PrimaryBank);
        if (canHotReload)
        {
            if (editor._activeView.Selection.GetActiveParam() != null)
            {
                ReloadMemoryParam(editor.Project.Handler.ParamData.PrimaryBank, editor._activeView.Selection.GetActiveParam());
            }
            else
            {
                TaskLogs.AddLog("No param has been selected yet for the Param Reloder.");
            }
        }
        else
        {
            TaskLogs.AddLog("Param Reloader cannot reload for this project.");
        }
    }
    public void ReloadAllParams(ParamEditorScreen editor)
    {
        var canHotReload = CanReloadMemoryParams(editor.Project.Handler.ParamData.PrimaryBank);
        if (canHotReload)
        {
            ReloadMemoryParams(editor.Project.Handler.ParamData.PrimaryBank, editor.Project.Handler.ParamData.PrimaryBank.Params.Keys.ToArray());
        }
        else
        {
            TaskLogs.AddLog("Param Reloader cannot reload for this project.");
        }
    }
}
