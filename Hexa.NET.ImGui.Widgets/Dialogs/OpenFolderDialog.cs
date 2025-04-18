namespace Hexa.NET.ImGui.Widgets.Dialogs
{
    using Hexa.NET.ImGui;

    public class OpenFolderDialog : FileDialogBase
    {
        private readonly MultiSelection selection = [];

        public OpenFolderDialog()
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
            OnlyAllowFolders = true;
        }

        public OpenFolderDialog(string startingPath)
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
            OnlyAllowFolders = true;
        }

        public OpenFolderDialog(string startingPath, string? searchFilter = null)
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
            OnlyAllowFolders = true;

            if (searchFilter != null)
            {
                AllowedExtensions.AddRange(searchFilter.Split(['|'], StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public override string Name { get; } = "Folder Picker";

        protected override ImGuiWindowFlags Flags { get; } = ImGuiWindowFlags.NoDocking;

        public string? SelectedFolder
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
            if (ImGui.InputText("Selected"u8, ref selectionString, 1024, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                selection.SetSelectionString(selectionString, GetValidationOptions());
                if (ImGui.IsKeyPressed(ImGuiKey.Enter))
                {
                    selection.Validate(GetValidationOptions());
                }
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

        private static MultiSelection.ValidationOptions GetValidationOptions()
        {
            return MultiSelection.ValidationOptions.MustExist | MultiSelection.ValidationOptions.AllowFolders;
        }

        public override void Reset()
        {
            selection.Clear();
            base.Reset();
        }

        protected override void OnCurrentFolderChanged(string old, string value)
        {
            selection.RootPath = value;
            if (!AllowMultipleSelection)
            {
                selection.Clear();
                selection.Add(value);
            }
        }

        protected override bool IsSelected(FileSystemItem entry)
        {
            if (entry.IsFile)
            {
                return false;
            }

            return selection.Contains(entry.Path);
        }

        protected override void OnClicked(FileSystemItem entry, bool shift, bool ctrl)
        {
            if (entry.IsFile)
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