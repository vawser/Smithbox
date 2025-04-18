namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets;
    using Hexa.NET.ImGui.Widgets.Extensions;
    using Hexa.NET.Utilities.IO;
    using Hexa.NET.Utilities.Text;
    using System.Diagnostics;
    using System.IO;
    using System.Numerics;

    public abstract class FileDialogBase : Dialog
    {
        private DirectoryInfo currentDir;
        private readonly List<FileSystemItem> entries = new();
        private string rootFolder;
        private string currentFolder;
        private readonly List<string> allowedExtensions = new();
        private RefreshFlags refreshFlags = RefreshFlags.Folders | RefreshFlags.Files;
        private readonly Stack<string> backHistory = new();
        private readonly Stack<string> forwardHistory = new();

        private SearchOptions searchOptions = new() { Flags = SearchOptionsFlags.Subfolders };

        private float widthDrives = 150;

        private Task? refreshTask;
        private volatile bool resetRefresh = false;
        private bool abortRefresh = false;
        private long searchStart, searchEnd;

        private bool reorder = false;

        protected List<FileSystemItem> Entries => entries;

        /// <summary>
        /// Gets the list of file extensions allowed for filtering.<br/>
        /// Extensions are used to determine which files are displayed in the dialog<br/>
        /// when <see cref="OnlyAllowFilteredExtensions"/> is enabled.<br/>
        /// </summary>
        public List<string> AllowedExtensions => allowedExtensions;

        /// <summary>
        /// Gets or sets the root folder of the dialog.<br/>
        /// - This is the starting point of navigation.<br/>
        /// - The dialog cannot navigate above this folder.<br/>
        /// </summary>
        public string RootFolder
        {
            get => rootFolder;
            set => rootFolder = value;
        }

        /// <summary>
        /// Gets or sets the current folder displayed in the dialog.<br/>
        /// - When set, the dialog updates to display the contents of the specified folder.<br/>
        /// - If the provided path does not exist, the folder remains unchanged.<br/>
        /// </summary>
        /// <param name="value">The path to the folder to set as the current folder.</param>
        public string CurrentFolder
        {
            get => currentFolder;
            set
            {
                if (!Directory.Exists(value))
                {
                    return;
                }

                if (currentFolder == value) return;

                var old = currentFolder;
                currentFolder = value;
                OnSetCurrentFolder(old, value);
                OnCurrentFolderChanged(old, value);
            }
        }

        /// <summary>
        /// Gets a <see cref="DirectoryInfo"/> object representing the current folder.<br/>
        /// Provides additional metadata about the current folder, such as attributes or creation time.<br/>
        /// </summary>
        public DirectoryInfo CurrentDir => currentDir;

        /// <summary>
        /// Gets or sets whether hidden files are displayed in the dialog.<br/>
        /// - When true: Hidden files are visible.<br/>
        /// - When false: Hidden files are not displayed.<br/>
        /// Changes to this property automatically call <see cref="Refresh()"/>
        /// </summary>
        public bool ShowHiddenFiles
        {
            get => (refreshFlags & RefreshFlags.Hidden) == 0;
            set
            {
                if (value)
                {
                    refreshFlags |= RefreshFlags.Hidden;
                }
                else
                {
                    refreshFlags &= ~RefreshFlags.Hidden;
                }
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets whether files are displayed in the dialog.<br/>
        /// - When true: Files are visible in the dialog.<br/>
        /// - When false: Files are hidden.<br/>
        /// Changes to this property automatically call <see cref="Refresh()"/>
        /// </summary>
        public bool ShowFiles
        {
            get => (refreshFlags & RefreshFlags.Files) != 0;
            set
            {
                if (value)
                {
                    refreshFlags |= RefreshFlags.Files;
                }
                else
                {
                    refreshFlags &= ~RefreshFlags.Files;
                }
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets whether folders are displayed in the dialog.<br/>
        /// - When true: Folders are visible in the dialog.<br/>
        /// - When false: Folders are hidden.<br/>
        /// Changes to this property automatically call <see cref="Refresh()"/>
        /// </summary>
        public bool ShowFolders
        {
            get => (refreshFlags & RefreshFlags.Folders) != 0;
            set
            {
                if (value)
                {
                    refreshFlags |= RefreshFlags.Folders;
                }
                else
                {
                    refreshFlags &= ~RefreshFlags.Folders;
                }
                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets whether only files with allowed extensions are displayed in the dialog.<br/>
        /// - When true: Files with extensions listed in <see cref="AllowedExtensions"/> are displayed.<br/>
        /// - When false: Files are not filtered by their extensions.<br/>
        /// Changes to this property automatically call <see cref="Refresh()"/>
        /// </summary>
        public bool OnlyAllowFilteredExtensions
        {
            get => (refreshFlags & RefreshFlags.OnlyAllowFilteredExtensions) != 0;
            set
            {
                if (value)
                {
                    refreshFlags |= RefreshFlags.OnlyAllowFilteredExtensions;
                }
                else
                {
                    refreshFlags &= ~RefreshFlags.OnlyAllowFilteredExtensions;
                }

                Refresh();
            }
        }

        /// <summary>
        /// Gets or sets whether the dialog allows selecting folders only.<br/>
        /// - When true: Only folders are displayed and selectable.<br/>
        /// - When false: Both files and folders can be displayed and selected.<br/>
        /// Adjusts `ShowFiles` and `ShowFolders` properties accordingly.<br/>
        /// Changes to this property automatically call <see cref="Refresh()"/>
        /// </summary>
        /// <remarks>
        /// When true, it forces a dialog like <see cref="OpenFileDialog"/> to behave like <see cref="OpenFolderDialog"/>.<br/>
        /// It is recommended to use <see cref="OpenFolderDialog"/> instead for better clarity and intent.<br/>
        /// </remarks>
        public bool OnlyAllowFolders
        {
            get => !ShowFiles && ShowFolders;
            set
            {
                if (value)
                {
                    ShowFiles = false;
                    ShowFolders = true;
                }
                else
                {
                    ShowFiles = true;
                }
                Refresh();
            }
        }

        public override void Show()
        {
            base.Show();

            Refresh();
        }

        public override void Close()
        {
            base.Close();

            abortRefresh = true;
            refreshTask?.Wait();
        }

        protected virtual unsafe void DrawMenuBar()
        {
            var style = WidgetManager.Style;
            if (ImGuiButton.TransparentButton(MaterialIcons.Home))
            {
                CurrentFolder = RootFolder;
                OnClicked(new FileSystemItem(currentFolder, $"{MaterialIcons.Folder}", FileSystemItemFlags.Folder), false, false);
            }
            ImGui.SameLine();
            if (ImGuiButton.TransparentButton(MaterialIcons.ArrowBack))
            {
                TryGoBack();
            }
            ImGui.SameLine();
            if (ImGuiButton.TransparentButton(MaterialIcons.ArrowForward))
            {
                TryGoForward();
            }
            ImGui.SameLine();
            if (ImGuiButton.TransparentButton(MaterialIcons.Refresh))
            {
                Refresh();
            }
            ImGui.SameLine();

            DrawBreadcrumb();

            ImGui.SameLine();

            ImGui.PushItemWidth(200);

            if (ImGui.InputTextWithHint("##Search", "Search ...", ref searchOptions.Pattern, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                searchOptions.Enabled = !string.IsNullOrEmpty(searchOptions.Pattern);
                RefreshAsync();
            }

            if (refreshTask != null && !refreshTask.IsCompleted)
            {
                ImGui.SameLine();
                Vector2 cursor = ImGui.GetCursorPos();
                cursor.X -= 32;
                ImGui.SetCursorPos(cursor);
                ImGuiSpinner.Spinner(9, 2, ImGui.GetColorU32(ImGuiCol.ButtonHovered));
            }

            ImGui.PopItemWidth();

            ImGui.Separator();

            if (ImGuiButton.TransparentButton($"{MaterialIcons.ViewCompact} View"))
            {
                ImGui.OpenPopup("ViewOptionsMenu"u8);
            }

            if (ImGui.BeginPopupContextItem("ViewOptionsMenu"u8))
            {
                bool requireRefresh = false;

                var flags = refreshFlags;

                bool hiddenFiles = (flags & RefreshFlags.Hidden) != 0;
                if (ImGui.MenuItem("Hidden Files"u8, hiddenFiles))
                {
                    if (hiddenFiles) // invert flag
                    {
                        refreshFlags &= ~RefreshFlags.Hidden;
                    }
                    else
                    {
                        refreshFlags |= RefreshFlags.Hidden;
                    }
                    requireRefresh = true;
                }

                bool systemFiles = (flags & RefreshFlags.SystemFiles) != 0;
                if (ImGui.MenuItem("System Files"u8, systemFiles))
                {
                    if (systemFiles) // invert flag
                    {
                        refreshFlags &= ~RefreshFlags.SystemFiles;
                    }
                    else
                    {
                        refreshFlags |= RefreshFlags.SystemFiles;
                    }
                    requireRefresh = true;
                }

                ImGui.EndPopup();

                if (requireRefresh)
                {
                    Refresh();
                }
            }
            if (!searchOptions.Enabled)
            {
                return;
            }
            if (ImGuiButton.TransparentButton($"{MaterialIcons.Search} Search options"))
            {
                ImGui.OpenPopup("SearchOptionsMenu"u8);
            }

            if (ImGui.BeginPopupContextItem("SearchOptionsMenu"u8))
            {
                bool requireRefresh = false;

                var flags = searchOptions.Flags;

                bool subFolders = (flags & SearchOptionsFlags.Subfolders) != 0;
                if (ImGui.MenuItem("All subfolders"u8, subFolders))
                {
                    if (subFolders) // invert flag
                    {
                        searchOptions.Flags &= ~SearchOptionsFlags.Subfolders;
                    }
                    else
                    {
                        searchOptions.Flags |= SearchOptionsFlags.Subfolders;
                    }
                    requireRefresh = true;
                }

                ImGui.Separator();

                if (ImGui.BeginMenu("Date modified"u8))
                {
                    for (SearchFilterDate i = SearchFilterDate.Today; i <= SearchFilterDate.LastYear; i++)
                    {
                        bool selected = i == searchOptions.DateModified;
                        if (ImGui.Selectable(ComboEnumHelper<SearchFilterDate>.GetName(i), selected))
                        {
                            requireRefresh = true;
                            if (searchOptions.DateModified == i)
                            {
                                searchOptions.DateModified = 0;
                                searchOptions.Flags &= ~SearchOptionsFlags.FilterDate;
                                continue;
                            }
                            searchOptions.DateModified = i;
                            searchOptions.Flags |= SearchOptionsFlags.FilterDate;
                        }
                    }
                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Size"u8))
                {
                    for (SearchFilterSize i = SearchFilterSize.Empty; i <= SearchFilterSize.Gigantic; i++)
                    {
                        bool selected = i == searchOptions.FileSize;
                        if (ImGui.Selectable(ComboEnumHelper<SearchFilterSize>.GetName(i), selected))
                        {
                            requireRefresh = true;
                            if (searchOptions.FileSize == i)
                            {
                                searchOptions.FileSize = 0;
                                searchOptions.Flags &= ~SearchOptionsFlags.FilterSize;
                                continue;
                            }
                            searchOptions.FileSize = i;
                            searchOptions.Flags |= SearchOptionsFlags.FilterSize;
                        }
                    }
                    ImGui.EndMenu();
                }

                ImGui.Separator();

                bool hiddenFiles = (flags & SearchOptionsFlags.Hidden) != 0;
                if (ImGui.MenuItem("Hidden Files"u8, hiddenFiles))
                {
                    if (hiddenFiles) // invert flag
                    {
                        searchOptions.Flags &= ~SearchOptionsFlags.Hidden;
                    }
                    else
                    {
                        searchOptions.Flags |= SearchOptionsFlags.Hidden;
                    }
                    requireRefresh = true;
                }

                bool systemFiles = (flags & SearchOptionsFlags.SystemFiles) != 0;
                if (ImGui.MenuItem("System Files"u8, systemFiles))
                {
                    if (systemFiles) // invert flag
                    {
                        searchOptions.Flags &= ~SearchOptionsFlags.SystemFiles;
                    }
                    else
                    {
                        searchOptions.Flags |= SearchOptionsFlags.SystemFiles;
                    }
                    requireRefresh = true;
                }

                ImGui.EndPopup();

                if (requireRefresh)
                {
                    RefreshAsync();
                }
            }

            ImGui.SameLine();

            if (ImGuiButton.TransparentButton($"{MaterialIcons.Close} Close search"))
            {
                searchOptions.Enabled = false;
                searchOptions.Pattern = string.Empty;
                Refresh();
            }

            ImGui.SameLine();

            var searchEnd = this.searchEnd;
            if (searchEnd == 0)
            {
                searchEnd = Stopwatch.GetTimestamp();
            }
            var took = new TimeSpan(searchEnd - searchStart);
            ImGui.Text($"Found: {entries.Count} Took: {took}");
        }

        protected abstract bool IsSelected(FileSystemItem entry);

        protected abstract void OnClicked(FileSystemItem entry, bool shift, bool ctrl);

        protected virtual void OnDoubleClicked(FileSystemItem entry, bool shift, bool ctrl)
        {
            if (entry.IsFolder)
            {
                CurrentFolder = entry.Path;
            }
        }

        protected abstract void OnEnterPressed();

        protected virtual void OnEscapePressed()
        {
            Close(DialogResult.Cancel);
        }

        protected virtual void DrawExplorer()
        {
            Vector2 itemSpacing = ImGui.GetStyle().ItemSpacing;
            DrawMenuBar();

            float footerHeightToReserve = itemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
            Vector2 avail = ImGui.GetContentRegionAvail();
            ImGui.Separator();

            if (ImGui.BeginChild("SidePanel"u8, new Vector2(widthDrives, -footerHeightToReserve), ImGuiWindowFlags.HorizontalScrollbar))
            {
                SidePanel();
            }
            ImGui.EndChild();

            ImGuiSplitter.VerticalSplitter("V"u8, ref widthDrives, 50, avail.X, -footerHeightToReserve);

            ImGui.SameLine();

            var cur = ImGui.GetCursorPos();
            ImGui.SetCursorPos(cur - itemSpacing);
            MainPanel(footerHeightToReserve);
            HandleInput();
        }

        protected virtual unsafe void DrawBreadcrumb()
        {
            var currentFolder = this.currentFolder;
            if (ImGuiBreadcrumb.Breadcrumb("Breadcrumb", ref currentFolder))
            {
                CurrentFolder = currentFolder;
            }
        }

        protected virtual unsafe bool MainPanel(float footerHeightToReserve)
        {
            if (currentDir.Exists)
            {
                var avail = ImGui.GetContentRegionAvail();
                if (searchOptions.Enabled)
                {
                    lock (entries)
                    {
                        ImGuiFileView<FileSystemItem>.FileSearchView("0", new Vector2(avail.X + ImGui.GetStyle().WindowPadding.X, -footerHeightToReserve), entries, IsSelected, OnClicked, OnDoubleClicked, ContextMenu, reorder);
                    }
                }
                else
                {
                    lock (entries)
                    {
                        ImGuiFileView<FileSystemItem>.FileView("1", new Vector2(avail.X + ImGui.GetStyle().WindowPadding.X, -footerHeightToReserve), entries, IsSelected, OnClicked, OnDoubleClicked, ContextMenu, reorder);
                    }
                }
            }

            reorder = false;

            return false;
        }

        protected virtual void ContextMenu(int itemIndex, FileSystemItem entry)
        {
            if (ImGui.MenuItem("Delete"))
            {
                DialogMessageBox dialog = new("Delete file?", "Are you sure?", DialogMessageBoxType.YesCancel);
                dialog.Userdata = (itemIndex, entry);
                dialog.Show(DeleteFileDialogCallback, this, DialogFlags.CenterOnParent | DialogFlags.AlwaysCenter);
            }
        }

        private void DeleteFileDialogCallback(object? sender, DialogResult result)
        {
            var args = ((DialogMessageBox)sender!).Userdata;
            (int itemIndex, FileSystemItem entry) = ((int itemIndex, FileSystemItem entry))args!;
            if (result == DialogResult.Yes)
            {
                File.Delete(entry.Path);
                entries.RemoveAt(itemIndex);
            }
        }

        private void DeleteFileCallback(MessageBox box, object? args)
        {
            (int itemIndex, FileSystemItem entry) = ((int itemIndex, FileSystemItem entry))args!;
            if (box.Result == MessageBoxResult.Yes)
            {
                File.Delete(entry.Path);
                entries.RemoveAt(itemIndex);
            }
        }

        protected virtual void SidePanel()
        {
            var currentFolder = this.currentFolder;
            if (ImGuiFileTreeView.FileTreeView("FileTreeView", default, ref currentFolder, rootFolder))
            {
                CurrentFolder = currentFolder;
                OnClicked(new FileSystemItem(currentFolder, $"{MaterialIcons.Folder}", FileSystemItemFlags.Folder), false, false);
            }
        }

        protected virtual void HandleInput()
        {
            bool focused = ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows);
            bool anyActive = ImGui.IsAnyItemActive() || ImGui.IsAnyItemFocused();

            // avoid handling input if any item is active, prevents issues with text input.
            if (!focused || anyActive)
            {
                return;
            }

            if (ImGui.IsKeyPressed(ImGuiKey.Escape))
            {
                OnEscapePressed();
            }
            if (ImGui.IsKeyPressed(ImGuiKey.Enter))
            {
                OnEnterPressed();
            }
            if (ImGui.IsKeyPressed(ImGuiKey.F5))
            {
                Refresh();
            }

            if (ImGui.IsMouseClicked((ImGuiMouseButton)3))
            {
                TryGoBack();
            }
            if (ImGui.IsMouseClicked((ImGuiMouseButton)4))
            {
                TryGoForward();
            }
        }

        private unsafe void DisplaySize(long size)
        {
            byte* sizeBuffer = stackalloc byte[32];
            int sizeLength = Utf8Formatter.FormatByteSize(sizeBuffer, 32, size, true, 2);
            ImGui.TextDisabled(sizeBuffer);
        }

        protected bool FindRange(FileSystemItem entry, FileSystemItem lastEntry, out int startIndex, out int endIndex)
        {
            lock (entries)
            {
                startIndex = Entries.IndexOf(lastEntry);

                if (startIndex == -1)
                {
                    endIndex = -1; // setting endIndex to a valid number since it's an out parameter
                    return false;
                }
                endIndex = Entries.IndexOf(entry);
                if (endIndex == -1)
                {
                    return false;
                }

                // Swap the indexes if the start index is greater than the end index.
                if (startIndex > endIndex)
                {
                    (startIndex, endIndex) = (endIndex, startIndex);
                }
            }
            return true;
        }

        protected virtual void OnSetCurrentFolder(string oldFolder, string folder)
        {
            backHistory.Push(oldFolder);
            forwardHistory.Clear();

            searchOptions.Enabled = false;
            searchOptions.Pattern = string.Empty;
            Refresh();
        }

        protected virtual void OnCurrentFolderChanged(string old, string value)
        {
        }

        protected void SetInternal(string folder, bool refresh = true)
        {
            if (!Directory.Exists(folder))
            {
                return;
            }

            var old = currentFolder;
            currentFolder = folder;
            OnCurrentFolderChanged(old, folder);
            if (refresh)
            {
                Refresh();
            }
        }

        public virtual void GoHome()
        {
            CurrentFolder = rootFolder;
        }

        public virtual void TryGoBack()
        {
#if NETSTANDARD2_0

            if (backHistory.Count > 0)
            {
                var historyItem = backHistory.Pop();
                forwardHistory.Push(CurrentFolder);
                SetInternal(historyItem);
            }
#else
            if (backHistory.TryPop(out var historyItem))
            {
                forwardHistory.Push(CurrentFolder);
                SetInternal(historyItem);
            }
#endif
        }

        public virtual void TryGoForward()
        {
#if NETSTANDARD2_0

            if (forwardHistory.Count > 0)
            {
                var historyItem = forwardHistory.Pop();
                backHistory.Push(CurrentFolder);
                SetInternal(historyItem);
            }
#else
            if (forwardHistory.TryPop(out var historyItem))
            {
                backHistory.Push(CurrentFolder);
                SetInternal(historyItem);
            }
#endif
        }

        public void ClearHistory()
        {
            forwardHistory.Clear();
            backHistory.Clear();
        }

        public virtual void Refresh()
        {
            currentDir = new DirectoryInfo(currentFolder);
            FileSystemHelper.ClearCache();
            abortRefresh = true;
            refreshTask?.Wait();
            lock (entries)
            {
                entries.Clear();
            }
            abortRefresh = false;
            Refresh(this);
        }

        public virtual void RefreshAsync()
        {
            currentDir = new DirectoryInfo(currentFolder);
            FileSystemHelper.ClearCache();

            lock (entries)
            {
                entries.Clear();
            }

            if (refreshTask == null || refreshTask.IsCompleted)
            {
                refreshTask = Task.Factory.StartNew(Refresh, this);
            }
            else
            {
                resetRefresh = true;
            }
        }

        private static void Refresh(object? x)
        {
            var dialog = (FileDialogBase)x!;

            bool resetSignaled = true;
            const int bufferSize = 64;
            Span<FileSystemItem> local = new FileSystemItem[bufferSize];

            while (resetSignaled)
            {
                dialog.searchEnd = 0;
                dialog.searchStart = Stopwatch.GetTimestamp();
                int i = 0;
                resetSignaled = false;
                foreach (var item in FileSystemHelper.Refresh(dialog.currentFolder, dialog.refreshFlags, dialog.allowedExtensions, dialog.searchOptions, dialog.IconSelector, MaterialIcons.Folder))
                {
                    local[i] = item;
                    i++;

                    if (i == bufferSize)
                    {
                        lock (dialog.entries)
                        {
                            dialog.entries.AddRange(local);
                        }
                        i = 0;
                    }

                    if (dialog.resetRefresh)
                    {
                        lock (dialog.entries)
                        {
                            dialog.entries.Clear();
                        }
                        dialog.resetRefresh = false;
                        resetSignaled = true;
                        i = 0;
                        break;
                    }

                    if (dialog.abortRefresh)
                    {
                        dialog.abortRefresh = false;
                        dialog.searchEnd = Stopwatch.GetTimestamp();
                        return;
                    }
                }

                if (i > 0)
                {
                    lock (dialog.entries)
                    {
                        dialog.entries.AddRange(local[..i]);
                    }
                }
                dialog.searchEnd = Stopwatch.GetTimestamp();
                dialog.reorder = true;
            }
        }

        protected virtual bool FileSystemItemSearchFilter(FileSystemItem arg)
        {
            return true;
        }

        protected virtual string IconSelector(FileMetadata file)
        {
            ReadOnlySpan<char> extension = FileUtils.GetExtension(file.Path.AsSpan());

            switch (extension)
            {
                case ".zip":
                    return $"{MaterialIcons.FolderZip}";

                case ".dds":
                case ".png":
                case ".jpg":
                case ".ico":
                    return $"{MaterialIcons.Image}";

                default:
                    return $"{MaterialIcons.Draft}"; ;
            }
        }
    }
}