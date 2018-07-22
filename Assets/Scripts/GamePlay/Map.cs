using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Map:MonoBehaviour
{
	//Instance of this class, so we can access map anywhere by Map.Instance
	public static Map Instance;
	//Max column of map
	public static int maxCol = 8;
	//Max row of map
	public static int maxRow = 10;
	
	//Start point of map
	public static int MAP_START_Y 	= -300;
	public static int MAP_START_X	= -240;
	
	//Map is grid, so we will have one list of squares
	[HideInInspector]
	public List<Square> squares = new List<Square>();
	
	int[] map;
	// Use this for initialization
	void Awake () 
	{
		Instance = this;
	}
	public void LoadMap(int[] pMap)
	{
		map = pMap;
		//Start to gen map
		GenSquares();
		StartCoroutine(GenItems());
	}
	/// <summary>
	/// Generate Map
	/// </summary>
	/// <returns>
	/// The map.
	/// </returns>
	void GenSquares()
	{
		for(int i = 0; i< maxRow; i++)
		{
			//per row, we will delay 0.1s, so row will appear step by step
			for(int j=0; j<maxCol; j++)
			{
				//per column, we will delay 0.05s, so in one row, items will appear step by step
				GenSquare(i,j);
			}
		}
	}
	
	/// <summary>
	/// Generate the square at [row,column]
	/// </summary>
	/// <param name='row'>
	/// Row.
	/// </param>
	/// <param name='col'>
	/// Col.
	/// </param>
	public void GenSquare(int row, int col)
	{
		//Create new square at that row,column
		Square square = null;
		//If at that point, map data is 1, gen non-empty item
		//otherwise, gen empty item
		int mapValue = map[row*maxCol + col];
		
		SquareType type = (SquareType)mapValue;
		
		square = new Square(row,col,type);
		square.SetParent(GameObject.Find("Panel").transform);
		if(type != SquareType.empty)
			square.LoadObjects();
		//add square to squares list
		squares.Add(square);
	}
	IEnumerator GenItems()
	{	
		GamePlay.Instance.BlockInput(true);
		//Gen square at row i, column j
		for(int i = 0; i< maxRow; i++)
		{
			//per row, we will delay 0.1s, so row will appear step by step
			yield return new WaitForSeconds(0.1f);
			for(int j=0; j<maxCol; j++)
			{
				Square square = GetSquare(i,j);
				if(square.CanMoveItemIn())
				{
					//per column, we will delay 0.05s, so in one row, items will appear step by step
					yield return new WaitForSeconds(0.05f);
					GenItem(square).DropIn(square);
				}
			}
		}
		CheckCombine();
	}
	public void ReGenItems()
	{
		StartCoroutine(OnReGenItems());
	}
	IEnumerator OnReGenItems()
	{
		GamePlay.Instance.BlockInput(true);
		//Gen square at row i, column j
		for(int i = 0; i< maxRow; i++)
		{
			//per row, we will delay 0.1s, so row will appear step by step
			for(int j=0; j<maxCol; j++)
			{
				Square square = GetSquare(i,j);
				if(square.item != null)
				{
					//per column, we will delay 0.05s, so in one row, items will appear step by step
					GenItem(square).Replace(square.item);
				}
			}
		}
		yield return new WaitForSeconds(1.75f);
		CheckCombine();
	}
	/// <summary>
	/// Generate item for square.
	/// </summary>
	/// <param name='square'>
	/// Square.
	/// </param>
	public Item GenItem(Square square)
	{
		//Rule to generate items: We won't generate item that immediately make combination
		//with available items.
		//Example: call the square is "X", if we have a item type "A", and now map is like this
		//A-A-X 
		//or 
		//X-A-A, 
		//or
		/*X
		  A
		  A*/
		//we won't gen "A" type for this square
		int row = square.row;
		int col = square.col;
		List<ItemType> remainTypes = new List<ItemType>();
		for(int i = 0; i < Enum.GetNames(typeof(ItemType)).Length;i++)
		{
			bool canGen = true;
			if(col > 1)
			{
				Square neighbor = GetSquare(row,col-1);
				if(neighbor.CanMoveItemOut() && neighbor.item.type == (ItemType)i)
					canGen = false;
			}
			if(col < maxCol-1)
			{
				Square neighbor = GetSquare(row,col+1);
				if(neighbor.CanMoveItemOut() && neighbor.item.type == (ItemType)i)
					canGen = false;
			}
			if(row > 1)
			{
				Square neighbor = GetSquare(row-1,col);
				if(neighbor.CanMoveItemOut() && neighbor.item.type == (ItemType)i)
					canGen = false;
			}
			if(canGen)
			{
				remainTypes.Add((ItemType)i);
			}
		}
		//Random generate type from remain types we can 
		int index = UnityEngine.Random.Range(0,remainTypes.Count);
		ItemType type = remainTypes[index];
		//Instaniate item's game object
		Item item = new Item(type);
		//Set start position
		item.SetPosition(new Vector3(square.PositionX,square.PositionY + 600,square.PositionZ));
		return item;
	}
	public Square GetSquare(int row, int col)
	{
		return squares[row * maxCol + col];
	}
	/// <summary>
	/// Check combination of current map
	/// </summary>
	/// <returns>
	/// The combine.
	/// </returns>
	public bool CheckCombine()
	{
		List<Combine> combines = new List<Combine>();
		//Check combinations by type
		for(int itemType=0; itemType< Enum.GetNames(typeof(ItemType)).Length; itemType++)
		{
			//Check all combination in horizontal direction
			for(int row=0; row<maxRow; row++)
			{
				List<Item> subCombines = new List<Item>();
				for(int col=0; col< maxCol; col++)
				{
					//Get current square
					Square square = GetSquare(row,col);
					if(!square.CanMoveItemOut())
					{
						if(subCombines.Count >= 3)
						{
							Combine combine = new Combine();
							combine.AddItems(subCombines);
							combines.Add(combine);
						}
						subCombines.Clear();
						continue;
					}
					//If current square is same type with current-checking item type
					//add it to subCombines
					if(square.item.type == (ItemType)itemType)
					{
						subCombines.Add(square.item);
					}
					//Until current square is out of map or current square's item type not 
					//same with checking type, if subCombines is > 3 elements, 
					//add subcombines to list combines
					if(square.item.type != (ItemType)itemType || col == maxCol-1)
					{
						if(subCombines.Count >= 3)
						{
							Combine combine = new Combine();
							combine.AddItems(subCombines);
							combines.Add(combine);
						}
						subCombines.Clear();
					}
				}
				
			}
			//Check all combination in horizontal direction, same with check all horizontal
			for(int col=0; col< maxCol; col++)
			{
				List<Item> subCombines = new List<Item>();
				for(int row=0; row < maxRow; row++)
				{
					Square square = GetSquare(row,col);
					if(!square.CanMoveItemOut())
					{
						if(subCombines.Count >= 3)
						{
							Combine combine = new Combine();
							combine.AddItems(subCombines);
							combines.Add(combine);
						}
						subCombines.Clear();
						continue;
					}
					if(square.item.type == (ItemType)itemType)
					{
						subCombines.Add(square.item);
					}
					if(square.item.type != (ItemType)itemType || row == maxRow-1)
					{
						if(subCombines.Count >= 3)
						{
							Combine combine = new Combine();
							combine.AddItems(subCombines);
							combines.Add(combine);
						}
						subCombines.Clear();
					}
				}
			}
			
		}
		//If can found combinations, disappear hint effect, count explosion,
		//and remove all combinations' item
		if(combines.Count > 0)
		{
			GamePlay.Instance.explosionCount++;
			GamePlay.Instance.UnHint();
			StartCoroutine(RemoveCombines(combines));
			return true;
		}
		else
		{
			//Reset selected item
			GamePlay.Instance.selectedItem = null;
			GamePlay.Instance.CheckWin();
		}
		return false;
	}
	
	//Remove all items in combination, then move all item down, 
	//after move all item down, check combinations again
	IEnumerator RemoveCombines(List<Combine> combines)
	{
		//Remove all items in combinations
		int count = combines.Count;
		while(count > 0)
		{
			count--;
			//destroy combine
			combines[count].Destroy();
			//remove combine from combinations list
			combines.RemoveAt(count);
		}
		//Play sound for explosion
		SoundEffect.Instance.PlaySoundRandom(SoundEffect.Explosion);
		yield return new WaitForSeconds(0.25f);
		//Move all item down
		for(int row=0; row<maxRow; row++)
		{
			for(int col=0; col<maxCol; col++)
			{
				//If current square is null, move above neighbor down
				if(GetSquare(row,col).CanMoveItemIn())
				{
					for(int k = row+1; k<maxRow;k++)
					{
						Square square = GetSquare(k,col);
						if(!square.CanMoveItemThrough())
							break;
						if(square.CanMoveItemOut())
						{
							square.item.DropIn(GetSquare(row,col));
							square.item = null;
							break;
						}
					}
				}
			}
		}
		yield return new WaitForSeconds(0.25f);
		//Re-generate items for all squares
		for(int col=0; col<maxCol;col++)
		{
			for(int row=maxRow-1; row>0;row--)
			{
				Square square = GetSquare(row,col);
				if(!square.CanMoveItemThrough())
					break;
				if(square.CanMoveItemIn())
				{
					GenItem(square).DropIn(square);
				}
			}
		}
		//yield return new WaitForSeconds(0.5f);
		//After all check map again until cannot found combination
		CheckCombine();
	}
	
	/// <summary>
	/// Automaticly find all items can make one combination to hint player
	/// </summary>
	/// <returns>
	/// The next move.
	/// </returns>
	public List<Item> FindNextMove()
	{
		List<Item> nextMoveItems = new List<Item>();
		//Check next move by type,
		for(int itemType=0; itemType< Enum.GetNames(typeof(ItemType)).Length; itemType++)
		{
			//Check squares around current square (called "x") 
			// "o" is type of item, if squares make a shape suitable, choose them
			for(int row=0; row<maxRow; row++)
			{
				for(int col=0; col< maxCol; col++)
				{
					Square square = GetSquare(row,col);
					if(!square.CanMoveItemOut())
					{
						nextMoveItems.Clear();
						continue;
					}
					//current square called x
					//o-o-x
					//	  o
					if(col > 1 && row > 0)
					{
						square = GetSquare(row-1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col-1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col-2);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count == 3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//    o
					//o-o x
					if(col > 1 && row < maxRow-1)
					{
						square = GetSquare(row+1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col-1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col-2);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count == 3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//x o o
					//o
					if(col < maxCol-2 && row > 0)
					{
						square = GetSquare(row-1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col+1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col+2);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count == 3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//o
					//x o o
					if(col < maxCol-2 && row <maxRow-1)
					{
						square = GetSquare(row+1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col+1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col+2);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count == 3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//o
					//o
					//x o
					if(col < maxCol-1 && row < maxRow-2)
					{
						square = GetSquare(row,col+1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row+1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row+2,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count == 3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//x o
					//o
					//o
					if(col < maxCol-1 && row >1)
					{
						square = GetSquare(row,col+1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row-1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row-2,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count == 3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//	o
					//  o
					//o x
					if(col > 0 && row < maxRow -2)
					{
						square = GetSquare(row,col-1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row+1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row+2,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count == 3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//o x
					//  o
					//  o
					if(col > 0 && row > 1)
					{
						square = GetSquare(row,col-1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row-1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row-2,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count == 3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//o-x-o-o
					if(col < maxCol-2 && col > 0)
					{
						square = GetSquare(row,col-1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col+1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col+2);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count>=3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//o-o-x-o
					if(col < maxCol-1 && col > 1)
					{
						square = GetSquare(row,col+1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col-1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row,col-2);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count>=3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					//o
					//x
					//o
					//o
					if(row < maxRow - 1 && row > 1)
					{
						square = GetSquare(row+1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row-1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row-2,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count>=3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					
					//o
					//o
					//x
					//o
					if(row < maxRow - 2 && row > 0)
					{
						square = GetSquare(row-1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row+1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
						square = GetSquare(row+2,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count>=3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
					//  o
					//o x o
					//  o
					if(row > 0)
					{
						square = GetSquare(row-1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(row<maxRow-1)
					{
						square = GetSquare(row+1,col);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(col>0)
					{
						square = GetSquare(row,col-1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(col<maxCol-1)
					{
						square = GetSquare(row,col+1);
						if(square.CanMoveItemOut() && square.item.type == (ItemType)itemType)
							nextMoveItems.Add(square.item);
					}
					if(nextMoveItems.Count>=3)
						return nextMoveItems;
					else
						nextMoveItems.Clear();
				}
			}
			
		}
		return nextMoveItems;
	}
	public void RemoveMap()
	{
		//Remove all squares
		for(int i=0; i<squares.Count; i++)
		{
			squares[i].RemoveAllObjects();
		}
		squares.Clear();
	}
}
