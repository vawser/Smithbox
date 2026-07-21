using SoulsFormats;
using StudioCore.Editors.ParamEditor;
using StudioCore.Memory;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Formats;

/// <summary>
/// This is a direct blob of fields used in DS3 for the stayparams.
/// Technically should support any other arbitary struct that contains simple fields.
/// </summary>
public class StructParam : SoulsFile<StructParam>
{
    /// <summary>
    /// We use the PARAMDEF system to define the layout of the struct
    /// </summary>
    public PARAMDEF Def { get; set; }
    public IReadOnlyList<StructCell> Cells { get; private set; }

    public StructParam() { }

    public StructCell this[string name] => Cells.FirstOrDefault(cell => cell.Def.InternalName == name);

    protected override void Read(BinaryReaderEx br, PARAMDEF def)
    {
        Def = def;

        var cells = new StructCell[def.Fields.Count];
        for (int i = 0; i < def.Fields.Count; i++)
        {
            PARAMDEF.Field field = def.Fields[i];
            object value = ParamUtil.ConvertDefaultValue(field);
            cells[i] = new StructCell(field, value);
        }
        Cells = cells;

        ReadCells(br);
    }

    protected override void Write(BinaryWriterEx bw)
    {
        WriteCells(bw);
    }

    internal void ReadCells(BinaryReaderEx br)
    {
        var cells = new StructCell[Def.Fields.Count];

        for (int i = 0; i < Def.Fields.Count; i++)
        {
            PARAMDEF.Field field = Def.Fields[i];
            object value = null;
            PARAMDEF.DefType type = field.DisplayType;

            if (type == PARAMDEF.DefType.b32)
                value = br.ReadInt32();
            else if (type == PARAMDEF.DefType.f32 || type == PARAMDEF.DefType.angle32)
                value = br.ReadSingle();
            else if (type == PARAMDEF.DefType.f64)
                value = br.ReadDouble();
            else if (type == PARAMDEF.DefType.fixstr)
                value = br.ReadFixStr(field.ArrayLength);
            else if (type == PARAMDEF.DefType.fixstrW)
                value = br.ReadFixStrW(field.ArrayLength * 2);
            else if (ParamUtil.IsBitType(type))
            {
                if (field.BitSize == -1)
                {
                    value = type switch
                    {
                        PARAMDEF.DefType.s8 => br.ReadSByte(),
                        PARAMDEF.DefType.u8 => br.ReadByte(),
                        PARAMDEF.DefType.s16 => br.ReadInt16(),
                        PARAMDEF.DefType.u16 => br.ReadUInt16(),
                        PARAMDEF.DefType.s32 => br.ReadInt32(),
                        PARAMDEF.DefType.u32 => br.ReadUInt32(),
                        PARAMDEF.DefType.dummy8 => br.ReadBytes(field.ArrayLength),
                        _ => throw new InvalidOperationException($"Unexpected type in bitfield handling: {type}")
                    };
                }
            }
            else
                throw new NotImplementedException($"Unsupported field type: {type}");

            cells[i] = new StructCell(field, value);
        }

        Cells = cells;
    }

    internal void WriteCells(BinaryWriterEx bw)
    {
        for (int i = 0; i < Cells.Count; i++)
        {
            StructCell cell = Cells[i];
            object value = cell.Value;
            PARAMDEF.Field field = cell.Def;
            PARAMDEF.DefType type = field.DisplayType;

            if (type == PARAMDEF.DefType.b32)
                bw.WriteInt32((int)value);
            else if (type == PARAMDEF.DefType.f32 || type == PARAMDEF.DefType.angle32)
                bw.WriteSingle((float)value);
            else if (type == PARAMDEF.DefType.f64)
                bw.WriteDouble((double)value);
            else if (type == PARAMDEF.DefType.fixstr)
                bw.WriteFixStr((string)value, field.ArrayLength);
            else if (type == PARAMDEF.DefType.fixstrW)
                bw.WriteFixStrW((string)value, field.ArrayLength * 2);
            else if (ParamUtil.IsBitType(type))
            {
                switch (type)
                {
                    case PARAMDEF.DefType.s8: bw.WriteSByte((sbyte)value); break;
                    case PARAMDEF.DefType.u8: bw.WriteByte((byte)value); break;
                    case PARAMDEF.DefType.s16: bw.WriteInt16((short)value); break;
                    case PARAMDEF.DefType.u16: bw.WriteUInt16((ushort)value); break;
                    case PARAMDEF.DefType.s32: bw.WriteInt32((int)value); break;
                    case PARAMDEF.DefType.u32: bw.WriteUInt32((uint)value); break;
                    case PARAMDEF.DefType.dummy8: bw.WriteBytes((byte[])value); break;
                    default: throw new InvalidOperationException($"Unexpected type in bitfield handling: {type}");
                }
            }
            else
            {
                throw new NotImplementedException($"Unsupported field type: {type}");
            }
        }
    }
}
