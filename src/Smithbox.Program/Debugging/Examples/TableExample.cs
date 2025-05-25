using Hexa.NET.ImGui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.DebugNS;

internal class TableExample
{
    private void ExampleTable()
    {

        if (ImGui.BeginTable($"exampleTable", 3, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);

            //- Repeat for each row
            // Name
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Example");
            UIHelper.Tooltip("Example");

            ImGui.TableSetColumnIndex(1);

            // Contents

            ImGui.TableSetColumnIndex(2);

            // Action

            ImGui.EndTable();
        }

    }
}
