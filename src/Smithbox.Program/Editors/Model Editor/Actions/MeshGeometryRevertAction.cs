using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Puts a mesh's geometry (vertices, faces, vertex buffers, bounds and bone table) back to vanilla's, as copied and
/// mapped to this model by <see cref="FlverDiff.CopyVanillaGeometry" />. The drawable catches up at the next live
/// preview; until then it is drawn without any gizmo moves still waiting to reach it.
/// </summary>
public class MeshGeometryRevertAction : ViewportAction, IModelHistoryItem
{
    private readonly ModelEntity Entity;
    private readonly MeshGeometry Before;
    private readonly MeshGeometry After;

    private Matrix4x4 OffsetBefore;

    public MeshGeometryRevertAction(ModelEntity entity, MeshGeometry vanilla)
    {
        Entity = entity;
        Before = MeshGeometry.Of((FLVER2.Mesh)entity.WrappedObject);
        After = vanilla;
    }

    public string Describe()
    {
        return LOC.Get("MODEL_Vanilla_History_Revert_Geometry", ModelHistory.NameObject(Entity));
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (Entity.WrappedObject is not FLVER2.Mesh mesh)
            return ActionEvent.NoEvent;

        After.ApplyTo(mesh);

        // The moves the drawable was still waiting for belong to the geometry just replaced
        OffsetBefore = Entity.DrawableOffset;
        Entity.DrawableOffset = Matrix4x4.Identity;

        Entity.InvalidateMeshPivot();
        Entity.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (Entity.WrappedObject is not FLVER2.Mesh mesh)
            return ActionEvent.NoEvent;

        Before.ApplyTo(mesh);

        Entity.DrawableOffset = OffsetBefore;

        Entity.InvalidateMeshPivot();
        Entity.UpdateRenderModel();

        return ActionEvent.NoEvent;
    }
}
