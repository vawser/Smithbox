using ImGuiNET;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.TAE.Animation;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActAnimationPropertyView
{
    private TimeActEditorScreen Screen;
    private TimeActSelectionManager Selection;
    private TimeActDecorator Decorator;
    private TimeActPropertyEditor PropertyEditor;

    public TimeActAnimationPropertyView(TimeActEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        Decorator = screen.Decorator;
        PropertyEditor = screen.PropertyEditor;
    }

    public void Display()
    {
        ImGui.Begin("Animation Properties##TimeActAnimationProperties");
        Selection.SwitchWindowContext(TimeActEditorContext.AnimationProperty);

        if (!Selection.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }

        var anim = Selection.CurrentTimeActAnimation;

        if (Selection.CurrentTemporaryAnimHeader == null)
        {
            Selection.CurrentTemporaryAnimHeader = new TransientAnimHeader();
            Selection.CurrentTemporaryAnimHeader.CurrentType = anim.MiniHeader.Type;

            if (anim.MiniHeader.Type is MiniHeaderType.Standard)
            {
                Selection.CurrentTemporaryAnimHeader.IsLoopByDefault = anim.MiniHeader.IsLoopByDefault;
                Selection.CurrentTemporaryAnimHeader.ImportsHKX = anim.MiniHeader.ImportsHKX;
                Selection.CurrentTemporaryAnimHeader.AllowDelayLoad = anim.MiniHeader.AllowDelayLoad;
                Selection.CurrentTemporaryAnimHeader.ImportHKXSourceAnimID = anim.MiniHeader.ImportHKXSourceAnimID;
                Selection.CurrentTemporaryAnimHeader.ImportFromAnimID = 0;
                Selection.CurrentTemporaryAnimHeader.Unknown = -1;
            }
            if (anim.MiniHeader.Type is MiniHeaderType.ImportOtherAnim)
            {
                Selection.CurrentTemporaryAnimHeader.IsLoopByDefault = false;
                Selection.CurrentTemporaryAnimHeader.ImportsHKX = false;
                Selection.CurrentTemporaryAnimHeader.AllowDelayLoad = false;
                Selection.CurrentTemporaryAnimHeader.ImportHKXSourceAnimID = 0;
                Selection.CurrentTemporaryAnimHeader.ImportFromAnimID = anim.MiniHeader.ImportFromAnimID;
                Selection.CurrentTemporaryAnimHeader.Unknown = anim.MiniHeader.Unknown;
            }
        }

        PropertyEditor.AnimationHeaderSection(Selection);

        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.Columns(3);
        }
        else
        {
            ImGui.Columns(2);
        }

        // Property Column
        ImGui.AlignTextToFramePadding();
        ImGui.Text("ID");
        UIHelper.ShowHoverTooltip("ID number of this animation.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        UIHelper.ShowHoverTooltip("The name of this animation entry.");

        if (Selection.CurrentTemporaryAnimHeader != null)
        {
            if (anim.MiniHeader.Type == MiniHeaderType.Standard)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Loop by Default");
                UIHelper.ShowHoverTooltip("Makes the animation loop by default. Only relevant for animations not controlled byESD or HKS such as ObjAct animations.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Allow Delay Load");
                UIHelper.ShowHoverTooltip("Whether to allow this animation to be loaded from delayload anibnds such as the c0000_cXXXX.anibnd player throw anibnds.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Imports HKX");
                UIHelper.ShowHoverTooltip("Whether to import the HKX (actual motion data) of the animation with the ID of referenced in Import HKX Source Anim ID.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Import HKX Source Anim ID");
                UIHelper.ShowHoverTooltip("Anim ID to import HKX from.");
            }

            if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Import Anim ID");
                UIHelper.ShowHoverTooltip("ID of animation from which to import motion dat and all events.");
            }
        }

        ImGui.NextColumn();

        // Value Column
        PropertyEditor.AnimationValueSection(Selection);

        // Type Column

        // Type Column
        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("int");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("string");

            if (Selection.CurrentTemporaryAnimHeader != null)
            {
                if (anim.MiniHeader.Type == MiniHeaderType.Standard)
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("b");

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("b");

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("b");

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("long");
                }

                if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("int");
                }
            }
        }

        ImGui.Columns(1);

        ImGui.End();
    }
}


