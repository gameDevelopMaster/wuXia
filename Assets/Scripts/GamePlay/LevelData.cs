using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
public enum GameMode
{
	TimeLimit,
	MoveLimit,
}
public class LevelData 
{
	public static LevelData Instance;
	
	//Map data is an array of int (8x10 = 80 elements), 
	//0 mean square will be empty (don't have any item in it)
	//1 mean square will have an item in that
	//You can change this map data to create many maps. Enjoy!
	public static int[] map = new int[]{0,0,1,1,1,1,0,0,
								 0,0,1,1,1,1,0,0,
							     1,1,3,3,3,3,1,1,
								 2,2,3,3,3,3,3,2,
								 2,2,2,0,0,2,2,2,
								 2,2,2,0,0,2,2,2,
								 2,3,3,3,3,3,3,2,
								 3,3,3,3,3,3,3,1,
								 0,0,1,1,1,1,0,0,
								 0,0,1,1,1,1,0,0};
	/*//You can open these map to test. Enjoy ^^!
	public int[] map = new int[]{1,1,1,1,1,1,1,1,
								 1,1,1,1,1,1,1,1,
							     1,1,1,1,1,1,1,1,
								 0,0,1,1,1,1,0,0,
								 0,0,1,1,1,1,0,0,
								 0,0,1,1,1,1,0,0,
								 0,0,1,1,1,1,0,0,
								 1,1,1,1,1,1,1,1,
								 1,1,1,1,1,1,1,1,
								 1,1,1,1,1,1,1,1};
	public int[] map = new int[]{0,0,0,0,0,0,0,1,
								 0,0,0,0,0,0,1,1,
							     0,0,0,0,0,1,1,1,
								 0,0,0,0,1,1,1,1,
								 0,0,0,1,1,1,1,1,
								 0,0,1,1,1,1,1,1,
								 0,1,1,1,1,1,1,1,
								 1,1,1,1,1,1,1,1,
								 0,0,0,0,0,0,0,0,
								 0,0,0,0,0,0,0,0};*/
	//List of mission in this map
	public static List<Mission> requestMissions = new List<Mission>();
	public static GameMode mode = GameMode.MoveLimit;
	public static int limitAmount = 40;
	public static void LoadDataFromLocal(int currentLevel)
	{
		//Read data from text file
		TextAsset mapText = Resources.Load("Maps/" + currentLevel) as TextAsset;
		ProcessGameDataFromString(mapText.text);
	}
	public static void LoadDataFromURL(int currentLevel)
	{
		//Read data from your server, if you want
	}
	static void ProcessGameDataFromString(string mapText)
	{
		//Structure of text file like this:
		//1st: Line start with "GM". This is game mode line (0-Move Limit, 1-Time Limit)
		//2nd: Line start with "LMT" is limit amount of play time (time of move or seconds depend on game mode)
			   //Ex: LMT 20  mean player can move 20 times or 20 seconds, depend on game mode
		//3rd: Line start with "MNS" is missions line. This is amount ofScore/Block/Ring/... 
			   //Ex: MNS 10000/24/0' mean user need get 1000 points, 24 block, and not need to get rings.
		//4th:Map lines: This is an array of square types.
		//First thing is split text to get all in arrays of text
		string[] lines = mapText.Split(new string[]{"\n"},StringSplitOptions.RemoveEmptyEntries);
		
		int mapLine = 0;
		foreach(string line in lines)
		{
			//check if line is game mode line
			if(line.StartsWith("GM"))
			{
				//Replace GM to get mode number, 
				string modeString = line.Replace("GM",string.Empty).Trim();
				//then parse it to interger
				int modeNum = int.Parse(modeString);
				//Assign game mode
				mode = (GameMode)modeNum;
			}
			else if(line.StartsWith("LMT"))
			{
				//Replace LTM to get limit number, 
				string amountString = line.Replace("LMT",string.Empty).Trim();
				//then parse it to interger and assign to limitAmount
				limitAmount = int.Parse(amountString);
			}
			//check third line to get missions
			else if(line.StartsWith("MNS"))
			{
				//Replace 'MNS' to get mission numbers
				string missionString = line.Replace("MNS",string.Empty).Trim();
				//Split again to get mission numbers
				string[] missionNumbers = missionString.Split(new string[]{"/"},StringSplitOptions.RemoveEmptyEntries);
				for(int i=0; i<missionNumbers.Length;i++)
				{
					//Set scores of mission and mission type
					int amount = int.Parse(missionNumbers[i].Trim());
					MissionType type = (MissionType)i;
					requestMissions.Add(new Mission(amount,type));
				}
			}
			else //Maps
			{
				//Split lines again to get map numbers
				string[] squareTypes = line.Split(new string[]{" "},StringSplitOptions.RemoveEmptyEntries);
				for(int i=0; i < squareTypes.Length; i++)
				{
					map[mapLine * Map.maxCol + i] = int.Parse(squareTypes[i].Trim());
				}
				mapLine++;
			}
		}
	}
	public static Mission GetMission(MissionType type)
	{
		return requestMissions.Find(obj=>obj.type == type);
	}
}
