// Checks ViewportGridSettingsMigration against a saved Configuration.json, read-only.
// Usage: GridSettingsMigrationCheck <path to Configuration.json>
using StudioCore.Application;
using StudioCore.Editors.Viewport;
using System.Text.Json;

var path = args.Length > 0 ? args[0]
    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Smithbox", "Configuration", "Configuration.json");

var json = File.ReadAllText(path);
using var doc = JsonDocument.Parse(json);
var root = doc.RootElement;

var cfg = new CFG();
ViewportGridSettingsMigration.Migrate(cfg, json);

var failures = 0;
void Check(string label, object expected, object actual)
{
    var ok = Equals(expected, actual);
    if (!ok) failures++;
    Console.WriteLine($"{(ok ? "ok  " : "FAIL")} {label}: expected {expected}, got {actual}");
}

float F(string name) => root.GetProperty(name).GetSingle();

foreach (var (prefix, set, snap) in new[] { ("MapEditor", cfg.MapEditor_Grids, true), ("ModelEditor", cfg.ModelEditor_Grids, false) })
{
    foreach (var type in Enum.GetValues<ViewportGridType>())
    {
        var g = set.Get(type);
        var n = $"{prefix}_{type}Grid";
        Check($"{n} Display", root.GetProperty($"{prefix}_Display{type}Grid").GetBoolean(), g.Display);
        Check($"{n} Size", root.GetProperty($"{n}_Size").GetInt32(), g.Size);
        Check($"{n} SectionSize", F($"{n}_SectionSize"), g.SectionSize);
        Check($"{n} Color.X", root.GetProperty($"{n}_Color").GetProperty("X").GetSingle(), g.Color.X);
        Check($"{n} Position", (F($"{n}_Position_X"), F($"{n}_Position_Y"), F($"{n}_Position_Z")), (g.Position.X, g.Position.Y, g.Position.Z));
        Check($"{n} Rotation", (F($"{n}_Rotation_X"), F($"{n}_Rotation_Y"), F($"{n}_Rotation_Z")), (g.Rotation.X, g.Rotation.Y, g.Rotation.Z));
        if (snap)
        {
            Check($"{n} SnapPosition_Y", root.GetProperty($"{n}_Configure_ApplyPosition_Y").GetBoolean(), g.SnapPosition_Y);
            Check($"{n} RootAxis", root.GetProperty($"{n}_Configure_RootAxis").GetInt32(), (int)g.RootAxis);
        }
    }
}

// Time Act starts as a copy of the Model Editor's grids, not shared with them
var tae = cfg.TimeActEditor_Grids.Get(ViewportGridType.Secondary);
var model = cfg.ModelEditor_Grids.Get(ViewportGridType.Secondary);
Check("TimeAct copy Rotation", model.Rotation, tae.Rotation);
Check("TimeAct is a separate object", false, ReferenceEquals(tae, model));

// A migrated config is left alone: run again on JSON that already has the grid sets
var options = new JsonSerializerOptions { IncludeFields = true };
var migratedJson = JsonSerializer.Serialize(cfg, options);
var again = new CFG();
again.MapEditor_Grids.Primary.Size = 7;
ViewportGridSettingsMigration.Migrate(again, migratedJson);
Check("Second migration leaves grid sets alone", 7, again.MapEditor_Grids.Primary.Size);

Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILED");
return failures == 0 ? 0 : 1;
