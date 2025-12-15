using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class FmgDumper
{
    public TextEditorScreen Editor;
    public ProjectEntry Project;

    public FmgDumper(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void MenubarOptions()
    {
        if (ImGui.MenuItem("Dump"))
        {
            _ = DumpAllFMGsAsync();
        }
        UIHelper.Tooltip("Dumps all the FMG contents into individual text files for data-mining.");
    }

    public async Task DumpAllFMGsAsync()
    {
        var dumpLocation = "";
        if (!PlatformUtils.Instance.OpenFolderDialog("Select Directory", out dumpLocation))
            return;

        // Run on background thread
        await Task.Run(() =>
        {
            Directory.CreateDirectory(dumpLocation);

            var entryTasks = Project.TextData.PrimaryBank.Entries.Select(entry =>
            {
                return Task.Run(() =>
                {
                    var wrapper = entry.Value;
                    var containerFolder = $"{dumpLocation}/{entry.Key.Folder}";
                    Directory.CreateDirectory(containerFolder);

                    var fmgTasks = wrapper.FmgWrappers.Select(fmgWrapper =>
                    {
                        return Task.Run(() =>
                        {
                            string typeFolder = null;

                            if (TextUtils.IsItemContainer(wrapper))
                            {
                                typeFolder = "item";
                            }
                            if (TextUtils.IsMenuContainer(wrapper))
                            {
                                typeFolder = "menu";
                            }

                            if (typeFolder != null)
                            {
                                var targetFolder = $"{containerFolder}/{typeFolder}";
                                Directory.CreateDirectory(targetFolder);

                                var filename = $"{fmgWrapper.ID} - {fmgWrapper.Name}.txt";
                                var writePath = $"{targetFolder}/{filename}";

                                var contents = string.Join(
                                    Environment.NewLine,
                                    fmgWrapper.File.Entries.Select(e => $"{e.ID};{e.Text}")
                                );

                                File.WriteAllText(writePath, contents);
                            }
                        });
                    });

                    Task.WaitAll(fmgTasks.ToArray());
                });
            });

            Task.WaitAll(entryTasks.ToArray());

            TaskLogs.AddLog("Finished dumping all FMGs.");
        });
    }
}
