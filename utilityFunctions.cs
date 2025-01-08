using System;
using System.Text;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace factionMissions.utilityFunctions {
	public static class UtilityModule {
		// For reference: Servants to Humanity First is distance of 4.27200187
		// For reference: Servants to Resistance is distance of 3.16227766
		// For reference: Protectorate to Resistance is distance of 2.54950976
		// For reference: Protectorate to Humanity First is distance of 3.5
		// For reference: Academy to Resistance is distance of 1.80277564
		// For reference: Academy to Humanity First is distance of 2.91547595

		public static double ideologicalDistance(UnityEngine.Vector3 vector1, UnityEngine.Vector3 vector2) {
			return Math.Pow(Math.Pow(vector1.x - vector2.x, 2) + Math.Pow(vector1.y - vector2.y, 2) + Math.Pow(vector1.z - vector2.z, 2), 0.5);
		}

		public static double ideologicalDistance(UnityEngine.Vector3 vector1, TIFactionState faction) {
			UnityEngine.Vector3 factionVector = faction.ideologyCoordinates;
			return Math.Pow(Math.Pow(vector1.x - factionVector.x, 2) + Math.Pow(vector1.y - factionVector.y, 2) + Math.Pow(vector1.z - factionVector.z, 2), 0.5);
		}

		public static double ideologicalDistance(TINationState target, TIFactionState faction) {
			UnityEngine.Vector3 nationVector = target.GetMeanPublicOpinionVector();
			UnityEngine.Vector3 factionVector = faction.ideologyCoordinates;
			return Math.Pow(Math.Pow(nationVector.x - factionVector.x, 2) + Math.Pow(nationVector.y - factionVector.y, 2) + Math.Pow(nationVector.z - factionVector.z, 2), 0.5);
		}

		public static string colourPositiveGood(float value, int precision=3, bool isPercent=false) {
			string strValue = "";
			if (!isPercent) {
				if (precision == 0) {
					strValue = value.ToString();
				}
				else if (precision == 1) {
					strValue = value.ToString("0.#");
				}
				else if (precision == 2) {
					strValue = value.ToString("0.##");
				}
				else if (precision == 3) {
					strValue = value.ToString("0.###");
				}
				else if (precision == 4) {
					strValue = value.ToString("0.####");
				}
				else {
					value.ToString("0.###");
				}
			}
			else {
				if (precision == 0) {
					strValue = value.ToPercent("P0");
				}
				else if (precision == 1) {
					strValue = value.ToPercent("P1");
				}
				else if (precision == 2) {
					strValue = value.ToPercent("P2");
				}
				else if (precision == 3) {
					strValue = value.ToPercent("P3");
				}
				else if (precision == 4) {
					strValue = value.ToPercent("P4");
				}
				else {
					value.ToPercent("P0");
				}
			}

			if (value > 0f) {
				return TIUtilities.GreenLine(strValue);
			}
			else if (value == 0f) {
				return strValue;
			}
			else {
				return TIUtilities.RedLine(strValue);
			}
		}

		public static string colourNegativeGood(float value, int precision=3, bool isPercent=false) {
			string strValue = "";
			if (!isPercent) {
				if (precision == 0) {
					strValue = value.ToString();
				}
				else if (precision == 1) {
					strValue = value.ToString("0.#");
				}
				else if (precision == 2) {
					strValue = value.ToString("0.##");
				}
				else if (precision == 3) {
					strValue = value.ToString("0.###");
				}
				else if (precision == 4) {
					strValue = value.ToString("0.####");
				}
				else {
					value.ToString("0.###");
				}
			}
			else {
				if (precision == 0) {
					strValue = value.ToPercent("P0");
				}
				else if (precision == 1) {
					strValue = value.ToPercent("P1");
				}
				else if (precision == 2) {
					strValue = value.ToPercent("P2");
				}
				else if (precision == 3) {
					strValue = value.ToPercent("P3");
				}
				else if (precision == 4) {
					strValue = value.ToPercent("P4");
				}
				else {
					value.ToPercent("P0");
				}
			}

			if (value > 0f) {
				return TIUtilities.RedLine(strValue);
			}
			else if (value == 0f) {
				return strValue;
			}
			else {
				return TIUtilities.GreenLine(strValue);
			}
		}

		public static string resistanceNetworkIncrease(float cellNetworkSize, string refregionName, TINationState nation, TIRegionState region, float networkModifier=1f) {
			StringBuilder builder = new StringBuilder("");
			string nationName = nation.displayName;
			if (Main.resistanceCellNetworksMacro == null) {
				Main.resistanceCellNetworksMacro = new Dictionary<int, Dictionary<string, TINationState>>();
			}
			if (Main.resistanceRegionNetworkSize == null) {
				Main.resistanceRegionNetworkSize = new Dictionary<string, int>();
			}
			if (Main.resistanceCellNetworksMicro == null) {
				Main.resistanceCellNetworksMicro = new Dictionary<int, Dictionary<string, TIRegionState>>();
			}
			if (Main.resistanceNationNetworkSize == null) {
				Main.resistanceNationNetworkSize = new Dictionary<string, int>();
			}

			cellNetworkSize *= networkModifier;
			if (Main.settings.cellnetworksAllowed) {
				if (Main.resistanceRegionNetworkSize.ContainsKey(refregionName)) {
					Main.resistanceRegionNetworkSize[refregionName] += (int) cellNetworkSize;
					builder.AppendLine("\nThe size of our Cell Network in "+refregionName+" has "+TIUtilities.GreenLine("increased")+" by ["+Main.operativeInlineSpritePath+factionMissions.utilityFunctions.UtilityModule.colourPositiveGood((int) cellNetworkSize)+"] to ["+Main.operativeInlineSpritePath+Main.resistanceRegionNetworkSize[refregionName]+"]. ");
				}
				else {
					Main.resistanceRegionNetworkSize[refregionName] = (int) cellNetworkSize;
					// if (!Main.resistanceCellNetworksMicro.ContainsKey((int) cellNetworkSize)) {
					// 	Main.resistanceCellNetworksMicro[(int) cellNetworkSize] = new Dictionary<string, TIRegionState>();
					// }
					// Main.resistanceCellNetworksMicro[(int) cellNetworkSize].Add(refregionName, region);
					builder.AppendLine("\nWe've "+TIUtilities.GreenLine("successfully")+" made contact with Local Sympathisers in "+refregionName+". We now possess a Cell Network of size ["+Main.operativeInlineSpritePath+(int) cellNetworkSize+"] in the region. ");
				}
				int biggestCellNetworkForRegion = Main.resistanceRegionNetworkSize[refregionName];
				int prevBiggest = 0;
				// bool changed = false;
				if (Main.resistanceNationNetworkSize.ContainsKey(nationName) && Main.resistanceNationNetworkSize[nationName] < biggestCellNetworkForRegion) {
					prevBiggest = Main.resistanceNationNetworkSize[nationName];
					Main.resistanceNationNetworkSize[nationName] = biggestCellNetworkForRegion;
					// changed = true;
					builder.AppendLine("\nWe've managed to "+TIUtilities.GreenLine("scale up")+" our Operations in "+nation.displayNameWithArticle+". The size of our largest Cell Network in the Nation has increased from ["+Main.operativeInlineSpritePath+prevBiggest+"] to ["+Main.operativeInlineSpritePath+biggestCellNetworkForRegion+"]. ");
				}
				else if (!Main.resistanceNationNetworkSize.ContainsKey(nationName)){
					Main.resistanceNationNetworkSize[nationName] = biggestCellNetworkForRegion;
					// if(!Main.resistanceCellNetworksMacro.ContainsKey((int) cellNetworkSize)) {
					// 	Main.resistanceCellNetworksMacro[(int) cellNetworkSize] = new Dictionary<string, TINationState>();
					// 	Main.resistanceCellNetworksMacro[(int) cellNetworkSize].Add(nationName, nation);
					// }
					// changed = true;
					builder.AppendLine("\nWe've managed to establish our first Cell Network in "+nation.displayNameWithArticle+". ");
				}
				// for (int index = Main.resistanceRegionNetworkSize[refregionName] - (int) cellNetworkSize; index < biggestCellNetworkForRegion; index++) {
				// 	if (!Main.resistanceCellNetworksMicro.ContainsKey(index)) {
				// 		Main.resistanceCellNetworksMicro[index] = new Dictionary<string, TIRegionState>();
				// 		Main.resistanceCellNetworksMicro[index].Add(refregionName, region);
				// 	}
				// 	else if (!Main.resistanceCellNetworksMicro[index].ContainsKey(refregionName)) {
				// 		Main.resistanceCellNetworksMicro[index].Add(refregionName, region);
				// 	}
				// }
				// if (changed) {
				// 	for (int index = Math.Max(1, prevBiggest); index < biggestCellNetworkForRegion; index++) {
				// 		if (!Main.resistanceCellNetworksMacro.ContainsKey(index)) {
				// 			Main.resistanceCellNetworksMacro[index] = new Dictionary<string, TINationState>();
				// 			Main.resistanceCellNetworksMacro[index].Add(nationName, nation);
				// 		}
				// 		else if (!Main.resistanceCellNetworksMacro[index].ContainsKey(nationName)) {
				// 			Main.resistanceCellNetworksMacro[index].Add(nationName, nation);
				// 		}
				// 	}
				// }
				return builder.ToString();
			}	
			return builder.ToString();
		}

		public static string resistanceNetworkDecrease(float cellNetworkSize, string refregionName, TINationState nation, float networkModifier = 1f) {
			StringBuilder builder = new StringBuilder("");
			string nationName = nation.displayName;
			if (Main.resistanceRegionNetworkSize != null && Main.settings.cellnetworksAllowed) {
				if (Main.resistanceRegionNetworkSize.ContainsKey(refregionName)) {
					int prevValue = Main.resistanceRegionNetworkSize[refregionName];
					// Cell Network size should be negative already
					cellNetworkSize *= networkModifier;
					Main.resistanceRegionNetworkSize[refregionName] += (int) cellNetworkSize;
					builder.AppendLine("\nThe size of our Cell Network in "+refregionName+" has "+TIUtilities.RedLine("shrunk")+" by ["+Main.operativeInlineSpritePath+TIUtilities.RedLine(((int) cellNetworkSize).ToString())+"] to ["+Main.operativeInlineSpritePath+Math.Max(0, Main.resistanceRegionNetworkSize[refregionName])+"]");
					if (true || Main.resistanceCellNetworksMicro != null) {
						// foreach(var networkLevel in Main.resistanceCellNetworksMicro.Keys) {
						// 	if (networkLevel > Main.resistanceRegionNetworkSize[refregionName] && Main.resistanceCellNetworksMicro[networkLevel] != null && Main.resistanceCellNetworksMicro[networkLevel].ContainsKey(refregionName)) {
						// 		Main.resistanceCellNetworksMicro[networkLevel].Remove(refregionName);
						// 	}
						// }
						if (Main.resistanceRegionNetworkSize[refregionName] <= 0) {
							Main.resistanceRegionNetworkSize.Remove(refregionName);
							builder.AppendLine("\n\nAll assets in "+refregionName+" have gone dark. It's likely that our enemies have managed to eliminate the Cell Network we had in place. ");
						}
					}
					if ((true || Main.resistanceCellNetworksMacro != null) && Main.resistanceNationNetworkSize != null) {
						if (Main.resistanceNationNetworkSize.ContainsKey(nationName) && prevValue >= Main.resistanceNationNetworkSize[nationName]) {
							int highestRegionValInNation = 0;
							string highestRegionNetwork = refregionName;
							foreach(var region in nation.regions) {
								if (Main.resistanceRegionNetworkSize.ContainsKey(region.displayName) && Main.resistanceRegionNetworkSize[region.displayName] > highestRegionValInNation) {
									highestRegionValInNation = Main.resistanceRegionNetworkSize[region.displayName];
									highestRegionNetwork = region.displayName;
								}
							}
							// foreach(var networkLevel in Main.resistanceCellNetworksMacro.Keys) {
							// 	if (networkLevel > highestRegionValInNation && Main.resistanceCellNetworksMacro[networkLevel].ContainsKey(nationName) && Main.resistanceCellNetworksMacro[networkLevel][nationName] != null) {
							// 		Main.resistanceCellNetworksMacro[networkLevel].Remove(nationName);
							// 	}
							// }
							builder.Append(TIUtilities.RedLine("Consequently")+", the largest Cell Network in "+nation.displayNameWithArticle+" is now the Cell Network of size ["+Main.operativeInlineSpritePath+highestRegionValInNation+"] based in "+highestRegionNetwork+", in contrast to the formerly largest Cell Network of size ["+Main.operativeInlineSpritePath+prevValue+"] in "+refregionName+". ");
						}
					}
				}
				else {
					builder.AppendLine("\nWe've failed to establish a Cell Network in "+refregionName+". ");
				}
			}
			return builder.ToString();
		}
	
		public static float getNumFriendlyCps(TINationState nation, TIFactionState councilorFaction) {
			float friendlyCPs = 0f;
			for (int index = 0; index < nation.numControlPoints; index++) {
				TIFactionState refCPFact = nation.GetControlPoint(index).faction;
				if (refCPFact != null && (refCPFact == councilorFaction || (Main.settings.friendlyFPFlag && refCPFact.GetDiplomacyMood(councilorFaction).Equals("Tolerance")))) {
					friendlyCPs += 1;
				}
			}
			return friendlyCPs;
		}

		public static bool underFriendlyControl(TIControlPoint refCP, TIMissionState mission) {
			return refCP != null && ((refCP.faction != null) && ((refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance"))) || (Main.settings.turnedCouncilBenefits && mission.councilor.turned && mission.councilor.agentForFaction == refCP.faction)));
		}
		public static bool underFriendlyControl(TIControlPoint refCP, TICouncilorState councilor) {
			return refCP != null && ((refCP.faction != null) && ((refCP.faction == councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(councilor.faction).Equals("Tolerance"))) || (Main.settings.turnedCouncilBenefits &&councilor.turned && councilor.agentForFaction == refCP.faction)));
		}

		public static bool underHostileControl(TIControlPoint refCP, TIMissionState mission) {
			return refCP != null && refCP.faction == null || !((refCP.faction != null && refCP.faction == mission.councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(mission.councilor.faction).Equals("Tolerance")) || (Main.settings.turnedCouncilBenefits &&mission.councilor.turned && mission.councilor.agentForFaction == refCP.faction)));
		}
		public static bool underHostileControl(TIControlPoint refCP, TICouncilorState councilor) {
			return refCP != null && refCP.faction == null || !((refCP.faction != null && refCP.faction == councilor.faction || (Main.settings.friendlyFPFlag && refCP.faction.GetDiplomacyMood(councilor.faction).Equals("Tolerance")) || (Main.settings.turnedCouncilBenefits &&councilor.turned && councilor.agentForFaction == refCP.faction)));
		}

		public static List<TIFactionState> presentHostiles(TINationState nationState, TIFactionState factionState) {
			List<TIFactionState> factionList = new List<TIFactionState>();
			foreach (TIFactionState faction in nationState.FactionsWithControlPoint) {
				if (faction != factionState && !faction.GetDiplomacyMood(factionState).Equals("Tolerance")) {
					factionList.Add(faction);
				}
			}
			return factionList;

		}

		public static float warStrength(TINationState nation) {
			float retMod = -10f;
			if (nation.atWar) {
				int numHostileNukes = 0;
				int numFriendlyNukes = nation.numNuclearWeapons;
				float friendlyEcoStrength = nation.BaseInvestmentPoints_month();
				float hostileEcoStrength = 0;
				float numFAStrength = nation.numStandardArmies * nation.militaryTechLevel * (nation.navalFreedom ? 1f : 0.5f);
				float numHAStrength = 0;
				foreach (TINationState allyNation in nation.CurrentWarAllies_AllWars()) {
					numFAStrength += allyNation.numStandardArmies * allyNation.militaryTechLevel * (allyNation.navalFreedom ? 1f : 0.5f);
					friendlyEcoStrength += allyNation.BaseInvestmentPoints_month();
				}
				
				foreach (TINationState hostileNation in nation.wars) {
					numHostileNukes += hostileNation.numNuclearWeapons;
					numHAStrength += hostileNation.numStandardArmies * hostileNation.militaryTechLevel * (hostileNation.navalFreedom ? 1f : 0.5f);
					hostileEcoStrength += hostileNation.BaseInvestmentPoints_month();
				}
				float provinceThreatScore = 0f;
				foreach (TIRegionState regionState in nation.regions) {
					if (regionState.OccupationUnderwayButNotComplete()) {
						provinceThreatScore += (regionState.coreEconomicRegion? 1.25f : (regionState.colonyRegion? 0.50f : 1.00f)) * (regionState.isCapital? 3f : 1f);
					}
					else if (regionState.IsOccupied()) {
						provinceThreatScore += 2 * (regionState.coreEconomicRegion? 1.25f : (regionState.colonyRegion? 0.50f : 1.00f)) * (regionState.isCapital? 3f : 1f);
					}
					
					if (regionState.BorderWithAnotherNation(true)) {
						provinceThreatScore += 0.5f;
					}
				}
				float winingStrength = Math.Max(1, friendlyEcoStrength/5f+numFAStrength+numFriendlyNukes*10f)/Math.Max(1, provinceThreatScore/nation.regions.Count * (5f * provinceThreatScore + hostileEcoStrength/5f+numHAStrength+numHostileNukes*10f));
				retMod = 1 * Math.Max(0.01f, Math.Min(1/winingStrength, 20)) - 10f;
			}
			return retMod;
		}
		public static bool isAllied(TIFactionState initatorFaction, TIFactionState checkedFaction) {
			if (Main.settings.allowTolerated) {
				if (initatorFaction.GetDiplomacyMood(checkedFaction).Equals("Tolerance")) {
					return true;
				}
			}
			//if (Main.settings.NAPCountsAsAllied) {
				//if (initatorFaction.CanTradeNAP)
			//}
			return initatorFaction.permanentAlly(checkedFaction);
		}
	}
}