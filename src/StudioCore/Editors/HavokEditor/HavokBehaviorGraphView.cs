using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKLib.hk2018;
using StudioCore.HavokEditor;
using static HKLib.hk2018.CustomLookAtTwistModifier;

namespace StudioCore.Editors.HavokEditor;

public class HavokBehaviorGraphView
{
    private HavokEditorScreen Screen;

    public HavokBehaviorGraphView(HavokEditorScreen screen)
    {
        Screen = screen;
    }
    public void DisplayProperties()
    {

    }

    public void DisplayGraph()
    {
        var loadedFile = Screen.SelectedContainerInfo.LoadedFile;

        if (loadedFile != null)
        {
            hkbBehaviorGraph behaviorGraph = (hkbBehaviorGraph)loadedFile.m_namedVariants[0].m_variant;

            if (ImGui.CollapsingHeader("hkbStateMachine"))
            {
                if (ImGui.TreeNodeEx("StateMachine"))
                {
                    // State Machines
                    hkbStateMachine hkbStateMachine = (hkbStateMachine)behaviorGraph.m_rootGenerator;

                    HavokStateMachineView(hkbStateMachine, 0);

                    ImGui.TreePop();
                }
            }

            if (ImGui.CollapsingHeader("General"))
            {
                ImGui.Text($"m_variableMode: {behaviorGraph.m_variableMode}");
            }

            // Information
            if (ImGui.CollapsingHeader("Attributes"))
            {
                // m_attributeDefaults
                for (int i = 0; i < behaviorGraph.m_data.m_attributeDefaults.Count; i++)
                {
                    ImGui.Text($"{i}: {behaviorGraph.m_data.m_attributeDefaults[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Variable Infos"))
            {
                // m_variableInfos
                for (int i = 0; i < behaviorGraph.m_data.m_variableInfos.Count; i++)
                {
                    ImGui.Text($"m_role: {i}: {behaviorGraph.m_data.m_variableInfos[i].m_role.m_role}");
                    ImGui.Text($"m_flags: {i}: {behaviorGraph.m_data.m_variableInfos[i].m_role.m_flags}");
                    ImGui.Text($"m_type: {i}: {behaviorGraph.m_data.m_variableInfos[i].m_type}");
                }
            }
            if (ImGui.CollapsingHeader("Character Property Infos"))
            {
                // m_characterPropertyInfos
                for (int i = 0; i < behaviorGraph.m_data.m_characterPropertyInfos.Count; i++)
                {
                    ImGui.Text($"m_role: {i}: {behaviorGraph.m_data.m_characterPropertyInfos[i].m_role.m_role}");
                    ImGui.Text($"m_flags: {i}: {behaviorGraph.m_data.m_characterPropertyInfos[i].m_role.m_flags}");
                    ImGui.Text($"m_type: {i}: {behaviorGraph.m_data.m_characterPropertyInfos[i].m_type}");
                }
            }
            if (ImGui.CollapsingHeader("Event Infos"))
            {
                // m_eventInfos
                for (int i = 0; i < behaviorGraph.m_data.m_eventInfos.Count; i++)
                {
                    ImGui.Text($"m_flags: {i}: {behaviorGraph.m_data.m_eventInfos[i].m_flags}");
                }
            }
            if (ImGui.CollapsingHeader("Variable Bounds"))
            {
                // m_variableBounds
                for (int i = 0; i < behaviorGraph.m_data.m_variableBounds.Count; i++)
                {
                    ImGui.Text($"m_min: {i}: {behaviorGraph.m_data.m_variableBounds[i].m_min.m_value}");
                    ImGui.Text($"m_max: {i}: {behaviorGraph.m_data.m_variableBounds[i].m_max.m_value}");
                }
            }
            if (ImGui.CollapsingHeader("Word Variable Values"))
            {
                // m_wordVariableValues
                for (int i = 0; i < behaviorGraph.m_data.m_variableInitialValues.m_wordVariableValues.Count; i++)
                {
                    ImGui.Text($"m_value: {i}: {behaviorGraph.m_data.m_variableInitialValues.m_wordVariableValues[i].m_value}");
                }
            }
            if (ImGui.CollapsingHeader("Quad Variable Values"))
            {
                // m_quadVariableValues
                for (int i = 0; i < behaviorGraph.m_data.m_variableInitialValues.m_quadVariableValues.Count; i++)
                {
                    ImGui.Text($"Vector4: {i}: {behaviorGraph.m_data.m_variableInitialValues.m_quadVariableValues[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Variant Variable Values"))
            {
                // m_variantVariableValues
                for (int i = 0; i < behaviorGraph.m_data.m_variableInitialValues.m_variantVariableValues.Count; i++)
                {
                    if (behaviorGraph.m_data.m_variableInitialValues.m_variantVariableValues[i] != null)
                        ImGui.Text($"m_propertyBag: {i}: {behaviorGraph.m_data.m_variableInitialValues.m_variantVariableValues[i].m_propertyBag}");
                }
            }
            if (ImGui.CollapsingHeader("Event Names"))
            {
                // m_eventNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_eventNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_eventNames[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Attribute Names"))
            {
                // m_attributeNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_attributeNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_attributeNames[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Variable Names"))
            {
                // m_variableNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_variableNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_variableNames[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Character Property Names"))
            {
                // m_characterPropertyNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_characterPropertyNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_characterPropertyNames[i]}");
                }
            }
            if (ImGui.CollapsingHeader("Animation Names"))
            {
                // m_animationNames
                for (int i = 0; i < behaviorGraph.m_data.m_stringData.m_animationNames.Count; i++)
                {
                    ImGui.Text($"m_eventName: {i}: {behaviorGraph.m_data.m_stringData.m_animationNames[i]}");
                }
            }
        }
    }

    // Recursive
    public void HavokStateMachineView(hkbStateMachine stateMachine, int depth)
    {
        if (stateMachine != null)
        {
            if (ImGui.TreeNodeEx($"HavokStateMachineView##HavokStateMachineView{depth}"))
            {
                ImGui.Text($"m_eventToSendWhenStateOrTransitionChanges: {stateMachine.m_eventToSendWhenStateOrTransitionChanges}");
                ImGui.Text($"m_startStateIdSelector: {stateMachine.m_startStateIdSelector}");
                ImGui.Text($"m_startStateId: {stateMachine.m_startStateId}");
                ImGui.Text($"m_returnToPreviousStateEventId: {stateMachine.m_returnToPreviousStateEventId}");
                ImGui.Text($"m_randomTransitionEventId: {stateMachine.m_randomTransitionEventId}");
                ImGui.Text($"m_transitionToNextHigherStateEventId: {stateMachine.m_transitionToNextHigherStateEventId}");
                ImGui.Text($"m_transitionToNextLowerStateEventId: {stateMachine.m_transitionToNextLowerStateEventId}");
                ImGui.Text($"m_syncVariableIndex: {stateMachine.m_syncVariableIndex}");
                ImGui.Text($"m_wrapAroundStateId: {stateMachine.m_wrapAroundStateId}");
                ImGui.Text($"m_maxSimultaneousTransitions: {stateMachine.m_maxSimultaneousTransitions}");
                ImGui.Text($"m_startStateMode: {stateMachine.m_startStateMode}");
                ImGui.Text($"m_selfTransitionMode: {stateMachine.m_selfTransitionMode}");
                ImGui.Text($"m_wildcardTransitions: {stateMachine.m_wildcardTransitions}");

                foreach (var entry in stateMachine.m_states)
                {
                    hkbStateMachineStateInfoView(entry, depth);
                }

                if (stateMachine.m_wildcardTransitions != null)
                {
                    foreach (var wEntry in stateMachine.m_wildcardTransitions.m_transitions)
                    {
                        ImGui.Text($"m_triggerInterval: {wEntry.m_triggerInterval}");
                        ImGui.Text($"m_initiateInterval: {wEntry.m_initiateInterval}");
                        ImGui.Text($"m_transition: {wEntry.m_transition}");
                        ImGui.Text($"m_condition: {wEntry.m_condition}");
                        ImGui.Text($"m_eventId: {wEntry.m_eventId}");
                        ImGui.Text($"m_toStateId: {wEntry.m_toStateId}");
                        ImGui.Text($"m_fromNestedStateId: {wEntry.m_fromNestedStateId}");
                        ImGui.Text($"m_priority: {wEntry.m_priority}");
                        ImGui.Text($"m_flags: {wEntry.m_flags}");
                    }
                }

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbStateMachineStateInfoView(hkbStateMachine.StateInfo entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbStateMachineStateInfoView##hkbStateMachineStateInfoView{depth}"))
            {
                ImGui.Text($"m_listeners: {entry.m_listeners}");

                if (entry.m_enterNotifyEvents != null)
                {
                    foreach (var sEntry in entry.m_enterNotifyEvents.m_events)
                    {
                        ImGui.Text($"m_events: {sEntry}");
                    }
                }
                if (entry.m_exitNotifyEvents != null)
                {
                    foreach (var sEntry in entry.m_exitNotifyEvents.m_events)
                    {
                        ImGui.Text($"m_events: {sEntry}");
                    }
                }
                if (entry.m_transitions != null)
                {
                    foreach (var tEntry in entry.m_transitions.m_transitions)
                    {
                        ImGui.Text($"m_triggerInterval: {tEntry.m_triggerInterval}");
                        ImGui.Text($"m_initiateInterval: {tEntry.m_initiateInterval}");
                        ImGui.Text($"m_transition: {tEntry.m_transition}");
                        ImGui.Text($"m_condition: {tEntry.m_condition}");
                        ImGui.Text($"m_eventId: {tEntry.m_eventId}");
                        ImGui.Text($"m_toStateId: {tEntry.m_toStateId}");
                        ImGui.Text($"m_fromNestedStateId: {tEntry.m_fromNestedStateId}");
                        ImGui.Text($"m_priority: {tEntry.m_priority}");
                        ImGui.Text($"m_flags: {tEntry.m_flags}");
                    }
                }

                if (entry.m_generator is hkbScriptGenerator)
                {
                    var scriptGenerator = (hkbScriptGenerator)entry.m_generator;
                    hkbScriptGeneratorView(scriptGenerator, depth);
                }
                else if (entry.m_generator is hkbBlenderGenerator)
                {
                    var blenderGenerator = (hkbBlenderGenerator)entry.m_generator;
                    hkbBlenderGeneratorView(blenderGenerator, depth);
                }
                else
                {
                    TaskLogs.AddLog($"hkbStateMachineStateInfoView: Unimplemented type: {entry.m_generator.GetType()}");
                }

                ImGui.Text($"m_name: {entry.m_name}");
                ImGui.Text($"m_stateId: {entry.m_stateId}");
                ImGui.Text($"m_probability: {entry.m_probability}");
                ImGui.Text($"m_enable: {entry.m_enable}");

                ImGui.TreePop();
            }
        }
        depth++;
    }

    public void hkbScriptGeneratorView(hkbScriptGenerator entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbScriptGeneratorView##hkbScriptGeneratorView{depth}"))
            {
                ImGui.Text($"m_name: {entry.m_name}");

                var variableBindingSet = (hkbVariableBindingSet)entry.m_variableBindingSet;
                variableBindingSetView(variableBindingSet, depth);

                ImGui.Text($"m_userData: {entry.m_userData}");
                ImGui.Text($"m_name: {entry.m_name}");

                if (entry.m_child is hkbModifierGenerator)
                {
                    var child_generator = (hkbModifierGenerator)entry.m_child;
                    hkbModifierGeneratorView(child_generator, depth);
                }
                else if (entry.m_child is hkbBlenderGenerator)
                {
                    var child_generator = (hkbBlenderGenerator)entry.m_child;
                    hkbBlenderGeneratorView(child_generator, depth);
                }
                else
                {
                    TaskLogs.AddLog($"hkbScriptGeneratorView: Unimplemented type: {entry.m_child.GetType()}");
                }

                ImGui.Text($"m_onActivateScript: {entry.m_onActivateScript}");
                ImGui.Text($"m_onPreUpdateScript: {entry.m_onPreUpdateScript}");
                ImGui.Text($"m_onGenerateScript: {entry.m_onGenerateScript}");
                ImGui.Text($"m_onHandleEventScript: {entry.m_onHandleEventScript}");
                ImGui.Text($"m_onDeactivateScript: {entry.m_onDeactivateScript}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbBlenderGeneratorChildView(hkbBlenderGeneratorChild entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbBlenderGeneratorChildView##hkbBlenderGeneratorChildView{depth}"))
            {
                if (entry.m_generator is hkbScriptGenerator)
                {
                    var scriptGenerator = (hkbScriptGenerator)entry.m_generator;
                    hkbScriptGeneratorView(scriptGenerator, depth);
                }
                else if (entry.m_generator is hkbBlenderGenerator)
                {
                    var blenderGenerator = (hkbBlenderGenerator)entry.m_generator;
                    hkbBlenderGeneratorView(blenderGenerator, depth);
                }
                else
                {
                    TaskLogs.AddLog($"hkbStateMachineStateInfoView: Unimplemented type: {entry.m_generator.GetType()}");
                }

                foreach (var bEntry in entry.m_boneWeights.m_boneWeights)
                {
                    ImGui.Text($"m_boneWeight: {bEntry}");
                }

                ImGui.Text($"m_weight: {entry.m_weight}");
                ImGui.Text($"m_worldFromModelWeight: {entry.m_worldFromModelWeight}");

                ImGui.TreePop();
            }
        }
        depth++;
    }

    public void variableBindingSetView(hkbVariableBindingSet entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"variableBindingSetView##variableBindingSetView{depth}"))
            {
                foreach (var bEntry in entry.m_bindings)
                {
                    ImGui.Text($"m_memberPath: {bEntry.m_memberPath}");
                    ImGui.Text($"m_variableIndex: {bEntry.m_variableIndex}");
                    ImGui.Text($"m_bitIndex: {bEntry.m_bitIndex}");
                    ImGui.Text($"m_bindingType: {bEntry.m_bindingType}");
                }

                ImGui.Text($"m_indexOfBindingToEnable: {entry.m_indexOfBindingToEnable}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbModifierGeneratorView(hkbModifierGenerator entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbModifierGeneratorView##hkbModifierGeneratorView{depth}"))
            {
                var variableBindingSet = (hkbVariableBindingSet)entry.m_variableBindingSet;
                variableBindingSetView(variableBindingSet, depth);

                ImGui.Text($"m_userData: {entry.m_userData}");
                ImGui.Text($"m_name: {entry.m_name}");

                if (entry.m_modifier is hkbModifierList)
                {
                    var modifierList = (hkbModifierList)entry.m_modifier;

                    if (modifierList != null)
                        hkbModifierListView(modifierList, depth);
                }
                else
                {
                    TaskLogs.AddLog($"hkbModifierGenerator: Unimplemented type: {entry.m_modifier.GetType()}");
                }

                var scriptGenerator = (hkbScriptGenerator)entry.m_generator;

                if (scriptGenerator != null)
                    hkbScriptGeneratorView(scriptGenerator, depth);

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbBlenderGeneratorView(hkbBlenderGenerator entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbBlenderGeneratorView##hkbBlenderGeneratorView{depth}"))
            {
                ImGui.Text($"m_referencePoseWeightThreshold: {entry.m_referencePoseWeightThreshold}");
                ImGui.Text($"m_blendParameter: {entry.m_blendParameter}");
                ImGui.Text($"m_minCyclicBlendParameter: {entry.m_minCyclicBlendParameter}");
                ImGui.Text($"m_maxCyclicBlendParameter: {entry.m_maxCyclicBlendParameter}");
                ImGui.Text($"m_indexOfSyncMasterChild: {entry.m_indexOfSyncMasterChild}");
                ImGui.Text($"m_flags: {entry.m_flags}");
                ImGui.Text($"m_subtractLastChild: {entry.m_subtractLastChild}");

                foreach(var sEntry in entry.m_children)
                {
                    if (sEntry != null)
                        hkbBlenderGeneratorChildView(sEntry, depth);
                }

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbModifierListView(hkbModifierList entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbModifierListView##hkbModifierListView{depth}"))
            {
                var variableBindingSet = (hkbVariableBindingSet)entry.m_variableBindingSet;
                variableBindingSetView(variableBindingSet, depth);

                ImGui.Text($"m_userData: {entry.m_userData}");
                ImGui.Text($"m_name: {entry.m_name}");
                ImGui.Text($"m_enable: {entry.m_enable}");

                foreach (var mEntry in entry.m_modifiers)
                {
                    if (mEntry is hkbTwistModifier)
                    {
                        hkbTwistModifierView((hkbTwistModifier)mEntry, depth);
                    }
                    else if (mEntry is hkbModifierList)
                    {
                        hkbModifierListView((hkbModifierList)mEntry, depth);
                    }
                    else if (mEntry is CustomLookAtTwistModifier)
                    {
                        CustomLookAtTwistModifierView((CustomLookAtTwistModifier)mEntry, depth);
                    }
                    else if (mEntry is hkbGetHandleOnBoneModifier)
                    {
                        hkbGetHandleOnBoneModifierView((hkbGetHandleOnBoneModifier)mEntry, depth);
                    }
                    else if (mEntry is hkbEvaluateHandleModifier)
                    {
                        hkbEvaluateHandleModifierView((hkbEvaluateHandleModifier)mEntry, depth);
                    }
                    else if (mEntry is hkbHandIkModifier)
                    {
                        hkbHandIkModifierView((hkbHandIkModifier)mEntry, depth);
                    }
                    else if (mEntry is hkbFootIkModifier)
                    {
                        hkbFootIkModifierView((hkbFootIkModifier)mEntry, depth);
                    }
                    else
                    {
                        TaskLogs.AddLog($"hkbModifierListView: Unimplemented type: {mEntry.GetType()}");
                    }
                }

                ImGui.TreePop();
            }
        }

        depth++;
    }
    public void hkbTwistModifierView(hkbTwistModifier entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbTwistModifierView##hkbTwistModifierView{depth}"))
            {
                var variableBindingSet = (hkbVariableBindingSet)entry.m_variableBindingSet;
                variableBindingSetView(variableBindingSet, depth);

                ImGui.Text($"m_userData: {entry.m_userData}");
                ImGui.Text($"m_name: {entry.m_name}");
                ImGui.Text($"m_enable: {entry.m_enable}");
                ImGui.Text($"m_axisOfRotation: {entry.m_axisOfRotation}");
                ImGui.Text($"m_twistAngle: {entry.m_twistAngle}");
                ImGui.Text($"m_startBoneIndex: {entry.m_startBoneIndex}");
                ImGui.Text($"m_endBoneIndex: {entry.m_endBoneIndex}");
                ImGui.Text($"m_setAngleMethod: {entry.m_setAngleMethod}");
                ImGui.Text($"m_rotationAxisCoordinates: {entry.m_rotationAxisCoordinates}");
                ImGui.Text($"m_isAdditive: {entry.m_isAdditive}");

                ImGui.TreePop();
            }
        }

        depth++;
    }
    public void CustomLookAtTwistModifierView(CustomLookAtTwistModifier entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"CustomLookAtTwistModifierView##CustomLookAtTwistModifierView{depth}"))
            {
                ImGui.Text($"m_ModifierID: {entry.m_ModifierID}");
                ImGui.Text($"m_rotationAxisType: {entry.m_rotationAxisType}");
                ImGui.Text($"m_SensingDummyPoly: {entry.m_SensingDummyPoly}");

                foreach(var sEntry in entry.m_twistParam)
                {
                    TwistParamView((TwistParam)sEntry, depth);
                }

                ImGui.Text($"m_UpLimitAngle: {entry.m_UpLimitAngle}");
                ImGui.Text($"m_DownLimitAngle: {entry.m_DownLimitAngle}");
                ImGui.Text($"m_RightLimitAngle: {entry.m_RightLimitAngle}");
                ImGui.Text($"m_LeftLimitAngle: {entry.m_LeftLimitAngle}");
                ImGui.Text($"m_UpMinimumAngle: {entry.m_UpMinimumAngle}");
                ImGui.Text($"m_DownMinimumAngle: {entry.m_DownMinimumAngle}");
                ImGui.Text($"m_RightMinimumAngle: {entry.m_RightMinimumAngle}");
                ImGui.Text($"m_LeftMinimumAngle: {entry.m_LeftMinimumAngle}");
                ImGui.Text($"m_SensingAngle: {entry.m_SensingAngle}");
                ImGui.Text($"m_setAngleMethod: {entry.m_setAngleMethod}");
                ImGui.Text($"m_isAdditive: {entry.m_isAdditive}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void TwistParamView(TwistParam entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"TwistParamView##TwistParamView{depth}"))
            {
                ImGui.Text($"m_startBoneIndex: {entry.m_startBoneIndex}");
                ImGui.Text($"m_endBoneIndex: {entry.m_endBoneIndex}");
                ImGui.Text($"m_targetRotationRate: {entry.m_targetRotationRate}");
                ImGui.Text($"m_newTargetGain: {entry.m_newTargetGain}");
                ImGui.Text($"m_onGain: {entry.m_onGain}");
                ImGui.Text($"m_offGain: {entry.m_offGain}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbGetHandleOnBoneModifierView(hkbGetHandleOnBoneModifier entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbGetHandleOnBoneModifierView##hkbGetHandleOnBoneModifierView{depth}"))
            {
                ImGui.Text($"m_handleOut: {entry.m_handleOut}");
                ImGui.Text($"m_localFrameName: {entry.m_localFrameName}");
                ImGui.Text($"m_ragdollBoneIndex: {entry.m_ragdollBoneIndex}");
                ImGui.Text($"m_animationBoneIndex: {entry.m_animationBoneIndex}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbEvaluateHandleModifierView(hkbEvaluateHandleModifier entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbEvaluateHandleModifierView##hkbEvaluateHandleModifierView{depth}"))
            {
                ImGui.Text($"m_handle: {entry.m_handle}");
                ImGui.Text($"m_handlePositionOut: {entry.m_handlePositionOut}");
                ImGui.Text($"m_handleRotationOut: {entry.m_handleRotationOut}");
                ImGui.Text($"m_isValidOut: {entry.m_isValidOut}");
                ImGui.Text($"m_extrapolationTimeStep: {entry.m_extrapolationTimeStep}");
                ImGui.Text($"m_handleChangeSpeed: {entry.m_handleChangeSpeed}");
                ImGui.Text($"m_handleChangeMode: {entry.m_handleChangeMode}");

                ImGui.TreePop();
            }
        }

        depth++;
    }

    public void hkbHandIkModifierView(hkbHandIkModifier entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbHandIkModifierView##hkbHandIkModifierView{depth}"))
            {
                foreach(var sEntry in entry.m_hands)
                {
                    hkbHandIkModifierHandView((hkbHandIkModifier.Hand)sEntry, depth);
                }

                ImGui.Text($"m_fadeInOutCurve: {entry.m_fadeInOutCurve}");

                ImGui.TreePop();
            }
        }

        depth++;
    }
    public void hkbHandIkModifierHandView(hkbHandIkModifier.Hand entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbHandIkModifierHandView##hkbHandIkModifierHandView{depth}"))
            {
                ImGui.Text($"m_elbowAxisLS: {entry.m_elbowAxisLS}");
                ImGui.Text($"m_backHandNormalLS: {entry.m_backHandNormalLS}");
                ImGui.Text($"m_handOffsetLS: {entry.m_handOffsetLS}");
                ImGui.Text($"m_handOrientationOffsetLS: {entry.m_handOrientationOffsetLS}");
                ImGui.Text($"m_maxElbowAngleDegrees: {entry.m_maxElbowAngleDegrees}");
                ImGui.Text($"m_minElbowAngleDegrees: {entry.m_minElbowAngleDegrees}");
                ImGui.Text($"m_shoulderIndex: {entry.m_shoulderIndex}");
                ImGui.Text($"m_shoulderSiblingIndex: {entry.m_shoulderSiblingIndex}");
                ImGui.Text($"m_elbowIndex: {entry.m_elbowIndex}");
                ImGui.Text($"m_elbowSiblingIndex: {entry.m_elbowSiblingIndex}");
                ImGui.Text($"m_fadm_wristIndexeInOutCurve: {entry.m_wristIndex}");
                ImGui.Text($"m_enforceEndPosition: {entry.m_enforceEndPosition}");
                ImGui.Text($"m_enforceEndRotation: {entry.m_enforceEndRotation}");
                ImGui.Text($"m_localFrameName: {entry.m_localFrameName}");

                ImGui.TreePop();
            }
        }

        depth++;
    }
    public void hkbFootIkModifierView(hkbFootIkModifier entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbHandIkModifierView##hkbHandIkModifierView{depth}"))
            {
                hkbFootIkGainsView((hkbFootIkGains)entry.m_gains, depth);

                foreach (var sEntry in entry.m_legs)
                {
                    hkbFootIkModifierLegView((hkbFootIkModifier.Leg)sEntry, depth);
                }

                ImGui.Text($"m_raycastDistanceUp: {entry.m_raycastDistanceUp}");
                ImGui.Text($"m_raycastDistanceDown: {entry.m_raycastDistanceDown}");
                ImGui.Text($"m_originalGroundHeightMS: {entry.m_originalGroundHeightMS}");
                ImGui.Text($"m_errorOut: {entry.m_errorOut}");
                ImGui.Text($"m_verticalOffset: {entry.m_verticalOffset}");
                ImGui.Text($"m_collisionFilterInfo: {entry.m_collisionFilterInfo}");
                ImGui.Text($"m_forwardAlignFraction: {entry.m_forwardAlignFraction}");
                ImGui.Text($"m_sidewaysAlignFraction: {entry.m_sidewaysAlignFraction}");
                ImGui.Text($"m_sidewaysSampleWidth: {entry.m_sidewaysSampleWidth}");
                ImGui.Text($"m_useTrackData: {entry.m_useTrackData}");
                ImGui.Text($"m_lockFeetWhenPlanted: {entry.m_lockFeetWhenPlanted}");
                ImGui.Text($"m_useCharacterUpVector: {entry.m_useCharacterUpVector}");
                ImGui.Text($"m_keepSourceFootEndAboveGround: {entry.m_keepSourceFootEndAboveGround}");
                ImGui.Text($"m_alignMode: {entry.m_alignMode}");

                ImGui.TreePop();
            }
        }
    }
    public void hkbFootIkGainsView(hkbFootIkGains entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbFootIkGainsView##hkbFootIkGainsView{depth}"))
            {
                ImGui.Text($"m_onOffGain: {entry.m_onOffGain}");
                ImGui.Text($"m_groundAscendingGain: {entry.m_groundAscendingGain}");
                ImGui.Text($"m_groundDescendingGain: {entry.m_groundDescendingGain}");
                ImGui.Text($"m_footPlantedGain: {entry.m_footPlantedGain}");
                ImGui.Text($"m_footRaisedGain: {entry.m_footRaisedGain}");
                ImGui.Text($"m_footLockingGain: {entry.m_footLockingGain}");
                ImGui.Text($"m_ankleRotationGain: {entry.m_ankleRotationGain}");
                ImGui.Text($"m_worldFromModelFeedbackGain: {entry.m_worldFromModelFeedbackGain}");
                ImGui.Text($"m_errorUpDownBias: {entry.m_errorUpDownBias}");
                ImGui.Text($"m_alignWorldFromModelGain: {entry.m_alignWorldFromModelGain}");
                ImGui.Text($"m_hipOrientationGain: {entry.m_hipOrientationGain}");

                ImGui.TreePop();
            }
        }
    }
    public void hkbFootIkModifierLegView(hkbFootIkModifier.Leg entry, int depth)
    {
        if (entry != null)
        {
            if (ImGui.TreeNodeEx($"hkbFootIkModifierLegView##hkbFootIkModifierLegView{depth}"))
            {
                ImGui.Text($"m_originalAnkleTransformMS: {entry.m_originalAnkleTransformMS}");
                ImGui.Text($"m_kneeAxisLS: {entry.m_kneeAxisLS}");
                ImGui.Text($"m_footEndLS: {entry.m_footEndLS}");
                ImGui.Text($"m_ungroundedEvent: {entry.m_ungroundedEvent}");
                ImGui.Text($"m_footPlantedAnkleHeightMS: {entry.m_footPlantedAnkleHeightMS}");
                ImGui.Text($"m_footRaisedAnkleHeightMS: {entry.m_footRaisedAnkleHeightMS}");
                ImGui.Text($"m_maxAnkleHeightMS: {entry.m_maxAnkleHeightMS}");
                ImGui.Text($"m_minAnkleHeightMS: {entry.m_minAnkleHeightMS}");
                ImGui.Text($"m_maxFootPitchDegrees: {entry.m_maxFootPitchDegrees}");
                ImGui.Text($"m_minFootPitchDegrees: {entry.m_minFootPitchDegrees}");
                ImGui.Text($"m_maxFootRollDegrees: {entry.m_maxFootRollDegrees}");
                ImGui.Text($"m_minFootRollDegrees: {entry.m_minFootRollDegrees}");
                ImGui.Text($"m_heelOffsetFromAnkle: {entry.m_heelOffsetFromAnkle}");
                ImGui.Text($"m_favorToeInterpenetrationOverSteepSlope: {entry.m_favorToeInterpenetrationOverSteepSlope}");
                ImGui.Text($"m_favorHeelInterpenetrationOverSteepSlope: {entry.m_favorHeelInterpenetrationOverSteepSlope}");
                ImGui.Text($"m_maxKneeAngleDegrees: {entry.m_maxKneeAngleDegrees}");
                ImGui.Text($"m_minKneeAngleDegrees: {entry.m_minKneeAngleDegrees}");
                ImGui.Text($"m_verticalError: {entry.m_verticalError}");
                ImGui.Text($"m_hipIndex: {entry.m_hipIndex}");
                ImGui.Text($"m_kneeIndex: {entry.m_kneeIndex}");
                ImGui.Text($"m_ankleIndex: {entry.m_ankleIndex}");
                ImGui.Text($"m_hitSomething: {entry.m_hitSomething}");
                ImGui.Text($"m_isPlantedMS: {entry.m_isPlantedMS}");
                ImGui.Text($"m_isOriginalAnkleTransformMSSet: {entry.m_isOriginalAnkleTransformMSSet}");

                ImGui.TreePop();
            }
        }
    }
}
