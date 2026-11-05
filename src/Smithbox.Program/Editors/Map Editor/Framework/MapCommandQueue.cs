using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class MapCommandQueue
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public bool DoFocus = false;

    /// <summary>
    /// A selection waiting for its map to finish loading. Commands arrive one per frame, so a select sent
    /// straight after a load arrives while the map is still loading (and its objects don't exist yet).
    /// </summary>
    private record PendingSelection(MapEditorView View, string MapID, Func<MapContainer, ISelectable> Resolve,
        string Description, DateTime Requested, bool LoadAttempted);

    private PendingSelection Pending;

    // A selection is given up on if its map hasn't loaded by then
    private static readonly TimeSpan PendingTimeout = TimeSpan.FromMinutes(2);

    public MapCommandQueue(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] commands)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        // Checked every frame, not only on frames that bring a command
        UpdatePendingSelection();

        if (commands == null)
            return;

        if (commands.Length <= 0)
            return;

        HandleLoadCommand(activeView, commands);
        HandleSelectCommand(activeView, commands);
        HandleIdSelectCommand(activeView, commands);
        HandlePropSearchCommand(activeView, commands);
    }

    public void HandleLoadCommand(MapEditorView curView, string[] commands)
    {
        if (commands[0] != "load")
            return;

        if (commands.Length < 2 || string.IsNullOrEmpty(commands[1]))
        {
            Smithbox.LogError<MapCommandQueue>("The map load command needs a map ID.");
            return;
        }

        ISelectable target = null;

        var mapid = commands[1];

        var container = curView.Selection.GetMapContainerFromMapID(mapid);

        if (container != null)
        {
            target = container.RootObject;
        }
        else
        {
            // Selected once loaded, so it shows in the content tree
            curView.Universe.LoadMap(mapid, true);
        }

        ApplyTargetSelection(curView, target);
    }

    public void HandleSelectCommand(MapEditorView curView, string[] commands)
    {
        // Every handler sees every command, so this one must only act on its own
        if (commands[0] != "select")
            return;

        if (commands.Length > 1)
        {
            var mapid = commands[1];
            if (commands.Length > 2)
            {
                var name = commands[2];
                MsbEntityType? entityType = commands.Length > 3 && Enum.TryParse(commands[3], out MsbEntityType parsed)
                    ? parsed
                    : null;

                SelectWhenLoaded(curView, mapid, $"\"{name}\"", m => entityType is MsbEntityType type
                    ? m.GetObjectsByName(name).FirstOrDefault(ent => ent is MsbEntity me && me.Type == type)
                    : m.GetObjectByName(name));
            }
            else
            {
                ApplyTargetSelection(curView, new ObjectContainerReference(mapid).GetSelectionTarget(curView.Universe));
            }
        }
    }

    /// <summary>
    /// map/idselect/{kind}/{mapID}/{entityID}: select the map object with the entity ID, sent by the Param Editor's
    /// "View in Map" buttons (bonfires, lamps, bosses, spirit summon stones). For the "enemy" kind an enemy part
    /// is preferred, as other objects can share the ID; any object with the ID is the fallback.
    /// </summary>
    public void HandleIdSelectCommand(MapEditorView curView, string[] commands)
    {
        if (commands[0] != "idselect")
            return;

        if (commands.Length < 4 || !uint.TryParse(commands[3], out var entityID) || entityID == 0)
        {
            Smithbox.LogError<MapCommandQueue>($"The map idselect command needs a kind, a map ID and an entity ID: {string.Join("/", commands)}");
            return;
        }

        var kind = commands[1];
        var mapid = commands[2];

        SelectWhenLoaded(curView, mapid, $"entity ID {entityID}", m =>
        {
            var matches = m.Objects.Where(e => GetEntityID(e) == entityID).ToList();

            if (kind == "enemy")
            {
                var enemy = matches.FirstOrDefault(e => EntityHelper.IsPartEnemy(e) || EntityHelper.IsPartDummyEnemy(e));

                if (enemy != null)
                    return enemy;
            }

            return matches.FirstOrDefault();
        });
    }

    /// <summary>
    /// The entity ID of a map object (int in older games, uint in newer ones), or 0 if it has none
    /// </summary>
    private static uint GetEntityID(Entity ent)
    {
        if (ent.WrappedObject == null)
            return 0;

        return PropFinderUtil.FindPropertyValue("EntityID", ent.WrappedObject) switch
        {
            uint u => u,
            int i => unchecked((uint)i),
            _ => 0
        };
    }

    /// <summary>
    /// Selects the object resolved from the map now if the map is loaded, otherwise once it has loaded
    /// (starting the load if no load is under way, so the command also works on its own).
    /// </summary>
    private void SelectWhenLoaded(MapEditorView curView, string mapID, string description, Func<MapContainer, ISelectable> resolve)
    {
        var map = curView.Selection.GetMapContainerFromMapID(mapID);

        if (map != null && !IsAnyMapLoading())
        {
            Pending = null;
            ApplyResolvedSelection(curView, map, resolve, description);
            return;
        }

        var loadAttempted = false;

        if (map == null && !IsAnyMapLoading())
        {
            if (!curView.Universe.LoadMap(mapID))
                return;

            loadAttempted = true;
        }

        // A newer request replaces an older one still waiting
        Pending = new PendingSelection(curView, mapID, resolve, description, DateTime.Now, loadAttempted);
    }

    private void UpdatePendingSelection()
    {
        if (Pending == null)
            return;

        // Wait for every load to finish: the map's objects are published before its models are loaded,
        // and framing the selection needs the models' bounds
        if (IsAnyMapLoading())
        {
            if (DateTime.Now - Pending.Requested > PendingTimeout)
            {
                Smithbox.LogError<MapCommandQueue>($"Gave up selecting {Pending.Description} in {Pending.MapID}: the map did not finish loading.");
                Pending = null;
            }

            return;
        }

        var pending = Pending;
        Pending = null;

        var map = pending.View.Selection.GetMapContainerFromMapID(pending.MapID);

        if (map == null)
        {
            // The load command can be refused while another map is loading, so try the load once more
            if (!pending.LoadAttempted && pending.View.Universe.LoadMap(pending.MapID))
            {
                Pending = pending with { LoadAttempted = true, Requested = DateTime.Now };
                return;
            }

            Smithbox.LogError<MapCommandQueue>($"Could not select {pending.Description}: map {pending.MapID} is not loaded.");
            return;
        }

        // The view may have been closed while the map loaded
        var view = map.OwnerView ?? pending.View;

        if (!Editor.ViewHandler.Views.Contains(view))
            view = Editor.ViewHandler.ActiveView;

        if (view != null)
        {
            ApplyResolvedSelection(view, map, pending.Resolve, pending.Description);
        }
    }

    private bool IsAnyMapLoading()
    {
        // Queued loads count too, so a selection doesn't resolve in the frame between one load and the next
        return Editor.ViewHandler.Views.Any(v => v != null && v.Universe.HasPendingLoads);
    }

    private void ApplyResolvedSelection(MapEditorView view, MapContainer map, Func<MapContainer, ISelectable> resolve, string description)
    {
        var target = resolve(map);

        if (target == null)
        {
            Smithbox.LogError<MapCommandQueue>($"Could not find {description} in {map.Name}.");
            return;
        }

        // Show the map in the content tree, so the tree can jump to the object
        view.Selection.SelectedMapID = map.Name;
        view.Selection.SelectedMapContainer = map;

        ApplyTargetSelection(view, target);
    }

    public void HandlePropSearchCommand(MapEditorView curView, string[] commands)
    {
        if (commands[0] != "propsearch")
            return;

        curView.LocalSearchView.propSearchCmd = commands.Skip(1).ToArray();
        curView.LocalSearchView.Property = curView.MapPropertyView.MsbPropertyView.RequestedSearchProperty;
        curView.MapPropertyView.MsbPropertyView.RequestedSearchProperty = null;
        curView.LocalSearchView.UpdatePropSearch = true;
    }

    public void ApplyTargetSelection(MapEditorView curView, ISelectable target)
    {
        if (target != null)
        {
            curView.ViewportSelection.ClearSelection();
            curView.ViewportSelection.AddSelection(target);
            curView.ViewportSelection.GotoTreeTarget = target;
            curView.FrameAction.ApplyViewportFrame();
        }
    }
}
