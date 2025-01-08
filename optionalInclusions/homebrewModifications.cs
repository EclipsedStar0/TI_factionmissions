using HarmonyLib;
using PavonisInteractive.TerraInvicta;
using PavonisInteractive.TerraInvicta.GamePlayScript.AI;

namespace factionMissions.homebrewBugFixes {
	[HarmonyPatch(typeof(PavonisInteractive.TerraInvicta.GamePlayScript.AI.AssessShipIsPointedTowardsEnemyFleetCenterOfMassLeafNode), nameof(PavonisInteractive.TerraInvicta.GamePlayScript.AI.AssessShipIsPointedTowardsEnemyFleetCenterOfMassLeafNode.Execute))]
	public static class shipPointingHeaderPatch {
		[HarmonyPrefix]
		public static bool shipPointingPatch(PavonisInteractive.TerraInvicta.GamePlayScript.AI.AssessShipIsPointedTowardsEnemyFleetCenterOfMassLeafNode __instance, ref CombatShipBehaviourTree.ConditionResponse __result) {
			// FileLog.Log("Begining Aspect 1");
			if (__instance == null) {
				__result = CombatShipBehaviourTree.ConditionResponse.Success;
				return false;
			}
			// FileLog.Log("Running Aspect 2");
			Traverse traverseObj = Traverse.Create(__instance);
			if (traverseObj == null) {
				__result = CombatShipBehaviourTree.ConditionResponse.Success;
				return false;
			}
			// FileLog.Log("Running Aspect 3");
			CombatShipBehaviourTree.SharedBehaviourData tempControl = traverseObj.Field("_sharedData").GetValue<CombatShipBehaviourTree.SharedBehaviourData>();
			// FileLog.Log("Running Aspect 4");
			Traverse traverseObj2 = Traverse.Create(tempControl);
			// FileLog.Log("Running Aspect 5");
			CombatFleetController tempControl2 = traverseObj2.Field("OpposingFleetController").GetValue<CombatFleetController>();
			// FileLog.Log("Running Aspect 6");
			if (tempControl2 == null || tempControl2.activeShipControllers == null || tempControl2.activeShipControllers.Count <= 0) {
				__result = CombatShipBehaviourTree.ConditionResponse.Success;
				return false;
			}
			// FileLog.Log("Concluded");
			return true;
		}
	}

	[HarmonyPatch(typeof(PavonisInteractive.TerraInvicta.GamePlayScript.AI.AssessShipIsAtDisengagementSpeedLeafNode), nameof(PavonisInteractive.TerraInvicta.GamePlayScript.AI.AssessShipIsAtDisengagementSpeedLeafNode.Execute))]
	public static class shipDisengagementHeaderPatch {
		[HarmonyPrefix]
		public static bool shipDisengagePatch(AssessShipIsAtDisengagementSpeedLeafNode __instance, ref CombatShipBehaviourTree.ConditionResponse __result) {
			Traverse traverseObj = Traverse.Create(__instance);
			CombatShipBehaviourTree.SharedBehaviourData tempControl = traverseObj.Field("_sharedData").GetValue<CombatShipBehaviourTree.SharedBehaviourData>();
			Traverse traverseObj2 = Traverse.Create(tempControl);
			CombatFleetController tempControl2 = traverseObj2.Field("OpposingFleetController").GetValue<CombatFleetController>();
			if (tempControl2 == null || tempControl2.activeShipControllers == null || tempControl2.activeShipControllers.Count <= 0) {
				__result = CombatShipBehaviourTree.ConditionResponse.Success;
				return false;
			}
			return true;
		}
	}
}