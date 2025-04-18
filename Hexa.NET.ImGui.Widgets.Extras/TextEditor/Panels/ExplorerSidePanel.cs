namespace Hexa.NET.ImGui.Widgets.Extras.TextEditor.Panels
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.ImGui.Widgets.Extras.TextEditor;
    using Hexa.NET.Utilities;
    using System;
    using System.Diagnostics;

    public static class Ut
    {
        public static bool StartsWith(this ReadOnlySpan<char> span, StdString str)
        {
            if (span.Length < str.Size)
            {
                return false;
            }

            for (int i = 0; i < str.Size; i++)
            {
                if (span[i] != str[i])
                {
                    return false;
                }
            }

            return true;
        }
    }

    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public struct FileSystemItem
    {
        public int ParentIndex;
        public int Depth;
        public int NextSiblingIndex;
        public ItemTypeFlags Type;
        public StdString Name;
        public StdString Path;
        public StdString Extension;

        public readonly bool IsEmpty => (Type & ItemTypeFlags.Empty) != 0;

        public unsafe void Release()
        {
            if (Name.Front != null)
            {
                Name.Release();
                Name.Release();
                Path.Release();
                Extension.Release();
                this = default;
            }
        }

        public override readonly string ToString()
        {
            return $"{Type}, Parent: {ParentIndex}, Depth {Depth}, Next {NextSiblingIndex}, {Name}";
        }

        private readonly string GetDebuggerDisplay()
        {
            return $"{Type}, Parent: {ParentIndex}, Depth {Depth}, Next {NextSiblingIndex}, {Name}";
        }
    }

    public enum ItemTypeFlags
    {
        None = 0,
        File = 1,
        Folder = 2,
        Empty = 4
    }

    // TODO: Implement explorer side panel.
    public unsafe class ExplorerSidePanel : SidePanel
    {
        private readonly HashSet<string> openFolders = [];
        private readonly FileSystemState fileSystemState = new();

        public ExplorerSidePanel()
        {
            fileSystemState.Refresh();
        }

        protected override void DisposeCore()
        {
        }

        public override string Icon { get; } = $"{MaterialIcons.FilePresent}";

        public override string Title { get; } = "Explorer";

        public override void DrawContent()
        {
            static void DisplayFileNode(FileSystemItem file)
            {
                if (ImGui.TreeNodeEx(file.Name.Data, ImGuiTreeNodeFlags.Leaf))
                {
                    ImGui.TreePop();
                }
            }

            static bool DisplayFolderNode(FileSystemItem folder, bool isRoot = false)
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;

                if (folder.IsEmpty)
                {
                    flags |= ImGuiTreeNodeFlags.Leaf;
                }

                if (isRoot)
                {
                    flags |= ImGuiTreeNodeFlags.Bullet;
                }

                return ImGui.TreeNodeEx(folder.Name.Data, flags);
            }

            int currentDepth = -1;

            lock (fileSystemState.SyncRoot)
            {
                int i = 0;
                while (i < fileSystemState.Items.Count)
                {
                    FileSystemItem item = fileSystemState.Items[i];

                    while (item.Depth <= currentDepth)
                    {
                        ImGui.TreePop();
                        currentDepth--;
                    }

                    if ((item.Type & ItemTypeFlags.File) != 0)
                    {
                        DisplayFileNode(item);
                        i = item.NextSiblingIndex;
                    }
                    else
                    {
                        bool nodeOpen = DisplayFolderNode(item, item.ParentIndex == -1);
                        if (nodeOpen)
                        {
                            i++; // go down the tree to the first child
                            currentDepth = item.Depth;
                        }
                        else
                        {
                            i = item.NextSiblingIndex; // go to the next sibling or parent's sibling
                            if (i == -1 || i == fileSystemState.Items.Count)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            while (currentDepth >= 0)
            {
                ImGui.TreePop();
                currentDepth--;
            }
        }

        public unsafe class FileSystemState
        {
            private readonly object _lock = new();
            private string? folderPath;
            private UnsafeList<FileSystemItem> items;

            private FileSystemWatcher? watcher;

            public object SyncRoot => _lock;

            public IReadOnlyList<FileSystemItem> Items => items;

            public void SetFolder(string folderPath)
            {
                lock (_lock)
                {
                    this.folderPath = folderPath;
                    watcher?.Dispose();
                    watcher = new(folderPath);
                    watcher.NotifyFilter |= NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Changed += WatcherChanged;
                    watcher.Created += WatcherChanged;
                    watcher.Deleted += WatcherChanged;
                    watcher.Renamed += WatcherChanged;
                    watcher.EnableRaisingEvents = true;
                    Refresh();
                }
            }

            private int FindParentIndex(string path)
            {
                ReadOnlySpan<char> parentPath = Path.GetDirectoryName(path.AsSpan());
                return FindItemIndex(parentPath);
            }

            private int FindItemIndex(ReadOnlySpan<char> path)
            {
                path = path.TrimStart(folderPath); // usually includes a leading slash

                // At best O(1)
                // At average O(log n)
                // At worst O(h) where h is the height of the tree

                // while a normal for loop would be O(n*m) where m is the length of the path

                int i = 0;
                while (i < items.Count)
                {
                    if (path.IsEmpty)
                    {
                        return i;
                    }

                    var item = items[i];
                    if (path.StartsWith(item.Name))
                    {
                        path = path[item.Name.Size..];
                        path = path.TrimStart(Path.DirectorySeparatorChar);
                        i++; // traverse into the folder
                    }
                    else
                    {
                        i = item.NextSiblingIndex; // go to the next file or folder
                    }
                }

                return -1;
            }

            private int FindInsertionIndex(int parentIndex, string path, bool isFile)
            {
                var name = Path.GetFileName(path.AsSpan());
                int i = parentIndex + 1;
                var depth = items[parentIndex].Depth + 1;
                while (i < items.Count)
                {
                    var item = items[i];

                    if ((item.Type & ItemTypeFlags.File) != 0 && !isFile)
                    {
                        i = item.NextSiblingIndex;
                        continue;
                    }

                    if (item.Depth != depth)
                    {
                        return i;
                    }

                    if (StdString.Compare(name, items[i].Name) < 0)
                    {
                        return i;
                    }

                    i = item.NextSiblingIndex;
                }

                return i;
            }

            private void Add(UnsafeList<FileSystemItem>* items, FileSystemItem item, int index)
            {
                items->Insert(index, item);
                AdjustIndices(items, index + 1, 1);
            }

            private void Remove(UnsafeList<FileSystemItem>* items, int index)
            {
                items->RemoveAt(index);
                AdjustIndices(items, index, -1);
            }

            private void CascadeRemove(UnsafeList<FileSystemItem>* items, int index)
            {
                int i = index;
                int delta = 1;
                int depth = items->At(index).Depth;
                items->RemoveAt(index);
                while (i < items->Count)
                {
                    if (items->At(i).Depth > depth)
                    {
                        delta++;
                        items->RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }

                AdjustIndices(items, i, -delta);
            }

            private static void AdjustIndices(UnsafeList<FileSystemItem>* items, int start, int delta)
            {
                for (int j = start; j < items->Count; j++)
                {
                    items->GetPointer(j)->NextSiblingIndex += delta;
                }
            }

            private void WatcherChanged(object sender, FileSystemEventArgs e)
            {
                Refresh();
            }

            public unsafe void Refresh()
            {
                if (folderPath == null)
                {
                    return;
                }

                lock (_lock)
                {
                    var localItems = items;

                    for (int i = 0; i < localItems.Count; i++)
                    {
                        localItems[i].Release();
                    }

                    ScanDirectory(folderPath, &localItems);
                    items = localItems;
                }
            }

            public static void ScanDirectory(string startDirectory, UnsafeList<FileSystemItem>* result)
            {
                FileSystemItem root = new()
                {
                    ParentIndex = -1,
                    Depth = 0,
                    Type = ItemTypeFlags.Folder,
                    Name = new StdString(Path.GetFileName(startDirectory)),
                    Path = new StdString(startDirectory),
                    Extension = default,
                    NextSiblingIndex = -1
                };
                result->Add(root);
                ProcessDirectory(startDirectory, 0, result->GetPointer(0), result, 1);

                for (int i = 0; i < result->Size; i++)
                {
                    Debug.WriteLine($"Item {i}: {result->At(i)}");
                }
            }

            private static void ProcessDirectory(string currentDirectory, int parentIndex, FileSystemItem* self, UnsafeList<FileSystemItem>* items, int depth)
            {
                int currentIndex = items->Count;
                int previousIndex = -1;
                bool empty = true;

                foreach (var directory in Directory.EnumerateDirectories(currentDirectory))
                {
                    int newItemIndex = items->Count;
                    items->Add(new FileSystemItem
                    {
                        ParentIndex = parentIndex,
                        Depth = depth,
                        Type = ItemTypeFlags.Folder,
                        Name = new StdString(Path.GetFileName(directory)),
                        Path = new StdString(directory),
                        Extension = default,
                        NextSiblingIndex = -1
                    });
                    if (previousIndex != -1)
                    {
                        items->GetPointer(previousIndex)->NextSiblingIndex = newItemIndex;
                    }
                    previousIndex = newItemIndex;
                    empty = false;

                    ProcessDirectory(directory, newItemIndex, items->GetPointer(newItemIndex), items, depth + 1);
                }

                if (previousIndex != -1)
                {
                    items->GetPointer(previousIndex)->NextSiblingIndex = items->Count;
                }

                foreach (var file in Directory.EnumerateFiles(currentDirectory))
                {
                    int newItemIndex = items->Count;
                    items->Add(new FileSystemItem
                    {
                        ParentIndex = parentIndex,
                        Depth = depth,
                        Type = ItemTypeFlags.File,
                        Name = new StdString(Path.GetFileName(file)),
                        Path = new StdString(file),
                        Extension = new StdString(Path.GetExtension(file)),
                        NextSiblingIndex = -1
                    });
                    if (previousIndex != -1)
                    {
                        items->GetPointer(previousIndex)->NextSiblingIndex = newItemIndex;
                    }
                    previousIndex = newItemIndex;
                    empty = false;
                }

                if (previousIndex != -1)
                {
                    items->GetPointer(previousIndex)->NextSiblingIndex = items->Count;
                }

                if (empty)
                {
                    self->Type |= ItemTypeFlags.Empty;
                }
            }
        }
    }
}