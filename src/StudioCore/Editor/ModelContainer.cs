using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MaterialEditor;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace StudioCore.Editor;

public class ModelContainer : ObjectContainer
{
    public NamedEntity meshesNode;
    public NamedEntity materialsNode;
    public NamedEntity matbinsNode;
    public NamedEntity layoutsNode;
    public NamedEntity bonesNode;
    public NamedEntity dmysNode;

    public Dictionary<int, string> MaterialDictionary = new Dictionary<int, string>();

    public ModelContainer()
    {
    }

    public ModelContainer(Universe u, string name)
    {
        Name = name;
        Universe = u;
        RootObject = new Entity(this, new MapTransformNode());
    }

    public void LoadFlver(FLVER2 flver, MeshRenderableProxy proxy)
    {
        MaterialDictionary.Clear();

        // Meshes
        meshesNode = new NamedEntity(this, null, "Meshes", 0);
        Objects.Add(meshesNode);
        RootObject.AddChild(meshesNode);
        for (var i = 0; i < flver.Meshes.Count; i++)
        {
            var meshnode = new NamedEntity(this, flver.Meshes[i], $@"Mesh {i}", i);
            if (proxy.Submeshes.Count > 0)
            {
                meshnode.RenderSceneMesh = proxy.Submeshes[i];
                proxy.Submeshes[i].SetSelectable(meshnode);
            }

            Objects.Add(meshnode);
            meshesNode.AddChild(meshnode);
        }

        // Materials
        materialsNode = new NamedEntity(this, null, "Materials", 0);
        Objects.Add(materialsNode);
        RootObject.AddChild(materialsNode);
        for (var i = 0; i < flver.Materials.Count; i++)
        {
            var mat = flver.Materials[i];

            if (!MaterialDictionary.ContainsKey(i))
            {
                MaterialDictionary.Add(i, mat.Name);
            }

            var matnode = new NamedEntity(this, flver.Materials[i], mat.Name, i);
            Objects.Add(matnode);
            materialsNode.AddChild(matnode);
        }

        // Matbin
        if (Project.Type == ProjectType.ER || Project.Type == ProjectType.AC6)
        {
            matbinsNode = new NamedEntity(this, null, "Matbin (Read-only)", 0);
            Objects.Add(matbinsNode);
            RootObject.AddChild(matbinsNode);
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

                        var matbinnode = new NamedEntity(this, matbin, $"{name}", i);
                        Objects.Add(matbinnode);
                        matbinsNode.AddChild(matbinnode);
                    }
                }
            }
        }

        // Layouts
        layoutsNode = new NamedEntity(this, null, "Layouts", 0);
        Objects.Add(layoutsNode);
        RootObject.AddChild(layoutsNode);
        for (var i = 0; i < flver.BufferLayouts.Count; i++)
        {
            var laynode = new NamedEntity(this, flver.BufferLayouts[i], $@"Layout {i}", i);
            Objects.Add(laynode);
            layoutsNode.AddChild(laynode);
        }

        // Bones
        bonesNode = new NamedEntity(this, null, "Bones", 0);
        Objects.Add(bonesNode);
        RootObject.AddChild(bonesNode);
        var boneEntList = new List<TransformableNamedEntity>();
        for (var i = 0; i < flver.Bones.Count; i++)
        {
            var bonenode =
                new TransformableNamedEntity(this, flver.Bones[i], flver.Bones[i].Name, i);

            if (CFG.Current.Model_ViewBones)
                bonenode.RenderSceneMesh = Universe.GetBoneDrawable(this, bonenode);

            Objects.Add(bonenode);
            boneEntList.Add(bonenode);
        }

        for (var i = 0; i < flver.Bones.Count; i++)
        {
            if (flver.Bones[i].ParentIndex == -1)
            {
                bonesNode.AddChild(boneEntList[i]);
            }
            else
            {
                boneEntList[flver.Bones[i].ParentIndex].AddChild(boneEntList[i]);
            }
        }

        // Dummy Polygons
        dmysNode = new NamedEntity(this, null, "DummyPolys", 0);
        Objects.Add(dmysNode);
        RootObject.AddChild(dmysNode);
        for (var i = 0; i < flver.Dummies.Count; i++)
        {
            var dmynode = new TransformableNamedEntity(this, flver.Dummies[i], $@"Dummy {i}", i);

            if (CFG.Current.ModelEditor_ViewDummyPolys)
                dmynode.RenderSceneMesh = Universe.GetDummyPolyDrawable(this, dmynode);

            Objects.Add(dmynode);
            dmysNode.AddChild(dmynode);
        }
    }
}

