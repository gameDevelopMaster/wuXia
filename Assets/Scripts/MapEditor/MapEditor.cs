using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
public class MapEditor : MonoBehaviour 
{
	public enum ToolType
	{
		None,
		Erase,
		Square,
		Block,
		BlockStone,
		BlockGlass,
		Save
	}
	public static MapEditor Instance				= null;
	
	//Chekc
	public bool isEnabled 							= false;	
	
	//Max column of map
	public static int maxCol 						= 8;
	
	//Max row of map
	public static int maxRow 						= 10;
	
	//Start point of map
	public static int MAP_START_Y 					= -300;
	
	public static int MAP_START_X					= -240;

	//Map is grid, so we will have one list of squares
	[HideInInspector]
	public List<Square> squares 					= new List<Square>();
	
	//List of object will be inactive when run map editor
	private List<GameObject> hiddenObjects 			= new List<GameObject>();
	
	//Array of map values
	private int[] map 								= new int[maxCol*maxRow];
	
	//Current tool user seclected
	private ToolType currentTool 					= ToolType.None;
	
	//Check if current game mode is limit move or limit time
	private bool isLimitMoveMode 					= true;
	
	//Number of block player need to pass
	private int blockNumber 						= 0;
	
	//Number of score player need to pass
	private string scoreRequest 					= "0";
	
	//Number of limitment
	private string limitAmount 						= "40";
	
	//File name to save info
	private string fileName							="1.txt";
	
	// Use this for initialization
	void Awake () 
	{
		Instance = this;
		//If in editor and map editor enable, disable all game parts
		if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		{
			if(isEnabled)
			{
				//Hide Gameplay object
				GameObject obj = GameObject.Find("GamePlay");
				obj.SetActive(false);
				hiddenObjects.Add(obj);
				
				//Hide MapManager object
			 	obj = GameObject.Find("MapManager");
				obj.SetActive(false);
				hiddenObjects.Add(obj);
				
				//Hide SoundEffect object
				obj = GameObject.Find("SoundEffect");
				obj.SetActive(false);
				hiddenObjects.Add(obj);
				
				//Set all map to 1 (mean all square not empty)
				for(int i=0; i<map.Length; i++)
				{
					map[i] = 1;
				}
				//Gen squares to edit
				GenSquares();
			}
		}
	
	}
	void GenSquares()
	{
		//Gen square by row and column
		for(int row = 0; row< maxRow; row++)
		{
			for(int col=0; col<maxCol; col++)
			{
				GenSquare(row,col);
			}
		}
	}
	public void GenSquare(int row, int col)
	{
		//Create new square at that row,column
		//Because gen to edit, so all square is normal
		Square square = new Square(row,col,SquareType.normal);
		//Set this square parent to Panel Editor
		square.SetParent(GameObject.Find("PanelEditor").transform);
		//Load square object for this square
		square.LoadObjects();
		//add square to squares list
		squares.Add(square);
	}
	
	// Update is called once per frame
	void OnGUI()
	{
		//Erase button
		if(GUI.Button(new Rect(10,80,80,40),"Erase"))
		{
			currentTool = ToolType.Erase;
		}
		//Normal button
		if(GUI.Button(new Rect(90,80,80,40),"Square"))
		{
			currentTool = ToolType.Square;
		}
		//Block button
		if(GUI.Button(new Rect(10,130,50,40),"Block"))
		{
			currentTool = ToolType.Block;
		}
		//Block stone button
		if(GUI.Button(new Rect(70,130,50,40),"Stone"))
		{
			currentTool = ToolType.BlockStone;
		}
		//Block glass button
		if(GUI.Button(new Rect(130,130,50,40),"Glass"))
		{
			currentTool = ToolType.BlockGlass;
		}
		string limitString = "Limit Move";

		isLimitMoveMode = GUI.Toggle(new Rect(10,200,160,40),isLimitMoveMode,"Limit Move");
		isLimitMoveMode = !GUI.Toggle(new Rect(90,200,160,40),!isLimitMoveMode, "Limit Time");
		string limitLabel = isLimitMoveMode?"Limit(moves)":"Limit(sec)";
		GUI.Label(new Rect(10,240,80,40),limitLabel);
		limitAmount = GUI.TextField(new Rect(90,240,80,30),limitAmount);
	
		GUI.Label(new Rect(10,280,80,40),"Score");
		scoreRequest = GUI.TextField(new Rect(90,280,80,30),scoreRequest);
		
		GUI.Label(new Rect(10,320,80,40),"Block Num");
		GUI.TextField(new Rect(90,320,80,30),""+blockNumber);
		
		GUI.Label(new Rect(10,360,80,40),"File Name");
		fileName = GUI.TextField(new Rect(90,360,80,30),fileName);
		//Save and test
		if(GUI.Button(new Rect(10,400,160,40),"Save & Test"))
		{
			if(!fileName.Contains(".txt")) fileName += ".txt";
			SaveMap(fileName);
			//Remove all squares
			for(int i=0; i<squares.Count; i++)
			{
				squares[i].RemoveAllObjects();
			}
			squares.Clear();
			//active all hidden objets (gameplay, map manager, sound effects)
			foreach(GameObject obj in hiddenObjects)
			{
				obj.SetActive(true);
			}
			gameObject.SetActive(false);
			//set current tool is none
			currentTool = ToolType.None;
			//set game mode is testing
			GamePlay.isTesting = true;
		}
	}
	//This function will be called in SquareObject when user click on
	//square in edit mode
	public void ClickOnSquare(Square square)
	{
		if(square.type == SquareType.block || square.type == SquareType.block_stone)
				blockNumber--;
		//If current tool is erase, set square to empty
		//And remove all objects in square
		if(currentTool == ToolType.Erase)
		{
			square.type = SquareType.empty;
			square.RemoveAllObjects();
		}
		//If current tool is block, set square type to block
		//then remove all objets, and load objects again by square type
		else if(currentTool == ToolType.Block)
		{
			square.type = SquareType.block;
			square.RemoveAllObjects();
			square.LoadObjects();
			blockNumber++;
			
		}
		//If current tool is block, set square type to block
		//then remove all objets, and load objects again by square type
		else if(currentTool == ToolType.BlockStone)
		{
			square.type = SquareType.block_stone;
			square.RemoveAllObjects();
			square.LoadObjects();
			blockNumber++;
		}
		else if(currentTool == ToolType.BlockGlass)
		{
			square.type = SquareType.block_glass;
			square.RemoveAllObjects();
			square.LoadObjects();
			blockNumber++;
		}
		//If current tool is square, set square type to normal
		//then remove all objects, and load objects again by square type
		if(currentTool == ToolType.Square)
		{
			square.type = SquareType.normal;
			square.RemoveAllObjects();
			square.LoadObjects();
		}
	}
	public void SaveMap(string fileName)
	{
		string saveString="";
		//Create save string
		saveString += isLimitMoveMode?"GM 0":"GM 1";
		saveString += "\r\n";
		saveString += "LMT " + limitAmount;
		saveString += "\r\n";
		saveString += "MNS " + scoreRequest + "/" + blockNumber + "/0";
		saveString += "\r\n";
		
		LevelData.limitAmount = int.Parse(limitAmount);
		
		LevelData.requestMissions.Clear();
		
		LevelData.requestMissions.Add(new Mission(int.Parse(scoreRequest.Trim()),MissionType.Score));
		
		LevelData.requestMissions.Add(new Mission(blockNumber,MissionType.Block));
		
		//set map data
		for(int row=0; row<maxRow; row++)
		{
			for(int col=0; col<maxCol; col++)
			{
				//Get square at [row,col]
				Square square = squares[row*maxCol+col];	
				//Set data to test
				LevelData.map[row*maxCol+col] = (int)square.type;
				//Save square type to save string
				saveString += (int)square.type;
				//if this column not yet end of row, add space between them
				if(col < (maxCol-1)) saveString += " ";					
			}
			//if this row is not yet end of row, add new line symbol between rows
			if(row <(maxRow-1)) saveString += "\r\n";
		}
		if(Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
		{
			//Write to file
			string activeDir = Application.dataPath + @"/Resources/Maps/";
			string newPath = System.IO.Path.Combine(activeDir, fileName);
		    StreamWriter sw = new StreamWriter(newPath);
		    sw.Write(saveString);			
		    sw.Close();
		}
	}
}
