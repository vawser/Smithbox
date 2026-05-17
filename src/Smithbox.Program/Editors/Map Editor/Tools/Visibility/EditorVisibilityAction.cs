using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using Veldrid.MetalBindings;

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
            UIHelper.Tooltip("Specific which tag the map objects will be filtered by.");

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
        UIHelper.WrappedText("Use these actions to adjust the in-editor visibility of map objects.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Selection", "Actions that affect the current selection of map objects.");

        UIHelper.MultiButtonInput("visActions",
            "toggleVis", "Toggle Visibility", "Toggles the current visibility state to the opposite.", ToggleVisLocalAction,
            "enableVis", "Make Visible", "Force the current visibility state to visible.", MakeVisibleLocalAction,
            "disableVis", "Make Invisible", "Force the current visibility state to invisible.", MakeInvisibleLocalAction);


        UIHelper.Spacer();
        UIHelper.SimpleHeader("Global", "Actions that affect all currently loaded map objects.");

        UIHelper.MultiButtonInput("globalVisActions",
            "toggleVis", "Toggle Visibility", "Toggles the current visibility state to the opposite.", ToggleVisGlobalAction,
            "enableVis", "Make Visible", "Force the current visibility state to visible.", MakeVisibleGlobalAction,
            "disableVis", "Make Invisible", "Force the current visibility state to invisible.", MakeInvisibleGlobalAction);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Target Tag", "Toggle the visibility of map objects with the associated alias tag (i.e. LOD).");

        UIHelper.HintTextInput("TagToggleInput", ref CFG.Current.Toolbar_Tag_Visibility_Target, "Enter the tag you want to target...");

        UIHelper.MultiButtonInput("tagActions",
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
            foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer == null)
                {
                    Smithbox.LogError<EditorVisibilityAction>("No map loaded.");
                    continue;
                }

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
        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
            {
                Smithbox.LogError<EditorVisibilityAction>("No map loaded.");
                continue;
            }

            foreach (Entity obj in entry.Value.MapContainer.Objects)
            {
                if (EntityHelper.IsPart(obj))
                {
                    if (Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Assets, out List<AliasEntry> assetAliases))
                    {
                        foreach (var assetEntry in assetAliases)
                        {
                            var modelName = obj.GetPropertyValue<string>("ModelName");

                            if (assetEntry.ID == modelName)
                            {
                                bool change = false;

                                foreach (var tag in assetEntry.Tags)
                                {
                                    if (tag == CFG.Current.Toolbar_Tag_Visibility_Target)
                                        change = true;
                                }

                                if (change)
                                {
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
                        }
                    }

                    if (Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.MapPieces, out List<AliasEntry> mapPieceAliases))
                    {
                        foreach (var mapPieceEntry in mapPieceAliases)
                        {
                            var entryName = $"m{mapPieceEntry.ID.Split("_").Last()}";
                            var modelName = obj.GetPropertyValue<string>("ModelName");

                            if (entryName == modelName)
                            {
                                bool change = false;

                                foreach (var tag in mapPieceEntry.Tags)
                                {
                                    if (tag == CFG.Current.Toolbar_Tag_Visibility_Target)
                                        change = true;
                                }

                                if (change)
                                {
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
                        }
                    }

                    obj.UpdateRenderModel();
                }
            }
        }
    }
}