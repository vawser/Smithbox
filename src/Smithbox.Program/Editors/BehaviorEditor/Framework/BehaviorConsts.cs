using HKLib.hk2018;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class EldenRingBehaviorConsts
{
    /// <summary>
    /// The havok classes to display as discrete classes when looking at a behavior hkx for a behbnd
    /// </summary>
    public static Dictionary<string, Type> BehaviorCategories = new Dictionary<string, Type>()
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

    /// <summary>
    /// The havok classes to display as discrete classes when looking at a information hkx for a behbnd
    /// </summary>
    public static Dictionary<string, Type> InformationCategories = new Dictionary<string, Type>()
    {
        {  "hkbProjectData", typeof(hkbProjectData) },
        {  "hkbProjectStringData", typeof(hkbProjectStringData) }
    };

    /// <summary>
    /// The havok classes to display as discrete classes when looking at a character hkx for a behbnd
    /// </summary>
    public static Dictionary<string, Type> CharacterCategories = new Dictionary<string, Type>()
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
}
/// <summary>
/// Determines which categories to use.
/// </summary>
public enum HavokCategoryType
{
    None,
    Information,
    Character,
    Behavior
}
