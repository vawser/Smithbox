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
    public TextEditorView Parent;
    public ProjectEntry Project;

    public FmgDumper(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void DumperDropdownOptions()
    {
        if (ImGui.BeginMenu($"{LOC.Get("TEXT_Dumper_Header_Text_Dump")}##dumperMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("TEXT_Dumper_Action_Dump")}##dumpAction"))
            {
                _ = DumpAllFMGsAsync();
            }
            GUI.Tooltip(LOC.Get("TEXT_Dumper_Action_Dump_TT"));

            ImGui.EndMenu();
        }
    }

    public async Task DumpAllFMGsAsync()
    {
        var dumpLocation = "";
        if (!PlatformUtils.Instance.OpenFolderDialog(LOC.Get("TEXT_Dumper_Dialog_Select_Directory"), out dumpLocation))
            return;

        // Run on background thread
        await Task.Run(() =>
        {
            Directory.CreateDirectory(dumpLocation);

            var entryTasks = Project.Handler.TextData.PrimaryBank.Containers.Select(entry =>
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

            Smithbox.Log(this, LOC.Get("TEXT_Dumper_Finished_Dump"));
        });
    }
}
