using SoulsFormats;
using System;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// Changes tags of a movie (placements, edit texts, external images, frame labels): each tag's body before and after,
/// restored with GFX.Tag.SetBytes so the tag objects (and the scene's references to them) stay the same.
/// </summary>
public class MovieTagChangeAction : EditorAction
{
    private readonly MovieContents Contents;
    private readonly List<(GFX.Tag Tag, byte[] Before, byte[] After)> Changes;

    public MovieTagChangeAction(MovieContents contents, List<(GFX.Tag Tag, byte[] Before, byte[] After)> changes)
    {
        Contents = contents;
        Changes = changes;
    }

    public bool IsEmpty => Changes.All(c => c.Before.AsSpan().SequenceEqual(c.After));

    public override ActionEvent Execute()
    {
        foreach (var (tag, _, after) in Changes)
            tag.SetBytes(after);
        Contents.MarkChanged();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var (tag, before, _) in Changes)
            tag.SetBytes(before);
        Contents.MarkChanged();
        return ActionEvent.NoEvent;
    }
}
