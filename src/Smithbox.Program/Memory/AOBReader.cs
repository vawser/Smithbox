using SoulsFormats;
using StudioCore.Logger;
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
                Smithbox.LogError(this, LOC.Get("MEM_No_Process_Found", processName));
            }
            else
            {
                this._targetProcess = processesByName[0];
                this._scanner = new AOBScanner(this._targetProcess);
            }
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("MEM_AOBReader_Init_Failed"), ex);
        }
    }

    public IntPtr AOBScan(byte[] pattern)
    {
        try
        {
            Smithbox.Log(this, LOC.Get("MEM_AOB_Scan_Start"));

            string mask = new string('x', pattern.Length);
            IntPtr pattern1 = _scanner.FindPattern(pattern, mask);

            Smithbox.Log(this, LOC.Get("MEM_AOB_Scan_Find", pattern1));

            return pattern1;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("MEM_AOB_Scan_Failed"), ex);
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
            Smithbox.LogError(this, LOC.Get("MEM_AOB_Read_Failed"), ex);
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
                    throw new NotSupportedException(
                        LOC.Get("MEM_AOB_Unsupported_Param_Def_Type", type));
            }
            return true;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("MEM_AOB_Write_Failed"), ex);
            return false;
        }
    }
}
