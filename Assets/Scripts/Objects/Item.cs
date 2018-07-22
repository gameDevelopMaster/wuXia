using UnityEngine;
using System.Collections;

//Item type, in our case is candy type, you can add more type in here
public enum ItemType
{
	green 	= 0, //first item always must be 0
	violet, //other items not need to define value
	red,
	pink,
	blue
}
public enum ItemSubType
{
	normal,
	special
}
//Item class
public class Item
{
	//Item Object script of this item, item object will be used to manager all
	//things relate to item's game object 
	public ItemObject 	itemObject;
	//Our map is square grid, and each item stay in one square
	public Square 		staySquare;
	//targetSquare is square this item will move to
	public Square		targetSquare;
	//Type of this item
	public ItemType 	type;
	
	public int 			score = 10; //score to get this item
	//Image of this item
	UIImageButton imageButton;
	string defaultImage;
	
	/// <summary>
	/// Non-empty item constructor <see cref="Item"/> class.
	/// </summary>
	/// <param name='pItemObject'>
	/// P item object.
	/// </param>
	/// <param name='itemType'>
	/// Item type.
	/// </param>
	public Item(ItemType itemType)
	{
		//set type
		type = itemType;
		//load game object by type
		LoadGameObject(itemType);
		//set item script for itemObject
		itemObject.itemScript = this;
		//get image of this item
		imageButton = itemObject.gameObject.GetComponent<UIImageButton>();
		defaultImage = imageButton.normalSprite;
	}
	public virtual void LoadGameObject(ItemType itemType)
	{
		int index = (int)itemType + 1;
		GameObject obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Items/Item" + index));
		obj.name = "Item" + index;
		obj.transform.parent = GameObject.Find("Panel").transform;
		obj.transform.localScale = new Vector3(0.9f,0.9f,0.9f);
		
		//Get itemObject component and create new item
		itemObject = obj.GetComponent<ItemObject>();
	}
	public void SetPosition(Vector3 localPos)
	{
		itemObject.gameObject.transform.localPosition = localPos;
	}
	/// <summary>
	/// Sets the stay square for this item.
	/// </summary>
	/// <param name='square'>
	/// Square.
	/// </param>
	public void SetStaySquare(Square square)
	{
		//set stay square
		staySquare = square;
		square.item = this;
		//change position of this item to square position
		//need to use localPosition because all items are chilren of NGUI objects in scenes
		itemObject.gameObject.transform.localPosition = square.Position + new Vector3(0,0,-2f);
	}
	public void CheckMove(Square toSquare)
	{
		
	}
	/// <summary>
	/// Move item into square
	/// </summary>
	/// <param name='square'>
	/// Square.
	/// </param>
	public void MoveIn(Square square)
	{
		GamePlay.Instance.StartCoroutine(MoveIn(square,0.5f));
	}
	public void DropIn(Square square)
	{
		GamePlay.Instance.StartCoroutine(DropIn(square,0.5f));
	}
	public IEnumerator DropIn(Square square, float time)
	{
		//Play sound effect
		SoundEffect.Instance.PlaySoundRandom(SoundEffect.Drop);
		
		//Move game object by iTween, need to scale square position to localPosition by mutilply with
		//gameobject local scale(0.002083333f)
		//because iTween move object using world position
		iTween.MoveTo(itemObject.gameObject, (square.Position + new Vector3(0,0,-2f)) * 0.002083333f, time);
		//assign this item to the square
		square.item = this;
		
		//assign stay square for this item
		staySquare = square;
		yield return new WaitForSeconds(time/2f);
		if(itemObject.anim != null)
			itemObject.anim.Play("ItemDrop");
		yield return new WaitForSeconds(0.5f);
		if(itemObject.anim != null)
			itemObject.anim.Play("ItemIdle");
	}
	public IEnumerator MoveIn(Square square, float time)
	{
		//Play sound effect
		SoundEffect.Instance.PlaySoundRandom(SoundEffect.Drop);
		
		//Move game object by iTween, need to scale square position to localPosition by mutilply with
		//gameobject local scale(0.002083333f)
		//because iTween move object using world position
		iTween.MoveTo(itemObject.gameObject, (square.Position + new Vector3(0,0,-2f))  * 0.002083333f, time);
		//assign this item to the square
		square.item = this;
		//assign stay square for this item
		staySquare = square;
		yield return new WaitForSeconds(time);
	}
	/// <summary>
	/// Move item with direction to change position of items when
	/// </summary>
	/// <param name='direction'>
	/// Direction.
	/// </param>
	public void Swap(Vector2 direction)
	{
		if(direction.magnitude == 0)
			return;
		//Find neighbor item
		int moveToRow = staySquare.row + (int)direction.y;
		int moveToCol = staySquare.col + (int)direction.x;
		//Cannot find out of map
		if(moveToRow < 0 || moveToRow >= Map.maxRow)
			return;
		if(moveToCol < 0 || moveToCol >= Map.maxCol)
			return;
		Square neighborSquare = Map.Instance.GetSquare(moveToRow,moveToCol);
		//if neighbor square is empty, cannot swawp
		if(!neighborSquare.CanMoveItemOut())
			return;
		//Do the swap with neirghbor item
		GamePlay.Instance.StartCoroutine(OnSwap(neighborSquare.item));
	}
	IEnumerator OnSwap(Item neighbor)
	{
		//Set game to "is moving" state to block other input
		GamePlay.Instance.BlockInput(true);
		//target square will be neighbor item's square
		targetSquare = neighbor.staySquare;
		//Move neighbor item to this item square
		neighbor.MoveIn(staySquare);
		//Move this item to neighbor item square in 0.5s
		MoveIn(targetSquare);
		yield return new WaitForSeconds(0.5f);
		//After swap, if cannot found any combination
		//swap items back
		if(!Map.Instance.CheckCombine())
		{
			targetSquare = neighbor.staySquare;
			neighbor.MoveIn(staySquare);
			MoveIn(targetSquare);
			//yield return new WaitForSeconds(0.5f);
			//Find next move
		}
		else
		{
			LevelData.limitAmount--;
		}
	}
	
	/// <summary>
	/// Show this item in next move items
	/// </summary>
	public void Hint()
	{
		//Set image of this item to "pressedSprite"(you can see in game object "Item1" in hierchary)
		if(imageButton != null)
		{
			//Play animation for hint
			itemObject.anim.Play("ItemDrop");
			//imageButton.SetNormalSprite(imageButton.hoverSprite);
		}
	}
	/// <summary>
	/// Remove hint effect
	/// </summary>
	public void UnHint()
	{
		if(imageButton != null)
		{
			//Stop animation
			itemObject.anim.Play("ItemIdle");
			//imageButton.SetNormalSprite(defaultImage);
		}
	}
	/// <summary>
	/// Destroy this instance.
	/// </summary>
	public virtual void Destroy()
	{
		//because one item can add into more than one combine,
		//so we need to check if this item already destroyed, 
		//dont destroy it any more
		if(staySquare == null)
			return;
		//show effect
		GameObject effect = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Sparkle"));
		effect.transform.parent = itemObject.transform.parent;
		effect.transform.localPosition = itemObject.transform.localPosition;
		GameObject.Destroy(effect,0.5f);
		//show score
		GameObject scoreObject = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Score"));
		scoreObject.transform.parent = itemObject.transform.parent;
		//make sure that score object appear in front of item
		scoreObject.transform.localPosition = itemObject.transform.localPosition + new Vector3(0,0,-1); 
		scoreObject.transform.localScale = new Vector3(40,30,0);
		scoreObject.GetComponent<UILabel>().text = "+" + score;
		iTween.MoveBy(scoreObject,new Vector3(0,60,0)* 0.002083333f, 0.5f);
		//iTween.FadeTo(scoreObject,0,0.5f);
		GameObject.Destroy(scoreObject,0.5f);
		//Add score
		MissionManager.Instance.AddScore(score,MissionType.Score);
		//reduce level from near squares
		foreach(Square square in staySquare.GetNeighbors())
		{
			if(square.type > SquareType.block)
			{
				square.RemoveBlock();
			}
		}
		//remove item from square
		staySquare.RemoveItem();
		staySquare = null;
		DestroyGameObject();
	}
	public void DestroyGameObject()
	{
		//destroy game object
		if(itemObject.gameObject)
		{
			GameObject.Destroy(itemObject.gameObject);
		}
	}
	public virtual void Replace(Item item)
	{
		if(item != null && item.itemObject != null)
		{
			iTween.RotateAdd(item.itemObject.gameObject,new Vector3(0,0,10),1.5f);
			iTween.ScaleTo(item.itemObject.gameObject,Vector3.zero,1.5f);
			GameObject.Destroy(item.itemObject.gameObject,1.5f);
			SetStaySquare(item.staySquare);
		}
		itemObject.gameObject.transform.localScale = Vector3.zero;
		iTween.ScaleTo(itemObject.gameObject,new Vector3(0.9f,0.9f,0.9f),1.5f);
		item = null;
	}
	
}
