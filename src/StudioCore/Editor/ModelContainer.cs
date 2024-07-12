using DotNext.Collections.Generic;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ModelEditor.Toolbar;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using static SoulsFormats.BTPB;
using static SoulsFormats.MSB_AC6.Region;

namespace StudioCore.Editor;

public class ModelContainer : ObjectContainer
{
    public Entity Mesh_RootNode { get; set; }
    public Entity MTD_RootNode { get; set; }
    public Entity Matbin_RootNode { get; set; }
    public Entity Layout_RootNode { get; set; }
    public Entity Bone_RootNode { get; set; }
    public Entity DummyPoly_RootNode { get; set; }

    public Dictionary<int, string> MaterialDictionary = new Dictionary<int, string>();

    public ModelContainer()
    {
    }

    public ModelContainer(Universe u, string name)
    {
        Name = name;
        Universe = u;

        RootObject = new Entity(this, new ModelRootNode("Root"));
        Mesh_RootNode = new Entity(this, new ModelRootNode("Meshes"));
        Bone_RootNode = new Entity(this, new ModelRootNode("Bones"));
        DummyPoly_RootNode = new Entity(this, new ModelRootNode("Dummy Polygons"));

        RootObject.AddChild(Mesh_RootNode);
        RootObject.AddChild(Bone_RootNode);
        RootObject.AddChild(DummyPoly_RootNode);
    }

    public void LoadFlver(FLVER2 flver, MeshRenderableProxy proxy)
    {
        //ModelEditorScreen.UpdateLoadedRenderMesh();

        MaterialDictionary.Clear();

        // Meshes
        for (var i = 0; i < flver.Meshes.Count; i++)
        {
            var meshNode = new NamedEntity(this, flver.Meshes[i], $@"Mesh {i}", i);
            if (Universe.IsRendering)
            {
                if (proxy.Submeshes.Count > 0 && i < proxy.Submeshes.Count)
                {
                    meshNode.RenderSceneMesh = proxy.Submeshes[i];
                    //proxy.Submeshes[i].SetSelectable(meshNode);
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
        var boneEntList = new List<TransformableNamedEntity>();
        for (var i = 0; i < flver.Bones.Count; i++)
        {
            var boneNode = new TransformableNamedEntity(this, flver.Bones[i], $"Bone {i} {{ {flver.Bones[i].Name} }}", i);

            boneNode.RenderSceneMesh = Universe.GetBoneDrawable(this, boneNode);

            if (!CFG.Current.ModelEditor_ViewBones)
            {
                boneNode.EditorVisible = false;
            }

            Objects.Add(boneNode);
            boneEntList.Add(boneNode);
        }

        for (var i = 0; i < flver.Bones.Count; i++)
        {
            if (flver.Bones[i].ParentIndex == -1)
            {
                Bone_RootNode.AddChild(boneEntList[i]);
            }
            else
            {
                boneEntList[flver.Bones[i].ParentIndex].AddChild(boneEntList[i]);
            }
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
        }
    }
}

