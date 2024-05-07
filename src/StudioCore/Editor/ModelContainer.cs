using DotNext.Collections.Generic;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ModelEditor.Toolbar;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Scene;
using StudioCore.UserProject;
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
        MTD_RootNode = new Entity(this, new ModelRootNode("Materials"));
        Matbin_RootNode = new Entity(this, new ModelRootNode("Matbins"));
        Layout_RootNode = new Entity(this, new ModelRootNode("Layouts"));
        Bone_RootNode = new Entity(this, new ModelRootNode("Bones"));
        DummyPoly_RootNode = new Entity(this, new ModelRootNode("Dummy Polygons"));

        RootObject.AddChild(Mesh_RootNode);
        RootObject.AddChild(MTD_RootNode);

        if (Project.Type == ProjectType.ER || Project.Type == ProjectType.AC6)
        {
            RootObject.AddChild(Matbin_RootNode);
        }

        RootObject.AddChild(Layout_RootNode);
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
                    proxy.Submeshes[i].SetSelectable(meshNode);
                }
            }

            Objects.Add(meshNode);
            Mesh_RootNode.AddChild(meshNode);
        }

        // MTDs
        for (var i = 0; i < flver.Materials.Count; i++)
        {
            var mtd = flver.Materials[i];

            if (!MaterialDictionary.ContainsKey(i))
            {
                MaterialDictionary.Add(i, mtd.Name);
            }

            var mtdNode = new NamedEntity(this, flver.Materials[i], mtd.Name, i);
            Objects.Add(mtdNode);
            MTD_RootNode.AddChild(mtdNode);
        }

        // Matbins
        if (Project.Type == ProjectType.ER || Project.Type == ProjectType.AC6)
        {
            for (var i = 0; i < flver.Materials.Count; i++)
            {
                var mat = flver.Materials[i];

                // Only add nodes if the material uses matbin
                if (mat.MTD.Contains("matxml"))
                {
                    var matname = Path.GetFileNameWithoutExtension(mat.MTD);

                    if (MaterialResourceBank.Matbins.ContainsKey(matname))
                    {
                        MATBIN matbin = MaterialResourceBank.Matbins[matname].Matbin;

                        var name = Path.GetFileNameWithoutExtension(matbin.SourcePath);

                        var matbinNode = new NamedEntity(this, matbin, $"{name}", i);
                        Objects.Add(matbinNode);
                        Matbin_RootNode.AddChild(matbinNode);
                    }
                }
            }
        }

        // Layouts
        for (var i = 0; i < flver.BufferLayouts.Count; i++)
        {
            var layoutNode = new NamedEntity(this, flver.BufferLayouts[i], $@"Layout {i}", i);
            Objects.Add(layoutNode);
            Layout_RootNode.AddChild(layoutNode);
        }

        // Bones
        var boneEntList = new List<TransformableNamedEntity>();
        for (var i = 0; i < flver.Bones.Count; i++)
        {
            var boneNode = new TransformableNamedEntity(this, flver.Bones[i], flver.Bones[i].Name, i);

            if (CFG.Current.Model_ViewBones)
            {
                boneNode.RenderSceneMesh = Universe.GetBoneDrawable(this, boneNode);
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

            if (CFG.Current.ModelEditor_ViewDummyPolys)
            {
                dummyPolyNode.RenderSceneMesh = Universe.GetDummyPolyDrawable(this, dummyPolyNode);
            }

            Objects.Add(dummyPolyNode);
            DummyPoly_RootNode.AddChild(dummyPolyNode);
        }
    }

    // Mesh
    public void DuplicateMeshIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER2.Mesh))
        {
            FLVER2.Mesh newMesh = (FLVER2.Mesh)selected.WrappedObject;
            for(int i = 0; i < CFG.Current.ModelEditor_Toolbar_DuplicateProperty_Amount; i++)
            {
                r.Flver.Meshes.Add(newMesh);
            }
        }
    }

    // Material
    public void DuplicateMaterialIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER2.Material))
        {
            FLVER2.Material newMaterial = (FLVER2.Material)selected.WrappedObject;
            for (int i = 0; i < CFG.Current.ModelEditor_Toolbar_DuplicateProperty_Amount; i++)
            {
                r.Flver.Materials.Add(newMaterial);
            }
        }
    }

    // Layout
    public void DuplicateLayoutIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER2.BufferLayout))
        {
            FLVER2.BufferLayout newLayout = (FLVER2.BufferLayout)selected.WrappedObject;
            for (int i = 0; i < CFG.Current.ModelEditor_Toolbar_DuplicateProperty_Amount; i++)
            {
                r.Flver.BufferLayouts.Add(newLayout);
            }
        }
    }

    // Bone
    public void DuplicateBoneIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER.Bone))
        {
            FLVER.Bone newBone = (FLVER.Bone)selected.WrappedObject;
            for (int i = 0; i < CFG.Current.ModelEditor_Toolbar_DuplicateProperty_Amount; i++)
            {
                r.Flver.Bones.Add(newBone);
            }
        }
    }

    // Dummy Poly
    public void DuplicateDummyPolyIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER.Dummy))
        {
            FLVER.Dummy newDummy = (FLVER.Dummy)selected.WrappedObject;
            for (int i = 0; i < CFG.Current.ModelEditor_Toolbar_DuplicateProperty_Amount; i++)
            {
                r.Flver.Dummies.Add(newDummy);
            }
        }
    }

    // Mesh
    public void DeleteMeshIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER2.Mesh))
        {
            FLVER2.Mesh oldMesh = (FLVER2.Mesh)selected.WrappedObject;

            if(CFG.Current.ModelEditor_Toolbar_DeleteProperty_FaceSetsOnly)
            {
                oldMesh.FaceSets.Clear();
            }
            else
            {
                r.Flver.Meshes.Remove(oldMesh);
            }
        }
    }

    // Material
    public void DeleteMaterialIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER2.Material))
        {
            FLVER2.Material oldMaterial = (FLVER2.Material)selected.WrappedObject;

            r.Flver.Materials.Remove(oldMaterial);
        }
    }

    // Layout
    public void DeleteLayoutIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER2.BufferLayout))
        {
            FLVER2.BufferLayout oldLayout = (FLVER2.BufferLayout)selected.WrappedObject;

            r.Flver.BufferLayouts.Remove(oldLayout);
        }
    }


    // Bone
    public void DeleteBoneIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER.Bone))
        {
            FLVER.Bone oldBone = (FLVER.Bone)selected.WrappedObject;

            r.Flver.Bones.Remove(oldBone);
        }
    }

    // DummyPoly
    public void DeleteDummyPolyIfValid(Entity selected, FlverResource r)
    {
        if (selected.WrappedObject.GetType() == typeof(FLVER.Dummy))
        {
            FLVER.Dummy oldDummy = (FLVER.Dummy)selected.WrappedObject;

            r.Flver.Dummies.Remove(oldDummy);
        }
    }
}

