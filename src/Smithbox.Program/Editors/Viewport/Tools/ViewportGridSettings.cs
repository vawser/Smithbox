using StudioCore.Editors.MapEditor;
using System;
using System.Numerics;
using System.Text.Json;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// The stored settings of one viewport grid. Kept in <see cref="Application.CFG" />, one
/// <see cref="ViewportGridSet" /> per editor.
/// </summary>
public class ViewportGridSettings
{
    public bool Display = false;

    /// <summary>Squares either side of the center of the grid</summary>
    public int Size = 1000;

    /// <summary>The size of one square</summary>
    public float SectionSize = 10;

    public Vector3 Color = new(0.5f, 0.5f, 0.5f);

    public Vector3 Position = Vector3.Zero;

    /// <summary>Euler rotation in degrees, applied in the order of <see cref="Renderer.Transform.EulerToQuaternion" /></summary>
    public Vector3 Rotation = Vector3.Zero;

    // Adjust to Grid, see AdjustToGridAction
    public bool SnapPosition_X = true;
    public bool SnapPosition_Y = true;
    public bool SnapPosition_Z = true;

    public bool SnapRotation_X = true;
    public bool SnapRotation_Y = true;
    public bool SnapRotation_Z = true;

    /// <summary>The axis treated as the 'floor' when adjusting to the grid</summary>
    public ViewportGridRootAxis RootAxis = ViewportGridRootAxis.Y;

    public ViewportGridSettings Clone()
    {
        return (ViewportGridSettings)MemberwiseClone();
    }
}

/// <summary>
/// The primary, secondary and tertiary grids of one editor.
/// </summary>
public class ViewportGridSet
{
    public ViewportGridSettings Primary = new();

    // Upright, facing along Z
    public ViewportGridSettings Secondary = new() { Rotation = new Vector3(90, 0, 0), RootAxis = ViewportGridRootAxis.X };

    // Upright, facing along X
    public ViewportGridSettings Tertiary = new() { Rotation = new Vector3(0, 0, 90), RootAxis = ViewportGridRootAxis.Z };

    public ViewportGridSettings Get(ViewportGridType type)
    {
        // A config saved before a grid existed leaves it null
        return type switch
        {
            ViewportGridType.Primary => Primary ??= new ViewportGridSettings(),
            ViewportGridType.Secondary => Secondary ??= new ViewportGridSettings(),
            ViewportGridType.Tertiary => Tertiary ??= new ViewportGridSettings(),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }

    public ViewportGridSet Clone()
    {
        return new ViewportGridSet
        {
            Primary = Get(ViewportGridType.Primary).Clone(),
            Secondary = Get(ViewportGridType.Secondary).Clone(),
            Tertiary = Get(ViewportGridType.Tertiary).Clone()
        };
    }
}

/// <summary>
/// Moves the grid settings of configs saved before <see cref="ViewportGridSet" />, when each grid value was its own
/// CFG field (MapEditor_PrimaryGrid_Size and so on), into the grid sets.
/// </summary>
public static class ViewportGridSettingsMigration
{
    /// <summary>
    /// Reads the old fields from the config's JSON. Only grid sets missing from the JSON are filled, so a migrated
    /// config is left alone.
    /// </summary>
    public static void Migrate(Application.CFG cfg, string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        if (!root.TryGetProperty(nameof(Application.CFG.MapEditor_Grids), out _))
        {
            MigrateSet(root, "MapEditor", cfg.MapEditor_Grids, true);
        }

        var hasModelGrids = root.TryGetProperty(nameof(Application.CFG.ModelEditor_Grids), out _);
        if (!hasModelGrids)
        {
            MigrateSet(root, "ModelEditor", cfg.ModelEditor_Grids, false);
        }

        // The Time Act Editor used the Model Editor's grids, so it starts from a copy of them
        if (!root.TryGetProperty(nameof(Application.CFG.TimeActEditor_Grids), out _) && !hasModelGrids)
        {
            cfg.TimeActEditor_Grids = cfg.ModelEditor_Grids.Clone();
        }
    }

    private static void MigrateSet(JsonElement root, string prefix, ViewportGridSet set, bool hasSnapSettings)
    {
        foreach (var type in new[] { ViewportGridType.Primary, ViewportGridType.Secondary, ViewportGridType.Tertiary })
        {
            var grid = set.Get(type);
            var name = $"{prefix}_{type}Grid";

            ReadBool(root, $"{prefix}_Display{type}Grid", ref grid.Display);
            ReadInt(root, $"{name}_Size", ref grid.Size);
            ReadFloat(root, $"{name}_SectionSize", ref grid.SectionSize);
            ReadVector3(root, $"{name}_Color", ref grid.Color);

            ReadFloat(root, $"{name}_Position_X", ref grid.Position.X);
            ReadFloat(root, $"{name}_Position_Y", ref grid.Position.Y);
            ReadFloat(root, $"{name}_Position_Z", ref grid.Position.Z);

            ReadFloat(root, $"{name}_Rotation_X", ref grid.Rotation.X);
            ReadFloat(root, $"{name}_Rotation_Y", ref grid.Rotation.Y);
            ReadFloat(root, $"{name}_Rotation_Z", ref grid.Rotation.Z);

            if (hasSnapSettings)
            {
                ReadBool(root, $"{name}_Configure_ApplyPosition_X", ref grid.SnapPosition_X);
                ReadBool(root, $"{name}_Configure_ApplyPosition_Y", ref grid.SnapPosition_Y);
                ReadBool(root, $"{name}_Configure_ApplyPosition_Z", ref grid.SnapPosition_Z);

                ReadBool(root, $"{name}_Configure_ApplyRotation_X", ref grid.SnapRotation_X);
                ReadBool(root, $"{name}_Configure_ApplyRotation_Y", ref grid.SnapRotation_Y);
                ReadBool(root, $"{name}_Configure_ApplyRotation_Z", ref grid.SnapRotation_Z);

                if (root.TryGetProperty($"{name}_Configure_RootAxis", out var axis)
                    && axis.ValueKind == JsonValueKind.Number
                    && axis.TryGetInt32(out var axisValue)
                    && Enum.IsDefined(typeof(ViewportGridRootAxis), axisValue))
                {
                    grid.RootAxis = (ViewportGridRootAxis)axisValue;
                }
            }
        }
    }

    private static void ReadBool(JsonElement root, string name, ref bool value)
    {
        if (root.TryGetProperty(name, out var e) && e.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            value = e.GetBoolean();
        }
    }

    private static void ReadInt(JsonElement root, string name, ref int value)
    {
        if (root.TryGetProperty(name, out var e) && e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var v))
        {
            value = v;
        }
    }

    private static void ReadFloat(JsonElement root, string name, ref float value)
    {
        if (root.TryGetProperty(name, out var e) && e.ValueKind == JsonValueKind.Number && e.TryGetSingle(out var v))
        {
            value = v;
        }
    }

    private static void ReadVector3(JsonElement root, string name, ref Vector3 value)
    {
        if (root.TryGetProperty(name, out var e) && e.ValueKind == JsonValueKind.Object)
        {
            ReadFloat(e, "X", ref value.X);
            ReadFloat(e, "Y", ref value.Y);
            ReadFloat(e, "Z", ref value.Z);
        }
    }
}
