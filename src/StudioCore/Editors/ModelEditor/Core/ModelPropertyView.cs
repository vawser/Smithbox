using ImGuiNET;
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
using StudioCore.Core.Project;
using StudioCore.Interface;
using System.Linq;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Editors.ModelEditor.Core.Properties;
using StudioCore.Editors.ModelEditor.Enums;

namespace StudioCore.Editors.ModelEditor.Core;

public class ModelPropertyView
{
    private ModelEditorScreen Screen;
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
        Screen = screen;
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

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (!UI.Current.Interface_ModelEditor_Properties)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Properties##ModelEditorProperties"))
        {
            if (Screen.ResManager.GetCurrentFLVER() != null && !SuspendView)
            {
                var entryType = Selection._lastSelectedEntry;

                if (entryType == ModelEntrySelectionType.Header)
                {
                    FlverHeaderEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.Dummy)
                {
                    FlverDummyEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.Material)
                {
                    FlverMaterialEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.GXList)
                {
                    FlverGxListEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.Node)
                {
                    FlverNodeEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.Mesh)
                {
                    FlverMeshEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.BufferLayout)
                {
                    FlverBufferLayoutEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.BaseSkeleton)
                {
                    FlverBaseSkeletonEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.AllSkeleton)
                {
                    FlverAllSkeletonEditor.Display();
                }
                else if (entryType == ModelEntrySelectionType.CollisionLow)
                {
                    var lowCol = Screen.ResManager.LoadedFlverContainer.ER_LowCollision;
                    HavokCollisionEditor.Display(lowCol);
                }
                else if (entryType == ModelEntrySelectionType.CollisionHigh)
                {
                    var highCol = Screen.ResManager.LoadedFlverContainer.ER_HighCollision;
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
