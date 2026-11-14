using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// A build of a map piece's GRASS file by the Model Editor's GRASS Builder: the whole file before and after, and
/// making the file (or taking back one made here) as a null side.
/// <para>
/// The file is changed in place, never swapped (<see cref="ModelGrassFileState"/>): the GRASS node and the property
/// grid hold the file and its entries, and the grid's own edits must still find their entry when undone.
/// </para>
/// </summary>
public class ModelGrassEditAction : ViewportAction, IModelHistoryItem
{
    private readonly ModelWrapper Wrapper;
    private readonly ModelGrassFileState Before;
    private readonly ModelGrassFileState After;
    private readonly string Description;

    public string Describe() => Description;

    /// <param name="after">The file as it should be, which isn't kept (its entries are copied).</param>
    /// <param name="description">What the edit did, for the History tool.</param>
    public ModelGrassEditAction(ModelWrapper wrapper, GRASS after, string description)
    {
        Wrapper = wrapper;
        Description = description;

        var current = wrapper.GRASS;

        Before = current == null ? null : ModelGrassFileState.Capture(current);

        // The file itself stays the one there is; a new one is made when there is none
        After = ModelGrassFileState.Into(current ?? new GRASS(), after);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Apply(After);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Apply(Before);
        return ActionEvent.NoEvent;
    }

    private void Apply(ModelGrassFileState state)
    {
        var container = Wrapper.Container;

        if (state == null)
        {
            // Taking back a file made here
            if (Wrapper.GRASS != null && Wrapper.IsGrassFileNew)
            {
                Wrapper.SetCreatedGrassFile(null);
                container?.SetGrassNode(null);
                MarkListsChanged();
            }

            return;
        }

        if (Wrapper.GRASS != state.File)
        {
            Wrapper.SetCreatedGrassFile(state.File);
            container?.SetGrassNode(state.File);
            MarkListsChanged();
        }

        state.Apply();
    }

    // The Containers list's grass: command counts loaded models by what they hold
    private void MarkListsChanged() => Wrapper.Parent?.Project?.Handler?.ModelData?.SidecarIndex?.MarkOverlayChanged();
}
