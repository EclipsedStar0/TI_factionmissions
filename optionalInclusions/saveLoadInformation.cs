using System;
using System.IO;
using System.Collections.Generic;
using HarmonyLib;
using PavonisInteractive.TerraInvicta;

namespace factionMissions.saveInformation {
	[HarmonyPatch(typeof(SaveMenuController), nameof(SaveMenuController.WriteSaveFile))]
	public static class saveMenuHeaderPatch {
		[HarmonyPostfix]
		public static void saveMenuPatch(SaveMenuController __instance) {
			Traverse traverseObj = Traverse.Create(__instance);
			string saveFileName = traverseObj.Field("saveFileString").GetValue<String>();
            Directory.CreateDirectory("mods/factionMissionData");
			StreamWriter saveFile = new StreamWriter("mods/factionMissionData/"+saveFileName+".txt");
			// saveFile.WriteLine("RegionMicroController = {");		
			// foreach(KeyValuePair<int, Dictionary<string, TIRegionState>> cellNetworkLevel in Main.resistanceCellNetworksMicro) {
			// 	saveFile.WriteLine("\t"+cellNetworkLevel.Key+" = {");
			// 	foreach(KeyValuePair<string, TIRegionState> region in cellNetworkLevel.Value) {
			// 		saveFile.WriteLine("\t\t"+region.Key);
			// 	}
			// 	saveFile.WriteLine("\t}");
			// }
			// saveFile.WriteLine("}");
			// saveFile.WriteLine("RegionMacroController = {");				
			// foreach(KeyValuePair<int, Dictionary<string, TINationState>> cellNetworkLevel in Main.resistanceCellNetworksMacro) {
			// saveFile.WriteLine("\t"+cellNetworkLevel.Key+" = {");
			// 	foreach(KeyValuePair<string, TINationState> nation in cellNetworkLevel.Value) {
			// 		saveFile.WriteLine("\t\t"+nation.Key);
			// 	}
			// 	saveFile.WriteLine("\t}");
			// }
			// saveFile.WriteLine("}");
			saveFile.WriteLine("RegionCellValue = {");
			foreach(KeyValuePair<string, int> region in Main.resistanceRegionNetworkSize) {
				saveFile.WriteLine("\t"+region.Key+" = "+region.Value);
			}
			saveFile.WriteLine("}");
			saveFile.WriteLine("NationCellValue = {");
			foreach(KeyValuePair<string, int> nation in Main.resistanceNationNetworkSize) {
				saveFile.WriteLine("\t"+nation.Key+" = "+nation.Value);
			}
			saveFile.WriteLine("}");
			saveFile.WriteLine("RegionArms = {");
			foreach(KeyValuePair<string, float> nation in Main.resistanceRegionArms) {
				saveFile.WriteLine("\t"+nation.Key+" = "+nation.Value);
			}
			saveFile.WriteLine("}");
			saveFile.WriteLine("RegionGDPMods = {");
			foreach(KeyValuePair<string, double> region in Main.resistanceRegionGDPModifiers) {
				saveFile.WriteLine("\t"+region.Key+" = "+region.Value);
			}
			saveFile.WriteLine("}");
			saveFile.WriteLine("ArmyTracker = {");
			foreach(KeyValuePair<string, TIArmyState> army in Main.armyTracker) {
				saveFile.WriteLine("\t"+army.Key+" = "+army.Value.homeNation);
			}
			saveFile.WriteLine("}");
			saveFile.WriteLine("ArmyStrengthTracker = {");
			foreach(KeyValuePair<string, float> army in Main.armyStrengthTracker) {
				saveFile.WriteLine("\t"+army.Key+" = "+army.Value);
			}
			saveFile.WriteLine("}");
			saveFile.WriteLine("ArmyTypeTracker = {");
			foreach(KeyValuePair<string, int> army in Main.armyTypeTracker) {
				saveFile.WriteLine("\t"+army.Key+" = "+army.Value);
			}
			saveFile.WriteLine("}");
			saveFile.Close();
		}
	}	

	public static class makeShiftHeader {
		public static string[] makeShiftSplit(string givenStr, string delimiter) {
			if (givenStr == null)
				throw new ArgumentNullException(nameof(givenStr));
			if (delimiter == null)
				throw new ArgumentNullException(nameof(delimiter));
			if (delimiter == string.Empty)
				throw new ArgumentException("Delimiter cannot be an empty string.", nameof(delimiter));

			delimiter = System.Text.RegularExpressions.Regex.Unescape(delimiter);;

			List<string> result = new List<string>();
			int start = 0;
			int index;

			while ((index = givenStr.IndexOf(delimiter, start)) != -1)
			{
				result.Add(givenStr.Substring(start, index - start));
				start = index + delimiter.Length;
			}

			// Add the last segment
			result.Add(givenStr.Substring(start));

			return result.ToArray();
		}

		public static string[] fmSplit(string givenStr, string delimiter) {
			string[] tempStr = makeShiftSplit(givenStr, delimiter);
			List<string> strArr = new List<string>();;
			foreach (string item in tempStr)
			{
				if (item == null || item.Equals(delimiter) || item == "") {
					// FileLog.Log("-NOT Adding ["+item+"]");
				}
				else {
					// FileLog.Log("-Adding ["+item+"]");
					strArr.Add(item);
				}
			}
			return strArr.ToArray();
		}

		public static string parsePrep(string givenStr) {
			// FileLog.Log("-Parse Prep fed: [["+givenStr+"]]");
			string newStr = "";
			int numDecimals = 0;
			foreach(char chr in givenStr) {
				if (chr.Equals(',') || chr.Equals('.') || chr.Equals('0') || chr.Equals('1')|| chr.Equals('2')|| chr.Equals('3')|| chr.Equals('4')|| chr.Equals('5')|| chr.Equals('6') || chr.Equals('7') || chr.Equals('8') || chr.Equals('9')) {
					if (chr.Equals('.') && numDecimals == 0) {
						newStr += chr;
						numDecimals += 1;
					}
					else if (!chr.Equals('.')) {
						newStr+=chr;
					}
				}
			}
			// FileLog.Log("-Parse Prepped result is [["+newStr+"]]");
			return newStr;
		}
	}

	[HarmonyPatch(typeof(LoadMenuController), nameof(LoadMenuController.LoadSaveFile))]
	public static class loadSaveHeaderPatch {
		[HarmonyPostfix]
		public static void loadSavePatch(LoadMenuController __instance) {
			// FileLog.Log("LoadSaveFile was called");
			LoadSaveButton saveButton = __instance.saveList.selectedButton;
			if (saveButton == null) {
				// FileLog.Log("Given invalid button");
				return;
			}
			else {
				// FileLog.Log("We have stepped into the LoadSaveFile Method");
				string saveFileName = saveButton.saveInfo.name+"\u200B";
				string saveFileName2 = saveButton.saveInfo.name;
				// FileLog.Log("Fed Save Name: ["+saveFileName+"]");
				// FileLog.Log(Environment.CurrentDirectory);
				string tempPath = Environment.CurrentDirectory;
				string newPath = "";
				foreach(char chr in tempPath) {
					if (chr.Equals('\\')) {
						newPath += '/';
					}
					else {
						newPath += chr;
					}
				}
				FileLog.Log("[factionMissions] Loading faction data from: "+tempPath+"/Mods/factionMissionData/"+saveFileName+".txt");
				StreamReader reader;
				try {
					reader = new StreamReader(tempPath+"/Mods/factionMissionData/"+saveFileName+".txt");
				}
				catch (FileNotFoundException exceptionGiven) {
					try {
						reader = new StreamReader(tempPath+"/Mods/factionMissionData/"+saveFileName2+".txt");
					}
					catch (FileNotFoundException) {
						FileLog.Log("[factionMissions] File: "+tempPath+"/Mods/factionMissionData/"+saveFileName+".txt could not be located. No data loaded.");
						return;
					}
				}
				// string test = "testMe\tnow";
				// FileLog.Log(test);
				
				// string[] test2 = test.makeshiftSplit("\t").Cast<string>().ToArray();

				// FileLog.Log("Made it past splitter");
				bool inTopLevel = true;
				bool inSubLevel = false;
				int inType = -1;
				int cellLevel = 0;
				string lineToRead = reader.ReadLine();
				// We need to clear all info in the dictionaries
				Main.resistanceCellNetworksMacro = null;
				Main.resistanceCellNetworksMicro = null;
				Main.resistanceNationNetworkSize = null;
				Main.resistanceRegionNetworkSize = null;
				Main.resistanceRegionArms = null;
				Main.resistanceRegionGDPModifiers = null;
				Main.armyTracker = null;
				Main.armyStrengthTracker = null;
				Main.armyTypeTracker = null;
				// FileLog.Log("Loading: "+"mods/factionMissionData/"+saveFileName+".txt");
				Dictionary<String, TINationState> cachedLookup = GameStateManager.NationLookup();
				while (lineToRead != null) {
					// FileLog.Log("Given Line: [[["+lineToRead+"]]]");
					if (inTopLevel) {
						string[] splitted = makeShiftHeader.fmSplit(lineToRead, " = {");
						//string[] splitted = {lineToRead, " = {"};
						if (splitted.Length > 0) {
							// FileLog.Log("Entering the Top Level Header");
							if (splitted[0].Equals("RegionMicroController")) {
								inType = 0;
								if (Main.resistanceCellNetworksMicro == null) {
									// FileLog.Log("-Creating new [resistanceCellNetworksMicro]");
									Main.resistanceCellNetworksMicro = new Dictionary<int, Dictionary<string, TIRegionState>>();
								}
							}
							else if (splitted[0].Equals("RegionMacroController")) {
								inType = 1;
								if (Main.resistanceCellNetworksMacro == null) {
									// FileLog.Log("-Creating new [resistanceCellNetworksMacro]");
									Main.resistanceCellNetworksMacro = new Dictionary<int, Dictionary<string, TINationState>>();
								}

							}
							else if (splitted[0].Equals("RegionCellValue")) {
								inType = 2;
								if (Main.resistanceRegionNetworkSize == null) {
									// FileLog.Log("-Creating new [resistanceRegionNetworkSize]");
									Main.resistanceRegionNetworkSize = new Dictionary<string, int>();
								}

							}
							else if (splitted[0].Equals("NationCellValue")) {
								inType = 3;
								if (Main.resistanceNationNetworkSize == null) {
									// FileLog.Log("-Creating new [resistanceNationNetworkSize]");
									Main.resistanceNationNetworkSize = new Dictionary<string, int>();
								}

							}
							else if (splitted[0].Equals("RegionArms")) {
								inType = 4;
								if (Main.resistanceRegionArms == null) {
									// FileLog.Log("-Creating new [resistanceRegionArms]");
									Main.resistanceRegionArms = new Dictionary<string, float>();
								}

							}
							else if (splitted[0].Equals("RegionGDPMods")) {
								inType = 5;
								if (Main.resistanceRegionGDPModifiers == null) {
									// FileLog.Log("-Creating new [resistanceRegionGDPModifiers]");
									Main.resistanceRegionGDPModifiers = new Dictionary<string, double>();
								}
							}
							else if (splitted[0].Equals("ArmyTracker")) {
								inType = 6;
								if (Main.armyTracker == null) {
									// FileLog.Log("-Creating new [armyTracker]");
									Main.armyTracker = new Dictionary<string, TIArmyState>();
								}
							}
							else if (splitted[0].Equals("ArmyStrengthTracker")) {
								inType = 7;
								if (Main.armyStrengthTracker == null) {
									// FileLog.Log("-Creating new [armyStrengthTracker]");
									Main.armyStrengthTracker = new Dictionary<string, float>();
								}
							}
							else if (splitted[0].Equals("ArmyTypeTracker")) {
								inType = 8;
								if (Main.armyTypeTracker == null) {
									// FileLog.Log("-Creating new [armyTypeTracker]");
									Main.armyTypeTracker = new Dictionary<string, int>();
								}
							}
							else {
								// FileLog.Log("Splitted[0] ("+splitted[0]+") did not match any of the provided conditions");
							}

							if (inType != -1) {
								inTopLevel = false;
							}
						}
					}
					else {
						string[] preSplit = makeShiftHeader.fmSplit(lineToRead, "\t");
						string[] splitted = makeShiftHeader.fmSplit(preSplit[0], " = ");
						if (lineToRead.Contains(" = {")) {
							// FileLog.Log("-Entering the subHeader");
							// string[] temp = makeShiftHeader.fmSplit(lineToRead, "\t");
							// foreach(string str in temp) {
							// 	FileLog.Log("--"+temp.Length+" fmSplit returned [["+str+"]]");
							// }
							if (!splitted[0].Equals("}")) {
								cellLevel = int.Parse(makeShiftHeader.parsePrep(splitted[0]));
								inSubLevel = true;
								// if (inType == 0) {
								// 	if(!Main.resistanceCellNetworksMicro.ContainsKey(cellLevel)) {
								// 		// FileLog.Log("-Creating new Cell Level: ["+cellLevel+"]"+" in[resistanceCellNetworksMicro]");
								// 		Main.resistanceCellNetworksMicro[cellLevel] = new Dictionary<string, TIRegionState>();
								// 	}
								// }
								// else if (inType == 1) {
								// 	if(!Main.resistanceCellNetworksMacro.ContainsKey(cellLevel)) {
								// 		// FileLog.Log("-Creating new Cell Level: ["+cellLevel+"]"+" in[resistanceCellNetworksMacro]");
								// 		Main.resistanceCellNetworksMacro[cellLevel] = new Dictionary<string, TINationState>();
								// 	}
								// }
							}
							else {
								// FileLog.Log("-Leaving the subHeader");
								inSubLevel = false;
							}
						}
						else {
							if (inSubLevel) {
								string[] temp = makeShiftHeader.fmSplit(lineToRead, "\t");
								if (!temp[0].Equals("}")) {
									// if (inType == 0) {
									// 	// FileLog.Log("-Setting value of resistanceCellNetworksMicro["+cellLevel+"]["+temp[0]+"] to null");
									// 	Main.resistanceCellNetworksMicro[cellLevel][temp[0]] = null;
									// }
									// else if (inType == 1) {
									// 	// FileLog.Log("-Setting value of resistanceCellNetworksMacro["+cellLevel+"]["+temp[0]+"] to null");
									// 	Main.resistanceCellNetworksMacro[cellLevel][temp[0]] = null;
									// }
								}
								else {
									// FileLog.Log("-Leaving the subHeader");
									inSubLevel = false;
								}
							}
							else {
								preSplit = makeShiftHeader.fmSplit(lineToRead, "\t");
								splitted = makeShiftHeader.fmSplit(preSplit[0], " = ");
								if (!splitted[0].Equals("}")) {
									// FileLog.Log("-Reading a value");
									string parsePrepped = makeShiftHeader.parsePrep(splitted[1]);
									if (inType == 2) {
										// FileLog.Log("-Assigning value ["+int.Parse(parsePrepped)+"] to resistanceRegionNetworkSize["+splitted[0]+"]");
										Main.resistanceRegionNetworkSize[splitted[0]] = int.Parse(parsePrepped);
									}
									else if (inType == 3) {
										// FileLog.Log("-Assigning value ["+int.Parse(parsePrepped)+"] to resistanceNationNetworkSize["+splitted[0]+"]");
										Main.resistanceNationNetworkSize[splitted[0]] = int.Parse(parsePrepped);
									}
									else if (inType == 4) {
										// FileLog.Log("-Assigning value ["+int.Parse(parsePrepped)+"] to resistanceRegionArms["+splitted[0]+"]");
										Main.resistanceRegionArms[splitted[0]] = float.Parse(parsePrepped);
									}
									else if (inType == 5) {
										Main.resistanceRegionGDPModifiers[splitted[0]] = double.Parse(parsePrepped);
									}
									else if (inType == 6) {
										if (cachedLookup[splitted[1]] != null) {
											foreach (TIArmyState army in cachedLookup[splitted[1]].armies) {
												if ((army.displayName).Equals(splitted[0])) {
													Main.armyTracker.Add(splitted[0], army);
													break;
												}
											}
										}
									}
									else if (inType == 7) {
										if (Main.armyTracker != null && Main.armyTracker[splitted[0]] != null) {
											Main.armyStrengthTracker[splitted[0]] = float.Parse(parsePrepped);
										}
									}
									else if (inType == 8) {
										// ints default to 0 if null.
										if (Main.armyTypeTracker != null) {
											Main.armyTypeTracker[splitted[0]] = int.Parse(parsePrepped);
										}
									}
								}
								else {
									// FileLog.Log("-Leaving Header-- returning to top level");
									inTopLevel = true;
								}
							}
						}
					}
					lineToRead = reader.ReadLine();
				}

				reader.Close();

			}
		}
	}	
}
