namespace Hexa.NET.ImGui.Widgets
{
    using Hexa.NET.ImGui.Widgets.Dialogs;
    using System.Numerics;

    public class ImGuiFileTreeView
    {
        public static bool FileTreeView(string strId, Vector2 size, ref string currentFolder, string homeFolder)
        {
            if (!ImGui.BeginChild(strId, size))
            {
                ImGui.EndChild();
                return false;
            }

            static bool Display(FileSystemItem item, ref string currentFolder, bool first = true)
            {
                if ((item.Flags & FileSystemItemFlags.Folder) == 0)
                {
                    return false;
                }

                Vector4 color = item.IsFolder && !first ? new(1.0f, 0.87f, 0.37f, 1.0f) : new(1.0f, 1.0f, 1.0f, 1.0f);
                bool isOpen = ImGui.TreeNodeEx(item.Name, ImGuiTreeNodeFlags.OpenOnArrow);
                bool changed = false;
                if (ImGui.IsItemHovered() && ImGui.IsItemClicked(ImGuiMouseButton.Left))
                {
                    currentFolder = item.Path;
                    changed = true;
                }

                if (isOpen)
                {
                    foreach (var subFolder in FileSystemHelper.GetFileSystemEntries(item.Path, RefreshFlags.Folders | RefreshFlags.Hidden, null))
                    {
                        changed |= Display(subFolder, ref currentFolder, false);
                    }

                    ImGui.TreePop();
                }

                return changed;
            }

            bool changed = false;

            ImGui.Indent();

            ImGui.Text($"{MaterialIcons.Home}");
            ImGui.SameLine();
            if (ImGui.Selectable("Home"u8, false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.NoAutoClosePopups))
            {
                currentFolder = homeFolder;
                changed = true;
            }
            ImGui.Unindent();

            ImGui.Separator();

            ImGui.Indent();
            foreach (var dir in FileSystemHelper.SpecialDirs)
            {
                ImGui.Text(dir.Icon);
                ImGui.SameLine();
                if (ImGui.Selectable(dir.Name, false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.NoAutoClosePopups))
                {
                    currentFolder = dir.Path;
                    changed = true;
                }
            }
            ImGui.Unindent();
            ImGui.Separator();

            ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 5f);
            if (ImGui.TreeNodeEx($"{MaterialIcons.Computer} Computer", ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var dir in FileSystemHelper.LogicalDrives)
                {
                    changed |= Display(dir, ref currentFolder);
                }
                ImGui.TreePop();
            }
            ImGui.PopStyleVar();

            ImGui.EndChild();

            return changed;
        }
    }
}