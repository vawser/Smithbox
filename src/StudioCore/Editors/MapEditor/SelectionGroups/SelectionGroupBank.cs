using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using StudioCore.Core.Project;
using StudioCore.Editor;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class SelectionGroupBank
{
    public SelectionGroupList Groups { get; set; }

    private string GroupDirectory = "";

    private string GroupFileName = "";

    public SelectionGroupBank()
    {
        GroupDirectory = "selections";
        GroupFileName = "selection_groups";
    }

    public void LoadBank()
    {
        try
        {
            Groups = LoadSelectionGroupJSON(GroupDirectory, GroupFileName);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load Selection Group Bank: {e.Message}");
        }

        TaskLogs.AddLog($"Selection Group Bank: Loaded Selection Groups");
    }

    public void CreateSelectionGroups()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (Smithbox.ProjectRoot == "")
            return;

        var SelectionDirectory = $"{Smithbox.ProjectRoot}\\.smithbox\\{MiscLocator.GetGameIDForDir()}\\selections";
        var SelectionPath = $"{SelectionDirectory}\\selection_groups.json";

        if (!Directory.Exists(SelectionDirectory))
        {
            try
            {
                Directory.CreateDirectory(SelectionDirectory);
            }
            catch
            {
                TaskLogs.AddLog($"Failed to create selection groups directory: {SelectionDirectory}");
                return;
            }

            string template = "{ \"Resources\": [ ] }";
            try
            {
                var fs = new FileStream(SelectionPath, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(template);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }

    public bool DeleteSelectionGroup(string currentResourceName)
    {
        var resource = Groups.Resources.Where(x => x.Name == currentResourceName).FirstOrDefault();
        Groups.Resources.Remove(resource);

        SaveSelectionGroups();

        return true;
    }

    public bool AddSelectionGroup(string name, List<string> tags, List<string> selection, int keybindIndex, bool isEdit = false, string oldName = "")
    {
        if (name == "")
        {
            PlatformUtils.Instance.MessageBox("Group name is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (!isEdit && Groups.Resources.Any(x => x.Name == name))
        {
            PlatformUtils.Instance.MessageBox("Group name already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (!isEdit && selection == null)
        {
            PlatformUtils.Instance.MessageBox("Selection is invalid.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (!isEdit && selection.Count == 0)
        {
            PlatformUtils.Instance.MessageBox("Selection is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (keybindIndex != -1 && Groups.Resources.Any(x => x.SelectionGroupKeybind == keybindIndex))
        {
            var group = Groups.Resources.Where(x => x.SelectionGroupKeybind == keybindIndex).First();
            if (isEdit)
            {
                group = Groups.Resources.Where(x => x.SelectionGroupKeybind == keybindIndex && x.Name != name).First();
            }
            PlatformUtils.Instance.MessageBox($"Keybind already assigned to another selection group: {group.Name}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else
        {
            // Delete old entry, since we will create it a-new with the edits immediately
            if (isEdit)
            {
                DeleteSelectionGroup(oldName);
            }

            var res = new SelectionGroupResource();
            res.Name = name;
            res.Tags = tags;
            res.Selection = selection;
            res.SelectionGroupKeybind = keybindIndex;

            Groups.Resources.Add(res);

            SaveSelectionGroups();
        }

        return false;
    }

    public bool SaveSelectionGroups()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return false;

        var SelectionDirectory = $"{Smithbox.ProjectRoot}\\.smithbox\\{MiscLocator.GetGameIDForDir()}\\selections";
        var SelectionPath = $"{SelectionDirectory}\\selection_groups.json";

        string jsonString = JsonSerializer.Serialize(Groups, typeof(SelectionGroupList), SelectionGroupListSerializationContext.Default);

        if (Directory.Exists(SelectionDirectory))
        {
            try
            {
                var fs = new FileStream(SelectionPath, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
        else
        {
            return false;
        }

        return true;
    }
    public SelectionGroupList LoadSelectionGroupJSON(string directory, string filename)
    {
        var smithboxResource = new SelectionGroupList();

        var smithboxResourcePath = $"{Smithbox.SmithboxDataRoot}\\{MiscLocator.GetGameIDForDir()}\\{directory}\\{filename}.json";

        if (File.Exists(smithboxResourcePath))
        {
            using (var stream = File.OpenRead(smithboxResourcePath))
            {
                smithboxResource = JsonSerializer.Deserialize(stream, SelectionGroupListSerializationContext.Default.SelectionGroupList);
            }
        }
        else
        {
            TaskLogs.AddLog($"{smithboxResource} does not exist!");
        }

        return smithboxResource;
    }
}
