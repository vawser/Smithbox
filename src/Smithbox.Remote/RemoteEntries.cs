using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Create and view MSB entries: region-new/-list, event-new/-set/-list, inspect (everything about a part),
///     lm-preview (darken a model's lightmap in the view only) and bookmark (named camera positions).
/// </summary>
public static partial class RemoteControl
{
    // Per-model lightmap factor for lm-preview; only in Smithbox builds that have it
    private static Dictionary<string, float> ModelLightFactors =>
        typeof(DolLightmapBank).GetField("ModelLightFactor")?.GetValue(null) as Dictionary<string, float>;

    #region References

    /// <summary>Names of the entries in one of the map's lists (Parts, Regions, Events)</summary>
    private static HashSet<string> EntryNames(MapContainer map, string list) =>
        MapEntries(map).Where(e => ListOf(e.WrappedObject) == list).Select(e => e.Name).ToHashSet();

    private static string ListOfType(Type t) =>
        typeof(IMsbPart).IsAssignableFrom(t) ? "Parts" :
        typeof(IMsbRegion).IsAssignableFrom(t) ? "Regions" :
        typeof(IMsbEvent).IsAssignableFrom(t) ? "Events" : null;

    /// <summary>References of an entry (fields with MSBReference): field, name, target list</summary>
    private static IEnumerable<(string Field, string Name, string List)> References(object entry)
    {
        foreach (var p in entry.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var attr = p.GetCustomAttribute<MSBReference>();
            if (attr?.ReferenceType == null)
                continue;

            var list = ListOfType(attr.ReferenceType);
            var v = p.GetValue(entry);

            if (v is string s && !string.IsNullOrEmpty(s))
                yield return (p.Name, s, list);
            else if (v is string[] arr)
                for (var i = 0; i < arr.Length; i++)
                    if (!string.IsNullOrEmpty(arr[i]))
                        yield return ($"{p.Name}[{i}]", arr[i], list);
        }
    }

    /// <summary>References to names that don't exist in the map (SoulsFormats would throw on save otherwise)</summary>
    private static List<string> MissingReferences(MapContainer map, object entry)
    {
        var names = new Dictionary<string, HashSet<string>>();
        var missing = new List<string>();

        foreach (var (field, name, list) in References(entry))
        {
            if (list == null)
                continue;
            if (!names.TryGetValue(list, out var set))
                names[list] = set = EntryNames(map, list);
            if (!set.Contains(name))
                missing.Add($"{field}={name} (no entry in {list})");
        }

        return missing;
    }

    /// <summary>Entries of the map that reference name</summary>
    private static IEnumerable<(MsbEntity Ent, string Field)> ReferencesTo(MapContainer map, string name) =>
        MapEntries(map).SelectMany(e => References(e.WrappedObject).Where(r => r.Name == name).Select(r => (e, r.Field)));

    #endregion

    #region region-new, region-list

    private static readonly string[] BbEventTypes = typeof(MSBB.Event).GetNestedTypes()
        .Where(t => typeof(MSBB.Event).IsAssignableFrom(t) && !t.IsAbstract).Select(t => t.Name).ToArray();

    private static MSB.Shape MakeShape(string kind, JsonObject args)
    {
        var size = GetVector(args, "size");
        float Dim(string name, int axis, float fallback) => Has(args, name) ? GetFloat(args, name, fallback) : size is Vector3 s ? Get(s, axis) : fallback;

        return kind.ToLowerInvariant() switch
        {
            "point" => new MSB.Shape.Point(),
            "circle" => new MSB.Shape.Circle { Radius = GetFloat(args, "radius", size?.X ?? 1) },
            "sphere" => new MSB.Shape.Sphere { Radius = GetFloat(args, "radius", size?.X ?? 1) },
            "cylinder" => new MSB.Shape.Cylinder { Radius = GetFloat(args, "radius", size?.X ?? 1), Height = Dim("height", 1, 2) },
            "rect" or "rectangle" or "square" => new MSB.Shape.Rectangle { Width = Dim("width", 0, 2), Depth = Dim("depth", 2, 2) },
            "box" => new MSB.Shape.Box { Width = Dim("width", 0, 2), Height = Dim("height", 1, 2), Depth = Dim("depth", 2, 2) },
            _ => throw new RemoteException($"Unknown shape '{kind}': point, circle, sphere, cylinder, rect, box")
        };
    }

    private static JsonObject DescribeShape(MSB.Shape shape)
    {
        var o = new JsonObject { ["shape"] = shape?.GetType().Name };
        if (shape == null)
            return o;
        foreach (var p in shape.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType == typeof(float)))
            o[p.Name] = Round((float)p.GetValue(shape)!);
        return o;
    }

    private static JsonObject DescribeRegionFull(MsbEntity ent, MapContainer map)
    {
        var o = DescribeRegion(ent);
        if (ent.WrappedObject.GetType().GetProperty("Shape")?.GetValue(ent.WrappedObject) is MSB.Shape shape)
            foreach (var (k, v) in DescribeShape(shape).ToList())
                o[k] = v?.DeepClone();
        o["entityId"] = ReadField(ent.WrappedObject, "EntityID") is int id ? id : null;
        o["usedBy"] = new JsonArray(ReferencesTo(map, ent.Name).Select(r => (JsonNode)$"{r.Ent.Name} ({PartTypeName(r.Ent)}.{r.Field})").ToArray());
        return o;
    }

    /// <summary>New event object (MSBB.Event.&lt;type&gt;) with name and EventID (map's max+1)</summary>
    private static MSBB.Event NewEvent(MapContainer target, string type, string name)
    {
        if (target.Objects.OfType<MsbEntity>().FirstOrDefault(e => e.WrappedObject != null)?.WrappedObject is not (MSBB.Part or MSBB.Region or MSBB.Event))
            throw new RemoteException("event-new/region-new only know Bloodborne MSBs (MSBB)");

        var t = typeof(MSBB.Event).GetNestedType(BbEventTypes.FirstOrDefault(n => n.Equals(type, StringComparison.OrdinalIgnoreCase)) ?? "-")
                ?? throw new RemoteException($"Unknown event type '{type}': {string.Join(", ", BbEventTypes)}");
        var ev = (MSBB.Event)Activator.CreateInstance(t)!;

        // Defaults from the first event of the type in the map (numbers only, no names, IDs or references)
        if (MapEntries(target).Select(e => e.WrappedObject).FirstOrDefault(e => e?.GetType() == t) is MSBB.Event template)
        {
            foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (p.CanWrite && p.GetSetMethod() != null && IsSimple(p.PropertyType) && p.PropertyType != typeof(string) &&
                    p.Name is not ("EventID" or "EntityID"))
                    p.SetValue(ev, p.GetValue(template));
            }

            LastEventTemplate = template.Name;
        }
        else
        {
            LastEventTemplate = null;
        }

        ev.Name = name;
        ev.EventID = MapEntries(target).Select(e => e.WrappedObject).OfType<MSBB.Event>().Select(e => e.EventID).DefaultIfEmpty(-1).Max() + 1;
        return ev;
    }

    // The event NewEvent took its defaults from, for the answer
    private static string LastEventTemplate;

    private static IEnumerator RegionNew(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var name = RequireString(args, "name");

        if (target.GetObjectByName(name) != null)
            throw new RemoteException($"{target.Name} already has an object named '{name}'");

        var shapes = new[] { "point", "circle", "sphere", "cylinder", "rect", "rectangle", "square", "box" };
        var kind = GetString(args, "shape") ?? "point";
        var eventType = GetString(args, "event");

        // type=Sound/SFX/...: BB regions have only a shape; the type is an event that points at the region
        if (GetString(args, "type") is string typeArg)
        {
            if (shapes.Contains(typeArg.ToLowerInvariant()))
                kind = Has(args, "shape") ? kind : typeArg;
            else if (typeArg.Equals("ActivationArea", StringComparison.OrdinalIgnoreCase) || !BbEventTypes.Contains(typeArg, StringComparer.OrdinalIgnoreCase))
                throw new RemoteException($"Bloodborne regions have no type, only a shape (point, circle, sphere, cylinder, rect, box). " +
                                          $"What they mean is set by an event with RegionName ({string.Join(", ", BbEventTypes)}); " +
                                          $"ActivationArea does not exist in BB (triggers are done by EMEVD via the region's EntityID)");
            else
                eventType ??= typeArg;
        }

        var shape = MakeShape(kind, args);
        var pos = new[] { GetVector(args, "pos") ?? throw new RemoteException("Missing argument 'pos'") };
        var snap = SnapToFloor(view, args, pos);
        while (snap.MoveNext())
            yield return null;

        var rot = GetVector(args, "rot") ?? new Vector3(0, GetFloat(args, "yaw", 0), 0);
        var region = new MSBB.Region
        {
            Name = name, Shape = shape, Position = pos[0], Rotation = rot,
            EntityID = (int)GetFloat(args, "entityId", -1),
        };

        MSBB.Event ev = null;
        if (eventType != null)
        {
            ev = NewEvent(target, eventType, GetString(args, "eventName") ?? $"{name} ({eventType})");
            if (target.GetObjectByName(ev.Name) != null)
                throw new RemoteException($"{target.Name} already has an object named '{ev.Name}' (eventName=)");
        }

        var regionEnt = new MsbEntity(view.Universe, target, region, MsbEntityType.Region);
        MsbEntity eventEnt = null;

        var step = new RemoteStep($"region-new {name}", st =>
        {
            st.Run(new EntAddAction(view, target, new List<MsbEntity> { regionEnt }, target.RootObject));
            st.Records.Add((new JsonObject
            {
                ["op"] = "region-new", ["name"] = name, ["map"] = target.Name, ["shape"] = DescribeShape(shape),
                ["pos"] = ToJson(pos[0]), ["rot"] = ToJson(rot), ["entityId"] = region.EntityID,
            }, new JsonObject { ["op"] = "region-delete", ["name"] = name, ["map"] = target.Name }));

            if (ev != null)
            {
                eventEnt = new MsbEntity(view.Universe, target, ev, MsbEntityType.Event);
                st.Run(new EntAddAction(view, target, new List<MsbEntity> { eventEnt }, target.RootObject));
                var fields = EventFields(args, ev, "event");
                fields["RegionName"] = name;
                if (ev is MSBB.Event.WindSFX)
                    fields["WindRegionName"] = name;
                foreach (var a in FieldActions(eventEnt, fields))
                    st.Run(a);
                var missing = MissingReferences(target, ev);
                if (missing.Count > 0)
                    throw new RemoteException("References without a target: " + string.Join("; ", missing));
                st.Records.Add((new JsonObject
                {
                    ["op"] = "event-new", ["name"] = ev.Name, ["map"] = target.Name, ["type"] = ev.GetType().Name, ["fields"] = fields.DeepClone(),
                }, new JsonObject { ["op"] = "event-delete", ["name"] = ev.Name, ["map"] = target.Name }));
            }
        });

        ExecuteStep(view, step, false);
        yield return null;

        result["region"] = DescribeRegionFull(regionEnt, target);
        if (eventEnt != null)
            result["event"] = DescribeEvent(eventEnt);
        result["records"] = RecordsJson(step);
    }

    private static IEnumerator RegionList(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var maps = Has(args, "map")
            ? new List<MapContainer> { RequireLoadedMap(view, GetString(args, "map")) }
            : LoadedMaps(view.Editor.Project).Select(m => m.Value).ToList();
        var filter = GetString(args, "filter") is string f ? new Regex(f, RegexOptions.IgnoreCase) : null;
        var shape = GetString(args, "shape") is string s ? new Regex(s, RegexOptions.IgnoreCase) : null;
        var near = GetVector(args, "near");
        var radius = GetFloat(args, "radius", 10);
        var limit = (int)GetFloat(args, "limit", 200);
        var list = new JsonArray();
        var total = 0;

        foreach (var map in maps)
        {
            foreach (var ent in MapEntries(map).Where(e => e.WrappedObject is IMsbRegion))
            {
                if (filter != null && !filter.IsMatch(ent.Name ?? ""))
                    continue;
                var sh = ent.WrappedObject.GetType().GetProperty("Shape")?.GetValue(ent.WrappedObject) as MSB.Shape;
                if (shape != null && !shape.IsMatch(sh?.GetType().Name ?? ""))
                    continue;
                if (near is Vector3 c && Vector3.Distance(c, GetPartVector(ent, "Position")) > radius)
                    continue;
                if (++total <= limit)
                    list.Add(DescribeRegionFull(ent, map));
            }
        }

        result["total"] = total;
        result["regions"] = list;
        yield break;
    }

    #endregion

    #region event-new, event-set, event-list

    /// <summary>
    ///     Fields from the shorthand arguments: part=, region=, entityId=, eventId=, points= (PatrolInfo), patrolType=, itemLots= (Treasure),
    ///     paramId=/flag=/state=/objActEntityId= (ObjAct), soundType=/soundId= (Sound), effectId= (SFX), plus fields={...}.
    ///     prefix: "event" reads eventFields= instead of fields= (region-new)
    /// </summary>
    private static JsonObject EventFields(JsonObject args, MSBB.Event ev, string prefix = null)
    {
        var fieldsKey = prefix == null ? "fields" : prefix + "Fields";
        var fields = args[fieldsKey] is JsonObject o ? (JsonObject)o.DeepClone()
            : GetString(args, fieldsKey) is string text ? JsonNode.Parse(text) as JsonObject ?? throw new RemoteException($"{fieldsKey} must be a JSON object")
            : new JsonObject();

        if (prefix != null)
            return fields;

        void Put(string arg, string field, Func<string, JsonNode> conv = null)
        {
            if (GetString(args, arg) is string v)
                fields[field] = conv != null ? conv(v) : v;
        }

        JsonNode Int(string v) => int.Parse(v, CultureInfo.InvariantCulture);

        Put("region", "RegionName");
        Put("entityId", "EntityID", Int);
        Put("eventId", "EventID", Int);

        switch (ev)
        {
            case MSBB.Event.Treasure:
                Put("part", "TreasurePartName");
                Put("collision", "PartName");
                if (GetString(args, "itemLots") is string lots)
                {
                    var l = lots.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x, CultureInfo.InvariantCulture)).ToArray();
                    for (var i = 0; i < 3; i++)
                        fields[$"ItemLot{i + 1}"] = i < l.Length ? l[i] : -1;
                }

                Put("inChest", "InChest", v => v is "1" or "true");
                break;
            case MSBB.Event.ObjAct:
                Put("part", "ObjActPartName");
                Put("objActEntityId", "ObjActEntityID", Int);
                Put("paramId", "ObjActParamID", Int);
                Put("flag", "EventFlagID", Int);
                Put("state", "ObjActState", Int);
                break;
            case MSBB.Event.PatrolInfo pi:
                Put("part", "PartName");
                Put("patrolType", "PatrolType", Int);
                if (GetString(args, "points") is string pts)
                {
                    var names = pts.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                    if (names.Length > pi.WalkPointNames.Length)
                        throw new RemoteException($"PatrolInfo has room for {pi.WalkPointNames.Length} points");
                    for (var i = 0; i < pi.WalkPointNames.Length; i++)
                        fields[$"WalkPointNames[{i}]"] = i < names.Length ? names[i] : null;
                }

                break;
            case MSBB.Event.Sound:
                Put("part", "PartName");
                Put("soundType", "SoundType", Int);
                Put("soundId", "SoundID", Int);
                break;
            case MSBB.Event.SFX:
                Put("part", "PartName");
                Put("effectId", "EffectID", Int);
                break;
            default:
                Put("part", "PartName");
                break;
        }

        return fields;
    }

    private static JsonObject DescribeEvent(MsbEntity ent)
    {
        var ev = ent.WrappedObject;
        var o = new JsonObject { ["map"] = ent.MapID, ["name"] = ent.Name, ["type"] = ev.GetType().Name };
        var t = ev.GetType();

        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (p.Name == "Name" || p.Name.StartsWith("Unk", StringComparison.Ordinal) || p.GetIndexParameters().Length > 0)
                continue;
            var v = p.GetValue(ev);
            if (v is string[] arr)
            {
                var used = arr.Where(x => !string.IsNullOrEmpty(x)).Select(x => (JsonNode)x).ToArray();
                if (used.Length > 0)
                    o[p.Name] = new JsonArray(used);
            }
            else if (v != null && IsSimple(p.PropertyType) && (p.DeclaringType != typeof(MSBB.Event) || p.Name is "EventID" or "EntityID" or "PartName" or "RegionName"))
            {
                o[p.Name] = ValueToJson(v);
            }
        }

        return o;
    }

    /// <summary>For PatrolInfo: enemy=a,b sets the enemies' PatrolInfoName to the event</summary>
    private static List<ViewportAction> EnemyPatrolActions(MapContainer target, JsonObject args, string eventName, JsonArray changed)
    {
        var actions = new List<ViewportAction>();
        foreach (var e in (GetString(args, "enemy") ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var enemy = RequirePart(target, e.Trim());
            if (enemy.WrappedObject.GetType().GetProperty("PatrolInfoName") == null)
                throw new RemoteException($"{enemy.Name} ({PartTypeName(enemy)}) has no PatrolInfoName");
            actions.AddRange(FieldActions(enemy, new JsonObject { ["PatrolInfoName"] = eventName }));
            changed.Add(enemy.Name);
        }

        return actions;
    }

    private static IEnumerator EventNew(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var type = RequireString(args, "type");
        var name = RequireString(args, "name");

        if (target.GetObjectByName(name) != null)
            throw new RemoteException($"{target.Name} already has an object named '{name}'");

        var ev = NewEvent(target, type, name);
        var template = LastEventTemplate;
        var fields = EventFields(args, ev);

        // Treasure: PartName is the collision the item lies on (as in the original maps), found below the part
        if (ev is MSBB.Event.Treasure && !fields.ContainsKey("PartName") && GetString(fields, "TreasurePartName") is string tp &&
            !GetBool(args, "noCollision", false))
        {
            var at = GetPartVector(RequirePart(target, tp), "Position");
            var below = new FloorResult();
            var cast = CastDown(view, at.X, at.Z, at.Y + 0.5f, true, below);
            while (cast.MoveNext())
                yield return null;
            var hit = below.Hits.FirstOrDefault(h => h.Mesh?.Kind == FirstPersonMeshKind.Map && h.Mesh.MapId == target.Name);
            if (hit.Mesh != null)
            {
                fields["PartName"] = hit.Mesh.PartName;
                result["collision"] = hit.Mesh.PartName;
            }
        }

        var ent = new MsbEntity(view.Universe, target, ev, MsbEntityType.Event);
        var enemies = new JsonArray();

        var step = new RemoteStep($"event-new {name}", st =>
        {
            st.Run(new EntAddAction(view, target, new List<MsbEntity> { ent }, target.RootObject));
            foreach (var a in FieldActions(ent, fields))
                st.Run(a);
            foreach (var a in EnemyPatrolActions(target, args, name, enemies))
                st.Run(a);

            var missing = MissingReferences(target, ev);
            if (missing.Count > 0)
                throw new RemoteException("References without a target: " + string.Join("; ", missing));

            st.Records.Add((new JsonObject
            {
                ["op"] = "event-new", ["name"] = name, ["map"] = target.Name, ["type"] = ev.GetType().Name, ["fields"] = fields.DeepClone(),
                ["enemies"] = enemies.DeepClone(),
            }, new JsonObject { ["op"] = "event-delete", ["name"] = name, ["map"] = target.Name }));
        });

        ExecuteStep(view, step, false);
        yield return null;

        result["event"] = DescribeEvent(ent);
        result["template"] = template;
        result["enemies"] = enemies;
        result["records"] = RecordsJson(step);
    }

    private static IEnumerator EventSet(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var ent = RequireEntry(target, RequireString(args, "name"), "Events");
        var ev = ent.WrappedObject as MSBB.Event ?? throw new RemoteException("event-set only knows Bloodborne events (MSBB)");
        var fields = EventFields(args, ev);
        var enemies = new JsonArray();

        if (fields.Count == 0 && !Has(args, "enemy"))
            throw new RemoteException("Nothing to set: part=, region=, points=, itemLots=, paramId=, ..., fields={...} or enemy=");

        var step = new RemoteStep($"event-set {ent.Name}", st =>
        {
            foreach (var a in FieldActions(ent, fields))
                st.Run(a);
            foreach (var a in EnemyPatrolActions(target, args, ent.Name, enemies))
                st.Run(a);

            var missing = MissingReferences(target, ev);
            if (missing.Count > 0)
                throw new RemoteException("References without a target: " + string.Join("; ", missing));

            st.Records.Add((new JsonObject
            {
                ["op"] = "set", ["name"] = ent.Name, ["map"] = target.Name, ["fields"] = fields.DeepClone(), ["enemies"] = enemies.DeepClone(),
            }, null));
        });

        ExecuteStep(view, step, false);
        yield return null;

        result["event"] = DescribeEvent(ent);
        result["enemies"] = enemies;
        result["records"] = RecordsJson(step);
    }

    private static IEnumerator EventList(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var maps = Has(args, "map")
            ? new List<MapContainer> { RequireLoadedMap(view, GetString(args, "map")) }
            : LoadedMaps(view.Editor.Project).Select(m => m.Value).ToList();
        var type = GetString(args, "type") is string t ? new Regex($"^({t})$", RegexOptions.IgnoreCase) : null;
        var filter = GetString(args, "filter") is string f ? new Regex(f, RegexOptions.IgnoreCase) : null;
        var refers = GetString(args, "refers") ?? GetString(args, "part") ?? GetString(args, "region");
        var limit = (int)GetFloat(args, "limit", 200);
        var list = new JsonArray();
        var total = 0;

        foreach (var map in maps)
        {
            foreach (var ent in MapEntries(map).Where(e => e.WrappedObject is IMsbEvent))
            {
                if (type != null && !type.IsMatch(ent.WrappedObject.GetType().Name))
                    continue;
                if (filter != null && !filter.IsMatch(ent.Name ?? ""))
                    continue;
                if (refers != null && References(ent.WrappedObject).All(r => r.Name != refers) &&
                    !(map.GetObjectByName(refers) is MsbEntity src && src.WrappedObject != null &&
                      References(src.WrappedObject).Any(r => r.Name == ent.Name && r.List == "Events")))
                    continue;
                if (++total > limit)
                    continue;

                var d = DescribeEvent(ent);
                var missing = MissingReferences(map, ent.WrappedObject);
                if (missing.Count > 0)
                    d["missingRefs"] = new JsonArray(missing.Select(m => (JsonNode)m).ToArray());
                list.Add(d);
            }
        }

        result["total"] = total;
        result["types"] = new JsonArray(BbEventTypes.Select(x => (JsonNode)x).ToArray());
        result["events"] = list;
        yield break;
    }

    #endregion

    #region inspect, lm-preview

    private class LightmapIndexInfo
    {
        public string Code;
        public HashSet<string> Names;
        public HashSet<string> CodeModels;
        public Dictionary<string, string> CodeByModel;
        public Dictionary<string, List<BTAB.Entry>> Atlas;
    }

    /// <summary>Lightmap index of the area (DolLightmapBank, via reflection), waits until it is loaded</summary>
    private static IEnumerator LoadLightmapIndex(MapEditorView view, MapContainer target, LightmapIndexInfo into)
    {
        var bank = view.Editor.Project.Handler.MapData.DolLightmapBank ?? throw new RemoteException("No lightmap bank");
        var t = bank.GetType();
        const BindingFlags any = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
        into.Code = (string)t.GetMethod("GetMapCode", any)!.Invoke(null, new object[] { target.Name })
                    ?? throw new RemoteException($"No map code for {target.Name}");
        var index = t.GetMethod("GetAreaIndex", any)!.Invoke(bank, new object[] { target.Name.Substring(0, 3) });
        var it = index.GetType();

        var watch = System.Diagnostics.Stopwatch.StartNew();
        while (!(bool)it.GetField("Ready")!.GetValue(index))
        {
            if (watch.Elapsed.TotalSeconds > 120)
                throw new RemoteException("The lightmap index did not load (no gi_env_<area>.tpfbhd?)");
            yield return null;
        }

        into.Names = (HashSet<string>)it.GetField("Names")!.GetValue(index);
        // Only in builds with the baked map code fallback; without them no part is reported as onlyFallback
        into.CodeModels = it.GetField("CodeModels")?.GetValue(index) as HashSet<string>
                          ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        into.CodeByModel = it.GetField("CodeByModel")?.GetValue(index) as Dictionary<string, string>
                           ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        into.Atlas = (Dictionary<string, List<BTAB.Entry>>)t.GetMethod("GetAtlasEntries", any)!.Invoke(null, new object[] { target });
    }

    /// <summary>"lit" (the game finds it), "onlyFallback" (only Smithbox finds it, via the baked map code), "missing"</summary>
    private static string LightmapStatus(LightmapIndexInfo lm, string part, string model, out string expected, out string smithboxFinds)
    {
        bool Found(string c, out string name)
        {
            var prefix = $"{c}_{model}_gi_0000_00_dol_";
            var suffix = lm.Atlas.TryGetValue(part, out var e) && e.Count > 0 ? $"_atlas{e[0].AtlasID:D3}" : "";
            name = $"{prefix}00{suffix}";
            return lm.Names.Contains(name) && lm.Names.Contains($"{prefix}01{suffix}");
        }

        smithboxFinds = null;
        if (Found(lm.Code, out expected))
            return "lit";

        if (!lm.CodeModels.Contains($"{lm.Code}_{model}") && lm.CodeByModel.TryGetValue(model, out var baked) && Found(baked, out var bakedName))
        {
            smithboxFinds = bakedName;
            return "onlyFallback";
        }

        return "missing";
    }

    /// <summary>Texture names Smithbox has loaded; known: all requested ones</summary>
    private static HashSet<string> LoadedTextures(out HashSet<string> known)
    {
        var loaded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        known = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pool in ResourceManager.Pools)
        foreach (var h in pool.Handles)
        {
            if (h.ResourceType != typeof(TextureResource))
                continue;
            var n = TextureName(h.Name);
            known.Add(n);
            if (h.IsLoaded())
                loaded.Add(n);
        }

        return loaded;
    }

    private static JsonArray GroupBits(uint[] groups)
    {
        var bits = new JsonArray();
        for (var i = 0; i < groups.Length; i++)
            for (var b = 0; b < 32; b++)
                if ((groups[i] >> b & 1) != 0)
                    bits.Add(i * 32 + b);
        return bits;
    }

    private static IEnumerator Inspect(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var project = view.Editor.Project;
        var ent = FindEntity(project, GetString(args, "map"), RequireString(args, "name")) as MsbEntity;

        if (ent?.WrappedObject is not IMsbPart part)
            throw new RemoteException("inspect needs a part (name=)");

        var map = RequireLoadedMap(view, ent.MapID);
        var model = part.ModelName;
        var type = PartTypeName(ent);

        foreach (var (k, v) in DescribePart(ent).ToList())
            result[k] = v?.DeepClone();
        result["entityId"] = ReadField(ent.WrappedObject, "EntityID") is int eid ? eid : null;

        // Groups: DrawGroups, DispGroups, BackreadGroups (bits set)
        var groups = new JsonObject();
        foreach (var p in ent.WrappedObject.GetType().GetProperties().Where(p => p.PropertyType == typeof(uint[]) && p.Name.Contains("Groups")))
            groups[p.Name] = GroupBits((uint[])p.GetValue(ent.WrappedObject)!);
        result["groups"] = groups;

        if (TryLocalBounds(ent, out var lb))
        {
            var b = ent.RenderSceneMesh.GetBounds();
            result["bbox"] = new JsonObject
            {
                ["min"] = ToJson(b.Min), ["max"] = ToJson(b.Max), ["size"] = ToJson(b.Max - b.Min),
                ["localSize"] = ToJson((lb.Max - lb.Min) * PartScale(ent)),
            };
        }

        // Model, materials, textures (loaded or not)
        var origin = Origins.TryGetValue(ent, out var o) ? o[0] : null;
        var flver = ReadFlver(project, ent.MapID, model) ?? (origin != null ? ReadFlver(project, origin, model) : null);
        var loadedTex = LoadedTextures(out var knownTex);
        var missingTex = new JsonArray();

        if (flver != null)
        {
            var materials = new JsonArray();
            foreach (var m in flver.Materials)
            {
                var tex = new JsonArray();
                foreach (var t in m.Textures.Where(t => !string.IsNullOrEmpty(t.Path)))
                {
                    var tn = TextureName(t.Path);
                    var lightmap = t.ParamName.Contains("DOL", StringComparison.OrdinalIgnoreCase);
                    var ok = lightmap || loadedTex.Contains(tn);
                    tex.Add(new JsonObject
                    {
                        ["param"] = t.ParamName, ["texture"] = tn, ["loaded"] = lightmap ? null : ok,
                        ["requested"] = lightmap ? null : knownTex.Contains(tn),
                    });
                    if (!ok)
                        missingTex.Add(tn);
                }

                materials.Add(new JsonObject { ["name"] = m.Name, ["mtd"] = Path.GetFileName(m.MTD ?? ""), ["textures"] = tex });
            }

            result["model"] = new JsonObject
            {
                ["name"] = model, ["found"] = true, ["meshes"] = flver.Meshes.Count,
                ["vertices"] = flver.Meshes.Sum(x => x.Vertices.Count), ["materials"] = materials,
                ["boundsMin"] = ToJson(flver.Header.BoundingBoxMin), ["boundsMax"] = ToJson(flver.Header.BoundingBoxMax),
            };
        }
        else
        {
            result["model"] = new JsonObject { ["name"] = model, ["found"] = false };
        }

        result["missingTextures"] = missingTex;

        // Lightmap and BTAB (map pieces)
        if (ent.WrappedObject is MSBB.Part.MapPiece)
        {
            var lm = new LightmapIndexInfo();
            var load = LoadLightmapIndex(view, map, lm);
            while (load.MoveNext())
                yield return null;

            var status = LightmapStatus(lm, ent.Name, model, out var expected, out var finds);
            var btab = lm.Atlas.TryGetValue(ent.Name, out var entries) ? entries : new List<BTAB.Entry>();
            result["lightmap"] = new JsonObject
            {
                ["status"] = status, ["expected"] = expected, ["smithboxFinds"] = finds,
                ["bound"] = (ent.RenderSceneMesh as MeshRenderableProxy)?.Lightmap != null,
                ["btab"] = new JsonArray(btab.Select(e => (JsonNode)new JsonObject
                {
                    ["atlas"] = e.AtlasID, ["material"] = e.MaterialName, ["uvScale"] = new JsonArray(e.UVScale.X, e.UVScale.Y),
                    ["uvOffset"] = new JsonArray(e.UVOffset.X, e.UVOffset.Y),
                }).ToArray()),
                ["viewFactor"] = ModelLightFactors != null && ModelLightFactors.TryGetValue(model, out var vf) ? vf : null,
            };
        }

        // Collision: the part's own reference fields, and the collision meshes in its box
        var refs = new JsonObject();
        foreach (var (field, name, _) in References(ent.WrappedObject))
            refs[field] = name;
        result["references"] = refs;
        result["usedBy"] = new JsonArray(ReferencesTo(map, ent.Name).Select(r => (JsonNode)$"{r.Ent.Name} ({PartTypeName(r.Ent)}.{r.Field})").ToArray());

        if (ent.RenderSceneMesh != null && CFG.Current.MapEditor_ModelLoad_Collisions)
        {
            var collision = EnsureCollision(view, 300);
            while (collision.MoveNext())
                yield return null;

            var box = ent.RenderSceneMesh.GetBounds();
            var counts = new Dictionary<string, (int N, string Source, string Kind)>();
            view.FirstPersonCollision.Current.Overlap(box.Min - new Vector3(0.05f), box.Max + new Vector3(0.05f), (mesh, _, _, _, _) =>
            {
                var key = mesh.PartName ?? "?";
                (int N, string Source, string Kind) c = counts.TryGetValue(key, out var had) ? had : (0, mesh.Source, mesh.Kind.ToString());
                counts[key] = (c.N + 1, c.Source, c.Kind);
            });
            result["collision"] = new JsonObject
            {
                ["own"] = counts.ContainsKey(ent.Name),
                ["inBox"] = new JsonArray(counts.OrderByDescending(c => c.Value.N).Take(12).Select(c => (JsonNode)new JsonObject
                {
                    ["part"] = c.Key, ["triangles"] = c.Value.N, ["source"] = c.Value.Source, ["kind"] = c.Value.Kind,
                }).ToArray()),
            };
        }
    }

    private static IEnumerator LmPreview(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var bank = view.Editor.Project.Handler.MapData.DolLightmapBank ?? throw new RemoteException("No lightmap bank");
        var factors = ModelLightFactors ?? throw new RemoteException("lm-preview needs a Smithbox build with DolLightmapBank.ModelLightFactor");

        if (GetBool(args, "clear", false))
        {
            factors.Clear();
        }
        else if (GetString(args, "model") is string models)
        {
            var factor = GetFloat(args, "factor", float.NaN);
            if (float.IsNaN(factor) || factor < 0)
                throw new RemoteException("Missing argument 'factor' (e.g. 0.35; 1 removes it)");
            foreach (var m in models.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (MathF.Abs(factor - 1) < 1e-6f)
                    factors.Remove(m);
                else
                    factors[m] = factor;
            }
        }

        // Entries made again with the factors on the next frames (the bank resolves them while drawing)
        bank.Reset();
        var watch = System.Diagnostics.Stopwatch.StartNew();
        var settled = 0;
        while (settled < 5)
        {
            yield return null;
            settled = bank.PendingPartCount == 0 && watch.Elapsed.TotalSeconds > 0.2 ? settled + 1 : 0;
            if (watch.Elapsed.TotalSeconds > GetFloat(args, "timeout", 120))
                throw new RemoteException($"Lightmaps still pending after the reset ({bank.PendingPartCount} parts)");
        }

        var idle = WaitIdle(GetFloat(args, "timeout", 120), 4);
        while (idle.MoveNext())
            yield return null;

        var parts = new JsonObject();
        foreach (var (model, f) in factors.ToList())
        {
            var ents = LoadedMaps(view.Editor.Project).SelectMany(m => m.Value.Objects.OfType<MsbEntity>())
                .Where(e => e.WrappedObject is IMsbPart p && string.Equals(p.ModelName, model, StringComparison.OrdinalIgnoreCase)).ToList();
            // The HDR scale of the first part's entry in the lightmap table, as the shader reads it
            JsonNode hdr = null;
            var table = SceneRenderer.Lightmaps;
            var binding = ents.Select(e => (e.RenderSceneMesh as MeshRenderableProxy)?.Lightmap).FirstOrDefault(b => b != null);
            if (table != null && binding != null &&
                typeof(LightmapTable).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(table) is LightmapData[] entries)
            {
                var idx = binding.Entries.First();
                hdr = ToJson(new Vector3(entries[idx].HdrScale.X, entries[idx].HdrScale.Y, entries[idx].HdrScale.Z));
            }

            parts[model] = new JsonObject
            {
                ["factor"] = f, ["parts"] = ents.Count,
                ["withLightmap"] = ents.Count(e => (e.RenderSceneMesh as MeshRenderableProxy)?.Lightmap != null),
                ["hdrScale"] = hdr,
            };
        }

        result["factors"] = parts;
        result["note"] = "View only: the models' lightmap light times factor; map unchanged. Parts without a lightmap (placeholder) don't change. " +
                         "Undo with lm-preview clear=true.";
    }

    #endregion

    #region bookmark

    private static string BookmarkFile(JsonObject args)
    {
        if (GetString(args, "file") is string f)
            return f;

        var project = GetProject() ?? throw new RemoteException("No project is loaded (or give file=)");
        return Path.Combine(project.Descriptor.ProjectPath, ".smithbox", "Remote", "bookmarks.json");
    }

    private static JsonArray ReadBookmarks(string file)
    {
        if (!File.Exists(file))
            return new JsonArray();

        var node = JsonNode.Parse(File.ReadAllText(file));
        return node?["bookmarks"] as JsonArray ?? new JsonArray();
    }

    private static void WriteBookmarks(string file, JsonArray list)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        var tmp = file + ".tmp";
        File.WriteAllText(tmp, new JsonObject { ["bookmarks"] = list.DeepClone() }.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        File.Move(tmp, file, true);
    }

    private static JsonObject FindBookmark(JsonArray list, string name) =>
        list.OfType<JsonObject>().FirstOrDefault(b => GetString(b, "name") == name)
        ?? throw new RemoteException($"No bookmark '{name}' (see bookmark list)");

    private static IEnumerator Bookmark(JsonObject args, JsonObject result)
    {
        var op = GetString(args, "op") ?? new[] { "save", "go", "list", "delete" }.FirstOrDefault(k => Has(args, k)) ?? "list";
        var file = BookmarkFile(args);
        var list = ReadBookmarks(file);
        result["file"] = file;
        result["op"] = op;

        switch (op)
        {
            case "list":
                result["bookmarks"] = list.DeepClone();
                break;
            case "save":
            {
                var name = RequireString(args, "name");
                var viewport = GetViewport(GetView());
                var cam = DescribeCamera(viewport);
                var bm = new JsonObject
                {
                    ["name"] = name, ["pos"] = cam["pos"]?.DeepClone(), ["rot"] = cam["rot"]?.DeepClone(), ["forward"] = cam["forward"]?.DeepClone(),
                    ["fov"] = cam["fov"]?.DeepClone(), ["mode"] = cam["mode"]?.DeepClone(), ["orthoSize"] = cam["orthoSize"]?.DeepClone(),
                    ["maps"] = new JsonArray(LoadedMaps(GetProject()).Select(m => (JsonNode)m.Key).ToArray()),
                    ["note"] = GetString(args, "note"), ["by"] = GetString(args, "by"),
                    ["time"] = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                };
                var old = list.OfType<JsonObject>().FirstOrDefault(b => GetString(b, "name") == name);
                if (old != null)
                    list[list.IndexOf(old)] = bm;
                else
                    list.Add(bm);
                WriteBookmarks(file, list);
                result["bookmark"] = bm.DeepClone();
                result["replaced"] = old != null;
                break;
            }
            case "delete":
            {
                var bm = FindBookmark(list, RequireString(args, "name"));
                list.Remove(bm);
                WriteBookmarks(file, list);
                result["deleted"] = GetString(bm, "name");
                break;
            }
            case "go":
            {
                var view = GetView();
                var viewport = GetViewport(view);
                var bm = FindBookmark(list, RequireString(args, "name"));

                if (GetBool(args, "load", false) && bm["maps"] is JsonArray maps)
                {
                    foreach (var m in maps.Select(x => x?.ToString()).Where(x => x != null))
                    {
                        var load = EnsureMapLoaded(view, m, args, new bool[1]);
                        while (load.MoveNext())
                            yield return null;
                    }
                }

                var camera = viewport.ViewportCamera;
                if (GetString(bm, "mode") is string mode && Enum.TryParse(mode, true, out ViewMode vm))
                    camera.SetProjectionType(vm);
                if (Has(bm, "orthoSize"))
                    camera.SetOrthographicSize(GetFloat(bm, "orthoSize", camera.OrthographicSize));
                if (Has(bm, "fov"))
                    CFG.Current.Viewport_Camera_FOV = GetFloat(bm, "fov", CFG.Current.Viewport_Camera_FOV);
                camera.CameraTransform.Position = GetVector(bm, "pos") ?? camera.CameraTransform.Position;
                if (GetVector(bm, "rot") is Vector3 rot)
                    camera.CameraTransform.EulerRotation = rot * Deg;
                if (viewport.Width > 0 && viewport.Height > 0)
                    camera.UpdateProjectionMatrix();
                yield return null;

                result["bookmark"] = bm.DeepClone();
                result["camera"] = DescribeCamera(viewport);
                break;
            }
            default:
                throw new RemoteException("bookmark save|go|list|delete (op=)");
        }
    }

    #endregion
}
