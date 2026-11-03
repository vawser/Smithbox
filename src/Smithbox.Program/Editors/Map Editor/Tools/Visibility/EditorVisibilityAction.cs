using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class EditorVisibilityAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public EditorVisibilityAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Visibility_Flip))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }

            if (InputManager.IsPressed(KeybindID.MapEditor_Visibility_Enable))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }

            if (InputManager.IsPressed(KeybindID.MapEditor_Visibility_Disable))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
            }
        }

        // These act on every loaded map object, so they don't need a selection
        if (InputManager.IsPressed(KeybindID.MapEditor_Global_Visibility_Flip))
        {
            ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_Global_Visibility_Enable))
        {
            ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
        }

        if (InputManager.IsPressed(KeybindID.MapEditor_Global_Visibility_Disable))
        {
            ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.BeginMenu("Editor Visibility"))
        {
            if (ImGui.MenuItem("Flip Selection Visibility", InputManager.GetHint(KeybindID.MapEditor_Visibility_Flip)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }

            if (ImGui.MenuItem("Enable Selection Visibility", InputManager.GetHint(KeybindID.MapEditor_Visibility_Enable)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }

            if (ImGui.MenuItem("Disable Selection Visibility", InputManager.GetHint(KeybindID.MapEditor_Visibility_Disable)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
            }

            if (ImGui.MenuItem("Flip Visibility for All", InputManager.GetHint(KeybindID.MapEditor_Global_Visibility_Flip)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
            }

            if (ImGui.MenuItem("Enable Visibility for All", InputManager.GetHint(KeybindID.MapEditor_Global_Visibility_Enable)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
            }

            if (ImGui.MenuItem("Disable Visibility for All", InputManager.GetHint(KeybindID.MapEditor_Global_Visibility_Disable)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.BeginMenu("Editor Visibility"))
        {
            if (ImGui.MenuItem("Flip Selection Visibility", InputManager.GetHint(KeybindID.MapEditor_Visibility_Flip)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }

            if (ImGui.MenuItem("Enable Selection Visibility", InputManager.GetHint(KeybindID.MapEditor_Visibility_Enable)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }

            if (ImGui.MenuItem("Disable Selection Visibility", InputManager.GetHint(KeybindID.MapEditor_Visibility_Disable)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
            }

            if (ImGui.MenuItem("Flip Visibility for All", InputManager.GetHint(KeybindID.MapEditor_Global_Visibility_Flip)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
            }

            if (ImGui.MenuItem("Enable Visibility for All", InputManager.GetHint(KeybindID.MapEditor_Global_Visibility_Enable)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
            }

            if (ImGui.MenuItem("Disable Visibility for All", InputManager.GetHint(KeybindID.MapEditor_Global_Visibility_Disable)))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Tool Menu
    /// </summary>
    public void OnToolMenu()
    {
        if (ImGui.BeginMenu("Toggle Editor Visibility by Tag"))
        {
            ImGui.InputText("##targetTag", ref CFG.Current.Toolbar_Tag_Visibility_Target, 255);
            GUI.Tooltip("Specify which tag the map objects will be filtered by.");

            if (ImGui.MenuItem("Enable Visibility"))
            {
                CFG.Current.Toolbar_Tag_Visibility_State_Enabled = true;
                CFG.Current.Toolbar_Tag_Visibility_State_Disabled = false;

                ApplyEditorVisibilityChangeByTag();
            }
            if (ImGui.MenuItem("Disable Visibility"))
            {
                CFG.Current.Toolbar_Tag_Visibility_State_Enabled = false;
                CFG.Current.Toolbar_Tag_Visibility_State_Disabled = true;

                ApplyEditorVisibilityChangeByTag();
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        GUI.WrappedText("Use these actions to adjust the in-editor visibility of map objects.");

        GUI.Spacer();
        GUI.SimpleHeader("Selection", "Actions that affect the current selection of map objects.");

        GUI.MultiButtonInput("visActions",
            "toggleVis", "Toggle Visibility", "Toggles the current visibility state to the opposite.", ToggleVisLocalAction,
            "enableVis", "Make Visible", "Force the current visibility state to visible.", MakeVisibleLocalAction,
            "disableVis", "Make Invisible", "Force the current visibility state to invisible.", MakeInvisibleLocalAction);


        GUI.Spacer();
        GUI.SimpleHeader("Global", "Actions that affect all currently loaded map objects.");

        GUI.MultiButtonInput("globalVisActions",
            "toggleVis", "Toggle Visibility", "Toggles the current visibility state to the opposite.", ToggleVisGlobalAction,
            "enableVis", "Make Visible", "Force the current visibility state to visible.", MakeVisibleGlobalAction,
            "disableVis", "Make Invisible", "Force the current visibility state to invisible.", MakeInvisibleGlobalAction);

        GUI.Spacer();
        GUI.SimpleHeader("Focus", "Temporarily ghost the map objects of every loaded map other than those of the current selection.");

        GUI.MultiButtonInput("focusActions",
            "focusMap", "Focus", "Ghost all map objects that do not belong to the same map as the current selection.", FocusAction,
            "clearFocus", "Clear Focus", "Return all map objects to their normal appearance.", ClearFocusAction);

        var focusedMaps = Project.Handler.MapData.FocusedMaps;
        if (focusedMaps.Count > 0)
        {
            GUI.WrappedText($"Focused: {string.Join(", ", focusedMaps.Select(x => x.Name))}");
        }

        GUI.Spacer();
        GUI.SimpleHeader("Target Tag", "Toggle the visibility of map objects with the associated alias tag (i.e. LOD).");

        GUI.HintTextInput("TagToggleInput", ref CFG.Current.Toolbar_Tag_Visibility_Target, "Enter the tag you want to target...");

        GUI.MultiButtonInput("tagActions",
            "enableVis", "Make Visible", "Force the current visibility state to visible.", MakeVisibleTagAction,
            "disableVis", "Make Invisible", "Force the current visibility state to invisible.", MakeInvisibleTagAction);
    }

    public void ToggleVisLocalAction()
    {
        ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
    }

    public void MakeVisibleLocalAction()
    {
        ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
    }

    public void MakeInvisibleLocalAction()
    {
        ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
    }

    public void ToggleVisGlobalAction()
    {
        ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
    }

    public void MakeVisibleGlobalAction()
    {
        ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
    }

    public void MakeInvisibleGlobalAction()
    {
        ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Disable);
    }

    public void MakeVisibleTagAction()
    {
        CFG.Current.Toolbar_Tag_Visibility_State_Enabled = true;
        CFG.Current.Toolbar_Tag_Visibility_State_Disabled = false;

        ApplyEditorVisibilityChangeByTag();
    }

    public void MakeInvisibleTagAction()
    {
        CFG.Current.Toolbar_Tag_Visibility_State_Enabled = false;
        CFG.Current.Toolbar_Tag_Visibility_State_Disabled = true;

        ApplyEditorVisibilityChangeByTag();
    }

    public void FocusAction()
    {
        HashSet<MapContainer> maps = View.ViewportSelection.GetFilteredSelection<Entity>()
            .Select(x => x.Container)
            .OfType<MapContainer>()
            .ToHashSet();

        if (maps.Count == 0)
        {
            Smithbox.LogError<EditorVisibilityAction>("No map object selected.");
            return;
        }

        SetFocusedMaps(maps);
    }

    public void ClearFocusAction()
    {
        SetFocusedMaps(new());
    }

    /// <summary>
    /// A focused map can't stay focused once it is gone, or the rest would stay ghosted for no reason.
    /// </summary>
    public void OnMapUnloaded(MapContainer map)
    {
        HashSet<MapContainer> focused = Project.Handler.MapData.FocusedMaps;

        if (focused.Contains(map))
        {
            SetFocusedMaps(focused.Where(x => x != map).ToHashSet());
        }
    }

    private void SetFocusedMaps(HashSet<MapContainer> maps)
    {
        Project.Handler.MapData.FocusedMaps = maps;

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
                continue;

            foreach (Entity obj in entry.Value.MapContainer.Objects)
            {
                obj.UpdateFocusState();
            }
        }

        View.DelayPicking();
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyEditorVisibilityChange(EditorVisibilityType targetType, EditorVisibilityState targetState)
    {
        if (targetType == EditorVisibilityType.Selected)
        {
            HashSet<Entity> selected = View.ViewportSelection.GetFilteredSelection<Entity>();

            if(selected.Count == 0)
            {
                Smithbox.LogError<EditorVisibilityAction>("No map object selected.");
                return;
            }

            foreach (Entity s in selected)
            {
                if (targetState is EditorVisibilityState.Enable)
                    s.EditorVisible = true;

                if (targetState is EditorVisibilityState.Disable)
                    s.EditorVisible = false;

                if (targetState is EditorVisibilityState.Flip)
                    s.EditorVisible = !s.EditorVisible;
            }
        }

        if (targetType == EditorVisibilityType.All)
        {
            // The bank holds every map in the game, not just the loaded ones
            if (!Project.Handler.MapData.PrimaryBank.Maps.Values.Any(x => x.MapContainer != null))
            {
                Smithbox.LogError<EditorVisibilityAction>("No map loaded.");
                return;
            }

            foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer == null)
                    continue;

                foreach (Entity obj in entry.Value.MapContainer.Objects)
                {
                    if (targetState is EditorVisibilityState.Enable)
                        obj.EditorVisible = true;

                    if (targetState is EditorVisibilityState.Disable)
                        obj.EditorVisible = false;

                    if (targetState is EditorVisibilityState.Flip)
                        obj.EditorVisible = !obj.EditorVisible;
                }
            }
        }

        View.DelayPicking();
    }
    public void ApplyEditorVisibilityChangeByTag()
    {
        if (!Project.Handler.MapData.PrimaryBank.Maps.Values.Any(x => x.MapContainer != null))
        {
            Smithbox.LogError<EditorVisibilityAction>("No map loaded.");
            return;
        }

        var target = CFG.Current.Toolbar_Tag_Visibility_Target;

        // Built once, so each part is a lookup rather than a walk over thousands of aliases
        var taggedAssets = GetTaggedAliasIDs(ProjectAliasType.Assets, target);
        var taggedMapPieces = GetTaggedAliasIDs(ProjectAliasType.MapPieces, target);

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            var map = entry.Value.MapContainer;

            if (map == null)
                continue;

            foreach (Entity obj in map.Objects)
            {
                if (!EntityHelper.IsPart(obj))
                    continue;

                var modelName = obj.GetPropertyValue<string>("ModelName");

                if (string.IsNullOrEmpty(modelName))
                    continue;

                // Map piece aliases are per map (m10_00_00_00_000004), but the model name is not (m000004)
                var mapPieceID = $"{map.Name}_{modelName.TrimStart('m', 'M')}";

                if (!taggedAssets.Contains(modelName) && !taggedMapPieces.Contains(mapPieceID))
                    continue;

                if (CFG.Current.Toolbar_Tag_Visibility_State_Enabled)
                {
                    obj.EditorVisible = true;
                }
                if (CFG.Current.Toolbar_Tag_Visibility_State_Disabled)
                {
                    obj.EditorVisible = false;
                }
            }
        }

        View.DelayPicking();
    }

    private HashSet<string> GetTaggedAliasIDs(ProjectAliasType aliasType, string tag)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (Project.Handler.ProjectData.Aliases.TryGetValue(aliasType, out List<AliasEntry> aliases))
        {
            foreach (var alias in aliases)
            {
                if (alias.ID != null && alias.Tags != null && alias.Tags.Contains(tag))
                {
                    ids.Add(alias.ID);
                }
            }
        }

        return ids;
    }
}