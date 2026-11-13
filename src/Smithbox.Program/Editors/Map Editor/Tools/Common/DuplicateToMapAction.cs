using Hexa.NET.ImGui;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class DuplicateToMapAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public bool DisplayPopup = false;

    public (string, ObjectContainer) TargetMap = ("None", null);
    public (string, Entity) TargetBTL = ("None", null);

    public DuplicateToMapAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Update Loop
    /// </summary>
    public void OnGui()
    {
        if (DisplayPopup)
        {
            ImGui.OpenPopup("##DupeToTargetMapPopup");
            DisplayPopup = false;
        }

        if (ImGui.BeginPopup("##DupeToTargetMapPopup"))
        {
            DisplayMenu();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Duplicate_To_Map))
            {
                // Opened by OnGui, so the popup is opened and drawn under the same ImGui ID
                DisplayPopup = true;
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.Selectable("Duplicate to Map"))
        {
            DisplayPopup = true;
        }
        GUI.Tooltip($"Duplicate the selected map objects into another map.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Duplicate_To_Map)}");
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.BeginMenu("Duplicate to Map"))
        {
            DisplayMenu();

            ImGui.EndMenu();
        }
        GUI.Tooltip($"Duplicate the selected map objects into another map.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Duplicate_To_Map)}");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        DisplayMenu();
    }


    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        GUI.WrappedText("Use this to duplicate an existing map object into a different loaded map.");

        GUI.Spacer();
        GUI.SimpleHeader("Target Map", "The target map to duplicate the current selection to.");

        MapTargetPicker.Display(View, "targetMapSelect", ref TargetMap);

        ImGui.Checkbox($"{LOC.Get("MAP_Keep_World_Position")}##duplicateKeepWorld", ref CFG.Current.MapEditor_Keep_World_Position_Across_Maps);
        GUI.Tooltip(LOC.Get("MAP_Keep_World_Position_TT"));

        var targetMap = View.Selection.ResolveTargetMap(TargetMap);

        if (targetMap == null)
        {
            GUI.WrappedText("No loaded map has been targeted.");
        }
        else
        {
            var sel = View.ViewportSelection.GetPlacedEntities();

            if (sel.Any(e => e.WrappedObject is BTL.Light))
            {
                GUI.SimpleHeader("Target Lights", "The target BTL to duplicate the current selection to.");

                if (ImGui.BeginCombo("##TargetBTL", TargetBTL.Item1))
                {
                    foreach (Entity btl in targetMap.BTLParents)
                    {
                        var adName = (string)btl.WrappedObject;
                        if (ImGui.Selectable(adName))
                        {
                            TargetBTL = (adName, btl);
                            break;
                        }
                    }
                    ImGui.EndCombo();
                }

                if (TargetBTL.Item2 == null)
                {
                    GUI.WrappedText("No BTL has been targeted.");
                }
            }

            GUI.Spacer();
            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("extDuplicateActions",
                "duplicateMap", "Duplicate to Map", "", DuplicateToMapActions);
        }
    }

    public void DuplicateToMapActions()
    {
        var targetMap = View.Selection.ResolveTargetMap(TargetMap);

        // Without a target the action would quietly duplicate in place
        if (targetMap == null)
        {
            Smithbox.LogError<DuplicateToMapAction>("No loaded map has been targeted.");
            return;
        }

        var sel = View.ViewportSelection.GetPlacedEntities();

        // A BTL picked for an earlier target map belongs to that map
        var targetBtl = TargetBTL.Item2 != null && targetMap.BTLParents.Contains(TargetBTL.Item2)
            ? TargetBTL.Item2
            : null;

        DuplicateToMap(sel, targetMap, targetBtl);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void DuplicateToMap(List<MsbEntity> mapObjects, MapContainer mapContainer, Entity btlParent)
    {
        var action = new EntDuplicateAction(
                view: View,
                objects: mapObjects,
                targetMap: mapContainer,
                targetBTL: btlParent,
                duplicateToMap: true,
                createMapGroupEntry: false,
                incrementEntityID: CFG.Current.Toolbar_Duplicate_Increment_Entity_ID,
                incrementInstanceID: CFG.Current.Toolbar_Duplicate_Increment_InstanceID,
                incrementPartNames: CFG.Current.Toolbar_Duplicate_Increment_PartNames,
                clearEntityID: CFG.Current.Toolbar_Duplicate_Clear_Entity_ID,
                clearEntityGroupID: CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs,
                randomiseRotation: CFG.Current.Toolbar_Duplicate_Randomise_Rotation,
                randomRotationDir: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Dir,
                randomRotationRange: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Range);

        ViewportAction duplicate = action;

        // The copies stay where the sources are in the world, rather than at the same local coordinates in the other
        // map (another tile in ER and NR). Each source's frame is taken now, as the step runs after the clones exist.
        if (CFG.Current.MapEditor_Keep_World_Position_Across_Maps)
        {
            var sourceFrames = mapObjects.ToDictionary(e => e, MapPlacement.GetParentWorld);

            duplicate = new ViewportCompoundAction(new List<ViewportAction>
            {
                action,
                new DeferredAction(() => MapPlacement.BuildKeepWorldAction(
                    action.CreatedPairs.Select(p => ((Entity)p.Clone, sourceFrames[p.Source]))))
            });
        }

        View.ViewportActionManager.ExecuteAction(duplicate);

        View.DelayPicking();
    }

    // Kept by the editor rather than the view, so what is copied in one view can be pasted in another
    private List<MsbEntity> StoredSelection
    {
        get => View.Editor.Clipboard;
        set => View.Editor.Clipboard = value;
    }

    public bool HasCopiedObjects => StoredSelection.Count > 0;

    /// <summary>
    /// How many objects are copied
    /// </summary>
    public int CopiedCount => StoredSelection.Count;

    /// <summary>
    /// The maps the copied objects came from, each once, in the order they were copied
    /// </summary>
    public List<string> CopiedSourceMaps => StoredSelection
        .Select(e => View.Editor.ClipboardSourceMaps.GetValueOrDefault(e))
        .Where(e => e != null)
        .Distinct()
        .ToList();

    public void OnCopy(MapEditorView activeView)
    {
        var sel = activeView.ViewportSelection.GetPlacedEntities();

        // Copying with nothing selected keeps what was copied before
        if (sel.Count == 0)
            return;

        var copied = new List<MsbEntity>();
        var frames = new Dictionary<MsbEntity, Matrix4x4>();
        var sources = new Dictionary<MsbEntity, string>();

        foreach(var entry in sel)
        {
            var clone = (MsbEntity)entry.Clone();
            copied.Add(clone);

            // Where the copied object's map placed it, so a paste into another map can keep it there
            frames[clone] = MapPlacement.GetParentWorld(entry);

            if (entry.Container is MapContainer map)
            {
                sources[clone] = map.Name;
            }
        }

        StoredSelection = copied;
        View.Editor.ClipboardParentWorlds = frames;
        View.Editor.ClipboardSourceMaps = sources;
    }

    /// <summary>
    /// Pastes into the active map
    /// </summary>
    public void OnPaste(MapEditorView activeView)
    {
        PasteInto(activeView.Selection.SelectedMapContainer);
    }

    /// <summary>
    /// Pastes every copied object into the given map, as one undo step in the view that loaded it
    /// </summary>
    public void PasteInto(MapContainer map)
    {
        // Nothing to paste: no empty undo step, and the selection is left alone
        if (!HasCopiedObjects || map == null || map.LoadState is not MapContentLoadState.Loaded)
            return;

        var view = map.OwnerView ?? View;
        var step = BuildPaste(view, StoredSelection, map);

        step.Label = LOC.Get("MAP_Clipboard_History_Paste", MapHistory.NameObjects(StoredSelection), map.Name);

        view.ViewportActionManager.ExecuteAction(step);
        view.DelayPicking();
    }

    /// <summary>
    /// Pastes each copied object back into the map it was copied from, where that map is still loaded. Each view
    /// gets one undo step for the maps it loaded.
    /// </summary>
    public void PasteIntoSourceMaps()
    {
        if (!HasCopiedObjects)
            return;

        var sources = View.Editor.ClipboardSourceMaps;
        var steps = new Dictionary<MapEditorView, List<ViewportAction>>();
        var pasted = new Dictionary<MapEditorView, List<MsbEntity>>();
        var skipped = 0;

        foreach (var group in StoredSelection.GroupBy(e => sources.GetValueOrDefault(e)))
        {
            var map = group.Key != null ? View.Selection.GetMapContainerFromMapID(group.Key) : null;

            if (map == null || map.LoadState is not MapContentLoadState.Loaded)
            {
                skipped += group.Count();
                continue;
            }

            var view = map.OwnerView ?? View;

            if (!steps.ContainsKey(view))
            {
                steps[view] = new List<ViewportAction>();
                pasted[view] = new List<MsbEntity>();
            }

            steps[view].Add(BuildPaste(view, group.ToList(), map));
            pasted[view].AddRange(group);
        }

        if (skipped > 0)
        {
            Smithbox.LogWarning<DuplicateToMapAction>(LOC.Get("MAP_Clipboard_Source_Not_Loaded", skipped));
        }

        foreach (var (view, actions) in steps)
        {
            var step = new ViewportCompoundAction(actions)
            {
                Label = LOC.Get("MAP_Clipboard_History_Paste_Source", MapHistory.NameObjects(pasted[view]))
            };

            view.ViewportActionManager.ExecuteAction(step);
            view.DelayPicking();
        }
    }

    /// <summary>
    /// The paste of the objects into the map: their clones, then (in the same step, once the clones exist) the move
    /// to the placement orb, or back to where the copied objects were in the world
    /// </summary>
    private ViewportCompoundAction BuildPaste(MapEditorView view, List<MsbEntity> objects, MapContainer mapContainer)
    {
        var btlParent = mapContainer.BTLParents.FirstOrDefault();

        var action = new EntDuplicateAction(
                view: view,
                objects: objects,
                targetMap: mapContainer,
                targetBTL: btlParent,
                duplicateToMap: true,
                createMapGroupEntry: false,
                incrementEntityID: CFG.Current.Toolbar_Duplicate_Increment_Entity_ID,
                incrementInstanceID: CFG.Current.Toolbar_Duplicate_Increment_InstanceID,
                incrementPartNames: CFG.Current.Toolbar_Duplicate_Increment_PartNames,
                clearEntityID: CFG.Current.Toolbar_Duplicate_Clear_Entity_ID,
                clearEntityGroupID: CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs,
                randomiseRotation: CFG.Current.Toolbar_Duplicate_Randomise_Rotation,
                randomRotationDir: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Dir,
                randomRotationRange: CFG.Current.Toolbar_Duplicate_Randomise_Rotation_Range);

        var actions = new List<ViewportAction> { action };

        // Moved in the same undo step, once the clones exist; the orb's position is taken now, at the paste
        if (CFG.Current.MapEditor_Paste_At_Placement_Orb && view.ViewportHandler.ActiveViewport?.Viewport != null)
        {
            var target = view.ViewportWindow.GetPlacementPosition();

            actions.Add(new DeferredAction(() => PullToCameraAction.BuildMoveToPointAction(action.CreatedClones, target)));
        }
        else if (CFG.Current.MapEditor_Keep_World_Position_Across_Maps)
        {
            // Without the orb, the pasted objects go where the copied ones were in the world, rather than at the
            // same local coordinates in the target map (another tile in ER and NR)
            var frames = View.Editor.ClipboardParentWorlds;

            actions.Add(new DeferredAction(() => MapPlacement.BuildKeepWorldAction(
                action.CreatedPairs
                    .Where(p => frames.ContainsKey(p.Source))
                    .Select(p => ((Entity)p.Clone, frames[p.Source])))));
        }

        return new ViewportCompoundAction(actions);
    }

    /// <summary>
    /// "Paste (3 objects)", or plain "Paste" with nothing copied
    /// </summary>
    public string PasteLabel => HasCopiedObjects
        ? LOC.Get("MAP_Clipboard_Paste_Count", MapHistory.NameObjects(StoredSelection))
        : LOC.Get("MAP_Viewport_Context_Paste");

    /// <summary>
    /// Copy, Paste and Paste into Source Map, for the context menus of the viewport and Map Contents
    /// </summary>
    public void OnClipboardContext(bool canCopy)
    {
        if (canCopy)
        {
            if (ImGui.Selectable($"{LOC.Get("MAP_Viewport_Context_Copy")}##clipboardCopy"))
            {
                OnCopy(View);
            }
            GUI.Tooltip(LOC.Get("MAP_Viewport_Context_Copy_TT", InputManager.GetHint(KeybindID.Copy)));
        }

        var activeMap = View.Selection.SelectedMapContainer;

        if (ImGui.Selectable($"{PasteLabel}##clipboardPaste", false,
            HasCopiedObjects && activeMap != null ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            PasteInto(activeMap);
        }
        GUI.Tooltip(LOC.Get("MAP_Clipboard_Paste_Active_TT", activeMap?.Name ?? "-", InputManager.GetHint(KeybindID.Paste)));

        if (HasCopiedObjects)
        {
            DisplayPasteIntoSource();
        }
    }

    /// <summary>
    /// Copy, Paste and a Paste Into menu of every target, for the Edit menu
    /// </summary>
    public void OnClipboardMenu()
    {
        var canCopy = View.ViewportSelection.GetPlacedEntities().Count > 0;

        if (ImGui.MenuItem($"{LOC.Get("MAP_Viewport_Context_Copy")}##clipboardMenuCopy", InputManager.GetHint(KeybindID.Copy), false, canCopy))
        {
            OnCopy(View);
        }
        GUI.Tooltip(LOC.Get("MAP_Clipboard_Copy_Menu_TT"));

        var activeMap = View.Selection.SelectedMapContainer;

        if (ImGui.MenuItem($"{PasteLabel}##clipboardMenuPaste", InputManager.GetHint(KeybindID.Paste), false, HasCopiedObjects && activeMap != null))
        {
            PasteInto(activeMap);
        }
        GUI.Tooltip(LOC.Get("MAP_Clipboard_Paste_Active_TT", activeMap?.Name ?? "-", InputManager.GetHint(KeybindID.Paste)));

        if (ImGui.BeginMenu($"{LOC.Get("MAP_Clipboard_Paste_Into")}##clipboardMenuPasteInto", HasCopiedObjects))
        {
            DisplayPasteIntoSource();

            ImGui.Separator();

            // Every loaded map, by ID
            var maps = Project.Handler.MapData.PrimaryBank.Maps.Values
                .Select(e => e.MapContainer)
                .Where(e => e != null && e.LoadState is MapContentLoadState.Loaded)
                .OrderBy(e => e.Name)
                .ToList();

            foreach (var map in maps)
            {
                if (ImGui.MenuItem($"{map.Name}##clipboardMenuPasteInto_{map.Name}"))
                {
                    PasteInto(map);
                }
            }

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("MAP_Clipboard_Paste_Into_TT"));
    }

    /// <summary>
    /// "Paste into this map", for a loaded map's row in the Map List and its root in Map Contents
    /// </summary>
    public void OnPasteIntoMapContext(MapContainer map)
    {
        if (!HasCopiedObjects || map == null || map.LoadState is not MapContentLoadState.Loaded)
            return;

        if (ImGui.Selectable($"{LOC.Get("MAP_Clipboard_Paste_Here", MapHistory.NameObjects(StoredSelection))}##clipboardPasteHere_{map.Name}"))
        {
            PasteInto(map);
        }
        GUI.Tooltip(LOC.Get("MAP_Clipboard_Paste_Here_TT", map.Name));
    }

    private void DisplayPasteIntoSource()
    {
        var sources = CopiedSourceMaps;

        var loaded = sources.Count(e => View.Selection.GetMapContainerFromMapID(e) is { LoadState: MapContentLoadState.Loaded });

        var label = sources.Count == 1
            ? LOC.Get("MAP_Clipboard_Paste_Source_One", sources[0])
            : LOC.Get("MAP_Clipboard_Paste_Source_Many", sources.Count);

        if (ImGui.Selectable($"{label}##clipboardPasteSource", false, loaded > 0 ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            PasteIntoSourceMaps();
        }
        GUI.Tooltip(loaded > 0
            ? LOC.Get("MAP_Clipboard_Paste_Source_TT", string.Join(", ", sources))
            : LOC.Get("MAP_Clipboard_Paste_Source_Unloaded_TT", string.Join(", ", sources)));
    }
}