using HarmonyLib;
using System.Collections.Generic;
using PavonisInteractive.TerraInvicta;

namespace factionMissions.MissionConditions {
    public class TIMissionCondition_atWar : TIMissionCondition
    {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
            TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation.atWar) {
				return "_Pass";
			}
			else {
				return "Requires the target nation to be at War with another Nation.";
			}
        }
    }

    public class TIMissionCondition_hasResistCellNetworkNationSmallerThan : TIMissionCondition {
		public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (!Main.settings.cellnetworksAllowed || Main.resistanceNationNetworkSize == null) {
				TIMissionCondition missionCondition = new TIMissionCondition_hasAtLeastSparseSupport();
				return missionCondition.CanTarget(councilor, possibleTarget);
			}
			else if (Main.settings.cellnetworksAllowed){
				if (!Main.resistanceNationNetworkSize.ContainsKey(ref_nation.displayName)) {
					return "_Pass";
				}
			}
			return "Requires the target nation to NOT have a cell network in place.";
        }

		public string CanTarget(TICouncilorState councilor, TIGameState possibleTarget, int resistCellNetworkSize = 0)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (!Main.settings.cellnetworksAllowed || Main.resistanceNationNetworkSize == null) {
				TIMissionCondition missionCondition = new TIMissionCondition_hasAtLeastSparseSupport();
				return missionCondition.CanTarget(councilor, possibleTarget);
			}
			else if (Main.settings.cellnetworksAllowed){
				if (!Main.resistanceNationNetworkSize.ContainsKey(ref_nation.displayName) || (Main.resistanceNationNetworkSize.ContainsKey(ref_nation.displayName) && Main.resistanceNationNetworkSize[ref_nation.displayName] < resistCellNetworkSize)) {
					return "_Pass";
				}
			}
			return "Requires the target nation to have a Cell Network SMALLER than ["+resistCellNetworkSize+"]; Largest CellNetwork in Nation is ["+Main.resistanceNationNetworkSize[ref_nation.displayName]+"]";
        }
	}
	public class TIMissionCondition_hasResistCellNetworkRegionSmallerThan : TIMissionCondition {
		public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TIRegionState ref_region = possibleTarget.ref_region;
			if (!Main.settings.cellnetworksAllowed || Main.resistanceRegionNetworkSize == null) {
				TIMissionCondition missionCondition = new TIMissionCondition_hasAtLeastSparseSupport();
				return missionCondition.CanTarget(councilor, possibleTarget);
			}
			else if (Main.settings.cellnetworksAllowed){
				if (!Main.resistanceRegionNetworkSize.ContainsKey(ref_region.displayName)) {
					return "_Pass";
				}
			}
			return "Requires the target region to NOT have a cell network in place.";
        }

		public string CanTarget(TICouncilorState councilor, TIGameState possibleTarget, int resistCellNetworkSize = 0)
        {
			TIRegionState ref_region = possibleTarget.ref_region;
			if (!Main.settings.cellnetworksAllowed || Main.resistanceRegionNetworkSize == null) {
				TIMissionCondition missionCondition = new TIMissionCondition_hasAtLeastSparseSupport();
				return missionCondition.CanTarget(councilor, possibleTarget);
			}
			else if (Main.settings.cellnetworksAllowed){
				if (!Main.resistanceRegionNetworkSize.ContainsKey(ref_region.displayName) || (Main.resistanceRegionNetworkSize.ContainsKey(ref_region.displayName) && Main.resistanceRegionNetworkSize[ref_region.displayName] < resistCellNetworkSize)) {
					return "_Pass";
				}
			}
			return "Requires the target region to have a Cell Network SMALLER than ["+resistCellNetworkSize+"]; Largest CellNetwork in Region is ["+Main.resistanceRegionNetworkSize[ref_region.displayName]+"]";
        }
	}

	public class TIMissionCondition_hasResistCellNetworkInNation : TIMissionCondition {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (!Main.settings.cellnetworksAllowed || Main.resistanceNationNetworkSize == null) {
					TIMissionCondition missionCondition = new TIMissionCondition_hasAtLeastSparseSupport();
					return missionCondition.CanTarget(councilor, possibleTarget);
				}
				else if (Main.settings.cellnetworksAllowed){
					if (Main.resistanceNationNetworkSize.ContainsKey(ref_nation.displayName)) {
						return "_Pass";
					}
				}
			}
			return "Lacking Cell Network in target nation";
        }

		public string CanTarget(TICouncilorState councilor, TIGameState possibleTarget, int resistCellNetworkSize = 0)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (!Main.settings.cellnetworksAllowed || Main.resistanceNationNetworkSize == null) {
					TIMissionCondition missionCondition = new TIMissionCondition_hasAtLeastSparseSupport();
					return missionCondition.CanTarget(councilor, possibleTarget);
				}
				else if (Main.settings.cellnetworksAllowed){
					if (Main.resistanceNationNetworkSize.ContainsKey(ref_nation.displayName) && Main.resistanceNationNetworkSize[ref_nation.displayName] >= resistCellNetworkSize) {
						return "_Pass";
					}
					else if (Main.resistanceNationNetworkSize.ContainsKey(ref_nation.displayName)) {
						return "Lacking Cell Network of size ["+resistCellNetworkSize+"] in target nation; Largest CellNetwork in Region is ["+Main.resistanceNationNetworkSize[ref_nation.displayName]+"]";
					}
				}
			}
			return "Lacking Cell Network of size ["+resistCellNetworkSize+"] in target nation;";
        }
    }

	public class TIMissionCondition_hasResistCellNetworkInRegion : TIMissionCondition {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			if (possibleTarget != null) {
				TIRegionState ref_region = possibleTarget.ref_region;
				if (!Main.settings.cellnetworksAllowed || Main.resistanceRegionNetworkSize == null) {
					TIMissionCondition missionCondition = new TIMissionCondition_hasAtLeastSparseSupport();
					FileLog.Log("(R) Cell Networks are forbidden, checking secondary condition");
					return missionCondition.CanTarget(councilor, possibleTarget);
				}
				else if (Main.settings.cellnetworksAllowed){
					if (Main.resistanceRegionNetworkSize.ContainsKey(ref_region.displayName)) {
						return "_Pass";
					}
				}
			}
			return "Lacking Cell Network in target region";
        }

		public string CanTarget(TICouncilorState councilor, TIGameState possibleTarget, int resistCellNetworkSize = 0)
        {
			if (possibleTarget != null) {
				TIRegionState ref_region = possibleTarget.ref_region;
				if (!Main.settings.cellnetworksAllowed || Main.resistanceRegionNetworkSize == null) {
					TIMissionCondition missionCondition = new TIMissionCondition_hasAtLeastSparseSupport();
					return missionCondition.CanTarget(councilor, possibleTarget);
				}
				else if (Main.settings.cellnetworksAllowed){
					if (Main.resistanceRegionNetworkSize.ContainsKey(ref_region.displayName) && Main.resistanceRegionNetworkSize[ref_region.displayName] >= resistCellNetworkSize) {
						return "_Pass";
					}
					else if (Main.resistanceRegionNetworkSize.ContainsKey(ref_region.displayName)) {
						return "Lacking Cell Network of size ["+resistCellNetworkSize+"] in target region; Largest CellNetwork in Region is ["+Main.resistanceRegionNetworkSize[ref_region.displayName]+"]";
					}
				}
			}
			return "Lacking Cell Network of size ["+resistCellNetworkSize+"] in target region;";
        }
    }

    public class TIMissionCondition_hasOwnCP : TIMissionCondition
    {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null && ref_nation.NumOwnedControlPoints > 0) {
				if (ref_nation.FactionControlPoints(councilor.faction, false, false, true).Count > 0) {
					return "_Pass";	
				}
			}
            return "This mission requires owning a control point in the target nation.";
        }
    }

    public class TIMissionCondition_hasFriendlyNotSelfCP : TIMissionCondition
    {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null && ref_nation.NumOwnedControlPoints > 0) {
				for (int index = 0; index < ref_nation.numControlPoints; index++) {
					if (ref_nation.GetControlPoint(index) != null && ref_nation.GetControlPoint(index).faction != councilor.faction && ref_nation.GetControlPoint(index).faction.GetDiplomacyMood(councilor.faction).Equals("Tolerance")) {
						return "_Pass";
					}
				}
			}
            return "This mission requires a faction with the 'Tolerance' Diplomacy Mood to control a control point in the target nation.";
        }
    }

    public class TIMissionCondition_hasFriendlyCPs : TIMissionCondition {
		public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget) {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null && ref_nation.NumOwnedControlPoints > 0) {
				for (int index = 0; index < ref_nation.numControlPoints; index++) {
					if (factionMissions.utilityFunctions.UtilityModule.underFriendlyControl(ref_nation.GetControlPoint(index), councilor)) {
						return "_Pass";
					}
				}
			}
			// return Loc.T("TIMissionConditon_hasFriendlyCPs", new object[] { }).ToString();
			return "This mission requires either owning a control point in the target, or a faction that has the 'Tolerance' diplomacy mood with your faction, own a control point in the requisite nation.";
		}

		public new List<string> feedback {
			get {
				FileLog.Log("Someone called for the FriendlyCP Feedback");
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionConditon_hasFriendlyCPs", new object[] { }));
				return missionConditionFeedback;
			}
		}
	}

	public class TIMissionCondition_hasEnemyCPs : TIMissionCondition {
		public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget) {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null && ref_nation.NumOwnedControlPoints > 0) {
				for (int index = 0; index < ref_nation.numControlPoints; index++) {
					if (factionMissions.utilityFunctions.UtilityModule.underHostileControl(ref_nation.GetControlPoint(index), councilor)) {
						return "_Pass";
					}
				}
			}
			// return Loc.T("TIMissionConditon_hasEnemyCPs", new object[] { }).ToString();
			return "This mission requres a faction that your faction has the diplomacy mood of 'War' to own a control point in the targetted nation.";
		}

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionConditon_hasEnemyCPs", new object[] { }));
				return missionConditionFeedback;
			}
		}
	}

	public class TIMissionCondition_hasSpaceProgram : TIMissionCondition {
		public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget) {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (ref_nation.spaceFlightProgram) {
					return "_Pass";
				}
			}
			// return Loc.T("TIMissionConditon_hasSpaceProgram", new object[] { }).ToString();
			return "This mission requires the target to have completed the Space Program Priority.";
		}

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionConditon_hasSpaceProgram", new object[] { }));
				return missionConditionFeedback;
			}
		}
	}

	public class TIMissionCondition_noSpaceProgram : TIMissionCondition {
		public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget) {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (!ref_nation.spaceFlightProgram) {
					return "_Pass";
				}
			}
			// return Loc.T("TIMissionConditon_noSpaceProgram", new object[] { }).ToString();
			return "This mission requres the target to NOT have completed the Space Program Priority.";
		}

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionConditon_noSpaceProgram", new object[] { }));
				return missionConditionFeedback;
			}
		}
	}

	public class TIMissionCondition_hasAtLeastSparseSupport : TIMissionCondition {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (ref_nation.GetPublicOpinionProportion(councilor.faction.ideology.ideology) > 0.1) {
					return "_Pass";
				}
			}
			// return Loc.T("TIMissionCondition_hasAtLeastSparseSupport", new object[] { }).ToString();
			return "This mission requires your faction to have at least 10% public support in the target nation.";
        }

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionCondition_hasAtLeastSparseSupport", new object[] { }));
				return missionConditionFeedback;
			}
		}
    }

	public class TIMissionCondition_hasAtLeastGeneralSupport : TIMissionCondition {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (ref_nation.GetPublicOpinionProportion(councilor.faction.ideology.ideology) > 0.2) {
					return "_Pass";
				}
			}
			// return Loc.T("TIMissionCondition_hasAtLeastGeneralSupport", new object[] { }).ToString();
			return "This mission requires your faction to have at least 20% public support in the target nation.";
        }

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionCondition_hasAtLeastGeneralSupport", new object[] { }));
				return missionConditionFeedback;
			}
		}
    }

	public class TIMissionCondition_hasAtLeastModerateSupport : TIMissionCondition {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (ref_nation.GetPublicOpinionProportion(councilor.faction.ideology.ideology) > 0.35) {
					return "_Pass";
				}
			}
			// return Loc.T("TIMissionCondition_hasAtLeastModerateSupport", new object[] { }).ToString();
			return "This mission requires your faction to have at least 35% public support in the target nation.";
        }

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionCondition_hasAtLeastModerateSupport", new object[] { }));
				return missionConditionFeedback;
			}
		}
    }

	public class TIMissionCondition_hasDominantSupport : TIMissionCondition {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (ref_nation.GetPublicOpinionProportion(councilor.faction.ideology.ideology) > 0.50) {
					return "_Pass";
				}
			}
			// return Loc.T("TIMissionCondition_hasDominantSupport", new object[] { }).ToString();
			return "This mission requires your faction to have at least 50% public support in the target nation.";
        }

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionCondition_hasDominantSupport", new object[] { }));
				return missionConditionFeedback;
			}
		}
    }

	public class TIMissionCondition_hasOverwhelmingSupport : TIMissionCondition {
        public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget)
        {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (ref_nation.GetPublicOpinionProportion(councilor.faction.ideology.ideology) > 0.75) {
					return "_Pass";
				}
			}
			// return Loc.T("TIMissionCondition_hasOverwhelmingSupport", new object[] { }).ToString();
			return "This mission requires your faction to have at least 75% public support in the target nation.";
        }

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionCondition_hasOverwhelmingSupport", new object[] { }));
				return missionConditionFeedback;
			}
		}
    }

	public class TIMissionCondition_ideologicalBattleground : TIMissionCondition {
		public override string CanTarget(TICouncilorState councilor, TIGameState possibleTarget) {
			TINationState ref_nation = possibleTarget.ref_nation;
			if (ref_nation != null) {
				if (factionMissions.utilityFunctions.UtilityModule.ideologicalDistance(ref_nation.GetMeanPublicOpinionVector(), ref_nation.GetMostPopularIdeology(true).ideologyCoordinates) > 2.50 / 2) {
					return "_Pass";
				}
			}
			// return Loc.T("TIMissionConditon_ideologicalBattleground", new object[] { }).ToString();
			return "This mission requires the target nation to be an 'ideological battleground' with extremist factions present to some substantial degree on both sides in the target nation";
		}

		public new List<string> feedback {
			get {
				List<string> missionConditionFeedback = new List<string>();
				missionConditionFeedback.Add(Loc.T("TIMissionConditon_ideologicalBattleground", new object[] { }));
				return missionConditionFeedback;
			}
		}
	}
}
