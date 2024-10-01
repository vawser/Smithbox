using HKLib.hk2018;
using HKLib.hk2018.hkaiCollisionAvoidance;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Scene;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editor;

public class ModelContainer : ObjectContainer
{
    public Entity Mesh_RootNode { get; set; }
    public Entity Bone_RootNode { get; set; }
    public Entity DummyPoly_RootNode { get; set; }

    public Entity Collision_RootNode { get; set; }

    public ModelContainer(Universe u, string name)
    {
        Name = name;
        Universe = u;

        RootObject = new Entity(this, new ModelRootNode("Root"));
        Mesh_RootNode = new Entity(this, new ModelRootNode("Meshes"));
        Bone_RootNode = new Entity(this, new ModelRootNode("Bones"));
        DummyPoly_RootNode = new Entity(this, new ModelRootNode("Dummy Polygons"));
        Collision_RootNode = new Entity(this, new ModelRootNode("Collision"));

        RootObject.AddChild(Mesh_RootNode);
        RootObject.AddChild(Bone_RootNode);
        RootObject.AddChild(DummyPoly_RootNode);
        RootObject.AddChild(Collision_RootNode);
    }

    public void LoadCollision(hkRootLevelContainer hkx, MeshRenderableProxy proxy, string name)
    {
        // Re-build the root node on reach load
        Collision_RootNode = new Entity(this, new ModelRootNode("Collision"));

        if (Universe.IsRendering)
        {
            TaskLogs.AddLog(name);

            for (int i = 0; i < proxy.Submeshes.Count; i++)
            {
                var colType = HavokCollisionType.Low;
                if(name.Contains("_h"))
                {
                    colType = HavokCollisionType.High;
                }

                var collisionNode = new CollisionEntity(this, hkx, $@"Collision {i}", i, colType);
                collisionNode.RenderSceneMesh = proxy.Submeshes[i];
                proxy.Submeshes[i].SetSelectable(collisionNode);

                collisionNode.EditorVisible = false;

                if (colType is HavokCollisionType.High)
                {
                    if (CFG.Current.ModelEditor_ViewHighCollision)
                    {
                        collisionNode.EditorVisible = true;
                    }
                }

                if (colType is HavokCollisionType.Low)
                {
                    if (CFG.Current.ModelEditor_ViewLowCollision)
                    {
                        collisionNode.EditorVisible = true;
                    }
                }

                Objects.Add(collisionNode);
                Collision_RootNode.AddChild(collisionNode);
            }
        }
    }

    public void LoadFlver(FLVER2 flver, MeshRenderableProxy proxy)
    {
        // Meshes
        for (var i = 0; i < flver.Meshes.Count; i++)
        {
            var meshNode = new NamedEntity(this, flver.Meshes[i], $@"Mesh {i}", i);
            if (Universe.IsRendering)
            {
                if (proxy.Submeshes.Count > 0 && i < proxy.Submeshes.Count)
                {
                    meshNode.RenderSceneMesh = proxy.Submeshes[i];
                    proxy.Submeshes[i].SetSelectable(meshNode);
                }
            }

            if (!CFG.Current.ModelEditor_ViewMeshes)
            {
                meshNode.EditorVisible = false;
            }

            Objects.Add(meshNode);
            Mesh_RootNode.AddChild(meshNode);
        }

        // Bones
        for (var i = 0; i < flver.Nodes.Count; i++)
        {
            var boneNode = new TransformableNamedEntity(this, flver.Nodes[i], $"Bone {i} {{ {flver.Nodes[i].Name} }}", i);

            boneNode.RenderSceneMesh = Universe.GetBoneDrawable(this, boneNode);

            if (!CFG.Current.ModelEditor_ViewBones)
            {
                boneNode.EditorVisible = false;
            }

            Objects.Add(boneNode);
            Bone_RootNode.AddChild(boneNode);
        }

        // Dummy Polygons
        for (var i = 0; i < flver.Dummies.Count; i++)
        {
            var dummyPolyNode = new TransformableNamedEntity(this, flver.Dummies[i], $@"Dummy {i}", i);

            dummyPolyNode.RenderSceneMesh = Universe.GetDummyPolyDrawable(this, dummyPolyNode);

            if (!CFG.Current.ModelEditor_ViewDummyPolys)
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

        pos = OffsetPos(pos, flver, dummy);
        dummy.Position = pos;

        //Smithbox.EditorHandler.ModelEditor.ViewportHandler.UpdateRepresentativeDummy(index, pos);
    }

    // Offset a vector position by the rotation and translation of a bone it is attached to
    public Vector3 RecursiveBoneOffset(Vector3 pos, FLVER.Node bone, FLVER2 flver)
    {
        pos = RotateVector(pos, bone.Rotation) + bone.Position;
        if (bone.ParentIndex >= 0)
            return RecursiveBoneOffset(pos, flver.Nodes[bone.ParentIndex], flver);
        return pos;
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

