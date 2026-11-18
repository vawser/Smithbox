#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The FaceGen Editor's one view: the face list, the properties of the active face, and the rows that use it. The
/// active face (the one last clicked in the list) is held here, so every window shows the same one.
/// </summary>
public class FaceGenEditorView
{
    public FaceGenEditorScreen Editor;
    public ProjectEntry Project;

    public RenderScene? RenderScene;
    public ViewportSelection ViewportSelection = new();
    public ViewportActionManager ViewportActionManager = new();
    public FaceGenUniverse Universe;
    public FaceViewportWindow ViewportWindow;

    public FaceListView ListView;
    public FacePropertiesView PropertiesView;
    public FaceReferencesView ReferencesView;
    public FaceGenToolWindow ToolWindow;

    /// <summary>What the face's character wears in the viewport (the Model Editor's Character Preview tool)</summary>
    public ModelCharacterPreview CharacterPreview;

    /// <summary>Plays c0000's animations on the viewport's character</summary>
    public FaceGenAnimationTool AnimationTool;

    /// <summary>The face param shown, and its active row</summary>
    public string? ParamName { get; private set; }

    public Param.Row? ActiveRow { get; private set; }

    /// <summary>Field values being dragged in the Properties window, not written yet, for the viewport to show</summary>
    public Dictionary<string, int> PreviewOverrides { get; } = new();

    public FaceGenEditorView(FaceGenEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        // Before the viewport, which asks it what to wear
        CharacterPreview = new ModelCharacterPreview(project, GetCharacterPreviewTarget)
        {
            NoTargetMessage = LOC.Get("FACEGEN_CharacterPreview_No_Face"),
            SexChoiceNote = LOC.Get("FACEGEN_CharacterPreview_Sex_Note")
        };

        RenderScene = Smithbox.Instance._context.Device != null ? new RenderScene() : null;
        Universe = new FaceGenUniverse(this, project);
        ViewportWindow = new FaceViewportWindow(this, project);

        ListView = new FaceListView(this);
        PropertiesView = new FacePropertiesView(this);
        ReferencesView = new FaceReferencesView(this);
        AnimationTool = new FaceGenAnimationTool(this, project);
        ToolWindow = new FaceGenToolWindow(this);
    }

    /// <summary>
    /// What the Character Preview tool dresses: the active face's character, always c0000. The tool's choice is
    /// resolved again by the viewport, on the face's sex rather than the tool's, so the appearance it hands over is
    /// only the signal to do so.
    /// </summary>
    private CharacterPreviewTarget? GetCharacterPreviewTarget()
    {
        if (ActiveRow == null)
            return null;

        var appearance = ViewportWindow?.Appearance ?? new CharacterAppearance(Array.Empty<CharacterPart>(), null);

        return new CharacterPreviewTarget(CharacterComposition.CompositeModel, appearance, _ => ViewportWindow?.OutfitChanged());
    }

    public Param? ActiveParam => ParamName != null ? FaceGenParams.GetParam(Project, ParamName) : null;

    /// <summary>Shows a face param, without a row until one is picked</summary>
    public void ShowParam(string paramName)
    {
        if (ParamName == paramName)
            return;

        ParamName = paramName;
        ActiveRow = null;
        ListView.ClearSelection();
    }

    /// <summary>Shows a row of a face param. False if there is no such row.</summary>
    public bool Select(string paramName, int rowId)
    {
        if (!FaceGenParams.IsFaceParam(Project, paramName))
            return false;

        if (FaceGenParams.GetParam(Project, paramName)?[rowId] is not { } row)
            return false;

        Select(paramName, row);
        return true;
    }

    public void Select(string paramName, Param.Row row)
    {
        ShowParam(paramName);
        ActiveRow = row;
        ListView.SelectOnly(row);
        ListView.ScrollToSelection = true;
    }

    /// <summary>
    /// Keeps the active row valid: a reload of the params replaces every row, so the active one is taken again by
    /// its ID, and a deleted one is dropped
    /// </summary>
    private void Revalidate()
    {
        var param = ActiveParam;

        if (param == null)
        {
            ActiveRow = null;
            return;
        }

        if (ActiveRow != null && param.IndexOfRow(ActiveRow) < 0)
            ActiveRow = param[ActiveRow.ID];
    }

    // ---- windows

    public void Display(uint dockspaceId)
    {
        Revalidate();

        if (CFG.Current.Interface_FaceGenEditor_List)
            Window($"{LOC.Get("FACEGEN_Window_List")}{ListWindowId}", dockspaceId, EditorFocusContext.FaceGenEditor_List, ListView.Display);

        if (CFG.Current.Interface_FaceGenEditor_Properties)
            Window($"{LOC.Get("FACEGEN_Window_Properties")}{PropertiesWindowId}", dockspaceId, EditorFocusContext.FaceGenEditor_Properties, PropertiesView.Display);

        if (CFG.Current.Interface_FaceGenEditor_References)
            Window($"{LOC.Get("FACEGEN_Window_References")}{ReferencesWindowId}", dockspaceId, EditorFocusContext.FaceGenEditor_References, ReferencesView.Display);

        if (CFG.Current.Interface_FaceGenEditor_ToolWindow)
            Window($"{LOC.Get("FACEGEN_Window_Tools")}{ToolWindowId}", dockspaceId, EditorFocusContext.FaceGenEditor_Tools, ToolWindow.Display);

        // The viewport window sets the focus itself
        ViewportWindow.Display(dockspaceId);
    }

    // ---- frame

    public void Update(float dt)
    {
        ViewportWindow.Update(dt);

        // After the viewport, which may have made the mesh again
        AnimationTool.Update();

        // The lighting the viewport borrows, once there is something to light (as the Projectile Editor does)
        if (ViewportWindow.HasMesh)
            Project.Handler.ViewportEnvironment.Update();
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (ViewportWindow.Viewport is VulkanViewport { Visible: true })
            ViewportWindow.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        ViewportWindow.EditorResized(window, device);
    }

    public bool InputCaptured()
    {
        return ViewportWindow.InputCaptured();
    }

    public void Dispose()
    {
        AnimationTool.StopAll();
        ViewportWindow.Dispose();
    }

    public const string ListWindowId = "###faceGenEditor_List";
    public const string PropertiesWindowId = "###faceGenEditor_Properties";
    public const string ReferencesWindowId = "###faceGenEditor_References";
    public const string ToolWindowId = "###faceGenEditor_Tools";

    private static void Window(string title, uint dockspace, EditorFocusContext focus, Action draw)
    {
        ImGui.SetNextWindowDockID(dockspace, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_FaceGenEditorView);

        if (ImGui.Begin(title, GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows) ||
                ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows) && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                FocusManager.SetFocus(focus);
            }

            draw();
        }

        ImGui.End();
    }
}
