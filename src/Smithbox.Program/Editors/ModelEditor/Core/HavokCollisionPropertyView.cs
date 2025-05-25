using HKLib.hk2018;
using Hexa.NET.ImGui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Core
{
    public class HavokCollisionPropertyView
    {
        private ModelEditorScreen Editor;

        public HavokCollisionPropertyView(ModelEditorScreen screen)
        {
            Editor = screen;
        }

        // TODO: do this via reflection
        public void Display(hkRootLevelContainer container)
        {
            hkRootLevelContainer rootLevelContainer = container;
            hknpPhysicsSceneData physicsSceneData = (hknpPhysicsSceneData)rootLevelContainer.m_namedVariants[0].m_variant;

            // System Data
            foreach (var sysEntry in physicsSceneData.m_systemDatas)
            {
                ImGui.Separator();
                ImGui.Text($"Materials");
                ImGui.Separator();

                // Materials
                foreach (var matEntry in sysEntry.m_materials)
                {
                    ImGui.Text($"m_name: {matEntry.m_name}");
                    ImGui.Text($"m_isExclusive: {matEntry.m_isExclusive}");
                    ImGui.Text($"m_flags: {matEntry.m_flags}");
                    ImGui.Text($"m_triggerType: {matEntry.m_triggerType}");
                    ImGui.Text($"m_triggerManifoldTolerance: {matEntry.m_triggerManifoldTolerance}");
                    ImGui.Text($"m_dynamicFriction: {matEntry.m_dynamicFriction}");
                    ImGui.Text($"m_staticFriction: {matEntry.m_staticFriction}");
                    ImGui.Text($"m_restitution: {matEntry.m_restitution}");
                    ImGui.Text($"m_frictionCombinePolicy: {matEntry.m_frictionCombinePolicy}");
                    ImGui.Text($"m_restitutionCombinePolicy: {matEntry.m_restitutionCombinePolicy}");
                    ImGui.Text($"m_weldingTolerance: {matEntry.m_weldingTolerance}");
                    ImGui.Text($"m_maxContactImpulse: {matEntry.m_maxContactImpulse}");
                    ImGui.Text($"m_fractionOfClippedImpulseToApply: {matEntry.m_fractionOfClippedImpulseToApply}");
                    ImGui.Text($"m_massChangerCategory: {matEntry.m_massChangerCategory}");
                    ImGui.Text($"m_massChangerHeavyObjectFactor: {matEntry.m_massChangerHeavyObjectFactor}");
                    ImGui.Text($"m_softContactForceFactor: {matEntry.m_softContactForceFactor}");
                    ImGui.Text($"m_softContactDampFactor: {matEntry.m_softContactDampFactor}");
                    ImGui.Text($"m_softContactSeparationVelocity: {matEntry.m_softContactSeparationVelocity}");
                    ImGui.Text($"m_surfaceVelocity: {matEntry.m_surfaceVelocity}");
                    ImGui.Text($"m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance: {matEntry.m_disablingCollisionsBetweenCvxCvxDynamicObjectsDistance}");
                    ImGui.Text($"m_userData: {matEntry.m_userData}");
                }

                ImGui.Separator();
                ImGui.Text($"Motion Properties");
                ImGui.Separator();

                // Motion Properties
                foreach (var motionPropEntry in sysEntry.m_motionProperties)
                {
                    ImGui.Text($"m_isExclusive: {motionPropEntry.m_isExclusive}");
                    ImGui.Text($"m_flags: {motionPropEntry.m_flags}");
                    ImGui.Text($"m_gravityFactor: {motionPropEntry.m_gravityFactor}");
                    ImGui.Text($"m_timeFactor: {motionPropEntry.m_timeFactor}");
                    ImGui.Text($"m_maxLinearSpeed: {motionPropEntry.m_maxLinearSpeed}");
                    ImGui.Text($"m_maxAngularSpeed: {motionPropEntry.m_maxAngularSpeed}");
                    ImGui.Text($"m_linearDamping: {motionPropEntry.m_linearDamping}");
                    ImGui.Text($"m_angularDamping: {motionPropEntry.m_angularDamping}");
                    ImGui.Text($"m_solverStabilizationSpeedThreshold: {motionPropEntry.m_solverStabilizationSpeedThreshold}");
                    ImGui.Text($"m_solverStabilizationSpeedReduction: {motionPropEntry.m_solverStabilizationSpeedReduction}");

                    if (motionPropEntry.m_deactivationSettings != null)
                    {
                        ImGui.Text($"m_maxDistSqrd: {motionPropEntry.m_deactivationSettings.m_maxDistSqrd}");
                        ImGui.Text($"m_maxRotSqrd: {motionPropEntry.m_deactivationSettings.m_maxRotSqrd}");
                        ImGui.Text($"m_invBlockSize: {motionPropEntry.m_deactivationSettings.m_invBlockSize}");
                        ImGui.Text($"m_pathingUpperThreshold: {motionPropEntry.m_deactivationSettings.m_pathingUpperThreshold}");
                        ImGui.Text($"m_pathingLowerThreshold: {motionPropEntry.m_deactivationSettings.m_pathingLowerThreshold}");
                        ImGui.Text($"m_numDeactivationFrequencyPasses: {motionPropEntry.m_deactivationSettings.m_numDeactivationFrequencyPasses}");
                        ImGui.Text($"m_deactivationVelocityScaleSquare: {motionPropEntry.m_deactivationSettings.m_deactivationVelocityScaleSquare}");
                        ImGui.Text($"m_minimumPathingVelocityScaleSquare: {motionPropEntry.m_deactivationSettings.m_minimumPathingVelocityScaleSquare}");
                        ImGui.Text($"m_spikingVelocityScaleThresholdSquared: {motionPropEntry.m_deactivationSettings.m_spikingVelocityScaleThresholdSquared}");
                        ImGui.Text($"m_minimumSpikingVelocityScaleSquared: {motionPropEntry.m_deactivationSettings.m_minimumSpikingVelocityScaleSquared}");
                    }
                    if (motionPropEntry.m_fullCastSettings != null)
                    {
                        ImGui.Text($"m_minSeparation: {motionPropEntry.m_fullCastSettings.m_minSeparation}");
                        ImGui.Text($"m_minSeparam_minExtraSeparationtion: {motionPropEntry.m_fullCastSettings.m_minExtraSeparation}");
                        ImGui.Text($"m_toiSeparation: {motionPropEntry.m_fullCastSettings.m_toiSeparation}");
                        ImGui.Text($"m_toiExtraSeparation: {motionPropEntry.m_fullCastSettings.m_toiExtraSeparation}");
                        ImGui.Text($"m_toiAccuracy: {motionPropEntry.m_fullCastSettings.m_toiAccuracy}");
                        ImGui.Text($"m_relativeSafeDeltaTime: {motionPropEntry.m_fullCastSettings.m_relativeSafeDeltaTime}");
                        ImGui.Text($"m_absoluteSafeDeltaTime: {motionPropEntry.m_fullCastSettings.m_absoluteSafeDeltaTime}");
                        ImGui.Text($"m_keepTime: {motionPropEntry.m_fullCastSettings.m_keepTime}");
                        ImGui.Text($"m_keepDistance: {motionPropEntry.m_fullCastSettings.m_keepDistance}");
                        ImGui.Text($"m_maxIterations: {motionPropEntry.m_fullCastSettings.m_maxIterations}");
                    }
                }

                ImGui.Separator();
                ImGui.Text($"Body Info");
                ImGui.Separator();

                // Body Info
                foreach (var bodyInfo in sysEntry.m_bodyCinfos)
                {
                    ImGui.Text($"m_flags: {bodyInfo.m_flags}");
                    ImGui.Text($"m_collisionCntrl: {bodyInfo.m_collisionCntrl}");
                    ImGui.Text($"m_collisionFilterInfo: {bodyInfo.m_collisionFilterInfo}");
                    ImGui.Text($"m_materialId: {bodyInfo.m_materialId}");
                    ImGui.Text($"m_qualityId: {bodyInfo.m_qualityId}");
                    ImGui.Text($"m_name: {bodyInfo.m_name}");
                    ImGui.Text($"m_userData: {bodyInfo.m_userData}");
                    ImGui.Text($"m_motionType: {bodyInfo.m_motionType}");
                    ImGui.Text($"m_position: {bodyInfo.m_position}");
                    ImGui.Text($"m_orientation: {bodyInfo.m_orientation}");
                    ImGui.Text($"m_linearVelocity: {bodyInfo.m_linearVelocity}");
                    ImGui.Text($"m_angularVelocity: {bodyInfo.m_angularVelocity}");
                    ImGui.Text($"m_mass: {bodyInfo.m_mass}");

                    if (bodyInfo.m_massDistribution != null)
                    {
                        ImGui.Text($"m_centerOfMassAndVolume: {bodyInfo.m_massDistribution.m_massDistribution.m_centerOfMassAndVolume}");
                        ImGui.Text($"m_majorAxisSpace: {bodyInfo.m_massDistribution.m_massDistribution.m_majorAxisSpace}");
                        ImGui.Text($"m_inertiaTensor: {bodyInfo.m_massDistribution.m_massDistribution.m_inertiaTensor}");
                    }

                    if (bodyInfo.m_dragProperties != null)
                    {
                        foreach (var entry in bodyInfo.m_dragProperties.m_dragProperties.m_centerAndOffset)
                        {
                            ImGui.Text($"m_centerAndOffset: {entry}");
                        }
                        foreach (var entry in bodyInfo.m_dragProperties.m_dragProperties.m_angularEffectsAndArea)
                        {
                            ImGui.Text($"m_angularEffectsAndArea: {entry}");
                        }
                        foreach (var entry in bodyInfo.m_dragProperties.m_dragProperties.m_armUVs)
                        {
                            ImGui.Text($"m_armUVs: {entry}");
                        }
                    }

                    ImGui.Text($"m_motionPropertiesId: {bodyInfo.m_motionPropertiesId}");

                    ImGui.Text($"m_serialAndIndex: {bodyInfo.m_desiredBodyId.m_serialAndIndex}");

                    ImGui.Text($"m_motionId: {bodyInfo.m_motionId}");
                    ImGui.Text($"m_collisionLookAheadDistance: {bodyInfo.m_collisionLookAheadDistance}");

                    ImGui.Text($"m_activationPriority: {bodyInfo.m_activationPriority}");
                }

                ImGui.Separator();
                ImGui.Text($"Constraint Info");
                ImGui.Separator();

                // Constraint Info
                foreach (var constraintInfo in sysEntry.m_constraintCinfos)
                {
                    ImGui.Text($"m_name: {constraintInfo.m_name}");
                    ImGui.Text($"m_constraintData: {constraintInfo.m_constraintData}");
                    ImGui.Text($"m_flags: {constraintInfo.m_flags}");
                    ImGui.Text($"m_bodyA.m_serialAndIndex: {constraintInfo.m_bodyA.m_serialAndIndex}");
                    ImGui.Text($"m_bodyB.m_serialAndIndex: {constraintInfo.m_bodyB.m_serialAndIndex}");
                    ImGui.Text($"m_desiredConstraintId: {constraintInfo.m_desiredConstraintId}");
                    ImGui.Text($"m_constraintGroupId: {constraintInfo.m_constraintGroupId}");
                }

                ImGui.Text($"m_name: {sysEntry.m_name}");
                ImGui.Text($"m_microStepMultiplier: {sysEntry.m_microStepMultiplier}");
            }

            if (physicsSceneData.m_worldCinfo != null)
            {
                ImGui.Separator();
                ImGui.Text($"World Info");
                ImGui.Separator();

                var worldInfo = physicsSceneData.m_worldCinfo.m_info;

                ImGui.Text($"m_bodyBufferCapacity: {worldInfo.m_bodyBufferCapacity}");
                ImGui.Text($"m_motionBufferCapacity: {worldInfo.m_motionBufferCapacity}");
                ImGui.Text($"m_constraintBufferCapacity: {worldInfo.m_constraintBufferCapacity}");
                ImGui.Text($"m_constraintGroupBufferCapacity: {worldInfo.m_constraintGroupBufferCapacity}");
                ImGui.Text($"m_useBodyBacklinkBuffer: {worldInfo.m_useBodyBacklinkBuffer}");

                var hknpMaterialLib = worldInfo.m_materialLibrary;
                var hknpMotionPropertiesLib = worldInfo.m_motionPropertiesLibrary;
                var hknpBodyQualityLib = worldInfo.m_qualityLibrary;

                ImGui.Text($"m_simulationType: {worldInfo.m_simulationType}");
                ImGui.Text($"m_numSplitterCells: {worldInfo.m_numSplitterCells}");
                ImGui.Text($"m_gravity: {worldInfo.m_gravity}");
                ImGui.Text($"m_airDensity: {worldInfo.m_airDensity}");
                ImGui.Text($"m_enableContactCaching: {worldInfo.m_enableContactCaching}");
                ImGui.Text($"m_mergeEventsBeforeDispatch: {worldInfo.m_mergeEventsBeforeDispatch}");
                ImGui.Text($"m_broadPhaseType: {worldInfo.m_broadPhaseType}");

                var broadPhaseAabb = worldInfo.m_broadPhaseAabb;
                var broadPhaseConfig = worldInfo.m_broadPhaseConfig;
                var collisionFilter = worldInfo.m_collisionFilter;
                var broadPhaseAshapeTagCodecabb = worldInfo.m_shapeTagCodec;

                ImGui.Text($"m_collisionTolerance: {worldInfo.m_collisionTolerance}");
                ImGui.Text($"m_relativeCollisionAccuracy: {worldInfo.m_relativeCollisionAccuracy}");
                ImGui.Text($"m_aabbMargin: {worldInfo.m_aabbMargin}");
                ImGui.Text($"m_enableWeldingForDefaultObjects: {worldInfo.m_enableWeldingForDefaultObjects}");
                ImGui.Text($"m_enableWeldingForCriticalObjects: {worldInfo.m_enableWeldingForCriticalObjects}");

                var weldingConfig = worldInfo.m_weldingConfig;
                var lodManagerCinfo = worldInfo.m_lodManagerCinfo;

                ImGui.Text($"m_enableSdfEdgeCollisions: {worldInfo.m_enableSdfEdgeCollisions}");
                ImGui.Text($"m_enableCollideWorkStealing: {worldInfo.m_enableCollideWorkStealing}");
                ImGui.Text($"m_particlesLandscapeQuadCacheSize: {worldInfo.m_particlesLandscapeQuadCacheSize}");
                ImGui.Text($"m_solverTau: {worldInfo.m_solverTau}");
                ImGui.Text($"m_solverDamp: {worldInfo.m_solverDamp}");
                ImGui.Text($"m_solverIterations: {worldInfo.m_solverIterations}");
                ImGui.Text($"m_solverMicrosteps: {worldInfo.m_solverMicrosteps}");
                ImGui.Text($"m_enableDeactivation: {worldInfo.m_enableDeactivation}");
                ImGui.Text($"m_enablePenetrationRecovery: {worldInfo.m_enablePenetrationRecovery}");
                ImGui.Text($"m_maxApproachSpeedForHighQualitySolver: {worldInfo.m_maxApproachSpeedForHighQualitySolver}");

                var bodyIntegrator = worldInfo.m_bodyIntegrator;

                ImGui.Text($"m_adjustSolverSettingsBasedOnTimestep: {worldInfo.m_adjustSolverSettingsBasedOnTimestep}");
                ImGui.Text($"m_expectedDeltaTime: {worldInfo.m_expectedDeltaTime}");
                ImGui.Text($"m_minSolverIterations: {worldInfo.m_minSolverIterations}");
                ImGui.Text($"m_maxSolverIterations: {worldInfo.m_maxSolverIterations}");
            }
        }
    }
}
