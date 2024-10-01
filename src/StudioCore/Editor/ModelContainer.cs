using HKLib.hk2018;
using HKLib.hk2018.hkaiCollisionAvoidance;
using HKLib.hk2018.TypeRegistryTest;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Numerics;
using static SoulsFormats.BTPB;

namespace StudioCore.Editor;

public class ModelContainer : ObjectContainer
{
    public string Name;

    public Entity Mesh_RootNode { get; set; }
    public Entity Bone_RootNode { get; set; }
    public Entity DummyPoly_RootNode { get; set; }

    public Entity HighCollision_RootNode { get; set; }
    public Entity LowCollision_RootNode { get; set; }

    public ModelContainer(Universe u, string name)
    {
        Name = name;
        Universe = u;

        RootObject = new Entity(this, new ModelRootNode("Root"));
        Mesh_RootNode = new Entity(this, new ModelRootNode("Meshes"));
        Bone_RootNode = new Entity(this, new ModelRootNode("Bones"));
        DummyPoly_RootNode = new Entity(this, new ModelRootNode("Dummy Polygons"));

        HighCollision_RootNode = new Entity(this, new ModelRootNode("High Collision"));
        LowCollision_RootNode = new Entity(this, new ModelRootNode("Low Collision"));

        RootObject.AddChild(Mesh_RootNode);
        RootObject.AddChild(Bone_RootNode);
        RootObject.AddChild(DummyPoly_RootNode);

        RootObject.AddChild(HighCollision_RootNode);
        RootObject.AddChild(LowCollision_RootNode);
    }

    public void OnGui()
    {
        if (!Universe.IsRendering)
            return;

        // Meshes
        foreach (var entry in Mesh_RootNode.Children)
        {
            entry.EditorVisible = CFG.Current.ModelEditor_ViewMeshes;
        }

        // Dummy Polygons
        foreach (var entry in DummyPoly_RootNode.Children)
        {
            entry.EditorVisible = CFG.Current.ModelEditor_ViewDummyPolys;
        }

        // Bones
        foreach (var entry in Bone_RootNode.Children)
        {
            entry.EditorVisible = CFG.Current.ModelEditor_ViewBones;
        }

        // High Collision
        foreach (CollisionEntity entry in HighCollision_RootNode.Children)
        {
            entry.EditorVisible = CFG.Current.ModelEditor_ViewHighCollision;
        }

        // Low Collision
        foreach (CollisionEntity entry in LowCollision_RootNode.Children)
        {
            entry.EditorVisible = CFG.Current.ModelEditor_ViewLowCollision;
        }
    }

    public void LoadFlver(string name, FLVER2 flver, MeshRenderableProxy flverProxy, MeshRenderableProxy lowCollisionProxy, MeshRenderableProxy highCollisionProxy)
    {
        if (!Universe.IsRendering)
            return;

        Name = name;

        // Meshes
        for (var i = 0; i < flver.Meshes.Count; i++)
        {
            var meshNode = new NamedEntity(this, flver.Meshes[i], $@"Mesh {i}", i);
            if (Universe.IsRendering)
            {
                if (flverProxy.Submeshes.Count > 0 && i < flverProxy.Submeshes.Count)
                {
                    meshNode.RenderSceneMesh = flverProxy.Submeshes[i];
                    flverProxy.Submeshes[i].SetSelectable(meshNode);
                }
            }

            Objects.Add(meshNode);
            Mesh_RootNode.AddChild(meshNode);
        }

        // Bones
        for (var i = 0; i < flver.Nodes.Count; i++)
        {
            var boneNode = new TransformableNamedEntity(this, flver.Nodes[i], $"Bone {i} {{ {flver.Nodes[i].Name} }}", i);

            boneNode.RenderSceneMesh = Universe.GetBoneDrawable(this, boneNode);

            Objects.Add(boneNode);
            Bone_RootNode.AddChild(boneNode);
        }

        // Dummy Polygons
        for (var i = 0; i < flver.Dummies.Count; i++)
        {
            var dummyPolyNode = new TransformableNamedEntity(this, flver.Dummies[i], $@"Dummy {i}", i);

            dummyPolyNode.RenderSceneMesh = Universe.GetDummyPolyDrawable(this, dummyPolyNode);

            Objects.Add(dummyPolyNode);
            DummyPoly_RootNode.AddChild(dummyPolyNode);

            //ApplyDummyOffsetting(dummyPolyNode, flver.Dummies[i], flver, i);
        }

        if (highCollisionProxy != null)
        {
            // High Collision
            for (int i = 0; i < highCollisionProxy.Submeshes.Count; i++)
            {
                if (highCollisionProxy.VirtPath.Contains("_h.hkx"))
                {
                    var collisionNode = new CollisionEntity(this, null, $@"Collision {i}", i, HavokCollisionType.High);
                    collisionNode.RenderSceneMesh = highCollisionProxy.Submeshes[i];
                    highCollisionProxy.Submeshes[i].SetSelectable(collisionNode);

                    Objects.Add(collisionNode);
                    HighCollision_RootNode.AddChild(collisionNode);
                }
            }
        }

        if (lowCollisionProxy != null)
        {
            // Low Collision
            for (int i = 0; i < lowCollisionProxy.Submeshes.Count; i++)
            {
                if (lowCollisionProxy.VirtPath.Contains("_l.hkx"))
                {
                    var collisionNode = new CollisionEntity(this, null, $@"Collision {i}", i, HavokCollisionType.Low);
                    collisionNode.RenderSceneMesh = lowCollisionProxy.Submeshes[i];
                    lowCollisionProxy.Submeshes[i].SetSelectable(collisionNode);

                    TaskLogs.AddLog($"{DateTime.Now} {collisionNode.Name} is selectable");

                    Objects.Add(collisionNode);
                    LowCollision_RootNode.AddChild(collisionNode);
                }
            }
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

