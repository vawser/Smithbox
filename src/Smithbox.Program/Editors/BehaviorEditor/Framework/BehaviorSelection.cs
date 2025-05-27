using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorSelection
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorSelection(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public FileDictionaryEntry SelectedFileEntry;
    public BinderContents SelectedBinderContents;
    public bool LoadBinder = false;

    public int SelectedInternalFileIndex = -1;
    public string SelectedInternalFileKey = "";
    public BinderFile SelectedBinderFile = null;
    public bool LoadFile = false;

    public HavokCategoryType SelectedInternalFileType = HavokCategoryType.None;

    public HavokBinarySerializer Selected_HKX3_Serializer = null;
    public hkRootLevelContainer Selected_HKX3_Root = null;
    public HKX2.hkRootLevelContainer Selected_HKX2_Root = null;
    
    public object? SelectedGraphRoot = null;
    public NodeRepresentation SelectedGraphNode = null;

    public Dictionary<string, List<object>> DisplayCategories = new();
    public string SelectedFieldCategory = "";
    public List<object> SelectedFieldObjects = null;
    public bool ForceSelectObjectCategory = false;

    public int SelectedFieldObjectIndex = -1;
    public object SelectedFieldObject = null;
    public bool ForceSelectObject = false;

    public bool IsBinderSelected(FileDictionaryEntry curEntry)
    {
        if (SelectedFileEntry == null)
            return false;

        if(curEntry.Path == SelectedFileEntry.Path)
            return true;

        return false;
    }

    public void SelectBinder(FileDictionaryEntry entry, BinderContents contents)
    {
        SelectedFileEntry = entry;
        SelectedBinderContents = contents;

        SelectedInternalFileIndex = -1;
        SelectedInternalFileKey = "";
        SelectedInternalFileType = HavokCategoryType.None;

        Selected_HKX3_Serializer = null;
        Selected_HKX3_Root = null;

        Selected_HKX2_Root = null;

        SelectedGraphRoot = null;
        SelectedGraphNode = null;
        SelectedFieldObject = null;

        SelectedFieldCategory = "";
        SelectedFieldObjects = null;

        SelectedFieldObjectIndex = -1;
        SelectedFieldObject = null;

        // Undo/Redo is reset on file switch
        Editor.ActionManager.Clear();
    }

    public bool IsFileSelected(BinderFile curFile)
    {
        if(SelectedBinderFile == null) 
            return false;

        if (curFile.Name == SelectedBinderFile.Name)
            return true;

        return false;
    }

    public void SelectFile(int index, BinderFile curFile)
    {
        SelectedBinderFile = curFile;
        SelectedInternalFileKey = curFile.Name;
        SelectedInternalFileIndex = index;

        SelectedInternalFileType = BehaviorUtils.GetInternalFileCategoryType(curFile.Name);

        Selected_HKX3_Serializer = null;
        Selected_HKX3_Root = null;

        Selected_HKX2_Root = null;

        SelectedGraphRoot = null;
        SelectedGraphNode = null;
        SelectedFieldObject = null;

        SelectedFieldCategory = "";
        SelectedFieldObjects = null;

        SelectedFieldObjectIndex = -1;
        SelectedFieldObject = null;

        if (Project.ProjectType is ProjectType.ER)
        {
            SelectedRoot_HKX3(curFile);
        }
    }

    public bool IsCategorySelected(string categoryName)
    {
        if (categoryName == SelectedFieldCategory)
            return true;

        return false;
    }

    public void SelectCategory(string categoryName, List<object> objects)
    {
        SelectedFieldCategory = categoryName;
        SelectedFieldObjects = objects;

        SelectedFieldObjectIndex = -1;
        SelectedFieldObject = null;
    }

    public bool IsHavokObjectSelected(int index)
    {
        if (SelectedFieldObjectIndex == index)
            return true;

        return false;
    }

    public void SelectHavokObject(int index, object curObj)
    {
        SelectedFieldObjectIndex = index;
        SelectedFieldObject = curObj;
        SelectedGraphRoot = curObj;
    }

    #region HKX3
    public void SelectedRoot_HKX3(BinderFile curFile)
    {
        Selected_HKX3_Serializer = new HavokBinarySerializer();
        using (MemoryStream memoryStream = new MemoryStream(curFile.Bytes.ToArray()))
        {
            Selected_HKX3_Root = (hkRootLevelContainer)Selected_HKX3_Serializer.Read(memoryStream);
        }

        BuildDisplayCategories_HKX3(SelectedInternalFileType, Selected_HKX3_Root);
    }

    public void BuildDisplayCategories_HKX3(HavokCategoryType categoryType, hkRootLevelContainer root)
    {
        DisplayCategories.Clear();

        var categoryDict = new Dictionary<string, Type>();

        switch (categoryType)
        {
            case HavokCategoryType.None: return;
            case HavokCategoryType.Information:
                categoryDict = EldenRingBehaviorConsts.InformationCategories;
                break;
            case HavokCategoryType.Character:
                categoryDict = EldenRingBehaviorConsts.CharacterCategories;
                break;
            case HavokCategoryType.Behavior:
                categoryDict = EldenRingBehaviorConsts.BehaviorCategories;
                break;
        }

        foreach (var entry in categoryDict)
        {
            var category = entry.Key;
            var havokType = entry.Value;

            var newCategory = new List<object>();

            TraverseObjectTree(root, newCategory, havokType);

            DisplayCategories.Add(category, newCategory);
        }
    }
    #endregion

    #region Utils
    private void TraverseObjectTree(object? obj, List<object> entries, Type targetType, HashSet<object>? visited = null)
    {
        if (obj == null)
        {
            return;
        }

        visited ??= new HashSet<object>();
        if (!visited.Add(obj))
        {
            return;
        }

        Type type = obj.GetType();
        bool isLeaf = type.IsPrimitive || type == typeof(string) || type.IsEnum;

        if (obj.GetType() == targetType)
        {
            entries.Add(obj);
        }

        if (obj is IList list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                TraverseObjectTree(list[i], entries, targetType, visited);
            }
        }
        else
        {
            foreach (var prop in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object? value = prop.GetValue(obj);
                TraverseObjectTree(value, entries, targetType, visited);
            }
        }

        visited.Remove(obj);
    }
    #endregion
}
