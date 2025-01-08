using System;
using PavonisInteractive.TerraInvicta;

namespace factionMissions.MissionModifiers {

	public class TIMissionModifier_NationThreatened : TIMissionModifier
    {
        public override float GetModifier(TICouncilorState attackingCouncilor, TIGameState target = null, float resourcesSpent = 0, FactionResource resource = FactionResource.None)
        {
			// Uses the same power-curve that the NationPopulation modifier has
			if (target != null && target.ref_nation != null)
			{		
				return 2f * factionMissions.utilityFunctions.UtilityModule.warStrength(target.ref_nation);
			}
			return 0f;
        }
    }

    public class TIMissionModifier_InvertedNationPopulation : TIMissionModifier
    {
        public override float GetModifier(TICouncilorState attackingCouncilor, TIGameState target = null, float resourcesSpent = 0, FactionResource resource = FactionResource.None)
        {
			// Ranges from 30 to -30, 15 at ~25.7M, 0 at 90M people, -15 at 540M
			if (target != null && target.ref_nation != null)
			{
				return (target.ref_nation.population_Millions/(target.ref_nation.population_Millions + 60f) - 1f) * 50f + 20f;
			}
			return 0f;
        }
    }

    public class TIMissionModifier_RegionPopulation : TIMissionModifier
    {
        public override float GetModifier(TICouncilorState attackingCouncilor, TIGameState target = null, float resourcesSpent = 0, FactionResource resource = FactionResource.None)
        {
			// Uses the same power-curve that the NationPopulation modifier has
			if (target != null && target.ref_region != null)
			{
				return (float) Math.Pow(target.ref_region.populationInMillions, 0.4f);
			}
			return 0f;
        }
    }

    public class TIMissionModifier_ResistCellNetworkMinor : TIMissionModifier {
        public override float GetModifier(TICouncilorState attackingCouncilor, TIGameState target, float resourcesSpent = 0, FactionResource resource = FactionResource.None)
        {
			TIMissionModifier missionMod = new TIMissionModifier_ResistCellNetwork();
            return missionMod.GetModifier(attackingCouncilor, target, resourcesSpent, resource) / 2;
        }
    }

	public class TIMissionModifier_ResistCellNetwork : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			float nationMax = 0f;
			float regionMax = 0f;
			if (target != null && target.ref_nation != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				if (Main.settings.cellnetworksAllowed && Main.resistanceNationNetworkSize != null && Main.resistanceNationNetworkSize.ContainsKey(TINationStateVar.displayName)) {
					nationMax = Main.resistanceNationNetworkSize[TINationStateVar.displayName];
				}
				else if (Main.settings.cellnetworksAllowed && Main.resistanceRegionNetworkSize != null && Main.resistanceRegionNetworkSize.ContainsKey(TINationStateVar.displayName)) {
					regionMax = Main.resistanceRegionNetworkSize[TINationStateVar.displayName];
				}
			}
			return Math.Max(nationMax/2, regionMax);
		}
        // public new string displayName() {
		// 	return Loc.T("TIMissionModifier_VeryEasyMission", new object[] { });;
		// }
    }
	
	public class TIMissionModifier_VeryEasyMission : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			return 2f;
		}
        // public new string displayName() {
		// 	return Loc.T("TIMissionModifier_VeryEasyMission", new object[] { });;
		// }
    }

	public class TIMissionModifier_EasyMission : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			return 5f;
		}
        // public new string displayName() {
		// 	return Loc.T("TIMissionModifier_EasyMission", new object[] { });;
		// }
	}

	public class TIMissionModifier_MediumMission : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			return 8f;
		}
        // public new string displayName() {
		// 	return Loc.T("TIMissionModifier_MediumMission", new object[] { });;
		// }
	}

	public class TIMissionModifier_ModerateMission : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			return 15f;
		}
        // public new string displayName() {
		// 	return Loc.T("TIMissionModifier_ModerateMission", new object[] { });;
		// }
	}

	public class TIMissionModifier_DifficultMission : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			// FileLog.Log("I was called1?");
			return 25f;
		}
        // public new string displayName() {
		// 	return Loc.T("TIMissionModifier_DifficultMission", new object[] { });;
		// }
	}

	public class TIMissionModifier_VeryDifficultMission : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			// FileLog.Log("I was called2?");
			return 35f;
		}
        // public new string displayName() {
		// 	return Loc.T("TIMissionModifier_VeryDifficultMission", new object[] { });;
		// }
	}

	public class TIMissionModifier_HardMission : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			return 50f;
		}
        // public new string displayName() {
		// 	return Loc.T("TIMissionModifier_HardMission", new object[] { });;
		// }
	}

	public class TIMissionModifier_hasKnowledgeSector : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.KnowledgeSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasKnowledgeSectorDEF : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.KnowledgeSector);
				if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, councilor)) {
					return 5f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasDefenceSector : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasDefenceSectorDEF : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.DefenseSector);
				if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, councilor)) {
					return 5f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasAristocracy : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.Aristocracy);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasCorporations : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.Corporations);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasFinancialSector : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.FinancialSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasAgriculturalSector : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.AgriculturalSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 6f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasExtractiveSector : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.ExtractiveSector);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasNationalIndustries : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.NationalIndustries);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasTradeUnions : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.TradeUnions);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasExecutiveCP : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.Executive);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 5f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasReligionCP : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.Religion);
				if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(refCP, councilor)) {
					return 5f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasSecurityApparatusDEF : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.SecurityApparatus);
				if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, councilor)) {
					return 6f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasWarlordsDEF : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.Warlords);
				if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_hasRegionalAuthoritiesDEF : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				TIControlPoint refCP = TINationStateVar.GetControlPointOfType(ControlPointType.RegionalAuthorities);
				if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(refCP, councilor)) {
					return 3f;
				}
			}
			return 0f;
		}
	}

    public class TIMissionModifier_CustomEcoModifier : TIMissionModifier
    {
        public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			TIMissionModifier capitaModifier = new TIMissionModifier_GDPCapitaModifier();
			TIMissionModifier gdpModifier = new TIMissionModifier_GDPModifier();
			return (capitaModifier.GetModifier(councilor, target, resourcesSpent, resource) + 2 * gdpModifier.GetModifier(councilor, target, resourcesSpent, resource));
        }
    }

    public class TIMissionModifier_GDPCapitaModifier : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				float perCapitaMod = UnityEngine.Mathf.Pow(TINationStateVar.perCapitaGDP/0.9f, 0.67f) / 60 - 30;
				// In a 'normal game'-- GDPMod will range from -30 (0PCGDP) to ~50 (281K PCGDP)-- reaching a value of 0 at ~65K PCGDP
				return perCapitaMod/2f; // Rescale Bounds to -15 and 15
			}			
			return 0f;
		}
	}

    public class TIMissionModifier_GDPModifier : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_nation != null && councilor != null) {
				TINationState TINationStateVar = TIUtilities.ObjectToNation(target, true);
				float gdpMod = (float) UnityEngine.Mathd.Pow(TINationStateVar.GDP / 3000, 0.62) / 1000000 - 30;
				// In a 'normal game'-- GDPMod will range from -30 (0B) to 127 (50,000B)-- reaching a value of 0 at 3,433B
				if (gdpMod > 0) {
					gdpMod = UnityEngine.Mathf.Pow(gdpMod, 0.7f); // To rescale the upper bound from 127 to 30
				}
				return gdpMod/2f; // Rescale Bounds to -15 and 15
			}			
			return 0f;
		}
	}

	public class TIMissionModifier_armyRegionDefencesDEF : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_region != null && councilor != null) {
				if (target.ref_region.IsOccupied()) {
					return (float)(target.ref_region.NumArmiesPresent(includeNations: false, includeAllies: false, includeEnemies: true, includeOnlyWarActiveAllies: false) - target.ref_region.NumFactionArmiesPresent(councilor.faction, includeNations: false, includeAllies: false, includeEnemies: true, includeMegafauna: false));
				}
				else {
					float num = (float)(target.ref_region.NumArmiesPresent(includeNations: true, includeAllies: false, includeEnemies: false, includeOnlyWarActiveAllies: false) - target.ref_region.NumFactionArmiesPresent(councilor.faction, includeNations: true, includeAllies: false, includeEnemies: false, includeMegafauna: true)) * target.ref_region.nation.militaryTechLevel;
					if (target.ref_region == target.ref_region.nation.capital) {
						if (target.ref_region.nation.executiveFaction != null && target.ref_region.nation.executiveFaction == councilor.faction) {
							num -= 2f;
						}
						else {
							num += 2f;
						}
					}

					if (target.ref_region.colonyRegion)
					{
						num -= 1f;
					}
					return num;
				}
			}
			return 0f;
		}
	}

	public class TIMissionModifier_armyRegionDefences : TIMissionModifier {
		public override float GetModifier(TICouncilorState councilor, TIGameState target, float resourcesSpent = 0f, FactionResource resource = FactionResource.None) {
			if (target != null && target.ref_region != null && councilor != null) {
				if (target.ref_region.IsOccupied()) {
					return (float)(target.ref_region.NumArmiesPresent(includeNations: false, includeAllies: false, includeEnemies: true, includeOnlyWarActiveAllies: false) - target.ref_region.NumFactionArmiesPresent(councilor.faction, includeNations: false, includeAllies: false, includeEnemies: true, includeMegafauna: false));
				}
				else {
					float num = (float) (target.ref_region.NumFactionArmiesPresent(councilor.faction, includeNations: true, includeAllies: false, includeEnemies: false, includeMegafauna: true) - target.ref_region.NumArmiesPresent(includeNations: true, includeAllies: false, includeEnemies: false, includeOnlyWarActiveAllies: false)) * target.ref_region.nation.militaryTechLevel;
					if (target.ref_region == target.ref_region.nation.capital) {
						if (target.ref_region.nation.executiveFaction != null && target.ref_region.nation.executiveFaction == councilor.faction) {
							num -= 2f;
						}
						else {
							num += 2f;
						}
					}

					if (target.ref_region.colonyRegion)
					{
						num -= 1f;
					}
					return num;
				}
			}
			return 0f;
		}
	}
}
