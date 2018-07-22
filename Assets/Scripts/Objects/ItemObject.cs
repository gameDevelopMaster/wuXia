using UnityEngine;
using System.Collections;

public class ItemObject : MonoBehaviour 
{
	//Item script, this variable will be used to controll logic part of item 
	public Item itemScript;
	public Animation anim;
	// Use this for initialization
	void Start () {
		anim = GetComponentInChildren<Animation>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnPress()
	{
		//Set selected item to this item script
		if(GamePlay.Instance.selectedItem == null && itemScript.staySquare.CanMoveItemOut())
		{
			GamePlay.Instance.selectedItem = itemScript;
		}
	}
}
