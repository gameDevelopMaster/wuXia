using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameState
{
	WaitConfirm,
	Ready,
	LoadLevel,
	Playing,
	GameOver,
}
public class GamePlay : MonoBehaviour 
{
	//Instance of this class, other class can call it anytime without find component
	public static GamePlay Instance 				= null;
	
	public static bool isTesting 					= false;
	
	public int currentLevel 						= 0;
	
	//Label for score
	public UISprite spriteNoMatchMore;
	public UISprite spriteGameOver;
	public UISprite spriteBlock;
	public UILabel 	scoreLabel;
	public UILabel 	moveNumber;
	public UILabel  blockNumber;
	
	//Each candy/jewel/... 
	//we call it an item, this is selected item
	[HideInInspector]
	public Item selectedItem 						= null;
	
	//Check if block input or not
	[HideInInspector]
	public bool isBlockedInput 						= false;

	//Current game state
	[HideInInspector]
	public GameState currentState 					= GameState.WaitConfirm;
	
	//After 10 senconds, 
	//we will automaticly hint player next move
	[HideInInspector]
	public float timeHintNextMove 					= 10;
	
	//Count how many explosion
	[HideInInspector]
	public int explosionCount 						= 0;
	
	//Count how many combo
	[HideInInspector]
	public int comboCount 							= 0;
	
	//In comboCountTime, if comboCount > 3, 
	//player will get excellent
	[HideInInspector]
	public float comboCountTime						= 5;
	
	//Object to store user informations(scores)
	private MissionManager missionManager			= null;
	//Speed of touch on the screen
	private Vector3 touchSpeed 						= Vector3.zero;
	//Move direction of touch
	private Vector2 moveDirection 					= Vector2.zero;
	//List of hinted items
	private List<Item> hintItems 					= new List<Item>();
	
	private float gameOverTime						= 0;
	//Start game, we assign instance of this class
	void Awake()
	{
		Instance = this;
		//Initialize for player score
		missionManager = new MissionManager();
	}
	void Start()
	{
		//Start level 0
		StartLevel(currentLevel);
	}
	void StartLevel(int level)
	{
		//Start game, show level information wait player confirm
		currentState = GameState.WaitConfirm;
		currentLevel = level;
		//Load infomation, if run in edit mode, not load data from local
		if(!isTesting)
		{
			LevelData.LoadDataFromLocal(currentLevel);
		}
		//reset score to 0 for all mission
		missionManager.InitScore(LevelData.requestMissions);
		
		//hide block sprite and number if don't have block mission
		if(LevelData.GetMission(MissionType.Block) == null)
		{
			spriteBlock.gameObject.SetActive(false);
			blockNumber.gameObject.SetActive(false);
		}
	}
	void  Update()
	{
		switch(currentState)
		{
			case GameState.WaitConfirm:
			{
				//Wait For input touch
				currentState = GameState.Ready;
				Map.Instance.RemoveMap();
				Map.Instance.LoadMap(LevelData.map);
				currentState = GameState.Playing;
				break;
			}
			case GameState.Playing:
			{
				//Only check touch move when we already select item and not yet moving
				if(selectedItem != null && !isBlockedInput)
				{
					GetInput();
				}
				CheckHint();
				CheckCombos();
				DisplayScore();
				break;
			}
			case GameState.GameOver:
			{
				StartLevel(currentLevel);
				break;
			}
		}
	}
	
	/// <summary>
	/// Gets the input.
	/// </summary>
	void GetInput()
	{
		//Get touch speed of mouse, change this code if you move project to mobile
		touchSpeed = new Vector3(Input.GetAxis("Mouse X"),Input.GetAxis("Mouse Y"));
		Debug.Log(touchSpeed);
		//If move of touch ins't zero
		if(touchSpeed.magnitude > 0)
		{
			//Get moving direction, with condition if moveX > moveY, direction will be X axis
			//otherwise
			moveDirection = Vector3.zero;
			if(Mathf.Abs(touchSpeed.x) >= Mathf.Abs(touchSpeed.y))
			{
				moveDirection.y = 0;
				moveDirection.x = (int)(touchSpeed.x/Mathf.Abs(touchSpeed.x));
			}
			else
			{
				moveDirection.x = 0;
				moveDirection.y = (int)(touchSpeed.y/Mathf.Abs(touchSpeed.y));
			}
			//After we got moving direction, 
			//let move selected item with that direction
			selectedItem.Swap(moveDirection);
		}
	}
	#region "Hint move"
	void CheckHint()
	{
		if(timeHintNextMove >0)
		{
			timeHintNextMove-=Time.deltaTime;
			if(timeHintNextMove <=0)
			{
				//If have hint items, show them
				if(hintItems.Count > 0)
				{
					//Show next move
					Hint();
				}
				
			}
		}
	}
	//show hint items
	void Hint()
	{
		foreach(Item item in hintItems)
		{
			item.Hint();
		}
	}
	/// <summary>
	/// un-hint.
	/// </summary>
	public void UnHint()
	{
		//Reset time to hint next move to 10 seconds again
		timeHintNextMove = 10;
		//Remove effect of hint items
		foreach(Item item in hintItems)
		{
			item.UnHint();
		}
	}
	public void FindHintItems()
	{
		if(currentState == GameState.Playing)
		{
			StartCoroutine(OnFindHintItems());
		}
	}
	IEnumerator OnFindHintItems()
	{
		hintItems = Map.Instance.FindNextMove();
		if(hintItems.Count == 0)
		{
			//Display no more match
			spriteNoMatchMore.GetComponent<Animation>().Play("NoMatchMore");
			yield return new WaitForSeconds(3f);
			Map.Instance.ReGenItems();
		}
		else
		{
			GamePlay.Instance.BlockInput(false);
		}
	}
	#endregion
	/// <summary>
	/// Checks the combos. In 5s, if have 2 explosions we will count it one combo. If have 2 combos, we count it
	/// as excellent
	/// </summary>
	void CheckCombos()
	{
		if(comboCountTime > 0)
		{
			comboCountTime -= Time.deltaTime;
			if(comboCountTime > 0)
			{
				//If explosion time is greater than 2, play sound sweet,
				//add 1 to comboCount
				if(explosionCount >= 2)
				{
					SoundEffect.Instance.PlaySound(SoundEffect.Combo,0);
					explosionCount = 0;
					comboCount++;
				}
				//if comboCount is >=2, play sound "excellent",
				//and reset comboCount
				if(comboCount >= 2)
				{
					SoundEffect.Instance.PlaySound(SoundEffect.Combo,1);
					comboCount = 0;
				}
			}
			else
			{
				//reset combo count 
				comboCountTime = 5;
				comboCount = 0;
				explosionCount = 0;
			}
		}
	}
	/// <summary>
	/// Updates the score.
	/// </summary>
	void DisplayScore()
	{
		Mission scoreMission = missionManager.GetMission(MissionType.Score);
		if(scoreMission != null)
			scoreLabel.text = ""+scoreMission.amount;
		Mission blockMission = missionManager.GetMission(MissionType.Block);
		Mission requestBlockMission = LevelData.GetMission(MissionType.Block);
		if(blockMission != null)
			blockNumber.text = blockMission.amount + "/" + requestBlockMission.amount;
		moveNumber.text = ""+LevelData.limitAmount;
	}
	/// <summary>
	/// Check win or loose
	/// </summary>
	public void CheckWin()
	{
		StartCoroutine(OnCheckWin());
	}
	IEnumerator OnCheckWin()
	{
		yield return new WaitForSeconds(1.5f);
		if(LevelData.limitAmount <= 0 && !MissionManager.Instance.IsWin())
		{
			isBlockedInput = true;
			spriteGameOver.GetComponent<Animation>().Play("GameOver");
			SoundEffect.Instance.PlaySoundRandom(SoundEffect.GameOver);
			yield return new WaitForSeconds(5f);
			//Game over
			currentState = GameState.GameOver;
			gameOverTime = 0;
		}
		else if(LevelData.limitAmount >=0 && MissionManager.Instance.IsWin())
		{
			StartLevel(currentLevel++);
		}
		else
		{
			//If game not yet finish, find hint items
			FindHintItems();
		}
	}
	public void BlockInput(bool block)
	{
		isBlockedInput = block;
	}
}
