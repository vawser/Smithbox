using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActFilters
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public string _fileContainerFilterString = "";
    public string _timeActFilterString = "";
    public string _timeActAnimationFilterString = "";
    public string _timeActEventFilterString = "";
    public string _timeActEventPropertyFilterString = "";

    public TimeActFilters(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public bool FileContainerFilter(string filename)
    {
        bool isValid = true;
        var input = _fileContainerFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = filename.ToLower();

            var aliasEntry = Editor.Project.Aliases.Characters.Where(e => e.ID == id).FirstOrDefault();
            var alias = "";
            var tags = new List<string>();

            if (aliasEntry != null)
            {
                alias = aliasEntry.Name;
                tags = aliasEntry.Tags;
            }

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (entry == alias)
                    partTruth[i] = true;

                if (alias.Contains(entry))
                    partTruth[i] = true;

                foreach (string tagStr in tags)
                {
                    if (entry == tagStr.ToLower())
                        partTruth[i] = true;

                    if (tagStr.ToLower().Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    public bool TimeActFilter(string filename, TAE taeEntry)
    {
        bool isValid = true;
        var input = _timeActFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = taeEntry.ID.ToString();
            var alias = "";
            var tags = new List<string>();

            var idStr = id.ToString();
            var idSection = idStr.Substring(idStr.Length - 3);

            var searchStr = $"{filename}_{idSection}";
            var aliasEntry = Editor.Project.Aliases.TimeActs.Where(e => e.ID == searchStr).FirstOrDefault();

            if (aliasEntry != null)
            {
                alias = aliasEntry.Name.ToLower();
                tags = aliasEntry.Tags;
            }

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (entry == alias)
                    partTruth[i] = true;

                if (alias.Contains(entry))
                    partTruth[i] = true;

                foreach (string tagStr in tags)
                {
                    if (entry == tagStr.ToLower())
                        partTruth[i] = true;

                    if (tagStr.ToLower().Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    public bool TimeActAnimationFilter(TAE.Animation animEntry)
    {
        bool isValid = true;
        var input = _timeActAnimationFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = animEntry.ID.ToString();
            var tags = new List<string>();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    public bool TimeActEventFilter(TAE.Event evtEntry)
    {
        bool isValid = true;
        var input = _timeActEventFilterString.ToLower();

        if (evtEntry == null)
            return true;

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = evtEntry.TypeName.ToString().ToLower();
            var alias = "";
            var tags = new List<string>();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (entry == alias)
                    partTruth[i] = true;

                if (alias.Contains(entry))
                    partTruth[i] = true;

                foreach (string tagStr in tags)
                {
                    if (entry == tagStr.ToLower())
                        partTruth[i] = true;

                    if (tagStr.ToLower().Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    public bool TimeActEventPropertyFilter(string propertyName)
    {
        bool isValid = true;
        var input = _timeActEventPropertyFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = propertyName;
            var alias = "";
            var tags = new List<string>();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == id)
                    partTruth[i] = true;

                if (id.Contains(entry))
                    partTruth[i] = true;

                if (entry == alias)
                    partTruth[i] = true;

                if (alias.Contains(entry))
                    partTruth[i] = true;

                foreach (string tagStr in tags)
                {
                    if (entry == tagStr.ToLower())
                        partTruth[i] = true;

                    if (tagStr.ToLower().Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }
}
