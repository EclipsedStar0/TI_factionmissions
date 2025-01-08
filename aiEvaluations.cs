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
			if (__result == -999) {
				// This means the game has flagged it as a Non-Vanalia Mission, giving it -999 means it will NEVER be picked in PayOff
				String missionDataName = mission.dataName;
				if (Main.masterMissionList.Contains(missionDataName)) {
					// High Priority = ~4000
					// Moderate Priority = ~2000
					// Baseline 'I'm here' = ~1000
					// Anything lower are 'Don't pick me'
					// Never Pick = less than 0 (-1)
					// Non-Vanilla = -999
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
									if (CP.faction != faction) {
										// NOT going to calculate total research via projects completed and their cost because that would be resource-expensive
										// Instead going to simply go by research/month and the 'who's in the lead' score ranking
										float temphold = CP.faction.GetAnnualInfluenceCostOfNextControlPoint(CP.nation)/CP.faction.GetBaselineControlPointMaintenanceCost(false);
										temphold *= CP.faction.GetMonthlyIncome(FactionResource.Research, dontRecalculate:true, suppressFactionResourcesUpdatedEvent:true);
										if (CP.faction.permanentAlly(faction)) {
											temphold *= 2f;
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
							tempHold = 7.5f - target.ref_nation.education;
							if (tempHold < 0f) {
								tempHold = 500f + -1f * UnityEngine.Mathf.Pow(6f* Math.Abs(tempHold), 2.15f);
							}
							else {
								tempHold = 500f + UnityEngine.Mathf.Pow(10f * Math.Abs(tempHold), 1.89f);
							}
							__result *= faction.aiValues.informationTechs;
							//Goes ~4000 at 0, ~2400 at 2, ~1300 at 4, ~667 at 6, 500 at 7.5, ~490 at 8, ~170 at 10, Negative at 10.47104
							break;
						case "StudyTechSummit":
							break;
						case "ServeProselytiseCouncillors":
							break;
						default:
							break;
					}
				}
			}
		}
	}
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
