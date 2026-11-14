using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// An edit of an object's EDGE file by the Model Editor's EDGE Editor: the whole file before and after, and making the
/// file (or taking back one made here) as a null side.
/// <para>
/// The file is changed in place, never swapped (<see cref="ModelEdgeFileState"/>): the EDGE node and the property grid
/// hold the file and its entries, and the grid's own edits (a <see cref="PropChangeAction"/> on an entry) must still
/// find their entry when undone.
/// </para>
/// </summary>
public class ModelEdgeEditAction : ViewportAction, IModelHistoryItem
{
    private readonly ModelWrapper Wrapper;
    private readonly ModelEdgeFileState Before;
    private readonly ModelEdgeFileState After;
    private readonly string Description;

    public string Describe() => Description;

    /// <param name="after">The file as it should be, which isn't kept (its entries are copied); null takes the file
    /// back, which only a file made here can be.</param>
    /// <param name="description">What the edit did, for the History tool.</param>
    public ModelEdgeEditAction(ModelWrapper wrapper, EDGE after, string description)
    {
        Wrapper = wrapper;
        Description = description;

        var current = wrapper.EDGE;

        Before = current == null ? null : ModelEdgeFileState.Capture(current);

        // The file itself stays the one there is; a new one is made when there is none
        After = after == null ? null : ModelEdgeFileState.Into(current ?? new EDGE { ID = after.ID }, after);
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

    private void Apply(ModelEdgeFileState state)
    {
        var container = Wrapper.Container;

        if (state == null)
        {
            // Taking back a file made here
            if (Wrapper.EDGE != null && Wrapper.IsEdgeFileNew)
            {
                Wrapper.SetCreatedEdgeFile(null);
                container?.SetEdgeNode(null);
            }

            return;
        }

        if (Wrapper.EDGE != state.File)
        {
            Wrapper.SetCreatedEdgeFile(state.File);
            container?.SetEdgeNode(state.File);
        }

        state.Apply();
    }
}
