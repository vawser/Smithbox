using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// ER Connections tool: the active map's connect collisions grouped by target map (with the connections back and the
/// map's limits), and the connects of other maps that link to it. Incoming connections need one scan of every map's MSB
/// (in the background); loaded maps always use their current, possibly unsaved, objects. Plan:
/// Claude/Documentation/Connect Collision Tooling.md (C6).
/// </summary>
public class ConnectionsTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    /// <summary>One connect collision as the overview needs it</summary>
    private sealed record ConnectRef(string SourceMap, string Name, string CollisionName, string Target, byte WorldState, bool Invert);

    // The scan of every map's MSB as saved; replaced as a whole when a scan finishes
    private List<ConnectRef> _scanned;
    private int _scanDone;
    private int _scanTotal;
    private bool _scanning;
    private string _scanSummary = "";

    public ConnectionsTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public bool CanUse() => Project.Descriptor.ProjectType is ProjectType.ER;

    public void OnToolWindow()
    {
        GUI.WrappedText(LOC.Get("MAP_Connections_Description"));

        var map = View.Selection.SelectedMapContainer;

        if (map == null || map.LoadState is not MapContentLoadState.Loaded)
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("MAP_Connections_No_Map"));
            return;
        }

        var all = AllConnects();

        // The map and its limits
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Connections_Map_Header", map.Name), LOC.Get("MAP_Connections_Map_Header_TT"));
        GUI.DisplayAlias(AliasHelper.GetMapNameAlias(Project, map.Name));

        var (targets, areas) = ConnectCollisions.Usage(map);
        var usage = LOC.Get("MAP_ConnectCollision_Usage", targets.Count, ConnectCollisions.MaxTargets, areas.Count, ConnectCollisions.MaxForeignAreas);

        if (targets.Count > ConnectCollisions.MaxTargets || areas.Count > ConnectCollisions.MaxForeignAreas)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, usage);
        else
            GUI.WrappedText(usage);

        GUI.Tooltip(LOC.Get("MAP_ConnectCollision_Usage_TT"));

        // Outgoing
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Connections_Outgoing_Header"), LOC.Get("MAP_Connections_Outgoing_Header_TT"));
        DisplayOutgoing(map, all);

        // Incoming
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Connections_Incoming_Header"), LOC.Get("MAP_Connections_Incoming_Header_TT"));
        DisplayScanControls();
        DisplayIncoming(map, all);
    }

    #region Lists

    private void DisplayOutgoing(MapContainer map, List<ConnectRef> all)
    {
        var connects = ConnectCollisions.ConnectsIn(map).ToList();

        if (connects.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Connections_Outgoing_None"));
            return;
        }

        var byTarget = connects
            .GroupBy(e => ConnectCollisions.TargetName(((MSBE.Part.ConnectCollision)e.WrappedObject).MapID) ?? "-")
            .OrderBy(g => g.Key, StringComparer.Ordinal);

        foreach (var group in byTarget)
        {
            var target = group.Key;
            var back = BackCount(all, target, map.Name);

            var backText = back switch
            {
                null => LOC.Get("MAP_Connections_Back_Unknown"),
                0 => LOC.Get("MAP_Connections_Back_None"),
                _ => LOC.Get("MAP_Connections_Back_Count", back)
            };

            var alias = AliasHelper.GetMapNameAlias(Project, target);
            var title = string.IsNullOrEmpty(alias)
                ? LOC.Get("MAP_Connections_Target_Row", target, group.Count(), backText)
                : LOC.Get("MAP_Connections_Target_Row_Alias", target, alias, group.Count(), backText);

            if (ImGui.TreeNodeEx($"{title}##connOut_{target}", ImGuiTreeNodeFlags.SpanAvailWidth))
            {
                GUI.MultiButtonInput($"connOutActions_{target}",
                    $"connOutSelect_{target}", LOC.Get("MAP_Connections_Select_All"), LOC.Get("MAP_Connections_Select_All_TT", target), () => SelectAll(group),
                    $"connOutLoad_{target}", LOC.Get("MAP_Connections_Load"), LOC.Get("MAP_Connections_Load_TT", target), () => LoadMap(target));

                foreach (var ent in group)
                {
                    var connect = (MSBE.Part.ConnectCollision)ent.WrappedObject;
                    var label = LOC.Get("MAP_Connections_Connect_Row", ent.Name, string.IsNullOrEmpty(connect.CollisionName) ? "-" : connect.CollisionName, GateShort(connect.UnkT0A, connect.UnkT0B));

                    if (ImGui.Selectable($"{label}##connOutEnt_{ent.Name}"))
                        Select(new[] { ent });

                    GUI.Tooltip(LOC.Get("MAP_Connections_Select_Connect_TT"));
                }

                ImGui.TreePop();
            }
        }
    }

    private void DisplayIncoming(MapContainer map, List<ConnectRef> all)
    {
        if (all == null)
            return;

        var incoming = all
            .Where(c => string.Equals(c.Target, map.Name, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(c.SourceMap, map.Name, StringComparison.OrdinalIgnoreCase))
            .GroupBy(c => c.SourceMap)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .ToList();

        if (incoming.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Connections_Incoming_None", map.Name));
            return;
        }

        var outgoingTargets = new HashSet<string>(
            ConnectCollisions.ConnectsIn(map).Select(e => ConnectCollisions.TargetName(((MSBE.Part.ConnectCollision)e.WrappedObject).MapID) ?? ""),
            StringComparer.OrdinalIgnoreCase);

        foreach (var group in incoming)
        {
            var source = group.Key;
            var alias = AliasHelper.GetMapNameAlias(Project, source);
            var oneWay = outgoingTargets.Contains(source) ? "" : LOC.Get("MAP_Connections_Incoming_One_Way");

            var title = string.IsNullOrEmpty(alias)
                ? LOC.Get("MAP_Connections_Source_Row", source, group.Count(), oneWay)
                : LOC.Get("MAP_Connections_Source_Row_Alias", source, alias, group.Count(), oneWay);

            if (ImGui.TreeNodeEx($"{title}##connIn_{source}", ImGuiTreeNodeFlags.SpanAvailWidth))
            {
                var sourceMap = View.Selection.GetMapContainerFromMapID(source);
                var loaded = sourceMap is { LoadState: MapContentLoadState.Loaded };

                if (!loaded)
                {
                    if (GUI.ExplainedButton(LOC.Get("MAP_Connections_Load_Source", source), LOC.Get("MAP_Connections_Load_TT", source), true))
                        LoadMap(source);
                }

                foreach (var c in group)
                {
                    var label = LOC.Get("MAP_Connections_Connect_Row", c.Name, string.IsNullOrEmpty(c.CollisionName) ? "-" : c.CollisionName, GateShort(c.WorldState, c.Invert));

                    if (loaded)
                    {
                        if (ImGui.Selectable($"{label}##connInEnt_{source}_{c.Name}"))
                        {
                            var ent = sourceMap.Objects.FirstOrDefault(e => e.WrappedObject is MSBE.Part.ConnectCollision && e.Name == c.Name);

                            if (ent != null)
                                Select(new[] { ent });
                        }

                        GUI.Tooltip(LOC.Get("MAP_Connections_Select_Connect_TT"));
                    }
                    else
                    {
                        ImGui.TextUnformatted(label);
                    }
                }

                ImGui.TreePop();
            }
        }
    }

    /// <summary>Connects of the target map that link back to the map, or null when that isn't known yet</summary>
    private int? BackCount(List<ConnectRef> all, string target, string mapName)
    {
        var targetMap = View.Selection.GetMapContainerFromMapID(target);

        if (targetMap is { LoadState: MapContentLoadState.Loaded })
        {
            return ConnectCollisions.ConnectsIn(targetMap)
                .Count(e => string.Equals(ConnectCollisions.TargetName(((MSBE.Part.ConnectCollision)e.WrappedObject).MapID), mapName, StringComparison.OrdinalIgnoreCase));
        }

        if (all == null)
            return null;

        return all.Count(c => string.Equals(c.SourceMap, target, StringComparison.OrdinalIgnoreCase)
            && string.Equals(c.Target, mapName, StringComparison.OrdinalIgnoreCase));
    }

    private static string GateShort(byte worldState, bool invert)
    {
        return worldState switch
        {
            0 or 10 or 30 => LOC.Get(invert ? "MAP_Connections_Gate_Off" : "MAP_Connections_Gate_On", 300 + worldState),
            11 or 12 => invert ? "" : LOC.Get("MAP_Connections_Gate_Never"),
            _ => ""
        };
    }

    #endregion

    #region Scan

    private void DisplayScanControls()
    {
        if (_scanning)
        {
            GUI.WrappedText(LOC.Get("MAP_Connections_Scanning", Volatile.Read(ref _scanDone), _scanTotal));
            return;
        }

        var label = _scanned == null ? LOC.Get("MAP_Connections_Scan") : LOC.Get("MAP_Connections_Rescan");

        if (GUI.ExplainedButton(label, LOC.Get("MAP_Connections_Scan_TT"), true))
            StartScan();

        if (!string.IsNullOrEmpty(_scanSummary))
            GUI.WrappedText(_scanSummary);
    }

    /// <summary>Reads every map's MSB from the project (or game) files in the background</summary>
    private void StartScan()
    {
        var adapter = MsbGameAdapter.For(Project.Descriptor.ProjectType);
        var wrappers = Project.Handler.MapData.PrimaryBank.Maps.Values.ToList();

        if (adapter == null)
            return;

        _scanning = true;
        _scanDone = 0;
        _scanTotal = wrappers.Count;

        Task.Run(() =>
        {
            var found = new List<ConnectRef>();
            var failed = 0;
            var watch = Stopwatch.StartNew();

            foreach (var wrapper in wrappers)
            {
                try
                {
                    var data = wrapper.TargetFS.ReadFile(wrapper.Path);

                    if (data != null && adapter.Read(data.Value) is MSBE msb)
                    {
                        foreach (var c in msb.Parts.ConnectCollisions)
                            found.Add(ToRef(wrapper.Name, c));
                    }
                }
                catch (Exception)
                {
                    failed++;
                }

                Interlocked.Increment(ref _scanDone);
            }

            _scanned = found;
            _scanSummary = LOC.Get("MAP_Connections_Scan_Done", wrappers.Count - failed, found.Count, watch.Elapsed.TotalSeconds.ToString("0.0"), failed);
            _scanning = false;
        });
    }

    private static ConnectRef ToRef(string mapName, MSBE.Part.ConnectCollision c)
    {
        return new ConnectRef(mapName, c.Name, c.CollisionName, ConnectCollisions.TargetName(c.MapID), c.UnkT0A, c.UnkT0B);
    }

    /// <summary>
    /// Every connect known: the loaded maps' current objects, then the scan for the other maps. Null before a scan, and
    /// then only the outgoing list and loaded maps are known.
    /// </summary>
    private List<ConnectRef> AllConnects()
    {
        var scanned = _scanned;

        if (scanned == null)
            return null;

        var loaded = new Dictionary<string, MapContainer>(StringComparer.OrdinalIgnoreCase);

        foreach (var wrapper in Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            if (wrapper.MapContainer is { LoadState: MapContentLoadState.Loaded } container)
                loaded[wrapper.Name] = container;
        }

        var all = scanned.Where(c => !loaded.ContainsKey(c.SourceMap)).ToList();

        foreach (var (name, container) in loaded)
        {
            foreach (var ent in ConnectCollisions.ConnectsIn(container))
                all.Add(ToRef(name, (MSBE.Part.ConnectCollision)ent.WrappedObject) with { Name = ent.Name });
        }

        return all;
    }

    #endregion

    private void LoadMap(string mapName)
    {
        if (View.Selection.GetMapContainerFromMapID(mapName) is { LoadState: MapContentLoadState.Loaded })
            return;

        if (Project.Locator?.MapFiles?.Entries?.Any(e => string.Equals(e.Filename, mapName, StringComparison.OrdinalIgnoreCase)) != true)
        {
            Smithbox.LogWarning<ConnectionsTool>(LOC.Get("MAP_ConnectCollision_Target_Missing", mapName));
            return;
        }

        View.Universe.LoadMap(mapName, false, false, true);
    }

    private void SelectAll(IEnumerable<Entity> ents) => Select(ents);

    private void Select(IEnumerable<Entity> ents)
    {
        View.ViewportSelection.ClearSelection();

        foreach (var ent in ents)
            View.ViewportSelection.AddSelection(ent);

        View.FrameAction.ApplyViewportFrame();
    }
}
