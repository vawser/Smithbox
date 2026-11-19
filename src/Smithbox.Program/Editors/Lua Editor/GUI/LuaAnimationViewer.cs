using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Lua Editor's character viewport: a Time Act Editor view it hosts (only its viewport is shown), so a battle
/// act's animations play with the Time Act Editor's own model loading, clips, root motion and events. It needs the
/// Time Act Editor enabled for the project. A character can be open in one view at a time (views share its model),
/// so one open in a Time Act Editor view can't be picked here, and the reverse.
/// </summary>
public class LuaAnimationViewer
{
    private readonly LuaEditorScreen Editor;

    // A view index the Time Act Editor's own views won't reach, so the viewport's ImGui ID is unique
    private const int HostedViewIndex = 7000;

    public TimeActEditorView View { get; private set; }

    /// <summary>The last thing the viewer did or couldn't do, for a line under the character box</summary>
    public string Status { get; private set; }

    public LuaAnimationViewer(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    private TimeActEditorScreen TimeActEditor => Editor.Project.Handler.TimeActEditor;

    /// <summary>The project's characters with time acts; empty without the Time Act Editor</summary>
    public List<TimeActContainerWrapper> Characters =>
        Editor.Project.Handler.TimeActData?.PrimaryBank?.GetCharacters() ?? new();

    public bool IsAvailable => TimeActEditor != null && Editor.Project.Handler.TimeActData?.PrimaryBank != null;

    public TimeActContainerWrapper Character => View?.Selection.SelectedTimeActContainerWrapper;

    /// <summary>Makes the hosted view on first use (its viewport holds GPU resources)</summary>
    private bool EnsureView()
    {
        if (View != null)
            return true;
        if (!IsAvailable)
            return false;

        View = new TimeActEditorView(TimeActEditor, Editor.Project, HostedViewIndex)
        {
            HostIsFocused = () => FocusManager.Focus == EditorFocusContext.LuaEditor_Viewport,
            HostFocus = () => FocusManager.SetFocus(EditorFocusContext.LuaEditor_Viewport),
        };
        TimeActEditor.ViewHandler.ExternalViews.Add(View);
        return true;
    }

    public void SelectCharacter(TimeActContainerWrapper source)
    {
        if (source == null || !EnsureView() || source == Character)
            return;

        if (TimeActEditor.ViewHandler.FindViewWithSource(source, View) is { } other)
        {
            Status = LOC.Get("LUA_Viewer_Open_In_Time_Act_Editor", source.Name, other.ViewIndex);
            return;
        }

        View.SourceWindow.SelectSource(source);
        Status = null;
    }

    /// <summary>The act's animations the character has, in order (see <see cref="LuaAct.AnimationCandidates"/>)</summary>
    public List<long> AnimationsOf(LuaAct act)
    {
        var source = Character;
        if (source == null)
            return new();
        return act.AnimationCandidates().Select(c => c.Id).Distinct()
            .Where(id => source.TryFindAnimation(id, out _, out _)).ToList();
    }

    /// <summary>Plays the act's animations one after another on the character</summary>
    public void Play(LuaAct act)
    {
        var source = Character;
        if (source == null)
        {
            Status = LOC.Get("LUA_Viewer_No_Character");
            return;
        }

        var ids = AnimationsOf(act);
        if (ids.Count == 0)
        {
            var wanted = act.AnimationCandidates();
            Status = wanted.Count == 0
                ? LOC.Get("LUA_Viewer_Act_No_Animation", act.Id)
                : LOC.Get("LUA_Viewer_Animations_Missing", act.Id, source.Name,
                    string.Join(", ", wanted.Select(w => source.FormatAnimationName(w.Id)).Distinct()));
            return;
        }

        View.ComboBuilderTool.PlaySequence(ids);
        Status = LOC.Get("LUA_Viewer_Playing", act.Id, string.Join(" → ", ids.Select(source.FormatAnimationName)));
    }

    // --- the screen's hooks ---

    // The dockspace inside the viewport's display window, which the viewport docks into
    private uint ViewportDockspace;

    /// <summary>
    /// Laid out as a Time Act Editor view: a display window (no background, so the viewport rendered beneath the
    /// windows shows through; no nav) docked with the Lua Editor's windows, holding a dockspace the viewport window
    /// docks into
    /// </summary>
    public void Display(uint dockspaceId)
    {
        if (!EnsureView())
            return;

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_LuaEditorView);
        if (ImGui.Begin($"{LOC.Get("LUA_Window_Viewport")}###luaEditor_ViewportHost", GUI.GetDisplayViewWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(EditorFocusContext.LuaEditor_Viewport);

            ViewportDockspace = ImGui.GetID("DockSpace_LuaEditor_Viewport");
            ImGui.DockSpace(ViewportDockspace, Vector2.Zero, ImGuiDockNodeFlags.None, ref GUI.DockGroup_LuaEditorViewport);
        }
        ImGui.End();

        // Drawn every frame (also when the display window is a hidden tab), as this is where the playback advances
        View.DisplayHosted(() =>
        {
            ImGui.SetNextWindowDockID(ViewportDockspace, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_LuaEditorViewport);
        });
    }

    public void Update(float dt)
    {
        if (View == null)
            return;
        View.ViewportWindow.Update(dt);

        // The map lighting the viewport borrows, as the Time Act Editor does once a model is loaded
        if (View.Universe.HasLoadedModel)
            Editor.Project.Handler.ViewportEnvironment.Update();
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (View?.ViewportWindow.Viewport is VulkanViewport { Visible: true })
            View.ViewportWindow.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device) => View?.ViewportWindow.EditorResized(window, device);

    public bool InputCaptured() => View?.ViewportWindow.InputCaptured() ?? false;
}
