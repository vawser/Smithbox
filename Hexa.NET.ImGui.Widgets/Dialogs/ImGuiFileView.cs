namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;
    using Hexa.NET.Utilities.Text;
    using System.Buffers;
    using System.Numerics;
    using System.Runtime.CompilerServices;

    public abstract class ImGuiFileView<T> where T : struct, IFileSystemItem
    {
        public abstract string CurrentFolder { get; set; }

        public virtual unsafe bool FileView(string strId, Vector2 size, List<T> entries, bool reorder = false)
        {
            ImGuiTableFlags flags =
                ImGuiTableFlags.Reorderable |
                ImGuiTableFlags.Resizable |
                ImGuiTableFlags.Hideable |
                ImGuiTableFlags.Sortable |
                ImGuiTableFlags.SizingFixedFit |
                ImGuiTableFlags.ScrollX |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.PadOuterX | ImGuiTableFlags.ContextMenuInBody | ImGuiTableFlags.NoSavedSettings;
            var avail = ImGui.GetContentRegionAvail();

            if (!ImGui.BeginTable(strId, 4, flags, size))
            {
                return false;
            }

            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortDescending);
            ImGui.TableSetupColumn("Date Modified", ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Type", ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Size", ImGuiTableColumnFlags.None);

            ImGui.TableSetupScrollFreeze(0, 1);

            ImGui.TableHeadersRow();

            ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();

            if (!sortSpecs.IsNull && (sortSpecs.SpecsDirty || reorder))
            {
                int sortColumnIndex = sortSpecs.Specs.ColumnIndex;
                bool ascending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending;
                IComparer<T> comparer;
                if (ascending)
                {
                    comparer = sortColumnIndex switch
                    {
                        0 => new AscendingComparer<T, CompareByNameComparer<T>>(),
                        1 => new AscendingComparer<T, CompareByDateModifiedComparer<T>>(),
                        2 => new AscendingComparer<T, CompareByTypeComparer<T>>(),
                        3 => new AscendingComparer<T, CompareBySizeComparer<T>>(),
                        _ => new AscendingComparer<T, CompareByNameComparer<T>>(),
                    };
                }
                else
                {
                    comparer = sortColumnIndex switch
                    {
                        0 => new CompareByNameComparer<T>(),
                        1 => new CompareByDateModifiedComparer<T>(),
                        2 => new CompareByTypeComparer<T>(),
                        3 => new CompareBySizeComparer<T>(),
                        _ => new CompareByNameComparer<T>(),
                    };
                }

                entries.Sort(comparer);
                sortSpecs.SpecsDirty = false;
            }

            bool shift = ImGui.GetIO().KeyShift;
            bool ctrl = ImGui.GetIO().KeyCtrl;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                ImGui.TableNextRow();
                if (ImGui.TableSetColumnIndex(0))
                {
                    bool selected = IsSelected(entry);
                    if (entry.IsFolder)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                    }

                    ImGui.Text(entry.Icon);
                    ImGui.SameLine();

                    if (entry.IsFolder)
                    {
                        ImGui.PopStyleColor();
                    }

                    if (ImGui.Selectable(entry.Name, selected, ImGuiSelectableFlags.NoAutoClosePopups | ImGuiSelectableFlags.SpanAllColumns))
                    {
                        OnClicked(entry, shift, ctrl);
                    }

                    if (ImGui.BeginPopupContextItem(entry.Name, ImGuiPopupFlags.MouseButtonRight))
                    {
                        // TODO: Implement context menu, but first figure out the bug with the popup
                        ImGui.Text("Test"u8);
                        ImGui.EndPopup();
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        if (entry.IsFolder)
                        {
                            CurrentFolder = entry.Path;
                        }

                        OnDoubleClicked(entry, shift, ctrl);
                    }
                }

                if (ImGui.TableSetColumnIndex(1))
                {
                    ImGui.TextDisabled($"{entry.DateModified:dd/MM/yyyy HH:mm}");
                }

                if (ImGui.TableSetColumnIndex(2))
                {
                    ImGui.TextDisabled(entry.Type);
                }

                if (entry.IsFile && ImGui.TableSetColumnIndex(3))
                {
                    DisplaySize(entry.Size);
                }
            }

            ImGui.EndTable();

            return false;
        }

        protected virtual void OnDoubleClicked(T entry, bool shift, bool ctrl)
        {
        }

        protected virtual void OnClicked(T entry, bool shift, bool ctrl)
        {
        }

        protected abstract bool IsSelected(T entry);

        protected static unsafe void DisplaySize(long size)
        {
            byte* sizeBuffer = stackalloc byte[32];
            int sizeLength = Utf8Formatter.FormatByteSize(sizeBuffer, 32, size, true, 2);
            ImGui.TextDisabled(sizeBuffer);
        }

        public delegate bool IsSelectedDelegate(T entry);

        public delegate void OnClickedDelegate(T entry, bool shift, bool ctrl);

        public delegate void OnDoubleClickedDelegate(T entry, bool shift, bool ctrl);

        public delegate void ContextMenuDelegate(int itemIndex, T entry);

        public static unsafe bool FileView(string strId, Vector2 size, List<T> entries, IsSelectedDelegate isSelectedDelegate, OnClickedDelegate onClickedDelegate, OnDoubleClickedDelegate onDoubleClickedDelegate, ContextMenuDelegate contextMenuDelegate, bool reorder)
        {
            ImGuiTableFlags flags =
                ImGuiTableFlags.Reorderable |
                ImGuiTableFlags.Resizable |
                ImGuiTableFlags.Hideable |
                ImGuiTableFlags.Sortable |
                ImGuiTableFlags.SizingFixedFit |
                ImGuiTableFlags.ScrollX |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.PadOuterX | ImGuiTableFlags.ContextMenuInBody | ImGuiTableFlags.NoSavedSettings;
            var avail = ImGui.GetContentRegionAvail();

            if (!ImGui.BeginTable(strId, 4, flags, size))
            {
                return false;
            }

            ImGui.TableSetupColumn("Name"u8, ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortDescending);
            ImGui.TableSetupColumn("Date Modified"u8, ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Type"u8, ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Size"u8, ImGuiTableColumnFlags.None);

            ImGui.TableSetupScrollFreeze(0, 1);

            ImGui.TableHeadersRow();

            ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();

            if (!sortSpecs.IsNull && (sortSpecs.SpecsDirty || reorder))
            {
                int sortColumnIndex = sortSpecs.Specs.ColumnIndex;
                bool ascending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending;
                IComparer<T> comparer;
                if (ascending)
                {
                    comparer = sortColumnIndex switch
                    {
                        0 => new AscendingComparer<T, CompareByNameComparer<T>>(),
                        1 => new AscendingComparer<T, CompareByDateModifiedComparer<T>>(),
                        2 => new AscendingComparer<T, CompareByTypeComparer<T>>(),
                        3 => new AscendingComparer<T, CompareBySizeComparer<T>>(),
                        _ => new AscendingComparer<T, CompareByNameComparer<T>>(),
                    };
                }
                else
                {
                    comparer = sortColumnIndex switch
                    {
                        0 => new CompareByNameComparer<T>(),
                        1 => new CompareByDateModifiedComparer<T>(),
                        2 => new CompareByTypeComparer<T>(),
                        3 => new CompareBySizeComparer<T>(),
                        _ => new CompareByNameComparer<T>(),
                    };
                }

                entries.Sort(comparer);
            }

            bool shift = ImGui.GetIO().KeyShift;
            bool ctrl = ImGui.GetIO().KeyCtrl;

            float lineHeight = ImGui.GetTextLineHeightWithSpacing();
            float scroll = ImGui.GetScrollY();

            if (size.Y <= 0)
            {
                size.Y += avail.Y;
            }

            int start = Math.Max((int)MathF.Floor(scroll / lineHeight), 0);
            int end = (int)MathF.Ceiling(size.Y / lineHeight) + start;

            if (end > entries.Count)
            {
                end = entries.Count;
            }

            if (start > 0)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Dummy(new(1, start * lineHeight));
            }

            byte* stack = stackalloc byte[64];

            for (int i = start; i < end && i < entries.Count; i++)
            {
                var entry = entries[i];

                ImGui.TableNextRow();
                if (ImGui.TableSetColumnIndex(0))
                {
                    bool selected = isSelectedDelegate(entry);
                    if (entry.IsFolder)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                    }

                    ImGui.Text(entry.Icon);
                    ImGui.SameLine();

                    if (entry.IsFolder)
                    {
                        ImGui.PopStyleColor();
                    }

                    if (ImGui.Selectable(entry.Name, selected, ImGuiSelectableFlags.NoAutoClosePopups | ImGuiSelectableFlags.SpanAllColumns))
                    {
                        onClickedDelegate(entry, shift, ctrl);
                    }

                    if (ImGui.BeginPopupContextItem(entry.Name, ImGuiPopupFlags.MouseButtonRight))
                    {
                        contextMenuDelegate(i, entry);
                        ImGui.EndPopup();
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        onDoubleClickedDelegate(entry, shift, ctrl);
                    }
                }

                if (ImGui.TableSetColumnIndex(1))
                {
                    Utf8Formatter.Format(entry.DateModified, stack, 64, "dd/MM/yyyy HH:mm");
                    ImGui.TextDisabled(stack);
                }

                if (ImGui.TableSetColumnIndex(2))
                {
                    ImGui.TextDisabled(entry.Type);
                }

                if (entry.IsFile && ImGui.TableSetColumnIndex(3))
                {
                    DisplaySize(entry.Size);
                }
            }

            if (end < entries.Count)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Dummy(new(1, (entries.Count - end) * lineHeight));
            }

            ImGui.EndTable();

            return false;
        }

        public static unsafe bool FileSearchView(string strId, Vector2 size, List<T> entries, IsSelectedDelegate isSelectedDelegate, OnClickedDelegate onClickedDelegate, OnDoubleClickedDelegate onDoubleClickedDelegate, ContextMenuDelegate contextMenuDelegate, bool reorder)
        {
            ImGuiTableFlags flags =
                ImGuiTableFlags.Reorderable |
                ImGuiTableFlags.Resizable |
                ImGuiTableFlags.Hideable |
                ImGuiTableFlags.Sortable |
                ImGuiTableFlags.SizingFixedFit |
                ImGuiTableFlags.ScrollX |
                ImGuiTableFlags.ScrollY |
                ImGuiTableFlags.PadOuterX | ImGuiTableFlags.ContextMenuInBody | ImGuiTableFlags.NoSavedSettings;
            var avail = ImGui.GetContentRegionAvail();

            if (!ImGui.BeginTable(strId, 4, flags, size))
            {
                return false;
            }

            ImGui.TableSetupColumn("Name"u8, ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortDescending);
            ImGui.TableSetupColumn("Date Modified"u8, ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Type"u8, ImGuiTableColumnFlags.None);
            ImGui.TableSetupColumn("Size"u8, ImGuiTableColumnFlags.None);

            ImGui.TableSetupScrollFreeze(0, 1);

            ImGui.TableHeadersRow();

            ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();

            if (!sortSpecs.IsNull && (sortSpecs.SpecsDirty || reorder))
            {
                int sortColumnIndex = sortSpecs.Specs.ColumnIndex;
                bool ascending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending;
                IComparer<T> comparer;
                if (ascending)
                {
                    comparer = sortColumnIndex switch
                    {
                        0 => new AscendingComparer<T, CompareByNameComparer<T>>(),
                        1 => new AscendingComparer<T, CompareByDateModifiedComparer<T>>(),
                        2 => new AscendingComparer<T, CompareByTypeComparer<T>>(),
                        3 => new AscendingComparer<T, CompareBySizeComparer<T>>(),
                        _ => new AscendingComparer<T, CompareByNameComparer<T>>(),
                    };
                }
                else
                {
                    comparer = sortColumnIndex switch
                    {
                        0 => new CompareByNameComparer<T>(),
                        1 => new CompareByDateModifiedComparer<T>(),
                        2 => new CompareByTypeComparer<T>(),
                        3 => new CompareBySizeComparer<T>(),
                        _ => new CompareByNameComparer<T>(),
                    };
                }

                entries.Sort(comparer);
                sortSpecs.SpecsDirty = false;
            }

            bool shift = ImGui.GetIO().KeyShift;
            bool ctrl = ImGui.GetIO().KeyCtrl;

            float lineHeight = ImGui.GetTextLineHeightWithSpacing();
            float rowHeight = lineHeight * 2;
            float scroll = ImGui.GetScrollY();

            if (size.Y <= 0)
            {
                size.Y += avail.Y;
            }

            int start = Math.Max((int)MathF.Floor(scroll / rowHeight), 0);
            int end = (int)MathF.Ceiling(size.Y / rowHeight) + start;

            if (end > entries.Count)
            {
                end = entries.Count;
            }

            if (start > 0)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Dummy(new(1, start * rowHeight));
            }

            for (int i = start; i < end && i < entries.Count; i++)
            {
                var entry = entries[i];

                ImGui.TableNextRow();
                if (ImGui.TableSetColumnIndex(0))
                {
                    bool selected = isSelectedDelegate(entry);
                    if (entry.IsFolder)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.87f, 0.37f, 1.0f));
                    }

                    var y = ImGui.GetCursorPosY();
                    ImGui.SetCursorPosY(y + (lineHeight - lineHeight * 0.5f));
                    ImGui.Text(entry.Icon);

                    ImGui.SameLine();
                    ImGui.SetCursorPosY(y);
                    var x = ImGui.GetCursorPosX();

                    if (entry.IsFolder)
                    {
                        ImGui.PopStyleColor();
                    }

                    if (ImGui.Selectable(entry.Name, selected, ImGuiSelectableFlags.NoAutoClosePopups | ImGuiSelectableFlags.SpanAllColumns, new(0, rowHeight)))
                    {
                        onClickedDelegate(entry, shift, ctrl);
                    }

                    if (ImGui.BeginPopupContextItem(entry.Name, ImGuiPopupFlags.MouseButtonRight))
                    {
                        contextMenuDelegate(i, entry);
                        ImGui.EndPopup();
                    }

                    if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        onDoubleClickedDelegate(entry, shift, ctrl);
                    }

                    y = ImGui.GetCursorPosY();
                    ImGui.SetCursorPosY(y - lineHeight);
                    ImGui.SetCursorPosX(x);
                    ImGui.TextDisabled(entry.Path);
                }

                if (ImGui.TableSetColumnIndex(1))
                {
                    ImGui.TextDisabled($"{entry.DateModified:dd/MM/yyyy HH:mm}");
                }

                if (ImGui.TableSetColumnIndex(2))
                {
                    ImGui.TextDisabled(entry.Type);
                }

                if (entry.IsFile && ImGui.TableSetColumnIndex(3))
                {
                    DisplaySize(entry.Size);
                }
            }

            if (end < entries.Count)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Dummy(new(1, (entries.Count - end) * rowHeight));
            }

            ImGui.EndTable();

            return false;
        }
    }
}