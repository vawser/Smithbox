using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public class MassEditHints
{
    private MapEditorScreen Screen;
    private MassEditHandler Handler;
    public MassEditHints(MapEditorScreen screen, MassEditHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    /// <summary>
    /// Handles the documentation popups
    /// </summary>
    public void DisplayHintPopups()
    {
        // MAP TARGET
        if (ImGui.BeginPopup("mapTargetHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.CollapsingHeader("Name", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"nameSelectionTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<string>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Select map whose name matches, or partially matches the specified string." +
                        "\nLeave blank to target all maps.");

                    ImGui.EndTable();
                }
            }

            if (ImGui.CollapsingHeader("Exclude", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"nameSelectionTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("exclude: <string>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Exclude map whose name matches, or partially matches the specified string.");

                    ImGui.EndTable();
                }
            }

            ImGui.EndPopup();
        }

        // SELECTION
        if (ImGui.BeginPopup("selectionInputHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.CollapsingHeader("Name", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"nameSelectionTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<string>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Select map objects whose name matches, or partially matches the specified string.");

                    ImGui.EndTable();
                }
            }

            if (ImGui.CollapsingHeader("Property Value", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"propValueSelectionTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("prop: <property name> [<index>] <comparator> <value>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Select map objects who possess the specified property, and where the " +
                        "\nproperty's value is equal, less than or greater than the specified value.");

                    // Parameter 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Parameters");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<property name>: the name of the property to target." +
                        "\nTarget a slot in an array property with the [] syntax.");

                    // Parameter 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<comparator>: the comparator to use." +
                        "\nAccepted symbols: =, <, >");

                    // Parameter 3
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<value>: the value to check for.");

                    // Example 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Examples");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("prop:EntityID = 1" +
                        "\nSelect all map objects with an Entity ID equal to 1.");

                    // Example 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("prop:EntityID > 1000" +
                        "\nSelect all map objects with an Entity ID equal or greater than 1000.");

                    // Example 3
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("prop:EntityGroupIDs[1] < 999" +
                        "\nSelect all map objects with an EntityGroupID (at index 1) equal or less than 999");

                    ImGui.EndTable();
                }
            }

            ImGui.EndPopup();
        }

        // EDIT
        if (ImGui.BeginPopup("editInputHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.CollapsingHeader("Basic Operations", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"basicOperationEditTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<property name> [<index>] <operation> <value>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Edit the specified property using the operation and value specified " +
                        "\nfor the map objects that meet the selection criteria.");

                    // Parameter 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Parameters");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<property name>: the name of the property to edit." +
                        "\nEdit a slot in an array property with the [] syntax.");

                    // Parameter 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<operation>: the operation to apply with the value." +
                        "\nAccepted operations: =, +, -, *, /");

                    // Parameter 3
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<value>: the value to use with the operation. " +
                        "\nIf the operation is +, -, * or /, it will conduct the operation with the " +
                        "\nexisting property value one the left-hand side.");

                    // Example 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Examples");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("EntityID = 1000" +
                        "\nSet the Entity ID field in all the map objects that meet the selection " +
                        "\ncriteria to 1000.");

                    // Example 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("EntityID + 999" +
                        "\nAdds 999 to the existing Entity ID field value of all the map objects that " +
                        "\nmeet the selection criteria.");

                    ImGui.EndTable();
                }
            }

            ImGui.EndPopup();
        }
    }
}
