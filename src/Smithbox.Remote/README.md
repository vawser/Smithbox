# Smithbox Remote Control

An HTTP remote control for the Smithbox Map Editor, so scripts and automation (including AI agents) can load projects and
maps, move the camera, take lit screenshots of the viewport, inspect and place parts, and run checks such as walkability,
line of sight or gap scans, without clicking through the UI.

It ships as a separate assembly, `Smithbox.Remote.dll`, that is loaded into an unchanged Smithbox at start-up. Without a
port it does nothing.

## How it attaches

It only uses public Smithbox APIs, no patching:

- `StartupHook.cs`: the .NET startup hook (`DOTNET_STARTUP_HOOKS`) loads the assembly before Smithbox' `Main`.
- `RemoteHooks.cs`: subscribes to `Smithbox.FrameUpdated`, starts the server on the first frame, then runs one step of the
  current command per frame on the main thread, and adds the port to the window title.
- Screenshots use the viewport's own snapshot (`VulkanViewport.Snapshot`): the tone mapped scene (with FXAA when it is on),
  without ImGui, gizmos or overlays.

Files: `RemoteControl.cs` (server, basic commands, help texts), `RemoteDesigner.cs` (placing and editing parts),
`RemoteTools.cs` (checks, search, MSB data), `RemoteEntries.cs` (regions, events, inspect, bookmarks), `RemoteSurvey.cs`
(render mesh rays, capsule-path, profile, scatter), `RemoteMedia.cs` (batch, shot-compare, flythrough),
`ViewportLabels.cs` (part names drawn into screenshots).

## Build

Docker image (Linux, CPU rendering with Mesa llvmpipe) and a Windows build of Smithbox with the extension:

```
src/Smithbox.Remote/build.sh        # image smithbox-remote, Windows build in out/win-x64
```

Only the extension, against the Smithbox sources in this repository:

```
dotnet build src/Smithbox.Remote/Smithbox.Remote.csproj -c Release-win      # or Release-linux
```

Then put `Smithbox.Remote.dll` from `src/Smithbox.Remote/bin/...` next to `Smithbox.Vulkan(.exe)`. The
extension must be built against the same Smithbox version it is loaded into.

## Run

Windows, e.g. `Smithbox-Remote.cmd` next to `Smithbox.Vulkan.exe`:

```
set DOTNET_STARTUP_HOOKS=%~dp0Smithbox.Remote.dll
start "" "%~dp0Smithbox.Vulkan.exe" --remote-port 7780
```

`--remote-port N` or the environment variable `SMITHBOX_REMOTE_PORT` turns the server on. `--remote-role <text>` only adds a
label to the window title, useful with several instances. The server listens on `127.0.0.1` only.

Docker (port 7790 inside the container; reach it with `docker exec`):

```
docker run -d --name sb --user 1000:1000 \
  -v <smithbox-home>:/home/sb -v <project>:/project -v <game-data>:/game:ro \
  smithbox-remote --remote-role Docker
```

Smithbox needs a project in `<smithbox-home>/.local/share/Smithbox/Projects/<GUID>.json` with container paths, e.g.

```json
{
  "ProjectGUID": "00000000-0000-4000-8000-000000000001",
  "ProjectName": "my-project",
  "ProjectPath": "/project",
  "DataPath": "/game",
  "ProjectType": 6,
  "AutoSelect": true,
  "EnableMapEditor": true
}
```

`ProjectType` is the number from `ProjectType.cs` (6 = Bloodborne, 8 = Elden Ring, ...). `entrypoint.sh` writes a configuration
with `Interface_Scale_by_DPI: false` if there is none (SDL reports no DPI under Xvfb, which would give a font size of 0).
Smithbox loads the last used project at start; send `wait` before the first real command.

## Calling it

Requests are `GET` or `POST /<command>`, arguments as query parameters or as a JSON object body. The answer is a JSON object
with `ok`; on errors it has `error` and the warnings Smithbox logged meanwhile. Commands run one at a time on the main thread.

```
curl -s -G http://127.0.0.1:7780/status
curl -s -G http://127.0.0.1:7780/help --data-urlencode cmd=screenshot
curl -s -X POST http://127.0.0.1:7780/map-load -d '{"map":"m10_00_00_00"}'
```

`remote.sh` wraps this (`SMITHBOX_REMOTE_PORT`, or `SMITHBOX_DOCKER=<container>` for a container):

```
R=src/Smithbox.Remote/remote.sh
$R project-load name=my-project
$R map-load map=m10_00_00_00
$R wait settle=10
$R cfg name=Viewport_Render_Mode value=1                       # game lighting instead of fullbright
$R filter hide=Collision,Region,Light,Debug,Navmesh            # clean images
$R camera pos=130,-18,160 lookat=130,-19,176 fov=55
$R screenshot path=/tmp/shots/a.png settle=3                   # path as Smithbox sees it (a container path in Docker)
$R screenshot path=/tmp/shots/b.png labels=true                # part names drawn into the image, positions in labels[]
```

`GET /help` lists every command with its arguments; `/help?cmd=<command>` shows one.

### Commands

- Basics: status, projects, project-load, maps, map-load, map-unload, map-save, wait, camera, select, frame, entities, cfg,
  cfg-list, window, command (any internal editor command), screenshot, log, filter.
- Parts: part-list, part-add, part-move, part-delete, group-copy, place-on, align, set, floor, ray, bbox, pick, undo, redo,
  model-preview. Each change is one Smithbox action, so undo and redo work as in the editor.
- Regions and events: region-add, region-move, region-delete, region-new, region-list, event-new, event-set, event-list.
- Checks: leak-scan (gap mode views: flat render with a magenta background), plan/section, diff, overlap, free-spot, los,
  capsule-path (walkable path for a capsule over the collision), profile, lightmap-check, texture-check, inspect.
- Search: find-model, model-search.
- MSB edits as data: ops-mark, ops-export, ops-apply.
- Media and batches: batch (several commands as one undo step), shot-compare, flythrough (GIF, or MP4 with ffmpeg), bookmark,
  scatter.

Responses of editing commands contain `record`/`records` (and `unrecords` for undo), so a client can keep its own log of
placements and replay it elsewhere.

## Tips

- Screenshots wait until nothing loads and then `settle` frames (default 10; 2-3 is enough in Docker). The Map Editor must be
  visible; the command brings it to the front itself.
- Speed per view (camera + screenshot), measured on a mid-size map: Windows with a GPU about 1 s; Docker 1600x900 with
  `settle=10` 7-10 s, with `settle=3` and `window width=960 height=540` about 2.3 s. For many views, send them in one script
  run and look at the results together rather than one round trip per view.
- Docker: the server log goes to stdout (`docker logs <container>`, lines with ` remote `: number, command, arguments, result
  and duration); `-e SMITHBOX_REMOTE_LOG=0` turns it off. Idle Smithbox renders continuously and uses about one CPU core.
- Docker: after `docker restart` Xvfb does not start (stale lock file); create a new container instead.
- Docker: the first command after start can see a wrong viewport size until the Map Editor is in front.
- Bloodborne under llvmpipe: with `Viewport_Render_Mode 1` the image can turn white while distance, volumetric and BB fog are on.
- Modal Smithbox dialogs block the remote control. Builds that have `LightAtlasResolver.DefaultEntryAnswerOverride` let part
  commands answer the light atlas question (`addAtlas=`) without a dialog.
- Optional members, used when the Smithbox build has them: `SceneRenderPipeline.MapEnvCubeName` (`envCube` in `status`) and
  `DolLightmapBank.ModelLightFactor` (`lm-preview`, otherwise it reports that it is not supported).

## Security

There is no authentication. The server only listens on the loopback interface, but any process on the machine (or in the
container) can drive Smithbox through it, including saving maps. Start it only when you need it.
