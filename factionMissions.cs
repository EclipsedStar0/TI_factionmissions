using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using HarmonyLib;
using UnityModManagerNet;
using PavonisInteractive.TerraInvicta;
using Extensions = UnityModManagerNet.Extensions;
using System.IO;
using PavonisInteractive.TerraInvicta.GamePlayScript.AI;
using UnityEngine;
using factionMissions.MissionModifiers;
using PavonisInteractive.TerraInvicta.Tasks;
using Poly2Tri;

namespace factionMissions
{
    public class Main
    {
        private static bool Load(UnityModManager.ModEntry modEntry)
		{
			new Harmony(modEntry.Info.Id).PatchAll();
			Main.settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
			modEntry.OnGUI = new Action<UnityModManager.ModEntry>(Main.OnGUI);
			modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(Main.OnSaveGUI);
			Main.mod = modEntry;
			modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(Main.OnToggle);
			Main.destroyMissionList = new List<string> {"DestroyRaiseMilitia"};
			Main.resistMissionList = new List<string> {"ResistPeacekeepers", "ResistCellNetwork", "ResistHumanitarianMission", "ResistSmuggleArms"};
			Main.escapeMissionList = new List<string> {"EscapeFundSpaceProgram", "EscapeExpandSpaceAgency"};
			Main.exploitMissionList = new List<string> {"ExploitIgnoreEcologicalProtections"};
			Main.studyMissionList = new List<string> {"StudyShareResearch", "StudyEducatePopulace", "StudyTechSummit"};
			Main.submitMissionList = new List<string> {};
			Main.serveMissionList = new List<string> {"ServeProselytiseCouncillors"};
			Main.resistanceCellNetworksMicro = new Dictionary<int, Dictionary<string, TIRegionState>> {{0, new Dictionary<string, TIRegionState>{{"FakeRegion", null}}}};
			Main.resistanceRegionNetworkSize = new Dictionary<string, int> {{"FakeRegion", 0}};
			Main.resistanceNationNetworkSize = new Dictionary<string, int> {{"FakeNation", 0}};
			Main.resistanceCellNetworksMacro = new Dictionary<int, Dictionary<string, TINationState>> {{0, new Dictionary<string, TINationState>{{"FakeNation", null}}}};
			Main.resistanceRegionArms = new Dictionary<string, float> {{"FakeRegion", 0f}};
			Main.resistanceRegionGDPModifiers = new Dictionary<string, double> {{"FakeRegion", 0}};
			Main.armyTracker = new Dictionary<string, TIArmyState> {};
			Main.armyStrengthTracker = new Dictionary<string, float> {};
			Main.armyTypeTracker = new Dictionary<string, int> {};

			Main.masterMissionList = new List<string> {};
			if (Main.destroyMissionList != null) {
				for (int index = 0; index < Main.destroyMissionList.Count; index++) {
					Main.masterMissionList.Add(Main.destroyMissionList[index]);
				}
			}
			if (Main.resistMissionList != null) {
				for (int index = 0; index < Main.resistMissionList.Count; index++) {
					Main.masterMissionList.Add(Main.resistMissionList[index]);
				}
				Main.operativeInlineSpritePath = TemplateManager.global.espionageInlineSpritePath;
			}
			if (Main.escapeMissionList != null) {
				for (int index = 0; index < Main.escapeMissionList.Count; index++) {
					Main.masterMissionList.Add(Main.escapeMissionList[index]);
				}
			}
			if (Main.exploitMissionList != null) {
				for (int index = 0; index < Main.exploitMissionList.Count; index++) {
					Main.masterMissionList.Add(Main.exploitMissionList[index]);
				}
			}
			if (Main.studyMissionList != null) {
				for (int index = 0; index < Main.studyMissionList.Count; index++) {
					Main.masterMissionList.Add(Main.studyMissionList[index]);
				}
			}
			if (Main.submitMissionList != null) {
				for (int index = 0; index < Main.submitMissionList.Count; index++) {
					Main.masterMissionList.Add(Main.submitMissionList[index]);
				}
			}
			if (Main.serveMissionList != null) {
				for (int index = 0; index < Main.serveMissionList.Count; index++) {
					Main.masterMissionList.Add(Main.serveMissionList[index]);
				}
			}

			Main.settings.OnChange();

			FileLog.Log("[factionMissions] Loaded");
			return true;
		}

		// Token: 0x06000002 RID: 2 RVA: 0x00002050 File Offset: 0x00000250
		private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
		{
			Main.enabled = value;
			return true;
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002059 File Offset: 0x00000259
		private static void OnGUI(UnityModManager.ModEntry modEntry)
		{
			Extensions.Draw<Settings>(Main.settings, modEntry);
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002066 File Offset: 0x00000266
		private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
		{
			Main.settings.Save(modEntry);
		}

		// Token: 0x04000001 RID: 1
		public static bool enabled;

		// Token: 0x04000002 RID: 2
		public static UnityModManager.ModEntry mod;

		// Token: 0x04000003 RID: 3
		public static Settings settings;

		public static List<String>? masterMissionList;		
		public static List<String>? destroyMissionList;
		public static List<String>? resistMissionList;
		public static List<String>? escapeMissionList;
		public static List<String>? exploitMissionList;
		public static List<String>? studyMissionList;
		public static List<String>? submitMissionList;
		public static List<String>? serveMissionList;
		public static Dictionary<int, Dictionary<string, TIRegionState>>? resistanceCellNetworksMicro;
		public static Dictionary<string, int>? resistanceRegionNetworkSize;
		public static Dictionary<string, int>? resistanceNationNetworkSize;
		public static Dictionary<int, Dictionary<string, TINationState>>? resistanceCellNetworksMacro;
		public static Dictionary<string, float>? resistanceRegionArms;
		public static Dictionary<string, double>? resistanceRegionGDPModifiers;

		public static Dictionary<string, TIArmyState>? armyTracker;
		public static Dictionary<string, float>? armyStrengthTracker;
		public static Dictionary<string, int>? armyTypeTracker;

		

		//public static UnityEngine.Sprite? operativeInlineSpritePath;
		public static string? operativeInlineSpritePath;
    }

    //Settings class to interface with Unity Mod Manager
    public class Settings : UnityModManager.ModSettings, IDrawable
	{
		// Token: 0x06000006 RID: 6 RVA: 0x0000207B File Offset: 0x0000027B
		public override void Save(UnityModManager.ModEntry modEntry)
		{
			UnityModManager.ModSettings.Save<Settings>(this, modEntry);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002084 File Offset: 0x00000284
		public void OnChange()
		{
			// These settings disable the missions if they have no missions listed in their list
			if (destroyMissions && (Main.destroyMissionList == null || (Main.destroyMissionList != null && Main.destroyMissionList.Count < 1))) {
				destroyMissions = false;
			}
			if (resistMissions && (Main.resistMissionList == null || (Main.resistMissionList != null && Main.resistMissionList.Count < 1))) {
				resistMissions = false;
				cellnetworksAllowed = false;
			}
			if (escapeMissions && (Main.escapeMissionList == null || (Main.escapeMissionList != null && Main.escapeMissionList.Count < 1))) {
				escapeMissions = false;
			}
			if (exploitMissions && (Main.exploitMissionList == null || (Main.exploitMissionList != null && Main.exploitMissionList.Count < 1))) {
				exploitMissions = false;
			}
			if (studyMissions && (Main.studyMissionList == null || (Main.studyMissionList != null && Main.studyMissionList.Count < 1))) {
				studyMissions = false;
			}
			if (submitMissions && (Main.submitMissionList == null || (Main.submitMissionList != null && Main.submitMissionList.Count < 1))) {
				submitMissions = false;
			}
			if (serveMissions && (Main.serveMissionList == null || (Main.serveMissionList != null && Main.serveMissionList.Count < 1))) {
				serveMissions = false;
			}
			
			//Now for other if-statements
			if (!resistMissions) {
				cellnetworksAllowed = false;

			}
		}

		[Draw("Disable (Performance) Costly Missions", Collapsible = true)]
        public bool costlyMissionsDisabled = true;

		[Header(header:"Faction Mission Toggles")]
		//[Draw("Settings", Collapsible = true)] public fmSettingsF1 FactionMissionToggles = new fmSettingsF1();
		[Draw("Enable Humanity First Missions", Collapsible = true)] public bool destroyMissions = true;
        [Draw("Enable Resistance Missions", Collapsible = true)] public bool resistMissions = true;
        [Draw("Enable Project Exodus Missions", Collapsible = true)] public bool escapeMissions = true;
        [Draw("Enable Initiative Missions", Collapsible = true)] public bool exploitMissions = true;
        [Draw("Enable Academy Missions", Collapsible = true)] public bool studyMissions = true;
        [Draw("Enable Protectorate Missions", Collapsible = true)] public bool submitMissions = true;
        [Draw("Enable Servants Missions", Collapsible = true)] public bool serveMissions = true;

		[Header(header:"Mission-Specific Configuration")]
        [Draw("Peacekeeper Advisory Factor", Collapsible = true)] public float peacekeeperFactor = 5;
        [Draw("Resistance Cell Networks", Collapsible = true)] public bool cellnetworksAllowed = true;
        [Draw("-Cell Network; 1 Network Level for every x Espionage", Collapsible = true, VisibleOn ="cellnetworksAllowed|true")] public float cellNetworkEspionageModifier = 5f;
        [Draw("-Cell Network; 1 Network Level for every x Command", Collapsible = true, VisibleOn ="cellnetworksAllowed|true")] public float cellNetworkCommandModifier = 10f;

		[Draw("-Cell Network; Allow Cell Network Spread", Collapsible = true, VisibleOn ="cellnetworksAllowed|true")] public bool adjacentCellNetworks = true;
		[Draw("-Enable GDP Modifiers?", Collapsible = true, VisibleOn ="cellnetworksAllowed|true")]	public bool GDPModifiers = true;


		[Header(header:"Mission-Generic Configuration")]
        [Draw("Friendly Control Points", Collapsible = true)] public bool friendlyFPFlag = true;
        [Draw("Consider Tolerance as Allied", Collapsible = true)] 	public bool allowTolerated = true;
		[Draw("Turned Councilors are *also* considered as the *Spying Faction* for Effect-Calculation", Collapsible = true)] public bool turnedCouncilBenefits = true;
		[Draw("Scale Influence of Investment Points in AI Evaluation.")] public float aiInvestPointScale = 100f;
	}	

	//[DrawFields(DrawFieldMask.Public)]
	// public class fmSettingsF1 {
	// 	[Draw("Enable Humanity First Missions", Collapsible = true)] public bool FMdestroyMissions = true;
    //     [Draw("Enable Resistance Missions", Collapsible = true)] public bool FMresistMissions = true;
    //     [Draw("Enable Project Exodus Missions", Collapsible = true)] public bool FMescapeMissions = true;
    //     [Draw("Enable Initiative Missions", Collapsible = true)] public bool FMexploitMissions = true;
    //     [Draw("Enable Academy Missions", Collapsible = true)] public bool FMstudyMissions = true;
    //     [Draw("Enable Protectorate Missions", Collapsible = true)] public bool FMsubmitMissions = true;
    //     [Draw("Enable Servants Missions", Collapsible = true)] public bool FMserveMissions = true;
	// }
		

}






	




	// [HarmonyPatch(typeof(TINationState), nameof(TINationState.AddToUnrest))]
    // public static class UnrestPatch {
	// 	[HarmonyPrefix]
	//  	public static bool Prefix(TINationState __instance, ref float value, ref float cap) {
	//  		value -= __instance.adviserCommandBonus * Main.settings.peacekeeperFactor * 2;
	// 		return true;
	//  	}
	// }

	// [HarmonyPatch(typeof(TIMissionEffect_Advise), nameof(TIMissionEffect_Advise.ApplyEffect), MethodType.Getter)]
	// public static class adviseMissionEffectPatch {		
	// 	[HarmonyPostfix]
	// 	public static string Postfix(String __result, ref TIMissionState mission, ref TIGameState target, ref TIMissionOutcome outcome) {
	// 		StringBuilder builder = new StringBuilder(__result);
	// 		if (mission.ref_councilor.GetAttribute(CouncilorAttribute.Command) > 0f)
	// 		{
	// 			var unrestAmount = Main.settings.peacekeeperFactor * mission.ref_councilor.GetAttribute(CouncilorAttribute.Command) / (float) 100;
	// 			if (Main.settings.resistMissions && mission.ref_councilor.ref_faction.template.dataName.Equals("ResistCouncil")) {
	// 				unrestAmount *= 2;
	// 				builder.AppendLine(" (The Resistance gets a x2 bonus to Peacekeeping)");
	// 			}
	// 			builder.AppendLine(Loc.T("UI.Nation.FactionMissions.AdviseEffect", new object[]{(unrestAmount).ToString("N2")}));
	// 			// FileLog.Log("[factionMissions] AppendAlphaState");
	// 		}
	// 		else {
	// 			builder.AppendLine("You have no command attribute or have a value of 0?");
	// 			// FileLog.Log("[factionMissions] AppendBravoState");
	// 		}
	// 		mission.ref_nation.AddToUnrest(-1 * Main.settings.peacekeeperFactor * mission.ref_councilor.GetAttribute(CouncilorAttribute.Command) / (float) 100);
	// 		return builder.ToString();
	// 	}
	// }

	
	// [HarmonyPatch(typeof(NationInfoController), nameof(NationInfoController.BuildUnrestTooltip))]
	// static class unrestUIPatch {
	// 	[HarmonyPostfix]
	// 	static string Postfix(String __result, ref TINationState nation) {
	// 		StringBuilder builder = new StringBuilder(__result);
	// 		if (nation != null) {
	// 			if (nation.advisingCouncilors.Count > 0f && nation.adviserCommandBonus > 0f)
	// 			{
	// 				builder.AppendLine(Loc.T("UI.Nation.FactionMissions.UnrestToolTip", new object[]{TIUtilities.FormatSmallNumber(nation.adviserCommandBonus * -1 * Main.settings.peacekeeperFactor, 7, 1, true)}));
	// 			}
	// 			else {
	// 				builder.AppendLine(Loc.T("UI.Nation.FactionMissions.UnrestToolTipTypeAlpha", new object[]{}));
	// 			}
	// 		}
	// 		else {
	// 			builder.AppendLine(Loc.T("UI.Nation.FactionMissions.UnrestToolTipTypeBravo", new object[]{}));
	// 		}
	// 		return builder.ToString();
	// 	}
	// }