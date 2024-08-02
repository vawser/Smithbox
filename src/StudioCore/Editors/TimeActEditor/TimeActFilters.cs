using HKLib.hk2018.hk;
using HKLib.hk2018.hkAsyncThreadPool;
using SoulsFormats;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.AnimationBank;

namespace StudioCore.Editors.TimeActEditor;

public static class TimeActFilters
{
    public static string _fileContainerFilterString = "";
    public static string _timeActFilterString = "";
    public static string _timeActAnimationFilterString = "";
    public static string _timeActEventFilterString = "";

    public static bool FileContainerFilter(AnimationFileInfo info)
    {
        bool isValid = true;
        var input = _fileContainerFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = info.Name.ToLower();

            var referenceDict = Smithbox.AliasCacheHandler.AliasCache.Characters;
            var alias = "";
            var tags = new List<string>();

            if (referenceDict.ContainsKey(info.Name))
            {
                alias = referenceDict[info.Name].name.ToLower();
                tags = referenceDict[info.Name].tags;
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

    public static bool TimeActFilter(AnimationFileInfo info, TAE taeEntry)
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

            if (Smithbox.BankHandler.TimeActAliases.Aliases != null)
            {
                var idStr = id.ToString();
                var idSection = idStr.Substring(idStr.Length - 3);

                var searchStr = $"{info.Name}_{idSection}";
                var refAlias = Smithbox.BankHandler.TimeActAliases.Aliases.list.Where(e => e.id == searchStr)
                    .FirstOrDefault();

                if (refAlias != null)
                {
                    alias = refAlias.name.ToLower();
                    tags = refAlias.tags;
                }
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

    public static bool TimeActAnimationFilter(AnimationFileInfo info, TAE.Animation animEntry)
    {
        bool isValid = true;
        var input = _timeActAnimationFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = animEntry.ID.ToString();
            var alias = "";
            var tags = new List<string>();

            // TODO: alias part

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

    public static bool TimeActEventFilter(AnimationFileInfo info, TAE.Event evtEntry)
    {
        bool isValid = true;
        var input = _timeActEventFilterString.ToLower();

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var id = evtEntry.TypeName.ToString();
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
