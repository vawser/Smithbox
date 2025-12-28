using Hexa.NET.ImGui;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

public class CreateAction
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public List<(string, Type)> Classes = new();
    public Type CreateSelectedType;

    public CreateAction(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        // Setup the classes list, done manually since FLVER doesn't change
        Classes.Add(("Dummy", typeof(FLVER.Dummy)));
        Classes.Add(("Node", typeof(FLVER.Node)));
        Classes.Add(("Material", typeof(FLVER2.Material)));
        Classes.Add(("Mesh", typeof(FLVER2.Mesh)));

        // Not supporting:
        // - the ignored classes such as BufferLayout and GxList
        // - the sub-classes such as Texture and FaceSet

        // FIXME: newly created model objects aren't displaying their renderable mesh
        // Since this action isn't that important, I've hidden the action for now
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        //if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_CreateMapObject) && Editor.ViewportSelection.IsSelection())
        //{
        //    ApplyObjectCreation();
        //}
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        //if (ImGui.BeginMenu("Create New Object"))
        //{
        //    DisplayMenu();

        //    ImGui.EndMenu();
        //}
        //UIHelper.Tooltip($"Create a new model object.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        //if (ImGui.CollapsingHeader("Create"))
        //{
        //    DisplayMenu();
        //}
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        var windowSize = new Vector2(800f, 500f);
        var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
        var sectionHeight = windowSize.Y * 0.25f;
        var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

        var display = false;

        if(Editor.Selection.SelectedModelWrapper != null)
        {
            var container = Editor.Selection.SelectedModelWrapper.Container;

            if (container != null)
                display = true;

        }

        if (!display)
            return;

        UIHelper.SimpleHeader("Target Type", "Target Type", "", UI.Current.ImGui_Default_Text_Color);

        if (ImGui.Button("Create Object", DPI.WholeWidthButton(sectionWidth, 24)))
        {
            ApplyObjectCreation();
        }

  
        UIHelper.SimpleHeader("Type", "Type", "", UI.Current.ImGui_Default_Text_Color);

        ImGui.BeginChild("flverClassSelection", sectionSize, ImGuiChildFlags.Borders);

        foreach ((string, Type) p in Classes)
        {
            if (ImGui.RadioButton(p.Item1, p.Item2 == CreateSelectedType))
            {
                CreateSelectedType = p.Item2;
            }
        }

        ImGui.EndChild();
    }

    public void ApplyObjectCreation()
    {
        if (Editor.Selection.SelectedModelWrapper != null)
        {
            var container = Editor.Selection.SelectedModelWrapper.Container;

            if (container != null && CreateSelectedType != null)
            {
                AddNewEntity(CreateSelectedType, container);
            }
        }
    }

    private void AddNewEntity(Type typ, ModelContainer container, Entity parent = null)
    {
        if (CreateSelectedType == null)
            return;

        var newEntity = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);

        var modelObjType = ModelEntityType.Editor;

        if(CreateSelectedType == typeof(FLVER.Dummy))
        {
            modelObjType = ModelEntityType.Dummy;
        }

        if (CreateSelectedType == typeof(FLVER.Node))
        {
            modelObjType = ModelEntityType.Node;
        }

        if (CreateSelectedType == typeof(FLVER2.Material))
        {
            modelObjType = ModelEntityType.Material;
        }

        if (CreateSelectedType == typeof(FLVER2.Mesh))
        {
            modelObjType = ModelEntityType.Mesh;
        }

        var newObj = new ModelEntity(Editor, container, newEntity, modelObjType);

        var act = new AddModelObjectAction(Editor, Project, container, new List<ModelEntity> { newObj });
        Editor.EditorActionManager.ExecuteAction(act);
    }
}