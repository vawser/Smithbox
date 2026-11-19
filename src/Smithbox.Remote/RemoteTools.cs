using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Veldrid.Utilities;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Check and data commands: ops-export/ops-apply (msbtool ops), set and region-*, leak-scan, plan, diff, overlap,
///     free-spot, los, lightmap-check, texture-check, find-model, model-search.
/// </summary>
public static partial class RemoteControl
{
    #region Entries and fields

    private static string ListOf(object w) => w switch
    {
        IMsbPart => "Parts",
        IMsbRegion => "Regions",
        IMsbEvent => "Events",
        _ => null
    };

    private static IEnumerable<MsbEntity> MapEntries(MapContainer container) =>
        container.Objects.OfType<MsbEntity>().Where(e => e.WrappedObject != null && ListOf(e.WrappedObject) != null);

    private static MsbEntity RequireEntry(MapContainer container, string name, string list = null)
    {
        var ent = MapEntries(container).FirstOrDefault(e => e.Name == name && (list == null || ListOf(e.WrappedObject) == list));
        return ent ?? throw new RemoteException($"No {list ?? "entry"} named '{name}' in {container.Name}");
    }

    private static bool IsSimple(Type t) =>
        t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(Vector3) || t == typeof(Vector2) ||
        t == typeof(decimal) || t == typeof(System.Drawing.Color);

    /// <summary>The fields of an entry as path -> value ("Position", "GparamConfigStruct.LightSetID"), without Name</summary>
    private static IEnumerable<(string Path, object Value)> Fields(object o, string prefix = "", int depth = 0)
    {
        foreach (var p in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanRead || p.GetIndexParameters().Length > 0 || (depth == 0 && p.Name == "Name"))
                continue;

            var t = p.PropertyType;
            object v;

            try
            {
                v = p.GetValue(o);
            }
            catch
            {
                continue;
            }

            if (IsSimple(t) && (p.CanWrite || depth > 0))
                yield return (prefix + p.Name, v);
            else if (t.IsArray && IsSimple(t.GetElementType()))
                yield return (prefix + p.Name, v);
            else if (depth == 0 && v != null && !t.IsArray && !typeof(IEnumerable).IsAssignableFrom(t) &&
                     t.Namespace?.StartsWith("SoulsFormats") == true)
            {
                foreach (var f in Fields(v, prefix + p.Name + ".", 1))
                    yield return f;
            }
        }
    }

    private static bool SameValue(object a, object b)
    {
        if (a is Array x && b is Array y)
            return x.Length == y.Length && x.Cast<object>().Zip(y.Cast<object>()).All(t => Equals(t.First, t.Second));

        return Equals(a, b);
    }

    private static JsonNode ValueToJson(object v) => v switch
    {
        null => null,
        Vector3 vec => new JsonArray(vec.X, vec.Y, vec.Z),
        Vector2 v2 => new JsonArray(v2.X, v2.Y),
        Enum e => Convert.ToInt64(e, CultureInfo.InvariantCulture),
        System.Drawing.Color c => new JsonArray(c.R, c.G, c.B, c.A),
        Array a => new JsonArray(a.Cast<object>().Select(ValueToJson).ToArray()),
        bool b => b,
        string s => s,
        float f => f,
        double d => d,
        _ => JsonValue.Create(Convert.ToDouble(v, CultureInfo.InvariantCulture))
    };

    private static object JsonToValue(JsonNode node, Type t, string field)
    {
        try
        {
            if (t == typeof(Vector3))
            {
                var a = node is JsonArray arr
                    ? arr.Select(e => ParseFloat(e, field)).ToArray()
                    : node.GetValue<string>().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray();
                return new Vector3(a[0], a[1], a[2]);
            }

            if (t.IsEnum)
            {
                var v = (JsonValue)node;
                return v.TryGetValue<string>(out var s) && !long.TryParse(s, out _)
                    ? Enum.Parse(t, s, true)
                    : Enum.ToObject(t, long.Parse(v.ToString(), CultureInfo.InvariantCulture));
            }

            if (t == typeof(string))
                return node?.GetValue<string>();

            if (t == typeof(bool))
                return node is JsonValue bv && bv.TryGetValue<bool>(out var b) ? b : node.ToString() is "1" or "true";

            return Convert.ChangeType(node.ToString().Trim('"'), t, CultureInfo.InvariantCulture);
        }
        catch (Exception e) when (e is not RemoteException)
        {
            throw new RemoteException($"'{field}': {node?.ToJsonString()} does not fit {t.Name}");
        }
    }

    /// <summary>
    ///     Changes for fields {"path": value}: "EntityID", "Position.Y", "GparamConfigStruct.LightSetID",
    ///     "DrawGroups[2]" or whole arrays
    /// </summary>
    private static List<ViewportAction> FieldActions(MsbEntity ent, JsonObject fields)
    {
        var actions = new List<ViewportAction>();

        foreach (var (path, node) in fields)
        {
            var parts = path.Split('.');
            object obj = ent.WrappedObject;

            for (var i = 0; i < parts.Length; i++)
            {
                var name = parts[i];
                var index = -1;
                var br = name.IndexOf('[');

                if (br > 0)
                {
                    index = int.Parse(name[(br + 1)..name.IndexOf(']')], CultureInfo.InvariantCulture);
                    name = name[..br];
                }

                var prop = obj.GetType().GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
                           ?? throw new RemoteException($"{obj.GetType().Name} has no field '{name}' ({path})");
                var last = i == parts.Length - 1;

                // Vector3 component: the whole vector is set
                if (prop.PropertyType == typeof(Vector3) && i == parts.Length - 2 && parts[^1] is "X" or "Y" or "Z")
                {
                    var v = (Vector3)prop.GetValue(obj);
                    var f = ParseFloat(node, path);
                    v = parts[^1] == "X" ? v with { X = f } : parts[^1] == "Y" ? v with { Y = f } : v with { Z = f };
                    actions.Add(Changed(ent, prop, obj, v));
                    break;
                }

                if (!last)
                {
                    obj = prop.GetValue(obj) ?? throw new RemoteException($"{path}: '{name}' is null");
                    continue;
                }

                if (prop.PropertyType.IsArray)
                {
                    var et = prop.PropertyType.GetElementType();
                    var arr = (Array)prop.GetValue(obj);

                    if (index >= 0)
                    {
                        actions.Add(Changed(ent, prop, obj, JsonToValue(node, et, path), index));
                    }
                    else
                    {
                        var vals = node as JsonArray ?? throw new RemoteException($"{path} needs an array");
                        if (vals.Count != arr.Length)
                            throw new RemoteException($"{path} needs {arr.Length} values");
                        for (var k = 0; k < vals.Count; k++)
                            actions.Add(Changed(ent, prop, obj, JsonToValue(vals[k], et, path), k));
                    }
                }
                else
                {
                    if (!prop.CanWrite)
                        throw new RemoteException($"{path} is read-only");
                    actions.Add(Changed(ent, prop, obj, JsonToValue(node, prop.PropertyType, path)));
                }
            }
        }

        return actions;
    }

    private static PropChangeAction Changed(MsbEntity ent, PropertyInfo prop, object obj, object value, int index = -1)
    {
        var act = index >= 0 ? new PropChangeAction(ent, prop, index, obj, value) : new PropChangeAction(ent, prop, obj, value);
        act.SetPostExecutionAction(_ => ent.UpdateRenderModel());
        return act;
    }

    private static JsonObject GetFieldsArg(JsonObject args)
    {
        if (args["fields"] is JsonObject o)
            return o;

        if (GetString(args, "fields") is string text)
            return JsonNode.Parse(text) as JsonObject ?? throw new RemoteException("fields must be a JSON object");

        if (GetString(args, "field") is string field)
            return new JsonObject { [field] = args["value"]?.DeepClone() ?? throw new RemoteException("field needs value=") };

        throw new RemoteException("Give fields={...} or field=<name> value=<value>");
    }

    #endregion

    #region set, region-add/-move/-delete

    private static IEnumerator SetFieldsCommand(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var ent = RequireEntry(target, RequireString(args, "name"), GetString(args, "list"));
        var fields = GetFieldsArg(args);
        var actions = FieldActions(ent, fields);

        var step = new RemoteStep($"set {ent.Name}", st =>
        {
            foreach (var a in actions)
                st.Run(a);
            st.Records.Add((new JsonObject { ["op"] = "set", ["name"] = ent.Name, ["map"] = target.Name, ["fields"] = fields.DeepClone() },
                null));
        });

        ExecuteStep(view, step, false);
        yield return null;

        result["name"] = ent.Name;
        result["list"] = ListOf(ent.WrappedObject);
        result["fields"] = new JsonObject(fields.Select(f => KeyValuePair.Create(f.Key,
            ReadField(ent.WrappedObject, f.Key) is var v ? ValueToJson(v) : null)));
        result["records"] = RecordsJson(step);
    }

    private static object ReadField(object o, string path)
    {
        foreach (var raw in path.Split('.'))
        {
            var name = raw;
            var index = -1;
            if (name.Contains('['))
            {
                index = int.Parse(name[(name.IndexOf('[') + 1)..name.IndexOf(']')], CultureInfo.InvariantCulture);
                name = name[..name.IndexOf('[')];
            }

            if (o is Vector3 v && name is "X" or "Y" or "Z")
                return name == "X" ? v.X : name == "Y" ? v.Y : v.Z;

            o = o?.GetType().GetProperty(name)?.GetValue(o);
            if (index >= 0 && o is Array a)
                o = a.GetValue(index);
        }

        return o;
    }

    private static IEnumerator RegionAdd(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var (sourceMap, sourceName) = SplitFrom(RequireString(args, "from"));
        var name = GetString(args, "name");

        if (name != null && target.GetObjectByName(name) != null)
            throw new RemoteException($"{target.Name} already has an object named '{name}'");

        var loaded = new bool[1];
        var load = EnsureMapLoaded(view, sourceMap, args, loaded);
        while (load.MoveNext())
            yield return null;

        try
        {
            var source = RequireEntry(RequireLoadedMap(view, sourceMap), sourceName, "Regions");
            var pos = new[] { GetVector(args, "pos") ?? GetPartVector(source, "Position") };
            var snap = SnapToFloor(view, args, pos);
            while (snap.MoveNext())
                yield return null;

            var rot = GetVector(args, "rot") ?? GetPartVector(source, "Rotation");
            MsbEntity clone = null;

            var step = new RemoteStep($"region-add {sourceName} from {sourceMap}", st =>
            {
                clone = DuplicateInto(st, view, source, target, GetBool(args, "keepEntityId", false), name);
                st.Run(PartChange(clone, pos[0], rot, null));
                st.Records.Add((new JsonObject
                {
                    ["op"] = "region-add", ["name"] = clone.Name, ["map"] = target.Name, ["src_map"] = sourceMap,
                    ["src_region"] = sourceName, ["pos"] = ToJson(pos[0]), ["rot"] = ToJson(rot),
                }, new JsonObject { ["op"] = "region-delete", ["name"] = clone.Name, ["map"] = target.Name }));
            });

            ExecuteStep(view, step, false);
            yield return null;

            result["region"] = DescribeRegion(clone);
            result["records"] = RecordsJson(step);
        }
        finally
        {
            if (loaded[0] && !GetBool(args, "keepSource", false))
                view.Universe.UnloadMap(sourceMap);
        }
    }

    private static JsonObject DescribeRegion(MsbEntity ent) => new()
    {
        ["map"] = ent.MapID,
        ["name"] = ent.Name,
        ["type"] = ent.WrappedObject.GetType().Name,
        ["pos"] = ToJson(GetPartVector(ent, "Position")),
        ["rot"] = ToJson(GetPartVector(ent, "Rotation")),
    };

    private static IEnumerator RegionMove(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var ent = RequireEntry(target, RequireString(args, "name"), "Regions");
        var oldPos = GetPartVector(ent, "Position");
        var oldRot = GetPartVector(ent, "Rotation");
        var local = GetBool(args, "local", false);

        var rot = GetVector(args, "rot") ?? oldRot;
        if (GetVector(args, "drot") is Vector3 drot)
        {
            var q = PartQuat(ent, rot);
            var dq = Transform.EulerToQuaternion(drot * Deg);
            rot = PartRotation(ent, local ? Quaternion.Concatenate(dq, q) : Quaternion.Concatenate(q, dq));
        }

        var pos = new[] { GetVector(args, "pos") ?? oldPos };
        if (GetVector(args, "dpos") is Vector3 dpos)
            pos[0] += local ? Vector3.Transform(dpos, PartQuat(ent, oldRot)) : dpos;

        var snap = SnapToFloor(view, args, pos);
        while (snap.MoveNext())
            yield return null;

        var step = new RemoteStep($"region-move {ent.Name}", st =>
        {
            st.Run(PartChange(ent, pos[0], rot, null));
            st.Records.Add((new JsonObject
            {
                ["op"] = "region-move", ["name"] = ent.Name, ["map"] = target.Name, ["pos"] = ToJson(pos[0]), ["rot"] = ToJson(rot),
            }, new JsonObject
            {
                ["op"] = "region-move", ["name"] = ent.Name, ["map"] = target.Name, ["pos"] = ToJson(oldPos), ["rot"] = ToJson(oldRot),
            }));
        });

        ExecuteStep(view, step, false);
        yield return null;

        result["region"] = DescribeRegion(ent);
        result["records"] = RecordsJson(step);
    }

    private static IEnumerator RegionDelete(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var ent = RequireEntry(target, RequireString(args, "name"), "Regions");
        var name = ent.Name;

        var step = new RemoteStep($"region-delete {name}", st =>
        {
            st.Run(new EntDeleteAction(view, new List<MsbEntity> { ent }, false));
            st.Records.Add((new JsonObject { ["op"] = "region-delete", ["name"] = name, ["map"] = target.Name }, null));
        });

        ExecuteStep(view, step, false);
        yield return null;

        result["deleted"] = name;
        result["records"] = RecordsJson(step);
    }

    #endregion

    #region ops-mark, ops-export, ops-apply

    private static readonly Dictionary<string, Dictionary<string, (string List, object Entry)>> Marks = new();

    private static object DeepCopyEntry(object entry) =>
        entry.GetType().GetMethod("DeepCopy", Type.EmptyTypes)?.Invoke(entry, null) ?? entry;

    private static string MsbFile(ProjectEntry project, string map, out bool inProject)
    {
        var rel = Path.Combine("map", "mapstudio", map + ".msb.dcx");
        var proj = Path.Combine(project.Descriptor.ProjectPath, rel);
        inProject = File.Exists(proj);
        return inProject ? proj : Path.Combine(project.Descriptor.DataPath, rel);
    }

    private static MSBB ReadMsb(ProjectEntry project, string map)
    {
        var bytes = project.VFS.FS.ReadFile($"/map/mapstudio/{map}.msb.dcx")
                    ?? throw new RemoteException($"No MSB for {map}");
        return MSBB.Read(bytes);
    }

    private static Dictionary<string, (string List, object Entry)> MsbEntries(MSBB msb)
    {
        var d = new Dictionary<string, (string, object)>();
        foreach (var e in msb.Parts.GetEntries()) d[e.Name] = ("Parts", e);
        foreach (var e in msb.Regions.GetEntries()) d.TryAdd(e.Name, ("Regions", e));
        foreach (var e in msb.Events.GetEntries()) d.TryAdd(e.Name, ("Events", e));
        return d;
    }

    private static IEnumerator OpsMark(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        Marks[target.Name] = MapEntries(target).ToDictionary(e => e.Name, e => (ListOf(e.WrappedObject), DeepCopyEntry(e.WrappedObject)));
        result["map"] = target.Name;
        result["entries"] = Marks[target.Name].Count;
        yield break;
    }

    private static JsonObject FieldDiff(object reference, object current)
    {
        var refFields = Fields(reference).ToDictionary(f => f.Path, f => f.Value);
        var set = new JsonObject();

        foreach (var (path, value) in Fields(current))
        {
            if (!refFields.TryGetValue(path, out var old) || !SameValue(old, value))
                set[path] = ValueToJson(value);
        }

        return set;
    }

    /// <summary>
    ///     ops-export: changes of the loaded map against the MSB on disk (project before game) or against the
    ///     mark (since=mark) as msbtool ops (delete, copy with set, set)
    /// </summary>
    private static IEnumerator OpsExport(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var project = view.Editor.Project;
        var target = GetTargetMap(view, args);
        var since = GetString(args, "since", "disk");
        // wsl=true: Windows paths (C:\...) as /mnt/c/... for msbtool under WSL (callers that convert paths themselves leave it off)
        var wsl = GetBool(args, "wsl", false);
        string P(string path) => wsl ? WslPath(path) : path;
        var file = MsbFile(project, target.Name, out _);

        var reference = since == "mark"
            ? Marks.GetValueOrDefault(target.Name) ?? throw new RemoteException("No mark for this map, see /ops-mark")
            : MsbEntries(ReadMsb(project, target.Name));

        var current = MapEntries(target).ToList();
        var names = current.Select(e => e.Name).ToHashSet();
        var ops = new JsonArray();
        var warnings = new JsonArray();
        var sources = new Dictionary<string, Dictionary<string, (string, object)>>();

        foreach (var (name, (list, _)) in reference.Where(r => !names.Contains(r.Key)).OrderBy(r => r.Key))
            ops.Add(new JsonObject { ["op"] = "delete", ["list"] = list, ["name"] = name });

        foreach (var ent in current)
        {
            var list = ListOf(ent.WrappedObject);

            if (reference.TryGetValue(ent.Name, out var r) && r.List == list)
            {
                var set = FieldDiff(r.Entry, ent.WrappedObject);
                if (set.Count > 0)
                    ops.Add(new JsonObject { ["op"] = "set", ["list"] = list, ["name"] = ent.Name, ["fields"] = set });
                continue;
            }

            // New: copied from where it came from (its origin, else an entry of this map with the same model)
            string fromFile = null, fromName = null;
            object fromEntry = null;

            if (Origins.TryGetValue(ent, out var origin))
            {
                if (!sources.TryGetValue(origin[0], out var src))
                    sources[origin[0]] = src = MsbEntries(ReadMsb(project, origin[0]));
                if (src.TryGetValue(origin[1], out var se))
                {
                    fromFile = P(MsbFile(project, origin[0], out _));
                    fromName = origin[1];
                    fromEntry = se.Item2;
                }
            }

            if (fromEntry == null)
            {
                var model = (ent.WrappedObject as IMsbPart)?.ModelName;
                // Regions only from one of the same shape (msbtool can't change a shape's type)
                var shape = ent.WrappedObject.GetType().GetProperty("Shape")?.GetValue(ent.WrappedObject)?.GetType();
                var same = reference.FirstOrDefault(x => x.Value.List == list && x.Value.Entry.GetType() == ent.WrappedObject.GetType() &&
                                                        (model == null || (x.Value.Entry as IMsbPart)?.ModelName == model) &&
                                                        (shape == null || x.Value.Entry.GetType().GetProperty("Shape")?.GetValue(x.Value.Entry)?.GetType() == shape));
                if (same.Key != null && since != "mark")
                {
                    fromFile = P(file);
                    fromName = same.Key;
                    fromEntry = same.Value.Entry;
                }
            }

            if (fromEntry == null)
            {
                warnings.Add($"{ent.Name}: no source entry for copy (origin unknown)");
                continue;
            }

            ops.Add(new JsonObject
            {
                ["op"] = "copy", ["from"] = fromFile, ["list"] = list, ["name"] = fromName, ["newName"] = ent.Name,
                ["set"] = FieldDiff(fromEntry, ent.WrappedObject),
            });
        }

        result["map"] = target.Name;
        result["reference"] = since == "mark" ? "mark" : P(file);
        result["ops"] = ops;
        result["warnings"] = warnings;

        if (GetString(args, "path") is string path)
        {
            File.WriteAllText(path, new JsonObject { ["ops"] = ops.DeepClone() }.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            result["path"] = path;
        }

        yield break;
    }

    /// <summary>X:\a\b as /mnt/x/a/b (WSL); other paths unchanged</summary>
    private static string WslPath(string path) =>
        path != null && path.Length > 2 && char.IsLetter(path[0]) && path[1] == ':' && (path[2] == '\\' || path[2] == '/')
            ? "/mnt/" + char.ToLowerInvariant(path[0]) + path[2..].Replace('\\', '/')
            : path;

    /// <summary>ops-apply: apply msbtool ops to the loaded map, as one undo step</summary>
    private static IEnumerator OpsApply(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        JsonArray ops;

        if (args["ops"] is JsonArray inline)
            ops = inline;
        else if (GetString(args, "ops") is string text)
            ops = (JsonNode.Parse(text) as JsonObject)?["ops"] as JsonArray ?? JsonNode.Parse(text) as JsonArray;
        else
            ops = JsonNode.Parse(File.ReadAllText(RequireString(args, "path")))?["ops"] as JsonArray;

        if (ops == null)
            throw new RemoteException("No ops (path=<edits.json> or ops=[...])");

        // Source maps of the copies first
        var loadedHere = new List<string>();
        foreach (var op in ops.OfType<JsonObject>().Where(o => GetString(o, "op") == "copy"))
        {
            var map = Path.GetFileName(GetString(op, "from") ?? "").Split('.')[0];
            if (map == "" || map == target.Name)
                continue;

            var loaded = new bool[1];
            var load = EnsureMapLoaded(view, map, args, loaded);
            while (load.MoveNext())
                yield return null;
            if (loaded[0])
                loadedHere.Add(map);
        }

        var skipped = new JsonArray();

        try
        {
            IEnumerable<MsbEntity> Select(MapContainer c, JsonObject op)
            {
                var list = GetString(op, "list");
                var name = GetString(op, "name");
                var type = GetString(op, "type");
                if (name == null && type == null)
                    throw new RemoteException("op needs name and/or type");
                return MapEntries(c).Where(e => (list == null || ListOf(e.WrappedObject) == list) &&
                                                (name == null || e.Name == name) &&
                                                (type == null || e.WrappedObject.GetType().Name == type)).ToList();
            }

            var step = new RemoteStep($"ops-apply {ops.Count} ops", st =>
            {
                foreach (var op in ops.OfType<JsonObject>())
                {
                    switch (GetString(op, "op"))
                    {
                        case "delete":
                        {
                            var hits = Select(target, op).ToList();
                            if (hits.Count > 0)
                                st.Run(new EntDeleteAction(view, hits, false));
                            foreach (var h in hits)
                                st.Records.Add((new JsonObject { ["op"] = "delete", ["name"] = h.Name, ["map"] = target.Name }, null));
                            break;
                        }
                        case "copy":
                        {
                            var map = Path.GetFileName(GetString(op, "from") ?? "").Split('.')[0];
                            var source = RequireLoadedMap(view, map == "" ? target.Name : map);
                            var hits = Select(source, op).ToList();
                            if (hits.Count == 0)
                                throw new RemoteException($"copy: nothing matched in {source.Name}");
                            foreach (var h in hits)
                            {
                                var clone = DuplicateInto(st, view, h, target, true, GetString(op, "newName") ?? h.Name);
                                if (op["set"] is JsonObject set)
                                    foreach (var a in FieldActions(clone, set))
                                        st.Run(a);
                                if (clone.WrappedObject is IMsbPart)
                                    RecordAdd(st, clone, target.Name, source.Name, h.Name, true, false);
                            }
                            break;
                        }
                        case "set":
                        {
                            var hits = Select(target, op).ToList();
                            if (hits.Count == 0)
                                throw new RemoteException($"set: nothing matched ({op.ToJsonString()})");
                            foreach (var h in hits)
                            {
                                foreach (var a in FieldActions(h, op["fields"] as JsonObject ?? new JsonObject()))
                                    st.Run(a);
                                st.Records.Add((new JsonObject
                                {
                                    ["op"] = "set", ["name"] = h.Name, ["map"] = target.Name, ["fields"] = op["fields"]?.DeepClone()
                                }, null));
                            }
                            break;
                        }
                        default:
                            skipped.Add(op.ToJsonString());
                            break;
                    }
                }
            });

            ExecuteStep(view, step, GetBool(args, "addAtlas", false));
            yield return null;

            result["applied"] = ops.Count - skipped.Count;
            result["skipped"] = skipped;
            result["records"] = RecordsJson(step);
        }
        finally
        {
            if (!GetBool(args, "keepSource", false))
                foreach (var map in loadedHere)
                    view.Universe.UnloadMap(map);
        }
    }

    #endregion

    #region Settings for a while

    private static object SetConfig(string name, object value)
    {
        var member = FindConfigMember(name);
        var old = GetMember(member);

        if (member is FieldInfo f)
            f.SetValue(CFG.Current, value);
        else
            ((PropertyInfo)member).SetValue(CFG.Current, value);

        return old;
    }

    /// <summary>Gap mode: flat, unlit, magenta background; returns what has to be restored</summary>
    private static List<(string, object)> GapMode(VulkanViewport viewport, out RenderFilter savedFilter)
    {
        var saved = new List<(string, object)>
        {
            ("Viewport_Render_Mode", SetConfig("Viewport_Render_Mode", 0)),
            ("Viewport_Bg_Color", SetConfig("Viewport_Bg_Color", new Vector3(1, 0, 1))),
            ("Viewport_Map_Lighting_Background", SetConfig("Viewport_Map_Lighting_Background", false)),
        };

        savedFilter = viewport.RenderScene.DrawFilter;
        viewport.RenderScene.DrawFilter &= ~ParseFilters("Collision,Region,Light,Debug,Navmesh,FogVolume,MapLimit,MultiPlayerWall,DummyEnemy,DummyAsset");
        return saved;
    }

    private static void Restore(List<(string, object)> saved)
    {
        foreach (var (name, value) in saved)
            SetConfig(name, value);
    }

    private static int CountMagenta(string path)
    {
        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
        var n = 0;
        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height; y++)
                foreach (var p in rows.GetRowSpan(y))
                    if (p.R == 255 && p.G == 0 && p.B == 255)
                        n++;
        });
        return n;
    }

    private static void SetCamera(VulkanViewport viewport, Vector3 pos, Vector3 dir)
    {
        var camera = viewport.ViewportCamera;
        camera.CameraTransform.Position = pos;
        var d = Vector3.Normalize(dir);
        camera.CameraTransform.EulerRotation = new Vector3(-MathF.Asin(Math.Clamp(d.Y, -1f, 1f)), MathF.Atan2(d.X, d.Z), 0);
        if (viewport.Width > 0 && viewport.Height > 0)
            camera.UpdateProjectionMatrix();
    }

    #endregion

    #region leak-scan

    /// <summary>View points from navmesh faces: eye height, 6 directions, 2 views at the floor, stairs every 1.5 m</summary>
    private static List<(Vector3 Pos, Vector3 Dir, string Tag)> ViewsFromFaces(string facesPath, float spacing, float eye)
    {
        var pts = new List<Vector3>();
        var slopes = new List<float?>();

        foreach (var line in File.ReadLines(facesPath))
        {
            var a = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (a.Length < 10)
                continue;

            var v = a.Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            var verts = Enumerable.Range(0, v.Length / 3).Select(i => new Vector3(v[3 * i], v[3 * i + 1], v[3 * i + 2])).ToArray();
            var c = verts.Aggregate(Vector3.Zero, (s, x) => s + x) / verts.Length;
            var n = Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]);
            n = n.LengthSquared() > 0 ? Vector3.Normalize(n) : Vector3.UnitY;
            var steep = MathF.Abs(n.Y) < MathF.Cos(12 * Deg);
            var d = steep ? 1.5f : spacing;

            if (pts.All(p => Vector3.Distance(p, c) >= d))
            {
                pts.Add(c);
                var g = n.Y < 0 ? -n : n;
                slopes.Add(steep ? MathF.Atan2(-g.X, -g.Z) / Deg : null);
            }
        }

        var views = new List<(Vector3, Vector3, string)>();

        for (var i = 0; i < pts.Count; i++)
        {
            var e = pts[i] + new Vector3(0, eye, 0);
            var list = Enumerable.Range(0, 6).Select(k => (60f * k, 2f)).ToList();
            list.AddRange(slopes[i] is float s ? new[] { (s, -6f), (s + 180, -8f) } : new[] { (0f, -7f), (180f, -7f) });

            foreach (var (deg, dy) in list)
            {
                var t = deg * Deg;
                views.Add((e, new Vector3(10 * MathF.Sin(t), dy, 10 * MathF.Cos(t)), $"p{i:D3}_r{(((int)deg % 360) + 360) % 360:D3}{(dy < 0 ? "u" : "")}"));
            }
        }

        return views;
    }

    private static IEnumerator LeakScan(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var viewport = GetViewport(view);
        var dir = RequireString(args, "dir");
        var threshold = (int)GetFloat(args, "threshold", 30);
        var settle = (int)GetFloat(args, "settle", 4);
        var views = new List<(Vector3 Pos, Vector3 Dir, string Tag)>();

        if (args["points"] is JsonArray || GetString(args, "points") is not null)
        {
            var arr = args["points"] as JsonArray ?? JsonNode.Parse(GetString(args, "points")) as JsonArray;
            var k = 0;
            foreach (var p in arr!.OfType<JsonObject>())
            {
                var pos = GetVector(p, "pos") ?? throw new RemoteException("point needs pos");
                var d = GetVector(p, "dir") ?? (GetVector(p, "lookat") is Vector3 l ? l - pos : throw new RemoteException("point needs dir or lookat"));
                views.Add((pos, d, $"v{k++:D3}"));
            }
        }
        else
        {
            views = ViewsFromFaces(RequireString(args, "faces"), GetFloat(args, "spacing", 4), GetFloat(args, "eye", 1.6f));

            // Does the faces file fit the loaded map? Share of faces with no collision just below them
            var collision = EnsureCollision(view, 300);
            while (collision.MoveNext())
                yield return null;

            var set = view.FirstPersonCollision.Current;
            var centers = File.ReadLines(RequireString(args, "faces"))
                .Select(l => l.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)).Where(a => a.Length >= 10)
                .Select(a => a.Skip(1).Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToArray())
                .Select(v => Enumerable.Range(0, v.Length / 3).Aggregate(Vector3.Zero, (c, i) => c + new Vector3(v[3 * i], v[3 * i + 1], v[3 * i + 2])) / (v.Length / 3))
                .ToList();
            var sample = centers.Where((_, i) => i % Math.Max(1, centers.Count / 300) == 0).ToList();
            var noFloor = sample.Count(c => !set.RayCast(c + new Vector3(0, 0.5f, 0), -Vector3.UnitY, 1.5f, out _));
            var share = sample.Count > 0 ? (double)noFloor / sample.Count : 0;
            result["facesCheck"] = new JsonObject
            {
                ["faces"] = centers.Count, ["sampled"] = sample.Count, ["noFloor"] = noFloor, ["share"] = Math.Round(share, 3),
            };
            if (share > GetFloat(args, "maxNoFloor", 0.2f))
                result["warning"] = $"{Math.Round(share * 100)} % of the checked faces have no collision just below them: " +
                                    "does the faces file match the loaded map (map, version, offset)?";
            if (share >= GetFloat(args, "abortNoFloor", 0.8f) && !GetBool(args, "force", false))
                throw new RemoteException($"leak-scan aborted: {Math.Round(share * 100)} % of the faces have no collision below them, " +
                                          "the faces file does not match the loaded map (force=true scans anyway)");
        }

        var max = (int)GetFloat(args, "maxViews", 400);
        if (views.Count > max)
            throw new RemoteException($"{views.Count} views, more than maxViews={max} (split the faces or raise it)");

        Directory.CreateDirectory(dir);
        var camera = viewport.ViewportCamera;
        var savedPos = camera.CameraTransform.Position;
        var savedRot = camera.CameraTransform.EulerRotation;
        var savedFov = SetConfig("Viewport_Camera_FOV", GetFloat(args, "fov", 90));
        var saved = GapMode(viewport, out var savedFilter);
        var counts = new JsonArray();
        var over = new JsonArray();

        try
        {
            foreach (var (pos, d, tag) in views)
            {
                SetCamera(viewport, pos, d);
                var file = Path.Combine(dir, $"leak_{tag}.png");
                var capture = CaptureTo(view, viewport, file, args, settle, new int[2]);
                while (capture.MoveNext())
                    yield return null;

                var m = CountMagenta(file);
                counts.Add(m);

                if (m > threshold)
                {
                    over.Add(new JsonObject { ["view"] = tag, ["pos"] = ToJson(pos), ["dir"] = ToJson(Vector3.Normalize(d)), ["magenta"] = m, ["path"] = file });
                }
                else if (!GetBool(args, "keep", false))
                {
                    File.Delete(file);
                }
            }
        }
        finally
        {
            Restore(saved);
            SetConfig("Viewport_Camera_FOV", savedFov);
            viewport.RenderScene.DrawFilter = savedFilter;
            camera.CameraTransform.Position = savedPos;
            camera.CameraTransform.EulerRotation = savedRot;
        }

        result["views"] = views.Count;
        result["counts"] = counts;
        result["threshold"] = threshold;
        result["over"] = over;
    }

    #endregion

    #region plan

    private static (float A, float B) Get2(JsonObject args, string name)
    {
        var s = GetString(args, name) ?? throw new RemoteException($"Missing argument '{name}'");
        var v = s.Trim('[', ']').Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => float.Parse(x, CultureInfo.InvariantCulture)).ToArray();
        return v.Length == 2 ? (v[0], v[1]) : v.Length == 3 ? (v[0], v[2]) : throw new RemoteException($"'{name}' must be x,z");
    }

    private static IEnumerator Plan(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var viewport = GetViewport(view);
        var path = RequireString(args, "path");
        var (x0, z0) = Get2(args, "min");
        var (x1, z1) = Get2(args, "max");
        var cut = GetFloat(args, "cut", float.NaN);
        if (float.IsNaN(cut))
            throw new RemoteException("Missing argument 'cut' (height of the cut, everything above is left out)");
        var depth = GetFloat(args, "depth", 30);

        view.Editor.Project.Handler.FocusEditorWindow(view.Editor);
        var focus = WaitFrames(5);
        while (focus.MoveNext())
            yield return null;

        var camera = viewport.ViewportCamera;
        var savedPos = camera.CameraTransform.Position;
        var savedRot = camera.CameraTransform.EulerRotation;
        var savedMode = camera.ViewMode;
        var savedSize = camera.OrthographicSize;
        var savedFilter = viewport.RenderScene.DrawFilter;
        var saved = new List<(string, object)>
        {
            ("Viewport_Orthographic_Near_Clip", SetConfig("Viewport_Orthographic_Near_Clip", 0f)),
            ("Viewport_Orthographic_Far_Clip", SetConfig("Viewport_Orthographic_Far_Clip", depth)),
        };

        if (GetBool(args, "gap", true))
            saved.AddRange(GapMode(viewport, out _));

        if (GetBool(args, "collision", false))
            viewport.RenderScene.DrawFilter |= RenderFilter.Collision;

        var cx = (x0 + x1) / 2;
        var cz = (z0 + z1) / 2;
        var w = viewport.Width;
        var h = viewport.Height;
        var size = MathF.Max(MathF.Abs(z1 - z0), MathF.Abs(x1 - x0) * h / w) * 1.02f;
        var mpp = size / h;

        try
        {
            camera.SetProjectionType(ViewMode.Orthographic);
            camera.SetOrthographicSize(size);
            camera.CameraTransform.Position = new Vector3(cx, cut, cz);
            camera.CameraTransform.EulerRotation = new Vector3(89.99f * Deg, 0, 0);
            camera.UpdateProjectionMatrix();

            var sizeOut = new int[2];
            var capture = CaptureTo(view, viewport, path, args, (int)GetFloat(args, "settle", 4), sizeOut);
            while (capture.MoveNext())
                yield return null;

            if (GetBool(args, "labels", false))
            {
                var labels = DrawLabels(view, viewport, args, path, result);
                while (labels.MoveNext())
                    yield return null;
            }

            // Meter grid ticks and a scale bar
            var grid = GetFloat(args, "grid", 1);
            using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path))
            {
                var iw = image.Width;
                var ih = image.Height;
                var s = (float)ih / h;
                var white = new Rgba32(255, 255, 255);
                int Px(float x) => (int)MathF.Round(((x - cx) / mpp + w / 2f) * s);
                int Py(float z) => (int)MathF.Round(((cz - z) / mpp + h / 2f) * s);

                for (var gx = MathF.Ceiling(MathF.Min(x0, x1) / grid) * grid; gx <= MathF.Max(x0, x1); gx += grid)
                {
                    var major = MathF.Abs(gx / (5 * grid) - MathF.Round(gx / (5 * grid))) < 1e-3f;
                    ViewportLabels.Line(image, Px(gx), 0, Px(gx), major ? 12 : 6, white);
                    if (major)
                        ViewportLabels.Text(image, new ViewportLabels.Box(Px(gx) + 2, 12, ($"x{gx:0.#}".Length) * 7 + 4, 17),
                            $"x{gx.ToString("0.#", CultureInfo.InvariantCulture)}", white, new Rgba32(0, 0, 0), 0.6f, 1);
                }

                for (var gz = MathF.Ceiling(MathF.Min(z0, z1) / grid) * grid; gz <= MathF.Max(z0, z1); gz += grid)
                {
                    var major = MathF.Abs(gz / (5 * grid) - MathF.Round(gz / (5 * grid))) < 1e-3f;
                    ViewportLabels.Line(image, 0, Py(gz), major ? 12 : 6, Py(gz), white);
                    if (major)
                        ViewportLabels.Text(image, new ViewportLabels.Box(14, Py(gz) - 8, ($"z{gz:0.#}".Length) * 7 + 4, 17),
                            $"z{gz.ToString("0.#", CultureInfo.InvariantCulture)}", white, new Rgba32(0, 0, 0), 0.6f, 1);
                }

                var bar = GetFloat(args, "bar", 5);
                var bx = 20;
                var by = ih - 30;
                var bl = (int)MathF.Round(bar / mpp * s);
                for (var t = -1; t <= 1; t++)
                    ViewportLabels.Line(image, bx, by + t, bx + bl, by + t, white);
                ViewportLabels.Text(image, new ViewportLabels.Box(bx, by - 20, 60, 17), $"{bar.ToString("0.#", CultureInfo.InvariantCulture)} m",
                    white, new Rgba32(0, 0, 0), 0.6f, 1);
                image.SaveAsPng(path);

                result["metersPerPixel"] = Round(mpp / s);
                result["origin"] = new JsonArray(Round(cx - w / 2f * mpp), Round(cz + h / 2f * mpp));
                result["note"] = "Pixel (px, py) = world x = origin[0] + px * metersPerPixel, z = origin[1] - py * metersPerPixel";
            }

            result["path"] = path;
            result["size"] = new JsonArray(sizeOut[0], sizeOut[1]);
        }
        finally
        {
            Restore(saved);
            viewport.RenderScene.DrawFilter = savedFilter;
            camera.SetProjectionType(savedMode);
            camera.SetOrthographicSize(savedSize);
            camera.CameraTransform.Position = savedPos;
            camera.CameraTransform.EulerRotation = savedRot;
            camera.UpdateProjectionMatrix();
        }
    }

    #endregion

    #region diff

    private static IEnumerator Diff(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var refPath = RequireString(args, "ref");
        var radius = GetFloat(args, "radius", 6);
        var types = new HashSet<string> { "MapPiece", "Object", "Collision", "Enemy", "DummyObject" };
        var old = MSBB.Read(new Memory<byte>(File.ReadAllBytes(refPath))).Parts.GetEntries()
            .Where(p => types.Contains(p.GetType().Name)).ToDictionary(p => p.Name);
        var cur = target.Objects.OfType<MsbEntity>().Where(e => e.WrappedObject is IMsbPart && types.Contains(PartTypeName(e)))
            .ToDictionary(e => e.Name);

        static bool Near(Vector3 a, Vector3 b) => Vector3.Distance(a, b) < 1e-3f;
        var changed = new JsonArray();
        var points = new List<Vector3>();

        foreach (var name in old.Keys.Union(cur.Keys).OrderBy(n => n))
        {
            old.TryGetValue(name, out var a);
            cur.TryGetValue(name, out var b);
            var bp = b?.WrappedObject as MSBB.Part;

            if (a != null && b != null && a.ModelName == ((IMsbPart)b.WrappedObject).ModelName &&
                Near(a.Position, GetPartVector(b, "Position")) && Near(a.Rotation, GetPartVector(b, "Rotation")) &&
                Near(a.Scale, PartScale(b)))
                continue;

            var kind = a == null ? "new" : b == null ? "deleted" : "changed";
            var type = a?.GetType().Name ?? PartTypeName(b);
            changed.Add(new JsonObject
            {
                ["name"] = name, ["kind"] = kind, ["type"] = type,
                ["old"] = a != null ? ToJson(a.Position) : null,
                ["new"] = b != null ? ToJson(GetPartVector(b, "Position")) : null,
            });

            if (type != "Collision")
            {
                if (a != null) points.Add(a.Position);
                if (b != null) points.Add(GetPartVector(b, "Position"));
            }
        }

        result["changed"] = changed;
        result["points"] = new JsonArray(points.Select(p => (JsonNode)ToJson(p)).ToArray());

        if (GetString(args, "faces") is string faces)
        {
            var outPath = RequireString(args, "out");
            int nIn = 0, nOut = 0;
            using (var w = new StreamWriter(outPath))
            {
                foreach (var line in File.ReadLines(faces))
                {
                    var v = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                    if (v.Length < 10)
                        continue;
                    nIn++;
                    var xyz = v.Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray();
                    var n = xyz.Length / 3;
                    var c = new Vector3(Enumerable.Range(0, n).Average(i => xyz[3 * i]), Enumerable.Range(0, n).Average(i => xyz[3 * i + 1]),
                        Enumerable.Range(0, n).Average(i => xyz[3 * i + 2]));
                    if (points.Any(p => MathF.Sqrt((c.X - p.X) * (c.X - p.X) + (c.Z - p.Z) * (c.Z - p.Z)) <= radius && MathF.Abs(c.Y - p.Y) <= 4))
                    {
                        w.WriteLine(line);
                        nOut++;
                    }
                }
            }

            result["faces"] = new JsonObject { ["in"] = nIn, ["out"] = nOut, ["path"] = outPath };
        }

        yield break;
    }

    #endregion

    #region Model files (FLVER)

    private class ModelGeo
    {
        public Vector3[] Triangles = Array.Empty<Vector3>();
        public string[] Textures = Array.Empty<string>();
        public Vector3 Min, Max;
        public bool Found;
    }

    private static readonly Dictionary<string, ModelGeo> ModelCache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The FLVER of a model: map piece from map/&lt;map&gt;/, object from obj/&lt;model&gt;.objbnd.dcx; null if not found</summary>
    private static FLVER2 ReadFlver(ProjectEntry project, string map, string model)
    {
        var fs = project.VFS.FS;

        try
        {
            if (model.StartsWith("o", StringComparison.OrdinalIgnoreCase))
            {
                var bytes = fs.ReadFile($"/obj/{model}.objbnd.dcx");
                if (bytes is Memory<byte> b)
                {
                    IEnumerable<BinderFile> files;
                    try
                    {
                        files = BND4.Read(b).Files;
                    }
                    catch (Exception)
                    {
                        files = BND3.Read(b).Files;
                    }

                    var f = files?.FirstOrDefault(x => x.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase));
                    if (f != null)
                        return FLVER2.Read(f.Bytes);
                }
            }
            else if (map != null && model.Length > 1)
            {
                var bytes = fs.ReadFile($"/map/{map}/{map}_{model.Substring(1)}.flver.dcx");
                if (bytes is Memory<byte> b)
                    return FLVER2.Read(b);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogWarning(typeof(RemoteControl), $"Remote: {model} not readable", e);
        }

        return null;
    }

    /// <summary>The triangles of all meshes (first FaceSet) in model space, three corners each</summary>
    private static List<Vector3> FlverTriangles(FLVER2 flver)
    {
        var tris = new List<Vector3>();
        foreach (var mesh in flver.Meshes)
        {
            var fset = mesh.FaceSets.FirstOrDefault();
            if (fset == null)
                continue;
            var idx = fset.Triangulate(mesh.Vertices.Count < ushort.MaxValue);
            for (var i = 0; i + 2 < idx.Count; i += 3)
            {
                if (idx[i] >= mesh.Vertices.Count || idx[i + 1] >= mesh.Vertices.Count || idx[i + 2] >= mesh.Vertices.Count)
                    continue;
                tris.Add(mesh.Vertices[idx[i]].Position);
                tris.Add(mesh.Vertices[idx[i + 1]].Position);
                tris.Add(mesh.Vertices[idx[i + 2]].Position);
            }
        }

        return tris;
    }

    private static ModelGeo LoadModel(ProjectEntry project, string map, string model, bool triangles)
    {
        var key = (model.StartsWith("o", StringComparison.OrdinalIgnoreCase) ? "" : map) + "/" + model + (triangles ? "+t" : "");
        if (ModelCache.TryGetValue(key, out var cached))
            return cached;

        var geo = new ModelGeo();
        ModelCache[key] = geo;
        var flver = ReadFlver(project, map, model);

        if (flver == null)
            return geo;

        geo.Found = true;
        geo.Min = flver.Header.BoundingBoxMin;
        geo.Max = flver.Header.BoundingBoxMax;
        geo.Textures = flver.Materials.SelectMany(m => m.Textures)
            .Where(t => !string.IsNullOrEmpty(t.Path) && !t.ParamName.Contains("DOL", StringComparison.OrdinalIgnoreCase))
            .Select(t => TextureName(t.Path)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

        if (triangles)
            geo.Triangles = FlverTriangles(flver).ToArray();

        return geo;
    }

    private static string TextureName(string path)
    {
        var name = path.Replace('\\', '/');
        name = name[(name.LastIndexOf('/') + 1)..];
        var dot = name.LastIndexOf('.');
        return (dot > 0 ? name[..dot] : name).ToLowerInvariant();
    }

    private static Matrix4x4 PartWorld(MsbEntity ent) => ent.GetLocalTransform().WorldMatrix;

    #endregion

    #region overlap

    private class PartGeo
    {
        public MsbEntity Ent;
        public Vector3[] W;
        public Vector3 Lo, Hi;
        public FirstPersonTriangleTree Tree;
    }

    private static (Vector3 Min, Vector3 Max) Region3(JsonObject args)
    {
        var a = GetVector(args, "min") ?? throw new RemoteException("Missing argument 'min' (x,y,z)");
        var b = GetVector(args, "max") ?? throw new RemoteException("Missing argument 'max' (x,y,z)");
        return (Vector3.Min(a, b), Vector3.Max(a, b));
    }

    private static bool BoxesOverlap(Vector3 aLo, Vector3 aHi, Vector3 bLo, Vector3 bHi, float pad = 0) =>
        aLo.X <= bHi.X + pad && bLo.X <= aHi.X + pad && aLo.Y <= bHi.Y + pad && bLo.Y <= aHi.Y + pad &&
        aLo.Z <= bHi.Z + pad && bLo.Z <= aHi.Z + pad;

    /// <summary>
    ///     overlap: (1) nearly coincident, equally oriented faces of different parts (z-fighting, on the FLVER
    ///     triangles), (2) small parts stuck inside each other (boxes, penetration depth)
    /// </summary>
    private static IEnumerator Overlap(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var project = view.Editor.Project;
        var target = GetTargetMap(view, args);
        var (lo, hi) = Region3(args);
        var maxd = GetFloat(args, "maxdist", 0.03f);
        var amin = GetFloat(args, "amin", 0);
        var types = ParseTypes(GetString(args, "type"), "MapPiece");
        var only = GetString(args, "only")?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var skip = GetString(args, "skip")?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet(StringComparer.OrdinalIgnoreCase) ?? new();

        var parts = new List<PartGeo>();
        foreach (var ent in target.Objects.OfType<MsbEntity>())
        {
            if (ent.WrappedObject is not IMsbPart mp || (types != null && !types.Contains(PartTypeName(ent))) ||
                skip.Contains(mp.ModelName) || (only != null && !only.Contains(mp.ModelName)))
                continue;

            var geo = LoadModel(project, target.Name, mp.ModelName, true);
            if (geo.Triangles.Length == 0)
                continue;

            var m = PartWorld(ent);
            var w = geo.Triangles.Select(v => Vector3.Transform(v, m)).ToArray();
            var (pl, ph) = Extent(w);
            if (!BoxesOverlap(pl, ph, lo, hi))
                continue;

            parts.Add(new PartGeo { Ent = ent, W = w, Lo = pl, Hi = ph });
            yield return null;
        }

        foreach (var p in parts)
            p.Tree = new FirstPersonTriangleTree(p.W);

        var coplanar = new List<JsonObject>();
        for (var ai = 0; ai < parts.Count; ai++)
        {
            var A = parts[ai];
            for (var bi = 0; bi < parts.Count; bi++)
            {
                var B = parts[bi];
                if (ai == bi || !BoxesOverlap(A.Lo, A.Hi, B.Lo, B.Hi, maxd))
                    continue;

                int n = 0;
                float area = 0, dmin = float.MaxValue, dmax = 0;
                var sLo = new Vector3(float.MaxValue);
                var sHi = new Vector3(float.MinValue);
                var nSum = Vector3.Zero;

                for (var t = 0; t + 2 < A.W.Length; t += 3)
                {
                    var w0 = A.W[t];
                    var e1 = A.W[t + 1] - w0;
                    var e2 = A.W[t + 2] - w0;
                    var cr = Vector3.Cross(e1, e2);
                    var ta = cr.Length() / 2;
                    if (ta < 1e-6f || ta < amin)
                        continue;
                    var nrm = cr / (2 * ta);
                    var steps = (int)Math.Clamp(MathF.Ceiling(MathF.Max(e1.Length(), e2.Length()) / 0.2f), 1, 12);

                    for (var i = 0; i < steps; i++)
                    for (var j = 0; j < steps - i; j++)
                    {
                        var p = w0 + (i + 1 / 3f) / steps * e1 + (j + 1 / 3f) / steps * e2;
                        if (!BoxesOverlap(p, p, B.Lo, B.Hi, maxd))
                            continue;

                        foreach (var sg in new[] { 1f, -1f })
                        {
                            if (!B.Tree.RayCast(p - sg * nrm * 1e-4f, sg * nrm, maxd, out var dist, out var tri) ||
                                Vector3.Dot(B.Tree.Normal(tri), nrm) <= 0.95f)
                                continue;

                            n++;
                            area += ta / (steps * steps);
                            dmin = MathF.Min(dmin, dist);
                            dmax = MathF.Max(dmax, dist);
                            sLo = Vector3.Min(sLo, p);
                            sHi = Vector3.Max(sHi, p);
                            nSum += nrm;
                            break;
                        }
                    }
                }

                if (n == 0)
                    continue;

                // Each pair once, the direction with more area
                var twin = coplanar.FindIndex(c => (string)c["a"] == B.Ent.Name && (string)c["b"] == A.Ent.Name);
                if (twin >= 0 && (double)coplanar[twin]["area"] >= Round(area))
                    continue;
                if (twin >= 0)
                    coplanar.RemoveAt(twin);

                coplanar.Add(new JsonObject
                {
                    ["a"] = A.Ent.Name, ["b"] = B.Ent.Name, ["samples"] = n, ["area"] = Round(area),
                    ["distCm"] = new JsonArray(Round(dmin * 100), Round(dmax * 100)),
                    ["min"] = ToJson(sLo), ["max"] = ToJson(sHi), ["normal"] = ToJson(Vector3.Normalize(nSum)),
                });
            }

            yield return null;
        }

        // Interpenetration of smaller parts (boxes), SAT depth
        var maxSize = GetFloat(args, "maxSize", 4);
        var minDepth = GetFloat(args, "minDepth", 0.05f);
        var boxes = new List<(MsbEntity Ent, Vector3 C, Vector3[] Ax, Vector3 He)>();
        foreach (var ent in target.Objects.OfType<MsbEntity>())
        {
            if (ent.WrappedObject is not IMsbPart || !TryLocalBounds(ent, out var lb))
                continue;
            var t = ent.GetLocalTransform();
            var he = (lb.Max - lb.Min) * t.Scale / 2;
            if (MathF.Max(he.X, MathF.Max(he.Y, he.Z)) * 2 > maxSize || PartTypeName(ent) == "Collision")
                continue;
            var c = Vector3.Transform((lb.Min + lb.Max) / 2, t.WorldMatrix);
            if (!BoxesOverlap(c, c, lo, hi))
                continue;
            var ax = new[] { Vector3.Transform(Vector3.UnitX, t.Rotation), Vector3.Transform(Vector3.UnitY, t.Rotation), Vector3.Transform(Vector3.UnitZ, t.Rotation) };
            boxes.Add((ent, c, ax, Vector3.Abs(he)));
        }

        var inside = new List<JsonObject>();
        for (var i = 0; i < boxes.Count; i++)
        for (var j = i + 1; j < boxes.Count; j++)
        {
            var depth = SatDepth(boxes[i], boxes[j]);
            if (depth > minDepth)
                inside.Add(new JsonObject { ["a"] = boxes[i].Ent.Name, ["b"] = boxes[j].Ent.Name, ["depth"] = Round(depth), ["at"] = ToJson((boxes[i].C + boxes[j].C) / 2) });
        }

        var limit = (int)GetFloat(args, "limit", 50);
        result["parts"] = parts.Count;
        result["coplanar"] = new JsonArray(coplanar.OrderByDescending(c => (double)c["area"]).Take(limit).Select(c => (JsonNode)c).ToArray());
        result["inside"] = new JsonArray(inside.OrderByDescending(c => (double)c["depth"]).Take(limit).Select(c => (JsonNode)c).ToArray());
    }

    private static float SatDepth((MsbEntity, Vector3 C, Vector3[] Ax, Vector3 He) a, (MsbEntity, Vector3 C, Vector3[] Ax, Vector3 He) b)
    {
        var axes = new List<Vector3>(a.Ax.Concat(b.Ax));
        foreach (var x in a.Ax)
        foreach (var y in b.Ax)
        {
            var c = Vector3.Cross(x, y);
            if (c.LengthSquared() > 1e-6f)
                axes.Add(Vector3.Normalize(c));
        }

        var d = b.C - a.C;
        var best = float.MaxValue;
        foreach (var axis in axes)
        {
            float Proj((MsbEntity, Vector3, Vector3[] Ax, Vector3 He) o) =>
                o.He.X * MathF.Abs(Vector3.Dot(o.Ax[0], axis)) + o.He.Y * MathF.Abs(Vector3.Dot(o.Ax[1], axis)) + o.He.Z * MathF.Abs(Vector3.Dot(o.Ax[2], axis));
            var overlap = Proj(a) + Proj(b) - MathF.Abs(Vector3.Dot(d, axis));
            if (overlap <= 0)
                return 0;
            best = MathF.Min(best, overlap);
        }

        return best;
    }

    #endregion

    #region free-spot, los

    private static IEnumerator FreeSpot(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var (lo, hi) = Region3(args);
        float sx, sz, height;

        if (GetString(args, "from") is string from || GetString(args, "model") is string)
        {
            var (m, p) = SplitFrom(GetString(args, "from") ?? throw new RemoteException("from=<map>/<part>"));
            var ent = RequirePart(RequireLoadedMap(view, m), p);
            if (!TryLocalBounds(ent, out var lb))
                throw new RemoteException("Model not loaded");
            var ext = (lb.Max - lb.Min) * PartScale(ent);
            (sx, height, sz) = (ext.X, ext.Y, ext.Z);
        }
        else
        {
            var s = GetVector(args, "size") ?? throw new RemoteException("Give size=x,y,z (meters) or from=<map>/<part>");
            (sx, height, sz) = (s.X, s.Y, s.Z);
        }

        var step = GetFloat(args, "step", 0.5f);
        var clearance = GetFloat(args, "clearance", 0.1f);
        var wall = GetBool(args, "wall", false);
        var yaws = (GetString(args, "yaws") ?? "0,90").Split(',').Select(v => float.Parse(v, CultureInfo.InvariantCulture)).ToArray();
        var limit = (int)GetFloat(args, "limit", 20);

        var collision = EnsureCollision(view, 300);
        while (collision.MoveNext())
            yield return null;

        var set = view.FirstPersonCollision.Current;
        var obstacles = target.Objects.OfType<MsbEntity>()
            .Where(e => e.WrappedObject is IMsbPart && PartTypeName(e) is "Object" or "MapPiece" or "Enemy" && e.RenderSceneMesh != null)
            .Select(e => e.RenderSceneMesh.GetBounds())
            .Where(b => (b.Max - b.Min).X < 3 && (b.Max - b.Min).Z < 3 && BoxesOverlap(b.Min, b.Max, lo - new Vector3(3), hi + new Vector3(3)))
            .ToList();

        var spots = new List<JsonObject>();
        var taken = new List<Vector3>();
        var spacing = MathF.Max(sx, sz);
        var tested = 0;

        for (var x = lo.X; x <= hi.X; x += step)
        {
            for (var z = lo.Z; z <= hi.Z; z += step)
            {
                tested++;
                if (!set.RayCast(new Vector3(x, hi.Y, z), -Vector3.UnitY, hi.Y - lo.Y, out var floor) || floor.Normal.Y < 0.7f)
                    continue;

                var c = floor.Point;
                foreach (var yaw in yaws)
                {
                    var q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw * Deg);
                    var ax = Vector3.Transform(Vector3.UnitX, q);
                    var az = Vector3.Transform(Vector3.UnitZ, q);
                    var ok = true;

                    // Flat under all four corners
                    foreach (var (u, v) in new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) })
                    {
                        var corner = c + ax * (u * sx / 2) + az * (v * sz / 2);
                        if (!set.RayCast(corner + new Vector3(0, 0.5f, 0), -Vector3.UnitY, 1.0f, out var h) || MathF.Abs(h.Point.Y - c.Y) > 0.15f || h.Normal.Y < 0.7f)
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (!ok)
                        continue;

                    // Walls around, at two heights
                    var wallSide = (string)null;
                    var wallDist = float.MaxValue;
                    foreach (var (dirv, half, side) in new[] { (ax, sx / 2, "+x"), (-ax, sx / 2, "-x"), (az, sz / 2, "+z"), (-az, sz / 2, "-z") })
                    {
                        foreach (var hy in new[] { 0.3f, MathF.Min(height, 1.5f) })
                        {
                            if (set.RayCast(c + new Vector3(0, hy, 0), dirv, half + 3, out var hit))
                            {
                                var room = hit.Distance - half;
                                if (room < clearance - 1e-3f)
                                    ok = false;
                                if (room < wallDist)
                                {
                                    wallDist = room;
                                    wallSide = side;
                                }
                            }
                        }
                    }

                    if (!ok || (wall && wallDist > clearance + 0.15f))
                        continue;

                    var boxLo = c - new Vector3(MathF.Abs(ax.X) * sx / 2 + MathF.Abs(az.X) * sz / 2, 0, MathF.Abs(ax.Z) * sx / 2 + MathF.Abs(az.Z) * sz / 2) + new Vector3(0, 0.05f, 0);
                    var boxHi = c + new Vector3(MathF.Abs(ax.X) * sx / 2 + MathF.Abs(az.X) * sz / 2, height, MathF.Abs(ax.Z) * sx / 2 + MathF.Abs(az.Z) * sz / 2);
                    if (obstacles.Any(b => BoxesOverlap(b.Min, b.Max, boxLo, boxHi)))
                        continue;

                    if (taken.Any(t => Vector3.Distance(t, c) < spacing))
                        continue;

                    taken.Add(c);
                    spots.Add(new JsonObject
                    {
                        ["pos"] = ToJson(c), ["yaw"] = yaw,
                        ["wallSide"] = wallDist < 3 ? wallSide : null,
                        ["wallDist"] = wallDist < 3 ? Round(wallDist) : null,
                    });
                    break;
                }

                if (spots.Count >= limit)
                    break;
            }

            if (spots.Count >= limit)
                break;

            yield return null;
        }

        result["size"] = new JsonArray(sx, height, sz);
        result["tested"] = tested;
        result["spots"] = new JsonArray(spots.Select(s => (JsonNode)s).ToArray());
    }

    private static IEnumerator Los(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var from = GetVector(args, "from") ?? throw new RemoteException("Missing argument 'from'");
        Entity targetPart = null;
        Vector3 to;

        if (GetString(args, "part") is string partName)
        {
            targetPart = FindEntity(view.Editor.Project, GetString(args, "map"), partName);
            var b = targetPart.RenderSceneMesh?.GetBounds() ?? throw new RemoteException("Part has no render mesh");
            to = (b.Min + b.Max) / 2;
        }
        else
        {
            to = GetVector(args, "to") ?? throw new RemoteException("Give to=x,y,z or part=<name>");
        }

        var dir = to - from;
        var dist = dir.Length();
        dir /= dist;

        var collision = EnsureCollision(view, 300);
        while (collision.MoveNext())
            yield return null;

        var blocked = view.FirstPersonCollision.Current.RayCast(from, dir, dist, out var hit) && hit.Distance < dist - 0.05f &&
                      hit.Mesh?.PartName != targetPart?.Name;

        // Render meshes the line crosses before the target: FLVER triangles (a hull the line runs inside doesn't count),
        // the box for a part whose model can't be read
        var scene = new MeshScene(view, null, new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "MapPiece", "Object", "DummyObject", "Enemy", "DummyEnemy" });
        var crossed = scene.Crossings(from, dir, 0.01f, dist - 0.05f, it => it.Ent == targetPart);

        result["from"] = ToJson(from);
        result["to"] = ToJson(to);
        result["distance"] = Round(dist);
        result["free"] = !blocked;
        result["blockedBy"] = blocked
            ? new JsonObject { ["collision"] = hit.Mesh?.PartName, ["point"] = ToJson(hit.Point), ["distance"] = Round(hit.Distance) }
            : null;
        result["freeRender"] = !crossed.Any(c => !c.BoxOnly);
        result["boxes"] = new JsonArray(crossed.Take(8).Select(c => (JsonNode)new JsonObject
        {
            ["name"] = c.Item.Ent.Name, ["distance"] = Round(c.Distance), ["point"] = ToJson(c.Point), ["test"] = c.BoxOnly ? "box" : "mesh",
        }).ToArray());
    }

    #endregion

    #region lightmap-check, texture-check

    private static IEnumerator LightmapCheck(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var lm = new LightmapIndexInfo();
        var load = LoadLightmapIndex(view, target, lm);
        while (load.MoveNext())
            yield return null;

        var filter = GetString(args, "filter") is string f ? new Regex(f, RegexOptions.IgnoreCase) : null;
        var ok = 0;
        var fallback = new JsonArray();
        var missing = new JsonArray();
        foreach (var ent in target.Objects.OfType<MsbEntity>().Where(e => e.WrappedObject is MSBB.Part.MapPiece))
        {
            if (filter != null && !filter.IsMatch(ent.Name))
                continue;
            var model = ((IMsbPart)ent.WrappedObject).ModelName;
            var status = LightmapStatus(lm, ent.Name, model, out var gameName, out var bakedName);

            if (status == "lit")
            {
                ok++;
                continue;
            }

            var entry = new JsonObject { ["part"] = ent.Name, ["model"] = model, ["btab"] = lm.Atlas.ContainsKey(ent.Name), ["expected"] = gameName };
            if (status == "onlyFallback")
            {
                entry["smithboxFinds"] = bakedName;
                fallback.Add(entry);
            }
            else
            {
                missing.Add(entry);
            }
        }

        result["map"] = target.Name;
        result["code"] = lm.Code;
        result["ok"] = true;
        result["lit"] = ok;
        result["onlyFallback"] = fallback;
        result["missing"] = missing;
        result["note"] = "onlyFallback: Smithbox finds the lightmap only via Viewport_Lightmap_Baked_Code_Fallback (different map code), " +
                         "in game it is missing (bright placeholder). missing: Smithbox finds none either.";
    }

    private static IEnumerator TextureCheck(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var project = view.Editor.Project;
        var target = GetTargetMap(view, args);
        var types = ParseTypes(GetString(args, "type"), "MapPiece", "Object");
        var filter = GetString(args, "filter") is string f ? new Regex(f, RegexOptions.IgnoreCase) : null;

        var idle = WaitIdle(GetFloat(args, "timeout", 600), 3);
        while (idle.MoveNext())
            yield return null;

        var loaded = LoadedTextures(out var known);

        var models = new Dictionary<string, List<string>>();
        foreach (var ent in target.Objects.OfType<MsbEntity>())
        {
            if (ent.WrappedObject is not IMsbPart mp || (types != null && !types.Contains(PartTypeName(ent))) ||
                (filter != null && !filter.IsMatch(ent.Name)))
                continue;
            if (!models.TryGetValue(mp.ModelName, out var list))
                models[mp.ModelName] = list = new List<string>();
            list.Add(ent.Name);
        }

        var bad = new JsonArray();
        var unread = new JsonArray();
        foreach (var (model, partNames) in models.OrderBy(m => m.Key))
        {
            var geo = LoadModel(project, target.Name, model, false);
            if (!geo.Found)
            {
                unread.Add(model);
                continue;
            }

            var miss = geo.Textures.Where(x => !loaded.Contains(x)).ToList();
            if (miss.Count > 0)
            {
                bad.Add(new JsonObject
                {
                    ["model"] = model,
                    ["parts"] = new JsonArray(partNames.Take(5).Select(p => (JsonNode)p).ToArray()),
                    ["instances"] = partNames.Count,
                    ["missing"] = new JsonArray(miss.Select(x => (JsonNode)new JsonObject { ["texture"] = x, ["requested"] = known.Contains(x) }).ToArray()),
                });
            }

            yield return null;
        }

        result["map"] = target.Name;
        result["models"] = models.Count;
        result["loadedTextures"] = loaded.Count;
        result["missing"] = bad;
        result["flverNotFound"] = unread;
        result["note"] = "Checked against the textures loaded in Smithbox (MapEditor_TextureLoad_* must be on); requested=false: " +
                         "Smithbox never requested them (different material/shader).";
    }

    #endregion

    #region find-model, model-search

    private record IndexPart(string Map, string Name, string Model, string Type, Vector3 Pos, Vector3 Scale);

    private static readonly Dictionary<string, List<IndexPart>> MapIndex = new();

    private static IEnumerable<string> ProjectMaps(ProjectEntry project, Regex maps) =>
        project.Handler.MapData.PrimaryBank.Maps.Keys.Select(k => k.Filename).Where(m => maps == null || maps.IsMatch(m)).OrderBy(m => m);

    private static IEnumerator BuildIndex(ProjectEntry project, Regex maps, List<IndexPart> into)
    {
        var n = 0;
        foreach (var map in ProjectMaps(project, maps))
        {
            if (!MapIndex.TryGetValue(map, out var list))
            {
                list = new List<IndexPart>();
                try
                {
                    var msb = ReadMsb(project, map);
                    foreach (var p in msb.Parts.GetEntries())
                        list.Add(new IndexPart(map, p.Name, p.ModelName, p.GetType().Name, p.Position, p.Scale));
                }
                catch (Exception)
                {
                    // No MSB (or unreadable): the map has no parts here
                }

                MapIndex[map] = list;
                if (++n % 5 == 0)
                    yield return null;
            }

            into.AddRange(list);
        }
    }

    private static JsonArray Size(ModelGeo g) => g.Found ? ToJson(g.Max - g.Min) : null;

    private static IEnumerator FindModel(JsonObject args, JsonObject result)
    {
        var project = GetProject() ?? throw new RemoteException("No project is loaded");
        var model = RequireString(args, "model");
        var mapsRx = GetString(args, "maps") is string m ? new Regex(m, RegexOptions.IgnoreCase) : null;
        var all = new List<IndexPart>();
        var build = BuildIndex(project, mapsRx, all);
        while (build.MoveNext())
            yield return null;

        var rx = new Regex($"^{model}$", RegexOptions.IgnoreCase);
        var hits = all.Where(p => p.Model != null && rx.IsMatch(p.Model)).ToList();
        var limit = (int)GetFloat(args, "limit", 20);

        result["model"] = model;
        result["instances"] = hits.Count;
        result["models"] = new JsonArray(hits.GroupBy(h => h.Model).OrderBy(g => g.Key).Take(50).Select(g =>
        {
            var first = g.First();
            var geo = LoadModel(project, first.Map, g.Key, false);
            return (JsonNode)new JsonObject
            {
                ["model"] = g.Key, ["type"] = first.Type, ["instances"] = g.Count(), ["size"] = Size(geo),
                ["textures"] = geo.Found ? new JsonArray(geo.Textures.Take(12).Select(x => (JsonNode)x).ToArray()) : null,
                ["maps"] = new JsonArray(g.GroupBy(x => x.Map).Select(mg => (JsonNode)new JsonObject
                {
                    ["map"] = mg.Key, ["count"] = mg.Count(),
                    ["parts"] = new JsonArray(mg.Take(limit).Select(x => (JsonNode)new JsonObject
                    {
                        ["name"] = x.Name, ["pos"] = ToJson(x.Pos), ["scale"] = x.Scale == Vector3.One ? null : ToJson(x.Scale)
                    }).ToArray()),
                }).ToArray()),
            };
        }).ToArray());
    }

    private static IEnumerator ModelSearch(JsonObject args, JsonObject result)
    {
        var project = GetProject() ?? throw new RemoteException("No project is loaded");
        var mapsRx = GetString(args, "maps") is string m ? new Regex(m, RegexOptions.IgnoreCase) : null;
        var nameRx = GetString(args, "name") is string n ? new Regex(n, RegexOptions.IgnoreCase) : null;
        var texRx = GetString(args, "texture") is string tx ? new Regex(tx, RegexOptions.IgnoreCase) : null;
        var types = ParseTypes(GetString(args, "type"), "MapPiece", "Object");
        var minSize = GetFloat(args, "minSize", 0);
        var maxSize = GetFloat(args, "maxSize", float.MaxValue);
        var maxRead = (int)GetFloat(args, "maxRead", 400);
        var limit = (int)GetFloat(args, "limit", 100);

        var all = new List<IndexPart>();
        var build = BuildIndex(project, mapsRx, all);
        while (build.MoveNext())
            yield return null;

        var groups = all.Where(p => p.Model != null && (types == null || types.Contains(p.Type)) && (nameRx == null || nameRx.IsMatch(p.Model)))
            .GroupBy(p => p.Model).OrderBy(g => g.Key).ToList();
        var needGeo = texRx != null || minSize > 0 || maxSize < float.MaxValue || GetBool(args, "sizes", false);
        if (needGeo && groups.Count > maxRead)
            throw new RemoteException($"{groups.Count} models to read for size/texture filters, more than maxRead={maxRead}; narrow name= or maps=");

        var list = new JsonArray();
        var read = 0;
        foreach (var g in groups)
        {
            var first = g.First();
            ModelGeo geo = null;
            if (needGeo)
            {
                geo = LoadModel(project, first.Map, g.Key, false);
                if (++read % 10 == 0)
                    yield return null;
                var ext = geo.Found ? geo.Max - geo.Min : Vector3.Zero;
                var big = MathF.Max(ext.X, MathF.Max(ext.Y, ext.Z));
                if (!geo.Found || big < minSize || big > maxSize)
                    continue;
                if (texRx != null && !geo.Textures.Any(x => texRx.IsMatch(x)))
                    continue;
            }

            list.Add(new JsonObject
            {
                ["model"] = g.Key, ["type"] = first.Type, ["instances"] = g.Count(),
                ["maps"] = new JsonArray(g.Select(x => x.Map).Distinct().Take(8).Select(x => (JsonNode)x).ToArray()),
                ["example"] = $"{first.Map}/{first.Name}",
                ["size"] = geo != null ? Size(geo) : null,
                ["textures"] = geo != null && texRx != null ? new JsonArray(geo.Textures.Where(x => texRx.IsMatch(x)).Select(x => (JsonNode)x).ToArray()) : null,
            });

            if (list.Count >= limit)
                break;
        }

        result["matches"] = list.Count;
        result["models"] = list;
    }

    #endregion
}
