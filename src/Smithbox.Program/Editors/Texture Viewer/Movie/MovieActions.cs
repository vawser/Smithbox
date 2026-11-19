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

/// <summary>
/// Shows an item icon in movies (Item Icon Injector, ItemIconPages): swaps the edited tag lists, and adds (or on
/// undo removes) the new image character in each movie's character table. The preview forgets its image lookups,
/// so cells added in the same action are found.
/// </summary>
public class MovieIconInjectAction : EditorAction
{
    private readonly List<(MovieContents Contents, ItemIconEdit Edit)> Edits;
    private readonly MovieImages Images;

    public MovieIconInjectAction(List<(MovieContents Contents, ItemIconEdit Edit)> edits, MovieImages images)
    {
        Edits = edits;
        Images = images;
    }

    public override ActionEvent Execute()
    {
        foreach (var (contents, edit) in Edits)
        {
            edit.Apply();
            if (edit.AddedImage != null)
                contents.Characters[(int)edit.AddedImage.Id] = edit.AddedImage;
            contents.MarkChanged();
        }

        Images?.Reset();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var (contents, edit) in Edits)
        {
            edit.Revert();
            if (edit.AddedImage != null)
                contents.Characters.Remove((int)edit.AddedImage.Id);
            contents.MarkChanged();
        }

        Images?.Reset();
        return ActionEvent.NoEvent;
    }
}
