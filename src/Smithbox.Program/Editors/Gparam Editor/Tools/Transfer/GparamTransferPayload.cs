using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// What a transfer payload holds.
/// </summary>
public enum GparamTransferKind
{
    Gparam,
    Group,
    Field,
    Values
}

/// <summary>
/// Where a payload was taken from, for display and log lines. Not used to place the data.
/// </summary>
public class GparamTransferSource
{
    public string File { get; set; }
    public string Group { get; set; }
    public string Field { get; set; }
}

/// <summary>
/// The data moved by Copy/Paste and Export/Import: a kind, where it came from, and the items themselves.
/// The same JSON goes to the clipboard and to files.
/// </summary>
/// <remarks>
/// The items are kept as raw JSON and turned into objects again on each Get call, so applying one payload twice
/// (pasting twice, or the staging panel applying it to several targets) never puts the same object in two places.
/// </remarks>
public class GparamTransferPayload
{
    public const string FormatId = "Smithbox.Gparam";
    public const int CurrentVersion = 1;

    public string Format { get; set; } = FormatId;
    public int Version { get; set; } = CurrentVersion;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GparamTransferKind Kind { get; set; }

    /// <summary>
    /// The project type it was taken from, e.g. "ER".
    /// </summary>
    public string Game { get; set; }

    public GparamTransferSource Source { get; set; } = new();

    /// <summary>
    /// For Values: the field type discriminator ("float", "color"...), as used on <see cref="GPARAM.IField"/>.
    /// </summary>
    public string FieldType { get; set; }

    public List<JsonElement> Items { get; set; } = new();

    /// <summary>
    /// Read from an export made before the envelope existed: a bare GPARAM, group, field or value wrapper.
    /// </summary>
    [JsonIgnore]
    public bool IsLegacy { get; private set; }

    [JsonIgnore]
    public int Count => Items?.Count ?? 0;

    #region Build

    public static GparamTransferPayload FromGparam(GPARAM gparam, string game, GparamTransferSource source)
    {
        return new GparamTransferPayload
        {
            Kind = GparamTransferKind.Gparam,
            Game = game,
            Source = source,
            Items = { JsonSerializer.SerializeToElement(gparam, GPARAMJson.Options) }
        };
    }

    public static GparamTransferPayload FromGroups(IEnumerable<GPARAM.Param> groups, string game, GparamTransferSource source)
    {
        return new GparamTransferPayload
        {
            Kind = GparamTransferKind.Group,
            Game = game,
            Source = source,
            Items = groups.Select(e => JsonSerializer.SerializeToElement(e, GPARAMJson.Options)).ToList()
        };
    }

    public static GparamTransferPayload FromFields(IEnumerable<GPARAM.IField> fields, string game, GparamTransferSource source)
    {
        return new GparamTransferPayload
        {
            Kind = GparamTransferKind.Field,
            Game = game,
            Source = source,
            // Typed as IField so the "$type" discriminator is written
            Items = fields.Select(e => JsonSerializer.SerializeToElement<GPARAM.IField>(e, GPARAMJson.Options)).ToList()
        };
    }

    public static GparamTransferPayload FromValues(GPARAM.IField field, IEnumerable<GPARAM.IFieldValue> values, string game, GparamTransferSource source)
    {
        return new GparamTransferPayload
        {
            Kind = GparamTransferKind.Values,
            Game = game,
            Source = source,
            FieldType = GPARAMJson.GetFieldTypeDiscriminator(field),
            Items = values.Select(e => JsonSerializer.SerializeToElement(e, e.GetType(), GPARAMJson.Options)).ToList()
        };
    }

    #endregion

    #region Get

    public GPARAM GetGparam()
    {
        return Items.Count == 0 ? null : Items[0].Deserialize<GPARAM>(GPARAMJson.Options);
    }

    public List<GPARAM.Param> GetGroups()
    {
        return Items.Select(e => e.Deserialize<GPARAM.Param>(GPARAMJson.Options)).ToList();
    }

    public List<GPARAM.IField> GetFields()
    {
        return Items.Select(e => e.Deserialize<GPARAM.IField>(GPARAMJson.Options)).ToList();
    }

    public List<GPARAM.IFieldValue> GetValues()
    {
        var type = GPARAMJson.GetFieldValueType(FieldType);

        return Items.Select(e => (GPARAM.IFieldValue)e.Deserialize(type, GPARAMJson.Options)).ToList();
    }

    #endregion

    #region Read / Write

    public string ToJson()
    {
        return JsonSerializer.Serialize(this, GPARAMJson.Options);
    }

    /// <summary>
    /// Read a payload from JSON text: the envelope, or a legacy export, told apart by its properties.
    /// Returns null with the reason in <paramref name="error"/> if the text isn't GPARAM transfer data.
    /// </summary>
    public static GparamTransferPayload Parse(string json, out string error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(json))
        {
            error = LOC.Get("GPARAM_Transfer_Parse_Empty");
            return null;
        }

        JsonElement root;

        try
        {
            root = JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch (JsonException)
        {
            error = LOC.Get("GPARAM_Transfer_Parse_Not_Json");
            return null;
        }

        if (root.ValueKind != JsonValueKind.Object)
        {
            error = LOC.Get("GPARAM_Transfer_Parse_Unknown");
            return null;
        }

        try
        {
            var payload = Has(root, "Format") ? ParseEnvelope(root, out error) : ParseLegacy(root, out error);

            // Check the items can be turned into objects now, so a bad payload fails here, not at paste time
            if (payload != null && !payload.CanMaterialize(out error))
                return null;

            return payload;
        }
        catch (Exception e) when (e is JsonException or NotSupportedException or InvalidOperationException)
        {
            error = LOC.Get("GPARAM_Transfer_Parse_Invalid", e.Message);
            return null;
        }
    }

    private static GparamTransferPayload ParseEnvelope(JsonElement root, out string error)
    {
        error = null;

        var payload = root.Deserialize<GparamTransferPayload>(GPARAMJson.Options);

        if (payload == null || payload.Format != FormatId)
        {
            error = LOC.Get("GPARAM_Transfer_Parse_Unknown");
            return null;
        }

        if (payload.Version > CurrentVersion)
        {
            error = LOC.Get("GPARAM_Transfer_Parse_Newer_Version", payload.Version, CurrentVersion);
            return null;
        }

        payload.Items ??= new List<JsonElement>();
        payload.Source ??= new GparamTransferSource();

        if (payload.Items.Count == 0)
        {
            error = LOC.Get("GPARAM_Transfer_Parse_No_Items");
            return null;
        }

        return payload;
    }

    /// <summary>
    /// The exports made before the envelope: each is the bare object, except a value, which was wrapped with its
    /// field type. Told apart by a property only that kind has.
    /// </summary>
    private static GparamTransferPayload ParseLegacy(JsonElement root, out string error)
    {
        error = null;

        var payload = new GparamTransferPayload { IsLegacy = true };

        if (Has(root, "FieldType") && Has(root, "Value"))
        {
            payload.Kind = GparamTransferKind.Values;
            payload.FieldType = root.GetProperty("FieldType").GetString();
            payload.Items.Add(root.GetProperty("Value").Clone());
        }
        else if (Has(root, "$type"))
        {
            payload.Kind = GparamTransferKind.Field;
            payload.Items.Add(root.Clone());
        }
        else if (Has(root, "Params"))
        {
            payload.Kind = GparamTransferKind.Gparam;
            payload.Items.Add(root.Clone());
        }
        else if (Has(root, "Fields"))
        {
            payload.Kind = GparamTransferKind.Group;
            payload.Items.Add(root.Clone());
        }
        else
        {
            error = LOC.Get("GPARAM_Transfer_Parse_Unknown");
            return null;
        }

        return payload;
    }

    private bool CanMaterialize(out string error)
    {
        error = null;

        switch (Kind)
        {
            case GparamTransferKind.Gparam:
                if (GetGparam() == null)
                    error = LOC.Get("GPARAM_Transfer_Parse_No_Items");
                break;

            case GparamTransferKind.Group:
                if (GetGroups().Any(e => e == null || e.Key == null))
                    error = LOC.Get("GPARAM_Transfer_Parse_Item_Invalid", KindName(Kind));
                break;

            case GparamTransferKind.Field:
                if (GetFields().Any(e => e == null || e.Key == null))
                    error = LOC.Get("GPARAM_Transfer_Parse_Item_Invalid", KindName(Kind));
                break;

            case GparamTransferKind.Values:
                if (string.IsNullOrEmpty(FieldType) || GetValues().Any(e => e == null))
                    error = LOC.Get("GPARAM_Transfer_Parse_Item_Invalid", KindName(Kind));
                break;
        }

        return error == null;
    }

    private static bool Has(JsonElement obj, string name)
    {
        return obj.TryGetProperty(name, out _);
    }

    #endregion

    /// <summary>
    /// The kind as the user reads it ("group", "values").
    /// </summary>
    public static string KindName(GparamTransferKind kind)
    {
        return kind switch
        {
            GparamTransferKind.Gparam => LOC.Get("GPARAM_Transfer_Kind_Gparam"),
            GparamTransferKind.Group => LOC.Get("GPARAM_Transfer_Kind_Group"),
            GparamTransferKind.Field => LOC.Get("GPARAM_Transfer_Kind_Field"),
            GparamTransferKind.Values => LOC.Get("GPARAM_Transfer_Kind_Values"),
            _ => kind.ToString()
        };
    }
}
