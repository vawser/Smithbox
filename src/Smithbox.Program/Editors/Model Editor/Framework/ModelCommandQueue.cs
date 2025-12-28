using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class ModelCommandQueue
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelCommandQueue(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] initcmd)
    {
        // Parse select commands
        if (initcmd != null && initcmd.Length > 1)
        {
            // TODO:

            ISelectable target = null;

            if (initcmd[0] == "load")
            {
                var filename = initcmd[1];

                var entry = Project.ModelData.PrimaryBank.Models
                    .FirstOrDefault(e => e.Key.Filename.ToLower() == filename.ToLower());

                if (entry.Value != null)
                {
                    Editor.Selection.SelectedModelContainerWrapper = entry.Value;
                    entry.Value.PopulateModelList();
                }

                if (Editor.Selection.SelectedModelContainerWrapper != null)
                {
                    var firstEntry = Editor.Selection.SelectedModelContainerWrapper.Models.FirstOrDefault();

                    if (firstEntry != null)
                    {
                        if (Editor.Selection.SelectedModelWrapper != null)
                        {
                            Editor.Selection.SelectedModelWrapper.Unload();
                        }

                        Editor.Selection.SelectedModelWrapper = firstEntry;

                        Editor.EditorActionManager.Clear();

                        firstEntry.Load();
                    }
                }
            }

            // Assumes we are working in the Model Editor with the target model already loaded
            if (initcmd[0] == "select" && initcmd.Length > 2)
            {
                var type = initcmd[1];
                var value = initcmd[2];

                if (Editor.Selection.SelectedModelWrapper != null)
                {
                    var container = Editor.Selection.SelectedModelWrapper.Container;

                    if (container != null)
                    {
                        if (type == "dummy")
                        {
                            var index = int.Parse(value);

                            for (int i = 0; i < container.Dummies.Count; i++)
                            {
                                if (index == i)
                                {
                                    target = container.Dummies[i];
                                }
                            }
                        }

                        if (type == "node")
                        {
                            var index = int.Parse(value);

                            for (int i = 0; i < container.Nodes.Count; i++)
                            {
                                if (index == i)
                                {
                                    target = container.Nodes[i];
                                }
                            }
                        }

                        if (type == "mesh")
                        {
                            var index = int.Parse(value);

                            for (int i = 0; i < container.Meshes.Count; i++)
                            {
                                if (index == i)
                                {
                                    target = container.Meshes[i];
                                }
                            }
                        }

                        if (type == "material")
                        {
                            var index = int.Parse(value);

                            for (int i = 0; i < container.Materials.Count; i++)
                            {
                                if (index == i)
                                {
                                    target = container.Materials[i];
                                }
                            }
                        }
                    }
                }

                if (target != null)
                {
                    Editor.Universe.Selection.ClearSelection(Editor);
                    Editor.Universe.Selection.AddSelection(Editor, target);
                    Editor.Universe.Selection.GotoTreeTarget = target;
                }
            }
        }
    }
}

