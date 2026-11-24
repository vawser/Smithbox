#nullable enable
using Havok.Shared;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The Behavior Editor's viewport (Havok behaviors): a character that plays the clips of the behavior selection.
/// Selecting a clip generator, selector, state or state machine lists the animations of the clips under it, and
/// plays the first. The character is picked in the Character Selector tool, or follows the selected behbnd (each is
/// named after its character); c0000 is dressed by the Character Preview tool.
/// Built on the shared layers only: a RenderScene and VulkanViewport, chr models through ModelLocator and the
/// ResourceManager, and Havok clips through AnimationPreviewPanel / ModelAnimator. Nothing here touches the behavior.
/// </summary>
public class BehaviorViewer : IDisposable
{
    private const int ClipListRows = 8;
    private const int CharacterListRows = 12;

    public HavokEditorScreen Editor { get; }
    public ProjectEntry Project { get; }

    public BehaviorUniverse Universe { get; }
    public IViewport? Viewport { get; private set; }

    /// <summary>Plays the character's clips (shared with the other editors' animation previews)</summary>
    public AnimationPreviewPanel Panel { get; }

    public BehaviorCharacter? Character { get; private set; }

    /// <summary>What c0000 wears</summary>
    public ModelCharacterPreview CharacterPreview { get; }

    private bool FramePending;

    public BehaviorViewer(HavokEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Universe = new BehaviorUniverse(project);
        Panel = new AnimationPreviewPanel(project);

        CharacterPreview = new ModelCharacterPreview(project, GetCharacterPreviewTarget)
        {
            NoTargetMessage = LOC.Get("BHV_Viewer_Preview_No_Target", CharacterComposition.CompositeModel)
        };
    }

    private CharacterPreviewTarget? GetCharacterPreviewTarget()
    {
        if (Character is not { IsComposite: true } character)
            return null;

        return new CharacterPreviewTarget(character.Name, character.Appearance!, appearance =>
        {
            // The character may have been swapped since the target was made
            if (Character == character)
            {
                character.SetAppearance(appearance);
                FramePending = true;
            }
        });
    }

    private void EnsureViewport()
    {
        if (Viewport != null)
            return;

        var rect = Smithbox.Instance._context.Window.Bounds;

        Viewport = Smithbox.Instance.HasRenderer
            ? new VulkanViewport(Universe, "BehaviorEditorViewport", rect.Width, rect.Height, Universe.RenderScene)
            : new NullViewport(Universe, "BehaviorEditorViewport", rect.Width, rect.Height, Universe.RenderScene);

        Universe.Viewport = Viewport;
    }

    #region Characters
    private List<string>? _characters;

    /// <summary>The project's characters (chr IDs with a chrbnd), from the file dictionary</summary>
    public List<string> Characters => _characters ??= Project.Locator.FileDictionary.Entries
        .Where(e => string.Equals(e.Extension, "chrbnd", StringComparison.OrdinalIgnoreCase) &&
                    e.Filename.StartsWith('c') && e.Filename.Length == 5)
        .Select(e => e.Filename.ToLowerInvariant())
        .Distinct()
        .OrderBy(n => n, StringComparer.Ordinal)
        .ToList();

    private HashSet<string>? _characterSet;

    private HashSet<string> CharacterSet => _characterSet ??= new HashSet<string>(Characters, StringComparer.OrdinalIgnoreCase);

    private Dictionary<string, string>? _characterAliases;

    private string GetCharacterLabel(string chr)
    {
        if (_characterAliases == null)
        {
            _characterAliases = new(StringComparer.OrdinalIgnoreCase);

            if (Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Characters, out var aliases) && aliases != null)
            {
                foreach (var alias in aliases)
                {
                    if (!string.IsNullOrEmpty(alias.ID) && !string.IsNullOrEmpty(alias.Name))
                        _characterAliases.TryAdd(alias.ID, alias.Name);
                }
            }
        }

        return _characterAliases.TryGetValue(chr, out var name) ? $"{chr} <{name}>" : chr;
    }

    /// <summary>Shows a character (null for none), playing what the last one was playing</summary>
    public void SetCharacter(string? name)
    {
        if (string.Equals(Character?.Name, name, StringComparison.OrdinalIgnoreCase))
            return;

        var replay = Character?.RequestedAnimation ?? Clips.FirstOrDefault().Animation;

        Character?.Dispose();
        Character = null;

        if (name != null)
        {
            EnsureViewport();

            Character = new BehaviorCharacter(this, name,
                CharacterComposition.IsComposite(name) ? CharacterPreview.GetAppearance() : null);

            if (replay != null)
                Character.Play(replay);

            FramePending = true;
        }
    }

    // The behbnd the character was last loaded for, so a binder only loads its character once it's selected
    private string? LastBinder;

    private void UpdateAutoLoad()
    {
        if (!CFG.Current.BehaviorEditor.BehaviorEditor_Viewport_Auto_Load_Character)
            return;

        var binder = Editor.ViewHandler.ActiveView?.Selection.BinderFileEntry?.Filename;

        if (binder == null || string.Equals(binder, LastBinder, StringComparison.OrdinalIgnoreCase))
            return;

        LastBinder = binder;

        // Not every behbnd has a character model of its name
        if (CharacterSet.Contains(binder))
            SetCharacter(binder.ToLowerInvariant());
    }
    #endregion

    #region Selection clips
    /// <summary>The animations of the clips under the behavior selection, with the clip generators naming them</summary>
    public List<(string Animation, string Clip)> Clips { get; private set; } = new();

    private object? ClipsSelection;
    private object? ClipsLastAction;
    private HavokEditorView? ClipsView;

    /// <summary>
    /// The clips are gathered again when the selection changes, or anything is edited (a clip's animation, or the
    /// generators under a state). A new selection plays its first clip.
    /// </summary>
    private void UpdateClips(HavokEditorView view)
    {
        var behaviorView = view.PropertyView.BehaviorView;
        var selected = behaviorView.IsShown ? behaviorView.GetSelectedObject() : null;
        var lastAction = view.ActionManager.PeekUndoAction();

        var selectionChanged = !ReferenceEquals(selected, ClipsSelection) || view != ClipsView;

        if (!selectionChanged && ReferenceEquals(lastAction, ClipsLastAction))
            return;

        ClipsSelection = selected;
        ClipsLastAction = lastAction;
        ClipsView = view;

        Clips = GatherClips(view, selected);

        if (selectionChanged && CFG.Current.BehaviorEditor.BehaviorEditor_Viewport_Play_On_Select && Clips.Count > 0)
            Character?.Play(Clips[0].Animation);
    }

    private static List<(string Animation, string Clip)> GatherClips(HavokEditorView view, object? selected)
    {
        var result = new List<(string, string)>();

        if (selected == null)
            return result;

        var generators = selected is IClipGenerator clip
            ? new List<IClipGenerator> { clip }
            : HavokTreeSearch.FindAll<IClipGenerator>(selected, view.PropertyCache.GetCachedHavokFields);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var generator in generators)
        {
            if (HavokBehaviorSymbols.GetValue(generator, "m_animationName") is not string animation ||
                string.IsNullOrWhiteSpace(animation) || !seen.Add(animation))
                continue;

            result.Add((animation, HavokTypeUtils.GetName(generator) ?? ""));
        }

        return result;
    }
    #endregion

    #region Windows
    /// <summary>
    /// The Viewport and Animation windows, docked with the view's others. Submitted from the first view only (the
    /// viewer is shared), while the clips follow the active view's selection.
    /// </summary>
    public void Display(uint dockspaceId, HavokEditorView view)
    {
        if (view != Editor.ViewHandler.Views.FirstOrDefault(v => v != null))
            return;

        var activeView = Editor.ViewHandler.ActiveView ?? view;
        UpdateClips(activeView);

        if (!CFG.Current.Interface.Interface_BehaviorEditor_Viewport)
        {
            if (Viewport is VulkanViewport hidden)
                hidden.Visible = false;

            return;
        }

        EnsureViewport();

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref Editor.ViewDockGroup);

        if (Viewport is VulkanViewport vulkan)
            vulkan.Display();
        else if (Viewport is NullViewport nullViewport)
            nullViewport.Display();

        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref Editor.ViewDockGroup);

        if (ImGui.Begin($"{LOC.Get("BHV_Viewer_Window_Animation")}###{Editor.IdPrefix}_Animation", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                Editor.ViewHandler.ActiveView = activeView;

            DisplayAnimationWindow();
        }

        ImGui.End();
    }

    private void DisplayAnimationWindow()
    {
        // Character
        if (Character == null)
        {
            GUI.WrappedText(LOC.Get("BHV_Viewer_No_Character"));
        }
        else
        {
            GUI.WrappedText(LOC.Get("BHV_Viewer_Character", GetCharacterLabel(Character.Name)));

            if (Character.RequestedAnimation is { } requested)
            {
                if (Character.IsAnimationMissing)
                    GUI.WrappedText(LOC.Get("BHV_Viewer_Animation_Missing", requested, Character.Name));
                else if (Character.IsLoading)
                    GUI.WrappedText(LOC.Get("BHV_Viewer_Animation_Loading"));
            }
        }

        // Options
        var playOnSelect = CFG.Current.BehaviorEditor.BehaviorEditor_Viewport_Play_On_Select;
        if (ImGui.Checkbox($"{LOC.Get("BHV_Viewer_Play_On_Select")}##behaviorViewerPlayOnSelect", ref playOnSelect))
            CFG.Current.BehaviorEditor.BehaviorEditor_Viewport_Play_On_Select = playOnSelect;
        GUI.Tooltip(LOC.Get("BHV_Viewer_Play_On_Select_TT"));

        ImGui.SameLine();

        var rootMotion = CFG.Current.BehaviorEditor.BehaviorEditor_Viewport_Root_Motion;
        if (ImGui.Checkbox($"{LOC.Get("BHV_Viewer_Root_Motion")}##behaviorViewerRootMotion", ref rootMotion))
            CFG.Current.BehaviorEditor.BehaviorEditor_Viewport_Root_Motion = rootMotion;
        GUI.Tooltip(LOC.Get("BHV_Viewer_Root_Motion_TT"));

        if (Character != null)
        {
            ImGui.SameLine();

            if (ImGui.Button($"{LOC.Get("BHV_Viewer_Frame")}##behaviorViewerFrame"))
                FramePending = true;
            GUI.Tooltip(LOC.Get("BHV_Viewer_Frame_TT"));
        }

        GUI.Spacer();

        // The behavior selection's clips
        GUI.SimpleHeader(LOC.Get("BHV_Viewer_Selection_Clips", Clips.Count), LOC.Get("BHV_Viewer_Selection_Clips_TT"));

        if (Clips.Count == 0)
        {
            GUI.WrappedText(LOC.Get("BHV_Viewer_Selection_Clips_None"));
        }
        else
        {
            var rows = Math.Min(Clips.Count, ClipListRows);
            var height = ImGui.GetTextLineHeightWithSpacing() * rows + ImGui.GetStyle().FramePadding.Y * 2;

            if (ImGui.BeginChild("behaviorViewerClips", new Vector2(-1, height), ImGuiChildFlags.Borders))
            {
                var clipper = new ImGuiListClipper();
                clipper.Begin(Clips.Count);

                while (clipper.Step())
                {
                    for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                    {
                        var (animation, clip) = Clips[i];
                        var current = string.Equals(Character?.RequestedAnimation, animation, StringComparison.OrdinalIgnoreCase);

                        if (ImGui.Selectable($"{animation}##behaviorViewerClip{i}", current))
                            Character?.Play(animation);

                        if (clip.Length > 0)
                        {
                            ImGui.SameLine();
                            ImGui.TextDisabled(clip);
                        }
                    }
                }

                clipper.End();
            }

            ImGui.EndChild();
        }

        GUI.Spacer();

        // The playback controls, and every animation the character has
        if (Character != null)
        {
            if (!Panel.IsSupported)
                GUI.WrappedText(LOC.Get("BHV_Viewer_Unsupported_Game"));
            else
                Panel.Display(Character.AnimationTarget);
        }
    }

    /// <summary>The Character Selector tool: picks the character the clips play on</summary>
    public void DisplayCharacterSelector()
    {
        if (!ToolSections.Header($"{LOC.Get("BHV_Viewer_Character_Selector_Header")}##behaviorCharacterSelector"))
            return;

        var autoLoad = CFG.Current.BehaviorEditor.BehaviorEditor_Viewport_Auto_Load_Character;
        if (ImGui.Checkbox($"{LOC.Get("BHV_Viewer_Auto_Load")}##behaviorViewerAutoLoad", ref autoLoad))
        {
            CFG.Current.BehaviorEditor.BehaviorEditor_Viewport_Auto_Load_Character = autoLoad;

            // Turned on: the selected behbnd's character comes in now
            LastBinder = null;
        }
        GUI.Tooltip(LOC.Get("BHV_Viewer_Auto_Load_TT"));

        GUI.WrappedText(Character != null
            ? LOC.Get("BHV_Viewer_Character", GetCharacterLabel(Character.Name))
            : LOC.Get("BHV_Viewer_Character_None"));

        if (Character != null)
        {
            if (ImGui.Button($"{LOC.Get("BHV_Viewer_Unload")}##behaviorViewerUnload"))
                SetCharacter(null);
            GUI.Tooltip(LOC.Get("BHV_Viewer_Unload_TT"));
        }

        GUI.Spacer();

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##behaviorCharacterFilter", LOC.Get("BHV_Viewer_Character_Filter"), ref CharacterFilter, 255);

        if (ImGui.BeginListBox("##behaviorCharacterList", new Vector2(-1, ImGui.GetTextLineHeightWithSpacing() * CharacterListRows)))
        {
            foreach (var chr in Characters)
            {
                var label = GetCharacterLabel(chr);

                if (CharacterFilter.Length > 0 && !label.Contains(CharacterFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                var selected = string.Equals(Character?.Name, chr, StringComparison.OrdinalIgnoreCase);

                if (ImGui.Selectable($"{label}##behaviorCharacter_{chr}", selected))
                    SetCharacter(chr);
            }

            ImGui.EndListBox();
        }
    }

    private string CharacterFilter = "";

    /// <summary>The Character Preview tool: what c0000 wears</summary>
    public void DisplayCharacterPreview()
    {
        CharacterPreview.OnToolWindow();
    }
    #endregion

    #region Screen hooks
    public void Update(float dt)
    {
        UpdateAutoLoad();

        Panel.Update();
        Character?.Update();

        if (Viewport != null)
            Viewport.Update(Smithbox.Instance._context.Window, dt);

        // The map lighting the viewport borrows, once there is something to light
        if (Character != null)
            Project.Handler.ViewportEnvironment.Update();

        if (FramePending)
            TryFrame();
    }

    /// <summary>Frames the character once its meshes have loaded (their bounds are known only then)</summary>
    private void TryFrame()
    {
        if (Character == null)
        {
            FramePending = false;
            return;
        }

        if (Character.TryGetBounds(out var bounds))
        {
            Viewport?.FitBox(bounds);
            FramePending = false;
        }
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        if (Viewport is VulkanViewport { Visible: true } vulkan)
            vulkan.Draw(device, cl);
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
    }

    public bool InputCaptured() => Viewport?.IsViewportSelected ?? false;

    public void Dispose()
    {
        Character?.Dispose();
        Character = null;
        Panel.StopAll();
        (Viewport as VulkanViewport)?.Dispose();
        Viewport = null;
        GC.SuppressFinalize(this);
    }
    #endregion
}
