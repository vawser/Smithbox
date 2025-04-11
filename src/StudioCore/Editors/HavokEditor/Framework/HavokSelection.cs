using HKLib.hk2018;
using HKLib.hk2018.hkAsyncThreadPool;
using StudioCore.Editors.HavokEditor.Data;
using StudioCore.Editors.HavokEditor.Enums;
using StudioCore.Editors.HavokEditor.Util;
using StudioCore.HavokEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Framework;

public class HavokSelection
{
    private HavokEditorScreen Screen;

    public HavokContainerInfo Container;
    public string BinderKey = "";
    public string FileKey = "";

    public HavokContainerType ContainerType = HavokContainerType.Behavior;
    private HavokInternalType FileType = HavokInternalType.None;

    public HavokSelection(HavokEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnProjectChanged()
    {
        Container = null;
        BinderKey = "";
        FileKey = "";
        ContainerType = HavokContainerType.Behavior;
        FileType = HavokInternalType.None;
    }

    /// <summary>
    /// Container
    /// </summary>
    public HavokContainerInfo GetContainer()
    {
        return Container;
    }

    public HavokContainerType GetContainerType()
    {
        return ContainerType;
    }

    public void SetContainerType(HavokContainerType containerType)
    {
        ContainerType = containerType;
    }

    public void MarkAsModified()
    {
        Container.IsModified = true;
    }

    public void MarkAsUnmodified()
    {
        Container.IsModified = false;
    }

    public int GetBinderFileCount()
    {
        if(Container == null || Container.InternalFileList == null)
        {
            return -1;
        }
        else
        {
            return Container.InternalFileList.Count;
        }
    }

    public string GetBinderKey()
    {
        return BinderKey;
    }

    public void SelectNewContainer(HavokContainerInfo newContainer)
    {
        Container = newContainer;
        BinderKey = newContainer.Filename;

        newContainer.LoadBinder();
    }

    /// <summary>
    /// File
    /// </summary>

    public string GetFileKey()
    {
        return FileKey;
    }

    public HavokInternalType GetFileType(string filename)
    {
        var internalTypeKey = HavokInternalType.None;

        if (GetContainerType() == HavokContainerType.Behavior)
        {
            if (filename.Contains("behaviors"))
            {
                internalTypeKey = HavokInternalType.Behavior;
            }
            else if (filename.Contains("characters"))
            {
                internalTypeKey = HavokInternalType.Character;
            }
            else
            {
                internalTypeKey = HavokInternalType.Info;
            }
        }
        else if (GetContainerType() == HavokContainerType.Collision)
        {
            internalTypeKey = HavokInternalType.Collision;
        }
        else if (GetContainerType() == HavokContainerType.Animation)
        {
            internalTypeKey = HavokInternalType.Animation;
        }

        return internalTypeKey;
    }

    public Dictionary<Type, List<object>> ObjectHierarchy = new();

    public Type SelectedObjectClass;
    public int SelectedObjectClassEntryIndex = -1;
    public IHavokObject SelectedObjectEntry;

    public void SelectNewFile(string newKey, HavokInternalType internalType)
    {
        FileKey = newKey.ToLower();
        FileType = internalType;

        var root = Container.ReadHavokRoot(FileKey, FileType.ToString());
        ObjectHierarchy = ObjectHierarchyScanner.GetAllObjectsByType(root);

        SelectedObjectClass = null;
        SelectedObjectClassEntryIndex = -1;
    }

    public void SelectNewClass(Type newType)
    {
        SelectedObjectClass = newType;
        SelectedObjectClassEntryIndex = 0;
    }

    public void SelectNewClassEntry(int index)
    {
        SelectedObjectClassEntryIndex = index;
    }

    /// <summary>
    /// Data
    /// </summary>
    /// 

    private Dictionary<Type, HavokClassMeta> AssociatedMetas = new();

    public HavokClassMeta GetClassMeta(Type curType)
    {
        if (curType == null)
            return null;

        if (AssociatedMetas.ContainsKey(curType))
        {
            return AssociatedMetas[curType];
        }
        else
        {
            var className = $"{curType}".Replace("HKLib.hk2018.", "").Replace("+", "_");

            var meta = HavokMeta.GetClassMeta(className);

            if(meta != null)
                AssociatedMetas.Add(curType, meta);

            return meta;
        }
    }

    public HavokFieldMeta GetFieldMeta(string fieldName)
    {
        if (SelectedObjectClass == null)
            return null;

        if (AssociatedMetas.ContainsKey(SelectedObjectClass))
        {
            var classMeta = AssociatedMetas[SelectedObjectClass];

            if(classMeta.Fields.ContainsKey(fieldName))
            {
                return classMeta.Fields[fieldName];
            }
        }

        return null;
    }
}

