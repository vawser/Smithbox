using System.Text.Json;
using StudioCore.Editors.MapEditor.MapGroup;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Platform;

namespace StudioCore.Editors.MapEditor.SelectionGroup;

public class SelectionGroupContainer
{
    public SelectionGroupList Data;

    private string SelectionDirectory = "";
    private string SelectionPath = "";

    public SelectionGroupContainer()
    {
        SelectionDirectory = $"{Project.ProjectDataDir}\\{Project.GetGameIDForDir()}\\selections";
        SelectionPath = $"{SelectionDirectory}\\selection_groups.json";

        if (!Directory.Exists(SelectionDirectory))
        {
            Directory.CreateDirectory(SelectionDirectory);
            string template = "{ \"Resources\": [ ] }";
            try
            {
                var fs = new FileStream(SelectionPath, System.IO.FileMode.Create);
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

        Data = LoadJSON();
    }

    private SelectionGroupList LoadJSON()
    {
        if (File.Exists(SelectionPath))
        {
            using (var stream = File.OpenRead(SelectionPath))
            {
                var selectionResource = JsonSerializer.Deserialize(stream, SelectionGroupListSerializationContext.Default.SelectionGroupList);

                return selectionResource;
            }
        }

        return null;
    }

    public bool DeleteSelectionGroup(string currentResourceName)
    {
        var resource = Data.Resources.Where(x => x.Name == currentResourceName).FirstOrDefault();
        Data.Resources.Remove(resource);

        SaveSelectionGroups();

        return true;
    }

    public bool AddSelectionGroup(string name, List<string> tags, List<string> selection, int keybindIndex, bool isEdit = false, string oldName = "")
    {
        if(name == "")
        {
            PlatformUtils.Instance.MessageBox("Group name is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if(!isEdit && Data.Resources.Any(x => x.Name == name))
        {
            PlatformUtils.Instance.MessageBox("Group name already exists.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if(!isEdit && selection == null)
        {
            PlatformUtils.Instance.MessageBox("Selection is invalid.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (!isEdit && selection.Count == 0)
        {
            PlatformUtils.Instance.MessageBox("Selection is empty.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else if (keybindIndex != -1 && Data.Resources.Any(x => x.SelectionGroupKeybind == keybindIndex))
        {
            var group = Data.Resources.Where(x => x.SelectionGroupKeybind == keybindIndex).First();
            PlatformUtils.Instance.MessageBox($"Keybind already assigned to another selection group: {group.Name}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        else
        {
            // Delete old entry, since we will create it a-new with the edits immediately
            if(isEdit)
            {
                DeleteSelectionGroup(oldName);
            }

            var res = new SelectionGroupResource();
            res.Name = name;
            res.Tags = tags;
            res.Selection = selection;
            res.SelectionGroupKeybind = keybindIndex;

            Data.Resources.Add(res);

            SaveSelectionGroups();
        }

        return false;
    }

    public bool SaveSelectionGroups()
    {
        string jsonString = JsonSerializer.Serialize(Data, typeof(SelectionGroupList), SelectionGroupListSerializationContext.Default);

        try
        {
            var fs = new FileStream(SelectionPath, System.IO.FileMode.Create);
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
        return false;
    }
}
