using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Logger;
using SoulsFormats;
using StudioCore.Renderer;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     A small HTTP server on 127.0.0.1 that lets scripts drive the Map Editor: load a project and maps, place the
///     camera, change settings and save screenshots of the viewport. Off unless a port is given, with
///     <c>--remote-port N</c> on the command line or the SMITHBOX_REMOTE_PORT environment variable.
///     <para>
///     Requests are <c>GET</c> or <c>POST /&lt;command&gt;</c>, arguments as query parameters or a JSON object body;
///     the answer is a JSON object with <c>ok</c>. Commands run one at a time on the main thread, as coroutines that
///     can wait for frames (see <see cref="Tick" />). The list is in <see cref="Commands" />.
///     </para>
/// </summary>
public static partial class RemoteControl
{
    // Present only in Smithbox builds with the extra Bloodborne renderer changes; looked up at run time
    private static readonly PropertyInfo EnvCubeNameProperty = typeof(SceneRenderPipeline).GetProperty("MapEnvCubeName");

    private class RemoteException : Exception
    {
        public RemoteException(string message) : base(message) { }
    }

    private class RemoteJob
    {
        public string Command;
        public JsonObject Args;
        public JsonObject Result = new();
        public IEnumerator Steps;
        public TaskCompletionSource<JsonObject> Done = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public long LogStart;
    }

    private delegate IEnumerator CommandHandler(JsonObject args, JsonObject result);

    // Every command with its arguments and result on stdout (in a container: docker logs); SMITHBOX_REMOTE_LOG=0 turns it off
    private static long CommandCount;
    private static readonly bool LogCommands = Environment.GetEnvironmentVariable("SMITHBOX_REMOTE_LOG") != "0";

    private static void LogCommand(string text)
    {
        if (LogCommands)
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} remote {text}");
    }

    private static string Shorten(string text, int max = 300) =>
        text.Length <= max ? text.ReplaceLineEndings(" ") : text[..max].ReplaceLineEndings(" ") + $" ... ({text.Length} chars)";

    private static readonly Dictionary<string, CommandHandler> Commands = new()
    {
        ["help"] = Help,
        ["status"] = Status,
        ["projects"] = Projects,
        ["project-load"] = ProjectLoad,
        ["maps"] = Maps,
        ["map-load"] = MapLoad,
        ["map-unload"] = MapUnload,
        ["wait"] = Wait,
        ["camera"] = Camera,
        ["select"] = Select,
        ["frame"] = Frame,
        ["entities"] = Entities,
        ["cfg"] = Config,
        ["cfg-list"] = ConfigList,
        ["window"] = Window,
        ["command"] = EditorCommand,
        ["screenshot"] = Screenshot,
        ["log"] = Log,
        ["filter"] = Filter,
        ["part-list"] = PartList,
        ["part-add"] = PartAdd,
        ["part-move"] = PartMove,
        ["part-delete"] = PartDelete,
        ["floor"] = Floor,
        ["ray"] = Ray,
        ["bbox"] = Bbox,
        ["map-save"] = MapSave,
        ["group-copy"] = GroupCopy,
        ["place-on"] = PlaceOn,
        ["align"] = Align,
        ["undo"] = Undo,
        ["redo"] = Redo,
        ["pick"] = Pick,
        ["model-preview"] = ModelPreview,
        ["set"] = SetFieldsCommand,
        ["region-add"] = RegionAdd,
        ["region-move"] = RegionMove,
        ["region-delete"] = RegionDelete,
        ["ops-mark"] = OpsMark,
        ["ops-export"] = OpsExport,
        ["ops-apply"] = OpsApply,
        ["leak-scan"] = LeakScan,
        ["plan"] = Plan,
        ["section"] = Plan,
        ["diff"] = Diff,
        ["overlap"] = Overlap,
        ["free-spot"] = FreeSpot,
        ["los"] = Los,
        ["lightmap-check"] = LightmapCheck,
        ["texture-check"] = TextureCheck,
        ["find-model"] = FindModel,
        ["model-search"] = ModelSearch,
        ["region-new"] = RegionNew,
        ["region-list"] = RegionList,
        ["event-new"] = EventNew,
        ["event-set"] = EventSet,
        ["event-list"] = EventList,
        ["scatter"] = Scatter,
        ["capsule-path"] = CapsulePath,
        ["profile"] = Profile,
        ["bookmark"] = Bookmark,
        ["flythrough"] = Flythrough,
        ["batch"] = Batch,
        ["shot-compare"] = ShotCompare,
        ["inspect"] = Inspect,
        ["lm-preview"] = LmPreview,
    };

    // Short help for /help (help cmd=<command>)
    private static readonly Dictionary<string, string> Usage = new()
    {
        ["status"] = "project, loaded maps, busy, camera, window size",
        ["project-load"] = "name=<project|GUID> [timeout=900]",
        ["map-load"] = "map=<id> [wait=true timeout=600]",
        ["map-unload"] = "map=<id> | all=true [force=true]",
        ["camera"] = "[pos=x,y,z] [lookat=x,y,z | dir=x,y,z | rot=p,y,r] [fov=] [mode=Perspective|Orthographic] [orthoSize=]",
        ["screenshot"] = "path=<absolute> [settle=10] [labels=true labelTypes=MapPiece,Object labelFilter=<regex> " +
                         "labelVisible=true labelMax=200 labelScale=1]: viewport image, with labels=true part names in the image " +
                         "(labelVisible: only those visible per render mesh) " +
                         "(selection yellow) and list labels[] with pixel positions",
        ["part-list"] = "[map=] [filter=<regex>]",
        ["part-add"] = "from=<map>/<part> [name=new] [pos=x,y,z] [rot=x,y,z] [snap=true] [map=target] [keepEntityId] [addAtlas] " +
                       "[keepSource]: copy (one undo step)",
        ["part-move"] = "name=<part> [pos=x,y,z] [rot=x,y,z] [dpos=dx,dy,dz] [drot=dx,dy,dz] [local=true] [scale=s|x,y,z] " +
                        "[snap=true]: absolute and/or relative; local=true: dpos/drot in the part's axes",
        ["part-delete"] = "name=<part> [map=]",
        ["group-copy"] = "from=<sourcemap> (parts=a,b,... | filter=<regex> | min=x,y,z max=x,y,z) [type=MapPiece,Object|all] " +
                         "[anchor=x,y,z] [to=x,y,z] [snap=true] [yaw=deg | rot=x,y,z] [suffix=_a | number=9100] [map=target] " +
                         "[dry=true] [limit=300] [keepSource]: copy an arrangement with relative placement, rotated about the anchor",
        ["place-on"] = "(from=<map>/<part> [name=new] | name=<existing part>) [origin=x,y,z dir=x,y,z | at=px,py] " +
                       "[orient=auto|up|upright|keep] [axis=+z] [spin=deg] [lean=deg] [gap=0] [offset=x,y,z] " +
                       "[contact=bbox|origin] [floor=true lift=0] [max=200] [hit=collision|mesh]: place at the ray hit " +
                       "(collision or render mesh of the target map), " +
                       "without a ray given, from the camera center",
        ["align"] = "name=<A> to=<B> side=+x|-x|+y|-y|+z|-z [gap=0] [frame=local|world] [matchRot=true] [yaw=deg] " +
                    "[cross=center|min|max|keep] [crossY=min] [offset=x,y,z]: A flush against the side of B",
        ["undo"] = "[steps=1]: undo the last Smithbox action(s); unrecords[] for the client's journal",
        ["redo"] = "[steps=1]",
        ["pick"] = "x=<px> y=<px> [render=true collision=true]: part and world point under an image pixel " +
                   "(render mesh via GPU picking, plus the collision hit)",
        ["model-preview"] = "from=<map>/<part> path=<png> | map=<map> (parts=a,b | models=m1,m2) dir=<folder> " +
                            "[view=iso|front|back|side|top] [size=384] [margin=1.1] [settle=4] [keepSource]: " +
                            "photograph a part alone (everything else briefly hidden)",
        ["floor"] = "x= z= [y=1000] [all=true]",
        ["ray"] = "from=x,y,z dir=x,y,z [max=100] [all=true]",
        ["bbox"] = "name=<part> [map=]",
        ["map-save"] = "[map=]",
        ["set"] = "name=<entry> [list=Parts|Regions|Events] (fields={\"EntityID\":123,\"Position.Y\":-19.5,\"DrawGroups[0]\":1} | " +
                  "field=<path> value=<value>) [map=]: set any fields of a part, region or event",
        ["region-add"] = "from=<map>/<region> [name=new] [pos=x,y,z] [rot=x,y,z] [snap=true] [map=target]: copy a region",
        ["region-move"] = "name=<region> [pos=] [rot=] [dpos=] [drot=] [local=true] [snap=true]",
        ["region-delete"] = "name=<region> [map=]",
        ["ops-mark"] = "[map=]: set a mark (in-memory state) for ops-export since=mark",
        ["ops-export"] = "[map=] [since=disk|mark] [path=<file>] [wsl=true]: changes as msbtool ops (delete/copy/set), " +
                         "relative to: the MSB on disk (project, else game) or the mark",
        ["ops-apply"] = "path=<edits.json> | ops=[...] [map=] [keepSource]: apply msbtool ops to the loaded map (one undo step)",
        ["leak-scan"] = "(faces=<navmesh_faces.txt> [spacing=4 eye=1.6] | points=[{pos,dir|lookat},...]) dir=<folder> " +
                        "[threshold=30] [settle=4] [fov=90] [keep=false] [maxViews=400] [maxNoFloor=0.2 abortNoFloor=0.8 force=true]: gap-mode views (magenta background reveals holes), magenta pixels per view, " +
                        "images kept only above the threshold",
        ["plan"] = "min=x,z max=x,z cut=<height> path=<png> [depth=30] [grid=1] [bar=5] [gap=true] [labels=true] [collision=true]: " +
                   "top view, everything above cut removed, with grid and scale bar (also: section)",
        ["section"] = "same as plan",
        ["diff"] = "ref=<reference.msb.dcx> [map=] [faces=<navmesh_faces.txt> out=<faces.txt> radius=6]: changed parts against " +
                   "a reference, check points, faces nearby for leak-scan",
        ["overlap"] = "min=x,y,z max=x,y,z [maxdist=0.03] [type=MapPiece] [only=m1,m2] [skip=...] [amin=0] [maxSize=4] " +
                      "[minDepth=0.05] [limit=50]: coplanar faces (FLVER triangles) and interpenetrating parts",
        ["free-spot"] = "min=x,y,z max=x,y,z (size=x,y,z | from=<map>/<part>) [step=0.5] [clearance=0.1] [wall=true] " +
                        "[yaws=0,90] [limit=20]: find a free, level spot",
        ["los"] = "from=x,y,z (to=x,y,z | part=<name>): line of sight against collision (free), plus the render meshes it crosses " +
                  "(boxes, freeRender; hulls it runs inside don't count)",
        ["lightmap-check"] = "[map=] [filter=<regex>]: MapPieces without a lightmap in game (name from map code + model + BTAB atlas)",
        ["texture-check"] = "[map=] [type=MapPiece,Object] [filter=<regex>]: models whose textures Smithbox could not load",
        ["find-model"] = "model=<name|regex> [maps=<regex>] [limit=20]: where a model is used across all maps of the project, size, textures",
        ["model-search"] = "[name=<regex>] [maps=<regex>] [type=MapPiece,Object] [texture=<regex>] [minSize= maxSize=] [sizes=true] " +
                           "[limit=100] [maxRead=400]: search models",
        ["region-new"] = "name=<new> pos=x,y,z [shape=point|circle|sphere|cylinder|rect|box] [radius= width= height= depth= | size=w,h,d] " +
                         "[rot=x,y,z | yaw=] [entityId=] [snap=true] [type=Sound|SFX|WindSFX|SpawnPoint|Message|... eventName= " +
                         "eventFields={...}] [map=]: create a region; BB regions have only one shape, type=<event type> also creates the " +
                         "event with RegionName (one undo step)",
        ["region-list"] = "[map=] [filter=<regex>] [shape=<regex>] [near=x,y,z radius=10] [limit=200]: regions with shape, size, " +
                          "EntityID and the entries referencing them",
        ["event-new"] = "type=PatrolInfo|Treasure|ObjAct|Sound|SFX|Generator|... name=<new> [part=] [region=] [entityId=] [eventId=] " +
                        "[points=r1,r2,... patrolType= enemy=e1,e2] [itemLots=a,b,c inChest=] [paramId= flag= state= objActEntityId=] " +
                        "[soundType= soundId=] [effectId=] [fields={...}] [map=]: create an MSB event, references checked (one undo step)",
        ["event-set"] = "name=<event> (as event-new: part=, points=, itemLots=, enemy=, ..., fields={...}) [map=]: change an event; " +
                        "points= replaces the whole route",
        ["event-list"] = "[map=] [type=<regex>] [filter=<regex>] [refers=<part|region>] [limit=200]: events with their fields, missing references",
        ["scatter"] = "palette=<map>/<part>[*weight],... min=x,y,z max=x,y,z count=N [seed=1] [yaw=0 yawSpread=360] [tilt=0] " +
                      "[scaleMin=1 scaleMax=1] [minDist=] [clearance=0.05] [wall=true axis=+z wallGap=0.02 wallRange=1.5] [avoid=true] " +
                      "[number=N | suffix=] [dry=true path=<png>] [maxTries=] [map=]: scatter irregularly (no grid), floor snap, " +
                      "one undo step; dry=true with path= shows the spots in an image",
        ["capsule-path"] = "from=x,y,z (to=x,y,z | toMin=x,y,z toMax=x,y,z) [radius=0.4 height=1.8] [stepHeight=0.45 slope=50] [step=0.25] " +
                           "[narrow=0.15] [drop=0] [pad=8 | min= max=] [simulate=true] [path=<png> cut= margin=3]: path for a capsule " +
                           "against the collision (A*), narrow spots, getting stuck (Smithbox walk controller), top view",
        ["profile"] = "(points=[[x,y,z],...] | from=x,y,z to=x,y,z) [step=1] [heights=0.5,1.5] [probe=1] [max=30] [path=<png> minWidth= minHead=]: " +
                      "corridor profile per station: floor, left/right, width, head room, slope; table and chart",
        ["bookmark"] = "save|go|list|delete [name=] [note=] [by=] [load=true] [file=<json>]: named camera points, file " +
                       "<project>/.smithbox/Remote/bookmarks.json",
        ["flythrough"] = "(points=[[x,y,z],...] [eye=0] | bookmarks=a,b,... [turnFrames=8] | faces=<file> [spacing=2 eye=1.6]) out=<gif|mp4> " +
                         "[step=0.5] [ahead=2] [lookat=x,y,z] [fps=8] [width=480] [renderMode=0|1] [fov=] [clean=true] [settle=3] [dir=<folder>] " +
                         "[maxFrames=300]: image sequence along a path",
        ["batch"] = "cmds=[{\"cmd\":\"part-move\",...},...] | path=<json> [label=]: commands as one undo step, all rolled back on error, " +
                    "result per step",
        ["shot-compare"] = "before=<png> after=<png> out=<png> | path=<prefix> (cmd={\"cmd\":...} | cmds=[...]) [undo=true] [settle=8]; [tol=24]: " +
                           "difference image and count of changed pixels, also automatically: image, command, image",
        ["inspect"] = "name=<part> [map=]: model, materials, textures (missing), lightmap/BTAB, collision in the part, box, groups, references",
        ["lm-preview"] = "model=m1,m2 factor=0.35 | clear=true: change a model's lightmap light in the view only (map unchanged)",
    };

    private static readonly ConcurrentQueue<RemoteJob> Queue = new();
    private static RemoteJob Current;
    private static TcpListener Listener;

    // The log, kept for /log and for the errors of a failed command (Smithbox writes no log file)
    private record LogLine(long Index, LogEvent Event);
    private static LogSubscription LogFeed;
    private static readonly List<LogLine> LogLines = new();
    private static long LogCount;
    private const int LogCapacity = 1000;

    public static int Port { get; private set; }

    public static bool IsRunning => Listener != null;

    /// <summary>For the window title: the port and the instance's role (--remote-role), empty without the server</summary>
    public static string TitleSuffix
    {
        get
        {
            if (!IsRunning)
                return "";

            var role = GetArg("--remote-role");
            return string.IsNullOrEmpty(role) ? $" [Remote {Port}]" : $" [Remote {Port} {role}]";
        }
    }

    /// <summary>
    ///     Starts the server if a port was given. Called once the editor has been set up.
    /// </summary>
    public static void Start()
    {
        var port = GetConfiguredPort();

        if (port <= 0)
            return;

        try
        {
            Listener = new TcpListener(IPAddress.Loopback, port);
            Listener.Start();
            Port = port;

            LogFeed = TaskLogs.Subscribe(_ => true);

            _ = Task.Run(AcceptLoop);

            Smithbox.Log(typeof(RemoteControl), $"Remote control listening on http://127.0.0.1:{port}/");
        }
        catch (Exception e)
        {
            Listener = null;
            Smithbox.LogError(typeof(RemoteControl), $"Remote control could not listen on port {port}.", e);
        }
    }

    public static void Stop()
    {
        try
        {
            Listener?.Stop();
        }
        catch
        {
            // Closing anyway
        }

        Listener = null;

        if (LogFeed != null)
        {
            TaskLogs.Unsubscribe(LogFeed);
            LogFeed = null;
        }
    }

    private static void DrainLog()
    {
        if (LogFeed == null)
            return;

        while (LogFeed.TryDequeue(out var evt))
        {
            LogLines.Add(new LogLine(++LogCount, evt));
        }

        if (LogLines.Count > LogCapacity)
            LogLines.RemoveRange(0, LogLines.Count - LogCapacity);
    }

    private static JsonObject DescribeLog(LogLine line, bool withException)
    {
        var evt = line.Event;
        var category = evt.Category ?? "";

        var entry = new JsonObject
        {
            ["i"] = line.Index,
            ["time"] = evt.Time.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture),
            ["level"] = evt.Level.ToString(),
            ["category"] = category[(category.LastIndexOf('.') + 1)..],
            ["message"] = evt.Message,
        };

        if (withException && evt.Exception != null)
        {
            var text = evt.Exception.ToString();
            entry["exception"] = text.Length > 6000 ? text[..6000] + " ..." : text;
        }

        return entry;
    }

    private static string GetArg(string name)
    {
        var args = Environment.GetCommandLineArgs();

        for (var i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }

        return null;
    }

    private static int GetConfiguredPort()
    {
        if (int.TryParse(GetArg("--remote-port"), out var argPort))
            return argPort;

        var env = Environment.GetEnvironmentVariable("SMITHBOX_REMOTE_PORT");

        return int.TryParse(env, out var envPort) ? envPort : 0;
    }

    /// <summary>
    ///     Runs the current command's next step. Called once per frame on the main thread, after the update and
    ///     before the draw, so a step that asks for a screenshot sees it drawn by the time of the next one.
    /// </summary>
    public static void Tick()
    {
        if (Listener == null)
            return;

        DrainLog();

        if (Current == null)
        {
            if (!Queue.TryDequeue(out Current))
                return;

            Current.LogStart = LogCount;
        }

        var job = Current;

        try
        {
            job.Steps ??= StartChecked(job.Command, job.Args, job.Result);

            if (job.Steps.MoveNext())
                return;

            job.Result["ok"] = true;
            job.Done.TrySetResult(job.Result);
        }
        catch (Exception e)
        {
            var error = new JsonObject
            {
                ["ok"] = false,
                ["error"] = e is RemoteException ? e.Message : e.ToString()
            };

            // What the command found out before it failed (e.g. the steps of a batch)
            foreach (var (key, value) in job.Result.ToList())
                if (key is not ("ok" or "error" or "log"))
                    error[key] = value?.DeepClone();

            // What went wrong is usually in the warnings and errors logged meanwhile
            DrainLog();
            var problems = LogLines
                .Where(l => l.Index > job.LogStart && l.Event.Level >= Microsoft.Extensions.Logging.LogLevel.Warning)
                .TakeLast(20)
                .Select(l => (JsonNode)DescribeLog(l, true))
                .ToArray();

            if (problems.Length > 0)
                error["log"] = new JsonArray(problems);

            job.Done.TrySetResult(error);
        }

        Current = null;
    }

    #region HTTP

    private static async Task AcceptLoop()
    {
        while (Listener != null)
        {
            TcpClient client;

            try
            {
                client = await Listener.AcceptTcpClientAsync();
            }
            catch
            {
                // Stopped
                return;
            }

            _ = Task.Run(() => HandleClient(client));
        }
    }

    private static async Task HandleClient(TcpClient client)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                var (method, target, headers, body) = await ReadRequest(stream);

                JsonObject response;
                int status;

                try
                {
                    (status, response) = await Dispatch(method, target, headers, body);
                }
                catch (Exception e)
                {
                    status = 400;
                    response = new JsonObject { ["ok"] = false, ["error"] = e.Message };
                }

                await WriteResponse(stream, status, response.ToJsonString());
            }
            catch (Exception e)
            {
                Smithbox.LogWarning(typeof(RemoteControl), "Remote control request failed.", e);
            }
        }
    }

    private static async Task<(int, JsonObject)> Dispatch(string method, string target, Dictionary<string, string> headers, string body)
    {
        var queryStart = target.IndexOf('?');
        var path = queryStart >= 0 ? target[..queryStart] : target;
        var query = queryStart >= 0 ? target[(queryStart + 1)..] : "";

        var command = Uri.UnescapeDataString(path.Trim('/')).ToLowerInvariant();

        if (command == "")
            command = "help";

        if (!Commands.ContainsKey(command))
        {
            LogCommand($"unknown command '{Shorten(command)}'");
            return (404, new JsonObject { ["ok"] = false, ["error"] = $"Unknown command '{command}', see /help" });
        }

        var args = new JsonObject();

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = pair.IndexOf('=');
            var key = Uri.UnescapeDataString((eq >= 0 ? pair[..eq] : pair).Replace('+', ' '));
            var value = eq >= 0 ? Uri.UnescapeDataString(pair[(eq + 1)..].Replace('+', ' ')) : "true";
            args[key] = value;
        }

        if (!string.IsNullOrWhiteSpace(body))
        {
            if (JsonNode.Parse(body) is not JsonObject bodyArgs)
                throw new RemoteException("The body must be a JSON object");

            foreach (var (key, value) in bodyArgs.ToList())
            {
                bodyArgs.Remove(key);
                args[key] = value;
            }
        }

        if (CheckCaller(headers, args) is string refused)
        {
            LogCommand($"refused {command}: {refused}");
            return (403, new JsonObject { ["ok"] = false, ["error"] = refused });
        }

        var job = new RemoteJob { Command = command, Args = args };
        var number = Interlocked.Increment(ref CommandCount);
        var watch = Stopwatch.StartNew();
        LogCommand($"#{number} {command} {Shorten(args.ToJsonString())}");
        Queue.Enqueue(job);

        var timeout = TimeSpan.FromMinutes(30);
        var finished = await Task.WhenAny(job.Done.Task, Task.Delay(timeout));

        if (finished != job.Done.Task)
        {
            LogCommand($"#{number} {command} timed out after {watch.Elapsed.TotalSeconds:F1} s");
            return (504, new JsonObject { ["ok"] = false, ["error"] = "The command did not finish in time" });
        }

        var result = job.Done.Task.Result;
        var ok = result["ok"]?.GetValue<bool>() == true;
        LogCommand($"#{number} {command} {(ok ? "ok" : "error: " + Shorten(result["error"]?.ToString() ?? ""))} " +
                   $"({watch.Elapsed.TotalSeconds:F1} s)");

        return (ok ? 200 : 400, result);
    }

    private static async Task<(string method, string target, Dictionary<string, string> headers, string body)> ReadRequest(NetworkStream stream)
    {
        var header = new List<byte>();
        var one = new byte[1];

        // The header, up to the blank line
        while (true)
        {
            var read = await stream.ReadAsync(one);

            if (read == 0)
                throw new IOException("Connection closed in the request header");

            header.Add(one[0]);

            var n = header.Count;
            if (n >= 4 && header[n - 4] == '\r' && header[n - 3] == '\n' && header[n - 2] == '\r' && header[n - 1] == '\n')
                break;

            if (n > 65536)
                throw new IOException("Request header too large");
        }

        var lines = Encoding.ASCII.GetString(header.ToArray()).Split("\r\n");
        var requestLine = lines[0].Split(' ');

        if (requestLine.Length < 2)
            throw new IOException("Malformed request line");

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines.Skip(1))
        {
            var colon = line.IndexOf(':');

            if (colon > 0)
                headers[line[..colon].Trim()] = line[(colon + 1)..].Trim();
        }

        var length = headers.TryGetValue("Content-Length", out var cl) ? int.Parse(cl, CultureInfo.InvariantCulture) : 0;

        if (length is < 0 or > 16 * 1024 * 1024)
            throw new IOException("Request body too large");

        var body = new byte[length];
        var offset = 0;

        while (offset < length)
        {
            var read = await stream.ReadAsync(body.AsMemory(offset, length - offset));

            if (read == 0)
                throw new IOException("Connection closed in the request body");

            offset += read;
        }

        return (requestLine[0], requestLine[1], headers, Encoding.UTF8.GetString(body));
    }

    private static async Task WriteResponse(NetworkStream stream, int status, string json)
    {
        var reason = status switch
        {
            200 => "OK",
            403 => "Forbidden",
            404 => "Not Found",
            504 => "Gateway Timeout",
            _ => "Bad Request"
        };

        var body = Encoding.UTF8.GetBytes(json + "\n");
        var head = Encoding.ASCII.GetBytes(
            $"HTTP/1.1 {status} {reason}\r\nContent-Type: application/json; charset=utf-8\r\n" +
            $"Content-Length: {body.Length}\r\nConnection: close\r\n\r\n");

        await stream.WriteAsync(head);
        await stream.WriteAsync(body);
        await stream.FlushAsync();
    }

    #endregion

    #region Arguments

    private static bool Has(JsonObject args, string name) => args[name] != null;

    private static string GetString(JsonObject args, string name, string fallback = null)
    {
        var node = args[name];

        if (node == null)
            return fallback;

        return node is JsonValue value && value.TryGetValue<string>(out var s) ? s : node.ToJsonString();
    }

    private static string RequireString(JsonObject args, string name)
    {
        var s = GetString(args, name);

        if (string.IsNullOrEmpty(s))
            throw new RemoteException($"Missing argument '{name}'");

        return s;
    }

    private static float GetFloat(JsonObject args, string name, float fallback)
    {
        var node = args[name];

        if (node == null)
            return fallback;

        return ParseFloat(node, name);
    }

    private static float ParseFloat(JsonNode node, string name)
    {
        if (node is JsonValue value)
        {
            if (value.TryGetValue<double>(out var d))
                return (float)d;

            if (value.TryGetValue<float>(out var fl))
                return fl;

            if (value.TryGetValue<int>(out var i))
                return i;

            if (value.TryGetValue<string>(out var s) &&
                float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
                return f;
        }

        throw new RemoteException($"Argument '{name}' must be a number");
    }

    private static bool GetBool(JsonObject args, string name, bool fallback)
    {
        var node = args[name];

        if (node == null)
            return fallback;

        if (node is JsonValue value)
        {
            if (value.TryGetValue<bool>(out var b))
                return b;

            if (value.TryGetValue<string>(out var s))
                return s is "1" or "true" or "yes" or "on";
        }

        throw new RemoteException($"Argument '{name}' must be true or false");
    }

    /// <summary>
    ///     A vector as a JSON array [x, y, z] or a string "x,y,z" / "x y z"
    /// </summary>
    private static Vector3? GetVector(JsonObject args, string name)
    {
        var node = args[name];

        if (node == null)
            return null;

        float[] values;

        if (node is JsonArray array)
        {
            values = array.Select(e => ParseFloat(e, name)).ToArray();
        }
        else
        {
            var s = GetString(args, name);
            values = s.Split(new[] { ',', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => float.TryParse(p, NumberStyles.Float, CultureInfo.InvariantCulture, out var f)
                    ? f
                    : throw new RemoteException($"Argument '{name}' must be three numbers"))
                .ToArray();
        }

        if (values.Length != 3)
            throw new RemoteException($"Argument '{name}' must be three numbers");

        return new Vector3(values[0], values[1], values[2]);
    }

    private static JsonArray ToJson(Vector3 v) => new(Round(v.X), Round(v.Y), Round(v.Z));

    private static double Round(float f) => Math.Round(f, 4);

    #endregion

    #region Editor state

    private static ProjectEntry GetProject() => Smithbox.Orchestrator?.SelectedProject;

    private static MapEditorView GetView(bool require = true)
    {
        var project = GetProject();
        var view = project?.Handler?.MapEditor?.ViewHandler?.ActiveView;

        if (view == null && require)
            throw new RemoteException(project == null
                ? "No project is loaded (see /projects and /project-load)"
                : "The project has no Map Editor");

        return view;
    }

    private static VulkanViewport GetViewport(MapEditorView view)
    {
        var handler = view.ViewportHandler;
        var viewport = handler.ActiveViewport?.Viewport as VulkanViewport
                       ?? handler.Viewports.Select(w => w?.Viewport).OfType<VulkanViewport>().FirstOrDefault();

        if (viewport == null)
            throw new RemoteException("The Map Editor has no 3D viewport (NullViewport / low requirements mode?)");

        return viewport;
    }

    private static IEnumerable<KeyValuePair<string, MapContainer>> LoadedMaps(ProjectEntry project)
    {
        var maps = project?.Handler?.MapData?.PrimaryBank?.Maps;

        if (maps == null)
            yield break;

        foreach (var (entry, wrapper) in maps.ToList())
        {
            if (wrapper?.MapContainer != null)
                yield return new(entry.Filename, wrapper.MapContainer);
        }
    }

    private static int PendingResourceJobs => ResourceManager.ActiveJobProgress.Count;

    private static bool IsBusy()
    {
        if (Smithbox.Orchestrator == null || Smithbox.Orchestrator.IsProjectLoading)
            return true;

        var view = GetView(false);

        if (view?.Universe != null && view.Universe.HasPendingLoads)
            return true;

        return PendingResourceJobs > 0;
    }

    /// <summary>
    ///     Waits until nothing loads, for <paramref name="settleFrames" /> frames in a row
    /// </summary>
    private static IEnumerator WaitIdle(double timeoutSeconds, int settleFrames)
    {
        var watch = Stopwatch.StartNew();
        var quiet = 0;

        while (quiet < settleFrames)
        {
            quiet = IsBusy() ? 0 : quiet + 1;

            if (watch.Elapsed.TotalSeconds > timeoutSeconds)
                throw new RemoteException($"Still loading after {timeoutSeconds} s " +
                                          $"(resource jobs: {PendingResourceJobs})");

            yield return null;
        }
    }

    private static IEnumerator WaitFrames(int frames)
    {
        for (var i = 0; i < frames; i++)
            yield return null;
    }

    private static JsonObject DescribeCamera(VulkanViewport viewport)
    {
        var camera = viewport.ViewportCamera;
        var t = camera.CameraTransform;
        var rot = t.EulerRotation * (180.0f / MathF.PI);
        var forward = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, t.RotationMatrix));

        return new JsonObject
        {
            ["pos"] = ToJson(t.Position),
            ["rot"] = ToJson(rot),
            ["forward"] = ToJson(forward),
            ["fov"] = Round(CFG.Current.Viewport_Camera_FOV),
            ["mode"] = camera.ViewMode.ToString(),
            ["orthoSize"] = Round(camera.OrthographicSize),
            ["viewport"] = new JsonArray(viewport.Width, viewport.Height),
        };
    }

    private static Entity FindEntity(ProjectEntry project, string mapId, string name)
    {
        var maps = LoadedMaps(project).Where(m => mapId == null || m.Key == mapId).ToList();

        if (mapId != null && maps.Count == 0)
            throw new RemoteException($"Map {mapId} is not loaded");

        foreach (var (_, container) in maps)
        {
            var ent = container.GetObjectByName(name);

            if (ent != null)
                return ent;
        }

        throw new RemoteException($"No object named '{name}' in {(mapId ?? "the loaded maps")}");
    }

    #endregion

    #region Commands

    private static IEnumerator Help(JsonObject args, JsonObject result)
    {
        if (GetString(args, "cmd") is string cmd)
        {
            if (!Commands.ContainsKey(cmd))
                throw new RemoteException($"Unknown command '{cmd}'");

            result["command"] = cmd;
            result["usage"] = Usage.GetValueOrDefault(cmd, "");
            yield break;
        }

        result["commands"] = new JsonArray(Commands.Keys.Select(k => (JsonNode)k).ToArray());
        result["usage"] = "GET or POST /<command>, arguments as query parameters or a JSON object body";
        var help = new JsonObject();
        foreach (var k in Commands.Keys)
            help[k] = Usage.GetValueOrDefault(k, "");
        result["help"] = help;
    }

    private static IEnumerator Status(JsonObject args, JsonObject result)
    {
        var orchestrator = Smithbox.Orchestrator;
        var project = GetProject();

        result["port"] = Port;
        result["projectLoading"] = orchestrator?.IsProjectLoading ?? false;
        result["busy"] = IsBusy();
        result["resourceJobs"] = PendingResourceJobs;

        if (project != null)
        {
            result["project"] = new JsonObject
            {
                ["name"] = project.Descriptor.ProjectName,
                ["type"] = project.Descriptor.ProjectType.ToString(),
                ["projectPath"] = project.Descriptor.ProjectPath,
                ["dataPath"] = project.Descriptor.DataPath,
                ["initialized"] = project.Initialized,
            };
        }

        result["loadedMaps"] = new JsonArray(LoadedMaps(project).Select(m => (JsonNode)m.Key).ToArray());

        var view = GetView(false);

        if (view != null)
        {
            result["loadingMaps"] = new JsonArray(view.Universe.LoadingMaps.Select(m => (JsonNode)m).ToArray());

            try
            {
                var statusViewport = GetViewport(view);
                result["camera"] = DescribeCamera(statusViewport);
                if (statusViewport.ViewPipeline is { } pipeline && EnvCubeNameProperty != null)
                    result["envCube"] = EnvCubeNameProperty.GetValue(pipeline) as string;
            }
            catch (RemoteException)
            {
                // No 3D viewport
            }
        }

        var window = Smithbox.Instance._context.Window;
        result["window"] = new JsonArray(window.Width, window.Height);

        yield break;
    }

    private static IEnumerator Projects(JsonObject args, JsonObject result)
    {
        var orchestrator = Smithbox.Orchestrator;

        // The stored projects are read after the first frames
        var watch = Stopwatch.StartNew();
        while (!orchestrator.ExistingProjectsRead && watch.Elapsed.TotalSeconds < 30)
            yield return null;

        var list = new JsonArray();

        foreach (var p in orchestrator.Projects.ToList())
        {
            list.Add(new JsonObject
            {
                ["name"] = p.Descriptor.ProjectName,
                ["guid"] = p.Descriptor.ProjectGUID.ToString(),
                ["type"] = p.Descriptor.ProjectType.ToString(),
                ["projectPath"] = p.Descriptor.ProjectPath,
                ["dataPath"] = p.Descriptor.DataPath,
                ["initialized"] = p.Initialized,
                ["selected"] = p == orchestrator.SelectedProject,
                ["mapEditor"] = p.Descriptor.EnableMapEditor,
            });
        }

        result["projects"] = list;
    }

    private static IEnumerator ProjectLoad(JsonObject args, JsonObject result)
    {
        var orchestrator = Smithbox.Orchestrator;
        var name = RequireString(args, "name");
        var timeout = GetFloat(args, "timeout", 900);

        var watch = Stopwatch.StartNew();
        while (!orchestrator.ExistingProjectsRead && watch.Elapsed.TotalSeconds < 30)
            yield return null;

        var project = orchestrator.Projects.FirstOrDefault(p =>
                          p.Descriptor.ProjectGUID.ToString().Equals(name, StringComparison.OrdinalIgnoreCase))
                      ?? orchestrator.Projects.FirstOrDefault(p =>
                          p.Descriptor.ProjectName.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (project == null)
            throw new RemoteException($"No project named '{name}' (see /projects)");

        if (!project.Descriptor.EnableMapEditor)
            throw new RemoteException($"Project '{name}' has the Map Editor disabled");

        if (orchestrator.IsProjectLoading)
            throw new RemoteException("A project is loading already");

        if (project.Initialized && orchestrator.SelectedProject != project)
        {
            orchestrator.SelectProject(project);
        }
        else if (!project.Initialized)
        {
            _ = orchestrator.StartupProject(project, null, LoadFocus.FirstEditor);

            // Loading starts on this call, but give it a frame to show
            yield return null;

            watch.Restart();
            while (orchestrator.IsProjectLoading || !project.Initialized)
            {
                if (watch.Elapsed.TotalSeconds > timeout)
                    throw new RemoteException($"Project '{name}' did not load within {timeout} s");

                if (!orchestrator.IsProjectLoading && !project.Initialized && watch.Elapsed.TotalSeconds > 5)
                    throw new RemoteException($"Project '{name}' failed to load, see the Smithbox log");

                yield return null;
            }
        }

        project.Handler.FocusEditorWindow(project.Handler.MapEditor);

        result["project"] = project.Descriptor.ProjectName;
    }

    private static IEnumerator Maps(JsonObject args, JsonObject result)
    {
        var project = GetProject() ?? throw new RemoteException("No project is loaded");
        var filter = GetString(args, "filter");
        var regex = filter != null ? new Regex(filter, RegexOptions.IgnoreCase) : null;

        var list = new JsonArray();

        foreach (var (entry, wrapper) in project.Handler.MapData.PrimaryBank.Maps.ToList())
        {
            if (regex != null && !regex.IsMatch(entry.Filename))
                continue;

            list.Add(new JsonObject
            {
                ["map"] = entry.Filename,
                ["loaded"] = wrapper?.MapContainer != null,
            });
        }

        result["maps"] = list;
        yield break;
    }

    private static IEnumerator MapLoad(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var map = RequireString(args, "map");
        var wait = GetBool(args, "wait", true);
        var timeout = GetFloat(args, "timeout", 600);

        if (view.Selection.GetFileEntryFromMapID(map) == null)
            throw new RemoteException($"The project has no map {map} (see /maps)");

        view.Editor.Project.Handler.FocusEditorWindow(view.Editor);

        if (view.Selection.GetMapContainerFromMapID(map) == null)
            view.Universe.LoadMap(map);

        result["map"] = map;

        if (!wait)
            yield break;

        var watch = Stopwatch.StartNew();
        while (view.Selection.GetMapContainerFromMapID(map) == null || view.Universe.IsMapLoading(map) ||
               view.Universe.IsMapQueued(map))
        {
            if (watch.Elapsed.TotalSeconds > timeout)
                throw new RemoteException($"Map {map} did not load within {timeout} s");

            // A failed load leaves nothing loading and no container
            if (watch.Elapsed.TotalSeconds > 2 && !view.Universe.HasPendingLoads &&
                view.Selection.GetMapContainerFromMapID(map) == null)
                throw new RemoteException($"Map {map} failed to load, see the Smithbox log");

            yield return null;
        }

        var idle = WaitIdle(timeout, 5);
        while (idle.MoveNext())
            yield return null;
    }

    private static IEnumerator MapUnload(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var force = GetBool(args, "force", false);
        var all = GetBool(args, "all", false);

        var targets = all
            ? LoadedMaps(view.Editor.Project).Select(m => m.Key).ToList()
            : new List<string> { RequireString(args, "map") };

        foreach (var map in targets)
        {
            var container = view.Selection.GetMapContainerFromMapID(map);

            if (container == null)
                continue;

            if (container.HasUnsavedChanges && !force)
                throw new RemoteException($"Map {map} has unsaved changes (force=true discards them)");
        }

        view.ViewportSelection.ClearSelection();

        foreach (var map in targets)
        {
            if (view.Selection.GetMapContainerFromMapID(map) != null)
                view.Universe.UnloadMap(map);
        }

        result["unloaded"] = new JsonArray(targets.Select(m => (JsonNode)m).ToArray());

        yield return null;
    }

    private static IEnumerator Wait(JsonObject args, JsonObject result)
    {
        var idle = WaitIdle(GetFloat(args, "timeout", 600), (int)GetFloat(args, "settle", 10));
        while (idle.MoveNext())
            yield return null;

        result["resourceJobs"] = PendingResourceJobs;
    }

    private static IEnumerator Camera(JsonObject args, JsonObject result)
    {
        var viewport = GetViewport(GetView());
        var camera = viewport.ViewportCamera;

        var mode = GetString(args, "mode");

        if (mode != null)
        {
            if (!Enum.TryParse(mode, true, out ViewMode viewMode))
                throw new RemoteException($"Unknown mode '{mode}': {string.Join(", ", Enum.GetNames<ViewMode>())}");

            camera.SetProjectionType(viewMode);
        }

        if (Has(args, "orthoSize"))
            camera.SetOrthographicSize(GetFloat(args, "orthoSize", camera.OrthographicSize));

        if (Has(args, "fov"))
            CFG.Current.Viewport_Camera_FOV = GetFloat(args, "fov", CFG.Current.Viewport_Camera_FOV);

        var pos = GetVector(args, "pos");

        if (pos is Vector3 p)
            camera.CameraTransform.Position = p;

        Vector3? dir = GetVector(args, "dir");
        var lookat = GetVector(args, "lookat");

        if (lookat is Vector3 target)
            dir = target - camera.CameraTransform.Position;

        if (dir is Vector3 d && d.LengthSquared() > 1e-12f)
        {
            // The rotation is qy * qz * qx (Transform.EulerToQuaternion), which turns +Z into
            // (sin y cos x, -sin x, cos y cos x). ViewportCamera.LookAtTransform has both signs the other way.
            d = Vector3.Normalize(d);
            camera.CameraTransform.EulerRotation = new Vector3(-MathF.Asin(Math.Clamp(d.Y, -1f, 1f)), MathF.Atan2(d.X, d.Z), 0);
        }
        else if (GetVector(args, "rot") is Vector3 rot)
        {
            camera.CameraTransform.EulerRotation = rot * (MathF.PI / 180.0f);
        }

        if (viewport.Width > 0 && viewport.Height > 0)
            camera.UpdateProjectionMatrix();

        // The new view is drawn on the next frame
        yield return null;

        result["camera"] = DescribeCamera(viewport);
    }

    private static IEnumerator Select(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var name = GetString(args, "name");

        view.ViewportSelection.ClearSelection();

        if (name != null)
        {
            var ent = FindEntity(view.Editor.Project, GetString(args, "map"), name);
            view.ViewportSelection.AddSelection(ent);
            result["selected"] = ent.Name;
        }

        yield break;
    }

    private static IEnumerator Frame(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var viewport = GetViewport(view);
        var ent = FindEntity(view.Editor.Project, GetString(args, "map"), RequireString(args, "name"));

        view.ViewportSelection.ClearSelection();
        view.ViewportSelection.AddSelection(ent);

        // FrameAction frames the active viewport
        view.ViewportHandler.ActiveViewport ??= view.ViewportHandler.Viewports.FirstOrDefault(w => w?.Viewport == viewport);
        view.FrameAction.ApplyViewportFrame();

        if (!GetBool(args, "keepSelection", false))
            view.ViewportSelection.ClearSelection();

        yield return null;

        result["framed"] = ent.Name;
        result["camera"] = DescribeCamera(viewport);
    }

    private static IEnumerator Entities(JsonObject args, JsonObject result)
    {
        var project = GetProject() ?? throw new RemoteException("No project is loaded");
        var mapId = GetString(args, "map");
        var filter = GetString(args, "filter");
        var typeFilter = GetString(args, "type");
        var limit = (int)GetFloat(args, "limit", 500);
        var regex = filter != null ? new Regex(filter, RegexOptions.IgnoreCase) : null;
        var typeRegex = typeFilter != null ? new Regex(typeFilter, RegexOptions.IgnoreCase) : null;

        var list = new JsonArray();
        var total = 0;

        foreach (var (map, container) in LoadedMaps(project))
        {
            if (mapId != null && map != mapId)
                continue;

            foreach (var ent in container.Objects.ToList())
            {
                if (ent?.WrappedObject == null || ent == container.RootObject)
                    continue;

                var type = ent.WrappedObject.GetType().FullName ?? "";
                type = type[(type.LastIndexOf('.') + 1)..].Replace('+', '.');

                if (regex != null && !regex.IsMatch(ent.Name ?? ""))
                    continue;

                if (typeRegex != null && !typeRegex.IsMatch(type))
                    continue;

                total++;

                if (list.Count >= limit)
                    continue;

                var t = ent.GetRootLocalTransform();

                list.Add(new JsonObject
                {
                    ["map"] = map,
                    ["name"] = ent.Name,
                    ["type"] = type,
                    ["pos"] = ToJson(t.Position),
                    ["rot"] = ToJson(t.EulerRotation * (180.0f / MathF.PI)),
                });
            }
        }

        result["total"] = total;
        result["entities"] = list;
        yield break;
    }

    private static MemberInfo FindConfigMember(string name)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

        return (MemberInfo)typeof(CFG).GetField(name, flags) ?? typeof(CFG).GetProperty(name, flags)
            ?? throw new RemoteException($"No setting '{name}' (see /cfg-list)");
    }

    private static Type MemberType(MemberInfo m) => m is FieldInfo f ? f.FieldType : ((PropertyInfo)m).PropertyType;

    private static object GetMember(MemberInfo m) =>
        m is FieldInfo f ? f.GetValue(CFG.Current) : ((PropertyInfo)m).GetValue(CFG.Current);

    private static JsonNode ConfigValueToJson(object value) => value switch
    {
        null => null,
        bool b => b,
        int i => i,
        uint u => u,
        float f => Round(f),
        double d => d,
        string s => s,
        Vector3 v => ToJson(v),
        Enum e => e.ToString(),
        _ => value.ToString()
    };

    private static IEnumerator Config(JsonObject args, JsonObject result)
    {
        var member = FindConfigMember(RequireString(args, "name"));
        var type = MemberType(member);

        if (Has(args, "value"))
        {
            object value;
            var text = GetString(args, "value");

            if (type == typeof(bool))
                value = GetBool(args, "value", false);
            else if (type == typeof(int))
                value = (int)GetFloat(args, "value", 0);
            else if (type == typeof(uint))
                value = (uint)GetFloat(args, "value", 0);
            else if (type == typeof(float))
                value = GetFloat(args, "value", 0);
            else if (type == typeof(double))
                value = (double)GetFloat(args, "value", 0);
            else if (type == typeof(string))
                value = text;
            else if (type == typeof(Vector3))
                value = GetVector(args, "value");
            else if (type.IsEnum)
                value = Enum.TryParse(type, text, true, out var e)
                    ? e
                    : throw new RemoteException($"'{text}' is not one of {string.Join(", ", Enum.GetNames(type))}");
            else
                throw new RemoteException($"Setting '{member.Name}' has type {type.Name}, which can't be set remotely");

            if (member is FieldInfo field)
            {
                field.SetValue(CFG.Current, value);
            }
            else
            {
                var property = (PropertyInfo)member;

                if (!property.CanWrite)
                    throw new RemoteException($"Setting '{member.Name}' is read-only");

                property.SetValue(CFG.Current, value);
            }

            yield return null;
        }

        result["name"] = member.Name;
        result["type"] = type.Name;
        result["value"] = ConfigValueToJson(GetMember(member));
    }

    private static IEnumerator ConfigList(JsonObject args, JsonObject result)
    {
        var filter = GetString(args, "filter");
        var regex = filter != null ? new Regex(filter, RegexOptions.IgnoreCase) : null;
        var list = new JsonObject();

        foreach (var field in typeof(CFG).GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (regex != null && !regex.IsMatch(field.Name))
                continue;

            list[field.Name] = ConfigValueToJson(field.GetValue(CFG.Current));
        }

        result["settings"] = list;
        yield break;
    }

    private static IEnumerator Window(JsonObject args, JsonObject result)
    {
        var window = Smithbox.Instance._context.Window;

        if (Has(args, "width") || Has(args, "height"))
        {
            var width = (int)GetFloat(args, "width", window.Width);
            var height = (int)GetFloat(args, "height", window.Height);

            window.Width = width;
            window.Height = height;

            // The resize and the docked windows' new sizes take a few frames
            var frames = WaitFrames(5);
            while (frames.MoveNext())
                yield return null;
        }

        result["window"] = new JsonArray(window.Width, window.Height);

        var view = GetView(false);

        if (view != null)
        {
            try
            {
                var viewport = GetViewport(view);
                result["viewport"] = new JsonArray(viewport.Width, viewport.Height);
            }
            catch (RemoteException)
            {
                // No 3D viewport
            }
        }
    }

    private static IEnumerator EditorCommand(JsonObject args, JsonObject result)
    {
        var command = RequireString(args, "cmd");
        EditorCommandQueue.AddCommand(command);
        result["queued"] = command;

        // Commands are taken one per frame
        yield return null;
        yield return null;
    }

    /// <summary>
    ///     Waits as the screenshot does (wait=true: idle for settle frames, else settle frames) and writes the viewport's
    ///     tone mapped image to path; size gets its width and height
    /// </summary>
    private static IEnumerator CaptureTo(MapEditorView view, VulkanViewport viewport, string path, JsonObject args, int settle,
        int[] size)
    {
        if (GetBool(args, "wait", true))
        {
            var idle = WaitIdle(GetFloat(args, "timeout", 600), settle);
            while (idle.MoveNext())
                yield return null;
        }
        else
        {
            // TAA and auto exposure settle over a few frames
            var frames = WaitFrames(settle);
            while (frames.MoveNext())
                yield return null;
        }

        // Smithbox's own viewport snapshot: the tone mapped scene without overlays, read back a few frames later
        SnapshotPixels pixels = null;
        var done = false;
        viewport.Snapshot.Request(p =>
        {
            pixels = p;
            done = true;
        });

        // Drawn only while the viewport is visible, which needs the Map Editor in front
        var waited = 0;
        while (!done)
        {
            waited++;

            if (waited == 10)
                view.Editor.Project.Handler.FocusEditorWindow(view.Editor);

            if (waited > 120 && viewport.Snapshot.Pending)
            {
                viewport.Snapshot.Cancel();
                throw new RemoteException("The viewport was not drawn: is the Map Editor's viewport visible?");
            }

            if (waited > 600)
                throw new RemoteException("The screenshot was not read back");

            yield return null;
        }

        if (pixels == null)
            throw new RemoteException("The screenshot could not be read back");

        // The alpha is whatever the tone map left, which a PNG viewer would show as transparency
        for (var i = 3; i < pixels.Rgba.Length; i += 4)
            pixels.Rgba[i] = 255;

        var folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
            Directory.CreateDirectory(folder);

        using (var image = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgba32>(
                   pixels.Rgba, pixels.Width, pixels.Height))
            SixLabors.ImageSharp.ImageExtensions.SaveAsPng(image, path);

        var width = pixels.Width;
        var height = pixels.Height;
        size[0] = width;
        size[1] = height;
    }

    private static IEnumerator Screenshot(JsonObject args, JsonObject result)
    {
        var path = RequireString(args, "path");

        if (!Path.IsPathRooted(path))
            throw new RemoteException("The path must be absolute (a Windows path, as Smithbox sees it)");

        var view = GetView();
        var viewport = GetViewport(view);
        var size = new int[2];

        var capture = CaptureTo(view, viewport, path, args, (int)GetFloat(args, "settle", 10), size);
        while (capture.MoveNext())
            yield return null;

        if (GetBool(args, "labels", false))
        {
            var labels = DrawLabels(view, viewport, args, path, result);
            while (labels.MoveNext())
                yield return null;
        }

        result["path"] = path;
        result["size"] = new JsonArray(size[0], size[1]);
        result["camera"] = DescribeCamera(viewport);
    }

    private static RenderFilter ParseFilters(string text)
    {
        RenderFilter flags = 0;

        foreach (var part in text.Split(new[] { ',', ' ', '|' }, StringSplitOptions.RemoveEmptyEntries))
        {
            if (!Enum.TryParse(part, true, out RenderFilter flag))
                throw new RemoteException($"Unknown filter '{part}': {string.Join(", ", Enum.GetNames<RenderFilter>())}");

            flags |= flag;
        }

        return flags;
    }

    /// <summary>
    ///     The viewport's draw filter (RenderFilter flags): set=..., show=..., hide=..., comma separated
    /// </summary>
    private static IEnumerator Filter(JsonObject args, JsonObject result)
    {
        var scene = GetViewport(GetView()).RenderScene;

        if (GetString(args, "set") is string set)
            scene.DrawFilter = ParseFilters(set);

        if (GetString(args, "show") is string show)
            scene.DrawFilter |= ParseFilters(show);

        if (GetString(args, "hide") is string hide)
            scene.DrawFilter &= ~ParseFilters(hide);

        var shown = Enum.GetValues<RenderFilter>()
            .Where(f => f != 0 && f != RenderFilter.All && System.Numerics.BitOperations.PopCount((uint)f) == 1 &&
                        scene.DrawFilter.HasFlag(f))
            .Select(f => (JsonNode)f.ToString())
            .ToArray();

        result["shown"] = new JsonArray(shown);

        yield return null;
    }

    #region Placement

    private static MapContainer RequireLoadedMap(MapEditorView view, string map)
    {
        return view.Selection.GetMapContainerFromMapID(map)
               ?? throw new RemoteException($"Map {map} is not loaded (see /map-load)");
    }

    private static MsbEntity RequirePart(MapContainer container, string name)
    {
        if (container.GetObjectByName(name) is MsbEntity { WrappedObject: IMsbPart } ent)
            return ent;

        throw new RemoteException($"No part named '{name}' in {container.Name}");
    }

    private static Vector3 GetPartVector(MsbEntity ent, string property) =>
        ent.WrappedObject.GetType().GetProperty(property)?.GetValue(ent.WrappedObject) is Vector3 v ? v : Vector3.Zero;

    /// <summary>
    ///     The map to place in: map=..., or the only loaded map
    /// </summary>
    private static MapContainer GetTargetMap(MapEditorView view, JsonObject args)
    {
        var map = GetString(args, "map");

        if (map != null)
            return RequireLoadedMap(view, map);

        var loaded = LoadedMaps(view.Editor.Project).ToList();

        if (loaded.Count != 1)
            throw new RemoteException("More than one map is loaded, say which with map=...");

        return loaded[0].Value;
    }

    private static JsonObject DescribePart(MsbEntity ent)
    {
        return new JsonObject
        {
            ["map"] = ent.MapID,
            ["name"] = ent.Name,
            ["model"] = (ent.WrappedObject as IMsbPart)?.ModelName,
            ["type"] = ent.WrappedObject.GetType().Name,
            ["pos"] = ToJson(GetPartVector(ent, "Position")),
            ["rot"] = ToJson(GetPartVector(ent, "Rotation")),
            ["scale"] = HasScale(ent) ? ToJson(GetPartVector(ent, "Scale")) : null,
        };
    }

    private class FloorResult
    {
        public List<FirstPersonHit> Hits = new();
    }

    /// <summary>
    ///     The player collision of the loaded maps (the First Person tool's), built if it isn't yet
    /// </summary>
    private static IEnumerator EnsureCollision(MapEditorView view, double timeoutSeconds)
    {
        var world = view.FirstPersonCollision;

        if (world == null || !world.CanUse())
            throw new RemoteException("Collision ray casts aren't supported for this game");

        if (!CFG.Current.MapEditor_ModelLoad_Collisions)
            throw new RemoteException("Collision loading is off (cfg MapEditor_ModelLoad_Collisions)");

        if (!world.RequestBuild())
            throw new RemoteException("There is no collision to cast against (no map loaded?)");

        var watch = Stopwatch.StartNew();

        while (world.IsBuilding)
        {
            world.Update();

            if (watch.Elapsed.TotalSeconds > timeoutSeconds)
                throw new RemoteException($"The collision did not build within {timeoutSeconds} s");

            yield return null;
        }

        world.Update();

        if (world.Status.Failed)
            throw new RemoteException("Building the collision failed, see /log");
    }

    /// <summary>
    ///     Casts straight down from (x, fromY, z): the first hit, or every surface below with all=true
    /// </summary>
    private static IEnumerator CastDown(MapEditorView view, float x, float z, float fromY, bool all, FloorResult result)
    {
        var collision = EnsureCollision(view, 300);
        while (collision.MoveNext())
            yield return null;

        var set = view.FirstPersonCollision.Current;
        var origin = new Vector3(x, fromY, z);

        for (var i = 0; i < 64; i++)
        {
            if (!set.RayCast(origin, -Vector3.UnitY, 5000, out var hit))
                break;

            result.Hits.Add(hit);

            if (!all)
                break;

            origin = hit.Point - new Vector3(0, 0.01f, 0);
        }
    }

    private static IEnumerator Floor(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var x = GetFloat(args, "x", float.NaN);
        var z = GetFloat(args, "z", float.NaN);

        if (float.IsNaN(x) || float.IsNaN(z))
            throw new RemoteException("Missing argument 'x' or 'z'");

        var floor = new FloorResult();
        var cast = CastDown(view, x, z, GetFloat(args, "y", 1000), GetBool(args, "all", false), floor);
        while (cast.MoveNext())
            yield return null;

        if (floor.Hits.Count == 0)
            throw new RemoteException($"No collision below ({x}, {z})");

        result["y"] = Round(floor.Hits[0].Point.Y);
        result["hits"] = new JsonArray(floor.Hits.Select(h => (JsonNode)new JsonObject
        {
            ["y"] = Round(h.Point.Y),
            ["material"] = h.Material,
            ["part"] = h.Mesh?.PartName,
            ["normal"] = ToJson(h.Normal),
        }).ToArray());
    }

    /// <summary>
    ///     ray from=x,y,z dir=x,y,z [max=100] [all=true]: casts against the player collision in any direction (measuring
    ///     corridor widths, niche depths, head room); hits with distance, point, material, collision part, normal
    /// </summary>
    private static IEnumerator Ray(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var from = GetVector(args, "from") ?? throw new RemoteException("Missing argument 'from'");
        var dir = GetVector(args, "dir") ?? throw new RemoteException("Missing argument 'dir'");
        if (dir.LengthSquared() < 1e-8f)
            throw new RemoteException("'dir' must not be zero");
        dir = Vector3.Normalize(dir);
        var max = GetFloat(args, "max", 100);
        var all = GetBool(args, "all", false);

        var collision = EnsureCollision(view, 300);
        while (collision.MoveNext())
            yield return null;

        var set = view.FirstPersonCollision.Current;
        var origin = from;
        var hits = new JsonArray();
        for (var i = 0; i < 64; i++)
        {
            if (!set.RayCast(origin, dir, max, out var hit))
                break;
            var dist = Vector3.Distance(from, hit.Point);
            if (dist > max)
                break;
            hits.Add(new JsonObject
            {
                ["distance"] = Round(dist),
                ["point"] = ToJson(hit.Point),
                ["material"] = hit.Material,
                ["part"] = hit.Mesh?.PartName,
                ["normal"] = ToJson(hit.Normal),
            });
            if (!all)
                break;
            origin = hit.Point + dir * 0.01f;
        }

        result["from"] = ToJson(from);
        result["dir"] = ToJson(dir);
        result["distance"] = hits.Count > 0 ? hits[0]["distance"]?.DeepClone() : null;
        result["hits"] = hits;
    }

    /// <summary>
    ///     bbox name=&lt;part&gt; [map=...]: world bounding box of a part's render mesh (min, max, size) and its local size
    /// </summary>
    private static IEnumerator Bbox(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var ent = FindEntity(view.Editor.Project, GetString(args, "map"), RequireString(args, "name"))
            ?? throw new RemoteException("Part not found");
        var mesh = ent.RenderSceneMesh ?? throw new RemoteException("Part has no render mesh (not drawn yet?)");
        var world = mesh.GetBounds();
        var local = mesh.GetLocalBounds();
        result["name"] = ent.Name;
        result["min"] = ToJson(world.Min);
        result["max"] = ToJson(world.Max);
        result["size"] = ToJson(world.Max - world.Min);
        result["localSize"] = ToJson(local.Max - local.Min);
        yield break;
    }

    /// <summary>
    ///     With snap=true, the position's height becomes the floor below it (cast from 1 m above)
    /// </summary>
    private static IEnumerator SnapToFloor(MapEditorView view, JsonObject args, Vector3[] pos)
    {
        if (!GetBool(args, "snap", false))
            yield break;

        var floor = new FloorResult();
        var cast = CastDown(view, pos[0].X, pos[0].Z, pos[0].Y + 1.0f, false, floor);
        while (cast.MoveNext())
            yield return null;

        if (floor.Hits.Count == 0)
            throw new RemoteException($"No collision below ({pos[0].X}, {pos[0].Z}) to snap to");

        pos[0].Y = floor.Hits[0].Point.Y;
    }

    private static IEnumerator WaitMapLoaded(MapEditorView view, string map, double timeoutSeconds)
    {
        var watch = Stopwatch.StartNew();

        while (view.Selection.GetMapContainerFromMapID(map) == null || view.Universe.IsMapLoading(map) ||
               view.Universe.IsMapQueued(map))
        {
            if (watch.Elapsed.TotalSeconds > timeoutSeconds)
                throw new RemoteException($"Map {map} did not load within {timeoutSeconds} s");

            if (watch.Elapsed.TotalSeconds > 2 && !view.Universe.HasPendingLoads &&
                view.Selection.GetMapContainerFromMapID(map) == null)
                throw new RemoteException($"Map {map} failed to load");

            yield return null;
        }
    }

    private static IEnumerator PartList(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var filter = GetString(args, "filter");
        var regex = filter != null ? new Regex(filter, RegexOptions.IgnoreCase) : null;
        var maps = Has(args, "map")
            ? new List<MapContainer> { RequireLoadedMap(view, GetString(args, "map")) }
            : LoadedMaps(view.Editor.Project).Select(m => m.Value).ToList();

        var list = new JsonArray();

        foreach (var container in maps)
        {
            foreach (var ent in container.Objects.OfType<MsbEntity>().ToList())
            {
                if (ent.WrappedObject is not IMsbPart || (regex != null && !regex.IsMatch(ent.Name ?? "")))
                    continue;

                list.Add(DescribePart(ent));
            }
        }

        result["total"] = list.Count;
        result["parts"] = list;
        yield break;
    }

    /// <summary>
    ///     part-add from=&lt;map&gt;/&lt;part&gt; [map=target] [name=new] [pos=x,y,z] [rot=x,y,z] [snap=true]:
    ///     duplicates a part (with its model) into the target map, loading the source map for the copy if needed.
    ///     One undo step.
    /// </summary>
    private static IEnumerator PartAdd(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var (sourceMap, sourcePart) = SplitFrom(RequireString(args, "from"));
        var name = GetString(args, "name");

        if (name != null && target.GetObjectByName(name) != null)
            throw new RemoteException($"{target.Name} already has an object named '{name}'");

        var loaded = new bool[1];
        var load = EnsureMapLoaded(view, sourceMap, args, loaded);
        while (load.MoveNext())
            yield return null;

        try
        {
            var source = RequirePart(RequireLoadedMap(view, sourceMap), sourcePart);
            var keepIds = GetBool(args, "keepEntityId", false);
            var addAtlas = GetBool(args, "addAtlas", false);

            var pos = new[] { GetVector(args, "pos") ?? GetPartVector(source, "Position") };
            var snap = SnapToFloor(view, args, pos);
            while (snap.MoveNext())
                yield return null;

            var rot = GetVector(args, "rot") ?? GetPartVector(source, "Rotation");
            MsbEntity clone = null;

            var step = new RemoteStep($"part-add {sourcePart} from {sourceMap}", st =>
            {
                clone = DuplicateInto(st, view, source, target, keepIds, name);
                st.Run(PartChange(clone, pos[0], rot, null));
                RecordAdd(st, clone, target.Name, sourceMap, sourcePart, keepIds, addAtlas);
            });

            ExecuteStep(view, step, addAtlas);
            yield return null;

            result["part"] = DescribePart(clone);
            result["record"] = step.Records[0].Record.DeepClone();
        }
        finally
        {
            if (loaded[0] && !GetBool(args, "keepSource", false))
                view.Universe.UnloadMap(sourceMap);
        }
    }

    /// <summary>
    ///     part-move name=... [map=...] [pos=x,y,z] [rot=x,y,z] [dpos=...] [drot=...] [local=true] [scale=...] [snap=true]
    /// </summary>
    private static IEnumerator PartMove(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var ent = RequirePart(target, RequireString(args, "name"));

        if (!new[] { "pos", "rot", "dpos", "drot", "scale" }.Any(k => Has(args, k)) && !GetBool(args, "snap", false))
            throw new RemoteException("Give pos=, rot=, dpos=, drot=, scale= or snap=true");

        var local = GetBool(args, "local", false);
        var oldPos = GetPartVector(ent, "Position");
        var oldRot = GetPartVector(ent, "Rotation");
        var oldScale = PartScale(ent);

        var rot = GetVector(args, "rot") ?? oldRot;
        var q = PartQuat(ent, rot);

        if (GetVector(args, "drot") is Vector3 drot)
        {
            var dq = Transform.EulerToQuaternion(drot * Deg);
            q = local ? Quaternion.Concatenate(dq, q) : Quaternion.Concatenate(q, dq);
            rot = PartRotation(ent, q);
        }

        var pos = new[] { GetVector(args, "pos") ?? oldPos };

        if (GetVector(args, "dpos") is Vector3 dpos)
            pos[0] += local ? Vector3.Transform(dpos, PartQuat(ent, oldRot)) : dpos;

        var snap = SnapToFloor(view, args, pos);
        while (snap.MoveNext())
            yield return null;

        var scale = GetScale(args);

        var step = new RemoteStep($"part-move {ent.Name}", st =>
        {
            st.Run(PartChange(ent, pos[0], rot, scale));
            RecordMove(st, ent, target.Name, oldPos, oldRot, oldScale);
        });

        ExecuteStep(view, step, false);
        yield return null;

        result["part"] = DescribePart(ent);
        result["record"] = step.Records[0].Record.DeepClone();
    }

    private static IEnumerator PartDelete(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);
        var ent = RequirePart(target, RequireString(args, "name"));
        var name = ent.Name;

        view.ViewportSelection.ClearSelection();

        var step = new RemoteStep($"part-delete {name}", st =>
        {
            st.Run(new EntDeleteAction(view, new List<MsbEntity> { ent }, false));
            st.Records.Add((new JsonObject { ["op"] = "delete", ["name"] = name, ["map"] = target.Name }, null));
        });

        ExecuteStep(view, step, false);
        yield return null;

        result["deleted"] = name;
        result["record"] = step.Records[0].Record.DeepClone();
    }

    /// <summary>
    ///     Saves the map's changed files into the project directory
    /// </summary>
    private static IEnumerator MapSave(JsonObject args, JsonObject result)
    {
        var view = GetView();
        var target = GetTargetMap(view, args);

        if (!target.HasUnsavedChanges && target.DirtyFiles is MapFileType.None)
        {
            result["saved"] = false;
            result["reason"] = "No changes";
            yield break;
        }

        view.Universe.SaveMap(target);

        yield return null;

        result["saved"] = true;
        result["map"] = target.Name;
        result["projectPath"] = view.Editor.Project.Descriptor.ProjectPath;
    }

    #endregion

    private static IEnumerator Log(JsonObject args, JsonObject result)
    {
        var count = (int)GetFloat(args, "count", 50);
        var since = (long)GetFloat(args, "since", 0);
        var level = GetString(args, "level", "Trace");
        var filter = GetString(args, "filter");
        var regex = filter != null ? new Regex(filter, RegexOptions.IgnoreCase) : null;

        if (!Enum.TryParse(level, true, out Microsoft.Extensions.Logging.LogLevel minLevel))
            throw new RemoteException($"Unknown level '{level}': Trace, Debug, Information, Warning, Error");

        DrainLog();

        var lines = LogLines
            .Where(l => l.Index > since && l.Event.Level >= minLevel)
            .Where(l => regex == null || regex.IsMatch(l.Event.Message ?? "") || regex.IsMatch(l.Event.Category ?? ""))
            .TakeLast(count)
            .Select(l => (JsonNode)DescribeLog(l, GetBool(args, "exceptions", true)))
            .ToArray();

        result["last"] = LogCount;
        result["entries"] = new JsonArray(lines);
        yield break;
    }

    #endregion
}
