using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json.Nodes;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Commands made of commands, and images: batch (several commands as one undo step, all rolled back on error),
///     shot-compare (difference image, also image/command/image) and flythrough (image sequence along a path as GIF/MP4).
/// </summary>
public static partial class RemoteControl
{
    #region Sub-commands and batch

    /// <summary>Runs another command as part of this one (same arguments as over HTTP)</summary>
    private static IEnumerator RunSub(string cmd, JsonObject args, JsonObject result)
    {
        if (!Commands.ContainsKey(cmd))
            throw new RemoteException($"Unknown command '{cmd}'");

        var steps = StartChecked(cmd, args, result);
        while (steps.MoveNext())
            yield return null;
    }

    // During a batch: the executed steps (instead of individually in the undo list) and their events
    private static List<RemoteStep> BatchSteps;
    private static ActionEvent BatchEvents;

    private static readonly FieldInfo RunningActionField =
        typeof(ViewportActionManager).GetField("RunningAction", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo NotifyHandlersMethod =
        typeof(ViewportActionManager).GetMethod("NotifyHandlers", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>One step in a batch: execute immediately, collect the map IDs (TouchedScopes) like the ActionManager does</summary>
    private static void ExecuteBatched(RemoteStep step)
    {
        var previous = RunningActionField?.GetValue(null);
        RunningActionField?.SetValue(null, step);

        try
        {
            BatchEvents |= step.Execute();
        }
        finally
        {
            RunningActionField?.SetValue(null, previous);
        }

        foreach (var (rec, _) in step.Records)
            if (GetString(rec, "map") is string map)
                step.TouchedScopes.Add(map);

        BatchSteps.Add(step);
    }

    private static readonly HashSet<string> NotInBatch = new() { "batch", "undo", "redo", "map-unload", "project-load", "shot-compare" };

    private static JsonArray GetCommandList(JsonObject args)
    {
        JsonNode node = args["cmds"];

        if (node is JsonValue && GetString(args, "cmds") is string text)
            node = JsonNode.Parse(text);
        else if (node == null && GetString(args, "path") is string path)
            node = JsonNode.Parse(File.ReadAllText(path));

        if (node is JsonObject o && o["cmds"] is JsonArray inner)
            node = inner;

        return node as JsonArray ?? throw new RemoteException("Give cmds=[{\"cmd\":\"part-move\",...},...] or path=<file.json>");
    }

    private static IEnumerator Batch(JsonObject args, JsonObject result)
    {
        var view = GetView();

        if (BatchSteps != null)
            throw new RemoteException("batch inside batch");

        var cmds = GetCommandList(args);
        var stepsOut = new JsonArray();
        var records = new JsonArray();
        Exception failure = null;
        var failedAt = -1;

        BatchSteps = new List<RemoteStep>();
        BatchEvents = ActionEvent.NoEvent;
        var executed = BatchSteps;

        try
        {
            for (var i = 0; i < cmds.Count && failure == null; i++)
            {
                var spec = cmds[i] as JsonObject ?? throw new RemoteException($"cmds[{i}] is not an object");
                var cmd = GetString(spec, "cmd") ?? throw new RemoteException($"cmds[{i}] has no \"cmd\"");

                if (NotInBatch.Contains(cmd))
                {
                    failure = new RemoteException($"{cmd} is not allowed in a batch");
                    failedAt = i;
                    break;
                }

                var subArgs = (JsonObject)spec.DeepClone();
                subArgs.Remove("cmd");
                // Arguments of the batch itself (e.g. map=) count for every step that doesn't give them
                foreach (var (key, value) in args.ToList())
                    if (key is not ("cmds" or "path" or "label") && !subArgs.ContainsKey(key))
                        subArgs[key] = value?.DeepClone();
                var sub = new JsonObject();
                IEnumerator it;

                try
                {
                    it = Commands.ContainsKey(cmd) ? StartChecked(cmd, subArgs, sub) : throw new RemoteException($"Unknown command '{cmd}'");
                }
                catch (Exception e)
                {
                    failure = e;
                    failedAt = i;
                    break;
                }

                while (true)
                {
                    bool more;
                    try
                    {
                        more = it.MoveNext();
                    }
                    catch (Exception e)
                    {
                        failure = e;
                        failedAt = i;
                        break;
                    }

                    if (!more)
                        break;
                    yield return null;
                }

                if (failure != null)
                {
                    stepsOut.Add(new JsonObject { ["cmd"] = cmd, ["ok"] = false, ["error"] = failure is RemoteException ? failure.Message : failure.ToString() });
                    break;
                }

                foreach (var r in (sub["record"] is JsonObject one ? new JsonArray(one.DeepClone()) : new JsonArray())
                         .Concat(sub["records"] as JsonArray ?? new JsonArray()).ToList())
                    records.Add(r?.DeepClone());
                sub.Remove("record");
                sub.Remove("records");
                stepsOut.Add(new JsonObject { ["cmd"] = cmd, ["ok"] = true, ["result"] = sub });
            }
        }
        finally
        {
            BatchSteps = null;
        }

        var manager = view.ViewportActionManager;

        if (failure != null)
        {
            // Everything back, newest first
            var undoEvents = ActionEvent.NoEvent;
            for (var i = executed.Count - 1; i >= 0; i--)
                undoEvents |= executed[i].Undo();
            NotifyHandlersMethod?.Invoke(manager, new object[] { undoEvents | BatchEvents });
            view.ViewportSelection.ClearSelection();
            yield return null;

            result["steps"] = stepsOut;
            result["failedAt"] = failedAt;
            result["rolledBack"] = executed.Count;
            throw new RemoteException($"batch: command {failedAt + 1} ({GetString(cmds[failedAt] as JsonObject ?? new JsonObject(), "cmd")}) " +
                                      $"failed, {executed.Count} step(s) rolled back: " +
                                      (failure is RemoteException ? failure.Message : failure.ToString()));
        }

        if (executed.Count > 0)
        {
            var events = BatchEvents;
            var batch = new RemoteStep(GetString(args, "label") ?? $"batch {cmds.Count} commands", st =>
            {
                foreach (var s in executed)
                    st.Adopt(s);
            });
            batch.PresetEvents(events);
            manager.ExecuteAction(batch);
            view.ViewportSelection.ClearSelection();
        }

        yield return null;

        result["steps"] = stepsOut;
        result["undoSteps"] = executed.Count > 0 ? 1 : 0;
        result["records"] = records;
    }

    #endregion

    #region shot-compare

    private static (int Changed, int Total, int[] Box) DiffImages(string before, string after, string output, int tol)
    {
        using var a = SixLabors.ImageSharp.Image.Load<Rgba32>(before);
        using var b = SixLabors.ImageSharp.Image.Load<Rgba32>(after);

        if (a.Width != b.Width || a.Height != b.Height)
            throw new RemoteException($"The images differ in size: {a.Width}x{a.Height} and {b.Width}x{b.Height}");

        using var d = new Image<Rgba32>(a.Width, a.Height);
        int changed = 0, x0 = int.MaxValue, y0 = int.MaxValue, x1 = -1, y1 = -1;

        for (var y = 0; y < a.Height; y++)
        {
            for (var x = 0; x < a.Width; x++)
            {
                var p = a[x, y];
                var q = b[x, y];
                var diff = Math.Max(Math.Abs(p.R - q.R), Math.Max(Math.Abs(p.G - q.G), Math.Abs(p.B - q.B)));
                var gray = (byte)((q.R * 30 + q.G * 59 + q.B * 11) / 200);

                if (diff > tol)
                {
                    changed++;
                    x0 = Math.Min(x0, x);
                    y0 = Math.Min(y0, y);
                    x1 = Math.Max(x1, x);
                    y1 = Math.Max(y1, y);
                    var k = Math.Min(1f, 0.45f + diff / 255f);
                    d[x, y] = new Rgba32((byte)(gray + (255 - gray) * k), (byte)(gray * (1 - k)), (byte)(gray * (1 - k)), 255);
                }
                else
                {
                    d[x, y] = new Rgba32(gray, gray, gray, 255);
                }
            }
        }

        if (changed > 0)
        {
            var yellow = new Rgba32(255, 220, 40);
            ViewportLabels.Line(d, x0 - 2, y0 - 2, x1 + 2, y0 - 2, yellow);
            ViewportLabels.Line(d, x0 - 2, y1 + 2, x1 + 2, y1 + 2, yellow);
            ViewportLabels.Line(d, x0 - 2, y0 - 2, x0 - 2, y1 + 2, yellow);
            ViewportLabels.Line(d, x1 + 2, y0 - 2, x1 + 2, y1 + 2, yellow);
        }

        d.SaveAsPng(output);
        return (changed, a.Width * a.Height, changed > 0 ? new[] { x0, y0, x1, y1 } : null);
    }

    private static IEnumerator ShotCompare(JsonObject args, JsonObject result)
    {
        var tol = (int)GetFloat(args, "tol", 24);
        string before, after, output;

        if (Has(args, "cmd") || Has(args, "cmds"))
        {
            // Picture, command, picture at the same camera
            var view = GetView();
            var viewport = GetViewport(view);
            var prefix = RequireString(args, "path");
            if (prefix.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                prefix = prefix[..^4];
            before = prefix + "_before.png";
            after = prefix + "_after.png";
            output = prefix + "_diff.png";
            Directory.CreateDirectory(Path.GetDirectoryName(prefix)!);
            var settle = (int)GetFloat(args, "settle", 8);

            var shot = CaptureTo(view, viewport, before, args, settle, new int[2]);
            while (shot.MoveNext())
                yield return null;

            var manager = view.ViewportActionManager;
            var top = manager.PeekUndoAction();
            var sub = new JsonObject();
            JsonObject subArgs;
            string cmd;

            if (Has(args, "cmds"))
            {
                cmd = "batch";
                subArgs = new JsonObject { ["cmds"] = GetCommandList(args).DeepClone() };
            }
            else
            {
                subArgs = args["cmd"] as JsonObject ?? JsonNode.Parse(GetString(args, "cmd")) as JsonObject
                          ?? throw new RemoteException("cmd must be a JSON object {\"cmd\":\"part-move\",...}");
                subArgs = (JsonObject)subArgs.DeepClone();
                cmd = GetString(subArgs, "cmd") ?? throw new RemoteException("cmd needs \"cmd\"");
                subArgs.Remove("cmd");
            }

            if (NotInBatch.Contains(cmd) && cmd != "batch")
                throw new RemoteException($"{cmd} can't be compared");

            var run = RunSub(cmd, subArgs, sub);
            while (run.MoveNext())
                yield return null;

            shot = CaptureTo(view, viewport, after, args, settle, new int[2]);
            while (shot.MoveNext())
                yield return null;

            var changedHistory = manager.PeekUndoAction() != top;
            if (GetBool(args, "undo", false) && changedHistory)
            {
                var undoResult = new JsonObject();
                result["undo"] = undoResult;
                var undo = RunSub("undo", new JsonObject(), undoResult);
                while (undo.MoveNext())
                    yield return null;
            }
            else
            {
                var recs = new JsonArray();
                if (sub["record"] is JsonObject one)
                    recs.Add(one.DeepClone());
                foreach (var r in sub["records"] as JsonArray ?? new JsonArray())
                    recs.Add(r?.DeepClone());
                result["records"] = recs;
            }

            sub.Remove("record");
            sub.Remove("records");
            result["command"] = cmd;
            result["commandResult"] = sub;
            result["undone"] = GetBool(args, "undo", false) && changedHistory;
        }
        else
        {
            before = RequireString(args, "before");
            after = RequireString(args, "after");
            output = RequireString(args, "out");
        }

        var (changed, total, box) = DiffImages(before, after, output, tol);
        result["before"] = before;
        result["after"] = after;
        result["diff"] = output;
        result["changed"] = changed;
        result["percent"] = Math.Round(100.0 * changed / Math.Max(1, total), 3);
        result["box"] = box != null ? new JsonArray(box.Select(v => (JsonNode)v).ToArray()) : null;
        result["tol"] = tol;
    }

    #endregion

    #region flythrough

    /// <summary>Navmesh faces (file as for leak-scan) as a path: centers at least spacing apart, chained by nearest neighbour</summary>
    private static List<Vector3> FacePath(string facesPath, float spacing)
    {
        var centers = new List<Vector3>();
        foreach (var line in File.ReadLines(facesPath))
        {
            var a = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (a.Length < 10)
                continue;
            var v = a.Skip(1).Select(s => float.Parse(s, CultureInfo.InvariantCulture)).ToArray();
            var n = v.Length / 3;
            var c = Vector3.Zero;
            for (var i = 0; i < n; i++)
                c += new Vector3(v[3 * i], v[3 * i + 1], v[3 * i + 2]);
            c /= n;
            if (centers.All(p => Vector3.Distance(p, c) >= spacing))
                centers.Add(c);
        }

        if (centers.Count == 0)
            return centers;

        var chain = new List<Vector3> { centers[0] };
        centers.RemoveAt(0);
        while (centers.Count > 0)
        {
            var last = chain[^1];
            var k = 0;
            for (var i = 1; i < centers.Count; i++)
                if (Vector3.DistanceSquared(centers[i], last) < Vector3.DistanceSquared(centers[k], last))
                    k = i;
            chain.Add(centers[k]);
            centers.RemoveAt(k);
        }

        return chain;
    }

    private static IEnumerator Flythrough(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var viewport = GetViewport(view);
        var output = RequireString(args, "out");
        var stepLen = MathF.Max(0.05f, GetFloat(args, "step", 0.5f));
        var ahead = GetFloat(args, "ahead", 2f);
        var lookat = GetVector(args, "lookat");
        var frames = new List<(Vector3 Pos, Vector3 Dir)>();

        if (GetString(args, "bookmarks") is string names)
        {
            var list = ReadBookmarks(BookmarkFile(args));
            var keys = names.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(n => FindBookmark(list, n.Trim()))
                .Select(b => (Pos: GetVector(b, "pos") ?? Vector3.Zero, Dir: GetVector(b, "forward") ?? Vector3.UnitZ)).ToList();
            if (keys.Count < 2)
                throw new RemoteException("bookmarks= needs at least two names");

            var turn = (int)GetFloat(args, "turnFrames", 8);
            for (var i = 0; i + 1 < keys.Count; i++)
            {
                var (p0, d0) = keys[i];
                var (p1, d1) = keys[i + 1];
                var angle = MathF.Acos(Math.Clamp(Vector3.Dot(Vector3.Normalize(d0), Vector3.Normalize(d1)), -1, 1)) / Deg;
                var n = Math.Max((int)MathF.Ceiling(Vector3.Distance(p0, p1) / stepLen), angle > 5 ? turn : 1);
                for (var k = 0; k < n; k++)
                {
                    var f = (float)k / n;
                    var d = Vector3.Lerp(Vector3.Normalize(d0), Vector3.Normalize(d1), f);
                    frames.Add((Vector3.Lerp(p0, p1, f), d.LengthSquared() > 1e-6f ? d : d0));
                }
            }

            frames.Add(keys[^1]);
        }
        else
        {
            List<Vector3> pts;
            float eye;
            if (GetString(args, "faces") is string faces)
            {
                pts = FacePath(faces, GetFloat(args, "spacing", 2));
                eye = GetFloat(args, "eye", 1.6f);
            }
            else
            {
                pts = GetPoints(args, "points") ?? throw new RemoteException("Give points=[[x,y,z],...], bookmarks=a,b,... or faces=<file>");
                eye = GetFloat(args, "eye", 0);
            }

            pts = pts.Select(p => p + new Vector3(0, eye, 0)).ToList();
            if (pts.Count < 2)
                throw new RemoteException("The path needs at least two points");

            // Resampled every step along the path, looking ahead along it
            var cum = new List<float> { 0 };
            for (var i = 1; i < pts.Count; i++)
                cum.Add(cum[^1] + Vector3.Distance(pts[i - 1], pts[i]));
            var total = cum[^1];

            Vector3 At(float s)
            {
                s = Math.Clamp(s, 0, total);
                var i = 1;
                while (i < pts.Count - 1 && cum[i] < s)
                    i++;
                var len = cum[i] - cum[i - 1];
                return len < 1e-6f ? pts[i] : Vector3.Lerp(pts[i - 1], pts[i], (s - cum[i - 1]) / len);
            }

            var lastDir = Vector3.Normalize(pts[1] - pts[0]);
            for (var s = 0f; s <= total + 1e-3f; s += stepLen)
            {
                var p = At(s);
                var d = lookat is Vector3 la ? la - p : At(s + ahead) - p;
                if (d.LengthSquared() < 1e-4f)
                    d = lastDir;
                lastDir = Vector3.Normalize(d);
                frames.Add((p, lastDir));
            }
        }

        var maxFrames = (int)GetFloat(args, "maxFrames", 300);
        if (frames.Count > maxFrames)
            throw new RemoteException($"{frames.Count} frames, more than maxFrames={maxFrames}: raise step= or maxFrames=");

        var dir = GetString(args, "dir") ?? Path.Combine(Path.GetDirectoryName(output)!, Path.GetFileNameWithoutExtension(output) + "_frames");
        Directory.CreateDirectory(dir);
        foreach (var old in Directory.GetFiles(dir, "frame_*.png"))
            File.Delete(old);

        view.Editor.Project.Handler.FocusEditorWindow(view.Editor);
        var focus = WaitFrames(5);
        while (focus.MoveNext())
            yield return null;

        var camera = viewport.ViewportCamera;
        var savedPos = camera.CameraTransform.Position;
        var savedRot = camera.CameraTransform.EulerRotation;
        var savedFilter = viewport.RenderScene.DrawFilter;
        var saved = new List<(string, object)>();
        if (Has(args, "renderMode"))
            saved.Add(("Viewport_Render_Mode", SetConfig("Viewport_Render_Mode", (int)GetFloat(args, "renderMode", 0))));
        if (Has(args, "fov"))
            saved.Add(("Viewport_Camera_FOV", SetConfig("Viewport_Camera_FOV", GetFloat(args, "fov", 60))));
        if (GetBool(args, "clean", true))
            viewport.RenderScene.DrawFilter &= ~ParseFilters("Collision,Region,Light,Debug,Navmesh");

        var files = new List<string>();
        var settle = (int)GetFloat(args, "settle", 3);
        var watch = Stopwatch.StartNew();

        try
        {
            for (var i = 0; i < frames.Count; i++)
            {
                SetCamera(viewport, frames[i].Pos, frames[i].Dir);
                var file = Path.Combine(dir, $"frame_{i:D4}.png");
                var capture = CaptureTo(view, viewport, file, args, settle, new int[2]);
                while (capture.MoveNext())
                    yield return null;
                files.Add(file);
            }
        }
        finally
        {
            Restore(saved);
            viewport.RenderScene.DrawFilter = savedFilter;
            camera.CameraTransform.Position = savedPos;
            camera.CameraTransform.EulerRotation = savedRot;
            if (viewport.Width > 0 && viewport.Height > 0)
                camera.UpdateProjectionMatrix();
        }

        var fps = GetFloat(args, "fps", 8);
        var width = (int)GetFloat(args, "width", 480);
        result["frames"] = files.Count;
        result["framesDir"] = dir;
        result["seconds"] = Math.Round(watch.Elapsed.TotalSeconds, 1);

        if (output.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
        {
            // ffmpeg, if there is one here; else the client may run its own on the frames
            Process proc = null;
            try
            {
                var psi = new ProcessStartInfo("ffmpeg") { UseShellExecute = false, RedirectStandardError = true, RedirectStandardOutput = true };
                foreach (var a in new[] { "-y", "-loglevel", "error", "-framerate", F(fps), "-i", Path.Combine(dir, "frame_%04d.png"),
                             "-vf", $"scale={width}:-2", "-pix_fmt", "yuv420p", output })
                    psi.ArgumentList.Add(a);
                proc = Process.Start(psi);
            }
            catch (Exception)
            {
                proc = null;
            }

            if (proc != null)
            {
                var stderr = proc.StandardError.ReadToEndAsync();
                while (!proc.HasExited)
                    yield return null;
                if (proc.ExitCode != 0)
                    throw new RemoteException("ffmpeg: " + stderr.Result);
                result["out"] = output;
                result["encoder"] = "ffmpeg";
            }
            else
            {
                var gif = Path.ChangeExtension(output, ".gif");
                EncodeGif(files, gif, fps, width);
                result["out"] = gif;
                result["encoder"] = "ImageSharp (GIF, no ffmpeg here)";
                result["needsFfmpeg"] = true;
                result["mp4"] = output;
                result["framePattern"] = Path.Combine(dir, "frame_%04d.png");
                result["fps"] = fps;
                result["width"] = width;
            }
        }
        else
        {
            EncodeGif(files, output, fps, width);
            result["out"] = output;
            result["encoder"] = "ImageSharp";
        }
    }

    private static void EncodeGif(List<string> files, string output, float fps, int width)
    {
        var delay = Math.Max(2, (int)MathF.Round(100 / MathF.Max(fps, 0.5f)));
        using var gif = SixLabors.ImageSharp.Image.Load<Rgba32>(files[0]);
        var height = (int)MathF.Round((float)gif.Height * width / gif.Width);
        gif.Mutate(c => c.Resize(width, height));
        gif.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = delay;
        gif.Metadata.GetGifMetadata().RepeatCount = 0;

        foreach (var f in files.Skip(1))
        {
            using var img = SixLabors.ImageSharp.Image.Load<Rgba32>(f);
            img.Mutate(c => c.Resize(width, height));
            var frame = gif.Frames.AddFrame(img.Frames.RootFrame);
            frame.Metadata.GetGifMetadata().FrameDelay = delay;
        }

        gif.SaveAsGif(output, new GifEncoder { ColorTableMode = SixLabors.ImageSharp.Formats.FrameColorTableMode.Local });
    }

    #endregion
}
