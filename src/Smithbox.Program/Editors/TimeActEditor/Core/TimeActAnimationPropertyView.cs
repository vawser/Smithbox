using Hexa.NET.ImGui;
using StudioCore.Core;
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
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActAnimationPropertyView(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.Begin("Animation Properties##TimeActAnimationProperties");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.AnimationProperty);

        if (!Editor.Selection.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }

        var anim = Editor.Selection.CurrentTimeActAnimation;

        if (Editor.Selection.CurrentTemporaryAnimHeader == null)
        {
            Editor.Selection.CurrentTemporaryAnimHeader = new TransientAnimHeader();
            Editor.Selection.CurrentTemporaryAnimHeader.CurrentType = anim.MiniHeader.Type;

            if (anim.MiniHeader.Type is MiniHeaderType.Standard)
            {
                Editor.Selection.CurrentTemporaryAnimHeader.IsLoopByDefault = anim.MiniHeader.IsLoopByDefault;
                Editor.Selection.CurrentTemporaryAnimHeader.ImportsHKX = anim.MiniHeader.ImportsHKX;
                Editor.Selection.CurrentTemporaryAnimHeader.AllowDelayLoad = anim.MiniHeader.AllowDelayLoad;
                Editor.Selection.CurrentTemporaryAnimHeader.ImportHKXSourceAnimID = anim.MiniHeader.ImportHKXSourceAnimID;
                Editor.Selection.CurrentTemporaryAnimHeader.ImportFromAnimID = 0;
                Editor.Selection.CurrentTemporaryAnimHeader.Unknown = -1;
            }
            if (anim.MiniHeader.Type is MiniHeaderType.ImportOtherAnim)
            {
                Editor.Selection.CurrentTemporaryAnimHeader.IsLoopByDefault = false;
                Editor.Selection.CurrentTemporaryAnimHeader.ImportsHKX = false;
                Editor.Selection.CurrentTemporaryAnimHeader.AllowDelayLoad = false;
                Editor.Selection.CurrentTemporaryAnimHeader.ImportHKXSourceAnimID = 0;
                Editor.Selection.CurrentTemporaryAnimHeader.ImportFromAnimID = anim.MiniHeader.ImportFromAnimID;
                Editor.Selection.CurrentTemporaryAnimHeader.Unknown = anim.MiniHeader.Unknown;
            }
        }

        Editor.PropertyEditor.AnimationHeaderSection(Editor.Selection);

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
        UIHelper.Tooltip("ID number of this animation.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        UIHelper.Tooltip("The name of this animation entry.");

        if (Editor.Selection.CurrentTemporaryAnimHeader != null)
        {
            if (anim.MiniHeader.Type == MiniHeaderType.Standard)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Loop by Default");
                UIHelper.Tooltip("Makes the animation loop by default. Only relevant for animations not controlled byESD or HKS such as ObjAct animations.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Allow Delay Load");
                UIHelper.Tooltip("Whether to allow this animation to be loaded from delayload anibnds such as the c0000_cXXXX.anibnd player throw anibnds.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Imports HKX");
                UIHelper.Tooltip("Whether to import the HKX (actual motion data) of the animation with the ID of referenced in Import HKX Source Anim ID.");

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Import HKX Source Anim ID");
                UIHelper.Tooltip("Anim ID to import HKX from.");
            }

            if (anim.MiniHeader.Type == MiniHeaderType.ImportOtherAnim)
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("Import Anim ID");
                UIHelper.Tooltip("ID of animation from which to import motion dat and all events.");
            }
        }

        ImGui.NextColumn();

        // Value Column
        Editor.PropertyEditor.AnimationValueSection(Editor.Selection);

        // Type Column

        // Type Column
        if (CFG.Current.TimeActEditor_DisplayPropertyType)
        {
            ImGui.NextColumn();

            ImGui.AlignTextToFramePadding();
            ImGui.Text("int");

            ImGui.AlignTextToFramePadding();
            ImGui.Text("string");

            if (Editor.Selection.CurrentTemporaryAnimHeader != null)
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


