using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static SoulsFormats.PARAM;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using System.Linq;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Editors.ModelEditor.Core.Properties;
using StudioCore.Editors.ModelEditor.Enums;
using StudioCore.Core;
using StudioCore.Configuration;

namespace StudioCore.Editors.ModelEditor.Core;

public class ModelPropertyView
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    private InternalFilePropertyView InternalFileEditor;
    private FlverHeaderPropertyView FlverHeaderEditor;
    private FlverDummyPropertyView FlverDummyEditor;
    private FlverMaterialPropertyView FlverMaterialEditor;
    private FlverGxListPropertyView FlverGxListEditor;
    private FlverNodePropertyView FlverNodeEditor;
    private FlverMeshPropertyView FlverMeshEditor;
    private FlverBufferLayoutPropertyView FlverBufferLayoutEditor;
    private FlverBaseSkeletonPropertyView FlverBaseSkeletonEditor;
    private FlverAllSkeletonPropertyView FlverAllSkeletonEditor;
    private HavokCollisionPropertyView HavokCollisionEditor;


    private bool SuspendView = false;

    public ModelPropertyView(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;

        // Views
        InternalFileEditor = new InternalFilePropertyView(screen);
        FlverHeaderEditor = new FlverHeaderPropertyView(screen);
        FlverDummyEditor = new FlverDummyPropertyView(screen);
        FlverMaterialEditor = new FlverMaterialPropertyView(screen);
        FlverGxListEditor = new FlverGxListPropertyView(screen);
        FlverNodeEditor = new FlverNodePropertyView(screen);
        FlverMeshEditor = new FlverMeshPropertyView(screen);
        FlverBufferLayoutEditor = new FlverBufferLayoutPropertyView(screen);
        FlverBaseSkeletonEditor = new FlverBaseSkeletonPropertyView(screen);
        FlverAllSkeletonEditor = new FlverAllSkeletonPropertyView(screen);
        HavokCollisionEditor = new HavokCollisionPropertyView(screen);
    }

    public void Display()
    {
        var scale = DPI.GetUIScale();

        if (!CFG.Current.Interface_ModelEditor_Properties)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Properties##ModelEditorProperties"))
        {
            Selection.SwitchWindowContext(ModelEditorContext.ModelProperties);

            if (Editor.ResManager.GetCurrentFLVER() != null && !SuspendView)
            {
                var entryType = Selection._selectedFlverGroupType;

                if (entryType == GroupSelectionType.Header)
                {
                    FlverHeaderEditor.Display();
                }
                else if (entryType == GroupSelectionType.Dummy)
                {
                    FlverDummyEditor.Display();
                }
                else if (entryType == GroupSelectionType.Material)
                {
                    FlverMaterialEditor.Display();
                }
                else if (entryType == GroupSelectionType.GXList)
                {
                    FlverGxListEditor.Display();
                }
                else if (entryType == GroupSelectionType.Node)
                {
                    FlverNodeEditor.Display();
                }
                else if (entryType == GroupSelectionType.Mesh)
                {
                    FlverMeshEditor.Display();
                }
                else if (entryType == GroupSelectionType.BufferLayout)
                {
                    FlverBufferLayoutEditor.Display();
                }
                else if (entryType == GroupSelectionType.BaseSkeleton)
                {
                    FlverBaseSkeletonEditor.Display();
                }
                else if (entryType == GroupSelectionType.AllSkeleton)
                {
                    FlverAllSkeletonEditor.Display();
                }
                else if (entryType == GroupSelectionType.CollisionLow)
                {
                    var lowCol = Editor.ResManager.LoadedFlverContainer.ER_LowCollision;
                    HavokCollisionEditor.Display(lowCol);
                }
                else if (entryType == GroupSelectionType.CollisionHigh)
                {
                    var highCol = Editor.ResManager.LoadedFlverContainer.ER_HighCollision;
                    HavokCollisionEditor.Display(highCol);
                }
                else
                {
                    InternalFileEditor.Display();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
