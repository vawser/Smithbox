using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.AnimEditor;

public class BehaviorView : IAnimView
{
    public AnimEditorView View;
    public ProjectEntry Project;

    public BehaviorSelection Selection = new();

    public BehaviorContainerList ContainerList;
    public BehaviorFileList FileList;

    public BehaviorContents_HKX1 Contents_HKX1;
    public BehaviorContents_HKX2 Contents_HKX2;
    public BehaviorContents_HKX3 Contents_HKX3;

    public BehaviorClipGen_HKX1 ClipGen_HKX1;
    public BehaviorClipGen_HKX2 ClipGen_HKX2;
    public BehaviorClipGen_HKX3 ClipGen_HKX3;

    public BehaviorProperties_HKX1 Properties_HKX1;
    public BehaviorProperties_HKX2 Properties_HKX2;
    public BehaviorProperties_HKX3 Properties_HKX3;

    public BehaviorView(AnimEditorView view, ProjectEntry project)
    {
        View = view; 
        Project = project;

        ContainerList = new(this, project);
        FileList = new(this, project);

        Contents_HKX1 = new(this, project);
        Contents_HKX2 = new(this, project);
        Contents_HKX3 = new(this, project);

        ClipGen_HKX1 = new(this, project);
        ClipGen_HKX2 = new(this, project);
        ClipGen_HKX3 = new(this, project);

        Properties_HKX1 = new(this, project);
        Properties_HKX2 = new(this, project);
        Properties_HKX3 = new(this, project);
    }

    public void Display()
    {
        // List
        if (ImGui.Begin($@"Behavior##BehaviorListWindow{View.ViewIndex}", UIHelper.GetMainWindowFlags()))
        {
            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.AnimEditor_BehaviorList);
                View.Editor.ViewHandler.ActiveView = View;
            }

            ContainerList.Display(width, height * 0.5f);
            FileList.Display(width, height * 0.5f);
        }

        ImGui.End();

        // Workboard
        if (ImGui.Begin($@"Workboard##BehaviorWorkboardWindow{View.ViewIndex}", UIHelper.GetMainWindowFlags()))
        {
            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.AnimEditor_BehaviorWorkboard);
                View.Editor.ViewHandler.ActiveView = View;
            }

            ImGui.BeginTabBar("workboardTabs"); ;

            if(ImGui.BeginTabItem("Contents"))
            {
                if (BehaviorUtils.SupportsHKX1(Project))
                {
                    Contents_HKX1.Display();
                }
                if (BehaviorUtils.SupportsHKX2(Project))
                {
                    Contents_HKX2.Display();
                }
                if (BehaviorUtils.SupportsHKX3(Project))
                {
                    Contents_HKX3.Display();
                }

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Clip Generator"))
            {
                if (BehaviorUtils.SupportsHKX1(Project))
                {
                    ClipGen_HKX1.Display();
                }
                if (BehaviorUtils.SupportsHKX2(Project))
                {
                    ClipGen_HKX2.Display();
                }
                if (BehaviorUtils.SupportsHKX3(Project))
                {
                    ClipGen_HKX3.Display();
                }

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();

        // List
        if (ImGui.Begin($@"Properties##BehaviorPropertiesWindow{View.ViewIndex}", UIHelper.GetMainWindowFlags()))
        {
            float width = ImGui.GetContentRegionAvail().X;
            float height = ImGui.GetContentRegionAvail().Y;

            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.AnimEditor_BehaviorProperties);
                View.Editor.ViewHandler.ActiveView = View;
            }

            if (BehaviorUtils.SupportsHKX1(Project))
            {
                Properties_HKX1.Display();
            }
            if (BehaviorUtils.SupportsHKX2(Project))
            {
                Properties_HKX2.Display();
            }
            if (BehaviorUtils.SupportsHKX3(Project))
            {
                Properties_HKX3.Display();
            }

        }

        ImGui.End();
    }

    public void InvalidateContent()
    {
        if (BehaviorUtils.SupportsHKX1(Project))
        {
            Contents_HKX1.Invalidate();
            ClipGen_HKX1.Invalidate();
        }
        if (BehaviorUtils.SupportsHKX2(Project))
        {
            Contents_HKX2.Invalidate();
            ClipGen_HKX2.Invalidate();
        }
        if (BehaviorUtils.SupportsHKX3(Project))
        {
            Contents_HKX3.Invalidate();
            ClipGen_HKX3.Invalidate();
        }
    }
}
