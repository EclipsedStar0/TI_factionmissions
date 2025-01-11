using System;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.Tasks;
using UnityEngine;

namespace factionMissions.AIEvaluations {
	[HarmonyPatch(typeof(AICouncilorMissionPlanner), nameof(AICouncilorMissionPlanner.GetPayoffForMissionTarget_Faction))]
	public static class PayoffTarget_FactionHeaderPatch {
		[HarmonyPostfix]
		public static void Postfix(float __result, ref TIFactionState faction, ref TIMissionTemplate mission, ref TIGameState target, ref List<CampaignMilestone> factionDesiredMilestones, ref Dictionary<TIControlPoint, float> rawControlPointPayoffs, ref Dictionary<TIControlPoint, float> controlPointPayoffs, ref Dictionary<TINationState, float> rawNationPayoffs,ref Dictionary<TINationState, float> nationPayoffs, ref float campaignDuration_years) {
			// Notice: __result == -999 means the game has flagged it as a Non-Vanalia Mission OR a mission targetting a Councillor, giving it <0 means it will NEVER be picked in PayOff>
			if (__result == -999) {
				String missionDataName = mission.dataName;
				if (Main.masterMissionList.Contains(missionDataName)) {
					// High Priority = ~4000
					// Moderate Priority = ~2000
					// Baseline 'I'm here' = ~1000
					// Anything lower are 'Don't pick me'
					// Never Pick = less than 0 (-1)
					// Non-Vanilla = -999
					int cameFromAnotherMission = 0;
					switch (missionDataName) {
						case "DestroyRaiseMilitia":
							List<TIFactionGoalState> factionGoals =  faction.GoalsWithTarget(target.ref_nation);
							int badGoal = 0;
							foreach (TIFactionGoalState factGoal in factionGoals) {
								if (factGoal.GetGoalType() == GoalType.NeutralizeNation) {
									badGoal = 1;
									break;
								}
							}

							if (target.ref_nation.atWar && badGoal == 0) {
								__result = 10f + factionMissions.utilityFunctions.UtilityModule.warStrength(target.ref_nation);
								__result *= faction.aiValues.wantEarthWarCapability;
							}
							else {
								__result = -1f;
							}
							break;
						case "ResistPeacekeepers":
							badGoal = 0;
							float goalSupportVal = 1f;
							factionGoals = faction.GoalsWithTarget(target.ref_nation);
							foreach (TIFactionGoalState factGoal in factionGoals) {
								if (factGoal.GetGoalType() == GoalType.NeutralizeNation) {
									badGoal = 1;
									break;
								}
								else if (factGoal.GetGoalType() == GoalType.MilitarizeNation) {
									goalSupportVal = 1.25f;
								}
							}
							if (badGoal == 1) {
								__result = -1f;
								break;
							}
							__result = 0.50f * AICouncilorMissionPlanner.ControlNationPayoff(faction, target.ref_region.nation.FirstNativeControlPoint(), controlPointPayoffs, campaignDuration_years);
							__result += goalSupportVal * 60f * UnityEngine.Mathf.Pow(target.ref_nation.unrest, 1.65f) * Math.Max(1f, target.ref_nation.unrest)/Math.Max(1f, target.ref_nation.unrestRestState);
							break;
						case "ResistCellNetwork":
							if (!Main.settings.cellnetworksAllowed) {
								__result = -1f;
								break;
							}
							badGoal = 0;
							factionGoals = faction.GoalsWithTarget(target.ref_nation);
							foreach (TIFactionGoalState factGoal in factionGoals) {
								if (factGoal.GetGoalType() == GoalType.PillageNation) {
									badGoal = 1;
									break;
								}
							}
							if (badGoal == 1) {
								__result = -1f;
								break;
							}
							__result = faction.aiValues.wantPopularity * 0.5f * AICouncilorMissionPlanner.PublicOpinionShiftPayoff(faction, target.ref_region.nation, nationPayoffs[target.ref_region.nation]);
							__result += faction.aiValues.gatherInfluence * 0.25f * AICouncilorMissionPlanner.ControlNationPayoff(faction, target.ref_region.nation.FirstNativeControlPoint(), controlPointPayoffs, campaignDuration_years);
							__result += 10f-target.ref_region.populationInMillions;
							if (Main.resistanceNationNetworkSize == null || Main.resistanceRegionNetworkSize == null) {
								// Set up our first cell-network
								__result += 5f * 10f * 25f;
							}
							else if ((Main.resistanceNationNetworkSize != null && Main.resistanceNationNetworkSize.Count < 2) || (Main.resistanceRegionNetworkSize == null && Main.resistanceRegionNetworkSize.Count < 2)) {
								// Set up our first cell-network
								__result += 5f * 10f * 25f;
							}
							else {
								// Not initial cell-network
								if (Main.resistanceNationNetworkSize != null && Main.resistanceNationNetworkSize.ContainsKey(target.displayName)) {
									// If target region's host nation already has a cell network
									if (Main.resistanceRegionNetworkSize == null || !Main.resistanceRegionNetworkSize.ContainsKey(target.ref_region.displayName) || Main.resistanceNationNetworkSize[target.ref_region.displayName] < Main.resistanceNationNetworkSize[target.displayName]) {
										// If the target region's network is smaller than the largest network in the nation
										__result *= 0.25f;
										if (Main.resistanceRegionNetworkSize != null && Main.resistanceRegionNetworkSize.ContainsKey(target.ref_region.displayName)) {
											__result += (10f * (25f - (Main.resistanceRegionNetworkSize[target.ref_region.displayName]))) * 1/faction.aiValues.riskAversion;
										}
									}
									else {
										// Region has No Network, but the Nation *does*
										__result += (10f * 5f) * 1/faction.aiValues.riskAversion;
									}
								}
								else {
									// Nation does not possess a cell-network
									__result += (5f * 10f * 10f) * 1/faction.aiValues.riskAversion;
								}
								foreach (TIRegionState region in target.ref_region.AdjacentRegions(false)) {
									if (Main.resistanceRegionNetworkSize != null) {
										if (Main.resistanceRegionNetworkSize.ContainsKey(region.displayName)) {
											__result += (3f * (25-Main.resistanceRegionNetworkSize[region.displayName])) * 1/faction.aiValues.riskAversion;
										}
										else {
											__result += (3f * 5f) * 1/faction.aiValues.riskAversion;
										}
									}
									else {
										__result += (3f * 5f) * 1/faction.aiValues.riskAversion;
									}
								}
							}
							break;
						case "ResistHumanitarianMission":
							badGoal = 0;
							goalSupportVal = 1f;
							factionGoals = faction.GoalsWithTarget(target.ref_nation);
							foreach (TIFactionGoalState factGoal in factionGoals) {
								if (factGoal.GetGoalType() == GoalType.PillageNation || factGoal.GetGoalType() == GoalType.NeutralizeNation) {
									badGoal = 1;
									break;
								}
								else if (factGoal.GetGoalType() == GoalType.DevelopNation) {
									goalSupportVal = 1.25f;
								}
							}
							if (badGoal == 1) {
								__result = -1f;
								break;
							}
							if (target.ref_nation.inequality < 3f && target.ref_nation.perCapitaGDP > 160000) {
								__result -= 1f;
							}
							else {
								__result = (235f - target.ref_nation.population_Millions) / 50f + (4f - target.ref_nation.inequality * 2f) + 5f * (65f - target.ref_nation.perCapitaGDP / 1000f);
								__result *= faction.aiValues.preserveLife * goalSupportVal;
							}
							break;
						case "ResistSmuggleArms":
							if (target.ref_region.IsOccupied()) {
								__result = -1f;
								break;
							}
							badGoal = 0;
							goalSupportVal = 1f;
							factionGoals = faction.GoalsWithTarget(target.ref_nation);
							foreach (TIFactionGoalState factGoal in factionGoals) {
								if (factGoal.GetGoalType() == GoalType.NeutralizeNation) {
									badGoal = 1;
									break;
								}
								else if (factGoal.GetGoalType() == GoalType.MilitarizeNation) {
									goalSupportVal = 1.75f;
								}
							}
							if (badGoal == 1) {
								__result = -1f;
								break;
							}
							float occupiedScore = 0f;
							foreach (TIRegionState region in target.ref_region.AdjacentRegions(false)) {
								if (region.nation == target.ref_nation || region.nation.IsAlliedWith(target.ref_region.nation, false)) {
									if (region.IsOccupied()) {
										occupiedScore += 10f;
									}
									else if (region.OccupationUnderwayButNotComplete()) {
										occupiedScore += 5f;
									}
									else {
										occupiedScore -= 2f;
									}
								}
							}
							__result = occupiedScore + 10f * factionMissions.utilityFunctions.UtilityModule.warStrength(target.ref_nation);
							if (target.ref_region.alienLanding.Extant() && !faction.shouldNeverAttackAliens) {
								__result *= 100f;
							}
							if (target.ref_region.OccupationUnderwayButNotComplete()) {
								__result *= 10f;
							}
							__result *= faction.aiValues.wantEarthWarCapability * goalSupportVal;
							break;
						case "EscapeFundSpaceProgram":
							if (target.ref_nation.spaceFlightProgram) {
								__result = -1f;
							}
							else {
								goalSupportVal = 1f;
								factionGoals = faction.GoalsWithTarget(target.ref_nation);
								foreach (TIFactionGoalState factGoal in factionGoals) {
									if (factGoal.GetGoalType() == GoalType.SpaceifyNation) {
										goalSupportVal = 1.5f;
										break;
									}
								}
								__result = goalSupportVal * faction.aiValues.wantSpaceFacilities * target.ref_nation.BaseInvestmentPoints_month() * 10f/Math.Max(0.25f, target.ref_nation.BestBoostLatitude);
								// Hypothetical 'best' case, could be 2 * 50 * 10/0.25 = 4000

								// This WILL be blown out if linearity investment points are present; Wherein 'best' case would essentially be this times 60;
							}
							break;
						case "EscapeExpandSpaceAgency":
							if (!target.ref_nation.spaceFlightProgram) {
								__result = -1f;
							}
							else {
								goalSupportVal = 1f;
								factionGoals = faction.GoalsWithTarget(target.ref_nation);
								foreach (TIFactionGoalState factGoal in factionGoals) {
									if (factGoal.GetGoalType() == GoalType.SpaceifyNation) {
										goalSupportVal = 1.5f;
										break;
									}
								}
								__result = goalSupportVal * faction.aiValues.wantSpaceFacilities * (10 * (6 - faction.MissionControlBalance) + 30 * Math.Min(50, target.ref_nation.missionControl));
							}
							break;
						case "ExploitIgnoreEcologicalProtections":
							badGoal = 0;
							goalSupportVal = 1f;
							factionGoals = faction.GoalsWithTarget(target.ref_nation);
							foreach (TIFactionGoalState factGoal in factionGoals) {
								if (factGoal.GetGoalType() == GoalType.DevelopNation) {
									badGoal = 1;
									break;
								}
								else if (factGoal.GetGoalType() == GoalType.PillageNation) {
									goalSupportVal = 1.75f;
								}
								else if (factGoal.GetGoalType() == GoalType.NeutralizeNation) {
									goalSupportVal = 1.15f;
								}
							}
							if (badGoal == 1) {
								__result = -1f;
								break;
							}

							// Easier to pull off closer to a value of 5, towards the midpoint
							float tempHold = -1f * (Math.Abs(5-target.ref_nation.democracy)-3f);
							if (tempHold < 0f) {
								tempHold = UnityEngine.Mathf.Pow(Math.Abs(tempHold), 2.73f) * -1f;
							}
							else {
								tempHold = UnityEngine.Mathf.Pow(tempHold, 1.73f);
							}
							// Means that this modifier will reach ~-6 to 6 to -6 at 0, 5, and 10 respectively.
							// Inequality modifier will range from 0 to ~20, being ~8 at 5 and ~20 at 10.
							__result = tempHold * 3f + UnityEngine.Mathf.Pow(target.ref_nation.inequality, 1.3f);
							__result *= 2 * UnityEngine.Mathf.Pow((target.ref_nation.perCapitaGDP/1000f + 30f)/30f, 1.72f);
							// Per capita modifier goes 2, ~5, ~8.5, ~25, ~48 at 0, 20K, 40K, 100K and 160K respectively
							// Around max values (so best-case for it being 'easy'-- is 1914.87277888)

							// Now we modify by amount of money they have
							tempHold = faction.GetYearlyIncome(FactionResource.Money, true, true) - 200;
							float tempHold2 = faction.resources.GetValueOrDefault(FactionResource.Money) - 250;
							float tempHold3 = faction.aiValues.gatherMoney * -1 * (tempHold2 * 5f + tempHold);
							__result *= goalSupportVal * tempHold3 * (1/faction.aiValues.lifeTechs);
							break;
						case "StudyShareResearch":
							__result = -50f;
							foreach(TIControlPoint CP in target.ref_nation.controlPoints) {
								if (CP.owned) {
									// NOT going to calculate total research via projects completed and their cost because that would be resource-expensive
									// Instead going to simply go by research/month and the 'who's in the lead' score ranking
									float temphold = CP.faction.GetAnnualInfluenceCostOfNextControlPoint(CP.nation)/CP.faction.GetBaselineControlPointMaintenanceCost(false);
									temphold *= CP.faction.GetMonthlyIncome(FactionResource.Research, dontRecalculate:true, suppressFactionResourcesUpdatedEvent:true);
									if (CP.faction != faction) {
										if (CP.faction.permanentAlly(faction)) {
											temphold *= 1.5f;
										}
										switch (CP.faction.GetDiplomacyMood(faction)) {
											case "Tolerance":
												__result += temphold;
												break;
											case "Conflicted":
												__result -= temphold * 0.5f;
												break;
											case "War":
												__result -= temphold * 0.8f;
												break;
											default:
												break;
										}
									}
									else {
										temphold *= 1.5f;
										if (cameFromAnotherMission == 1) {
											temphold *= 2.5f;
										}
										__result += temphold;
									}
								}
							}
							float tempo = target.ref_nation.education;
							if (tempo < 7) {
								tempo = Mathf.Pow(tempo + 2f, 2.1f)-35f;
							}
							else {
								tempo = 10f * Mathf.Pow(tempo, 1.2f) - 37.4f;
							}
							__result = 5f * __result + 2f * tempo + Mathf.Pow((target.ref_nation.perCapitaGDP / 1000f + 20f)/30f, 1.72f)/7f;;
							__result *= faction.aiValues.gatherScience;
							break;
						case "StudyEducatePopulace":
							badGoal = 0;
							goalSupportVal = 1f;
							factionGoals = faction.GoalsWithTarget(target.ref_nation);
							foreach (TIFactionGoalState factGoal in factionGoals) {
								if (factGoal.GetGoalType() == GoalType.PillageNation || factGoal.GetGoalType() == GoalType.NeutralizeNation) {
									badGoal = 1;
									break;
								}
								else if (factGoal.GetGoalType() == GoalType.PillageNation) {
									goalSupportVal = 1.75f;
								}
								else if (factGoal.GetGoalType() == GoalType.NeutralizeNation) {
									goalSupportVal = 1.15f;
								}
							}
							if (badGoal == 1) {
								__result = -1f;
								break;
							}
							__result = 7.5f - target.ref_nation.education;
							if (__result < 0f) {
								__result = 500f + -1f * UnityEngine.Mathf.Pow(6f* Math.Abs(__result), 2.15f);
							}
							else {
								__result = 500f + UnityEngine.Mathf.Pow(10f * Math.Abs(__result), 1.89f);
							}
							__result *= faction.aiValues.informationTechs;
							//Goes ~4000 at 0, ~2400 at 2, ~1300 at 4, ~667 at 6, 500 at 7.5, ~490 at 8, ~170 at 10, Negative at 10.47104
							break;
						case "StudyTechSummit":
							badGoal = 0;
							goalSupportVal = 1f;
							factionGoals = faction.GoalsWithTarget(target.ref_nation);
							foreach (TIFactionGoalState factGoal in factionGoals) {
								if (factGoal.GetGoalType() == GoalType.PillageNation || factGoal.GetGoalType() == GoalType.NeutralizeNation) {
									badGoal = 1;
									break;
								}
								else if (factGoal.GetGoalType() == GoalType.DevelopNation) {
									goalSupportVal = 1.5f;
								}
							}
							if (badGoal == 1) {
								__result = -1f;
								break;
							}
							cameFromAnotherMission = 1;
							goto case "StudyShareResearch";
						case "ServeProselytiseCouncillors":
							__result = 0f;
							if (target.ref_councilor.faction == faction) {
								__result = -1f;
								break;
							}
							// Not having this means we don't even have faction/identity/class
							if (!faction.HasMemoryOnCouncilorBasicData(target.ref_councilor)) {
								__result = -1f;
								break;
							}
							float neccessityToLower = 0;
							switch (faction.GetDiplomacyMood(target.ref_councilor.faction)) {
								case "Tolerance":
									neccessityToLower -= 2f;
									break;
								case "Conflicted":
									neccessityToLower += 1f;
									break;
								case "War":
									neccessityToLower += 2f;
									break;
								default:
									break;
							}
							if (faction.enemyTotalWarFactions.Contains(target.ref_councilor.faction)) {
								neccessityToLower += 4f;
							}
							else if (faction.enemyWarFactions.Contains(target.ref_councilor.faction)) {
								neccessityToLower += 2f;
							}
							if (faction.mostPowerfulHumanEnemy == target.ref_councilor.faction) {
								switch (faction.selfAssessement) {
									case FactionSelfAssessment.LosingBig:
										neccessityToLower += 3f;
										break;
									case FactionSelfAssessment.Losing:
										neccessityToLower += 1.5f;
										break;
									case FactionSelfAssessment.Even:
										neccessityToLower += 0f;
										break;
									case FactionSelfAssessment.Ahead:
										neccessityToLower -= 1.5f;
										break;
									case FactionSelfAssessment.WayAhead:
										neccessityToLower -= 3f;
										break;
								}
							}
							float targetAttriScore = 0;
							float tempAdmin = target.ref_councilor.GetAttribute(CouncilorAttribute.Administration, adminForOrgControl:true);
							float perc = target.ref_councilor.availableAdministration/tempAdmin;
							if (target.ref_councilor.turned) {
								neccessityToLower -= 2f;
							}

							if (faction.HasMemoryOnCouncilorDetails(target.ref_councilor)) {
								if (target.ref_councilor.learnedMissionsTemplateNames.Contains("Inspire")) {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Persuasion) * 2f;
								}
								else {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Persuasion);
								}

								if (faction.IsInTechRace) {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Science) * 3f;
								}
								else {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Science);
								}

								if (target.ref_councilor.learnedMissionsTemplateNames.Contains("InvestigateCouncilor")) {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Investigation) * 3f;
								}
								else {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Investigation) * 1.25f;
								}

								targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Command);
								if (target.ref_councilor.learnedMissionsTemplateNames.Contains("Assassinate")) {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Espionage) * 3f;
								}
								else {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Espionage) * 1.25f;
								}

								targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Security) * 0.5f;
								if (faction.HasMemoryOnCouncilorSecrets(target.ref_councilor)) {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.Loyalty) * 5f;
								}
								else {
									targetAttriScore += target.ref_councilor.GetAttribute(CouncilorAttribute.ApparentLoyalty) * 5f;
								}
								targetAttriScore += 30f * (1-perc);
							}
							__result = targetAttriScore * (neccessityToLower + targetAttriScore/40f) * 10f;
							break;
						default:
							break;
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(AICouncilorMissionPlanner), nameof(AICouncilorMissionPlanner.GetPayoffForMissionTarget_Individual))]
	public static class PayoffTarget_IndividualHeaderPatch {
		[HarmonyPostfix]
		public static void Postfix(float __result, ref TIFactionState faction, ref TIMissionTemplate mission, ref TICouncilorState councilor, ref TIGameState target, ref List<TIMissionTemplate> requiredMissions, ref List<TIMissionTemplate> missingRequiredMissions, ref Dictionary<TINationState, float> nationPayoffs, ref bool huntingForAlienActivity, ref float huntAbility, ref List<TIFactionState> warFactions, ref TIRegionState recentAlienSite, ref float timeSinceAlienSite_days) {
			// Notice: Missions here are ones not caught by PayoffTarget_FactionHeaderPatch, GetPayoffForMissionTarget_Faction, or GetPayoffForMissionTarget_Individual 
			if (__result == 0) {
				String missionDataName = mission.dataName;
				if (Main.masterMissionList.Contains(missionDataName)) {
				
				}
			}
		}
	}

	// This is checking to see the 'Worth' of a nation; Evidently, we want to make sure nations with cell-networks have a higher worth given the time/resources invested into establishing said networks
	[HarmonyPatch(typeof(AIEvaluators), nameof(AIEvaluators.EvaluateNation))]
	public static class EvalNationHeader {
		[HarmonyPostfix]
		public static void Postfix(ref TIFactionState faction, ref TINationState nation, float __result) {
			if (faction.Equals("ResistCouncil")) {
				if (Main.settings.resistMissions && Main.settings.cellnetworksAllowed) {
					if (Main.resistanceNationNetworkSize != null && Main.resistanceNationNetworkSize.ContainsKey(nation.displayName)) {
						__result += Main.resistanceNationNetworkSize[nation.displayName] * 5f;
					}
				}
			}
		}
	}
}
