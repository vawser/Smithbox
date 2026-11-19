using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The Texture Viewer's GFx mode (Scaleform Editor, phases 2–3): a menu movie opened from the Movies tab. While a movie
/// is open the view's File List shows its display list, the Texture List its images, the Viewer the stage, and
/// Properties the selected instance. Instance names and frame labels are what the game looks elements up by, so they
/// are read-only unless renaming is unlocked. See Claude/Documentation/Scaleform Editor Analysis.md.
/// </summary>
public partial class MovieView
{
    public TexEditorView View;
    public ProjectEntry Project;

    public FileDictionaryEntry Entry { get; private set; }
    public MovieContents Contents { get; private set; }
    public MovieScene Scene { get; private set; }

    public bool IsActive => Contents != null;

    /// <summary>The selected instance, by the placements from the root to it.</summary>
    public List<GFX.PlaceObject> SelectedPath;

    /// <summary>Set when the selection changed from the stage, so the display list opens down to it.</summary>
    private bool RevealSelection;

    /// <summary>Moves apply to every keyframe of the instance's animation track (else only the keyframe shown).</summary>
    public bool MoveWholeTrack = true;

    /// <summary>Instance names and frame labels may be renamed (the game looks elements up by them).</summary>
    public bool AllowRename;

    private MovieBank Bank => Project.Handler?.TextureData?.PrimaryBank?.Movies;

    public MovieView(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Open(FileDictionaryEntry entry)
    {
        var contents = Bank?.Load(entry);
        if (contents == null)
            return;

        if (Contents != contents)
        {
            Entry = entry;
            Contents = contents;
            Scene = new MovieScene(contents);
            SelectedPath = null;
            FitPending = true;
        }
    }

    public void Close()
    {
        CancelEdit();
        Entry = null;
        Contents = null;
        Scene = null;
        SelectedPath = null;
    }

    public MovieNode Selected => Scene?.Find(SelectedPath);

    /// <summary>What the GFX mode's windows show before a movie is opened.</summary>
    private static void NoMovie()
    {
        GUI.WrappedText(LOC.Get("TEXVIEW_Movie_None_Open"));
    }

    public void Select(MovieNode node, bool reveal = false)
    {
        SelectedPath = node?.Path;
        RevealSelection = reveal;
    }

    #region Edits
    private List<(GFX.Tag Tag, byte[] Before)> Pending;

    /// <summary>Starts a live edit of these tags; EndEdit makes it one undoable action.</summary>
    public void BeginEdit(IEnumerable<GFX.Tag> tags)
    {
        CancelEdit();
        Pending = tags.Where(t => t != null).Distinct().Select(t => (t, t.GetBytes())).ToList();
    }

    public void EndEdit()
    {
        if (Pending == null || Contents == null)
            return;

        var action = new MovieTagChangeAction(Contents, Pending.Select(p => (p.Tag, p.Before, p.Tag.GetBytes())).ToList());
        Pending = null;

        if (!action.IsEmpty)
            View.ActionManager.ExecuteAction(action);
        else
            Contents.MarkDirty();
    }

    public void CancelEdit()
    {
        if (Pending == null)
            return;

        foreach (var (tag, before) in Pending)
            tag.SetBytes(before);
        Pending = null;
        Contents?.MarkDirty();
    }

    /// <summary>One undoable change of the given tags, made by 'change'.</summary>
    public void Change(IEnumerable<GFX.Tag> tags, Action change)
    {
        BeginEdit(tags);
        try
        {
            change();
        }
        catch (Exception e)
        {
            CancelEdit();
            Smithbox.LogError(this, LOC.Get("TEXVIEW_Movie_Log_Edit_Failed"), e);
            return;
        }
        EndEdit();
    }

    /// <summary>
    /// The placements a move of this instance changes: every keyframe of its track (when MoveWholeTrack), or the
    /// keyframe shown. An instance with no matrix yet gets one on the placement that created it.
    /// </summary>
    public List<GFX.PlaceObject> MoveTargets(MovieNode node)
    {
        if (node.MatrixSource == null)
            return [node.Origin];

        if (MoveWholeTrack && node.Track != null && node.Track.KeyframeCount > 0)
            return node.Track.WithMatrix.ToList();

        return [node.MatrixSource];
    }

    /// <summary>A world-space move in pixels as a move in the parent's space, in twips.</summary>
    public static (int Dx, int Dy) ToParentTwips(MovieNode node, Vector2 worldDelta)
    {
        var local = worldDelta;
        if (node.Parent != null && Matrix3x2.Invert(node.Parent.World, out var inverse))
            local = Vector2.TransformNormal(worldDelta, inverse);

        return ((int)MathF.Round(local.X * GFX.TwipsPerPixel), (int)MathF.Round(local.Y * GFX.TwipsPerPixel));
    }

    /// <summary>Translates the placements by a parent-space offset in twips (adds a matrix where there is none).</summary>
    public static void Translate(IEnumerable<GFX.PlaceObject> targets, int dx, int dy)
    {
        foreach (var p in targets)
        {
            p.Matrix ??= new GFX.Matrix();
            p.Matrix.Translate(dx, dy);
        }
    }

    /// <summary>Moves an instance by a world-space offset in pixels, as one action.</summary>
    public void Nudge(MovieNode node, Vector2 worldDelta)
    {
        var targets = MoveTargets(node);
        var (dx, dy) = ToParentTwips(node, worldDelta);
        if (dx == 0 && dy == 0)
            return;
        Change(targets, () => Translate(targets, dx, dy));
    }
    #endregion

    #region Live move (canvas drag)
    private List<(GFX.PlaceObject Place, int X, int Y, bool HadMatrix)> DragStart;
    private MovieNode DragNode;

    public bool IsDragging => DragStart != null;

    public void BeginMove(MovieNode node)
    {
        var targets = MoveTargets(node);
        BeginEdit(targets);
        DragNode = node;
        DragStart = targets.Select(p => (p, p.Matrix?.TranslateX ?? 0, p.Matrix?.TranslateY ?? 0, p.Matrix != null)).ToList();
    }

    public void UpdateMove(Vector2 worldDelta)
    {
        if (DragStart == null)
            return;

        var (dx, dy) = ToParentTwips(DragNode, worldDelta);
        foreach (var (place, x, y, hadMatrix) in DragStart)
        {
            // A click without a move leaves a placement with no matrix as it was
            if (!hadMatrix && dx == 0 && dy == 0)
            {
                place.Matrix = null;
                continue;
            }

            place.Matrix ??= new GFX.Matrix();
            place.Matrix.TranslateX = x + dx;
            place.Matrix.TranslateY = y + dy;
        }
        Contents.MarkDirty();
    }

    public void EndMove()
    {
        if (DragStart == null)
            return;
        DragStart = null;
        DragNode = null;
        EndEdit();
    }

    public void CancelMove()
    {
        DragStart = null;
        DragNode = null;
        CancelEdit();
    }
    #endregion

    /// <summary>The nearest named instance at or above a node (the game's addressable elements), else the node.</summary>
    public static MovieNode NamedOrSelf(MovieNode node)
    {
        for (var n = node; n != null; n = n.Parent)
        {
            if (!string.IsNullOrEmpty(n.Name))
                return n;
        }
        return node;
    }

    /// <summary>How many placements use the sprite whose timeline holds this instance (0 for the root timeline).</summary>
    public int SharedUses(MovieNode node)
    {
        return node.TimelineSprite == 0 ? 0 : Contents.SpriteUses.GetValueOrDefault(node.TimelineSprite);
    }
}
