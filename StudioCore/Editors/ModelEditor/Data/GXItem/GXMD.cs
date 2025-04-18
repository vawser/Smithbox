using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Credit to RunDevelopment for this code
/// </summary>
public class GXMD
{
    public Dictionary<int, GXMDItem> Items { get; set; }

    public GXMD(Dictionary<int, GXMDItem> items)
    {
        Items = items;
    }
    public GXMD() : this(new Dictionary<int, GXMDItem>()) { }

    private struct GXValueReader
    {
        private int index;
        private GXValue[] values;

        public bool End => index == values.Length;

        public GXValueReader(byte[] data)
        {
            index = 0;
            values = data.ToGxValues();
        }

        private GXValue Read()
        {
            if (index >= values.Length)
            {
                throw new FormatException("Stream of GXValues ended early.");
            }
            return values[index++];
        }
        public int ReadInt() => Read().I;
        public float ReadFloat() => Read().F;
    }

    public static GXMD FromBytes(byte[] data)
    {
        var reader = new GXValueReader(data);

        var result = new Dictionary<int, GXMDItem>();

        var count = reader.ReadInt();
        for (var j = 0; j < count; j++)
        {
            var id = reader.ReadInt();
            var dataType = reader.ReadInt();

            if (dataType == 0)
            {
                var value = reader.ReadInt();
                if (value != 0 && value != 1)
                    throw new FormatException($"Invalid flag value {value}. Expected 0 or 1.");

                if (result.TryGetValue(id, out var item))
                {
                    if (item.Flagged != null)
                        throw new FormatException($"{id} cannot be flagged twice.");
                    item.Flagged = value != 0;
                }
                else
                {
                    throw new FormatException($"{id} cannot be flagged because it doesn't exist.");
                }
            }
            else
            {
                GXMDItem item;
                switch ((GXMDItemType)dataType)
                {
                    case GXMDItemType.Float:
                        item = new GXMDItem(reader.ReadFloat());
                        break;
                    case GXMDItemType.Float2:
                        item = new GXMDItem(new Vector2(reader.ReadFloat(), reader.ReadFloat()));
                        break;
                    case GXMDItemType.Float3:
                        item = new GXMDItem(new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()));
                        break;
                    case GXMDItemType.Float5:
                        item = new GXMDItem(new Float5(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat()));
                        break;
                    default:
                        throw new FormatException($"Unknown data type {dataType}");
                }

                result.Add(id, item);
            }
        }

        if (!reader.End)
            throw new FormatException($"Invalid bytes. There are still values left after parsing the GXMD.");

        return new GXMD(result);
    }

    private List<GXValue> ToValues()
    {
        var result = new List<GXValue>();
        void WriteInt(int i) => result.Add(new GXValue(i: i));
        void WriteFloat(float f) => result.Add(new GXValue(f: f));

        WriteInt(Items.Count + Items.Values.Where(i => i.Flagged.HasValue).Count());

        foreach (var (id, item) in Items)
        {
            WriteInt(id);
            WriteInt((int)item.Type);

            item.Validate();

            switch (item.Type)
            {
                case GXMDItemType.Float:
                    {
                        var value = (float)item.Value;
                        WriteFloat(value);
                        break;
                    }
                case GXMDItemType.Float2:
                    {
                        var value = (Vector2)item.Value;
                        WriteFloat(value.X);
                        WriteFloat(value.Y);
                        break;
                    }
                case GXMDItemType.Float3:
                    {
                        var value = (Vector3)item.Value;
                        WriteFloat(value.X);
                        WriteFloat(value.Y);
                        WriteFloat(value.Z);
                        break;
                    }
                case GXMDItemType.Float5:
                    {
                        var value = (Float5)item.Value;
                        WriteFloat(value.Item0);
                        WriteFloat(value.Item1);
                        WriteFloat(value.Item2);
                        WriteFloat(value.Item3);
                        WriteFloat(value.Item4);
                        break;
                    }
                default:
                    throw new FormatException($"Invalid item type {item.Type}");
            }

            if (item.Flagged.HasValue)
            {
                WriteInt(id);
                WriteInt(0);
                WriteInt(item.Flagged.Value ? 1 : 0);
            }
        }

        return result;
    }
    public byte[] ToBytes() => ToValues().ToGxDataBytes();
}
