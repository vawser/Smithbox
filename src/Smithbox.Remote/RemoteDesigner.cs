using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Veldrid.Utilities;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Commands for modelling like a level designer: copy groups from original maps (group-copy), set parts onto a
///     surface hit by a ray (place-on), dock parts flush (align), undo/redo, screenshot with part names
///     (screenshot labels=true), query pixels (pick) and model previews (model-preview). Every change is exactly one
///     Smithbox action (<see cref="RemoteStep" />), so a single undo reverts it.
/// </summary>
public static partial class RemoteControl
{
    // Answers Smithbox's light atlas question without a dialog; only in builds that have the field, otherwise the dialog shows
    private static readonly System.Reflection.FieldInfo AtlasAnswerOverride =
        typeof(LightAtlasResolver).GetField("DefaultEntryAnswerOverride");

    #region Undo steps

    /// <summary>
    ///     One undo step for a remote command: the sub-actions run on the first Execute (Build) and are undone in
    ///     reverse order. Records are entries for a placement journal kept by the client.
    /// </summary>
    private class RemoteStep : ViewportAction
    {
        private readonly List<ViewportAction> Done = new();
        private readonly Action<RemoteStep> Build;
        private bool Built;
        private ActionEvent Events;

        public readonly string Label;

        /// <summary>Records for the client journal, each with its inverse record for undo (null if the journal can't express it)</summary>
        public readonly List<(JsonObject Record, JsonObject Inverse)> Records = new();

        public RemoteStep(string label, Action<RemoteStep> build)
        {
            Label = label;
            Build = build;
        }

        public void Run(ViewportAction action)
        {
            Events |= action.Execute();
            Done.Add(action);
        }

        /// <summary>batch: adopt an already executed step (redo/undo then go through it)</summary>
        public void Adopt(RemoteStep inner)
        {
            Done.Add(inner);
            Records.AddRange(inner.Records);
            TouchedScopes.UnionWith(inner.TouchedScopes);
        }

        /// <summary>batch: the events of the adopted steps, reported by the first Execute</summary>
        public void PresetEvents(ActionEvent events) => Events |= events;

        public override ActionEvent Execute(bool isRedo = false)
        {
            if (!Built)
            {
                Built = true;

                try
                {
                    Build(this);
                }
                catch
                {
                    for (var i = Done.Count - 1; i >= 0; i--)
                        Done[i].Undo();

                    Done.Clear();
                    throw;
                }

                return Events;
            }

            var evt = ActionEvent.NoEvent;

            foreach (var action in Done)
                evt |= action.Execute(true);

            return evt;
        }

        public override ActionEvent Undo()
        {
            var evt = ActionEvent.NoEvent;

            for (var i = Done.Count - 1; i >= 0; i--)
                evt |= Done[i].Undo();

            return evt;
        }
    }

    /// <summary>
    ///     Executes a step; addAtlas answers Smithbox's light atlas question (BTAB entries for new models)
    /// </summary>
    private static void ExecuteStep(MapEditorView view, RemoteStep step, bool addAtlas)
    {
        AtlasAnswerOverride?.SetValue(null, (bool?)addAtlas);

        try
        {
            // In a batch the steps are collected and go into the history as one at the end
            if (BatchSteps != null)
                ExecuteBatched(step);
            else
                view.ViewportActionManager.ExecuteAction(step);
        }
        finally
        {
            AtlasAnswerOverride?.SetValue(null, null);
        }

        view.ViewportSelection.ClearSelection();
    }

    private static JsonArray RecordsJson(RemoteStep step) =>
        new(step.Records.Select(r => (JsonNode)r.Record.DeepClone()).ToArray());

    private static JsonObject AddRecord(MsbEntity clone, string map, string srcMap, string srcPart, bool keepIds, bool addAtlas)
    {
        return new JsonObject
        {
            ["op"] = "add",
            ["name"] = clone.Name,
            ["map"] = map,
            ["src_map"] = srcMap,
            ["src_part"] = srcPart,
            ["pos"] = ToJson(GetPartVector(clone, "Position")),
            ["rot"] = ToJson(GetPartVector(clone, "Rotation")),
            ["entity_id_cleared"] = !keepIds,
            ["atlas_added"] = addAtlas,
        };
    }

    private static void RecordAdd(RemoteStep step, MsbEntity clone, string map, string srcMap, string srcPart, bool keepIds,
        bool addAtlas)
    {
        step.Records.Add((AddRecord(clone, map, srcMap, srcPart, keepIds, addAtlas),
            new JsonObject { ["op"] = "delete", ["name"] = clone.Name, ["map"] = map }));
    }

    /// <summary>Move record with the current values; the inverse record holds the previous values</summary>
    private static void RecordMove(RemoteStep step, MsbEntity ent, string map, Vector3 oldPos, Vector3 oldRot, Vector3 oldScale)
    {
        var scale = GetPartVector(ent, "Scale");
        var rec = new JsonObject
        {
            ["op"] = "move", ["name"] = ent.Name, ["map"] = map,
            ["pos"] = ToJson(GetPartVector(ent, "Position")), ["rot"] = ToJson(GetPartVector(ent, "Rotation")),
        };
        var inv = new JsonObject
        {
            ["op"] = "move", ["name"] = ent.Name, ["map"] = map, ["pos"] = ToJson(oldPos), ["rot"] = ToJson(oldRot),
        };

        if (HasScale(ent) && (scale != Vector3.One || oldScale != Vector3.One))
        {
            rec["scale"] = ToJson(scale);
            inv["scale"] = ToJson(oldScale);
        }

        step.Records.Add((rec, inv));
    }

    private static IEnumerator Undo(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var manager = view.ViewportActionManager;
        var steps = (int)GetFloat(args, "steps", 1);
        var done = new JsonArray();
        var unrecords = new JsonArray();

        for (var i = 0; i < steps && manager.CanUndo(); i++)
        {
            var action = manager.PeekUndoAction();
            manager.UndoAction();

            if (action is RemoteStep step)
            {
                done.Add(step.Label);

                for (var r = step.Records.Count - 1; r >= 0; r--)
                {
                    unrecords.Add(new JsonObject
                    {
                        ["record"] = step.Records[r].Record.DeepClone(),
                        ["inverse"] = step.Records[r].Inverse?.DeepClone(),
                    });
                }
            }
            else
            {
                done.Add(action.GetType().Name + " (Smithbox, not in journal)");
            }
        }

        view.ViewportSelection.ClearSelection();
        yield return null;

        result["undone"] = done;
        result["unrecords"] = unrecords;
        result["canUndo"] = manager.CanUndo();
        result["canRedo"] = manager.CanRedo();
    }

    private static IEnumerator Redo(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var manager = view.ViewportActionManager;
        var steps = (int)GetFloat(args, "steps", 1);
        var done = new JsonArray();
        var records = new JsonArray();

        for (var i = 0; i < steps && manager.CanRedo(); i++)
        {
            manager.RedoAction();
            var action = manager.PeekUndoAction();

            if (action is RemoteStep step)
            {
                done.Add(step.Label);

                foreach (var r in step.Records)
                    records.Add(r.Record.DeepClone());
            }
            else
            {
                done.Add(action?.GetType().Name + " (Smithbox, not in journal)");
            }
        }

        view.ViewportSelection.ClearSelection();
        yield return null;

        result["redone"] = done;
        result["records"] = records;
        result["canUndo"] = manager.CanUndo();
        result["canRedo"] = manager.CanRedo();
    }

    #endregion

    #region Transforms

    private const float Deg = MathF.PI / 180f;

    private static bool HasScale(MsbEntity ent) => ent.WrappedObject.GetType().GetProperty("Scale") != null;

    /// <summary>Rotation as a quaternion, the way Smithbox reads it from the part's Rotation field (Entity.GetLocalTransform)</summary>
    private static Quaternion PartQuat(MsbEntity ent, Vector3 rotation)
    {
        var r = ent.IsRotationPropertyRadians("Rotation") ? rotation : rotation * Deg;
        return ent.IsRotationXZY("Rotation") ? Transform.EulerToQuaternionXZY(r) : Transform.EulerToQuaternion(r);
    }

    /// <summary>Quaternion back into the Rotation field (like Entity.GetUpdateTransformAction), rounded to 1e-4</summary>
    private static Vector3 PartRotation(MsbEntity ent, Quaternion q)
    {
        var t = new Transform(Vector3.Zero, Quaternion.Normalize(q), Vector3.One);
        var r = ent.IsRotationXZY("Rotation") ? t.EulerRotationXZY : t.EulerRotation;

        if (!ent.IsRotationPropertyRadians("Rotation"))
            r /= Deg;

        static float Clean(float f)
        {
            var v = MathF.Round(f, 4);
            return v == 0 ? 0 : v;
        }

        return new Vector3(Clean(r.X), Clean(r.Y), Clean(r.Z));
    }

    private static Vector3 PartScale(MsbEntity ent) => HasScale(ent) ? GetPartVector(ent, "Scale") : Vector3.One;

    /// <summary>Position, rotation (field values) and scale of a part as one change</summary>
    private static PropChangeAction PartChange(MsbEntity ent, Vector3? pos, Vector3? rot, Vector3? scale)
    {
        var type = ent.WrappedObject.GetType();
        var act = new PropChangeAction(ent, ent.WrappedObject);

        if (pos is Vector3 p)
            act.AddPropertyChange(type.GetProperty("Position"), p);

        if (rot is Vector3 r)
            act.AddPropertyChange(type.GetProperty("Rotation"), r);

        if (scale is Vector3 s)
        {
            var prop = type.GetProperty("Scale") ?? throw new RemoteException($"{type.Name} has no Scale");
            act.AddPropertyChange(prop, s);
        }

        act.SetPostExecutionAction(_ => ent.UpdateRenderModel());
        return act;
    }

    /// <summary>Local (model) box of the render mesh; false while the model is not loaded</summary>
    private static bool TryLocalBounds(Entity ent, out BoundingBox box)
    {
        box = default;
        var mesh = ent?.RenderSceneMesh;

        if (mesh == null)
            return false;

        box = mesh.GetLocalBounds();
        var size = box.Max - box.Min;

        return size.X >= 0 && size.Y >= 0 && size.Z >= 0 && size.LengthSquared() > 1e-10f &&
               float.IsFinite(size.X + size.Y + size.Z);
    }

    private static IEnumerable<Vector3> Corners(BoundingBox b)
    {
        for (var i = 0; i < 8; i++)
            yield return new Vector3((i & 1) == 0 ? b.Min.X : b.Max.X, (i & 2) == 0 ? b.Min.Y : b.Max.Y,
                (i & 4) == 0 ? b.Min.Z : b.Max.Z);
    }

    /// <summary>The corners of the local box after scale and rotation (without translation), as in WorldMatrix = S * R * T</summary>
    private static IEnumerable<Vector3> PlacedCorners(BoundingBox local, Quaternion q, Vector3 scale) =>
        Corners(local).Select(c => Vector3.Transform(c * scale, q));

    private static (Vector3 Min, Vector3 Max) Extent(IEnumerable<Vector3> points)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var p in points)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        return (min, max);
    }

    /// <summary>Shortest rotation from a to b (both normalized)</summary>
    private static Quaternion FromTo(Vector3 a, Vector3 b)
    {
        var dot = Math.Clamp(Vector3.Dot(a, b), -1f, 1f);

        if (dot > 0.99999f)
            return Quaternion.Identity;

        if (dot < -0.99999f)
        {
            var axis = Vector3.Cross(a, Vector3.UnitX);
            if (axis.LengthSquared() < 1e-6f)
                axis = Vector3.Cross(a, Vector3.UnitZ);
            return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), MathF.PI);
        }

        return Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(a, b)), MathF.Acos(dot));
    }

    /// <summary>"+x", "-z", "y" ... as axis index and sign</summary>
    private static (int Axis, float Sign) ParseSide(string side, string argName)
    {
        var s = side.Trim().ToLowerInvariant();
        var sign = s.StartsWith('-') ? -1f : 1f;
        s = s.TrimStart('+', '-');

        return s switch
        {
            "x" => (0, sign),
            "y" => (1, sign),
            "z" => (2, sign),
            _ => throw new RemoteException($"'{argName}' must be +x, -x, +y, -y, +z or -z")
        };
    }

    private static float Get(Vector3 v, int axis) => axis == 0 ? v.X : axis == 1 ? v.Y : v.Z;

    private static Vector3 With(Vector3 v, int axis, float value) =>
        axis == 0 ? new Vector3(value, v.Y, v.Z) : axis == 1 ? new Vector3(v.X, value, v.Z) : new Vector3(v.X, v.Y, value);

    private static Vector3 AxisVector(int axis, float sign) => With(Vector3.Zero, axis, sign);

    /// <summary>scale=1.5 or scale=1,2,1</summary>
    private static Vector3? GetScale(JsonObject args)
    {
        if (!Has(args, "scale"))
            return null;

        var text = GetString(args, "scale");

        if (!text.Contains(',') && !text.Contains(' ') && !text.Contains('['))
        {
            var f = GetFloat(args, "scale", 1);
            return new Vector3(f);
        }

        return GetVector(args, "scale");
    }

    #endregion

    #region Map and part helpers

    /// <summary>Loads a map (e.g. the source of a copy) if it is not loaded, and waits for its models</summary>
    private static IEnumerator EnsureMapLoaded(MapEditorView view, string map, JsonObject args, bool[] loadedHere)
    {
        if (view.Selection.GetMapContainerFromMapID(map) != null)
            yield break;

        if (view.Selection.GetFileEntryFromMapID(map) == null)
            throw new RemoteException($"The project has no map {map}");

        view.Universe.LoadMap(map);
        loadedHere[0] = true;

        var load = WaitMapLoaded(view, map, GetFloat(args, "timeout", 600));
        while (load.MoveNext())
            yield return null;

        var idle = WaitIdle(GetFloat(args, "timeout", 600), 3);
        while (idle.MoveNext())
            yield return null;
    }

    private static (string Map, string Part) SplitFrom(string from)
    {
        var slash = from.LastIndexOf('/');

        if (slash <= 0 || slash == from.Length - 1)
            throw new RemoteException("from must be <map>/<part>, e.g. m24_01_00_00/o240028_5307");

        return (from[..slash], from[(slash + 1)..]);
    }

    /// <summary>Duplicates a part into the target map (EntDuplicateAction) and optionally renames it, within the step</summary>
    private static MsbEntity DuplicateInto(RemoteStep step, MapEditorView view, MsbEntity source, MapContainer target,
        bool keepIds, string name)
    {
        var toOtherMap = source.MapID != target.Name;
        var duplicate = new EntDuplicateAction(view, new List<MsbEntity> { source },
            targetMap: toOtherMap ? target : null,
            duplicateToMap: toOtherMap,
            clearEntityID: !keepIds,
            clearEntityGroupID: !keepIds,
            placeAtListEnd: true);

        step.Run(duplicate);

        var clone = duplicate.CreatedClones.FirstOrDefault()
                    ?? throw new RemoteException($"{source.Name} could not be duplicated, see /log");

        // The clone keeps the source's map ID, which the delete and the selection look maps up by
        clone.MapID = target.Name;
        Origins.AddOrUpdate(clone, new[] { source.MapID, source.Name });

        if (name != null && clone.Name != name)
        {
            var nameProp = clone.WrappedObject.GetType().GetProperty("Name");
            step.Run(new PropChangeAction(clone, nameProp, clone.WrappedObject, name));
        }

        return clone;
    }

    /// <summary>Where a copy came from (source map, source name), for ops-export</summary>
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<MsbEntity, string[]> Origins = new();

    private static string PartTypeName(Entity ent) => ent.WrappedObject?.GetType().Name ?? "";

    /// <summary>type=MapPiece,Object,Enemy: class names of the MSB parts (MSBB.Part.*), "all" for all</summary>
    private static HashSet<string> ParseTypes(string text, params string[] fallback)
    {
        var parts = text?.Split(new[] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries) ?? fallback;

        if (parts.Length == 0 || parts.Any(p => p.Equals("all", StringComparison.OrdinalIgnoreCase)))
            return null;

        return new HashSet<string>(parts, StringComparer.OrdinalIgnoreCase);
    }

    #endregion

    #region group-copy

    private static IEnumerator GroupCopy(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var sourceMap = RequireString(args, "from");
        var dry = GetBool(args, "dry", false);
        var keepIds = GetBool(args, "keepEntityId", false);
        var addAtlas = GetBool(args, "addAtlas", false);

        var loaded = new bool[1];
        var load = EnsureMapLoaded(view, sourceMap, args, loaded);
        while (load.MoveNext())
            yield return null;

        try
        {
            var source = RequireLoadedMap(view, sourceMap);
            var names = GetString(args, "parts")?.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries);
            var filter = GetString(args, "filter");
            var regex = filter != null ? new Regex(filter, RegexOptions.IgnoreCase) : null;
            var min = GetVector(args, "min");
            var max = GetVector(args, "max");

            if (names == null && regex == null && min == null)
                throw new RemoteException("Give parts=a,b,..., filter=<regex> or min=x,y,z max=x,y,z");

            if ((min == null) != (max == null))
                throw new RemoteException("min and max go together");

            var types = names != null && !Has(args, "type") ? null : ParseTypes(GetString(args, "type"), "MapPiece", "Object");
            var boxMin = min is Vector3 a && max is Vector3 b ? Vector3.Min(a, b) : Vector3.Zero;
            var boxMax = min is Vector3 a2 && max is Vector3 b2 ? Vector3.Max(a2, b2) : Vector3.Zero;

            var picked = new List<MsbEntity>();

            foreach (var ent in source.Objects.OfType<MsbEntity>().ToList())
            {
                if (ent.WrappedObject is not IMsbPart)
                    continue;

                if (names != null && !names.Contains(ent.Name))
                    continue;

                if (regex != null && !regex.IsMatch(ent.Name ?? ""))
                    continue;

                if (types != null && !types.Contains(PartTypeName(ent)))
                    continue;

                if (min != null)
                {
                    var p = GetPartVector(ent, "Position");

                    if (p.X < boxMin.X || p.Y < boxMin.Y || p.Z < boxMin.Z || p.X > boxMax.X || p.Y > boxMax.Y || p.Z > boxMax.Z)
                        continue;
                }

                picked.Add(ent);
            }

            if (names != null)
            {
                var missing = names.Where(n => picked.All(e => e.Name != n)).ToList();

                if (missing.Count > 0)
                    throw new RemoteException($"Not in {sourceMap} (or filtered out): {string.Join(", ", missing)}");
            }

            if (picked.Count == 0)
                throw new RemoteException($"No parts of {sourceMap} match");

            var limit = (int)GetFloat(args, "limit", 300);

            if (picked.Count > limit)
                throw new RemoteException($"{picked.Count} parts match, more than limit={limit}");

            // Anchor in the source map, target point in the target map
            var positions = picked.Select(e => GetPartVector(e, "Position")).ToList();
            var anchor = GetVector(args, "anchor") ?? (min != null
                ? new Vector3((boxMin.X + boxMax.X) / 2, positions.Min(p => p.Y), (boxMin.Z + boxMax.Z) / 2)
                : new Vector3(positions.Average(p => p.X), positions.Min(p => p.Y), positions.Average(p => p.Z)));
            // With snap and no anchor given, the anchor sits on the source map's floor (so floor goes onto floor)
            if (!Has(args, "anchor") && GetBool(args, "snap", false))
            {
                var below = new FloorResult();
                var cast = CastDown(view, anchor.X, anchor.Z, positions.Max(p => p.Y) + 1.0f, true, below);
                while (cast.MoveNext())
                    yield return null;

                var floor = below.Hits.FirstOrDefault(h => h.Mesh?.MapId == sourceMap);
                if (floor.Mesh != null)
                    anchor.Y = floor.Point.Y;

                result["anchorFloor"] = floor.Mesh != null;
            }

            var to = new[] { GetVector(args, "to") ?? anchor };

            var snap = SnapToFloor(view, args, to);
            while (snap.MoveNext())
                yield return null;

            var rotDeg = GetVector(args, "rot") ?? new Vector3(0, GetFloat(args, "yaw", 0), 0);
            var groupRot = Transform.EulerToQuaternion(rotDeg * Deg);

            // New names: suffix appended, or number=N numbered from N (<model>_<N+i>), else Smithbox's unique names
            var suffix = GetString(args, "suffix");
            var number = Has(args, "number") ? (int)GetFloat(args, "number", 0) : -1;

            string NewName(MsbEntity s, int i)
            {
                if (number >= 0)
                {
                    var us = s.Name.LastIndexOf('_');
                    return (us > 0 ? s.Name[..us] : s.Name) + "_" + (number + i).ToString("D4");
                }

                return suffix != null ? s.Name + suffix : null;
            }

            var plan = picked.Select((s, i) =>
            {
                var t = s.GetLocalTransform();
                var pos = to[0] + Vector3.Transform(t.Position - anchor, groupRot);
                var q = Quaternion.Normalize(Quaternion.Concatenate(t.Rotation, groupRot));
                return (Source: s, Name: NewName(s, i), Pos: pos, Rot: PartRotation(s, q));
            }).ToList();

            var newNames = plan.Where(p => p.Name != null).Select(p => p.Name).ToList();
            var taken = newNames.Where(n => target.GetObjectByName(n) != null).ToList();

            if (taken.Count > 0)
                throw new RemoteException($"{target.Name} already has: {string.Join(", ", taken.Take(10))}");

            if (newNames.Count != newNames.Distinct().Count())
                throw new RemoteException("The new names are not unique (suffix/number)");

            result["anchor"] = ToJson(anchor);
            result["to"] = ToJson(to[0]);
            result["rot"] = ToJson(rotDeg);

            if (dry)
            {
                result["dry"] = true;
                result["parts"] = new JsonArray(plan.Select(p => (JsonNode)new JsonObject
                {
                    ["src"] = p.Source.Name,
                    ["type"] = PartTypeName(p.Source),
                    ["model"] = (p.Source.WrappedObject as IMsbPart)?.ModelName,
                    ["name"] = p.Name,
                    ["pos"] = ToJson(p.Pos),
                    ["rot"] = ToJson(p.Rot),
                }).ToArray());
                yield break;
            }

            var clones = new List<MsbEntity>();
            var step = new RemoteStep($"group-copy {picked.Count} from {sourceMap}", st =>
            {
                foreach (var p in plan)
                {
                    var clone = DuplicateInto(st, view, p.Source, target, keepIds, p.Name);
                    st.Run(PartChange(clone, p.Pos, p.Rot, null));
                    clones.Add(clone);
                    RecordAdd(st, clone, target.Name, sourceMap, p.Source.Name, keepIds, addAtlas);
                }
            });

            ExecuteStep(view, step, addAtlas);
            yield return null;

            result["parts"] = new JsonArray(clones.Select(c => (JsonNode)DescribePart(c)).ToArray());
            result["records"] = RecordsJson(step);
        }
        finally
        {
            if (loaded[0] && !GetBool(args, "keepSource", false))
                view.Universe.UnloadMap(sourceMap);
        }
    }

    #endregion

    #region place-on

    private class RayHit
    {
        public bool Found;
        public FirstPersonHit Hit;
        public Vector3 Origin;
        public Vector3 Direction;
    }

    /// <summary>
    ///     The ray of a command: origin=/dir=, at=px,py (image pixel of the viewport) or the camera center
    /// </summary>
    private static (Vector3 Origin, Vector3 Dir) GetCommandRay(MapEditorView view, JsonObject args)
    {
        var origin = GetVector(args, "origin");
        var dir = GetVector(args, "dir");

        if (origin != null || dir != null)
        {
            if (origin == null || dir == null)
                throw new RemoteException("origin and dir go together");

            if (dir.Value.LengthSquared() < 1e-8f)
                throw new RemoteException("'dir' must not be zero");

            return (origin.Value, Vector3.Normalize(dir.Value));
        }

        var viewport = GetViewport(view);

        if (GetString(args, "at") is string at)
        {
            var xy = at.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (xy.Length != 2 || !float.TryParse(xy[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var px) ||
                !float.TryParse(xy[1], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var py))
                throw new RemoteException("at must be x,y (pixels of the screenshot)");

            var (vx, vy) = ImageToViewport(viewport, px, py);
            var ray = viewport.GetRay(vx, vy);
            return (ray.Origin, Vector3.Normalize(ray.Direction));
        }

        var t = viewport.ViewportCamera.CameraTransform;
        return (t.Position, Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, t.RotationMatrix)));
    }

    /// <summary>Image pixel (screenshot) to viewport coordinates, pixel center</summary>
    private static (float X, float Y) ImageToViewport(VulkanViewport viewport, float px, float py)
    {
        var target = viewport.ViewPipeline?.Target;
        var iw = target != null && target.Width > 0 ? target.Width : (uint)viewport.Width;
        var ih = target != null && target.Height > 0 ? target.Height : (uint)viewport.Height;
        return ((px + 0.5f) * viewport.Width / iw, (py + 0.5f) * viewport.Height / ih);
    }

    /// <summary>First collision hit, skipping hits on the collision of the excluded parts</summary>
    private static IEnumerator CastRay(MapEditorView view, Vector3 origin, Vector3 dir, float max, ICollection<string> skip,
        RayHit result)
    {
        var collision = EnsureCollision(view, 300);
        while (collision.MoveNext())
            yield return null;

        var set = view.FirstPersonCollision.Current;
        var from = origin;
        result.Origin = origin;
        result.Direction = dir;

        for (var i = 0; i < 64; i++)
        {
            var left = max - Vector3.Distance(origin, from);

            if (left <= 0 || !set.RayCast(from, dir, left, out var hit))
                yield break;

            if (skip != null && hit.Mesh?.PartName != null && skip.Contains(hit.Mesh.PartName))
            {
                from = hit.Point + dir * 0.01f;
                continue;
            }

            hit.Distance = Vector3.Distance(origin, hit.Point);
            result.Hit = hit;
            result.Found = true;
            yield break;
        }
    }

    private static IEnumerator PlaceOn(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var keepIds = GetBool(args, "keepEntityId", false);
        var addAtlas = GetBool(args, "addAtlas", false);
        var fromArg = GetString(args, "from");
        var name = GetString(args, "name");

        MsbEntity existing = null;
        string sourceMap = null, sourcePart = null;
        var loaded = new bool[1];

        if (fromArg != null)
        {
            (sourceMap, sourcePart) = SplitFrom(fromArg);

            if (name != null && target.GetObjectByName(name) != null)
                throw new RemoteException($"{target.Name} already has an object named '{name}'");

            var load = EnsureMapLoaded(view, sourceMap, args, loaded);
            while (load.MoveNext())
                yield return null;
        }
        else
        {
            existing = RequirePart(target, name ?? throw new RemoteException("Give from=<map>/<part> (new copy) or name=<part> (move)"));
        }

        try
        {
            var model = existing ?? RequirePart(RequireLoadedMap(view, sourceMap), sourcePart);
            var (origin, dir) = GetCommandRay(view, args);
            var skip = existing != null ? new HashSet<string> { existing.Name } : null;

            // hit=mesh: against the render meshes of the target map (FLVER triangles) instead of the collision
            var hitMode = GetString(args, "hit", "collision").ToLowerInvariant();
            if (hitMode is not ("collision" or "mesh"))
                throw new RemoteException("hit must be collision or mesh");
            var scene = hitMode == "mesh" ? new MeshScene(view, target.Name) : null;

            var cast = new RayHit();
            if (scene != null)
            {
                MeshCast(scene, origin, dir, GetFloat(args, "max", 200), skip, cast);
            }
            else
            {
                var ray = CastRay(view, origin, dir, GetFloat(args, "max", 200), skip, cast);
                while (ray.MoveNext())
                    yield return null;
            }

            if (!cast.Found)
                throw new RemoteException($"The ray from {ToJson(origin).ToJsonString()} along {ToJson(dir).ToJsonString()} hits no " +
                                          (scene != null ? "render mesh" : "collision"));

            var hit = cast.Hit;
            var n = Vector3.Normalize(hit.Normal);
            var orient = GetString(args, "orient", "auto").ToLowerInvariant();

            if (orient == "auto")
                orient = MathF.Abs(n.Y) >= 0.7f ? "up" : "upright";

            var spin = GetFloat(args, "spin", 0) * Deg;
            var current = model.GetLocalTransform().Rotation;
            Quaternion q;

            switch (orient)
            {
                case "up":
                case "normal":
                    // Local +Y along the normal, turned by spin about it
                    q = Quaternion.Concatenate(Quaternion.CreateFromAxisAngle(Vector3.UnitY, spin), FromTo(Vector3.UnitY, n));
                    break;
                case "upright":
                {
                    // World up kept; the local axis (default +z, the front) points away from the surface
                    var (axis, sign) = ParseSide(GetString(args, "axis", "+z"), "axis");
                    if (axis == 1)
                        throw new RemoteException("axis must be horizontal (+x, -x, +z, -z) for orient=upright");
                    var a = AxisVector(axis, sign);
                    var h = new Vector3(n.X, 0, n.Z);
                    var yaw = h.LengthSquared() > 1e-6f ? MathF.Atan2(h.X, h.Z) - MathF.Atan2(a.X, a.Z) : 0;
                    q = Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw + spin);

                    var lean = GetFloat(args, "lean", 0) * Deg;
                    if (lean != 0 && h.LengthSquared() > 1e-6f)
                    {
                        // Tips the top towards the surface
                        var k = Vector3.Normalize(Vector3.Cross(Vector3.UnitY, -Vector3.Normalize(h)));
                        q = Quaternion.Concatenate(q, Quaternion.CreateFromAxisAngle(k, lean));
                    }

                    break;
                }
                case "keep":
                    q = current;
                    break;
                default:
                    throw new RemoteException("orient must be auto, up, upright or keep");
            }

            var scale = existing != null ? PartScale(existing) : PartScale(model);
            var gap = GetFloat(args, "gap", 0);
            var contact = GetString(args, "contact", "bbox").ToLowerInvariant();
            var pos = hit.Point + n * gap;
            var bboxUsed = false;
            BoundingBox local = default;

            if (contact == "bbox" && TryLocalBounds(model, out local))
            {
                // The box's lowest point along the normal touches the surface
                var d = PlacedCorners(local, q, scale).Min(c => Vector3.Dot(c, n));
                pos -= n * d;
                bboxUsed = true;
            }

            pos += GetVector(args, "offset") ?? Vector3.Zero;

            // Optionally down onto the floor below (leaning against a wall)
            if (GetBool(args, "floor", false))
            {
                var bottom = bboxUsed ? PlacedCorners(local, q, scale).Min(c => c.Y) : 0f;
                var below = new RayHit();
                if (scene != null)
                {
                    MeshCast(scene, pos + new Vector3(0, 0.25f, 0), -Vector3.UnitY, 50, skip, below);
                }
                else
                {
                    var down = CastRay(view, pos + new Vector3(0, 0.25f, 0), -Vector3.UnitY, 50, skip, below);
                    while (down.MoveNext())
                        yield return null;
                }

                if (!below.Found)
                    throw new RemoteException($"floor=true: no {(scene != null ? "render mesh" : "collision")} below the placed part");

                pos.Y = below.Hit.Point.Y - bottom + GetFloat(args, "lift", 0);
            }

            var rot = PartRotation(model, q);
            MsbEntity placed = null;
            RemoteStep step;

            if (existing != null)
            {
                var oldPos = GetPartVector(existing, "Position");
                var oldRot = GetPartVector(existing, "Rotation");
                var oldScale = PartScale(existing);
                step = new RemoteStep($"place-on {existing.Name}", st =>
                {
                    st.Run(PartChange(existing, pos, rot, null));
                    RecordMove(st, existing, target.Name, oldPos, oldRot, oldScale);
                });
                placed = existing;
            }
            else
            {
                step = new RemoteStep($"place-on {sourcePart} from {sourceMap}", st =>
                {
                    placed = DuplicateInto(st, view, model, target, keepIds, name);
                    st.Run(PartChange(placed, pos, rot, null));
                    RecordAdd(st, placed, target.Name, sourceMap, sourcePart, keepIds, addAtlas);
                });
            }

            ExecuteStep(view, step, addAtlas);
            yield return null;

            result["part"] = DescribePart(placed);
            result["hit"] = new JsonObject
            {
                ["point"] = ToJson(hit.Point),
                ["normal"] = ToJson(n),
                ["distance"] = Round(hit.Distance),
                ["collision"] = hit.Mesh?.PartName,
                ["material"] = hit.Material,
            };
            result["orient"] = orient;
            result["hitMode"] = hitMode;
            result["contact"] = bboxUsed ? "bbox" : "origin";
            result["records"] = RecordsJson(step);
        }
        finally
        {
            if (loaded[0] && !GetBool(args, "keepSource", false))
                view.Universe.UnloadMap(sourceMap);
        }
    }

    #endregion

    #region align

    private static IEnumerator Align(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var a = RequirePart(target, RequireString(args, "name"));
        var toMap = GetString(args, "toMap");
        var b = RequirePart(toMap != null ? RequireLoadedMap(view, toMap) : target, RequireString(args, "to"));

        if (a == b)
            throw new RemoteException("name and to are the same part");

        if (!TryLocalBounds(a, out var boxA) || !TryLocalBounds(b, out var boxB))
            throw new RemoteException("A part's model is not loaded yet (no render mesh bounds), see /wait");

        var (axis, sign) = ParseSide(RequireString(args, "side"), "side");
        var gap = GetFloat(args, "gap", 0);
        var frame = GetString(args, "frame", "local").ToLowerInvariant();

        if (frame is not ("local" or "world"))
            throw new RemoteException("frame must be local or world");

        var ta = a.GetLocalTransform();
        var tb = b.GetLocalTransform();
        var scaleA = PartScale(a);
        var scaleB = PartScale(b);

        // A's rotation: B's (local frame, matchRot) or its own, then yaw degrees about world Y
        var qa = frame == "local" && GetBool(args, "matchRot", true) ? tb.Rotation : ta.Rotation;
        var yaw = GetFloat(args, "yaw", 0);
        if (yaw != 0)
            qa = Quaternion.Concatenate(qa, Quaternion.CreateFromAxisAngle(Vector3.UnitY, yaw * Deg));
        qa = Quaternion.Normalize(qa);

        // Everything in frame F: B's local axes (origin at B) or the world
        var frameRot = frame == "local" ? tb.Rotation : Quaternion.Identity;
        var inv = Quaternion.Inverse(frameRot);
        var frameOrigin = frame == "local" ? tb.Position : Vector3.Zero;

        Vector3 ToFrame(Vector3 w) => Vector3.Transform(w - frameOrigin, inv);

        var (bMin, bMax) = frame == "local"
            ? Extent(Corners(boxB).Select(c => c * scaleB))
            : Extent(PlacedCorners(boxB, tb.Rotation, scaleB).Select(c => c + tb.Position));

        // A's box relative to its own origin, in frame F
        var (aMin, aMax) = Extent(PlacedCorners(boxA, qa, scaleA).Select(c => Vector3.Transform(c, inv)));
        var aOrigin = ToFrame(ta.Position);

        var cross = GetString(args, "cross", "center").ToLowerInvariant();
        var crossY = GetString(args, "crossY", "min").ToLowerInvariant();
        var o = Vector3.Zero;

        for (var i = 0; i < 3; i++)
        {
            float v;

            if (i == axis)
            {
                v = sign > 0 ? Get(bMax, i) + gap - Get(aMin, i) : Get(bMin, i) - gap - Get(aMax, i);
            }
            else
            {
                var mode = i == 1 ? crossY : cross;
                v = mode switch
                {
                    "keep" => Get(aOrigin, i),
                    "center" => (Get(bMin, i) + Get(bMax, i)) / 2 - (Get(aMin, i) + Get(aMax, i)) / 2,
                    "min" => Get(bMin, i) - Get(aMin, i),
                    "max" => Get(bMax, i) - Get(aMax, i),
                    _ => throw new RemoteException("cross/crossY must be keep, center, min or max")
                };
            }

            o = With(o, i, v);
        }

        var pos = frameOrigin + Vector3.Transform(o, frameRot) + (GetVector(args, "offset") ?? Vector3.Zero);
        var rot = PartRotation(a, qa);
        var oldPos = GetPartVector(a, "Position");
        var oldRot = GetPartVector(a, "Rotation");
        var oldScale = PartScale(a);

        var step = new RemoteStep($"align {a.Name} to {b.Name} {RequireString(args, "side")}", st =>
        {
            st.Run(PartChange(a, pos, rot, null));
            RecordMove(st, a, target.Name, oldPos, oldRot, oldScale);
        });

        ExecuteStep(view, step, false);
        yield return null;

        // Check: the actual distance between the faces in frame F
        var (nMin, nMax) = Extent(PlacedCorners(boxA, a.GetLocalTransform().Rotation, scaleA)
            .Select(c => ToFrame(c + GetPartVector(a, "Position"))));
        var actualGap = sign > 0 ? Get(nMin, axis) - Get(bMax, axis) : Get(bMin, axis) - Get(nMax, axis);

        result["part"] = DescribePart(a);
        result["to"] = b.Name;
        result["frame"] = frame;
        result["gap"] = Round(actualGap);
        result["records"] = RecordsJson(step);
    }

    #endregion

    #region pick

    private static readonly FieldInfo CursorXField =
        typeof(VulkanViewport).GetField("CursorX", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo CursorYField =
        typeof(VulkanViewport).GetField("CursorY", BindingFlags.NonPublic | BindingFlags.Instance);

    private class PickResult
    {
        public bool Done;
        public Entity Entity;
        public Vector3? Point;
        public float Depth;
    }

    /// <summary>
    ///     Smithbox's GPU picking at a viewport pixel: the drawn part (render mesh) and its depth. For this the
    ///     mouse cursor (CursorX/Y) is set to the pixel every frame until the result arrives.
    /// </summary>
    private static IEnumerator GpuPick(MapEditorView view, VulkanViewport viewport, float vx, float vy, PickResult result)
    {
        if (CursorXField == null || CursorYField == null)
            yield break;

        var pipeline = viewport.ViewPipeline;
        var waited = 0;

        while (pipeline.IsPickingPending)
        {
            if (++waited > 60)
                yield break;

            yield return null;
        }

        var cx = (int)viewport.RenderViewport.X + (int)MathF.Floor(vx);
        var cy = (int)viewport.RenderViewport.Y + (int)MathF.Floor(vy);

        void Aim()
        {
            CursorXField.SetValue(viewport, cx);
            CursorYField.SetValue(viewport, cy);
        }

        Aim();
        pipeline.PickingResultsReady = false;
        pipeline.CreateAsyncPickingRequest();

        for (waited = 0; !pipeline.PickingResultsReady; waited++)
        {
            if (waited == 10)
                view.Editor.Project.Handler.FocusEditorWindow(view.Editor);

            if (waited > 120)
                throw new RemoteException("No picking result: is the Map Editor's viewport visible?");

            yield return null;
            Aim();
        }

        var depth = pipeline.PickingDepth;
        var selected = pipeline.GetSelection();
        result.Done = true;
        result.Entity = selected as Entity;
        result.Depth = depth;

        if (depth > 0 && Unproject(viewport, new Vector2(MathF.Floor(vx) + 0.5f, MathF.Floor(vy) + 0.5f), depth, out var p))
            result.Point = p;
    }

    /// <summary>Screen point and depth to a world point (like MapRulerTool.Unproject)</summary>
    private static bool Unproject(VulkanViewport viewport, Vector2 screen, float depth, out Vector3 point)
    {
        point = default;

        if (viewport.Width <= 0 || viewport.Height <= 0)
            return false;

        var x = 2.0f * screen.X / viewport.Width - 1.0f;
        var y = 1.0f - 2.0f * screen.Y / viewport.Height;
        var z = viewport.Device.IsDepthRangeZeroToOne ? depth : depth * 2.0f - 1.0f;
        var viewProjection = viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH * viewport.ViewportCamera.ProjectionMatrix;

        if (!Matrix4x4.Invert(viewProjection, out var inverse))
            return false;

        var world = Vector4.Transform(new Vector4(x, y, z, 1.0f), inverse);

        if (MathF.Abs(world.W) < 1e-12f)
            return false;

        point = new Vector3(world.X, world.Y, world.Z) / world.W;
        return float.IsFinite(point.X) && float.IsFinite(point.Y) && float.IsFinite(point.Z);
    }

    private static JsonObject DescribeEntity(Entity ent)
    {
        if (ent == null)
            return null;

        var obj = new JsonObject
        {
            ["name"] = ent.Name,
            ["type"] = PartTypeName(ent),
            ["map"] = (ent as MsbEntity)?.MapID ?? ent.Container?.Name,
        };

        if (ent.WrappedObject is IMsbPart part)
            obj["model"] = part.ModelName;

        return obj;
    }

    private static IEnumerator Pick(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var viewport = GetViewport(view);
        var x = GetFloat(args, "x", float.NaN);
        var y = GetFloat(args, "y", float.NaN);

        if (float.IsNaN(x) || float.IsNaN(y))
            throw new RemoteException("Missing argument 'x' or 'y' (pixels of the screenshot)");

        var (vx, vy) = ImageToViewport(viewport, x, y);
        var ray = viewport.GetRay(vx, vy);
        var dir = Vector3.Normalize(ray.Direction);

        result["ray"] = new JsonObject { ["origin"] = ToJson(ray.Origin), ["dir"] = ToJson(dir) };

        if (GetBool(args, "render", true))
        {
            var pick = new PickResult();
            var gpu = GpuPick(view, viewport, vx, vy, pick);
            while (gpu.MoveNext())
                yield return null;

            if (pick.Done)
            {
                var render = DescribeEntity(pick.Entity) ?? new JsonObject();
                if (pick.Point is Vector3 p)
                {
                    render["point"] = ToJson(p);
                    render["distance"] = Round(Vector3.Distance(ray.Origin, p));
                }

                result["render"] = render;
            }
        }

        if (GetBool(args, "collision", true))
        {
            var cast = new RayHit();
            var c = CastRay(view, ray.Origin, dir, GetFloat(args, "max", 100000), null, cast);
            while (c.MoveNext())
                yield return null;

            if (cast.Found)
            {
                result["collision"] = new JsonObject
                {
                    ["part"] = cast.Hit.Mesh?.PartName,
                    ["point"] = ToJson(cast.Hit.Point),
                    ["normal"] = ToJson(cast.Hit.Normal),
                    ["distance"] = Round(cast.Hit.Distance),
                    ["material"] = cast.Hit.Material,
                };
            }
        }

        var best = result["render"]?["point"] ?? result["collision"]?["point"];
        result["point"] = best?.DeepClone();
        result["part"] = (result["render"]?["name"] ?? result["collision"]?["part"])?.DeepClone();
    }

    #endregion

    #region Labels

    private static readonly Dictionary<string, RenderFilter> TypeFilters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["MapPiece"] = RenderFilter.MapPiece,
        ["Object"] = RenderFilter.Object,
        ["DummyObject"] = RenderFilter.Object,
        ["Enemy"] = RenderFilter.Character,
        ["DummyEnemy"] = RenderFilter.Character,
        ["Player"] = RenderFilter.Character,
        ["Collision"] = RenderFilter.Collision,
        ["ConnectCollision"] = RenderFilter.Collision,
    };

    private static Rgba32 LabelColor(string type) => type switch
    {
        "MapPiece" => new Rgba32(235, 235, 235),
        "Object" or "DummyObject" => new Rgba32(120, 230, 255),
        "Enemy" or "DummyEnemy" or "Player" => new Rgba32(255, 140, 120),
        _ => new Rgba32(190, 255, 150)
    };

    /// <summary>
    ///     Writes the part names into the image at their projected position (center of the render box). labelTypes,
    ///     labelFilter, labelVisible (omit parts hidden according to collision), labelMax, labelScale; selection in yellow.
    /// </summary>
    private static IEnumerator DrawLabels(MapEditorView view, VulkanViewport viewport, JsonObject args, string path,
        JsonObject result)
    {
        var types = ParseTypes(GetString(args, "labelTypes"), "MapPiece", "Object", "DummyObject", "Enemy", "DummyEnemy",
            "Player");
        var filter = GetString(args, "labelFilter");
        var regex = filter != null ? new Regex(filter, RegexOptions.IgnoreCase) : null;
        var visibleOnly = GetBool(args, "labelVisible", false);
        var max = (int)GetFloat(args, "labelMax", 200);
        var scale = Math.Clamp((int)GetFloat(args, "labelScale", 1), 1, 4);
        var drawFilter = viewport.RenderScene.DrawFilter;
        var selected = new HashSet<Entity>(view.ViewportSelection.GetSelection().OfType<Entity>());
        var eye = viewport.ViewportCamera.CameraTransform.Position;

        // labelVisible: what the render meshes hide (FLVER triangles), see below
        var scene = visibleOnly ? new MeshScene(view) : null;
        var sceneItems = scene?.Items.ToDictionary(i => (Entity)i.Ent) ?? new Dictionary<Entity, MeshItem>();

        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);
        var sx = (float)image.Width / viewport.Width;
        var sy = (float)image.Height / viewport.Height;

        var candidates = new List<(Entity Ent, string Type, Vector3 World, float X, float Y, float Dist)>();

        foreach (var (_, container) in LoadedMaps(view.Editor.Project))
        {
            foreach (var ent in container.Objects.OfType<MsbEntity>())
            {
                if (ent.WrappedObject is not IMsbPart || !ent.EditorVisible)
                    continue;

                var type = PartTypeName(ent);

                if (types != null && !types.Contains(type))
                    continue;

                if (TypeFilters.TryGetValue(type, out var f) && !drawFilter.HasFlag(f))
                    continue;

                if (regex != null && !regex.IsMatch(ent.Name ?? ""))
                    continue;

                var mesh = ent.RenderSceneMesh;
                Vector3 world;

                if (mesh != null && TryLocalBounds(ent, out _))
                {
                    var b = mesh.GetBounds();
                    world = (b.Min + b.Max) / 2;
                }
                else
                {
                    world = ent.GetRootLocalTransform().Position;
                }

                var screen = viewport.WorldToScreen(world, out var inFront);

                if (!inFront || screen.X < 0 || screen.Y < 0 || screen.X >= viewport.Width || screen.Y >= viewport.Height)
                    continue;

                candidates.Add((ent, type, world, screen.X * sx, screen.Y * sy, Vector3.Distance(eye, world)));
            }
        }

        var labels = new JsonArray();
        var used = new List<ViewportLabels.Box>();
        var hiddenCount = 0;

        foreach (var c in candidates.OrderBy(c => selected.Contains(c.Ent) ? 0 : 1).ThenBy(c => c.Dist))
        {
            if (labels.Count >= max)
                break;

            var x = (int)c.X;
            var y = (int)c.Y;

            if (scene != null)
            {
                // Visible if a ray from the eye reaches a sample point of the part's box (middle, then towards the corners)
                // without a render mesh in front, or meets the part itself first; the label goes to that point
                var samples = new List<Vector3> { c.World };
                if (TryLocalBounds(c.Ent, out var lb))
                {
                    var w = c.Ent.GetWorldMatrix();
                    var mid = (lb.Min + lb.Max) / 2;
                    samples.AddRange(Corners(lb).Select(k => Vector3.Transform(mid + (k - mid) * 0.6f, w)));
                    samples.AddRange(new[] { 0, 1, 2 }.SelectMany(axis => new[] { -0.4f, 0.4f }.Select(f =>
                        Vector3.Transform(mid + With(Vector3.Zero, axis, Get(lb.Max - lb.Min, axis) * f), w))));
                }

                // A part with its own triangles counts where a ray meets it first (the label goes to that surface point);
                // one without (figures, unreadable models) where nothing is in front of the sample point
                var item = sceneItems.GetValueOrDefault(c.Ent);
                var solid = item != null && scene.TreeOf(item) != null;
                var reach = TryLocalBounds(c.Ent, out var rb) ? ((rb.Max - rb.Min) * PartScale((MsbEntity)c.Ent)).Length() : 1f;
                Vector3? seen = null;
                foreach (var sp in samples)
                {
                    var d = Vector3.Distance(eye, sp);
                    if (d < 1e-3f)
                        continue;
                    var hitSomething = scene.RayCast(eye, sp - eye, solid ? d + reach : d - 0.05f, null, false, out var mh);
                    if (solid ? hitSomething && mh.Item.Ent == c.Ent : !hitSomething)
                    {
                        seen = solid ? mh.Point : sp;
                        break;
                    }
                }

                if (seen == null)
                {
                    hiddenCount++;
                    continue;
                }

                if (seen.Value != c.World)
                {
                    var sc = viewport.WorldToScreen(seen.Value, out var front);
                    if (front && sc.X >= 0 && sc.Y >= 0 && sc.X < viewport.Width && sc.Y < viewport.Height)
                    {
                        x = (int)(sc.X * sx);
                        y = (int)(sc.Y * sy);
                    }
                }
            }
            var isSelected = selected.Contains(c.Ent);
            var text = c.Ent.Name ?? "?";
            var drawn = ViewportLabels.TryPlace(used, text, x, y, scale, image.Width, image.Height, out var box);
            var color = isSelected ? new Rgba32(255, 220, 40) : LabelColor(c.Type);

            ViewportLabels.Marker(image, x, y, color);

            if (drawn)
            {
                var lx = Math.Clamp(x, box.X, box.X + box.W - 1);
                var ly = Math.Clamp(y, box.Y, box.Y + box.H - 1);
                ViewportLabels.Line(image, x, y, lx, ly, color);
                ViewportLabels.Text(image, box, text, isSelected ? new Rgba32(0, 0, 0) : color,
                    isSelected ? new Rgba32(255, 220, 40) : new Rgba32(0, 0, 0), isSelected ? 0.9f : 0.6f, scale);
            }

            labels.Add(new JsonObject
            {
                ["name"] = text,
                ["type"] = c.Type,
                ["map"] = (c.Ent as MsbEntity)?.MapID,
                ["x"] = x,
                ["y"] = y,
                ["distance"] = Round(c.Dist),
                ["drawn"] = drawn,
                ["selected"] = isSelected,
            });
        }

        image.SaveAsPng(path);
        result["labels"] = labels;
        result["labelsTotal"] = candidates.Count;
        if (scene != null)
            result["labelsHidden"] = hiddenCount;
        yield break;
    }

    #endregion

    #region model-preview

    /// <summary>
    ///     View directions (from the part to the camera) in the part's own axis system; objects face local +Z
    /// </summary>
    private static Vector3 PreviewDirection(string view) => view switch
    {
        "iso" => Vector3.Normalize(new Vector3(0.75f, 0.6f, 1f)),
        "front" => Vector3.Normalize(new Vector3(0, 0.2f, 1f)),
        "back" => Vector3.Normalize(new Vector3(0, 0.2f, -1f)),
        "side" => Vector3.Normalize(new Vector3(1f, 0.2f, 0)),
        "top" => Vector3.Normalize(new Vector3(0, 1f, 0.02f)),
        _ => throw new RemoteException("view must be iso, front, back, side or top")
    };

    /// <summary>
    ///     model-preview: photograph a part (from=map/part, or model=... in map=) alone against an empty background. All
    ///     other objects of the loaded maps are briefly hidden for this (EditorVisible), camera and filters are reset
    ///     afterwards; the map does not change. Several: parts=a,b or models=... with dir=folder.
    /// </summary>
    private static IEnumerator ModelPreview(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var viewport = GetViewport(view);
        var from = GetString(args, "from");
        string sourceMap;
        var wanted = new List<(string Part, string Model)>();

        if (from != null)
        {
            var (m, p) = SplitFrom(from);
            sourceMap = m;
            wanted.Add((p, null));
        }
        else
        {
            sourceMap = RequireString(args, "map");

            foreach (var p in GetString(args, "parts")?.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries) ?? [])
                wanted.Add((p, null));

            foreach (var m in GetString(args, "models", GetString(args, "model"))?.Split(new[] { ',', ' ', ';' },
                         StringSplitOptions.RemoveEmptyEntries) ?? [])
                wanted.Add((null, m));
        }

        if (wanted.Count == 0)
            throw new RemoteException("Give from=<map>/<part>, or map=... with parts=... or models=...");

        var single = wanted.Count == 1 && Has(args, "path");
        var path = single ? GetString(args, "path") : null;
        var dir = single ? null : RequireString(args, "dir");

        if (!Path.IsPathRooted(path ?? dir))
            throw new RemoteException("path/dir must be absolute (as Smithbox sees it)");

        var previewView = GetString(args, "view", "iso").ToLowerInvariant();
        var direction = PreviewDirection(previewView);
        var size = (int)GetFloat(args, "size", 384);
        var margin = GetFloat(args, "margin", 1.1f);

        var loaded = new bool[1];
        var load = EnsureMapLoaded(view, sourceMap, args, loaded);
        while (load.MoveNext())
            yield return null;

        // The viewport's size (and with it the projection) is only right once the Map Editor is in front
        view.Editor.Project.Handler.FocusEditorWindow(view.Editor);
        var settle = WaitFrames(5);
        while (settle.MoveNext())
            yield return null;

        var camera = viewport.ViewportCamera;
        var savedPos = camera.CameraTransform.Position;
        var savedRot = camera.CameraTransform.EulerRotation;
        var savedFilter = viewport.RenderScene.DrawFilter;
        var hidden = new List<Entity>();
        var shots = new JsonArray();

        try
        {
            var source = RequireLoadedMap(view, sourceMap);
            var parts = new List<MsbEntity>();

            foreach (var (partName, model) in wanted)
            {
                var ent = partName != null
                    ? RequirePart(source, partName)
                    : source.Objects.OfType<MsbEntity>().FirstOrDefault(e =>
                          e.WrappedObject is IMsbPart mp && string.Equals(mp.ModelName, model, StringComparison.OrdinalIgnoreCase))
                      ?? throw new RemoteException($"No part with model {model} in {sourceMap}");
                parts.Add(ent);
            }

            // Everything else out of sight; the parts themselves are shown one at a time
            foreach (var (_, container) in LoadedMaps(view.Editor.Project))
            {
                foreach (var ent in container.Objects)
                {
                    if (ent != null && ent.EditorVisible && ent != container.RootObject)
                    {
                        ent.EditorVisible = false;
                        hidden.Add(ent);
                    }
                }
            }

            viewport.RenderScene.DrawFilter = (savedFilter | RenderFilter.MapPiece | RenderFilter.Object | RenderFilter.Character) &
                                              ~RenderFilter.Collision;

            foreach (var ent in parts)
            {
                ent.EditorVisible = true;

                var wait = WaitIdle(GetFloat(args, "timeout", 600), 2);
                while (wait.MoveNext())
                    yield return null;

                if (!TryLocalBounds(ent, out var local))
                    throw new RemoteException($"{ent.Name}: model has no render mesh bounds");

                var t = ent.GetLocalTransform();
                // From the model's own box: the world box of a part can be larger (light radius, submeshes)
                var (pMin, pMax) = Extent(PlacedCorners(local, t.Rotation, t.Scale));
                var center = t.Position + (pMin + pMax) / 2;
                var radius = MathF.Max(0.05f, ((local.Max - local.Min) * t.Scale).Length() / 2);
                // The narrower of the two angles of the projection, as the square is cut from the middle
                if (viewport.Width > 0 && viewport.Height > 0)
                    camera.UpdateProjectionMatrix();
                var proj = camera.ProjectionMatrix;
                var fov = 2 * MathF.Atan(1f / MathF.Max(MathF.Abs(proj.M11), MathF.Abs(proj.M22)));
                var distance = radius * margin / MathF.Sin(fov / 2);
                var d = Vector3.Normalize(Vector3.Transform(direction, t.Rotation));
                var look = -d;

                camera.CameraTransform.Position = center + d * distance;
                camera.CameraTransform.EulerRotation =
                    new Vector3(-MathF.Asin(Math.Clamp(look.Y, -1f, 1f)), MathF.Atan2(look.X, look.Z), 0);

                if (viewport.Width > 0 && viewport.Height > 0)
                    camera.UpdateProjectionMatrix();

                var model = (ent.WrappedObject as IMsbPart)?.ModelName ?? ent.Name;
                var file = path ?? Path.Combine(dir, $"{model}_{ent.Name}.png");
                var size2 = new int[2];
                var capture = CaptureTo(view, viewport, file, args, (int)GetFloat(args, "settle", 4), size2);
                while (capture.MoveNext())
                    yield return null;

                // Square around the middle (the sphere fills the height), scaled to size
                using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(file))
                {
                    var side = Math.Min(image.Width, image.Height);
                    image.Mutate(c => c
                        .Crop(new SixLabors.ImageSharp.Rectangle((image.Width - side) / 2, (image.Height - side) / 2, side, side))
                        .Resize(size, size));
                    image.SaveAsPng(file);
                }

                var extent = local.Max - local.Min;
                shots.Add(new JsonObject
                {
                    ["part"] = ent.Name,
                    ["model"] = model,
                    ["type"] = PartTypeName(ent),
                    ["path"] = file,
                    ["localSize"] = ToJson(extent * PartScale(ent)),
                    ["camera"] = ToJson(camera.CameraTransform.Position),
                    ["distance"] = Round(distance),
                    ["radius"] = Round(radius),
                    ["fov"] = Round(fov / Deg),
                    ["viewport"] = new JsonArray(viewport.Width, viewport.Height),
                });

                ent.EditorVisible = false;
            }
        }
        finally
        {
            foreach (var ent in hidden)
                ent.EditorVisible = true;

            viewport.RenderScene.DrawFilter = savedFilter;
            camera.CameraTransform.Position = savedPos;
            camera.CameraTransform.EulerRotation = savedRot;

            if (viewport.Width > 0 && viewport.Height > 0)
                camera.UpdateProjectionMatrix();

            if (loaded[0] && !GetBool(args, "keepSource", false))
                view.Universe.UnloadMap(sourceMap);
        }

        result["view"] = previewView;
        result["previews"] = shots;
    }

    #endregion
}
