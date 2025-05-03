using HKLib.hk2018.hk;
using HKLib.hk2018.hkAsyncThreadPool;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.Bank.TimeActBank;

namespace StudioCore.Editors.TimeActEditor;

public static class TimeActFilters
{
    public static string _fileContainerFilterString = "";
    public static string _timeActFilterString = "";
    public static string _timeActAnimationFilterString = "";
    public static string _timeActEventFilterString = "";
    public static string _timeActEventPropertyFilterString = "";

    public static bool FileContainerFilter(TimeActEditorScreen editor, TimeActContainerWrapper info)
    {
        bool isValid = true;
        var input = _fileContainerFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = info.Name.ToLower();

            var aliasEntry = editor.Project.Aliases.Characters.Where(e => e.ID == id).FirstOrDefault();
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

    public static bool TimeActFilter(TimeActEditorScreen editor, TimeActContainerWrapper info, TAE taeEntry)
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

            var searchStr = $"{info.Name}_{idSection}";
            var aliasEntry = editor.Project.Aliases.TimeActs.Where(e => e.ID == searchStr).FirstOrDefault();

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

    public static bool TimeActAnimationFilter(TimeActContainerWrapper info, TAE.Animation animEntry)
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

    public static bool TimeActEventFilter(TimeActContainerWrapper info, TAE.Event evtEntry)
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

    public static bool TimeActEventPropertyFilter(TimeActContainerWrapper info, string propertyName)
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
