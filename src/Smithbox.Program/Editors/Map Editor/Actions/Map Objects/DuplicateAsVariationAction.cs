using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Duplicate as Variation (Session Preview, analysis doc M6): duplicates the objects with the Duplicate tool's
/// settings, then sets the placement fields on the copies, as one undo step. The field changes are made once the
/// copies exist, on the first execute, and replayed on redo.
/// </summary>
public class DuplicateAsVariationAction : ViewportAction, IMapHistoryItem
{
    private readonly EntDuplicateAction Duplicate;
    private readonly Func<IReadOnlyList<MsbEntity>, ViewportAction> MakeFieldChanges;
    private readonly string Description;
    private ViewportAction FieldChanges;

    public DuplicateAsVariationAction(EntDuplicateAction duplicate, Func<IReadOnlyList<MsbEntity>, ViewportAction> makeFieldChanges,
        string description)
    {
        Duplicate = duplicate;
        MakeFieldChanges = makeFieldChanges;
        Description = description;
    }

    public string Describe() => Description;

    public override ActionEvent Execute(bool isRedo = false)
    {
        var result = Duplicate.Execute(isRedo);

        FieldChanges ??= MakeFieldChanges(Duplicate.CreatedClones);
        FieldChanges?.Execute(isRedo);

        return result;
    }

    public override ActionEvent Undo()
    {
        FieldChanges?.Undo();
        return Duplicate.Undo();
    }
}
