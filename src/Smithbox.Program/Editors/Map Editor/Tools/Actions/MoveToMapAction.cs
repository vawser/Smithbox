using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class MoveToMapAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public bool DisplayPopup = false;

    public (string, ObjectContainer) TargetMap = ("None", null);
    public (string, Entity) TargetBTL = ("None", null);

    public MoveToMapAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Update Loop
    /// </summary>
    public void OnGui()
    {
        if (DisplayPopup)
        {
            ImGui.OpenPopup("##MoveToTargetMapPopup");
            DisplayPopup = false;
        }

        if (ImGui.BeginPopup("##MoveToTargetMapPopup"))
        {
            DisplayMenu();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Move to Map"))
        {
            DisplayMenu();
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.BeginMenu("Move Selected to Map"))
        {
            DisplayMenu();

            ImGui.EndMenu();
        }
        UIHelper.Tooltip($"Move the selected map objects into another map.");
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        var windowSize = new Vector2(800f, 500f);
        var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
        var sectionHeight = windowSize.Y * 0.25f;
        var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

        UIHelper.SimpleHeader("Target Map", "Target Map", "The target map to duplicate the current selection to.", UI.Current.ImGui_Default_Text_Color);

        ImGui.BeginChild("##mapMoveSelectionSection", sectionSize, ImGuiChildFlags.Borders);

        foreach (var entry in Project.MapData.PrimaryBank.Maps)
        {
            var mapID = entry.Key.Filename;
            var map = entry.Value.MapContainer;

            if (map != null)
            {
                if (ImGui.Selectable(mapID, TargetMap.Item1 == mapID))
                {
                    TargetMap = (mapID, map);
                }

                var mapName = AliasHelper.GetMapNameAlias(Editor.Project, mapID);
                UIHelper.DisplayAlias(mapName);
            }
        }

        ImGui.EndChild();

        if (TargetMap.Item2 == null)
        {
            UIHelper.WrappedText("No map has been loaded or targeted.");
        }
        else
        {
            MapContainer targetMap = (MapContainer)TargetMap.Item2;

            var sel = Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();

            if (sel.Any(e => e.WrappedObject is BTL.Light))
            {
                if (ImGui.BeginCombo("Targeted BTL", TargetBTL.Item1))
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
                    UIHelper.WrappedText("No BTL has been targeted.");
                }
            }

            if (ImGui.Button("Duplicate##dupeToMapAction", DPI.WholeWidthButton(sectionWidth, 24)))
            {
                MoveToMap(sel, targetMap, TargetBTL.Item2);
            }
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void MoveToMap(List<MsbEntity> selection, MapContainer targetMap, Entity targetBtl)
    {
        var action = new MoveMapObjectsAction(Editor, selection, true, targetMap, targetBtl);
        Editor.EditorActionManager.ExecuteAction(action);
    }
}