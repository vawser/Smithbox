namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;
    using Hexa.NET.ImGui.Widgets.Extensions;

    public class OpenFileDialog : FileDialogBase
    {
        private readonly MultiSelection selection = [];

        public OpenFileDialog()
        {
            string startingPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                {
                    startingPath = AppContext.BaseDirectory;
                }
            }

            RootFolder = startingPath;
            SetInternal(startingPath, refresh: false);
            ShowFiles = ShowFolders = true;
        }

        public OpenFileDialog(string startingPath)
        {
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                {
                    startingPath = AppContext.BaseDirectory;
                }
            }

            RootFolder = startingPath;
            SetInternal(startingPath, refresh: false);
            ShowFiles = ShowFolders = true;
        }

        public OpenFileDialog(string startingPath, string? searchFilter = null)
        {
            if (File.Exists(startingPath))
            {
                startingPath = Path.GetDirectoryName(startingPath) ?? string.Empty;
            }
            else if (string.IsNullOrEmpty(startingPath) || !Directory.Exists(startingPath))
            {
                startingPath = Environment.CurrentDirectory;
                if (string.IsNullOrEmpty(startingPath))
                {
                    startingPath = AppContext.BaseDirectory;
                }
            }

            RootFolder = startingPath;
            SetInternal(startingPath, refresh: false);
            ShowFiles = ShowFolders = true;

            if (searchFilter != null)
            {
                AllowedExtensions.AddRange(searchFilter.Split(['|'], StringSplitOptions.RemoveEmptyEntries));
            }
        }

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoDocking;

        public override string Name { get; } = "File Picker";

        /// <summary>
        /// Gets the selected path.<br/>
        /// - When selecting files, this returns the selected file path.<br/>
        /// - When `OnlyAllowFolders` is enabled, this returns the selected folder path.<br/>
        /// </summary>
        public string? SelectedFile
        {
            get => selection.Count > 0 ? selection[0] : null;
        }

        public bool AllowMultipleSelection
        {
            get => selection.AllowMultipleSelection;
            set => selection.AllowMultipleSelection = value;
        }

        public IReadOnlyList<string> Selection => selection;

        protected override void DrawContent()
        {
            DrawExplorer();

            var selectionString = selection.SelectionString;
            if (ImGui.InputText("Selected"u8, ref selectionString, 2048, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                selectionString = selectionString.TrimText('\"');

                if (selectionString.IsValidPath())
                {
                    CurrentFolder = Path.GetDirectoryName(selectionString)!;
                    selectionString = selectionString.GetRelativePath(CurrentFolder);
                }

                selection.SetSelectionString(selectionString, GetValidationOptions());
            }

            ImGui.SameLine();
            if (ImGui.Button("Cancel"u8))
            {
                Close(DialogResult.Cancel);
            }

            ImGui.SameLine();
            if (ImGui.Button("Open"u8))
            {
                selection.Validate(GetValidationOptions());
                Close(selection.Count > 0 ? DialogResult.Ok : DialogResult.Failed);
            }
        }

        private MultiSelection.ValidationOptions GetValidationOptions()
        {
            MultiSelection.ValidationOptions options = MultiSelection.ValidationOptions.MustExist;
            if (OnlyAllowFolders)
            {
                options |= MultiSelection.ValidationOptions.AllowFolders;
            }
            else
            {
                options |= MultiSelection.ValidationOptions.AllowFiles;
            }

            return options;
        }

        public override void Reset()
        {
            selection.Clear();
            base.Reset();
        }

        protected override void OnCurrentFolderChanged(string old, string value)
        {
            selection.RootPath = value;
            if (OnlyAllowFolders && !AllowMultipleSelection)
            {
                selection.Clear();
                selection.Add(value);
            }
        }

        protected override bool IsSelected(FileSystemItem entry)
        {
            if (entry.IsFile == OnlyAllowFolders)
            {
                return false;
            }

            return selection.Contains(entry.Path);
        }

        protected override void OnClicked(FileSystemItem entry, bool shift, bool ctrl)
        {
            if (entry.IsFile == OnlyAllowFolders)
            {
                return;
            }

            if (shift)
            {
                string? last = selection.Count > 0 ? selection[^1] : null;
                if (last == null)
                {
                    return;
                }

                // Avoid querying the file system by setting the field directly and not calling the constructor.
                FileSystemItem lastEntry = new() { Path = last };

                if (FindRange(entry, lastEntry, out int startIndex, out int endIndex))
                {
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        selection.Add(Entries[i].Path);
                    }
                }
            }
            else if (ctrl)
            {
                selection.Add(entry.Path);
            }
            else
            {
                selection.Clear();
                selection.Add(entry.Path);
            }
        }

        protected override void OnDoubleClicked(FileSystemItem entry, bool shift, bool ctrl)
        {
            base.OnDoubleClicked(entry, shift, ctrl);
            if (!entry.IsFile) return;
            selection.Validate(GetValidationOptions());
            if (selection.Count == 0)
            {
                return;
            }

            Close(DialogResult.Ok);
        }

        protected override void OnEnterPressed()
        {
            selection.Validate(GetValidationOptions());
            if (selection.Count == 0)
            {
                return;
            }

            Close(DialogResult.Ok);
        }
    }
}