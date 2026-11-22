using Havok.Shared;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// An edit of a model's collision by the Model Editor's Collision tool: each body given a rebuilt shape (a commit of
/// moved vertices or repainted faces, or a mesh built from the FLVER), and layers whose file the tool made. Undoing
/// gives the bodies their old shapes back and takes the made files back. Both mark the files as changed, so the
/// model's save writes them, and call back so what is drawn follows.
/// </summary>
public class ModelCollisionAction : ViewportAction, IModelHistoryItem
{
    public record ShapeSwap(ModelCollisionFile File, IHavokBody Body, object OldShape, object NewShape);

    private readonly ModelWrapper Wrapper;
    private readonly List<ShapeSwap> Swaps;
    private readonly List<ModelCollisionFile> NewFiles;
    private readonly Action OnChanged;
    private readonly string Description;

    public string Describe() => Description;

    public ModelCollisionAction(ModelWrapper wrapper, List<ShapeSwap> swaps, List<ModelCollisionFile> newFiles,
        Action onChanged, string description)
    {
        Wrapper = wrapper;
        Swaps = swaps;
        NewFiles = newFiles ?? new List<ModelCollisionFile>();
        OnChanged = onChanged;
        Description = description;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var file in NewFiles)
        {
            file.IsModified = true;
            Wrapper.CollisionFiles.SetNewFile(file.Type, file);
        }

        foreach (var swap in Swaps)
        {
            CollisionEditorShapes.SetShape(swap.Body, swap.NewShape);
            swap.File.IsModified = true;
        }

        OnChanged?.Invoke();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var swap in Swaps)
        {
            CollisionEditorShapes.SetShape(swap.Body, swap.OldShape);
            swap.File.IsModified = true;
        }

        foreach (var file in NewFiles)
        {
            Wrapper.CollisionFiles.SetNewFile(file.Type, null);
        }

        OnChanged?.Invoke();

        return ActionEvent.NoEvent;
    }
}
