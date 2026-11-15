using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Application;

/// <summary>
/// The fixed area right of the editor rail that the active top-level editor fills.
/// </summary>
/// <remarks>
/// Top-level editors used to be windows docked into the main dockspace, which made them a tab bar. They are now
/// drawn one at a time into this area, and the editor rail picks which one. The window names are unchanged, so
/// each editor's own dockspace (and the layout saved for it) carries over.
/// </remarks>
public static class EditorHost
{
    private const ImGuiWindowFlags HostFlags =
        ImGuiWindowFlags.NoDocking |
        ImGuiWindowFlags.NoTitleBar |
        ImGuiWindowFlags.NoMove |
        ImGuiWindowFlags.NoResize |
        ImGuiWindowFlags.NoCollapse |
        ImGuiWindowFlags.NoBringToFrontOnFocus;

    /// <summary>Width of the editor rail, which the host area starts after</summary>
    public static float RailWidth => CFG.Current.Interface_Editor_Rail_Text_Labels
        ? EditorRail.TextRailWidth()
        : MathF.Round(ImGui.GetFrameHeight() * 2.0f);

    /// <summary>
    /// Begins a top-level editor's window over the host area. Pair with <see cref="ImGui.End"/> as for
    /// <see cref="ImGui.Begin(string, ImGuiWindowFlags)"/>.
    /// </summary>
    public static bool Begin(string name, ImGuiWindowFlags flags)
    {
        var viewport = ImGui.GetMainViewport();
        var railWidth = RailWidth;

        ImGui.SetNextWindowPos(viewport.WorkPos + new Vector2(railWidth, 0));
        ImGui.SetNextWindowSize(Vector2.Max(viewport.WorkSize - new Vector2(railWidth, 0), Vector2.One));
        ImGui.SetNextWindowViewport(viewport.ID);

        return ImGui.Begin(name, flags | HostFlags);
    }
}

/// <summary>
/// The vertical bar left of the editors with one button per top-level editor, grouped by category.
/// </summary>
public static class EditorRail
{
    // How often the unsaved markers are refreshed, as listing unsaved files can walk an editor's data
    private const double UnsavedRefreshSeconds = 1.0;

    private static readonly Dictionary<string, int> _unsavedCounts = new();
    private static double _nextUnsavedRefresh = 0;

    private static readonly Vector4 UnsavedColor = new(0.95f, 0.70f, 0.20f, 1.0f);

    /// <summary>
    /// Width of the rail when it shows editor names, fitting the longest name. Every registered editor is
    /// measured, not just the ones the project has, so the rail keeps its width between projects.
    /// </summary>
    public static float TextRailWidth()
    {
        var scale = DPI.UIScale();
        var padding = MathF.Round(4.0f * scale);
        var style = ImGui.GetStyle();

        var widest = MathF.Max(ImGui.CalcTextSize(LOC.Get("PROJECT_Editor_Title")).X,
            ImGui.CalcTextSize(LOC.Get("SETTINGS_Editor_Title")).X);

        foreach (var entry in EditorRegistry.Entries)
            widest = MathF.Max(widest, ImGui.CalcTextSize(LOC.Get(entry.LabelKey)).X);

        // Text, its frame padding, the accent bar on its left and room for the unsaved marker on its right
        var button = widest + style.FramePadding.X * 2 + AccentWidth(scale) + MarkerRadius(scale) * 4;

        return MathF.Round(button + padding * 2);
    }

    private static float AccentWidth(float scale) => MathF.Max(2.0f, 3.0f * scale);

    private static float MarkerRadius(float scale) => MathF.Max(2.0f, 3.0f * scale);

    public static unsafe void Display(ProjectOrchestrator orchestrator, EditorEntry activeEntry)
    {
        var viewport = ImGui.GetMainViewport();
        var railWidth = EditorHost.RailWidth;
        var scale = DPI.UIScale();
        var padding = MathF.Round(4.0f * scale);

        ImGui.SetNextWindowPos(viewport.WorkPos);
        ImGui.SetNextWindowSize(new Vector2(railWidth, Math.Max(viewport.WorkSize.Y, 1)));
        ImGui.SetNextWindowViewport(viewport.ID);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(padding, padding));
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, padding));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 0.0f);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0.0f);

        // GUI.ApplyBaseStyle sets a 100px minimum window size, which would widen the rail over the host area
        ImGui.PushStyleVar(ImGuiStyleVar.WindowMinSize, Vector2.One);

        var flags =
            ImGuiWindowFlags.NoDecoration |
            ImGuiWindowFlags.NoMove |
            ImGuiWindowFlags.NoDocking |
            ImGuiWindowFlags.NoSavedSettings |
            ImGuiWindowFlags.NoBringToFrontOnFocus |
            ImGuiWindowFlags.NoFocusOnAppearing |
            ImGuiWindowFlags.NoNav;

        if (ImGui.Begin("##editorRail", flags))
        {
            var buttonSize = railWidth - padding * 2;

            // Project Editor, always present
            var settingsShown = orchestrator.ActiveEditorId == EditorRegistry.SettingsEditorId;

            if (RailButton(EditorRegistry.ProjectEditorId, Icons.Home,
                    LOC.Get("PROJECT_Editor_Title"), activeEntry == null && !settingsShown, false, 0,
                    buttonSize, scale))
            {
                orchestrator.ActivateEditor(EditorRegistry.ProjectEditorId);
            }

            var project = orchestrator.SelectedProject;

            if (project is { Initialized: true, Handler: not null })
            {
                RefreshUnsavedCounts(project);

                var lastCategory = EditorCategory.Project;

                foreach (var entry in EditorRegistry.Entries)
                {
                    if (!entry.IsAvailable(project))
                        continue;

                    if (entry.Category != lastCategory)
                    {
                        ImGui.Separator();
                        lastCategory = entry.Category;
                    }

                    var loading = entry.Screen(project.Handler) == null;
                    _unsavedCounts.TryGetValue(entry.Id, out var unsaved);

                    if (RailButton(entry.Id, entry.Icon, LOC.Get(entry.LabelKey),
                            activeEntry == entry, loading, unsaved, buttonSize, scale))
                    {
                        orchestrator.ActivateEditor(entry.Id);
                    }
                }
            }

            // Settings Editor, always present and always last
            ImGui.Separator();

            if (RailButton(EditorRegistry.SettingsEditorId, Icons.Cog, LOC.Get("SETTINGS_Editor_Title"),
                    settingsShown, false, 0, buttonSize, scale))
            {
                orchestrator.ActivateEditor(EditorRegistry.SettingsEditorId);
            }

            DrawRightBorder(scale);
        }

        ImGui.End();

        ImGui.PopStyleVar(5);
    }

    /// <summary>
    /// A line down the rail's right edge, separating it from the editor. Drawn rather than using the window
    /// border, which would go round all four sides.
    /// </summary>
    private static void DrawRightBorder(float scale)
    {
        var windowPos = ImGui.GetWindowPos();
        var windowSize = ImGui.GetWindowSize();
        var thickness = MathF.Max(1.0f, MathF.Round(scale));
        var x = windowPos.X + windowSize.X - thickness;

        var drawList = ImGui.GetWindowDrawList();

        // The window's clip rect stops inside its padding, so it is widened to the whole window for the edge
        drawList.PushClipRect(windowPos, windowPos + windowSize, false);
        drawList.AddRectFilled(new Vector2(x, windowPos.Y), new Vector2(x + thickness, windowPos.Y + windowSize.Y),
            ImGui.GetColorU32(ImGuiCol.Border));
        drawList.PopClipRect();
    }

    private static unsafe bool RailButton(string id, string icon, string label, bool active, bool loading,
        int unsaved, float size, float scale)
    {
        var pushed = 0;

        if (active)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, *ImGui.GetStyleColorVec4(ImGuiCol.ButtonActive));
            pushed++;
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
            pushed++;
        }

        if (loading)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled));
            pushed++;
        }

        var textLabels = CFG.Current.Interface_Editor_Rail_Text_Labels;
        bool clicked;

        if (textLabels)
        {
            // Left-aligned name, indented past the accent bar
            var framePadding = ImGui.GetStyle().FramePadding;
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.0f, 0.5f));
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,
                new Vector2(framePadding.X + AccentWidth(scale), framePadding.Y));

            clicked = ImGui.Button($"{label}##editorRail_{id}",
                new Vector2(size, MathF.Round(ImGui.GetFrameHeight() * 1.25f)));

            ImGui.PopStyleVar(2);
        }
        else
        {
            clicked = ImGui.Button($"{icon}##editorRail_{id}", new Vector2(size, size));
        }

        ImGui.PopStyleColor(pushed);

        var min = ImGui.GetItemRectMin();
        var max = ImGui.GetItemRectMax();
        var drawList = ImGui.GetWindowDrawList();

        // Accent bar on the active editor
        if (active)
        {
            drawList.AddRectFilled(min, new Vector2(min.X + AccentWidth(scale), max.Y),
                ImGui.GetColorU32(ImGuiCol.CheckMark));
        }

        // Unsaved marker
        if (unsaved > 0)
        {
            var radius = MarkerRadius(scale);
            var markerY = textLabels ? (min.Y + max.Y) * 0.5f : min.Y + radius * 2;
            drawList.AddCircleFilled(new Vector2(max.X - radius * 2, markerY), radius,
                ImGui.GetColorU32(UnsavedColor));
        }

        // With names shown, the tooltip only has something to add while loading or unsaved
        if (ImGui.IsItemHovered() && (!textLabels || loading || unsaved > 0))
        {
            ImGui.BeginTooltip();

            if (!textLabels)
                ImGui.TextUnformatted(label);

            if (loading)
                ImGui.TextDisabled(LOC.Get("EDITOR_Editor_Is_Loading"));

            if (unsaved > 0)
                ImGui.TextColored(UnsavedColor, LOC.Get("INTERFACE_Rail_Unsaved_Files", unsaved));

            ImGui.EndTooltip();
        }

        return clicked;
    }

    private static void RefreshUnsavedCounts(ProjectEntry project)
    {
        var now = ImGui.GetTime();

        if (now < _nextUnsavedRefresh)
            return;

        _nextUnsavedRefresh = now + UnsavedRefreshSeconds;
        _unsavedCounts.Clear();

        foreach (var entry in EditorRegistry.Entries)
        {
            if (!entry.IsAvailable(project))
                continue;

            var screen = entry.Screen(project.Handler);

            if (screen == null)
                continue;

            try
            {
                var count = screen.GetUnsavedFiles()?.Count ?? 0;

                if (count > 0)
                    _unsavedCounts[entry.Id] = count;
            }
            catch (Exception)
            {
                // A marker isn't worth failing the frame over; the unsaved changes prompt still lists the files
            }
        }
    }
}
