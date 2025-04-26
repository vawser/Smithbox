using HKLib.hk2018;
using StudioCore.Core.ProjectNS;
using StudioCore.Resources.JSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.BehaviorEditorNS;

public class BehaviorData
{
    public Project Project;

    public BehaviorBank PrimaryBank;

    public FileDictionary BehaviorFiles;

    public BehaviorData(Project projectOwner)
    {
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        BehaviorFiles = new();
        BehaviorFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "behbnd").ToList();
    }

    // Categories
    private Dictionary<string, Type> HavokCategories_Behavior_ER = new Dictionary<string, Type>()
    {
        {  "hkbStateMachine", typeof(hkbStateMachine) },
        {  "hkbScriptGenerator", typeof(hkbScriptGenerator) },
        {  "hkbModifierGenerator", typeof(hkbModifierGenerator) },
        {  "hkbModifierList", typeof(hkbModifierList) },
        {  "CustomLookAtTwistModifier", typeof(CustomLookAtTwistModifier) },
        {  "hkbGetHandleOnBoneModifier", typeof(hkbGetHandleOnBoneModifier) },
        {  "hkbEvaluateHandleModifier", typeof(hkbEvaluateHandleModifier) },
        {  "hkbHandIkControlsModifier", typeof(hkbHandIkControlsModifier) },
        {  "hkbTwistModifier", typeof(hkbTwistModifier) },
        {  "hkbFootIkControlsModifier", typeof(hkbFootIkControlsModifier) },
        {  "hkbBlenderGenerator", typeof(hkbBlenderGenerator) },
        {  "hkbLayerGenerator", typeof(hkbLayerGenerator) },
        {  "CustomManualSelectorGenerator", typeof(CustomManualSelectorGenerator) },
        {  "hkbClipGenerator", typeof(hkbClipGenerator) },
        {  "hkbManualSelectorGenerator", typeof(hkbManualSelectorGenerator) },
        {  "hkbManualSelectorTransitionEffect", typeof(hkbManualSelectorTransitionEffect) },
        {  "hkbGeneratorTransitionEffect", typeof(hkbGeneratorTransitionEffect) }

        // Handled via 'Variables' window
        // {  "hkbVariableBindingSet", typeof(hkbVariableBindingSet) },
        // {  "hkbVariableInfo", typeof(hkbVariableInfo) },
        // {  "hkbVariableBounds", typeof(hkbVariableBounds) },
        // {  "hkbVariableValue", typeof(hkbVariableValue) },
        // {  "hkbVariableValueSet", typeof(hkbVariableValueSet) }

        // Handled via 'Behavior Graph Properties' window
        // {  "hkbBehaviorGraph", typeof(hkbBehaviorGraph) },
        // {  "hkbBehaviorGraphData", typeof(hkbBehaviorGraphData) },
        // {  "hkbBehaviorGraphStringData", typeof(hkbBehaviorGraphStringData) }
        
        // Blocked for now as node editor can't handle the massive graph it produces
        // {  "hkRootLevelContainer", typeof(hkRootLevelContainer) },

        // Misc
        // {  "hkbBoneWeightArray", typeof(hkbBoneWeightArray) },
        // {  "hkbEvent", typeof(hkbEvent) },
        // {  "hkbRoleAttribute", typeof(hkbRoleAttribute) },
        // {  "hkbEventInfo", typeof(hkbEventInfo) },
        // {  "hkbHandIkControlData", typeof(hkbHandIkControlData) },
        // {  "hkbFootIkControlData", typeof(hkbFootIkControlData) },
        // {  "hkbFootIkGains", typeof(hkbFootIkGains) },
        // {  "hkbEventProperty", typeof(hkbEventProperty) },
        // {  "hkbBlenderGeneratorChild", typeof(hkbBlenderGeneratorChild) },
        // {  "hkbLayer", typeof(hkbLayer) },
        // {  "hkbEventDrivenBlendingObject", typeof(hkbEventDrivenBlendingObject) },
        // {  "hkbBlendingTransitionEffect", typeof(hkbBlendingTransitionEffect) },
        // {  "CustomTransitionEffect", typeof(CustomTransitionEffect) },
        // {  "hkbHoldFromBlendingTransitionEffect", typeof(hkbHoldFromBlendingTransitionEffect) },
        // {  "hkPropertyBag", typeof(hkPropertyBag) },
    };

    private Dictionary<string, Type> HavokCategories_Information_ER = new Dictionary<string, Type>()
    {
        {  "hkbProjectData", typeof(hkbProjectData) },
        {  "hkbProjectStringData", typeof(hkbProjectStringData) }
    };

    private Dictionary<string, Type> HavokCategories_Character_ER = new Dictionary<string, Type>()
    {
        {  "hkbCharacterData", typeof(hkbCharacterData) },
        {  "hkbCharacterControllerSetup", typeof(hkbCharacterControllerSetup) },
        {  "hkbRigidBodySetup", typeof(hkbRigidBodySetup) },
        {  "hkbCharacterStringData", typeof(hkbCharacterStringData) },
        {  "hkbAssetBundleStringData", typeof(hkbAssetBundleStringData) },
        {  "hkbFootIkDriverInfo", typeof(hkbFootIkDriverInfo) },
        {  "hkbHandIkDriverInfo", typeof(hkbHandIkDriverInfo) },
        {  "hkbMirroredSkeletonInfo", typeof(hkbMirroredSkeletonInfo) }

        // {  "hkbRoleAttribute", typeof(hkbRoleAttribute) },
        // {  "hkbVariableBindingSet", typeof(hkbVariableBindingSet) },
        // {  "hkbVariableInfo", typeof(hkbVariableInfo) },
        // {  "hkbVariableBounds", typeof(hkbVariableBounds) },
        // {  "hkbVariableValue", typeof(hkbVariableValue) },
        // {  "hkbVariableValueSet", typeof(hkbVariableValueSet) }
    };

    public Dictionary<string, List<object>> Categories = new();

    public void BuildCategories(HavokCategoryType categoryType, hkRootLevelContainer root)
    {
        Categories.Clear();

        var categoryDict = new Dictionary<string, Type>();

        switch (categoryType)
        {
            case HavokCategoryType.None: return;
            case HavokCategoryType.Information:
                categoryDict = HavokCategories_Information_ER;
                break;
            case HavokCategoryType.Character:
                categoryDict = HavokCategories_Character_ER;
                break;
            case HavokCategoryType.Behavior:
                categoryDict = HavokCategories_Behavior_ER;
                break;
        }

        foreach (var entry in categoryDict)
        {
            var category = entry.Key;
            var havokType = entry.Value;

            var newCategory = new List<object>();
            TraverseObjectTree(root, newCategory, havokType);
            Categories.Add(category, newCategory);
        }
    }

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
}
