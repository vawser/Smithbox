using Hexa.NET.ImGui;
using StudioCore.Editors.MapEditor.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.DebugNS;

/// <summary>
/// Example of a Tree node that recurses for all child nodes
/// </summary>
//internal class TreeNodeExample
//{
//    private void RootNode()
//    {
//        Entity RootObject = new Entity(); // Ignore this

//        ImGui.Begin("ExampleWindow");

//        if (ImGui.CollapsingHeader("Root Node"))
//        {
//            ImGui.PushID("RootNode");

//            var open = ImGui.TreeNodeEx(RootObject.Name, ImGuiTreeNodeFlags.DefaultOpen);
//            if (open)
//            {
//                for (var i = 0; i < RootObject.Children.Count; i++)
//                {
//                    var childObject = RootObject.Children[i];
//                    ChildNode(childObject, i);
//                }

//                ImGui.TreePop();
//            }

//            ImGui.PopID();
//        }

//        ImGui.End();
//    }

//    private void ChildNode(Entity curObject, int index)
//    {
//        ImGui.PushID($"{curObject.Name}{index}");

//        var open = ImGui.TreeNodeEx(curObject.Name, ImGuiTreeNodeFlags.DefaultOpen);
//        if (open)
//        {
//            for (var i = 0; i < curObject.Children.Count; i++)
//            {
//                var childObject = curObject.Children[i];
//                ImGui.PushID(i);

//                var childOpen = ImGui.TreeNodeEx($"{childObject.Name}{i}", ImGuiTreeNodeFlags.DefaultOpen);

//                if (childOpen)
//                {
//                    ChildNode(childObject, i);
//                    ImGui.TreePop();
//                }

//                ImGui.PopID();
//            }

//            ImGui.TreePop();
//        }

//        ImGui.PopID();
//    }
//}
