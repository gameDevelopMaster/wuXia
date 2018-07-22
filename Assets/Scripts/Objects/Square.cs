using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Our map is one grid, so square 
public enum SquareType
{
	empty = 0,
	normal= 1,
	block,
	block_stone,
	block_glass,
}
public class Square
{
	public SquareObject squareObject 		= null;
	public static int SQUARE_WIDTH 			= 70;
	public SquareType type					= SquareType.normal; 
	public List<GameObject> blockObjects 	= new List<GameObject>();
	public Item item 						= null;
	public int row  						= 0;
	public int col							= 0;
	private GameObject squareBackground		= null;
	private Transform parent;
	//Init square by row, col and square type
	public Square(int pRow, int pCol, SquareType pType)
	{
		row = pRow;
		col = pCol;
		type = pType;
		blockObjects = new List<GameObject>();
	}
	//Set parent for this square
	public void SetParent(Transform tf)
	{
		parent = tf;
	}
	//Remove all object in this square
	public void RemoveAllObjects()
	{
		//Remove background
		if(squareBackground)
			GameObject.Destroy(squareBackground);
		//Remove blocks if have
		for(int i=0; i< blockObjects.Count; i++)
		{
			if(blockObjects[i])
				GameObject.Destroy(blockObjects[i]);
		}
		blockObjects.Clear();
		//Remove item if have
		if(item != null)
		{
			item.DestroyGameObject();
			item = null;
		}
	}
	//Load all game objects in this square
	public void LoadObjects()
	{
		//Load background
		GameObject obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Squares/square_background"));
		obj.transform.parent = parent;
		obj.transform.localScale = new Vector3(68,68,1);
		//Set square object
		squareObject = obj.GetComponent<SquareObject>();
		squareObject.squareScript = this;
		AddBackground(obj);
		
		switch(type)
		{
			case SquareType.block:
			{
				//gen block object
				obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Squares/brick"));
				obj.transform.parent = parent;
				obj.transform.localScale = new Vector3(64,64,1);
				AddObject(obj);
				break;
			}
			case SquareType.block_stone:
			{
				//gen block object
				obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Squares/brick"));
				obj.transform.parent = parent;
				obj.transform.localScale = new Vector3(64,64,1);
				AddObject(obj);
				//gen stone object in front of
				obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Squares/brick_stone"));
				obj.transform.parent = parent;
				obj.transform.localScale = new Vector3(64,64,1);
				AddObject(obj);
				break;
			}
			case SquareType.block_glass:
			{
				//gen block object
				obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Squares/brick"));
				obj.transform.parent = parent;
				obj.transform.localScale = new Vector3(64,64,1);
				AddObject(obj);
				//gen glass object in front of
				obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Squares/brick_glass"));
				obj.transform.parent = parent;
				obj.transform.localScale = new Vector3(64,64,1);
				AddObject(obj);
				break;
			}
		}
	}
	/// <summary>
	/// Adds the item.
	/// </summary>
	/// <param name='pItem'>
	/// P item.
	/// </param>
	public void AddItem(Item pItem)
	{
		item = pItem;
	}
	
	/// <summary>
	/// Adds the background.
	/// </summary>
	/// <param name='obj'>
	/// Object.
	/// </param>
	public void AddBackground(GameObject obj)
	{
		squareBackground = obj;
		obj.transform.localPosition = new Vector3(PositionX,PositionY,-3);
	}
	
	/// <summary>
	/// Adds block object into this square.
	/// </summary>
	/// <param name='obj'>
	/// Object.
	/// </param>
	public void AddObject(GameObject obj)
	{
		//If this is first layer block, set it behind,
		if(blockObjects.Count == 0)
		{
			obj.transform.localPosition = new Vector3(PositionX,PositionY,-2);
		}
		else
		{
			obj.transform.localPosition = new Vector3(PositionX,PositionY,1);
		}
		blockObjects.Add(obj);
	}
	public void RemoveBlock()
	{
		int topLayer = blockObjects.Count-1;
		if(topLayer >=0)
		{
			//Animation
			blockObjects[topLayer].GetComponent<Animation>().Play("BrickRotate");
			blockObjects[topLayer].GetComponent<UISprite>().depth = 4;
			//Make it drop and rotate random
			blockObjects[topLayer].GetComponent<Rigidbody>().useGravity = true;
			blockObjects[topLayer].GetComponent<Rigidbody>().AddRelativeForce(Random.insideUnitCircle.x*Random.Range(30,40),Random.Range(100,150),0);
			//Destroy and remove from list
			GameObject.Destroy(blockObjects[topLayer],1.5f);
			blockObjects.RemoveAt(topLayer);
			
			//Reduce square style
			if(type == SquareType.block)
			{
				type = SquareType.normal;
				//Calculate score
				MissionManager.Instance.AddScore(1,MissionType.Block);
			}
			else if(type == SquareType.block_glass)
			{
				type = SquareType.block;
			}
			else if(type == SquareType.block_stone)
			{
				type = SquareType.block;
			}
		}
	}
	/// <summary>
	/// Removes item in square.
	/// </summary>
	public void RemoveItem()
	{
		RemoveBlock();
		item = null;
	}
	/// <summary>
	/// Gets the neighbors of this square.
	/// </summary>
	/// <returns>
	/// The neighbors.
	/// </returns>
	public List<Square> GetNeighbors()
	{
		List<Square> squares = new List<Square>();
		if(row < Map.maxRow-1)
		{
			squares.Add(Map.Instance.GetSquare(row+1,col));
		}
		if(row > 0)
			squares.Add(Map.Instance.GetSquare(row-1,col));
		if(col > 0)
		{
			squares.Add(Map.Instance.GetSquare(row,col-1));
		}
		if(col < Map.maxCol-1)
		{
			squares.Add(Map.Instance.GetSquare(row,col+1));
		}
		return squares;
	}
	/// <summary>
	/// Determines whether this instance can move item in.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance can move item in; otherwise, <c>false</c>.
	/// </returns>
	public bool CanMoveItemIn()
	{
		//If there is item, cannot move other item in
		if(item != null)
			return false;
		//If this square is empty, cannot move item in
		if(type == SquareType.empty)
			return false;
		//If this square is stone, cannot move item in
		if(type == SquareType.block_stone)
			return false;
		return true;
	}
	/// <summary>
	/// Determines whether this instance can move item out.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance can move item out; otherwise, <c>false</c>.
	/// </returns>
	public bool CanMoveItemOut()
	{
		//if there isn't item, cannot move item out
		if(item == null)
			return false;
		//If square is empty, cannot move item out
		if(type == SquareType.empty)
			return false;
		//If square is stone, cannot move item out
		if(type == SquareType.block_stone)
			return false;
		//If square is glass, user cannot pick item out
		if(type == SquareType.block_glass)
			return false;
		return true;
	}
	/// <summary>
	/// Determines whether this instance can move item through.
	/// </summary>
	/// <returns>
	/// <c>true</c> if this instance can move item through; otherwise, <c>false</c>.
	/// </returns>
	public bool CanMoveItemThrough()
	{
		//If square is stone, cannot move throught
		if(type == SquareType.block_stone)
			return false;
		return true;
	}
	#region "POSITION FUNCTIONS"
	public int PositionX
	{
		get
		{
			return Map.MAP_START_X + col * SQUARE_WIDTH;
		}
	}
	public int PositionY
	{
		get
		{
			return Map.MAP_START_Y + row * SQUARE_WIDTH;
		}
	}
	public int PositionZ
	{
		get
		{
			return -1;
		}
	}
	public Vector3 Position
	{
		get
		{
			return new Vector3(PositionX, PositionY, PositionZ);
		}
	}
	#endregion
}
