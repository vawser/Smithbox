using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Linq;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The viewport of a Gparam Editor view: a map (see <see cref="GparamUniverse" />) lit by the selected GPARAM. The
/// lighting is read as the Map Editor reads it (<see cref="GparamLightingBank" />), but from the editor's copies of
/// the GPARAMs, so their unsaved edits show as they are made.
/// </summary>
public class GparamViewportWindow
{
    public GparamEditorView View;
    public ProjectEntry Project;

    public IViewport Viewport;
    public Rectangle Rect;

    public bool ViewportUsingKeyboard;

    /// <summary>The bank the lighting is read with, which reads the editor's GPARAMs before the project's files</summary>
    public GparamLightingBank LightingBank;

    /// <summary>The lighting the viewport is lit with, null if there is nothing to light it with</summary>
    public GparamLighting Lighting { get; private set; }

    /// <summary>The time of day the lighting was read at</summary>
    public float LightingHours { get; private set; }

    // What the lighting was last read for, so it is only read again when one of them changes or a value is edited
    private string LightingFile;
    private string LightingMap;
    private int LightingWeather;
    private bool LightingWeatherOutdoor;
    private bool LightingDirty = true;

    // A color being dragged changes its value without an action, so the lighting is read again while it is held,
    // this often
    private DateTime LastLiveRefresh;
    private static readonly TimeSpan LiveRefreshInterval = TimeSpan.FromMilliseconds(100);

    // The file the map was last loaded for, so selecting another value of the file doesn't load it again
    private string AutoLoadedFile;

    public GparamViewportWindow(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        LightingBank = new GparamLightingBank(project)
        {
            FileOverride = GetEditorGparam
        };

        Rect = view.Window.Bounds;

        if (view.Device != null)
        {
            if (Smithbox.Instance.CurrentBackend is RenderingBackend.Vulkan)
            {
                Viewport = new VulkanViewport(View.Universe, $"GparamViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
            }
            else
            {
                Viewport = new NullViewport(View.Universe, $"GparamViewport_{View.ViewIndex}", Rect.Width, Rect.Height, view.RenderScene);
            }
        }
    }

    /// <summary>
    /// The editor's copy of a GPARAM, if it has read it, so the viewport shows its edits
    /// </summary>
    private GPARAM GetEditorGparam(string name)
    {
        var bank = Project.Handler.GparamData?.PrimaryBank;

        if (bank == null)
            return null;

        foreach (var entry in bank.Entries)
        {
            if (entry.Value.IsLoaded() && entry.Key.Filename.Equals(name, StringComparison.OrdinalIgnoreCase))
                return entry.Value.Gparam;
        }

        return null;
    }

    /// <summary>
    /// Has the lighting read again, for a value that was edited
    /// </summary>
    public void MarkLightingDirty()
    {
        LightingDirty = true;
    }

    public void Display(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_GparamEditorView);

        if (Viewport is VulkanViewport vulkanViewport)
        {
            vulkanViewport.Display();
        }

        if (Viewport is NullViewport nullViewport)
        {
            nullViewport.Display();
        }
    }

    public void Update(float deltatime)
    {
        if (Viewport != null)
        {
            ViewportUsingKeyboard = Viewport.Update(View.Window, deltatime);
        }

        UpdateAutoLoad();

        View.Universe.Update();

        UpdateLighting();

        // The map's volumes, lightmaps and lights, with the lighting just resolved
        View.Universe.Environment.Update(Lighting, GetHours(), (Viewport as VulkanViewport)?.ViewPipeline?.Eye);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        View.Window = window;
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        Viewport.Draw(device, cl);
    }

    public bool InputCaptured()
    {
        return Viewport != null && Viewport.IsViewportSelected;
    }

    /// <summary>
    /// The GPARAM selected, null if none
    /// </summary>
    public string GetSelectedFile()
    {
        return View.Selection.SelectedFileEntry?.Filename;
    }

    /// <summary>
    /// Loads the map of a newly selected GPARAM, if that is turned on and it is a map's
    /// </summary>
    private void UpdateAutoLoad()
    {
        var file = GetSelectedFile();

        if (string.Equals(file, AutoLoadedFile, StringComparison.OrdinalIgnoreCase))
            return;

        AutoLoadedFile = file;

        if (!CFG.Current.GparamEditor_Viewport_Auto_Load_Map || !View.Universe.CanLoadMaps())
            return;

        var map = View.Universe.FindMapForFile(file);

        if (map == null)
            return;

        if (string.Equals(map, View.Universe.LoadedMapId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(map, View.Universe.LoadingMapId, StringComparison.OrdinalIgnoreCase))
            return;

        View.Universe.LoadMap(map);
    }

    /// <summary>
    /// The time of day to light at: the selected value's, when following it and its field changes through the day,
    /// else the Map Editor's, which every viewport lights at. A field with one time keeps the Map Editor's, as its
    /// value holds all day.
    /// </summary>
    public float GetHours()
    {
        if (CFG.Current.GparamEditor_Viewport_Follow_Value_Time && View.Selection.SelectedFieldValues.Count > 0 &&
            View.Selection.GetSelectedField() is { } field &&
            field.Values.Select(v => v.TimeOfDay).Distinct().Skip(1).Any())
        {
            return View.Selection.SelectedFieldValues.Values.First().TimeOfDay;
        }

        return CFG.Current.Viewport_Map_Lighting_Hours;
    }

    private void UpdateLighting()
    {
        if (!LightingBank.CanUse())
        {
            Lighting = null;
            return;
        }

        var file = GetSelectedFile();
        var map = View.Universe.LoadedMapId;
        var hours = GetHours();
        // The Map Editor's weather, as for the time of day, so both editors light a map alike
        var weather = CFG.Current.Viewport_Map_Lighting_Weather;
        var weatherOutdoor = CFG.Current.Viewport_Map_Lighting_Weather_Outdoor;

        // A shared GPARAM with no map loaded has nothing to light
        if (file == null && map == null)
        {
            Lighting = null;
            return;
        }

        var now = DateTime.UtcNow;

        // Colors are dragged without an action until they are let go of
        if (ImGui.IsAnyItemActive() && FocusManager.IsInGparamEditor() && now - LastLiveRefresh > LiveRefreshInterval)
        {
            LightingDirty = true;
        }

        if (!LightingDirty && Lighting != null && LightingHours == hours && LightingWeather == weather &&
            LightingWeatherOutdoor == weatherOutdoor &&
            string.Equals(LightingFile, file, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(LightingMap, map, StringComparison.OrdinalIgnoreCase))
            return;

        LightingDirty = false;
        LastLiveRefresh = now;

        LightingFile = file;
        LightingMap = map;
        LightingHours = hours;
        LightingWeather = weather;
        LightingWeatherOutdoor = weatherOutdoor;

        var lighting = LightingBank.Resolve(map, file, GparamLightingBank.SceneLightSetId, hours);
        lighting.Models = LightingBank.Resolve(map, file, GparamLightingBank.ModelLightSetId, hours);

        Lighting = lighting;
    }
}
