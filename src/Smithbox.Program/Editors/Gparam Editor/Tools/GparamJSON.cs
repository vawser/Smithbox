using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.GparamEditor;

public static class GPARAMJson
{
    private static JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        IncludeFields = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new Vector2Converter(),
            new Vector3Converter(),
            new Vector4Converter(),
            new ColorConverter(),
            new ByteArrayConverter()
        }
    };

    // GPARAM (full file)
    public static string ToJson(GPARAM gparam)
    {
        return JsonSerializer.Serialize(gparam, Options);
    }

    public static GPARAM FromJson(string json)
    {
        return JsonSerializer.Deserialize<GPARAM>(json, Options);
    }

    // GPARAM.Param
    public static string ToJson(GPARAM.Param param)
    {
        return JsonSerializer.Serialize(param, Options);
    }

    public static GPARAM.Param ParamFromJson(string json)
    {
        return JsonSerializer.Deserialize<GPARAM.Param>(json, Options);
    }

    // GPARAM.IField
    public static string ToJson(GPARAM.IField field)
    {
        return JsonSerializer.Serialize(field, Options);
    }

    public static GPARAM.IField FieldFromJson(string json)
    {
        return JsonSerializer.Deserialize<GPARAM.IField>(json, Options);
    }

    // GPARAM.IFieldValue
    public class FieldValueWrapper
    {
        /// <summary>
        /// The field-type discriminator string (e.g. "float", "color", "vec3").
        /// Matches the values used on <see cref="GPARAM.IField"/>'s
        /// [JsonDerivedType] attributes.
        /// </summary>
        public string FieldType { get; set; }

        /// <summary>
        /// The serialised <see cref="GPARAM.IFieldValue"/> payload stored as a
        /// raw JSON element so it can be re-deserialised into the correct
        /// concrete <c>FieldValue&lt;T&gt;</c> type on import.
        /// </summary>
        public JsonElement Value { get; set; }
    }

    /// <summary>
    /// Maps a field-type discriminator string to its concrete
    /// <see cref="GPARAM.IFieldValue"/> implementation type.
    /// </summary>
    private static readonly Dictionary<string, Type> FieldValueTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "sbyte",  typeof(GPARAM.FieldValue<sbyte>)   },
        { "short",  typeof(GPARAM.FieldValue<short>)   },
        { "int",    typeof(GPARAM.FieldValue<int>)     },
        { "long",   typeof(GPARAM.FieldValue<long>)    },
        { "byte",   typeof(GPARAM.FieldValue<byte>)    },
        { "ushort", typeof(GPARAM.FieldValue<ushort>)  },
        { "uint",   typeof(GPARAM.FieldValue<uint>)    },
        { "ulong",  typeof(GPARAM.FieldValue<ulong>)   },
        { "float",  typeof(GPARAM.FieldValue<float>)   },
        { "double", typeof(GPARAM.FieldValue<double>)  },
        { "bool",   typeof(GPARAM.FieldValue<bool>)    },
        { "vec2",   typeof(GPARAM.FieldValue<Vector2>) },
        { "vec3",   typeof(GPARAM.FieldValue<Vector3>) },
        { "vec4",   typeof(GPARAM.FieldValue<Vector4>) },
        { "color",  typeof(GPARAM.FieldValue<Color>)   },
        { "string", typeof(GPARAM.FieldValue<string>)  },
    };

    /// <summary>
    /// Serialises a single <see cref="GPARAM.IFieldValue"/> together with a
    /// <paramref name="fieldTypeDiscriminator"/> so it can be reconstructed
    /// without its parent field context.
    /// </summary>
    /// <param name="value">The value to export.</param>
    /// <param name="fieldTypeDiscriminator">
    /// The discriminator string of the parent <see cref="GPARAM.IField"/>
    /// (e.g. "float", "color").  Pass the value from the parent field's
    /// <c>[JsonDerivedType]</c> attribute.
    /// </param>
    public static string ToJson(GPARAM.IFieldValue value, string fieldTypeDiscriminator)
    {
        // Serialise the value payload into a JsonElement first.
        var rawBytes = JsonSerializer.SerializeToUtf8Bytes(value, value.GetType(), Options);
        var payload = JsonSerializer.Deserialize<JsonElement>(rawBytes);

        var wrapper = new FieldValueWrapper
        {
            FieldType = fieldTypeDiscriminator,
            Value = payload
        };

        return JsonSerializer.Serialize(wrapper, Options);
    }

    /// <summary>
    /// Deserialises a <see cref="GPARAM.IFieldValue"/> that was previously
    /// exported with <see cref="ToJson(GPARAM.IFieldValue, string)"/>.
    /// </summary>
    public static GPARAM.IFieldValue FieldValueFromJson(string json)
    {
        var wrapper = JsonSerializer.Deserialize<FieldValueWrapper>(json, Options)
            ?? throw new JsonException("Failed to deserialise FieldValueWrapper.");

        if (!FieldValueTypeMap.TryGetValue(wrapper.FieldType, out var concreteType))
            throw new NotSupportedException($"Unknown field type discriminator: '{wrapper.FieldType}'.");

        var rawBytes = JsonSerializer.SerializeToUtf8Bytes(wrapper.Value);
        return (GPARAM.IFieldValue)JsonSerializer.Deserialize(rawBytes, concreteType, Options)
            ?? throw new JsonException($"Failed to deserialise FieldValue<T> for type '{wrapper.FieldType}'.");
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the JSON discriminator string for a concrete
    /// <see cref="GPARAM.IField"/> instance (e.g. "float", "color").
    /// Used when building a <see cref="FieldValueWrapper"/> from the editor.
    /// </summary>
    public static string GetFieldTypeDiscriminator(GPARAM.IField field)
    {
        return field switch
        {
            GPARAM.SbyteField => "sbyte",
            GPARAM.ShortField => "short",
            GPARAM.IntField => "int",
            GPARAM.LongField => "long",
            GPARAM.ByteField => "byte",
            GPARAM.UshortField => "ushort",
            GPARAM.UintField => "uint",
            GPARAM.UlongField => "ulong",
            GPARAM.FloatField => "float",
            GPARAM.DoubleField => "double",
            GPARAM.BoolField => "bool",
            GPARAM.Vector2Field => "vec2",
            GPARAM.Vector3Field => "vec3",
            GPARAM.Vector4Field => "vec4",
            GPARAM.ColorField => "color",
            GPARAM.StringField => "string",
            _ => throw new NotSupportedException($"No discriminator mapping for field type {field.GetType().Name}.")
        };
    }
}

// =============================================================================
// JSON converters
// =============================================================================

public class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0, y = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector2(x, y);

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string prop = reader.GetString();
                reader.Read();

                switch (prop)
                {
                    case "X": x = reader.GetSingle(); break;
                    case "Y": y = reader.GetSingle(); break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteEndObject();
    }
}

public class Vector3Converter : JsonConverter<Vector3>
{
    public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0, y = 0, z = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector3(x, y, z);

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string prop = reader.GetString();
                reader.Read();

                switch (prop)
                {
                    case "X": x = reader.GetSingle(); break;
                    case "Y": y = reader.GetSingle(); break;
                    case "Z": z = reader.GetSingle(); break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteEndObject();
    }
}

public class Vector4Converter : JsonConverter<Vector4>
{
    public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        float x = 0, y = 0, z = 0, w = 0;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return new Vector4(x, y, z, w);

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string prop = reader.GetString();
                reader.Read();

                switch (prop)
                {
                    case "X": x = reader.GetSingle(); break;
                    case "Y": y = reader.GetSingle(); break;
                    case "Z": z = reader.GetSingle(); break;
                    case "W": w = reader.GetSingle(); break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("X", value.X);
        writer.WriteNumber("Y", value.Y);
        writer.WriteNumber("Z", value.Z);
        writer.WriteNumber("W", value.W);
        writer.WriteEndObject();
    }
}

public class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        int r = 0, g = 0, b = 0, a = 255;

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return Color.FromArgb(a, r, g, b);

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                string prop = reader.GetString();
                reader.Read();

                switch (prop)
                {
                    case "R": r = reader.GetInt32(); break;
                    case "G": g = reader.GetInt32(); break;
                    case "B": b = reader.GetInt32(); break;
                    case "A": a = reader.GetInt32(); break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("R", value.R);
        writer.WriteNumber("G", value.G);
        writer.WriteNumber("B", value.B);
        writer.WriteNumber("A", value.A);
        writer.WriteEndObject();
    }
}

public class ByteArrayConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.String)
        {
            string base64 = reader.GetString();
            if (string.IsNullOrEmpty(base64))
                return Array.Empty<byte>();

            return Convert.FromBase64String(base64);
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var list = new List<byte>();
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                list.Add(reader.GetByte());
            }
            return list.ToArray();
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        if (value == null || value.Length == 0)
        {
            writer.WriteStringValue("");
            return;
        }

        writer.WriteStringValue(Convert.ToBase64String(value));
    }
}