using SoulsFormats;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ModelEditor;
using StudioCore.Scene.Framework;
using StudioCore.Scene.Helpers;
using System.Numerics;

namespace StudioCore.Editor;

public class ModelContainer : ObjectContainer
{
    public ModelEditorScreen Editor;

    public ModelUniverse Universe { get; set; }

    public Entity Mesh_RootNode { get; set; }
    public Entity Bone_RootNode { get; set; }
    public Entity DummyPoly_RootNode { get; set; }

    public ModelContainer(ModelEditorScreen editor, ModelUniverse u, FLVER2 flver, MeshRenderableProxy flverProxy)
    {
        Universe = u;

        RootObject = new Entity(Editor, this, new ModelRootNode("Root"));
        Mesh_RootNode = new Entity(Editor, this, new ModelRootNode("Meshes"));
        Bone_RootNode = new Entity(Editor, this, new ModelRootNode("Bones"));
        DummyPoly_RootNode = new Entity(Editor, this, new ModelRootNode("Dummy Polygons"));

        RootObject.AddChild(Mesh_RootNode);
        RootObject.AddChild(Bone_RootNode);
        RootObject.AddChild(DummyPoly_RootNode);

        if (!CFG.Current.Viewport_Enable_Rendering)
            return;

        // Meshes
        for (var i = 0; i < flver.Meshes.Count; i++)
        {
            var meshNode = new NamedEntity(Editor, this, flver.Meshes[i], $@"Mesh {i}", i);
            if (CFG.Current.Viewport_Enable_Rendering)
            {
                if (flverProxy.Submeshes.Count > 0 && i < flverProxy.Submeshes.Count)
                {
                    meshNode.RenderSceneMesh = flverProxy.Submeshes[i];
                    flverProxy.Submeshes[i].SetSelectable(meshNode);
                }
            }

            if (CFG.Current.ModelEditor_ViewMeshes)
            {
                meshNode.EditorVisible = true;
            }
            else
            {
                meshNode.EditorVisible = false;
            }

            Objects.Add(meshNode);
            Mesh_RootNode.AddChild(meshNode);
        }

        // Bones
        for (var i = 0; i < flver.Nodes.Count; i++)
        {
            var boneNode = new TransformableNamedEntity(Editor, this, flver.Nodes[i], $"Bone {i} {{ {flver.Nodes[i].Name} }}", i);

            boneNode.RenderSceneMesh = DrawableHelper.GetBoneDrawable(Universe.RenderScene, this, boneNode);
            if(CFG.Current.ModelEditor_ViewDummyPolys)
            {
                boneNode.EditorVisible = true;
            }
            else
            {
                boneNode.EditorVisible = false;
            }

            Objects.Add(boneNode);
            Bone_RootNode.AddChild(boneNode);
        }

        // Dummy Polygons
        for (var i = 0; i < flver.Dummies.Count; i++)
        {
            var dummyPolyNode = new TransformableNamedEntity(Editor, this, flver.Dummies[i], $@"Dummy {i}", i);

            dummyPolyNode.RenderSceneMesh = DrawableHelper.GetDummyPolyDrawable(Universe.RenderScene, this, dummyPolyNode);

            if (CFG.Current.ModelEditor_ViewDummyPolys)
            {
                dummyPolyNode.EditorVisible = true;
            }
            else
            {
                dummyPolyNode.EditorVisible = false;
            }

            Objects.Add(dummyPolyNode);
            DummyPoly_RootNode.AddChild(dummyPolyNode);

            //ApplyDummyOffsetting(dummyPolyNode, flver.Dummies[i], flver, i);
        }
    }

    public void ApplyDummyOffsetting(TransformableNamedEntity ent, FLVER.Dummy dummy, FLVER2 flver, int index)
    {
        var pos = new Vector3(dummy.Position.X, dummy.Position.Y, dummy.Position.Z);

        Vector3 OffsetPos(Vector3 pos, FLVER2 flver, FLVER.Dummy dummy)
        {
            if (dummy.ParentBoneIndex >= 0)
                return RecursiveBoneOffset(pos, flver.Nodes[dummy.ParentBoneIndex], flver);

            return pos;
        }
    // Offset a vector position by the rotation and translation of a bone it is attached to

        pos = OffsetPos(pos, flver, dummy);
        dummy.Position = pos;

        //Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeDummy(index, pos);
    }

    public Vector3 RecursiveBoneOffset(Vector3 translation, FLVER.Node bone, FLVER2 flver)
    {
        translation = RotateVector(translation, bone.Rotation) + bone.Translation;
        if (bone.ParentIndex >= 0)
            return RecursiveBoneOffset(translation, flver.Nodes[bone.ParentIndex], flver);
        return translation;
    }

    // Rotation logic taken from cannon.js:
    // https://github.com/schteppe/cannon.js/blob/master/src/math/Quaternion.js#L249
    public Vector3 RotateVector(Vector3 v, Vector3 r)
    {
        var quat = Quaternion.CreateFromYawPitchRoll(r.X, r.Y, r.Z);

        var target = new Vector3();

        var x = v.X;
        var y = v.Y;
        var z = v.Z;

        var qx = quat.X;
        var qy = quat.Y;
        var qz = quat.Z;
        var qw = quat.W;

        // q*v
        var ix = qw * x + qy * z - qz * y;
        var iy = qw * y + qz * x - qx * z;
        var iz = qw * z + qx * y - qy * x;
        var iw = -qx * x - qy * y - qz * z;

        target.X = ix * qw + iw * -qx + iy * -qz - iz * -qy;
        target.Y = iy * qw + iw * -qy + iz * -qx - ix * -qz;
        target.Z = iz * qw + iw * -qz + ix * -qy - iy * -qx;

        return target;
    }
}

