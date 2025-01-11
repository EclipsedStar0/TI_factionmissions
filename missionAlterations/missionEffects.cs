using System;
using System.Text;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using PavonisInteractive.TerraInvicta;

namespace factionMissions.MissionEffects {
	[HarmonyPatch(typeof(TIArmyState), nameof(TIArmyState.dailyHealRate), MethodType.Getter)]
	public class armyHealthHeaderPatch {
		[HarmonyPostfix]
		public static void dailyHealPatch(TIArmyState __instance, ref float __result) {
			if (Main.armyStrengthTracker != null) {
				if (Main.armyStrengthTracker.ContainsKey(__instance.displayName)) {
					__result += Main.armyStrengthTracker[__instance.displayName];
				}
			}
		}
	}
	[HarmonyPatch(typeof(TIArmyState), nameof(TIArmyState.battleValue), MethodType.Getter)]
	public class armyBattleValueHeader {
		[HarmonyPostfix]
		public static void armyBattleValuePatch(TIArmyState __instance, ref float __result) {
			if (Main.armyStrengthTracker != null) {
				if (Main.armyStrengthTracker.ContainsKey(__instance.displayName)) {
					__result -= 0.5f;
				}
			}
		}
	}

    public class TIMissionEffect_DestroyRaiseMilitia : TIMissionEffect
    {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
			StringBuilder builder = new StringBuilder("");
			if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				float modifier = mission.councilor.GetAttribute(CouncilorAttribute.Persuasion) / 10f + mission.councilor.GetAttribute(CouncilorAttribute.Command) / 15f;
				float friendlyCPs = utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.ref_councilor.faction);
				
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.RegionalAuthorities);
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Warlords);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.75f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.RegionalAuthorities);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.5f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Religion);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.40f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.25f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.MassMedia);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.25f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.25f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.10f;
				}
				modifier += friendlyCPs/4f;
				modifier *= (1+friendlyCPs)/(mission.ref_nation.numControlPoints + 1);
				modifier += mission.ref_nation.GetPublicOpinionProportion(mission.ref_councilor.faction.ideology.ideology) * 2f/3f;
				modifier += -1 * utilityFunctions.UtilityModule.warStrength(mission.ref_nation)/5f;
				if (outcome == TIMissionOutcome.CriticalSuccess) {
					modifier = Math.Max(modifier + 0.5f, modifier * 2f);
				}
				if (Main.armyStrengthTracker == null) {
					Main.armyStrengthTracker = new Dictionary<string, float>();
				}
				if (Main.armyTracker == null) {
					Main.armyTracker = new Dictionary<string, TIArmyState>();
				}
				Traverse traverseObj = Traverse.Create(mission.ref_nation);
				// WARNING: THIS IS CREATING AN ARMY; Code below copy-pasted from TINationState
				bool continueOn = true;
				TIRegionState nextArmyRegion = mission.ref_nation.GetNextArmyRegion();
				if (nextArmyRegion == null)
				{
					foreach (TIControlPoint ticontrolPoint in mission.ref_nation.controlPoints)
					{
						Log.Error("Can't find region to locate new " + mission.ref_nation.displayName + " army", Array.Empty<object>());
						ticontrolPoint.SetControlPointPriority(PriorityType.BuildArmy, 0, false, false);
					}
					continueOn = false;
				}
				if (continueOn) {
					TIArmyState tiarmyState = GameStateManager.CreateNewGameState<TIArmyState>();
					tiarmyState.createdFromTemplate = false;
					tiarmyState.deploymentType = DeploymentType.Standard;
					tiarmyState.controlPointIdx = mission.ref_nation.GetNextArmyControlPointIdx();
					tiarmyState.homeRegion = nextArmyRegion;
					tiarmyState.NewArmy(ArmyType.Human, 0, 1f);
					tiarmyState.MoveArmyToRegion(nextArmyRegion, true);
					mission.ref_nation.AddArmy(tiarmyState);
					TINotificationQueueState.LogNewArmyBuilt(tiarmyState);
					mission.ref_nation.SetDataDirty();
					TIGlobalValuesState.GlobalValues.ModifyMarketValuesForArmyPriority();
					tiarmyState.SetGameStateCreated();
					foreach (TIControlPoint ticontrolPoint2 in mission.ref_nation.controlPoints)
					{
						if (!mission.ref_nation.ValidPriority(PriorityType.BuildArmy))
						{
							ticontrolPoint2.SetControlPointPriority(PriorityType.BuildArmy, 0, false, false);
						}
					}
					tiarmyState.displayName = "(Militia) "+Loc.T(new StringBuilder("TIArmyTemplate.displayName.").Append(tiarmyState.homeRegion.templateName).Append(".").Append((0).ToString()).ToString());
                	tiarmyState.displayNameWithArticle = "(Militia) " + Loc.T(new StringBuilder("TIArmyTemplate.displayNameWithArticle.").Append(tiarmyState.homeRegion.templateName).Append(".").Append((0).ToString()).ToString());

					Main.armyTracker.Add(tiarmyState.displayName, tiarmyState);
					Main.armyStrengthTracker.Add(tiarmyState.displayName, (1-modifier/Math.Max(1, modifier + 8f))/100f);				
				}
			}
			else {
				if (outcome == TIMissionOutcome.CriticalFailure) {

				}
			}
			return builder.ToString();
        }
    }

    public class TIMissionEffect_ResistPeacekeepers : TIMissionEffect {
		public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success) {
			FileLog.Log("Entered");
			if (target.ref_nation == null) {
				return string.Empty;
			}
			else {
				if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
					TIFactionState randFaction = target.ref_nation.WeightedRandomFactionByControlPoints();
					float propoStrength = 0.10f;
					float peacekeepingFactor = 1f;
					propoStrength += (mission.councilor.faction.PropagandaBonus / 3);
					if (target.ref_nation.unrestRestState * 2 > target.ref_nation.unrest) {
						propoStrength += 0.30f;
					}
					if(target.ref_nation.executiveControlPoint.faction == mission.councilor.faction) {
						peacekeepingFactor += 0.25f;
						propoStrength += 0.10f;
					}
					if (target.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus) != null && target.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus).faction == mission.councilor.faction) {
						peacekeepingFactor += 0.15f;
					}
					if (target.ref_nation.GetControlPointOfType(ControlPointType.Warlords) != null && target.ref_nation.GetControlPointOfType(ControlPointType.Warlords).faction == mission.councilor.faction) {
						peacekeepingFactor += 0.25f;
					}
					if (target.ref_nation.GetControlPointOfType(ControlPointType.MassMedia) != null && target.ref_nation.GetControlPointOfType(ControlPointType.MassMedia).faction == mission.councilor.faction) {
						propoStrength += 0.05f;
					}
					if (target.ref_nation.GetControlPointOfType(ControlPointType.Religion) != null && target.ref_nation.GetControlPointOfType(ControlPointType.Religion).faction == mission.councilor.faction) {
						propoStrength += 0.10f;
					}

					if (randFaction == mission.councilor) {
						propoStrength *= 1.15f;
						peacekeepingFactor *= 1.15f;
					}
					else if (randFaction != null) {
						if (randFaction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")) {
							propoStrength *= 1.00f;
							peacekeepingFactor *= 1.00f;
						}
						else if (randFaction.GetDiplomacyMood(mission.councilor.faction).Equals("Conflicted")) {
							propoStrength -= 2f;
							peacekeepingFactor -= 0.35f;
						}
						else if (randFaction.GetDiplomacyMood(mission.councilor.faction).Equals("War")) {
							propoStrength -= 4f;
							peacekeepingFactor -= 0.65f;
						}
					}
					propoStrength += mission.councilor.GetAttribute(CouncilorAttribute.Persuasion) / 50f;
					peacekeepingFactor += mission.councilor.GetAttribute(CouncilorAttribute.Command) / 50f;

					// Highest (other) Crit propo is: (2.3 + 9/3) * mults + 0.5 --> ~5.8 at max
					// Default (other)  highest propo is: (1.15 + 9/3) * mults --> ~4.15 at max
					// Highest (other)  Crit Peace is: (4.3) * mults + 2.5 --> ~ 6.8
					// Default (other)  highest peace is: (2.15) * mults --> ~2.15 

					float opinionOutcome = 0f;
					if (outcome == TIMissionOutcome.CriticalSuccess) {
						propoStrength = (float) Math.Max(propoStrength, propoStrength*2);
						peacekeepingFactor *= (float) Math.Max(peacekeepingFactor, peacekeepingFactor*2);
						propoStrength += TIEffectsState.SumEffectsModifiers(Context.PublicCampaignStrength, mission.councilor.faction, propoStrength);
						propoStrength += UnityEngine.Random.Range(-0.5f, 0.5f);
						opinionOutcome = mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, propoStrength);
					}
						
					peacekeepingFactor += TIEffectsState.SumEffectsModifiers(Context.ArmyUnrestReductionImpact, mission.councilor.faction, peacekeepingFactor);
					peacekeepingFactor += UnityEngine.Random.Range(-2.5f, 2.5f);

					if (randFaction != null && randFaction != mission.councilor.faction) {
						// You need a propoStrength + 1.5f * peacekeepingFactor of 10 or higher to decrease heat
						float hateVal = Math.Max(-6, Math.Min((float) (2f - Math.Pow((double) 1.2f, propoStrength + 1.5f * peacekeepingFactor)), 6));
						randFaction.GainFactionHate(mission.councilor.faction, hateVal);
					}

					float prevUnrest = mission.ref_nation.unrest;
					mission.ref_nation.StabilizeNation(mission.councilor.faction, peacekeepingFactor);
					float nowUnrest = mission.ref_nation.unrest;
					StringBuilder builder = new StringBuilder();
					builder.AppendLine("\n"+Loc.T("TIMissionEffect_ResistPeacekeepers.Special0", new object[] {
						TemplateManager.global.unrestInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourNegativeGood((nowUnrest-prevUnrest), 3)
					}));

					if (outcome == TIMissionOutcome.CriticalSuccess) {
						builder.AppendLine(Loc.T("TIMissionEffect_ResistPeacekeepers.Special1", new object[]
						{
							factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(opinionOutcome, 0, true),
							mission.ref_nation.GetPublicOpinionOfFaction(mission.councilor.faction).ToPercent("P0")
						}));
					}
					return builder.ToString();
				}
				else {
					if (outcome == TIMissionOutcome.CriticalFailure) {
						// Decrease support if enemy have Mass Media/Religion
						// Do unrest increase if enemy have Security/Warlords
						// Heighten effects slightly if at War or enemy have executive
						

						TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus);
						TIFactionState detainFaction = mission.councilor.faction;

						float supportMalus = 1.5f;
						float ownSupport = 0f;
						float detainPeriod = 1f;
						bool flagger = false;
						if(refCP != null && refCP.faction != mission.councilor.faction) {
							// Detain Target
							detainPeriod += 1;
							detainFaction = refCP.faction;
							flagger = true;
							if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("War")) {
								detainPeriod += 0.5f;
							}
							else if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Conflicted")) {
								detainPeriod += 0.25f;
							}
							else if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")) {
								detainPeriod -= 0.50f;
							}
						}
						else if (refCP != null && refCP.faction == mission.councilor.faction) {
							detainPeriod -= 1;
						}
						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Warlords);
						if (refCP != null && refCP.faction != mission.councilor.faction) {
							detainPeriod += 2;
							detainFaction = refCP.faction;
							flagger = true;
							if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("War")) {
								detainPeriod += 1f;
							}
							else if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Conflicted")) {
								detainPeriod += 0.5f;
							}
							else if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")) {
								detainPeriod -= 0.50f;
							}
						}
						else if (refCP != null && refCP.faction == mission.councilor.faction) {
							detainPeriod -= 1;
						}

						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.MassMedia);
						if (refCP != null && refCP.faction != mission.councilor.faction) {
							supportMalus += 1f;
							if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("War")) {
								supportMalus += 0.50f;
							}
							else if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Conflicted")) {
								supportMalus += 0.25f;
							}
							else if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")) {
								ownSupport += 0.20f;
							}
						}
						else if (refCP != null && refCP.faction == mission.councilor.faction) {
							ownSupport += 0.5f;
						}
						
						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Religion);
						if (refCP != null && refCP.faction != mission.councilor.faction) {
							supportMalus += 1.5f;
							if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("War")) {
								supportMalus += 1.50f;
							}
							else if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Conflicted")) {
								supportMalus += 0.75f;
							}
							else if (refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")) {
								ownSupport += 0.50f;
							}
						}
						else if (refCP != null && refCP.faction == mission.councilor.faction) {
							ownSupport += 0.75f;
						}

						ownSupport += mission.councilor.GetAttribute(CouncilorAttribute.Persuasion) / 50f;
						ownSupport += TIEffectsState.SumEffectsModifiers(Context.PublicCampaignStrength, mission.councilor.faction, ownSupport);
						ownSupport += UnityEngine.Random.Range(-1.5f, 1.5f);
						supportMalus += UnityEngine.Random.Range(-1.5f, 1.5f);
						ownSupport = ownSupport - supportMalus;
						float opinionOutcome = mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, ownSupport);
						StringBuilder builder = new StringBuilder();
						if (ownSupport > 0) {
							detainPeriod *= 0.5f;
							detainPeriod -= 1f;
							builder.Append("\n"+Loc.T("TIMissionEffect_ResistPeacekeepers.Special2", new object[]
							{
								factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(opinionOutcome, 0, true),
								mission.ref_nation.GetPublicOpinionOfFaction(mission.councilor.faction).ToPercent("P0")
							}));
						}
						else {
							builder.Append("\n"+Loc.T("TIMissionEffect_ResistPeacekeepers.Special3", new object[]
							{
								factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(opinionOutcome, 0, true),
								mission.ref_nation.GetPublicOpinionOfFaction(mission.councilor.faction).ToPercent("P0")
							}));
						}

						if (flagger && detainPeriod > 0 && detainFaction != null) {
							mission.councilor.DetainCouncilor(detainFaction, detainPeriod, 0, detainFaction!=mission.councilor.faction);
							if (ownSupport > 0) {
								
								float supportBoost = 3 * mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology);
								int counter = (int) supportBoost * 2;
								Traverse traverseObj = Traverse.Create(mission.ref_nation);
								for (int index = 0; index < counter; index++) {
									// To Add IncreaseUnrestAttempts so that coups that result from this tipping the scale go to the faction the people sided with
									mission.ref_nation.IncreaseUnrest(mission.councilor.faction, 0, false);
								}
								float prevUnrest = mission.ref_nation.unrest;
								mission.ref_nation.IncreaseUnrest(mission.councilor.faction, supportBoost, false);
								float nowUnrest = mission.ref_nation.unrest;
								builder.AppendLine(Loc.T("TIMissionEffect_ResistPeacekeepers.Special4", new object[]
								{
									TemplateManager.global.unrestInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(nowUnrest-prevUnrest, 3),
									TemplateManager.global.unrestInlineSpritePath+(nowUnrest).ToString("0.###"),
									detainFaction.displayNameWithColor
								}));
							}
							else {
								builder.AppendLine(Loc.T("TIMissionEffect_ResistPeacekeepers.Special5", new object[]
								{
									detainFaction.displayNameWithColor
								}));
							}
							builder.Append("\n\n["+mission.councilor.displayName+"] will be detained for ["+detainPeriod+"] turns. ");
						}
						return builder.ToString();
					}
					else {
						return string.Empty;
					}
				}
			}
		}
	}

	public class TIMissionEffect_ResistCellNetwork : TIMissionEffect {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
			StringBuilder builder = new StringBuilder("");
			float cellNetworkSize = 0f;
			string refregionName = mission.ref_region.displayName;
			if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				cellNetworkSize += 1 + mission.councilor.GetAttribute(CouncilorAttribute.Command) / Main.settings.cellNetworkEspionageModifier + mission.councilor.GetAttribute(CouncilorAttribute.Espionage) / Main.settings.cellNetworkEspionageModifier;
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.RegionalAuthorities);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					cellNetworkSize += 0.5f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					cellNetworkSize += 0.5f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Religion);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					cellNetworkSize += 0.25f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.MassMedia);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					cellNetworkSize += 0.25f;
				}

				if (outcome == TIMissionOutcome.CriticalSuccess) {
					cellNetworkSize = Math.Max(cellNetworkSize + 0.5f, cellNetworkSize * 2f);
				}
				builder.Append(factionMissions.utilityFunctions.UtilityModule.resistanceNetworkIncrease(cellNetworkSize, refregionName, mission.ref_nation, mission.ref_region));			
				if (Main.settings.adjacentCellNetworks && cellNetworkSize > 2) {
					builder.AppendLine("Additionally-- Our Operatives in the region have managed to expand our Operations beyond the bounds of "+refregionName);
					builder.AppendLine("*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*-*");
					foreach(TIRegionState region in mission.ref_region.AdjacentRegions(true)) {
						float modd = 0.4f;
						if (modd * cellNetworkSize >= 1f) {
							builder.Append(factionMissions.utilityFunctions.UtilityModule.resistanceNetworkIncrease(cellNetworkSize, region.displayName, region.nation, region, modd));
						}
					}
				}			
			}
			else {
				if (outcome == TIMissionOutcome.CriticalFailure) {
					float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
					
					cellNetworkSize -= 1f;
					TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.RegionalAuthorities);
					if (refCP != null) {
						if (refCP.faction == null) {
							cellNetworkSize -= 0.30f;
						}
						else if (!(refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							cellNetworkSize -= 0.5f;
						}
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
					if (refCP != null) {
						if (refCP.faction == null) {
							cellNetworkSize -= 0.30f;
						}
						else if (!(refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							cellNetworkSize -= 0.5f;
						}
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Legislature);
					if (refCP != null) {
						if (refCP.faction == null) {
							cellNetworkSize -= 0.30f;
						}
						else if (!(refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							cellNetworkSize -= 0.5f;
						}
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus);
					if (refCP != null) {
						if (refCP.faction == null) {
							cellNetworkSize -= 0.30f;
						}
						else if (!(refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							cellNetworkSize -= 0.50f;
						}
					}
					cellNetworkSize += friendlyCPs/4f;
					cellNetworkSize *= 1 - (1f + friendlyCPs)/(1f + 2 * mission.ref_nation.numControlPoints);
					builder.Append(factionMissions.utilityFunctions.UtilityModule.resistanceNetworkDecrease(cellNetworkSize, refregionName, mission.ref_nation));
				}
			}
        	return builder.ToString();
        }
    }

	[HarmonyPatch(typeof(TIRegionState), nameof(TIRegionState.NationalGDPProportion))]
	public static class GDPProportionHeaderPatcj {
		[HarmonyPrefix]
		public static bool GDPProportionPatch(TIRegionState __instance, ref float __result) {
			float num = 0f;
			float num2 = 0f;
			bool enabled = false;
			if (Main.settings.GDPModifiers && Main.resistanceRegionGDPModifiers != null && Main.resistanceRegionGDPModifiers.Count > 1) {
				enabled = true;
			}
			foreach (TIRegionState tiregionState in __instance.nation.regions)
			{
				float num3 = tiregionState.populationInMillions;

				if (enabled && Main.resistanceRegionGDPModifiers.ContainsKey(tiregionState.displayName)) {
					num *= (float) (1 + Main.resistanceRegionGDPModifiers[tiregionState.displayName]);
				}

				if (tiregionState.coreEconomicRegion)
				{
					num3 *= TemplateManager.global.coreEcoRegionGDPModifier;
				}
				if (tiregionState.resourceRegion)
				{
					num3 *= TemplateManager.global.coreResourceRegionGDPModifier;
				}
				if (tiregionState.colonyRegion)
				{
					num3 *= TemplateManager.global.colonyRegionGDPModifier;
				}
				if (tiregionState == __instance)
				{
					num2 = num3;
				}
				num += num3;
			}
			__result =  num2 / num;
			return false;
		}
	}

    public class TIMissionEffect_ResistHumanitarianMission : TIMissionEffect
    {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
			StringBuilder builder = new StringBuilder("");
            if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				float modifier = 0.25f + mission.councilor.GetAttribute(CouncilorAttribute.Administration) / 5f + mission.councilor.GetAttribute(CouncilorAttribute.Science) / 5f;
				
				modifier += Math.Min(Math.Max(0.10f, mission.councilor.faction.GetNetDailyIncome(FactionResource.Influence) / 5f), 3f);
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.AgriculturalSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 3.5f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.ExtractiveSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 1.5f;
				}
				
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.NationalIndustries);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 1.0f;
				}
				
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.TradeUnions);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.5f;
				}
				
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.RegionalAuthorities);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 1.5f;
				}
				
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Legislature);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.75f;
				}
				
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 1.00f;
				}

				float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
				modifier += friendlyCPs/4f;
				modifier *= (1f + friendlyCPs)/(1f + mission.ref_nation.numControlPoints);

				if (outcome == TIMissionOutcome.CriticalSuccess) {
					modifier = Math.Max(modifier + 0.5f, modifier * 2f);
				}

				float prevPopulation = mission.ref_region.populationInMillions;
				float prevGrowth = mission.ref_region.annualPopGrowthModifier;
				float prevShare = mission.ref_region.NationalGDPProportion();
				double prevRegionPC = mission.ref_region.regionalPerCapitaGDP;
				double prevNationPC = mission.ref_nation.perCapitaGDP;
				if (mission.ref_region.annualPopulationGrowth < 0) {
					mission.ref_region.ChangeAnnualPopulationGrowthModifier(modifier / 50f);
				}
				else {
					mission.ref_region.ChangeAnnualPopulationGrowthModifier(modifier / 150f);
				}
				double popGrowthMonth = UnityEngine.Mathd.Pow(1.0 + 0.005 + mission.ref_region.annualPopulationGrowth, 0.0833333358168602) - 1.0f;
				popGrowthMonth += (double)UnityEngine.Random.Range(-0.000412f, 0.000412f);				
				if (popGrowthMonth < 0) {
					mission.ref_region.ChangePopulation_Millions((float) Math.Abs(popGrowthMonth * mission.ref_region.populationInMillions) * modifier / 10f);
					//mission.ref_region.ChangeAnnualPopulationGrowthModifier(modifier / 50f);
					builder.Append("\nOur relief mission to "+mission.ref_region.displayName+" has managed to save an estimated "+TemplateManager.global.populationInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood((mission.ref_region.populationInMillions-prevPopulation))+" Million people from an untimely death-- bringing Projected Population counts in the region to "+TemplateManager.global.populationInlineSpritePath+mission.ref_region.populationInMillions.ToString("0.###")+". ");
				}
				else {
					mission.ref_region.ChangePopulation_Millions((float) Math.Abs(popGrowthMonth * mission.ref_region.populationInMillions) * modifier / 10f);
					//mission.ref_region.ChangeAnnualPopulationGrowthModifier(modifier / 150f);
					builder.Append("\nOur relief mission to "+mission.ref_region.displayName+" has unexpectedly led to a 'baby-boom' as a result of our efforts. The population in the region is estimated to have grown by "+TemplateManager.global.populationInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood((mission.ref_region.populationInMillions-prevPopulation), 3)+" Million inhabitants, up to a total of "+TemplateManager.global.populationInlineSpritePath+mission.ref_region.populationInMillions.ToString("0.###")+". ");
				}
				float prevInequality = mission.ref_nation.inequality;
				double gdpCapitaPercentChange = (modifier + mission.councilor.faction.GetNetDailyIncome(FactionResource.Money) / 10f - Mathd.Pow(prevInequality, 1.60)/10f)/100f;
				// double acChange = gdpCapitaPercentChange * mission.ref_region.population/mission.ref_nation.population;
				// float prevNationPerCapita = mission.ref_nation.perCapitaGDP;
				double gdpFormula = (mission.ref_region.regionalPerCapitaGDP - gdpCapitaPercentChange * 50 < 5623) ? 5000 : (mission.ref_region.regionalPerCapitaGDP - gdpCapitaPercentChange * 50 < 20000) ? 10000000 / (2*Mathd.Pow(mission.ref_region.regionalPerCapitaGDP - gdpCapitaPercentChange * 50 , 0.8f)) : -2 * Mathd.Pow(prevRegionPC - 20000 - gdpCapitaPercentChange * 50, 0.7f) + 1811.949159;
				gdpFormula = Math.Min(5000, Math.Max(1, gdpFormula));
				// mission.ref_nation.ModifyGDP((mission.ref_region.populationInMillions-prevPopulation) * Math.Min(2000, Math.Max(100, 0.2f * mission.ref_region.regionalPerCapitaGDP)));
				double shippedAid = 10 * 1000000 * (mission.ref_region.populationInMillions-prevPopulation) * (mission.ref_region.regionalPerCapitaGDP + Math.Min(5000, Math.Max(1, gdpFormula)));
				mission.ref_nation.ModifyGDP(shippedAid);
				if (Main.settings.GDPModifiers) {
					if (Main.resistanceRegionGDPModifiers == null) {
						Main.resistanceRegionGDPModifiers = new Dictionary<string, double>() {{"FakeRegion", 0f}};
					}
					if (Main.resistanceRegionGDPModifiers.ContainsKey(mission.ref_region.displayName)) {	
						Main.resistanceRegionGDPModifiers[mission.ref_region.displayName] *= ((mission.ref_region.populationInMillions-prevPopulation)/prevPopulation) * Math.Max((gdpFormula + mission.ref_region.regionalPerCapitaGDP)/mission.ref_region.regionalPerCapitaGDP, (gdpFormula + mission.ref_region.regionalPerCapitaGDP)/mission.ref_nation.perCapitaGDP);
					}
					else {	
						Main.resistanceRegionGDPModifiers[mission.ref_region.displayName] = ((mission.ref_region.populationInMillions-prevPopulation)/prevPopulation) * Math.Max((gdpFormula + mission.ref_region.regionalPerCapitaGDP)/mission.ref_region.regionalPerCapitaGDP, (gdpFormula + mission.ref_region.regionalPerCapitaGDP)/mission.ref_nation.perCapitaGDP);
					}
				}


				// mission.ref_nation.GDPPctChange((float) acChange);
				// if (Main.settings.GDPModifiers) {
				// 	if (Main.resistanceRegionGDPModifiers == null) {
				// 		Main.resistanceRegionGDPModifiers = new Dictionary<string, double>() {{"FakeRegion", 0f}};
				// 	}
				// 	if (Main.resistanceRegionGDPModifiers.ContainsKey(mission.ref_region.displayName)) {	
				// 		Main.resistanceRegionGDPModifiers[mission.ref_region.displayName] += (prevShare * (1+acChange * 3) / mission.ref_region.NationalGDPProportion()) * (mission.ref_region.populationInMillions/prevPopulation);
				// 	}
				// 	else {	
				// 		Main.resistanceRegionGDPModifiers[mission.ref_region.displayName] = (prevShare * (1+acChange * 3) / mission.ref_region.NationalGDPProportion()) * (mission.ref_region.populationInMillions/prevPopulation);
				// 	}
				// }
				// else {
				// 	mission.ref_nation.GDPPctChange(((1 + mission.ref_nation.BaseInvestmentPoints_month() / 20f) * modifier * mission.ref_nation.economyPriorityPerCapitaIncomeChange / 10)/mission.ref_nation.perCapitaGDP);
				// }
				
				//double ineqChange = (mission.ref_region.population/mission.ref_nation.population) * 0.25f * mission.ref_nation.economyPriorityInequalityChange * (mission.ref_region.regionalPerCapitaGDP - prevRegionPC) / mission.ref_nation.economyPriorityPerCapitaIncomeChange;
				double ineqChange = (mission.ref_region.populationInMillions - prevPopulation)/mission.ref_nation.population_Millions * (gdpFormula + prevRegionPC)/mission.ref_nation.economyPriorityPerCapitaIncomeChange * mission.ref_nation.economyPriorityInequalityChange;
				mission.ref_nation.AddToInequality((float) ineqChange, TINationState.InequalityChangeReason.Events);
				builder.Append("Annual Growth Rate in "+mission.ref_region.displayName+" has increased by "+TemplateManager.global.lifeTechInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood((mission.ref_region.annualPopGrowthModifier-prevGrowth)/100, 3, true)+" to "+TemplateManager.global.lifeTechInlineSpritePath+(mission.ref_region.annualPopGrowthModifier/100).ToPercent("P3")+". ");
				builder.Append(""+mission.councilor.faction.displayName+" has shipped in a total of "+TIUtilities.FormatBigNumber(shippedAid)+" to "+mission.ref_region.displayName+", providing an estimated "+TemplateManager.global.populationInlineSpritePath+TIUtilities.FormatBigNumber(1000000 * (mission.ref_region.populationInMillions-prevPopulation))+" people with a GDP-PerCapita of "+TemplateManager.global.perCapitaGDPInlineSpritePath+((int) (gdpFormula+mission.ref_region.regionalPerCapitaGDP)).ToString()+". ");
				builder.Append("As a result of this aid, GDP-PerCapita in the region as a whole has changed by "+TemplateManager.global.perCapitaGDPInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood((float) (mission.ref_region.regionalPerCapitaGDP - prevRegionPC))+" to "+TemplateManager.global.perCapitaGDPInlineSpritePath+(int) mission.ref_region.regionalPerCapitaGDP+". ");
				builder.Append("As a result, National GDP Concentration in the region has changed by "+TemplateManager.global.ECO_InlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood((mission.ref_region.NationalGDPProportion() - prevShare), 2, true)+" to "+TemplateManager.global.ECO_InlineSpritePath+mission.ref_region.NationalGDPProportion().ToPercent("P2")+". ");
				builder.Append("Additionally, Inequality in the region has changed by "+TemplateManager.global.inequalityInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(mission.ref_nation.inequality - prevInequality, 3) + " to a value of "+TemplateManager.global.inequalityInlineSpritePath+mission.ref_nation.inequality.ToString("0.###")+". ");
				builder.Append("Additionally, public opinion of "+mission.councilor.faction.displayName+" has changed by "+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, modifier), 0, true)+" to a value of "+mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology).ToPercent("P0")+". ");

			}
			else {
				if (outcome == TIMissionOutcome.CriticalFailure) {
					float badOutcomeStrength = -3f + mission.councilor.GetAttribute(CouncilorAttribute.Administration) / 20f;
					TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.AgriculturalSector);
					if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
						badOutcomeStrength += 2.0f;
					}
					
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Bureaucracy);
					if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
						badOutcomeStrength += 0.50f;
					}
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
					if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
						badOutcomeStrength += 0.50f;
					}

					float randVal = UnityEngine.Random.Range(0, 10);
					float publicSuportModifier = 1f;
					if (randVal < 2) {
						float prevIneq = mission.ref_nation.inequality;
						badOutcomeStrength -= 2.5f * mission.ref_nation.corruption;
						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Oligarchs);
						if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
							badOutcomeStrength -= 0.75f;
						}
						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Aristocracy);
						if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
							badOutcomeStrength -= 0.50f;
						}
						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Corporations);
						if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
							badOutcomeStrength -= 0.20f;
						}

						mission.ref_nation.AddToInequality(badOutcomeStrength * mission.ref_nation.spoilsPriorityInequalityChange, TINationState.InequalityChangeReason.Events);
						builder.Append("\nOur relief mission to "+mission.ref_region+" has not gone to plan. Trapped in a mire of Bureaucracy and enterprising Elites, most of the aid we sent to the region was redirected or misappropriated. As a result, inequality in the region has changed by "+TemplateManager.global.inequalityInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(mission.ref_nation.inequality-prevIneq)+" to a value of "+TemplateManager.global.inequalityInlineSpritePath+mission.ref_nation.inequality+". ");
					}
					else {
						float prevPopulation = mission.ref_region.population / 1000000;
						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus);
						badOutcomeStrength -= mission.ref_nation.unrest / 5f;
						if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
							badOutcomeStrength -= 0.75f;
						}
						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
						if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
							badOutcomeStrength -= 0.50f;
						}
						refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Warlords);
						if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
							badOutcomeStrength -= 0.20f;
						}

						double popGrowthMonth = mission.ref_region.annualPopulationGrowth / 12f;
						mission.ref_region.ChangePopulation_Millions((float) Math.Abs(popGrowthMonth) * badOutcomeStrength / 1000000f);

						builder.Append("Our relief mission to "+mission.ref_region+" has been sabotaged! ");
						string PopString = TemplateManager.global.populationInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood((mission.ref_region.populationInMillions-prevPopulation)/1000000, 3)+" Million inhabitants and bringing the population down to a total of "+mission.ref_region.populationInMillions;
						if (randVal < 4) {
							mission.ref_region.ChangeAnnualPopulationGrowthModifier(badOutcomeStrength/100f);
							builder.Append("Someone has poisoned the food we sent in the relief mission-- killing "+PopString+". As a direct result of this-- the Region's Annual Population growth has "+TIUtilities.RedLine("permenantly")+"decreased by "+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(badOutcomeStrength/100f, 3)+" as well");
						}
						else if (randVal < 7) {
							double prevNationGDP = mission.ref_nation.GDP;
							mission.ref_nation.ModifyGDP(badOutcomeStrength * mission.ref_nation.economyPriorityPerCapitaIncomeChange * mission.ref_region.population);
							builder.Append("Someone has sabotaged vital infrastructure in the region-- inadvertedly leading to the deathes of "+PopString+". This destruction of industry has altered National GDP by "+TemplateManager.global.ECO_InlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood((float) (mission.ref_nation.GDP - prevNationGDP)/1000000000f)+" Bn to a value of "+(mission.ref_nation.GDP/1000000000f).ToString("N0")+" Bn. ");
						}
						else {
							float prevUnrest = mission.ref_nation.unrest;
							mission.ref_nation.AddToUnrest(Math.Abs(mission.ref_nation.militaryPriorityUnrestChange) * badOutcomeStrength);
							builder.Append("A violent Paramilitary organization assaulted our Relief Mission-- killing "+PopString+". As a result of these actions, Unrest has risen by "+TemplateManager.global.unrestInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(mission.ref_nation.unrest-prevUnrest)+" to a value of "+TemplateManager.global.unrestInlineSpritePath+mission.ref_nation.unrest.ToString("0.###"));
						}
					}
					builder.Append("Additionally, Public Support for "+mission.councilor.faction.displayName+" has changed by "+mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, badOutcomeStrength * publicSuportModifier)+" to a value of "+mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology).ToPercent("P0")+". ");
				}
			}
			return builder.ToString();
        }
    }

    public class TIMissionEffect_ResistSmuggleArms : TIMissionEffect
    {
         public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
			StringBuilder builder = new StringBuilder("");
			bool clearenceFlag = false;
			float cellSize = 0f;
			if (Main.settings.cellnetworksAllowed && Main.resistanceRegionNetworkSize != null && Main.resistanceRegionNetworkSize.ContainsKey(mission.ref_region.displayName)) {
				if (Main.resistanceRegionNetworkSize[mission.ref_region.displayName] >= 4f) {
					clearenceFlag = true;
				}
				else {
					cellSize = Main.resistanceRegionNetworkSize[mission.ref_region.displayName];
				}
			}
			else if (!Main.settings.cellnetworksAllowed && mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology) > 0.1) {
				clearenceFlag = true;
			}
			if (!clearenceFlag && Main.settings.cellnetworksAllowed) {
				builder.AppendLine("We do not yet have enough Clandestine Support within the target region to effectively smuggle arms into "+mission.ref_region.displayName+". The Cell Network in "+mission.ref_region.displayName+" is only of size ["+cellSize+"], when a size of [4] or higher is required. ");
				return builder.ToString();
			}
			else if (!clearenceFlag) {
				builder.AppendLine("We do not yet have enough Public Support within the target region to effectively smuggle arms into "+mission.ref_region.displayName+". Our public support in the region is only ["+mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology).ToPercent("P0")+"] of the 10% required. ");
				return builder.ToString();
			}

            if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				float modifier = 1f + mission.councilor.GetAttribute(CouncilorAttribute.Command) / 5f + mission.councilor.GetAttribute(CouncilorAttribute.Espionage) / 10f;
				if (Main.settings.cellnetworksAllowed) {
                	modifier += Main.resistanceRegionNetworkSize[mission.ref_region.displayName] / 10;
				}
				else {
                	modifier += mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology);
				}

				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 1f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.5f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Warlords);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.25f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.25f;
				}

				float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
				modifier += friendlyCPs/4f;
				modifier *= (1f + friendlyCPs)/(1f + mission.ref_nation.numControlPoints);

				modifier += utilityFunctions.UtilityModule.warStrength(mission.ref_nation);
				
				if (mission.ref_region.BorderWithAnotherNation(true)) {
					modifier += 1.5f;
				}

				if (outcome == TIMissionOutcome.CriticalSuccess) {
					modifier = Math.Max(modifier + 0.5f, modifier * 2f);
				}

				float prevMiltech = mission.ref_nation.militaryTechLevel;
				if (Main.settings.cellnetworksAllowed) {
					float prevDef = 0f;
					if (Main.resistanceRegionArms.ContainsKey(mission.ref_region.displayName)) {
						prevDef = Main.resistanceRegionArms[mission.ref_region.displayName];
						Main.resistanceRegionArms[mission.ref_region.displayName] += modifier / 20f;
					}
					else {
						Main.resistanceRegionArms[mission.ref_region.displayName] = modifier / 20f;
					}
					Main.resistanceRegionArms[mission.ref_region.displayName] = Math.Min(Main.resistanceRegionArms[mission.ref_region.displayName], 25f);
					builder.Append("\nThe Operatives we've dispatched to "+mission.ref_region.displayName+" have successfully supplied local supporters with armaments.");
					if (!Main.settings.costlyMissionsDisabled) {
						builder.Append("We can expect the defensiveness of the region to change by "+TemplateManager.global.habDefenseScoreInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(Main.resistanceRegionArms[mission.ref_region.displayName]-prevDef, 3)+" to a value of "+TemplateManager.global.habDefenseScoreInlineSpritePath+Main.resistanceRegionArms[mission.ref_region.displayName].ToString("0.###")+". ");
					}
					builder.Append(factionMissions.utilityFunctions.UtilityModule.resistanceNetworkDecrease(2, mission.ref_region.displayName, mission.ref_nation));
				}
				else {
					modifier *= 1.25f;
				}
				float modifierLite = modifier * mission.ref_region.NationalGDPProportion() * mission.ref_nation.BaseInvestmentPoints_month();
				float prevArmyProgress = mission.ref_nation.GetAccumulatedInvestmentPoints(PriorityType.BuildArmy);
				mission.ref_nation.AddToMilitaryTechLevel(modifierLite / TemplateManager.global.GetRequiredInvestmentPoints(PriorityType.Military) * mission.ref_nation.militaryPriorityTechLevelChange / 20f);
				mission.ref_nation.ModifyAccumulatedInvestment(PriorityType.BuildArmy, modifierLite / TemplateManager.global.GetRequiredInvestmentPoints(PriorityType.BuildArmy) / 20f, false, true);
				builder.Append("The Miltech of "+mission.ref_nation.displayName+" has changed by "+TemplateManager.global.miltechInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.militaryTechLevel - prevMiltech, 3)+" to a value of "+TemplateManager.global.miltechInlineSpritePath+mission.ref_nation.militaryTechLevel.ToString("0.###")+". ");
				builder.Append("The Operatives we sent to "+mission.ref_region.displayName+" have also begun training up local militias and have increased Recruitment Progress by "+TemplateManager.global.ARM_InlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.GetAccumulatedInvestmentPoints(PriorityType.BuildArmy) - prevArmyProgress, 3)+" to a value of "+TemplateManager.global.ARM_InlineSpritePath+mission.ref_nation.GetAccumulatedInvestmentPoints(PriorityType.BuildArmy).ToString("0.###")+". ");
			}
			else {
				if (outcome == TIMissionOutcome.CriticalFailure) {
					float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
					float cellNetworkSize = 0f;
					
					cellNetworkSize -= 1f;
					TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.RegionalAuthorities);
					if (refCP != null) {
						if (refCP.faction == null) {
							cellNetworkSize -= 0.30f;
						}
						else if (!(refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							cellNetworkSize -= 0.5f;
						}
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
					if (refCP != null) {
						if (refCP.faction == null) {
							cellNetworkSize -= 0.30f;
						}
						else if (!(refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							cellNetworkSize -= 0.5f;
						}
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Legislature);
					if (refCP != null) {
						if (refCP.faction == null) {
							cellNetworkSize -= 0.30f;
						}
						else if (!(refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							cellNetworkSize -= 0.5f;
						}
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus);
					if (refCP != null) {
						if (refCP.faction == null) {
							cellNetworkSize -= 0.30f;
						}
						else if (!(refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							cellNetworkSize -= 0.50f;
						}
					}

					cellNetworkSize -= 0.50f * mission.ref_region.NumArmiesPresent(true, true, false, false);
					cellNetworkSize += friendlyCPs/4f;
					cellNetworkSize *= 1 - (1f + friendlyCPs)/(1f + 2 * mission.ref_nation.numControlPoints);
					builder.Append(factionMissions.utilityFunctions.UtilityModule.resistanceNetworkDecrease(cellNetworkSize, mission.ref_region.displayName, mission.ref_nation));
					builder.Append("As a result of our failed Arms-Smuggling operation in "+mission.ref_region+", Public Support in "+mission.ref_nation+" has changed by "+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, cellNetworkSize), 0, true)+" to a value of "+mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology).ToPercent("P0"));
				}
			}
			
			return builder.ToString();
        }
    }

	// Harmony patch specifically for ResistSmuggleArms
	[HarmonyPatch(typeof(TIArmyState), nameof(TIArmyState.LocalForcesBaseDefenseLevel))]
	public static class localDefenseHeaderPatch {
		[HarmonyPostfix]
		public static void localDefencePatch(TIArmyState __instance, ref float __result) {
			if (!Main.settings.costlyMissionsDisabled && Main.settings.resistMissions && Main.settings.cellnetworksAllowed && Main.resistanceRegionArms != null) {
				if (Main.resistanceRegionArms.ContainsKey(__instance.currentRegion.displayName)) {
					float modifier = 1f;
					if (!__instance.FriendlyRegion(__instance.currentRegion)) {
						modifier *= -1f;
						// if (__instance.faction.displayName.Equals("ResistCouncil")) {
						// 	modifier *= -1f;
						// }
						// foreach(TICouncilorState councilor in __instance.faction.councilors) {
						// 	if (councilor.learnedMissionsTemplateNames.Contains("ResistSmuggleArms")) {
						// 		modifier *= -1f;
						// 		break;
						// 	}
						// }
					}
					// FileLog.Log("[factionMissions] Applying modifier of ["+(modifier * Main.resistanceRegionArms[__instance.currentRegion.displayName])+"] to local defensive value of Region: ["+__instance.currentRegion.displayName+"]");
					__result += modifier * Math.Min(2.5f * Main.resistanceRegionArms[__instance.currentRegion.displayName]/25f, 2.5f);
				}
				else {
					// FileLog.Log("[factionMissions] Region: ["+__instance.currentRegion.displayName+"] in not in resistanceRegionArms[].");
				}
			}
			else {
				// FileLog.Log("[factionMissions] TIArmyState.LocalForcesBaseDefenseLevel reports that Main.settings.resistMissions is:"+Main.settings.resistMissions+"; Main.settings.cellnetworksAllowed is:"+Main.settings.cellnetworksAllowed+"; Main.resistanceRegionArms != null is:"+(Main.resistanceRegionArms != null)+"; ");
			}
		}
	}

	[HarmonyPatch(typeof(TIArmyState), nameof(TIArmyState.CombatBreakdown_Army))]
	public static class combatBreakdownHeader {	
		[HarmonyPostfix]
		public static void combatBreakdownPatch(TIArmyState __instance, ref string __result) {
			if (!Main.settings.costlyMissionsDisabled) {
				if(Main.settings.resistMissions && Main.resistanceRegionArms != null) {
					if (Main.resistanceRegionArms.ContainsKey(__instance.currentRegion.displayName)) {
						StringBuilder builder = new StringBuilder(__result);
						float val = Main.resistanceRegionArms[__instance.currentRegion.displayName];
						if (!__instance.FriendlyRegion(__instance.currentRegion)) {
							val *= -1f;
						}
						
						if (val > 0) {
							__result += __instance.combatEffectiveness * val;
							builder.AppendLine(" +"+val.ToString("0.###")+" from Clandestine Operatives");
						}
						else if (val < 0 && __instance.combatEffectiveness > 0) {
							__result += Math.Max(1/__instance.combatEffectiveness * val, -1.4f);
							builder.AppendLine(" "+val.ToString("0.###")+" from Clandestine Operatives");
						}
						__result = builder.ToString();
					}
				}
				if (Main.armyStrengthTracker != null && Main.armyStrengthTracker.ContainsKey(__instance.displayName)) {
					StringBuilder builder = new StringBuilder(__result);
					float val = 0f;
					if (Main.armyTypeTracker.ContainsKey(__instance.displayName)) {
						switch (Main.armyTypeTracker[__instance.displayName]) {
							case 0: val = -0.5f; builder.AppendLine(" "+val.ToString("0.###")+" from being Militia"); break;
							case 1: val = 0.5f; builder.AppendLine(" +"+val.ToString("0.###")+" from being SpecOps"); break;
							default: break;
						}
					}
					__result = builder.ToString();
				}				
			}
			// else {
			// 	FileLog.Log("[factionMissions] TIArmyState.CombatBreakdown_Army reports that Main.settings.resistMissions is:"+Main.settings.resistMissions+"; Main.resistanceRegionArms != null is:"+(Main.resistanceRegionArms != null)+"; ");
			// }
		}
	}

	[HarmonyPatch(typeof(TIArmyState), nameof(TIArmyState.GetAttackValue))]
	public static class armyAttackHeader {
		[HarmonyPostfix]
		public static void armyAttackValuePatch(TIArmyState __instance, ref float __result) {
			if (!Main.settings.costlyMissionsDisabled && Main.settings.resistMissions && Main.resistanceRegionArms != null) {
				if (Main.resistanceRegionArms.ContainsKey(__instance.currentRegion.displayName)) {
					float modifier = 1f;
					float val = Main.resistanceRegionArms[__instance.currentRegion.displayName];
					if (!__instance.FriendlyRegion(__instance.currentRegion)) {
						modifier *= -1f;
						// if (__instance.faction.displayName.Equals("ResistCouncil")) {
						// 	modifier *= -1f;
						// }
						// foreach(TICouncilorState councilor in __instance.faction.councilors) {
						// 	if (councilor.learnedMissionsTemplateNames.Contains("ResistSmuggleArms")) {
						// 		modifier *= -1f;
						// 		break;
						// 	}
						// }
					}
					val *= modifier;
					if (val > 0) {
						__result += __instance.combatEffectiveness * val;
					}
					else if (val < 0 && __instance.combatEffectiveness > 0) {
						__result += Math.Max(1/__instance.combatEffectiveness * val, -1.4f);
					}
				}
				else {
					// FileLog.Log("[factionMissions] Region: ["+__instance.currentRegion.displayName+"] in not in resistanceRegionArms[].");
				}
			}
			else {
				// FileLog.Log("[factionMissions] TIArmyState.LocalForcesBaseDefenseLevel reports that Main.settings.resistMissions is:"+Main.settings.resistMissions+"; Main.settings.cellnetworksAllowed is:"+Main.settings.cellnetworksAllowed+"; Main.resistanceRegionArms != null is:"+(Main.resistanceRegionArms != null)+"; ");
			}
		}
	}

	[HarmonyPatch(typeof(TIRegionState), nameof(TIRegionState.IncreaseOccupationValue))]
	public static class increaseOccupationHeader {
		[HarmonyPrefix]
		public static bool increaseOccupationPatch(TIRegionState __instance, ref float value) {
			if (!Main.settings.costlyMissionsDisabled && Main.resistanceRegionArms != null && Main.resistanceRegionArms.ContainsKey(__instance.displayName)) {
				float val = Mathf.Clamp(Main.resistanceRegionArms[__instance.displayName] / 25f, 0f, 0.45f);
				if (__instance.occupations != null && __instance.leadOccupier != null && __instance.occupations.ContainsKey(__instance.leadOccupier)) {
					val *= (1-__instance.occupations[__instance.leadOccupier]);
				}
				float prevVal = value;
				val = ((25*value)-0.21f*Mathf.Pow(val, 1.2f))/25f;
				// val will be negative on both sides + and -
				// But this is the amount of damage reduced by Clandestine Ops, so we need to flip it if ___value was less than 0
				if (value > 0) {
					val *= -1f;
				}
				value -= val;

				if (value > 0) {
					Main.resistanceRegionArms[__instance.displayName] -= Mathf.Pow(1+value-(value-val), 8f)/4;	
					if (Main.resistanceRegionArms[__instance.displayName] <= 0) {
						Main.resistanceRegionArms.Remove(__instance.displayName);
					}
				}
			}
			return true;
		}
	}

    public class TIMissionEffect_EscapeFundSpaceProgram : TIMissionEffect {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
			FileLog.Log("EA1");
			StringBuilder builder = new StringBuilder("");
			if (mission.ref_nation.spaceFlightProgram) {
				// builder.AppendLine(Loc.T("TIMissionEffect_EscapeFundSpaceProgram.Special0", new object[] { }));
				builder.AppendLine("\nOur efforts in funding the development of a Space Program have... been reprioritised elsewhere. Appearently "+mission.ref_nation.displayNameWithArticle+" already *has* a Space Program. They just didn't think to tell us... ");
				return builder.ToString();
			}
			
			FileLog.Log("EA2");
			if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				FileLog.Log("EB3");
				float modifyValue = mission.councilor.GetAttribute(CouncilorAttribute.Science) / 5;
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.KnowledgeSector);
				
				float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
				FileLog.Log("EB4");

				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifyValue += 5f;
				}
				else if (refCP != null) {
					modifyValue -= 2f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifyValue += 5f;
				}
				else if (refCP != null)  {
					modifyValue -= 2f;
				}
				modifyValue += 10f * (-0.15f + mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology));
				modifyValue += friendlyCPs/4f;
				modifyValue *= (1f + friendlyCPs)/(1f + mission.ref_nation.numControlPoints);
				
				if (outcome == TIMissionOutcome.CriticalSuccess) {
					modifyValue = Math.Max(modifyValue + 0.5f, modifyValue * 2f);
				}
				FileLog.Log("EB5");
				
				float refVal = modifyValue + mission.ref_nation.GetAccumulatedInvestmentPoints(PriorityType.SpaceflightProgram);
				mission.ref_nation.SetAccumulatedInvestmentPoints(PriorityType.SpaceflightProgram, refVal, true);
				// builder.AppendLine(Loc.T("TIMissionEffect_EscapeFundSpaceProgram.Special1", new object[]
				// 	{
				// 		(refVal - modifyValue).ToString("0.###"),
				// 		(refVal).ToString("0.###")
				// 	}));
				builder.AppendLine("\nWe've made substantial headway in establishing a Space Program in "+mission.ref_nation.displayNameWithArticle+". We've increased investment in the Program by "+TemplateManager.global.investmentInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(modifyValue, 0)+" to a total of "+TemplateManager.global.investmentInlineSpritePath+refVal.ToString("0.###")+". ");

			} else {
				FileLog.Log("EC3");
				if (outcome == TIMissionOutcome.CriticalFailure) {
					float prevFunding = mission.ref_nation.spaceFundingIncome_year;
					float value = mission.ref_nation.spaceFundingPriorityIncomeChange * - UnityEngine.Random.Range(0f, 30f)/10;
					mission.ref_nation.ChangeAnnualSpaceFundingValue(value);
					FileLog.Log("EC4");
					// builder.AppendLine(Loc.T("TIMissionEffect_EscapeFundSpaceProgram.Special2", new object[]
					// {
					// 	(mission.ref_nation.spaceFundingIncome_year-prevFunding).ToString("0.###"),
					// 	(mission.ref_nation.spaceFundingIncome_year).ToString("0.###")
					// }));
					builder.AppendLine("\nThe local government has slashed our annual funding by "+TemplateManager.global.moneyInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.spaceFundingIncome_year-prevFunding, 3)+" to a value of "+TemplateManager.global.moneyInlineSpritePath+(mission.ref_nation.spaceFundingIncome_year).ToString("0.###")+" annually. ");
				}
			}
			FileLog.Log("EA4");
			return builder.ToString();
        }
    }

	public class TIMissionEffect_EscapeExpandSpaceAgency : TIMissionEffect {
		public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success) {
			StringBuilder builder = new StringBuilder();
			if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				float modifyValue = 1f;
				modifyValue += mission.councilor.GetAttribute(CouncilorAttribute.Administration) / 20f + mission.councilor.GetAttribute(CouncilorAttribute.Science) / 20f;
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Legislature);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifyValue += 1.5f * mission.ref_nation.democracy / 10f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifyValue += 10f / mission.ref_nation.democracy;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.NationalIndustries);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifyValue += 0.5f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Bureaucracy);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifyValue += 0.5f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifyValue += 0.5f;
				}
				float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
				modifyValue += friendlyCPs/4f;
				modifyValue *= (1f + friendlyCPs)/(1f + mission.ref_nation.numControlPoints);
				
				if (outcome == TIMissionOutcome.CriticalSuccess) {
					modifyValue = Math.Max(modifyValue + 0.5f, modifyValue * 2f);
				}
				Traverse traversObj = Traverse.Create(mission.ref_nation);
				float prevBoost = mission.ref_nation.currentBoost_year;
				float prevMC = mission.ref_nation.currentMissionControl;
				for (int index = 0; index < modifyValue; index++) {
					mission.ref_nation.BoostPriorityComplete();
					if (index % 2 == 1) {
						traversObj.Method("MissionControlPriorityComplete").GetValue();
					}
				}
				float nowBoost = mission.ref_nation.currentBoost_year;
				float nowMC = mission.ref_nation.currentMissionControl;
				// builder.AppendLine(Loc.T("TIMissionEffect_EscapeExpandSpaceAgency.Special0", new object[]
				// {
				// 	(nowBoost-prevBoost).ToString("0.###"),
				// 	(nowBoost).ToString("0.###"),
				// 	(nowMC-prevMC).ToString(),
				// 	(nowMC).ToString()
				// }));
				builder.AppendLine("\nThe resources we've managed to persuade the Government to allocate to their Space Program have increased "+mission.ref_nation.displayNameWithArticle+"'s Annual Boost by "+TemplateManager.global.boostInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(nowBoost - prevBoost, 0)+" to a value of "+TemplateManager.global.boostInlineSpritePath+(nowBoost).ToString("0.###")+", and Mission Control by "+TemplateManager.global.missionControlInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(nowMC - prevMC, 0)+" to "+TemplateManager.global.missionControlInlineSpritePath+(nowMC).ToString()+". ");
			}
			else {
				if (outcome == TIMissionOutcome.CriticalFailure) {
					float prevFunding = mission.ref_nation.spaceFundingIncome_year;
					mission.ref_nation.ChangeAnnualSpaceFundingValue(-1 * mission.ref_nation.spaceFundingPriorityIncomeChange * UnityEngine.Random.Range(10f,30f)/10f);
					float nowFunding = mission.ref_nation.spaceFundingIncome_year;
					// builder.AppendLine(Loc.T("TIMissionEffect_EscapeExpandSpaceAgency.Special1", new object[]
					// 	{
					// 		(nowFunding-prevFunding).ToString("0.###"),
					// 		(nowFunding).ToString("0.###")
					// 	}));
					builder.AppendLine("\nThe Government has decided to slash our annual funding by "+TemplateManager.global.moneyInlineSpritePath+(nowFunding-prevFunding).ToString("0.###")+" to a value of "+TemplateManager.global.moneyInlineSpritePath+(nowFunding).ToString("0.###")+" annually. ");
					if (mission.ref_nation.spaceFundingIncome_year <= 0f) {
						float opinionOutcome = mission.ref_nation.PropagandaOnPop(mission.ref_councilor.faction.ideology, (UnityEngine.Random.Range(0f,40f)/20f + mission.ref_councilor.GetAttribute(CouncilorAttribute.Persuasion)) / 20f - 5f);
						// builder.AppendLine(Loc.T("TIMissionEffect_EscapeExpandSpaceAgency.Special2", new object[]
						// {
						// 	opinionOutcome.ToPercent("P0"),
						// 	mission.ref_nation.GetPublicOpinionOfFaction(mission.councilor.faction).ToPercent("P0")
						// }));
						builder.Append("Public Outcry over wasted funds have caused "+mission.councilor.faction.displayNameCapitalizedWithColor+"'s support to change by "+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(opinionOutcome, 0, true)+" to a value of "+mission.ref_nation.GetPublicOpinionOfFaction(mission.councilor.faction).ToPercent("P0")+". ");
					}
				}
			}
			return builder.ToString();
		}
	}

    public class TIMissionEffect_ExploitIgnoreEcologicalProtections : TIMissionEffect
    {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
			StringBuilder builder = new StringBuilder("");
			if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				float numSpoilRuns = 0.25f + mission.councilor.GetAttribute(CouncilorAttribute.Persuasion) / 20 + mission.councilor.GetAttribute(CouncilorAttribute.Administration) / 20;

				float sustainChange = mission.ref_nation.spoilsSustainabilityChange;
				float inequalityChange = mission.ref_nation.spoilsPriorityInequalityChange;
				float govChange = mission.ref_nation.spoilsPriorityDemocracyChange;
				// float prevSupport = mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology);
				float moneyChange = mission.ref_nation.spoilsPriorityMoney;
				float supportStrengthChange = 0f;
				int ownCPs = mission.ref_nation.CountFactionControlPoints(mission.councilor.faction, false, false, true);

				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Oligarchs);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					numSpoilRuns += 0.5f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Aristocracy);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					numSpoilRuns += 0.5f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.FinancialSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					numSpoilRuns += 0.5f;
					supportStrengthChange += 0.25f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Corporations);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					numSpoilRuns += 0.5f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Legislature);
				if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
					numSpoilRuns -= 0.5f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.KnowledgeSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					supportStrengthChange += 0.5f;
				}
				else if (refCP != null) {
					supportStrengthChange -= 1f;
				}
				// If mission.councilor.faction does not possess the Legislature, they will find difficulty in enacting the effects of this mission

				numSpoilRuns *= ownCPs/mission.ref_nation.numControlPoints;

				if (outcome == TIMissionOutcome.CriticalSuccess) {
					numSpoilRuns = Math.Max(numSpoilRuns + 0.5f, numSpoilRuns * 2f);
				}
				supportStrengthChange += numSpoilRuns / 10f;
				supportStrengthChange += (-((mission.ref_nation.education) * (float)((1-ownCPs) / mission.ref_nation.numControlPoints)) / 4f) * (float) factionMissions.utilityFunctions.UtilityModule.ideologicalDistance(mission.ref_nation, mission.councilor.faction) - 1.5f;
				float prevIneq = mission.ref_nation.inequality;
				float prevGov = mission.ref_nation.democracy;
				float prevSustain = mission.ref_nation.sustainability;
				mission.ref_nation.AddToInequality(0.75f * numSpoilRuns * inequalityChange, TINationState.InequalityChangeReason.SpoilsPriority);
				mission.ref_nation.AddToSustainability(-1 * (50-Math.Min(mission.councilor.GetAttribute(CouncilorAttribute.Science), 25))/25 * numSpoilRuns * sustainChange * 2);
				mission.ref_nation.AddToDemocracy(0.75f * numSpoilRuns * govChange);
				float opinionChange = 0f;
				if (outcome == TIMissionOutcome.CriticalSuccess && supportStrengthChange > 0) {
					opinionChange = mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, 2f * supportStrengthChange);
				}
				else if (outcome == TIMissionOutcome.Success) {
					opinionChange = mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, supportStrengthChange);
				}

				float moneyGain = mission.councilor.faction.AddToCurrentResource(moneyChange * numSpoilRuns, FactionResource.Money);
				
				builder.AppendLine("\nThanks to our Lobbyists and Industry Leaders in "+mission.ref_nation.displayNameWithArticle+", we've managed to tidy a profit of "+TIUtilities.PathResourceIcon(FactionResource.Money)+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(moneyGain, 0)+". As a result of our actions, the following has occured:");
				builder.AppendLine("-Inequality has changed by "+TemplateManager.global.inequalityInlineSpritePath+"("+factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(mission.ref_nation.inequality - prevIneq)+") to a value of "+TemplateManager.global.inequalityInlineSpritePath+"("+mission.ref_nation.inequality+")");
				builder.AppendLine("-Government Score has changed by "+TemplateManager.global.democracyInlineSpritePath+"("+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.democracy - prevGov)+") to a value of "+TemplateManager.global.democracyInlineSpritePath+"("+mission.ref_nation.democracy+")");
				builder.AppendLine("-Sustainability has changed by "+TemplateManager.global.ENV_InlineSpritePath+"("+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.sustainability - prevSustain, 0)+") to a value of "+TemplateManager.global.ENV_InlineSpritePath+"("+mission.ref_nation.sustainability+")");
				if (opinionChange != 0) {
					builder.Append("-Public Opinion has changed by [");
					builder.Append(factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(opinionChange, 2, true));
					builder.AppendLine("] to ["+mission.ref_nation.GetPublicOpinionOfFaction(mission.councilor.faction).ToString("P0")+"]");
				}
				
				// float missionSupportStrength = 0f;
				// float currentStrengthFactor = 4f;
				// while (Math.Abs(prevSupport - mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology)) > 0.001) {
				// 	bool flipVal = mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology) > prevSupport;
				// 	mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, currentStrengthFactor);
				// 	if (mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology) > prevSupport != flipVal) {
				// 		currentStrengthFactor /= -2;
				// 	}
				// 	else {
				// 		missionSupportStrength += currentStrengthFactor;
				// 	}
				// }
				
				// negative missionSupportStrength is now the public opinion strength malus Spoils Priority Suffered


			}
			else {
				if (outcome == TIMissionOutcome.CriticalFailure) {
					float numRuns = 0.50f;

					TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						numRuns += 0.50f;
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.TheParty);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						numRuns += 0.25f;
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Religion);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						numRuns += 0.75f;
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.MassMedia);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						numRuns += 0.75f;
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Legislature);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						numRuns += 0.5f;
					}

					numRuns -= mission.councilor.GetAttribute(CouncilorAttribute.Persuasion) / 50;
					float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
					numRuns -= friendlyCPs/4f;
					numRuns *= 1-(1+friendlyCPs)/(2 * mission.ref_nation.numControlPoints+1);

					float numLostMoney = numRuns * mission.ref_nation.spoilsPriorityMoney * 0.5f;
					double ideologicalDistance = factionMissions.utilityFunctions.UtilityModule.ideologicalDistance(mission.ref_nation.GetMeanPublicOpinionVector(), mission.councilor.faction);
					float numLostInfluence = numRuns * (float) ideologicalDistance * 20f;
					float trueLoss = numLostInfluence;
					float currentInfluence = mission.councilor.faction.resources[FactionResource.Influence];
					float currentMoney = mission.councilor.faction.resources[FactionResource.Money];
					float actualLostMoney = numLostMoney;
					if (numLostMoney > currentMoney) {
						numLostInfluence += 0.05f * (numLostMoney-currentMoney);
						actualLostMoney = currentMoney;
					}
					float opinionOutcome = 0f;
					bool lossFlag = false;
					if (numLostInfluence > currentInfluence) {
						trueLoss = currentInfluence;
						lossFlag = true;
						opinionOutcome = mission.ref_nation.PropagandaOnPop(mission.councilor.faction.ideology, (trueLoss - numLostInfluence) / -10);
					}
					
					mission.councilor.faction.AddToCurrentResource(-1 * trueLoss, FactionResource.Influence);
					mission.councilor.faction.AddToCurrentResource(-1 * actualLostMoney, FactionResource.Money);
					builder.AppendLine("The Local Government has managed to prevent much of our efforts from going through, we've lost "+TIUtilities.InlineResourceStr(FactionResource.Influence)+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(trueLoss, 0)+" and "+TIUtilities.InlineResourceStr(FactionResource.Money)+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(actualLostMoney, 0)+" in this endeavour. ");
					if (lossFlag) {
						builder.Append("Additionally-- we've lost what sway we had over the local populace, with Opinion changing by ["+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(opinionOutcome, 0, true)+"] to a value of ["+mission.ref_nation.GetPublicOpinionProportion(mission.councilor.faction.ideology.ideology).ToString("P0")+"]");
					}
				}
			}

            return builder.ToString();
        }
    }

    public class TIMissionEffect_StudyEducatePopulace : TIMissionEffect
    {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
			StringBuilder builder = new StringBuilder("");
			if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				float modifierRun = 1f + mission.councilor.GetAttribute(CouncilorAttribute.Science) / 5f;
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.KnowledgeSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifierRun += 1f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Legislature);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifierRun += 0.25f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifierRun += 0.25f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifierRun += 0.5f;
				}

				float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
				modifierRun += friendlyCPs/4f;
				modifierRun *= (1+friendlyCPs)/(mission.ref_nation.numControlPoints+1);

				if (outcome == TIMissionOutcome.CriticalSuccess) {
					modifierRun = Math.Max(modifierRun + 0.5f, modifierRun * 2f);
				}
				
				float researchFromEducationModifier = (mission.ref_nation.knowledgePriorityEducationChange * 30 + mission.ref_nation.education)/mission.ref_nation.education;
				float researchIncreasse = researchFromEducationModifier * 100f * modifierRun * mission.councilor.faction.GetAggregateStat(CouncilorAttribute.Science, false) / 20f;
				float influenceIncrease = researchIncreasse / 5f;
				float prevGov = mission.ref_nation.democracy;
				float prevCohesion = mission.ref_nation.cohesion;
				float prevEducation = mission.ref_nation.education;
				for (int index = 0; index < modifierRun; index++) {
					mission.ref_nation.KnowledgePriorityComplete();
				}
				builder.Append("\nWe've gained "+TIUtilities.InlineResourceStr(FactionResource.Research)+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(researchIncreasse, 0)+" and "+TIUtilities.InlineResourceStr(FactionResource.Influence)+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(influenceIncrease, 0));
				builder.Append("Additionally, the Government Score of "+mission.ref_nation.displayNameWithArticle+" has changed by "+TemplateManager.global.democracyInlineSpritePath+(mission.ref_nation.democracy-prevGov).ToString("0.###")+" to a value of "+TemplateManager.global.democracyInlineSpritePath+mission.ref_nation.democracy.ToString("0.###")+". ");
				builder.Append("Additionally, the Knowledge Level of "+mission.ref_nation.displayNameWithArticle+" has changed by "+TemplateManager.global.educationInlineSpritePath+(mission.ref_nation.education-prevEducation).ToString("0.###")+" to a value of "+TemplateManager.global.educationInlineSpritePath+mission.ref_nation.education.ToString("0.###")+". ");
				builder.Append("Additionally, the Cohesion of "+mission.ref_nation.displayNameWithArticle+" has changed by "+TemplateManager.global.cohesionInlineSpritePath+(mission.ref_nation.cohesion-prevCohesion).ToString("0.###")+" to a value of "+TemplateManager.global.cohesionInlineSpritePath+mission.ref_nation.cohesion.ToString("0.###")+". ");
			}
			else {
				if (outcome == TIMissionOutcome.CriticalFailure) {
					
					TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.KnowledgeSector);
					float modifierRun = -1.5f;
					if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
						modifierRun += 0.95f;
					}
					else if (refCP != null) {
						modifierRun -= 2f;
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
					if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
						modifierRun += 0.45f;
					}
					else if (refCP != null) {
						modifierRun -= 1f;
					}

					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Legislature);
					if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
						modifierRun += 0.2f;
					}
					else if (refCP != null) {
						modifierRun -= 0.4f;
					}
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
					if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
						modifierRun += 0.1f;
					}
					else if (refCP != null) {
						modifierRun -= 0.2f;
					}

					float hostileCPs = 0f;
					for (int index = 0; index < mission.ref_nation.numControlPoints; index++) {
						TIFactionState refFact = mission.ref_nation.GetControlPoint(index).faction;
						if (refFact == null || !(refFact == mission.councilor.faction || (Main.settings.friendlyFPFlag && refFact.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")))) {
							hostileCPs += 1;
						}
					}
					modifierRun *= (1f + hostileCPs/mission.ref_nation.numControlPoints);
					float researchLoss = (float) modifierRun * (mission.ref_nation.perCapitaGDP) / 500f * (Math.Max(0.1f, mission.ref_nation.education - mission.councilor.faction.GetAggregateStat(CouncilorAttribute.Science, false) / 40f));
					float prevEdu = mission.ref_nation.education;
					float eduLoss = mission.ref_nation.knowledgePriorityEducationChange * (Math.Max(0.1f, modifierRun + mission.councilor.faction.GetAggregateStat(CouncilorAttribute.Science, false) / 40f));
					mission.ref_nation.AddToEducation(eduLoss, TINationState.EducationChangeReason.Effect);
										
					builder.AppendLine("We've lost "+TemplateManager.global.scienceInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(researchLoss, 3)+" as a result of our failures in "+mission.ref_nation.displayName+". Additionally, the Eudcation Score of "+mission.ref_nation.displayName+" has changed by "+TemplateManager.global.knowledgePriorityEducationIncrease+"("+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.education-prevEdu, 3)+") to a value of "+TemplateManager.global.knowledgePriorityEducationIncrease+"("+(prevEdu-mission.ref_nation.education).ToString("0.###")+"). ");

				}
			}

            return builder.ToString();
        }
    }

	// Success = Help Allies Research; Failure = Detriment to Allies
    public class TIMissionEffect_StudyShareResearch : TIMissionEffect
    {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
            StringBuilder builder = new StringBuilder("");
			if ((outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) || (outcome == TIMissionOutcome.Failure || outcome == TIMissionOutcome.CriticalFailure)) {
				float modifier = 0.25f + mission.councilor.GetAttribute(CouncilorAttribute.Science) / 10 + mission.councilor.GetAttribute(CouncilorAttribute.Administration) / 20;
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.KnowledgeSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.5f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.25f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Bureaucracy);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.10f;
				}
				float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
				modifier += friendlyCPs/4f;
				modifier *= (1+friendlyCPs)/(mission.ref_nation.numControlPoints+1);
				modifier *= (mission.ref_nation.education/7f);
				modifier *= (mission.ref_nation.perCapitaGDP/60000f);
				// builder.Append(TIUtilities.GetTemplateValue(line));

				builder.Append("\n----------------------------------------");
				foreach (TIFactionState faction in mission.ref_nation.FactionsWithControlPoint) {
					float numOwnCPs = mission.ref_nation.FactionControlPoints(faction, false, false, true).Count;
					float amount = 60f * (modifier * numOwnCPs/mission.ref_nation.numControlPoints);
					builder.Append("\n\t"+faction.factionIcon64UI+faction.displayNameCapitalizedWithColor+" has recieved "+TIUtilities.InlineResourceStr(FactionResource.Research));
					if (outcome == TIMissionOutcome.Failure || outcome == TIMissionOutcome.CriticalFailure) {
						amount *= -1f * (2f/3f);
						if (faction != mission.ref_faction) {
							if (mission.ref_faction.GetDiplomacyMood(faction).Equals("Tolerance")) {
								amount *= (outcome == TIMissionOutcome.Failure)? 0.75f : 1.25f;
								builder.Append(factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(amount));
							}
							else if (mission.ref_faction.GetDiplomacyMood(faction).Equals("Conflicted")) {
								amount *= (outcome == TIMissionOutcome.Failure)? 0.40f : 0.25f;
								builder.Append(factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(amount));
							}
							else if (mission.ref_faction.GetDiplomacyMood(faction).Equals("War")) {
								amount *= (outcome == TIMissionOutcome.Failure)? 0.25f : 0.10f;
								builder.Append(factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(amount));
							}						
						}
						else {
							amount *= (outcome == TIMissionOutcome.Failure)? 0.5f : 0.6f;
							builder.Append(factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(amount));
						}
					}
					else {
						if (faction != mission.ref_faction) {
							if (mission.ref_faction.GetDiplomacyMood(faction).Equals("Tolerance")) {
								amount *= (outcome == TIMissionOutcome.Success)? 0.75f : 1.25f;
								builder.Append(factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(amount));
							}
							else if (mission.ref_faction.GetDiplomacyMood(faction).Equals("Conflicted")) {
								amount *= (outcome == TIMissionOutcome.Success)? 0.40f : 0.25f;
								builder.Append(factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(amount));
							}
							else if (mission.ref_faction.GetDiplomacyMood(faction).Equals("War")) {
								amount *= (outcome == TIMissionOutcome.Success)? 0.25f : 0.10f;
								builder.Append(factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(amount));
							}						
						}
						else {
							amount *= (outcome == TIMissionOutcome.Success)? 0.5f : 0.60f;
							builder.Append(factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(amount));
						}
					}
					builder.Append(" from our mission to "+mission.ref_region.displayName+". ");
					faction.AddToCurrentResource(amount, FactionResource.Research);
				}
				if (outcome == TIMissionOutcome.CriticalSuccess) {
					float prevEdu = mission.ref_nation.education;
					float prevCohesion = mission.ref_nation.cohesion;
					float prevGov = mission.ref_nation.democracy;
					mission.ref_nation.AddToEducation(modifier * mission.ref_nation.knowledgePriorityEducationChange, TINationState.EducationChangeReason.Effect);
					mission.ref_nation.AddToCohesion(modifier * mission.ref_nation.knowledgePriorityCohesionChange);
					mission.ref_nation.GovernmentPriorityComplete();
					builder.Append("\n----------------------------------------");
					builder.Append("\n"+mission.ref_nation.displayName+" has experienced the following changes as a result:");
					builder.Append("\n\t-Education has changed by "+TemplateManager.global.educationInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.education - prevEdu) +" to a value of "+TemplateManager.global.educationInlineSpritePath+mission.ref_nation.education);
					builder.Append("\n\t-Cohesion has changed by "+TemplateManager.global.cohesionInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.cohesion - prevCohesion) +" to a value of "+TemplateManager.global.cohesionInlineSpritePath+mission.ref_nation.cohesion);
					builder.Append("\n\t-Government has changed by "+TemplateManager.global.democracyInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.democracy - prevGov) +" to a value of "+TemplateManager.global.democracyInlineSpritePath+mission.ref_nation.democracy);
					foreach (TINationState neighbourNation in mission.ref_nation.AdjacentNations(false)) {
						prevEdu = neighbourNation.education;
						prevCohesion = neighbourNation.cohesion;
						prevGov = neighbourNation.democracy;
						neighbourNation.AddToEducation(modifier * neighbourNation.knowledgePriorityEducationChange, TINationState.EducationChangeReason.Effect);
						neighbourNation.AddToCohesion(modifier * neighbourNation.knowledgePriorityCohesionChange);
						neighbourNation.GovernmentPriorityComplete();
						builder.Append("\n"+neighbourNation.displayName+" has experienced the following changes as a result:");
						builder.Append("\n\t-Education has changed by "+TemplateManager.global.educationInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(neighbourNation.education - prevEdu) +" to a value of "+TemplateManager.global.educationInlineSpritePath+neighbourNation.education);
						builder.Append("\n\t-Cohesion has changed by "+TemplateManager.global.cohesionInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(neighbourNation.cohesion - prevCohesion) +" to a value of "+TemplateManager.global.cohesionInlineSpritePath+neighbourNation.cohesion);
						builder.Append("\n\t-Government has changed by "+TemplateManager.global.democracyInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(neighbourNation.democracy - prevGov) +" to a value of "+TemplateManager.global.democracyInlineSpritePath+neighbourNation.democracy);
					}
				}
			}
			return builder.ToString();
        }
    }

	// Success = Boost OWN research; Triggers KnowledgeInvestmentCompletion for Allied Nations; Failure Boosts Enemies and Rivals.
	public class TIMissionEffect_StudyTechSummit : TIMissionEffect
    {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
            StringBuilder builder = new StringBuilder("");
			if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				float modifier = 0.25f + mission.councilor.GetAttribute(CouncilorAttribute.Science) / 10 + mission.councilor.GetAttribute(CouncilorAttribute.Administration) / 20;
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.KnowledgeSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.5f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.25f;
				}

				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Bureaucracy);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.10f;
				}
				float friendlyCPs = factionMissions.utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.councilor.faction);
				modifier += friendlyCPs/4f;
				modifier *= (1+friendlyCPs)/(mission.ref_nation.numControlPoints+1);
				modifier *= (mission.ref_nation.education/7f);
				modifier *= (mission.ref_nation.perCapitaGDP/60000f);
				builder.Append("\n----------------------------------------");
				foreach (TIFactionState faction in mission.ref_nation.FactionsWithControlPoint) {
					builder.Append("\n\t"+faction.factionIcon64UI+faction.displayNameCapitalizedWithColor+" has recieved "+TIUtilities.InlineResourceStr(FactionResource.Research));
					float numOwnCPs = mission.ref_nation.FactionControlPoints(faction, false, false, true).Count;
					float amount = 60f * (modifier * numOwnCPs/mission.ref_nation.numControlPoints);
					if (faction != mission.ref_faction) {
						if (mission.ref_faction.GetDiplomacyMood(faction).Equals("Tolerance")) {
							amount *= (outcome == TIMissionOutcome.Success)? 0.50f : 0.60f;
							builder.Append(factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(amount));
						}
						else if (mission.ref_faction.GetDiplomacyMood(faction).Equals("Conflicted")) {
							amount *= (outcome == TIMissionOutcome.Success)? 0.40f : 0.25f;
							builder.Append(factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(amount));
						}
						else if (mission.ref_faction.GetDiplomacyMood(faction).Equals("War")) {
							amount *= (outcome == TIMissionOutcome.Success)? 0.25f : 0.10f;
							builder.Append(factionMissions.utilityFunctions.UtilityModule.colourNegativeGood(amount));
						}						
					}
					else {
						amount *= (outcome == TIMissionOutcome.Success)? 0.75f : 1.25f;
						builder.Append(factionMissions.utilityFunctions.UtilityModule.colourPositiveGood(amount));
					}
					builder.Append(" .");
				}
				
				float prevEdu = mission.ref_nation.education;
				float prevCohesion = mission.ref_nation.cohesion;
				float prevGov = mission.ref_nation.democracy;
				mission.ref_nation.AddToEducation(modifier * mission.ref_nation.knowledgePriorityEducationChange, TINationState.EducationChangeReason.Effect);
				mission.ref_nation.AddToCohesion(modifier * mission.ref_nation.knowledgePriorityCohesionChange);
				mission.ref_nation.GovernmentPriorityComplete();
				builder.Append("\n----------------------------------------");
				builder.Append("\n"+mission.ref_nation.displayName+" has experienced the following changes as a result:");
				builder.Append("\n\t-Education has changed by "+TemplateManager.global.educationInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.education - prevEdu) +" to a value of "+TemplateManager.global.educationInlineSpritePath+mission.ref_nation.education);
				builder.Append("\n\t-Cohesion has changed by "+TemplateManager.global.cohesionInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.cohesion - prevCohesion) +" to a value of "+TemplateManager.global.cohesionInlineSpritePath+mission.ref_nation.cohesion);
				builder.Append("\n\t-Government has changed by "+TemplateManager.global.democracyInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(mission.ref_nation.democracy - prevGov) +" to a value of "+TemplateManager.global.democracyInlineSpritePath+mission.ref_nation.democracy);
				foreach (TINationState allyNation in mission.ref_nation.allies) {
					bool continueOn = true;
					if (outcome != TIMissionOutcome.CriticalSuccess) {
						if (UnityEngine.Random.Range(0f,100f) > modifier * 30) {
							// Makes so that ~40% of allyNations don't get the boost. This % decreases the higher modifier is
							continueOn = false;
						}
					}
					if (continueOn) {
						prevEdu = allyNation.education;
						prevCohesion = allyNation.cohesion;
						prevGov = allyNation.democracy;
						allyNation.AddToEducation(modifier * allyNation.knowledgePriorityEducationChange, TINationState.EducationChangeReason.Effect);
						allyNation.AddToCohesion(modifier * allyNation.knowledgePriorityCohesionChange);
						allyNation.GovernmentPriorityComplete();
						builder.Append("\n"+allyNation.displayName+" has experienced the following changes as a result:");
						builder.Append("\n\t-Education has changed by "+TemplateManager.global.educationInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(allyNation.education - prevEdu) +" to a value of "+TemplateManager.global.educationInlineSpritePath+allyNation.education);
						builder.Append("\n\t-Cohesion has changed by "+TemplateManager.global.cohesionInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(allyNation.cohesion - prevCohesion) +" to a value of "+TemplateManager.global.cohesionInlineSpritePath+allyNation.cohesion);
						builder.Append("\n\t-Government has changed by "+TemplateManager.global.democracyInlineSpritePath+utilityFunctions.UtilityModule.colourPositiveGood(allyNation.democracy - prevGov) +" to a value of "+TemplateManager.global.democracyInlineSpritePath+allyNation.democracy);
					}
				}
			}
			else if (outcome == TIMissionOutcome.Failure) {
				if (outcome == TIMissionOutcome.CriticalFailure) {
					float friendlyCPs = utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.ref_councilor.faction);
					float hostileCPs = mission.ref_nation.numControlPoints - friendlyCPs;
					float modifier = 0.5f;
					TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.KnowledgeSector);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						modifier += 1.5f;
					}
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Bureaucracy);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						modifier += 0.75f;
					}
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Executive);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						modifier += 0.5f;
					}
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.DefenseSector);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						modifier += 0.25f;
					}
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Warlords);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						modifier += 0.25f;
					}
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.SecurityApparatus);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						modifier += 0.15f;
					}
					refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.RegionalAuthorities);
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, mission)) {
						modifier += 0.15f;
					}
					modifier += hostileCPs/4f;
					modifier *= (1+hostileCPs)/(mission.ref_nation.numControlPoints + 1);
					modifier += -1 * mission.ref_councilor.GetAttribute(CouncilorAttribute.Security) / 10f;
					foreach (TIFactionState faction in utilityFunctions.UtilityModule.presentHostiles(mission.ref_nation, mission.ref_councilor.faction)) {
						modifier += (mission.ref_nation.CountFactionControlPoints(faction, false, false, true) / mission.ref_nation.numControlPoints) * TIMissionModifier.CouncilCollectiveDefense(faction, CouncilorAttribute.Espionage) / 15f / 4f;
					}
					if (modifier > 0f) {
						foreach (TINationState rivalNation in mission.ref_nation.rivals) {
							bool continueOn = true;
							if (outcome != TIMissionOutcome.CriticalSuccess) {
								if (UnityEngine.Random.Range(0f,100f) > modifier * 30) {
									continueOn = false;
								}
							}
							if (continueOn) {
								float prevEdu = rivalNation.education;
								float prevCohesion = rivalNation.cohesion;
								float prevGov = rivalNation.democracy;
								rivalNation.AddToEducation(modifier * rivalNation.knowledgePriorityEducationChange, TINationState.EducationChangeReason.Effect);
								rivalNation.AddToCohesion(modifier * rivalNation.knowledgePriorityCohesionChange);
								rivalNation.GovernmentPriorityComplete();
								builder.Append("\n"+rivalNation.displayName+" has experienced the following changes as a result:");
								builder.Append("\n\t-Education has changed by "+TemplateManager.global.educationInlineSpritePath+utilityFunctions.UtilityModule.colourNegativeGood(rivalNation.education - prevEdu) +" to a value of "+TemplateManager.global.educationInlineSpritePath+rivalNation.education);
								builder.Append("\n\t-Cohesion has changed by "+TemplateManager.global.cohesionInlineSpritePath+utilityFunctions.UtilityModule.colourNegativeGood(rivalNation.cohesion - prevCohesion) +" to a value of "+TemplateManager.global.cohesionInlineSpritePath+rivalNation.cohesion);
								builder.Append("\n\t-Government has changed by "+TemplateManager.global.democracyInlineSpritePath+utilityFunctions.UtilityModule.colourNegativeGood(rivalNation.democracy - prevGov) +" to a value of "+TemplateManager.global.democracyInlineSpritePath+rivalNation.democracy);
							}
						}
					}
				}
			}
			return builder.ToString();
        }
    }

	
	// Think of this as the INSPIRE mission, with the caveat that it can only be used on Councillors not owned by you.
	// Success = Decrease target-councillors' loyalty; (Increases loyalty for allies); Critical Failure = Increase target-councillors' loyalty.
	public class TIMissionEffect_ServeProselytiseCouncillors : TIMissionEffect
    {
        public override string ApplyEffect(TIMissionState mission, TIGameState target, TIMissionOutcome outcome = TIMissionOutcome.Success)
        {
            StringBuilder builder = new StringBuilder("");
			if (outcome == TIMissionOutcome.Success || outcome == TIMissionOutcome.CriticalSuccess) {
				float minVal = 1f;
				float maxVal = 3f;
				if (outcome == TIMissionOutcome.CriticalSuccess) {
					minVal += 1f;
					maxVal += 2f;
				}
				float loyaltyChange = (int) UnityEngine.Random.Range(minVal, maxVal);
				float modifier = 0.50f + mission.councilor.GetAttribute(CouncilorAttribute.Persuasion) / 10;
				TIControlPoint refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.MassMedia);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 0.5f;
				}
				refCP = mission.ref_nation.GetControlPointOfType(ControlPointType.Religion);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, mission)) {
					modifier += 1.0f;
				}
				float friendlyCPs = utilityFunctions.UtilityModule.getNumFriendlyCps(mission.ref_nation, mission.ref_councilor.faction);
				modifier += friendlyCPs/4f;
				modifier *= (1f + friendlyCPs)/(1f + mission.ref_nation.numControlPoints);
				// Max modifier can reach is 5 in a normal game, which equates to a max of 6-8 (Normal Success), or 7-10 (Crits)
				loyaltyChange += modifier;

				CouncilorView viewofCouncilor = mission.councilor.faction.GetViewofCouncilor(mission.ref_councilor);
				float attribute = viewofCouncilor.GetAttribute(CouncilorAttribute.ApparentLoyalty);
				int realChangeVal = (int) UnityEngine.Random.Range(Math.Max(0, modifier - 3f), modifier);
				int fakeChangeVal = (int) UnityEngine.Random.Range(Math.Max(0, modifier - 3f), modifier);
				target.ref_councilor.ModifyAttribute(CouncilorAttribute.Loyalty, realChangeVal);
				target.ref_councilor.ModifyAttribute(CouncilorAttribute.ApparentLoyalty, realChangeVal);
				if ((mission.ref_councilor.ref_faction != target.ref_councilor.ref_faction && mission.ref_councilor.faction.GetDiplomacyMood(target.ref_councilor.ref_faction).Equals("Tolerance")) && mission.ref_councilor.turned && UnityEngine.Random.Range(0f, 15f) < (float) mission.ref_councilor.GetAttribute(CouncilorAttribute.Loyalty, true, true, true, false))
				{
					mission.ref_councilor.UnTurnCouncilor(false, true);
				}

			}
			else if (outcome == TIMissionOutcome.CriticalFailure) {

			}
			return builder.ToString();
        }
    }
}
