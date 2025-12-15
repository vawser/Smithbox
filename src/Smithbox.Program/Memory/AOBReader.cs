using SoulsFormats;
using StudioCore.Utilities;
using System;
using System.Diagnostics;

namespace StudioCore.Memory;

public class AOBReader
{
    private readonly AOBScanner _scanner;
    private readonly Process _targetProcess;

    public bool IsProcessValid => this._targetProcess != null && !this._targetProcess.HasExited;

    public AOBReader(string processName)
    {
        try
        {
            Process[] processesByName = Process.GetProcessesByName(processName);
            if (processesByName.Length == 0)
            {
                TaskLogs.AddLog($"Error: Process '{processName}' not found. Please run the game first.");
            }
            else
            {
                this._targetProcess = processesByName[0];
                this._scanner = new AOBScanner(this._targetProcess);
            }
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to initialize AOBReader", Microsoft.Extensions.Logging.LogLevel.Error, LogPriority.High, ex);
        }
    }

    public IntPtr AOBScan(byte[] pattern)
    {
        try
        {
            TaskLogs.AddLog("Starting AOB scan...");

            string mask = new string('x', pattern.Length);
            IntPtr pattern1 = _scanner.FindPattern(pattern, mask);

            TaskLogs.AddLog($"Found at: 0x{pattern1}");

            return pattern1;
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Scan failed", Microsoft.Extensions.Logging.LogLevel.Error, LogPriority.High, ex);
            return new IntPtr(-1);
        }
    }

    public object ReadMemory(IntPtr baseAddress, long offset, PARAMDEF.DefType type)
    {
        object obj = null;

        try
        {
            IntPtr address = baseAddress + (IntPtr)(int)offset;
            switch (type)
            {
                case PARAMDEF.DefType.s8:
                    obj = _scanner.ReadSByte(address);
                    break;
                case PARAMDEF.DefType.u8:
                    obj = _scanner.ReadByte(address);
                    break;
                case PARAMDEF.DefType.s16:
                    obj = _scanner.ReadInt16(address);
                    break;
                case PARAMDEF.DefType.u16:
                    obj = _scanner.ReadUInt16(address);
                    break;
                case PARAMDEF.DefType.s32:
                    obj = _scanner.ReadInt32(address);
                    break;
                case PARAMDEF.DefType.u32:
                    obj = _scanner.ReadUInt32(address);
                    break;
                case PARAMDEF.DefType.b32:
                    obj = _scanner.ReadInt32(address);
                    break;
                case PARAMDEF.DefType.f32:
                    obj = _scanner.ReadFloat(address);
                    break;
                case PARAMDEF.DefType.angle32:
                    obj = _scanner.ReadFloat(address);
                    break;
                case PARAMDEF.DefType.f64:
                    obj = _scanner.ReadDouble(address);
                    break;
            }
            return obj;
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Read failed", Microsoft.Extensions.Logging.LogLevel.Error, LogPriority.High, ex);
            return null;
        }
    }

    public bool WriteMemory(IntPtr baseAddress, long offset, object value, PARAMDEF.DefType type)
    {
        try
        {
            IntPtr address = baseAddress + (IntPtr)(int)offset;

            switch (type)
            {
                case PARAMDEF.DefType.s8:
                    _scanner.WriteSByte(address, Convert.ToSByte(value));
                    break;
                case PARAMDEF.DefType.u8:
                    _scanner.WriteByte(address, Convert.ToByte(value));
                    break;
                case PARAMDEF.DefType.s16:
                    _scanner.WriteInt16(address, Convert.ToInt16(value));
                    break;
                case PARAMDEF.DefType.u16:
                    _scanner.WriteUInt16(address, Convert.ToUInt16(value));
                    break;
                case PARAMDEF.DefType.s32:
                case PARAMDEF.DefType.b32:
                    _scanner.WriteInt32(address, Convert.ToInt32(value));
                    break;
                case PARAMDEF.DefType.u32:
                    _scanner.WriteUInt32(address, Convert.ToUInt32(value));
                    break;
                case PARAMDEF.DefType.f32:
                case PARAMDEF.DefType.angle32:
                    _scanner.WriteSingle(address, Convert.ToSingle(value));
                    break;
                case PARAMDEF.DefType.f64:
                    _scanner.WriteDouble(address, Convert.ToDouble(value));
                    break;
                default:
                    throw new NotSupportedException($"Unsupported type: {type}");
            }
            return true;
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Write failed", Microsoft.Extensions.Logging.LogLevel.Error, LogPriority.High, ex);
            return false;
        }
    }
}
