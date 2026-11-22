using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

// The Flying volume layer (AC6): the outer envelope of the map's flying navigation volume (NavVolumeEnvelope), drawn by
// the tool on its own like the Runtime cuts layer, as the navmesh overlay builder knows nothing of volumes. The volume
// is read from the project's files (the map's .nvvhktbnd) on a worker thread and its envelope kept per map, so turning
// the layer on and off, or redrawing the navmesh overlay, doesn't read it again.
public partial class NavigationTool
{
    private readonly NavigationOverlayRenderer VolumeRenderer = new();

    /// <summary>Set when the overlay, its map or the Flying volume layer changes: the envelope is drawn again.</summary>
    private bool VolumeOverlayDirty;

    /// <summary>Each map's envelope in map space, or null when the map has no volume; being built while not completed.</summary>
    private readonly Dictionary<string, Task<List<(Vector3 A, Vector3 B)>>> VolumeEnvelopes = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The map whose envelope is waiting to be built before it can be drawn.</summary>
    private string VolumePendingMap;

    /// <summary>Whether this project has flying navigation volumes (AC6 only).</summary>
    private bool HasFlyingVolumes => Project.Descriptor.ProjectType is ProjectType.AC6;

    private void UpdateVolumeOverlay()
    {
        // A pending envelope is drawn as soon as it is built
        if (VolumePendingMap != null && VolumeEnvelopes.TryGetValue(VolumePendingMap, out var pending) && pending.IsCompleted)
        {
            VolumePendingMap = null;
            VolumeOverlayDirty = true;
        }

        if (!VolumeOverlayDirty)
            return;

        VolumeOverlayDirty = false;
        VolumeRenderer.Clear();

        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;
        if (!ShowOverlay || scene == null || !HasFlyingVolumes || !Layers.Contains(NavOverlayLayer.FlyingVolume) || OverlayContainer == null)
            return;

        var map = OverlayContainer;
        if (!VolumeEnvelopes.TryGetValue(map.Name, out var task))
        {
            string mapId = map.Name;
            task = Task.Run(() => BuildEnvelope(mapId));
            VolumeEnvelopes[mapId] = task;
        }

        if (!task.IsCompleted)
        {
            VolumePendingMap = map.Name;
            return;
        }

        if (task.IsFaulted)
        {
            Smithbox.Log(this, LOC.Get("MAP_Navigation_Volume_Read_Failed", map.Name, task.Exception?.InnerException?.Message ?? ""));
            VolumeEnvelopes.Remove(map.Name);
            return;
        }

        var lines = task.Result;
        if (lines == null || lines.Count == 0)
            return;

        // Placed as the Map Editor places the volume: by its NVVA entry's entity, else the volume root
        var world = VolumeWorld(map);
        var overlay = new NavOverlay { MapId = map.Name, Set = OverlaySet };
        var batch = overlay.Batch(NavOverlayLayer.FlyingVolume);
        foreach (var (a, b) in lines)
            batch.Lines.Add((Vector3.Transform(a, world), Vector3.Transform(b, world)));

        VolumeRenderer.Show(scene, overlay, FaceOpacity);
    }

    /// <summary>Reads the map's flying navigation volume and builds its envelope; null when the map has none.</summary>
    private List<(Vector3 A, Vector3 B)> BuildEnvelope(string mapId)
    {
        var path = $"/map/{mapId[..3]}/{mapId}/{mapId}.nvvhktbnd.dcx";
        if (!Project.VFS.FS.FileExists(path))
            return null;

        var binder = BND4.Read(Project.VFS.FS.ReadFile(path).Value);
        var hkx = binder.Files.FirstOrDefault(f => f.Name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase));
        if (hkx == null)
            return null;

        var bytes = hkx.Bytes.ToArray();
        if (DCX.Is(bytes))
            bytes = DCX.Decompress(bytes).ToArray();

        var profile = Havok.HavokProfile.FromGame(Project.Descriptor.ProjectType.ToString());
        var document = Havok.Files.HavokFile.Read(bytes, profile, null);
        var volume = document.ObjectsOf<Havok.hk2019.hkaiNavVolume>().FirstOrDefault();
        return volume == null ? null : NavVolumeEnvelope.Build(volume);
    }

    /// <summary>Where the map's volume is drawn: its NVVA entry's entity, else the volume root, else the map's origin.</summary>
    private static Matrix4x4 VolumeWorld(MapContainer map)
    {
        var root = map.NavVolumeParent;
        var entry = root?.Children.FirstOrDefault(c => c.WrappedObject is NVA.Navmesh);
        return entry?.GetWorldMatrix() ?? root?.GetWorldMatrix() ?? Matrix4x4.Identity;
    }
}
