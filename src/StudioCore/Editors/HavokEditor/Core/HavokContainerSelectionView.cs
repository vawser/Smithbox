using Hexa.NET.ImGui;
using StudioCore.Editors.HavokEditor.Enums;
using StudioCore.Editors.HavokEditor.Framework;
using StudioCore.HavokEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Core;

public class HavokContainerSelectionView
{
    private HavokEditorScreen Screen;

    public HavokContainerSelectionView(HavokEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnGui()
    {
        var containerType = Screen.Selection.GetContainerType();

        ImGui.Begin("Containers##HavokContainerList");

        switch(containerType)
        {
            case HavokContainerType.Behavior:
                DisplayBehaviorList(); 
                break;
            case HavokContainerType.Collision:
                DisplayCollisionList();
                break;
            case HavokContainerType.Animation:
                DisplayAnimationList();
                break;

        }

        ImGui.End();
    }

    private void DisplayBehaviorList()
    {
        var binderKey = Screen.Selection.GetBinderKey();

        foreach (var info in HavokFileBank.BehaviorContainerBank)
        {
            if (ImGui.Selectable($@" {info.Filename}", info.Filename == binderKey))
            {
                Screen.Selection.SelectNewContainer(info);
            }
            HavokDisplayUtils.DisplaySelectableAlias(info.Filename, Smithbox.AliasCacheHandler.AliasCache.Characters);
        }
    }

    private void DisplayCollisionList()
    {
        var binderKey = Screen.Selection.GetBinderKey();

        foreach (var info in HavokFileBank.CollisionContainerBank)
        {
            if (ImGui.Selectable($@" {info.Filename}", info.Filename == binderKey))
            {
                Screen.Selection.SelectNewContainer(info);
            }
        }
    }
    private void DisplayAnimationList()
    {
        // TODO
    }
}
