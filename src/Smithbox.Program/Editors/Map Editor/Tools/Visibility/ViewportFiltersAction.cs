using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class ViewportFiltersAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    private bool DisplayRegionFilters = true;

    public ViewportFiltersAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        var scene = View.ViewportHandler.ActiveViewport.RenderScene;

        var objName = "Object";
        if (Project.Descriptor.ProjectType is ProjectType.ER 
            or ProjectType.AC6 
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            objName = "Asset";
        }

        GUI.WrappedText("Configure what types of map objects are rendered within the viewport.");

        GUI.Spacer();
        GUI.SimpleHeader("Basic Filters", "Toggle the display of various basic map object forms.");

        FilterCheckbox(scene, "Map Piece", RenderFilter.MapPiece);
        FilterCheckbox(scene, objName, RenderFilter.Object);
        FilterCheckbox(scene, "Character", RenderFilter.Character);

        if (Project.Descriptor.ProjectType is not (ProjectType.DS2 or ProjectType.DS2S))
        {
            FilterCheckbox(scene, "Dummy Enemy", RenderFilter.DummyEnemy);
            FilterCheckbox(scene, $"Dummy {objName}", RenderFilter.DummyAsset);
        }

        FilterCheckbox(scene, "Region", RenderFilter.Region);
        FilterCheckbox(scene, "Light", RenderFilter.Light);
        FilterCheckbox(scene, "Map Collision", RenderFilter.Collision);
        FilterCheckbox(scene, "Connect Collision", RenderFilter.ConnectCollision);
        FilterCheckbox(scene, $"{objName} Collision", RenderFilter.ObjectCollision);
        FilterCheckbox(scene, "Grass", RenderFilter.Grass);

        if (Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            FilterCheckbox(scene, "Edge Point", RenderFilter.EdgePoint);
        }

        FilterCheckbox(scene, "Navmesh", RenderFilter.Navmesh);

        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            FilterCheckbox(scene, "Nav Volume", RenderFilter.NavVolume);
            FilterCheckbox(scene, "Mission Area Limit", RenderFilter.MapLimit);
        }

        FilterCheckbox(scene, "Speed Tree", RenderFilter.SpeedTree);
        if (Project.Descriptor.ProjectType is ProjectType.ER)
        {
            FilterCheckbox(scene, "Invasion Point", RenderFilter.AutoInvade);
            FilterCheckbox(scene, "Multiplayer Wall", RenderFilter.MultiPlayerWall);
        }

        if (Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.NR)
        {
            FilterCheckbox(scene, "Fog Volume", RenderFilter.FogVolume);
        }

        if (Project.Descriptor.ProjectType is ProjectType.AC6)
        {
            DisplayNavVolumeSlice();
        }

        GUI.Spacer();
        GUI.SimpleHeader("Collision Type", "Toggle which types of collision mesh are displayed. Several can be displayed at once.");

        foreach (var curType in View.HavokCollisionBank.GetSupportedCollisionTypes())
        {
            var visible = View.HavokCollisionBank.IsCollisionTypeVisible(curType);

            if (ImGui.Checkbox($"{LOC.Get(curType.GetDisplayName())}##colType{curType}", ref visible))
            {
                View.HavokCollisionBank.ToggleCollisionType(curType);
                View.DelayPicking();
            }
        }

        if (!(Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
        {
            GUI.Spacer();
            GUI.SimpleHeader("Patrol Routes", "Toggle whether patrol routes are displayed.");

            // Read from the manager, as the filter menu and the shortcut also show and hide the routes
            var patrolVisType = View.PatrolDrawManager.IsVisible ? PatrolRouteVisType.Visible : PatrolRouteVisType.Hidden;

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##patrolVisType", patrolVisType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(PatrolRouteVisType)))
                {
                    var curType = (PatrolRouteVisType)entry;

                    if (ImGui.Selectable($"{curType.GetDisplayName()}", curType == patrolVisType))
                    {
                        if (curType is PatrolRouteVisType.Visible)
                        {
                            View.PatrolDrawManager.Generate();
                        }
                        else if (curType is PatrolRouteVisType.Hidden)
                        {
                            View.PatrolDrawManager.Clear();
                        }

                        View.DelayPicking();
                    }
                }

                ImGui.EndCombo();
            }
        }

        if (View.EffectPlayback.IsSupportedGame)
        {
            DisplayEffectPlayback();
        }

        GUI.Spacer();
        GUI.ConditionalHeader("Region Filters", "Toggle individual region types.", ref DisplayRegionFilters);

        if (DisplayRegionFilters)
        {
            View.RegionFilters.DisplayToolWindowOptions();
        }
    }

    /// <summary>
    /// The state is read from the scene each frame, as the filter menu and presets change it too
    /// </summary>
    private void FilterCheckbox(RenderScene scene, string name, RenderFilter filter)
    {
        var enabled = scene.DrawFilter.HasFlag(filter);

        if (ImGui.Checkbox($"{name}##basicFilter{filter}", ref enabled))
        {
            scene.SetDrawFilter(filter, enabled);
            View.BasicFilters.StoreFilterState();
            View.DelayPicking();
        }
    }

    /// <summary>
    /// The effects of SFX, WindSFX and Message regions: whether they are shown, which, and playback
    /// </summary>
    private void DisplayEffectPlayback()
    {
        var playback = View.EffectPlayback;
        var cfg = CFG.Current;

        GUI.Spacer();
        GUI.SimpleHeader("Effects", "Play the particle effects that map elements place, such as SFX and WindSFX regions.");

        var enabled = cfg.MapEditor_Display_Effects;
        if (ImGui.Checkbox("Display Effects##mapEffectsDisplay", ref enabled))
        {
            cfg.MapEditor_Display_Effects = enabled;

            if (enabled)
                playback.Play();
        }
        GUI.Tooltip("Play the effects of the map elements in range of the camera. This is drawn with the FX Editor's renderer, so the FX Editor must be enabled for the project.");

        if (!cfg.MapEditor_Display_Effects)
            return;

        if (!playback.IsAvailable)
        {
            GUI.WrappedText("Effects are not available: enable the FX Editor for this project to play them.");
            return;
        }

        // Start / stop
        if (playback.IsPlaying)
        {
            if (ImGui.Button("Pause##mapEffectsPause"))
                playback.Pause();
            GUI.Tooltip("Freeze the effects where they are.");
        }
        else
        {
            if (ImGui.Button($"{(playback.IsStopped ? "Start" : "Play")}##mapEffectsPlay"))
                playback.Play();
            GUI.Tooltip("Start the effects playing.");
        }

        ImGui.SameLine();
        ImGui.BeginDisabled(playback.IsStopped);
        if (ImGui.Button("Stop##mapEffectsStop"))
            playback.Stop();
        ImGui.EndDisabled();
        GUI.Tooltip("Stop the effects and remove them from the viewport.");

        ImGui.SameLine();
        if (ImGui.Button("Restart##mapEffectsRestart"))
            playback.Restart();
        GUI.Tooltip("Start every effect in range again from its beginning.");

        GUI.SetInputWidth();
        ImGui.SliderFloat("Speed##mapEffectsSpeed", ref playback.Speed, 0.1f, 4.0f, "%.2fx");
        GUI.Tooltip("The playback speed of the effects. 1 is the game's speed.");

        // Which elements
        if (ImGui.Checkbox("SFX##mapEffectsSfx", ref cfg.MapEditor_Effects_Sfx))
            playback.Invalidate();
        GUI.Tooltip("Play the effects of SFX regions.");

        if (ImGui.Checkbox("Wind SFX##mapEffectsWindSfx", ref cfg.MapEditor_Effects_WindSfx))
            playback.Invalidate();
        GUI.Tooltip("Play the effects of WindSFX regions.");

        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
        {
            if (ImGui.Checkbox("Message Sign##mapEffectsMessageSfx", ref cfg.MapEditor_Effects_MessageSfx))
                playback.Invalidate();
            GUI.Tooltip("Play the sign effect of Message regions that set one.");
        }

        if (playback.SupportsAssetEffects)
        {
            var assetName = Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT ? "Object" : "Asset";
            var paramName = Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT ? "ModelSfxParam" : "AssetModelSfxParam";

            if (ImGui.Checkbox($"{assetName} Effects##mapEffectsAssets", ref cfg.MapEditor_Effects_Assets))
                playback.Invalidate();
            GUI.Tooltip($"Play the effects a {assetName.ToLower()} places on its dummy polys through {paramName} (the row of its model, plus its SFX relative ID). A relative ID of -1 places none.");
        }

        if (ImGui.Checkbox("Selected Only##mapEffectsSelectedOnly", ref cfg.MapEditor_Effects_Selected_Only))
            playback.Invalidate();
        GUI.Tooltip("Play only the effects of the selected map elements, whatever their distance from the camera.");

        // Limits, as a map can place hundreds of effects
        GUI.SetInputWidth();
        ImGui.DragFloat("Max Distance##mapEffectsMaxDistance", ref cfg.MapEditor_Effects_Max_Distance, 1.0f, 1.0f, 5000.0f, "%.0f");
        GUI.Tooltip("Only effects within this distance of the camera are played.");

        GUI.SetInputWidth();
        ImGui.SliderInt("Max Effects##mapEffectsMaxCount", ref cfg.MapEditor_Effects_Max_Count, 1, 256);
        GUI.Tooltip("The most effects played at once; the nearest to the camera are played first.");

        // Status
        if (playback.Bank is { IsIndexing: true } bank)
        {
            GUI.WrappedText($"Indexing effects: {bank.IndexedBinders} / {bank.TotalBinders}");
        }
        else if (playback.SourceCount == 0)
        {
            GUI.WrappedText("No effect elements in the loaded maps.");
        }
        else
        {
            var status = $"{playback.SourceCount} effect elements, {playback.RunningCount} playing, {playback.ParticleCount} particles";
            if (playback.MissingCount > 0)
                status += $", {playback.MissingCount} not in the FX bank";

            GUI.WrappedText(status);
        }
    }

    private void DisplayNavVolumeSlice()
    {
        GUI.Spacer();
        GUI.SimpleHeader("Nav Volume Slice", "Cut the nav volumes at a height, hiding everything above it, to see the space that can be flown through at that height.");

        var enabled = NavVolumeSlice.Enabled;
        if (ImGui.Checkbox("Slice##navVolumeSlice", ref enabled))
        {
            NavVolumeSlice.Enabled = enabled;

            // Nothing is shown of the volume while its filter is off
            var scene = View.ViewportHandler.ActiveViewport.RenderScene;
            if (enabled && !scene.DrawFilter.HasFlag(RenderFilter.NavVolume))
            {
                scene.SetDrawFilter(RenderFilter.NavVolume, true);
                View.BasicFilters.StoreFilterState();
            }

            // Starts in the middle of the volumes, as the last height may be outside of them
            if (enabled && GetNavVolumeHeightRange() is (float low, float high)
                && (NavVolumeSlice.Height < low || NavVolumeSlice.Height > high))
            {
                NavVolumeSlice.Height = (low + high) / 2.0f;
            }

            View.DelayPicking();
        }
        GUI.Tooltip("Hide the parts of the nav volumes above the slice height.");

        if (!NavVolumeSlice.Enabled)
            return;

        var height = NavVolumeSlice.Height;
        GUI.SetInputWidth();

        if (GetNavVolumeHeightRange() is (float min, float max))
        {
            if (ImGui.SliderFloat("Height##navVolumeSliceHeight", ref height, min, max, "%.1f"))
            {
                NavVolumeSlice.Height = height;
            }
        }
        else if (ImGui.DragFloat("Height##navVolumeSliceHeight", ref height, 1.0f))
        {
            NavVolumeSlice.Height = height;
        }
        GUI.Tooltip("The world height the nav volumes are cut at.");

        if (ImGui.Button("Use Camera Height##navVolumeSliceCamera"))
        {
            NavVolumeSlice.Height = View.ViewportHandler.ActiveViewport.Viewport.ViewportCamera.CameraTransform.Position.Y;
        }
        GUI.Tooltip("Cut the nav volumes at the height of the camera.");

        var crossSection = NavVolumeSlice.ShowCrossSection;
        if (ImGui.Checkbox("Show Cross-Section##navVolumeCrossSection", ref crossSection))
        {
            NavVolumeSlice.ShowCrossSection = crossSection;
        }
        GUI.Tooltip("Draw where the slice passes through open space as a flat orange surface, with gaps where flight is blocked. It is only drawn while the camera is above the slice.");
    }

    /// <summary>
    /// The world heights covered by the loaded nav volumes, or null if none are loaded yet
    /// </summary>
    private (float Min, float Max)? GetNavVolumeHeightRange()
    {
        float? min = null;
        float? max = null;

        foreach (var entry in View.Project.Handler.MapData.PrimaryBank.Maps)
        {
            var parent = entry.Value.MapContainer?.NavVolumeParent;

            if (parent == null)
                continue;

            foreach (var child in parent.Children)
            {
                if (child.RenderSceneMesh is not MeshRenderableProxy mesh)
                    continue;

                var bounds = mesh.GetBounds();

                // The bounds are empty until the volume has loaded
                if (bounds.Max.Y <= bounds.Min.Y)
                    continue;

                min = Math.Min(min ?? bounds.Min.Y, bounds.Min.Y);
                max = Math.Max(max ?? bounds.Max.Y, bounds.Max.Y);
            }
        }

        return min is float low && max is float high ? (low, high) : null;
    }
}

public enum PatrolRouteVisType
{
    [Display(Name = "Visible")]
    Visible,
    [Display(Name = "Hidden")]
    Hidden
}