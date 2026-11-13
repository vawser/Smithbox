using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class MoveToMapAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public bool DisplayPopup = false;

    public (string, ObjectContainer) TargetMap = ("None", null);
    public (string, Entity) TargetBTL = ("None", null);

    public MoveToMapAction(MapEditorView view, ProjectEntry project)
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
        GUI.Tooltip($"Move the selected map objects into another map.");
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

        GUI.SimpleHeader("Target Map", "Target Map", "The target map to move the current selection to.", UI.Current.ImGui_Default_Text_Color);

        MapTargetPicker.Display(View, "moveTargetMapSelect", ref TargetMap);

        ImGui.Checkbox($"{LOC.Get("MAP_Keep_World_Position")}##moveKeepWorld", ref CFG.Current.MapEditor_Keep_World_Position_Across_Maps);
        GUI.Tooltip(LOC.Get("MAP_Keep_World_Position_TT"));

        var targetMap = View.Selection.ResolveTargetMap(TargetMap);

        if (targetMap == null)
        {
            GUI.WrappedText("No map has been loaded or targeted.");
        }
        else
        {

            var sel = View.ViewportSelection.GetPlacedEntities();

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
                    GUI.WrappedText("No BTL has been targeted.");
                }
            }

            if (ImGui.Button("Move##moveToMapAction", DPI.WholeWidthButton(sectionWidth, 24)))
            {
                // A BTL picked for an earlier target map belongs to that map
                var targetBtl = TargetBTL.Item2 != null && targetMap.BTLParents.Contains(TargetBTL.Item2)
                    ? TargetBTL.Item2
                    : null;

                if (targetBtl == null && sel.Any(e => e.WrappedObject is BTL.Light))
                {
                    Smithbox.LogError<MoveToMapAction>(LOC.Get("MAP_Move_No_Target_BTL"));
                }
                else
                {
                    MoveToMap(sel, targetMap, targetBtl);
                }
            }
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void MoveToMap(List<MsbEntity> selection, MapContainer targetMap, Entity targetBtl)
    {
        // Name references resolve by name, so a moved object can't take a name already used in the target map's
        // list (parts, regions or events). Moving them under new names would break the references between them,
        // so the move is refused and the clashes named.
        var clashes = selection
            .Where(e => e.Container != targetMap)
            .SelectMany(e => MsbNaming.FindClashes(targetMap, e, e.Name).Where(c => !selection.Contains(c)))
            .Select(c => c.Name)
            .Distinct()
            .ToList();

        if (clashes.Count > 0)
        {
            PlatformUtils.Instance.MessageBox(
                LOC.Get("MAP_Move_Dialog_Name_Clash", targetMap.Name, string.Join(", ", clashes.Take(10)),
                    clashes.Count > 10 ? LOC.Get("MAP_Move_Dialog_Name_Clash_More", clashes.Count - 10) : ""),
                LOC.Get("MAP_Options_Dialog_Confirm"),
                MessageBoxButtons.OK);
            return;
        }

        ViewportAction action = new EntMoveAction(View, selection, true, targetMap, targetBtl);

        // The objects stay where they are in the world, rather than at the same local coordinates in the other map
        // (another tile in ER and NR). Their frames are taken now, before the move gives them new parents.
        if (CFG.Current.MapEditor_Keep_World_Position_Across_Maps)
        {
            var frames = selection.Select(e => ((Entity)e, MapPlacement.GetParentWorld(e))).ToList();

            action = new ViewportCompoundAction(new List<ViewportAction>
            {
                action,
                new DeferredAction(() => MapPlacement.BuildKeepWorldAction(frames))
            });
        }

        // MSB references only resolve within a map, so moving breaks those between the moved objects and the
        // ones left behind, in either direction
        var moving = selection.Cast<Entity>().ToHashSet();
        var leaving = selection.Where(e => e.Container != null && e.Container != targetMap).ToList();

        var broken = leaving
            .GroupBy(e => e.Container)
            .SelectMany(g =>
                MsbReferences.FindReferencesTo(g.Key, g).Where(r => !moving.Contains(r.Slot.Referrer))
                .Concat(MsbReferences.FindReferencesFrom(g.Key, g).Where(r => !moving.Contains(r.Target))))
            .ToList();

        if (broken.Count > 0)
        {
            var result = PlatformUtils.Instance.MessageBox(
                LOC.Get("MAP_Move_Dialog_Broken_References", broken.Count, MsbReferences.Describe(broken)),
                LOC.Get("MAP_Options_Dialog_Confirm"),
                MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Cancel || result == DialogResult.None)
                return;

            // Yes clears the references along with the move, in one undo step
            if (result == DialogResult.Yes)
            {
                action = new ViewportCompoundAction(new List<ViewportAction>
                {
                    new ClearReferencesAction(broken.Select(r => r.Slot)),
                    action
                });
            }
        }

        View.ViewportActionManager.ExecuteAction(action);
    }
}