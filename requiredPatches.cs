using System;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

namespace factionMissions.requiredPatches {
	// Needed for Mission Effects that use TIMissionTarget_Nation (ie, you select a nation for the mission)
	// Needed for applying custom Mission Conditions for Any NonVanila Mission
	[HarmonyPatch(typeof(TIMissionTarget_Nation), nameof(TIMissionTarget_Nation.ValidateSingleTarget))]
	public static class validTargetNationHeaderPatch {

		[HarmonyPostfix]
		public static void validateSingleTargetPatch(TIMissionTarget_Nation __instance, List<string> __result, ref TIMissionTemplate mission, ref TICouncilorState councilor, ref TIGameState target) {
			if (Main.masterMissionList != null && Main.masterMissionList.Contains(mission.dataName)) {
				if (Main.settings.destroyMissions && Main.destroyMissionList != null && Main.destroyMissionList.Contains(mission.dataName)) {
					if (mission.dataName.Equals("DestroyRaiseMilitia")) {
						TIMissionCondition condition = new factionMissions.MissionConditions.TIMissionCondition_atWar();
						__result.Add(condition.CanTarget(councilor, target));
						
						TIMissionCondition condition2 = new factionMissions.MissionConditions.TIMissionCondition_hasOwnCP();
						if (Main.settings.friendlyFPFlag) {
							condition2 = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyCPs();
						}
						__result.Add(condition2.CanTarget(councilor, target));
					}
				}
				else if (Main.settings.resistMissions && Main.resistMissionList != null && Main.resistMissionList.Contains(mission.dataName)) {
					if (mission.dataName.Equals("ResistPeacekeepers")) {
						factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInNation condition = new factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInNation();
						__result.Add(condition.CanTarget(councilor, target, 4));

						TIMissionCondition condition2 = new factionMissions.MissionConditions.TIMissionCondition_hasOwnCP();
						if (Main.settings.friendlyFPFlag) {
							condition2 = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyCPs();
						}
						__result.Add(condition2.CanTarget(councilor, target));
					}
				}
				else if (Main.settings.escapeMissions && Main.escapeMissionList != null && Main.escapeMissionList.Contains(mission.dataName)) {
					if (mission.dataName.Equals("EscapeFundSpaceProgram")) {
						TIMissionCondition condition = new factionMissions.MissionConditions.TIMissionCondition_noSpaceProgram();
						__result.Add(condition.CanTarget(councilor, target));
						
						if (Main.settings.friendlyFPFlag) {
							condition = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyCPs();
						}
						else {
							condition = new factionMissions.MissionConditions.TIMissionCondition_hasOwnCP();
						}
						
						__result.Add(condition.CanTarget(councilor, target));
					}
					else if(mission.dataName.Equals("EscapeExpandSpaceAgency")) {
						TIMissionCondition condition = new factionMissions.MissionConditions.TIMissionCondition_hasSpaceProgram();
						__result.Add(condition.CanTarget(councilor, target));
						
						if (Main.settings.friendlyFPFlag) {
							condition = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyCPs();
						}
						else {
							condition = new factionMissions.MissionConditions.TIMissionCondition_hasOwnCP();
						}
						__result.Add(condition.CanTarget(councilor, target));
					}
				}
				else if (Main.settings.exploitMissions && Main.exploitMissionList != null && Main.exploitMissionList.Contains(mission.dataName)) {
					if (mission.dataName.Equals("ExploitIgnoreEcologicalProtections")) {
						TIMissionCondition condition = new factionMissions.MissionConditions.TIMissionCondition_hasOwnCP();
						if (Main.settings.friendlyFPFlag) {
							condition = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyCPs();
						}
						__result.Add(condition.CanTarget(councilor, target));
					}
				}
				else if (Main.settings.studyMissions && Main.studyMissionList != null && Main.studyMissionList.Contains(mission.dataName)) {
					if (mission.dataName.Equals("StudyEducatePopulace")) {
						
					}
					else if (mission.dataName.Equals("StudyShareResearch")) {
						TIMissionCondition condition = new factionMissions.MissionConditions.TIMissionCondition_hasOwnCP();
						__result.Add(condition.CanTarget(councilor, target));

						condition = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyNotSelfCP();
						__result.Add(condition.CanTarget(councilor, target));

					}
					else if (mission.dataName.Equals("StudyTechSummit")) {
						TIMissionCondition condition = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyCPs();
						__result.Add(condition.CanTarget(councilor, target));
					}
				}
				else if (Main.settings.submitMissions && Main.submitMissionList != null && Main.submitMissionList.Contains(mission.dataName)) {

				}
				else if (Main.settings.serveMissions && Main.serveMissionList != null && Main.serveMissionList.Contains(mission.dataName)) {

				}
			}
		}
	}

	// Needed for Mission Effects that use TIMissionTarget_Region (ie, you select a region for the mission)
	// Needed for applying custom Mission Conditions for Any NonVanila Mission
	[HarmonyPatch(typeof(TIMissionTarget_Region), nameof(TIMissionTarget_Region.ValidateSingleTarget))]
	public static class validTargetRegionHeaderPatch {

		[HarmonyPostfix]
		public static void validateSingleTargetPatch(TIMissionTarget_Nation __instance, List<string> __result, ref TIMissionTemplate mission, ref TICouncilorState councilor, ref TIGameState target) {
			if (Main.masterMissionList != null && Main.masterMissionList.Contains(mission.dataName)) {
				if (Main.settings.destroyMissions && Main.destroyMissionList != null && Main.destroyMissionList.Contains(mission.dataName)) {
					// Humanity First does not yet have any Region-targetting missions
				}
				else if (Main.settings.resistMissions && Main.resistMissionList != null && Main.resistMissionList.Contains(mission.dataName)) {
					if (mission.dataName.Equals("ResistCellNetwork")) {
						factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkRegionSmallerThan condition = new factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkRegionSmallerThan();
						__result.Add(condition.CanTarget(councilor, target, 60));
					}
					else if (mission.dataName.Equals("ResistHumanitarianMission")) {
						factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInNation condition = new factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInNation();
						__result.Add(condition.CanTarget(councilor, target, 4));

						factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInRegion condition2 = new factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInRegion();
						__result.Add(condition2.CanTarget(councilor, target, 4));

						TIMissionCondition condition3 = new factionMissions.MissionConditions.TIMissionCondition_hasOwnCP();
						if (Main.settings.friendlyFPFlag) {
							condition3 = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyCPs();
						}
						__result.Add(condition3.CanTarget(councilor, target));
					}
					else if (mission.dataName.Equals("ResistSmuggleArms")) {
						factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInNation condition = new factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInNation();
						__result.Add(condition.CanTarget(councilor, target, 4));

						factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInRegion condition2 = new factionMissions.MissionConditions.TIMissionCondition_hasResistCellNetworkInRegion();
						__result.Add(condition2.CanTarget(councilor, target, 4));

						TIMissionCondition condition3 = new factionMissions.MissionConditions.TIMissionCondition_hasOwnCP();
						if (Main.settings.friendlyFPFlag) {
							condition3 = new factionMissions.MissionConditions.TIMissionCondition_hasFriendlyCPs();
						}
						__result.Add(condition3.CanTarget(councilor, target));
					}
				}
				else if (Main.settings.escapeMissions && Main.escapeMissionList != null && Main.escapeMissionList.Contains(mission.dataName)) {
					// Exodus not YET targetting Regions
				}
				else if (Main.settings.exploitMissions && Main.exploitMissionList != null && Main.exploitMissionList.Contains(mission.dataName)) {
					// Initiative not YET targetting Regions
				}
				else if (Main.settings.studyMissions && Main.studyMissionList != null && Main.studyMissionList.Contains(mission.dataName)) {
					// Academy Missions aren't targetting Regions
				}
				else if (Main.settings.submitMissions && Main.submitMissionList != null && Main.submitMissionList.Contains(mission.dataName)) {

				}
				else if (Main.settings.serveMissions && Main.serveMissionList != null && Main.serveMissionList.Contains(mission.dataName)) {

				}
			}
		}
	}

	// Needed for APPLYING NonVanilia Mission Modifiers
	[HarmonyPatch(typeof(TIMissionResolution_Contested), "GetAllModifiers")]
	public static class getAllModifiersHeaderPatch {
		[HarmonyPostfix]
		static void GetAllModifiersPatch(TIMissionResolution_Contested __instance, List<TIMissionModifier> __result, ref TIMissionTemplate mission, ref bool attacking, ref TICouncilorState councilor, ref TIGameState target, ref float resourcesSpent) {
			if (Main.masterMissionList != null && Main.masterMissionList.Contains(mission.dataName)) {
				TIMissionModifier missionModifier;
				Traverse traverseObj = Traverse.Create(councilor);
				//FileLog.Log("I entered the AllModifiersPatch for councilor"+councilor.displayName+" of faction ["+councilor.faction.displayName+"]");
				//FileLog.Log("Mission Query: "+mission.dataName+"["+Main.settings.escapeMissions+"]["+(Main.escapeMissionList != null)+"]["+Main.escapeMissionList.Contains(mission.dataName)+"]");
				if (attacking) {
					//FileLog.Log("I have entered the attack branch");
					if (Main.settings.destroyMissions && Main.destroyMissionList != null && Main.destroyMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Destroy Missions");
						if (mission.dataName.Equals("DestroyRaiseMilitia")) {
							missionModifier = new TIMissionModifier_MassMedia();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasReligionCP();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSector();
							__result.Add(missionModifier);

							missionModifier = new TIMissionModifier_RegionalAuthorities();
							__result.Add(missionModifier);

							missionModifier = new TIMissionModifier_Warlords();
							__result.Add(missionModifier);
						}
					}
					else if (Main.settings.resistMissions && Main.resistMissionList != null && (Main.resistMissionList.Contains(mission.dataName))) {
						//FileLog.Log("Inspect Resist Missions");
						if (mission.dataName.Equals("ResistPeacekeepers")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_armyRegionDefences();
							__result.Add(missionModifier);
						}
						else if (mission.dataName.Equals("ResistCellNetwork")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_ResistCellNetworkMinor();
							__result.Add(missionModifier);
						}
						else if (mission.dataName.Equals("ResistHumanitarianMission")) {
							missionModifier = new TIMissionModifier_RegionalAuthorities();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasAgriculturalSector();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasExtractiveSector();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasNationalIndustries();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasTradeUnions();
							__result.Add(missionModifier);
							
							missionModifier = new TIMissionModifier_Bureaucracy();
							__result.Add(missionModifier);

						}
						else if (mission.dataName.Equals("ResistSmuggleArms")) {
							missionModifier = new TIMissionModifier_RegionalAuthorities();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasExecutiveCP();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSector();
							__result.Add(missionModifier);
							
							missionModifier = new TIMissionModifier_Warlords();
							__result.Add(missionModifier);
							
							missionModifier = new TIMissionModifier_SecurityApparatus();
							__result.Add(missionModifier);
						}

						if (!mission.dataName.Equals("ResistCellNetwork")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_ResistCellNetwork();
							__result.Add(missionModifier);
						}
					}
					else if (Main.settings.escapeMissions && Main.escapeMissionList != null && Main.escapeMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Escape Missions");
						if (mission.dataName.Equals("EscapeFundSpaceProgram")) {
							//FileLog.Log("I was... maybe called?");
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_CustomEcoModifier();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSector();
							__result.Add(missionModifier);	
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSector();
							__result.Add(missionModifier);	
						}
						else if(mission.dataName.Equals("EscapeExpandSpaceAgency")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_CustomEcoModifier();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSector();
							__result.Add(missionModifier);	
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSector();
							__result.Add(missionModifier);	
							
						}
					}
					else if (Main.settings.exploitMissions && Main.exploitMissionList != null && Main.exploitMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Exploit Missions");
						if (mission.dataName.Equals("ExploitIgnoreEcologicalProtections")) {
						
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasAristocracy();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasCorporations();
							__result.Add(missionModifier);	
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasFinancialSector();
							__result.Add(missionModifier);
						}
					}
					else if (Main.settings.studyMissions && Main.studyMissionList != null && Main.studyMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Study Missions");
						if (mission.dataName.Equals("StudyEducatePopulace")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSector();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSector();
							__result.Add(missionModifier);	
						
						}
						else if (mission.dataName.Equals("StudyShareResearch")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSector();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSector();
							__result.Add(missionModifier);	

						}
						else if (mission.dataName.Equals("StudyTechSummit")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSector();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSector();
							__result.Add(missionModifier);	
							
						}
					}
					else if (Main.settings.submitMissions && Main.submitMissionList != null && Main.submitMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Submit Missions");
					}
					else if (Main.settings.serveMissions && Main.serveMissionList != null && Main.serveMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Serve Missions");
					}
				}
				else {
					//FileLog.Log("I have entered the defence branch");
					if (Main.settings.destroyMissions && Main.destroyMissionList != null && Main.destroyMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Destroy Missions");
						if (mission.dataName.Equals("DestroyRaiseMilitia")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_NationThreatened();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_InvertedNationPopulation();
							__result.Add(missionModifier);
						
						}
					}
					else if (Main.settings.resistMissions && Main.resistMissionList != null && Main.resistMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Resist Missions");
						if (mission.dataName.Equals("ResistPeacekeepers")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_RegionPopulation();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_ModerateMission();
							__result.Add(missionModifier);
							
						}
						else if (mission.dataName.Equals("ResistCellNetwork")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_RegionPopulation();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_MediumMission();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_armyRegionDefencesDEF();
							__result.Add(missionModifier);

						}
						else if (mission.dataName.Equals("ResistHumanitarianMission")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_RegionPopulation();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_ModerateMission();
							__result.Add(missionModifier);

						}
						else if (mission.dataName.Equals("ResistSmuggleArms")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_RegionPopulation();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_ModerateMission();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSectorDEF();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasSecurityApparatusDEF();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasRegionalAuthoritiesDEF();
							__result.Add(missionModifier);

							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_armyRegionDefencesDEF();
							__result.Add(missionModifier);
						}
					}
					else if (Main.settings.escapeMissions && Main.escapeMissionList != null && Main.escapeMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Escape Missions");
						if (mission.dataName.Equals("EscapeFundSpaceProgram")) {
							//FileLog.Log("We be doing a little checking... ");
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_VeryDifficultMission();
							__result.Add(missionModifier);
						}
						else if(mission.dataName.Equals("EscapeExpandSpaceAgency")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_DifficultMission();
							__result.Add(missionModifier);
						}
					}
					else if (Main.settings.exploitMissions && Main.exploitMissionList != null && Main.exploitMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Exploit Missions");
						if (mission.dataName.Equals("ExploitIgnoreEcologicalProtections")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_MediumMission();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSectorDEF();
							__result.Add(missionModifier);
						}
					}
					else if (Main.settings.studyMissions && Main.studyMissionList != null && Main.studyMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Study Missions");
						if (mission.dataName.Equals("StudyEducatePopulace")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_MediumMission();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSectorDEF();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSectorDEF();
							__result.Add(missionModifier);						
						}
						else if (mission.dataName.Equals("StudyShareResearch")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_MediumMission();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSectorDEF();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSectorDEF();
							__result.Add(missionModifier);	
						}
						else if (mission.dataName.Equals("StudyTechSummit")) {
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_MediumMission();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasDefenceSectorDEF();
							__result.Add(missionModifier);
							
							missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_hasKnowledgeSectorDEF();
							__result.Add(missionModifier);	
						}
					}
					else if (Main.settings.submitMissions && Main.submitMissionList != null && Main.submitMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Submit Missions");
					}
					else if (Main.settings.serveMissions && Main.serveMissionList != null && Main.serveMissionList.Contains(mission.dataName)) {
						//FileLog.Log("Inspect Serve Missions");
					}
				}			
			}
			else if (Main.settings.cellnetworksAllowed && Main.settings.resistMissions && Main.resistMissionList.Contains("ResistCellNetwork") && councilor.learnedMissionsTemplateNames.Contains("ResistCellNetwork")) {
				if (attacking) {
					TIMissionModifier missionModifier = new factionMissions.MissionModifiers.TIMissionModifier_ResistCellNetwork();
					__result.Add(missionModifier);
				}
			}
		}
	}


	// // Needed for DISPLAYING any NonVanila MissionModifiers? FileLog doesn't seem to think so; Maybe not
	// [HarmonyPatch(typeof(TIMissionTemplate), nameof(TIMissionTemplate.multiLineDescriptionWithModifiers), MethodType.Getter)]
	// public static class missionModifierHeaderPatch {
	// 	[HarmonyPostfix]
	// 	public static void missionModifierPatch (TIMissionTemplate __instance, string __result) {
	// 		if (Main.masterMissionList != null && Main.masterMissionList.Contains(__instance.dataName)) {
	// 			FileLog.Log("I have entered the missionmodifierPatch");
	// 			StringBuilder builder = new StringBuilder(__result);
	// 			TIMissionModifier missionModifier;
	// 			if (Main.settings.destroyMissions && Main.destroyMissionList != null && Main.destroyMissionList.Contains(__instance.dataName)) {
					
	// 			}
	// 			else if (Main.settings.resistMissions && Main.resistMissionList != null && Main.resistMissionList.Contains(__instance.dataName)) {
	// 				if (__instance.dataName.Equals("ResistPeacekeepers")) {
	// 					missionModifier = new TIMissionModifier_EasyMission();
	// 					builder.AppendLine(missionModifier.displayName);
	// 				}
	// 			}
	// 			else if (Main.settings.escapeMissions && Main.escapeMissionList != null && Main.escapeMissionList.Contains(__instance.dataName)) {
	// 				if (__instance.dataName.Equals("EscapeFundSpaceProgram")) {
	// 					FileLog.Log("Ermmmm.. maybe?");
	// 					missionModifier = new TIMissionModifier_VeryDifficultMission();
	// 					builder.AppendLine(missionModifier.displayName);
	// 				}
	// 				else if(__instance.dataName.Equals("EscapeExpandSpaceAgency")) {
	// 					missionModifier = new TIMissionModifier_DifficultMission();
	// 					builder.AppendLine(missionModifier.displayName);						
	// 				}
	// 			}
	// 			else if (Main.settings.exploitMissions && Main.exploitMissionList != null && Main.exploitMissionList.Contains(__instance.dataName)) {

	// 			}
	// 			else if (Main.settings.studyMissions && Main.studyMissionList != null && Main.studyMissionList.Contains(__instance.dataName)) {

	// 			}
	// 			else if (Main.settings.submitMissions && Main.submitMissionList != null && Main.submitMissionList.Contains(__instance.dataName)) {

	// 			}
	// 			else if (Main.settings.serveMissions && Main.serveMissionList != null && Main.serveMissionList.Contains(__instance.dataName)) {

	// 			}
	// 			__result = builder.ToString();
	// 		}
	// 	}
	// }


	// NEEDED for calling/applying the effects of Any NonVanila Mission
	[HarmonyPatch(typeof(TIMissionState), nameof(TIMissionState.ResolveMission))]
	public static class ResolveMissionEffectPatch {
		[HarmonyPrefix]
		// This must be done to get the neccessary ApplyEffect methods to run
		public static bool ResolveMissionPatch(TIMissionState __instance, ref bool forceAbort, ref MissionResult __result) {
			bool matched = false;
			if (Main.masterMissionList != null && Main.masterMissionList.Contains(__instance.GetMyTemplate().dataName)) {
				matched = true;
				String missionDataName = __instance.missionTemplate.dataName;

				// THE FOLLOWING IS A COPY-PASTED CODE SEGMENT FROM TIMissionState.ResolveMIssion
				if (!GameStateManager.MissionPhase().currentlyResolvingMissions.Contains(__instance))
				{
					GameStateManager.MissionPhase().currentlyResolvingMissions.Add(__instance);
				}
				MissionResult missionResult = new MissionResult
				{
					councilor = __instance.councilor,
					missionTemplate = __instance.missionTemplate,
					noiseModifier = 0f
				};
				TIFactionState faction = __instance.councilor.faction;
				if (__instance.councilor.status != CouncilorStatus.Active || __instance.councilor.activeMission == null || faction == null)
				{
					missionResult.missionResult = TIMissionOutcome.None;
					__instance.missionOutcome = missionResult.missionResult;
					__instance.councilor.ClearActiveMission();
					__instance.MissionResolved();
					__result = missionResult;
					return false;
				}
				if (!forceAbort && __instance.councilor.ref_faction.player.isAI && AIEvaluators.AI_ShouldAbortBadMission(__instance))
				{
					forceAbort = true;
				}
				TICouncilorState ref_councilor = __instance.target.ref_councilor;
				if (!forceAbort)
				{
					__instance.councilor.CheckAndChaseMissionTarget();
				}
				TIMissionResolution resolutionMethod = __instance.missionTemplate.resolutionMethod;
				if ((forceAbort || !__instance.councilor.active || __instance.target.deleted || !__instance.missionTemplate.target.ValidTarget(__instance.missionTemplate.target.ValidateSingleTarget(__instance.missionTemplate, __instance.councilor, __instance.target))) && !__instance.missionTemplate.debugForced)
				{
					missionResult.missionResult = TIMissionOutcome.Aborted;
					if (__instance.missionTemplate.hasCost)
					{
						faction.AddToCurrentResource(__instance.resources, __instance.missionTemplate.cost.resourceType, false);
					}
				}
				else
				{
					float successChance = resolutionMethod.GetSuccessChance(__instance.missionTemplate, __instance.councilor, __instance.target, __instance.resources, false);
					missionResult.successChance = successChance;
					TIMissionResult timissionResult = resolutionMethod.GetMissionOutcome(__instance.missionTemplate, __instance.councilor, __instance.target, __instance.resources);
					if (faction.isActivePlayer && !resolutionMethod.automaticSuccess)
					{
						TIMissionOutcome outcome = timissionResult.outcome;
						if (outcome > TIMissionOutcome.Failure)
						{
							if (outcome - TIMissionOutcome.Success <= 1)
							{
								Mood.GoodNews();
							}
						}
						else
						{
							Mood.BadNews();
						}
					}
					if (faction != null && faction.isActivePlayer && successChance >= 0.99f && (timissionResult.outcome == TIMissionOutcome.Failure || timissionResult.outcome == TIMissionOutcome.CriticalFailure))
					{
						faction.UnlockAchievement("failEasyMission");
					}
					if (TemplateManager.global.debug_noMissionFail)
					{
						timissionResult = new TIMissionResult
						{
							outcome = TIMissionOutcome.Success,
							roll = 0f
						};
					}
					else if (TemplateManager.global.debug_alwaysCritFail)
					{
						timissionResult = new TIMissionResult
						{
							outcome = TIMissionOutcome.CriticalFailure,
							roll = 1f
						};
					}
					missionResult.missionResult = timissionResult.outcome;
					if (resolutionMethod.automaticSuccess)
					{
						missionResult.roll = 0f;
					}
					else
					{
						missionResult.roll = timissionResult.roll;
					}
					missionResult.target = __instance.target;
				}
				List<TIGameState> oldControlPoints = new List<TIGameState>();
				List<TIGameState> newControlPoints = new List<TIGameState>();
				TIFactionState ref_faction = __instance.target.ref_faction;
				if (missionResult.Attempted)
				{
					MissionMovementRule movementRule = __instance.missionTemplate.movementRule;
					if (movementRule != MissionMovementRule.MoveUponAttempt)
					{
						if (movementRule == MissionMovementRule.MoveWhenSuccessful)
						{
							string text;
							if (missionResult.Success && __instance.councilor.ValidDestination(__instance.targetLocation, out text))
							{
								__instance.councilor.ChangeLocation(__instance.targetLocation);
							}
						}
					}
					else
					{
						__instance.councilor.ChangeLocation(__instance.targetLocation);
					}
					TINationState ref_nation = __instance.target.ref_nation;
					List<TIFactionState> ref_factions = __instance.target.ref_factions;
					if (ref_nation != null)
					{
						oldControlPoints = ref_nation.controlPointOwnersByPoint;
					}
					if (missionResult.missionResult == TIMissionOutcome.CriticalFailure)
					{
						TITraitTemplate.ProcessLoyaltyChangeFromTraits(faction, SpecialTraitRule.LoyaltyLossOnFactionCriticalFailure, 1);
						TITraitTemplate.ProcessLoyaltyChangeFromTraits(__instance.councilor, SpecialTraitRule.LoyaltyLossOnPersonalCritFailure, 1);
					}
					if (__instance.missionTemplate.targetEffects != null)
					{
						// int cout = 0;
						foreach (TIMissionEffect timissionEffect in __instance.missionTemplate.targetEffects)
						{
							// FileLog.Log("Doing effect: "+__instance.missionTemplate.targetEffects[cout]);
							string text2 = timissionEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							// FileLog.Log("Recieved: ["+text2+"]");
							if (string.IsNullOrEmpty(missionResult.valueChange) && text2 != string.Empty)
							{
								missionResult.valueChange = text2;
							}
							// cout += 1;
						}

						// START OF MOD-SPECIFIC CODE

						string ret = "";
						TIMissionEffect miEffect;
						if (1 == 1) {
							// DestroyMissions
						}
						if (1 == 1) {
							// ResistMissions
							if (missionDataName.Equals("ResistPeacekeepers")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_ResistPeacekeepers();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
							else if (missionDataName.Equals("ResistCellNetwork")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_ResistCellNetwork();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
							else if (missionDataName.Equals("ResistHumanitarianMission")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_ResistHumanitarianMission();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
							else if (missionDataName.Equals("ResistSmuggleArms")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_ResistSmuggleArms();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
						}
						if (1 == 1) {
							if (missionDataName.Equals("EscapeFundSpaceProgram")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_EscapeFundSpaceProgram();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
							else if (missionDataName.Equals("EscapeExpandSpaceAgency")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_EscapeExpandSpaceAgency();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
							
							// Escape Missions
						}
						if (1 == 1) {
							// Exploit Missions
							if (missionDataName.Equals("ExploitIgnoreEcologicalProtections")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_ExploitIgnoreEcologicalProtections();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
							
						}
						if (1 == 1) {
							// Study Missions
							if (missionDataName.Equals("StudyEducatePopulace")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_StudyEducatePopulace();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
							else if (missionDataName.Equals("StudyShareResearch")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_StudyShareResearch();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
							else if (missionDataName.Equals("StudyTechSummit")) {
								miEffect = new factionMissions.MissionEffects.TIMissionEffect_StudyTechSummit();
								ret = miEffect.ApplyEffect(__instance, __instance.target, missionResult.missionResult);
							}
						}
						if (1 == 1) {
							// Submit Missions
						}
						if (1 == 1) {
							// Serve Missions
						}

						if (string.IsNullOrEmpty(missionResult.valueChange) && ret != string.Empty)
						{
							missionResult.valueChange = ret;
						}

						// END OF MOD-SPECIFIC CODE
					}
					if (__instance.missionTemplate.councilorEffects != null)
					{
						foreach (TIMissionEffect timissionEffect2 in __instance.missionTemplate.councilorEffects)
						{
							string text3 = timissionEffect2.ApplyEffect(__instance, __instance.councilor, missionResult.missionResult);
							if (string.IsNullOrEmpty(missionResult.valueChange) && text3 != string.Empty)
							{
								missionResult.valueChange = text3;
							}
						}

						// START OF MOD-SPECIFIC CODE

						if (1 == 1) {
							// DestroyMissions
						}
						if (1 == 1) {
							// ResistMissions
							if (missionDataName.Equals("ResistPeacekeepers")) {
								
							}
						}
						if (1 == 1) {
							// Escape Missions
						}
						if (1 == 1) {
							// Exploit Missions
						}
						if (1 == 1) {
							// Study Missions
						}
						if (1 == 1) {
							// Submit Missions
						}
						if (1 == 1) {
							// Serve Missions
						}

						// END OF MOD-SPECIFIC CODE
					}
					if (ref_nation != null)
					{
						newControlPoints = __instance.target.ref_nation.controlPointOwnersByPoint;
					}
					int num;
					if (__instance.missionTemplate.specialPost && __instance.target.isRegionXenoformingState)
					{
						num = (missionResult.Success ? 2 : 1);
					}
					else
					{
						num = (missionResult.Success ? __instance.missionTemplate.XPonSuccess : (__instance.missionTemplate.XPonSuccess / 2));
					}
					float phasesPerMonth = TIMissionPhaseState.phasesPerMonth;
					if (phasesPerMonth == 1f)
					{
						num *= 2;
					}
					else if (phasesPerMonth > 1f && phasesPerMonth < 2f)
					{
						num = (int)Math.Ceiling((double)((float)num * 4f / 3f));
					}
					__instance.councilor.ChangeXP(num);
					foreach (TIFactionState tifactionState in ref_factions)
					{
						if (tifactionState != faction && missionResult.Attempted)
						{
							if (__instance.missionTemplate.specialPost && __instance.target.isRegionXenoformingState)
							{
								tifactionState.GainFactionHate(faction, TemplateManager.global.factionHateForBurnXenoforming, false);
							}
							else if (faction.IsAlienFaction && !tifactionState.CanDetectAlienMission(__instance))
							{
								tifactionState.GainFactionHate(GameStateManager.AlienProxy(), __instance.missionTemplate.hate[(int)missionResult.missionResult], false);
							}
							else
							{
								tifactionState.GainFactionHate(faction, __instance.missionTemplate.hate[(int)missionResult.missionResult], false);
							}
						}
					}
				}
				TINotificationQueueState.LogMissionOutcome(__instance, missionResult, ref_faction, newControlPoints, oldControlPoints, false);
				if (__instance.councilor.agentForFaction != null)
				{
					TINotificationQueueState.LogMissionOutcome(__instance, missionResult, ref_faction, newControlPoints, oldControlPoints, true);
				}
				faction.CheckForObjectivesCompleteViaMission(__instance, missionResult);
				if (missionResult.Attempted)
				{
					if (__instance.missionTemplate.targetEffects != null)
					{
						foreach (TIMissionEffect timissionEffect3 in from x in __instance.missionTemplate.targetEffects
						where x.HasDelayedEffect()
						select x)
						{
							timissionEffect3.ApplyDelayedEffect(__instance, __instance.target, missionResult.missionResult, "");
						}
						// START OF MOD-SPECIFIC CODE

						if (1 == 1) {
							// DestroyMissions
						}
						if (1 == 1) {
							// ResistMissions
							if (missionDataName.Equals("ResistPeacekeepers")) {

							}
						}
						if (1 == 1) {
							// Escape Missions
						}
						if (1 == 1) {
							// Exploit Missions
						}
						if (1 == 1) {
							// Study Missions
						}
						if (1 == 1) {
							// Submit Missions
						}
						if (1 == 1) {
							// Serve Missions
						}


						// END OF MOD-SPECIFIC CODE
					}
					if (__instance.missionTemplate.councilorEffects != null)
					{
						foreach (TIMissionEffect timissionEffect4 in from x in __instance.missionTemplate.councilorEffects
						where x.HasDelayedEffect()
						select x)
						{
							timissionEffect4.ApplyDelayedEffect(__instance, __instance.councilor, missionResult.missionResult, "");
						}
						// START OF MOD-SPECIFIC CODE

						if (1 == 1) {
							// DestroyMissions
						}
						if (1 == 1) {
							// ResistMissions
							if (missionDataName.Equals("ResistPeacekeepers")) {

							}
						}
						if (1 == 1) {
							// Escape Missions
						}
						if (1 == 1) {
							// Exploit Missions
						}
						if (1 == 1) {
							// Study Missions
						}
						if (1 == 1) {
							// Submit Missions
						}
						if (1 == 1) {
							// Serve Missions
						}


						// END OF MOD-SPECIFIC CODE
					}
				}
				if (!__instance.councilor.deleted)
				{
					if (missionResult.Attempted)
					{
						Traverse traverseObj = Traverse.Create(__instance);
						traverseObj.Method("DetectionPhase").SetValue((__instance.councilor, missionResult));
						if (missionResult.Failed)
						{
							__instance.councilor.faction.AddSuspicionForFailure(missionResult);
						}
						if (!__instance.target.deleted && __instance.target.isCouncilorState && __instance.target.ref_faction != null && (!__instance.councilor.isAlien || __instance.target.ref_faction.CanDetectAlienMission(__instance)) && __instance.MissionNoise(missionResult.missionResult) > 0f && __instance.missionTemplate.hate[(int)missionResult.missionResult] > 0f)
						{
							__instance.target.ref_councilor.AddToParanoia(__instance.councilor.faction);
						}
						if (missionResult.Failed && !__instance.councilor.isAlien && __instance.missionTemplate.hate[(int)missionResult.missionResult] > 0f)
						{
							TINotificationQueueState.LogEnemyMissionFailure(__instance, missionResult);
						}
					}
					__instance.councilor.SetCompletedMission(__instance);
					__instance.councilor.SetPriorMission(__instance.missionTemplate, __instance.target);
					__instance.councilor.ClearActiveMission();
				}
				
				__instance.MissionResolved();
				__instance.missionOutcome = missionResult.missionResult;
				// END OF TIMissionState.ResolveMission COPY-PASTED CODE
				__result = missionResult;

				return false;
			}
			
			if (matched) {
				return false;
			}
			else {
				return true;
			}
		}

	}


	// [HarmonyPatch(typeof(TIMissionState), "ResolveMission")]
	// public static class ResolveMissionEffectPatch
	// {
	// 	static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	// 	{
	// 		int instructionInsertionIndex = -1;
	// 		int numFinals = 0;
	// 		var codes = new List<CodeInstruction>(instructions);
	// 		for (int index = 0; index < codes.Count; index++) {
	// 			if(codes[index].opcode == OpCodes.Endfinally) {
	// 				if (numFinals < 1) {
	// 					numFinals += 1;
	// 				}
	// 				else {
	// 					instructionInsertionIndex = index - 1;
	// 					break;
	// 				}
	// 			}
	// 		}
	// 		var instructionsToInsert = new List<CodeInstruction>();

	// 		/*

	// 		if (this.missionTemplate.dataName.Equals("ResistAdvise"))
	// 		{
	// 			this.councilor.ref_nation.AddToUnrest((float)(10 * this.councilor.GetAttribute(CouncilorAttribute.Command, true, true, true, false)) / 100f, 10f);
	// 		}

	// 		*/

	// 		/* (155,6)-(155,63) main.cs //
	// 		/* 0x0000049E 02           // IL_049E: ldarg.0
	// 		/* 0x0000049F 2875300006   // IL_049F: call      instance class TIMissionTemplate PavonisInteractive.TerraInvicta.TIMissionState::get_missionTemplate()
	// 		/* 0x000004A4 6F98120006   // IL_04A4: callvirt  instance string TIDataTemplate::get_dataName()
	// 		/* 0x000004A9 72????????   // IL_04A9: ldstr     "ResistAdvise"
	// 		/* 0x000004AE 6F????????   // IL_04AE: callvirt  instance bool [mscorlib]System.String::Equals(string)
	// 		/* 0x000004B3 2C2F         // IL_04B3: brfalse.s IL_04E4
	// 		*/

	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Field(typeof(TIMissionState), nameof(TIMissionState.missionTemplate))));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Field(typeof(TIDataTemplate), nameof(TIDataTemplate.dataName))));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldstr, "Resist Advise"));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(String), nameof(String.Equals))));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Brfalse_S, 18 * 4));


	// 		/* (156,7)-(156,124) main.cs //
	// 		/* 0x000004B5 02           // IL_04B5: ldarg.0
	// 		/* 0x000004B6 7BF51F0004   // IL_04B6: ldfld     class PavonisInteractive.TerraInvicta.TICouncilorState PavonisInteractive.TerraInvicta.TIMissionState::councilor
	// 		/* 0x000004BB 6F41410006   // IL_04BB: callvirt  instance class PavonisInteractive.TerraInvicta.TINationState PavonisInteractive.TerraInvicta.TIGameState::get_ref_nation()
	// 		/* 0x000004C0 1F0A         // IL_04C0: ldc.i4.s  10
	// 		/* 0x000004C2 02           // IL_04C2: ldarg.0
	// 		/* 0x000004C3 7BF51F0004   // IL_04C3: ldfld     class PavonisInteractive.TerraInvicta.TICouncilorState PavonisInteractive.TerraInvicta.TIMissionState::councilor
	// 		/* 0x000004C8 1A           // IL_04C8: ldc.i4.4
	// 		/* 0x000004C9 17           // IL_04C9: ldc.i4.1
	// 		/* 0x000004CA 17           // IL_04CA: ldc.i4.1
	// 		/* 0x000004CB 17           // IL_04CB: ldc.i4.1
	// 		/* 0x000004CC 16           // IL_04CC: ldc.i4.0
	// 		/* 0x000004CD 6F1F290006   // IL_04CD: callvirt  instance int32 PavonisInteractive.TerraInvicta.TICouncilorState::GetAttribute(valuetype PavonisInteractive.TerraInvicta.CouncilorAttribute, bool, bool, bool, bool)
	// 		/* 0x000004D2 5A           // IL_04D2: mul
	// 		/* 0x000004D3 6B           // IL_04D3: conv.r4
	// 		/* 0x000004D4 220000C842   // IL_04D4: ldc.r4    100
	// 		/* 0x000004D9 5B           // IL_04D9: div
	// 		/* 0x000004DA 2200002041   // IL_04DA: ldc.r4    10
	// 		/* 0x000004DF 6FBF320006   // IL_04DF: callvirt  instance void PavonisInteractive.TerraInvicta.TINationState::AddToUnrest(float32, float32)
	// 		*/

	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TIMissionState), nameof(TIMissionState.councilor))));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Field(typeof(TIGameState), nameof(TIGameState.ref_nation))));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_S, Main.settings.peacekeeperFactor * 2));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TIMissionState), nameof(TIMissionState.councilor))));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_4));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(TICouncilorState), nameof(TICouncilorState.GetAttribute), new Type[] {typeof(CouncilorAttribute), typeof(bool), typeof(bool), typeof(bool), typeof(bool)})));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Mul));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Conv_R4));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Div));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_R4, 10));
	// 		instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(TINationState), nameof(TINationState.AddToUnrest), new Type[] {typeof(float), typeof(float)})));
	// 		FileLog.Log("[factionMissions] Finished Parsing Transpiler");
			
	// 		if (instructionInsertionIndex != -1)
	// 		{
	// 			codes.InsertRange(instructionInsertionIndex, instructionsToInsert);
	// 		}
	// 		return codes;
	// 	}
	// }

	// [HarmonyReversePatch]
	// [HarmonyPatch(typeof(TIMissionEffect_ResistAdvise), "ApplyEffect")]
	// public class TIMissionEffect_ResistAdvise : TIMissionEffect {
	// 	public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
	// 	{
	// 		FileLog.Log("[factionMissions] Run Alpha Alpha");
	// 		if (mission.ref_nation.isNationState) {
	// 			var unrestVal = 2 * Main.settings.peacekeeperFactor * mission.ref_councilor.GetAttribute(CouncilorAttribute.Command) / (float) 100;
	// 			mission.ref_nation.AddToUnrest(-1 * unrestVal);
	// 			FileLog.Log("[factionMissions] Run Bravo Bravo");
	// 			return "Our Advisors on Peacekeeping Operations to "+target.ref_nation.displayName+" are reducing the unrest by " + (unrestVal).ToString("N2");
	// 		}
	// 		FileLog.Log("[factionMissions] Run Ceta Ceta");
	// 		return "Our Advisors have begun... rudimentary Peacekeeping Operations.";
	// 	}
	// }

	// [HarmonyPatch(typeof(TIMissionTemplate), "get_missionIconImagePath_Off")]
	// static class missionIconOffHeaderPatch {
	// 	[HarmonyPrefix]
	// 	static bool missionIconPathOffPatch(String __result, TIMissionTemplate __instance) {
	// 		if (Main.masterMissionList != null && Main.masterMissionList.Contains(__instance.dataName)) {
	// 			__result = __instance.missionIconImagePath;
	// 			return false;
	// 		}
	// 		return true;
	// 	}
	// }
	// [HarmonyPatch(typeof(TIMissionTemplate), "get_missionIconImagePath_On")]
	// static class missionIconOnHeaderPatch {
	// 	[HarmonyPrefix]
	// 	static bool missionIconPathOnPatch(String __result, TIMissionTemplate __instance) {
	// 		if (Main.masterMissionList != null && Main.masterMissionList.Contains(__instance.dataName)) {
	// 			__result = __instance.missionIconImagePath;
	// 			return false;
	// 		}
	// 		return true;
	// 	}
	// }


	// This is NEEDED to add your respective missions when you recruit councillors; Because of this-- you must get NEW councillors inorder to get the missions
	[HarmonyPatch(typeof(TICouncilorState), nameof(TICouncilorState.SetFaction))]
	public static class CouncilorFactionSetPatch {
		[HarmonyPostfix]
		public static void Postfix(TICouncilorState __instance, ref TIFactionState faction) {
			if (Main.destroyMissionList != null) {
				for (int index = 0; index < Main.destroyMissionList.Count; index++) {
					if (__instance.learnedMissionsTemplateNames.Contains(Main.destroyMissionList[index])) {
						__instance.learnedMissions.Remove(TemplateManager.Find<TIMissionTemplate>(Main.destroyMissionList[index], false));
					}
				}
			}
			if (Main.resistMissionList != null) {
				for (int index = 0; index < Main.resistMissionList.Count; index++) {
					if (__instance.learnedMissionsTemplateNames.Contains(Main.resistMissionList[index])) {
						__instance.learnedMissions.Remove(TemplateManager.Find<TIMissionTemplate>(Main.resistMissionList[index], false));
					}
				}
			}
			if (Main.escapeMissionList != null) {
				for (int index = 0; index < Main.escapeMissionList.Count; index++) {
					if (__instance.learnedMissionsTemplateNames.Contains(Main.escapeMissionList[index])) {
						__instance.learnedMissions.Remove(TemplateManager.Find<TIMissionTemplate>(Main.escapeMissionList[index], false));
					}
				}
			}
			if (Main.exploitMissionList != null) {
				for (int index = 0; index < Main.exploitMissionList.Count; index++) {
					if (__instance.learnedMissionsTemplateNames.Contains(Main.exploitMissionList[index])) {
						__instance.learnedMissions.Remove(TemplateManager.Find<TIMissionTemplate>(Main.exploitMissionList[index], false));
					}
				}
			}
			if (Main.studyMissionList != null) {
				for (int index = 0; index < Main.studyMissionList.Count; index++) {
					if (__instance.learnedMissionsTemplateNames.Contains(Main.studyMissionList[index])) {
						__instance.learnedMissions.Remove(TemplateManager.Find<TIMissionTemplate>(Main.studyMissionList[index], false));
					}
				}
			}
			if (Main.submitMissionList != null) {
				for (int index = 0; index < Main.submitMissionList.Count; index++) {
					if (__instance.learnedMissionsTemplateNames.Contains(Main.submitMissionList[index])) {
						__instance.learnedMissions.Remove(TemplateManager.Find<TIMissionTemplate>(Main.submitMissionList[index], false));
					}
				}
			}
			if (Main.serveMissionList != null) {
				for (int index = 0; index < Main.serveMissionList.Count; index++) {
					if (__instance.learnedMissionsTemplateNames.Contains(Main.serveMissionList[index])) {
						__instance.learnedMissions.Remove(TemplateManager.Find<TIMissionTemplate>(Main.serveMissionList[index], false));
					}
				}
			}

			if (Main.settings.destroyMissions && Main.destroyMissionList != null && faction.template.dataName.Equals("DestroyCouncil")) {
				for (int index = 0; index < Main.destroyMissionList.Count; index++) {
					if (!__instance.learnedMissionsTemplateNames.Contains(Main.destroyMissionList[index])) {
						__instance.LearnMission(TemplateManager.Find<TIMissionTemplate>(Main.destroyMissionList[index], false));
					}
				}
			}		
			else if (Main.settings.resistMissions && Main.resistMissionList != null && faction.template.dataName.Equals("ResistCouncil")) {
				for (int index = 0; index < Main.resistMissionList.Count; index++) {
					if (!__instance.learnedMissionsTemplateNames.Contains(Main.resistMissionList[index])) {
						__instance.LearnMission(TemplateManager.Find<TIMissionTemplate>(Main.resistMissionList[index], false));
					}
				}
			}		
			else if (Main.settings.escapeMissions && Main.escapeMissionList != null && faction.template.dataName.Equals("EscapeCouncil")) {
				for (int index = 0; index < Main.escapeMissionList.Count; index++) {
					if (!__instance.learnedMissionsTemplateNames.Contains(Main.escapeMissionList[index])) {
						__instance.LearnMission(TemplateManager.Find<TIMissionTemplate>(Main.escapeMissionList[index], false));
					}
				}
			}		
			else if (Main.settings.exploitMissions && Main.exploitMissionList != null && faction.template.dataName.Equals("ExploitCouncil")) {
				for (int index = 0; index < Main.exploitMissionList.Count; index++) {
					if (!__instance.learnedMissionsTemplateNames.Contains(Main.exploitMissionList[index])) {
						__instance.LearnMission(TemplateManager.Find<TIMissionTemplate>(Main.exploitMissionList[index], false));
					}
				}
			}		
			else if (Main.settings.studyMissions && Main.studyMissionList != null && faction.template.dataName.Equals("CooperateCouncil")) {
				for (int index = 0; index < Main.studyMissionList.Count; index++) {
					if (!__instance.learnedMissionsTemplateNames.Contains(Main.studyMissionList[index])) {
						__instance.LearnMission(TemplateManager.Find<TIMissionTemplate>(Main.studyMissionList[index], false));
					}
				}
			}		
			else if (Main.settings.submitMissions && Main.submitMissionList != null && faction.template.dataName.Equals("AppeaseCouncil")) {
				for (int index = 0; index < Main.submitMissionList.Count; index++) {
					if (!__instance.learnedMissionsTemplateNames.Contains(Main.submitMissionList[index])) {
						__instance.LearnMission(TemplateManager.Find<TIMissionTemplate>(Main.submitMissionList[index], false));
					}
				}
			}		
			else if (Main.settings.serveMissions && Main.serveMissionList != null && faction.template.dataName.Equals("SubmitCouncil")) {
				for (int index = 0; index < Main.serveMissionList.Count; index++) {
					if (!__instance.learnedMissionsTemplateNames.Contains(Main.serveMissionList[index])) {
						__instance.LearnMission(TemplateManager.Find<TIMissionTemplate>(Main.serveMissionList[index], false));
					}
				}
			}
		}
	}

}