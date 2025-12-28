using SoulsFormats.KF4;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;
using static SoulsFormats.DDS;

namespace StudioCore.Editors.ModelEditor;

public class AddModelObjectAction : ViewportAction
{
    private readonly ModelEditorScreen Editor;
    private readonly ProjectEntry Project;

    private readonly ModelContainer Container;
    private readonly List<ModelEntity> Added = new();

    public AddModelObjectAction(ModelEditorScreen editor, ProjectEntry project, ModelContainer container, List<ModelEntity> objects)
    {
        Editor = editor;
        Project = project;

        Container = container;

        Added.AddRange(objects);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        for (var i = 0; i < Added.Count(); i++)
        {
            Container.Objects.Add(Added[i]);
            Container.RootObject.AddChild(Added[i]);
            Added[i].UpdateRenderModel(Editor);

            if (Added[i].RenderSceneMesh != null)
            {
                Added[i].RenderSceneMesh.SetSelectable(Added[i]);

                Added[i].RenderSceneMesh.AutoRegister = true;
                Added[i].RenderSceneMesh.Register();
            }
        }

        Editor.ViewportSelection.ClearSelection(Editor);
        foreach (ModelEntity c in Added)
        {
            Editor.ViewportSelection.AddSelection(Editor, c);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Added.Count(); i++)
        {
            Container.Objects.Remove(Added[i]);
            if (Added[i] != null)
            {
                Added[i].Parent.RemoveChild(Added[i]);
            }

            if (Added[i].RenderSceneMesh != null)
            {
                Added[i].RenderSceneMesh.AutoRegister = false;
                Added[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        Editor.ViewportSelection.ClearSelection(Editor);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }

}
