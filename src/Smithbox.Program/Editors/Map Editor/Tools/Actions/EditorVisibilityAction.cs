using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class EditorVisibilityAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public EditorVisibilityAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.Map_Visibility_FlipSelected) && Editor.ViewportSelection.IsSelection())
        {
            ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_Visibility_EnableSelected) && Editor.ViewportSelection.IsSelection())
        {
            ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_Visibility_DisableSelected) && Editor.ViewportSelection.IsSelection())
        {
            ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_Visibility_FlipAll))
        {
            ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_Visibility_EnableAll))
        {
            ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.Map_Visibility_DisableAll))
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
            if (ImGui.MenuItem("Flip Visibility for Selected", KeyBindings.Current.Map_Visibility_FlipSelected.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }

            if (ImGui.MenuItem("Enable Visibility for Selected", KeyBindings.Current.MAP_Visibility_EnableSelected.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }

            if (ImGui.MenuItem("Disable Visibility for Selected", KeyBindings.Current.MAP_Visibility_DisableSelected.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
            }

            if (ImGui.MenuItem("Flip Visibility for All", KeyBindings.Current.MAP_Visibility_FlipAll.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
            }

            if (ImGui.MenuItem("Enable Visibility for All", KeyBindings.Current.MAP_Visibility_EnableAll.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
            }

            if (ImGui.MenuItem("Disable Visibility for All", KeyBindings.Current.Map_Visibility_DisableAll.HintText))
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
            if (ImGui.MenuItem("Flip Visibility for Selected", KeyBindings.Current.Map_Visibility_FlipSelected.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Flip);
            }

            if (ImGui.MenuItem("Enable Visibility for Selected", KeyBindings.Current.MAP_Visibility_EnableSelected.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Enable);
            }

            if (ImGui.MenuItem("Disable Visibility for Selected", KeyBindings.Current.MAP_Visibility_DisableSelected.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.Selected, EditorVisibilityState.Disable);
            }

            if (ImGui.MenuItem("Flip Visibility for All", KeyBindings.Current.MAP_Visibility_FlipAll.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Flip);
            }

            if (ImGui.MenuItem("Enable Visibility for All", KeyBindings.Current.MAP_Visibility_EnableAll.HintText))
            {
                ApplyEditorVisibilityChange(EditorVisibilityType.All, EditorVisibilityState.Enable);
            }

            if (ImGui.MenuItem("Disable Visibility for All", KeyBindings.Current.Map_Visibility_DisableAll.HintText))
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
        var windowWidth = ImGui.GetWindowWidth();

        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyEditorVisibilityChange(EditorVisibilityType targetType, EditorVisibilityState targetState)
    {
        if (targetType == EditorVisibilityType.Selected)
        {
            HashSet<Entity> selected = Editor.ViewportSelection.GetFilteredSelection<Entity>();

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
            foreach (var entry in Project.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer == null)
                {
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
    }
    public void ApplyEditorVisibilityChangeByTag()
    {
        foreach (var entry in Project.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
            {
                continue;
            }

            foreach (Entity obj in entry.Value.MapContainer.Objects)
            {
                if (obj.IsPart())
                {
                    if (Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Assets, out List<AliasEntry> assetAliases))
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

                    if (Project.CommonData.Aliases.TryGetValue(ProjectAliasType.MapPieces, out List<AliasEntry> mapPieceAliases))
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

                    obj.UpdateRenderModel(Editor);
                }
            }
        }
    }
}